using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using Trinity.Combat.Abilities;
using Trinity.Config.Combat;
using Trinity.DbProvider;
using Trinity.Helpers;
using Trinity.Technicals;
using Zeta.Bot.Logic;
using Zeta.Game;
using Zeta.Game.Internals.Actors;
using Zeta.Game.Internals.SNO;
using Logger = Trinity.Technicals.Logger;

namespace Trinity
{
    public partial class Trinity
    {
        private static bool RefreshUnit()
        {
            bool addToCache = true;

            if (!(c_diaObject is DiaUnit))
                return false;

            if (c_diaUnit == null)
                return false;

            if (!c_diaUnit.IsValid)
                return false;

            if (!c_diaUnit.CommonData.IsValid)
                return false;

            if (c_diaObject.CommonData.ACDGuid == -1)
                return false;

            // Always set this, otherwise we divide by zero later
            CurrentCacheObject.KillRange = CurrentBotKillRange;

            // grab this first
            c_CurrentAnimation = c_diaUnit.CommonData.CurrentAnimation;

            // See if this is a boss
            CurrentCacheObject.IsBoss = (DataDictionary.BossIds.Contains(CurrentCacheObject.ActorSNO) || c_diaUnit.Name.Contains("Boss"));
            if (CurrentCacheObject.IsBoss)
                CurrentCacheObject.KillRange = CurrentCacheObject.RadiusDistance + 10f;

            // hax for Diablo_shadowClone
            c_unit_IsAttackable = CurrentCacheObject.InternalName.StartsWith("Diablo_shadowClone");

            if (c_HitPoints <= 0d && !CurrentCacheObject.IsBoss)
                c_unit_IsAttackable = false;

            if (c_diaUnit.IsDead)
                c_unit_IsAttackable = false;

            try
            {
                if (c_diaUnit.Movement.IsValid)
                {
                    c_IsFacingPlayer = c_diaUnit.IsFacingPlayer;
                    c_Rotation = c_diaUnit.Movement.Rotation;
                    c_DirectionVector = c_diaUnit.Movement.DirectionVector;

                    CurrentCacheObject.IsFacingPlayer = c_IsFacingPlayer;
                }

                c_Rotation = c_diaUnit.Movement.Rotation;
            }
            catch (Exception ex)
            {
                Logger.LogDebug(LogCategory.CacheManagement, "Error while reading Rotation/Facing: {0}", ex.ToString());
            }

            /*
            *  TeamID  - check once for all units except bosses (which can potentially change teams - Belial, Cydea)
            */
            string teamIdHash = HashGenerator.GetGenericHash("teamId.RActorGuid=" + CurrentCacheObject.RActorGuid + ".ActorSNO=" + CurrentCacheObject.ActorSNO + ".WorldId=" + Player.WorldID);

            int teamId;
            if (!CurrentCacheObject.IsBoss && GenericCache.ContainsKey(teamIdHash))
            {
                teamId = (int)GenericCache.GetObject(teamIdHash).Value;
            }
            else
            {
                teamId = c_diaUnit.TeamId;

                GenericCache.AddToCache(new GenericCacheObject
                {
                    Key = teamIdHash,
                    Value = teamId,
                    Expires = DateTime.UtcNow.AddMinutes(60)
                });
            }

            CacheObjectIsBountyObjective();

            try
            {
                CurrentCacheObject.IsNPC = c_diaUnit.IsNPC;
            }
            catch (Exception)
            {
                Logger.LogDebug("Error refreshing IsNPC");
            }

            CacheUnitNPCIsOperatable();

            CacheObjectMinimapActive();

            try
            {
                CurrentCacheObject.IsQuestMonster = c_diaUnit.IsQuestMonster;
                if (CurrentCacheObject.IsQuestMonster)
                    CurrentCacheObject.KillRange = CurrentCacheObject.RadiusDistance + 10f;
            }
            catch (Exception ex)
            {
                Logger.LogDebug(LogCategory.CacheManagement, "Error reading IsQuestMonster for Unit sno:{0} raGuid:{1} name:{2} ex:{3}",
                    CurrentCacheObject.ActorSNO, CurrentCacheObject.RActorGuid, CurrentCacheObject.InternalName, ex.Message);
            }

            try
            {
                CurrentCacheObject.IsQuestGiver = c_diaUnit.IsQuestGiver;

                // Interact with quest givers, except when doing town-runs
                if (ZetaDia.CurrentAct == Act.OpenWorld && CurrentCacheObject.IsQuestGiver && !(WantToTownRun || ForceVendorRunASAP || BrainBehavior.IsVendoring))
                {
                    CurrentCacheObject.Type = GObjectType.Interactable;
                    CurrentCacheObject.Type = GObjectType.Interactable;
                    CurrentCacheObject.Radius = c_diaObject.CollisionSphere.Radius;
                    return true;
                }
            }
            catch (Exception)
            {
                Logger.LogDebug("Error refreshing IsQuestGiver");
            }

            if ((teamId == 1 || teamId == 2 || teamId == 17))
            {
                addToCache = false;
                c_IgnoreSubStep += "IsTeam" + teamId;
                return addToCache;
            }

            /* Always refresh monster type */
            if (CurrentCacheObject.Type != GObjectType.Player && !CurrentCacheObject.IsBoss)
            {
                switch (c_diaObject.CommonData.MonsterInfo.MonsterType)
                {
                    case MonsterType.Ally:
                    case MonsterType.Scenery:
                    case MonsterType.Helper:
                    case MonsterType.Team:
                        {
                            CurrentCacheObject.IsAlly = true;

                            addToCache = false;
                            c_IgnoreSubStep += "AllySceneryHelperTeam";
                            return addToCache;
                        }
                }
            }

            if (DataDictionary.FollowerIds.Contains(CurrentCacheObject.ActorSNO))
            {
                CurrentCacheObject.IsAlly = true;
    
                addToCache = false;
                c_IgnoreSubStep += "FollowerIds";
                return addToCache;
            }

            // Only set treasure goblins to true *IF* they haven't disabled goblins! Then check the SNO in the goblin hash list!
            c_unit_IsTreasureGoblin = false;
            // Flag this as a treasure goblin *OR* ignore this object altogether if treasure goblins are set to ignore
            if (DataDictionary.GoblinIds.Contains(CurrentCacheObject.ActorSNO))
            {
                if (Settings.Combat.Misc.GoblinPriority != 0)
                {
                    c_unit_IsTreasureGoblin = true;
                }
                else
                {
                    addToCache = false;
                    c_IgnoreSubStep += "IgnoreTreasureGoblins";
                    return addToCache;
                }
            }

            // Pull up the Monster Affix cached data
            RefreshAffixes();
            if (c_MonsterAffixes.HasFlag(MonsterAffixes.Shielding))
                c_unit_HasShieldAffix = true;

            // Only if at full health, else don't bother checking each loop
            // See if we already have this monster's size stored, if not get it and cache it
            if (!CacheData.MonsterSizes.TryGetValue(CurrentCacheObject.ActorSNO, out c_unit_MonsterSize))
            {
                try
                {
                    RefreshMonsterSize();
                }
                catch
                {
                    Logger.LogDebug("Error refreshing MonsterSize");
                }
            }

            RefreshMonsterHealth();

            DebugUtil.LogAnimation(CurrentCacheObject);

            // Unit is already dead
            if (c_HitPoints <= 0d && !CurrentCacheObject.IsBoss)
            {
                addToCache = false;
                c_IgnoreSubStep += "0HitPoints";
                return addToCache;
            }

            if (c_diaUnit.IsDead)
            {
                addToCache = false;
                c_IgnoreSubStep += "IsDead";
                return addToCache;
            }

            if (CurrentCacheObject.IsQuestMonster || CurrentCacheObject.IsBountyObjective)
                return true;

            addToCache = RefreshUnitAttributes(addToCache, c_diaUnit);

            if (!addToCache)
                return addToCache;

            // Set Kill range
            CurrentCacheObject.KillRange = SetKillRange();

            if (CurrentCacheObject.RadiusDistance <= CurrentCacheObject.KillRange)
                AnyMobsInRange = true;

            return addToCache;
        }

