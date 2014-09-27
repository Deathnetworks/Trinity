using System;
using System.Collections.Generic;
using System.Linq;
using Trinity.Cache;
using Trinity.Helpers;
using Trinity.Objects;
using Trinity.Reference;
using Zeta.Game;
using Zeta.Game.Internals;
using Zeta.Game.Internals.Actors;

namespace Trinity.Reference
{

    public class ItemRanks : FieldCollection<ItemRanks, ItemRank>
    {
        #region Methods

        private static HashSet<int> _itemIds;
        public static HashSet<int> ItemIds
        {
            get 
            { 
                return _itemIds ?? (_itemIds = new HashSet<int>(ToList()
                    .Select(i => i.Item.Id))); 
            }
        }

        public static IEnumerable<ItemRank> GetRankedItems(ActorClass actorClass, int minPercent = 10, double minSampleSize = 10, int betterThanRank = 5)
        {
            return ToList().Where(i => i.SoftcoreRank.Any(ird =>
                ird.Rank < betterThanRank &&
                ird.Class == actorClass &&
                ird.PercentUsed >= minPercent &&
                ird.SampleSize >= minSampleSize));
        }

        public static HashSet<int> GetRankedIds(ActorClass actorClass, int minPercent = 10, double minSampleSize = 10, int betterThanRank = 5)
        {
            return new HashSet<int>(GetRankedItems(actorClass, minPercent, minSampleSize, betterThanRank).Select(v => v.Item.Id));
        }

        private static Dictionary<int,ItemRank> _itemRankDictionary = new Dictionary<int, ItemRank>();
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

                var list = ToList();

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

        // AUTO-GENERATED on Thu, 25 Sep 2014 02:22:00 GMT

        public static ItemRank AughildsSearch = new ItemRank

