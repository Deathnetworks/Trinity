
using System.Linq;
using Trinity.Reference;
using Zeta.Common;
using Zeta.Game;
using Zeta.Game.Internals.Actors;

namespace Trinity.Combat.Abilities
{
    public class CrusaderCombat : CombatBase
    {
        public static Config.Combat.CrusaderSetting CrusaderSettings
        {
            get { return Trinity.Settings.Combat.Crusader; }
        }

        public static TrinityPower GetPower()
        {

            TrinityPower power = null;

            if (!UseOOCBuff && IsCurrentlyAvoiding)
            {
                if (CanCastSteedChargeOutOfCombat())
                {
                    return new TrinityPower(SNOPower.X1_Crusader_SteedCharge);
                }
            }

            if (!UseOOCBuff && !IsCurrentlyAvoiding)
            {
                /*
                 *  Laws for Active Buff
                 */

                //There are doubles?? not sure which is correct yet
                // Laws of Hope
                // Laws of Hope2
                if (CanCast(SNOPower.X1_Crusader_LawsOfHope2) && (TargetUtil.EliteOrTrashInRange(16f) || TargetUtil.AnyMobsInRange(15f, 5)))
                {
                    return new TrinityPower(SNOPower.X1_Crusader_LawsOfHope2);
                }

                // LawsOfJustice
                // LawsOfJustice2
                if (CanCast(SNOPower.X1_Crusader_LawsOfJustice2) && (TargetUtil.EliteOrTrashInRange(16f) || Player.CurrentHealthPct <= CrusaderSettings.LawsOfJusticeHpPct || TargetUtil.AnyMobsInRange(15f, 5)))
                {
                    return new TrinityPower(SNOPower.X1_Crusader_LawsOfJustice2);
                }

                // LawsOfValor
                // LawsOfValor2
                if (CanCast(SNOPower.X1_Crusader_LawsOfValor2) && (TargetUtil.EliteOrTrashInRange(16f) || TargetUtil.AnyMobsInRange(15f, 5) || Settings.Combat.Crusader.SpamLawsOfValor))
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
                    return new TrinityPower(SNOPower.X1_Crusader_ShieldGlare, 15f, TargetUtil.GetBestPiercePoint(15f));
                }

                // Iron Skin
                if (CanCastIronSkin())
                {
                    return new TrinityPower(SNOPower.X1_Crusader_IronSkin);
                }

                // Provoke
                if (CanCast(SNOPower.X1_Crusader_Provoke) && (TargetUtil.EliteOrTrashInRange(15f) || TargetUtil.AnyMobsInRange(15f, CrusaderSettings.ProvokeAoECount)))
                {
                    return new TrinityPower(SNOPower.X1_Crusader_Provoke);
                }

                // Consecration
                bool hasSGround = CacheData.Hotbar.ActiveSkills.Any(s => s.Power == SNOPower.X1_Crusader_Consecration && s.RuneIndex == 3);
                if ((!hasSGround && CanCastConsecration()) ||
                   (hasSGround && CanCastConsecration() && (TargetUtil.AnyMobsInRange(15f, 5) || TargetUtil.IsEliteTargetInRange(15f))))
                {
                    return new TrinityPower(SNOPower.X1_Crusader_Consecration);
                }
                // Akarats when off Cooldown
                if (CrusaderSettings.SpamAkarats && CanCast(SNOPower.X1_Crusader_AkaratsChampion))
                {
                    return new TrinityPower(SNOPower.X1_Crusader_AkaratsChampion);
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
                        TargetUtil.GetBestClusterPoint();
                    return new TrinityPower(SNOPower.X1_Crusader_Bombardment, 16f, bestPoint);
                }

                // FallingSword
                if (CanCastFallingSword())
                {
                    return new TrinityPower(SNOPower.X1_Crusader_FallingSword, 16f, TargetUtil.GetBestClusterPoint(15f, 65f));
                }

                // HeavensFury
                bool hasAkkhan = (Sets.ArmorOfAkkhan.IsThirdBonusActive);
                if (CanCastHeavensFury() && !hasAkkhan)
                {
                    return new TrinityPower(SNOPower.X1_Crusader_HeavensFury3, 65f, TargetUtil.GetBestClusterPoint());
                }

                if (CanCastHeavensFuryHolyShotgun() && hasAkkhan)
                {
                    return new TrinityPower(SNOPower.X1_Crusader_HeavensFury3, 15f, CurrentTarget.Position);
                }

                // Condemn
                if (CanCastCondemn())
                {
                    return new TrinityPower(SNOPower.X1_Crusader_Condemn);
                }

                // Phalanx
                if (CanCastPhalanx())
                {
                    var bestPierceTarget = TargetUtil.GetBestPierceTarget(45f);
                    if (bestPierceTarget != null)
                        return new TrinityPower(SNOPower.x1_Crusader_Phalanx3, 45f, bestPierceTarget.ACDGuid);
                }
                if (CanCastPhalanxStampede())
                {
                    var bestPierceTarget = TargetUtil.GetBestPierceTarget(35f);
                    if (bestPierceTarget != null)
                        return new TrinityPower(SNOPower.x1_Crusader_Phalanx3, 35f, bestPierceTarget.ACDGuid);
                }

                // Blessed Shield : Piercing Shield
                bool hasPiercingShield = CacheData.Hotbar.ActiveSkills.Any(s => s.Power == SNOPower.X1_Crusader_BlessedShield && s.RuneIndex == 5);
                if (CanCastBlessedShieldPiercingShield(hasPiercingShield))
                {
                    var bestPierceTarget = TargetUtil.GetBestPierceTarget(45f);
                    if (bestPierceTarget != null)
                        return new TrinityPower(SNOPower.X1_Crusader_BlessedShield, 14f, bestPierceTarget.ACDGuid);
                }

                // Blessed Shield
                if (CanCastBlessedShield() && !hasPiercingShield)
                {
                    return new TrinityPower(SNOPower.X1_Crusader_BlessedShield, 14f, TargetUtil.GetBestClusterUnit().ACDGuid);
                }

                // Fist of Heavens
                if (CanCastFistOfHeavens())
                {
                    float range = Settings.Combat.Crusader.FistOfHeavensDist;
                    float clusterRange = 8f;
                    if (Runes.Crusader.DivineWell.IsActive)
                        clusterRange = 18f;
                    return new TrinityPower(SNOPower.X1_Crusader_FistOfTheHeavens, range, TargetUtil.GetBestClusterUnit(clusterRange).Position);
                }

                // Blessed Hammer
                if (CanCastBlessedHammer())
                {
                    return new TrinityPower(SNOPower.X1_Crusader_BlessedHammer, 20f, TargetUtil.GetBestClusterUnit(8f, 20f).Position);
                }

                // Bastion Of Will require primary usage
                if (IsBastionsPrimaryBuffWillExpire)
                {
                    // Justice
                    if (CanCast(SNOPower.X1_Crusader_Justice))
                        return new TrinityPower(SNOPower.X1_Crusader_Justice, 7f, CurrentTarget.ACDGuid);

                    // Smite
                    if (CanCast(SNOPower.X1_Crusader_Smite))
                        return new TrinityPower(SNOPower.X1_Crusader_Smite, 15f, TargetUtil.GetBestClusterUnit(15f, 15f).ACDGuid);

                    // Slash
                    if (CanCast(SNOPower.X1_Crusader_Slash))
                        return new TrinityPower(SNOPower.X1_Crusader_Slash, 15f, TargetUtil.GetBestClusterUnit(5f, 8f).ACDGuid);

                    // Punish
                    if (CanCast(SNOPower.X1_Crusader_Punish))
                        return new TrinityPower(SNOPower.X1_Crusader_Punish, 7f, CurrentTarget.ACDGuid);
                }

                // Provoke
                if (CanCast(SNOPower.X1_Crusader_Provoke) && TargetUtil.AnyMobsInRange(15f, 4))
                {
                    return new TrinityPower(SNOPower.X1_Crusader_Provoke);
                }

                // Shield Bash
                if (CanCast(SNOPower.X1_Crusader_ShieldBash2))
                {
                    return new TrinityPower(SNOPower.X1_Crusader_ShieldBash2, 65f, TargetUtil.GetBestPiercePoint(65f));
                }

                // Blessed Hammer, spin outwards 
                // Limitless rune can spawn new hammers
                if (CanCast(SNOPower.X1_Crusader_BlessedHammer) && (TargetUtil.EliteOrTrashInRange(20f) || TargetUtil.AnyTrashInRange(20f, 3)))
                {
                    return new TrinityPower(SNOPower.X1_Crusader_BlessedHammer);
                }

                // Sweep Attack
                if (CanCastSweepAttack())
                {
                    return new TrinityPower(SNOPower.X1_Crusader_SweepAttack, 5f, TargetUtil.GetBestClusterUnit(18f, 18f).ACDGuid);
                }

                /*
                 *  Basic Attacks
                 */


                //Blessed Shield
                if (CanCast(SNOPower.X1_Crusader_BlessedShield))
                {
                    return new TrinityPower(SNOPower.X1_Crusader_BlessedShield, 14f, CurrentTarget.ACDGuid);
                }

                // Justice
                if (CanCast(SNOPower.X1_Crusader_Justice))
                {
                    return new TrinityPower(SNOPower.X1_Crusader_Justice, 7f, CurrentTarget.ACDGuid);
                }

                // Smite
                if (CanCast(SNOPower.X1_Crusader_Smite))
                {
                    return new TrinityPower(SNOPower.X1_Crusader_Smite, 15f, TargetUtil.GetBestClusterUnit(15f, 15f).ACDGuid);
                }

                // Slash
                if (CanCast(SNOPower.X1_Crusader_Slash))
                {
                    return new TrinityPower(SNOPower.X1_Crusader_Slash, 15f, TargetUtil.GetBestClusterUnit(5f, 8f).ACDGuid);
                }

                // Punish
                if (CanCast(SNOPower.X1_Crusader_Punish))
                {
                    return new TrinityPower(SNOPower.X1_Crusader_Punish, 7f, CurrentTarget.ACDGuid);
                }
            }

