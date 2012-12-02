using GilesTrinity.DbProvider;
using GilesTrinity.Technicals;
using System;
using System.IO;
using System.Linq;
using Zeta;
using Zeta.Common;
using Zeta.Common.Plugins;
using Zeta.CommonBot;
using Zeta.Internals.Actors;
using Zeta.Internals.Actors.Gizmos;
using Zeta.Internals.SNO;
namespace GilesTrinity
{
    public partial class GilesTrinity : IPlugin
    {
        private static bool CacheDiaObject(DiaObject freshObject)
        {
            /*
             *  Initialize Variables
             */
            bool AddToCache;

            RefreshStepInit(out AddToCache);
            /*
             *  Get primary reference objects and keys
             */
            c_diaObject = freshObject;

            /*
             * Set Common Data
             */
            AddToCache = RefreshStepGetCommonData(freshObject);
            //if (!AddToCache) { c_IgnoreReason = "GetCommonData"; return AddToCache; }

            // Ractor GUID
            c_RActorGuid = freshObject.RActorGuid;
            // Check to see if we've already looked at this GUID

            AddToCache = RefreshStepSkipDoubleCheckGuid(AddToCache);
            if (!AddToCache) { c_IgnoreReason = "SkipDoubleCheckGuid"; return AddToCache; }
            // ActorSNO
            AddToCache = RefreshStepCachedActorSNO(AddToCache);

            // Have ActorSNO Check for SNO based navigation obstacle hashlist
            c_IsObstacle = hashSNONavigationObstacles.Contains(c_ActorSNO);

            if (!AddToCache) { c_IgnoreReason = "CachedActorSNO"; return AddToCache; }
            // Get Internal Name
            AddToCache = RefreshInternalName(AddToCache);
            if (!AddToCache) { c_IgnoreReason = "InternalName"; return AddToCache; }
            // Get ACDGuid
            AddToCache = RefreshStepCachedACDGuid(AddToCache);
            if (!AddToCache) { c_IgnoreReason = "CachedACDGuid"; return AddToCache; }


            /*
             * Begin main refresh routine
             */
            // Set Giles Object Type
            AddToCache = RefreshStepCachedObjectType(AddToCache);
            if (!AddToCache) { c_IgnoreReason = "CachedObjectType"; return AddToCache; }

            // Check Blacklists
            AddToCache = RefreshStepCheckBlacklists(AddToCache);
            if (!AddToCache) { c_IgnoreReason = "CheckBlacklists"; return AddToCache; }

            // Get Cached Position
            AddToCache = RefreshStepCachedPosition(AddToCache);
            if (!AddToCache) { c_IgnoreReason = "CachedPosition"; return AddToCache; }

            // Always Refresh Distance for every object
            RefreshStepCalculateDistance();

            // Always Refresh ZDiff for every object
            AddToCache = RefreshStepNewObjectTypeZDiff(AddToCache);
            if (!AddToCache) { c_IgnoreReason = "ZDiff"; return AddToCache; }

            // Add new Obstacle to cache
            AddToCache = RefreshStepAddObstacleToCache(AddToCache);
            if (!AddToCache) { c_IgnoreReason = "AddObstacleToCache"; return AddToCache; }

            // Get DynamicId and GameBalanceId
            AddToCache = RefreshStepCachedDynamicIds(AddToCache);
            if (!AddToCache) { c_IgnoreReason = "CachedDynamicIds"; return AddToCache; }

            // Summons by the player 
            AddToCache = RefreshStepCachedPlayerSummons(AddToCache);
            if (!AddToCache) { c_IgnoreReason = "CachedPlayerSummons"; return AddToCache; }

            /* 
             * Main Switch on Object Type - Refresh individual object types (Units, Items, Gizmos)
             */
            RefreshStepMainObjectType(ref AddToCache);
            if (!AddToCache) { c_IgnoreReason = "MainObjectType"; return AddToCache; }

            // Ignore all LoS
            AddToCache = RefreshStepIgnoreLoS(AddToCache);
            if (!AddToCache) { c_IgnoreReason = "IgnoreLoS"; return AddToCache; }
            // Ignore anything unknown
            AddToCache = RefreshStepIgnoreUnknown(AddToCache);
            if (!AddToCache) { c_IgnoreReason = "IgnoreUnknown"; return AddToCache; }
            // Double Check Blacklists
            AddToCache = RefreshStepCheckBlacklists(AddToCache);
            if (!AddToCache) { c_IgnoreReason = "DoubleCheckBlacklists"; return AddToCache; }
            // If it's a unit, add it to the monster cache
            AddUnitToMonsterObstacleCache(AddToCache);
            if (AddToCache)
            {
                c_IgnoreReason = "None";
                GilesObjectCache.Add(
                    new GilesObject(c_diaObject)
                    {
                        Position = c_Position,
                        Type = c_ObjectType,
                        Weight = c_Weight,
                        CentreDistance = c_CentreDistance,
                        RadiusDistance = c_RadiusDistance,
                        InternalName = c_Name,
                        ACDGuid = c_ACDGUID,
                        RActorGuid = c_RActorGuid,
                        DynamicID = c_GameDynamicID,
                        BalanceID = c_BalanceID,
                        ActorSNO = c_ActorSNO,
                        Level = c_ItemLevel,
                        GoldAmount = c_GoldStackSize,
                        OneHanded = c_IsOneHandedItem,
                        ItemQuality = c_ItemQuality,
                        DBItemType = c_DBItemType,
                        FollowerType = c_item_tFollowerType,
                        GilesItemType = c_item_GItemType,
                        IsElite = c_unit_IsElite,
                        IsRare = c_unit_IsRare,
                        IsUnique = c_unit_IsUnique,
                        IsMinion = c_unit_IsMinion,
                        IsTreasureGoblin = c_unit_IsTreasureGoblin,
                        IsBoss = c_unit_IsBoss,
                        IsAttackable = c_unit_IsAttackable,
                        HitPoints = c_HitPoints,
                        Radius = c_Radius,
                        MonsterStyle = c_unit_MonsterSize,
                        IsEliteRareUnique = c_IsEliteRareUnique,
                        ForceLeapAgainst = c_ForceLeapAgainst
                    });
            }
            return true;
        }
        /// <summary>
        /// Adds a unit to cache hashMonsterObstacleCache
        /// </summary>
        /// <param name="AddToCache"></param>
        private static void AddUnitToMonsterObstacleCache(bool AddToCache)
        {
            if (AddToCache && c_ObjectType == GObjectType.Unit)
            {
                // Handle bosses
                if (c_unit_IsBoss)
                {
                    // Force to elite and add to the collision list
                    c_unit_IsElite = true;
                    hashMonsterObstacleCache.Add(new GilesObstacle(c_Position, (c_Radius * 0.15f), c_ActorSNO));
                }
                else
                {
                    // Add to the collision-list
                    hashMonsterObstacleCache.Add(new GilesObstacle(c_Position, (c_Radius * 0.10f), c_ActorSNO));
                }
            }
        }
        /// <summary>
        /// Initializes variable set for single object refresh
        /// </summary>
        /// <param name="AddTocache"></param>
        /// <param name="iPercentage"></param>
        private static void RefreshStepInit(out bool AddTocache)
        {
            AddTocache = true;
            // Start this object as off as unknown type
            c_ObjectType = GObjectType.Unknown;
            // We will set weight up later in RefreshDiaObjects after we process all valid items
            c_Weight = 0;
            c_Position = Vector3.Zero;
            c_ObjectType = GObjectType.Unknown;
            c_Weight = 0d;
            c_CentreDistance = 0f;
            c_RadiusDistance = 0f;
            c_Radius = 0f;
            c_ZDiff = 0f;
            c_Name = "";
            c_IgnoreReason = "";
            c_IgnoreSubStep = "";
            c_ACDGUID = -1;
            c_RActorGuid = -1;
            c_GameDynamicID = -1;
            c_BalanceID = -1;
            c_ActorSNO = -1;
            c_ItemLevel = -1;
            c_GoldStackSize = -1;
            c_HitPoints = -1;
            c_IsOneHandedItem = false;
            c_unit_IsElite = false;
            c_unit_IsRare = false;
            c_unit_IsUnique = false;
            c_unit_IsMinion = false;
            c_unit_IsTreasureGoblin = false;
            c_unit_IsBoss = false;
            c_unit_IsAttackable = false;
            c_IsEliteRareUnique = false;
            c_ForceLeapAgainst = false;
            c_IsObstacle = false;
            c_ItemQuality = ItemQuality.Invalid;
            c_DBItemType = ItemType.Unknown;
            c_item_tFollowerType = FollowerType.None;
            c_item_GItemType = GItemType.Unknown;
            c_unit_MonsterSize = MonsterSize.Unknown;
            c_diaObject = null;
            c_CurrentAnimation = SNOAnim.Invalid;
        }
        /// <summary>
        /// Inserts the ActorSNO <see cref="dictGilesActorSNOCache"/> and sets <see cref="c_ActorSNO"/>
        /// </summary>
        /// <param name="AddToCache"></param>
        /// <param name="c_diaObject"></param>
        /// <returns></returns>
        private static bool RefreshStepCachedActorSNO(bool AddToCache)
        {
            // Get the Actor SNO, cached if possible
            if (!dictGilesActorSNOCache.TryGetValue(c_RActorGuid, out c_ActorSNO))
            {
                try
                {
                    c_ActorSNO = c_diaObject.ActorSNO;
                }
                catch (Exception ex)
                {
                    DbHelper.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "Safely handled exception getting ActorSNO for an object.");
                    DbHelper.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "{0}", ex);
                    AddToCache = false;
                }
                dictGilesActorSNOCache.Add(c_RActorGuid, c_ActorSNO);
            }
            return AddToCache;
        }
        private static bool RefreshInternalName(bool AddToCache)
        {
            // This is "internalname" for items, and just a "generic" name for objects and units - cached if possible
            if (!dictGilesInternalNameCache.TryGetValue(c_RActorGuid, out c_Name))
            {
                try
                {
                    c_Name = nameNumberTrimRegex.Replace(c_diaObject.Name, "");
                }
                catch (Exception ex)
                {
                    DbHelper.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "Safely handled exception getting InternalName for an object [{0}]", c_ActorSNO);
                    DbHelper.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "{0}", ex);
                    AddToCache = false;
                }
                dictGilesInternalNameCache.Add(c_RActorGuid, c_Name);
            }
            return AddToCache;
        }
        private static bool RefreshStepGetCommonData(DiaObject thisobj)
        {
            c_CommonData = thisobj.CommonData;
            if (c_CommonData == null)
            {
                return false;
            }
            else if (!c_CommonData.IsValid)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        private static bool RefreshStepIgnoreNullCommonData(bool AddToCache)
        {
            // Null Common Data makes a DiaUseless!
            if (c_ObjectType == GObjectType.Unit || c_ObjectType == GObjectType.Item || c_ObjectType == GObjectType.Gold)
            {
                if (c_CommonData == null)
                {
                    AddToCache = false;
                }
                if (c_CommonData != null && !c_CommonData.IsValid)
                {
                    AddToCache = false;
                }
            }
            return AddToCache;
        }
        private static bool RefreshStepAddObstacleToCache(bool AddToCache)
        {
            // Have Position, Now store the location etc. of this obstacle and continue
            if (c_IsObstacle)
            {
                hashNavigationObstacleCache.Add(new GilesObstacle(c_Position, dictSNONavigationSize[c_ActorSNO], c_ActorSNO));
                AddToCache = false;
            }
            return AddToCache;
        }
        private static void RefreshStepCalculateDistance()
        {
            // Calculate distance, don't rely on DB's internal method as this may hit Diablo 3 memory again
            c_CentreDistance = Vector3.Distance(playerStatus.CurrentPosition, c_Position);
            // Set radius-distance to centre distance at first
            c_RadiusDistance = c_CentreDistance;
        }
        private static bool RefreshStepSkipDoubleCheckGuid(bool AddToCache)
        {
            // See if we've already checked this ractor, this loop
            if (hashDoneThisRactor.Contains(c_RActorGuid))
            {
                AddToCache = false;
                //return bWantThis;
            }
            else
            {
                hashDoneThisRactor.Add(c_RActorGuid);
            }
            return AddToCache;
        }
        private static bool RefreshStepCachedObjectType(bool AddToCache)
        {
            // Set the object type
            // begin with default... 
            c_ObjectType = GObjectType.Unknown;
            // Either get the cached Giles object type, or calculate it fresh
            if (!c_IsObstacle && !dictGilesObjectTypeCache.TryGetValue(c_RActorGuid, out c_ObjectType))
            {
                // See if it's an avoidance first from the SNO
                if (hashAvoidanceSNOList.Contains(c_ActorSNO) || hashAvoidanceBuffSNOList.Contains(c_ActorSNO) || hashAvoidanceSNOProjectiles.Contains(c_ActorSNO))
                {
                    // If avoidance is disabled, ignore this avoidance stuff
                    if (!Settings.Combat.Misc.AvoidAOE)
                    {
                        AddToCache = false;
                        c_IgnoreSubStep = "AvoidanceDisabled";
                    }
                    else
                    {
                        // Avoidance isn't disabled, so set this object type to avoidance
                        c_ObjectType = GObjectType.Avoidance;
                    }

                    // Checking for BuffVisualEffect - for Butcher, maybe useful other places?
                    if (hashAvoidanceBuffSNOList.Contains(c_ActorSNO) && Settings.Combat.Misc.AvoidAOE)
                    {
                        bool hasBuff = false;
                        try
                        {
                            hasBuff = c_CommonData.GetAttribute<int>(ActorAttributeType.BuffVisualEffect) > 0;
                        }
                        catch
                        {
                            // Remove on exception, otherwise it may get stuck in the cache
                            dictGilesObjectTypeCache.Remove(c_RActorGuid);
                        }
                        if (hasBuff)
                        {
                            AddToCache = true;
                            c_ObjectType = GObjectType.Avoidance;
                        }
                        else
                        {
                            dictGilesObjectTypeCache.Remove(c_RActorGuid);
                            AddToCache = false;
                            c_IgnoreSubStep = "NoBuffVisualEffect";
                        }
                    }

                }
                // It's not an avoidance, so let's calculate it's object type "properly"
                else
                {
                    // Calculate the object type of this object
                    //if (thisobj is DiaUnit && thisobj.ActorType == ActorType.Unit)                    
                    if (c_diaObject.ActorType == ActorType.Unit)
                    {
                        if (c_CommonData == null)
                        {
                            AddToCache = false;
                        }
                        else if (c_diaObject.ACDGuid != c_CommonData.ACDGuid)
                        {
                            AddToCache = false;
                        }
                        else
                        {
                            c_ObjectType = GObjectType.Unit;
                        }
                    }
                    else if (hashForceSNOToItemList.Contains(c_ActorSNO) || c_diaObject.ActorType == ActorType.Item)
                    {
                        if (c_CommonData == null)
                        {
                            AddToCache = false;
                        }
                        if (c_CommonData != null && c_diaObject.ACDGuid != c_CommonData.ACDGuid)
                        {
                            AddToCache = false;
                        }
                        if (c_Name.ToLower().StartsWith("gold"))
                        {
                            c_ObjectType = GObjectType.Gold;
                        }
                        else
                        {
                            c_ObjectType = GObjectType.Item;
                        }
                    }
                    else if (c_diaObject.ActorType == ActorType.Gizmo)
                    {
                        if (c_diaObject.ActorInfo.GizmoType == GizmoType.Shrine)
                            c_ObjectType = GObjectType.Shrine;
                        else if (c_diaObject.ActorInfo.GizmoType == GizmoType.Healthwell)
                            c_ObjectType = GObjectType.HealthWell;
                        else if (c_diaObject.ActorInfo.GizmoType == GizmoType.DestructibleLootContainer)
                            c_ObjectType = GObjectType.Destructible;
                        else if (c_diaObject.ActorInfo.GizmoType == GizmoType.Destructible)
                            c_ObjectType = GObjectType.Destructible;
                        else if (c_diaObject.ActorInfo.GizmoType == GizmoType.Barricade)
                            c_ObjectType = GObjectType.Barricade;
                        else if (c_diaObject.ActorInfo.GizmoType == GizmoType.LootContainer)
                            c_ObjectType = GObjectType.Container;
                        else if (hashSNOInteractWhitelist.Contains(c_ActorSNO))
                            c_ObjectType = GObjectType.Interactable;
                        else if (c_diaObject.ActorInfo.GizmoType == GizmoType.Door)
                            c_ObjectType = GObjectType.Door;
                        else if (c_diaObject.ActorInfo.GizmoType == GizmoType.WeirdGroup57)
                            c_ObjectType = GObjectType.Interactable;
                        else
                            c_ObjectType = GObjectType.Unknown;
                    }
                    else
                        c_ObjectType = GObjectType.Unknown;
                }
                // Now cache the object type
                dictGilesObjectTypeCache.Add(c_RActorGuid, c_ObjectType);
            }
            return AddToCache;
        }
        private static void RefreshStepMainObjectType(ref bool AddToCache)
        {
            // Now do stuff specific to object types
            switch (c_ObjectType)
            {
                // Handle Unit-type Objects
                case GObjectType.Unit:
                    {
                        AddToCache = RefreshGilesUnit(AddToCache);
                        break;
                    }
                // Handle Item-type Objects
                case GObjectType.Item:
                    {
                        if (!ForceVendorRunASAP)
                        {
                            AddToCache = RefreshGilesItem(AddToCache);
                            c_IgnoreReason = "RefreshGilesItem";
                        }
                        else
                        {
                            AddToCache = false;
                            c_IgnoreSubStep = "bGilesForcedVendoring";
                        }
                        break;

                    }
                // Handle Gold
                // NOTE: Only identified as gold after *FIRST* loop as an "item" by above code
                case GObjectType.Gold:
                    {
                        AddToCache = RefreshGilesGold(AddToCache);
                        c_IgnoreSubStep = "RefreshGilesGold";
                        break;
                    }
                // Handle Globes
                // NOTE: Only identified as globe after *FIRST* loop as an "item" by above code
                case GObjectType.Globe:
                    {
                        // Ignore it if it's not in range yet
                        if (c_CentreDistance > iCurrentMaxLootRadius || c_CentreDistance > 37f)
                        {
                            c_IgnoreSubStep = "GlobeOutOfRange";
                            AddToCache = false;
                        }
                        AddToCache = true;
                        break;
                    }
                // Handle Avoidance Objects
                case GObjectType.Avoidance:
                    {
                        AddToCache = RefreshGilesAvoidance(AddToCache);
                        if (!AddToCache) { c_IgnoreSubStep = "RefreshGilesAvoidance"; }

                        break;
                    }
                // Handle Other-type Objects
                case GObjectType.Destructible:
                case GObjectType.Door:
                case GObjectType.Barricade:
                case GObjectType.Container:
                case GObjectType.Shrine:
                case GObjectType.Interactable:
                case GObjectType.HealthWell:
                    {
                        AddToCache = RefreshGilesGizmo(AddToCache);
                        break;
                    }
                // Object switch on type (to seperate shrines, destructibles, barricades etc.)
                case GObjectType.Unknown:
                default:
                    {
                        break;
                    }
            }
        }
        private static bool RefreshGilesUnit(bool AddToCache)
        {
            DiaUnit thisUnit = (DiaUnit)c_diaObject;
            AddToCache = true;
            // Store the dia unit reference (we'll be able to remove this if we update ractor list every single loop, yay!)
            c_diaObject = null;
            // See if this is a boss
            c_unit_IsBoss = hashBossSNO.Contains(c_ActorSNO);

            try
            {
                c_CurrentAnimation = thisUnit.CommonData.CurrentAnimation;
            }
            catch { }

            // hax for Diablo_shadowClone
            c_unit_IsAttackable = c_Name.StartsWith("Diablo_shadowClone");
            try
            {
                c_diaObject = (DiaUnit)thisUnit;
                // Prepare the fake object for target handler
                if (FakeObject == null)
                    FakeObject = thisUnit;
            }
            catch
            {
                AddToCache = false;
                c_IgnoreSubStep = "NotAUnit";
            }
            if (c_CommonData.ACDGuid == -1)
            {
                AddToCache = false;
            }
            // Dictionary based caching of monster types based on the SNO codes
            MonsterType monsterType;
            // See if we need to refresh the monster type or not
            bool bAddToDictionary = !dictionaryStoredMonsterTypes.TryGetValue(c_ActorSNO, out monsterType);
            bool bRefreshMonsterType = bAddToDictionary;
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
            // Now see if we do need to get new data for this boss or not
            if (bRefreshMonsterType)
            {
                try
                {
                    monsterType = RefreshMonsterType(c_CommonData, monsterType, bAddToDictionary);
                }
                catch (Exception ex)
                {
                    DbHelper.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "Safely handled exception getting monsterinfo and monstertype for unit {0} [{1}]", c_Name, c_ActorSNO);
                    DbHelper.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "{0}", ex);
                    DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.CacheManagement, "ActorTypeAttempt={0}", thisUnit.ActorType);
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
                        //return bWantThis;
                    }
                    break;
            }
            // health calculations
            double dThisMaxHealth;
            // Get the max health of this unit, a cached version if available, if not cache it
            if (!dictGilesMaxHealthCache.TryGetValue(c_RActorGuid, out dThisMaxHealth))
            {
                try
                {
                    dThisMaxHealth = c_CommonData.GetAttribute<float>(ActorAttributeType.HitpointsMax);
                }
                catch (Exception ex)
                {
                    DbHelper.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "Safely handled exception getting attribute max health for unit {0} [{1}]", c_Name, c_ActorSNO);
                    DbHelper.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "{0}", ex);
                    AddToCache = false;
                    //return bWantThis;
                }
                dictGilesMaxHealthCache.Add(c_RActorGuid, dThisMaxHealth);
            }
            // Now try to get the current health - using temporary and intelligent caching
            // Health calculations
            int iLastCheckedHealth;
            double dThisCurrentHealth = 0d;
            bool bHasCachedHealth;
            // See if we already have a cached value for health or not for this monster
            if (dictGilesLastHealthChecked.TryGetValue(c_RActorGuid, out iLastCheckedHealth))
            {
                bHasCachedHealth = true;
                iLastCheckedHealth++;
                if (iLastCheckedHealth > 6)
                    iLastCheckedHealth = 1;
                if (CurrentTargetRactorGUID == c_RActorGuid && iLastCheckedHealth > 3)
                    iLastCheckedHealth = 1;
            }
            else
            {
                bHasCachedHealth = false;
                iLastCheckedHealth = 1;
            }
            // Update health once every 5 cycles, except for current target, which is every cycle
            if (iLastCheckedHealth == 1)
            {
                try
                {
                    dThisCurrentHealth = c_CommonData.GetAttribute<float>(ActorAttributeType.HitpointsCur);
                }
                catch
                {
                    // This happens so frequently in DB/D3 that this fails, let's not even bother logging it anymore
                    //Logging.WriteDiagnostic("[Trinity] Safely handled exception getting current health for unit " + tmp_sThisInternalName + " [" + tmp_iThisActorSNO.ToString() + "]");
                    // Add this monster to our very short-term ignore list
                    if (!c_unit_IsBoss)
                    {
                        hashRGUIDBlacklist3.Add(c_RActorGuid);
                        dateSinceBlacklist3Clear = DateTime.Now;
                        NeedToClearBlacklist3 = true;
                    }
                    AddToCache = false;
                }
                RefreshCachedHealth(iLastCheckedHealth, dThisCurrentHealth, bHasCachedHealth);
            }
            else
            {
                dThisCurrentHealth = dictGilesLastHealthCache[c_RActorGuid];
                dictGilesLastHealthChecked[c_RActorGuid] = iLastCheckedHealth;
            }
            // And finally put the two together for a current health percentage
            c_HitPoints = dThisCurrentHealth / dThisMaxHealth;
            // Unit is already dead
            if (c_HitPoints <= 0d && !c_unit_IsBoss)
            {
                // Add this monster to our very short-term ignore list
                hashRGUIDBlacklist3.Add(c_RActorGuid);
                dateSinceBlacklist3Clear = DateTime.Now;
                NeedToClearBlacklist3 = true;
                AddToCache = false;
                c_IgnoreSubStep = "0HitPoints";
            }
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
                }
            }
            // Pull up the Monster Affix cached data
            MonsterAffixes theseaffixes = RefreshAffixes(c_CommonData);
            //intell -- Other dangerous: Nightmarish, Mortar, Desecrator, Fire Chains, Knockback, Electrified
            /*
             * 
             * This should be moved to HandleTarget
             * 
             */
            if (GilesUseTimer(SNOPower.Barbarian_WrathOfTheBerserker, true))
            {
                //WotB only used on Arcane, Frozen, Jailer, Molten and Electrified+Reflect Damage elites
                if (theseaffixes.HasFlag(MonsterAffixes.ArcaneEnchanted) || theseaffixes.HasFlag(MonsterAffixes.Frozen) ||
                    theseaffixes.HasFlag(MonsterAffixes.Jailer) || theseaffixes.HasFlag(MonsterAffixes.Molten) ||
                   (theseaffixes.HasFlag(MonsterAffixes.Electrified) && theseaffixes.HasFlag(MonsterAffixes.ReflectsDamage)) ||
                    //Bosses and uber elites
                    c_unit_IsBoss || c_ActorSNO == 256015 || c_ActorSNO == 256000 || c_ActorSNO == 255996 ||
                    //...or more than 4 elite mobs in range (only elites/rares/uniques, not minions!)
                    iElitesWithinRange[RANGE_50] > 4)
                    bUseBerserker = true;
            }
            else
                bUseBerserker = false;
            if (theseaffixes.HasFlag(MonsterAffixes.Waller))
                bCheckGround = true;
            else
                bCheckGround = false;
            // Is this something we should try to force leap/other movement abilities against?
            c_ForceLeapAgainst = false;
            double dUseKillRadius = RefreshKillRadius();
            // Now ignore any unit not within our kill or extended kill radius
            if (c_RadiusDistance > dUseKillRadius)
            {
                AddToCache = false;
                c_IgnoreSubStep = "OutsideofKillRadius";
            }
			try {
				if (thisUnit.IsUntargetable)
				{
					AddToCache = false;
					c_IgnoreSubStep += "Untargettable+";
				}
				// Disabled because of chickens
				// if (thisUnit.IsHidden)
				// {
				//    AddToCache = false;
				//    c_IgnoreSubStep += "IsHidden+";
				// }
				if (thisUnit.IsInvulnerable)
				{
					AddToCache = false;
					c_IgnoreSubStep += "IsInvulnerable+";
				}
				if (thisUnit.IsBurrowed)
				{
					AddToCache = false;
					c_IgnoreSubStep += "IsBurrowed+";
				}
				if (thisUnit.IsHelper || thisUnit.IsNPC || thisUnit.IsTownVendor)
				{
					AddToCache = false;
					c_IgnoreSubStep += "IsNPCOrHelper+";
				}
			} catch {}
            // Safe is-attackable detection
            if (AddToCache)
                c_unit_IsAttackable = true;
            else
                c_unit_IsAttackable = false;
            if (c_unit_IsBoss || theseaffixes.HasFlag(MonsterAffixes.Shielding))
            {
                try
                {

                    c_unit_IsAttackable = !thisUnit.IsInvulnerable;
                }
                catch (Exception ex)
                {
                    DbHelper.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "Safely handled exception getting is-invulnerable attribute for unit {0} [{1}]", c_Name, c_ActorSNO);
                    DbHelper.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "{0}", ex);
                    c_unit_IsAttackable = true;
                }
            }


            // rrrix disabled this because it can change at ANY TIME and caching is a bad idea for this!
            // Inactive units like trees, withermoths etc. still underground
            //if (c_HitPoints >= 1f || c_unit_bIsBoss)
            //{
            //    // Get the burrowing data for this unit
            //    bool bBurrowed;
            //    if (!dictGilesBurrowedCache.TryGetValue(c_RActorGuid, out bBurrowed) || c_unit_bIsBoss)
            //    {
            //        try
            //        {
            //            bBurrowed = thisUnit.IsBurrowed;
            //        }
            //        catch (Exception ex)
            //        {
            //            Logging.WriteDiagnostic("[Trinity] Safely handled exception getting is-untargetable or is-burrowed attribute for unit " + c_Name + " [" + c_ActorSNO.ToString() + "]");
            //            Logging.WriteDiagnostic(ex.ToString());
            //            bWantThis = false;
            //            //return bWantThis;
            //        }
            //        // Only cache it if it's NOT burrowed (if it *IS* - then we need to keep re-checking until it comes out!)
            //        if (!bBurrowed)
            //        {
            //            // Don't cache for bosses, as we have to check for bosses popping in and out of the game during a complex fight
            //            if (!c_unit_bIsBoss)
            //                dictGilesBurrowedCache.Add(c_RActorGuid, bBurrowed);
            //        }
            //        else
            //        {
            //            // Unit is burrowed, so we need to ignore it until it isn't!
            //            c_IgnoreSubStep = "Burrowed";
            //            bWantThis = false;
            //            //return bWantThis;
            //        }
            //    }
            //}

            // Only if at full health, else don't bother checking each loop
            // See if we already have this monster's size stored, if not get it and cache it
            if (!dictionaryStoredMonsterSizes.TryGetValue(c_ActorSNO, out c_unit_MonsterSize))
            {
                try
                {
                    SNORecordMonster monsterInfo = thisUnit.MonsterInfo;
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
                catch (Exception ex)
                {
                    DbHelper.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "Safely handled exception getting monstersize info for unit {0} [{1}]", c_Name, c_ActorSNO);
                    DbHelper.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "{0}", ex);
                    AddToCache = false;
                    //return bWantThis;
                }
            }
            // Retrieve collision sphere radius, cached if possible
            if (!dictGilesCollisionSphereCache.TryGetValue(c_ActorSNO, out c_Radius))
            {
                try
                {
                    c_Radius = thisUnit.CollisionSphere.Radius;
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
                catch (Exception ex)
                {
                    DbHelper.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "Safely handled exception getting collisionsphere radius for unit {0} [{1}]", c_Name, c_ActorSNO);
                    DbHelper.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "{0}", ex);
                    AddToCache = false;
                    //return bWantThis;
                }
                dictGilesCollisionSphereCache.Add(c_ActorSNO, c_Radius);
            }
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
            if ((Settings.Combat.Misc.ExtendedTrashKill && iKeepKillRadiusExtendedFor > 0) || ForceVendorRunASAP)
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
        private static bool RefreshGilesItem(bool AddToCache)
        {
            AddToCache = false;
            if (c_BalanceID == -1)
            {
                AddToCache = false;
                c_IgnoreSubStep = "InvalidBalanceID";
            }
            // Try and pull up cached item data on this item, if not, add to our local memory cache
            GilesGameBalanceDataCache balanceCachEntry;
            if (!dictGilesGameBalanceDataCache.TryGetValue(c_BalanceID, out balanceCachEntry))
            {
                DiaItem item = c_diaObject as DiaItem;
                if (item != null)
                {
                    try
                    {
                        c_ItemLevel = item.CommonData.Level;
                        c_DBItemType = item.CommonData.ItemType;
                        c_IsOneHandedItem = item.CommonData.IsOneHand;
                        c_item_tFollowerType = item.CommonData.FollowerSpecialType;

                        // Add to session cache
                        dictGilesGameBalanceDataCache.Add(c_BalanceID, new GilesGameBalanceDataCache(c_ItemLevel, c_DBItemType, c_IsOneHandedItem, c_item_tFollowerType));
                    }
                    catch (Exception ex)
                    {
                        DbHelper.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement,
                            "Safely handled exception getting un-cached ACD Item data (level/item type etc.) for item {0} [{1}]", c_Name, c_ActorSNO);
                        DbHelper.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "{0}", ex);
                        AddToCache = false;
                        c_IgnoreSubStep = "CommonDataException";
                    }
                }
                else
                {
                    // Couldn't get the game balance data for this item, so ignore it for now
                    AddToCache = false;
                    c_IgnoreSubStep = "NoBalanceData";
                }
            }
            else
            {
                // We pulled this data from the dictionary cache, so use it instead of trying to get new data from DB/D3 memory!
                c_ItemLevel = balanceCachEntry.ItemLevel;
                c_DBItemType = balanceCachEntry.ItemType;
                c_IsOneHandedItem = balanceCachEntry.OneHand;
                c_item_tFollowerType = balanceCachEntry.FollowerType;
            }

            // Calculate custom Giles item type
            c_item_GItemType = DetermineItemType(c_Name, c_DBItemType, c_item_tFollowerType);
            // And temporarily store the base type
            GItemBaseType itemBaseType = DetermineBaseType(c_item_GItemType);
            // Treat all globes as a yes
            if (c_item_GItemType == GItemType.HealthGlobe)
            {
                c_ObjectType = GObjectType.Globe;
                // Create or alter this cached object type
                GObjectType objectType;
                if (!dictGilesObjectTypeCache.TryGetValue(c_RActorGuid, out objectType))
                    dictGilesObjectTypeCache.Add(c_RActorGuid, c_ObjectType);
                else
                    dictGilesObjectTypeCache[c_RActorGuid] = c_ObjectType;
                AddToCache = true;
            }

            // Quality of item for "genuine" items
            c_ItemQuality = ItemQuality.Invalid;
            if (itemBaseType != GItemBaseType.Unknown && itemBaseType != GItemBaseType.HealthGlobe && itemBaseType != GItemBaseType.Gem && itemBaseType != GItemBaseType.Misc &&
                !hashForceSNOToItemList.Contains(c_ActorSNO))
            {
                // Get the quality of this item, cached if possible
                if (!dictGilesQualityCache.TryGetValue(c_RActorGuid, out c_ItemQuality))
                {
                    try
                    {
                        c_ItemQuality = (ItemQuality)c_CommonData.GetAttribute<int>(ActorAttributeType.ItemQualityLevel);
                    }
                    catch
                    {
                        DbHelper.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "Safely handled exception getting item-quality for item {0} [{1}]", c_Name, c_ActorSNO);
                        AddToCache = false;
                        c_IgnoreSubStep = "ItemQualityLevelException";
                    }
                    dictGilesQualityCache.Add(c_RActorGuid, c_ItemQuality);
                    dictGilesQualityRechecked.Add(c_RActorGuid, false);
                }
                else
                {
                    // Because item-quality is such a sensitive thing, we don't want to risk losing items
                    // So we check a cached item quality a 2nd time - as long as it's the same, we won't check again
                    // However, if there's any inconsistencies, we keep checking, and keep the highest-read quality as the real value
                    if (!dictGilesQualityRechecked[c_RActorGuid])
                    {
                        ItemQuality temporaryItemQualityCheck = ItemQuality.Invalid;
                        try
                        {
                            temporaryItemQualityCheck = (ItemQuality)c_CommonData.GetAttribute<int>(ActorAttributeType.ItemQualityLevel);
                            // If the newly-received quality is higher, then store the new quality
                            if (temporaryItemQualityCheck > c_ItemQuality)
                            {
                                dictGilesQualityCache[c_RActorGuid] = temporaryItemQualityCheck;
                                c_ItemQuality = temporaryItemQualityCheck;
                            }
                            // And now flag it so we don't check this item again
                            dictGilesQualityRechecked[c_RActorGuid] = true;
                        }
                        catch
                        {
                            DbHelper.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement,
                                "Safely handled exception double-checking item-quality for item {0} [{1}]", c_Name, c_ActorSNO);
                        }
                    }
                }
            }
            // Item stats
            RefreshItemStats(itemBaseType);
            // Ignore it if it's not in range yet - allow legendary items to have 15 feet extra beyond our profile max loot radius
            float fExtraRange = 0f;
			// !sp - loot range extension range for legendaries
            if (iKeepLootRadiusExtendedFor > 0) {
                fExtraRange = 30f;
			}
            if (c_ItemQuality >= ItemQuality.Rare4)
            {
                fExtraRange = iCurrentMaxLootRadius; //!sp - double range for Rares
            }
            if (c_ItemQuality >= ItemQuality.Legendary)
            {
                fExtraRange = iCurrentMaxLootRadius; //!sp - double range for Rares
            }
            if (c_ItemQuality >= ItemQuality.Legendary)
            {
                fExtraRange = 10*iCurrentMaxLootRadius; //!sp - mega range for Legendaries
            }

            if (c_CentreDistance > (iCurrentMaxLootRadius + fExtraRange))
            {
                AddToCache = false;
                c_IgnoreSubStep = "OutOfRange";
            }

            // Get whether or not we want this item, cached if possible
            if (!dictGilesPickupItem.TryGetValue(c_RActorGuid, out AddToCache))
            {
                if (Settings.Loot.ItemFilterMode == global::GilesTrinity.Settings.Loot.ItemFilterMode.DemonBuddy)
                {
                    AddToCache = ItemManager.EvaluateItem((ACDItem)c_CommonData, ItemManager.RuleType.PickUp);
                }
                else
                {
                    AddToCache = GilesPickupItemValidation(c_Name, c_ItemLevel, c_ItemQuality, c_BalanceID, c_DBItemType, c_item_tFollowerType, c_GameDynamicID);
                }
                dictGilesPickupItem.Add(c_RActorGuid, AddToCache);
            }
            // Using DB built-in item rules
            if (AddToCache)
            {
                if (ForceVendorRunASAP)
                {
                    AddToCache = false;
                    c_IgnoreSubStep = "ForcedVendoring";
                }
                AddToCache = true;
            }

            AddToCache = MosterObstacleInPathCacheObject(AddToCache);

            // Didn't pass giles pickup rules/DB internal rule match, so ignore it
            if (!AddToCache)
                c_IgnoreSubStep = "NoMatchingRule";

            return AddToCache;
        }
        private static bool RefreshGilesGold(bool AddToCache)
        {
            double iPercentage = 0;
            int rangedMinimumStackSize = 0;
            AddToCache = true;

            // Get the gold amount of this pile, cached if possible
            if (!dictGilesGoldAmountCache.TryGetValue(c_RActorGuid, out c_GoldStackSize))
            {
                try
                {
                    c_GoldStackSize = c_CommonData.GetAttribute<int>(ActorAttributeType.Gold);
                }
                catch
                {
                    DbHelper.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "Safely handled exception getting gold pile amount for item {0} [{1}]", c_Name, c_ActorSNO);
                    AddToCache = false;
                }
                dictGilesGoldAmountCache.Add(c_RActorGuid, c_GoldStackSize);
            }
			
			// Ignore gold piles that are (currently) too small...
            rangedMinimumStackSize = Settings.Loot.Pickup.MinimumGoldStack;
			int min_cash = 100;	//absolute min cash to consider
			int max_distance = 80;
			if (c_GoldStackSize < min_cash) {
				rangedMinimumStackSize = min_cash;
			} else if (c_CentreDistance >= max_distance) {
				rangedMinimumStackSize = 0;	//too far away
			} else {
				//scale the min stack size based on distance
				//this will enable smaller, local cash values to be picked up
				//while enroute, picking up items or larger amounts
				//better for toons with low pickup range
				int min_range = 6; //anything below this should be collected
				int max_range = 30; //anything beyond this should be at the upper threshold
				int max_cash = Math.Max(min_cash, rangedMinimumStackSize);
				double cash_range = Math.Max(0, c_CentreDistance-min_range);
				double rangedPerc = cash_range/(max_range-min_range); //no ceiling on this to capture distant, high values. twice distance=twice value
				int newMinStack = (int)Math.Floor(rangedPerc * (max_cash-min_cash))+min_cash;
				rangedMinimumStackSize = newMinStack;
				if (c_GoldStackSize >= rangedMinimumStackSize) {
					//Logging.Write("[SP] Gold v=" + c_GoldStackSize + " ,d=" + c_CentreDistance + ",w=" + rangedPerc + ",nms="+newMinStack);
				}
			}
            if (c_GoldStackSize < rangedMinimumStackSize)
            {
                AddToCache = false;
            }

            // Blacklist gold piles already in pickup radius range
            if (c_CentreDistance <= ZetaDia.Me.GoldPickUpRadius)
            {
                hashRGUIDBlacklist3.Add(c_RActorGuid);
                hashRGUIDBlacklist60.Add(c_RActorGuid);
                AddToCache = false;
            }

            DbHelper.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "Gold Stack {0} has iPercentage {1} with rangeMinimumStackSize: {2} Distance: {3} MininumGoldStack: {4} PickupRadius: {5} AddToCache: {6}",
                c_GoldStackSize, iPercentage, rangedMinimumStackSize, c_CentreDistance, Settings.Loot.Pickup.MinimumGoldStack, ZetaDia.Me.GoldPickUpRadius, AddToCache);

            return AddToCache;
        }
        private static bool RefreshGilesGizmo(bool AddToCache)
        {
            // start as true, then set as false as we go. If nothing matches below, it will return true.
            AddToCache = true;
            // Check the primary object blacklist
            if (hashSNOIgnoreBlacklist.Contains(c_ActorSNO))
            {
                AddToCache = false;
                c_IgnoreSubStep = "hashSNOIgnoreBlacklist";
                //return bWantThis;
            }
            // Ignore it if it's not in range yet, except health wells
            if ((c_RadiusDistance > iCurrentMaxLootRadius || c_RadiusDistance > 50) && c_ObjectType != GObjectType.HealthWell)
            {
                AddToCache = false;
                c_IgnoreSubStep = "NotInRange";
                //return bWantThis;
            }
            if (c_Name.ToLower().StartsWith("minimapicon"))
            {
                // Minimap icons caused a few problems in the past, so this force-blacklists them
                hashRGUIDBlacklist60.Add(c_RActorGuid);
                c_IgnoreSubStep = "minimapicon";
                AddToCache = false;
                //return bWantThis;
            }
            // Retrieve collision sphere radius, cached if possible
            if (!dictGilesCollisionSphereCache.TryGetValue(c_ActorSNO, out c_Radius))
            {
                try
                {
                    c_Radius = c_diaObject.CollisionSphere.Radius;

                    //// Take 8 from the radius
                    //c_fRadius -= 10f;
                    //// Minimum range clamp
                    //if (c_fRadius <= 1f)
                    //    c_fRadius = 1f;
                    //// Maximum range clamp
                    //if (c_fRadius >= 16f)
                    //    c_fRadius = 16f;
                }
                catch
                {
                    DbHelper.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "Safely handled exception getting collisionsphere radius for object {0} [{1}]", c_Name, c_ActorSNO);
                    AddToCache = false;
                    //return bWantThis;
                }
                dictGilesCollisionSphereCache.Add(c_ActorSNO, c_Radius);
            }

            // A "fake distance" to account for the large-object size of monsters
            c_RadiusDistance -= (float)c_Radius;
            if (c_RadiusDistance <= 1f)
                c_RadiusDistance = 1f;

            // Anything that's been disabled by a script
            bool bDisabledByScript = false;
            try
            {
                bDisabledByScript = c_CommonData.GetAttribute<int>(ActorAttributeType.GizmoDisabledByScript) > 0;
            }
            catch
            {
                DbHelper.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement,
                    "Safely handled exception getting Gizmo-Disabled-By-Script attribute for object {0} [{1}]", c_Name, c_ActorSNO);
                AddToCache = false;
            }
            if (bDisabledByScript)
            {
                AddToCache = false;
                c_IgnoreSubStep = "GizmoDisabledByScript";
            }
            // Now for the specifics
            int iThisPhysicsSNO;
            int iExtendedRange;
            double iMinDistance;
            bool GizmoUsed = false;
            switch (c_ObjectType)
            {
                case GObjectType.Door:
                    {
                        AddToCache = true;
                        try
                        {
                            string currentAnimation = c_CommonData.CurrentAnimation.ToString().ToLower();
                            GizmoUsed = currentAnimation.EndsWith("open") || currentAnimation.EndsWith("opening");

                            // special hax for A3 Iron Gates
                            if (currentAnimation.Contains("IronGate") && currentAnimation.Contains("Open"))
                                GizmoUsed = false;
                            if (currentAnimation.Contains("IronGate") && currentAnimation.Contains("idle"))
                                GizmoUsed = true;
                        }
                        catch { }
                        if (GizmoUsed)
                        {
                            hashRGUIDBlacklist90.Add(c_RActorGuid);
                            AddToCache = false;
                            c_IgnoreSubStep = "Door is Open or Opening";
                        }
                        if (AddToCache)
                        {
                            try
                            {
                                DiaGizmo door = null;
                                if (c_diaObject is GizmoDoor)
                                {
                                    door = (GizmoDoor)c_diaObject;

                                    if (door != null && door.IsGizmoDisabledByScript)
                                    {
                                        hashRGUIDBlacklist90.Add(c_RActorGuid);
                                        AddToCache = false;
                                        c_IgnoreSubStep = "DoorDisabledbyScript";
                                    }
                                }
                                else
                                {
                                    AddToCache = false;
                                    c_IgnoreSubStep = "InvalidCastToDoor";
                                }
                            }

                            catch { }
                        }
                    }
                    break;
                case GObjectType.Interactable:
                    AddToCache = true;
                    // Special interactables
                    if (c_CentreDistance > 30f)
                    {
                        AddToCache = false;
                        //return bWantThis;
                    }
                    c_Radius = 4f;
                    break;
                case GObjectType.HealthWell:
                    {
                        AddToCache = true;
                        try
                        {
                            GizmoUsed = (c_CommonData.GetAttribute<int>(ActorAttributeType.GizmoCharges) <= 0 && c_CommonData.GetAttribute<int>(ActorAttributeType.GizmoCharges) > 0);
                        }
                        catch
                        {
                            DbHelper.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "Safely handled exception getting shrine-been-operated attribute for object {0} [{1}]", c_Name, c_ActorSNO);
                            AddToCache = true;
                            //return bWantThis;
                        }
                        if (GizmoUsed)
                        {
                            c_IgnoreSubStep = "GizmoCharges";
                            AddToCache = false;
                            //return bWantThis;
                        }
                        bWaitingAfterPower = true;
                    }
                    break;
                case GObjectType.Shrine:
                    {
                        AddToCache = true;
                        // Shrines
                        // Check if either we want to ignore all shrines
                        if (!Settings.WorldObject.UseShrine)
                        {
                            // We're ignoring all shrines, so blacklist this one
                            hashRGUIDBlacklist60.Add(c_RActorGuid);
                            AddToCache = false;
                            //return bWantThis;
                        }
                        // Already used, blacklist it and don't look at it again
                        try
                        {
                            GizmoUsed = (c_CommonData.GetAttribute<int>(ActorAttributeType.GizmoHasBeenOperated) > 0);
                        }
                        catch
                        {
                            DbHelper.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "Safely handled exception getting shrine-been-operated attribute for object {0} [{1}]", c_Name, c_ActorSNO);
                            AddToCache = false;
                            //return bWantThis;
                        }
                        if (GizmoUsed)
                        {
                            // It's already open!
                            hashRGUIDBlacklist60.Add(c_RActorGuid);
                            c_IgnoreSubStep = "GizmoHasBeenOperated";
                            AddToCache = false;
                            //return bWantThis;
                        }
                        // Bag it!
                        c_Radius = 4f;
                        break;
                    }
                case GObjectType.Barricade:
                    {
                        AddToCache = true;
                        // Get the cached physics SNO of this object
                        if (!dictPhysicsSNO.TryGetValue(c_ActorSNO, out iThisPhysicsSNO))
                        {
                            try
                            {
                                iThisPhysicsSNO = c_diaObject.PhysicsSNO;
                            }
                            catch
                            {
                                DbHelper.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "Safely handled exception getting physics SNO for object {0} [{1}]", c_Name, c_ActorSNO);
                                AddToCache = false;
                                //return bWantThis;
                            }
                            dictPhysicsSNO.Add(c_ActorSNO, iThisPhysicsSNO);
                        }
                        // No physics mesh? Ignore this destructible altogether
                        //if (iThisPhysicsSNO <= 0)
                        //{
                        //    
                        // No physics mesh on a destructible, probably bugged
                        //    hashRGUIDIgnoreBlacklist60.Add(tmp_iThisRActorGuid);
                        //    tmp_cacheIgnoreSubStep = "NoPhysicsSNO";
                        //    bWantThis = false;
                        //    
                        //return bWantThis;
                        //}
                        // Set min distance to user-defined setting
                        iMinDistance = Settings.WorldObject.DestructibleRange + (c_Radius * 0.70);
                        if (ForceCloseRangeTarget)
                            iMinDistance += 6f;
                        // Large objects, like logs - Give an extra xx feet of distance
                        if (dictSNOExtendedDestructRange.TryGetValue(c_ActorSNO, out iExtendedRange))
                            iMinDistance = Settings.WorldObject.DestructibleRange + iExtendedRange;
                        // This object isn't yet in our destructible desire range
                        if (iMinDistance <= 0 || c_CentreDistance > iMinDistance)
                        {
                            c_IgnoreSubStep = "NotInBarricadeRange";
                            AddToCache = false;
                            //return bWantThis;
                        }
                        break;
                    }
                case GObjectType.Destructible:
                    {
                        AddToCache = true;
                        // Get the cached physics SNO of this object
                        if (!dictPhysicsSNO.TryGetValue(c_ActorSNO, out iThisPhysicsSNO))
                        {
                            try
                            {
                                iThisPhysicsSNO = c_diaObject.PhysicsSNO;
                            }
                            catch
                            {
                                DbHelper.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "Safely handled exception getting physics SNO for object {0} [{1}]", c_Name, c_ActorSNO);
                                AddToCache = false;
                                //return bWantThis;
                            }
                            dictPhysicsSNO.Add(c_ActorSNO, iThisPhysicsSNO);
                        }
                        // No physics mesh? Ignore this destructible altogether
                        //if (iThisPhysicsSNO <= 0)
                        //{
                        //    
                        // No physics mesh on a destructible, probably bugged
                        //    hashRGUIDIgnoreBlacklist60.Add(tmp_iThisRActorGuid);
                        //    tmp_cacheIgnoreSubStep = "NoPhysicsMesh";
                        //    bWantThis = false;
                        //    
                        //return bWantThis;
                        //}
                        // Set min distance to user-defined setting
                        iMinDistance = Settings.WorldObject.DestructibleRange + (c_Radius * 0.30);
                        if (ForceCloseRangeTarget)
                            iMinDistance += 6f;
                        // Large objects, like logs - Give an extra xx feet of distance
                        if (dictSNOExtendedDestructRange.TryGetValue(c_ActorSNO, out iExtendedRange))
                            iMinDistance = Settings.WorldObject.DestructibleRange + iExtendedRange;
                        // This object isn't yet in our destructible desire range
                        if (iMinDistance <= 0 || c_RadiusDistance > iMinDistance)
                        {
                            AddToCache = false;
                            c_IgnoreSubStep = "NotInDestructableRange";
                        }
                        // Only break destructables if we're stuck
                        if (!GilesPlayerMover.UnstuckChecker())
                        {
                            AddToCache = false;
                            c_IgnoreSubStep = "NotStuck";
                        }
                        // If we're standing on it, usually right before above unstucker returns true
                        if (c_RadiusDistance <= 2f)
                        {
                            AddToCache = true;
                        }

                        // special mojo for whitelists
                        if (hashSNOInteractWhitelist.Contains(c_ActorSNO))
                            AddToCache = true;

                        if (hashSNOContainerResplendant.Contains(c_ActorSNO))
                            AddToCache = true;

                        break;
                    }
                case GObjectType.Container:
                    {
                        // We want to do some vendoring, so don't open anything new yet
                        if (ForceVendorRunASAP)
                        {
                            AddToCache = false;
                            //return bWantThis;
                        }
                        // Already open, blacklist it and don't look at it again
                        bool bThisOpen = false;
                        try
                        {
                            bThisOpen = (c_CommonData.GetAttribute<int>(ActorAttributeType.ChestOpen) > 0);
                        }
                        catch
                        {
                            DbHelper.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "Safely handled exception getting container-been-opened attribute for object {0} [{1}]", c_Name, c_ActorSNO);
                            AddToCache = false;
                            //return bWantThis;
                        }
                        if (bThisOpen)
                        {
                            // It's already open!
                            hashRGUIDBlacklist60.Add(c_RActorGuid);
                            AddToCache = false;
                            //return bWantThis;
                        }
                        else if (!bThisOpen && c_Name.ToLower().Contains("chest") && !c_Name.ToLower().Contains("chest_rare"))
                        {
                            // This should make the magic happen with Chests we actually want :)
                            AddToCache = true;
                        }
                        // Default to blacklisting all containers, then find reasons not to
                        bool bBlacklistThis = true;
                        iMinDistance = 0f;
                        // Get the cached physics SNO of this object
                        if (!dictPhysicsSNO.TryGetValue(c_ActorSNO, out iThisPhysicsSNO))
                        {
                            try
                            {
                                iThisPhysicsSNO = c_diaObject.PhysicsSNO;
                            }
                            catch
                            {
                                DbHelper.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "Safely handled exception getting physics SNO for object {0} [{1}]", c_Name, c_ActorSNO);
                                AddToCache = false;
                                //return bWantThis;
                            }
                            dictPhysicsSNO.Add(c_ActorSNO, iThisPhysicsSNO);
                        }
                        // Any physics mesh? Give a minimum distance of 5 feet
                        if (c_Name.ToLower().Contains("corpse") && Settings.WorldObject.IgnoreNonBlocking)
                        {
                            bBlacklistThis = true;
                        }
                        else if (iThisPhysicsSNO > 0 || !Settings.WorldObject.IgnoreNonBlocking)
                        {
                            //Logging.WriteDiagnostic("[Trinity] open container " + tmp_sThisInternalName + "[" + tmp_iThisActorSNO.ToString() + "]" + iThisPhysicsSNO);
                            bBlacklistThis = false;
                            iMinDistance = Settings.WorldObject.ContainerOpenRange;
                        }
                        else
                        {
                            bBlacklistThis = true;
                        }
                        // Whitelist for chests we want to open if we ever get close enough to them
                        if (hashSNOContainerWhitelist.Contains(c_ActorSNO))
                        {
                            bBlacklistThis = false;
                            if (Settings.WorldObject.ContainerOpenRange > 0)
                                iMinDistance = Settings.WorldObject.ContainerOpenRange + 5;
                        }
                        else if (c_Name.ToLower().Contains("chest") && !c_Name.ToLower().Contains("chest_rare"))
                        {
                            DbHelper.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "GSDebug: Possible Chest SNO: {0}, SNO={1}", c_Name, c_ActorSNO);
                        }
                        // Superlist for rare chests etc.
                        if (hashSNOContainerResplendant.Contains(c_ActorSNO))
                        {
                            bBlacklistThis = false;
                            if (Settings.WorldObject.ContainerOpenRange > 0)
                                iMinDistance = Settings.WorldObject.ContainerOpenRange + 20;
                            else
                                iMinDistance = 10;
                        }
                        else if (c_Name.Contains("chest_rare"))
                        {
                            DbHelper.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "GSDebug: Possible Resplendant Chest SNO: {0}, SNO={1}", c_Name, c_ActorSNO);
                        }
                        // Blacklist this if it's something we should never bother looking at again
                        if (bBlacklistThis)
                        {
                            hashRGUIDBlacklist60.Add(c_RActorGuid);
                            AddToCache = false;
                            //return bWantThis;
                        }
                        if (iMinDistance <= 0 || c_CentreDistance > iMinDistance)
                        {
                            AddToCache = false;
                            //return bWantThis;
                        }
                        // Bag it!
                        //tmp_fThisRadius = 4f;
                        //bWantThis = true;
                        break;
                    }
            }
            return AddToCache;
        }
        private static bool RefreshGilesAvoidance(bool AddToCache)
        {
            AddToCache = true;
            // Note if you are looking here - an AOE object won't even appear at this stage if you have Settings.Combat.Misc.AvoidAOE switched off!
            //if (!hashAvoidanceSNOList.Contains(tmp_iThisActorSNO))
            //{
            //    Logging.WriteDiagnostic("GSDebug: Invalid avoidance detected, SNO=" + tmp_iThisActorSNO.ToString() + ", name=" + tmp_sThisInternalName + ", object type=" +
            //        tmp_ThisGilesObjectType.ToString());
            //    bWantThis = false;
            //    
            //return bWantThis;
            //}

            try
            {
                c_CurrentAnimation = c_CommonData.CurrentAnimation;
            }
            catch { }

            bool bIgnoreThisAvoidance = false;
            double dThisHealthAvoid = GetAvoidanceHealth();
            // Monks with Serenity up ignore all AOE's
            if (iMyCachedActorClass == ActorClass.Monk && hashPowerHotbarAbilities.Contains(SNOPower.Monk_Serenity) && GilesHasBuff(SNOPower.Monk_Serenity))
            {
                // Monks with serenity are immune
                bIgnoreThisAvoidance = true;
            }
            // Witch doctors with spirit walk available and not currently Spirit Walking will subtly ignore ice balls, arcane, desecrator & plague cloud
            if (iMyCachedActorClass == ActorClass.WitchDoctor && hashPowerHotbarAbilities.Contains(SNOPower.Witchdoctor_SpiritWalk) &&
                (!GilesHasBuff(SNOPower.Witchdoctor_SpiritWalk) && GilesUseTimer(SNOPower.Witchdoctor_SpiritWalk)) || GilesHasBuff(SNOPower.Witchdoctor_SpiritWalk))
            {
                if (c_ActorSNO == 223675 || c_ActorSNO == 402 || c_ActorSNO == 219702 || c_ActorSNO == 221225 || c_ActorSNO == 84608 || c_ActorSNO == 108869)
                {
                    // Ignore ICE/Arcane/Desc/PlagueCloud altogether with spirit walk up or available
                    bIgnoreThisAvoidance = true;
                }
            }
            // Remove ice balls if the barbarian has wrath of the berserker up, and reduce health from most other SNO avoidances
            if (iMyCachedActorClass == ActorClass.Barbarian && hashPowerHotbarAbilities.Contains(SNOPower.Barbarian_WrathOfTheBerserker) && GilesHasBuff(SNOPower.Barbarian_WrathOfTheBerserker))
            {
                if (c_ActorSNO == 223675 || c_ActorSNO == 402)
                {
                    // Ignore ice-balls altogether with wrath up
                    bIgnoreThisAvoidance = true;
                }
                else
                {
                    // Use half-health for anything else except arcanes or desecrate with wrath up
                    if (c_ActorSNO == 219702 || c_ActorSNO == 221225)
                        // Arcane
                        bIgnoreThisAvoidance = true;
                    else if (c_ActorSNO == 84608)
                        // Desecrator
                        dThisHealthAvoid *= 0.2;
                    else if (c_ActorSNO == 4803 || c_ActorSNO == 4804 || c_ActorSNO == 224225 || c_ActorSNO == 247987)
                        // Molten core
                        dThisHealthAvoid *= 1;
                    else
                        // Anything else
                        dThisHealthAvoid *= 0.3;
                }
            }
            // Add it to the list of known avoidance objects, *IF* our health is lower than this avoidance health limit
            if (!bIgnoreThisAvoidance && dThisHealthAvoid >= playerStatus.CurrentHealthPct)
            {
                // Generate a "weight" for how badly we want to avoid this obstacle, based on a percentage of 100% the avoidance health is, multiplied into a max of 200 weight
                double dThisWeight = (200 * dThisHealthAvoid);

                hashAvoidanceObstacleCache.Add(new GilesObstacle(c_Position, (float)GetAvoidanceRadius(), c_ActorSNO, dThisWeight, c_Name));

                // Is this one under our feet? If so flag it up so we can find an avoidance spot
                if (c_CentreDistance <= GetAvoidanceRadius())
                {
                    StandingInAvoidance = true;

                    // Note if this is a travelling projectile or not so we can constantly update our safe points
                    if (hashAvoidanceSNOProjectiles.Contains(c_ActorSNO))
                        IsAvoidingProjectiles = true;
                }
            }
            // Butcher WIP
            //bool tmp_bHasBuff = false;
            //try
            //{
            //    tmp_bHasBuff = tempCommonData.GetAttribute<int>(ActorAttributeType.BuffVisualEffect) > 0;
            //}
            //catch
            //{
            //}
            // continue because we aren't actually treating this as a TARGET - avoidance has special handling after all targets are found
            return AddToCache;
        }
        /// <summary>
        /// Special handling for whether or not we want to cache an object that's not in LoS
        /// </summary>
        /// <param name="c_diaObject"></param>
        /// <param name="AddToCache"></param>
        /// <returns></returns>
        private static bool RefreshStepIgnoreLoS(bool AddToCache = false)
        {
            try
            {

                if (c_ObjectType == GObjectType.Unit)
                {
                    bool isNavigable = pf.IsNavigable(gp.WorldToGrid(c_Position.ToVector2()));

                    if (!isNavigable)
                    {
                        AddToCache = false;
                        c_IgnoreSubStep = "NotNavigable";
                    }
                    // Ignore units not in LoS except bosses, rares, champs
                    if (c_ObjectType == GObjectType.Unit && !c_diaObject.InLineOfSight && !(c_unit_IsBoss && c_unit_IsElite || c_unit_IsRare))
                    {
                        AddToCache = false;
                        c_IgnoreSubStep = "UnitNotInLoS";
                    }
                    // always set true for bosses nearby
                    if (c_unit_IsBoss && c_RadiusDistance < 100f)
                    {
                        AddToCache = true;
                        c_IgnoreSubStep = "";
                    }
                    // always take the current target even if not in LoS
                    if (c_RActorGuid == CurrentTargetRactorGUID)
                    {
                        AddToCache = true;
                        c_IgnoreSubStep = "";
                    }
                }

                // Simple whitelist for LoS 
                if (LineOfSightWhitelist.Contains(c_ActorSNO))
                {
                    AddToCache = true;
                    c_IgnoreSubStep = "";
                }
                // Always pickup Infernal Keys whether or not in LoS
                if (hashForceSNOToItemList.Contains(c_ActorSNO))
                {
                    AddToCache = true;
                    c_IgnoreSubStep = "";
                }


                //if (!thisobj.InLineOfSight && thisobj.ZDiff < 14f && !tmp_unit_bThisBoss)
                //{
                //    bWantThis = false;
                //    tmp_cacheIgnoreSubStep = "IgnoreLoS";
                //    
                //return bWantThis;
                //}
            }
            catch
            {
                AddToCache = false;
                c_IgnoreSubStep = "IgnoreLoSException";
            }
            return AddToCache;
        }
        private static bool RefreshStepIgnoreUnknown(bool AddToCache)
        {
            // We couldn't get a valid object type, so ignore it
            if (!c_IsObstacle && c_ObjectType == GObjectType.Unknown)
            {
                AddToCache = false;
            }
            return AddToCache;
        }
        private static bool RefreshStepCachedACDGuid(bool AddToCache)
        {
            // Get the ACDGUID, cached if possible, only for non-avoidance stuff
            if (!c_IsObstacle && c_ObjectType != GObjectType.Avoidance)
            {
                if (!dictGilesACDGUIDCache.TryGetValue(c_RActorGuid, out c_ACDGUID))
                {
                    try
                    {
                        c_ACDGUID = c_diaObject.ACDGuid;
                    }
                    catch (Exception ex)
                    {
                        DbHelper.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "Safely handled exception getting ACDGUID for an object [{0}]", c_ActorSNO);
                        DbHelper.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "{0}", ex);
                        AddToCache = false;
                    }
                    dictGilesACDGUIDCache.Add(c_RActorGuid, c_ACDGUID);
                }
                // No ACDGUID, so shouldn't be anything we want to deal with
                if (c_ACDGUID == -1)
                {
                    AddToCache = false;
                }
            }
            else
            {
                // Give AOE's -1 ACDGUID, since it's not needed for avoidance stuff
                c_ACDGUID = -1;
            }
            return AddToCache;
        }
        private static bool RefreshStepCachedPosition(bool AddToCache)
        {
            // Try and get a cached position for anything that isn't avoidance or units (avoidance and units can move, sadly, so we risk DB mis-reads for those things!
            if (c_ObjectType != GObjectType.Avoidance && c_ObjectType != GObjectType.Unit)
            {
                // Get the position, cached if possible
                if (!dictGilesVectorCache.TryGetValue(c_RActorGuid, out c_Position))
                {
                    try
                    {
                        //c_vPosition = thisobj.Position;
                        Vector3 pos = c_diaObject.Position;

                        // always get Height of wherever the nav says it is (for flying things..)
                        c_Position = new Vector3(pos.X, pos.Y, gp.GetHeight(pos.ToVector2()));
                    }
                    catch (Exception ex)
                    {
                        DbHelper.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "Safely handled exception getting position for a static object [{0}]", c_ActorSNO);
                        DbHelper.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "{0}", ex);
                        AddToCache = false;
                    }
                    // Now cache it
                    dictGilesVectorCache.Add(c_RActorGuid, c_Position);
                }
            }
            // Ok pull up live-position data for units/avoidance now...
            else
            {
                try
                {
                    c_Position = c_diaObject.Position;
                }
                catch (Exception ex)
                {
                    DbHelper.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "Safely handled exception getting position for a unit or avoidance object [{0}]", c_ActorSNO);
                    DbHelper.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "{0}", ex);
                }
            }
            return AddToCache;
        }
        private static bool RefreshStepCachedDynamicIds(bool AddToCache)
        {
            // Try and grab the dynamic id and game balance id, if necessary and if possible
            if (c_ObjectType == GObjectType.Item)
            {
                // Get the Dynamic ID, cached if possible
                if (!dictGilesDynamicIDCache.TryGetValue(c_RActorGuid, out c_GameDynamicID))
                {
                    try
                    {
                        c_GameDynamicID = c_CommonData.DynamicId;
                    }
                    catch (Exception ex)
                    {
                        DbHelper.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "Safely handled exception getting DynamicID for item {0} [{1}]", c_Name, c_ActorSNO);
                        DbHelper.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "{0}", ex);
                        AddToCache = false;
                        //return bWantThis;
                    }
                    dictGilesDynamicIDCache.Add(c_RActorGuid, c_GameDynamicID);
                }
                // Get the Game Balance ID, cached if possible
                if (!dictGilesGameBalanceIDCache.TryGetValue(c_RActorGuid, out c_BalanceID))
                {
                    try
                    {
                        c_BalanceID = c_CommonData.GameBalanceId;
                    }
                    catch (Exception ex)
                    {
                        DbHelper.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "Safely handled exception getting GameBalanceID for item {0} [{1}]", c_Name, c_ActorSNO);
                        DbHelper.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "{0}", ex);
                        AddToCache = false;
                        //return bWantThis;
                    }
                    dictGilesGameBalanceIDCache.Add(c_RActorGuid, c_BalanceID);
                }
            }
            else
            {
                c_GameDynamicID = -1;
                c_BalanceID = -1;
            }
            return AddToCache;
        }
        private static bool RefreshStepNewObjectTypeZDiff(bool AddToCache)
        {
            // Ignore stuff which has a Z-height-difference too great, it's probably on a different level etc. - though not avoidance!
            if (c_ObjectType != GObjectType.Avoidance)
            {
                // Calculate the z-height difference between our current position, and this object's position
                c_ZDiff = Math.Abs(playerStatus.CurrentPosition.Z - c_Position.Z);
                switch (c_ObjectType)
                {
                    case GObjectType.Door:
                    case GObjectType.Unit:
                    case GObjectType.Barricade:
                        // Ignore monsters (units) who's Z-height is 14 foot or more than our own z-height
                        // rrrix: except bosses like Belial :)
                        if (c_ZDiff >= 14f && !c_unit_IsBoss)
                        {
                            AddToCache = false;
                            //return bWantThis;
                        }
                        break;
                    case GObjectType.Item:
                    case GObjectType.HealthWell:
                        // Items at 26+ z-height difference (we don't want to risk missing items so much)
                        if (c_ZDiff >= 26f)
                        {
                            AddToCache = false;
                            //return bWantThis;
                        }
                        break;
                    case GObjectType.Gold:
                    case GObjectType.Globe:
                        // Gold/Globes at 11+ z-height difference
                        if (c_ZDiff >= 11f)
                        {
                            AddToCache = false;
                            //return bWantThis;
                        }
                        break;
                    case GObjectType.Destructible:
                    case GObjectType.Shrine:
                    case GObjectType.Container:
                        // Destructibles, shrines and containers are the least important, so a z-height change of only 7 is enough to ignore (help avoid stucks at stairs etc.)
                        if (c_ZDiff >= 7f)
                        {
                            AddToCache = false;
                            //return bWantThis;
                        }
                        break;
                    case GObjectType.Interactable:
                        // Special interactable objects
                        if (c_ZDiff >= 9f)
                        {
                            AddToCache = false;
                            //return bWantThis;
                        }
                        break;
                    case GObjectType.Unknown:
                    default:
                        {
                            // Don't touch it!
                        }
                        break;
                }
            }
            else
            {
                AddToCache = true;
            }
            return AddToCache;
        }
        private static bool RefreshStepCachedPlayerSummons(bool AddToCache)
        {
            // Count up Mystic Allys, gargantuans, and zombies - if the player has those skills
            if (iMyCachedActorClass == ActorClass.Monk)
            {
                if (hashPowerHotbarAbilities.Contains(SNOPower.Monk_MysticAlly) && hashMysticAlly.Contains(c_ActorSNO))
                {
                    iPlayerOwnedMysticAlly++;
                    AddToCache = false;
                }
            }
            // Count up Demon Hunter pets
            if (iMyCachedActorClass == ActorClass.DemonHunter)
            {
                if (hashPowerHotbarAbilities.Contains(SNOPower.DemonHunter_Companion) && hashDHPets.Contains(c_ActorSNO))
                {
                    iPlayerOwnedDHPets++;
                    AddToCache = false;
                }
            }
            // Count up zombie dogs and gargantuans next
            if (iMyCachedActorClass == ActorClass.WitchDoctor)
            {
                if (hashPowerHotbarAbilities.Contains(SNOPower.Witchdoctor_Gargantuan) && hashGargantuan.Contains(c_ActorSNO))
                {
                    iPlayerOwnedGargantuan++;
                    AddToCache = false;
                }
                if (hashPowerHotbarAbilities.Contains(SNOPower.Witchdoctor_SummonZombieDog) && hashZombie.Contains(c_ActorSNO))
                {
                    iPlayerOwnedZombieDog++;
                    AddToCache = false;
                }
            }
            return AddToCache;
        }
        private static bool RefreshStepCheckBlacklists(bool AddToCache)
        {
            if (!hashAvoidanceSNOList.Contains(c_ActorSNO) && !hashAvoidanceBuffSNOList.Contains(c_ActorSNO))
            {
                // See if it's something we should always ignore like ravens etc.
                if (!c_IsObstacle && hashActorSNOIgnoreBlacklist.Contains(c_ActorSNO))
                {
                    AddToCache = false;
                    c_IgnoreSubStep = "hashActorSNOIgnoreBlacklist";
                    return AddToCache;
                }
                if (!c_IsObstacle && hashSNOIgnoreBlacklist.Contains(c_ActorSNO))
                {
                    AddToCache = false;
                    c_IgnoreSubStep = "hashSNOIgnoreBlacklist";
                    return AddToCache;
                }
                // Temporary ractor GUID ignoring, to prevent 2 interactions in a very short time which can cause stucks
                if (IgnoreTargetForLoops > 0 && IgnoreRactorGUID == c_RActorGuid)
                {
                    AddToCache = false;
                    c_IgnoreSubStep = "iIgnoreThisRactorGUID";
                    return AddToCache;
                }
                // Check our extremely short-term destructible-blacklist
                if (hashRGUIDDestructible3SecBlacklist.Contains(c_RActorGuid))
                {
                    AddToCache = false;
                    c_IgnoreSubStep = "hashRGUIDDestructible3SecBlacklist";
                    return AddToCache;
                }
                // Check our extremely short-term destructible-blacklist
                if (hashRGUIDBlacklist3.Contains(c_RActorGuid))
                {
                    AddToCache = false;
                    c_IgnoreSubStep = "hashRGUIDBlacklist3";
                    return AddToCache;
                }
                // See if it's on our 90 second blacklist (from being stuck targeting it), as long as it's distance is not extremely close
                if (hashRGUIDBlacklist90.Contains(c_RActorGuid))
                {
                    AddToCache = false;
                    c_IgnoreSubStep = "hashRGUIDBlacklist90";
                    return AddToCache;
                }
                // 60 second blacklist
                if (hashRGUIDBlacklist60.Contains(c_RActorGuid))
                {
                    AddToCache = false;
                    c_IgnoreSubStep = "hashRGUIDBlacklist60";
                    return AddToCache;
                }
                // 15 second blacklist
                if (hashRGUIDBlacklist15.Contains(c_RActorGuid))
                {
                    AddToCache = false;
                    c_IgnoreSubStep = "hashRGUIDBlacklist15";
                    return AddToCache;
                }
            }
            else
            {
                AddToCache = true;
            }
            return AddToCache;
        }


        private static bool MosterObstacleInPathCacheObject(bool AddToCache)
        {
            // Don't add an item if a monster is blocking our path
            if (hashMonsterObstacleCache.Any(o => GilesIntersectsPath(o.Location, o.Radius, playerStatus.CurrentPosition, c_Position)))
            {
                AddToCache = false;
            }
            return AddToCache;
        }
        private static string UtilSpacedConcat(params object[] args)
        {
            string output = "";
            foreach (object o in args)
            {
                output += o.ToString() + ", ";
            }
            return output;
        }
        private static double GetAvoidanceRadius()
        {
            try
            {
                return AvoidanceManager.GetAvoidanceRadiusBySNO(c_ActorSNO, c_Radius);
            }
            catch
            {
                return c_Radius;
            }

        }
        private static double GetAvoidanceRadius(int actorSNO)
        {
            try
            {
                return AvoidanceManager.GetAvoidanceRadiusBySNO(actorSNO, c_Radius);
            }
            catch
            {
                return c_Radius;
            }

        }
        private static double GetAvoidanceHealth()
        {
            try
            {
                return AvoidanceManager.GetAvoidanceHealthBySNO(c_ActorSNO, 1);
            }
            catch
            {
                // 100% unless specified
                return 1;
            }
        }
    }
}
