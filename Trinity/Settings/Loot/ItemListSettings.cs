using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Xml;
using System.Xml.Serialization;
using Trinity.Config;
using Trinity.Reference;
using Trinity.UI;
using Trinity.UI.UIComponents;
using Trinity.UIComponents;
using Zeta.Common;
using Logger = Trinity.Technicals.Logger;
using System.IO.Compression;
using System.Text;
using System.IO;
using Org.BouncyCastle.Ocsp;
using Trinity.Helpers;

namespace Trinity.Settings.Loot
{
    /// <summary>
    /// Settings for ItemList looting
    /// </summary>
    [DataContract(Namespace = "")]
    public class ItemListSettings : ITrinitySetting<ItemListSettings>, INotifyPropertyChanged
    {
        #region Fields

        private static List<LItem> _cachedItems;
        private static Dictionary<int,LItem> _cachedItemsDictionary;
        private FullyObservableCollection<LItem> _displayItems = new FullyObservableCollection<LItem>();
        private List<LItem> _selectedItems = new List<LItem>();
        private GroupingType _grouping;
        private string _filterText;
        private DeferredAction _deferredAction;
        private CollectionViewSource _collection = new CollectionViewSource();
        private string _exportCode;
        private string _validationMessage;
        private ModalPage _selectedModalPage;
        private bool _isModalVisible;
        private Dictionary<int, LItem> _viewPortal;

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
            IsEquipped,
            ActorClass,
            IsSetItem,
            IsCrafted,
            IsValid
        }

        public enum SortingType
        {
            None,
            Name,
            Id
        }

        public enum ModalPage
        {
            None,
            Import,
            Export
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
        /// This is only used for displaying the UI and user interaction.        
        /// </summary>
        [IgnoreDataMember]
        public FullyObservableCollection<LItem> DisplayItems
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
        /// Contains the currently selected items; this is persisted to the settings file.
        /// Code elsewhere in trinity (such as loot engien) can check items against it at any time.
        /// LItems here have a minimal set of data; only Ids of the items and rules are are saved.
        /// </summary>
        [DataMember(IsRequired = false)]
        public List<LItem> SelectedItems
        {
            get
            {                
                return _selectedItems;
            }
            set
            {
                if (_selectedItems != value)
                {
                    _selectedItems = value;
                    OnPropertyChanged("SelectedItems");
                }
            }
        }

        /// <summary>
        /// Whether the groupings are automatically expanded
        /// </summary>
        public bool GroupsExpandedByDefault { get; set; }

        /// <summary>
        /// Compressed/Encoded string of selected items and their rules
        /// </summary>
        public string ExportCode
        {
            get { return _exportCode; }
            set
            {
                if (_exportCode != value)
                {
                    _exportCode = value;
                    OnPropertyChanged("ExportCode");
                }                
            }
        }

        /// <summary>
        /// Message for Import/Export user information
        /// </summary>
        public string ValidationMessage
        {
            get { return _validationMessage; }
            set
            {
                if (_validationMessage != value)
                {
                    _validationMessage = value;
                    OnPropertyChanged("ValidationMessage");
                }
            }
        }

        /// <summary>
        /// Selected panel for the import/export modal
        /// </summary>
        public ModalPage SelectedModalPage
        {
            get { return _selectedModalPage; }
            set
            {
                if (_selectedModalPage != value)
                {
                    _selectedModalPage = value;
                    OnPropertyChanged("SelectedModalPage");
                }
            }
        }

        /// <summary>
        /// Hides/Shows the modal
        /// </summary>
        public bool IsModalVisible
        {
            get { return _isModalVisible; }
            set
            {
                if (_isModalVisible != value)
                {
                    _isModalVisible = value;
                    OnPropertyChanged("IsModalVisible");
                }
            }
        }

        #endregion

        #region Commands

        public ICommand ResetFilterCommand { get; set; }
        public ICommand ExportCommand { get; set; }
        public ICommand ImportCommand { get; set; }
        public ICommand LoadModalCommand { get; set; }
        public ICommand CloseModalCommand { get; set; }

