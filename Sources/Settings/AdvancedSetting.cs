using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace GilesTrinity.Settings
{
    [DataContract]
    public class AdvancedSetting : ITrinitySetting<AdvancedSetting>
    {
        public AdvancedSetting()
        {
            Reset();
        }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(true)]
        public bool UnstuckerEnabled
        { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(false)]
        public bool AllowRestartGame
        { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(false)]
        public bool TPSEnabled
        { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(10)]
        public int TPSLimit
        { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(false)]
        public bool DebugInStatusBar
        { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(false)]
        public bool LogStuckLocation
        { get; set; }

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
    }
}
