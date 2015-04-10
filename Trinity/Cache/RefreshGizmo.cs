using System;
using Trinity.Config.Combat;
using Trinity.DbProvider;
using Trinity.Technicals;
using Zeta.Common.Plugins;
using Zeta.Game;
using Zeta.Game.Internals.Actors;
using Zeta.Game.Internals.Actors.Gizmos;
using Logger = Trinity.Technicals.Logger;

namespace Trinity
{
    public partial class Trinity : IPlugin
    {
        private static bool RefreshGizmo(bool AddToCache)
        {
            if (!(c_diaObject is DiaGizmo))
                return false;

            c_diaGizmo = c_diaObject as DiaGizmo;

            if (!Settings.WorldObject.AllowPlayerResurection && c_CacheObject.ActorSNO == DataDictionary.PLAYER_HEADSTONE_SNO)
            {
                c_InfosSubStep += "IgnoreHeadstones";
                AddToCache = false;
                return AddToCache;
            }
            // start as true, then set as false as we go. If nothing matches below, it will return true.
            AddToCache = true;

            bool openResplendentChest = c_CacheObject.InternalName.ToLower().Contains("chest_rare");

            // Ignore it if it's not in range yet, except shrines, pools of reflection and resplendent chests if we're opening chests
            if ((c_CacheObject.RadiusDistance > CurrentBotLootRange || c_CacheObject.RadiusDistance > 50) && c_CacheObject.Type != GObjectType.HealthWell &&
                c_CacheObject.Type != GObjectType.Shrine && c_CacheObject.RActorGuid != LastTargetRactorGUID)
            {
                AddToCache = false;
                c_InfosSubStep += "NotInRange";
            }

            // re-add resplendent chests
            if (openResplendentChest)
            {
                AddToCache = true;
                c_InfosSubStep = "";
            }

            CacheObjectIsBountyObjective();

            CacheObjectMinimapActive();


            if (c_diaGizmo != null)
            {
                c_CacheObject.GizmoType = c_diaGizmo.ActorInfo.GizmoType;
            }

            // Anything that's been disabled by a script
            bool isGizmoDisabledByScript = false;
            try
            {
                if (c_diaObject is DiaGizmo)
                {
                    isGizmoDisabledByScript = c_diaGizmo.IsGizmoDisabledByScript;
                }
            }
            catch
            {
                // add methode & Dictionary check
                AddObjectToNavigationObstacleCache();

                Logger.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement,
                    "Safely handled exception getting Gizmo-Disabled-By-Script attribute for object {0} [{1}]", c_CacheObject.InternalName, c_CacheObject.ActorSNO);
                c_InfosSubStep += "isGizmoDisabledByScriptException";
                AddToCache = false;
            }
            if (isGizmoDisabledByScript)
            {
                // add methode & Dictionary check
                AddObjectToNavigationObstacleCache();

                AddToCache = false;
                c_InfosSubStep += "GizmoDisabledByScript";
                return AddToCache;

            }


            // Anything that's Untargetable
            bool untargetable = false;
            try
            {
                if (c_diaObject is DiaGizmo)
                {
                    untargetable = CacheObjectUntargetable() > 0;
                }
            }
            catch
            {

            }


            // Anything that's Invulnerable
            bool invulnerable = false;
            try
            {
                if (c_diaObject is DiaGizmo)
                {
                    invulnerable = CacheObjectInvulnerable() > 0;
                }
            }
            catch
            {

            }

            bool noDamage = false;
            try
            {
                if (c_diaObject is DiaGizmo)
                {
                    noDamage = CacheObjectNoDamage() > 0;
                }
            }
            catch
            {
                // add methode & Dictionary check
                AddObjectToNavigationObstacleCache();

                Logger.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement,
                    "Safely handled exception getting NoDamage attribute for object {0} [{1}]", c_CacheObject.InternalName, c_CacheObject.ActorSNO);
                c_InfosSubStep += "NoDamage";
                AddToCache = false;
            }

