using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trinity.Helpers;
using Trinity.Objects;
using Trinity.Reference;
using Trinity.Settings.Loot;
using Trinity.Technicals;
using Trinity.UIComponents;
using Zeta.Game;
using Zeta.Game.Internals.Actors;

namespace Trinity.Items
{
    public class ItemList
    {
        internal static bool ShouldStashItem(CachedACDItem cItem)
        {
            if (cItem.AcdItem != null && cItem.AcdItem.IsValid)
            {
                bool result = false;
                var item = new Item(cItem.AcdItem);
                var wrappedItem = new ItemWrapper(cItem.AcdItem);

                result = ShouldStashItem(item, cItem);                

                string action = result ? "KEEP" : "TRASH";

                //Logger.Log("List Item - {0} - {1}", action, item.Name);
                
                return result;
            }
            return false;
        }

        internal static bool ShouldStashItem(Item referenceItem, CachedACDItem cItem)
        {
            if (referenceItem == null)
                return false;

            var itemSetting = Trinity.Settings.Loot.ItemList.SelectedItems.FirstOrDefault(i => referenceItem.Id == i.Id);
            if (itemSetting != null)
            {
                Logger.LogDebug("  >>  {0} is a Selected ListItem with {1} rules", cItem.RealName, itemSetting.Rules.Count);
                
                foreach (var itemRule in itemSetting.Rules)
                {
                    var result = true;
                    double itemValue = 0;
                    double ruleValue = 0;

                    switch (itemRule.ItemProperty)
                    {
                        case ItemProperty.Ancient:
                            itemValue = cItem.IsAncient ? 1 : 0;
                            ruleValue = itemRule.Value;
                            result = cItem.IsAncient == (itemRule.Value == 1);   
                            break;

                        case ItemProperty.PrimaryStat:
                            itemValue = ItemDataUtils.GetMainStatValue(cItem.AcdItem);
                            ruleValue = itemRule.Value;
                            result = itemValue >= ruleValue;
                            break;

                        case ItemProperty.CriticalHitChance:
                            itemValue = cItem.CritPercent;
                            ruleValue = itemRule.Value;
                            result = itemValue >= ruleValue;
                            break;

                        case ItemProperty.CriticalHitDamage:
                            itemValue = cItem.CritDamagePercent;
                            ruleValue = itemRule.Value;
                            result = itemValue >= ruleValue;
                            break;   
                    }

                    Logger.LogDebug("  >>  Evaluated {0} -- {1} (Item: {2} -v- Rule: {3}) = {4}", 
                        cItem.RealName, 
                        itemRule.ItemProperty, 
                        itemValue,
                        ruleValue,
                        result);

                    if (!result)
                        return false;
                }
                return true;
            }
            return false;
        }

    }
}

