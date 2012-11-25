using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace GilesTrinity.Settings.Combat
{
    [DataContract]
    public class WitchDoctorSetting : ITrinitySetting<WitchDoctorSetting>, IAvoidanceHealth
    {
        public WitchDoctorSetting()
        {
            Reset();
        }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(0.7f)]
        public float PotionLevel
        { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(0.8f)]
        public float HealthGlobeLevel
        { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(10)]
        public int KiteLimit
        { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(1f)]
        public float AvoidArcaneHealth
        { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(1f)]
        public float AvoidDesecratorHealth
        { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(1f)]
        public float AvoidMoltenCoreHealth
        { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(0.9f)]
        public float AvoidMoltenTrailHealth
        { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(0.9f)]
        public float AvoidPoisonTreeHealth
        { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(0.9f)]
        public float AvoidPlagueCloudHealth
        { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(1f)]
        public float AvoidIceBallsHealth
        { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(1f)]
        public float AvoidPlagueHandsHealth
        { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(1f)]
        public float AvoidBeesWaspsHealth
        { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(1f)]
        public float AvoidAzmoPoolsHealth
        { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(1f)]
        public float AvoidAzmoBodiesHealth
        { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(0.85f)]
        public float AvoidShamanFireHealth
        { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(1f)]
        public float AvoidGhomGasHealth
        { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(1f)]
        public float AvoidAzmoFireBallHealth
        { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(1f)]
        public float AvoidBelialHealth { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(1f)]
        public float AvoidButcherFloorPanelHealth { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(0.8f)]
        public float AvoidDiabloMeteorHealth { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(1f)]
        public float AvoidDiabloPrisonHealth { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(0.9f)]
        public float AvoidDiabloRingOfFireHealth { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(1f)]
        public float AvoidIceTrailHealth { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(0.3f)]
        public float AvoidMageFireHealth { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(0.7f)]
        public float AvoidMaghdaProjectilleHealth { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(0.1f)]
        public float AvoidMoltenBallHealth { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(0.5f)]
        public float AvoidWallOfFireHealth { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(1f)]
        public float AvoidZoltBubbleHealth { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(0.7f)]
        public float AvoidZoltTwisterHealth { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(false)]
        public bool GraveInjustice
        { get; set; }

        public void Reset()
        {
            TrinitySetting.Reset(this);
        }

        public void CopyTo(WitchDoctorSetting setting)
        {
            TrinitySetting.CopyTo(this, setting);
        }

        public WitchDoctorSetting Clone()
        {
            return TrinitySetting.Clone(this);
        }
    }
}
