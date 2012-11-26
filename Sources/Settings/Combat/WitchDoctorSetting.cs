﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace GilesTrinity.Settings.Combat
{
    [DataContract]
    public class WitchDoctorSetting : ITrinitySetting<WitchDoctorSetting>, IAvoidanceHealth, INotifyPropertyChanged
    {
        #region Fields
        private float _PotionLevel;
        private float _HealthGlobeLevel;
        private int _KiteLimit;
        private float _AvoidArcaneHealth;
        private float _AvoidDesecratorHealth;
        private float _AvoidMoltenCoreHealth;
        private float _AvoidMoltenTrailHealth;
        private float _AvoidPoisonTreeHealth;
        private float _AvoidPlagueCloudHealth;
        private float _AvoidIceBallsHealth;
        private float _AvoidPlagueHandsHealth;
        private float _AvoidBeesWaspsHealth;
        private float _AvoidAzmoPoolsHealth;
        private float _AvoidAzmoBodiesHealth;
        private float _AvoidShamanFireHealth;
        private float _AvoidGhomGasHealth;
        private float _AvoidAzmoFireBallHealth;
        private float _AvoidBelialHealth;
        private float _AvoidButcherFloorPanelHealth;
        private float _AvoidDiabloMeteorHealth;
        private float _AvoidDiabloPrisonHealth;
        private float _AvoidDiabloRingOfFireHealth;
        private float _AvoidIceTrailHealth;
        private float _AvoidMageFireHealth;
        private float _AvoidMaghdaProjectilleHealth;
        private float _AvoidMoltenBallHealth;
        private float _AvoidWallOfFireHealth;
        private float _AvoidZoltBubbleHealth;
        private float _AvoidZoltTwisterHealth;
        private bool _GraveInjustice;
        #endregion Fields

        #region Events
        /// <summary>
        /// Occurs when property changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion Events

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="WitchDoctorSetting" /> class.
        /// </summary>
        public WitchDoctorSetting()
        {
            Reset();
        }
        #endregion Constructors

        #region Properties
        [DataMember(IsRequired = false)]
        [DefaultValue(0.7f)]
        public float PotionLevel
        { get; set; }

        [DataMember(IsRequired = false)]
        [DefaultValue(0.8f)]
        public float HealthGlobeLevel
        { get; set; }

        [DataMember(IsRequired = false)]
        [DefaultValue(10)]
        public int KiteLimit
        { get; set; }

        [DataMember(IsRequired = false)]
        [DefaultValue(1f)]
        public float AvoidArcaneHealth
        { get; set; }

        [DataMember(IsRequired = false)]
        [DefaultValue(1f)]
        public float AvoidDesecratorHealth
        { get; set; }

        [DataMember(IsRequired = false)]
        [DefaultValue(1f)]
        public float AvoidMoltenCoreHealth
        { get; set; }

        [DataMember(IsRequired = false)]
        [DefaultValue(0.9f)]
        public float AvoidMoltenTrailHealth
        { get; set; }

        [DataMember(IsRequired = false)]
        [DefaultValue(0.9f)]
        public float AvoidPoisonTreeHealth
        { get; set; }

        [DataMember(IsRequired = false)]
        [DefaultValue(0.9f)]
        public float AvoidPlagueCloudHealth
        { get; set; }

        [DataMember(IsRequired = false)]
        [DefaultValue(1f)]
        public float AvoidIceBallsHealth
        { get; set; }

        [DataMember(IsRequired = false)]
        [DefaultValue(1f)]
        public float AvoidPlagueHandsHealth
        { get; set; }

        [DataMember(IsRequired = false)]
        [DefaultValue(1f)]
        public float AvoidBeesWaspsHealth
        { get; set; }

        [DataMember(IsRequired = false)]
        [DefaultValue(1f)]
        public float AvoidAzmoPoolsHealth
        { get; set; }

        [DataMember(IsRequired = false)]
        [DefaultValue(1f)]
        public float AvoidAzmoBodiesHealth
        { get; set; }

        [DataMember(IsRequired = false)]
        [DefaultValue(0.85f)]
        public float AvoidShamanFireHealth
        { get; set; }

        [DataMember(IsRequired = false)]
        [DefaultValue(1f)]
        public float AvoidGhomGasHealth
        { get; set; }

        [DataMember(IsRequired = false)]
        [DefaultValue(1f)]
        public float AvoidAzmoFireBallHealth
        { get; set; }

        [DataMember(IsRequired = false)]
        [DefaultValue(1f)]
        public float AvoidBelialHealth { get; set; }

        [DataMember(IsRequired = false)]
        [DefaultValue(1f)]
        public float AvoidButcherFloorPanelHealth { get; set; }

        [DataMember(IsRequired = false)]
        [DefaultValue(0.8f)]
        public float AvoidDiabloMeteorHealth { get; set; }

        [DataMember(IsRequired = false)]
        [DefaultValue(1f)]
        public float AvoidDiabloPrisonHealth { get; set; }

        [DataMember(IsRequired = false)]
        [DefaultValue(0.9f)]
        public float AvoidDiabloRingOfFireHealth { get; set; }

        [DataMember(IsRequired = false)]
        [DefaultValue(1f)]
        public float AvoidIceTrailHealth { get; set; }

        [DataMember(IsRequired = false)]
        [DefaultValue(0.3f)]
        public float AvoidMageFireHealth { get; set; }

        [DataMember(IsRequired = false)]
        [DefaultValue(0.7f)]
        public float AvoidMaghdaProjectilleHealth { get; set; }

        [DataMember(IsRequired = false)]
        [DefaultValue(0.1f)]
        public float AvoidMoltenBallHealth { get; set; }

        [DataMember(IsRequired = false)]
        [DefaultValue(0.5f)]
        public float AvoidWallOfFireHealth { get; set; }

        [DataMember(IsRequired = false)]
        [DefaultValue(1f)]
        public float AvoidZoltBubbleHealth { get; set; }

        [DataMember(IsRequired = false)]
        [DefaultValue(0.7f)]
        public float AvoidZoltTwisterHealth { get; set; }

        [DataMember(IsRequired = false)]
        [DefaultValue(false)]
        public bool GraveInjustice
        { get; set; }
        #endregion Properties

        #region Methods
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

        /// <summary>
        /// Called when property changed.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion Methods
    }
}