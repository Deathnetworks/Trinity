using GilesTrinity.DbProvider;
using System.Collections.Generic;
using System.Linq;
using Zeta;
using Zeta.Common;
using Zeta.Internals.Actors;

namespace GilesTrinity.Swap
{
    class WeaponSwap
    {
		private static Dictionary<InventorySlot, ACDItem> originalItems;
		private static readonly HashSet<int> IgnoreItems = new HashSet<int>();
		private static bool wearingDPSGear = false;

        static void Main()
        {
            WeaponSwap weaponSwap = new WeaponSwap();
			wearingDPSGear = false;
        }

        public WeaponSwap()
        {
            //init();
        }
		
		public bool DpsGearOn()
		{
			return wearingDPSGear;
		}
		
		public void SwapGear()
        {
            using (ZetaDia.Memory.AcquireFrame())
            {
                ZetaDia.Actors.Update();
                if (!wearingDPSGear)
                {
                    originalItems =
                        ZetaDia.Me.Inventory.Equipped.ToDictionary(
                            key => key.InventorySlot, v => v);
                    IgnoreItems.Clear();

                    DbHelper.Log(TrinityLogLevel.Debug, LogCategory.WeaponSwap, "Swapping to 2H");
                    Equip(InventorySlot.PlayerLeftHand);


                    // Simple state holder so we know when we're wearing MF gear.
                    wearingDPSGear = true;
                }
                else
                {
                    DbHelper.Log(TrinityLogLevel.Debug, LogCategory.WeaponSwap, "Swapping to Dual Wield");

                    // Go back to our original set of gear, in their exact places!
                    foreach (var i in originalItems)
                    {
                        ZetaDia.Me.Inventory.EquipItem(i.Value.DynamicId, i.Key);
                    }
                    wearingDPSGear = false;
                }
            }
        }


        private static ACDItem GetBestDPSItem(InventorySlot slot)
        {
            List<int> ignoreItemIds = originalItems.Values.Select(i => i.DynamicId).ToList();

            
            // For each item that matches the slot we're requesting, get the highest DPS.
            return (from i in ZetaDia.Me.Inventory.Backpack
                    // Ensure the item is valid.
                    // Correct slot, and not on ignore lists.
                    where i.ValidInventorySlots.Contains(slot) &&
                          !ignoreItemIds.Contains(i.DynamicId) &&
                          !IgnoreItems.Contains(i.DynamicId) &&
                          // Make sure the item actually is a weapon!
                          i.Stats.WeaponDamagePerSecond > 0
                    // Order by highest DPS
                    orderby i.Stats.WeaponDamagePerSecond descending
                    
                    // Grab the first.
                    select i).FirstOrDefault();
        }


        private static void Equip(InventorySlot slot)
        {
            ACDItem i = GetBestDPSItem(slot);
            if (i != null)
            {
                ZetaDia.Me.Inventory.EquipItem(i.DynamicId, slot);
                // We equipped the item, so lets ignore it for the next run of getting best equip.
                // This avoids a bug with equipping the same ring twice!
                IgnoreItems.Add(i.DynamicId);
            }
        }
    }
}
