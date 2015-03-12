using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Trinity.Config;
using Trinity.Reference;
using Trinity.UI;
using Trinity.UI.UIComponents;
using Trinity.UIComponents;
using Zeta.Common;
using Logger = Trinity.Technicals.Logger;

namespace Trinity.Settings.Loot
{
    [DataContract(Namespace = "")]
    public class ItemListSettings : ITrinitySetting<ItemListSettings>, INotifyPropertyChanged
    {
        #region Fields

        private static List<SettingsItem> _cachedItems;
        private FullyObservableCollection<SettingsItem> _displayItems = new FullyObservableCollection<SettingsItem>();
        private List<SettingsItem> _selectedItems = new List<SettingsItem>();
        private GroupingType _grouping;
        private string _filterText;
        private DeferredAction _deferredAction;
        private CollectionViewSource _collection;

        #endregion

        #region Constructors

        public ItemListSettings()
        {        
            CacheReferenceItems();                   
            Reset();
        }

        #endregion

        #region Enums

        public enum GroupingType
        {
            None,
            BaseType,
            ItemType,
            SetName,
            IsSetItem,
            IsCrafted,
            IsValid
        }

        #endregion

        #region Properties

        /// <summary>
        /// The CollectionView runs on top of the DisplayItems and adds additional functionality for grouping, sorting and filtering.
        /// </summary>
        public CollectionViewSource Collection
        {
            get
            {

                return _collection;
            }
            set
            {
                if (_collection != value)
                {
                    _collection = value;
                    OnPropertyChanged("Collection");
                }
            }
        }

        /// <summary>
        /// Current grouping
        /// </summary>
        [DataMember]
        public GroupingType Grouping
        {
            get { return _grouping; }
            set
            {
                if (_grouping != value)
                {
                    _grouping = value;
                    OnPropertyChanged("Grouping");
                    ChangeGrouping(value);
                }
            }
        }

        /// <summary>
        /// Filtering text
        /// </summary>
        [IgnoreDataMember]
        public string FilterText
        {
            get { return _filterText; }
            set
            {
                if (_filterText != value)
                {
                    _filterText = value;
                    OnPropertyChanged("FilterText");
                    OnPropertyChanged("IsFiltered");
                    ChangeFilterPending(value);
                }
            }
        }

        /// <summary>
        /// If the view is currently filtered
        /// </summary>
        [IgnoreDataMember]
        public bool IsFiltered
        {
            get { return !string.IsNullOrEmpty(FilterText); }
        }

        /// <summary>
        /// Main collection for all items, underlies CollectionViewSource
        /// </summary>
        [IgnoreDataMember]
        public FullyObservableCollection<SettingsItem> DisplayItems
        {
            get
            {
                return _displayItems;
            }
            set
            {
                if (_displayItems != value)
                {
                    _displayItems = value;
                    OnPropertyChanged("DisplayItems");
                }
            }
        }

        /// <summary>
        /// The source of truth - currently selected items, this is persisted to the settings file.
        /// </summary>
        [DataMember(IsRequired = false)]
        public List<SettingsItem> SelectedItems
        {
            get
            {
                return _selectedItems;
            }
            set
            {
                if (_selectedItems != value)
                {
                    if (_selectedItems == null || value != null)
                    {
                        _selectedItems = value;
                        OnPropertyChanged("SelectedItems");
                    }
                }
            }
        }

        /// <summary>
        /// Whether the groupings are automatically expanded
        /// </summary>
        public bool GroupsExpandedByDefault { get; set; }

        #endregion

        #region Commands

        public ICommand ResetFilterCommand { get; set; }

        public void LoadCommands()
        {
            ResetFilterCommand = new RelayCommand(parameter =>
            {
                FilterText = string.Empty;
            });
        }

        #endregion

        #region Methods

        /// <summary>
        /// Setup work called on Construction / Reset
        /// </summary>
        private void Initialization()
        {
            CacheReferenceItems();
            DisplayItems = new FullyObservableCollection<SettingsItem>(_cachedItems, true);            
            BindEvents();
            LoadCommands();
            GroupsExpandedByDefault = false;
        }

        /// <summary>
        /// Selected settings is always available for loot rule processing etc via ItemList.Selected property.
        /// But we only care about the UI Control being properly populated if the settings window is open.
        /// </summary>
        private void SettingsWindowOpened()
        {            
            CreateView();
            UpdateSelectedItems();
        }

        /// <summary>
        /// Configure the CollectionViewSource
        /// </summary>
        public void CreateView()
        {
            Collection = new CollectionViewSource();
            Collection.Source = DisplayItems;
            ChangeGrouping(Grouping);
            Collection.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
            Collection.View.Refresh();
        }

        /// <summary>
        /// Convert Legendary items to SettingsItem objects only once to a static collection.
        /// </summary>
        public static void CacheReferenceItems()
        {
            if (_cachedItems == null)
                _cachedItems = Legendary.ToList().Select(i => new SettingsItem(i)).ToList();
        }

