using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Trinity.Cache;
using Trinity.Helpers;
using Trinity.Objects;
using Zeta.Common;
using Zeta.Game;
using Zeta.Game.Internals.Actors;
using Logger = Trinity.Technicals.Logger;

namespace Trinity.Reference
{
    public class Legendary : FieldCollection<Legendary,Item>
    {
        // Load static version of sets class
        public class Sets : Reference.Sets { }

        // Generated at 6/24/2014 9:58:58 PM

        // Amulet   
        public static Item BlackthornesDuncraigCross = new Item(224189, "Blackthorne's Duncraig Cross", ItemType.Amulet);
        public static Item CountessJuliasCameo = new Item(298050, "Countess Julia's Cameo", ItemType.Amulet);
        public static Item DovuEnergyTrap = new Item(298054, "Dovu Energy Trap", ItemType.Amulet);
        public static Item ExecutionersMedal = new Item(341342, "Executioner's Medal", ItemType.Amulet);
        public static Item EyeofEtlich = new Item(197823, "Eye of Etlich", ItemType.Amulet);
        public static Item GoldenGorgetofLeoric = new Item(298052, "Golden Gorget of Leoric", ItemType.Amulet);
        public static Item HalcyonsAscent = new Item(298056, "Halcyon's Ascent", ItemType.Amulet);
        public static Item HauntofVaxo = new Item(297806, "Haunt of Vaxo", ItemType.Amulet);
        public static Item HolyBeacon = new Item(197822, "Holy Beacon", ItemType.Amulet);
        public static Item KymbosGold = new Item(197812, "Kymbo's Gold", ItemType.Amulet);
        public static Item MarasKaleidoscope = new Item(197824, "Mara's Kaleidoscope", ItemType.Amulet);
        public static Item MoonlightWard = new Item(197813, "Moonlight Ward", ItemType.Amulet);
        public static Item Ouroboros = new Item(197815, "Ouroboros", ItemType.Amulet);
        public static Item OverwhelmingDesire = new Item(298053, "Overwhelming Desire", ItemType.Amulet);
        public static Item RakoffsGlassofLife = new Item(298055, "Rakoff's Glass of Life", ItemType.Amulet);
        public static Item RondalsLocket = new Item(197818, "Rondal's Locket", ItemType.Amulet);
        public static Item SquirtsNecklace = new Item(197819, "Squirt's Necklace", ItemType.Amulet);
        public static Item SunwukosShines = new Item(336174, "Sunwuko's Shines", ItemType.Amulet);
        public static Item TalRashasAllegiance = new Item(222486, "Tal Rasha's Allegiance", ItemType.Amulet);
        public static Item TalismanofAranoch = new Item(197821, "Talisman of Aranoch", ItemType.Amulet);
        public static Item TheEssofJohan = new Item(298051, "The Ess of Johan", ItemType.Amulet);
        public static Item TheFlavorofTime = new Item(193659, "The Flavor of Time", ItemType.Amulet);
        public static Item TheStarofAzkaranth = new Item(197817, "The Star of Azkaranth", ItemType.Amulet);
        public static Item TheTravelersPledge = new Item(222490, "The Traveler's Pledge", ItemType.Amulet);
        public static Item XephirianAmulet = new Item(197814, "Xephirian Amulet", ItemType.Amulet);

        // Axe   
        public static Item ButchersCarver = new Item(186494, "Butcher's Carver", ItemType.Axe);
        public static Item CinderSwitch = new Item(6329, "Cinder Switch", ItemType.Axe);
        public static Item FleshTearer = new Item(116388, "Flesh Tearer", ItemType.Axe);
        public static Item Genzaniku = new Item(116386, "Genzaniku", ItemType.Axe);
        public static Item Hack = new Item(271598, "Hack", ItemType.Axe);
        public static Item MesserschmidtsReaver = new Item(191065, "Messerschmidt's Reaver", ItemType.Axe);
        public static Item Skorn = new Item(192887, "Skorn", ItemType.Axe);
        public static Item SkySplitter = new Item(116389, "Sky Splitter", ItemType.Axe);
        public static Item TheBurningAxeofSankis = new Item(181484, "The Burning Axe of Sankis", ItemType.Axe);
        public static Item TheButchersSickle = new Item(189973, "The Butcher's Sickle", ItemType.Axe);
        public static Item TheExecutioner = new Item(186560, "The Executioner", ItemType.Axe);
        public static Item UtarsRoar = new Item(116387, "Utar's Roar", ItemType.Axe);

        // Belt   
        public static Item AngelHairBraid = new Item(193666, "Angel Hair Braid", ItemType.Belt);
        public static Item BlackthornesNotchedBelt = new Item(224191, "Blackthorne's Notched Belt", ItemType.Belt);
        public static Item CaptainCrimsonsSilkGirdle = new Item(222974, "Captain Crimson's Silk Girdle", ItemType.Belt);
        public static Item CordoftheSherma = new Item(298127, "Cord of the Sherma", ItemType.Belt);
        public static Item DemonsRestraint = new Item(222740, "Demon's Restraint", ItemType.Belt);
        public static Item FleetingStrap = new Item(193667, "Fleeting Strap", ItemType.Belt);
        public static Item Goldwrap = new Item(193671, "Goldwrap", ItemType.Belt);
        public static Item GuardiansCase = new Item(222976, "Guardian's Case", ItemType.Belt);
        public static Item HarringtonWaistguard = new Item(298129, "Harrington Waistguard", ItemType.Belt);
        public static Item HellcatWaistguard = new Item(193668, "Hellcat Waistguard", ItemType.Belt);
        public static Item HwojWrap = new Item(298131, "Hwoj Wrap", ItemType.Belt);
        public static Item InnasFavor = new Item(222487, "Inna's Favor", ItemType.Belt);
        public static Item InsatiableBelt = new Item(298126, "Insatiable Belt", ItemType.Belt);
        public static Item JangsEnvelopment = new Item(298130, "Jang’s Envelopment ", ItemType.Belt);
        public static Item KredesFlame = new Item(197836, "Krede’s Flame", ItemType.Belt);
        public static Item RazorStrop = new Item(298124, "Razor Strop", ItemType.Belt);
        public static Item SaffronWrap = new Item(193664, "Saffron Wrap", ItemType.Belt);
        public static Item SashofKnives = new Item(298125, "Sash of Knives", ItemType.Belt);
        public static Item SeborsNightmare = new Item(299381, "Sebor’s Nightmare", ItemType.Belt);
        public static Item StringofEars = new Item(193669, "String of Ears", ItemType.Belt);
        public static Item TalRashasBrace = new Item(212657, "Tal Rasha's Brace", ItemType.Belt);
        public static Item TheWitchingHour = new Item(193670, "The Witching Hour", ItemType.Belt);
        public static Item ThundergodsVigor = new Item(212230, "Thundergod's Vigor", ItemType.Belt);
        public static Item VigilanteBelt = new Item(193665, "Vigilante Belt", ItemType.Belt);

