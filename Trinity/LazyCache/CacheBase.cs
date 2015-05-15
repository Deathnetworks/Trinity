using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Web.Security;
using System.Web.UI.WebControls;
using System.Windows;
using System.Windows.Controls.Primitives;
using Trinity.Technicals;
using Zeta.Game;
using Zeta.Common;
using Zeta.Game.Internals;
using Zeta.Game.Internals.Actors;
using Zeta.Game.Internals.Actors.Gizmos;
using Zeta.Game.Internals.SNO;
using Logger = Trinity.Technicals.Logger;

namespace Trinity.LazyCache
{
    /// <summary>
    /// Base for cache objects. 
    /// </summary>
    public class CacheBase
    {
        #region Constructors

        public CacheBase() { }

        public CacheBase(DiaObject rActor)
        {
            Object = rActor;
            Source = rActor.CommonData;
            ACDGuid = rActor.ACDGuid;
            RActorGuid = rActor.RActorGuid;
            ActorSNO = rActor.ActorSNO;
            ActorType = rActor.ActorType;
            ActorMeta = CacheMeta.GetOrCreateActorMeta(this);            
            InternalName = ActorMeta.InternalName;
            TrinityType = GetTrinityType(Source, ActorType, ActorSNO, ActorMeta.GizmoType, InternalName);
            LastUpdated = CacheManager.LastUpdated;
            ACDItem = Source as ACDItem;
            DiaGizmo = Object as DiaGizmo;
            DiaItem = Object as DiaItem;
            DiaUnit = Object as DiaUnit;            
        }

        #endregion

        #region Properties

        public int ACDGuid { get; set; }
        public int RActorGuid { get; set; }
        public int ActorSNO { get; set; }
        public ActorType ActorType { get; set; }
        public TrinityObjectType TrinityType { get; set; }
        public ACD Source { get; set; }
        public DiaObject Object { get; set; }
        public string InternalName { get; set; }
        public CacheMeta.ActorMeta ActorMeta { get; set; }
        public DateTime LastUpdated { get; set; }
        public ACDItem ACDItem { get; set; }
        public DiaGizmo DiaGizmo { get; set; }
        public DiaItem DiaItem { get; set; }
        public DiaUnit DiaUnit { get; set; }

        #endregion

        #region Methods

        public bool IsValid
        {
            get { return IsProperValid(Object, Source, ActorType, ACDGuid, RActorGuid); }
        }

        public static bool IsProperValid(DiaObject obj, ACD acd, ActorType actorType, int acdGuid, int rActorGuid)
        {
            if (acd == null || obj == null)
                return false;

            if (!acd.IsValid || !obj.IsValid || acd.IsDisposed)
                return false;

            if (actorType == ActorType.Monster && (obj as DiaUnit) == null)
                return false;

            if (actorType == ActorType.Gizmo && (obj as DiaGizmo) == null)
                return false;

            if (acdGuid == -1 || rActorGuid == -1)
                return false;

            //if (string.IsNullOrEmpty(internalName))
            //    return false;

            return true;
        }

        /// <summary>
        /// Safely request of a property from DiaUnit
        /// </summary>
        public T GetUnitProperty<T>(Func<DiaUnit, T> selector, [CallerMemberName] string caller = "")
        {
            try
            {
                return selector != null && DiaUnit != null && DiaUnit.IsValid && Source.IsProperValid() ? selector(DiaUnit) : CacheUtilities.Default<T>();
            }
            catch (Exception ex)
            {
                if (ex.Message.StartsWith("Only part of a ReadProcessMemory"))
                {
                    var acd = ZetaDia.Actors.GetACDByGuid(ACDGuid);
                    var actor = ZetaDia.Actors.GetActorsOfType<DiaUnit>().Where(u => u.ACDGuid == ACDGuid);

                    //Logger.LogError("DB Memory Exception in GetUnitProperty Caller={0} ACDGuid={1} ActorType={2} Name={3} SNO={4} Exception={5} {6}",
                    //    caller, ACDGuid, ActorType, (Source != null) ? Source.Name : string.Empty, ActorSNO, ex.Message, ex.InnerException);
                }
                else throw;
            }
            return CacheUtilities.Default<T>();
        }

        /// <summary>
        /// Safely request of a property from DiaUnit
        /// </summary>
        public T GetACDItemProperty<T>(Func<ACDItem, T> selector, [CallerMemberName] string caller = "")
        {
            try
            {
                return selector != null && ACDItem != null && ACDItem.IsValid && Source.IsProperValid() ? selector(ACDItem) : CacheUtilities.Default<T>();
            }
            catch (Exception ex)
            {
                if (ex.Message.StartsWith("Only part of a ReadProcessMemory"))
                {
                    Logger.LogError("DB Memory Exception in GetACDItemProperty Caller={0} ACDGuid={1} ActorType={2} Name={3} SNO={4} Exception={5} {6}",
                        caller, ACDGuid, ActorType, (Source != null) ? Source.Name : String.Empty, ActorSNO, ex.Message, ex.InnerException);
                }
                else throw;
            }
            return CacheUtilities.Default<T>();
        }

