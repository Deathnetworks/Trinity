using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trinity.Items;
using Zeta.Game;
using Zeta.Game.Internals.Actors;

namespace Trinity.DbProvider
{
    public class RecipeBot
    {
        public static async Task<bool> RecipeBotRunner()
        {
            if (!ZetaDia.IsInTown)
                return false;

            //CraftingPlan_Smith
            //CraftingPlan_Jeweler
            var itemsList = ZetaDia.Actors.GetActorsOfType<ACDItem>().Where(i => i.IsValid).Select(i => new ItemWrapper(i)).ToList();

            List<ItemWrapper> plans = itemsList.Where(wrappedItem => wrappedItem.InternalName.Contains("CraftingPlan_")).ToList();

            // Move to Stash
            // Transfer Plans to backpack until pack full

            // For Smith plans, move to blacksmith
            // Open Blacksmith window
            // Upgrade Blacksmith to max
            // Use Item if not already known
            // If already known: Move to Vendor
            // Sell Item

            // For Jeweler plans
            // Move to Jeweler
            // Open Window
            // Upgrade Jeweler to max
            // Use Item if not already known
            // If already Known: move to vendor
            // Sell Item

            // Repeat

            return false;
        }
    }
}
