using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GilesTrinity.Settings.Combat;
using GilesTrinity.Technicals;
using Zeta.Internals.Actors;
using Zeta.Internals.SNO;

namespace GilesTrinity
{
    public partial class GilesTrinity
    {
        private static bool RefreshGilesUnit(bool AddToCache)
        {
            AddToCache = true;

            // See if this is a boss
            c_unit_IsBoss = hashBossSNO.Contains(c_ActorSNO);

            // hax for Diablo_shadowClone
            c_unit_IsAttackable = c_InternalName.StartsWith("Diablo_shadowClone");

            // Prepare the fake object for target handler
            if (FakeObject == null)
                FakeObject = c_diaUnit;

            if (c_CommonData.ACDGuid == -1)
            {
                AddToCache = false;
                return AddToCache;
            }

            // Dictionary based caching of monster types based on the SNO codes
            MonsterType monsterType;
            // See if we need to refresh the monster type or not
            bool bAddToDictionary = !dictionaryStoredMonsterTypes.TryGetValue(c_ActorSNO, out monsterType);
            bool bRefreshMonsterType = bAddToDictionary;
            using (new PerformanceLogger("RefreshUnit.5"))
            {
                // If it's a boss and it was an ally, keep refreshing until it's not an ally
                // Because some bosses START as allied for cutscenes etc. until they become hostile
                if (c_unit_IsBoss && !bRefreshMonsterType)
                {
                    switch (monsterType)
                    {
                        case MonsterType.Ally:
                        case MonsterType.Scenery:
                        case MonsterType.Helper:
                        case MonsterType.Team:
                            bRefreshMonsterType = true;
                            break;
                    }
                }
            }
            using (new PerformanceLogger("RefreshUnit.6"))
            {
                // Now see if we do need to get new data for this boss or not
                if (bRefreshMonsterType)
                {
                    try
                    {
                        monsterType = RefreshMonsterType(c_CommonData, monsterType, bAddToDictionary);
                    }
                    catch (Exception ex)
                    {
                        DbHelper.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "Safely handled exception getting monsterinfo and monstertype for unit {0} [{1}]", c_InternalName, c_ActorSNO);
                        DbHelper.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "{0}", ex);
                        DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.CacheManagement, "ActorTypeAttempt={0}", c_diaUnit.ActorType);
                        AddToCache = false;
                    }
                }

