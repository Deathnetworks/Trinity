using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Trinity.Helpers;
using Trinity.Items;
using Trinity.UI.UIComponents;
using Zeta.Bot;
using Zeta.Game;
using Zeta.Game.Internals.Actors;
using Zeta.Common;

using Logger = Trinity.Technicals.Logger;

namespace Trinity.UI
{
    class TabUi
    {
        private static UniformGrid _tabGrid;
        private static TabItem _tabItem;
        internal static void InstallTab()
        {
            Application.Current.Dispatcher.Invoke(
                () =>
                {

                    Window mainWindow = Application.Current.MainWindow;

                    _tabGrid = new UniformGrid
                    {
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Top,
                        MaxHeight = 180,
                    };

                    CreateButton("Configure", ShowMainTrinityUIEventHandler);
                    CreateButton("Sort Backpack", SortBackEventHandler);
                    CreateButton("Sort Stash", SortStashEventHandler);
                    CreateButton("Clean Stash", CleanStashEventHandler);
                    CreateButton("Reload Item Rules", ReloadItemRulesEventHandler);
                    CreateButton("Drop Legendaries", DropLegendariesEventHandler);
                    CreateButton("Find New ActorIds", GetNewActorSNOsEventHandler);
                    CreateButton("Dump My Build", DumpBuildEventHandler);
                    CreateButton("Show Cache", ShowCacheWindowEventHandler);


                    _tabItem = new TabItem
                    {
                        Header = "Trinity",
                        ToolTip = "Trinity Functions",
                        Content = _tabGrid,
                    };

                    var tabs = mainWindow.FindName("tabControlMain") as TabControl;
                    if (tabs == null)
                        return;

                    tabs.Items.Add(_tabItem);
                }
            );
        }

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

        private static void CreateButton(string buttonText, RoutedEventHandler clickHandler)
        {
            var button = new Button
            {
                Width = 120,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(3),
                Content = buttonText
            };
            button.Click += clickHandler;
            _tabGrid.Children.Add(button);
        }


        /**************
         * 
         * WARNING
         * 
         * ALWAYS surround your RoutedEventHandlers in try/catch. Failure to do so will result in Demonbuddy CRASHING if an exception is thrown.
         * 
         * WARNING
         *  
         *************/

        private static void ShowCacheWindowEventHandler(object sender, RoutedEventArgs routedEventArgs)
        {
            try
            {
                //CacheUI.CreateWindow().Show();
            }
            catch (Exception ex)
            {
                Logger.LogError("Error showing CacheUI:" + ex);
            }
        }

        private static void ShowMainTrinityUIEventHandler(object sender, RoutedEventArgs routedEventArgs)
        {
            try
            {
                var configWindow = UILoader.GetDisplayWindow();
                configWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                Logger.LogError("Error showing Configuration from TabUI:" + ex);
            }
        }

        private static void DumpBuildEventHandler(object sender, RoutedEventArgs routedEventArgs)
        {
            DebugUtil.LogBuildAndItems();
        }

        private static void GetNewActorSNOsEventHandler(object sender, RoutedEventArgs routedEventArgs)
        {
            try
            {
                DebugUtil.LogNewItems();
            }
            catch (Exception ex)
            {
                Logger.LogError("Error logging new items:" + ex);
            }
        }

        private static void SortBackEventHandler(object sender, RoutedEventArgs e)
        {
            try
            {
                ItemSort.SortBackpack();
            }
            catch (Exception ex)
            {
                Logger.LogError("Error sorting backpack:" + ex);
            }
        }

        private static void DropLegendariesEventHandler(object sender, RoutedEventArgs e)
        {
            try
            {
                using (new MemoryHelper())
                {
                    ZetaDia.Me.Inventory.Backpack.Where(i => i.ItemQualityLevel == ItemQuality.Legendary).ForEach(i => i.Drop());

                    if (BotMain.IsRunning && !BotMain.IsPausedForStateExecution)
                        BotMain.PauseFor(TimeSpan.FromSeconds(2));
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Error dropping legendaries:" + ex);
            }
        }

        private static void SortStashEventHandler(object sender, RoutedEventArgs e)
        {
            try
            {
                ItemSort.SortStash();
            }
            catch (Exception ex)
            {
                Logger.LogError("Error dropping legendaries:" + ex);
            }
        }

        private static void CleanStashEventHandler(object sender, RoutedEventArgs e)
        {
            try
            {
                var result = MessageBox.Show("Are you sure? This may remove and salvage/sell items from your stash! Permanently!", "Clean Stash Confirmation",
                           MessageBoxButton.OKCancel);

                if (result == MessageBoxResult.OK)
                {
                    CleanStash.RunCleanStash();
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Error Cleaning Stash:" + ex);
            }
        }

        private static void ReloadItemRulesEventHandler(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Trinity.StashRule == null)
                    Trinity.StashRule = new ItemRules.Interpreter();

                if (Trinity.StashRule != null)
                {
                    BotMain.PauseWhile(Trinity.StashRule.reloadFromUI);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Error Reloading Item Rules:" + ex);
            }
        }


    }
}
