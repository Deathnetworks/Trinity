
using System.Linq;
using Zeta.Common;
using Zeta.Game.Internals.Actors;

namespace Trinity.Combat.Abilities
{
    public class CrusaderCombat : CombatBase
    {

        public static global::Trinity.Config.Combat.CrusaderSetting CrusaderSettings
        {
            get { return Trinity.Settings.Combat.Crusader; }
        }


        public static TrinityPower GetPower()
        {

            TrinityPower power = null;

            if (!UseOOCBuff && IsCurrentlyAvoiding)
            {
                if (CanCastSteedCharge())
                {
                    return new TrinityPower(SNOPower.X1_Crusader_SteedCharge);
                }
            }

            if (!UseOOCBuff && !IsCurrentlyAvoiding && CurrentTarget != null)
            {
                /*
                 *  Laws for Active Buff
                 */

                //There are doubles?? not sure which is correct yet
                // Laws of Hope
                // Laws of Hope2
                if (CombatBase.CanCast(SNOPower.X1_Crusader_LawsOfHope2) && (TargetUtil.EliteOrTrashInRange(16f) || TargetUtil.AnyMobsInRange(15f, 5, true)))
                {
                    return new TrinityPower(SNOPower.X1_Crusader_LawsOfHope2);
                }

                // LawsOfJustice
                // LawsOfJustice2
                if (CombatBase.CanCast(SNOPower.X1_Crusader_LawsOfJustice2) && (TargetUtil.EliteOrTrashInRange(16f) || Player.CurrentHealthPct <= CrusaderSettings.LawsOfJusticeHpPct || TargetUtil.AnyMobsInRange(15f, 5, true)))
                {
                    return new TrinityPower(SNOPower.X1_Crusader_LawsOfJustice2);
                }

                // LawsOfValor
                // LawsOfValor2
                if (CombatBase.CanCast(SNOPower.X1_Crusader_LawsOfValor2) && (TargetUtil.EliteOrTrashInRange(16f) || TargetUtil.AnyMobsInRange(15f, 5, true)))
                {
                    return new TrinityPower(SNOPower.X1_Crusader_LawsOfValor2);
                }

                // Judgement
                if (CanCastJudgement())
                {
                    return new TrinityPower(SNOPower.X1_Crusader_Judgment, 20f, TargetUtil.GetBestClusterPoint(20f));
                }

                // Shield Glare
                if (CanCastShieldGlare())
                {
                    var arcTarget = TargetUtil.GetBestArcTarget(45f, 70f);
                    if (arcTarget != null && arcTarget.Position != Vector3.Zero)
                        return new TrinityPower(SNOPower.X1_Crusader_ShieldGlare, 15f, arcTarget.Position);
                }

                // Iron Skin
                if (CanCastIronSkin())
                {
                    return new TrinityPower(SNOPower.X1_Crusader_IronSkin);
                }

                // Provoke
                if (CombatBase.CanCast(SNOPower.X1_Crusader_Provoke) && (TargetUtil.EliteOrTrashInRange(15f) || TargetUtil.AnyMobsInRange(15f, CrusaderSettings.ProvokeAoECount)))
                {
                    return new TrinityPower(SNOPower.X1_Crusader_Provoke);
                }

                // Consecration
                if (CanCastConsecration())
                {
                    return new TrinityPower(SNOPower.X1_Crusader_Consecration);
                }

                // AkaratsChampion
                if (CanCastAkaratsChampion())
                {
                    return new TrinityPower(SNOPower.X1_Crusader_AkaratsChampion);
                }

                // Bombardment
                if (CanCastBombardment())
                {
                    Vector3 bestPoint = CurrentTarget.IsEliteRareUnique ?
                        CurrentTarget.Position :
                        TargetUtil.GetBestClusterPoint(15f);
                    return new TrinityPower(SNOPower.X1_Crusader_Bombardment, 16f, bestPoint);
                }

                // FallingSword
                if (CanCastFallingSword())
                {
                    return new TrinityPower(SNOPower.X1_Crusader_FallingSword, 16f, TargetUtil.GetBestClusterPoint(15f, 65f, false, true));
                }

                // HeavensFury
                if (CanCastHeavensFury())
                {
                    return new TrinityPower(SNOPower.X1_Crusader_HeavensFury3, 16f, TargetUtil.GetBestClusterPoint(15f));
                }

                // Condemn
                if (CanCastCondemn())
                {
                    return new TrinityPower(SNOPower.X1_Crusader_Condemn);
                }

                // Phalanx
                if (CanCastPhalanx())
                {
                    return new TrinityPower(SNOPower.x1_Crusader_Phalanx3, 45f, TargetUtil.GetBestClusterPoint(15f, 45f, true, true));
                }

                // Blessed Shield : Piercing Shield
                bool hasPiercingShield = HotbarSkills.AssignedSkills.Any(s => s.Power == SNOPower.X1_Crusader_BlessedShield && s.RuneIndex == 5);
                if (CanCastBlessedShieldPiercingShield(hasPiercingShield))
                {
                    var bestPierceTarget = TargetUtil.GetBestPierceTarget(45f);
                    if (bestPierceTarget != null)
                        return new TrinityPower(SNOPower.X1_Crusader_BlessedShield, 14f, bestPierceTarget.ACDGuid);
                }

                // Blessed Shield
                if (CanCastBlessedShield(hasPiercingShield))
                {
                    return new TrinityPower(SNOPower.X1_Crusader_BlessedShield, 14f, TargetUtil.GetBestClusterUnit(15f, 65f, 1).ACDGuid);
                }

                // Fist of Heavens
                if (CanCastFistOfHeavens())
                {
                    return new TrinityPower(SNOPower.X1_Crusader_FistOfTheHeavens, 65f, TargetUtil.GetBestClusterUnit(8f, 65f, 1).Position);
                }

                // Provoke
                if (CombatBase.CanCast(SNOPower.X1_Crusader_Provoke) && TargetUtil.AnyMobsInRange(15f, 4))
                {
                    return new TrinityPower(SNOPower.X1_Crusader_Provoke);
                }

                // Shield Bash
                if (CombatBase.CanCast(SNOPower.X1_Crusader_ShieldBash2))
                {
                    var bestPierceTarget = TargetUtil.GetBestClusterUnit(15f, 65f, 1, false, false);
                    if (bestPierceTarget != null)
                        return new TrinityPower(SNOPower.X1_Crusader_ShieldBash2, 65f, bestPierceTarget.ACDGuid);
                }

                // Blessed Hammer, spin outwards 
                // Limitless rune can spawn new hammers
                if (CombatBase.CanCast(SNOPower.X1_Crusader_BlessedHammer) && (TargetUtil.EliteOrTrashInRange(20f) || TargetUtil.AnyTrashInRange(20f, 3)))
                {
                    return new TrinityPower(SNOPower.X1_Crusader_BlessedHammer);
                }

                // Sweep Attack
                if (CanCastSweepAttack())
                {
                    return new TrinityPower(SNOPower.X1_Crusader_SweepAttack, 5f, TargetUtil.GetBestClusterUnit(18f, 18f, 1).ACDGuid);
                }

                /*
                 *  Basic Attacks
                 */

                // Justice
                if (CombatBase.CanCast(SNOPower.X1_Crusader_Justice))
                {
                    return new TrinityPower(SNOPower.X1_Crusader_Justice, 7f, CurrentTarget.ACDGuid);
                }

                // Smite
                if (CombatBase.CanCast(SNOPower.X1_Crusader_Smite))
                {
                    return new TrinityPower(SNOPower.X1_Crusader_Smite, 15f, TargetUtil.GetBestClusterUnit(15f, 15f).ACDGuid);
                }

                // Slash
                if (CombatBase.CanCast(SNOPower.X1_Crusader_Slash))
                {
                    return new TrinityPower(SNOPower.X1_Crusader_Slash, 15f, TargetUtil.GetBestClusterUnit(5f, 8f, 1).ACDGuid);
                }

                // Punish
                if (CombatBase.CanCast(SNOPower.X1_Crusader_Punish))
                {
                    return new TrinityPower(SNOPower.X1_Crusader_Punish, 7f, CurrentTarget.ACDGuid);
                }

                //Blessed Shield no wrath needed with special weapon
                if (CombatBase.CanCast(SNOPower.X1_Crusader_BlessedShield))
                {
                    return new TrinityPower(SNOPower.X1_Crusader_BlessedShield, 14f, CurrentTarget.ACDGuid);
                }

            }

            // Buffs
            if (UseOOCBuff)
            {
                if (CombatBase.CanCast(SNOPower.X1_Crusader_SteedCharge) && CrusaderSettings.SteedChargeOOC)
                {
                    return new TrinityPower(SNOPower.X1_Crusader_SteedCharge);
                }

                /*
                 *  Laws
                 */

                //There are doubles?? not sure which is correct yet
                // Laws of Hope2
                if (CombatBase.CanCast(SNOPower.X1_Crusader_LawsOfHope2) && Player.CurrentHealthPct <= CrusaderSettings.LawsOfHopeHpPct)
                {
                    return new TrinityPower(SNOPower.X1_Crusader_LawsOfHope2);
                }

                // LawsOfJustice2
                if (CombatBase.CanCast(SNOPower.X1_Crusader_LawsOfJustice2) && (TargetUtil.EliteOrTrashInRange(16f) || Player.CurrentHealthPct <= CrusaderSettings.LawsOfJusticeHpPct))
                {
                    return new TrinityPower(SNOPower.X1_Crusader_LawsOfJustice2);
                }

                // LawsOfValor2
                if (CombatBase.CanCast(SNOPower.X1_Crusader_LawsOfValor2) && TargetUtil.EliteOrTrashInRange(16f))
                {
                    return new TrinityPower(SNOPower.X1_Crusader_LawsOfValor2);
                }
            }

            // Default Attacks
            if (IsNull(power))
                power = CombatBase.DefaultPower;

            return power;
        }

