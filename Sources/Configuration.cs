﻿using GilesTrinity.UI;
using System.IO;
using System.Windows;
using Zeta.Common.Plugins;

namespace GilesTrinity
{
    public partial class GilesTrinity : IPlugin
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
                return UILoader.GetDisplayWindow(Path.Combine(sTrinityPluginPath, "UI"));
            }
        }
    }
}