            // Buffs
            if (UseOOCBuff)
            {
                if (CanCast(SNOPower.X1_Crusader_SteedCharge) && CrusaderSettings.SteedChargeOOC && ZetaDia.Me.Movement.SpeedXY > 0)
                {
                    return new TrinityPower(SNOPower.X1_Crusader_SteedCharge);
                }

                /*
                 *  Laws
                 */

                //There are doubles?? not sure which is correct yet
                // Laws of Hope2
                if (CanCast(SNOPower.X1_Crusader_LawsOfHope2) && Player.CurrentHealthPct <= CrusaderSettings.LawsOfHopeHpPct)
                {
                    return new TrinityPower(SNOPower.X1_Crusader_LawsOfHope2);
                }

                // LawsOfJustice2
                if (CanCast(SNOPower.X1_Crusader_LawsOfJustice2) && (TargetUtil.EliteOrTrashInRange(16f) || Player.CurrentHealthPct <= CrusaderSettings.LawsOfJusticeHpPct))
                {
                    return new TrinityPower(SNOPower.X1_Crusader_LawsOfJustice2);
                }

                // LawsOfValor2
                if (CanCast(SNOPower.X1_Crusader_LawsOfValor2) && TargetUtil.EliteOrTrashInRange(16f))
                {
                    return new TrinityPower(SNOPower.X1_Crusader_LawsOfValor2);
                }
            }

