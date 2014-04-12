﻿using System;
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
                return false;

            if (!c_diaUnit.CommonData.IsValid)
                return false;

            if (c_CommonData.ACDGuid == -1)
                return false;

            // grab this first
            c_CurrentAnimation = c_diaUnit.CommonData.CurrentAnimation;

            // See if this is a boss
            c_unit_IsBoss = DataDictionary.BossIds.Contains(c_ActorSNO);

            // hax for Diablo_shadowClone
            c_unit_IsAttackable = c_InternalName.StartsWith("Diablo_shadowClone");

            try
            {
                if (c_diaUnit.Movement.IsValid)
                {
                    c_IsFacingPlayer = c_diaUnit.IsFacingPlayer;
                    c_Rotation = c_diaUnit.Movement.Rotation;
                    c_DirectionVector = c_diaUnit.Movement.DirectionVector;
                }
            }
            catch (Exception ex)
            {
                Logger.LogDebug(LogCategory.CacheManagement, "Error while reading Rotation/Facing: {0}", ex.ToString());
            }

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
                    Expires = DateTime.UtcNow.AddMinutes(60)
                });
            }

            try
            {
                CurrentCacheObject.IsBountyObjective = (c_CommonData.GetAttribute<int>(ActorAttributeType.BountyObjective) > 0);
            }
            catch (Exception)
            {
                Logger.LogDebug("Error refreshing IsNPC");
            }
            
            try
            {
                CurrentCacheObject.IsNPC = (c_CommonData.GetAttribute<int>(ActorAttributeType.IsNPC) > 0);
            }
            catch (Exception)
            {
                Logger.LogDebug("Error refreshing IsNPC");
            }

            try
            {
                CurrentCacheObject.NPCIsOperable = (c_CommonData.GetAttribute<int>(ActorAttributeType.NPCIsOperatable) > 0);
            }
            catch (Exception)
            {
                Logger.LogDebug("Error refreshing NPCIsOperable");
            }

            try
            {
                CurrentCacheObject.IsMinimapActive = CurrentCacheObject.Unit.CommonData.GetAttribute<int>(ActorAttributeType.MinimapActive) > 0;
            }
            catch (Exception ex)
            {
                Logger.LogDebug(LogCategory.CacheManagement, "Error reading IsMinimapActive for Unit sno:{0} raGuid:{1} name:{2} ex:{3}",
                    CurrentCacheObject.ActorSNO, CurrentCacheObject.RActorGuid, CurrentCacheObject.InternalName, ex.Message);
            }

            try
            {
                CurrentCacheObject.IsQuestMonster = CurrentCacheObject.Unit.CommonData.GetAttribute<int>(ActorAttributeType.QuestMonster) > 1;
            }
            catch (Exception ex)
            {
                Logger.LogDebug(LogCategory.CacheManagement, "Error reading IsQuestMonster for Unit sno:{0} raGuid:{1} name:{2} ex:{3}",
                    CurrentCacheObject.ActorSNO, CurrentCacheObject.RActorGuid, CurrentCacheObject.InternalName, ex.Message);
            }

            if ((teamId == 1 || teamId == 2 || teamId == 17))
            {
                AddToCache = false;
                c_IgnoreSubStep += "IsTeam" + teamId.ToString();
                return AddToCache;
            }

            /* Always refresh monster type */
            if (c_ObjectType != GObjectType.Player && !c_unit_IsBoss)
            {
                switch (c_CommonData.MonsterInfo.MonsterType)
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
                if (c_MonsterAffixes.HasFlag(MonsterAffixes.Shielding))
                    c_unit_HasShieldAffix = true;

            }
            double killRange;

            killRange = RefreshKillRadius();

            c_KillRange = killRange;


            // Only if at full health, else don't bother checking each loop
            // See if we already have this monster's size stored, if not get it and cache it
            if (!CacheData.MonsterSizes.TryGetValue(c_ActorSNO, out c_unit_MonsterSize))
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
            if (!DataDictionary.CustomObjectRadius.TryGetValue(c_ActorSNO, out c_Radius))
            {
                // Retrieve collision sphere radius, cached if possible
                if (!CacheData.CollisionSphere.TryGetValue(c_ActorSNO, out c_Radius))
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
                    CacheData.CollisionSphere.Add(c_ActorSNO, c_Radius);
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


            if (CurrentCacheObject.IsQuestMonster || CurrentCacheObject.IsBountyObjective)
                return AddToCache;

            // Extended kill radius after last fighting, or when we want to force a town run
            if ((Settings.Combat.Misc.ExtendedTrashKill && iKeepKillRadiusExtendedFor > 0) || ForceVendorRunASAP || TownRun.IsTryingToTownPortal())
            {
                if (CurrentCacheObject.RadiusDistance <= killRange && AddToCache)
                    AnyMobsInRange = true;
            }
            else
            {
                if (CurrentCacheObject.RadiusDistance <= Settings.Combat.Misc.NonEliteRange && AddToCache)
                    AnyMobsInRange = true;
            }
            if (c_unit_IsTreasureGoblin)
                AnyTreasureGoblinsPresent = true;

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
                CacheData.MonsterSizes.Add(c_ActorSNO, c_unit_MonsterSize);
            }
            else
            {
                c_unit_MonsterSize = MonsterSize.Unknown;
            }
        }

        private static void RefreshMonsterHealth()
        {
            if (!c_diaUnit.IsValid)
                return;

            // health calculations
            double maxHealth;
            // Get the max health of this unit, a cached version if available, if not cache it
            if (!CacheData.UnitMaxHealth.TryGetValue(c_RActorGuid, out maxHealth))
            {
                maxHealth = c_diaUnit.HitpointsMax;
                CacheData.UnitMaxHealth.Add(c_RActorGuid, maxHealth);
            }

            // Health calculations            
            c_HitPoints = c_diaUnit.HitpointsCurrent;

            // And finally put the two together for a current health percentage
            c_HitPointsPct = c_HitPoints / maxHealth;
        }

        private static bool RefreshUnitAttributes(bool AddToCache = true, DiaUnit unit = null)
        {


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
            if (!CacheData.UnitIsBurrowed.TryGetValue(c_RActorGuid, out isBurrowed))
            {
                isBurrowed = unit.IsBurrowed;
                // if the unit is NOT burrowed - we can attack them, add to cache (as IsAttackable)
                if (!isBurrowed)
                {
                    CacheData.UnitIsBurrowed.Add(c_RActorGuid, isBurrowed);
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
                if (!CacheData.UnitMonsterAffix.TryGetValue(c_RActorGuid, out affixFlags))
                {
                    try
                    {
                        affixFlags = c_CommonData.MonsterAffixes;
                        CacheData.UnitMonsterAffix.Add(c_RActorGuid, affixFlags);
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
        private static MonsterType RefreshMonsterType(bool addToDictionary)
        {
            SNORecordMonster monsterInfo = c_CommonData.MonsterInfo;
            MonsterType monsterType;
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
                if (addToDictionary)
                    CacheData.MonsterTypes.Add(c_ActorSNO, monsterType);
                else
                    CacheData.MonsterTypes[c_ActorSNO] = monsterType;
            }
            else
            {
                monsterType = MonsterType.Undead;
            }
            return monsterType;
        }
        private static bool RefreshStepCachedSummons(bool AddToCache)
        {
            if (c_diaUnit != null && c_diaUnit.IsValid)
            {

                if (!CacheData.SummonedByACDId.TryGetValue(c_ACDGUID, out c_SummonedByACDId))
                {
                    try
                    {
                        c_SummonedByACDId = c_diaUnit.SummonedByACDId;
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "Exception reading SummonedByACDId {0}", ex.ToString());
                    }
                }
                if (!CacheData.IsSummoner.TryGetValue(c_ACDGUID, out c_IsSummoner))
                {
                    try
                    {
                        c_IsSummoner = c_diaUnit.CommonData.GetAttribute<int>(ActorAttributeType.SummonerID) > 0;
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "Exception reading SummonerID {0}", ex.ToString());
                    }
                }

                if (c_SummonedByACDId == Player.ACDGuid)
                {
                    c_IsSummonedByPlayer = true;
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