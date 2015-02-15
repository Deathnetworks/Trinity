using System;
using System.Collections.Generic;
using System.Linq;
using Trinity.Helpers;
using Trinity.Items;
using Trinity.Objects;
using Trinity.Settings.Loot;
using Trinity.Technicals;
using Zeta.Game;
using Zeta.Game.Internals.Actors;
using Logger = Trinity.Technicals.Logger;

namespace Trinity.Reference
{
    public class ItemRanks : FieldCollection<ItemRanks, ItemRank>
    {
        #region Methods
        internal static bool ShouldStashItem(CachedACDItem cItem)
        {
            if (cItem.AcdItem != null && cItem.AcdItem.IsValid)
            {
                bool result = false;
                var item = new Item(cItem.AcdItem);
                var wrappedItem = new ItemWrapper(cItem.AcdItem);

                if (Trinity.Settings.Loot.ItemRank.AncientItemsOnly && wrappedItem.IsEquipment && !cItem.IsAncient)
                {
                    result = false;
                }
                else if (Trinity.Settings.Loot.ItemRank.RequireSocketsOnJewelry && wrappedItem.IsJewlery && cItem.AcdItem.NumSockets != 0)
                {
                    result = false;
                }
                else
                {
                    result = ShouldStashItem(item);
                }
                string action = result ? "KEEP" : "TRASH";
                Logger.Log(LogCategory.ItemValuation, "Ranked Item - {0} - {1}", action, item.Name);

                return result;
            }
            return false;
        }

        internal static bool ShouldStashItem(Item item)
        {
            return GetRankedItemsFromSettings().Any(i => i.Item.Equals(item));
        }

        private static HashSet<int> _itemIds;

        public static HashSet<int> ItemIds
        {
            get
            {
                return _itemIds ?? (_itemIds = new HashSet<int>(ToList()
                    .Select(i => i.Item.Id)));
            }
        }

        public static IEnumerable<ItemRank> GetRankedItems(ActorClass actorClass, double minPercent = 10, int minSampleSize = 10, int betterThanRank = 5)
        {
            var list = ToList().Where(i => i.SoftcoreRank.Any(ird =>
                ird.Rank <= betterThanRank &&
                ird.Class == actorClass &&
                ird.PercentUsed >= minPercent &&
                ird.SampleSize >= minSampleSize));

            List<ItemRank> result = new List<ItemRank>();
            foreach (var ir in list)
            {
                var newIr = new ItemRank { Item = ir.Item };
                foreach (var ird in ir.SoftcoreRank.Where(ird => ird != null && ird.Rank <= betterThanRank && ird.Class == actorClass && ird.PercentUsed >= minPercent && ird.SampleSize >= minSampleSize))
                {
                    newIr.SoftcoreRank.Add(ird);
                }
                foreach (var ird in ir.HardcoreRank.Where(ird => ird != null && ird.Rank <= betterThanRank && ird.Class == actorClass && ird.PercentUsed >= minPercent && ird.SampleSize >= minSampleSize))
                {
                    newIr.HardcoreRank.Add(ird);
                }
                if (newIr.SoftcoreRank.Any())
                    result.Add(newIr);
            }

            return result;
        }

        public static HashSet<int> GetRankedIds(ActorClass actorClass, double minPercent = 10, int minSampleSize = 10, int betterThanRank = 5)
        {
            return new HashSet<int>(GetRankedItems(actorClass, minPercent, minSampleSize, betterThanRank).Select(v => v.Item.Id));
        }

        private static double lastSettingSignature;
        private static List<ItemRank> LastRankedItemsList = new List<ItemRank>();
        public static List<ItemRank> GetRankedItemsFromSettings()
        {
            var irs = Trinity.Settings.Loot.ItemRank;
            var settingSignature = (int)Trinity.Player.ActorClass + (int)irs.ItemRankMode + irs.MinimumRank + irs.MinimumSampleSize + irs.MinimumPercent;
            if (settingSignature == lastSettingSignature)
                return LastRankedItemsList;

            lastSettingSignature = settingSignature;
            LastRankedItemsList = GetRankedItemsFromSettings(Trinity.Settings.Loot.ItemRank);
            return LastRankedItemsList;
        }

        public static List<ItemRank> GetRankedItemsFromSettings(ItemRankSettings irs)
        {
            List<ItemRank> ird = new List<ItemRank>();

            if (irs.ItemRankMode == ItemRankMode.AnyClass)
            {
                foreach (ActorClass actor in Enum.GetValues(typeof(ActorClass)).Cast<ActorClass>())
                {
                    if (actor == ActorClass.Invalid)
                        continue;
                    foreach (ItemRank itemRank in GetRankedItems(actor, irs.MinimumPercent, irs.MinimumSampleSize, irs.MinimumRank))
                    {
                        ird.Add(itemRank);
                    }
                }
            }
            else if (ZetaDia.Me.IsFullyValid())
            {
                ird.AddRange(GetRankedItems(ZetaDia.Me.ActorClass, irs.MinimumPercent, irs.MinimumSampleSize, irs.MinimumRank));
            }
            return ird;
        }

        private static Dictionary<int, ItemRank> _itemRankDictionary = new Dictionary<int, ItemRank>();

        public static ItemRank GetItemRank(ACDItem item)
        {
            if (!_itemRankDictionary.Any())
                _itemRankDictionary = ToList().ToDictionary(k => k.Item.Id, k => k);

            ItemRank itemRank;

            if (_itemRankDictionary.TryGetValue(item.ActorSNO, out itemRank))
            {
                return itemRank;
            }

            return null;
        }

        private static Dictionary<ActorClass, HashSet<int>> _highRankedIds;

        public static Dictionary<ActorClass, HashSet<int>> HighRankedIds
        {
            get
            {
                if (_highRankedIds != null)
                    return _highRankedIds;

                _highRankedIds = new Dictionary<ActorClass, HashSet<int>>();

                foreach (ActorClass actorClass in (ActorClass[])Enum.GetValues(typeof(ActorClass)))
                {
                    var irs = GetRankedItems(actorClass, 15, 10, 4);

                    var idsHash = new HashSet<int>(irs.Select(v => v.Item.Id));

                    _highRankedIds.Add(actorClass, idsHash);
                }

                return _highRankedIds;
            }
        }

        #endregion



        // AUTO-GENERATED on Sun, 15 Feb 2015 19:03:05 GMT

