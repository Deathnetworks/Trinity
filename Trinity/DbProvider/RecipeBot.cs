using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trinity.Items;
using Zeta.Bot;
using Zeta.Bot.Coroutines;
using Zeta.Bot.Navigation;
using Zeta.Common;
using Zeta.Game;
using Zeta.Game.Internals;
using Zeta.Game.Internals.Actors;
using Zeta.Game.Internals.Actors.Gizmos;
using Zeta.TreeSharp;

namespace Trinity.DbProvider
{
    public class RecipeBot
    {
        public Composite RecipeBotComposite()
        {
            return new ActionRunCoroutine(ret => RecipeBotRunner());
        }

        public static async Task<bool> RecipeBotRunner()
        {
            if (!ZetaDia.IsInTown)
                return false;

            // For Smith plans, move to blacksmith
            // Use Item if not already known
            if (HasSmithPlans && Forge != null && UIElements.SalvageWindow.IsVisible)
            {
                ZetaDia.Me.Inventory.UseItem(BackPackSmithPlans.FirstOrDefault().DynamicId);
                return true;
            }
            // Open Blacksmith window
            if (HasSmithPlans && Forge != null && !UIElements.SalvageWindow.IsVisible)
            {
                if (Forge.Distance > 10f)
                {
                    var moveResult = await CommonCoroutines.MoveAndStop(Forge.Position, 10f, "Forge Location");
                    if (moveResult == MoveResult.ReachedDestination)
                    {
                        return false;
                    }
                }
                else
                {
                    Forge.Interact();
                    return true;
                }

            }

            // Upgrade Blacksmith to max.. not sure if we can do that?
            // If already known: Move to Vendor
            // Sell Item

            // For Jeweler plans
            // Move to Jeweler
            // Open Window
            // Upgrade Jeweler to max
            // Use Item if not already known
            // If already Known: move to vendor
            // Sell Item

            if (await MoveToAndOpenStash())
                return true;

            // Couldn't move to stash, something derpd?
            if (Stash == null)
                return false;

            // Transfer Plans to backpack until pack full
            while (UIElements.StashWindow.IsVisible && TrinityItemManager.FindValidBackpackLocation(false) != TrinityItemManager.NoFreeSlot)
            {
                ZetaDia.Me.Inventory.QuickWithdraw(FirstStashPlan);
            }


            return false;
        }

        public static async Task<bool> MoveToAndOpenStash()
        {
            // Move to Stash
            if (Stash == null || (Stash != null && Stash.Position.Distance2D(ZetaDia.Me.Position) > 10f))
            {
                var moveResult = await CommonCoroutines.MoveTo(TownRun.StashLocation, "Shared Stash Location");
                if (moveResult == MoveResult.ReachedDestination)
                {
                    return false;
                }
                if (moveResult == MoveResult.Moved)
                {
                    return true;
                }
            }
            if (Stash == null)
                return false;

            if (!UIElements.StashWindow.IsVisible)
            {
                Stash.Interact();
                return true;
            }
            return false;
        }


        // [1CB30FA4] Type: Monster Name: PT_Blacksmith_ForgeWeaponShortcut-6673 ActorSNO: 195171, Distance: 7.695145
        const int ForgeActorSNO = 195171;
        public static DiaUnit Forge
        {
            get
            {
                return ZetaDia.Actors.GetActorsOfType<DiaUnit>(true).FirstOrDefault(i => i.IsValid && i.CommonData != null && i.ActorSNO == ForgeActorSNO);
            }
        }

        public static GizmoPlayerSharedStash Stash
        {
            get
            {
                return ZetaDia.Actors.GetActorsOfType<GizmoPlayerSharedStash>(true).FirstOrDefault();
            }
        }

        public static ACDItem FirstStashPlan
        {
            get
            {
                return ZetaDia.Me.Inventory.StashItems.FirstOrDefault(i => i.IsValid && i.ItemType == ItemType.CraftingPlan);
            }
        }

        public static bool HasSmithPlans
        {
            get { return BackPackSmithPlans.Any(); }
        }

        public static bool HasJewelerPlans
        {
            get { return BackPackJewelerPlans.Any(); }
        }

        public static List<ItemWrapper> BackPackSmithPlans
        {
            get { return Plans.Where(i => i.InventorySlot == InventorySlot.BackpackItems && i.Name.StartsWith("CraftingPlan_Smith")).ToList(); }
        }

        public static List<ItemWrapper> BackPackJewelerPlans
        {
            get { return Plans.Where(i => i.InventorySlot == InventorySlot.BackpackItems && i.Name.StartsWith("CraftingPlan_Jeweler")).ToList(); }
        }

        public static List<ItemWrapper> Plans
        {
            get
            {
                //CraftingPlan_Smith
                //CraftingPlan_Jeweler
                var itemsList = ZetaDia.Actors.GetActorsOfType<ACDItem>().Where(i => i.IsValid).Select(i => new ItemWrapper(i)).ToList();

                List<ItemWrapper> plans = itemsList.Where(wrappedItem => wrappedItem.InternalName.Contains("CraftingPlan_")).ToList();

                return plans;
            }
        }

    }
}
