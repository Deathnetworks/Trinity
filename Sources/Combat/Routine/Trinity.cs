// VERSION 1.2.0
using System;
using System.Windows;
using Zeta;
using Zeta.Common;
using Zeta.Common.Plugins;
using Zeta.CommonBot;
using Zeta.Internals.Actors;
using Zeta.TreeSharp;
using Action = Zeta.TreeSharp.Action;

namespace TrinityRoutine
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
                try
                {
                    foreach (PluginContainer plugin in PluginManager.Plugins)
                    {
                        if (plugin.Plugin.Name == "Trinity")
                        {
                            return plugin.Plugin.DisplayWindow;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logging.Write("[Trinity] Error Opening Plugin Config window!");
                    Logging.Write("[Trinity] {0}", ex);
                }
                Logging.Write("[Trinity] Unable to open Plugin Config window!");
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

    }
}
