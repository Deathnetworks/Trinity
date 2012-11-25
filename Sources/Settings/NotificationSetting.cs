using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace GilesTrinity.Settings
{
    [DataContract]
    public class NotificationSetting : ITrinitySetting<NotificationSetting>
    {
        public NotificationSetting()
        {
            Reset();
        }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(false)]
        public bool IPhoneEnabled
        { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue("")]
        public string IPhoneKey
        { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(false)]
        public bool AndroidEnabled
        { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue("")]
        public string AndroidKey
        { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(false)]
        public bool MailEnabled
        { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue("")]
        public string EmailAddress
        { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue("")]
        public string EmailPassword
        { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue("")]
        public string BotName
        { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(100000)]
        public int WeaponScore
        { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(30000)]
        public int ArmorScore
        { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(28000)]
        public int JewelryScore
        { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(false)]
        public bool LegendaryScoring
        { get; set; }


        public void Reset()
        {
            TrinitySetting.Reset(this);
        }

        public void CopyTo(NotificationSetting setting)
        {
            TrinitySetting.CopyTo(this, setting);
        }

        public NotificationSetting Clone()
        {
            return TrinitySetting.Clone(this);
        }
    }
}