        private static bool CanCastSweepAttack()
        {
            return CombatBase.CanCast(SNOPower.X1_Crusader_SweepAttack) &&
                (TargetUtil.UnitsPlayerFacing(18f) > CrusaderSettings.SweepAttackAoECount || TargetUtil.EliteOrTrashInRange(18f) || Player.PrimaryResource > 60 || CurrentTarget.CountUnitsBehind(12f) > 1);
        }

        private static bool CanCastFistOfHeavens()
        {
            return CombatBase.CanCast(SNOPower.X1_Crusader_FistOfTheHeavens) &&
                (TargetUtil.ClusterExists(8f, 8f, 2) || TargetUtil.EliteOrTrashInRange(8f) || Player.PrimaryResourcePct > 0.5);
        }

        private static bool CanCastBlessedShield(bool hasPiercingShield)
        {
            return CombatBase.CanCast(SNOPower.X1_Crusader_BlessedShield) && !hasPiercingShield && (TargetUtil.ClusterExists(14f, 3) || TargetUtil.EliteOrTrashInRange(14f));
        }

        private static bool CanCastBlessedShieldPiercingShield(bool hasPiercingShield)
        {
            return CombatBase.CanCast(SNOPower.X1_Crusader_BlessedShield) && hasPiercingShield && (TargetUtil.ClusterExists(14f, 3) || TargetUtil.EliteOrTrashInRange(14f));
        }

