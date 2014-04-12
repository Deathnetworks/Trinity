using System;
using System.Threading;
using System.Windows;
using Trinity.Technicals;
using Zeta.Bot;
using Zeta.Bot.Navigation;
using Zeta.Common;
using Zeta.Common.Plugins;
using Zeta.Game;
using Zeta.TreeSharp;
using Logger = Trinity.Technicals.Logger;

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
            PrioritySelector VendorRunPrioritySelector =
                (TreeHooks.Instance.Hooks["VendorRun"][0] as Decorator).Children[0] as PrioritySelector;
            TreeHooks.Instance.ReplaceHook("VendorRun",
                new Decorator(ret => TownRun.TownRunCanRun(ret), TownRun.TownRunWrapper(VendorRunPrioritySelector)));

            // Loot tree is now empty and never runs (Loot is handled through combat)
            //TreeHooks.Instance.ReplaceHook("Loot", new Decorator(ret => false, new Action()));

            // Death Handling
            //TreeHooks.Instance.ReplaceHook("Death",
            //    DbProvider.DeathHandler.CreateDeathHandler(TreeHooks.Instance.Hooks["Death"][0]));
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
                //BotMain.TicksPerSecond = Int32.MaxValue;
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
                Navigator.StuckHandler = new Zeta.Bot.Navigation.DefaultStuckHandler();
                Logger.Log(TrinityLogLevel.Verbose, LogCategory.UserInformation, "Using Default Demonbuddy Unstucker", true);
            }
        }

        internal static void DOU_p(bool t)
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