        /// <summary>
        /// Wire up for events
        /// </summary>
        public void BindEvents()
        {
            UILoader.OnSettingsWindowOpened -= SettingsWindowOpened;
            UILoader.OnSettingsWindowOpened += SettingsWindowOpened;

            DisplayItems.ChildElementPropertyChanged -= SyncSelectedItem;
            DisplayItems.ChildElementPropertyChanged += SyncSelectedItem;

            TrinitySetting.OnUserRequestedReset -= OnUserRequestedReset;
            TrinitySetting.OnUserRequestedReset += OnUserRequestedReset;
        }

        /// <summary>
        /// Reset is called many times for many reasons, we need to only reset selected 
        /// when the user has clicked the reset button in the settings window
        /// because running UpdateSelectedItems() 4-5 times during load is costly.
        /// </summary>
        private void OnUserRequestedReset()
        {
            SelectedItems.Clear();
            CreateView();
            UpdateSelectedItems();
        }

        /// <summary>
        /// Change the grouping order
        /// </summary>
        /// <param name="groupingType"></param>
        internal void ChangeGrouping(GroupingType groupingType)
        {
            if (Collection == null)
                return;

            // Prevent the collection from updating until outside of the using block.
            using (Collection.DeferRefresh())
            {
                Collection.GroupDescriptions.Clear();
                if (groupingType != GroupingType.None)
                    Collection.GroupDescriptions.Add(new PropertyGroupDescription(groupingType.ToString()));
            }
        }

        /// <summary>
        /// Change the search filter when the user stops typing
        /// </summary>
        /// <param name="property"></param>
        internal void ChangeFilterPending(string property)
        {
            if (_deferredAction == null)
                _deferredAction = DeferredAction.Create(ExecuteFilter);

            _deferredAction.Defer(TimeSpan.FromMilliseconds(250));
        }

        /// <summary>
        /// Rebind the filter to get it to fire
        /// </summary>
        private void ExecuteFilter()
        {
            Collection.Filter -= FilterHandler;
            Collection.Filter += FilterHandler;                
        }

        /// <summary>
        /// Expression that is run against every item in the collection to filter
        /// </summary>
        private void FilterHandler(object sender, FilterEventArgs e)
        {
            if (string.IsNullOrEmpty(FilterText))
                e.Accepted = true;

            var item = e.Item as SettingsItem;

            if (item == null || string.IsNullOrEmpty(item.Name))
                e.Accepted = false;
            else
                e.Accepted = item.Name.ToLowerInvariant().Contains(FilterText.ToLowerInvariant());
        }

        /// <summary>
        /// Updates the selected collection whenever an object is set to selected.
        /// </summary>
        /// <param name="args"></param>
        public void SyncSelectedItem(ChildElementPropertyChangedEventArgs args)
        {
            var item = args.ChildElement as SettingsItem;
            if (item != null && args.PropertyName.ToString() == "IsSelected")
            {
                var match = _selectedItems.FirstOrDefault(i => i.Id == item.Id);
                if (match != null && !item.IsSelected)
                {
                    _selectedItems.Remove(match);
                    Logger.LogVerbose("Removed {0} ({2}) from Selected Items, NewSelectedCount={1}", item.Name, _selectedItems.Count, item.Id);
                }
                else if (match == null && item.IsSelected)
                {
                    _selectedItems.Add(item);
                    Logger.LogVerbose("Added {0} ({2}) to Selected Items, NewSelectedCount={1}", item.Name, _selectedItems.Count, item.Id);
                }
            }
        }

        /// <summary>
        /// Updates the DisplayItems collection to match the Selected collection
        /// </summary>
        public void UpdateSelectedItems()
        {
            if (_selectedItems == null || _displayItems == null || _collection == null || _collection.View == null || _collection.View.SourceCollection == null)
                return;

            var selectedIds = new HashSet<int>(_selectedItems.Select(i => i.Id));
            var castView = _collection.View.SourceCollection.Cast<SettingsItem>();

            // Prevent the collection from updating until outside of the using block.
            using (Collection.DeferRefresh())
            {
                castView.ForEach(item =>
                {
                    if (selectedIds.Contains(item.Id))
                        item.IsSelected = true;
                });
            }
        }

        #endregion

        #region ITrinitySetting

        public void Reset()
        {
            TrinitySetting.Reset(this);
            Initialization();
        }

        public void CopyTo(ItemListSettings setting)
        {
            TrinitySetting.CopyTo(this, setting);
            setting.SelectedItems = SelectedItems;
        }

        public ItemListSettings Clone()
        {
            return TrinitySetting.Clone(this);
        }

        [OnDeserializing()]
        internal void OnDeserializingMethod(StreamingContext context)
        {

        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

    }
}