        // Boots   
        public static Item AshearasFinders = new Item(205618, "Asheara's Finders", ItemType.Boots);
        public static Item BlackthornesSpurs = new Item(222463, "Blackthorne's Spurs", ItemType.Boots);
        public static Item BoardWalkers = new Item(205621, "Board Walkers", ItemType.Boots);
        public static Item BojAnglers = new Item(197224, "Boj Anglers", ItemType.Boots);
        public static Item BootsofDisregard = new Item(322905, "Boots of Disregard", ItemType.Boots);
        public static Item CainsTravelers = new Item(197225, "Cain's Travelers", ItemType.Boots);
        public static Item CaptainCrimsonsWaders = new Item(197221, "Captain Crimson's Waders", ItemType.Boots);
        public static Item CaptainCrimsonsWhalers = new Item(197221, "Captain Crimson's Whalers", ItemType.Boots);
        public static Item EightDemonBoots = new Item(338031, "Eight-Demon Boots", ItemType.Boots);
        public static Item FireWalkers = new Item(205624, "Fire Walkers", ItemType.Boots);
        public static Item FirebirdsTarsi = new Item(358793, "Firebird's Tarsi", ItemType.Boots);
        public static Item HelltoothGreaves = new Item(340524, "Helltooth Greaves", ItemType.Boots);
        public static Item IceClimbers = new Item(222464, "Ice Climbers", ItemType.Boots);
        public static Item IllusoryBoots = new Item(332342, "Illusory Boots", ItemType.Boots);
        public static Item ImmortalKingsStride = new Item(205625, "Immortal King's Stride", ItemType.Boots);
        public static Item IrontoeMudsputters = new Item(339125, "Irontoe Mudsputters", ItemType.Boots);
        public static Item JadeHarvestersSwiftness = new Item(338037, "Jade Harvester's Swiftness", ItemType.Boots);
        public static Item LutSocks = new Item(205622, "Lut Socks", ItemType.Boots);
        public static Item MaraudersTreads = new Item(336995, "Marauder's Treads", ItemType.Boots);
        public static Item NatalyasBloodyFootprints = new Item(197223, "Natalya's Bloody Footprints", ItemType.Boots);
        public static Item RaekorsStriders = new Item(336987, "Raekor’s Striders", ItemType.Boots);
        public static Item SabatonsofAkkhan = new Item(358795, "Sabatons of Akkhan", ItemType.Boots);
        public static Item SagesPassage = new Item(205626, "Sage's Passage", ItemType.Boots);
        public static Item TheCrudestBoots = new Item(205620, "The Crudest Boots", ItemType.Boots);
        public static Item TheShadowsHeels = new Item(332364, "The Shadow’s Heels", ItemType.Boots);
        public static Item VyrsSwaggeringStance = new Item(332363, "Vyr’s Swaggering Stance", ItemType.Boots);
        public static Item ZunimassasTrail = new Item(205627, "Zunimassa's Trail", ItemType.Boots);

        // Bow   
        public static Item Cluckeye = new Item(175582, "Cluckeye", ItemType.Bow);
        public static Item SydyruCrust = new Item(221893, "Sydyru Crust", ItemType.Bow);
        public static Item TheRavensWing = new Item(221938, "The Raven's Wing", ItemType.Bow);
        public static Item UnboundBolt = new Item(220654, "Unbound Bolt", ItemType.Bow);
        public static Item Uskang = new Item(175580, "Uskang", ItemType.Bow);

        // Bracer   
        public static Item AncientParthanDefenders = new Item(298116, "Ancient Parthan Defenders", ItemType.Bracer);
        public static Item AughildsSearch = new Item(222972, "Aughild's Search", ItemType.Bracer);
        public static Item CusterianWristguards = new Item(298122, "Custerian Wristguards", ItemType.Bracer);
        public static Item DemonsAnimus = new Item(222741, "Demon's Animus", ItemType.Bracer);
        public static Item GungdoGear = new Item(193688, "Gungdo Gear", ItemType.Bracer);
        public static Item KethryesSplint = new Item(193683, "Kethryes' Splint", ItemType.Bracer);
        public static Item LacuniProwlers = new Item(193687, "Lacuni Prowlers", ItemType.Bracer);
        public static Item NemesisBracers = new Item(298121, "Nemesis Bracers", ItemType.Bracer);
        public static Item PromiseofGlory = new Item(193684, "Promise of Glory", ItemType.Bracer);
        public static Item ReapersWraps = new Item(298118, "Reaper's Wraps", ItemType.Bracer);
        public static Item SanguinaryVambraces = new Item(298120, "Sanguinary Vambraces", ItemType.Bracer);
        public static Item ShacklesoftheInvoker = new Item(335030, "Shackles of the Invoker", ItemType.Bracer);
        public static Item SlaveBonds = new Item(193685, "Slave Bonds", ItemType.Bracer);
        public static Item SteadyStrikers = new Item(193686, "Steady Strikers", ItemType.Bracer);
        public static Item StrongarmBracers = new Item(193692, "Strongarm Bracers", ItemType.Bracer);
        public static Item TragOulCoils = new Item(298119, "Trag'Oul Coils", ItemType.Bracer);
        public static Item WarzechianArmguards = new Item(298115, "Warzechian Armguards", ItemType.Bracer);

        // CeremonialDagger   
        public static Item AnessaziEdge = new Item(196250, "Anessazi Edge", ItemType.CeremonialDagger);
        public static Item DeadlyRebirth = new Item(193433, "Deadly Rebirth", ItemType.CeremonialDagger);
        public static Item LastBreath = new Item(195370, "Last Breath", ItemType.CeremonialDagger);
        public static Item LivingUmbralOath = new Item(192540, "Living Umbral Oath", ItemType.CeremonialDagger);
        public static Item ManajumasCarvingKnife = new Item(223365, "Manajuma's Carving Knife", ItemType.CeremonialDagger);
        public static Item RhenhoFlayer = new Item(271745, "Rhen'ho Flayer", ItemType.CeremonialDagger);
        public static Item StarmetalKukri = new Item(271738, "Starmetal Kukri", ItemType.CeremonialDagger);
        public static Item TheGidbinn = new Item(209246, "The Gidbinn", ItemType.CeremonialDagger);
        public static Item TheSpiderQueensGrasp = new Item(222978, "The Spider Queen’s Grasp", ItemType.CeremonialDagger);

