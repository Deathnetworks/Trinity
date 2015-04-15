using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Buddy.Coroutines;
using Trinity.DbProvider;
using Trinity.Helpers;
using Trinity.Items;
using Zeta.Bot.Coroutines;
using Zeta.Bot.Navigation;
using Zeta.Common;
using Zeta.Game;
using Zeta.Game.Internals;
using Zeta.Game.Internals.Actors;
using Zeta.Game.Internals.Actors.Gizmos;
using Logger = Trinity.Technicals.Logger;

namespace Trinity.Coroutines
{
    public class TrinityCoroutines
    {
        public static async Task<bool> MoveTo(Vector3 location, string destinationName, float range = 10f)
        {
            while (Trinity.Player.IsFullyValid && !Trinity.Player.IsInCombat && location.Distance2D(Trinity.Player.Position) > range)
            {
                Logger.LogVerbose("Moving to " + destinationName);
                PlayerMover.NavigateTo(location, destinationName);
                await Coroutine.Yield();
            }
            if (location.Distance2D(Trinity.Player.Position) <= range)
                Navigator.PlayerMover.MoveStop();

            return true;
        }

        public static async Task<bool> MoveToAndInteract(DiaObject obj, float range = -1f)
        {
            if (obj == null)
                return false;
            if (!obj.IsFullyValid())
                return false;
            if (range == -1f)
                range = obj.CollisionSphere.Radius;

            if (obj.Position.Distance2D(Trinity.Player.Position) > range)
                await MoveTo(obj.Position, obj.Name);

            if (obj.Position.Distance2D(Trinity.Player.Position) < range)
                obj.Interact();

            return true;
        }

        private static bool _startedOutOfTown;

        public static bool StartedOutOfTown
        {
            get { return _startedOutOfTown; }
            set { _startedOutOfTown = value; }
        }


        public static async Task<bool> ReturnToStashTask()
        {
            if (ZetaDia.Me.IsInCombat)
            {
                Logger.LogDebug("Cannot return to stash while in combat");
                return false;
            }
            if (!ZetaDia.IsInTown && ZetaDia.Me.IsFullyValid() && !ZetaDia.Me.IsInCombat && UIElements.BackgroundScreenPCButtonRecall.IsEnabled)
            {
                StartedOutOfTown = true;
                await CommonCoroutines.UseTownPortal("Returning to stash");
                return true;
            }

            if (!GameUI.IsElementVisible(GameUI.StashDialogMainPage) && ZetaDia.IsInTown)
            {
                // Move to Stash
                if (TownRun.StashLocation.Distance2D(Trinity.Player.Position) > 10f)
                {
                    await MoveTo(TownRun.StashLocation, "Shared Stash");
                    return true;
                }
                if (TownRun.StashLocation.Distance2D(Trinity.Player.Position) <= 10f && TownRun.SharedStash == null)
                {
                    Logger.LogError("Shared Stash actor is null!");
                    return false;
                }

                // Open Stash
                if (TownRun.StashLocation.Distance2D(Trinity.Player.Position) <= 10f && TownRun.SharedStash != null && !GameUI.IsElementVisible(GameUI.StashDialogMainPage))
                {
                    while (Trinity.Player.IsMoving)
                    {
                        Navigator.PlayerMover.MoveStop();
                        await Coroutine.Yield();
                    }
                    Logger.Log("Opening Stash");
                    TownRun.SharedStash.Interact();
                    await Coroutine.Sleep(200);
                    await Coroutine.Yield();
                    if (GameUI.IsElementVisible(GameUI.StashDialogMainPage))
                        return true;
                    return true;
                }
            }
            return true;
        }

    }
}
