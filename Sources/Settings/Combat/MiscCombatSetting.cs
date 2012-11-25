using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace GilesTrinity.Settings.Combat
{
    [DataContract]
    public class MiscCombatSetting : ITrinitySetting<MiscCombatSetting>, INotifyPropertyChanged
    {
        public MiscCombatSetting()
        {
            Reset();
        }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(GoblinPriority.Prioritize)]
        public GoblinPriority GoblinPriority
        { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(40)]
        public int NonEliteRange
        { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(true)]
        public bool ExtendedTrashKill
        { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(true)]
        public bool AvoidAOE
        { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(true)]
        public bool CollectHealthGlobe
        { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(true)]
        public bool AllowOOCMovement
        { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(false)]
        public bool AllowBacktracking
        { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(800)]
        public int DelayAfterKill
        { get; set; }

        public void Reset()
        {
            TrinitySetting.Reset(this);
        }

        public void CopyTo(MiscCombatSetting setting)
        {
            TrinitySetting.CopyTo(this, setting);
        }

        public MiscCombatSetting Clone()
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