        // Chest   
        public static Item AquilaCuirass = new Item(197203, "Aquila Cuirass", ItemType.Chest);
        public static Item ArmoroftheKindRegent = new Item(332202, "Armor of the Kind Regent", ItemType.Chest);
        public static Item AughildsRule = new Item(197193, "Aughild's Rule", ItemType.Chest);
        public static Item BlackthornesSurcoat = new Item(222456, "Blackthorne's Surcoat", ItemType.Chest);
        public static Item BornsFrozenSoul = new Item(197199, "Born's Frozen Soul", ItemType.Chest);
        public static Item BreastplateofAkkhan = new Item(358796, "Breastplate of Akkhan", ItemType.Chest);
        public static Item Chaingmail = new Item(197204, "Chaingmail", ItemType.Chest);
        public static Item Cindercoat = new Item(222455, "Cindercoat", ItemType.Chest);
        public static Item DemonsMarrow = new Item(205612, "Demon's Marrow", ItemType.Chest);
        public static Item FirebirdsBreast = new Item(358788, "Firebird's Breast", ItemType.Chest);
        public static Item Goldskin = new Item(205616, "Goldskin", ItemType.Chest);
        public static Item HeartofIron = new Item(205607, "Heart of Iron", ItemType.Chest);
        public static Item HeartoftheCrashingWave = new Item(338032, "Heart of the Crashing Wave", ItemType.Chest);
        public static Item HelltoothTunic = new Item(363088, "Helltooth Tunic", ItemType.Chest);
        public static Item ImmortalKingsEternalReign = new Item(205613, "Immortal King's Eternal Reign", ItemType.Chest);
        public static Item InnasVastExpanse = new Item(205614, "Inna's Vast Expanse", ItemType.Chest);
        public static Item JadeHarvestersPeace = new Item(338038, "Jade Harvester's Peace", ItemType.Chest);
        public static Item MantleoftheRydraelm = new Item(205609, "Mantle of the Rydraelm", ItemType.Chest);
        public static Item MaraudersCarapace = new Item(363803, "Marauder's Carapace", ItemType.Chest);
        public static Item RaekorsHeart = new Item(336984, "Raekor’s Heart", ItemType.Chest);
        public static Item ShiMizusHaori = new Item(332200, "Shi Mizu's Haori", ItemType.Chest);
        public static Item TalRashasRelentlessPursuit = new Item(211626, "Tal Rasha's Relentless Pursuit", ItemType.Chest);
        public static Item TheShadowsBane = new Item(332359, "The Shadow’s Bane", ItemType.Chest);
        public static Item TyraelsMight = new Item(205608, "Tyrael's Might", ItemType.Chest);
        public static Item VirtueInvoker = new Item(253994, "Virtue Invoker", ItemType.Chest);
        public static Item VyrsAstonishingAura = new Item(332357, "Vyr’s Astonishing Aura", ItemType.Chest);
        public static Item ZunimassasMarrow = new Item(205615, "Zunimassa's Marrow", ItemType.Chest);

        // Cloak   
        public static Item BeckonSail = new Item(223150, "Beckon Sail", ItemType.Cloak);
        public static Item Blackfeather = new Item(332206, "Blackfeather", ItemType.Cloak);
        public static Item CapeoftheDarkNight = new Item(223149, "Cape of the Dark Night", ItemType.Cloak);
        public static Item CloakofDeception = new Item(332208, "Cloak of Deception", ItemType.Cloak);
        public static Item NatalyasEmbrace = new Item(208934, "Natalya's Embrace", ItemType.Cloak);
        public static Item TheCloakoftheGarwulf = new Item(223151, "The Cloak of the Garwulf", ItemType.Cloak);

        // Crossbow   
        public static Item ArcaneBarb = new Item(194957, "Arcane Barb", ItemType.Crossbow);
        public static Item BakkanCaster = new Item(98163, "Bakkan Caster", ItemType.Crossbow);
        public static Item BurizaDoKyanon = new Item(194219, "Buriza-Do Kyanon", ItemType.Crossbow);
        public static Item DemonMachine = new Item(222286, "Demon Machine", ItemType.Crossbow);
        public static Item Hellrack = new Item(192836, "Hellrack", ItemType.Crossbow);
        public static Item PusSpitter = new Item(204874, "Pus Spitter", ItemType.Crossbow);

        // CrusaderShield   
        public static Item HallowedBulwark = new Item(299413, "Hallowed Bulwark", ItemType.CrusaderShield);
        public static Item Hellskull = new Item(299415, "Hellskull", ItemType.CrusaderShield);
        public static Item Jekangbord = new Item(299412, "Jekangbord", ItemType.CrusaderShield);
        public static Item Salvation = new Item(299418, "Salvation", ItemType.CrusaderShield);
        public static Item SublimeConviction = new Item(299416, "Sublime Conviction", ItemType.CrusaderShield);
        public static Item TheFinalWitness = new Item(299417, "The Final Witness", ItemType.CrusaderShield);

        // Dagger   
        public static Item BloodMagicEdge = new Item(195655, "Blood-Magic Edge", ItemType.Dagger);
        public static Item EnviousBlade = new Item(271732, "Envious Blade", ItemType.Dagger);
        public static Item Kill = new Item(192579, "Kill", ItemType.Dagger);
        public static Item PigSticker = new Item(221313, "Pig Sticker", ItemType.Dagger);
        public static Item TheBarber = new Item(195174, "The Barber", ItemType.Dagger);
        public static Item Wizardspike = new Item(219329, "Wizardspike", ItemType.Dagger);

        // Daibo   
        public static Item Balance = new Item(195145, "Balance", ItemType.Daibo);
        public static Item FlyingDragon = new Item(197065, "Flying Dragon", ItemType.Daibo);
        public static Item IncenseTorchoftheGrandTemple = new Item(192342, "Incense Torch of the Grand Temple", ItemType.Daibo);
        public static Item InnasReach = new Item(212208, "Inna's Reach", ItemType.Daibo);
        public static Item LaiYuisPersuader = new Item(209214, "Lai Yui's Persuader", ItemType.Daibo);
        public static Item RozpedinsForce = new Item(196880, "Rozpedin's Force", ItemType.Daibo);
        public static Item TheFlowofEternity = new Item(197072, "The Flow of Eternity", ItemType.Daibo);
        public static Item ThePaddle = new Item(197068, "The Paddle", ItemType.Daibo);

        // FistWeapon   
        public static Item CrystalFist = new Item(175939, "Crystal Fist", ItemType.FistWeapon);
        public static Item DemonClaw = new Item(193459, "Demon Claw", ItemType.FistWeapon);
        public static Item Fleshrake = new Item(145850, "Fleshrake", ItemType.FistWeapon);
        public static Item LogansClaw = new Item(145849, "Logan's Claw", ItemType.FistWeapon);
        public static Item RabidStrike = new Item(196472, "Rabid Strike", ItemType.FistWeapon);
        public static Item ShenlongsFistofLegend = new Item(208996, "Shenlong's Fist of Legend", ItemType.FistWeapon);
        public static Item ShenlongsRelentlessAssault = new Item(208898, "Shenlong's Relentless Assault", ItemType.FistWeapon);
        public static Item SledgeFist = new Item(175938, "Sledge Fist", ItemType.FistWeapon);
        public static Item TheFistofAzTurrasq = new Item(175937, "The Fist of Az'Turrasq", ItemType.FistWeapon);

