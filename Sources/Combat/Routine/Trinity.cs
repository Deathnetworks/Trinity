// VERSION 1.1.2
using System;
using System.Windows;
using Zeta;
using Zeta.CommonBot;
using Zeta.Internals.Actors;
using Zeta.TreeSharp;
using Trinity;
using Zeta.Common.Plugins;
using Action = Zeta.TreeSharp.Action;
using System.Diagnostics;

namespace Trinity
{
    public class TrinityRoutine : CombatRoutine
    {
        public override void Initialize()
        {
            foreach (PluginContainer plugin in PluginManager.Plugins)
            {
                if (plugin.Plugin.Name == "Trinity" && !plugin.Enabled)
                {
                    plugin.Enabled = true;
                }
            }
        }

        public override void Dispose()
        {
        }

        public override string Name { get { return "Trinity"; } }

        public override Window ConfigWindow
        {
            get
            {
                foreach (PluginContainer plugin in PluginManager.Plugins)
                {
                    if (plugin.Plugin.Name == "Trinity")
                    {
                        return plugin.Plugin.DisplayWindow;
                    }
                }
                return null;
            }
        }

        public override ActorClass Class
        {
            get
            {
                if (!ZetaDia.IsInGame || ZetaDia.IsLoadingWorld)
                {
                    // Return none if we are oog to make sure we can start the bot anytime.
                    return ActorClass.Invalid;
                }

                return ZetaDia.Me.ActorClass;
            }
        }

        public override SNOPower DestroyObjectPower
        {
            get
            {
                return SNOPower.None;
            }
        }

        public override float DestroyObjectDistance { get { return 0; } }

        public override Composite Combat { get { return new Action(); } }
        public override Composite Buff { get { return new Action(); } }

        public static bool Vt(bool t, string f)
        {
            var v1 = FileVersionInfo.GetVersionInfo(f);
            string[] v2 = v1.FileVersion.Split('.');
            int v = 0;
            Int32.TryParse(v2[v2.Length - 2], out v);
            if (v >= 1425)
                t = true;
            return t;
        }
    }
}
