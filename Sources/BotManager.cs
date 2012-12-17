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
                DbHelper.Log(TrinityLogLevel.Error, LogCategory.UserInformation, "WARNING: Giles Trinity is NOT YET ENABLED. Bot start detected");
                DbHelper.Log(TrinityLogLevel.Error, LogCategory.UserInformation, "Ignore this message if you are not currently using Giles Trinity.");
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
				weaponSwap.Reset();
            }

            RefreshProfileBlacklists();

            ReplaceTreeHooks();

            GilesPlayerMover.timeLastRecordedPosition = DateTime.Now;
            GilesPlayerMover.timeLastRestartedGame = DateTime.Now;

            //try
            //{
            //    CacheManager.Initialize();
            //}
            //catch (Exception ex)
            //{
            //    DbHelper.Log(TrinityLogLevel.Debug, LogCategory.GlobalHandler, "Error Initializing CacheManager");
            //    DbHelper.Log(TrinityLogLevel.Debug, LogCategory.GlobalHandler, "{0}\n{1}", ex.Message, ex.StackTrace);
            //}
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
                DbHelper.Log(TrinityLogLevel.Debug, LogCategory.GlobalHandler, "Error Destroying CacheManager");
                DbHelper.Log(TrinityLogLevel.Debug, LogCategory.GlobalHandler, "{0}\n{1}", ex.Message, ex.StackTrace);
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

                    VendorRunPrioritySelector.Children[3] = TownRun.Decorators.GetPreStashDecorator();
                    VendorRunPrioritySelector.Children[4] = TownRun.Decorators.GetStashDecorator();
                    VendorRunPrioritySelector.Children[5] = TownRun.Decorators.GetSellDecorator();
                    VendorRunPrioritySelector.Children[6] = TownRun.Decorators.GetSalvageDecorator();

                    hook.Value[0] = new Decorator(ret => TownRun.GilesTownRunCheckOverlord(ret), VendorRunPrioritySelector);

                }

                if (hook.Key.Contains("Loot"))
                {
                    // Replace the loot behavior tree with a blank one, as we no longer need it
                    hook.Value[0] = new Decorator(ret => false, new Action());
                }

            }
        }
    }
}