        // Flail   
        public static Item BalefulRemnant = new Item(299435, "Baleful Remnant", ItemType.Flail);
        public static Item Darklight = new Item(299428, "Darklight", ItemType.Flail);
        public static Item FateoftheFell = new Item(299436, "Fate of the Fell", ItemType.Flail);
        public static Item GoldenFlense = new Item(299437, "Golden Flense", ItemType.Flail);
        public static Item GoldenScourge = new Item(299419, "Golden Scourge", ItemType.Flail);
        public static Item InviolableFaith = new Item(299429, "Inviolable Faith", ItemType.Flail);
        public static Item JustiniansMercy = new Item(299424, "Justinian's Mercy", ItemType.Flail);
        public static Item KassarsRetribution = new Item(299426, "Kassar's Retribution", ItemType.Flail);
        public static Item Swiftmount = new Item(299425, "Swiftmount", ItemType.Flail);
        public static Item TheMortalDrama = new Item(299431, "The Mortal Drama", ItemType.Flail);

        // FollowerSpecial   
        public static Item EnchantingFavor = new Item(366968, "Enchanting Favor", ItemType.FollowerSpecial);
        public static Item HandoftheProphet = new Item(366980, "Hand of the Prophet", ItemType.FollowerSpecial);
        public static Item RelicofAkarat = new Item(366969, "Relic of Akarat", ItemType.FollowerSpecial);
        public static Item RibaldEtchings = new Item(366971, "Ribald Etchings", ItemType.FollowerSpecial);
        public static Item SkeletonKey = new Item(366970, "Skeleton Key", ItemType.FollowerSpecial);
        public static Item SmokingThurible = new Item(366979, "Smoking Thurible", ItemType.FollowerSpecial);

        // Gloves   
        public static Item AshearasWard = new Item(205636, "Asheara's Ward", ItemType.Gloves);
        public static Item CainsScribe = new Item(197210, "Cain's Scribe", ItemType.Gloves);
        public static Item FirebirdsTalons = new Item(358789, "Firebird's Talons", ItemType.Gloves);
        public static Item FistsofThunder = new Item(338033, "Fists of Thunder", ItemType.Gloves);
        public static Item ForgeHarm = new Item(341342, "Forge Harm", ItemType.Gloves);
        //public static Item ForgeHarm = new Item(224051, "Forge Harm", ItemType.Gloves);
        //public static Item ForgeHarm = new Item(197193, "Forge Harm", ItemType.Gloves);
        public static Item Frostburn = new Item(197205, "Frostburn", ItemType.Gloves);
        public static Item GauntletsofAkkhan = new Item(358798, "Gauntlets of Akkhan", ItemType.Gloves);
        public static Item GladiatorGauntlets = new Item(205635, "Gladiator Gauntlets", ItemType.Gloves);
        public static Item GlovesofWorship = new Item(332344, "Gloves of Worship", ItemType.Gloves);
        public static Item HelltoothGauntlets = new Item(363094, "Helltooth Gauntlets", ItemType.Gloves);
        public static Item ImmortalKingsIrons = new Item(205631, "Immortal King's Irons", ItemType.Gloves);
        public static Item JadeHarvestersMercy = new Item(338039, "Jade Harvester's Mercy", ItemType.Gloves);
        public static Item Magefist = new Item(197206, "Magefist", ItemType.Gloves);
        public static Item MaraudersGloves = new Item(336992, "Marauder's Gloves", ItemType.Gloves);
        public static Item PendersPurchase = new Item(197207, "Penders Purchase", ItemType.Gloves);
        public static Item PrideoftheInvoker = new Item(335027, "Pride of the Invoker", ItemType.Gloves);
        public static Item PulloftheEarth = new Item(340523, "Pull of the Earth", ItemType.Gloves);
        public static Item RaekorsWraps = new Item(336985, "Raekor’s Wraps", ItemType.Gloves);
        public static Item StArchewsGage = new Item(332172, "St. Archew's Gage", ItemType.Gloves);
        public static Item StoneGauntlets = new Item(205640, "Stone Gauntlets", ItemType.Gloves);
        public static Item SunwukosPaws = new Item(336172, "Sunwuko's Paws", ItemType.Gloves);
        public static Item TaskerandTheo = new Item(205642, "Tasker and Theo", ItemType.Gloves);
        public static Item VyrsGraspingGauntlets = new Item(346210, "Vyr’s Grasping Gauntlets", ItemType.Gloves);

        // HandCrossbow   
        public static Item BalefireCaster = new Item(192528, "Balefire Caster", ItemType.HandCrossbow);
        public static Item Blitzbolter = new Item(195078, "Blitzbolter", ItemType.HandCrossbow);
        public static Item DanettasRevenge = new Item(211749, "Danetta's Revenge", ItemType.HandCrossbow);
        public static Item Dawn = new Item(196409, "Dawn", ItemType.HandCrossbow);
        public static Item Helltrapper = new Item(271914, "Helltrapper", ItemType.HandCrossbow);
        public static Item Izzuccob = new Item(192467, "Izzuccob", ItemType.HandCrossbow);

        // Helm   
        public static Item AndarielsVisage = new Item(198014, "Andariel's Visage", ItemType.Helm);
        public static Item BlindFaith = new Item(197037, "Blind Faith", ItemType.Helm);
        public static Item BrokenCrown = new Item(220630, "Broken Crown", ItemType.Helm);
        public static Item CrownoftheInvoker = new Item(335028, "Crown of the Invoker", ItemType.Helm);
        public static Item DeathseersCowl = new Item(298146, "Deathseer's Cowl", ItemType.Helm);
        public static Item EyesoftheEarth = new Item(340528, "Eyes of the Earth", ItemType.Helm);
        public static Item FirebirdsPlume = new Item(358791, "Firebird's Plume", ItemType.Helm);
        public static Item HelmofAkkhan = new Item(358799, "Helm of Akkhan", ItemType.Helm);
        public static Item ImmortalKingsTriumph = new Item(210265, "Immortal King's Triumph", ItemType.Helm);
        public static Item JadeHarvestersWisdom = new Item(338040, "Jade Harvester's Wisdom", ItemType.Helm);
        public static Item LeoricsCrown = new Item(196024, "Leoric's Crown", ItemType.Helm);
        public static Item MaraudersVisage = new Item(336994, "Marauder's Visage", ItemType.Helm);
        public static Item MaskofJeramSetHelm = new Item(369016, "Mask of Jeram", ItemType.Helm);
        public static Item MaskoftheSearingSky = new Item(338034, "Mask of the Searing Sky", ItemType.Helm);
        public static Item MempoofTwilight = new Item(223577, "Mempo of Twilight", ItemType.Helm);
        public static Item NatalyasSight = new Item(210851, "Natalya's Sight", ItemType.Helm);
        public static Item PridesFall = new Item(298147, "Pride's Fall", ItemType.Helm);
        public static Item RaekorsWill = new Item(336988, "Raekor’s Will", ItemType.Helm);
        public static Item SkullofResonance = new Item(220549, "Skull of Resonance", ItemType.Helm);
        public static Item SunwukosCrown = new Item(336173, "Sunwuko's Crown", ItemType.Helm);
        public static Item TalRashasGuiseofWisdom = new Item(211531, "Tal Rasha's Guise of Wisdom", ItemType.Helm);