        /// <summary>
        /// Safely request of a property from DiaUnit
        /// </summary>
        public T GetDiaItemProperty<T>(Func<DiaItem, T> selector, [CallerMemberName] string caller = "")
        {
            try            
            {
                return selector != null && DiaItem != null && DiaItem.IsValid && Source.IsProperValid() ? selector(DiaItem) : CacheUtilities.Default<T>();
            }
            catch (Exception ex)
            {
                if (ex.Message.StartsWith("Only part of a ReadProcessMemory"))
                {
                    Logger.LogError("DB Memory Exception in GetDiaItemProperty Caller={0} ACDGuid={1} ActorType={2} Name={3} SNO={4} Exception={5} {6}",
                        caller, ACDGuid, ActorType, (Source != null) ? Source.Name : String.Empty, (Source != null) ? Source.ActorSNO.ToString() : String.Empty, ex.Message, ex.InnerException);
                }
                else throw;
            }
            return CacheUtilities.Default<T>();
        }

        /// <summary>
        /// Safely request of a property from DiaGizmo
        /// </summary>
        public T GetGizmoProperty<T>(Func<DiaGizmo, T> selector, [CallerMemberName] string caller = "")
        {
            try
            {
                return selector != null && DiaGizmo != null && DiaGizmo.IsValid && Source.IsProperValid() ? selector(DiaGizmo) : CacheUtilities.Default<T>();
            }
            catch (Exception ex)
            {
                if (ex.Message.StartsWith("Only part of a ReadProcessMemory"))
                {
                    Logger.LogError("DB Memory Exception in GetGizmoProperty Caller={0} ACDGuid={1} ActorType={2} Name={3} SNO={4} Exception={5} {6}",
                        caller, ACDGuid, ActorType, (Source != null) ? Source.Name : String.Empty, ActorSNO, ex.Message, ex.InnerException);
                }
                else throw;
            }
            return CacheUtilities.Default<T>();
        }

        /// <summary>
        /// Safely request of a property from DiaObject 
        /// </summary>
        public T GetObjectProperty<T>(Func<DiaObject, T> selector, [CallerMemberName] string caller = "")
        {
            try
            {
                return selector != null && Object != null && Object.IsValid && Source.IsProperValid() ? selector(Object) : CacheUtilities.Default<T>();
            }
            catch (Exception ex)
            {
                if (ex.Message.StartsWith("Only part of a ReadProcessMemory"))
                {
                    Logger.LogError("DB Memory Exception in GetObjectProperty Caller={0} ACDGuid={1} ActorType={2} Name={3} SNO={4} Exception={5} {6}",
                        caller, ACDGuid, ActorType, (Source != null) ? Source.Name : String.Empty, (Source != null) ? Source.ActorSNO.ToString() : String.Empty, ex.Message, ex.InnerException);
                }
                else throw;                
            }
            return CacheUtilities.Default<T>();
        }

        /// <summary>
        /// Safely request of a property from Source ACD 
        /// </summary>
        public T GetSourceProperty<T>(Func<ACD, T> selector, [CallerMemberName] string caller = "")
        {
            try
            {
                return selector != null && Source != null && Source.IsProperValid() ? selector(Source) : CacheUtilities.Default<T>();
            }
            catch (Exception ex)
            {
                if (ex.Message.StartsWith("Only part of a ReadProcessMemory"))
                {
                    Logger.LogError("DB Memory Exception in GetSourceProperty Caller={0} ACDGuid={1} ActorType={2} Name={3} SNO={4} Exception={5} {6}",
                        caller, ACDGuid, ActorType, (Source != null) ? Source.Name : String.Empty, (Source != null) ? Source.ActorSNO.ToString() : String.Empty, ex.Message, ex.InnerException);
                }
                else throw;
            }
            return CacheUtilities.Default<T>();
        }

        /// <summary>
        /// Safely request of a property from the specified source object
        /// </summary>
        public T2 GetProperty<T1, T2>(T1 source, Func<T1, T2> selector, [CallerMemberName] string caller = "")
        {
            try
            {
                return selector != null && DiaUnit != null && DiaUnit.IsValid && Source.IsProperValid() ? selector(source) : CacheUtilities.Default<T2>();
            }
            catch (Exception ex)
            {
                if (ex.Message.StartsWith("Only part of a ReadProcessMemory"))
                {
                    Logger.LogError("DB Memory Exception in GetProperty SourceType={5} Caller={0} ACDGuid={1} InternalName={2} Exception={3} {4}",
                        caller, ACDGuid, Source.Name, ex.Message, ex.InnerException, typeof(T1));
                }
                else throw;
            }
            return CacheUtilities.Default<T2>();
        }

