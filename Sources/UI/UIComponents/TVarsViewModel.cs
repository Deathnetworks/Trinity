using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Trinity.Technicals;
using Trinity.UI;

namespace Trinity.UIComponents
{
    public class TVarsViewModel
    {
        public ICollectionView TVars { get; private set; }
        public ICommand ResetTVarsCommand { get; private set; }
        public ICommand SaveTVarsCommand { get; private set; }
        public ICommand DumpTVarsCommand { get; private set; }

        public TVarsViewModel()
        {
            try
            {
                V.ValidateLoad();
                TVars = CollectionViewSource.GetDefaultView(V.Data.OrderBy(v => v.Key));

                InitializeButtons();
            }
            catch (Exception ex)
            {
                Logger.LogNormal("Exception in TVarsViewModel constructor {0}", ex.ToString());
            }
        }

        private void InitializeButtons()
        {
            try
            {
                ResetTVarsCommand = new RelayCommand(
                                        (parameter) =>
                                        {
                                            try
                                            {
                                                if (MessageBox.Show(
                                                    "Are you sure you want to reset all Trinity Variables?",
                                                    "Confirm Reset All",
                                                    MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                                                {
                                                    V.ResetAll();
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                Logger.Log("Exception Resetting TVars: {0}", ex);
                                            }
                                        });
                SaveTVarsCommand = new RelayCommand(
                                        (parameter) =>
                                        {
                                            try { V.Save(); }
                                            catch (Exception ex)
                                            {
                                                Logger.Log("Exception saving TVars: {0}", ex);
                                            }
                                        });
                DumpTVarsCommand = new RelayCommand(
                                        (parameter) =>
                                        {
                                            try { V.Dump(); }
                                            catch (Exception ex)
                                            {
                                                Logger.Log("Exception Dumping TVars: {0}", ex);
                                            }

                                        });
            }
            catch (Exception ex)
            {
                Logger.LogNormal("Exception in InitializeButtons: {0}", ex.ToString());

            }
        }

        private static Window _window;

        public static Window CreateWindow()
        {
            try
            {
                var filename = Path.Combine(FileManager.PluginPath, "UI", "TVars.xaml");

                if (_window == null)
                {
                    _window = new Window();
                }

                _window.DataContext = new TVarsViewModel();

                string content = File.ReadAllText(filename);
                UserControl userControl = UILoader.LoadAndTransformXamlFile<UserControl>(filename);

                _window.Content = userControl;
                _window.Height = 620;
                _window.Width = 600;
                _window.MinHeight = 580;
                _window.MinWidth = 600;
                _window.Title = "Trinity Variables";

                _window.Closed += Window_Closed;
            }
            catch (Exception ex)
            {
                Logger.LogNormal("Unable to open Trinity Variables: {0}", ex.ToString());
            }

            return _window;
        }

        static void Window_Closed(object sender, EventArgs e)
        {
            try
            {
                CancelDataGridEdit(_window.Content);
                _window = null;
            }
            catch (Exception ex)
            {
                Logger.LogNormal("Exception in Window_Closed: {0}", ex.ToString());

            }
        }

        static void CancelDataGridEdit(object elem)
        {
            try
            {
                if (elem is DataGrid)
                {
                    ((DataGrid)elem).CancelEdit();
                }
                else if (elem is ContentControl)
                {
                    var cc = (ContentControl)elem;
                    if (cc.HasContent)
                        CancelDataGridEdit(cc.Content);
                }
            }
            catch (Exception ex)
            {
                Logger.LogNormal("Exception in CancelDataGridEdit: {0}", ex.ToString());
            }

        }
    }
}
