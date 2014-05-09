using System.ComponentModel;
using System.Runtime.Serialization;
using Trinity.Config.Combat;

namespace Trinity.Config.Loot
{
    [DataContract(Namespace = "")]
    public class PickupSetting : ITrinitySetting<PickupSetting>, INotifyPropertyChanged
    {
        #region Fields
        private bool _PickupUpgrades;
        private bool _PickupGrayItems;
        private bool _PickupWhiteItems;
        private int _WeaponBlueLevel;
        private int _WeaponYellowLevel;
        private int _ArmorBlueLevel;
        private int _ArmorYellowLevel;
        private int _JewelryBlueLevel;
        private int _JewelryYellowLevel;
        private bool _PickupblueFollowerItems;
        private bool _PickupYellowFollowerItems;
        private int _PotionCount;
        private TrinityGemType _GemType;
        private int _GemLevel;
        private int _LegendaryLevel;
        private int _MinimumGoldStack;
        private bool _PickupGold;
        private bool _CraftTomes;
        private bool _Plans;
        private bool _LegendaryPlans;
        private bool _Designs;
        private TrinityItemQuality _TItemQuality;
        private bool _CraftMaterials;
        private bool _InfernalKeys;
        private bool _PickupLowLevel;
        private bool _LootRunKey;
        private bool _BloodShards;
        private bool _IgnoreTwoHandedWeapons;

        private bool _IgnoreLegendaryInAoE;
        private bool _IgnoreNonLegendaryInAoE;
        private bool _IgnoreLegendaryNearElites;
        private bool _IgnoreRareNearElites;
        private bool _IgnoreGoldInAoE;
        private bool _IgnoreGoldNearElites;
        #endregion Fields

        #region Events
        /// <summary>
        /// Occurs when property changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion Events

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="PickupSetting" /> class.
        /// </summary>
        public PickupSetting()
        {
            Reset();
        }
        #endregion Constructors

