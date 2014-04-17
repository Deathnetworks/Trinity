using System;
using System.Linq;
using Trinity.Technicals;
using Zeta.Bot;
using Zeta.Bot.Navigation;
using Zeta.Common;
using Zeta.Game;
using Zeta.Game.Internals.Actors;
using Zeta.Game.Internals.SNO;
using Logger = Trinity.Technicals.Logger;
namespace Trinity
{
    public partial class Trinity
    {
        /// <summary>
        /// This will eventually be come our single source of truth and we can get rid of most/all of the below "c_" variables
        /// </summary>
        private static TrinityCacheObject CurrentCacheObject = new TrinityCacheObject();

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
        private static double c_KillRange = 0f;
        private static MonsterAffixes c_MonsterAffixes = MonsterAffixes.None;
        private static bool c_IsFacingPlayer;
        private static float c_Rotation;
        private static Vector2 c_DirectionVector = Vector2.Zero;
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
            {
                c_diaUnit = freshObject as DiaUnit;
            }

            /*
             * Set Common Data
             */
            AddToCache = RefreshStepGetCommonData(freshObject);

            // Ractor GUID
            c_RActorGuid = freshObject.RActorGuid;
            // Check to see if we've already looked at this GUID
            CurrentCacheObject.RActorGuid = freshObject.RActorGuid;
            CurrentCacheObject.ACDGuid = freshObject.ACDGuid;

            // Get Name
            c_InternalName = nameNumberTrimRegex.Replace(freshObject.Name, "");
            CurrentCacheObject.InternalName = nameNumberTrimRegex.Replace(freshObject.Name, "");

            // ActorSNO
            AddToCache = RefreshStepCachedActorSNO(AddToCache);
            if (!AddToCache) { c_IgnoreReason = "CachedActorSNO"; return AddToCache; }
            CurrentCacheObject.ActorSNO = freshObject.ActorSNO;

            CurrentCacheObject.ActorType = freshObject.ActorType;

            // Have ActorSNO Check for SNO based navigation obstacle hashlist
            c_IsObstacle = DataDictionary.NavigationObstacleIds.Contains(c_ActorSNO);

            // Add Cell Weight for Obstacle
            if (c_IsObstacle)
            {
                if (!DataDictionary.ObstacleCustomRadius.TryGetValue(c_ActorSNO, out c_Radius) &&
                    !CacheData.CollisionSphere.TryGetValue(c_RActorGuid, out c_Radius))
                {
                    c_Radius = c_diaObject.CollisionSphere.Radius;
                    //CacheData.CollisionSphere.Add(c_RActorGuid, c_Radius);
                }

                Vector3 pos;
                if (!CacheData.Position.TryGetValue(c_RActorGuid, out pos))
                {
                    CurrentCacheObject.Position = c_diaObject.Position;
                    //CacheData.Position.Add(c_RActorGuid, CurrentCacheObject.Position);
                }
                if (pos != Vector3.Zero)
                    CurrentCacheObject.Position = pos;

                CacheData.NavigationObstacles.Add(new CacheObstacleObject()
                {
                    ActorSNO = c_ActorSNO,
                    Name = c_InternalName,
                    Position = CurrentCacheObject.Position,
                    Radius = c_Radius,
                    ObjectType = c_ObjectType,
                });

                ((MainGridProvider)MainGridProvider).AddCellWeightingObstacle(c_ActorSNO, c_Radius);

                c_IgnoreReason = "NavigationObstacle";
                AddToCache = false;
                return AddToCache;
            }

            // Get ACDGuid
            AddToCache = RefreshStepCachedACDGuid(AddToCache);
            if (!AddToCache) { c_IgnoreReason = "CachedACDGuid"; return AddToCache; }

            // Summons by the player 
            AddToCache = RefreshStepCachedSummons(AddToCache);
            if (!AddToCache) { c_IgnoreReason = "CachedPlayerSummons"; return AddToCache; }


            CurrentCacheObject.Position = CurrentCacheObject.Object.Position;

            // Always Refresh Distance for every object
            RefreshStepCalculateDistance();

            using (new PerformanceLogger("RefreshDiaObject.CachedType"))
            {
                /*
                 * Set Object Type
                 */
                AddToCache = RefreshStepCachedObjectType(AddToCache);
                if (!AddToCache) { c_IgnoreReason = "CachedObjectType"; return AddToCache; }
            }

            CurrentCacheObject.Type = c_ObjectType;
            if (CurrentCacheObject.Type != GObjectType.Item)
            {
                CurrentCacheObject.ObjectHash = HashGenerator.GenerateWorldObjectHash(c_ActorSNO, CurrentCacheObject.Position, c_ObjectType.ToString(), Trinity.CurrentWorldDynamicId);
            }