        public void LoadCommands()
        {
            ResetFilterCommand = new RelayCommand(parameter =>
            {
                FilterText = string.Empty;
            });
            
            LoadModalCommand = new RelayCommand(parameter =>
            {
                if (parameter == null)
                    return;

                ModalPage page;
                if (Enum.TryParse(parameter.ToString(),out page))
                {
                    if (page != ModalPage.None)
                    {
                        SelectedModalPage = page;
                        IsModalVisible = true;
                    }

                    ExportCode = string.Empty;

                    if(page == ModalPage.Export)
                        ExportCommand.Execute(parameter);
                }

                Logger.Log("Selecting modal content... {0}", parameter.ToString());
            });

            CloseModalCommand = new RelayCommand(parameter =>
            {
                IsModalVisible = false;
            });

            ImportCommand = new RelayCommand(parameter =>
            {
                Logger.Log("Importing ItemList...");

                var oldSlected = _selectedItems.Count;

                ImportFromCode(ExportCode);

                Logger.Log("Selected Before = {0} After = {1}", oldSlected, _selectedItems.Count);

                IsModalVisible = false;
            });

            ExportCommand = new RelayCommand(parameter =>
            {
                Logger.Log("Exporting ItemList... {0}", parameter);
                ExportCode = CreateExportCode();
            });

        }

        #endregion

        #region Methods

        /// <summary>
        /// Generates an export code of the current state
        /// </summary>        
        public string CreateExportCode()
        {
            var settingsXml = TrinitySetting.GetSettingsXml(this);
            return ExportHelper.Compress(settingsXml);
        }

        /// <summary>
        /// Decodes an export code and applies it to the current state.
        /// </summary>
        public ItemListSettings ImportFromCode(string code)
        {
            if (string.IsNullOrEmpty(code) || string.IsNullOrWhiteSpace(code))
            {
                ValidationMessage = "You must enter an import/export code";
                Logger.Log("You must enter an import/export code");
            }
            try
            {
                var decompressedXml = ExportHelper.Decompress(ExportCode);
                var newSettings = TrinitySetting.GetSettingsInstance<ItemListSettings>(decompressedXml);

                Grouping = newSettings.Grouping;

                using (Collection.DeferRefresh())
                {
                    SelectedItems = newSettings.SelectedItems;
                    UpdateSelectedItems();
                }
            }
            catch (Exception ex)
            {
                ValidationMessage = string.Format("Error importing itemlist. {0} {1}", ex.Message, ex.InnerException);
                Logger.Log("Error importing itemlist. {0} {1}", ex.Message, ex.InnerException);
            }
            return this;
        }

        /// <summary>
        /// Setup work called on Construction / Reset
        /// </summary>
        private void Initialization()
        {
            CacheReferenceItems();
            DisplayItems = new FullyObservableCollection<LItem>(_cachedItems, true);            
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
            ChangeSorting(SortingType.Name);
        }

