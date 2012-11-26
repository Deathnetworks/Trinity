using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace GilesTrinity.Settings.Combat
{
    [DataContract]
    public class AvoidanceRadiusSetting : ITrinitySetting<AvoidanceRadiusSetting>, INotifyPropertyChanged
    {
        [DataMember(IsRequired = false)]
        [DefaultValue(12)]
        public int Arcane
        { get; set; }

        [DataMember(IsRequired = false)]
        [DefaultValue(10)]
        public int Desecrator
        { get; set; }

        [DataMember(IsRequired = false)]
        [DefaultValue(19)]
        public int MoltenCore
        { get; set; }

        [DataMember(IsRequired = false)]
        [DefaultValue(6)]
        public int MoltenTrail
        { get; set; }

        [DataMember(IsRequired = false)]
        [DefaultValue(14)]
        public int PoisonTree
        { get; set; }

        [DataMember(IsRequired = false)]
        [DefaultValue(14)]
        public int PlagueCloud
        { get; set; }

        [DataMember(IsRequired = false)]
        [DefaultValue(20)]
        public int IceBalls
        { get; set; }

        [DataMember(IsRequired = false)]
        [DefaultValue(12)]
        public int PlagueHands
        { get; set; }

        [DataMember(IsRequired = false)]
        [DefaultValue(10)]
        public int BeesWasps
        { get; set; }

        [DataMember(IsRequired = false)]
        [DefaultValue(54)]
        public int AzmoPools
        { get; set; }

        [DataMember(IsRequired = false)]
        [DefaultValue(47)]
        public int AzmoBodies
        { get; set; }

        [DataMember(IsRequired = false)]
        [DefaultValue(8)]
        public int ShamanFire
        { get; set; }

        [DataMember(IsRequired = false)]
        [DefaultValue(25)]
        public int GhomGas
        { get; set; }

        [DataMember(IsRequired = false)]
        [DefaultValue(30)]
        public int AzmoFireBall 
        { get; set; }

        [DataMember(IsRequired = false)]
        [DefaultValue(20)]
        public int Belial { get; set; }

        [DataMember(IsRequired = false)]
        [DefaultValue(40)]
        public int ButcherFloorPanel { get; set; }

        [DataMember(IsRequired = false)]
        [DefaultValue(28)]
        public int DiabloMeteor { get; set; }

        [DataMember(IsRequired = false)]
        [DefaultValue(15)]
        public int DiabloPrison { get; set; }

        [DataMember(IsRequired = false)]
        [DefaultValue(50)]
        public int DiabloRingOfFire { get; set; }

        [DataMember(IsRequired = false)]
        [DefaultValue(6)]
        public int IceTrail { get; set; }

        [DataMember(IsRequired = false)]
        [DefaultValue(10)]
        public int MageFire { get; set; }

        [DataMember(IsRequired = false)]
        [DefaultValue(8)]
        public int MaghdaProjectille { get; set; }

        [DataMember(IsRequired = false)]
        [DefaultValue(8)]
        public int MoltenBall { get; set; }

        [DataMember(IsRequired = false)]
        [DefaultValue(10)]
        public int WallOfFire { get; set; }

        [DataMember(IsRequired = false)]
        [DefaultValue(20)]
        public int ZoltBubble { get; set; }

        [DataMember(IsRequired = false)]
        [DefaultValue(12)]
        public int ZoltTwister { get; set; }

        public void Reset()
        {
            TrinitySetting.Reset(this);
        }

        public void CopyTo(AvoidanceRadiusSetting setting)
        {
            TrinitySetting.CopyTo(this, setting);
        }

        public AvoidanceRadiusSetting Clone()
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
