using System;
using System.Windows;
using Zeta;
using Zeta.CommonBot;
using Zeta.Internals.Actors;
using Zeta.TreeSharp;
using GilesTrinity;

namespace GilesTrinity
{
    public class GilesRoutine : CombatRoutine
    {
        public override void Initialize()
        {
        }

        public override void Dispose()
        {
        }

        public override string Name { get { return GilesTrinity.Instance.Name; } }

        public override Window ConfigWindow { get { return GilesTrinity.Instance.DisplayWindow; } }

        public override ActorClass Class { get { return ZetaDia.Me.ActorClass; } }

        public override SNOPower DestroyObjectPower { get { return ZetaDia.Me.GetHotbarPowerId(HotbarSlot.HotbarMouseLeft); } }

        public override float DestroyObjectDistance { get { return 15; } }

        /*private Composite _combat;
        private Composite _buff;*/
        public override Composite Combat { get { return new PrioritySelector(); } }
        public override Composite Buff { get { return new PrioritySelector(); } }

    }
}
