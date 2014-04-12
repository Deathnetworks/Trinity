using System.Text.RegularExpressions;
using Trinity.Config.Combat;
using Trinity.Technicals;
using Zeta.Game.Internals.Actors;

namespace Trinity.Helpers
{
    public static class Extensions
    {
        public static TrinityItemQuality GetTrinityItemQuality(this ACDItem item)
        {
            if (item == null)
                return TrinityItemQuality.None;
            if (!item.IsValid)
                return TrinityItemQuality.None;

            var itemQuality = item.ItemLinkColorQuality();

            switch (itemQuality)
            {
                case ItemQuality.Invalid:
                    return TrinityItemQuality.None;
                case ItemQuality.Inferior:
                case ItemQuality.Normal:
                case ItemQuality.Superior:
                    return TrinityItemQuality.Common;
                case ItemQuality.Magic1:
                case ItemQuality.Magic2:
                case ItemQuality.Magic3:
                    return TrinityItemQuality.Magic;
                case ItemQuality.Rare4:
                case ItemQuality.Rare5:
                case ItemQuality.Rare6:
                    return TrinityItemQuality.Rare;
                case ItemQuality.Legendary:
                case ItemQuality.Special:
                default:
                    return TrinityItemQuality.Legendary;

            }
        }

        private static Regex ItemQualityRegex = new Regex("{c:[a-zA-Z0-9]{8}}", RegexOptions.Compiled);

        public static ItemQuality ItemLinkColorQuality(this ACDItem item)
        {
            if (item == null)
                return ItemQuality.Invalid;
            if (!item.IsValid)
                return ItemQuality.Invalid;

            /*
            {c:ff00ff00} = Set
            {c:ffff8000} = Legendary
            {c:ffffff00} = Rare
            {c:ff6969ff} = Magic
             */

            string itemLink = item.ItemLink;

            string linkColor = ItemQualityRegex.Match(itemLink).Value;

            ItemQuality qualityResult;
            string itemLinkLog = itemLink.Replace("{", "{{").Replace("}", "}}");

            switch (linkColor)
            {
                case "{c:ff00ff00}":
                    qualityResult = ItemQuality.Legendary;
                    break;
                case "{c:ffff8000}":
                    qualityResult = ItemQuality.Legendary;
                    break;
                case "{c:ffffff00}":
                    qualityResult = ItemQuality.Rare4;
                    break;
                case "{c:ff6969ff}":
                    qualityResult = ItemQuality.Magic1;
                    break;
                case "{c:ffffffff}":
                    qualityResult = ItemQuality.Normal;
                    break;
                    // got this off a "lore book" - not sure what it actually equates to
                case "{c:ff99bbff}":
                    qualityResult = ItemQuality.Normal;
                    break;
                case "":
                    qualityResult = ItemQuality.Invalid;
                    break;
                default:
                    Logger.Log("Invalid Item Link color={0} internalName={1} name={2} gameBalanceId={3}", linkColor, item.InternalName, item.Name, item.GameBalanceId);
                    qualityResult = item.ItemQualityLevel;
                    break;
            }

            return qualityResult;
        }
    }
}
