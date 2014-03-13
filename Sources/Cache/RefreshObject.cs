using System;
using System.Linq;
using Trinity.Technicals;
using Zeta.Bot;
using Zeta.Bot.Navigation;
using Zeta.Common;
using Zeta.Common.Plugins;
using Zeta.Game;
using Zeta.Game.Internals.Actors;
using Zeta.Game.Internals.SNO;
using Logger = Trinity.Technicals.Logger;
namespace Trinity
{
    public partial class Trinity : IPlugin
    {
        /// <summary>
        /// This will eventually be come our single source of truth and we can get rid of most/all of the below "c_" variables
        /// </summary>
        //private static TrinityCacheObject cacheEntry = null;

        private static Vector3 c_Position = Vector3.Zero;
        private static GObjectType c_ObjectType = GObjectType.Unknown;
        /// <summary>
        /// Percent of total health remaining on unit
        /// </summary>
        private static double c_HitPointsPct = 0d;
        private static double c_HitPoints = 0d;
        private static float c_CentreDistance = 0f;
        private static float c_RadiusDistance = 0f;
        private static float c_Radius = 0f;
        private static float c_ZDiff = 0f;
        private static string c_ItemDisplayName = "";
        private static int c_GameBalanceID = 0;
        private static string c_InternalName = "";
        private static string c_IgnoreReason = "";
        private static string c_IgnoreSubStep = "";
        private static int c_ACDGUID = 0;
        private static int c_RActorGuid = 0;
        private static int c_GameDynamicID = 0;
        private static int c_BalanceID = 0;
        private static int c_ActorSNO = 0;
        private static int c_SummonedByACDId = 0;
        private static bool c_IsSummonedByPlayer = false;
        private static int c_ItemLevel = 0;
        private static string c_ItemLink = String.Empty;
        private static int c_GoldStackSize = 0;
        private static bool c_IsOneHandedItem = false;
        private static bool c_IsTwoHandedItem = false;
        private static ItemQuality c_ItemQuality = ItemQuality.Invalid;
        private static int c_ItemQualityLevelIdentified = -1;
        private static ItemType c_DBItemType = ItemType.Unknown;
        private static ItemBaseType c_DBItemBaseType = ItemBaseType.None;
        private static FollowerType c_item_tFollowerType = FollowerType.None;
        private static GItemType c_item_GItemType = GItemType.Unknown;
        private static MonsterSize c_unit_MonsterSize = MonsterSize.Unknown;
        private static DiaObject c_diaObject = null;
        private static DiaUnit c_diaUnit = null;
        private static ACD c_CommonData = null;
        private static SNOAnim c_CurrentAnimation = SNOAnim.Invalid;
        private static bool c_unit_IsElite = false;
        private static bool c_unit_IsRare = false;
        private static bool c_unit_IsUnique = false;
        private static bool c_unit_IsMinion = false;
        private static bool c_unit_IsTreasureGoblin = false;
        private static bool c_IsEliteRareUnique = false;
        private static bool c_unit_IsBoss = false;
        private static bool c_unit_IsAttackable = false;
        private static bool c_unit_HasShieldAffix = false;
        private static bool c_ForceLeapAgainst = false;
        private static bool c_IsObstacle = false;
        private static bool c_HasBeenNavigable = false;
        private static bool c_HasBeenRaycastable = false;
        private static bool c_HasBeenInLoS = false;
        private static string c_ItemMd5Hash = string.Empty;
        private static bool c_HasDotDPS = false;
        private static string c_ObjectHash = String.Empty;
        private static double c_KillRange = 0f;
        private static MonsterAffixes c_MonsterAffixes = MonsterAffixes.None;
        private static bool c_IsFacingPlayer;
        private static float c_Rotation;
        private static bool c_IsSummoner = false;

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

            //AddToCache = RefreshStepSkipDoubleCheckGuid(AddToCache);
            //if (!AddToCache) { c_IgnoreReason = "SkipDoubleCheckGuid"; return AddToCache; }

