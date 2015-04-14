﻿using System.ComponentModel;
using System.Reflection;
using System.Runtime.Serialization;

namespace Trinity.Config.Combat
{
    [DataContract(Namespace = "")]
    public class DemonHunterSetting : ITrinitySetting<DemonHunterSetting>, IAvoidanceHealth, INotifyPropertyChanged
    {
        #region Fields
        private float _PotionLevel;
        private float _HealthGlobeLevel;
        private float _HealthGlobeLevelResource;
        private int _KiteLimit;
        private int _RangedAttackRange;
        private int _VaultMovementDelay;
        private int _VaultCombatDelay;
        private float _MinHealthVaultKite;
        private float _MinHealthVaultAvoidance;
        private int _MinDistVaultKite;
        private int _MinDistVaultAvoidance;
        private int _MinDistVaultCombat;
        private int _MinDistVaultHealth;
        private int _MinDistVaultGlobeItem;
        private int _MinDistVaultShrineOther;
        private bool _SpamSmokeScreen;
        private bool _SpamShadowPower;
        private bool _CompanionOffCooldown;
        private bool _RainOfVengeanceOffCD;
        private int _StrafeMinHatred;
        private int _RapidFireMinHatred;
        private bool _VengeanceElitesOnly;
        private float _MinDiciplineOOCVaultPct;
        private float _MinDiciplineCombatVaultPct;
        private KiteMode _KiteMode;
        private DemonHunterVaultMode _VaultMode;


        private float _AvoidArcaneHealth;
        private float _AvoidAzmoBodiesHealth;
        private float _AvoidAzmoFireBallHealth;
        private float _AvoidAzmoPoolsHealth;
        private float _AvoidBeesWaspsHealth;
        private float _AvoidBelialHealth;
        private float _AvoidButcherFloorPanelHealth;
        private float _AvoidDesecratorHealth;
        private float _AvoidDiabloMeteorHealth;
        private float _AvoidDiabloPrisonHealth;
        private float _AvoidDiabloRingOfFireHealth;
        private float _AvoidFrozenPulseHealth;
        private float _AvoidGhomGasHealth;
        private float _AvoidGrotesqueHealth;
        private float _AvoidIceBallsHealth;
        private float _AvoidIceTrailHealth;
        private float _AvoidOrbiterHealth;
        private float _AvoidMageFireHealth;
        private float _AvoidMaghdaProjectilleHealth;
        private float _AvoidMoltenBallHealth;
        private float _AvoidMoltenCoreHealth;
        private float _AvoidMoltenTrailHealth;
        private float _AvoidPlagueCloudHealth;
        private float _AvoidPlagueHandsHealth;
        private float _AvoidPoisonEnchantedHealth;
        private float _AvoidPoisonTreeHealth;
        private float _AvoidShamanFireHealth;
        private float _AvoidThunderstormHealth;
        private float _AvoidWallOfFireHealth;
        private float _AvoidWormholeHealth;
        private float _AvoidZoltBubbleHealth;
        private float _AvoidZoltTwisterHealth; 
        #endregion Fields

        #region Events
        /// <summary>
        /// Occurs when property changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion Events

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="DemonHunterSetting" /> class.
        /// </summary>
        public DemonHunterSetting()
        {
            Reset();
        }
        #endregion Constructors

