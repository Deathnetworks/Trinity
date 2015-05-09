using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Web.Security;
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

        public CacheBase(ACD acd, int acdGuid, bool loadActorProps = true)
        {
            ACDGuid = acdGuid;
            Source = acd;

            if(loadActorProps)
                LoadActorProperties();
        }

        public CacheBase(ACD acd, bool loadActorProps = true)
        {
            ACDGuid = acd.ACDGuid;     
            Source = acd;

            if (loadActorProps)
                LoadActorProperties();
        }

        public CacheBase(int acdGuid, bool loadActorProps = true)
        {
            ACDGuid = acdGuid;

            if (loadActorProps)
                LoadActorProperties();
        }

        private void LoadActorProperties()
        {            
            var aProps = CacheNatives.GetActorProperties(ActorSNO);
            _actorInfo = aProps.CachedActorInfo;
            _monsterInfo = aProps.CachedMonsterInfo;
            _isCorruptDiaUnit = aProps.IsInvalidMonsterInfo;
        }

        #endregion

        #region Fields

        private readonly CacheField<TrinityObjectType> _trinityType = new CacheField<TrinityObjectType>();
        private readonly CacheField<ActorType> _actorType = new CacheField<ActorType>();
        private readonly CacheField<DiaObject> _object = new CacheField<DiaObject>(UpdateSpeed.RealTime);
        private readonly CacheField<ACD> _source = new CacheField<ACD>();
        private readonly CacheField<DiaItem> _diaItem = new CacheField<DiaItem>(UpdateSpeed.RealTime);
        private readonly CacheField<ACDItem> _acdItem = new CacheField<ACDItem>(UpdateSpeed.RealTime);
        private readonly CacheField<DiaGizmo> _diaGizmo = new CacheField<DiaGizmo>(UpdateSpeed.RealTime);
        private readonly CacheField<DiaUnit> _diaUnit = new CacheField<DiaUnit>(UpdateSpeed.RealTime);
        private readonly CacheField<int> _actorSNO = new CacheField<int>();

        private CacheNatives.CachedActorInfo _actorInfo;
        private CacheNatives.CachedMonsterInfo _monsterInfo;
        private bool _isCorruptDiaUnit;

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
        /// Trinity's actor type
        /// </summary>
        public TrinityObjectType TrinityType
        {
            get
            {
                if (_trinityType.IsCacheValid) return _trinityType.CachedValue;
                else return _trinityType.CachedValue = TrinityObject.GetTrinityObjectType(Source, ActorType);
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
                else return _actorType.CachedValue = Source.ActorType;
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
                return _object.CachedValue = CacheManager.GetRActorOfTypeByACDGuid<DiaObject>(ACDGuid);
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
                else return ACDGuid != 0 ? (_source.CachedValue = ZetaDia.Actors.GetACDByGuid(ACDGuid)) : null;
            }
            set { _source.SetValueOverride(value); }
        }

        /// <summary>
        /// DB's actual Gizmo object (accessing properties reads directly from Diablo memory)
        /// </summary>
        public DiaGizmo Gizmo
        {
            get
            {
                if (_diaGizmo.IsCacheValid) return _diaGizmo.CachedValue;
                return ACDGuid != 0 ? (_diaGizmo.CachedValue = Object as DiaGizmo) : null;
            }
        }

        /// <summary>
        /// DB's actual Item object (accessing properties reads directly from Diablo memory)
        /// </summary>
        public ACDItem Item
        {
            get
            {
                if (_acdItem.IsCacheValid) return _acdItem.CachedValue;
                return ACDGuid != 0 ? (_acdItem.CachedValue = Source as ACDItem) : null;
            }
        }

        /// <summary>
        /// DB's actual Gizmo (accessing properties reads directly from Diablo memory)
        /// </summary>
        public DiaItem DiaItem
        {
            get
            {
                if (_diaItem.IsCacheValid) return _diaItem.CachedValue;
                return ACDGuid != 0 ? (_diaItem.CachedValue = Object as DiaItem) : null;
            }
        }

        public DiaUnit Unit
        {
            get
            {
                if (_diaUnit.IsCacheValid || _isCorruptDiaUnit)
                    return _diaUnit.CachedValue;

                return ACDGuid != 0 ? (_diaUnit.CachedValue = Object as DiaUnit) : null;
            }
        }

        /// <summary>
        /// ActorInfo
        /// </summary>
        public CacheNatives.CachedActorInfo ActorInfo
        {
            get { return _actorInfo; }
            set { _actorInfo = value; }
        }

        public CacheNatives.CachedMonsterInfo MonsterInfo
        {
            get { return _monsterInfo; }
            set { _monsterInfo = value; }
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
                return selector != null && Unit != null && !_isCorruptDiaUnit && Unit.IsValid && Source.IsProperValid() ? selector(Unit) : CacheUtilities.Default<T>();
            }
            catch (Exception ex)
            {
                if (ex.Message.StartsWith("Only part of a ReadProcessMemory"))
                {
                    var acd = ZetaDia.Actors.GetACDByGuid(ACDGuid);
                    var actor = ZetaDia.Actors.GetActorsOfType<DiaUnit>().Where(u => u.ACDGuid == ACDGuid);

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

        public TrinityUnit ToTrinityUnit()
        {
            return CopyTo(new TrinityUnit(Source, ACDGuid, false));
        }

        public TrinityGizmo ToTrinityGizmo()
        {
            return CopyTo(new TrinityGizmo(Source, ACDGuid, false));
        }

        public TrinityAvoidance ToTrinityAvoidance()
        {
            return CopyTo(new TrinityAvoidance(Source, ACDGuid, false));
        }

        public TrinityItem ToTrinityItem()
        {
            return CopyTo(new TrinityItem(Source, ACDGuid, false));
        }

        public TrinityPlayer ToTrinityPlayer()
        {
            return CopyTo(new TrinityPlayer(Source, ACDGuid, false));
        }

        public TrinityObject ToTrinityObject()
        {
            return CopyTo(new TrinityObject(Source, ACDGuid, false));
        }

        public T CopyTo<T>(T other) where T : CacheBase
        {
            other.Object = Object;
            other.LastUpdated = LastUpdated;
            other.MonsterInfo = MonsterInfo;
            other.ActorInfo = ActorInfo;
            other.TrinityType = TrinityType;
            other.ActorSNO = ActorSNO;
            other.ActorType = ActorType;
            return other;
        }

        #endregion 

    }
}
