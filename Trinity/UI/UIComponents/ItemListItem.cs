using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using JetBrains.Annotations;
using Microsoft.Win32.SafeHandles;
using Trinity.Items;
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
    public class ItemListItem : Item, INotifyPropertyChanged, ICloneable
    {
        private bool _isSelected;
        private readonly Item _item;
        private ObservableCollection<ItemRule> _rules = new ObservableCollection<ItemRule>();

        public ItemListItem(Item item)
        {
            LoadCommands();

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
            return new ItemListItem(_item)
            {
                IsSelected = IsSelected,
            };
        }

        [DataMember]
        public ObservableCollection<ItemRule> Rules
        {
            get { return _rules; }
            set 
            {
                if (_rules != value)
                {
                    _rules = value;
                    OnPropertyChanged();
                }
            }
        }

        #region Commands

        public ICommand AddRuleCommand { get; set; }
        public ICommand RemoveRuleCommand { get; set; }

        public void LoadCommands()
        {
            AddRuleCommand = new RelayCommand(parameter =>
            {
                Logger.Log("Selected {0}", parameter.ToString());

                var item = parameter as ComboBoxItem;
                var selectedPropertyName = item != null ? item.Content.ToString() : parameter.ToString();

                ItemProperty property;
                if (Enum.TryParse(selectedPropertyName, out property) && property != ItemProperty.Unknown && !Rules.Any(r => r.ItemProperty == property))
                {
                    Rules.Add(new ItemRule
                    {
                        ItemPropertyId = (int)property,
                        Name = property.ToString()                        
                    });
                }                
            });

            RemoveRuleCommand = new RelayCommand(parameter =>
            {
                if (parameter == null)
                    return;

                Logger.Log("1) Clicked Remove Rule Button: {0}", parameter.ToString());

                ItemProperty property;
                if (Enum.TryParse(parameter.ToString(), out property))
                {
                    var rule = Rules.FirstOrDefault(r => r.ItemProperty == property);
                    if (rule != null)
                    {
                        Rules.Remove(rule);
                    }
                }                
            });
        }

        #endregion
    }


}
