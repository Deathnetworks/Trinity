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
using Zeta.Game;

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

                result = ShouldStashItem(item);    

                string action = result ? "KEEP" : "TRASH";
                Logger.Log(LogCategory.ItemValuation, "List Item - {0} - {1}", action, item.Name);

                return result;
            }
            return false;
        }

        internal static bool ShouldStashItem(Item item)
        {
            
            return Trinity.Settings.Loot.ItemList.SelectedItems.Any(i => item.Id == i.Id);
        }

    }
}

