using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Trinity.Technicals;
using Zeta.CommonBot.Profile;
using Zeta.TreeSharp;
using Zeta.XmlEngine;
using Action = Zeta.TreeSharp.Action;

namespace Trinity.XmlTags
{
    /// <summary>
    /// Trinity Combat Ignore will let users add a SNO, and optionally specify to exclude elites or trash. This will be reset on every profile load.
    /// </summary>
    [XmlElement("TrinityCombatIgnore")]
    class TrinityCombatIgnore : ProfileBehavior
    {
        private bool isDone;
        public override bool IsDone
        {
            get { return isDone; }
        }

        [XmlAttribute("actorId")]
        [XmlAttribute("actorSNO")]
        [XmlAttribute("actorSno")]
        public int ActorSNO { get; set; }

        [XmlAttribute("actorName")]
        public string ActorName { get; set; }

        [XmlAttribute("exceptElites")]
        public bool ExceptElites { get; set; }

        [XmlAttribute("exceptTrash")]
        public bool ExceptTrash { get; set; }

        protected override Composite CreateBehavior()
        {
            return new Action(ret => AddToIgnoreList());
        }

        private void AddToIgnoreList()
        {
            if (ActorSNO > 0)
            {
                IgnoreList.Add(new CombatIgnoreUnit()
                {
                    ActorSNO = ActorSNO,
                    ActorName = ActorName,
                    ExceptElites = ExceptElites,
                    ExceptTrash = ExceptTrash
                });

                if (!string.IsNullOrWhiteSpace(ActorName))
                    Technicals.Logger.LogNormal("Added {0} ({1}) to Combat Ignore List", ActorName, ActorSNO);
                else
                    Technicals.Logger.LogNormal("Added {0} to Combat Ignore List", ActorSNO);

            }
            else
            {
                Technicals.Logger.LogNormal("Unable to add to Trinity Combat Ignore List", ActorSNO);
            }

            isDone = true;
        }

        public static HashSet<CombatIgnoreUnit> IgnoreList = new HashSet<CombatIgnoreUnit>();

        public class CombatIgnoreUnit
        {
            public int ActorSNO { get; set; }
            public string ActorName { get; set; }
            public bool ExceptElites { get; set; }
            public bool ExceptTrash { get; set; }

            public CombatIgnoreUnit() { }
        }

    }
}
