using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Buddy.Coroutines;
using Trinity.Helpers;
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
            while (ZetaDia.Me.IsFullyValid() && !ZetaDia.Me.IsInCombat && location.Distance2D(ZetaDia.Me.Position) > range)
            {
                Logger.LogVerbose("Moving to " + destinationName);
                Navigator.MoveTo(location, destinationName);
                await Coroutine.Yield();
            }
            if (location.Distance2D(ZetaDia.Me.Position) <= range)
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

            if (obj.Position.Distance2D(ZetaDia.Me.Position) > range)
                await MoveTo(obj.Position, obj.Name);

            if (obj.Position.Distance2D(ZetaDia.Me.Position) < range)
                obj.Interact();
            
            return true;
        }
    }
}
