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

                if (_ConfigWindow == null)
                {
                    _ConfigWindow = new Window();
                }
                UserControl xamlContent;
                //using (StreamReader xamlStream = new StreamReader(Path.Combine(uiPath, "MainView.xaml")))
                using (Stream xamlStream = LoadAndTransformXamlFile(Path.Combine(uiPath, "MainView.xaml")))
                {
                    //xamlContent = XamlReader.Load(xamlStream.BaseStream) as UserControl;
                    xamlContent = XamlReader.Load(xamlStream) as UserControl;
                }
                LoadChild(xamlContent, uiPath);
                _ConfigWindow.Content = xamlContent;
                _ConfigWindow.Height = xamlContent.Height + 30;
                _ConfigWindow.Width = xamlContent.Width;
                _ConfigWindow.Title = "Giles Trinity";
                _ConfigWindow.DataContext = GilesTrinity.Settings;
                // Event handling for the config window loading up/closing
                //configWindow.Loaded += configWindow_Loaded;
                _ConfigWindow.Closed += WindowClosed;
                // And finally put all of this content in effect
                _ConfigWindow.Content = xamlContent;
            }
            catch (XamlParseException ex)
            {
                Zeta.Common.Logging.WriteException(ex);
            }
            return _ConfigWindow;
        }

        private static Stream LoadAndTransformXamlFile(string filename)
        {
            string filecontent = File.ReadAllText(filename);
            filecontent = filecontent.Replace("xmlns:ut=\"clr-namespace:GilesTrinity.UIComponents\"", "xmlns:ut=\"clr-namespace:GilesTrinity.UIComponents;assembly=" + Assembly.GetExecutingAssembly().GetName().Name + "\"");
            return new MemoryStream(Encoding.Default.GetBytes(filecontent));
        }
        /// <summary>Call when Config Window is closed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        static void WindowClosed(object sender, System.EventArgs e)
        {
            lock (_ConfigWindow)
            {
                _ConfigWindow = null;
            }
        }

        /// <summary>Loads recursivly the child in ContentControl or Decorator with Tag.</summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="uiPath">The UI path.</param>
        private static void LoadChild(FrameworkElement parentControl, string uiPath)
        {
            foreach (FrameworkElement ctrl in LogicalTreeHelper.GetChildren(parentControl).OfType<FrameworkElement>())
            {
                string contentName  = ctrl.Tag as string;
                if (!string.IsNullOrWhiteSpace(contentName) && contentName.EndsWith(".xaml"))
                {
                    UserControl xamlContent;
                    //using (StreamReader xamlStream = new StreamReader(System.IO.Path.Combine(uiPath, contentName)))
                    using (Stream xamlStream = LoadAndTransformXamlFile(System.IO.Path.Combine(uiPath, contentName)))
                    {
                        //xamlContent = XamlReader.Load(xamlStream.BaseStream) as UserControl;
                        xamlContent = XamlReader.Load(xamlStream) as UserControl;
                    }
                    LoadChild(xamlContent, uiPath);
                    if (ctrl is ContentControl)
                    {
                        ((ContentControl)ctrl).Content = xamlContent;
                    }
                    else if (ctrl is Decorator)
                    {
                        ((Decorator)ctrl).Child = xamlContent;
                    }
                    else
                    {
                        Zeta.Common.Logging.Write("ctrl : {0}", ctrl.GetType().FullName);
                    }
                }
                else
                {
                    LoadChild(ctrl, uiPath);
                }
            }
        }
    }
}
