using System.ComponentModel;
using System.Runtime.Serialization;

namespace Trinity.Settings.Combat
{
    [DataContract]
    public class MonkSetting : ITrinitySetting<MonkSetting>, IAvoidanceHealth, INotifyPropertyChanged
    {
        #region Fields
        private TempestRushOption _TROption;
        private float _PotionLevel;
        private float _HealthGlobeLevel;
        private float _AvoidArcaneHealth;
        private float _AvoidDesecratorHealth;
        private float _AvoidMoltenCoreHealth;
        private float _AvoidMoltenTrailHealth;
        private float _AvoidPoisonTreeHealth;
        private float _AvoidPlagueCloudHealth;
        private float _AvoidIceBallsHealth;
        private float _AvoidPlagueHandsHealth;
        private float _AvoidBeesWaspsHealth;
        private float _AvoidAzmoPoolsHealth;
        private float _AvoidAzmoBodiesHealth;
        private float _AvoidShamanFireHealth;
        private float _AvoidGhomGasHealth;
        private float _AvoidAzmoFireBallHealth;
        private float _AvoidBelialHealth;
        private float _AvoidButcherFloorPanelHealth;
        private float _AvoidDiabloMeteorHealth;
        private float _AvoidDiabloPrisonHealth;
        private float _AvoidDiabloRingOfFireHealth;
        private float _AvoidIceTrailHealth;
        private float _AvoidMageFireHealth;
        private float _AvoidMaghdaProjectilleHealth;
        private float _AvoidMoltenBallHealth;
        private float _AvoidWallOfFireHealth;
        private float _AvoidZoltBubbleHealth;
        private float _AvoidZoltTwisterHealth;
        private float _AvoidSuccubusStarHealth;
        private int _TR_MinSpirit;
        private int _TR_MinDist;
        private bool _HasInnaSet;
        private bool _SWBeforeWoL;
        private bool _DisableMantraSpam;
        private bool _TargetBasedZigZag;
        private int _MinCycloneTrashCount;
        private int _MinWoLTrashCount;
        #endregion Fields

        #region Events
        /// <summary>
        /// Occurs when property changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion Events

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="MonkSetting" /> class.
        /// </summary>
        public MonkSetting()
        {
            Reset();
        }
        #endregion Constructors

