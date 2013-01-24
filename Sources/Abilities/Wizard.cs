using System;
using System.Linq;

using Zeta;
using Zeta.Common;
using Zeta.Common.Plugins;
using Zeta.CommonBot;
using Zeta.Internals.Actors;
namespace GilesTrinity
{
    public partial class GilesTrinity : IPlugin
    {
        private static TrinityPower GetWizardPower(bool IsCurrentlyAvoiding, bool UseOOCBuff, bool UseDestructiblePower)
        {
            // Pick the best destructible power available
            if (UseDestructiblePower)
            {
                if (!GetHasBuff(SNOPower.Wizard_Archon))
                {
                    return GetWizardDestructablePower();
                }
                else
                {
                    if (CurrentTarget.RadiusDistance <= 10f)
                        return new TrinityPower(SNOPower.Wizard_Archon_ArcaneStrike, 20f, vNullLocation, -1, CurrentTarget.ACDGuid, 0, 0, USE_SLOWLY);
                    return new TrinityPower(SNOPower.Wizard_Archon_DisintegrationWave, 19f, vNullLocation, -1, CurrentTarget.ACDGuid, 0, 0, USE_SLOWLY);
                }
            }
            // Wizards want to save up to a reserve of 65+ energy
            MinEnergyReserve = 65;
            if (!GetHasBuff(SNOPower.Wizard_Archon))
            {
                // Slow time, for if being followed
                if (UseOOCBuff && !PlayerStatus.IsIncapacitated && Hotbar.Contains(SNOPower.Wizard_SlowTime) &&
                    GilesUseTimer(SNOPower.Wizard_SlowTime, true) && PowerManager.CanCast(SNOPower.Wizard_SlowTime))
                {
                    return new TrinityPower(SNOPower.Wizard_SlowTime, 0f, vNullLocation, iCurrentWorldID, -1, 1, 1, USE_SLOWLY);
                }
                // Slow Time for in combat
                if (!UseOOCBuff && !PlayerStatus.IsIncapacitated && Hotbar.Contains(SNOPower.Wizard_SlowTime) &&
                    (ElitesWithinRange[RANGE_25] > 0 || AnythingWithinRange[RANGE_25] > 1 || PlayerStatus.CurrentHealthPct <= 0.7 || ((CurrentTarget.IsEliteRareUnique || CurrentTarget.IsTreasureGoblin || CurrentTarget.IsBoss) && CurrentTarget.RadiusDistance <= 35f)) &&
                    PowerManager.CanCast(SNOPower.Wizard_SlowTime))
                {
                    return new TrinityPower(SNOPower.Wizard_SlowTime, 0f, vNullLocation, iCurrentWorldID, -1, 1, 1, USE_SLOWLY);
                }
                // Wave of force
                if (!UseOOCBuff && !PlayerStatus.IsIncapacitated && PlayerStatus.PrimaryResource >= 25 &&
                    (
                    // Check this isn't a critical mass wizard, cos they won't want to use this except for low health unless they don't have nova/blast in which case go for it
                    (Settings.Combat.Wizard.CriticalMass && ((!Hotbar.Contains(SNOPower.Wizard_FrostNova) && !Hotbar.Contains(SNOPower.Wizard_ExplosiveBlast)) ||
                        (PlayerStatus.CurrentHealthPct <= 0.7 && (ElitesWithinRange[RANGE_15] > 0 || AnythingWithinRange[RANGE_15] > 0 || (CurrentTarget.IsBossOrEliteRareUnique && CurrentTarget.RadiusDistance <= 23f)))))
                    // Else normal wizard in which case check standard stuff
                    || (!Settings.Combat.Wizard.CriticalMass && ElitesWithinRange[RANGE_15] > 0 || AnythingWithinRange[RANGE_15] > 3 || PlayerStatus.CurrentHealthPct <= 0.7 || (CurrentTarget.IsBossOrEliteRareUnique && CurrentTarget.RadiusDistance <= 23f))
                    ) &&
                    Hotbar.Contains(SNOPower.Wizard_WaveOfForce) &&
                    GilesUseTimer(SNOPower.Wizard_WaveOfForce, true) && PowerManager.CanCast(SNOPower.Wizard_WaveOfForce))
                {
                    return new TrinityPower(SNOPower.Wizard_WaveOfForce, 0f, vNullLocation, iCurrentWorldID, -1, 1, 2, USE_SLOWLY);
                }
                // Blizzard
                if (!UseOOCBuff && !PlayerStatus.IsIncapacitated && Hotbar.Contains(SNOPower.Wizard_Blizzard) &&
                    LastPowerUsed != SNOPower.Wizard_Blizzard &&
                    (ElitesWithinRange[RANGE_25] > 0 || AnythingWithinRange[RANGE_25] > 2 || CurrentTarget.IsEliteRareUnique || CurrentTarget.IsTreasureGoblin || CurrentTarget.IsBoss || PlayerStatus.CurrentHealthPct <= 0.7) &&
                    PlayerStatus.PrimaryResource >= 40 && GilesUseTimer(SNOPower.Wizard_Blizzard))
                {
                    Vector3 targetDirection = MathEx.CalculatePointFrom(PlayerStatus.CurrentPosition, CurrentTarget.Position, 1f);

                    ZetaDia.Me.UsePower(SNOPower.Walk, targetDirection);
                    return new TrinityPower(SNOPower.Wizard_Blizzard, 40f, new Vector3(CurrentTarget.Position.X, CurrentTarget.Position.Y, CurrentTarget.Position.Z), iCurrentWorldID, -1, 1, 1, USE_SLOWLY);
                }
                // Meteor
                if (!UseOOCBuff && !PlayerStatus.IsIncapacitated && Hotbar.Contains(SNOPower.Wizard_Meteor) &&
                    (ElitesWithinRange[RANGE_25] > 0 || AnythingWithinRange[RANGE_25] > 2 || CurrentTarget.IsEliteRareUnique || CurrentTarget.IsBoss || CurrentTarget.IsTreasureGoblin) &&
                    PlayerStatus.PrimaryResource >= 50 && PowerManager.CanCast(SNOPower.Wizard_Meteor))
                {
                    return new TrinityPower(SNOPower.Wizard_Meteor, 21f, new Vector3(CurrentTarget.Position.X, CurrentTarget.Position.Y, CurrentTarget.Position.Z), iCurrentWorldID, -1, 1, 2, USE_SLOWLY);
                }
                // Teleport in combat for critical-mass wizards
                if (!UseOOCBuff && !IsCurrentlyAvoiding && !PlayerStatus.IsIncapacitated && Hotbar.Contains(SNOPower.Wizard_Teleport) && Settings.Combat.Wizard.CriticalMass &&
                    LastPowerUsed != SNOPower.Wizard_Teleport &&
                    PlayerStatus.PrimaryResource >= 15 && CurrentTarget.CentreDistance <= 35f &&
                    PowerManager.CanCast(SNOPower.Wizard_Teleport))
                {
                    vSideToSideTarget = FindZigZagTargetLocation(CurrentTarget.Position, CurrentTarget.CentreDistance, true);
                    return new TrinityPower(SNOPower.Wizard_Teleport, 35f, vSideToSideTarget, iCurrentWorldID, -1, 1, 2, USE_SLOWLY);
                }
                // Diamond Skin SPAM
                if (Hotbar.Contains(SNOPower.Wizard_DiamondSkin) && LastPowerUsed != SNOPower.Wizard_DiamondSkin &&
                    (ElitesWithinRange[RANGE_25] > 0 || AnythingWithinRange[RANGE_25] > 0 || PlayerStatus.CurrentHealthPct <= 0.90 || PlayerStatus.IsIncapacitated || PlayerStatus.IsRooted || (!UseOOCBuff && CurrentTarget.RadiusDistance <= 40f)) &&
                    ((Settings.Combat.Wizard.CriticalMass && !UseOOCBuff) || !GetHasBuff(SNOPower.Wizard_DiamondSkin)) &&
                    PowerManager.CanCast(SNOPower.Wizard_DiamondSkin))
                {
                    return new TrinityPower(SNOPower.Wizard_DiamondSkin, 0f, vNullLocation, iCurrentWorldID, -1, 0, 1, USE_SLOWLY);
                }
                // The three wizard armors, done in an else-if loop so it doesn't keep replacing one with the other
                if (!PlayerStatus.IsIncapacitated && PlayerStatus.PrimaryResource >= 25)
                {
                    // Energy armor as priority cast if available and not buffed
                    if (Hotbar.Contains(SNOPower.Wizard_EnergyArmor))
                    {
                        if ((!GetHasBuff(SNOPower.Wizard_EnergyArmor) && PowerManager.CanCast(SNOPower.Wizard_EnergyArmor)) || (Hotbar.Contains(SNOPower.Wizard_Archon) && (!GetHasBuff(SNOPower.Wizard_EnergyArmor) || GilesUseTimer(SNOPower.Wizard_EnergyArmor))))
                        {
                            return new TrinityPower(SNOPower.Wizard_EnergyArmor, 0f, vNullLocation, iCurrentWorldID, -1, 1, 2, USE_SLOWLY);
                        }
                    }
                    // Ice Armor
                    else if (Hotbar.Contains(SNOPower.Wizard_IceArmor))
                    {
                        if (!GetHasBuff(SNOPower.Wizard_IceArmor) && PowerManager.CanCast(SNOPower.Wizard_IceArmor))
                        {
                            return new TrinityPower(SNOPower.Wizard_IceArmor, 0f, vNullLocation, iCurrentWorldID, -1, 1, 2, USE_SLOWLY);
                        }
                    }
                    // Storm Armor
                    else if (Hotbar.Contains(SNOPower.Wizard_StormArmor))
                    {
                        if (!GetHasBuff(SNOPower.Wizard_StormArmor) || ((DateTime.Now.Subtract(dictAbilityLastUse[SNOPower.Wizard_StormArmor]).TotalMilliseconds >= 15000) && PowerManager.CanCast(SNOPower.Wizard_Archon)))
                        {
                            return new TrinityPower(SNOPower.Wizard_StormArmor, 0f, vNullLocation, iCurrentWorldID, -1, 1, 2, USE_SLOWLY);
                        }
                    }
                }
                // Magic Weapon                        
                if (!PlayerStatus.IsIncapacitated && Hotbar.Contains(SNOPower.Wizard_MagicWeapon) && PowerManager.CanCast(SNOPower.Wizard_MagicWeapon) &&
                    (!GetHasBuff(SNOPower.Wizard_MagicWeapon) || ((DateTime.Now.Subtract(dictAbilityLastUse[SNOPower.Wizard_MagicWeapon]).TotalMilliseconds >= 10000) && PowerManager.CanCast(SNOPower.Wizard_Archon))))
                {
                    return new TrinityPower(SNOPower.Wizard_MagicWeapon, 0f, vNullLocation, iCurrentWorldID, -1, 1, 2, USE_SLOWLY);
                }
                // Familiar
                // Magic Weapon
                if (!PlayerStatus.IsIncapacitated && Hotbar.Contains(SNOPower.Wizard_MagicWeapon) &&
                    PlayerStatus.PrimaryResource >= 25 && (GilesUseTimer(SNOPower.Wizard_MagicWeapon) || !GetHasBuff(SNOPower.Wizard_MagicWeapon)))
                {
                    return new TrinityPower(SNOPower.Wizard_MagicWeapon, 0f, vNullLocation, iCurrentWorldID, -1, 1, 2, USE_SLOWLY);
                }
                // Familiar
                if (!PlayerStatus.IsIncapacitated && Hotbar.Contains(SNOPower.Wizard_Familiar) &&
                    PlayerStatus.PrimaryResource >= 25 && GilesUseTimer(SNOPower.Wizard_Familiar))
                {
                    return new TrinityPower(SNOPower.Wizard_Familiar, 0f, vNullLocation, iCurrentWorldID, -1, 1, 2, USE_SLOWLY);
                }
                // Hydra
                if (!UseOOCBuff && !PlayerStatus.IsIncapacitated &&
                    LastPowerUsed != SNOPower.Wizard_Hydra &&
                    (ElitesWithinRange[RANGE_15] > 0 || AnythingWithinRange[RANGE_15] > 4 || PlayerStatus.CurrentHealthPct <= 0.7 || ((CurrentTarget.IsEliteRareUnique || CurrentTarget.IsBoss || CurrentTarget.IsTreasureGoblin) && CurrentTarget.RadiusDistance <= 15f)) &&
                    Hotbar.Contains(SNOPower.Wizard_Hydra) &&
                    PlayerStatus.PrimaryResource >= 15 && GilesUseTimer(SNOPower.Wizard_Hydra))
                {
                    // For distant monsters, try to target a little bit in-front of them (as they run towards us), if it's not a treasure goblin
                    float fExtraDistance = 0f;
                    if (CurrentTarget.CentreDistance > 17f && !CurrentTarget.IsTreasureGoblin)
                    {
                        fExtraDistance = CurrentTarget.CentreDistance - 17f;
                        if (fExtraDistance > 5f)
                            fExtraDistance = 5f;
                        if (CurrentTarget.CentreDistance - fExtraDistance < 15f)
                            fExtraDistance -= 2;
                    }
                    Vector3 vNewTarget = MathEx.CalculatePointFrom(CurrentTarget.Position, PlayerStatus.CurrentPosition, CurrentTarget.CentreDistance - fExtraDistance);
                    return new TrinityPower(SNOPower.Wizard_Hydra, 30f, vNewTarget, iCurrentWorldID, -1, 1, 2, USE_SLOWLY);
                }
                // Mirror Image  @ half health or 5+ monsters or rooted/incapacitated or last elite left @25% health
                if (!UseOOCBuff && Hotbar.Contains(SNOPower.Wizard_MirrorImage) &&
                    (PlayerStatus.CurrentHealthPct <= 0.50 || AnythingWithinRange[RANGE_30] >= 5 || PlayerStatus.IsIncapacitated || PlayerStatus.IsRooted || (ElitesWithinRange[RANGE_30] == 1 && CurrentTarget.IsEliteRareUnique && !CurrentTarget.IsBoss && CurrentTarget.HitPoints <= 0.35)) &&
                    PowerManager.CanCast(SNOPower.Wizard_MirrorImage))
                {
                    return new TrinityPower(SNOPower.Wizard_MirrorImage, 0f, vNullLocation, iCurrentWorldID, -1, 1, 1, USE_SLOWLY);
                }
                // Archon
                if (!UseOOCBuff && !IsCurrentlyAvoiding && Hotbar.Contains(SNOPower.Wizard_Archon) &&
                    (ElitesWithinRange[RANGE_30] >= 1 || AnythingWithinRange[RANGE_25] >= 3 || (CurrentTarget.IsBossOrEliteRareUnique && CurrentTarget.RadiusDistance <= 30f)) &&
                    PlayerStatus.PrimaryResource >= 25 && PlayerStatus.CurrentHealthPct >= 0.10 &&
                    PowerManager.CanCast(SNOPower.Wizard_Archon))
                {
                    // Familiar has been removed for now. Uncomment the three comments below relating to familiars to force re-buffing them
                    bool bHasBuffAbilities = (Hotbar.Contains(SNOPower.Wizard_MagicWeapon) ||
                        //hashPowerHotbarAbilities.Contains(SNOPower.Wizard_Familiar) ||
                        Hotbar.Contains(SNOPower.Wizard_EnergyArmor) || Hotbar.Contains(SNOPower.Wizard_IceArmor) ||
                        Hotbar.Contains(SNOPower.Wizard_StormArmor));
                    int iExtraEnergyNeeded = 25;
                    if (Hotbar.Contains(SNOPower.Wizard_MagicWeapon)) iExtraEnergyNeeded += 25;
                    //if (hashPowerHotbarAbilities.Contains(SNOPower.Wizard_Familiar)) iExtraEnergyNeeded += 25;
                    if (Hotbar.Contains(SNOPower.Wizard_EnergyArmor) || Hotbar.Contains(SNOPower.Wizard_IceArmor) ||
                        Hotbar.Contains(SNOPower.Wizard_StormArmor)) iExtraEnergyNeeded += 25;
                    if (!bHasBuffAbilities || PlayerStatus.PrimaryResource <= iExtraEnergyNeeded)
                        CanCastArchon = true;
                    if (!CanCastArchon)
                    {
                        dictAbilityLastUse[SNOPower.Wizard_MagicWeapon] = DateTime.Today;
                        //dictAbilityLastUse[SNOPower.Wizard_Familiar] = DateTime.Today;
                        dictAbilityLastUse[SNOPower.Wizard_EnergyArmor] = DateTime.Today;
                        dictAbilityLastUse[SNOPower.Wizard_IceArmor] = DateTime.Today;
                        dictAbilityLastUse[SNOPower.Wizard_StormArmor] = DateTime.Today;
                        CanCastArchon = true;
                    }
                    else
                    {
                        CanCastArchon = false;
                        return new TrinityPower(SNOPower.Wizard_Archon, 0f, vNullLocation, iCurrentWorldID, -1, 4, 5, USE_SLOWLY);
                    }
                }
                // Frost Nova SPAM
                if (!UseOOCBuff && Hotbar.Contains(SNOPower.Wizard_FrostNova) && !PlayerStatus.IsIncapacitated &&
                    ((ElitesWithinRange[RANGE_25] > 0 || AnythingWithinRange[RANGE_25] > 0 || PlayerStatus.CurrentHealthPct <= 0.7) && CurrentTarget.RadiusDistance <= 12f) &&
                    PowerManager.CanCast(SNOPower.Wizard_FrostNova))
                {
                    float fThisRange = 14f;
                    if (Settings.Combat.Wizard.CriticalMass)
                        fThisRange = 9f;
                    return new TrinityPower(SNOPower.Wizard_FrostNova, fThisRange, vNullLocation, iCurrentWorldID, -1, 0, 0, USE_SLOWLY);
                }
                // Explosive Blast SPAM when enough AP, blow erry thing up, nah mean
                if (!UseOOCBuff && Hotbar.Contains(SNOPower.Wizard_ExplosiveBlast) && !PlayerStatus.IsIncapacitated && PlayerStatus.PrimaryResource >= 20 &&
                    ((ElitesWithinRange[RANGE_25] >= 1 || AnythingWithinRange[RANGE_25] >= 1 || PlayerStatus.CurrentHealthPct <= 0.7) && CurrentTarget.RadiusDistance <= 12f) &&
                    PowerManager.CanCast(SNOPower.Wizard_ExplosiveBlast))
                {
                    float fThisRange = 11f;
                    if (Settings.Combat.Wizard.CriticalMass)
                        fThisRange = 9f;
                    return new TrinityPower(SNOPower.Wizard_ExplosiveBlast, fThisRange, vNullLocation, iCurrentWorldID, -1, 0, 0, USE_SLOWLY);
                }

                // Check to see if we have a signature spell on our hotbar, for energy twister check
                bool bHasSignatureSpell = (Hotbar.Contains(SNOPower.Wizard_MagicMissile) || Hotbar.Contains(SNOPower.Wizard_ShockPulse) ||
                    Hotbar.Contains(SNOPower.Wizard_SpectralBlade) || Hotbar.Contains(SNOPower.Wizard_Electrocute));


                // Energy Twister SPAMS whenever 35 or more ap to generate Arcane Power
                if (!UseOOCBuff && !PlayerStatus.IsIncapacitated && Hotbar.Contains(SNOPower.Wizard_EnergyTwister) && PlayerStatus.PrimaryResource >= 35 &&
                    // If using storm chaser, then force a signature spell every 1 stack of the buff, if we have a signature spell
                    (!bHasSignatureSpell || GetBuffStacks(SNOPower.Wizard_EnergyTwister) < 1) &&
                    (ElitesWithinRange[RANGE_30] >= 1 || AnythingWithinRange[RANGE_25] >= 1 || CurrentTarget.RadiusDistance <= 12f) &&
                    (!Hotbar.Contains(SNOPower.Wizard_Electrocute) || !hashActorSNOFastMobs.Contains(CurrentTarget.ActorSNO)) &&
                    ((Settings.Combat.Wizard.CriticalMass && !bHasSignatureSpell) || !Settings.Combat.Wizard.CriticalMass))
                {
                    Vector3 targetDirection = MathEx.CalculatePointFrom(PlayerStatus.CurrentPosition, CurrentTarget.Position, 1f);
                    ZetaDia.Me.UsePower(SNOPower.Walk, targetDirection);
                    float fThisRange = 28f;
                    //if (Settings.Combat.Wizard.CriticalMass)
                    //    fThisRange = 9f;
                    return new TrinityPower(SNOPower.Wizard_EnergyTwister, fThisRange, CurrentTarget.Position, iCurrentWorldID, -1, 0, 0, USE_SLOWLY);
                }



                // Disintegrate
                if (!UseOOCBuff && !PlayerStatus.IsIncapacitated && Hotbar.Contains(SNOPower.Wizard_Disintegrate) &&
                    ((PlayerStatus.PrimaryResource >= 20 && !PlayerStatus.WaitingForReserveEnergy) || PlayerStatus.PrimaryResource >= MinEnergyReserve))
                {
                    float fThisRange = 35f;
                    if (Settings.Combat.Wizard.CriticalMass)
                        fThisRange = 20f;
                    return new TrinityPower(SNOPower.Wizard_Disintegrate, fThisRange, vNullLocation, -1, CurrentTarget.ACDGuid, 0, 0, SIGNATURE_SPAM);
                }
                // Arcane Orb
                if (!UseOOCBuff && !PlayerStatus.IsIncapacitated && Hotbar.Contains(SNOPower.Wizard_ArcaneOrb) &&
                    ((PlayerStatus.PrimaryResource >= 35 && !PlayerStatus.WaitingForReserveEnergy) || PlayerStatus.PrimaryResource >= MinEnergyReserve) &&
                    GilesUseTimer(SNOPower.Wizard_ArcaneOrb))
                {
                    float fThisRange = 40f;
                    if (Settings.Combat.Wizard.CriticalMass)
                        fThisRange = 20f;
                    return new TrinityPower(SNOPower.Wizard_ArcaneOrb, fThisRange, vNullLocation, -1, CurrentTarget.ACDGuid, 1, 1, USE_SLOWLY);
                }
                // Arcane Torrent
                if (!UseOOCBuff && !PlayerStatus.IsIncapacitated && Hotbar.Contains(SNOPower.Wizard_ArcaneTorrent) &&
                    ((PlayerStatus.PrimaryResource >= 16 && !PlayerStatus.WaitingForReserveEnergy) || PlayerStatus.PrimaryResource >= MinEnergyReserve) &&
                    GilesUseTimer(SNOPower.Wizard_ArcaneTorrent))
                {
                    float fThisRange = 40f;
                    /*if (Settings.Combat.Wizard.CriticalMass)
                        fThisRange = 20f;*/
                    return new TrinityPower(SNOPower.Wizard_ArcaneTorrent, fThisRange, vNullLocation, -1, CurrentTarget.ACDGuid, 0, 0, USE_SLOWLY);
                }
                // Ray of Frost
                if (!UseOOCBuff && !IsCurrentlyAvoiding && !PlayerStatus.IsIncapacitated && Hotbar.Contains(SNOPower.Wizard_RayOfFrost) &&
                    PlayerStatus.PrimaryResource >= 12)
                {
                    float fThisRange = 35f;
                    if (Settings.Combat.Wizard.CriticalMass)
                        fThisRange = 20f;
                    return new TrinityPower(SNOPower.Wizard_RayOfFrost, fThisRange, vNullLocation, -1, CurrentTarget.ACDGuid, 0, 0, SIGNATURE_SPAM);
                }
                // Magic Missile
                if (!UseOOCBuff && !IsCurrentlyAvoiding && Hotbar.Contains(SNOPower.Wizard_MagicMissile))
                {
                    float fThisRange = 35f;
                    if (Settings.Combat.Wizard.CriticalMass)
                        fThisRange = 20f;
                    return new TrinityPower(SNOPower.Wizard_MagicMissile, fThisRange, vNullLocation, -1, CurrentTarget.ACDGuid, 0, 0, USE_SLOWLY);
                }
                // Shock Pulse
                if (!UseOOCBuff && !IsCurrentlyAvoiding && Hotbar.Contains(SNOPower.Wizard_ShockPulse))
                {
                    return new TrinityPower(SNOPower.Wizard_ShockPulse, 15f, vNullLocation, -1, CurrentTarget.ACDGuid, 0, 1, USE_SLOWLY);
                }
                // Spectral Blade
                if (!UseOOCBuff && !IsCurrentlyAvoiding && Hotbar.Contains(SNOPower.Wizard_SpectralBlade))
                {
                    return new TrinityPower(SNOPower.Wizard_SpectralBlade, 14f, vNullLocation, -1, CurrentTarget.ACDGuid, 0, 1, USE_SLOWLY);
                }
                // Electrocute
                if (!UseOOCBuff && !IsCurrentlyAvoiding && Hotbar.Contains(SNOPower.Wizard_Electrocute))
                {
                    return new TrinityPower(SNOPower.Wizard_Electrocute, 40f, vNullLocation, -1, CurrentTarget.ACDGuid, 0, 0, USE_SLOWLY);
                }
                // Default attacks
                if (!UseOOCBuff && !IsCurrentlyAvoiding)
                {
                    return new TrinityPower(GetDefaultWeaponPower(), GetDefaultWeaponDistance(), vNullLocation, -1, CurrentTarget.ACDGuid, 0, 0, USE_SLOWLY);
                }
                return new TrinityPower(GetDefaultWeaponPower(), GetDefaultWeaponDistance(), vNullLocation, -1, -1, 0, 0, USE_SLOWLY);
            }
            else
            {
                // Archon form
                // Archon Slow Time for in combat
                if (!UseOOCBuff && !PlayerStatus.IsIncapacitated &&
                    (ElitesWithinRange[RANGE_25] > 0 || AnythingWithinRange[RANGE_25] > 1 || PlayerStatus.CurrentHealthPct <= 0.7 || ((CurrentTarget.IsEliteRareUnique || CurrentTarget.IsTreasureGoblin || CurrentTarget.IsBoss) && CurrentTarget.RadiusDistance <= 35f)) &&
                    Hotbar.Contains(SNOPower.Wizard_Archon_SlowTime) &&
                    GilesUseTimer(SNOPower.Wizard_Archon_SlowTime, true) && PowerManager.CanCast(SNOPower.Wizard_Archon_SlowTime))
                {
                    return new TrinityPower(SNOPower.Wizard_Archon_SlowTime, 0f, vNullLocation, iCurrentWorldID, -1, 1, 1, USE_SLOWLY);
                }
                // Archon Teleport in combat
                if (!UseOOCBuff && !IsCurrentlyAvoiding && !PlayerStatus.IsIncapacitated && Hotbar.Contains(SNOPower.Wizard_Archon_Teleport) &&
                    // Try and teleport-retreat from 1 elite or 3+ greys or a boss at 15 foot range
                    (ElitesWithinRange[RANGE_15] >= 1 || AnythingWithinRange[RANGE_15] >= 3 || (CurrentTarget.IsBoss && CurrentTarget.RadiusDistance <= 15f)) &&
                    GilesUseTimer(SNOPower.Wizard_Archon_Teleport) && PowerManager.CanCast(SNOPower.Wizard_Archon_Teleport))
                {
                    Vector3 vNewTarget = MathEx.CalculatePointFrom(CurrentTarget.Position, PlayerStatus.CurrentPosition, -20f);
                    return new TrinityPower(SNOPower.Wizard_Archon_Teleport, 35f, vNewTarget, iCurrentWorldID, -1, 1, 1, USE_SLOWLY);
                }
                // Arcane Blast
                if (!UseOOCBuff && !PlayerStatus.IsIncapacitated &&
                    (ElitesWithinRange[RANGE_15] >= 1 || AnythingWithinRange[RANGE_15] >= 1 ||
                     (CurrentTarget.IsBossOrEliteRareUnique && CurrentTarget.RadiusDistance <= 15f)) &&
                    GilesUseTimer(SNOPower.Wizard_Archon_ArcaneBlast) && PowerManager.CanCast(SNOPower.Wizard_Archon_ArcaneBlast))
                {
                    return new TrinityPower(SNOPower.Wizard_Archon_ArcaneBlast, 0f, vNullLocation, iCurrentWorldID, -1, 1, 1, USE_SLOWLY);
                }
                // Arcane Strike (Arcane Strike) Rapid Spam at close-range only
                if (!UseOOCBuff && !PlayerStatus.IsIncapacitated && CurrentTarget.RadiusDistance <= 7f &&
                    CurrentTarget.IsBossOrEliteRareUnique)
                {
                    return new TrinityPower(SNOPower.Wizard_Archon_ArcaneStrike, 7f, vNullLocation, -1, CurrentTarget.ACDGuid, 1, 1, USE_SLOWLY);
                }
                // Disintegrate
                if (!UseOOCBuff && !IsCurrentlyAvoiding && !PlayerStatus.IsIncapacitated)
                {
                    return new TrinityPower(SNOPower.Wizard_Archon_DisintegrationWave, 49f, vNullLocation, -1, CurrentTarget.ACDGuid, 0, 0, SIGNATURE_SPAM);
                }
                return new TrinityPower(SNOPower.None, -1, vNullLocation, -1, -1, 0, 0, USE_SLOWLY);
            }
        }