        public static ItemRank AughildsSearch = new ItemRank
        {
            Item = Legendary.AughildsSearch,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 464, PercentUsed = 46.49, Rank = 1, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 310, PercentUsed = 31.28, Rank = 1, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 78, PercentUsed = 7.81, Rank = 3, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 694, PercentUsed = 69.61, Rank = 1, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 243, PercentUsed = 24.30, Rank = 2, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 71, PercentUsed = 7.14, Rank = 3, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 547, PercentUsed = 54.75, Rank = 1, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 120, PercentUsed = 12.00, Rank = 3, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 13, PercentUsed = 1.30, Rank = 8, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 371, PercentUsed = 37.10, Rank = 2, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 51, PercentUsed = 5.10, Rank = 3, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 8, PercentUsed = 0.80, Rank = 6, }, 

            }
        };
        public static ItemRank ReapersWraps = new ItemRank
        {
            Item = Legendary.ReapersWraps,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 216, PercentUsed = 21.64, Rank = 2, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 123, PercentUsed = 12.41, Rank = 3, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 424, PercentUsed = 42.44, Rank = 1, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 20, PercentUsed = 2.01, Rank = 4, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 79, PercentUsed = 7.90, Rank = 3, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 451, PercentUsed = 45.37, Rank = 1, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 140, PercentUsed = 14.01, Rank = 2, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 22, PercentUsed = 2.20, Rank = 6, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 611, PercentUsed = 61.10, Rank = 1, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 5, PercentUsed = 0.50, Rank = 10, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 15, PercentUsed = 1.50, Rank = 9, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 141, PercentUsed = 14.10, Rank = 2, }, 

            }
        };
        public static ItemRank StrongarmBracers = new ItemRank
        {
            Item = Legendary.StrongarmBracers,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 115, PercentUsed = 11.52, Rank = 3, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 269, PercentUsed = 27.14, Rank = 2, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 45, PercentUsed = 4.50, Rank = 6, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 173, PercentUsed = 17.35, Rank = 2, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 433, PercentUsed = 43.30, Rank = 1, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 343, PercentUsed = 34.51, Rank = 2, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 102, PercentUsed = 10.21, Rank = 3, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 201, PercentUsed = 20.10, Rank = 2, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 15, PercentUsed = 1.50, Rank = 7, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 429, PercentUsed = 42.90, Rank = 1, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 700, PercentUsed = 70.00, Rank = 1, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 711, PercentUsed = 71.10, Rank = 1, }, 

            }
        };
        public static ItemRank SlaveBonds = new ItemRank
        {
            Item = Legendary.SlaveBonds,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 27, PercentUsed = 2.71, Rank = 4, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 30, PercentUsed = 3.03, Rank = 6, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 166, PercentUsed = 16.62, Rank = 2, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 9, PercentUsed = 0.90, Rank = 8, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 62, PercentUsed = 6.20, Rank = 4, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 24, PercentUsed = 2.41, Rank = 4, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 12, PercentUsed = 1.20, Rank = 9, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 6, PercentUsed = 0.60, Rank = 10, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 18, PercentUsed = 1.80, Rank = 6, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 10, PercentUsed = 1.00, Rank = 8, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 26, PercentUsed = 2.60, Rank = 7, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 5, PercentUsed = 0.50, Rank = 8, }, 

            }
        };
        public static ItemRank LacuniProwlers = new ItemRank
        {
            Item = Legendary.LacuniProwlers,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 24, PercentUsed = 2.40, Rank = 5, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 12, PercentUsed = 1.21, Rank = 10, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 73, PercentUsed = 7.31, Rank = 4, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 14, PercentUsed = 1.40, Rank = 6, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 16, PercentUsed = 1.60, Rank = 8, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 18, PercentUsed = 1.81, Rank = 5, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 12, PercentUsed = 1.20, Rank = 8, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 14, PercentUsed = 1.40, Rank = 7, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 86, PercentUsed = 8.60, Rank = 3, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 49, PercentUsed = 4.90, Rank = 3, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 33, PercentUsed = 3.30, Rank = 5, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 6, PercentUsed = 0.60, Rank = 7, }, 

            }
        };
        public static ItemRank WarzechianArmguards = new ItemRank
        {
            Item = Legendary.WarzechianArmguards,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 22, PercentUsed = 2.20, Rank = 6, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 27, PercentUsed = 2.72, Rank = 7, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 16, PercentUsed = 1.60, Rank = 10, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 7, PercentUsed = 0.70, Rank = 10, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 32, PercentUsed = 3.20, Rank = 6, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 48, PercentUsed = 4.80, Rank = 5, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 27, PercentUsed = 2.70, Rank = 5, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 12, PercentUsed = 1.20, Rank = 9, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 11, PercentUsed = 1.10, Rank = 7, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 58, PercentUsed = 5.80, Rank = 2, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 5, PercentUsed = 0.50, Rank = 10, }, 

            }
        };
        public static ItemRank AncientParthanDefenders = new ItemRank
        {
            Item = Legendary.AncientParthanDefenders,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 17, PercentUsed = 1.70, Rank = 7, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 69, PercentUsed = 6.96, Rank = 4, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 13, PercentUsed = 1.30, Rank = 9, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 8, PercentUsed = 0.80, Rank = 8, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 10, PercentUsed = 1.00, Rank = 10, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 478, PercentUsed = 47.80, Rank = 1, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 6, PercentUsed = 0.60, Rank = 9, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 14, PercentUsed = 1.40, Rank = 10, }, 

            }
        };
        public static ItemRank NemesisBracers = new ItemRank
        {
            Item = Legendary.NemesisBracers,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 17, PercentUsed = 1.70, Rank = 8, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 18, PercentUsed = 1.82, Rank = 9, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 38, PercentUsed = 3.80, Rank = 7, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 10, PercentUsed = 1.00, Rank = 7, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 34, PercentUsed = 3.40, Rank = 5, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 12, PercentUsed = 1.21, Rank = 7, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 59, PercentUsed = 5.91, Rank = 4, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 72, PercentUsed = 7.20, Rank = 4, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 32, PercentUsed = 3.20, Rank = 5, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 34, PercentUsed = 3.40, Rank = 4, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 42, PercentUsed = 4.20, Rank = 4, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 46, PercentUsed = 4.60, Rank = 3, }, 

            }
        };
        public static ItemRank SteadyStrikers = new ItemRank
        {
            Item = Legendary.SteadyStrikers,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 15, PercentUsed = 1.50, Rank = 9, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 61, PercentUsed = 6.11, Rank = 5, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 22, PercentUsed = 2.21, Rank = 3, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 12, PercentUsed = 1.20, Rank = 10, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 7, PercentUsed = 0.70, Rank = 9, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 19, PercentUsed = 1.90, Rank = 6, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 10, PercentUsed = 1.00, Rank = 8, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 144, PercentUsed = 14.40, Rank = 2, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 30, PercentUsed = 3.00, Rank = 5, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 30, PercentUsed = 3.00, Rank = 6, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 16, PercentUsed = 1.60, Rank = 5, }, 

            }
        };
        public static ItemRank PromiseOfGlory = new ItemRank
        {
            Item = Legendary.PromiseOfGlory,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 13, PercentUsed = 1.30, Rank = 10, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {

            }
        };
        public static ItemRank SunwukosCrown = new ItemRank
        {
            Item = Legendary.SunwukosCrown,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 354, PercentUsed = 35.47, Rank = 1, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 355, PercentUsed = 35.54, Rank = 1, }, 

            }
        };
        public static ItemRank LeoricsCrown = new ItemRank
        {
            Item = Legendary.LeoricsCrown,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 103, PercentUsed = 10.32, Rank = 2, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 103, PercentUsed = 10.39, Rank = 4, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 12, PercentUsed = 1.20, Rank = 6, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 7, PercentUsed = 0.70, Rank = 8, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 36, PercentUsed = 3.60, Rank = 5, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 399, PercentUsed = 40.14, Rank = 2, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 165, PercentUsed = 16.52, Rank = 2, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 404, PercentUsed = 40.40, Rank = 1, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 25, PercentUsed = 2.50, Rank = 4, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 9, PercentUsed = 0.90, Rank = 5, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 41, PercentUsed = 4.10, Rank = 2, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 583, PercentUsed = 58.30, Rank = 1, }, 

            }
        };
        public static ItemRank TzoKrinsGaze = new ItemRank
        {
            Item = Legendary.TzoKrinsGaze,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 83, PercentUsed = 8.32, Rank = 3, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 94, PercentUsed = 9.41, Rank = 4, }, 

            }
        };
        public static ItemRank InnasRadiance = new ItemRank
        {
            Item = Legendary.InnasRadiance,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 81, PercentUsed = 8.12, Rank = 4, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 102, PercentUsed = 10.21, Rank = 3, }, 

            }
        };
        public static ItemRank MaskOfTheSearingSky = new ItemRank
        {
            Item = Legendary.MaskOfTheSearingSky,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 75, PercentUsed = 7.52, Rank = 5, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 56, PercentUsed = 5.61, Rank = 5, }, 

            }
        };
        public static ItemRank TheEyeOfTheStorm = new ItemRank
        {
            Item = Legendary.TheEyeOfTheStorm,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 64, PercentUsed = 6.41, Rank = 6, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 45, PercentUsed = 4.50, Rank = 7, }, 

            }
        };
        public static ItemRank EyeOfPeshkov = new ItemRank
        {
            Item = Legendary.EyeOfPeshkov,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 47, PercentUsed = 4.71, Rank = 7, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 47, PercentUsed = 4.70, Rank = 6, }, 

            }
        };
        public static ItemRank AughildsSpike = new ItemRank
        {
            Item = Legendary.AughildsSpike,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 22, PercentUsed = 2.20, Rank = 8, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 39, PercentUsed = 3.94, Rank = 7, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 24, PercentUsed = 2.40, Rank = 5, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 47, PercentUsed = 4.70, Rank = 4, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 27, PercentUsed = 2.72, Rank = 5, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 2, PercentUsed = 0.20, Rank = 8, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 2, PercentUsed = 0.20, Rank = 9, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 12, PercentUsed = 1.20, Rank = 5, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 3, PercentUsed = 0.30, Rank = 5, }, 

            }
        };
        public static ItemRank GyanaNaKashu = new ItemRank
        {
            Item = Legendary.GyanaNaKashu,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 20, PercentUsed = 2.00, Rank = 9, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {

            }
        };
        public static ItemRank CainsInsight = new ItemRank
        {
            Item = Legendary.CainsInsight,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 20, PercentUsed = 2.00, Rank = 10, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 18, PercentUsed = 1.82, Rank = 9, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 2, PercentUsed = 0.20, Rank = 9, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 6, PercentUsed = 0.60, Rank = 9, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 11, PercentUsed = 1.11, Rank = 6, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 3, PercentUsed = 0.30, Rank = 7, }, 

            }
        };
        public static ItemRank SunwukosBalance = new ItemRank
        {
            Item = Legendary.SunwukosBalance,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 546, PercentUsed = 54.71, Rank = 1, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 679, PercentUsed = 67.97, Rank = 1, }, 

            }
        };
        public static ItemRank AughildsPower = new ItemRank
        {
            Item = Legendary.AughildsPower,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 153, PercentUsed = 15.33, Rank = 2, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 292, PercentUsed = 29.47, Rank = 1, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 67, PercentUsed = 6.71, Rank = 2, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 711, PercentUsed = 71.31, Rank = 1, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 194, PercentUsed = 19.40, Rank = 2, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 45, PercentUsed = 4.53, Rank = 3, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 156, PercentUsed = 15.62, Rank = 2, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 111, PercentUsed = 11.10, Rank = 2, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 20, PercentUsed = 2.00, Rank = 2, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 427, PercentUsed = 42.70, Rank = 1, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 47, PercentUsed = 4.70, Rank = 2, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 11, PercentUsed = 1.10, Rank = 5, }, 

            }
        };
        public static ItemRank MantleOfTheUpsidedownSinners = new ItemRank
        {
            Item = Legendary.MantleOfTheUpsidedownSinners,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 124, PercentUsed = 12.42, Rank = 3, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 96, PercentUsed = 9.61, Rank = 3, }, 

            }
        };
        public static ItemRank AshearasCustodian = new ItemRank
        {
            Item = Legendary.AshearasCustodian,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 50, PercentUsed = 5.01, Rank = 4, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 31, PercentUsed = 3.13, Rank = 5, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 5, PercentUsed = 0.50, Rank = 8, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 17, PercentUsed = 1.71, Rank = 5, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 14, PercentUsed = 1.40, Rank = 5, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 20, PercentUsed = 2.01, Rank = 4, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 7, PercentUsed = 0.70, Rank = 5, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 6, PercentUsed = 0.60, Rank = 5, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 25, PercentUsed = 2.50, Rank = 6, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 5, PercentUsed = 0.50, Rank = 3, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 43, PercentUsed = 4.30, Rank = 3, }, 

            }
        };
        public static ItemRank BornsPrivilege = new ItemRank
        {
            Item = Legendary.BornsPrivilege,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 27, PercentUsed = 2.71, Rank = 5, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 5, PercentUsed = 0.50, Rank = 9, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 19, PercentUsed = 1.90, Rank = 3, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 9, PercentUsed = 0.91, Rank = 5, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 26, PercentUsed = 2.60, Rank = 4, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 2, PercentUsed = 0.20, Rank = 8, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 4, PercentUsed = 0.40, Rank = 4, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 47, PercentUsed = 4.70, Rank = 3, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 4, PercentUsed = 0.40, Rank = 4, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 86, PercentUsed = 8.60, Rank = 2, }, 

            }
        };
        public static ItemRank HomingPads = new ItemRank
        {
            Item = Legendary.HomingPads,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 13, PercentUsed = 1.30, Rank = 6, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 15, PercentUsed = 1.51, Rank = 6, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 7, PercentUsed = 0.70, Rank = 6, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 10, PercentUsed = 1.00, Rank = 7, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 5, PercentUsed = 0.50, Rank = 10, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 5, PercentUsed = 0.50, Rank = 9, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 3, PercentUsed = 0.30, Rank = 6, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 1, PercentUsed = 0.10, Rank = 9, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 6, PercentUsed = 0.60, Rank = 3, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 4, PercentUsed = 0.40, Rank = 8, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 2, PercentUsed = 0.20, Rank = 6, }, 

            }
        };
        public static ItemRank PauldronsOfTheSkeletonKing = new ItemRank
        {
            Item = Legendary.PauldronsOfTheSkeletonKing,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 13, PercentUsed = 1.30, Rank = 7, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 13, PercentUsed = 1.31, Rank = 8, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 9, PercentUsed = 0.90, Rank = 3, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 21, PercentUsed = 2.11, Rank = 4, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 13, PercentUsed = 1.30, Rank = 6, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 5, PercentUsed = 0.50, Rank = 8, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 1, PercentUsed = 0.10, Rank = 9, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 39, PercentUsed = 3.90, Rank = 4, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 2, PercentUsed = 0.20, Rank = 6, }, 

            }
        };
        public static ItemRank DeathWatchMantle = new ItemRank
        {
            Item = Legendary.DeathWatchMantle,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 9, PercentUsed = 0.90, Rank = 8, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 14, PercentUsed = 1.41, Rank = 7, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 8, PercentUsed = 0.80, Rank = 5, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 19, PercentUsed = 1.90, Rank = 4, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 3, PercentUsed = 0.30, Rank = 10, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 2, PercentUsed = 0.20, Rank = 7, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 4, PercentUsed = 0.40, Rank = 6, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 2, PercentUsed = 0.20, Rank = 6, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 1, PercentUsed = 0.10, Rank = 10, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 2, PercentUsed = 0.20, Rank = 5, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 1, PercentUsed = 0.10, Rank = 9, }, 

            }
        };
        public static ItemRank SpauldersOfZakara = new ItemRank
        {
            Item = Legendary.SpauldersOfZakara,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 8, PercentUsed = 0.80, Rank = 9, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 13, PercentUsed = 1.31, Rank = 10, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 8, PercentUsed = 0.80, Rank = 4, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 9, PercentUsed = 0.90, Rank = 8, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 6, PercentUsed = 0.60, Rank = 9, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 7, PercentUsed = 0.70, Rank = 6, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 1, PercentUsed = 0.10, Rank = 10, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 1, PercentUsed = 0.10, Rank = 10, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 1, PercentUsed = 0.10, Rank = 8, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 1, PercentUsed = 0.10, Rank = 9, }, 

            }
        };
        public static ItemRank ProfanePauldrons = new ItemRank
        {
            Item = Legendary.ProfanePauldrons,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 5, PercentUsed = 0.50, Rank = 10, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 4, PercentUsed = 0.40, Rank = 9, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 11, PercentUsed = 1.10, Rank = 6, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 8, PercentUsed = 0.80, Rank = 8, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 1, PercentUsed = 0.10, Rank = 7, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 28, PercentUsed = 2.80, Rank = 5, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 1, PercentUsed = 0.10, Rank = 8, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 1, PercentUsed = 0.10, Rank = 10, }, 

            }
        };
        public static ItemRank AughildsRule = new ItemRank
        {
            Item = Legendary.AughildsRule,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 335, PercentUsed = 33.57, Rank = 1, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 58, PercentUsed = 5.85, Rank = 5, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 8, PercentUsed = 0.80, Rank = 7, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 206, PercentUsed = 20.66, Rank = 2, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 33, PercentUsed = 3.30, Rank = 5, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 28, PercentUsed = 2.82, Rank = 5, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 372, PercentUsed = 37.24, Rank = 1, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 22, PercentUsed = 2.20, Rank = 4, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 232, PercentUsed = 23.20, Rank = 2, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 10, PercentUsed = 1.00, Rank = 4, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 8, PercentUsed = 0.80, Rank = 6, }, 

            }
        };
        public static ItemRank InnasVastExpanse = new ItemRank
        {
            Item = Legendary.InnasVastExpanse,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 262, PercentUsed = 26.25, Rank = 2, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 312, PercentUsed = 31.23, Rank = 2, }, 

            }
        };
        public static ItemRank HeartOfTheCrashingWave = new ItemRank
        {
            Item = Legendary.HeartOfTheCrashingWave,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 121, PercentUsed = 12.12, Rank = 3, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 87, PercentUsed = 8.71, Rank = 4, }, 

            }
        };
        public static ItemRank BlackthornesSurcoat = new ItemRank
        {
            Item = Legendary.BlackthornesSurcoat,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 99, PercentUsed = 9.92, Rank = 4, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 88, PercentUsed = 8.88, Rank = 4, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 16, PercentUsed = 1.60, Rank = 6, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 147, PercentUsed = 14.74, Rank = 4, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 44, PercentUsed = 4.40, Rank = 4, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 33, PercentUsed = 3.32, Rank = 4, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 17, PercentUsed = 1.70, Rank = 6, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 18, PercentUsed = 1.80, Rank = 5, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 85, PercentUsed = 8.50, Rank = 4, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 4, PercentUsed = 0.40, Rank = 6, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 9, PercentUsed = 0.90, Rank = 5, }, 

            }
        };
        public static ItemRank Cindercoat = new ItemRank
        {
            Item = Legendary.Cindercoat,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 69, PercentUsed = 6.91, Rank = 5, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 118, PercentUsed = 11.91, Rank = 3, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 155, PercentUsed = 15.52, Rank = 2, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 20, PercentUsed = 2.01, Rank = 7, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 418, PercentUsed = 41.80, Rank = 1, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 55, PercentUsed = 5.53, Rank = 2, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 91, PercentUsed = 9.11, Rank = 3, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 62, PercentUsed = 6.20, Rank = 3, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 483, PercentUsed = 48.30, Rank = 2, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 10, PercentUsed = 1.00, Rank = 7, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 582, PercentUsed = 58.20, Rank = 1, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 24, PercentUsed = 2.40, Rank = 3, }, 

            }
        };
        public static ItemRank BornsFrozenSoul = new ItemRank
        {
            Item = Legendary.BornsFrozenSoul,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 48, PercentUsed = 4.81, Rank = 6, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 13, PercentUsed = 1.31, Rank = 6, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 3, PercentUsed = 0.30, Rank = 9, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 7, PercentUsed = 0.70, Rank = 8, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 12, PercentUsed = 1.21, Rank = 6, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 71, PercentUsed = 7.11, Rank = 5, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 11, PercentUsed = 1.10, Rank = 6, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 44, PercentUsed = 4.40, Rank = 6, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 3, PercentUsed = 0.30, Rank = 8, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 126, PercentUsed = 12.60, Rank = 2, }, 

            }
        };
        public static ItemRank TyraelsMight = new ItemRank
        {
            Item = Legendary.TyraelsMight,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 10, PercentUsed = 1.00, Rank = 7, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 8, PercentUsed = 0.81, Rank = 8, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 8, PercentUsed = 0.80, Rank = 9, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 3, PercentUsed = 0.30, Rank = 7, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 1, PercentUsed = 0.10, Rank = 8, }, 

            }
        };
        public static ItemRank ImmortalKingsEternalReign = new ItemRank
        {
            Item = Legendary.ImmortalKingsEternalReign,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 4, PercentUsed = 0.40, Rank = 8, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 395, PercentUsed = 39.86, Rank = 1, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 131, PercentUsed = 13.10, Rank = 2, }, 

            }
        };
        public static ItemRank TalRashasRelentlessPursuit = new ItemRank
        {
            Item = Legendary.TalRashasRelentlessPursuit,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 3, PercentUsed = 0.30, Rank = 9, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 22, PercentUsed = 2.21, Rank = 5, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 125, PercentUsed = 12.50, Rank = 3, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 9, PercentUsed = 0.90, Rank = 7, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 1, PercentUsed = 0.10, Rank = 10, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 2, PercentUsed = 0.20, Rank = 5, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 66, PercentUsed = 6.60, Rank = 5, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 36, PercentUsed = 3.60, Rank = 3, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 2, PercentUsed = 0.20, Rank = 7, }, 

            }
        };
        public static ItemRank AquilaCuirass = new ItemRank
        {
            Item = Legendary.AquilaCuirass,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 3, PercentUsed = 0.30, Rank = 10, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 2, PercentUsed = 0.20, Rank = 9, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {

            }
        };
        public static ItemRank SunwukosPaws = new ItemRank
        {
            Item = Legendary.SunwukosPaws,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 587, PercentUsed = 58.82, Rank = 1, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 789, PercentUsed = 78.98, Rank = 1, }, 

            }
        };
        public static ItemRank FistsOfThunder = new ItemRank
        {
            Item = Legendary.FistsOfThunder,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 126, PercentUsed = 12.63, Rank = 2, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 102, PercentUsed = 10.21, Rank = 2, }, 

            }
        };
        public static ItemRank AshearasWard = new ItemRank
        {
            Item = Legendary.AshearasWard,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 70, PercentUsed = 7.01, Rank = 3, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 24, PercentUsed = 2.42, Rank = 6, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 5, PercentUsed = 0.50, Rank = 7, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 14, PercentUsed = 1.40, Rank = 7, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 21, PercentUsed = 2.10, Rank = 6, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 20, PercentUsed = 2.01, Rank = 5, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 12, PercentUsed = 1.20, Rank = 5, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 25, PercentUsed = 2.50, Rank = 4, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 7, PercentUsed = 0.70, Rank = 6, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 71, PercentUsed = 7.10, Rank = 2, }, 

            }
        };
        public static ItemRank Magefist = new ItemRank
        {
            Item = Legendary.Magefist,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 45, PercentUsed = 4.51, Rank = 4, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 91, PercentUsed = 9.18, Rank = 4, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 129, PercentUsed = 12.91, Rank = 3, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 48, PercentUsed = 4.81, Rank = 4, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 369, PercentUsed = 36.90, Rank = 2, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 42, PercentUsed = 4.23, Rank = 3, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 17, PercentUsed = 1.70, Rank = 4, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 23, PercentUsed = 2.30, Rank = 5, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 63, PercentUsed = 6.30, Rank = 3, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 8, PercentUsed = 0.80, Rank = 8, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 219, PercentUsed = 21.90, Rank = 2, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 15, PercentUsed = 1.50, Rank = 5, }, 

            }
        };
        public static ItemRank CainsScrivener = new ItemRank
        {
            Item = Legendary.CainsScrivener,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 32, PercentUsed = 3.21, Rank = 5, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 9, PercentUsed = 0.91, Rank = 10, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 5, PercentUsed = 0.50, Rank = 6, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 60, PercentUsed = 6.02, Rank = 3, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 8, PercentUsed = 0.80, Rank = 9, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 12, PercentUsed = 1.21, Rank = 7, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 10, PercentUsed = 1.00, Rank = 6, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 4, PercentUsed = 0.40, Rank = 8, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 2, PercentUsed = 0.20, Rank = 5, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 42, PercentUsed = 4.20, Rank = 3, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 4, PercentUsed = 0.40, Rank = 9, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 12, PercentUsed = 1.20, Rank = 6, }, 

            }
        };
        public static ItemRank GladiatorGauntlets = new ItemRank
        {
            Item = Legendary.GladiatorGauntlets,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 21, PercentUsed = 2.10, Rank = 6, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 19, PercentUsed = 1.92, Rank = 7, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 3, PercentUsed = 0.30, Rank = 10, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 23, PercentUsed = 2.31, Rank = 5, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 22, PercentUsed = 2.20, Rank = 5, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 19, PercentUsed = 1.91, Rank = 6, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 3, PercentUsed = 0.30, Rank = 9, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 1, PercentUsed = 0.10, Rank = 9, }, 

            }
        };
        public static ItemRank StArchewsGage = new ItemRank
        {
            Item = Legendary.StArchewsGage,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 18, PercentUsed = 1.80, Rank = 7, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 11, PercentUsed = 1.11, Rank = 9, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 16, PercentUsed = 1.60, Rank = 6, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 2, PercentUsed = 0.20, Rank = 10, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 4, PercentUsed = 0.40, Rank = 8, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 23, PercentUsed = 2.30, Rank = 5, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 3, PercentUsed = 0.30, Rank = 10, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 48, PercentUsed = 4.80, Rank = 3, }, 

            }
        };
        public static ItemRank Frostburn = new ItemRank
        {
            Item = Legendary.Frostburn,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 13, PercentUsed = 1.30, Rank = 8, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 11, PercentUsed = 1.10, Rank = 4, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 12, PercentUsed = 1.20, Rank = 7, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 23, PercentUsed = 2.30, Rank = 3, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 1, PercentUsed = 0.10, Rank = 10, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 4, PercentUsed = 0.40, Rank = 4, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 9, PercentUsed = 0.90, Rank = 7, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 6, PercentUsed = 0.60, Rank = 7, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 2, PercentUsed = 0.20, Rank = 9, }, 

            }
        };
        public static ItemRank GlovesOfWorship = new ItemRank
        {
            Item = Legendary.GlovesOfWorship,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 9, PercentUsed = 0.90, Rank = 9, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 5, PercentUsed = 0.50, Rank = 10, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 7, PercentUsed = 0.70, Rank = 10, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 8, PercentUsed = 0.80, Rank = 8, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 5, PercentUsed = 0.50, Rank = 7, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 8, PercentUsed = 0.80, Rank = 6, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 2, PercentUsed = 0.20, Rank = 6, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 14, PercentUsed = 1.40, Rank = 4, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 2, PercentUsed = 0.20, Rank = 8, }, 

            }
        };
        public static ItemRank PendersPurchase = new ItemRank
        {
            Item = Legendary.PendersPurchase,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 8, PercentUsed = 0.80, Rank = 10, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 4, PercentUsed = 0.40, Rank = 8, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 5, PercentUsed = 0.50, Rank = 9, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {

            }
        };
        public static ItemRank InnasFavor = new ItemRank
        {
            Item = Legendary.InnasFavor,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 271, PercentUsed = 27.15, Rank = 1, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 405, PercentUsed = 40.54, Rank = 1, }, 

            }
        };
        public static ItemRank BlackthornesNotchedBelt = new ItemRank
        {
            Item = Legendary.BlackthornesNotchedBelt,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 204, PercentUsed = 20.44, Rank = 2, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 40, PercentUsed = 4.04, Rank = 6, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 128, PercentUsed = 12.81, Rank = 2, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 309, PercentUsed = 30.99, Rank = 1, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 139, PercentUsed = 13.90, Rank = 4, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 56, PercentUsed = 5.63, Rank = 4, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 37, PercentUsed = 3.70, Rank = 6, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 20, PercentUsed = 2.00, Rank = 7, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 8, PercentUsed = 0.80, Rank = 5, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 147, PercentUsed = 14.70, Rank = 2, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 39, PercentUsed = 3.90, Rank = 3, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 15, PercentUsed = 1.50, Rank = 5, }, 

            }
        };
        public static ItemRank ThundergodsVigor = new ItemRank
        {
            Item = Legendary.ThundergodsVigor,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 143, PercentUsed = 14.33, Rank = 3, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 80, PercentUsed = 8.07, Rank = 3, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 26, PercentUsed = 2.60, Rank = 9, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 21, PercentUsed = 2.10, Rank = 8, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 21, PercentUsed = 2.11, Rank = 8, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 83, PercentUsed = 8.31, Rank = 4, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 19, PercentUsed = 1.90, Rank = 8, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 4, PercentUsed = 0.40, Rank = 7, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 9, PercentUsed = 0.90, Rank = 9, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 4, PercentUsed = 0.40, Rank = 9, }, 

            }
        };
        public static ItemRank StringOfEars = new ItemRank
        {
            Item = Legendary.StringOfEars,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 72, PercentUsed = 7.21, Rank = 4, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 25, PercentUsed = 2.52, Rank = 8, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 103, PercentUsed = 10.31, Rank = 3, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 94, PercentUsed = 9.43, Rank = 3, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 253, PercentUsed = 25.30, Rank = 1, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 46, PercentUsed = 4.63, Rank = 5, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 19, PercentUsed = 1.90, Rank = 8, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 8, PercentUsed = 0.80, Rank = 9, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 22, PercentUsed = 2.20, Rank = 8, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 197, PercentUsed = 19.70, Rank = 2, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 10, PercentUsed = 1.00, Rank = 6, }, 

            }
        };
        public static ItemRank TheWitchingHour = new ItemRank
        {
            Item = Legendary.TheWitchingHour,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 64, PercentUsed = 6.41, Rank = 5, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 44, PercentUsed = 4.44, Rank = 5, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 393, PercentUsed = 39.34, Rank = 1, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 274, PercentUsed = 27.48, Rank = 2, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 174, PercentUsed = 17.40, Rank = 2, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 84, PercentUsed = 8.45, Rank = 3, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 208, PercentUsed = 20.82, Rank = 2, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 83, PercentUsed = 8.30, Rank = 4, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 831, PercentUsed = 83.10, Rank = 1, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 495, PercentUsed = 49.50, Rank = 1, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 596, PercentUsed = 59.60, Rank = 1, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 261, PercentUsed = 26.10, Rank = 2, }, 

            }
        };
        public static ItemRank CaptainCrimsonsSilkGirdle = new ItemRank
        {
            Item = Legendary.CaptainCrimsonsSilkGirdle,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 58, PercentUsed = 5.81, Rank = 6, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 15, PercentUsed = 1.51, Rank = 9, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 13, PercentUsed = 1.30, Rank = 9, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 17, PercentUsed = 1.70, Rank = 10, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 245, PercentUsed = 24.65, Rank = 2, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 48, PercentUsed = 4.80, Rank = 5, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 5, PercentUsed = 0.50, Rank = 6, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 2, PercentUsed = 0.20, Rank = 10, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 8, PercentUsed = 0.80, Rank = 10, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 132, PercentUsed = 13.20, Rank = 3, }, 

            }
        };
        public static ItemRank HellcatWaistguard = new ItemRank
        {
            Item = Legendary.HellcatWaistguard,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 46, PercentUsed = 4.61, Rank = 7, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 61, PercentUsed = 6.11, Rank = 5, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 46, PercentUsed = 4.61, Rank = 7, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 46, PercentUsed = 4.60, Rank = 5, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 15, PercentUsed = 1.51, Rank = 9, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 13, PercentUsed = 1.30, Rank = 10, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 4, PercentUsed = 0.40, Rank = 9, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 25, PercentUsed = 2.50, Rank = 7, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 9, PercentUsed = 0.90, Rank = 8, }, 

            }
        };
        public static ItemRank VigilanteBelt = new ItemRank
        {
            Item = Legendary.VigilanteBelt,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 38, PercentUsed = 3.81, Rank = 8, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 6, PercentUsed = 0.60, Rank = 10, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 399, PercentUsed = 40.14, Rank = 1, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 87, PercentUsed = 8.71, Rank = 3, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 20, PercentUsed = 2.00, Rank = 5, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 139, PercentUsed = 13.90, Rank = 3, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 13, PercentUsed = 1.30, Rank = 7, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 512, PercentUsed = 51.20, Rank = 1, }, 

            }
        };
        public static ItemRank FleetingStrap = new ItemRank
        {
            Item = Legendary.FleetingStrap,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 20, PercentUsed = 2.00, Rank = 9, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 29, PercentUsed = 2.90, Rank = 7, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 32, PercentUsed = 3.21, Rank = 8, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 12, PercentUsed = 1.21, Rank = 10, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 11, PercentUsed = 1.10, Rank = 9, }, 

            }
        };
        public static ItemRank Goldwrap = new ItemRank
        {
            Item = Legendary.Goldwrap,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 12, PercentUsed = 1.20, Rank = 10, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 65, PercentUsed = 6.51, Rank = 4, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 49, PercentUsed = 4.91, Rank = 6, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 38, PercentUsed = 3.80, Rank = 6, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 21, PercentUsed = 2.10, Rank = 7, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 94, PercentUsed = 9.40, Rank = 2, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 51, PercentUsed = 5.10, Rank = 4, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 18, PercentUsed = 1.80, Rank = 6, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 8, PercentUsed = 0.80, Rank = 8, }, 

            }
        };
        public static ItemRank InnasTemperance = new ItemRank
        {
            Item = Legendary.InnasTemperance,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 257, PercentUsed = 25.75, Rank = 1, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 7, PercentUsed = 0.70, Rank = 9, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 4, PercentUsed = 0.40, Rank = 10, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 414, PercentUsed = 41.44, Rank = 1, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 2, PercentUsed = 0.20, Rank = 10, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 19, PercentUsed = 1.90, Rank = 3, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 4, PercentUsed = 0.40, Rank = 10, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 2, PercentUsed = 0.20, Rank = 9, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 9, PercentUsed = 0.90, Rank = 7, }, 

            }
        };
        public static ItemRank BlackthornesJoustingMail = new ItemRank
        {
            Item = Legendary.BlackthornesJoustingMail,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 208, PercentUsed = 20.84, Rank = 2, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 112, PercentUsed = 11.30, Rank = 4, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 35, PercentUsed = 3.50, Rank = 2, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 402, PercentUsed = 40.32, Rank = 1, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 55, PercentUsed = 5.50, Rank = 2, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 39, PercentUsed = 3.92, Rank = 4, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 148, PercentUsed = 14.81, Rank = 3, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 22, PercentUsed = 2.20, Rank = 4, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 4, PercentUsed = 0.40, Rank = 6, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 191, PercentUsed = 19.10, Rank = 3, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 11, PercentUsed = 1.10, Rank = 5, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 5, PercentUsed = 0.50, Rank = 9, }, 

            }
        };
        public static ItemRank DepthDiggers = new ItemRank
        {
            Item = Legendary.DepthDiggers,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 200, PercentUsed = 20.04, Rank = 3, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 89, PercentUsed = 8.98, Rank = 5, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 13, PercentUsed = 1.30, Rank = 6, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 5, PercentUsed = 0.50, Rank = 9, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 10, PercentUsed = 1.01, Rank = 7, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 94, PercentUsed = 9.41, Rank = 4, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 13, PercentUsed = 1.30, Rank = 5, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 1, PercentUsed = 0.10, Rank = 8, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 6, PercentUsed = 0.60, Rank = 8, }, 

            }
        };
        public static ItemRank ScalesOfTheDancingSerpent = new ItemRank
        {
            Item = Legendary.ScalesOfTheDancingSerpent,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 111, PercentUsed = 11.12, Rank = 4, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 80, PercentUsed = 8.01, Rank = 5, }, 

            }
        };
        public static ItemRank CaptainCrimsonsThrust = new ItemRank
        {
            Item = Legendary.CaptainCrimsonsThrust,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 98, PercentUsed = 9.82, Rank = 5, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 16, PercentUsed = 1.61, Rank = 9, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 15, PercentUsed = 1.50, Rank = 4, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 20, PercentUsed = 2.01, Rank = 8, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 22, PercentUsed = 2.20, Rank = 5, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 135, PercentUsed = 13.58, Rank = 2, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 164, PercentUsed = 16.42, Rank = 2, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 3, PercentUsed = 0.30, Rank = 9, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 6, PercentUsed = 0.60, Rank = 5, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 7, PercentUsed = 0.70, Rank = 7, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 6, PercentUsed = 0.60, Rank = 6, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 127, PercentUsed = 12.70, Rank = 2, }, 

            }
        };
        public static ItemRank AshearasPace = new ItemRank
        {
            Item = Legendary.AshearasPace,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 27, PercentUsed = 2.71, Rank = 6, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 38, PercentUsed = 3.83, Rank = 6, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 14, PercentUsed = 1.40, Rank = 5, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 27, PercentUsed = 2.71, Rank = 7, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 15, PercentUsed = 1.50, Rank = 8, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 16, PercentUsed = 1.61, Rank = 5, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 3, PercentUsed = 0.30, Rank = 8, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 6, PercentUsed = 0.60, Rank = 7, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 5, PercentUsed = 0.50, Rank = 9, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 5, PercentUsed = 0.50, Rank = 7, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 26, PercentUsed = 2.60, Rank = 3, }, 

            }
        };
        public static ItemRank HexingPantsOfMrYan = new ItemRank
        {
            Item = Legendary.HexingPantsOfMrYan,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 26, PercentUsed = 2.61, Rank = 7, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 113, PercentUsed = 11.40, Rank = 3, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 22, PercentUsed = 2.20, Rank = 3, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 63, PercentUsed = 6.32, Rank = 5, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 22, PercentUsed = 2.20, Rank = 6, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 8, PercentUsed = 0.80, Rank = 8, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 55, PercentUsed = 5.51, Rank = 6, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 95, PercentUsed = 9.50, Rank = 2, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 25, PercentUsed = 2.50, Rank = 2, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 84, PercentUsed = 8.40, Rank = 4, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 18, PercentUsed = 1.80, Rank = 3, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 11, PercentUsed = 1.10, Rank = 5, }, 

            }
        };
        public static ItemRank CainsHabit = new ItemRank
        {
            Item = Legendary.CainsHabit,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 18, PercentUsed = 1.80, Rank = 8, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 25, PercentUsed = 2.52, Rank = 7, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 7, PercentUsed = 0.70, Rank = 8, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 130, PercentUsed = 13.04, Rank = 3, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 28, PercentUsed = 2.80, Rank = 4, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 13, PercentUsed = 1.31, Rank = 6, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 15, PercentUsed = 1.50, Rank = 7, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 12, PercentUsed = 1.20, Rank = 6, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 3, PercentUsed = 0.30, Rank = 7, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 219, PercentUsed = 21.90, Rank = 2, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 20, PercentUsed = 2.00, Rank = 2, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 9, PercentUsed = 0.90, Rank = 6, }, 

            }
        };
        public static ItemRank HammerJammers = new ItemRank
        {
            Item = Legendary.HammerJammers,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 6, PercentUsed = 0.60, Rank = 9, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 3, PercentUsed = 0.30, Rank = 10, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 14, PercentUsed = 1.40, Rank = 10, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 2, PercentUsed = 0.20, Rank = 10, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 2, PercentUsed = 0.20, Rank = 9, }, 

            }
        };


        public static ItemRank TheCrudestBoots = new ItemRank
        {
            Item = Legendary.TheCrudestBoots,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 273, PercentUsed = 27.35, Rank = 1, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 495, PercentUsed = 49.55, Rank = 1, }, 

            }
        };
        public static ItemRank BlackthornesSpurs = new ItemRank
        {
            Item = Legendary.BlackthornesSpurs,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 180, PercentUsed = 18.04, Rank = 2, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 44, PercentUsed = 4.44, Rank = 5, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 23, PercentUsed = 2.30, Rank = 3, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 140, PercentUsed = 14.04, Rank = 3, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 31, PercentUsed = 3.10, Rank = 5, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 31, PercentUsed = 3.12, Rank = 4, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 153, PercentUsed = 15.32, Rank = 2, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 6, PercentUsed = 0.60, Rank = 6, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 98, PercentUsed = 9.80, Rank = 4, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 11, PercentUsed = 1.10, Rank = 4, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 4, PercentUsed = 0.40, Rank = 7, }, 

            }
        };
        public static ItemRank IceClimbers = new ItemRank
        {
            Item = Legendary.IceClimbers,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 144, PercentUsed = 14.43, Rank = 3, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 113, PercentUsed = 11.40, Rank = 4, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 6, PercentUsed = 0.60, Rank = 8, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 96, PercentUsed = 9.63, Rank = 4, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 49, PercentUsed = 4.90, Rank = 2, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 18, PercentUsed = 1.81, Rank = 5, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 46, PercentUsed = 4.60, Rank = 5, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 17, PercentUsed = 1.70, Rank = 4, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 1, PercentUsed = 0.10, Rank = 6, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 37, PercentUsed = 3.70, Rank = 6, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 6, PercentUsed = 0.60, Rank = 6, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 5, PercentUsed = 0.50, Rank = 6, }, 

            }
        };
        public static ItemRank EightdemonBoots = new ItemRank
        {
            Item = Legendary.EightdemonBoots,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 135, PercentUsed = 13.53, Rank = 4, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 96, PercentUsed = 9.61, Rank = 4, }, 

            }
        };
        public static ItemRank CaptainCrimsonsWaders = new ItemRank
        {
            Item = Legendary.CaptainCrimsonsWaders,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 76, PercentUsed = 7.62, Rank = 5, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 5, PercentUsed = 0.50, Rank = 9, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 16, PercentUsed = 1.60, Rank = 10, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 22, PercentUsed = 2.20, Rank = 6, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 130, PercentUsed = 13.08, Rank = 2, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 125, PercentUsed = 12.51, Rank = 3, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 2, PercentUsed = 0.20, Rank = 4, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 8, PercentUsed = 0.80, Rank = 9, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 5, PercentUsed = 0.50, Rank = 7, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 39, PercentUsed = 3.90, Rank = 3, }, 

            }
        };
        public static ItemRank AshearasFinders = new ItemRank
        {
            Item = Legendary.AshearasFinders,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 36, PercentUsed = 3.61, Rank = 6, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 18, PercentUsed = 1.82, Rank = 8, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 6, PercentUsed = 0.60, Rank = 7, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 22, PercentUsed = 2.21, Rank = 8, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 15, PercentUsed = 1.50, Rank = 7, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 13, PercentUsed = 1.31, Rank = 7, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 5, PercentUsed = 0.50, Rank = 10, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 3, PercentUsed = 0.30, Rank = 9, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 7, PercentUsed = 0.70, Rank = 10, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 6, PercentUsed = 0.60, Rank = 5, }, 

            }
        };
        public static ItemRank CainsTravelers = new ItemRank
        {
            Item = Legendary.CainsTravelers,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 33, PercentUsed = 3.31, Rank = 7, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 21, PercentUsed = 2.12, Rank = 6, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 8, PercentUsed = 0.80, Rank = 6, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 91, PercentUsed = 9.13, Rank = 5, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 35, PercentUsed = 3.50, Rank = 3, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 11, PercentUsed = 1.11, Rank = 8, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 17, PercentUsed = 1.70, Rank = 7, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 12, PercentUsed = 1.20, Rank = 5, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 1, PercentUsed = 0.10, Rank = 5, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 182, PercentUsed = 18.20, Rank = 3, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 17, PercentUsed = 1.70, Rank = 2, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 11, PercentUsed = 1.10, Rank = 5, }, 

            }
        };
        public static ItemRank IllusoryBoots = new ItemRank
        {
            Item = Legendary.IllusoryBoots,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 30, PercentUsed = 3.01, Rank = 8, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 10, PercentUsed = 1.01, Rank = 9, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 10, PercentUsed = 1.00, Rank = 5, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 43, PercentUsed = 4.31, Rank = 6, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 8, PercentUsed = 0.80, Rank = 10, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 16, PercentUsed = 1.61, Rank = 6, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 18, PercentUsed = 1.80, Rank = 6, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 1, PercentUsed = 0.10, Rank = 7, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 64, PercentUsed = 6.40, Rank = 5, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 5, PercentUsed = 0.50, Rank = 8, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 116, PercentUsed = 11.60, Rank = 2, }, 

            }
        };
        public static ItemRank FireWalkers = new ItemRank
        {
            Item = Legendary.FireWalkers,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 23, PercentUsed = 2.30, Rank = 9, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 19, PercentUsed = 1.92, Rank = 7, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 13, PercentUsed = 1.30, Rank = 4, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 18, PercentUsed = 1.81, Rank = 9, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 11, PercentUsed = 1.10, Rank = 9, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 10, PercentUsed = 1.01, Rank = 9, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 6, PercentUsed = 0.60, Rank = 9, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 5, PercentUsed = 0.50, Rank = 7, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 8, PercentUsed = 0.80, Rank = 8, }, 

            }
        };
        public static ItemRank NatalyasBloodyFootprints = new ItemRank
        {
            Item = Legendary.NatalyasBloodyFootprints,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 19, PercentUsed = 1.90, Rank = 10, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 62, PercentUsed = 6.21, Rank = 2, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 11, PercentUsed = 1.10, Rank = 8, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 63, PercentUsed = 6.30, Rank = 2, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 2, PercentUsed = 0.20, Rank = 9, }, 

            }
        };
        public static ItemRank SunwukosShines = new ItemRank
        {
            Item = Legendary.SunwukosShines,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 255, PercentUsed = 25.55, Rank = 1, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 569, PercentUsed = 56.96, Rank = 1, }, 

            }
        };
        public static ItemRank BlackthornesDuncraigCross = new ItemRank
        {
            Item = Legendary.BlackthornesDuncraigCross,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 137, PercentUsed = 13.73, Rank = 2, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 88, PercentUsed = 8.88, Rank = 4, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 160, PercentUsed = 16.02, Rank = 1, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 178, PercentUsed = 17.85, Rank = 1, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 152, PercentUsed = 15.20, Rank = 2, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 87, PercentUsed = 8.75, Rank = 2, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 88, PercentUsed = 8.81, Rank = 2, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 68, PercentUsed = 6.80, Rank = 4, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 22, PercentUsed = 2.20, Rank = 4, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 101, PercentUsed = 10.10, Rank = 3, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 104, PercentUsed = 10.40, Rank = 3, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 66, PercentUsed = 6.60, Rank = 4, }, 

            }
        };
        public static ItemRank CountessJuliasCameo = new ItemRank
        {
            Item = Legendary.CountessJuliasCameo,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 76, PercentUsed = 7.62, Rank = 3, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 98, PercentUsed = 9.89, Rank = 2, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 82, PercentUsed = 8.21, Rank = 4, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 111, PercentUsed = 11.13, Rank = 2, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 47, PercentUsed = 4.70, Rank = 8, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 65, PercentUsed = 6.54, Rank = 7, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 14, PercentUsed = 1.40, Rank = 8, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 48, PercentUsed = 4.80, Rank = 7, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 30, PercentUsed = 3.00, Rank = 10, }, 

            }
        };
        public static ItemRank TheTravelersPledge = new ItemRank
        {
            Item = Legendary.TheTravelersPledge,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 58, PercentUsed = 5.81, Rank = 4, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 62, PercentUsed = 6.26, Rank = 6, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 132, PercentUsed = 13.21, Rank = 3, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 62, PercentUsed = 6.22, Rank = 6, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 79, PercentUsed = 7.90, Rank = 3, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 77, PercentUsed = 7.75, Rank = 4, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 31, PercentUsed = 3.10, Rank = 4, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 60, PercentUsed = 6.00, Rank = 7, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 114, PercentUsed = 11.40, Rank = 2, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 79, PercentUsed = 7.90, Rank = 4, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 89, PercentUsed = 8.90, Rank = 4, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 55, PercentUsed = 5.50, Rank = 8, }, 

            }
        };
        public static ItemRank GoldenGorgetOfLeoric = new ItemRank
        {
            Item = Legendary.GoldenGorgetOfLeoric,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 56, PercentUsed = 5.61, Rank = 5, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 72, PercentUsed = 7.27, Rank = 5, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 33, PercentUsed = 3.30, Rank = 10, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 82, PercentUsed = 8.22, Rank = 5, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 42, PercentUsed = 4.20, Rank = 9, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 50, PercentUsed = 5.03, Rank = 9, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 14, PercentUsed = 1.40, Rank = 7, }, 

            }
        };
        public static ItemRank Ouroboros = new ItemRank
        {
            Item = Legendary.Ouroboros,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 53, PercentUsed = 5.31, Rank = 6, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 98, PercentUsed = 9.89, Rank = 3, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 50, PercentUsed = 5.01, Rank = 7, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 85, PercentUsed = 8.53, Rank = 4, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 60, PercentUsed = 6.00, Rank = 4, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 76, PercentUsed = 7.65, Rank = 5, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 31, PercentUsed = 3.10, Rank = 5, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 49, PercentUsed = 4.90, Rank = 9, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 17, PercentUsed = 1.70, Rank = 6, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 48, PercentUsed = 4.80, Rank = 8, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 59, PercentUsed = 5.90, Rank = 8, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 57, PercentUsed = 5.70, Rank = 7, }, 

            }
        };
        public static ItemRank HauntOfVaxo = new ItemRank
        {
            Item = Legendary.HauntOfVaxo,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 44, PercentUsed = 4.41, Rank = 7, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 139, PercentUsed = 14.03, Rank = 1, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 72, PercentUsed = 7.21, Rank = 5, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 56, PercentUsed = 5.62, Rank = 8, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 60, PercentUsed = 6.00, Rank = 5, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 88, PercentUsed = 8.85, Rank = 1, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 30, PercentUsed = 3.00, Rank = 6, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 65, PercentUsed = 6.50, Rank = 5, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 12, PercentUsed = 1.20, Rank = 9, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 52, PercentUsed = 5.20, Rank = 6, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 76, PercentUsed = 7.60, Rank = 6, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 76, PercentUsed = 7.60, Rank = 3, }, 

            }
        };
        public static ItemRank SquirtsNecklace = new ItemRank
        {
            Item = Legendary.SquirtsNecklace,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 41, PercentUsed = 4.11, Rank = 8, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 48, PercentUsed = 4.84, Rank = 7, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 70, PercentUsed = 7.01, Rank = 6, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 48, PercentUsed = 4.81, Rank = 9, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 51, PercentUsed = 5.10, Rank = 7, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 73, PercentUsed = 7.34, Rank = 6, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 14, PercentUsed = 1.40, Rank = 9, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 17, PercentUsed = 1.70, Rank = 5, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 38, PercentUsed = 3.80, Rank = 10, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 51, PercentUsed = 5.10, Rank = 9, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 35, PercentUsed = 3.50, Rank = 10, }, 

            }
        };

        public static ItemRank MarasKaleidoscope = new ItemRank
        {
            Item = Legendary.MarasKaleidoscope,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 20, PercentUsed = 2.00, Rank = 10, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 48, PercentUsed = 4.84, Rank = 8, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 44, PercentUsed = 4.40, Rank = 8, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 57, PercentUsed = 5.72, Rank = 7, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 59, PercentUsed = 5.94, Rank = 8, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 139, PercentUsed = 13.90, Rank = 1, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 26, PercentUsed = 2.60, Rank = 3, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 63, PercentUsed = 6.30, Rank = 5, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 84, PercentUsed = 8.40, Rank = 5, }, 

            }
        };
        public static ItemRank RingOfRoyalGrandeur = new ItemRank
        {
            Item = Legendary.RingOfRoyalGrandeur,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 805, PercentUsed = 80.66, Rank = 1, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 765, PercentUsed = 77.19, Rank = 1, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 880, PercentUsed = 88.09, Rank = 1, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 712, PercentUsed = 71.41, Rank = 1, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 641, PercentUsed = 64.10, Rank = 1, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 788, PercentUsed = 79.28, Rank = 1, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 829, PercentUsed = 82.98, Rank = 1, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 544, PercentUsed = 54.40, Rank = 2, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 899, PercentUsed = 89.90, Rank = 1, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 593, PercentUsed = 59.30, Rank = 1, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 390, PercentUsed = 39.00, Rank = 3, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 582, PercentUsed = 58.20, Rank = 2, }, 

            }
        };
        public static ItemRank StoneOfJordan = new ItemRank
        {
            Item = Legendary.StoneOfJordan,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 264, PercentUsed = 26.45, Rank = 2, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 372, PercentUsed = 37.54, Rank = 2, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 409, PercentUsed = 40.94, Rank = 2, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 344, PercentUsed = 34.50, Rank = 2, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 543, PercentUsed = 54.30, Rank = 2, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 490, PercentUsed = 49.30, Rank = 2, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 320, PercentUsed = 32.03, Rank = 3, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 569, PercentUsed = 56.90, Rank = 1, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 582, PercentUsed = 58.20, Rank = 2, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 415, PercentUsed = 41.50, Rank = 2, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 815, PercentUsed = 81.50, Rank = 1, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 600, PercentUsed = 60.00, Rank = 1, }, 

            }
        };
        public static ItemRank Unity = new ItemRank
        {
            Item = Legendary.Unity,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 226, PercentUsed = 22.65, Rank = 3, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 292, PercentUsed = 29.47, Rank = 3, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 222, PercentUsed = 22.22, Rank = 3, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 232, PercentUsed = 23.27, Rank = 4, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 286, PercentUsed = 28.60, Rank = 3, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 242, PercentUsed = 24.35, Rank = 3, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 441, PercentUsed = 44.14, Rank = 2, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 390, PercentUsed = 39.00, Rank = 3, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 87, PercentUsed = 8.70, Rank = 4, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 271, PercentUsed = 27.10, Rank = 3, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 526, PercentUsed = 52.60, Rank = 2, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 291, PercentUsed = 29.10, Rank = 3, }, 

            }
        };
        public static ItemRank TheCompassRose = new ItemRank
        {
            Item = Legendary.TheCompassRose,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 63, PercentUsed = 6.31, Rank = 4, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 48, PercentUsed = 4.84, Rank = 6, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 112, PercentUsed = 11.21, Rank = 5, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 32, PercentUsed = 3.21, Rank = 6, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 75, PercentUsed = 7.50, Rank = 4, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 51, PercentUsed = 5.13, Rank = 5, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 21, PercentUsed = 2.10, Rank = 5, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 44, PercentUsed = 4.40, Rank = 5, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 114, PercentUsed = 11.40, Rank = 3, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 53, PercentUsed = 5.30, Rank = 6, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 58, PercentUsed = 5.80, Rank = 4, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 52, PercentUsed = 5.20, Rank = 4, }, 

            }
        };
        public static ItemRank BulkathossWeddingBand = new ItemRank
        {
            Item = Legendary.BulkathossWeddingBand,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 57, PercentUsed = 5.71, Rank = 5, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 68, PercentUsed = 6.86, Rank = 4, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 22, PercentUsed = 2.20, Rank = 8, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 31, PercentUsed = 3.11, Rank = 7, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 30, PercentUsed = 3.00, Rank = 6, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 38, PercentUsed = 3.82, Rank = 7, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 12, PercentUsed = 1.20, Rank = 10, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 105, PercentUsed = 10.50, Rank = 4, }, 

            }
        };
        public static ItemRank LeoricsSignet = new ItemRank
        {
            Item = Legendary.LeoricsSignet,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 55, PercentUsed = 5.51, Rank = 6, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 52, PercentUsed = 5.25, Rank = 5, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 38, PercentUsed = 3.80, Rank = 6, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 19, PercentUsed = 1.91, Rank = 8, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 57, PercentUsed = 5.70, Rank = 5, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 40, PercentUsed = 4.02, Rank = 6, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 14, PercentUsed = 1.40, Rank = 6, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 20, PercentUsed = 2.00, Rank = 6, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 32, PercentUsed = 3.20, Rank = 8, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 9, PercentUsed = 0.90, Rank = 10, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 19, PercentUsed = 1.90, Rank = 5, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 25, PercentUsed = 2.50, Rank = 7, }, 

            }
        };
        public static ItemRank ManaldHeal = new ItemRank
        {
            Item = Legendary.ManaldHeal,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 38, PercentUsed = 3.81, Rank = 7, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 27, PercentUsed = 2.72, Rank = 8, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {

            }
        };
        public static ItemRank TheWailingHost = new ItemRank
        {
            Item = Legendary.TheWailingHost,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 28, PercentUsed = 2.81, Rank = 8, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 15, PercentUsed = 1.51, Rank = 9, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 14, PercentUsed = 1.40, Rank = 9, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 13, PercentUsed = 1.30, Rank = 9, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 19, PercentUsed = 1.90, Rank = 9, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {

            }
        };
        public static ItemRank BandOfTheRueChambers = new ItemRank
        {
            Item = Legendary.BandOfTheRueChambers,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 26, PercentUsed = 2.61, Rank = 9, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 14, PercentUsed = 1.40, Rank = 7, }, 

            }
        };
        public static ItemRank RogarsHugeStone = new ItemRank
        {
            Item = Legendary.RogarsHugeStone,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 25, PercentUsed = 2.51, Rank = 10, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 24, PercentUsed = 2.42, Rank = 7, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 18, PercentUsed = 1.81, Rank = 10, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 28, PercentUsed = 2.80, Rank = 6, }, 

            }
        };
        public static ItemRank WonKhimLau = new ItemRank
        {
            Item = Legendary.WonKhimLau,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 57, PercentUsed = 5.71, Rank = 4, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 35, PercentUsed = 3.50, Rank = 4, }, 

            }
        };
        public static ItemRank ThunderfuryBlessedBladeOfTheWindseeker = new ItemRank
        {
            Item = Legendary.ThunderfuryBlessedBladeOfTheWindseeker,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 59, PercentUsed = 5.91, Rank = 3, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 98, PercentUsed = 9.89, Rank = 2, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 47, PercentUsed = 4.71, Rank = 6, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 87, PercentUsed = 8.70, Rank = 5, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 23, PercentUsed = 2.30, Rank = 7, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 5, PercentUsed = 0.50, Rank = 8, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 24, PercentUsed = 2.40, Rank = 8, }, 

            }
        };
        public static ItemRank Jawbreaker = new ItemRank
        {
            Item = Legendary.Jawbreaker,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 29, PercentUsed = 2.91, Rank = 3, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 17, PercentUsed = 1.70, Rank = 8, }, 

            }
        };
        public static ItemRank OdynSon = new ItemRank
        {
            Item = Legendary.OdynSon,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 22, PercentUsed = 2.20, Rank = 8, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 71, PercentUsed = 7.16, Rank = 4, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 32, PercentUsed = 3.20, Rank = 6, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 42, PercentUsed = 4.20, Rank = 4, }, 

            }
        };
        public static ItemRank SledgeFist = new ItemRank
        {
            Item = Legendary.SledgeFist,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 29, PercentUsed = 2.91, Rank = 7, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 12, PercentUsed = 1.20, Rank = 8, }, 

            }
        };
        public static ItemRank ShardOfHate = new ItemRank
        {
            Item = Legendary.ShardOfHate,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 20, PercentUsed = 2.00, Rank = 6, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 39, PercentUsed = 3.94, Rank = 7, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 13, PercentUsed = 1.30, Rank = 7, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 18, PercentUsed = 1.80, Rank = 9, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 6, PercentUsed = 0.60, Rank = 9, }, 

            }
        };
        public static ItemRank BornsFuriousWrath = new ItemRank
        {
            Item = Legendary.BornsFuriousWrath,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 31, PercentUsed = 3.11, Rank = 6, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 18, PercentUsed = 1.80, Rank = 10, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 33, PercentUsed = 3.30, Rank = 5, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 5, PercentUsed = 0.50, Rank = 9, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 45, PercentUsed = 4.50, Rank = 6, }, 

            }
        };
        public static ItemRank Fulminator = new ItemRank
        {
            Item = Legendary.Fulminator,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 12, PercentUsed = 1.20, Rank = 8, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 11, PercentUsed = 1.11, Rank = 10, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {

            }
        };
        public static ItemRank TheFistOfAzturrasq = new ItemRank
        {
            Item = Legendary.TheFistOfAzturrasq,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 10, PercentUsed = 1.00, Rank = 9, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 27, PercentUsed = 2.70, Rank = 3, }, 

            }
        };
        public static ItemRank Azurewrath = new ItemRank
        {
            Item = Legendary.Azurewrath,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 10, PercentUsed = 1.00, Rank = 10, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 18, PercentUsed = 1.81, Rank = 8, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 8, PercentUsed = 0.80, Rank = 10, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 9, PercentUsed = 0.90, Rank = 8, }, 

            }
        };
        public static ItemRank FlyingDragon = new ItemRank
        {
            Item = Legendary.FlyingDragon,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 281, PercentUsed = 28.16, Rank = 1, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 248, PercentUsed = 24.82, Rank = 2, }, 

            }
        };
        public static ItemRank IncenseTorchOfTheGrandTemple = new ItemRank
        {
            Item = Legendary.IncenseTorchOfTheGrandTemple,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 119, PercentUsed = 11.92, Rank = 2, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 399, PercentUsed = 39.94, Rank = 1, }, 

            }
        };
        public static ItemRank InnasReach = new ItemRank
        {
            Item = Legendary.InnasReach,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 53, PercentUsed = 5.31, Rank = 5, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 16, PercentUsed = 1.60, Rank = 9, }, 

            }
        };
        public static ItemRank TheFlowOfEternity = new ItemRank
        {
            Item = Legendary.TheFlowOfEternity,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 21, PercentUsed = 2.10, Rank = 9, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {

            }
        };
        public static ItemRank TheFurnace = new ItemRank
        {
            Item = Legendary.TheFurnace,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 17, PercentUsed = 1.70, Rank = 10, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 94, PercentUsed = 9.49, Rank = 3, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 17, PercentUsed = 1.71, Rank = 9, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 123, PercentUsed = 12.30, Rank = 4, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 169, PercentUsed = 17.00, Rank = 2, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 45, PercentUsed = 4.50, Rank = 3, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 515, PercentUsed = 51.50, Rank = 1, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 191, PercentUsed = 19.10, Rank = 2, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 273, PercentUsed = 27.30, Rank = 1, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 447, PercentUsed = 44.70, Rank = 1, }, 

            }
        };
        public static ItemRank CusterianWristguards = new ItemRank
        {
            Item = Legendary.CusterianWristguards,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 31, PercentUsed = 3.13, Rank = 5, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 37, PercentUsed = 3.70, Rank = 8, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 16, PercentUsed = 1.60, Rank = 5, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 24, PercentUsed = 2.40, Rank = 7, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 16, PercentUsed = 1.61, Rank = 6, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 13, PercentUsed = 1.30, Rank = 7, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 10, PercentUsed = 1.00, Rank = 9, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 45, PercentUsed = 4.50, Rank = 4, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 26, PercentUsed = 2.60, Rank = 6, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 17, PercentUsed = 1.70, Rank = 8, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 19, PercentUsed = 1.90, Rank = 4, }, 

            }
        };
        public static ItemRank Madstone = new ItemRank
        {
            Item = Legendary.Madstone,

            HardcoreRank = new List<ItemRankData>
            {

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 35, PercentUsed = 3.50, Rank = 8, }, 

            }
        };
        public static ItemRank TheLawsOfSeph = new ItemRank
        {
            Item = Legendary.TheLawsOfSeph,

            HardcoreRank = new List<ItemRankData>
            {

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 31, PercentUsed = 3.10, Rank = 9, }, 

            }
        };
        public static ItemRank AndarielsVisage = new ItemRank
        {
            Item = Legendary.AndarielsVisage,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 67, PercentUsed = 6.76, Rank = 5, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 30, PercentUsed = 3.00, Rank = 4, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 9, PercentUsed = 0.90, Rank = 7, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 56, PercentUsed = 5.60, Rank = 3, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 33, PercentUsed = 3.32, Rank = 4, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 25, PercentUsed = 2.50, Rank = 10, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 30, PercentUsed = 3.00, Rank = 5, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 45, PercentUsed = 4.50, Rank = 3, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 5, PercentUsed = 0.50, Rank = 7, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 22, PercentUsed = 2.20, Rank = 3, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 15, PercentUsed = 1.50, Rank = 4, }, 

            }
        };
        public static ItemRank VileWard = new ItemRank
        {
            Item = Legendary.VileWard,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 152, PercentUsed = 15.34, Rank = 4, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 6, PercentUsed = 0.60, Rank = 7, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 4, PercentUsed = 0.40, Rank = 10, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 12, PercentUsed = 1.20, Rank = 7, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 2, PercentUsed = 0.20, Rank = 8, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 701, PercentUsed = 70.10, Rank = 1, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 2, PercentUsed = 0.20, Rank = 7, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 2, PercentUsed = 0.20, Rank = 7, }, 

            }
        };
        public static ItemRank Goldskin = new ItemRank
        {
            Item = Legendary.Goldskin,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 11, PercentUsed = 1.11, Rank = 7, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 1, PercentUsed = 0.10, Rank = 9, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 14, PercentUsed = 1.40, Rank = 8, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 5, PercentUsed = 0.50, Rank = 9, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 4, PercentUsed = 0.40, Rank = 8, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 8, PercentUsed = 0.80, Rank = 8, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 2, PercentUsed = 0.20, Rank = 8, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 2, PercentUsed = 0.20, Rank = 4, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 8, PercentUsed = 0.80, Rank = 8, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 3, PercentUsed = 0.30, Rank = 7, }, 

            }
        };
        public static ItemRank ShiMizusHaori = new ItemRank
        {
            Item = Legendary.ShiMizusHaori,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 2, PercentUsed = 0.20, Rank = 10, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 2, PercentUsed = 0.20, Rank = 10, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 3, PercentUsed = 0.30, Rank = 10, }, 

            }
        };
        public static ItemRank TaskerAndTheo = new ItemRank
        {
            Item = Legendary.TaskerAndTheo,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 60, PercentUsed = 6.05, Rank = 5, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 422, PercentUsed = 42.24, Rank = 1, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 589, PercentUsed = 59.08, Rank = 1, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 47, PercentUsed = 4.70, Rank = 3, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 31, PercentUsed = 3.12, Rank = 4, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 2, PercentUsed = 0.20, Rank = 10, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 74, PercentUsed = 7.40, Rank = 3, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 238, PercentUsed = 23.80, Rank = 2, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 494, PercentUsed = 49.40, Rank = 1, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 75, PercentUsed = 7.50, Rank = 3, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 11, PercentUsed = 1.10, Rank = 7, }, 

            }
        };
        public static ItemRank HarringtonWaistguard = new ItemRank
        {
            Item = Legendary.HarringtonWaistguard,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 31, PercentUsed = 3.13, Rank = 7, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 56, PercentUsed = 5.61, Rank = 6, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 71, PercentUsed = 7.12, Rank = 4, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 32, PercentUsed = 3.20, Rank = 7, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 37, PercentUsed = 3.72, Rank = 6, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 15, PercentUsed = 1.50, Rank = 9, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 8, PercentUsed = 0.80, Rank = 10, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 17, PercentUsed = 1.70, Rank = 3, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 26, PercentUsed = 2.60, Rank = 6, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 38, PercentUsed = 3.80, Rank = 4, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 22, PercentUsed = 2.20, Rank = 4, }, 

            }
        };
        public static ItemRank EyeOfEtlich = new ItemRank
        {
            Item = Legendary.EyeOfEtlich,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 34, PercentUsed = 3.40, Rank = 9, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 53, PercentUsed = 5.30, Rank = 6, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 13, PercentUsed = 1.30, Rank = 10, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 60, PercentUsed = 6.00, Rank = 6, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 16, PercentUsed = 1.60, Rank = 7, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 42, PercentUsed = 4.20, Rank = 9, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 141, PercentUsed = 14.10, Rank = 1, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 58, PercentUsed = 5.80, Rank = 6, }, 

            }
        };
        public static ItemRank AvariceBand = new ItemRank
        {
            Item = Legendary.AvariceBand,

            HardcoreRank = new List<ItemRankData>
            {

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 28, PercentUsed = 2.80, Rank = 4, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 17, PercentUsed = 1.70, Rank = 7, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 76, PercentUsed = 7.60, Rank = 5, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 11, PercentUsed = 1.10, Rank = 9, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 13, PercentUsed = 1.30, Rank = 6, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 17, PercentUsed = 1.70, Rank = 10, }, 

            }
        };
        public static ItemRank NatalyasReflection = new ItemRank
        {
            Item = Legendary.NatalyasReflection,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 121, PercentUsed = 12.11, Rank = 4, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 13, PercentUsed = 1.30, Rank = 8, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 55, PercentUsed = 5.50, Rank = 6, }, 

            }
        };
        public static ItemRank Wyrdward = new ItemRank
        {
            Item = Legendary.Wyrdward,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 18, PercentUsed = 1.80, Rank = 10, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 12, PercentUsed = 1.20, Rank = 9, }, 

            }
        };
        public static ItemRank SunKeeper = new ItemRank
        {
            Item = Legendary.SunKeeper,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 38, PercentUsed = 3.83, Rank = 8, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 78, PercentUsed = 7.82, Rank = 4, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 132, PercentUsed = 13.20, Rank = 2, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Monk, SampleSize = 16, PercentUsed = 1.60, Rank = 10, }, 
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 20, PercentUsed = 2.00, Rank = 8, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 114, PercentUsed = 11.40, Rank = 3, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 153, PercentUsed = 15.30, Rank = 3, }, 

            }
        };
        public static ItemRank SanguinaryVambraces = new ItemRank
        {
            Item = Legendary.SanguinaryVambraces,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 21, PercentUsed = 2.12, Rank = 8, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 8, PercentUsed = 0.80, Rank = 9, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 7, PercentUsed = 0.70, Rank = 10, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 5, PercentUsed = 0.50, Rank = 9, }, 

            }
        };
        public static ItemRank ImmortalKingsTriumph = new ItemRank
        {
            Item = Legendary.ImmortalKingsTriumph,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 327, PercentUsed = 33.00, Rank = 1, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 113, PercentUsed = 11.30, Rank = 3, }, 

            }
        };
        public static ItemRank RaekorsWill = new ItemRank
        {
            Item = Legendary.RaekorsWill,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 11, PercentUsed = 1.11, Rank = 10, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 3, PercentUsed = 0.30, Rank = 7, }, 

            }
        };
        public static ItemRank EyesOfTheEarth = new ItemRank
        {
            Item = Legendary.EyesOfTheEarth,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 113, PercentUsed = 11.40, Rank = 3, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 63, PercentUsed = 6.30, Rank = 4, }, 

            }
        };
        public static ItemRank SkullOfResonance = new ItemRank
        {
            Item = Legendary.SkullOfResonance,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 40, PercentUsed = 4.04, Rank = 6, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 5, PercentUsed = 0.50, Rank = 6, }, 

            }
        };
        public static ItemRank MempoOfTwilight = new ItemRank
        {
            Item = Legendary.MempoOfTwilight,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 18, PercentUsed = 1.82, Rank = 8, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 2, PercentUsed = 0.20, Rank = 10, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 11, PercentUsed = 1.10, Rank = 8, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 8, PercentUsed = 0.80, Rank = 8, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 5, PercentUsed = 0.50, Rank = 6, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 2, PercentUsed = 0.20, Rank = 10, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 4, PercentUsed = 0.40, Rank = 8, }, 

            }
        };
        public static ItemRank RaekorsBurden = new ItemRank
        {
            Item = Legendary.RaekorsBurden,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 13, PercentUsed = 1.31, Rank = 9, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 3, PercentUsed = 0.30, Rank = 7, }, 

            }
        };
        public static ItemRank SpiresOfTheEarth = new ItemRank
        {
            Item = Legendary.SpiresOfTheEarth,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 179, PercentUsed = 18.06, Rank = 3, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 108, PercentUsed = 10.80, Rank = 3, }, 

            }
        };
        public static ItemRank RaekorsHeart = new ItemRank
        {
            Item = Legendary.RaekorsHeart,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 7, PercentUsed = 0.71, Rank = 9, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 2, PercentUsed = 0.20, Rank = 9, }, 

            }
        };
        public static ItemRank Chaingmail = new ItemRank
        {
            Item = Legendary.Chaingmail,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 5, PercentUsed = 0.50, Rank = 10, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {

            }
        };
        public static ItemRank ImmortalKingsIrons = new ItemRank
        {
            Item = Legendary.ImmortalKingsIrons,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 309, PercentUsed = 31.18, Rank = 1, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 64, PercentUsed = 6.40, Rank = 4, }, 

            }
        };
        public static ItemRank RaekorsWraps = new ItemRank
        {
            Item = Legendary.RaekorsWraps,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 11, PercentUsed = 1.11, Rank = 8, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 4, PercentUsed = 0.40, Rank = 7, }, 

            }
        };
        public static ItemRank PullOfTheEarth = new ItemRank
        {
            Item = Legendary.PullOfTheEarth,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 143, PercentUsed = 14.43, Rank = 3, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 84, PercentUsed = 8.40, Rank = 2, }, 

            }
        };
        public static ItemRank ImmortalKingsTribalBinding = new ItemRank
        {
            Item = Legendary.ImmortalKingsTribalBinding,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 452, PercentUsed = 45.61, Rank = 1, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 95, PercentUsed = 9.50, Rank = 3, }, 

            }
        };
        public static ItemRank PrideOfCassius = new ItemRank
        {
            Item = Legendary.PrideOfCassius,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 112, PercentUsed = 11.30, Rank = 2, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 461, PercentUsed = 46.10, Rank = 1, }, 

            }
        };
        public static ItemRank ChilaniksChain = new ItemRank
        {
            Item = Legendary.ChilaniksChain,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 55, PercentUsed = 5.55, Rank = 4, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 20, PercentUsed = 2.00, Rank = 6, }, 

            }
        };
        public static ItemRank GirdleOfGiants = new ItemRank
        {
            Item = Legendary.GirdleOfGiants,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 14, PercentUsed = 1.41, Rank = 10, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {

            }
        };
        public static ItemRank RaekorsBreeches = new ItemRank
        {
            Item = Legendary.RaekorsBreeches,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 13, PercentUsed = 1.31, Rank = 10, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 730, PercentUsed = 73.00, Rank = 1, }, 

            }
        };
        public static ItemRank WeightOfTheEarth = new ItemRank
        {
            Item = Legendary.WeightOfTheEarth,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 193, PercentUsed = 19.48, Rank = 2, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 91, PercentUsed = 9.10, Rank = 3, }, 

            }
        };
        public static ItemRank PoxFaulds = new ItemRank
        {
            Item = Legendary.PoxFaulds,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 20, PercentUsed = 2.02, Rank = 8, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 18, PercentUsed = 1.81, Rank = 9, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 5, PercentUsed = 0.50, Rank = 9, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 4, PercentUsed = 0.40, Rank = 8, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 21, PercentUsed = 2.10, Rank = 6, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 2, PercentUsed = 0.20, Rank = 8, }, 

            }
        };
        public static ItemRank ImmortalKingsStride = new ItemRank
        {
            Item = Legendary.ImmortalKingsStride,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 263, PercentUsed = 26.54, Rank = 1, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 104, PercentUsed = 10.40, Rank = 2, }, 

            }
        };
        public static ItemRank RaekorsStriders = new ItemRank
        {
            Item = Legendary.RaekorsStriders,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 10, PercentUsed = 1.01, Rank = 10, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 4, PercentUsed = 0.40, Rank = 8, }, 

            }
        };
        public static ItemRank LutSocks = new ItemRank
        {
            Item = Legendary.LutSocks,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 198, PercentUsed = 19.98, Rank = 3, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 103, PercentUsed = 10.30, Rank = 3, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 2, PercentUsed = 0.20, Rank = 8, }, 

            }
        };
        public static ItemRank XephirianAmulet = new ItemRank
        {
            Item = Legendary.XephirianAmulet,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 25, PercentUsed = 2.52, Rank = 10, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 44, PercentUsed = 4.40, Rank = 10, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 109, PercentUsed = 10.90, Rank = 2, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 84, PercentUsed = 8.40, Rank = 2, }, 

            }
        };
        public static ItemRank SkullGrasp = new ItemRank
        {
            Item = Legendary.SkullGrasp,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 22, PercentUsed = 2.22, Rank = 8, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 23, PercentUsed = 2.30, Rank = 8, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 14, PercentUsed = 1.40, Rank = 8, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 6, PercentUsed = 0.60, Rank = 10, }, 

            }
        };
        public static ItemRank Nagelring = new ItemRank
        {
            Item = Legendary.Nagelring,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 14, PercentUsed = 1.41, Rank = 10, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 24, PercentUsed = 2.40, Rank = 7, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 8, PercentUsed = 0.80, Rank = 9, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 5, PercentUsed = 0.50, Rank = 10, }, 

            }
        };
        public static ItemRank TheBurningAxeOfSankis = new ItemRank
        {
            Item = Legendary.TheBurningAxeOfSankis,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 44, PercentUsed = 4.44, Rank = 6, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 48, PercentUsed = 4.80, Rank = 6, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 12, PercentUsed = 1.20, Rank = 4, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 33, PercentUsed = 3.30, Rank = 6, }, 

            }
        };
        public static ItemRank Devastator = new ItemRank
        {
            Item = Legendary.Devastator,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 31, PercentUsed = 3.13, Rank = 6, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 124, PercentUsed = 12.40, Rank = 3, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 6, PercentUsed = 0.60, Rank = 5, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 258, PercentUsed = 25.80, Rank = 2, }, 

            }
        };
        public static ItemRank Doombringer = new ItemRank
        {
            Item = Legendary.Doombringer,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 17, PercentUsed = 1.72, Rank = 7, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 92, PercentUsed = 9.23, Rank = 3, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 5, PercentUsed = 0.50, Rank = 6, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 9, PercentUsed = 0.90, Rank = 9, }, 

            }
        };
        public static ItemRank BulkathossWarriorBlood = new ItemRank
        {
            Item = Legendary.BulkathossWarriorBlood,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 14, PercentUsed = 1.41, Rank = 8, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 3, PercentUsed = 0.30, Rank = 10, }, 

            }
        };
        public static ItemRank Stormshield = new ItemRank
        {
            Item = Legendary.Stormshield,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 13, PercentUsed = 1.31, Rank = 9, }, 
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 9, PercentUsed = 0.90, Rank = 6, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 2, PercentUsed = 0.20, Rank = 10, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 49, PercentUsed = 4.90, Rank = 6, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 1, PercentUsed = 0.10, Rank = 10, }, 

            }
        };
        public static ItemRank Maximus = new ItemRank
        {
            Item = Legendary.Maximus,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 171, PercentUsed = 17.26, Rank = 1, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 48, PercentUsed = 4.80, Rank = 7, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 52, PercentUsed = 5.23, Rank = 5, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 121, PercentUsed = 12.10, Rank = 2, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 25, PercentUsed = 2.50, Rank = 7, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 16, PercentUsed = 1.60, Rank = 7, }, 

            }
        };
        public static ItemRank ImmortalKingsBoulderBreaker = new ItemRank
        {
            Item = Legendary.ImmortalKingsBoulderBreaker,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 71, PercentUsed = 7.16, Rank = 5, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 20, PercentUsed = 2.00, Rank = 7, }, 

            }
        };
        public static ItemRank SchaefersHammer = new ItemRank
        {
            Item = Legendary.SchaefersHammer,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 32, PercentUsed = 3.23, Rank = 9, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {

            }
        };
        public static ItemRank StalgardsDecimator = new ItemRank
        {
            Item = Legendary.StalgardsDecimator,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 30, PercentUsed = 3.03, Rank = 10, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 26, PercentUsed = 2.60, Rank = 6, }, 

            }
        };
        public static ItemRank SagesApogee = new ItemRank
        {
            Item = Legendary.SagesApogee,

            HardcoreRank = new List<ItemRankData>
            {

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 2, PercentUsed = 0.20, Rank = 9, }, 

            }
        };
        public static ItemRank PridesFall = new ItemRank
        {
            Item = Legendary.PridesFall,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 78, PercentUsed = 7.81, Rank = 2, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 15, PercentUsed = 1.50, Rank = 6, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 1, PercentUsed = 0.10, Rank = 10, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 178, PercentUsed = 17.80, Rank = 2, }, 

            }
        };
        public static ItemRank SagesPassage = new ItemRank
        {
            Item = Legendary.SagesPassage,

            HardcoreRank = new List<ItemRankData>
            {

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 2, PercentUsed = 0.20, Rank = 10, }, 

            }
        };
        public static ItemRank TheStarOfAzkaranth = new ItemRank
        {
            Item = Legendary.TheStarOfAzkaranth,

            HardcoreRank = new List<ItemRankData>
            {

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 138, PercentUsed = 13.80, Rank = 2, }, 

            }
        };
        public static ItemRank TheFlavorOfTime = new ItemRank
        {
            Item = Legendary.TheFlavorOfTime,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 44, PercentUsed = 4.43, Rank = 10, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 57, PercentUsed = 5.70, Rank = 8, }, 
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 8, PercentUsed = 0.80, Rank = 10, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 64, PercentUsed = 6.40, Rank = 5, }, 

            }
        };

        public static ItemRank Focus = new ItemRank
        {
            Item = Legendary.Focus,

            HardcoreRank = new List<ItemRankData>
            {

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 7, PercentUsed = 0.70, Rank = 10, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 7, PercentUsed = 0.70, Rank = 9, }, 

            }
        };
        public static ItemRank TheThreeHundredthSpear = new ItemRank
        {
            Item = Legendary.TheThreeHundredthSpear,

            HardcoreRank = new List<ItemRankData>
            {

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 5, PercentUsed = 0.50, Rank = 7, }, 

            }
        };
        public static ItemRank HeartSlaughter = new ItemRank
        {
            Item = Legendary.HeartSlaughter,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 136, PercentUsed = 13.68, Rank = 3, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 56, PercentUsed = 5.60, Rank = 3, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 113, PercentUsed = 11.30, Rank = 2, }, 

            }
        };
        public static ItemRank FuryOfTheVanishedPeak = new ItemRank
        {
            Item = Legendary.FuryOfTheVanishedPeak,

            HardcoreRank = new List<ItemRankData>
            {

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 30, PercentUsed = 3.00, Rank = 5, }, 

            }
        };
        public static ItemRank SledgeOfAthskeleng = new ItemRank
        {
            Item = Legendary.SledgeOfAthskeleng,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 16, PercentUsed = 1.61, Rank = 9, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 15, PercentUsed = 1.50, Rank = 10, }, 

            }
        };
        public static ItemRank GuardiansAversion = new ItemRank
        {
            Item = Legendary.GuardiansAversion,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 19, PercentUsed = 1.90, Rank = 9, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {

            }
        };
        public static ItemRank MaraudersVisage = new ItemRank
        {
            Item = Legendary.MaraudersVisage,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 780, PercentUsed = 78.08, Rank = 1, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 712, PercentUsed = 71.20, Rank = 1, }, 

            }
        };
        public static ItemRank NatalyasSight = new ItemRank
        {
            Item = Legendary.NatalyasSight,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 43, PercentUsed = 4.30, Rank = 3, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 8, PercentUsed = 0.80, Rank = 5, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 1, PercentUsed = 0.10, Rank = 9, }, 

            }
        };
        public static ItemRank BrokenCrown = new ItemRank
        {
            Item = Legendary.BrokenCrown,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 9, PercentUsed = 0.90, Rank = 7, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 4, PercentUsed = 0.40, Rank = 9, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {

            }
        };
        public static ItemRank BlindFaith = new ItemRank
        {
            Item = Legendary.BlindFaith,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 3, PercentUsed = 0.30, Rank = 8, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 2, PercentUsed = 0.20, Rank = 8, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 3, PercentUsed = 0.30, Rank = 6, }, 

            }
        };
        public static ItemRank MaraudersSpines = new ItemRank
        {
            Item = Legendary.MaraudersSpines,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 875, PercentUsed = 87.59, Rank = 1, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 947, PercentUsed = 94.70, Rank = 1, }, 

            }
        };
        public static ItemRank Corruption = new ItemRank
        {
            Item = Legendary.Corruption,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 2, PercentUsed = 0.20, Rank = 10, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 5, PercentUsed = 0.50, Rank = 7, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 3, PercentUsed = 0.30, Rank = 5, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 2, PercentUsed = 0.20, Rank = 8, }, 

            }
        };
        public static ItemRank MaraudersCarapace = new ItemRank
        {
            Item = Legendary.MaraudersCarapace,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 742, PercentUsed = 74.27, Rank = 1, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 489, PercentUsed = 48.90, Rank = 1, }, 

            }
        };
        public static ItemRank NatalyasEmbrace = new ItemRank
        {
            Item = Legendary.NatalyasEmbrace,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 23, PercentUsed = 2.30, Rank = 3, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 6, PercentUsed = 0.60, Rank = 3, }, 

            }
        };
        public static ItemRank TheCloakOfTheGarwulf = new ItemRank
        {
            Item = Legendary.TheCloakOfTheGarwulf,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 20, PercentUsed = 2.00, Rank = 4, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 1, PercentUsed = 0.10, Rank = 8, }, 

            }
        };
        public static ItemRank BeckonSail = new ItemRank
        {
            Item = Legendary.BeckonSail,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 17, PercentUsed = 1.70, Rank = 5, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {

            }
        };
        public static ItemRank TheShadowsBane = new ItemRank
        {
            Item = Legendary.TheShadowsBane,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 6, PercentUsed = 0.60, Rank = 8, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 2, PercentUsed = 0.20, Rank = 6, }, 

            }
        };
        public static ItemRank CapeOfTheDarkNight = new ItemRank
        {
            Item = Legendary.CapeOfTheDarkNight,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 1, PercentUsed = 0.10, Rank = 10, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {

            }
        };
        public static ItemRank MaraudersGloves = new ItemRank
        {
            Item = Legendary.MaraudersGloves,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 384, PercentUsed = 38.44, Rank = 2, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 661, PercentUsed = 66.10, Rank = 1, }, 

            }
        };
        public static ItemRank TheShadowsGrasp = new ItemRank
        {
            Item = Legendary.TheShadowsGrasp,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 7, PercentUsed = 0.70, Rank = 5, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 1, PercentUsed = 0.10, Rank = 8, }, 

            }
        };
        public static ItemRank SagesGesture = new ItemRank
        {
            Item = Legendary.SagesGesture,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 4, PercentUsed = 0.40, Rank = 9, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {

            }
        };
        public static ItemRank SaffronWrap = new ItemRank
        {
            Item = Legendary.SaffronWrap,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 27, PercentUsed = 2.70, Rank = 8, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 9, PercentUsed = 0.90, Rank = 4, }, 

            }
        };
        public static ItemRank GuardiansCase = new ItemRank
        {
            Item = Legendary.GuardiansCase,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 19, PercentUsed = 1.90, Rank = 10, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {

            }
        };
        public static ItemRank MaraudersEncasement = new ItemRank
        {
            Item = Legendary.MaraudersEncasement,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 856, PercentUsed = 85.69, Rank = 1, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 922, PercentUsed = 92.20, Rank = 1, }, 

            }
        };
        public static ItemRank TheShadowsCoil = new ItemRank
        {
            Item = Legendary.TheShadowsCoil,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 9, PercentUsed = 0.90, Rank = 7, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 1, PercentUsed = 0.10, Rank = 9, }, 

            }
        };
        public static ItemRank MaraudersTreads = new ItemRank
        {
            Item = Legendary.MaraudersTreads,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 839, PercentUsed = 83.98, Rank = 1, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 913, PercentUsed = 91.30, Rank = 1, }, 

            }
        };
        public static ItemRank TheShadowsHeels = new ItemRank
        {
            Item = Legendary.TheShadowsHeels,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 5, PercentUsed = 0.50, Rank = 10, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 3, PercentUsed = 0.30, Rank = 3, }, 

            }
        };
        public static ItemRank StolenRing = new ItemRank
        {
            Item = Legendary.StolenRing,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 12, PercentUsed = 1.20, Rank = 10, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {

            }
        };
        public static ItemRank BombadiersRucksack = new ItemRank
        {
            Item = Legendary.BombadiersRucksack,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 531, PercentUsed = 53.15, Rank = 1, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 134, PercentUsed = 13.40, Rank = 2, }, 

            }
        };

        public static ItemRank MeticulousBolts = new ItemRank
        {
            Item = Legendary.MeticulousBolts,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 57, PercentUsed = 5.71, Rank = 3, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 119, PercentUsed = 11.90, Rank = 3, }, 

            }
        };
        public static ItemRank DeadMansLegacy = new ItemRank
        {
            Item = Legendary.DeadMansLegacy,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 32, PercentUsed = 3.20, Rank = 4, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 2, PercentUsed = 0.20, Rank = 9, }, 

            }
        };
        public static ItemRank SpinesOfSeethingHatred = new ItemRank
        {
            Item = Legendary.SpinesOfSeethingHatred,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 30, PercentUsed = 3.00, Rank = 5, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {

            }
        };
        public static ItemRank SinSeekers = new ItemRank
        {
            Item = Legendary.SinSeekers,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 24, PercentUsed = 2.40, Rank = 6, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {

            }
        };
        public static ItemRank ArchfiendArrows = new ItemRank
        {
            Item = Legendary.ArchfiendArrows,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 22, PercentUsed = 2.20, Rank = 7, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {

            }
        };
        public static ItemRank DanettasRevenge = new ItemRank
        {
            Item = Legendary.DanettasRevenge,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 20, PercentUsed = 2.00, Rank = 8, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 31, PercentUsed = 3.10, Rank = 8, }, 

            }
        };
        public static ItemRank HolyPointShot = new ItemRank
        {
            Item = Legendary.HolyPointShot,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 19, PercentUsed = 1.90, Rank = 9, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 37, PercentUsed = 3.70, Rank = 4, }, 

            }
        };
        public static ItemRank EmimeisDuffel = new ItemRank
        {
            Item = Legendary.EmimeisDuffel,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 11, PercentUsed = 1.10, Rank = 10, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {

            }
        };
        public static ItemRank Etrayu = new ItemRank
        {
            Item = Legendary.Etrayu,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 189, PercentUsed = 18.92, Rank = 1, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 68, PercentUsed = 6.80, Rank = 3, }, 

            }
        };
        public static ItemRank NatalyasSlayer = new ItemRank
        {
            Item = Legendary.NatalyasSlayer,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 131, PercentUsed = 13.11, Rank = 2, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 57, PercentUsed = 5.70, Rank = 5, }, 

            }
        };
        public static ItemRank ArcaneBarb = new ItemRank
        {
            Item = Legendary.ArcaneBarb,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 112, PercentUsed = 11.21, Rank = 3, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 348, PercentUsed = 34.80, Rank = 1, }, 

            }
        };
        public static ItemRank BurizadoKyanon = new ItemRank
        {
            Item = Legendary.BurizadoKyanon,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 74, PercentUsed = 7.41, Rank = 4, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 59, PercentUsed = 5.90, Rank = 4, }, 

            }
        };
        public static ItemRank Calamity = new ItemRank
        {
            Item = Legendary.Calamity,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 68, PercentUsed = 6.81, Rank = 5, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 27, PercentUsed = 2.70, Rank = 9, }, 

            }
        };
        public static ItemRank UnboundBolt = new ItemRank
        {
            Item = Legendary.UnboundBolt,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 44, PercentUsed = 4.40, Rank = 6, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {

            }
        };
        public static ItemRank Helltrapper = new ItemRank
        {
            Item = Legendary.Helltrapper,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 43, PercentUsed = 4.30, Rank = 7, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 109, PercentUsed = 10.90, Rank = 2, }, 

            }
        };
        public static ItemRank Kridershot = new ItemRank
        {
            Item = Legendary.Kridershot,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 39, PercentUsed = 3.90, Rank = 8, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 39, PercentUsed = 3.90, Rank = 7, }, 

            }
        };
        public static ItemRank Manticore = new ItemRank
        {
            Item = Legendary.Manticore,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 37, PercentUsed = 3.70, Rank = 9, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {

            }
        };
        public static ItemRank Windforce = new ItemRank
        {
            Item = Legendary.Windforce,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 33, PercentUsed = 3.30, Rank = 10, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {

            }
        };
        public static ItemRank TragoulCoils = new ItemRank
        {
            Item = Legendary.TragoulCoils,

            HardcoreRank = new List<ItemRankData>
            {

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 2, PercentUsed = 0.20, Rank = 10, }, 

            }
        };
        public static ItemRank DeathseersCowl = new ItemRank
        {
            Item = Legendary.DeathseersCowl,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 3, PercentUsed = 0.30, Rank = 10, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 1, PercentUsed = 0.10, Rank = 10, }, 

            }
        };

        public static ItemRank KrelmsBuffBelt = new ItemRank
        {
            Item = Legendary.KrelmsBuffBelt,

            HardcoreRank = new List<ItemRankData>
            {

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 4, PercentUsed = 0.40, Rank = 8, }, 

            }
        };
        public static ItemRank SashOfKnives = new ItemRank
        {
            Item = Legendary.SashOfKnives,

            HardcoreRank = new List<ItemRankData>
            {

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 3, PercentUsed = 0.30, Rank = 10, }, 

            }
        };

        public static ItemRank KymbosGold = new ItemRank
        {
            Item = Legendary.KymbosGold,

            HardcoreRank = new List<ItemRankData>
            {

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 12, PercentUsed = 1.20, Rank = 8, }, 

            }
        };
        public static ItemRank BandOfUntoldSecrets = new ItemRank
        {
            Item = Legendary.BandOfUntoldSecrets,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 13, PercentUsed = 1.30, Rank = 10, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 25, PercentUsed = 2.50, Rank = 7, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 20, PercentUsed = 2.01, Rank = 9, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 7, PercentUsed = 0.70, Rank = 9, }, 
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 30, PercentUsed = 3.00, Rank = 5, }, 

            }
        };
        public static ItemRank DanettasSpite = new ItemRank
        {
            Item = Legendary.DanettasSpite,

            HardcoreRank = new List<ItemRankData>
            {

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 27, PercentUsed = 2.70, Rank = 10, }, 

            }
        };
        public static ItemRank BalefireCaster = new ItemRank
        {
            Item = Legendary.BalefireCaster,

            HardcoreRank = new List<ItemRankData>
            {

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 11, PercentUsed = 1.10, Rank = 8, }, 

            }
        };
        public static ItemRank DemonMachine = new ItemRank
        {
            Item = Legendary.DemonMachine,

            HardcoreRank = new List<ItemRankData>
            {

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 48, PercentUsed = 4.80, Rank = 6, }, 

            }
        };
        public static ItemRank MaskOfJeram = new ItemRank
        {
            Item = Legendary.MaskOfJeram,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 698, PercentUsed = 70.01, Rank = 1, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 327, PercentUsed = 32.70, Rank = 2, }, 

            }
        };
        public static ItemRank Quetzalcoatl = new ItemRank
        {
            Item = Legendary.Quetzalcoatl,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 130, PercentUsed = 13.04, Rank = 2, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 344, PercentUsed = 34.40, Rank = 1, }, 

            }
        };
        public static ItemRank Carnevil = new ItemRank
        {
            Item = Legendary.Carnevil,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 67, PercentUsed = 6.72, Rank = 3, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 91, PercentUsed = 9.10, Rank = 4, }, 

            }
        };
        public static ItemRank TiklandianVisage = new ItemRank
        {
            Item = Legendary.TiklandianVisage,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 28, PercentUsed = 2.81, Rank = 4, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 195, PercentUsed = 19.50, Rank = 3, }, 

            }
        };
        public static ItemRank JadeHarvestersWisdom = new ItemRank
        {
            Item = Legendary.JadeHarvestersWisdom,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 15, PercentUsed = 1.50, Rank = 5, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {

            }
        };
        public static ItemRank ZunimassasVision = new ItemRank
        {
            Item = Legendary.ZunimassasVision,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 11, PercentUsed = 1.10, Rank = 6, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 3, PercentUsed = 0.30, Rank = 8, }, 

            }
        };
        public static ItemRank TheGrinReaper = new ItemRank
        {
            Item = Legendary.TheGrinReaper,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 6, PercentUsed = 0.60, Rank = 9, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 7, PercentUsed = 0.70, Rank = 6, }, 

            }
        };
        public static ItemRank HelltoothMask = new ItemRank
        {
            Item = Legendary.HelltoothMask,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 3, PercentUsed = 0.30, Rank = 10, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {

            }
        };
        public static ItemRank JadeHarvestersJoy = new ItemRank
        {
            Item = Legendary.JadeHarvestersJoy,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 171, PercentUsed = 17.15, Rank = 2, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 388, PercentUsed = 38.80, Rank = 2, }, 

            }
        };
        public static ItemRank HelltoothMantle = new ItemRank
        {
            Item = Legendary.HelltoothMantle,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 24, PercentUsed = 2.41, Rank = 3, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 22, PercentUsed = 2.20, Rank = 7, }, 

            }
        };
        public static ItemRank ZunimassasMarrow = new ItemRank
        {
            Item = Legendary.ZunimassasMarrow,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 399, PercentUsed = 40.02, Rank = 1, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 186, PercentUsed = 18.60, Rank = 3, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 1, PercentUsed = 0.10, Rank = 10, }, 

            }
        };
        public static ItemRank JadeHarvestersPeace = new ItemRank
        {
            Item = Legendary.JadeHarvestersPeace,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 148, PercentUsed = 14.84, Rank = 3, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 348, PercentUsed = 34.80, Rank = 1, }, 

            }
        };
        public static ItemRank HelltoothTunic = new ItemRank
        {
            Item = Legendary.HelltoothTunic,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 21, PercentUsed = 2.11, Rank = 6, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 5, PercentUsed = 0.50, Rank = 9, }, 

            }
        };
        public static ItemRank JadeHarvestersMercy = new ItemRank
        {
            Item = Legendary.JadeHarvestersMercy,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 200, PercentUsed = 20.06, Rank = 2, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 365, PercentUsed = 36.50, Rank = 2, }, 

            }
        };
        public static ItemRank HelltoothGauntlets = new ItemRank
        {
            Item = Legendary.HelltoothGauntlets,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 6, PercentUsed = 0.60, Rank = 8, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 12, PercentUsed = 1.20, Rank = 6, }, 

            }
        };
        public static ItemRank HwojWrap = new ItemRank
        {
            Item = Legendary.HwojWrap,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 63, PercentUsed = 6.32, Rank = 5, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 41, PercentUsed = 4.10, Rank = 5, }, 

            }
        };
        public static ItemRank JadeHarvestersCourage = new ItemRank
        {
            Item = Legendary.JadeHarvestersCourage,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 180, PercentUsed = 18.05, Rank = 2, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 364, PercentUsed = 36.40, Rank = 1, }, 

            }
        };
        public static ItemRank SwampLandWaders = new ItemRank
        {
            Item = Legendary.SwampLandWaders,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 88, PercentUsed = 8.83, Rank = 4, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 77, PercentUsed = 7.70, Rank = 5, }, 

            }
        };
        public static ItemRank HelltoothLegGuards = new ItemRank
        {
            Item = Legendary.HelltoothLegGuards,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 30, PercentUsed = 3.01, Rank = 6, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 6, PercentUsed = 0.60, Rank = 8, }, 

            }
        };
        public static ItemRank ZunimassasTrail = new ItemRank
        {
            Item = Legendary.ZunimassasTrail,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 377, PercentUsed = 37.81, Rank = 1, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 188, PercentUsed = 18.80, Rank = 2, }, 

            }
        };
        public static ItemRank JadeHarvestersSwiftness = new ItemRank
        {
            Item = Legendary.JadeHarvestersSwiftness,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 157, PercentUsed = 15.75, Rank = 2, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 373, PercentUsed = 37.30, Rank = 1, }, 

            }
        };
        public static ItemRank HelltoothGreaves = new ItemRank
        {
            Item = Legendary.HelltoothGreaves,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 22, PercentUsed = 2.21, Rank = 7, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 14, PercentUsed = 1.40, Rank = 7, }, 

            }
        };
        public static ItemRank RondalsLocket = new ItemRank
        {
            Item = Legendary.RondalsLocket,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 32, PercentUsed = 3.21, Rank = 10, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {

            }
        };
        public static ItemRank TheTallMansFinger = new ItemRank
        {
            Item = Legendary.TheTallMansFinger,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 328, PercentUsed = 32.90, Rank = 3, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 263, PercentUsed = 26.30, Rank = 4, }, 

            }
        };
        public static ItemRank ZunimassasPox = new ItemRank
        {
            Item = Legendary.ZunimassasPox,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 137, PercentUsed = 13.74, Rank = 5, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 26, PercentUsed = 2.60, Rank = 7, }, 

            }
        };
        public static ItemRank UhkapianSerpent = new ItemRank
        {
            Item = Legendary.UhkapianSerpent,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 459, PercentUsed = 46.04, Rank = 1, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 217, PercentUsed = 21.70, Rank = 1, }, 

            }
        };
        public static ItemRank ZunimassasStringOfSkulls = new ItemRank
        {
            Item = Legendary.ZunimassasStringOfSkulls,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 292, PercentUsed = 29.29, Rank = 2, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 176, PercentUsed = 17.60, Rank = 2, }, 

            }
        };
        public static ItemRank ThingOfTheDeep = new ItemRank
        {
            Item = Legendary.ThingOfTheDeep,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 125, PercentUsed = 12.54, Rank = 3, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 143, PercentUsed = 14.30, Rank = 3, }, 

            }
        };
        public static ItemRank Homunculus = new ItemRank
        {
            Item = Legendary.Homunculus,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 27, PercentUsed = 2.71, Rank = 4, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 117, PercentUsed = 11.70, Rank = 4, }, 

            }
        };
        public static ItemRank ManajumasGoryFetch = new ItemRank
        {
            Item = Legendary.ManajumasGoryFetch,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 18, PercentUsed = 1.81, Rank = 5, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 2, PercentUsed = 0.20, Rank = 9, }, 

            }
        };
        public static ItemRank Spite = new ItemRank
        {
            Item = Legendary.Spite,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 7, PercentUsed = 0.70, Rank = 7, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 7, PercentUsed = 0.70, Rank = 7, }, 

            }
        };
        public static ItemRank ShukranisTriumph = new ItemRank
        {
            Item = Legendary.ShukranisTriumph,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 5, PercentUsed = 0.50, Rank = 8, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 55, PercentUsed = 5.50, Rank = 5, }, 

            }
        };
        public static ItemRank GazingDemise = new ItemRank
        {
            Item = Legendary.GazingDemise,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 4, PercentUsed = 0.40, Rank = 9, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 3, PercentUsed = 0.30, Rank = 8, }, 

            }
        };

        public static ItemRank StarmetalKukri = new ItemRank
        {
            Item = Legendary.StarmetalKukri,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 281, PercentUsed = 28.18, Rank = 1, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 427, PercentUsed = 42.70, Rank = 1, }, 

            }
        };
        public static ItemRank RhenhoFlayer = new ItemRank
        {
            Item = Legendary.RhenhoFlayer,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 211, PercentUsed = 21.16, Rank = 2, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 62, PercentUsed = 6.20, Rank = 5, }, 

            }
        };
        public static ItemRank TheDaggerOfDarts = new ItemRank
        {
            Item = Legendary.TheDaggerOfDarts,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 60, PercentUsed = 6.02, Rank = 5, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 91, PercentUsed = 9.10, Rank = 4, }, 

            }
        };
        public static ItemRank ManajumasCarvingKnife = new ItemRank
        {
            Item = Legendary.ManajumasCarvingKnife,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 27, PercentUsed = 2.71, Rank = 7, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 6, PercentUsed = 0.60, Rank = 10, }, 

            }
        };
        public static ItemRank LastBreath = new ItemRank
        {
            Item = Legendary.LastBreath,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 15, PercentUsed = 1.50, Rank = 10, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 11, PercentUsed = 1.10, Rank = 7, }, 

            }
        };
        public static ItemRank VisageOfGiyua = new ItemRank
        {
            Item = Legendary.VisageOfGiyua,

            HardcoreRank = new List<ItemRankData>
            {

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 3, PercentUsed = 0.30, Rank = 9, }, 

            }
        };
        public static ItemRank BurdenOfTheInvoker = new ItemRank
        {
            Item = Legendary.BurdenOfTheInvoker,

            HardcoreRank = new List<ItemRankData>
            {

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 2, PercentUsed = 0.20, Rank = 9, }, 

            }
        };
        public static ItemRank VyrsAstonishingAura = new ItemRank
        {
            Item = Legendary.VyrsAstonishingAura,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 11, PercentUsed = 1.10, Rank = 7, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 1, PercentUsed = 0.10, Rank = 10, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 1, PercentUsed = 0.10, Rank = 9, }, 

            }
        };
        public static ItemRank PrideOfTheInvoker = new ItemRank
        {
            Item = Legendary.PrideOfTheInvoker,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 5, PercentUsed = 0.50, Rank = 9, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 3, PercentUsed = 0.30, Rank = 9, }, 

            }
        };
        public static ItemRank VyrsGraspingGauntlets = new ItemRank
        {
            Item = Legendary.VyrsGraspingGauntlets,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 9, PercentUsed = 0.90, Rank = 8, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 3, PercentUsed = 0.30, Rank = 10, }, 
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 5, PercentUsed = 0.50, Rank = 8, }, 

            }
        };
        public static ItemRank RechelsRingOfLarceny = new ItemRank
        {
            Item = Legendary.RechelsRingOfLarceny,

            HardcoreRank = new List<ItemRankData>
            {

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 81, PercentUsed = 8.10, Rank = 5, }, 

            }
        };
        public static ItemRank IvoryTower = new ItemRank
        {
            Item = Legendary.IvoryTower,

            HardcoreRank = new List<ItemRankData>
            {

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 1, PercentUsed = 0.10, Rank = 10, }, 

            }
        };
        public static ItemRank WrathOfTheBoneKing = new ItemRank
        {
            Item = Legendary.WrathOfTheBoneKing,

            HardcoreRank = new List<ItemRankData>
            {

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 15, PercentUsed = 1.50, Rank = 6, }, 

            }
        };
        public static ItemRank FirebirdsPlume = new ItemRank
        {
            Item = Legendary.FirebirdsPlume,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 690, PercentUsed = 69.00, Rank = 1, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 877, PercentUsed = 87.70, Rank = 1, }, 

            }
        };
        public static ItemRank TalRashasGuiseOfWisdom = new ItemRank
        {
            Item = Legendary.TalRashasGuiseOfWisdom,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 89, PercentUsed = 8.90, Rank = 2, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 20, PercentUsed = 2.00, Rank = 4, }, 

            }
        };
        public static ItemRank DarkMagesShade = new ItemRank
        {
            Item = Legendary.DarkMagesShade,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 12, PercentUsed = 1.20, Rank = 7, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 2, PercentUsed = 0.20, Rank = 10, }, 

            }
        };
        public static ItemRank TheMagistrate = new ItemRank
        {
            Item = Legendary.TheMagistrate,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 5, PercentUsed = 0.50, Rank = 10, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 4, PercentUsed = 0.40, Rank = 7, }, 

            }
        };
        public static ItemRank FirebirdsPinions = new ItemRank
        {
            Item = Legendary.FirebirdsPinions,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 692, PercentUsed = 69.20, Rank = 1, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 928, PercentUsed = 92.80, Rank = 1, }, 

            }
        };
        public static ItemRank FirebirdsBreast = new ItemRank
        {
            Item = Legendary.FirebirdsBreast,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 318, PercentUsed = 31.80, Rank = 2, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 350, PercentUsed = 35.00, Rank = 2, }, 

            }
        };
        public static ItemRank FirebirdsTalons = new ItemRank
        {
            Item = Legendary.FirebirdsTalons,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 435, PercentUsed = 43.50, Rank = 1, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 643, PercentUsed = 64.30, Rank = 1, }, 

            }
        };
        public static ItemRank TalRashasBrace = new ItemRank
        {
            Item = Legendary.TalRashasBrace,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 173, PercentUsed = 17.30, Rank = 3, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 35, PercentUsed = 3.50, Rank = 5, }, 

            }
        };
        public static ItemRank JangsEnvelopment = new ItemRank
        {
            Item = Legendary.JangsEnvelopment,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 19, PercentUsed = 1.90, Rank = 9, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {

            }
        };
        public static ItemRank FirebirdsDown = new ItemRank
        {
            Item = Legendary.FirebirdsDown,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 752, PercentUsed = 75.20, Rank = 1, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 911, PercentUsed = 91.10, Rank = 1, }, 

            }
        };
        public static ItemRank VyrsFantasticFinery = new ItemRank
        {
            Item = Legendary.VyrsFantasticFinery,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 17, PercentUsed = 1.70, Rank = 7, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 2, PercentUsed = 0.20, Rank = 10, }, 

            }
        };
        public static ItemRank FirebirdsTarsi = new ItemRank
        {
            Item = Legendary.FirebirdsTarsi,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 740, PercentUsed = 74.00, Rank = 1, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 916, PercentUsed = 91.60, Rank = 1, }, 

            }
        };
        public static ItemRank VyrsSwaggeringStance = new ItemRank
        {
            Item = Legendary.VyrsSwaggeringStance,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 12, PercentUsed = 1.20, Rank = 8, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 5, PercentUsed = 0.50, Rank = 9, }, 

            }
        };
        public static ItemRank TalRashasAllegiance = new ItemRank
        {
            Item = Legendary.TalRashasAllegiance,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 164, PercentUsed = 16.40, Rank = 1, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 63, PercentUsed = 6.30, Rank = 7, }, 

            }
        };
        public static ItemRank FirebirdsEye = new ItemRank
        {
            Item = Legendary.FirebirdsEye,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 545, PercentUsed = 54.50, Rank = 1, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 621, PercentUsed = 62.10, Rank = 1, }, 

            }
        };
        public static ItemRank Mirrorball = new ItemRank
        {
            Item = Legendary.Mirrorball,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 107, PercentUsed = 10.70, Rank = 2, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 13, PercentUsed = 1.30, Rank = 2, }, 

            }
        };
        public static ItemRank TalRashasUnwaveringGlare = new ItemRank
        {
            Item = Legendary.TalRashasUnwaveringGlare,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 45, PercentUsed = 4.50, Rank = 3, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 13, PercentUsed = 1.30, Rank = 3, }, 

            }
        };
        public static ItemRank Triumvirate = new ItemRank
        {
            Item = Legendary.Triumvirate,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 36, PercentUsed = 3.60, Rank = 4, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 8, PercentUsed = 0.80, Rank = 5, }, 

            }
        };
        public static ItemRank ChantodosForce = new ItemRank
        {
            Item = Legendary.ChantodosForce,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 12, PercentUsed = 1.20, Rank = 5, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 4, PercentUsed = 0.40, Rank = 7, }, 

            }
        };
        public static ItemRank LightOfGrace = new ItemRank
        {
            Item = Legendary.LightOfGrace,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 12, PercentUsed = 1.20, Rank = 6, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 7, PercentUsed = 0.70, Rank = 6, }, 

            }
        };
        public static ItemRank MykensBallOfHate = new ItemRank
        {
            Item = Legendary.MykensBallOfHate,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 9, PercentUsed = 0.90, Rank = 7, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 10, PercentUsed = 1.00, Rank = 4, }, 

            }
        };
        public static ItemRank WinterFlurry = new ItemRank
        {
            Item = Legendary.WinterFlurry,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 4, PercentUsed = 0.40, Rank = 8, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 2, PercentUsed = 0.20, Rank = 9, }, 

            }
        };
        public static ItemRank TheOculus = new ItemRank
        {
            Item = Legendary.TheOculus,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 3, PercentUsed = 0.30, Rank = 9, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {

            }
        };
        public static ItemRank SerpentsSparker = new ItemRank
        {
            Item = Legendary.SerpentsSparker,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 198, PercentUsed = 19.80, Rank = 1, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 115, PercentUsed = 11.50, Rank = 4, }, 

            }
        };
        public static ItemRank WandOfWoh = new ItemRank
        {
            Item = Legendary.WandOfWoh,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 21, PercentUsed = 2.10, Rank = 8, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 36, PercentUsed = 3.60, Rank = 5, }, 

            }
        };
        public static ItemRank SloraksMadness = new ItemRank
        {
            Item = Legendary.SloraksMadness,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 20, PercentUsed = 2.00, Rank = 9, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 5, PercentUsed = 0.50, Rank = 10, }, 

            }
        };
        public static ItemRank VelvetCamaral = new ItemRank
        {
            Item = Legendary.VelvetCamaral,

            HardcoreRank = new List<ItemRankData>
            {

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 4, PercentUsed = 0.40, Rank = 6, }, 

            }
        };
        public static ItemRank StormCrow = new ItemRank
        {
            Item = Legendary.StormCrow,

            HardcoreRank = new List<ItemRankData>
            {

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 2, PercentUsed = 0.20, Rank = 9, }, 

            }
        };
        public static ItemRank IrontoeMudsputters = new ItemRank
        {
            Item = Legendary.IrontoeMudsputters,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 1, PercentUsed = 0.10, Rank = 10, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 2, PercentUsed = 0.20, Rank = 10, }, 

            }
        };
        public static ItemRank Restraint = new ItemRank
        {
            Item = Legendary.Restraint,

            HardcoreRank = new List<ItemRankData>
            {

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 9, PercentUsed = 0.90, Rank = 7, }, 

            }
        };
        public static ItemRank CosmicStrand = new ItemRank
        {
            Item = Legendary.CosmicStrand,

            HardcoreRank = new List<ItemRankData>
            {

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Wizard, SampleSize = 2, PercentUsed = 0.20, Rank = 8, }, 

            }
        };
        public static ItemRank HelmOfAkkhan = new ItemRank
        {
            Item = Legendary.HelmOfAkkhan,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 426, PercentUsed = 42.86, Rank = 1, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 344, PercentUsed = 34.40, Rank = 2, }, 

            }
        };
        public static ItemRank RolandsVisage = new ItemRank
        {
            Item = Legendary.RolandsVisage,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 51, PercentUsed = 5.13, Rank = 3, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 25, PercentUsed = 2.50, Rank = 3, }, 

            }
        };
        public static ItemRank TheHelmOfRule = new ItemRank
        {
            Item = Legendary.TheHelmOfRule,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 9, PercentUsed = 0.91, Rank = 7, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 2, PercentUsed = 0.20, Rank = 8, }, 

            }
        };
        public static ItemRank PauldronsOfAkkhan = new ItemRank
        {
            Item = Legendary.PauldronsOfAkkhan,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 812, PercentUsed = 81.69, Rank = 1, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 807, PercentUsed = 80.70, Rank = 1, }, 

            }
        };
        public static ItemRank RolandsMantle = new ItemRank
        {
            Item = Legendary.RolandsMantle,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 66, PercentUsed = 6.64, Rank = 2, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 30, PercentUsed = 3.00, Rank = 4, }, 

            }
        };
        public static ItemRank BreastplateOfAkkhan = new ItemRank
        {
            Item = Legendary.BreastplateOfAkkhan,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 789, PercentUsed = 79.38, Rank = 1, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 793, PercentUsed = 79.30, Rank = 1, }, 

            }
        };
        public static ItemRank RolandsBearing = new ItemRank
        {
            Item = Legendary.RolandsBearing,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 48, PercentUsed = 4.83, Rank = 3, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 21, PercentUsed = 2.10, Rank = 4, }, 

            }
        };
        public static ItemRank ArmorOfTheKindRegent = new ItemRank
        {
            Item = Legendary.ArmorOfTheKindRegent,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 6, PercentUsed = 0.60, Rank = 7, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {

            }
        };
        public static ItemRank MantleOfTheRydraelm = new ItemRank
        {
            Item = Legendary.MantleOfTheRydraelm,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 2, PercentUsed = 0.20, Rank = 10, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {

            }
        };
        public static ItemRank GauntletsOfAkkhan = new ItemRank
        {
            Item = Legendary.GauntletsOfAkkhan,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 775, PercentUsed = 77.97, Rank = 1, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 794, PercentUsed = 79.40, Rank = 1, }, 

            }
        };
        public static ItemRank RolandsGrasp = new ItemRank
        {
            Item = Legendary.RolandsGrasp,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 64, PercentUsed = 6.44, Rank = 2, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 23, PercentUsed = 2.30, Rank = 4, }, 

            }
        };
        public static ItemRank AngelHairBraid = new ItemRank
        {
            Item = Legendary.AngelHairBraid,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 24, PercentUsed = 2.41, Rank = 7, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 9, PercentUsed = 0.90, Rank = 7, }, 

            }
        };
        public static ItemRank CuissesOfAkkhan = new ItemRank
        {
            Item = Legendary.CuissesOfAkkhan,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 697, PercentUsed = 70.12, Rank = 1, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 765, PercentUsed = 76.50, Rank = 1, }, 

            }
        };
        public static ItemRank RolandsDetermination = new ItemRank
        {
            Item = Legendary.RolandsDetermination,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 54, PercentUsed = 5.43, Rank = 3, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 26, PercentUsed = 2.60, Rank = 4, }, 

            }
        };
        public static ItemRank SabatonsOfAkkhan = new ItemRank
        {
            Item = Legendary.SabatonsOfAkkhan,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 693, PercentUsed = 69.72, Rank = 1, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 775, PercentUsed = 77.50, Rank = 1, }, 

            }
        };
        public static ItemRank RolandsStride = new ItemRank
        {
            Item = Legendary.RolandsStride,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 50, PercentUsed = 5.03, Rank = 3, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 29, PercentUsed = 2.90, Rank = 4, }, 

            }
        };
        public static ItemRank EternalUnion = new ItemRank
        {
            Item = Legendary.EternalUnion,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 74, PercentUsed = 7.44, Rank = 4, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 21, PercentUsed = 2.10, Rank = 8, }, 

            }
        };
        public static ItemRank UnrelentingPhalanx = new ItemRank
        {
            Item = Legendary.UnrelentingPhalanx,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 109, PercentUsed = 10.97, Rank = 3, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 205, PercentUsed = 20.50, Rank = 2, }, 

            }
        };
        public static ItemRank Hellskull = new ItemRank
        {
            Item = Legendary.Hellskull,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 245, PercentUsed = 24.65, Rank = 2, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 114, PercentUsed = 11.40, Rank = 4, }, 

            }
        };
        public static ItemRank PiroMarella = new ItemRank
        {
            Item = Legendary.PiroMarella,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 43, PercentUsed = 4.33, Rank = 4, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 11, PercentUsed = 1.10, Rank = 7, }, 

            }
        };
        public static ItemRank LidlessWall = new ItemRank
        {
            Item = Legendary.LidlessWall,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 37, PercentUsed = 3.72, Rank = 5, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 18, PercentUsed = 1.80, Rank = 6, }, 

            }
        };
        public static ItemRank DefenderOfWestmarch = new ItemRank
        {
            Item = Legendary.DefenderOfWestmarch,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 35, PercentUsed = 3.52, Rank = 6, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {

            }
        };
        public static ItemRank TheFinalWitness = new ItemRank
        {
            Item = Legendary.TheFinalWitness,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 31, PercentUsed = 3.12, Rank = 7, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 142, PercentUsed = 14.20, Rank = 3, }, 

            }
        };
        public static ItemRank Jekangbord = new ItemRank
        {
            Item = Legendary.Jekangbord,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 26, PercentUsed = 2.62, Rank = 8, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 5, PercentUsed = 0.50, Rank = 9, }, 

            }
        };
        public static ItemRank HallowedBarricade = new ItemRank
        {
            Item = Legendary.HallowedBarricade,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 24, PercentUsed = 2.41, Rank = 9, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 7, PercentUsed = 0.70, Rank = 8, }, 

            }
        };
        public static ItemRank EberliCharo = new ItemRank
        {
            Item = Legendary.EberliCharo,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 24, PercentUsed = 2.41, Rank = 10, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 4, PercentUsed = 0.40, Rank = 10, }, 

            }
        };
        public static ItemRank FateOfTheFell = new ItemRank
        {
            Item = Legendary.FateOfTheFell,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 265, PercentUsed = 26.66, Rank = 1, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 94, PercentUsed = 9.40, Rank = 4, }, 

            }
        };
        public static ItemRank BalefulRemnant = new ItemRank
        {
            Item = Legendary.BalefulRemnant,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 104, PercentUsed = 10.46, Rank = 4, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 16, PercentUsed = 1.60, Rank = 8, }, 

            }
        };
        public static ItemRank BladeOfProphecy = new ItemRank
        {
            Item = Legendary.BladeOfProphecy,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 39, PercentUsed = 3.92, Rank = 6, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 45, PercentUsed = 4.50, Rank = 5, }, 

            }
        };
        public static ItemRank GoldenFlense = new ItemRank
        {
            Item = Legendary.GoldenFlense,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 31, PercentUsed = 3.12, Rank = 7, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 16, PercentUsed = 1.60, Rank = 9, }, 

            }
        };
        public static ItemRank Darklight = new ItemRank
        {
            Item = Legendary.Darklight,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 18, PercentUsed = 1.81, Rank = 8, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 11, PercentUsed = 1.10, Rank = 10, }, 

            }
        };
        public static ItemRank Swiftmount = new ItemRank
        {
            Item = Legendary.Swiftmount,

            HardcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 12, PercentUsed = 1.21, Rank = 10, }, 

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 110, PercentUsed = 11.00, Rank = 3, }, 

            }
        };
        public static ItemRank CrownOfTheInvoker = new ItemRank
        {
            Item = Legendary.CrownOfTheInvoker,

            HardcoreRank = new List<ItemRankData>
            {

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 2, PercentUsed = 0.20, Rank = 7, }, 

            }
        };
        public static ItemRank StoneGauntlets = new ItemRank
        {
            Item = Legendary.StoneGauntlets,

            HardcoreRank = new List<ItemRankData>
            {

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 2, PercentUsed = 0.20, Rank = 10, }, 

            }
        };
        public static ItemRank SeborsNightmare = new ItemRank
        {
            Item = Legendary.SeborsNightmare,

            HardcoreRank = new List<ItemRankData>
            {

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 3, PercentUsed = 0.30, Rank = 10, }, 

            }
        };
        public static ItemRank BoardWalkers = new ItemRank
        {
            Item = Legendary.BoardWalkers,

            HardcoreRank = new List<ItemRankData>
            {

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 1, PercentUsed = 0.10, Rank = 10, }, 

            }
        };
        public static ItemRank TheEssOfJohan = new ItemRank
        {
            Item = Legendary.TheEssOfJohan,

            HardcoreRank = new List<ItemRankData>
            {

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 43, PercentUsed = 4.30, Rank = 9, }, 

            }
        };
        public static ItemRank JusticeLantern = new ItemRank
        {
            Item = Legendary.JusticeLantern,

            HardcoreRank = new List<ItemRankData>
            {

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 20, PercentUsed = 2.00, Rank = 9, }, 

            }
        };
        public static ItemRank FrydehrsWrath = new ItemRank
        {
            Item = Legendary.FrydehrsWrath,

            HardcoreRank = new List<ItemRankData>
            {

            },
            SoftcoreRank = new List<ItemRankData>
            {
            new ItemRankData { Class = ActorClass.Crusader, SampleSize = 31, PercentUsed = 3.10, Rank = 5, }, 

            }
        };



    }
}