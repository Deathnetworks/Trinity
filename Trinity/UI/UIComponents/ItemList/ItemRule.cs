using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Trinity.Objects;
using Trinity.Reference;

namespace Trinity.UIComponents
{
    [DataContract(Namespace = "")]
    public class ItemRule : INotifyPropertyChanged
    {
        public ItemRule()
        {
            Value = 1;
        }

        private double _value;

        public string Name { get { return ItemProperty.ToString(); }}

        [DataMember]
        public int Id { get; set; }

        public ItemProperty ItemProperty
        {
            get { return (ItemProperty)Id; }
        }

        [DataMember]        
        public double Value
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

        public double Min
        {
            get { return ItemStatRange.AbsMin; }
        }

        public double Max
        {
            get { return ItemStatRange.AbsMax; }
        }

        public double Step
        {
            get { return ItemStatRange.AbsStep; }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public ItemStatRange ItemStatRange { get; set; }
    }
}
