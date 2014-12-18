using System.ComponentModel;
using System.Runtime.Serialization;
using Trinity.Settings.Loot;

namespace Trinity.Config.Loot
{
    [DataContract(Namespace = "")]
    public class ItemSetting : ITrinitySetting<ItemSetting>, INotifyPropertyChanged
    {
        #region Fields
        private ItemFilterMode _ItemFilterMode;
        private PickupSetting _Pickup;
        private TownRunSetting _TownRun;
        private ItemRuleSetting _ItemRules;
        private ItemRankSettings _itemRank;
        #endregion Fields

        #region Events
        /// <summary>
        /// Occurs when property changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion Events

        #region Constructors
        public ItemSetting()
        {
            Reset();
            Pickup = new PickupSetting();
            TownRun = new TownRunSetting();
            ItemRules = new ItemRuleSetting();
        }
        #endregion Constructors

        #region Properties
        [DataMember(IsRequired = false)]
        [DefaultValue(ItemFilterMode.TrinityOnly)]
        public ItemFilterMode ItemFilterMode
        {
            get
            {
                return _ItemFilterMode;
            }
            set
            {
                if (_ItemFilterMode != value)
                {
                    _ItemFilterMode = value;
                    OnPropertyChanged("ItemFilterMode");
                }
            }
        }

        [DataMember(IsRequired = false)]
        public PickupSetting Pickup
        {
            get
            {
                return _Pickup;
            }
            set
            {
                if (_Pickup != value)
                {
                    _Pickup = value;
                    OnPropertyChanged("Pickup");
                }
            }
        }

        [DataMember(IsRequired = false)]
        public TownRunSetting TownRun
        {
            get
            {
                return _TownRun;
            }
            set
            {
                if (_TownRun != value)
                {
                    _TownRun = value;
                    OnPropertyChanged("TownRun");
                }
            }
        }
        [DataMember(IsRequired = false)]
        public ItemRuleSetting ItemRules
        {
            get
            {
                return _ItemRules;
            }
            set
            {
                if (_ItemRules != value)
                {
                    _ItemRules = value;
                    OnPropertyChanged("ItemRules");
                }
            }
        }
        [DataMember(IsRequired = false)]
        public ItemRankSettings ItemRank
        {
            get
            {
                return _itemRank;
            }
            set
            {
                if (_itemRank != value)
                {
                    _itemRank = value;
                    OnPropertyChanged("ItemRank");
                }
            }
        }
        #endregion Properties

        #region Methods
        public void Reset()
        {
            TrinitySetting.Reset(this);
        }

        public void CopyTo(ItemSetting setting)
        {
            TrinitySetting.CopyTo(this, setting);
        }

        public ItemSetting Clone()
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

        }

        #endregion Methods
    }
}
