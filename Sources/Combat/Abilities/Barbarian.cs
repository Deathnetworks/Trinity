using Trinity.DbProvider;
using Trinity.Technicals;
using System;
using System.Linq;
using Zeta.Common;
using Zeta.Common.Plugins;
using Zeta.CommonBot;
using Zeta.Internals.Actors;
using Trinity.Combat.Abilities;

namespace Trinity
{
    public partial class Trinity : IPlugin
    {
        private static TrinityPower GetBarbarianPower(bool IsCurrentlyAvoiding, bool UseOOCBuff, bool UseDestructiblePower)
        {

            // Pick the best destructible power available
            if (UseDestructiblePower)
            {
                return GetBarbarianDestroyPower();
            }
            // Barbarians need 56 reserve for special spam like WW
            MinEnergyReserve = 56;
            // Ignore Pain when low on health
            if (!UseOOCBuff && Hotbar.Contains(SNOPower.Barbarian_IgnorePain) && Player.CurrentHealthPct <= 0.45 &&
                SNOPowerUseTimer(SNOPower.Barbarian_IgnorePain, true) && PowerManager.CanCast(SNOPower.Barbarian_IgnorePain))
            {
                return new TrinityPower(SNOPower.Barbarian_IgnorePain, 0f, Vector3.Zero, CurrentWorldDynamicId, -1, 0, 0, WAIT_FOR_ANIM);
            }

            IsWaitingForSpecial = false;

            if (Player.PrimaryResource < MinEnergyReserve)
            {
                if (!UseOOCBuff && !IsCurrentlyAvoiding && Hotbar.Contains(SNOPower.Barbarian_Earthquake) &&
                    ElitesWithinRange[RANGE_25] >= 1 && SNOPowerUseTimer(SNOPower.Barbarian_Earthquake) && !GetHasBuff(SNOPower.Barbarian_Earthquake))
                {
                    Logger.LogNormal("Waiting for Barbarian_Earthquake 1!");
                    IsWaitingForSpecial = true;
                }
                // Earthquake, elites close-range only
                if (!UseOOCBuff && !IsCurrentlyAvoiding && Hotbar.Contains(SNOPower.Barbarian_Earthquake) && !Player.IsIncapacitated &&
                    (ElitesWithinRange[RANGE_15] > 0 || (CurrentTarget.IsBossOrEliteRareUnique && CurrentTarget.RadiusDistance <= 13f)) &&
                    SNOPowerUseTimer(SNOPower.Barbarian_Earthquake, true) && !GetHasBuff(SNOPower.Barbarian_Earthquake) &&
                    PowerManager.CanCast(SNOPower.Barbarian_Earthquake))
                {
                    if (Player.PrimaryResource >= 50)
                        return new TrinityPower(SNOPower.Barbarian_Earthquake, 13f, Vector3.Zero, CurrentWorldDynamicId, -1, 4, 4, WAIT_FOR_ANIM);
                    Logger.LogNormal("Waiting for Barbarian_Earthquake 2!");
                    IsWaitingForSpecial = true;
                }
                if (!UseOOCBuff && !IsCurrentlyAvoiding && Hotbar.Contains(SNOPower.Barbarian_WrathOfTheBerserker) &&
                    ElitesWithinRange[RANGE_25] >= 1 && SNOPowerUseTimer(SNOPower.Barbarian_WrathOfTheBerserker) && !GetHasBuff(SNOPower.Barbarian_WrathOfTheBerserker))
                {
                    Logger.LogNormal("Waiting for Barbarian_WrathOfTheBerserker 1!");
                    IsWaitingForSpecial = true;
                }
                // Berserker special for ignore elites
                if (!UseOOCBuff && !IsCurrentlyAvoiding && Hotbar.Contains(SNOPower.Barbarian_WrathOfTheBerserker) && CombatBase.IgnoringElites &&
                    (TargetUtil.AnyMobsInRange(25, 3) || TargetUtil.AnyMobsInRange(50, 10) || TargetUtil.AnyMobsInRange(Settings.Combat.Misc.TrashPackClusterRadius, Settings.Combat.Misc.TrashPackSize)) &&
                    SNOPowerUseTimer(SNOPower.Barbarian_WrathOfTheBerserker) && !GetHasBuff(SNOPower.Barbarian_WrathOfTheBerserker))
                {
                    Logger.LogNormal("Waiting for Barbarian_WrathOfTheBerserker 2!");
                    IsWaitingForSpecial = true;
                }
                if (!UseOOCBuff && !IsCurrentlyAvoiding && Hotbar.Contains(SNOPower.Barbarian_CallOfTheAncients) &&
                    ElitesWithinRange[RANGE_25] >= 1 && SNOPowerUseTimer(SNOPower.Barbarian_CallOfTheAncients) && !GetHasBuff(SNOPower.Barbarian_CallOfTheAncients))
                {
                    Logger.LogNormal("Waiting for Barbarian_CallOfTheAncients!");
                    IsWaitingForSpecial = true;
                }
            }

            // Wrath of the berserker, elites only (wrath of berserker)
            if (!UseOOCBuff && Hotbar.Contains(SNOPower.Barbarian_WrathOfTheBerserker) &&
                // If using WOTB on all elites, or if we should only use on "hard" affixes
                (!Settings.Combat.Barbarian.WOTBHardOnly || (CombatBase.HardElitesPresent && Settings.Combat.Barbarian.WOTBHardOnly)) &&
                // Not on heart of sin after Cydaea
                CurrentTarget.ActorSNO != 193077 &&
                // Make sure we are allowed to use wrath on goblins, else make sure this isn't a goblin
                (
                 (!Settings.Combat.Barbarian.UseWOTBGoblin || (Settings.Combat.Barbarian.UseWOTBGoblin && CurrentTarget.IsTreasureGoblin)) ||
                // If ignoring elites completely, trigger on 3 trash within 25 yards, or 10 trash in 50 yards
                 (CombatBase.IgnoringElites && (TargetUtil.AnyMobsInRange(25, 3) || TargetUtil.AnyMobsInRange(50, 10)) || !CombatBase.IgnoringElites) ||
                // Otherwise use when Elite target is in 20 yards
                 (TargetUtil.AnyElitesInRange(20, 1) || TargetUtil.IsEliteTargetInRange(20f)) ||
                // Or if our health is low
                 Player.CurrentHealthPct <= 60
                ) &&
                // Don't still have the buff
                !GetHasBuff(SNOPower.Barbarian_WrathOfTheBerserker) && PowerManager.CanCast(SNOPower.Barbarian_WrathOfTheBerserker))
            {
                if (Player.PrimaryResource >= 50)
                {
                    Logger.Log(TrinityLogLevel.Verbose, LogCategory.UserInformation, "Barbarian_WrathOfTheBerserker being used!({0})", CurrentTarget.InternalName);
                    IsWaitingForSpecial = false;
                    return new TrinityPower(SNOPower.Barbarian_WrathOfTheBerserker, 0f, Vector3.Zero, CurrentWorldDynamicId, -1, 1, 1, WAIT_FOR_ANIM);
                }
                else
                {
                    Logger.Log(TrinityLogLevel.Verbose, LogCategory.UserInformation, "Barbarian_WrathOfTheBerserker ready, waiting for fury...");
                    IsWaitingForSpecial = true;
                }
            }
            // Call of the ancients, elites only
            if (!UseOOCBuff && !IsCurrentlyAvoiding && Hotbar.Contains(SNOPower.Barbarian_CallOfTheAncients) && !Player.IsIncapacitated &&
                (ElitesWithinRange[RANGE_25] > 0 || ((CurrentTarget.IsEliteRareUnique || CurrentTarget.IsTreasureGoblin || CurrentTarget.IsBoss) && CurrentTarget.RadiusDistance <= 25f)) &&
                SNOPowerUseTimer(SNOPower.Barbarian_CallOfTheAncients, true) &&
                PowerManager.CanCast(SNOPower.Barbarian_CallOfTheAncients))
            {
                if (Player.PrimaryResource >= 50)
                {
                    IsWaitingForSpecial = false;
                    return new TrinityPower(SNOPower.Barbarian_CallOfTheAncients, 0f, Vector3.Zero, CurrentWorldDynamicId, -1, 4, 4, WAIT_FOR_ANIM);
                }
                else
                {
                    Logger.Log(TrinityLogLevel.Verbose, LogCategory.UserInformation, "Call of the Ancients ready, waiting for fury...");
                    IsWaitingForSpecial = true;
                }
            }
            // Battle rage, for if being followed and before we do sprint
            if (UseOOCBuff && !Player.IsIncapacitated && Hotbar.Contains(SNOPower.Barbarian_BattleRage) &&
                (SNOPowerUseTimer(SNOPower.Barbarian_BattleRage) || !GetHasBuff(SNOPower.Barbarian_BattleRage)) &&
                Player.PrimaryResource >= 20 && PowerManager.CanCast(SNOPower.Barbarian_BattleRage))
            {
                return new TrinityPower(SNOPower.Barbarian_BattleRage, 0f, Vector3.Zero, CurrentWorldDynamicId, -1, 1, 1, WAIT_FOR_ANIM);
            }
            // Special segment for sprint as an out-of-combat only
            if (UseOOCBuff && !DisableOutofCombatSprint &&
                (Settings.Combat.Misc.AllowOOCMovement || GetHasBuff(SNOPower.Barbarian_WrathOfTheBerserker)) &&
                !Player.IsIncapacitated && Hotbar.Contains(SNOPower.Barbarian_Sprint) &&
                !GetHasBuff(SNOPower.Barbarian_Sprint) &&
                Player.PrimaryResource >= 20 && SNOPowerUseTimer(SNOPower.Barbarian_Sprint) && PowerManager.CanCast(SNOPower.Barbarian_Sprint))
            {
                return new TrinityPower(SNOPower.Barbarian_Sprint, 0f, Vector3.Zero, CurrentWorldDynamicId, -1, 0, 0, WAIT_FOR_ANIM);
            }

            // War cry, constantly maintain
            if (!Player.IsIncapacitated && Hotbar.Contains(SNOPower.Barbarian_WarCry) &&
                (Player.PrimaryResource <= 60 || !GetHasBuff(SNOPower.Barbarian_WarCry)) &&
                SNOPowerUseTimer(SNOPower.Barbarian_WarCry, true) && (!GetHasBuff(SNOPower.Barbarian_WarCry) || PowerManager.CanCast(SNOPower.Barbarian_WarCry)))
            {
                return new TrinityPower(SNOPower.Barbarian_WarCry, 0f, Vector3.Zero, CurrentWorldDynamicId, -1, 1, 1, WAIT_FOR_ANIM);
            }

            // Threatening shout
            if (!UseOOCBuff && Hotbar.Contains(SNOPower.Barbarian_ThreateningShout) && !Player.IsIncapacitated &&
                ((TargetUtil.AnyMobsInRange(25, Settings.Combat.Barbarian.MinThreatShoutMobCount)) || TargetUtil.IsEliteTargetInRange(25f)) &&
                (
                  (Hotbar.Contains(SNOPower.Barbarian_Whirlwind) && Player.PrimaryResource <= 10) ||
                  (IsWaitingForSpecial && Player.PrimaryResource <= MinEnergyReserve)
                ) &&
              SNOPowerUseTimer(SNOPower.Barbarian_ThreateningShout, true) && PowerManager.CanCast(SNOPower.Barbarian_ThreateningShout))
            {
                return new TrinityPower(SNOPower.Barbarian_ThreateningShout, 0f, Vector3.Zero, CurrentWorldDynamicId, -1, 1, 1, WAIT_FOR_ANIM);
            }
            // Threatening shout out-of-combat
            if (UseOOCBuff && Settings.Combat.Barbarian.ThreatShoutOOC && Hotbar.Contains(SNOPower.Barbarian_ThreateningShout) &&
                !Player.IsIncapacitated && Player.PrimaryResource < 25 &&
                SNOPowerUseTimer(SNOPower.Barbarian_ThreateningShout, true) && PowerManager.CanCast(SNOPower.Barbarian_ThreateningShout))
            {
                return new TrinityPower(SNOPower.Barbarian_ThreateningShout, 0f, Vector3.Zero, CurrentWorldDynamicId, -1, 1, 1, WAIT_FOR_ANIM);
            }
            // Ground Stomp
            if (!UseOOCBuff && Hotbar.Contains(SNOPower.Barbarian_GroundStomp) && !Player.IsIncapacitated &&
                (ElitesWithinRange[RANGE_15] > 0 || AnythingWithinRange[RANGE_15] > 4 || Player.CurrentHealthPct <= 0.7) &&
                SNOPowerUseTimer(SNOPower.Barbarian_GroundStomp, true) &&
                PowerManager.CanCast(SNOPower.Barbarian_GroundStomp))
            {
                return new TrinityPower(SNOPower.Barbarian_GroundStomp, 16f, Vector3.Zero, CurrentWorldDynamicId, -1, 1, 2, WAIT_FOR_ANIM);
            }
            // Revenge used off-cooldown
            if (!UseOOCBuff && Hotbar.Contains(SNOPower.Barbarian_Revenge) && !Player.IsIncapacitated &&
                // Don't use revenge on goblins, too slow!
                (!CurrentTarget.IsTreasureGoblin || AnythingWithinRange[RANGE_12] >= 5) &&
                // Doesn't need CURRENT target to be in range, just needs ANYTHING to be within 9 foot, since it's an AOE!
                (AnythingWithinRange[RANGE_6] > 0 || CurrentTarget.RadiusDistance <= 6f) &&
                SNOPowerUseTimer(SNOPower.Barbarian_Revenge) && PowerManager.CanCast(SNOPower.Barbarian_Revenge))
            {
                // Note - we have LONGER animation times for whirlwind-users
                // Since whirlwind seems to interrupt rend so easily
                int iPreDelay = 0;
                int iPostDelay = 0;
                if (Hotbar.Contains(SNOPower.Barbarian_Whirlwind))
                {
                    if (LastPowerUsed == SNOPower.Barbarian_Whirlwind)
                    {
                        iPreDelay = 3;
                        iPostDelay = 3;
                    }
                }
                return new TrinityPower(SNOPower.Barbarian_Revenge, 0f, Player.Position, CurrentWorldDynamicId, -1, iPreDelay, iPostDelay, WAIT_FOR_ANIM);
            }
            // Furious charge
            if (!UseOOCBuff && Hotbar.Contains(SNOPower.Barbarian_FuriousCharge) &&
                (ElitesWithinRange[RANGE_12] > 3 &&
                SNOPowerUseTimer(SNOPower.Barbarian_FuriousCharge) &&
                PowerManager.CanCast(SNOPower.Barbarian_FuriousCharge)))
            {
                float fExtraDistance;
                if (CurrentTarget.CentreDistance <= 25)
                    fExtraDistance = 30;
                else
                    fExtraDistance = (25 - CurrentTarget.CentreDistance);
                if (fExtraDistance < 5f)
                    fExtraDistance = 5f;
                Vector3 vNewTarget = MathEx.CalculatePointFrom(CurrentTarget.Position, Player.Position, CurrentTarget.CentreDistance + fExtraDistance);
                return new TrinityPower(SNOPower.Barbarian_FuriousCharge, 32f, vNewTarget, CurrentWorldDynamicId, -1, 1, 2, WAIT_FOR_ANIM);
            }
            // Leap used when off-cooldown, or when out-of-range
            if (!UseOOCBuff && Hotbar.Contains(SNOPower.Barbarian_Leap) && !Player.IsIncapacitated &&
                (AnythingWithinRange[RANGE_20] > 1 || ElitesWithinRange[RANGE_20] > 0) && SNOPowerUseTimer(SNOPower.Barbarian_Leap, true) &&
                PowerManager.CanCast(SNOPower.Barbarian_Leap))
            {
                // For close-by monsters, try to leap a little further than their centre-point
                float fExtraDistance = CurrentTarget.Radius;
                if (fExtraDistance <= 4f)
                    fExtraDistance = 4f;
                if (CurrentTarget.CentreDistance + fExtraDistance > 35f)
                    fExtraDistance = 35 - CurrentTarget.CentreDistance;
                Vector3 vNewTarget = MathEx.CalculatePointFrom(CurrentTarget.Position, Player.Position, CurrentTarget.CentreDistance + fExtraDistance);
                return new TrinityPower(SNOPower.Barbarian_Leap, 35f, vNewTarget, CurrentWorldDynamicId, -1, 2, 2, WAIT_FOR_ANIM);
            }


            // Rend
            if (!UseOOCBuff && !Player.IsIncapacitated && Hotbar.Contains(SNOPower.Barbarian_Rend) &&
                TargetUtil.AnyMobsInRange(9) && !CurrentTarget.IsTreasureGoblin &&
                ((!IsWaitingForSpecial && Player.PrimaryResource >= 20) || (IsWaitingForSpecial && Player.PrimaryResource > MinEnergyReserve)) &&
                (SNOPowerUseTimer(SNOPower.Barbarian_Rend) && (NonRendedTargets_9 > 2 || !CurrentTarget.HasDotDPS)) &&
                (TimeSinceUse(SNOPower.Barbarian_Rend) > 1500 || TargetUtil.AnyMobsInRange(10f, 6)) && LastPowerUsed != SNOPower.Barbarian_Rend
                )
            {
                // Note - we have LONGER animation times for whirlwind-users
                // Since whirlwind seems to interrupt rend so easily
                int rendPreDelay = 0;
                int rendPostDelay = 1;
                if (Hotbar.Contains(SNOPower.Barbarian_Whirlwind) && (LastPowerUsed == SNOPower.Barbarian_Whirlwind || LastPowerUsed == SNOPower.None))
                {
                    rendPreDelay = 2;
                    rendPostDelay = 2;
                }
                return new TrinityPower(SNOPower.Barbarian_Rend, 0f, Player.Position, CurrentWorldDynamicId, -1, rendPreDelay, rendPostDelay, WAIT_FOR_ANIM);
            }

            // Overpower used off-cooldown
            if (!UseOOCBuff && Hotbar.Contains(SNOPower.Barbarian_Overpower) && !Player.IsIncapacitated &&
                (CurrentTarget.RadiusDistance <= 6f ||
                    (
                        AnythingWithinRange[RANGE_6] >= 1 &&
                        (CurrentTarget.IsEliteRareUnique || CurrentTarget.IsMinion || CurrentTarget.IsBoss || GetHasBuff(SNOPower.Barbarian_WrathOfTheBerserker) ||
                        (CurrentTarget.IsTreasureGoblin && CurrentTarget.CentreDistance <= 6f) || Hotbar.Contains(SNOPower.Barbarian_SeismicSlam))
                    )
                ) &&
                SNOPowerUseTimer(SNOPower.Barbarian_Overpower) && PowerManager.CanCast(SNOPower.Barbarian_Overpower))
            {
                int iPreDelay = 0;
                int iPostDelay = 0;
                return new TrinityPower(SNOPower.Barbarian_Overpower, 0f, Player.Position, CurrentWorldDynamicId, -1, iPreDelay, iPostDelay, WAIT_FOR_ANIM);
            }
            // Seismic slam enemies within close range
            if (!UseOOCBuff && !IsWaitingForSpecial && Hotbar.Contains(SNOPower.Barbarian_SeismicSlam) && !Player.IsIncapacitated &&
                (!Hotbar.Contains(SNOPower.Barbarian_BattleRage) || (Hotbar.Contains(SNOPower.Barbarian_BattleRage) && GetHasBuff(SNOPower.Barbarian_BattleRage))) &&
                Player.PrimaryResource >= 15 && CurrentTarget.CentreDistance <= 40f && (AnythingWithinRange[RANGE_50] > 1 ||
                (AnythingWithinRange[RANGE_50] > 0 && Player.PrimaryResourcePct >= 0.85 && CurrentTarget.HitPointsPct >= 0.30) ||
                (CurrentTarget.IsBoss || CurrentTarget.IsEliteRareUnique || (CurrentTarget.IsTreasureGoblin && CurrentTarget.CentreDistance <= 20f))))
            {
                return new TrinityPower(SNOPower.Barbarian_SeismicSlam, 40f, Vector3.Zero, -1, CurrentTarget.ACDGuid, 2, 2, WAIT_FOR_ANIM);
            }
            // Ancient spear 
            if (!UseOOCBuff && !IsCurrentlyAvoiding && Hotbar.Contains(SNOPower.Barbarian_AncientSpear) &&
                SNOPowerUseTimer(SNOPower.Barbarian_AncientSpear) && PowerManager.CanCast(SNOPower.Barbarian_AncientSpear) &&
                CurrentTarget.HitPointsPct >= 0.20)
            {
                // For close-by monsters, try to leap a little further than their centre-point
                float fExtraDistance = CurrentTarget.Radius;
                if (fExtraDistance <= 4f)
                    fExtraDistance = 30f;
                if (CurrentTarget.CentreDistance + fExtraDistance > 60f)
                    fExtraDistance = 60 - CurrentTarget.CentreDistance;
                if (fExtraDistance < 30)
                    fExtraDistance = 30f;
                Vector3 vNewTarget = MathEx.CalculatePointFrom(CurrentTarget.Position, Player.Position, CurrentTarget.CentreDistance + fExtraDistance);
                return new TrinityPower(SNOPower.Barbarian_AncientSpear, 55f, vNewTarget, CurrentWorldDynamicId, -1, 2, 2, WAIT_FOR_ANIM);
            }
            // Sprint buff, if same suitable targets as elites, keep maintained for WW users
            if (!UseOOCBuff && !DisableOutofCombatSprint && Hotbar.Contains(SNOPower.Barbarian_Sprint) && !Player.IsIncapacitated &&
                // Let's check if is not spaming too much
                TimeSinceUse(SNOPower.Barbarian_Sprint) >= 200 &&
                // Fury Dump Options for sprint: use at max energy constantly, or on a timer
                (
                    (Settings.Combat.Barbarian.FuryDumpWOTB && Player.PrimaryResourcePct >= 0.95 && GetHasBuff(SNOPower.Barbarian_WrathOfTheBerserker)) ||
                    (Settings.Combat.Barbarian.FuryDumpAlways && Player.PrimaryResourcePct >= 0.95) ||
                    ((SNOPowerUseTimer(SNOPower.Barbarian_Sprint) && !GetHasBuff(SNOPower.Barbarian_Sprint)) &&
                // Always keep up if we are whirlwinding, if the target is a goblin, or if we are 16 feet away from the target
                    (Hotbar.Contains(SNOPower.Barbarian_Whirlwind) || CurrentTarget.IsTreasureGoblin || (CurrentTarget.CentreDistance >= 16f && Player.PrimaryResource >= 40)))
                ) &&
                // If they have battle-rage, make sure it's up
                (!Hotbar.Contains(SNOPower.Barbarian_BattleRage) || (Hotbar.Contains(SNOPower.Barbarian_BattleRage) && GetHasBuff(SNOPower.Barbarian_BattleRage))) &&
                // Check for minimum energy
                Player.PrimaryResource >= 20)
            {
                return new TrinityPower(SNOPower.Barbarian_Sprint, 0f, Vector3.Zero, CurrentWorldDynamicId, -1, 0, 0, WAIT_FOR_ANIM);
            }

            //skillDict.Add("Frenzy", SNOPower.Barbarian_Frenzy);
            //runeDict.Add("Sidearm", 1);
            //runeDict.Add("Triumph", 4);
            //runeDict.Add("Vanguard", 2);
            //runeDict.Add("Smite", 3);
            //runeDict.Add("Maniac", 0);

            bool hasManiacRune = HotbarSkills.AssignedSkills.Any(s => s.Power == SNOPower.Barbarian_Frenzy && s.RuneIndex == 0);

            // Frenzy to 5 stacks
            if (!UseOOCBuff && !IsCurrentlyAvoiding && !Player.IsRooted && Hotbar.Contains(SNOPower.Barbarian_Frenzy) &&
                !TargetUtil.AnyMobsInRange(15f, 3) && GetBuffStacks(SNOPower.Barbarian_Frenzy) < 5)
            {
                return new TrinityPower(SNOPower.Barbarian_Frenzy, 10f, Vector3.Zero, -1, CurrentTarget.ACDGuid, 0, 0, NO_WAIT_ANIM);
            }

            // Whirlwind spam as long as necessary pre-buffs are up
            if (!UseOOCBuff && !IsCurrentlyAvoiding && Hotbar.Contains(SNOPower.Barbarian_Whirlwind) && !Player.IsIncapacitated && !Player.IsRooted &&
                (!IsWaitingForSpecial || (IsWaitingForSpecial && Player.PrimaryResource > MinEnergyReserve)) &&
                //(!IsWaitingForSpecial || (IsWaitingForSpecial && !(TargetUtil.AnyMobsInRange(3, 15) || ForceCloseRangeTarget))) && // make sure we're not surrounded if waiting for special
                // Don't WW against goblins, units in the special SNO list
                (!Settings.Combat.Barbarian.SelectiveWhirlwind || (Settings.Combat.Barbarian.SelectiveWhirlwind && bAnyNonWWIgnoreMobsInRange && !DataDictionary.WhirlwindIgnoreSNOIds.Contains(CurrentTarget.ActorSNO))) &&
                // Only if within 15 foot of main target
                ((CurrentTarget.RadiusDistance <= 25f || AnythingWithinRange[RANGE_25] >= 1)) &&
                (AnythingWithinRange[RANGE_50] >= 2 || CurrentTarget.HitPointsPct >= 0.30 || CurrentTarget.IsBossOrEliteRareUnique || Player.CurrentHealthPct <= 0.60) &&
                // Check for energy reservation amounts
                //((playerStatus.dCurrentEnergy >= 20 && !playerStatus.bWaitingForReserveEnergy) || playerStatus.dCurrentEnergy >= iWaitingReservedAmount) &&
                Player.PrimaryResource >= 10 &&
                // If they have battle-rage, make sure it's up
                (!Hotbar.Contains(SNOPower.Barbarian_BattleRage) || (Hotbar.Contains(SNOPower.Barbarian_BattleRage) && GetHasBuff(SNOPower.Barbarian_BattleRage))))
            {
                bool shouldGetNewZigZag =
                    (DateTime.Now.Subtract(LastChangedZigZag).TotalMilliseconds >= 1200 ||
                    CurrentTarget.ACDGuid != LastZigZagUnitAcdGuid ||
                    CombatBase.ZigZagPosition.Distance2D(Player.Position) <= 5f);

                if (shouldGetNewZigZag)
                {
                    var wwdist = 15f;

                    CombatBase.ZigZagPosition = TargetUtil.GetZigZagTarget(CurrentTarget.Position, wwdist);

                    //LastPowerUsed = SNOPower.None;
                    LastZigZagUnitAcdGuid = CurrentTarget.ACDGuid;
                    LastChangedZigZag = DateTime.Now;
                }
                return new TrinityPower(SNOPower.Barbarian_Whirlwind, 10f, CombatBase.ZigZagPosition, CurrentWorldDynamicId, -1, 0, 1, NO_WAIT_ANIM);
            }
            // Battle rage, constantly maintain
            if (!UseOOCBuff && Hotbar.Contains(SNOPower.Barbarian_BattleRage) && !Player.IsIncapacitated &&
                // Fury Dump Options for battle rage IF they don't have sprint 
                (
                 (Settings.Combat.Barbarian.FuryDumpWOTB && Player.PrimaryResourcePct >= 0.99 && GetHasBuff(SNOPower.Barbarian_WrathOfTheBerserker)) ||
                 (Settings.Combat.Barbarian.FuryDumpAlways && Player.PrimaryResourcePct >= 0.99) || !GetHasBuff(SNOPower.Barbarian_BattleRage)
                ) &&
                Player.PrimaryResource >= 20 && PowerManager.CanCast(SNOPower.Barbarian_BattleRage))
            {
                return new TrinityPower(SNOPower.Barbarian_BattleRage, 0f, Vector3.Zero, CurrentWorldDynamicId, -1, 0, 0, WAIT_FOR_ANIM);
            }
            // Hammer of the ancients spam-attacks - never use if waiting for special
            if (!UseOOCBuff && !IsCurrentlyAvoiding && !Player.IsIncapacitated && !IsWaitingForSpecial && Hotbar.Contains(SNOPower.Barbarian_HammerOfTheAncients) &&
                Player.PrimaryResource >= 20)
            {
                //return new TrinityPower(SNOPower.Barbarian_HammerOfTheAncients, 12f, Vector3.Zero, -1, CurrentTarget.ACDGuid, 2, 2, USE_SLOWLY);
                return new TrinityPower(SNOPower.Barbarian_HammerOfTheAncients, 18f, CurrentTarget.Position, CurrentWorldDynamicId, -1, 2, 2, WAIT_FOR_ANIM);
            }
            // Weapon throw
            if (!UseOOCBuff && !IsCurrentlyAvoiding && Hotbar.Contains(SNOPower.Barbarian_WeaponThrow)
                && (Player.PrimaryResource >= 10 && (CurrentTarget.RadiusDistance >= 5f || BarbHasNoPrimary())))
            {
                return new TrinityPower(SNOPower.Barbarian_WeaponThrow, 80f, Vector3.Zero, -1, CurrentTarget.ACDGuid, 0, 0, NO_WAIT_ANIM);
            }
            // Frenzy rapid-attacks
            if (!UseOOCBuff && !IsCurrentlyAvoiding && Hotbar.Contains(SNOPower.Barbarian_Frenzy))
            {
                return new TrinityPower(SNOPower.Barbarian_Frenzy, 10f, Vector3.Zero, -1, CurrentTarget.ACDGuid, 0, 0, NO_WAIT_ANIM);
            }
            // Bash fast-attacks
            if (!UseOOCBuff && !IsCurrentlyAvoiding && Hotbar.Contains(SNOPower.Barbarian_Bash))
            {
                return new TrinityPower(SNOPower.Barbarian_Bash, 10f, Vector3.Zero, -1, CurrentTarget.ACDGuid, 0, 1, WAIT_FOR_ANIM);
            }
            // Cleave fast-attacks
            if (!UseOOCBuff && !IsCurrentlyAvoiding && Hotbar.Contains(SNOPower.Barbarian_Cleave))
            {
                return new TrinityPower(SNOPower.Barbarian_Cleave, 10f, Vector3.Zero, -1, CurrentTarget.ACDGuid, 0, 2, WAIT_FOR_ANIM);
            }

            // Default attacks
            return CombatBase.DefaultPower;
        }

