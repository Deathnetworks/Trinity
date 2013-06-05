using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Trinity
{
    /// <summary>
    /// A specific Trinity Variable
    /// </summary>
    public class TVar : IEquatable<TVar>, ICloneable
    {
        public string Name { get; set; }
        public Type Type { get; set; }
        public object Value { get; set; }
        public object DefaultValue { get; set; }
        public object ProfileValue { get; set; }
        public bool AllowProfileSet { get; set; }
        public bool UserAllowProfileSet { get; set; }
        public string Description { get; set; }

        public TVar()
        {

        }

        public TVar(string name, object value, string description = "")
        {
            Name = name;
            Value = value;
            DefaultValue = value;
            ProfileValue = value;
            Type = value.GetType();
            AllowProfileSet = false;
            UserAllowProfileSet = false;
        }

        public TVar(string name, object value, object defaultValue, string description = "")
        {
            Name = name;
            Value = value;
            DefaultValue = defaultValue;
            ProfileValue = value;
            Type = value.GetType();
            AllowProfileSet = false;
            UserAllowProfileSet = false;
        }

        public TVar(string name, object value, object defaultValue, object profileValue, string description = "")
        {
            Name = name;
            Value = value;
            DefaultValue = defaultValue;
            ProfileValue = profileValue;
            Type = value.GetType();
            AllowProfileSet = false;
            UserAllowProfileSet = false;
        }

        public TVar(string name, object value, object defaultValue, object profileValue, string description, bool allowProfileSet, bool userAllowProfileSet)
        {
            Name = name;
            Value = value;
            DefaultValue = defaultValue;
            ProfileValue = profileValue;
            Type = value.GetType();
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
                Value = this.Value,
                DefaultValue = this.DefaultValue,
                ProfileValue = this.ProfileValue,
                AllowProfileSet = this.AllowProfileSet,
                UserAllowProfileSet = this.UserAllowProfileSet
            };
            return clone;
        }
    }
}
