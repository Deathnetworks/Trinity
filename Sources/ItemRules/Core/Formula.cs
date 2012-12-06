using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GilesTrinity.ItemRules.Core
{
    class Formula
    {


        public float getItemEHP(float armor, float resistance, float vitality, float monsterlvl)
        {
            //Armor Damage Reduction = Armor/(50*Monster_Level + Armor)
            float armorDamageReduction = armor / (50 * monsterlvl + armor);
            //Resistance Damage Reduction = Resistance/(5*Monster_Level + Armor)
            float resistanceDamageReduction = resistance / (5 * monsterlvl + armorDamageReduction);
            //Total Damage Reduction = 1-(1-Armor Damage Reduction) * (1-Resistance Damage Reduction)
            float totalDamageReduction = 1 - (1 - armorDamageReduction) * (1 - resistanceDamageReduction);
            //HP = Vitality * 35
            float baseHealth = vitality * 35;
            //eHP = HP / (1-Total Damage Reduction)
            float eHP = baseHealth / (1 - totalDamageReduction);

            return eHP;
        }
    }
}