            // Default Attacks
            if (IsNull(null))
                power = DefaultPower;

            return power;
        }

        private static bool CanCastSweepAttack()
        {
            return CanCast(SNOPower.X1_Crusader_SweepAttack) &&
                (TargetUtil.UnitsPlayerFacing(18f) > CrusaderSettings.SweepAttackAoECount || TargetUtil.EliteOrTrashInRange(18f) || Player.PrimaryResource > 60 || CurrentTarget.CountUnitsBehind(12f) > 1);
        }

        private static bool CanCastFistOfHeavens()
        {
            return CanCast(SNOPower.X1_Crusader_FistOfTheHeavens, CanCastFlags.NoTimer);
        }

        private static bool CanCastBlessedHammer()
        {
            return CanCast(SNOPower.X1_Crusader_BlessedHammer) &&
                (TargetUtil.ClusterExists(8f, 8f, 1) || TargetUtil.EliteOrTrashInRange(8f) || Player.PrimaryResourcePct > 0.5);
        }

        private static bool CanCastBlessedShield()
        {
            return CanCast(SNOPower.X1_Crusader_BlessedShield) && (TargetUtil.ClusterExists(14f, 3) || TargetUtil.EliteOrTrashInRange(14f));
        }

        private static bool CanCastBlessedShieldPiercingShield(bool hasPiercingShield)
        {
            return CanCast(SNOPower.X1_Crusader_BlessedShield) && hasPiercingShield && (TargetUtil.ClusterExists(14f, 3) || TargetUtil.EliteOrTrashInRange(14f));
        }

        private static bool CanCastPhalanx()
        {
            return CanCast(SNOPower.x1_Crusader_Phalanx3) && (TargetUtil.ClusterExists(45f, 3) || TargetUtil.EliteOrTrashInRange(45f));
        }

        private static bool CanCastPhalanxStampede()
        {
            return (Legendary.UnrelentingPhalanx.IsEquipped && CanCast(SNOPower.x1_Crusader_Phalanx3) && TargetUtil.AnyMobsInRange(35f, 1) && Runes.Crusader.Stampede.IsActive);
        }

        private static bool CanCastSteedChargeOutOfCombat()
        {
            return CanCast(SNOPower.X1_Crusader_SteedCharge) && CrusaderSettings.SteedChargeOOC && Player.MovementSpeed > 0 && !Player.IsInTown && ZetaDia.Me.LoopingAnimationEndTime == 0;
        }

