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
        public static SNOTable _getActorTable;

        #region Constructors

        public CacheBase() { }

        public CacheBase(DiaObject rActor)
        {
            Object = rActor;
            Source = rActor.CommonData;
            ACDGuid = rActor.ACDGuid;
            RActorGuid = rActor.RActorGuid;
            ActorSNO = rActor.ActorSNO;

            TrinityItemType trinityItemType;
            ActorType actorType;
            GetActorAndItemType(rActor, ActorSNO, out actorType, out trinityItemType);
            ActorType = actorType;
            TrinityItemType = trinityItemType;

            ActorMeta = CacheMeta.GetOrCreateActorMeta(this);                                   
            LastUpdated = CacheManager.LastUpdated;
            ACDItem = Source as ACDItem;
            DiaGizmo = Object as DiaGizmo;
            DiaItem = Object as DiaItem;
            DiaUnit = Object as DiaUnit;
            InternalName = GetInternalName(ActorMeta, Source);
            TrinityType = GetTrinityType(Source, ActorType, ActorSNO, ActorMeta.GizmoType, InternalName);
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
        public TrinityItemType TrinityItemType { get; set; }

        #endregion

        #region Methods

        public bool IsValid
        {
            get { return IsProperValid(Object, Source, ActorType, ACDGuid, RActorGuid, ActorSNO); }
        }

        public static bool IsProperValid(DiaObject obj, ACD acd, ActorType actorType, int acdGuid, int rActorGuid, int actorSNO)
        {
            if (acd == null || obj == null)
                return false;

            if (!acd.IsValid || !obj.IsValid || acd.IsDisposed)
                return false;

            if (actorType == ActorType.Item && (!(obj is DiaItem) || !(acd is ACDItem)))
                return false;

            if (actorType == ActorType.Monster && (obj as DiaUnit) == null)
                return false;

            if (actorType == ActorType.Gizmo && (obj as DiaGizmo) == null)
                return false;
            
            if (acdGuid == -1 || rActorGuid == -1)
                return false;

            return true;
        }

        private void GetActorAndItemType(DiaObject obj, int actorSNO, out ActorType outActorType, out TrinityItemType outTrinityItemType)
        {
            TrinityItemType trinityItemType = TrinityItemType.Unknown;
            if (DataDictionary.ItemTypeReference.ContainsKey(actorSNO))
            {
                outTrinityItemType = trinityItemType;
                outActorType = ActorType.Item;
                return;
            }
            outTrinityItemType = trinityItemType;
            outActorType = obj.ActorType;            
        }

        /// <summary>
        /// Safely request of a property from DiaUnit
        /// </summary>
        public T GetUnitProperty<T>(Func<DiaUnit, T> selector, [CallerMemberName] string caller = "")
        {
            try
            {
                return selector != null && DiaUnit != null && DiaUnit.IsValid && Source.IsProbablyValid() ? selector(DiaUnit) : CacheUtilities.Default<T>();
            }
            catch (Exception ex)
            {
                if (ex.Message.StartsWith("Only part of a ReadProcessMemory"))
                {
                    Logger.LogError("DB Memory Exception in GetUnitProperty Caller={0} ACDGuid={1} ActorType={2} Name={3} SNO={4} Exception={5} {6}",
                        caller, ACDGuid, ActorType, (Source != null) ? Source.Name : string.Empty, ActorSNO, ex.Message, ex.InnerException);
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
                return selector != null && ACDItem != null && ACDItem.IsValid && Source.IsProbablyValid() ? selector(ACDItem) : CacheUtilities.Default<T>();
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
                return selector != null && DiaItem != null && DiaItem.IsValid && Source.IsProbablyValid() ? selector(DiaItem) : CacheUtilities.Default<T>();
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
                return selector != null && DiaGizmo != null && DiaGizmo.IsValid && Source.IsProbablyValid() ? selector(DiaGizmo) : CacheUtilities.Default<T>();
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
                return selector != null && Object != null && Object.IsValid && Source.IsProbablyValid() ? selector(Object) : CacheUtilities.Default<T>();
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
                return selector != null && Source != null && Source.IsProbablyValid() ? selector(Source) : CacheUtilities.Default<T>();
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
                return selector != null && DiaUnit != null && DiaUnit.IsValid && Source.IsProbablyValid() ? selector(source) : CacheUtilities.Default<T2>();
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

            if (DataDictionary.CursedChestSNO.Contains(actorSNO))
                return TrinityObjectType.CursedChest;

            if (DataDictionary.CursedShrineSNO.Contains(actorSNO))
                return TrinityObjectType.CursedShrine;

            if (DataDictionary.ShrineSNO.Contains(actorSNO))
                return TrinityObjectType.Shrine;

            if (DataDictionary.GoldSNO.Contains(actorSNO))
                return TrinityObjectType.Gold;

            if (actorType == ActorType.Item || DataDictionary.ForceToItemOverrideIds.Contains(actorSNO))
                return TrinityObjectType.Item;

            if (DataDictionary.InteractWhiteListIds.Contains(actorSNO))
                return TrinityObjectType.Interactable;

            if (DataDictionary.AvoidanceTypeSNO.ContainsKey(actorSNO) || DataDictionary.AvoidanceSNO.Contains(actorSNO))
                return TrinityObjectType.Avoidance;

            if (actorType == ActorType.Monster)
                return TrinityObjectType.Unit;

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

            if (DataDictionary.PlayerBannerSNO.Contains(actorSNO))
                return TrinityObjectType.Banner;

            if (internalName.StartsWith("Waypoint-"))
                return TrinityObjectType.Waypoint;

            return TrinityObjectType.Unknown;
        }

        public static string GetInternalName(CacheMeta.ActorMeta meta, ACD acd)
        {
            if (meta.IsValid)
                return meta.InternalName;

            return Trinity.NameNumberTrimRegex.Replace(acd.Name, "");
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
