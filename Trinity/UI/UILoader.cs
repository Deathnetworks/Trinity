using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Trinity.Technicals;
using Trinity.UI.UIComponents;

namespace Trinity.UI
{
    public class UILoader
    {
        public static Window ConfigWindow;
        private static UserControl _windowContent;
        private static readonly object ContentLock = new object();

        public static void CloseWindow()
        {
            ConfigWindow.Close();
        }

        public static Window GetDisplayWindow()
        {
            return GetDisplayWindow(Path.Combine(FileManager.PluginPath, "UI"));
        }

        #region Events

        public delegate void LoaderEvent();

        /// <summary>
        /// Occurs when settings window is open and properly loaded
        /// </summary>
        public static event LoaderEvent OnSettingsWindowOpened = () => { };

        #endregion

        public static Window GetDisplayWindow(string uiPath)
        {
            using (new PerformanceLogger("GetDisplayWindow"))
            {
                try
                {
                    // Check we can actually find the .xaml file first - if not, report an error
                    if (!File.Exists(Path.Combine(uiPath, "MainView.xaml")))
                    {
                        Logger.Log(TrinityLogLevel.Verbose, LogCategory.UI, "MainView.xaml not found {0}", Path.Combine(uiPath, "MainView.xaml"));
                        return null;
                    }
                    Logger.Log(TrinityLogLevel.Verbose, LogCategory.UI, "MainView.xaml found");
                    if (ConfigWindow == null)
                    {
                        ConfigWindow = new Window();
                    }
                    Logger.Log(TrinityLogLevel.Verbose, LogCategory.UI, "Load Context");
                    ConfigWindow.DataContext = new ConfigViewModel(Trinity.Settings);

                    Logger.Log(TrinityLogLevel.Verbose, LogCategory.UI, "Load MainView.xaml");
                    if (_windowContent == null)
                    {
                        LoadWindowContent(uiPath);
                    }
                    Logger.Log(TrinityLogLevel.Verbose, LogCategory.UI, "Put MainControl to Window");
                    ConfigWindow.Content = _windowContent;
                    Logger.Log(TrinityLogLevel.Verbose, LogCategory.UI, "Configure Window");
                    ConfigWindow.Height = 620;
                    ConfigWindow.Width = 480;
                    ConfigWindow.MinHeight = 580;
                    ConfigWindow.MinWidth = 480;
                    ConfigWindow.Title = "Trinity";

                    // Event handling for the config window loading up/closing
                    //configWindow.Loaded += configWindow_Loaded;
                    Logger.Log(TrinityLogLevel.Verbose, LogCategory.UI, "Set Window Events");
                    ConfigWindow.Closed += WindowClosed;

                    Application.Current.Exit += WindowClosed;

                    ConfigWindow.ContentRendered += (sender, args) => OnSettingsWindowOpened();

                    Logger.Log(TrinityLogLevel.Verbose, LogCategory.UI, "Window build finished.");
                }
                catch (XamlParseException ex)
                {
                    Logger.Log(TrinityLogLevel.Error, LogCategory.UI, "{0}", ex);
                    return ConfigWindow;
                }
                return ConfigWindow;
            }
        }

        /// <summary>
        /// Loads the plugin config window XAML asyncronously, for fast re-use later.
        /// </summary>
        internal static void PreLoadWindowContent()
        {
            try
            {
                Trinity.BeginInvoke(() => LoadWindowContent(Path.Combine(FileManager.PluginPath, "UI")));
            }
            catch (Exception ex)
            {
                Logger.LogError("Exception pre-loadingn window content! " + ex);
            }
        }

