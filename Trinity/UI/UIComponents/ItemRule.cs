using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Trinity.UIComponents
{
    [DataContract(Namespace = "")]
    public class ItemRule : INotifyPropertyChanged
    {
        public ItemRule()
        {
            Value = 1;
        }

        private int _value;
        private int _min;
        private int _max;

        public string Name { get { return ItemProperty.ToString(); }}

        [DataMember]
        public int ItemPropertyId { get; set; }

        public ItemProperty ItemProperty
        {
            get { return (ItemProperty)ItemPropertyId; }
        }

        [DataMember]        
        public int Value
        {
            get { return _value; }
            set
            {
                if (_value != value)
                {
                    _value = value;
                    OnPropertyChanged();
                }
            }
        }

        [DataMember]
        public int Min
        {
            get { return _min; }
            set
            {
                if (_min != value)
                {
                    _min = value;
                    OnPropertyChanged();
                }
            }
        }

        [DataMember]
        public int Max
        {
            get { return _max; }
            set
            {
                if (_max != value)
                {
                    _max = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