        private static TrinityPower GetWizardDestructablePower()
        {
            if (Hotbar.Contains(SNOPower.Wizard_EnergyTwister) && PlayerStatus.PrimaryResource >= 35)
                return new TrinityPower(SNOPower.Wizard_EnergyTwister, 9f, vNullLocation, -1, -1, 0, 0, USE_SLOWLY);

            if (Hotbar.Contains(SNOPower.Wizard_MagicMissile))
                return new TrinityPower(SNOPower.Wizard_MagicMissile, 15f, vNullLocation, -1, -1, 0, 0, USE_SLOWLY);

            if (Hotbar.Contains(SNOPower.Wizard_ShockPulse))
                return new TrinityPower(SNOPower.Wizard_ShockPulse, 10f, vNullLocation, -1, -1, 0, 0, USE_SLOWLY);

            if (Hotbar.Contains(SNOPower.Wizard_SpectralBlade))
                return new TrinityPower(SNOPower.Wizard_SpectralBlade, 9f, vNullLocation, -1, -1, 0, 0, USE_SLOWLY);

            if (Hotbar.Contains(SNOPower.Wizard_Electrocute))
                return new TrinityPower(SNOPower.Wizard_Electrocute, 9f, vNullLocation, -1, -1, 0, 0, USE_SLOWLY);

            return new TrinityPower(GetDefaultWeaponPower(), GetDefaultWeaponDistance(), vNullLocation, -1, -1, 0, 0, USE_SLOWLY);
        }



    }
}