        #region Properties
        [DataMember(IsRequired = false)]
        [DefaultValue(true)]
        public bool PickupUpgrades
        {
            get
            {
                return _PickupUpgrades;
            }
            set
            {
                if (_PickupUpgrades != value)
                {
                    _PickupUpgrades = value;
                    OnPropertyChanged("PickupUpgrades");
                }
            }
        }
        [DataMember(IsRequired = false)]
        [DefaultValue(false)]
        public bool PickupGrayItems
        {
            get
            {
                return _PickupGrayItems;
            }
            set
            {
                if (_PickupGrayItems != value)
                {
                    _PickupGrayItems = value;
                    OnPropertyChanged("PickupGrayItems");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(false)]
        public bool PickupWhiteItems
        {
            get
            {
                return _PickupWhiteItems;
            }
            set
            {
                if (_PickupWhiteItems != value)
                {
                    _PickupWhiteItems = value;
                    OnPropertyChanged("PickupWhiteItems");
                }
            }
        }
        
        [DataMember(IsRequired = false)]
        [DefaultValue(0)]
        public int WeaponBlueLevel
        {
            get
            {
                return _WeaponBlueLevel;
            }
            set
            {
                if (_WeaponBlueLevel != value)
                {
                    _WeaponBlueLevel = value;
                    OnPropertyChanged("WeaponBlueLevel");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(70)]
        public int WeaponYellowLevel
        {
            get
            {
                return _WeaponYellowLevel;
            }
            set
            {
                if (_WeaponYellowLevel != value)
                {
                    _WeaponYellowLevel = value;
                    OnPropertyChanged("WeaponYellowLevel");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(0)]
        public int ArmorBlueLevel
        {
            get
            {
                return _ArmorBlueLevel;
            }
            set
            {
                if (_ArmorBlueLevel != value)
                {
                    _ArmorBlueLevel = value;
                    OnPropertyChanged("ArmorBlueLevel");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(70)]
        public int ArmorYellowLevel
        {
            get
            {
                return _ArmorYellowLevel;
            }
            set
            {
                if (_ArmorYellowLevel != value)
                {
                    _ArmorYellowLevel = value;
                    OnPropertyChanged("ArmorYellowLevel");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(0)]
        public int JewelryBlueLevel
        {
            get
            {
                return _JewelryBlueLevel;
            }
            set
            {
                if (_JewelryBlueLevel != value)
                {
                    _JewelryBlueLevel = value;
                    OnPropertyChanged("JewelryBlueLevel");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(70)]
        public int JewelryYellowLevel
        {
            get
            {
                return _JewelryYellowLevel;
            }
            set
            {
                if (_JewelryYellowLevel != value)
                {
                    _JewelryYellowLevel = value;
                    OnPropertyChanged("JewelryYellowLevel");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(98)]
        public int PotionCount
        {
            get
            {
                return _PotionCount;
            }
            set
            {
                if (_PotionCount != value)
                {
                    _PotionCount = value;
                    OnPropertyChanged("PotionCount");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(TrinityGemType.Ruby | TrinityGemType.Amethyst | TrinityGemType.Emerald | TrinityGemType.Topaz | TrinityGemType.Diamond)]
        public TrinityGemType GemType
        {
            get
            {
                return _GemType;
            }
            set
            {
                if (_GemType != value)
                {
                    _GemType = value;
                    OnPropertyChanged("GemType");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(60)]
        public int GemLevel
        {
            get
            {
                return _GemLevel;
            }
            set
            {
                if (_GemLevel != value)
                {
                    _GemLevel = value;
                    OnPropertyChanged("GemLevel");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(1)]
        public int LegendaryLevel
        {
            get
            {
                return _LegendaryLevel;
            }
            set
            {
                if (_LegendaryLevel != value)
                {
                    _LegendaryLevel = value;
                    OnPropertyChanged("LegendaryLevel");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(150)]
        public int MinimumGoldStack
        {
            get
            {
                return _MinimumGoldStack;
            }
            set
            {
                if (_MinimumGoldStack != value)
                {
                    _MinimumGoldStack = value;
                    OnPropertyChanged("MinimumGoldStack");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(true)]
        public bool PickupGold
        {
            get
            {
                return _PickupGold;
            }
            set
            {
                if (_PickupGold != value)
                {
                    _PickupGold = value;
                    OnPropertyChanged("PickupGold");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(true)]
        public bool CraftTomes
        {
            get
            {
                return _CraftTomes;
            }
            set
            {
                if (_CraftTomes != value)
                {
                    _CraftTomes = value;
                    OnPropertyChanged("CraftTomes");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(true)]
        public bool Plans
        {
            get
            {
                return _Plans;
            }
            set
            {
                if (_Plans != value)
                {
                    _Plans = value;
                    OnPropertyChanged("Plans");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(true)]
        public bool LegendaryPlans
        {
            get
            {
                return _LegendaryPlans;
            }
            set
            {
                if (_LegendaryPlans != value)
                {
                    _LegendaryPlans = value;
                    OnPropertyChanged("LegendaryPlans");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(true)]
        public bool Designs
        {
            get
            {
                return _Designs;
            }
            set
            {
                if (_Designs != value)
                {
                    _Designs = value;
                    OnPropertyChanged("Designs");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(false)]
        public bool PickupBlueFollowerItems
        {
            get
            {
                return _PickupblueFollowerItems;
            }
            set
            {
                if (_PickupblueFollowerItems != value)
                {
                    _PickupblueFollowerItems = value;
                    OnPropertyChanged("FollowerBluePickup");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(false)]
        public bool PickupYellowFollowerItems
        {
            get
            {
                return _PickupYellowFollowerItems;
            }
            set
            {
                if (_PickupYellowFollowerItems != value)
                {
                    _PickupYellowFollowerItems = value;
                    OnPropertyChanged("FollowerYellowPickup");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(TrinityItemQuality.Rare)]
        public TrinityItemQuality MiscItemQuality
        {
            get
            {
                return _TItemQuality;
            }
            set
            {
                if (_TItemQuality != value)
                {
                    _TItemQuality = value;
                    OnPropertyChanged("MiscItemLevel");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(true)]
        public bool CraftMaterials
        {
            get
            {
                return _CraftMaterials;
            }
            set
            {
                if (_CraftMaterials != value)
                {
                    _CraftMaterials = value;
                    OnPropertyChanged("CraftMaterials");
                }
            }
        }
        [DataMember(IsRequired = false)]
        [DefaultValue(true)]
        public bool InfernalKeys
        {
            get
            {
                return _InfernalKeys;
            }
            set
            {
                if (_InfernalKeys != value)
                {
                    _InfernalKeys = value;
                    OnPropertyChanged("InfernalKeys");
                }
            }
        }
        [DataMember(IsRequired = false)]
        [DefaultValue(true)]
        public bool PickupLowLevel
        {
            get
            {
                return _PickupLowLevel;
            }
            set
            {
                if (_PickupLowLevel != value)
                {
                    _PickupLowLevel = value;
                    OnPropertyChanged("PickupLowLevel");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(true)]
        public bool LootRunKey
        {
            get
            {
                return _LootRunKey;
            }
            set
            {
                if (_LootRunKey != value)
                {
                    _LootRunKey = value;
                    OnPropertyChanged("LootRunKey");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(true)]
        public bool BloodShards
        {
            get
            {
                return _BloodShards;
            }
            set
            {
                if (_BloodShards != value)
                {
                    _BloodShards = value;
                    OnPropertyChanged("BloodShards");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(false)]
        public bool IgnoreTwoHandedWeapons
        {
            get
            {
                return _IgnoreTwoHandedWeapons;
            }
            set
            {
                if (_IgnoreTwoHandedWeapons != value)
                {
                    _IgnoreTwoHandedWeapons = value;
                    OnPropertyChanged("TwoHandedWeapons");
                }
            }
        }
        [DataMember(IsRequired = false)]
        [DefaultValue(false)]
        public bool IgnoreLegendaryInAoE
        {
            get
            {
                return _IgnoreLegendaryInAoE;
            }
            set
            {
                if (_IgnoreLegendaryInAoE != value)
                {
                    _IgnoreLegendaryInAoE = value;
                    OnPropertyChanged("IgnoreLegendaryInAoE");
                }
            }
        }
        [DataMember(IsRequired = false)]
        [DefaultValue(false)]
        public bool IgnoreNonLegendaryInAoE
        {
            get
            {
                return _IgnoreNonLegendaryInAoE;
            }
            set
            {
                if (_IgnoreNonLegendaryInAoE != value)
                {
                    _IgnoreNonLegendaryInAoE = value;
                    OnPropertyChanged("IgnoreNonLegendaryInAoE");
                }
            }
        }
        [DataMember(IsRequired = false)]
        [DefaultValue(false)]
        public bool IgnoreLegendaryNearElites
        {
            get
            {
                return _IgnoreLegendaryNearElites;
            }
            set
            {
                if (_IgnoreLegendaryNearElites != value)
                {
                    _IgnoreLegendaryNearElites = value;
                    OnPropertyChanged("IgnoreLegendaryNearElites");
                }
            }
        }
        [DataMember(IsRequired = false)]
        [DefaultValue(false)]
        public bool IgnoreNonLegendaryNearElites
        {
            get
            {
                return _IgnoreRareNearElites;
            }
            set
            {
                if (_IgnoreRareNearElites != value)
                {
                    _IgnoreRareNearElites = value;
                    OnPropertyChanged("IgnoreNonLegendaryNearElites");
                }
            }
        }
        [DataMember(IsRequired = false)]
        [DefaultValue(false)]
        public bool IgnoreGoldInAoE
        {
            get
            {
                return _IgnoreGoldInAoE;
            }
            set
            {
                if (_IgnoreGoldInAoE != value)
                {
                    _IgnoreGoldInAoE = value;
                    OnPropertyChanged("IgnoreGoldInAoE");
                }
            }
        }
        [DataMember(IsRequired = false)]
        [DefaultValue(false)]
        public bool IgnoreGoldNearElites
        {
            get
            {
                return _IgnoreGoldNearElites;
            }
            set
            {
                if (_IgnoreGoldNearElites != value)
                {
                    _IgnoreGoldNearElites = value;
                    OnPropertyChanged("IgnoreGoldNearElites");
                }
            }
        }
        #endregion Properties

        #region Methods
        public void Reset()
        {
            TrinitySetting.Reset(this);
        }

        public void CopyTo(PickupSetting setting)
        {
            TrinitySetting.CopyTo(this, setting);
        }

        public PickupSetting Clone()
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
        [OnDeserializing]
        internal void OnDeserializingMethod(StreamingContext context)
        {
            CraftMaterials = true;
            InfernalKeys = true;
            Designs = true;
            Plans = true;
            LegendaryPlans = true;
            PickupLowLevel = true;
            IgnoreTwoHandedWeapons = false;
            PickupGrayItems = true;
            PickupWhiteItems = true;
            PickupBlueFollowerItems = true;
            PickupYellowFollowerItems = true;
            PickupUpgrades = true;
            MiscItemQuality = TrinityItemQuality.Common;
            BloodShards = true;
            LootRunKey = true;
            PickupGold = true;
        }
        #endregion Methods
    }
}