        // Legs   
        public static Item AshearasPace = new Item(209054, "Asheara's Pace", ItemType.Legs);
        public static Item BlackthornesJoustingMail = new Item(222477, "Blackthorne's Jousting Mail", ItemType.Legs);
        public static Item CainsHabit = new Item(197218, "Cain's Habit", ItemType.Legs);
        public static Item CaptainCrimsonsThrust = new Item(197214, "Captain Crimson's Thrust", ItemType.Legs);
        public static Item CuissesofAkkhan = new Item(358800, "Cuisses of Akkhan", ItemType.Legs);
        public static Item DeathsBargain = new Item(332205, "Death's Bargain", ItemType.Legs);
        public static Item DepthDiggers = new Item(197216, "Depth Diggers", ItemType.Legs);
        public static Item FirebirdsDown = new Item(358790, "Firebird's Down", ItemType.Legs);
        public static Item HammerJammers = new Item(209059, "Hammer Jammers", ItemType.Legs);
        public static Item HelltoothLegGuards = new Item(340522, "Helltooth Leg Guards", ItemType.Legs);
        public static Item HexingPantsofMrYan = new Item(332204, "Hexing Pants of Mr. Yan", ItemType.Legs);
        public static Item InnasTemperance = new Item(205646, "Inna's Temperance", ItemType.Legs);
        public static Item JadeHarvestersCourage = new Item(338041, "Jade Harvester's Courage", ItemType.Legs);
        public static Item MaraudersEncasement = new Item(336993, "Marauder's Encasement", ItemType.Legs);
        public static Item PoxFaulds = new Item(197220, "Pox Faulds", ItemType.Legs);
        public static Item RaekorsBreeches = new Item(336986, "Raekor’s Breeches", ItemType.Legs);
        public static Item ScalesoftheDancingSerpent = new Item(338035, "Scales of the Dancing Serpent", ItemType.Legs);
        public static Item SwampLandWaders = new Item(209057, "Swamp Land Waders", ItemType.Legs);
        public static Item TheShadowsCoil = new Item(332361, "The Shadow’s Coil", ItemType.Legs);
        public static Item VyrsFantasticFinery = new Item(332360, "Vyr’s Fantastic Finery", ItemType.Legs);
        public static Item WeightoftheEarth = new Item(340521, "Weight of the Earth", ItemType.Legs);

        // Mace   
        public static Item ArthefsSparkofLife = new Item(59633, "Arthef’s Spark of Life", ItemType.Mace);
        public static Item Crushbane = new Item(99227, "Crushbane", ItemType.Mace);
        public static Item Devastator = new Item(188177, "Devastator", ItemType.Mace);
        public static Item EchoingFury = new Item(188181, "Echoing Fury", ItemType.Mace);
        public static Item JacesHammerofVigilance = new Item(271648, "Jace’s Hammer of Vigilance", ItemType.Mace);
        public static Item MadMonarchsScepter = new Item(271663, "Mad Monarch's Scepter", ItemType.Mace);
        public static Item Nailbiter = new Item(188158, "Nailbiter", ItemType.Mace);
        public static Item Neanderthal = new Item(102665, "Neanderthal", ItemType.Mace);
        public static Item Nutcracker = new Item(188169, "Nutcracker", ItemType.Mace);
        public static Item OdynSon = new Item(188185, "Odyn Son", ItemType.Mace);
        public static Item SchaefersHammer = new Item(197717, "Schaefer's Hammer", ItemType.Mace);
        public static Item Skywarden = new Item(190840, "Skywarden", ItemType.Mace);
        public static Item SledgeofAthskeleng = new Item(190866, "Sledge of Athskeleng", ItemType.Mace);
        public static Item Solanium = new Item(271662, "Solanium", ItemType.Mace);
        public static Item Soulsmasher = new Item(271671, "Soulsmasher", ItemType.Mace);
        public static Item SunKeeper = new Item(188173, "Sun Keeper", ItemType.Mace);
        public static Item Sunder = new Item(190868, "Sunder", ItemType.Mace);
        public static Item SupremacyNexus = new Item(191584, "Supremacy Nexus", ItemType.Mace);
        public static Item TelrandensHand = new Item(188189, "Telranden's Hand", ItemType.Mace);
        public static Item TheFurnace = new Item(271666, "The Furnace", ItemType.Mace);
        public static Item WrathoftheBoneKing = new Item(191584, "Wrath of the Bone King", ItemType.Mace);

        // MightyBelt   
        public static Item AgelessMight = new Item(193675, "Ageless Might", ItemType.MightyBelt);
        public static Item ChilaniksChain = new Item(298133, "Chilanik’s Chain", ItemType.MightyBelt);
        public static Item DreadIron = new Item(193672, "Dread Iron", ItemType.MightyBelt);
        public static Item GirdleofGiants = new Item(212232, "Girdle of Giants", ItemType.MightyBelt);
        public static Item ImmortalKingsTribalBinding = new Item(212235, "Immortal King's Tribal Binding", ItemType.MightyBelt);
        public static Item KotuursBrace = new Item(193674, "Kotuur's Brace", ItemType.MightyBelt);
        public static Item Lamentation = new Item(212234, "Lamentation", ItemType.MightyBelt);
        public static Item PrideofCassius = new Item(193673, "Pride of Cassius", ItemType.MightyBelt);
        public static Item TheUndisputedChampion = new Item(193676, "The Undisputed Champion", ItemType.MightyBelt);

        // MightyWeapon   
        public static Item AmbosPride = new Item(193486, "Ambo's Pride", ItemType.MightyWeapon);
        public static Item BastionsRevered = new Item(195690, "Bastion's Revered", ItemType.MightyWeapon);
        public static Item BladeoftheWarlord = new Item(193611, "Blade of the Warlord", ItemType.MightyWeapon);
        public static Item BulKathossSolemnVow = new Item(208771, "Bul-Kathos's Solemn Vow", ItemType.MightyWeapon);
        public static Item BulKathossWarriorBlood = new Item(208775, "Bul-Kathos's Warrior Blood", ItemType.MightyWeapon);
        public static Item FjordCutter = new Item(192105, "Fjord Cutter", ItemType.MightyWeapon);
        public static Item ImmortalKingsBoulderBreaker = new Item(210678, "Immortal King's Boulder Breaker", ItemType.MightyWeapon);
        public static Item MadawcsSorrow = new Item(272012, "Madawc's Sorrow", ItemType.MightyWeapon);
        public static Item NightsReaping = new Item(192705, "Night's Reaping", ItemType.MightyWeapon);
        public static Item TheGavelofJudgment = new Item(193657, "The Gavel of Judgment", ItemType.MightyWeapon);
        public static Item WaroftheDead = new Item(196308, "War of the Dead", ItemType.MightyWeapon);

