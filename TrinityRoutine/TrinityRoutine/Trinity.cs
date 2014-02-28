using System.Windows;
using Zeta.Bot;
using Zeta.Common.Plugins;
using Zeta.Game;
using Zeta.Game.Internals.Actors;
using Zeta.TreeSharp;

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

        public override SNOPower DestroyObjectPower { get { return ZetaDia.CPlayer.GetActiveSkillBySlot(HotbarSlot.HotbarMouseLeft).Power; } }

        public override float DestroyObjectDistance { get { return 15; } }

        public override Composite Combat { get { return new PrioritySelector(); } }
        public override Composite Buff { get { return new PrioritySelector(); } }

    }
}
