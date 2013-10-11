using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml;
using Trinity.Helpers;
using Trinity.Technicals;
using Zeta;

namespace Trinity
{
    /// <summary>
    /// Trinity Dynamic Variables
    /// </summary>
    [KnownType(typeof(TVar))]
    [DataContract(Namespace = "")]
    public static class V
    {
        public static void SetDefaults()
        {
            // Disable file save on variable modification/insert
            batch = true;

            // Barbarian 
            Set(new TVar("Barbarian.MinEnergyReserve", 56, "Ignore Pain Emergency Use Minimum Health Percent"));
            Set(new TVar("Barbarian.IgnorePain.MinHealth", 0.45f, "Ignore Pain Emergency Use Minimum Health Percent"));
            Set(new TVar("Barbarian.Avoidance.WOTB.Other", 0.3f, "General WOTB Avoidance health multiplier"));
            Set(new TVar("Barbarian.Avoidance.WOTB.Desecrator", 0.2f, "WOTB Desecrator health multiplier"));
            Set(new TVar("Barbarian.Avoidance.WOTB.Arcane", 0f, "WOTB Arcane health multiplier"));
            Set(new TVar("Barbarian.Avoidance.WOTB.IceBall", 0f, "WOTB IceBall health multiplier"));
            Set(new TVar("Barbarian.Avoidance.WOTB.PoisonTree", 1f, "WOTB Poison Tree / Spore health multiplier"));
            Set(new TVar("Barbarian.Avoidance.WOTB.Belial", 1f, "WOTB Belial health multiplier"));
            Set(new TVar("Barbarian.Avoidance.WOTB.BeastCharge", 1f, "WOTB Belial health multiplier"));
            Set(new TVar("Barbarian.ThreatShout.Range", 25f, "Threating Shout Mob Range distance"));
            Set(new TVar("Barbarian.ThreatShout.OOCMaxFury", 25, "Threating Shout Out of Combat Max Fury"));
            Set(new TVar("Barbarian.WarCry.MaxFury", 60, "Maximum Fury to cast WarCry (with buff)"));
            Set(new TVar("Barbarian.Sprint.MinFury", 20f, "Minimum Fury to try cast Sprint"));
            Set(new TVar("Barbarian.Sprint.SingleTargetRange", 16f, "Minimum Fury to try cast Sprint"));
            Set(new TVar("Barbarian.Sprint.SingleTargetMinFury", 20f, "Minimum Fury to try cast Sprint"));
            Set(new TVar("Barbarian.Sprint.MinUseDelay", 250, "Minimum time in Millseconds before Sprint can be re-cast, even with Fury dump"));
            Set(new TVar("Barbarian.BattleRage.MinFury", 20, "Minimum Fury to try cast Battle Rage"));
            Set(new TVar("Barbarian.WOTB.MinFury", 50, "Minimum Fury to try cast WOTB"));
            Set(new TVar("Barbarian.WOTB.MinRange", 20f, "Elites in Range to try cast WOTB (with WOTB.MinCount) non-hard elites, non ignore elites"));
            Set(new TVar("Barbarian.WOTB.MinCount", 1, "Elite count to try cast WOTB (with WOTB.MinRange) non-hard elites, non ignore elites"));
            Set(new TVar("Barbarian.WOTB.RangeNear", 25f, "Nearby range check to use WOTB with Ignore Elites"));
            Set(new TVar("Barbarian.WOTB.RangeFar", 50f, "Nearby mob count check to use WOTB with Ignore Elites"));
            Set(new TVar("Barbarian.WOTB.CountNear", 3, "Extended range check to use WOTB with Ignore Elites"));
            Set(new TVar("Barbarian.WOTB.CountFar", 10, "Extended mob count check to use WOTB with Ignore Elites"));
            Set(new TVar("Barbarian.WOTB.HardEliteCountOverride", 4, "Will over-ride WOTB hard elite check when this many elites are present"));
            Set(new TVar("Barbarian.WOTB.HardEliteRangeOverride", 50f, "Range check distance for WOTB Hard elite override"));
            Set(new TVar("Barbarian.WOTB.FuryDumpMin", 0.95, "Percentage Fury to start dumping"));
            Set(new TVar("Barbarian.WOTB.EmergencyHealth", 0.49, "Always pop WOTB when below this % Health"));
            Set(new TVar("Barbarian.CallOfTheAncients.MinFury", 50, "Minimum Fury to try cast Call of the Ancients"));
            Set(new TVar("Barbarian.CallOfTheAncients.MinEliteRange", 25f, "Minimum range elites must be in to use COTA"));
            Set(new TVar("Barbarian.CallOfTheAncients.TickDelay", 4, "Pre and Post use Tick Delay"));
            Set(new TVar("Barbarian.AncientSpear.UseRange", 55f, "Power Use Range"));
            Set(new TVar("Barbarian.Whirlwind.UseRange", 10f, "Power Use Range"));
            Set(new TVar("Barbarian.Whirlwind.MinFury", 10d, "Minimum Fury"));
            Set(new TVar("Barbarian.Whirlwind.TrashRange", 25f, "Minimum Fury"));
            Set(new TVar("Barbarian.Whirlwind.TrashCount", 1, "Minimum Fury"));
            Set(new TVar("Barbarian.Whirlwind.EliteRange", 25f, "Minimum Fury"));
            Set(new TVar("Barbarian.Whirlwind.EliteCount", 1, "Minimum Fury"));
            Set(new TVar("Barbarian.Whirlwind.UseForMovement", true, "Use Whirlwind when moving near ignored mobs"));
            Set(new TVar("Barbarian.Whirlwind.ZigZagDistance", 15f, "Whirlwind ZigZag Range"));
            Set(new TVar("Barbarian.Whirlwind.ZigZagMaxTime", 1200, "Maximum time to keep a zig zag point before forcing a new point (millseconds)"));
            Set(new TVar("Barbarian.Bash.UseRange", 6f, "Power Use Range"));
            Set(new TVar("Barbarian.Frenzy.UseRange", 10f, "Power Use Range"));
            Set(new TVar("Barbarian.Cleave.UseRange", 6f, "Power Use Range"));
            Set(new TVar("Barbarian.WeaponThrow.UseRange", 25f, "Power Use Range"));
            Set(new TVar("Barbarian.GroundStomp.UseBelowHealthPct", 0.70f, "Use Ground Stomp below this health % (regardless of unit count)"));
            Set(new TVar("Barbarian.GroundStomp.EliteRange", 15f, "Use Ground Stomp Elites check Range"));
            Set(new TVar("Barbarian.GroundStomp.EliteCount", 1, "Use Ground Stomp when this many Elites in Range"));
            Set(new TVar("Barbarian.GroundStomp.TrashRange", 15f, "Use Ground Stomp Trash Mobs check Range"));
            Set(new TVar("Barbarian.GroundStomp.TrashCount", 4, "Use Ground Stomp when this many Trash Mobs in Range"));
            Set(new TVar("Barbarian.Revenge.TrashRange", 9f, "Use Revenge Trash Range"));
            Set(new TVar("Barbarian.Revenge.TrashCount", 1, "Use Revenge Trash Count"));
            Set(new TVar("Barbarian.FuriousCharge.EliteRange", 15f, "Furious Charge Elite Check Range"));
            Set(new TVar("Barbarian.FuriousCharge.EliteCount", 1, "Minimum Furious Charge Elite Count"));
            Set(new TVar("Barbarian.FuriousCharge.TrashRange", 15f, "Furious Charge Trash Check Range"));
            Set(new TVar("Barbarian.FuriousCharge.TrashCount", 3, "Minimum Furious Charge Trash Count"));
            Set(new TVar("Barbarian.FuriousCharge.UseRange", 32f, "Use Range"));
            Set(new TVar("Barbarian.FuriousCharge.MinExtraTargetDistance", 5f, "Extra distance added to target for Furious Charge direction"));
            Set(new TVar("Barbarian.Leap.UseRange", 35f, "Power Use Range"));
            Set(new TVar("Barbarian.Leap.EliteRange", 20f, "Leap Elite Check Range"));
            Set(new TVar("Barbarian.Leap.EliteCount", 1, "Minimum Leap Elite Count"));
            Set(new TVar("Barbarian.Leap.TrashRange", 20f, "Leap Trash Check Range"));
            Set(new TVar("Barbarian.Leap.TrashCount", 1, "Minimum Leap Trash Count"));
            Set(new TVar("Barbarian.Leap.MinExtraDistance", 4f, "Extra distance added to target for Leap direction"));

            Set(new TVar("Barbarian.Rend.MinNonBleedMobCount", 1, "Cast rend when this many mobs surrounding are not bleeding"));
            Set(new TVar("Barbarian.Rend.MinUseIntervalMillseconds", 0, "Minimum Delay between uses"));
            Set(new TVar("Barbarian.Rend.UseRange", 10f, "Power Use Range"));
            Set(new TVar("Barbarian.Rend.MaxRange", 10f, "Maximum Range for targets to be Rended"));
            Set(new TVar("Barbarian.Rend.MinFury", 20, "Minimum Fury"));
            Set(new TVar("Barbarian.Rend.TickDelay", 4, "Rend Pre and Post Tick Delay"));
            Set(new TVar("Barbarian.Rend.SpamBelowHealthPct", 25f, "Always spam rend when below this Percent Health"));

            Set(new TVar("Barbarian.OverPower.MaxRange", 9f, "Maximum Range Overpower is triggered"));
            Set(new TVar("Barbarian.SeismicSlam.CurrentTargetRange", 40f, "Maximum Current Target range"));
            Set(new TVar("Barbarian.SeismicSlam.MinFury", 15, "Minimum Fury for Seismic Slam"));
            Set(new TVar("Barbarian.SeismicSlam.TrashRange", 20f, "Seismic Slam Trash Check Range"));
            Set(new TVar("Barbarian.SeismicSlam.EliteRange", 20f, "Elite Target Range"));
            Set(new TVar("Barbarian.SeismicSlam.UseRange", 40f, "Power Use Range"));
            Set(new TVar("Barbarian.HammerOfTheAncients.UseRange", 10f, "Use Range"));
            Set(new TVar("Barbarian.AncientSpear.MinHealthPct", 0.2, "Minimum Target Health Percent for Ancient Spear"));

            // Misc Spells
            Set(new TVar("SpellDelay.DrinkHealthPotion", 30000, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.Weapon_Melee_Instant", 5, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.Weapon_Ranged_Instant", 5, "Spell Use Delay/Interval, milliseconds"));

            // Barbarian Spells
            Set(new TVar("SpellDelay.Barbarian_Bash", 5, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.Barbarian_Cleave", 5, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.Barbarian_Frenzy", 5, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.Barbarian_HammerOfTheAncients", 150, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.Barbarian_Rend", 1500, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.Barbarian_SeismicSlam", 200, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.Barbarian_Whirlwind", 5, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.Barbarian_GroundStomp", 12200, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.Barbarian_Leap", 10200, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.Barbarian_Sprint", 2800, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.Barbarian_IgnorePain", 30200, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.Barbarian_AncientSpear", 300, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.Barbarian_Revenge", 600, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.Barbarian_FuriousCharge", 500, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.Barbarian_Overpower", 200, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.Barbarian_WeaponThrow", 5, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.Barbarian_ThreateningShout", 10200, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.Barbarian_BattleRage", 118000, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.Barbarian_WarCry", 20500, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.Barbarian_Earthquake", 120500, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.Barbarian_CallOfTheAncients", 120500, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.Barbarian_WrathOfTheBerserker", 120500, "Spell Use Delay/Interval, milliseconds"));

            // Monk skills
            Set(new TVar("SpellDelay.Monk_FistsofThunder", 5, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.Monk_DeadlyReach", 5, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.Monk_CripplingWave", 5, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.Monk_WayOfTheHundredFists", 5, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.Monk_LashingTailKick", 250, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.Monk_TempestRush", 15, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.Monk_WaveOfLight", 750, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.Monk_BlindingFlash", 15200, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.Monk_BreathOfHeaven", 15200, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.Monk_Serenity", 20200, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.Monk_InnerSanctuary", 20200, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.Monk_DashingStrike", 1000, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.Monk_ExplodingPalm", 250, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.Monk_SweepingWind", 1500, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.Monk_CycloneStrike", 900, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.Monk_SevenSidedStrike", 30200, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.Monk_MysticAlly", 30000, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.Monk_MantraOfEvasion", 3300, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.Monk_MantraOfRetribution", 3300, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.Monk_MantraOfHealing", 3300, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.Monk_MantraOfConviction", 3300, "Spell Use Delay/Interval, milliseconds"));

            // Wizard skills
            Set(new TVar("SpellDelay.Wizard_MagicMissile", 5, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.Wizard_ShockPulse", 5, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.Wizard_SpectralBlade", 5, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.Wizard_Electrocute", 5, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.Wizard_RayOfFrost", 5, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.Wizard_ArcaneOrb", 500, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.Wizard_ArcaneTorrent", 5, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.Wizard_Disintegrate", 5, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.Wizard_FrostNova", 9000, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.Wizard_DiamondSkin", 15000, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.Wizard_SlowTime", 16000, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.Wizard_Teleport", 16000, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.Wizard_WaveOfForce", 12000, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.Wizard_EnergyTwister", 5, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.Wizard_Hydra", 12000, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.Wizard_Meteor", 1000, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.Wizard_Blizzard", 4000, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.Wizard_IceArmor", 60000, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.Wizard_StormArmor", 60000, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.Wizard_EnergyArmor", 60000, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.Wizard_MagicWeapon", 60000, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.Wizard_Familiar", 60000, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.Wizard_ExplosiveBlast", 6000, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.Wizard_MirrorImage", 5000, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.Wizard_Archon", 100000, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.Wizard_Archon_ArcaneBlast", 5000, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.Wizard_Archon_ArcaneStrike", 200, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.Wizard_Archon_DisintegrationWave", 5, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.Wizard_Archon_SlowTime", 16000, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.Wizard_Archon_Teleport", 10000, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.Wizard_Archon_Cancel", 1500, "Spell Use Delay/Interval, milliseconds"));

            // Witch Doctor skills
            Set(new TVar("SpellDelay.Witchdoctor_PoisonDart", 5, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.Witchdoctor_CorpseSpider", 5, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.Witchdoctor_PlagueOfToads", 5, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.Witchdoctor_Firebomb", 5, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.Witchdoctor_GraspOfTheDead", 6000, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.Witchdoctor_Firebats", 5, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.Witchdoctor_Haunt", 12000, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.Witchdoctor_Locust_Swarm", 8000, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.Witchdoctor_SummonZombieDog", 25000, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.Witchdoctor_Horrify", 16200, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.Witchdoctor_SpiritWalk", 15200, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.Witchdoctor_Hex", 15200, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.Witchdoctor_SoulHarvest", 15000, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.Witchdoctor_Sacrifice", 1000, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.Witchdoctor_MassConfusion", 45200, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.Witchdoctor_ZombieCharger", 5, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.Witchdoctor_SpiritBarrage", 15000, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.Witchdoctor_AcidCloud", 5, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.Witchdoctor_WallOfZombies", 25200, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.Witchdoctor_Gargantuan", 25000, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.Witchdoctor_BigBadVoodoo", 120000, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.Witchdoctor_FetishArmy", 90000, "Spell Use Delay/Interval, milliseconds"));

            // Demon Hunter skills
            Set(new TVar("SpellDelay.DemonHunter_HungeringArrow", 5, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.DemonHunter_EntanglingShot", 5, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.DemonHunter_BolaShot", 5, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.DemonHunter_Grenades", 5, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.DemonHunter_Impale", 5, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.DemonHunter_RapidFire", 5, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.DemonHunter_Chakram", 5, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.DemonHunter_ElementalArrow", 5, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.DemonHunter_Caltrops", 3000, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.DemonHunter_SmokeScreen", 3000, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.DemonHunter_ShadowPower", 5000, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.DemonHunter_Vault", 400, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.DemonHunter_Preparation", 5000, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.DemonHunter_Companion", 30000, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.DemonHunter_MarkedForDeath", 3000, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.DemonHunter_EvasiveFire", 300, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.DemonHunter_FanOfKnives", 10000, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.DemonHunter_SpikeTrap", 1000, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.DemonHunter_Sentry", 8000, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.DemonHunter_Strafe", 5, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.DemonHunter_Multishot", 5, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.DemonHunter_ClusterArrow", 150, "Spell Use Delay/Interval, milliseconds"));
            Set(new TVar("SpellDelay.DemonHunter_RainOfVengeance", 10000, "Spell Use Delay/Interval, milliseconds"));

            // Demon Hunter
            Set(new TVar("DemonHunter.MinEnergyReserve", 0, "Ignore Pain Emergency Use Minimum Health Percent"));

            // Monk
            Set(new TVar("Monk.Avoidance.Serenity", 0f, "Monk Serenity buff Avoidance health multiplier"));
            Set(new TVar("Monk.MinEnergyReserve", 0, "Ignore Pain Emergency Use Minimum Health Percent"));
            Set(new TVar("Monk.SweepingWind.SpamOnLowHealthPct", 0.50f, "Spam Sweeping Wind for Health Regen when below this Health Percent"));

            // Witch Doctor
            Set(new TVar("WitchDoctor.Avoidance.SpiritWalk", 0f, "WitchDoctor Spirit walk Avoidance health multiplier"));
            Set(new TVar("WitchDoctor.MinEnergyReserve", 0, "Witch Doctor Special Minimum Mana reserve"));
            Set(new TVar("WitchDoctor.SpiritWalk.HealingJourneyHealth", 0.65d, "Percent Health Threshold to use Spirit Walk with Healing Journey"));
            Set(new TVar("WitchDoctor.SpiritWalk.HonoredGuestMana", 0.50d, "Percent Mana Threshold to use Spirit Walk with Honored Guest"));
            Set(new TVar("WitchDoctor.Firebats.MaintainRange", 35f, "Maintain Firebats while any mobs are in this range"));

            // Wizard
            Set(new TVar("Wizard.MinEnergyReserve", 0, "Ignore Pain Emergency Use Minimum Health Percent"));

            // Global
            Set(new TVar("Combat.DefaultTickPreDelay", 1, "Default Combat Power Pre-use Delay (in ticks)"));
            Set(new TVar("Combat.DefaultTickPostDelay", 1, "Default Combat Power Post-use Delay (in ticks)"));

            Set(new TVar("Weight.Globe.MinPlayerHealthPct", 0.90d, "Minimum player health before health globes considered (does not effect emergency health globe)"));
            Set(new TVar("Weight.Globe.MinPartyHealthPct", 0.90d, "Minimum party player health befefore globes considered (party mode only)"));

            // Cache
            Set(new TVar("Cache.PretownRun.MaxDistance", 1500, "Default PreTownrun max distance"));
            Set(new TVar("Cache.TownPortal.KillRange", 60f, "Forced maximum distance for clearing the area before using a Town Portal"));

            Set(new TVar("Cache.HotSpot.MaxDistance", 2500f, "Maximum distance to add team hotspots to cache"));
            Set(new TVar("Cache.HotSpot.MinDistance", 50f, "Minimum distance to add team hotspots to cache"));

            // XmlTags
            Set(new TVar("XmlTag.TrinityTownPortal.DefaultWaitTime", 2500, "Time in Milliseconds to set the default wait time for TrinityTownPortal (may be overriden by Profile tags)"));
            Set(new TVar("XmlTag.TrinityTownPortal.ForceWaitTime", -1, "If not -1, Force set the Time in Milliseconds to clear the area for TrinityTownPortal"));

            batch = false;
        }

        internal static void Save()
        {
            try
            {
                if (!ZetaDia.Service.Platform.IsConnected || !ZetaDia.Service.CurrentHero.IsValid)
                    return;
            }
            catch (Exception ex)
            {
                Logger.Log("Exception saving TVars in checking Service and Hero: {0}", ex);
            }

            var filename = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Settings", ZetaDia.Service.CurrentHero.BattleTagName, "TVars.xml");
            lock (sync)
            {
                try
                {
                    Logger.Log(TrinityLogLevel.Debug, LogCategory.UserInformation, "Saving Variable Setting file");
                    using (Stream stream = File.Open(filename, FileMode.Create))
                    {
                        var knownTypes = new List<Type>()
                        {
                            typeof(int),
                            typeof(string),
                            typeof(float),
                            typeof(double),
                            typeof(bool),
                            typeof(TVar),
                            typeof(DictionaryEntry),
                            typeof(KeyValuePair<string, TVar>),
                            typeof(Dictionary<string,TVar>),
                        };
                        var serializer = new DataContractSerializer(typeof(ObservableDictionary<string, TVar>), "TVars", "", knownTypes);

                        var xmlWriterSettings = new XmlWriterSettings { Indent = true };
                        using (var xmlWriter = XmlWriter.Create(stream, xmlWriterSettings))
                        {
                            serializer.WriteObject(xmlWriter, V.Data);
                        }
                        stream.Close();
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(TrinityLogLevel.Error, LogCategory.UserInformation, "Error while saving Variable Setting file: {0}", ex);
                }
            }
        }

        private static bool loaded = false;
        internal static void Load()
        {
            try
            {
                if (!ZetaDia.Service.Platform.IsConnected || !ZetaDia.Service.CurrentHero.IsValid)
                    return;
            }
            catch (Exception ex)
            {
                Logger.Log("Exception Loading TVars in checking Service and Hero: {0}", ex);
            }

            var filename = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Settings", ZetaDia.Service.CurrentHero.BattleTagName, "TVars.xml");

            lock (sync)
            {
                try
                {
                    batch = true;

                    if (File.Exists(filename))
                    {
                        using (Stream stream = File.Open(filename, FileMode.Open))
                        {
                            var serializer = new DataContractSerializer(typeof(ObservableDictionary<string, TVar>), "TVars", "");

                            var reader = XmlReader.Create(stream);
                            var loadedData = (ObservableDictionary<string, TVar>)serializer.ReadObject(reader);

                            if (loadedData.Count > 0)
                            {
                                //V.Data = loadedData;
                                foreach (KeyValuePair<string, TVar> kvp in loadedData)
                                {
                                    Set(kvp.Value);
                                }
                            }

                            stream.Close();
                            Logger.Log(TrinityLogLevel.Debug, LogCategory.UserInformation, "Variable Setting file loaded");
                        }
                    }
                    else
                    {
                        Save();
                    }
                    loaded = true;
                }
                catch (Exception ex)
                {
                    Logger.Log(TrinityLogLevel.Error, LogCategory.UserInformation, "Error while loading Variable Setting file: {0}", ex);
                }
                finally
                {
                    batch = false;
                }
            }
        }

        /// <summary>
        /// Reload variables every time a game is started
        /// </summary>
        internal static void ValidateLoad()
        {
            if (loaded)
                return;

            if (Data == null)
                Data = new ObservableDictionary<string, TVar>();

            SetDefaults();
            Load();
        }

        /// <summary>
        /// Static Constructor
        /// </summary>
        private static void TVars()
        {
            ValidateLoad();
            Data.CollectionChanged += Data_CollectionChanged;
        }

        /// <summary>
        /// Saves the Data into the XML file each time a variable is modified
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void Data_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (!batch)
                Save();
        }

        /// <summary>
        /// Making this entire thing thread safe
        /// </summary>
        private static object sync = new object();

        /// <summary>
        /// Allow CollectionChanged file save
        /// </summary>
        private static bool batch = false;

        /// <summary>
        /// Contains all of our configuration data
        /// </summary>
        //private static Dictionary<string, TVar> data = new Dictionary<string, TVar>();
        [DataMember(IsRequired = true)]
        internal static ObservableDictionary<string, TVar> Data
        {
            get { return _data; }
            set { _data = value; }
        }

        private static ObservableDictionary<string, TVar> _data = new ObservableDictionary<string, TVar>();

        /// <summary>
        /// Check if we have the given key in our dictionary
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool ContainsKey(string key)
        {
            lock (sync)
            {
                var containsKey = Data.ContainsKey(key);
                if (loaded && !containsKey)
                    Logger.LogDebug("Warning: unknown Trinity Variable requested: {0}", key);
                return containsKey;
            }
        }

        /// <summary>
        /// Returns a variable value as a string
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetString(string key)
        {
            lock (sync)
            {
                if (!ContainsKey(key))
                    return string.Empty;

                try
                {
                    return Convert.ToString(Data[key].Value);
                }
                catch (InvalidCastException)
                {
                    return string.Empty;
                }
            }
        }

        /// <summary>
        /// Returns a variable value as an Integer
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static int GetInt(string key)
        {
            lock (sync)
            {
                if (!ContainsKey(key))
                    return 0;

                try
                {
                    return Convert.ToInt32(Data[key].Value);
                }
                catch (InvalidCastException)
                {
                    return 0;
                }
            }
        }

        /// <summary>
        /// Returns a variable value as a float
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static float GetFloat(string key)
        {
            lock (sync)
            {
                if (!ContainsKey(key))
                    return 0f;

                try
                {
                    return Convert.ToSingle(Data[key].Value);
                }
                catch (InvalidCastException)
                {
                    return 0f;
                }

            }
        }

        /// <summary>
        /// Returns a variable value as a double
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static double GetDouble(string key)
        {
            lock (sync)
            {
                if (!ContainsKey(key))
                    return 0d;

                try
                {
                    return Convert.ToDouble(Data[key].Value);
                }
                catch (InvalidCastException)
                {
                    return 0d;
                }
            }
        }

        /// <summary>
        /// Returns a variable value as a Boolean
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool GetBool(string key)
        {
            if (!ContainsKey(key))
                return false;

            lock (sync)
            {
                try
                {
                    return Convert.ToBoolean(Data[key].Value);
                }
                catch (InvalidCastException)
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Returns a variable value as a string
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string S(string key)
        {
            return GetString(key);
        }

        /// <summary>
        /// Returns a variable value as an Integer
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static int I(string key)
        {
            return GetInt(key);
        }

        /// <summary>
        /// Returns a variable value as a float
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static float F(string key)
        {
            return GetFloat(key);
        }

        /// <summary>
        /// Returns a variable value as a double
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static double D(string key)
        {
            return GetDouble(key);
        }

        /// <summary>
        /// Returns a variable value as a Boolean
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool B(string key)
        {
            return GetBool(key);
        }

        /// <summary>
        /// Gets the type of a given variable
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static Type GetType(string key)
        {
            lock (sync)
            {
                if (ContainsKey(key))
                    return Data[key].Value.GetType();
            }
            return null;
        }

        /// <summary>
        /// Sets a Variable (stores it in the dictionary)
        /// </summary>
        /// <param name="var"></param>
        public static void Set(TVar var)
        {
            try
            {
                lock (sync)
                {
                    if (ContainsKey(var.Name))
                        Data[var.Name] = var;
                    else
                        Data.Add(var.Name, var);
                }
            }
            catch (Exception ex)
            {
                Logger.Log("Exception Setting TVar: {0} {1}", var, ex);
            }

        }

        /// <summary>
        /// Gets a variable value
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static object GetValue(string key)
        {
            lock (sync)
            {
                if (ContainsKey(key))
                    return Data[key].Value;
            }
            return null;
        }

        /// <summary>
        /// Restores the default value of a variable
        /// </summary>
        /// <param name="key"></param>
        public static void SetDefaultValue(string key)
        {
            lock (sync)
            {
                if (ContainsKey(key))
                {
                    TVar v = Data[key];
                    v.Value = v.DefaultValue;
                    Data[key] = v;
                }
            }
        }

        /// <summary>
        /// Resets all variables to default values
        /// </summary>
        public static void ResetAll()
        {
            Data.Clear();
            SetDefaults();
            Save();
            Logger.LogNormal("Reset all Trinity Variables to default.");
        }

        public static void Dump()
        {
            Logger.LogNormal("Found {0} TVars", Data.Count);
            foreach (KeyValuePair<string, TVar> item in Data)
            {
                if (item.Value.DefaultValue == item.Value.Value)
                    continue;
                Logger.LogNormal(item.Value.ToString());
            }
            Logger.LogNormal("\n");
        }
    }
}
