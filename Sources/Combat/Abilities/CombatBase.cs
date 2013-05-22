using Trinity.DbProvider;
using Trinity.Technicals;
using System;
using System.Linq;
using Zeta.Common;
using Zeta.Common.Plugins;
using Zeta.CommonBot;
using Zeta.Internals.Actors;
using System.Collections.Generic;
using Zeta;

namespace Trinity.Combat.Abilities
{
    public class CombatBase
    {
        public static TrinityPower GetDefaultPower()
        {
            return GetDefaultPower(new CombatContext());
        }

        public static TrinityPower GetDefaultPower(CombatContext ctx)
        {
            // Default attacks
            if (!ctx.UseOutOfCombatBuff && !ctx.IsCurrentlyAvoiding)
            {
                if (Trinity.PlayerStatus.ActorClass == ActorClass.Monk && ctx.Hotbar.Contains(SNOPower.Monk_SweepingWind))
                {
                    Trinity.Monk_TickSweepingWindSpam();
                }

                return new TrinityPower()
                {
                    SNOPower = GetDefaultWeaponPower(),
                    MinimumRange = GetDefaultWeaponDistance(),
                    TargetRActorGUID = ctx.CurrentTarget.ACDGuid,
                    WaitForAnimationFinished = true
                };
            }
            return new TrinityPower();
        }

        /// <summary>
        /// Gets the default weapon power based on the current equipped primary weapon
        /// </summary>
        /// <returns></returns>
        public static SNOPower GetDefaultWeaponPower()
        {
            ACDItem rhItem = ZetaDia.Me.Inventory.Equipped.Where(i => i.InventorySlot == InventorySlot.PlayerLeftHand).FirstOrDefault();
            if (rhItem == null)
                return SNOPower.None;

            switch (rhItem.ItemType)
            {
                default:
                    return SNOPower.Weapon_Melee_Instant;
                case ItemType.Axe:
                case ItemType.CeremonialDagger:
                case ItemType.Dagger:
                case ItemType.Daibo:
                case ItemType.FistWeapon:
                case ItemType.Mace:
                case ItemType.Polearm:
                case ItemType.Spear:
                case ItemType.Staff:
                case ItemType.Sword:
                    return SNOPower.Weapon_Melee_Instant;
                case ItemType.Wand:
                    return SNOPower.Weapon_Ranged_Wand;
                case ItemType.Bow:
                case ItemType.Crossbow:
                case ItemType.HandCrossbow:
                    return SNOPower.Weapon_Ranged_Projectile;
            }
        }
        /// <summary>
        /// Gets the default weapon distance based on the current equipped primary weapon
        /// </summary>
        /// <returns></returns>
        public static float GetDefaultWeaponDistance()
        {
            switch (GetDefaultWeaponPower())
            {
                case SNOPower.Weapon_Ranged_Instant:
                case SNOPower.Weapon_Ranged_Projectile:
                    return 65f;
                case SNOPower.Weapon_Ranged_Wand:
                    return 35f;
                case SNOPower.Weapon_Melee_Instant:
                case SNOPower.Weapon_Melee_Instant_BothHand:
                default:
                    return 10f;
            }
        }
    }
}