        private static void CacheUnitNPCIsOperatable()
        {
            try
            {
                CurrentCacheObject.NPCIsOperable = (c_diaObject.CommonData.GetAttribute<int>(ActorAttributeType.NPCIsOperatable) > 0);
            }
            catch (Exception)
            {
                Logger.LogDebug("Error refreshing NPCIsOperable");
            }
        }

        internal static Transform SetVector(Grid ctl)
        {
            return ctl.RenderTransform = new RotateTransform(180, ctl.RenderSize.Width / 2, ctl.RenderSize.Height / 2);
        }

        private static void RefreshMonsterSize()
        {
            SNORecordMonster monsterInfo = c_diaUnit.MonsterInfo;
            if (monsterInfo != null)
            {
                c_unit_MonsterSize = monsterInfo.MonsterSize;
                CacheData.MonsterSizes.Add(CurrentCacheObject.ActorSNO, c_unit_MonsterSize);
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
            if (!CacheData.UnitMaxHealth.TryGetValue(CurrentCacheObject.RActorGuid, out maxHealth))
            {
                maxHealth = c_diaUnit.HitpointsMax;
                CacheData.UnitMaxHealth.Add(CurrentCacheObject.RActorGuid, maxHealth);
            }

            // Health calculations            
            c_HitPoints = c_diaUnit.HitpointsCurrent;

            // And finally put the two together for a current health percentage
            c_HitPointsPct = c_HitPoints / maxHealth;
        }

        private static bool RefreshUnitAttributes(bool AddToCache = true, DiaUnit unit = null)
        {


            if (!DataDictionary.IgnoreUntargettableAttribute.Contains(CurrentCacheObject.ActorSNO) && unit.IsUntargetable)
            {
                AddToCache = false;
                c_IgnoreSubStep += "IsUntargetable";
                return AddToCache;
            }

            // don't check for invulnerability on shielded and boss units, they are treated seperately
            if (!c_unit_HasShieldAffix && unit.IsInvulnerable)
            {
                AddToCache = false;
                c_IgnoreSubStep += "IsInvulnerable";
                return AddToCache;
            }

            bool isBurrowed = false;
            if (!CacheData.UnitIsBurrowed.TryGetValue(CurrentCacheObject.RActorGuid, out isBurrowed))
            {
                isBurrowed = unit.IsBurrowed;
                // if the unit is NOT burrowed - we can attack them, add to cache (as IsAttackable)
                if (!isBurrowed)
                {
                    CacheData.UnitIsBurrowed.Add(CurrentCacheObject.RActorGuid, isBurrowed);
                }
            }

            if (isBurrowed)
            {
                AddToCache = false;
                c_IgnoreSubStep += "IsBurrowed";
                return AddToCache;
            }

            // only check for DotDPS/Bleeding in certain conditions to save CPU for everyone else
            // barbs with rend
            // All WD's
            // Monks with Way of the Hundred Fists + Fists of Fury
            if (AddToCache &&
                ((Player.ActorClass == ActorClass.Barbarian && Hotbar.Contains(SNOPower.Barbarian_Rend)) ||
                Player.ActorClass == ActorClass.Witchdoctor ||
                (Player.ActorClass == ActorClass.Monk && CacheData.Hotbar.ActiveSkills.Any(s => s.Power == SNOPower.Monk_WayOfTheHundredFists && s.RuneIndex == 0)))
                )
            {
                bool hasdotDPS = CacheObjectHasDOTDPS();
                c_HasDotDPS = hasdotDPS;
            }

            // Kite (can make it in navHelper, avoid to re loop twice)
            if (CurrentCacheObject.Position.Distance2D(Trinity.Player.Position) < CombatBase.KiteDistance * 0.6 && CurrentCacheObject.IsInLineOfSight(true))
            {
                if (CurrentCacheObject.IsBoss && CombatBase.KiteMode != KiteMode.Never)
                {
                    Trinity.Player.NeedToKite = true;
                }
                else if (CurrentCacheObject.IsBossOrEliteRareUnique && (CombatBase.KiteMode == KiteMode.Elites || CombatBase.KiteMode == KiteMode.Always))
                {
                    Trinity.Player.NeedToKite = true;
                }
                else if (CombatBase.KiteMode == KiteMode.Always)
                {
                    Trinity.Player.NeedToKite = true;
                }
            }

            return AddToCache;

        }

        private static bool CacheObjectHasDOTDPS()
        {
            return c_diaObject.CommonData.GetAttribute<int>(ActorAttributeType.DOTDPS) != 0;
        }
        private static double SetKillRange()
        {
            // Always within kill range if in the NoCheckKillRange list!
            if (DataDictionary.NoCheckKillRange.Contains(CurrentCacheObject.ActorSNO))
                return CurrentCacheObject.RadiusDistance + 100f;

            double killRange;

            // Bosses, always kill
            if (CurrentCacheObject.IsBoss)
            {
                return CurrentCacheObject.RadiusDistance + 100f;
            }

            // Elitey type mobs and things
            if ((c_unit_IsElite || c_unit_IsRare || c_unit_IsUnique))
            {
                // using new GUI slider for elite kill range
                killRange = Settings.Combat.Misc.EliteRange;
            }
            else
            {
                killRange = CurrentBotKillRange;
            }

            if (!TownRun.IsTryingToTownPortal())
                return killRange;

            // Safety for TownRuns
            if (killRange <= V.F("Cache.TownPortal.KillRange")) killRange = V.F("Cache.TownPortal.KillRange");
            return killRange;
        }
        private static void RefreshAffixes()
        {
            MonsterAffixes affixFlags;
            if (!CacheData.UnitMonsterAffix.TryGetValue(CurrentCacheObject.RActorGuid, out affixFlags))
            {
                try
                {
                    affixFlags = c_diaObject.CommonData.MonsterAffixes;
                    CacheData.UnitMonsterAffix.Add(CurrentCacheObject.RActorGuid, affixFlags);
                }
                catch (Exception ex)
                {
                    affixFlags = MonsterAffixes.None;
                    Logger.Log(LogCategory.CacheManagement, "Handled Exception getting affixes for Monster SNO={0} Name={1} RAGuid={2}", CurrentCacheObject.ActorSNO, CurrentCacheObject.InternalName, CurrentCacheObject.RActorGuid);
                    Logger.Log(LogCategory.CacheManagement, ex.ToString());
                }
            }

            c_unit_IsElite = affixFlags.HasFlag(MonsterAffixes.Elite);
            c_unit_IsRare = affixFlags.HasFlag(MonsterAffixes.Rare);
            c_unit_IsUnique = affixFlags.HasFlag(MonsterAffixes.Unique);
            c_unit_IsMinion = affixFlags.HasFlag(MonsterAffixes.Minion);
            // All-in-one flag for quicker if checks throughout
            c_IsEliteRareUnique = (c_unit_IsElite || c_unit_IsRare || c_unit_IsUnique || c_unit_IsMinion || DataDictionary.EliteRareUniqueIds.Contains(CurrentCacheObject.ActorSNO));

            c_MonsterAffixes = affixFlags;
        }
        private static MonsterType RefreshMonsterType(bool addToDictionary)
        {
            SNORecordMonster monsterInfo = c_diaObject.CommonData.MonsterInfo;
            MonsterType monsterType;
            if (monsterInfo != null)
            {
                // Force Jondar as an undead, since Diablo 3 sticks him as a permanent ally
                if (CurrentCacheObject.ActorSNO == 86624)
                {
                    monsterType = MonsterType.Undead;
                }
                else
                {
                    monsterType = monsterInfo.MonsterType;
                }
                // Is this going to be a new dictionary entry, or updating one already existing?
                if (addToDictionary)
                    CacheData.MonsterTypes.Add(CurrentCacheObject.ActorSNO, monsterType);
                else
                    CacheData.MonsterTypes[CurrentCacheObject.ActorSNO] = monsterType;
            }
            else
            {
                monsterType = MonsterType.Undead;
            }
            return monsterType;
        }
        private static bool RefreshStepCachedSummons()
        {
            if (c_diaUnit.IsFullyValid())
            {
                try
                {
                    CurrentCacheObject.SummonedByACDId = c_diaUnit.SummonedByACDId;
                }
                catch
                {
                    // Only part of a ReadProcessMemory or WriteProcessMemory request was completed
                }
                try
                {
                    CurrentCacheObject.IsSummoner = c_diaUnit.SummonerId > 0;
                }
                catch
                {
                    // Only part of a ReadProcessMemory or WriteProcessMemory request was completed 
                }

                // SummonedByACDId is not ACDGuid, it's DynamicID
                if (CurrentCacheObject.SummonedByACDId == Player.MyDynamicID)
                {
                    CurrentCacheObject.IsSummonedByPlayer = true;
                }

                if (CurrentCacheObject.IsSummonedByPlayer)
                {
                // Count up Mystic Allys, gargantuans, and zombies - if the player has those skills
                if (Player.ActorClass == ActorClass.Monk)
                {
                    if (DataDictionary.MysticAllyIds.Contains(CurrentCacheObject.ActorSNO))
                    {
                            PlayerOwnedMysticAllyCount++;
                            c_IgnoreSubStep += "IsPlayerSummoned";
                        return false;
                    }
                }
                    // Count up Demon Hunter sentries
                if (Player.ActorClass == ActorClass.DemonHunter)
                {
                        if (DataDictionary.DemonHunterSentryIds.Contains(CurrentCacheObject.ActorSNO))
                    {
                            if (CurrentCacheObject.Distance < 75f)
                        {
                                PlayerOwnedDHSentryCount++;
                        }

                            CacheData.SentryTurret.Add(new CacheObstacleObject()
                            {
                                ActorSNO = CurrentCacheObject.ActorSNO,
                                RActorGUID = CurrentCacheObject.RActorGuid,
                                Position = CurrentCacheObject.Position,
                                Radius = CurrentCacheObject.Radius,
                            });

                            c_IgnoreSubStep += "IsPlayerSummoned";
                        return false;
                    }
                        if (DataDictionary.DemonHunterPetIds.Contains(CurrentCacheObject.ActorSNO))
                {
                            PlayerOwnedDHPetsCount++;
                            c_IgnoreSubStep += "IsPlayerSummoned";
                        return false;
                    }
                }
                // Count up Wiz hydras
                if (Player.ActorClass == ActorClass.Wizard)
                {
                    if (DataDictionary.WizardHydraIds.Contains(CurrentCacheObject.ActorSNO))
                    {
                            PlayerOwnedHydraCount++;
                            c_IgnoreSubStep += "IsPlayerSummoned";
                        return false;
                    }
                }
                // Count up zombie dogs and gargantuans next
                if (Player.ActorClass == ActorClass.Witchdoctor)
                {
                        if (DataDictionary.BigBadVoodooIds.Contains(CurrentCacheObject.ActorSNO))
                    {
                            CacheData.Voodoo.Add(new CacheObstacleObject()
                        {
                                ActorSNO = CurrentCacheObject.ActorSNO,
                                RActorGUID = CurrentCacheObject.RActorGuid,
                                Position = CurrentCacheObject.Position,
                                Radius = CurrentCacheObject.Radius,
                            });

                            c_IgnoreSubStep += "IsPlayerSummoned";
                        return false;
                    }
                    if (DataDictionary.ZombieDogIds.Contains(CurrentCacheObject.ActorSNO))
                    {
                            PlayerOwnedZombieDogCount++;
                            c_IgnoreSubStep += "IsPlayerSummoned";
                            return false;
                        }
                        if (DataDictionary.GargantuanIds.Contains(CurrentCacheObject.ActorSNO))
                        {
                            PlayerOwnedGargantuanCount++;
                            c_IgnoreSubStep += "IsPlayerSummoned";
                            return false;
                        }
                        if (DataDictionary.FetishArmyIds.Contains(CurrentCacheObject.ActorSNO))
                        {
                            Trinity.PlayerOwnedFetishCount++;
                            c_IgnoreSubStep += "IsPlayerSummoned";
                        return false;
                    }
                }

                    c_IgnoreSubStep += "IsPlayerSummoned";
                    return false;
                }
            }
            return true;
        }
    }
}
