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
using GilesTrinity.Technicals;

namespace GilesTrinity.Swap
{
    /// <summary>
    /// Version: 1.0.1b
    /// - Fixed an issue that would cause people that aren't monks using weapon swap to run the checks during town run.
    /// </summary>
    class WeaponSwap
    {
        private static bool wearingDPSGear = false;
        // Whether or not both weapons are 2 handers OR if can't find the offhand.
        private static bool MainIsTwoHander = false;
        private static bool hasChecked = false;
        private static bool crashedDuringSwap = false;
        // Is able to swap or not??
        private static bool ableToSwap = true;
        private static int sGamesCreated = -1;
        /// <summary>
        /// Make sure rows and columns have the same amount of numbers inside (top left corner is row=0,column=0, bottom right corner is row=5,column=9). 
        /// Each combination of Row and Column is an item location, for instance Row 0, Column 3 -> Top row, 4th column from the left.
        /// Then we look at the array "items" -> if first item in rows is 0, first item in columns is 3 and first item in items is InventorySlot.PlayerLeftFinger -> 
        ///         It will be the location we put the left ring for swapping.
        /// </summary>
        private static int[] rows = new int[] { };
        private static int[] columns = new int[] { };

        /// <summary>
        /// Possible Inventory Slots:
        ///     InventorySlot.PlayerNeck
        ///     InventorySlot.PlayerRightFinger
        ///     InventorySlot.PlayerLeftFinger
        ///     InventorySlot.PlayerHead
        ///     InventorySlot.PlayerShoulders
        ///     InventorySlot.PlayerTorso
        ///     InventorySlot.PlayerBracers
        ///     InventorySlot.PlayerHands
        ///     InventorySlot.PlayerWaist
        ///     InventorySlot.PlayerLegs
        ///     InventorySlot.PlayerFeet
        /// </summary>

        private static InventorySlot[] items = new InventorySlot[] { };
        // Last slot is reserved for Offhand (if exists) and the slot before last for the main hand
        private static int[] mainID = new int[rows.Length + 3];
        // Last slot is reserved for the 2 Handed weapon we swap to.
        private static int[] altID = new int[rows.Length + 1];

        static void Main()
        {
            WeaponSwap weaponSwap = new WeaponSwap();
            wearingDPSGear = false;
        }

        // Returns if this item is protected by the swapper or not -> should make items safe from town run routine
        public bool SwapperUsing(ACDItem thisItem)
        {
            if (!GilesTrinity.Settings.Combat.Monk.SweepingWindWeaponSwap || GilesTrinity.playerStatus.ActorClass != ActorClass.Monk)
            {
                return false;
            }
            else if (hasChecked == false)
            {
                SecurityCheck();
            }
            return (mainID.Contains(thisItem.DynamicId) || altID.Contains(thisItem.DynamicId));
        }

        public WeaponSwap()
        {
        }

        public void SwapCastBlindingFlash(bool sHasInnaSet)
        {
            if (GilesTrinity.playerStatus.CurrentEnergy >= 85 || (sHasInnaSet && GilesTrinity.playerStatus.CurrentEnergy >= 15))
            {
                ZetaDia.Me.UsePower(SNOPower.Monk_BlindingFlash, ZetaDia.Me.Position, ZetaDia.Me.WorldDynamicId, -1);
            }
        }

        public void SwapCastSweepingWinds(bool sHasInnaSet)
        {
            if (GilesTrinity.playerStatus.CurrentEnergy >= 75 || (sHasInnaSet && GilesTrinity.playerStatus.CurrentEnergy >= 5))
            {
                ZetaDia.Me.UsePower(SNOPower.Monk_SweepingWind, ZetaDia.Me.Position, ZetaDia.Me.WorldDynamicId, -1);
                GilesTrinity.SweepWindSpam = DateTime.Now;
            }
        }

        public void Reset()
        {
            hasChecked = false;
        }

        internal static DateTime LastCheckCanSwap = DateTime.Now;

        public bool CanSwap()
        {
            //if (DateTime.Now.Subtract(LastCheckCanSwap).TotalMilliseconds <= 250)
            //    return ableToSwap;
            //LastCheckCanSwap = DateTime.Now;

            if (sGamesCreated < GilesTrinity.iTotalJoinGames)
            {
                sGamesCreated = GilesTrinity.iTotalJoinGames;
                Reset();
            }
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
                return (ableToSwap);
            }
        }

        internal static DateTime LastSecurityCheck = DateTime.Now;

