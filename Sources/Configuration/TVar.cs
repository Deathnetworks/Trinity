using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using Trinity.Technicals;

namespace Trinity
{
    /// <summary>
    /// A specific Trinity Variable
    /// </summary>
    [DataContract(Namespace = "")]
    public class TVar : IEquatable<TVar>, ICloneable, INotifyPropertyChanged, IEditableObject
    {
        private string _name;
        private string _type;
        private object _value;
        private object _defaultValue;
        private object _profileValue;
        private bool _allowProfileSet;
        private bool _userAllowProfileSet;
        private string _description;

        #region Events
        /// <summary>
        /// Occurs when property changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion Events

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Name
        {
            get { return _name; }
            set { _name = value; OnPropertyChanged("Name"); }
        }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Type
        {
            get { return _type; }
            set
            {
                switch (value)
                {
                    case "String": _type = "string"; break;
                    case "Int32": _type = "int"; break;
                    case "Single": _type = "float"; break;
                    case "Double": _type = "double"; break;
                    case "Boolean": _type = "bool"; break;
                    default:
                        _type = value;
                        break;
                }
                OnPropertyChanged("Type");
            }
        }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public object Value
        {
            get { return _value; }
            set
            {
                if (value == null)
                {
                    OnPropertyChanged("Value");
                    return;
                }

                Type defaultType = _defaultValue.GetType();
                Type newType = value.GetType();

                try
                {
                    if (defaultType == typeof(int))
                        _value = Int32.Parse(value.ToString());
                    else if (defaultType == typeof(bool))
                        _value = Boolean.Parse(value.ToString());
                    else if (defaultType == typeof(float))
                        _value = Single.Parse(value.ToString());
                    else if (defaultType == typeof(double))
                        _value = Double.Parse(value.ToString());
                    else
                        _value = value; // string
                }
                catch
                {
                    Logger.LogNormal("Specified value is invalid for {0}. Value={1} Type={2}, expected {3}", this.Name, value, value.GetType().Name, defaultType.Name);
                }

                OnPropertyChanged("Value");
            }
        }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public object DefaultValue
        {
            get { return _defaultValue; }
            set { _defaultValue = value; OnPropertyChanged("DefaultValue"); }
        }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public object ProfileValue
        {
            get { return _profileValue; }
            set { _profileValue = value; OnPropertyChanged("ProfileValue"); }
        }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool AllowProfileSet
        {
            get { return _allowProfileSet; }
            set { _allowProfileSet = value; OnPropertyChanged("AllowProfileSet"); }
        }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool UserAllowProfileSet
        {
            get { return _userAllowProfileSet; }
            set { _userAllowProfileSet = value; OnPropertyChanged("userAllowProfileSet"); }
        }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Description
        {
            get { return _description; }
            set { _description = value; OnPropertyChanged("Description"); }
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

        public TVar()
        {

        }

        public TVar(string name, object value, string description = "")
        {
            Name = name;
            DefaultValue = value;
            Value = value;
            Description = description;
            ProfileValue = value;
            Type = value.GetType().Name.ToString();
            AllowProfileSet = false;
            UserAllowProfileSet = false;
        }

        public TVar(string name, object value, object defaultValue, string description = "")
        {
            Name = name;
            DefaultValue = defaultValue;
            Value = value;
            Description = description;
            ProfileValue = value;
            Type = value.GetType().Name.ToString();
            AllowProfileSet = false;
            UserAllowProfileSet = false;
        }

        public TVar(string name, object value, object defaultValue, object profileValue, string description = "")
        {
            Name = name;
            DefaultValue = defaultValue;
            Value = value;
            Description = description;
            ProfileValue = profileValue;
            Type = value.GetType().Name.ToString();
            AllowProfileSet = false;
            UserAllowProfileSet = false;
        }

        public TVar(string name, object value, object defaultValue, object profileValue, string description, bool allowProfileSet, bool userAllowProfileSet)
        {
            Name = name;
            DefaultValue = defaultValue;
            Value = value;
            Description = description;
            ProfileValue = profileValue;
            Type = value.GetType().Name.ToString();
            AllowProfileSet = allowProfileSet;
            UserAllowProfileSet = userAllowProfileSet;
        }

        public bool Equals(TVar other)
        {
            return this.Name == other.Name && this.Value == other.Value;
        }

        public object Clone()
        {
            TVar clone = new TVar()
            {
                Name = this.Name,
                DefaultValue = this.DefaultValue,
                Value = this.Value,
                Type = this.Type,
                Description = this.Description,
                ProfileValue = this.ProfileValue,
                AllowProfileSet = this.AllowProfileSet,
                UserAllowProfileSet = this.UserAllowProfileSet
            };
            return clone;
        }

        private TVar backup;
        private bool inTxn;

        public void BeginEdit()
        {
            if (!inTxn)
            {
                backup = (TVar)this.Clone();
                inTxn = true;
            }
        }

        public void CancelEdit()
        {
            if (inTxn)
            {
                this.Name = Name;
                this.DefaultValue = DefaultValue;
                this.Value = Value;
                this.Type = Type;
                this.Description = Description;
                this.ProfileValue = ProfileValue;
                this.AllowProfileSet = AllowProfileSet;
                this.UserAllowProfileSet = UserAllowProfileSet;
            }
        }

        public void EndEdit()
        {
            if (inTxn)
            {
                backup = new TVar();
                inTxn = false;
            }
        }

        public override string ToString()
        {
            return string.Format("TVar Name={0} Value={1} Type={2}",
                this.Name,
                this.Value,
                this.Value.GetType().Name);
        }
    }
}