            double minDistance;
            bool gizmoUsed = false;
            switch (c_CacheObject.Type)
            {
                case GObjectType.Door:
                    {
                        AddToCache = true;

                        // add methode & Dictionary check
                        AddObjectToNavigationObstacleCache();

                        var gizmoDoor = c_diaObject as GizmoDoor;
                        if (gizmoDoor != null && gizmoDoor.IsLocked)
                        {
                            c_InfosSubStep += "IsLocked";
                            return false;
                        }

                        // This is causing bugs I think? Maybe need to check a combination of attribute....
                        //if (gizmoDoor != null && !gizmoDoor.Operatable)
                        //{
                        //    c_InfosSubStep += "IsNotOperatable";
                        //    return false;
                        //}
                        if (c_diaObject is DiaGizmo && ((DiaGizmo)c_diaObject).HasBeenOperated)
                        {
                            c_InfosSubStep += "Door has been operated";
                            return false;
                        }

                        try
                        {
                            string currentAnimation = c_diaObject.CommonData.CurrentAnimation.ToString().ToLower();
                            gizmoUsed = currentAnimation.EndsWith("open") || currentAnimation.EndsWith("opening");

                            // special hax for A3 Iron Gates
                            if (currentAnimation.Contains("irongate") && currentAnimation.Contains("open"))
                                gizmoUsed = false;
                            if (currentAnimation.Contains("irongate") && currentAnimation.Contains("idle"))
                                gizmoUsed = true;
                        }
                        catch { }
                        if (gizmoUsed)
                        {
                            Blacklist3Seconds.Add(c_CacheObject.RActorGuid);
                            c_InfosSubStep += "Door is Open or Opening";
                            return false;
                        }

                        try
                        {
                            int gizmoState = CacheObjectGizmoState();
                            if (gizmoState == 1)
                            {
                                c_InfosSubStep += "GizmoState=1";
                                return false;
                            }
                        }
                        catch
                        {
                            c_InfosSubStep += "GizmoStateException";
                            return false;
                        }

                        if (untargetable)
                        {
                            // add methode & Dictionary check
                            AddObjectToNavigationObstacleCache();

                            c_InfosSubStep += "Untargetable";
                            return false;
                        }


                        if (AddToCache)
                        {
                            try
                            {
                                DiaGizmo door = null;
                                if (c_diaObject is DiaGizmo)
                                {
                                    door = (DiaGizmo)c_diaObject;

                                    if (door != null && door.IsGizmoDisabledByScript)
                                    {
                                        // add methode & Dictionary check
                                        AddObjectToNavigationObstacleCache();

                                        Blacklist3Seconds.Add(c_CacheObject.RActorGuid);
                                        c_InfosSubStep += "DoorDisabledbyScript";
                                        return false;
                                    }
                                }
                                else
                                {
                                    c_InfosSubStep += "InvalidCastToDoor";
                                    return false;
                                }
                            }

                            catch 
                            {
                                c_InfosSubStep += "InvalidDoor";
                                return false;
                            }
                        }
                    }
                    break;
                case GObjectType.Interactable:
                    {
                        AddToCache = true;

                        if (untargetable)
                        {
                            // add methode & Dictionary check
                            AddObjectToNavigationObstacleCache();

                            c_InfosSubStep += "Untargetable";
                            return false;
                        }


                        int endAnimation;
                        if (DataDictionary.InteractEndAnimations.TryGetValue(c_CacheObject.ActorSNO, out endAnimation))
                        {
                            if (endAnimation == (int)c_diaGizmo.CommonData.CurrentAnimation)
                            {
                                c_InfosSubStep += "EndAnimation";
                                return false;
                            }
                        }

                        if (c_diaGizmo.GizmoState == 1)
                        {
                            c_InfosSubStep += "GizmoState1";
                            return false;
                        }

                        if (c_diaGizmo.HasBeenOperated)
                        {
                            c_InfosSubStep += "GizmoHasBeenOperated";
                            return false;
                        }

                        c_CacheObject.Radius = c_diaObject.CollisionSphere.Radius;
                    }
                    break;
                case GObjectType.HealthWell:
                    {
                        AddToCache = true;
                        try
                        {
                            gizmoUsed = (CacheObjectGizmoCharges() <= 0 && CacheObjectGizmoCharges() > 0);
                        }
                        catch
                        {
                            Logger.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "Safely handled exception getting shrine-been-operated attribute for object {0} [{1}]", c_CacheObject.InternalName, c_CacheObject.ActorSNO);
                            AddToCache = true;
                            //return bWantThis;
                        }
                        try
                        {
                            int gizmoState = CacheObjectGizmoState();
                            if (gizmoState == 1)
                            {
                                AddToCache = false;
                                c_InfosSubStep += "GizmoState=1";
                                return AddToCache;
                            }
                        }
                        catch
                        {
                            AddToCache = false;
                            c_InfosSubStep += "GizmoStateException";
                            return AddToCache;
                        }
                        if (gizmoUsed)
                        {
                            c_InfosSubStep += "GizmoCharges";
                            AddToCache = false;
                            return AddToCache;
                        }
                        if (untargetable)
                        {
                            // add methode & Dictionary check
                            AddObjectToNavigationObstacleCache();

                            AddToCache = false;
                            c_InfosSubStep += "Untargetable";
                            return AddToCache;
                        }

                    }
                    break;
                case GObjectType.CursedShrine:
                case GObjectType.Shrine:
                    {
                        AddToCache = true;
                        // Shrines
                        // Check if either we want to ignore all shrines
                        if (!Settings.WorldObject.UseShrine)
                        {
                            // We're ignoring all shrines, so blacklist this one
                            c_InfosSubStep += "IgnoreAllShrinesSet";
                            AddToCache = false;
                            return AddToCache;
                        }

                        try
                        {
                            int gizmoState = CacheObjectGizmoState();
                            if (gizmoState == 1)
                            {
                                AddToCache = false;
                                c_InfosSubStep += "GizmoState=1";
                                return AddToCache;
                            }
                        }
                        catch
                        {
                            AddToCache = false;
                            c_InfosSubStep += "GizmoStateException";
                            return AddToCache;
                        }
                        if (untargetable)
                        {
                            // add methode & Dictionary check
                            AddObjectToNavigationObstacleCache();

                            AddToCache = false;
                            c_InfosSubStep += "Untargetable";
                            return AddToCache;
                        }


                        // Determine what shrine type it is, and blacklist if the user has disabled it
                        switch (c_CacheObject.ActorSNO)
                        {
                            case (int)SNOActor.Shrine_Global_Frenzied:  //Frenzy Shrine
                                if (!Settings.WorldObject.UseFrenzyShrine)
                                {
                                    Blacklist60Seconds.Add(c_CacheObject.RActorGuid);
                                    c_InfosSubStep += "IgnoreShrineOption";
                                    AddToCache = false;
                                }
                                if (Player.ActorClass == ActorClass.Monk && Settings.Combat.Monk.TROption.HasFlag(TempestRushOption.MovementOnly) && Hotbar.Contains(SNOPower.Monk_TempestRush))
                                {
                                    // Frenzy shrines are a huge time sink for monks using Tempest Rush to move, we should ignore them.
                                    AddToCache = false;
                                    c_InfosSubStep += "IgnoreShrineOption";
                                    return AddToCache;
                                }
                                break;

                            case (int)SNOActor.Shrine_Global_Fortune:  //Fortune Shrine 
                                if (!Settings.WorldObject.UseFortuneShrine)
                                {
                                    AddToCache = false;
                                    c_InfosSubStep += "IgnoreShrineOption";
                                    return AddToCache;
                                }
                                break;

                            case (int)SNOActor.Shrine_Global_Blessed: //Protection Shrine
                                if (!Settings.WorldObject.UseProtectionShrine)
                                {
                                    AddToCache = false;
                                    c_InfosSubStep += "IgnoreShrineOption";
                                    return AddToCache;
                                }
                                break;

                            case (int)SNOActor.Shrine_Global_Reloaded: //Empowered Shrine - Shrine_Global_Reloaded
                                if (!Settings.WorldObject.UseEmpoweredShrine)
                                {
                                    AddToCache = false;
                                    c_InfosSubStep += "IgnoreShrineOption";
                                    return AddToCache;
                                }
                                break;

                            case (int)SNOActor.Shrine_Global_Enlightened:  //Enlightened Shrine - Shrine_Global_Enlightened
                                if (!Settings.WorldObject.UseEnlightenedShrine)
                                {
                                    AddToCache = false;
                                    c_InfosSubStep += "IgnoreShrineOption";
                                    return AddToCache;
                                }
                                break;

                            case (int)SNOActor.Shrine_Global_Hoarder:  //Fleeting Shrine
                                if (!Settings.WorldObject.UseFleetingShrine)
                                {
                                    AddToCache = false;
                                    c_InfosSubStep += "IgnoreShrineOption";
                                    return AddToCache;
                                }
                                break;

                            case (int)SNOActor.x1_LR_Shrine_Infinite_Casting:  //Channeling Pylon - x1_LR_Shrine_Infinite_Casting
                                if (!Settings.WorldObject.UseChannelingPylon)
                                {
                                    AddToCache = false;
                                    c_InfosSubStep += "IgnoreShrineOption";
                                    return AddToCache;
                                }
                                break;

                            case (int)SNOActor.x1_LR_Shrine_Electrified:  //Conduit Pylon - x1_LR_Shrine_Electrified
                                if (!Settings.WorldObject.UseConduitPylon)
                                {
                                    AddToCache = false;
                                    c_InfosSubStep += "IgnoreShrineOption";
                                    return AddToCache;
                                }
                                break;

                            case (int)SNOActor.x1_LR_Shrine_Invulnerable:  //Shield Pylon -x1_LR_Shrine_Invulnerable
                                if (!Settings.WorldObject.UseShieldPylon)
                                {
                                    AddToCache = false;
                                    c_InfosSubStep += "IgnoreShrineOption";
                                    return AddToCache;
                                }
                                break;

                            case (int)SNOActor.x1_LR_Shrine_Run_Speed:  //Speed Pylon - x1_LR_Shrine_Run_Speed
                                if (!Settings.WorldObject.UseSpeedPylon)
                                {
                                    AddToCache = false;
                                    c_InfosSubStep += "IgnoreShrineOption";
                                    return AddToCache;
                                }
                                break;
                            case (int)SNOActor.x1_LR_Shrine_Damage:  //Power Pylon - x1_LR_Shrine_Damage
                                if (!Settings.WorldObject.UsePowerPylon)
                                {
                                    AddToCache = false;
                                    c_InfosSubStep += "IgnoreShrineOption";
                                    return AddToCache;
                                }
                                break;
                        }  //end switch

                        // Bag it!
                        c_CacheObject.Radius = 4f;
                        break;
                    }
                case GObjectType.Barricade:
                    {
                        AddToCache = true;

                        var gizmoDestructible = c_diaObject as GizmoDestructible;
                        if (gizmoDestructible != null && gizmoDestructible.HitpointsCurrentPct <= 0)
                        {
                            c_InfosSubStep += "HitPoints0";
                            return false;
                        }

                        if (noDamage)
                        {
                            // add methode & Dictionary check
                            AddObjectToNavigationObstacleCache();

                            AddToCache = false;
                            c_InfosSubStep += "NoDamage";
                            return AddToCache;
                        }
                        if (untargetable)
                        {
                            // add methode & Dictionary check
                            AddObjectToNavigationObstacleCache();

                            AddToCache = false;
                            c_InfosSubStep += "Untargetable";
                            return AddToCache;
                        }


                        if (invulnerable)
                        {
                            // add methode & Dictionary check
                            AddObjectToNavigationObstacleCache();

                            AddToCache = false;
                            c_InfosSubStep += "Invulnerable";
                            return AddToCache;
                        }

                        float maxRadiusDistance;

                        if (DataDictionary.DestructableObjectRadius.TryGetValue(c_CacheObject.ActorSNO, out maxRadiusDistance))
                        {
                            if (c_CacheObject.RadiusDistance < maxRadiusDistance)
                            {
                                AddToCache = true;
                                c_InfosSubStep = "";
                            }
                        }

                        if (DateTime.UtcNow.Subtract(PlayerMover.LastGeneratedStuckPosition).TotalSeconds <= 1)
                        {
                            AddToCache = true;
                            c_InfosSubStep = "";
                            break;
                        }

                        // Set min distance to user-defined setting
                        minDistance = Settings.WorldObject.DestructibleRange + c_CacheObject.Radius;
                        if (_forceCloseRangeTarget)
                            minDistance += 6f;

                        // This object isn't yet in our destructible desire range
                        if (minDistance <= 0 || c_CacheObject.RadiusDistance > minDistance)
                        {
                            c_InfosSubStep += "NotInBarricadeRange";
                            AddToCache = false;
                            return AddToCache;
                        }

                        break;
                    }
                case GObjectType.Destructible:
                    {
                        AddToCache = true;

                        var gizmoDestructible = c_diaObject as GizmoDestructible;
                        try
                        {
                        if (gizmoDestructible != null && gizmoDestructible.HitpointsCurrentPct <= 0)
                        {
                                c_InfosSubStep += "HitPoints0";
                            return false;
                        }
                        }
                        catch { }

                        if (noDamage)
                        {
                            // add methode & Dictionary check
                            AddObjectToNavigationObstacleCache();

                            AddToCache = false;
                            c_InfosSubStep += "NoDamage";
                            return AddToCache;
                        }

                        if (invulnerable)
                        {
                            // add methode & Dictionary check
                            AddObjectToNavigationObstacleCache();

                            AddToCache = false;
                            c_InfosSubStep += "Invulnerable";
                            return AddToCache;
                        }
                        if (untargetable)
                        {
                            // add methode & Dictionary check
                            AddObjectToNavigationObstacleCache();

                            AddToCache = false;
                            c_InfosSubStep += "Untargetable";
                            return AddToCache;
                        }


                        if (Player.ActorClass == ActorClass.Monk && Hotbar.Contains(SNOPower.Monk_TempestRush) && TimeSinceUse(SNOPower.Monk_TempestRush) <= 150)
                        {
                            AddToCache = false;
                            c_InfosSubStep += "MonkTR";
                            break;
                        }

                        if (Player.ActorClass == ActorClass.Monk && Hotbar.Contains(SNOPower.Monk_SweepingWind) && GetHasBuff(SNOPower.Monk_SweepingWind))
                        {
                            AddToCache = false;
                            c_InfosSubStep += "MonkSW";
                            break;
                        }

                        if (c_CacheObject.IsQuestMonster || c_CacheObject.IsMinimapActive)
                        {
                            AddToCache = true;
                            c_InfosSubStep = "";
                            break;
                        }

                        if (!DataDictionary.ForceDestructibles.Contains(c_CacheObject.ActorSNO) && Settings.WorldObject.DestructibleOption == DestructibleIgnoreOption.ForceIgnore)
                        {
                            AddToCache = false;
                            c_InfosSubStep += "ForceIgnoreDestructibles";
                            break;
                        }

                        if (DateTime.UtcNow.Subtract(PlayerMover.LastGeneratedStuckPosition).TotalSeconds <= 1)
                        {
                            AddToCache = true;
                            c_InfosSubStep = "";
                            break;
                        }

                        // Set min distance to user-defined setting
                        minDistance = Settings.WorldObject.DestructibleRange;
                        if (_forceCloseRangeTarget)
                            minDistance += 6f;

                        // Only break destructables if we're stuck and using IgnoreNonBlocking
                        if (Settings.WorldObject.DestructibleOption == DestructibleIgnoreOption.DestroyAll)
                        {
                            minDistance += 12f;
                            AddToCache = true;
                            c_InfosSubStep = "";
                        }

                        float maxRadiusDistance;

                        if (DataDictionary.DestructableObjectRadius.TryGetValue(c_CacheObject.ActorSNO, out maxRadiusDistance))
                        {
                            if (c_CacheObject.RadiusDistance < maxRadiusDistance)
                            {
                                AddToCache = true;
                                c_InfosSubStep = "";
                            }
                        }
                        // Always add large destructibles within ultra close range
                        if (!AddToCache && c_CacheObject.Radius >= 10f && c_CacheObject.RadiusDistance <= 5.9f)
                        {
                            AddToCache = true;
                            c_InfosSubStep = "";
                            break;
                        }

                        // This object isn't yet in our destructible desire range
                        if (AddToCache && (minDistance <= 1 || c_CacheObject.RadiusDistance > minDistance) && PlayerMover.GetMovementSpeed() >= 1)
                        {
                            AddToCache = false;
                            c_InfosSubStep += "NotInDestructableRange";
                        }
                        if (AddToCache && c_CacheObject.RadiusDistance <= 2f && DateTime.UtcNow.Subtract(PlayerMover.LastGeneratedStuckPosition).TotalMilliseconds > 500)
                        {
                            AddToCache = false;
                            c_InfosSubStep += "NotStuck2";
                        }

                        // Add if we're standing still and bumping into it
                        if (c_CacheObject.RadiusDistance <= 2f && PlayerMover.GetMovementSpeed() <= 0)
                        {
                            AddToCache = true;
                            c_InfosSubStep = "";
                        }

                        if (c_CacheObject.RActorGuid == LastTargetRactorGUID)
                        {
                            AddToCache = true;
                            c_InfosSubStep = "";
                        }

                        break;
                    }
                case GObjectType.CursedChest:
                case GObjectType.Container:
                    {
                        AddToCache = false;

                        bool isRareChest = c_CacheObject.InternalName.ToLower().Contains("chest_rare") || DataDictionary.ResplendentChestIds.Contains(c_CacheObject.ActorSNO);
                        bool isChest = (!isRareChest && c_CacheObject.InternalName.ToLower().Contains("chest")) ||
                            DataDictionary.ContainerWhiteListIds.Contains(c_CacheObject.ActorSNO); // We know it's a container but this is not a known rare chest
                        bool isCorpse = c_CacheObject.InternalName.ToLower().Contains("corpse");
                        bool isWeaponRack = c_CacheObject.InternalName.ToLower().Contains("rack");
                        bool isGroundClicky = c_CacheObject.InternalName.ToLower().Contains("ground_clicky");

                        // We want to do some vendoring, so don't open anything new yet
                        if (ForceVendorRunASAP)
                        {
                            AddToCache = false;
                            c_InfosSubStep += "ForceVendorRunASAP";
                        }

                        // Already open, blacklist it and don't look at it again
                        bool chestOpen;
                        try
                        {
                            chestOpen = CacheObjectIsChestOpen() > 0;
                        }
                        catch
                        {
                            Logger.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "Safely handled exception getting container-been-opened attribute for object {0} [{1}]", c_CacheObject.InternalName, c_CacheObject.ActorSNO);
                            c_InfosSubStep += "ChestOpenException";
                            AddToCache = false;
                            return AddToCache;
                        }

