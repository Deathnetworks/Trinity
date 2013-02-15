using System.ComponentModel;
using System.Runtime.Serialization;

namespace GilesTrinity.Settings.Loot
{
    [DataContract]
    public class PickupSetting : ITrinitySetting<PickupSetting>, INotifyPropertyChanged
    {
        #region Fields
        private int _WeaponBlueLevel;
        private int _WeaponYellowLevel;
        private int _ArmorBlueLevel;
        private int _ArmorYellowLevel;
        private int _JewelryBlueLevel;
        private int _JewelryYellowLevel;
        private PotionMode _PotionMode;
        private int _Potionlevel;
        private TrinityGemType _GemType;
        private int _GemLevel;
        private int _LegendaryLevel;
        private int _MinimumGoldStack;
        private bool _CraftTomes;
        private bool _DesignPlan;
        private bool _FollowerItem;
        private int _MiscItemLevel;
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
        [DefaultValue(63)]
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
        [DefaultValue(63)]
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
        [DefaultValue(58)]
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
        [DefaultValue(PotionMode.Cap)]
        public PotionMode PotionMode
        {
            get
            {
                return _PotionMode;
            }
            set
            {
                if (_PotionMode != value)
                {
                    _PotionMode = value;
                    OnPropertyChanged("PotionMode");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(60)]
        public int PotionLevel
        {
            get
            {
                return _Potionlevel;
            }
            set
            {
                if (_Potionlevel != value)
                {
                    _Potionlevel = value;
                    OnPropertyChanged("PotionLevel");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(TrinityGemType.Ruby | TrinityGemType.Amethyst | TrinityGemType.Emerald | TrinityGemType.Topaz)]
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
        [DefaultValue(14)]
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
        [DefaultValue(100)]
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
        public bool DesignPlan
        {
            get
            {
                return _DesignPlan;
            }
            set
            {
                if (_DesignPlan != value)
                {
                    _DesignPlan = value;
                    OnPropertyChanged("DesignPlan");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(true)]
        public bool FollowerItem
        {
            get
            {
                return _FollowerItem;
            }
            set
            {
                if (_FollowerItem != value)
                {
                    _FollowerItem = value;
                    OnPropertyChanged("FollowerItem");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(60)]
        public int MiscItemLevel
        {
            get
            {
                return _MiscItemLevel;
            }
            set
            {
                if (_MiscItemLevel != value)
                {
                    _MiscItemLevel = value;
                    OnPropertyChanged("MiscItemLevel");
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
            TrinitySetting.CopyTo(this,setting);
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
        #endregion Methods
    }
}
