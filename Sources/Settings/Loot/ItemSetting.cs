using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace GilesTrinity.Settings.Loot
{
    [DataContract]
    public class ItemSetting : ITrinitySetting<ItemSetting>
    {
        public ItemSetting()
        {
            Reset();
            Pickup = new PickupSetting();
            TownRun = new TownRunSetting();
        }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(ItemFilterMode.TrinityOnly)]
        public ItemFilterMode ItemFilterMode
        { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public PickupSetting Pickup
        { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public TownRunSetting TownRun
        { get; set; }

        public void Reset()
        {
            TrinitySetting.Reset(this);
        }

        public void CopyTo(ItemSetting setting)
        {
            TrinitySetting.CopyTo(this, setting);
        }

        public ItemSetting Clone()
        {
            return TrinitySetting.Clone(this);
        }
    }
}
