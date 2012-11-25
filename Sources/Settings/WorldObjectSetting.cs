using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace GilesTrinity.Settings
{
    [DataContract]
    public class WorldObjectSetting : ITrinitySetting<WorldObjectSetting>
    {
        public WorldObjectSetting()
        {
            Reset();
        }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(15)]
        public int ContainerOpenRange
        { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(12)]
        public int DestructibleRange
        { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(true)]
        public bool UseShrine
        { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(true)]
        public bool IgnoreNonBlocking
        { get; set; }


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
    }
}
