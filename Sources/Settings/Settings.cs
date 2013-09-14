﻿using System.IO;
using System.Windows;
using Trinity.Technicals;
using Trinity.UI;
using Zeta.Common.Plugins;

namespace Trinity
{
    public partial class Trinity : IPlugin
    {
        // Save Configuration
        private void SaveConfiguration()
        {
            Settings.Save();
        }
        // Load Configuration
        private void LoadConfiguration()
        {
            Settings.Load();
        }

        /// <summary>
        /// Gets the configuration Window for UnifiedTrinity.
        /// </summary>
        /// <value>The display window.</value>
        public Window DisplayWindow
        {
            get
            {
                return UILoader.GetDisplayWindow(Path.Combine(FileManager.PluginPath, "UI"));
            }
        }
    }
}