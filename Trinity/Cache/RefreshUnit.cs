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

            if (c_CommonData.ACDGuid == -1)
                return false;

            // Always set this, otherwise we divide by zero later
            c_CacheObject.KillRange = CurrentBotKillRange;


            // See if this is a boss
            using (new MemorySpy("RefreshUnit().CheckIsBoss"))
            {
                c_CacheObject.IsBoss = (DataDictionary.BossIds.Contains(c_CacheObject.ActorSNO) || c_diaUnit.Name.Contains("Boss"));
                if (c_CacheObject.IsBoss)
                    c_CacheObject.KillRange = c_CacheObject.RadiusDistance + 10f;
            }


            // hax for Diablo_shadowClone
            using (new MemorySpy("RefreshUnit().CheckIsAttackable"))
            {
                c_unit_IsAttackable = c_CacheObject.InternalName.StartsWith("Diablo_shadowClone");
            }

            /*
            *  TeamID  - check once for all units except bosses (which can potentially change teams - Belial, Cydea)
            */
            int teamId;
            using (new MemorySpy("RefreshUnit().CheckIsTeamId"))
            {
                string teamIdHash = HashGenerator.GetGenericHash("teamId.RActorGuid=" + c_CacheObject.RActorGuid + ".ActorSNO=" + c_CacheObject.ActorSNO + ".WorldId=" + Player.WorldID);

                if (!c_CacheObject.IsBoss && GenericCache.ContainsKey(teamIdHash))
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
            }

            using (new MemorySpy("RefreshUnit().CheckIsBountyObjective"))
            {
                CacheObjectIsBountyObjective();
            }

            using (new MemorySpy("RefreshUnit().CheckIsNPC"))
            {
                try
                {
                    c_CacheObject.IsNPC = c_diaUnit.IsNPC;
                }
                catch (Exception)
                {
                    Logger.LogDebug("Error refreshing IsNPC");
                }
            }

            using (new MemorySpy("RefreshUnit().CheckIsNPCOperable"))
            {
                CacheUnitNPCIsOperatable();
            }

            using (new MemorySpy("RefreshUnit().CheckMinimapActive"))
            {
                CacheObjectMinimapActive();
            }

            using (new MemorySpy("RefreshUnit().CheckIsQuestMonster"))
            {
                try
                {
                    c_CacheObject.IsQuestMonster = c_diaUnit.IsQuestMonster;
                    if (c_CacheObject.IsQuestMonster)
                        c_CacheObject.KillRange = c_CacheObject.RadiusDistance + 10f;
                }
                catch (Exception ex)
                {
                    Logger.LogDebug(LogCategory.CacheManagement, "Error reading IsQuestMonster for Unit sno:{0} raGuid:{1} name:{2} ex:{3}",
                        c_CacheObject.ActorSNO, c_CacheObject.RActorGuid, c_CacheObject.InternalName, ex.Message);
                }
            }

            using (new MemorySpy("RefreshUnit().CheckIsQuestGiver"))
            {
                try
                {
                    c_CacheObject.IsQuestGiver = c_diaUnit.IsQuestGiver;

                    // Interact with quest givers, except when doing town-runs
                    if (ZetaDia.CurrentAct == Act.OpenWorld && c_CacheObject.IsQuestGiver && !(WantToTownRun || ForceVendorRunASAP || BrainBehavior.IsVendoring))
                    {
                        c_CacheObject.Type = GObjectType.Interactable;
                        c_CacheObject.Type = GObjectType.Interactable;
                        c_CacheObject.Radius = c_diaObject.CollisionSphere.Radius;
                        return true;
                    }
                }
                catch (Exception)
                {
                    Logger.LogDebug("Error refreshing IsQuestGiver");
                }
            }

            if ((teamId == 1 || teamId == 2 || teamId == 17))
            {
                addToCache = false;
                c_InfosSubStep += "IsTeam" + teamId;
                return addToCache;
            }

            using (new MemorySpy("RefreshUnit().CheckMonsterType"))
            {
                /* Always refresh monster type */
                if (c_CacheObject.Type != GObjectType.Player && !c_CacheObject.IsBoss)
                {
                    if (c_unit_MonsterInfo == null)
                        c_unit_MonsterInfo = c_diaUnit.MonsterInfo;

                    if (c_unit_MonsterInfo != null)
                    {
                        switch (c_unit_MonsterInfo.MonsterType)
                        {
                            case MonsterType.Ally:
                            case MonsterType.Scenery:
                            case MonsterType.Helper:
                            case MonsterType.Team:
                                {
                                    c_CacheObject.IsAlly = true;

                                    addToCache = false;
                                    c_InfosSubStep += "AllySceneryHelperTeam";
                                    return addToCache;
                                }
                        }
                    }
                }
            }

            using (new MemorySpy("RefreshUnit().CheckFollower"))
            {
                if (DataDictionary.FollowerIds.Contains(c_CacheObject.ActorSNO))
                {
                    c_CacheObject.IsAlly = true;

                    addToCache = false;
                    c_InfosSubStep += "FollowerIds";
                    return addToCache;
                }
            }

            using (new MemorySpy("RefreshUnit().CheckGoblin"))
            {
                // Only set treasure goblins to true *IF* they haven't disabled goblins! Then check the SNO in the goblin hash list!
                c_unit_IsTreasureGoblin = false;
                // Flag this as a treasure goblin *OR* ignore this object altogether if treasure goblins are set to ignore
                if (DataDictionary.GoblinIds.Contains(c_CacheObject.ActorSNO) || c_CacheObject.InternalName.ToLower().StartsWith("treasuregoblin"))
                {
                    if (Settings.Combat.Misc.GoblinPriority != 0)
                    {
                        c_unit_IsTreasureGoblin = true;
                    }
                    else
                    {
                        addToCache = false;
                        c_InfosSubStep += "IgnoreTreasureGoblins";
                        return addToCache;
                    }
                }
            }
            using (new MemorySpy("RefreshUnit().CheckAffix"))
            {
                // Pull up the Monster Affix cached data
                RefreshAffixes();
                if (c_MonsterAffixes.HasFlag(MonsterAffixes.Shielding))
                    c_unit_HasShieldAffix = true;
            }

            using (new MemorySpy("RefreshUnit().CheckMonsterSize"))
            {
                // Only if at full health, else don't bother checking each loop
                // See if we already have this monster's size stored, if not get it and cache it
                RefreshMonsterSize();
            }

            using (new MemorySpy("RefreshUnit().CheckMonsterHealth"))
            {
                RefreshMonsterHealth();

                //DebugUtil.LogAnimation(c_CacheObject);

                // Unit is already dead
                if (c_HitPoints <= 0d && !c_CacheObject.IsBoss)
                {
                    addToCache = false;
                    c_InfosSubStep += "0HitPoints";
                    return addToCache;
                }

                if (c_diaUnit.IsDead)
                {
                    addToCache = false;
                    c_InfosSubStep += "IsDead";
                    return addToCache;
                }
            }

            if (c_CacheObject.IsQuestMonster || c_CacheObject.IsBountyObjective)
                return true;

            using (new MemorySpy("RefreshUnit().CheckAttributes"))
            {
                addToCache = RefreshUnitAttributes(addToCache, c_diaUnit);
            }

            if (!addToCache)
                return addToCache;

            // Set Kill range
            using (new MemorySpy("RefreshUnit().CheckKillRange"))
            {
                c_CacheObject.KillRange = SetKillRange();
            }

            if (c_CacheObject.RadiusDistance <= c_CacheObject.KillRange)
                AnyMobsInRange = true;

            return addToCache;
        }

        private static void CacheUnitNPCIsOperatable()
        {
            try
            {
                c_CacheObject.NPCIsOperable = (c_CommonData.GetAttribute<int>(ActorAttributeType.NPCIsOperatable) > 0);
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
            if (!CacheData.MonsterSizes.TryGetValue(c_CacheObject.RActorGuid, out c_unit_MonsterSize))
            {
                if (c_unit_MonsterInfo == null)
                    c_unit_MonsterInfo = c_diaUnit.MonsterInfo;

                c_unit_MonsterSize = c_unit_MonsterInfo != null ? c_unit_MonsterInfo.MonsterSize : MonsterSize.Unknown;
                CacheData.MonsterSizes.Add(c_CacheObject.RActorGuid, c_unit_MonsterSize);
            }
        }

        private static void RefreshMonsterHealth()
        {
            if (!c_diaUnit.IsValid)
                return;

            // health calculations
            double maxHealth;
            // Get the max health of this unit, a cached version if available, if not cache it
            if (!CacheData.UnitMaxHealth.TryGetValue(c_CacheObject.RActorGuid, out maxHealth))
            {
                maxHealth = c_diaUnit.HitpointsMax;
                CacheData.UnitMaxHealth.Add(c_CacheObject.RActorGuid, maxHealth);
            }

            // Health calculations            
            c_HitPoints = c_diaUnit.HitpointsCurrent;

            // And finally put the two together for a current health percentage
            c_HitPointsPct = c_HitPoints / maxHealth;
        }

        private static bool RefreshUnitAttributes(bool AddToCache = true, DiaUnit unit = null)
        {
            if (!DataDictionary.IgnoreUntargettableAttribute.Contains(c_CacheObject.ActorSNO) && unit.IsUntargetable)
            {
                AddToCache = false;
                c_InfosSubStep += "IsUntargetable";
                return AddToCache;
            }

            // don't check for invulnerability on shielded and boss units, they are treated seperately
            if (!c_unit_HasShieldAffix && unit.IsInvulnerable)
            {
                AddToCache = false;
                c_InfosSubStep += "IsInvulnerable";
                return AddToCache;
            }

            bool isBurrowed = false;
            if (!CacheData.UnitIsBurrowed.TryGetValue(c_CacheObject.RActorGuid, out isBurrowed))
            {
                isBurrowed = unit.IsBurrowed;
                CacheData.UnitIsBurrowed.Add(c_CacheObject.RActorGuid, isBurrowed);
            }

            if (isBurrowed)
            {
                AddToCache = false;
                c_InfosSubStep += "IsBurrowed";
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

            return AddToCache;

        }

        private static bool CacheObjectHasDOTDPS()
        {
            return c_CommonData.GetAttribute<int>(ActorAttributeType.DOTDPS) != 0;
        }
        private static double SetKillRange()
        {
            // Always within kill range if in the NoCheckKillRange list!
            if (DataDictionary.NoCheckKillRange.Contains(c_CacheObject.ActorSNO))
                return c_CacheObject.RadiusDistance + 100f;

            double killRange;

            // Bosses, always kill
            if (c_CacheObject.IsBoss)
            {
                return c_CacheObject.RadiusDistance + 100f;
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
            using (new MemorySpy("RefreshUnit().RefreshAffixes().Check"))
            {
                if (!CacheData.UnitMonsterAffix.TryGetValue(c_CacheObject.RActorGuid, out affixFlags))
                {
                    try
                    {
                        using (new MemorySpy("RefreshUnit().RefreshAffixes().Get"))
                        { affixFlags = c_CommonData.MonsterAffixes; }

                        CacheData.UnitMonsterAffix.Add(c_CacheObject.RActorGuid, affixFlags);
                    }
                    catch (Exception ex)
                    {
                        affixFlags = MonsterAffixes.None;
                        Logger.Log(TrinityLogLevel.Error, LogCategory.CacheManagement, "Handled Exception getting affixes for Monster SNO={0} Name={1} RAGuid={2}", c_CacheObject.ActorSNO, c_CacheObject.InternalName, c_CacheObject.RActorGuid);
                        Logger.Log(TrinityLogLevel.Error, LogCategory.CacheManagement, ex.ToString());
                    }
                }
            }

            using (new MemorySpy("RefreshUnit().RefreshAffixes().SetValue"))
            {
                c_unit_IsElite = affixFlags.HasFlag(MonsterAffixes.Elite);
                c_unit_IsRare = affixFlags.HasFlag(MonsterAffixes.Rare);
                c_unit_IsUnique = affixFlags.HasFlag(MonsterAffixes.Unique);
                c_unit_IsMinion = affixFlags.HasFlag(MonsterAffixes.Minion);

                // All-in-one flag for quicker if checks throughout
                c_IsEliteRareUnique = (c_unit_IsElite || c_unit_IsRare || c_unit_IsUnique || c_unit_IsMinion || DataDictionary.EliteRareUniqueIds.Contains(c_CacheObject.ActorSNO));
                c_MonsterAffixes = affixFlags;
            }
        }

        private static bool RefreshStepCachedSummons()
        {
            if (c_diaUnit.IsFullyValid())
            {
                int i_SummonedByACDId;
                if (!CacheData.SummonedByACDId.TryGetValue(c_CacheObject.RActorGuid, out i_SummonedByACDId))
                {
                    try
                    {
                        i_SummonedByACDId = c_diaUnit.SummonedByACDId;
                        CacheData.SummonedByACDId.Add(c_CacheObject.RActorGuid, i_SummonedByACDId);
                    }
                    catch {/* Continue */}
                }

                bool b_IsSummoner;
                if (!CacheData.IsSummoner.TryGetValue(c_CacheObject.RActorGuid, out b_IsSummoner))
                {
                    try
                    {
                        b_IsSummoner = c_diaUnit.SummonerId > 0;
                        CacheData.IsSummoner.Add(c_CacheObject.RActorGuid, b_IsSummoner);
                    }
                    catch {/* Continue */}
                }

                c_CacheObject.SummonedByACDId = i_SummonedByACDId;
                c_CacheObject.IsSummoner = b_IsSummoner;

                // SummonedByACDId is not ACDGuid, it's DynamicID
                if (c_CacheObject.SummonedByACDId == Player.MyDynamicID)
                {
                    c_CacheObject.IsSummonedByPlayer = true;
                }

                if (c_CacheObject.IsSummonedByPlayer)
                {
                    // Count up Mystic Allys, gargantuans, and zombies - if the player has those skills
                    if (Player.ActorClass == ActorClass.Monk)
                    {
                        if (DataDictionary.MysticAllyIds.Contains(c_CacheObject.ActorSNO))
                        {
                            PlayerOwnedMysticAllyCount++;
                            c_InfosSubStep += "IsPlayerSummoned";
                            return false;
                        }
                    }
                    // Count up Demon Hunter sentries
                    if (Player.ActorClass == ActorClass.DemonHunter)
                    {
                        if (DataDictionary.DemonHunterSentryIds.Contains(c_CacheObject.ActorSNO))
                        {
                            if (c_CacheObject.Distance < 75f)
                            {
                                PlayerOwnedDHSentryCount++;
                            }

                            CacheData.SentryTurret.Add(new CacheObstacleObject()
                            {
                                ActorSNO = c_CacheObject.ActorSNO,
                                RActorGUID = c_CacheObject.RActorGuid,
                                Position = c_CacheObject.Position,
                                Radius = c_CacheObject.Radius,
                            });

                            c_InfosSubStep += "IsPlayerSummoned";
                            return false;
                        }
                        if (DataDictionary.DemonHunterPetIds.Contains(c_CacheObject.ActorSNO))
                        {
                            PlayerOwnedDHPetsCount++;
                            c_InfosSubStep += "IsPlayerSummoned";
                            return false;
                        }
                    }
                    // Count up Wiz hydras
                    if (Player.ActorClass == ActorClass.Wizard)
                    {
                        if (DataDictionary.WizardHydraIds.Contains(c_CacheObject.ActorSNO))
                        {
                            PlayerOwnedHydraCount++;
                            c_InfosSubStep += "IsPlayerSummoned";
                            return false;
                        }
                    }
                    // Count up zombie dogs and gargantuans next
                    if (Player.ActorClass == ActorClass.Witchdoctor)
                    {
                        if (DataDictionary.BigBadVoodooIds.Contains(c_CacheObject.ActorSNO))
                        {
                            CacheData.Voodoo.Add(new CacheObstacleObject()
                            {
                                ActorSNO = c_CacheObject.ActorSNO,
                                RActorGUID = c_CacheObject.RActorGuid,
                                Position = c_CacheObject.Position,
                                Radius = c_CacheObject.Radius,
                            });

                            c_InfosSubStep += "IsPlayerSummoned";
                            return false;
                        }
                        if (DataDictionary.ZombieDogIds.Contains(c_CacheObject.ActorSNO))
                        {
                            PlayerOwnedZombieDogCount++;
                            c_InfosSubStep += "IsPlayerSummoned";
                            return false;
                        }
                        if (DataDictionary.GargantuanIds.Contains(c_CacheObject.ActorSNO))
                        {
                            PlayerOwnedGargantuanCount++;
                            c_InfosSubStep += "IsPlayerSummoned";
                            return false;
                        }
                        if (DataDictionary.FetishArmyIds.Contains(c_CacheObject.ActorSNO))
                        {
                            Trinity.PlayerOwnedFetishCount++;
                            c_InfosSubStep += "IsPlayerSummoned";
                            return false;
                        }
                    }

                    c_InfosSubStep += "IsPlayerSummoned";
                    return false;
                }
            }

            return true;
        }
    }
}
