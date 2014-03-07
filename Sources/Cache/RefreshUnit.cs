using System;
using System.Linq;
using Trinity.Combat.Abilities;
using Trinity.Config.Combat;
using Trinity.Technicals;
using Zeta.Game;
using Zeta.Game.Internals.Actors;
using Zeta.Game.Internals.SNO;

namespace Trinity
{
    public partial class Trinity
    {
        private static bool RefreshUnit(bool AddToCache)
        {
            AddToCache = true;

            if (c_diaUnit == null)
                return AddToCache;

            if (!c_diaUnit.IsValid)
                return AddToCache;

            if (!c_diaUnit.CommonData.IsValid)
                return AddToCache;

            // grab this first
            c_CurrentAnimation = c_diaUnit.CommonData.CurrentAnimation;

            // See if this is a boss
            c_unit_IsBoss = DataDictionary.BossIds.Contains(c_ActorSNO);

            // hax for Diablo_shadowClone
            c_unit_IsAttackable = c_InternalName.StartsWith("Diablo_shadowClone");
            c_IsFacingPlayer = c_diaUnit.IsFacingPlayer;
            c_Rotation = c_diaUnit.Movement.Rotation;

            if (c_CommonData.ACDGuid == -1)
            {
                AddToCache = false;
                return AddToCache;
            }

            // Dictionary based caching of monster types based on the SNO codes
            MonsterType monsterType;
            // See if we need to refresh the monster type or not
            bool notInCache = !dictionaryStoredMonsterTypes.TryGetValue(c_ActorSNO, out monsterType);
            // either we're in a quest area or not in cache
            bool refreshMonsterType = DataDictionary.QuestLevelAreaIds.Contains(Player.LevelAreaId) || notInCache;
            using (new PerformanceLogger("RefreshUnit.5"))
            {
                // If it's a boss and it was an ally, keep refreshing until it's not an ally
                // Because some bosses START as allied for cutscenes etc. until they become hostile
                if (c_unit_IsBoss && !refreshMonsterType)
                {
                    switch (monsterType)
                    {
                        case MonsterType.Ally:
                        case MonsterType.Scenery:
                        case MonsterType.Helper:
                        case MonsterType.Team:
                            refreshMonsterType = true;
                            break;
                    }
                }
            }
            using (new PerformanceLogger("RefreshUnit.6"))
            {
                // Now see if we do need to get new data for this boss or not
                if (refreshMonsterType)
                {
                    try
                    {
                        monsterType = RefreshMonsterType(c_CommonData, monsterType, notInCache);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "Safely handled exception getting monsterinfo and monstertype for unit {0} [{1}]", c_InternalName, c_ActorSNO);
                        Logger.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "{0}", ex);
                        Logger.Log(TrinityLogLevel.Verbose, LogCategory.CacheManagement, "ActorTypeAttempt={0}", c_diaUnit.ActorType);
                        AddToCache = false;
                    }
                }

