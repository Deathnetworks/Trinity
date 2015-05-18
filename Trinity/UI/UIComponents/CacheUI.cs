using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using Trinity.Cache;
using Trinity.Helpers;
using Trinity.LazyCache;
using Trinity.Technicals;
using Trinity.UIComponents;
using Zeta.Bot;
using Zeta.Common;
using Zeta.Game;
using Zeta.Game.Internals;
using Zeta.Game.Internals.Actors;
using Zeta.Game.Internals.SNO;
using Application = System.Windows.Application;
using Logger = Trinity.Technicals.Logger;
using UserControl = System.Windows.Controls.UserControl;

namespace Trinity.UI.UIComponents
{
    public class CacheUI
    {
        private static Window _window;
        internal static CacheUIDataModel DataModel = new CacheUIDataModel();

        private const int MininumWidth = 25;
        private const int MinimumHeight = 25;

        private const int UpdateDelay = 1000;

        public static Window CreateWindow()
        {
            try
            {
                if (DataModel == null)
                    DataModel = new CacheUIDataModel();

                _window = new Window
                {
                    Height = 450,
                    Width = 1900,
                    MinHeight = MinimumHeight,
                    MinWidth = MininumWidth,
                    Title = "Trinity Cache",
                    Content = UILoader.LoadAndTransformXamlFile<UserControl>(Path.Combine(FileManager.PluginPath, "UI", "CacheUI.xaml")),
                    DataContext = DataModel
                };

                DataModel.CacheUpdateTime.Clear();

                DataModel.PropertyChanged += DataModelOnPropertyChanged;

                _isWindowOpen = true;
                _updateCount = 0;
                _window.Closed += Window_Closed;

                Configuration.Events.OnCacheUpdated += Update;
                
                RegisterNotRunningLazyCacheUpdate();

                _window.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            }
            catch (Exception ex)
            {
                Logger.LogNormal("Unable to open Trinity CacheUI: {0}", ex);
            }

            return _window;
        }

        private static void DataModelOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            var dataModel = sender as CacheUIDataModel;
            if (dataModel == null)
                return;
            
            // Freeze records to return cached data only because the collection will get stale and cause exceptions
            // when user tries to do anything with them like Copy to clipboard.
            if (propertyChangedEventArgs.PropertyName == "Enabled")
            {
                if(dataModel.Enabled)
                    dataModel.LazyCache.ForEach(CacheUtilities.UnFreeze);
                else
                    dataModel.LazyCache.ForEach(CacheUtilities.Freeze);
            }
        }

        /// <summary>
        /// Allows the UICache LazyCache Panel to be updated while bot is not started.
        /// </summary>
        private static void RegisterNotRunningLazyCacheUpdate()
        {
            if (!BotMain.IsRunning && !BotMain.IsPausedForStateExecution)
            {

                Logger.Log("Starting CacheUI update thread");
                Worker.Start(() =>
                {
                    if (BotMain.IsRunning || !_isWindowOpen)
                    {
                        Logger.Log("Shutting down CacheUI update thread");

                        if (!_isWindowOpen)
                            CacheManager.Stop();

                        return true;
                    }

                    using (new MemoryHelper())
                    {

                        if (!BotMain.IsPausedForStateExecution && DataModel.IsLazyCacheVisible)
                        {
                            if (ZetaDia.IsInGame && ZetaDia.Me != null)
                            {
                                if (!CacheManager.IsRunning)
                                    CacheManager.Start();

                                CacheManager.Update();
                                CacheUI.Update();                                    
                            }
                        }
                    }

                    return false;

                }, 250); //250ms Ticks
               
            }
        }

        private static bool _isUpdating;
        private static int _updateCount;
        private static readonly Stopwatch LastUpdatedStopwatch = new Stopwatch();
        private static void Update()
        {
            try
            {
                if (!_isWindowOpen)
                    return;

                if (!DataModel.Enabled)
                    return;

                if (!LastUpdatedStopwatch.IsRunning)
                    LastUpdatedStopwatch.Start();
                else if (LastUpdatedStopwatch.ElapsedMilliseconds < 250)
                    return;
                else
                    LastUpdatedStopwatch.Restart();

                if (_isUpdating)
                    return;
                _isUpdating = true;

                _updateCount++;

                if(DataModel.IsDefaultVisible)
                    DataModel.Cache = new ObservableCollection<CacheUIObject>(GetCacheActorList());

                if (DataModel.IsLazyCacheVisible)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        // The initial update is always huge and messes up the charts.
                        if (_updateCount > 1 || CacheManager.LastUpdated == DateTime.MinValue)
                        {
                            DataModel.CacheUpdateTime.Add(new CacheUIDataModel.ChartDatum(CacheManager.LastUpdated, CacheManager.LastUpdateTimeTaken));
                            DataModel.WeightUpdateTime.Add(new CacheUIDataModel.ChartDatum(CacheManager.LastUpdated, CacheManager.LastWeightingTimeTaken));
                        }

                        if (DataModel.CacheUpdateTime.Count > 100)
                            DataModel.CacheUpdateTime.RemoveAt(0);

                        if (DataModel.WeightUpdateTime.Count > 100)
                            DataModel.WeightUpdateTime.RemoveAt(0);
                    });                       

                    DataModel.LazyCache = new ObservableCollection<TrinityObject>(GetLazyCacheActorList());
                }

                _isUpdating = false;
            }
            catch (Exception ex)
            {
                _isUpdating = false;
                Logger.LogError("Error in CacheUI Worker: " + ex);
            }
        }

        public static List<TrinityObject> GetLazyCacheActorList()
        {
            try
            {
                return CacheManager.GetActorsOfType<TrinityObject>()
                    .OrderByDescending(o => o.Weight)
                    .ThenBy(o => o.Distance)
                    .ToList();     
            }
            catch (Exception ex)
            {
                Logger.Log("Exception in GetLazyCacheActorList > GetActorsOfType. {0}", ex.Message, ex.InnerException);
                throw;
            }
          
        }

        public static List<CacheUIObject> GetCacheActorList()
        {
            using (new PerformanceLogger("CacheUI DefaultActorList"))
            {
                return ZetaDia.Actors.GetActorsOfType<DiaObject>(true)
                    .Where(i => i.IsFullyValid())
                    .Select(o => new CacheUIObject(o))
                    .OrderByDescending(o => o.InCache)
                    .ThenByDescending(o => o.Weight)
                    .ThenBy(o => o.Distance)
                    .ToList();
            }
        }

        private static bool _isWindowOpen;

        private static ICommand CopyToClipboardCommand = new RelayCommand(param =>
        {
            Logger.Log("Copy to Clipboard Command Fired {0}", param);
        });

        private static void Window_Closed(object sender, EventArgs e)
        {
            try
            {
                _isWindowOpen = false;
                Configuration.Events.OnCacheUpdated -= Update;
                _window = null;
            }
            catch (Exception ex)
            {
                Logger.LogNormal("Exception in Window_Closed: {0}", ex.ToString());

            }
        }

    }
}
