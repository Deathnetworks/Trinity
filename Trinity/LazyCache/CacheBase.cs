using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Windows;
using System.Windows.Controls.Primitives;
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

        public CacheBase(ACD acd)
        {
            ACDGuid = acd.ACDGuid;     
            Source = acd;       
        }

        public CacheBase(int acdGuid)
        {
            ACDGuid = acdGuid;
        }

        #endregion

        #region Fields

        private readonly CacheField<TrinityObjectType> _trinityType = new CacheField<TrinityObjectType>();
        private readonly CacheField<ActorType> _actorType = new CacheField<ActorType>(500);
        private readonly CacheField<DiaObject> _object = new CacheField<DiaObject>();
        private readonly CacheField<ACD> _source = new CacheField<ACD>();

        #endregion

        #region Properties

        /// <summary>
        /// Unique id for getting actor from DB's ActorManager
        /// </summary>
        public int ACDGuid { get; set; }

        /// <summary>
        /// Time since source object was assigned.
        /// </summary>
        public DateTime LastUpdated
        {
            get { return _source.LastUpdate; }
        }

        /// <summary>
        /// Trinity's actor type
        /// </summary>
        public TrinityObjectType TrinityType
        {
            get { return _trinityType.IsCacheValid ? _trinityType.CachedValue : (_trinityType.CachedValue = TrinityObject.GetTrinityObjectType(Source)); }
            set { _trinityType.SetValueOverride(value); }
        }

        /// <summary>
        /// DB's actor type
        /// </summary>
        public ActorType ActorType
        {
            get { return _actorType.IsCacheValid ? _actorType.CachedValue : (_actorType.CachedValue = Source.ActorType); }
            set { _actorType.SetValueOverride(value); }
        }

        /// <summary>
        /// DB's actual DiaObject (accessing properties reads directly from Diablo memory)
        /// </summary>
        public DiaObject Object
        {
            get { return _object.IsCacheValid ? _object.CachedValue : (_object.CachedValue = CacheManager.GetRActorOfTypeByACDGuid<DiaObject>(ACDGuid)); }
            set { _object.SetValueOverride(value); }
        }

        /// <summary>
        /// DB's actual ACD object (accessing properties reads directly from Diablo memory)
        /// </summary>
        public ACD Source
        {
            get { return _source.IsCacheValid ? _source.CachedValue : ACDGuid != 0 ? (_source.CachedValue = ZetaDia.Actors.GetACDById(ACDGuid)) : null; }
            set { _source.SetValueOverride(value); }
        }

        /// <summary>
        /// DB's actual Gizmo object (accessing properties reads directly from Diablo memory)
        /// </summary>
        public DiaGizmo Gizmo
        {
            get { return Object as DiaGizmo; }
        }

        /// <summary>
        /// DB's actual Item object (accessing properties reads directly from Diablo memory)
        /// </summary>
        public ACDItem Item
        {
            get { return Source as ACDItem; }
        }


        /// <summary>
        /// DB's actual Gizmo (accessing properties reads directly from Diablo memory)
        /// </summary>
        public DiaItem DiaItem
        {
            get { return Object as DiaItem; }
        }

        /// <summary>
        /// DB's actual Unit object (accessing properties reads directly from Diablo memory)
        /// </summary>
        public DiaUnit Unit
        {
            get { return Object as DiaUnit; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Checks ACD for Null/IsValid/Disposed etc.
        /// </summary>
        public bool IsValid
        {
            get { return _source.CachedValue != null && _source.CachedValue.IsValid && !_source.CachedValue.IsDisposed && _source.CachedValue.ACDGuid != -1; }
        }

        /// <summary>
        /// Safely request of a property from DiaUnit
        /// </summary>
        public T GetUnitProperty<T>(Func<DiaUnit, T> selector, [CallerMemberName] string caller = "")
        {
            try
            {
                return selector != null && Unit != null && Unit.IsValid && Source.IsProperValid() ? selector(Unit) : CacheUtilities.Default<T>();
            }
            catch (Exception ex)
            {
                if (ex.Message.StartsWith("Only part of a ReadProcessMemory"))
                {
                    Logger.LogError("DB Memory Exception in GetUnitProperty Caller={0} ACDGuid={1} InternalName={2} Exception={3} {4}",
                        caller, ACDGuid, Source.Name, ex.Message, ex.InnerException);
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
                return selector != null && Item != null && Item.IsValid && Source.IsProperValid() ? selector(Item) : CacheUtilities.Default<T>();
            }
            catch (Exception ex)
            {
                if (ex.Message.StartsWith("Only part of a ReadProcessMemory"))
                {
                    Logger.LogError("DB Memory Exception in GetACDItemProperty Caller={0} ACDGuid={1} ActorType={2} Name={3} SNO={4} Exception={5} {6}",
                        caller, ACDGuid, ActorType, (Source != null) ? Source.Name : string.Empty, (Source != null) ? Source.ActorSNO.ToString() : string.Empty, ex.Message, ex.InnerException);
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
                return selector != null && Item != null && DiaItem.IsValid && Source.IsProperValid() ? selector(DiaItem) : CacheUtilities.Default<T>();
            }
            catch (Exception ex)
            {
                if (ex.Message.StartsWith("Only part of a ReadProcessMemory"))
                {
                    Logger.LogError("DB Memory Exception in GetDiaItemProperty Caller={0} ACDGuid={1} ActorType={2} Name={3} SNO={4} Exception={5} {6}",
                        caller, ACDGuid, ActorType, (Source != null) ? Source.Name : string.Empty, (Source != null) ? Source.ActorSNO.ToString() : string.Empty, ex.Message, ex.InnerException);
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
                return selector != null && Gizmo != null && Gizmo.IsValid && Source.IsProperValid() ? selector(Gizmo) : CacheUtilities.Default<T>();
            }
            catch (Exception ex)
            {
                if (ex.Message.StartsWith("Only part of a ReadProcessMemory"))
                {
                    Logger.LogError("DB Memory Exception in GetGizmoProperty Caller={0} ACDGuid={1} ActorType={2} Name={3} SNO={4} Exception={5} {6}",
                        caller, ACDGuid, ActorType, (Source != null) ? Source.Name : string.Empty, (Source != null) ? Source.ActorSNO.ToString() : string.Empty, ex.Message, ex.InnerException);
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
                        caller, ACDGuid, ActorType, (Source != null) ? Source.Name : string.Empty, (Source != null) ? Source.ActorSNO.ToString() : string.Empty, ex.Message, ex.InnerException);
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
                        caller, ACDGuid, ActorType, (Source != null) ? Source.Name : string.Empty, (Source != null) ? Source.ActorSNO.ToString() : string.Empty, ex.Message, ex.InnerException);
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
                return selector != null && Unit != null && Unit.IsValid && Source.IsProperValid() ? selector(source) : CacheUtilities.Default<T2>();
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
        /// Replace the source ACD with a new object reference
        /// </summary>
        /// <param name="acd"></param>
        public void UpdateSource(ACD acd)
        {
            Source = acd;
        }

        /// <summary>
        /// Triggers source to update itself using a new ACDGuid
        /// </summary>
        public void UpdateSource(int acdGuid)
        {
            ACDGuid = acdGuid;
            _source.IsValueCreated = false;
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

        public static implicit operator CacheBase(ACD x)
        {
            return new CacheBase(x);
        }

        public static explicit operator ACD(CacheBase x)
        {
            return x.Source;
        }

        #endregion 

    }
}