        {
            Item = Legendary.AughildsSearch,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 473, PercentUsed = 48.61, Rank = 1,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 354, PercentUsed = 36.80, Rank = 1,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 135, PercentUsed = 13.73, Rank = 2,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 649, PercentUsed = 66.29, Rank = 1,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 424, PercentUsed = 43.49, Rank = 1,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 98, PercentUsed = 10.04, Rank = 2,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 705, PercentUsed = 70.85, Rank = 1,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 119, PercentUsed = 11.95, Rank = 2,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 24, PercentUsed = 2.43, Rank = 9,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 393, PercentUsed = 39.70, Rank = 2,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 67, PercentUsed = 6.72, Rank = 2,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 25, PercentUsed = 2.52, Rank = 3,},
            }
        };

        public static ItemRank ReapersWraps = new ItemRank

        {
            Item = Legendary.ReapersWraps,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 198, PercentUsed = 20.35, Rank = 2,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 180, PercentUsed = 18.71, Rank = 3,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 469, PercentUsed = 47.71, Rank = 1,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 26, PercentUsed = 2.66, Rank = 3,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 113, PercentUsed = 11.59, Rank = 3,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 694, PercentUsed = 71.11, Rank = 1,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 73, PercentUsed = 7.34, Rank = 3,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 24, PercentUsed = 2.41, Rank = 4,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 173, PercentUsed = 17.49, Rank = 2,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 15, PercentUsed = 1.52, Rank = 7,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 16, PercentUsed = 1.60, Rank = 6,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 259, PercentUsed = 26.06, Rank = 2,},
            }
        };

        public static ItemRank StrongarmBracers = new ItemRank

        {
            Item = Legendary.StrongarmBracers,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 119, PercentUsed = 12.23, Rank = 3,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 240, PercentUsed = 24.95, Rank = 2,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 63, PercentUsed = 6.41, Rank = 4,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 226, PercentUsed = 23.08, Rank = 2,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 251, PercentUsed = 25.74, Rank = 2,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 89, PercentUsed = 9.12, Rank = 3,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 105, PercentUsed = 10.55, Rank = 2,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 707, PercentUsed = 70.98, Rank = 1,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 85, PercentUsed = 8.59, Rank = 6,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 480, PercentUsed = 48.48, Rank = 1,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 811, PercentUsed = 81.34, Rank = 1,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 630, PercentUsed = 63.38, Rank = 1,},
            }
        };

        public static ItemRank WarzechianArmguards = new ItemRank

        {
            Item = Legendary.WarzechianArmguards,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 29, PercentUsed = 2.98, Rank = 4,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 17, PercentUsed = 1.77, Rank = 7,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 28, PercentUsed = 2.85, Rank = 8,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 23, PercentUsed = 2.35, Rank = 4,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 25, PercentUsed = 2.56, Rank = 6,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 12, PercentUsed = 1.23, Rank = 5,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 25, PercentUsed = 2.51, Rank = 5,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 11, PercentUsed = 1.10, Rank = 5,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 29, PercentUsed = 2.93, Rank = 8,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 25, PercentUsed = 2.53, Rank = 4,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 25, PercentUsed = 2.51, Rank = 3,},
            }
        };

        public static ItemRank CusterianWristguards = new ItemRank

        {
            Item = Legendary.CusterianWristguards,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 20, PercentUsed = 2.06, Rank = 5,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 16, PercentUsed = 1.66, Rank = 8,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 33, PercentUsed = 3.36, Rank = 6,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 10, PercentUsed = 1.02, Rank = 6,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 24, PercentUsed = 2.46, Rank = 7,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 8, PercentUsed = 0.82, Rank = 7,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 28, PercentUsed = 2.81, Rank = 4,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 11, PercentUsed = 1.10, Rank = 7,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 91, PercentUsed = 9.20, Rank = 4,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 26, PercentUsed = 2.63, Rank = 3,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 19, PercentUsed = 1.91, Rank = 5,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 24, PercentUsed = 2.41, Rank = 4,},
            }
        };

        public static ItemRank NemesisBracers = new ItemRank

        {
            Item = Legendary.NemesisBracers,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 19, PercentUsed = 1.95, Rank = 6,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 20, PercentUsed = 2.08, Rank = 5,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 31, PercentUsed = 3.15, Rank = 7,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 15, PercentUsed = 1.53, Rank = 5,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 26, PercentUsed = 2.67, Rank = 5,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 14, PercentUsed = 1.43, Rank = 4,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 10, PercentUsed = 1.01, Rank = 7,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 11, PercentUsed = 1.10, Rank = 6,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 62, PercentUsed = 6.27, Rank = 7,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 17, PercentUsed = 1.72, Rank = 5,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 7, PercentUsed = 0.70, Rank = 8,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 15, PercentUsed = 1.51, Rank = 5,},
            }
        };

        public static ItemRank SlaveBonds = new ItemRank

        {
            Item = Legendary.SlaveBonds,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 18, PercentUsed = 1.85, Rank = 7,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 19, PercentUsed = 1.98, Rank = 6,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 97, PercentUsed = 9.87, Rank = 3,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 27, PercentUsed = 2.77, Rank = 4,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 12, PercentUsed = 1.23, Rank = 6,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 10, PercentUsed = 1.01, Rank = 8,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 6, PercentUsed = 0.60, Rank = 10,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 315, PercentUsed = 31.85, Rank = 1,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 3, PercentUsed = 0.30, Rank = 8,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 22, PercentUsed = 2.21, Rank = 4,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 7, PercentUsed = 0.70, Rank = 8,},
            }
        };

        public static ItemRank SanguinaryVambraces = new ItemRank

        {
            Item = Legendary.SanguinaryVambraces,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 13, PercentUsed = 1.34, Rank = 8,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 12, PercentUsed = 1.25, Rank = 9,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 4, PercentUsed = 0.41, Rank = 7,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 13, PercentUsed = 1.33, Rank = 8,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 4, PercentUsed = 0.41, Rank = 10,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 2, PercentUsed = 0.20, Rank = 9,},
            }
        };

        public static ItemRank AncientParthanDefenders = new ItemRank

        {
            Item = Legendary.AncientParthanDefenders,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 9, PercentUsed = 0.92, Rank = 9,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 37, PercentUsed = 3.85, Rank = 4,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 66, PercentUsed = 6.63, Rank = 3,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 3, PercentUsed = 0.30, Rank = 10,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 2, PercentUsed = 0.20, Rank = 10,},
            }
        };

        public static ItemRank PromiseOfGlory = new ItemRank

        {
            Item = Legendary.PromiseOfGlory,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 8, PercentUsed = 0.82, Rank = 10,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 12, PercentUsed = 1.23, Rank = 9,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 7, PercentUsed = 0.72, Rank = 8,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 4, PercentUsed = 0.40, Rank = 10,},
            }
        };

        public static ItemRank SunwukosCrown = new ItemRank

        {
            Item = Legendary.SunwukosCrown,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 283, PercentUsed = 29.09, Rank = 1,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 492, PercentUsed = 49.45, Rank = 1,},
            }
        };

        public static ItemRank MaskOfTheSearingSky = new ItemRank

        {
            Item = Legendary.MaskOfTheSearingSky,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 140, PercentUsed = 14.39, Rank = 2,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 82, PercentUsed = 8.24, Rank = 3,},
            }
        };

        public static ItemRank TheEyeOfTheStorm = new ItemRank

        {
            Item = Legendary.TheEyeOfTheStorm,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 109, PercentUsed = 11.20, Rank = 3,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 47, PercentUsed = 4.72, Rank = 5,},
            }
        };

        public static ItemRank InnasRadiance = new ItemRank

        {
            Item = Legendary.InnasRadiance,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 97, PercentUsed = 9.97, Rank = 4,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 19, PercentUsed = 1.91, Rank = 7,},
            }
        };

        public static ItemRank AndarielsVisage = new ItemRank

        {
            Item = Legendary.AndarielsVisage,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 52, PercentUsed = 5.34, Rank = 5,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 72, PercentUsed = 7.48, Rank = 4,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 32, PercentUsed = 3.26, Rank = 3,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 13, PercentUsed = 1.33, Rank = 4,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 65, PercentUsed = 6.67, Rank = 4,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 51, PercentUsed = 5.23, Rank = 3,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 24, PercentUsed = 2.41, Rank = 6,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 34, PercentUsed = 3.41, Rank = 5,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 22, PercentUsed = 2.22, Rank = 3,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 3, PercentUsed = 0.30, Rank = 6,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 27, PercentUsed = 2.71, Rank = 4,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 23, PercentUsed = 2.31, Rank = 4,},
            }
        };

        public static ItemRank LeoricsCrown = new ItemRank

        {
            Item = Legendary.LeoricsCrown,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 45, PercentUsed = 4.62, Rank = 6,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 49, PercentUsed = 5.09, Rank = 6,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 15, PercentUsed = 1.53, Rank = 6,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 23, PercentUsed = 2.36, Rank = 5,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 163, PercentUsed = 16.70, Rank = 2,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 55, PercentUsed = 5.53, Rank = 4,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 534, PercentUsed = 53.61, Rank = 1,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 3, PercentUsed = 0.30, Rank = 5,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 1, PercentUsed = 0.10, Rank = 8,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 35, PercentUsed = 3.51, Rank = 2,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 478, PercentUsed = 48.09, Rank = 1,},
            }
        };

        public static ItemRank AughildsSpike = new ItemRank

        {
            Item = Legendary.AughildsSpike,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 44, PercentUsed = 4.52, Rank = 7,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 57, PercentUsed = 5.93, Rank = 5,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 26, PercentUsed = 2.64, Rank = 5,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 84, PercentUsed = 8.62, Rank = 3,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 32, PercentUsed = 3.28, Rank = 5,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 9, PercentUsed = 0.90, Rank = 7,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 1, PercentUsed = 0.10, Rank = 7,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 22, PercentUsed = 2.21, Rank = 5,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 3, PercentUsed = 0.30, Rank = 5,},
            }
        };

        public static ItemRank GyanaNaKashu = new ItemRank

        {
            Item = Legendary.GyanaNaKashu,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 31, PercentUsed = 3.19, Rank = 8,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 16, PercentUsed = 1.61, Rank = 8,},
            }
        };

        public static ItemRank EyeOfPeshkov = new ItemRank

        {
            Item = Legendary.EyeOfPeshkov,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 29, PercentUsed = 2.98, Rank = 9,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 222, PercentUsed = 22.31, Rank = 2,},
            }
        };

        public static ItemRank CainsInsight = new ItemRank

        {
            Item = Legendary.CainsInsight,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 19, PercentUsed = 1.95, Rank = 10,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 9, PercentUsed = 0.94, Rank = 10,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 2, PercentUsed = 0.20, Rank = 10,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 10, PercentUsed = 1.03, Rank = 9,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 21, PercentUsed = 2.15, Rank = 6,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 1, PercentUsed = 0.10, Rank = 9,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 1, PercentUsed = 0.10, Rank = 7,},
            }
        };

        public static ItemRank SunwukosBalance = new ItemRank

        {
            Item = Legendary.SunwukosBalance,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 360, PercentUsed = 37.00, Rank = 1,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 761, PercentUsed = 76.48, Rank = 1,},
            }
        };

        public static ItemRank AughildsPower = new ItemRank

        {
            Item = Legendary.AughildsPower,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 258, PercentUsed = 26.52, Rank = 2,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 307, PercentUsed = 31.91, Rank = 1,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 110, PercentUsed = 11.19, Rank = 2,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 662, PercentUsed = 67.62, Rank = 1,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 333, PercentUsed = 34.15, Rank = 2,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 68, PercentUsed = 6.97, Rank = 2,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 58, PercentUsed = 5.83, Rank = 3,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 87, PercentUsed = 8.73, Rank = 3,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 22, PercentUsed = 2.22, Rank = 2,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 432, PercentUsed = 43.64, Rank = 2,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 73, PercentUsed = 7.32, Rank = 2,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 19, PercentUsed = 1.91, Rank = 3,},
            }
        };

        public static ItemRank MantleOfTheUpsidedownSinners = new ItemRank

        {
            Item = Legendary.MantleOfTheUpsidedownSinners,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 194, PercentUsed = 19.94, Rank = 3,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 127, PercentUsed = 12.76, Rank = 2,},
            }
        };

        public static ItemRank AshearasCustodian = new ItemRank

        {
            Item = Legendary.AshearasCustodian,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 50, PercentUsed = 5.14, Rank = 4,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 54, PercentUsed = 5.61, Rank = 5,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 12, PercentUsed = 1.22, Rank = 3,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 20, PercentUsed = 2.04, Rank = 3,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 24, PercentUsed = 2.46, Rank = 4,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 26, PercentUsed = 2.66, Rank = 4,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 8, PercentUsed = 0.80, Rank = 5,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 3, PercentUsed = 0.30, Rank = 10,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 2, PercentUsed = 0.20, Rank = 5,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 4, PercentUsed = 0.40, Rank = 7,},
            }
        };

        public static ItemRank BornsPrivilege = new ItemRank

        {
            Item = Legendary.BornsPrivilege,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 18, PercentUsed = 1.85, Rank = 5,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 29, PercentUsed = 2.97, Rank = 3,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 8, PercentUsed = 0.82, Rank = 5,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 26, PercentUsed = 2.61, Rank = 4,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 9, PercentUsed = 0.90, Rank = 5,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 10, PercentUsed = 1.01, Rank = 5,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 11, PercentUsed = 1.10, Rank = 3,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 1, PercentUsed = 0.10, Rank = 5,},
            }
        };

        public static ItemRank SpauldersOfZakara = new ItemRank

        {
            Item = Legendary.SpauldersOfZakara,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 15, PercentUsed = 1.54, Rank = 6,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 12, PercentUsed = 1.25, Rank = 8,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 8, PercentUsed = 0.81, Rank = 7,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 6, PercentUsed = 0.61, Rank = 8,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 8, PercentUsed = 0.82, Rank = 7,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 6, PercentUsed = 0.61, Rank = 8,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 5, PercentUsed = 0.50, Rank = 7,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 3, PercentUsed = 0.30, Rank = 4,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 2, PercentUsed = 0.20, Rank = 9,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 2, PercentUsed = 0.20, Rank = 4,},
            }
        };

        public static ItemRank DeathWatchMantle = new ItemRank

        {
            Item = Legendary.DeathWatchMantle,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 12, PercentUsed = 1.23, Rank = 7,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 11, PercentUsed = 1.14, Rank = 9,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 8, PercentUsed = 0.81, Rank = 5,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 10, PercentUsed = 1.03, Rank = 6,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 3, PercentUsed = 0.31, Rank = 10,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 1, PercentUsed = 0.10, Rank = 9,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 5, PercentUsed = 0.50, Rank = 6,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 1, PercentUsed = 0.10, Rank = 7,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 2, PercentUsed = 0.20, Rank = 8,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 1, PercentUsed = 0.10, Rank = 6,},
            }
        };

        public static ItemRank PauldronsOfTheSkeletonKing = new ItemRank

        {
            Item = Legendary.PauldronsOfTheSkeletonKing,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 12, PercentUsed = 1.23, Rank = 8,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 11, PercentUsed = 1.14, Rank = 10,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 8, PercentUsed = 0.81, Rank = 6,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 9, PercentUsed = 0.92, Rank = 5,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 15, PercentUsed = 1.54, Rank = 5,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 6, PercentUsed = 0.61, Rank = 6,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 3, PercentUsed = 0.30, Rank = 7,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 12, PercentUsed = 1.21, Rank = 4,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 3, PercentUsed = 0.30, Rank = 4,},
            }
        };

        public static ItemRank HomingPads = new ItemRank

        {
            Item = Legendary.HomingPads,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 9, PercentUsed = 0.92, Rank = 9,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 16, PercentUsed = 1.66, Rank = 7,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 11, PercentUsed = 1.12, Rank = 4,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 8, PercentUsed = 0.82, Rank = 6,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 7, PercentUsed = 0.72, Rank = 8,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 6, PercentUsed = 0.61, Rank = 7,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 6, PercentUsed = 0.61, Rank = 3,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 1, PercentUsed = 0.10, Rank = 10,},
            }
        };

        public static ItemRank ProfanePauldrons = new ItemRank

        {
            Item = Legendary.ProfanePauldrons,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 8, PercentUsed = 0.82, Rank = 10,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 3, PercentUsed = 0.31, Rank = 9,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 7, PercentUsed = 0.72, Rank = 7,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 5, PercentUsed = 0.51, Rank = 9,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 4, PercentUsed = 0.41, Rank = 9,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 3, PercentUsed = 0.30, Rank = 6,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 3, PercentUsed = 0.30, Rank = 9,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 1, PercentUsed = 0.10, Rank = 8,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 19, PercentUsed = 1.92, Rank = 3,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 1, PercentUsed = 0.10, Rank = 9,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 1, PercentUsed = 0.10, Rank = 7,},
            }
        };

        public static ItemRank AughildsRule = new ItemRank

        {
            Item = Legendary.AughildsRule,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 281, PercentUsed = 28.88, Rank = 1,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 60, PercentUsed = 6.24, Rank = 5,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 16, PercentUsed = 1.63, Rank = 5,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 224, PercentUsed = 22.88, Rank = 2,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 47, PercentUsed = 4.82, Rank = 4,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 35, PercentUsed = 3.59, Rank = 4,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 649, PercentUsed = 65.23, Rank = 1,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 56, PercentUsed = 5.62, Rank = 4,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 1, PercentUsed = 0.10, Rank = 5,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 266, PercentUsed = 26.87, Rank = 2,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 7, PercentUsed = 0.70, Rank = 5,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 6, PercentUsed = 0.60, Rank = 5,},
            }
        };

        public static ItemRank InnasVastExpanse = new ItemRank

        {
            Item = Legendary.InnasVastExpanse,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 227, PercentUsed = 23.33, Rank = 2,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 98, PercentUsed = 9.85, Rank = 3,},
            }
        };

        public static ItemRank HeartOfTheCrashingWave = new ItemRank

        {
            Item = Legendary.HeartOfTheCrashingWave,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 180, PercentUsed = 18.50, Rank = 3,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 125, PercentUsed = 12.56, Rank = 2,},
            }
        };

        public static ItemRank BlackthornesSurcoat = new ItemRank

        {
            Item = Legendary.BlackthornesSurcoat,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 147, PercentUsed = 15.11, Rank = 4,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 102, PercentUsed = 10.60, Rank = 4,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 7, PercentUsed = 0.71, Rank = 7,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 129, PercentUsed = 13.18, Rank = 4,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 47, PercentUsed = 4.82, Rank = 5,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 45, PercentUsed = 4.61, Rank = 3,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 19, PercentUsed = 1.91, Rank = 7,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 22, PercentUsed = 2.21, Rank = 5,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 1, PercentUsed = 0.10, Rank = 6,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 23, PercentUsed = 2.32, Rank = 5,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 5, PercentUsed = 0.50, Rank = 7,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 4, PercentUsed = 0.40, Rank = 6,},
            }
        };

        public static ItemRank Cindercoat = new ItemRank

        {
            Item = Legendary.Cindercoat,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 43, PercentUsed = 4.42, Rank = 5,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 133, PercentUsed = 13.83, Rank = 3,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 149, PercentUsed = 15.16, Rank = 2,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 33, PercentUsed = 3.37, Rank = 5,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 280, PercentUsed = 28.72, Rank = 2,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 88, PercentUsed = 9.02, Rank = 2,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 28, PercentUsed = 2.81, Rank = 4,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 100, PercentUsed = 10.04, Rank = 3,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 72, PercentUsed = 7.28, Rank = 2,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 20, PercentUsed = 2.02, Rank = 6,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 329, PercentUsed = 33.00, Rank = 2,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 26, PercentUsed = 2.62, Rank = 2,},
            }
        };

        public static ItemRank Goldskin = new ItemRank

        {
            Item = Legendary.Goldskin,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 15, PercentUsed = 1.54, Rank = 6,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 6, PercentUsed = 0.62, Rank = 9,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 9, PercentUsed = 0.92, Rank = 6,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 15, PercentUsed = 1.53, Rank = 6,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 4, PercentUsed = 0.41, Rank = 9,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 6, PercentUsed = 0.61, Rank = 6,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 28, PercentUsed = 2.81, Rank = 5,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 12, PercentUsed = 1.20, Rank = 6,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 14, PercentUsed = 1.42, Rank = 3,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 12, PercentUsed = 1.21, Rank = 7,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 6, PercentUsed = 0.60, Rank = 6,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 7, PercentUsed = 0.70, Rank = 4,},
            }
        };

        public static ItemRank BornsFrozenSoul = new ItemRank

        {
            Item = Legendary.BornsFrozenSoul,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 15, PercentUsed = 1.54, Rank = 7,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 21, PercentUsed = 2.18, Rank = 6,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 11, PercentUsed = 1.13, Rank = 8,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 5, PercentUsed = 0.51, Rank = 7,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 26, PercentUsed = 2.61, Rank = 6,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 12, PercentUsed = 1.20, Rank = 7,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 9, PercentUsed = 0.91, Rank = 8,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 4, PercentUsed = 0.40, Rank = 8,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 1, PercentUsed = 0.10, Rank = 9,},
            }
        };

        public static ItemRank TyraelsMight = new ItemRank

        {
            Item = Legendary.TyraelsMight,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 9, PercentUsed = 0.92, Rank = 8,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 6, PercentUsed = 0.62, Rank = 8,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 3, PercentUsed = 0.31, Rank = 9,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 4, PercentUsed = 0.41, Rank = 8,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 4, PercentUsed = 0.40, Rank = 9,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 8, PercentUsed = 0.80, Rank = 8,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 2, PercentUsed = 0.20, Rank = 9,},
            }
        };

        public static ItemRank MantleOfTheRydraelm = new ItemRank

        {
            Item = Legendary.MantleOfTheRydraelm,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 7, PercentUsed = 0.72, Rank = 9,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
            }
        };

        public static ItemRank Chaingmail = new ItemRank

        {
            Item = Legendary.Chaingmail,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 6, PercentUsed = 0.62, Rank = 10,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 2, PercentUsed = 0.20, Rank = 10,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 3, PercentUsed = 0.31, Rank = 10,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 4, PercentUsed = 0.41, Rank = 9,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 1, PercentUsed = 0.10, Rank = 7,},
            }
        };

        public static ItemRank SunwukosPaws = new ItemRank

        {
            Item = Legendary.SunwukosPaws,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 384, PercentUsed = 39.47, Rank = 1,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 777, PercentUsed = 78.09, Rank = 1,},
            }
        };

        public static ItemRank FistsOfThunder = new ItemRank

        {
            Item = Legendary.FistsOfThunder,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 246, PercentUsed = 25.28, Rank = 2,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 129, PercentUsed = 12.96, Rank = 2,},
            }
        };

        public static ItemRank AshearasWard = new ItemRank

        {
            Item = Legendary.AshearasWard,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 77, PercentUsed = 7.91, Rank = 3,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 27, PercentUsed = 2.81, Rank = 6,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 7, PercentUsed = 0.71, Rank = 6,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 19, PercentUsed = 1.94, Rank = 5,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 38, PercentUsed = 3.90, Rank = 5,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 25, PercentUsed = 2.56, Rank = 4,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 15, PercentUsed = 1.51, Rank = 4,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 8, PercentUsed = 0.81, Rank = 5,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 5, PercentUsed = 0.50, Rank = 7,},
            }
        };

        public static ItemRank Magefist = new ItemRank

        {
            Item = Legendary.Magefist,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 68, PercentUsed = 6.99, Rank = 4,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 93, PercentUsed = 9.67, Rank = 4,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 200, PercentUsed = 20.35, Rank = 3,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 58, PercentUsed = 5.92, Rank = 3,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 340, PercentUsed = 34.87, Rank = 2,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 59, PercentUsed = 6.05, Rank = 2,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 30, PercentUsed = 3.02, Rank = 3,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 27, PercentUsed = 2.71, Rank = 5,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 65, PercentUsed = 6.57, Rank = 2,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 20, PercentUsed = 2.02, Rank = 3,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 211, PercentUsed = 21.16, Rank = 2,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 16, PercentUsed = 1.61, Rank = 3,},
            }
        };

        public static ItemRank CainsScrivener = new ItemRank

        {
            Item = Legendary.CainsScrivener,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 42, PercentUsed = 4.32, Rank = 5,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 10, PercentUsed = 1.04, Rank = 8,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 3, PercentUsed = 0.31, Rank = 8,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 35, PercentUsed = 3.58, Rank = 4,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 14, PercentUsed = 1.44, Rank = 7,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 15, PercentUsed = 1.54, Rank = 5,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 10, PercentUsed = 1.01, Rank = 6,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 4, PercentUsed = 0.40, Rank = 6,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 1, PercentUsed = 0.10, Rank = 5,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 18, PercentUsed = 1.82, Rank = 4,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 2, PercentUsed = 0.20, Rank = 8,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 2, PercentUsed = 0.20, Rank = 6,},
            }
        };

        public static ItemRank GladiatorGauntlets = new ItemRank

        {
            Item = Legendary.GladiatorGauntlets,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 40, PercentUsed = 4.11, Rank = 6,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 12, PercentUsed = 1.22, Rank = 4,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 17, PercentUsed = 1.74, Rank = 6,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 8, PercentUsed = 0.82, Rank = 7,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 5, PercentUsed = 0.50, Rank = 7,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 1, PercentUsed = 0.10, Rank = 6,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 1, PercentUsed = 0.10, Rank = 9,},
            }
        };

        public static ItemRank StArchewsGage = new ItemRank

        {
            Item = Legendary.StArchewsGage,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 11, PercentUsed = 1.13, Rank = 7,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 7, PercentUsed = 0.73, Rank = 10,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 2, PercentUsed = 0.20, Rank = 10,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 4, PercentUsed = 0.41, Rank = 9,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 11, PercentUsed = 1.13, Rank = 8,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 3, PercentUsed = 0.30, Rank = 6,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 1, PercentUsed = 0.10, Rank = 10,},
            }
        };

        public static ItemRank PendersPurchase = new ItemRank

        {
            Item = Legendary.PendersPurchase,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 8, PercentUsed = 0.82, Rank = 8,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 2, PercentUsed = 0.20, Rank = 10,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 5, PercentUsed = 0.50, Rank = 8,},
            }
        };

        public static ItemRank GlovesOfWorship = new ItemRank

        {
            Item = Legendary.GlovesOfWorship,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 8, PercentUsed = 0.82, Rank = 9,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 7, PercentUsed = 0.73, Rank = 9,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 2, PercentUsed = 0.20, Rank = 9,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 7, PercentUsed = 0.72, Rank = 8,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 10, PercentUsed = 1.03, Rank = 9,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 5, PercentUsed = 0.51, Rank = 8,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 2, PercentUsed = 0.20, Rank = 9,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 3, PercentUsed = 0.30, Rank = 7,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 1, PercentUsed = 0.10, Rank = 7,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 1, PercentUsed = 0.10, Rank = 8,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 2, PercentUsed = 0.20, Rank = 9,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 1, PercentUsed = 0.10, Rank = 8,},
            }
        };

        public static ItemRank PrideOfTheInvoker = new ItemRank

        {
            Item = Legendary.PrideOfTheInvoker,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 7, PercentUsed = 0.72, Rank = 10,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 5, PercentUsed = 0.51, Rank = 9,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 1, PercentUsed = 0.10, Rank = 10,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 1, PercentUsed = 0.10, Rank = 9,},
            }
        };

        public static ItemRank ThundergodsVigor = new ItemRank

        {
            Item = Legendary.ThundergodsVigor,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 270, PercentUsed = 27.75, Rank = 1,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 126, PercentUsed = 13.10, Rank = 2,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 22, PercentUsed = 2.24, Rank = 9,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 23, PercentUsed = 2.36, Rank = 10,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 26, PercentUsed = 2.66, Rank = 7,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 119, PercentUsed = 11.96, Rank = 2,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 23, PercentUsed = 2.31, Rank = 7,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 6, PercentUsed = 0.61, Rank = 9,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 8, PercentUsed = 0.80, Rank = 9,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 4, PercentUsed = 0.40, Rank = 10,},
            }
        };

        public static ItemRank InnasFavor = new ItemRank

        {
            Item = Legendary.InnasFavor,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 236, PercentUsed = 24.25, Rank = 2,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 89, PercentUsed = 8.94, Rank = 3,},
            }
        };

        public static ItemRank BlackthornesNotchedBelt = new ItemRank

        {
            Item = Legendary.BlackthornesNotchedBelt,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 137, PercentUsed = 14.08, Rank = 3,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 50, PercentUsed = 5.20, Rank = 5,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 101, PercentUsed = 10.27, Rank = 4,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 266, PercentUsed = 27.17, Rank = 1,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 117, PercentUsed = 12.00, Rank = 3,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 52, PercentUsed = 5.33, Rank = 5,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 76, PercentUsed = 7.64, Rank = 4,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 14, PercentUsed = 1.41, Rank = 10,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 26, PercentUsed = 2.63, Rank = 4,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 113, PercentUsed = 11.41, Rank = 2,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 73, PercentUsed = 7.32, Rank = 3,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 15, PercentUsed = 1.51, Rank = 5,},
            }
        };

        public static ItemRank StringOfEars = new ItemRank

        {
            Item = Legendary.StringOfEars,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 66, PercentUsed = 6.78, Rank = 4,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 20, PercentUsed = 2.08, Rank = 8,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 139, PercentUsed = 14.14, Rank = 3,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 84, PercentUsed = 8.58, Rank = 5,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 162, PercentUsed = 16.62, Rank = 2,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 41, PercentUsed = 4.20, Rank = 6,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 23, PercentUsed = 2.31, Rank = 9,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 11, PercentUsed = 1.11, Rank = 5,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 95, PercentUsed = 9.60, Rank = 4,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 346, PercentUsed = 34.70, Rank = 2,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 10, PercentUsed = 1.01, Rank = 6,},
            }
        };

        public static ItemRank TheWitchingHour = new ItemRank

        {
            Item = Legendary.TheWitchingHour,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 54, PercentUsed = 5.55, Rank = 5,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 22, PercentUsed = 2.29, Rank = 7,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 213, PercentUsed = 21.67, Rank = 1,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 203, PercentUsed = 20.74, Rank = 2,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 116, PercentUsed = 11.90, Rank = 4,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 73, PercentUsed = 7.48, Rank = 4,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 455, PercentUsed = 45.73, Rank = 1,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 40, PercentUsed = 4.02, Rank = 4,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 780, PercentUsed = 78.87, Rank = 1,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 374, PercentUsed = 37.78, Rank = 1,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 394, PercentUsed = 39.52, Rank = 1,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 205, PercentUsed = 20.62, Rank = 2,},
            }
        };

        public static ItemRank CaptainCrimsonsSilkGirdle = new ItemRank

        {
            Item = Legendary.CaptainCrimsonsSilkGirdle,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 45, PercentUsed = 4.62, Rank = 6,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 14, PercentUsed = 1.46, Rank = 9,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 39, PercentUsed = 3.97, Rank = 7,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 50, PercentUsed = 5.13, Rank = 7,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 445, PercentUsed = 45.59, Rank = 1,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 20, PercentUsed = 2.01, Rank = 10,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 9, PercentUsed = 0.91, Rank = 7,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 13, PercentUsed = 1.30, Rank = 7,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 79, PercentUsed = 7.95, Rank = 3,},
            }
        };

        public static ItemRank HellcatWaistguard = new ItemRank

        {
            Item = Legendary.HellcatWaistguard,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 35, PercentUsed = 3.60, Rank = 7,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 65, PercentUsed = 6.61, Rank = 5,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 62, PercentUsed = 6.33, Rank = 6,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 51, PercentUsed = 5.23, Rank = 6,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 17, PercentUsed = 1.74, Rank = 9,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 51, PercentUsed = 5.13, Rank = 5,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 9, PercentUsed = 0.91, Rank = 6,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 27, PercentUsed = 2.73, Rank = 8,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 10, PercentUsed = 1.00, Rank = 8,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 5, PercentUsed = 0.50, Rank = 9,},
            }
        };

        public static ItemRank FleetingStrap = new ItemRank

        {
            Item = Legendary.FleetingStrap,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 23, PercentUsed = 2.36, Rank = 8,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 37, PercentUsed = 3.76, Rank = 8,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 27, PercentUsed = 2.76, Rank = 8,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 23, PercentUsed = 2.36, Rank = 9,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 13, PercentUsed = 1.33, Rank = 10,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 7, PercentUsed = 0.71, Rank = 9,},
            }
        };

        public static ItemRank HarringtonWaistguard = new ItemRank

        {
            Item = Legendary.HarringtonWaistguard,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 20, PercentUsed = 2.06, Rank = 9,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 27, PercentUsed = 2.81, Rank = 6,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 204, PercentUsed = 20.75, Rank = 2,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 134, PercentUsed = 13.69, Rank = 3,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 56, PercentUsed = 5.74, Rank = 5,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 78, PercentUsed = 7.99, Rank = 3,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 23, PercentUsed = 2.31, Rank = 8,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 37, PercentUsed = 3.71, Rank = 5,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 55, PercentUsed = 5.56, Rank = 3,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 62, PercentUsed = 6.26, Rank = 7,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 31, PercentUsed = 3.11, Rank = 6,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 29, PercentUsed = 2.92, Rank = 4,},
            }
        };

        public static ItemRank Goldwrap = new ItemRank

        {
            Item = Legendary.Goldwrap,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 12, PercentUsed = 1.23, Rank = 10,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 47, PercentUsed = 4.78, Rank = 6,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 49, PercentUsed = 5.01, Rank = 7,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 27, PercentUsed = 2.77, Rank = 8,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 27, PercentUsed = 2.71, Rank = 7,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 16, PercentUsed = 1.61, Rank = 9,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 61, PercentUsed = 6.17, Rank = 2,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 66, PercentUsed = 6.67, Rank = 6,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 31, PercentUsed = 3.11, Rank = 5,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 6, PercentUsed = 0.60, Rank = 8,},
            }
        };

        public static ItemRank DepthDiggers = new ItemRank

        {
            Item = Legendary.DepthDiggers,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 210, PercentUsed = 21.58, Rank = 1,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 93, PercentUsed = 9.67, Rank = 5,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 10, PercentUsed = 1.02, Rank = 7,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 6, PercentUsed = 0.62, Rank = 9,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 22, PercentUsed = 2.25, Rank = 6,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 378, PercentUsed = 37.99, Rank = 1,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 22, PercentUsed = 2.21, Rank = 4,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 1, PercentUsed = 0.10, Rank = 8,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 4, PercentUsed = 0.40, Rank = 7,},
            }
        };

        public static ItemRank InnasTemperance = new ItemRank

        {
            Item = Legendary.InnasTemperance,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 195, PercentUsed = 20.04, Rank = 2,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 110, PercentUsed = 11.06, Rank = 3,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 6, PercentUsed = 0.61, Rank = 4,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 4, PercentUsed = 0.40, Rank = 8,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 6, PercentUsed = 0.60, Rank = 5,},
            }
        };

        public static ItemRank ScalesOfTheDancingSerpent = new ItemRank

        {
            Item = Legendary.ScalesOfTheDancingSerpent,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 175, PercentUsed = 17.99, Rank = 3,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 107, PercentUsed = 10.75, Rank = 4,},
            }
        };

        public static ItemRank BlackthornesJoustingMail = new ItemRank

        {
            Item = Legendary.BlackthornesJoustingMail,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 161, PercentUsed = 16.55, Rank = 4,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 103, PercentUsed = 10.71, Rank = 3,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 29, PercentUsed = 2.95, Rank = 4,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 335, PercentUsed = 34.22, Rank = 1,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 74, PercentUsed = 7.59, Rank = 2,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 46, PercentUsed = 4.71, Rank = 3,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 274, PercentUsed = 27.54, Rank = 2,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 19, PercentUsed = 1.91, Rank = 5,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 4, PercentUsed = 0.40, Rank = 5,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 117, PercentUsed = 11.82, Rank = 3,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 5, PercentUsed = 0.50, Rank = 7,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 6, PercentUsed = 0.60, Rank = 6,},
            }
        };

        public static ItemRank CaptainCrimsonsThrust = new ItemRank

        {
            Item = Legendary.CaptainCrimsonsThrust,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 62, PercentUsed = 6.37, Rank = 5,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 15, PercentUsed = 1.56, Rank = 10,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 30, PercentUsed = 3.05, Rank = 3,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 10, PercentUsed = 1.02, Rank = 10,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 59, PercentUsed = 6.05, Rank = 3,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 255, PercentUsed = 26.13, Rank = 2,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 57, PercentUsed = 5.73, Rank = 5,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 4, PercentUsed = 0.40, Rank = 7,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 10, PercentUsed = 1.01, Rank = 3,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 5, PercentUsed = 0.51, Rank = 8,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 22, PercentUsed = 2.21, Rank = 3,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 43, PercentUsed = 4.33, Rank = 2,},
            }
        };

        public static ItemRank AshearasPace = new ItemRank

        {
            Item = Legendary.AshearasPace,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 56, PercentUsed = 5.76, Rank = 6,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 78, PercentUsed = 8.11, Rank = 6,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 22, PercentUsed = 2.24, Rank = 5,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 42, PercentUsed = 4.29, Rank = 6,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 36, PercentUsed = 3.69, Rank = 6,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 23, PercentUsed = 2.36, Rank = 5,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 1, PercentUsed = 0.10, Rank = 10,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 4, PercentUsed = 0.40, Rank = 8,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 4, PercentUsed = 0.40, Rank = 9,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 3, PercentUsed = 0.30, Rank = 9,},
            }
        };

        public static ItemRank CainsHabit = new ItemRank

        {
            Item = Legendary.CainsHabit,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 37, PercentUsed = 3.80, Rank = 7,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 29, PercentUsed = 3.01, Rank = 7,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 10, PercentUsed = 1.02, Rank = 8,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 116, PercentUsed = 11.85, Rank = 3,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 40, PercentUsed = 4.10, Rank = 5,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 15, PercentUsed = 1.54, Rank = 7,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 24, PercentUsed = 2.41, Rank = 7,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 9, PercentUsed = 0.90, Rank = 6,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 4, PercentUsed = 0.40, Rank = 6,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 114, PercentUsed = 11.52, Rank = 4,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 14, PercentUsed = 1.40, Rank = 5,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 3, PercentUsed = 0.30, Rank = 8,},
            }
        };

        public static ItemRank PoxFaulds = new ItemRank

        {
            Item = Legendary.PoxFaulds,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 16, PercentUsed = 1.64, Rank = 8,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 25, PercentUsed = 2.60, Rank = 8,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 11, PercentUsed = 1.12, Rank = 8,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 5, PercentUsed = 0.51, Rank = 10,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 3, PercentUsed = 0.31, Rank = 9,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 2, PercentUsed = 0.20, Rank = 8,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 3, PercentUsed = 0.30, Rank = 9,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 6, PercentUsed = 0.61, Rank = 7,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 2, PercentUsed = 0.20, Rank = 9,},
            }
        };

        public static ItemRank HexingPantsOfMrYan = new ItemRank

        {
            Item = Legendary.HexingPantsOfMrYan,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 14, PercentUsed = 1.44, Rank = 9,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 96, PercentUsed = 9.98, Rank = 4,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 44, PercentUsed = 4.48, Rank = 2,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 81, PercentUsed = 8.27, Rank = 4,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 26, PercentUsed = 2.67, Rank = 7,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 7, PercentUsed = 0.72, Rank = 8,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 32, PercentUsed = 3.22, Rank = 6,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 72, PercentUsed = 7.23, Rank = 3,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 22, PercentUsed = 2.22, Rank = 2,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 149, PercentUsed = 15.05, Rank = 2,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 14, PercentUsed = 1.40, Rank = 4,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 12, PercentUsed = 1.21, Rank = 4,},
            }
        };

        public static ItemRank HammerJammers = new ItemRank

        {
            Item = Legendary.HammerJammers,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 8, PercentUsed = 0.82, Rank = 10,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 7, PercentUsed = 0.71, Rank = 9,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 10, PercentUsed = 1.02, Rank = 9,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 2, PercentUsed = 0.20, Rank = 9,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 3, PercentUsed = 0.30, Rank = 10,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 1, PercentUsed = 0.10, Rank = 10,},
            }
        };

        public static ItemRank EightdemonBoots = new ItemRank

        {
            Item = Legendary.EightdemonBoots,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 220, PercentUsed = 22.61, Rank = 1,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 125, PercentUsed = 12.56, Rank = 4,},
            }
        };

        public static ItemRank IceClimbers = new ItemRank

        {
            Item = Legendary.IceClimbers,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 151, PercentUsed = 15.52, Rank = 2,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 95, PercentUsed = 9.88, Rank = 4,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 15, PercentUsed = 1.53, Rank = 3,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 96, PercentUsed = 9.81, Rank = 4,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 59, PercentUsed = 6.05, Rank = 2,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 22, PercentUsed = 2.25, Rank = 5,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 178, PercentUsed = 17.89, Rank = 3,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 16, PercentUsed = 1.61, Rank = 4,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 2, PercentUsed = 0.20, Rank = 5,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 103, PercentUsed = 10.40, Rank = 4,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 8, PercentUsed = 0.80, Rank = 6,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 6, PercentUsed = 0.60, Rank = 4,},
            }
        };

        public static ItemRank BlackthornesSpurs = new ItemRank

        {
            Item = Legendary.BlackthornesSpurs,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 150, PercentUsed = 15.42, Rank = 3,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 27, PercentUsed = 2.81, Rank = 6,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 10, PercentUsed = 1.02, Rank = 7,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 132, PercentUsed = 13.48, Rank = 3,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 48, PercentUsed = 4.92, Rank = 5,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 18, PercentUsed = 1.84, Rank = 6,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 293, PercentUsed = 29.45, Rank = 1,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 7, PercentUsed = 0.70, Rank = 6,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 1, PercentUsed = 0.10, Rank = 8,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 72, PercentUsed = 7.27, Rank = 5,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 7, PercentUsed = 0.70, Rank = 7,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 2, PercentUsed = 0.20, Rank = 8,},
            }
        };

        public static ItemRank TheCrudestBoots = new ItemRank

        {
            Item = Legendary.TheCrudestBoots,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 123, PercentUsed = 12.64, Rank = 4,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 256, PercentUsed = 25.73, Rank = 2,},
            }
        };

        public static ItemRank CaptainCrimsonsWaders = new ItemRank

        {
            Item = Legendary.CaptainCrimsonsWaders,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 69, PercentUsed = 7.09, Rank = 5,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 7, PercentUsed = 0.73, Rank = 10,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 11, PercentUsed = 1.12, Rank = 6,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 7, PercentUsed = 0.72, Rank = 10,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 54, PercentUsed = 5.54, Rank = 3,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 204, PercentUsed = 20.90, Rank = 2,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 59, PercentUsed = 5.93, Rank = 5,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 2, PercentUsed = 0.20, Rank = 9,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 1, PercentUsed = 0.10, Rank = 9,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 3, PercentUsed = 0.30, Rank = 10,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 20, PercentUsed = 2.01, Rank = 3,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 36, PercentUsed = 3.62, Rank = 2,},
            }
        };

        public static ItemRank AshearasFinders = new ItemRank

        {
            Item = Legendary.AshearasFinders,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 64, PercentUsed = 6.58, Rank = 6,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 31, PercentUsed = 3.22, Rank = 5,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 11, PercentUsed = 1.12, Rank = 5,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 29, PercentUsed = 2.96, Rank = 6,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 31, PercentUsed = 3.18, Rank = 7,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 23, PercentUsed = 2.36, Rank = 4,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 8, PercentUsed = 0.80, Rank = 9,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 5, PercentUsed = 0.51, Rank = 9,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 5, PercentUsed = 0.50, Rank = 8,},
            }
        };

        public static ItemRank CainsTravelers = new ItemRank

        {
            Item = Legendary.CainsTravelers,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 61, PercentUsed = 6.27, Rank = 7,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 19, PercentUsed = 1.98, Rank = 8,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 9, PercentUsed = 0.92, Rank = 8,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 90, PercentUsed = 9.19, Rank = 5,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 39, PercentUsed = 4.00, Rank = 6,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 11, PercentUsed = 1.13, Rank = 9,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 26, PercentUsed = 2.61, Rank = 6,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 11, PercentUsed = 1.10, Rank = 5,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 2, PercentUsed = 0.20, Rank = 3,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 109, PercentUsed = 11.01, Rank = 3,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 16, PercentUsed = 1.60, Rank = 4,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 4, PercentUsed = 0.40, Rank = 6,},
            }
        };

        public static ItemRank FireWalkers = new ItemRank

        {
            Item = Legendary.FireWalkers,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 40, PercentUsed = 4.11, Rank = 8,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 20, PercentUsed = 2.08, Rank = 7,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 13, PercentUsed = 1.32, Rank = 4,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 13, PercentUsed = 1.33, Rank = 9,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 19, PercentUsed = 1.95, Rank = 9,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 12, PercentUsed = 1.23, Rank = 8,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 4, PercentUsed = 0.40, Rank = 10,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 2, PercentUsed = 0.20, Rank = 8,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 2, PercentUsed = 0.20, Rank = 4,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 20, PercentUsed = 2.02, Rank = 7,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 2, PercentUsed = 0.20, Rank = 10,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 4, PercentUsed = 0.40, Rank = 5,},
            }
        };

        public static ItemRank IrontoeMudsputters = new ItemRank

        {
            Item = Legendary.IrontoeMudsputters,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 20, PercentUsed = 2.06, Rank = 9,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 1, PercentUsed = 0.10, Rank = 10,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
            }
        };

        public static ItemRank IllusoryBoots = new ItemRank

        {
            Item = Legendary.IllusoryBoots,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 17, PercentUsed = 1.75, Rank = 10,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 19, PercentUsed = 1.94, Rank = 8,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 13, PercentUsed = 1.33, Rank = 10,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 13, PercentUsed = 1.33, Rank = 7,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 24, PercentUsed = 2.41, Rank = 7,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 3, PercentUsed = 0.30, Rank = 7,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 2, PercentUsed = 0.20, Rank = 6,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 28, PercentUsed = 2.83, Rank = 6,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 2, PercentUsed = 0.20, Rank = 9,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 2, PercentUsed = 0.20, Rank = 7,},
            }
        };

        public static ItemRank SunwukosShines = new ItemRank

        {
            Item = Legendary.SunwukosShines,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 168, PercentUsed = 17.27, Rank = 1,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 310, PercentUsed = 31.16, Rank = 1,},
            }
        };

        public static ItemRank HauntOfVaxo = new ItemRank

        {
            Item = Legendary.HauntOfVaxo,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 90, PercentUsed = 9.25, Rank = 2,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 169, PercentUsed = 17.57, Rank = 1,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 75, PercentUsed = 7.63, Rank = 5,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 91, PercentUsed = 9.30, Rank = 4,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 60, PercentUsed = 6.15, Rank = 6,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 108, PercentUsed = 11.07, Rank = 2,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 46, PercentUsed = 4.62, Rank = 5,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 110, PercentUsed = 11.04, Rank = 2,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 39, PercentUsed = 3.94, Rank = 6,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 73, PercentUsed = 7.37, Rank = 4,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 92, PercentUsed = 9.23, Rank = 3,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 104, PercentUsed = 10.46, Rank = 2,},
            }
        };

        public static ItemRank BlackthornesDuncraigCross = new ItemRank

        {
            Item = Legendary.BlackthornesDuncraigCross,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 86, PercentUsed = 8.84, Rank = 3,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 63, PercentUsed = 6.55, Rank = 5,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 94, PercentUsed = 9.56, Rank = 3,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 74, PercentUsed = 7.56, Rank = 5,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 70, PercentUsed = 7.18, Rank = 4,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 44, PercentUsed = 4.51, Rank = 8,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 138, PercentUsed = 13.87, Rank = 2,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 63, PercentUsed = 6.33, Rank = 6,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 53, PercentUsed = 5.36, Rank = 2,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 100, PercentUsed = 10.10, Rank = 2,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 112, PercentUsed = 11.23, Rank = 1,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 53, PercentUsed = 5.33, Rank = 7,},
            }
        };

        public static ItemRank CountessJuliasCameo = new ItemRank

        {
            Item = Legendary.CountessJuliasCameo,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 85, PercentUsed = 8.74, Rank = 4,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 114, PercentUsed = 11.85, Rank = 2,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 101, PercentUsed = 10.27, Rank = 2,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 140, PercentUsed = 14.30, Rank = 1,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 74, PercentUsed = 7.59, Rank = 2,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 95, PercentUsed = 9.73, Rank = 3,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 41, PercentUsed = 4.12, Rank = 10,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 49, PercentUsed = 4.95, Rank = 3,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 73, PercentUsed = 7.37, Rank = 3,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 46, PercentUsed = 4.61, Rank = 9,},
            }
        };

        public static ItemRank Ouroboros = new ItemRank

        {
            Item = Legendary.Ouroboros,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 77, PercentUsed = 7.91, Rank = 5,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 92, PercentUsed = 9.56, Rank = 4,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 111, PercentUsed = 11.29, Rank = 1,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 115, PercentUsed = 11.75, Rank = 2,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 57, PercentUsed = 5.85, Rank = 7,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 116, PercentUsed = 11.89, Rank = 1,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 47, PercentUsed = 4.72, Rank = 4,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 63, PercentUsed = 6.33, Rank = 5,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 36, PercentUsed = 3.64, Rank = 7,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 71, PercentUsed = 7.17, Rank = 5,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 62, PercentUsed = 6.22, Rank = 7,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 80, PercentUsed = 8.05, Rank = 3,},
            }
        };

        public static ItemRank GoldenGorgetOfLeoric = new ItemRank

        {
            Item = Legendary.GoldenGorgetOfLeoric,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 59, PercentUsed = 6.06, Rank = 6,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 103, PercentUsed = 10.71, Rank = 3,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 72, PercentUsed = 7.32, Rank = 6,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 113, PercentUsed = 11.54, Rank = 3,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 62, PercentUsed = 6.36, Rank = 5,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 74, PercentUsed = 7.58, Rank = 4,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 30, PercentUsed = 3.02, Rank = 7,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 47, PercentUsed = 4.72, Rank = 8,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 42, PercentUsed = 4.24, Rank = 7,},
            }
        };

        public static ItemRank SquirtsNecklace = new ItemRank

        {
            Item = Legendary.SquirtsNecklace,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 36, PercentUsed = 3.70, Rank = 7,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 41, PercentUsed = 4.26, Rank = 6,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 56, PercentUsed = 5.70, Rank = 8,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 48, PercentUsed = 4.90, Rank = 8,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 38, PercentUsed = 3.90, Rank = 8,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 61, PercentUsed = 6.25, Rank = 5,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 25, PercentUsed = 2.51, Rank = 9,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 50, PercentUsed = 5.02, Rank = 7,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 40, PercentUsed = 4.04, Rank = 5,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 39, PercentUsed = 3.94, Rank = 8,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 40, PercentUsed = 4.01, Rank = 10,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 61, PercentUsed = 6.14, Rank = 4,},
            }
        };

        public static ItemRank TheTravelersPledge = new ItemRank

        {
            Item = Legendary.TheTravelersPledge,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 33, PercentUsed = 3.39, Rank = 8,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 27, PercentUsed = 2.81, Rank = 10,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 67, PercentUsed = 6.82, Rank = 7,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 29, PercentUsed = 2.97, Rank = 10,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 40, PercentUsed = 4.10, Rank = 9,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 24, PercentUsed = 2.41, Rank = 10,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 41, PercentUsed = 4.12, Rank = 9,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 28, PercentUsed = 2.83, Rank = 8,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 55, PercentUsed = 5.52, Rank = 8,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 49, PercentUsed = 4.93, Rank = 8,},
            }
        };

        public static ItemRank MarasKaleidoscope = new ItemRank

        {
            Item = Legendary.MarasKaleidoscope,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 28, PercentUsed = 2.88, Rank = 9,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 37, PercentUsed = 3.85, Rank = 8,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 52, PercentUsed = 5.29, Rank = 9,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 69, PercentUsed = 7.05, Rank = 6,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 33, PercentUsed = 3.38, Rank = 9,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 57, PercentUsed = 5.84, Rank = 6,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 38, PercentUsed = 3.82, Rank = 6,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 95, PercentUsed = 9.54, Rank = 4,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 44, PercentUsed = 4.45, Rank = 4,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 68, PercentUsed = 6.87, Rank = 6,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 85, PercentUsed = 8.53, Rank = 5,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 60, PercentUsed = 6.04, Rank = 5,},
            }
        };

        public static ItemRank XephirianAmulet = new ItemRank

        {
            Item = Legendary.XephirianAmulet,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 25, PercentUsed = 2.57, Rank = 10,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 37, PercentUsed = 3.78, Rank = 10,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 27, PercentUsed = 2.71, Rank = 8,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 106, PercentUsed = 10.64, Rank = 3,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 37, PercentUsed = 3.74, Rank = 10,},
            }
        };

        public static ItemRank RingOfRoyalGrandeur = new ItemRank

        {
            Item = Legendary.RingOfRoyalGrandeur,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 741, PercentUsed = 76.16, Rank = 1,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 776, PercentUsed = 80.67, Rank = 1,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 817, PercentUsed = 83.11, Rank = 1,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 672, PercentUsed = 68.64, Rank = 1,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 762, PercentUsed = 78.15, Rank = 1,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 780, PercentUsed = 79.92, Rank = 1,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 820, PercentUsed = 82.41, Rank = 1,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 688, PercentUsed = 69.08, Rank = 1,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 933, PercentUsed = 94.34, Rank = 1,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 598, PercentUsed = 60.40, Rank = 1,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 359, PercentUsed = 36.01, Rank = 3,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 616, PercentUsed = 61.97, Rank = 2,},
            }
        };

        public static ItemRank StoneOfJordan = new ItemRank

        {
            Item = Legendary.StoneOfJordan,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 312, PercentUsed = 32.07, Rank = 2,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 437, PercentUsed = 45.43, Rank = 2,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 411, PercentUsed = 41.81, Rank = 2,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 393, PercentUsed = 40.14, Rank = 2,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 520, PercentUsed = 53.33, Rank = 2,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 545, PercentUsed = 55.84, Rank = 2,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 283, PercentUsed = 28.44, Rank = 3,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 464, PercentUsed = 46.59, Rank = 3,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 477, PercentUsed = 48.23, Rank = 2,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 392, PercentUsed = 39.60, Rank = 2,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 839, PercentUsed = 84.15, Rank = 1,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 712, PercentUsed = 71.63, Rank = 1,},
            }
        };

        public static ItemRank Unity = new ItemRank

        {
            Item = Legendary.Unity,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 232, PercentUsed = 23.84, Rank = 3,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 289, PercentUsed = 30.04, Rank = 3,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 333, PercentUsed = 33.88, Rank = 3,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 231, PercentUsed = 23.60, Rank = 4,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 266, PercentUsed = 27.28, Rank = 3,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 250, PercentUsed = 25.61, Rank = 3,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 524, PercentUsed = 52.66, Rank = 2,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 575, PercentUsed = 57.73, Rank = 2,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 319, PercentUsed = 32.25, Rank = 3,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 364, PercentUsed = 36.77, Rank = 4,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 599, PercentUsed = 60.08, Rank = 2,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 402, PercentUsed = 40.44, Rank = 3,},
            }
        };

        public static ItemRank LeoricsSignet = new ItemRank

        {
            Item = Legendary.LeoricsSignet,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 75, PercentUsed = 7.71, Rank = 4,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 61, PercentUsed = 6.34, Rank = 4,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 36, PercentUsed = 3.66, Rank = 5,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 21, PercentUsed = 2.15, Rank = 6,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 58, PercentUsed = 5.95, Rank = 4,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 45, PercentUsed = 4.61, Rank = 4,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 11, PercentUsed = 1.11, Rank = 7,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 13, PercentUsed = 1.31, Rank = 6,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 10, PercentUsed = 1.01, Rank = 7,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 6, PercentUsed = 0.61, Rank = 10,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 16, PercentUsed = 1.60, Rank = 6,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 6, PercentUsed = 0.60, Rank = 8,},
            }
        };

        public static ItemRank BulkathossWeddingBand = new ItemRank

        {
            Item = Legendary.BulkathossWeddingBand,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 56, PercentUsed = 5.76, Rank = 5,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 37, PercentUsed = 3.85, Rank = 5,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 18, PercentUsed = 1.83, Rank = 7,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 9, PercentUsed = 0.92, Rank = 9,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 29, PercentUsed = 2.97, Rank = 5,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 33, PercentUsed = 3.38, Rank = 6,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 11, PercentUsed = 1.11, Rank = 6,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 14, PercentUsed = 1.41, Rank = 5,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 4, PercentUsed = 0.40, Rank = 9,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 9, PercentUsed = 0.91, Rank = 7,},
            }
        };

        public static ItemRank Wyrdward = new ItemRank

        {
            Item = Legendary.Wyrdward,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 29, PercentUsed = 2.98, Rank = 6,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 14, PercentUsed = 1.44, Rank = 8,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 5, PercentUsed = 0.50, Rank = 9,},
            }
        };

        public static ItemRank BandOfUntoldSecrets = new ItemRank

        {
            Item = Legendary.BandOfUntoldSecrets,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 23, PercentUsed = 2.36, Rank = 7,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 16, PercentUsed = 1.66, Rank = 8,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 14, PercentUsed = 1.44, Rank = 9,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 13, PercentUsed = 1.33, Rank = 9,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 4, PercentUsed = 0.40, Rank = 10,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 6, PercentUsed = 0.60, Rank = 7,},
            }
        };

        public static ItemRank BandOfTheRueChambers = new ItemRank

        {
            Item = Legendary.BandOfTheRueChambers,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 22, PercentUsed = 2.26, Rank = 8,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 28, PercentUsed = 2.81, Rank = 4,},
            }
        };

        public static ItemRank PuzzleRing = new ItemRank

        {
            Item = Legendary.PuzzleRing,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 22, PercentUsed = 2.26, Rank = 9,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 19, PercentUsed = 1.98, Rank = 7,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 16, PercentUsed = 1.63, Rank = 8,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 16, PercentUsed = 1.63, Rank = 7,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 12, PercentUsed = 1.23, Rank = 10,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 6, PercentUsed = 0.60, Rank = 8,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 6, PercentUsed = 0.61, Rank = 9,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 5, PercentUsed = 0.50, Rank = 9,},
            }
        };

        public static ItemRank RogarsHugeStone = new ItemRank

        {
            Item = Legendary.RogarsHugeStone,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 20, PercentUsed = 2.06, Rank = 10,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 15, PercentUsed = 1.56, Rank = 9,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
            }
        };

        public static ItemRank WonKhimLau = new ItemRank

        {
            Item = Legendary.WonKhimLau,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 70, PercentUsed = 7.19, Rank = 3,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 31, PercentUsed = 3.12, Rank = 3,},
            }
        };

        public static ItemRank OdynSon = new ItemRank

        {
            Item = Legendary.OdynSon,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 60, PercentUsed = 6.17, Rank = 5,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 115, PercentUsed = 11.95, Rank = 3,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 48, PercentUsed = 4.82, Rank = 2,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 78, PercentUsed = 7.83, Rank = 3,},
            }
        };

        public static ItemRank ThunderfuryBlessedBladeOfTheWindseeker = new ItemRank

        {
            Item = Legendary.ThunderfuryBlessedBladeOfTheWindseeker,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 154, PercentUsed = 15.83, Rank = 2,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 136, PercentUsed = 14.14, Rank = 2,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 80, PercentUsed = 8.17, Rank = 4,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 172, PercentUsed = 17.64, Rank = 2,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 20, PercentUsed = 2.01, Rank = 4,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 10, PercentUsed = 1.00, Rank = 9,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 14, PercentUsed = 1.41, Rank = 6,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 30, PercentUsed = 3.01, Rank = 6,},
            }
        };

        public static ItemRank Jawbreaker = new ItemRank

        {
            Item = Legendary.Jawbreaker,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 28, PercentUsed = 2.88, Rank = 7,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 16, PercentUsed = 1.61, Rank = 6,},
            }
        };

        public static ItemRank ShardOfHate = new ItemRank

        {
            Item = Legendary.ShardOfHate,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 38, PercentUsed = 3.91, Rank = 6,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 55, PercentUsed = 5.72, Rank = 7,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 15, PercentUsed = 1.51, Rank = 7,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 33, PercentUsed = 3.31, Rank = 5,},
            }
        };

        public static ItemRank Fulminator = new ItemRank

        {
            Item = Legendary.Fulminator,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 26, PercentUsed = 2.67, Rank = 8,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 18, PercentUsed = 1.87, Rank = 7,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
            }
        };

        public static ItemRank SledgeFist = new ItemRank

        {
            Item = Legendary.SledgeFist,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 16, PercentUsed = 1.64, Rank = 7,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 12, PercentUsed = 1.21, Rank = 8,},
            }
        };

        public static ItemRank SunKeeper = new ItemRank

        {
            Item = Legendary.SunKeeper,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 19, PercentUsed = 1.95, Rank = 10,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 61, PercentUsed = 6.34, Rank = 5,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 136, PercentUsed = 13.89, Rank = 3,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 55, PercentUsed = 5.64, Rank = 5,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 18, PercentUsed = 1.81, Rank = 5,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 36, PercentUsed = 3.61, Rank = 4,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 185, PercentUsed = 18.69, Rank = 3,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 177, PercentUsed = 17.75, Rank = 2,},
            }
        };

        public static ItemRank Devastator = new ItemRank

        {
            Item = Legendary.Devastator,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 14, PercentUsed = 1.44, Rank = 9,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 40, PercentUsed = 4.16, Rank = 8,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 36, PercentUsed = 3.69, Rank = 8,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 9, PercentUsed = 0.90, Rank = 10,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 9, PercentUsed = 0.90, Rank = 9,},
            }
        };

        public static ItemRank TheFistOfAzturrasq = new ItemRank

        {
            Item = Legendary.TheFistOfAzturrasq,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 11, PercentUsed = 1.13, Rank = 10,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 6, PercentUsed = 0.60, Rank = 9,},
            }
        };

        public static ItemRank FlyingDragon = new ItemRank

        {
            Item = Legendary.FlyingDragon,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 214, PercentUsed = 21.99, Rank = 1,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 737, PercentUsed = 74.07, Rank = 1,},
            }
        };

        public static ItemRank InnasReach = new ItemRank

        {
            Item = Legendary.InnasReach,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 69, PercentUsed = 7.09, Rank = 4,},
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
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 24, PercentUsed = 2.47, Rank = 9,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 9, PercentUsed = 0.90, Rank = 10,},
            }
        };

        public static ItemRank SteadyStrikers = new ItemRank

        {
            Item = Legendary.SteadyStrikers,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 19, PercentUsed = 1.93, Rank = 9,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 11, PercentUsed = 1.13, Rank = 10,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 12, PercentUsed = 1.21, Rank = 6,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 6, PercentUsed = 0.60, Rank = 9,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 89, PercentUsed = 9.00, Rank = 5,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 3, PercentUsed = 0.30, Rank = 9,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 11, PercentUsed = 1.10, Rank = 7,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 8, PercentUsed = 0.80, Rank = 6,},
            }
        };

        public static ItemRank LacuniProwlers = new ItemRank

        {
            Item = Legendary.LacuniProwlers,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 9, PercentUsed = 0.94, Rank = 10,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 42, PercentUsed = 4.27, Rank = 5,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 3, PercentUsed = 0.31, Rank = 9,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 5, PercentUsed = 0.51, Rank = 9,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 9, PercentUsed = 0.90, Rank = 9,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 98, PercentUsed = 9.91, Rank = 3,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 16, PercentUsed = 1.62, Rank = 6,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 3, PercentUsed = 0.30, Rank = 9,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 7, PercentUsed = 0.70, Rank = 7,},
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
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 9, PercentUsed = 0.90, Rank = 9,},
            }
        };

        public static ItemRank TzoKrinsGaze = new ItemRank

        {
            Item = Legendary.TzoKrinsGaze,
            HardcoreRank = new List<ItemRankData>
            {
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 8, PercentUsed = 0.80, Rank = 10,},
            }
        };

        public static ItemRank JadeHarvestersJoy = new ItemRank

        {
            Item = Legendary.JadeHarvestersJoy,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 230, PercentUsed = 23.49, Rank = 2,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 1, PercentUsed = 0.10, Rank = 8,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 492, PercentUsed = 49.70, Rank = 1,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 1, PercentUsed = 0.10, Rank = 8,},
            }
        };

        public static ItemRank Corruption = new ItemRank

        {
            Item = Legendary.Corruption,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 2, PercentUsed = 0.20, Rank = 10,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 1, PercentUsed = 0.10, Rank = 10,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 2, PercentUsed = 0.20, Rank = 6,},
            }
        };

        public static ItemRank TalRashasRelentlessPursuit = new ItemRank

        {
            Item = Legendary.TalRashasRelentlessPursuit,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 12, PercentUsed = 1.23, Rank = 7,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 187, PercentUsed = 19.18, Rank = 3,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 11, PercentUsed = 1.11, Rank = 8,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 1, PercentUsed = 0.10, Rank = 7,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 29, PercentUsed = 2.93, Rank = 4,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 45, PercentUsed = 4.51, Rank = 3,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 1, PercentUsed = 0.10, Rank = 10,},
            }
        };

        public static ItemRank ShiMizusHaori = new ItemRank

        {
            Item = Legendary.ShiMizusHaori,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 3, PercentUsed = 0.31, Rank = 10,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 1, PercentUsed = 0.10, Rank = 10,},
            }
        };

        public static ItemRank Frostburn = new ItemRank

        {
            Item = Legendary.Frostburn,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 7, PercentUsed = 0.71, Rank = 7,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 10, PercentUsed = 1.03, Rank = 10,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 13, PercentUsed = 1.31, Rank = 5,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 3, PercentUsed = 0.30, Rank = 8,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 1, PercentUsed = 0.10, Rank = 10,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 13, PercentUsed = 1.30, Rank = 5,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 2, PercentUsed = 0.20, Rank = 5,},
            }
        };

        public static ItemRank VigilanteBelt = new ItemRank

        {
            Item = Legendary.VigilanteBelt,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 22, PercentUsed = 2.24, Rank = 10,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 147, PercentUsed = 15.06, Rank = 2,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 44, PercentUsed = 4.42, Rank = 6,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 25, PercentUsed = 2.51, Rank = 6,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 7, PercentUsed = 0.71, Rank = 8,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 97, PercentUsed = 9.80, Rank = 3,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 6, PercentUsed = 0.60, Rank = 10,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 594, PercentUsed = 59.76, Rank = 1,},
            }
        };

        public static ItemRank NatalyasBloodyFootprints = new ItemRank

        {
            Item = Legendary.NatalyasBloodyFootprints,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 98, PercentUsed = 9.97, Rank = 2,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 12, PercentUsed = 1.21, Rank = 8,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 48, PercentUsed = 4.85, Rank = 2,},
            }
        };

