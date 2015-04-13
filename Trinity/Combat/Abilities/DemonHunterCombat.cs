using System;
using System.Linq;
using Trinity.Config.Combat;
using Trinity.DbProvider;
using Trinity.Reference;
using Trinity.Technicals;
using Zeta.Common;
using Zeta.Game;
using Zeta.Game.Internals.Actors;
using Logger = Trinity.Technicals.Logger;

namespace Trinity.Combat.Abilities
{
    public class DemonHunterCombat : CombatBase
    {
        #region Fields
        public static DemonHunterSetting DHSettings
        {
            get { return Trinity.Settings.Combat.DemonHunter; }
        }
        private static bool DemonHunter_HasNoPrimary()
        {
            return !(Hotbar.Contains(SNOPower.X1_DemonHunter_EvasiveFire) ||
                Hotbar.Contains(SNOPower.DemonHunter_Bolas) ||
                Hotbar.Contains(SNOPower.X1_DemonHunter_EntanglingShot) ||
                Hotbar.Contains(SNOPower.DemonHunter_Grenades) ||
                Hotbar.Contains(SNOPower.DemonHunter_HungeringArrow));
        }
        private static int MaxSentryCount
        {
            get
            {
                return 2 + (Legendary.BombardiersRucksack.IsEquipped ? 2 : 0) + (Passives.DemonHunter.CustomEngineering.IsActive ? 1 : 0);
            }
        }
        private static bool IsSentryOnTarget(int minCount = 1, float range = 55f)
        {
            if (!CacheData.SentryTurret.Any() || CurrentTarget == null)
                return false;

            return CacheData.SentryTurret.Count(s => s.Position.Distance2D(CurrentTarget.Position) <= range && CurrentTarget.IsInLineOfSightOfPoint(s.Position)) >= minCount;
        }
        private static bool IsSentryOnPosition(Vector3 loc, int minCount = 1, float range = 55f)
        {
            if (loc == new Vector3())
                return false;

            if (!CacheData.SentryTurret.Any())
                return false;

            return CacheData.SentryTurret.Count(s => s.Position.Distance2D(CurrentTarget.Position) <= range && NavHelper.CanRayCast(s.Position, loc, true)) >= minCount;
        }
        private static float _RangedAttackRange = -1f;
        private static float RangedAttackRange
        {
            get
            {
                if (_RangedAttackRange >= 0f)
                    return _RangedAttackRange;

                if (Sets.EmbodimentOfTheMarauder.IsMaxBonusActive && IsSentryOnTarget(2))
                    _RangedAttackRange = 0f;
                else
                    _RangedAttackRange = DHSettings.RangedAttackRange;

                return _RangedAttackRange;
            }
        }
        private static Vector3 LastZeiOfStoneLocation = new Vector3();
        #endregion

        /// <summary>
        /// Get combat/avoidance buff/power
        /// </summary>
        /// <returns></returns>
        public static TrinityPower GetPower()
        {
            TrinityPower power = null;

            // Destructible
            if (UseDestructiblePower)
            {
                return GetDemonHunterDestroyPower();
            }

            // Buffs
            if ((!Player.IsInCombat || UseOOCBuff) && !IsCurrentlyAvoiding)
            {
                power = GetBuffPower();
                if (!IsNull(power)) { return power; }
            }

            // In Combat, Avoiding
            if (IsCurrentlyAvoiding)
            {
                power = GetCombatAvoidancePower();
                if (!IsNull(power)) { return power; }
            }

            if (Player.IsRooted || Player.IsIncapacitated)
            {
                power = GetCombatAvoidancePower();
                if (!IsNull(power)) { return power; }
            }

            if (IsCurrentlyAvoiding)
                return DefaultPower;

            // Marauder cast/move routine
            if ((CurrentTarget != null || Player.IsInCombat) && Sets.EmbodimentOfTheMarauder.IsMaxBonusActive)
            {
                power = RunMarauderRoutine();
                if (!IsNull(power)) { return power; }
            }

            // In combat, Not Avoiding
            if (CurrentTarget != null)
            {
                power = GetCombatBuffPower();
                if (!IsNull(power)) { return power; }

                if (CurrentTarget.IsUnit)
                {
                    // Bastion Of Will require primary usage
                    if (Enemies.Nearby.Units.Any() && IsBastionsPrimaryBuffWillExpired)
                    {
                        power = GetPrimaryPower();
                        if (!IsNull(power)) { return power; }
                    }

                    power = GetCombatPower();
                    if (!IsNull(power)) { return power; }

                    power = GetPrimaryPower();
                    if (!IsNull(power)) { return power; }
                }
            }

            // Default attacks
            return DefaultPower;
        }

        /// <summary>
        /// Return primary power
        /// </summary>
        /// <returns></returns>
        private static TrinityPower GetPrimaryPower()
        {
            if (Sets.EmbodimentOfTheMarauder.IsMaxBonusActive && AreaHasCastCriteria(SentryCastSkillsCastArea))
            {
                if (Skills.DemonHunter.EvasiveFire.IsActive && !Skills.DemonHunter.EvasiveFire.Cast(SentryCastSkillsCastArea.Position))
                    return new TrinityPower(SNOPower.X1_DemonHunter_EvasiveFire, RangedAttackRange, SentryCastSkillsCastArea.Position); ;

                if (Skills.DemonHunter.HungeringArrow.IsActive && !Skills.DemonHunter.HungeringArrow.Cast(SentryCastSkillsCastArea.Position))
                    return new TrinityPower(SNOPower.DemonHunter_HungeringArrow, RangedAttackRange, SentryCastSkillsCastArea.Position);

                if (Skills.DemonHunter.EntanglingShot.IsActive && !Skills.DemonHunter.EntanglingShot.Cast(SentryCastSkillsCastArea.Position))
                    return new TrinityPower(SNOPower.X1_DemonHunter_EntanglingShot, RangedAttackRange, SentryCastSkillsCastArea.Position);

                if (Skills.DemonHunter.Bolas.IsActive && !Skills.DemonHunter.Bolas.Cast(SentryCastSkillsCastArea.Position))
                    return new TrinityPower(SNOPower.DemonHunter_Bolas, RangedAttackRange, SentryCastSkillsCastArea.Position);

                if (Skills.DemonHunter.Grenade.IsActive && !Skills.DemonHunter.Grenade.Cast(SentryCastSkillsCastArea.Position))
                    return new TrinityPower(SNOPower.DemonHunter_Grenades, RangedAttackRange, SentryCastSkillsCastArea.Position);
            }
            else if (Sets.EmbodimentOfTheMarauder.IsMaxBonusActive && SentryCastSkillsCastArea != null && SentryCastSkillsCastArea.Position != Vector3.Zero)
            {
                //MoveToSentryCastSkillsCastArea(SentryCastSkillsCastArea);

                if (Skills.DemonHunter.EvasiveFire.IsActive)
                    return new TrinityPower(SNOPower.X1_DemonHunter_EvasiveFire, RangedAttackRange, SentryCastSkillsCastArea.Position); ;

                if (Skills.DemonHunter.HungeringArrow.IsActive)
                    return new TrinityPower(SNOPower.DemonHunter_HungeringArrow, RangedAttackRange, SentryCastSkillsCastArea.Position);

                if (Skills.DemonHunter.EntanglingShot.IsActive)
                    return new TrinityPower(SNOPower.X1_DemonHunter_EntanglingShot, RangedAttackRange, SentryCastSkillsCastArea.Position);

                if (Skills.DemonHunter.Bolas.IsActive)
                    return new TrinityPower(SNOPower.DemonHunter_Bolas, RangedAttackRange, SentryCastSkillsCastArea.Position);

                if (Skills.DemonHunter.Grenade.IsActive)
                    return new TrinityPower(SNOPower.DemonHunter_Grenades, RangedAttackRange, SentryCastSkillsCastArea.Position);
            }

            // Evasive Fire
            if (Hotbar.Contains(SNOPower.X1_DemonHunter_EvasiveFire))
            {
                float range = (Player.PrimaryResourceMissing > 5 || Sets.EmbodimentOfTheMarauder.IsMaxBonusActive) ? RangedAttackRange : 10f;
                return new TrinityPower(SNOPower.X1_DemonHunter_EvasiveFire, RangedAttackRange, SentryCastSkillsCastArea.Position);
            }

            // Hungering Arrow
            if (Hotbar.Contains(SNOPower.DemonHunter_HungeringArrow))
            {
                return new TrinityPower(SNOPower.DemonHunter_HungeringArrow, RangedAttackRange, SentryCastSkillsCastArea.Position);
            }

            // Entangling shot
            if (Skills.DemonHunter.EntanglingShot.IsActive)
            {
                return new TrinityPower(SNOPower.X1_DemonHunter_EntanglingShot, RangedAttackRange, SentryCastSkillsCastArea.Position);
            }

            // Bola Shot
            if (Hotbar.Contains(SNOPower.DemonHunter_Bolas))
            {
                float range = RangedAttackRange > 50f ? 50f : RangedAttackRange;
                return new TrinityPower(SNOPower.DemonHunter_Bolas, range, SentryCastSkillsCastArea.Position);
            }

            // Grenades
            if (Hotbar.Contains(SNOPower.DemonHunter_Grenades))
            {
                float range = RangedAttackRange > 30f ? 30f : RangedAttackRange;
                return new TrinityPower(SNOPower.DemonHunter_Grenades, RangedAttackRange, SentryCastSkillsCastArea.Position);
            }

            var generator = SkillUtils.Active.FirstOrDefault(s => s.IsGenerator);
            return generator != null ? generator.ToPower(RangedAttackRange, Enemies.BestCluster.Position) : DefaultPower;
        }

