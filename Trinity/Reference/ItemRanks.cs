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

                if (Trinity.Settings.Loot.ItemRank.AncientItemsOnly && !cItem.IsAncient)
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

        // AUTO-GENERATED on Wed, 17 Dec 2014 04:30:17 GMT

        public static ItemRank AughildsSearch = new ItemRank

        {
            Item = Legendary.AughildsSearch,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 568, PercentUsed = 57.20, Rank = 1,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 354, PercentUsed = 35.98, Rank = 1,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 83, PercentUsed = 8.30, Rank = 4,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 691, PercentUsed = 69.66, Rank = 1,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 271, PercentUsed = 27.13, Rank = 2,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 55, PercentUsed = 5.54, Rank = 3,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 786, PercentUsed = 78.60, Rank = 1,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 143, PercentUsed = 14.30, Rank = 3,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 21, PercentUsed = 2.10, Rank = 7,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 333, PercentUsed = 33.30, Rank = 2,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 53, PercentUsed = 5.30, Rank = 2,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 10, PercentUsed = 1.00, Rank = 7,},
            }
        };

        public static ItemRank ReapersWraps = new ItemRank

        {
            Item = Legendary.ReapersWraps,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 144, PercentUsed = 14.50, Rank = 2,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 130, PercentUsed = 13.21, Rank = 3,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 259, PercentUsed = 25.90, Rank = 2,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 21, PercentUsed = 2.12, Rank = 3,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 63, PercentUsed = 6.31, Rank = 4,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 489, PercentUsed = 49.24, Rank = 1,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 30, PercentUsed = 3.00, Rank = 3,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 11, PercentUsed = 1.10, Rank = 9,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 26, PercentUsed = 2.60, Rank = 6,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 5, PercentUsed = 0.50, Rank = 9,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 6, PercentUsed = 0.60, Rank = 9,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 99, PercentUsed = 9.90, Rank = 2,},
            }
        };

        public static ItemRank StrongarmBracers = new ItemRank

        {
            Item = Legendary.StrongarmBracers,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 126, PercentUsed = 12.69, Rank = 3,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 278, PercentUsed = 28.25, Rank = 2,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 41, PercentUsed = 4.10, Rank = 7,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 177, PercentUsed = 17.84, Rank = 2,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 475, PercentUsed = 47.55, Rank = 1,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 345, PercentUsed = 34.74, Rank = 2,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 66, PercentUsed = 6.60, Rank = 2,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 283, PercentUsed = 28.30, Rank = 2,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 18, PercentUsed = 1.80, Rank = 8,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 498, PercentUsed = 49.80, Rank = 1,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 757, PercentUsed = 75.70, Rank = 1,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 762, PercentUsed = 76.20, Rank = 1,},
            }
        };

        public static ItemRank SlaveBonds = new ItemRank

        {
            Item = Legendary.SlaveBonds,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 19, PercentUsed = 1.91, Rank = 4,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 37, PercentUsed = 3.76, Rank = 5,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 273, PercentUsed = 27.30, Rank = 1,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 12, PercentUsed = 1.21, Rank = 7,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 64, PercentUsed = 6.41, Rank = 3,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 22, PercentUsed = 2.22, Rank = 4,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 5, PercentUsed = 0.50, Rank = 9,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 8, PercentUsed = 0.80, Rank = 10,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 76, PercentUsed = 7.60, Rank = 5,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 6, PercentUsed = 0.60, Rank = 8,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 15, PercentUsed = 1.50, Rank = 8,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 5, PercentUsed = 0.50, Rank = 8,},
            }
        };

        public static ItemRank WarzechianArmguards = new ItemRank

        {
            Item = Legendary.WarzechianArmguards,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 15, PercentUsed = 1.51, Rank = 5,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 12, PercentUsed = 1.22, Rank = 10,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 6, PercentUsed = 0.60, Rank = 10,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 20, PercentUsed = 2.00, Rank = 6,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 19, PercentUsed = 1.90, Rank = 6,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 13, PercentUsed = 1.30, Rank = 7,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 13, PercentUsed = 1.30, Rank = 9,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 15, PercentUsed = 1.50, Rank = 6,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 42, PercentUsed = 4.20, Rank = 3,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 5, PercentUsed = 0.50, Rank = 9,},
            }
        };

        public static ItemRank LacuniProwlers = new ItemRank

        {
            Item = Legendary.LacuniProwlers,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 15, PercentUsed = 1.51, Rank = 6,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 13, PercentUsed = 1.32, Rank = 9,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 104, PercentUsed = 10.40, Rank = 3,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 12, PercentUsed = 1.21, Rank = 6,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 15, PercentUsed = 1.50, Rank = 7,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 14, PercentUsed = 1.41, Rank = 5,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 17, PercentUsed = 1.70, Rank = 7,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 18, PercentUsed = 1.80, Rank = 5,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 201, PercentUsed = 20.10, Rank = 2,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 48, PercentUsed = 4.80, Rank = 3,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 25, PercentUsed = 2.50, Rank = 5,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 12, PercentUsed = 1.20, Rank = 6,},
            }
        };

        public static ItemRank SteadyStrikers = new ItemRank

        {
            Item = Legendary.SteadyStrikers,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 14, PercentUsed = 1.41, Rank = 7,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 65, PercentUsed = 6.50, Rank = 5,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 20, PercentUsed = 2.02, Rank = 4,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 10, PercentUsed = 1.00, Rank = 9,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 23, PercentUsed = 2.30, Rank = 4,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 12, PercentUsed = 1.20, Rank = 8,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 478, PercentUsed = 47.80, Rank = 1,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 31, PercentUsed = 3.10, Rank = 4,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 38, PercentUsed = 3.80, Rank = 4,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 17, PercentUsed = 1.70, Rank = 5,},
            }
        };

        public static ItemRank AncientParthanDefenders = new ItemRank

        {
            Item = Legendary.AncientParthanDefenders,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 14, PercentUsed = 1.41, Rank = 8,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 51, PercentUsed = 5.18, Rank = 4,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 10, PercentUsed = 1.00, Rank = 10,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 10, PercentUsed = 1.01, Rank = 8,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 4, PercentUsed = 0.40, Rank = 10,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 435, PercentUsed = 43.50, Rank = 1,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 5, PercentUsed = 0.50, Rank = 10,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 3, PercentUsed = 0.30, Rank = 10,},
            }
        };

        public static ItemRank NemesisBracers = new ItemRank

        {
            Item = Legendary.NemesisBracers,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 14, PercentUsed = 1.41, Rank = 9,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 17, PercentUsed = 1.73, Rank = 7,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 32, PercentUsed = 3.20, Rank = 8,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 9, PercentUsed = 0.91, Rank = 8,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 23, PercentUsed = 2.30, Rank = 5,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 12, PercentUsed = 1.21, Rank = 7,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 10, PercentUsed = 1.00, Rank = 8,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 30, PercentUsed = 3.00, Rank = 4,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 83, PercentUsed = 8.30, Rank = 3,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 7, PercentUsed = 0.70, Rank = 7,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 24, PercentUsed = 2.40, Rank = 6,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 34, PercentUsed = 3.40, Rank = 3,},
            }
        };

        public static ItemRank GungdoGear = new ItemRank

        {
            Item = Legendary.GungdoGear,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 11, PercentUsed = 1.11, Rank = 10,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 15, PercentUsed = 1.50, Rank = 10,},
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
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 404, PercentUsed = 40.68, Rank = 1,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 493, PercentUsed = 49.30, Rank = 1,},
            }
        };

        public static ItemRank LeoricsCrown = new ItemRank

        {
            Item = Legendary.LeoricsCrown,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 115, PercentUsed = 11.58, Rank = 2,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 83, PercentUsed = 8.43, Rank = 4,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 15, PercentUsed = 1.50, Rank = 4,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 27, PercentUsed = 2.70, Rank = 5,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 369, PercentUsed = 37.16, Rank = 2,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 142, PercentUsed = 14.20, Rank = 3,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 609, PercentUsed = 60.90, Rank = 1,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 3, PercentUsed = 0.30, Rank = 5,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 5, PercentUsed = 0.50, Rank = 6,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 51, PercentUsed = 5.10, Rank = 2,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 522, PercentUsed = 52.20, Rank = 1,},
            }
        };

        public static ItemRank MaskOfTheSearingSky = new ItemRank

        {
            Item = Legendary.MaskOfTheSearingSky,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 88, PercentUsed = 8.86, Rank = 3,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 44, PercentUsed = 4.40, Rank = 4,},
            }
        };

        public static ItemRank EyeOfPeshkov = new ItemRank

        {
            Item = Legendary.EyeOfPeshkov,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 81, PercentUsed = 8.16, Rank = 4,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 181, PercentUsed = 18.10, Rank = 2,},
            }
        };

        public static ItemRank TheEyeOfTheStorm = new ItemRank

        {
            Item = Legendary.TheEyeOfTheStorm,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 66, PercentUsed = 6.65, Rank = 5,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 43, PercentUsed = 4.30, Rank = 5,},
            }
        };

        public static ItemRank InnasRadiance = new ItemRank

        {
            Item = Legendary.InnasRadiance,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 59, PercentUsed = 5.94, Rank = 6,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 11, PercentUsed = 1.10, Rank = 9,},
            }
        };

        public static ItemRank AughildsSpike = new ItemRank

        {
            Item = Legendary.AughildsSpike,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 34, PercentUsed = 3.42, Rank = 7,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 37, PercentUsed = 3.76, Rank = 7,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 15, PercentUsed = 1.50, Rank = 5,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 47, PercentUsed = 4.70, Rank = 3,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 23, PercentUsed = 2.32, Rank = 5,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 6, PercentUsed = 0.60, Rank = 7,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 2, PercentUsed = 0.20, Rank = 9,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 14, PercentUsed = 1.40, Rank = 5,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 2, PercentUsed = 0.20, Rank = 5,},
            }
        };

        public static ItemRank TzoKrinsGaze = new ItemRank

        {
            Item = Legendary.TzoKrinsGaze,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 22, PercentUsed = 2.22, Rank = 8,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 16, PercentUsed = 1.60, Rank = 7,},
            }
        };

        public static ItemRank TheLawsOfSeph = new ItemRank

        {
            Item = Legendary.TheLawsOfSeph,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 18, PercentUsed = 1.81, Rank = 9,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 14, PercentUsed = 1.40, Rank = 8,},
            }
        };

        public static ItemRank CainsInsight = new ItemRank

        {
            Item = Legendary.CainsInsight,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 17, PercentUsed = 1.71, Rank = 10,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 12, PercentUsed = 1.22, Rank = 8,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 1, PercentUsed = 0.10, Rank = 9,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 9, PercentUsed = 0.90, Rank = 6,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 11, PercentUsed = 1.11, Rank = 6,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 1, PercentUsed = 0.10, Rank = 7,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 2, PercentUsed = 0.20, Rank = 10,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 1, PercentUsed = 0.10, Rank = 7,},
            }
        };

        public static ItemRank SunwukosBalance = new ItemRank

        {
            Item = Legendary.SunwukosBalance,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 553, PercentUsed = 55.69, Rank = 1,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 821, PercentUsed = 82.10, Rank = 1,},
            }
        };

        public static ItemRank AughildsPower = new ItemRank

        {
            Item = Legendary.AughildsPower,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 151, PercentUsed = 15.21, Rank = 2,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 318, PercentUsed = 32.32, Rank = 1,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 67, PercentUsed = 6.70, Rank = 2,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 703, PercentUsed = 70.87, Rank = 1,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 218, PercentUsed = 21.82, Rank = 2,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 42, PercentUsed = 4.23, Rank = 3,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 51, PercentUsed = 5.10, Rank = 3,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 131, PercentUsed = 13.10, Rank = 3,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 22, PercentUsed = 2.20, Rank = 2,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 368, PercentUsed = 36.80, Rank = 2,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 56, PercentUsed = 5.60, Rank = 2,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 9, PercentUsed = 0.90, Rank = 5,},
            }
        };

        public static ItemRank MantleOfTheUpsidedownSinners = new ItemRank

        {
            Item = Legendary.MantleOfTheUpsidedownSinners,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 134, PercentUsed = 13.49, Rank = 3,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 93, PercentUsed = 9.30, Rank = 2,},
            }
        };

        public static ItemRank AshearasCustodian = new ItemRank

        {
            Item = Legendary.AshearasCustodian,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 53, PercentUsed = 5.34, Rank = 4,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 41, PercentUsed = 4.17, Rank = 5,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 4, PercentUsed = 0.40, Rank = 6,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 28, PercentUsed = 2.82, Rank = 3,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 18, PercentUsed = 1.80, Rank = 3,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 23, PercentUsed = 2.32, Rank = 4,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 5, PercentUsed = 0.50, Rank = 5,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 42, PercentUsed = 4.20, Rank = 4,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 3, PercentUsed = 0.30, Rank = 4,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 50, PercentUsed = 5.00, Rank = 3,},
            }
        };

        public static ItemRank BornsPrivilege = new ItemRank

        {
            Item = Legendary.BornsPrivilege,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 43, PercentUsed = 4.33, Rank = 5,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 7, PercentUsed = 0.71, Rank = 8,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 18, PercentUsed = 1.80, Rank = 4,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 8, PercentUsed = 0.81, Rank = 5,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 16, PercentUsed = 1.60, Rank = 4,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 5, PercentUsed = 0.50, Rank = 5,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 62, PercentUsed = 6.20, Rank = 3,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 11, PercentUsed = 1.10, Rank = 3,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 78, PercentUsed = 7.80, Rank = 2,},
            }
        };

        public static ItemRank HomingPads = new ItemRank

        {
            Item = Legendary.HomingPads,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 8, PercentUsed = 0.81, Rank = 6,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 10, PercentUsed = 1.02, Rank = 9,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 6, PercentUsed = 0.60, Rank = 4,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 10, PercentUsed = 1.01, Rank = 6,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 4, PercentUsed = 0.40, Rank = 7,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 7, PercentUsed = 0.70, Rank = 7,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 2, PercentUsed = 0.20, Rank = 6,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 4, PercentUsed = 0.40, Rank = 3,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 2, PercentUsed = 0.20, Rank = 9,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 3, PercentUsed = 0.30, Rank = 5,},
            }
        };

        public static ItemRank SpauldersOfZakara = new ItemRank

        {
            Item = Legendary.SpauldersOfZakara,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 7, PercentUsed = 0.70, Rank = 7,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 13, PercentUsed = 1.32, Rank = 7,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 4, PercentUsed = 0.40, Rank = 5,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 6, PercentUsed = 0.60, Rank = 9,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 7, PercentUsed = 0.70, Rank = 6,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 4, PercentUsed = 0.40, Rank = 7,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 2, PercentUsed = 0.20, Rank = 4,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 4, PercentUsed = 0.40, Rank = 8,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 1, PercentUsed = 0.10, Rank = 8,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 1, PercentUsed = 0.10, Rank = 8,},
            }
        };

        public static ItemRank PauldronsOfTheSkeletonKing = new ItemRank

        {
            Item = Legendary.PauldronsOfTheSkeletonKing,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 5, PercentUsed = 0.50, Rank = 8,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 7, PercentUsed = 0.70, Rank = 3,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 12, PercentUsed = 1.21, Rank = 5,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 11, PercentUsed = 1.10, Rank = 5,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 4, PercentUsed = 0.40, Rank = 8,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 1, PercentUsed = 0.10, Rank = 7,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 2, PercentUsed = 0.20, Rank = 10,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 29, PercentUsed = 2.90, Rank = 5,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 3, PercentUsed = 0.30, Rank = 6,},
            }
        };

        public static ItemRank DeathWatchMantle = new ItemRank

        {
            Item = Legendary.DeathWatchMantle,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 4, PercentUsed = 0.40, Rank = 9,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 10, PercentUsed = 1.02, Rank = 8,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 4, PercentUsed = 0.40, Rank = 7,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 7, PercentUsed = 0.70, Rank = 6,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 2, PercentUsed = 0.20, Rank = 9,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 5, PercentUsed = 0.50, Rank = 6,},
            }
        };

        public static ItemRank Corruption = new ItemRank

        {
            Item = Legendary.Corruption,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 3, PercentUsed = 0.30, Rank = 10,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 6, PercentUsed = 0.61, Rank = 10,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 3, PercentUsed = 0.30, Rank = 10,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 3, PercentUsed = 0.30, Rank = 8,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 1, PercentUsed = 0.10, Rank = 5,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 1, PercentUsed = 0.10, Rank = 10,},
            }
        };

        public static ItemRank AughildsRule = new ItemRank

        {
            Item = Legendary.AughildsRule,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 415, PercentUsed = 41.79, Rank = 1,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 55, PercentUsed = 5.59, Rank = 5,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 5, PercentUsed = 0.50, Rank = 7,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 226, PercentUsed = 22.78, Rank = 2,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 27, PercentUsed = 2.70, Rank = 5,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 19, PercentUsed = 1.91, Rank = 5,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 744, PercentUsed = 74.40, Rank = 1,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 22, PercentUsed = 2.20, Rank = 4,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 216, PercentUsed = 21.60, Rank = 2,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 5, PercentUsed = 0.50, Rank = 6,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 5, PercentUsed = 0.50, Rank = 5,},
            }
        };

        public static ItemRank InnasVastExpanse = new ItemRank

        {
            Item = Legendary.InnasVastExpanse,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 190, PercentUsed = 19.13, Rank = 2,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 41, PercentUsed = 4.10, Rank = 3,},
            }
        };

        public static ItemRank HeartOfTheCrashingWave = new ItemRank

        {
            Item = Legendary.HeartOfTheCrashingWave,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 132, PercentUsed = 13.29, Rank = 3,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 87, PercentUsed = 8.70, Rank = 2,},
            }
        };

        public static ItemRank BlackthornesSurcoat = new ItemRank

        {
            Item = Legendary.BlackthornesSurcoat,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 117, PercentUsed = 11.78, Rank = 4,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 82, PercentUsed = 8.33, Rank = 4,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 8, PercentUsed = 0.80, Rank = 6,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 122, PercentUsed = 12.30, Rank = 4,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 30, PercentUsed = 3.00, Rank = 4,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 42, PercentUsed = 4.23, Rank = 4,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 22, PercentUsed = 2.20, Rank = 7,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 21, PercentUsed = 2.10, Rank = 5,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 65, PercentUsed = 6.50, Rank = 5,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 2, PercentUsed = 0.20, Rank = 9,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 4, PercentUsed = 0.40, Rank = 6,},
            }
        };

        public static ItemRank BornsFrozenSoul = new ItemRank

        {
            Item = Legendary.BornsFrozenSoul,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 63, PercentUsed = 6.34, Rank = 5,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 16, PercentUsed = 1.63, Rank = 6,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 5, PercentUsed = 0.50, Rank = 9,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 8, PercentUsed = 0.80, Rank = 7,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 11, PercentUsed = 1.11, Rank = 6,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 23, PercentUsed = 2.30, Rank = 5,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 16, PercentUsed = 1.60, Rank = 6,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 57, PercentUsed = 5.70, Rank = 6,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 5, PercentUsed = 0.50, Rank = 7,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 127, PercentUsed = 12.70, Rank = 2,},
            }
        };

        public static ItemRank Cindercoat = new ItemRank

        {
            Item = Legendary.Cindercoat,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 29, PercentUsed = 2.92, Rank = 6,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 121, PercentUsed = 12.30, Rank = 3,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 91, PercentUsed = 9.10, Rank = 2,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 28, PercentUsed = 2.82, Rank = 5,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 402, PercentUsed = 40.24, Rank = 1,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 66, PercentUsed = 6.65, Rank = 2,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 33, PercentUsed = 3.30, Rank = 4,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 76, PercentUsed = 7.60, Rank = 3,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 74, PercentUsed = 7.40, Rank = 2,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 22, PercentUsed = 2.20, Rank = 7,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 414, PercentUsed = 41.40, Rank = 2,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 19, PercentUsed = 1.90, Rank = 3,},
            }
        };

        public static ItemRank TyraelsMight = new ItemRank

        {
            Item = Legendary.TyraelsMight,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 10, PercentUsed = 1.01, Rank = 7,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 7, PercentUsed = 0.71, Rank = 8,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 4, PercentUsed = 0.40, Rank = 10,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 1, PercentUsed = 0.10, Rank = 10,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 6, PercentUsed = 0.60, Rank = 9,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 4, PercentUsed = 0.40, Rank = 7,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 1, PercentUsed = 0.10, Rank = 8,},
            }
        };

        public static ItemRank Goldskin = new ItemRank

        {
            Item = Legendary.Goldskin,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 3, PercentUsed = 0.30, Rank = 8,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 6, PercentUsed = 0.61, Rank = 9,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 17, PercentUsed = 1.71, Rank = 7,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 7, PercentUsed = 0.70, Rank = 9,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 2, PercentUsed = 0.20, Rank = 8,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 23, PercentUsed = 2.30, Rank = 6,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 2, PercentUsed = 0.20, Rank = 8,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 5, PercentUsed = 0.50, Rank = 4,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 7, PercentUsed = 0.70, Rank = 8,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 9, PercentUsed = 0.90, Rank = 5,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 3, PercentUsed = 0.30, Rank = 7,},
            }
        };

        public static ItemRank Chaingmail = new ItemRank

        {
            Item = Legendary.Chaingmail,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 3, PercentUsed = 0.30, Rank = 10,},
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
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 582, PercentUsed = 58.61, Rank = 1,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 821, PercentUsed = 82.10, Rank = 1,},
            }
        };

        public static ItemRank FistsOfThunder = new ItemRank

        {
            Item = Legendary.FistsOfThunder,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 147, PercentUsed = 14.80, Rank = 2,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 96, PercentUsed = 9.60, Rank = 2,},
            }
        };

        public static ItemRank AshearasWard = new ItemRank

        {
            Item = Legendary.AshearasWard,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 74, PercentUsed = 7.45, Rank = 3,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 23, PercentUsed = 2.34, Rank = 6,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 3, PercentUsed = 0.30, Rank = 6,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 17, PercentUsed = 1.71, Rank = 5,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 19, PercentUsed = 1.90, Rank = 5,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 24, PercentUsed = 2.42, Rank = 4,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 8, PercentUsed = 0.80, Rank = 5,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 31, PercentUsed = 3.10, Rank = 3,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 5, PercentUsed = 0.50, Rank = 8,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 70, PercentUsed = 7.00, Rank = 2,},
            }
        };

        public static ItemRank Magefist = new ItemRank

        {
            Item = Legendary.Magefist,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 35, PercentUsed = 3.52, Rank = 4,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 81, PercentUsed = 8.23, Rank = 4,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 118, PercentUsed = 11.80, Rank = 3,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 56, PercentUsed = 5.65, Rank = 3,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 400, PercentUsed = 40.04, Rank = 2,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 40, PercentUsed = 4.03, Rank = 3,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 32, PercentUsed = 3.20, Rank = 3,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 20, PercentUsed = 2.00, Rank = 5,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 71, PercentUsed = 7.10, Rank = 2,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 12, PercentUsed = 1.20, Rank = 6,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 139, PercentUsed = 13.90, Rank = 2,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 15, PercentUsed = 1.50, Rank = 5,},
            }
        };

        public static ItemRank CainsScrivener = new ItemRank

        {
            Item = Legendary.CainsScrivener,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 34, PercentUsed = 3.42, Rank = 5,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 6, PercentUsed = 0.61, Rank = 9,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 37, PercentUsed = 3.73, Rank = 4,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 9, PercentUsed = 0.90, Rank = 10,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 10, PercentUsed = 1.01, Rank = 6,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 6, PercentUsed = 0.60, Rank = 6,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 3, PercentUsed = 0.30, Rank = 8,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 1, PercentUsed = 0.10, Rank = 6,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 16, PercentUsed = 1.60, Rank = 5,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 3, PercentUsed = 0.30, Rank = 9,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 2, PercentUsed = 0.20, Rank = 8,},
            }
        };

        public static ItemRank Frostburn = new ItemRank

        {
            Item = Legendary.Frostburn,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 23, PercentUsed = 2.32, Rank = 6,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 1, PercentUsed = 0.10, Rank = 8,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 10, PercentUsed = 1.00, Rank = 9,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 16, PercentUsed = 1.60, Rank = 4,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 3, PercentUsed = 0.30, Rank = 7,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 2, PercentUsed = 0.20, Rank = 5,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 5, PercentUsed = 0.50, Rank = 8,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 16, PercentUsed = 1.60, Rank = 6,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 1, PercentUsed = 0.10, Rank = 9,},
            }
        };

        public static ItemRank StArchewsGage = new ItemRank

        {
            Item = Legendary.StArchewsGage,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 20, PercentUsed = 2.01, Rank = 7,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 4, PercentUsed = 0.41, Rank = 10,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 1, PercentUsed = 0.10, Rank = 9,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 11, PercentUsed = 1.11, Rank = 7,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 1, PercentUsed = 0.10, Rank = 10,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 1, PercentUsed = 0.10, Rank = 7,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 25, PercentUsed = 2.50, Rank = 4,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 56, PercentUsed = 5.60, Rank = 3,},
            }
        };

        public static ItemRank GladiatorGauntlets = new ItemRank

        {
            Item = Legendary.GladiatorGauntlets,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 18, PercentUsed = 1.81, Rank = 8,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 9, PercentUsed = 0.91, Rank = 8,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 4, PercentUsed = 0.40, Rank = 5,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 15, PercentUsed = 1.51, Rank = 6,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 10, PercentUsed = 1.00, Rank = 8,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 9, PercentUsed = 0.91, Rank = 7,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 2, PercentUsed = 0.20, Rank = 8,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 1, PercentUsed = 0.10, Rank = 9,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 3, PercentUsed = 0.30, Rank = 9,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 2, PercentUsed = 0.20, Rank = 10,},
            }
        };

        public static ItemRank PendersPurchase = new ItemRank

        {
            Item = Legendary.PendersPurchase,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 11, PercentUsed = 1.11, Rank = 9,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 1, PercentUsed = 0.10, Rank = 10,},
            }
        };

        public static ItemRank GlovesOfWorship = new ItemRank

        {
            Item = Legendary.GlovesOfWorship,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 6, PercentUsed = 0.60, Rank = 10,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 4, PercentUsed = 0.40, Rank = 9,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 13, PercentUsed = 1.30, Rank = 6,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 9, PercentUsed = 0.91, Rank = 8,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 5, PercentUsed = 0.50, Rank = 7,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 4, PercentUsed = 0.40, Rank = 6,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 1, PercentUsed = 0.10, Rank = 10,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 21, PercentUsed = 2.10, Rank = 4,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 11, PercentUsed = 1.10, Rank = 6,},
            }
        };

        public static ItemRank BlackthornesNotchedBelt = new ItemRank

        {
            Item = Legendary.BlackthornesNotchedBelt,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 231, PercentUsed = 23.26, Rank = 1,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 43, PercentUsed = 4.37, Rank = 5,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 137, PercentUsed = 13.70, Rank = 2,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 307, PercentUsed = 30.95, Rank = 1,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 170, PercentUsed = 17.02, Rank = 3,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 69, PercentUsed = 6.95, Rank = 4,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 43, PercentUsed = 4.30, Rank = 4,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 7, PercentUsed = 0.70, Rank = 5,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 145, PercentUsed = 14.50, Rank = 2,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 57, PercentUsed = 5.70, Rank = 3,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 8, PercentUsed = 0.80, Rank = 5,},
            }
        };

        public static ItemRank InnasFavor = new ItemRank

        {
            Item = Legendary.InnasFavor,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 214, PercentUsed = 21.55, Rank = 2,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 41, PercentUsed = 4.10, Rank = 5,},
            }
        };

        public static ItemRank ThundergodsVigor = new ItemRank

        {
            Item = Legendary.ThundergodsVigor,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 150, PercentUsed = 15.11, Rank = 3,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 106, PercentUsed = 10.77, Rank = 2,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 28, PercentUsed = 2.80, Rank = 9,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 16, PercentUsed = 1.60, Rank = 10,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 22, PercentUsed = 2.22, Rank = 8,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 82, PercentUsed = 8.20, Rank = 2,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 20, PercentUsed = 2.00, Rank = 6,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 30, PercentUsed = 3.00, Rank = 4,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 4, PercentUsed = 0.40, Rank = 9,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 11, PercentUsed = 1.10, Rank = 8,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 6, PercentUsed = 0.60, Rank = 7,},
            }
        };

        public static ItemRank StringOfEars = new ItemRank

        {
            Item = Legendary.StringOfEars,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 81, PercentUsed = 8.16, Rank = 4,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 24, PercentUsed = 2.44, Rank = 8,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 125, PercentUsed = 12.50, Rank = 3,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 104, PercentUsed = 10.48, Rank = 3,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 287, PercentUsed = 28.73, Rank = 1,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 36, PercentUsed = 3.63, Rank = 5,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 14, PercentUsed = 1.40, Rank = 9,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 36, PercentUsed = 3.60, Rank = 6,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 211, PercentUsed = 21.10, Rank = 2,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 8, PercentUsed = 0.80, Rank = 6,},
            }
        };

        public static ItemRank TheWitchingHour = new ItemRank

        {
            Item = Legendary.TheWitchingHour,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 78, PercentUsed = 7.85, Rank = 5,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 36, PercentUsed = 3.66, Rank = 6,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 327, PercentUsed = 32.70, Rank = 1,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 238, PercentUsed = 23.99, Rank = 2,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 167, PercentUsed = 16.72, Rank = 4,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 87, PercentUsed = 8.76, Rank = 3,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 613, PercentUsed = 61.30, Rank = 1,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 53, PercentUsed = 5.30, Rank = 4,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 839, PercentUsed = 83.90, Rank = 1,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 474, PercentUsed = 47.40, Rank = 1,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 521, PercentUsed = 52.10, Rank = 1,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 260, PercentUsed = 26.00, Rank = 2,},
            }
        };

        public static ItemRank VigilanteBelt = new ItemRank

        {
            Item = Legendary.VigilanteBelt,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 58, PercentUsed = 5.84, Rank = 6,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 18, PercentUsed = 1.80, Rank = 10,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 7, PercentUsed = 0.71, Rank = 10,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 363, PercentUsed = 36.56, Rank = 1,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 52, PercentUsed = 5.20, Rank = 3,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 24, PercentUsed = 2.40, Rank = 5,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 5, PercentUsed = 0.50, Rank = 7,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 136, PercentUsed = 13.60, Rank = 3,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 13, PercentUsed = 1.30, Rank = 7,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 550, PercentUsed = 55.00, Rank = 1,},
            }
        };

        public static ItemRank CaptainCrimsonsSilkGirdle = new ItemRank

        {
            Item = Legendary.CaptainCrimsonsSilkGirdle,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 45, PercentUsed = 4.53, Rank = 7,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 10, PercentUsed = 1.02, Rank = 10,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 16, PercentUsed = 1.61, Rank = 9,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 16, PercentUsed = 1.60, Rank = 9,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 291, PercentUsed = 29.31, Rank = 2,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 15, PercentUsed = 1.50, Rank = 8,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 10, PercentUsed = 1.00, Rank = 9,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 111, PercentUsed = 11.10, Rank = 3,},
            }
        };

        public static ItemRank HellcatWaistguard = new ItemRank

        {
            Item = Legendary.HellcatWaistguard,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 43, PercentUsed = 4.33, Rank = 8,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 64, PercentUsed = 6.40, Rank = 5,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 46, PercentUsed = 4.64, Rank = 7,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 24, PercentUsed = 2.40, Rank = 7,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 12, PercentUsed = 1.21, Rank = 10,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 21, PercentUsed = 2.10, Rank = 6,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 1, PercentUsed = 0.10, Rank = 8,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 20, PercentUsed = 2.00, Rank = 8,},
            }
        };

        public static ItemRank FleetingStrap = new ItemRank

        {
            Item = Legendary.FleetingStrap,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 19, PercentUsed = 1.91, Rank = 9,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 37, PercentUsed = 3.70, Rank = 7,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 21, PercentUsed = 2.12, Rank = 8,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 2, PercentUsed = 0.20, Rank = 10,},
            }
        };

        public static ItemRank HarringtonWaistguard = new ItemRank

        {
            Item = Legendary.HarringtonWaistguard,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 16, PercentUsed = 1.61, Rank = 10,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 24, PercentUsed = 2.44, Rank = 7,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 88, PercentUsed = 8.80, Rank = 4,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 80, PercentUsed = 8.06, Rank = 5,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 41, PercentUsed = 4.10, Rank = 5,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 33, PercentUsed = 3.32, Rank = 6,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 8, PercentUsed = 0.80, Rank = 10,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 17, PercentUsed = 1.70, Rank = 8,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 32, PercentUsed = 3.20, Rank = 3,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 39, PercentUsed = 3.90, Rank = 5,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 48, PercentUsed = 4.80, Rank = 4,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 22, PercentUsed = 2.20, Rank = 4,},
            }
        };

        public static ItemRank DepthDiggers = new ItemRank

        {
            Item = Legendary.DepthDiggers,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 248, PercentUsed = 24.97, Rank = 1,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 87, PercentUsed = 8.84, Rank = 5,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 7, PercentUsed = 0.70, Rank = 6,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 2, PercentUsed = 0.20, Rank = 9,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 16, PercentUsed = 1.61, Rank = 6,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 210, PercentUsed = 21.00, Rank = 2,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 18, PercentUsed = 1.80, Rank = 4,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 6, PercentUsed = 0.60, Rank = 4,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 6, PercentUsed = 0.60, Rank = 6,},
            }
        };

        public static ItemRank BlackthornesJoustingMail = new ItemRank

        {
            Item = Legendary.BlackthornesJoustingMail,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 228, PercentUsed = 22.96, Rank = 2,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 96, PercentUsed = 9.76, Rank = 4,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 17, PercentUsed = 1.70, Rank = 3,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 411, PercentUsed = 41.43, Rank = 1,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 49, PercentUsed = 4.90, Rank = 2,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 31, PercentUsed = 3.12, Rank = 4,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 482, PercentUsed = 48.20, Rank = 1,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 11, PercentUsed = 1.10, Rank = 6,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 2, PercentUsed = 0.20, Rank = 6,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 172, PercentUsed = 17.20, Rank = 3,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 13, PercentUsed = 1.30, Rank = 4,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 3, PercentUsed = 0.30, Rank = 7,},
            }
        };

        public static ItemRank InnasTemperance = new ItemRank

        {
            Item = Legendary.InnasTemperance,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 189, PercentUsed = 19.03, Rank = 3,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 3, PercentUsed = 0.30, Rank = 10,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 49, PercentUsed = 4.90, Rank = 5,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 12, PercentUsed = 1.20, Rank = 3,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 5, PercentUsed = 0.50, Rank = 10,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 3, PercentUsed = 0.30, Rank = 8,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 12, PercentUsed = 1.20, Rank = 5,},
            }
        };

        public static ItemRank ScalesOfTheDancingSerpent = new ItemRank

        {
            Item = Legendary.ScalesOfTheDancingSerpent,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 119, PercentUsed = 11.98, Rank = 4,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 86, PercentUsed = 8.60, Rank = 4,},
            }
        };

        public static ItemRank CaptainCrimsonsThrust = new ItemRank

        {
            Item = Legendary.CaptainCrimsonsThrust,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 96, PercentUsed = 9.67, Rank = 5,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 9, PercentUsed = 0.91, Rank = 10,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 13, PercentUsed = 1.30, Rank = 4,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 22, PercentUsed = 2.22, Rank = 7,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 23, PercentUsed = 2.30, Rank = 5,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 154, PercentUsed = 15.51, Rank = 2,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 104, PercentUsed = 10.40, Rank = 3,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 4, PercentUsed = 0.40, Rank = 8,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 1, PercentUsed = 0.10, Rank = 7,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 7, PercentUsed = 0.70, Rank = 9,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 10, PercentUsed = 1.00, Rank = 5,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 121, PercentUsed = 12.10, Rank = 2,},
            }
        };

        public static ItemRank CainsHabit = new ItemRank

        {
            Item = Legendary.CainsHabit,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 32, PercentUsed = 3.22, Rank = 6,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 21, PercentUsed = 2.13, Rank = 7,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 5, PercentUsed = 0.50, Rank = 8,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 116, PercentUsed = 11.69, Rank = 3,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 34, PercentUsed = 3.40, Rank = 3,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 10, PercentUsed = 1.01, Rank = 8,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 24, PercentUsed = 2.40, Rank = 7,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 15, PercentUsed = 1.50, Rank = 5,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 4, PercentUsed = 0.40, Rank = 5,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 178, PercentUsed = 17.80, Rank = 2,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 23, PercentUsed = 2.30, Rank = 2,},
            }
        };

        public static ItemRank AshearasPace = new ItemRank

        {
            Item = Legendary.AshearasPace,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 23, PercentUsed = 2.32, Rank = 7,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 49, PercentUsed = 4.98, Rank = 6,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 10, PercentUsed = 1.00, Rank = 5,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 39, PercentUsed = 3.93, Rank = 6,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 14, PercentUsed = 1.40, Rank = 8,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 19, PercentUsed = 1.91, Rank = 5,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 3, PercentUsed = 0.30, Rank = 9,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 8, PercentUsed = 0.80, Rank = 8,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 8, PercentUsed = 0.80, Rank = 7,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 23, PercentUsed = 2.30, Rank = 3,},
            }
        };

        public static ItemRank HexingPantsOfMrYan = new ItemRank

        {
            Item = Legendary.HexingPantsOfMrYan,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 13, PercentUsed = 1.31, Rank = 8,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 143, PercentUsed = 14.53, Rank = 3,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 32, PercentUsed = 3.20, Rank = 2,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 77, PercentUsed = 7.76, Rank = 5,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 23, PercentUsed = 2.30, Rank = 6,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 14, PercentUsed = 1.41, Rank = 7,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 28, PercentUsed = 2.80, Rank = 6,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 131, PercentUsed = 13.10, Rank = 2,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 18, PercentUsed = 1.80, Rank = 2,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 138, PercentUsed = 13.80, Rank = 4,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 9, PercentUsed = 0.90, Rank = 6,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 3, PercentUsed = 0.30, Rank = 8,},
            }
        };

        public static ItemRank PoxFaulds = new ItemRank

        {
            Item = Legendary.PoxFaulds,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 6, PercentUsed = 0.60, Rank = 9,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 20, PercentUsed = 2.03, Rank = 8,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 6, PercentUsed = 0.60, Rank = 10,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 3, PercentUsed = 0.30, Rank = 9,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 3, PercentUsed = 0.30, Rank = 8,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 6, PercentUsed = 0.60, Rank = 7,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 22, PercentUsed = 2.20, Rank = 6,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 1, PercentUsed = 0.10, Rank = 9,},
            }
        };

        public static ItemRank SwampLandWaders = new ItemRank

        {
            Item = Legendary.SwampLandWaders,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 3, PercentUsed = 0.30, Rank = 10,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 81, PercentUsed = 8.17, Rank = 4,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 54, PercentUsed = 5.40, Rank = 5,},
            }
        };

        public static ItemRank BlackthornesSpurs = new ItemRank

        {
            Item = Legendary.BlackthornesSpurs,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 235, PercentUsed = 23.67, Rank = 1,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 32, PercentUsed = 3.25, Rank = 5,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 9, PercentUsed = 0.90, Rank = 4,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 143, PercentUsed = 14.42, Rank = 3,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 36, PercentUsed = 3.60, Rank = 3,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 24, PercentUsed = 2.42, Rank = 4,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 532, PercentUsed = 53.20, Rank = 1,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 10, PercentUsed = 1.00, Rank = 6,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 101, PercentUsed = 10.10, Rank = 4,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 7, PercentUsed = 0.70, Rank = 7,},
            }
        };

        public static ItemRank IceClimbers = new ItemRank

        {
            Item = Legendary.IceClimbers,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 179, PercentUsed = 18.03, Rank = 2,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 125, PercentUsed = 12.70, Rank = 4,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 14, PercentUsed = 1.40, Rank = 3,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 99, PercentUsed = 9.98, Rank = 4,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 40, PercentUsed = 4.00, Rank = 2,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 19, PercentUsed = 1.91, Rank = 5,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 90, PercentUsed = 9.00, Rank = 4,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 20, PercentUsed = 2.00, Rank = 4,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 48, PercentUsed = 4.80, Rank = 6,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 7, PercentUsed = 0.70, Rank = 6,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 2, PercentUsed = 0.20, Rank = 8,},
            }
        };

        public static ItemRank TheCrudestBoots = new ItemRank

        {
            Item = Legendary.TheCrudestBoots,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 162, PercentUsed = 16.31, Rank = 3,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 106, PercentUsed = 10.60, Rank = 2,},
            }
        };

        public static ItemRank EightdemonBoots = new ItemRank

        {
            Item = Legendary.EightdemonBoots,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 139, PercentUsed = 14.00, Rank = 4,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 88, PercentUsed = 8.80, Rank = 5,},
            }
        };

        public static ItemRank CaptainCrimsonsWaders = new ItemRank

        {
            Item = Legendary.CaptainCrimsonsWaders,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 85, PercentUsed = 8.56, Rank = 5,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 3, PercentUsed = 0.30, Rank = 10,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 4, PercentUsed = 0.40, Rank = 10,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 9, PercentUsed = 0.91, Rank = 10,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 28, PercentUsed = 2.80, Rank = 5,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 148, PercentUsed = 14.90, Rank = 2,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 96, PercentUsed = 9.60, Rank = 3,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 1, PercentUsed = 0.10, Rank = 5,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 5, PercentUsed = 0.50, Rank = 10,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 8, PercentUsed = 0.80, Rank = 5,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 32, PercentUsed = 3.20, Rank = 3,},
            }
        };

        public static ItemRank CainsTravelers = new ItemRank

        {
            Item = Legendary.CainsTravelers,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 56, PercentUsed = 5.64, Rank = 6,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 13, PercentUsed = 1.32, Rank = 7,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 6, PercentUsed = 0.60, Rank = 8,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 92, PercentUsed = 9.27, Rank = 5,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 34, PercentUsed = 3.40, Rank = 4,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 7, PercentUsed = 0.70, Rank = 9,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 25, PercentUsed = 2.50, Rank = 7,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 14, PercentUsed = 1.40, Rank = 5,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 2, PercentUsed = 0.20, Rank = 3,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 160, PercentUsed = 16.00, Rank = 2,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 22, PercentUsed = 2.20, Rank = 2,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 2, PercentUsed = 0.20, Rank = 6,},
            }
        };

        public static ItemRank AshearasFinders = new ItemRank

        {
            Item = Legendary.AshearasFinders,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 42, PercentUsed = 4.23, Rank = 7,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 17, PercentUsed = 1.73, Rank = 6,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 5, PercentUsed = 0.50, Rank = 9,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 27, PercentUsed = 2.72, Rank = 7,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 10, PercentUsed = 1.00, Rank = 8,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 11, PercentUsed = 1.11, Rank = 7,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 5, PercentUsed = 0.50, Rank = 9,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 1, PercentUsed = 0.10, Rank = 9,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 16, PercentUsed = 1.60, Rank = 8,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 10, PercentUsed = 1.00, Rank = 4,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 2, PercentUsed = 0.20, Rank = 5,},
            }
        };

        public static ItemRank IllusoryBoots = new ItemRank

        {
            Item = Legendary.IllusoryBoots,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 25, PercentUsed = 2.52, Rank = 8,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 8, PercentUsed = 0.80, Rank = 5,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 29, PercentUsed = 2.92, Rank = 6,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 4, PercentUsed = 0.40, Rank = 10,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 17, PercentUsed = 1.71, Rank = 6,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 27, PercentUsed = 2.70, Rank = 6,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 1, PercentUsed = 0.10, Rank = 6,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 53, PercentUsed = 5.30, Rank = 5,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 6, PercentUsed = 0.60, Rank = 8,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 107, PercentUsed = 10.70, Rank = 2,},
            }
        };

        public static ItemRank FireWalkers = new ItemRank

        {
            Item = Legendary.FireWalkers,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 15, PercentUsed = 1.51, Rank = 9,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 5, PercentUsed = 0.51, Rank = 9,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 7, PercentUsed = 0.70, Rank = 6,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 14, PercentUsed = 1.41, Rank = 9,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 5, PercentUsed = 0.50, Rank = 9,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 8, PercentUsed = 0.81, Rank = 8,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 4, PercentUsed = 0.40, Rank = 7,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 8, PercentUsed = 0.80, Rank = 9,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 2, PercentUsed = 0.20, Rank = 7,},
            }
        };

        public static ItemRank NatalyasBloodyFootprints = new ItemRank

        {
            Item = Legendary.NatalyasBloodyFootprints,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 14, PercentUsed = 1.41, Rank = 10,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 47, PercentUsed = 4.70, Rank = 2,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 18, PercentUsed = 1.80, Rank = 8,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 66, PercentUsed = 6.60, Rank = 2,},
            }
        };

        public static ItemRank SunwukosShines = new ItemRank

        {
            Item = Legendary.SunwukosShines,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 224, PercentUsed = 22.56, Rank = 1,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 367, PercentUsed = 36.70, Rank = 1,},
            }
        };

        public static ItemRank BlackthornesDuncraigCross = new ItemRank

        {
            Item = Legendary.BlackthornesDuncraigCross,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 133, PercentUsed = 13.39, Rank = 2,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 85, PercentUsed = 8.64, Rank = 4,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 168, PercentUsed = 16.80, Rank = 1,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 168, PercentUsed = 16.94, Rank = 1,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 159, PercentUsed = 15.92, Rank = 2,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 86, PercentUsed = 8.66, Rank = 3,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 199, PercentUsed = 19.90, Rank = 2,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 55, PercentUsed = 5.50, Rank = 6,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 23, PercentUsed = 2.30, Rank = 3,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 83, PercentUsed = 8.30, Rank = 3,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 108, PercentUsed = 10.80, Rank = 3,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 42, PercentUsed = 4.20, Rank = 9,},
            }
        };

        public static ItemRank CountessJuliasCameo = new ItemRank

        {
            Item = Legendary.CountessJuliasCameo,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 84, PercentUsed = 8.46, Rank = 3,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 94, PercentUsed = 9.55, Rank = 3,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 70, PercentUsed = 7.00, Rank = 5,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 116, PercentUsed = 11.69, Rank = 2,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 42, PercentUsed = 4.20, Rank = 9,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 70, PercentUsed = 7.05, Rank = 6,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 14, PercentUsed = 1.40, Rank = 9,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 32, PercentUsed = 3.20, Rank = 10,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 34, PercentUsed = 3.40, Rank = 10,},
            }
        };

        public static ItemRank Ouroboros = new ItemRank

        {
            Item = Legendary.Ouroboros,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 64, PercentUsed = 6.45, Rank = 4,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 95, PercentUsed = 9.65, Rank = 2,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 84, PercentUsed = 8.40, Rank = 4,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 91, PercentUsed = 9.17, Rank = 3,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 75, PercentUsed = 7.51, Rank = 3,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 73, PercentUsed = 7.35, Rank = 5,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 24, PercentUsed = 2.40, Rank = 6,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 44, PercentUsed = 4.40, Rank = 8,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 20, PercentUsed = 2.00, Rank = 4,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 47, PercentUsed = 4.70, Rank = 8,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 54, PercentUsed = 5.40, Rank = 8,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 43, PercentUsed = 4.30, Rank = 8,},
            }
        };

        public static ItemRank TheTravelersPledge = new ItemRank

        {
            Item = Legendary.TheTravelersPledge,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 60, PercentUsed = 6.04, Rank = 5,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 57, PercentUsed = 5.79, Rank = 7,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 136, PercentUsed = 13.60, Rank = 3,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 54, PercentUsed = 5.44, Rank = 7,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 65, PercentUsed = 6.51, Rank = 4,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 64, PercentUsed = 6.45, Rank = 7,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 28, PercentUsed = 2.80, Rank = 5,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 44, PercentUsed = 4.40, Rank = 9,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 169, PercentUsed = 16.90, Rank = 2,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 68, PercentUsed = 6.80, Rank = 4,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 72, PercentUsed = 7.20, Rank = 5,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 60, PercentUsed = 6.00, Rank = 5,},
            }
        };

        public static ItemRank GoldenGorgetOfLeoric = new ItemRank

        {
            Item = Legendary.GoldenGorgetOfLeoric,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 47, PercentUsed = 4.73, Rank = 6,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 79, PercentUsed = 8.03, Rank = 5,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 40, PercentUsed = 4.00, Rank = 8,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 79, PercentUsed = 7.96, Rank = 5,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 50, PercentUsed = 5.01, Rank = 7,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 52, PercentUsed = 5.24, Rank = 8,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 17, PercentUsed = 1.70, Rank = 8,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 40, PercentUsed = 4.00, Rank = 9,},
            }
        };

        public static ItemRank SquirtsNecklace = new ItemRank

        {
            Item = Legendary.SquirtsNecklace,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 40, PercentUsed = 4.03, Rank = 8,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 47, PercentUsed = 4.78, Rank = 8,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 63, PercentUsed = 6.30, Rank = 7,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 36, PercentUsed = 3.63, Rank = 10,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 48, PercentUsed = 4.80, Rank = 8,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 79, PercentUsed = 7.96, Rank = 4,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 15, PercentUsed = 1.50, Rank = 9,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 39, PercentUsed = 3.90, Rank = 9,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 40, PercentUsed = 4.00, Rank = 10,},
            }
        };

        public static ItemRank HauntOfVaxo = new ItemRank

        {
            Item = Legendary.HauntOfVaxo,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 37, PercentUsed = 3.73, Rank = 9,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 161, PercentUsed = 16.36, Rank = 1,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 66, PercentUsed = 6.60, Rank = 6,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 65, PercentUsed = 6.55, Rank = 6,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 59, PercentUsed = 5.91, Rank = 5,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 92, PercentUsed = 9.26, Rank = 2,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 34, PercentUsed = 3.40, Rank = 4,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 83, PercentUsed = 8.30, Rank = 4,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 15, PercentUsed = 1.50, Rank = 8,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 58, PercentUsed = 5.80, Rank = 5,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 69, PercentUsed = 6.90, Rank = 6,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 95, PercentUsed = 9.50, Rank = 2,},
            }
        };

        public static ItemRank EyeOfEtlich = new ItemRank

        {
            Item = Legendary.EyeOfEtlich,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 31, PercentUsed = 3.12, Rank = 10,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 31, PercentUsed = 3.10, Rank = 10,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 58, PercentUsed = 5.81, Rank = 6,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 13, PercentUsed = 1.30, Rank = 10,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 43, PercentUsed = 4.30, Rank = 10,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 14, PercentUsed = 1.40, Rank = 10,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 49, PercentUsed = 4.90, Rank = 7,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 141, PercentUsed = 14.10, Rank = 1,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 69, PercentUsed = 6.90, Rank = 4,},
            }
        };

        public static ItemRank RingOfRoyalGrandeur = new ItemRank

        {
            Item = Legendary.RingOfRoyalGrandeur,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 830, PercentUsed = 83.59, Rank = 1,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 820, PercentUsed = 83.33, Rank = 1,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 910, PercentUsed = 91.00, Rank = 1,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 681, PercentUsed = 68.65, Rank = 1,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 706, PercentUsed = 70.67, Rank = 1,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 788, PercentUsed = 79.36, Rank = 1,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 809, PercentUsed = 80.90, Rank = 1,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 748, PercentUsed = 74.80, Rank = 1,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 926, PercentUsed = 92.60, Rank = 1,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 582, PercentUsed = 58.20, Rank = 1,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 411, PercentUsed = 41.10, Rank = 3,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 539, PercentUsed = 53.90, Rank = 2,},
            }
        };

        public static ItemRank Unity = new ItemRank

        {
            Item = Legendary.Unity,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 250, PercentUsed = 25.18, Rank = 2,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 315, PercentUsed = 32.01, Rank = 3,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 241, PercentUsed = 24.10, Rank = 3,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 228, PercentUsed = 22.98, Rank = 4,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 314, PercentUsed = 31.43, Rank = 3,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 256, PercentUsed = 25.78, Rank = 3,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 487, PercentUsed = 48.70, Rank = 2,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 329, PercentUsed = 32.90, Rank = 3,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 83, PercentUsed = 8.30, Rank = 4,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 295, PercentUsed = 29.50, Rank = 3,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 475, PercentUsed = 47.50, Rank = 2,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 351, PercentUsed = 35.10, Rank = 3,},
            }
        };

        public static ItemRank StoneOfJordan = new ItemRank

        {
            Item = Legendary.StoneOfJordan,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 240, PercentUsed = 24.17, Rank = 3,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 432, PercentUsed = 43.90, Rank = 2,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 401, PercentUsed = 40.10, Rank = 2,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 352, PercentUsed = 35.48, Rank = 3,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 569, PercentUsed = 56.96, Rank = 2,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 509, PercentUsed = 51.26, Rank = 2,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 262, PercentUsed = 26.20, Rank = 3,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 526, PercentUsed = 52.60, Rank = 2,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 578, PercentUsed = 57.80, Rank = 2,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 405, PercentUsed = 40.50, Rank = 2,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 802, PercentUsed = 80.20, Rank = 1,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 624, PercentUsed = 62.40, Rank = 1,},
            }
        };

        public static ItemRank TheCompassRose = new ItemRank

        {
            Item = Legendary.TheCompassRose,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 63, PercentUsed = 6.34, Rank = 4,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 37, PercentUsed = 3.76, Rank = 6,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 122, PercentUsed = 12.20, Rank = 4,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 37, PercentUsed = 3.73, Rank = 6,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 55, PercentUsed = 5.51, Rank = 4,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 45, PercentUsed = 4.53, Rank = 5,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 19, PercentUsed = 1.90, Rank = 6,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 26, PercentUsed = 2.60, Rank = 5,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 165, PercentUsed = 16.50, Rank = 3,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 37, PercentUsed = 3.70, Rank = 6,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 49, PercentUsed = 4.90, Rank = 4,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 50, PercentUsed = 5.00, Rank = 4,},
            }
        };

        public static ItemRank LeoricsSignet = new ItemRank

        {
            Item = Legendary.LeoricsSignet,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 52, PercentUsed = 5.24, Rank = 5,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 39, PercentUsed = 3.96, Rank = 5,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 30, PercentUsed = 3.00, Rank = 6,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 24, PercentUsed = 2.42, Rank = 7,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 54, PercentUsed = 5.41, Rank = 5,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 36, PercentUsed = 3.63, Rank = 6,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 9, PercentUsed = 0.90, Rank = 9,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 14, PercentUsed = 1.40, Rank = 7,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 12, PercentUsed = 1.20, Rank = 7,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 17, PercentUsed = 1.70, Rank = 6,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 18, PercentUsed = 1.80, Rank = 10,},
            }
        };

        public static ItemRank BulkathossWeddingBand = new ItemRank

        {
            Item = Legendary.BulkathossWeddingBand,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 50, PercentUsed = 5.04, Rank = 6,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 41, PercentUsed = 4.17, Rank = 4,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 20, PercentUsed = 2.00, Rank = 7,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 16, PercentUsed = 1.61, Rank = 8,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 18, PercentUsed = 1.80, Rank = 9,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 24, PercentUsed = 2.42, Rank = 8,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 16, PercentUsed = 1.60, Rank = 8,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 59, PercentUsed = 5.90, Rank = 4,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 9, PercentUsed = 0.90, Rank = 10,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 12, PercentUsed = 1.20, Rank = 9,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 6, PercentUsed = 0.60, Rank = 9,},
            }
        };

        public static ItemRank BandOfTheRueChambers = new ItemRank

        {
            Item = Legendary.BandOfTheRueChambers,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 30, PercentUsed = 3.02, Rank = 7,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 48, PercentUsed = 4.80, Rank = 4,},
            }
        };

        public static ItemRank PuzzleRing = new ItemRank

        {
            Item = Legendary.PuzzleRing,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 27, PercentUsed = 2.72, Rank = 8,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
            }
        };

        public static ItemRank ManaldHeal = new ItemRank

        {
            Item = Legendary.ManaldHeal,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 26, PercentUsed = 2.62, Rank = 9,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 29, PercentUsed = 2.92, Rank = 7,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
            }
        };

        public static ItemRank RogarsHugeStone = new ItemRank

        {
            Item = Legendary.RogarsHugeStone,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 25, PercentUsed = 2.52, Rank = 10,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 14, PercentUsed = 1.42, Rank = 9,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 14, PercentUsed = 1.40, Rank = 9,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 7, PercentUsed = 0.70, Rank = 10,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 11, PercentUsed = 1.10, Rank = 10,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 31, PercentUsed = 3.10, Rank = 6,},
            }
        };

        public static ItemRank ThunderfuryBlessedBladeOfTheWindseeker = new ItemRank

        {
            Item = Legendary.ThunderfuryBlessedBladeOfTheWindseeker,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 64, PercentUsed = 6.45, Rank = 2,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 102, PercentUsed = 10.37, Rank = 2,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 42, PercentUsed = 4.23, Rank = 6,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 78, PercentUsed = 7.81, Rank = 5,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 9, PercentUsed = 0.90, Rank = 10,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 14, PercentUsed = 1.40, Rank = 4,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 8, PercentUsed = 0.80, Rank = 8,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 11, PercentUsed = 1.10, Rank = 7,},
            }
        };

        public static ItemRank OdynSon = new ItemRank

        {
            Item = Legendary.OdynSon,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 27, PercentUsed = 2.72, Rank = 6,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 99, PercentUsed = 10.06, Rank = 3,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 31, PercentUsed = 3.10, Rank = 2,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 49, PercentUsed = 4.90, Rank = 3,},
            }
        };

        public static ItemRank WonKhimLau = new ItemRank

        {
            Item = Legendary.WonKhimLau,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 62, PercentUsed = 6.24, Rank = 3,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 25, PercentUsed = 2.50, Rank = 3,},
            }
        };

        public static ItemRank Jawbreaker = new ItemRank

        {
            Item = Legendary.Jawbreaker,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 34, PercentUsed = 3.42, Rank = 4,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 19, PercentUsed = 1.90, Rank = 4,},
            }
        };

        public static ItemRank BornsFuriousWrath = new ItemRank

        {
            Item = Legendary.BornsFuriousWrath,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 26, PercentUsed = 2.62, Rank = 8,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 21, PercentUsed = 2.10, Rank = 9,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 10, PercentUsed = 1.00, Rank = 6,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 57, PercentUsed = 5.70, Rank = 5,},
            }
        };

        public static ItemRank SledgeFist = new ItemRank

        {
            Item = Legendary.SledgeFist,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 38, PercentUsed = 3.83, Rank = 5,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 9, PercentUsed = 0.90, Rank = 8,},
            }
        };

        public static ItemRank ShardOfHate = new ItemRank

        {
            Item = Legendary.ShardOfHate,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 21, PercentUsed = 2.11, Rank = 10,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 43, PercentUsed = 4.37, Rank = 8,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 9, PercentUsed = 0.90, Rank = 6,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 24, PercentUsed = 2.40, Rank = 7,},
            }
        };

        public static ItemRank Stormshield = new ItemRank

        {
            Item = Legendary.Stormshield,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 21, PercentUsed = 2.11, Rank = 8,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 9, PercentUsed = 0.91, Rank = 6,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 9, PercentUsed = 0.91, Rank = 10,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 46, PercentUsed = 4.60, Rank = 6,},
            }
        };

        public static ItemRank Azurewrath = new ItemRank

        {
            Item = Legendary.Azurewrath,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 26, PercentUsed = 2.62, Rank = 7,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 9, PercentUsed = 0.91, Rank = 10,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 27, PercentUsed = 2.72, Rank = 7,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 9, PercentUsed = 0.90, Rank = 9,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 4, PercentUsed = 0.40, Rank = 8,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 6, PercentUsed = 0.60, Rank = 9,},
            }
        };

        public static ItemRank SunKeeper = new ItemRank

        {
            Item = Legendary.SunKeeper,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 11, PercentUsed = 1.11, Rank = 10,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 48, PercentUsed = 4.88, Rank = 6,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 95, PercentUsed = 9.58, Rank = 3,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 171, PercentUsed = 17.12, Rank = 3,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 10, PercentUsed = 1.00, Rank = 7,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 26, PercentUsed = 2.60, Rank = 6,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 143, PercentUsed = 14.30, Rank = 3,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 166, PercentUsed = 16.60, Rank = 2,},
            }
        };

        public static ItemRank FlyingDragon = new ItemRank

        {
            Item = Legendary.FlyingDragon,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 407, PercentUsed = 40.99, Rank = 1,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 781, PercentUsed = 78.10, Rank = 1,},
            }
        };

        public static ItemRank InnasReach = new ItemRank

        {
            Item = Legendary.InnasReach,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 60, PercentUsed = 6.04, Rank = 4,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
            }
        };

        public static ItemRank IncenseTorchOfTheGrandTemple = new ItemRank

        {
            Item = Legendary.IncenseTorchOfTheGrandTemple,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 23, PercentUsed = 2.32, Rank = 9,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 18, PercentUsed = 1.80, Rank = 5,},
            }
        };

        public static ItemRank CusterianWristguards = new ItemRank

        {
            Item = Legendary.CusterianWristguards,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 22, PercentUsed = 2.24, Rank = 6,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 44, PercentUsed = 4.40, Rank = 6,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 14, PercentUsed = 1.41, Rank = 5,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 12, PercentUsed = 1.20, Rank = 8,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 13, PercentUsed = 1.31, Rank = 6,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 21, PercentUsed = 2.10, Rank = 5,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 14, PercentUsed = 1.40, Rank = 6,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 76, PercentUsed = 7.60, Rank = 4,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 29, PercentUsed = 2.90, Rank = 5,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 20, PercentUsed = 2.00, Rank = 7,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 22, PercentUsed = 2.20, Rank = 4,},
            }
        };

        public static ItemRank AndarielsVisage = new ItemRank

        {
            Item = Legendary.AndarielsVisage,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 62, PercentUsed = 6.30, Rank = 5,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 21, PercentUsed = 2.10, Rank = 3,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 14, PercentUsed = 1.41, Rank = 5,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 39, PercentUsed = 3.90, Rank = 4,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 36, PercentUsed = 3.63, Rank = 4,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 24, PercentUsed = 2.40, Rank = 6,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 46, PercentUsed = 4.60, Rank = 5,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 44, PercentUsed = 4.40, Rank = 2,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 9, PercentUsed = 0.90, Rank = 5,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 31, PercentUsed = 3.10, Rank = 3,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 17, PercentUsed = 1.70, Rank = 4,},
            }
        };

        public static ItemRank GyanaNaKashu = new ItemRank

        {
            Item = Legendary.GyanaNaKashu,
            HardcoreRank = new List<ItemRankData>
            {
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 7, PercentUsed = 0.70, Rank = 10,},
            }
        };

        public static ItemRank VileWard = new ItemRank

        {
            Item = Legendary.VileWard,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 102, PercentUsed = 10.37, Rank = 4,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 2, PercentUsed = 0.20, Rank = 9,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 3, PercentUsed = 0.30, Rank = 9,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 1, PercentUsed = 0.10, Rank = 8,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 664, PercentUsed = 66.40, Rank = 1,},
            }
        };

        public static ItemRank TalRashasRelentlessPursuit = new ItemRank

        {
            Item = Legendary.TalRashasRelentlessPursuit,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 24, PercentUsed = 2.42, Rank = 6,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 114, PercentUsed = 11.41, Rank = 3,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 15, PercentUsed = 1.50, Rank = 8,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 2, PercentUsed = 0.20, Rank = 5,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 94, PercentUsed = 9.40, Rank = 4,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 42, PercentUsed = 4.20, Rank = 3,},
            }
        };

        public static ItemRank TaskerAndTheo = new ItemRank

        {
            Item = Legendary.TaskerAndTheo,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 78, PercentUsed = 7.93, Rank = 5,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 617, PercentUsed = 61.70, Rank = 1,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 609, PercentUsed = 61.39, Rank = 1,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 53, PercentUsed = 5.31, Rank = 3,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 23, PercentUsed = 2.32, Rank = 5,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 2, PercentUsed = 0.20, Rank = 9,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 95, PercentUsed = 9.50, Rank = 3,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 895, PercentUsed = 89.50, Rank = 1,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 493, PercentUsed = 49.30, Rank = 1,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 52, PercentUsed = 5.20, Rank = 3,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 7, PercentUsed = 0.70, Rank = 7,},
            }
        };

        public static ItemRank Goldwrap = new ItemRank

        {
            Item = Legendary.Goldwrap,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 61, PercentUsed = 6.10, Rank = 6,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 52, PercentUsed = 5.24, Rank = 6,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 27, PercentUsed = 2.70, Rank = 6,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 13, PercentUsed = 1.31, Rank = 9,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 17, PercentUsed = 1.70, Rank = 7,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 18, PercentUsed = 1.80, Rank = 7,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 70, PercentUsed = 7.00, Rank = 2,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 52, PercentUsed = 5.20, Rank = 4,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 44, PercentUsed = 4.40, Rank = 5,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 6, PercentUsed = 0.60, Rank = 8,},
            }
        };

        public static ItemRank DemonsPlate = new ItemRank

        {
            Item = Legendary.DemonsPlate,
            HardcoreRank = new List<ItemRankData>
            {
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 1, PercentUsed = 0.10, Rank = 10,},
            }
        };

        public static ItemRank IrontoeMudsputters = new ItemRank

        {
            Item = Legendary.IrontoeMudsputters,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 2, PercentUsed = 0.20, Rank = 10,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 2, PercentUsed = 0.20, Rank = 10,},
            }
        };

        public static ItemRank MarasKaleidoscope = new ItemRank

        {
            Item = Legendary.MarasKaleidoscope,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 41, PercentUsed = 4.17, Rank = 9,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 54, PercentUsed = 5.44, Rank = 8,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 45, PercentUsed = 4.53, Rank = 9,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 21, PercentUsed = 2.10, Rank = 7,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 60, PercentUsed = 6.00, Rank = 5,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 17, PercentUsed = 1.70, Rank = 6,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 56, PercentUsed = 5.60, Rank = 6,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 77, PercentUsed = 7.70, Rank = 4,},
            }
        };

        public static ItemRank AvariceBand = new ItemRank

        {
            Item = Legendary.AvariceBand,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 15, PercentUsed = 1.50, Rank = 8,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 13, PercentUsed = 1.31, Rank = 10,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 15, PercentUsed = 1.50, Rank = 10,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 31, PercentUsed = 3.10, Rank = 5,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 16, PercentUsed = 1.60, Rank = 6,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 77, PercentUsed = 7.70, Rank = 5,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 14, PercentUsed = 1.40, Rank = 8,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 29, PercentUsed = 2.90, Rank = 5,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 36, PercentUsed = 3.60, Rank = 5,},
            }
        };

        public static ItemRank NatalyasReflection = new ItemRank

        {
            Item = Legendary.NatalyasReflection,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 121, PercentUsed = 12.10, Rank = 5,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 19, PercentUsed = 1.90, Rank = 7,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 47, PercentUsed = 4.70, Rank = 6,},
            }
        };

        public static ItemRank BandOfUntoldSecrets = new ItemRank

        {
            Item = Legendary.BandOfUntoldSecrets,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 19, PercentUsed = 1.93, Rank = 7,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 12, PercentUsed = 1.20, Rank = 10,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 14, PercentUsed = 1.41, Rank = 9,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 21, PercentUsed = 2.10, Rank = 8,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 17, PercentUsed = 1.71, Rank = 9,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 7, PercentUsed = 0.70, Rank = 10,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 12, PercentUsed = 1.20, Rank = 8,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 26, PercentUsed = 2.60, Rank = 8,},
            }
        };

        public static ItemRank TheBurningAxeOfSankis = new ItemRank

        {
            Item = Legendary.TheBurningAxeOfSankis,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 37, PercentUsed = 3.76, Rank = 9,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 39, PercentUsed = 3.90, Rank = 6,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 5, PercentUsed = 0.50, Rank = 8,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 13, PercentUsed = 1.30, Rank = 10,},
            }
        };

        public static ItemRank Devastator = new ItemRank

        {
            Item = Legendary.Devastator,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 30, PercentUsed = 3.05, Rank = 6,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 28, PercentUsed = 2.80, Rank = 7,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 5, PercentUsed = 0.50, Rank = 10,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 4, PercentUsed = 0.40, Rank = 10,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 9, PercentUsed = 0.90, Rank = 8,},
            }
        };

        public static ItemRank TheFurnace = new ItemRank

        {
            Item = Legendary.TheFurnace,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 76, PercentUsed = 7.72, Rank = 5,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 14, PercentUsed = 1.41, Rank = 9,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 176, PercentUsed = 17.62, Rank = 2,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 169, PercentUsed = 17.02, Rank = 2,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 14, PercentUsed = 1.40, Rank = 6,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 564, PercentUsed = 56.40, Rank = 1,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 207, PercentUsed = 20.70, Rank = 2,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 555, PercentUsed = 55.50, Rank = 1,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 577, PercentUsed = 57.70, Rank = 1,},
            }
        };

        public static ItemRank SanguinaryVambraces = new ItemRank

        {
            Item = Legendary.SanguinaryVambraces,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 16, PercentUsed = 1.63, Rank = 8,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 7, PercentUsed = 0.71, Rank = 9,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 4, PercentUsed = 0.40, Rank = 9,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 2, PercentUsed = 0.20, Rank = 10,},
            }
        };

        public static ItemRank ImmortalKingsTriumph = new ItemRank

        {
            Item = Legendary.ImmortalKingsTriumph,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 407, PercentUsed = 41.36, Rank = 1,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 115, PercentUsed = 11.50, Rank = 2,},
            }
        };

        public static ItemRank EyesOfTheEarth = new ItemRank

        {
            Item = Legendary.EyesOfTheEarth,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 147, PercentUsed = 14.94, Rank = 2,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 81, PercentUsed = 8.10, Rank = 4,},
            }
        };

        public static ItemRank RaekorsWill = new ItemRank

        {
            Item = Legendary.RaekorsWill,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 11, PercentUsed = 1.12, Rank = 9,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 99, PercentUsed = 9.90, Rank = 3,},
            }
        };

        public static ItemRank SkullOfResonance = new ItemRank

        {
            Item = Legendary.SkullOfResonance,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 41, PercentUsed = 4.17, Rank = 6,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 15, PercentUsed = 1.50, Rank = 6,},
            }
        };

        public static ItemRank MempoOfTwilight = new ItemRank

        {
            Item = Legendary.MempoOfTwilight,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 10, PercentUsed = 1.02, Rank = 10,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 3, PercentUsed = 0.30, Rank = 8,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 5, PercentUsed = 0.50, Rank = 10,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 5, PercentUsed = 0.50, Rank = 8,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 1, PercentUsed = 0.10, Rank = 10,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 2, PercentUsed = 0.20, Rank = 6,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 2, PercentUsed = 0.20, Rank = 9,},
            }
        };

        public static ItemRank SpiresOfTheEarth = new ItemRank

        {
            Item = Legendary.SpiresOfTheEarth,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 249, PercentUsed = 25.30, Rank = 2,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 139, PercentUsed = 13.90, Rank = 2,},
            }
        };

        public static ItemRank RaekorsBurden = new ItemRank

        {
            Item = Legendary.RaekorsBurden,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 15, PercentUsed = 1.52, Rank = 6,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 3, PercentUsed = 0.30, Rank = 8,},
            }
        };

        public static ItemRank ImmortalKingsEternalReign = new ItemRank

        {
            Item = Legendary.ImmortalKingsEternalReign,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 464, PercentUsed = 47.15, Rank = 1,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 1, PercentUsed = 0.10, Rank = 10,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 171, PercentUsed = 17.10, Rank = 2,},
            }
        };

        public static ItemRank RaekorsHeart = new ItemRank

        {
            Item = Legendary.RaekorsHeart,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 8, PercentUsed = 0.81, Rank = 7,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 2, PercentUsed = 0.20, Rank = 9,},
            }
        };

        public static ItemRank ShiMizusHaori = new ItemRank

        {
            Item = Legendary.ShiMizusHaori,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 2, PercentUsed = 0.20, Rank = 10,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 1, PercentUsed = 0.10, Rank = 10,},
            }
        };

        public static ItemRank ImmortalKingsIrons = new ItemRank

        {
            Item = Legendary.ImmortalKingsIrons,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 352, PercentUsed = 35.77, Rank = 1,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 71, PercentUsed = 7.10, Rank = 4,},
            }
        };

        public static ItemRank RaekorsWraps = new ItemRank

        {
            Item = Legendary.RaekorsWraps,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 12, PercentUsed = 1.22, Rank = 7,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 678, PercentUsed = 67.80, Rank = 1,},
            }
        };

        public static ItemRank PullOfTheEarth = new ItemRank

        {
            Item = Legendary.PullOfTheEarth,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 190, PercentUsed = 19.31, Rank = 3,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 116, PercentUsed = 11.60, Rank = 2,},
            }
        };

        public static ItemRank ImmortalKingsTribalBinding = new ItemRank

        {
            Item = Legendary.ImmortalKingsTribalBinding,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 492, PercentUsed = 50.00, Rank = 1,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 106, PercentUsed = 10.60, Rank = 3,},
            }
        };

        public static ItemRank PrideOfCassius = new ItemRank

        {
            Item = Legendary.PrideOfCassius,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 93, PercentUsed = 9.45, Rank = 3,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 539, PercentUsed = 53.90, Rank = 1,},
            }
        };

        public static ItemRank ChilaniksChain = new ItemRank

        {
            Item = Legendary.ChilaniksChain,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 59, PercentUsed = 6.00, Rank = 4,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 16, PercentUsed = 1.60, Rank = 9,},
            }
        };

        public static ItemRank Lamentation = new ItemRank

        {
            Item = Legendary.Lamentation,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 10, PercentUsed = 1.02, Rank = 9,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 12, PercentUsed = 1.20, Rank = 10,},
            }
        };

        public static ItemRank RaekorsBreeches = new ItemRank

        {
            Item = Legendary.RaekorsBreeches,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 13, PercentUsed = 1.32, Rank = 9,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 1, PercentUsed = 0.10, Rank = 10,},
            }
        };

        public static ItemRank WeightOfTheEarth = new ItemRank

        {
            Item = Legendary.WeightOfTheEarth,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 253, PercentUsed = 25.71, Rank = 2,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 120, PercentUsed = 12.00, Rank = 3,},
            }
        };

        public static ItemRank ImmortalKingsStride = new ItemRank

        {
            Item = Legendary.ImmortalKingsStride,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 311, PercentUsed = 31.61, Rank = 1,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 121, PercentUsed = 12.10, Rank = 3,},
            }
        };

        public static ItemRank LutSocks = new ItemRank

        {
            Item = Legendary.LutSocks,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 268, PercentUsed = 27.24, Rank = 2,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 138, PercentUsed = 13.80, Rank = 2,},
            }
        };

        public static ItemRank RaekorsStriders = new ItemRank

        {
            Item = Legendary.RaekorsStriders,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 11, PercentUsed = 1.12, Rank = 8,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 1, PercentUsed = 0.10, Rank = 8,},
            }
        };

        public static ItemRank KymbosGold = new ItemRank

        {
            Item = Legendary.KymbosGold,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 31, PercentUsed = 3.15, Rank = 10,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 34, PercentUsed = 3.40, Rank = 9,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 17, PercentUsed = 1.70, Rank = 7,},
            }
        };

        public static ItemRank SkullGrasp = new ItemRank

        {
            Item = Legendary.SkullGrasp,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 16, PercentUsed = 1.63, Rank = 8,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 24, PercentUsed = 2.40, Rank = 7,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
            }
        };

        public static ItemRank Nagelring = new ItemRank

        {
            Item = Legendary.Nagelring,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 11, PercentUsed = 1.12, Rank = 10,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
            }
        };

        public static ItemRank Doombringer = new ItemRank

        {
            Item = Legendary.Doombringer,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 14, PercentUsed = 1.42, Rank = 7,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 91, PercentUsed = 9.17, Rank = 4,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 5, PercentUsed = 0.50, Rank = 7,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 19, PercentUsed = 1.90, Rank = 6,},
            }
        };

        public static ItemRank BulkathossWarriorBlood = new ItemRank

        {
            Item = Legendary.BulkathossWarriorBlood,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 13, PercentUsed = 1.32, Rank = 8,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
            }
        };

        public static ItemRank Fulminator = new ItemRank

        {
            Item = Legendary.Fulminator,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 11, PercentUsed = 1.12, Rank = 9,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
            }
        };

        public static ItemRank Maximus = new ItemRank

        {
            Item = Legendary.Maximus,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 184, PercentUsed = 18.70, Rank = 1,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 79, PercentUsed = 7.91, Rank = 4,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 54, PercentUsed = 5.44, Rank = 5,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 116, PercentUsed = 11.60, Rank = 2,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 12, PercentUsed = 1.20, Rank = 5,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 17, PercentUsed = 1.70, Rank = 7,},
            }
        };

        public static ItemRank ImmortalKingsBoulderBreaker = new ItemRank

        {
            Item = Legendary.ImmortalKingsBoulderBreaker,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 82, PercentUsed = 8.33, Rank = 4,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 16, PercentUsed = 1.60, Rank = 8,},
            }
        };

        public static ItemRank StalgardsDecimator = new ItemRank

        {
            Item = Legendary.StalgardsDecimator,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 47, PercentUsed = 4.78, Rank = 7,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 34, PercentUsed = 3.40, Rank = 5,},
            }
        };

        public static ItemRank SchaefersHammer = new ItemRank

        {
            Item = Legendary.SchaefersHammer,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 32, PercentUsed = 3.25, Rank = 10,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 12, PercentUsed = 1.21, Rank = 10,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
            }
        };

        public static ItemRank CrownOfTheInvoker = new ItemRank

        {
            Item = Legendary.CrownOfTheInvoker,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 2, PercentUsed = 0.20, Rank = 10,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 1, PercentUsed = 0.10, Rank = 8,},
            }
        };

        public static ItemRank BlindFaith = new ItemRank

        {
            Item = Legendary.BlindFaith,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 1, PercentUsed = 0.10, Rank = 10,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 1, PercentUsed = 0.10, Rank = 9,},
            }
        };

        public static ItemRank ProfanePauldrons = new ItemRank

        {
            Item = Legendary.ProfanePauldrons,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 1, PercentUsed = 0.10, Rank = 10,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 9, PercentUsed = 0.91, Rank = 7,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 1, PercentUsed = 0.10, Rank = 10,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 2, PercentUsed = 0.20, Rank = 10,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 3, PercentUsed = 0.30, Rank = 9,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 21, PercentUsed = 2.10, Rank = 7,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 3, PercentUsed = 0.30, Rank = 6,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 1, PercentUsed = 0.10, Rank = 7,},
            }
        };

        public static ItemRank PrideOfTheInvoker = new ItemRank

        {
            Item = Legendary.PrideOfTheInvoker,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 4, PercentUsed = 0.40, Rank = 10,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 1, PercentUsed = 0.10, Rank = 10,},
            }
        };

        public static ItemRank HammerJammers = new ItemRank

        {
            Item = Legendary.HammerJammers,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 9, PercentUsed = 0.91, Rank = 9,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 1, PercentUsed = 0.10, Rank = 10,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 1, PercentUsed = 0.10, Rank = 9,},
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
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 193, PercentUsed = 19.30, Rank = 1,},
            }
        };

        public static ItemRank XephirianAmulet = new ItemRank

        {
            Item = Legendary.XephirianAmulet,
            HardcoreRank = new List<ItemRankData>
            {
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 109, PercentUsed = 10.90, Rank = 2,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 118, PercentUsed = 11.80, Rank = 2,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 86, PercentUsed = 8.60, Rank = 3,},
            }
        };

        public static ItemRank TheFlavorOfTime = new ItemRank

        {
            Item = Legendary.TheFlavorOfTime,
            HardcoreRank = new List<ItemRankData>
            {
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 52, PercentUsed = 5.20, Rank = 7,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 18, PercentUsed = 1.80, Rank = 5,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 59, PercentUsed = 5.90, Rank = 6,},
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
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 10, PercentUsed = 1.00, Rank = 8,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 9, PercentUsed = 0.90, Rank = 8,},
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
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 9, PercentUsed = 0.90, Rank = 9,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 13, PercentUsed = 1.30, Rank = 7,},
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
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 4, PercentUsed = 0.40, Rank = 9,},
            }
        };

        public static ItemRank HeartSlaughter = new ItemRank

        {
            Item = Legendary.HeartSlaughter,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 150, PercentUsed = 15.11, Rank = 3,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 42, PercentUsed = 4.20, Rank = 4,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 87, PercentUsed = 8.70, Rank = 2,},
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
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 14, PercentUsed = 1.40, Rank = 9,},
            }
        };

        public static ItemRank GuardiansAversion = new ItemRank

        {
            Item = Legendary.GuardiansAversion,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 30, PercentUsed = 3.00, Rank = 9,},
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
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 874, PercentUsed = 87.40, Rank = 1,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 914, PercentUsed = 91.40, Rank = 1,},
            }
        };

        public static ItemRank NatalyasSight = new ItemRank

        {
            Item = Legendary.NatalyasSight,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 35, PercentUsed = 3.50, Rank = 2,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 5, PercentUsed = 0.50, Rank = 4,},
            }
        };

        public static ItemRank PridesFall = new ItemRank

        {
            Item = Legendary.PridesFall,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 13, PercentUsed = 1.30, Rank = 6,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 6, PercentUsed = 0.60, Rank = 9,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 31, PercentUsed = 3.10, Rank = 3,},
            }
        };

        public static ItemRank BrokenCrown = new ItemRank

        {
            Item = Legendary.BrokenCrown,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 4, PercentUsed = 0.40, Rank = 7,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 2, PercentUsed = 0.20, Rank = 9,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 1, PercentUsed = 0.10, Rank = 6,},
            }
        };

        public static ItemRank MaraudersSpines = new ItemRank

        {
            Item = Legendary.MaraudersSpines,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 885, PercentUsed = 88.50, Rank = 1,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 971, PercentUsed = 97.10, Rank = 1,},
            }
        };

        public static ItemRank BurdenOfTheInvoker = new ItemRank

        {
            Item = Legendary.BurdenOfTheInvoker,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 2, PercentUsed = 0.20, Rank = 8,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
            }
        };

        public static ItemRank MaraudersCarapace = new ItemRank

        {
            Item = Legendary.MaraudersCarapace,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 827, PercentUsed = 82.70, Rank = 1,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 910, PercentUsed = 91.00, Rank = 1,},
            }
        };

        public static ItemRank NatalyasEmbrace = new ItemRank

        {
            Item = Legendary.NatalyasEmbrace,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 27, PercentUsed = 2.70, Rank = 3,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 9, PercentUsed = 0.90, Rank = 3,},
            }
        };

        public static ItemRank TheCloakOfTheGarwulf = new ItemRank

        {
            Item = Legendary.TheCloakOfTheGarwulf,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 13, PercentUsed = 1.30, Rank = 4,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
            }
        };

        public static ItemRank BeckonSail = new ItemRank

        {
            Item = Legendary.BeckonSail,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 8, PercentUsed = 0.80, Rank = 5,},
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
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 1, PercentUsed = 0.10, Rank = 9,},
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
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 230, PercentUsed = 23.00, Rank = 2,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 19, PercentUsed = 1.90, Rank = 3,},
            }
        };

        public static ItemRank TheShadowsGrasp = new ItemRank

        {
            Item = Legendary.TheShadowsGrasp,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 5, PercentUsed = 0.50, Rank = 4,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 4, PercentUsed = 0.40, Rank = 4,},
            }
        };

        public static ItemRank SagesGesture = new ItemRank

        {
            Item = Legendary.SagesGesture,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 2, PercentUsed = 0.20, Rank = 7,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
            }
        };

        public static ItemRank GuardiansCase = new ItemRank

        {
            Item = Legendary.GuardiansCase,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 29, PercentUsed = 2.90, Rank = 8,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 2, PercentUsed = 0.20, Rank = 10,},
            }
        };

        public static ItemRank MaraudersEncasement = new ItemRank

        {
            Item = Legendary.MaraudersEncasement,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 879, PercentUsed = 87.90, Rank = 1,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 954, PercentUsed = 95.40, Rank = 1,},
            }
        };

        public static ItemRank TheShadowsCoil = new ItemRank

        {
            Item = Legendary.TheShadowsCoil,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 4, PercentUsed = 0.40, Rank = 9,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
            }
        };

        public static ItemRank MaraudersTreads = new ItemRank

        {
            Item = Legendary.MaraudersTreads,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 873, PercentUsed = 87.30, Rank = 1,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 928, PercentUsed = 92.80, Rank = 1,},
            }
        };

        public static ItemRank TheShadowsHeels = new ItemRank

        {
            Item = Legendary.TheShadowsHeels,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 6, PercentUsed = 0.60, Rank = 7,},
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
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 750, PercentUsed = 75.00, Rank = 1,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 337, PercentUsed = 33.70, Rank = 2,},
            }
        };

        public static ItemRank MeticulousBolts = new ItemRank

        {
            Item = Legendary.MeticulousBolts,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 64, PercentUsed = 6.40, Rank = 2,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 557, PercentUsed = 55.70, Rank = 1,},
            }
        };

        public static ItemRank DanettasRevenge = new ItemRank

        {
            Item = Legendary.DanettasRevenge,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 27, PercentUsed = 2.70, Rank = 3,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 15, PercentUsed = 1.50, Rank = 9,},
            }
        };

        public static ItemRank DeadMansLegacy = new ItemRank

        {
            Item = Legendary.DeadMansLegacy,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 22, PercentUsed = 2.20, Rank = 4,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
            }
        };

        public static ItemRank DanettasSpite = new ItemRank

        {
            Item = Legendary.DanettasSpite,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 21, PercentUsed = 2.10, Rank = 5,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 15, PercentUsed = 1.50, Rank = 10,},
            }
        };

        public static ItemRank ArchfiendArrows = new ItemRank

        {
            Item = Legendary.ArchfiendArrows,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 17, PercentUsed = 1.70, Rank = 6,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 4, PercentUsed = 0.40, Rank = 10,},
            }
        };

        public static ItemRank SinSeekers = new ItemRank

        {
            Item = Legendary.SinSeekers,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 15, PercentUsed = 1.50, Rank = 7,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
            }
        };

        public static ItemRank Calamity = new ItemRank

        {
            Item = Legendary.Calamity,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 75, PercentUsed = 7.50, Rank = 3,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 19, PercentUsed = 1.90, Rank = 8,},
            }
        };

        public static ItemRank EmimeisDuffel = new ItemRank

        {
            Item = Legendary.EmimeisDuffel,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 9, PercentUsed = 0.90, Rank = 9,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
            }
        };

        public static ItemRank SpinesOfSeethingHatred = new ItemRank

        {
            Item = Legendary.SpinesOfSeethingHatred,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 8, PercentUsed = 0.80, Rank = 10,},
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
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 257, PercentUsed = 25.70, Rank = 1,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 157, PercentUsed = 15.70, Rank = 2,},
            }
        };

        public static ItemRank NatalyasSlayer = new ItemRank

        {
            Item = Legendary.NatalyasSlayer,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 123, PercentUsed = 12.30, Rank = 2,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 68, PercentUsed = 6.80, Rank = 3,},
            }
        };

        public static ItemRank BurizadoKyanon = new ItemRank

        {
            Item = Legendary.BurizadoKyanon,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 63, PercentUsed = 6.30, Rank = 4,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 34, PercentUsed = 3.40, Rank = 4,},
            }
        };

        public static ItemRank UnboundBolt = new ItemRank

        {
            Item = Legendary.UnboundBolt,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 54, PercentUsed = 5.40, Rank = 5,},
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
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 47, PercentUsed = 4.70, Rank = 6,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 555, PercentUsed = 55.50, Rank = 1,},
            }
        };

        public static ItemRank ArcaneBarb = new ItemRank

        {
            Item = Legendary.ArcaneBarb,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 45, PercentUsed = 4.50, Rank = 7,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 24, PercentUsed = 2.40, Rank = 6,},
            }
        };

        public static ItemRank Kridershot = new ItemRank

        {
            Item = Legendary.Kridershot,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 42, PercentUsed = 4.20, Rank = 8,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 27, PercentUsed = 2.70, Rank = 5,},
            }
        };

        public static ItemRank PusSpitter = new ItemRank

        {
            Item = Legendary.PusSpitter,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 35, PercentUsed = 3.50, Rank = 9,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
            }
        };

        public static ItemRank Manticore = new ItemRank

        {
            Item = Legendary.Manticore,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 35, PercentUsed = 3.50, Rank = 10,},
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
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 3, PercentUsed = 0.30, Rank = 10,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 3, PercentUsed = 0.30, Rank = 10,},
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
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 6, PercentUsed = 0.60, Rank = 6,},
            }
        };

        public static ItemRank SaffronWrap = new ItemRank

        {
            Item = Legendary.SaffronWrap,
            HardcoreRank = new List<ItemRankData>
            {
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 1, PercentUsed = 0.10, Rank = 9,},
            }
        };

        public static ItemRank TheWailingHost = new ItemRank

        {
            Item = Legendary.TheWailingHost,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 24, PercentUsed = 2.40, Rank = 6,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 9, PercentUsed = 0.90, Rank = 9,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 6, PercentUsed = 0.60, Rank = 10,},
            }
        };

        public static ItemRank HolyPointShot = new ItemRank

        {
            Item = Legendary.HolyPointShot,
            HardcoreRank = new List<ItemRankData>
            {
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 32, PercentUsed = 3.20, Rank = 3,},
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
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 11, PercentUsed = 1.10, Rank = 7,},
            }
        };

        public static ItemRank TheNinthCirriSatchel = new ItemRank

        {
            Item = Legendary.TheNinthCirriSatchel,
            HardcoreRank = new List<ItemRankData>
            {
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 5, PercentUsed = 0.50, Rank = 8,},
            }
        };

        public static ItemRank TheRavensWing = new ItemRank

        {
            Item = Legendary.TheRavensWing,
            HardcoreRank = new List<ItemRankData>
            {
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 23, PercentUsed = 2.30, Rank = 7,},
            }
        };

        public static ItemRank MaskOfJeram = new ItemRank

        {
            Item = Legendary.MaskOfJeram,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 695, PercentUsed = 70.06, Rank = 1,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 349, PercentUsed = 34.90, Rank = 2,},
            }
        };

        public static ItemRank Quetzalcoatl = new ItemRank

        {
            Item = Legendary.Quetzalcoatl,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 151, PercentUsed = 15.22, Rank = 2,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 374, PercentUsed = 37.40, Rank = 1,},
            }
        };

        public static ItemRank Carnevil = new ItemRank

        {
            Item = Legendary.Carnevil,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 55, PercentUsed = 5.54, Rank = 3,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 53, PercentUsed = 5.30, Rank = 4,},
            }
        };

        public static ItemRank TiklandianVisage = new ItemRank

        {
            Item = Legendary.TiklandianVisage,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 28, PercentUsed = 2.82, Rank = 4,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 193, PercentUsed = 19.30, Rank = 3,},
            }
        };

        public static ItemRank TheGrinReaper = new ItemRank

        {
            Item = Legendary.TheGrinReaper,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 11, PercentUsed = 1.11, Rank = 6,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 2, PercentUsed = 0.20, Rank = 8,},
            }
        };

        public static ItemRank JadeHarvestersWisdom = new ItemRank

        {
            Item = Legendary.JadeHarvestersWisdom,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 10, PercentUsed = 1.01, Rank = 7,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 1, PercentUsed = 0.10, Rank = 10,},
            }
        };

        public static ItemRank ZunimassasVision = new ItemRank

        {
            Item = Legendary.ZunimassasVision,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 9, PercentUsed = 0.91, Rank = 8,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
            }
        };

        public static ItemRank HelltoothMask = new ItemRank

        {
            Item = Legendary.HelltoothMask,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 4, PercentUsed = 0.40, Rank = 9,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 4, PercentUsed = 0.40, Rank = 7,},
            }
        };

        public static ItemRank TheHelmOfRule = new ItemRank

        {
            Item = Legendary.TheHelmOfRule,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 2, PercentUsed = 0.20, Rank = 10,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 7, PercentUsed = 0.70, Rank = 7,},
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
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 185, PercentUsed = 18.65, Rank = 2,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 444, PercentUsed = 44.40, Rank = 1,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 1, PercentUsed = 0.10, Rank = 7,},
            }
        };

        public static ItemRank HelltoothMantle = new ItemRank

        {
            Item = Legendary.HelltoothMantle,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 18, PercentUsed = 1.81, Rank = 4,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 25, PercentUsed = 2.50, Rank = 6,},
            }
        };

        public static ItemRank ZunimassasMarrow = new ItemRank

        {
            Item = Legendary.ZunimassasMarrow,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 378, PercentUsed = 38.10, Rank = 1,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 141, PercentUsed = 14.10, Rank = 3,},
            }
        };

        public static ItemRank JadeHarvestersPeace = new ItemRank

        {
            Item = Legendary.JadeHarvestersPeace,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 165, PercentUsed = 16.63, Rank = 3,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 388, PercentUsed = 38.80, Rank = 1,},
            }
        };

        public static ItemRank HelltoothTunic = new ItemRank

        {
            Item = Legendary.HelltoothTunic,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 11, PercentUsed = 1.11, Rank = 8,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 5, PercentUsed = 0.50, Rank = 9,},
            }
        };

        public static ItemRank JadeHarvestersMercy = new ItemRank

        {
            Item = Legendary.JadeHarvestersMercy,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 212, PercentUsed = 21.37, Rank = 2,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 395, PercentUsed = 39.50, Rank = 2,},
            }
        };

        public static ItemRank HelltoothGauntlets = new ItemRank

        {
            Item = Legendary.HelltoothGauntlets,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 7, PercentUsed = 0.71, Rank = 8,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 6, PercentUsed = 0.60, Rank = 7,},
            }
        };

        public static ItemRank HwojWrap = new ItemRank

        {
            Item = Legendary.HwojWrap,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 87, PercentUsed = 8.77, Rank = 4,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 33, PercentUsed = 3.30, Rank = 7,},
            }
        };

        public static ItemRank JadeHarvestersCourage = new ItemRank

        {
            Item = Legendary.JadeHarvestersCourage,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 189, PercentUsed = 19.05, Rank = 2,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 397, PercentUsed = 39.70, Rank = 1,},
            }
        };

        public static ItemRank HelltoothLegGuards = new ItemRank

        {
            Item = Legendary.HelltoothLegGuards,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 21, PercentUsed = 2.12, Rank = 8,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 10, PercentUsed = 1.00, Rank = 7,},
            }
        };

        public static ItemRank ZunimassasTrail = new ItemRank

        {
            Item = Legendary.ZunimassasTrail,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 368, PercentUsed = 37.10, Rank = 1,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 144, PercentUsed = 14.40, Rank = 3,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 2, PercentUsed = 0.20, Rank = 10,},
            }
        };

        public static ItemRank JadeHarvestersSwiftness = new ItemRank

        {
            Item = Legendary.JadeHarvestersSwiftness,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 176, PercentUsed = 17.74, Rank = 2,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 434, PercentUsed = 43.40, Rank = 1,},
            }
        };

        public static ItemRank HelltoothGreaves = new ItemRank

        {
            Item = Legendary.HelltoothGreaves,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 18, PercentUsed = 1.81, Rank = 8,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 17, PercentUsed = 1.70, Rank = 7,},
            }
        };

        public static ItemRank RondalsLocket = new ItemRank

        {
            Item = Legendary.RondalsLocket,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 37, PercentUsed = 3.73, Rank = 9,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 36, PercentUsed = 3.63, Rank = 10,},
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
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 369, PercentUsed = 37.20, Rank = 2,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 271, PercentUsed = 27.10, Rank = 4,},
            }
        };

        public static ItemRank ZunimassasPox = new ItemRank

        {
            Item = Legendary.ZunimassasPox,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 111, PercentUsed = 11.19, Rank = 5,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 26, PercentUsed = 2.60, Rank = 7,},
            }
        };

        public static ItemRank UhkapianSerpent = new ItemRank

        {
            Item = Legendary.UhkapianSerpent,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 522, PercentUsed = 52.62, Rank = 1,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 244, PercentUsed = 24.40, Rank = 1,},
            }
        };

        public static ItemRank ZunimassasStringOfSkulls = new ItemRank

        {
            Item = Legendary.ZunimassasStringOfSkulls,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 286, PercentUsed = 28.83, Rank = 2,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 138, PercentUsed = 13.80, Rank = 3,},
            }
        };

        public static ItemRank ThingOfTheDeep = new ItemRank

        {
            Item = Legendary.ThingOfTheDeep,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 99, PercentUsed = 9.98, Rank = 3,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 159, PercentUsed = 15.90, Rank = 2,},
            }
        };

        public static ItemRank Spite = new ItemRank

        {
            Item = Legendary.Spite,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 10, PercentUsed = 1.01, Rank = 4,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
            }
        };

        public static ItemRank ManajumasGoryFetch = new ItemRank

        {
            Item = Legendary.ManajumasGoryFetch,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 9, PercentUsed = 0.91, Rank = 5,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 2, PercentUsed = 0.20, Rank = 9,},
            }
        };

        public static ItemRank Homunculus = new ItemRank

        {
            Item = Legendary.Homunculus,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 9, PercentUsed = 0.91, Rank = 7,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 91, PercentUsed = 9.10, Rank = 4,},
            }
        };

        public static ItemRank ShukranisTriumph = new ItemRank

        {
            Item = Legendary.ShukranisTriumph,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 7, PercentUsed = 0.71, Rank = 8,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 65, PercentUsed = 6.50, Rank = 5,},
            }
        };

        public static ItemRank LidlessWall = new ItemRank

        {
            Item = Legendary.LidlessWall,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 2, PercentUsed = 0.20, Rank = 9,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 44, PercentUsed = 4.43, Rank = 3,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 1, PercentUsed = 0.10, Rank = 10,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 20, PercentUsed = 2.00, Rank = 4,},
            }
        };

        public static ItemRank StarmetalKukri = new ItemRank

        {
            Item = Legendary.StarmetalKukri,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 293, PercentUsed = 29.54, Rank = 1,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 441, PercentUsed = 44.10, Rank = 1,},
            }
        };

        public static ItemRank RhenhoFlayer = new ItemRank

        {
            Item = Legendary.RhenhoFlayer,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 227, PercentUsed = 22.88, Rank = 2,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 54, PercentUsed = 5.40, Rank = 4,},
            }
        };

        public static ItemRank TheDaggerOfDarts = new ItemRank

        {
            Item = Legendary.TheDaggerOfDarts,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 52, PercentUsed = 5.24, Rank = 5,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 50, PercentUsed = 5.00, Rank = 5,},
            }
        };

        public static ItemRank ManajumasCarvingKnife = new ItemRank

        {
            Item = Legendary.ManajumasCarvingKnife,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 23, PercentUsed = 2.32, Rank = 8,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
            }
        };

        public static ItemRank LastBreath = new ItemRank

        {
            Item = Legendary.LastBreath,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 14, PercentUsed = 1.41, Rank = 10,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 4, PercentUsed = 0.40, Rank = 10,},
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
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 93, PercentUsed = 9.30, Rank = 5,},
            }
        };

        public static ItemRank DefenderOfWestmarch = new ItemRank

        {
            Item = Legendary.DefenderOfWestmarch,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 32, PercentUsed = 3.22, Rank = 5,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 5, PercentUsed = 0.50, Rank = 7,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 3, PercentUsed = 0.30, Rank = 7,},
            }
        };

        public static ItemRank GazingDemise = new ItemRank

        {
            Item = Legendary.GazingDemise,
            HardcoreRank = new List<ItemRankData>
            {
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 3, PercentUsed = 0.30, Rank = 8,},
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
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 13, PercentUsed = 1.30, Rank = 7,},
            }
        };

        public static ItemRank FirebirdsPlume = new ItemRank

        {
            Item = Legendary.FirebirdsPlume,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 754, PercentUsed = 75.48, Rank = 1,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 861, PercentUsed = 86.10, Rank = 1,},
            }
        };

        public static ItemRank TalRashasGuiseOfWisdom = new ItemRank

        {
            Item = Legendary.TalRashasGuiseOfWisdom,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 79, PercentUsed = 7.91, Rank = 2,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 19, PercentUsed = 1.90, Rank = 4,},
            }
        };

        public static ItemRank DarkMagesShade = new ItemRank

        {
            Item = Legendary.DarkMagesShade,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 6, PercentUsed = 0.60, Rank = 7,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
            }
        };

        public static ItemRank VelvetCamaral = new ItemRank

        {
            Item = Legendary.VelvetCamaral,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 6, PercentUsed = 0.60, Rank = 8,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 3, PercentUsed = 0.30, Rank = 8,},
            }
        };

        public static ItemRank FirebirdsPinions = new ItemRank

        {
            Item = Legendary.FirebirdsPinions,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 699, PercentUsed = 69.97, Rank = 1,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 918, PercentUsed = 91.80, Rank = 1,},
            }
        };

        public static ItemRank FirebirdsBreast = new ItemRank

        {
            Item = Legendary.FirebirdsBreast,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 385, PercentUsed = 38.54, Rank = 2,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 507, PercentUsed = 50.70, Rank = 1,},
            }
        };

        public static ItemRank VyrsAstonishingAura = new ItemRank

        {
            Item = Legendary.VyrsAstonishingAura,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 7, PercentUsed = 0.70, Rank = 8,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 3, PercentUsed = 0.30, Rank = 8,},
            }
        };

        public static ItemRank FirebirdsTalons = new ItemRank

        {
            Item = Legendary.FirebirdsTalons,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 432, PercentUsed = 43.24, Rank = 1,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 731, PercentUsed = 73.10, Rank = 1,},
            }
        };

        public static ItemRank VyrsGraspingGauntlets = new ItemRank

        {
            Item = Legendary.VyrsGraspingGauntlets,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 13, PercentUsed = 1.30, Rank = 7,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 7, PercentUsed = 0.70, Rank = 7,},
            }
        };

        public static ItemRank TalRashasBrace = new ItemRank

        {
            Item = Legendary.TalRashasBrace,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 173, PercentUsed = 17.32, Rank = 2,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 34, PercentUsed = 3.40, Rank = 6,},
            }
        };

        public static ItemRank JangsEnvelopment = new ItemRank

        {
            Item = Legendary.JangsEnvelopment,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 20, PercentUsed = 2.00, Rank = 8,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 6, PercentUsed = 0.60, Rank = 10,},
            }
        };

        public static ItemRank FirebirdsDown = new ItemRank

        {
            Item = Legendary.FirebirdsDown,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 792, PercentUsed = 79.28, Rank = 1,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 910, PercentUsed = 91.00, Rank = 1,},
            }
        };

        public static ItemRank VyrsFantasticFinery = new ItemRank

        {
            Item = Legendary.VyrsFantasticFinery,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 18, PercentUsed = 1.80, Rank = 7,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 2, PercentUsed = 0.20, Rank = 9,},
            }
        };

        public static ItemRank SkelonsDeceit = new ItemRank

        {
            Item = Legendary.SkelonsDeceit,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 2, PercentUsed = 0.20, Rank = 10,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
            }
        };

        public static ItemRank FirebirdsTarsi = new ItemRank

        {
            Item = Legendary.FirebirdsTarsi,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 777, PercentUsed = 77.78, Rank = 1,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 909, PercentUsed = 90.90, Rank = 1,},
            }
        };

        public static ItemRank VyrsSwaggeringStance = new ItemRank

        {
            Item = Legendary.VyrsSwaggeringStance,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 16, PercentUsed = 1.60, Rank = 7,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 5, PercentUsed = 0.50, Rank = 9,},
            }
        };

        public static ItemRank TalRashasAllegiance = new ItemRank

        {
            Item = Legendary.TalRashasAllegiance,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 167, PercentUsed = 16.72, Rank = 1,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 60, PercentUsed = 6.00, Rank = 7,},
            }
        };

        public static ItemRank FirebirdsEye = new ItemRank

        {
            Item = Legendary.FirebirdsEye,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 513, PercentUsed = 51.35, Rank = 1,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 325, PercentUsed = 32.50, Rank = 1,},
            }
        };

        public static ItemRank Mirrorball = new ItemRank

        {
            Item = Legendary.Mirrorball,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 83, PercentUsed = 8.31, Rank = 2,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 26, PercentUsed = 2.60, Rank = 2,},
            }
        };

        public static ItemRank TalRashasUnwaveringGlare = new ItemRank

        {
            Item = Legendary.TalRashasUnwaveringGlare,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 50, PercentUsed = 5.01, Rank = 3,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 17, PercentUsed = 1.70, Rank = 3,},
            }
        };

        public static ItemRank Triumvirate = new ItemRank

        {
            Item = Legendary.Triumvirate,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 29, PercentUsed = 2.90, Rank = 4,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 12, PercentUsed = 1.20, Rank = 4,},
            }
        };

        public static ItemRank LightOfGrace = new ItemRank

        {
            Item = Legendary.LightOfGrace,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 10, PercentUsed = 1.00, Rank = 5,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 8, PercentUsed = 0.80, Rank = 5,},
            }
        };

        public static ItemRank MykensBallOfHate = new ItemRank

        {
            Item = Legendary.MykensBallOfHate,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 8, PercentUsed = 0.80, Rank = 6,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 3, PercentUsed = 0.30, Rank = 8,},
            }
        };

        public static ItemRank ChantodosForce = new ItemRank

        {
            Item = Legendary.ChantodosForce,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 4, PercentUsed = 0.40, Rank = 7,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 2, PercentUsed = 0.20, Rank = 9,},
            }
        };

        public static ItemRank WinterFlurry = new ItemRank

        {
            Item = Legendary.WinterFlurry,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 4, PercentUsed = 0.40, Rank = 8,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 4, PercentUsed = 0.40, Rank = 6,},
            }
        };

        public static ItemRank TheOculus = new ItemRank

        {
            Item = Legendary.TheOculus,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 3, PercentUsed = 0.30, Rank = 9,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 3, PercentUsed = 0.30, Rank = 7,},
            }
        };

        public static ItemRank Denial = new ItemRank

        {
            Item = Legendary.Denial,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 1, PercentUsed = 0.10, Rank = 10,},
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
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 218, PercentUsed = 21.82, Rank = 1,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 92, PercentUsed = 9.20, Rank = 3,},
            }
        };

        public static ItemRank WandOfWoh = new ItemRank

        {
            Item = Legendary.WandOfWoh,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 25, PercentUsed = 2.50, Rank = 8,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 57, PercentUsed = 5.70, Rank = 4,},
            }
        };

        public static ItemRank SloraksMadness = new ItemRank

        {
            Item = Legendary.SloraksMadness,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 20, PercentUsed = 2.00, Rank = 10,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 9, PercentUsed = 0.90, Rank = 9,},
            }
        };

        public static ItemRank TheMagistrate = new ItemRank

        {
            Item = Legendary.TheMagistrate,
            HardcoreRank = new List<ItemRankData>
            {
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 6, PercentUsed = 0.60, Rank = 6,},
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
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 4, PercentUsed = 0.40, Rank = 7,},
            }
        };

        public static ItemRank DemonsMarrow = new ItemRank

        {
            Item = Legendary.DemonsMarrow,
            HardcoreRank = new List<ItemRankData>
            {
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 1, PercentUsed = 0.10, Rank = 10,},
            }
        };

        public static ItemRank Rimeheart = new ItemRank

        {
            Item = Legendary.Rimeheart,
            HardcoreRank = new List<ItemRankData>
            {
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 12, PercentUsed = 1.20, Rank = 6,},
            }
        };

        public static ItemRank GestureOfOrpheus = new ItemRank

        {
            Item = Legendary.GestureOfOrpheus,
            HardcoreRank = new List<ItemRankData>
            {
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 8, PercentUsed = 0.80, Rank = 10,},
            }
        };

        public static ItemRank HelmOfAkkhan = new ItemRank

        {
            Item = Legendary.HelmOfAkkhan,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 466, PercentUsed = 46.93, Rank = 1,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 407, PercentUsed = 40.70, Rank = 2,},
            }
        };

        public static ItemRank RolandsVisage = new ItemRank

        {
            Item = Legendary.RolandsVisage,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 50, PercentUsed = 5.04, Rank = 3,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 22, PercentUsed = 2.20, Rank = 3,},
            }
        };

        public static ItemRank PauldronsOfAkkhan = new ItemRank

        {
            Item = Legendary.PauldronsOfAkkhan,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 824, PercentUsed = 82.98, Rank = 1,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 828, PercentUsed = 82.80, Rank = 1,},
            }
        };

        public static ItemRank RolandsMantle = new ItemRank

        {
            Item = Legendary.RolandsMantle,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 56, PercentUsed = 5.64, Rank = 2,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 26, PercentUsed = 2.60, Rank = 4,},
            }
        };

        public static ItemRank BreastplateOfAkkhan = new ItemRank

        {
            Item = Legendary.BreastplateOfAkkhan,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 786, PercentUsed = 79.15, Rank = 1,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 815, PercentUsed = 81.50, Rank = 1,},
            }
        };

        public static ItemRank RolandsBearing = new ItemRank

        {
            Item = Legendary.RolandsBearing,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 45, PercentUsed = 4.53, Rank = 3,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 18, PercentUsed = 1.80, Rank = 4,},
            }
        };

        public static ItemRank ArmorOfTheKindRegent = new ItemRank

        {
            Item = Legendary.ArmorOfTheKindRegent,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 4, PercentUsed = 0.40, Rank = 7,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
            }
        };

        public static ItemRank AquilaCuirass = new ItemRank

        {
            Item = Legendary.AquilaCuirass,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 1, PercentUsed = 0.10, Rank = 9,},
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
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 799, PercentUsed = 80.46, Rank = 1,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 803, PercentUsed = 80.30, Rank = 1,},
            }
        };

        public static ItemRank RolandsGrasp = new ItemRank

        {
            Item = Legendary.RolandsGrasp,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 61, PercentUsed = 6.14, Rank = 2,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 25, PercentUsed = 2.50, Rank = 4,},
            }
        };

        public static ItemRank StoneGauntlets = new ItemRank

        {
            Item = Legendary.StoneGauntlets,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 1, PercentUsed = 0.10, Rank = 9,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 1, PercentUsed = 0.10, Rank = 10,},
            }
        };

        public static ItemRank AngelHairBraid = new ItemRank

        {
            Item = Legendary.AngelHairBraid,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 25, PercentUsed = 2.52, Rank = 7,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 5, PercentUsed = 0.50, Rank = 9,},
            }
        };

        public static ItemRank CuissesOfAkkhan = new ItemRank

        {
            Item = Legendary.CuissesOfAkkhan,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 681, PercentUsed = 68.58, Rank = 1,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 799, PercentUsed = 79.90, Rank = 1,},
            }
        };

        public static ItemRank RolandsDetermination = new ItemRank

        {
            Item = Legendary.RolandsDetermination,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 46, PercentUsed = 4.63, Rank = 3,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 22, PercentUsed = 2.20, Rank = 4,},
            }
        };

        public static ItemRank SabatonsOfAkkhan = new ItemRank

        {
            Item = Legendary.SabatonsOfAkkhan,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 683, PercentUsed = 68.78, Rank = 1,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 817, PercentUsed = 81.70, Rank = 1,},
            }
        };

        public static ItemRank RolandsStride = new ItemRank

        {
            Item = Legendary.RolandsStride,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 50, PercentUsed = 5.04, Rank = 3,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 25, PercentUsed = 2.50, Rank = 4,},
            }
        };

        public static ItemRank EternalUnion = new ItemRank

        {
            Item = Legendary.EternalUnion,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 69, PercentUsed = 6.95, Rank = 4,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 25, PercentUsed = 2.50, Rank = 9,},
            }
        };

        public static ItemRank StolenRing = new ItemRank

        {
            Item = Legendary.StolenRing,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 11, PercentUsed = 1.11, Rank = 10,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
            }
        };

        public static ItemRank UnrelentingPhalanx = new ItemRank

        {
            Item = Legendary.UnrelentingPhalanx,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 439, PercentUsed = 44.21, Rank = 1,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 731, PercentUsed = 73.10, Rank = 1,},
            }
        };

        public static ItemRank Hellskull = new ItemRank

        {
            Item = Legendary.Hellskull,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 281, PercentUsed = 28.30, Rank = 2,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 88, PercentUsed = 8.80, Rank = 3,},
            }
        };

        public static ItemRank PiroMarella = new ItemRank

        {
            Item = Legendary.PiroMarella,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 41, PercentUsed = 4.13, Rank = 4,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 8, PercentUsed = 0.80, Rank = 5,},
            }
        };

        public static ItemRank HallowedBarricade = new ItemRank

        {
            Item = Legendary.HallowedBarricade,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 31, PercentUsed = 3.12, Rank = 6,},
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
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 30, PercentUsed = 3.02, Rank = 7,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 135, PercentUsed = 13.50, Rank = 2,},
            }
        };

        public static ItemRank Jekangbord = new ItemRank

        {
            Item = Legendary.Jekangbord,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 25, PercentUsed = 2.52, Rank = 8,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 3, PercentUsed = 0.30, Rank = 6,},
            }
        };

        public static ItemRank EberliCharo = new ItemRank

        {
            Item = Legendary.EberliCharo,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 20, PercentUsed = 2.01, Rank = 9,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
            }
        };

        public static ItemRank FateOfTheFell = new ItemRank

        {
            Item = Legendary.FateOfTheFell,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 302, PercentUsed = 30.41, Rank = 1,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 75, PercentUsed = 7.50, Rank = 3,},
            }
        };

        public static ItemRank BalefulRemnant = new ItemRank

        {
            Item = Legendary.BalefulRemnant,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 110, PercentUsed = 11.08, Rank = 4,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 16, PercentUsed = 1.60, Rank = 8,},
            }
        };

        public static ItemRank GoldenFlense = new ItemRank

        {
            Item = Legendary.GoldenFlense,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 34, PercentUsed = 3.42, Rank = 6,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 14, PercentUsed = 1.40, Rank = 9,},
            }
        };

        public static ItemRank BladeOfProphecy = new ItemRank

        {
            Item = Legendary.BladeOfProphecy,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 25, PercentUsed = 2.52, Rank = 7,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 11, PercentUsed = 1.10, Rank = 10,},
            }
        };

        public static ItemRank Darklight = new ItemRank

        {
            Item = Legendary.Darklight,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 22, PercentUsed = 2.22, Rank = 8,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
            }
        };

        public static ItemRank GyrfalconsFoote = new ItemRank

        {
            Item = Legendary.GyrfalconsFoote,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 16, PercentUsed = 1.61, Rank = 9,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
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
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 1, PercentUsed = 0.10, Rank = 8,},
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
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 1, PercentUsed = 0.10, Rank = 9,},
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
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 51, PercentUsed = 5.10, Rank = 7,},
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
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 27, PercentUsed = 2.70, Rank = 7,},
            }
        };

        public static ItemRank Salvation = new ItemRank

        {
            Item = Legendary.Salvation,
            HardcoreRank = new List<ItemRankData>
            {
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 1, PercentUsed = 0.10, Rank = 8,},
            }
        };

        public static ItemRank AkaratsAwakening = new ItemRank

        {
            Item = Legendary.AkaratsAwakening,
            HardcoreRank = new List<ItemRankData>
            {
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 1, PercentUsed = 0.10, Rank = 9,},
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
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 1, PercentUsed = 0.10, Rank = 10,},
            }
        };

        public static ItemRank Swiftmount = new ItemRank

        {
            Item = Legendary.Swiftmount,
            HardcoreRank = new List<ItemRankData>
            {
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 68, PercentUsed = 6.80, Rank = 4,},
            }
        };

        public static ItemRank InviolableFaith = new ItemRank

        {
            Item = Legendary.InviolableFaith,
            HardcoreRank = new List<ItemRankData>
            {
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 22, PercentUsed = 2.20, Rank = 6,},
            }
        };
    }
}