        // Mojo   
        public static Item GazingDemise = new Item(194995, "Gazing Demise", ItemType.Mojo);
        public static Item Homunculus = new Item(194991, "Homunculus", ItemType.Mojo);
        public static Item ManajumasGoryFetch = new Item(210993, "Manajuma's Gory Fetch", ItemType.Mojo);
        public static Item ShukranisTriumph = new Item(272070, "Shukrani’s Triumph", ItemType.Mojo);
        public static Item Spite = new Item(194988, "Spite", ItemType.Mojo);
        public static Item ThingoftheDeep = new Item(192468, "Thing of the Deep", ItemType.Mojo);
        public static Item UhkapianSerpent = new Item(191278, "Uhkapian Serpent", ItemType.Mojo);
        public static Item ZunimassasStringofSkulls = new Item(216525, "Zunimassa's String of Skulls", ItemType.Mojo);

        // Orb   
        public static Item ChantodosForce = new Item(212277, "Chantodo's Force", ItemType.Orb);
        public static Item CosmicStrand = new Item(195127, "Cosmic Strand", ItemType.Orb);
        public static Item FirebirdsEye = new Item(358819, "Firebird's Eye", ItemType.Orb);
        public static Item LightofGrace = new Item(272038, "Light of Grace", ItemType.Orb);
        public static Item Mirrorball = new Item(272022, "Mirrorball", ItemType.Orb);
        public static Item MykensBallofHate = new Item(272037, "Myken's Ball of Hate", ItemType.Orb);
        public static Item TalRashasUnwaveringGlare = new Item(212780, "Tal Rasha's Unwavering Glare", ItemType.Orb);
        public static Item TheOculus = new Item(192320, "The Oculus", ItemType.Orb);
        public static Item Triumvirate = new Item(195325, "Triumvirate", ItemType.Orb);
        public static Item WinterFlurry = new Item(184199, "Winter Flurry", ItemType.Orb);

        // Polearm   
        public static Item BovineBardiche = new Item(272056, "Bovine Bardiche", ItemType.Polearm);
        public static Item HeartSlaughter = new Item(192569, "Heart Slaughter", ItemType.Polearm);
        public static Item PledgeofCaldeum = new Item(196570, "Pledge of Caldeum", ItemType.Polearm);
        public static Item Standoff = new Item(191570, "Standoff", ItemType.Polearm);
        public static Item Vigilance = new Item(195491, "Vigilance", ItemType.Polearm);

        // Potion   
        public static Item BottomlessPotionofKulleAid = new Item(344093, "Bottomless Potion of Kulle-Aid", ItemType.Potion);
        public static Item BottomlessPotionofMutilation = new Item(342824, "Bottomless Potion of Mutilation", ItemType.Potion);
        public static Item BottomlessPotionofRegeneration = new Item(341343, "Bottomless Potion of Regeneration", ItemType.Potion);
        public static Item BottomlessPotionoftheDiamond = new Item(341342, "Bottomless Potion of the Diamond", ItemType.Potion);
        public static Item BottomlessPotionoftheLeech = new Item(342823, "Bottomless Potion of the Leech", ItemType.Potion);
        public static Item BottomlessPotionoftheTower = new Item(341333, "Bottomless Potion of the Tower", ItemType.Potion);

        // Quiver   
        public static Item ArchfiendArrows = new Item(197626, "Archfiend Arrows", ItemType.Quiver);
        public static Item BombadiersRucksack = new Item(298171, "Bombadier's Rucksack", ItemType.Quiver);
        public static Item DeadMansLegacy = new Item(197630, "Dead Man's Legacy", ItemType.Quiver);
        public static Item EmimeisDuffel = new Item(298172, "Emimei’s Duffel", ItemType.Quiver);
        public static Item FletchersPride = new Item(197629, "Fletcher's Pride", ItemType.Quiver);
        public static Item SilverStarPiercers = new Item(197628, "Silver Star Piercers", ItemType.Quiver);
        public static Item SinSeekers = new Item(197625, "Sin Seekers", ItemType.Quiver);
        public static Item TheNinthCirriSatchel = new Item(298170, "The Ninth Cirri Satchel", ItemType.Quiver);

        // Ring   
        public static Item AvariceBand = new Item(298095, "Avarice Band", ItemType.Ring);
        public static Item BandofHollowWhispers = new Item(197834, "Band of Hollow Whispers", ItemType.Ring);
        public static Item BandoftheRueChambers = new Item(298093, "Band of the Rue Chambers", ItemType.Ring);
        public static Item BandofUntoldSecrets = new Item(212602, "Band of Untold Secrets", ItemType.Ring);
        public static Item BrokenPromises = new Item(212589, "Broken Promises", ItemType.Ring);
        public static Item BulKathossWeddingBand = new Item(212603, "Bul-Kathos's Wedding Band", ItemType.Ring);
        public static Item EternalUnion = new Item(212601, "Eternal Union", ItemType.Ring);
        public static Item Focus = new Item(332209, "Focus", ItemType.Ring);
        public static Item HellfireRing = new Item(260327, "Hellfire Ring", ItemType.Ring);
        public static Item JusticeLantern = new Item(212590, "Justice Lantern", ItemType.Ring);
        public static Item LeoricsSignet = new Item(197835, "Leoric's Signet", ItemType.Ring);
        public static Item LitanyoftheUndaunted = new Item(212651, "Litany of the Undaunted", ItemType.Ring);
        public static Item ManaldHeal = new Item(212546, "Manald Heal", ItemType.Ring);
        public static Item Nagelring = new Item(212586, "Nagelring", ItemType.Ring);
        public static Item NatalyasReflection = new Item(212545, "Natalya's Reflection", ItemType.Ring);
        public static Item ObsidianRingoftheZodiac = new Item(212588, "Obsidian Ring of the Zodiac", ItemType.Ring);
        public static Item OculusRing = new Item(212648, "Oculus Ring", ItemType.Ring);
        public static Item PandemoniumLoop = new Item(298096, "Pandemonium Loop", ItemType.Ring);
        public static Item PuzzleRing = new Item(197837, "Puzzle Ring", ItemType.Ring);
        public static Item RechelsRingofLarceny = new Item(298091, "Rechel's Ring of Larceny", ItemType.Ring);
        public static Item Restraint = new Item(332210, "Restraint", ItemType.Ring);
        public static Item RingofRoyalGrandeur = new Item(298094, "Ring of Royal Grandeur", ItemType.Ring);
        public static Item RogarsHugeStone = new Item(298090, "Rogar's Huge Stone", ItemType.Ring);
        public static Item SkullGrasp = new Item(212618, "Skull Grasp", ItemType.Ring);
        public static Item StolenRing = new Item(197839, "Stolen Ring", ItemType.Ring);
        public static Item StoneofJordan = new Item(212582, "Stone of Jordan", ItemType.Ring);
        public static Item TheCompassRose = new Item(212587, "The Compass Rose", ItemType.Ring);
        public static Item TheTallMansFinger = new Item(298088, "The Tall Man's Finger", ItemType.Ring);
        public static Item TheWailingHost = new Item(212650, "The Wailing Host", ItemType.Ring);
        public static Item Unity = new Item(212581, "Unity", ItemType.Ring);
        public static Item Wyrdward = new Item(298089, "Wyrdward", ItemType.Ring);
        public static Item ZunimassasPox = new Item(212579, "Zunimassa's Pox", ItemType.Ring);

