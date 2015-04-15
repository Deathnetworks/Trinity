using System.Windows;
using Zeta;
using Zeta.CommonBot;
using Zeta.Internals.Actors;
using Zeta.TreeSharp;

namespace Trinity
{
    public class Trinity : CombatRoutine
    {
        public override string Name { get { return "Trinity"; } }

        public override void Initialize() { }
        public override void Dispose() { }
        public override Window ConfigWindow
        {
            get
            {
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
        public override float DestroyObjectDistance { get { return 15; } }
        public override Composite Combat { get { return new PrioritySelector(); } }
        public override Composite Buff { get { return new PrioritySelector(); } }
    }
}
