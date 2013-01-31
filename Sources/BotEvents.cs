using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Zeta.CommonBot;
using Zeta.Internals.Actors;

namespace GilesTrinity
{
    public partial class GilesTrinity
    {
        private void TrinityOnItemStashed(object sender, ItemEventArgs e)
        {
            ACDItem i = e.Item;

            var cachedItem = GilesCachedACDItem.GetCachedItem(i);

            ResetTownRun();

            switch (i.ItemBaseType)
            {
                case ItemBaseType.Gem:
                case ItemBaseType.Misc:
                    break;
                default:
                    TownRun.LogGoodItems(cachedItem, cachedItem.TrinityItemBaseType, cachedItem.TrinityItemType, GilesTrinity.ValueThisItem(cachedItem, cachedItem.TrinityItemType));
                    break;
            }
        }

        private void TrinityOnItemSalvaged(object sender, ItemEventArgs e)
        {
            ACDItem i = e.Item;

            var cachedItem = GilesCachedACDItem.GetCachedItem(i);

            ResetTownRun();
            switch (i.ItemBaseType)
            {
                case ItemBaseType.Gem:
                case ItemBaseType.Misc:
                    break;
                default:
                    TownRun.LogJunkItems(cachedItem, cachedItem.TrinityItemBaseType, cachedItem.TrinityItemType, GilesTrinity.ValueThisItem(cachedItem, cachedItem.TrinityItemType));
                    break;
            }
        }

        private void TrinityOnItemSold(object sender, ItemEventArgs e)
        {
            ACDItem i = e.Item;

            var cachedItem = GilesCachedACDItem.GetCachedItem(i);

            ResetTownRun();

            switch (i.ItemBaseType)
            {
                case ItemBaseType.Gem:
                case ItemBaseType.Misc:
                    break;
                default:
                    TownRun.LogJunkItems(cachedItem, cachedItem.TrinityItemBaseType, cachedItem.TrinityItemType, GilesTrinity.ValueThisItem(cachedItem, cachedItem.TrinityItemType));
                    break;
            }
        }

        private static void ResetTownRun()
        {
            ResetValuationStatStrings();
            TownRun.TownRunCheckTimer.Reset();
            ForceVendorRunASAP = false;
            IsReadyToTownRun = false;
        }


    }
}
