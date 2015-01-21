using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using Trinity.Helpers;
using Trinity.Technicals;
using Zeta.Bot;
using Zeta.Game;
using Zeta.Game.Internals.Actors;

namespace Trinity
{
    public partial class CacheData
    {
        /// <summary>
        /// Fast Inventory Cache, Self-Updating, Use instead of ZetaDia.Inventory
        /// </summary>
        public class InventoryCache
        {
            static InventoryCache()
            {
                Instance.Clear();
                Pulsator.OnPulse += (sender, args) => Instance.UpdateInventoryCache();    
            }

            private static InventoryCache _instance;
            public static InventoryCache Instance
            {
                get
                {
                    if (_instance != null) return _instance;
                    _instance = new InventoryCache();
                    _instance.UpdateInventoryCache();
                    return _instance;
                }
                set { _instance = value; }
            }

            public List<ACDItem> Backpack { get; private set; }

            public List<ACDItem> Stash { get; private set; }
            public List<ACDItem> Equipped { get; private set; }
            public HashSet<int> EquippedIds { get; private set; }
            public ILookup<int, ACDItem> BackpackByActorId { get; private set; }
            public ILookup<int, ACDItem> StashByActorId { get; private set; }

            public void UpdateInventoryCache()
            {
                using (new PerformanceLogger("UpdateCachedInventoryData"))
                {

                    if (!Player.IsValid)
                        return;

                    Clear();

                    foreach (var item in ZetaDia.Actors.GetActorsOfType<ACDItem>())
                    {
                        if (!item.IsValid)
                            return;

                        switch (item.InventorySlot)
                        {
                            case InventorySlot.BackpackItems:
                                Instance.Backpack.Add(item);
                                break;

                            case InventorySlot.SharedStash:
                                Instance.Stash.Add(item);
                                break;

                            case InventorySlot.Bracers:
                            case InventorySlot.Feet:
                            case InventorySlot.Hands:
                            case InventorySlot.Head:
                            case InventorySlot.Waist:
                            case InventorySlot.Shoulders:
                            case InventorySlot.Torso:
                            case InventorySlot.LeftFinger:
                            case InventorySlot.RightFinger:
                            case InventorySlot.RightHand:
                            case InventorySlot.LeftHand:
                            case InventorySlot.Legs:
                            case InventorySlot.Neck:
                            case InventorySlot.Socket:
                                Instance.Equipped.Add(item);
                                Instance.EquippedIds.Add(item.ActorSNO);
                                break;
                        }
                    }

                    Instance.BackpackByActorId = Instance.Backpack.ToLookup(k => k.ActorSNO, v => v);
                    Instance.StashByActorId = Instance.Stash.ToLookup(k => k.ActorSNO, v => v);

                    Logger.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, 
                        "Refreshed Inventory: Backpack={0} Stash={1} Equipped={2}",
                        Backpack.Count,
                        Stash.Count,
                        Equipped.Count);
                }
            }

            public void Clear()
            {
                Backpack = new List<ACDItem>();
                Stash = new List<ACDItem>();
                Equipped = new List<ACDItem>();
                EquippedIds = new HashSet<int>();
                BackpackByActorId = new LookupList<int, ACDItem>();
                StashByActorId = new LookupList<int, ACDItem>();
            }

        }

    }
}