        /// <summary>
        /// Replace the data source with a new object reference
        /// </summary>
        public void UpdateSource(DiaObject rActor)
        {
            Object = rActor;
            Source = rActor.CommonData;
        }

        /// <summary>
        /// Replace the data source with a new object reference
        /// </summary>
        public TrinityObject UpdateSource(TrinityObject obj, ACD acd, DiaObject rActor)
        {            
            obj.Object = rActor;
            obj.Source = acd;
            return obj;
        }

        /// <summary>
        /// Trinity's actor type.
        /// </summary>
        internal static TrinityObjectType GetTrinityType(ACD acd, ActorType actorType, int actorSNO, GizmoType gizmoType, string internalName)
        {

            if (actorType == ActorType.Monster)
                return TrinityObjectType.Unit;

            if (internalName.Contains("CursedChest"))
                return TrinityObjectType.CursedChest;

            if (internalName.Contains("CursedShrine"))
                return TrinityObjectType.CursedShrine;

            if (DataDictionary.Shrines.Any(s => s == (SNOActor)actorSNO))
                return TrinityObjectType.Shrine;

            if (acd is ACDItem && internalName.ToLower().Contains("gold"))
                return TrinityObjectType.Gold;

            if (actorType == ActorType.Item || DataDictionary.ForceToItemOverrideIds.Contains(actorSNO))
                return TrinityObjectType.Item;

            if (DataDictionary.InteractWhiteListIds.Contains(actorSNO))
                return TrinityObjectType.Interactable;

            if (AvoidanceManager.GetAvoidanceType(actorSNO) != AvoidanceType.None)
                return TrinityObjectType.Avoidance;

            if (actorType == ActorType.Gizmo)
            {
                switch (gizmoType)
                {
                    case GizmoType.HealingWell:
                        return TrinityObjectType.HealthWell;

                    case GizmoType.Door:
                        return TrinityObjectType.Door;

                    case GizmoType.BreakableDoor:
                        return TrinityObjectType.Barricade;

                    case GizmoType.PoolOfReflection:
                    case GizmoType.PowerUp:
                        return TrinityObjectType.Shrine;

                    case GizmoType.Chest:
                        return TrinityObjectType.Container;

                    case GizmoType.DestroyableObject:
                    case GizmoType.BreakableChest:
                        return TrinityObjectType.Destructible;

                    case GizmoType.PlacedLoot:
                    case GizmoType.Switch:
                    case GizmoType.Headstone:
                        return TrinityObjectType.Interactable;

                    case GizmoType.Portal:
                        return TrinityObjectType.Portal;
                }
            }

            if (acd.ActorType == ActorType.Player)
                return TrinityObjectType.Player;

            if (internalName.StartsWith("Banner_Player"))
                return TrinityObjectType.Banner;

            if (internalName.StartsWith("Waypoint-"))
                return TrinityObjectType.Waypoint;

            return TrinityObjectType.Unknown;
        }

        public override int GetHashCode()
        {
            return ACDGuid;
        }

        public override bool Equals(object obj)
        {
            return GetHashCode() == obj.GetHashCode();
        }

        #endregion

        #region Operators

        public TrinityUnit ToTrinityUnit()
        {
            return CopyTo(new TrinityUnit());
        }

        public TrinityGizmo ToTrinityGizmo()
        {
            return CopyTo(new TrinityGizmo());
        }

        public TrinityAvoidance ToTrinityAvoidance()
        {
            return CopyTo(new TrinityAvoidance());
        }

        public TrinityItem ToTrinityItem()
        {
            return CopyTo(new TrinityItem());
        }

        public TrinityPlayer ToTrinityPlayer()
        {
            return CopyTo(new TrinityPlayer());
        }

        public TrinityObject ToTrinityObject()
        {
            return CopyTo(new TrinityObject());
        }

        public T CopyTo<T>(T other) where T : CacheBase
        {
            other.Object = Object;
            other.Source = Source;                
            other.ACDGuid = ACDGuid;
            other.RActorGuid = RActorGuid;
            other.ActorSNO = ActorSNO;
            other.ActorType = ActorType;
            other.ActorMeta = ActorMeta;
            other.InternalName = InternalName;
            other.TrinityType = TrinityType;
            other.LastUpdated = LastUpdated;
            other.ACDItem = ACDItem;
            other.DiaGizmo = DiaGizmo;
            other.DiaItem = DiaItem;
            other.DiaUnit = DiaUnit;
            return other;
        }

        #endregion
    }
}