        /// <summary>
        /// Gets the best (non-movement related) avoidance power
        /// </summary>
        /// <returns></returns>
        private static TrinityPower GetCombatAvoidancePower()
        {
            // Smoke Screen
            if (CanCast(SNOPower.DemonHunter_SmokeScreen, CanCastFlags.NoTimer) &&
                !GetHasBuff(SNOPower.DemonHunter_ShadowPower))
            {
                return new TrinityPower(SNOPower.DemonHunter_SmokeScreen);
            }

            // Smoke Screen
            if (CanCast(SNOPower.DemonHunter_SmokeScreen, CanCastFlags.NoTimer) &&
                !GetHasBuff(SNOPower.DemonHunter_ShadowPower) &&
                (Player.CurrentHealthPct <= 0.50 || Player.IsRooted || TargetUtil.AnyMobsInRange(12) ||
                (Legendary.MeticulousBolts.IsEquipped && TargetUtil.AnyMobsInRange(60)) || Player.IsIncapacitated))
            {
                return new TrinityPower(SNOPower.DemonHunter_SmokeScreen);
            }

            return null;
        }

        /// <summary>
        /// Gets the best combat buff power for the current conditions
        /// </summary>
        /// <returns></returns>
        private static TrinityPower GetCombatBuffPower()
        {
            // NotSpam Shadow Power
            if (!Settings.Combat.DemonHunter.SpamShadowPower && CanCast(SNOPower.DemonHunter_ShadowPower) && !Player.IsIncapacitated &&
                (!GetHasBuff(SNOPower.DemonHunter_ShadowPower) || Player.CurrentHealthPct <= EmergencyHealthPotionLimit) && // if we don't have the buff or our health is low
                (Player.CurrentHealthPct < 1f || Player.IsRooted || TargetUtil.AnyMobsInRange(15)))
            {
                return new TrinityPower(SNOPower.DemonHunter_ShadowPower);
            }

            // Smoke Screen
            if (CanCast(SNOPower.DemonHunter_SmokeScreen, CanCastFlags.NoTimer) &&
                !GetHasBuff(SNOPower.DemonHunter_ShadowPower) &&
                (Player.CurrentHealthPct <= 0.50 || Player.IsRooted || TargetUtil.AnyMobsInRange(12) ||
                (Legendary.MeticulousBolts.IsEquipped && TargetUtil.AnyMobsInRange(60)) || Player.IsIncapacitated))
            {
                return new TrinityPower(SNOPower.DemonHunter_SmokeScreen);
            }

            // Preparation, restore Disc if needed
            float useDelay = Runes.DemonHunter.FocusedMind.IsActive ? 15000 : 500;
            if (CanCast(SNOPower.DemonHunter_Preparation, CanCastFlags.NoTimer) &&
            Player.SecondaryResource <= V.F("DemonHunter.MinPreparationDiscipline") &&
            !Runes.DemonHunter.Punishment.IsActive &&
            TimeSincePowerUse(SNOPower.DemonHunter_Preparation) >= useDelay)
            {
                return new TrinityPower(SNOPower.DemonHunter_Preparation);
            }

            // Preparation: Punishment
            if (CanCast(SNOPower.DemonHunter_Preparation, CanCastFlags.NoTimer) &&
                Runes.DemonHunter.Punishment.IsActive && (Player.PrimaryResourceMax - Player.PrimaryResource) >= 75)
            {
                return new TrinityPower(SNOPower.DemonHunter_Preparation);
            }

            // Companion
            if (!Player.IsIncapacitated && Hotbar.Contains(SNOPower.X1_DemonHunter_Companion))
            {
                // Use Spider Slow on 4 or more trash mobs in an area or on Unique/Elite/Champion
                if (Runes.DemonHunter.SpiderCompanion.IsActive && CanCast(SNOPower.X1_DemonHunter_Companion) && TargetUtil.ClusterExists(25f, 4) && TargetUtil.EliteOrTrashInRange(25f))
                {
                    return new TrinityPower(SNOPower.X1_DemonHunter_Companion);
                }

                //Use Bat when Hatred is Needed
                if (Runes.DemonHunter.BatCompanion.IsActive && CanCast(SNOPower.X1_DemonHunter_Companion) && Player.PrimaryResourceMissing >= 60)
                {
                    return new TrinityPower(SNOPower.X1_DemonHunter_Companion);
                }

                // Use Boar Taunt on 3 or more trash mobs in an area or on Unique/Elite/Champion
                if (Runes.DemonHunter.BoarCompanion.IsActive && CanCast(SNOPower.X1_DemonHunter_Companion) && ((TargetUtil.ClusterExists(20f, 4) && TargetUtil.EliteOrTrashInRange(20f)) ||
                    (CurrentTarget.IsBossOrEliteRareUnique && CurrentTarget.Distance <= 20f)))
                {
                    return new TrinityPower(SNOPower.X1_DemonHunter_Companion);
                }

                // Ferrets used for picking up Health Globes when low on Health
                if (Runes.DemonHunter.FerretCompanion.IsActive && Trinity.ObjectCache.Any(o => o.Type == GObjectType.HealthGlobe && o.Distance < 60f) && Player.CurrentHealthPct < EmergencyHealthPotionLimit)
                {
                    return new TrinityPower(SNOPower.X1_DemonHunter_Companion);
                }

                // Use Wolf Howl on Unique/Elite/Champion - Would help for farming trash, but trash farming should not need this - Used on Elites to reduce Deaths per hour
                if (Runes.DemonHunter.WolfCompanion.IsActive && CanCast(SNOPower.X1_DemonHunter_Companion, CanCastFlags.NoTimer) &&
                    ((CurrentTarget.IsBossOrEliteRareUnique || TargetUtil.AnyMobsInRange(40, 10)) && CurrentTarget.RadiusDistance < 25f))
                {
                    return new TrinityPower(SNOPower.X1_DemonHunter_Companion);
                }
            }

            // Companion active attack on elite
            if (CanCast(SNOPower.X1_DemonHunter_Companion) && CurrentTarget.IsEliteRareUnique)
            {
                return new TrinityPower(SNOPower.X1_DemonHunter_Companion);
            }

            // Companion off CD
            if (CanCast(SNOPower.X1_DemonHunter_Companion, CanCastFlags.NoTimer) && TargetUtil.AnyMobsInRange(60) && Settings.Combat.DemonHunter.CompanionOffCooldown)
            {
                return new TrinityPower(SNOPower.X1_DemonHunter_Companion);
            }

            int mfdResource = Hotbar.Contains(SNOPower.DemonHunter_SmokeScreen) ? (Runes.DemonHunter.MortalEnemy.IsActive ? 3 : 17) : 17;
            // Marked for Death
            if (CanCast(SNOPower.DemonHunter_MarkedForDeath) &&
                Player.SecondaryResource >= mfdResource &&
                !CurrentTarget.HasDebuff(SNOPower.DemonHunter_MarkedForDeath))
            {
                return new TrinityPower(SNOPower.DemonHunter_MarkedForDeath, DHSettings.RangedAttackRange, CurrentTarget.Position);
            }

            // Vengeance
            if (CanCast(SNOPower.X1_DemonHunter_Vengeance, CanCastFlags.NoTimer) &&
                ((!Settings.Combat.DemonHunter.VengeanceElitesOnly && TargetUtil.AnyMobsInRange(DHSettings.RangedAttackRange + 5f, 6)) || TargetUtil.IsEliteTargetInRange(80f)))
            {
                if (Runes.DemonHunter.Seethe.IsActive && Player.PrimaryResourcePct < 0.3 && !CanCast(SNOPower.DemonHunter_Preparation, CanCastFlags.NoTimer) && !CanCast(SNOPower.X1_DemonHunter_Companion, CanCastFlags.NoTimer))
                    return new TrinityPower(SNOPower.X1_DemonHunter_Vengeance);
                else if (!Runes.DemonHunter.Seethe.IsActive)
                    return new TrinityPower(SNOPower.X1_DemonHunter_Vengeance);
            }

            // Rain of Vengeance OffCD with Dark Cloud
            if (!Player.IsInTown && CanCast(SNOPower.DemonHunter_RainOfVengeance, CanCastFlags.NoTimer) &&
               !Player.IsIncapacitated && Runes.DemonHunter.DarkCloud.IsActive && Settings.Combat.DemonHunter.RainOfVengeanceOffCD)
            {
                return new TrinityPower(SNOPower.DemonHunter_RainOfVengeance);
            }

            return null;
        }

