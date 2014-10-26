using System.Collections.Generic;
using System.Linq;
using Trinity.Cache;
using Trinity.Helpers;
using Trinity.Objects;
using Zeta.Game.Internals.Actors;

namespace Trinity.Reference
{
    public class Legendary : FieldCollection<Legendary,Item>
    {
        // Load static version of sets class
        public class Sets : Reference.Sets { }

        // The following items are not listed on the game guide index pages (dafuq blizzard? update your guide already)

        /// <summary>
        /// Helltrapper - 7–10% chance on hit to summon a Spike Trap, Caltrops or Sentry.
        /// </summary>
        public static Item Helltrapper = new Item
        {
            Id = 271914,
            Name = "Helltrapper",
            Quality = ItemQuality.Legendary,
            Slug = "helltrapper-3tfdaj",
            ItemType = ItemType.HandCrossbow,
            BaseType = ItemBaseType.Weapon,
            InternalName = "Unique_HandXBow_102_x1",
            DataUrl = "https://us.battle.net/api/d3/data/item/helltrapper-3tfdaj",
            Url = "https://us.battle.net/d3/en/item/helltrapper-3tfdaj",
            RelativeUrl = "/d3/en/item/helltrapper-3tfdaj",
            IsCrafted = false,
            LegendaryAffix = "7–10% chance on hit to summon a Spike Trap, Caltrops or Sentry",
            SetName = "",
        };

        /// <summary>
        /// Ring of Royal Grandeur - Reduces the number of items needed for set bonuses by 1 (to a minimum of 2).
        /// </summary>
        public static Item RingOfRoyalGrandeur = new Item
        {
            Id = 298094,
            Name = "Ring of Royal Grandeur",
            Quality = ItemQuality.Legendary,
            Slug = "ring-of-royal-grandeur-3qRFop",
            ItemType = ItemType.Ring,
            BaseType = ItemBaseType.Jewelry,
            InternalName = "ring_norm_unique_031",
            DataUrl = "https://us.battle.net/api/d3/data/item/ring-of-royal-grandeur-3qRFop",
            Url = "https://us.battle.net/d3/en/item/ring-of-royal-grandeur-3qRFop",
            RelativeUrl = "/d3/en/item/ring-of-royal-grandeur-3qRFop",
            IsCrafted = false,
            LegendaryAffix = "Reduces the number of items needed for set bonuses by 1 (to a minimum of 2).",
            SetName = "",
        };

        /// <summary>
        /// Sanguinary Vambraces - Chance on being hit to deal 1000% of your Thorns damage to nearby enemies.
        /// </summary>
        public static Item SanguinaryVambraces = new Item
        {
            Id = 298120,
            Name = "Sanguinary Vambraces",
            Quality = ItemQuality.Legendary,
            Slug = "sanguinary-vambraces-1385JU",
            ItemType = ItemType.Bracer,
            BaseType = ItemBaseType.Armor,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/item/sanguinary-vambraces-1385JU",
            Url = "https://us.battle.net/d3/en/item/sanguinary-vambraces-1385JU",
            RelativeUrl = "/d3/en/item/sanguinary-vambraces-1385JU",
            IsCrafted = false,
            LegendaryAffix = "Chance on being hit to deal 1000% of your Thorns damage to nearby enemies.",
            SetName = "",
        };

        /// <summary>
        /// Pride's Fall - Your resource costs are reduced by 30% after not taking damage for 5 seconds.
        /// </summary>
        public static Item PridesFall = new Item
        {
            Id = 298147,
            Name = "Pride's Fall",
            Quality = ItemQuality.Legendary,
            Slug = "prides-fall-pgClp",
            ItemType = ItemType.Helm,
            BaseType = ItemBaseType.Armor,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/item/prides-fall-pgClp",
            Url = "https://us.battle.net/d3/en/item/prides-fall-pgClp",
            RelativeUrl = "/d3/en/item/prides-fall-pgClp",
            IsCrafted = false,
            LegendaryAffix = "Your resource costs are reduced by 30% after not taking damage for 5 seconds.",
            SetName = "",
        };

        /// <summary>
        /// Insatiable Belt - Picking up a Health Globe increases your maximum Life by 5% for 15 seconds, stacking up to 5 times.
        /// </summary>
        public static Item InsatiableBelt = new Item
        {
            Id = 298126,
            Name = "Insatiable Belt",
            Quality = ItemQuality.Legendary,
            Slug = "insatiable-belt-1Yuo9W",
            ItemType = ItemType.Belt,
            BaseType = ItemBaseType.Armor,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/item/insatiable-belt-1Yuo9W",
            Url = "https://us.battle.net/d3/en/item/insatiable-belt-1Yuo9W",
            RelativeUrl = "/d3/en/item/insatiable-belt-1Yuo9W",
            IsCrafted = false,
            LegendaryAffix = "Picking up a Health Globe increases your maximum Life by 5% for 15 seconds, stacking up to 5 times.",
            SetName = "",
        };

        /// <summary>
        /// Salvation - Blocked attacks heal you and your allies for 20–30% of the amount blocked.
        /// </summary>
        public static Item Salvation = new Item
        {
            Id = 299418,
            Name = "Salvation",
            Quality = ItemQuality.Legendary,
            Slug = "salvation-TxMPT",
            ItemType = ItemType.CrusaderShield,
            BaseType = ItemBaseType.None,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/item/salvation-TxMPT",
            Url = "https://us.battle.net/d3/en/item/salvation-TxMPT",
            RelativeUrl = "/d3/en/item/salvation-TxMPT",
            IsCrafted = false,
            LegendaryAffix = "Blocked attacks heal you and your allies for 20–30% of the amount blocked.",
            SetName = "",
        };

        /// <summary>
        /// Gloves of Worship - Shrine effects last for 10 minutes.
        /// </summary>
        public static Item GlovesOfWorship = new Item
        {
            Id = 332344,
            Name = "Gloves of Worship",
            Quality = ItemQuality.Legendary,
            Slug = "gloves-of-worship-FA37P",
            ItemType = ItemType.Gloves,
            BaseType = ItemBaseType.Armor,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/item/gloves-of-worship-FA37P",
            Url = "https://us.battle.net/d3/en/item/gloves-of-worship-FA37P",
            RelativeUrl = "/d3/en/item/gloves-of-worship-FA37P",
            IsCrafted = false,
            LegendaryAffix = "Shrine effects last for 10 minutes.",
            SetName = "",
        };

        /// <summary>
        /// Avarice Band - Each time you pick up gold, increase your Gold and Health Pickup radius by 1 yard for 10 seconds, stacking up to 30 times.
        /// </summary>
        public static Item AvariceBand = new Item
        {
            Id = 298095,
            Name = "Avarice Band",
            Quality = ItemQuality.Legendary,
            Slug = "avarice-band-3qROK2",
            ItemType = ItemType.Ring,
            BaseType = ItemBaseType.Armor,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/item/avarice-band-3qROK2",
            Url = "https://us.battle.net/d3/en/item/avarice-band-3qROK2",
            RelativeUrl = "/d3/en/item/avarice-band-3qROK2",
            IsCrafted = false,
            LegendaryAffix = "Each time you pick up gold, increase your Gold and Health Pickup radius by 1 yard for 10 seconds, stacking up to 30 times.",
            SetName = "",
        };

        /// <summary>
        /// Mad Monarch's Scepter - After killing 10 enemies, you release a Poison Nova that deals 1050–1400% weapon damage as Poison to enemies within 30 yards.
        /// </summary>
        public static Item MadMonarchsScepter = new Item
        {
            Id = 271663,
            Name = "Mad Monarch's Scepter",
            Quality = ItemQuality.Legendary,
            Slug = "mad-monarchs-scepter-2F34bH",
            ItemType = ItemType.Mace,
            BaseType = ItemBaseType.Weapon,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/item/mad-monarchs-scepter-2F34bH",
            Url = "https://us.battle.net/d3/en/item/mad-monarchs-scepter-2F34bH",
            RelativeUrl = "/d3/en/item/mad-monarchs-scepter-2F34bH",
            IsCrafted = false,
            LegendaryAffix = "After killing 10 enemies, you release a Poison Nova that deals 1050–1400% weapon damage as Poison to enemies within 30 yards.",
            SetName = "",
        };

        /// <summary>
        /// Golden Gorget of Leoric - After earning a massacre bonus, 4–6 Skeletons are summoned to fight by your side for 10 seconds.
        /// </summary>
        public static Item GoldenGorgetOfLeoric = new Item
        {
            Id = 298052,
            Name = "Golden Gorget of Leoric",
            Quality = ItemQuality.Legendary,
            Slug = "golden-gorget-of-leoric-1I0CCL",
            ItemType = ItemType.Amulet,
            BaseType = ItemBaseType.Armor,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/item/golden-gorget-of-leoric-1I0CCL",
            Url = "https://us.battle.net/d3/en/item/golden-gorget-of-leoric-1I0CCL",
            RelativeUrl = "/d3/en/item/golden-gorget-of-leoric-1I0CCL",
            IsCrafted = false,
            LegendaryAffix = "After earning a massacre bonus, 4–6 Skeletons are summoned to fight by your side for 10 seconds.",
            SetName = "",
        };

        /// <summary>
        /// Illusory Boots - You may move unhindered through enemies.
        /// </summary>
        public static Item IllusoryBoots = new Item
        {
            Id = 332342,
            Name = "Illusory Boots",
            Quality = ItemQuality.Legendary,
            Slug = "illusory-boots-29WYI0",
            ItemType = ItemType.Boots,
            BaseType = ItemBaseType.Armor,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/item/illusory-boots-29WYI0",
            Url = "https://us.battle.net/d3/en/item/illusory-boots-29WYI0",
            RelativeUrl = "/d3/en/item/illusory-boots-29WYI0",
            IsCrafted = false,
            LegendaryAffix = "You may move unhindered through enemies.",
            SetName = "",
        };

        /// <summary>
        /// Pauldrons of the Skeleton King - When receiving fatal damage, there is a chance that you are instead restored to 25% of maximum Life and cause nearby enemies to flee in fear.
        /// </summary>
        public static Item PauldronsOfTheSkeletonKing = new Item
        {
            Id = 298164,
            Name = "Pauldrons of the Skeleton King",
            Quality = ItemQuality.Legendary,
            Slug = "pauldrons-of-the-skeleton-king-2AGH0r",
            ItemType = ItemType.Shoulder,
            BaseType = ItemBaseType.Armor,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/item/pauldrons-of-the-skeleton-king-2AGH0r",
            Url = "https://us.battle.net/d3/en/item/pauldrons-of-the-skeleton-king-2AGH0r",
            RelativeUrl = "/d3/en/item/pauldrons-of-the-skeleton-king-2AGH0r",
            IsCrafted = false,
            LegendaryAffix = "When receiving fatal damage, there is a chance that you are instead restored to 25% of maximum Life and cause nearby enemies to flee in fear.",
            SetName = "",
        };


        // AUTO-GENERATED on Mon, 08 Sep 2014 20:37:19 GMT


        /// <summary>
        /// Split Tusk 
        /// </summary>
        public static Item SplitTusk = new Item
        {
            Id = 221167,
            Name = "Split Tusk",
            Quality = ItemQuality.Legendary,
            Slug = "split-tusk",
            ItemType = ItemType.VoodooMask,
            BaseType = ItemBaseType.Armor,
            InternalName = "voodooMask_norm_unique_03",
            DataUrl = "https://us.battle.net/api/d3/data/item/split-tusk",
            Url = "https://us.battle.net/d3/en/item/split-tusk",
            RelativeUrl = "/d3/en/item/split-tusk",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Quetzalcoatl Locust Swarm and Haunt now deal their damage in half of the normal duration.
        /// </summary>
        public static Item Quetzalcoatl = new Item
        {
            Id = 204136,
            Name = "Quetzalcoatl",
            Quality = ItemQuality.Legendary,
            Slug = "quetzalcoatl",
            ItemType = ItemType.VoodooMask,
            BaseType = ItemBaseType.Armor,
            InternalName = "voodooMask_norm_base_05",
            DataUrl = "https://us.battle.net/api/d3/data/item/quetzalcoatl",
            Url = "https://us.battle.net/d3/en/item/quetzalcoatl",
            RelativeUrl = "/d3/en/item/quetzalcoatl",
            IsCrafted = false,
            LegendaryAffix = "Locust Swarm and Haunt now deal their damage in half of the normal duration.",
            SetName = "",
        };

        /// <summary>
        /// Carnevil Your Fetishes shoot a Poison Dart every time you do.
        /// </summary>
        public static Item Carnevil = new Item
        {
            Id = 299442,
            Name = "Carnevil",
            Quality = ItemQuality.Legendary,
            Slug = "carnevil",
            ItemType = ItemType.VoodooMask,
            BaseType = ItemBaseType.Armor,
            InternalName = "voodoomask_norm_unique_07",
            DataUrl = "https://us.battle.net/api/d3/data/item/carnevil",
            Url = "https://us.battle.net/d3/en/item/carnevil",
            RelativeUrl = "/d3/en/item/carnevil",
            IsCrafted = false,
            LegendaryAffix = "Your Fetishes shoot a Poison Dart every time you do.",
            SetName = "",
        };

        /// <summary>
        /// Mask of Jeram Pets deal 75% more damage.
        /// </summary>
        public static Item MaskOfJeram = new Item
        {
            Id = 299443,
            Name = "Mask of Jeram",
            Quality = ItemQuality.Legendary,
            Slug = "mask-of-jeram",
            ItemType = ItemType.VoodooMask,
            BaseType = ItemBaseType.Armor,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/item/mask-of-jeram",
            Url = "https://us.battle.net/d3/en/item/mask-of-jeram",
            RelativeUrl = "/d3/en/item/mask-of-jeram",
            IsCrafted = false,
            LegendaryAffix = "Pets deal 75% more damage.",
            SetName = "",
        };

        /// <summary>
        /// The Grin Reaper Chance to summon horrific Mimics when attacking.
        /// </summary>
        public static Item TheGrinReaper = new Item
        {
            Id = 221166,
            Name = "The Grin Reaper",
            Quality = ItemQuality.Legendary,
            Slug = "the-grin-reaper",
            ItemType = ItemType.VoodooMask,
            BaseType = ItemBaseType.Armor,
            InternalName = "voodooMask_norm_unique_01",
            DataUrl = "https://us.battle.net/api/d3/data/item/the-grin-reaper",
            Url = "https://us.battle.net/d3/en/item/the-grin-reaper",
            RelativeUrl = "/d3/en/item/the-grin-reaper",
            IsCrafted = false,
            LegendaryAffix = "Chance to summon horrific Mimics when attacking.",
            SetName = "",
        };

        /// <summary>
        /// Tiklandian Visage Horrify causes you to Fear and Root enemies around you for 6–8 seconds.
        /// </summary>
        public static Item TiklandianVisage = new Item
        {
            Id = 221382,
            Name = "Tiklandian Visage",
            Quality = ItemQuality.Legendary,
            Slug = "tiklandian-visage",
            ItemType = ItemType.VoodooMask,
            BaseType = ItemBaseType.Armor,
            InternalName = "voodooMask_norm_unique_06",
            DataUrl = "https://us.battle.net/api/d3/data/item/tiklandian-visage",
            Url = "https://us.battle.net/d3/en/item/tiklandian-visage",
            RelativeUrl = "/d3/en/item/tiklandian-visage",
            IsCrafted = false,
            LegendaryAffix = "Horrify causes you to Fear and Root enemies around you for 6–8 seconds.",
            SetName = "",
        };

        /// <summary>
        /// Visage of Giyua Summon a Fetish Army after you kill 2 Elites.
        /// </summary>
        public static Item VisageOfGiyua = new Item
        {
            Id = 221168,
            Name = "Visage of Giyua",
            Quality = ItemQuality.Legendary,
            Slug = "visage-of-giyua",
            ItemType = ItemType.VoodooMask,
            BaseType = ItemBaseType.Armor,
            InternalName = "voodooMask_norm_unique_05",
            DataUrl = "https://us.battle.net/api/d3/data/item/visage-of-giyua",
            Url = "https://us.battle.net/d3/en/item/visage-of-giyua",
            RelativeUrl = "/d3/en/item/visage-of-giyua",
            IsCrafted = false,
            LegendaryAffix = "Summon a Fetish Army after you kill 2 Elites.",
            SetName = "",
        };

        /// <summary>
        /// Zunimassa's Vision 
        /// </summary>
        public static Item ZunimassasVision = new Item
        {
            Id = 221202,
            Name = "Zunimassa's Vision",
            Quality = ItemQuality.Legendary,
            Slug = "zunimassas-vision",
            ItemType = ItemType.VoodooMask,
            BaseType = ItemBaseType.Armor,
            InternalName = "voodooMask_norm_unique_04",
            DataUrl = "https://us.battle.net/api/d3/data/item/zunimassas-vision",
            Url = "https://us.battle.net/d3/en/item/zunimassas-vision",
            RelativeUrl = "/d3/en/item/zunimassas-vision",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Zunimassa's Haunt",
        };

        /// <summary>
        /// See No Evil 
        /// </summary>
        public static Item SeeNoEvil = new Item
        {
            Id = 222171,
            Name = "See No Evil",
            Quality = ItemQuality.Legendary,
            Slug = "see-no-evil",
            ItemType = ItemType.SpiritStone,
            BaseType = ItemBaseType.Armor,
            InternalName = "spiritStone_norm_unique_13",
            DataUrl = "https://us.battle.net/api/d3/data/item/see-no-evil",
            Url = "https://us.battle.net/d3/en/item/see-no-evil",
            RelativeUrl = "/d3/en/item/see-no-evil",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Gyana Na Kashu Lashing Tail Kick releases a piercing fireball that deals 525–700% weapon damage as Fire to enemies within 10 yards on impact.
        /// </summary>
        public static Item GyanaNaKashu = new Item
        {
            Id = 222169,
            Name = "Gyana Na Kashu",
            Quality = ItemQuality.Legendary,
            Slug = "gyana-na-kashu",
            ItemType = ItemType.SpiritStone,
            BaseType = ItemBaseType.Armor,
            InternalName = "spiritStone_norm_unique_05",
            DataUrl = "https://us.battle.net/api/d3/data/item/gyana-na-kashu",
            Url = "https://us.battle.net/d3/en/item/gyana-na-kashu",
            RelativeUrl = "/d3/en/item/gyana-na-kashu",
            IsCrafted = false,
            LegendaryAffix = "Lashing Tail Kick releases a piercing fireball that deals 525–700% weapon damage as Fire to enemies within 10 yards on impact.",
            SetName = "",
        };

        /// <summary>
        /// Erlang Shen 
        /// </summary>
        public static Item ErlangShen = new Item
        {
            Id = 222173,
            Name = "Erlang Shen",
            Quality = ItemQuality.Legendary,
            Slug = "erlang-shen",
            ItemType = ItemType.SpiritStone,
            BaseType = ItemBaseType.Armor,
            InternalName = "spiritStone_norm_unique_07",
            DataUrl = "https://us.battle.net/api/d3/data/item/erlang-shen",
            Url = "https://us.battle.net/d3/en/item/erlang-shen",
            RelativeUrl = "/d3/en/item/erlang-shen",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// The Mind's Eye Inner Sanctuary increases Spirit Regeneration per second by 10–15.
        /// </summary>
        public static Item TheMindsEye = new Item
        {
            Id = 222172,
            Name = "The Mind's Eye",
            Quality = ItemQuality.Legendary,
            Slug = "the-minds-eye",
            ItemType = ItemType.SpiritStone,
            BaseType = ItemBaseType.Armor,
            InternalName = "spiritstone_norm_unique_06",
            DataUrl = "https://us.battle.net/api/d3/data/item/the-minds-eye",
            Url = "https://us.battle.net/d3/en/item/the-minds-eye",
            RelativeUrl = "/d3/en/item/the-minds-eye",
            IsCrafted = false,
            LegendaryAffix = "Inner Sanctuary increases Spirit Regeneration per second by 10–15.",
            SetName = "",
        };

        /// <summary>
        /// Eye of Peshkov Reduce the cooldown of Breath of Heaven by 38–50%.
        /// </summary>
        public static Item EyeOfPeshkov = new Item
        {
            Id = 299464,
            Name = "Eye of Peshkov",
            Quality = ItemQuality.Legendary,
            Slug = "eye-of-peshkov",
            ItemType = ItemType.SpiritStone,
            BaseType = ItemBaseType.Armor,
            InternalName = "spiritstone_norm_unique_16",
            DataUrl = "https://us.battle.net/api/d3/data/item/eye-of-peshkov",
            Url = "https://us.battle.net/d3/en/item/eye-of-peshkov",
            RelativeUrl = "/d3/en/item/eye-of-peshkov",
            IsCrafted = false,
            LegendaryAffix = "Reduce the cooldown of Breath of Heaven by 38–50%.",
            SetName = "",
        };

        /// <summary>
        /// Kekegi's Unbreakable Spirit Damaging enemies has a chance to grant you an effect that removes the Spirit cost of your abilities for 2–4 seconds.
        /// </summary>
        public static Item KekegisUnbreakableSpirit = new Item
        {
            Id = 299461,
            Name = "Kekegi's Unbreakable Spirit",
            Quality = ItemQuality.Legendary,
            Slug = "kekegis-unbreakable-spirit",
            ItemType = ItemType.SpiritStone,
            BaseType = ItemBaseType.Armor,
            InternalName = "spiritstone_norm_unique_15",
            DataUrl = "https://us.battle.net/api/d3/data/item/kekegis-unbreakable-spirit",
            Url = "https://us.battle.net/d3/en/item/kekegis-unbreakable-spirit",
            RelativeUrl = "/d3/en/item/kekegis-unbreakable-spirit",
            IsCrafted = false,
            LegendaryAffix = "Damaging enemies has a chance to grant you an effect that removes the Spirit cost of your abilities for 2–4 seconds.",
            SetName = "",
        };

        /// <summary>
        /// The Laws of Seph Using Blinding Flash restores 125–165 Spirit.
        /// </summary>
        public static Item TheLawsOfSeph = new Item
        {
            Id = 299454,
            Name = "The Laws of Seph",
            Quality = ItemQuality.Legendary,
            Slug = "the-laws-of-seph",
            ItemType = ItemType.SpiritStone,
            BaseType = ItemBaseType.Armor,
            InternalName = "spiritstone_norm_unique_14",
            DataUrl = "https://us.battle.net/api/d3/data/item/the-laws-of-seph",
            Url = "https://us.battle.net/d3/en/item/the-laws-of-seph",
            RelativeUrl = "/d3/en/item/the-laws-of-seph",
            IsCrafted = false,
            LegendaryAffix = "Using Blinding Flash restores 125–165 Spirit.",
            SetName = "",
        };

        /// <summary>
        /// Bezoar Stone 
        /// </summary>
        public static Item BezoarStone = new Item
        {
            Id = 222306,
            Name = "Bezoar Stone",
            Quality = ItemQuality.Legendary,
            Slug = "bezoar-stone",
            ItemType = ItemType.SpiritStone,
            BaseType = ItemBaseType.Armor,
            InternalName = "spiritStone_norm_unique_11",
            DataUrl = "https://us.battle.net/api/d3/data/item/bezoar-stone",
            Url = "https://us.battle.net/d3/en/item/bezoar-stone",
            RelativeUrl = "/d3/en/item/bezoar-stone",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// The Eye of the Storm 
        /// </summary>
        public static Item TheEyeOfTheStorm = new Item
        {
            Id = 222170,
            Name = "The Eye of the Storm",
            Quality = ItemQuality.Legendary,
            Slug = "the-eye-of-the-storm",
            ItemType = ItemType.SpiritStone,
            BaseType = ItemBaseType.Armor,
            InternalName = "spiritStone_norm_unique_02",
            DataUrl = "https://us.battle.net/api/d3/data/item/the-eye-of-the-storm",
            Url = "https://us.battle.net/d3/en/item/the-eye-of-the-storm",
            RelativeUrl = "/d3/en/item/the-eye-of-the-storm",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Madstone Your Seven-Sided Strike applies Exploding Palm.
        /// </summary>
        public static Item Madstone = new Item
        {
            Id = 221572,
            Name = "Madstone",
            Quality = ItemQuality.Legendary,
            Slug = "madstone",
            ItemType = ItemType.SpiritStone,
            BaseType = ItemBaseType.Armor,
            InternalName = "spiritStone_norm_unique_03",
            DataUrl = "https://us.battle.net/api/d3/data/item/madstone",
            Url = "https://us.battle.net/d3/en/item/madstone",
            RelativeUrl = "/d3/en/item/madstone",
            IsCrafted = false,
            LegendaryAffix = "Your Seven-Sided Strike applies Exploding Palm.",
            SetName = "",
        };

        /// <summary>
        /// Tzo Krin's Gaze Wave of Light is now cast at your enemy.
        /// </summary>
        public static Item TzoKrinsGaze = new Item
        {
            Id = 222305,
            Name = "Tzo Krin's Gaze",
            Quality = ItemQuality.Legendary,
            Slug = "tzo-krins-gaze",
            ItemType = ItemType.SpiritStone,
            BaseType = ItemBaseType.Armor,
            InternalName = "spiritStone_norm_unique_12",
            DataUrl = "https://us.battle.net/api/d3/data/item/tzo-krins-gaze",
            Url = "https://us.battle.net/d3/en/item/tzo-krins-gaze",
            RelativeUrl = "/d3/en/item/tzo-krins-gaze",
            IsCrafted = false,
            LegendaryAffix = "Wave of Light is now cast at your enemy.",
            SetName = "",
        };

        /// <summary>
        /// Inna's Radiance 
        /// </summary>
        public static Item InnasRadiance = new Item
        {
            Id = 222307,
            Name = "Inna's Radiance",
            Quality = ItemQuality.Legendary,
            Slug = "innas-radiance",
            ItemType = ItemType.SpiritStone,
            BaseType = ItemBaseType.Armor,
            InternalName = "spiritStone_norm_unique_08",
            DataUrl = "https://us.battle.net/api/d3/data/item/innas-radiance",
            Url = "https://us.battle.net/d3/en/item/innas-radiance",
            RelativeUrl = "/d3/en/item/innas-radiance",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Inna's Mantra",
        };

        /// <summary>
        /// Homing Pads Your Town Portal is no longer interrupted by taking damage. While casting Town Portal you gain a protective bubble that reduces damage taken by 50–65%.
        /// </summary>
        public static Item HomingPads = new Item
        {
            Id = 198573,
            Name = "Homing Pads",
            Quality = ItemQuality.Legendary,
            Slug = "homing-pads",
            ItemType = ItemType.Shoulder,
            BaseType = ItemBaseType.Armor,
            InternalName = "shoulderPads_norm_unique_01",
            DataUrl = "https://us.battle.net/api/d3/data/item/homing-pads",
            Url = "https://us.battle.net/d3/en/item/homing-pads",
            RelativeUrl = "/d3/en/item/homing-pads",
            IsCrafted = false,
            LegendaryAffix = "Your Town Portal is no longer interrupted by taking damage. While casting Town Portal you gain a protective bubble that reduces damage taken by 50–65%.",
            SetName = "",
        };

        /// <summary>
        /// Born's Impunity 
        /// </summary>
        public static Item BornsImpunity = new Item
        {
            Id = 0,
            Name = "Born's Impunity",
            Quality = ItemQuality.Legendary,
            Slug = "borns-pauldrons",
            ItemType = ItemType.Shoulder,
            BaseType = ItemBaseType.Armor,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/borns-pauldrons",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/borns-pauldrons",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/borns-pauldrons",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "Born's Defiance",
        };

        /// <summary>
        /// Death Watch Mantle 15–35% chance to explode in a fan of knives for 200% weapon damage when hit.
        /// </summary>
        public static Item DeathWatchMantle = new Item
        {
            Id = 200310,
            Name = "Death Watch Mantle",
            Quality = ItemQuality.Legendary,
            Slug = "death-watch-mantle",
            ItemType = ItemType.Shoulder,
            BaseType = ItemBaseType.Armor,
            InternalName = "shoulderPads_norm_unique_02",
            DataUrl = "https://us.battle.net/api/d3/data/item/death-watch-mantle",
            Url = "https://us.battle.net/d3/en/item/death-watch-mantle",
            RelativeUrl = "/d3/en/item/death-watch-mantle",
            IsCrafted = false,
            LegendaryAffix = "15–35% chance to explode in a fan of knives for 200% weapon damage when hit.",
            SetName = "",
        };

        /// <summary>
        /// Profane Pauldrons 
        /// </summary>
        public static Item ProfanePauldrons = new Item
        {
            Id = 298158,
            Name = "Profane Pauldrons",
            Quality = ItemQuality.Legendary,
            Slug = "profane-pauldrons",
            ItemType = ItemType.Shoulder,
            BaseType = ItemBaseType.Armor,
            InternalName = "shoulderpads_norm_unique_08",
            DataUrl = "https://us.battle.net/api/d3/data/item/profane-pauldrons",
            Url = "https://us.battle.net/d3/en/item/profane-pauldrons",
            RelativeUrl = "/d3/en/item/profane-pauldrons",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Spaulders of Zakara Your items become indestructible.
        /// </summary>
        public static Item SpauldersOfZakara = new Item
        {
            Id = 298163,
            Name = "Spaulders of Zakara",
            Quality = ItemQuality.Legendary,
            Slug = "spaulders-of-zakara",
            ItemType = ItemType.Shoulder,
            BaseType = ItemBaseType.Armor,
            InternalName = "shoulderpads_norm_unique_09",
            DataUrl = "https://us.battle.net/api/d3/data/item/spaulders-of-zakara",
            Url = "https://us.battle.net/d3/en/item/spaulders-of-zakara",
            RelativeUrl = "/d3/en/item/spaulders-of-zakara",
            IsCrafted = false,
            LegendaryAffix = "Your items become indestructible.",
            SetName = "",
        };

        /// <summary>
        /// Aughild's Reign 
        /// </summary>
        public static Item AughildsReign = new Item
        {
            Id = 0,
            Name = "Aughild's Reign",
            Quality = ItemQuality.Legendary,
            Slug = "aughilds-triumph",
            ItemType = ItemType.Shoulder,
            BaseType = ItemBaseType.Armor,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/aughilds-triumph",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/aughilds-triumph",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/aughilds-triumph",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "Aughild's Victory",
        };

        /// <summary>
        /// Asheara's Guard 
        /// </summary>
        public static Item AshearasGuard = new Item
        {
            Id = 0,
            Name = "Asheara's Guard",
            Quality = ItemQuality.Legendary,
            Slug = "ashearas-vigilance",
            ItemType = ItemType.Shoulder,
            BaseType = ItemBaseType.Armor,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/ashearas-vigilance",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/ashearas-vigilance",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/ashearas-vigilance",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "Asheara's Uniform",
        };

        /// <summary>
        /// Vile Ward Furious Charge deals 30–35% increased damage for every enemy hit while charging.
        /// </summary>
        public static Item VileWard = new Item
        {
            Id = 201325,
            Name = "Vile Ward",
            Quality = ItemQuality.Legendary,
            Slug = "vile-ward",
            ItemType = ItemType.Shoulder,
            BaseType = ItemBaseType.Armor,
            InternalName = "shoulderPads_norm_unique_03",
            DataUrl = "https://us.battle.net/api/d3/data/item/vile-ward",
            Url = "https://us.battle.net/d3/en/item/vile-ward",
            RelativeUrl = "/d3/en/item/vile-ward",
            IsCrafted = false,
            LegendaryAffix = "Furious Charge deals 30–35% increased damage for every enemy hit while charging.",
            SetName = "",
        };

        /// <summary>
        /// Seven Sins 
        /// </summary>
        public static Item SevenSins = new Item
        {
            Id = 0,
            Name = "Seven Sins",
            Quality = ItemQuality.Legendary,
            Slug = "seven-sins",
            ItemType = ItemType.Shoulder,
            BaseType = ItemBaseType.Armor,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/seven-sins",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/seven-sins",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/seven-sins",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Demon's Flight 
        /// </summary>
        public static Item DemonsFlight = new Item
        {
            Id = 0,
            Name = "Demon's Flight",
            Quality = ItemQuality.Legendary,
            Slug = "demons-wings",
            ItemType = ItemType.Shoulder,
            BaseType = ItemBaseType.Armor,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/demons-wings",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/demons-wings",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/demons-wings",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "Demon's Skin",
        };

        /// <summary>
        /// Burden of the Invoker 
        /// </summary>
        public static Item BurdenOfTheInvoker = new Item
        {
            Id = 335029,
            Name = "Burden of the Invoker",
            Quality = ItemQuality.Legendary,
            Slug = "burden-of-the-invoker",
            ItemType = ItemType.Shoulder,
            BaseType = ItemBaseType.Armor,
            InternalName = "shoulderpads_norm_set_12",
            DataUrl = "https://us.battle.net/api/d3/data/item/burden-of-the-invoker",
            Url = "https://us.battle.net/d3/en/item/burden-of-the-invoker",
            RelativeUrl = "/d3/en/item/burden-of-the-invoker",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Thorns of the Invoker",
        };

        /// <summary>
        /// Firebird's Pinions 
        /// </summary>
        public static Item FirebirdsPinions = new Item
        {
            Id = 358792,
            Name = "Firebird's Pinions",
            Quality = ItemQuality.Legendary,
            Slug = "firebirds-pinions",
            ItemType = ItemType.Shoulder,
            BaseType = ItemBaseType.Armor,
            InternalName = "shoulderpads_norm_set_06",
            DataUrl = "https://us.battle.net/api/d3/data/item/firebirds-pinions",
            Url = "https://us.battle.net/d3/en/item/firebirds-pinions",
            RelativeUrl = "/d3/en/item/firebirds-pinions",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Firebird's Finery",
        };

        /// <summary>
        /// Helltooth Mantle 
        /// </summary>
        public static Item HelltoothMantle = new Item
        {
            Id = 340525,
            Name = "Helltooth Mantle",
            Quality = ItemQuality.Legendary,
            Slug = "helltooth-mantle",
            ItemType = ItemType.Shoulder,
            BaseType = ItemBaseType.Armor,
            InternalName = "shoulderpads_norm_set_16",
            DataUrl = "https://us.battle.net/api/d3/data/item/helltooth-mantle",
            Url = "https://us.battle.net/d3/en/item/helltooth-mantle",
            RelativeUrl = "/d3/en/item/helltooth-mantle",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Helltooth Harness",
        };

        /// <summary>
        /// Jade Harvester's Joy 
        /// </summary>
        public static Item JadeHarvestersJoy = new Item
        {
            Id = 338042,
            Name = "Jade Harvester's Joy",
            Quality = ItemQuality.Legendary,
            Slug = "jade-harvesters-joy",
            ItemType = ItemType.Shoulder,
            BaseType = ItemBaseType.Armor,
            InternalName = "shoulderpads_norm_set_09",
            DataUrl = "https://us.battle.net/api/d3/data/item/jade-harvesters-joy",
            Url = "https://us.battle.net/d3/en/item/jade-harvesters-joy",
            RelativeUrl = "/d3/en/item/jade-harvesters-joy",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Raiment of the Jade Harvester",
        };

        /// <summary>
        /// Mantle of the Upside-Down Sinners 
        /// </summary>
        public static Item MantleOfTheUpsidedownSinners = new Item
        {
            Id = 0,
            Name = "Mantle of the Upside-Down Sinners",
            Quality = ItemQuality.Legendary,
            Slug = "mantle-of-the-upsidedown-sinners",
            ItemType = ItemType.Shoulder,
            BaseType = ItemBaseType.Armor,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/item/mantle-of-the-upsidedown-sinners",
            Url = "https://us.battle.net/d3/en/item/mantle-of-the-upsidedown-sinners",
            RelativeUrl = "/d3/en/item/mantle-of-the-upsidedown-sinners",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Raiment of a Thousand Storms",
        };

        /// <summary>
        /// Marauder's Spines 
        /// </summary>
        public static Item MaraudersSpines = new Item
        {
            Id = 336996,
            Name = "Marauder's Spines",
            Quality = ItemQuality.Legendary,
            Slug = "marauders-spines",
            ItemType = ItemType.Shoulder,
            BaseType = ItemBaseType.Armor,
            InternalName = "shoulderpads_norm_set_07",
            DataUrl = "https://us.battle.net/api/d3/data/item/marauders-spines",
            Url = "https://us.battle.net/d3/en/item/marauders-spines",
            RelativeUrl = "/d3/en/item/marauders-spines",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Embodiment of the Marauder",
        };

        /// <summary>
        /// Pauldrons of Akkhan 
        /// </summary>
        public static Item PauldronsOfAkkhan = new Item
        {
            Id = 358801,
            Name = "Pauldrons of Akkhan",
            Quality = ItemQuality.Legendary,
            Slug = "pauldrons-of-akkhan",
            ItemType = ItemType.Shoulder,
            BaseType = ItemBaseType.Armor,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/item/pauldrons-of-akkhan",
            Url = "https://us.battle.net/d3/en/item/pauldrons-of-akkhan",
            RelativeUrl = "/d3/en/item/pauldrons-of-akkhan",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Armor of Akkhan",
        };

        /// <summary>
        /// Raekor's Burden 
        /// </summary>
        public static Item RaekorsBurden = new Item
        {
            Id = 336989,
            Name = "Raekor's Burden",
            Quality = ItemQuality.Legendary,
            Slug = "raekors-burden",
            ItemType = ItemType.Shoulder,
            BaseType = ItemBaseType.Armor,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/item/raekors-burden",
            Url = "https://us.battle.net/d3/en/item/raekors-burden",
            RelativeUrl = "/d3/en/item/raekors-burden",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "The Legacy of Raekor",
        };

        /// <summary>
        /// Roland's Mantle 
        /// </summary>
        public static Item RolandsMantle = new Item
        {
            Id = 404699,
            Name = "Roland's Mantle",
            Quality = ItemQuality.Legendary,
            Slug = "rolands-mantle",
            ItemType = ItemType.Shoulder,
            BaseType = ItemBaseType.Armor,
            InternalName = "p1_shoulderPads_norm_set_01",
            DataUrl = "https://us.battle.net/api/d3/data/item/rolands-mantle",
            Url = "https://us.battle.net/d3/en/item/rolands-mantle",
            RelativeUrl = "/d3/en/item/rolands-mantle",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Roland's Legacy",
        };

        /// <summary>
        /// Spires of the Earth 
        /// </summary>
        public static Item SpiresOfTheEarth = new Item
        {
            Id = 340526,
            Name = "Spires of the Earth",
            Quality = ItemQuality.Legendary,
            Slug = "spires-of-the-earth",
            ItemType = ItemType.Shoulder,
            BaseType = ItemBaseType.Armor,
            InternalName = "shoulderpads_norm_set_15",
            DataUrl = "https://us.battle.net/api/d3/data/item/spires-of-the-earth",
            Url = "https://us.battle.net/d3/en/item/spires-of-the-earth",
            RelativeUrl = "/d3/en/item/spires-of-the-earth",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Might of the Earth",
        };

        /// <summary>
        /// Sunwuko's Balance 
        /// </summary>
        public static Item SunwukosBalance = new Item
        {
            Id = 336175,
            Name = "Sunwuko's Balance",
            Quality = ItemQuality.Legendary,
            Slug = "sunwukos-balance",
            ItemType = ItemType.Shoulder,
            BaseType = ItemBaseType.Armor,
            InternalName = "shoulderpads_norm_set_11",
            DataUrl = "https://us.battle.net/api/d3/data/item/sunwukos-balance",
            Url = "https://us.battle.net/d3/en/item/sunwukos-balance",
            RelativeUrl = "/d3/en/item/sunwukos-balance",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Monkey King's Garb",
        };

        /// <summary>
        /// Corruption 
        /// </summary>
        public static Item Corruption = new Item
        {
            Id = 223619,
            Name = "Corruption",
            Quality = ItemQuality.Legendary,
            Slug = "corruption",
            ItemType = ItemType.Shoulder,
            BaseType = ItemBaseType.Armor,
            InternalName = "shoulderPads_norm_unique_04",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/corruption",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/corruption",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/corruption",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Asheara's Custodian 
        /// </summary>
        public static Item AshearasCustodian = new Item
        {
            Id = 225132,
            Name = "Asheara's Custodian",
            Quality = ItemQuality.Legendary,
            Slug = "ashearas-custodian",
            ItemType = ItemType.Shoulder,
            BaseType = ItemBaseType.Armor,
            InternalName = "shoulderPads_norm_unique_07",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/ashearas-custodian",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/ashearas-custodian",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/ashearas-custodian",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "Asheara's Vestments",
        };

        /// <summary>
        /// Aughild's Power 
        /// </summary>
        public static Item AughildsPower = new Item
        {
            Id = 299443,
            Name = "Aughild's Power",
            Quality = ItemQuality.Legendary,
            Slug = "aughilds-power",
            ItemType = ItemType.Shoulder,
            BaseType = ItemBaseType.Armor,
            InternalName = "voodoomask_norm_unique_08",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/aughilds-power",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/aughilds-power",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/aughilds-power",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "Aughild's Authority",
        };

        /// <summary>
        /// Born's Privilege 
        /// </summary>
        public static Item BornsPrivilege = new Item
        {
            Id = 0,
            Name = "Born's Privilege",
            Quality = ItemQuality.Legendary,
            Slug = "borns-privilege",
            ItemType = ItemType.Shoulder,
            BaseType = ItemBaseType.Armor,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/borns-privilege",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/borns-privilege",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/borns-privilege",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "Born's Command",
        };

        /// <summary>
        /// Demon's Aileron 
        /// </summary>
        public static Item DemonsAileron = new Item
        {
            Id = 224397,
            Name = "Demon's Aileron",
            Quality = ItemQuality.Legendary,
            Slug = "demons-aileron",
            ItemType = ItemType.Shoulder,
            BaseType = ItemBaseType.Armor,
            InternalName = "shoulderPads_norm_unique_06",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/demons-aileron",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/demons-aileron",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/demons-aileron",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "Demon's Hide",
        };

        /// <summary>
        /// Beckon Sail When receiving fatal damage, you instead automatically cast Smoke Screen and are healed to 25% Life. This effect may occur once every 120 seconds.
        /// </summary>
        public static Item BeckonSail = new Item
        {
            Id = 223150,
            Name = "Beckon Sail",
            Quality = ItemQuality.Legendary,
            Slug = "beckon-sail",
            ItemType = ItemType.Chest,
            BaseType = ItemBaseType.Armor,
            InternalName = "Cloak_norm_unique_02",
            DataUrl = "https://us.battle.net/api/d3/data/item/beckon-sail",
            Url = "https://us.battle.net/d3/en/item/beckon-sail",
            RelativeUrl = "/d3/en/item/beckon-sail",
            IsCrafted = false,
            LegendaryAffix = "When receiving fatal damage, you instead automatically cast Smoke Screen and are healed to 25% Life. This effect may occur once every 120 seconds.",
            SetName = "",
        };

        /// <summary>
        /// Blackfeather Dodging or getting hit by a ranged attack automatically shoots a homing rocket back at the attacker for 600–800% weapon damage as Physical.
        /// </summary>
        public static Item Blackfeather = new Item
        {
            Id = 332206,
            Name = "Blackfeather",
            Quality = ItemQuality.Legendary,
            Slug = "blackfeather",
            ItemType = ItemType.Chest,
            BaseType = ItemBaseType.Armor,
            InternalName = "cloak_norm_unique_01",
            DataUrl = "https://us.battle.net/api/d3/data/item/blackfeather",
            Url = "https://us.battle.net/d3/en/item/blackfeather",
            RelativeUrl = "/d3/en/item/blackfeather",
            IsCrafted = false,
            LegendaryAffix = "Dodging or getting hit by a ranged attack automatically shoots a homing rocket back at the attacker for 600–800% weapon damage as Physical.",
            SetName = "",
        };

        /// <summary>
        /// The Cloak of the Garwulf Companion - Wolf Companion now summons 3 wolves.
        /// </summary>
        public static Item TheCloakOfTheGarwulf = new Item
        {
            Id = 223151,
            Name = "The Cloak of the Garwulf",
            Quality = ItemQuality.Legendary,
            Slug = "the-cloak-of-the-garwulf",
            ItemType = ItemType.Chest,
            BaseType = ItemBaseType.Armor,
            InternalName = "Cloak_norm_unique_03",
            DataUrl = "https://us.battle.net/api/d3/data/item/the-cloak-of-the-garwulf",
            Url = "https://us.battle.net/d3/en/item/the-cloak-of-the-garwulf",
            RelativeUrl = "/d3/en/item/the-cloak-of-the-garwulf",
            IsCrafted = false,
            LegendaryAffix = "Companion - Wolf Companion now summons 3 wolves.",
            SetName = "",
        };

        /// <summary>
        /// Cape of the Dark Night Automatically drop Caltrops when you are hit. This effect may only occur once every 6 seconds.
        /// </summary>
        public static Item CapeOfTheDarkNight = new Item
        {
            Id = 223149,
            Name = "Cape of the Dark Night",
            Quality = ItemQuality.Legendary,
            Slug = "cape-of-the-dark-night",
            ItemType = ItemType.Chest,
            BaseType = ItemBaseType.Armor,
            InternalName = "Cloak_norm_unique_01",
            DataUrl = "https://us.battle.net/api/d3/data/item/cape-of-the-dark-night",
            Url = "https://us.battle.net/d3/en/item/cape-of-the-dark-night",
            RelativeUrl = "/d3/en/item/cape-of-the-dark-night",
            IsCrafted = false,
            LegendaryAffix = "Automatically drop Caltrops when you are hit. This effect may only occur once every 6 seconds.",
            SetName = "",
        };

        /// <summary>
        /// Natalya's Embrace 
        /// </summary>
        public static Item NatalyasEmbrace = new Item
        {
            Id = 208934,
            Name = "Natalya's Embrace",
            Quality = ItemQuality.Legendary,
            Slug = "natalyas-embrace",
            ItemType = ItemType.Chest,
            BaseType = ItemBaseType.Armor,
            InternalName = "Cloak_norm_set_03",
            DataUrl = "https://us.battle.net/api/d3/data/item/natalyas-embrace",
            Url = "https://us.battle.net/d3/en/item/natalyas-embrace",
            RelativeUrl = "/d3/en/item/natalyas-embrace",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Natalya's Vengeance",
        };

        /// <summary>
        /// Gungdo Gear 
        /// </summary>
        public static Item GungdoGear = new Item
        {
            Id = 193688,
            Name = "Gungdo Gear",
            Quality = ItemQuality.Legendary,
            Slug = "gungdo-gear",
            ItemType = ItemType.Bracer,
            BaseType = ItemBaseType.Armor,
            InternalName = "Bracers_norm_unique_06",
            DataUrl = "https://us.battle.net/api/d3/data/item/gungdo-gear",
            Url = "https://us.battle.net/d3/en/item/gungdo-gear",
            RelativeUrl = "/d3/en/item/gungdo-gear",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Steady Strikers 
        /// </summary>
        public static Item SteadyStrikers = new Item
        {
            Id = 193686,
            Name = "Steady Strikers",
            Quality = ItemQuality.Legendary,
            Slug = "steady-strikers",
            ItemType = ItemType.Bracer,
            BaseType = ItemBaseType.Armor,
            InternalName = "bracers_norm_unique_04",
            DataUrl = "https://us.battle.net/api/d3/data/item/steady-strikers",
            Url = "https://us.battle.net/d3/en/item/steady-strikers",
            RelativeUrl = "/d3/en/item/steady-strikers",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Slave Bonds 
        /// </summary>
        public static Item SlaveBonds = new Item
        {
            Id = 193685,
            Name = "Slave Bonds",
            Quality = ItemQuality.Legendary,
            Slug = "slave-bonds",
            ItemType = ItemType.Bracer,
            BaseType = ItemBaseType.Armor,
            InternalName = "Bracers_norm_unique_03",
            DataUrl = "https://us.battle.net/api/d3/data/item/slave-bonds",
            Url = "https://us.battle.net/d3/en/item/slave-bonds",
            RelativeUrl = "/d3/en/item/slave-bonds",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Ancient Parthan Defenders Each stunned enemy within 25 yards reduces your damage taken by 9–12%.
        /// </summary>
        public static Item AncientParthanDefenders = new Item
        {
            Id = 298116,
            Name = "Ancient Parthan Defenders",
            Quality = ItemQuality.Legendary,
            Slug = "ancient-parthan-defenders",
            ItemType = ItemType.Bracer,
            BaseType = ItemBaseType.Armor,
            InternalName = "bracers_norm_unique_12",
            DataUrl = "https://us.battle.net/api/d3/data/item/ancient-parthan-defenders",
            Url = "https://us.battle.net/d3/en/item/ancient-parthan-defenders",
            RelativeUrl = "/d3/en/item/ancient-parthan-defenders",
            IsCrafted = false,
            LegendaryAffix = "Each stunned enemy within 25 yards reduces your damage taken by 9–12%.",
            SetName = "",
        };

        /// <summary>
        /// Custerian Wristguards Picking up gold grants experience.
        /// </summary>
        public static Item CusterianWristguards = new Item
        {
            Id = 298122,
            Name = "Custerian Wristguards",
            Quality = ItemQuality.Legendary,
            Slug = "custerian-wristguards",
            ItemType = ItemType.Bracer,
            BaseType = ItemBaseType.Armor,
            InternalName = "bracers_norm_unique_17",
            DataUrl = "https://us.battle.net/api/d3/data/item/custerian-wristguards",
            Url = "https://us.battle.net/d3/en/item/custerian-wristguards",
            RelativeUrl = "/d3/en/item/custerian-wristguards",
            IsCrafted = false,
            LegendaryAffix = "Picking up gold grants experience.",
            SetName = "",
        };

        /// <summary>
        /// Nemesis Bracers Shrines will spawn an enemy champion.
        /// </summary>
        public static Item NemesisBracers = new Item
        {
            Id = 298121, //205615,
            Name = "Nemesis Bracers",
            Quality = ItemQuality.Legendary,
            Slug = "nemesis-bracers",
            ItemType = ItemType.Bracer,
            BaseType = ItemBaseType.Armor,
            InternalName = "chestArmor_norm_unique_088",
            DataUrl = "https://us.battle.net/api/d3/data/item/nemesis-bracers",
            Url = "https://us.battle.net/d3/en/item/nemesis-bracers",
            RelativeUrl = "/d3/en/item/nemesis-bracers",
            IsCrafted = false,
            LegendaryAffix = "Shrines will spawn an enemy champion.",
            SetName = "",
        };

        /// <summary>
        /// Warzechian Armguards Every time you destroy a wreckable object, you gain a short burst of speed.
        /// </summary>
        public static Item WarzechianArmguards = new Item
        {
            Id = 298115,
            Name = "Warzechian Armguards",
            Quality = ItemQuality.Legendary,
            Slug = "warzechian-armguards",
            ItemType = ItemType.Bracer,
            BaseType = ItemBaseType.Armor,
            InternalName = "bracers_norm_unique_11",
            DataUrl = "https://us.battle.net/api/d3/data/item/warzechian-armguards",
            Url = "https://us.battle.net/d3/en/item/warzechian-armguards",
            RelativeUrl = "/d3/en/item/warzechian-armguards",
            IsCrafted = false,
            LegendaryAffix = "Every time you destroy a wreckable object, you gain a short burst of speed.",
            SetName = "",
        };

        /// <summary>
        /// Aughild's Ultimatum 
        /// </summary>
        public static Item AughildsUltimatum = new Item
        {
            Id = 0,
            Name = "Aughild's Ultimatum",
            Quality = ItemQuality.Legendary,
            Slug = "aughilds-demands",
            ItemType = ItemType.Bracer,
            BaseType = ItemBaseType.Armor,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/aughilds-demands",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/aughilds-demands",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/aughilds-demands",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "Aughild's Victory",
        };

        /// <summary>
        /// Promise of Glory 4–6% chance to spawn a Nephalem Glory globe when you Blind an enemy.
        /// </summary>
        public static Item PromiseOfGlory = new Item
        {
            Id = 193684,
            Name = "Promise of Glory",
            Quality = ItemQuality.Legendary,
            Slug = "promise-of-glory",
            ItemType = ItemType.Bracer,
            BaseType = ItemBaseType.Armor,
            InternalName = "Bracers_norm_unique_02",
            DataUrl = "https://us.battle.net/api/d3/data/item/promise-of-glory",
            Url = "https://us.battle.net/d3/en/item/promise-of-glory",
            RelativeUrl = "/d3/en/item/promise-of-glory",
            IsCrafted = false,
            LegendaryAffix = "4–6% chance to spawn a Nephalem Glory globe when you Blind an enemy.",
            SetName = "",
        };

        /// <summary>
        /// Guardian's Deflector 
        /// </summary>
        public static Item GuardiansDeflector = new Item
        {
            Id = 0,
            Name = "Guardian's Deflector",
            Quality = ItemQuality.Legendary,
            Slug = "guardians-bands",
            ItemType = ItemType.Bracer,
            BaseType = ItemBaseType.Armor,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/guardians-bands",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/guardians-bands",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/guardians-bands",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "Guardian's Contingency",
        };

        /// <summary>
        /// Lacuni Prowlers 
        /// </summary>
        public static Item LacuniProwlers = new Item
        {
            Id = 193687,
            Name = "Lacuni Prowlers",
            Quality = ItemQuality.Legendary,
            Slug = "lacuni-prowlers",
            ItemType = ItemType.Bracer,
            BaseType = ItemBaseType.Armor,
            InternalName = "Bracers_norm_unique_05",
            DataUrl = "https://us.battle.net/api/d3/data/item/lacuni-prowlers",
            Url = "https://us.battle.net/d3/en/item/lacuni-prowlers",
            RelativeUrl = "/d3/en/item/lacuni-prowlers",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Strongarm Bracers Enemies hit by knockbacks suffer 20–30% more damage for 5 seconds when they land.
        /// </summary>
        public static Item StrongarmBracers = new Item
        {
            Id = 193692,
            Name = "Strongarm Bracers",
            Quality = ItemQuality.Legendary,
            Slug = "strongarm-bracers",
            ItemType = ItemType.Bracer,
            BaseType = ItemBaseType.Armor,
            InternalName = "Bracers_norm_unique_07",
            DataUrl = "https://us.battle.net/api/d3/data/item/strongarm-bracers",
            Url = "https://us.battle.net/d3/en/item/strongarm-bracers",
            RelativeUrl = "/d3/en/item/strongarm-bracers",
            IsCrafted = false,
            LegendaryAffix = "Enemies hit by knockbacks suffer 20–30% more damage for 5 seconds when they land.",
            SetName = "",
        };

        /// <summary>
        /// Wondrous Deflectors 
        /// </summary>
        public static Item WondrousDeflectors = new Item
        {
            Id = 0,
            Name = "Wondrous Deflectors",
            Quality = ItemQuality.Legendary,
            Slug = "wondrous-deflectors",
            ItemType = ItemType.Bracer,
            BaseType = ItemBaseType.Armor,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/wondrous-deflectors",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/wondrous-deflectors",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/wondrous-deflectors",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Demon's Revenge 
        /// </summary>
        public static Item DemonsRevenge = new Item
        {
            Id = 0,
            Name = "Demon's Revenge",
            Quality = ItemQuality.Legendary,
            Slug = "demons-manacles",
            ItemType = ItemType.Bracer,
            BaseType = ItemBaseType.Armor,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/demons-manacles",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/demons-manacles",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/demons-manacles",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "Demon's Skin",
        };

        /// <summary>
        /// Trag'Oul Coils Healing wells replenish all resources and reduce all cooldowns by 45–60 seconds.
        /// </summary>
        public static Item TragoulCoils = new Item
        {
            Id = 298119,
            Name = "Trag'Oul Coils",
            Quality = ItemQuality.Legendary,
            Slug = "tragoul-coils",
            ItemType = ItemType.Bracer,
            BaseType = ItemBaseType.Armor,
            InternalName = "bracers_norm_unique_14",
            DataUrl = "https://us.battle.net/api/d3/data/item/tragoul-coils",
            Url = "https://us.battle.net/d3/en/item/tragoul-coils",
            RelativeUrl = "/d3/en/item/tragoul-coils",
            IsCrafted = false,
            LegendaryAffix = "Healing wells replenish all resources and reduce all cooldowns by 45–60 seconds.",
            SetName = "",
        };

        /// <summary>
        /// Krelm's Buff Bracers You are immune to Knockback and Stun effects.
        /// </summary>
        public static Item KrelmsBuffBracers = new Item
        {
            Id = 336185,
            Name = "Krelm's Buff Bracers",
            Quality = ItemQuality.Legendary,
            Slug = "krelms-buff-bracers",
            ItemType = ItemType.Bracer,
            BaseType = ItemBaseType.Armor,
            InternalName = "bracers_norm_set_02",
            DataUrl = "https://us.battle.net/api/d3/data/item/krelms-buff-bracers",
            Url = "https://us.battle.net/d3/en/item/krelms-buff-bracers",
            RelativeUrl = "/d3/en/item/krelms-buff-bracers",
            IsCrafted = false,
            LegendaryAffix = "You are immune to Knockback and Stun effects.",
            SetName = "Krelm’s Buff Bulwark",
        };

        /// <summary>
        /// Shackles of the Invoker 
        /// </summary>
        public static Item ShacklesOfTheInvoker = new Item
        {
            Id = 335030,
            Name = "Shackles of the Invoker",
            Quality = ItemQuality.Legendary,
            Slug = "shackles-of-the-invoker",
            ItemType = ItemType.Bracer,
            BaseType = ItemBaseType.Armor,
            InternalName = "bracers_norm_set_12",
            DataUrl = "https://us.battle.net/api/d3/data/item/shackles-of-the-invoker",
            Url = "https://us.battle.net/d3/en/item/shackles-of-the-invoker",
            RelativeUrl = "/d3/en/item/shackles-of-the-invoker",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Thorns of the Invoker",
        };

        /// <summary>
        /// Kethryes' Splint 
        /// </summary>
        public static Item KethryesSplint = new Item
        {
            Id = 193683,
            Name = "Kethryes' Splint",
            Quality = ItemQuality.Legendary,
            Slug = "kethryes-splint",
            ItemType = ItemType.Bracer,
            BaseType = ItemBaseType.Armor,
            InternalName = "Bracers_norm_unique_01",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/kethryes-splint",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/kethryes-splint",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/kethryes-splint",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Reaper's Wraps Health globes restore 25–30% of your primary resource.
        /// </summary>
        public static Item ReapersWraps = new Item
        {
            Id = 0,
            Name = "Reaper's Wraps",
            Quality = ItemQuality.Legendary,
            Slug = "reapers-wraps",
            ItemType = ItemType.Bracer,
            BaseType = ItemBaseType.Armor,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/reapers-wraps",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/reapers-wraps",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/reapers-wraps",
            IsCrafted = true,
            LegendaryAffix = "Health globes restore 25–30% of your primary resource.",
            SetName = "",
        };

        /// <summary>
        /// Aughild's Search 
        /// </summary>
        public static Item AughildsSearch = new Item
        {
            Id = 212581,
            Name = "Aughild's Search",
            Quality = ItemQuality.Legendary,
            Slug = "aughilds-search",
            ItemType = ItemType.Bracer,
            BaseType = ItemBaseType.Armor,
            InternalName = "Ring_norm_unique_010",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/aughilds-search",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/aughilds-search",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/aughilds-search",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "Aughild's Authority",
        };

        /// <summary>
        /// Demon's Animus 
        /// </summary>
        public static Item DemonsAnimus = new Item
        {
            Id = 222741,
            Name = "Demon's Animus",
            Quality = ItemQuality.Legendary,
            Slug = "demons-animus",
            ItemType = ItemType.Bracer,
            BaseType = ItemBaseType.Armor,
            InternalName = "Bracers_norm_unique_08",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/demons-animus",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/demons-animus",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/demons-animus",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "Demon's Hide",
        };

        /// <summary>
        /// Guardian's Aversion 
        /// </summary>
        public static Item GuardiansAversion = new Item
        {
            Id = 0,
            Name = "Guardian's Aversion",
            Quality = ItemQuality.Legendary,
            Slug = "guardians-aversion",
            ItemType = ItemType.Bracer,
            BaseType = ItemBaseType.Armor,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/guardians-aversion",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/guardians-aversion",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/guardians-aversion",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "Guardian's Jeopardy",
        };

        /// <summary>
        /// Aquila Cuirass 
        /// </summary>
        public static Item AquilaCuirass = new Item
        {
            Id = 197203,
            Name = "Aquila Cuirass",
            Quality = ItemQuality.Legendary,
            Slug = "aquila-cuirass",
            ItemType = ItemType.Chest,
            BaseType = ItemBaseType.Armor,
            InternalName = "chestarmor_norm_unique_047",
            DataUrl = "https://us.battle.net/api/d3/data/item/aquila-cuirass",
            Url = "https://us.battle.net/d3/en/item/aquila-cuirass",
            RelativeUrl = "/d3/en/item/aquila-cuirass",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Heart of Iron 
        /// </summary>
        public static Item HeartOfIron = new Item
        {
            Id = 205607,
            Name = "Heart of Iron",
            Quality = ItemQuality.Legendary,
            Slug = "heart-of-iron",
            ItemType = ItemType.Chest,
            BaseType = ItemBaseType.Armor,
            InternalName = "chestArmor_norm_unique_074",
            DataUrl = "https://us.battle.net/api/d3/data/item/heart-of-iron",
            Url = "https://us.battle.net/d3/en/item/heart-of-iron",
            RelativeUrl = "/d3/en/item/heart-of-iron",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Born's Heart of Steel 
        /// </summary>
        public static Item BornsHeartOfSteel = new Item
        {
            Id = 0,
            Name = "Born's Heart of Steel",
            Quality = ItemQuality.Legendary,
            Slug = "borns-carapace",
            ItemType = ItemType.Chest,
            BaseType = ItemBaseType.Armor,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/borns-carapace",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/borns-carapace",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/borns-carapace",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "Born's Defiance",
        };

        /// <summary>
        /// Chaingmail After earning a survival bonus, quickly heal to full Life.
        /// </summary>
        public static Item Chaingmail = new Item
        {
            Id = 197204,
            Name = "Chaingmail",
            Quality = ItemQuality.Legendary,
            Slug = "chaingmail",
            ItemType = ItemType.Chest,
            BaseType = ItemBaseType.Armor,
            InternalName = "chestArmor_norm_unique_048",
            DataUrl = "https://us.battle.net/api/d3/data/item/chaingmail",
            Url = "https://us.battle.net/d3/en/item/chaingmail",
            RelativeUrl = "/d3/en/item/chaingmail",
            IsCrafted = false,
            LegendaryAffix = "After earning a survival bonus, quickly heal to full Life.",
            SetName = "",
        };

        /// <summary>
        /// Cindercoat Reduces the resource cost of Fire skills by 23–30%.
        /// </summary>
        public static Item Cindercoat = new Item
        {
            Id = 0,
            Name = "Cindercoat",
            Quality = ItemQuality.Legendary,
            Slug = "cindercoat",
            ItemType = ItemType.Chest,
            BaseType = ItemBaseType.Armor,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/item/cindercoat",
            Url = "https://us.battle.net/d3/en/item/cindercoat",
            RelativeUrl = "/d3/en/item/cindercoat",
            IsCrafted = false,
            LegendaryAffix = "Reduces the resource cost of Fire skills by 23–30%.",
            SetName = "",
        };

        /// <summary>
        /// Shi Mizu's Haori While below 20–25% Life, all attacks are guaranteed Critical Hits.
        /// </summary>
        public static Item ShiMizusHaori = new Item
        {
            Id = 332200,
            Name = "Shi Mizu's Haori",
            Quality = ItemQuality.Legendary,
            Slug = "shi-mizus-haori",
            ItemType = ItemType.Chest,
            BaseType = ItemBaseType.Armor,
            InternalName = "chestarmor_norm_unique_01",
            DataUrl = "https://us.battle.net/api/d3/data/item/shi-mizus-haori",
            Url = "https://us.battle.net/d3/en/item/shi-mizus-haori",
            RelativeUrl = "/d3/en/item/shi-mizus-haori",
            IsCrafted = false,
            LegendaryAffix = "While below 20–25% Life, all attacks are guaranteed Critical Hits.",
            SetName = "",
        };

        /// <summary>
        /// Aughild's Dominion 
        /// </summary>
        public static Item AughildsDominion = new Item
        {
            Id = 0,
            Name = "Aughild's Dominion",
            Quality = ItemQuality.Legendary,
            Slug = "aughilds-vestments",
            ItemType = ItemType.Chest,
            BaseType = ItemBaseType.Armor,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/aughilds-vestments",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/aughilds-vestments",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/aughilds-vestments",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "Aughild's Victory",
        };

        /// <summary>
        /// Goldskin Chance for enemies to drop gold when you hit them.
        /// </summary>
        public static Item Goldskin = new Item
        {
            Id = 205642,
            Name = "Goldskin",
            Quality = ItemQuality.Legendary,
            Slug = "goldskin",
            ItemType = ItemType.Chest,
            BaseType = ItemBaseType.Armor,
            InternalName = "Gloves_norm_unique_078",
            DataUrl = "https://us.battle.net/api/d3/data/item/goldskin",
            Url = "https://us.battle.net/d3/en/item/goldskin",
            RelativeUrl = "/d3/en/item/goldskin",
            IsCrafted = false,
            LegendaryAffix = "Chance for enemies to drop gold when you hit them.",
            SetName = "",
        };

        /// <summary>
        /// Tyrael's Might 
        /// </summary>
        public static Item TyraelsMight = new Item
        {
            Id = 205608,
            Name = "Tyrael's Might",
            Quality = ItemQuality.Legendary,
            Slug = "tyraels-might",
            ItemType = ItemType.Chest,
            BaseType = ItemBaseType.Armor,
            InternalName = "chestArmor_norm_unique_075",
            DataUrl = "https://us.battle.net/api/d3/data/item/tyraels-might",
            Url = "https://us.battle.net/d3/en/item/tyraels-might",
            RelativeUrl = "/d3/en/item/tyraels-might",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Blackthorne's Surcoat 
        /// </summary>
        public static Item BlackthornesSurcoat = new Item
        {
            Id = 222456,
            Name = "Blackthorne's Surcoat",
            Quality = ItemQuality.Legendary,
            Slug = "blackthornes-surcoat",
            ItemType = ItemType.Chest,
            BaseType = ItemBaseType.Armor,
            InternalName = "chestArmor_norm_unique_050",
            DataUrl = "https://us.battle.net/api/d3/data/item/blackthornes-surcoat",
            Url = "https://us.battle.net/d3/en/item/blackthornes-surcoat",
            RelativeUrl = "/d3/en/item/blackthornes-surcoat",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Blackthorne's Battlegear",
        };

        /// <summary>
        /// Immortal King's Eternal Reign 
        /// </summary>
        public static Item ImmortalKingsEternalReign = new Item
        {
            Id = 205613,
            Name = "Immortal King's Eternal Reign",
            Quality = ItemQuality.Legendary,
            Slug = "immortal-kings-eternal-reign",
            ItemType = ItemType.Chest,
            BaseType = ItemBaseType.Armor,
            InternalName = "chestArmor_norm_unique_086",
            DataUrl = "https://us.battle.net/api/d3/data/item/immortal-kings-eternal-reign",
            Url = "https://us.battle.net/d3/en/item/immortal-kings-eternal-reign",
            RelativeUrl = "/d3/en/item/immortal-kings-eternal-reign",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Immortal King's Call",
        };

        /// <summary>
        /// Inna's Vast Expanse 
        /// </summary>
        public static Item InnasVastExpanse = new Item
        {
            Id = 205614,
            Name = "Inna's Vast Expanse",
            Quality = ItemQuality.Legendary,
            Slug = "innas-vast-expanse",
            ItemType = ItemType.Chest,
            BaseType = ItemBaseType.Armor,
            InternalName = "chestArmor_norm_unique_087",
            DataUrl = "https://us.battle.net/api/d3/data/item/innas-vast-expanse",
            Url = "https://us.battle.net/d3/en/item/innas-vast-expanse",
            RelativeUrl = "/d3/en/item/innas-vast-expanse",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Inna's Mantra",
        };

        /// <summary>
        /// Tal Rasha's Relentless Pursuit 
        /// </summary>
        public static Item TalRashasRelentlessPursuit = new Item
        {
            Id = 211626,
            Name = "Tal Rasha's Relentless Pursuit",
            Quality = ItemQuality.Legendary,
            Slug = "tal-rashas-relentless-pursuit",
            ItemType = ItemType.Chest,
            BaseType = ItemBaseType.Armor,
            InternalName = "chestArmor_norm_set_01",
            DataUrl = "https://us.battle.net/api/d3/data/item/tal-rashas-relentless-pursuit",
            Url = "https://us.battle.net/d3/en/item/tal-rashas-relentless-pursuit",
            RelativeUrl = "/d3/en/item/tal-rashas-relentless-pursuit",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Tal Rasha's Elements",
        };

        /// <summary>
        /// Zunimassa's Marrow 
        /// </summary>
        public static Item ZunimassasMarrow = new Item
        {
            Id = 205615,
            Name = "Zunimassa's Marrow",
            Quality = ItemQuality.Legendary,
            Slug = "zunimassas-marrow",
            ItemType = ItemType.Chest,
            BaseType = ItemBaseType.Armor,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/item/zunimassas-marrow",
            Url = "https://us.battle.net/d3/en/item/zunimassas-marrow",
            RelativeUrl = "/d3/en/item/zunimassas-marrow",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Zunimassa's Haunt",
        };

        /// <summary>
        /// Robes of the Rydraelm 
        /// </summary>
        public static Item RobesOfTheRydraelm = new Item
        {
            Id = 0,
            Name = "Robes of the Rydraelm",
            Quality = ItemQuality.Legendary,
            Slug = "robes-of-the-rydraelm",
            ItemType = ItemType.Chest,
            BaseType = ItemBaseType.Armor,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/robes-of-the-rydraelm",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/robes-of-the-rydraelm",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/robes-of-the-rydraelm",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Demon's Heart 
        /// </summary>
        public static Item DemonsHeart = new Item
        {
            Id = 0,
            Name = "Demon's Heart",
            Quality = ItemQuality.Legendary,
            Slug = "demons-cage",
            ItemType = ItemType.Chest,
            BaseType = ItemBaseType.Armor,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/demons-cage",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/demons-cage",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/demons-cage",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "Demon's Skin",
        };

        /// <summary>
        /// Armor of the Kind Regent Smite will now also be cast at a second nearby enemy.
        /// </summary>
        public static Item ArmorOfTheKindRegent = new Item
        {
            Id = 332202,
            Name = "Armor of the Kind Regent",
            Quality = ItemQuality.Legendary,
            Slug = "armor-of-the-kind-regent",
            ItemType = ItemType.Chest,
            BaseType = ItemBaseType.Armor,
            InternalName = "chestarmor_norm_unique_02",
            DataUrl = "https://us.battle.net/api/d3/data/item/armor-of-the-kind-regent",
            Url = "https://us.battle.net/d3/en/item/armor-of-the-kind-regent",
            RelativeUrl = "/d3/en/item/armor-of-the-kind-regent",
            IsCrafted = false,
            LegendaryAffix = "Smite will now also be cast at a second nearby enemy.",
            SetName = "",
        };

        /// <summary>
        /// Breastplate of Akkhan 
        /// </summary>
        public static Item BreastplateOfAkkhan = new Item
        {
            Id = 358796,
            Name = "Breastplate of Akkhan",
            Quality = ItemQuality.Legendary,
            Slug = "breastplate-of-akkhan",
            ItemType = ItemType.Chest,
            BaseType = ItemBaseType.Armor,
            InternalName = "chestarmor_norm_set_10",
            DataUrl = "https://us.battle.net/api/d3/data/item/breastplate-of-akkhan",
            Url = "https://us.battle.net/d3/en/item/breastplate-of-akkhan",
            RelativeUrl = "/d3/en/item/breastplate-of-akkhan",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Armor of Akkhan",
        };

        /// <summary>
        /// Firebird's Breast 
        /// </summary>
        public static Item FirebirdsBreast = new Item
        {
            Id = 358788,
            Name = "Firebird's Breast",
            Quality = ItemQuality.Legendary,
            Slug = "firebirds-breast",
            ItemType = ItemType.Chest,
            BaseType = ItemBaseType.Armor,
            InternalName = "chestarmor_norm_set_06",
            DataUrl = "https://us.battle.net/api/d3/data/item/firebirds-breast",
            Url = "https://us.battle.net/d3/en/item/firebirds-breast",
            RelativeUrl = "/d3/en/item/firebirds-breast",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Firebird's Finery",
        };

        /// <summary>
        /// Heart of the Crashing Wave 
        /// </summary>
        public static Item HeartOfTheCrashingWave = new Item
        {
            Id = 338032,
            Name = "Heart of the Crashing Wave",
            Quality = ItemQuality.Legendary,
            Slug = "heart-of-the-crashing-wave",
            ItemType = ItemType.Chest,
            BaseType = ItemBaseType.Armor,
            InternalName = "chestarmor_norm_set_08",
            DataUrl = "https://us.battle.net/api/d3/data/item/heart-of-the-crashing-wave",
            Url = "https://us.battle.net/d3/en/item/heart-of-the-crashing-wave",
            RelativeUrl = "/d3/en/item/heart-of-the-crashing-wave",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Raiment of a Thousand Storms",
        };

        /// <summary>
        /// Helltooth Tunic 
        /// </summary>
        public static Item HelltoothTunic = new Item
        {
            Id = 363088,
            Name = "Helltooth Tunic",
            Quality = ItemQuality.Legendary,
            Slug = "helltooth-tunic",
            ItemType = ItemType.Chest,
            BaseType = ItemBaseType.Armor,
            InternalName = "chestarmor_norm_set_16",
            DataUrl = "https://us.battle.net/api/d3/data/item/helltooth-tunic",
            Url = "https://us.battle.net/d3/en/item/helltooth-tunic",
            RelativeUrl = "/d3/en/item/helltooth-tunic",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Helltooth Harness",
        };

        /// <summary>
        /// Jade Harvester's Peace 
        /// </summary>
        public static Item JadeHarvestersPeace = new Item
        {
            Id = 338038,
            Name = "Jade Harvester's Peace",
            Quality = ItemQuality.Legendary,
            Slug = "jade-harvesters-peace",
            ItemType = ItemType.Chest,
            BaseType = ItemBaseType.Armor,
            InternalName = "chestarmor_norm_set_09",
            DataUrl = "https://us.battle.net/api/d3/data/item/jade-harvesters-peace",
            Url = "https://us.battle.net/d3/en/item/jade-harvesters-peace",
            RelativeUrl = "/d3/en/item/jade-harvesters-peace",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Raiment of the Jade Harvester",
        };

        /// <summary>
        /// Marauder's Carapace 
        /// </summary>
        public static Item MaraudersCarapace = new Item
        {
            Id = 363803,
            Name = "Marauder's Carapace",
            Quality = ItemQuality.Legendary,
            Slug = "marauders-carapace",
            ItemType = ItemType.Chest,
            BaseType = ItemBaseType.Armor,
            InternalName = "chestarmor_norm_set_07",
            DataUrl = "https://us.battle.net/api/d3/data/item/marauders-carapace",
            Url = "https://us.battle.net/d3/en/item/marauders-carapace",
            RelativeUrl = "/d3/en/item/marauders-carapace",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Embodiment of the Marauder",
        };

        /// <summary>
        /// Raekor's Heart 
        /// </summary>
        public static Item RaekorsHeart = new Item
        {
            Id = 336984,
            Name = "Raekor's Heart",
            Quality = ItemQuality.Legendary,
            Slug = "raekors-heart",
            ItemType = ItemType.Chest,
            BaseType = ItemBaseType.Armor,
            InternalName = "chestarmor_norm_set_05",
            DataUrl = "https://us.battle.net/api/d3/data/item/raekors-heart",
            Url = "https://us.battle.net/d3/en/item/raekors-heart",
            RelativeUrl = "/d3/en/item/raekors-heart",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "The Legacy of Raekor",
        };

        /// <summary>
        /// Roland's Bearing 
        /// </summary>
        public static Item RolandsBearing = new Item
        {
            Id = 404095,
            Name = "Roland's Bearing",
            Quality = ItemQuality.Legendary,
            Slug = "rolands-bearing",
            ItemType = ItemType.Chest,
            BaseType = ItemBaseType.Armor,
            InternalName = "chestArmor_norm_base_flippy",
            DataUrl = "https://us.battle.net/api/d3/data/item/rolands-bearing",
            Url = "https://us.battle.net/d3/en/item/rolands-bearing",
            RelativeUrl = "/d3/en/item/rolands-bearing",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Roland's Legacy",
        };

        /// <summary>
        /// The Shadow's Bane 
        /// </summary>
        public static Item TheShadowsBane = new Item
        {
            Id = 332359,
            Name = "The Shadow's Bane",
            Quality = ItemQuality.Legendary,
            Slug = "the-shadows-bane",
            ItemType = ItemType.Chest,
            BaseType = ItemBaseType.Armor,
            InternalName = "chestarmor_norm_set_14",
            DataUrl = "https://us.battle.net/api/d3/data/item/the-shadows-bane",
            Url = "https://us.battle.net/d3/en/item/the-shadows-bane",
            RelativeUrl = "/d3/en/item/the-shadows-bane",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "The Shadow’s Mantle",
        };

        /// <summary>
        /// Vyr's Astonishing Aura 
        /// </summary>
        public static Item VyrsAstonishingAura = new Item
        {
            Id = 332357,
            Name = "Vyr's Astonishing Aura",
            Quality = ItemQuality.Legendary,
            Slug = "vyrs-astonishing-aura",
            ItemType = ItemType.Chest,
            BaseType = ItemBaseType.Armor,
            InternalName = "chestarmor_norm_set_13",
            DataUrl = "https://us.battle.net/api/d3/data/item/vyrs-astonishing-aura",
            Url = "https://us.battle.net/d3/en/item/vyrs-astonishing-aura",
            RelativeUrl = "/d3/en/item/vyrs-astonishing-aura",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Vyr's Amazing Arcana",
        };

        /// <summary>
        /// Mantle of the Rydraelm 
        /// </summary>
        public static Item MantleOfTheRydraelm = new Item
        {
            Id = 205609,
            Name = "Mantle of the Rydraelm",
            Quality = ItemQuality.Legendary,
            Slug = "mantle-of-the-rydraelm",
            ItemType = ItemType.Chest,
            BaseType = ItemBaseType.Armor,
            InternalName = "chestArmor_norm_unique_076",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/mantle-of-the-rydraelm",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/mantle-of-the-rydraelm",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/mantle-of-the-rydraelm",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Aughild's Rule 
        /// </summary>
        public static Item AughildsRule = new Item
        {
            Id = 364697,
            Name = "Aughild's Rule",
            Quality = ItemQuality.Legendary,
            Slug = "aughilds-rule",
            ItemType = ItemType.Chest,
            BaseType = ItemBaseType.Armor,
            InternalName = "CraftingReagent_Legendary_Unique_InfernalMachine_Diablo_x1",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/aughilds-rule",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/aughilds-rule",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/aughilds-rule",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "Aughild's Authority",
        };

        /// <summary>
        /// Born's Frozen Soul 
        /// </summary>
        public static Item BornsFrozenSoul = new Item
        {
            Id = 197199,
            Name = "Born's Frozen Soul",
            Quality = ItemQuality.Legendary,
            Slug = "borns-frozen-soul",
            ItemType = ItemType.Chest,
            BaseType = ItemBaseType.Armor,
            InternalName = "chestArmor_norm_unique_044",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/borns-frozen-soul",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/borns-frozen-soul",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/borns-frozen-soul",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "Born's Command",
        };

        /// <summary>
        /// Demon's Marrow 
        /// </summary>
        public static Item DemonsMarrow = new Item
        {
            Id = 205612,
            Name = "Demon's Marrow",
            Quality = ItemQuality.Legendary,
            Slug = "demons-marrow",
            ItemType = ItemType.Chest,
            BaseType = ItemBaseType.Armor,
            InternalName = "chestArmor_norm_unique_085",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/demons-marrow",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/demons-marrow",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/demons-marrow",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "Demon's Hide",
        };

        /// <summary>
        /// Broken Crown 
        /// </summary>
        public static Item BrokenCrown = new Item
        {
            Id = 220630,
            Name = "Broken Crown",
            Quality = ItemQuality.Legendary,
            Slug = "broken-crown",
            ItemType = ItemType.Helm,
            BaseType = ItemBaseType.Armor,
            InternalName = "Helm_norm_unique_02",
            DataUrl = "https://us.battle.net/api/d3/data/item/broken-crown",
            Url = "https://us.battle.net/d3/en/item/broken-crown",
            RelativeUrl = "/d3/en/item/broken-crown",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Leoric's Crown Increase the effect of any gem socketed into this item by 75–100%.
        /// </summary>
        public static Item LeoricsCrown = new Item
        {
            Id = 196024,
            Name = "Leoric's Crown",
            Quality = ItemQuality.Legendary,
            Slug = "leorics-crown",
            ItemType = ItemType.Helm,
            BaseType = ItemBaseType.Armor,
            InternalName = "Helm_norm_unique_01",
            DataUrl = "https://us.battle.net/api/d3/data/item/leorics-crown",
            Url = "https://us.battle.net/d3/en/item/leorics-crown",
            RelativeUrl = "/d3/en/item/leorics-crown",
            IsCrafted = false,
            LegendaryAffix = "Increase the effect of any gem socketed into this item by 75–100%.",
            SetName = "",
        };

        /// <summary>
        /// Cain's Memory 
        /// </summary>
        public static Item CainsMemory = new Item
        {
            Id = 0,
            Name = "Cain's Memory",
            Quality = ItemQuality.Legendary,
            Slug = "cains-laurel",
            ItemType = ItemType.Helm,
            BaseType = ItemBaseType.Armor,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/cains-laurel",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/cains-laurel",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/cains-laurel",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "Cain's Fate",
        };

        /// <summary>
        /// Blind Faith 
        /// </summary>
        public static Item BlindFaith = new Item
        {
            Id = 197037,
            Name = "Blind Faith",
            Quality = ItemQuality.Legendary,
            Slug = "blind-faith",
            ItemType = ItemType.Helm,
            BaseType = ItemBaseType.Armor,
            InternalName = "helm_norm_unique_07",
            DataUrl = "https://us.battle.net/api/d3/data/item/blind-faith",
            Url = "https://us.battle.net/d3/en/item/blind-faith",
            RelativeUrl = "/d3/en/item/blind-faith",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Deathseer's Cowl 15–20% chance on being hit by an Undead enemy to charm it for 2 seconds.
        /// </summary>
        public static Item DeathseersCowl = new Item
        {
            Id = 298146,
            Name = "Deathseer's Cowl",
            Quality = ItemQuality.Legendary,
            Slug = "deathseers-cowl",
            ItemType = ItemType.Helm,
            BaseType = ItemBaseType.Armor,
            InternalName = "helm_norm_unique_14",
            DataUrl = "https://us.battle.net/api/d3/data/item/deathseers-cowl",
            Url = "https://us.battle.net/d3/en/item/deathseers-cowl",
            RelativeUrl = "/d3/en/item/deathseers-cowl",
            IsCrafted = false,
            LegendaryAffix = "15–20% chance on being hit by an Undead enemy to charm it for 2 seconds.",
            SetName = "",
        };

        /// <summary>
        /// Aughild's Peak 
        /// </summary>
        public static Item AughildsPeak = new Item
        {
            Id = 0,
            Name = "Aughild's Peak",
            Quality = ItemQuality.Legendary,
            Slug = "aughilds-brow",
            ItemType = ItemType.Helm,
            BaseType = ItemBaseType.Armor,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/aughilds-brow",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/aughilds-brow",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/aughilds-brow",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "Aughild's Victory",
        };

        /// <summary>
        /// Skull of Resonance Threatening Shout has a chance to Charm enemies and cause them to join your side.
        /// </summary>
        public static Item SkullOfResonance = new Item
        {
            Id = 220549,
            Name = "Skull of Resonance",
            Quality = ItemQuality.Legendary,
            Slug = "skull-of-resonance",
            ItemType = ItemType.Helm,
            BaseType = ItemBaseType.Armor,
            InternalName = "Helm_norm_unique_04",
            DataUrl = "https://us.battle.net/api/d3/data/item/skull-of-resonance",
            Url = "https://us.battle.net/d3/en/item/skull-of-resonance",
            RelativeUrl = "/d3/en/item/skull-of-resonance",
            IsCrafted = false,
            LegendaryAffix = "Threatening Shout has a chance to Charm enemies and cause them to join your side.",
            SetName = "",
        };

        /// <summary>
        /// Guardian's Foresight 
        /// </summary>
        public static Item GuardiansForesight = new Item
        {
            Id = 0,
            Name = "Guardian's Foresight",
            Quality = ItemQuality.Legendary,
            Slug = "guardians-watch",
            ItemType = ItemType.Helm,
            BaseType = ItemBaseType.Armor,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/guardians-watch",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/guardians-watch",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/guardians-watch",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "Guardian's Contingency",
        };

        /// <summary>
        /// Andariel's Visage Chance on hit to release a Poison Nova that deals 100–130% weapon damage as Poison to enemies within 10 yards.
        /// </summary>
        public static Item AndarielsVisage = new Item
        {
            Id = 198014,
            Name = "Andariel's Visage",
            Quality = ItemQuality.Legendary,
            Slug = "andariels-visage",
            ItemType = ItemType.Helm,
            BaseType = ItemBaseType.Armor,
            InternalName = "Helm_norm_unique_03",
            DataUrl = "https://us.battle.net/api/d3/data/item/andariels-visage",
            Url = "https://us.battle.net/d3/en/item/andariels-visage",
            RelativeUrl = "/d3/en/item/andariels-visage",
            IsCrafted = false,
            LegendaryAffix = "Chance on hit to release a Poison Nova that deals 100–130% weapon damage as Poison to enemies within 10 yards.",
            SetName = "",
        };

        /// <summary>
        /// Mempo of Twilight 
        /// </summary>
        public static Item MempoOfTwilight = new Item
        {
            Id = 223577,
            Name = "Mempo of Twilight",
            Quality = ItemQuality.Legendary,
            Slug = "mempo-of-twilight",
            ItemType = ItemType.Helm,
            BaseType = ItemBaseType.Armor,
            InternalName = "Helm_norm_unique_12",
            DataUrl = "https://us.battle.net/api/d3/data/item/mempo-of-twilight",
            Url = "https://us.battle.net/d3/en/item/mempo-of-twilight",
            RelativeUrl = "/d3/en/item/mempo-of-twilight",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Immortal King's Triumph 
        /// </summary>
        public static Item ImmortalKingsTriumph = new Item
        {
            Id = 210265,
            Name = "Immortal King's Triumph",
            Quality = ItemQuality.Legendary,
            Slug = "immortal-kings-triumph",
            ItemType = ItemType.Helm,
            BaseType = ItemBaseType.Armor,
            InternalName = "Helm_norm_unique_08",
            DataUrl = "https://us.battle.net/api/d3/data/item/immortal-kings-triumph",
            Url = "https://us.battle.net/d3/en/item/immortal-kings-triumph",
            RelativeUrl = "/d3/en/item/immortal-kings-triumph",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Immortal King's Call",
        };

        /// <summary>
        /// Natalya's Sight 
        /// </summary>
        public static Item NatalyasSight = new Item
        {
            Id = 210851,
            Name = "Natalya's Sight",
            Quality = ItemQuality.Legendary,
            Slug = "natalyas-sight",
            ItemType = ItemType.Helm,
            BaseType = ItemBaseType.Armor,
            InternalName = "helm_norm_unique_09",
            DataUrl = "https://us.battle.net/api/d3/data/item/natalyas-sight",
            Url = "https://us.battle.net/d3/en/item/natalyas-sight",
            RelativeUrl = "/d3/en/item/natalyas-sight",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Natalya's Vengeance",
        };

        /// <summary>
        /// Tal Rasha's Guise of Wisdom 
        /// </summary>
        public static Item TalRashasGuiseOfWisdom = new Item
        {
            Id = 211531,
            Name = "Tal Rasha's Guise of Wisdom",
            Quality = ItemQuality.Legendary,
            Slug = "tal-rashas-guise-of-wisdom",
            ItemType = ItemType.Helm,
            BaseType = ItemBaseType.Armor,
            InternalName = "Helm_norm_unique_10",
            DataUrl = "https://us.battle.net/api/d3/data/item/tal-rashas-guise-of-wisdom",
            Url = "https://us.battle.net/d3/en/item/tal-rashas-guise-of-wisdom",
            RelativeUrl = "/d3/en/item/tal-rashas-guise-of-wisdom",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Tal Rasha's Elements",
        };

        /// <summary>
        /// The Helm of Command 
        /// </summary>
        public static Item TheHelmOfCommand = new Item
        {
            Id = 0,
            Name = "The Helm of Command",
            Quality = ItemQuality.Legendary,
            Slug = "the-helm-of-command",
            ItemType = ItemType.Helm,
            BaseType = ItemBaseType.Armor,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/the-helm-of-command",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/the-helm-of-command",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/the-helm-of-command",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Sage's Orbit 
        /// </summary>
        public static Item SagesOrbit = new Item
        {
            Id = 0,
            Name = "Sage's Orbit",
            Quality = ItemQuality.Legendary,
            Slug = "sages-stones",
            ItemType = ItemType.Helm,
            BaseType = ItemBaseType.Armor,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/sages-stones",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/sages-stones",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/sages-stones",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "Sage's Plight",
        };

        /// <summary>
        /// Crown of the Invoker 
        /// </summary>
        public static Item CrownOfTheInvoker = new Item
        {
            Id = 335028,
            Name = "Crown of the Invoker",
            Quality = ItemQuality.Legendary,
            Slug = "crown-of-the-invoker",
            ItemType = ItemType.Helm,
            BaseType = ItemBaseType.Armor,
            InternalName = "helm_norm_set_12",
            DataUrl = "https://us.battle.net/api/d3/data/item/crown-of-the-invoker",
            Url = "https://us.battle.net/d3/en/item/crown-of-the-invoker",
            RelativeUrl = "/d3/en/item/crown-of-the-invoker",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Thorns of the Invoker",
        };

        /// <summary>
        /// Eyes of the Earth 
        /// </summary>
        public static Item EyesOfTheEarth = new Item
        {
            Id = 340528,
            Name = "Eyes of the Earth",
            Quality = ItemQuality.Legendary,
            Slug = "eyes-of-the-earth",
            ItemType = ItemType.Helm,
            BaseType = ItemBaseType.Armor,
            InternalName = "helm_norm_set_15",
            DataUrl = "https://us.battle.net/api/d3/data/item/eyes-of-the-earth",
            Url = "https://us.battle.net/d3/en/item/eyes-of-the-earth",
            RelativeUrl = "/d3/en/item/eyes-of-the-earth",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Might of the Earth",
        };

        /// <summary>
        /// Firebird's Plume 
        /// </summary>
        public static Item FirebirdsPlume = new Item
        {
            Id = 358791,
            Name = "Firebird's Plume",
            Quality = ItemQuality.Legendary,
            Slug = "firebirds-plume",
            ItemType = ItemType.Helm,
            BaseType = ItemBaseType.Armor,
            InternalName = "helm_norm_set_06",
            DataUrl = "https://us.battle.net/api/d3/data/item/firebirds-plume",
            Url = "https://us.battle.net/d3/en/item/firebirds-plume",
            RelativeUrl = "/d3/en/item/firebirds-plume",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Firebird's Finery",
        };

        /// <summary>
        /// Helltooth Mask 
        /// </summary>
        public static Item HelltoothMask = new Item
        {
            Id = 369016,
            Name = "Helltooth Mask",
            Quality = ItemQuality.Legendary,
            Slug = "helltooth-mask",
            ItemType = ItemType.Helm,
            BaseType = ItemBaseType.Armor,
            InternalName = "helm_norm_set_16",
            DataUrl = "https://us.battle.net/api/d3/data/item/helltooth-mask",
            Url = "https://us.battle.net/d3/en/item/helltooth-mask",
            RelativeUrl = "/d3/en/item/helltooth-mask",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Helltooth Harness",
        };

        /// <summary>
        /// Helm of Akkhan 
        /// </summary>
        public static Item HelmOfAkkhan = new Item
        {
            Id = 358799,
            Name = "Helm of Akkhan",
            Quality = ItemQuality.Legendary,
            Slug = "helm-of-akkhan",
            ItemType = ItemType.Helm,
            BaseType = ItemBaseType.Armor,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/item/helm-of-akkhan",
            Url = "https://us.battle.net/d3/en/item/helm-of-akkhan",
            RelativeUrl = "/d3/en/item/helm-of-akkhan",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Armor of Akkhan",
        };

        /// <summary>
        /// Jade Harvester's Wisdom 
        /// </summary>
        public static Item JadeHarvestersWisdom = new Item
        {
            Id = 338040,
            Name = "Jade Harvester's Wisdom",
            Quality = ItemQuality.Legendary,
            Slug = "jade-harvesters-wisdom",
            ItemType = ItemType.Helm,
            BaseType = ItemBaseType.Armor,
            InternalName = "helm_norm_set_09",
            DataUrl = "https://us.battle.net/api/d3/data/item/jade-harvesters-wisdom",
            Url = "https://us.battle.net/d3/en/item/jade-harvesters-wisdom",
            RelativeUrl = "/d3/en/item/jade-harvesters-wisdom",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Raiment of the Jade Harvester",
        };

        /// <summary>
        /// Marauder's Visage 
        /// </summary>
        public static Item MaraudersVisage = new Item
        {
            Id = 336994,
            Name = "Marauder's Visage",
            Quality = ItemQuality.Legendary,
            Slug = "marauders-visage",
            ItemType = ItemType.Helm,
            BaseType = ItemBaseType.Armor,
            InternalName = "helm_norm_set_07",
            DataUrl = "https://us.battle.net/api/d3/data/item/marauders-visage",
            Url = "https://us.battle.net/d3/en/item/marauders-visage",
            RelativeUrl = "/d3/en/item/marauders-visage",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Embodiment of the Marauder",
        };

        /// <summary>
        /// Mask of the Searing Sky 
        /// </summary>
        public static Item MaskOfTheSearingSky = new Item
        {
            Id = 338034,
            Name = "Mask of the Searing Sky",
            Quality = ItemQuality.Legendary,
            Slug = "mask-of-the-searing-sky",
            ItemType = ItemType.Helm,
            BaseType = ItemBaseType.Armor,
            InternalName = "helm_norm_set_08",
            DataUrl = "https://us.battle.net/api/d3/data/item/mask-of-the-searing-sky",
            Url = "https://us.battle.net/d3/en/item/mask-of-the-searing-sky",
            RelativeUrl = "/d3/en/item/mask-of-the-searing-sky",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Raiment of a Thousand Storms",
        };

        /// <summary>
        /// Raekor's Will 
        /// </summary>
        public static Item RaekorsWill = new Item
        {
            Id = 336988,
            Name = "Raekor's Will",
            Quality = ItemQuality.Legendary,
            Slug = "raekors-will",
            ItemType = ItemType.Helm,
            BaseType = ItemBaseType.Armor,
            InternalName = "helm_norm_set_05",
            DataUrl = "https://us.battle.net/api/d3/data/item/raekors-will",
            Url = "https://us.battle.net/d3/en/item/raekors-will",
            RelativeUrl = "/d3/en/item/raekors-will",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "The Legacy of Raekor",
        };

        /// <summary>
        /// Roland's Visage 
        /// </summary>
        public static Item RolandsVisage = new Item
        {
            Id = 404700,
            Name = "Roland's Visage",
            Quality = ItemQuality.Legendary,
            Slug = "rolands-visage",
            ItemType = ItemType.Helm,
            BaseType = ItemBaseType.Armor,
            InternalName = "Helm_norm_base_flippy",
            DataUrl = "https://us.battle.net/api/d3/data/item/rolands-visage",
            Url = "https://us.battle.net/d3/en/item/rolands-visage",
            RelativeUrl = "/d3/en/item/rolands-visage",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Roland's Legacy",
        };

        /// <summary>
        /// Sunwuko's Crown 
        /// </summary>
        public static Item SunwukosCrown = new Item
        {
            Id = 336173,
            Name = "Sunwuko's Crown",
            Quality = ItemQuality.Legendary,
            Slug = "sunwukos-crown",
            ItemType = ItemType.Helm,
            BaseType = ItemBaseType.Armor,
            InternalName = "helm_norm_set_11",
            DataUrl = "https://us.battle.net/api/d3/data/item/sunwukos-crown",
            Url = "https://us.battle.net/d3/en/item/sunwukos-crown",
            RelativeUrl = "/d3/en/item/sunwukos-crown",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Monkey King's Garb",
        };

        /// <summary>
        /// The Helm of Rule 
        /// </summary>
        public static Item TheHelmOfRule = new Item
        {
            Id = 222889,
            Name = "The Helm of Rule",
            Quality = ItemQuality.Legendary,
            Slug = "the-helm-of-rule",
            ItemType = ItemType.Helm,
            BaseType = ItemBaseType.Armor,
            InternalName = "Helm_norm_unique_11",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/the-helm-of-rule",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/the-helm-of-rule",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/the-helm-of-rule",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Aughild's Spike 
        /// </summary>
        public static Item AughildsSpike = new Item
        {
            Id = 0,
            Name = "Aughild's Spike",
            Quality = ItemQuality.Legendary,
            Slug = "aughilds-spike",
            ItemType = ItemType.Helm,
            BaseType = ItemBaseType.Armor,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/aughilds-spike",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/aughilds-spike",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/aughilds-spike",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "Aughild's Authority",
        };

        /// <summary>
        /// Cain's Insight 
        /// </summary>
        public static Item CainsInsight = new Item
        {
            Id = 222559,
            Name = "Cain's Insight",
            Quality = ItemQuality.Legendary,
            Slug = "cains-insight",
            ItemType = ItemType.Helm,
            BaseType = ItemBaseType.Armor,
            InternalName = "Helm_norm_set_02",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/cains-insight",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/cains-insight",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/cains-insight",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "Cain's Destiny",
        };

        /// <summary>
        /// Guardian's Gaze 
        /// </summary>
        public static Item GuardiansGaze = new Item
        {
            Id = 0,
            Name = "Guardian's Gaze",
            Quality = ItemQuality.Legendary,
            Slug = "guardians-gaze",
            ItemType = ItemType.Helm,
            BaseType = ItemBaseType.Armor,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/guardians-gaze",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/guardians-gaze",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/guardians-gaze",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "Guardian's Jeopardy",
        };

        /// <summary>
        /// Sage's Apogee 
        /// </summary>
        public static Item SagesApogee = new Item
        {
            Id = 221624,
            Name = "Sage's Apogee",
            Quality = ItemQuality.Legendary,
            Slug = "sages-apogee",
            ItemType = ItemType.Helm,
            BaseType = ItemBaseType.Armor,
            InternalName = "Helm_inferno_set_01",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/sages-apogee",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/sages-apogee",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/sages-apogee",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "Sage's Journey",
        };

        /// <summary>
        /// The Swami The damage bonus from kills while in Archon form now lasts for 10–15 seconds after Archon expires.
        /// </summary>
        public static Item TheSwami = new Item
        {
            Id = 218681,
            Name = "The Swami",
            Quality = ItemQuality.Legendary,
            Slug = "the-swami",
            ItemType = ItemType.WizardHat,
            BaseType = ItemBaseType.Armor,
            InternalName = "wizardhat_norm_unique_03",
            DataUrl = "https://us.battle.net/api/d3/data/item/the-swami",
            Url = "https://us.battle.net/d3/en/item/the-swami",
            RelativeUrl = "/d3/en/item/the-swami",
            IsCrafted = false,
            LegendaryAffix = "The damage bonus from kills while in Archon form now lasts for 10–15 seconds after Archon expires.",
            SetName = "",
        };

        /// <summary>
        /// Dark Mage's Shade Automatically cast Diamond Skin when you fall below 35% Life. This effect may occur once every 15–20 seconds.
        /// </summary>
        public static Item DarkMagesShade = new Item
        {
            Id = 224908,
            Name = "Dark Mage's Shade",
            Quality = ItemQuality.Legendary,
            Slug = "dark-mages-shade",
            ItemType = ItemType.WizardHat,
            BaseType = ItemBaseType.Armor,
            InternalName = "wizardhat_norm_unique_05",
            DataUrl = "https://us.battle.net/api/d3/data/item/dark-mages-shade",
            Url = "https://us.battle.net/d3/en/item/dark-mages-shade",
            RelativeUrl = "/d3/en/item/dark-mages-shade",
            IsCrafted = false,
            LegendaryAffix = "Automatically cast Diamond Skin when you fall below 35% Life. This effect may occur once every 15–20 seconds.",
            SetName = "",
        };

        /// <summary>
        /// Archmage's Vicalyke Your Mirror Images have a chance to multiply when killed by enemies.
        /// </summary>
        public static Item ArchmagesVicalyke = new Item
        {
            Id = 299471,
            Name = "Archmage's Vicalyke",
            Quality = ItemQuality.Legendary,
            Slug = "archmages-vicalyke",
            ItemType = ItemType.WizardHat,
            BaseType = ItemBaseType.Armor,
            InternalName = "wizardhat_norm_unique_06",
            DataUrl = "https://us.battle.net/api/d3/data/item/archmages-vicalyke",
            Url = "https://us.battle.net/d3/en/item/archmages-vicalyke",
            RelativeUrl = "/d3/en/item/archmages-vicalyke",
            IsCrafted = false,
            LegendaryAffix = "Your Mirror Images have a chance to multiply when killed by enemies.",
            SetName = "",
        };

        /// <summary>
        /// The Magistrate Frost Hydra now periodically casts Frost Nova.
        /// </summary>
        public static Item TheMagistrate = new Item
        {
            Id = 325579,
            Name = "The Magistrate",
            Quality = ItemQuality.Legendary,
            Slug = "the-magistrate",
            ItemType = ItemType.WizardHat,
            BaseType = ItemBaseType.Armor,
            InternalName = "wizardhat_norm_unique_08",
            DataUrl = "https://us.battle.net/api/d3/data/item/the-magistrate",
            Url = "https://us.battle.net/d3/en/item/the-magistrate",
            RelativeUrl = "/d3/en/item/the-magistrate",
            IsCrafted = false,
            LegendaryAffix = "Frost Hydra now periodically casts Frost Nova.",
            SetName = "",
        };

        /// <summary>
        /// Velvet Camaral Double the number of enemies your Electrocute jumps to.
        /// </summary>
        public static Item VelvetCamaral = new Item
        {
            Id = 299472,
            Name = "Velvet Camaral",
            Quality = ItemQuality.Legendary,
            Slug = "velvet-camaral",
            ItemType = ItemType.WizardHat,
            BaseType = ItemBaseType.Armor,
            InternalName = "wizardhat_norm_unique_07",
            DataUrl = "https://us.battle.net/api/d3/data/item/velvet-camaral",
            Url = "https://us.battle.net/d3/en/item/velvet-camaral",
            RelativeUrl = "/d3/en/item/velvet-camaral",
            IsCrafted = false,
            LegendaryAffix = "Double the number of enemies your Electrocute jumps to.",
            SetName = "",
        };

        /// <summary>
        /// Storm Crow 20–40% chance to cast a fiery ball when attacking.
        /// </summary>
        public static Item StormCrow = new Item
        {
            Id = 220694,
            Name = "Storm Crow",
            Quality = ItemQuality.Legendary,
            Slug = "storm-crow",
            ItemType = ItemType.WizardHat,
            BaseType = ItemBaseType.Armor,
            InternalName = "wizardHat_norm_unique_04",
            DataUrl = "https://us.battle.net/api/d3/data/item/storm-crow",
            Url = "https://us.battle.net/d3/en/item/storm-crow",
            RelativeUrl = "/d3/en/item/storm-crow",
            IsCrafted = false,
            LegendaryAffix = "20–40% chance to cast a fiery ball when attacking.",
            SetName = "",
        };

        /// <summary>
        /// Cain's Scribe 
        /// </summary>
        public static Item CainsScribe = new Item
        {
            Id = 0,
            Name = "Cain's Scribe",
            Quality = ItemQuality.Legendary,
            Slug = "cains-warmers",
            ItemType = ItemType.Gloves,
            BaseType = ItemBaseType.Armor,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/cains-warmers",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/cains-warmers",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/cains-warmers",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "Cain's Fate",
        };

        /// <summary>
        /// Stone Gauntlets 
        /// </summary>
        public static Item StoneGauntlets = new Item
        {
            Id = 205640,
            Name = "Stone Gauntlets",
            Quality = ItemQuality.Legendary,
            Slug = "stone-gauntlets",
            ItemType = ItemType.Gloves,
            BaseType = ItemBaseType.Armor,
            InternalName = "Gloves_norm_unique_076",
            DataUrl = "https://us.battle.net/api/d3/data/item/stone-gauntlets",
            Url = "https://us.battle.net/d3/en/item/stone-gauntlets",
            RelativeUrl = "/d3/en/item/stone-gauntlets",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Magefist 
        /// </summary>
        public static Item Magefist = new Item
        {
            Id = 197206,
            Name = "Magefist",
            Quality = ItemQuality.Legendary,
            Slug = "magefist",
            ItemType = ItemType.Gloves,
            BaseType = ItemBaseType.Armor,
            InternalName = "Gloves_norm_unique_044",
            DataUrl = "https://us.battle.net/api/d3/data/item/magefist",
            Url = "https://us.battle.net/d3/en/item/magefist",
            RelativeUrl = "/d3/en/item/magefist",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// St. Archew's Gage When there are 5 or more enemies within 12 yards, you gain an absorb shield equal to 20–25% of your maximum Life for 6 seconds. This effect may occur once every 30 seconds.
        /// </summary>
        public static Item StArchewsGage = new Item
        {
            Id = 332172,
            Name = "St. Archew's Gage",
            Quality = ItemQuality.Legendary,
            Slug = "st-archews-gage",
            ItemType = ItemType.Gloves,
            BaseType = ItemBaseType.Armor,
            InternalName = "gloves_norm_unique_01",
            DataUrl = "https://us.battle.net/api/d3/data/item/st-archews-gage",
            Url = "https://us.battle.net/d3/en/item/st-archews-gage",
            RelativeUrl = "/d3/en/item/st-archews-gage",
            IsCrafted = false,
            LegendaryAffix = "When there are 5 or more enemies within 12 yards, you gain an absorb shield equal to 20–25% of your maximum Life for 6 seconds. This effect may occur once every 30 seconds.",
            SetName = "",
        };

        /// <summary>
        /// Pendergrasps 
        /// </summary>
        public static Item Pendergrasps = new Item
        {
            Id = 0,
            Name = "Pendergrasps",
            Quality = ItemQuality.Legendary,
            Slug = "pendergrasps",
            ItemType = ItemType.Gloves,
            BaseType = ItemBaseType.Armor,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/pendergrasps",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/pendergrasps",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/pendergrasps",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Gladiator Gauntlets After earning a massacre bonus, gold rains from sky.
        /// </summary>
        public static Item GladiatorGauntlets = new Item
        {
            Id = 205635,
            Name = "Gladiator Gauntlets",
            Quality = ItemQuality.Legendary,
            Slug = "gladiator-gauntlets",
            ItemType = ItemType.Gloves,
            BaseType = ItemBaseType.Armor,
            InternalName = "Gloves_norm_unique_090",
            DataUrl = "https://us.battle.net/api/d3/data/item/gladiator-gauntlets",
            Url = "https://us.battle.net/d3/en/item/gladiator-gauntlets",
            RelativeUrl = "/d3/en/item/gladiator-gauntlets",
            IsCrafted = false,
            LegendaryAffix = "After earning a massacre bonus, gold rains from sky.",
            SetName = "",
        };

        /// <summary>
        /// Asheara's Iron Fist 
        /// </summary>
        public static Item AshearasIronFist = new Item
        {
            Id = 0,
            Name = "Asheara's Iron Fist",
            Quality = ItemQuality.Legendary,
            Slug = "ashearas-clasp",
            ItemType = ItemType.Gloves,
            BaseType = ItemBaseType.Armor,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/ashearas-clasp",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/ashearas-clasp",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/ashearas-clasp",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "Asheara's Uniform",
        };

        /// <summary>
        /// Frostburn Your Chill effects have up to a 34–45% chance to Freeze instead of Slow.
        /// </summary>
        public static Item Frostburn = new Item
        {
            Id = 197205,
            Name = "Frostburn",
            Quality = ItemQuality.Legendary,
            Slug = "frostburn",
            ItemType = ItemType.Gloves,
            BaseType = ItemBaseType.Armor,
            InternalName = "Gloves_norm_unique_043",
            DataUrl = "https://us.battle.net/api/d3/data/item/frostburn",
            Url = "https://us.battle.net/d3/en/item/frostburn",
            RelativeUrl = "/d3/en/item/frostburn",
            IsCrafted = false,
            LegendaryAffix = "Your Chill effects have up to a 34–45% chance to Freeze instead of Slow.",
            SetName = "",
        };

        /// <summary>
        /// Tasker and Theo Increase attack speed of your pets by 40–50%.
        /// </summary>
        public static Item TaskerAndTheo = new Item
        {
            Id = 0,
            Name = "Tasker and Theo",
            Quality = ItemQuality.Legendary,
            Slug = "tasker-and-theo",
            ItemType = ItemType.Gloves,
            BaseType = ItemBaseType.Armor,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/item/tasker-and-theo",
            Url = "https://us.battle.net/d3/en/item/tasker-and-theo",
            RelativeUrl = "/d3/en/item/tasker-and-theo",
            IsCrafted = false,
            LegendaryAffix = "Increase attack speed of your pets by 40–50%.",
            SetName = "",
        };

        /// <summary>
        /// Immortal King's Irons 
        /// </summary>
        public static Item ImmortalKingsIrons = new Item
        {
            Id = 205631,
            Name = "Immortal King's Irons",
            Quality = ItemQuality.Legendary,
            Slug = "immortal-kings-irons",
            ItemType = ItemType.Gloves,
            BaseType = ItemBaseType.Armor,
            InternalName = "Gloves_norm_unique_086",
            DataUrl = "https://us.battle.net/api/d3/data/item/immortal-kings-irons",
            Url = "https://us.battle.net/d3/en/item/immortal-kings-irons",
            RelativeUrl = "/d3/en/item/immortal-kings-irons",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Immortal King's Call",
        };

        /// <summary>
        /// Sage's Gesture 
        /// </summary>
        public static Item SagesGesture = new Item
        {
            Id = 0,
            Name = "Sage's Gesture",
            Quality = ItemQuality.Legendary,
            Slug = "sages-grasp",
            ItemType = ItemType.Gloves,
            BaseType = ItemBaseType.Armor,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/sages-grasp",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/sages-grasp",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/sages-grasp",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "Sage's Plight",
        };

        /// <summary>
        /// Firebird's Talons 
        /// </summary>
        public static Item FirebirdsTalons = new Item
        {
            Id = 358789,
            Name = "Firebird's Talons",
            Quality = ItemQuality.Legendary,
            Slug = "firebirds-talons",
            ItemType = ItemType.Gloves,
            BaseType = ItemBaseType.Armor,
            InternalName = "gloves_norm_set_06",
            DataUrl = "https://us.battle.net/api/d3/data/item/firebirds-talons",
            Url = "https://us.battle.net/d3/en/item/firebirds-talons",
            RelativeUrl = "/d3/en/item/firebirds-talons",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Firebird's Finery",
        };

        /// <summary>
        /// Fists of Thunder 
        /// </summary>
        public static Item FistsOfThunder = new Item
        {
            Id = 338033,
            Name = "Fists of Thunder",
            Quality = ItemQuality.Legendary,
            Slug = "fists-of-thunder",
            ItemType = ItemType.Gloves,
            BaseType = ItemBaseType.Armor,
            InternalName = "gloves_norm_set_08",
            DataUrl = "https://us.battle.net/api/d3/data/item/fists-of-thunder",
            Url = "https://us.battle.net/d3/en/item/fists-of-thunder",
            RelativeUrl = "/d3/en/item/fists-of-thunder",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Raiment of a Thousand Storms",
        };

        /// <summary>
        /// Gauntlets of Akkhan 
        /// </summary>
        public static Item GauntletsOfAkkhan = new Item
        {
            Id = 358798,
            Name = "Gauntlets of Akkhan",
            Quality = ItemQuality.Legendary,
            Slug = "gauntlets-of-akkhan",
            ItemType = ItemType.Gloves,
            BaseType = ItemBaseType.Armor,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/item/gauntlets-of-akkhan",
            Url = "https://us.battle.net/d3/en/item/gauntlets-of-akkhan",
            RelativeUrl = "/d3/en/item/gauntlets-of-akkhan",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Armor of Akkhan",
        };

        /// <summary>
        /// Helltooth Gauntlets 
        /// </summary>
        public static Item HelltoothGauntlets = new Item
        {
            Id = 363094,
            Name = "Helltooth Gauntlets",
            Quality = ItemQuality.Legendary,
            Slug = "helltooth-gauntlets",
            ItemType = ItemType.Gloves,
            BaseType = ItemBaseType.Armor,
            InternalName = "gloves_norm_set_16",
            DataUrl = "https://us.battle.net/api/d3/data/item/helltooth-gauntlets",
            Url = "https://us.battle.net/d3/en/item/helltooth-gauntlets",
            RelativeUrl = "/d3/en/item/helltooth-gauntlets",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Helltooth Harness",
        };

        /// <summary>
        /// Jade Harvester's Mercy 
        /// </summary>
        public static Item JadeHarvestersMercy = new Item
        {
            Id = 338039,
            Name = "Jade Harvester's Mercy",
            Quality = ItemQuality.Legendary,
            Slug = "jade-harvesters-mercy",
            ItemType = ItemType.Gloves,
            BaseType = ItemBaseType.Armor,
            InternalName = "gloves_norm_set_09",
            DataUrl = "https://us.battle.net/api/d3/data/item/jade-harvesters-mercy",
            Url = "https://us.battle.net/d3/en/item/jade-harvesters-mercy",
            RelativeUrl = "/d3/en/item/jade-harvesters-mercy",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Raiment of the Jade Harvester",
        };

        /// <summary>
        /// Marauder's Gloves 
        /// </summary>
        public static Item MaraudersGloves = new Item
        {
            Id = 336992,
            Name = "Marauder's Gloves",
            Quality = ItemQuality.Legendary,
            Slug = "marauders-gloves",
            ItemType = ItemType.Gloves,
            BaseType = ItemBaseType.Armor,
            InternalName = "gloves_norm_set_07",
            DataUrl = "https://us.battle.net/api/d3/data/item/marauders-gloves",
            Url = "https://us.battle.net/d3/en/item/marauders-gloves",
            RelativeUrl = "/d3/en/item/marauders-gloves",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Embodiment of the Marauder",
        };

        /// <summary>
        /// Pride of the Invoker 
        /// </summary>
        public static Item PrideOfTheInvoker = new Item
        {
            Id = 335027,
            Name = "Pride of the Invoker",
            Quality = ItemQuality.Legendary,
            Slug = "pride-of-the-invoker",
            ItemType = ItemType.Gloves,
            BaseType = ItemBaseType.Armor,
            InternalName = "gloves_norm_set_12",
            DataUrl = "https://us.battle.net/api/d3/data/item/pride-of-the-invoker",
            Url = "https://us.battle.net/d3/en/item/pride-of-the-invoker",
            RelativeUrl = "/d3/en/item/pride-of-the-invoker",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Thorns of the Invoker",
        };

        /// <summary>
        /// Pull of the Earth 
        /// </summary>
        public static Item PullOfTheEarth = new Item
        {
            Id = 340523,
            Name = "Pull of the Earth",
            Quality = ItemQuality.Legendary,
            Slug = "pull-of-the-earth",
            ItemType = ItemType.Gloves,
            BaseType = ItemBaseType.Armor,
            InternalName = "gloves_norm_set_15",
            DataUrl = "https://us.battle.net/api/d3/data/item/pull-of-the-earth",
            Url = "https://us.battle.net/d3/en/item/pull-of-the-earth",
            RelativeUrl = "/d3/en/item/pull-of-the-earth",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Might of the Earth",
        };

        /// <summary>
        /// Raekor's Wraps 
        /// </summary>
        public static Item RaekorsWraps = new Item
        {
            Id = 336985,
            Name = "Raekor's Wraps",
            Quality = ItemQuality.Legendary,
            Slug = "raekors-wraps",
            ItemType = ItemType.Gloves,
            BaseType = ItemBaseType.Armor,
            InternalName = "gloves_norm_set_05",
            DataUrl = "https://us.battle.net/api/d3/data/item/raekors-wraps",
            Url = "https://us.battle.net/d3/en/item/raekors-wraps",
            RelativeUrl = "/d3/en/item/raekors-wraps",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "The Legacy of Raekor",
        };

        /// <summary>
        /// Roland's Grasp 
        /// </summary>
        public static Item RolandsGrasp = new Item
        {
            Id = 404096,
            Name = "Roland's Grasp",
            Quality = ItemQuality.Legendary,
            Slug = "rolands-grasp",
            ItemType = ItemType.Gloves,
            BaseType = ItemBaseType.Armor,
            InternalName = "Gloves_norm_base_flippy",
            DataUrl = "https://us.battle.net/api/d3/data/item/rolands-grasp",
            Url = "https://us.battle.net/d3/en/item/rolands-grasp",
            RelativeUrl = "/d3/en/item/rolands-grasp",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Roland's Legacy",
        };

        /// <summary>
        /// Sunwuko's Paws 
        /// </summary>
        public static Item SunwukosPaws = new Item
        {
            Id = 336172,
            Name = "Sunwuko's Paws",
            Quality = ItemQuality.Legendary,
            Slug = "sunwukos-paws",
            ItemType = ItemType.Gloves,
            BaseType = ItemBaseType.Armor,
            InternalName = "gloves_norm_set_11",
            DataUrl = "https://us.battle.net/api/d3/data/item/sunwukos-paws",
            Url = "https://us.battle.net/d3/en/item/sunwukos-paws",
            RelativeUrl = "/d3/en/item/sunwukos-paws",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Monkey King's Garb",
        };

        /// <summary>
        /// The Shadow's Grasp 
        /// </summary>
        public static Item TheShadowsGrasp = new Item
        {
            Id = 332362,
            Name = "The Shadow's Grasp",
            Quality = ItemQuality.Legendary,
            Slug = "the-shadows-grasp",
            ItemType = ItemType.Gloves,
            BaseType = ItemBaseType.Armor,
            InternalName = "gloves_norm_set_14",
            DataUrl = "https://us.battle.net/api/d3/data/item/the-shadows-grasp",
            Url = "https://us.battle.net/d3/en/item/the-shadows-grasp",
            RelativeUrl = "/d3/en/item/the-shadows-grasp",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "The Shadow’s Mantle",
        };

        /// <summary>
        /// Vyr's Grasping Gauntlets 
        /// </summary>
        public static Item VyrsGraspingGauntlets = new Item
        {
            Id = 346210,
            Name = "Vyr's Grasping Gauntlets",
            Quality = ItemQuality.Legendary,
            Slug = "vyrs-grasping-gauntlets",
            ItemType = ItemType.Gloves,
            BaseType = ItemBaseType.Armor,
            InternalName = "gloves_norm_set_13",
            DataUrl = "https://us.battle.net/api/d3/data/item/vyrs-grasping-gauntlets",
            Url = "https://us.battle.net/d3/en/item/vyrs-grasping-gauntlets",
            RelativeUrl = "/d3/en/item/vyrs-grasping-gauntlets",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Vyr's Amazing Arcana",
        };

        /// <summary>
        /// Penders Purchase 
        /// </summary>
        public static Item PendersPurchase = new Item
        {
            Id = 197207,
            Name = "Penders Purchase",
            Quality = ItemQuality.Legendary,
            Slug = "penders-purchase",
            ItemType = ItemType.Gloves,
            BaseType = ItemBaseType.Armor,
            InternalName = "Gloves_norm_unique_045",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/penders-purchase",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/penders-purchase",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/penders-purchase",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Asheara's Ward 
        /// </summary>
        public static Item AshearasWard = new Item
        {
            Id = 205636,
            Name = "Asheara's Ward",
            Quality = ItemQuality.Legendary,
            Slug = "ashearas-ward",
            ItemType = ItemType.Gloves,
            BaseType = ItemBaseType.Armor,
            InternalName = "Gloves_norm_unique_073",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/ashearas-ward",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/ashearas-ward",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/ashearas-ward",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "Asheara's Vestments",
        };

        /// <summary>
        /// Cain's Scrivener 
        /// </summary>
        public static Item CainsScrivener = new Item
        {
            Id = 0,
            Name = "Cain's Scrivener",
            Quality = ItemQuality.Legendary,
            Slug = "cains-scriviner",
            ItemType = ItemType.Gloves,
            BaseType = ItemBaseType.Armor,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/cains-scriviner",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/cains-scriviner",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/cains-scriviner",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "Cain's Destiny",
        };

        /// <summary>
        /// Sage's Purchase 
        /// </summary>
        public static Item SagesPurchase = new Item
        {
            Id = 205632,
            Name = "Sage's Purchase",
            Quality = ItemQuality.Legendary,
            Slug = "sages-purchase",
            ItemType = ItemType.Gloves,
            BaseType = ItemBaseType.Armor,
            InternalName = "Gloves_norm_unique_087",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/sages-purchase",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/sages-purchase",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/sages-purchase",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "Sage's Journey",
        };

        /// <summary>
        /// Girdle of Giants 
        /// </summary>
        public static Item GirdleOfGiants = new Item
        {
            Id = 212232,
            Name = "Girdle of Giants",
            Quality = ItemQuality.Legendary,
            Slug = "girdle-of-giants",
            ItemType = ItemType.Belt,
            BaseType = ItemBaseType.Armor,
            InternalName = "BarbBelt_norm_unique_04",
            DataUrl = "https://us.battle.net/api/d3/data/item/girdle-of-giants",
            Url = "https://us.battle.net/d3/en/item/girdle-of-giants",
            RelativeUrl = "/d3/en/item/girdle-of-giants",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// The Undisputed Champion 
        /// </summary>
        public static Item TheUndisputedChampion = new Item
        {
            Id = 193676,
            Name = "The Undisputed Champion",
            Quality = ItemQuality.Legendary,
            Slug = "the-undisputed-champion",
            ItemType = ItemType.Belt,
            BaseType = ItemBaseType.Armor,
            InternalName = "BarbBelt_norm_unique_06",
            DataUrl = "https://us.battle.net/api/d3/data/item/the-undisputed-champion",
            Url = "https://us.battle.net/d3/en/item/the-undisputed-champion",
            RelativeUrl = "/d3/en/item/the-undisputed-champion",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Kotuur's Brace 
        /// </summary>
        public static Item KotuursBrace = new Item
        {
            Id = 193674,
            Name = "Kotuur's Brace",
            Quality = ItemQuality.Legendary,
            Slug = "kotuurs-brace",
            ItemType = ItemType.Belt,
            BaseType = ItemBaseType.Armor,
            InternalName = "BarbBelt_norm_unique_07",
            DataUrl = "https://us.battle.net/api/d3/data/item/kotuurs-brace",
            Url = "https://us.battle.net/d3/en/item/kotuurs-brace",
            RelativeUrl = "/d3/en/item/kotuurs-brace",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Pride of Cassius Increases the duration of Ignore Pain by 4–6 seconds.
        /// </summary>
        public static Item PrideOfCassius = new Item
        {
            Id = 193673,
            Name = "Pride of Cassius",
            Quality = ItemQuality.Legendary,
            Slug = "pride-of-cassius",
            ItemType = ItemType.Belt,
            BaseType = ItemBaseType.Armor,
            InternalName = "BarbBelt_norm_unique_02",
            DataUrl = "https://us.battle.net/api/d3/data/item/pride-of-cassius",
            Url = "https://us.battle.net/d3/en/item/pride-of-cassius",
            RelativeUrl = "/d3/en/item/pride-of-cassius",
            IsCrafted = false,
            LegendaryAffix = "Increases the duration of Ignore Pain by 4–6 seconds.",
            SetName = "",
        };

        /// <summary>
        /// Chilanik's Chain Using War Cry increases the movement speed for you and all allies affected by 30–40% for 10 seconds.
        /// </summary>
        public static Item ChilaniksChain = new Item
        {
            Id = 298133,
            Name = "Chilanik's Chain",
            Quality = ItemQuality.Legendary,
            Slug = "chilaniks-chain",
            ItemType = ItemType.Belt,
            BaseType = ItemBaseType.Armor,
            InternalName = "barbbelt_norm_unique_10",
            DataUrl = "https://us.battle.net/api/d3/data/item/chilaniks-chain",
            Url = "https://us.battle.net/d3/en/item/chilaniks-chain",
            RelativeUrl = "/d3/en/item/chilaniks-chain",
            IsCrafted = false,
            LegendaryAffix = "Using War Cry increases the movement speed for you and all allies affected by 30–40% for 10 seconds.",
            SetName = "",
        };

        /// <summary>
        /// Dread Iron 
        /// </summary>
        public static Item DreadIron = new Item
        {
            Id = 193672,
            Name = "Dread Iron",
            Quality = ItemQuality.Legendary,
            Slug = "dread-iron",
            ItemType = ItemType.Belt,
            BaseType = ItemBaseType.Armor,
            InternalName = "BarbBelt_norm_unique_01",
            DataUrl = "https://us.battle.net/api/d3/data/item/dread-iron",
            Url = "https://us.battle.net/d3/en/item/dread-iron",
            RelativeUrl = "/d3/en/item/dread-iron",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Ageless Might 
        /// </summary>
        public static Item AgelessMight = new Item
        {
            Id = 193675,
            Name = "Ageless Might",
            Quality = ItemQuality.Legendary,
            Slug = "ageless-might",
            ItemType = ItemType.Belt,
            BaseType = ItemBaseType.Armor,
            InternalName = "BarbBelt_norm_unique_08",
            DataUrl = "https://us.battle.net/api/d3/data/item/ageless-might",
            Url = "https://us.battle.net/d3/en/item/ageless-might",
            RelativeUrl = "/d3/en/item/ageless-might",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Lamentation Rend can now stack up to 2 times on an enemy.
        /// </summary>
        public static Item Lamentation = new Item
        {
            Id = 212234,
            Name = "Lamentation",
            Quality = ItemQuality.Legendary,
            Slug = "lamentation",
            ItemType = ItemType.Belt,
            BaseType = ItemBaseType.Armor,
            InternalName = "barbbelt_norm_unique_05",
            DataUrl = "https://us.battle.net/api/d3/data/item/lamentation",
            Url = "https://us.battle.net/d3/en/item/lamentation",
            RelativeUrl = "/d3/en/item/lamentation",
            IsCrafted = false,
            LegendaryAffix = "Rend can now stack up to 2 times on an enemy.",
            SetName = "",
        };

        /// <summary>
        /// Immortal King's Tribal Binding 
        /// </summary>
        public static Item ImmortalKingsTribalBinding = new Item
        {
            Id = 212235,
            Name = "Immortal King's Tribal Binding",
            Quality = ItemQuality.Legendary,
            Slug = "immortal-kings-tribal-binding",
            ItemType = ItemType.Belt,
            BaseType = ItemBaseType.Armor,
            InternalName = "BarbBelt_norm_unique_09",
            DataUrl = "https://us.battle.net/api/d3/data/item/immortal-kings-tribal-binding",
            Url = "https://us.battle.net/d3/en/item/immortal-kings-tribal-binding",
            RelativeUrl = "/d3/en/item/immortal-kings-tribal-binding",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Immortal King's Call",
        };

        /// <summary>
        /// Goldwrap On gold pickup: Gain armor for 5 seconds equal to the amount picked up.
        /// </summary>
        public static Item Goldwrap = new Item
        {
            Id = 193671,
            Name = "Goldwrap",
            Quality = ItemQuality.Legendary,
            Slug = "goldwrap",
            ItemType = ItemType.Belt,
            BaseType = ItemBaseType.Armor,
            InternalName = "belt_norm_unique_08",
            DataUrl = "https://us.battle.net/api/d3/data/item/goldwrap",
            Url = "https://us.battle.net/d3/en/item/goldwrap",
            RelativeUrl = "/d3/en/item/goldwrap",
            IsCrafted = false,
            LegendaryAffix = "On gold pickup: Gain armor for 5 seconds equal to the amount picked up.",
            SetName = "",
        };

        /// <summary>
        /// Vigilante Belt 
        /// </summary>
        public static Item VigilanteBelt = new Item
        {
            Id = 193665,
            Name = "Vigilante Belt",
            Quality = ItemQuality.Legendary,
            Slug = "vigilante-belt",
            ItemType = ItemType.Belt,
            BaseType = ItemBaseType.Armor,
            InternalName = "Belt_norm_unique_02",
            DataUrl = "https://us.battle.net/api/d3/data/item/vigilante-belt",
            Url = "https://us.battle.net/d3/en/item/vigilante-belt",
            RelativeUrl = "/d3/en/item/vigilante-belt",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Quick Draw Belt 
        /// </summary>
        public static Item QuickDrawBelt = new Item
        {
            Id = 0,
            Name = "Quick Draw Belt",
            Quality = ItemQuality.Legendary,
            Slug = "quick-draw-belt",
            ItemType = ItemType.Belt,
            BaseType = ItemBaseType.Armor,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/quick-draw-belt",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/quick-draw-belt",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/quick-draw-belt",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Saffron Wrap 
        /// </summary>
        public static Item SaffronWrap = new Item
        {
            Id = 193664,
            Name = "Saffron Wrap",
            Quality = ItemQuality.Legendary,
            Slug = "saffron-wrap",
            ItemType = ItemType.Belt,
            BaseType = ItemBaseType.Armor,
            InternalName = "Belt_norm_unique_01",
            DataUrl = "https://us.battle.net/api/d3/data/item/saffron-wrap",
            Url = "https://us.battle.net/d3/en/item/saffron-wrap",
            RelativeUrl = "/d3/en/item/saffron-wrap",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// String of Ears 
        /// </summary>
        public static Item StringOfEars = new Item
        {
            Id = 193669,
            Name = "String of Ears",
            Quality = ItemQuality.Legendary,
            Slug = "string-of-ears",
            ItemType = ItemType.Belt,
            BaseType = ItemBaseType.Armor,
            InternalName = "Belt_norm_unique_06",
            DataUrl = "https://us.battle.net/api/d3/data/item/string-of-ears",
            Url = "https://us.battle.net/d3/en/item/string-of-ears",
            RelativeUrl = "/d3/en/item/string-of-ears",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Captain Crimson's Satin Sash 
        /// </summary>
        public static Item CaptainCrimsonsSatinSash = new Item
        {
            Id = 0,
            Name = "Captain Crimson's Satin Sash",
            Quality = ItemQuality.Legendary,
            Slug = "captain-crimsons-brace",
            ItemType = ItemType.Belt,
            BaseType = ItemBaseType.Armor,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/captain-crimsons-brace",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/captain-crimsons-brace",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/captain-crimsons-brace",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "Captain Crimson's Finery",
        };

        /// <summary>
        /// Cord of the Sherma Chance on hit to create a chaos field that Blinds and Slows enemies inside for 2–4 seconds.
        /// </summary>
        public static Item CordOfTheSherma = new Item
        {
            Id = 298127,
            Name = "Cord of the Sherma",
            Quality = ItemQuality.Legendary,
            Slug = "cord-of-the-sherma",
            ItemType = ItemType.Belt,
            BaseType = ItemBaseType.Armor,
            InternalName = "belt_norm_unique_18",
            DataUrl = "https://us.battle.net/api/d3/data/item/cord-of-the-sherma",
            Url = "https://us.battle.net/d3/en/item/cord-of-the-sherma",
            RelativeUrl = "/d3/en/item/cord-of-the-sherma",
            IsCrafted = false,
            LegendaryAffix = "Chance on hit to create a chaos field that Blinds and Slows enemies inside for 2–4 seconds.",
            SetName = "",
        };

        /// <summary>
        /// Harrington Waistguard Opening a chest grants 100–135% increased damage for 10 seconds.
        /// </summary>
        public static Item HarringtonWaistguard = new Item
        {
            Id = 298129,
            Name = "Harrington Waistguard",
            Quality = ItemQuality.Legendary,
            Slug = "harrington-waistguard",
            ItemType = ItemType.Belt,
            BaseType = ItemBaseType.Armor,
            InternalName = "StaffOfCow",
            DataUrl = "https://us.battle.net/api/d3/data/item/harrington-waistguard",
            Url = "https://us.battle.net/d3/en/item/harrington-waistguard",
            RelativeUrl = "/d3/en/item/harrington-waistguard",
            IsCrafted = false,
            LegendaryAffix = "Opening a chest grants 100–135% increased damage for 10 seconds.",
            SetName = "",
        };

        /// <summary>
        /// Hwoj Wrap Locust Swarm also Slows enemies by 60–80%.
        /// </summary>
        public static Item HwojWrap = new Item
        {
            Id = 193670,
            Name = "Hwoj Wrap",
            Quality = ItemQuality.Legendary,
            Slug = "hwoj-wrap",
            ItemType = ItemType.Belt,
            BaseType = ItemBaseType.Armor,
            InternalName = "Belt_norm_unique_07",
            DataUrl = "https://us.battle.net/api/d3/data/item/hwoj-wrap",
            Url = "https://us.battle.net/d3/en/item/hwoj-wrap",
            RelativeUrl = "/d3/en/item/hwoj-wrap",
            IsCrafted = false,
            LegendaryAffix = "Locust Swarm also Slows enemies by 60–80%.",
            SetName = "",
        };

        /// <summary>
        /// Razor Strop Picking up a Health Globe releases an explosion that deals 300–400% weapon damage as Fire to enemies within 20 yards.
        /// </summary>
        public static Item RazorStrop = new Item
        {
            Id = 298124,
            Name = "Razor Strop",
            Quality = ItemQuality.Legendary,
            Slug = "razor-strop",
            ItemType = ItemType.Belt,
            BaseType = ItemBaseType.Armor,
            InternalName = "belt_norm_unique_15",
            DataUrl = "https://us.battle.net/api/d3/data/item/razor-strop",
            Url = "https://us.battle.net/d3/en/item/razor-strop",
            RelativeUrl = "/d3/en/item/razor-strop",
            IsCrafted = false,
            LegendaryAffix = "Picking up a Health Globe releases an explosion that deals 300–400% weapon damage as Fire to enemies within 20 yards.",
            SetName = "",
        };

        /// <summary>
        /// Sash of Knives 25% chance on attack to throw a dagger at a nearby enemy for 275–350% weapon damage as Physical.
        /// </summary>
        public static Item SashOfKnives = new Item
        {
            Id = 298125,
            Name = "Sash of Knives",
            Quality = ItemQuality.Legendary,
            Slug = "sash-of-knives",
            ItemType = ItemType.Belt,
            BaseType = ItemBaseType.Armor,
            InternalName = "belt_norm_unique_16",
            DataUrl = "https://us.battle.net/api/d3/data/item/sash-of-knives",
            Url = "https://us.battle.net/d3/en/item/sash-of-knives",
            RelativeUrl = "/d3/en/item/sash-of-knives",
            IsCrafted = false,
            LegendaryAffix = "25% chance on attack to throw a dagger at a nearby enemy for 275–350% weapon damage as Physical.",
            SetName = "",
        };

        /// <summary>
        /// Sebor's Nightmare Haunt is cast on 5 nearby enemies when you open a chest.
        /// </summary>
        public static Item SeborsNightmare = new Item
        {
            Id = 299381,
            Name = "Sebor's Nightmare",
            Quality = ItemQuality.Legendary,
            Slug = "sebors-nightmare",
            ItemType = ItemType.Belt,
            BaseType = ItemBaseType.Armor,
            InternalName = "belt_norm_unique_22",
            DataUrl = "https://us.battle.net/api/d3/data/item/sebors-nightmare",
            Url = "https://us.battle.net/d3/en/item/sebors-nightmare",
            RelativeUrl = "/d3/en/item/sebors-nightmare",
            IsCrafted = false,
            LegendaryAffix = "Haunt is cast on 5 nearby enemies when you open a chest.",
            SetName = "",
        };

        /// <summary>
        /// Angel Hair Braid Punish gains the effect of every rune.
        /// </summary>
        public static Item AngelHairBraid = new Item
        {
            Id = 193666,
            Name = "Angel Hair Braid",
            Quality = ItemQuality.Legendary,
            Slug = "angel-hair-braid",
            ItemType = ItemType.Belt,
            BaseType = ItemBaseType.Armor,
            InternalName = "belt_norm_unique_03",
            DataUrl = "https://us.battle.net/api/d3/data/item/angel-hair-braid",
            Url = "https://us.battle.net/d3/en/item/angel-hair-braid",
            RelativeUrl = "/d3/en/item/angel-hair-braid",
            IsCrafted = false,
            LegendaryAffix = "Punish gains the effect of every rune.",
            SetName = "",
        };

        /// <summary>
        /// Thundergod's Vigor Blocking, dodging or being hit causes you to discharge bolts of electricity that deal 100–130% weapon damage as Lightning.
        /// </summary>
        public static Item ThundergodsVigor = new Item
        {
            Id = 212230,
            Name = "Thundergod's Vigor",
            Quality = ItemQuality.Legendary,
            Slug = "thundergods-vigor",
            ItemType = ItemType.Belt,
            BaseType = ItemBaseType.Armor,
            InternalName = "BarbBelt_norm_unique_03",
            DataUrl = "https://us.battle.net/api/d3/data/item/thundergods-vigor",
            Url = "https://us.battle.net/d3/en/item/thundergods-vigor",
            RelativeUrl = "/d3/en/item/thundergods-vigor",
            IsCrafted = false,
            LegendaryAffix = "Blocking, dodging or being hit causes you to discharge bolts of electricity that deal 100–130% weapon damage as Lightning.",
            SetName = "",
        };

        /// <summary>
        /// Guardian's Sheath 
        /// </summary>
        public static Item GuardiansSheath = new Item
        {
            Id = 0,
            Name = "Guardian's Sheath",
            Quality = ItemQuality.Legendary,
            Slug = "guardians-scabbard",
            ItemType = ItemType.Belt,
            BaseType = ItemBaseType.Armor,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/guardians-scabbard",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/guardians-scabbard",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/guardians-scabbard",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "Guardian's Contingency",
        };

        /// <summary>
        /// Hellcat Waistguard 
        /// </summary>
        public static Item HellcatWaistguard = new Item
        {
            Id = 193668,
            Name = "Hellcat Waistguard",
            Quality = ItemQuality.Legendary,
            Slug = "hellcat-waistguard",
            ItemType = ItemType.Belt,
            BaseType = ItemBaseType.Armor,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/item/hellcat-waistguard",
            Url = "https://us.battle.net/d3/en/item/hellcat-waistguard",
            RelativeUrl = "/d3/en/item/hellcat-waistguard",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// The Witching Hour 
        /// </summary>
        public static Item TheWitchingHour = new Item
        {
            Id = 0,
            Name = "The Witching Hour",
            Quality = ItemQuality.Legendary,
            Slug = "the-witching-hour",
            ItemType = ItemType.Belt,
            BaseType = ItemBaseType.Armor,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/item/the-witching-hour",
            Url = "https://us.battle.net/d3/en/item/the-witching-hour",
            RelativeUrl = "/d3/en/item/the-witching-hour",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Blackthorne's Notched Belt 
        /// </summary>
        public static Item BlackthornesNotchedBelt = new Item
        {
            Id = 224191,
            Name = "Blackthorne's Notched Belt",
            Quality = ItemQuality.Legendary,
            Slug = "blackthornes-notched-belt",
            ItemType = ItemType.Belt,
            BaseType = ItemBaseType.Armor,
            InternalName = "Belt_norm_unique_14",
            DataUrl = "https://us.battle.net/api/d3/data/item/blackthornes-notched-belt",
            Url = "https://us.battle.net/d3/en/item/blackthornes-notched-belt",
            RelativeUrl = "/d3/en/item/blackthornes-notched-belt",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Blackthorne's Battlegear",
        };

        /// <summary>
        /// Inna's Favor 
        /// </summary>
        public static Item InnasFavor = new Item
        {
            Id = 222487,
            Name = "Inna's Favor",
            Quality = ItemQuality.Legendary,
            Slug = "innas-favor",
            ItemType = ItemType.Belt,
            BaseType = ItemBaseType.Armor,
            InternalName = "Belt_norm_unique_10",
            DataUrl = "https://us.battle.net/api/d3/data/item/innas-favor",
            Url = "https://us.battle.net/d3/en/item/innas-favor",
            RelativeUrl = "/d3/en/item/innas-favor",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Inna's Mantra",
        };

        /// <summary>
        /// Tal Rasha's Brace 
        /// </summary>
        public static Item TalRashasBrace = new Item
        {
            Id = 212657,
            Name = "Tal Rasha's Brace",
            Quality = ItemQuality.Legendary,
            Slug = "tal-rashas-brace",
            ItemType = ItemType.Belt,
            BaseType = ItemBaseType.Armor,
            InternalName = "Belt_norm_unique_09",
            DataUrl = "https://us.battle.net/api/d3/data/item/tal-rashas-brace",
            Url = "https://us.battle.net/d3/en/item/tal-rashas-brace",
            RelativeUrl = "/d3/en/item/tal-rashas-brace",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Tal Rasha's Elements",
        };

        /// <summary>
        /// Demon's Lock 
        /// </summary>
        public static Item DemonsLock = new Item
        {
            Id = 0,
            Name = "Demon's Lock",
            Quality = ItemQuality.Legendary,
            Slug = "demons-binding",
            ItemType = ItemType.Belt,
            BaseType = ItemBaseType.Armor,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/demons-binding",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/demons-binding",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/demons-binding",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "Demon's Skin",
        };

        /// <summary>
        /// Jang's Envelopment Enemies damaged by Black Hole are also slowed by 60–80% for 3 seconds.
        /// </summary>
        public static Item JangsEnvelopment = new Item
        {
            Id = 298130,
            Name = "Jang's Envelopment",
            Quality = ItemQuality.Legendary,
            Slug = "jangs-envelopment",
            ItemType = ItemType.Belt,
            BaseType = ItemBaseType.Armor,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/item/jangs-envelopment",
            Url = "https://us.battle.net/d3/en/item/jangs-envelopment",
            RelativeUrl = "/d3/en/item/jangs-envelopment",
            IsCrafted = false,
            LegendaryAffix = "Enemies damaged by Black Hole are also slowed by 60–80% for 3 seconds.",
            SetName = "",
        };

        /// <summary>
        /// Krelm's Buff Belt Gain 25% run speed. This effect is lost for 5 seconds after taking damage.
        /// </summary>
        public static Item KrelmsBuffBelt = new Item
        {
            Id = 0,
            Name = "Krelm's Buff Belt",
            Quality = ItemQuality.Legendary,
            Slug = "krelms-buff-belt",
            ItemType = ItemType.Belt,
            BaseType = ItemBaseType.Armor,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/item/krelms-buff-belt",
            Url = "https://us.battle.net/d3/en/item/krelms-buff-belt",
            RelativeUrl = "/d3/en/item/krelms-buff-belt",
            IsCrafted = false,
            LegendaryAffix = "Gain 25% run speed. This effect is lost for 5 seconds after taking damage.",
            SetName = "Krelm’s Buff Bulwark",
        };

        /// <summary>
        /// Fleeting Strap 
        /// </summary>
        public static Item FleetingStrap = new Item
        {
            Id = 193667,
            Name = "Fleeting Strap",
            Quality = ItemQuality.Legendary,
            Slug = "fleeting-strap",
            ItemType = ItemType.Belt,
            BaseType = ItemBaseType.Armor,
            InternalName = "Belt_norm_unique_04",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/fleeting-strap",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/fleeting-strap",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/fleeting-strap",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Captain Crimson's Silk Girdle 
        /// </summary>
        public static Item CaptainCrimsonsSilkGirdle = new Item
        {
            Id = 222974,
            Name = "Captain Crimson's Silk Girdle",
            Quality = ItemQuality.Legendary,
            Slug = "captain-crimsons-silk-girdle",
            ItemType = ItemType.Belt,
            BaseType = ItemBaseType.Armor,
            InternalName = "Belt_norm_unique_12",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/captain-crimsons-silk-girdle",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/captain-crimsons-silk-girdle",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/captain-crimsons-silk-girdle",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "Captain Crimson's Trimmings",
        };

        /// <summary>
        /// Demon's Restraint 
        /// </summary>
        public static Item DemonsRestraint = new Item
        {
            Id = 0,
            Name = "Demon's Restraint",
            Quality = ItemQuality.Legendary,
            Slug = "demons-restraint",
            ItemType = ItemType.Belt,
            BaseType = ItemBaseType.Armor,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/demons-restraint",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/demons-restraint",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/demons-restraint",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "Demon's Hide",
        };

        /// <summary>
        /// Guardian's Case 
        /// </summary>
        public static Item GuardiansCase = new Item
        {
            Id = 222976,
            Name = "Guardian's Case",
            Quality = ItemQuality.Legendary,
            Slug = "guardians-case",
            ItemType = ItemType.Belt,
            BaseType = ItemBaseType.Armor,
            InternalName = "Belt_norm_unique_13",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/guardians-case",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/guardians-case",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/guardians-case",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "Guardian's Jeopardy",
        };

        /// <summary>
        /// Hellfire Amulet Strength 
        /// </summary>
        public static Item HellfireAmuletStrength = new Item
        {
            Id = 0,
            Name = "Hellfire Amulet Strength",
            Quality = ItemQuality.Legendary,
            Slug = "hellfire-amulet-of-strength",
            ItemType = ItemType.Amulet,
            BaseType = ItemBaseType.None,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/hellfire-amulet-of-strength",
            Url = "https://us.battle.net/d3/en/artisan/jeweler/recipe/hellfire-amulet-of-strength",
            RelativeUrl = "/d3/en/artisan/jeweler/recipe/hellfire-amulet-of-strength",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Hellfire Amulet Intelligence 
        /// </summary>
        public static Item HellfireAmuletIntelligence = new Item
        {
            Id = 0,
            Name = "Hellfire Amulet Intelligence",
            Quality = ItemQuality.Legendary,
            Slug = "hellfire-amulet-of-intelligence",
            ItemType = ItemType.Amulet,
            BaseType = ItemBaseType.None,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/hellfire-amulet-of-intelligence",
            Url = "https://us.battle.net/d3/en/artisan/jeweler/recipe/hellfire-amulet-of-intelligence",
            RelativeUrl = "/d3/en/artisan/jeweler/recipe/hellfire-amulet-of-intelligence",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Hellfire Amulet Dexterity 
        /// </summary>
        public static Item HellfireAmuletDexterity = new Item
        {
            Id = 0,
            Name = "Hellfire Amulet Dexterity",
            Quality = ItemQuality.Legendary,
            Slug = "hellfire-amulet-of-dexterity",
            ItemType = ItemType.Amulet,
            BaseType = ItemBaseType.None,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/hellfire-amulet-of-dexterity",
            Url = "https://us.battle.net/d3/en/artisan/jeweler/recipe/hellfire-amulet-of-dexterity",
            RelativeUrl = "/d3/en/artisan/jeweler/recipe/hellfire-amulet-of-dexterity",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Moonlight Ward Hitting an enemy within 15 yards has a chance to ward you with shards of Arcane energy that explode when enemies get close, dealing 240–320% weapon damage as Arcane to enemies within 15 yards.
        /// </summary>
        public static Item MoonlightWard = new Item
        {
            Id = 197813,
            Name = "Moonlight Ward",
            Quality = ItemQuality.Legendary,
            Slug = "moonlight-ward",
            ItemType = ItemType.Amulet,
            BaseType = ItemBaseType.None,
            InternalName = "Amulet_norm_unique_03",
            DataUrl = "https://us.battle.net/api/d3/data/item/moonlight-ward",
            Url = "https://us.battle.net/d3/en/item/moonlight-ward",
            RelativeUrl = "/d3/en/item/moonlight-ward",
            IsCrafted = false,
            LegendaryAffix = "Hitting an enemy within 15 yards has a chance to ward you with shards of Arcane energy that explode when enemies get close, dealing 240–320% weapon damage as Arcane to enemies within 15 yards.",
            SetName = "",
        };

        /// <summary>
        /// Squirt's Necklace 
        /// </summary>
        public static Item SquirtsNecklace = new Item
        {
            Id = 197819,
            Name = "Squirt's Necklace",
            Quality = ItemQuality.Legendary,
            Slug = "squirts-necklace",
            ItemType = ItemType.Amulet,
            BaseType = ItemBaseType.None,
            InternalName = "amulet_norm_unique_08",
            DataUrl = "https://us.battle.net/api/d3/data/item/squirts-necklace",
            Url = "https://us.battle.net/d3/en/item/squirts-necklace",
            RelativeUrl = "/d3/en/item/squirts-necklace",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Eye of Etlich 
        /// </summary>
        public static Item EyeOfEtlich = new Item
        {
            Id = 197823,
            Name = "Eye of Etlich",
            Quality = ItemQuality.Legendary,
            Slug = "eye-of-etlich",
            ItemType = ItemType.Amulet,
            BaseType = ItemBaseType.None,
            InternalName = "Amulet_norm_unique_12",
            DataUrl = "https://us.battle.net/api/d3/data/item/eye-of-etlich",
            Url = "https://us.battle.net/d3/en/item/eye-of-etlich",
            RelativeUrl = "/d3/en/item/eye-of-etlich",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Rondal's Locket 
        /// </summary>
        public static Item RondalsLocket = new Item
        {
            Id = 197818,
            Name = "Rondal's Locket",
            Quality = ItemQuality.Legendary,
            Slug = "rondals-locket",
            ItemType = ItemType.Amulet,
            BaseType = ItemBaseType.None,
            InternalName = "Amulet_norm_unique_07",
            DataUrl = "https://us.battle.net/api/d3/data/item/rondals-locket",
            Url = "https://us.battle.net/d3/en/item/rondals-locket",
            RelativeUrl = "/d3/en/item/rondals-locket",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Talisman of Aranoch Prevent all Cold damage taken and heal yourself for 10–15% of the amount prevented.
        /// </summary>
        public static Item TalismanOfAranoch = new Item
        {
            Id = 197821,
            Name = "Talisman of Aranoch",
            Quality = ItemQuality.Legendary,
            Slug = "talisman-of-aranoch",
            ItemType = ItemType.Amulet,
            BaseType = ItemBaseType.None,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/item/talisman-of-aranoch",
            Url = "https://us.battle.net/d3/en/item/talisman-of-aranoch",
            RelativeUrl = "/d3/en/item/talisman-of-aranoch",
            IsCrafted = false,
            LegendaryAffix = "Prevent all Cold damage taken and heal yourself for 10–15% of the amount prevented.",
            SetName = "",
        };

        /// <summary>
        /// Ancestors' Grace When receiving fatal damage, you are instead restored to 100% of maximum Life and resources. This item is destroyed in the process.
        /// </summary>
        public static Item AncestorsGrace = new Item
        {
            Id = 0,
            Name = "Ancestors' Grace",
            Quality = ItemQuality.Legendary,
            Slug = "ancestors-grace",
            ItemType = ItemType.Amulet,
            BaseType = ItemBaseType.None,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/item/ancestors-grace",
            Url = "https://us.battle.net/d3/en/item/ancestors-grace",
            RelativeUrl = "/d3/en/item/ancestors-grace",
            IsCrafted = false,
            LegendaryAffix = "When receiving fatal damage, you are instead restored to 100% of maximum Life and resources. This item is destroyed in the process.",
            SetName = "",
        };

        /// <summary>
        /// Countess Julia's Cameo Prevent all Arcane damage taken and heal yourself for 20–25% of the amount prevented.
        /// </summary>
        public static Item CountessJuliasCameo = new Item
        {
            Id = 192468,
            Name = "Countess Julia's Cameo",
            Quality = ItemQuality.Legendary,
            Slug = "countess-julias-cameo",
            ItemType = ItemType.Amulet,
            BaseType = ItemBaseType.None,
            InternalName = "Mojo_norm_unique_02",
            DataUrl = "https://us.battle.net/api/d3/data/item/countess-julias-cameo",
            Url = "https://us.battle.net/d3/en/item/countess-julias-cameo",
            RelativeUrl = "/d3/en/item/countess-julias-cameo",
            IsCrafted = false,
            LegendaryAffix = "Prevent all Arcane damage taken and heal yourself for 20–25% of the amount prevented.",
            SetName = "",
        };

        /// <summary>
        /// Dovu Energy Trap Increases duration of Stun effects by 20–25%.
        /// </summary>
        public static Item DovuEnergyTrap = new Item
        {
            Id = 298054,
            Name = "Dovu Energy Trap",
            Quality = ItemQuality.Legendary,
            Slug = "dovu-energy-trap",
            ItemType = ItemType.Amulet,
            BaseType = ItemBaseType.None,
            InternalName = "amulet_norm_unique_23",
            DataUrl = "https://us.battle.net/api/d3/data/item/dovu-energy-trap",
            Url = "https://us.battle.net/d3/en/item/dovu-energy-trap",
            RelativeUrl = "/d3/en/item/dovu-energy-trap",
            IsCrafted = false,
            LegendaryAffix = "Increases duration of Stun effects by 20–25%.",
            SetName = "",
        };

        /// <summary>
        /// Haunt of Vaxo Summons shadow clones to your aid when you Stun an enemy. This effect may occur once every 30 seconds.
        /// </summary>
        public static Item HauntOfVaxo = new Item
        {
            Id = 222464,
            Name = "Haunt of Vaxo",
            Quality = ItemQuality.Legendary,
            Slug = "haunt-of-vaxo",
            ItemType = ItemType.Amulet,
            BaseType = ItemBaseType.None,
            InternalName = "Boots_norm_unique_051",
            DataUrl = "https://us.battle.net/api/d3/data/item/haunt-of-vaxo",
            Url = "https://us.battle.net/d3/en/item/haunt-of-vaxo",
            RelativeUrl = "/d3/en/item/haunt-of-vaxo",
            IsCrafted = false,
            LegendaryAffix = "Summons shadow clones to your aid when you Stun an enemy. This effect may occur once every 30 seconds.",
            SetName = "",
        };

        /// <summary>
        /// Rakoff's Glass of Life Enemies you kill have a 3–4% additional chance to drop a health globe.
        /// </summary>
        public static Item RakoffsGlassOfLife = new Item
        {
            Id = 298055,
            Name = "Rakoff's Glass of Life",
            Quality = ItemQuality.Legendary,
            Slug = "rakoffs-glass-of-life",
            ItemType = ItemType.Amulet,
            BaseType = ItemBaseType.None,
            InternalName = "amulet_norm_unique_24",
            DataUrl = "https://us.battle.net/api/d3/data/item/rakoffs-glass-of-life",
            Url = "https://us.battle.net/d3/en/item/rakoffs-glass-of-life",
            RelativeUrl = "/d3/en/item/rakoffs-glass-of-life",
            IsCrafted = false,
            LegendaryAffix = "Enemies you kill have a 3–4% additional chance to drop a health globe.",
            SetName = "",
        };

        /// <summary>
        /// The Ess of Johan Chance on hit to pull in enemies toward your target and Slow them by 60–80%.
        /// </summary>
        public static Item TheEssOfJohan = new Item
        {
            Id = 298051,
            Name = "The Ess of Johan",
            Quality = ItemQuality.Legendary,
            Slug = "the-ess-of-johan",
            ItemType = ItemType.Amulet,
            BaseType = ItemBaseType.None,
            InternalName = "amulet_norm_unique_20",
            DataUrl = "https://us.battle.net/api/d3/data/item/the-ess-of-johan",
            Url = "https://us.battle.net/d3/en/item/the-ess-of-johan",
            RelativeUrl = "/d3/en/item/the-ess-of-johan",
            IsCrafted = false,
            LegendaryAffix = "Chance on hit to pull in enemies toward your target and Slow them by 60–80%.",
            SetName = "",
        };

        /// <summary>
        /// Holy Beacon 
        /// </summary>
        public static Item HolyBeacon = new Item
        {
            Id = 197822,
            Name = "Holy Beacon",
            Quality = ItemQuality.Legendary,
            Slug = "holy-beacon",
            ItemType = ItemType.Amulet,
            BaseType = ItemBaseType.None,
            InternalName = "amulet_norm_unique_11",
            DataUrl = "https://us.battle.net/api/d3/data/item/holy-beacon",
            Url = "https://us.battle.net/d3/en/item/holy-beacon",
            RelativeUrl = "/d3/en/item/holy-beacon",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Kymbo's Gold Picking up gold heals you for an amount equal to the gold that was picked up.
        /// </summary>
        public static Item KymbosGold = new Item
        {
            Id = 216525,
            Name = "Kymbo's Gold",
            Quality = ItemQuality.Legendary,
            Slug = "kymbos-gold",
            ItemType = ItemType.Amulet,
            BaseType = ItemBaseType.None,
            InternalName = "Mojo_norm_unique_07",
            DataUrl = "https://us.battle.net/api/d3/data/item/kymbos-gold",
            Url = "https://us.battle.net/d3/en/item/kymbos-gold",
            RelativeUrl = "/d3/en/item/kymbos-gold",
            IsCrafted = false,
            LegendaryAffix = "Picking up gold heals you for an amount equal to the gold that was picked up.",
            SetName = "",
        };

        /// <summary>
        /// The Flavor of Time 
        /// </summary>
        public static Item TheFlavorOfTime = new Item
        {
            Id = 193659,
            Name = "The Flavor of Time",
            Quality = ItemQuality.Legendary,
            Slug = "the-flavor-of-time",
            ItemType = ItemType.Amulet,
            BaseType = ItemBaseType.None,
            InternalName = "Amulet_norm_unique_01",
            DataUrl = "https://us.battle.net/api/d3/data/item/the-flavor-of-time",
            Url = "https://us.battle.net/d3/en/item/the-flavor-of-time",
            RelativeUrl = "/d3/en/item/the-flavor-of-time",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Mara's Kaleidoscope Prevent all Poison damage taken and heal yourself for 10–15% of the amount prevented.
        /// </summary>
        public static Item MarasKaleidoscope = new Item
        {
            Id = 197824,
            Name = "Mara's Kaleidoscope",
            Quality = ItemQuality.Legendary,
            Slug = "maras-kaleidoscope",
            ItemType = ItemType.Amulet,
            BaseType = ItemBaseType.None,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/item/maras-kaleidoscope",
            Url = "https://us.battle.net/d3/en/item/maras-kaleidoscope",
            RelativeUrl = "/d3/en/item/maras-kaleidoscope",
            IsCrafted = false,
            LegendaryAffix = "Prevent all Poison damage taken and heal yourself for 10–15% of the amount prevented.",
            SetName = "",
        };

        /// <summary>
        /// Ouroboros 
        /// </summary>
        public static Item Ouroboros = new Item
        {
            Id = 197815,
            Name = "Ouroboros",
            Quality = ItemQuality.Legendary,
            Slug = "ouroboros",
            ItemType = ItemType.Amulet,
            BaseType = ItemBaseType.None,
            InternalName = "Amulet_norm_unique_05",
            DataUrl = "https://us.battle.net/api/d3/data/item/ouroboros",
            Url = "https://us.battle.net/d3/en/item/ouroboros",
            RelativeUrl = "/d3/en/item/ouroboros",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// The Star of Azkaranth Prevent all Fire damage taken and heal yourself for 10–15% of the amount prevented.
        /// </summary>
        public static Item TheStarOfAzkaranth = new Item
        {
            Id = 197817,
            Name = "The Star of Azkaranth",
            Quality = ItemQuality.Legendary,
            Slug = "the-star-of-azkaranth",
            ItemType = ItemType.Amulet,
            BaseType = ItemBaseType.None,
            InternalName = "amulet_norm_unique_06",
            DataUrl = "https://us.battle.net/api/d3/data/item/the-star-of-azkaranth",
            Url = "https://us.battle.net/d3/en/item/the-star-of-azkaranth",
            RelativeUrl = "/d3/en/item/the-star-of-azkaranth",
            IsCrafted = false,
            LegendaryAffix = "Prevent all Fire damage taken and heal yourself for 10–15% of the amount prevented.",
            SetName = "",
        };

        /// <summary>
        /// Xephirian Amulet Prevent all Lightning damage taken and heal yourself for 10–15% of the amount prevented.
        /// </summary>
        public static Item XephirianAmulet = new Item
        {
            Id = 197814,
            Name = "Xephirian Amulet",
            Quality = ItemQuality.Legendary,
            Slug = "xephirian-amulet",
            ItemType = ItemType.Amulet,
            BaseType = ItemBaseType.None,
            InternalName = "Amulet_norm_unique_04",
            DataUrl = "https://us.battle.net/api/d3/data/item/xephirian-amulet",
            Url = "https://us.battle.net/d3/en/item/xephirian-amulet",
            RelativeUrl = "/d3/en/item/xephirian-amulet",
            IsCrafted = false,
            LegendaryAffix = "Prevent all Lightning damage taken and heal yourself for 10–15% of the amount prevented.",
            SetName = "",
        };

        /// <summary>
        /// Blackthorne's Duncraig Cross 
        /// </summary>
        public static Item BlackthornesDuncraigCross = new Item
        {
            Id = 224189,
            Name = "Blackthorne's Duncraig Cross",
            Quality = ItemQuality.Legendary,
            Slug = "blackthornes-duncraig-cross",
            ItemType = ItemType.Amulet,
            BaseType = ItemBaseType.None,
            InternalName = "amulet_norm_unique_19",
            DataUrl = "https://us.battle.net/api/d3/data/item/blackthornes-duncraig-cross",
            Url = "https://us.battle.net/d3/en/item/blackthornes-duncraig-cross",
            RelativeUrl = "/d3/en/item/blackthornes-duncraig-cross",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Blackthorne's Battlegear",
        };

        /// <summary>
        /// Tal Rasha's Allegiance 
        /// </summary>
        public static Item TalRashasAllegiance = new Item
        {
            Id = 222486,
            Name = "Tal Rasha's Allegiance",
            Quality = ItemQuality.Legendary,
            Slug = "tal-rashas-allegiance",
            ItemType = ItemType.Amulet,
            BaseType = ItemBaseType.None,
            InternalName = "Amulet_norm_unique_14",
            DataUrl = "https://us.battle.net/api/d3/data/item/tal-rashas-allegiance",
            Url = "https://us.battle.net/d3/en/item/tal-rashas-allegiance",
            RelativeUrl = "/d3/en/item/tal-rashas-allegiance",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Tal Rasha's Elements",
        };

        /// <summary>
        /// The Traveler's Pledge 
        /// </summary>
        public static Item TheTravelersPledge = new Item
        {
            Id = 222490,
            Name = "The Traveler's Pledge",
            Quality = ItemQuality.Legendary,
            Slug = "the-travelers-pledge",
            ItemType = ItemType.Amulet,
            BaseType = ItemBaseType.None,
            InternalName = "Amulet_norm_unique_15",
            DataUrl = "https://us.battle.net/api/d3/data/item/the-travelers-pledge",
            Url = "https://us.battle.net/d3/en/item/the-travelers-pledge",
            RelativeUrl = "/d3/en/item/the-travelers-pledge",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Endless Walk",
        };

        /// <summary>
        /// Halcyon's Ascent 
        /// </summary>
        public static Item HalcyonsAscent = new Item
        {
            Id = 298056,
            Name = "Halcyon's Ascent",
            Quality = ItemQuality.Legendary,
            Slug = "halcyons-ascent",
            ItemType = ItemType.Amulet,
            BaseType = ItemBaseType.None,
            InternalName = "amulet_norm_unique_25",
            DataUrl = "https://us.battle.net/api/d3/data/item/halcyons-ascent",
            Url = "https://us.battle.net/d3/en/item/halcyons-ascent",
            RelativeUrl = "/d3/en/item/halcyons-ascent",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Sunwuko's Shines 
        /// </summary>
        public static Item SunwukosShines = new Item
        {
            Id = 336174,
            Name = "Sunwuko's Shines",
            Quality = ItemQuality.Legendary,
            Slug = "sunwukos-shines",
            ItemType = ItemType.Amulet,
            BaseType = ItemBaseType.None,
            InternalName = "amulet_norm_set_11",
            DataUrl = "https://us.battle.net/api/d3/data/item/sunwukos-shines",
            Url = "https://us.battle.net/d3/en/item/sunwukos-shines",
            RelativeUrl = "/d3/en/item/sunwukos-shines",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Monkey King's Garb",
        };

        /// <summary>
        /// Gazing Demise 
        /// </summary>
        public static Item GazingDemise = new Item
        {
            Id = 194995,
            Name = "Gazing Demise",
            Quality = ItemQuality.Legendary,
            Slug = "gazing-demise",
            ItemType = ItemType.Mojo,
            BaseType = ItemBaseType.None,
            InternalName = "Mojo_norm_unique_05",
            DataUrl = "https://us.battle.net/api/d3/data/item/gazing-demise",
            Url = "https://us.battle.net/d3/en/item/gazing-demise",
            RelativeUrl = "/d3/en/item/gazing-demise",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Homunculus A Zombie Dog is automatically summoned to your side every 4–6 seconds.
        /// </summary>
        public static Item Homunculus = new Item
        {
            Id = 194991,
            Name = "Homunculus",
            Quality = ItemQuality.Legendary,
            Slug = "homunculus",
            ItemType = ItemType.Mojo,
            BaseType = ItemBaseType.None,
            InternalName = "Mojo_norm_unique_04",
            DataUrl = "https://us.battle.net/api/d3/data/item/homunculus",
            Url = "https://us.battle.net/d3/en/item/homunculus",
            RelativeUrl = "/d3/en/item/homunculus",
            IsCrafted = false,
            LegendaryAffix = "A Zombie Dog is automatically summoned to your side every 4–6 seconds.",
            SetName = "",
        };

        /// <summary>
        /// Bitterness 
        /// </summary>
        public static Item Bitterness = new Item
        {
            Id = 194988,
            Name = "Bitterness",
            Quality = ItemQuality.Legendary,
            Slug = "bitterness",
            ItemType = ItemType.Mojo,
            BaseType = ItemBaseType.None,
            InternalName = "Mojo_norm_unique_03",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/bitterness",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/bitterness",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/bitterness",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Shukrani's Triumph Spirit Walk lasts until you attack or until an enemy is within 30 yards of you.
        /// </summary>
        public static Item ShukranisTriumph = new Item
        {
            Id = 272070,
            Name = "Shukrani's Triumph",
            Quality = ItemQuality.Legendary,
            Slug = "shukranis-triumph",
            ItemType = ItemType.Mojo,
            BaseType = ItemBaseType.None,
            InternalName = "mojo_norm_unique_11",
            DataUrl = "https://us.battle.net/api/d3/data/item/shukranis-triumph",
            Url = "https://us.battle.net/d3/en/item/shukranis-triumph",
            RelativeUrl = "/d3/en/item/shukranis-triumph",
            IsCrafted = false,
            LegendaryAffix = "Spirit Walk lasts until you attack or until an enemy is within 30 yards of you.",
            SetName = "",
        };

        /// <summary>
        /// Thing of the Deep 
        /// </summary>
        public static Item ThingOfTheDeep = new Item
        {
            Id = 0,
            Name = "Thing of the Deep",
            Quality = ItemQuality.Legendary,
            Slug = "thing-of-the-deep",
            ItemType = ItemType.Mojo,
            BaseType = ItemBaseType.None,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/item/thing-of-the-deep",
            Url = "https://us.battle.net/d3/en/item/thing-of-the-deep",
            RelativeUrl = "/d3/en/item/thing-of-the-deep",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Uhkapian Serpent 25–30% of the damage you take is redirected to your Zombie Dogs.
        /// </summary>
        public static Item UhkapianSerpent = new Item
        {
            Id = 191278,
            Name = "Uhkapian Serpent",
            Quality = ItemQuality.Legendary,
            Slug = "uhkapian-serpent",
            ItemType = ItemType.Mojo,
            BaseType = ItemBaseType.None,
            InternalName = "Mojo_norm_unique_01",
            DataUrl = "https://us.battle.net/api/d3/data/item/uhkapian-serpent",
            Url = "https://us.battle.net/d3/en/item/uhkapian-serpent",
            RelativeUrl = "/d3/en/item/uhkapian-serpent",
            IsCrafted = false,
            LegendaryAffix = "25–30% of the damage you take is redirected to your Zombie Dogs.",
            SetName = "",
        };

        /// <summary>
        /// Manajuma's Gory Fetch 
        /// </summary>
        public static Item ManajumasGoryFetch = new Item
        {
            Id = 210993,
            Name = "Manajuma's Gory Fetch",
            Quality = ItemQuality.Legendary,
            Slug = "manajumas-gory-fetch",
            ItemType = ItemType.Mojo,
            BaseType = ItemBaseType.None,
            InternalName = "Mojo_norm_unique_06",
            DataUrl = "https://us.battle.net/api/d3/data/item/manajumas-gory-fetch",
            Url = "https://us.battle.net/d3/en/item/manajumas-gory-fetch",
            RelativeUrl = "/d3/en/item/manajumas-gory-fetch",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Manajuma's Way",
        };

        /// <summary>
        /// Zunimassa's String of Skulls 
        /// </summary>
        public static Item ZunimassasStringOfSkulls = new Item
        {
            Id = 216525,
            Name = "Zunimassa's String of Skulls",
            Quality = ItemQuality.Legendary,
            Slug = "zunimassas-string-of-skulls",
            ItemType = ItemType.Mojo,
            BaseType = ItemBaseType.None,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/item/zunimassas-string-of-skulls",
            Url = "https://us.battle.net/d3/en/item/zunimassas-string-of-skulls",
            RelativeUrl = "/d3/en/item/zunimassas-string-of-skulls",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Zunimassa's Haunt",
        };

        /// <summary>
        /// Spite 
        /// </summary>
        public static Item Spite = new Item
        {
            Id = 0,
            Name = "Spite",
            Quality = ItemQuality.Legendary,
            Slug = "spite",
            ItemType = ItemType.Mojo,
            BaseType = ItemBaseType.None,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/spite",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/spite",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/spite",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Pox Faulds When 3 or more enemies are within 12 yards, you release a vile stench that deals 240–320% weapon damage as Poison every second for 10 seconds to enemies within 15 yards.
        /// </summary>
        public static Item PoxFaulds = new Item
        {
            Id = 197220,
            Name = "Pox Faulds",
            Quality = ItemQuality.Legendary,
            Slug = "pox-faulds",
            ItemType = ItemType.Legs,
            BaseType = ItemBaseType.Armor,
            InternalName = "pants_norm_unique_048",
            DataUrl = "https://us.battle.net/api/d3/data/item/pox-faulds",
            Url = "https://us.battle.net/d3/en/item/pox-faulds",
            RelativeUrl = "/d3/en/item/pox-faulds",
            IsCrafted = false,
            LegendaryAffix = "When 3 or more enemies are within 12 yards, you release a vile stench that deals 240–320% weapon damage as Poison every second for 10 seconds to enemies within 15 yards.",
            SetName = "",
        };

        /// <summary>
        /// Cain's Robes 
        /// </summary>
        public static Item CainsRobes = new Item
        {
            Id = 0,
            Name = "Cain's Robes",
            Quality = ItemQuality.Legendary,
            Slug = "cains-raiment",
            ItemType = ItemType.Legs,
            BaseType = ItemBaseType.Armor,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/cains-raiment",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/cains-raiment",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/cains-raiment",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "Cain's Fate",
        };

        /// <summary>
        /// Hammer Jammers 
        /// </summary>
        public static Item HammerJammers = new Item
        {
            Id = 209059,
            Name = "Hammer Jammers",
            Quality = ItemQuality.Legendary,
            Slug = "hammer-jammers",
            ItemType = ItemType.Legs,
            BaseType = ItemBaseType.Armor,
            InternalName = "pants_norm_unique_077",
            DataUrl = "https://us.battle.net/api/d3/data/item/hammer-jammers",
            Url = "https://us.battle.net/d3/en/item/hammer-jammers",
            RelativeUrl = "/d3/en/item/hammer-jammers",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Captain Crimson's Bowsprit 
        /// </summary>
        public static Item CaptainCrimsonsBowsprit = new Item
        {
            Id = 0,
            Name = "Captain Crimson's Bowsprit",
            Quality = ItemQuality.Legendary,
            Slug = "captain-crimsons-codpiece",
            ItemType = ItemType.Legs,
            BaseType = ItemBaseType.Armor,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/captain-crimsons-codpiece",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/captain-crimsons-codpiece",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/captain-crimsons-codpiece",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "Captain Crimson's Finery",
        };

        /// <summary>
        /// Hexing Pants of Mr. Yan Your resource generation and damage is increased by 25% while moving and decreased by 20–25% while standing still.
        /// </summary>
        public static Item HexingPantsOfMrYan = new Item
        {
            Id = 332204,
            Name = "Hexing Pants of Mr. Yan",
            Quality = ItemQuality.Legendary,
            Slug = "hexing-pants-of-mr-yan",
            ItemType = ItemType.Legs,
            BaseType = ItemBaseType.Armor,
            InternalName = "pants_norm_unique_01",
            DataUrl = "https://us.battle.net/api/d3/data/item/hexing-pants-of-mr-yan",
            Url = "https://us.battle.net/d3/en/item/hexing-pants-of-mr-yan",
            RelativeUrl = "/d3/en/item/hexing-pants-of-mr-yan",
            IsCrafted = false,
            LegendaryAffix = "Your resource generation and damage is increased by 25% while moving and decreased by 20–25% while standing still.",
            SetName = "",
        };

        /// <summary>
        /// Swamp Land Waders 
        /// </summary>
        public static Item SwampLandWaders = new Item
        {
            Id = 0,
            Name = "Swamp Land Waders",
            Quality = ItemQuality.Legendary,
            Slug = "swamp-land-waders",
            ItemType = ItemType.Legs,
            BaseType = ItemBaseType.Armor,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/item/swamp-land-waders",
            Url = "https://us.battle.net/d3/en/item/swamp-land-waders",
            RelativeUrl = "/d3/en/item/swamp-land-waders",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Asheara's Gait 
        /// </summary>
        public static Item AshearasGait = new Item
        {
            Id = 0,
            Name = "Asheara's Gait",
            Quality = ItemQuality.Legendary,
            Slug = "ashearas-cradle",
            ItemType = ItemType.Legs,
            BaseType = ItemBaseType.Armor,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/ashearas-cradle",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/ashearas-cradle",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/ashearas-cradle",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "Asheara's Uniform",
        };

        /// <summary>
        /// Depth Diggers Primary skills that generate resource deal 80–100% additional damage.
        /// </summary>
        public static Item DepthDiggers = new Item
        {
            Id = 197216,
            Name = "Depth Diggers",
            Quality = ItemQuality.Legendary,
            Slug = "depth-diggers",
            ItemType = ItemType.Legs,
            BaseType = ItemBaseType.Armor,
            InternalName = "pants_norm_unique_044",
            DataUrl = "https://us.battle.net/api/d3/data/item/depth-diggers",
            Url = "https://us.battle.net/d3/en/item/depth-diggers",
            RelativeUrl = "/d3/en/item/depth-diggers",
            IsCrafted = false,
            LegendaryAffix = "Primary skills that generate resource deal 80–100% additional damage.",
            SetName = "",
        };

        /// <summary>
        /// Blackthorne's Jousting Mail 
        /// </summary>
        public static Item BlackthornesJoustingMail = new Item
        {
            Id = 222477,
            Name = "Blackthorne's Jousting Mail",
            Quality = ItemQuality.Legendary,
            Slug = "blackthornes-jousting-mail",
            ItemType = ItemType.Legs,
            BaseType = ItemBaseType.Armor,
            InternalName = "pants_norm_unique_050",
            DataUrl = "https://us.battle.net/api/d3/data/item/blackthornes-jousting-mail",
            Url = "https://us.battle.net/d3/en/item/blackthornes-jousting-mail",
            RelativeUrl = "/d3/en/item/blackthornes-jousting-mail",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Blackthorne's Battlegear",
        };

        /// <summary>
        /// Inna's Temperance 
        /// </summary>
        public static Item InnasTemperance = new Item
        {
            Id = 205646,
            Name = "Inna's Temperance",
            Quality = ItemQuality.Legendary,
            Slug = "innas-temperance",
            ItemType = ItemType.Legs,
            BaseType = ItemBaseType.Armor,
            InternalName = "pants_norm_unique_087",
            DataUrl = "https://us.battle.net/api/d3/data/item/innas-temperance",
            Url = "https://us.battle.net/d3/en/item/innas-temperance",
            RelativeUrl = "/d3/en/item/innas-temperance",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Inna's Mantra",
        };

        /// <summary>
        /// Gehennas 
        /// </summary>
        public static Item Gehennas = new Item
        {
            Id = 222476,
            Name = "Gehennas",
            Quality = ItemQuality.Legendary,
            Slug = "gehennas",
            ItemType = ItemType.Legs,
            BaseType = ItemBaseType.Armor,
            InternalName = "pants_norm_unique_049",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/gehennas",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/gehennas",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/gehennas",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Demon's Scale 
        /// </summary>
        public static Item DemonsScale = new Item
        {
            Id = 0,
            Name = "Demon's Scale",
            Quality = ItemQuality.Legendary,
            Slug = "demons-flesh",
            ItemType = ItemType.Legs,
            BaseType = ItemBaseType.Armor,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/demons-flesh",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/demons-flesh",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/demons-flesh",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "Demon's Skin",
        };

        /// <summary>
        /// Cuisses of Akkhan 
        /// </summary>
        public static Item CuissesOfAkkhan = new Item
        {
            Id = 358800,
            Name = "Cuisses of Akkhan",
            Quality = ItemQuality.Legendary,
            Slug = "cuisses-of-akkhan",
            ItemType = ItemType.Legs,
            BaseType = ItemBaseType.Armor,
            InternalName = "pants_norm_set_10",
            DataUrl = "https://us.battle.net/api/d3/data/item/cuisses-of-akkhan",
            Url = "https://us.battle.net/d3/en/item/cuisses-of-akkhan",
            RelativeUrl = "/d3/en/item/cuisses-of-akkhan",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Armor of Akkhan",
        };

        /// <summary>
        /// Firebird's Down 
        /// </summary>
        public static Item FirebirdsDown = new Item
        {
            Id = 358790,
            Name = "Firebird's Down",
            Quality = ItemQuality.Legendary,
            Slug = "firebirds-down",
            ItemType = ItemType.Legs,
            BaseType = ItemBaseType.Armor,
            InternalName = "pants_norm_set_06",
            DataUrl = "https://us.battle.net/api/d3/data/item/firebirds-down",
            Url = "https://us.battle.net/d3/en/item/firebirds-down",
            RelativeUrl = "/d3/en/item/firebirds-down",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Firebird's Finery",
        };

        /// <summary>
        /// Helltooth Leg Guards 
        /// </summary>
        public static Item HelltoothLegGuards = new Item
        {
            Id = 340522,
            Name = "Helltooth Leg Guards",
            Quality = ItemQuality.Legendary,
            Slug = "helltooth-leg-guards",
            ItemType = ItemType.Legs,
            BaseType = ItemBaseType.Armor,
            InternalName = "pants_norm_set_16",
            DataUrl = "https://us.battle.net/api/d3/data/item/helltooth-leg-guards",
            Url = "https://us.battle.net/d3/en/item/helltooth-leg-guards",
            RelativeUrl = "/d3/en/item/helltooth-leg-guards",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Helltooth Harness",
        };

        /// <summary>
        /// Jade Harvester's Courage 
        /// </summary>
        public static Item JadeHarvestersCourage = new Item
        {
            Id = 338041,
            Name = "Jade Harvester's Courage",
            Quality = ItemQuality.Legendary,
            Slug = "jade-harvesters-courage",
            ItemType = ItemType.Legs,
            BaseType = ItemBaseType.Armor,
            InternalName = "pants_norm_set_09",
            DataUrl = "https://us.battle.net/api/d3/data/item/jade-harvesters-courage",
            Url = "https://us.battle.net/d3/en/item/jade-harvesters-courage",
            RelativeUrl = "/d3/en/item/jade-harvesters-courage",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Raiment of the Jade Harvester",
        };

        /// <summary>
        /// Marauder's Encasement 
        /// </summary>
        public static Item MaraudersEncasement = new Item
        {
            Id = 336993,
            Name = "Marauder's Encasement",
            Quality = ItemQuality.Legendary,
            Slug = "marauders-encasement",
            ItemType = ItemType.Legs,
            BaseType = ItemBaseType.Armor,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/item/marauders-encasement",
            Url = "https://us.battle.net/d3/en/item/marauders-encasement",
            RelativeUrl = "/d3/en/item/marauders-encasement",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Embodiment of the Marauder",
        };

        /// <summary>
        /// Raekor's Breeches 
        /// </summary>
        public static Item RaekorsBreeches = new Item
        {
            Id = 336986,
            Name = "Raekor's Breeches",
            Quality = ItemQuality.Legendary,
            Slug = "raekors-breeches",
            ItemType = ItemType.Legs,
            BaseType = ItemBaseType.Armor,
            InternalName = "pants_norm_set_05",
            DataUrl = "https://us.battle.net/api/d3/data/item/raekors-breeches",
            Url = "https://us.battle.net/d3/en/item/raekors-breeches",
            RelativeUrl = "/d3/en/item/raekors-breeches",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "The Legacy of Raekor",
        };

        /// <summary>
        /// Roland's Determination 
        /// </summary>
        public static Item RolandsDetermination = new Item
        {
            Id = 404097,
            Name = "Roland's Determination",
            Quality = ItemQuality.Legendary,
            Slug = "rolands-determination",
            ItemType = ItemType.Legs,
            BaseType = ItemBaseType.Armor,
            InternalName = "p1_Pants_norm_set_01",
            DataUrl = "https://us.battle.net/api/d3/data/item/rolands-determination",
            Url = "https://us.battle.net/d3/en/item/rolands-determination",
            RelativeUrl = "/d3/en/item/rolands-determination",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Roland's Legacy",
        };

        /// <summary>
        /// Scales of the Dancing Serpent 
        /// </summary>
        public static Item ScalesOfTheDancingSerpent = new Item
        {
            Id = 338035,
            Name = "Scales of the Dancing Serpent",
            Quality = ItemQuality.Legendary,
            Slug = "scales-of-the-dancing-serpent",
            ItemType = ItemType.Legs,
            BaseType = ItemBaseType.Armor,
            InternalName = "pants_norm_set_08",
            DataUrl = "https://us.battle.net/api/d3/data/item/scales-of-the-dancing-serpent",
            Url = "https://us.battle.net/d3/en/item/scales-of-the-dancing-serpent",
            RelativeUrl = "/d3/en/item/scales-of-the-dancing-serpent",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Raiment of a Thousand Storms",
        };

        /// <summary>
        /// The Shadow's Coil 
        /// </summary>
        public static Item TheShadowsCoil = new Item
        {
            Id = 332361,
            Name = "The Shadow's Coil",
            Quality = ItemQuality.Legendary,
            Slug = "the-shadows-coil",
            ItemType = ItemType.Legs,
            BaseType = ItemBaseType.Armor,
            InternalName = "pants_norm_set_14",
            DataUrl = "https://us.battle.net/api/d3/data/item/the-shadows-coil",
            Url = "https://us.battle.net/d3/en/item/the-shadows-coil",
            RelativeUrl = "/d3/en/item/the-shadows-coil",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "The Shadow’s Mantle",
        };

        /// <summary>
        /// Vyr's Fantastic Finery 
        /// </summary>
        public static Item VyrsFantasticFinery = new Item
        {
            Id = 332360,
            Name = "Vyr's Fantastic Finery",
            Quality = ItemQuality.Legendary,
            Slug = "vyrs-fantastic-finery",
            ItemType = ItemType.Legs,
            BaseType = ItemBaseType.Armor,
            InternalName = "pants_norm_set_13",
            DataUrl = "https://us.battle.net/api/d3/data/item/vyrs-fantastic-finery",
            Url = "https://us.battle.net/d3/en/item/vyrs-fantastic-finery",
            RelativeUrl = "/d3/en/item/vyrs-fantastic-finery",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Vyr's Amazing Arcana",
        };

        /// <summary>
        /// Weight of the Earth 
        /// </summary>
        public static Item WeightOfTheEarth = new Item
        {
            Id = 340521,
            Name = "Weight of the Earth",
            Quality = ItemQuality.Legendary,
            Slug = "weight-of-the-earth",
            ItemType = ItemType.Legs,
            BaseType = ItemBaseType.Armor,
            InternalName = "pants_norm_set_15",
            DataUrl = "https://us.battle.net/api/d3/data/item/weight-of-the-earth",
            Url = "https://us.battle.net/d3/en/item/weight-of-the-earth",
            RelativeUrl = "/d3/en/item/weight-of-the-earth",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Might of the Earth",
        };

        /// <summary>
        /// Skelon's Deceit 
        /// </summary>
        public static Item SkelonsDeceit = new Item
        {
            Id = 222476,
            Name = "Skelon's Deceit",
            Quality = ItemQuality.Legendary,
            Slug = "skelons-deceit",
            ItemType = ItemType.Legs,
            BaseType = ItemBaseType.Armor,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/skelons-deceit",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/skelons-deceit",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/skelons-deceit",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Asheara's Pace 
        /// </summary>
        public static Item AshearasPace = new Item
        {
            Id = 209054,
            Name = "Asheara's Pace",
            Quality = ItemQuality.Legendary,
            Slug = "ashearas-pace",
            ItemType = ItemType.Legs,
            BaseType = ItemBaseType.Armor,
            InternalName = "pants_norm_unique_073",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/ashearas-pace",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/ashearas-pace",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/ashearas-pace",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "Asheara's Vestments",
        };

        /// <summary>
        /// Cain's Habit 
        /// </summary>
        public static Item CainsHabit = new Item
        {
            Id = 197218,
            Name = "Cain's Habit",
            Quality = ItemQuality.Legendary,
            Slug = "cains-habit",
            ItemType = ItemType.Legs,
            BaseType = ItemBaseType.Armor,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/cains-habit",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/cains-habit",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/cains-habit",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "Cain's Destiny",
        };

        /// <summary>
        /// Captain Crimson's Thrust 
        /// </summary>
        public static Item CaptainCrimsonsThrust = new Item
        {
            Id = 197214,
            Name = "Captain Crimson's Thrust",
            Quality = ItemQuality.Legendary,
            Slug = "captain-crimsons-thrust",
            ItemType = ItemType.Legs,
            BaseType = ItemBaseType.Armor,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/captain-crimsons-thrust",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/captain-crimsons-thrust",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/captain-crimsons-thrust",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "Captain Crimson's Trimmings",
        };

        /// <summary>
        /// Demon's Plate 
        /// </summary>
        public static Item DemonsPlate = new Item
        {
            Id = 205644,
            Name = "Demon's Plate",
            Quality = ItemQuality.Legendary,
            Slug = "demons-plate",
            ItemType = ItemType.Legs,
            BaseType = ItemBaseType.Armor,
            InternalName = "pants_norm_unique_085",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/demons-plate",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/demons-plate",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/demons-plate",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "Demon's Hide",
        };

        /// <summary>
        /// Hellfire Ring Strength Chance to launch an explosive ball of Hellfire when you attack.
        /// </summary>
        public static Item HellfireRingStrength = new Item
        {
            Id = 260327,
            Name = "Hellfire Ring Strength",
            Quality = ItemQuality.Legendary,
            Slug = "hellfire-ring-of-strength",
            ItemType = ItemType.Ring,
            BaseType = ItemBaseType.None,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/hellfire-ring-of-strength",
            Url = "https://us.battle.net/d3/en/artisan/jeweler/recipe/hellfire-ring-of-strength",
            RelativeUrl = "/d3/en/artisan/jeweler/recipe/hellfire-ring-of-strength",
            IsCrafted = true,
            LegendaryAffix = "Chance to launch an explosive ball of Hellfire when you attack.",
            SetName = "",
        };

        /// <summary>
        /// Hellfire Ring Intelligence Chance to launch an explosive ball of Hellfire when you attack.
        /// </summary>
        public static Item HellfireRingIntelligence = new Item
        {
            Id = 260327,
            Name = "Hellfire Ring Intelligence",
            Quality = ItemQuality.Legendary,
            Slug = "hellfire-ring-of-intelligence",
            ItemType = ItemType.Ring,
            BaseType = ItemBaseType.None,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/hellfire-ring-of-intelligence",
            Url = "https://us.battle.net/d3/en/artisan/jeweler/recipe/hellfire-ring-of-intelligence",
            RelativeUrl = "/d3/en/artisan/jeweler/recipe/hellfire-ring-of-intelligence",
            IsCrafted = true,
            LegendaryAffix = "Chance to launch an explosive ball of Hellfire when you attack.",
            SetName = "",
        };

        /// <summary>
        /// Hellfire Ring Vitality Chance to launch an explosive ball of Hellfire when you attack.
        /// </summary>
        public static Item HellfireRingVitality = new Item
        {
            Id = 260327,
            Name = "Hellfire Ring Vitality",
            Quality = ItemQuality.Legendary,
            Slug = "hellfire-ring-of-vitality",
            ItemType = ItemType.Ring,
            BaseType = ItemBaseType.None,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/hellfire-ring-of-vitality",
            Url = "https://us.battle.net/d3/en/artisan/jeweler/recipe/hellfire-ring-of-vitality",
            RelativeUrl = "/d3/en/artisan/jeweler/recipe/hellfire-ring-of-vitality",
            IsCrafted = true,
            LegendaryAffix = "Chance to launch an explosive ball of Hellfire when you attack.",
            SetName = "",
        };

        /// <summary>
        /// Hellfire Ring Dexterity Chance to launch an explosive ball of Hellfire when you attack.
        /// </summary>
        public static Item HellfireRingDexterity = new Item
        {
            Id = 260327,
            Name = "Hellfire Ring Dexterity",
            Quality = ItemQuality.Legendary,
            Slug = "hellfire-ring-of-dexterity",
            ItemType = ItemType.Ring,
            BaseType = ItemBaseType.None,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/hellfire-ring-of-dexterity",
            Url = "https://us.battle.net/d3/en/artisan/jeweler/recipe/hellfire-ring-of-dexterity",
            RelativeUrl = "/d3/en/artisan/jeweler/recipe/hellfire-ring-of-dexterity",
            IsCrafted = true,
            LegendaryAffix = "Chance to launch an explosive ball of Hellfire when you attack.",
            SetName = "",
        };

        /// <summary>
        /// Hellfire Ring Ring Chance on hit to engulf the ground in lava, dealing 200% weapon damage per second for 6 seconds.
        /// </summary>
        public static Item HellfireRingRing = new Item
        {
            Id = 260327,
            Name = "Hellfire Ring Ring",
            Quality = ItemQuality.Legendary,
            Slug = "hellfire-ring",
            ItemType = ItemType.Ring,
            BaseType = ItemBaseType.None,
            InternalName = "Ring_norm_unique_024",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/hellfire-ring",
            Url = "https://us.battle.net/d3/en/artisan/jeweler/recipe/hellfire-ring",
            RelativeUrl = "/d3/en/artisan/jeweler/recipe/hellfire-ring",
            IsCrafted = true,
            LegendaryAffix = "Chance on hit to engulf the ground in lava, dealing 200% weapon damage per second for 6 seconds.",
            SetName = "",
        };

        /// <summary>
        /// Manald Heal 
        /// </summary>
        public static Item ManaldHeal = new Item
        {
            Id = 212546,
            Name = "Manald Heal",
            Quality = ItemQuality.Legendary,
            Slug = "manald-heal",
            ItemType = ItemType.Ring,
            BaseType = ItemBaseType.None,
            InternalName = "Ring_norm_unique_021",
            DataUrl = "https://us.battle.net/api/d3/data/item/manald-heal",
            Url = "https://us.battle.net/d3/en/item/manald-heal",
            RelativeUrl = "/d3/en/item/manald-heal",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Nagelring 
        /// </summary>
        public static Item Nagelring = new Item
        {
            Id = 212586,
            Name = "Nagelring",
            Quality = ItemQuality.Legendary,
            Slug = "nagelring",
            ItemType = ItemType.Ring,
            BaseType = ItemBaseType.None,
            InternalName = "Ring_norm_unique_018",
            DataUrl = "https://us.battle.net/api/d3/data/item/nagelring",
            Url = "https://us.battle.net/d3/en/item/nagelring",
            RelativeUrl = "/d3/en/item/nagelring",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Stolen Ring 
        /// </summary>
        public static Item StolenRing = new Item
        {
            Id = 197839,
            Name = "Stolen Ring",
            Quality = ItemQuality.Legendary,
            Slug = "stolen-ring",
            ItemType = ItemType.Ring,
            BaseType = ItemBaseType.None,
            InternalName = "Ring_norm_unique_005",
            DataUrl = "https://us.battle.net/api/d3/data/item/stolen-ring",
            Url = "https://us.battle.net/d3/en/item/stolen-ring",
            RelativeUrl = "/d3/en/item/stolen-ring",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Leoric's Signet 
        /// </summary>
        public static Item LeoricsSignet = new Item
        {
            Id = 197835,
            Name = "Leoric's Signet",
            Quality = ItemQuality.Legendary,
            Slug = "leorics-signet",
            ItemType = ItemType.Ring,
            BaseType = ItemBaseType.None,
            InternalName = "Ring_norm_unique_002",
            DataUrl = "https://us.battle.net/api/d3/data/item/leorics-signet",
            Url = "https://us.battle.net/d3/en/item/leorics-signet",
            RelativeUrl = "/d3/en/item/leorics-signet",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Band of Untold Secrets 
        /// </summary>
        public static Item BandOfUntoldSecrets = new Item
        {
            Id = 212602,
            Name = "Band of Untold Secrets",
            Quality = ItemQuality.Legendary,
            Slug = "band-of-untold-secrets",
            ItemType = ItemType.Ring,
            BaseType = ItemBaseType.None,
            InternalName = "Ring_norm_unique_009",
            DataUrl = "https://us.battle.net/api/d3/data/item/band-of-untold-secrets",
            Url = "https://us.battle.net/d3/en/item/band-of-untold-secrets",
            RelativeUrl = "/d3/en/item/band-of-untold-secrets",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Broken Promises 
        /// </summary>
        public static Item BrokenPromises = new Item
        {
            Id = 212589,
            Name = "Broken Promises",
            Quality = ItemQuality.Legendary,
            Slug = "broken-promises",
            ItemType = ItemType.Ring,
            BaseType = ItemBaseType.None,
            InternalName = "Ring_norm_unique_006",
            DataUrl = "https://us.battle.net/api/d3/data/item/broken-promises",
            Url = "https://us.battle.net/d3/en/item/broken-promises",
            RelativeUrl = "/d3/en/item/broken-promises",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Puzzle Ring Summon a treasure goblin who picks up normal-quality items for you. After picking up 12–16 items, he drops a rare item with a chance for a legendary.
        /// </summary>
        public static Item PuzzleRing = new Item
        {
            Id = 197837,
            Name = "Puzzle Ring",
            Quality = ItemQuality.Legendary,
            Slug = "puzzle-ring",
            ItemType = ItemType.Ring,
            BaseType = ItemBaseType.None,
            InternalName = "Ring_norm_unique_004",
            DataUrl = "https://us.battle.net/api/d3/data/item/puzzle-ring",
            Url = "https://us.battle.net/d3/en/item/puzzle-ring",
            RelativeUrl = "/d3/en/item/puzzle-ring",
            IsCrafted = false,
            LegendaryAffix = "Summon a treasure goblin who picks up normal-quality items for you. After picking up 12–16 items, he drops a rare item with a chance for a legendary.",
            SetName = "",
        };

        /// <summary>
        /// Band of the Rue Chambers Your Spirit Generators generate 40–50% more Spirit.
        /// </summary>
        public static Item BandOfTheRueChambers = new Item
        {
            Id = 298093,
            Name = "Band of the Rue Chambers",
            Quality = ItemQuality.Legendary,
            Slug = "band-of-the-rue-chambers",
            ItemType = ItemType.Ring,
            BaseType = ItemBaseType.None,
            InternalName = "ring_norm_unique_030",
            DataUrl = "https://us.battle.net/api/d3/data/item/band-of-the-rue-chambers",
            Url = "https://us.battle.net/d3/en/item/band-of-the-rue-chambers",
            RelativeUrl = "/d3/en/item/band-of-the-rue-chambers",
            IsCrafted = false,
            LegendaryAffix = "Your Spirit Generators generate 40–50% more Spirit.",
            SetName = "",
        };

        /// <summary>
        /// Rechel's Ring of Larceny Gain 45–60% increased movement speed for 4 seconds after Fearing an enemy.
        /// </summary>
        public static Item RechelsRingOfLarceny = new Item
        {
            Id = 298091,
            Name = "Rechel's Ring of Larceny",
            Quality = ItemQuality.Legendary,
            Slug = "rechels-ring-of-larceny",
            ItemType = ItemType.Ring,
            BaseType = ItemBaseType.None,
            InternalName = "ring_norm_unique_028",
            DataUrl = "https://us.battle.net/api/d3/data/item/rechels-ring-of-larceny",
            Url = "https://us.battle.net/d3/en/item/rechels-ring-of-larceny",
            RelativeUrl = "/d3/en/item/rechels-ring-of-larceny",
            IsCrafted = false,
            LegendaryAffix = "Gain 45–60% increased movement speed for 4 seconds after Fearing an enemy.",
            SetName = "",
        };

        /// <summary>
        /// Rogar's Huge Stone Increase your Life per Second by up to 75–100% based on your missing Life.
        /// </summary>
        public static Item RogarsHugeStone = new Item
        {
            Id = 298090,
            Name = "Rogar's Huge Stone",
            Quality = ItemQuality.Legendary,
            Slug = "rogars-huge-stone",
            ItemType = ItemType.Ring,
            BaseType = ItemBaseType.None,
            InternalName = "ring_norm_unique_027",
            DataUrl = "https://us.battle.net/api/d3/data/item/rogars-huge-stone",
            Url = "https://us.battle.net/d3/en/item/rogars-huge-stone",
            RelativeUrl = "/d3/en/item/rogars-huge-stone",
            IsCrafted = false,
            LegendaryAffix = "Increase your Life per Second by up to 75–100% based on your missing Life.",
            SetName = "",
        };

        /// <summary>
        /// The Tall Man's Finger Zombie Dogs instead summons a single gargantuan dog with more damage and health than all other dogs combined.
        /// </summary>
        public static Item TheTallMansFinger = new Item
        {
            Id = 298088,
            Name = "The Tall Man's Finger",
            Quality = ItemQuality.Legendary,
            Slug = "the-tall-mans-finger",
            ItemType = ItemType.Ring,
            BaseType = ItemBaseType.None,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/item/the-tall-mans-finger",
            Url = "https://us.battle.net/d3/en/item/the-tall-mans-finger",
            RelativeUrl = "/d3/en/item/the-tall-mans-finger",
            IsCrafted = false,
            LegendaryAffix = "Zombie Dogs instead summons a single gargantuan dog with more damage and health than all other dogs combined.",
            SetName = "",
        };

        /// <summary>
        /// Wyrdward Lightning damage has a 13–17% chance to Stun for 1.5 seconds.
        /// </summary>
        public static Item Wyrdward = new Item
        {
            Id = 298089,
            Name = "Wyrdward",
            Quality = ItemQuality.Legendary,
            Slug = "wyrdward",
            ItemType = ItemType.Ring,
            BaseType = ItemBaseType.None,
            InternalName = "ring_norm_unique_026",
            DataUrl = "https://us.battle.net/api/d3/data/item/wyrdward",
            Url = "https://us.battle.net/d3/en/item/wyrdward",
            RelativeUrl = "/d3/en/item/wyrdward",
            IsCrafted = false,
            LegendaryAffix = "Lightning damage has a 13–17% chance to Stun for 1.5 seconds.",
            SetName = "",
        };

        /// <summary>
        /// Bul-Kathos's Wedding Band You drain life from enemies around you.
        /// </summary>
        public static Item BulkathossWeddingBand = new Item
        {
            Id = 212603,
            Name = "Bul-Kathos's Wedding Band",
            Quality = ItemQuality.Legendary,
            Slug = "bulkathoss-wedding-band",
            ItemType = ItemType.Ring,
            BaseType = ItemBaseType.None,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/item/bulkathoss-wedding-band",
            Url = "https://us.battle.net/d3/en/item/bulkathoss-wedding-band",
            RelativeUrl = "/d3/en/item/bulkathoss-wedding-band",
            IsCrafted = false,
            LegendaryAffix = "You drain life from enemies around you.",
            SetName = "",
        };

        /// <summary>
        /// Eternal Union Increases the duration of Phalanx avatars by 200%.
        /// </summary>
        public static Item EternalUnion = new Item
        {
            Id = 212601,
            Name = "Eternal Union",
            Quality = ItemQuality.Legendary,
            Slug = "eternal-union",
            ItemType = ItemType.Ring,
            BaseType = ItemBaseType.None,
            InternalName = "Ring_norm_unique_007",
            DataUrl = "https://us.battle.net/api/d3/data/item/eternal-union",
            Url = "https://us.battle.net/d3/en/item/eternal-union",
            RelativeUrl = "/d3/en/item/eternal-union",
            IsCrafted = false,
            LegendaryAffix = "Increases the duration of Phalanx avatars by 200%.",
            SetName = "",
        };

        /// <summary>
        /// Justice Lantern 
        /// </summary>
        public static Item JusticeLantern = new Item
        {
            Id = 212590,
            Name = "Justice Lantern",
            Quality = ItemQuality.Legendary,
            Slug = "justice-lantern",
            ItemType = ItemType.Ring,
            BaseType = ItemBaseType.None,
            InternalName = "ring_norm_unique_008",
            DataUrl = "https://us.battle.net/api/d3/data/item/justice-lantern",
            Url = "https://us.battle.net/d3/en/item/justice-lantern",
            RelativeUrl = "/d3/en/item/justice-lantern",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Obsidian Ring of the Zodiac 
        /// </summary>
        public static Item ObsidianRingOfTheZodiac = new Item
        {
            Id = 212588,
            Name = "Obsidian Ring of the Zodiac",
            Quality = ItemQuality.Legendary,
            Slug = "obsidian-ring-of-the-zodiac",
            ItemType = ItemType.Ring,
            BaseType = ItemBaseType.None,
            InternalName = "ring_norm_unique_023",
            DataUrl = "https://us.battle.net/api/d3/data/item/obsidian-ring-of-the-zodiac",
            Url = "https://us.battle.net/d3/en/item/obsidian-ring-of-the-zodiac",
            RelativeUrl = "/d3/en/item/obsidian-ring-of-the-zodiac",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Band of Hollow Whispers This ring occasionally haunts nearby enemies.
        /// </summary>
        public static Item BandOfHollowWhispers = new Item
        {
            Id = 197834,
            Name = "Band of Hollow Whispers",
            Quality = ItemQuality.Legendary,
            Slug = "band-of-hollow-whispers",
            ItemType = ItemType.Ring,
            BaseType = ItemBaseType.None,
            InternalName = "ring_norm_unique_001",
            DataUrl = "https://us.battle.net/api/d3/data/item/band-of-hollow-whispers",
            Url = "https://us.battle.net/d3/en/item/band-of-hollow-whispers",
            RelativeUrl = "/d3/en/item/band-of-hollow-whispers",
            IsCrafted = false,
            LegendaryAffix = "This ring occasionally haunts nearby enemies.",
            SetName = "",
        };

        /// <summary>
        /// Krede's Flame Taking Fire damage restores your primary resource.
        /// </summary>
        public static Item KredesFlame = new Item
        {
            Id = 197836,
            Name = "Krede's Flame",
            Quality = ItemQuality.Legendary,
            Slug = "kredes-flame",
            ItemType = ItemType.Ring,
            BaseType = ItemBaseType.None,
            InternalName = "Ring_norm_unique_003",
            DataUrl = "https://us.battle.net/api/d3/data/item/kredes-flame",
            Url = "https://us.battle.net/d3/en/item/kredes-flame",
            RelativeUrl = "/d3/en/item/kredes-flame",
            IsCrafted = false,
            LegendaryAffix = "Taking Fire damage restores your primary resource.",
            SetName = "",
        };

        /// <summary>
        /// Oculus Ring 
        /// </summary>
        public static Item OculusRing = new Item
        {
            Id = 212648,
            Name = "Oculus Ring",
            Quality = ItemQuality.Legendary,
            Slug = "oculus-ring",
            ItemType = ItemType.Ring,
            BaseType = ItemBaseType.None,
            InternalName = "Ring_norm_unique_017",
            DataUrl = "https://us.battle.net/api/d3/data/item/oculus-ring",
            Url = "https://us.battle.net/d3/en/item/oculus-ring",
            RelativeUrl = "/d3/en/item/oculus-ring",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Skull Grasp 
        /// </summary>
        public static Item SkullGrasp = new Item
        {
            Id = 212618,
            Name = "Skull Grasp",
            Quality = ItemQuality.Legendary,
            Slug = "skull-grasp",
            ItemType = ItemType.Ring,
            BaseType = ItemBaseType.None,
            InternalName = "Ring_norm_unique_022",
            DataUrl = "https://us.battle.net/api/d3/data/item/skull-grasp",
            Url = "https://us.battle.net/d3/en/item/skull-grasp",
            RelativeUrl = "/d3/en/item/skull-grasp",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Stone of Jordan 
        /// </summary>
        public static Item StoneOfJordan = new Item
        {
            Id = 212582,
            Name = "Stone of Jordan",
            Quality = ItemQuality.Legendary,
            Slug = "stone-of-jordan",
            ItemType = ItemType.Ring,
            BaseType = ItemBaseType.None,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/item/stone-of-jordan",
            Url = "https://us.battle.net/d3/en/item/stone-of-jordan",
            RelativeUrl = "/d3/en/item/stone-of-jordan",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Unity All damage taken is split between wearers of this item.
        /// </summary>
        public static Item Unity = new Item
        {
            Id = 212581,
            Name = "Unity",
            Quality = ItemQuality.Legendary,
            Slug = "unity",
            ItemType = ItemType.Ring,
            BaseType = ItemBaseType.None,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/item/unity",
            Url = "https://us.battle.net/d3/en/item/unity",
            RelativeUrl = "/d3/en/item/unity",
            IsCrafted = false,
            LegendaryAffix = "All damage taken is split between wearers of this item.",
            SetName = "",
        };

        /// <summary>
        /// Litany of the Undaunted 
        /// </summary>
        public static Item LitanyOfTheUndaunted = new Item
        {
            Id = 212651,
            Name = "Litany of the Undaunted",
            Quality = ItemQuality.Legendary,
            Slug = "litany-of-the-undaunted",
            ItemType = ItemType.Ring,
            BaseType = ItemBaseType.None,
            InternalName = "Ring_norm_unique_015",
            DataUrl = "https://us.battle.net/api/d3/data/item/litany-of-the-undaunted",
            Url = "https://us.battle.net/d3/en/item/litany-of-the-undaunted",
            RelativeUrl = "/d3/en/item/litany-of-the-undaunted",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Legacy of Nightmares",
        };

        /// <summary>
        /// Natalya's Reflection 
        /// </summary>
        public static Item NatalyasReflection = new Item
        {
            Id = 212545,
            Name = "Natalya's Reflection",
            Quality = ItemQuality.Legendary,
            Slug = "natalyas-reflection",
            ItemType = ItemType.Ring,
            BaseType = ItemBaseType.None,
            InternalName = "Ring_norm_unique_011",
            DataUrl = "https://us.battle.net/api/d3/data/item/natalyas-reflection",
            Url = "https://us.battle.net/d3/en/item/natalyas-reflection",
            RelativeUrl = "/d3/en/item/natalyas-reflection",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Natalya's Vengeance",
        };

        /// <summary>
        /// The Compass Rose 
        /// </summary>
        public static Item TheCompassRose = new Item
        {
            Id = 212587,
            Name = "The Compass Rose",
            Quality = ItemQuality.Legendary,
            Slug = "the-compass-rose",
            ItemType = ItemType.Ring,
            BaseType = ItemBaseType.None,
            InternalName = "Ring_norm_unique_013",
            DataUrl = "https://us.battle.net/api/d3/data/item/the-compass-rose",
            Url = "https://us.battle.net/d3/en/item/the-compass-rose",
            RelativeUrl = "/d3/en/item/the-compass-rose",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Endless Walk",
        };

        /// <summary>
        /// The Wailing Host 
        /// </summary>
        public static Item TheWailingHost = new Item
        {
            Id = 212650,
            Name = "The Wailing Host",
            Quality = ItemQuality.Legendary,
            Slug = "the-wailing-host",
            ItemType = ItemType.Ring,
            BaseType = ItemBaseType.None,
            InternalName = "ring_norm_unique_014",
            DataUrl = "https://us.battle.net/api/d3/data/item/the-wailing-host",
            Url = "https://us.battle.net/d3/en/item/the-wailing-host",
            RelativeUrl = "/d3/en/item/the-wailing-host",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Legacy of Nightmares",
        };

        /// <summary>
        /// Zunimassa's Pox 
        /// </summary>
        public static Item ZunimassasPox = new Item
        {
            Id = 212579,
            Name = "Zunimassa's Pox",
            Quality = ItemQuality.Legendary,
            Slug = "zunimassas-pox",
            ItemType = ItemType.Ring,
            BaseType = ItemBaseType.None,
            InternalName = "Ring_norm_unique_012",
            DataUrl = "https://us.battle.net/api/d3/data/item/zunimassas-pox",
            Url = "https://us.battle.net/d3/en/item/zunimassas-pox",
            RelativeUrl = "/d3/en/item/zunimassas-pox",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Zunimassa's Haunt",
        };

        /// <summary>
        /// Focus 
        /// </summary>
        public static Item Focus = new Item
        {
            Id = 332209,
            Name = "Focus",
            Quality = ItemQuality.Legendary,
            Slug = "focus",
            ItemType = ItemType.Ring,
            BaseType = ItemBaseType.None,
            InternalName = "ring_norm_set_001",
            DataUrl = "https://us.battle.net/api/d3/data/item/focus",
            Url = "https://us.battle.net/d3/en/item/focus",
            RelativeUrl = "/d3/en/item/focus",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Bastions of Will",
        };

        /// <summary>
        /// Restraint 
        /// </summary>
        public static Item Restraint = new Item
        {
            Id = 332210,
            Name = "Restraint",
            Quality = ItemQuality.Legendary,
            Slug = "restraint",
            ItemType = ItemType.Ring,
            BaseType = ItemBaseType.None,
            InternalName = "ring_norm_set_002",
            DataUrl = "https://us.battle.net/api/d3/data/item/restraint",
            Url = "https://us.battle.net/d3/en/item/restraint",
            RelativeUrl = "/d3/en/item/restraint",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Bastions of Will",
        };

        /// <summary>
        /// Lut Socks Leap can be cast again within 2 seconds before the cooldown begins.
        /// </summary>
        public static Item LutSocks = new Item
        {
            Id = 205622,
            Name = "Lut Socks",
            Quality = ItemQuality.Legendary,
            Slug = "lut-socks",
            ItemType = ItemType.Boots,
            BaseType = ItemBaseType.Armor,
            InternalName = "Boots_norm_unique_077",
            DataUrl = "https://us.battle.net/api/d3/data/item/lut-socks",
            Url = "https://us.battle.net/d3/en/item/lut-socks",
            RelativeUrl = "/d3/en/item/lut-socks",
            IsCrafted = false,
            LegendaryAffix = "Leap can be cast again within 2 seconds before the cooldown begins.",
            SetName = "",
        };

        /// <summary>
        /// The Crudest Boots Mystic Ally summons two Mystic Allies that fight by your side.
        /// </summary>
        public static Item TheCrudestBoots = new Item
        {
            Id = 205620,
            Name = "The Crudest Boots",
            Quality = ItemQuality.Legendary,
            Slug = "the-crudest-boots",
            ItemType = ItemType.Boots,
            BaseType = ItemBaseType.Armor,
            InternalName = "Boots_norm_unique_075",
            DataUrl = "https://us.battle.net/api/d3/data/item/the-crudest-boots",
            Url = "https://us.battle.net/d3/en/item/the-crudest-boots",
            RelativeUrl = "/d3/en/item/the-crudest-boots",
            IsCrafted = false,
            LegendaryAffix = "Mystic Ally summons two Mystic Allies that fight by your side.",
            SetName = "",
        };

        /// <summary>
        /// Cain's Sandals 
        /// </summary>
        public static Item CainsSandals = new Item
        {
            Id = 197225,
            Name = "Cain's Sandals",
            Quality = ItemQuality.Legendary,
            Slug = "cains-slippers",
            ItemType = ItemType.Boots,
            BaseType = ItemBaseType.Armor,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/cains-slippers",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/cains-slippers",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/cains-slippers",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "Cain's Fate",
        };

        /// <summary>
        /// Boj Anglers 
        /// </summary>
        public static Item BojAnglers = new Item
        {
            Id = 197224,
            Name = "Boj Anglers",
            Quality = ItemQuality.Legendary,
            Slug = "boj-anglers",
            ItemType = ItemType.Boots,
            BaseType = ItemBaseType.Armor,
            InternalName = "Boots_norm_unique_045",
            DataUrl = "https://us.battle.net/api/d3/data/item/boj-anglers",
            Url = "https://us.battle.net/d3/en/item/boj-anglers",
            RelativeUrl = "/d3/en/item/boj-anglers",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Captain Crimson's Whalers 
        /// </summary>
        public static Item CaptainCrimsonsWhalers = new Item
        {
            Id = 197221,
            Name = "Captain Crimson's Whalers",
            Quality = ItemQuality.Legendary,
            Slug = "captain-crimsons-deck-boots",
            ItemType = ItemType.Boots,
            BaseType = ItemBaseType.Armor,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/captain-crimsons-deck-boots",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/captain-crimsons-deck-boots",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/captain-crimsons-deck-boots",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "Captain Crimson's Finery",
        };

        /// <summary>
        /// Irontoe Mudsputters Gain up to 25% increased movement speed based on amount of Life missing.
        /// </summary>
        public static Item IrontoeMudsputters = new Item
        {
            Id = 339125,
            Name = "Irontoe Mudsputters",
            Quality = ItemQuality.Legendary,
            Slug = "irontoe-mudsputters",
            ItemType = ItemType.Boots,
            BaseType = ItemBaseType.Armor,
            InternalName = "boots_norm_unique_04",
            DataUrl = "https://us.battle.net/api/d3/data/item/irontoe-mudsputters",
            Url = "https://us.battle.net/d3/en/item/irontoe-mudsputters",
            RelativeUrl = "/d3/en/item/irontoe-mudsputters",
            IsCrafted = false,
            LegendaryAffix = "Gain up to 25% increased movement speed based on amount of Life missing.",
            SetName = "",
        };

        /// <summary>
        /// Fire Walkers Burn the ground you walk on, dealing 100% weapon damage each second.
        /// </summary>
        public static Item FireWalkers = new Item
        {
            Id = 205624,
            Name = "Fire Walkers",
            Quality = ItemQuality.Legendary,
            Slug = "fire-walkers",
            ItemType = ItemType.Boots,
            BaseType = ItemBaseType.Armor,
            InternalName = "Boots_norm_unique_085",
            DataUrl = "https://us.battle.net/api/d3/data/item/fire-walkers",
            Url = "https://us.battle.net/d3/en/item/fire-walkers",
            RelativeUrl = "/d3/en/item/fire-walkers",
            IsCrafted = false,
            LegendaryAffix = "Burn the ground you walk on, dealing 100% weapon damage each second.",
            SetName = "",
        };

        /// <summary>
        /// Lost Boys 
        /// </summary>
        public static Item LostBoys = new Item
        {
            Id = 205621,
            Name = "Lost Boys",
            Quality = ItemQuality.Legendary,
            Slug = "lost-boys",
            ItemType = ItemType.Boots,
            BaseType = ItemBaseType.Armor,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/lost-boys",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/lost-boys",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/lost-boys",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Asheara's Tracks 
        /// </summary>
        public static Item AshearasTracks = new Item
        {
            Id = 0,
            Name = "Asheara's Tracks",
            Quality = ItemQuality.Legendary,
            Slug = "ashearas-lock",
            ItemType = ItemType.Boots,
            BaseType = ItemBaseType.Armor,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/ashearas-lock",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/ashearas-lock",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/ashearas-lock",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "Asheara's Uniform",
        };

        /// <summary>
        /// Ice Climbers Gain immunity to Freeze and Immobilize effects.
        /// </summary>
        public static Item IceClimbers = new Item
        {
            Id = 222464,
            Name = "Ice Climbers",
            Quality = ItemQuality.Legendary,
            Slug = "ice-climbers",
            ItemType = ItemType.Boots,
            BaseType = ItemBaseType.Armor,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/item/ice-climbers",
            Url = "https://us.battle.net/d3/en/item/ice-climbers",
            RelativeUrl = "/d3/en/item/ice-climbers",
            IsCrafted = false,
            LegendaryAffix = "Gain immunity to Freeze and Immobilize effects.",
            SetName = "",
        };

        /// <summary>
        /// Blackthorne's Spurs 
        /// </summary>
        public static Item BlackthornesSpurs = new Item
        {
            Id = 222463,
            Name = "Blackthorne's Spurs",
            Quality = ItemQuality.Legendary,
            Slug = "blackthornes-spurs",
            ItemType = ItemType.Boots,
            BaseType = ItemBaseType.Armor,
            InternalName = "Boots_norm_unique_050",
            DataUrl = "https://us.battle.net/api/d3/data/item/blackthornes-spurs",
            Url = "https://us.battle.net/d3/en/item/blackthornes-spurs",
            RelativeUrl = "/d3/en/item/blackthornes-spurs",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Blackthorne's Battlegear",
        };

        /// <summary>
        /// Immortal King's Stride 
        /// </summary>
        public static Item ImmortalKingsStride = new Item
        {
            Id = 205625,
            Name = "Immortal King's Stride",
            Quality = ItemQuality.Legendary,
            Slug = "immortal-kings-stride",
            ItemType = ItemType.Boots,
            BaseType = ItemBaseType.Armor,
            InternalName = "Boots_norm_unique_086",
            DataUrl = "https://us.battle.net/api/d3/data/item/immortal-kings-stride",
            Url = "https://us.battle.net/d3/en/item/immortal-kings-stride",
            RelativeUrl = "/d3/en/item/immortal-kings-stride",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Immortal King's Call",
        };

        /// <summary>
        /// Natalya's Bloody Footprints 
        /// </summary>
        public static Item NatalyasBloodyFootprints = new Item
        {
            Id = 197223,
            Name = "Natalya's Bloody Footprints",
            Quality = ItemQuality.Legendary,
            Slug = "natalyas-bloody-footprints",
            ItemType = ItemType.Boots,
            BaseType = ItemBaseType.Armor,
            InternalName = "Boots_norm_unique_044",
            DataUrl = "https://us.battle.net/api/d3/data/item/natalyas-bloody-footprints",
            Url = "https://us.battle.net/d3/en/item/natalyas-bloody-footprints",
            RelativeUrl = "/d3/en/item/natalyas-bloody-footprints",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Natalya's Vengeance",
        };

        /// <summary>
        /// Zunimassa's Trail 
        /// </summary>
        public static Item ZunimassasTrail = new Item
        {
            Id = 205627,
            Name = "Zunimassa's Trail",
            Quality = ItemQuality.Legendary,
            Slug = "zunimassas-trail",
            ItemType = ItemType.Boots,
            BaseType = ItemBaseType.Armor,
            InternalName = "Boots_norm_unique_088",
            DataUrl = "https://us.battle.net/api/d3/data/item/zunimassas-trail",
            Url = "https://us.battle.net/d3/en/item/zunimassas-trail",
            RelativeUrl = "/d3/en/item/zunimassas-trail",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Zunimassa's Haunt",
        };

        /// <summary>
        /// Sage's Journey 
        /// </summary>
        public static Item SagesJourney = new Item
        {
            Id = 0,
            Name = "Sage's Journey",
            Quality = ItemQuality.Legendary,
            Slug = "sages-seekers",
            ItemType = ItemType.Boots,
            BaseType = ItemBaseType.Armor,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/sages-seekers",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/sages-seekers",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/sages-seekers",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "Sage's Plight",
        };

        /// <summary>
        /// Eight-Demon Boots 
        /// </summary>
        public static Item EightdemonBoots = new Item
        {
            Id = 338031,
            Name = "Eight-Demon Boots",
            Quality = ItemQuality.Legendary,
            Slug = "eightdemon-boots",
            ItemType = ItemType.Boots,
            BaseType = ItemBaseType.Armor,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/item/eightdemon-boots",
            Url = "https://us.battle.net/d3/en/item/eightdemon-boots",
            RelativeUrl = "/d3/en/item/eightdemon-boots",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Raiment of a Thousand Storms",
        };

        /// <summary>
        /// Firebird's Tarsi 
        /// </summary>
        public static Item FirebirdsTarsi = new Item
        {
            Id = 358793,
            Name = "Firebird's Tarsi",
            Quality = ItemQuality.Legendary,
            Slug = "firebirds-tarsi",
            ItemType = ItemType.Boots,
            BaseType = ItemBaseType.Armor,
            InternalName = "boots_norm_set_06",
            DataUrl = "https://us.battle.net/api/d3/data/item/firebirds-tarsi",
            Url = "https://us.battle.net/d3/en/item/firebirds-tarsi",
            RelativeUrl = "/d3/en/item/firebirds-tarsi",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Firebird's Finery",
        };

        /// <summary>
        /// Helltooth Greaves 
        /// </summary>
        public static Item HelltoothGreaves = new Item
        {
            Id = 340524,
            Name = "Helltooth Greaves",
            Quality = ItemQuality.Legendary,
            Slug = "helltooth-greaves",
            ItemType = ItemType.Boots,
            BaseType = ItemBaseType.Armor,
            InternalName = "boots_norm_set_16",
            DataUrl = "https://us.battle.net/api/d3/data/item/helltooth-greaves",
            Url = "https://us.battle.net/d3/en/item/helltooth-greaves",
            RelativeUrl = "/d3/en/item/helltooth-greaves",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Helltooth Harness",
        };

        /// <summary>
        /// Jade Harvester's Swiftness 
        /// </summary>
        public static Item JadeHarvestersSwiftness = new Item
        {
            Id = 338037,
            Name = "Jade Harvester's Swiftness",
            Quality = ItemQuality.Legendary,
            Slug = "jade-harvesters-swiftness",
            ItemType = ItemType.Boots,
            BaseType = ItemBaseType.Armor,
            InternalName = "boots_norm_set_09",
            DataUrl = "https://us.battle.net/api/d3/data/item/jade-harvesters-swiftness",
            Url = "https://us.battle.net/d3/en/item/jade-harvesters-swiftness",
            RelativeUrl = "/d3/en/item/jade-harvesters-swiftness",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Raiment of the Jade Harvester",
        };

        /// <summary>
        /// Marauder's Treads 
        /// </summary>
        public static Item MaraudersTreads = new Item
        {
            Id = 336995,
            Name = "Marauder's Treads",
            Quality = ItemQuality.Legendary,
            Slug = "marauders-treads",
            ItemType = ItemType.Boots,
            BaseType = ItemBaseType.Armor,
            InternalName = "boots_norm_set_07",
            DataUrl = "https://us.battle.net/api/d3/data/item/marauders-treads",
            Url = "https://us.battle.net/d3/en/item/marauders-treads",
            RelativeUrl = "/d3/en/item/marauders-treads",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Embodiment of the Marauder",
        };

        /// <summary>
        /// Raekor's Striders 
        /// </summary>
        public static Item RaekorsStriders = new Item
        {
            Id = 336987,
            Name = "Raekor's Striders",
            Quality = ItemQuality.Legendary,
            Slug = "raekors-striders",
            ItemType = ItemType.Boots,
            BaseType = ItemBaseType.Armor,
            InternalName = "boots_norm_set_05",
            DataUrl = "https://us.battle.net/api/d3/data/item/raekors-striders",
            Url = "https://us.battle.net/d3/en/item/raekors-striders",
            RelativeUrl = "/d3/en/item/raekors-striders",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "The Legacy of Raekor",
        };

        /// <summary>
        /// Roland's Stride 
        /// </summary>
        public static Item RolandsStride = new Item
        {
            Id = 404094,
            Name = "Roland's Stride",
            Quality = ItemQuality.Legendary,
            Slug = "rolands-stride",
            ItemType = ItemType.Boots,
            BaseType = ItemBaseType.Armor,
            InternalName = "p1_Boots_norm_set_01",
            DataUrl = "https://us.battle.net/api/d3/data/item/rolands-stride",
            Url = "https://us.battle.net/d3/en/item/rolands-stride",
            RelativeUrl = "/d3/en/item/rolands-stride",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Roland's Legacy",
        };

        /// <summary>
        /// Sabatons of Akkhan 
        /// </summary>
        public static Item SabatonsOfAkkhan = new Item
        {
            Id = 358795,
            Name = "Sabatons of Akkhan",
            Quality = ItemQuality.Legendary,
            Slug = "sabatons-of-akkhan",
            ItemType = ItemType.Boots,
            BaseType = ItemBaseType.Armor,
            InternalName = "boots_norm_set_10",
            DataUrl = "https://us.battle.net/api/d3/data/item/sabatons-of-akkhan",
            Url = "https://us.battle.net/d3/en/item/sabatons-of-akkhan",
            RelativeUrl = "/d3/en/item/sabatons-of-akkhan",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Armor of Akkhan",
        };

        /// <summary>
        /// The Shadow's Heels 
        /// </summary>
        public static Item TheShadowsHeels = new Item
        {
            Id = 332364,
            Name = "The Shadow's Heels",
            Quality = ItemQuality.Legendary,
            Slug = "the-shadows-heels",
            ItemType = ItemType.Boots,
            BaseType = ItemBaseType.Armor,
            InternalName = "boots_norm_set_14",
            DataUrl = "https://us.battle.net/api/d3/data/item/the-shadows-heels",
            Url = "https://us.battle.net/d3/en/item/the-shadows-heels",
            RelativeUrl = "/d3/en/item/the-shadows-heels",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "The Shadow’s Mantle",
        };

        /// <summary>
        /// Vyr's Swaggering Stance 
        /// </summary>
        public static Item VyrsSwaggeringStance = new Item
        {
            Id = 332363,
            Name = "Vyr's Swaggering Stance",
            Quality = ItemQuality.Legendary,
            Slug = "vyrs-swaggering-stance",
            ItemType = ItemType.Boots,
            BaseType = ItemBaseType.Armor,
            InternalName = "boots_norm_set_13",
            DataUrl = "https://us.battle.net/api/d3/data/item/vyrs-swaggering-stance",
            Url = "https://us.battle.net/d3/en/item/vyrs-swaggering-stance",
            RelativeUrl = "/d3/en/item/vyrs-swaggering-stance",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Vyr's Amazing Arcana",
        };

        /// <summary>
        /// Board Walkers 
        /// </summary>
        public static Item BoardWalkers = new Item
        {
            Id = 205621,
            Name = "Board Walkers",
            Quality = ItemQuality.Legendary,
            Slug = "board-walkers",
            ItemType = ItemType.Boots,
            BaseType = ItemBaseType.Armor,
            InternalName = "Boots_norm_unique_076",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/board-walkers",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/board-walkers",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/board-walkers",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Asheara's Finders 
        /// </summary>
        public static Item AshearasFinders = new Item
        {
            Id = 205618,
            Name = "Asheara's Finders",
            Quality = ItemQuality.Legendary,
            Slug = "ashearas-finders",
            ItemType = ItemType.Boots,
            BaseType = ItemBaseType.Armor,
            InternalName = "Boots_norm_unique_073",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/ashearas-finders",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/ashearas-finders",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/ashearas-finders",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "Asheara's Vestments",
        };

        /// <summary>
        /// Cain's Travelers 
        /// </summary>
        public static Item CainsTravelers = new Item
        {
            Id = 197225,
            Name = "Cain's Travelers",
            Quality = ItemQuality.Legendary,
            Slug = "cains-travelers",
            ItemType = ItemType.Boots,
            BaseType = ItemBaseType.Armor,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/cains-travelers",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/cains-travelers",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/cains-travelers",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "Cain's Destiny",
        };

        /// <summary>
        /// Captain Crimson's Waders 
        /// </summary>
        public static Item CaptainCrimsonsWaders = new Item
        {
            Id = 197221,
            Name = "Captain Crimson's Waders",
            Quality = ItemQuality.Legendary,
            Slug = "captain-crimsons-waders",
            ItemType = ItemType.Boots,
            BaseType = ItemBaseType.Armor,
            InternalName = "Boots_norm_unique_043",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/captain-crimsons-waders",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/captain-crimsons-waders",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/captain-crimsons-waders",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "Captain Crimson's Trimmings",
        };

        /// <summary>
        /// Sage's Passage 
        /// </summary>
        public static Item SagesPassage = new Item
        {
            Id = 0,
            Name = "Sage's Passage",
            Quality = ItemQuality.Legendary,
            Slug = "sages-passage",
            ItemType = ItemType.Boots,
            BaseType = ItemBaseType.Armor,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/sages-passage",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/sages-passage",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/sages-passage",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "Sage's Journey",
        };

        /// <summary>
        /// Akarat's Awakening Every successful block has a 20–25% chance to reduce all cooldowns by 1 second.
        /// </summary>
        public static Item AkaratsAwakening = new Item
        {
            Id = 299414,
            Name = "Akarat's Awakening",
            Quality = ItemQuality.Legendary,
            Slug = "akarats-awakening",
            ItemType = ItemType.Shield,
            BaseType = ItemBaseType.None,
            InternalName = "crushield_norm_unique_04",
            DataUrl = "https://us.battle.net/api/d3/data/item/akarats-awakening",
            Url = "https://us.battle.net/d3/en/item/akarats-awakening",
            RelativeUrl = "/d3/en/item/akarats-awakening",
            IsCrafted = false,
            LegendaryAffix = "Every successful block has a 20–25% chance to reduce all cooldowns by 1 second.",
            SetName = "",
        };

        /// <summary>
        /// Hallowed Bulwark Iron Skin also increases your Block Amount by 45–60%.
        /// </summary>
        public static Item HallowedBulwark = new Item
        {
            Id = 299413,
            Name = "Hallowed Bulwark",
            Quality = ItemQuality.Legendary,
            Slug = "hallowed-bulwark",
            ItemType = ItemType.Shield,
            BaseType = ItemBaseType.None,
            InternalName = "crushield_norm_unique_03",
            DataUrl = "https://us.battle.net/api/d3/data/item/hallowed-bulwark",
            Url = "https://us.battle.net/d3/en/item/hallowed-bulwark",
            RelativeUrl = "/d3/en/item/hallowed-bulwark",
            IsCrafted = false,
            LegendaryAffix = "Iron Skin also increases your Block Amount by 45–60%.",
            SetName = "",
        };

        /// <summary>
        /// Hellskull Gain 10% increased damage while wielding a two-handed weapon.
        /// </summary>
        public static Item Hellskull = new Item
        {
            Id = 299415,
            Name = "Hellskull",
            Quality = ItemQuality.Legendary,
            Slug = "hellskull",
            ItemType = ItemType.Shield,
            BaseType = ItemBaseType.None,
            InternalName = "crushield_norm_unique_05",
            DataUrl = "https://us.battle.net/api/d3/data/item/hellskull",
            Url = "https://us.battle.net/d3/en/item/hellskull",
            RelativeUrl = "/d3/en/item/hellskull",
            IsCrafted = false,
            LegendaryAffix = "Gain 10% increased damage while wielding a two-handed weapon.",
            SetName = "",
        };

        /// <summary>
        /// Jekangbord Blessed Shield ricochets to 4–6 additional enemies.
        /// </summary>
        public static Item Jekangbord = new Item
        {
            Id = 299412,
            Name = "Jekangbord",
            Quality = ItemQuality.Legendary,
            Slug = "jekangbord",
            ItemType = ItemType.Shield,
            BaseType = ItemBaseType.None,
            InternalName = "crushield_norm_unique_02",
            DataUrl = "https://us.battle.net/api/d3/data/item/jekangbord",
            Url = "https://us.battle.net/d3/en/item/jekangbord",
            RelativeUrl = "/d3/en/item/jekangbord",
            IsCrafted = false,
            LegendaryAffix = "Blessed Shield ricochets to 4–6 additional enemies.",
            SetName = "",
        };

        /// <summary>
        /// Sublime Conviction When you block, you have up to a 15–20% chance to Stun the attacker for 1.5 seconds based on your current Wrath.
        /// </summary>
        public static Item SublimeConviction = new Item
        {
            Id = 299416,
            Name = "Sublime Conviction",
            Quality = ItemQuality.Legendary,
            Slug = "sublime-conviction",
            ItemType = ItemType.Shield,
            BaseType = ItemBaseType.None,
            InternalName = "crushield_norm_unique_06",
            DataUrl = "https://us.battle.net/api/d3/data/item/sublime-conviction",
            Url = "https://us.battle.net/d3/en/item/sublime-conviction",
            RelativeUrl = "/d3/en/item/sublime-conviction",
            IsCrafted = false,
            LegendaryAffix = "When you block, you have up to a 15–20% chance to Stun the attacker for 1.5 seconds based on your current Wrath.",
            SetName = "",
        };

        /// <summary>
        /// The Final Witness Shield Glare now hits all enemies around you.
        /// </summary>
        public static Item TheFinalWitness = new Item
        {
            Id = 299417,
            Name = "The Final Witness",
            Quality = ItemQuality.Legendary,
            Slug = "the-final-witness",
            ItemType = ItemType.Shield,
            BaseType = ItemBaseType.None,
            InternalName = "crushield_norm_unique_07",
            DataUrl = "https://us.battle.net/api/d3/data/item/the-final-witness",
            Url = "https://us.battle.net/d3/en/item/the-final-witness",
            RelativeUrl = "/d3/en/item/the-final-witness",
            IsCrafted = false,
            LegendaryAffix = "Shield Glare now hits all enemies around you.",
            SetName = "",
        };

        /// <summary>
        /// Frydehr’s Wrath Condemn has no cooldown but instead costs 40 Wrath.
        /// </summary>
        public static Item FrydehrsWrath = new Item
        {
            Id = 405429,
            Name = "Frydehr’s Wrath",
            Quality = ItemQuality.Legendary,
            Slug = "frydehrs-wrath",
            ItemType = ItemType.Shield,
            BaseType = ItemBaseType.None,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/item/frydehrs-wrath",
            Url = "https://us.battle.net/d3/en/item/frydehrs-wrath",
            RelativeUrl = "/d3/en/item/frydehrs-wrath",
            IsCrafted = false,
            LegendaryAffix = "Condemn has no cooldown but instead costs 40 Wrath.",
            SetName = "",
        };

        /// <summary>
        /// Unrelenting Phalanx Phalanx now casts twice.
        /// </summary>
        public static Item UnrelentingPhalanx = new Item
        {
            Id = 405514,
            Name = "Unrelenting Phalanx",
            Quality = ItemQuality.Legendary,
            Slug = "unrelenting-phalanx",
            ItemType = ItemType.Shield,
            BaseType = ItemBaseType.None,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/item/unrelenting-phalanx",
            Url = "https://us.battle.net/d3/en/item/unrelenting-phalanx",
            RelativeUrl = "/d3/en/item/unrelenting-phalanx",
            IsCrafted = false,
            LegendaryAffix = "Phalanx now casts twice.",
            SetName = "",
        };

        /// <summary>
        /// Piro Marella Reduces the Wrath cost of Shield Bash by 40–50%.
        /// </summary>
        public static Item PiroMarella = new Item
        {
            Id = 299411,
            Name = "Piro Marella",
            Quality = ItemQuality.Legendary,
            Slug = "piro-marella",
            ItemType = ItemType.Shield,
            BaseType = ItemBaseType.None,
            InternalName = "crushield_norm_unique_01",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/piro-marella",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/piro-marella",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/piro-marella",
            IsCrafted = true,
            LegendaryAffix = "Reduces the Wrath cost of Shield Bash by 40–50%.",
            SetName = "",
        };

        /// <summary>
        /// Denial 
        /// </summary>
        public static Item Denial = new Item
        {
            Id = 152666,
            Name = "Denial",
            Quality = ItemQuality.Legendary,
            Slug = "denial",
            ItemType = ItemType.Shield,
            BaseType = ItemBaseType.None,
            InternalName = "Shield_norm_unique_03",
            DataUrl = "https://us.battle.net/api/d3/data/item/denial",
            Url = "https://us.battle.net/d3/en/item/denial",
            RelativeUrl = "/d3/en/item/denial",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Wall of Bone 20–30% chance to be protected by a shield of bones when you are hit.
        /// </summary>
        public static Item WallOfBone = new Item
        {
            Id = 152667,
            Name = "Wall of Bone",
            Quality = ItemQuality.Legendary,
            Slug = "wall-of-bone",
            ItemType = ItemType.Shield,
            BaseType = ItemBaseType.None,
            InternalName = "Shield_norm_unique_04",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/wall-of-bone",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/wall-of-bone",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/wall-of-bone",
            IsCrafted = true,
            LegendaryAffix = "20–30% chance to be protected by a shield of bones when you are hit.",
            SetName = "",
        };

        /// <summary>
        /// Defender of Westmarch Blocks have a chance of summoning a charging wolf that deals 300–400% weapon damage to all enemies it passes through.
        /// </summary>
        public static Item DefenderOfWestmarch = new Item
        {
            Id = 298182,
            Name = "Defender of Westmarch",
            Quality = ItemQuality.Legendary,
            Slug = "defender-of-westmarch",
            ItemType = ItemType.Shield,
            BaseType = ItemBaseType.None,
            InternalName = "shield_norm_unique_09",
            DataUrl = "https://us.battle.net/api/d3/data/item/defender-of-westmarch",
            Url = "https://us.battle.net/d3/en/item/defender-of-westmarch",
            RelativeUrl = "/d3/en/item/defender-of-westmarch",
            IsCrafted = false,
            LegendaryAffix = "Blocks have a chance of summoning a charging wolf that deals 300–400% weapon damage to all enemies it passes through.",
            SetName = "",
        };

        /// <summary>
        /// Eberli Charo Reduces the cooldown of Heaven's Fury by 45–50%.
        /// </summary>
        public static Item EberliCharo = new Item
        {
            Id = 298186,
            Name = "Eberli Charo",
            Quality = ItemQuality.Legendary,
            Slug = "eberli-charo",
            ItemType = ItemType.Shield,
            BaseType = ItemBaseType.None,
            InternalName = "shield_norm_unique_10",
            DataUrl = "https://us.battle.net/api/d3/data/item/eberli-charo",
            Url = "https://us.battle.net/d3/en/item/eberli-charo",
            RelativeUrl = "/d3/en/item/eberli-charo",
            IsCrafted = false,
            LegendaryAffix = "Reduces the cooldown of Heaven's Fury by 45–50%.",
            SetName = "",
        };

        /// <summary>
        /// Freeze of Deflection Blocking an attack has a chance to Freeze the attacker for 0.5–1.5 seconds.
        /// </summary>
        public static Item FreezeOfDeflection = new Item
        {
            Id = 61550,
            Name = "Freeze of Deflection",
            Quality = ItemQuality.Legendary,
            Slug = "freeze-of-deflection",
            ItemType = ItemType.Shield,
            BaseType = ItemBaseType.None,
            InternalName = "Shield_norm_unique_01",
            DataUrl = "https://us.battle.net/api/d3/data/item/freeze-of-deflection",
            Url = "https://us.battle.net/d3/en/item/freeze-of-deflection",
            RelativeUrl = "/d3/en/item/freeze-of-deflection",
            IsCrafted = false,
            LegendaryAffix = "Blocking an attack has a chance to Freeze the attacker for 0.5–1.5 seconds.",
            SetName = "",
        };

        /// <summary>
        /// Vo'Toyias Spiker Enemies affected by Provoke take double damage from Thorns.
        /// </summary>
        public static Item VotoyiasSpiker = new Item
        {
            Id = 298188,
            Name = "Vo'Toyias Spiker",
            Quality = ItemQuality.Legendary,
            Slug = "votoyias-spiker",
            ItemType = ItemType.Shield,
            BaseType = ItemBaseType.None,
            InternalName = "shield_norm_unique_12",
            DataUrl = "https://us.battle.net/api/d3/data/item/votoyias-spiker",
            Url = "https://us.battle.net/d3/en/item/votoyias-spiker",
            RelativeUrl = "/d3/en/item/votoyias-spiker",
            IsCrafted = false,
            LegendaryAffix = "Enemies affected by Provoke take double damage from Thorns.",
            SetName = "",
        };

        /// <summary>
        /// Lidless Wall 
        /// </summary>
        public static Item LidlessWall = new Item
        {
            Id = 195389,
            Name = "Lidless Wall",
            Quality = ItemQuality.Legendary,
            Slug = "lidless-wall",
            ItemType = ItemType.Shield,
            BaseType = ItemBaseType.None,
            InternalName = "Shield_norm_unique_07",
            DataUrl = "https://us.battle.net/api/d3/data/item/lidless-wall",
            Url = "https://us.battle.net/d3/en/item/lidless-wall",
            RelativeUrl = "/d3/en/item/lidless-wall",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Ivory Tower 
        /// </summary>
        public static Item IvoryTower = new Item
        {
            Id = 197478,
            Name = "Ivory Tower",
            Quality = ItemQuality.Legendary,
            Slug = "ivory-tower",
            ItemType = ItemType.Shield,
            BaseType = ItemBaseType.None,
            InternalName = "Shield_norm_unique_08",
            DataUrl = "https://us.battle.net/api/d3/data/item/ivory-tower",
            Url = "https://us.battle.net/d3/en/item/ivory-tower",
            RelativeUrl = "/d3/en/item/ivory-tower",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Stormshield 
        /// </summary>
        public static Item Stormshield = new Item
        {
            Id = 192484,
            Name = "Stormshield",
            Quality = ItemQuality.Legendary,
            Slug = "stormshield",
            ItemType = ItemType.Shield,
            BaseType = ItemBaseType.None,
            InternalName = "Shield_norm_unique_06",
            DataUrl = "https://us.battle.net/api/d3/data/item/stormshield",
            Url = "https://us.battle.net/d3/en/item/stormshield",
            RelativeUrl = "/d3/en/item/stormshield",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Hallowed Defender 
        /// </summary>
        public static Item HallowedDefender = new Item
        {
            Id = 0,
            Name = "Hallowed Defender",
            Quality = ItemQuality.Legendary,
            Slug = "hallowed-barrier",
            ItemType = ItemType.Shield,
            BaseType = ItemBaseType.None,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/hallowed-barrier",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/hallowed-barrier",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/hallowed-barrier",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "Hallowed Defenders",
        };

        /// <summary>
        /// Wall of Man 20–30% chance to be protected by a shield of bones when you are hit.
        /// </summary>
        public static Item WallOfMan = new Item
        {
            Id = 152667,
            Name = "Wall of Man",
            Quality = ItemQuality.Legendary,
            Slug = "wall-of-man",
            ItemType = ItemType.Shield,
            BaseType = ItemBaseType.None,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/wall-of-man",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/wall-of-man",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/wall-of-man",
            IsCrafted = true,
            LegendaryAffix = "20–30% chance to be protected by a shield of bones when you are hit.",
            SetName = "",
        };

        /// <summary>
        /// Hallowed Barricade 
        /// </summary>
        public static Item HallowedBarricade = new Item
        {
            Id = 223758,
            Name = "Hallowed Barricade",
            Quality = ItemQuality.Legendary,
            Slug = "hallowed-barricade",
            ItemType = ItemType.Shield,
            BaseType = ItemBaseType.None,
            InternalName = "Shield_norm_set_01",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/hallowed-barricade",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/hallowed-barricade",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/hallowed-barricade",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "Hallowed Protectors",
        };

        /// <summary>
        /// Winter Flurry Enemies killed by Cold damage have a 15–20% chance to release a Frost Nova.
        /// </summary>
        public static Item WinterFlurry = new Item
        {
            Id = 184199,
            Name = "Winter Flurry",
            Quality = ItemQuality.Legendary,
            Slug = "winter-flurry",
            ItemType = ItemType.Orb,
            BaseType = ItemBaseType.None,
            InternalName = "orb_norm_unique_01",
            DataUrl = "https://us.battle.net/api/d3/data/item/winter-flurry",
            Url = "https://us.battle.net/d3/en/item/winter-flurry",
            RelativeUrl = "/d3/en/item/winter-flurry",
            IsCrafted = false,
            LegendaryAffix = "Enemies killed by Cold damage have a 15–20% chance to release a Frost Nova.",
            SetName = "",
        };

        /// <summary>
        /// Light of Grace Ray of Frost now pierces.
        /// </summary>
        public static Item LightOfGrace = new Item
        {
            Id = 272038,
            Name = "Light of Grace",
            Quality = ItemQuality.Legendary,
            Slug = "light-of-grace",
            ItemType = ItemType.Orb,
            BaseType = ItemBaseType.None,
            InternalName = "orb_norm_unique_09",
            DataUrl = "https://us.battle.net/api/d3/data/item/light-of-grace",
            Url = "https://us.battle.net/d3/en/item/light-of-grace",
            RelativeUrl = "/d3/en/item/light-of-grace",
            IsCrafted = false,
            LegendaryAffix = "Ray of Frost now pierces.",
            SetName = "",
        };

        /// <summary>
        /// Mirrorball Magic Missile fires 1–2 extra missiles.
        /// </summary>
        public static Item Mirrorball = new Item
        {
            Id = 272022,
            Name = "Mirrorball",
            Quality = ItemQuality.Legendary,
            Slug = "mirrorball",
            ItemType = ItemType.Orb,
            BaseType = ItemBaseType.None,
            InternalName = "orb_norm_unique_07",
            DataUrl = "https://us.battle.net/api/d3/data/item/mirrorball",
            Url = "https://us.battle.net/d3/en/item/mirrorball",
            RelativeUrl = "/d3/en/item/mirrorball",
            IsCrafted = false,
            LegendaryAffix = "Magic Missile fires 1–2 extra missiles.",
            SetName = "",
        };

        /// <summary>
        /// Myken's Ball of Hate Electrocute can chain to enemies that have already been hit.
        /// </summary>
        public static Item MykensBallOfHate = new Item
        {
            Id = 272037,
            Name = "Myken's Ball of Hate",
            Quality = ItemQuality.Legendary,
            Slug = "mykens-ball-of-hate",
            ItemType = ItemType.Orb,
            BaseType = ItemBaseType.None,
            InternalName = "orb_norm_unique_08",
            DataUrl = "https://us.battle.net/api/d3/data/item/mykens-ball-of-hate",
            Url = "https://us.battle.net/d3/en/item/mykens-ball-of-hate",
            RelativeUrl = "/d3/en/item/mykens-ball-of-hate",
            IsCrafted = false,
            LegendaryAffix = "Electrocute can chain to enemies that have already been hit.",
            SetName = "",
        };

        /// <summary>
        /// Singularity 
        /// </summary>
        public static Item Singularity = new Item
        {
            Id = 0,
            Name = "Singularity",
            Quality = ItemQuality.Legendary,
            Slug = "singularity",
            ItemType = ItemType.Orb,
            BaseType = ItemBaseType.None,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/singularity",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/singularity",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/singularity",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// The Oculus Taking damage has up to a 15–20% chance to reset the cooldown on Teleport.
        /// </summary>
        public static Item TheOculus = new Item
        {
            Id = 192320,
            Name = "The Oculus",
            Quality = ItemQuality.Legendary,
            Slug = "the-oculus",
            ItemType = ItemType.Orb,
            BaseType = ItemBaseType.None,
            InternalName = "orb_norm_unique_02",
            DataUrl = "https://us.battle.net/api/d3/data/item/the-oculus",
            Url = "https://us.battle.net/d3/en/item/the-oculus",
            RelativeUrl = "/d3/en/item/the-oculus",
            IsCrafted = false,
            LegendaryAffix = "Taking damage has up to a 15–20% chance to reset the cooldown on Teleport.",
            SetName = "",
        };

        /// <summary>
        /// Triumvirate 
        /// </summary>
        public static Item Triumvirate = new Item
        {
            Id = 195325,
            Name = "Triumvirate",
            Quality = ItemQuality.Legendary,
            Slug = "triumvirate",
            ItemType = ItemType.Orb,
            BaseType = ItemBaseType.None,
            InternalName = "orb_norm_unique_04",
            DataUrl = "https://us.battle.net/api/d3/data/item/triumvirate",
            Url = "https://us.battle.net/d3/en/item/triumvirate",
            RelativeUrl = "/d3/en/item/triumvirate",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Chantodo's Force 
        /// </summary>
        public static Item ChantodosForce = new Item
        {
            Id = 212277,
            Name = "Chantodo's Force",
            Quality = ItemQuality.Legendary,
            Slug = "chantodos-force",
            ItemType = ItemType.Orb,
            BaseType = ItemBaseType.None,
            InternalName = "orb_norm_unique_05",
            DataUrl = "https://us.battle.net/api/d3/data/item/chantodos-force",
            Url = "https://us.battle.net/d3/en/item/chantodos-force",
            RelativeUrl = "/d3/en/item/chantodos-force",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Chantodo's Resolve",
        };

        /// <summary>
        /// Tal Rasha's Unwavering Glare 
        /// </summary>
        public static Item TalRashasUnwaveringGlare = new Item
        {
            Id = 212780,
            Name = "Tal Rasha's Unwavering Glare",
            Quality = ItemQuality.Legendary,
            Slug = "tal-rashas-unwavering-glare",
            ItemType = ItemType.Orb,
            BaseType = ItemBaseType.None,
            InternalName = "orb_norm_unique_06",
            DataUrl = "https://us.battle.net/api/d3/data/item/tal-rashas-unwavering-glare",
            Url = "https://us.battle.net/d3/en/item/tal-rashas-unwavering-glare",
            RelativeUrl = "/d3/en/item/tal-rashas-unwavering-glare",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Tal Rasha's Elements",
        };

        /// <summary>
        /// Firebird's Eye 
        /// </summary>
        public static Item FirebirdsEye = new Item
        {
            Id = 358819,
            Name = "Firebird's Eye",
            Quality = ItemQuality.Legendary,
            Slug = "firebirds-eye",
            ItemType = ItemType.Orb,
            BaseType = ItemBaseType.None,
            InternalName = "orb_norm_set_06",
            DataUrl = "https://us.battle.net/api/d3/data/item/firebirds-eye",
            Url = "https://us.battle.net/d3/en/item/firebirds-eye",
            RelativeUrl = "/d3/en/item/firebirds-eye",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Firebird's Finery",
        };

        /// <summary>
        /// Cosmic Strand Teleport gains the effect of the Wormhole rune.
        /// </summary>
        public static Item CosmicStrand = new Item
        {
            Id = 195127,
            Name = "Cosmic Strand",
            Quality = ItemQuality.Legendary,
            Slug = "cosmic-strand",
            ItemType = ItemType.Orb,
            BaseType = ItemBaseType.None,
            InternalName = "orb_norm_unique_03",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/cosmic-strand",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/cosmic-strand",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/cosmic-strand",
            IsCrafted = true,
            LegendaryAffix = "Teleport gains the effect of the Wormhole rune.",
            SetName = "",
        };

        /// <summary>
        /// Fletcher's Pride 
        /// </summary>
        public static Item FletchersPride = new Item
        {
            Id = 197629,
            Name = "Fletcher's Pride",
            Quality = ItemQuality.Legendary,
            Slug = "fletchers-pride",
            ItemType = ItemType.Quiver,
            BaseType = ItemBaseType.None,
            InternalName = "Quiver_norm_unique_06",
            DataUrl = "https://us.battle.net/api/d3/data/item/fletchers-pride",
            Url = "https://us.battle.net/d3/en/item/fletchers-pride",
            RelativeUrl = "/d3/en/item/fletchers-pride",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Sin Seekers 
        /// </summary>
        public static Item SinSeekers = new Item
        {
            Id = 197625,
            Name = "Sin Seekers",
            Quality = ItemQuality.Legendary,
            Slug = "sin-seekers",
            ItemType = ItemType.Quiver,
            BaseType = ItemBaseType.None,
            InternalName = "Quiver_norm_unique_02",
            DataUrl = "https://us.battle.net/api/d3/data/item/sin-seekers",
            Url = "https://us.battle.net/d3/en/item/sin-seekers",
            RelativeUrl = "/d3/en/item/sin-seekers",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Holy Point Shot 
        /// </summary>
        public static Item HolyPointShot = new Item
        {
            Id = 197627,
            Name = "Holy Point Shot",
            Quality = ItemQuality.Legendary,
            Slug = "holy-point-shot",
            ItemType = ItemType.Quiver,
            BaseType = ItemBaseType.None,
            InternalName = "Quiver_norm_unique_04",
            DataUrl = "https://us.battle.net/api/d3/data/item/holy-point-shot",
            Url = "https://us.battle.net/d3/en/item/holy-point-shot",
            RelativeUrl = "/d3/en/item/holy-point-shot",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Spines of Seething Hatred Chakram now generates 3–4 Hatred.
        /// </summary>
        public static Item SpinesOfSeethingHatred = new Item
        {
            Id = 197628,
            Name = "Spines of Seething Hatred",
            Quality = ItemQuality.Legendary,
            Slug = "spines-of-seething-hatred",
            ItemType = ItemType.Quiver,
            BaseType = ItemBaseType.None,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/item/spines-of-seething-hatred",
            Url = "https://us.battle.net/d3/en/item/spines-of-seething-hatred",
            RelativeUrl = "/d3/en/item/spines-of-seething-hatred",
            IsCrafted = false,
            LegendaryAffix = "Chakram now generates 3–4 Hatred.",
            SetName = "",
        };

        /// <summary>
        /// Bombadier's Rucksack You may have 2 additional Sentries.
        /// </summary>
        public static Item BombadiersRucksack = new Item
        {
            Id = 298171,
            Name = "Bombadier's Rucksack",
            Quality = ItemQuality.Legendary,
            Slug = "bombadiers-rucksack",
            ItemType = ItemType.Quiver,
            BaseType = ItemBaseType.None,
            InternalName = "quiver_norm_unique_09",
            DataUrl = "https://us.battle.net/api/d3/data/item/bombadiers-rucksack",
            Url = "https://us.battle.net/d3/en/item/bombadiers-rucksack",
            RelativeUrl = "/d3/en/item/bombadiers-rucksack",
            IsCrafted = false,
            LegendaryAffix = "You may have 2 additional Sentries.",
            SetName = "",
        };

        /// <summary>
        /// Emimei's Duffel Bolas now explode instantly.
        /// </summary>
        public static Item EmimeisDuffel = new Item
        {
            Id = 298172,
            Name = "Emimei's Duffel",
            Quality = ItemQuality.Legendary,
            Slug = "emimeis-duffel",
            ItemType = ItemType.Quiver,
            BaseType = ItemBaseType.None,
            InternalName = "quiver_norm_unique_10",
            DataUrl = "https://us.battle.net/api/d3/data/item/emimeis-duffel",
            Url = "https://us.battle.net/d3/en/item/emimeis-duffel",
            RelativeUrl = "/d3/en/item/emimeis-duffel",
            IsCrafted = false,
            LegendaryAffix = "Bolas now explode instantly.",
            SetName = "",
        };

        /// <summary>
        /// The Ninth Cirri Satchel Hungering Arrow has 20% additional chance to pierce.
        /// </summary>
        public static Item TheNinthCirriSatchel = new Item
        {
            Id = 298170,
            Name = "The Ninth Cirri Satchel",
            Quality = ItemQuality.Legendary,
            Slug = "the-ninth-cirri-satchel",
            ItemType = ItemType.Quiver,
            BaseType = ItemBaseType.None,
            InternalName = "quiver_norm_unique_08",
            DataUrl = "https://us.battle.net/api/d3/data/item/the-ninth-cirri-satchel",
            Url = "https://us.battle.net/d3/en/item/the-ninth-cirri-satchel",
            RelativeUrl = "/d3/en/item/the-ninth-cirri-satchel",
            IsCrafted = false,
            LegendaryAffix = "Hungering Arrow has 20% additional chance to pierce.",
            SetName = "",
        };

        /// <summary>
        /// Meticulous Bolts Elemental Arrow - Ball Lightning now travels at 30–40% speed.
        /// </summary>
        public static Item MeticulousBolts = new Item
        {
            Id = 197624,
            Name = "Meticulous Bolts",
            Quality = ItemQuality.Legendary,
            Slug = "meticulous-bolts",
            ItemType = ItemType.Quiver,
            BaseType = ItemBaseType.None,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/item/meticulous-bolts",
            Url = "https://us.battle.net/d3/en/item/meticulous-bolts",
            RelativeUrl = "/d3/en/item/meticulous-bolts",
            IsCrafted = false,
            LegendaryAffix = "Elemental Arrow - Ball Lightning now travels at 30–40% speed.",
            SetName = "",
        };

        /// <summary>
        /// Dead Man's Legacy 
        /// </summary>
        public static Item DeadMansLegacy = new Item
        {
            Id = 197630,
            Name = "Dead Man's Legacy",
            Quality = ItemQuality.Legendary,
            Slug = "dead-mans-legacy",
            ItemType = ItemType.Quiver,
            BaseType = ItemBaseType.None,
            InternalName = "quiver_norm_unique_07",
            DataUrl = "https://us.battle.net/api/d3/data/item/dead-mans-legacy",
            Url = "https://us.battle.net/d3/en/item/dead-mans-legacy",
            RelativeUrl = "/d3/en/item/dead-mans-legacy",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Black Bone Arrows 
        /// </summary>
        public static Item BlackBoneArrows = new Item
        {
            Id = 0,
            Name = "Black Bone Arrows",
            Quality = ItemQuality.Legendary,
            Slug = "black-bone-arrows",
            ItemType = ItemType.Quiver,
            BaseType = ItemBaseType.None,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/black-bone-arrows",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/black-bone-arrows",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/black-bone-arrows",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Archfiend Arrows 
        /// </summary>
        public static Item ArchfiendArrows = new Item
        {
            Id = 197626,
            Name = "Archfiend Arrows",
            Quality = ItemQuality.Legendary,
            Slug = "archfiend-arrows",
            ItemType = ItemType.Quiver,
            BaseType = ItemBaseType.None,
            InternalName = "Quiver_norm_unique_03",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/archfiend-arrows",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/archfiend-arrows",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/archfiend-arrows",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Genzaniku Chance to summon a ghostly Fallen Champion when attacking.
        /// </summary>
        public static Item Genzaniku = new Item
        {
            Id = 116386,
            Name = "Genzaniku",
            Quality = ItemQuality.Legendary,
            Slug = "genzaniku",
            ItemType = ItemType.Axe,
            BaseType = ItemBaseType.Weapon,
            InternalName = "Axe_norm_unique_01",
            DataUrl = "https://us.battle.net/api/d3/data/item/genzaniku",
            Url = "https://us.battle.net/d3/en/item/genzaniku",
            RelativeUrl = "/d3/en/item/genzaniku",
            IsCrafted = false,
            LegendaryAffix = "Chance to summon a ghostly Fallen Champion when attacking.",
            SetName = "",
        };

        /// <summary>
        /// Flesh Tearer 
        /// </summary>
        public static Item FleshTearer = new Item
        {
            Id = 358801,
            Name = "Flesh Tearer",
            Quality = ItemQuality.Legendary,
            Slug = "flesh-tearer",
            ItemType = ItemType.Axe,
            BaseType = ItemBaseType.Weapon,
            InternalName = "shoulderpads_norm_set_10",
            DataUrl = "https://us.battle.net/api/d3/data/item/flesh-tearer",
            Url = "https://us.battle.net/d3/en/item/flesh-tearer",
            RelativeUrl = "/d3/en/item/flesh-tearer",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Hack 75–100% of your Thorns damage is applied on every attack.
        /// </summary>
        public static Item Hack = new Item
        {
            Id = 271598,
            Name = "Hack",
            Quality = ItemQuality.Legendary,
            Slug = "hack",
            ItemType = ItemType.Axe,
            BaseType = ItemBaseType.Weapon,
            InternalName = "axe_norm_unique_09",
            DataUrl = "https://us.battle.net/api/d3/data/item/hack",
            Url = "https://us.battle.net/d3/en/item/hack",
            RelativeUrl = "/d3/en/item/hack",
            IsCrafted = false,
            LegendaryAffix = "75–100% of your Thorns damage is applied on every attack.",
            SetName = "",
        };

        /// <summary>
        /// The Wedge 
        /// </summary>
        public static Item TheWedge = new Item
        {
            Id = 0,
            Name = "The Wedge",
            Quality = ItemQuality.Legendary,
            Slug = "the-wedge",
            ItemType = ItemType.Axe,
            BaseType = ItemBaseType.Weapon,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/the-wedge",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/the-wedge",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/the-wedge",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// The Butcher's Sickle 20–25% chance to drag enemies to you when attacking.
        /// </summary>
        public static Item TheButchersSickle = new Item
        {
            Id = 189973,
            Name = "The Butcher's Sickle",
            Quality = ItemQuality.Legendary,
            Slug = "the-butchers-sickle",
            ItemType = ItemType.Axe,
            BaseType = ItemBaseType.Weapon,
            InternalName = "Axe_norm_unique_06",
            DataUrl = "https://us.battle.net/api/d3/data/item/the-butchers-sickle",
            Url = "https://us.battle.net/d3/en/item/the-butchers-sickle",
            RelativeUrl = "/d3/en/item/the-butchers-sickle",
            IsCrafted = false,
            LegendaryAffix = "20–25% chance to drag enemies to you when attacking.",
            SetName = "",
        };

        /// <summary>
        /// The Burning Axe of Sankis Chance to fight through the pain when enemies hit you.
        /// </summary>
        public static Item TheBurningAxeOfSankis = new Item
        {
            Id = 181484,
            Name = "The Burning Axe of Sankis",
            Quality = ItemQuality.Legendary,
            Slug = "the-burning-axe-of-sankis",
            ItemType = ItemType.Axe,
            BaseType = ItemBaseType.Weapon,
            InternalName = "Axe_norm_unique_05",
            DataUrl = "https://us.battle.net/api/d3/data/item/the-burning-axe-of-sankis",
            Url = "https://us.battle.net/d3/en/item/the-burning-axe-of-sankis",
            RelativeUrl = "/d3/en/item/the-burning-axe-of-sankis",
            IsCrafted = false,
            LegendaryAffix = "Chance to fight through the pain when enemies hit you.",
            SetName = "",
        };

        /// <summary>
        /// Hallowed Storm 
        /// </summary>
        public static Item HallowedStorm = new Item
        {
            Id = 223461,
            Name = "Hallowed Storm",
            Quality = ItemQuality.Legendary,
            Slug = "hallowed-divide",
            ItemType = ItemType.Axe,
            BaseType = ItemBaseType.Weapon,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/hallowed-divide",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/hallowed-divide",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/hallowed-divide",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "Hallowed Defenders",
        };

        /// <summary>
        /// Utar's Roar 
        /// </summary>
        public static Item UtarsRoar = new Item
        {
            Id = 0,
            Name = "Utar's Roar",
            Quality = ItemQuality.Legendary,
            Slug = "utars-roar",
            ItemType = ItemType.Axe,
            BaseType = ItemBaseType.Weapon,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/utars-roar",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/utars-roar",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/utars-roar",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Hallowed Breach 
        /// </summary>
        public static Item HallowedBreach = new Item
        {
            Id = 223461,
            Name = "Hallowed Breach",
            Quality = ItemQuality.Legendary,
            Slug = "hallowed-breach",
            ItemType = ItemType.Axe,
            BaseType = ItemBaseType.Weapon,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/hallowed-breach",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/hallowed-breach",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/hallowed-breach",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "Hallowed Protectors",
        };

        /// <summary>
        /// Scrimshaw 
        /// </summary>
        public static Item Scrimshaw = new Item
        {
            Id = 197095,
            Name = "Scrimshaw",
            Quality = ItemQuality.Legendary,
            Slug = "scrimshaw",
            ItemType = ItemType.Spear,
            BaseType = ItemBaseType.Weapon,
            InternalName = "Spear_norm_unique_04",
            DataUrl = "https://us.battle.net/api/d3/data/item/scrimshaw",
            Url = "https://us.battle.net/d3/en/item/scrimshaw",
            RelativeUrl = "/d3/en/item/scrimshaw",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Arreat's Law 
        /// </summary>
        public static Item ArreatsLaw = new Item
        {
            Id = 191446,
            Name = "Arreat's Law",
            Quality = ItemQuality.Legendary,
            Slug = "arreats-law",
            ItemType = ItemType.Spear,
            BaseType = ItemBaseType.Weapon,
            InternalName = "Spear_norm_unique_01",
            DataUrl = "https://us.battle.net/api/d3/data/item/arreats-law",
            Url = "https://us.battle.net/d3/en/item/arreats-law",
            RelativeUrl = "/d3/en/item/arreats-law",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// The Three Hundredth Spear 
        /// </summary>
        public static Item TheThreeHundredthSpear = new Item
        {
            Id = 196638,
            Name = "The Three Hundredth Spear",
            Quality = ItemQuality.Legendary,
            Slug = "the-three-hundredth-spear",
            ItemType = ItemType.Spear,
            BaseType = ItemBaseType.Weapon,
            InternalName = "Spear_norm_unique_03",
            DataUrl = "https://us.battle.net/api/d3/data/item/the-three-hundredth-spear",
            Url = "https://us.battle.net/d3/en/item/the-three-hundredth-spear",
            RelativeUrl = "/d3/en/item/the-three-hundredth-spear",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Empyrean Messenger 
        /// </summary>
        public static Item EmpyreanMessenger = new Item
        {
            Id = 194241,
            Name = "Empyrean Messenger",
            Quality = ItemQuality.Legendary,
            Slug = "empyrean-messenger",
            ItemType = ItemType.Spear,
            BaseType = ItemBaseType.Weapon,
            InternalName = "spear_norm_unique_02",
            DataUrl = "https://us.battle.net/api/d3/data/item/empyrean-messenger",
            Url = "https://us.battle.net/d3/en/item/empyrean-messenger",
            RelativeUrl = "/d3/en/item/empyrean-messenger",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Akanesh, the Herald of Righteousness 
        /// </summary>
        public static Item AkaneshTheHeraldOfRighteousness = new Item
        {
            Id = 272043,
            Name = "Akanesh, the Herald of Righteousness",
            Quality = ItemQuality.Legendary,
            Slug = "akanesh-the-herald-of-righteousness",
            ItemType = ItemType.Spear,
            BaseType = ItemBaseType.Weapon,
            InternalName = "spear_norm_unique_05",
            DataUrl = "https://us.battle.net/api/d3/data/item/akanesh-the-herald-of-righteousness",
            Url = "https://us.battle.net/d3/en/item/akanesh-the-herald-of-righteousness",
            RelativeUrl = "/d3/en/item/akanesh-the-herald-of-righteousness",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Fleshrake 
        /// </summary>
        public static Item Fleshrake = new Item
        {
            Id = 145850,
            Name = "Fleshrake",
            Quality = ItemQuality.Legendary,
            Slug = "fleshrake",
            ItemType = ItemType.FistWeapon,
            BaseType = ItemBaseType.Weapon,
            InternalName = "fistWeapon_norm_unique_03",
            DataUrl = "https://us.battle.net/api/d3/data/item/fleshrake",
            Url = "https://us.battle.net/d3/en/item/fleshrake",
            RelativeUrl = "/d3/en/item/fleshrake",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Rabid Strike 
        /// </summary>
        public static Item RabidStrike = new Item
        {
            Id = 196472,
            Name = "Rabid Strike",
            Quality = ItemQuality.Legendary,
            Slug = "rabid-strike",
            ItemType = ItemType.FistWeapon,
            BaseType = ItemBaseType.Weapon,
            InternalName = "fistweapon_norm_unique_10",
            DataUrl = "https://us.battle.net/api/d3/data/item/rabid-strike",
            Url = "https://us.battle.net/d3/en/item/rabid-strike",
            RelativeUrl = "/d3/en/item/rabid-strike",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Scarbringer 
        /// </summary>
        public static Item Scarbringer = new Item
        {
            Id = 130557,
            Name = "Scarbringer",
            Quality = ItemQuality.Legendary,
            Slug = "scarbringer",
            ItemType = ItemType.FistWeapon,
            BaseType = ItemBaseType.Weapon,
            InternalName = "fistWeapon_norm_unique_01",
            DataUrl = "https://us.battle.net/api/d3/data/item/scarbringer",
            Url = "https://us.battle.net/d3/en/item/scarbringer",
            RelativeUrl = "/d3/en/item/scarbringer",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Sledge Fist 
        /// </summary>
        public static Item SledgeFist = new Item
        {
            Id = 175938,
            Name = "Sledge Fist",
            Quality = ItemQuality.Legendary,
            Slug = "sledge-fist",
            ItemType = ItemType.FistWeapon,
            BaseType = ItemBaseType.Weapon,
            InternalName = "fistWeapon_norm_unique_07",
            DataUrl = "https://us.battle.net/api/d3/data/item/sledge-fist",
            Url = "https://us.battle.net/d3/en/item/sledge-fist",
            RelativeUrl = "/d3/en/item/sledge-fist",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Jawbreaker When Dashing Strike hits an enemy more than 30–35 yards away, its Charge cost is refunded.
        /// </summary>
        public static Item Jawbreaker = new Item
        {
            Id = 271957,
            Name = "Jawbreaker",
            Quality = ItemQuality.Legendary,
            Slug = "jawbreaker",
            ItemType = ItemType.FistWeapon,
            BaseType = ItemBaseType.Weapon,
            InternalName = "fistweapon_norm_unique_14",
            DataUrl = "https://us.battle.net/api/d3/data/item/jawbreaker",
            Url = "https://us.battle.net/d3/en/item/jawbreaker",
            RelativeUrl = "/d3/en/item/jawbreaker",
            IsCrafted = false,
            LegendaryAffix = "When Dashing Strike hits an enemy more than 30–35 yards away, its Charge cost is refunded.",
            SetName = "",
        };

        /// <summary>
        /// Logan's Claw 
        /// </summary>
        public static Item LogansClaw = new Item
        {
            Id = 145849,
            Name = "Logan's Claw",
            Quality = ItemQuality.Legendary,
            Slug = "logans-claw",
            ItemType = ItemType.FistWeapon,
            BaseType = ItemBaseType.Weapon,
            InternalName = "fistweapon_norm_unique_02",
            DataUrl = "https://us.battle.net/api/d3/data/item/logans-claw",
            Url = "https://us.battle.net/d3/en/item/logans-claw",
            RelativeUrl = "/d3/en/item/logans-claw",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Demon Hand 
        /// </summary>
        public static Item DemonHand = new Item
        {
            Id = 0,
            Name = "Demon Hand",
            Quality = ItemQuality.Legendary,
            Slug = "demon-hand",
            ItemType = ItemType.FistWeapon,
            BaseType = ItemBaseType.Weapon,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/demon-hand",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/demon-hand",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/demon-hand",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Crystal Fist 
        /// </summary>
        public static Item CrystalFist = new Item
        {
            Id = 175939,
            Name = "Crystal Fist",
            Quality = ItemQuality.Legendary,
            Slug = "crystal-fist",
            ItemType = ItemType.FistWeapon,
            BaseType = ItemBaseType.Weapon,
            InternalName = "fistWeapon_norm_unique_08",
            DataUrl = "https://us.battle.net/api/d3/data/item/crystal-fist",
            Url = "https://us.battle.net/d3/en/item/crystal-fist",
            RelativeUrl = "/d3/en/item/crystal-fist",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// The Fist of Az'Turrasq Exploding Palm's on-death explosion damage is increased by 75–100%.
        /// </summary>
        public static Item TheFistOfAzturrasq = new Item
        {
            Id = 175937,
            Name = "The Fist of Az'Turrasq",
            Quality = ItemQuality.Legendary,
            Slug = "the-fist-of-azturrasq",
            ItemType = ItemType.FistWeapon,
            BaseType = ItemBaseType.Weapon,
            InternalName = "fistWeapon_norm_unique_06",
            DataUrl = "https://us.battle.net/api/d3/data/item/the-fist-of-azturrasq",
            Url = "https://us.battle.net/d3/en/item/the-fist-of-azturrasq",
            RelativeUrl = "/d3/en/item/the-fist-of-azturrasq",
            IsCrafted = false,
            LegendaryAffix = "Exploding Palm's on-death explosion damage is increased by 75–100%.",
            SetName = "",
        };

        /// <summary>
        /// Won Khim Lau 
        /// </summary>
        public static Item WonKhimLau = new Item
        {
            Id = 145851,
            Name = "Won Khim Lau",
            Quality = ItemQuality.Legendary,
            Slug = "won-khim-lau",
            ItemType = ItemType.FistWeapon,
            BaseType = ItemBaseType.Weapon,
            InternalName = "fistWeapon_norm_unique_04",
            DataUrl = "https://us.battle.net/api/d3/data/item/won-khim-lau",
            Url = "https://us.battle.net/d3/en/item/won-khim-lau",
            RelativeUrl = "/d3/en/item/won-khim-lau",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Shenlong's Fist of Legend 
        /// </summary>
        public static Item ShenlongsFistOfLegend = new Item
        {
            Id = 208996,
            Name = "Shenlong's Fist of Legend",
            Quality = ItemQuality.Legendary,
            Slug = "shenlongs-fist-of-legend",
            ItemType = ItemType.FistWeapon,
            BaseType = ItemBaseType.Weapon,
            InternalName = "fistWeapon_norm_unique_12",
            DataUrl = "https://us.battle.net/api/d3/data/item/shenlongs-fist-of-legend",
            Url = "https://us.battle.net/d3/en/item/shenlongs-fist-of-legend",
            RelativeUrl = "/d3/en/item/shenlongs-fist-of-legend",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Shenlong's Spirit",
        };

        /// <summary>
        /// Shenlong's Relentless Assault 
        /// </summary>
        public static Item ShenlongsRelentlessAssault = new Item
        {
            Id = 208898,
            Name = "Shenlong's Relentless Assault",
            Quality = ItemQuality.Legendary,
            Slug = "shenlongs-relentless-assault",
            ItemType = ItemType.FistWeapon,
            BaseType = ItemBaseType.Weapon,
            InternalName = "fistWeapon_norm_unique_11",
            DataUrl = "https://us.battle.net/api/d3/data/item/shenlongs-relentless-assault",
            Url = "https://us.battle.net/d3/en/item/shenlongs-relentless-assault",
            RelativeUrl = "/d3/en/item/shenlongs-relentless-assault",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Shenlong's Spirit",
        };

        /// <summary>
        /// Hallowed Hand 
        /// </summary>
        public static Item HallowedHand = new Item
        {
            Id = 223526,
            Name = "Hallowed Hand",
            Quality = ItemQuality.Legendary,
            Slug = "hallowed-touch",
            ItemType = ItemType.FistWeapon,
            BaseType = ItemBaseType.Weapon,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/hallowed-touch",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/hallowed-touch",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/hallowed-touch",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "Hallowed Defenders",
        };

        /// <summary>
        /// Vengeful Wind Increases the maximum stack count of Sweeping Wind by 3.
        /// </summary>
        public static Item VengefulWind = new Item
        {
            Id = 403775,
            Name = "Vengeful Wind",
            Quality = ItemQuality.Legendary,
            Slug = "vengeful-wind",
            ItemType = ItemType.FistWeapon,
            BaseType = ItemBaseType.Weapon,
            InternalName = "p1_fistWeapon_norm_unique_02",
            DataUrl = "https://us.battle.net/api/d3/data/item/vengeful-wind",
            Url = "https://us.battle.net/d3/en/item/vengeful-wind",
            RelativeUrl = "/d3/en/item/vengeful-wind",
            IsCrafted = false,
            LegendaryAffix = "Increases the maximum stack count of Sweeping Wind by 3.",
            SetName = "",
        };

        /// <summary>
        /// Demon Claw 
        /// </summary>
        public static Item DemonClaw = new Item
        {
            Id = 193459,
            Name = "Demon Claw",
            Quality = ItemQuality.Legendary,
            Slug = "demon-claw",
            ItemType = ItemType.FistWeapon,
            BaseType = ItemBaseType.Weapon,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/demon-claw",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/demon-claw",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/demon-claw",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Hallowed Hold 
        /// </summary>
        public static Item HallowedHold = new Item
        {
            Id = 223526,
            Name = "Hallowed Hold",
            Quality = ItemQuality.Legendary,
            Slug = "hallowed-hold",
            ItemType = ItemType.FistWeapon,
            BaseType = ItemBaseType.Weapon,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/hallowed-hold",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/hallowed-hold",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/hallowed-hold",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "Hallowed Protectors",
        };

        /// <summary>
        /// The Barber 
        /// </summary>
        public static Item TheBarber = new Item
        {
            Id = 195174,
            Name = "The Barber",
            Quality = ItemQuality.Legendary,
            Slug = "the-barber",
            ItemType = ItemType.Dagger,
            BaseType = ItemBaseType.Weapon,
            InternalName = "Dagger_norm_unique_03",
            DataUrl = "https://us.battle.net/api/d3/data/item/the-barber",
            Url = "https://us.battle.net/d3/en/item/the-barber",
            RelativeUrl = "/d3/en/item/the-barber",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Pig Sticker Squeal!
        /// </summary>
        public static Item PigSticker = new Item
        {
            Id = 221313,
            Name = "Pig Sticker",
            Quality = ItemQuality.Legendary,
            Slug = "pig-sticker",
            ItemType = ItemType.Dagger,
            BaseType = ItemBaseType.Weapon,
            InternalName = "dagger_norm_unique_06",
            DataUrl = "https://us.battle.net/api/d3/data/item/pig-sticker",
            Url = "https://us.battle.net/d3/en/item/pig-sticker",
            RelativeUrl = "/d3/en/item/pig-sticker",
            IsCrafted = false,
            LegendaryAffix = "Squeal!",
            SetName = "",
        };

        /// <summary>
        /// Kill 
        /// </summary>
        public static Item Kill = new Item
        {
            Id = 192579,
            Name = "Kill",
            Quality = ItemQuality.Legendary,
            Slug = "kill",
            ItemType = ItemType.Dagger,
            BaseType = ItemBaseType.Weapon,
            InternalName = "Dagger_norm_unique_02",
            DataUrl = "https://us.battle.net/api/d3/data/item/kill",
            Url = "https://us.battle.net/d3/en/item/kill",
            RelativeUrl = "/d3/en/item/kill",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Wizardspike Performing an attack has a 20–25% chance to hurl a Frozen Orb.
        /// </summary>
        public static Item Wizardspike = new Item
        {
            Id = 219329,
            Name = "Wizardspike",
            Quality = ItemQuality.Legendary,
            Slug = "wizardspike",
            ItemType = ItemType.Dagger,
            BaseType = ItemBaseType.Weapon,
            InternalName = "Dagger_norm_unique_05",
            DataUrl = "https://us.battle.net/api/d3/data/item/wizardspike",
            Url = "https://us.battle.net/d3/en/item/wizardspike",
            RelativeUrl = "/d3/en/item/wizardspike",
            IsCrafted = false,
            LegendaryAffix = "Performing an attack has a 20–25% chance to hurl a Frozen Orb.",
            SetName = "",
        };

        /// <summary>
        /// Blood-Magic Blade Blood oozes from you.
        /// </summary>
        public static Item BloodmagicBlade = new Item
        {
            Id = 0,
            Name = "Blood-Magic Blade",
            Quality = ItemQuality.Legendary,
            Slug = "bloodmagic-blade",
            ItemType = ItemType.Dagger,
            BaseType = ItemBaseType.Weapon,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/bloodmagic-blade",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/bloodmagic-blade",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/bloodmagic-blade",
            IsCrafted = true,
            LegendaryAffix = "Blood oozes from you.",
            SetName = "",
        };

        /// <summary>
        /// Blood-Magic Edge Blood oozes from you.
        /// </summary>
        public static Item BloodmagicEdge = new Item
        {
            Id = 195655,
            Name = "Blood-Magic Edge",
            Quality = ItemQuality.Legendary,
            Slug = "bloodmagic-edge",
            ItemType = ItemType.Dagger,
            BaseType = ItemBaseType.Weapon,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/bloodmagic-edge",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/bloodmagic-edge",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/bloodmagic-edge",
            IsCrafted = true,
            LegendaryAffix = "Blood oozes from you.",
            SetName = "",
        };

        /// <summary>
        /// Deadly Rebirth Grasp of the Dead gains the effect of the Rain of Corpses rune.
        /// </summary>
        public static Item DeadlyRebirth = new Item
        {
            Id = 193433,
            Name = "Deadly Rebirth",
            Quality = ItemQuality.Legendary,
            Slug = "deadly-rebirth",
            ItemType = ItemType.CeremonialDagger,
            BaseType = ItemBaseType.Weapon,
            InternalName = "ceremonialDagger_norm_unique_02",
            DataUrl = "https://us.battle.net/api/d3/data/item/deadly-rebirth",
            Url = "https://us.battle.net/d3/en/item/deadly-rebirth",
            RelativeUrl = "/d3/en/item/deadly-rebirth",
            IsCrafted = false,
            LegendaryAffix = "Grasp of the Dead gains the effect of the Rain of Corpses rune.",
            SetName = "",
        };

        /// <summary>
        /// Umbral Oath 
        /// </summary>
        public static Item UmbralOath = new Item
        {
            Id = 192540,
            Name = "Umbral Oath",
            Quality = ItemQuality.Legendary,
            Slug = "umbral-oath",
            ItemType = ItemType.CeremonialDagger,
            BaseType = ItemBaseType.Weapon,
            InternalName = "ceremonialDagger_norm_unique_01",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/umbral-oath",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/umbral-oath",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/umbral-oath",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Rhen'ho Flayer Plague of Toads now seek out enemies and can explode twice.
        /// </summary>
        public static Item RhenhoFlayer = new Item
        {
            Id = 271745,
            Name = "Rhen'ho Flayer",
            Quality = ItemQuality.Legendary,
            Slug = "rhenho-flayer",
            ItemType = ItemType.CeremonialDagger,
            BaseType = ItemBaseType.Weapon,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/item/rhenho-flayer",
            Url = "https://us.battle.net/d3/en/item/rhenho-flayer",
            RelativeUrl = "/d3/en/item/rhenho-flayer",
            IsCrafted = false,
            LegendaryAffix = "Plague of Toads now seek out enemies and can explode twice.",
            SetName = "",
        };

        /// <summary>
        /// Sacred Harvester Soul Harvest now stacks up to 10 times.
        /// </summary>
        public static Item SacredHarvester = new Item
        {
            Id = 403748,
            Name = "Sacred Harvester",
            Quality = ItemQuality.Legendary,
            Slug = "sacred-harvester",
            ItemType = ItemType.CeremonialDagger,
            BaseType = ItemBaseType.Weapon,
            InternalName = "p1_ceremonialDagger_norm_unique_01",
            DataUrl = "https://us.battle.net/api/d3/data/item/sacred-harvester",
            Url = "https://us.battle.net/d3/en/item/sacred-harvester",
            RelativeUrl = "/d3/en/item/sacred-harvester",
            IsCrafted = false,
            LegendaryAffix = "Soul Harvest now stacks up to 10 times.",
            SetName = "",
        };

        /// <summary>
        /// The Dagger of Darts Your Poison Darts and your Fetishes' Poison Darts now pierce.
        /// </summary>
        public static Item TheDaggerOfDarts = new Item
        {
            Id = 403767,
            Name = "The Dagger of Darts",
            Quality = ItemQuality.Legendary,
            Slug = "the-dagger-of-darts",
            ItemType = ItemType.CeremonialDagger,
            BaseType = ItemBaseType.Weapon,
            InternalName = "p1_ceremonialDagger_norm_unique_02",
            DataUrl = "https://us.battle.net/api/d3/data/item/the-dagger-of-darts",
            Url = "https://us.battle.net/d3/en/item/the-dagger-of-darts",
            RelativeUrl = "/d3/en/item/the-dagger-of-darts",
            IsCrafted = false,
            LegendaryAffix = "Your Poison Darts and your Fetishes' Poison Darts now pierce.",
            SetName = "",
        };

        /// <summary>
        /// Last Breath 
        /// </summary>
        public static Item LastBreath = new Item
        {
            Id = 195370,
            Name = "Last Breath",
            Quality = ItemQuality.Legendary,
            Slug = "last-breath",
            ItemType = ItemType.CeremonialDagger,
            BaseType = ItemBaseType.Weapon,
            InternalName = "ceremonialDagger_norm_unique_03",
            DataUrl = "https://us.battle.net/api/d3/data/item/last-breath",
            Url = "https://us.battle.net/d3/en/item/last-breath",
            RelativeUrl = "/d3/en/item/last-breath",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// The Spider Queen's Grasp Corpse Spiders releases a web on impact that Slows enemies by 60–80%.
        /// </summary>
        public static Item TheSpiderQueensGrasp = new Item
        {
            Id = 222978,
            Name = "The Spider Queen's Grasp",
            Quality = ItemQuality.Legendary,
            Slug = "the-spider-queens-grasp",
            ItemType = ItemType.CeremonialDagger,
            BaseType = ItemBaseType.Weapon,
            InternalName = "ceremonialDagger_norm_unique_05",
            DataUrl = "https://us.battle.net/api/d3/data/item/the-spider-queens-grasp",
            Url = "https://us.battle.net/d3/en/item/the-spider-queens-grasp",
            RelativeUrl = "/d3/en/item/the-spider-queens-grasp",
            IsCrafted = false,
            LegendaryAffix = "Corpse Spiders releases a web on impact that Slows enemies by 60–80%.",
            SetName = "",
        };

        /// <summary>
        /// Starmetal Kukri Reduce the cooldown of Fetish Army and Big Bad Voodoo by 1 second each time your fetishes deal damage.
        /// </summary>
        public static Item StarmetalKukri = new Item
        {
            Id = 271738,
            Name = "Starmetal Kukri",
            Quality = ItemQuality.Legendary,
            Slug = "starmetal-kukri",
            ItemType = ItemType.CeremonialDagger,
            BaseType = ItemBaseType.Weapon,
            InternalName = "ceremonialdagger_norm_unique_10",
            DataUrl = "https://us.battle.net/api/d3/data/item/starmetal-kukri",
            Url = "https://us.battle.net/d3/en/item/starmetal-kukri",
            RelativeUrl = "/d3/en/item/starmetal-kukri",
            IsCrafted = false,
            LegendaryAffix = "Reduce the cooldown of Fetish Army and Big Bad Voodoo by 1 second each time your fetishes deal damage.",
            SetName = "",
        };

        /// <summary>
        /// Anessazi Edge Zombie Dogs stuns enemies around them for 1.5 seconds when summoned.
        /// </summary>
        public static Item AnessaziEdge = new Item
        {
            Id = 196250,
            Name = "Anessazi Edge",
            Quality = ItemQuality.Legendary,
            Slug = "anessazi-edge",
            ItemType = ItemType.CeremonialDagger,
            BaseType = ItemBaseType.Weapon,
            InternalName = "ceremonialDagger_norm_unique_04",
            DataUrl = "https://us.battle.net/api/d3/data/item/anessazi-edge",
            Url = "https://us.battle.net/d3/en/item/anessazi-edge",
            RelativeUrl = "/d3/en/item/anessazi-edge",
            IsCrafted = false,
            LegendaryAffix = "Zombie Dogs stuns enemies around them for 1.5 seconds when summoned.",
            SetName = "",
        };

        /// <summary>
        /// The Gidbinn Chance to summon a Fetish when attacking.
        /// </summary>
        public static Item TheGidbinn = new Item
        {
            Id = 209246,
            Name = "The Gidbinn",
            Quality = ItemQuality.Legendary,
            Slug = "the-gidbinn",
            ItemType = ItemType.CeremonialDagger,
            BaseType = ItemBaseType.Weapon,
            InternalName = "ceremonialDagger_norm_unique_09",
            DataUrl = "https://us.battle.net/api/d3/data/item/the-gidbinn",
            Url = "https://us.battle.net/d3/en/item/the-gidbinn",
            RelativeUrl = "/d3/en/item/the-gidbinn",
            IsCrafted = false,
            LegendaryAffix = "Chance to summon a Fetish when attacking.",
            SetName = "",
        };

        /// <summary>
        /// Manajuma's Carving Knife 
        /// </summary>
        public static Item ManajumasCarvingKnife = new Item
        {
            Id = 223365,
            Name = "Manajuma's Carving Knife",
            Quality = ItemQuality.Legendary,
            Slug = "manajumas-carving-knife",
            ItemType = ItemType.CeremonialDagger,
            BaseType = ItemBaseType.Weapon,
            InternalName = "ceremonialDagger_norm_unique_06",
            DataUrl = "https://us.battle.net/api/d3/data/item/manajumas-carving-knife",
            Url = "https://us.battle.net/d3/en/item/manajumas-carving-knife",
            RelativeUrl = "/d3/en/item/manajumas-carving-knife",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Manajuma's Way",
        };

        /// <summary>
        /// Hallowed Salvation 
        /// </summary>
        public static Item HallowedSalvation = new Item
        {
            Id = 0,
            Name = "Hallowed Salvation",
            Quality = ItemQuality.Legendary,
            Slug = "hallowed-sacrifice",
            ItemType = ItemType.CeremonialDagger,
            BaseType = ItemBaseType.Weapon,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/hallowed-sacrifice",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/hallowed-sacrifice",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/hallowed-sacrifice",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "Hallowed Defenders",
        };

        /// <summary>
        /// Living Umbral Oath 
        /// </summary>
        public static Item LivingUmbralOath = new Item
        {
            Id = 0,
            Name = "Living Umbral Oath",
            Quality = ItemQuality.Legendary,
            Slug = "living-umbral-oath",
            ItemType = ItemType.CeremonialDagger,
            BaseType = ItemBaseType.Weapon,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/living-umbral-oath",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/living-umbral-oath",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/living-umbral-oath",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Hallowed Sufferance 
        /// </summary>
        public static Item HallowedSufferance = new Item
        {
            Id = 0,
            Name = "Hallowed Sufferance",
            Quality = ItemQuality.Legendary,
            Slug = "hallowed-sufferance",
            ItemType = ItemType.CeremonialDagger,
            BaseType = ItemBaseType.Weapon,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/hallowed-sufferance",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/hallowed-sufferance",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/hallowed-sufferance",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "Hallowed Protectors",
        };

        /// <summary>
        /// Odyn Son 20–40% chance to Chain Lightning enemies when you hit them.
        /// </summary>
        public static Item OdynSon = new Item
        {
            Id = 188185,
            Name = "Odyn Son",
            Quality = ItemQuality.Legendary,
            Slug = "odyn-son",
            ItemType = ItemType.Mace,
            BaseType = ItemBaseType.Weapon,
            InternalName = "Mace_norm_unique_08",
            DataUrl = "https://us.battle.net/api/d3/data/item/odyn-son",
            Url = "https://us.battle.net/d3/en/item/odyn-son",
            RelativeUrl = "/d3/en/item/odyn-son",
            IsCrafted = false,
            LegendaryAffix = "20–40% chance to Chain Lightning enemies when you hit them.",
            SetName = "",
        };

        /// <summary>
        /// Nutcracker 
        /// </summary>
        public static Item Nutcracker = new Item
        {
            Id = 188169,
            Name = "Nutcracker",
            Quality = ItemQuality.Legendary,
            Slug = "nutcracker",
            ItemType = ItemType.Mace,
            BaseType = ItemBaseType.Weapon,
            InternalName = "Mace_norm_unique_03",
            DataUrl = "https://us.battle.net/api/d3/data/item/nutcracker",
            Url = "https://us.battle.net/d3/en/item/nutcracker",
            RelativeUrl = "/d3/en/item/nutcracker",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Telranden's Hand 
        /// </summary>
        public static Item TelrandensHand = new Item
        {
            Id = 188189,
            Name = "Telranden's Hand",
            Quality = ItemQuality.Legendary,
            Slug = "telrandens-hand",
            ItemType = ItemType.Mace,
            BaseType = ItemBaseType.Weapon,
            InternalName = "Mace_norm_unique_09",
            DataUrl = "https://us.battle.net/api/d3/data/item/telrandens-hand",
            Url = "https://us.battle.net/d3/en/item/telrandens-hand",
            RelativeUrl = "/d3/en/item/telrandens-hand",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Jace's Hammer of Vigilance Increase the size of your Blessed Hammers.
        /// </summary>
        public static Item JacesHammerOfVigilance = new Item
        {
            Id = 271648,
            Name = "Jace's Hammer of Vigilance",
            Quality = ItemQuality.Legendary,
            Slug = "jaces-hammer-of-vigilance",
            ItemType = ItemType.Mace,
            BaseType = ItemBaseType.Weapon,
            InternalName = "mace_norm_unique_10",
            DataUrl = "https://us.battle.net/api/d3/data/item/jaces-hammer-of-vigilance",
            Url = "https://us.battle.net/d3/en/item/jaces-hammer-of-vigilance",
            RelativeUrl = "/d3/en/item/jaces-hammer-of-vigilance",
            IsCrafted = false,
            LegendaryAffix = "Increase the size of your Blessed Hammers.",
            SetName = "",
        };

        /// <summary>
        /// Solanium Critical Hits have a 3–4% chance to spawn a health globe.
        /// </summary>
        public static Item Solanium = new Item
        {
            Id = 271662,
            Name = "Solanium",
            Quality = ItemQuality.Legendary,
            Slug = "solanium",
            ItemType = ItemType.Mace,
            BaseType = ItemBaseType.Weapon,
            InternalName = "mace_norm_unique_11",
            DataUrl = "https://us.battle.net/api/d3/data/item/solanium",
            Url = "https://us.battle.net/d3/en/item/solanium",
            RelativeUrl = "/d3/en/item/solanium",
            IsCrafted = false,
            LegendaryAffix = "Critical Hits have a 3–4% chance to spawn a health globe.",
            SetName = "",
        };

        /// <summary>
        /// Nailbiter 
        /// </summary>
        public static Item Nailbiter = new Item
        {
            Id = 188158,
            Name = "Nailbiter",
            Quality = ItemQuality.Legendary,
            Slug = "nailbiter",
            ItemType = ItemType.Mace,
            BaseType = ItemBaseType.Weapon,
            InternalName = "Mace_norm_unique_02",
            DataUrl = "https://us.battle.net/api/d3/data/item/nailbiter",
            Url = "https://us.battle.net/d3/en/item/nailbiter",
            RelativeUrl = "/d3/en/item/nailbiter",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Neanderthal 
        /// </summary>
        public static Item Neanderthal = new Item
        {
            Id = 102665,
            Name = "Neanderthal",
            Quality = ItemQuality.Legendary,
            Slug = "neanderthal",
            ItemType = ItemType.Mace,
            BaseType = ItemBaseType.Weapon,
            InternalName = "Mace_norm_unique_01",
            DataUrl = "https://us.battle.net/api/d3/data/item/neanderthal",
            Url = "https://us.battle.net/d3/en/item/neanderthal",
            RelativeUrl = "/d3/en/item/neanderthal",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Echoing Fury 
        /// </summary>
        public static Item EchoingFury = new Item
        {
            Id = 188181,
            Name = "Echoing Fury",
            Quality = ItemQuality.Legendary,
            Slug = "echoing-fury",
            ItemType = ItemType.Mace,
            BaseType = ItemBaseType.Weapon,
            InternalName = "Mace_norm_unique_07",
            DataUrl = "https://us.battle.net/api/d3/data/item/echoing-fury",
            Url = "https://us.battle.net/d3/en/item/echoing-fury",
            RelativeUrl = "/d3/en/item/echoing-fury",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Sun Keeper 
        /// </summary>
        public static Item SunKeeper = new Item
        {
            Id = 188173,
            Name = "Sun Keeper",
            Quality = ItemQuality.Legendary,
            Slug = "sun-keeper",
            ItemType = ItemType.Mace,
            BaseType = ItemBaseType.Weapon,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/item/sun-keeper",
            Url = "https://us.battle.net/d3/en/item/sun-keeper",
            RelativeUrl = "/d3/en/item/sun-keeper",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Earthshatter 20–35% chance to cause the ground to shudder when attacking.
        /// </summary>
        public static Item Earthshatter = new Item
        {
            Id = 0,
            Name = "Earthshatter",
            Quality = ItemQuality.Legendary,
            Slug = "earthshatter",
            ItemType = ItemType.Mace,
            BaseType = ItemBaseType.Weapon,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/earthshatter",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/earthshatter",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/earthshatter",
            IsCrafted = true,
            LegendaryAffix = "20–35% chance to cause the ground to shudder when attacking.",
            SetName = "",
        };

        /// <summary>
        /// Devastator 
        /// </summary>
        public static Item Devastator = new Item
        {
            Id = 188177,
            Name = "Devastator",
            Quality = ItemQuality.Legendary,
            Slug = "devastator",
            ItemType = ItemType.Mace,
            BaseType = ItemBaseType.Weapon,
            InternalName = "Mace_norm_unique_06",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/devastator",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/devastator",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/devastator",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Darklight Fist of the Heavens has a 45–60% chance to also be cast at your location.
        /// </summary>
        public static Item Darklight = new Item
        {
            Id = 299428,
            Name = "Darklight",
            Quality = ItemQuality.Legendary,
            Slug = "darklight",
            ItemType = ItemType.Flail,
            BaseType = ItemBaseType.Weapon,
            InternalName = "flail1h_norm_unique_06",
            DataUrl = "https://us.battle.net/api/d3/data/item/darklight",
            Url = "https://us.battle.net/d3/en/item/darklight",
            RelativeUrl = "/d3/en/item/darklight",
            IsCrafted = false,
            LegendaryAffix = "Fist of the Heavens has a 45–60% chance to also be cast at your location.",
            SetName = "",
        };

        /// <summary>
        /// Gyrfalcon's Foote Removes the resource cost of Blessed Shield.
        /// </summary>
        public static Item GyrfalconsFoote = new Item
        {
            Id = 299427,
            Name = "Gyrfalcon's Foote",
            Quality = ItemQuality.Legendary,
            Slug = "gyrfalcons-foote",
            ItemType = ItemType.Flail,
            BaseType = ItemBaseType.Weapon,
            InternalName = "flail1h_norm_unique_05",
            DataUrl = "https://us.battle.net/api/d3/data/item/gyrfalcons-foote",
            Url = "https://us.battle.net/d3/en/item/gyrfalcons-foote",
            RelativeUrl = "/d3/en/item/gyrfalcons-foote",
            IsCrafted = false,
            LegendaryAffix = "Removes the resource cost of Blessed Shield.",
            SetName = "",
        };

        /// <summary>
        /// Inviolable Faith Casting Consecration also casts Consecration beneath all of your allies.
        /// </summary>
        public static Item InviolableFaith = new Item
        {
            Id = 299429,
            Name = "Inviolable Faith",
            Quality = ItemQuality.Legendary,
            Slug = "inviolable-faith",
            ItemType = ItemType.Flail,
            BaseType = ItemBaseType.Weapon,
            InternalName = "flail1h_norm_unique_07",
            DataUrl = "https://us.battle.net/api/d3/data/item/inviolable-faith",
            Url = "https://us.battle.net/d3/en/item/inviolable-faith",
            RelativeUrl = "/d3/en/item/inviolable-faith",
            IsCrafted = false,
            LegendaryAffix = "Casting Consecration also casts Consecration beneath all of your allies.",
            SetName = "",
        };

        /// <summary>
        /// Justinian's Mercy Blessed Hammer gains the effect of the Dominion rune.
        /// </summary>
        public static Item JustiniansMercy = new Item
        {
            Id = 299424,
            Name = "Justinian's Mercy",
            Quality = ItemQuality.Legendary,
            Slug = "justinians-mercy",
            ItemType = ItemType.Flail,
            BaseType = ItemBaseType.Weapon,
            InternalName = "flail1h_norm_unique_02",
            DataUrl = "https://us.battle.net/api/d3/data/item/justinians-mercy",
            Url = "https://us.battle.net/d3/en/item/justinians-mercy",
            RelativeUrl = "/d3/en/item/justinians-mercy",
            IsCrafted = false,
            LegendaryAffix = "Blessed Hammer gains the effect of the Dominion rune.",
            SetName = "",
        };

        /// <summary>
        /// Kassar's Retribution Casting Justice increases your movement speed by 15–20% for 2 seconds.
        /// </summary>
        public static Item KassarsRetribution = new Item
        {
            Id = 299426,
            Name = "Kassar's Retribution",
            Quality = ItemQuality.Legendary,
            Slug = "kassars-retribution",
            ItemType = ItemType.Flail,
            BaseType = ItemBaseType.Weapon,
            InternalName = "flail1h_norm_unique_04",
            DataUrl = "https://us.battle.net/api/d3/data/item/kassars-retribution",
            Url = "https://us.battle.net/d3/en/item/kassars-retribution",
            RelativeUrl = "/d3/en/item/kassars-retribution",
            IsCrafted = false,
            LegendaryAffix = "Casting Justice increases your movement speed by 15–20% for 2 seconds.",
            SetName = "",
        };

        /// <summary>
        /// Swiftmount Doubles the duration of Steed Charge.
        /// </summary>
        public static Item Swiftmount = new Item
        {
            Id = 299425,
            Name = "Swiftmount",
            Quality = ItemQuality.Legendary,
            Slug = "swiftmount",
            ItemType = ItemType.Flail,
            BaseType = ItemBaseType.Weapon,
            InternalName = "flail1h_norm_unique_03",
            DataUrl = "https://us.battle.net/api/d3/data/item/swiftmount",
            Url = "https://us.battle.net/d3/en/item/swiftmount",
            RelativeUrl = "/d3/en/item/swiftmount",
            IsCrafted = false,
            LegendaryAffix = "Doubles the duration of Steed Charge.",
            SetName = "",
        };

        /// <summary>
        /// Golden Scourge Smite now jumps to 3 additional enemies.
        /// </summary>
        public static Item GoldenScourge = new Item
        {
            Id = 0,
            Name = "Golden Scourge",
            Quality = ItemQuality.Legendary,
            Slug = "golden-scourge",
            ItemType = ItemType.Flail,
            BaseType = ItemBaseType.Weapon,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/golden-scourge",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/golden-scourge",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/golden-scourge",
            IsCrafted = true,
            LegendaryAffix = "Smite now jumps to 3 additional enemies.",
            SetName = "",
        };

        /// <summary>
        /// Monster Hunter 
        /// </summary>
        public static Item MonsterHunter = new Item
        {
            Id = 115140,
            Name = "Monster Hunter",
            Quality = ItemQuality.Legendary,
            Slug = "monster-hunter",
            ItemType = ItemType.Sword,
            BaseType = ItemBaseType.Weapon,
            InternalName = "Sword_norm_unique_01",
            DataUrl = "https://us.battle.net/api/d3/data/item/monster-hunter",
            Url = "https://us.battle.net/d3/en/item/monster-hunter",
            RelativeUrl = "/d3/en/item/monster-hunter",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Wildwood 
        /// </summary>
        public static Item Wildwood = new Item
        {
            Id = 270978,
            Name = "Wildwood",
            Quality = ItemQuality.Legendary,
            Slug = "wildwood",
            ItemType = ItemType.Sword,
            BaseType = ItemBaseType.Weapon,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/item/wildwood",
            Url = "https://us.battle.net/d3/en/item/wildwood",
            RelativeUrl = "/d3/en/item/wildwood",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Born's Searing Spite 
        /// </summary>
        public static Item BornsSearingSpite = new Item
        {
            Id = 0,
            Name = "Born's Searing Spite",
            Quality = ItemQuality.Legendary,
            Slug = "borns-seething-rage",
            ItemType = ItemType.Sword,
            BaseType = ItemBaseType.Weapon,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/borns-seething-rage",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/borns-seething-rage",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/borns-seething-rage",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "Born's Defiance",
        };

        /// <summary>
        /// Doombringer 
        /// </summary>
        public static Item Doombringer = new Item
        {
            Id = 185397,
            Name = "Doombringer",
            Quality = ItemQuality.Legendary,
            Slug = "doombringer",
            ItemType = ItemType.Sword,
            BaseType = ItemBaseType.Weapon,
            InternalName = "sword_norm_unique_07",
            DataUrl = "https://us.battle.net/api/d3/data/item/doombringer",
            Url = "https://us.battle.net/d3/en/item/doombringer",
            RelativeUrl = "/d3/en/item/doombringer",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// The Ancient Bonesaber of Zumakalis 
        /// </summary>
        public static Item TheAncientBonesaberOfZumakalis = new Item
        {
            Id = 194481,
            Name = "The Ancient Bonesaber of Zumakalis",
            Quality = ItemQuality.Legendary,
            Slug = "the-ancient-bonesaber-of-zumakalis",
            ItemType = ItemType.Sword,
            BaseType = ItemBaseType.Weapon,
            InternalName = "Sword_norm_unique_08",
            DataUrl = "https://us.battle.net/api/d3/data/item/the-ancient-bonesaber-of-zumakalis",
            Url = "https://us.battle.net/d3/en/item/the-ancient-bonesaber-of-zumakalis",
            RelativeUrl = "/d3/en/item/the-ancient-bonesaber-of-zumakalis",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Exarian 
        /// </summary>
        public static Item Exarian = new Item
        {
            Id = 271617,
            Name = "Exarian",
            Quality = ItemQuality.Legendary,
            Slug = "exarian",
            ItemType = ItemType.Sword,
            BaseType = ItemBaseType.Weapon,
            InternalName = "sword_norm_unique_13",
            DataUrl = "https://us.battle.net/api/d3/data/item/exarian",
            Url = "https://us.battle.net/d3/en/item/exarian",
            RelativeUrl = "/d3/en/item/exarian",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Fulminator Lightning damage has a chance to turn enemies into lightning rods, causing them to pulse 167–222% weapon damage as Lightning every second to nearby enemies for 6 seconds.
        /// </summary>
        public static Item Fulminator = new Item
        {
            Id = 271631,
            Name = "Fulminator",
            Quality = ItemQuality.Legendary,
            Slug = "fulminator",
            ItemType = ItemType.Sword,
            BaseType = ItemBaseType.Weapon,
            InternalName = "sword_norm_unique_15",
            DataUrl = "https://us.battle.net/api/d3/data/item/fulminator",
            Url = "https://us.battle.net/d3/en/item/fulminator",
            RelativeUrl = "/d3/en/item/fulminator",
            IsCrafted = false,
            LegendaryAffix = "Lightning damage has a chance to turn enemies into lightning rods, causing them to pulse 167–222% weapon damage as Lightning every second to nearby enemies for 6 seconds.",
            SetName = "",
        };

        /// <summary>
        /// Gift of Silaria 
        /// </summary>
        public static Item GiftOfSilaria = new Item
        {
            Id = 271630,
            Name = "Gift of Silaria",
            Quality = ItemQuality.Legendary,
            Slug = "gift-of-silaria",
            ItemType = ItemType.Sword,
            BaseType = ItemBaseType.Weapon,
            InternalName = "sword_norm_unique_14",
            DataUrl = "https://us.battle.net/api/d3/data/item/gift-of-silaria",
            Url = "https://us.battle.net/d3/en/item/gift-of-silaria",
            RelativeUrl = "/d3/en/item/gift-of-silaria",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Rimeheart 10% chance on hit to instantly deal 10000% weapon damage as Cold to enemies that are Frozen.
        /// </summary>
        public static Item Rimeheart = new Item
        {
            Id = 271636,
            Name = "Rimeheart",
            Quality = ItemQuality.Legendary,
            Slug = "rimeheart",
            ItemType = ItemType.Sword,
            BaseType = ItemBaseType.Weapon,
            InternalName = "sword_norm_unique_20",
            DataUrl = "https://us.battle.net/api/d3/data/item/rimeheart",
            Url = "https://us.battle.net/d3/en/item/rimeheart",
            RelativeUrl = "/d3/en/item/rimeheart",
            IsCrafted = false,
            LegendaryAffix = "10% chance on hit to instantly deal 10000% weapon damage as Cold to enemies that are Frozen.",
            SetName = "",
        };

        /// <summary>
        /// Thunderfury, Blessed Blade of the Windseeker Chance on hit to blast your enemy with Lightning, dealing 279–372% weapon damage as Lightning and then jumping to additional nearby enemies. Each enemy hit has their attack speed and movement speed reduced by 30% for 3 seconds. Jumps up to 5 targets.
        /// </summary>
        public static Item ThunderfuryBlessedBladeOfTheWindseeker = new Item
        {
            Id = 229716,
            Name = "Thunderfury, Blessed Blade of the Windseeker",
            Quality = ItemQuality.Legendary,
            Slug = "thunderfury-blessed-blade-of-the-windseeker",
            ItemType = ItemType.Sword,
            BaseType = ItemBaseType.Weapon,
            InternalName = "sword_norm_unique_12",
            DataUrl = "https://us.battle.net/api/d3/data/item/thunderfury-blessed-blade-of-the-windseeker",
            Url = "https://us.battle.net/d3/en/item/thunderfury-blessed-blade-of-the-windseeker",
            RelativeUrl = "/d3/en/item/thunderfury-blessed-blade-of-the-windseeker",
            IsCrafted = false,
            LegendaryAffix = "Chance on hit to blast your enemy with Lightning, dealing 279–372% weapon damage as Lightning and then jumping to additional nearby enemies. Each enemy hit has their attack speed and movement speed reduced by 30% for 3 seconds. Jumps up to 5 targets.",
            SetName = "",
        };

        /// <summary>
        /// Sever Slain enemies rest in pieces.
        /// </summary>
        public static Item Sever = new Item
        {
            Id = 115141,
            Name = "Sever",
            Quality = ItemQuality.Legendary,
            Slug = "sever",
            ItemType = ItemType.Sword,
            BaseType = ItemBaseType.Weapon,
            InternalName = "Sword_norm_unique_02",
            DataUrl = "https://us.battle.net/api/d3/data/item/sever",
            Url = "https://us.battle.net/d3/en/item/sever",
            RelativeUrl = "/d3/en/item/sever",
            IsCrafted = false,
            LegendaryAffix = "Slain enemies rest in pieces.",
            SetName = "",
        };

        /// <summary>
        /// Skycutter Chance to summon angelic assistance when attacking.
        /// </summary>
        public static Item Skycutter = new Item
        {
            Id = 182347,
            Name = "Skycutter",
            Quality = ItemQuality.Legendary,
            Slug = "skycutter",
            ItemType = ItemType.Sword,
            BaseType = ItemBaseType.Weapon,
            InternalName = "Sword_norm_unique_04",
            DataUrl = "https://us.battle.net/api/d3/data/item/skycutter",
            Url = "https://us.battle.net/d3/en/item/skycutter",
            RelativeUrl = "/d3/en/item/skycutter",
            IsCrafted = false,
            LegendaryAffix = "Chance to summon angelic assistance when attacking.",
            SetName = "",
        };

        /// <summary>
        /// Azurewrath Undead enemies within 25 yards take 30–40% weapon damage as Holy every second and are sometimes knocked back.
        /// </summary>
        public static Item Azurewrath = new Item
        {
            Id = 192511,
            Name = "Azurewrath",
            Quality = ItemQuality.Legendary,
            Slug = "azurewrath",
            ItemType = ItemType.Sword,
            BaseType = ItemBaseType.Weapon,
            InternalName = "sword_norm_unique_06",
            DataUrl = "https://us.battle.net/api/d3/data/item/azurewrath",
            Url = "https://us.battle.net/d3/en/item/azurewrath",
            RelativeUrl = "/d3/en/item/azurewrath",
            IsCrafted = false,
            LegendaryAffix = "Undead enemies within 25 yards take 30–40% weapon damage as Holy every second and are sometimes knocked back.",
            SetName = "",
        };

        /// <summary>
        /// Devil Tongue 
        /// </summary>
        public static Item DevilTongue = new Item
        {
            Id = 189552,
            Name = "Devil Tongue",
            Quality = ItemQuality.Legendary,
            Slug = "devil-tongue",
            ItemType = ItemType.Sword,
            BaseType = ItemBaseType.Weapon,
            InternalName = "Sword_norm_unique_05",
            DataUrl = "https://us.battle.net/api/d3/data/item/devil-tongue",
            Url = "https://us.battle.net/d3/en/item/devil-tongue",
            RelativeUrl = "/d3/en/item/devil-tongue",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Griswold's Masterpiece 
        /// </summary>
        public static Item GriswoldsMasterpiece = new Item
        {
            Id = 0,
            Name = "Griswold's Masterpiece",
            Quality = ItemQuality.Legendary,
            Slug = "griswolds-masterpiece",
            ItemType = ItemType.Sword,
            BaseType = ItemBaseType.Weapon,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/griswolds-masterpiece",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/griswolds-masterpiece",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/griswolds-masterpiece",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Shard of Hate Elemental skills have a chance to trigger a powerful attack that deals 200–250% weapon damage: Cold skills trigger Freezing Skull Poison skills trigger Poison Nova Lightning skills trigger Charged Bolt
        /// </summary>
        public static Item ShardOfHate = new Item
        {
            Id = 376463,
            Name = "Shard of Hate",
            Quality = ItemQuality.Legendary,
            Slug = "shard-of-hate",
            ItemType = ItemType.Sword,
            BaseType = ItemBaseType.Weapon,
            InternalName = "sword_norm_promo_02",
            DataUrl = "https://us.battle.net/api/d3/data/item/shard-of-hate",
            Url = "https://us.battle.net/d3/en/item/shard-of-hate",
            RelativeUrl = "/d3/en/item/shard-of-hate",
            IsCrafted = false,
            LegendaryAffix = "Elemental skills have a chance to trigger a powerful attack that deals 200–250% weapon damage: Cold skills trigger Freezing Skull Poison skills trigger Poison Nova Lightning skills trigger Charged Bolt",
            SetName = "",
        };

        /// <summary>
        /// Little Rogue 
        /// </summary>
        public static Item LittleRogue = new Item
        {
            Id = 313291,
            Name = "Little Rogue",
            Quality = ItemQuality.Legendary,
            Slug = "little-rogue",
            ItemType = ItemType.Sword,
            BaseType = ItemBaseType.Weapon,
            InternalName = "sword_norm_set_03",
            DataUrl = "https://us.battle.net/api/d3/data/item/little-rogue",
            Url = "https://us.battle.net/d3/en/item/little-rogue",
            RelativeUrl = "/d3/en/item/little-rogue",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Istvan's Paired Blades",
        };

        /// <summary>
        /// The Slanderer 
        /// </summary>
        public static Item TheSlanderer = new Item
        {
            Id = 313290,
            Name = "The Slanderer",
            Quality = ItemQuality.Legendary,
            Slug = "the-slanderer",
            ItemType = ItemType.Sword,
            BaseType = ItemBaseType.Weapon,
            InternalName = "sword_norm_set_02",
            DataUrl = "https://us.battle.net/api/d3/data/item/the-slanderer",
            Url = "https://us.battle.net/d3/en/item/the-slanderer",
            RelativeUrl = "/d3/en/item/the-slanderer",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Istvan's Paired Blades",
        };

        /// <summary>
        /// Griswold's Perfection 
        /// </summary>
        public static Item GriswoldsPerfection = new Item
        {
            Id = 270977,
            Name = "Griswold's Perfection",
            Quality = ItemQuality.Legendary,
            Slug = "griswolds-perfection",
            ItemType = ItemType.Sword,
            BaseType = ItemBaseType.Weapon,
            InternalName = "Sword_norm_unique_10",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/griswolds-perfection",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/griswolds-perfection",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/griswolds-perfection",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Born's Furious Wrath 
        /// </summary>
        public static Item BornsFuriousWrath = new Item
        {
            Id = 223408,
            Name = "Born's Furious Wrath",
            Quality = ItemQuality.Legendary,
            Slug = "borns-furious-wrath",
            ItemType = ItemType.Sword,
            BaseType = ItemBaseType.Weapon,
            InternalName = "Sword_norm_set_01",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/borns-furious-wrath",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/borns-furious-wrath",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/borns-furious-wrath",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "Born's Command",
        };

        /// <summary>
        /// Fjord Cutter 20–30% chance to be surrounded by a Chilling Aura when attacking.
        /// </summary>
        public static Item FjordCutter = new Item
        {
            Id = 192105,
            Name = "Fjord Cutter",
            Quality = ItemQuality.Legendary,
            Slug = "fjord-cutter",
            ItemType = ItemType.MightyWeapon,
            BaseType = ItemBaseType.Weapon,
            InternalName = "mightyWeapon_1H_norm_unique_01",
            DataUrl = "https://us.battle.net/api/d3/data/item/fjord-cutter",
            Url = "https://us.battle.net/d3/en/item/fjord-cutter",
            RelativeUrl = "/d3/en/item/fjord-cutter",
            IsCrafted = false,
            LegendaryAffix = "20–30% chance to be surrounded by a Chilling Aura when attacking.",
            SetName = "",
        };

        /// <summary>
        /// Ambo's Pride 
        /// </summary>
        public static Item AmbosPride = new Item
        {
            Id = 193486,
            Name = "Ambo's Pride",
            Quality = ItemQuality.Legendary,
            Slug = "ambos-pride",
            ItemType = ItemType.MightyWeapon,
            BaseType = ItemBaseType.Weapon,
            InternalName = "mightyweapon_1h_norm_unique_03",
            DataUrl = "https://us.battle.net/api/d3/data/item/ambos-pride",
            Url = "https://us.battle.net/d3/en/item/ambos-pride",
            RelativeUrl = "/d3/en/item/ambos-pride",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Harvest Moon 
        /// </summary>
        public static Item HarvestMoon = new Item
        {
            Id = 0,
            Name = "Harvest Moon",
            Quality = ItemQuality.Legendary,
            Slug = "harvest-moon",
            ItemType = ItemType.MightyWeapon,
            BaseType = ItemBaseType.Weapon,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/harvest-moon",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/harvest-moon",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/harvest-moon",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Blade of the Warlord 
        /// </summary>
        public static Item BladeOfTheWarlord = new Item
        {
            Id = 193611,
            Name = "Blade of the Warlord",
            Quality = ItemQuality.Legendary,
            Slug = "blade-of-the-warlord",
            ItemType = ItemType.MightyWeapon,
            BaseType = ItemBaseType.Weapon,
            InternalName = "mightyweapon_1h_norm_unique_04",
            DataUrl = "https://us.battle.net/api/d3/data/item/blade-of-the-warlord",
            Url = "https://us.battle.net/d3/en/item/blade-of-the-warlord",
            RelativeUrl = "/d3/en/item/blade-of-the-warlord",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Bul-Kathos's Solemn Vow 
        /// </summary>
        public static Item BulkathossSolemnVow = new Item
        {
            Id = 208771,
            Name = "Bul-Kathos's Solemn Vow",
            Quality = ItemQuality.Legendary,
            Slug = "bulkathoss-solemn-vow",
            ItemType = ItemType.MightyWeapon,
            BaseType = ItemBaseType.Weapon,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/item/bulkathoss-solemn-vow",
            Url = "https://us.battle.net/d3/en/item/bulkathoss-solemn-vow",
            RelativeUrl = "/d3/en/item/bulkathoss-solemn-vow",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Bul-Kathos's Oath",
        };

        /// <summary>
        /// Bul-Kathos's Warrior Blood 
        /// </summary>
        public static Item BulkathossWarriorBlood = new Item
        {
            Id = 208775,
            Name = "Bul-Kathos's Warrior Blood",
            Quality = ItemQuality.Legendary,
            Slug = "bulkathoss-warrior-blood",
            ItemType = ItemType.MightyWeapon,
            BaseType = ItemBaseType.Weapon,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/item/bulkathoss-warrior-blood",
            Url = "https://us.battle.net/d3/en/item/bulkathoss-warrior-blood",
            RelativeUrl = "/d3/en/item/bulkathoss-warrior-blood",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Bul-Kathos's Oath",
        };

        /// <summary>
        /// Hallowed Reckoning 
        /// </summary>
        public static Item HallowedReckoning = new Item
        {
            Id = 0,
            Name = "Hallowed Reckoning",
            Quality = ItemQuality.Legendary,
            Slug = "hallowed-destroyer",
            ItemType = ItemType.MightyWeapon,
            BaseType = ItemBaseType.Weapon,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/hallowed-destroyer",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/hallowed-destroyer",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/hallowed-destroyer",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "Hallowed Defenders",
        };

        /// <summary>
        /// Remorseless Hammer of the Ancients has a 25–30% chance to summon an Ancient for 20 seconds.
        /// </summary>
        public static Item Remorseless = new Item
        {
            Id = 0,
            Name = "Remorseless",
            Quality = ItemQuality.Legendary,
            Slug = "remorseless",
            ItemType = ItemType.MightyWeapon,
            BaseType = ItemBaseType.Weapon,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/item/remorseless",
            Url = "https://us.battle.net/d3/en/item/remorseless",
            RelativeUrl = "/d3/en/item/remorseless",
            IsCrafted = false,
            LegendaryAffix = "Hammer of the Ancients has a 25–30% chance to summon an Ancient for 20 seconds.",
            SetName = "",
        };

        /// <summary>
        /// Night's Reaping 
        /// </summary>
        public static Item NightsReaping = new Item
        {
            Id = 0,
            Name = "Night's Reaping",
            Quality = ItemQuality.Legendary,
            Slug = "nights-reaping",
            ItemType = ItemType.MightyWeapon,
            BaseType = ItemBaseType.Weapon,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/nights-reaping",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/nights-reaping",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/nights-reaping",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Hallowed Nemesis 
        /// </summary>
        public static Item HallowedNemesis = new Item
        {
            Id = 0,
            Name = "Hallowed Nemesis",
            Quality = ItemQuality.Legendary,
            Slug = "hallowed-nemesis",
            ItemType = ItemType.MightyWeapon,
            BaseType = ItemBaseType.Weapon,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/hallowed-nemesis",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/hallowed-nemesis",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/hallowed-nemesis",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "Hallowed Protectors",
        };

        /// <summary>
        /// Pledge of Caldeum 
        /// </summary>
        public static Item PledgeOfCaldeum = new Item
        {
            Id = 196570,
            Name = "Pledge of Caldeum",
            Quality = ItemQuality.Legendary,
            Slug = "pledge-of-caldeum",
            ItemType = ItemType.Polearm,
            BaseType = ItemBaseType.Weapon,
            InternalName = "Polearm_norm_unique_04",
            DataUrl = "https://us.battle.net/api/d3/data/item/pledge-of-caldeum",
            Url = "https://us.battle.net/d3/en/item/pledge-of-caldeum",
            RelativeUrl = "/d3/en/item/pledge-of-caldeum",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Standoff 
        /// </summary>
        public static Item Standoff = new Item
        {
            Id = 191570,
            Name = "Standoff",
            Quality = ItemQuality.Legendary,
            Slug = "standoff",
            ItemType = ItemType.Polearm,
            BaseType = ItemBaseType.Weapon,
            InternalName = "Polearm_norm_unique_01",
            DataUrl = "https://us.battle.net/api/d3/data/item/standoff",
            Url = "https://us.battle.net/d3/en/item/standoff",
            RelativeUrl = "/d3/en/item/standoff",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Bovine Bardiche Chance on hit to summon a herd of murderous cows.
        /// </summary>
        public static Item BovineBardiche = new Item
        {
            Id = 272056,
            Name = "Bovine Bardiche",
            Quality = ItemQuality.Legendary,
            Slug = "bovine-bardiche",
            ItemType = ItemType.Polearm,
            BaseType = ItemBaseType.Weapon,
            InternalName = "polearm_norm_unique_05",
            DataUrl = "https://us.battle.net/api/d3/data/item/bovine-bardiche",
            Url = "https://us.battle.net/d3/en/item/bovine-bardiche",
            RelativeUrl = "/d3/en/item/bovine-bardiche",
            IsCrafted = false,
            LegendaryAffix = "Chance on hit to summon a herd of murderous cows.",
            SetName = "",
        };

        /// <summary>
        /// Heart Slaughter 
        /// </summary>
        public static Item HeartSlaughter = new Item
        {
            Id = 192569,
            Name = "Heart Slaughter",
            Quality = ItemQuality.Legendary,
            Slug = "heart-slaughter",
            ItemType = ItemType.Polearm,
            BaseType = ItemBaseType.Weapon,
            InternalName = "polearm_norm_unique_02",
            DataUrl = "https://us.battle.net/api/d3/data/item/heart-slaughter",
            Url = "https://us.battle.net/d3/en/item/heart-slaughter",
            RelativeUrl = "/d3/en/item/heart-slaughter",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Vigilance Getting hit has a chance to automatically cast Inner Sanctuary.
        /// </summary>
        public static Item Vigilance = new Item
        {
            Id = 195491,
            Name = "Vigilance",
            Quality = ItemQuality.Legendary,
            Slug = "vigilance",
            ItemType = ItemType.Polearm,
            BaseType = ItemBaseType.Weapon,
            InternalName = "Polearm_norm_unique_03",
            DataUrl = "https://us.battle.net/api/d3/data/item/vigilance",
            Url = "https://us.battle.net/d3/en/item/vigilance",
            RelativeUrl = "/d3/en/item/vigilance",
            IsCrafted = false,
            LegendaryAffix = "Getting hit has a chance to automatically cast Inner Sanctuary.",
            SetName = "",
        };

        /// <summary>
        /// The Executioner 
        /// </summary>
        public static Item TheExecutioner = new Item
        {
            Id = 186560,
            Name = "The Executioner",
            Quality = ItemQuality.Legendary,
            Slug = "the-executioner",
            ItemType = ItemType.Axe,
            BaseType = ItemBaseType.Weapon,
            InternalName = "twoHandedAxe_norm_unique_02",
            DataUrl = "https://us.battle.net/api/d3/data/item/the-executioner",
            Url = "https://us.battle.net/d3/en/item/the-executioner",
            RelativeUrl = "/d3/en/item/the-executioner",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Butcher's Carver The Butcher still inhabits his carver.
        /// </summary>
        public static Item ButchersCarver = new Item
        {
            Id = 186494,
            Name = "Butcher's Carver",
            Quality = ItemQuality.Legendary,
            Slug = "butchers-carver",
            ItemType = ItemType.Axe,
            BaseType = ItemBaseType.Weapon,
            InternalName = "twoHandedAxe_norm_unique_03",
            DataUrl = "https://us.battle.net/api/d3/data/item/butchers-carver",
            Url = "https://us.battle.net/d3/en/item/butchers-carver",
            RelativeUrl = "/d3/en/item/butchers-carver",
            IsCrafted = false,
            LegendaryAffix = "The Butcher still inhabits his carver.",
            SetName = "",
        };

        /// <summary>
        /// Messerschmidt's Reaver 
        /// </summary>
        public static Item MesserschmidtsReaver = new Item
        {
            Id = 191065,
            Name = "Messerschmidt's Reaver",
            Quality = ItemQuality.Legendary,
            Slug = "messerschmidts-reaver",
            ItemType = ItemType.Axe,
            BaseType = ItemBaseType.Weapon,
            InternalName = "twoHandedAxe_norm_unique_04",
            DataUrl = "https://us.battle.net/api/d3/data/item/messerschmidts-reaver",
            Url = "https://us.battle.net/d3/en/item/messerschmidts-reaver",
            RelativeUrl = "/d3/en/item/messerschmidts-reaver",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Skorn 
        /// </summary>
        public static Item Skorn = new Item
        {
            Id = 192887,
            Name = "Skorn",
            Quality = ItemQuality.Legendary,
            Slug = "skorn",
            ItemType = ItemType.Axe,
            BaseType = ItemBaseType.Weapon,
            InternalName = "twoHandedAxe_norm_unique_05",
            DataUrl = "https://us.battle.net/api/d3/data/item/skorn",
            Url = "https://us.battle.net/d3/en/item/skorn",
            RelativeUrl = "/d3/en/item/skorn",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Fire Brand 25–50% chance to cast a fireball when attacking.
        /// </summary>
        public static Item FireBrand = new Item
        {
            Id = 0,
            Name = "Fire Brand",
            Quality = ItemQuality.Legendary,
            Slug = "fire-brand",
            ItemType = ItemType.Axe,
            BaseType = ItemBaseType.Weapon,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/fire-brand",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/fire-brand",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/fire-brand",
            IsCrafted = true,
            LegendaryAffix = "25–50% chance to cast a fireball when attacking.",
            SetName = "",
        };

        /// <summary>
        /// Cinder Switch 25–50% chance to cast a fireball when attacking.
        /// </summary>
        public static Item CinderSwitch = new Item
        {
            Id = 6329,
            Name = "Cinder Switch",
            Quality = ItemQuality.Legendary,
            Slug = "cinder-switch",
            ItemType = ItemType.Axe,
            BaseType = ItemBaseType.Weapon,
            InternalName = "twoHandedAxe_norm_unique_01",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/cinder-switch",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/cinder-switch",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/cinder-switch",
            IsCrafted = true,
            LegendaryAffix = "25–50% chance to cast a fireball when attacking.",
            SetName = "",
        };

        /// <summary>
        /// Baleful Remnant Enemies killed while Akarat's Champion is active turn into Phalanx Avatars for 10 seconds.
        /// </summary>
        public static Item BalefulRemnant = new Item
        {
            Id = 299435,
            Name = "Baleful Remnant",
            Quality = ItemQuality.Legendary,
            Slug = "baleful-remnant",
            ItemType = ItemType.Flail,
            BaseType = ItemBaseType.Weapon,
            InternalName = "flail2h_norm_unique_02",
            DataUrl = "https://us.battle.net/api/d3/data/item/baleful-remnant",
            Url = "https://us.battle.net/d3/en/item/baleful-remnant",
            RelativeUrl = "/d3/en/item/baleful-remnant",
            IsCrafted = false,
            LegendaryAffix = "Enemies killed while Akarat's Champion is active turn into Phalanx Avatars for 10 seconds.",
            SetName = "",
        };

        /// <summary>
        /// Fate of the Fell Gain two additional rays of Heaven’s Fury.
        /// </summary>
        public static Item FateOfTheFell = new Item
        {
            Id = 299436,
            Name = "Fate of the Fell",
            Quality = ItemQuality.Legendary,
            Slug = "fate-of-the-fell",
            ItemType = ItemType.Flail,
            BaseType = ItemBaseType.Weapon,
            InternalName = "flail2h_norm_unique_03",
            DataUrl = "https://us.battle.net/api/d3/data/item/fate-of-the-fell",
            Url = "https://us.battle.net/d3/en/item/fate-of-the-fell",
            RelativeUrl = "/d3/en/item/fate-of-the-fell",
            IsCrafted = false,
            LegendaryAffix = "Gain two additional rays of Heaven’s Fury.",
            SetName = "",
        };

        /// <summary>
        /// Golden Flense Sweep Attack restores 3–4 Wrath for each enemy hit.
        /// </summary>
        public static Item GoldenFlense = new Item
        {
            Id = 299437,
            Name = "Golden Flense",
            Quality = ItemQuality.Legendary,
            Slug = "golden-flense",
            ItemType = ItemType.Flail,
            BaseType = ItemBaseType.Weapon,
            InternalName = "flail2h_norm_unique_04",
            DataUrl = "https://us.battle.net/api/d3/data/item/golden-flense",
            Url = "https://us.battle.net/d3/en/item/golden-flense",
            RelativeUrl = "/d3/en/item/golden-flense",
            IsCrafted = false,
            LegendaryAffix = "Sweep Attack restores 3–4 Wrath for each enemy hit.",
            SetName = "",
        };

        /// <summary>
        /// The Mortal Drama Double the number of Bombardment impacts.
        /// </summary>
        public static Item TheMortalDrama = new Item
        {
            Id = 299431,
            Name = "The Mortal Drama",
            Quality = ItemQuality.Legendary,
            Slug = "the-mortal-drama",
            ItemType = ItemType.Flail,
            BaseType = ItemBaseType.Weapon,
            InternalName = "flail2h_norm_unique_01",
            DataUrl = "https://us.battle.net/api/d3/data/item/the-mortal-drama",
            Url = "https://us.battle.net/d3/en/item/the-mortal-drama",
            RelativeUrl = "/d3/en/item/the-mortal-drama",
            IsCrafted = false,
            LegendaryAffix = "Double the number of Bombardment impacts.",
            SetName = "",
        };

        /// <summary>
        /// Balance 
        /// </summary>
        public static Item Balance = new Item
        {
            Id = 195145,
            Name = "Balance",
            Quality = ItemQuality.Legendary,
            Slug = "balance",
            ItemType = ItemType.Daibo,
            BaseType = ItemBaseType.Weapon,
            InternalName = "combatstaff_norm_unique_04",
            DataUrl = "https://us.battle.net/api/d3/data/item/balance",
            Url = "https://us.battle.net/d3/en/item/balance",
            RelativeUrl = "/d3/en/item/balance",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Lai Yui's Taiji 
        /// </summary>
        public static Item LaiYuisTaiji = new Item
        {
            Id = 0,
            Name = "Lai Yui's Taiji",
            Quality = ItemQuality.Legendary,
            Slug = "lai-yuis-taiji",
            ItemType = ItemType.Daibo,
            BaseType = ItemBaseType.Weapon,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/lai-yuis-taiji",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/lai-yuis-taiji",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/lai-yuis-taiji",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// The Flow of Eternity Reduces the cooldown of Seven-Sided Strike by 45–60%.
        /// </summary>
        public static Item TheFlowOfEternity = new Item
        {
            Id = 197072,
            Name = "The Flow of Eternity",
            Quality = ItemQuality.Legendary,
            Slug = "the-flow-of-eternity",
            ItemType = ItemType.Daibo,
            BaseType = ItemBaseType.Weapon,
            InternalName = "combatStaff_norm_unique_06",
            DataUrl = "https://us.battle.net/api/d3/data/item/the-flow-of-eternity",
            Url = "https://us.battle.net/d3/en/item/the-flow-of-eternity",
            RelativeUrl = "/d3/en/item/the-flow-of-eternity",
            IsCrafted = false,
            LegendaryAffix = "Reduces the cooldown of Seven-Sided Strike by 45–60%.",
            SetName = "",
        };

        /// <summary>
        /// The Paddle Slap!
        /// </summary>
        public static Item ThePaddle = new Item
        {
            Id = 197068,
            Name = "The Paddle",
            Quality = ItemQuality.Legendary,
            Slug = "the-paddle",
            ItemType = ItemType.Daibo,
            BaseType = ItemBaseType.Weapon,
            InternalName = "combatStaff_norm_unique_03",
            DataUrl = "https://us.battle.net/api/d3/data/item/the-paddle",
            Url = "https://us.battle.net/d3/en/item/the-paddle",
            RelativeUrl = "/d3/en/item/the-paddle",
            IsCrafted = false,
            LegendaryAffix = "Slap!",
            SetName = "",
        };

        /// <summary>
        /// Staff of Kyro 
        /// </summary>
        public static Item StaffOfKyro = new Item
        {
            Id = 271749,
            Name = "Staff of Kyro",
            Quality = ItemQuality.Legendary,
            Slug = "staff-of-kyro",
            ItemType = ItemType.Daibo,
            BaseType = ItemBaseType.Weapon,
            InternalName = "combatstaff_norm_unique_09",
            DataUrl = "https://us.battle.net/api/d3/data/item/staff-of-kyro",
            Url = "https://us.battle.net/d3/en/item/staff-of-kyro",
            RelativeUrl = "/d3/en/item/staff-of-kyro",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Warstaff of General Quang Tempest Rush gains the effect of the Tailwind rune.
        /// </summary>
        public static Item WarstaffOfGeneralQuang = new Item
        {
            Id = 271765,
            Name = "Warstaff of General Quang",
            Quality = ItemQuality.Legendary,
            Slug = "warstaff-of-general-quang",
            ItemType = ItemType.Daibo,
            BaseType = ItemBaseType.Weapon,
            InternalName = "combatstaff_norm_unique_10",
            DataUrl = "https://us.battle.net/api/d3/data/item/warstaff-of-general-quang",
            Url = "https://us.battle.net/d3/en/item/warstaff-of-general-quang",
            RelativeUrl = "/d3/en/item/warstaff-of-general-quang",
            IsCrafted = false,
            LegendaryAffix = "Tempest Rush gains the effect of the Tailwind rune.",
            SetName = "",
        };

        /// <summary>
        /// Incense Torch of the Grand Temple Reduces the Spirit cost of Wave of Light by 40–50%.
        /// </summary>
        public static Item IncenseTorchOfTheGrandTemple = new Item
        {
            Id = 192342,
            Name = "Incense Torch of the Grand Temple",
            Quality = ItemQuality.Legendary,
            Slug = "incense-torch-of-the-grand-temple",
            ItemType = ItemType.Daibo,
            BaseType = ItemBaseType.Weapon,
            InternalName = "combatStaff_norm_unique_01",
            DataUrl = "https://us.battle.net/api/d3/data/item/incense-torch-of-the-grand-temple",
            Url = "https://us.battle.net/d3/en/item/incense-torch-of-the-grand-temple",
            RelativeUrl = "/d3/en/item/incense-torch-of-the-grand-temple",
            IsCrafted = false,
            LegendaryAffix = "Reduces the Spirit cost of Wave of Light by 40–50%.",
            SetName = "",
        };

        /// <summary>
        /// Flying Dragon Chance to double your attack speed when attacking.
        /// </summary>
        public static Item FlyingDragon = new Item
        {
            Id = 197065,
            Name = "Flying Dragon",
            Quality = ItemQuality.Legendary,
            Slug = "flying-dragon",
            ItemType = ItemType.Daibo,
            BaseType = ItemBaseType.Weapon,
            InternalName = "combatStaff_norm_unique_02",
            DataUrl = "https://us.battle.net/api/d3/data/item/flying-dragon",
            Url = "https://us.battle.net/d3/en/item/flying-dragon",
            RelativeUrl = "/d3/en/item/flying-dragon",
            IsCrafted = false,
            LegendaryAffix = "Chance to double your attack speed when attacking.",
            SetName = "",
        };

        /// <summary>
        /// Inna's Reach 
        /// </summary>
        public static Item InnasReach = new Item
        {
            Id = 212208,
            Name = "Inna's Reach",
            Quality = ItemQuality.Legendary,
            Slug = "innas-reach",
            ItemType = ItemType.Daibo,
            BaseType = ItemBaseType.Weapon,
            InternalName = "combatStaff_norm_unique_08",
            DataUrl = "https://us.battle.net/api/d3/data/item/innas-reach",
            Url = "https://us.battle.net/d3/en/item/innas-reach",
            RelativeUrl = "/d3/en/item/innas-reach",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Inna's Mantra",
        };

        /// <summary>
        /// Rozpedin's Staff 
        /// </summary>
        public static Item RozpedinsStaff = new Item
        {
            Id = 0,
            Name = "Rozpedin's Staff",
            Quality = ItemQuality.Legendary,
            Slug = "rozpedins-staff",
            ItemType = ItemType.Daibo,
            BaseType = ItemBaseType.Weapon,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/rozpedins-staff",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/rozpedins-staff",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/rozpedins-staff",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Lai Yui's Persuader 
        /// </summary>
        public static Item LaiYuisPersuader = new Item
        {
            Id = 0,
            Name = "Lai Yui's Persuader",
            Quality = ItemQuality.Legendary,
            Slug = "lai-yuis-persuader",
            ItemType = ItemType.Daibo,
            BaseType = ItemBaseType.Weapon,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/lai-yuis-persuader",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/lai-yuis-persuader",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/lai-yuis-persuader",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Rozpedin's Force 
        /// </summary>
        public static Item RozpedinsForce = new Item
        {
            Id = 0,
            Name = "Rozpedin's Force",
            Quality = ItemQuality.Legendary,
            Slug = "rozpedins-force",
            ItemType = ItemType.Daibo,
            BaseType = ItemBaseType.Weapon,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/rozpedins-force",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/rozpedins-force",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/rozpedins-force",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Hellish Staff of Herding 
        /// </summary>
        public static Item HellishStaffOfHerding = new Item
        {
            Id = 0,
            Name = "Hellish Staff of Herding",
            Quality = ItemQuality.Legendary,
            Slug = "hellish-staff-of-herding",
            ItemType = ItemType.Staff,
            BaseType = ItemBaseType.Weapon,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/hellish-staff-of-herding",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/hellish-staff-of-herding",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/hellish-staff-of-herding",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Infernal Staff of Herding 
        /// </summary>
        public static Item InfernalStaffOfHerding = new Item
        {
            Id = 367201,
            Name = "Infernal Staff of Herding",
            Quality = ItemQuality.Legendary,
            Slug = "infernal-staff-of-herding",
            ItemType = ItemType.Staff,
            BaseType = ItemBaseType.Weapon,
            InternalName = "wizardhat_norm_base_02",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/infernal-staff-of-herding",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/infernal-staff-of-herding",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/infernal-staff-of-herding",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Nightmarish Staff of Herding 
        /// </summary>
        public static Item NightmarishStaffOfHerding = new Item
        {
            Id = 0,
            Name = "Nightmarish Staff of Herding",
            Quality = ItemQuality.Legendary,
            Slug = "nightmarish-staff-of-herding",
            ItemType = ItemType.Staff,
            BaseType = ItemBaseType.Weapon,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/nightmarish-staff-of-herding",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/nightmarish-staff-of-herding",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/nightmarish-staff-of-herding",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Autumn's Call 
        /// </summary>
        public static Item AutumnsCall = new Item
        {
            Id = 184228,
            Name = "Autumn's Call",
            Quality = ItemQuality.Legendary,
            Slug = "autumns-call",
            ItemType = ItemType.Staff,
            BaseType = ItemBaseType.Weapon,
            InternalName = "staff_norm_unique_03",
            DataUrl = "https://us.battle.net/api/d3/data/item/autumns-call",
            Url = "https://us.battle.net/d3/en/item/autumns-call",
            RelativeUrl = "/d3/en/item/autumns-call",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// The Broken Staff 
        /// </summary>
        public static Item TheBrokenStaff = new Item
        {
            Id = 59601,
            Name = "The Broken Staff",
            Quality = ItemQuality.Legendary,
            Slug = "the-broken-staff",
            ItemType = ItemType.Staff,
            BaseType = ItemBaseType.Weapon,
            InternalName = "staff_norm_unique_01",
            DataUrl = "https://us.battle.net/api/d3/data/item/the-broken-staff",
            Url = "https://us.battle.net/d3/en/item/the-broken-staff",
            RelativeUrl = "/d3/en/item/the-broken-staff",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Ahavarion, Spear of Lycander Chance on killing a demon to gain a random Shrine effect.
        /// </summary>
        public static Item AhavarionSpearOfLycander = new Item
        {
            Id = 271768,
            Name = "Ahavarion, Spear of Lycander",
            Quality = ItemQuality.Legendary,
            Slug = "ahavarion-spear-of-lycander",
            ItemType = ItemType.Staff,
            BaseType = ItemBaseType.Weapon,
            InternalName = "staff_norm_unique_08",
            DataUrl = "https://us.battle.net/api/d3/data/item/ahavarion-spear-of-lycander",
            Url = "https://us.battle.net/d3/en/item/ahavarion-spear-of-lycander",
            RelativeUrl = "/d3/en/item/ahavarion-spear-of-lycander",
            IsCrafted = false,
            LegendaryAffix = "Chance on killing a demon to gain a random Shrine effect.",
            SetName = "",
        };

        /// <summary>
        /// The Smoldering Core Lesser enemies are now lured to your Meteor impact areas.
        /// </summary>
        public static Item TheSmolderingCore = new Item
        {
            Id = 271774,
            Name = "The Smoldering Core",
            Quality = ItemQuality.Legendary,
            Slug = "the-smoldering-core",
            ItemType = ItemType.Staff,
            BaseType = ItemBaseType.Weapon,
            InternalName = "staff_norm_unique_10",
            DataUrl = "https://us.battle.net/api/d3/data/item/the-smoldering-core",
            Url = "https://us.battle.net/d3/en/item/the-smoldering-core",
            RelativeUrl = "/d3/en/item/the-smoldering-core",
            IsCrafted = false,
            LegendaryAffix = "Lesser enemies are now lured to your Meteor impact areas.",
            SetName = "",
        };

        /// <summary>
        /// Valthek's Rebuke Energy Twister now travels in a straight path.
        /// </summary>
        public static Item ValtheksRebuke = new Item
        {
            Id = 271773,
            Name = "Valthek's Rebuke",
            Quality = ItemQuality.Legendary,
            Slug = "valtheks-rebuke",
            ItemType = ItemType.Staff,
            BaseType = ItemBaseType.Weapon,
            InternalName = "staff_norm_unique_09",
            DataUrl = "https://us.battle.net/api/d3/data/item/valtheks-rebuke",
            Url = "https://us.battle.net/d3/en/item/valtheks-rebuke",
            RelativeUrl = "/d3/en/item/valtheks-rebuke",
            IsCrafted = false,
            LegendaryAffix = "Energy Twister now travels in a straight path.",
            SetName = "",
        };

        /// <summary>
        /// The Magi 
        /// </summary>
        public static Item TheMagi = new Item
        {
            Id = 0,
            Name = "The Magi",
            Quality = ItemQuality.Legendary,
            Slug = "the-magi",
            ItemType = ItemType.Staff,
            BaseType = ItemBaseType.Weapon,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/the-magi",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/the-magi",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/the-magi",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Maloth's Focus Enemies occasionally flee at the sight of this staff.
        /// </summary>
        public static Item MalothsFocus = new Item
        {
            Id = 193832,
            Name = "Maloth's Focus",
            Quality = ItemQuality.Legendary,
            Slug = "maloths-focus",
            ItemType = ItemType.Staff,
            BaseType = ItemBaseType.Weapon,
            InternalName = "Staff_norm_unique_06",
            DataUrl = "https://us.battle.net/api/d3/data/item/maloths-focus",
            Url = "https://us.battle.net/d3/en/item/maloths-focus",
            RelativeUrl = "/d3/en/item/maloths-focus",
            IsCrafted = false,
            LegendaryAffix = "Enemies occasionally flee at the sight of this staff.",
            SetName = "",
        };

        /// <summary>
        /// Wormwood 
        /// </summary>
        public static Item Wormwood = new Item
        {
            Id = 195407,
            Name = "Wormwood",
            Quality = ItemQuality.Legendary,
            Slug = "wormwood",
            ItemType = ItemType.Staff,
            BaseType = ItemBaseType.Weapon,
            InternalName = "Staff_norm_unique_07",
            DataUrl = "https://us.battle.net/api/d3/data/item/wormwood",
            Url = "https://us.battle.net/d3/en/item/wormwood",
            RelativeUrl = "/d3/en/item/wormwood",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// The Grand Vizier Reduces the Arcane Power cost of Meteor by 40–50%.
        /// </summary>
        public static Item TheGrandVizier = new Item
        {
            Id = 192167,
            Name = "The Grand Vizier",
            Quality = ItemQuality.Legendary,
            Slug = "the-grand-vizier",
            ItemType = ItemType.Staff,
            BaseType = ItemBaseType.Weapon,
            InternalName = "Staff_norm_unique_04",
            DataUrl = "https://us.battle.net/api/d3/data/item/the-grand-vizier",
            Url = "https://us.battle.net/d3/en/item/the-grand-vizier",
            RelativeUrl = "/d3/en/item/the-grand-vizier",
            IsCrafted = false,
            LegendaryAffix = "Reduces the Arcane Power cost of Meteor by 40–50%.",
            SetName = "",
        };

        /// <summary>
        /// The Tormentor Chance to charm enemies when you hit them.
        /// </summary>
        public static Item TheTormentor = new Item
        {
            Id = 193066,
            Name = "The Tormentor",
            Quality = ItemQuality.Legendary,
            Slug = "the-tormentor",
            ItemType = ItemType.Staff,
            BaseType = ItemBaseType.Weapon,
            InternalName = "staff_norm_unique_05",
            DataUrl = "https://us.battle.net/api/d3/data/item/the-tormentor",
            Url = "https://us.battle.net/d3/en/item/the-tormentor",
            RelativeUrl = "/d3/en/item/the-tormentor",
            IsCrafted = false,
            LegendaryAffix = "Chance to charm enemies when you hit them.",
            SetName = "",
        };

        /// <summary>
        /// Mark of The Magi 
        /// </summary>
        public static Item MarkOfTheMagi = new Item
        {
            Id = 59612,
            Name = "Mark of The Magi",
            Quality = ItemQuality.Legendary,
            Slug = "mark-of-the-magi",
            ItemType = ItemType.Staff,
            BaseType = ItemBaseType.Weapon,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/mark-of-the-magi",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/mark-of-the-magi",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/mark-of-the-magi",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Faithful Memory 
        /// </summary>
        public static Item FaithfulMemory = new Item
        {
            Id = 198960,
            Name = "Faithful Memory",
            Quality = ItemQuality.Legendary,
            Slug = "faithful-memory",
            ItemType = ItemType.Sword,
            BaseType = ItemBaseType.Weapon,
            InternalName = "twoHandedSword_norm_unique_09",
            DataUrl = "https://us.battle.net/api/d3/data/item/faithful-memory",
            Url = "https://us.battle.net/d3/en/item/faithful-memory",
            RelativeUrl = "/d3/en/item/faithful-memory",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// The Zweihander 
        /// </summary>
        public static Item TheZweihander = new Item
        {
            Id = 59665,
            Name = "The Zweihander",
            Quality = ItemQuality.Legendary,
            Slug = "the-zweihander",
            ItemType = ItemType.Sword,
            BaseType = ItemBaseType.Weapon,
            InternalName = "twohandedsword_norm_unique_01",
            DataUrl = "https://us.battle.net/api/d3/data/item/the-zweihander",
            Url = "https://us.battle.net/d3/en/item/the-zweihander",
            RelativeUrl = "/d3/en/item/the-zweihander",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Blackguard 
        /// </summary>
        public static Item Blackguard = new Item
        {
            Id = 270979,
            Name = "Blackguard",
            Quality = ItemQuality.Legendary,
            Slug = "blackguard",
            ItemType = ItemType.Sword,
            BaseType = ItemBaseType.Weapon,
            InternalName = "twoHandedSword_norm_unique_10",
            DataUrl = "https://us.battle.net/api/d3/data/item/blackguard",
            Url = "https://us.battle.net/d3/en/item/blackguard",
            RelativeUrl = "/d3/en/item/blackguard",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Scourge 20–45% chance to explode with demonic fury when attacking.
        /// </summary>
        public static Item Scourge = new Item
        {
            Id = 181511,
            Name = "Scourge",
            Quality = ItemQuality.Legendary,
            Slug = "scourge",
            ItemType = ItemType.Sword,
            BaseType = ItemBaseType.Weapon,
            InternalName = "twoHandedSword_norm_unique_06",
            DataUrl = "https://us.battle.net/api/d3/data/item/scourge",
            Url = "https://us.battle.net/d3/en/item/scourge",
            RelativeUrl = "/d3/en/item/scourge",
            IsCrafted = false,
            LegendaryAffix = "20–45% chance to explode with demonic fury when attacking.",
            SetName = "",
        };

        /// <summary>
        /// Stalgard's Decimator Your melee attacks throw a piercing axe at a nearby enemy, dealing 550–700% weapon damage as Physical.
        /// </summary>
        public static Item StalgardsDecimator = new Item
        {
            Id = 271639,
            Name = "Stalgard's Decimator",
            Quality = ItemQuality.Legendary,
            Slug = "stalgards-decimator",
            ItemType = ItemType.Sword,
            BaseType = ItemBaseType.Weapon,
            InternalName = "twohandedsword_norm_unique_11",
            DataUrl = "https://us.battle.net/api/d3/data/item/stalgards-decimator",
            Url = "https://us.battle.net/d3/en/item/stalgards-decimator",
            RelativeUrl = "/d3/en/item/stalgards-decimator",
            IsCrafted = false,
            LegendaryAffix = "Your melee attacks throw a piercing axe at a nearby enemy, dealing 550–700% weapon damage as Physical.",
            SetName = "",
        };

        /// <summary>
        /// Blade of Prophecy Condemned enemies also trigger Condemn's explosion.
        /// </summary>
        public static Item BladeOfProphecy = new Item
        {
            Id = 184184,
            Name = "Blade of Prophecy",
            Quality = ItemQuality.Legendary,
            Slug = "blade-of-prophecy",
            ItemType = ItemType.Sword,
            BaseType = ItemBaseType.Weapon,
            InternalName = "twoHandedSword_norm_unique_03",
            DataUrl = "https://us.battle.net/api/d3/data/item/blade-of-prophecy",
            Url = "https://us.battle.net/d3/en/item/blade-of-prophecy",
            RelativeUrl = "/d3/en/item/blade-of-prophecy",
            IsCrafted = false,
            LegendaryAffix = "Condemned enemies also trigger Condemn's explosion.",
            SetName = "",
        };

        /// <summary>
        /// The Sultan of Blinding Sand 
        /// </summary>
        public static Item TheSultanOfBlindingSand = new Item
        {
            Id = 184190,
            Name = "The Sultan of Blinding Sand",
            Quality = ItemQuality.Legendary,
            Slug = "the-sultan-of-blinding-sand",
            ItemType = ItemType.Sword,
            BaseType = ItemBaseType.Weapon,
            InternalName = "twoHandedSword_norm_unique_05",
            DataUrl = "https://us.battle.net/api/d3/data/item/the-sultan-of-blinding-sand",
            Url = "https://us.battle.net/d3/en/item/the-sultan-of-blinding-sand",
            RelativeUrl = "/d3/en/item/the-sultan-of-blinding-sand",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Maximus Chance on hit to summon a Demonic Slave.
        /// </summary>
        public static Item Maximus = new Item
        {
            Id = 184187,
            Name = "Maximus",
            Quality = ItemQuality.Legendary,
            Slug = "maximus",
            ItemType = ItemType.Sword,
            BaseType = ItemBaseType.Weapon,
            InternalName = "twoHandedSword_norm_unique_04",
            DataUrl = "https://us.battle.net/api/d3/data/item/maximus",
            Url = "https://us.battle.net/d3/en/item/maximus",
            RelativeUrl = "/d3/en/item/maximus",
            IsCrafted = false,
            LegendaryAffix = "Chance on hit to summon a Demonic Slave.",
            SetName = "",
        };

        /// <summary>
        /// The Grandfather 
        /// </summary>
        public static Item TheGrandfather = new Item
        {
            Id = 190360,
            Name = "The Grandfather",
            Quality = ItemQuality.Legendary,
            Slug = "the-grandfather",
            ItemType = ItemType.Sword,
            BaseType = ItemBaseType.Weapon,
            InternalName = "twohandedsword_norm_unique_08",
            DataUrl = "https://us.battle.net/api/d3/data/item/the-grandfather",
            Url = "https://us.battle.net/d3/en/item/the-grandfather",
            RelativeUrl = "/d3/en/item/the-grandfather",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Warmonger 
        /// </summary>
        public static Item Warmonger = new Item
        {
            Id = 181495,
            Name = "Warmonger",
            Quality = ItemQuality.Legendary,
            Slug = "warmonger",
            ItemType = ItemType.Sword,
            BaseType = ItemBaseType.Weapon,
            InternalName = "twoHandedSword_norm_unique_07",
            DataUrl = "https://us.battle.net/api/d3/data/item/warmonger",
            Url = "https://us.battle.net/d3/en/item/warmonger",
            RelativeUrl = "/d3/en/item/warmonger",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Cam's Rebuttal Falling Sword can be used again within 4 seconds before the cooldown is triggered.
        /// </summary>
        public static Item CamsRebuttal = new Item
        {
            Id = 271644,
            Name = "Cam's Rebuttal",
            Quality = ItemQuality.Legendary,
            Slug = "cams-rebuttal",
            ItemType = ItemType.Sword,
            BaseType = ItemBaseType.Weapon,
            InternalName = "twohandedsword_norm_unique_12",
            DataUrl = "https://us.battle.net/api/d3/data/item/cams-rebuttal",
            Url = "https://us.battle.net/d3/en/item/cams-rebuttal",
            RelativeUrl = "/d3/en/item/cams-rebuttal",
            IsCrafted = false,
            LegendaryAffix = "Falling Sword can be used again within 4 seconds before the cooldown is triggered.",
            SetName = "",
        };

        /// <summary>
        /// Unending War 
        /// </summary>
        public static Item UnendingWar = new Item
        {
            Id = 0,
            Name = "Unending War",
            Quality = ItemQuality.Legendary,
            Slug = "unending-war",
            ItemType = ItemType.MightyWeapon,
            BaseType = ItemBaseType.Weapon,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/unending-war",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/unending-war",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/unending-war",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Bastion's Revered Frenzy now stacks up to 10 times.
        /// </summary>
        public static Item BastionsRevered = new Item
        {
            Id = 195690,
            Name = "Bastion's Revered",
            Quality = ItemQuality.Legendary,
            Slug = "bastions-revered",
            ItemType = ItemType.MightyWeapon,
            BaseType = ItemBaseType.Weapon,
            InternalName = "mightyWeapon_2H_norm_unique_03",
            DataUrl = "https://us.battle.net/api/d3/data/item/bastions-revered",
            Url = "https://us.battle.net/d3/en/item/bastions-revered",
            RelativeUrl = "/d3/en/item/bastions-revered",
            IsCrafted = false,
            LegendaryAffix = "Frenzy now stacks up to 10 times.",
            SetName = "",
        };

        /// <summary>
        /// Fury of the Vanished Peak Reduces the Fury cost of Seismic Slam by 40–50%.
        /// </summary>
        public static Item FuryOfTheVanishedPeak = new Item
        {
            Id = 0,
            Name = "Fury of the Vanished Peak",
            Quality = ItemQuality.Legendary,
            Slug = "fury-of-the-vanished-peak",
            ItemType = ItemType.MightyWeapon,
            BaseType = ItemBaseType.Weapon,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/item/fury-of-the-vanished-peak",
            Url = "https://us.battle.net/d3/en/item/fury-of-the-vanished-peak",
            RelativeUrl = "/d3/en/item/fury-of-the-vanished-peak",
            IsCrafted = false,
            LegendaryAffix = "Reduces the Fury cost of Seismic Slam by 40–50%.",
            SetName = "",
        };

        /// <summary>
        /// Madawc's Sorrow Stun enemies for 2 seconds the first time you hit them.
        /// </summary>
        public static Item MadawcsSorrow = new Item
        {
            Id = 272012,
            Name = "Madawc's Sorrow",
            Quality = ItemQuality.Legendary,
            Slug = "madawcs-sorrow",
            ItemType = ItemType.MightyWeapon,
            BaseType = ItemBaseType.Weapon,
            InternalName = "mightyweapon_2h_norm_unique_11",
            DataUrl = "https://us.battle.net/api/d3/data/item/madawcs-sorrow",
            Url = "https://us.battle.net/d3/en/item/madawcs-sorrow",
            RelativeUrl = "/d3/en/item/madawcs-sorrow",
            IsCrafted = false,
            LegendaryAffix = "Stun enemies for 2 seconds the first time you hit them.",
            SetName = "",
        };

        /// <summary>
        /// The Gavel of Judgment 
        /// </summary>
        public static Item TheGavelOfJudgment = new Item
        {
            Id = 193657,
            Name = "The Gavel of Judgment",
            Quality = ItemQuality.Legendary,
            Slug = "the-gavel-of-judgment",
            ItemType = ItemType.MightyWeapon,
            BaseType = ItemBaseType.Weapon,
            InternalName = "mightyWeapon_2H_norm_unique_01",
            DataUrl = "https://us.battle.net/api/d3/data/item/the-gavel-of-judgment",
            Url = "https://us.battle.net/d3/en/item/the-gavel-of-judgment",
            RelativeUrl = "/d3/en/item/the-gavel-of-judgment",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Immortal King's Boulder Breaker 
        /// </summary>
        public static Item ImmortalKingsBoulderBreaker = new Item
        {
            Id = 210678,
            Name = "Immortal King's Boulder Breaker",
            Quality = ItemQuality.Legendary,
            Slug = "immortal-kings-boulder-breaker",
            ItemType = ItemType.MightyWeapon,
            BaseType = ItemBaseType.Weapon,
            InternalName = "mightyWeapon_2H_norm_unique_10",
            DataUrl = "https://us.battle.net/api/d3/data/item/immortal-kings-boulder-breaker",
            Url = "https://us.battle.net/d3/en/item/immortal-kings-boulder-breaker",
            RelativeUrl = "/d3/en/item/immortal-kings-boulder-breaker",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Immortal King's Call",
        };

        /// <summary>
        /// War of the Dead 
        /// </summary>
        public static Item WarOfTheDead = new Item
        {
            Id = 0,
            Name = "War of the Dead",
            Quality = ItemQuality.Legendary,
            Slug = "war-of-the-dead",
            ItemType = ItemType.MightyWeapon,
            BaseType = ItemBaseType.Weapon,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/war-of-the-dead",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/war-of-the-dead",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/war-of-the-dead",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Arthef's Spark of Life Heal for 3–4% of your missing Life when you kill an Undead enemy.
        /// </summary>
        public static Item ArthefsSparkOfLife = new Item
        {
            Id = 59633,
            Name = "Arthef's Spark of Life",
            Quality = ItemQuality.Legendary,
            Slug = "arthefs-spark-of-life",
            ItemType = ItemType.Mace,
            BaseType = ItemBaseType.Weapon,
            InternalName = "twoHandedMace_norm_unique_01",
            DataUrl = "https://us.battle.net/api/d3/data/item/arthefs-spark-of-life",
            Url = "https://us.battle.net/d3/en/item/arthefs-spark-of-life",
            RelativeUrl = "/d3/en/item/arthefs-spark-of-life",
            IsCrafted = false,
            LegendaryAffix = "Heal for 3–4% of your missing Life when you kill an Undead enemy.",
            SetName = "",
        };

        /// <summary>
        /// Crushbane 
        /// </summary>
        public static Item Crushbane = new Item
        {
            Id = 99227,
            Name = "Crushbane",
            Quality = ItemQuality.Legendary,
            Slug = "crushbane",
            ItemType = ItemType.Mace,
            BaseType = ItemBaseType.Weapon,
            InternalName = "twoHandedMace_norm_unique_02",
            DataUrl = "https://us.battle.net/api/d3/data/item/crushbane",
            Url = "https://us.battle.net/d3/en/item/crushbane",
            RelativeUrl = "/d3/en/item/crushbane",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Skywarden Every 60 seconds, gain a random Law for 60 seconds.
        /// </summary>
        public static Item Skywarden = new Item
        {
            Id = 190840,
            Name = "Skywarden",
            Quality = ItemQuality.Legendary,
            Slug = "skywarden",
            ItemType = ItemType.Mace,
            BaseType = ItemBaseType.Weapon,
            InternalName = "twohandedmace_norm_unique_03",
            DataUrl = "https://us.battle.net/api/d3/data/item/skywarden",
            Url = "https://us.battle.net/d3/en/item/skywarden",
            RelativeUrl = "/d3/en/item/skywarden",
            IsCrafted = false,
            LegendaryAffix = "Every 60 seconds, gain a random Law for 60 seconds.",
            SetName = "",
        };

        /// <summary>
        /// Wrath of the Bone King 
        /// </summary>
        public static Item WrathOfTheBoneKing = new Item
        {
            Id = 191584,
            Name = "Wrath of the Bone King",
            Quality = ItemQuality.Legendary,
            Slug = "wrath-of-the-bone-king",
            ItemType = ItemType.Mace,
            BaseType = ItemBaseType.Weapon,
            InternalName = "twoHandedMace_norm_unique_06",
            DataUrl = "https://us.battle.net/api/d3/data/item/wrath-of-the-bone-king",
            Url = "https://us.battle.net/d3/en/item/wrath-of-the-bone-king",
            RelativeUrl = "/d3/en/item/wrath-of-the-bone-king",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// The Furnace Increases damage against elites by 40–50%.
        /// </summary>
        public static Item TheFurnace = new Item
        {
            Id = 271666,
            Name = "The Furnace",
            Quality = ItemQuality.Legendary,
            Slug = "the-furnace",
            ItemType = ItemType.Mace,
            BaseType = ItemBaseType.Weapon,
            InternalName = "twohandedmace_norm_unique_08",
            DataUrl = "https://us.battle.net/api/d3/data/item/the-furnace",
            Url = "https://us.battle.net/d3/en/item/the-furnace",
            RelativeUrl = "/d3/en/item/the-furnace",
            IsCrafted = false,
            LegendaryAffix = "Increases damage against elites by 40–50%.",
            SetName = "",
        };

        /// <summary>
        /// Cataclysm 25–50% chance to sunder the ground your enemies walk on when you attack.
        /// </summary>
        public static Item Cataclysm = new Item
        {
            Id = 0,
            Name = "Cataclysm",
            Quality = ItemQuality.Legendary,
            Slug = "cataclysm",
            ItemType = ItemType.Mace,
            BaseType = ItemBaseType.Weapon,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/cataclysm",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/cataclysm",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/cataclysm",
            IsCrafted = true,
            LegendaryAffix = "25–50% chance to sunder the ground your enemies walk on when you attack.",
            SetName = "",
        };

        /// <summary>
        /// Schaefer's Hammer Getting hit has a chance to charge you with Lightning, causing you to deal 190–250% weapon damage as Lightning every second for 5 seconds to nearby enemies.
        /// </summary>
        public static Item SchaefersHammer = new Item
        {
            Id = 197717,
            Name = "Schaefer's Hammer",
            Quality = ItemQuality.Legendary,
            Slug = "schaefers-hammer",
            ItemType = ItemType.Mace,
            BaseType = ItemBaseType.Weapon,
            InternalName = "twoHandedMace_norm_unique_07",
            DataUrl = "https://us.battle.net/api/d3/data/item/schaefers-hammer",
            Url = "https://us.battle.net/d3/en/item/schaefers-hammer",
            RelativeUrl = "/d3/en/item/schaefers-hammer",
            IsCrafted = false,
            LegendaryAffix = "Getting hit has a chance to charge you with Lightning, causing you to deal 190–250% weapon damage as Lightning every second for 5 seconds to nearby enemies.",
            SetName = "",
        };

        /// <summary>
        /// Sledge of Athskeleng 
        /// </summary>
        public static Item SledgeOfAthskeleng = new Item
        {
            Id = 190866,
            Name = "Sledge of Athskeleng",
            Quality = ItemQuality.Legendary,
            Slug = "sledge-of-athskeleng",
            ItemType = ItemType.Mace,
            BaseType = ItemBaseType.Weapon,
            InternalName = "twoHandedMace_norm_unique_04",
            DataUrl = "https://us.battle.net/api/d3/data/item/sledge-of-athskeleng",
            Url = "https://us.battle.net/d3/en/item/sledge-of-athskeleng",
            RelativeUrl = "/d3/en/item/sledge-of-athskeleng",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Sunder 25–50% chance to sunder the ground your enemies walk on when you attack.
        /// </summary>
        public static Item Sunder = new Item
        {
            Id = 0,
            Name = "Sunder",
            Quality = ItemQuality.Legendary,
            Slug = "sunder",
            ItemType = ItemType.Mace,
            BaseType = ItemBaseType.Weapon,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/sunder",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/sunder",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/sunder",
            IsCrafted = true,
            LegendaryAffix = "25–50% chance to sunder the ground your enemies walk on when you attack.",
            SetName = "",
        };

        /// <summary>
        /// Izzuccob 
        /// </summary>
        public static Item Izzuccob = new Item
        {
            Id = 192467,
            Name = "Izzuccob",
            Quality = ItemQuality.Legendary,
            Slug = "izzuccob",
            ItemType = ItemType.HandCrossbow,
            BaseType = ItemBaseType.Weapon,
            InternalName = "handXbow_norm_unique_01",
            DataUrl = "https://us.battle.net/api/d3/data/item/izzuccob",
            Url = "https://us.battle.net/d3/en/item/izzuccob",
            RelativeUrl = "/d3/en/item/izzuccob",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Balefire Caster 
        /// </summary>
        public static Item BalefireCaster = new Item
        {
            Id = 192528,
            Name = "Balefire Caster",
            Quality = ItemQuality.Legendary,
            Slug = "balefire-caster",
            ItemType = ItemType.HandCrossbow,
            BaseType = ItemBaseType.Weapon,
            InternalName = "handxbow_norm_unique_02",
            DataUrl = "https://us.battle.net/api/d3/data/item/balefire-caster",
            Url = "https://us.battle.net/d3/en/item/balefire-caster",
            RelativeUrl = "/d3/en/item/balefire-caster",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// K'mar Tenclip Strafe gains the effect of the Drifting Shadow rune.
        /// </summary>
        public static Item KmarTenclip = new Item
        {
            Id = 271892,
            Name = "K'mar Tenclip",
            Quality = ItemQuality.Legendary,
            Slug = "kmar-tenclip",
            ItemType = ItemType.HandCrossbow,
            BaseType = ItemBaseType.Weapon,
            InternalName = "handxbow_norm_unique_10",
            DataUrl = "https://us.battle.net/api/d3/data/item/kmar-tenclip",
            Url = "https://us.battle.net/d3/en/item/kmar-tenclip",
            RelativeUrl = "/d3/en/item/kmar-tenclip",
            IsCrafted = false,
            LegendaryAffix = "Strafe gains the effect of the Drifting Shadow rune.",
            SetName = "",
        };

        /// <summary>
        /// Deadeye 
        /// </summary>
        public static Item Deadeye = new Item
        {
            Id = 0,
            Name = "Deadeye",
            Quality = ItemQuality.Legendary,
            Slug = "deadeye",
            ItemType = ItemType.HandCrossbow,
            BaseType = ItemBaseType.Weapon,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/deadeye",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/deadeye",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/deadeye",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Dawn 
        /// </summary>
        public static Item Dawn = new Item
        {
            Id = 196409,
            Name = "Dawn",
            Quality = ItemQuality.Legendary,
            Slug = "dawn",
            ItemType = ItemType.HandCrossbow,
            BaseType = ItemBaseType.Weapon,
            InternalName = "handXbow_norm_unique_04",
            DataUrl = "https://us.battle.net/api/d3/data/item/dawn",
            Url = "https://us.battle.net/d3/en/item/dawn",
            RelativeUrl = "/d3/en/item/dawn",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Calamity Enemies you hit become Marked for Death.
        /// </summary>
        public static Item Calamity = new Item
        {
            Id = 225181,
            Name = "Calamity",
            Quality = ItemQuality.Legendary,
            Slug = "calamity",
            ItemType = ItemType.HandCrossbow,
            BaseType = ItemBaseType.Weapon,
            InternalName = "handXbow_norm_unique_08",
            DataUrl = "https://us.battle.net/api/d3/data/item/calamity",
            Url = "https://us.battle.net/d3/en/item/calamity",
            RelativeUrl = "/d3/en/item/calamity",
            IsCrafted = false,
            LegendaryAffix = "Enemies you hit become Marked for Death.",
            SetName = "",
        };

        /// <summary>
        /// Danetta's Revenge Vault gains the effect of the Rattling Roll rune.
        /// </summary>
        public static Item DanettasRevenge = new Item
        {
            Id = 211749,
            Name = "Danetta's Revenge",
            Quality = ItemQuality.Legendary,
            Slug = "danettas-revenge",
            ItemType = ItemType.HandCrossbow,
            BaseType = ItemBaseType.Weapon,
            InternalName = "handXbow_norm_unique_07",
            DataUrl = "https://us.battle.net/api/d3/data/item/danettas-revenge",
            Url = "https://us.battle.net/d3/en/item/danettas-revenge",
            RelativeUrl = "/d3/en/item/danettas-revenge",
            IsCrafted = false,
            LegendaryAffix = "Vault gains the effect of the Rattling Roll rune.",
            SetName = "Danetta's Hatred",
        };

        /// <summary>
        /// Danetta's Spite Leave a clone of yourself behind after using Vault.
        /// </summary>
        public static Item DanettasSpite = new Item
        {
            Id = 211745,
            Name = "Danetta's Spite",
            Quality = ItemQuality.Legendary,
            Slug = "danettas-spite",
            ItemType = ItemType.HandCrossbow,
            BaseType = ItemBaseType.Weapon,
            InternalName = "handXbow_norm_unique_06",
            DataUrl = "https://us.battle.net/api/d3/data/item/danettas-spite",
            Url = "https://us.battle.net/d3/en/item/danettas-spite",
            RelativeUrl = "/d3/en/item/danettas-spite",
            IsCrafted = false,
            LegendaryAffix = "Leave a clone of yourself behind after using Vault.",
            SetName = "Danetta's Hatred",
        };

        /// <summary>
        /// Natalya's Slayer 
        /// </summary>
        public static Item NatalyasSlayer = new Item
        {
            Id = 210874,
            Name = "Natalya's Slayer",
            Quality = ItemQuality.Legendary,
            Slug = "natalyas-slayer",
            ItemType = ItemType.HandCrossbow,
            BaseType = ItemBaseType.Weapon,
            InternalName = "handXbow_norm_unique_05",
            DataUrl = "https://us.battle.net/api/d3/data/item/natalyas-slayer",
            Url = "https://us.battle.net/d3/en/item/natalyas-slayer",
            RelativeUrl = "/d3/en/item/natalyas-slayer",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Natalya's Vengeance",
        };

        /// <summary>
        /// Hallowed Judgment 
        /// </summary>
        public static Item HallowedJudgment = new Item
        {
            Id = 0,
            Name = "Hallowed Judgment",
            Quality = ItemQuality.Legendary,
            Slug = "hallowed-avenger",
            ItemType = ItemType.HandCrossbow,
            BaseType = ItemBaseType.Weapon,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/hallowed-avenger",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/hallowed-avenger",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/hallowed-avenger",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "Hallowed Defenders",
        };

        /// <summary>
        /// Blitzbolter 
        /// </summary>
        public static Item Blitzbolter = new Item
        {
            Id = 195078,
            Name = "Blitzbolter",
            Quality = ItemQuality.Legendary,
            Slug = "blitzbolter",
            ItemType = ItemType.HandCrossbow,
            BaseType = ItemBaseType.Weapon,
            InternalName = "handXbow_norm_unique_03",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/blitzbolter",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/blitzbolter",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/blitzbolter",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Hallowed Condemnation 
        /// </summary>
        public static Item HallowedCondemnation = new Item
        {
            Id = 0,
            Name = "Hallowed Condemnation",
            Quality = ItemQuality.Legendary,
            Slug = "hallowed-condemnation",
            ItemType = ItemType.HandCrossbow,
            BaseType = ItemBaseType.Weapon,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/hallowed-condemnation",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/hallowed-condemnation",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/hallowed-condemnation",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "Hallowed Protectors",
        };

        /// <summary>
        /// Uskang 
        /// </summary>
        public static Item Uskang = new Item
        {
            Id = 175580,
            Name = "Uskang",
            Quality = ItemQuality.Legendary,
            Slug = "uskang",
            ItemType = ItemType.Bow,
            BaseType = ItemBaseType.Weapon,
            InternalName = "bow_norm_unique_01",
            DataUrl = "https://us.battle.net/api/d3/data/item/uskang",
            Url = "https://us.battle.net/d3/en/item/uskang",
            RelativeUrl = "/d3/en/item/uskang",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Longshot 
        /// </summary>
        public static Item Longshot = new Item
        {
            Id = 0,
            Name = "Longshot",
            Quality = ItemQuality.Legendary,
            Slug = "longshot",
            ItemType = ItemType.Bow,
            BaseType = ItemBaseType.Weapon,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/longshot",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/longshot",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/longshot",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Etrayu 
        /// </summary>
        public static Item Etrayu = new Item
        {
            Id = 175581,
            Name = "Etrayu",
            Quality = ItemQuality.Legendary,
            Slug = "etrayu",
            ItemType = ItemType.Bow,
            BaseType = ItemBaseType.Weapon,
            InternalName = "Bow_norm_unique_02",
            DataUrl = "https://us.battle.net/api/d3/data/item/etrayu",
            Url = "https://us.battle.net/d3/en/item/etrayu",
            RelativeUrl = "/d3/en/item/etrayu",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// The Raven's Wing A raven flies to your side.
        /// </summary>
        public static Item TheRavensWing = new Item
        {
            Id = 221938,
            Name = "The Raven's Wing",
            Quality = ItemQuality.Legendary,
            Slug = "the-ravens-wing",
            ItemType = ItemType.Bow,
            BaseType = ItemBaseType.Weapon,
            InternalName = "Bow_norm_unique_07",
            DataUrl = "https://us.battle.net/api/d3/data/item/the-ravens-wing",
            Url = "https://us.battle.net/d3/en/item/the-ravens-wing",
            RelativeUrl = "/d3/en/item/the-ravens-wing",
            IsCrafted = false,
            LegendaryAffix = "A raven flies to your side.",
            SetName = "",
        };

        /// <summary>
        /// Kridershot Elemental Arrow now generates 3–4 Hatred.
        /// </summary>
        public static Item Kridershot = new Item
        {
            Id = 271875,
            Name = "Kridershot",
            Quality = ItemQuality.Legendary,
            Slug = "kridershot",
            ItemType = ItemType.Bow,
            BaseType = ItemBaseType.Weapon,
            InternalName = "bow_norm_unique_09",
            DataUrl = "https://us.battle.net/api/d3/data/item/kridershot",
            Url = "https://us.battle.net/d3/en/item/kridershot",
            RelativeUrl = "/d3/en/item/kridershot",
            IsCrafted = false,
            LegendaryAffix = "Elemental Arrow now generates 3–4 Hatred.",
            SetName = "",
        };

        /// <summary>
        /// Cluckeye 25–50% chance to cluck when attacking.
        /// </summary>
        public static Item Cluckeye = new Item
        {
            Id = 175582,
            Name = "Cluckeye",
            Quality = ItemQuality.Legendary,
            Slug = "cluckeye",
            ItemType = ItemType.Bow,
            BaseType = ItemBaseType.Weapon,
            InternalName = "Bow_norm_unique_03",
            DataUrl = "https://us.battle.net/api/d3/data/item/cluckeye",
            Url = "https://us.battle.net/d3/en/item/cluckeye",
            RelativeUrl = "/d3/en/item/cluckeye",
            IsCrafted = false,
            LegendaryAffix = "25–50% chance to cluck when attacking.",
            SetName = "",
        };

        /// <summary>
        /// Windforce 
        /// </summary>
        public static Item Windforce = new Item
        {
            Id = 192602,
            Name = "Windforce",
            Quality = ItemQuality.Legendary,
            Slug = "windforce",
            ItemType = ItemType.Bow,
            BaseType = ItemBaseType.Weapon,
            InternalName = "bow_norm_unique_04",
            DataUrl = "https://us.battle.net/api/d3/data/item/windforce",
            Url = "https://us.battle.net/d3/en/item/windforce",
            RelativeUrl = "/d3/en/item/windforce",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Venomhusk 
        /// </summary>
        public static Item Venomhusk = new Item
        {
            Id = 0,
            Name = "Venomhusk",
            Quality = ItemQuality.Legendary,
            Slug = "venomhusk",
            ItemType = ItemType.Bow,
            BaseType = ItemBaseType.Weapon,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/venomhusk",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/venomhusk",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/venomhusk",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Leonine Bow of Hashir Bola Shot has a 15–20% chance on explosion to pull in all enemies within 24 yards.
        /// </summary>
        public static Item LeonineBowOfHashir = new Item
        {
            Id = 271882,
            Name = "Leonine Bow of Hashir",
            Quality = ItemQuality.Legendary,
            Slug = "leonine-bow-of-hashir",
            ItemType = ItemType.Bow,
            BaseType = ItemBaseType.Weapon,
            InternalName = "bow_norm_unique_11",
            DataUrl = "https://us.battle.net/api/d3/data/item/leonine-bow-of-hashir",
            Url = "https://us.battle.net/d3/en/item/leonine-bow-of-hashir",
            RelativeUrl = "/d3/en/item/leonine-bow-of-hashir",
            IsCrafted = false,
            LegendaryAffix = "Bola Shot has a 15–20% chance on explosion to pull in all enemies within 24 yards.",
            SetName = "",
        };

        /// <summary>
        /// Sydyru Crust 
        /// </summary>
        public static Item SydyruCrust = new Item
        {
            Id = 221893,
            Name = "Sydyru Crust",
            Quality = ItemQuality.Legendary,
            Slug = "sydyru-crust",
            ItemType = ItemType.Bow,
            BaseType = ItemBaseType.Weapon,
            InternalName = "Bow_norm_unique_06",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/sydyru-crust",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/sydyru-crust",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/sydyru-crust",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Unbound Bolt 
        /// </summary>
        public static Item UnboundBolt = new Item
        {
            Id = 220654,
            Name = "Unbound Bolt",
            Quality = ItemQuality.Legendary,
            Slug = "unbound-bolt",
            ItemType = ItemType.Bow,
            BaseType = ItemBaseType.Weapon,
            InternalName = "Bow_norm_unique_05",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/unbound-bolt",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/unbound-bolt",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/unbound-bolt",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Demon Machine 35–65% chance to shoot explosive bolts when attacking.
        /// </summary>
        public static Item DemonMachine = new Item
        {
            Id = 222286,
            Name = "Demon Machine",
            Quality = ItemQuality.Legendary,
            Slug = "demon-machine",
            ItemType = ItemType.Crossbow,
            BaseType = ItemBaseType.Weapon,
            InternalName = "xbow_norm_unique_07",
            DataUrl = "https://us.battle.net/api/d3/data/item/demon-machine",
            Url = "https://us.battle.net/d3/en/item/demon-machine",
            RelativeUrl = "/d3/en/item/demon-machine",
            IsCrafted = false,
            LegendaryAffix = "35–65% chance to shoot explosive bolts when attacking.",
            SetName = "",
        };

        /// <summary>
        /// Buriza-Do Kyanon Your projectiles pierce 1–2 additional times.
        /// </summary>
        public static Item BurizadoKyanon = new Item
        {
            Id = 0,
            Name = "Buriza-Do Kyanon",
            Quality = ItemQuality.Legendary,
            Slug = "burizado-kyanon",
            ItemType = ItemType.Crossbow,
            BaseType = ItemBaseType.Weapon,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/item/burizado-kyanon",
            Url = "https://us.battle.net/d3/en/item/burizado-kyanon",
            RelativeUrl = "/d3/en/item/burizado-kyanon",
            IsCrafted = false,
            LegendaryAffix = "Your projectiles pierce 1–2 additional times.",
            SetName = "",
        };

        /// <summary>
        /// Bakkan Caster 
        /// </summary>
        public static Item BakkanCaster = new Item
        {
            Id = 98163,
            Name = "Bakkan Caster",
            Quality = ItemQuality.Legendary,
            Slug = "bakkan-caster",
            ItemType = ItemType.Crossbow,
            BaseType = ItemBaseType.Weapon,
            InternalName = "xbow_norm_unique_01",
            DataUrl = "https://us.battle.net/api/d3/data/item/bakkan-caster",
            Url = "https://us.battle.net/d3/en/item/bakkan-caster",
            RelativeUrl = "/d3/en/item/bakkan-caster",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Pus Spitter 25–50% chance to lob an acid blob when attacking.
        /// </summary>
        public static Item PusSpitter = new Item
        {
            Id = 204874,
            Name = "Pus Spitter",
            Quality = ItemQuality.Legendary,
            Slug = "pus-spitter",
            ItemType = ItemType.Crossbow,
            BaseType = ItemBaseType.Weapon,
            InternalName = "XBow_norm_unique_05",
            DataUrl = "https://us.battle.net/api/d3/data/item/pus-spitter",
            Url = "https://us.battle.net/d3/en/item/pus-spitter",
            RelativeUrl = "/d3/en/item/pus-spitter",
            IsCrafted = false,
            LegendaryAffix = "25–50% chance to lob an acid blob when attacking.",
            SetName = "",
        };

        /// <summary>
        /// Starspine 
        /// </summary>
        public static Item Starspine = new Item
        {
            Id = 0,
            Name = "Starspine",
            Quality = ItemQuality.Legendary,
            Slug = "starspine",
            ItemType = ItemType.Crossbow,
            BaseType = ItemBaseType.Weapon,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/starspine",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/starspine",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/starspine",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Hellrack Chance to root enemies to the ground when you hit them.
        /// </summary>
        public static Item Hellrack = new Item
        {
            Id = 192836,
            Name = "Hellrack",
            Quality = ItemQuality.Legendary,
            Slug = "hellrack",
            ItemType = ItemType.Crossbow,
            BaseType = ItemBaseType.Weapon,
            InternalName = "XBow_norm_unique_02",
            DataUrl = "https://us.battle.net/api/d3/data/item/hellrack",
            Url = "https://us.battle.net/d3/en/item/hellrack",
            RelativeUrl = "/d3/en/item/hellrack",
            IsCrafted = false,
            LegendaryAffix = "Chance to root enemies to the ground when you hit them.",
            SetName = "",
        };

        /// <summary>
        /// Manticore 
        /// </summary>
        public static Item Manticore = new Item
        {
            Id = 221760,
            Name = "Manticore",
            Quality = ItemQuality.Legendary,
            Slug = "manticore",
            ItemType = ItemType.Crossbow,
            BaseType = ItemBaseType.Weapon,
            InternalName = "XBow_norm_unique_06",
            DataUrl = "https://us.battle.net/api/d3/data/item/manticore",
            Url = "https://us.battle.net/d3/en/item/manticore",
            RelativeUrl = "/d3/en/item/manticore",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Chanon Bolter Your Spike Traps lure enemies to them. Enemies may be taunted once every 12–16 seconds.
        /// </summary>
        public static Item ChanonBolter = new Item
        {
            Id = 271884,
            Name = "Chanon Bolter",
            Quality = ItemQuality.Legendary,
            Slug = "chanon-bolter",
            ItemType = ItemType.Crossbow,
            BaseType = ItemBaseType.Weapon,
            InternalName = "xbow_norm_unique_08",
            DataUrl = "https://us.battle.net/api/d3/data/item/chanon-bolter",
            Url = "https://us.battle.net/d3/en/item/chanon-bolter",
            RelativeUrl = "/d3/en/item/chanon-bolter",
            IsCrafted = false,
            LegendaryAffix = "Your Spike Traps lure enemies to them. Enemies may be taunted once every 12–16 seconds.",
            SetName = "",
        };

        /// <summary>
        /// Wojahnni Assaulter Rapid Fire deals 30–40% more damage for every second that you channel. Stacks up to 4 times.
        /// </summary>
        public static Item WojahnniAssaulter = new Item
        {
            Id = 271889,
            Name = "Wojahnni Assaulter",
            Quality = ItemQuality.Legendary,
            Slug = "wojahnni-assaulter",
            ItemType = ItemType.Crossbow,
            BaseType = ItemBaseType.Weapon,
            InternalName = "xbow_norm_unique_09",
            DataUrl = "https://us.battle.net/api/d3/data/item/wojahnni-assaulter",
            Url = "https://us.battle.net/d3/en/item/wojahnni-assaulter",
            RelativeUrl = "/d3/en/item/wojahnni-assaulter",
            IsCrafted = false,
            LegendaryAffix = "Rapid Fire deals 30–40% more damage for every second that you channel. Stacks up to 4 times.",
            SetName = "",
        };

        /// <summary>
        /// Arcane Barb 
        /// </summary>
        public static Item ArcaneBarb = new Item
        {
            Id = 194957,
            Name = "Arcane Barb",
            Quality = ItemQuality.Legendary,
            Slug = "arcane-barb",
            ItemType = ItemType.Crossbow,
            BaseType = ItemBaseType.Weapon,
            InternalName = "XBow_norm_unique_04",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/arcane-barb",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/arcane-barb",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/arcane-barb",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Starfire 
        /// </summary>
        public static Item Starfire = new Item
        {
            Id = 182074,
            Name = "Starfire",
            Quality = ItemQuality.Legendary,
            Slug = "starfire",
            ItemType = ItemType.Wand,
            BaseType = ItemBaseType.Weapon,
            InternalName = "Wand_norm_unique_04",
            DataUrl = "https://us.battle.net/api/d3/data/item/starfire",
            Url = "https://us.battle.net/d3/en/item/starfire",
            RelativeUrl = "/d3/en/item/starfire",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Blackhand Key 
        /// </summary>
        public static Item BlackhandKey = new Item
        {
            Id = 193355,
            Name = "Blackhand Key",
            Quality = ItemQuality.Legendary,
            Slug = "blackhand-key",
            ItemType = ItemType.Wand,
            BaseType = ItemBaseType.Weapon,
            InternalName = "wand_norm_unique_06",
            DataUrl = "https://us.battle.net/api/d3/data/item/blackhand-key",
            Url = "https://us.battle.net/d3/en/item/blackhand-key",
            RelativeUrl = "/d3/en/item/blackhand-key",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Serpent's Sparker You may have one extra Hydra active at a time.
        /// </summary>
        public static Item SerpentsSparker = new Item
        {
            Id = 272084,
            Name = "Serpent's Sparker",
            Quality = ItemQuality.Legendary,
            Slug = "serpents-sparker",
            ItemType = ItemType.Wand,
            BaseType = ItemBaseType.Weapon,
            InternalName = "wand_norm_unique_02",
            DataUrl = "https://us.battle.net/api/d3/data/item/serpents-sparker",
            Url = "https://us.battle.net/d3/en/item/serpents-sparker",
            RelativeUrl = "/d3/en/item/serpents-sparker",
            IsCrafted = false,
            LegendaryAffix = "You may have one extra Hydra active at a time.",
            SetName = "",
        };

        /// <summary>
        /// Wand of Woh 3 additional Explosive Blasts are triggered after casting Explosive Blast.
        /// </summary>
        public static Item WandOfWoh = new Item
        {
            Id = 272086,
            Name = "Wand of Woh",
            Quality = ItemQuality.Legendary,
            Slug = "wand-of-woh",
            ItemType = ItemType.Wand,
            BaseType = ItemBaseType.Weapon,
            InternalName = "wand_norm_unique_06",
            DataUrl = "https://us.battle.net/api/d3/data/item/wand-of-woh",
            Url = "https://us.battle.net/d3/en/item/wand-of-woh",
            RelativeUrl = "/d3/en/item/wand-of-woh",
            IsCrafted = false,
            LegendaryAffix = "3 additional Explosive Blasts are triggered after casting Explosive Blast.",
            SetName = "",
        };

        /// <summary>
        /// Fragment of Destiny 
        /// </summary>
        public static Item FragmentOfDestiny = new Item
        {
            Id = 181995,
            Name = "Fragment of Destiny",
            Quality = ItemQuality.Legendary,
            Slug = "fragment-of-destiny",
            ItemType = ItemType.Wand,
            BaseType = ItemBaseType.Weapon,
            InternalName = "Wand_norm_unique_02",
            DataUrl = "https://us.battle.net/api/d3/data/item/fragment-of-destiny",
            Url = "https://us.battle.net/d3/en/item/fragment-of-destiny",
            RelativeUrl = "/d3/en/item/fragment-of-destiny",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Gesture of Orpheus Reduces the cooldown of Slow Time by 50–70%.
        /// </summary>
        public static Item GestureOfOrpheus = new Item
        {
            Id = 182071,
            Name = "Gesture of Orpheus",
            Quality = ItemQuality.Legendary,
            Slug = "gesture-of-orpheus",
            ItemType = ItemType.Wand,
            BaseType = ItemBaseType.Weapon,
            InternalName = "Wand_norm_unique_03",
            DataUrl = "https://us.battle.net/api/d3/data/item/gesture-of-orpheus",
            Url = "https://us.battle.net/d3/en/item/gesture-of-orpheus",
            RelativeUrl = "/d3/en/item/gesture-of-orpheus",
            IsCrafted = false,
            LegendaryAffix = "Reduces the cooldown of Slow Time by 50–70%.",
            SetName = "",
        };

        /// <summary>
        /// Slorak's Madness This wand finds your death humorous.
        /// </summary>
        public static Item SloraksMadness = new Item
        {
            Id = 181982,
            Name = "Slorak's Madness",
            Quality = ItemQuality.Legendary,
            Slug = "sloraks-madness",
            ItemType = ItemType.Wand,
            BaseType = ItemBaseType.Weapon,
            InternalName = "Wand_norm_unique_01",
            DataUrl = "https://us.battle.net/api/d3/data/item/sloraks-madness",
            Url = "https://us.battle.net/d3/en/item/sloraks-madness",
            RelativeUrl = "/d3/en/item/sloraks-madness",
            IsCrafted = false,
            LegendaryAffix = "This wand finds your death humorous.",
            SetName = "",
        };

        /// <summary>
        /// Chantodo's Will 
        /// </summary>
        public static Item ChantodosWill = new Item
        {
            Id = 210479,
            Name = "Chantodo's Will",
            Quality = ItemQuality.Legendary,
            Slug = "chantodos-will",
            ItemType = ItemType.Wand,
            BaseType = ItemBaseType.Weapon,
            InternalName = "Wand_norm_unique_07",
            DataUrl = "https://us.battle.net/api/d3/data/item/chantodos-will",
            Url = "https://us.battle.net/d3/en/item/chantodos-will",
            RelativeUrl = "/d3/en/item/chantodos-will",
            IsCrafted = false,
            LegendaryAffix = "",
            SetName = "Chantodo's Resolve",
        };

        /// <summary>
        /// Ruinstoke 
        /// </summary>
        public static Item Ruinstoke = new Item
        {
            Id = 0,
            Name = "Ruinstoke",
            Quality = ItemQuality.Legendary,
            Slug = "ruinstoke",
            ItemType = ItemType.Wand,
            BaseType = ItemBaseType.Weapon,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/ruinstoke",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/ruinstoke",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/ruinstoke",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Hallowed Scepter 
        /// </summary>
        public static Item HallowedScepter = new Item
        {
            Id = 0,
            Name = "Hallowed Scepter",
            Quality = ItemQuality.Legendary,
            Slug = "hallowed-rod",
            ItemType = ItemType.Wand,
            BaseType = ItemBaseType.Weapon,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/hallowed-rod",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/hallowed-rod",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/hallowed-rod",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "Hallowed Defenders",
        };

        /// <summary>
        /// Aether Walker Teleport no longer has a cooldown but costs 25 Arcane Power.
        /// </summary>
        public static Item AetherWalker = new Item
        {
            Id = 0,
            Name = "Aether Walker",
            Quality = ItemQuality.Legendary,
            Slug = "aether-walker",
            ItemType = ItemType.Wand,
            BaseType = ItemBaseType.Weapon,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/item/aether-walker",
            Url = "https://us.battle.net/d3/en/item/aether-walker",
            RelativeUrl = "/d3/en/item/aether-walker",
            IsCrafted = false,
            LegendaryAffix = "Teleport no longer has a cooldown but costs 25 Arcane Power.",
            SetName = "",
        };

        /// <summary>
        /// Atrophy 
        /// </summary>
        public static Item Atrophy = new Item
        {
            Id = 182081,
            Name = "Atrophy",
            Quality = ItemQuality.Legendary,
            Slug = "atrophy",
            ItemType = ItemType.Wand,
            BaseType = ItemBaseType.Weapon,
            InternalName = "Wand_norm_unique_05",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/atrophy",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/atrophy",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/atrophy",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "",
        };

        /// <summary>
        /// Hallowed Baton 
        /// </summary>
        public static Item HallowedBaton = new Item
        {
            Id = 0,
            Name = "Hallowed Baton",
            Quality = ItemQuality.Legendary,
            Slug = "hallowed-baton",
            ItemType = ItemType.Wand,
            BaseType = ItemBaseType.Weapon,
            InternalName = "",
            DataUrl = "https://us.battle.net/api/d3/data/recipe/hallowed-baton",
            Url = "https://us.battle.net/d3/en/artisan/blacksmith/recipe/hallowed-baton",
            RelativeUrl = "/d3/en/artisan/blacksmith/recipe/hallowed-baton",
            IsCrafted = true,
            LegendaryAffix = "",
            SetName = "Hallowed Protectors",
        };

        //// Generated at 6/24/2014 9:58:58 PM

        //// Amulet   
        //public static Item BlackthornesDuncraigCross = new Item(224189, "Blackthorne's Duncraig Cross", ItemType.Amulet);
        //public static Item CountessJuliasCameo = new Item(298050, "Countess Julia's Cameo", ItemType.Amulet);
        //public static Item DovuEnergyTrap = new Item(298054, "Dovu Energy Trap", ItemType.Amulet);
        //public static Item ExecutionersMedal = new Item(341342, "Executioner's Medal", ItemType.Amulet);
        //public static Item EyeofEtlich = new Item(197823, "Eye of Etlich", ItemType.Amulet);
        //public static Item GoldenGorgetofLeoric = new Item(298052, "Golden Gorget of Leoric", ItemType.Amulet);
        //public static Item HalcyonsAscent = new Item(298056, "Halcyon's Ascent", ItemType.Amulet);
        //public static Item HauntofVaxo = new Item(297806, "Haunt of Vaxo", ItemType.Amulet);
        //public static Item HolyBeacon = new Item(197822, "Holy Beacon", ItemType.Amulet);
        //public static Item KymbosGold = new Item(197812, "Kymbo's Gold", ItemType.Amulet);
        //public static Item MarasKaleidoscope = new Item(197824, "Mara's Kaleidoscope", ItemType.Amulet);
        //public static Item MoonlightWard = new Item(197813, "Moonlight Ward", ItemType.Amulet);
        //public static Item Ouroboros = new Item(197815, "Ouroboros", ItemType.Amulet);
        //public static Item OverwhelmingDesire = new Item(298053, "Overwhelming Desire", ItemType.Amulet);
        //public static Item RakoffsGlassofLife = new Item(298055, "Rakoff's Glass of Life", ItemType.Amulet);
        //public static Item RondalsLocket = new Item(197818, "Rondal's Locket", ItemType.Amulet);
        //public static Item SquirtsNecklace = new Item(197819, "Squirt's Necklace", ItemType.Amulet);
        //public static Item SunwukosShines = new Item(336174, "Sunwuko's Shines", ItemType.Amulet);
        //public static Item TalRashasAllegiance = new Item(222486, "Tal Rasha's Allegiance", ItemType.Amulet);
        //public static Item TalismanofAranoch = new Item(197821, "Talisman of Aranoch", ItemType.Amulet);
        //public static Item TheEssofJohan = new Item(298051, "The Ess of Johan", ItemType.Amulet);
        //public static Item TheFlavorofTime = new Item(193659, "The Flavor of Time", ItemType.Amulet);
        //public static Item TheStarofAzkaranth = new Item(197817, "The Star of Azkaranth", ItemType.Amulet);
        //public static Item TheTravelersPledge = new Item(222490, "The Traveler's Pledge", ItemType.Amulet);
        //public static Item XephirianAmulet = new Item(197814, "Xephirian Amulet", ItemType.Amulet);

        //// Axe   
        //public static Item ButchersCarver = new Item(186494, "Butcher's Carver", ItemType.Axe);
        //public static Item CinderSwitch = new Item(6329, "Cinder Switch", ItemType.Axe);
        //public static Item FleshTearer = new Item(116388, "Flesh Tearer", ItemType.Axe);
        //public static Item Genzaniku = new Item(116386, "Genzaniku", ItemType.Axe);
        //public static Item Hack = new Item(271598, "Hack", ItemType.Axe);
        //public static Item MesserschmidtsReaver = new Item(191065, "Messerschmidt's Reaver", ItemType.Axe);
        //public static Item Skorn = new Item(192887, "Skorn", ItemType.Axe);
        //public static Item SkySplitter = new Item(116389, "Sky Splitter", ItemType.Axe);
        //public static Item TheBurningAxeofSankis = new Item(181484, "The Burning Axe of Sankis", ItemType.Axe);
        //public static Item TheButchersSickle = new Item(189973, "The Butcher's Sickle", ItemType.Axe);
        //public static Item TheExecutioner = new Item(186560, "The Executioner", ItemType.Axe);
        //public static Item UtarsRoar = new Item(116387, "Utar's Roar", ItemType.Axe);

        //// Belt   
        //public static Item AngelHairBraid = new Item(193666, "Angel Hair Braid", ItemType.Belt);
        //public static Item BlackthornesNotchedBelt = new Item(224191, "Blackthorne's Notched Belt", ItemType.Belt);
        //public static Item CaptainCrimsonsSilkGirdle = new Item(222974, "Captain Crimson's Silk Girdle", ItemType.Belt);
        //public static Item CordoftheSherma = new Item(298127, "Cord of the Sherma", ItemType.Belt);
        //public static Item DemonsRestraint = new Item(222740, "Demon's Restraint", ItemType.Belt);
        //public static Item FleetingStrap = new Item(193667, "Fleeting Strap", ItemType.Belt);
        //public static Item Goldwrap = new Item(193671, "Goldwrap", ItemType.Belt);
        //public static Item GuardiansCase = new Item(222976, "Guardian's Case", ItemType.Belt);
        //public static Item HarringtonWaistguard = new Item(298129, "Harrington Waistguard", ItemType.Belt);
        //public static Item HellcatWaistguard = new Item(193668, "Hellcat Waistguard", ItemType.Belt);
        //public static Item HwojWrap = new Item(298131, "Hwoj Wrap", ItemType.Belt);
        //public static Item InnasFavor = new Item(222487, "Inna's Favor", ItemType.Belt);
        //public static Item InsatiableBelt = new Item(298126, "Insatiable Belt", ItemType.Belt);
        //public static Item JangsEnvelopment = new Item(298130, "Jang’s Envelopment ", ItemType.Belt);
        //public static Item KredesFlame = new Item(197836, "Krede’s Flame", ItemType.Belt);
        //public static Item RazorStrop = new Item(298124, "Razor Strop", ItemType.Belt);
        //public static Item SaffronWrap = new Item(193664, "Saffron Wrap", ItemType.Belt);
        //public static Item SashofKnives = new Item(298125, "Sash of Knives", ItemType.Belt);
        //public static Item SeborsNightmare = new Item(299381, "Sebor’s Nightmare", ItemType.Belt);
        //public static Item StringofEars = new Item(193669, "String of Ears", ItemType.Belt);
        //public static Item TalRashasBrace = new Item(212657, "Tal Rasha's Brace", ItemType.Belt);
        //public static Item TheWitchingHour = new Item(193670, "The Witching Hour", ItemType.Belt);
        //public static Item ThundergodsVigor = new Item(212230, "Thundergod's Vigor", ItemType.Belt);
        //public static Item VigilanteBelt = new Item(193665, "Vigilante Belt", ItemType.Belt);

        //// Boots   
        //public static Item AshearasFinders = new Item(205618, "Asheara's Finders", ItemType.Boots);
        //public static Item BlackthornesSpurs = new Item(222463, "Blackthorne's Spurs", ItemType.Boots);
        //public static Item BoardWalkers = new Item(205621, "Board Walkers", ItemType.Boots);
        //public static Item BojAnglers = new Item(197224, "Boj Anglers", ItemType.Boots);
        //public static Item BootsofDisregard = new Item(322905, "Boots of Disregard", ItemType.Boots);
        //public static Item CainsTravelers = new Item(197225, "Cain's Travelers", ItemType.Boots);
        //public static Item CaptainCrimsonsWaders = new Item(197221, "Captain Crimson's Waders", ItemType.Boots);
        //public static Item CaptainCrimsonsWhalers = new Item(197221, "Captain Crimson's Whalers", ItemType.Boots);
        //public static Item EightDemonBoots = new Item(338031, "Eight-Demon Boots", ItemType.Boots);
        //public static Item FireWalkers = new Item(205624, "Fire Walkers", ItemType.Boots);
        //public static Item FirebirdsTarsi = new Item(358793, "Firebird's Tarsi", ItemType.Boots);
        //public static Item HelltoothGreaves = new Item(340524, "Helltooth Greaves", ItemType.Boots);
        //public static Item IceClimbers = new Item(222464, "Ice Climbers", ItemType.Boots);
        //public static Item IllusoryBoots = new Item(332342, "Illusory Boots", ItemType.Boots);
        //public static Item ImmortalKingsStride = new Item(205625, "Immortal King's Stride", ItemType.Boots);
        //public static Item IrontoeMudsputters = new Item(339125, "Irontoe Mudsputters", ItemType.Boots);
        //public static Item JadeHarvestersSwiftness = new Item(338037, "Jade Harvester's Swiftness", ItemType.Boots);
        //public static Item LutSocks = new Item(205622, "Lut Socks", ItemType.Boots);
        //public static Item MaraudersTreads = new Item(336995, "Marauder's Treads", ItemType.Boots);
        //public static Item NatalyasBloodyFootprints = new Item(197223, "Natalya's Bloody Footprints", ItemType.Boots);
        //public static Item RaekorsStriders = new Item(336987, "Raekor’s Striders", ItemType.Boots);
        //public static Item SabatonsofAkkhan = new Item(358795, "Sabatons of Akkhan", ItemType.Boots);
        //public static Item SagesPassage = new Item(205626, "Sage's Passage", ItemType.Boots);
        //public static Item TheCrudestBoots = new Item(205620, "The Crudest Boots", ItemType.Boots);
        //public static Item TheShadowsHeels = new Item(332364, "The Shadow’s Heels", ItemType.Boots);
        //public static Item VyrsSwaggeringStance = new Item(332363, "Vyr’s Swaggering Stance", ItemType.Boots);
        //public static Item ZunimassasTrail = new Item(205627, "Zunimassa's Trail", ItemType.Boots);

        //// Bow   
        //public static Item Cluckeye = new Item(175582, "Cluckeye", ItemType.Bow);
        //public static Item SydyruCrust = new Item(221893, "Sydyru Crust", ItemType.Bow);
        //public static Item TheRavensWing = new Item(221938, "The Raven's Wing", ItemType.Bow);
        //public static Item UnboundBolt = new Item(220654, "Unbound Bolt", ItemType.Bow);
        //public static Item Uskang = new Item(175580, "Uskang", ItemType.Bow);

        //// Bracer   
        //public static Item AncientParthanDefenders = new Item(298116, "Ancient Parthan Defenders", ItemType.Bracer);
        //public static Item AughildsSearch = new Item(222972, "Aughild's Search", ItemType.Bracer);
        //public static Item CusterianWristguards = new Item(298122, "Custerian Wristguards", ItemType.Bracer);
        //public static Item DemonsAnimus = new Item(222741, "Demon's Animus", ItemType.Bracer);
        //public static Item GungdoGear = new Item(193688, "Gungdo Gear", ItemType.Bracer);
        //public static Item KethryesSplint = new Item(193683, "Kethryes' Splint", ItemType.Bracer);
        //public static Item LacuniProwlers = new Item(193687, "Lacuni Prowlers", ItemType.Bracer);
        //public static Item NemesisBracers = new Item(298121, "Nemesis Bracers", ItemType.Bracer);
        //public static Item PromiseofGlory = new Item(193684, "Promise of Glory", ItemType.Bracer);
        //public static Item ReapersWraps = new Item(298118, "Reaper's Wraps", ItemType.Bracer);
        //public static Item SanguinaryVambraces = new Item(298120, "Sanguinary Vambraces", ItemType.Bracer);
        //public static Item ShacklesoftheInvoker = new Item(335030, "Shackles of the Invoker", ItemType.Bracer);
        //public static Item SlaveBonds = new Item(193685, "Slave Bonds", ItemType.Bracer);
        //public static Item SteadyStrikers = new Item(193686, "Steady Strikers", ItemType.Bracer);
        //public static Item StrongarmBracers = new Item(193692, "Strongarm Bracers", ItemType.Bracer);
        //public static Item TragOulCoils = new Item(298119, "Trag'Oul Coils", ItemType.Bracer);
        //public static Item WarzechianArmguards = new Item(298115, "Warzechian Armguards", ItemType.Bracer);

        //// CeremonialDagger   
        //public static Item AnessaziEdge = new Item(196250, "Anessazi Edge", ItemType.CeremonialDagger);
        //public static Item DeadlyRebirth = new Item(193433, "Deadly Rebirth", ItemType.CeremonialDagger);
        //public static Item LastBreath = new Item(195370, "Last Breath", ItemType.CeremonialDagger);
        //public static Item LivingUmbralOath = new Item(192540, "Living Umbral Oath", ItemType.CeremonialDagger);
        //public static Item ManajumasCarvingKnife = new Item(223365, "Manajuma's Carving Knife", ItemType.CeremonialDagger);
        //public static Item RhenhoFlayer = new Item(271745, "Rhen'ho Flayer", ItemType.CeremonialDagger);
        //public static Item StarmetalKukri = new Item(271738, "Starmetal Kukri", ItemType.CeremonialDagger);
        //public static Item TheGidbinn = new Item(209246, "The Gidbinn", ItemType.CeremonialDagger);
        //public static Item TheSpiderQueensGrasp = new Item(222978, "The Spider Queen’s Grasp", ItemType.CeremonialDagger);

        //// Chest   
        //public static Item AquilaCuirass = new Item(197203, "Aquila Cuirass", ItemType.Chest);
        //public static Item ArmoroftheKindRegent = new Item(332202, "Armor of the Kind Regent", ItemType.Chest);
        //public static Item AughildsRule = new Item(197193, "Aughild's Rule", ItemType.Chest);
        //public static Item BlackthornesSurcoat = new Item(222456, "Blackthorne's Surcoat", ItemType.Chest);
        //public static Item BornsFrozenSoul = new Item(197199, "Born's Frozen Soul", ItemType.Chest);
        //public static Item BreastplateofAkkhan = new Item(358796, "Breastplate of Akkhan", ItemType.Chest);
        //public static Item Chaingmail = new Item(197204, "Chaingmail", ItemType.Chest);
        //public static Item Cindercoat = new Item(222455, "Cindercoat", ItemType.Chest);
        //public static Item DemonsMarrow = new Item(205612, "Demon's Marrow", ItemType.Chest);
        //public static Item FirebirdsBreast = new Item(358788, "Firebird's Breast", ItemType.Chest);
        //public static Item Goldskin = new Item(205616, "Goldskin", ItemType.Chest);
        //public static Item HeartofIron = new Item(205607, "Heart of Iron", ItemType.Chest);
        //public static Item HeartoftheCrashingWave = new Item(338032, "Heart of the Crashing Wave", ItemType.Chest);
        //public static Item HelltoothTunic = new Item(363088, "Helltooth Tunic", ItemType.Chest);
        //public static Item ImmortalKingsEternalReign = new Item(205613, "Immortal King's Eternal Reign", ItemType.Chest);
        //public static Item InnasVastExpanse = new Item(205614, "Inna's Vast Expanse", ItemType.Chest);
        //public static Item JadeHarvestersPeace = new Item(338038, "Jade Harvester's Peace", ItemType.Chest);
        //public static Item MantleoftheRydraelm = new Item(205609, "Mantle of the Rydraelm", ItemType.Chest);
        //public static Item MaraudersCarapace = new Item(363803, "Marauder's Carapace", ItemType.Chest);
        //public static Item RaekorsHeart = new Item(336984, "Raekor’s Heart", ItemType.Chest);
        //public static Item ShiMizusHaori = new Item(332200, "Shi Mizu's Haori", ItemType.Chest);
        //public static Item TalRashasRelentlessPursuit = new Item(211626, "Tal Rasha's Relentless Pursuit", ItemType.Chest);
        //public static Item TheShadowsBane = new Item(332359, "The Shadow’s Bane", ItemType.Chest);
        //public static Item TyraelsMight = new Item(205608, "Tyrael's Might", ItemType.Chest);
        //public static Item VirtueInvoker = new Item(253994, "Virtue Invoker", ItemType.Chest);
        //public static Item VyrsAstonishingAura = new Item(332357, "Vyr’s Astonishing Aura", ItemType.Chest);
        //public static Item ZunimassasMarrow = new Item(205615, "Zunimassa's Marrow", ItemType.Chest);

        //// Cloak   
        //public static Item BeckonSail = new Item(223150, "Beckon Sail", ItemType.Cloak);
        //public static Item Blackfeather = new Item(332206, "Blackfeather", ItemType.Cloak);
        //public static Item CapeoftheDarkNight = new Item(223149, "Cape of the Dark Night", ItemType.Cloak);
        //public static Item CloakofDeception = new Item(332208, "Cloak of Deception", ItemType.Cloak);
        //public static Item NatalyasEmbrace = new Item(208934, "Natalya's Embrace", ItemType.Cloak);
        //public static Item TheCloakoftheGarwulf = new Item(223151, "The Cloak of the Garwulf", ItemType.Cloak);

        //// Crossbow   
        //public static Item ArcaneBarb = new Item(194957, "Arcane Barb", ItemType.Crossbow);
        //public static Item BakkanCaster = new Item(98163, "Bakkan Caster", ItemType.Crossbow);
        //public static Item BurizaDoKyanon = new Item(194219, "Buriza-Do Kyanon", ItemType.Crossbow);
        //public static Item DemonMachine = new Item(222286, "Demon Machine", ItemType.Crossbow);
        //public static Item Hellrack = new Item(192836, "Hellrack", ItemType.Crossbow);
        //public static Item PusSpitter = new Item(204874, "Pus Spitter", ItemType.Crossbow);

        //// CrusaderShield   
        //public static Item HallowedBulwark = new Item(299413, "Hallowed Bulwark", ItemType.CrusaderShield);
        //public static Item Hellskull = new Item(299415, "Hellskull", ItemType.CrusaderShield);
        //public static Item Jekangbord = new Item(299412, "Jekangbord", ItemType.CrusaderShield);
        //public static Item Salvation = new Item(299418, "Salvation", ItemType.CrusaderShield);
        //public static Item SublimeConviction = new Item(299416, "Sublime Conviction", ItemType.CrusaderShield);
        //public static Item TheFinalWitness = new Item(299417, "The Final Witness", ItemType.CrusaderShield);

        //// Dagger   
        //public static Item BloodMagicEdge = new Item(195655, "Blood-Magic Edge", ItemType.Dagger);
        //public static Item EnviousBlade = new Item(271732, "Envious Blade", ItemType.Dagger);
        //public static Item Kill = new Item(192579, "Kill", ItemType.Dagger);
        //public static Item PigSticker = new Item(221313, "Pig Sticker", ItemType.Dagger);
        //public static Item TheBarber = new Item(195174, "The Barber", ItemType.Dagger);
        //public static Item Wizardspike = new Item(219329, "Wizardspike", ItemType.Dagger);

        //// Daibo   
        //public static Item Balance = new Item(195145, "Balance", ItemType.Daibo);
        //public static Item FlyingDragon = new Item(197065, "Flying Dragon", ItemType.Daibo);
        //public static Item IncenseTorchoftheGrandTemple = new Item(192342, "Incense Torch of the Grand Temple", ItemType.Daibo);
        //public static Item InnasReach = new Item(212208, "Inna's Reach", ItemType.Daibo);
        //public static Item LaiYuisPersuader = new Item(209214, "Lai Yui's Persuader", ItemType.Daibo);
        //public static Item RozpedinsForce = new Item(196880, "Rozpedin's Force", ItemType.Daibo);
        //public static Item TheFlowofEternity = new Item(197072, "The Flow of Eternity", ItemType.Daibo);
        //public static Item ThePaddle = new Item(197068, "The Paddle", ItemType.Daibo);

        //// FistWeapon   
        //public static Item CrystalFist = new Item(175939, "Crystal Fist", ItemType.FistWeapon);
        //public static Item DemonClaw = new Item(193459, "Demon Claw", ItemType.FistWeapon);
        //public static Item Fleshrake = new Item(145850, "Fleshrake", ItemType.FistWeapon);
        //public static Item LogansClaw = new Item(145849, "Logan's Claw", ItemType.FistWeapon);
        //public static Item RabidStrike = new Item(196472, "Rabid Strike", ItemType.FistWeapon);
        //public static Item ShenlongsFistofLegend = new Item(208996, "Shenlong's Fist of Legend", ItemType.FistWeapon);
        //public static Item ShenlongsRelentlessAssault = new Item(208898, "Shenlong's Relentless Assault", ItemType.FistWeapon);
        //public static Item SledgeFist = new Item(175938, "Sledge Fist", ItemType.FistWeapon);
        //public static Item TheFistofAzTurrasq = new Item(175937, "The Fist of Az'Turrasq", ItemType.FistWeapon);

        //// Flail   
        //public static Item BalefulRemnant = new Item(299435, "Baleful Remnant", ItemType.Flail);
        //public static Item Darklight = new Item(299428, "Darklight", ItemType.Flail);
        //public static Item FateoftheFell = new Item(299436, "Fate of the Fell", ItemType.Flail);
        //public static Item GoldenFlense = new Item(299437, "Golden Flense", ItemType.Flail);
        //public static Item GoldenScourge = new Item(299419, "Golden Scourge", ItemType.Flail);
        //public static Item InviolableFaith = new Item(299429, "Inviolable Faith", ItemType.Flail);
        //public static Item JustiniansMercy = new Item(299424, "Justinian's Mercy", ItemType.Flail);
        //public static Item KassarsRetribution = new Item(299426, "Kassar's Retribution", ItemType.Flail);
        //public static Item Swiftmount = new Item(299425, "Swiftmount", ItemType.Flail);
        //public static Item TheMortalDrama = new Item(299431, "The Mortal Drama", ItemType.Flail);

        //// FollowerSpecial   
        //public static Item EnchantingFavor = new Item(366968, "Enchanting Favor", ItemType.FollowerSpecial);
        //public static Item HandoftheProphet = new Item(366980, "Hand of the Prophet", ItemType.FollowerSpecial);
        //public static Item RelicofAkarat = new Item(366969, "Relic of Akarat", ItemType.FollowerSpecial);
        //public static Item RibaldEtchings = new Item(366971, "Ribald Etchings", ItemType.FollowerSpecial);
        //public static Item SkeletonKey = new Item(366970, "Skeleton Key", ItemType.FollowerSpecial);
        //public static Item SmokingThurible = new Item(366979, "Smoking Thurible", ItemType.FollowerSpecial);

        //// Gloves   
        //public static Item AshearasWard = new Item(205636, "Asheara's Ward", ItemType.Gloves);
        //public static Item CainsScribe = new Item(197210, "Cain's Scribe", ItemType.Gloves);
        //public static Item FirebirdsTalons = new Item(358789, "Firebird's Talons", ItemType.Gloves);
        //public static Item FistsofThunder = new Item(338033, "Fists of Thunder", ItemType.Gloves);
        //public static Item ForgeHarm = new Item(341342, "Forge Harm", ItemType.Gloves);
        ////public static Item ForgeHarm = new Item(224051, "Forge Harm", ItemType.Gloves);
        ////public static Item ForgeHarm = new Item(197193, "Forge Harm", ItemType.Gloves);
        //public static Item Frostburn = new Item(197205, "Frostburn", ItemType.Gloves);
        //public static Item GauntletsofAkkhan = new Item(358798, "Gauntlets of Akkhan", ItemType.Gloves);
        //public static Item GladiatorGauntlets = new Item(205635, "Gladiator Gauntlets", ItemType.Gloves);
        //public static Item GlovesofWorship = new Item(332344, "Gloves of Worship", ItemType.Gloves);
        //public static Item HelltoothGauntlets = new Item(363094, "Helltooth Gauntlets", ItemType.Gloves);
        //public static Item ImmortalKingsIrons = new Item(205631, "Immortal King's Irons", ItemType.Gloves);
        //public static Item JadeHarvestersMercy = new Item(338039, "Jade Harvester's Mercy", ItemType.Gloves);
        //public static Item Magefist = new Item(197206, "Magefist", ItemType.Gloves);
        //public static Item MaraudersGloves = new Item(336992, "Marauder's Gloves", ItemType.Gloves);
        //public static Item PendersPurchase = new Item(197207, "Penders Purchase", ItemType.Gloves);
        //public static Item PrideoftheInvoker = new Item(335027, "Pride of the Invoker", ItemType.Gloves);
        //public static Item PulloftheEarth = new Item(340523, "Pull of the Earth", ItemType.Gloves);
        //public static Item RaekorsWraps = new Item(336985, "Raekor’s Wraps", ItemType.Gloves);
        //public static Item StArchewsGage = new Item(332172, "St. Archew's Gage", ItemType.Gloves);
        //public static Item StoneGauntlets = new Item(205640, "Stone Gauntlets", ItemType.Gloves);
        //public static Item SunwukosPaws = new Item(336172, "Sunwuko's Paws", ItemType.Gloves);
        //public static Item TaskerandTheo = new Item(205642, "Tasker and Theo", ItemType.Gloves);
        //public static Item VyrsGraspingGauntlets = new Item(346210, "Vyr’s Grasping Gauntlets", ItemType.Gloves);

        //// HandCrossbow   
        //public static Item BalefireCaster = new Item(192528, "Balefire Caster", ItemType.HandCrossbow);
        //public static Item Blitzbolter = new Item(195078, "Blitzbolter", ItemType.HandCrossbow);
        //public static Item DanettasSpite = new Item(211745, "Danetta's Spite", ItemType.HandCrossbow);
        //public static Item DanettasRevenge = new Item(211749, "Danetta's Revenge", ItemType.HandCrossbow);
        //public static Item Dawn = new Item(196409, "Dawn", ItemType.HandCrossbow);

        //public static Item Izzuccob = new Item(192467, "Izzuccob", ItemType.HandCrossbow);

        //// Helm   
        //public static Item AndarielsVisage = new Item(198014, "Andariel's Visage", ItemType.Helm);
        //public static Item BlindFaith = new Item(197037, "Blind Faith", ItemType.Helm);
        //public static Item BrokenCrown = new Item(220630, "Broken Crown", ItemType.Helm);
        //public static Item CrownoftheInvoker = new Item(335028, "Crown of the Invoker", ItemType.Helm);
        //public static Item DeathseersCowl = new Item(298146, "Deathseer's Cowl", ItemType.Helm);
        //public static Item EyesoftheEarth = new Item(340528, "Eyes of the Earth", ItemType.Helm);
        //public static Item FirebirdsPlume = new Item(358791, "Firebird's Plume", ItemType.Helm);
        //public static Item HelmofAkkhan = new Item(358799, "Helm of Akkhan", ItemType.Helm);
        //public static Item ImmortalKingsTriumph = new Item(210265, "Immortal King's Triumph", ItemType.Helm);
        //public static Item JadeHarvestersWisdom = new Item(338040, "Jade Harvester's Wisdom", ItemType.Helm);
        //public static Item LeoricsCrown = new Item(196024, "Leoric's Crown", ItemType.Helm);
        //public static Item MaraudersVisage = new Item(336994, "Marauder's Visage", ItemType.Helm);
        //public static Item MaskofJeramSetHelm = new Item(369016, "Mask of Jeram", ItemType.Helm);
        //public static Item MaskoftheSearingSky = new Item(338034, "Mask of the Searing Sky", ItemType.Helm);
        //public static Item MempoofTwilight = new Item(223577, "Mempo of Twilight", ItemType.Helm);
        //public static Item NatalyasSight = new Item(210851, "Natalya's Sight", ItemType.Helm);
        //public static Item PridesFall = new Item(298147, "Pride's Fall", ItemType.Helm);
        //public static Item RaekorsWill = new Item(336988, "Raekor’s Will", ItemType.Helm);
        //public static Item SkullofResonance = new Item(220549, "Skull of Resonance", ItemType.Helm);
        //public static Item SunwukosCrown = new Item(336173, "Sunwuko's Crown", ItemType.Helm);
        //public static Item TalRashasGuiseofWisdom = new Item(211531, "Tal Rasha's Guise of Wisdom", ItemType.Helm);

        //// Legs   
        //public static Item AshearasPace = new Item(209054, "Asheara's Pace", ItemType.Legs);
        //public static Item BlackthornesJoustingMail = new Item(222477, "Blackthorne's Jousting Mail", ItemType.Legs);
        //public static Item CainsHabit = new Item(197218, "Cain's Habit", ItemType.Legs);
        //public static Item CaptainCrimsonsThrust = new Item(197214, "Captain Crimson's Thrust", ItemType.Legs);
        //public static Item CuissesofAkkhan = new Item(358800, "Cuisses of Akkhan", ItemType.Legs);
        //public static Item DeathsBargain = new Item(332205, "Death's Bargain", ItemType.Legs);
        //public static Item DepthDiggers = new Item(197216, "Depth Diggers", ItemType.Legs);
        //public static Item FirebirdsDown = new Item(358790, "Firebird's Down", ItemType.Legs);
        //public static Item HammerJammers = new Item(209059, "Hammer Jammers", ItemType.Legs);
        //public static Item HelltoothLegGuards = new Item(340522, "Helltooth Leg Guards", ItemType.Legs);
        //public static Item HexingPantsofMrYan = new Item(332204, "Hexing Pants of Mr. Yan", ItemType.Legs);
        //public static Item InnasTemperance = new Item(205646, "Inna's Temperance", ItemType.Legs);
        //public static Item JadeHarvestersCourage = new Item(338041, "Jade Harvester's Courage", ItemType.Legs);
        //public static Item MaraudersEncasement = new Item(336993, "Marauder's Encasement", ItemType.Legs);
        //public static Item PoxFaulds = new Item(197220, "Pox Faulds", ItemType.Legs);
        //public static Item RaekorsBreeches = new Item(336986, "Raekor’s Breeches", ItemType.Legs);
        //public static Item ScalesoftheDancingSerpent = new Item(338035, "Scales of the Dancing Serpent", ItemType.Legs);
        //public static Item SwampLandWaders = new Item(209057, "Swamp Land Waders", ItemType.Legs);
        //public static Item TheShadowsCoil = new Item(332361, "The Shadow’s Coil", ItemType.Legs);
        //public static Item VyrsFantasticFinery = new Item(332360, "Vyr’s Fantastic Finery", ItemType.Legs);
        //public static Item WeightoftheEarth = new Item(340521, "Weight of the Earth", ItemType.Legs);

        //// Mace   
        //public static Item ArthefsSparkofLife = new Item(59633, "Arthef’s Spark of Life", ItemType.Mace);
        //public static Item Crushbane = new Item(99227, "Crushbane", ItemType.Mace);
        //public static Item Devastator = new Item(188177, "Devastator", ItemType.Mace);
        //public static Item EchoingFury = new Item(188181, "Echoing Fury", ItemType.Mace);
        //public static Item JacesHammerofVigilance = new Item(271648, "Jace’s Hammer of Vigilance", ItemType.Mace);
        //public static Item MadMonarchsScepter = new Item(271663, "Mad Monarch's Scepter", ItemType.Mace);
        //public static Item Nailbiter = new Item(188158, "Nailbiter", ItemType.Mace);
        //public static Item Neanderthal = new Item(102665, "Neanderthal", ItemType.Mace);
        //public static Item Nutcracker = new Item(188169, "Nutcracker", ItemType.Mace);
        //public static Item OdynSon = new Item(188185, "Odyn Son", ItemType.Mace);
        //public static Item SchaefersHammer = new Item(197717, "Schaefer's Hammer", ItemType.Mace);
        //public static Item Skywarden = new Item(190840, "Skywarden", ItemType.Mace);
        //public static Item SledgeofAthskeleng = new Item(190866, "Sledge of Athskeleng", ItemType.Mace);
        //public static Item Solanium = new Item(271662, "Solanium", ItemType.Mace);
        //public static Item Soulsmasher = new Item(271671, "Soulsmasher", ItemType.Mace);
        //public static Item SunKeeper = new Item(188173, "Sun Keeper", ItemType.Mace);
        //public static Item Sunder = new Item(190868, "Sunder", ItemType.Mace);
        //public static Item SupremacyNexus = new Item(191584, "Supremacy Nexus", ItemType.Mace);
        //public static Item TelrandensHand = new Item(188189, "Telranden's Hand", ItemType.Mace);
        //public static Item TheFurnace = new Item(271666, "The Furnace", ItemType.Mace);
        //public static Item WrathoftheBoneKing = new Item(191584, "Wrath of the Bone King", ItemType.Mace);

        //// MightyBelt   
        //public static Item AgelessMight = new Item(193675, "Ageless Might", ItemType.MightyBelt);
        //public static Item ChilaniksChain = new Item(298133, "Chilanik’s Chain", ItemType.MightyBelt);
        //public static Item DreadIron = new Item(193672, "Dread Iron", ItemType.MightyBelt);
        //public static Item GirdleofGiants = new Item(212232, "Girdle of Giants", ItemType.MightyBelt);
        //public static Item ImmortalKingsTribalBinding = new Item(212235, "Immortal King's Tribal Binding", ItemType.MightyBelt);
        //public static Item KotuursBrace = new Item(193674, "Kotuur's Brace", ItemType.MightyBelt);
        //public static Item Lamentation = new Item(212234, "Lamentation", ItemType.MightyBelt);
        //public static Item PrideofCassius = new Item(193673, "Pride of Cassius", ItemType.MightyBelt);
        //public static Item TheUndisputedChampion = new Item(193676, "The Undisputed Champion", ItemType.MightyBelt);

        //// MightyWeapon   
        //public static Item AmbosPride = new Item(193486, "Ambo's Pride", ItemType.MightyWeapon);
        //public static Item BastionsRevered = new Item(195690, "Bastion's Revered", ItemType.MightyWeapon);
        //public static Item BladeoftheWarlord = new Item(193611, "Blade of the Warlord", ItemType.MightyWeapon);
        //public static Item BulKathossSolemnVow = new Item(208771, "Bul-Kathos's Solemn Vow", ItemType.MightyWeapon);
        //public static Item BulKathossWarriorBlood = new Item(208775, "Bul-Kathos's Warrior Blood", ItemType.MightyWeapon);
        //public static Item FjordCutter = new Item(192105, "Fjord Cutter", ItemType.MightyWeapon);
        //public static Item ImmortalKingsBoulderBreaker = new Item(210678, "Immortal King's Boulder Breaker", ItemType.MightyWeapon);
        //public static Item MadawcsSorrow = new Item(272012, "Madawc's Sorrow", ItemType.MightyWeapon);
        //public static Item NightsReaping = new Item(192705, "Night's Reaping", ItemType.MightyWeapon);
        //public static Item TheGavelofJudgment = new Item(193657, "The Gavel of Judgment", ItemType.MightyWeapon);
        //public static Item WaroftheDead = new Item(196308, "War of the Dead", ItemType.MightyWeapon);

        //// Mojo   
        //public static Item GazingDemise = new Item(194995, "Gazing Demise", ItemType.Mojo);
        //public static Item Homunculus = new Item(194991, "Homunculus", ItemType.Mojo);
        //public static Item ManajumasGoryFetch = new Item(210993, "Manajuma's Gory Fetch", ItemType.Mojo);
        //public static Item ShukranisTriumph = new Item(272070, "Shukrani’s Triumph", ItemType.Mojo);
        //public static Item Spite = new Item(194988, "Spite", ItemType.Mojo);
        //public static Item ThingoftheDeep = new Item(192468, "Thing of the Deep", ItemType.Mojo);
        //public static Item UhkapianSerpent = new Item(191278, "Uhkapian Serpent", ItemType.Mojo);
        //public static Item ZunimassasStringofSkulls = new Item(216525, "Zunimassa's String of Skulls", ItemType.Mojo);

        //// Orb   
        //public static Item ChantodosForce = new Item(212277, "Chantodo's Force", ItemType.Orb);
        //public static Item CosmicStrand = new Item(195127, "Cosmic Strand", ItemType.Orb);
        //public static Item FirebirdsEye = new Item(358819, "Firebird's Eye", ItemType.Orb);
        //public static Item LightofGrace = new Item(272038, "Light of Grace", ItemType.Orb);
        //public static Item Mirrorball = new Item(272022, "Mirrorball", ItemType.Orb);
        //public static Item MykensBallofHate = new Item(272037, "Myken's Ball of Hate", ItemType.Orb);
        //public static Item TalRashasUnwaveringGlare = new Item(212780, "Tal Rasha's Unwavering Glare", ItemType.Orb);
        //public static Item TheOculus = new Item(192320, "The Oculus", ItemType.Orb);
        //public static Item Triumvirate = new Item(195325, "Triumvirate", ItemType.Orb);
        //public static Item WinterFlurry = new Item(184199, "Winter Flurry", ItemType.Orb);

        //// Polearm   
        //public static Item BovineBardiche = new Item(272056, "Bovine Bardiche", ItemType.Polearm);
        //public static Item HeartSlaughter = new Item(192569, "Heart Slaughter", ItemType.Polearm);
        //public static Item PledgeofCaldeum = new Item(196570, "Pledge of Caldeum", ItemType.Polearm);
        //public static Item Standoff = new Item(191570, "Standoff", ItemType.Polearm);
        //public static Item Vigilance = new Item(195491, "Vigilance", ItemType.Polearm);

        //// Potion   
        //public static Item BottomlessPotionofKulleAid = new Item(344093, "Bottomless Potion of Kulle-Aid", ItemType.Potion);
        //public static Item BottomlessPotionofMutilation = new Item(342824, "Bottomless Potion of Mutilation", ItemType.Potion);
        //public static Item BottomlessPotionofRegeneration = new Item(341343, "Bottomless Potion of Regeneration", ItemType.Potion);
        //public static Item BottomlessPotionoftheDiamond = new Item(341342, "Bottomless Potion of the Diamond", ItemType.Potion);
        //public static Item BottomlessPotionoftheLeech = new Item(342823, "Bottomless Potion of the Leech", ItemType.Potion);
        //public static Item BottomlessPotionoftheTower = new Item(341333, "Bottomless Potion of the Tower", ItemType.Potion);

        //// Quiver   
        //public static Item ArchfiendArrows = new Item(197626, "Archfiend Arrows", ItemType.Quiver);
        //public static Item BombadiersRucksack = new Item(298171, "Bombadier's Rucksack", ItemType.Quiver);
        //public static Item DeadMansLegacy = new Item(197630, "Dead Man's Legacy", ItemType.Quiver);
        //public static Item EmimeisDuffel = new Item(298172, "Emimei’s Duffel", ItemType.Quiver);
        //public static Item FletchersPride = new Item(197629, "Fletcher's Pride", ItemType.Quiver);
        //public static Item SilverStarPiercers = new Item(197628, "Silver Star Piercers", ItemType.Quiver);
        //public static Item SinSeekers = new Item(197625, "Sin Seekers", ItemType.Quiver);
        //public static Item TheNinthCirriSatchel = new Item(298170, "The Ninth Cirri Satchel", ItemType.Quiver);

        //// Ring   
        //public static Item AvariceBand = new Item(298095, "Avarice Band", ItemType.Ring);
        //public static Item BandofHollowWhispers = new Item(197834, "Band of Hollow Whispers", ItemType.Ring);
        //public static Item BandoftheRueChambers = new Item(298093, "Band of the Rue Chambers", ItemType.Ring);
        //public static Item BandofUntoldSecrets = new Item(212602, "Band of Untold Secrets", ItemType.Ring);
        //public static Item BrokenPromises = new Item(212589, "Broken Promises", ItemType.Ring);
        //public static Item BulKathossWeddingBand = new Item(212603, "Bul-Kathos's Wedding Band", ItemType.Ring);
        //public static Item EternalUnion = new Item(212601, "Eternal Union", ItemType.Ring);
        //public static Item Focus = new Item(332209, "Focus", ItemType.Ring);
        //public static Item HellfireRing = new Item(260327, "Hellfire Ring", ItemType.Ring);
        //public static Item JusticeLantern = new Item(212590, "Justice Lantern", ItemType.Ring);
        //public static Item LeoricsSignet = new Item(197835, "Leoric's Signet", ItemType.Ring);
        //public static Item LitanyoftheUndaunted = new Item(212651, "Litany of the Undaunted", ItemType.Ring);
        //public static Item ManaldHeal = new Item(212546, "Manald Heal", ItemType.Ring);
        //public static Item Nagelring = new Item(212586, "Nagelring", ItemType.Ring);
        //public static Item NatalyasReflection = new Item(212545, "Natalya's Reflection", ItemType.Ring);
        //public static Item ObsidianRingoftheZodiac = new Item(212588, "Obsidian Ring of the Zodiac", ItemType.Ring);
        //public static Item OculusRing = new Item(212648, "Oculus Ring", ItemType.Ring);
        //public static Item PandemoniumLoop = new Item(298096, "Pandemonium Loop", ItemType.Ring);
        //public static Item PuzzleRing = new Item(197837, "Puzzle Ring", ItemType.Ring);
        //public static Item RechelsRingofLarceny = new Item(298091, "Rechel's Ring of Larceny", ItemType.Ring);
        //public static Item Restraint = new Item(332210, "Restraint", ItemType.Ring);
        //public static Item RingofRoyalGrandeur = new Item(298094, "Ring of Royal Grandeur", ItemType.Ring);
        //public static Item RogarsHugeStone = new Item(298090, "Rogar's Huge Stone", ItemType.Ring);
        //public static Item SkullGrasp = new Item(212618, "Skull Grasp", ItemType.Ring);
        //public static Item StolenRing = new Item(197839, "Stolen Ring", ItemType.Ring);
        //public static Item StoneofJordan = new Item(212582, "Stone of Jordan", ItemType.Ring);
        //public static Item TheCompassRose = new Item(212587, "The Compass Rose", ItemType.Ring);
        //public static Item TheTallMansFinger = new Item(298088, "The Tall Man's Finger", ItemType.Ring);
        //public static Item TheWailingHost = new Item(212650, "The Wailing Host", ItemType.Ring);
        //public static Item Unity = new Item(212581, "Unity", ItemType.Ring);
        //public static Item Wyrdward = new Item(298089, "Wyrdward", ItemType.Ring);
        //public static Item ZunimassasPox = new Item(212579, "Zunimassa's Pox", ItemType.Ring);

        //// Shield   
        //public static Item CovensCriterion = new Item(298191, "Coven’s Criterion", ItemType.Shield);
        //public static Item DefenderofWestmarch = new Item(298182, "Defender of Westmarch", ItemType.Shield);
        //public static Item Denial = new Item(152666, "Denial", ItemType.Shield);
        //public static Item EberliCharo = new Item(298186, "Eberli Charo", ItemType.Shield);
        //public static Item FreezeofDeflection = new Item(61550, "Freeze of Deflection", ItemType.Shield);
        //public static Item IvoryTower = new Item(197478, "Ivory Tower", ItemType.Shield);
        //public static Item LidlessWall = new Item(195389, "Lidless Wall", ItemType.Shield);
        //public static Item Stormshield = new Item(192484, "Stormshield", ItemType.Shield);
        //public static Item VoToyiasSpiker = new Item(298188, "Vo'Toyias Spiker", ItemType.Shield);

        //// Shoulder   
        //public static Item AshearasCustodian = new Item(225132, "Asheara's Custodian", ItemType.Shoulder);
        //public static Item AughildsPower = new Item(224051, "Aughild's Power", ItemType.Shoulder);
        //public static Item BornsPrivilege = new Item(222948, "Born's Privilege", ItemType.Shoulder);
        //public static Item BurdenoftheInvoker = new Item(335029, "Burden of the Invoker", ItemType.Shoulder);
        //public static Item DeathWatchMantle = new Item(200310, "Death Watch Mantle", ItemType.Shoulder);
        //public static Item DemonsAileron = new Item(224397, "Demon's Aileron", ItemType.Shoulder);
        //public static Item FirebirdsPinions = new Item(358792, "Firebird's Pinions", ItemType.Shoulder);
        //public static Item HelltoothMantle = new Item(340525, "Helltooth Mantle", ItemType.Shoulder);
        //public static Item HomingPads = new Item(198573, "Homing Pads", ItemType.Shoulder);
        //public static Item JadeHarvestersJoy = new Item(338042, "Jade Harvester's Joy", ItemType.Shoulder);
        //public static Item MantleoftheUpsideDownSinners = new Item(338036, "Mantle of the Upside-Down Sinners", ItemType.Shoulder);
        //public static Item MaraudersSpines = new Item(336996, "Marauder's Spines", ItemType.Shoulder);
        //public static Item PauldronsofAkkhan = new Item(358801, "Pauldrons of Akkhan", ItemType.Shoulder);
        //public static Item PauldronsoftheSkeletonKing = new Item(298164, "Pauldrons of the Skeleton King", ItemType.Shoulder);
        //public static Item ProfanePauldrons = new Item(298158, "Profane Pauldrons", ItemType.Shoulder);
        //public static Item RaekorsBurden = new Item(336989, "Raekor’s Burden", ItemType.Shoulder);
        //public static Item SpauldersofZakara = new Item(298163, "Spaulders of Zakara", ItemType.Shoulder);
        //public static Item SpiresoftheEarth = new Item(340526, "Spires of the Earth", ItemType.Shoulder);
        //public static Item SunwukosBalance = new Item(336175, "Sunwuko's Balance", ItemType.Shoulder);
        //public static Item VileWard = new Item(201325, "Vile Ward", ItemType.Shoulder);

        //// Spear   
        //public static Item ArreatsLaw = new Item(191446, "Arreat's Law", ItemType.Spear);
        //public static Item EmpyreanMessenger = new Item(194241, "Empyrean Messenger", ItemType.Spear);
        //public static Item Scrimshaw = new Item(197095, "Scrimshaw", ItemType.Spear);
        //public static Item TheThreeHundredthSpear = new Item(196638, "The Three Hundredth Spear", ItemType.Spear);

        //// SpiritStone   
        //public static Item EyeofPeshkov = new Item(299464, "Eye of Peshkov", ItemType.SpiritStone);
        //public static Item GyanaNaKashu = new Item(222169, "Gyana Na Kashu", ItemType.SpiritStone);
        //public static Item InnasRadiance = new Item(222307, "Inna's Radiance", ItemType.SpiritStone);
        //public static Item KekegisUnbreakableSpirit = new Item(299461, "Kekegi's Unbreakable Spirit", ItemType.SpiritStone);
        //public static Item SeeNoEvil = new Item(222171, "See No Evil", ItemType.SpiritStone);
        //public static Item TheEyeoftheStorm = new Item(222170, "The Eye of the Storm", ItemType.SpiritStone);
        //public static Item TheLawsofSeph = new Item(299454, "The Laws of Seph", ItemType.SpiritStone);
        //public static Item TheMindsEye = new Item(222172, "The Mind's Eye", ItemType.SpiritStone);
        //public static Item TzoKrinsGaze = new Item(222305, "Tzo Krin's Gaze", ItemType.SpiritStone);

        //// Staff   
        //public static Item AutumnsCall = new Item(184228, "Autumn's Call", ItemType.Staff);
        //public static Item MalothsFocus = new Item(193832, "Maloth's Focus", ItemType.Staff);
        //public static Item MarkofTheMagi = new Item(59612, "Mark of The Magi", ItemType.Staff);
        //public static Item TheBrokenStaff = new Item(59601, "The Broken Staff", ItemType.Staff);
        //public static Item TheGrandVizier = new Item(192167, "The Grand Vizier", ItemType.Staff);
        //public static Item TheTormentor = new Item(193066, "The Tormentor", ItemType.Staff);
        //public static Item ValtheksRebuke = new Item(271773, "Valthek's Rebuke", ItemType.Staff);
        //public static Item Wormwood = new Item(195407, "Wormwood", ItemType.Staff);

        //// Sword   
        //public static Item Azurewrath = new Item(192511, "Azurewrath", ItemType.Sword);
        //public static Item BladeofProphecy = new Item(184184, "Blade of Prophecy", ItemType.Sword);
        //public static Item CamsRebuttal = new Item(271644, "Cam's Rebuttal", ItemType.Sword);
        //public static Item DevilTongue = new Item(189552, "Devil Tongue", ItemType.Sword);
        //public static Item Doombringer = new Item(185397, "Doombringer", ItemType.Sword);
        //public static Item Exarian = new Item(271617, "Exarian", ItemType.Sword);
        //public static Item FaithfulMemory = new Item(198960, "Faithful Memory", ItemType.Sword);
        //public static Item Fulminator = new Item(271631, "Fulminator", ItemType.Sword);
        //public static Item GiftofSilaria = new Item(271630, "Gift of Silaria", ItemType.Sword);
        //public static Item GriswoldsPerfection = new Item(270977, "Griswold's Perfection", ItemType.Sword);
        //public static Item LittleRogue = new Item(313291, "Little Rogue", ItemType.Sword);
        //public static Item Maximus = new Item(184187, "Maximus", ItemType.Sword);
        //public static Item MonsterHunter = new Item(115140, "Monster Hunter", ItemType.Sword);
        //public static Item Rimeheart = new Item(271636, "Rimeheart", ItemType.Sword);
        //public static Item Scourge = new Item(181511, "Scourge", ItemType.Sword);
        //public static Item Sever = new Item(115141, "Sever", ItemType.Sword);
        //public static Item ShardofHate = new Item(376463, "Shard of Hate", ItemType.Sword);
        //public static Item Skycutter = new Item(182347, "Skycutter", ItemType.Sword);
        //public static Item StalgardsDecimator = new Item(271639, "Stalgard's Decimator", ItemType.Sword);
        //public static Item TheAncientBonesaberofZumakalis = new Item(194481, "The Ancient Bonesaber of Zumakalis", ItemType.Sword);
        //public static Item TheGrandfather = new Item(190360, "The Grandfather", ItemType.Sword);
        //public static Item TheSlanderer = new Item(313290, "The Slanderer", ItemType.Sword);
        //public static Item TheSultanofBlindingSand = new Item(184190, "The Sultan of Blinding Sand", ItemType.Sword);
        //public static Item TheZweihander = new Item(59665, "The Zweihander", ItemType.Sword);
        //public static Item ThunderfuryBlessedBladeoftheWindseeker = new Item(229716, "Thunderfury, Blessed Blade of the Windseeker", ItemType.Sword);
        //public static Item Warmonger = new Item(181495, "Warmonger", ItemType.Sword);
        //public static Item Wildwood = new Item(270978, "Wildwood", ItemType.Sword);

        //// VoodooMask   
        //public static Item Carnevil = new Item(299442, "Carnevil", ItemType.VoodooMask);
        //public static Item MaskofJeram = new Item(299443, "Mask of Jeram", ItemType.VoodooMask);
        //public static Item Quetzalcoatl = new Item(204136, "Quetzalcoatl", ItemType.VoodooMask);
        //public static Item SplitTusk = new Item(221167, "Split Tusk", ItemType.VoodooMask);
        //public static Item TheGrinReaper = new Item(221166, "The Grin Reaper", ItemType.VoodooMask);
        //public static Item TiklandianVisage = new Item(221382, "Tiklandian Visage", ItemType.VoodooMask);
        //public static Item VisageofGiyua = new Item(221168, "Visage of Giyua", ItemType.VoodooMask);
        //public static Item ZunimassasVision = new Item(221202, "Zunimassa's Vision", ItemType.VoodooMask);

        //// Wand   
        //public static Item Atrophy = new Item(182081, "Atrophy", ItemType.Wand);
        //public static Item BlackhandKey = new Item(193355, "Blackhand Key", ItemType.Wand);
        //public static Item ChantodosWill = new Item(210479, "Chantodo's Will", ItemType.Wand);
        //public static Item FragmentofDestiny = new Item(181995, "Fragment of Destiny", ItemType.Wand);
        //public static Item GestureofOrpheus = new Item(182071, "Gesture of Orpheus", ItemType.Wand);
        //public static Item SerpentsSparker = new Item(272084, "Serpent's Sparker", ItemType.Wand);
        //public static Item SloraksMadness = new Item(181982, "Slorak's Madness", ItemType.Wand);
        //public static Item Starfire = new Item(182074, "Starfire", ItemType.Wand);
        //public static Item WandofWoh = new Item(272086, "Wand of Woh", ItemType.Wand);

        //// WizardHat   
        //public static Item ArchmagesVicalyke = new Item(299471, "Archmage's Vicalyke", ItemType.WizardHat);
        //public static Item StormCrow = new Item(220694, "Storm Crow", ItemType.WizardHat);
        //public static Item TheMagistrate = new Item(325579, "The Magistrate", ItemType.WizardHat);
        //public static Item TheSwami = new Item(218681, "The Swami", ItemType.WizardHat);
        //public static Item VelvetCamaral = new Item(299472, "Velvet Camaral", ItemType.WizardHat);

        //#region Legacy Items (not from diacollector)

        //public static Item SagesPurchase = new Item(205632, "Sage's Purchase", ItemType.Gloves);
        //public static Item CainsScrivener = new Item(197210, "Cain's Scrivener", ItemType.Gloves);
        //public static Item BornsFuriousWrath = new Item(223408, "Born's Furious Wrath", ItemType.Sword);
        //public static Item AughildsSpike = new Item(223972, "Aughild's Spike", ItemType.Helm);
        //public static Item SagesApogee = new Item(221624, "Sage's Apogee", ItemType.Helm);
        //public static Item GuardiansGaze = new Item(221518, "Guardian's Gaze", ItemType.Helm);
        //public static Item CainsInsight = new Item(222559, "Cain's Insight", ItemType.Helm);
        //public static Item DemonsPlate = new Item(205644, "Demon's Plate", ItemType.Legs);
        //public static Item WonKhimLau = new Item(145851, "Won Khim Lau", ItemType.FistWeapon);
        //public static Item HallowedHold = new Item(223526, "Hallowed Hold", ItemType.FistWeapon);
        //public static Item Jawbreaker = new Item(271957, "Jawbreaker", ItemType.FistWeapon);
        //public static Item GuardiansAversion = new Item(222981, "Guardian's Aversion", ItemType.Bracer);
        //public static Item StaffofKyro = new Item(271749, "Staff of Kyro", ItemType.Daibo);
        //public static Item HallowedBreach = new Item(223461, "Hallowed Breach", ItemType.Axe);
        //public static Item AhavarionSpearofLycander = new Item(271768, "Ahavarion Spear of Lycander", ItemType.Staff);
        //public static Item FuryoftheVanishedPeak = new Item(195138, "Fury of the Vanished Peak", ItemType.MightyWeapon);
        //public static Item HallowedNemesis = new Item(223627, "Hallowed Nemesis", ItemType.MightyWeapon);
        //public static Item HallowedBaton = new Item(224184, "Hallowed Baton", ItemType.Wand);
        //public static Item DarkMagesShade = new Item(224908, "Dark Mage's Shade", ItemType.WizardHat);
        //public static Item HallowedBarricade = new Item(223758, "Hallowed Barricade", ItemType.Shield);
        //public static Item HallowedCondemnation = new Item(223763, "Hallowed Condemnation", ItemType.HandCrossbow);
        //public static Item HallowedSufferance = new Item(223396, "Hallowed Sufferance", ItemType.CeremonialDagger);

        //#endregion

        #region Methods

        /// <summary>
        /// Hashset of all Legendaries ActorSNO
        /// </summary>
        public static HashSet<int> ItemIds
        {
            get { return _itemIds ?? (_itemIds = new HashSet<int>(ToList().Select(i => i.Id))); }
        }
        private static HashSet<int> _itemIds;

        /// <summary>
        /// Gets equipped legendaries
        /// </summary>
        public static List<Item> Equipped
        {
            get { return ToList().Where(i => i.IsEquipped).ToList(); }
        }

        /// <summary>
        /// Gets equipped legendaries as ACDItems
        /// </summary>
        public static List<ACDItem> EquippedACDItems
        {
            get { return EquippedItemCache.Instance.Items.Where(i => ItemIds.Contains(i.ActorSNO)).ToList(); }
        }

        #endregion

    }


}