                // Make sure it's a valid monster type
                if (c_ObjectType != GObjectType.Player)
                {
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
                    }
                }
            }
            // Force return here for un-attackable allies
            if (!AddToCache)
                return AddToCache;

            using (new PerformanceLogger("RefreshUnit.8"))
            {
                // Only set treasure goblins to true *IF* they haven't disabled goblins! Then check the SNO in the goblin hash list!
                c_unit_IsTreasureGoblin = false;
                // Flag this as a treasure goblin *OR* ignore this object altogether if treasure goblins are set to ignore
                if (DataDictionary.GoblinIds.Contains(c_ActorSNO))
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
                RefreshAffixes();

                // Is this something we should try to force leap/other movement abilities against?
                c_ForceLeapAgainst = false;
            }
            double killRange;

            killRange = RefreshKillRadius();

            c_KillRange = killRange;

            if (c_MonsterAffixes.HasFlag(MonsterAffixes.Shielding))
                c_unit_HasShieldAffix = true;

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
                    Logger.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "Safely handled exception getting monstersize info for unit {0} [{1}]", c_InternalName, c_ActorSNO);
                    Logger.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "{0}", ex);
                    AddToCache = false;
                    return AddToCache;
                }
            }
            if (!DataDictionary.InteractAtCustomRange.TryGetValue(c_ActorSNO, out c_Radius))
            {
                // Retrieve collision sphere radius, cached if possible
                if (!collisionSphereCache.TryGetValue(c_ActorSNO, out c_Radius))
                {
                    try
                    {
                        RefreshMonsterRadius();
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "Safely handled exception getting collisionsphere radius for unit {0} [{1}]", c_InternalName, c_ActorSNO);
                        Logger.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "{0}", ex);
                        AddToCache = false;
                        return AddToCache;
                    }
                    collisionSphereCache.Add(c_ActorSNO, c_Radius);
                }
            }

            RefreshMonsterHealth();

            // Unit is already dead
            if (c_HitPoints <= 0d && !c_unit_IsBoss)
            {
                AddToCache = false;
                c_IgnoreSubStep = "0HitPoints";
                return AddToCache;
            }

            if (c_diaUnit.IsDead)
            {
                AddToCache = false;
                c_IgnoreSubStep = "IsDead";
                return AddToCache;
            }

            AddToCache = RefreshUnitAttributes(AddToCache, c_diaUnit);

            if (!AddToCache)
                return AddToCache;


            // A "fake distance" to account for the large-object size of monsters
            c_RadiusDistance -= (float)c_Radius;
            if (c_RadiusDistance <= 1f)
                c_RadiusDistance = 1f;

            // Special flags to decide whether to target anything at all
            if (c_IsEliteRareUnique || c_unit_IsBoss)
                AnyElitesPresent = true;

            // Extended kill radius after last fighting, or when we want to force a town run
            if ((Settings.Combat.Misc.ExtendedTrashKill && iKeepKillRadiusExtendedFor > 0) || ForceVendorRunASAP || TownRun.IsTryingToTownPortal())
            {
                if (c_RadiusDistance <= killRange && AddToCache)
                    AnyMobsInRange = true;
            }
            else
            {
                if (c_RadiusDistance <= Settings.Combat.Misc.NonEliteRange && AddToCache)
                    AnyMobsInRange = true;
            }
            if (c_unit_IsTreasureGoblin)
                AnyTreasureGoblinsPresent = true;

            // Units with very high priority (1900+) allow an extra 50% on the non-elite kill slider range
            //if (!AnyMobsInRange && !AnyElitesPresent && !AnyTreasureGoblinsPresent && c_RadiusDistance <= (Settings.Combat.Misc.NonEliteRange * 1.5))
            //{
            //    int extraPriority;
            //    // Enable extended kill radius for specific unit-types
            //    if (DataDictionary.RangedMonsterIds.Contains(c_ActorSNO))
            //    {
            //        AnyMobsInRange = true;
            //    }
            //    if (!AnyMobsInRange && DataDictionary.MonsterCustomWeights.TryGetValue(c_ActorSNO, out extraPriority))
            //    {
            //        if (extraPriority >= 1900)
            //        {
            //            AnyMobsInRange = true;
            //        }
            //    }
            //}
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

        private static void RefreshMonsterHealth()
        {
            // health calculations
            double maxHealth;
            // Get the max health of this unit, a cached version if available, if not cache it
            if (!unitMaxHealthCache.TryGetValue(c_RActorGuid, out maxHealth))
            {
                maxHealth = c_diaUnit.HitpointsMax;
                unitMaxHealthCache.Add(c_RActorGuid, maxHealth);
            }

            // Health calculations
            c_HitPoints = c_diaUnit.HitpointsCurrent;

            // And finally put the two together for a current health percentage
            c_HitPointsPct = c_HitPoints / maxHealth;
        }

        private static bool RefreshUnitAttributes(bool AddToCache = true, DiaUnit unit = null)
        {
            /*
             *  TeamID  - check once for all units except bosses (which can potentially change teams - Belial, Cydea)
             */
            string teamIdHash = HashGenerator.GetGenericHash("teamId.RActorGuid=" + c_RActorGuid + ".ActorSNO=" + c_ActorSNO + ".WorldId=" + Player.WorldID);

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
            if (teamId == 1 || teamId == 2)
            {
                AddToCache = false;
                c_IgnoreSubStep += "IsTeam1|2+";
                return AddToCache;
            }

            if (!DataDictionary.IgnoreUntargettableAttribute.Contains(c_ActorSNO) && unit.IsUntargetable)
            {
                AddToCache = false;
                c_IgnoreSubStep = "IsUntargetable";
                return AddToCache;
            }

            // don't check for invulnerability on shielded and boss units, they are treated seperately
            if (!c_unit_HasShieldAffix && unit.IsInvulnerable)
            {
                AddToCache = false;
                c_IgnoreSubStep = "IsInvulnerable";
                return AddToCache;
            }

            bool isBurrowed = false;
            if (!unitBurrowedCache.TryGetValue(c_RActorGuid, out isBurrowed))
            {
                isBurrowed = unit.IsBurrowed;
                // if the unit is NOT burrowed - we can attack them, add to cache (as IsAttackable)
                if (!isBurrowed)
                {
                    unitBurrowedCache.Add(c_RActorGuid, isBurrowed);
                }
            }

            if (isBurrowed)
            {
                AddToCache = false;
                c_IgnoreSubStep = "IsBurrowed";
                return AddToCache;
            }

            // only check for DotDPS/Bleeding in certain conditions to save CPU for everyone else
            // barbs with rend
            // All WD's
            // Monks with Way of the Hundred Fists + Fists of Fury
            if (AddToCache &&
                ((Player.ActorClass == ActorClass.Barbarian && Hotbar.Contains(SNOPower.Barbarian_Rend)) ||
                Player.ActorClass == ActorClass.Witchdoctor ||
                (Player.ActorClass == ActorClass.Monk && HotbarSkills.AssignedSkills.Any(s => s.Power == SNOPower.Monk_WayOfTheHundredFists && s.RuneIndex == 0)))
                )
            {
                ////bool hasdotDPS = c_CommonData.GetAttribute<int>(ActorAttributeType.DOTDPS) != 0;
                //bool isBleeding = c_CommonData.GetAttribute<int>(ActorAttributeType.Bleeding) != 0;
                //c_HasDotDPS = hasdotDPS && isBleeding;
                bool hasdotDPS = c_CommonData.GetAttribute<int>(ActorAttributeType.DOTDPS) != 0;
                c_HasDotDPS = hasdotDPS;
            }
            return AddToCache;

        }
        private static double RefreshKillRadius()
        {
            // Cancel altogether if it's not even in range, unless it's a boss or an injured treasure goblin
            double killRange = CurrentBotKillRange;

            if (CombatBase.IsQuestingMode && killRange <= 45f)
                killRange = 45f;

            // Bosses get extra radius
            if (c_unit_IsBoss)
            {
                if (c_ActorSNO != 80509)
                    // Kulle Exception
                    killRange *= 1.5;
                // And even more if they're already injured
                if (c_HitPointsPct <= 0.98)
                    killRange *= 4;
                // And make sure we have a MINIMUM range for bosses - incase they are at screen edge etc + Kulle exception
                if (killRange <= 200 && c_ActorSNO != 80509)
                    killRange = 200;
            }
            // Special short-range list to ignore weakling mobs
            if (PlayerKiteDistance <= 0 && !GetHasBuff(SNOPower.Wizard_Archon) && DataDictionary.ShortRangeAttackMonsterIds.Contains(c_ActorSNO))
            {
                killRange = 12;
            }
            // Prevent long-range mobs beign ignored while they may be pounding on us
            if (killRange <= 30 && DataDictionary.RangedMonsterIds.Contains(c_ActorSNO))
                killRange = 120f;

            // Injured treasure goblins get a huge extra radius - since they don't stay on the map long if injured, anyway!
            if (c_unit_IsTreasureGoblin && (c_CentreDistance <= 60 || c_HitPointsPct <= 0.99))
            {
                c_ForceLeapAgainst = true;
                if (Settings.Combat.Misc.GoblinPriority <= GoblinPriority.Prioritize)
                    killRange *= 2.5;
                else
                    killRange *= 4;
                // Minimum distance of 60
                if (killRange <= 60) killRange = 60;
            }
            // Elitey type mobs and things
            else if ((c_unit_IsElite || c_unit_IsRare || c_unit_IsUnique || c_unit_IsMinion))
            {
                c_ForceLeapAgainst = true;

                // using new GUI slider for elite kill range
                killRange = Settings.Combat.Misc.EliteRange;

                // if we've damaged an elite and its still on screen, keep it
                if (c_HitPointsPct < 1 && killRange < 60)
                {
                    killRange = 60;
                }
            }
            // Safety for TownRun and UseTownPortalTag
            if (TownRun.IsTryingToTownPortal())
            {
                if (killRange <= V.F("Cache.TownPortal.KillRange")) killRange = V.F("Cache.TownPortal.KillRange");
            }
            return killRange;
        }
        private static void RefreshAffixes()
        {
            using (new PerformanceLogger("RefreshAffixes"))
            {
                MonsterAffixes affixFlags;
                if (!unitMonsterAffixCache.TryGetValue(c_RActorGuid, out affixFlags))
                {
                    try
                    {
                        affixFlags = c_CommonData.MonsterAffixes;
                        unitMonsterAffixCache.Add(c_RActorGuid, affixFlags);
                    }
                    catch (Exception ex)
                    {
                        affixFlags = MonsterAffixes.None;
                        Logger.Log(LogCategory.CacheManagement, "Handled Exception getting affixes for Monster SNO={0} Name={1} RAGuid={2}", c_ActorSNO, c_InternalName, c_RActorGuid);
                        Logger.Log(LogCategory.CacheManagement, ex.ToString());
                    }
                }

                c_unit_IsElite = affixFlags.HasFlag(MonsterAffixes.Elite);
                c_unit_IsRare = affixFlags.HasFlag(MonsterAffixes.Rare);
                c_unit_IsUnique = affixFlags.HasFlag(MonsterAffixes.Unique);
                c_unit_IsMinion = affixFlags.HasFlag(MonsterAffixes.Minion);
                // All-in-one flag for quicker if checks throughout
                c_IsEliteRareUnique = (c_unit_IsElite || c_unit_IsRare || c_unit_IsUnique || c_unit_IsMinion);

                c_MonsterAffixes = affixFlags;

            }
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
        private static bool RefreshStepCachedPlayerSummons(bool AddToCache)
        {
            if (c_diaUnit != null)
            {
                c_SummonedByACDId = c_diaUnit.SummonedByACDId;

                if (c_SummonedByACDId == Player.MyDynamicID)
                {
                    c_IsPlayerSummoned = true;
                }

                // Count up Mystic Allys, gargantuans, and zombies - if the player has those skills
                if (Player.ActorClass == ActorClass.Monk)
                {
                    if (Hotbar.Contains(SNOPower.X1_Monk_MysticAlly_v2) && DataDictionary.MysticAllyIds.Contains(c_ActorSNO))
                    {
                        if (c_diaUnit.SummonedByACDId == Player.MyDynamicID)
                            iPlayerOwnedMysticAlly++;
                        AddToCache = false;
                    }
                }
                // Count up Demon Hunter pets
                if (Player.ActorClass == ActorClass.DemonHunter)
                {
                    if (Hotbar.Contains(SNOPower.X1_DemonHunter_Companion) && DataDictionary.DemonHunterPetIds.Contains(c_ActorSNO))
                    {
                        if (c_diaUnit.SummonedByACDId == Player.MyDynamicID)
                            iPlayerOwnedDHPets++;
                        AddToCache = false;
                    }
                }
                // Count up zombie dogs and gargantuans next
                if (Player.ActorClass == ActorClass.Witchdoctor)
                {
                    if (Hotbar.Contains(SNOPower.Witchdoctor_Gargantuan) && DataDictionary.GargantuanIds.Contains(c_ActorSNO))
                    {
                        if (c_diaUnit.SummonedByACDId == Player.MyDynamicID)
                            iPlayerOwnedGargantuan++;
                        AddToCache = false;
                    }
                    if (Hotbar.Contains(SNOPower.Witchdoctor_SummonZombieDog) && DataDictionary.ZombieDogIds.Contains(c_ActorSNO))
                    {
                        if (c_diaUnit.SummonedByACDId == Player.MyDynamicID)
                            PlayerOwnedZombieDog++;
                        AddToCache = false;
                    }
                }
            }
            return AddToCache;
        }

    }
}