            // Get Internal Name
            AddToCache = RefreshInternalName(AddToCache);
            if (!AddToCache) { c_IgnoreReason = "InternalName"; return AddToCache; }

            // ActorSNO
            AddToCache = RefreshStepCachedActorSNO(AddToCache);
            if (!AddToCache) { c_IgnoreReason = "CachedActorSNO"; return AddToCache; }

            // Have ActorSNO Check for SNO based navigation obstacle hashlist
            c_IsObstacle = DataDictionary.NavigationObstacleIds.Contains(c_ActorSNO);

            // Get ACDGuid
            AddToCache = RefreshStepCachedACDGuid(AddToCache);
            if (!AddToCache) { c_IgnoreReason = "CachedACDGuid"; return AddToCache; }
            // Summons by the player 
            AddToCache = RefreshStepCachedSummons(AddToCache);
            if (!AddToCache) { c_IgnoreReason = "CachedPlayerSummons"; return AddToCache; }

            using (new PerformanceLogger("RefreshDiaObject.CachedType"))
            {
                /*
                 * Begin main refresh routine
                 */
                // Set Object Type
                AddToCache = RefreshStepCachedObjectType(AddToCache);
                if (!AddToCache) { c_IgnoreReason = "CachedObjectType"; return AddToCache; }
            }

            // Check Blacklists
            AddToCache = RefreshStepCheckBlacklists(AddToCache);
            if (!AddToCache) { c_IgnoreReason = "CheckBlacklists"; return AddToCache; }

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
                c_ObjectHash = HashGenerator.GenerateWorldObjectHash(c_ActorSNO, c_Position, c_ObjectType.ToString(), Trinity.CurrentWorldDynamicId);
            }

            // Always Refresh Distance for every object
            RefreshStepCalculateDistance();

