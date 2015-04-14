using System;
using System.Linq;
using Trinity.Cache;
using Trinity.Helpers;
using Trinity.Items;
using Trinity.Technicals;
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
        private static TrinityCacheObject c_CacheObject = new TrinityCacheObject();

        private static Avoidance c_Avoidance = null;
        private static ACD c_CommonData = null;
        private static SNOAnim c_CurrentAnimation = SNOAnim.Invalid;
        private static ItemBaseType c_DBItemBaseType = ItemBaseType.None;
        private static ItemType c_DBItemType = ItemType.Unknown;
        private static DiaGizmo c_diaGizmo = null;
        private static DiaObject c_diaObject = null;
        private static DiaUnit c_diaUnit = null;
        private static Vector2 c_DirectionVector = Vector2.Zero;
        private static int c_GoldStackSize = 0;
        private static bool c_HasDotDPS = false;
        private static double c_HitPoints = 0d;
        private static double c_HitPointsPct = 0d;
        private static string c_IgnoreReason = "";
        private static string c_InfosSubStep = "";
        private static bool c_IsAncient = false;
        private static bool c_IsEliteRareUnique = false;
        private static bool c_IsFacingPlayer;
        private static bool c_IsObstacle = false;
        private static bool c_IsOneHandedItem = false;
        private static bool c_IsTwoHandedItem = false;
        private static GItemType c_item_GItemType = GItemType.Unknown;
        private static FollowerType c_item_tFollowerType = FollowerType.None;
        private static string c_InternalName = "";
        private static string c_ItemDisplayName = "";
        private static int c_ItemLevel = 0;
        private static string c_ItemLink = String.Empty;
        private static string c_ItemMd5Hash = string.Empty;
        private static ItemQuality c_ItemQuality = ItemQuality.Invalid;
        private static MonsterAffixes c_MonsterAffixes = MonsterAffixes.None;
        private static ActorMovement c_Movement = null;
        private static float c_Radius = 0f;
        private static float c_Rotation = 0f;
        private static int c_TargetACDGuid = -1;
        private static Vector3 c_TargetACDPosition = Vector3.Zero;
        private static bool c_unit_HasShieldAffix = false;
        private static bool c_unit_IsAttackable = false;
        private static bool c_unit_IsElite = false;
        private static bool c_unit_IsMinion = false;
        private static bool c_unit_IsRare = false;
        private static bool c_unit_IsTreasureGoblin = false;
        private static bool c_unit_IsUnique = false;
        private static SNORecordMonster c_unit_MonsterInfo = null;
        private static MonsterSize c_unit_MonsterSize = MonsterSize.Unknown;
        private static float c_ZDiff = 0f;

        private static bool GetReturnIgnore(string reason, bool addToIgnoreCache = true)
        {
            c_IgnoreReason = reason;

            using (new MemorySpy("CacheDiaObject().GetReturnIgnore()"))
            {
                if (addToIgnoreCache)
                {
                bool containsKey = CacheData.ObjectsIgnored.TryGetValue(c_CacheObject.RActorGuid, out reason);

                    if (c_CacheObject.RActorGuid != -1)
                    {
                    if (!containsKey) CacheData.ObjectsIgnored.Add(c_CacheObject.RActorGuid, c_IgnoreReason);
                        else c_IgnoreReason += " " + reason;
                    }
                }
            }

            return false;
        }

        private static bool CacheDiaObject(DiaObject diaObject)
        {
            bool AddToCache = true;
            RefreshStepInit();

            AddToCache = diaObject.IsValid;
            if (!AddToCache)
            {
                return GetReturnIgnore("InvalidRActor");
            }

            c_CommonData = diaObject.CommonData;

            AddToCache = c_CommonData != null;
            if (!AddToCache)
            {
                return GetReturnIgnore("ACDNull");
            }

            AddToCache = c_CommonData.IsValid;
            if (!AddToCache)
            {
                return GetReturnIgnore("InvalidACD");
            }

            try
            {
                c_CacheObject.InternalName = NameNumberTrimRegex.Replace(diaObject.Name, "");
            }
            catch
            {
                return GetReturnIgnore("InvalidName");
            }

            AddToCache = !IgnoreNames.Any(i => c_CacheObject.InternalName.ToLower().StartsWith(i, StringComparison.OrdinalIgnoreCase));
            if (!AddToCache)
            {
                return GetReturnIgnore("IgnoreName");
            }

            try
            {
                c_diaObject = diaObject;

                c_CacheObject.RActorGuid = c_diaObject.RActorGuid;
                c_CacheObject.ActorSNO = c_diaObject.ActorSNO;
                c_CacheObject.ActorType = c_diaObject.ActorType;
                c_CacheObject.ACDGuid = c_diaObject.ACDGuid;

                c_CacheObject.LastSeenTime = DateTime.UtcNow;
                c_CacheObject.Position = c_diaObject.Position;
            }
            catch
            {
                return GetReturnIgnore("InvalidObject");
            }

            using (new MemorySpy("CacheDiaObject().GetRadius"))
            {
                if (!DataDictionary.CustomObjectRadius.TryGetValue(c_CacheObject.ActorSNO, out c_Radius))
                    c_Radius = c_diaObject.CollisionSphere.Radius;

                c_CacheObject.Radius = c_Radius;
            }

            using (new MemorySpy("CacheDiaObject().CheckNavObstacles"))
            {
                AddToCache = RefreshStepNavigationObstacle();
                if (!AddToCache)
                {
                    return GetReturnIgnore("NavObstacle");
                }
            }

            using (new MemorySpy("CacheDiaObject().StepObjectType"))
            {
                AddToCache = RefreshStepCachedObjectType();
                if (!AddToCache)
                {
                    return GetReturnIgnore("CachedObjectType");
                }
            }

            using (new MemorySpy("CacheDiaObject().StepPlayerSummons"))
            {
                AddToCache = RefreshStepCachedSummons();
                if (!AddToCache)
                {
                    return GetReturnIgnore("CachedPlayerSummons");
                }
            }

            using (new MemorySpy("CacheDiaObject().StepCheckBlacklists"))
            {
                AddToCache = RefreshStepCheckBlacklists();
                if (!AddToCache)
                {
                    return GetReturnIgnore("CheckBlacklists");
                }

                if (CacheData.ObjectsIgnored.ContainsKey(c_CacheObject.RActorGuid))
                { 
                    return GetReturnIgnore("AlreadyIgnored"); 
                }
            }

            using (new MemorySpy("CacheDiaObject().StepZDiff"))
            {
                AddToCache = RefreshStepObjectTypeZDiff(AddToCache);
                if (!AddToCache)
                {
                    return GetReturnIgnore("ZDiff");
                }
            }

            using (new MemorySpy("CacheDiaObject().GetComplex"))
            {
                if (c_CacheObject.IsUnit || c_CacheObject.Type == GObjectType.Unknown)
                {
                    try { c_CurrentAnimation = c_diaObject.CommonData.CurrentAnimation; }
                    catch { c_InfosSubStep += "InvalidAnimation "; }

                    try { c_IsFacingPlayer = c_diaObject.IsFacingPlayer; }
                    catch { c_InfosSubStep += "ErrorGettingFacing "; }

                    try
                    {
                        if (c_diaObject.Movement.IsValid)
                        {
                            c_Movement = c_diaObject.Movement;
                            c_Rotation = c_Movement.Rotation;
                            c_DirectionVector = c_Movement.DirectionVector;
                            if (c_Movement.ACDTarget != null)
                            {
                                c_TargetACDGuid = c_Movement.ACDTargetGuid;
                                c_TargetACDPosition = c_Movement.ACDTarget.Position;
                            }
                        }
                    }
                    catch { c_InfosSubStep += "ErrorGettingMovement "; }
                }
            }

            using (new MemorySpy("CacheDiaObject().StepMainObjectType"))
            {
                RefreshStepMainObjectType(ref AddToCache);
                if (!AddToCache)
                {
                    return GetReturnIgnore("MainObjectType");
                }
            }

            using (new MemorySpy("CacheDiaObject().StepIgnoreUnknown"))
            {
                AddToCache = RefreshStepIgnoreUnknown(AddToCache);
                if (!AddToCache)
                {
                    return GetReturnIgnore("IgnoreUnknown");
                }
            }

            using (new MemorySpy("CacheDiaObject().LoSCheck"))
            {
                AddToCache = RefreshStepIgnoreLoS();
                if (!AddToCache)
                {
                    return GetReturnIgnore("IgnoreLoS", false);
                }
            }

            AddUnitToMonsterObstacleCache();

            c_IgnoreReason = "None";
            c_CacheObject.ACDGuid = c_CacheObject.ACDGuid;
            c_CacheObject.ActorSNO = c_CacheObject.ActorSNO;
            c_CacheObject.Animation = c_CurrentAnimation;
            c_CacheObject.DBItemBaseType = c_DBItemBaseType;
            c_CacheObject.DBItemType = c_DBItemType;
            c_CacheObject.DirectionVector = c_DirectionVector;
            c_CacheObject.Distance = c_CacheObject.Distance;
            c_CacheObject.DynamicID = c_CacheObject.DynamicID;
            c_CacheObject.FollowerType = c_item_tFollowerType;
            c_CacheObject.GameBalanceID = c_CacheObject.GameBalanceID;
            c_CacheObject.GoldAmount = c_GoldStackSize;
            c_CacheObject.HasAffixShielded = c_unit_HasShieldAffix;
            c_CacheObject.HasDotDPS = c_HasDotDPS;
            c_CacheObject.HitPoints = c_HitPoints;
            c_CacheObject.HitPointsPct = c_HitPointsPct;
            c_CacheObject.InternalName = c_CacheObject.InternalName;
            c_CacheObject.IsAttackable = c_unit_IsAttackable;
            c_CacheObject.IsElite = c_unit_IsElite;
            c_CacheObject.IsEliteRareUnique = c_IsEliteRareUnique;
            c_CacheObject.IsFacingPlayer = c_IsFacingPlayer;
            c_CacheObject.IsMinion = c_unit_IsMinion;
            c_CacheObject.IsRare = c_unit_IsRare;
            c_CacheObject.IsTreasureGoblin = c_unit_IsTreasureGoblin;
            c_CacheObject.IsUnique = c_unit_IsUnique;
            c_CacheObject.ItemLevel = c_ItemLevel;
            c_CacheObject.ItemLink = c_ItemLink;
            c_CacheObject.ItemQuality = c_ItemQuality;
            c_CacheObject.MonsterAffixes = c_MonsterAffixes;
            c_CacheObject.MonsterSize = c_unit_MonsterSize;
            c_CacheObject.OneHanded = c_IsOneHandedItem;
            c_CacheObject.RActorGuid = c_CacheObject.RActorGuid;
            c_CacheObject.Radius = c_CacheObject.Radius;
            c_CacheObject.Rotation = c_Rotation;
            c_CacheObject.TrinityItemType = c_item_GItemType;
            c_CacheObject.TwoHanded = c_IsTwoHandedItem;
            c_CacheObject.Type = c_CacheObject.Type;
            c_CacheObject.IsAncient = c_IsAncient;
            c_CacheObject.ZDiff = c_ZDiff;

            ObjectCache.Add(c_CacheObject);
            return true;
        }

        private static bool RefreshStepNavigationObstacle()
        {
            c_IsObstacle = DataDictionary.NavigationObstacleIds.Contains(c_CacheObject.ActorSNO);
            if (c_IsObstacle)
            {
                try { c_CurrentAnimation = c_diaObject.CommonData.CurrentAnimation; }
                catch { c_InfosSubStep += "Obstacle.InvalidAnimation "; }

                try { c_IsFacingPlayer = c_diaObject.IsFacingPlayer; }
                catch { c_InfosSubStep += "Obstacle.ErrorGettingFacing "; }

                RAAvoidances();

                AddObjectToNavigationObstacleCache();
                return false;
            }

            return true;
        }

        private static void AddObjectToNavigationObstacleCache()
        {
            MainGridProvider.AddCellWeightingObstacle(c_CacheObject.ActorSNO, c_CacheObject.Radius);

            CacheData.AddToNavigationObstacles(new CacheObstacleObject()
            {
                ActorSNO = c_CacheObject.ActorSNO,
                RActorGUID = c_CacheObject.RActorGuid,
                Name = c_CacheObject.InternalName,
                Position = c_CacheObject.Position,
                Radius = c_CacheObject.Radius,
                ObjectType = c_CacheObject.Type,
            });
        }
        /// <summary>
        /// Adds a unit to cache hashMonsterObstacleCache
        /// </summary>
        private static void AddUnitToMonsterObstacleCache()
        {
            if (c_CacheObject.Type == GObjectType.Unit)
            {
                // Add to the collision-list
                CacheData.MonsterObstacles.Add(new CacheObstacleObject()
                {
                    ActorSNO = c_CacheObject.ActorSNO,
                    Name = c_CacheObject.InternalName,
                    Position = c_CacheObject.Position,
                    Radius = c_CacheObject.Radius,
                    ObjectType = c_CacheObject.Type,
                });
            }
        }
        /// <summary>
        /// Initializes variable set for single object refresh
        /// </summary>
        private static void RefreshStepInit()
        {
            c_CacheObject = new TrinityCacheObject();
            c_CacheObject.Type = GObjectType.Unknown;
            c_CacheObject.Distance = -1f;
            c_CacheObject.Radius = 0f;
            c_CacheObject.ACDGuid = -1;
            c_CacheObject.RActorGuid = -1;
            c_CacheObject.DynamicID = -1;
            c_CacheObject.GameBalanceID = -1;
            c_CacheObject.ActorSNO = -1;            

            c_ZDiff = 0f;
            c_ItemDisplayName = "";
            c_ItemLink = "";
            c_CacheObject.InternalName = "";
            c_IgnoreReason = "";
            c_InfosSubStep = "";
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
            c_unit_IsAttackable = false;
            c_unit_HasShieldAffix = false;
            c_IsEliteRareUnique = false;
            c_IsObstacle = false;
            c_ItemMd5Hash = string.Empty;
            c_ItemQuality = ItemQuality.Invalid;
            c_DBItemBaseType = ItemBaseType.None;
            c_DBItemType = ItemType.Unknown;
            c_item_tFollowerType = FollowerType.None;
            c_item_GItemType = GItemType.Unknown;
            c_unit_MonsterSize = MonsterSize.Unknown;
            c_diaObject = null;
            c_diaUnit = null;
            c_diaGizmo = null;
            c_CurrentAnimation = SNOAnim.Invalid;
            c_HasDotDPS = false;
            c_MonsterAffixes = MonsterAffixes.None;
            c_IsFacingPlayer = false;
            c_Rotation = 0f;
            c_DirectionVector = Vector2.Zero;
            c_TargetACDGuid = -1;
            c_TargetACDPosition = Vector3.Zero;
            c_Movement = null;
            c_CommonData = null;
            c_Radius = 0f;
            c_Avoidance = null;
            c_unit_MonsterInfo = null;
        }

        private static bool RefreshStepCachedObjectType()
        {
            bool isAvoidance;
            using (new MemorySpy("StepObjectType().Avoidance"))
            {
                isAvoidance = DataDictionary.ActorAvoidances.TryGetValue((SNOActor)c_CacheObject.ActorSNO, out c_Avoidance);
            }

            if (isAvoidance)
            {
                if (!Settings.Combat.Misc.AvoidAOE) { c_InfosSubStep += "AvoidanceDisabled "; return false; }
                c_CacheObject.Type = GObjectType.Avoidance;
            }
            else
            {
                using (new MemorySpy("StepObjectType().Check"))
                {
                    if (DataDictionary.ButcherFloorPanels.Contains(c_CacheObject.ActorSNO))
                    {
                        if (Settings.Combat.Misc.AvoidAOE)
                        {
                            bool hasBuff;
                            try
                            {
                                hasBuff = c_diaObject.CommonData.GetAttribute<int>(ActorAttributeType.HasLookOverride) > 0;
                            }
                            catch
                            {
                                c_InfosSubStep += "ErrorGettingButcherBEffect";
                                return false;
                            }

                            if (!hasBuff)
                            {
                                c_InfosSubStep += "ButcherFloorUnbuffed "; return false;
                            }
                            c_CacheObject.Type = GObjectType.Avoidance;
                        }
                        else { c_InfosSubStep += "AvoidanceDisabled "; return false; }
                    }
                    else if (c_diaObject.ActorType == ActorType.Monster)
                    {
                        if (c_diaObject.ACDGuid != c_diaObject.CommonData.ACDGuid)
                        {
                            c_InfosSubStep += "InvalidUnitACD "; return false;
                        }

                        c_diaUnit = c_diaObject as DiaUnit;
                        c_CacheObject.Type = GObjectType.Unit;
                    }
                    else if (c_diaObject.ActorType == ActorType.Player)
                    {
                        if (c_diaObject.ACDGuid != c_diaObject.CommonData.ACDGuid)
                        {
                            c_InfosSubStep += "InvalidUnitACD "; return false;
                        }

                        c_diaUnit = c_diaObject as DiaUnit;
                        c_CacheObject.Type = GObjectType.Player;
                    }
                    else if (DataDictionary.ForceToItemOverrideIds.Contains(c_CacheObject.ActorSNO) || (c_diaObject.ActorType == ActorType.Item))
                    {
                        if (c_diaObject.ACDGuid != c_diaObject.CommonData.ACDGuid)
                        {
                            c_InfosSubStep += "InvalidItemACD "; return false;
                        }

                        if (c_CacheObject.InternalName.ToLower().StartsWith("gold"))
                        {
                            c_CacheObject.Type = GObjectType.Gold;
                        }
                        else
                        {
                            c_CacheObject.Type = GObjectType.Item;
                        }
                    }
                    else if (DataDictionary.InteractWhiteListIds.Contains(c_CacheObject.ActorSNO))
                    {
                        c_CacheObject.Type = GObjectType.Interactable;
                    }
                    else if (c_diaObject.ActorType == ActorType.Gizmo && c_CacheObject.Distance <= 90)
                    {
                        c_diaGizmo = c_diaObject as DiaGizmo;

                        if (c_CacheObject.InternalName.Contains("CursedChest"))
                        {
                            c_CacheObject.Type = GObjectType.CursedChest;
                        }
                        else if (c_CacheObject.InternalName.Contains("CursedShrine"))
                        {
                            c_CacheObject.Type = GObjectType.CursedShrine;
                        }
                        else if (c_diaGizmo.IsBarricade)
                        {
                            c_CacheObject.Type = GObjectType.Barricade;
                        }
                        else
                        {
                            switch (c_diaGizmo.ActorInfo.GizmoType)
                            {
                                case GizmoType.HealingWell:
                                    c_CacheObject.Type = GObjectType.HealthWell;
                                    break;
                                case GizmoType.Door:
                                    c_CacheObject.Type = GObjectType.Door;
                                    break;
                                case GizmoType.PoolOfReflection:
                                case GizmoType.PowerUp:
                                    c_CacheObject.Type = GObjectType.Shrine;
                                    break;
                                case GizmoType.Chest:
                                    c_CacheObject.Type = GObjectType.Container;
                                    break;
                                case GizmoType.BreakableDoor:
                                    c_CacheObject.Type = GObjectType.Barricade;
                                    break;
                                case GizmoType.BreakableChest:
                                    c_CacheObject.Type = GObjectType.Destructible;
                                    break;
                                case GizmoType.DestroyableObject:
                                    c_CacheObject.Type = GObjectType.Destructible;
                                    break;
                                case GizmoType.PlacedLoot:
                                case GizmoType.Switch:
                                case GizmoType.Headstone:
                                    c_CacheObject.Type = GObjectType.Interactable;
                                    break;
                                default:
                                    c_CacheObject.Type = GObjectType.Unknown;
                                    break;
                            }
                        }
                    }
                }
            }

            return true;
        }

        private static void RefreshStepMainObjectType(ref bool AddToCache)
        {
            // Now do stuff specific to object types
            switch (c_CacheObject.Type)
            {
                case GObjectType.Player:
                    {
                        using (new MemorySpy("StepMainObjectType().Player"))
                        {
                            AddToCache = RefreshUnit();
                        }
                        break;
                    }
                // Handle Unit-type Objects
                case GObjectType.Unit:
                    {
                        if (!Combat.Abilities.CombatBase.IsCombatAllowed)
                        {
                            c_InfosSubStep += "CombatDisabled"; AddToCache = false;
                            break;
                        }

                        using (new MemorySpy("StepMainObjectType().UnitAAvoidance"))
                        {
                            RAAvoidances();
                        }
                        using (new MemorySpy("StepMainObjectType().Unit"))
                        {
                            AddToCache = RefreshUnit();
                        }
                        break;
                    }
                // Handle Item-type Objects
                case GObjectType.Item:
                    {
                        if (TrinityItemManager.FindValidBackpackLocation(true) == new Vector2(-1, -1))
                        {
                            c_InfosSubStep += "NoFreeSlots"; AddToCache = false;
                            break;
                        }

                        using (new MemorySpy("StepMainObjectType().Item"))
                        {
                            AddToCache = RefreshItem();
                        }
                        break;
                    }
                // Handle Gold
                case GObjectType.Gold:
                    {
                        using (new MemorySpy("StepMainObjectType().Gold"))
                        {
                            AddToCache = RefreshGold();
                        }
                        break;
                    }
                case GObjectType.PowerGlobe:
                case GObjectType.HealthGlobe:
                case GObjectType.ProgressionGlobe:
                    {
                        AddToCache = true;
                        break;
                    }
                // Handle Avoidance Objects
                case GObjectType.Avoidance:
                    {
                        using (new MemorySpy("StepMainObjectType().AAvoidance"))
                        {
                            RAAvoidances();
                        }
                        using (new MemorySpy("StepMainObjectType().Avoidance"))
                        {
                            AddToCache = RefreshAvoidance();
                        }
                        break;
                    }
                // Handle Door
                case GObjectType.Door:
                    {
                        c_CacheObject.Radius = 30f;
                        AddObjectToNavigationObstacleCache();

                        using (new MemorySpy("StepMainObjectType().Gizmo"))
                        {
                            AddToCache = RefreshGizmo(AddToCache);
                        }
                        break;
                    }
                // Handle Other-type Objects
                case GObjectType.Destructible:
                case GObjectType.Barricade:
                case GObjectType.Container:
                case GObjectType.Shrine:
                case GObjectType.Interactable:
                case GObjectType.HealthWell:
                case GObjectType.CursedChest:
                case GObjectType.CursedShrine:
                    {
                        AddObjectToNavigationObstacleCache();
                        using (new MemorySpy("StepMainObjectType().Gizmo"))
                        {
                            AddToCache = RefreshGizmo(AddToCache);
                        }
                        break;
                    }
                // Object switch on type (to seperate shrines, destructibles, barricades etc.)
                default:
                    {
                        using (new MemorySpy("StepMainObjectType().UnknownAAvoidance")) { RAAvoidances(); }
                        DebugUtil.LogUnknown(c_diaObject);

                        c_InfosSubStep += "Unknown." + c_diaObject.ActorType;
                        AddToCache = false;
                        break;
                    }

            }
        }


        /// <summary>
        /// Special handling for whether or not we want to cache an object that's not in LoS
        /// </summary>
        /// <returns></returns>
        private static bool RefreshStepIgnoreLoS()
        {
            //// Force navigable/los
            //if (NavHelper.CanRayCast(Player.Position, c_CacheObject.Position))
            //{
            //    //c_CacheObject.IsInLineOfSight = true;
            //}

            // add everything, new IsNavigable & IsInLineOfSight are use in weighting just when needed, reduce usage from memory
            return true;
        }

        private static bool RefreshStepIgnoreUnknown(bool AddToCache)
        {
            // We couldn't get a valid object type, so ignore it
            if (c_CacheObject.Type == GObjectType.Unknown)
            {
                AddToCache = false;
            }
            return AddToCache;
        }

        private static bool RefreshStepObjectTypeZDiff(bool AddToCache)
        {
            c_ZDiff = c_diaObject.ZDiff;
            // always take current target regardless if ZDiff changed
            if (c_CacheObject.RActorGuid == LastTargetRactorGUID)
            {
                AddToCache = true;
                return AddToCache;
            }

            // Ignore stuff which has a Z-height-difference too great, it's probably on a different level etc. - though not avoidance!
            if (c_CacheObject.Type != GObjectType.Avoidance)
            {
                // Calculate the z-height difference between our current position, and this object's position
                switch (c_CacheObject.Type)
                {
                    case GObjectType.Door:
                    case GObjectType.Unit:
                    case GObjectType.Barricade:
                        // Ignore monsters (units) who's Z-height is 14 foot or more than our own z-height except bosses
                        if (c_ZDiff >= 11f && !c_CacheObject.IsBoss)
                        {
                            AddToCache = true;
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

        private static bool RefreshStepCheckBlacklists()
        {
            if (!c_CacheObject.IsAvoidance && !c_CacheObject.IsBountyObjective && !c_CacheObject.IsQuestMonster)
            {
                // Temporary ractor GUID ignoring, to prevent 2 interactions in a very short time which can cause stucks
                if (_ignoreTargetForLoops > 0 && _ignoreRactorGuid == c_CacheObject.RActorGuid)
                {
                    c_InfosSubStep += "IgnoreRactorGUID ";
                    return false;
                }
                // Check our extremely short-term destructible-blacklist
                if (_destructible3SecBlacklist.Contains(c_CacheObject.RActorGuid))
                {
                    c_InfosSubStep += "Destructible3SecBlacklist ";
                    return false;
                }
                // Check our extremely short-term destructible-blacklist
                if (Blacklist1Second.Contains(c_CacheObject.RActorGuid))
                {
                    c_InfosSubStep += "hashRGUIDBlacklist1 ";
                    return false;
                }
                // Check our extremely short-term destructible-blacklist
                if (Blacklist3Seconds.Contains(c_CacheObject.RActorGuid))
                {
                    c_InfosSubStep += "hashRGUIDBlacklist3 ";
                    return false;
                }
                // See if it's on our 90 second blacklist (from being stuck targeting it), as long as it's distance is not extremely close
                if (Blacklist90Seconds.Contains(c_CacheObject.RActorGuid))
                {
                    c_InfosSubStep += "Blacklist90Seconds ";
                    return false;
                }
                // 60 second blacklist
                if (Blacklist60Seconds.Contains(c_CacheObject.RActorGuid))
                {
                    c_InfosSubStep += "Blacklist60Seconds ";
                    return false;
                }
                // 15 second blacklist
                if (Blacklist15Seconds.Contains(c_CacheObject.RActorGuid))
                {
                    c_InfosSubStep += "Blacklist15Seconds ";
                    return false;
                }
                // See if it's something we should always ignore like ravens etc.
                if (DataDictionary.BlackListIds.Contains(c_CacheObject.ActorSNO))
                {
                    c_InfosSubStep += "Blacklist ";
                    return false;
                }
            }

            if (c_CacheObject.Type != GObjectType.Item)
            {
                c_CacheObject.ObjectHash = HashGenerator.GenerateWorldObjectHash(c_CacheObject.ActorSNO, c_CacheObject.Position, c_CacheObject.Type.ToString(), Trinity.CurrentWorldDynamicId);
                if (c_CacheObject.ObjectHash != String.Empty && GenericBlacklist.ContainsKey(c_CacheObject.ObjectHash))
                {
                    c_InfosSubStep += "GenericBlacklist ";
                    return false;
                }
            }

            return true;
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
                CacheData.CurrentUnitHealth.Add(c_CacheObject.RActorGuid, dThisCurrentHealth);
                CacheData.LastCheckedUnitHealth.Add(c_CacheObject.RActorGuid, iLastCheckedHealth);
            }
            else
            {
                CacheData.CurrentUnitHealth[c_CacheObject.RActorGuid] = dThisCurrentHealth;
                CacheData.LastCheckedUnitHealth[c_CacheObject.RActorGuid] = iLastCheckedHealth;
            }
        }
        private static void CacheObjectMinimapActive()
        {
            try
            {
                c_CacheObject.IsMinimapActive = c_diaObject.CommonData.GetAttribute<int>(ActorAttributeType.MinimapActive) > 0;
            }
            catch
            {
                // Stuff it

            }
        }
        private static void CacheObjectIsBountyObjective()
        {
            try
            {
                c_CacheObject.IsBountyObjective = (c_diaObject.CommonData.GetAttribute<int>(ActorAttributeType.BountyObjective) != 0);
                if (c_CacheObject.IsBountyObjective)
                    c_CacheObject.KillRange = c_CacheObject.RadiusDistance + 10f;
            }
            catch
            {
                // Stuff it
            }
        }



    }
}