        private void SecurityCheck()
        {
            // Don't run if we're not a monk
            if (GilesTrinity.playerStatus.ActorClass != ActorClass.Monk)
                return;

            // Don't run if we've already checked within 250ms
            //if (DateTime.Now.Subtract(LastSecurityCheck).TotalMilliseconds <= 250)
            //    return;
            //LastSecurityCheck = DateTime.Now;

            ableToSwap = true;
            //ZetaDia.Actors.Update();
            ACDItem invItem = null;
            if (ZetaDia.Me.Inventory.ItemInLocation(InventorySlot.PlayerBackpack, 0, 0))
            {
                invItem = ZetaDia.Me.Inventory.Backpack.Where(i => i.InventoryColumn == 0 && i.InventoryRow == 0).FirstOrDefault();
            }
            ACDItem offHand = null;
            if (ZetaDia.Me.Inventory.ItemInLocation(InventorySlot.PlayerBackpack, 9, 4))
            {
                offHand = ZetaDia.Me.Inventory.Backpack.Where(i => i.InventoryColumn == 9 && i.InventoryRow == 4).FirstOrDefault();
            }
            ACDItem offHandEQ = null;
            if (ZetaDia.Me.Inventory.ItemInLocation(InventorySlot.PlayerRightHand, 0, 0))
            {
                offHandEQ = ZetaDia.Me.Inventory.Equipped.FirstOrDefault(i => i.InventorySlot == InventorySlot.PlayerRightHand);
            }
            ACDItem equipItem = null;
            if (ZetaDia.Me.Inventory.ItemInLocation(InventorySlot.PlayerLeftHand, 0, 0))
            {
                equipItem = ZetaDia.Me.Inventory.Equipped.FirstOrDefault(i => i.InventorySlot == InventorySlot.PlayerLeftHand);
            }
            // Make sure both main items exist and are weapons
            if (equipItem != null && invItem != null &&
                equipItem.ValidInventorySlots.Contains(InventorySlot.PlayerLeftHand) && invItem.ValidInventorySlots.Contains(InventorySlot.PlayerLeftHand) && !invItem.IsUnidentified)
            {
                // Check if we are swapping 2x 2 Handed weapons
                if (invItem.IsTwoHand && equipItem.IsTwoHand)
                {
                    MainIsTwoHander = true;
                    DbHelper.Log(TrinityLogLevel.Normal, LogCategory.WeaponSwap, "[Swapper] Detected you are using a Two-Hander as main.");
                    // Two Hand
                    altID[altID.Length - 1] = invItem.DynamicId;
                    // Main hand
                    mainID[mainID.Length - 2] = equipItem.DynamicId;
                }
                // Check if no offhand can be found
                else if ((offHand == null || offHand.ItemType != ItemType.Shield && !offHand.IsOneHand) && (offHandEQ == null || offHandEQ.ItemType != ItemType.Shield && !offHandEQ.IsOneHand || offHandEQ.IsUnidentified))
                {
                    DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "[Swapper] Can't find offhand, please post this log and the one before this -> on the forums.");
                    DbHelper.Log(TrinityLogLevel.Normal, LogCategory.WeaponSwap, "[Swapper] Will continue without offhand.");
                    MainIsTwoHander = true;
                    if (equipItem.IsTwoHand)
                    {
                        crashedDuringSwap = true;
                        // Two hand
                        altID[altID.Length - 1] = equipItem.DynamicId;
                        // Main hand
                        mainID[mainID.Length - 2] = invItem.DynamicId;
                    }
                    else
                    {
                        // Two Hand
                        altID[altID.Length - 1] = invItem.DynamicId;
                        // Main hand
                        mainID[mainID.Length - 2] = equipItem.DynamicId;
                    }
                }
                // Check if we crashed during swap
                else if (invItem != null && equipItem.IsTwoHand)
                {
                    crashedDuringSwap = true;
                    DbHelper.Log(TrinityLogLevel.Normal, LogCategory.WeaponSwap, "[Swapper] Crashed during a swap, no fear all is ok.");

                    // Two hand
                    altID[altID.Length - 1] = equipItem.DynamicId;
                    // Main hand
                    mainID[mainID.Length - 2] = invItem.DynamicId;
                    // Offhand
                    mainID[mainID.Length - 1] = offHand.DynamicId;
                }
                // Default
                else
                {
                    // Main hand
                    mainID[mainID.Length - 2] = equipItem.DynamicId;
                    // Two hand
                    altID[altID.Length - 1] = invItem.DynamicId;
                    // Offhand
                    mainID[mainID.Length - 1] = offHandEQ.DynamicId;
                }
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.WeaponSwap, "[Swapper] Saving item ID's for later use.");
                if (crashedDuringSwap == true)
                {
                    // If crashed during swapped, the currently equipped items are the alternative items and the items in the inventory are the MAIN items.
                    for (int i = 0; i < items.Length; i++)
                    {
                        try
                        {
                            altID[i] = ZetaDia.Me.Inventory.Equipped.FirstOrDefault(j => j.InventorySlot == items[i]).DynamicId;

                            ACDItem item = ZetaDia.Me.Inventory.Backpack.Where(j => j.InventoryColumn == columns[i] && j.InventoryRow == rows[i]).FirstOrDefault();
                            mainID[i] = item.DynamicId;
                            if (!item.ValidInventorySlots.Contains(items[i]))
                            {
                                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "[Swapper] Item at location: Row = " + rows[i] + ", Column = " + columns[i] + " can't be placed in " + items[i].ToString());
                                items[i] = InventorySlot.Gold;
                            }

                        }
                        catch
                        {
                            DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "[Swapper] Problem with item at location: Row = " + rows[i] + ", Column = " + columns[i] + ".");
                            items[i] = InventorySlot.Gold;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < items.Length; i++)
                    {
                        try
                        {
                            mainID[i] = ZetaDia.Me.Inventory.Equipped.FirstOrDefault(j => j.InventorySlot == items[i]).DynamicId;

                            ACDItem item = ZetaDia.Me.Inventory.Backpack.Where(j => j.InventoryColumn == columns[i] && j.InventoryRow == rows[i]).FirstOrDefault();
                            altID[i] = item.DynamicId;
                            if (!item.ValidInventorySlots.Contains(items[i]))
                            {
                                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "[Swapper] Problem with item at location: Row = " + rows[i] + ", Column = " + columns[i]);
                                items[i] = InventorySlot.Gold;
                            }
                        }
                        catch
                        {
                            DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "[Swapper] Problem with item at location: Row = " + rows[i] + ", Column = " + columns[i]);
                            items[i] = InventorySlot.Gold;
                        }
                    }
                }
                hasChecked = true;
            }
            else
            {
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "[Swapper] Unable to swap - there is no weapon in the top left corner.");
                ableToSwap = false;
            }
        }

        public bool DpsGearOn()
        {
            return wearingDPSGear;
        }

        internal static DateTime LastCheckSwapGear = DateTime.Now;

        public void SwapGear()
        {
            if (GilesTrinity.playerStatus.ActorClass != ActorClass.Monk)
            {
                return;
            }

            //if (DateTime.Now.Subtract(LastCheckSwapGear).TotalMilliseconds <= 250)
            //    return;
            //LastCheckSwapGear = DateTime.Now;

            if (!hasChecked)
            {
                SecurityCheck();
            }
            if (ableToSwap == true)
            {
                if (crashedDuringSwap == false)
                {
                    if (!wearingDPSGear)
                    {
                        if (MainIsTwoHander ||
                            !MainIsTwoHander && !ZetaDia.Me.Inventory.ItemInLocation(InventorySlot.PlayerBackpack, 9, 5) && !ZetaDia.Me.Inventory.ItemInLocation(InventorySlot.PlayerBackpack, 9, 4))
                        {
                            if (!MainIsTwoHander)
                            {
                                // Move Offhand to bottom right corner
                                ZetaDia.Me.Inventory.MoveItem(mainID[mainID.Length - 1],
                                        ZetaDia.Me.CommonData.DynamicId, InventorySlot.PlayerBackpack, 9, 4);
                            }
                            // Swap other shiz
                            for (int i = 0; i < rows.Length; i++)
                            {
                                if (items[i] != InventorySlot.Gold)
                                {
                                    ZetaDia.Me.Inventory.EquipItem(altID[i], items[i]);
                                }
                            }
                            // Equip the 2 handed weapon
                            ZetaDia.Me.Inventory.EquipItem(altID[altID.Length - 1], InventorySlot.PlayerLeftHand);
                            wearingDPSGear = true;
                            DbHelper.Log(TrinityLogLevel.Normal, LogCategory.WeaponSwap, "[Swapper] Swapped to strong 2 hander.");
                        }
                        else
                        {
                            // Force town run due to last spot taken!
                            GilesTrinity.bWantToTownRun = true;
                            DbHelper.Log(TrinityLogLevel.Normal, LogCategory.WeaponSwap, "[Swapper] For some reason bottom right corner isn't protected, initializing town run to clear it up.");
                        }
                    }
                    else
                    {
                        // Equip non weapon items
                        for (int i = 0; i < rows.Length; i++)
                        {
                            if (items[i] != InventorySlot.Gold)
                            {
                                ZetaDia.Me.Inventory.EquipItem(mainID[i], items[i]);
                            }
                        }
                        // Equip main hand
                        ZetaDia.Me.Inventory.EquipItem(mainID[mainID.Length - 2], InventorySlot.PlayerLeftHand);
                        if (!MainIsTwoHander)
                        {
                            // Equip offhand
                            ZetaDia.Me.Inventory.EquipItem(mainID[mainID.Length - 1], InventorySlot.PlayerRightHand);
                        }
                        wearingDPSGear = false;
                        DbHelper.Log(TrinityLogLevel.Normal, LogCategory.WeaponSwap, "[Swapper] Swapped back to normal gear.");
                    }
                }
            }
            else if (crashedDuringSwap == true)
            {
                wearingDPSGear = true;
            }
            crashedDuringSwap = false;
        }

    }
}
