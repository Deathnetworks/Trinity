using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trinity.Combat.Abilities;
using Trinity.Config.Combat;
using Trinity.LazyCache;
using Trinity.Reference;
using Trinity.Technicals;
using Zeta.Game;
using Zeta.Game.Internals.Actors;
using Zeta.Game.Internals.SNO;

namespace Trinity.Combat.Weighting
{
    public class UnitWeighting : WeightingBase
    {
        /// <summary>
        /// Weighting for Units
        /// </summary>
        public static IEnumerable<Weight> GetWeight(TrinityObject cacheObject)
        {
            var weightFactors = new List<Weight>();
            var unit = cacheObject as TrinityUnit;            
            
            /*
             * Ignore rules 
             * 
             * */

            if (unit == null)
                return weightFactors.Return(WeightReason.InvalidType);

            if (unit.IsSummonedByPlayer)
                return weightFactors.Return(WeightReason.IsSummon);

            if (!unit.IsHostile)
                return weightFactors.Return(WeightReason.NotHostile);

            if (unit.IsDead)
                return weightFactors.Return(WeightReason.DeadUnit);

            if (unit.HasShieldingAffix && unit.IsInvulnerable)
                return weightFactors.Return(WeightReason.NotAttackable);

            if(unit.IsBoss && CombatContext.ShouldIgnoreBosses)
                return weightFactors.Return(WeightReason.IgnoreBosses);

            if (unit.IsEliteRareUnique && CombatContext.ShouldIgnoreElites)
                return weightFactors.Return(WeightReason.IgnoreElites);
            
            if (unit.IsTrash && CombatContext.ShouldIgnoreTrashMobs)
                return weightFactors.Return(WeightReason.IgnoreTrash);

            if (!unit.IsBoss && !unit.IsGoblin)
            {
                if (Trinity.Settings.Combat.Misc.IgnoreTrashBelowHealth > 0 && unit.CurrentHealthPct < Trinity.Settings.Combat.Misc.IgnoreTrashBelowHealth)
                    return weightFactors.Return(WeightReason.IgnoreHealth);

                if (Trinity.Settings.Combat.Misc.IgnoreTrashBelowHealthDoT > 0 && unit.HasDotDps && unit.CurrentHealthPct < Trinity.Settings.Combat.Misc.IgnoreTrashBelowHealthDoT)
                    return weightFactors.Return(WeightReason.IgnoreHealthDot);
            }

            if (Trinity.Settings.Combat.Misc.GoblinPriority == GoblinPriority.Ignore && unit.IsGoblin)
                return weightFactors.Return(WeightReason.GoblinIgnore);

            /*
             * Force Kill Rules
             * 
             * */

            if (Trinity.Settings.Combat.Misc.GoblinPriority == GoblinPriority.Kamikaze && unit.IsGoblin)
                return weightFactors.Return(WeightReason.GoblinKamikaze, WeightManager.MaxWeight);

            /*
             * Unit Weighting
             * 
             * */

            weightFactors.Add(new Weight(5000, WeightMethod.Set, WeightReason.Start));

            if(unit.IsBoss)
                weightFactors.Add(new Weight(30000, WeightMethod.Add, WeightReason.IsBoss));

            if(unit.IsEliteRareUnique)
                weightFactors.Add(new Weight(15000, WeightMethod.Add, WeightReason.IsElite));

            // Distance - Negative weight for units beyond kill radius, positive weight to those closer
            weightFactors.Add(new Weight((CombatContext.KillRadius - unit.Distance) * 100, WeightMethod.Add, WeightReason.Distance));

            if (Trinity.Settings.Combat.Misc.ForceKillSummoners && unit.IsSummoner)
                weightFactors.Add(new Weight(1000, WeightMethod.Add, WeightReason.Summoner));

            // Clustering - Add weight to groups of units.
            weightFactors.Add(new Weight(unit.UnitsNearby * 250, WeightMethod.Add, WeightReason.Cluster));

            // Avoidance - Reduce weight for units in avoidance
            if (CacheManager.Me.IsMelee && unit.IsStandingInAvoidance)
                weightFactors.Add(new Weight(0.5, WeightMethod.Multiply, WeightReason.InAvoidance));

            // Goblin Priority
            if (Trinity.Settings.Combat.Misc.GoblinPriority == GoblinPriority.Prioritize && unit.IsGoblin)
                weightFactors.Add(new Weight(10000, WeightMethod.Add, WeightReason.GoblinPriority));





            //if (unit.IsTownVendor || !unit.IsAttackable)
            //{
            //    weightFactors.Add(new Weight(0, WeightMethod.Set, WeightReason.NotAttackable));
            //    return weightFactors;
            //}

            //// Not attackable, could be shielded, make super low priority
            //if (unit.HasShieldingAffix && unit.IsInvulnerable)
            //{
            //    // Only 100 weight helps prevent it being prioritized over an unshielded
            //    weightFactors.Add(new Weight(100, WeightMethod.Set, WeightReason.NotAttackable));
            //    return weightFactors;
            //}

            //// Ignore Dead monsters :)
            //if (unit.IsDead)
            //{
            //    weightFactors.Add(new Weight(0, WeightMethod.Set, WeightReason.DeadUnit));
            //    return weightFactors;
            //}

            //bool isInHotSpot = GroupHotSpots.CacheObjectIsInHotSpot(unit.Position);

            //// Ignore Trash Situations
            //if (unit.IsTrash)
            //{
            //    int nearbyMonsterCount = unit.UnitsNearby;
            //    bool elitesInRangeOfUnit = !CombatBase.IgnoringElites && CacheManager.Monsters.Any(u => u.IsElite && u.Distance < 15f);
            //    bool shouldIgnoreTrashMob = CombatContext.ShouldIgnoreTrashMobs && nearbyMonsterCount < Trinity.Settings.Combat.Misc.TrashPackSize && !elitesInRangeOfUnit;
            //    bool ignoreSummoner = unit.IsSummoner && !Trinity.Settings.Combat.Misc.ForceKillSummoners || unit.IsBlocking;

            //    // Ignore trash mobs < 15% health or 50% health with a DoT
            //    if (unit.IsTrash && shouldIgnoreTrashMob &&
            //        (unit.HitpointsCurrentPct < Trinity.Settings.Combat.Misc.IgnoreTrashBelowHealth ||
            //            unit.HitpointsCurrentPct < Trinity.Settings.Combat.Misc.IgnoreTrashBelowHealthDoT && unit.HasDotDps) &&
            //            !unit.IsQuestMonster && !unit.IsMinimapActive)
            //    {
            //        weightFactors.Add(new Weight(0, WeightMethod.Set, WeightReason.IgnoreHealthDot));
            //        return weightFactors;
            //    }

            //    // Ignore Solitary Trash mobs (no elites present)
            //    // Except if has been primary target or if already low on health (<= 20%)
            //    if ((shouldIgnoreTrashMob && !isInHotSpot &&
            //            !unit.IsQuestMonster && !unit.IsMinimapActive && !ignoreSummoner &&
            //            !unit.IsBountyObjective) || CombatContext.IsHealthGlobeEmergency || CombatContext.ShouldPrioritizeContainers
            //            || CombatContext.ShouldPrioritizeShrines || CombatContext.ShouldKamakaziGoblins)
            //    {
            //        weightFactors.Add(new Weight(0, WeightMethod.Set, WeightReason.IgnoreOrphanTrash));
            //        return weightFactors;
            //    }
            //}

            //// Ignore Elite Situations
            //if (unit.IsEliteRareUnique)
            //{
            //    // Ignore elite option, except if trying to town portal
            //    if ((!unit.IsBoss || CombatContext.ShouldIgnoreBosses) && !unit.IsBountyObjective &&
            //        CombatContext.ShouldIgnoreElites && unit.IsEliteRareUnique && !isInHotSpot &&
            //        !(unit.HitpointsCurrentPct <= ((float)Trinity.Settings.Combat.Misc.ForceKillElitesHealth / 100))
            //        || CombatContext.IsHealthGlobeEmergency || CombatContext.ShouldPrioritizeShrines || CombatContext.ShouldPrioritizeContainers || CombatContext.ShouldKamakaziGoblins)
            //    {
            //        weightFactors.Add(new Weight(0, WeightMethod.Set, WeightReason.IgnoreElites));
            //        return weightFactors;
            //    }
            //}

            //// Monster is not within kill range
            //if (!unit.IsBoss && !unit.IsTreasureGoblin && Trinity.LastTargetRactorGUID != unit.RActorGuid &&
            //    unit.RadiusDistance > unit.KillRange && !unit.IsQuestMonster && !unit.IsBountyObjective)
            //{
            //    weightFactors.Add(new Weight(0, WeightMethod.Set, WeightReason.NotInKillRange));
            //    return weightFactors;
            //}

            //// Ignore event monsters that are too far from event
            //if (CombatContext.InActiveEvent)
            //{
            //    var eventObject = CacheManager.Objects.FirstOrDefault(o => o.IsEventObject);
            //    if (eventObject != null)
            //    {
            //        var eventObjectPosition = eventObject.Position;
            //        if (!unit.IsQuestMonster && unit.Position.Distance2DSqr(eventObjectPosition) > 75 * 75)
            //        {
            //            weightFactors.Add(new Weight(0, WeightMethod.Set, WeightReason.TooFarFromEvent));
            //            return weightFactors;
            //        }
            //    }
            //}

            //// Force a close range target because we seem to be stuck *OR* if not ranged and currently rooted
            //if (CombatContext.PrioritizeCloseRangeUnits)
            //{
            //    double rangePercent = (20d - unit.RadiusDistance) / 20d;
            //    weightFactors.Add(new Weight(Math.Max(rangePercent * WeightManager.MaxWeight, 200d), WeightMethod.Set, WeightReason.GoblinKamikaze));

            //    // Goblin priority KAMIKAZEEEEEEEE
            //    if (unit.IsTreasureGoblin && Trinity.Settings.Combat.Misc.GoblinPriority == GoblinPriority.Kamikaze)
            //        weightFactors.Add(new Weight(0, WeightMethod.Set, WeightReason.GoblinKamikaze));

            //    return weightFactors;
            //}

            //// Starting weight of 500 for Trash
            //if (unit.IsTrash)
            //{
            //    var startingWeight = Math.Max((unit.KillRange - unit.RadiusDistance) / unit.KillRange * 100d, 2d);
            //    weightFactors.Add(new Weight(startingWeight, WeightMethod.Set, WeightReason.StartingWeight));
            //}

            //// Elite Weight based on kill range and max possible weight
            //if (unit.IsBossOrEliteRareUnique)
            //{
            //    var startingWeight = unit.Weight = Math.Max((unit.KillRange - unit.RadiusDistance) / unit.KillRange * WeightManager.MaxWeight, 2000d);
            //    weightFactors.Add(new Weight(startingWeight, WeightMethod.Set, WeightReason.IsBossEliteRareUnique));
            //}

            //// Bounty Objectives goooo
            //if (unit.IsBountyObjective && !unit.IsBlocking)
            //{
            //    weightFactors.Add(new Weight(15000d, WeightMethod.Add, WeightReason.BountyObjective));
            //}

            //// set a minimum 100 just to make sure it's not 0
            //if (CombatContext.IsKillBounty || CombatContext.InActiveEvent)
            //    weightFactors.Add(new Weight(100, WeightMethod.Add, WeightReason.InActiveEvent));

            //// Elites with Archon get super weight
            //if (!CombatBase.IgnoringElites && CacheManager.Me.ActorClass == ActorClass.Wizard && Trinity.GetHasBuff(SNOPower.Wizard_Archon) && unit.IsBossOrEliteRareUnique)
            //    weightFactors.Add(new Weight(10000d, WeightMethod.Add, WeightReason.ArchonElite));

            //// Monsters near other players given higher weight
            //if (CacheManager.Players.Count > 1)
            //{
            //    var group = 0d;
            //    foreach (var player in CacheManager.Players.Where(p => !p.IsMe))
            //    {
            //        group += Math.Max(((55f - unit.Position.Distance2D(player.Position)) / 55f * 500d), 2d);
            //    }
            //    weightFactors.Add(new Weight(group, WeightMethod.Add, WeightReason.ElitesNearPlayers));
            //}

            //// Is standing in HotSpot - focus fire!
            //if (isInHotSpot)
            //    weightFactors.Add(new Weight(10000, WeightMethod.Add, WeightReason.InHotSpot));

            //// Give extra weight to ranged enemies
            //if ((CacheManager.Me.ActorClass == ActorClass.Barbarian || CacheManager.Me.ActorClass == ActorClass.Monk) &&
            //    (unit.MonsterSize == MonsterSize.Ranged || DataDictionary.RangedMonsterIds.Contains(unit.ActorSNO)))
            //{
            //    weightFactors.Add(new Weight(1100d, WeightMethod.Add, WeightReason.RangedUnit));
            //}

            //// Lower health gives higher weight - health is worth up to 1000ish extra weight
            //if (unit.IsTrash && unit.HitpointsCurrentPct < 0.20 && unit.IsAlive)
            //    weightFactors.Add(new Weight(Math.Max((1 - unit.HitpointsCurrentPct) / 100 * 1000d, 100d), WeightMethod.Add, WeightReason.LowHPTrash));

            //// Elites on low health get extra priority - up to 2500ish
            //if (unit.IsEliteRareUnique && unit.HitpointsCurrentPct < 0.25 && unit.HitpointsCurrentPct > 0.01)
            //    weightFactors.Add(new Weight(Math.Max((1 - unit.HitpointsCurrentPct) / 100 * 2500d, 100d), WeightMethod.Add, WeightReason.LowHPElite));

            //// Bonuses to priority type monsters from the dictionary/hashlist set at the top of the code
            //int extraPriority;
            //if (DataDictionary.MonsterCustomWeights.TryGetValue(unit.ActorSNO, out extraPriority))
            //{
            //    // adding a constant multiple of 3 to all weights here (e.g. 999 becomes 1998)
            //    weightFactors.Add(new Weight(extraPriority * 2d, WeightMethod.Add, WeightReason.XtraPriority));
            //}

            //// Extra weight for summoners
            //if (!unit.IsBoss && unit.IsSummoner)
            //    weightFactors.Add(new Weight(2500, WeightMethod.Add, WeightReason.Summoner));

            //// Was already a target and is still viable, give it some free extra weight, to help stop flip-flopping between two targets
            //if (unit.IsLastTarget && unit.Distance <= 25f)
            //    weightFactors.Add(new Weight(1000d, WeightMethod.Add, WeightReason.PreviousTarget));

            //// Close range get higher weights the more of them there are, to prevent body-blocking
            //if (!unit.IsBoss && unit.RadiusDistance <= 10f)
            //    weightFactors.Add(new Weight(3000d * unit.Radius, WeightMethod.Add, WeightReason.CloseProximity));

            //// Special additional weight for corrupt growths in act 4 ONLY if they are at close range (not a standard priority thing)
            //if ((unit.ActorSNO == 210120 || unit.ActorSNO == 210268) && unit.Distance <= 25f)
            //    weightFactors.Add(new Weight(5000d * unit.Radius, WeightMethod.Add, WeightReason.CorruptGrowth));

            //// If standing Molten, Arcane, or Poison Tree near unit, reduce weight
            //if (CombatBase.KiteDistance <= 0 && CacheManager.Avoidances.Any(aoe =>
            //        (aoe.AvoidanceType == AvoidanceType.Arcane ||
            //         aoe.AvoidanceType == AvoidanceType.MoltenCore ||
            //            //aoe.AvoidanceType == AvoidanceType.MoltenTrail ||
            //         aoe.AvoidanceType == AvoidanceType.PoisonTree) &&
            //        unit.Position.Distance2D(aoe.Position) <= aoe.Radius))
            //{
            //    weightFactors.Add(new Weight(1, WeightMethod.Set, WeightReason.InSpecialAoE));
            //}

            //// If any AoE between us and target, reduce weight, for melee only
            //if (!Trinity.Settings.Combat.Misc.KillMonstersInAoE &&
            //    CombatBase.KiteDistance <= 0 && unit.RadiusDistance > 3f &&
            //    CacheManager.Avoidances.Any(aoe => aoe.AvoidanceType != AvoidanceType.PlagueCloud &&
            //    MathUtil.IntersectsPath(aoe.Position, aoe.Radius, CacheManager.Me.Position, unit.Position)))
            //{
            //    weightFactors.Add(new Weight(1, WeightMethod.Set, WeightReason.AoEPathLine));
            //}
            //// See if there's any AOE avoidance in that spot, if so reduce the weight to 1, for melee only
            //if (!Trinity.Settings.Combat.Misc.KillMonstersInAoE &&
            //    CombatBase.KiteDistance <= 0 && unit.RadiusDistance > 3f &&
            //    CacheData.TimeBoundAvoidance.Any(aoe => aoe.AvoidanceType != AvoidanceType.PlagueCloud &&
            //    unit.Position.Distance2D(aoe.Position) <= aoe.Radius))
            //{
            //    weightFactors.Add(new Weight(1, WeightMethod.Set, WeightReason.InAoE));
            //}

            //// Deal with treasure goblins - note, of priority is set to "0", then the is-a-goblin flag isn't even set for use here - the monster is ignored
            //// Goblins on low health get extra priority - up to 2000ish
            //if (Trinity.Settings.Combat.Misc.GoblinPriority >= GoblinPriority.Prioritize && unit.IsTreasureGoblin && unit.HitpointsCurrentPct <= 0.98)
            //    weightFactors.Add(new Weight(Math.Max(((1 - unit.HitpointsCurrentPct) / 100) * 2000d, 100d), WeightMethod.Add, WeightReason.LowHPGoblin));

            //// If this is a goblin and there's no doors or barricades blocking our path to it.
            //if (unit.IsTreasureGoblin && !CacheManager.Objects.Any(obj => (obj.TrinityType == TrinityObjectType.Door || obj.TrinityType == TrinityObjectType.Barricade) && !MathUtil.IntersectsPath(obj.Position, obj.Radius, CacheManager.Me.Position, unit.Position)))
            //{
            //    // Logging goblin sightings
            //    if (Trinity.lastGoblinTime == DateTime.MinValue)
            //    {
            //        Trinity.TotalNumberGoblins++;
            //        Trinity.LastGoblinTime = DateTime.UtcNow;
            //        Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "Goblin #{0} in sight. Distance={1:0}", Trinity.TotalNumberGoblins, unit.Distance);
            //    }
            //    else
            //    {
            //        if (DateTime.UtcNow.Subtract(Trinity.LastGoblinTime).TotalMilliseconds > 30000)
            //            Trinity.LastGoblinTime = DateTime.MinValue;
            //    }

            //    // Ignore goblins in AOE
            //    if (CacheManager.Avoidances.Any(aoe => unit.Position.Distance2D(aoe.Position) <= aoe.Radius) && Trinity.Settings.Combat.Misc.GoblinPriority != GoblinPriority.Kamikaze)
            //    {
            //        weightFactors.Add(new Weight(1, WeightMethod.Set, WeightReason.InAoE));
            //        return weightFactors;
            //    }

            //    // Original Trinity stuff for priority handling now
            //    switch (Trinity.Settings.Combat.Misc.GoblinPriority)
            //    {
            //        case GoblinPriority.Normal:
            //            weightFactors.Add(new Weight(751, WeightMethod.Add, WeightReason.GoblinNormal));
            //            break;

            //        case GoblinPriority.Prioritize:
            //            // Super-high priority option below... 
            //            var gobWeight = Math.Max((unit.KillRange - unit.RadiusDistance) / unit.KillRange * WeightManager.MaxWeight, 1000d);
            //            weightFactors.Add(new Weight(gobWeight, WeightMethod.Add, WeightReason.GoblinPriority));
            //            break;
            //        case GoblinPriority.Kamikaze:
            //            // KAMIKAZE SUICIDAL TREASURE GOBLIN RAPE AHOY!
            //            weightFactors.Add(new Weight(WeightManager.MaxWeight, WeightMethod.Add, WeightReason.GoblinKamikaze));
            //            break;
            //    }
            //}

            return weightFactors;
        }
    }
}
