using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Trinity.Technicals;

namespace Trinity.UI.UIComponents
{
    public class CacheUI
    {
        private static Window _window;
        private static UserControl _userControl;
        internal static CacheUIDataModel DataModel = new CacheUIDataModel();

        private const int MininumWidth = 25;
        private const int MinimumHeight = 25;

        private const int updateDelay = 1000;

        private static Thread _workerThread;

        public static Window CreateWindow()
        {
            try
            {
                var filename = Path.Combine(FileManager.PluginPath, "UI", "CacheUI.xaml");

                if (_window == null)
                {
                    _window = new Window();
                }

                if (_userControl == null)
                    _userControl = UILoader.LoadAndTransformXamlFile<UserControl>(filename);

                //_window.Content = _userControl;
                _window.Height = 150;
                _window.Width = 600;
                _window.MinHeight = MinimumHeight;
                _window.MinWidth = MininumWidth;
                _window.Title = "Trinity Cache";


                var dgMainCache = new DataGrid { ItemsSource = DataModel.ObservableCache };
                //foreach (var column in TrinityCacheObjectColumns())
                //{
                //    dgMainCache.Columns.Add(column);
                //}

                _window.Content = new TabControl
                {
                    Items = {
                        new TabItem
                        {
                            Header = "Main Cache",
                            Content = new ScrollViewer {
                                HorizontalScrollBarVisibility = ScrollBarVisibility.Visible,
                                Content = dgMainCache
                            }
                        }
                    }
                };

                if (DataModel == null)
                    DataModel = new CacheUIDataModel();
                _userControl.DataContext = DataModel;

                _workerThread = new Thread(RunWorker)
                {
                    IsBackground = true
                };
                _workerThread.Start();

                _window.Closed += Window_Closed;

            }
            catch (Exception ex)
            {
                Logger.LogNormal("Unable to open Trinity CacheUI: {0}", ex);
            }

            return _window;
        }



        private static List<DataGridColumn> TrinityCacheObjectColumns()
        {
            var collection = new List<DataGridColumn>();
            foreach (var pi in typeof(TrinityCacheObject).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                bool skip = false;
                foreach (var ca in pi.CustomAttributes)
                {
                    if (ca.GetType() == typeof(NoCopy))
                    {
                        skip = true;
                        break;
                    }
                }
                if (skip)
                    continue;

                if (pi.PropertyType.IsPrimitive)
                {
                    collection.Add(new DataGridTextColumn { Header = pi.Name, IsReadOnly = true, Binding = new Binding(pi.Name) });
                }
            }
            return collection;
        }


        private static void RunWorker()
        {
            while (true)
            {
                try
                {
                    Trinity.Invoke(() =>
                    {
                        DataModel.ObservableCache.Clear();
                        foreach (var o in DataModel.SourceCacheObjects)
                        {
                            DataModel.ObservableCache.Add(o);
                        }
                    });
                    Thread.Sleep(updateDelay);
                }
                catch (ThreadAbortException) { Logger.Log("CacheUI Worker shutting down"); }
                catch (Exception ex)
                {
                    Logger.LogError("Error in CacheUI Worker: " + ex);
                }
            }
        }

        private static void Window_Closed(object sender, EventArgs e)
        {
            try
            {
                if (_workerThread.IsAlive)
                    _workerThread.Abort();
                _window = null;
            }
            catch (Exception ex)
            {
                Logger.LogNormal("Exception in Window_Closed: {0}", ex.ToString());

            }
        }

    }
}
