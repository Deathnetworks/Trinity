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
        private int _ContainerOpenRange;
        private int _DestructibleRange;
        private bool _UseShrine;
        private bool _IgnoreNonBlocking;

        public WorldObjectSetting()
        {
            Reset();
        }

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

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
