using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Trinity.Helpers;
using Trinity.Items;
using Trinity.Technicals;
using Trinity.Reference;
using Trinity.Objects;
using Zeta.Bot;
using Zeta.Common;
using Zeta.Game;
using Zeta.Game.Internals.Actors;
using Extensions = Zeta.Common.Extensions;
using Logger = Trinity.Technicals.Logger;

namespace Trinity.UI
{
    class TabUi
    {
        private static Button _btnSortBackpack, _btnSortStash, _btnReloadItemRules, _btnGetNewActorSNOs, _btnDropLegendaries, _btnCleanStash;

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

                    _btnCleanStash = new Button
                    {
                        Width = 120,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Top,
                        Margin = new Thickness(3),
                        Content = "Clean Stash"
                    };

                    _btnReloadItemRules = new Button
                    {
                        Width = 120,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Top,
                        Margin = new Thickness(3),
                        Content = "Reload Item Rules"
                    };

                    _btnDropLegendaries = new Button
                    {
                        Width = 120,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Top,
                        Margin = new Thickness(3),
                        Content = "Drop Legendaries"
                    };

                    _btnGetNewActorSNOs = new Button
                    {
                        Width = 120,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Top,
                        Margin = new Thickness(3),
                        Content = "Find New ActorIds"
                    };

                    _btnDumpBuild = new Button
                    {
                        Width = 120,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Top,
                        Margin = new Thickness(3),
                        Content = "Dump My Build"
                    };

                    Window mainWindow = Application.Current.MainWindow;

                    _btnSortBackpack.Click += _btnSortBackpack_Click;
                    _btnSortStash.Click += _btnSortStash_Click;
                    _btnCleanStash.Click += _btnCleanStash_Click;
                    _btnReloadItemRules.Click += _btnReloadItemRules_Click;
                    _btnDropLegendaries.Click += _btnDropLegendaries_Click;
                    _btnGetNewActorSNOs.Click += BtnGetNewActorSnOsOnClick;
                    _btnDumpBuild.Click += BtnDumpBuildOnClick;

                    var uniformGrid = new UniformGrid
                    {
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Top,
                        MaxHeight = 180,
                    };

                    uniformGrid.Children.Add(_btnSortBackpack);
                    uniformGrid.Children.Add(_btnSortStash);
                    uniformGrid.Children.Add(_btnCleanStash);
                    uniformGrid.Children.Add(_btnReloadItemRules);
                    uniformGrid.Children.Add(_btnDropLegendaries);
                    uniformGrid.Children.Add(_btnGetNewActorSNOs);
                    uniformGrid.Children.Add(_btnDumpBuild);

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

        private static void BtnDumpBuildOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            DebugUtil.LogBuildAndItems();            
        }

        private static void BtnGetNewActorSnOsOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            DebugUtil.LogNewItems();
        }


        static void _btnSortBackpack_Click(object sender, RoutedEventArgs e)
        {
            ItemSort.SortBackpack();         
        }

        static void _btnDropLegendaries_Click(object sender, RoutedEventArgs e)
        {
            using (new MemoryHelper())
            {
                ZetaDia.Me.Inventory.Backpack.Where(i => i.ItemQualityLevel == ItemQuality.Legendary).ForEach(i => i.Drop());

                if (BotMain.IsRunning && !BotMain.IsPausedForStateExecution)
                    BotMain.PauseFor(TimeSpan.FromSeconds(2));                
            }
        }

        static void _btnSortStash_Click(object sender, RoutedEventArgs e)
        {
            ItemSort.SortStash();
        }

        static void _btnCleanStash_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Are you sure? This may remove and salvage/sell items from your stash! Permanently!", "Clean Stash Confirmation",
                MessageBoxButton.OKCancel);

            if (result == MessageBoxResult.OK)
            {
                CleanStash.RunCleanStash();
            }
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
        private static Button _btnDumpBuild;

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
