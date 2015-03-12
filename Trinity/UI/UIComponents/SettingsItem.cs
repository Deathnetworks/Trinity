using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Win32.SafeHandles;
using Trinity.Objects;
using Trinity.UIComponents;
using Zeta.Common;
using Zeta.XmlEngine;
using Trinity.Technicals;
using Logger = Trinity.Technicals.Logger;

namespace Trinity.UI.UIComponents
{
    /// <summary>
    /// Item Object with wrapped for use in SettingsUI
    /// </summary>
    [DataContract(Namespace = "")]
    public class SettingsItem : Item, INotifyPropertyChanged, ICloneable
    {
        private bool _isSelected;
        private readonly Item _item;

        public SettingsItem(Item item)
        {
            _item = item;

            Name = item.Name;
            Id = item.Id;
            BaseType = item.BaseType;
            DataUrl = item.DataUrl;
            InternalName = item.InternalName;
            IsCrafted = item.IsCrafted;
            ItemType = item.ItemType;
            LegendaryAffix = item.LegendaryAffix;
            Quality = item.Quality;
            RelativeUrl = item.RelativeUrl;            
            IsCrafted = item.IsCrafted;
            Slug = item.Slug;
            Url = item.Url;

            IsSetItem = item.IsSetItem;
            SetName = item.IsSetItem ? item.Set.Name : "None";
        }

        public new bool IsSetItem { get; set; }

        public string InvalidReason { get; set; }

        public string QualityTypeLabel
        {
            get { return string.Format("{0} {1}{2}", Quality, IsSetItem ? "Set " : string.Empty, ItemType); }
        }

        public bool IsValid
        {
            get
            {
                if (Id == 0)
                {
                    InvalidReason = "Id is 0";
                    return false;
                }

                return true;                  
            }          
        }

        public bool IsSelected
        {
            get
            {
                //Logger.Log("{0} Property: IsSelected get ({1})", Name, _isSelected);
                return _isSelected;                 
            }
            set
            {
                //Logger.Log("{0} Property: IsSelected set to {1}",Name,value);
                _isSelected = value;
                OnPropertyChanged();
            }
        }

        public void Reset()
        {
            IsSelected = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public object Clone()
        {
            return new SettingsItem(_item)
            {
                IsSelected = IsSelected,
            };
        }
    }
}