        private static bool CanCastCondemn()
        {
            return CanCast(SNOPower.X1_Crusader_Condemn) && (TargetUtil.EliteOrTrashInRange(16f) || TargetUtil.AnyMobsInRange(15f, CrusaderSettings.CondemnAoECount))
                && (!Legendary.FrydehrsWrath.IsEquipped || Player.PrimaryResource >= 40);
        }

        private static bool CanCastHeavensFury()
        {
            return (CanCast(SNOPower.X1_Crusader_HeavensFury3) && (TargetUtil.EliteOrTrashInRange(65f) || TargetUtil.ClusterExists(15f, 65f, CrusaderSettings.HeavensFuryAoECount)));
        }

        private static bool CanCastHeavensFuryHolyShotgun()
        {
            return (CanCast(SNOPower.X1_Crusader_HeavensFury3) && TargetUtil.AnyMobsInRange(15f, 1) && Runes.Crusader.FiresOfHeaven.IsActive);
        }

        private static bool CanCastFallingSword()
        {
            return CanCast(SNOPower.X1_Crusader_FallingSword) && (CurrentTarget.IsEliteRareUnique || TargetUtil.ClusterExists(15f, 65f, CrusaderSettings.FallingSwordAoECount));
        }

        private static bool CanCastBombardment()
        {
            return CanCast(SNOPower.X1_Crusader_Bombardment) && TargetUtil.EliteOrTrashInRange(35f) &&
                (CurrentTarget.IsEliteRareUnique || TargetUtil.ClusterExists(15f, CrusaderSettings.BombardmentAoECount));
        }

        private static bool CanCastAkaratsChampion()
        {
            return CanCast(SNOPower.X1_Crusader_AkaratsChampion) && (TargetUtil.EliteOrTrashInRange(16f) || Player.CurrentHealthPct <= 0.25);
        }

        private static bool CanCastConsecration()
        {
            return CanCast(SNOPower.X1_Crusader_Consecration) && Player.CurrentHealthPct <= CrusaderSettings.ConsecrationHpPct;
        }

        private static bool CanCastIronSkin()
        {
            return CanCast(SNOPower.X1_Crusader_IronSkin) && ((Player.CurrentHealthPct <= CrusaderSettings.IronSkinHpPct) ||
                (CurrentTarget.IsBossOrEliteRareUnique && CurrentTarget.RadiusDistance <= 10f));
        }

        private static bool CanCastShieldGlare()
        {
            return CanCast(SNOPower.X1_Crusader_ShieldGlare) && ((CurrentTarget.IsBossOrEliteRareUnique && CurrentTarget.RadiusDistance <= 15f) ||
                                TargetUtil.UnitsPlayerFacing(16f) >= CrusaderSettings.ShieldGlareAoECount);
        }

        private static bool CanCastJudgement()
        {
            return CanCast(SNOPower.X1_Crusader_Judgment) && (TargetUtil.EliteOrTrashInRange(16f) || TargetUtil.ClusterExists(15f, CrusaderSettings.JudgmentAoECount));
        }

        private static TrinityPower DestroyObjectPower
        {
            get
            {
                // Sweep Attack
                if (CanCast(SNOPower.X1_Crusader_SweepAttack))
                {
                    return new TrinityPower(SNOPower.X1_Crusader_SweepAttack, 15f, CurrentTarget.ACDGuid);
                }

                //Blessed Shield
                if (CanCast(SNOPower.X1_Crusader_BlessedShield))
                {
                    return new TrinityPower(SNOPower.X1_Crusader_BlessedShield, 14f, CurrentTarget.ACDGuid);
                }

                // Justice
                if (CanCast(SNOPower.X1_Crusader_Justice))
                {
                    return new TrinityPower(SNOPower.X1_Crusader_Justice, 45f, CurrentTarget.ACDGuid);
                }

                // Smite
                if (CanCast(SNOPower.X1_Crusader_Smite))
                {
                    return new TrinityPower(SNOPower.X1_Crusader_Smite, 15f, CurrentTarget.ACDGuid);
                }

                // Slash
                if (CanCast(SNOPower.X1_Crusader_Slash))
                {
                    return new TrinityPower(SNOPower.X1_Crusader_Slash, 5f, CurrentTarget.ACDGuid);
                }

                // Punish
                if (CanCast(SNOPower.X1_Crusader_Punish))
                {
                    return new TrinityPower(SNOPower.X1_Crusader_Punish, 5f, CurrentTarget.ACDGuid);
                }
                return DefaultPower;
            }
        }



    }
}
