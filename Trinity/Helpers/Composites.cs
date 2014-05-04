using System;
using System.Linq;
using Trinity.Technicals;
using Zeta.Bot.Logic;
using Zeta.Game;
using Zeta.TreeSharp;
using Action = Zeta.TreeSharp.Action;

namespace Trinity.Helpers
{
    public class Composites
    {
        private const int waitForCacheDropDelay = 1000;

        public static Composite CreateLootBehavior(Composite child)
        {
            return
            new PrioritySelector(
                CreateUseHoradricCache(),
                child
            );
        }

        private static DateTime lastCheckedForHoradricCache = DateTime.MinValue;
        private static DateTime lastFoundHoradricCache = DateTime.MinValue;

        public static DateTime LastCheckedForHoradricCache
        {
            get { return Composites.lastCheckedForHoradricCache; }
            set { Composites.lastCheckedForHoradricCache = value; }
        }

        public static DateTime LastFoundHoradricCache
        {
            get { return Composites.lastFoundHoradricCache; }
            set { Composites.lastFoundHoradricCache = value; }
        }

        public static Composite CreateUseHoradricCache()
        {
            return
            new PrioritySelector(
                new Action(ret => WaitForHoradricCacheDrops()),
                new Decorator(ret => Trinity.Settings.Loot.TownRun.OpenHoradricCaches && !BrainBehavior.IsVendoring && !Trinity.ForceVendorRunASAP && !TownRun.IsTryingToTownPortal() &&
                        DateTime.UtcNow.Subtract(LastCheckedForHoradricCache).TotalSeconds > 1,
                    new Sequence(
                        new Action(ret => LastCheckedForHoradricCache = DateTime.UtcNow),
                        new Decorator(ret => HasHoradricCaches(),
                            new Action(ret => OpenHoradricCache())
                        )
                    )
                )
            );

        }

        private static RunStatus WaitForHoradricCacheDrops()
        {
            if (DateTime.UtcNow.Subtract(LastFoundHoradricCache).TotalMilliseconds < waitForCacheDropDelay)
            {
                Logger.Log("Waiting for Horadric Cache drops");
                return RunStatus.Running;
            }

            return RunStatus.Failure;
        }


        internal static RunStatus OpenHoradricCache()
        {
            if (HasHoradricCaches())
            {
                var item = ZetaDia.Me.Inventory.Backpack.First(i => i.InternalName.StartsWith(Items.ItemIds.HORADRIC_CACHE));
                ZetaDia.Me.Inventory.UseItem(item.DynamicId);
                LastFoundHoradricCache = DateTime.UtcNow;
                Trinity.TotalBountyCachesOpened++;
                return RunStatus.Running;
            }

            return RunStatus.Success;

        }

        internal static bool HasHoradricCaches()
        {
            return ZetaDia.Me.Inventory.Backpack.Any(i => i.InternalName.StartsWith(Items.ItemIds.HORADRIC_CACHE));
        }

    }
}