        #region Properties
        [DataMember(IsRequired = false)]
        [DefaultValue(0.5f)]
        public float MinDiciplineOOCVaultPct
        {
            get
            {
                return _MinDiciplineOOCVaultPct;
            }
            set
            {
                if (_MinDiciplineOOCVaultPct != value)
                {
                    _MinDiciplineOOCVaultPct = value;
                    OnPropertyChanged("PotionLevel");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(0.5f)]
        public float MinDiciplineCombatVaultPct
        {
            get
            {
                return _MinDiciplineCombatVaultPct;
            }
            set
            {
                if (_MinDiciplineCombatVaultPct != value)
                {
                    _MinDiciplineCombatVaultPct = value;
                    OnPropertyChanged("PotionLevel");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(0.35f)]
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
        [DefaultValue(0.5f)]
        public float HealthGlobeLevelResource
        {
            get
            {
                return _HealthGlobeLevelResource;
            }
            set
            {
                if (_HealthGlobeLevelResource != value)
                {
                    _HealthGlobeLevelResource = value;
                    OnPropertyChanged("HealthGlobeLevelResource");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(15)]
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
        [DefaultValue(35)]
        public int RangedAttackRange
        {
            get
            {
                return _RangedAttackRange;
            }
            set
            {
                if (_RangedAttackRange != value)
                {
                    _RangedAttackRange = value;
                    OnPropertyChanged("RangedAttackRange");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(false)]
        public bool SpamSmokeScreen
        {
            get
            {
                return _SpamSmokeScreen;
            }
            set
            {
                if (_SpamSmokeScreen != value)
                {
                    _SpamSmokeScreen = value;
                    OnPropertyChanged("SpamSmokeScreen");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(false)]
        public bool RainOfVengeanceOffCD
        {
            get
            {
                return _RainOfVengeanceOffCD;
            }
            set
            {
                if (_RainOfVengeanceOffCD != value)
                {
                    _RainOfVengeanceOffCD = value;
                    OnPropertyChanged("RainOfVengeanceOffCD");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(false)]
        public bool SpamShadowPower
        {
            get
            {
                return _SpamShadowPower;
            }
            set
            {
                if (_SpamShadowPower != value)
                {
                    _SpamShadowPower = value;
                    OnPropertyChanged("SpamShadowPower");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(false)]
        public bool CompanionOffCooldown
        {
            get
            {
                return _CompanionOffCooldown;
            }
            set
            {
                if (_CompanionOffCooldown != value)
                {
                    _CompanionOffCooldown = value;
                    OnPropertyChanged("CompanionOffCooldown");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(48)]
        public int StrafeMinHatred
        {
            get
            {
                return _StrafeMinHatred;
            }
            set
            {
                if (_StrafeMinHatred != value)
                {
                    _StrafeMinHatred = value;
                    OnPropertyChanged("StrafeMinHatred");
                }
            }
        }
        
        [DataMember(IsRequired = false)]
        [DefaultValue(true)]
        public bool VengeanceElitesOnly
        {
            get
            {
                return _VengeanceElitesOnly;
            }
            set
            {
                if (_VengeanceElitesOnly != value)
                {
                    _VengeanceElitesOnly = value;
                    OnPropertyChanged("VenganceElitesOnly");
                }
            }
        }
        [DataMember(IsRequired = false)]
        [DefaultValue(60)]
        public int RapidFireMinHatred
        {
            get
            {
                return _RapidFireMinHatred;
            }
            set
            {
                if (_RapidFireMinHatred != value)
                {
                    _RapidFireMinHatred = value;
                    OnPropertyChanged("RapidFireMinHatred");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(KiteMode.Bosses)]
        public KiteMode KiteMode
        {
            get
            {
                return _KiteMode;
            }
            set
            {
                if (_KiteMode != value)
                {
                    _KiteMode = value;
                    OnPropertyChanged("KiteMode");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(DemonHunterVaultMode.Always)]
        public DemonHunterVaultMode VaultMode
        {
            get
            {
                return _VaultMode;
            }
            set
            {
                if (_VaultMode != value)
                {
                    _VaultMode = value;
                    OnPropertyChanged("VaultMode");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(1500)]
        public int VaultMovementDelay
        {
            get
            {
                return _VaultMovementDelay;
            }
            set
            {
                if (_VaultMovementDelay != value)
                {
                    _VaultMovementDelay = value;
                    OnPropertyChanged("VaultMovementDelay");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(400)]
        public int VaultCombatDelay
        {
            get
            {
                return _VaultCombatDelay;
            }
            set
            {
                if (_VaultCombatDelay != value)
                {
                    _VaultCombatDelay = value;
                    OnPropertyChanged("VaultCombatDelay");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(0.95f)]
        public float MinHealthVaultKite
        {
            get
            {
                return _MinHealthVaultKite;
            }
            set
            {
                if (_MinHealthVaultKite != value)
                {
                    _MinHealthVaultKite = value;
                    OnPropertyChanged("MinHealthVaultKite");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(1f)]
        public float MinHealthVaultAvoidance
        {
            get
            {
                return _MinHealthVaultAvoidance;
            }
            set
            {
                if (_MinHealthVaultAvoidance != value)
                {
                    _MinHealthVaultAvoidance = value;
                    OnPropertyChanged("MinHealthVaultAvoidance");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(20)]
        public int MinDistVaultKite
        {
            get
            {
                return _MinDistVaultKite;
            }
            set
            {
                if (_MinDistVaultKite != value)
                {
                    _MinDistVaultKite = value;
                    OnPropertyChanged("MinDistVaultKite");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(8)]
        public int MinDistVaultAvoidance
        {
            get
            {
                return _MinDistVaultAvoidance;
            }
            set
            {
                if (_MinDistVaultAvoidance != value)
                {
                    _MinDistVaultAvoidance = value;
                    OnPropertyChanged("MinDistVaultAvoidance");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(35)]
        public int MinDistVaultCombat
        {
            get
            {
                return _MinDistVaultCombat;
            }
            set
            {
                if (_MinDistVaultCombat != value)
                {
                    _MinDistVaultCombat = value;
                    OnPropertyChanged("MinDistVaultCombat");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(15)]
        public int MinDistVaultHealth
        {
            get
            {
                return _MinDistVaultHealth;
            }
            set
            {
                if (_MinDistVaultHealth != value)
                {
                    _MinDistVaultHealth = value;
                    OnPropertyChanged("MinDistVaultHealth");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(28)]
        public int MinDistVaultGlobeItem
        {
            get
            {
                return _MinDistVaultGlobeItem;
            }
            set
            {
                if (_MinDistVaultGlobeItem != value)
                {
                    _MinDistVaultGlobeItem = value;
                    OnPropertyChanged("MinDistVaultGlobeItem");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(30)]
        public int MinDistVaultShrineOther
        {
            get
            {
                return _MinDistVaultShrineOther;
            }
            set
            {
                if (_MinDistVaultShrineOther != value)
                {
                    _MinDistVaultShrineOther = value;
                    OnPropertyChanged("MinDistVaultShrineOther");
                }
            }
        }

        /* Avoidances */

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
        public float AvoidPoisonEnchantedHealth
        {
            get
            {
                return _AvoidPoisonEnchantedHealth;
            }
            set
            {
                if (_AvoidPoisonEnchantedHealth != value)
                {
                    _AvoidPoisonEnchantedHealth = value;
                    OnPropertyChanged("AvoidPoisonEnchantedHealth");
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
        [DefaultValue(1f)]
        public float AvoidGrotesqueHealth
        {
            get
            {
                return _AvoidGrotesqueHealth;
            }
            set
            {
                if (_AvoidGrotesqueHealth != value)
                {
                    _AvoidGrotesqueHealth = value;
                    OnPropertyChanged("AvoidGrotesqueHealth");
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
        [DefaultValue(0.25f)]
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
        [DefaultValue(0.25f)]
        public float AvoidFrozenPulseHealth
        {
            get
            {
                return _AvoidFrozenPulseHealth;
            }
            set
            {
                if (_AvoidFrozenPulseHealth != value)
                {
                    _AvoidFrozenPulseHealth = value;
                    OnPropertyChanged("AvoidFrozenPulseHealth");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(0.60f)]
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
        [DefaultValue(1f)]
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
        [DefaultValue(1f)]
        public float AvoidOrbiterHealth
        {
            get
            {
                return _AvoidOrbiterHealth;
            }
            set
            {
                if (_AvoidOrbiterHealth != value)
                {
                    _AvoidOrbiterHealth = value;
                    OnPropertyChanged("AvoidOrbiterHealth");
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
        [DefaultValue(0.2f)]
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
        [DefaultValue(0.5f)]
        public float AvoidWormholeHealth
        {
            get
            {
                return _AvoidWormholeHealth;
            }
            set
            {
                if (_AvoidWormholeHealth != value)
                {
                    _AvoidWormholeHealth = value;
                    OnPropertyChanged("AvoidWormholeHealth");
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
        [DefaultValue(0.50f)]
        public float AvoidThunderstormHealth
        {
            get
            {
                return _AvoidThunderstormHealth;
            }
            set
            {
                if (_AvoidThunderstormHealth != value)
                {
                    _AvoidThunderstormHealth = value;
                    OnPropertyChanged("AvoidThunderstormHealth");
                }
            }
        }
        #endregion Properties

        #region Methods
        public void Reset()
        {
            TrinitySetting.Reset(this);
        }

        public void CopyTo(DemonHunterSetting setting)
        {
            TrinitySetting.CopyTo(this, setting);
        }

        public DemonHunterSetting Clone()
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
        [OnDeserializing]
        internal void OnDeserializingMethod(StreamingContext context)
        {
            AvoidGrotesqueHealth = 1;
            AvoidOrbiterHealth = 1;
            AvoidWormholeHealth = 0.50f;
            StrafeMinHatred = 48;
            VaultMode = DemonHunterVaultMode.Always;
        }

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            // Fix for type default
            if (RangedAttackRange < 10f)
                RangedAttackRange = 65;
        }

        #endregion Methods
    }
}