//        public static ItemRank HellfireAmulet = new ItemRank

//        {
//            Item = Legendary.HellfireAmulet,

//            HardcoreRank = new List<ItemRankData>
//{
//new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 37, PercentUsed = 3.85, Rank = 9, }, 
//new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 91, PercentUsed = 9.26, Rank = 4, }, 
//new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 40, PercentUsed = 4.09, Rank = 9, }, 
//new ItemRankData { Class = ActorClass.Crusader, SampleSize = 39, PercentUsed = 4.00, Rank = 10, }, 

//},
//            SoftcoreRank = new List<ItemRankData>
//{
//new ItemRankData { Class = ActorClass.Monk, SampleSize = 120, PercentUsed = 12.06, Rank = 3, }, 
//new ItemRankData { Class = ActorClass.Barbarian, SampleSize = 116, PercentUsed = 11.65, Rank = 1, }, 
//new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 567, PercentUsed = 57.33, Rank = 1, }, 
//new ItemRankData { Class = ActorClass.Witchdoctor, SampleSize = 243, PercentUsed = 24.55, Rank = 1, }, 
//new ItemRankData { Class = ActorClass.Wizard, SampleSize = 112, PercentUsed = 11.23, Rank = 2, }, 
//new ItemRankData { Class = ActorClass.Crusader, SampleSize = 200, PercentUsed = 20.12, Rank = 1, }, 