                        // Check if chest is open
                        if (chestOpen)
                        {
                            // It's already open!
                            AddToCache = false;
                            c_InfosSubStep += "AlreadyOpen";
                            return AddToCache;
                        }

                        if (untargetable)
                        {
                            // add methode & Dictionary check
                            AddObjectToNavigationObstacleCache();

                            AddToCache = false;
                            c_InfosSubStep += "Untargetable";
                            return AddToCache;
                        }

                        // Resplendent chests have no range check
                        if (isRareChest && Settings.WorldObject.OpenRareChests)
                        {
                            AddToCache = true;
                            return AddToCache;
                        }

                        // Regular container, check range
                        if (c_CacheObject.RadiusDistance <= Settings.WorldObject.ContainerOpenRange)
                        {
                            if (isChest && Settings.WorldObject.OpenContainers)
                                return true;

                            if (isCorpse && Settings.WorldObject.InspectCorpses)
                                return true;

                            if (isGroundClicky && Settings.WorldObject.InspectGroundClicky)
                                return true;

                            if (isWeaponRack && Settings.WorldObject.InspectWeaponRacks)
                                return true;
                        }

                        if (c_CacheObject.IsQuestMonster)
                        {
                            AddToCache = true;
                            return AddToCache;
                        }

                        if (Settings.WorldObject.OpenAnyContainer)
                        {
                            AddToCache = true;
                            return AddToCache;
                        }

                        if (!isChest && !isCorpse && !isRareChest)
                        {
                            c_InfosSubStep += "InvalidContainer";
                        }
                        else
                        {
                            c_InfosSubStep += "IgnoreContainer";
                        }
                        break;
                    }
            }
            return AddToCache;
        }

        private static int CacheObjectIsChestOpen()
        {
            return c_diaObject.CommonData.GetAttribute<int>(ActorAttributeType.ChestOpen);
        }

        private static int CacheObjectGizmoCharges()
        {
            return c_diaObject.CommonData.GetAttribute<int>(ActorAttributeType.GizmoCharges);
        }

        private static int CacheObjectGizmoState()
        {
            return c_diaObject.CommonData.GetAttribute<int>(ActorAttributeType.GizmoState);
        }

        private static int CacheObjectNoDamage()
        {
            return c_diaObject.CommonData.GetAttribute<int>(ActorAttributeType.NoDamage);
        }

        private static int CacheObjectInvulnerable()
        {
            return c_diaObject.CommonData.GetAttribute<int>(ActorAttributeType.Invulnerable);
        }

        private static int CacheObjectUntargetable()
        {
            return c_diaObject.CommonData.GetAttribute<int>(ActorAttributeType.Untargetable);
        }
    }
}
