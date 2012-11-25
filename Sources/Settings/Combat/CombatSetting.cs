using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace GilesTrinity.Settings.Combat
{
    [DataContract]
    public class CombatSetting : ITrinitySetting<CombatSetting>
    {
        public CombatSetting()
        {
            Misc = new MiscCombatSetting();
            AvoidanceRadius = new AvoidanceRadiusSetting();
            Barbarian = new BarbarianSetting();
            Monk = new MonkSetting();
            Wizard = new WizardSetting();
            WitchDoctor = new WitchDoctorSetting();
            DemonHunter = new DemonHunterSetting();
        }

        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public MiscCombatSetting Misc
        { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public AvoidanceRadiusSetting AvoidanceRadius
        { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public BarbarianSetting Barbarian
        { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public MonkSetting Monk
        { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public WizardSetting Wizard
        { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public WitchDoctorSetting WitchDoctor
        { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public DemonHunterSetting DemonHunter
        { get; set; }

        public void Reset()
        {
            TrinitySetting.Reset(this);
        }

        public void CopyTo(CombatSetting setting)
        {
            TrinitySetting.CopyTo(this, setting);
        }

        public CombatSetting Clone()
        {
            return TrinitySetting.Clone(this);
        }
    }
}