        /// <summary>
        /// Gets the best combat power for the current conditions
        /// </summary>
        /// <returns></returns>
        private static TrinityPower GetCombatPower()
        {
            // Sentry Turret
            if (SentryCastArea != null && CanCast(SNOPower.DemonHunter_Sentry, CanCastFlags.NoTimer) && Trinity.PlayerOwnedDHSentryCount < MaxSentryCount)
            {
                Vector3 zeiOfStoneNewTarget = MathEx.CalculatePointFrom(Player.Position, SentryCastArea.Position, 51f);
                if (IsZeisOfStoneEquipped && !Runes.DemonHunter.PolarStation.IsActive && zeiOfStoneNewTarget.Distance2D(LastZeiOfStoneLocation) >= 25f &&
                    SentryCastArea.Units.OrderByDescending(u => u.Distance).FirstOrDefault().IsInLineOfSightOfPoint(zeiOfStoneNewTarget))
                {
                    LastZeiOfStoneLocation = zeiOfStoneNewTarget;
                    return new TrinityPower(SNOPower.DemonHunter_Sentry, DHSettings.RangedAttackRange, zeiOfStoneNewTarget);
                }
                else
                    return new TrinityPower(SNOPower.DemonHunter_Sentry, DHSettings.RangedAttackRange, SentryCastArea.Position);
            }

            // Caltrops
            if (CanCast(SNOPower.DemonHunter_Caltrops) && TargetUtil.AnyMobsInRange(40) && !GetHasBuff(SNOPower.DemonHunter_Caltrops))
            {
                return new TrinityPower(SNOPower.DemonHunter_Caltrops);
            }

            // Fan of Knives
            if (CanCast(SNOPower.DemonHunter_FanOfKnives) &&
                (TargetUtil.EliteOrTrashInRange(15) || TargetUtil.AnyTrashInRange(15f, 5, false)))
            {
                return new TrinityPower(SNOPower.DemonHunter_FanOfKnives);
            }

            // Strafe spam - similar to barbarian whirlwind routine
            if (CanCast(SNOPower.DemonHunter_Strafe) &&
                Player.PrimaryResource >= Settings.Combat.DemonHunter.StrafeMinHatred)
            {
                bool shouldGetNewZigZag =
                    (DateTime.UtcNow.Subtract(LastChangedZigZag).TotalMilliseconds >= V.I("Barbarian.Whirlwind.ZigZagMaxTime") ||
                    CurrentTarget.ACDGuid != LastZigZagUnitAcdGuid ||
                    ZigZagPosition.Distance2D(Player.Position) <= 5f);

                if (shouldGetNewZigZag)
                {
                    var wwdist = V.F("Barbarian.Whirlwind.ZigZagDistance");

                    ZigZagPosition = TargetUtil.GetZigZagTarget(CurrentTarget.Position, wwdist);

                    LastZigZagUnitAcdGuid = CurrentTarget.ACDGuid;
                    LastChangedZigZag = DateTime.UtcNow;
                }

                int postCastTickDelay = TrinityPower.MillisecondsToTickDelay(250);

                return new TrinityPower(SNOPower.DemonHunter_Strafe, 15f, ZigZagPosition, Trinity.Player.WorldDynamicID, -1, 0, postCastTickDelay);
            }

            // Spike Trap
            if (CanCast(SNOPower.DemonHunter_SpikeTrap) &&
                LastPowerUsed != SNOPower.DemonHunter_SpikeTrap)
            {
                // For distant monsters, try to target a little bit in-front of them (as they run towards us), if it's not a treasure goblin
                float reducedDistance = 0f;
                if (CurrentTarget.Distance > 17f && !CurrentTarget.IsTreasureGoblin)
                {
                    reducedDistance = CurrentTarget.Distance - 17f;
                    if (reducedDistance > 5f)
                        reducedDistance = 5f;
                }
                Vector3 vNewTarget = MathEx.CalculatePointFrom(CurrentTarget.Position, Player.Position, CurrentTarget.Distance - reducedDistance);
                return new TrinityPower(SNOPower.DemonHunter_SpikeTrap, 35f, vNewTarget, Trinity.Player.WorldDynamicID, -1, 1, 1);
            }

            if (CurrentTarget.MonsterAffixes.HasFlag(MonsterAffixes.ReflectsDamage) &&
                Sets.EmbodimentOfTheMarauder.IsFullyEquipped && IsSentryOnTarget())
            {
                SentryCastSkillsCastArea.Position = MathEx.CalculatePointFrom(SentryCastSkillsCastArea.Position, Player.Position, -30f);
            }

            // Cluster Arrow
            if (CanCast(SNOPower.DemonHunter_ClusterArrow))
            {
                return new TrinityPower(SNOPower.DemonHunter_ClusterArrow, RangedAttackRange, SentryCastSkillsCastArea.Position);
            }

            // Elemental Arrow for non-kridershot
            if (CanCast(SNOPower.DemonHunter_ElementalArrow))
            {
                return new TrinityPower(SNOPower.DemonHunter_ElementalArrow, RangedAttackRange, SentryCastSkillsCastArea.Position);
            }

            // Multi Shot
            if (CanCast(SNOPower.DemonHunter_Multishot))
            {
                return new TrinityPower(SNOPower.DemonHunter_Multishot, RangedAttackRange, SentryCastSkillsCastArea.Position);
            }

            // Chakram normal attack
            if (CanCast(SNOPower.DemonHunter_Chakram) && !Runes.DemonHunter.ShurikenCloud.IsActive)
            {
                if (DHSettings.RangedAttackRange > 0)
                    DHSettings.RangedAttackRange = 50;

                return new TrinityPower(SNOPower.DemonHunter_Chakram, RangedAttackRange, SentryCastSkillsCastArea.Position);
            }

            // Chakram:Shuriken Cloud
            if (!Player.IsInTown && CanCast(SNOPower.DemonHunter_Chakram, CanCastFlags.NoTimer) &&
                Runes.DemonHunter.ShurikenCloud.IsActive && TimeSincePowerUse(SNOPower.DemonHunter_Chakram) >= 110000)
            {
                return new TrinityPower(SNOPower.DemonHunter_Chakram);
            }

            // Impale
            if (CanCast(SNOPower.DemonHunter_Impale))
            {
                return new TrinityPower(SNOPower.DemonHunter_Impale, RangedAttackRange, SentryCastSkillsCastArea.Position);
            }

            // Rapid Fire
            if (CanCast(SNOPower.DemonHunter_RapidFire))
            {
                // Players with grenades *AND* rapid fire should spam grenades at close-range instead
                if (CanCast(SNOPower.DemonHunter_Grenades) && CurrentTarget.RadiusDistance <= 18f)
                {
                    return new TrinityPower(SNOPower.DemonHunter_Grenades, 18f, CurrentTarget.Position);
                }
                // Now return rapid fire, if not sending grenades instead
                return new TrinityPower(SNOPower.DemonHunter_RapidFire, DHSettings.RangedAttackRange, CurrentTarget.Position);
            }

            /*// Rain of Vengeance
            if (CanCast(SNOPower.DemonHunter_RainOfVengeance) && !Player.IsIncapacitated &&
               (TargetUtil.ClusterExists(45f, 3) || TargetUtil.EliteOrTrashInRange(45f)) ||
               (TargetUtil.AnyMobsInRange(55f) && Settings.Combat.DemonHunter.RainOfVengeanceOffCD && !Runes.DemonHunter.DarkCloud.IsActive))
            {
                var bestClusterPoint = TargetUtil.GetBestClusterPoint(45f, 65f, false);

                return new TrinityPower(SNOPower.DemonHunter_RainOfVengeance, 0f, bestClusterPoint);
            }*/

            var spender = SkillUtils.Active.FirstOrDefault(s => s.IsSpender && s.CanCast());
            return spender != null ? spender.ToPower(RangedAttackRange, Enemies.BestCluster.Position) : null;
        }

