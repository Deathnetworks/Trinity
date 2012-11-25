using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace GilesTrinity.Settings
{
    [DataContract]
    public class MailServerSetting : ITrinitySetting<MailServerSetting>
    {
        public MailServerSetting()
        {
            Reset();
        }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue("")]
        public string ServerAddress
        { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue("")]
        public string Username
        { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue("")]
        public string Password
        { get; set; }

        public void Reset()
        {
            TrinitySetting.Reset(this);
        }

        public void CopyTo(MailServerSetting setting)
        {
            TrinitySetting.CopyTo(this, setting);
        }

        public MailServerSetting Clone()
        {
            return TrinitySetting.Clone(this);
        }
    }
}
