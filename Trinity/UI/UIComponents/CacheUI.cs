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
                DataModel.WeightUpdateTime.Clear();

                DataModel.PropertyChanged += DataModelOnPropertyChanged;

                DataModel.LaunchRadarUICommand = new RelayCommand(param =>
                {
                    DataModel.IsLazyCacheVisible = false;
                    DataModel.LazyCache.Clear();

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        CreateRadarWindow().Show();
                    });

                    DataModel.IsLazyCacheVisible = true;
                });
                

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

        private static Window CreateRadarWindow()
        {
            try
            {
                Logger.Log("Creating Radar Window");

                if (DataModel == null)
                    DataModel = new CacheUIDataModel();

                _radarWindow = new Window
                {
                    Height = 700,
                    Width = 700,
                    MinHeight = MinimumHeight,
                    MinWidth = MininumWidth,
                    Title = "Trinity Radar",
                    Content = UILoader.LoadAndTransformXamlFile<UserControl>(Path.Combine(FileManager.PluginPath, "UI", "RadarUI.xaml")),
                    DataContext = DataModel
                };

                _isRadarWindowOpen = true;
                _radarWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                _window.Closed += Window_Closed;
            }
            catch (Exception ex)
            {
                Logger.LogNormal("Unable to open Trinity RadarUI: {0}", ex);
            }

            return _radarWindow;
        }

        private static void DataModelOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            var dataModel = sender as CacheUIDataModel;
            if (dataModel == null)
                return;

            if (propertyChangedEventArgs.PropertyName == "Enabled")
            {
                FreezeLazyCache();
            }
        }

        /// <summary>
        /// Freeze records to return cached data only; useful when bot is not running or records are being stored
        /// for longer than pulse/framelock, and the object no longer has valid references to memory.
        /// </summary>
        private static void FreezeLazyCache()
        {
            if (DataModel.Enabled)
                DataModel.LazyCache.ForEach(CacheUtilities.UnFreeze);
            else
                DataModel.LazyCache.ForEach(CacheUtilities.Freeze);
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
                        {
                            CacheManager.Stop();
                        }
                        return true;
                    }

                    using (new MemoryHelper())
                    {
                        try
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
                        catch (Exception ex)
                        {
                            Logger.Log("Exception in LazyCacheUpdate thread. {0} {1}", ex.Message, ex.InnerException);
                            throw;
                        }

                    }

                    return false;

                }, 50);
               
            }
        }

        private static bool _isUpdating;
        private static int _updateCount;

        private static DateTime _lastUpdatedDefault = DateTime.MinValue;
        private static DateTime _lastUpdatedLazy = DateTime.MinValue;

        private static void Update()
        {
            try
            {
                if (!_isWindowOpen || !DataModel.Enabled || _isUpdating)
                    return;

                if (DataModel.IsDefaultVisible && DateTime.UtcNow.Subtract(_lastUpdatedDefault).TotalMilliseconds > 250)
                {
                    _isUpdating = true;
                    _updateCount++;

                    DataModel.Cache = new ObservableCollection<CacheUIObject>(GetCacheActorList());
                    _lastUpdatedDefault = DateTime.UtcNow;
                }

                if (DataModel.IsLazyCacheVisible && DateTime.UtcNow.Subtract(_lastUpdatedLazy).TotalMilliseconds > 50)
                {
                    _isUpdating = true;
                    _updateCount++;

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

                        DataModel.LazyCache = new ObservableCollection<TrinityObject>(GetLazyCacheActorList());

                    });

                    _lastUpdatedLazy = DateTime.UtcNow;
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
        private static bool _isRadarWindowOpen;
        private static Window _radarWindow;

        private static void Window_Closed(object sender, EventArgs e)
        {
            try
            {
                _isWindowOpen = false;
                Configuration.Events.OnCacheUpdated -= Update;
                DataModel.PropertyChanged -= DataModelOnPropertyChanged;
                _window = null;
            }
            catch (Exception ex)
            {
                Logger.LogNormal("Exception in Window_Closed: {0}", ex.ToString());

            }
        }
    }
}
