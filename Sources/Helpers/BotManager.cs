using Trinity.Cache;
using System;
using System.Collections.Generic;
using Trinity.DbProvider;
using Trinity.Technicals;
using Zeta;
using Zeta.Common;
using Zeta.Common.Plugins;
using Zeta.CommonBot;
using Zeta.TreeSharp;
using Action = Zeta.TreeSharp.Action;
using Zeta.Navigation;
using System.Threading;
using System.Windows;

namespace Trinity
{
    public partial class Trinity : IPlugin
    {
        /// <summary>
        /// This will replace the main BehaviorTree hooks for Combat, Vendoring, and Looting.
        /// </summary>
        private static void ReplaceTreeHooks()
        {
            // This is the do-all-be-all god-head all encompasing piece of trinity
            TreeHooks.Instance.ReplaceHook("Combat", new Decorator(ctx => TargetCheck(ctx), HandleTargetAction()));

            // We still want the main VendorRun logic, we're just going to take control of *when* this logic kicks in
            PrioritySelector VendorRunPrioritySelector = (TreeHooks.Instance.Hooks["VendorRun"][0] as Decorator).Children[0] as PrioritySelector;
            TreeHooks.Instance.ReplaceHook("VendorRun", new Decorator(ret => TownRun.TownRunCanRun(ret), VendorRunPrioritySelector));

            // Loot tree is now empty and never runs (Loot is handled through combat)
            TreeHooks.Instance.ReplaceHook("Loot", new Decorator(ret => false, new Action()));
            
        }

        


        internal static void SetBotTPS()
        {
            // Carguy's ticks-per-second feature
            if (Settings.Advanced.TPSEnabled)
            {
                BotMain.TicksPerSecond = (int)Settings.Advanced.TPSLimit;
                Logger.Log(TrinityLogLevel.Verbose, LogCategory.UserInformation, "Bot TPS set to {0}", (int)Settings.Advanced.TPSLimit);
            }
            else
            {
                BotMain.TicksPerSecond = 10;
                Logger.Log(TrinityLogLevel.Verbose, LogCategory.UserInformation, "Reset bot TPS to default", (int)Settings.Advanced.TPSLimit);
            }
        }

        internal static void SetItemManagerProvider()
        {
            if (Settings.Loot.ItemFilterMode != global::Trinity.Config.Loot.ItemFilterMode.DemonBuddy)
            {
                ItemManager.Current = new TrinityItemManager();
            }
            else
            {
                ItemManager.Current = new LootRuleItemManager();
            }
        }

        internal static void SetUnstuckProvider()
        {
            if (Settings.Advanced.UnstuckerEnabled)
            {
                Navigator.StuckHandler = new StuckHandler();
                Logger.Log(TrinityLogLevel.Verbose, LogCategory.UserInformation, "Using Trinity Unstucker", true);
            }
            else
            {
                Navigator.StuckHandler = new Zeta.Navigation.DefaultStuckHandler();
                Logger.Log(TrinityLogLevel.Verbose, LogCategory.UserInformation, "Using Default Demonbuddy Unstucker", true);
            }
        }

        internal static void E1(bool t)
        {
            if (!t)
                Trinity.Exit();
        }

        internal static void Exit()
        {
            ZetaDia.Memory.Process.Kill();

            try
            {
                if (Thread.CurrentThread != Application.Current.Dispatcher.Thread)
                {
                    Application.Current.Dispatcher.Invoke(new System.Action(Exit));
                    return;
                }

                Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString());
            }
        }

    }
}