        /// <summary>
        /// Checks and casts buffs if needed
        /// </summary>
        /// <returns></returns>
        private static TrinityPower GetBuffPower()
        {
            // Shadow Power on low health (nospam)
            if (CanCast(SNOPower.DemonHunter_ShadowPower) && !Player.IsIncapacitated &&
                (TimeSincePowerUse(SNOPower.DemonHunter_ShadowPower) > 4500 || !GetHasBuff(SNOPower.DemonHunter_ShadowPower)) &&
                Player.CurrentHealthPct <= Trinity.PlayerEmergencyHealthPotionLimit &&
                Player.SecondaryResource >= 14) // When spamming Shadow Power, save some Discipline for emergencies
            {
                return new TrinityPower(SNOPower.DemonHunter_ShadowPower);
            }

            // Spam Shadow Power
            if (Settings.Combat.DemonHunter.SpamShadowPower && CanCast(SNOPower.DemonHunter_ShadowPower) && !Player.IsIncapacitated &&
                TimeSincePowerUse(SNOPower.DemonHunter_ShadowPower) > 250 && !GetHasBuff(SNOPower.DemonHunter_ShadowPower) &&
                Player.SecondaryResource >= 14)
            {
                return new TrinityPower(SNOPower.DemonHunter_ShadowPower);
            }

            // Smoke Screen spam
            if (Settings.Combat.DemonHunter.SpamSmokeScreen && CanCast(SNOPower.DemonHunter_SmokeScreen) &&
                !GetHasBuff(SNOPower.DemonHunter_ShadowPower))
            {
                return new TrinityPower(SNOPower.DemonHunter_SmokeScreen);
            }

            // Preparation, restore Disc if needed
            float useDelay = Runes.DemonHunter.FocusedMind.IsActive ? 15000 : 500;
            if (CanCast(SNOPower.DemonHunter_Preparation, CanCastFlags.NoTimer) &&
            Player.SecondaryResource <= V.F("DemonHunter.MinPreparationDiscipline") &&
            !Runes.DemonHunter.Punishment.IsActive &&
            TimeSincePowerUse(SNOPower.DemonHunter_Preparation) >= useDelay)
            {
                return new TrinityPower(SNOPower.DemonHunter_Preparation);
            }

            // Preparation: Punishment
            if (CanCast(SNOPower.DemonHunter_Preparation, CanCastFlags.NoTimer) &&
                Runes.DemonHunter.Punishment.IsActive && (Player.PrimaryResourceMax - Player.PrimaryResource) >= 75)
            {
                return new TrinityPower(SNOPower.DemonHunter_Preparation);
            }

            return null;
        }

