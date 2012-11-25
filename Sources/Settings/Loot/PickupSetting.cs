using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace GilesTrinity.Settings.Loot
{
    [DataContract]
    public class PickupSetting : ITrinitySetting<PickupSetting>
    {
        public PickupSetting()
        {
            Reset();
        }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(1)]
        public int WeaponBlueLevel
        { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(1)]
        public int WeaponYellowLevel
        { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(1)]
        public int ArmorBlueLevel
        { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(1)]
        public int ArmorYellowLevel
        { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(1)]
        public int JewelryBlueLevel
        { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(1)]
        public int JewelryYellowLevel
        { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(PotionMode.Cap)]
        public PotionMode PotionMode
        { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(1)]
        public int Potionlevel
        { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(TrinityGemType.Ruby | TrinityGemType.Amethys | TrinityGemType.Emerald | TrinityGemType.Topaz)]
        public TrinityGemType GemType
        { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(1)]
        public int GemLevel
        { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(1)]
        public int LegendaryLevel
        { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(1)]
        public int MinimumGoldStack
        { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(true)]
        public bool CraftTomes
        { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(true)]
        public bool DesignPlan
        { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(true)]
        public bool FollowerItem
        { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(1)]
        public int MiscItemLevel
        { get; set; }

        public void Reset()
        {
            TrinitySetting.Reset(this);
        }

        public void CopyTo(PickupSetting setting)
        {
            TrinitySetting.CopyTo(this,setting);
        }

        public PickupSetting Clone()
        {
            return TrinitySetting.Clone(this);
        }
    }
}
