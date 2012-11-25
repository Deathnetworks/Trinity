using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace GilesTrinity.Settings.Loot
{
    [DataContract]
    public class TownRunSetting : ITrinitySetting<TownRunSetting>
    {
        public TownRunSetting()
        { 
            Reset(); 
        }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(TrashMode.Selling)]
        public TrashMode TrashMode
        { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(70000)]
        public int WeaponScore
        { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(16000)]
        public int ArmorScore
        { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(15000)]
        public int JewelryScore
        { get; set; }

        public void Reset()
        {
            TrinitySetting.Reset(this);
        }

        public void CopyTo(TownRunSetting setting)
        {
            TrinitySetting.CopyTo(this, setting);
        }

        public TownRunSetting Clone()
        {
            return TrinitySetting.Clone(this);
        }
    }
}
