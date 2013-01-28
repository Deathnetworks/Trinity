using System.ComponentModel;
using System.Runtime.Serialization;

namespace GilesTrinity.Settings.Loot
{
    [DataContract]
    public class ItemSetting : ITrinitySetting<ItemSetting>, INotifyPropertyChanged
    {
        #region Fields
        private ItemFilterMode _ItemFilterMode;
        private ItemRuleType _ItemRuleType;
        private PickupSetting _Pickup;
        private TownRunSetting _TownRun;
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
        [DefaultValue(ItemRuleType.Soft)]
        public ItemRuleType ItemRuleType
        {
            get
            {
                return _ItemRuleType;
            }
            set
            {
                if (_ItemRuleType != value)
                {
                    _ItemRuleType = value;
                    OnPropertyChanged("ItemRuleType");
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
            this.ItemRuleType = ItemRuleType.Config;
        }

        #endregion Methods
    }
}
