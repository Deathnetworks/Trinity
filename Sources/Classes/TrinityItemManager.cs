using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Zeta.CommonBot;
using Zeta.CommonBot.Items;
using Zeta.Internals.Actors;

namespace GilesTrinity.Classes
{
    public class TrinityItemManager : ItemManager
    {
        public TrinityItemManager()
        {

        }

        private RuleTypePriority _priority = null;
        public override RuleTypePriority Priority
        {
            get
            {
                if (_priority == null)
                {
                    _priority = new RuleTypePriority();
                    _priority.Priority1 = ItemEvaluationType.Keep;
                    _priority.Priority2 = ItemEvaluationType.Salvage;
                    _priority.Priority3 = ItemEvaluationType.Sell;
                }
                return _priority;
            }
        }

        public override bool EvaluateItem(ACDItem item, ItemEvaluationType evaluationType)
        {
            return false;
        }
        //public override bool ItemIsProtected(ACDItem item)
        //{
        //    return false;
        //}
        public override bool ShouldPickUpItem(ACDItem item)
        {
            return false;
        }
        public override bool ShouldSalvageItem(ACDItem item)
        {
            return false;
        }
        public override bool ShouldSellItem(ACDItem item)
        {
            return false;
        }
        public override bool ShouldStashItem(ACDItem item)
        {
            return false;
        }

    }
}