            // Check Blacklists
            AddToCache = RefreshStepCheckBlacklists(AddToCache);
            if (!AddToCache) { c_IgnoreReason = "CheckBlacklists"; return AddToCache; }

            if (c_ObjectType == GObjectType.Item)
            {
                if (GenericBlacklist.ContainsKey(CurrentCacheObject.ObjectHash))
                {
                    AddToCache = false;
                    c_IgnoreReason = "GenericBlacklist";
                    return AddToCache;
                }
            }

            // Always Refresh ZDiff for every object
            AddToCache = RefreshStepObjectTypeZDiff(AddToCache);
            if (!AddToCache) { c_IgnoreReason = "ZDiff"; return AddToCache; }

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

            if (CurrentCacheObject.ObjectHash != String.Empty && GenericBlacklist.ContainsKey(CurrentCacheObject.ObjectHash))
            {
                AddToCache = false;
                c_IgnoreSubStep = "GenericBlacklist";
                return AddToCache;
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

                CurrentCacheObject.Type = c_ObjectType;
                CurrentCacheObject.CentreDistance = c_CentreDistance;
                CurrentCacheObject.RadiusDistance = c_RadiusDistance;
                CurrentCacheObject.InternalName = c_InternalName;
                CurrentCacheObject.Animation = c_CurrentAnimation;
                CurrentCacheObject.ACDGuid = c_ACDGUID;
                CurrentCacheObject.RActorGuid = c_RActorGuid;
                CurrentCacheObject.DynamicID = c_GameDynamicID;
                CurrentCacheObject.BalanceID = c_BalanceID;
                CurrentCacheObject.ActorSNO = c_ActorSNO;
                CurrentCacheObject.ItemLevel = c_ItemLevel;
                CurrentCacheObject.GoldAmount = c_GoldStackSize;
                CurrentCacheObject.OneHanded = c_IsOneHandedItem;
                CurrentCacheObject.TwoHanded = c_IsTwoHandedItem;
                CurrentCacheObject.ItemQuality = c_ItemQuality;
                CurrentCacheObject.DBItemBaseType = c_DBItemBaseType;
                CurrentCacheObject.DBItemType = c_DBItemType;
                CurrentCacheObject.FollowerType = c_item_tFollowerType;
                CurrentCacheObject.TrinityItemType = c_item_GItemType;
                CurrentCacheObject.IsElite = c_unit_IsElite;
                CurrentCacheObject.IsRare = c_unit_IsRare;
                CurrentCacheObject.IsUnique = c_unit_IsUnique;
                CurrentCacheObject.IsMinion = c_unit_IsMinion;
                CurrentCacheObject.IsTreasureGoblin = c_unit_IsTreasureGoblin;
                CurrentCacheObject.IsBoss = c_unit_IsBoss;
                CurrentCacheObject.IsAttackable = c_unit_IsAttackable;
                CurrentCacheObject.HitPoints = c_HitPoints;
                CurrentCacheObject.HitPointsPct = c_HitPointsPct;
                CurrentCacheObject.Radius = c_Radius;
                CurrentCacheObject.MonsterSize = c_unit_MonsterSize;
                CurrentCacheObject.IsEliteRareUnique = c_IsEliteRareUnique;
                CurrentCacheObject.ForceLeapAgainst = c_ForceLeapAgainst;
                CurrentCacheObject.HasDotDPS = c_HasDotDPS;
                CurrentCacheObject.KillRange = c_KillRange;
                CurrentCacheObject.HasAffixShielded = c_unit_HasShieldAffix;
                CurrentCacheObject.MonsterAffixes = c_MonsterAffixes;
                CurrentCacheObject.HasBeenInLoS = c_HasBeenInLoS;
                CurrentCacheObject.HasBeenNavigable = c_HasBeenNavigable;
                CurrentCacheObject.HasBeenRaycastable = c_HasBeenRaycastable;
                CurrentCacheObject.ItemLink = c_ItemLink;
                CurrentCacheObject.Rotation = c_Rotation;
                CurrentCacheObject.DirectionVector = c_DirectionVector;
                CurrentCacheObject.IsFacingPlayer = c_IsFacingPlayer;
                CurrentCacheObject.IsSummonedByPlayer = c_IsSummonedByPlayer;
                CurrentCacheObject.IsSummoner = c_IsSummoner;

                ObjectCache.Add(CurrentCacheObject);
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
                    CacheData.NavigationObstacles.Add(new CacheObstacleObject()
                    {
                        ActorSNO = c_ActorSNO,
                        Radius = c_Radius,
                        Position = CurrentCacheObject.Position,
                        Name = c_InternalName,
                        ObjectType = c_ObjectType,
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
                // Add to the collision-list
                CacheData.MonsterObstacles.Add(new CacheObstacleObject()
                {
                    ActorSNO = c_ActorSNO,
                    Name = c_InternalName,
                    Position = CurrentCacheObject.Position,
                    Radius = c_Radius,
                    ObjectType = c_ObjectType,
                });
            }
        }
        /// <summary>
        /// Initializes variable set for single object refresh
        /// </summary>
        private static void RefreshStepInit(out bool AddTocache)
        {
            CurrentCacheObject = new TrinityCacheObject();
            AddTocache = true;
            // Start this object as off as unknown type
            c_ObjectType = GObjectType.Unknown;

            c_CentreDistance = 0f;
            c_RadiusDistance = 0f;
            c_Radius = 0f;
            c_ZDiff = 0f;
            c_ItemDisplayName = "";
            c_ItemLink = "";
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
            c_KillRange = 0f;
            c_MonsterAffixes = MonsterAffixes.None;
            c_IsFacingPlayer = false;
            c_Rotation = 0f;
            c_IsSummonedByPlayer = false;
            c_IsSummoner = false;
            c_DirectionVector = Vector2.Zero;
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
            if (!CacheData.ActorSNO.TryGetValue(c_RActorGuid, out c_ActorSNO))
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
                CacheData.ActorSNO.Add(c_RActorGuid, c_ActorSNO);
            }
            return AddToCache;
        }

