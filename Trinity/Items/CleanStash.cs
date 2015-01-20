using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Buddy.Coroutines;
using Trinity.Coroutines;
using Trinity.Helpers;
using Zeta.Bot;
using Zeta.Bot.Coroutines;
using Zeta.Common;
using Zeta.Game;
using Zeta.Game.Internals;
using Zeta.TreeSharp;
using Logger = Trinity.Technicals.Logger;

namespace Trinity.Items
{
    public class CleanStash
    {
        private const int ItemMovementDelay = 100;
        const string HookName = "VendorRun";

        private static Composite _cleanBehavior;
        private static bool _hookInserted;

        public static void RunCleanStash()
        {
            if (!BotMain.IsRunning)
            {
                Logger.LogError("Unable to clean stash while bot is not running");
                return;
            }

            try
            {
                GoldInactivity.Instance.ResetCheckGold();
                XpInactivity.Instance.ResetCheckXp();

                if (!_hookInserted)
                {
                    _cleanBehavior = CreateCleanBehavior();
                    TreeHooks.Instance.InsertHook(HookName, 0, _cleanBehavior);
                    _hookInserted = true;
                    BotMain.OnStop += bot => RemoveBehavior();
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Error running clean stash: " + ex);
                RemoveBehavior();
            }

        }

        private static void RemoveBehavior()
        {
            if (_cleanBehavior != null)
            {
                try
                {
                    if (_hookInserted)
                    {
                        TreeHooks.Instance.RemoveHook(HookName, _cleanBehavior);
                        _hookInserted = false;
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogDebug("Sort behavior not inserted? " + ex);
                }
            }
        }

        public static Composite CreateCleanBehavior()
        {
            return new ActionRunCoroutine(ret => CleanTask());
        }

        private static bool _isFinished = false;
        public static async Task<bool> CleanTask()
        {
            if (!ZetaDia.IsInGame)
                return false;
            if (ZetaDia.IsLoadingWorld)
                return false;
            if (!ZetaDia.Me.IsFullyValid())
                return false;

            if (ZetaDia.Me.IsParticipatingInTieredLootRun)
            {
                Logger.LogNormal("Cannot clean stash while in trial/greater rift");
                RemoveBehavior();
                return false;
            }

            if (TrinityItemManager.FindValidBackpackLocation(true) == new Vector2(-1, -1))
            {
                Trinity.ForceVendorRunASAP = true;
                return false;
            }
            if (!await TrinityCoroutines.ReturnToStashTask())
            {
                _isFinished = true;
                return false;
            }
            if (GameUI.IsElementVisible(GameUI.StashDialogMainPage))
            {
                Logger.Log("Cleaning stash...");

                foreach (var item in ZetaDia.Me.Inventory.StashItems.Where(i => i.ACDGuid > 0 && i.IsValid).ToList())
                {
                    if (!ItemManager.Current.ShouldStashItem(item))
                    {
                        Logger.Log("Removing {0} from stash", item.Name);
                        ZetaDia.Me.Inventory.QuickWithdraw(item);
                        await Coroutine.Sleep(ItemMovementDelay);
                        await Coroutine.Yield();

                        if (TrinityItemManager.FindValidBackpackLocation(true) == new Vector2(-1, -1))
                        {
                            Trinity.ForceVendorRunASAP = true;
                            return false;
                        }
                    }
                }

                Trinity.ForceVendorRunASAP = true;
                _isFinished = true;
                Logger.Log("Waiting 5 seconds...");
                BotMain.StatusText = "Waiting 5 seconds...";
                await Coroutine.Sleep(5000);

                if (TrinityCoroutines.StartedOutOfTown && ZetaDia.IsInTown)
                    await CommonBehaviors.TakeTownPortalBack().ExecuteCoroutine();
            }
            if (_isFinished)
                RemoveBehavior();
            return true;

        }

    }
}
