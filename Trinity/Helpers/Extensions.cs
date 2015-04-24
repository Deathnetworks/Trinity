using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Media;
using Trinity.Config.Combat;
using Trinity.Reference;
using Trinity.Technicals;
using Zeta.Bot;
using Zeta.Game.Internals.Actors;
using Zeta.Game.Internals.SNO;

namespace Trinity.Helpers
{
    public static class Extensions
    {

        /// <summary>
        /// Allows a nullable backed property and use _field.GetValueOrDefaultAttribute() for [DefaultValue(1)] attribute
        /// </summary>
        public static T GetValueOrDefaultAttribute<T>(this T? obj, [CallerMemberName] string name = "", Type type = null) where T : struct, IComparable
        {
            if (obj.HasValue)
                return obj.Value;

            if (type == null)
            {
                var frame = new StackFrame(1);
                var method = frame.GetMethod();
                if (method.DeclaringType != null)
                {
                    type = method.DeclaringType;
                }
                else
                {
                    return default(T);
                }
            }

            var properties = TypeDescriptor.GetProperties(type)[name];
            if (properties != null)
            {
                var myAttribute = properties.Attributes.OfType<DefaultValueAttribute>().FirstOrDefault();
                if (myAttribute != null)
                {
                    return (T)Convert.ChangeType(myAttribute.Value, typeof(T));
                }
            }
            return default(T);
        }

        /// <summary>
        /// Gets a dictionary value or the default
        /// </summary>
        public static TValue GetValueOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key)
        {
            TValue value;
            return dictionary.TryGetValue(key, out value) ? value : default(TValue);
        }

        /// <summary>
        /// Get an attribute, exceptions get swallowed and default returned
        /// </summary>
        public static T GetAttributeOrDefault<T>(this TrinityCacheObject actor, ActorAttributeType type) where T : struct
        {
            try
            {
                actor.CommonData.GetAttribute<T>(type);
            }
            catch (Exception ex)
            {
                Logger.LogDebug(LogCategory.CacheManagement, "Exception on {0} accessing {1} attribute: {2}", actor.InternalName, type, ex);
            }
            return default(T);
        }

        /// <summary>
        /// Get an attribute, exceptions get swallowed and default returned
        /// </summary>
        public static T GetAttributeOrDefault<T>(this ACD actorACD, ActorAttributeType type) where T : struct
        {
            try
            {
                actorACD.GetAttribute<T>(type);
            }
            catch (Exception ex)
            {
                Logger.LogDebug(LogCategory.CacheManagement, "Exception on {0} accessing {1} attribute: {2}", actorACD.Name, type, ex);
            }
            return default(T);
        }

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

        public static EnumValue<TEnum> ToEnumValue<TEnum>(this TEnum e) where TEnum : struct, IConvertible
        {
            if (!typeof(TEnum).IsEnum)
            {
                throw new ArgumentException("T must be an enumerated type");
            }
            return new EnumValue<TEnum>(e);
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
                case "{c:ff00ff00}": // Green
                    qualityResult = ItemQuality.Legendary;
                    break;
                case "{c:ffff8000}": // Orange
                    qualityResult = ItemQuality.Legendary;
                    break;
                case "{c:ffffff00}": // Yellow
                    qualityResult = ItemQuality.Rare4;
                    break;
                case "{c:ff6969ff}": // Magic Blue 
                    qualityResult = ItemQuality.Magic1;
                    break;
                case "{c:ffffffff}": // White
                    qualityResult = ItemQuality.Normal;
                    break;
                // got this off a "lore book" - not sure what it actually equates to
                case "{c:ff99bbff}": // Gem Blue
                    qualityResult = ItemQuality.Normal;
                    break;
                case "{c:ffc236ff}": // Purple
                    qualityResult = ItemQuality.Special;
                    break;
                case "{c:ff888888}": // Gray
                    qualityResult = ItemQuality.Inferior;
                    break;
                case "":
                    qualityResult = item.ItemQualityLevel;
                    break;
                default:
                    Logger.Log("Invalid Item Link color={0} internalName={1} name={2} gameBalanceId={3}", linkColor, item.InternalName, item.Name, item.GameBalanceId);
                    qualityResult = item.ItemQualityLevel;
                    break;
            }

            return qualityResult;
        }

        public static bool IsSetItem(this ACDItem item)
        {
            if (item == null)
                return false;
            if (!item.IsValid)
                return false;

            string itemLink = item.ItemLink;

            string linkColor = ItemQualityRegex.Match(itemLink).Value;

            if (linkColor == "{c:ff00ff00}")
                return true;

            return false;
        }

        public static string ItemSetName(this ACDItem item)
        {
            if (!item.IsSetItem())
                return null;

            var set = Sets.Where(s => s.ItemIds.Contains(item.ActorSNO)).FirstOrDefault();
            if (set != null)
                return set.Name;
            
            return null;
        }

        public static int GetGemQualityLevel(this ACDItem item)
        {
            if (item == null)
                return 0;
            if (!item.IsValid)
                return 0;

            // Imperial Gem hax
            if (item.InternalName.EndsWith("_16"))
                return 68;

            return item.Level;
        }
        public static double GetNavCellSize(this NavCell cell)
        {
            var diff = cell.Max.ToVector2() - cell.Min.ToVector2();
            return diff.X * diff.Y;
        }

        /// <summary>
        /// Returns if a DiaObject is not null, is valid, and it's ACD is not null, and is valid
        /// </summary>
        /// <param name="diaObject"></param>
        /// <returns></returns>
        public static bool IsFullyValid(this DiaObject diaObject)
        {
            return diaObject != null && diaObject.IsValid && diaObject.ACDGuid != 0 && diaObject.CommonData != null && diaObject.CommonData.IsValid;
        }

        /// <summary>
        /// Removed duplicates from a list based on specified property .DistinctBy(o => o.property)
        /// </summary>
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>
            (this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            var seenKeys = new HashSet<TKey>();
            return source.Where(element => seenKeys.Add(keySelector(element)));
        }

        /// <summary>
        /// Splits a StringLikeThisWithCapitalLetters into words with spaces between.
        /// </summary>
        public static string AddSpacesToSentence(this string text, bool preserveAcronyms = false)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;
            StringBuilder newText = new StringBuilder(text.Length * 2);
            newText.Append(text[0]);
            for (int i = 1; i < text.Length; i++)
            {
                if (char.IsUpper(text[i]))
                    if ((text[i - 1] != ' ' && !char.IsUpper(text[i - 1])) ||
                        (preserveAcronyms && char.IsUpper(text[i - 1]) &&
                         i < text.Length - 1 && !char.IsUpper(text[i + 1])))
                        newText.Append(' ');
                newText.Append(text[i]);
            }
            return newText.ToString();
        }
    }
}
