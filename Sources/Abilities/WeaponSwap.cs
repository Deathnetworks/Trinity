using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Collections;
using Zeta;
using Zeta.Common;
using Zeta.Common.Plugins;
using Zeta.CommonBot;
using Zeta.CommonBot.Profile;
using Zeta.CommonBot.Profile.Composites;
using Zeta.Internals;
using Zeta.Internals.Actors;
using Zeta.Internals.Actors.Gizmos;
using Zeta.Internals.SNO;
using Zeta.Navigation;
using Zeta.TreeSharp;
using System.Reflection;
using System.Threading;

namespace GilesTrinity.Swap
{
    class WeaponSwap
    {
        private static bool wearingDPSGear = false;
        private static bool MainIsTwoHander = false;
        private static bool hasChecked = false;
        private static bool crashedDuringSwap = false;

        static void Main()
        {
            WeaponSwap weaponSwap = new WeaponSwap();
            wearingDPSGear = false;
        }

        public WeaponSwap()
        {
        }

        public bool CanSwap()
        {
            if (hasChecked == false)
            {
                SecurityCheck();
            }
            if (crashedDuringSwap == true)
            {
                return (true);
            }
            else
            {
                return (!ZetaDia.Me.Inventory.ItemInLocation(InventorySlot.PlayerBackpack, 9, 5) && !ZetaDia.Me.Inventory.ItemInLocation(InventorySlot.PlayerBackpack, 9, 4));
            }
        }

        private void SecurityCheck()
        {
            int row = 0; int column = 0;
            var myItem = ZetaDia.Me.Inventory.Backpack.Where(i => i.InventoryColumn == column && i.InventoryRow == row).FirstOrDefault();
            if (myItem.IsTwoHand && ZetaDia.Me.Inventory.Equipped.FirstOrDefault(i => i.InventorySlot == InventorySlot.PlayerLeftHand).IsTwoHand)
            {
                MainIsTwoHander = true;
                Logging.Write("[Swapper] Detected you are using a Two-Hander as main.");
            }
            else if (!myItem.IsTwoHand)
            {
                crashedDuringSwap = true;
                Logging.Write("[Swapper] Crashed during a swap, no fear all is ok.");
            }
            hasChecked = true;
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
                int row = 0; int column = 0;
                var myItem = ZetaDia.Me.Inventory.Backpack.Where(i => i.InventoryColumn == column && i.InventoryRow == row).FirstOrDefault();
                if (hasChecked == false)
                {
                    SecurityCheck();
                }
                if (!wearingDPSGear && !crashedDuringSwap)
                {
                    if (!ZetaDia.Me.Inventory.ItemInLocation(InventorySlot.PlayerBackpack, 9, 5) && !ZetaDia.Me.Inventory.ItemInLocation(InventorySlot.PlayerBackpack, 9, 4))
                    {
                        if (!MainIsTwoHander)
                        {
                            //Move Offhand to bottom right corner
                            ZetaDia.Me.Inventory.MoveItem(ZetaDia.Me.Inventory.Equipped.FirstOrDefault(i => i.InventorySlot == InventorySlot.PlayerRightHand).DynamicId,
                                    ZetaDia.Me.CommonData.DynamicId, InventorySlot.PlayerBackpack, 9, 4);
                        }
                        //Swap other shiz
                        row = 0; column = 0;
                        myItem = ZetaDia.Me.Inventory.Backpack.Where(i => i.InventoryColumn == column && i.InventoryRow == row).FirstOrDefault();
                        ZetaDia.Me.Inventory.EquipItem(myItem.DynamicId, InventorySlot.PlayerLeftHand);
                        wearingDPSGear = true;
                        Logging.Write("[Swapper] Swapped to strong 2 hander.");
                    }
                    else
                    {
                        //Force town run due to last spot taken!
                        GilesTrinity.bWantToTownRun = true;
                        Logging.Write("[Swapper] For some reason bottom right corner isn't protected, initializing town run to clear it up.");
                    }
                }
                else if (!crashedDuringSwap)
                {
                    row = 0; column = 0;
                    myItem = ZetaDia.Me.Inventory.Backpack.Where(i => i.InventoryColumn == column && i.InventoryRow == row).FirstOrDefault();
                    ZetaDia.Me.Inventory.EquipItem(myItem.DynamicId, InventorySlot.PlayerLeftHand);
                    if (!MainIsTwoHander)
                    {
                        //Equip offhand
                        row = 4; column = 9;
                        myItem = ZetaDia.Me.Inventory.Backpack.Where(i => i.InventoryColumn == column && i.InventoryRow == row).FirstOrDefault();
                        ZetaDia.Me.Inventory.EquipItem(myItem.DynamicId, InventorySlot.PlayerRightHand);
                    }
                    wearingDPSGear = false;
                    Logging.Write("[Swapper] Swapped back to normal gear.");
                }
                if (crashedDuringSwap == true)
                {
                    wearingDPSGear = true;
                }
                crashedDuringSwap = false;
            }
        }
    }
}
