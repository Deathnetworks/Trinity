using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.Windows.Markup;
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
using Zeta.XmlEngine;
namespace GilesTrinity
{
    public partial class GilesTrinity : IPlugin
    {
        private static GilesPower GetBarbarianPower(bool bCurrentlyAvoiding, bool bOOCBuff, bool bDestructiblePower)
        {

            // Pick the best destructible power available
            if (bDestructiblePower)
            {
                return GetBarbarianDestroyPower();
            }
            // Barbarians need 56 reserve for special spam like WW
            iWaitingReservedAmount = 56;
            // Ignore Pain when low on health
            if (!bOOCBuff && hashPowerHotbarAbilities.Contains(SNOPower.Barbarian_IgnorePain) && playerStatus.dCurrentHealthPct <= 0.45 &&
                GilesUseTimer(SNOPower.Barbarian_IgnorePain, true) && PowerManager.CanCast(SNOPower.Barbarian_IgnorePain))
            {
                return new GilesPower(SNOPower.Barbarian_IgnorePain, 0f, vNullLocation, iCurrentWorldID, -1, 0, 0, USE_SLOWLY);
            }
            // Flag up a variable to see if we should reserve 50 fury for special abilities
            bWaitingForSpecial = false;
            if (!bOOCBuff && !bCurrentlyAvoiding && hashPowerHotbarAbilities.Contains(SNOPower.Barbarian_Earthquake) &&
                iElitesWithinRange[RANGE_25] >= 1 && GilesUseTimer(SNOPower.Barbarian_Earthquake))
            {
                bWaitingForSpecial = true;
            }
            if (!bOOCBuff && !bCurrentlyAvoiding && hashPowerHotbarAbilities.Contains(SNOPower.Barbarian_WrathOfTheBerserker) &&
                iElitesWithinRange[RANGE_25] >= 1 && GilesUseTimer(SNOPower.Barbarian_WrathOfTheBerserker))
            {
                bWaitingForSpecial = true;
            }
            if (!bOOCBuff && !bCurrentlyAvoiding && hashPowerHotbarAbilities.Contains(SNOPower.Barbarian_CallOfTheAncients) &&
                iElitesWithinRange[RANGE_25] >= 1 && GilesUseTimer(SNOPower.Barbarian_CallOfTheAncients))
            {
                bWaitingForSpecial = true;
            }
            // Earthquake, elites close-range only
            if (!bOOCBuff && !bCurrentlyAvoiding && hashPowerHotbarAbilities.Contains(SNOPower.Barbarian_Earthquake) && !playerStatus.bIsIncapacitated &&
                (iElitesWithinRange[RANGE_15] > 0 || ((CurrentTarget.IsEliteRareUnique || CurrentTarget.IsBoss) && CurrentTarget.fRadiusDistance <= 13f)) &&
                GilesUseTimer(SNOPower.Barbarian_Earthquake, true) &&
                PowerManager.CanCast(SNOPower.Barbarian_Earthquake))
            {
                if (playerStatus.dCurrentEnergy >= 50)
                    return new GilesPower(SNOPower.Barbarian_Earthquake, 13f, vNullLocation, iCurrentWorldID, -1, 4, 4, USE_SLOWLY);
                bWaitingForSpecial = true;
            }
            // Wrath of the berserker, elites only (wrath of berserker)
            //intell
            if (!bOOCBuff && hashPowerHotbarAbilities.Contains(SNOPower.Barbarian_WrathOfTheBerserker) && bUseBerserker &&
                // Not on heart of sin after Cydaea
                CurrentTarget.iActorSNO != 193077 &&
                // Make sure we are allowed to use wrath on goblins, else make sure this isn't a goblin
                (settings.bGoblinWrath || !CurrentTarget.bIsTreasureGoblin || iElitesWithinRange[RANGE_15] >= 1) &&
                // If on a boss, only when injured
                ((CurrentTarget.IsBoss && CurrentTarget.iHitPoints <= 0.99 && !hashPowerHotbarAbilities.Contains(SNOPower.Barbarian_Whirlwind)) ||
                // If *NOT* on a boss, and definitely no boss in range, then judge based on any elites at all within 30 feet
                 ((!CurrentTarget.IsBoss || hashPowerHotbarAbilities.Contains(SNOPower.Barbarian_Whirlwind)) &&
                   (!bAnyBossesInRange || hashPowerHotbarAbilities.Contains(SNOPower.Barbarian_Whirlwind)) &&
                   ((iElitesWithinRange[RANGE_20] >= 1 || CurrentTarget.IsEliteRareUnique) && (CurrentTarget.iHitPoints >= 0.30 || playerStatus.dCurrentHealthPct <= 0.60))
                 )) &&
                // Don't still have the buff
                !GilesHasBuff(SNOPower.Barbarian_WrathOfTheBerserker) &&
                GilesUseTimer(SNOPower.Barbarian_WrathOfTheBerserker, true) && PowerManager.CanCast(SNOPower.Barbarian_WrathOfTheBerserker))
            {
                if (playerStatus.dCurrentEnergy >= 56)
                //intell
                {
                    if (CurrentTarget.iActorSNO == 255996)
                        Logging.Write("[Trinity] Berserk being used: Act 1 Warden, Odeg!");
                    else if (CurrentTarget.iActorSNO == 256000)
                        Logging.Write("[Trinity] Berserk being used: Act 2 Warden, Sokahr!");
                    else if (CurrentTarget.iActorSNO == 256015)
                        Logging.Write("[Trinity] Berserk being used: Act 3 Warden, Xah'rith!");
                    else
                        Logging.Write("[Trinity] Berserk being used!");
                    Logging.Write("[Trinity] Berserk being used!");
                    bUseBerserker = false;
                    return new GilesPower(SNOPower.Barbarian_WrathOfTheBerserker, 0f, vNullLocation, iCurrentWorldID, -1, 1, 1, USE_SLOWLY);
                    //intell -- 4, 4
                }
                else
                {
                    Logging.Write("[Trinity] Berserk ready, waiting for fury...");
                    bWaitingForSpecial = true;
                }
            }
            // Call of the ancients, elites only
            if (!bOOCBuff && !bCurrentlyAvoiding && hashPowerHotbarAbilities.Contains(SNOPower.Barbarian_CallOfTheAncients) && !playerStatus.bIsIncapacitated &&
                (iElitesWithinRange[RANGE_25] > 0 || ((CurrentTarget.IsEliteRareUnique || CurrentTarget.bIsTreasureGoblin || CurrentTarget.IsBoss) && CurrentTarget.fRadiusDistance <= 25f)) &&
                GilesUseTimer(SNOPower.Barbarian_CallOfTheAncients, true) &&
                PowerManager.CanCast(SNOPower.Barbarian_CallOfTheAncients))
            {
                if (playerStatus.dCurrentEnergy >= 50)
                    return new GilesPower(SNOPower.Barbarian_CallOfTheAncients, 0f, vNullLocation, iCurrentWorldID, -1, 4, 4, USE_SLOWLY);
                bWaitingForSpecial = true;
            }
            // Battle rage, for if being followed and before we do sprint
            if (bOOCBuff && !playerStatus.bIsIncapacitated && hashPowerHotbarAbilities.Contains(SNOPower.Barbarian_BattleRage) &&
                (GilesUseTimer(SNOPower.Barbarian_BattleRage) || !GilesHasBuff(SNOPower.Barbarian_BattleRage)) &&
                playerStatus.dCurrentEnergy >= 20 && PowerManager.CanCast(SNOPower.Barbarian_BattleRage))
            {
                return new GilesPower(SNOPower.Barbarian_BattleRage, 0f, vNullLocation, iCurrentWorldID, -1, 1, 1, USE_SLOWLY);
            }
            // Special segment for sprint as an out-of-combat only
            if (bOOCBuff && !bDontSpamOutofCombat &&
                (settings.bOutOfCombatMovementPowers || GilesHasBuff(SNOPower.Barbarian_WrathOfTheBerserker)) &&
                !playerStatus.bIsIncapacitated && hashPowerHotbarAbilities.Contains(SNOPower.Barbarian_Sprint) &&
                !GilesHasBuff(SNOPower.Barbarian_Sprint) &&
                playerStatus.dCurrentEnergy >= 20 && GilesUseTimer(SNOPower.Barbarian_Sprint) && PowerManager.CanCast(SNOPower.Barbarian_Sprint))
            {
                return new GilesPower(SNOPower.Barbarian_Sprint, 0f, vNullLocation, iCurrentWorldID, -1, 0, 0, USE_SLOWLY);
            }
            // War cry, constantly maintain
            if (!playerStatus.bIsIncapacitated && hashPowerHotbarAbilities.Contains(SNOPower.Barbarian_WarCry) &&
                (playerStatus.dCurrentEnergy <= 60 || !GilesHasBuff(SNOPower.Barbarian_WarCry)) &&
                GilesUseTimer(SNOPower.Barbarian_WarCry, true) && (!GilesHasBuff(SNOPower.Barbarian_WarCry) || PowerManager.CanCast(SNOPower.Barbarian_WarCry)))
            {
                return new GilesPower(SNOPower.Barbarian_WarCry, 0f, vNullLocation, iCurrentWorldID, -1, 1, 1, USE_SLOWLY);
            }
            // Threatening shout
            //intell -- added spam usage if fury is low AND is a ww build or waiting for special
            if (!bOOCBuff && hashPowerHotbarAbilities.Contains(SNOPower.Barbarian_ThreateningShout) && !playerStatus.bIsIncapacitated &&
              (
                  iElitesWithinRange[RANGE_20] > 1 || (CurrentTarget.IsBoss && CurrentTarget.fRadiusDistance <= 20) ||
                  (iAnythingWithinRange[RANGE_20] > 2 && !bAnyBossesInRange && (iElitesWithinRange[RANGE_50] == 0 || hashPowerHotbarAbilities.Contains(SNOPower.Barbarian_SeismicSlam))) ||
                  playerStatus.dCurrentHealthPct <= 0.75 || (hashPowerHotbarAbilities.Contains(SNOPower.Barbarian_Whirlwind) && playerStatus.dCurrentEnergy < 10) ||
                  (bWaitingForSpecial && playerStatus.dCurrentEnergy <= 50)
                  ) &&
              GilesUseTimer(SNOPower.Barbarian_ThreateningShout, true) && PowerManager.CanCast(SNOPower.Barbarian_ThreateningShout))
            {
                return new GilesPower(SNOPower.Barbarian_ThreateningShout, 0f, vNullLocation, iCurrentWorldID, -1, 1, 1, USE_SLOWLY);
            }
            // Threatening shout out-of-combat: helps battle rage and sprint (5+15=20)
            //intell
            if (bOOCBuff && hashPowerHotbarAbilities.Contains(SNOPower.Barbarian_ThreateningShout) &&
                (hashPowerHotbarAbilities.Contains(SNOPower.Barbarian_Sprint) || hashPowerHotbarAbilities.Contains(SNOPower.Barbarian_BattleRage)) &&
                !playerStatus.bIsIncapacitated && playerStatus.dCurrentEnergy >= 5 && playerStatus.dCurrentEnergy < 20 &&
                GilesUseTimer(SNOPower.Barbarian_ThreateningShout, true) && PowerManager.CanCast(SNOPower.Barbarian_ThreateningShout))
            {
                return new GilesPower(SNOPower.Barbarian_ThreateningShout, 0f, vNullLocation, iCurrentWorldID, -1, 1, 1, USE_SLOWLY);
            }
            // Ground Stomp
            if (!bOOCBuff && hashPowerHotbarAbilities.Contains(SNOPower.Barbarian_GroundStomp) && !playerStatus.bIsIncapacitated &&
                (iElitesWithinRange[RANGE_15] > 0 || iAnythingWithinRange[RANGE_15] > 4 || playerStatus.dCurrentHealthPct <= 0.7) &&
                GilesUseTimer(SNOPower.Barbarian_GroundStomp, true) &&
                PowerManager.CanCast(SNOPower.Barbarian_GroundStomp))
            {
                return new GilesPower(SNOPower.Barbarian_GroundStomp, 16f, vNullLocation, iCurrentWorldID, -1, 1, 2, USE_SLOWLY);
            }
            // Revenge used off-cooldown
            if (!bOOCBuff && hashPowerHotbarAbilities.Contains(SNOPower.Barbarian_Revenge) && !playerStatus.bIsIncapacitated &&
                // Don't use revenge on goblins, too slow!
                (!CurrentTarget.bIsTreasureGoblin || iAnythingWithinRange[RANGE_12] >= 5) &&
                // Doesn't need CURRENT target to be in range, just needs ANYTHING to be within 9 foot, since it's an AOE!
                (iAnythingWithinRange[RANGE_6] > 0 || CurrentTarget.fRadiusDistance <= 6f) &&
                GilesUseTimer(SNOPower.Barbarian_Revenge) && PowerManager.CanCast(SNOPower.Barbarian_Revenge))
            {
                // Note - we have LONGER animation times for whirlwind-users
                // Since whirlwind seems to interrupt rend so easily
                int iPreDelay = 3;
                int iPostDelay = 3;
                if (hashPowerHotbarAbilities.Contains(SNOPower.Barbarian_Whirlwind))
                {
                    if (powerLastSnoPowerUsed == SNOPower.Barbarian_Whirlwind)
                    {
                        iPreDelay = 5;
                        iPostDelay = 5;
                    }
                }
                return new GilesPower(SNOPower.Barbarian_Revenge, 0f, playerStatus.vCurrentPosition, iCurrentWorldID, -1, iPreDelay, iPostDelay, USE_SLOWLY);
            }
            // Furious charge
            if (!bOOCBuff && hashPowerHotbarAbilities.Contains(SNOPower.Barbarian_FuriousCharge) &&
                (iElitesWithinRange[RANGE_12] > 3 &&
                GilesUseTimer(SNOPower.Barbarian_FuriousCharge) &&
                PowerManager.CanCast(SNOPower.Barbarian_FuriousCharge)))
            {
                float fExtraDistance;
                if (CurrentTarget.fCentreDist <= 25)
                    fExtraDistance = 30;
                else
                    fExtraDistance = (25 - CurrentTarget.fCentreDist);
                if (fExtraDistance < 5f)
                    fExtraDistance = 5f;
                Vector3 vNewTarget = MathEx.CalculatePointFrom(CurrentTarget.vPosition, playerStatus.vCurrentPosition, CurrentTarget.fCentreDist + fExtraDistance);
                return new GilesPower(SNOPower.Barbarian_FuriousCharge, 32f, vNewTarget, iCurrentWorldID, -1, 1, 2, USE_SLOWLY);
            }
            // Leap used when off-cooldown, or when out-of-range
            if (!bOOCBuff && hashPowerHotbarAbilities.Contains(SNOPower.Barbarian_Leap) && !playerStatus.bIsIncapacitated &&
                (iAnythingWithinRange[RANGE_20] > 1 || iElitesWithinRange[RANGE_20] > 0) && GilesUseTimer(SNOPower.Barbarian_Leap, true) &&
                PowerManager.CanCast(SNOPower.Barbarian_Leap))
            {
                // For close-by monsters, try to leap a little further than their centre-point
                float fExtraDistance = CurrentTarget.fRadius;
                if (fExtraDistance <= 4f)
                    fExtraDistance = 4f;
                if (CurrentTarget.fCentreDist + fExtraDistance > 35f)
                    fExtraDistance = 35 - CurrentTarget.fCentreDist;
                Vector3 vNewTarget = MathEx.CalculatePointFrom(CurrentTarget.vPosition, playerStatus.vCurrentPosition, CurrentTarget.fCentreDist + fExtraDistance);
                return new GilesPower(SNOPower.Barbarian_Leap, 35f, vNewTarget, iCurrentWorldID, -1, 2, 2, USE_SLOWLY);
            }
            // Rend spam
            if (!bOOCBuff && !playerStatus.bIsIncapacitated && hashPowerHotbarAbilities.Contains(SNOPower.Barbarian_Rend) &&
                iAnythingWithinRange[RANGE_12] >= 1 &&
                // Doesn't need CURRENT target to be in range, just needs ANYTHING to be within 9 foot, since it's an AOE!
                (iAnythingWithinRange[RANGE_6] > 0 || CurrentTarget.fRadiusDistance <= 6f) &&
                // Don't use against goblins (they run too quick!)
                (!CurrentTarget.bIsTreasureGoblin || iAnythingWithinRange[RANGE_12] >= 5) &&
                (
                // This segment is for people who DON'T have whirlwind
                     (!hashPowerHotbarAbilities.Contains(SNOPower.Barbarian_Whirlwind) &&
                        (
                // *DON'T* use rend if we currently have wrath/earthquake/call available & needed but need to save up energy energy
                        (!bWaitingForSpecial || playerStatus.dCurrentEnergy >= 75) &&
                // Bunch of optionals now that go hand in hand with all of the above...
                            (
                // Either off full 4 second or so cooldown...
                             GilesUseTimer(SNOPower.Barbarian_Rend) ||
                // ... or ability to spam rend every 0.4 seconds if more enemies in range than when last used rend...
                             (iAnythingWithinRange[RANGE_6] > iWithinRangeLastRend && DateTime.Now.Subtract(dictAbilityLastUse[SNOPower.Barbarian_Rend]).TotalMilliseconds >= 1000) ||
                // ... or ability to spam rend every 1.1 seconds if current primary target changes...
                             (CurrentTarget.iACDGuid != iACDGUIDLastRend && DateTime.Now.Subtract(dictAbilityLastUse[SNOPower.Barbarian_Rend]).TotalMilliseconds >= 1800) ||
                // ... or ability to spam rend every 1.5 seconds with almost full fury
                             (playerStatus.dCurrentEnergyPct >= 0.85 && DateTime.Now.Subtract(dictAbilityLastUse[SNOPower.Barbarian_Rend]).TotalMilliseconds >= 2500) ||
                // ... or ability to spam rend every 2 seconds with a lot of fury
                             (playerStatus.dCurrentEnergyPct >= 0.65 && DateTime.Now.Subtract(dictAbilityLastUse[SNOPower.Barbarian_Rend]).TotalMilliseconds >= 3500)
                            )
                        )) ||
                // This segment is for people who *DO* have whirlwind
                     (hashPowerHotbarAbilities.Contains(SNOPower.Barbarian_Whirlwind) &&
                // See if it's off-cooldown and at least 40 fury, or use as a fury dump
                         (
                            (settings.bFuryDumpWrath && playerStatus.dCurrentEnergyPct >= 0.92 && GilesHasBuff(SNOPower.Barbarian_WrathOfTheBerserker)) ||
                            (settings.bFuryDumpAlways && playerStatus.dCurrentEnergyPct >= 0.92) ||
                            (DateTime.Now.Subtract(dictAbilityLastUse[SNOPower.Barbarian_Rend]).TotalMilliseconds >= 2800)
                         ) &&
                // Max once every 1.2 seconds even if fury dumping, so sprint can be fury dumped too
                // DateTime.Now.Subtract(dictAbilityLastUse[SNOPower.Barbarian_Rend]).TotalMilliseconds >= 1200 &&
                // 3+ mobs of any kind at close range *OR* one elite/boss/special at close range
                         (
                            (iAnythingWithinRange[RANGE_15] >= 3 && iElitesWithinRange[RANGE_12] >= 1) ||
                            (iAnythingWithinRange[RANGE_15] >= 3 && CurrentTarget.bIsTreasureGoblin && CurrentTarget.fRadiusDistance <= 13f) ||
                            iAnythingWithinRange[RANGE_15] >= 5 ||
                            ((CurrentTarget.IsEliteRareUnique || CurrentTarget.IsBoss) && CurrentTarget.fRadiusDistance <= 13f && iAnythingWithinRange[RANGE_15] >= 3)
                         )
                     )
                ) &&
                // And finally, got at least 20 energy
                playerStatus.dCurrentEnergy >= 20)
            {
                iWithinRangeLastRend = iAnythingWithinRange[RANGE_6];
                iACDGUIDLastRend = CurrentTarget.iACDGuid;
                // Note - we have LONGER animation times for whirlwind-users
                // Since whirlwind seems to interrupt rend so easily
                int iPreDelay = 3;
                int iPostDelay = 3;
                if (hashPowerHotbarAbilities.Contains(SNOPower.Barbarian_Whirlwind))
                {
                    if (powerLastSnoPowerUsed == SNOPower.Barbarian_Whirlwind || powerLastSnoPowerUsed == SNOPower.None)
                    {
                        iPreDelay = 5;
                        iPostDelay = 5;
                    }
                }
                return new GilesPower(SNOPower.Barbarian_Rend, 0f, playerStatus.vCurrentPosition, iCurrentWorldID, -1, iPreDelay, iPostDelay, USE_SLOWLY);
            }
            // Overpower used off-cooldown
            if (!bOOCBuff && hashPowerHotbarAbilities.Contains(SNOPower.Barbarian_Overpower) && !playerStatus.bIsIncapacitated &&
                // Doesn't need CURRENT target to be in range, just needs ANYTHING to be within 9 foot, since it's an AOE!
                //(iAnythingWithinRange[RANGE_5] > 0 || targetCurrent.fRadiusDistance <= 6f) &&
                //intell -- now used on melee goblin
                (
                    iAnythingWithinRange[RANGE_6] >= 2 ||
                    (playerStatus.dCurrentHealthPct <= 0.85 && CurrentTarget.fRadiusDistance <= 5f) ||
                    (
                        iAnythingWithinRange[RANGE_6] >= 1 &&
                        (CurrentTarget.IsEliteRareUnique || CurrentTarget.bIsMinion || CurrentTarget.IsBoss || GilesHasBuff(SNOPower.Barbarian_WrathOfTheBerserker) ||
                        (CurrentTarget.bIsTreasureGoblin && CurrentTarget.fCentreDist <= 6f) || hashPowerHotbarAbilities.Contains(SNOPower.Barbarian_SeismicSlam))
                        )
                ) &&
                GilesUseTimer(SNOPower.Barbarian_Overpower) && PowerManager.CanCast(SNOPower.Barbarian_Overpower))
            {
                // Note - we have LONGER animation times for whirlwind-users
                // Since whirlwind seems to interrupt rend so easily
                /*int iPreDelay = 3;
                int iPostDelay = 3;
                if (hashPowerHotbarAbilities.Contains(SNOPower.Barbarian_Whirlwind))
                {
                    if (powerLastSnoPowerUsed == SNOPower.Barbarian_Whirlwind || powerLastSnoPowerUsed == SNOPower.None)
                    {
                        iPreDelay = 5;
                        iPostDelay = 5;
                    }
                }*/
                int iPreDelay = 0;
                int iPostDelay = 0;
                return new GilesPower(SNOPower.Barbarian_Overpower, 0f, playerStatus.vCurrentPosition, iCurrentWorldID, -1, iPreDelay, iPostDelay, USE_SLOWLY);
            }
            // Seismic slam enemies within close range
            if (!bOOCBuff && !bWaitingForSpecial && hashPowerHotbarAbilities.Contains(SNOPower.Barbarian_SeismicSlam) && !playerStatus.bIsIncapacitated &&
                (!hashPowerHotbarAbilities.Contains(SNOPower.Barbarian_BattleRage) || (hashPowerHotbarAbilities.Contains(SNOPower.Barbarian_BattleRage) && GilesHasBuff(SNOPower.Barbarian_BattleRage))) &&
                playerStatus.dCurrentEnergy >= 15 && CurrentTarget.fCentreDist <= 40f && (iAnythingWithinRange[RANGE_50] > 1 ||
                (iAnythingWithinRange[RANGE_50] > 0 && playerStatus.dCurrentEnergyPct >= 0.85 && CurrentTarget.iHitPoints >= 0.30) ||
                (CurrentTarget.IsBoss || CurrentTarget.IsEliteRareUnique || (CurrentTarget.bIsTreasureGoblin && CurrentTarget.fCentreDist <= 20f))))
            {
                return new GilesPower(SNOPower.Barbarian_SeismicSlam, 40f, vNullLocation, -1, CurrentTarget.iACDGuid, 2, 2, USE_SLOWLY);
            }
            // Ancient spear 
            if (!bOOCBuff && !bCurrentlyAvoiding && hashPowerHotbarAbilities.Contains(SNOPower.Barbarian_AncientSpear) &&
                GilesUseTimer(SNOPower.Barbarian_AncientSpear) &&
                PowerManager.CanCast(SNOPower.Barbarian_AncientSpear) &&
          CurrentTarget.iHitPoints >= 0.20)
            {
                // For close-by monsters, try to leap a little further than their centre-point
                float fExtraDistance = CurrentTarget.fRadius;
                if (fExtraDistance <= 4f)
                    fExtraDistance = 30f;
                if (CurrentTarget.fCentreDist + fExtraDistance > 60f)
                    fExtraDistance = 60 - CurrentTarget.fCentreDist;
                if (fExtraDistance < 30)
                    fExtraDistance = 30f;
                Vector3 vNewTarget = MathEx.CalculatePointFrom(CurrentTarget.vPosition, playerStatus.vCurrentPosition, CurrentTarget.fCentreDist + fExtraDistance);
                return new GilesPower(SNOPower.Barbarian_AncientSpear, 55f, vNewTarget, iCurrentWorldID, -1, 2, 2, USE_SLOWLY);
            }
            // Sprint buff, if same suitable targets as elites, keep maintained for WW users
            if (!bOOCBuff && !bDontSpamOutofCombat && hashPowerHotbarAbilities.Contains(SNOPower.Barbarian_Sprint) && !playerStatus.bIsIncapacitated &&
                //intell -- Let's check if is not spaming to much
                DateTime.Now.Subtract(dictAbilityLastUse[SNOPower.Barbarian_Sprint]).TotalMilliseconds >= 200 &&
                // Fury Dump Options for sprint: use at max energy constantly, or on a timer
                (
                    (settings.bFuryDumpWrath && playerStatus.dCurrentEnergyPct >= 0.95 && GilesHasBuff(SNOPower.Barbarian_WrathOfTheBerserker)) ||
                    (settings.bFuryDumpAlways && playerStatus.dCurrentEnergyPct >= 0.95) ||
                    ((GilesUseTimer(SNOPower.Barbarian_Sprint) && !GilesHasBuff(SNOPower.Barbarian_Sprint)) &&
                // Always keep up if we are whirlwinding, or if the target is a goblin
                        (hashPowerHotbarAbilities.Contains(SNOPower.Barbarian_Whirlwind) || CurrentTarget.bIsTreasureGoblin))
                ) &&
                // Or if the target is >16 feet away and we have 50+ fury
                //(targetCurrent.fCentreDistance >= 16f && playerStatus.dCurrentEnergy >= 50)
                // If they have battle-rage, make sure it's up
                (!hashPowerHotbarAbilities.Contains(SNOPower.Barbarian_BattleRage) || (hashPowerHotbarAbilities.Contains(SNOPower.Barbarian_BattleRage) && GilesHasBuff(SNOPower.Barbarian_BattleRage))) &&
                // Check for reserved-energy waiting or not
                //((playerStatus.dCurrentEnergy >= 40 && !playerStatus.bWaitingForReserveEnergy) || playerStatus.dCurrentEnergy >= iWaitingReservedAmount) &&
                playerStatus.dCurrentEnergy >= 20)
            {
                return new GilesPower(SNOPower.Barbarian_Sprint, 0f, vNullLocation, iCurrentWorldID, -1, 0, 0, USE_SLOWLY);
            }
            // Whirlwind spam as long as necessary pre-buffs are up
            if (!bOOCBuff && !bCurrentlyAvoiding && hashPowerHotbarAbilities.Contains(SNOPower.Barbarian_Whirlwind) && !playerStatus.bIsIncapacitated && !playerStatus.bIsRooted &&
                // Don't WW against goblins, units in the special SNO list
                (!settings.bSelectiveWhirlwind || bAnyNonWWIgnoreMobsInRange || !hashActorSNOWhirlwindIgnore.Contains(CurrentTarget.iActorSNO)) &&
                // Only if within 15 foot of main target
                ((CurrentTarget.fRadiusDistance <= 25f || iAnythingWithinRange[RANGE_25] >= 1)) &&
                (iAnythingWithinRange[RANGE_50] >= 2 || CurrentTarget.iHitPoints >= 0.30 || CurrentTarget.IsBoss || CurrentTarget.IsEliteRareUnique || playerStatus.dCurrentHealthPct <= 0.60) &&
                // Check for energy reservation amounts
                //((playerStatus.dCurrentEnergy >= 20 && !playerStatus.bWaitingForReserveEnergy) || playerStatus.dCurrentEnergy >= iWaitingReservedAmount) &&
                playerStatus.dCurrentEnergy >= 10 &&
                // If they have battle-rage, make sure it's up
                (!hashPowerHotbarAbilities.Contains(SNOPower.Barbarian_BattleRage) || (hashPowerHotbarAbilities.Contains(SNOPower.Barbarian_BattleRage) && GilesHasBuff(SNOPower.Barbarian_BattleRage))))
            // If they have sprint, make sure it's up
            // (!hashPowerHotbarAbilities.Contains(SNOPower.Barbarian_Sprint) || (hashPowerHotbarAbilities.Contains(SNOPower.Barbarian_Sprint) && GilesHasBuff(SNOPower.Barbarian_Sprint)))
            {
                bool bGenerateNewZigZag = (DateTime.Now.Subtract(lastChangedZigZag).TotalMilliseconds >= 2000f ||
                    (vPositionLastZigZagCheck != vNullLocation && playerStatus.vCurrentPosition == vPositionLastZigZagCheck && DateTime.Now.Subtract(lastChangedZigZag).TotalMilliseconds >= 1200) ||
                    Vector3.Distance(playerStatus.vCurrentPosition, vSideToSideTarget) <= 5f ||
                    CurrentTarget.iACDGuid != iACDGUIDLastWhirlwind);
                vPositionLastZigZagCheck = playerStatus.vCurrentPosition;
                if (bGenerateNewZigZag)
                {
                    //float fExtraDistance = targetCurrent.fCentreDistance+(targetCurrent.fCentreDistance <= 16f ? 16f : 8f);
                    vSideToSideTarget = FindZigZagTargetLocation(CurrentTarget.vPosition, CurrentTarget.fCentreDist + 25f);
                    // Resetting this to ensure the "no-spam" is reset since we changed our target location
                    if (iAnythingWithinRange[RANGE_30] >= 6 || iElitesWithinRange[RANGE_30] >= 3 || c_iActorSNO == 89690)
                        vSideToSideTarget = FindZigZagTargetLocation(CurrentTarget.vPosition, 20f, false, false, true);
                    else
                        vSideToSideTarget = FindZigZagTargetLocation(CurrentTarget.vPosition, 20f);
                    powerLastSnoPowerUsed = SNOPower.None;
                    iACDGUIDLastWhirlwind = CurrentTarget.iACDGuid;
                    lastChangedZigZag = DateTime.Now;
                }
                return new GilesPower(SNOPower.Barbarian_Whirlwind, 10f, vSideToSideTarget, iCurrentWorldID, -1, 0, 0, USE_SLOWLY);
            }
            // Battle rage, constantly maintain
            if (!bOOCBuff && hashPowerHotbarAbilities.Contains(SNOPower.Barbarian_BattleRage) && !playerStatus.bIsIncapacitated &&
                // Fury Dump Options for battle rage IF they don't have sprint 
                (
                 (settings.bFuryDumpWrath && playerStatus.dCurrentEnergyPct >= 0.99 && GilesHasBuff(SNOPower.Barbarian_WrathOfTheBerserker)) ||
                 (settings.bFuryDumpAlways && playerStatus.dCurrentEnergyPct >= 0.99) || !GilesHasBuff(SNOPower.Barbarian_BattleRage)
                ) &&
                playerStatus.dCurrentEnergy >= 20 && PowerManager.CanCast(SNOPower.Barbarian_BattleRage))
            {
                //intell
                return new GilesPower(SNOPower.Barbarian_BattleRage, 0f, vNullLocation, iCurrentWorldID, -1, 0, 0, USE_SLOWLY);
            }
            // Hammer of the ancients spam-attacks - never use if waiting for special
            if (!bOOCBuff && !bCurrentlyAvoiding && !bWaitingForSpecial && hashPowerHotbarAbilities.Contains(SNOPower.Barbarian_HammerOfTheAncients) && !playerStatus.bIsIncapacitated &&
                playerStatus.dCurrentEnergy >= 20 &&
               (
                // More than 75 energy... *OR* 55 energy and target is high on health... 
                    playerStatus.dCurrentEnergy >= 75 || (playerStatus.dCurrentEnergy >= 55 && CurrentTarget.iHitPoints >= 0.50) ||
                // OR... target is elite/goblin/boss...
                    CurrentTarget.IsEliteRareUnique || CurrentTarget.bIsTreasureGoblin || CurrentTarget.IsBoss ||
                // OR... player WOTB is active... OR player is low on health...
                    GilesHasBuff(SNOPower.Barbarian_WrathOfTheBerserker) || playerStatus.dCurrentHealthPct <= 0.38
               ) &&
               GilesUseTimer(SNOPower.Barbarian_HammerOfTheAncients))
            {
                return new GilesPower(SNOPower.Barbarian_HammerOfTheAncients, 12f, vNullLocation, -1, CurrentTarget.iACDGuid, 1, 1, USE_SLOWLY);
            }
            // Weapon throw
            if (!bOOCBuff && !bCurrentlyAvoiding && hashPowerHotbarAbilities.Contains(SNOPower.Barbarian_WeaponThrow)
                && (playerStatus.dCurrentEnergy >= 10 && CurrentTarget.fRadiusDistance >= 5f))
            {
                return new GilesPower(SNOPower.Barbarian_WeaponThrow, 80f, vNullLocation, -1, CurrentTarget.iACDGuid, 0, 0, SIGNATURE_SPAM);
            }
            // Frenzy rapid-attacks
            if (!bOOCBuff && !bCurrentlyAvoiding && hashPowerHotbarAbilities.Contains(SNOPower.Barbarian_Frenzy))
            {
                return new GilesPower(SNOPower.Barbarian_Frenzy, 10f, vNullLocation, -1, CurrentTarget.iACDGuid, 0, 0, SIGNATURE_SPAM);
            }
            // Bash fast-attacks
            if (!bOOCBuff && !bCurrentlyAvoiding && hashPowerHotbarAbilities.Contains(SNOPower.Barbarian_Bash))
            {
                return new GilesPower(SNOPower.Barbarian_Bash, 10f, vNullLocation, -1, CurrentTarget.iACDGuid, 0, 1, USE_SLOWLY);
            }
            // Cleave fast-attacks
            if (!bOOCBuff && !bCurrentlyAvoiding && hashPowerHotbarAbilities.Contains(SNOPower.Barbarian_Cleave))
            {
                return new GilesPower(SNOPower.Barbarian_Cleave, 10f, vNullLocation, -1, CurrentTarget.iACDGuid, 0, 2, USE_SLOWLY);
            }
            // Default attacks
            if (!bOOCBuff && !bCurrentlyAvoiding)
            {
                return new GilesPower(SNOPower.Weapon_Melee_Instant, 10f, vNullLocation, -1, CurrentTarget.iACDGuid, 1, 1, USE_SLOWLY);
            }
            return defaultPower;
        }

