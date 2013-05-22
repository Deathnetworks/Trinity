using System;
using System.Windows;
using Zeta;
using Zeta.CommonBot;
using Zeta.Internals.Actors;
using Zeta.TreeSharp;
using Trinity;
using Zeta.Common.Plugins;

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

        public override ActorClass Class { get { return ZetaDia.Me.ActorClass; } }

        public override SNOPower DestroyObjectPower { get { return ZetaDia.Me.GetHotbarPowerId(HotbarSlot.HotbarMouseLeft); } }

        public override float DestroyObjectDistance { get { return 15; } }

        public override Composite Combat { get { return new PrioritySelector(); } }
        public override Composite Buff { get { return new PrioritySelector(); } }

    }
}
