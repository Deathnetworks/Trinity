using System.ComponentModel;
using System.Runtime.Serialization;

namespace GilesTrinity.Settings
{
    [DataContract]
    public class WorldObjectSetting : ITrinitySetting<WorldObjectSetting>, INotifyPropertyChanged
    {

        #region Fields
        private int _ContainerOpenRange;
        private int _DestructibleRange;
        private bool _UseShrine;
        private bool _UseFrenzyShrine;
        private bool _UseFortuneShrine;
        private bool _UseProtectionShrine;
        private bool _UseEmpoweredShrine;
        private bool _UseEnlightenedShrine;
        private bool _UseFleetingShrine;
        private bool _IgnoreNonBlocking;
        #endregion Fields

        #region Events
        /// <summary>
        /// Occurs when property changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion Events

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="WorldObjectSetting" /> class.
        /// </summary>
        public WorldObjectSetting()
        {
            Reset();
        }
        #endregion Constructors

        #region Properties
        [DataMember(IsRequired = false)]
        [DefaultValue(15)]
        public int ContainerOpenRange
        {
            get
            {
                return _ContainerOpenRange;
            }
            set 
            {
                if (_ContainerOpenRange != value)
                {
                    _ContainerOpenRange = value;
                    OnPropertyChanged("ContainerOpenRange");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(12)]
        public int DestructibleRange
        {
            get
            {
                return _DestructibleRange;
            }
            set
            {
                if (_DestructibleRange != value)
                {
                    _DestructibleRange = value;
                    OnPropertyChanged("DestructibleRange");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(true)]
        public bool UseShrine
        {
            get
            {
                return _UseShrine;
            }
            set
            {
                if (_UseShrine != value)
                {
                    _UseShrine = value;
                    OnPropertyChanged("UseShrine");
                }
            }
        }
        
        [DataMember(IsRequired = false)]
        [DefaultValue(true)]
        public bool UseEnlightenedShrine
        {
            get
            {
                return _UseEnlightenedShrine;
            }
            set
            {
                if (_UseEnlightenedShrine != value)
                {
                    _UseEnlightenedShrine = value;
                    OnPropertyChanged("UseEnlightenedShrine");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(true)]
        public bool UseFrenzyShrine
        {
            get
            {
                return _UseFrenzyShrine;
            }
            set
            {
                if (_UseFrenzyShrine != value)
                {
                    _UseFrenzyShrine = value;
                    OnPropertyChanged("UseFrenzyShrine");
                }
            }
        }
        
        [DataMember(IsRequired = false)]
        [DefaultValue(true)]
        public bool UseFortuneShrine
        {
            get
            {
                return _UseFortuneShrine;
            }
            set
            {
                if (_UseFortuneShrine != value)
                {
                    _UseFortuneShrine = value;
                    OnPropertyChanged("UseFortuneShrine");
                }
            }
        }
        
        [DataMember(IsRequired = false)]
        [DefaultValue(true)]
        public bool UseProtectionShrine
        {
            get
            {
                return _UseProtectionShrine;
            }
            set
            {
                if (_UseProtectionShrine != value)
                {
                    _UseProtectionShrine = value;
                    OnPropertyChanged("UseProtectionShrine");
                }
            }
        }
        
        [DataMember(IsRequired = false)]
        [DefaultValue(true)]
        public bool UseEmpoweredShrine
        {
            get
            {
                return _UseEmpoweredShrine;
            }
            set
            {
                if (_UseEmpoweredShrine != value)
                {
                    _UseEmpoweredShrine = value;
                    OnPropertyChanged("UseEmpoweredShrine");
                }
            }
        }
        
        [DataMember(IsRequired = false)]
        [DefaultValue(true)]
        public bool UseFleetingShrine
        {
            get
            {
                return _UseFleetingShrine;
            }
            set
            {
                if (_UseFleetingShrine != value)
                {
                    _UseFleetingShrine = value;
                    OnPropertyChanged("UseFleetingShrine");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(true)]
        public bool IgnoreNonBlocking
        {
            get
            {
                return _IgnoreNonBlocking;
            }
            set
            {
                if (_IgnoreNonBlocking != value)
                {
                    _IgnoreNonBlocking = value;
                    OnPropertyChanged("IgnoreNonBlocking");
                }
            }
        }
        #endregion Properties

        #region Methods
        public void Reset()
        {
            TrinitySetting.Reset(this);
        }

        public void CopyTo(WorldObjectSetting setting)
        {
            TrinitySetting.CopyTo(this, setting);
        }

        public WorldObjectSetting Clone()
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
            this.UseEmpoweredShrine = true;
            this.UseEnlightenedShrine = true;
            this.UseFleetingShrine = true;
            this.UseFortuneShrine = true;
            this.UseFrenzyShrine = true;
            this.UseProtectionShrine = true;
        }
        #endregion Methods
    }
}