        private static bool BarbHasNoPrimary()
        {
            return !(Hotbar.Contains(SNOPower.Barbarian_Frenzy) ||
                Hotbar.Contains(SNOPower.Barbarian_Bash) ||
                Hotbar.Contains(SNOPower.Barbarian_Cleave));
        }

        private static TrinityPower GetBarbarianDestroyPower()
        {
            if (Hotbar.Contains(SNOPower.Barbarian_Whirlwind) && Player.PrimaryResource > MinEnergyReserve)
                return new TrinityPower(SNOPower.Barbarian_Whirlwind, 10f, CombatBase.ZigZagPosition, CurrentWorldDynamicId, -1, 0, 0, WAIT_FOR_ANIM);
            if (Hotbar.Contains(SNOPower.Barbarian_Frenzy))
                return new TrinityPower(SNOPower.Barbarian_Frenzy, 10f, Vector3.Zero, -1, -1, 0, 0, WAIT_FOR_ANIM);
            if (Hotbar.Contains(SNOPower.Barbarian_Bash))
                return new TrinityPower(SNOPower.Barbarian_Bash, 6f, Vector3.Zero, -1, -1, 0, 0, WAIT_FOR_ANIM);
            if (Hotbar.Contains(SNOPower.Barbarian_Cleave))
                return new TrinityPower(SNOPower.Barbarian_Cleave, 6f, Vector3.Zero, -1, -1, 0, 0, WAIT_FOR_ANIM);
            if (Hotbar.Contains(SNOPower.Barbarian_Rend) && Player.PrimaryResourcePct >= 0.65)
                return new TrinityPower(SNOPower.Barbarian_Rend, 10f, Vector3.Zero, -1, -1, 0, 0, WAIT_FOR_ANIM);
            if (Hotbar.Contains(SNOPower.Barbarian_WeaponThrow) && Player.PrimaryResource >= 20)
                return new TrinityPower(SNOPower.Barbarian_WeaponThrow, 15f, Vector3.Zero, -1, -1, 0, 0, WAIT_FOR_ANIM);
            return CombatBase.DefaultPower;
        }

    }
}
