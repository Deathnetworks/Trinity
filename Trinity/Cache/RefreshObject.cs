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
        private static TrinityCacheObject CurrentCacheObject = new TrinityCacheObject();

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
        private static bool c_HasBeenNavigable = false;
        private static bool c_HasBeenRaycastable = false;
        private static bool c_HasBeenInLoS = false;
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
                    bool containsKey = CacheData.ObjectsIgnored.TryGetValue(CurrentCacheObject.RActorGuid, out reason);

                    if (CurrentCacheObject.RActorGuid != -1)
                    {
                        if (!containsKey) CacheData.ObjectsIgnored.Add(CurrentCacheObject.RActorGuid, c_IgnoreReason);
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
                CurrentCacheObject.InternalName = NameNumberTrimRegex.Replace(diaObject.Name, "");
            }
            catch
            {
                return GetReturnIgnore("InvalidName");
            }

            AddToCache = !IgnoreNames.Any(i => CurrentCacheObject.InternalName.ToLower().StartsWith(i, StringComparison.OrdinalIgnoreCase));
            if (!AddToCache)
            {
                return GetReturnIgnore("IgnoreName");
            }

            try
            {
                c_diaObject = diaObject;

                CurrentCacheObject.RActorGuid = c_diaObject.RActorGuid;
                CurrentCacheObject.ActorSNO = c_diaObject.ActorSNO;
                CurrentCacheObject.ActorType = c_diaObject.ActorType;
                CurrentCacheObject.ACDGuid = c_diaObject.ACDGuid;

                CurrentCacheObject.LastSeenTime = DateTime.UtcNow;
                CurrentCacheObject.Position = c_diaObject.Position;
            }
            catch
            {
                return GetReturnIgnore("InvalidObject");
            }

            using (new MemorySpy("CacheDiaObject().GetRadius"))
            {
                if (!DataDictionary.CustomObjectRadius.TryGetValue(CurrentCacheObject.ActorSNO, out c_Radius))
                    c_Radius = c_diaObject.CollisionSphere.Radius;

                CurrentCacheObject.Radius = c_Radius;
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

                if (CacheData.ObjectsIgnored.ContainsKey(CurrentCacheObject.RActorGuid))
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
                if (CurrentCacheObject.IsUnit || CurrentCacheObject.Type == GObjectType.Unknown)
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
                CurrentCacheObject.IsInLineOfSight = RefreshStepIgnoreLoS();
                if (!CurrentCacheObject.IsInLineOfSight)
                {
                    return GetReturnIgnore("IgnoreLoS", false);
                }
            }

            AddUnitToMonsterObstacleCache();

            c_IgnoreReason = "None";
            CurrentCacheObject.ACDGuid = CurrentCacheObject.ACDGuid;
            CurrentCacheObject.ActorSNO = CurrentCacheObject.ActorSNO;
            CurrentCacheObject.Animation = c_CurrentAnimation;
            CurrentCacheObject.DBItemBaseType = c_DBItemBaseType;
            CurrentCacheObject.DBItemType = c_DBItemType;
            CurrentCacheObject.DirectionVector = c_DirectionVector;
            CurrentCacheObject.Distance = CurrentCacheObject.Distance;
            CurrentCacheObject.DynamicID = CurrentCacheObject.DynamicID;
            CurrentCacheObject.FollowerType = c_item_tFollowerType;
            CurrentCacheObject.GameBalanceID = CurrentCacheObject.GameBalanceID;
            CurrentCacheObject.GoldAmount = c_GoldStackSize;
            CurrentCacheObject.HasAffixShielded = c_unit_HasShieldAffix;
            CurrentCacheObject.HasDotDPS = c_HasDotDPS;
            CurrentCacheObject.HitPoints = c_HitPoints;
            CurrentCacheObject.HitPointsPct = c_HitPointsPct;
            CurrentCacheObject.InternalName = CurrentCacheObject.InternalName;
            CurrentCacheObject.IsAttackable = c_unit_IsAttackable;
            CurrentCacheObject.IsElite = c_unit_IsElite;
            CurrentCacheObject.IsEliteRareUnique = c_IsEliteRareUnique;
            CurrentCacheObject.IsFacingPlayer = c_IsFacingPlayer;
            CurrentCacheObject.IsMinion = c_unit_IsMinion;
            CurrentCacheObject.IsRare = c_unit_IsRare;
            CurrentCacheObject.IsTreasureGoblin = c_unit_IsTreasureGoblin;
            CurrentCacheObject.IsUnique = c_unit_IsUnique;
            CurrentCacheObject.ItemLevel = c_ItemLevel;
            CurrentCacheObject.ItemLink = c_ItemLink;
            CurrentCacheObject.ItemQuality = c_ItemQuality;
            CurrentCacheObject.MonsterAffixes = c_MonsterAffixes;
            CurrentCacheObject.MonsterSize = c_unit_MonsterSize;
            CurrentCacheObject.OneHanded = c_IsOneHandedItem;
            CurrentCacheObject.RActorGuid = CurrentCacheObject.RActorGuid;
            CurrentCacheObject.Radius = CurrentCacheObject.Radius;
            CurrentCacheObject.Rotation = c_Rotation;
            CurrentCacheObject.TrinityItemType = c_item_GItemType;
            CurrentCacheObject.TwoHanded = c_IsTwoHandedItem;
            CurrentCacheObject.Type = CurrentCacheObject.Type;
            CurrentCacheObject.IsAncient = c_IsAncient;
            CurrentCacheObject.ZDiff = c_ZDiff;

            ObjectCache.Add(CurrentCacheObject);
            return true;
        }

        private static bool RefreshStepNavigationObstacle()
        {
            c_IsObstacle = DataDictionary.NavigationObstacleIds.Contains(CurrentCacheObject.ActorSNO);
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
            if (DataDictionary.ObstacleCustomRadius.ContainsKey(CurrentCacheObject.ActorSNO))
                CurrentCacheObject.Radius = DataDictionary.ObstacleCustomRadius[CurrentCacheObject.ActorSNO];

            MainGridProvider.AddCellWeightingObstacle(CurrentCacheObject.ActorSNO, CurrentCacheObject.Radius);

            CacheData.AddToNavigationObstacles(new CacheObstacleObject()
            {
                ActorSNO = CurrentCacheObject.ActorSNO,
                RActorGUID = CurrentCacheObject.RActorGuid,
                Name = CurrentCacheObject.InternalName,
                Position = CurrentCacheObject.Position,
                Radius = CurrentCacheObject.Radius,
                ObjectType = CurrentCacheObject.Type,
            });
        }
        /// <summary>
        /// Adds a unit to cache hashMonsterObstacleCache
        /// </summary>
        private static void AddUnitToMonsterObstacleCache()
        {
            if (CurrentCacheObject.Type == GObjectType.Unit)
            {
                // Add to the collision-list
                CacheData.MonsterObstacles.Add(new CacheObstacleObject()
                {
                    ActorSNO = CurrentCacheObject.ActorSNO,
                    Name = CurrentCacheObject.InternalName,
                    Position = CurrentCacheObject.Position,
                    Radius = CurrentCacheObject.Radius,
                    ObjectType = CurrentCacheObject.Type,
                });
            }
        }
        /// <summary>
        /// Initializes variable set for single object refresh
        /// </summary>
        private static void RefreshStepInit()
        {
            CurrentCacheObject = new TrinityCacheObject();
            CurrentCacheObject.Type = GObjectType.Unknown;
            CurrentCacheObject.Distance = -1f;
            CurrentCacheObject.Radius = 0f;
            CurrentCacheObject.ACDGuid = -1;
            CurrentCacheObject.RActorGuid = -1;
            CurrentCacheObject.DynamicID = -1;
            CurrentCacheObject.GameBalanceID = -1;
            CurrentCacheObject.ActorSNO = -1;

            c_ZDiff = 0f;
            c_ItemDisplayName = "";
            c_ItemLink = "";
            CurrentCacheObject.InternalName = "";
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
                isAvoidance = DataDictionary.ActorAvoidances.TryGetValue((SNOActor)CurrentCacheObject.ActorSNO, out c_Avoidance);
            }

            if (isAvoidance)
            {
                if (!Settings.Combat.Misc.AvoidAOE) { c_InfosSubStep += "AvoidanceDisabled "; return false; }
                CurrentCacheObject.Type = GObjectType.Avoidance;
            }
            else
            {
                using (new MemorySpy("StepObjectType().Check"))
                {
                    if (DataDictionary.ButcherFloorPanels.Contains(CurrentCacheObject.ActorSNO))
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
                            CurrentCacheObject.Type = GObjectType.Avoidance;
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
                        CurrentCacheObject.Type = GObjectType.Unit;
                    }
                    else if (c_diaObject.ActorType == ActorType.Player)
                    {
                        if (c_diaObject.ACDGuid != c_diaObject.CommonData.ACDGuid)
                        {
                            c_InfosSubStep += "InvalidUnitACD "; return false;
                        }

                        c_diaUnit = c_diaObject as DiaUnit;
                        CurrentCacheObject.Type = GObjectType.Player;
                    }
                    else if (DataDictionary.ForceToItemOverrideIds.Contains(CurrentCacheObject.ActorSNO) || (c_diaObject.ActorType == ActorType.Item))
                    {
                        if (c_diaObject.ACDGuid != c_diaObject.CommonData.ACDGuid)
                        {
                            c_InfosSubStep += "InvalidItemACD "; return false;
                        }

                        if (CurrentCacheObject.InternalName.ToLower().StartsWith("gold"))
                        {
                            CurrentCacheObject.Type = GObjectType.Gold;
                        }
                        else
                        {
                            CurrentCacheObject.Type = GObjectType.Item;
                        }
                    }
                    else if (DataDictionary.InteractWhiteListIds.Contains(CurrentCacheObject.ActorSNO))
                    {
                        CurrentCacheObject.Type = GObjectType.Interactable;
                    }
                    else if (c_diaObject.ActorType == ActorType.Gizmo && CurrentCacheObject.Distance <= 90)
                    {
                        c_diaGizmo = c_diaObject as DiaGizmo;

                        if (CurrentCacheObject.InternalName.Contains("CursedChest"))
                        {
                            CurrentCacheObject.Type = GObjectType.CursedChest;
                        }
                        else if (CurrentCacheObject.InternalName.Contains("CursedShrine"))
                        {
                            CurrentCacheObject.Type = GObjectType.CursedShrine;
                        }
                        else if (c_diaGizmo.IsBarricade)
                        {
                            CurrentCacheObject.Type = GObjectType.Barricade;
                        }
                        else
                        {
                            switch (c_diaGizmo.ActorInfo.GizmoType)
                            {
                                case GizmoType.HealingWell:
                                    CurrentCacheObject.Type = GObjectType.HealthWell;
                                    break;
                                case GizmoType.Door:
                                    CurrentCacheObject.Type = GObjectType.Door;
                                    break;
                                case GizmoType.PoolOfReflection:
                                case GizmoType.PowerUp:
                                    CurrentCacheObject.Type = GObjectType.Shrine;
                                    break;
                                case GizmoType.Chest:
                                    CurrentCacheObject.Type = GObjectType.Container;
                                    break;
                                case GizmoType.BreakableDoor:
                                    CurrentCacheObject.Type = GObjectType.Barricade;
                                    break;
                                case GizmoType.BreakableChest:
                                    CurrentCacheObject.Type = GObjectType.Destructible;
                                    break;
                                case GizmoType.DestroyableObject:
                                    CurrentCacheObject.Type = GObjectType.Destructible;
                                    break;
                                case GizmoType.PlacedLoot:
                                case GizmoType.Switch:
                                case GizmoType.Headstone:
                                    CurrentCacheObject.Type = GObjectType.Interactable;
                                    break;
                                default:
                                    CurrentCacheObject.Type = GObjectType.Unknown;
                                    break;
                            }
                        }
                    }
                }
            }

            return true;
        }

        private static void RefreshStepMainObjectType(ref bool addToCache)
        {
            // Now do stuff specific to object types
            switch (CurrentCacheObject.Type)
            {
                case GObjectType.Player:
                    {
                        using (new MemorySpy("StepMainObjectType().Player"))
                        {
                            addToCache = RefreshUnit();
                        }
                        break;
                    }
                // Handle Unit-type Objects
                case GObjectType.Unit:
                    {
                        if (!Combat.Abilities.CombatBase.IsCombatAllowed)
                        {
                            c_InfosSubStep += "CombatDisabled"; addToCache = false;
                            break;
                        }

                        using (new MemorySpy("StepMainObjectType().UnitAAvoidance"))
                        {
                            RAAvoidances();
                        }
                        using (new MemorySpy("StepMainObjectType().Unit"))
                        {
                            addToCache = RefreshUnit();
                        }
                        break;
                    }
                // Handle Item-type Objects
                case GObjectType.Item:
                    {
                        if (TrinityItemManager.FindValidBackpackLocation(true) == new Vector2(-1, -1))
                        {
                            c_InfosSubStep += "NoFreeSlots"; addToCache = false;
                            break;
                        }

                        using (new MemorySpy("StepMainObjectType().Item"))
                        {
                            addToCache = RefreshItem();
                        }
                        break;
                    }
                // Handle Gold
                case GObjectType.Gold:
                    {
                        using (new MemorySpy("StepMainObjectType().Gold"))
                        {
                            addToCache = RefreshGold();
                        }
                        break;
                    }
                case GObjectType.PowerGlobe:
                case GObjectType.HealthGlobe:
                case GObjectType.ProgressionGlobe:
                    {
                        addToCache = true;
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
                            addToCache = RefreshAvoidance();
                        }
                        break;
                    }
                // Handle Door
                case GObjectType.Door:
                    {
                        CurrentCacheObject.Radius = 30f;

                        using (new MemorySpy("StepMainObjectType().Gizmo"))
                        {
                            addToCache = RefreshGizmo(addToCache);
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
                        using (new MemorySpy("StepMainObjectType().Gizmo"))
                        {
                            addToCache = RefreshGizmo(addToCache);
                        }
                        break;
                    }
                // Object switch on type (to seperate shrines, destructibles, barricades etc.)
                default:
                    {
                        using (new MemorySpy("StepMainObjectType().UnknownAAvoidance")) { RAAvoidances(); }
                        DebugUtil.LogUnknown(c_diaObject);

                        c_InfosSubStep += "Unknown." + c_diaObject.ActorType;
                        addToCache = false;
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
            bool addToCache = true;
            try
            {
                if (CurrentCacheObject.Type == GObjectType.Item || CurrentCacheObject.Type == GObjectType.Gold)
                    return true;

                // No need for raycasting in certain level areas (rift trial for example)
                if (DataDictionary.NeverRaycastLevelAreaIds.Contains(Player.LevelAreaId))
                {
                    c_HasBeenRaycastable = true;
                    if (!CacheData.HasBeenRayCasted.ContainsKey(CurrentCacheObject.RActorGuid))
                        CacheData.HasBeenRayCasted.Add(CurrentCacheObject.RActorGuid, c_HasBeenRaycastable);
                    return true;
                }

                if (!DataDictionary.AlwaysRaycastWorlds.Contains(Player.WorldID))
                {
                    // Bounty Objectives should always be on the weight list
                    if (CurrentCacheObject.IsBountyObjective)
                        return true;

                    // Quest Monsters should get LoS white-listed
                    if (CurrentCacheObject.IsQuestMonster)
                        return true;

                    // Always LoS Units during events
                    if (CurrentCacheObject.Type == GObjectType.Unit && Player.InActiveEvent)
                        return true;
                }
                // Everything except items and the current target
                if (CurrentCacheObject.RActorGuid != LastTargetRactorGUID && CurrentCacheObject.Type != GObjectType.Unknown)
                {
                    if (CurrentCacheObject.Distance < 95)
                    {
                        using (new PerformanceLogger("RefreshLoS.2"))
                        {
                            // Get whether or not this RActor has ever been in a path line with AllowWalk. If it hasn't, don't add to cache and keep rechecking
                            if (!CacheData.HasBeenRayCasted.TryGetValue(CurrentCacheObject.RActorGuid, out c_HasBeenRaycastable) || DataDictionary.AlwaysRaycastWorlds.Contains(Player.WorldID))
                            {
                                if (CurrentCacheObject.Distance >= 1f && CurrentCacheObject.Distance <= 5f)
                                {
                                    c_HasBeenRaycastable = true;
                                    if (!CacheData.HasBeenRayCasted.ContainsKey(CurrentCacheObject.RActorGuid))
                                        CacheData.HasBeenRayCasted.Add(CurrentCacheObject.RActorGuid, c_HasBeenRaycastable);
                                }
                                else if (Settings.Combat.Misc.UseNavMeshTargeting)
                                {
                                    Vector3 myPos = new Vector3(Player.Position.X, Player.Position.Y, Player.Position.Z + 8f);
                                    Vector3 cPos = new Vector3(CurrentCacheObject.Position.X, CurrentCacheObject.Position.Y, CurrentCacheObject.Position.Z + 8f);
                                    cPos = MathEx.CalculatePointFrom(cPos, myPos, CurrentCacheObject.Radius + 1f);

                                    if (Single.IsNaN(cPos.X) || Single.IsNaN(cPos.Y) || Single.IsNaN(cPos.Z))
                                        cPos = CurrentCacheObject.Position;

                                    if (!NavHelper.CanRayCast(myPos, cPos))
                                    {
                                        c_InfosSubStep += "UnableToRayCast";
                                        addToCache = false;
                                    }
                                    else
                                    {
                                        c_HasBeenRaycastable = true;
                                        if (!CacheData.HasBeenRayCasted.ContainsKey(CurrentCacheObject.RActorGuid))
                                            CacheData.HasBeenRayCasted.Add(CurrentCacheObject.RActorGuid, c_HasBeenRaycastable);
                                    }
                                }
                                else
                                {
                                    if (c_ZDiff > 14f)
                                    {
                                        c_InfosSubStep += "LoS.ZDiff";
                                        addToCache = false;
                                    }
                                    else
                                    {
                                        c_HasBeenRaycastable = true;
                                        if (!CacheData.HasBeenRayCasted.ContainsKey(CurrentCacheObject.RActorGuid))
                                            CacheData.HasBeenRayCasted.Add(CurrentCacheObject.RActorGuid, c_HasBeenRaycastable);
                                    }

                                }
                            }
                        }
                        using (new PerformanceLogger("RefreshLoS.3"))
                        {
                            // Get whether or not this RActor has ever been in "Line of Sight" (as determined by Demonbuddy). If it hasn't, don't add to cache and keep rechecking
                            if (!CacheData.HasBeenInLoS.TryGetValue(CurrentCacheObject.RActorGuid, out c_HasBeenInLoS) || DataDictionary.AlwaysRaycastWorlds.Contains(Player.WorldID))
                            {
                                // Ignore units not in LoS except bosses
                                if (!CurrentCacheObject.IsBoss && !c_diaObject.InLineOfSight)
                                {
                                    c_InfosSubStep += "NotInLoS";
                                }
                                else
                                {
                                    c_HasBeenInLoS = true;
                                    if (!CacheData.HasBeenInLoS.ContainsKey(CurrentCacheObject.RActorGuid))
                                        CacheData.HasBeenInLoS.Add(CurrentCacheObject.RActorGuid, c_HasBeenInLoS);
                                }

                            }
                        }
                    }
                    else
                    {
                        c_InfosSubStep += "LoS-OutOfRange";
                    }


                    // always set true for bosses nearby
                    if (CurrentCacheObject.IsBoss || CurrentCacheObject.IsQuestMonster || CurrentCacheObject.IsBountyObjective)
                    {
                        addToCache = true;
                    }
                    // always take the current target even if not in LoS
                    if (CurrentCacheObject.RActorGuid == LastTargetRactorGUID)
                    {
                        addToCache = true;
                    }
                }

                // Simple whitelist for LoS 
                if (DataDictionary.LineOfSightWhitelist.Contains(CurrentCacheObject.ActorSNO))
                {
                    addToCache = true;
                }
                // Always pickup Infernal Keys whether or not in LoS
                if (DataDictionary.ForceToItemOverrideIds.Contains(CurrentCacheObject.ActorSNO))
                {
                    addToCache = true;
                }

            }
            catch (Exception ex)
            {
                addToCache = true;
                c_InfosSubStep += "IgnoreLoSException";
                Logger.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "{0}", ex);
            }
            return addToCache;
        }

        private static bool RefreshStepIgnoreUnknown(bool addToCache)
        {
            // We couldn't get a valid object type, so ignore it
            if (CurrentCacheObject.Type == GObjectType.Unknown)
            {
                addToCache = false;
            }
            return addToCache;
        }

        private static bool RefreshStepObjectTypeZDiff(bool AddToCache)
        {
            c_ZDiff = c_diaObject.ZDiff;
            // always take current target regardless if ZDiff changed
            if (CurrentCacheObject.RActorGuid == LastTargetRactorGUID)
            {
                AddToCache = true;
                return AddToCache;
            }

            // Ignore stuff which has a Z-height-difference too great, it's probably on a different level etc. - though not avoidance!
            if (CurrentCacheObject.Type != GObjectType.Avoidance)
            {
                // Calculate the z-height difference between our current position, and this object's position
                switch (CurrentCacheObject.Type)
                {
                    case GObjectType.Door:
                    case GObjectType.Unit:
                    case GObjectType.Barricade:
                        // Ignore monsters (units) who's Z-height is 14 foot or more than our own z-height except bosses
                        if (c_ZDiff >= 11f && !CurrentCacheObject.IsBoss)
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
            if (!CurrentCacheObject.IsAvoidance && !CurrentCacheObject.IsBountyObjective && !CurrentCacheObject.IsQuestMonster)
            {
                // Temporary ractor GUID ignoring, to prevent 2 interactions in a very short time which can cause stucks
                if (_ignoreTargetForLoops > 0 && _ignoreRactorGuid == CurrentCacheObject.RActorGuid)
                {
                    c_InfosSubStep += "IgnoreRactorGUID ";
                    return false;
                }
                // Check our extremely short-term destructible-blacklist
                if (_destructible3SecBlacklist.Contains(CurrentCacheObject.RActorGuid))
                {
                    c_InfosSubStep += "Destructible3SecBlacklist ";
                    return false;
                }
                // Check our extremely short-term destructible-blacklist
                if (Blacklist1Second.Contains(CurrentCacheObject.RActorGuid))
                {
                    c_InfosSubStep += "hashRGUIDBlacklist1 ";
                    return false;
                }
                // Check our extremely short-term destructible-blacklist
                if (Blacklist3Seconds.Contains(CurrentCacheObject.RActorGuid))
                {
                    c_InfosSubStep += "hashRGUIDBlacklist3 ";
                    return false;
                }
                // See if it's on our 90 second blacklist (from being stuck targeting it), as long as it's distance is not extremely close
                if (Blacklist90Seconds.Contains(CurrentCacheObject.RActorGuid))
                {
                    c_InfosSubStep += "Blacklist90Seconds ";
                    return false;
                }
                // 60 second blacklist
                if (Blacklist60Seconds.Contains(CurrentCacheObject.RActorGuid))
                {
                    c_InfosSubStep += "Blacklist60Seconds ";
                    return false;
                }
                // 15 second blacklist
                if (Blacklist15Seconds.Contains(CurrentCacheObject.RActorGuid))
                {
                    c_InfosSubStep += "Blacklist15Seconds ";
                    return false;
                }
                // See if it's something we should always ignore like ravens etc.
                if (DataDictionary.BlackListIds.Contains(CurrentCacheObject.ActorSNO))
                {
                    c_InfosSubStep += "Blacklist ";
                    return false;
                }
            }

            if (CurrentCacheObject.Type != GObjectType.Item)
            {
                CurrentCacheObject.ObjectHash = HashGenerator.GenerateWorldObjectHash(CurrentCacheObject.ActorSNO, CurrentCacheObject.Position, CurrentCacheObject.Type.ToString(), Trinity.CurrentWorldDynamicId);
                if (CurrentCacheObject.ObjectHash != String.Empty && GenericBlacklist.ContainsKey(CurrentCacheObject.ObjectHash))
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
                CacheData.CurrentUnitHealth.Add(CurrentCacheObject.RActorGuid, dThisCurrentHealth);
                CacheData.LastCheckedUnitHealth.Add(CurrentCacheObject.RActorGuid, iLastCheckedHealth);
            }
            else
            {
                CacheData.CurrentUnitHealth[CurrentCacheObject.RActorGuid] = dThisCurrentHealth;
                CacheData.LastCheckedUnitHealth[CurrentCacheObject.RActorGuid] = iLastCheckedHealth;
            }
        }
        private static void CacheObjectMinimapActive()
        {
            try
            {
                CurrentCacheObject.IsMinimapActive = c_diaObject.CommonData.GetAttribute<int>(ActorAttributeType.MinimapActive) > 0;
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
                CurrentCacheObject.IsBountyObjective = (c_diaObject.CommonData.GetAttribute<int>(ActorAttributeType.BountyObjective) != 0);
                if (CurrentCacheObject.IsBountyObjective)
                    CurrentCacheObject.KillRange = CurrentCacheObject.RadiusDistance + 10f;
            }
            catch
            {
                // Stuff it
            }
        }



    }
}
