﻿using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using Org.BouncyCastle.Asn1.Esf;
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
                Pulsator.OnPulse += (sender, args) => Instance.UpdateInventoryCache();
            }

            public InventoryCache()
            {
                // Make sure data is immediately available 
                // while bot is not running or before pulse starts
                UpdateInventoryCache();
            }

            private static InventoryCache _instance = null;
            public static InventoryCache Instance
            {
                get { return _instance ?? (_instance = new InventoryCache()); }
                set { _instance = value; }
            }

            public List<ACDItem> Backpack { get; private set; }
            public List<ACDItem> Stash { get; private set; }
            public List<ACDItem> Equipped { get; private set; }
            public List<ACDItem> Ground { get; private set; }
            public List<ACDItem> Buyback { get; private set; }
            public List<ACDItem> Other { get; private set; }
            public HashSet<int> EquippedIds { get; private set; }
            public bool IsGroundItemOverload { get; private set; }

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
                                Backpack.Add(item);
                                break;

                            case InventorySlot.SharedStash:
                                Stash.Add(item);
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
                                Equipped.Add(item);
                                EquippedIds.Add(item.ActorSNO);
                                break;

                            case InventorySlot.Buyback:
                                Buyback.Add(item);
                                break;

                            case InventorySlot.None:
                                Ground.Add(item);
                                break;

                            default:
                                    Other.Add(item);
                                break;

                        }
                    }

                    IsGroundItemOverload = (Ground.Count > 50);

                    Logger.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, 
                        "Refreshed Inventory: Backpack={0} Stash={1} Equipped={2} Ground={3}",
                        Backpack.Count,
                        Stash.Count,
                        Equipped.Count,
                        Ground.Count);
                }
            }

            public void Clear()
            {
                Backpack = new List<ACDItem>();
                Stash = new List<ACDItem>();
                Equipped = new List<ACDItem>();
                EquippedIds = new HashSet<int>();
                Ground = new List<ACDItem>();
                Buyback = new List<ACDItem>();
                Other = new List<ACDItem>();
            }

        }

    }
}