        // Shield   
        public static Item CovensCriterion = new Item(298191, "Coven’s Criterion", ItemType.Shield);
        public static Item DefenderofWestmarch = new Item(298182, "Defender of Westmarch", ItemType.Shield);
        public static Item Denial = new Item(152666, "Denial", ItemType.Shield);
        public static Item EberliCharo = new Item(298186, "Eberli Charo", ItemType.Shield);
        public static Item FreezeofDeflection = new Item(61550, "Freeze of Deflection", ItemType.Shield);
        public static Item IvoryTower = new Item(197478, "Ivory Tower", ItemType.Shield);
        public static Item LidlessWall = new Item(195389, "Lidless Wall", ItemType.Shield);
        public static Item Stormshield = new Item(192484, "Stormshield", ItemType.Shield);
        public static Item VoToyiasSpiker = new Item(298188, "Vo'Toyias Spiker", ItemType.Shield);

        // Shoulder   
        public static Item AshearasCustodian = new Item(225132, "Asheara's Custodian", ItemType.Shoulder);
        public static Item AughildsPower = new Item(224051, "Aughild's Power", ItemType.Shoulder);
        public static Item BornsPrivilege = new Item(222948, "Born's Privilege", ItemType.Shoulder);
        public static Item BurdenoftheInvoker = new Item(335029, "Burden of the Invoker", ItemType.Shoulder);
        public static Item DeathWatchMantle = new Item(200310, "Death Watch Mantle", ItemType.Shoulder);
        public static Item DemonsAileron = new Item(224397, "Demon's Aileron", ItemType.Shoulder);
        public static Item FirebirdsPinions = new Item(358792, "Firebird's Pinions", ItemType.Shoulder);
        public static Item HelltoothMantle = new Item(340525, "Helltooth Mantle", ItemType.Shoulder);
        public static Item HomingPads = new Item(198573, "Homing Pads", ItemType.Shoulder);
        public static Item JadeHarvestersJoy = new Item(338042, "Jade Harvester's Joy", ItemType.Shoulder);
        public static Item MantleoftheUpsideDownSinners = new Item(338036, "Mantle of the Upside-Down Sinners", ItemType.Shoulder);
        public static Item MaraudersSpines = new Item(336996, "Marauder's Spines", ItemType.Shoulder);
        public static Item PauldronsofAkkhan = new Item(358801, "Pauldrons of Akkhan", ItemType.Shoulder);
        public static Item PauldronsoftheSkeletonKing = new Item(298164, "Pauldrons of the Skeleton King", ItemType.Shoulder);
        public static Item ProfanePauldrons = new Item(298158, "Profane Pauldrons", ItemType.Shoulder);
        public static Item RaekorsBurden = new Item(336989, "Raekor’s Burden", ItemType.Shoulder);
        public static Item SpauldersofZakara = new Item(298163, "Spaulders of Zakara", ItemType.Shoulder);
        public static Item SpiresoftheEarth = new Item(340526, "Spires of the Earth", ItemType.Shoulder);
        public static Item SunwukosBalance = new Item(336175, "Sunwuko's Balance", ItemType.Shoulder);
        public static Item VileWard = new Item(201325, "Vile Ward", ItemType.Shoulder);

        // Spear   
        public static Item ArreatsLaw = new Item(191446, "Arreat's Law", ItemType.Spear);
        public static Item EmpyreanMessenger = new Item(194241, "Empyrean Messenger", ItemType.Spear);
        public static Item Scrimshaw = new Item(197095, "Scrimshaw", ItemType.Spear);
        public static Item TheThreeHundredthSpear = new Item(196638, "The Three Hundredth Spear", ItemType.Spear);

        // SpiritStone   
        public static Item EyeofPeshkov = new Item(299464, "Eye of Peshkov", ItemType.SpiritStone);
        public static Item GyanaNaKashu = new Item(222169, "Gyana Na Kashu", ItemType.SpiritStone);
        public static Item InnasRadiance = new Item(222307, "Inna's Radiance", ItemType.SpiritStone);
        public static Item KekegisUnbreakableSpirit = new Item(299461, "Kekegi's Unbreakable Spirit", ItemType.SpiritStone);
        public static Item SeeNoEvil = new Item(222171, "See No Evil", ItemType.SpiritStone);
        public static Item TheEyeoftheStorm = new Item(222170, "The Eye of the Storm", ItemType.SpiritStone);
        public static Item TheLawsofSeph = new Item(299454, "The Laws of Seph", ItemType.SpiritStone);
        public static Item TheMindsEye = new Item(222172, "The Mind's Eye", ItemType.SpiritStone);
        public static Item TzoKrinsGaze = new Item(222305, "Tzo Krin's Gaze", ItemType.SpiritStone);

        // Staff   
        public static Item AutumnsCall = new Item(184228, "Autumn's Call", ItemType.Staff);
        public static Item MalothsFocus = new Item(193832, "Maloth's Focus", ItemType.Staff);
        public static Item MarkofTheMagi = new Item(59612, "Mark of The Magi", ItemType.Staff);
        public static Item TheBrokenStaff = new Item(59601, "The Broken Staff", ItemType.Staff);
        public static Item TheGrandVizier = new Item(192167, "The Grand Vizier", ItemType.Staff);
        public static Item TheTormentor = new Item(193066, "The Tormentor", ItemType.Staff);
        public static Item ValtheksRebuke = new Item(271773, "Valthek's Rebuke", ItemType.Staff);
        public static Item Wormwood = new Item(195407, "Wormwood", ItemType.Staff);

