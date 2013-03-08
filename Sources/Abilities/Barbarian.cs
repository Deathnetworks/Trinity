using GilesTrinity.DbProvider;
using GilesTrinity.Technicals;
using System;
using System.Linq;
using Zeta.Common;
using Zeta.Common.Plugins;
using Zeta.CommonBot;
using Zeta.Internals.Actors;
namespace GilesTrinity
{
    public partial class GilesTrinity : IPlugin
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
            if (!UseOOCBuff && Hotbar.Contains(SNOPower.Barbarian_IgnorePain) && PlayerStatus.CurrentHealthPct <= 0.45 &&
                GilesUseTimer(SNOPower.Barbarian_IgnorePain, true) && PowerManager.CanCast(SNOPower.Barbarian_IgnorePain))
            {
                return new TrinityPower(SNOPower.Barbarian_IgnorePain, 0f, vNullLocation, CurrentWorldDynamicId, -1, 0, 0, USE_SLOWLY);
            }
            // Flag up a variable to see if we should reserve 50 fury for special abilities
            IsWaitingForSpecial = false;
            if (!UseOOCBuff && !IsCurrentlyAvoiding && Hotbar.Contains(SNOPower.Barbarian_Earthquake) &&
                ElitesWithinRange[RANGE_25] >= 1 && GilesUseTimer(SNOPower.Barbarian_Earthquake))
            {
                IsWaitingForSpecial = true;
            }
            if (!UseOOCBuff && !IsCurrentlyAvoiding && Hotbar.Contains(SNOPower.Barbarian_WrathOfTheBerserker) &&
                ElitesWithinRange[RANGE_25] >= 1 && GilesUseTimer(SNOPower.Barbarian_WrathOfTheBerserker))
            {
                IsWaitingForSpecial = true;
            }
            if (!UseOOCBuff && !IsCurrentlyAvoiding && Hotbar.Contains(SNOPower.Barbarian_CallOfTheAncients) &&
                ElitesWithinRange[RANGE_25] >= 1 && GilesUseTimer(SNOPower.Barbarian_CallOfTheAncients))
            {
                IsWaitingForSpecial = true;
            }
            // Earthquake, elites close-range only
            if (!UseOOCBuff && !IsCurrentlyAvoiding && Hotbar.Contains(SNOPower.Barbarian_Earthquake) && !PlayerStatus.IsIncapacitated &&
                (ElitesWithinRange[RANGE_15] > 0 || (CurrentTarget.IsBossOrEliteRareUnique && CurrentTarget.RadiusDistance <= 13f)) &&
                GilesUseTimer(SNOPower.Barbarian_Earthquake, true) &&
                PowerManager.CanCast(SNOPower.Barbarian_Earthquake))
            {
                if (PlayerStatus.PrimaryResource >= 50)
                    return new TrinityPower(SNOPower.Barbarian_Earthquake, 13f, vNullLocation, CurrentWorldDynamicId, -1, 4, 4, USE_SLOWLY);
                IsWaitingForSpecial = true;
            }
            // Wrath of the berserker, elites only (wrath of berserker)
            if (!UseOOCBuff && Hotbar.Contains(SNOPower.Barbarian_WrathOfTheBerserker) &&
                // If using WOTB on all elites, or if we should only use on "hard" affixes
                (!Settings.Combat.Barbarian.WOTBHardOnly || (bUseBerserker && Settings.Combat.Barbarian.WOTBHardOnly)) &&
                // Not on heart of sin after Cydaea
                CurrentTarget.ActorSNO != 193077 &&
                // Make sure we are allowed to use wrath on goblins, else make sure this isn't a goblin
                (Settings.Combat.Barbarian.UseWOTBGoblin || !CurrentTarget.IsTreasureGoblin || ElitesWithinRange[RANGE_15] >= 1) &&
                // If on a boss, only when injured
                ((CurrentTarget.IsBoss && CurrentTarget.HitPointsPct <= 0.99 && !Hotbar.Contains(SNOPower.Barbarian_Whirlwind)) ||
                // If *NOT* on a boss, and definitely no boss in range, then judge based on any elites at all within 30 feet
                 ((!CurrentTarget.IsBoss || Hotbar.Contains(SNOPower.Barbarian_Whirlwind)) &&
                   (!bAnyBossesInRange || Hotbar.Contains(SNOPower.Barbarian_Whirlwind)) &&
                   ((ElitesWithinRange[RANGE_20] >= 1 || CurrentTarget.IsEliteRareUnique) && (CurrentTarget.HitPointsPct >= 0.30 || PlayerStatus.CurrentHealthPct <= 0.60))
                 )) &&
                //Do not activate too far from targets (for bosses / uber elites)
                CurrentTarget.CentreDistance <= 35f &&
                // Don't still have the buff
                !GetHasBuff(SNOPower.Barbarian_WrathOfTheBerserker) && PowerManager.CanCast(SNOPower.Barbarian_WrathOfTheBerserker))
            {
                if (PlayerStatus.PrimaryResource >= 50)
                {
                    DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.UserInformation, "Berserk being used!({0})", CurrentTarget.InternalName);
                    bUseBerserker = false;
                    return new TrinityPower(SNOPower.Barbarian_WrathOfTheBerserker, 0f, vNullLocation, CurrentWorldDynamicId, -1, 1, 1, USE_SLOWLY);
                }
                else
                {
                    DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.UserInformation, "Berserk ready, waiting for fury...");
                    IsWaitingForSpecial = true;
                }
            }
            // Call of the ancients, elites only
            if (!UseOOCBuff && !IsCurrentlyAvoiding && Hotbar.Contains(SNOPower.Barbarian_CallOfTheAncients) && !PlayerStatus.IsIncapacitated &&
                (ElitesWithinRange[RANGE_25] > 0 || ((CurrentTarget.IsEliteRareUnique || CurrentTarget.IsTreasureGoblin || CurrentTarget.IsBoss) && CurrentTarget.RadiusDistance <= 25f)) &&
                GilesUseTimer(SNOPower.Barbarian_CallOfTheAncients, true) &&
                PowerManager.CanCast(SNOPower.Barbarian_CallOfTheAncients))
            {
                if (PlayerStatus.PrimaryResource >= 50)
                    return new TrinityPower(SNOPower.Barbarian_CallOfTheAncients, 0f, vNullLocation, CurrentWorldDynamicId, -1, 4, 4, USE_SLOWLY);
                IsWaitingForSpecial = true;
            }
            // Battle rage, for if being followed and before we do sprint
            if (UseOOCBuff && !PlayerStatus.IsIncapacitated && Hotbar.Contains(SNOPower.Barbarian_BattleRage) &&
                (GilesUseTimer(SNOPower.Barbarian_BattleRage) || !GetHasBuff(SNOPower.Barbarian_BattleRage)) &&
                PlayerStatus.PrimaryResource >= 20 && PowerManager.CanCast(SNOPower.Barbarian_BattleRage))
            {
                return new TrinityPower(SNOPower.Barbarian_BattleRage, 0f, vNullLocation, CurrentWorldDynamicId, -1, 1, 1, USE_SLOWLY);
            }
            // Special segment for sprint as an out-of-combat only
            if (UseOOCBuff && !bDontSpamOutofCombat &&
                (Settings.Combat.Misc.AllowOOCMovement || GetHasBuff(SNOPower.Barbarian_WrathOfTheBerserker)) &&
                !PlayerStatus.IsIncapacitated && Hotbar.Contains(SNOPower.Barbarian_Sprint) &&
                !GetHasBuff(SNOPower.Barbarian_Sprint) &&
                PlayerStatus.PrimaryResource >= 20 && GilesUseTimer(SNOPower.Barbarian_Sprint) && PowerManager.CanCast(SNOPower.Barbarian_Sprint))
            {
                return new TrinityPower(SNOPower.Barbarian_Sprint, 0f, vNullLocation, CurrentWorldDynamicId, -1, 0, 0, USE_SLOWLY);
            }
            // War cry, constantly maintain
            if (!PlayerStatus.IsIncapacitated && Hotbar.Contains(SNOPower.Barbarian_WarCry) &&
                (PlayerStatus.PrimaryResource <= 60 || !GetHasBuff(SNOPower.Barbarian_WarCry)) &&
                GilesUseTimer(SNOPower.Barbarian_WarCry, true) && (!GetHasBuff(SNOPower.Barbarian_WarCry) || PowerManager.CanCast(SNOPower.Barbarian_WarCry)))
            {
                return new TrinityPower(SNOPower.Barbarian_WarCry, 0f, vNullLocation, CurrentWorldDynamicId, -1, 1, 1, USE_SLOWLY);
            }
            // Threatening shout
            if (!UseOOCBuff && Hotbar.Contains(SNOPower.Barbarian_ThreateningShout) && !PlayerStatus.IsIncapacitated &&
              (
                  ElitesWithinRange[RANGE_20] >= 2 || (CurrentTarget.IsBoss && CurrentTarget.RadiusDistance <= 20) ||
                  (AnythingWithinRange[RANGE_20] >= 3 && !bAnyBossesInRange && (ElitesWithinRange[RANGE_50] == 0 || Hotbar.Contains(SNOPower.Barbarian_SeismicSlam))) ||
                  PlayerStatus.CurrentHealthPct <= 0.75 || (Hotbar.Contains(SNOPower.Barbarian_Whirlwind) && PlayerStatus.PrimaryResource <= 10) ||
                  (IsWaitingForSpecial && PlayerStatus.PrimaryResource <= 50)
                  ) &&
              GilesUseTimer(SNOPower.Barbarian_ThreateningShout, true) && PowerManager.CanCast(SNOPower.Barbarian_ThreateningShout))
            {
                return new TrinityPower(SNOPower.Barbarian_ThreateningShout, 0f, vNullLocation, CurrentWorldDynamicId, -1, 1, 1, USE_SLOWLY);
            }
            // Threatening shout out-of-combat: helps battle rage and sprint (5+15=20)
            if (UseOOCBuff && Hotbar.Contains(SNOPower.Barbarian_ThreateningShout) &&
                (Hotbar.Contains(SNOPower.Barbarian_Sprint) || Hotbar.Contains(SNOPower.Barbarian_BattleRage)) &&
                !PlayerStatus.IsIncapacitated && PlayerStatus.PrimaryResource >= 5 && PlayerStatus.PrimaryResource < 20 &&
                GilesUseTimer(SNOPower.Barbarian_ThreateningShout, true) && PowerManager.CanCast(SNOPower.Barbarian_ThreateningShout))
            {
                return new TrinityPower(SNOPower.Barbarian_ThreateningShout, 0f, vNullLocation, CurrentWorldDynamicId, -1, 1, 1, USE_SLOWLY);
            }
            // Ground Stomp
            if (!UseOOCBuff && Hotbar.Contains(SNOPower.Barbarian_GroundStomp) && !PlayerStatus.IsIncapacitated &&
                (ElitesWithinRange[RANGE_15] > 0 || AnythingWithinRange[RANGE_15] > 4 || PlayerStatus.CurrentHealthPct <= 0.7) &&
                GilesUseTimer(SNOPower.Barbarian_GroundStomp, true) &&
                PowerManager.CanCast(SNOPower.Barbarian_GroundStomp))
            {
                return new TrinityPower(SNOPower.Barbarian_GroundStomp, 16f, vNullLocation, CurrentWorldDynamicId, -1, 1, 2, USE_SLOWLY);
            }
            // Revenge used off-cooldown
            if (!UseOOCBuff && Hotbar.Contains(SNOPower.Barbarian_Revenge) && !PlayerStatus.IsIncapacitated &&
                // Don't use revenge on goblins, too slow!
                (!CurrentTarget.IsTreasureGoblin || AnythingWithinRange[RANGE_12] >= 5) &&
                // Doesn't need CURRENT target to be in range, just needs ANYTHING to be within 9 foot, since it's an AOE!
                (AnythingWithinRange[RANGE_6] > 0 || CurrentTarget.RadiusDistance <= 6f) &&
                GilesUseTimer(SNOPower.Barbarian_Revenge) && PowerManager.CanCast(SNOPower.Barbarian_Revenge))
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
                return new TrinityPower(SNOPower.Barbarian_Revenge, 0f, PlayerStatus.CurrentPosition, CurrentWorldDynamicId, -1, iPreDelay, iPostDelay, USE_SLOWLY);
            }
            // Furious charge
            if (!UseOOCBuff && Hotbar.Contains(SNOPower.Barbarian_FuriousCharge) &&
                (ElitesWithinRange[RANGE_12] > 3 &&
                GilesUseTimer(SNOPower.Barbarian_FuriousCharge) &&
                PowerManager.CanCast(SNOPower.Barbarian_FuriousCharge)))
            {
                float fExtraDistance;
                if (CurrentTarget.CentreDistance <= 25)
                    fExtraDistance = 30;
                else
                    fExtraDistance = (25 - CurrentTarget.CentreDistance);
                if (fExtraDistance < 5f)
                    fExtraDistance = 5f;
                Vector3 vNewTarget = MathEx.CalculatePointFrom(CurrentTarget.Position, PlayerStatus.CurrentPosition, CurrentTarget.CentreDistance + fExtraDistance);
                return new TrinityPower(SNOPower.Barbarian_FuriousCharge, 32f, vNewTarget, CurrentWorldDynamicId, -1, 1, 2, USE_SLOWLY);
            }
            // Leap used when off-cooldown, or when out-of-range
            if (!UseOOCBuff && Hotbar.Contains(SNOPower.Barbarian_Leap) && !PlayerStatus.IsIncapacitated &&
                (AnythingWithinRange[RANGE_20] > 1 || ElitesWithinRange[RANGE_20] > 0) && GilesUseTimer(SNOPower.Barbarian_Leap, true) &&
                PowerManager.CanCast(SNOPower.Barbarian_Leap))
            {
                // For close-by monsters, try to leap a little further than their centre-point
                float fExtraDistance = CurrentTarget.Radius;
                if (fExtraDistance <= 4f)
                    fExtraDistance = 4f;
                if (CurrentTarget.CentreDistance + fExtraDistance > 35f)
                    fExtraDistance = 35 - CurrentTarget.CentreDistance;
                Vector3 vNewTarget = MathEx.CalculatePointFrom(CurrentTarget.Position, PlayerStatus.CurrentPosition, CurrentTarget.CentreDistance + fExtraDistance);
                return new TrinityPower(SNOPower.Barbarian_Leap, 35f, vNewTarget, CurrentWorldDynamicId, -1, 2, 2, USE_SLOWLY);
            }


            // Rend spam for Non-WhirlWind users
            if (!UseOOCBuff && !PlayerStatus.IsIncapacitated && Hotbar.Contains(SNOPower.Barbarian_Rend) && 
                TargetUtil.AnyMobsInRange(9) && !CurrentTarget.IsTreasureGoblin &&
                ((!IsWaitingForSpecial && PlayerStatus.PrimaryResource >= 20) || (IsWaitingForSpecial && PlayerStatus.PrimaryResource > MinEnergyReserve)) &&
                (GilesUseTimer(SNOPower.Barbarian_Rend) && (NonRendedTargets_9 > 2 || !CurrentTarget.HasDotDPS)) &&
                (TimeSinceUse(SNOPower.Barbarian_Rend) > 1500 || TargetUtil.AnyMobsInRange(10f, 6)) && LastPowerUsed != SNOPower.Barbarian_Rend
                )
            {
                iWithinRangeLastRend = GilesObjectCache.Count(u => u.Type == GObjectType.Unit && u.RadiusDistance <= 9f);
                iACDGUIDLastRend = CurrentTarget.ACDGuid;
                // Note - we have LONGER animation times for whirlwind-users
                // Since whirlwind seems to interrupt rend so easily
                int rendPreDelay = 0;
                int rendPostDelay = 1;
                if (Hotbar.Contains(SNOPower.Barbarian_Whirlwind) && (LastPowerUsed == SNOPower.Barbarian_Whirlwind || LastPowerUsed == SNOPower.None))
                {
                    rendPreDelay = 2;
                    rendPostDelay = 2;
                }
                return new TrinityPower(SNOPower.Barbarian_Rend, 0f, PlayerStatus.CurrentPosition, CurrentWorldDynamicId, -1, rendPreDelay, rendPostDelay, USE_SLOWLY);
            }

            // Overpower used off-cooldown
            if (!UseOOCBuff && Hotbar.Contains(SNOPower.Barbarian_Overpower) && !PlayerStatus.IsIncapacitated &&
                (CurrentTarget.RadiusDistance <= 6f ||
                    (
                        AnythingWithinRange[RANGE_6] >= 1 &&
                        (CurrentTarget.IsEliteRareUnique || CurrentTarget.IsMinion || CurrentTarget.IsBoss || GetHasBuff(SNOPower.Barbarian_WrathOfTheBerserker) ||
                        (CurrentTarget.IsTreasureGoblin && CurrentTarget.CentreDistance <= 6f) || Hotbar.Contains(SNOPower.Barbarian_SeismicSlam))
                    )
                ) &&
                GilesUseTimer(SNOPower.Barbarian_Overpower) && PowerManager.CanCast(SNOPower.Barbarian_Overpower))
            {
                int iPreDelay = 0;
                int iPostDelay = 0;
                // Note - we have LONGER animation times for whirlwind-users
                // Since whirlwind seems to interrupt rend so easily
                /*if (hashPowerHotbarAbilities.Contains(SNOPower.Barbarian_Whirlwind))
                {
                    if (powerLastSnoPowerUsed == SNOPower.Barbarian_Whirlwind || powerLastSnoPowerUsed == SNOPower.None)
                    {
                        iPreDelay = 5;
                        iPostDelay = 5;
                    }
                }*/
                return new TrinityPower(SNOPower.Barbarian_Overpower, 0f, PlayerStatus.CurrentPosition, CurrentWorldDynamicId, -1, iPreDelay, iPostDelay, USE_SLOWLY);
            }
            // Seismic slam enemies within close range
            if (!UseOOCBuff && !IsWaitingForSpecial && Hotbar.Contains(SNOPower.Barbarian_SeismicSlam) && !PlayerStatus.IsIncapacitated &&
                (!Hotbar.Contains(SNOPower.Barbarian_BattleRage) || (Hotbar.Contains(SNOPower.Barbarian_BattleRage) && GetHasBuff(SNOPower.Barbarian_BattleRage))) &&
                PlayerStatus.PrimaryResource >= 15 && CurrentTarget.CentreDistance <= 40f && (AnythingWithinRange[RANGE_50] > 1 ||
                (AnythingWithinRange[RANGE_50] > 0 && PlayerStatus.PrimaryResourcePct >= 0.85 && CurrentTarget.HitPointsPct >= 0.30) ||
                (CurrentTarget.IsBoss || CurrentTarget.IsEliteRareUnique || (CurrentTarget.IsTreasureGoblin && CurrentTarget.CentreDistance <= 20f))))
            {
                return new TrinityPower(SNOPower.Barbarian_SeismicSlam, 40f, vNullLocation, -1, CurrentTarget.ACDGuid, 2, 2, USE_SLOWLY);
            }
            // Ancient spear 
            if (!UseOOCBuff && !IsCurrentlyAvoiding && Hotbar.Contains(SNOPower.Barbarian_AncientSpear) &&
                GilesUseTimer(SNOPower.Barbarian_AncientSpear) && PowerManager.CanCast(SNOPower.Barbarian_AncientSpear) &&
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
                Vector3 vNewTarget = MathEx.CalculatePointFrom(CurrentTarget.Position, PlayerStatus.CurrentPosition, CurrentTarget.CentreDistance + fExtraDistance);
                return new TrinityPower(SNOPower.Barbarian_AncientSpear, 55f, vNewTarget, CurrentWorldDynamicId, -1, 2, 2, USE_SLOWLY);
            }
            // Sprint buff, if same suitable targets as elites, keep maintained for WW users
            if (!UseOOCBuff && !bDontSpamOutofCombat && Hotbar.Contains(SNOPower.Barbarian_Sprint) && !PlayerStatus.IsIncapacitated &&
                // Let's check if is not spaming too much
                DateTime.Now.Subtract(dictAbilityLastUse[SNOPower.Barbarian_Sprint]).TotalMilliseconds >= 200 &&
                // Fury Dump Options for sprint: use at max energy constantly, or on a timer
                (
                    (Settings.Combat.Barbarian.FuryDumpWOTB && PlayerStatus.PrimaryResourcePct >= 0.95 && GetHasBuff(SNOPower.Barbarian_WrathOfTheBerserker)) ||
                    (Settings.Combat.Barbarian.FuryDumpAlways && PlayerStatus.PrimaryResourcePct >= 0.95) ||
                    ((GilesUseTimer(SNOPower.Barbarian_Sprint) && !GetHasBuff(SNOPower.Barbarian_Sprint)) &&
                // Always keep up if we are whirlwinding, if the target is a goblin, or if we are 16 feet away from the target
                    (Hotbar.Contains(SNOPower.Barbarian_Whirlwind) || CurrentTarget.IsTreasureGoblin || (CurrentTarget.CentreDistance >= 16f && PlayerStatus.PrimaryResource >= 40)))
                ) &&
                // If they have battle-rage, make sure it's up
                (!Hotbar.Contains(SNOPower.Barbarian_BattleRage) || (Hotbar.Contains(SNOPower.Barbarian_BattleRage) && GetHasBuff(SNOPower.Barbarian_BattleRage))) &&
                // Check for minimum energy
                PlayerStatus.PrimaryResource >= 20)
            {
                return new TrinityPower(SNOPower.Barbarian_Sprint, 0f, vNullLocation, CurrentWorldDynamicId, -1, 0, 0, USE_SLOWLY);
            }
            // Whirlwind spam as long as necessary pre-buffs are up
            if (!UseOOCBuff && !IsCurrentlyAvoiding && Hotbar.Contains(SNOPower.Barbarian_Whirlwind) && !PlayerStatus.IsIncapacitated && !PlayerStatus.IsRooted &&
                // Don't WW against goblins, units in the special SNO list
                (!Settings.Combat.Barbarian.SelectiveWhirlwind || bAnyNonWWIgnoreMobsInRange || !hashActorSNOWhirlwindIgnore.Contains(CurrentTarget.ActorSNO)) &&
                // Only if within 15 foot of main target
                ((CurrentTarget.RadiusDistance <= 25f || AnythingWithinRange[RANGE_25] >= 1)) &&
                (AnythingWithinRange[RANGE_50] >= 2 || CurrentTarget.HitPointsPct >= 0.30 || CurrentTarget.IsBoss || CurrentTarget.IsEliteRareUnique || PlayerStatus.CurrentHealthPct <= 0.60) &&
                // Check for energy reservation amounts
                //((playerStatus.dCurrentEnergy >= 20 && !playerStatus.bWaitingForReserveEnergy) || playerStatus.dCurrentEnergy >= iWaitingReservedAmount) &&
                PlayerStatus.PrimaryResource >= 10 &&
                // If they have battle-rage, make sure it's up
                (!Hotbar.Contains(SNOPower.Barbarian_BattleRage) || (Hotbar.Contains(SNOPower.Barbarian_BattleRage) && GetHasBuff(SNOPower.Barbarian_BattleRage))))
            // If they have sprint, make sure it's up
            // (!hashPowerHotbarAbilities.Contains(SNOPower.Barbarian_Sprint) || (hashPowerHotbarAbilities.Contains(SNOPower.Barbarian_Sprint) && GilesHasBuff(SNOPower.Barbarian_Sprint)))
            {
                bool bGenerateNewZigZag = (DateTime.Now.Subtract(lastChangedZigZag).TotalMilliseconds >= 2000f ||
                    (vPositionLastZigZagCheck != vNullLocation && PlayerStatus.CurrentPosition == vPositionLastZigZagCheck && DateTime.Now.Subtract(lastChangedZigZag).TotalMilliseconds >= 1200) ||
                    Vector3.Distance(PlayerStatus.CurrentPosition, vSideToSideTarget) <= 5f ||
                    CurrentTarget.ACDGuid != iACDGUIDLastWhirlwind);
                vPositionLastZigZagCheck = PlayerStatus.CurrentPosition;
                if (bGenerateNewZigZag)
                {
                    var wwdist = Settings.Combat.Barbarian.TargetBasedZigZag ? 10f : 20f;

                    if (bCheckGround)
                        vSideToSideTarget = FindZigZagTargetLocation(CurrentTarget.Position, wwdist, false, true, true);
                    else if (AnythingWithinRange[RANGE_30] >= 6 || ElitesWithinRange[RANGE_30] >= 3 || c_ActorSNO == 89690)
                        vSideToSideTarget = FindZigZagTargetLocation(CurrentTarget.Position, wwdist, false, true);
                    else
                        vSideToSideTarget = FindZigZagTargetLocation(CurrentTarget.Position, wwdist);
                    LastPowerUsed = SNOPower.None;
                    iACDGUIDLastWhirlwind = CurrentTarget.ACDGuid;
                    lastChangedZigZag = DateTime.Now;
                }
                return new TrinityPower(SNOPower.Barbarian_Whirlwind, 10f, vSideToSideTarget, CurrentWorldDynamicId, -1, 0, 0, USE_SLOWLY);
            }
            // Battle rage, constantly maintain
            if (!UseOOCBuff && Hotbar.Contains(SNOPower.Barbarian_BattleRage) && !PlayerStatus.IsIncapacitated &&
                // Fury Dump Options for battle rage IF they don't have sprint 
                (
                 (Settings.Combat.Barbarian.FuryDumpWOTB && PlayerStatus.PrimaryResourcePct >= 0.99 && GetHasBuff(SNOPower.Barbarian_WrathOfTheBerserker)) ||
                 (Settings.Combat.Barbarian.FuryDumpAlways && PlayerStatus.PrimaryResourcePct >= 0.99) || !GetHasBuff(SNOPower.Barbarian_BattleRage)
                ) &&
                PlayerStatus.PrimaryResource >= 20 && PowerManager.CanCast(SNOPower.Barbarian_BattleRage))
            {
                return new TrinityPower(SNOPower.Barbarian_BattleRage, 0f, vNullLocation, CurrentWorldDynamicId, -1, 0, 0, USE_SLOWLY);
            }
            // Hammer of the ancients spam-attacks - never use if waiting for special
            if (!UseOOCBuff && !IsCurrentlyAvoiding && !PlayerStatus.IsIncapacitated && !IsWaitingForSpecial && Hotbar.Contains(SNOPower.Barbarian_HammerOfTheAncients) &&
                PlayerStatus.PrimaryResource >= 20)
            {
                //return new TrinityPower(SNOPower.Barbarian_HammerOfTheAncients, 12f, vNullLocation, -1, CurrentTarget.ACDGuid, 2, 2, USE_SLOWLY);
                return new TrinityPower(SNOPower.Barbarian_HammerOfTheAncients, 18f, CurrentTarget.Position, CurrentWorldDynamicId, -1, 2, 2, USE_SLOWLY);
            }
            // Weapon throw
            if (!UseOOCBuff && !IsCurrentlyAvoiding && Hotbar.Contains(SNOPower.Barbarian_WeaponThrow)
                && (PlayerStatus.PrimaryResource >= 10 && (CurrentTarget.RadiusDistance >= 5f || BarbHasNoPrimary())))
            {
                return new TrinityPower(SNOPower.Barbarian_WeaponThrow, 80f, vNullLocation, -1, CurrentTarget.ACDGuid, 0, 0, SIGNATURE_SPAM);
            }
            // Frenzy rapid-attacks
            if (!UseOOCBuff && !IsCurrentlyAvoiding && Hotbar.Contains(SNOPower.Barbarian_Frenzy))
            {
                return new TrinityPower(SNOPower.Barbarian_Frenzy, 10f, vNullLocation, -1, CurrentTarget.ACDGuid, 0, 0, SIGNATURE_SPAM);
            }
            // Bash fast-attacks
            if (!UseOOCBuff && !IsCurrentlyAvoiding && Hotbar.Contains(SNOPower.Barbarian_Bash))
            {
                return new TrinityPower(SNOPower.Barbarian_Bash, 10f, vNullLocation, -1, CurrentTarget.ACDGuid, 0, 1, USE_SLOWLY);
            }
            // Cleave fast-attacks
            if (!UseOOCBuff && !IsCurrentlyAvoiding && Hotbar.Contains(SNOPower.Barbarian_Cleave))
            {
                return new TrinityPower(SNOPower.Barbarian_Cleave, 10f, vNullLocation, -1, CurrentTarget.ACDGuid, 0, 2, USE_SLOWLY);
            }
            // Default attacks
            if (!UseOOCBuff && !IsCurrentlyAvoiding)
            {
                return new TrinityPower(GetDefaultWeaponPower(), GetDefaultWeaponDistance(), vNullLocation, -1, CurrentTarget.ACDGuid, 0, 0, USE_SLOWLY);
            }
            return new TrinityPower(SNOPower.None, -1, vNullLocation, -1, -1, 0, 0, USE_SLOWLY);
        }

        private static bool BarbHasNoPrimary()
        {
            return !(Hotbar.Contains(SNOPower.Barbarian_Frenzy) ||
                Hotbar.Contains(SNOPower.Barbarian_Bash) ||
                Hotbar.Contains(SNOPower.Barbarian_Cleave));
        }

        private static TrinityPower GetBarbarianDestroyPower()
        {
            if (Hotbar.Contains(SNOPower.Barbarian_Whirlwind) && PlayerStatus.PrimaryResource > MinEnergyReserve)
                return new TrinityPower(SNOPower.Barbarian_Whirlwind, 10f, vSideToSideTarget, CurrentWorldDynamicId, -1, 0, 0, USE_SLOWLY);
            if (Hotbar.Contains(SNOPower.Barbarian_Frenzy))
                return new TrinityPower(SNOPower.Barbarian_Frenzy, 10f, vNullLocation, -1, -1, 0, 0, USE_SLOWLY);
            if (Hotbar.Contains(SNOPower.Barbarian_Bash))
                return new TrinityPower(SNOPower.Barbarian_Bash, 6f, vNullLocation, -1, -1, 0, 0, USE_SLOWLY);
            if (Hotbar.Contains(SNOPower.Barbarian_Cleave))
                return new TrinityPower(SNOPower.Barbarian_Cleave, 6f, vNullLocation, -1, -1, 0, 0, USE_SLOWLY);
            if (Hotbar.Contains(SNOPower.Barbarian_Rend) && PlayerStatus.PrimaryResourcePct >= 0.65)
                return new TrinityPower(SNOPower.Barbarian_Rend, 10f, vNullLocation, -1, -1, 0, 0, USE_SLOWLY);
            if (Hotbar.Contains(SNOPower.Barbarian_WeaponThrow) && PlayerStatus.PrimaryResource >= 20)
                return new TrinityPower(SNOPower.Barbarian_WeaponThrow, 15f, vNullLocation, -1, -1, 0, 0, USE_SLOWLY);
            return new TrinityPower(SNOPower.Weapon_Melee_Instant, 10f, vNullLocation, -1, -1, 0, 0, USE_SLOWLY);
        }

    }
}
