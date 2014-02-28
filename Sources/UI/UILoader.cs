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
using Trinity.UIComponents;

namespace Trinity.UI
{
    public class UILoader
    {
        public static Window _configWindow;
        private static UserControl _windowContent;
        private static object _contentLock = new object();

        public static void CloseWindow()
        {
            _configWindow.Close();
        }

        public static Window GetDisplayWindow()
        {
            return UILoader.GetDisplayWindow(Path.Combine(FileManager.PluginPath, "UI"));
        }

        public static Window GetDisplayWindow(string uiPath)
        {
            using (new PerformanceLogger("GetDisplayWindow"))
            {
                // Check we can actually find the .xaml file first - if not, report an error
                if (!File.Exists(Path.Combine(uiPath, "MainView.xaml")))
                {
                    Logger.Log(TrinityLogLevel.Verbose, LogCategory.UI, "MainView.xaml not found {0}", Path.Combine(uiPath, "MainView.xaml"));
                    return null;
                }
                try
                {
                    Logger.Log(TrinityLogLevel.Verbose, LogCategory.UI, "MainView.xaml found");
                    if (_configWindow == null)
                    {
                        _configWindow = new Window();
                    }
                    Logger.Log(TrinityLogLevel.Verbose, LogCategory.UI, "Load Context");
                    _configWindow.DataContext = new ConfigViewModel(Trinity.Settings);

                    Logger.Log(TrinityLogLevel.Verbose, LogCategory.UI, "Load MainView.xaml");
                    if (_windowContent == null)
                    {
                        LoadWindowContent(uiPath);
                    }
                    Logger.Log(TrinityLogLevel.Verbose, LogCategory.UI, "Put MainControl to Window");
                    _configWindow.Content = _windowContent;
                    Logger.Log(TrinityLogLevel.Verbose, LogCategory.UI, "Configure Window");
                    _configWindow.Height = 620;
                    _configWindow.Width = 480;
                    _configWindow.MinHeight = 580;
                    _configWindow.MinWidth = 480;
                    _configWindow.Title = "Trinity";

                    // Event handling for the config window loading up/closing
                    //configWindow.Loaded += configWindow_Loaded;
                    Logger.Log(TrinityLogLevel.Verbose, LogCategory.UI, "Set Window Events");
                    _configWindow.Closed += WindowClosed;

                    Demonbuddy.App.Current.Exit += WindowClosed;


                    Logger.Log(TrinityLogLevel.Verbose, LogCategory.UI, "Window build finished.");
                }
                catch (XamlParseException ex)
                {
                    Logger.Log(TrinityLogLevel.Error, LogCategory.UI, "{0}", ex);
                    return _configWindow;
                }
                return _configWindow;
            }
        }

        /// <summary>
        /// Loads the plugin config window XAML asyncronously, for fast re-use later.
        /// </summary>
        internal static void PreLoadWindowContent()
        {
            Trinity.BeginInvoke(new Action(() => LoadWindowContent(Path.Combine(FileManager.PluginPath, "UI"))));
        }

        internal static void LoadWindowContent(string uiPath)
        {
            lock (_contentLock)
            {
                _windowContent = LoadAndTransformXamlFile<UserControl>(Path.Combine(uiPath, "MainView.xaml"));
                Logger.Log(TrinityLogLevel.Verbose, LogCategory.UI, "Load Children");
                LoadChild(_windowContent, uiPath);
                Logger.Log(TrinityLogLevel.Verbose, LogCategory.UI, "Load Resources");
                LoadResourceForWindow(Path.Combine(uiPath, "Template.xaml"), _windowContent);
            }
        }

        private static void LoadResourceForWindow(string filename, UserControl control)
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

        /// <summary>Loads the and transform xaml file.</summary>
        /// <param name="filename">The filename to load.</param>
        /// <returns><see cref="Stream"/> which contains transformed XAML file.</returns>
        internal static T LoadAndTransformXamlFile<T>(string filename)
        {
            Logger.Log(TrinityLogLevel.Verbose, LogCategory.UI, "Load XAML file : {0}", filename);
            string filecontent = File.ReadAllText(filename);

            // Change reference to custom Trinity class
            filecontent = filecontent.Replace("xmlns:ut=\"clr-namespace:Trinity.UIComponents\"", "xmlns:ut=\"clr-namespace:Trinity.UIComponents;assembly=" + Assembly.GetExecutingAssembly().GetName().Name + "\"");

            // Remove Template designer reference
            //filecontent = filecontent.Replace("<ResourceDictionary.MergedDictionaries><ResourceDictionary Source=\"..\\Template.xaml\"/></ResourceDictionary.MergedDictionaries>", string.Empty);
            //filecontent = filecontent.Replace("<ResourceDictionary.MergedDictionaries><ResourceDictionary Source=\"Template.xaml\"/></ResourceDictionary.MergedDictionaries>", string.Empty);

            filecontent = Regex.Replace(filecontent, "<ResourceDictionary.MergedDictionaries>.*</ResourceDictionary.MergedDictionaries>", string.Empty, RegexOptions.Multiline | RegexOptions.Compiled);

            return (T)XamlReader.Load(new MemoryStream(Encoding.UTF8.GetBytes(filecontent)));
        }

        /// <summary>Call when Config Window is closed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        static void WindowClosed(object sender, System.EventArgs e)
        {
            Logger.Log(TrinityLogLevel.Verbose, LogCategory.UI, "Window closed.");
            _configWindow = null;
        }

        /// <summary>Loads recursivly the child in ContentControl or Decorator with Tag.</summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="uiPath">The UI path.</param>
        private static void LoadChild(FrameworkElement parentControl, string uiPath)
        {
            using (new PerformanceLogger("LoadChild"))
            {
                // Loop in Children of parent control of type FrameworkElement 
                foreach (FrameworkElement ctrl in LogicalTreeHelper.GetChildren(parentControl).OfType<FrameworkElement>())
                {
                    string contentName = ctrl.Tag as string;
                    // Tag contains a string end with ".xaml" : It's dymanic content 
                    if (!string.IsNullOrWhiteSpace(contentName) && contentName.EndsWith(".xaml"))
                    {
                        // Load content from XAML file
                        LoadDynamicContent(uiPath, ctrl, System.IO.Path.Combine(uiPath, contentName));
                    }
                    else
                    {
                        // Try again with children of control
                        LoadChild(ctrl, uiPath);
                    }
                }
            }
        }

        /// <summary>Loads the dynamic content from XAML file.</summary>
        /// <param name="uiPath">The UI path.</param>
        /// <param name="ctrl">The CTRL.</param>
        /// <param name="filename">Name of the content.</param>
        private static void LoadDynamicContent(string uiPath, FrameworkElement ctrl, string filename)
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

    }
}