        // Sword   
        public static Item Azurewrath = new Item(192511, "Azurewrath", ItemType.Sword);
        public static Item BladeofProphecy = new Item(184184, "Blade of Prophecy", ItemType.Sword);
        public static Item CamsRebuttal = new Item(271644, "Cam's Rebuttal", ItemType.Sword);
        public static Item DevilTongue = new Item(189552, "Devil Tongue", ItemType.Sword);
        public static Item Doombringer = new Item(185397, "Doombringer", ItemType.Sword);
        public static Item Exarian = new Item(271617, "Exarian", ItemType.Sword);
        public static Item FaithfulMemory = new Item(198960, "Faithful Memory", ItemType.Sword);
        public static Item Fulminator = new Item(271631, "Fulminator", ItemType.Sword);
        public static Item GiftofSilaria = new Item(271630, "Gift of Silaria", ItemType.Sword);
        public static Item GriswoldsPerfection = new Item(270977, "Griswold's Perfection", ItemType.Sword);
        public static Item LittleRogue = new Item(313291, "Little Rogue", ItemType.Sword);
        public static Item Maximus = new Item(184187, "Maximus", ItemType.Sword);
        public static Item MonsterHunter = new Item(115140, "Monster Hunter", ItemType.Sword);
        public static Item Rimeheart = new Item(271636, "Rimeheart", ItemType.Sword);
        public static Item Scourge = new Item(181511, "Scourge", ItemType.Sword);
        public static Item Sever = new Item(115141, "Sever", ItemType.Sword);
        public static Item ShardofHate = new Item(376463, "Shard of Hate", ItemType.Sword);
        public static Item Skycutter = new Item(182347, "Skycutter", ItemType.Sword);
        public static Item StalgardsDecimator = new Item(271639, "Stalgard's Decimator", ItemType.Sword);
        public static Item TheAncientBonesaberofZumakalis = new Item(194481, "The Ancient Bonesaber of Zumakalis", ItemType.Sword);
        public static Item TheGrandfather = new Item(190360, "The Grandfather", ItemType.Sword);
        public static Item TheSlanderer = new Item(313290, "The Slanderer", ItemType.Sword);
        public static Item TheSultanofBlindingSand = new Item(184190, "The Sultan of Blinding Sand", ItemType.Sword);
        public static Item TheZweihander = new Item(59665, "The Zweihander", ItemType.Sword);
        public static Item ThunderfuryBlessedBladeoftheWindseeker = new Item(229716, "Thunderfury, Blessed Blade of the Windseeker", ItemType.Sword);
        public static Item Warmonger = new Item(181495, "Warmonger", ItemType.Sword);
        public static Item Wildwood = new Item(270978, "Wildwood", ItemType.Sword);

        // VoodooMask   
        public static Item Carnevil = new Item(299442, "Carnevil", ItemType.VoodooMask);
        public static Item MaskofJeram = new Item(299443, "Mask of Jeram", ItemType.VoodooMask);
        public static Item Quetzalcoatl = new Item(204136, "Quetzalcoatl", ItemType.VoodooMask);
        public static Item SplitTusk = new Item(221167, "Split Tusk", ItemType.VoodooMask);
        public static Item TheGrinReaper = new Item(221166, "The Grin Reaper", ItemType.VoodooMask);
        public static Item TiklandianVisage = new Item(221382, "Tiklandian Visage", ItemType.VoodooMask);
        public static Item VisageofGiyua = new Item(221168, "Visage of Giyua", ItemType.VoodooMask);
        public static Item ZunimassasVision = new Item(221202, "Zunimassa's Vision", ItemType.VoodooMask);

        // Wand   
        public static Item Atrophy = new Item(182081, "Atrophy", ItemType.Wand);
        public static Item BlackhandKey = new Item(193355, "Blackhand Key", ItemType.Wand);
        public static Item ChantodosWill = new Item(210479, "Chantodo's Will", ItemType.Wand);
        public static Item FragmentofDestiny = new Item(181995, "Fragment of Destiny", ItemType.Wand);
        public static Item GestureofOrpheus = new Item(182071, "Gesture of Orpheus", ItemType.Wand);
        public static Item SerpentsSparker = new Item(272084, "Serpent's Sparker", ItemType.Wand);
        public static Item SloraksMadness = new Item(181982, "Slorak's Madness", ItemType.Wand);
        public static Item Starfire = new Item(182074, "Starfire", ItemType.Wand);
        public static Item WandofWoh = new Item(272086, "Wand of Woh", ItemType.Wand);

        // WizardHat   
        public static Item ArchmagesVicalyke = new Item(299471, "Archmage's Vicalyke", ItemType.WizardHat);
        public static Item StormCrow = new Item(220694, "Storm Crow", ItemType.WizardHat);
        public static Item TheMagistrate = new Item(325579, "The Magistrate", ItemType.WizardHat);
        public static Item TheSwami = new Item(218681, "The Swami", ItemType.WizardHat);
        public static Item VelvetCamaral = new Item(299472, "Velvet Camaral", ItemType.WizardHat);

        #region Legacy Items (not from diacollector)

        public static Item SagesPurchase = new Item(205632, "Sage's Purchase", ItemType.Gloves);
        public static Item CainsScrivener = new Item(197210, "Cain's Scrivener", ItemType.Gloves);
        public static Item BornsFuriousWrath = new Item(223408, "Born's Furious Wrath", ItemType.Sword);
        public static Item AughildsSpike = new Item(223972, "Aughild's Spike", ItemType.Helm);
        public static Item SagesApogee = new Item(221624, "Sage's Apogee", ItemType.Helm);
        public static Item GuardiansGaze = new Item(221518, "Guardian's Gaze", ItemType.Helm);
        public static Item CainsInsight = new Item(222559, "Cain's Insight", ItemType.Helm);
        public static Item DemonsPlate = new Item(205644, "Demon's Plate", ItemType.Legs);
        public static Item WonKhimLau = new Item(145851, "Won Khim Lau", ItemType.FistWeapon);
        public static Item HallowedHold = new Item(223526, "Hallowed Hold", ItemType.FistWeapon);
        public static Item Jawbreaker = new Item(271957, "Jawbreaker", ItemType.FistWeapon);
        public static Item GuardiansAversion = new Item(222981, "Guardian's Aversion", ItemType.Bracer);
        public static Item StaffofKyro = new Item(271749, "Staff of Kyro", ItemType.Daibo);
        public static Item HallowedBreach = new Item(223461, "Hallowed Breach", ItemType.Axe);
        public static Item AhavarionSpearofLycander = new Item(271768, "Ahavarion Spear of Lycander", ItemType.Staff);
        public static Item FuryoftheVanishedPeak = new Item(195138, "Fury of the Vanished Peak", ItemType.MightyWeapon);
        public static Item HallowedNemesis = new Item(223627, "Hallowed Nemesis", ItemType.MightyWeapon);
        public static Item HallowedBaton = new Item(224184, "Hallowed Baton", ItemType.Wand);
        public static Item DarkMagesShade = new Item(224908, "Dark Mage's Shade", ItemType.WizardHat);
        public static Item HallowedBarricade = new Item(223758, "Hallowed Barricade", ItemType.Shield);
        public static Item HallowedCondemnation = new Item(223763, "Hallowed Condemnation", ItemType.HandCrossbow);
        public static Item HallowedSufferance = new Item(223396, "Hallowed Sufferance", ItemType.CeremonialDagger);

        #endregion

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