        internal static void LoadWindowContent(string uiPath)
        {
            try
            {
                lock (ContentLock)
                {
                    _windowContent = LoadAndTransformXamlFile<UserControl>(Path.Combine(uiPath, "MainView.xaml"));
                    Logger.Log(TrinityLogLevel.Verbose, LogCategory.UI, "Load Children");
                    LoadChild(_windowContent, uiPath);
                    Logger.Log(TrinityLogLevel.Verbose, LogCategory.UI, "Load Resources");
                    LoadResourceForWindow(Path.Combine(uiPath, "Template.xaml"), _windowContent);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Exception loading window content! {0}", ex);
            }
        }

        private static void LoadResourceForWindow(string filename, UserControl control)
        {
            try
            {
                using (new PerformanceLogger("LoadResourceForWindow"))
                {
                    ResourceDictionary resource = LoadAndTransformXamlFile<ResourceDictionary>(filename);
                    foreach (System.Collections.DictionaryEntry res in resource)
                    {
                        control.Resources.Add(res.Key, res.Value);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Error loading resources {0}", ex);
            }
        }

        /// <summary>Loads the and transform xaml file.</summary>
        /// <param name="filename">The filename to load.</param>
        /// <returns><see cref="Stream"/> which contains transformed XAML file.</returns>
        internal static T LoadAndTransformXamlFile<T>(string filename)
        {
            try
            {
                Logger.Log(TrinityLogLevel.Verbose, LogCategory.UI, "Load XAML file : {0}", filename);
                string filecontent = File.ReadAllText(filename);

                // Change reference to custom Trinity class
                filecontent = filecontent.Replace("xmlns:ut=\"clr-namespace:Trinity.UIComponents\"", "xmlns:ut=\"clr-namespace:Trinity.UIComponents;assembly=" + Assembly.GetExecutingAssembly().GetName().Name + "\"");
                filecontent = filecontent.Replace("xmlns:objects=\"clr-namespace:Trinity.Objects\"", "xmlns:objects=\"clr-namespace:Trinity.Objects;assembly=" + Assembly.GetExecutingAssembly().GetName().Name + "\"");
                filecontent = filecontent.Replace("xmlns:mock=\"clr-namespace:Trinity.Settings.Mock\"", "xmlns:mock=\"clr-namespace:Trinity.Settings.Mock;assembly=" + Assembly.GetExecutingAssembly().GetName().Name + "\"");
                filecontent = filecontent.Replace("xmlns:charts=\"clr-namespace:LineChartLib\"", "xmlns:charts=\"clr-namespace:LineChartLib;assembly=" + Assembly.GetExecutingAssembly().GetName().Name + "\"");
                

                // Remove Template designer reference
                //filecontent = filecontent.Replace("<ResourceDictionary.MergedDictionaries><ResourceDictionary Source=\"..\\Template.xaml\"/></ResourceDictionary.MergedDictionaries>", string.Empty);
                //filecontent = filecontent.Replace("<ResourceDictionary.MergedDictionaries><ResourceDictionary Source=\"Template.xaml\"/></ResourceDictionary.MergedDictionaries>", string.Empty);

                filecontent = Regex.Replace(filecontent, "<ResourceDictionary.MergedDictionaries>.*</ResourceDictionary.MergedDictionaries>", string.Empty, RegexOptions.Singleline | RegexOptions.Compiled);

                return (T)XamlReader.Load(new MemoryStream(Encoding.UTF8.GetBytes(filecontent)));
            }
            catch (Exception ex)
            {
                Logger.LogError("Error loading/transforming XAML {0}", ex);
                return default(T);
            }
        }

        /// <summary>Call when Config Window is closed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        static void WindowClosed(object sender, EventArgs e)
        {
            Logger.Log(TrinityLogLevel.Verbose, LogCategory.UI, "Window closed.");
            ConfigWindow = null;
        }

        /// <summary>Loads recursivly the child in ContentControl or Decorator with Tag.</summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="uiPath">The UI path.</param>
        private static void LoadChild(FrameworkElement parentControl, string uiPath)
        {
            try
            {
                // Loop in Children of parent control of type FrameworkElement 
                foreach (FrameworkElement ctrl in LogicalTreeHelper.GetChildren(parentControl).OfType<FrameworkElement>())
                {
                    string contentName = ctrl.Tag as string;
                    // Tag contains a string end with ".xaml" : It's dymanic content 
                    if (!string.IsNullOrWhiteSpace(contentName) && contentName.EndsWith(".xaml"))
                    {
                        // Load content from XAML file
                        LoadDynamicContent(uiPath, ctrl, Path.Combine(uiPath, contentName));
                    }
                    else
                    {
                        // Try again with children of control
                        LoadChild(ctrl, uiPath);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Exception loading child {0}", ex);
            }
        }

        /// <summary>Loads the dynamic content from XAML file.</summary>
        /// <param name="uiPath">The UI path.</param>
        /// <param name="ctrl">The CTRL.</param>
        /// <param name="filename">Name of the content.</param>
        private static void LoadDynamicContent(string uiPath, FrameworkElement ctrl, string filename)
        {
            try
            {
                if (File.Exists(filename))
                {
                    UserControl xamlContent = LoadAndTransformXamlFile<UserControl>(filename);

                    // Dynamic load of content is possible on Content control (UserControl, ...)
                    if (ctrl is ContentControl)
                    {
                        ((ContentControl)ctrl).Content = xamlContent;
                    }
                    // Or on Decorator control (Border, ...)
                    else if (ctrl is Decorator)
                    {
                        ((Decorator)ctrl).Child = xamlContent;
                    }
                    // Otherwise, log control where you try to put dynamic tag
                    else
                    {
                        Logger.Log(TrinityLogLevel.Verbose, LogCategory.UI, "Control of type '{0}' can't be used for dynamic loading.", ctrl.GetType().FullName);
                        return;
                    }
                    // Content added to parent control, try to search dynamic control in children
                    LoadChild(xamlContent, uiPath);
                }
                else
                {
                    Logger.Log(TrinityLogLevel.Error, LogCategory.UI, "Error XAML file not found : '{0}'", filename);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Exception loading Dynamic Content {0}", ex);
            }
        }

    }
}
