using GilesTrinity.UIComponents;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace GilesTrinity.UI
{
    public class UILoader
    {
        public static Window _ConfigWindow;
        
        public static Window GetDisplayWindow(string uiPath)
        {
            // Check we can actually find the .xaml file first - if not, report an error
            if (!File.Exists(Path.Combine(uiPath, "MainView.xaml")))
            {
                Zeta.Common.Logging.Write("MainView.xaml not found {0}", Path.Combine(uiPath, "MainView.xaml"));
                return null;
            }
            try
            {
                Zeta.Common.Logging.Write("MainView.xaml found");
                if (_ConfigWindow == null)
                {
                    _ConfigWindow = new Window();
                }
                Zeta.Common.Logging.Write("Load Context");
                _ConfigWindow.DataContext = new ConfigViewModel(GilesTrinity.Settings);

                Zeta.Common.Logging.Write("Load MainView.xaml");
                UserControl mainControl = LoadAndTransformXamlFile<UserControl>(Path.Combine(uiPath, "MainView.xaml"));
                Zeta.Common.Logging.Write("Load Children");
                LoadChild(mainControl, uiPath);
                Zeta.Common.Logging.Write("Load Resources");
                LoadResourceForWindow(Path.Combine(uiPath, "Template.xaml"), mainControl);

                Zeta.Common.Logging.Write("Configure Window");
                _ConfigWindow.Content = mainControl;
                _ConfigWindow.Height = 510;
                _ConfigWindow.Width = 480;
                _ConfigWindow.MinHeight = 470 + 40;
                _ConfigWindow.MinWidth = 432 + 40;
                _ConfigWindow.Title = "Giles Trinity";
                
                // Event handling for the config window loading up/closing
                //configWindow.Loaded += configWindow_Loaded;
                _ConfigWindow.Closed += WindowClosed;

                Demonbuddy.App.Current.Exit += WindowClosed;

                Zeta.Common.Logging.Write("Put MainControl to Window");
                // And finally put all of this content in effect
                _ConfigWindow.Content = mainControl;
                Zeta.Common.Logging.Write("End");
            }
            catch (XamlParseException ex)
            {
                Zeta.Common.Logging.WriteException(ex);
                return _ConfigWindow;
            }
            return _ConfigWindow;
        }

        private static void LoadResourceForWindow(string filename, UserControl control)
        {
            ResourceDictionary resource = LoadAndTransformXamlFile<ResourceDictionary>(filename);
            foreach (System.Collections.DictionaryEntry res in resource)
            {
                control.Resources.Add(res.Key, res.Value);
            }
        }

        /// <summary>Loads the and transform xaml file.</summary>
        /// <param name="filename">The filename to load.</param>
        /// <returns><see cref="Stream"/> which contains transformed XAML file.</returns>
        private static T LoadAndTransformXamlFile<T>(string filename)
        {
            Zeta.Common.Logging.Write("Load XAML file : {0}", filename);
            string filecontent = File.ReadAllText(filename);
            // Change reference to custom Trinity class
            filecontent = filecontent.Replace("xmlns:ut=\"clr-namespace:GilesTrinity.UIComponents\"", "xmlns:ut=\"clr-namespace:GilesTrinity.UIComponents;assembly=" + Assembly.GetExecutingAssembly().GetName().Name + "\"");
            // Remove Template designer reference
            filecontent = filecontent.Replace("<ResourceDictionary.MergedDictionaries><ResourceDictionary Source=\"..\\Template.xaml\"/></ResourceDictionary.MergedDictionaries>", string.Empty);
            filecontent = filecontent.Replace("<ResourceDictionary.MergedDictionaries><ResourceDictionary Source=\"Template.xaml\"/></ResourceDictionary.MergedDictionaries>", string.Empty);

            return (T)XamlReader.Load(new MemoryStream(Encoding.Default.GetBytes(filecontent)));
        }

        /// <summary>Call when Config Window is closed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        static void WindowClosed(object sender, System.EventArgs e)
        {
            _ConfigWindow = null;
        }

        /// <summary>Loads recursivly the child in ContentControl or Decorator with Tag.</summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="uiPath">The UI path.</param>
        private static void LoadChild(FrameworkElement parentControl, string uiPath)
        {
            // Loop in Children of parent control of type FrameworkElement 
            foreach (FrameworkElement ctrl in LogicalTreeHelper.GetChildren(parentControl).OfType<FrameworkElement>())
            {
                string contentName  = ctrl.Tag as string;
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
                    Zeta.Common.Logging.Write("ctrl : {0}", ctrl.GetType().FullName);
                    return;
                }
                // Content added to parent control, try to search dynamic control in children
                LoadChild(xamlContent, uiPath);
            }
            else
            {
                Zeta.Common.Logging.Write("Error XAML file not found : '{0}'", filename);
            }
        }
    }
}
