﻿using Zeta.Game.Internals;

namespace Trinity.Items
{
    class ItemStatsDataFactory
    {
        internal static ItemStatsData GetItemStatsDataFromStats(ItemStats stats)
        {
            if (!stats.Item.IsValid)
                return default(ItemStatsData);

            ItemStatsData itemStatsData = new ItemStatsData()
            {
                HatredRegen = stats.HatredRegen,
                MaxDiscipline = stats.MaxDiscipline,
                MaxArcanePower = stats.MaxArcanePower,
                MaxMana = stats.MaxMana,
                MaxFury = stats.MaxFury,
                MaxSpirit = stats.MaxSpirit,
                ManaRegen = stats.ManaRegen,
                SpiritRegen = stats.SpiritRegen,
                ArcaneOnCrit = stats.ArcaneOnCrit,
                HealthPerSpiritSpent = stats.HealthPerSpiritSpent,
                AttackSpeedPercent = stats.AttackSpeedPercent,
                AttackSpeedPercentBonus = stats.AttackSpeedPercentBonus,
                Quality = stats.Quality.ToString(),
                Level = stats.Level,
                ItemLevelRequirementReduction = stats.ItemLevelRequirementReduction,
                RequiredLevel = stats.RequiredLevel,
                CritPercent = stats.CritPercent,
                CritDamagePercent = stats.CritDamagePercent,
                BlockChance = stats.BlockChance,
                BlockChanceBonus = stats.BlockChanceBonus,
                HighestPrimaryAttribute = stats.HighestPrimaryAttribute,
                Intelligence = stats.Intelligence,
                Vitality = stats.Vitality,
                Strength = stats.Strength,
                Dexterity = stats.Dexterity,
                Armor = stats.Armor,
                ArmorBonus = stats.ArmorBonus,
                ArmorTotal = stats.ArmorTotal,
                Sockets = stats.Sockets,
                LifeSteal = stats.LifeSteal,
                LifeOnHit = stats.LifeOnHit,
                LifeOnKill = stats.LifeOnKill,
                MagicFind = stats.MagicFind,
                GoldFind = stats.GoldFind,
                ExperienceBonus = stats.ExperienceBonus,
                WeaponOnHitSlowProcChance = stats.WeaponOnHitSlowProcChance,
                WeaponOnHitBlindProcChance = stats.WeaponOnHitBlindProcChance,
                WeaponOnHitChillProcChance = stats.WeaponOnHitChillProcChance,
                WeaponOnHitFearProcChance = stats.WeaponOnHitFearProcChance,
                WeaponOnHitFreezeProcChance = stats.WeaponOnHitFreezeProcChance,
                WeaponOnHitImmobilizeProcChance = stats.WeaponOnHitImmobilizeProcChance,
                WeaponOnHitKnockbackProcChance = stats.WeaponOnHitKnockbackProcChance,
                WeaponOnHitBleedProcChance = stats.WeaponOnHitBleedProcChance,
                WeaponDamagePercent = stats.WeaponDamagePercent,
                WeaponAttacksPerSecond = stats.WeaponAttacksPerSecond,
                WeaponMinDamage = stats.WeaponMinDamage,
                WeaponMaxDamage = stats.WeaponMaxDamage,
                WeaponDamagePerSecond = stats.WeaponDamagePerSecond,
                WeaponDamageType = stats.WeaponDamageType.ToString(),
                MaxDamageElemental = stats.MaxDamageElemental,
                MinDamageElemental = stats.MinDamageElemental,
                MinDamageFire = stats.MinDamageFire,
                MaxDamageFire = stats.MaxDamageFire,
                MinDamageLightning = stats.MinDamageLightning,
                MaxDamageLightning = stats.MaxDamageLightning,
                MinDamageCold = stats.MinDamageCold,
                MaxDamageCold = stats.MaxDamageCold,
                MinDamagePoison = stats.MinDamagePoison,
                MaxDamagePoison = stats.MaxDamagePoison,
                MinDamageArcane = stats.MinDamageArcane,
                MaxDamageArcane = stats.MaxDamageArcane,
                MinDamageHoly = stats.MinDamageHoly,
                MaxDamageHoly = stats.MaxDamageHoly,
                OnHitAreaDamageProcChance = stats.OnHitAreaDamageProcChance,
                PowerCooldownReductionPercent = stats.PowerCooldownReductionPercent,
                ResourceCostReductionPercent = stats.ResourceCostReductionPercent,
                PickUpRadius = stats.PickUpRadius,
                MovementSpeed = stats.MovementSpeed,
                HealthGlobeBonus = stats.HealthGlobeBonus,
                HealthPerSecond = stats.HealthPerSecond,
                LifePercent = stats.LifePercent,
                DamagePercentBonusVsElites = stats.DamagePercentBonusVsElites,
                DamagePercentReductionFromElites = stats.DamagePercentReductionFromElites,
                Thorns = stats.Thorns,
                ResistAll = stats.ResistAll,
                ResistArcane = stats.ResistArcane,
                ResistCold = stats.ResistCold,
                ResistFire = stats.ResistFire,
                ResistHoly = stats.ResistHoly,
                ResistLightning = stats.ResistLightning,
                ResistPhysical = stats.ResistPhysical,
                ResistPoison = stats.ResistPoison,
                DamageReductionPhysicalPercent = stats.DamageReductionPhysicalPercent,
                SkillDamagePercentBonus = stats.SkillDamagePercentBonus,
                ArcaneSkillDamagePercentBonus = stats.ArcaneSkillDamagePercentBonus,
                ColdSkillDamagePercentBonus = stats.ColdSkillDamagePercentBonus,
                FireSkillDamagePercentBonus = stats.FireSkillDamagePercentBonus,
                HolySkillDamagePercentBonus = stats.HolySkillDamagePercentBonus,
                LightningSkillDamagePercentBonus = stats.LightningSkillDamagePercentBonus,
                PosionSkillDamagePercentBonus = stats.PosionSkillDamagePercentBonus,
                MinDamage = stats.MinDamage,
                MaxDamage = stats.MaxDamage,
                BaseType = stats.BaseType.ToString(),
                ItemType = stats.ItemType.ToString()
            };
            return itemStatsData;
        }
    }
}
