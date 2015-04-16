using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        public static Window CreateWindow()
        {
            try
            {
                _window = new Window
                {
                    Height = 150,
                    Width = 600,
                    MinHeight = MinimumHeight,
                    MinWidth = MininumWidth,
                    Title = "Trinity Cache",
                    Content = new TabControl
                    {
                        Items =
                        {
                            new TabItem
                            {
                                Header = "Main Cache",
                                Content = new ScrollViewer
                                {
                                    HorizontalScrollBarVisibility = ScrollBarVisibility.Visible,
                                    Content = new DataGrid
                                    {
                                        AutoGenerateColumns = false,
                                        ItemsSource = DataModel.ObservableCache,
                                        DataContext = DataModel.ObservableCache,
                                        Columns =
                                        {
                                            new DataGridTextColumn {Header = "Name", IsReadOnly = true, Binding = new Binding("InternalName")},
                                            new DataGridTextColumn {Header = "Type", IsReadOnly = true, Binding = new Binding("Type")},
                                            new DataGridTextColumn {Header = "Weight", IsReadOnly = true, Binding = new Binding("Weight")},
                                            new DataGridTextColumn {Header = "WeightInfo", IsReadOnly = true, Binding = new Binding("WeightInfo")},
                                            new DataGridTextColumn {Header = "IsBossOrEliteRareUnique", IsReadOnly = true, Binding = new Binding("IsBossOrEliteRareUnique")},
                                            new DataGridTextColumn {Header = "Distance", IsReadOnly = true, Binding = new Binding("Distance")},
                                            new DataGridTextColumn {Header = "Radius", IsReadOnly = true, Binding = new Binding("Radius")},
                                            //new DataGridTextColumn{Header = "", IsReadOnly = true, Binding = new Binding("") },
                                            //new DataGridTextColumn{Header = "", IsReadOnly = true, Binding = new Binding("") },
                                            //new DataGridTextColumn{Header = "", IsReadOnly = true, Binding = new Binding("") },
                                            //new DataGridTextColumn{Header = "", IsReadOnly = true, Binding = new Binding("") },
                                        }
                                    }
                                }
                            }
                        }
                    }
                };

                if (DataModel == null)
                    DataModel = new CacheUIDataModel();
                _userControl.DataContext = DataModel;

                _window.Closed += Window_Closed;
                Configuration.Events.OnCacheUpdated += Update;
                Update();

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


        private static void Update()
        {
            try
            {
                Trinity.Invoke(() =>
                {
                    foreach (var o in DataModel.SourceCacheObjects)
                    {
                        var existing = DataModel.ObservableCache.FirstOrDefault(oc => oc.RActorGuid == o.RActorGuid);
                        if (existing != null)
                        {
                            DataModel.ObservableCache.Remove(existing);
                            DataModel.ObservableCache.Add(o);
                        }
                        else
                            DataModel.ObservableCache.Add(o);
                    }
                    foreach (var o in DataModel.ObservableCache.ToList())
                    {
                        if (DataModel.SourceCacheObjects.All(so => so.RActorGuid != o.RActorGuid))
                            DataModel.ObservableCache.Remove(o);
                    }
                });
            }
            catch (Exception ex)
            {
                Logger.LogError("Error in CacheUI Worker: " + ex);
            }
        }

        private static void Window_Closed(object sender, EventArgs e)
        {
            try
            {
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
