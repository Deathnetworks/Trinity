using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zeta.Game.Internals.Actors;

namespace Trinity.Helpers
{
    public static class GetAttributeExtensions
    {
        public static double SkillDamagePercent(this ACDItem acdItem, SNOPower power)
        {
            return Math.Round(acdItem.GetAttribute<float>(((int) power << 12) + ((int) ActorAttributeType.PowerDamagePercentBonus & 0xFFF))*100, MidpointRounding.AwayFromZero);
        }

        public static double WeaponDamagePercent(this ACDItem acdItem)
        {
            return Math.Round(acdItem.GetAttribute<float>(ActorAttributeType.DamageWeaponPercentAll) *100, MidpointRounding.AwayFromZero);
        }
    }
}