//}
//        };
        public static ItemRank AvariceBand = new ItemRank

        {
            Item = Legendary.AvariceBand,
            HardcoreRank = new List<ItemRankData>
            {
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 23, PercentUsed = 2.31, Rank = 5,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 14, PercentUsed = 1.41, Rank = 4,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 92, PercentUsed = 9.30, Rank = 4,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 12, PercentUsed = 1.21, Rank = 7,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 19, PercentUsed = 1.91, Rank = 5,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 24, PercentUsed = 2.41, Rank = 4,},
            }
        };

        public static ItemRank NatalyasReflection = new ItemRank

        {
            Item = Legendary.NatalyasReflection,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 118, PercentUsed = 12.00, Rank = 4,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 10, PercentUsed = 1.01, Rank = 8,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 39, PercentUsed = 3.94, Rank = 5,},
            }
        };

        public static ItemRank TheCompassRose = new ItemRank

        {
            Item = Legendary.TheCompassRose,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 19, PercentUsed = 1.98, Rank = 6,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 35, PercentUsed = 3.56, Rank = 6,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 11, PercentUsed = 1.12, Rank = 8,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 18, PercentUsed = 1.85, Rank = 6,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 21, PercentUsed = 2.15, Rank = 7,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 9, PercentUsed = 0.90, Rank = 9,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 9, PercentUsed = 0.90, Rank = 7,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 17, PercentUsed = 1.72, Rank = 6,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 7, PercentUsed = 0.71, Rank = 8,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 30, PercentUsed = 3.01, Rank = 4,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 18, PercentUsed = 1.81, Rank = 5,},
            }
        };

        public static ItemRank ManaldHeal = new ItemRank

        {
            Item = Legendary.ManaldHeal,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 17, PercentUsed = 1.74, Rank = 8,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 9, PercentUsed = 0.90, Rank = 10,},
            }
        };

        public static ItemRank Azurewrath = new ItemRank

        {
            Item = Legendary.Azurewrath,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 26, PercentUsed = 2.66, Rank = 6,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 8, PercentUsed = 0.80, Rank = 6,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 6, PercentUsed = 0.61, Rank = 9,},
            }
        };

        public static ItemRank TheBurningAxeOfSankis = new ItemRank

        {
            Item = Legendary.TheBurningAxeOfSankis,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 56, PercentUsed = 5.82, Rank = 6,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 51, PercentUsed = 5.23, Rank = 6,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 7, PercentUsed = 0.70, Rank = 8,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 27, PercentUsed = 2.71, Rank = 2,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 10, PercentUsed = 1.00, Rank = 8,},
            }
        };

        public static ItemRank BornsFuriousWrath = new ItemRank

        {
            Item = Legendary.BornsFuriousWrath,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 14, PercentUsed = 1.46, Rank = 9,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Monk, SampleSize = 10, PercentUsed = 1.01, Rank = 9,},
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 5, PercentUsed = 0.50, Rank = 7,},
            }
        };

        public static ItemRank ImmortalKingsTriumph = new ItemRank

        {
            Item = Legendary.ImmortalKingsTriumph,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 397, PercentUsed = 41.27, Rank = 1,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 90, PercentUsed = 9.04, Rank = 4,},
            }
        };

        public static ItemRank EyesOfTheEarth = new ItemRank

        {
            Item = Legendary.EyesOfTheEarth,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 147, PercentUsed = 15.28, Rank = 2,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 99, PercentUsed = 9.94, Rank = 3,},
            }
        };

        public static ItemRank RaekorsWill = new ItemRank

        {
            Item = Legendary.RaekorsWill,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 17, PercentUsed = 1.77, Rank = 8,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 3, PercentUsed = 0.30, Rank = 8,},
            }
        };

        public static ItemRank SkullOfResonance = new ItemRank

        {
            Item = Legendary.SkullOfResonance,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 18, PercentUsed = 1.87, Rank = 7,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 12, PercentUsed = 1.20, Rank = 6,},
            }
        };

        public static ItemRank MempoOfTwilight = new ItemRank

        {
            Item = Legendary.MempoOfTwilight,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 10, PercentUsed = 1.04, Rank = 9,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 6, PercentUsed = 0.61, Rank = 7,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 4, PercentUsed = 0.41, Rank = 9,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 10, PercentUsed = 1.03, Rank = 8,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 8, PercentUsed = 0.82, Rank = 7,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 2, PercentUsed = 0.20, Rank = 6,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 5, PercentUsed = 0.50, Rank = 6,},
            }
        };

        public static ItemRank SpiresOfTheEarth = new ItemRank

        {
            Item = Legendary.SpiresOfTheEarth,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 246, PercentUsed = 25.57, Rank = 2,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 175, PercentUsed = 17.57, Rank = 2,},
            }
        };

        public static ItemRank RaekorsBurden = new ItemRank

        {
            Item = Legendary.RaekorsBurden,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 26, PercentUsed = 2.70, Rank = 6,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 3, PercentUsed = 0.30, Rank = 8,},
            }
        };

        public static ItemRank VileWard = new ItemRank

        {
            Item = Legendary.VileWard,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 59, PercentUsed = 6.13, Rank = 4,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 6, PercentUsed = 0.61, Rank = 8,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 3, PercentUsed = 0.31, Rank = 9,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 639, PercentUsed = 64.16, Rank = 1,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 2, PercentUsed = 0.20, Rank = 5,},
            }
        };

        public static ItemRank ImmortalKingsEternalReign = new ItemRank

        {
            Item = Legendary.ImmortalKingsEternalReign,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 445, PercentUsed = 46.26, Rank = 1,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 104, PercentUsed = 10.44, Rank = 2,},
            }
        };

        public static ItemRank RaekorsHeart = new ItemRank

        {
            Item = Legendary.RaekorsHeart,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 15, PercentUsed = 1.56, Rank = 7,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 2, PercentUsed = 0.20, Rank = 9,},
            }
        };

        public static ItemRank ImmortalKingsIrons = new ItemRank

        {
            Item = Legendary.ImmortalKingsIrons,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 355, PercentUsed = 36.90, Rank = 1,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 60, PercentUsed = 6.02, Rank = 3,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 1, PercentUsed = 0.10, Rank = 7,},
            }
        };

        public static ItemRank PullOfTheEarth = new ItemRank

        {
            Item = Legendary.PullOfTheEarth,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 194, PercentUsed = 20.17, Rank = 2,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 177, PercentUsed = 17.77, Rank = 2,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 1, PercentUsed = 0.10, Rank = 10,},
            }
        };

        public static ItemRank RaekorsWraps = new ItemRank

        {
            Item = Legendary.RaekorsWraps,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 13, PercentUsed = 1.35, Rank = 7,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 3, PercentUsed = 0.30, Rank = 9,},
            }
        };

        public static ItemRank TaskerAndTheo = new ItemRank

        {
            Item = Legendary.TaskerAndTheo,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 44, PercentUsed = 4.57, Rank = 5,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 362, PercentUsed = 36.83, Rank = 2,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 568, PercentUsed = 58.02, Rank = 1,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 57, PercentUsed = 5.85, Rank = 4,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 11, PercentUsed = 1.13, Rank = 6,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 39, PercentUsed = 3.92, Rank = 4,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 857, PercentUsed = 86.65, Rank = 1,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 419, PercentUsed = 42.32, Rank = 2,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 58, PercentUsed = 5.82, Rank = 3,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 11, PercentUsed = 1.11, Rank = 4,},
            }
        };

        public static ItemRank ImmortalKingsTribalBinding = new ItemRank

        {
            Item = Legendary.ImmortalKingsTribalBinding,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 488, PercentUsed = 50.73, Rank = 1,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 87, PercentUsed = 8.73, Rank = 3,},
            }
        };

        public static ItemRank PrideOfCassius = new ItemRank

        {
            Item = Legendary.PrideOfCassius,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 60, PercentUsed = 6.24, Rank = 3,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 548, PercentUsed = 55.02, Rank = 1,},
            }
        };

        public static ItemRank ChilaniksChain = new ItemRank

        {
            Item = Legendary.ChilaniksChain,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 58, PercentUsed = 6.03, Rank = 4,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 22, PercentUsed = 2.21, Rank = 8,},
            }
        };

        public static ItemRank GirdleOfGiants = new ItemRank

        {
            Item = Legendary.GirdleOfGiants,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 11, PercentUsed = 1.14, Rank = 10,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
            }
        };

        public static ItemRank WeightOfTheEarth = new ItemRank

        {
            Item = Legendary.WeightOfTheEarth,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 244, PercentUsed = 25.36, Rank = 1,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 171, PercentUsed = 17.17, Rank = 2,},
            }
        };

        public static ItemRank RaekorsBreeches = new ItemRank

        {
            Item = Legendary.RaekorsBreeches,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 23, PercentUsed = 2.39, Rank = 9,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 2, PercentUsed = 0.20, Rank = 10,},
            }
        };

        public static ItemRank ImmortalKingsStride = new ItemRank

        {
            Item = Legendary.ImmortalKingsStride,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 319, PercentUsed = 33.16, Rank = 1,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 76, PercentUsed = 7.63, Rank = 3,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 1, PercentUsed = 0.10, Rank = 9,},
            }
        };

        public static ItemRank LutSocks = new ItemRank

        {
            Item = Legendary.LutSocks,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 259, PercentUsed = 26.92, Rank = 2,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 193, PercentUsed = 19.38, Rank = 2,},
            }
        };

        public static ItemRank RaekorsStriders = new ItemRank

        {
            Item = Legendary.RaekorsStriders,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 14, PercentUsed = 1.46, Rank = 9,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 2, PercentUsed = 0.20, Rank = 10,},
            }
        };

        public static ItemRank RondalsLocket = new ItemRank

        {
            Item = Legendary.RondalsLocket,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 39, PercentUsed = 4.05, Rank = 7,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 42, PercentUsed = 4.27, Rank = 10,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 51, PercentUsed = 5.21, Rank = 7,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 50, PercentUsed = 5.12, Rank = 7,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 38, PercentUsed = 3.84, Rank = 9,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 34, PercentUsed = 3.42, Rank = 10,},
            }
        };

        public static ItemRank Nagelring = new ItemRank

        {
            Item = Legendary.Nagelring,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 15, PercentUsed = 1.56, Rank = 10,},
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 13, PercentUsed = 1.32, Rank = 9,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 15, PercentUsed = 1.54, Rank = 7,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
            }
        };

        public static ItemRank BulkathossWarriorBlood = new ItemRank

        {
            Item = Legendary.BulkathossWarriorBlood,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 14, PercentUsed = 1.46, Rank = 8,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 4, PercentUsed = 0.40, Rank = 10,},
            }
        };

        public static ItemRank BulkathossSolemnVow = new ItemRank

        {
            Item = Legendary.BulkathossSolemnVow,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 13, PercentUsed = 1.35, Rank = 10,},
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
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 165, PercentUsed = 17.15, Rank = 1,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 45, PercentUsed = 4.62, Rank = 7,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 71, PercentUsed = 7.27, Rank = 3,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 306, PercentUsed = 30.72, Rank = 2,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 37, PercentUsed = 3.71, Rank = 5,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 14, PercentUsed = 1.41, Rank = 7,},
            }
        };

        public static ItemRank ImmortalKingsBoulderBreaker = new ItemRank

        {
            Item = Legendary.ImmortalKingsBoulderBreaker,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 74, PercentUsed = 7.69, Rank = 4,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 17, PercentUsed = 1.71, Rank = 8,},
            }
        };

        public static ItemRank TheFurnace = new ItemRank

        {
            Item = Legendary.TheFurnace,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 36, PercentUsed = 3.74, Rank = 9,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 15, PercentUsed = 1.53, Rank = 9,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 58, PercentUsed = 5.95, Rank = 4,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 62, PercentUsed = 6.35, Rank = 4,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 371, PercentUsed = 37.25, Rank = 1,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 265, PercentUsed = 26.77, Rank = 2,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 525, PercentUsed = 52.66, Rank = 1,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 520, PercentUsed = 52.31, Rank = 1,},
            }
        };

        public static ItemRank StalgardsDecimator = new ItemRank

        {
            Item = Legendary.StalgardsDecimator,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 26, PercentUsed = 2.70, Rank = 10,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 22, PercentUsed = 2.21, Rank = 6,},
            }
        };

        public static ItemRank TragoulCoils = new ItemRank

        {
            Item = Legendary.TragoulCoils,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 3, PercentUsed = 0.31, Rank = 10,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 10, PercentUsed = 1.00, Rank = 8,},
            }
        };

        public static ItemRank CrownOfTheInvoker = new ItemRank

        {
            Item = Legendary.CrownOfTheInvoker,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 3, PercentUsed = 0.31, Rank = 10,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 1, PercentUsed = 0.10, Rank = 9,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 1, PercentUsed = 0.10, Rank = 8,},
            }
        };

        public static ItemRank DemonsHeart = new ItemRank

        {
            Item = Legendary.DemonsHeart,
            HardcoreRank = new List<ItemRankData>
            {
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 1, PercentUsed = 0.10, Rank = 10,},
            }
        };

        public static ItemRank StoneGauntlets = new ItemRank

        {
            Item = Legendary.StoneGauntlets,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 2, PercentUsed = 0.20, Rank = 10,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 1, PercentUsed = 0.10, Rank = 10,},
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
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 4, PercentUsed = 0.40, Rank = 10,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 4, PercentUsed = 0.40, Rank = 10,},
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
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 5, PercentUsed = 0.50, Rank = 8,},
            }
        };

        public static ItemRank Doombringer = new ItemRank

        {
            Item = Legendary.Doombringer,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 68, PercentUsed = 6.95, Rank = 5,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 4, PercentUsed = 0.40, Rank = 9,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 8, PercentUsed = 0.81, Rank = 8,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 4, PercentUsed = 0.40, Rank = 10,},
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
                new ItemRankData {Class = ActorClass.Barbarian, SampleSize = 18, PercentUsed = 1.81, Rank = 7,},
            }
        };

        public static ItemRank GungdoGear = new ItemRank

        {
            Item = Legendary.GungdoGear,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 13, PercentUsed = 1.32, Rank = 10,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 2, PercentUsed = 0.20, Rank = 10,},
            }
        };

        public static ItemRank MaraudersVisage = new ItemRank

        {
            Item = Legendary.MaraudersVisage,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 779, PercentUsed = 79.25, Rank = 1,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 905, PercentUsed = 91.51, Rank = 1,},
            }
        };

        public static ItemRank NatalyasSight = new ItemRank

        {
            Item = Legendary.NatalyasSight,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 71, PercentUsed = 7.22, Rank = 2,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 17, PercentUsed = 1.72, Rank = 4,},
            }
        };

        public static ItemRank PridesFall = new ItemRank

        {
            Item = Legendary.PridesFall,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 31, PercentUsed = 3.15, Rank = 4,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 16, PercentUsed = 1.64, Rank = 7,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 5, PercentUsed = 0.51, Rank = 9,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 25, PercentUsed = 2.53, Rank = 2,},
            }
        };

        public static ItemRank DeathseersCowl = new ItemRank

        {
            Item = Legendary.DeathseersCowl,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 4, PercentUsed = 0.41, Rank = 8,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
            }
        };

        public static ItemRank BrokenCrown = new ItemRank

        {
            Item = Legendary.BrokenCrown,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 3, PercentUsed = 0.31, Rank = 9,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 2, PercentUsed = 0.20, Rank = 10,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 6, PercentUsed = 0.62, Rank = 10,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 1, PercentUsed = 0.10, Rank = 10,},
            }
        };

        public static ItemRank MaraudersSpines = new ItemRank

        {
            Item = Legendary.MaraudersSpines,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 800, PercentUsed = 81.38, Rank = 1,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 943, PercentUsed = 95.35, Rank = 1,},
            }
        };

        public static ItemRank MaraudersCarapace = new ItemRank

        {
            Item = Legendary.MaraudersCarapace,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 704, PercentUsed = 71.62, Rank = 1,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 888, PercentUsed = 89.79, Rank = 1,},
            }
        };

        public static ItemRank NatalyasEmbrace = new ItemRank

        {
            Item = Legendary.NatalyasEmbrace,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 37, PercentUsed = 3.76, Rank = 3,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 4, PercentUsed = 0.40, Rank = 4,},
            }
        };

        public static ItemRank TheCloakOfTheGarwulf = new ItemRank

        {
            Item = Legendary.TheCloakOfTheGarwulf,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 24, PercentUsed = 2.44, Rank = 4,},
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
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 7, PercentUsed = 0.71, Rank = 8,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
            }
        };

        public static ItemRank Blackfeather = new ItemRank

        {
            Item = Legendary.Blackfeather,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 6, PercentUsed = 0.61, Rank = 9,},
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
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 5, PercentUsed = 0.51, Rank = 10,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 1, PercentUsed = 0.10, Rank = 8,},
            }
        };

        public static ItemRank MaraudersGloves = new ItemRank

        {
            Item = Legendary.MaraudersGloves,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 366, PercentUsed = 37.23, Rank = 1,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 48, PercentUsed = 4.85, Rank = 3,},
            }
        };

        public static ItemRank TheShadowsGrasp = new ItemRank

        {
            Item = Legendary.TheShadowsGrasp,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 8, PercentUsed = 0.81, Rank = 5,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 1, PercentUsed = 0.10, Rank = 8,},
            }
        };

        public static ItemRank MaraudersEncasement = new ItemRank

        {
            Item = Legendary.MaraudersEncasement,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 794, PercentUsed = 80.77, Rank = 1,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 929, PercentUsed = 93.93, Rank = 1,},
            }
        };

        public static ItemRank TheShadowsCoil = new ItemRank

        {
            Item = Legendary.TheShadowsCoil,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 4, PercentUsed = 0.41, Rank = 10,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 1, PercentUsed = 0.10, Rank = 10,},
            }
        };

        public static ItemRank MaraudersTreads = new ItemRank

        {
            Item = Legendary.MaraudersTreads,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 776, PercentUsed = 78.94, Rank = 1,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 919, PercentUsed = 92.92, Rank = 1,},
            }
        };

        public static ItemRank TheShadowsHeels = new ItemRank

        {
            Item = Legendary.TheShadowsHeels,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 3, PercentUsed = 0.31, Rank = 10,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 2, PercentUsed = 0.20, Rank = 7,},
            }
        };

        public static ItemRank SkullGrasp = new ItemRank

        {
            Item = Legendary.SkullGrasp,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 9, PercentUsed = 0.92, Rank = 10,},
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
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 680, PercentUsed = 69.18, Rank = 1,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 872, PercentUsed = 88.17, Rank = 1,},
            }
        };

        public static ItemRank DeadMansLegacy = new ItemRank

        {
            Item = Legendary.DeadMansLegacy,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 46, PercentUsed = 4.68, Rank = 2,},
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
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 42, PercentUsed = 4.27, Rank = 10,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 18, PercentUsed = 1.82, Rank = 4,},
            }
        };

        public static ItemRank DanettasSpite = new ItemRank

        {
            Item = Legendary.DanettasSpite,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 50, PercentUsed = 5.09, Rank = 7,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 19, PercentUsed = 1.92, Rank = 3,},
            }
        };

        public static ItemRank SinSeekers = new ItemRank

        {
            Item = Legendary.SinSeekers,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 36, PercentUsed = 3.66, Rank = 5,},
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
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 44, PercentUsed = 4.48, Rank = 8,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 54, PercentUsed = 5.46, Rank = 5,},
            }
        };

        public static ItemRank ArchfiendArrows = new ItemRank

        {
            Item = Legendary.ArchfiendArrows,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 17, PercentUsed = 1.73, Rank = 7,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 4, PercentUsed = 0.40, Rank = 9,},
            }
        };

        public static ItemRank EmimeisDuffel = new ItemRank

        {
            Item = Legendary.EmimeisDuffel,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 11, PercentUsed = 1.12, Rank = 9,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
            }
        };

        public static ItemRank HolyPointShot = new ItemRank

        {
            Item = Legendary.HolyPointShot,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 11, PercentUsed = 1.12, Rank = 10,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 6, PercentUsed = 0.61, Rank = 8,},
            }
        };

        public static ItemRank NatalyasSlayer = new ItemRank

        {
            Item = Legendary.NatalyasSlayer,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 126, PercentUsed = 12.82, Rank = 1,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 55, PercentUsed = 5.56, Rank = 4,},
            }
        };

        public static ItemRank ArcaneBarb = new ItemRank

        {
            Item = Legendary.ArcaneBarb,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 102, PercentUsed = 10.38, Rank = 2,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 62, PercentUsed = 6.27, Rank = 3,},
            }
        };

        public static ItemRank BurizadoKyanon = new ItemRank

        {
            Item = Legendary.BurizadoKyanon,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 101, PercentUsed = 10.27, Rank = 3,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 77, PercentUsed = 7.79, Rank = 2,},
            }
        };

        public static ItemRank Etrayu = new ItemRank

        {
            Item = Legendary.Etrayu,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 84, PercentUsed = 8.55, Rank = 4,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 370, PercentUsed = 37.41, Rank = 1,},
            }
        };

        public static ItemRank Manticore = new ItemRank

        {
            Item = Legendary.Manticore,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 82, PercentUsed = 8.34, Rank = 5,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 40, PercentUsed = 4.04, Rank = 8,},
            }
        };

        public static ItemRank Kridershot = new ItemRank

        {
            Item = Legendary.Kridershot,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 67, PercentUsed = 6.82, Rank = 6,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 51, PercentUsed = 5.16, Rank = 6,},
            }
        };

        public static ItemRank Hellrack = new ItemRank

        {
            Item = Legendary.Hellrack,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 42, PercentUsed = 4.27, Rank = 9,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
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
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 3, PercentUsed = 0.30, Rank = 10,},
            }
        };

        public static ItemRank SwampLandWaders = new ItemRank

        {
            Item = Legendary.SwampLandWaders,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 75, PercentUsed = 7.66, Rank = 5,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 1, PercentUsed = 0.10, Rank = 9,},
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 73, PercentUsed = 7.37, Rank = 5,},
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
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 25, PercentUsed = 2.53, Rank = 9,},
            }
        };

        public static ItemRank EyeOfEtlich = new ItemRank

        {
            Item = Legendary.EyeOfEtlich,
            HardcoreRank = new List<ItemRankData>
            {
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 20, PercentUsed = 2.02, Rank = 10,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 71, PercentUsed = 7.12, Rank = 6,},
            }
        };

