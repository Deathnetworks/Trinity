using System.ComponentModel;
using System.Runtime.Serialization;

namespace Trinity.Config.Loot
{
    [DataContract(Namespace = "")]
    public class TownRunSetting : ITrinitySetting<TownRunSetting>, INotifyPropertyChanged, IExtensibleDataObject
    {
        #region Fields
        private int _WeaponScore;
        private int _ArmorScore;
        private int _JewelryScore;
        private SalvageOption _SalvageBlueItemOption;
        private SalvageOption _SalvageYellowItemOption;
        private SalvageOption _SalvageLegendaryItemOption;
        private int _FreeBagSlots;
        #endregion Fields

        #region Events
        /// <summary>
        /// Occurs when property changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion Events

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="TownRunSetting" /> class.
        /// </summary>
        public TownRunSetting()
        {
            Reset();
        }
        #endregion Constructors

        #region Properties
        [DataMember(IsRequired = false)]
        [DefaultValue(70000)]
        public int WeaponScore
        {
            get
            {
                return _WeaponScore;
            }
            set
            {
                if (_WeaponScore != value)
                {
                    _WeaponScore = value;
                    OnPropertyChanged("WeaponScore");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(16000)]
        public int ArmorScore
        {
            get
            {
                return _ArmorScore;
            }
            set
            {
                if (_ArmorScore != value)
                {
                    _ArmorScore = value;
                    OnPropertyChanged("ArmorScore");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(15000)]
        public int JewelryScore
        {
            get
            {
                return _JewelryScore;
            }
            set
            {
                if (_JewelryScore != value)
                {
                    _JewelryScore = value;
                    OnPropertyChanged("JewelryScore");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(SalvageOption.None)]
        public SalvageOption SalvageBlueItemOption
        {
            get
            {
                return _SalvageBlueItemOption;
            }
            set
            {
                if (_SalvageBlueItemOption != value)
                {
                    _SalvageBlueItemOption = value;
                    OnPropertyChanged("SalvageBlueItemOption");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(SalvageOption.None)]
        public SalvageOption SalvageYellowItemOption
        {
            get
            {
                return _SalvageYellowItemOption;
            }
            set
            {
                if (_SalvageYellowItemOption != value)
                {
                    _SalvageYellowItemOption = value;
                    OnPropertyChanged("SalvageYellowItemOption");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(SalvageOption.None)]
        public SalvageOption SalvageLegendaryItemOption
        {
            get
            {
                return _SalvageLegendaryItemOption;
            }
            set
            {
                if (_SalvageLegendaryItemOption != value)
                {
                    _SalvageLegendaryItemOption = value;
                    OnPropertyChanged("SalvageLegendaryItemOption");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(1)]
        public int FreeBagSlots
        {
            get
            {
                return _FreeBagSlots;
            }
            set
            {
                if (_FreeBagSlots != value)
                {
                    _FreeBagSlots = value;
                    OnPropertyChanged("FreeBagSlots");
                }
            }
        }
        #endregion Properties

        #region Methods
        public void Reset()
        {
            TrinitySetting.Reset(this);
        }

        public void CopyTo(TownRunSetting setting)
        {
            TrinitySetting.CopyTo(this, setting);
        }

        public TownRunSetting Clone()
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
            this.FreeBagSlots = 1;
        }

        #endregion Methods

        public ExtensionDataObject ExtensionData
        {
            get
            {
                return null;
            }
            set
            {
                //_ExtendedData = value;
            }
        }
    }
}
