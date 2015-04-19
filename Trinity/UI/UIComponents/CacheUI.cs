using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Trinity.Cache;
using Trinity.Helpers;
using Trinity.Technicals;
using Zeta.Game;
using Zeta.Game.Internals.Actors;

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
                    Height = 300,
                    Width = 1200,
                    MinHeight = MinimumHeight,
                    MinWidth = MininumWidth,
                    Title = "Trinity Cache",
                    Content = UILoader.LoadAndTransformXamlFile<UserControl>(Path.Combine(FileManager.PluginPath, "UI", "CacheUI.xaml")),
                    DataContext = DataModel
                };

                _isWindowOpen = true;
                _window.Closed += Window_Closed;
                Configuration.Events.OnCacheUpdated += Update;
                _window.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            }
            catch (Exception ex)
            {
                Logger.LogNormal("Unable to open Trinity CacheUI: {0}", ex);
            }

            return _window;
        }

        private static bool _isUpdating;
        private static readonly Stopwatch LastUpdatedStopwatch = new Stopwatch();
        private static void Update()
        {
            try
            {
                if (!_isWindowOpen)
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

                DataModel.Cache = new ObservableCollection<CacheUIObject>(GetCacheActorList());

                _isUpdating = false;
            }
            catch (Exception ex)
            {
                _isUpdating = false;
                Logger.LogError("Error in CacheUI Worker: " + ex);
            }
        }

        public static System.Collections.Generic.List<CacheUIObject> GetCacheActorList()
        {
            return ZetaDia.Actors.GetActorsOfType<DiaObject>(true)
                                .Where(i => i.IsFullyValid())
                                .Select(o => new CacheUIObject(o))
                                .OrderByDescending(o => o.InCache)
                                .ThenBy(o => o.Distance)
                                .ToList();
        }
        private static bool _isWindowOpen;
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