//        public static ItemRank HellfireRing = new ItemRank

//        {
//            Item = Legendary.HellfireRing,

//            HardcoreRank = new List<ItemRankData>
//            {

//            },
//            SoftcoreRank = new List<ItemRankData>
//{
//new ItemRankData { Class = ActorClass.DemonHunter, SampleSize = 4, PercentUsed = 0.40, Rank = 8, }, 

//}
//        };
        public static ItemRank KredesFlame = new ItemRank

        {
            Item = Legendary.KredesFlame,
            HardcoreRank = new List<ItemRankData>
            {
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 4, PercentUsed = 0.40, Rank = 9,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 4, PercentUsed = 0.40, Rank = 10,},
            }
        };

        public static ItemRank MeticulousBolts = new ItemRank

        {
            Item = Legendary.MeticulousBolts,
            HardcoreRank = new List<ItemRankData>
            {
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 10, PercentUsed = 1.01, Rank = 5,},
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
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 7, PercentUsed = 0.71, Rank = 6,},
            }
        };

        public static ItemRank SpinesOfSeethingHatred = new ItemRank

        {
            Item = Legendary.SpinesOfSeethingHatred,
            HardcoreRank = new List<ItemRankData>
            {
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 3, PercentUsed = 0.30, Rank = 10,},
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
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 42, PercentUsed = 4.25, Rank = 7,},
            }
        };

        public static ItemRank UnboundBolt = new ItemRank

        {
            Item = Legendary.UnboundBolt,
            HardcoreRank = new List<ItemRankData>
            {
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 37, PercentUsed = 3.74, Rank = 9,},
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
                new ItemRankData {Class = ActorClass.DemonHunter, SampleSize = 33, PercentUsed = 3.34, Rank = 10,},
            }
        };

        public static ItemRank GuardiansAversion = new ItemRank

        {
            Item = Legendary.GuardiansAversion,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 3, PercentUsed = 0.31, Rank = 8,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
            }
        };

        public static ItemRank MaskOfJeram = new ItemRank

        {
            Item = Legendary.MaskOfJeram,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 684, PercentUsed = 69.87, Rank = 1,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 416, PercentUsed = 42.02, Rank = 2,},
            }
        };

        public static ItemRank Quetzalcoatl = new ItemRank

        {
            Item = Legendary.Quetzalcoatl,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 202, PercentUsed = 20.63, Rank = 2,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 485, PercentUsed = 48.99, Rank = 1,},
            }
        };

        public static ItemRank Carnevil = new ItemRank

        {
            Item = Legendary.Carnevil,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 30, PercentUsed = 3.06, Rank = 3,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 38, PercentUsed = 3.84, Rank = 3,},
            }
        };

        public static ItemRank TheGrinReaper = new ItemRank

        {
            Item = Legendary.TheGrinReaper,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 10, PercentUsed = 1.02, Rank = 5,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 5, PercentUsed = 0.51, Rank = 5,},
            }
        };

        public static ItemRank TiklandianVisage = new ItemRank

        {
            Item = Legendary.TiklandianVisage,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 8, PercentUsed = 0.82, Rank = 6,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 35, PercentUsed = 3.54, Rank = 4,},
            }
        };

        public static ItemRank ZunimassasVision = new ItemRank

        {
            Item = Legendary.ZunimassasVision,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 7, PercentUsed = 0.72, Rank = 7,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 2, PercentUsed = 0.20, Rank = 7,},
            }
        };

        public static ItemRank JadeHarvestersWisdom = new ItemRank

        {
            Item = Legendary.JadeHarvestersWisdom,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 4, PercentUsed = 0.41, Rank = 8,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
            }
        };

        public static ItemRank HelltoothMantle = new ItemRank

        {
            Item = Legendary.HelltoothMantle,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 17, PercentUsed = 1.74, Rank = 4,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 8, PercentUsed = 0.81, Rank = 6,},
            }
        };

        public static ItemRank SevenSins = new ItemRank

        {
            Item = Legendary.SevenSins,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 1, PercentUsed = 0.10, Rank = 10,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 2, PercentUsed = 0.21, Rank = 10,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
            }
        };

        public static ItemRank ZunimassasMarrow = new ItemRank

        {
            Item = Legendary.ZunimassasMarrow,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 321, PercentUsed = 32.79, Rank = 1,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 136, PercentUsed = 13.74, Rank = 3,},
            }
        };

        public static ItemRank JadeHarvestersPeace = new ItemRank

        {
            Item = Legendary.JadeHarvestersPeace,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 216, PercentUsed = 22.06, Rank = 3,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 483, PercentUsed = 48.79, Rank = 1,},
            }
        };

        public static ItemRank HelltoothTunic = new ItemRank

        {
            Item = Legendary.HelltoothTunic,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 8, PercentUsed = 0.82, Rank = 8,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 2, PercentUsed = 0.20, Rank = 10,},
            }
        };

        public static ItemRank JadeHarvestersMercy = new ItemRank

        {
            Item = Legendary.JadeHarvestersMercy,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 236, PercentUsed = 24.11, Rank = 2,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 492, PercentUsed = 49.70, Rank = 1,},
            }
        };

        public static ItemRank HelltoothGauntlets = new ItemRank

        {
            Item = Legendary.HelltoothGauntlets,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 9, PercentUsed = 0.92, Rank = 7,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 1, PercentUsed = 0.10, Rank = 7,},
            }
        };

        public static ItemRank HwojWrap = new ItemRank

        {
            Item = Legendary.HwojWrap,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 96, PercentUsed = 9.81, Rank = 4,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 86, PercentUsed = 8.69, Rank = 5,},
            }
        };

        public static ItemRank RazorStrop = new ItemRank

        {
            Item = Legendary.RazorStrop,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 10, PercentUsed = 1.02, Rank = 9,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
            }
        };

        public static ItemRank CordOfTheSherma = new ItemRank

        {
            Item = Legendary.CordOfTheSherma,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 7, PercentUsed = 0.72, Rank = 10,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 3, PercentUsed = 0.30, Rank = 10,},
            }
        };

        public static ItemRank JadeHarvestersCourage = new ItemRank

        {
            Item = Legendary.JadeHarvestersCourage,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 248, PercentUsed = 25.33, Rank = 2,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 497, PercentUsed = 50.20, Rank = 1,},
            }
        };

        public static ItemRank HelltoothLegGuards = new ItemRank

        {
            Item = Legendary.HelltoothLegGuards,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 28, PercentUsed = 2.86, Rank = 7,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 8, PercentUsed = 0.81, Rank = 6,},
            }
        };

        public static ItemRank ZunimassasTrail = new ItemRank

        {
            Item = Legendary.ZunimassasTrail,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 330, PercentUsed = 33.71, Rank = 1,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 135, PercentUsed = 13.64, Rank = 2,},
            }
        };

        public static ItemRank JadeHarvestersSwiftness = new ItemRank

        {
            Item = Legendary.JadeHarvestersSwiftness,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 226, PercentUsed = 23.08, Rank = 2,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 492, PercentUsed = 49.70, Rank = 1,},
            }
        };

        public static ItemRank HelltoothGreaves = new ItemRank

        {
            Item = Legendary.HelltoothGreaves,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 20, PercentUsed = 2.04, Rank = 7,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 8, PercentUsed = 0.81, Rank = 8,},
            }
        };

        public static ItemRank TheTallMansFinger = new ItemRank

        {
            Item = Legendary.TheTallMansFinger,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 385, PercentUsed = 39.33, Rank = 3,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 372, PercentUsed = 37.58, Rank = 3,},
            }
        };

        public static ItemRank ZunimassasPox = new ItemRank

        {
            Item = Legendary.ZunimassasPox,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 92, PercentUsed = 9.40, Rank = 5,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 19, PercentUsed = 1.92, Rank = 5,},
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 5, PercentUsed = 0.50, Rank = 8,},
            }
        };

        public static ItemRank StolenRing = new ItemRank

        {
            Item = Legendary.StolenRing,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 7, PercentUsed = 0.72, Rank = 10,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 11, PercentUsed = 1.13, Rank = 10,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
            }
        };

        public static ItemRank UhkapianSerpent = new ItemRank

        {
            Item = Legendary.UhkapianSerpent,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 513, PercentUsed = 52.40, Rank = 1,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 286, PercentUsed = 28.89, Rank = 1,},
            }
        };

        public static ItemRank ZunimassasStringOfSkulls = new ItemRank

        {
            Item = Legendary.ZunimassasStringOfSkulls,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 270, PercentUsed = 27.58, Rank = 2,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 128, PercentUsed = 12.93, Rank = 3,},
            }
        };

        public static ItemRank ThingOfTheDeep = new ItemRank

        {
            Item = Legendary.ThingOfTheDeep,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 135, PercentUsed = 13.79, Rank = 3,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 186, PercentUsed = 18.79, Rank = 2,},
            }
        };

        public static ItemRank Homunculus = new ItemRank

        {
            Item = Legendary.Homunculus,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 10, PercentUsed = 1.02, Rank = 4,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 16, PercentUsed = 1.62, Rank = 5,},
            }
        };

        public static ItemRank Spite = new ItemRank

        {
            Item = Legendary.Spite,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 9, PercentUsed = 0.92, Rank = 5,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 4, PercentUsed = 0.40, Rank = 7,},
            }
        };

        public static ItemRank ManajumasGoryFetch = new ItemRank

        {
            Item = Legendary.ManajumasGoryFetch,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 6, PercentUsed = 0.61, Rank = 6,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 2, PercentUsed = 0.20, Rank = 8,},
            }
        };

        public static ItemRank GazingDemise = new ItemRank

        {
            Item = Legendary.GazingDemise,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 4, PercentUsed = 0.41, Rank = 7,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
            }
        };

        public static ItemRank ShukranisTriumph = new ItemRank

        {
            Item = Legendary.ShukranisTriumph,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 1, PercentUsed = 0.10, Rank = 9,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 56, PercentUsed = 5.66, Rank = 4,},
            }
        };

        public static ItemRank Stormshield = new ItemRank

        {
            Item = Legendary.Stormshield,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 1, PercentUsed = 0.10, Rank = 10,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 6, PercentUsed = 0.61, Rank = 6,},
            }
        };

        public static ItemRank RhenhoFlayer = new ItemRank

        {
            Item = Legendary.RhenhoFlayer,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 273, PercentUsed = 27.89, Rank = 1,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 80, PercentUsed = 8.08, Rank = 4,},
            }
        };

        public static ItemRank StarmetalKukri = new ItemRank

        {
            Item = Legendary.StarmetalKukri,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 232, PercentUsed = 23.70, Rank = 2,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 334, PercentUsed = 33.74, Rank = 1,},
            }
        };

        public static ItemRank TheDaggerOfDarts = new ItemRank

        {
            Item = Legendary.TheDaggerOfDarts,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 25, PercentUsed = 2.55, Rank = 7,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 35, PercentUsed = 3.54, Rank = 5,},
            }
        };

        public static ItemRank GiftOfSilaria = new ItemRank

        {
            Item = Legendary.GiftOfSilaria,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 18, PercentUsed = 1.84, Rank = 8,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 7, PercentUsed = 0.70, Rank = 10,},
            }
        };

        public static ItemRank UtarsRoar = new ItemRank

        {
            Item = Legendary.UtarsRoar,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 13, PercentUsed = 1.33, Rank = 10,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
            }
        };

        public static ItemRank KethryesSplint = new ItemRank

        {
            Item = Legendary.KethryesSplint,
            HardcoreRank = new List<ItemRankData>
            {
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 1, PercentUsed = 0.10, Rank = 10,},
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
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 16, PercentUsed = 1.62, Rank = 6,},
            }
        };

        public static ItemRank DefenderOfWestmarch = new ItemRank

        {
            Item = Legendary.DefenderOfWestmarch,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 58, PercentUsed = 5.94, Rank = 3,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 2, PercentUsed = 0.20, Rank = 9,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 4, PercentUsed = 0.40, Rank = 8,},
            }
        };

        public static ItemRank LidlessWall = new ItemRank

        {
            Item = Legendary.LidlessWall,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 45, PercentUsed = 4.61, Rank = 6,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 1, PercentUsed = 0.10, Rank = 10,},
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 15, PercentUsed = 1.51, Rank = 3,},
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
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 14, PercentUsed = 1.41, Rank = 7,},
            }
        };

        public static ItemRank LastBreath = new ItemRank

        {
            Item = Legendary.LastBreath,
            HardcoreRank = new List<ItemRankData>
            {
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Witchdoctor, SampleSize = 4, PercentUsed = 0.40, Rank = 10,},
            }
        };

        public static ItemRank FirebirdsPlume = new ItemRank

        {
            Item = Legendary.FirebirdsPlume,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 541, PercentUsed = 55.49, Rank = 1,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 867, PercentUsed = 86.96, Rank = 1,},
            }
        };

        public static ItemRank TalRashasGuiseOfWisdom = new ItemRank

        {
            Item = Legendary.TalRashasGuiseOfWisdom,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 149, PercentUsed = 15.28, Rank = 2,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 32, PercentUsed = 3.21, Rank = 3,},
            }
        };

        public static ItemRank StormCrow = new ItemRank

        {
            Item = Legendary.StormCrow,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 21, PercentUsed = 2.15, Rank = 6,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 3, PercentUsed = 0.30, Rank = 7,},
            }
        };

        public static ItemRank FirebirdsPinions = new ItemRank

        {
            Item = Legendary.FirebirdsPinions,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 520, PercentUsed = 53.33, Rank = 1,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 901, PercentUsed = 90.37, Rank = 1,},
            }
        };

        public static ItemRank FirebirdsBreast = new ItemRank

        {
            Item = Legendary.FirebirdsBreast,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 326, PercentUsed = 33.44, Rank = 1,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 584, PercentUsed = 58.58, Rank = 1,},
            }
        };

        public static ItemRank VyrsAstonishingAura = new ItemRank

        {
            Item = Legendary.VyrsAstonishingAura,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 11, PercentUsed = 1.13, Rank = 7,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 4, PercentUsed = 0.40, Rank = 9,},
            }
        };

        public static ItemRank FirebirdsTalons = new ItemRank

        {
            Item = Legendary.FirebirdsTalons,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 354, PercentUsed = 36.31, Rank = 1,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 659, PercentUsed = 66.10, Rank = 1,},
            }
        };

        public static ItemRank VyrsGraspingGauntlets = new ItemRank

        {
            Item = Legendary.VyrsGraspingGauntlets,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 23, PercentUsed = 2.36, Rank = 6,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 11, PercentUsed = 1.10, Rank = 6,},
            }
        };

        public static ItemRank TalRashasBrace = new ItemRank

        {
            Item = Legendary.TalRashasBrace,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 266, PercentUsed = 27.28, Rank = 1,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 56, PercentUsed = 5.62, Rank = 4,},
            }
        };

        public static ItemRank FirebirdsDown = new ItemRank

        {
            Item = Legendary.FirebirdsDown,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 618, PercentUsed = 63.38, Rank = 1,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 896, PercentUsed = 89.87, Rank = 1,},
            }
        };

        public static ItemRank VyrsFantasticFinery = new ItemRank

        {
            Item = Legendary.VyrsFantasticFinery,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 22, PercentUsed = 2.26, Rank = 8,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 9, PercentUsed = 0.90, Rank = 6,},
            }
        };

        public static ItemRank FirebirdsTarsi = new ItemRank

        {
            Item = Legendary.FirebirdsTarsi,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 607, PercentUsed = 62.26, Rank = 1,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 897, PercentUsed = 89.97, Rank = 1,},
            }
        };

        public static ItemRank VyrsSwaggeringStance = new ItemRank

        {
            Item = Legendary.VyrsSwaggeringStance,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 23, PercentUsed = 2.36, Rank = 8,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 12, PercentUsed = 1.20, Rank = 5,},
            }
        };

        public static ItemRank TalRashasAllegiance = new ItemRank

        {
            Item = Legendary.TalRashasAllegiance,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 250, PercentUsed = 25.64, Rank = 1,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 85, PercentUsed = 8.53, Rank = 4,},
            }
        };

        public static ItemRank MoonlightWard = new ItemRank

        {
            Item = Legendary.MoonlightWard,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 71, PercentUsed = 7.28, Rank = 3,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
            }
        };

        public static ItemRank FirebirdsEye = new ItemRank

        {
            Item = Legendary.FirebirdsEye,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 443, PercentUsed = 45.44, Rank = 1,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 328, PercentUsed = 32.90, Rank = 1,},
            }
        };

        public static ItemRank Mirrorball = new ItemRank

        {
            Item = Legendary.Mirrorball,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 172, PercentUsed = 17.64, Rank = 2,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 27, PercentUsed = 2.71, Rank = 2,},
            }
        };

        public static ItemRank TalRashasUnwaveringGlare = new ItemRank

        {
            Item = Legendary.TalRashasUnwaveringGlare,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 89, PercentUsed = 9.13, Rank = 3,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 26, PercentUsed = 2.61, Rank = 3,},
            }
        };

        public static ItemRank Triumvirate = new ItemRank

        {
            Item = Legendary.Triumvirate,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 47, PercentUsed = 4.82, Rank = 4,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 16, PercentUsed = 1.60, Rank = 4,},
            }
        };

        public static ItemRank LightOfGrace = new ItemRank

        {
            Item = Legendary.LightOfGrace,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 27, PercentUsed = 2.77, Rank = 5,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 5, PercentUsed = 0.50, Rank = 5,},
            }
        };

        public static ItemRank MykensBallOfHate = new ItemRank

        {
            Item = Legendary.MykensBallOfHate,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 22, PercentUsed = 2.26, Rank = 6,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 4, PercentUsed = 0.40, Rank = 6,},
            }
        };

        public static ItemRank ChantodosForce = new ItemRank

        {
            Item = Legendary.ChantodosForce,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 21, PercentUsed = 2.15, Rank = 7,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 2, PercentUsed = 0.20, Rank = 9,},
            }
        };

        public static ItemRank TheOculus = new ItemRank

        {
            Item = Legendary.TheOculus,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 6, PercentUsed = 0.62, Rank = 8,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 2, PercentUsed = 0.20, Rank = 8,},
            }
        };

        public static ItemRank WinterFlurry = new ItemRank

        {
            Item = Legendary.WinterFlurry,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 4, PercentUsed = 0.41, Rank = 9,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 3, PercentUsed = 0.30, Rank = 7,},
            }
        };

        public static ItemRank CosmicStrand = new ItemRank

        {
            Item = Legendary.CosmicStrand,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 3, PercentUsed = 0.31, Rank = 10,},
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
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 246, PercentUsed = 25.23, Rank = 1,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 99, PercentUsed = 9.93, Rank = 3,},
            }
        };

        public static ItemRank WandOfWoh = new ItemRank

        {
            Item = Legendary.WandOfWoh,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 65, PercentUsed = 6.67, Rank = 3,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 39, PercentUsed = 3.91, Rank = 4,},
            }
        };

        public static ItemRank GestureOfOrpheus = new ItemRank

        {
            Item = Legendary.GestureOfOrpheus,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 25, PercentUsed = 2.56, Rank = 9,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
            }
        };

        public static ItemRank SloraksMadness = new ItemRank

        {
            Item = Legendary.SloraksMadness,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 24, PercentUsed = 2.46, Rank = 10,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
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
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 2, PercentUsed = 0.20, Rank = 8,},
            }
        };

        public static ItemRank DemonsFlight = new ItemRank

        {
            Item = Legendary.DemonsFlight,
            HardcoreRank = new List<ItemRankData>
            {
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 1, PercentUsed = 0.10, Rank = 7,},
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
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 1, PercentUsed = 0.10, Rank = 10,},
            }
        };

        public static ItemRank TheGrandVizier = new ItemRank

        {
            Item = Legendary.TheGrandVizier,
            HardcoreRank = new List<ItemRankData>
            {
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Wizard, SampleSize = 10, PercentUsed = 1.00, Rank = 7,},
            }
        };

        public static ItemRank HelmOfAkkhan = new ItemRank

        {
            Item = Legendary.HelmOfAkkhan,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 622, PercentUsed = 63.73, Rank = 1,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 439, PercentUsed = 44.16, Rank = 2,},
            }
        };

        public static ItemRank RolandsVisage = new ItemRank

        {
            Item = Legendary.RolandsVisage,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 39, PercentUsed = 4.00, Rank = 4,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 24, PercentUsed = 2.41, Rank = 3,},
            }
        };

        public static ItemRank TheHelmOfRule = new ItemRank

        {
            Item = Legendary.TheHelmOfRule,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 6, PercentUsed = 0.61, Rank = 8,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 2, PercentUsed = 0.20, Rank = 6,},
            }
        };

        public static ItemRank PauldronsOfAkkhan = new ItemRank

        {
            Item = Legendary.PauldronsOfAkkhan,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 784, PercentUsed = 80.33, Rank = 1,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 934, PercentUsed = 93.96, Rank = 1,},
            }
        };

        public static ItemRank RolandsMantle = new ItemRank

        {
            Item = Legendary.RolandsMantle,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 43, PercentUsed = 4.41, Rank = 3,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 29, PercentUsed = 2.92, Rank = 2,},
            }
        };

        public static ItemRank BreastplateOfAkkhan = new ItemRank

        {
            Item = Legendary.BreastplateOfAkkhan,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 741, PercentUsed = 75.92, Rank = 1,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 915, PercentUsed = 92.05, Rank = 1,},
            }
        };

        public static ItemRank RolandsBearing = new ItemRank

        {
            Item = Legendary.RolandsBearing,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 26, PercentUsed = 2.66, Rank = 5,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 23, PercentUsed = 2.31, Rank = 3,},
            }
        };

        public static ItemRank ArmorOfTheKindRegent = new ItemRank

        {
            Item = Legendary.ArmorOfTheKindRegent,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 3, PercentUsed = 0.31, Rank = 10,},
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
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 774, PercentUsed = 79.30, Rank = 1,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 925, PercentUsed = 93.06, Rank = 1,},
            }
        };

        public static ItemRank RolandsGrasp = new ItemRank

        {
            Item = Legendary.RolandsGrasp,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 39, PercentUsed = 4.00, Rank = 3,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 26, PercentUsed = 2.62, Rank = 2,},
            }
        };

        public static ItemRank AngelHairBraid = new ItemRank

        {
            Item = Legendary.AngelHairBraid,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 26, PercentUsed = 2.66, Rank = 8,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 6, PercentUsed = 0.60, Rank = 7,},
            }
        };

        public static ItemRank CuissesOfAkkhan = new ItemRank

        {
            Item = Legendary.CuissesOfAkkhan,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 547, PercentUsed = 56.05, Rank = 1,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 878, PercentUsed = 88.33, Rank = 1,},
            }
        };

        public static ItemRank RolandsDetermination = new ItemRank

        {
            Item = Legendary.RolandsDetermination,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 32, PercentUsed = 3.28, Rank = 4,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 26, PercentUsed = 2.62, Rank = 3,},
            }
        };

        public static ItemRank DemonsPlate = new ItemRank

        {
            Item = Legendary.DemonsPlate,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 1, PercentUsed = 0.10, Rank = 10,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 1, PercentUsed = 0.10, Rank = 10,},
            }
        };

        public static ItemRank SabatonsOfAkkhan = new ItemRank

        {
            Item = Legendary.SabatonsOfAkkhan,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 611, PercentUsed = 62.60, Rank = 1,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 904, PercentUsed = 90.95, Rank = 1,},
            }
        };

        public static ItemRank RolandsStride = new ItemRank

        {
            Item = Legendary.RolandsStride,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 40, PercentUsed = 4.10, Rank = 3,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 28, PercentUsed = 2.82, Rank = 3,},
            }
        };

        public static ItemRank EternalUnion = new ItemRank

        {
            Item = Legendary.EternalUnion,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 35, PercentUsed = 3.59, Rank = 5,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 15, PercentUsed = 1.51, Rank = 6,},
            }
        };

        public static ItemRank Hellskull = new ItemRank

        {
            Item = Legendary.Hellskull,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 454, PercentUsed = 46.52, Rank = 1,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 230, PercentUsed = 23.14, Rank = 2,},
            }
        };

        public static ItemRank UnrelentingPhalanx = new ItemRank

        {
            Item = Legendary.UnrelentingPhalanx,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 139, PercentUsed = 14.24, Rank = 2,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 695, PercentUsed = 69.92, Rank = 1,},
            }
        };

        public static ItemRank HallowedBarricade = new ItemRank

        {
            Item = Legendary.HallowedBarricade,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 57, PercentUsed = 5.84, Rank = 4,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 5, PercentUsed = 0.50, Rank = 7,},
            }
        };

        public static ItemRank Jekangbord = new ItemRank

        {
            Item = Legendary.Jekangbord,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 52, PercentUsed = 5.33, Rank = 5,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 9, PercentUsed = 0.91, Rank = 5,},
            }
        };

        public static ItemRank TheFinalWitness = new ItemRank

        {
            Item = Legendary.TheFinalWitness,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 38, PercentUsed = 3.89, Rank = 7,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 9, PercentUsed = 0.91, Rank = 6,},
            }
        };

        public static ItemRank EberliCharo = new ItemRank

        {
            Item = Legendary.EberliCharo,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 35, PercentUsed = 3.59, Rank = 8,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
            }
        };

        public static ItemRank PiroMarella = new ItemRank

        {
            Item = Legendary.PiroMarella,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 34, PercentUsed = 3.48, Rank = 9,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 11, PercentUsed = 1.11, Rank = 4,},
            }
        };

        public static ItemRank HallowedBulwark = new ItemRank

        {
            Item = Legendary.HallowedBulwark,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 12, PercentUsed = 1.23, Rank = 10,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 3, PercentUsed = 0.30, Rank = 9,},
            }
        };

        public static ItemRank FateOfTheFell = new ItemRank

        {
            Item = Legendary.FateOfTheFell,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 487, PercentUsed = 49.90, Rank = 1,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 211, PercentUsed = 21.23, Rank = 2,},
            }
        };

        public static ItemRank BalefulRemnant = new ItemRank

        {
            Item = Legendary.BalefulRemnant,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 102, PercentUsed = 10.45, Rank = 2,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 29, PercentUsed = 2.92, Rank = 4,},
            }
        };

        public static ItemRank Darklight = new ItemRank

        {
            Item = Legendary.Darklight,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 51, PercentUsed = 5.23, Rank = 5,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 9, PercentUsed = 0.91, Rank = 8,},
            }
        };

        public static ItemRank GyrfalconsFoote = new ItemRank

        {
            Item = Legendary.GyrfalconsFoote,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 35, PercentUsed = 3.59, Rank = 6,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 7, PercentUsed = 0.70, Rank = 9,},
            }
        };

        public static ItemRank HeartSlaughter = new ItemRank

        {
            Item = Legendary.HeartSlaughter,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 20, PercentUsed = 2.05, Rank = 7,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 108, PercentUsed = 10.87, Rank = 3,},
            }
        };

        public static ItemRank SchaefersHammer = new ItemRank

        {
            Item = Legendary.SchaefersHammer,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 19, PercentUsed = 1.95, Rank = 8,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
            }
        };

        public static ItemRank GoldenFlense = new ItemRank

        {
            Item = Legendary.GoldenFlense,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 18, PercentUsed = 1.84, Rank = 9,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 19, PercentUsed = 1.91, Rank = 6,},
            }
        };

        public static ItemRank BladeOfProphecy = new ItemRank

        {
            Item = Legendary.BladeOfProphecy,
            HardcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 12, PercentUsed = 1.23, Rank = 10,},
            },
            SoftcoreRank = new List<ItemRankData>
            {
            }
        };

        public static ItemRank DemonsAileron = new ItemRank

        {
            Item = Legendary.DemonsAileron,
            HardcoreRank = new List<ItemRankData>
            {
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 1, PercentUsed = 0.10, Rank = 6,},
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
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 1, PercentUsed = 0.10, Rank = 8,},
            }
        };

        public static ItemRank BojAnglers = new ItemRank

        {
            Item = Legendary.BojAnglers,
            HardcoreRank = new List<ItemRankData>
            {
            },
            SoftcoreRank = new List<ItemRankData>
            {
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 1, PercentUsed = 0.10, Rank = 10,},
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
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 55, PercentUsed = 5.53, Rank = 6,},
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
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 37, PercentUsed = 3.72, Rank = 9,},
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
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 2, PercentUsed = 0.20, Rank = 10,},
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
                new ItemRankData {Class = ActorClass.Crusader, SampleSize = 19, PercentUsed = 1.91, Rank = 5,},
            }
        };
    }
}