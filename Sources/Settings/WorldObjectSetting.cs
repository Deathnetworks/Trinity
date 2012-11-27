using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace GilesTrinity.Settings
{
    [DataContract]
    public class WorldObjectSetting : ITrinitySetting<WorldObjectSetting>, INotifyPropertyChanged
    {

        #region Fields
        private int _ContainerOpenRange;
        private int _DestructibleRange;
        private bool _UseShrine;
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
        #endregion Methods
    }
}
