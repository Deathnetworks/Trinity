﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
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
using Trinity;
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
        private int _ops;

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
            TrinityItemType = item.TrinityItemType;
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

        /// <summary>
        /// Properties that are valid for this particular item
        /// </summary>
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

        /// <summary>
        /// The number of optional rules that must be True
        /// </summary>
        [DataMember]
        public int Ops
        {
            get
            {
                if (_ops == 0)
                    _ops = 1;

                return _ops;
            }
            set
            {
                if (_ops != value)
                {
                    _ops = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<int> OpCountNumbers
        {
            get { return new ObservableCollection<int>(new List<int> {1, 2, 3, 4}); }
        }

        public void Reset()
        {
            IsSelected = false;
        }



        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            //Logger.Log("Property Changed {0}", propertyName);
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public object Clone()
        {
            return new LItem(_item)
            {
                IsSelected = IsSelected,
                //OpCount = OpCount
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
                    OnPropertyChanged("OptionalRules");
                    OnPropertyChanged("RequiredRules");
                }
            }
        }

        public ObservableCollection<LRule> RequiredRules 
        {
            get { return new ObservableCollection<LRule>(Rules.Where(r => r.RuleType == RuleType.Required)); }
        }

        public ObservableCollection<LRule> OptionalRules
        {
            get { return new ObservableCollection<LRule>(Rules.Where(r => r.RuleType == RuleType.Optional)); }
        }

        public ItemStatRange GetItemStatRange(ItemProperty property)
        {
            return ItemDataUtils.GetItemStatRange(_item, property);
        }

        #region Commands

        public ICommand AddRequiredRuleCommand { get; set; }
        public ICommand AddOptionalRuleCommand { get; set; }
        public ICommand RemoveRuleCommand { get; set; }

        public void LoadCommands()
        {
            AddRequiredRuleCommand = new RelayCommand(parameter =>
            {
                try
                {
                    if (parameter == null)
                    {                        
                        Logger.Log("Parameter = null in AddRequiredRuleCommand {0}", parameter.ToString());
                        return;
                    }
                        

                    Logger.Log("AddOptionalRuleCommand {0}", parameter.ToString());

                    var item = parameter as ComboBoxItem;
                    var selectedPropertyName = item != null ? item.Content.ToString() : parameter.ToString();

                    AddRule(selectedPropertyName, RuleType.Required);
                }
                catch (Exception ex)
                {
                    Logger.Log("Exception in AddRequiredRuleCommand: {0} {1}", ex.Message, ex.InnerException);
                }

            });

            AddOptionalRuleCommand = new RelayCommand(parameter =>
            {
                try
                {
                    if (parameter == null)
                    {
                        Logger.Log("Parameter = null in AddRequiredRuleCommand {0}", parameter.ToString());
                        return;
                    }

                    Logger.Log("AddOptionalRuleCommand {0}", parameter.ToString());

                    var item = parameter as ComboBoxItem;
                    var selectedPropertyName = item != null ? item.Content.ToString() : parameter.ToString();

                    AddRule(selectedPropertyName, RuleType.Optional);
                }
                catch (Exception ex)
                {
                    Logger.Log("Exception in AddRequiredRuleCommand: {0} {1}", ex.Message, ex.InnerException);
                }

            });

            RemoveRuleCommand = new RelayCommand(parameter =>
            {
                try
                {
                    var lRule = parameter as LRule;
                    if (lRule == null)
                        return;

                    Logger.Log("RemoveRuleCommand: {0}", lRule.Name);

                    Rules.Remove(lRule);

                    OnPropertyChanged("Rules");
                    OnPropertyChanged( lRule.RuleType + "Rules");
                }
                catch (Exception ex)
                {
                    Logger.Log("Exception in AddRequiredRuleCommand: {0} {1}", ex.Message, ex.InnerException);
                }
            });
        }

        #endregion

        #region Methods

        public void AddRule(string propertyName, RuleType ruleType)
        {
            var propertiesWithDuplicatesAllowed = new HashSet<ItemProperty>
            {
                ItemProperty.ElementalDamage,
                ItemProperty.SkillDamage
            };

            Func<ItemProperty, bool> allowedToAdd = p => Rules.All(r => r.ItemProperty != p) || propertiesWithDuplicatesAllowed.Contains(p);

            ItemProperty property;

            Logger.Log("Attempting to Add {0} Type={1}", propertyName, ruleType);

            if (Enum.TryParse(propertyName, out property) && property != ItemProperty.Unknown && allowedToAdd(property))
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
                    TrinityItemType = TrinityItemType,
                    RuleType = ruleType,
                    Value = LRule.GetDefaultValue(property)
                });
                
                OnPropertyChanged("Rules");
                OnPropertyChanged(ruleType + "Rules");
            }      
      

        }

        #endregion
    }


}
