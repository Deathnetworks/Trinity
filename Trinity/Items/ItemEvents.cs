using Zeta.Bot;
using Zeta.Game.Internals.Actors;
namespace Trinity
{
    public class ItemEvents
    {
        internal static void TrinityOnItemStashed(object sender, ItemEventArgs e)
        {
            ACDItem i = e.Item;

            if (!i.IsValid)
                return;

            var cachedItem = CachedACDItem.GetCachedItem(i);

            ResetTownRun();

            switch (i.ItemBaseType)
            {
                case ItemBaseType.Gem:
                case ItemBaseType.Misc:
                    break;
                default:
                    TownRun.LogGoodItems(cachedItem, cachedItem.TrinityItemBaseType, cachedItem.TrinityItemType, ItemValuation.ValueThisItem(cachedItem, cachedItem.TrinityItemType));
                    break;
            }
        }

        internal static void TrinityOnItemSalvaged(object sender, ItemEventArgs e)
        {
            ACDItem i = e.Item;

            var cachedItem = CachedACDItem.GetCachedItem(i);

            ResetTownRun();
            switch (i.ItemBaseType)
            {
                case ItemBaseType.Gem:
                case ItemBaseType.Misc:
                    break;
                default:
                    TownRun.LogJunkItems(cachedItem, cachedItem.TrinityItemBaseType, cachedItem.TrinityItemType, ItemValuation.ValueThisItem(cachedItem, cachedItem.TrinityItemType));
                    break;
            }
        }

        internal static void TrinityOnItemSold(object sender, ItemEventArgs e)
        {
            ACDItem i = e.Item;

            var cachedItem = CachedACDItem.GetCachedItem(i);

            ResetTownRun();

            switch (i.ItemBaseType)
            {
                case ItemBaseType.Gem:
                case ItemBaseType.Misc:
                    break;
                default:
                    TownRun.LogJunkItems(cachedItem, cachedItem.TrinityItemBaseType, cachedItem.TrinityItemType, ItemValuation.ValueThisItem(cachedItem, cachedItem.TrinityItemType));
                    break;
            }
        }

        internal static void TrinityOnOnItemIdentificationRequest(object sender, ItemIdentifyRequestEventArgs e)
        {
            e.IgnoreIdentification = !Trinity.ItemRulesIdentifyValidation(e.Item);
        }

        internal static void ResetTownRun()
        {
            ItemValuation.ResetValuationStatStrings();
            TownRun.TownRunCheckTimer.Reset();
            Trinity.ForceVendorRunASAP = false;
            Trinity.IsReadyToTownRun = false;
        }


    }
}
