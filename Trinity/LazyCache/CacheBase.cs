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
        #region Fields

        private readonly CacheField<TrinityObjectType> _trinityType = new CacheField<TrinityObjectType>();
        private readonly CacheField<CacheMeta.ActorMeta> _actorMeta = new CacheField<CacheMeta.ActorMeta>();
        private readonly CacheField<ActorType> _actorType = new CacheField<ActorType>();
        private readonly CacheField<DiaObject> _object = new CacheField<DiaObject>();
        private readonly CacheField<ACD> _source = new CacheField<ACD>();
        private readonly CacheField<int> _actorSNO = new CacheField<int>();

        public CacheBase(ACD x, int acdGuid)
        {
            Source = x;
            ACDGuid = acdGuid;
        }

        public CacheBase(ACD x)
        {
            Source = x;
            ACDGuid = x.ACDGuid;
        }

        public CacheBase()
        {

        }

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
            set { _source.LastUpdate = value; }
        }

        /// <summary>
        /// Unique identifier for the actor
        /// </summary>
        public int ActorSNO
        {
            get
            {
                if (_actorSNO.IsCacheValid) return _actorSNO.CachedValue;
                return _actorSNO.CachedValue = Source.ActorSNO;
            }
            set { _actorSNO.SetValueOverride(value); }
        }

        /// <summary>
        /// ActorInfo and MonsterInfo data
        /// </summary>
        public CacheMeta.ActorMeta ActorMeta
        {
            get
            {
                if (_actorMeta.IsCacheValid) return _actorMeta.CachedValue;
                return _actorMeta.CachedValue = CacheMeta.GetActorMeta(this);
            }
            set { _actorMeta.SetValueOverride(value); }
        }

        /// <summary>
        /// Trinity's actor type
        /// </summary>
        public TrinityObjectType TrinityType
        {
            get
            {
                if (_trinityType.IsCacheValid) return _trinityType.CachedValue;
                return _trinityType.CachedValue = TrinityObject.GetTrinityObjectType(this);
            }
            set { _trinityType.SetValueOverride(value); }
        }

        /// <summary>
        /// DB's actor type
        /// </summary>
        public ActorType ActorType
        {
            get
            {
                if (_actorType.IsCacheValid) return _actorType.CachedValue;
                return _actorType.CachedValue = Source.ActorType;
            }
            set { _actorType.SetValueOverride(value); }
        }

        /// <summary>
        /// DB's actual DiaObject (accessing properties reads directly from Diablo memory)
        /// </summary>
        public DiaObject Object
        {
            get
            {
                if (_object.IsCacheValid) return _object.CachedValue;
                return _object.CachedValue = ZetaDia.Actors.GetActorByACDId(ACDGuid);
                
                //return _object.CachedValue = CacheFactory.CreateTypedDiaObject(Source, ActorType);
            }
            set { _object.SetValueOverride(value); }
        }

        /// <summary>
        /// DB's actual ACD object (accessing properties reads directly from Diablo memory)
        /// </summary>
        public ACD Source
        {
            get
            {
                if (_source.IsCacheValid) return _source.CachedValue;
                return ACDGuid != 0 ? (_source.CachedValue = ZetaDia.Actors.GetACDByGuid(ACDGuid)) : null;
            }
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
        /// DB's actual Unit (accessing properties reads directly from Diablo memory)
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
                return selector != null && Item != null && Item.IsValid && Source.IsProperValid() ? selector(Item) : CacheUtilities.Default<T>();
            }
            catch (Exception ex)
            {
                if (ex.Message.StartsWith("Only part of a ReadProcessMemory"))
                {
                    Logger.LogError("DB Memory Exception in GetACDItemProperty Caller={0} ACDGuid={1} ActorType={2} Name={3} SNO={4} Exception={5} {6}",
                        caller, ACDGuid, ActorType, (Source != null) ? Source.Name : string.Empty, ActorSNO, ex.Message, ex.InnerException);
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
                        caller, ACDGuid, ActorType, (Source != null) ? Source.Name : string.Empty, ActorSNO, ex.Message, ex.InnerException);
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
        public void UpdateSource(ACD acd)
        {
            Source = acd;
        }

        /// <summary>
        /// Replace the source ACD with a new object reference
        /// </summary>
        public TrinityObject UpdateSource(TrinityObject obj, ACD acd)
        {
            obj.Source = acd;
            return obj;
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

        public TrinityUnit ToTrinityUnit()
        {
            return CopyTo(new TrinityUnit(Source, ACDGuid));
        }

        public TrinityGizmo ToTrinityGizmo()
        {
            return CopyTo(new TrinityGizmo(Source, ACDGuid));
        }

        public TrinityAvoidance ToTrinityAvoidance()
        {
            return CopyTo(new TrinityAvoidance(Source, ACDGuid));
        }

        public TrinityItem ToTrinityItem()
        {
            return CopyTo(new TrinityItem(Source, ACDGuid));
        }

        public TrinityPlayer ToTrinityPlayer()
        {
            return CopyTo(new TrinityPlayer(Source, ACDGuid));
        }

        public TrinityObject ToTrinityObject()
        {
            return CopyTo(new TrinityObject(Source, ACDGuid));
        }

        public T CopyTo<T>(T other) where T : CacheBase
        {
            other.Object = Object;
            other.ACDGuid = ACDGuid;
            other.LastUpdated = _source.LastUpdate;
            other.ActorMeta = ActorMeta;
            other.TrinityType = TrinityType;
            other.ActorSNO = ActorSNO;
            other.ActorType = ActorType;
            return other;
        }

        #endregion
    }
}
