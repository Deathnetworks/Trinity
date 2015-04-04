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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;
using JetBrains.Annotations;
using Microsoft.Win32.SafeHandles;
using Trinity.Items;
using Trinity.Objects;
using Trinity.Reference;
using Trinity.UIComponents;
using Zeta.Common;
using Trinity.Technicals;
using Logger = Trinity.Technicals.Logger;

namespace Trinity.UI.UIComponents
{
    /// <summary>
    /// Item Object with wrapped for use in SettingsUI
    /// </summary>
    [DataContract(Namespace = "")]
    public class LItem : Item, INotifyPropertyChanged, ICloneable
    {
        private bool _isSelected;
        private readonly Item _item;
        private List<ItemProperty> _itemProperties;
        private ObservableCollection<LRule> _rules = new ObservableCollection<LRule>();

        public LItem(Item item)
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
            IconUrl = item.IconUrl;
            IsTwoHanded = item.IsTwoHanded;
            GItemType = item.GItemType;
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
            get { return _isSelected; }
            set
            {                
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged();                    
                }
            }
        }

        public List<ItemProperty> ItemProperties
        {
            get { return _itemProperties ?? (_itemProperties = ItemDataUtils.GetPropertiesForItem(_item)); }
            set
            {
                if (_itemProperties != value)
                {
                    _itemProperties = value;
                    OnPropertyChanged();
                }
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
            return new LItem(_item)
            {
                IsSelected = IsSelected,
            };
        }


        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public ObservableCollection<LRule> Rules
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

        public ItemStatRange GetItemStatRange(ItemProperty property)
        {
            return ItemDataUtils.GetItemStatRange(_item, property);
        }

        #region Commands

        public ICommand AddRuleCommand { get; set; }
        public ICommand RemoveRuleCommand { get; set; }

        public void LoadCommands()
        {
            AddRuleCommand = new RelayCommand(parameter =>
            {
                if (parameter == null)
                    return;

                Logger.Log("AddRuleCommand {0}", parameter.ToString());

                var item = parameter as ComboBoxItem;
                var selectedPropertyName = item != null ? item.Content.ToString() : parameter.ToString();

                ItemProperty property;
                if (Enum.TryParse(selectedPropertyName, out property) && property != ItemProperty.Unknown && !Rules.Any(r => r.ItemProperty == property))
                {
                    var statRange = GetItemStatRange(property);
                    if (statRange != null)
                    {
                        Logger.LogVerbose(string.Format("Stats Min = {0} Max = {1} Step = {2}", 
                            statRange.AbsMin.ToString(), statRange.AbsMax.ToString(), statRange.AbsStep.ToString()));
                    }
                    
                    Rules.Add(new LRule
                    {
                        Id = (int)property,
                        ItemStatRange = statRange,
                        GItemType = GItemType
                    });
                }                
            });

            RemoveRuleCommand = new RelayCommand(parameter =>
            {
                if (parameter == null)
                    return;

                Logger.Log("RemoveRuleCommand: {0}", parameter.ToString());

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