            // Always Refresh ZDiff for every object
            AddToCache = RefreshStepObjectTypeZDiff(AddToCache);
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
                ObjectCache.Add(
                    new TrinityCacheObject(c_diaObject)
                    {
                        Position = c_Position,
                        Type = c_ObjectType,
                        CentreDistance = c_CentreDistance,
                        RadiusDistance = c_RadiusDistance,
                        InternalName = c_InternalName,
                        Animation = c_CurrentAnimation,
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
                        TrinityItemType = c_item_GItemType,
                        IsElite = c_unit_IsElite,
                        IsRare = c_unit_IsRare,
                        IsUnique = c_unit_IsUnique,
                        IsMinion = c_unit_IsMinion,
                        IsTreasureGoblin = c_unit_IsTreasureGoblin,
                        IsBoss = c_unit_IsBoss,
                        IsAttackable = c_unit_IsAttackable,
                        HitPoints = c_HitPoints,
                        HitPointsPct = c_HitPointsPct,
                        Radius = c_Radius,
                        MonsterSize = c_unit_MonsterSize,
                        IsEliteRareUnique = c_IsEliteRareUnique,
                        ForceLeapAgainst = c_ForceLeapAgainst,
                        HasDotDPS = c_HasDotDPS,
                        ObjectHash = c_ObjectHash,
                        KillRange = c_KillRange,
                        HasAffixShielded = c_unit_HasShieldAffix,
                        MonsterAffixes = c_MonsterAffixes,
                        DiaObject = c_diaObject,
                        HasBeenInLoS = c_HasBeenInLoS,
                        HasBeenNavigable = c_HasBeenNavigable,
                        HasBeenRaycastable = c_HasBeenRaycastable,
                        ItemLink = c_ItemLink,
                        Rotation = c_Rotation,
                        IsFacingPlayer = c_IsFacingPlayer,
                        IsSummonedByPlayer = c_IsSummonedByPlayer,
                        IsSummoner = c_IsSummoner
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
                    NavigationObstacleCache.Add(new CacheObstacleObject()
                    {
                        ActorSNO = c_ActorSNO,
                        Radius = c_Radius,
                        Location = c_Position,
                        Name = c_InternalName,
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
                    MonsterObstacleCache.Add(new CacheObstacleObject(c_Position, c_Radius, c_ActorSNO));
                }
                else
                {
                    // Add to the collision-list
                    MonsterObstacleCache.Add(new CacheObstacleObject(c_Position, c_Radius, c_ActorSNO));
                }
            }
        }
        /// <summary>
        /// Initializes variable set for single object refresh
        /// </summary>
        private static void RefreshStepInit(out bool AddTocache)
        {
            AddTocache = true;
            // Start this object as off as unknown type
            c_ObjectType = GObjectType.Unknown;
            // We will set weight up later in RefreshDiaObjects after we process all valid items
            c_Position = Vector3.Zero;
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
            c_IsSummonedByPlayer = false;
            c_SummonedByACDId = -1;
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
            c_unit_HasShieldAffix = false;
            c_IsEliteRareUnique = false;
            c_ForceLeapAgainst = false;
            c_IsObstacle = false;
            c_HasBeenNavigable = false;
            c_HasBeenRaycastable = false;
            c_HasBeenInLoS = false;
            c_ItemMd5Hash = string.Empty;
            c_ItemQuality = ItemQuality.Invalid;
            c_ItemQualityLevelIdentified = -1;
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
            c_MonsterAffixes = MonsterAffixes.None;
            c_IsFacingPlayer = false;
            c_Rotation = 0f;
            c_IsSummonedByPlayer = false;
            c_IsSummoner = false;
        }
        /// <summary>
        /// Inserts the ActorSNO <see cref="actorSNOCache"/> and sets <see cref="c_ActorSNO"/>
        /// </summary>
        /// <param name="AddToCache"></param>
        /// <param name="c_diaObject"></param>
        /// <returns></returns>
        private static bool RefreshStepCachedActorSNO(bool AddToCache)
        {
            // Get the Actor SNO, cached if possible
            if (!CacheData.actorSNOCache.TryGetValue(c_RActorGuid, out c_ActorSNO))
            {
                try
                {
                    c_ActorSNO = c_diaObject.ActorSNO;
                }
                catch (Exception ex)
                {
                    Logger.Log(TrinityLogLevel.Error, LogCategory.CacheManagement, "Safely handled exception getting ActorSNO for an object.");
                    Logger.Log(TrinityLogLevel.Error, LogCategory.CacheManagement, "{0}", ex);
                    AddToCache = false;
                }
                CacheData.actorSNOCache.Add(c_RActorGuid, c_ActorSNO);
            }
            return AddToCache;
        }

        private static bool RefreshInternalName(bool AddToCache)
        {
            // This is "internalname" for items, and just a "generic" name for objects and units - cached if possible
            if (!CacheData.nameCache.TryGetValue(c_RActorGuid, out c_InternalName))
            {
                try
                {
                    c_InternalName = nameNumberTrimRegex.Replace(c_diaObject.Name, "");
                }
                catch (Exception ex)
                {
                    Logger.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "Safely handled exception getting InternalName for an object [{0}]", c_ActorSNO);
                    Logger.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "{0}", ex);
                    AddToCache = false;
                }
                CacheData.nameCache.Add(c_RActorGuid, c_InternalName);
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

            return true;
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
                NavigationObstacleCache.Add(new CacheObstacleObject(c_Position, DataDictionary.ObstacleCustomRadius[c_ActorSNO], c_ActorSNO));
                AddToCache = false;

            }
            return AddToCache;
        }

        private static void RefreshStepCalculateDistance()
        {
            // Calculate distance, don't rely on DB's internal method as this may hit Diablo 3 memory again
            c_CentreDistance = Player.Position.Distance2D(c_Position);

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

            // Check if it's a unit with an animation we should avoid. We need to recheck this every time.
            if (Settings.Combat.Misc.AvoidAOE && DataDictionary.AvoidanceAnimations.Contains(new DoubleInt(c_ActorSNO, (int)c_diaObject.CommonData.CurrentAnimation)))
            {
                // The ActorSNO and Animation match a known pair, avoid this!
                // Example: "Grotesque" death animation
                AddToCache = true;
                c_ObjectType = GObjectType.Avoidance;
            }

            // Either get the cached object type, or calculate it fresh
            else if (!c_IsObstacle && !CacheData.objectTypeCache.TryGetValue(c_RActorGuid, out c_ObjectType))
            {
                // See if it's an avoidance first from the SNO
                bool isAvoidanceSNO = (DataDictionary.Avoidances.Contains(c_ActorSNO) || DataDictionary.AvoidanceBuffs.Contains(c_ActorSNO) || DataDictionary.AvoidanceProjectiles.Contains(c_ActorSNO));

                // We're avoiding AoE and this is an AoE
                if (Settings.Combat.Misc.AvoidAOE && isAvoidanceSNO)
                {
                    using (new PerformanceLogger("RefreshCachedType.0"))
                    {
                        // Checking for BuffVisualEffect - for Butcher, maybe useful other places?
                        if (DataDictionary.AvoidanceBuffs.Contains(c_ActorSNO))
                        {
                            bool hasBuff = false;
                            try
                            {
                                hasBuff = c_CommonData.GetAttribute<int>(ActorAttributeType.BuffVisualEffect) > 0;
                            }
                            catch
                            {
                                // Remove on exception, otherwise it may get stuck in the cache
                                CacheData.objectTypeCache.Remove(c_RActorGuid);
                            }
                            if (hasBuff)
                            {
                                AddToCache = true;
                                c_ObjectType = GObjectType.Avoidance;
                            }
                            else
                            {
                                CacheData.objectTypeCache.Remove(c_RActorGuid);
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
                else if (!Settings.Combat.Misc.AvoidAOE && isAvoidanceSNO)
                {
                    AddToCache = false;
                    c_IgnoreSubStep = "IgnoreAvoidance";
                }
                // It's not an avoidance, so let's calculate it's object type "properly"
                else
                {
                    // Calculate the object type of this object
                    if (c_diaObject.ActorType == ActorType.Monster)
                    //if (c_diaObject is DiaUnit)
                    {
                        using (new PerformanceLogger("RefreshCachedType.1"))
                        {
                            if (c_CommonData == null)
                            {
                                c_IgnoreSubStep = "InvalidUnitCommonData";
                                AddToCache = false;
                            }
                            else if (c_diaObject.ACDGuid != c_CommonData.ACDGuid)
                            {
                                c_IgnoreSubStep = "InvalidUnitACDGuid";
                                AddToCache = false;
                            }
                            else
                            {
                                c_ObjectType = GObjectType.Unit;
                            }
                        }
                    }
                    else if (c_diaObject.ActorType == ActorType.Player)
                    {
                        c_ObjectType = GObjectType.Player;
                    }
                    else if (DataDictionary.ForceToItemOverrideIds.Contains(c_ActorSNO) || (c_diaObject.ActorType == ActorType.Item))
                    {
                        using (new PerformanceLogger("RefreshCachedType.2"))
                        {
                            c_ObjectType = GObjectType.Item;

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
                        }
                    }
                    else if (DataDictionary.InteractWhiteListIds.Contains(c_ActorSNO))
                        c_ObjectType = GObjectType.Interactable;

                    else if (c_diaObject is DiaGizmo && c_diaObject.ActorType == ActorType.Gizmo)
                    {
                        DiaGizmo c_diaGizmo;
                        c_diaGizmo = (DiaGizmo)c_diaObject;

                        if (c_diaGizmo.IsBarricade)
                            c_ObjectType = GObjectType.Barricade;

                        else
                        {
                            switch (c_diaGizmo.ActorInfo.GizmoType)
                            {
                                case GizmoType.HealingWell:
                                    c_ObjectType = GObjectType.HealthWell;
                                    break;
                                case GizmoType.Door:
                                    c_ObjectType = GObjectType.Door;
                                    break;
                                case GizmoType.PoolOfReflection:
                                case GizmoType.PowerUp:
                                    c_ObjectType = GObjectType.Shrine;
                                    break;
                                case GizmoType.Chest:
                                    c_ObjectType = GObjectType.Container;
                                    break;
                                case GizmoType.BreakableDoor:
                                case GizmoType.BreakableChest:
                                case GizmoType.DestroyableObject:
                                    //case GizmoType.DestroySelfWhenNear:
                                    c_ObjectType = GObjectType.Destructible;
                                    break;
                                case GizmoType.PlacedLoot:
                                case GizmoType.Switch:
                                case GizmoType.Headstone:
                                    c_ObjectType = GObjectType.Interactable;
                                    break;
                                default:
                                    c_ObjectType = GObjectType.Unknown;
                                    break;
                            }
                        }
                    }
                    else
                        c_ObjectType = GObjectType.Unknown;
                }
                if (c_ObjectType != GObjectType.Unknown)
                {  // Now cache the object type
                    CacheData.objectTypeCache.Add(c_RActorGuid, c_ObjectType);
                }
            }
            return AddToCache;
        }

        private static void RefreshStepMainObjectType(ref bool AddToCache)
        {
            // Now do stuff specific to object types
            switch (c_ObjectType)
            {
                case GObjectType.Player:
                    {
                        AddToCache = RefreshUnit(AddToCache);
                        break;
                    }
                // Handle Unit-type Objects
                case GObjectType.Unit:
                    {
                        // Not allowed to kill monsters due to profile settings
                        if (!Combat.Abilities.CombatBase.IsCombatAllowed)
                        {
                            AddToCache = false;
                            c_IgnoreSubStep = "CombatDisabled";
                            break;
                        }
                        else
                        {
                            AddToCache = RefreshUnit(AddToCache);
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

                        if (c_ObjectType != GObjectType.HealthGlobe && (ForceVendorRunASAP || TownRun.TownRunTimerRunning()))
                        {
                            AddToCache = false;
                            c_IgnoreSubStep = "IsTryingToTownPortal";
                        }
                        else
                        {
                            AddToCache = RefreshItem();
                            c_IgnoreReason = "RefreshItem";
                        }

                        break;

                    }
                // Handle Gold
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
                            AddToCache = RefreshGold(AddToCache);
                            c_IgnoreSubStep = "RefreshGold";
                            break;
                        }
                    }
                case GObjectType.PowerGlobe:
                case GObjectType.HealthGlobe:
                    {
                        // Ignore it if it's not in range yet
                        if (c_CentreDistance > CurrentBotLootRange || c_CentreDistance > 60f)
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
                        AddToCache = RefreshAvoidance(AddToCache);
                        if (!AddToCache) { c_IgnoreSubStep = "RefreshAvoidance"; }

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
                        //if (!(c_diaObject is DiaGizmo))
                        //{
                        //    string debugInfo = string.Format("Type: {0} Name: {1} ActorType: {2} SNO: {3} ObjectType: {4}",
                        //        c_diaObject.GetType().Name,
                        //        c_diaObject.Name,
                        //        c_diaObject.ActorType,
                        //        c_diaObject.ActorSNO,
                        //        c_ObjectType);

                        //    Logger.LogDebug("Attempted to Refresh Gizmo on Object that is not a Gizmo! " + debugInfo);
                        //    c_IgnoreSubStep = "InvalidGizmoCast";
                        //    AddToCache = false;
                        //    break;
                        //}
                        //else
                        //{
                        AddToCache = RefreshGizmo(AddToCache);
                        break;
                        //}
                    }
                // Object switch on type (to seperate shrines, destructibles, barricades etc.)
                case GObjectType.Unknown:
                default:
                    {
                        c_IgnoreSubStep = "Unknown." + c_diaObject.ActorType.ToString();
                        AddToCache = false;
                        break;
                    }
            }
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
                if (c_ObjectType != GObjectType.Item && c_RActorGuid != LastTargetRactorGUID && c_ObjectType != GObjectType.Unknown)
                {
                    if (c_CentreDistance < 125)
                    {
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
                                        if (!CacheData.hasBeenRayCastedCache.TryGetValue(c_RActorGuid, out c_HasBeenRaycastable))
                                        {
                                            if (c_CentreDistance <= 5f)
                                            {
                                                c_HasBeenRaycastable = true;
                                                CacheData.hasBeenRayCastedCache.Add(c_RActorGuid, c_HasBeenRaycastable);
                                            }
                                            else if (Settings.Combat.Misc.UseNavMeshTargeting)
                                            {
                                                Vector3 myPos = new Vector3(Player.Position.X, Player.Position.Y, Player.Position.Z + 8f);
                                                Vector3 cPos = new Vector3(c_Position.X, c_Position.Y, c_Position.Z + 8f);
                                                cPos = MathEx.CalculatePointFrom(cPos, myPos, c_Radius + 1f);

                                                if (Navigator.Raycast(myPos, cPos))
                                                {
                                                    AddToCache = false;
                                                    c_IgnoreSubStep = "UnableToRayCast";
                                                }
                                                else
                                                {
                                                    c_HasBeenRaycastable = true;
                                                    CacheData.hasBeenRayCastedCache.Add(c_RActorGuid, c_HasBeenRaycastable);
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
                                                    CacheData.hasBeenRayCastedCache.Add(c_RActorGuid, c_HasBeenRaycastable);
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
                                        if (!CacheData.hasBeenInLoSCache.TryGetValue(c_RActorGuid, out c_HasBeenInLoS))
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
                                                    CacheData.hasBeenInLoSCache.Add(c_RActorGuid, c_HasBeenInLoS);
                                                }
                                            }
                                            else
                                            {
                                                c_HasBeenInLoS = true;
                                                CacheData.hasBeenInLoSCache.Add(c_RActorGuid, c_HasBeenInLoS);
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
                    if (c_RActorGuid == LastTargetRactorGUID)
                    {
                        AddToCache = true;
                        c_IgnoreSubStep = "";
                    }
                }

                // Simple whitelist for LoS 
                if (DataDictionary.LineOfSightWhitelist.Contains(c_ActorSNO))
                {
                    AddToCache = true;
                    c_IgnoreSubStep = "";
                }
                // Always pickup Infernal Keys whether or not in LoS
                if (DataDictionary.ForceToItemOverrideIds.Contains(c_ActorSNO))
                {
                    AddToCache = true;
                    c_IgnoreSubStep = "";
                }

            }
            catch (Exception ex)
            {
                AddToCache = true;
                c_IgnoreSubStep = "IgnoreLoSException";
                Logger.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "{0}", ex);
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
                if (!CacheData.ACDGUIDCache.TryGetValue(c_RActorGuid, out c_ACDGUID))
                {
                    try
                    {
                        c_ACDGUID = c_diaObject.ACDGuid;
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "Safely handled exception getting ACDGUID for an object [{0}]", c_ActorSNO);
                        Logger.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "{0}", ex);
                        AddToCache = false;
                    }
                    CacheData.ACDGUIDCache.Add(c_RActorGuid, c_ACDGUID);
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
            if (c_ObjectType != GObjectType.Avoidance && c_ObjectType != GObjectType.Unit && c_ObjectType != GObjectType.Player)
            {
                // Get the position, cached if possible
                if (!CacheData.positionCache.TryGetValue(c_RActorGuid, out c_Position))
                {
                    try
                    {
                        //c_vPosition = thisobj.Position;
                        Vector3 pos = c_diaObject.Position;

                        if (Settings.Combat.Misc.UseNavMeshTargeting)
                        {
                            // always get Height of wherever the nav says it is (for flying things..)
                            c_Position = new Vector3(pos.X, pos.Y, MainGridProvider.GetHeight(pos.ToVector2()));
                        }
                        else
                            c_Position = pos;

                    }
                    catch (Exception ex)
                    {
                        Logger.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "Safely handled exception getting position for a static object [{0}]", c_ActorSNO);
                        Logger.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "{0}", ex);
                        AddToCache = false;
                    }
                    // Now cache it
                    CacheData.positionCache.Add(c_RActorGuid, c_Position);
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
                    Logger.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "Safely handled exception getting position for a unit or avoidance object [{0}]", c_ActorSNO);
                    Logger.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "{0}", ex);
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
                if (!CacheData.dynamicIDCache.TryGetValue(c_RActorGuid, out c_GameDynamicID))
                {
                    try
                    {
                        c_GameDynamicID = c_CommonData.DynamicId;
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "Safely handled exception getting DynamicID for item {0} [{1}]", c_InternalName, c_ActorSNO);
                        Logger.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "{0}", ex);
                        AddToCache = false;
                        //return bWantThis;
                    }
                    CacheData.dynamicIDCache.Add(c_RActorGuid, c_GameDynamicID);
                }
                // Get the Game Balance ID, cached if possible
                if (!CacheData.gameBalanceIDCache.TryGetValue(c_RActorGuid, out c_BalanceID))
                {
                    try
                    {
                        c_BalanceID = c_CommonData.GameBalanceId;
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "Safely handled exception getting GameBalanceID for item {0} [{1}]", c_InternalName, c_ActorSNO);
                        Logger.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "{0}", ex);
                        AddToCache = false;
                        //return bWantThis;
                    }
                    CacheData.gameBalanceIDCache.Add(c_RActorGuid, c_BalanceID);
                }
            }
            else
            {
                c_GameDynamicID = -1;
                c_BalanceID = -1;
            }
            return AddToCache;
        }

        private static bool RefreshStepObjectTypeZDiff(bool AddToCache)
        {
            c_ZDiff = c_diaObject.ZDiff;
            // always take current target regardless if ZDiff changed
            if (c_RActorGuid == LastTargetRactorGUID)
            {
                AddToCache = true;
                return AddToCache;
            }

            // Special whitelist for always getting stuff regardless of ZDiff or LoS
            if (DataDictionary.LineOfSightWhitelist.Contains(c_ActorSNO))
            {
                AddToCache = true;
                return AddToCache;
            }
            // Ignore stuff which has a Z-height-difference too great, it's probably on a different level etc. - though not avoidance!
            if (c_ObjectType != GObjectType.Avoidance)
            {
                // Calculate the z-height difference between our current position, and this object's position
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
                    case GObjectType.HealthGlobe:
                    case GObjectType.PowerGlobe:
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



        private static bool RefreshStepCheckBlacklists(bool AddToCache)
        {
            if (!DataDictionary.Avoidances.Contains(c_ActorSNO) && !DataDictionary.AvoidanceBuffs.Contains(c_ActorSNO))
            {
                // See if it's something we should always ignore like ravens etc.
                if (!c_IsObstacle && DataDictionary.BlackListIds.Contains(c_ActorSNO))
                {
                    AddToCache = false;
                    c_IgnoreSubStep = "Blacklist";
                    return AddToCache;
                }
                // Temporary ractor GUID ignoring, to prevent 2 interactions in a very short time which can cause stucks
                if (IgnoreTargetForLoops > 0 && IgnoreRactorGUID == c_RActorGuid)
                {
                    AddToCache = false;
                    c_IgnoreSubStep = "IgnoreRactorGUID";
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



        private static string UtilSpacedConcat(params object[] args)
        {
            string output = "";
            foreach (object o in args)
            {
                output += o.ToString() + ", ";
            }
            return output;
        }


        private static void RefreshCachedHealth(int iLastCheckedHealth, double dThisCurrentHealth, bool bHasCachedHealth)
        {
            if (!bHasCachedHealth)
            {
                CacheData.currentHealthCache.Add(c_RActorGuid, dThisCurrentHealth);
                CacheData.currentHealthCheckTimeCache.Add(c_RActorGuid, iLastCheckedHealth);
            }
            else
            {
                CacheData.currentHealthCache[c_RActorGuid] = dThisCurrentHealth;
                CacheData.currentHealthCheckTimeCache[c_RActorGuid] = iLastCheckedHealth;
            }
        }

    }
}
