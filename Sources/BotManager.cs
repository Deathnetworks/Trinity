using GilesTrinity.Cache;
using System;
using System.Collections.Generic;
using GilesTrinity.DbProvider;
using GilesTrinity.Technicals;
using Zeta;
using Zeta.Common;
using Zeta.Common.Plugins;
using Zeta.CommonBot;
using Zeta.TreeSharp;
using Action = Zeta.TreeSharp.Action;

namespace GilesTrinity
{
    public partial class GilesTrinity : IPlugin
    {
        /// <summary>
        /// This is wired up by Plugin.OnEnabled, and called when the bot is started
        /// </summary>
        /// <param name="bot"></param>
        private static void TrinityBotStart(IBot bot)
        {
            if (!bPluginEnabled && bot != null)
            {
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "WARNING: Giles Trinity is NOT YET ENABLED. Bot start detected");
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "Ignore this message if you are not currently using Giles Trinity.");
                return;
            }
            // Recording of all the XML's in use this run
            try
            {
                string sThisProfile = Zeta.CommonBot.Settings.GlobalSettings.Instance.LastProfile;
                if (sThisProfile != sLastProfileSeen)
                {
                    listProfilesLoaded.Add(sThisProfile);
                    sLastProfileSeen = sThisProfile;
                    if (sFirstProfileSeen == "")
                        sFirstProfileSeen = sThisProfile;
                }
            }
            catch
            {
            }
            // Update actors if possible (if already in-game)
            if (ZetaDia.IsInGame && !ZetaDia.IsLoadingWorld && ZetaDia.Actors != null)
            {
                ZetaDia.Actors.Update();
            }
            bMappedPlayerAbilities = false;
            if (!bMaintainStatTracking)
            {
                ItemStatsWhenStartedBot = DateTime.Now;
                ItemStatsLastPostedReport = DateTime.Now;
                bMaintainStatTracking = true;
            }
            else
            {
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "Note: Maintaining item stats from previous run. To reset stats fully, please restart DB.");
            }

            RefreshProfileBlacklists();

            ReplaceTreeHooks();

            try
            {
                CacheManager.Initialize();
            }
            catch (Exception ex)
            {
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.GlobalHandler, "Error Initializing CacheManager");
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.GlobalHandler, "{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }

        // When the bot stops, output a final item-stats report so it is as up-to-date as can be
        private void TrinityBotStop(IBot bot)
        {
            // Issue final reports
            OutputReport();
            vBacktrackList = new SortedList<int, Vector3>();
            iTotalBacktracks = 0;
            GilesPlayerMover.iTotalAntiStuckAttempts = 1;
            GilesPlayerMover.vSafeMovementLocation = Vector3.Zero;
            GilesPlayerMover.vOldPosition = Vector3.Zero;
            GilesPlayerMover.iTimesReachedStuckPoint = 0;
            GilesPlayerMover.timeLastRecordedPosition = DateTime.Today;
            GilesPlayerMover.timeStartedUnstuckMeasure = DateTime.Today;
            hashUseOnceID = new HashSet<int>();
            dictUseOnceID = new Dictionary<int, int>();
            dictRandomID = new Dictionary<int, int>();
            iMaxDeathsAllowed = 0;
            iDeathsThisRun = 0;
            try
            {
                CacheManager.Destroy();
            }
            catch (Exception ex)
            {
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.GlobalHandler, "Error Destroying CacheManager");
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.GlobalHandler, "{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }

        /// <summary>
        /// This will replace the main BehaviorTree hooks for Combat, Vendoring, and Looting.
        /// </summary>
        private static void ReplaceTreeHooks()
        {
            foreach (var hook in TreeHooks.Instance.Hooks)
            {
                // Replace the combat behavior tree, as that happens first and so gets done quicker!
                if (hook.Key.Contains("Combat"))
                {
                    hook.Value[0] = new Zeta.TreeSharp.Decorator(ctx => GilesGlobalOverlord(ctx), HandleTargetAction());
                }

                // Vendor run hook
                // Wipe the vendorrun and loot behavior trees, since we no longer want them
                if (hook.Key.Contains("VendorRun"))
                {   
                    Decorator VendorRunDecorator = hook.Value[0] as Decorator;
                    PrioritySelector VendorRunPrioritySelector = VendorRunDecorator.Children[0] as PrioritySelector;

                    VendorRunPrioritySelector.Children[3] = GetPreStashDecorator();
                    VendorRunPrioritySelector.Children[4] = GetStashDecorator();
                    VendorRunPrioritySelector.Children[5] = GetSellDecorator();
                    VendorRunPrioritySelector.Children[6] = GetSalvageDecorator();

                    hook.Value[0] = new Decorator(ret => GilesTownRunCheckOverlord(ret), VendorRunPrioritySelector);

                }

                if (hook.Key.Contains("Loot"))
                {
                    // Replace the loot behavior tree with a blank one, as we no longer need it
                    hook.Value[0] = new Decorator(ret => GilesBlankDecorator(ret),
                        new Action(ret => GilesBlankAction(ret))
                        );
                }

            }
        }

        private static Decorator GetPreStashDecorator()
        {
            return new Decorator(ctx => GilesPreStashPauseOverlord(ctx),
                new Sequence(
                    new Action(ctx => GilesStashPrePause(ctx)),
                    new Action(ctx => GilesStashPause(ctx))
                )
            );
        }

        private static Decorator GetStashDecorator()
        {
            return new Decorator(ctx => GilesStashOverlord(ctx),
                new Sequence(
                    new Action(ctx => GilesOptimisedPreStash(ctx)),
                    new Action(ctx => GilesOptimisedStash(ctx)),
                    new Action(ctx => GilesOptimisedPostStash(ctx)),
                    new Sequence(
                        new Action(ctx => GilesStashPrePause(ctx)),
                        new Action(ctx => GilesStashPause(ctx))
                    )
                )
            );
        }

        private static Decorator GetSellDecorator()
        {
            return new Decorator(ctx => GilesSellOverlord(ctx),
                new Sequence(
                    new Action(ctx => GilesOptimisedPreSell(ctx)),
                    new Action(ctx => GilesOptimisedSell(ctx)),
                    new Action(ctx => GilesOptimisedPostSell(ctx)),
                    new Sequence(
                        new Action(ctx => GilesStashPrePause(ctx)),
                        new Action(ctx => GilesStashPause(ctx))
                    )
                )
            );
        }

        private static Decorator GetSalvageDecorator()
        {
            return new Decorator(ctx => GilesSalvageOverlord(ctx),
                new Sequence(
                    new Action(ctx => GilesOptimisedPreSalvage(ctx)),
                    new Action(ctx => GilesOptimisedSalvage(ctx)),
                    new Action(ctx => GilesOptimisedPostSalvage(ctx)),
                    new Sequence(
                        new Action(ctx => GilesStashPrePause(ctx)),
                        new Action(ctx => GilesStashPause(ctx))
                    )
                )
            );
        }
    }
}
