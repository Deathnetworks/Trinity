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
using System.Text;
using GilesTrinity.Cache;
using GilesTrinity.Settings.Combat;
using Zeta.Navigation;
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
            if (freshObject is DiaUnit)
                c_diaUnit = freshObject as DiaUnit;

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
            if (!AddToCache) { c_IgnoreReason = "CachedActorSNO"; return AddToCache; }

            // Have ActorSNO Check for SNO based navigation obstacle hashlist
            c_IsObstacle = hashSNONavigationObstacles.Contains(c_ActorSNO);

            // Get Internal Name
            AddToCache = RefreshInternalName(AddToCache);
            if (!AddToCache) { c_IgnoreReason = "InternalName"; return AddToCache; }
            // Get ACDGuid
            AddToCache = RefreshStepCachedACDGuid(AddToCache);
            if (!AddToCache) { c_IgnoreReason = "CachedACDGuid"; return AddToCache; }
            // Summons by the player 
            AddToCache = RefreshStepCachedPlayerSummons(AddToCache);
            if (!AddToCache) { c_IgnoreReason = "CachedPlayerSummons"; return AddToCache; }
            // Check Blacklists
            AddToCache = RefreshStepCheckBlacklists(AddToCache);
            if (!AddToCache) { c_IgnoreReason = "CheckBlacklists"; return AddToCache; }

            using (new PerformanceLogger("RefreshDiaObject.CachedType"))
            {
                /*
                 * Begin main refresh routine
                 */
                // Set Giles Object Type
                AddToCache = RefreshStepCachedObjectType(AddToCache);
                if (!AddToCache) { c_IgnoreReason = "CachedObjectType"; return AddToCache; }
            }

            // Get Cached Position
            AddToCache = RefreshStepCachedPosition(AddToCache);
            if (!AddToCache) { c_IgnoreReason = "CachedPosition"; return AddToCache; }

            if (c_ObjectType == GObjectType.Item)
            {
                if (GenericBlacklist.ContainsKey(c_ObjectHash))
                {
                    AddToCache = false;
                    c_IgnoreReason = "GenericBlacklist";
                    return AddToCache;
                }

                /* Generic Blacklisting for shifting RActorGUID bug */
                c_ObjectHash = HashGenerator.GenerateWorldObjectHash(c_ActorSNO, c_Position, c_ObjectType.ToString(), GilesTrinity.CurrentWorldDynamicId);
            }

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

            using (new PerformanceLogger("RefreshDiaObject.MainObjectType"))
            {
                /* 
                 * Main Switch on Object Type - Refresh individual object types (Units, Items, Gizmos)
                 */
                RefreshStepMainObjectType(ref AddToCache);
                if (!AddToCache) { c_IgnoreReason = "MainObjectType"; return AddToCache; }
            }

            // Ignore anything unknown
            AddToCache = RefreshStepIgnoreUnknown(AddToCache);
            if (!AddToCache) { c_IgnoreReason = "IgnoreUnknown"; return AddToCache; }

            using (new PerformanceLogger("RefreshDiaObject.LoS"))
            {
                // Ignore all LoS
                AddToCache = RefreshStepIgnoreLoS(AddToCache);
                if (!AddToCache) { c_IgnoreReason = "IgnoreLoS"; return AddToCache; }
            }

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
                        InternalName = c_InternalName,
                        ACDGuid = c_ACDGUID,
                        RActorGuid = c_RActorGuid,
                        DynamicID = c_GameDynamicID,
                        BalanceID = c_BalanceID,
                        ActorSNO = c_ActorSNO,
                        ItemLevel = c_ItemLevel,
                        GoldAmount = c_GoldStackSize,
                        OneHanded = c_IsOneHandedItem,
                        TwoHanded = c_IsTwoHandedItem,
                        ItemQuality = c_ItemQuality,
                        DBItemBaseType = c_DBItemBaseType,
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
                        HitPointsPct = c_HitPointsPct,
                        Radius = c_Radius,
                        MonsterStyle = c_unit_MonsterSize,
                        IsEliteRareUnique = c_IsEliteRareUnique,
                        ForceLeapAgainst = c_ForceLeapAgainst,
                        HasDotDPS = c_HasDotDPS,
                        ObjectHash = c_ObjectHash,
                        KillRange = c_KillRange
                    });
            }
            return true;
        }

        private static void AddGizmoToNavigationObstacleCache()
        {
            switch (c_ObjectType)
            {
                case GObjectType.Barricade:
                case GObjectType.Container:
                case GObjectType.Destructible:
                case GObjectType.Door:
                case GObjectType.HealthWell:
                case GObjectType.Interactable:
                case GObjectType.Shrine:
                    hashNavigationObstacleCache.Add(new GilesObstacle()
                    {
                        ActorSNO = c_ActorSNO,
                        Radius = c_Radius,
                        Location = c_Position,
                        Name = c_InternalName,
                        Weight = c_Weight
                    });
                    break;
            }
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
                    hashMonsterObstacleCache.Add(new GilesObstacle(c_Position, c_Radius, c_ActorSNO));
                }
                else
                {
                    // Add to the collision-list
                    hashMonsterObstacleCache.Add(new GilesObstacle(c_Position, c_Radius, c_ActorSNO));
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
            c_InternalName = "";
            c_IgnoreReason = "";
            c_IgnoreSubStep = "";
            c_ACDGUID = -1;
            c_RActorGuid = -1;
            c_GameDynamicID = -1;
            c_BalanceID = -1;
            c_ActorSNO = -1;
            c_ItemLevel = -1;
            c_GoldStackSize = -1;
            c_HitPointsPct = -1;
            c_HitPoints = -1;
            c_IsOneHandedItem = false;
            c_IsTwoHandedItem = false;
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
            c_HasBeenNavigable = false;
            c_HasBeenRaycastable = false;
            c_HasBeenInLoS = false;
            c_ItemMd5Hash = string.Empty;
            c_ItemQuality = ItemQuality.Invalid;
            c_DBItemBaseType = ItemBaseType.None;
            c_DBItemType = ItemType.Unknown;
            c_item_tFollowerType = FollowerType.None;
            c_item_GItemType = GItemType.Unknown;
            c_unit_MonsterSize = MonsterSize.Unknown;
            c_diaObject = null;
            c_diaUnit = null;
            c_CurrentAnimation = SNOAnim.Invalid;
            c_HasDotDPS = false;
            c_ObjectHash = String.Empty;
            c_KillRange = 0f;
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
            if (!dictGilesInternalNameCache.TryGetValue(c_RActorGuid, out c_InternalName))
            {
                try
                {
                    c_InternalName = nameNumberTrimRegex.Replace(c_diaObject.Name, "");
                }
                catch (Exception ex)
                {
                    DbHelper.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "Safely handled exception getting InternalName for an object [{0}]", c_ActorSNO);
                    DbHelper.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "{0}", ex);
                    AddToCache = false;
                }
                dictGilesInternalNameCache.Add(c_RActorGuid, c_InternalName);
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
            c_CentreDistance = Vector3.Distance(PlayerStatus.CurrentPosition, c_Position);
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

            if (ignoreNames.Any(n => c_InternalName.ToLower().Contains(n.ToLower())))
            {
                AddToCache = false;
                c_IgnoreSubStep = "IgnoreNames";
                return AddToCache;
            }
            // Either get the cached Giles object type, or calculate it fresh
            if (!c_IsObstacle && !dictGilesObjectTypeCache.TryGetValue(c_RActorGuid, out c_ObjectType))
            {
                // See if it's an avoidance first from the SNO
                if (Settings.Combat.Misc.AvoidAOE && (hashAvoidanceSNOList.Contains(c_ActorSNO) || hashAvoidanceBuffSNOList.Contains(c_ActorSNO) || hashAvoidanceSNOProjectiles.Contains(c_ActorSNO)))
                {
                    using (new PerformanceLogger("RefreshCachedType.0"))
                    {
                        // Checking for BuffVisualEffect - for Butcher, maybe useful other places?
                        if (hashAvoidanceBuffSNOList.Contains(c_ActorSNO))
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
                        else
                        {
                            // Avoidance isn't disabled, so set this object type to avoidance
                            c_ObjectType = GObjectType.Avoidance;
                        }
                    }
                }
                // It's not an avoidance, so let's calculate it's object type "properly"
                else
                {
                    // Calculate the object type of this object
                    if (c_diaObject is DiaUnit)
                    {
                        using (new PerformanceLogger("RefreshCachedType.1"))
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
                    }
                    else if (hashForceSNOToItemList.Contains(c_ActorSNO) || (c_diaObject is DiaItem))
                    {
                        using (new PerformanceLogger("RefreshCachedType.2"))
                        {
                            if (c_CommonData == null)
                            {
                                AddToCache = false;
                            }
                            if (c_CommonData != null && c_diaObject.ACDGuid != c_CommonData.ACDGuid)
                            {
                                AddToCache = false;
                            }
                            if (c_InternalName.ToLower().StartsWith("gold"))
                            {
                                c_ObjectType = GObjectType.Gold;
                            }
                            else
                            {
                                c_ObjectType = GObjectType.Item;
                            }
                        }
                    }
                    else if (c_diaObject is DiaGizmo)
                    {
                        DiaGizmo c_diaGizmo;
                        using (new PerformanceLogger("RefreshCachedType.3"))
                        {
                            c_diaGizmo = (DiaGizmo)c_diaObject;
                        }
                        using (new PerformanceLogger("RefreshCachedType.4"))
                        {

                            if (c_diaObject is GizmoShrine)
                                c_ObjectType = GObjectType.Shrine;
                            else if (c_diaObject is GizmoHealthwell)
                                c_ObjectType = GObjectType.HealthWell;
                            else if (c_diaObject is GizmoDestructibleLootContainer)
                                c_ObjectType = GObjectType.Destructible;
                            else if (c_diaObject is GizmoLootContainer)
                                c_ObjectType = GObjectType.Container;
                            else if (c_diaObject is GizmoDoor)
                                c_ObjectType = GObjectType.Door;
                            else if (c_diaGizmo.IsBarricade)
                                c_ObjectType = GObjectType.Barricade;
                            else if (c_diaObject is GizmoDestructible)
                                c_ObjectType = GObjectType.Destructible;
                            else if (hashSNOInteractWhitelist.Contains(c_ActorSNO))
                                c_ObjectType = GObjectType.Interactable;
                            else if (c_diaObject.ActorInfo.GizmoType == GizmoType.WeirdGroup57)
                                c_ObjectType = GObjectType.Interactable;
                            else
                                c_ObjectType = GObjectType.Unknown;
                        }
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
                        // Not allowed to kill monsters due to profile settings
                        if (!CombatTargeting.Instance.AllowedToKillMonsters)
                        {
                            AddToCache = false;
                            c_IgnoreSubStep = "CombatDisabled";
                            break;
                        }
                        else
                        {
                            AddToCache = RefreshGilesUnit(AddToCache);
                        }
                        break;
                    }
                // Handle Item-type Objects
                case GObjectType.Item:
                    {
                        // Not allowed to loot due to profile settings
                        // rrrix disabled this since noobs can't figure out their profile is broken... looting is always enabled now
                        //if (!ProfileManager.CurrentProfile.PickupLoot || !LootTargeting.Instance.AllowedToLoot || LootTargeting.Instance.DisableLooting)
                        //{
                        //    AddToCache = false;
                        //    c_IgnoreSubStep = "LootingDisabled";
                        //    break;
                        //}
                        if (!ForceVendorRunASAP && !TownRun.IsTryingToTownPortal())
                        {
                            AddToCache = RefreshGilesItem();
                            c_IgnoreReason = "RefreshGilesItem";
                        }
                        else
                        {
                            AddToCache = false;
                            c_IgnoreSubStep = "IsTryingToTownPortal";
                        }
                        break;

                    }
                // Handle Gold
                // NOTE: Only identified as gold after *FIRST* loop as an "item" by above code
                case GObjectType.Gold:
                    {
                        // Not allowed to loot due to profile settings
                        if (!ProfileManager.CurrentProfile.PickupLoot || !LootTargeting.Instance.AllowedToLoot || LootTargeting.Instance.DisableLooting)
                        {
                            AddToCache = false;
                            c_IgnoreSubStep = "LootingDisabled";
                            break;
                        }
                        else
                        {
                            AddToCache = RefreshGilesGold(AddToCache);
                            c_IgnoreSubStep = "RefreshGilesGold";
                            break;
                        }
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
                        AddToCache = false;
                        break;
                    }
            }
        }

        private static bool RefreshGilesGizmo(bool AddToCache)
        {
            // start as true, then set as false as we go. If nothing matches below, it will return true.
            AddToCache = true;

            bool openResplendentChests = Zeta.CommonBot.Settings.CharacterSettings.Instance.OpenChests && c_InternalName.ToLower().Contains("chest_rare");

            // Ignore it if it's not in range yet, except health wells and resplendent chests if we're opening chests
            if ((c_RadiusDistance > iCurrentMaxLootRadius || c_RadiusDistance > 50) && c_ObjectType != GObjectType.HealthWell && c_ObjectType != GObjectType.Shrine && c_RActorGuid != CurrentTargetRactorGUID)
            {
                AddToCache = false;
                c_IgnoreSubStep = "NotInRange";
            }

            // re-add resplendent chests
            if (openResplendentChests)
            {
                AddToCache = true;
            }

            if (c_InternalName.ToLower().StartsWith("minimapicon"))
            {
                // Minimap icons caused a few problems in the past, so this force-blacklists them
                hashRGUIDBlacklist60.Add(c_RActorGuid);
                c_IgnoreSubStep = "minimapicon";
                AddToCache = false;
                return AddToCache;
            }
            // Retrieve collision sphere radius, cached if possible
            if (!dictGilesCollisionSphereCache.TryGetValue(c_ActorSNO, out c_Radius))
            {
                try
                {
                    if (!dictSNOExtendedDestructRange.TryGetValue(c_ActorSNO, out c_Radius))
                    {
                        c_Radius = c_diaObject.CollisionSphere.Radius;

                        if (c_ObjectType == GObjectType.Destructible && c_Radius >= 5f)
                        {
                            c_Radius = c_Radius / 2;
                        }
                    }

                    // Minimum range clamp
                    if (c_Radius <= 1f)
                        c_Radius = 1f;
                    // Maximum range clamp
                    if (c_Radius >= 16f)
                        c_Radius = 16f;
                }
                catch
                {
                    DbHelper.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "Safely handled exception getting collisionsphere radius for object {0} [{1}]", c_InternalName, c_ActorSNO);
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
                switch (c_ObjectType)
                {
                    case GObjectType.Shrine:
                    case GObjectType.Door:
                    case GObjectType.Container:
                    case GObjectType.Interactable:
                        bDisabledByScript = ((DiaGizmo)c_diaObject).IsGizmoDisabledByScript;
                        break;
                }
            }
            catch
            {
                DbHelper.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement,
                    "Safely handled exception getting Gizmo-Disabled-By-Script attribute for object {0} [{1}]", c_InternalName, c_ActorSNO);
                AddToCache = false;
            }
            if (bDisabledByScript)
            {
                AddToCache = false;
                c_IgnoreSubStep = "GizmoDisabledByScript";
                return AddToCache;
            }
            // Now for the specifics
            int iThisPhysicsSNO;
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
                            if (currentAnimation.Contains("irongate") && currentAnimation.Contains("open"))
                                GizmoUsed = false;
                            if (currentAnimation.Contains("irongate") && currentAnimation.Contains("idle"))
                                GizmoUsed = true;
                        }
                        catch { }
                        if (GizmoUsed)
                        {
                            hashRGUIDBlacklist3.Add(c_RActorGuid);
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
                                        hashRGUIDBlacklist3.Add(c_RActorGuid);
                                        AddToCache = false;
                                        c_IgnoreSubStep = "DoorDisabledbyScript";
                                        return AddToCache;
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
                        return AddToCache;
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
                            DbHelper.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "Safely handled exception getting shrine-been-operated attribute for object {0} [{1}]", c_InternalName, c_ActorSNO);
                            AddToCache = true;
                            //return bWantThis;
                        }
                        if (GizmoUsed)
                        {
                            c_IgnoreSubStep = "GizmoCharges";
                            AddToCache = false;
                            return AddToCache;
                        }
                        IsWaitingAfterPower = true;
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
                            c_IgnoreSubStep = "IgnoreAllShrinesSet";
                            AddToCache = false;
                            return AddToCache;
                        }

                        // Determine what shrine type it is, and blacklist if the user has disabled it
                        switch (c_ActorSNO)
                        {
                            case 176077:  //Frenzy Shrine
                                if (!Settings.WorldObject.UseFrenzyShrine)
                                {
                                    hashRGUIDBlacklist60.Add(c_RActorGuid);
                                    AddToCache = false;
                                }
                                if (PlayerStatus.ActorClass == ActorClass.Monk && Settings.Combat.Monk.TROption.HasFlag(TempestRushOption.MovementOnly) && Hotbar.Contains(SNOPower.Monk_TempestRush))
                                {
                                    // Frenzy shrines are a huge time sink for monks using Tempest Rush to move, we should ignore them.
                                    AddToCache = false;
                                    c_IgnoreSubStep = "IgnoreShrineOption";
                                    return AddToCache;
                                }
                                break;

                            case 176076:  //Fortune Shrine
                                if (!Settings.WorldObject.UseFortuneShrine)
                                {
                                    AddToCache = false;
                                    c_IgnoreSubStep = "IgnoreShrineOption";
                                    return AddToCache;
                                }
                                break;

                            case 176074:  //Protection Shrine
                                if (!Settings.WorldObject.UseProtectionShrine)
                                {
                                    AddToCache = false;
                                    c_IgnoreSubStep = "IgnoreShrineOption";
                                    return AddToCache;
                                }
                                break;

                            case 260330:  //Empowered Shrine
                                if (!Settings.WorldObject.UseEmpoweredShrine)
                                {
                                    AddToCache = false;
                                    c_IgnoreSubStep = "IgnoreShrineOption";
                                    return AddToCache;
                                }
                                break;

                            case 176075:  //Enlightened Shrine
                                if (!Settings.WorldObject.UseEnlightenedShrine)
                                {
                                    AddToCache = false;
                                    c_IgnoreSubStep = "IgnoreShrineOption";
                                    return AddToCache;
                                }
                                break;

                            case 260331:  //Fleeting Shrine
                                if (!Settings.WorldObject.UseFleetingShrine)
                                {
                                    AddToCache = false;
                                    c_IgnoreSubStep = "IgnoreShrineOption";
                                    return AddToCache;
                                }
                                break;

                            default:
                                break;
                        }  //end switch

                        // Already used, blacklist it and don't look at it again
                        try
                        {
                            GizmoUsed = (c_CommonData.GetAttribute<int>(ActorAttributeType.GizmoHasBeenOperated) > 0);
                        }
                        catch
                        {
                            DbHelper.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "Safely handled exception getting shrine-been-operated attribute for object {0} [{1}]", c_InternalName, c_ActorSNO);
                            AddToCache = false;
                            //return bWantThis;
                        }
                        if (GizmoUsed)
                        {
                            // It's already open!
                            c_IgnoreSubStep = "GizmoHasBeenOperated";
                            AddToCache = false;
                            return AddToCache;
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
                                DbHelper.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "Safely handled exception getting physics SNO for object {0} [{1}]", c_InternalName, c_ActorSNO);
                                AddToCache = false;
                            }
                            dictPhysicsSNO.Add(c_ActorSNO, iThisPhysicsSNO);
                        }

                        // Set min distance to user-defined setting
                        iMinDistance = Settings.WorldObject.DestructibleRange + c_Radius;
                        if (ForceCloseRangeTarget)
                            iMinDistance += 6f;

                        // Large objects, like logs - Give an extra xx feet of distance
                        //if (dictSNOExtendedDestructRange.TryGetValue(c_ActorSNO, out iExtendedRange))
                        //    iMinDistance = Settings.WorldObject.DestructibleRange + iExtendedRange;

                        // This object isn't yet in our destructible desire range
                        if (iMinDistance <= 0 || c_RadiusDistance > iMinDistance)
                        {
                            c_IgnoreSubStep = "NotInBarricadeRange";
                            AddToCache = false;
                            return AddToCache;
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
                                DbHelper.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "Safely handled exception getting physics SNO for object {0} [{1}]", c_InternalName, c_ActorSNO);
                                AddToCache = false;
                                //return bWantThis;
                            }
                            dictPhysicsSNO.Add(c_ActorSNO, iThisPhysicsSNO);
                        }

                        // Set min distance to user-defined setting
                        iMinDistance = Settings.WorldObject.DestructibleRange;
                        if (ForceCloseRangeTarget)
                            iMinDistance += 6f;

                        // Large objects, like logs - Give an extra xx feet of distance
                        //if (dictSNOExtendedDestructRange.TryGetValue(c_ActorSNO, out iExtendedRange))
                        //    iMinDistance = Settings.WorldObject.DestructibleRange + iExtendedRange;

                        if (Settings.WorldObject.IgnoreNonBlocking)
                        {
                            AddToCache = false;
                            c_IgnoreSubStep = "IgnoringDestructables";
                        }

                        // Only break destructables if we're stuck and using IgnoreNonBlocking
                        if (PlayerMover.GetMovementSpeed() > 1 && !AddToCache && Settings.WorldObject.IgnoreNonBlocking)
                        {
                            AddToCache = false;
                            c_IgnoreSubStep = "NotStuck";
                        }
                        else
                        {
                            iMinDistance += 12f;
                            AddToCache = true;
                        }

                        // This object isn't yet in our destructible desire range
                        if (iMinDistance <= 0 || c_RadiusDistance > iMinDistance)
                        {
                            AddToCache = false;
                            c_IgnoreSubStep = "NotInDestructableRange";
                        }

                        if (c_RActorGuid == CurrentTargetRactorGUID)
                        {
                            AddToCache = true;
                            c_IgnoreSubStep = "";
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
                            DbHelper.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "Safely handled exception getting container-been-opened attribute for object {0} [{1}]", c_InternalName, c_ActorSNO);
                            AddToCache = false;
                        }
                        if (bThisOpen)
                        {
                            // It's already open!
                            AddToCache = false;
                            return AddToCache;
                        }
                        else if (!bThisOpen && c_InternalName.ToLower().Contains("chest") && !c_InternalName.ToLower().Contains("chest_rare"))
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
                                DbHelper.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "Safely handled exception getting physics SNO for object {0} [{1}]", c_InternalName, c_ActorSNO);
                                AddToCache = false;
                            }
                            dictPhysicsSNO.Add(c_ActorSNO, iThisPhysicsSNO);
                        }
                        // Any physics mesh? Give a minimum distance of 5 feet
                        if (c_InternalName.ToLower().Contains("corpse") && Settings.WorldObject.IgnoreNonBlocking)
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
                        else if (c_InternalName.ToLower().Contains("chest") && !c_InternalName.ToLower().Contains("chest_rare"))
                        {
                            DbHelper.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "GSDebug: Possible Chest SNO: {0}, SNO={1}", c_InternalName, c_ActorSNO);
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
                        else if (c_InternalName.Contains("chest_rare"))
                        {
                            DbHelper.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "GSDebug: Possible Resplendant Chest SNO: {0}, SNO={1}", c_InternalName, c_ActorSNO);
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
            // Retrieve collision sphere radius, cached if possible
            if (!dictGilesCollisionSphereCache.TryGetValue(c_ActorSNO, out c_Radius))
            {
                try
                {
                    c_Radius = c_diaObject.CollisionSphere.Radius;
                    if (c_Radius <= 5f)
                        c_Radius = 5f;
                }
                catch (Exception ex)
                {
                    DbHelper.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "Safely handled exception getting collisionsphere radius for Avoidance {0} [{1}]", c_InternalName, c_ActorSNO);
                    DbHelper.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "{0}", ex);
                    AddToCache = false;
                    return AddToCache;
                }
                dictGilesCollisionSphereCache.Add(c_ActorSNO, c_Radius);
            }

            try
            {
                c_CurrentAnimation = c_CommonData.CurrentAnimation;
            }
            catch { }

            bool ignoreAvoidance = false;
            double minAvoidanceHealth = GetAvoidanceHealth(c_ActorSNO);
            double minAvoidanceRadius = GetAvoidanceRadius(c_ActorSNO, c_Radius);

            // Monks with Serenity up ignore all AOE's
            if (PlayerStatus.ActorClass == ActorClass.Monk && Hotbar.Contains(SNOPower.Monk_Serenity) && GetHasBuff(SNOPower.Monk_Serenity))
            {
                // Monks with serenity are immune
                ignoreAvoidance = true;
                DbHelper.Log(TrinityLogLevel.Debug, LogCategory.Avoidance, "Ignoring avoidance as a Monk with Serenity");
            }
            // Witch doctors with spirit walk available and not currently Spirit Walking will subtly ignore ice balls, arcane, desecrator & plague cloud
            if (PlayerStatus.ActorClass == ActorClass.WitchDoctor && Hotbar.Contains(SNOPower.Witchdoctor_SpiritWalk) &&
                (!GetHasBuff(SNOPower.Witchdoctor_SpiritWalk) && GilesUseTimer(SNOPower.Witchdoctor_SpiritWalk)) || GetHasBuff(SNOPower.Witchdoctor_SpiritWalk))
            {
                if (c_ActorSNO == 223675 || c_ActorSNO == 402 || c_ActorSNO == 219702 || c_ActorSNO == 221225 || c_ActorSNO == 84608 || c_ActorSNO == 108869)
                {
                    // Ignore ICE/Arcane/Desc/PlagueCloud altogether with spirit walk up or available
                    ignoreAvoidance = true;
                    DbHelper.Log(TrinityLogLevel.Debug, LogCategory.Avoidance, "Ignoring avoidance as a WitchDoctor with Spirit Walk");
                }
            }
            // Remove ice balls if the barbarian has wrath of the berserker up, and reduce health from most other SNO avoidances
            if (PlayerStatus.ActorClass == ActorClass.Barbarian && Hotbar.Contains(SNOPower.Barbarian_WrathOfTheBerserker) && GetHasBuff(SNOPower.Barbarian_WrathOfTheBerserker))
            {
                if (c_ActorSNO == 223675 || c_ActorSNO == 402)
                {
                    // Ignore ice-balls altogether with wrath up
                    ignoreAvoidance = true;
                    DbHelper.Log(TrinityLogLevel.Debug, LogCategory.Avoidance, "Ignoring avoidance as a Barbarian with WOTB");
                }
                else
                {
                    // Use half-health for anything else except arcanes or desecrate with wrath up
                    if (c_ActorSNO == 219702 || c_ActorSNO == 221225)
                    {
                        // Arcane
                        ignoreAvoidance = true;
                        DbHelper.Log(TrinityLogLevel.Debug, LogCategory.Avoidance, "Ignoring avoidance as a Barbarian with WOTB");
                    }
                    else if (c_ActorSNO == 84608)
                        // Desecrator
                        minAvoidanceHealth *= 0.2;
                    else
                        // Anything else
                        minAvoidanceHealth *= 0.3;
                }
            }
            if (ignoreAvoidance)
            {
                AddToCache = false;
                DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.Avoidance, "Ignoring Avoidance! Name={0} SNO={1} radius={2:0} health={3:0.00} dist={4:0}",
                       c_InternalName, c_ActorSNO, minAvoidanceRadius, minAvoidanceHealth, c_CentreDistance);
                return AddToCache;

            }

            // Add it to the list of known avoidance objects, *IF* our health is lower than this avoidance health limit
            if (minAvoidanceHealth >= PlayerStatus.CurrentHealthPct)
            {
                // Generate a "weight" for how badly we want to avoid this obstacle, based on a percentage of 100% the avoidance health is, multiplied into a max of 200 weight
                double dThisWeight = (200 * minAvoidanceHealth);

                hashAvoidanceObstacleCache.Add(new GilesObstacle(c_Position, (float)GetAvoidanceRadius(), c_ActorSNO, dThisWeight, c_InternalName));

                // Is this one under our feet? If so flag it up so we can find an avoidance spot
                if (c_CentreDistance <= minAvoidanceRadius)
                {
                    StandingInAvoidance = true;

                    // Note if this is a travelling projectile or not so we can constantly update our safe points
                    if (hashAvoidanceSNOProjectiles.Contains(c_ActorSNO))
                    {
                        IsAvoidingProjectiles = true;
                        DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.Avoidance, "Is standing in avoidance for projectile Name={0} SNO={1} radius={2:0} health={3:0.00} dist={4:0}",
                           c_InternalName, c_ActorSNO, minAvoidanceRadius, minAvoidanceHealth, c_CentreDistance);
                    }
                    else
                    {
                        DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.Avoidance, "Is standing in avoidance Name={0} SNO={1} radius={2:0} health={3:0.00} dist={4:0}",
                       c_InternalName, c_ActorSNO, minAvoidanceRadius, minAvoidanceHealth, c_CentreDistance);
                    }
                }
                else
                {
                    DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.Avoidance, "NOT standing in Avoidance Name={0} SNO={1} radius={2:0} health={3:0.00} dist={4:0}",
                   c_InternalName, c_ActorSNO, minAvoidanceRadius, minAvoidanceHealth, c_CentreDistance);
                }
            }
            else
            {
                DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.Avoidance, "Enough health for avoidance, ignoring Name={0} SNO={1} radius={2:0} health={3:0.00} dist={4:0}",
               c_InternalName, c_ActorSNO, minAvoidanceRadius, minAvoidanceHealth, c_CentreDistance);
            }

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
                // Everything except items and the current target
                if (c_ObjectType != GObjectType.Item && c_RActorGuid != CurrentTargetRactorGUID && c_ObjectType != GObjectType.Unknown)
                {
                    if (c_CentreDistance < 125)
                    {
                        switch (c_ObjectType)
                        {
                            case GObjectType.Destructible:
                            case GObjectType.Unit:
                            case GObjectType.Shrine:
                            case GObjectType.Barricade:
                            case GObjectType.Gold:
                                {
                                    using (new PerformanceLogger("RefreshLoS.1"))
                                    {
                                        // Get whether or not this RActor has ever been navigable. If it hasn't, don't add to cache and keep rechecking
                                        if (!dictHasBeenNavigableCache.TryGetValue(c_RActorGuid, out c_HasBeenNavigable))
                                        {
                                            if (Settings.Combat.Misc.UseNavMeshTargeting)
                                            {
                                                bool isNavigable = gp.CanStandAt(gp.WorldToGrid(c_Position.ToVector2()));

                                                if (!isNavigable)
                                                {
                                                    AddToCache = false;
                                                    c_IgnoreSubStep = "NotNavigable";
                                                }
                                                else
                                                {
                                                    c_HasBeenNavigable = true;
                                                    dictHasBeenNavigableCache.Add(c_RActorGuid, c_HasBeenNavigable);
                                                }
                                            }
                                            else
                                            {
                                                c_HasBeenNavigable = true;
                                                dictHasBeenNavigableCache.Add(c_RActorGuid, c_HasBeenNavigable);
                                            }
                                        }
                                    }
                                }
                                break;

                        }
                        switch (c_ObjectType)
                        {
                            case GObjectType.Destructible:
                            case GObjectType.Unit:
                            case GObjectType.Shrine:
                            case GObjectType.Gold:
                                {
                                    using (new PerformanceLogger("RefreshLoS.2"))
                                    {
                                        // Get whether or not this RActor has ever been in a path line with AllowWalk. If it hasn't, don't add to cache and keep rechecking
                                        if (!dictHasBeenRayCastedCache.TryGetValue(c_RActorGuid, out c_HasBeenRaycastable))
                                        {
                                            if (c_RadiusDistance <= 12f)
                                            {
                                                c_HasBeenRaycastable = true;
                                                dictHasBeenRayCastedCache.Add(c_RActorGuid, c_HasBeenRaycastable);
                                            }
                                            else if (Settings.Combat.Misc.UseNavMeshTargeting)
                                            {
                                                Vector3 myPos = new Vector3(PlayerStatus.CurrentPosition.X, PlayerStatus.CurrentPosition.Y, PlayerStatus.CurrentPosition.Z + 8f);
                                                Vector3 cPos = new Vector3(c_Position.X, c_Position.Y, c_Position.Z + 8f);

                                                cPos = MathEx.CalculatePointFrom(myPos, cPos, c_CentreDistance - PlayerStatus.GoldPickupRadius);

                                                if (Navigator.Raycast(myPos, cPos))
                                                {
                                                    AddToCache = false;
                                                    c_IgnoreSubStep = "UnableToRayCast";
                                                }
                                                else
                                                {
                                                    c_HasBeenRaycastable = true;
                                                    dictHasBeenRayCastedCache.Add(c_RActorGuid, c_HasBeenRaycastable);
                                                }

                                            }
                                            else
                                            {
                                                if (c_ZDiff > 14f)
                                                {
                                                    AddToCache = false;
                                                    c_IgnoreSubStep = "LoS.ZDiff";
                                                }
                                                else
                                                {
                                                    c_HasBeenRaycastable = true;
                                                    dictHasBeenRayCastedCache.Add(c_RActorGuid, c_HasBeenRaycastable);
                                                }

                                            }
                                        }
                                    }
                                }
                                break;

                        }
                        switch (c_ObjectType)
                        {
                            case GObjectType.Unit:
                                {
                                    using (new PerformanceLogger("RefreshLoS.3"))
                                    {
                                        // Get whether or not this RActor has ever been in "Line of Sight" (as determined by Demonbuddy). If it hasn't, don't add to cache and keep rechecking
                                        if (!dictHasBeenInLoSCache.TryGetValue(c_RActorGuid, out c_HasBeenInLoS))
                                        {
                                            if (Settings.Combat.Misc.UseNavMeshTargeting)
                                            {
                                                // Ignore units not in LoS except bosses, rares, champs
                                                if (!(c_unit_IsBoss) && !c_diaObject.InLineOfSight)
                                                {
                                                    AddToCache = false;
                                                    c_IgnoreSubStep = "NotInLoS";
                                                }
                                                else
                                                {
                                                    c_HasBeenInLoS = true;
                                                    dictHasBeenInLoSCache.Add(c_RActorGuid, c_HasBeenInLoS);
                                                }
                                            }
                                            else
                                            {
                                                c_HasBeenInLoS = true;
                                                dictHasBeenInLoSCache.Add(c_RActorGuid, c_HasBeenInLoS);
                                            }
                                        }
                                    }
                                    break;
                                }
                        }
                    }
                    else
                    {
                        AddToCache = false;
                        c_IgnoreSubStep = "LoS-OutOfRange";
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

                        if (Settings.Combat.Misc.UseNavMeshTargeting)
                        {
                            // always get Height of wherever the nav says it is (for flying things..)
                            c_Position = new Vector3(pos.X, pos.Y, gp.GetHeight(pos.ToVector2()));
                        }
                        else
                            c_Position = pos;

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
                        DbHelper.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "Safely handled exception getting DynamicID for item {0} [{1}]", c_InternalName, c_ActorSNO);
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
                        DbHelper.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "Safely handled exception getting GameBalanceID for item {0} [{1}]", c_InternalName, c_ActorSNO);
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
            // always take current target regardless if ZDiff changed
            if (c_RActorGuid == CurrentTargetRactorGUID)
            {
                AddToCache = true;
                return AddToCache;
            }

            // Special whitelist for always getting stuff regardless of ZDiff or LoS
            if (LineOfSightWhitelist.Contains(c_ActorSNO))
            {
                AddToCache = true;
                return AddToCache;
            }
            // Ignore stuff which has a Z-height-difference too great, it's probably on a different level etc. - though not avoidance!
            if (c_ObjectType != GObjectType.Avoidance)
            {
                // Calculate the z-height difference between our current position, and this object's position
                c_ZDiff = Math.Abs(PlayerStatus.CurrentPosition.Z - c_Position.Z);
                switch (c_ObjectType)
                {
                    case GObjectType.Door:
                    case GObjectType.Unit:
                    case GObjectType.Barricade:
                        // Ignore monsters (units) who's Z-height is 14 foot or more than our own z-height except bosses
                        if (c_ZDiff >= 14f && !c_unit_IsBoss)
                        {
                            AddToCache = false;
                        }
                        break;
                    case GObjectType.Item:
                    case GObjectType.HealthWell:
                        // Items at 26+ z-height difference (we don't want to risk missing items so much)
                        if (c_ZDiff >= 26f)
                        {
                            AddToCache = false;
                        }
                        break;
                    case GObjectType.Gold:
                    case GObjectType.Globe:
                        // Gold/Globes at 11+ z-height difference
                        if (c_ZDiff >= 11f)
                        {
                            AddToCache = false;
                        }
                        break;
                    case GObjectType.Destructible:
                    case GObjectType.Shrine:
                    case GObjectType.Container:
                        // Destructibles, shrines and containers are the least important, so a z-height change of only 7 is enough to ignore (help avoid stucks at stairs etc.)
                        if (c_ZDiff >= 7f)
                        {
                            AddToCache = false;
                        }
                        break;
                    case GObjectType.Interactable:
                        // Special interactable objects
                        if (c_ZDiff >= 9f)
                        {
                            AddToCache = false;
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
            if (c_diaUnit != null)
            {
                // Count up Mystic Allys, gargantuans, and zombies - if the player has those skills
                if (PlayerStatus.ActorClass == ActorClass.Monk)
                {
                    if (Hotbar.Contains(SNOPower.Monk_MysticAlly) && hashMysticAlly.Contains(c_ActorSNO))
                    {
                        if (c_diaUnit.SummonedByACDId == PlayerStatus.MyDynamicID)
                            iPlayerOwnedMysticAlly++;
                        AddToCache = false;
                    }
                }
                // Count up Demon Hunter pets
                if (PlayerStatus.ActorClass == ActorClass.DemonHunter)
                {
                    if (Hotbar.Contains(SNOPower.DemonHunter_Companion) && hashDHPets.Contains(c_ActorSNO))
                    {
                        if (c_diaUnit.SummonedByACDId == PlayerStatus.MyDynamicID)
                            iPlayerOwnedDHPets++;
                        AddToCache = false;
                    }
                }
                // Count up zombie dogs and gargantuans next
                if (PlayerStatus.ActorClass == ActorClass.WitchDoctor)
                {
                    if (Hotbar.Contains(SNOPower.Witchdoctor_Gargantuan) && hashGargantuan.Contains(c_ActorSNO))
                    {
                        if (c_diaUnit.SummonedByACDId == PlayerStatus.MyDynamicID)
                            iPlayerOwnedGargantuan++;
                        AddToCache = false;
                    }
                    if (Hotbar.Contains(SNOPower.Witchdoctor_SummonZombieDog) && hashZombie.Contains(c_ActorSNO))
                    {
                        if (c_diaUnit.SummonedByACDId == PlayerStatus.MyDynamicID)
                            iPlayerOwnedZombieDog++;
                        AddToCache = false;
                    }
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
                if (c_ObjectHash != String.Empty && GenericBlacklist.ContainsKey(c_ObjectHash))
                {
                    AddToCache = false;
                    c_IgnoreSubStep = "GenericBlacklist";
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
            if (hashMonsterObstacleCache.Any(o => GilesIntersectsPath(o.Location, o.Radius, PlayerStatus.CurrentPosition, c_Position)))
            {
                AddToCache = false;
                c_IgnoreSubStep = "MonsterInPath";
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
        private static double GetAvoidanceRadius(int actorSNO = -1, float radius = -1f)
        {
            if (actorSNO == -1)
                actorSNO = c_ActorSNO;

            if (radius == -1f)
                radius = 20f;

            try
            {

                return AvoidanceManager.GetAvoidanceRadiusBySNO(actorSNO, radius);
            }
            catch (Exception ex)
            {
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.Avoidance, "Exception getting avoidance radius for sno={0} radius={1}", actorSNO, radius);
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.Avoidance, ex.ToString());
                return radius;
            }

        }

        private static double GetAvoidanceHealth(int actorSNO = -1)
        {
            // snag our SNO from cache variable if not provided
            if (actorSNO == -1)
                actorSNO = c_ActorSNO;
            try
            {
                if (actorSNO != -1)
                    return AvoidanceManager.GetAvoidanceHealthBySNO(c_ActorSNO, 1);
                else
                    return AvoidanceManager.GetAvoidanceHealthBySNO(actorSNO, 1);
            }
            catch (Exception ex)
            {
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.Avoidance, "Exception getting avoidance radius for sno={0}", actorSNO);
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.Avoidance, ex.ToString());
                // 100% unless specified
                return 1;
            }
        }
        private static void RefreshCachedHealth(int iLastCheckedHealth, double dThisCurrentHealth, bool bHasCachedHealth)
        {
            if (!bHasCachedHealth)
            {
                dictGilesLastHealthCache.Add(c_RActorGuid, dThisCurrentHealth);
                dictGilesLastHealthChecked.Add(c_RActorGuid, iLastCheckedHealth);
            }
            else
            {
                dictGilesLastHealthCache[c_RActorGuid] = dThisCurrentHealth;
                dictGilesLastHealthChecked[c_RActorGuid] = iLastCheckedHealth;
            }
        }

    }
}