        private static bool CanCastPhalanx()
        {
            return CombatBase.CanCast(SNOPower.x1_Crusader_Phalanx3) && (TargetUtil.ClusterExists(15f, 3) || TargetUtil.EliteOrTrashInRange(15f));
        }

        private static bool CanCastSteedCharge()
        {
            return CombatBase.CanCast(SNOPower.X1_Crusader_SteedCharge)
                && (TargetUtil.ClusterExists(CrusaderSettings.SteedChargeMinRange, 3) || TargetUtil.EliteOrTrashInRange(CrusaderSettings.SteedChargeMinRange));
        }

        private static bool CanCastCondemn()
        {
            return CombatBase.CanCast(SNOPower.X1_Crusader_Condemn) && (TargetUtil.EliteOrTrashInRange(16f) || TargetUtil.AnyMobsInRange(15f, CrusaderSettings.CondemnAoECount));
        }

        private static bool CanCastHeavensFury()
        {
            return CombatBase.CanCast(SNOPower.X1_Crusader_HeavensFury3) && (TargetUtil.EliteOrTrashInRange(16f) || TargetUtil.ClusterExists(15f, CrusaderSettings.HeavensFuryAoECount));
        }

        private static bool CanCastFallingSword()
        {
            return CombatBase.CanCast(SNOPower.X1_Crusader_FallingSword) && (CurrentTarget.IsEliteRareUnique || TargetUtil.ClusterExists(15f, CrusaderSettings.FallingSwordAoECount));
        }

