using GilesTrinity.Technicals;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace GilesTrinity.Settings
{
    [DataContract]
    public class AdvancedSetting : ITrinitySetting<AdvancedSetting>, INotifyPropertyChanged
    {
        #region Fields
        private bool _UnstuckerEnabled;
        private bool _AllowRestartGame;
        private bool _TPSEnabled;
        private int _TPSLimit;
        private bool _LogStuckLocation;
        private bool _DebugInStatusBar;
        private bool _DebugCache;
        private bool _DebugWeights;
        private bool _DebugItemValuation;
        private LogCategory _LogCategories;
        #endregion Fields

        #region Events
        /// <summary>
        /// Occurs when property changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion Events

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="AdvancedSetting" /> class.
        /// </summary>
        public AdvancedSetting()
        {
            Reset();
        }
        #endregion Constructors

        #region Properties
        [DataMember(IsRequired = false)]
        [DefaultValue(LogCategory.UserInformation)]
        public LogCategory LogCategories
        {
            get
            {
                return _LogCategories;
            }
            set
            {
                if (_LogCategories != value)
                {
                    _LogCategories = value;
                    OnPropertyChanged("LogCategories");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(true)]
        public bool UnstuckerEnabled
        {
            get
            {
                return _UnstuckerEnabled;
            }
            set
            {
                if (_UnstuckerEnabled != value)
                {
                    _UnstuckerEnabled = value;
                    OnPropertyChanged("UnstuckerEnabled");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(false)]
        public bool AllowRestartGame
        {
            get
            {
                return _AllowRestartGame;
            }
            set
            {
                if (_AllowRestartGame != value)
                {
                    _AllowRestartGame = value;
                    OnPropertyChanged("AllowRestartGame");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(false)]
        public bool TPSEnabled
        {
            get
            {
                return _TPSEnabled;
            }
            set
            {
                if (_TPSEnabled != value)
                {
                    _TPSEnabled = value;
                    OnPropertyChanged("TPSEnabled");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(10)]
        public int TPSLimit
        {
            get
            {
                return _TPSLimit;
            }
            set
            {
                if (_TPSLimit != value)
                {
                    _TPSLimit = value;
                    OnPropertyChanged("TPSLimit");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(false)]
        public bool DebugInStatusBar
        {
            get
            {
                return _DebugInStatusBar;
            }
            set
            {
                if (_DebugInStatusBar != value)
                {
                    _DebugInStatusBar = value;
                    OnPropertyChanged("DebugInStatusBar");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(false)]
        public bool LogStuckLocation
        {
            get
            {
                return _LogStuckLocation;
            }
            set
            {
                if (_LogStuckLocation != value)
                {
                    _LogStuckLocation = value;
                    OnPropertyChanged("LogStuckLocation");
                }
            }
        }
        #endregion Properties

        #region Methods
        public void Reset()
        {
            TrinitySetting.Reset(this);
        }

        public void CopyTo(AdvancedSetting setting)
        {
            TrinitySetting.CopyTo(this, setting);
        }

        public AdvancedSetting Clone()
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
        #endregion Methods
    }
}