        #region Properties
        [DataMember(IsRequired = false)]
        [DefaultValue(0.46f)]
        public float PotionLevel
        {
            get
            {
                return _PotionLevel;
            }
            set
            {
                if (_PotionLevel != value)
                {
                    _PotionLevel = value;
                    OnPropertyChanged("PotionLevel");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(0.6f)]
        public float HealthGlobeLevel
        {
            get
            {
                return _HealthGlobeLevel;
            }
            set
            {
                if (_HealthGlobeLevel != value)
                {
                    _HealthGlobeLevel = value;
                    OnPropertyChanged("HealthGlobeLevel");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(1f)]
        public float AvoidArcaneHealth
        {
            get
            {
                return _AvoidArcaneHealth;
            }
            set
            {
                if (_AvoidArcaneHealth != value)
                {
                    _AvoidArcaneHealth = value;
                    OnPropertyChanged("AvoidArcaneHealth");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(1f)]
        public float AvoidDesecratorHealth
        {
            get
            {
                return _AvoidDesecratorHealth;
            }
            set
            {
                if (_AvoidDesecratorHealth != value)
                {
                    _AvoidDesecratorHealth = value;
                    OnPropertyChanged("AvoidDesecratorHealth");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(1f)]
        public float AvoidMoltenCoreHealth
        {
            get
            {
                return _AvoidMoltenCoreHealth;
            }
            set
            {
                if (_AvoidMoltenCoreHealth != value)
                {
                    _AvoidMoltenCoreHealth = value;
                    OnPropertyChanged("AvoidMoltenCoreHealth");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(0.65f)]
        public float AvoidMoltenTrailHealth
        {
            get
            {
                return _AvoidMoltenTrailHealth;
            }
            set
            {
                if (_AvoidMoltenTrailHealth != value)
                {
                    _AvoidMoltenTrailHealth = value;
                    OnPropertyChanged("AvoidMoltenTrailHealth");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(0.65f)]
        public float AvoidPoisonTreeHealth
        {
            get
            {
                return _AvoidPoisonTreeHealth;
            }
            set
            {
                if (_AvoidPoisonTreeHealth != value)
                {
                    _AvoidPoisonTreeHealth = value;
                    OnPropertyChanged("AvoidPoisonTreeHealth");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(0.35f)]
        public float AvoidPlagueCloudHealth
        {
            get
            {
                return _AvoidPlagueCloudHealth;
            }
            set
            {
                if (_AvoidPlagueCloudHealth != value)
                {
                    _AvoidPlagueCloudHealth = value;
                    OnPropertyChanged("AvoidPlagueCloudHealth");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(0.85f)]
        public float AvoidIceBallsHealth
        {
            get
            {
                return _AvoidIceBallsHealth;
            }
            set
            {
                if (_AvoidIceBallsHealth != value)
                {
                    _AvoidIceBallsHealth = value;
                    OnPropertyChanged("AvoidIceBallsHealth");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(0.85f)]
        public float AvoidPlagueHandsHealth
        {
            get
            {
                return _AvoidPlagueHandsHealth;
            }
            set
            {
                if (_AvoidPlagueHandsHealth != value)
                {
                    _AvoidPlagueHandsHealth = value;
                    OnPropertyChanged("AvoidPlagueHandsHealth");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(0.75f)]
        public float AvoidBeesWaspsHealth
        {
            get
            {
                return _AvoidBeesWaspsHealth;
            }
            set
            {
                if (_AvoidBeesWaspsHealth != value)
                {
                    _AvoidBeesWaspsHealth = value;
                    OnPropertyChanged("AvoidBeesWaspsHealth");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(0.85f)]
        public float AvoidAzmoPoolsHealth
        {
            get
            {
                return _AvoidAzmoPoolsHealth;
            }
            set
            {
                if (_AvoidAzmoPoolsHealth != value)
                {
                    _AvoidAzmoPoolsHealth = value;
                    OnPropertyChanged("AvoidAzmoPoolsHealth");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(0.75f)]
        public float AvoidAzmoBodiesHealth
        {
            get
            {
                return _AvoidAzmoBodiesHealth;
            }
            set
            {
                if (_AvoidAzmoBodiesHealth != value)
                {
                    _AvoidAzmoBodiesHealth = value;
                    OnPropertyChanged("AvoidAzmoBodiesHealth");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(0f)]
        public float AvoidShamanFireHealth
        {
            get
            {
                return _AvoidShamanFireHealth;
            }
            set
            {
                if (_AvoidShamanFireHealth != value)
                {
                    _AvoidShamanFireHealth = value;
                    OnPropertyChanged("AvoidShamanFireHealth");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(0.5f)]
        public float AvoidGhomGasHealth
        {
            get
            {
                return _AvoidGhomGasHealth;
            }
            set
            {
                if (_AvoidGhomGasHealth != value)
                {
                    _AvoidGhomGasHealth = value;
                    OnPropertyChanged("AvoidGhomGasHealth");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(0.75f)]
        public float AvoidAzmoFireBallHealth
        {
            get
            {
                return _AvoidAzmoFireBallHealth;
            }
            set
            {
                if (_AvoidAzmoFireBallHealth != value)
                {
                    _AvoidAzmoFireBallHealth = value;
                    OnPropertyChanged("AvoidAzmoFireBallHealth");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(1f)]
        public float AvoidBelialHealth
        {
            get
            {
                return _AvoidBelialHealth;
            }
            set
            {
                if (_AvoidBelialHealth != value)
                {
                    _AvoidBelialHealth = value;
                    OnPropertyChanged("AvoidBelialHealth");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(1f)]
        public float AvoidButcherFloorPanelHealth
        {
            get
            {
                return _AvoidButcherFloorPanelHealth;
            }
            set
            {
                if (_AvoidButcherFloorPanelHealth != value)
                {
                    _AvoidButcherFloorPanelHealth = value;
                    OnPropertyChanged("AvoidButcherFloorPanelHealth");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(0.5f)]
        public float AvoidDiabloMeteorHealth
        {
            get
            {
                return _AvoidDiabloMeteorHealth;
            }
            set
            {
                if (_AvoidDiabloMeteorHealth != value)
                {
                    _AvoidDiabloMeteorHealth = value;
                    OnPropertyChanged("AvoidDiabloMeteorHealth");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(1f)]
        public float AvoidDiabloPrisonHealth
        {
            get
            {
                return _AvoidDiabloPrisonHealth;
            }
            set
            {
                if (_AvoidDiabloPrisonHealth != value)
                {
                    _AvoidDiabloPrisonHealth = value;
                    OnPropertyChanged("AvoidDiabloPrisonHealth");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(0.5f)]
        public float AvoidDiabloRingOfFireHealth
        {
            get
            {
                return _AvoidDiabloRingOfFireHealth;
            }
            set
            {
                if (_AvoidDiabloRingOfFireHealth != value)
                {
                    _AvoidDiabloRingOfFireHealth = value;
                    OnPropertyChanged("AvoidDiabloRingOfFireHealth");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(1f)]
        public float AvoidIceTrailHealth
        {
            get
            {
                return _AvoidIceTrailHealth;
            }
            set
            {
                if (_AvoidIceTrailHealth != value)
                {
                    _AvoidIceTrailHealth = value;
                    OnPropertyChanged("AvoidIceTrailHealth");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(0.3f)]
        public float AvoidMageFireHealth
        {
            get
            {
                return _AvoidMageFireHealth;
            }
            set
            {
                if (_AvoidMageFireHealth != value)
                {
                    _AvoidMageFireHealth = value;
                    OnPropertyChanged("AvoidMageFireHealth");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(0.7f)]
        public float AvoidMaghdaProjectilleHealth
        {
            get
            {
                return _AvoidMaghdaProjectilleHealth;
            }
            set
            {
                if (_AvoidMaghdaProjectilleHealth != value)
                {
                    _AvoidMaghdaProjectilleHealth = value;
                    OnPropertyChanged("AvoidMaghdaProjectilleHealth");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(0f)]
        public float AvoidMoltenBallHealth
        {
            get
            {
                return _AvoidMoltenBallHealth;
            }
            set
            {
                if (_AvoidMoltenBallHealth != value)
                {
                    _AvoidMoltenBallHealth = value;
                    OnPropertyChanged("AvoidMoltenBallHealth");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(0.5f)]
        public float AvoidWallOfFireHealth
        {
            get
            {
                return _AvoidWallOfFireHealth;
            }
            set
            {
                if (_AvoidWallOfFireHealth != value)
                {
                    _AvoidWallOfFireHealth = value;
                    OnPropertyChanged("AvoidWallOfFireHealth");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(1f)]
        public float AvoidZoltBubbleHealth
        {
            get
            {
                return _AvoidZoltBubbleHealth;
            }
            set
            {
                if (_AvoidZoltBubbleHealth != value)
                {
                    _AvoidZoltBubbleHealth = value;
                    OnPropertyChanged("AvoidZoltBubbleHealth");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(0.7f)]
        public float AvoidZoltTwisterHealth
        {
            get
            {
                return _AvoidZoltTwisterHealth;
            }
            set
            {
                if (_AvoidZoltTwisterHealth != value)
                {
                    _AvoidZoltTwisterHealth = value;
                    OnPropertyChanged("AvoidZoltTwisterHealth");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(0f)]
        public float AvoidSuccubusStarHealth
        {
            get
            {
                return _AvoidSuccubusStarHealth;
            }
            set
            {
                if (_AvoidSuccubusStarHealth != value)
                {
                    _AvoidSuccubusStarHealth = value;
                    OnPropertyChanged("AvoidSuccubusStarHealth");
                }
            }
        }
        [DataMember(IsRequired = false)]
        [DefaultValue(60)]
        public float TR_MinSpirit
        {
            get
            {
                return _TR_MinSpirit;
            }
            set
            {
                if (_TR_MinSpirit != value)
                {
                    _TR_MinSpirit = (int)value;
                    OnPropertyChanged("TR_MinSpirit");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(10f)]
        public float TR_MinDist
        {
            get
            {
                return _TR_MinDist;
            }
            set
            {
                if (_TR_MinDist != value)
                {
                    _TR_MinDist = (int)value;
                    OnPropertyChanged("TR_MinDist");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(TempestRushOption.Always)]
        public TempestRushOption TROption
        {
            get
            {
                return _TROption;
            }
            set
            {
                if (_TROption != value)
                {
                    _TROption = value;
                    OnPropertyChanged("TROption");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(false)]
        public bool HasInnaSet
        {
            get
            {
                return _HasInnaSet;
            }
            set
            {
                if (_HasInnaSet != value)
                {
                    _HasInnaSet = value;
                    OnPropertyChanged("HasInnaSet");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(false)]
        public bool SWBeforeWoL
        {
            get
            {
                return _SWBeforeWoL;
            }
            set
            {
                if (_SWBeforeWoL != value)
                {
                    _SWBeforeWoL = value;
                    OnPropertyChanged("SWBeforeWoL");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(false)]
        public bool DisableMantraSpam
        {
            get
            {
                return _DisableMantraSpam;
            }
            set
            {
                if (_DisableMantraSpam != value)
                {
                    _DisableMantraSpam = value;
                    OnPropertyChanged("DisableMantraSpam");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(true)]
        public bool TargetBasedZigZag
        {
            get
            {
                return _TargetBasedZigZag;
            }
            set
            {
                if (_TargetBasedZigZag != value)
                {
                    _TargetBasedZigZag = value;
                    OnPropertyChanged("TargetBasedZigZag");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(2)]
        public int MinCycloneTrashCount
        {
            get
            {
                return _MinCycloneTrashCount;
            }
            set
            {
                if (_MinCycloneTrashCount != value)
                {
                    _MinCycloneTrashCount = (int)value;
                    OnPropertyChanged("MinCycloneTrashCount");
                }
            }
        }
        [DataMember(IsRequired = false)]
        [DefaultValue(2)]
        public int MinWoLTrashCount
        {
            get
            {
                return _MinWoLTrashCount;
            }
            set
            {
                if (_MinWoLTrashCount != value)
                {
                    _MinWoLTrashCount = (int)value;
                    OnPropertyChanged("MinWoLTrashCount");
                }
            }
        }        
        #endregion Properties

        #region Methods
        public void Reset()
        {
            TrinitySetting.Reset(this);
        }

        public void CopyTo(MonkSetting setting)
        {
            TrinitySetting.CopyTo(this, setting);
        }

        public MonkSetting Clone()
        {
            return TrinitySetting.Clone(this);
        }

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        /// <summary>
        /// This will set default values for new settings if they were not present in the serialized XML (otherwise they will be the type defaults)
        /// </summary>
        /// <param name="context"></param>
        [OnDeserializing()]
        internal void OnDeserializingMethod(StreamingContext context)
        {
            this._TR_MinSpirit = 60;
            this._TR_MinDist = 10;
            this._TargetBasedZigZag = true;
            this._TROption = TempestRushOption.Always;
            this._AvoidSuccubusStarHealth = 0.7f;
            this._MinCycloneTrashCount = 2;
            this._MinWoLTrashCount = 2;
        }
        #endregion Methods
    }
}