        /// <summary>
        /// Get best power to destroy object
        /// </summary>
        /// <returns></returns>
        private static TrinityPower GetDemonHunterDestroyPower()
        {
            if (Hotbar.Contains(SNOPower.DemonHunter_HungeringArrow))
                return new TrinityPower(SNOPower.DemonHunter_HungeringArrow, DHSettings.RangedAttackRange, CurrentTarget.Position);

            if (Hotbar.Contains(SNOPower.X1_DemonHunter_EntanglingShot))
                return new TrinityPower(SNOPower.X1_DemonHunter_EntanglingShot, DHSettings.RangedAttackRange, CurrentTarget.Position);

            if (Hotbar.Contains(SNOPower.DemonHunter_Bolas))
                return new TrinityPower(SNOPower.DemonHunter_Bolas, DHSettings.RangedAttackRange, CurrentTarget.Position);

            if (Hotbar.Contains(SNOPower.DemonHunter_Grenades))
                return new TrinityPower(SNOPower.DemonHunter_Grenades, DHSettings.RangedAttackRange, CurrentTarget.Position);

            if (Hotbar.Contains(SNOPower.DemonHunter_ElementalArrow) && Player.PrimaryResource >= 10)
                return new TrinityPower(SNOPower.DemonHunter_ElementalArrow, DHSettings.RangedAttackRange, CurrentTarget.Position);

            if (Hotbar.Contains(SNOPower.X1_DemonHunter_EvasiveFire))
                return new TrinityPower(SNOPower.X1_DemonHunter_EvasiveFire, 10f, CurrentTarget.Position);

            if (Hotbar.Contains(SNOPower.DemonHunter_RapidFire) && Player.PrimaryResource >= 10)
                return new TrinityPower(SNOPower.DemonHunter_RapidFire, DHSettings.RangedAttackRange, CurrentTarget.Position);

            if (Hotbar.Contains(SNOPower.DemonHunter_Chakram) && Player.PrimaryResource >= 20)
                return new TrinityPower(SNOPower.DemonHunter_Chakram, 50f, CurrentTarget.Position);

            return DefaultPower;
        }

        /// <summary>
        /// Special marauder combat routine
        /// </summary>
        /// <returns></returns>
        public static TrinityPower RunMarauderRoutine()
        {
            // Fields
            TrinityPower power = null;

            if (CurrentTarget != null)
                power = GetCombatBuffPower();

            if (!IsNull(power))
            {
                return power;
            }

            if (CurrentTarget == null)
                return null;

            // Sentry
            if (SentryCastArea != null && Skills.DemonHunter.Sentry.CanCast(CanCastFlags.NoTimer) && Trinity.PlayerOwnedDHSentryCount < MaxSentryCount)
            {
                Vector3 zeiOfStoneNewTarget = MathEx.CalculatePointFrom(Player.Position, SentryCastArea.Position, 51f);
                if (AreaHasCastCriteria(SentryCastArea, true))
                {
                    if (IsZeisOfStoneEquipped && !Runes.DemonHunter.PolarStation.IsActive && zeiOfStoneNewTarget.Distance2D(LastZeiOfStoneLocation) >= 25f &&
                        SentryCastArea.Units.OrderByDescending(u => u.Distance).FirstOrDefault().IsInLineOfSightOfPoint(zeiOfStoneNewTarget))
                    {
                        if (!Skills.DemonHunter.Sentry.Cast(zeiOfStoneNewTarget))
                        {
                            LastZeiOfStoneLocation = zeiOfStoneNewTarget;
                            power = new TrinityPower(SNOPower.DemonHunter_Sentry, DHSettings.RangedAttackRange, zeiOfStoneNewTarget);
                        }
                    }
                    else if (!Skills.DemonHunter.Sentry.Cast(SentryCastArea.Position))
                    {
                        power = new TrinityPower(SNOPower.DemonHunter_Sentry, DHSettings.RangedAttackRange, SentryCastArea.Position);
                    }
                }
                else if (SentryCastArea != null && SentryCastArea.Position != Vector3.Zero)
                {
                    if (IsZeisOfStoneEquipped && !Runes.DemonHunter.PolarStation.IsActive && zeiOfStoneNewTarget.Distance2D(LastZeiOfStoneLocation) >= 25f &&
                        SentryCastArea.Units.OrderByDescending(u => u.Distance).FirstOrDefault().IsInLineOfSightOfPoint(zeiOfStoneNewTarget))
                    {
                        LastZeiOfStoneLocation = zeiOfStoneNewTarget;
                        power = new TrinityPower(SNOPower.DemonHunter_Sentry, DHSettings.RangedAttackRange, zeiOfStoneNewTarget);
                    }
                    else
                    {
                        power = new TrinityPower(SNOPower.DemonHunter_Sentry, DHSettings.RangedAttackRange, SentryCastArea.Position);
                    }

                    if (CurrentTarget == null || CurrentTarget.IsUnit)
                        //MoveToSentryCastArea(SentryCastArea);

                        if (!IsNull(power) && CurrentTarget == null)
                        {
                            Trinity.CurrentTarget = new TrinityCacheObject()
                            {
                                Position = SentryCastArea.Position,
                                Type = GObjectType.Unit,
                                Weight = 2d,
                                Radius = 20f,
                                InternalName = "SentryCastArea"
                            };
                        }
                }
            }

            // SentryCastSkills
            if (AreaHasCastCriteria(SentryCastSkillsCastArea))
            {
                if (Skills.DemonHunter.ClusterArrow.CanCast() && !Skills.DemonHunter.ClusterArrow.Cast(SentryCastSkillsCastArea.Position))
                    power = new TrinityPower(SNOPower.DemonHunter_ClusterArrow, RangedAttackRange, SentryCastSkillsCastArea.Position);
                else if (CanCast(SNOPower.DemonHunter_Multishot, CombatBase.CanCastFlags.NoTimer) && !Skills.DemonHunter.Multishot.Cast(SentryCastSkillsCastArea.Position))
                    power = new TrinityPower(SNOPower.DemonHunter_Multishot, RangedAttackRange, SentryCastSkillsCastArea.Position);
                else if (Skills.DemonHunter.Chakram.CanCast() && !Skills.DemonHunter.Chakram.Cast(SentryCastSkillsCastArea.Position))
                    power = new TrinityPower(SNOPower.DemonHunter_Chakram, RangedAttackRange, SentryCastSkillsCastArea.Position);
                else if (Skills.DemonHunter.Impale.CanCast() && !Skills.DemonHunter.Impale.Cast(SentryCastSkillsCastArea.Position))
                    power = new TrinityPower(SNOPower.DemonHunter_Impale, RangedAttackRange, SentryCastSkillsCastArea.Position);
                else if (Skills.DemonHunter.ElementalArrow.CanCast() && !Skills.DemonHunter.ElementalArrow.Cast(SentryCastSkillsCastArea.Position))
                    power = new TrinityPower(SNOPower.DemonHunter_ElementalArrow, RangedAttackRange, SentryCastSkillsCastArea.Position);
            }
            else if (IsNull(power) && SentryCastSkillsCastArea != null && SentryCastSkillsCastArea.Position != Vector3.Zero)
            {
                if (Skills.DemonHunter.ClusterArrow.CanCast(CombatBase.CanCastFlags.NoTimer))
                    power = new TrinityPower(SNOPower.DemonHunter_ClusterArrow, RangedAttackRange, SentryCastSkillsCastArea.Position);
                else if (CanCast(SNOPower.DemonHunter_Multishot, CombatBase.CanCastFlags.NoTimer))
                    power = new TrinityPower(SNOPower.DemonHunter_Multishot, RangedAttackRange, SentryCastSkillsCastArea.Position);
                else if (Skills.DemonHunter.Chakram.CanCast(CombatBase.CanCastFlags.NoTimer))
                    power = new TrinityPower(SNOPower.DemonHunter_Chakram, RangedAttackRange, SentryCastSkillsCastArea.Position);
                else if (Skills.DemonHunter.Impale.CanCast(CombatBase.CanCastFlags.NoTimer))
                    power = new TrinityPower(SNOPower.DemonHunter_Impale, RangedAttackRange, SentryCastSkillsCastArea.Position);
                else if (Skills.DemonHunter.ElementalArrow.CanCast(CombatBase.CanCastFlags.NoTimer))
                    power = new TrinityPower(SNOPower.DemonHunter_ElementalArrow, RangedAttackRange, SentryCastSkillsCastArea.Position);

                if ((CurrentTarget == null || CurrentTarget.IsUnit) && RangedAttackRange > 1f)
                {
                    //MoveToSentryCastSkillsCastArea(SentryCastSkillsCastArea);
                }

                if (!IsNull(power) && CurrentTarget == null)
                {
                    Trinity.CurrentTarget = new TrinityCacheObject()
                    {
                        Position = SentryCastSkillsCastArea.Position,
                        Type = GObjectType.Unit,
                        Weight = 2d,
                        Radius = 20f,
                        InternalName = "SentryCastSkillsCastArea",
                    };
                }
            }

            if (!IsNull(power) && CurrentTarget == null)
            {
                Trinity.CurrentTarget = TargetUtil.GetClosestTarget(150f);
            }

            return power;
        }

