using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using JetBrains.Annotations;
using Trinity.Config;
using Trinity.Config.Loot;
using Trinity.Helpers;
using Trinity.Objects;
using Trinity.Reference;
using Zeta.Game;

namespace Trinity.Settings.Loot
{
    [DataContract]
    public class ItemRankSettings : ITrinitySetting<ItemSetting>, INotifyPropertyChanged
    {
        [NotNull]
        public string CurrentItemsList
        {
            get
            {
                if (Trinity.Settings.Loot.ItemFilterMode != ItemFilterMode.ItemRanks)
                    return "Item ranking is currently disabled.";

                StringBuilder sb = new StringBuilder();
                if (ItemRankMode == ItemRankMode.AnyClass)
                {
                    List<ItemRank> ird = new List<ItemRank>();
                    foreach (ActorClass actor in Enum.GetValues(typeof(ActorClass)).Cast<ActorClass>())
                    {
                        if (actor == ActorClass.Invalid)
                            continue;
                        foreach (ItemRank itemRank in ItemRanks.GetRankedItems(actor, _minimumPercent, _minimumSampleSize, _minimumRank))
                        {
                            ird.Add(itemRank);
                        }
                    }
                    foreach (var itemRank in ird.OrderByDescending(i => i.Item.BaseType).ThenBy(i => i.Item.ItemType).ThenBy(i => i.SoftcoreRank.FirstOrDefault().PercentUsed))
                    {
                        sb.AppendLine(string.Format("{0}/{1} - {2} - pct={3} rank={4} ss={5}", itemRank.Item.BaseType, itemRank.Item.ItemType, itemRank.Item.Name,
                            itemRank.SoftcoreRank.FirstOrDefault().PercentUsed,
                            itemRank.SoftcoreRank.FirstOrDefault().Rank,
                            itemRank.SoftcoreRank.FirstOrDefault().SampleSize));
                    }
                }
                else if (ZetaDia.Me.IsFullyValid() && ZetaDia.Me.ActorClass != ActorClass.Invalid)
                {
                    foreach (ItemRank itemRank in ItemRanks.GetRankedItems(ZetaDia.Me.ActorClass, _minimumPercent, _minimumSampleSize, _minimumRank))
                    {
                        sb.AppendLine(string.Format("{0}/{1} - {2} - pct={3} rank={4} ss={5}", itemRank.Item.BaseType, itemRank.Item.ItemType, itemRank.Item.Name,
                            itemRank.SoftcoreRank.FirstOrDefault().PercentUsed,
                            itemRank.SoftcoreRank.FirstOrDefault().Rank,
                            itemRank.SoftcoreRank.FirstOrDefault().SampleSize));
                    }
                }
                else
                {
                    sb.AppendLine("Could not read Hero Class.");
                }
                return sb.ToString();
            }
        }

        #region Fields
        private ItemRankMode _itemRankMode;
        private double _minimumPercent;
        private int _minimumSampleSize;
        private int _minimumRank;
        #endregion Fields

        #region Properties
        [DataMember(IsRequired = false)]
        [DefaultValue(ItemRankMode.HeroOnly)]
        public ItemRankMode ItemRankMode
        {
            get
            {
                return _itemRankMode;
            }
            set
            {
                if (_itemRankMode != value)
                {
                    _itemRankMode = value;
                    OnPropertyChanged("ItemRankMode");
                    OnPropertyChanged("CurrentItemsList");
                }
            }
        }
        [DataMember(IsRequired = false)]
        [DefaultValue(10)]
        public double MinimumPercent
        {
            get
            {
                return _minimumPercent;
            }
            set
            {
                if (_minimumPercent != value)
                {
                    _minimumPercent = value;
                    OnPropertyChanged("MinimumPercent");
                    OnPropertyChanged("CurrentItemsList");
                }
            }
        }
        [DataMember(IsRequired = false)]
        [DefaultValue(10)]
        public int MinimumSampleSize
        {
            get
            {
                return _minimumSampleSize;
            }
            set
            {
                if (_minimumSampleSize != value)
                {
                    _minimumSampleSize = value;
                    OnPropertyChanged("MinimumSampleSize");
                    OnPropertyChanged("CurrentItemsList");
                }
            }
        }
        [DataMember(IsRequired = false)]
        [DefaultValue(10)]
        public int MinimumRank
        {
            get
            {
                return _minimumRank;
            }
            set
            {
                if (_minimumRank != value)
                {
                    _minimumRank = value;
                    OnPropertyChanged("MinimumRank");
                    OnPropertyChanged("CurrentItemsList");
                }
            }
        }
        #endregion

        #region Events
        /// <summary>
        /// Occurs when property changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion Events

        #region Methods
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
            _itemRankMode = ItemRankMode.HeroOnly;
            _minimumPercent = 10;
            _minimumRank = 10;
            _minimumSampleSize = 10;
        }

        #endregion Methods

    }
}
