
using System.Linq;
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
            if (!UseOOCBuff && !IsCurrentlyAvoiding)
            {
                // Judgement
                if (CanCastJudgement())
                {
                    return new TrinityPower(SNOPower.X1_Crusader_Judgment, 20f, TargetUtil.GetBestClusterPoint(20f));
                }

                // Shield Glare
                if (CanCastShieldGlare())
                {
                    return new TrinityPower(SNOPower.X1_Crusader_ShieldGlare, 15f, CurrentTarget.ACDGuid);
                }

                // Iron Skin
                if (CanCastIronSkin())
                {
                    return new TrinityPower(SNOPower.X1_Crusader_IronSkin);
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
                    return new TrinityPower(SNOPower.X1_Crusader_Bombardment, 16f, TargetUtil.GetBestClusterPoint(15f));
                }

                // FallingSword
                if (CanCastFallingSword())
                {
                    return new TrinityPower(SNOPower.X1_Crusader_FallingSword, 16f, TargetUtil.GetBestClusterPoint(15f));
                }

                // HeavensFury
                if (CanCastHeavensFury())
                {
                    return new TrinityPower(SNOPower.X1_Crusader_HeavensFury3, 16f, TargetUtil.GetBestClusterPoint(15f));
                }

                // Condemn
                if (CanCastCondemn())
                {
                    return new TrinityPower(SNOPower.X1_Crusader_Condemn, 16f, TargetUtil.GetBestClusterPoint(15f));
                }

                // Steed Charge through trash
                if (CanCastSteedCharge())
                {
                    return new TrinityPower(SNOPower.X1_Crusader_SteedCharge);
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
                    return new TrinityPower(SNOPower.X1_Crusader_BlessedShield, 14f, TargetUtil.GetBestPierceTarget(45f).ACDGuid);
                }

                // Blessed Shield
                if (CanCastBlessedShield(hasPiercingShield))
                {
                    return new TrinityPower(SNOPower.X1_Crusader_BlessedShield, 14f, CurrentTarget.ACDGuid);
                }

                // Fist of Heavens
                if (CanCastFistOfHeavens())
                {
                    return new TrinityPower(SNOPower.X1_Crusader_FistOfTheHeavens);
                }

                // Shield Bash
                if (CombatBase.CanCast(SNOPower.X1_Crusader_ShieldBash2))
                {
                    return new TrinityPower(SNOPower.X1_Crusader_ShieldBash2, 15f, TargetUtil.GetBestClusterUnit(15f, 15f, 1).ACDGuid);
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


            }

            // Buffs
            if (UseOOCBuff)
            {
                if (CombatBase.CanCast(SNOPower.X1_Crusader_BlessedHammer) && !GetHasBuff(SNOPower.X1_Crusader_BlessedHammer))
                {
                    return new TrinityPower(SNOPower.X1_Crusader_BlessedHammer);
                }

                /*
                 *  Laws
                 */

                //There are doubles?? not sure which is correct yet
                // Laws of Hope
                if (CombatBase.CanCast(SNOPower.X1_Crusader_LawsOfHope) && Player.CurrentHealthPct <= CrusaderSettings.LawsOfHopeHpPct)
                {
                    return new TrinityPower(SNOPower.X1_Crusader_LawsOfHope);
                }
                // Laws of Hope2
                if (CombatBase.CanCast(SNOPower.X1_Crusader_LawsOfHope2) && Player.CurrentHealthPct <= CrusaderSettings.LawsOfHopeHpPct)
                {
                    return new TrinityPower(SNOPower.X1_Crusader_LawsOfHope2);
                }

                // LawsOfJustice
                if (CombatBase.CanCast(SNOPower.X1_Crusader_LawsOfJustice) && (TargetUtil.EliteOrTrashInRange(16f) || Player.CurrentHealthPct <= CrusaderSettings.LawsOfJusticeHpPct))
                {
                    return new TrinityPower(SNOPower.X1_Crusader_LawsOfJustice);
                }
                // LawsOfJustice2
                if (CombatBase.CanCast(SNOPower.X1_Crusader_LawsOfJustice2) && (TargetUtil.EliteOrTrashInRange(16f) || Player.CurrentHealthPct <= CrusaderSettings.LawsOfJusticeHpPct))
                {
                    return new TrinityPower(SNOPower.X1_Crusader_LawsOfJustice2);
                }

                // LawsOfValor
                if (CombatBase.CanCast(SNOPower.X1_Crusader_LawsOfValor) && TargetUtil.EliteOrTrashInRange(16f))
                {
                    return new TrinityPower(SNOPower.X1_Crusader_LawsOfValor);
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
            return CombatBase.CanCast(SNOPower.X1_Crusader_SweepAttack) && TargetUtil.UnitsPlayerFacing(18f) > CrusaderSettings.SweepAttackAoECount;
        }

        private static bool CanCastFistOfHeavens()
        {
            return CombatBase.CanCast(SNOPower.X1_Crusader_FistOfTheHeavens) && TargetUtil.ClusterExists(8f, 8f, 2);
        }

        private static bool CanCastBlessedShield(bool hasPiercingShield)
        {
            return CombatBase.CanCast(SNOPower.X1_Crusader_BlessedShield) && !hasPiercingShield && TargetUtil.ClusterExists(14f, 3);
        }

        private static bool CanCastBlessedShieldPiercingShield(bool hasPiercingShield)
        {
            return CombatBase.CanCast(SNOPower.X1_Crusader_BlessedShield) && hasPiercingShield && TargetUtil.ClusterExists(14f, 3);
        }

        private static bool CanCastPhalanx()
        {
            return CombatBase.CanCast(SNOPower.x1_Crusader_Phalanx3) && TargetUtil.ClusterExists(15f, 3);
        }

        private static bool CanCastSteedCharge()
        {
            return CombatBase.CanCast(SNOPower.X1_Crusader_SteedCharge) && TargetUtil.ClusterExists(CrusaderSettings.SteedChargeMinRange, 3);
        }

        private static bool CanCastCondemn()
        {
            return CombatBase.CanCast(SNOPower.X1_Crusader_Condemn) && (TargetUtil.EliteOrTrashInRange(16f) || TargetUtil.ClusterExists(15f, CrusaderSettings.CondemnAoECount));
        }

        private static bool CanCastHeavensFury()
        {
            return CombatBase.CanCast(SNOPower.X1_Crusader_HeavensFury3) && (TargetUtil.EliteOrTrashInRange(16f) || TargetUtil.ClusterExists(15f, CrusaderSettings.HeavensFuryAoECount));
        }

        private static bool CanCastFallingSword()
        {
            return CombatBase.CanCast(SNOPower.X1_Crusader_FallingSword) && (TargetUtil.EliteOrTrashInRange(16f) || TargetUtil.ClusterExists(15f, CrusaderSettings.FallingSwordAoECount));
        }

        private static bool CanCastBombardment()
        {
            return CombatBase.CanCast(SNOPower.X1_Crusader_Bombardment) && (TargetUtil.EliteOrTrashInRange(16f) || TargetUtil.ClusterExists(15f, CrusaderSettings.BombardmentAoECount));
        }

        private static bool CanCastAkaratsChampion()
        {
            return CombatBase.CanCast(SNOPower.X1_Crusader_AkaratsChampion) && (TargetUtil.EliteOrTrashInRange(16f) || Player.CurrentHealthPct <= 0.30);
        }

        private static bool CanCastConsecration()
        {
            return CombatBase.CanCast(SNOPower.X1_Crusader_Consecration) && Player.CurrentHealthPct <= CrusaderSettings.ConsecrationHpPct;
        }

        private static bool CanCastIronSkin()
        {
            return CombatBase.CanCast(SNOPower.X1_Crusader_IronSkin) && Player.CurrentHealthPct <= CrusaderSettings.IronSkinHpPct;
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
