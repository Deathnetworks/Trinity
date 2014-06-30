using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zeta.Game;
using Zeta.Game.Internals.Actors;

namespace Trinity.Objects
{
    /// <summary>
    /// Contains information about a Passive
    /// </summary>
    public class Passive
    {
        public int Index { get; set; }
        public SNOPower SNOPower { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int RequiredLevel { get; set; }
        public string Tooltip { get; set; }
        public string Slug { get; set; }
        public ActorClass Class { get; set; }

        public Passive()
        {
            Index = 0;
            SNOPower = SNOPower.None;
            Name = string.Empty;
            Description = string.Empty;
            RequiredLevel = 0;
            Tooltip = string.Empty;
            Slug = string.Empty;
            Class = ActorClass.Invalid;
        }

        public override int GetHashCode()
        {
            return Index.GetHashCode() ^ Name.GetHashCode();
        }
    }
}