        #region Marauder routine helpers
        public static void ResetArea()
        {
            _SentryCastArea = null;
            _SentryCastSkillsCastArea = null;
            _RangedAttackRange = -1f;
        }

        private static TargetArea _SentryCastArea { get; set; }
        private static TargetArea SentryCastArea
        {
            get
            {
                if (_SentryCastArea != null)
                    return _SentryCastArea;

                TrinityCacheObject targetCacheObject = TargetUtil.GetClosestTarget(150f, Player.Position, false);

                if (Runes.DemonHunter.PolarStation.IsActive && TargetUtil.ClusterExists(20f, 3))
                    _SentryCastArea = Enemies.BestCluster;

                else if (TargetUtil.ClusterExists(20f, DHSettings.RangedAttackRange))
                    _SentryCastArea = new TargetArea(20f, TargetUtil.GetBestClusterUnit(20f, DHSettings.RangedAttackRange + 5f, _useWeights: false).Position);

                else if (TargetUtil.ClusterExists(20f, 65f))
                    _SentryCastArea = new TargetArea(20f, TargetUtil.GetBestClusterUnit(20f, 65f + 5f, _useWeights: false).Position);

                else if (CurrentTarget != null && CurrentTarget.IsUnit)
                    _SentryCastArea = new TargetArea(60f, CurrentTarget.Position);

                else if (targetCacheObject != null && targetCacheObject != default(TrinityCacheObject) && targetCacheObject.Position != Vector3.Zero)
                    _SentryCastArea = new TargetArea(60f, targetCacheObject.Position);

                return _SentryCastArea;
            }
        }

        private static TargetArea _SentryCastSkillsCastArea { get; set; }
        private static TargetArea SentryCastSkillsCastArea
        {
            get
            {
                if (_SentryCastSkillsCastArea != null)
                    return _SentryCastSkillsCastArea;

                TrinityCacheObject targetCacheObject = default(TrinityCacheObject);
                if (Skills.DemonHunter.ElementalArrow.IsActive && Runes.DemonHunter.BallLightning.IsActive)
                    targetCacheObject = TargetUtil.GetBestPierceTarget(DHSettings.RangedAttackRange);
                else
                    targetCacheObject = TargetUtil.GetClosestTarget(150f, _useWeights: false);

                if (Skills.DemonHunter.ClusterArrow.IsActive && TargetUtil.ClusterExists(20f, DHSettings.RangedAttackRange))
                    _SentryCastSkillsCastArea = new TargetArea(20f, TargetUtil.GetBestClusterUnit(20f, DHSettings.RangedAttackRange + 5f, _useWeights: false).Position);

                else if (Skills.DemonHunter.Multishot.IsActive && TargetUtil.ClusterExists(40f, DHSettings.RangedAttackRange))
                    _SentryCastSkillsCastArea = new TargetArea(40f, TargetUtil.GetBestClusterUnit(40f, DHSettings.RangedAttackRange + 5f, _useWeights: false).Position);

                else if (Skills.DemonHunter.Chakram.IsActive && TargetUtil.ClusterExists(20f, Math.Min(DHSettings.RangedAttackRange, 50f)))
                    _SentryCastSkillsCastArea = new TargetArea(20f, TargetUtil.GetBestClusterUnit(20f, Math.Min(DHSettings.RangedAttackRange + 5f, 50f), _useWeights: false).Position);

                if (Skills.DemonHunter.ClusterArrow.IsActive && TargetUtil.ClusterExists(20f, 90f))
                    _SentryCastSkillsCastArea = new TargetArea(20f, TargetUtil.GetBestClusterUnit(20f, 65f, _useWeights: false).Position);

                else if (Skills.DemonHunter.Multishot.IsActive && TargetUtil.ClusterExists(40f, 90f))
                    _SentryCastSkillsCastArea = new TargetArea(40f, TargetUtil.GetBestClusterUnit(40f, 65f, _useWeights: false).Position);

                else if (Skills.DemonHunter.Chakram.IsActive && TargetUtil.ClusterExists(20f, 90f))
                    _SentryCastSkillsCastArea = new TargetArea(20f, TargetUtil.GetBestClusterUnit(20f, 65f, _useWeights: false).Position);

                else if (targetCacheObject != null && targetCacheObject != default(TrinityCacheObject) && targetCacheObject.Position != Vector3.Zero)
                    _SentryCastSkillsCastArea = new TargetArea(20f, targetCacheObject.Position);

                else if (CurrentTarget != null && CurrentTarget.IsUnit)
                    _SentryCastSkillsCastArea = new TargetArea(20f, CurrentTarget.Position);
                return _SentryCastSkillsCastArea;
            }
        }

        private static bool AreaHasCastCriteria(TargetArea area, bool rangeRequired = false)
        {
            return !CombatBase.PlayerShouldNotFight &&
                (area != null && area.Position != Vector3.Zero && area.UnitCount >= 1 &&
                (area.Position.Distance2D(Trinity.Player.Position) <= DHSettings.RangedAttackRange &&
                area.Units.OrderByDescending(u => u.Distance).FirstOrDefault().IsInLineOfSight ||
                RangedAttackRange <= 1f && !rangeRequired));
        }

        private static void MoveToSentryCastArea(TargetArea area)
        {
            CombatBase.QueuedMovement.Queue(new QueuedMovement
            {
                Name = "Sentry Cast Position",
                Destination = area.Position,
                OnUpdate = m =>
                {
                    if (SentryCastArea != null)
                    {
                        m.Destination = SentryCastArea.Position;

                        if (Skills.DemonHunter.Sentry.CanCast(CanCastFlags.NoTimer) && Trinity.PlayerOwnedDHSentryCount < MaxSentryCount && AreaHasCastCriteria(SentryCastArea, true))
                        {
                            Skills.DemonHunter.Sentry.Cast(SentryCastArea.Position);
                        }
                    }
                    if (CurrentTarget != null && CurrentTarget.IsUnit)
                    {
                        Trinity.CurrentTarget = TargetUtil.GetClosestTarget(100f);
                    }
                },
                OnFinished = m =>
                {
                    if (Skills.DemonHunter.Sentry.CanCast(CanCastFlags.NoTimer) && Trinity.PlayerOwnedDHSentryCount < MaxSentryCount && AreaHasCastCriteria(SentryCastArea, true))
                    {
                        Skills.DemonHunter.Sentry.Cast(SentryCastArea.Position);
                    }
                },
                StopCondition = m =>
                    AreaHasCastCriteria(SentryCastArea, true) ||
                    SentryCastArea == null || CurrentTarget == null || !CurrentTarget.IsUnit || SentryCastArea.Position == Vector3.Zero ||
                    CombatBase.PlayerShouldNotFight,
                Options = new QueuedMovementOptions
                {
                    Logging = LogLevel.Info,
                    Type = MoveType.SpecialCombat
                }
            });
        }