        private static bool RefreshInternalName(bool AddToCache)
        {
            // This is "internalname" for items, and just a "generic" name for objects and units - cached if possible
            if (!CacheData.Name.TryGetValue(c_RActorGuid, out c_InternalName))
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
                CacheData.Name.Add(c_RActorGuid, c_InternalName);
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

        private static void RefreshStepCalculateDistance()
        {
            // Calculate distance, don't rely on DB's internal method as this may hit Diablo 3 memory again
            c_CentreDistance = Player.Position.Distance2D(CurrentCacheObject.Position);
            CurrentCacheObject.CentreDistance = Player.Position.Distance2D(CurrentCacheObject.Position);

            // Set radius-distance to centre distance at first
            c_RadiusDistance = c_CentreDistance;
            CurrentCacheObject.RadiusDistance = Player.Position.Distance2D(CurrentCacheObject.Position);
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

            if (DataDictionary.SameWorldPortals.Contains(CurrentCacheObject.ActorSNO))
            {
                c_ObjectType = GObjectType.JumpLinkPortal;
                return true;
            }


        // Either get the cached object type, or calculate it fresh
            else if (!c_IsObstacle && !CacheData.ObjectType.TryGetValue(c_RActorGuid, out c_ObjectType))
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
                                CacheData.ObjectType.Remove(c_RActorGuid);
                            }
                            if (hasBuff)
                            {
                                AddToCache = true;
                                c_ObjectType = GObjectType.Avoidance;
                            }
                            else
                            {
                                CacheData.ObjectType.Remove(c_RActorGuid);
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

                    else if (c_diaObject is DiaGizmo && c_diaObject.ActorType == ActorType.Gizmo && c_CentreDistance <= 90)
                    {

                        DiaGizmo c_diaGizmo;
                        c_diaGizmo = (DiaGizmo)c_diaObject;

                        if (CurrentCacheObject.InternalName.Contains("CursedChest"))
                        {
                            c_ObjectType = GObjectType.CursedChest;
                            return true;
                        }

                        if (CurrentCacheObject.InternalName.Contains("CursedShrine"))
                        {
                            c_ObjectType = GObjectType.CursedShrine;
                            return true;
                        }

                        if (c_diaGizmo.IsBarricade)
                        {
                            c_ObjectType = GObjectType.Barricade;
                        }
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
                                    c_ObjectType = GObjectType.Barricade;
                                    break;
                                case GizmoType.BreakableChest:
                                    c_ObjectType = GObjectType.Destructible;
                                    break;
                                case GizmoType.DestroyableObject:
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
                {  // Now cache the object type if it's on the screen and we know what it is
                    //CacheData.ObjectType.Add(c_RActorGuid, c_ObjectType);
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
                        if (!LootTargeting.Instance.AllowedToLoot || LootTargeting.Instance.DisableLooting)
                        {
                            AddToCache = false;
                            c_IgnoreSubStep = "LootingDisabled";
                            break;
                        }


                        if (c_ObjectType != GObjectType.HealthGlobe && c_ObjectType != GObjectType.PowerGlobe && (ForceVendorRunASAP || TownRun.TownRunTimerRunning()))
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
                case GObjectType.JumpLinkPortal:
                case GObjectType.CursedChest:
                case GObjectType.CursedShrine:
                    {
                        AddToCache = RefreshGizmo(AddToCache);
                        break;
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
                // Bounty Objectives should always be on the weight list
                if (CurrentCacheObject.IsBountyObjective)
                    return true;

                // Always LoS Units during events
                if (c_ObjectType == GObjectType.Unit && Player.InActiveEvent)
                    return true;

                // Everything except items and the current target
                if (c_ObjectType != GObjectType.Item && c_RActorGuid != LastTargetRactorGUID && c_ObjectType != GObjectType.Unknown)
                {
                    if (c_CentreDistance < 95)
                    {
                        switch (c_ObjectType)
                        {
                            case GObjectType.Destructible:
                            case GObjectType.HealthWell:
                            case GObjectType.Unit:
                            case GObjectType.Shrine:
                            case GObjectType.Gold:
                                {
                                    using (new PerformanceLogger("RefreshLoS.2"))
                                    {
                                        // Get whether or not this RActor has ever been in a path line with AllowWalk. If it hasn't, don't add to cache and keep rechecking
                                        if (!CacheData.HasBeenRayCasted.TryGetValue(c_RActorGuid, out c_HasBeenRaycastable) || DataDictionary.AlwaysRaycastWorlds.Contains(Trinity.Player.WorldID))
                                        {
                                            if (c_CentreDistance >= 1f && c_CentreDistance <= 5f)
                                            {
                                                c_HasBeenRaycastable = true;
                                                CacheData.HasBeenRayCasted.Add(c_RActorGuid, c_HasBeenRaycastable);
                                            }
                                            else if (Settings.Combat.Misc.UseNavMeshTargeting)
                                            {
                                                Vector3 myPos = new Vector3(Player.Position.X, Player.Position.Y, Player.Position.Z + 8f);
                                                Vector3 cPos = new Vector3(CurrentCacheObject.Position.X, CurrentCacheObject.Position.Y, CurrentCacheObject.Position.Z + 8f);
                                                cPos = MathEx.CalculatePointFrom(cPos, myPos, c_Radius + 1f);

                                                if (Single.IsNaN(cPos.X) || Single.IsNaN(cPos.Y) || Single.IsNaN(cPos.Z))
                                                    cPos = CurrentCacheObject.Position;

                                                if (!NavHelper.CanRayCast(myPos, cPos))
                                                {
                                                    AddToCache = false;
                                                    c_IgnoreSubStep = "UnableToRayCast";
                                                }
                                                else
                                                {
                                                    c_HasBeenRaycastable = true;
                                                    CacheData.HasBeenRayCasted.Add(c_RActorGuid, c_HasBeenRaycastable);
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
                                                    CacheData.HasBeenRayCasted.Add(c_RActorGuid, c_HasBeenRaycastable);
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
                                        if (!CacheData.HasBeenInLoS.TryGetValue(c_RActorGuid, out c_HasBeenInLoS) || DataDictionary.AlwaysRaycastWorlds.Contains(Trinity.Player.WorldID))
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
                                                    CacheData.HasBeenInLoS.Add(c_RActorGuid, c_HasBeenInLoS);
                                                }
                                            }
                                            else
                                            {
                                                c_HasBeenInLoS = true;
                                                CacheData.HasBeenInLoS.Add(c_RActorGuid, c_HasBeenInLoS);
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
                    if ((c_unit_IsBoss || CurrentCacheObject.IsQuestMonster || CurrentCacheObject.IsBountyObjective) && c_RadiusDistance < 100f)
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
                if (!CacheData.AcdGuid.TryGetValue(c_RActorGuid, out c_ACDGUID))
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
                    CacheData.AcdGuid.Add(c_RActorGuid, c_ACDGUID);
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

        private static bool RefreshStepCachedDynamicIds(bool AddToCache)
        {
            // Try and grab the dynamic id and game balance id, if necessary and if possible
            if (c_ObjectType == GObjectType.Item)
            {
                // Get the Dynamic ID, cached if possible
                if (!CacheData.DynamicID.TryGetValue(c_RActorGuid, out c_GameDynamicID))
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
                    CacheData.DynamicID.Add(c_RActorGuid, c_GameDynamicID);
                }
                // Get the Game Balance ID, cached if possible
                if (!CacheData.GameBalanceID.TryGetValue(c_RActorGuid, out c_BalanceID))
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
                    CacheData.GameBalanceID.Add(c_RActorGuid, c_BalanceID);
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
            if (!DataDictionary.Avoidances.Contains(c_ActorSNO) && !DataDictionary.AvoidanceBuffs.Contains(c_ActorSNO) && !CurrentCacheObject.IsBountyObjective && !CurrentCacheObject.IsQuestMonster)
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
                CacheData.CurrentUnitHealth.Add(c_RActorGuid, dThisCurrentHealth);
                CacheData.LastCheckedUnitHealth.Add(c_RActorGuid, iLastCheckedHealth);
            }
            else
            {
                CacheData.CurrentUnitHealth[c_RActorGuid] = dThisCurrentHealth;
                CacheData.LastCheckedUnitHealth[c_RActorGuid] = iLastCheckedHealth;
            }
        }

    }
}
