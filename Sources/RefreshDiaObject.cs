using System;
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
        private static bool CacheDiaObject(DiaObject thisObj)
        {
            /*
             *  Initialize Variables
             */
            bool bWantThis;
            int iCurrentMinimumStackSize;
            double iPercentage;
            RefreshStepInit(out bWantThis, out iCurrentMinimumStackSize, out iPercentage);
            /*
             *  Get primary reference objects and keys
             */
            c_diaObject = thisObj;
            // Set Common Data
            bWantThis = RefreshStepGetCommonData(thisObj);
            if (!bWantThis) { c_IgnoreReason = "GetCommonData"; return bWantThis; }
            // Ractor GUID
            c_iRActorGuid = thisObj.RActorGuid;
            // Check to see if we've already looked at this GUID
            bWantThis = RefreshStepSkipDoubleCheckGuid(bWantThis);
            if (!bWantThis) { c_IgnoreReason = "SkipDoubleCheckGuid"; return bWantThis; }
            // ActorSNO
            bWantThis = RefreshStepCachedActorSNO(bWantThis, thisObj);
            if (!bWantThis) { c_IgnoreReason = "CachedActorSNO"; return bWantThis; }
            // Get Internal Name
            bWantThis = RefreshInternalName(bWantThis, thisObj);
            if (!bWantThis) { c_IgnoreReason = "InternalName"; return bWantThis; }
            // Summons by the player 
            bWantThis = RefreshStepCachedPlayerSummons(bWantThis, c_CommonData);
            if (!bWantThis) { c_IgnoreReason = "CachedPlayerSummons"; return bWantThis; }
            // Check for navigation obstacle hashlist
            c_bIsObstacle = hashSNONavigationObstacles.Contains(c_iActorSNO);
            /*
             * Begin main refresh routine
             */
            // Set Giles Object Type
            bWantThis = RefreshStepCachedObjectType(bWantThis, thisObj, c_CommonData, c_bIsObstacle);
            if (!bWantThis) { c_IgnoreReason = "CachedObjectType"; return bWantThis; }
            // Check Blacklists
            bWantThis = RefreshStepCheckBlacklists(bWantThis);
            if (!bWantThis) { c_IgnoreReason = "CheckBlacklists"; return bWantThis; }
            // Get ACDGuid
            bWantThis = RefreshStepCachedACDGuid(bWantThis, thisObj, c_bIsObstacle);
            if (!bWantThis) { c_IgnoreReason = "CachedACDGuid"; return bWantThis; }
            // Get Cached Position
            bWantThis = RefreshStepCachedPosition(bWantThis, thisObj);
            if (!bWantThis) { c_IgnoreReason = "CachedPosition"; return bWantThis; }
            // Get Fresh Distance
            RefreshStepCalculateDistance();
            // Add new Obstacle to cache
            bWantThis = RefreshStepAddObstacleToCache(bWantThis, c_bIsObstacle);
            if (!bWantThis) { c_IgnoreReason = "AddObstacleToCache"; return bWantThis; }
            // Get DynamicId and GameBalanceId
            bWantThis = RefreshStepCachedDynamicIds(bWantThis, c_CommonData);
            if (!bWantThis) { c_IgnoreReason = "CachedDynamicIds"; return bWantThis; }
            /* 
             * Main Switch on Object Type - Refresh individual object types (Units, Items, Gizmos)
             */
            RefreshStepMainObjectType(thisObj, ref bWantThis, c_CommonData, ref iCurrentMinimumStackSize, ref iPercentage);
            if (!bWantThis) { c_IgnoreReason = "MainObjectType"; return bWantThis; }
            // Nothing we want this loop!
            // Always Get ZDiff
            bWantThis = RefreshStepNewObjectTypeZDiff(bWantThis);
            if (!bWantThis) { c_IgnoreReason = "ZDiff"; return bWantThis; }
            // Ignore all LoS
            bWantThis = RefreshStepIgnoreLoS(thisObj, bWantThis);
            if (!bWantThis) { c_IgnoreReason = "IgnoreLoS"; return bWantThis; }
            // Ignore anything unknown
            bWantThis = RefreshStepIgnoreUnknown(bWantThis, c_bIsObstacle);
            if (!bWantThis) { c_IgnoreReason = "IgnoreUnknown"; return bWantThis; }
            // Double Check Blacklists
            bWantThis = RefreshStepCheckBlacklists(bWantThis);
            if (!bWantThis) { c_IgnoreReason = "DoubleCheckBlacklists"; return bWantThis; }
            // If it's a unit, add it to the monster cache
            AddUnitToCache(bWantThis);
            if (bWantThis)
            {
                c_IgnoreReason = "None";
                listGilesObjectCache.Add(new GilesObject(c_vPosition, c_ObjectType, c_dWeight, c_fCentreDistance, c_fRadiusDistance, c_sName,
                    c_iACDGUID, c_iRActorGuid, c_iDynamicID, c_iBalanceID, c_iActorSNO, c_item_iLevel, c_item_iGoldStackSize, c_item_bOneHanded,
                    c_item_tQuality, c_item_tDBItemType, c_item_tFollowerType, c_item_GilesItemType, c_unit_bIsElite, c_unit_bIsRare, c_unit_bIsUnique,
                    c_unit_bIsMinion, c_unit_bIsTreasureGoblin, c_unit_bIsBoss, c_unit_bIsAttackable, c_unit_dHitPoints, c_fRadius,
                    c_unit_MonsterSize, c_diaObject, c_bIsEliteRareUnique, c_bForceLeapAgainst));
            }
            return true;
        }
        private static void AddUnitToCache(bool bWantThis)
        {
            if (bWantThis && c_ObjectType == GilesObjectType.Unit)
            {
                // Handle bosses
                if (c_unit_bIsBoss)
                {
                    // Force to elite and add to the collision list
                    c_unit_bIsElite = true;
                    hashMonsterObstacleCache.Add(new GilesObstacle(c_vPosition, (c_fRadius * 0.15f), c_iActorSNO));
                }
                else
                {
                    // Add to the collision-list
                    hashMonsterObstacleCache.Add(new GilesObstacle(c_vPosition, (c_fRadius * 0.10f), c_iActorSNO));
                }
            }
        }
        private static void RefreshStepInit(out bool bWantThis, out int iCurrentMinimumStackSize, out double iPercentage)
        {
            bWantThis = true;
            // Start this object as off as unknown type
            c_ObjectType = GilesObjectType.Unknown;
            // We will set weight up later in RefreshDiaObjects after we process all valid items
            c_dWeight = 0;
            // Variables used by multiple parts of the switch below
            iCurrentMinimumStackSize = 0;
            iPercentage = 0;
            c_vPosition = Vector3.Zero;
            c_ObjectType = GilesObjectType.Unknown;
            c_dWeight = 0d;
            c_fCentreDistance = 0f;
            c_fRadiusDistance = 0f;
            c_fRadius = 0f;
            c_fZDiff = 0f;
            c_sName = "";
            c_IgnoreReason = "";
            c_IgnoreSubStep = "";
            c_iACDGUID = -1;
            c_iRActorGuid = -1;
            c_iDynamicID = -1;
            c_iBalanceID = -1;
            c_iActorSNO = -1;
            c_item_iLevel = -1;
            c_item_iGoldStackSize = -1;
            c_unit_dHitPoints = -1;
            c_item_bOneHanded = false;
            c_unit_bIsElite = false;
            c_unit_bIsRare = false;
            c_unit_bIsUnique = false;
            c_unit_bIsMinion = false;
            c_unit_bIsTreasureGoblin = false;
            c_unit_bIsBoss = false;
            c_unit_bIsAttackable = false;
            c_bIsEliteRareUnique = false;
            c_bForceLeapAgainst = false;
            c_bIsObstacle = false;
            c_item_tQuality = ItemQuality.Invalid;
            c_item_tDBItemType = ItemType.Unknown;
            c_item_tFollowerType = FollowerType.None;
            c_item_GilesItemType = GilesItemType.Unknown;
            c_unit_MonsterSize = MonsterSize.Unknown;
            c_diaObject = null;
        }
        private static bool RefreshStepCachedActorSNO(bool bWantThis, DiaObject thisobj)
        {
            // Get the Actor SNO, cached if possible
            if (!dictGilesActorSNOCache.TryGetValue(c_iRActorGuid, out c_iActorSNO))
            {
                try
                {
                    c_iActorSNO = thisobj.ActorSNO;
                }
                catch (Exception ex)
                {
                    Logging.WriteDiagnostic("[Trinity] Safely handled exception getting ActorSNO for an object.");
                    Logging.WriteDiagnostic(ex.ToString());
                    bWantThis = false;
                    //return bWantThis;
                }
                dictGilesActorSNOCache.Add(c_iRActorGuid, c_iActorSNO);
            }
            return bWantThis;
        }
        private static bool RefreshInternalName(bool bWantThis, DiaObject thisobj)
        {
            // This is "internalname" for items, and just a "generic" name for objects and units - cached if possible
            if (!dictGilesInternalNameCache.TryGetValue(c_iRActorGuid, out c_sName))
            {
                try
                {
                    c_sName = nameNumberTrimRegex.Replace(thisobj.Name, "");
                }
                catch (Exception ex)
                {
                    Logging.WriteDiagnostic("[Trinity] Safely handled exception getting InternalName for an object [" + c_iActorSNO.ToString() + "]");
                    Logging.WriteDiagnostic(ex.ToString());
                    bWantThis = false;
                    //return bWantThis;
                }
                dictGilesInternalNameCache.Add(c_iRActorGuid, c_sName);
            }
            return bWantThis;
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
        private static bool RefreshStepIgnoreNullCommonData(bool bWantThis, ACD tempCommonData)
        {
            // Null Common Data makes a DiaUseless!
            if (c_ObjectType == GilesObjectType.Unit || c_ObjectType == GilesObjectType.Item || c_ObjectType == GilesObjectType.Gold)
            {
                if (tempCommonData == null)
                {
                    bWantThis = false;
                    //return bWantThis;
                }
                if (tempCommonData != null && !tempCommonData.IsValid)
                {
                    bWantThis = false;
                }
            }
            return bWantThis;
        }
        private static bool RefreshStepAddObstacleToCache(bool bWantThis, bool bThisObstacle)
        {
            // Have Position, Now store the location etc. of this obstacle and continue
            if (bThisObstacle)
            {
                hashNavigationObstacleCache.Add(new GilesObstacle(c_vPosition, dictSNONavigationSize[c_iActorSNO], c_iActorSNO));
                bWantThis = false;
            }
            return bWantThis;
        }
        private static void RefreshStepCalculateDistance()
        {
            // Calculate distance, don't rely on DB's internal method as this may hit Diablo 3 memory again
            c_fCentreDistance = Vector3.Distance(playerStatus.vCurrentPosition, c_vPosition);
            // Set radius-distance to centre distance at first
            c_fRadiusDistance = c_fCentreDistance;
        }
        private static bool RefreshStepSkipDoubleCheckGuid(bool bWantThis)
        {
            // See if we've already checked this ractor, this loop
            if (hashDoneThisRactor.Contains(c_iRActorGuid))
            {
                bWantThis = false;
                //return bWantThis;
            }
            else
            {
                hashDoneThisRactor.Add(c_iRActorGuid);
            }
            return bWantThis;
        }
        private static bool RefreshStepCachedObjectType(bool bWantThis, DiaObject thisobj, ACD tempCommonData, bool bThisObstacle)
        {
            // Set the object type
            // begin with default... 
            c_ObjectType = GilesObjectType.Unknown;
            // Either get the cached Giles object type, or calculate it fresh
            if (!bThisObstacle && !dictGilesObjectTypeCache.TryGetValue(c_iRActorGuid, out c_ObjectType))
            {
                // See if it's an avoidance first from the SNO
                if (hashAvoidanceSNOList.Contains(c_iActorSNO) || hashAvoidanceBuffSNOList.Contains(c_iActorSNO) || hashAvoidanceSNOProjectiles.Contains(c_iActorSNO))
                {
                    // If avoidance is disabled, ignore this avoidance stuff
                    if (!settings.bEnableAvoidance)
                    {
                        bWantThis = false;
                        //return bWantThis;
                    }
                    // Checking for BuffVisualEffect - for Butcher, maybe useful other places?
                    if (hashAvoidanceBuffSNOList.Contains(c_iActorSNO))
                    {
                        bool hasBuff = false;
                        try
                        {
                            hasBuff = tempCommonData.GetAttribute<int>(ActorAttributeType.BuffVisualEffect) > 0;
                        }
                        catch { }
                        if (hasBuff)
                        {
                            bWantThis = true;
                            c_ObjectType = GilesObjectType.Avoidance;
                        }
                        else
                        {
                            dictGilesObjectTypeCache.Remove(c_iRActorGuid);
                            bWantThis = false;
                        }
                    }
                    else
                    {
                        // Avoidance isn't disabled, so set this object type to avoidance
                        c_ObjectType = GilesObjectType.Avoidance;
                    }
                }
                // It's not an avoidance, so let's calculate it's object type "properly"
                else
                {
                    // Calculate the object type of this object
                    //if (thisobj is DiaUnit && thisobj.ActorType == ActorType.Unit)                    
                    if (thisobj.ActorType == ActorType.Unit)
                    {
                        if (tempCommonData == null)
                        {
                            bWantThis = false;
                        }
                        if (tempCommonData != null && thisobj.ACDGuid != tempCommonData.ACDGuid)
                        {
                            bWantThis = false;
                        }
                        c_ObjectType = GilesObjectType.Unit;
                    }
                    else if (hashForceSNOToItemList.Contains(c_iActorSNO) || thisobj.ActorType == ActorType.Item)
                    {
                        if (tempCommonData == null)
                        {
                            bWantThis = false;
                        }
                        if (thisobj.ACDGuid != tempCommonData.ACDGuid)
                        {
                            bWantThis = false;
                        }
                        if (c_sName.ToLower().StartsWith("gold"))
                        {
                            c_ObjectType = GilesObjectType.Gold;
                        }
                        else
                        {
                            c_ObjectType = GilesObjectType.Item;
                        }
                    }
                    else if (thisobj.ActorType == ActorType.Gizmo)
                    {
                        if (thisobj.ActorInfo.GizmoType == GizmoType.Shrine)
                            c_ObjectType = GilesObjectType.Shrine;
                        else if (thisobj.ActorInfo.GizmoType == GizmoType.Healthwell)
                            c_ObjectType = GilesObjectType.HealthWell;
                        else if (thisobj.ActorInfo.GizmoType == GizmoType.DestructibleLootContainer)
                            c_ObjectType = GilesObjectType.Destructible;
                        else if (thisobj.ActorInfo.GizmoType == GizmoType.Destructible)
                            c_ObjectType = GilesObjectType.Destructible;
                        else if (thisobj.ActorInfo.GizmoType == GizmoType.Barricade)
                            c_ObjectType = GilesObjectType.Barricade;
                        else if (thisobj.ActorInfo.GizmoType == GizmoType.LootContainer)
                            c_ObjectType = GilesObjectType.Container;
                        else if (hashSNOInteractWhitelist.Contains(c_iActorSNO))
                            c_ObjectType = GilesObjectType.Interactable;
                        else if (thisobj.ActorInfo.GizmoType == GizmoType.Door)
                            c_ObjectType = GilesObjectType.Door;
                        else
                            c_ObjectType = GilesObjectType.Unknown;
                    }
                    else
                        c_ObjectType = GilesObjectType.Unknown;
                }
                // Now cache the object type
                dictGilesObjectTypeCache.Add(c_iRActorGuid, c_ObjectType);
            }
            return bWantThis;
        }
        private static void RefreshStepMainObjectType(DiaObject thisobj, ref bool bWantThis, ACD tempCommonData, ref int iCurrentMinimumStackSize, ref double iPercentage)
        {
            // Now do stuff specific to object types
            switch (c_ObjectType)
            {
                // Handle Unit-type Objects
                case GilesObjectType.Unit:
                    {
                        bWantThis = RefreshGilesUnit(bWantThis, thisobj, tempCommonData);
                        break;
                    }
                // Handle Item-type Objects
                case GilesObjectType.Item:
                    {
                        if (!bGilesForcedVendoring)
                        {
                            bWantThis = RefreshGilesItem(bWantThis, thisobj, tempCommonData, iCurrentMinimumStackSize, iPercentage);
                            c_IgnoreSubStep = "RefreshGilesItem";
                        }
                        else
                        {
                            bWantThis = false;
                            c_IgnoreSubStep = "bGilesForcedVendoring";
                        }
                        break;

                    }
                // Handle Gold
                // NOTE: Only identified as gold after *FIRST* loop as an "item" by above code
                case GilesObjectType.Gold:
                    {
                        bWantThis = RefreshGilesGold(bWantThis, tempCommonData, out iCurrentMinimumStackSize, out iPercentage);
                        c_IgnoreSubStep = "RefreshGilesGold";
                        break;
                    }
                // Handle Globes
                // NOTE: Only identified as globe after *FIRST* loop as an "item" by above code
                case GilesObjectType.Globe:
                    {
                        // Ignore it if it's not in range yet
                        if (c_fCentreDistance > iCurrentMaxLootRadius || c_fCentreDistance > 37f)
                        {
                            c_IgnoreSubStep = "GlobeOutOfRange";
                            bWantThis = false;
                        }
                        bWantThis = true;
                        break;
                    }
                // Handle Avoidance Objects
                case GilesObjectType.Avoidance:
                    {
                        bWantThis = RefreshGilesAvoidance(bWantThis);
                        if (!bWantThis) { c_IgnoreSubStep = "RefreshGilesAvoidance"; }

                        break;
                    }
                // Handle Other-type Objects
                case GilesObjectType.Destructible:
                case GilesObjectType.Door:
                case GilesObjectType.Barricade:
                case GilesObjectType.Container:
                case GilesObjectType.Shrine:
                case GilesObjectType.Interactable:
                case GilesObjectType.HealthWell:
                    {
                        bWantThis = RefreshGilesGizmo(bWantThis, thisobj, tempCommonData);
                        break;
                    }
                // Object switch on type (to seperate shrines, destructibles, barricades etc.)
                case GilesObjectType.Unknown:
                default:
                    {
                        break;
                    }
            }
            // Main huge switch on object type (core types - units, items, objects)
        }
        private static bool RefreshStepIgnoreLoS(DiaObject thisobj, bool bWantThis = false)
        {
            // NOTHING that's not in line of site.
            try
            {
                //if (tmp_ThisGilesObjectType == GilesObjectType.Item ||
                //    tmp_ThisGilesObjectType == GilesObjectType.Unit ||
                //    tmp_ThisGilesObjectType == GilesObjectType.Gold ||
                //    tmp_ThisGilesObjectType == GilesObjectType.Globe ||
                //    tmp_ThisGilesObjectType == GilesObjectType.Avoidance ||
                //    tmp_ThisGilesObjectType == GilesObjectType.HealthWell)
                //{
                //    return true;
                //}
                bool isNavigable = pf.IsNavigable(gp.WorldToGrid(c_vPosition.ToVector2()));
                if (!isNavigable)
                {
                    bWantThis = false;
                    c_IgnoreSubStep = "NotNavigable";
                }
                if (c_ObjectType == GilesObjectType.Unit && !c_diaObject.InLineOfSight && !c_unit_bIsBoss)
                {
                    bWantThis = false;
                    c_IgnoreSubStep = "UnitNotInLoS";
                }
                // always set true for bosses nearby
                if (c_unit_bIsBoss && c_fRadiusDistance < 100f)
                {
                    bWantThis = true;
                    c_IgnoreSubStep = "";
                }
                // always take the current target even if not in LoS
                if (c_iRActorGuid == iCurrentTargetRactorGUID)
                {
                    bWantThis = true;
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
                bWantThis = false;
                c_IgnoreSubStep = "IgnoreLoSException";
            }
            return bWantThis;
        }
        private static bool RefreshStepIgnoreUnknown(bool bWantThis, bool bThisObstacle)
        {
            // We couldn't get a valid object type, so ignore it
            if (!bThisObstacle && c_ObjectType == GilesObjectType.Unknown)
            {
                bWantThis = false;
                //return bWantThis;
            }
            return bWantThis;
        }
        private static bool RefreshStepCachedACDGuid(bool bWantThis, DiaObject thisobj, bool bThisObstacle)
        {
            // Get the ACDGUID, cached if possible, only for non-avoidance stuff
            if (!bThisObstacle && c_ObjectType != GilesObjectType.Avoidance)
            {
                if (!dictGilesACDGUIDCache.TryGetValue(c_iRActorGuid, out c_iACDGUID))
                {
                    try
                    {
                        c_iACDGUID = thisobj.ACDGuid;
                    }
                    catch (Exception ex)
                    {
                        Logging.WriteDiagnostic("[Trinity] Safely handled exception getting ACDGUID for an object [" + c_iActorSNO.ToString() + "]");
                        Logging.WriteDiagnostic(ex.ToString());
                        bWantThis = false;
                        //return bWantThis;
                    }
                    dictGilesACDGUIDCache.Add(c_iRActorGuid, c_iACDGUID);
                }
                // No ACDGUID, so shouldn't be anything we want to deal with
                if (c_iACDGUID == -1)
                {
                    bWantThis = false;
                    //return bWantThis;
                }
            }
            else
            {
                // Give AOE's -1 ACDGUID, since it's not needed for avoidance stuff
                c_iACDGUID = -1;
            }
            return bWantThis;
        }
        private static bool RefreshStepCachedPosition(bool bWantThis, DiaObject thisobj)
        {
            // Try and get a cached position for anything that isn't avoidance or units (avoidance and units can move, sadly, so we risk DB mis-reads for those things!
            if (c_ObjectType != GilesObjectType.Avoidance && c_ObjectType != GilesObjectType.Unit)
            {
                // Get the position, cached if possible
                if (!dictGilesVectorCache.TryGetValue(c_iRActorGuid, out c_vPosition))
                {
                    try
                    {
                        //c_vPosition = thisobj.Position;
                        Vector3 pos = thisobj.Position;

                        // always get Height of wherever the nav says it is (for flying things..)
                        c_vPosition = new Vector3(pos.X, pos.Y, gp.GetHeight(pos.ToVector2()));
                    }
                    catch (Exception ex)
                    {
                        Logging.WriteDiagnostic("[Trinity] Safely handled exception getting position for a static object [" + c_iActorSNO.ToString() + "]");
                        Logging.WriteDiagnostic(ex.ToString());
                        bWantThis = false;
                    }
                    // Now cache it
                    dictGilesVectorCache.Add(c_iRActorGuid, c_vPosition);
                }
            }
            // Ok pull up live-position data for units/avoidance now...
            else
            {
                try
                {
                    c_vPosition = thisobj.Position;
                }
                catch (Exception ex)
                {
                    Logging.WriteDiagnostic("[Trinity] Safely handled exception getting position for a unit or avoidance object [" + c_iActorSNO.ToString() + "]");
                    Logging.WriteDiagnostic(ex.ToString());
                }
            }
            return bWantThis;
        }
        private static bool RefreshStepCachedDynamicIds(bool bWantThis, ACD tempCommonData)
        {
            // Try and grab the dynamic id and game balance id, if necessary and if possible
            if (c_ObjectType == GilesObjectType.Item)
            {
                // Get the Dynamic ID, cached if possible
                if (!dictGilesDynamicIDCache.TryGetValue(c_iRActorGuid, out c_iDynamicID))
                {
                    try
                    {
                        c_iDynamicID = tempCommonData.DynamicId;
                    }
                    catch (Exception ex)
                    {
                        Logging.WriteDiagnostic("[Trinity] Safely handled exception getting DynamicID for item " + c_sName + " [" + c_iActorSNO.ToString() + "]");
                        Logging.WriteDiagnostic(ex.ToString());
                        bWantThis = false;
                        //return bWantThis;
                    }
                    dictGilesDynamicIDCache.Add(c_iRActorGuid, c_iDynamicID);
                }
                // Get the Game Balance ID, cached if possible
                if (!dictGilesGameBalanceIDCache.TryGetValue(c_iRActorGuid, out c_iBalanceID))
                {
                    try
                    {
                        c_iBalanceID = tempCommonData.GameBalanceId;
                    }
                    catch (Exception ex)
                    {
                        Logging.WriteDiagnostic("[Trinity] Safely handled exception getting GameBalanceID for item " + c_sName + " [" + c_iActorSNO.ToString() + "]");
                        Logging.WriteDiagnostic(ex.ToString());
                        bWantThis = false;
                        //return bWantThis;
                    }
                    dictGilesGameBalanceIDCache.Add(c_iRActorGuid, c_iBalanceID);
                }
            }
            else
            {
                c_iDynamicID = -1;
                c_iBalanceID = -1;
            }
            return bWantThis;
        }
        private static bool RefreshStepNewObjectTypeZDiff(bool bWantThis)
        {
            // Ignore stuff which has a Z-height-difference too great, it's probably on a different level etc. - though not avoidance!
            if (c_ObjectType != GilesObjectType.Avoidance)
            {
                // Calculate the z-height difference between our current position, and this object's position
                c_fZDiff = Math.Abs(playerStatus.vCurrentPosition.Z - c_vPosition.Z);
                switch (c_ObjectType)
                {
                    case GilesObjectType.Door:
                    case GilesObjectType.Unit:
                    case GilesObjectType.Barricade:
                        // Ignore monsters (units) who's Z-height is 14 foot or more than our own z-height
                        // rrrix: except bosses like Belial :)
                        if (c_fZDiff >= 14f && !c_unit_bIsBoss)
                        {
                            bWantThis = false;
                            //return bWantThis;
                        }
                        break;
                    case GilesObjectType.Item:
                    case GilesObjectType.HealthWell:
                        // Items at 26+ z-height difference (we don't want to risk missing items so much)
                        if (c_fZDiff >= 26f)
                        {
                            bWantThis = false;
                            //return bWantThis;
                        }
                        break;
                    case GilesObjectType.Gold:
                    case GilesObjectType.Globe:
                        // Gold/Globes at 11+ z-height difference
                        if (c_fZDiff >= 11f)
                        {
                            bWantThis = false;
                            //return bWantThis;
                        }
                        break;
                    case GilesObjectType.Destructible:
                    case GilesObjectType.Shrine:
                    case GilesObjectType.Container:
                        // Destructibles, shrines and containers are the least important, so a z-height change of only 7 is enough to ignore (help avoid stucks at stairs etc.)
                        if (c_fZDiff >= 7f)
                        {
                            bWantThis = false;
                            //return bWantThis;
                        }
                        break;
                    case GilesObjectType.Interactable:
                        // Special interactable objects
                        if (c_fZDiff >= 9f)
                        {
                            bWantThis = false;
                            //return bWantThis;
                        }
                        break;
                    case GilesObjectType.Unknown:
                    default:
                        {
                            // Don't touch it!
                        }
                        break;
                }
            }
            else
            {
                bWantThis = true;
            }
            return bWantThis;
        }
        private static bool RefreshStepCachedPlayerSummons(bool bWantThis, ACD tempCommonData)
        {
            // Count up Mystic Allys, gargantuans, and zombies - if the player has those skills
            if (iMyCachedActorClass == ActorClass.Monk)
            {
                if (hashPowerHotbarAbilities.Contains(SNOPower.Monk_MysticAlly) && hashMysticAlly.Contains(c_iActorSNO))
                {
                    iPlayerOwnedMysticAlly++;
                    bWantThis = false;
                }
            }
            // Count up Demon Hunter pets
            if (iMyCachedActorClass == ActorClass.DemonHunter)
            {
                if (hashPowerHotbarAbilities.Contains(SNOPower.DemonHunter_Companion) && hashDHPets.Contains(c_iActorSNO))
                {
                    iPlayerOwnedDHPets++;
                    bWantThis = false;
                }
            }
            // Count up zombie dogs and gargantuans next
            if (iMyCachedActorClass == ActorClass.WitchDoctor)
            {
                if (hashPowerHotbarAbilities.Contains(SNOPower.Witchdoctor_Gargantuan) && hashGargantuan.Contains(c_iActorSNO))
                {
                    iPlayerOwnedGargantuan++;
                    bWantThis = false;
                }
                if (hashPowerHotbarAbilities.Contains(SNOPower.Witchdoctor_SummonZombieDog) && hashZombie.Contains(c_iActorSNO))
                {
                    iPlayerOwnedZombieDog++;
                    bWantThis = false;
                }
            }
            return bWantThis;
        }
        private static bool RefreshStepCheckBlacklists(bool bWantThis)
        {
            if (!hashAvoidanceSNOList.Contains(c_iActorSNO) && !hashAvoidanceBuffSNOList.Contains(c_iActorSNO))
            {
                // See if it's something we should always ignore like ravens etc.
                if (!c_bIsObstacle && hashActorSNOIgnoreBlacklist.Contains(c_iActorSNO))
                {
                    bWantThis = false;
                    c_IgnoreSubStep = "hashActorSNOIgnoreBlacklist";
                    return bWantThis;
                }
                if (!c_bIsObstacle && hashSNOIgnoreBlacklist.Contains(c_iActorSNO))
                {
                    bWantThis = false;
                    c_IgnoreSubStep = "hashSNOIgnoreBlacklist";
                    return bWantThis;
                }
                // Temporary ractor GUID ignoring, to prevent 2 interactions in a very short time which can cause stucks
                if (iIgnoreThisForLoops > 0 && iIgnoreThisRactorGUID == c_iRActorGuid)
                {
                    bWantThis = false;
                    c_IgnoreSubStep = "iIgnoreThisRactorGUID";
                    return bWantThis;
                }
                // Check our extremely short-term destructible-blacklist
                if (hashRGUIDDestructible3SecBlacklist.Contains(c_iRActorGuid))
                {
                    bWantThis = false;
                    c_IgnoreSubStep = "hashRGUIDDestructible3SecBlacklist";
                    return bWantThis;
                }
                // Check our extremely short-term destructible-blacklist
                if (hashRGuid3SecBlacklist.Contains(c_iRActorGuid))
                {
                    bWantThis = false;
                    c_IgnoreSubStep = "hashRGuid3SecBlacklist";
                    return bWantThis;
                }
                // See if it's on our 90 second blacklist (from being stuck targeting it), as long as it's distance is not extremely close
                if (hashRGUIDIgnoreBlacklist90.Contains(c_iRActorGuid))
                {
                    bWantThis = false;
                    c_IgnoreSubStep = "hashRGUIDTemporaryIgnoreBlacklist90";
                    return bWantThis;
                }
                // 60 second blacklist
                if (hashRGUIDIgnoreBlacklist60.Contains(c_iRActorGuid))
                {
                    bWantThis = false;
                    c_IgnoreSubStep = "hashRGUIDIgnoreBlacklist60";
                    return bWantThis;
                }
                // 15 second blacklist
                if (hashRGUIDIgnoreBlacklist15.Contains(c_iRActorGuid))
                {
                    bWantThis = false;
                    c_IgnoreSubStep = "hashRGUIDIgnoreBlacklist15";
                    return bWantThis;
                }
            }
            else
            {
                bWantThis = true;
            }
            return bWantThis;
        }
        private static bool RefreshGilesUnit(bool bWantThis, DiaObject thisobj, ACD tempCommonData)
        {
            DiaUnit thisUnit = (DiaUnit)thisobj;
            bWantThis = true;
            // Store the dia unit reference (we'll be able to remove this if we update ractor list every single loop, yay!)
            c_diaObject = null;
            // See if this is a boss
            c_unit_bIsBoss = hashBossSNO.Contains(c_iActorSNO);
            // hax for Diablo_shadowClone
            c_unit_bIsAttackable = c_sName.StartsWith("Diablo_shadowClone");
            try
            {
                c_diaObject = (DiaUnit)thisUnit;
                // Prepare the fake object for target handler
                if (thisFakeObject == null)
                    thisFakeObject = thisUnit;
            }
            catch
            {
                bWantThis = false;
                c_IgnoreSubStep = "NotAUnit";
                //return bWantThis;
            }
            if (tempCommonData.ACDGuid == -1)
            {
                bWantThis = false;
            }
            // Dictionary based caching of monster types based on the SNO codes
            MonsterType monsterType;
            // See if we need to refresh the monster type or not
            bool bAddToDictionary = !dictionaryStoredMonsterTypes.TryGetValue(c_iActorSNO, out monsterType);
            bool bRefreshMonsterType = bAddToDictionary;
            // If it's a boss and it was an ally, keep refreshing until it's not an ally
            // Because some bosses START as allied for cutscenes etc. until they become hostile
            if (c_unit_bIsBoss && !bRefreshMonsterType)
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
                    monsterType = RefreshMonsterType(tempCommonData, monsterType, bAddToDictionary);
                }
                catch (Exception ex)
                {
                    Logging.WriteDiagnostic("[Trinity] Safely handled exception getting monsterinfo and monstertype for unit " + c_sName + " [" + c_iActorSNO.ToString() + "]");
                    Logging.WriteDiagnostic(ex.ToString());
                    Logging.WriteDiagnostic("ActorTypeAttempt=");
                    Logging.WriteDiagnostic(thisUnit.ActorType.ToString());
                    bWantThis = false;
                    //return bWantThis;
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
                        bWantThis = false;
                        c_IgnoreSubStep = "AllySceneryHelperTeam";
                        //return bWantThis;
                    }
                    break;
            }
            // health calculations
            double dThisMaxHealth;
            // Get the max health of this unit, a cached version if available, if not cache it
            if (!dictGilesMaxHealthCache.TryGetValue(c_iRActorGuid, out dThisMaxHealth))
            {
                try
                {
                    dThisMaxHealth = tempCommonData.GetAttribute<float>(ActorAttributeType.HitpointsMax);
                }
                catch (Exception ex)
                {
                    Logging.WriteDiagnostic("[Trinity] Safely handled exception getting attribute max health for unit " + c_sName + " [" + c_iActorSNO.ToString() + "]");
                    Logging.WriteDiagnostic(ex.ToString());
                    bWantThis = false;
                    //return bWantThis;
                }
                dictGilesMaxHealthCache.Add(c_iRActorGuid, dThisMaxHealth);
            }
            // Now try to get the current health - using temporary and intelligent caching
            // Health calculations
            int iLastCheckedHealth;
            double dThisCurrentHealth = 0d;
            bool bHasCachedHealth;
            // See if we already have a cached value for health or not for this monster
            if (dictGilesLastHealthChecked.TryGetValue(c_iRActorGuid, out iLastCheckedHealth))
            {
                bHasCachedHealth = true;
                iLastCheckedHealth++;
                if (iLastCheckedHealth > 6)
                    iLastCheckedHealth = 1;
                if (iCurrentTargetRactorGUID == c_iRActorGuid && iLastCheckedHealth > 3)
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
                    dThisCurrentHealth = tempCommonData.GetAttribute<float>(ActorAttributeType.HitpointsCur);
                }
                catch
                {
                    // This happens so frequently in DB/D3 that this fails, let's not even bother logging it anymore
                    //Logging.WriteDiagnostic("[Trinity] Safely handled exception getting current health for unit " + tmp_sThisInternalName + " [" + tmp_iThisActorSNO.ToString() + "]");
                    // Add this monster to our very short-term ignore list
                    if (!c_unit_bIsBoss)
                    {
                        hashRGuid3SecBlacklist.Add(c_iRActorGuid);
                        lastTemporaryBlacklist = DateTime.Now;
                        bNeedClearTemporaryBlacklist = true;
                    }
                    bWantThis = false;
                    //return bWantThis;
                }
                RefreshCachedHealth(iLastCheckedHealth, dThisCurrentHealth, bHasCachedHealth);
            }
            else
            {
                dThisCurrentHealth = dictGilesLastHealthCache[c_iRActorGuid];
                dictGilesLastHealthChecked[c_iRActorGuid] = iLastCheckedHealth;
            }
            // And finally put the two together for a current health percentage
            c_unit_dHitPoints = dThisCurrentHealth / dThisMaxHealth;
            // Unit is already dead
            if (c_unit_dHitPoints <= 0d && !c_unit_bIsBoss)
            {
                // Add this monster to our very short-term ignore list
                hashRGuid3SecBlacklist.Add(c_iRActorGuid);
                lastTemporaryBlacklist = DateTime.Now;
                bNeedClearTemporaryBlacklist = true;
                bWantThis = false;
                c_IgnoreSubStep = "0HitPoints";
                //return bWantThis;
            }
            // Only set treasure goblins to true *IF* they haven't disabled goblins! Then check the SNO in the goblin hash list!
            c_unit_bIsTreasureGoblin = false;
            // Flag this as a treasure goblin *OR* ignore this object altogether if treasure goblins are set to ignore
            if (hashActorSNOGoblins.Contains(c_iActorSNO))
            {
                if (settings.iTreasureGoblinPriority != 0)
                {
                    c_unit_bIsTreasureGoblin = true;
                }
                else
                {
                    bWantThis = false;
                    c_IgnoreSubStep = "IgnoreTreasureGoblins";
                    //return bWantThis;
                }
            }
            // Pull up the Monster Affix cached data
            MonsterAffixes theseaffixes = RefreshAffixes(tempCommonData);
            //intell -- Other dangerous: Nightmarish, Mortar, Desecrator, Fire Chains, Knockback, Electrified
            if (GilesUseTimer(SNOPower.Barbarian_WrathOfTheBerserker, true))
            {
                //WotB only used on Arcane, Frozen, Jailer, Molten and Electrified+Reflect Damage elites
                if (theseaffixes.HasFlag(MonsterAffixes.ArcaneEnchanted) || theseaffixes.HasFlag(MonsterAffixes.Frozen) ||
                    theseaffixes.HasFlag(MonsterAffixes.Jailer) || theseaffixes.HasFlag(MonsterAffixes.Molten) ||
                   (theseaffixes.HasFlag(MonsterAffixes.Electrified) && theseaffixes.HasFlag(MonsterAffixes.ReflectsDamage)) ||
                    //Bosses and uber elites
                    c_unit_bIsBoss || c_iActorSNO == 256015 || c_iActorSNO == 256000 || c_iActorSNO == 255996 ||
                    //...or more than 4 elite mobs in range (only elites/rares/uniques, not minions!)
                    iElitesWithinRange[RANGE_50] > 4)
                    bUseBerserker = true;
            }
            else
                bUseBerserker = false;
            // Is this something we should try to force leap/other movement abilities against?
            c_bForceLeapAgainst = false;
            double dUseKillRadius = RefreshKillRadius();
            // Now ignore any unit not within our kill or extended kill radius
            if (c_fRadiusDistance > dUseKillRadius)
            {
                bWantThis = false;
                c_IgnoreSubStep = "OutsideofKillRadius";
            }
            if (thisUnit.IsUntargetable)
            {
                bWantThis = false;
                c_IgnoreSubStep += "Untargettable+";
            }
            if (thisUnit.IsHidden)
            {
                bWantThis = false;
                c_IgnoreSubStep += "IsHidden+";
            }
            if (thisUnit.IsInvulnerable)
            {
                bWantThis = false;
                c_IgnoreSubStep += "IsInvulnerable+";
            }
            //if (tmp_unit_diaUnit.IsBurrowed)
            //{
            //    bWantThis = false;
            //    tmp_cacheIgnoreSubStep += "IsBurrowed+";
            //    hashRGuid3SecBlacklist.Add(tmp_iThisRActorGuid);
            //}
            if (thisUnit.IsHelper || thisUnit.IsNPC || thisUnit.IsTownVendor)
            {
                bWantThis = false;
                c_IgnoreSubStep += "IsNPCOrHelper+";
            }
            // Safe is-attackable detection
            c_unit_bIsAttackable = true;
            if (c_unit_bIsBoss || theseaffixes.HasFlag(MonsterAffixes.Shielding))
            {
                try
                {
                    c_unit_bIsAttackable = (tempCommonData.GetAttribute<int>(ActorAttributeType.Invulnerable) <= 0);
                }
                catch (Exception ex)
                {
                    Logging.WriteDiagnostic("[Trinity] Safely handled exception getting is-invulnerable attribute for unit " + c_sName + " [" + c_iActorSNO.ToString() + "]");
                    Logging.WriteDiagnostic(ex.ToString());
                    c_unit_bIsAttackable = true;
                }
            }
            // Inactive units like trees, withermoths etc. still underground
            if (c_unit_dHitPoints >= 1f || c_unit_bIsBoss)
            {
                // Get the burrowing data for this unit
                bool bBurrowed;
                if (!dictGilesBurrowedCache.TryGetValue(c_iRActorGuid, out bBurrowed) || c_unit_bIsBoss)
                {
                    try
                    {
                        bBurrowed = (tempCommonData.GetAttribute<int>(ActorAttributeType.Untargetable) != 0) || (tempCommonData.GetAttribute<int>(ActorAttributeType.Burrowed) != 0);
                    }
                    catch (Exception ex)
                    {
                        Logging.WriteDiagnostic("[Trinity] Safely handled exception getting is-untargetable or is-burrowed attribute for unit " + c_sName + " [" + c_iActorSNO.ToString() + "]");
                        Logging.WriteDiagnostic(ex.ToString());
                        bWantThis = false;
                        //return bWantThis;
                    }
                    // Only cache it if it's NOT burrowed (if it *IS* - then we need to keep re-checking until it comes out!)
                    if (!bBurrowed)
                    {
                        // Don't cache for bosses, as we have to check for bosses popping in and out of the game during a complex fight
                        if (!c_unit_bIsBoss)
                            dictGilesBurrowedCache.Add(c_iRActorGuid, bBurrowed);
                    }
                    else
                    {
                        // Unit is burrowed, so we need to ignore it until it isn't!
                        c_IgnoreSubStep = "Burrowed";
                        bWantThis = false;
                        //return bWantThis;
                    }
                }
            }
            // Only if at full health, else don't bother checking each loop
            // See if we already have this monster's size stored, if not get it and cache it
            if (!dictionaryStoredMonsterSizes.TryGetValue(c_iActorSNO, out c_unit_MonsterSize))
            {
                try
                {
                    SNORecordMonster monsterInfo = thisUnit.MonsterInfo;
                    if (monsterInfo != null)
                    {
                        c_unit_MonsterSize = monsterInfo.MonsterSize;
                        dictionaryStoredMonsterSizes.Add(c_iActorSNO, c_unit_MonsterSize);
                    }
                    else
                    {
                        c_unit_MonsterSize = MonsterSize.Unknown;
                    }
                }
                catch (Exception ex)
                {
                    Logging.WriteDiagnostic("[Trinity] Safely handled exception getting monstersize info for unit " + c_sName + " [" + c_iActorSNO.ToString() + "]");
                    Logging.WriteDiagnostic(ex.ToString());
                    bWantThis = false;
                    //return bWantThis;
                }
            }
            // Retrieve collision sphere radius, cached if possible
            if (!dictGilesCollisionSphereCache.TryGetValue(c_iActorSNO, out c_fRadius))
            {
                try
                {
                    c_fRadius = thisUnit.CollisionSphere.Radius;
                    // Take 6 from the radius
                    if (!c_unit_bIsBoss)
                        c_fRadius -= 6f;
                    // Minimum range clamp
                    if (c_fRadius <= 1f)
                        c_fRadius = 1f;
                    // Maximum range clamp
                    if (c_fRadius >= 20f)
                        c_fRadius = 20f;
                }
                catch (Exception ex)
                {
                    Logging.WriteDiagnostic("[Trinity] Safely handled exception getting collisionsphere radius for unit " + c_sName + " [" + c_iActorSNO.ToString() + "]");
                    Logging.WriteDiagnostic(ex.ToString());
                    bWantThis = false;
                    //return bWantThis;
                }
                dictGilesCollisionSphereCache.Add(c_iActorSNO, c_fRadius);
            }
            // A "fake distance" to account for the large-object size of monsters
            c_fRadiusDistance -= (float)c_fRadius;
            if (c_fRadiusDistance <= 1f)
                c_fRadiusDistance = 1f;
            // All-in-one flag for quicker if checks throughout
            c_bIsEliteRareUnique = (c_unit_bIsElite || c_unit_bIsRare || c_unit_bIsUnique || c_unit_bIsMinion);
            // Special flags to decide whether to target anything at all
            if (c_bIsEliteRareUnique || c_unit_bIsBoss)
                bAnyChampionsPresent = true;
            // Extended kill radius after last fighting, or when we want to force a town run
            if ((settings.bExtendedKillRange && iKeepKillRadiusExtendedFor > 0) || bGilesForcedVendoring)
            {
                if (c_fRadiusDistance <= dUseKillRadius && bWantThis)
                    bAnyMobsInCloseRange = true;
            }
            else
            {
                if (c_fRadiusDistance <= settings.iMonsterKillRange && bWantThis)
                    bAnyMobsInCloseRange = true;
            }
            if (c_unit_bIsTreasureGoblin)
                bAnyTreasureGoblinsPresent = true;
            // Units with very high priority (1900+) allow an extra 50% on the non-elite kill slider range
            if (!bAnyMobsInCloseRange && !bAnyChampionsPresent && !bAnyTreasureGoblinsPresent && c_fRadiusDistance <= (settings.iMonsterKillRange * 1.5))
            {
                int iExtraPriority;
                // Enable extended kill radius for specific unit-types
                if (hashActorSNORanged.Contains(c_iActorSNO))
                {
                    bAnyMobsInCloseRange = true;
                }
                if (!bAnyMobsInCloseRange && dictActorSNOPriority.TryGetValue(c_iActorSNO, out iExtraPriority))
                {
                    if (iExtraPriority >= 1900)
                    {
                        bAnyMobsInCloseRange = true;
                    }
                }
            }
            return bWantThis;
        }
        private static bool RefreshGilesItem(bool bWantThis, DiaObject thisobj, ACD tempCommonData, int iCurrentMinimumStackSize, double iPercentage)
        {
            bWantThis = false;
            if (c_iBalanceID == -1)
            {
                bWantThis = false;
                //return bWantThis;
            }
            // Try and pull up cached item data on this item, if not, add to our local memory cache
            GilesGameBalanceDataCache tempGilesGameBalanceId;
            if (!dictGilesGameBalanceDataCache.TryGetValue(c_iBalanceID, out tempGilesGameBalanceId))
            {
                DiaItem tempitem = thisobj as DiaItem;
                if (tempitem != null)
                {
                    try
                    {
                        c_item_iLevel = tempitem.CommonData.Level;
                        c_item_tDBItemType = tempitem.CommonData.ItemType;
                        c_item_bOneHanded = tempitem.CommonData.IsOneHand;
                        c_item_tFollowerType = tempitem.CommonData.FollowerSpecialType;
                        dictGilesGameBalanceDataCache.Add(c_iBalanceID, new GilesGameBalanceDataCache(c_item_iLevel, c_item_tDBItemType, c_item_bOneHanded,
                            c_item_tFollowerType));
                        // Temporarily log stuff
                        //if (bLogBalanceDataForGiles)
                        //{
                        //    FileStream LogStream = File.Open(sTrinityPluginPath + "_BalanceData_" + ZetaDia.Service.CurrentHero.BattleTagName + ".log", FileMode.Append, FileAccess.Write, FileShare.Read);
                        //    using (StreamWriter LogWriter = new StreamWriter(LogStream))
                        //    {
                        //        LogWriter.WriteLine("{" + tmp_iThisBalanceID.ToString() + ", new GilesGameBalanceDataCache(" +
                        //            tmp_item_iThisLevel.ToString() + ", ItemType." + tmp_item_ThisDBItemType.ToString() + ", " +
                        //            tmp_item_bThisOneHanded.ToString().ToLower() + ", FollowerType." + tmp_item_ThisFollowerType.ToString() + ")}, 
                        //" + tmp_sThisInternalName + " [" + tmp_iThisActorSNO.ToString() + "]");
                        //    }
                        //    LogStream.Close();
                        //}
                    }
                    catch (Exception ex)
                    {
                        Logging.WriteDiagnostic("[Trinity] Safely handled exception getting un-cached ACD Item data (level/item type etc.) for item " + c_sName + " [" + c_iActorSNO.ToString() + "]");
                        Logging.WriteDiagnostic(ex.ToString());
                        bWantThis = false;
                        //return bWantThis;
                    }
                }
                else
                {
                    // Couldn't get the game balance data for this item, so ignore it for now
                    bWantThis = false;
                    //return bWantThis;
                }
            }
            else
            {
                // We pulled this data from the dictionary cache, so use it instead of trying to get new data from DB/D3 memory!
                c_item_iLevel = tempGilesGameBalanceId.iThisItemLevel;
                c_item_tDBItemType = tempGilesGameBalanceId.thisItemType;
                c_item_bOneHanded = tempGilesGameBalanceId.bThisOneHand;
                c_item_tFollowerType = tempGilesGameBalanceId.thisFollowerType;
            }
            // Able to get cached data or not?
            // Error reading the item?
            if (c_iBalanceID == -1)
            {
                bWantThis = false;
                //return bWantThis;
            }
            // Calculate custom Giles item type
            c_item_GilesItemType = DetermineItemType(c_sName, c_item_tDBItemType, c_item_tFollowerType);
            // And temporarily store the base type
            GilesBaseItemType tempbasetype = DetermineBaseType(c_item_GilesItemType);
            // Treat all globes as a yes
            if (c_item_GilesItemType == GilesItemType.HealthGlobe)
            {
                c_ObjectType = GilesObjectType.Globe;
                // Create or alter this cached object type
                GilesObjectType tempobjecttype;
                if (!dictGilesObjectTypeCache.TryGetValue(c_iRActorGuid, out tempobjecttype))
                    dictGilesObjectTypeCache.Add(c_iRActorGuid, c_ObjectType);
                else
                    dictGilesObjectTypeCache[c_iRActorGuid] = c_ObjectType;
                bWantThis = true;
                //break;
            }
            //
            // Gold amount for gold piles
            //tmp_item_iThisGoldAmount = -1;
            //
            // Handle gold piles first
            //if (tmp_sThisInternalName.ToLower().StartsWith("gold"))
            //{
            //    
            // Get the gold amount of this pile, cached if possible
            //    if (!dictGilesGoldAmountCache.TryGetValue(tmp_iThisRActorGuid, out tmp_item_iThisGoldAmount))
            //    {
            //        try
            //        {
            //            tmp_item_iThisGoldAmount = tempCommonData.GetAttribute<int>(ActorAttributeType.Gold);
            //        }
            //        catch
            //        {
            //            Logging.WriteDiagnostic("[Trinity] Safely handled exception getting gold pile amount for item " + tmp_sThisInternalName + " [" + tmp_iThisActorSNO.ToString() + "]");
            //            bWantThis = false;
            //            
            //return bWantThis;
            //        }
            //        dictGilesGoldAmountCache.Add(tmp_iThisRActorGuid, tmp_item_iThisGoldAmount);
            //    }
            //    
            // Ignore gold piles that are (currently) too small...
            //    iCurrentMinimumStackSize = settings.iMinimumGoldStack;
            //    
            // Up to 40% less gold limit needed at close range
            //    if (tmp_fCentreDistance <= 20f)
            //    {
            //        iPercentage = (1 - (tmp_fCentreDistance / 20)) * 0.4;
            //        iCurrentMinimumStackSize -= (int)Math.Floor(iPercentage * iCurrentMinimumStackSize);
            //    }
            //    
            // And up to 40% or even higher extra gold limit at distant range
            //    else if (tmp_fCentreDistance > 20f)
            //    {
            //        iPercentage = (tmp_fCentreDistance / 50) * 0.8;
            //        iCurrentMinimumStackSize += (int)Math.Floor(iPercentage * iCurrentMinimumStackSize);
            //    }
            //    
            // Now check if this gold pile is currently less than this limit
            //    if (tmp_item_iThisGoldAmount < iCurrentMinimumStackSize)
            //    {
            //        bWantThis = false;
            //        
            //return bWantThis;
            //    }
            //    
            // Blacklist gold piles already in pickup radius range
            //    if (tmp_fCentreDistance <= ZetaDia.Me.GoldPickUpRadius)
            //    {
            //        hashRGUIDIgnoreBlacklist60.Add(tmp_iThisRActorGuid);
            //        bWantThis = false;
            //        
            //return bWantThis;
            //    }
            //    tmp_ThisGilesObjectType = GilesObjectType.Gold;
            //    
            // Create or alter this cached object type
            //    GilesObjectType tempobjecttype;
            //    if (!dictGilesObjectTypeCache.TryGetValue(tmp_iThisRActorGuid, out tempobjecttype))
            //        dictGilesObjectTypeCache.Add(tmp_iThisRActorGuid, tmp_ThisGilesObjectType);
            //    else
            //        dictGilesObjectTypeCache[tmp_iThisRActorGuid] = tmp_ThisGilesObjectType;
            //    bWantThis = true;
            //    
            //break;
            //}
            // Quality of item for "genuine" items
            c_item_tQuality = ItemQuality.Invalid;
            if (tempbasetype != GilesBaseItemType.Unknown && tempbasetype != GilesBaseItemType.HealthGlobe && tempbasetype != GilesBaseItemType.Gem && tempbasetype != GilesBaseItemType.Misc &&
                !hashForceSNOToItemList.Contains(c_iActorSNO))
            {
                // Get the quality of this item, cached if possible
                if (!dictGilesQualityCache.TryGetValue(c_iRActorGuid, out c_item_tQuality))
                {
                    try
                    {
                        c_item_tQuality = (ItemQuality)tempCommonData.GetAttribute<int>(ActorAttributeType.ItemQualityLevel);
                    }
                    catch
                    {
                        Logging.WriteDiagnostic("[Trinity] Safely handled exception getting item-quality for item " + c_sName + " [" + c_iActorSNO.ToString() + "]");
                        bWantThis = false;
                        //return bWantThis;
                    }
                    dictGilesQualityCache.Add(c_iRActorGuid, c_item_tQuality);
                    dictGilesQualityRechecked.Add(c_iRActorGuid, false);
                }
                else
                {
                    // Because item-quality is such a sensitive thing, we don't want to risk losing items
                    // So we check a cached item quality a 2nd time - as long as it's the same, we won't check again
                    // However, if there's any inconsistencies, we keep checking, and keep the highest-read quality as the real value
                    if (!dictGilesQualityRechecked[c_iRActorGuid])
                    {
                        ItemQuality temporaryItemQualityCheck = ItemQuality.Invalid;
                        bool bFailedReading = false;
                        try
                        {
                            temporaryItemQualityCheck = (ItemQuality)tempCommonData.GetAttribute<int>(ActorAttributeType.ItemQualityLevel);
                        }
                        catch
                        {
                            Logging.WriteDiagnostic("[Trinity] Safely handled exception double-checking item-quality for item " + c_sName + " [" + c_iActorSNO.ToString() + "]");
                            bFailedReading = true;
                        }
                        // Make sure we didn't get a failure re-reading the quality, if we did we'll just keep re-checking until we don't
                        if (!bFailedReading)
                        {
                            // If the newly-received quality is higher, then store the new quality
                            if (temporaryItemQualityCheck > c_item_tQuality)
                            {
                                dictGilesQualityCache[c_iRActorGuid] = temporaryItemQualityCheck;
                                c_item_tQuality = temporaryItemQualityCheck;
                            }
                            // And now flag it so we don't check this item again
                            dictGilesQualityRechecked[c_iRActorGuid] = true;
                        }
                    }
                }
            }
            // Item stats
            RefreshItemStats(tempbasetype);
            // Ignore it if it's not in range yet - allow legendary items to have 15 feet extra beyond our profile max loot radius
            float fExtraRange = 0f;
            if (iKeepLootRadiusExtendedFor > 0 || c_item_tQuality >= ItemQuality.Rare4)
            {
                fExtraRange = 30f;
            }
            if (iKeepLootRadiusExtendedFor > 0 || c_item_tQuality >= ItemQuality.Legendary)
            {
                fExtraRange = 50f;
            }
            if (c_fCentreDistance > (iCurrentMaxLootRadius + fExtraRange))
            {
                bWantThis = false;
                //return bWantThis;
            }
            // Now see if we actually want it
            if (settings.bUseGilesFilters)
            {
                // Get whether or not we want this item, cached if possible
                bool bWantThisItem;
                if (!dictGilesPickupItem.TryGetValue(c_iRActorGuid, out bWantThisItem))
                {
                    bWantThisItem = GilesPickupItemValidation(c_sName, c_item_iLevel, c_item_tQuality, c_iBalanceID, c_item_tDBItemType,
                        c_item_tFollowerType, c_iDynamicID);
                    dictGilesPickupItem.Add(c_iRActorGuid, bWantThisItem);
                }
                // Using Giles filters
                if (bWantThisItem)
                {
                    // If we are trying to run a vendor-run, then don't deal with items atm
                    if (bGilesForcedVendoring)
                    {
                        bWantThis = false;
                        //return bWantThis;
                    }
                    bWantThis = true;
                    //break;
                }
            }
            else
            {
                // Get whether or not we want this item, cached if possible
                bool bWantThisItem;
                if (!dictGilesPickupItem.TryGetValue(c_iRActorGuid, out bWantThisItem))
                {
                    bWantThisItem = ItemManager.EvaluateItem((ACDItem)tempCommonData, ItemManager.RuleType.PickUp);
                    dictGilesPickupItem.Add(c_iRActorGuid, bWantThisItem);
                }
                // Using DB built-in item rules
                if (bWantThisItem)
                {
                    if (bGilesForcedVendoring)
                    {
                        bWantThis = false;
                        //return bWantThis;
                    }
                    bWantThis = true;
                    //break;
                }
            }
            // Didn't pass giles pickup rules/DB internal rule match, so ignore it
            //bWantThis = false;
            return bWantThis;
        }
        private static bool RefreshGilesGold(bool bWantThis, ACD tempCommonData, out int iCurrentMinimumStackSize, out double iPercentage)
        {
            iPercentage = 0;
            bWantThis = true;
            // Get the gold amount of this pile, cached if possible
            if (!dictGilesGoldAmountCache.TryGetValue(c_iRActorGuid, out c_item_iGoldStackSize))
            {
                try
                {
                    c_item_iGoldStackSize = tempCommonData.GetAttribute<int>(ActorAttributeType.Gold);
                }
                catch
                {
                    Logging.WriteDiagnostic("[Trinity] Safely handled exception getting gold pile amount for item " + c_sName + " [" + c_iActorSNO.ToString() + "]");
                    bWantThis = false;
                    //return bWantThis;
                }
                dictGilesGoldAmountCache.Add(c_iRActorGuid, c_item_iGoldStackSize);
            }
            // Ignore gold piles that are (currently) too small...
            iCurrentMinimumStackSize = settings.iMinimumGoldStack;
            // Up to 40% less gold limit needed at close range
            if (c_fCentreDistance <= 20f)
            {
                iPercentage = (1 - (c_fCentreDistance / 20)) * 0.4;
                iCurrentMinimumStackSize -= (int)Math.Floor(iPercentage * iCurrentMinimumStackSize);
            }
            // And up to 40% or even higher extra gold limit at distant range
            else if (c_fCentreDistance >= 30f)
            {
                iPercentage = (c_fCentreDistance / 40) * 0.4;
                iCurrentMinimumStackSize += (int)Math.Floor(iPercentage * iCurrentMinimumStackSize);
            }
            // Now check if this gold pile is currently less than this limit
            if (c_item_iGoldStackSize < iCurrentMinimumStackSize)
            {
                bWantThis = false;
                //return bWantThis;
            }
            // Blacklist gold piles already in pickup radius range
            if (c_fCentreDistance <= ZetaDia.Me.GoldPickUpRadius)
            {
                hashRGUIDIgnoreBlacklist60.Add(c_iRActorGuid);
                bWantThis = false;
                return bWantThis;
            }
            //tmp_ThisGilesObjectType = GilesObjectType.Gold;
            //bWantThis = true;
            return bWantThis;
        }
        private static bool RefreshGilesGizmo(bool bWantThis, DiaObject thisobj, ACD tempCommonData)
        {
            // start as true, then set as false as we go. If nothing matches below, it will return true.
            bWantThis = true;
            // Check the primary object blacklist
            if (hashSNOIgnoreBlacklist.Contains(c_iActorSNO))
            {
                bWantThis = false;
                c_IgnoreSubStep = "hashSNOIgnoreBlacklist";
                //return bWantThis;
            }
            // Ignore it if it's not in range yet, except health wells
            if ((c_fRadiusDistance > iCurrentMaxLootRadius || c_fRadiusDistance > 50) && c_ObjectType != GilesObjectType.HealthWell)
            {
                bWantThis = false;
                c_IgnoreSubStep = "NotInRange";
                //return bWantThis;
            }
            if (c_sName.ToLower().StartsWith("minimapicon"))
            {
                // Minimap icons caused a few problems in the past, so this force-blacklists them
                hashRGUIDIgnoreBlacklist60.Add(c_iRActorGuid);
                c_IgnoreSubStep = "minimapicon";
                bWantThis = false;
                //return bWantThis;
            }
            // Retrieve collision sphere radius, cached if possible
            if (!dictGilesCollisionSphereCache.TryGetValue(c_iActorSNO, out c_fRadius))
            {
                try
                {
                    c_fRadius = thisobj.CollisionSphere.Radius;

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
                    Logging.WriteDiagnostic("[Trinity] Safely handled exception getting collisionsphere radius for object " + c_sName + " [" + c_iActorSNO.ToString() + "]");
                    bWantThis = false;
                    //return bWantThis;
                }
                dictGilesCollisionSphereCache.Add(c_iActorSNO, c_fRadius);
            }

            // A "fake distance" to account for the large-object size of monsters
            c_fRadiusDistance -= (float)c_fRadius;
            if (c_fRadiusDistance <= 1f)
                c_fRadiusDistance = 1f;

            // Anything that's been disabled by a script
            bool bDisabledByScript = false;
            try
            {
                bDisabledByScript = tempCommonData.GetAttribute<int>(ActorAttributeType.GizmoDisabledByScript) > 0;
            }
            catch
            {
                Logging.WriteDiagnostic("[Trinity] Safely handled exception getting Gizmo-Disabled-By-Script attribute for object " + c_sName + " [" + c_iActorSNO.ToString() + "]");
                bWantThis = false;
            }
            if (bDisabledByScript)
            {
                bWantThis = false;
                c_IgnoreSubStep = "GizmoDisabledByScript";
            }
            // Now for the specifics
            int iThisPhysicsSNO;
            int iExtendedRange;
            double iMinDistance;
            bool bThisUsed = false;
            switch (c_ObjectType)
            {
                case GilesObjectType.Door:
                    {
                        bWantThis = true;
                        try
                        {
                            string currentAnimation = tempCommonData.CurrentAnimation.ToString().ToLower();
                            bThisUsed = currentAnimation.EndsWith("open") || currentAnimation.EndsWith("opening");
                        }
                        catch { }
                        if (bThisUsed)
                        {
                            hashRGUIDIgnoreBlacklist90.Add(c_iRActorGuid);
                            bWantThis = false;
                            c_IgnoreSubStep = "Door is Open or Opening";
                        }
                        if (bWantThis)
                        {
                            try
                            {
                                DiaGizmo door = (GizmoDoor)thisobj;
                                if (door.IsGizmoDisabledByScript)
                                {
                                    hashRGUIDIgnoreBlacklist90.Add(c_iRActorGuid);
                                    bWantThis = false;
                                    c_IgnoreSubStep = "DoorDisabledbyScript";
                                }
                            }
                            catch { }
                        }
                    }
                    break;
                case GilesObjectType.Interactable:
                    bWantThis = true;
                    // Special interactables
                    if (c_fCentreDistance > 30f)
                    {
                        bWantThis = false;
                        //return bWantThis;
                    }
                    c_fRadius = 4f;
                    break;
                case GilesObjectType.HealthWell:
                    {
                        bWantThis = true;
                        try
                        {
                            bThisUsed = (tempCommonData.GetAttribute<int>(ActorAttributeType.GizmoCharges) <= 0 && tempCommonData.GetAttribute<int>(ActorAttributeType.GizmoCharges) > 0);
                        }
                        catch
                        {
                            Logging.WriteDiagnostic("[Trinity] Safely handled exception getting shrine-been-operated attribute for object " + c_sName + " [" + c_iActorSNO.ToString() + "]");
                            bWantThis = true;
                            //return bWantThis;
                        }
                        if (bThisUsed)
                        {
                            c_IgnoreSubStep = "GizmoCharges";
                            bWantThis = false;
                            //return bWantThis;
                        }
                        bWaitingAfterPower = true;
                    }
                    break;
                case GilesObjectType.Shrine:
                    {
                        bWantThis = true;
                        // Shrines
                        // Check if either we want to ignore all shrines
                        if (settings.bIgnoreAllShrines)
                        {
                            // We're ignoring all shrines, so blacklist this one
                            hashRGUIDIgnoreBlacklist60.Add(c_iRActorGuid);
                            bWantThis = false;
                            //return bWantThis;
                        }
                        // Already used, blacklist it and don't look at it again
                        try
                        {
                            bThisUsed = (tempCommonData.GetAttribute<int>(ActorAttributeType.GizmoHasBeenOperated) > 0);
                        }
                        catch
                        {
                            Logging.WriteDiagnostic("[Trinity] Safely handled exception getting shrine-been-operated attribute for object " + c_sName + " [" + c_iActorSNO.ToString() + "]");
                            bWantThis = false;
                            //return bWantThis;
                        }
                        if (bThisUsed)
                        {
                            // It's already open!
                            hashRGUIDIgnoreBlacklist60.Add(c_iRActorGuid);
                            c_IgnoreSubStep = "GizmoHasBeenOperated";
                            bWantThis = false;
                            //return bWantThis;
                        }
                        // Bag it!
                        c_fRadius = 4f;
                        break;
                    }
                case GilesObjectType.Barricade:
                    {
                        bWantThis = true;
                        // Get the cached physics SNO of this object
                        if (!dictPhysicsSNO.TryGetValue(c_iActorSNO, out iThisPhysicsSNO))
                        {
                            try
                            {
                                iThisPhysicsSNO = thisobj.PhysicsSNO;
                            }
                            catch
                            {
                                Logging.WriteDiagnostic("[Trinity] Safely handled exception getting physics SNO for object " + c_sName + " [" + c_iActorSNO.ToString() + "]");
                                bWantThis = false;
                                //return bWantThis;
                            }
                            dictPhysicsSNO.Add(c_iActorSNO, iThisPhysicsSNO);
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
                        iMinDistance = settings.iDestructibleAttackRange + (c_fRadius * 0.70);
                        if (bForceCloseRangeTarget)
                            iMinDistance += 6f;
                        // Large objects, like logs - Give an extra xx feet of distance
                        if (dictSNOExtendedDestructRange.TryGetValue(c_iActorSNO, out iExtendedRange))
                            iMinDistance = settings.iDestructibleAttackRange + iExtendedRange;
                        // This object isn't yet in our destructible desire range
                        if (iMinDistance <= 0 || c_fCentreDistance > iMinDistance)
                        {
                            c_IgnoreSubStep = "NotInBarricadeRange";
                            bWantThis = false;
                            //return bWantThis;
                        }
                        break;
                    }
                case GilesObjectType.Destructible:
                    {
                        bWantThis = true;
                        // Get the cached physics SNO of this object
                        if (!dictPhysicsSNO.TryGetValue(c_iActorSNO, out iThisPhysicsSNO))
                        {
                            try
                            {
                                iThisPhysicsSNO = thisobj.PhysicsSNO;
                            }
                            catch
                            {
                                Logging.WriteDiagnostic("[Trinity] Safely handled exception getting physics SNO for object " + c_sName + " [" + c_iActorSNO.ToString() + "]");
                                bWantThis = false;
                                //return bWantThis;
                            }
                            dictPhysicsSNO.Add(c_iActorSNO, iThisPhysicsSNO);
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
                        iMinDistance = settings.iDestructibleAttackRange + (c_fRadius * 0.30);
                        if (bForceCloseRangeTarget)
                            iMinDistance += 6f;
                        // Large objects, like logs - Give an extra xx feet of distance
                        if (dictSNOExtendedDestructRange.TryGetValue(c_iActorSNO, out iExtendedRange))
                            iMinDistance = settings.iDestructibleAttackRange + iExtendedRange;
                        // This object isn't yet in our destructible desire range
                        if (iMinDistance <= 0 || c_fRadiusDistance > iMinDistance)
                        {
                            bWantThis = false;
                            c_IgnoreSubStep = "NotInDestructableRange";
                        }
                        // Only break destructables if we're stuck
                        if (!GilesPlayerMover.UnstuckChecker())
                        {
                            bWantThis = false;
                            c_IgnoreSubStep = "NotStuck";
                        }
                        // If we're standing on it, usually right before above unstucker returns true
                        if (c_fRadiusDistance <= 2f)
                        {
                            bWantThis = true;
                        }
                        break;
                    }
                case GilesObjectType.Container:
                    {
                        // We want to do some vendoring, so don't open anything new yet
                        if (bGilesForcedVendoring)
                        {
                            bWantThis = false;
                            //return bWantThis;
                        }
                        // Already open, blacklist it and don't look at it again
                        bool bThisOpen = false;
                        try
                        {
                            bThisOpen = (tempCommonData.GetAttribute<int>(ActorAttributeType.ChestOpen) > 0);
                        }
                        catch
                        {
                            Logging.WriteDiagnostic("[Trinity] Safely handled exception getting container-been-opened attribute for object " + c_sName + " [" + c_iActorSNO.ToString() + "]");
                            bWantThis = false;
                            //return bWantThis;
                        }
                        if (bThisOpen)
                        {
                            // It's already open!
                            hashRGUIDIgnoreBlacklist60.Add(c_iRActorGuid);
                            bWantThis = false;
                            //return bWantThis;
                        }
                        else if (!bThisOpen && c_sName.ToLower().Contains("chest") && !c_sName.ToLower().Contains("chest_rare"))
                        {
                            // This should make the magic happen with Chests we actually want :)
                            bWantThis = true;
                        }
                        // Default to blacklisting all containers, then find reasons not to
                        bool bBlacklistThis = true;
                        iMinDistance = 0f;
                        // Get the cached physics SNO of this object
                        if (!dictPhysicsSNO.TryGetValue(c_iActorSNO, out iThisPhysicsSNO))
                        {
                            try
                            {
                                iThisPhysicsSNO = thisobj.PhysicsSNO;
                            }
                            catch
                            {
                                Logging.WriteDiagnostic("[Trinity] Safely handled exception getting physics SNO for object " + c_sName + " [" + c_iActorSNO.ToString() + "]");
                                bWantThis = false;
                                //return bWantThis;
                            }
                            dictPhysicsSNO.Add(c_iActorSNO, iThisPhysicsSNO);
                        }
                        // Any physics mesh? Give a minimum distance of 5 feet
                        if (c_sName.ToLower().Contains("corpse") && settings.bIgnoreCorpses)
                        {
                            bBlacklistThis = true;
                        }
                        else if (iThisPhysicsSNO > 0 || !settings.bIgnoreCorpses)
                        {
                            //Logging.WriteDiagnostic("[Trinity] open container " + tmp_sThisInternalName + "[" + tmp_iThisActorSNO.ToString() + "]" + iThisPhysicsSNO);
                            bBlacklistThis = false;
                            iMinDistance = settings.iContainerOpenRange;
                        }
                        else
                        {
                            bBlacklistThis = true;
                        }
                        // Whitelist for chests we want to open if we ever get close enough to them
                        if (hashSNOContainerWhitelist.Contains(c_iActorSNO))
                        {
                            bBlacklistThis = false;
                            if (settings.iContainerOpenRange > 0)
                                iMinDistance = settings.iContainerOpenRange + 5;
                        }
                        else if (c_sName.ToLower().Contains("chest") && !c_sName.ToLower().Contains("chest_rare"))
                        {
                            Logging.WriteDiagnostic("GSDebug: Possible Chest SNO: " + c_sName + ", SNO=" + c_iActorSNO.ToString());
                        }
                        // Superlist for rare chests etc.
                        if (hashSNOContainerResplendant.Contains(c_iActorSNO))
                        {
                            bBlacklistThis = false;
                            if (settings.iContainerOpenRange > 0)
                                iMinDistance = settings.iContainerOpenRange + 20;
                            else
                                iMinDistance = 10;
                        }
                        else if (c_sName.Contains("chest_rare"))
                        {
                            Logging.WriteDiagnostic("GSDebug: Possible Resplendant Chest SNO: " + c_sName + ", SNO=" + c_iActorSNO.ToString());
                        }
                        // Blacklist this if it's something we should never bother looking at again
                        if (bBlacklistThis)
                        {
                            hashRGUIDIgnoreBlacklist60.Add(c_iRActorGuid);
                            bWantThis = false;
                            //return bWantThis;
                        }
                        if (iMinDistance <= 0 || c_fCentreDistance > iMinDistance)
                        {
                            bWantThis = false;
                            //return bWantThis;
                        }
                        // Bag it!
                        //tmp_fThisRadius = 4f;
                        //bWantThis = true;
                        break;
                    }
            }
            return bWantThis;
        }
        private static bool RefreshGilesAvoidance(bool bWantThis)
        {
            bWantThis = true;
            // Note if you are looking here - an AOE object won't even appear at this stage if you have settings.bEnableAvoidance switched off!
            //if (!hashAvoidanceSNOList.Contains(tmp_iThisActorSNO))
            //{
            //    Logging.WriteDiagnostic("GSDebug: Invalid avoidance detected, SNO=" + tmp_iThisActorSNO.ToString() + ", name=" + tmp_sThisInternalName + ", object type=" +
            //        tmp_ThisGilesObjectType.ToString());
            //    bWantThis = false;
            //    
            //return bWantThis;
            //}
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
                if (c_iActorSNO == 223675 || c_iActorSNO == 402 || c_iActorSNO == 219702 || c_iActorSNO == 221225 || c_iActorSNO == 84608 || c_iActorSNO == 108869)
                {
                    // Ignore ICE/Arcane/Desc/PlagueCloud altogether with spirit walk up or available
                    bIgnoreThisAvoidance = true;
                }
            }
            // Remove ice balls if the barbarian has wrath of the berserker up, and reduce health from most other SNO avoidances
            if (iMyCachedActorClass == ActorClass.Barbarian && hashPowerHotbarAbilities.Contains(SNOPower.Barbarian_WrathOfTheBerserker) && GilesHasBuff(SNOPower.Barbarian_WrathOfTheBerserker))
            {
                if (c_iActorSNO == 223675 || c_iActorSNO == 402)
                {
                    // Ignore ice-balls altogether with wrath up
                    bIgnoreThisAvoidance = true;
                }
                else
                {
                    // Use half-health for anything else except arcanes or desecrate with wrath up
                    if (c_iActorSNO == 219702 || c_iActorSNO == 221225)
                        // Arcane
                        bIgnoreThisAvoidance = true;
                    else if (c_iActorSNO == 84608)
                        // Desecrator
                        dThisHealthAvoid *= 0.2;
                    else if (c_iActorSNO == 4803 || c_iActorSNO == 4804 || c_iActorSNO == 224225 || c_iActorSNO == 247987)
                        // Molten core
                        dThisHealthAvoid *= 1;
                    else
                        // Anything else
                        dThisHealthAvoid *= 0.3;
                }
            }
            // Add it to the list of known avoidance objects, *IF* our health is lower than this avoidance health limit
            if (!bIgnoreThisAvoidance && dThisHealthAvoid >= playerStatus.dCurrentHealthPct)
            {
                // Generate a "weight" for how badly we want to avoid this obstacle, based on a percentage of 100% the avoidance health is, multiplied into a max of 200 weight
                double dThisWeight = (200 * dThisHealthAvoid);
                hashAvoidanceObstacleCache.Add(new GilesObstacle(c_vPosition, (float)GetAvoidanceRadius(), c_iActorSNO, dThisWeight));
                // Is this one under our feet? If so flag it up so we can find an avoidance spot
                if (c_fCentreDistance <= GetAvoidanceRadius())
                {
                    bRequireAvoidance = true;
                    // Note if this is a travelling projectile or not so we can constantly update our safe points
                    if (hashAvoidanceSNOProjectiles.Contains(c_iActorSNO))
                        bTravellingAvoidance = true;
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
            return bWantThis;
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
                return dictAvoidanceRadius[c_iActorSNO];
            }
            catch
            {
                return c_fRadius;
            }

        }
        private static double GetAvoidanceRadius(int actorSNO)
        {
            try
            {
                return dictAvoidanceRadius[actorSNO];
            }
            catch
            {
                return c_fRadius;
            }

        }
        private static double GetAvoidanceHealth()
        {
            try
            {
                return dictAvoidanceHealth[c_iActorSNO];
            }
            catch
            {
                // 100% unless specified
                return 1;
            }
        }
    }
}