        private static void MoveToSentryCastSkillsCastArea(TargetArea area)
        {
            CombatBase.QueuedMovement.Queue(new QueuedMovement
            {
                Name = "Hatred Skills Cast Position",
                Destination = area.Position,
                OnUpdate = m =>
                {
                    if (SentryCastSkillsCastArea != null)
                    {
                        m.Destination = SentryCastSkillsCastArea.Position;

                        if (AreaHasCastCriteria(SentryCastSkillsCastArea))
                        {
                            if (Skills.DemonHunter.ClusterArrow.CanCast())
                                Skills.DemonHunter.ClusterArrow.Cast(SentryCastSkillsCastArea.Position);
                            else if (CanCast(SNOPower.DemonHunter_Multishot, CombatBase.CanCastFlags.NoTimer))
                                Skills.DemonHunter.Multishot.Cast(SentryCastSkillsCastArea.Position);
                            else if (Skills.DemonHunter.Chakram.CanCast())
                                Skills.DemonHunter.Chakram.Cast(SentryCastSkillsCastArea.Position);
                            else if (Skills.DemonHunter.Impale.CanCast())
                                Skills.DemonHunter.Impale.Cast(SentryCastSkillsCastArea.Position);
                            else if (Skills.DemonHunter.ElementalArrow.CanCast())
                                Skills.DemonHunter.ElementalArrow.Cast(SentryCastSkillsCastArea.Position);
                            else if (Skills.DemonHunter.EvasiveFire.CanCast())
                                Skills.DemonHunter.EvasiveFire.Cast(SentryCastSkillsCastArea.Position);
                            else if (Skills.DemonHunter.HungeringArrow.CanCast())
                                Skills.DemonHunter.HungeringArrow.Cast(SentryCastSkillsCastArea.Position);
                            else if (Skills.DemonHunter.EntanglingShot.CanCast())
                                Skills.DemonHunter.EntanglingShot.Cast(SentryCastSkillsCastArea.Position);
                            else if (Skills.DemonHunter.Bolas.CanCast())
                                Skills.DemonHunter.Bolas.Cast(SentryCastSkillsCastArea.Position);
                            else if (Skills.DemonHunter.Grenade.CanCast())
                                Skills.DemonHunter.Grenade.Cast(SentryCastSkillsCastArea.Position);
                        }
                        if (CurrentTarget != null && CurrentTarget.IsUnit)
                        {
                            Trinity.CurrentTarget = TargetUtil.GetClosestTarget(100f);
                        }
                    }
                },
                OnFinished = m =>
                {
                    if (AreaHasCastCriteria(SentryCastSkillsCastArea))
                    {
                        if (Skills.DemonHunter.ClusterArrow.CanCast())
                            Skills.DemonHunter.ClusterArrow.Cast(SentryCastSkillsCastArea.Position);
                        else if (CanCast(SNOPower.DemonHunter_Multishot, CombatBase.CanCastFlags.NoTimer))
                            Skills.DemonHunter.Multishot.Cast(SentryCastSkillsCastArea.Position);
                        else if (Skills.DemonHunter.Chakram.CanCast())
                            Skills.DemonHunter.Chakram.Cast(SentryCastSkillsCastArea.Position);
                        else if (Skills.DemonHunter.Impale.CanCast())
                            Skills.DemonHunter.Impale.Cast(SentryCastSkillsCastArea.Position);
                        else if (Skills.DemonHunter.ElementalArrow.CanCast())
                            Skills.DemonHunter.ElementalArrow.Cast(SentryCastSkillsCastArea.Position);
                        else if (Skills.DemonHunter.EvasiveFire.CanCast())
                            Skills.DemonHunter.EvasiveFire.Cast(SentryCastSkillsCastArea.Position);
                        else if (Skills.DemonHunter.HungeringArrow.CanCast())
                            Skills.DemonHunter.HungeringArrow.Cast(SentryCastSkillsCastArea.Position);
                        else if (Skills.DemonHunter.EntanglingShot.CanCast())
                            Skills.DemonHunter.EntanglingShot.Cast(SentryCastSkillsCastArea.Position);
                        else if (Skills.DemonHunter.Bolas.CanCast())
                            Skills.DemonHunter.Bolas.Cast(SentryCastSkillsCastArea.Position);
                        else if (Skills.DemonHunter.Grenade.CanCast())
                            Skills.DemonHunter.Grenade.Cast(SentryCastSkillsCastArea.Position);
                    }
                },
                StopCondition = m =>
                    AreaHasCastCriteria(SentryCastSkillsCastArea) ||
                    SentryCastSkillsCastArea == null || CurrentTarget == null || !CurrentTarget.IsUnit || SentryCastSkillsCastArea.Position == Vector3.Zero ||
                    CombatBase.PlayerShouldNotFight,
                Options = new QueuedMovementOptions
                {
                    Logging = LogLevel.Info,
                    Type = MoveType.SpecialCombat
                }
            });
        }
        #endregion
        #region Vault helpers
        public static bool CurrentlyUseVault
        {
            get
            {
                return Player.ActorClass == ActorClass.DemonHunter && Hotbar.Contains(SNOPower.DemonHunter_Vault) &&
                    CombatBase.TimeSincePowerUse(SNOPower.DemonHunter_Vault) < 400;
            }
        }

        public static bool CanCastCombatVaultMovement
        {
            get
            {
                return CombatBase.TimeSincePowerUse(SNOPower.DemonHunter_Vault) >= Trinity.Settings.Combat.DemonHunter.VaultCombatDelay &&
                    Skills.DemonHunter.Vault.CanCast() &&
                    Trinity.Settings.Combat.DemonHunter.VaultMode != DemonHunterVaultMode.MovementOnly;
            }
        }

        public static bool CanCastOocVaultMovement
        {
            get
            {
                return CombatBase.TimeSincePowerUse(SNOPower.DemonHunter_Vault) >= Trinity.Settings.Combat.DemonHunter.VaultMovementDelay &&
                    Settings.Combat.DemonHunter.VaultMode != DemonHunterVaultMode.CombatOnly &&
                    !TargetUtil.AnyMobsInRange(85f, false) &&
                    Skills.DemonHunter.Vault.CanCast() &&
                    Trinity.Settings.Combat.DemonHunter.VaultMode != DemonHunterVaultMode.CombatOnly &&
                    ((Player.SecondaryResourcePct > Settings.Combat.DemonHunter.MinDiciplineOOCVaultPct && !Sets.DanettasHatred.IsFullyEquipped) ||
                    Sets.DanettasHatred.IsFullyEquipped) && !Passives.DemonHunter.TacticalAdvantage.IsBuffActive;
            }
        }