                // Make sure it's a valid monster type
                switch (monsterType)
                {
                    case MonsterType.Ally:
                    case MonsterType.Scenery:
                    case MonsterType.Helper:
                    case MonsterType.Team:
                        {
                            AddToCache = false;
                            c_IgnoreSubStep = "AllySceneryHelperTeam";
                            return AddToCache;
                        }
                    //break;
                }
            }
            // Force return here for un-attackable allies
            if (!AddToCache)
                return AddToCache;

            MonsterAffixes monsterAffixes;
            using (new PerformanceLogger("RefreshUnit.8"))
            {
                // Only set treasure goblins to true *IF* they haven't disabled goblins! Then check the SNO in the goblin hash list!
                c_unit_IsTreasureGoblin = false;
                // Flag this as a treasure goblin *OR* ignore this object altogether if treasure goblins are set to ignore
                if (hashActorSNOGoblins.Contains(c_ActorSNO))
                {
                    if (Settings.Combat.Misc.GoblinPriority != 0)
                    {
                        c_unit_IsTreasureGoblin = true;
                    }
                    else
                    {
                        AddToCache = false;
                        c_IgnoreSubStep = "IgnoreTreasureGoblins";
                        return AddToCache;
                    }
                }
                // Pull up the Monster Affix cached data
                monsterAffixes = RefreshAffixes(c_CommonData);

                /*
                 * 
                 * This should be moved to HandleTarget
                 * 
                 */
                if (PlayerStatus.ActorClass == ActorClass.Barbarian && Hotbar.Contains(SNOPower.Barbarian_WrathOfTheBerserker) && GilesUseTimer(SNOPower.Barbarian_WrathOfTheBerserker, true))
                {
                    //WotB only used on Arcane, Frozen, Jailer, Molten and Electrified+Reflect Damage elites
                    if (monsterAffixes.HasFlag(MonsterAffixes.ArcaneEnchanted) || monsterAffixes.HasFlag(MonsterAffixes.Frozen) ||
                        monsterAffixes.HasFlag(MonsterAffixes.Jailer) || monsterAffixes.HasFlag(MonsterAffixes.Molten) ||
                       (monsterAffixes.HasFlag(MonsterAffixes.Electrified) && monsterAffixes.HasFlag(MonsterAffixes.ReflectsDamage)) ||
                        //Bosses and uber elites
                        c_unit_IsBoss || c_ActorSNO == 256015 || c_ActorSNO == 256000 || c_ActorSNO == 255996 ||
                        //...or more than 4 elite mobs in range (only elites/rares/uniques, not minions!)
                        ElitesWithinRange[RANGE_50] > 4)
                        shouldUseBerserkerPower = true;
                }
                else
                    shouldUseBerserkerPower = false;

                // Is this something we should try to force leap/other movement abilities against?
                c_ForceLeapAgainst = false;
            }
            double dUseKillRadius;

            dUseKillRadius = RefreshKillRadius();

            c_KillRange = dUseKillRadius;

            if (monsterAffixes.HasFlag(MonsterAffixes.Shielding))
                c_unit_IsShielded = true;

            // Only if at full health, else don't bother checking each loop
            // See if we already have this monster's size stored, if not get it and cache it
            if (!dictionaryStoredMonsterSizes.TryGetValue(c_ActorSNO, out c_unit_MonsterSize))
            {
                try
                {
                    RefreshMonsterSize();
                }
                catch (Exception ex)
                {
                    DbHelper.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "Safely handled exception getting monstersize info for unit {0} [{1}]", c_InternalName, c_ActorSNO);
                    DbHelper.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "{0}", ex);
                    AddToCache = false;
                    return AddToCache;
                }
            }
            // Retrieve collision sphere radius, cached if possible
            if (!dictGilesCollisionSphereCache.TryGetValue(c_ActorSNO, out c_Radius))
            {
                try
                {
                    RefreshMonsterRadius();
                }
                catch (Exception ex)
                {
                    DbHelper.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "Safely handled exception getting collisionsphere radius for unit {0} [{1}]", c_InternalName, c_ActorSNO);
                    DbHelper.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "{0}", ex);
                    AddToCache = false;
                    return AddToCache;
                }
                dictGilesCollisionSphereCache.Add(c_ActorSNO, c_Radius);
            }

            double dThisMaxHealth = RefreshMonsterHealth();

            // And finally put the two together for a current health percentage
            c_HitPointsPct = c_HitPoints / dThisMaxHealth;

            // Unit is already dead
            if (c_HitPoints <= 0d && !c_unit_IsBoss)
            {
                AddToCache = false;
                c_IgnoreSubStep = "0HitPoints";

                // return here immediately
                return AddToCache;
            }
            // only refresh active attributes if within killrange
            AddToCache = RefreshUnitAttributes(AddToCache, c_diaUnit);

            if (!AddToCache)
                return AddToCache;

            c_CurrentAnimation = c_diaUnit.CommonData.CurrentAnimation;


            // A "fake distance" to account for the large-object size of monsters
            c_RadiusDistance -= (float)c_Radius;
            if (c_RadiusDistance <= 1f)
                c_RadiusDistance = 1f;
            // All-in-one flag for quicker if checks throughout
            c_IsEliteRareUnique = (c_unit_IsElite || c_unit_IsRare || c_unit_IsUnique || c_unit_IsMinion);
            // Special flags to decide whether to target anything at all
            if (c_IsEliteRareUnique || c_unit_IsBoss)
                bAnyChampionsPresent = true;
            // Extended kill radius after last fighting, or when we want to force a town run
            if ((Settings.Combat.Misc.ExtendedTrashKill && iKeepKillRadiusExtendedFor > 0) || ForceVendorRunASAP || TownRun.IsTryingToTownPortal())
            {
                if (c_RadiusDistance <= dUseKillRadius && AddToCache)
                    bAnyMobsInCloseRange = true;
            }
            else
            {
                if (c_RadiusDistance <= Settings.Combat.Misc.NonEliteRange && AddToCache)
                    bAnyMobsInCloseRange = true;
            }
            if (c_unit_IsTreasureGoblin)
                bAnyTreasureGoblinsPresent = true;
            // Units with very high priority (1900+) allow an extra 50% on the non-elite kill slider range
            if (!bAnyMobsInCloseRange && !bAnyChampionsPresent && !bAnyTreasureGoblinsPresent && c_RadiusDistance <= (Settings.Combat.Misc.NonEliteRange * 1.5))
            {
                int iExtraPriority;
                // Enable extended kill radius for specific unit-types
                if (hashActorSNORanged.Contains(c_ActorSNO))
                {
                    bAnyMobsInCloseRange = true;
                }
                if (!bAnyMobsInCloseRange && dictActorSNOPriority.TryGetValue(c_ActorSNO, out iExtraPriority))
                {
                    if (iExtraPriority >= 1900)
                    {
                        bAnyMobsInCloseRange = true;
                    }
                }
            }
            return AddToCache;
        }

        private static void RefreshMonsterRadius()
        {
            c_Radius = c_diaUnit.CollisionSphere.Radius;
            // Take 6 from the radius
            if (!c_unit_IsBoss)
                c_Radius -= 6f;
            // Minimum range clamp
            if (c_Radius <= 1f)
                c_Radius = 1f;
            // Maximum range clamp
            if (c_Radius >= 20f)
                c_Radius = 20f;
        }

        private static void RefreshMonsterSize()
        {
            SNORecordMonster monsterInfo = c_diaUnit.MonsterInfo;
            if (monsterInfo != null)
            {
                c_unit_MonsterSize = monsterInfo.MonsterSize;
                dictionaryStoredMonsterSizes.Add(c_ActorSNO, c_unit_MonsterSize);
            }
            else
            {
                c_unit_MonsterSize = MonsterSize.Unknown;
            }
        }

        private static double RefreshMonsterHealth()
        {
            // health calculations
            double dThisMaxHealth;
            // Get the max health of this unit, a cached version if available, if not cache it
            if (!dictGilesMaxHealthCache.TryGetValue(c_RActorGuid, out dThisMaxHealth))
            {
                dThisMaxHealth = c_diaUnit.HitpointsMax;
                dictGilesMaxHealthCache.Add(c_RActorGuid, dThisMaxHealth);
            }
            // Now try to get the current health - using temporary and intelligent caching
            // Health calculations
            c_HitPoints = c_diaUnit.HitpointsCurrent;
            return dThisMaxHealth;
        }

        private static bool RefreshUnitAttributes(bool AddToCache = true, DiaUnit unit = null)
        {
            /*
             *  TeamID  - check once for all units except bosses (which can potentially change teams - Belial, Cydea)
             */
            string teamIdHash = HashGenerator.GetGenericHash("teamId.RActorGuid=" + c_RActorGuid + ".ActorSNO=" + c_ActorSNO + ".WorldId=" + PlayerStatus.WorldID);

            int teamId = 0;
            if (!c_unit_IsBoss && GenericCache.ContainsKey(teamIdHash))
            {
                teamId = (int)GenericCache.GetObject(teamIdHash).Value;
            }
            else
            {
                teamId = c_CommonData.GetAttribute<int>(ActorAttributeType.TeamID);

                GenericCache.AddToCache(new GenericCacheObject()
                {
                    Key = teamIdHash,
                    Value = teamId,
                    Expires = DateTime.Now.AddMinutes(60)
                });
            }
            if (teamId == 1)
            {
                AddToCache = false;
                c_IgnoreSubStep += "IsTeam1+";
                return AddToCache;
            }



            if (unit.IsUntargetable)
            {
                AddToCache = false;
                c_IgnoreSubStep = "IsUntargetable";
                return AddToCache;
            }

            // don't check for invulnerability on shielded units, they are treated seperately
            if (!c_unit_IsShielded && unit.IsInvulnerable)
            {
                AddToCache = false;
                c_IgnoreSubStep = "IsInvulnerable";
                return AddToCache;
            }

            bool isBurrowed = false;
            if (!dictGilesBurrowedCache.TryGetValue(c_RActorGuid, out isBurrowed))
            {
                isBurrowed = unit.IsBurrowed;
                // if the unit is NOT burrowed - we can attack them, add to cache (as IsAttackable)
                if (!isBurrowed)
                {
                    dictGilesBurrowedCache.Add(c_RActorGuid, isBurrowed);
                }
            }

            if (isBurrowed)
            {
                AddToCache = false;
                c_IgnoreSubStep = "IsBurrowed";
                return AddToCache;
            }

            // only check for DotDPS/Bleeding in certain conditions to save CPU for everyone else
            if (AddToCache && ((PlayerStatus.ActorClass == ActorClass.Barbarian && Hotbar.Contains(SNOPower.Barbarian_Rend)) || PlayerStatus.ActorClass == ActorClass.WitchDoctor))
            {
                bool hasdotDPS = c_CommonData.GetAttribute<int>(ActorAttributeType.DOTDPS) != 0;
                bool isBleeding = c_CommonData.GetAttribute<int>(ActorAttributeType.Bleeding) != 0;
                c_HasDotDPS = hasdotDPS && isBleeding;
            }
            return AddToCache;

        }
        private static double RefreshKillRadius()
        {
            // Cancel altogether if it's not even in range, unless it's a boss or an injured treasure goblin
            double dUseKillRadius = iCurrentMaxKillRadius;
            // Bosses get extra radius
            if (c_unit_IsBoss)
            {
                if (c_ActorSNO != 80509)
                    // Kulle Exception
                    dUseKillRadius *= 1.5;
                // And even more if they're already injured
                if (c_HitPointsPct <= 0.98)
                    dUseKillRadius *= 4;
                // And make sure we have a MINIMUM range for bosses - incase they are at screen edge etc.
                if (dUseKillRadius <= 200)
                    if (c_ActorSNO != 80509)
                        // Kulle Exception
                        dUseKillRadius = 200;
            }
            // Special short-range list to ignore weakling mobs
            if (PlayerKiteDistance <= 0 && !GetHasBuff(SNOPower.Wizard_Archon))
            {
                if (hashActorSNOShortRangeOnly.Contains(c_ActorSNO))
                    dUseKillRadius = 12;
            }
            // Prevent long-range mobs beign ignored while they may be pounding on us
            if (dUseKillRadius <= 30 && hashActorSNORanged.Contains(c_ActorSNO))
                dUseKillRadius = 120f;

            // Injured treasure goblins get a huge extra radius - since they don't stay on the map long if injured, anyway!
            if (c_unit_IsTreasureGoblin && (c_CentreDistance <= 60 || c_HitPointsPct <= 0.99))
            {
                c_ForceLeapAgainst = true;
                if (Settings.Combat.Misc.GoblinPriority <= GoblinPriority.Prioritize)
                    dUseKillRadius *= 2.5;
                else
                    dUseKillRadius *= 4;
                // Minimum distance of 60
                if (dUseKillRadius <= 60) dUseKillRadius = 60;
            }
            // Elitey type mobs and things
            else if ((c_unit_IsElite || c_unit_IsRare || c_unit_IsUnique || c_unit_IsMinion))
            {
                c_ForceLeapAgainst = true;
                if (c_HitPointsPct <= 0.99)
                {
                    dUseKillRadius *= 2;
                    if (dUseKillRadius <= 150) dUseKillRadius = 150;
                }
                else
                {
                    if (dUseKillRadius <= 120) dUseKillRadius = 120;
                }
            }
            // Safety for TownRun and UseTownPortalTag
            if (TownRun.IsTryingToTownPortal())
            {
                if (dUseKillRadius <= 90) dUseKillRadius = 90;
            }
            return dUseKillRadius;
        }
        private static MonsterAffixes RefreshAffixes(ACD acd)
        {
            MonsterAffixes affixFlags;
            if (!dictGilesMonsterAffixCache.TryGetValue(c_RActorGuid, out affixFlags))
            {
                try
                {
                    affixFlags = acd.MonsterAffixes;
                    dictGilesMonsterAffixCache.Add(c_RActorGuid, affixFlags);
                }
                catch (Exception ex)
                {
                    affixFlags = MonsterAffixes.None;
                    DbHelper.Log(LogCategory.CacheManagement, "Handled Exception getting affixes for Monster SNO={0} Name={1} RAGuid={2}", c_ActorSNO, c_InternalName, c_RActorGuid);
                    DbHelper.Log(LogCategory.CacheManagement, ex.ToString());
                }
            }
            c_unit_IsElite = affixFlags.HasFlag(MonsterAffixes.Elite);
            c_unit_IsRare = affixFlags.HasFlag(MonsterAffixes.Rare);
            c_unit_IsUnique = affixFlags.HasFlag(MonsterAffixes.Unique);
            c_unit_IsMinion = affixFlags.HasFlag(MonsterAffixes.Minion);
            return affixFlags;
        }
        private static MonsterType RefreshMonsterType(ACD tempCommonData, MonsterType monsterType, bool bAddToDictionary)
        {
            SNORecordMonster monsterInfo = tempCommonData.MonsterInfo;
            if (monsterInfo != null)
            {
                // Force Jondar as an undead, since Diablo 3 sticks him as a permanent ally
                if (c_ActorSNO == 86624)
                {
                    monsterType = MonsterType.Undead;
                }
                else
                {
                    monsterType = monsterInfo.MonsterType;
                }
                // Is this going to be a new dictionary entry, or updating one already existing?
                if (bAddToDictionary)
                    dictionaryStoredMonsterTypes.Add(c_ActorSNO, monsterType);
                else
                    dictionaryStoredMonsterTypes[c_ActorSNO] = monsterType;
            }
            else
            {
                monsterType = MonsterType.Undead;
            }
            return monsterType;
        }


    }
}
