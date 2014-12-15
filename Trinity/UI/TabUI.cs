using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Trinity.Items;
using Zeta.Bot;

namespace Trinity.UI
{
    class TabUi
    {
        private static Button _btnSortBackpack, _btnSortStash, _btnReloadItemRules;

        internal static void InstallTab()
        {
            Application.Current.Dispatcher.Invoke(
                () =>
                {
                    // 1st column x: 432
                    // 2nd column x: 552
                    // 3rd column x: 672

                    // Y rows: 10, 33, 56, 79, 102

                    _btnSortBackpack = new Button
                    {
                        Width = 120,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Top,
                        Margin = new Thickness(3),
                        Content = "Sort Backpack"
                    };

                    _btnSortStash = new Button
                    {
                        Width = 120,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Top,
                        Margin = new Thickness(3),
                        Content = "Sort Stash"
                    };

                    _btnReloadItemRules = new Button
                    {
                        Width = 120,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Top,
                        Margin = new Thickness(3),
                        Content = "Reload Item Rules"
                    };


                    Window mainWindow = Application.Current.MainWindow;

                    _btnSortBackpack.Click += _btnSortBackpack_Click;
                    _btnSortStash.Click += _btnSortStash_Click;
                    _btnReloadItemRules.Click += _btnReloadItemRules_Click;

                    UniformGrid uniformGrid = new UniformGrid
                    {
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Top,
                        MaxHeight = 180,
                    };

                    uniformGrid.Children.Add(_btnSortBackpack);
                    uniformGrid.Children.Add(_btnSortStash);
                    uniformGrid.Children.Add(_btnReloadItemRules);

                    _tabItem = new TabItem
                    {
                        Header = "Trinity",
                        ToolTip = "Trinity Functions",
                        Content = uniformGrid,
                    };

                    var tabs = mainWindow.FindName("tabControlMain") as TabControl;
                    if (tabs == null)
                        return;

                    tabs.Items.Add(_tabItem);
                }
            );
        }

        static void _btnSortBackpack_Click(object sender, RoutedEventArgs e)
        {
            ItemSort.SortBackpack();
        }

        static void _btnSortStash_Click(object sender, RoutedEventArgs e)
        {
            ItemSort.SortStash();
        }

        static void _btnReloadItemRules_Click(object sender, RoutedEventArgs e)
        {
            if (Trinity.StashRule == null)
                Trinity.StashRule = new ItemRules.Interpreter();

            if (Trinity.StashRule != null)
            {
                BotMain.PauseWhile(Trinity.StashRule.reloadFromUI);
            }
        }


        private static TabItem _tabItem;

        internal static void RemoveTab()
        {
            Application.Current.Dispatcher.Invoke(
                () =>
                {
                    Window mainWindow = Application.Current.MainWindow;
                    var tabs = mainWindow.FindName("tabControlMain") as TabControl;
                    if (tabs == null)
                        return;
                    tabs.Items.Remove(_tabItem);
                }
            );
        }

    }
}