        public static bool IsVaultAptCombatMovement(Vector3 loc)
        {
            float minCombatDist = Trinity.Settings.Combat.DemonHunter.MinDistVaultShrineOther;
            if (CurrentTarget != null) { minCombatDist += CurrentTarget.RequiredRange; }
            float minDicipline = 0f;
            bool inLoS = true;
            bool specialTimeSinceUse = true;
            if (CurrentTarget != null)
            {
                inLoS = CurrentTarget.IsInLineOfSight;
                switch (CurrentTarget.Type)
                {
                    case GObjectType.Player:
                        return false;
                    case GObjectType.Unit:
                        minCombatDist = Trinity.Settings.Combat.DemonHunter.MinDistVaultCombat +
                            Math.Max(CurrentTarget.RequiredRange, CombatBase.KiteDistance) +
                            TargetUtil.GetClosestTarget(150f).Position.Distance2D(CurrentTarget.Position);
                        minDicipline = Settings.Combat.DemonHunter.MinDiciplineCombatVaultPct;
                        specialTimeSinceUse = CombatBase.TimeSincePowerUse(SNOPower.DemonHunter_Vault) >= 2000;
                        break;
                    case GObjectType.ProgressionGlobe:
                    case GObjectType.PowerGlobe:
                    case GObjectType.Item:
                    case GObjectType.MarkerLocation:
                    case GObjectType.Gold:
                        minCombatDist = Trinity.Settings.Combat.DemonHunter.MinDistVaultGlobeItem + CurrentTarget.RequiredRange;
                        minDicipline = Settings.Combat.DemonHunter.MinDiciplineCombatVaultPct;
                        specialTimeSinceUse = CombatBase.TimeSincePowerUse(SNOPower.DemonHunter_Vault) >= 1500;
                        break;
                    case GObjectType.HealthWell:
                    case GObjectType.HealthGlobe:
                        if (Player.AvoidDeath || Player.CurrentHealthPct < 0.4)
                        {
                            minCombatDist = 0f;
                            specialTimeSinceUse = CombatBase.TimeSincePowerUse(SNOPower.DemonHunter_Vault) >= 100;
                        }
                        else if (Player.CurrentHealthPct < 0.6)
                        {
                            minCombatDist = Trinity.Settings.Combat.DemonHunter.MinDistVaultHealth + CurrentTarget.RequiredRange;
                            specialTimeSinceUse = CombatBase.TimeSincePowerUse(SNOPower.DemonHunter_Vault) >= 1000;
                        }
                        else
                        {
                            minCombatDist = Trinity.Settings.Combat.DemonHunter.MinDistVaultGlobeItem + CurrentTarget.RequiredRange;
                            minDicipline = Settings.Combat.DemonHunter.MinDiciplineCombatVaultPct;
                            specialTimeSinceUse = CombatBase.TimeSincePowerUse(SNOPower.DemonHunter_Vault) >= 1500;
                        }
                        break;
                    default:
                        minCombatDist = Trinity.Settings.Combat.DemonHunter.MinDistVaultShrineOther + CurrentTarget.RequiredRange;
                        minDicipline = Settings.Combat.DemonHunter.MinDiciplineCombatVaultPct;
                        specialTimeSinceUse = CombatBase.TimeSincePowerUse(SNOPower.DemonHunter_Vault) >= 1000;
                        break;
                }
            }
            else
            {
                minCombatDist = Trinity.Settings.Combat.DemonHunter.MinDistVaultCombat + CombatBase.KiteDistance;
                minDicipline = Settings.Combat.DemonHunter.MinDiciplineCombatVaultPct;
                specialTimeSinceUse = CombatBase.TimeSincePowerUse(SNOPower.DemonHunter_Vault) >= 2000;
            }

            return
                /* Real combat movement */
                !Trinity.Player.StandingInAvoidance && !Trinity.Player.AvoidDeath && !Trinity.Player.NeedToKite &&
                /* Move to target & target in LoS */
                PlayerMover.IsMovementToTarget(loc) && inLoS &&
                /* Save dicipline for avoid vault movement */
                Player.SecondaryResourcePct > minDicipline &&
                /* Distance to target respect dh settings */
                loc.Distance2D(Trinity.Player.Position) >= minCombatDist && loc.Distance2D(Trinity.Player.Position) < 150 && specialTimeSinceUse &&
                /* target not in 0.8 kite monster zone */
                !Trinity.ObjectCache.Any(o => o.IsUnit && loc.Distance2D(o.Position) <= CombatBase.KiteDistance * 0.8) &&
                /* Loc not in aoe and not intersect aoe */
                !TargetUtil.LocOrPathInAoE(loc);
        }

        public static bool IsVaultAptKiteMovement(Vector3 loc)
        {
            return
                /* Vault only at min dh settings health AND */
                Player.CurrentHealthPct <= DHSettings.MinHealthVaultKite &&

                /* 1) A real kite movement OR */
                ((!Trinity.Player.StandingInAvoidance && !Trinity.Player.AvoidDeath && Trinity.Player.NeedToKite) ||

                /* 2) Target not null and not move to target */
                (CurrentTarget != null && CurrentTarget.IsUnit && !PlayerMover.IsMovementToTarget(loc) && CurrentTarget.Distance <= CombatBase.KiteDistance &&
                /* Distance to target respect dh settings */
                loc.Distance2D(Trinity.Player.Position) >= Trinity.Settings.Combat.DemonHunter.MinDistVaultKite &&
                /* Loc not in aoe and not intersect aoe OR */
                !TargetUtil.LocOrPathInAoE(loc)) ||

                /* 3) Target is kite */
                (CurrentTarget != null && CurrentTarget.IsKite &&
                /* Distance to target respect dh settings */
                loc.Distance2D(Trinity.Player.Position) >= Trinity.Settings.Combat.DemonHunter.MinDistVaultKite));
        }

        public static bool IsVaultAptAvoidanceMovement(Vector3 loc)
        {
            return Player.CurrentHealthPct <= DHSettings.MinHealthVaultAvoidance &&
                (Trinity.Player.AvoidDeath || CombatBase.PlayerIsSurrounded || Trinity.Player.StandingInAvoidance ||
                (CurrentTarget != null && CurrentTarget.IsAvoidance && !CurrentTarget.IsKite)) &&
                loc.Distance2D(Trinity.Player.Position) >= Trinity.Settings.Combat.DemonHunter.MinDistVaultAvoidance;
        }

        public static void LogVault(Vector3 loc, bool cMove = false, bool kMove = false, bool aMove = false, string wI = "")
        {
            string log = "";
            if (aMove)
                log = string.Format("Using vault for avoidance movement{1}, Dist={0:0.0}", Vector3.Distance(loc, Trinity.Player.Position),
                    CacheData.AvoidanceObstacles.Any() ?
                    " (" + CacheData.AvoidanceObstacles.OrderBy(a => Vector3.Distance(a.Position, Trinity.Player.Position)).FirstOrDefault().Name + ") " : "");

            else if (kMove)
                log = string.Format("Using vault for kite movement, Dist={0:0.0}", Vector3.Distance(loc, Trinity.Player.Position));

            else if (cMove)
                log = string.Format("Using vault for combat movement{1}, Dist={0:0.0}", Vector3.Distance(loc, Trinity.Player.Position),
                    CurrentTarget != null ?
                    " (" + CurrentTarget.Type.ToString() + " Attempt) " : "");

            else if (!TargetUtil.AnyMobsInRange(85f, false))
                log = string.Format("Using vault for ooc movement, Dist={0:0.0}", Vector3.Distance(loc, Trinity.Player.Position));

            else
                log = string.Format("Using vault for unknown reason ?_?, Dist={0:0.0}", Vector3.Distance(loc, Trinity.Player.Position));

            Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, log + (Settings.Advanced.LogCategories.HasFlag(LogCategory.Movement) && wI != "" ? " WeightsInfos:" + wI : ""));
        }
        #endregion
    }
}
