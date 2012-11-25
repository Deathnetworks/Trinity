using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace GilesTrinity.Settings
{
    [DataContract]
    public class NotificationSetting : ITrinitySetting<NotificationSetting>, INotifyPropertyChanged
    {
        #region Fields
        private bool _IPhoneEnabled;
        private string _IPhoneKey;
        private bool _AndroidEnabled;
        private string _AndroidKey;
        private bool _MailEnabled;
        private string _EmailAddress;
        private string _EmailPassword;
        private string _BotName;
        private int _WeaponScore;
        private int _ArmorScore;
        private int _JewelryScore;
        private bool _LegendaryScoring;
        #endregion Fields

        #region Events
        /// <summary>
        /// Occurs when property changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion Events

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationSetting" /> class.
        /// </summary>
        public NotificationSetting()
        {
            Reset();
        }
        #endregion Constructors

        #region Properties
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(false)]
        public bool IPhoneEnabled
        {
            get
            {
                return _IPhoneEnabled;
            }
            set
            {
                if (_IPhoneEnabled != value)
                {
                    _IPhoneEnabled = value;
                    OnPropertyChanged("IPhoneEnabled");
                }
            }
        }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue("")]
        public string IPhoneKey
        {
            get
            {
                return _IPhoneKey;
            }
            set
            {
                if (_IPhoneKey != value)
                {
                    _IPhoneKey = value;
                    OnPropertyChanged("IPhoneKey");
                }
            }
        }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(false)]
        public bool AndroidEnabled
        {
            get
            {
                return _AndroidEnabled;
            }
            set
            {
                if (_AndroidEnabled != value)
                {
                    _AndroidEnabled = value;
                    OnPropertyChanged("AndroidEnabled");
                }
            }
        }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue("")]
        public string AndroidKey
        {
            get
            {
                return _AndroidKey;
            }
            set
            {
                if (_AndroidKey != value)
                {
                    _AndroidKey = value;
                    OnPropertyChanged("AndroidKey");
                }
            }
        }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(false)]
        public bool MailEnabled
        {
            get
            {
                return _MailEnabled;
            }
            set
            {
                if (_MailEnabled != value)
                {
                    _MailEnabled = value;
                    OnPropertyChanged("MailEnabled");
                }
            }
        }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue("")]
        public string EmailAddress
        {
            get
            {
                return _EmailAddress;
            }
            set
            {
                if (_EmailAddress != value)
                {
                    _EmailAddress = value;
                    OnPropertyChanged("EmailAddress");
                }
            }
        }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue("")]
        public string EmailPassword
        {
            get
            {
                return _EmailPassword;
            }
            set
            {
                if (_EmailPassword != value)
                {
                    _EmailPassword = value;
                    OnPropertyChanged("EmailPassword");
                }
            }
        }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue("")]
        public string BotName
        {
            get
            {
                return _BotName;
            }
            set
            {
                if (_BotName != value)
                {
                    _BotName = value;
                    OnPropertyChanged("BotName");
                }
            }
        }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(100000)]
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

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(30000)]
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

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(28000)]
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

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(false)]
        public bool LegendaryScoring
        {
            get
            {
                return _LegendaryScoring;
            }
            set
            {
                if (_LegendaryScoring != value)
                {
                    _LegendaryScoring = value;
                    OnPropertyChanged("LegendaryScoring");
                }
            }
        }
        #endregion Properties

        #region Methods
        public void Reset()
        {
            TrinitySetting.Reset(this);
        }

        public void CopyTo(NotificationSetting setting)
        {
            TrinitySetting.CopyTo(this, setting);
        }

        public NotificationSetting Clone()
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