        /// <summary>
        /// Convert Legendary items to SettingsItem objects only once to a static collection.
        /// </summary>
        public static void CacheReferenceItems()
        {
            if (_cachedItems == null)
                _cachedItems = Legendary.ToList().Where(i => !i.IsCrafted && i.Id != 0).Select(i => new LItem(i)).ToList();
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
        /// Change the grouping property
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
        /// Change the sorting order
        /// </summary>
        /// <param name="sortingType"></param>
        internal void ChangeSorting(SortingType sortingType)
        {
            if (Collection == null)
                return;

            using (Collection.DeferRefresh())
            {
                Collection.SortDescriptions.Clear();
                Collection.SortDescriptions.Add(new SortDescription(sortingType.ToString(), ListSortDirection.Ascending));
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

            var item = e.Item as LItem;

            if (item == null || string.IsNullOrEmpty(item.Name))
            {
                e.Accepted = false;
            }
            else
            {
                e.Accepted = item.Name.ToLowerInvariant().Contains(FilterText.ToLowerInvariant());

                if (!e.Accepted)
                    e.Accepted = item.Id.ToString().Equals(FilterText);
            }
                

        }

        /// <summary>
        /// Updates the selected collection whenever an object is set to selected.
        /// </summary>
        /// <param name="args"></param>
        public void SyncSelectedItem(ChildElementPropertyChangedEventArgs args)
        {
            var item = args.ChildElement as LItem;
            if (item != null && args.PropertyName.ToString() == "IsSelected")
            {
                var match = _selectedItems.FirstOrDefault(i => i.Id == item.Id);
                if (match != null)
                {
                    if (!item.IsSelected)
                    {
                        // Remove
                        _selectedItems.Remove(match);
                        Logger.LogVerbose("Removed {0} ({2}) from Selected Items, NewSelectedCount={1}", item.Name, _selectedItems.Count, item.Id);
                    }
                    else
                    {
                        // Update Data
                    }

                }
                else if (match == null && item.IsSelected)
                {
                    _selectedItems.Add(item);
                    Logger.LogVerbose("Added {0} ({2}) to Selected Items, NewSelectedCount={1}", item.Name, _selectedItems.Count, item.Id);
                }
            }
        }

        /// <summary>
        /// Updates the DisplayItems collection to match the Selected collection.         
        /// </summary>
        public void UpdateSelectedItems()
        {
            if (_selectedItems == null || _displayItems == null || _collection == null || _collection.View == null || _collection.View.SourceCollection == null)
            {
                Logger.Log("Skipping UpdateSelectedItems due to Null");
                return;
            }

            // Prevent the collection from updating until outside of the using block.
            using (Collection.DeferRefresh())
            {
                var selectedDictionary = _selectedItems.DistinctBy(i => i.Id).ToDictionary(k => k.Id, v => v);
                var castView = _collection.View.SourceCollection.Cast<LItem>();

                castView.ForEach(item =>
                {

                    // After XML settings load _selectedItems will contain LItem husks that are lacking useful information, just what was saved.
                    // We want to take the saved information and make an object that is fully populated and linked with the UI collection.

                    LItem selectedItem;

                    if (selectedDictionary.TryGetValue(item.Id, out selectedItem))
                    {
                        if(!item.IsSelected)
                            Logger.LogVerbose("Update: Selecting {0} ({1}) with {2} rules", item.Name, item.Id, item.Rules.Count);
                      
                        selectedItem.Rules.ForEach(r =>
                        {
                            r.GItemType = item.GItemType;
                            r.ItemStatRange = item.GetItemStatRange(r.ItemProperty);
                        });
                        item.IsSelected = true;
                        item.Rules = selectedItem.Rules;
                        item.Ops = selectedItem.Ops;

                        // Replacing the reference to automatically receive changes from UI.
                        _selectedItems.Remove(selectedItem);
                        _selectedItems.Add(item);
                    }
                    else
                    {
                        if (item.IsSelected)
                        {
                            Logger.LogVerbose("Update: Deselecting {0}", item.Name);
                            item.IsSelected = false;
                        }                            
                    }
                });
            }
        }

        
        //public int SelectedTestIds { get; set; }
        //public RuleType RuleType
        //{
        //    get { return (RuleType)TypeId; }
        //    set { TypeId = (int)value; }
        //}
        //[DataMember]
        //public List<LItem> SelectedTestObjects
        //{
        //    get
        //    {
        //        return CollectionViewSource == null ? null : CollectionViewSource.Where(r => r.IsSelected).ToList();
        //    }
        //    set
        //    {
        //        if (value == null)
        //            return;

        //        var selectedDictionary = value.ToDictionary(k => k.Id, v => v);
        //        var newItems = new FullyObservableCollection<LItem>(_cachedItems, true); 
        //        newItems.ForEach(i =>
        //        {
        //            LItem item;
        //            if (selectedDictionary.TryGetValue(i.Id, out item))
        //            {
        //                i.IsSelected = true;
        //                i.Rules = item.Rules;
        //                i.OptionalCount = item.OptionalCount;
        //            }
        //        });

        //        //var cachedItemIndex = 
        //        //_cachedItems.ForEach(c =>
        //        //{
        //        //    if(c == value)
        //        //});
        //        //value.ForEach();
                
        //        //LoadedTestObjects = value;
        //    }
        //}

        ////private List<LItem> LoadedTestObjects;

        /////// <summary>
        /////// Provides fast lookup of the view objects by id
        /////// </summary>
        ////public Dictionary<int, LItem> CollectionViewPortal
        ////{
        ////    get
        ////    {
        ////        if (CollectionViewSource == null)
        ////            return null;

        ////        return _viewPortal ?? (_viewPortal = CollectionViewSource.ToDictionary(k => k.Id, v => v));
        ////    }
        ////}

        /////// <summary>
        /////// Provides access to modify the view source collection
        /////// </summary>
        //public List<LItem> CollectionViewSource
        //{
        //    get
        //    {
        //        if (_collection == null || _collection.View == null || _collection.View.SourceCollection == null)
        //            return null;

        //        return _collection.View.SourceCollection.Cast<LItem>().ToList();
        //    }
        //}

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