        private static GilesPower GetBarbarianDestroyPower()
        {
            if (hashPowerHotbarAbilities.Contains(SNOPower.Barbarian_Frenzy))
                return new GilesPower(SNOPower.Barbarian_Frenzy, 10f, vNullLocation, -1, -1, 0, 0, USE_SLOWLY);
            if (hashPowerHotbarAbilities.Contains(SNOPower.Barbarian_Bash))
                return new GilesPower(SNOPower.Barbarian_Bash, 10f, vNullLocation, -1, -1, 0, 0, USE_SLOWLY);
            if (hashPowerHotbarAbilities.Contains(SNOPower.Barbarian_Cleave))
                return new GilesPower(SNOPower.Barbarian_Cleave, 10f, vNullLocation, -1, -1, 0, 0, USE_SLOWLY);
            if (hashPowerHotbarAbilities.Contains(SNOPower.Barbarian_Rend) && playerStatus.dCurrentEnergyPct >= 0.65)
                return new GilesPower(SNOPower.Barbarian_Rend, 10f, vNullLocation, -1, -1, 0, 0, USE_SLOWLY);
            if (hashPowerHotbarAbilities.Contains(SNOPower.Barbarian_WeaponThrow) && playerStatus.dCurrentEnergy >= 20)
                return new GilesPower(SNOPower.Barbarian_WeaponThrow, 15f, vNullLocation, -1, -1, 0, 0, USE_SLOWLY);
            return new GilesPower(SNOPower.Weapon_Melee_Instant, 10f, vNullLocation, -1, -1, 0, 0, USE_SLOWLY);
        }

    }
}
