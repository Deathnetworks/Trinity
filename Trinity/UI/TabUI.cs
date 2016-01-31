using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Trinity.Helpers;
using Trinity.Items;
using Trinity.LazyCache;
using Trinity.Technicals;
using Trinity.UI.UIComponents;
using Zeta.Bot;
using Zeta.Game;
using Zeta.Game.Internals.Actors;
using Zeta.Common;
using Application = System.Windows.Application;
using Button = System.Windows.Controls.Button;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using Logger = Trinity.Technicals.Logger;
using MessageBox = System.Windows.MessageBox;
using TabControl = System.Windows.Controls.TabControl;

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
                    CreateButton("Reset TVars", ResetTVarsEventHandler);
                    CreateButton("Start LazyCache", StartLazyCacheEventHandler);
                    CreateButton("Stop LazyCache", StopLazyCacheEventHandler);
                    CreateButton("Cache Test", CacheTestCacheEventHandler);

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

        #region TabMethods
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
        #endregion

        /**************
         * 
         * WARNING
         * 
         * ALWAYS surround your RoutedEventHandlers in try/catch. Failure to do so will result in Demonbuddy CRASHING if an exception is thrown.
         * 
         * WARNING
         *  
         *************/

        private static void CacheTestCacheEventHandler(object sender, RoutedEventArgs routedEventArgs)
        {
            try
            {
                var testUnit = CacheManager.Units.FirstOrDefault();
                if (testUnit != null)
                {
                    var rActorGuid = testUnit.RActorGuid;
                    var ACDGuid = testUnit.ACDGuid;
                    var accessCount = 25;
                    var timerA = new Stopwatch();
                    var timerB = new Stopwatch();

                    var timerBResults = new List<double>();

                    Logger.Log("Starting NOT-CACHED Test for {0}, {1} cycles", testUnit.InternalName, accessCount);

                    // Not Cached

                    timerA.Start();
                    var zetaObj = ZetaDia.Actors.GetActorsOfType<DiaUnit>(true).FirstOrDefault(o => o.RActorGuid == rActorGuid);
                    timerA.Stop();

                    for (int i = 0; i < accessCount; i++)
                    {
                        timerB.Reset();
                        timerB.Start();  
                        var position = zetaObj.Position;
                        var distance = zetaObj.Distance;
                        var actorType = zetaObj.ActorType;
                        var los = NavHelper.CanRayCast(position);
                        timerB.Stop();
                        timerBResults.Add(timerB.Elapsed.TotalMilliseconds);
                        Logger.Log("Position={0} Distance={1} los={2} type={3} time={4:00.0000}ms", position, distance, los, actorType, timerB.Elapsed.TotalMilliseconds);
                    }

                    Logger.Log("FindTime={0:00.0000}ms Cycles={1:00.0000}ms CycleAVG={2:00.0000}ms", timerA.Elapsed.TotalMilliseconds, timerBResults.Sum(), timerBResults.Average());

                    Logger.Log("Starting LAZYCACHE Test for {0}, {1} cycles", testUnit.InternalName, accessCount);

                    // LazyCache

                    timerA.Reset();
                    timerBResults.Clear();

                    timerA.Start();
                    var trinityObject = CacheManager.GetActorByACDGuid<TrinityUnit>(ACDGuid);
                    timerA.Stop();

                    if (trinityObject == null)
                    {
                        Logger.Log("Actor not found. RActorGuid={0}", rActorGuid);
                    }
                    else
                    {
                        for (int x = 0; x < accessCount; x++)
                        {
                            timerB.Reset();
                            timerB.Start();
                            var position = trinityObject.Position;
                            var distance = trinityObject.Distance;
                            var actorType = trinityObject.ActorType;
                            var los = trinityObject.IsInLineOfSight;
                            timerB.Stop();
                            timerBResults.Add(timerB.Elapsed.TotalMilliseconds);
                            Logger.Log("Position={0} Distance={1} los={2} type={3} time={4:00.0000}ms", position, distance, los, actorType, timerB.Elapsed.TotalMilliseconds);
                        }

                        Logger.Log("FindTime={0:00.0000}ms Cycles={1:00.0000}ms CycleAVG={2:00.0000}ms", timerA.Elapsed.TotalMilliseconds, timerBResults.Sum(), timerBResults.Average());
                    }


                    Logger.Log("Starting TRINITYCACHEOBJECT Test for {0}, {1} cycles", testUnit.InternalName, accessCount);

                    // existing TrinityCacheObject

                    timerA.Reset();
                    timerBResults.Clear();
                    timerA.Start();
                    zetaObj = ZetaDia.Actors.GetActorsOfType<DiaUnit>(true).FirstOrDefault(o => o.RActorGuid == rActorGuid);
                    TrinityCacheObject trinObj = new TrinityCacheObject();
                    timerA.Stop();

                    if (zetaObj == null)
                    {
                        Logger.Log("Actor not found. RActorGuid={0}", rActorGuid);
                    }
                    else
                    {
                        for (int i = 0; i < accessCount; i++)
                        {
                            timerB.Reset();
                            timerB.Start();
                            if (i == 0)
                            {
                                trinObj.Position = zetaObj.Position;
                                trinObj.Distance = zetaObj.Distance;
                                trinObj.HasBeenInLoS = NavHelper.CanRayCast(trinObj.Position);
                            }
                            var position = trinObj.Position;
                            var distance = trinObj.Distance;
                            var actorType = trinObj.ActorType;
                            var los = trinObj.HasBeenInLoS;
                            timerB.Stop();
                            timerBResults.Add(timerB.Elapsed.TotalMilliseconds);
                            Logger.Log("Position={0} Distance={1} los={2} type={3} time={4:00.0000}ms", position, distance, los, actorType, timerB.Elapsed.TotalMilliseconds);
                        }

                        Logger.Log("FindTime={0:00.0000}ms Cycles={1:00.0000}ms CycleAVG={2:00.0000}ms", timerA.Elapsed.TotalMilliseconds, timerBResults.Sum(), timerBResults.Average());
                        
                    }


                }
                Logger.Log("Finished Cache Test");
            }
            catch (Exception ex)
            {
                Logger.LogError("Exception {0}", ex);
            }
        }

        private static void StartLazyCacheEventHandler(object sender, RoutedEventArgs routedEventArgs)
        {
            try
            {
                CacheManager.Start();
            }
            catch (Exception ex)
            {
                Logger.LogError("Error Starting LazyCache: " + ex);
            }
        }

        private static void StopLazyCacheEventHandler(object sender, RoutedEventArgs routedEventArgs)
        {
            try
            {
                CacheManager.Stop();
            }
            catch (Exception ex)
            {
                Logger.LogError("Error Starting LazyCache: " + ex);
            }
        }


        private static void ResetTVarsEventHandler(object sender, RoutedEventArgs routedEventArgs)
        {
            try
            {
                var doReset = MessageBox.Show("This will reset all of the advanced Trinity Variables. Are you sure?", "Reset Trinity Variables",
                    MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                if (doReset == MessageBoxResult.OK)
                    V.ResetAll();
            }
            catch (Exception ex)
            {
                Logger.LogError("Error Resetting TVar's:" + ex);
            }
        }

        private static void ShowCacheWindowEventHandler(object sender, RoutedEventArgs routedEventArgs)
        {
            try
            {
                CacheUI.CreateWindow();
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
