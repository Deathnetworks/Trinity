using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
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
            V.ValidateLoad();
            TVars = CollectionViewSource.GetDefaultView(V.Data);

            InitializeButtons();
        }

        private void InitializeButtons()
        {
            ResetTVarsCommand = new RelayCommand(
                                    (parameter) =>
                                    {
                                        if (MessageBox.Show("Are you sure you want to reset all Trinity Variables?", "Confirm Reset All",
                                            MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                                        {
                                            V.ResetAll();
                                        }
                                    });
            SaveTVarsCommand = new RelayCommand(
                                    (parameter) =>
                                    {
                                        V.Save();
                                    });
            DumpTVarsCommand = new RelayCommand(
                                    (parameter) =>
                                    {
                                        V.Dump();
                                    });
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
            CancelDataGridEdit(_window.Content);
            _window = null;
        }

        static void CancelDataGridEdit(object elem)
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
    }
}
