﻿using System.ComponentModel;
using System.Runtime.Serialization;

namespace GilesTrinity.Settings.Combat
{
    [DataContract]
    public class WizardSetting : ITrinitySetting<WizardSetting>, IAvoidanceHealth, INotifyPropertyChanged
    {
        #region Fields
        private float _PotionLevel;
        private float _HealthGlobeLevel;
        private int _KiteLimit;
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
        private bool _CriticalMass;
        private bool _OnlyKiteInArchon;
        private bool _WaitArchon;

        private bool _NoArcaneStrike;
        private int _ArchonMobCount;
        private float _ArchonMobDistance;
        private WizardKiteOption _KiteOption;
        private WizardArchonCancelOption _ArchonCancelOption;
        private int _ArchonCancelSeconds;
        #endregion Fields

        #region Events
        /// <summary>
        /// Occurs when property changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion Events

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="WizardSetting" /> class.
        /// </summary>
        public WizardSetting()
        {
            Reset();
        }
        #endregion Constructors

        #region Properties
        [DataMember(IsRequired = false)]
        [DefaultValue(false)]
        public bool NoArcaneStrike
        {
            get
            {
                return _NoArcaneStrike;
            }
            set
            {
                if (_NoArcaneStrike != value)
                {
                    _NoArcaneStrike = value;
                    OnPropertyChanged("NoArcaneStrike");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(2)]
        public int ArchonMobCount
        {
            get
            {
                return _ArchonMobCount;
            }
            set
            {
                if (_ArchonMobCount != value)
                {
                    _ArchonMobCount = value;
                    OnPropertyChanged("ArchonMobCount");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(35f)]
        public float ArchonMobDistance
        {
            get
            {
                return _ArchonMobDistance;
            }
            set
            {
                if (_ArchonMobDistance != value)
                {
                    _ArchonMobDistance = value;
                    OnPropertyChanged("ArchonMobDistance");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(300)]
        public int ArchonCancelSeconds
        {
            get
            {
                return _ArchonCancelSeconds;
            }
            set
            {
                if (_ArchonCancelSeconds != value)
                {
                    _ArchonCancelSeconds = value;
                    OnPropertyChanged("ArchonCancelSeconds");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(WizardArchonCancelOption.RebuffMagicWeaponFamiliar)]
        public WizardArchonCancelOption ArchonCancelOption
        {
            get
            {
                return _ArchonCancelOption;
            }
            set
            {
                if (_ArchonCancelOption != value)
                {
                    _ArchonCancelOption = value;
                    OnPropertyChanged("ArchonCancelOption");
                }
            }
        }
        [DataMember(IsRequired = false)]
        [DefaultValue(WizardKiteOption.Anytime)]
        public WizardKiteOption KiteOption
        {
            get
            {
                return _KiteOption;
            }
            set
            {
                if (_KiteOption != value)
                {
                    _KiteOption = value;
                    OnPropertyChanged("KiteOption");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(0.7f)]
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
        [DefaultValue(false)]
        public bool CriticalMass
        {
            get
            {
                return _CriticalMass;
            }
            set
            {
                if (_CriticalMass != value)
                {
                    _CriticalMass = value;
                    OnPropertyChanged("CriticalMass");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(false)]
        public bool OnlyKiteInArchon
        {
            get
            {
                return _OnlyKiteInArchon;
            }
            set
            {
                if (_OnlyKiteInArchon != value)
                {
                    _OnlyKiteInArchon = value;
                    OnPropertyChanged("OnlyKiteInArchon");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(false)]
        public bool WaitArchon
        {
            get
            {
                return _WaitArchon;
            }
            set
            {
                if (_WaitArchon != value)
                {
                    _WaitArchon = value;
                    OnPropertyChanged("WaitArchon");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(0.8f)]
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
        [DefaultValue(0)]
        public int KiteLimit
        {
            get
            {
                return _KiteLimit;
            }
            set
            {
                if (_KiteLimit != value)
                {
                    _KiteLimit = value;
                    OnPropertyChanged("KiteLimit");
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
        [DefaultValue(0.9f)]
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
        [DefaultValue(0.9f)]
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
        [DefaultValue(0.9f)]
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
        [DefaultValue(1f)]
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
        [DefaultValue(1f)]
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
        [DefaultValue(1f)]
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
        [DefaultValue(1f)]
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
        [DefaultValue(1f)]
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
        [DefaultValue(0.85f)]
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
        [DefaultValue(1f)]
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
        [DefaultValue(1f)]
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
        [DefaultValue(0.8f)]
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
        [DefaultValue(0.9f)]
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
        [DefaultValue(0.1f)]
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


        #endregion Properties

        #region Methods
        public void Reset()
        {
            TrinitySetting.Reset(this);
        }

        public void CopyTo(WizardSetting setting)
        {
            TrinitySetting.CopyTo(this, setting);
        }

        public WizardSetting Clone()
        {
            return TrinitySetting.Clone(this);
        }

        /// <summary>
        /// Called when property changed.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
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
            this._ArchonMobDistance = 35;
            this._ArchonMobCount = 3;
            this._ArchonCancelSeconds = 300;
            this._ArchonCancelOption = WizardArchonCancelOption.RebuffMagicWeaponFamiliar;
        }

        #endregion Methods
    }
}