        private static bool CanCastBombardment()
        {
            return CombatBase.CanCast(SNOPower.X1_Crusader_Bombardment) && TargetUtil.EliteOrTrashInRange(35f) &&
                (CurrentTarget.IsEliteRareUnique || TargetUtil.ClusterExists(15f, CrusaderSettings.BombardmentAoECount));
        }

        private static bool CanCastAkaratsChampion()
        {
            return CombatBase.CanCast(SNOPower.X1_Crusader_AkaratsChampion) && (TargetUtil.EliteOrTrashInRange(16f) || Player.CurrentHealthPct <= 0.25);
        }

        private static bool CanCastConsecration()
        {
            return CombatBase.CanCast(SNOPower.X1_Crusader_Consecration) && Player.CurrentHealthPct <= CrusaderSettings.ConsecrationHpPct;
        }

        private static bool CanCastIronSkin()
        {
            return CombatBase.CanCast(SNOPower.X1_Crusader_IronSkin) && ((Player.CurrentHealthPct <= CrusaderSettings.IronSkinHpPct) ||
                (CurrentTarget.IsBossOrEliteRareUnique && CurrentTarget.RadiusDistance <= 10f));
        }

        private static bool CanCastShieldGlare()
        {
            return CombatBase.CanCast(SNOPower.X1_Crusader_ShieldGlare) && ((CurrentTarget.IsBossOrEliteRareUnique && CurrentTarget.RadiusDistance <= 15f) ||
                                TargetUtil.UnitsPlayerFacing(16f) >= CrusaderSettings.ShieldGlareAoECount);
        }

        private static bool CanCastJudgement()
        {
            return CombatBase.CanCast(SNOPower.X1_Crusader_Judgment) && (TargetUtil.EliteOrTrashInRange(16f) || TargetUtil.ClusterExists(15f, CrusaderSettings.JudgmentAoECount));
        }

        private static TrinityPower DestroyObjectPower
        {
            get
            {
                // Sweep Attack
                if (CombatBase.CanCast(SNOPower.X1_Crusader_SweepAttack))
                {
                    return new TrinityPower(SNOPower.X1_Crusader_SweepAttack, 15f, CurrentTarget.ACDGuid);
                }

                // Justice
                if (CombatBase.CanCast(SNOPower.X1_Crusader_Justice))
                {
                    return new TrinityPower(SNOPower.X1_Crusader_Justice, 45f, CurrentTarget.ACDGuid);
                }

                // Smite
                if (CombatBase.CanCast(SNOPower.X1_Crusader_Smite))
                {
                    return new TrinityPower(SNOPower.X1_Crusader_Smite, 15f, CurrentTarget.ACDGuid);
                }

                // Slash
                if (CombatBase.CanCast(SNOPower.X1_Crusader_Slash))
                {
                    return new TrinityPower(SNOPower.X1_Crusader_Slash, 5f, CurrentTarget.ACDGuid);
                }

                // Punish
                if (CombatBase.CanCast(SNOPower.X1_Crusader_Punish))
                {
                    return new TrinityPower(SNOPower.X1_Crusader_Punish, 5f, CurrentTarget.ACDGuid);
                }
                return CombatBase.DefaultPower;
            }
        }



    }
}
