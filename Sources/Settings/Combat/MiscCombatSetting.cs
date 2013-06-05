using System.ComponentModel;
using System.Runtime.Serialization;

namespace Trinity.Config.Combat
{
    [DataContract(Namespace = "")]
    public class MiscCombatSetting : ITrinitySetting<MiscCombatSetting>, INotifyPropertyChanged
    {
        #region Fields
        private GoblinPriority _GoblinPriority;
        private int _NonEliteRange;
        private int _EliteRange;
        private bool _ExtendedTrashKill;
        private bool _AvoidAOE;
        private bool _KillMonstersInAoE;
        private bool _CollectHealthGlobe;
        private bool _AllowOOCMovement;
        private bool _AllowBacktracking;
        private int _DelayAfterKill;
        private bool _UseNavMeshTargeting;
        private int _TrashPackSize;
        private float _TrashPackClusterRadius;
        private bool _IgnoreElites;
        private bool _AvoidDeath;
        #endregion Fields

        #region Events
        /// <summary>
        /// Occurs when property changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion Events

        #region Constructors
        public MiscCombatSetting()
        {
            Reset();
        }
        #endregion Constructors

        #region Properties

        [DataMember(IsRequired = false)]
        [DefaultValue(2)]
        public int TrashPackSize
        {
            get
            {
                return _TrashPackSize;
            }
            set
            {
                if (_TrashPackSize != value)
                {
                    _TrashPackSize = value;
                    OnPropertyChanged("TrashPackSize");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(40f)]
        public float TrashPackClusterRadius
        {
            get
            {
                return _TrashPackClusterRadius;
            }
            set
            {
                if (_TrashPackClusterRadius != value)
                {
                    _TrashPackClusterRadius = value;
                    OnPropertyChanged("TrashPackClusterRadius");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(true)]
        public bool UseNavMeshTargeting
        {
            get
            {
                return _UseNavMeshTargeting;
            }
            set
            {
                if (_UseNavMeshTargeting != value)
                {
                    _UseNavMeshTargeting = value;
                    OnPropertyChanged("UseNavMeshTargeting");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(GoblinPriority.Prioritize)]
        public GoblinPriority GoblinPriority
        {
            get
            {
                return _GoblinPriority;
            }
            set
            {
                if (_GoblinPriority != value)
                {
                    _GoblinPriority = value;
                    OnPropertyChanged("GoblinPriority");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(40)]
        public int NonEliteRange
        {
            get
            {
                return _NonEliteRange;
            }
            set
            {
                if (_NonEliteRange != value)
                {
                    _NonEliteRange = value;
                    OnPropertyChanged("NonEliteRange");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(150)]
        public int EliteRange
        {
            get
            {
                return _EliteRange;
            }
            set
            {
                if (_EliteRange != value)
                {
                    _EliteRange = value;
                    OnPropertyChanged("EliteRange");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(true)]
        public bool ExtendedTrashKill
        {
            get
            {
                return _ExtendedTrashKill;
            }
            set
            {
                if (_ExtendedTrashKill != value)
                {
                    _ExtendedTrashKill = value;
                    OnPropertyChanged("ExtendedTrashKill");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(true)]
        public bool AvoidAOE
        {
            get
            {
                return _AvoidAOE;
            }
            set
            {
                if (_AvoidAOE != value)
                {
                    _AvoidAOE = value;
                    OnPropertyChanged("AvoidAOE");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(true)]
        public bool KillMonstersInAoE
        {
            get
            {
                return _KillMonstersInAoE;
            }
            set
            {
                if (_KillMonstersInAoE != value)
                {
                    _KillMonstersInAoE = value;
                    OnPropertyChanged("KillMonstersInAoE");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(true)]
        public bool CollectHealthGlobe
        {
            get
            {
                return _CollectHealthGlobe;
            }
            set
            {
                if (_CollectHealthGlobe != value)
                {
                    _CollectHealthGlobe = value;
                    OnPropertyChanged("CollectHealthGlobe");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(true)]
        public bool AllowOOCMovement
        {
            get
            {
                return _AllowOOCMovement;
            }
            set
            {
                if (_AllowOOCMovement != value)
                {
                    _AllowOOCMovement = value;
                    OnPropertyChanged("AllowOOCMovement");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(false)]
        public bool AllowBacktracking
        {
            get
            {
                return _AllowBacktracking;
            }
            set
            {
                if (_AllowBacktracking != value)
                {
                    _AllowBacktracking = value;
                    OnPropertyChanged("AllowBacktracking");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(0)]
        public int DelayAfterKill
        {
            get
            {
                return _DelayAfterKill;
            }
            set
            {
                if (_DelayAfterKill != value)
                {
                    _DelayAfterKill = value;
                    OnPropertyChanged("DelayAfterKill");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(false)]
        public bool IgnoreElites
        {
            get
            {
                return _IgnoreElites;
            }
            set
            {
                if (_IgnoreElites != value)
                {
                    _IgnoreElites = value;
                    OnPropertyChanged("IgnoreElites");
                }
            }
        }
        [DataMember(IsRequired = false)]
        [DefaultValue(false)]
        public bool AvoidDeath
        {
            get
            {
                return _AvoidDeath;
            }
            set
            {
                if (_AvoidDeath != value)
                {
                    _AvoidDeath = value;
                    OnPropertyChanged("AvoidDeath");
                }
            }
        }
        #endregion Properties

        #region Methods
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

        /// <summary>
        /// This will set default values for new settings if they were not present in the serialized XML (otherwise they will be the type defaults)
        /// </summary>
        /// <param name="context"></param>
        [OnDeserializing()]
        internal void OnDeserializingMethod(StreamingContext context)
        {
            this.UseNavMeshTargeting = true;
            this.TrashPackClusterRadius = 40f;
            this.TrashPackSize = 2;
            this.KillMonstersInAoE = true;
            this.EliteRange = 150;
        }
        #endregion Methods
    }
}
