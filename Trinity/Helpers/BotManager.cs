using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Trinity.Config;
using Trinity.DbProvider;
using Trinity.Items;
using Trinity.Settings.Loot;
using Trinity.Technicals;
using Zeta.Bot;
using Zeta.Bot.Coroutines;
using Zeta.Bot.Navigation;
using Zeta.Common;
using Zeta.Game;
using Zeta.TreeSharp;
using Action = Zeta.TreeSharp.Action;
using Logger = Trinity.Technicals.Logger;

namespace Trinity.Helpers
{
    public class BotManager
    {
        private static TrinitySetting Settings { get { return Trinity.Settings; } }

        private static readonly Dictionary<string, Composite> OriginalHooks = new Dictionary<string, Composite>();

        private static Composite _goldInactiveComposite;
        private static Composite _xpInactiveComposite;

        /// <summary>
        /// This will replace the main BehaviorTree hooks for Combat, Vendoring, and Looting.
        /// </summary>
        internal static void ReplaceTreeHooks()
        {
            if (Trinity.IsPluginEnabled)
            {
                ReplaceCombatHook();
                ReplaceVendorRunHook();
                ReplaceLootHook();
                InsertOutOfGameHooks();
            }
            else
            {
                ReplaceHookWithOriginal("Combat");
                ReplaceHookWithOriginal("VendorRun");
                ReplaceHookWithOriginal("Loot");

                Logger.Log("Removing GoldInactivity from BotBehavior");
                TreeHooks.Instance.RemoveHook("BotBehavior", _goldInactiveComposite);
            }
        }

        private static void ReplaceCombatHook()
        {
            if (!TreeHooks.Instance.Hooks.ContainsKey("Combat"))
                return;
            // This is the do-all-be-all god-head all encompasing piece of trinity
            StoreAndReplaceHook("Combat", new ActionRunCoroutine(ret => MainCombatTask()));
        }

        private static async Task<bool> MainCombatTask()
        {
            // If we aren't in the game or a world is loading, don't do anything yet
            if (!ZetaDia.IsInGame || ZetaDia.IsLoadingWorld || !ZetaDia.Me.IsValid)
            {
                Logger.LogDebug(LogCategory.GlobalHandler, "Not in game, IsLoadingWorld, or Me is Invalid");
                return true;
            }

            try
            {
                // We keep dying because we're spawning in AoE and next to 50 elites and we need to just leave the game
                if (DateTime.UtcNow.Subtract(Trinity.LastDeathTime).TotalSeconds < 30 &&
                    ZetaDia.Me.Inventory.Equipped.Any() &&
                    ZetaDia.Me.Inventory.Equipped.Where(i => i.ACDGuid != 0 && i.IsValid).Average(i => i.DurabilityPercent) < 0.05 && !ZetaDia.IsInTown)
                {
                    Logger.Log("Durability is zero, emergency leave game");
                    ZetaDia.Service.Party.LeaveGame(true);
                    await CommonCoroutines.LeaveGame("Durability is zero");
                    Logger.LogDebug(LogCategory.GlobalHandler, "Recently died, durability zero");
                    return true;
                }   
            }
            catch { }
            return await new Decorator(Trinity.TargetCheck, new Action(ret => Trinity.HandleTarget())).ExecuteCoroutine();
        }

        private static void ReplaceVendorRunHook()
        {
            if (!TreeHooks.Instance.Hooks.ContainsKey("VendorRun"))
                return;
            // We still want the main VendorRun logic, we're just going to take control of *when* this logic kicks in
            var vendorDecorator = TreeHooks.Instance.Hooks["VendorRun"][0] as Decorator;
            if (vendorDecorator != null)
            {
                StoreAndReplaceHook("VendorRun", new Decorator(TownRun.TownRunCanRun, new ActionRunCoroutine(ret => TownRun.TownRunCoroutineWrapper(vendorDecorator))));
            }
        }

        private static void ReplaceLootHook()
        {
            if (!TreeHooks.Instance.Hooks.ContainsKey("Loot"))
                return;
            // Loot tree is now empty and never runs (Loot is handled through combat)
            // This is for special out of combat handling like Horadric Cache
            Composite lootComposite = TreeHooks.Instance.Hooks["Loot"][0];
            StoreAndReplaceHook("Loot", Composites.CreateLootBehavior(lootComposite));
        }

        private static void InsertOutOfGameHooks()
        {
            const string hookName = "TreeStart";

            if (_goldInactiveComposite == null)
                _goldInactiveComposite = GoldInactivity.CreateGoldInactiveLeaveGame();

            if (_xpInactiveComposite == null)
                _xpInactiveComposite = XpInactivity.CreateXpInactiveLeaveGame();

            Logger.Log("Inserting GoldInactivity into " + hookName);
            TreeHooks.Instance.InsertHook(hookName, 0, _goldInactiveComposite);

            Logger.Log("Inserting XPInactivity into " + hookName);
            TreeHooks.Instance.InsertHook(hookName, 0, _xpInactiveComposite);
        }

        internal static void InstanceOnOnHooksCleared(object sender, EventArgs eventArgs)
        {
            ReplaceTreeHooks();
        }

        private static void StoreAndReplaceHook(string hookName, Composite behavior)
        {
            if (!OriginalHooks.ContainsKey(hookName))
                OriginalHooks.Add(hookName, TreeHooks.Instance.Hooks[hookName][0]);

            Logger.Log("Replacing " + hookName + " Hook");
            TreeHooks.Instance.ReplaceHook(hookName, behavior);
        }

        private static void ReplaceHookWithOriginal(string hook)
        {
            if (OriginalHooks.ContainsKey(hook))
            {
                Logger.Log("Replacing " + hook + " Hook with Original");
                TreeHooks.Instance.ReplaceHook(hook, OriginalHooks[hook]);
            }
        }


        internal static void SetBotTicksPerSecond()
        {
            // Carguy's ticks-per-second feature
            if (Settings.Advanced.TPSEnabled)
            {
                BotMain.TicksPerSecond = Settings.Advanced.TPSLimit;
                Logger.Log(TrinityLogLevel.Verbose, LogCategory.UserInformation, "Bot TPS set to {0}", Settings.Advanced.TPSLimit);
            }
            else
            {
                BotMain.TicksPerSecond = 10;
                //BotMain.TicksPerSecond = Int32.MaxValue;
                Logger.Log(TrinityLogLevel.Verbose, LogCategory.UserInformation, "Reset bot TPS to default", Settings.Advanced.TPSLimit);
            }
        }

        internal static void SetItemManagerProvider()
        {
            if (Settings.Loot.ItemFilterMode != ItemFilterMode.DemonBuddy)
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
                Navigator.StuckHandler = new DefaultStuckHandler();
                Logger.Log(TrinityLogLevel.Verbose, LogCategory.UserInformation, "Using Default Demonbuddy Unstucker", true);
            }
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
