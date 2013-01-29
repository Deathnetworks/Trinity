﻿using GilesTrinity.Cache;
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
            catch { }
            // Update actors if possible (if already in-game)
            if (ZetaDia.IsInGame && !ZetaDia.IsLoadingWorld && ZetaDia.Actors != null)
            {
                ZetaDia.Actors.Update();
                UpdateSearchGridProvider(true);
            }
            HasMappedPlayerAbilities = false;
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

            PlayerMover.TimeLastRecordedPosition = DateTime.Now;
            PlayerMover.timeLastRestartedGame = DateTime.Now;
            PlayerMover.ResetCheckGold();

            if (Zeta.CommonBot.Settings.CharacterSettings.Instance.KillRadius < 20)
            {
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "WARNING: Low Kill Radius detected, currently set to: {0} (you can change this through Demonbuddy bot settings)",
                    Zeta.CommonBot.Settings.CharacterSettings.Instance.KillRadius);
            }

            if (Zeta.CommonBot.Settings.CharacterSettings.Instance.LootRadius < 50)
            {
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "WARNING: Low Gold Loot Radius detected, currently set to: {0} (you can change this through Demonbuddy bot settings)",
                    Zeta.CommonBot.Settings.CharacterSettings.Instance.LootRadius);
            }

            StashRule.readConfiguration();

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
            PlayerMover.iTotalAntiStuckAttempts = 1;
            PlayerMover.vSafeMovementLocation = Vector3.Zero;
            PlayerMover.vOldPosition = Vector3.Zero;
            PlayerMover.iTimesReachedStuckPoint = 0;
            PlayerMover.TimeLastRecordedPosition = DateTime.Today;
            PlayerMover.LastGeneratedStuckPosition = DateTime.Today;
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
                    hook.Value[0] = new Zeta.TreeSharp.Decorator(ctx => CheckHasTarget(ctx), HandleTargetAction());
                }

                // Vendor run hook
                // Wipe the vendorrun and loot behavior trees, since we no longer want them
                if (hook.Key.Contains("VendorRun"))
                {
                    Decorator VendorRunDecorator = hook.Value[0] as Decorator;
                    PrioritySelector VendorRunPrioritySelector = VendorRunDecorator.Children[0] as PrioritySelector;

                    //VendorRunPrioritySelector.Children[3] = TownRun.Decorators.GetPreStashDecorator();
                    //VendorRunPrioritySelector.Children[4] = TownRun.Decorators.GetStashDecorator();
                    //VendorRunPrioritySelector.Children[5] = TownRun.Decorators.GetSellDecorator();
                    //VendorRunPrioritySelector.Children[6] = TownRun.Decorators.GetSalvageDecorator();

                    VendorRunPrioritySelector.Children[3] = TownRun.Decorators.GetPreStashDecorator();
                    VendorRunPrioritySelector.Children[4] = TownRun.Decorators.GetSalvageDecorator();
                    VendorRunPrioritySelector.Children[5] = TownRun.Decorators.GetSellDecorator();
                    VendorRunPrioritySelector.Children[6] = TownRun.Decorators.GetStashDecorator();

                    hook.Value[0] = new Decorator(ret => TownRun.TownRunCanRun(ret), VendorRunPrioritySelector);

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
