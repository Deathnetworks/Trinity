using System;
using System.Collections.Generic;
using Zeta.Common;
using Zeta.Common.Plugins;
using Zeta.Internals.Actors;
namespace GilesTrinity
{
    public partial class GilesTrinity : IPlugin
    {
        public static readonly HashSet<int> BossLevelAreaIDs = new HashSet<int> {60194, 130163, 60714, 19789, 62726, 90881, 195268, 58494, 81178, 60757, 111232, 112580, 119656, 111516, 143648, 215396, 119882, 109563, 153669, 215235, 55313, 60193, };

        /// <summary>
        /// This list is used when an actor has an attribute BuffVisualEffect=1, e.g. fire floors in The Butcher arena
        /// </summary>
        internal static HashSet<int> hashAvoidanceBuffSNOList = new HashSet<int>
        {
            // Butcher Floor Panels
            201454, 201464, 201426, 201438, 200969, 201423, 201242,
            // A1 Halls of Agony Fire walls
            89578, 89579,

        };

        /// <summary>
        /// A list of all the SNO's to avoid - you could add
        /// </summary>
        internal static HashSet<int> hashAvoidanceSNOList = new HashSet<int>
        {
            // Arcane        Arcane 2      Desecrator   Poison Tree    Molten Core   Molten Core 2   Molten Trail   Plague Cloud   Ice Balls
            219702,          221225,       84608,       5482,6578,     4803, 4804,   224225, 247987, 95868,         108869,        402, 223675,
            // Bees-Wasps    Plague-Hands  Azmo Pools   Azmo fireball  Azmo bodies   Belial 1       Belial 2
            5212,            3865,         123124,      123842,        123839,       161822,        161833,
            // Sha-Ball      Mol Ball      Mage Fire    Diablo Prison  Diablo Meteor Ice-trail
            4103,            160154,       432,         168031,        214845,       260377,
            // wall of fire
            199997, 199998,
            // Zolt Bubble	 Zolt Twister  Ghom Gas 	Maghda Proj
            185924,			 139741,	   93837,		166686,
            // Diablo Ring of Fire
            226350, 226525,
        };

        /// <summary>
        /// A list of SNO's that are projectiles (so constantly look for new locations while avoiding)
        /// </summary>
        internal static HashSet<int> hashAvoidanceSNOProjectiles = new HashSet<int>
            {
                // Bees-Wasps  Sha-Ball   Mol Ball   Azmo fireball	Zolt Twister	Maghda Projectile   Succubus Stars  Diablo Expanding Fire           Diablo Lightning Breath
                5212,          4103,      160154,    123842,		139741,			166686,             164829,         185999, 196526, 136533
            };

        /*
         * Combat-related dictionaries/defaults
         */

        /// <summary>
        /// A special list of things *NOT* to use whirlwind on (eg because they move too quick/WW is ineffective on)
        /// </summary>
        internal static HashSet<int> hashActorSNOWhirlwindIgnore = new HashSet<int> {
            4304, 5984, 5985, 5987, 5988,
         };

        /// <summary>
        /// Very fast moving mobs (eg wasps), for special skill-selection decisions
        /// </summary>
        internal static HashSet<int> hashActorSNOFastMobs = new HashSet<int> {
            5212
         };
        /// <summary>
        /// A list of crappy "summoned mobs" we should always ignore unless they are very close to us, eg "grunts", summoned skeletons etc.
        /// </summary>
        internal static HashSet<int> hashActorSNOShortRangeOnly = new HashSet<int> {
            4084, 4085, 5395, 144315,
         };
        /// <summary>
        /// Dictionary for priorities, like the skeleton summoner cos it keeps bringing new stuff
        /// </summary>
        internal static Dictionary<int, int> dictActorSNOPriority = new Dictionary<int, int> {
            // Wood wraiths all this line (495 & 496 & 6572 & 139454 & 139456 & 170324 & 170325)
            {495, 901}, {496, 901}, {6572, 901}, {139454, 901}, {139456, 901}, {170324, 901}, {170325, 901},
            //intell -- added 4099 (act 2 fallen shaman)
            // Fallen Shaman prophets goblin Summoners (365 & 4100)
            {365, 1901}, {4099, 1901}, {4100, 1901},
            // The annoying grunts summoned by the above
            {4084, -401},
            // Wretched mothers that summon zombies in act 1 (6639)
            {6639, 951},
            // Fallen lunatic (4095)
            {4095, 2999},
            // Pestilence hands (4738)
            {4738, 1901},
            // Belial Minions
            {104014, 1500},
            // Act 1 doors that skeletons get stuck behind
            {454, 1500},
            // Cydaea boss (95250)
            {95250, 1501},
            //intell
            //Cydaea Spiderlings (137139)
            {137139, -301},
            // GoatMutantshaman Elite (4304)
            {4304, 999},
            // GoatMutantshaman (4300)
            {4300, 901},
            // Succubus (5508)
            {5508, 801},
            // skeleton summoners (5387, 5388, 5389)
            {5387, 951}, {5388, 951}, {5389, 951},
            // Weak skeletons summoned by the above
            {5395, -401},
            // Wasp/Bees - Act 2 annoying flyers (5212)
            {5212, 751},
            // Dark summoner - summons the helion dogs (6035)
            {6035, 501},
            // Dark berserkers - has the huge damaging slow hit (6052)
            {6052, 501},
            // The giant undead fat grotesques that explode in act 1 (3847)
            {3847, 401},
            // Hive pods that summon flyers in act 1 (4152, 4153, 4154)
            {4152, 901}, {4153, 901}, {4154, 901},
            // Totems in act 1 that summon the ranged goatmen (166452)
            {166452, 901},
            // Totems in act 1 dungeons that summon skeletons (176907)
            {176907, 901},
            // Act 2 Sand Monster + Zultun Kulle (kill sand monster first)
            {226849, 20000}, {80509, 1100},
            // Maghda and her minions
            {6031, 801}, {178512, 901},
            // Uber Bosses - Skeleton King {255929}, Maghda {256189} & Pets {219702} which must be killed first
            {255929, 2999}, {219702, 1999}, {256189, 999},
            // Uber Bosses - Zoltun Kulle {256508} & Siegebreaker {256187}
            // Siegebreaker removed so the focus remains on Zoltun Kulle until he is dead
            {256508, 2999},
            //{256508, 2999}, {256187, 1899},
            // Uber Bosses - Ghom {256709} & Rakanot {256711}
            {256709, 2999}, {256711, 1899},
         };
        /// <summary>
        /// A list of all known SNO's of treasure goblins/bandits etc.
        /// </summary>
        internal static HashSet<int> hashActorSNOGoblins = new HashSet<int> {
            5984, 5985, 5987, 5988
         };
        // A list of ranged mobs that should be attacked even if they are outside of the routines current kill radius
        //365, 4100 = fallen; 4300, 4304 = goat shaman; 4738 = pestilence; 4299 = goat ranged; 62736, 130794 = demon flyer; 5508 = succubus
        // 210120 = corrupt growths, act 4
        /// <summary>
        /// Contains ActorSNO of ranged units that should be attacked even if outside of kill radius
        /// </summary>
        internal static HashSet<int> hashActorSNORanged = new HashSet<int> {
            365, 4100, 4304, 4300, 4738, 4299, 62736, 130794, 5508, 210120, 5388, 4286
         };
        // A list of bosses in the game, just to make CERTAIN they are treated as elites
        /// <summary>
        /// Contains ActorSNO of known Bosses
        /// </summary>
        internal static HashSet<int> hashBossSNO = new HashSet<int>
        {
            // Siegebreaker (96192), Azmodan (89690), Cydea (95250), Heart-thing (193077), 
            96192,                   89690,           95250,         193077, 
            //Kulle (80509), Small Belial (220160), Big Belial (3349), Diablo 1 (114917), terror Diablo (133562)
            80509,           220160,                3349,              114917,            133562, 
            
            255929, 256711, 256508, 256187, 256189, 256709,
            // Another Cydaea
            137139,
            // Diablo shadow clones (needs all of them, there is a male & female version of each class!)
            144001, 144003, 143996, 143994, 
            // Jondar, Chancellor, Queen Araneae (act 1 dungeons), Skeleton King, Butcher
            86624, 156353, 51341, 5350, 3526,
            // Corrupt Growths, Act 4, Istaku
            210120, 215103,
            // Rakanoth
            4630, 
         };
        // IGNORE LIST / BLACKLIST - for units / monsters / npcs
        // Special blacklist for things like ravens, templar/scoundrel/enchantress in town, witch-doctor summons, tornado-animations etc. etc. that should never be attacked
        // Note: This is a MONSTER blacklist - so only stuff that needs to be ignored by the combat-engine. An "object" blacklist is further down!
        /// <summary>
        /// Contains ActorSNO of Units that should be blacklisted
        /// </summary>
        internal static HashSet<int> hashActorSNOIgnoreBlacklist = new HashSet<int>
        {
            5840, 111456, 5013, 5014, 205756, 205746, 4182, 4183, 4644, 4062, 4538, 52693, 162575, 2928, 51291, 51292,
            96132, 90958, 90959, 80980, 51292, 51291, 2928, 3546, 129345, 81857, 138428, 81857, 60583, 170038, 174854, 190390,
            194263, 174900, 87189, 90072, 107031, 106584, 186130, 187265,
            108012, 103279, 74004, 84531, 84538,  190492, 209133, 6318, 107705, 105681, 89934,
            89933, 182276, 117574, 182271, 182283, 182278, 128895, 81980, 82111, 81226, 81227, 107067, 106749,
            107107, 107112, 106731, 107752, 107829, 90321, 107828, 121327, 185391, 249320, 81232, 81231, 81239, 81515, 210433, 195414,
            80758, 80757, 80745, 81229, 81230, 82109, 83024, 83025, 82972, 83959, 249190, 251396, 138472, 118260, 200226, 192654, 245828,
            215103, 132951, 217508, 199998, 199997, 114527, 245910, 169123, 123885, 169890, 168878, 169891, 169077, 169904, 169907,
            169906, 169908, 169905, 169909, 179780, 179778, 179772, 179779, 179776, 122305, 110959, 103235, 103215, 105763, 103217, 51353, 80140,
            4176, 178664, 173827, 133741, 159144, 181748, 159098, 206569, 200706, 5895, 5896, 5897, 5899, 4686, 87037, 85843, 103919, 249338,
            251416, 249192, 80812, 4798, 183892,196899, 196900, 196903, 223333, 220636, 218951, 245838,
            //blackhawk
            3384,
            //bone pile
            218951,245838,
            // rrrix act 1
            108882, 245919,
            // rrrix act 2
            213907, 92519, 61544, 105681, 113983, 114527, 114642, 139933, 144405, 156890, 164057, 164195, 180254, 180802, 180809, 181173, 181174, 181177, 181181,
            181182, 181185, 181290, 181292, 181306, 181309, 181313, 181326, 181563, 181857, 181858, 187265, 191433, 191462, 191641, 192880, 192881, 196413, 196435,
            197280, 199191, 199264, 199274, 199597, 199664, 200979, 200982, 201236, 201580, 201581, 201583, 204183, 205746, 205756, 210087, 213907, 218228, 219223,
            220114, 3011, 3205, 3539, 3582, 3584, 3595, 3600, 4580, 52693, 5466, 55005, 5509, 62522, 
            205756, 5509, 200371, 167185,
            // rrrix act 3
            60108,
            // uber fire chains in Realm of Turmoil and Iron Gate in Realm of Chaos
            263014, 
         };
        // Three special lists used purely for checking for the existance of a player's summoned mystic ally, gargantuan, or zombie dog
        internal static HashSet<int> hashMysticAlly = new HashSet<int> { 169123, 123885, 169890, 168878, 169891, 169077, 169904, 169907, 169906, 169908, 169905, 169909 };
        internal static HashSet<int> hashGargantuan = new HashSet<int> { 179780, 179778, 179772, 179779, 179776, 122305 };
        internal static HashSet<int> hashZombie = new HashSet<int> { 110959, 103235, 103215, 105763, 103217, 51353 };
        internal static HashSet<int> hashDHPets = new HashSet<int> { 178664, 173827, 133741, 159144, 181748, 159098 };

        /// <summary>
        /// World-object dictionaries eg large object lists, ignore lists etc.
        /// A list of SNO's to *FORCE* to type: Item. (BE CAREFUL WITH THIS!).
        /// 166943 = infernal key
        /// </summary>
        internal static HashSet<int> hashForceSNOToItemList = new HashSet<int> {
            166943,
         };
        // Interactable whitelist - things that need interacting with like special wheels, levers - they will be blacklisted for 30 seconds after one-use
        internal static HashSet<int> hashSNOInteractWhitelist = new HashSet<int> {
            56686, 211999, 52685, 54882,  180575,
         };
        /// <summary>
        /// NOTE: you don't NEED interactable SNO's listed here. But if they are listed here, *THIS* is the range at which your character will try to walk to within the object
        /// BEFORE trying to actually "click it". Certain objects need you to get very close, so it's worth having them listed with low interact ranges
        /// </summary>
        internal static Dictionary<int, int> dictInteractableRange = new Dictionary<int, int> {
            {56686, 4}, {52685, 4}, {54882, 40}, {3349, 25}, {225270, 35}, {180575, 10}
         };
        /// <summary>
        /// Navigation obstacles for standard navigation down dungeons etc. to help DB movement
        /// MAKE SURE you add the *SAME* SNO to the "size" dictionary below, and include a reasonable size (keep it smaller rather than larger) for the SNO.
        /// </summary>
        internal static HashSet<int> hashSNONavigationObstacles = new HashSet<int> {
            174900, 191459, 104632, 196211, 200872, 
        };
        /// <summary>
        /// Size of the navigation obstacles above (actual SNO list must be matching the above list!)
        /// </summary>
        internal static Dictionary<int, int> dictSNONavigationSize = new Dictionary<int, int> {
            {174900, 10}, {191459, 13}, {104632, 20}, {196211, 25}, {200872, 25},
         };
        /// <summary>
        /// Destructible things that are very large and need breaking at a bigger distance - eg logstacks, large crates, carts, etc.
        /// </summary>
        internal static Dictionary<int, int> dictSNOExtendedDestructRange = new Dictionary<int, int> {
            {2972, 10}, {80357, 16}, {116508, 10}, {113932, 8}, {197514, 18}, {108587, 8}, {108618, 8}, {108612, 8}, {116409, 18}, {121586, 22},
            {195101, 10}, {195108, 25}, {170657, 8}, {181228, 10}, {211959, 25}, {210418, 25}, {174496, 8}, {193963, 10}, {159066, 12}, {160570, 12},
            {55325, 14}, {5718, 14}, {5909, 10}, {5792, 8}, {108194, 8}, {129031, 25}, {192867, 8}, {155255, 8}
         };
        /// <summary>
        /// Destructible things that need targeting by a location instead of an ACDGUID (stuff you can't "click on" to destroy in-game)
        /// </summary>
        internal static HashSet<int> hashDestructableLocationTarget = new HashSet<int> {
            170657, 116409, 121586,
         };
        /// <summary>
        /// Resplendent chest SNO list
        /// </summary>
        internal static HashSet<int> hashSNOContainerResplendant = new HashSet<int> {
            62873, 95011, 81424, 108230, 111808, 111809, 211861, 62866,
            // Magi
			112182,
         };
        /// <summary>
        /// Objects that should never be ignored due to no Line of Sight (LoS) or ZDiff
        /// </summary>
        internal static HashSet<int> LineOfSightWhitelist = new HashSet<int>
        {
            116807, // Butcher Health Well
            180575, // Diablo arena Health Well
            129031, // A3 Skycrown Catapults
            //Small Belial (220160), Big Belial (3349),
            220160,                3349,             

        };
        /// <summary>
        /// Chests/average-level containers that deserve a bit of extra radius (ie - they are more worthwhile to loot than "random" misc/junk containers)
        /// </summary>
        internal static HashSet<int> hashSNOContainerWhitelist = new HashSet<int> {
            62859, 62865, 62872, 78790, 79016, 94708, 96522, 130170, 108122, 111870, 111947, 213447, 213446, 51300, 179865, 109264, 212491, 210422, 211861,
            // Magi
			196945, 70534,
         };
        // IGNORE LIST / BLACKLIST - for world objects
        // World objects that should always be ignored - eg certain destructables, certain containers, etc. - anything handled as a "world object" rather than a monster
        /// <summary>
        /// Contains ActorSNO's of world objects that should be blacklisted
        /// </summary>
        internal static HashSet<int> hashSNOIgnoreBlacklist = new HashSet<int> {
            163449, 78030, 2909, 58283, 58309, 58321, 87809, 90150, 91600, 97023, 97350, 97381, 72689, 121327, 54515, 3340, 122076, 123640,
            60665, 60844, 78554, 86400, 86428, 81699, 86266, 86400, 110769, 192466, 211456, 6190, 80002, 104596, 58836, 104827, 74909, 6155, 6156, 6158, 6159, 75132,
            181504, 91688, 3016, 3007, 3011, 3014, 130858, 131573, 214396, 182730, 226087, 141639, 206569, 15119, 54413, 54926, 2979, 5776, 3949,
            108490, 52833, 200371, 153752, 2972, 
            //a3dun_crater_st_Demo_ChainPylon_Fire_Azmodan, a3dun_crater_st_Demon_ChainPylon_Fire_MistressOfPain
            198977, 201680,
            //trOut_Leor_painting
            217285,
            // uber fire chains in Realm of Turmoil  
            263014,
      };

        /// <summary>
        /// Timers for abilities and selection of best ability etc.
        /// </summary>
        private static Dictionary<SNOPower, int> dictAbilityRepeatDefaults = new Dictionary<SNOPower, int>
            {
                {SNOPower.DrinkHealthPotion, 30000},
                {SNOPower.Weapon_Melee_Instant, 5},
                {SNOPower.Weapon_Ranged_Instant, 5},
                {SNOPower.Barbarian_Bash, 5},
                {SNOPower.Barbarian_Cleave, 5},
                {SNOPower.Barbarian_Frenzy, 5},
                {SNOPower.Barbarian_HammerOfTheAncients, 150},
                {SNOPower.Barbarian_Rend, 2650},
                {SNOPower.Barbarian_SeismicSlam, 200},
                {SNOPower.Barbarian_Whirlwind, 5},
                {SNOPower.Barbarian_GroundStomp, 12200},
                {SNOPower.Barbarian_Leap, 10200},
                {SNOPower.Barbarian_Sprint, 2900},
                {SNOPower.Barbarian_IgnorePain, 30200},
                {SNOPower.Barbarian_AncientSpear, 300},
                // Has a rune that resets cooldown from 10 seconds to 0 on crit
                {SNOPower.Barbarian_Revenge, 600},
                {SNOPower.Barbarian_FuriousCharge, 500},
                // Need to be able to check skill-rune for the dynamic cooldown - set to 10 always except for the skill rune :(
                {SNOPower.Barbarian_Overpower, 200},
                {SNOPower.Barbarian_WeaponThrow, 5},
                {SNOPower.Barbarian_ThreateningShout, 10200},
                {SNOPower.Barbarian_BattleRage, 118000},
                {SNOPower.Barbarian_WarCry, 20500},
                {SNOPower.Barbarian_Earthquake, 120500},
                // Need to be able to check skill-run for dynamic cooldown, and passive for extra cooldown
                {SNOPower.Barbarian_CallOfTheAncients, 120500},
                // Need to be able to check passive for cooldown
                {SNOPower.Barbarian_WrathOfTheBerserker, 120500},
                // Need to be able to check passive for cooldown
                // Monk skills
                {SNOPower.Monk_FistsofThunder, 5},
                {SNOPower.Monk_DeadlyReach, 5},
                {SNOPower.Monk_CripplingWave, 5},
                {SNOPower.Monk_WayOfTheHundredFists, 5},
                {SNOPower.Monk_LashingTailKick, 250},
                {SNOPower.Monk_TempestRush, 50},
                {SNOPower.Monk_WaveOfLight, 250},
                {SNOPower.Monk_BlindingFlash, 15200},
                {SNOPower.Monk_BreathOfHeaven, 15200},
                {SNOPower.Monk_Serenity, 20200},
                {SNOPower.Monk_InnerSanctuary, 20200},
                {SNOPower.Monk_DashingStrike, 1000},
                {SNOPower.Monk_ExplodingPalm, 5000},
                {SNOPower.Monk_SweepingWind, 5700},
                {SNOPower.Monk_CycloneStrike, 10000},
                {SNOPower.Monk_SevenSidedStrike, 30200},
                {SNOPower.Monk_MysticAlly, 30000},
                {SNOPower.Monk_MantraOfEvasion, 3300},
                {SNOPower.Monk_MantraOfRetribution, 3300},
                {SNOPower.Monk_MantraOfHealing, 3300},
                {SNOPower.Monk_MantraOfConviction, 3300},
                // Wizard skills
                {SNOPower.Wizard_MagicMissile, 5},
                {SNOPower.Wizard_ShockPulse, 5},
                {SNOPower.Wizard_SpectralBlade, 5},
                {SNOPower.Wizard_Electrocute, 5},
                {SNOPower.Wizard_RayOfFrost, 5},
                {SNOPower.Wizard_ArcaneOrb, 500},
                {SNOPower.Wizard_ArcaneTorrent, 5},
                {SNOPower.Wizard_Disintegrate, 5},
                {SNOPower.Wizard_FrostNova, 9000},
                {SNOPower.Wizard_DiamondSkin, 15000},
                {SNOPower.Wizard_SlowTime, 16000},
                // Is actually 20 seconds, with a rune that changes to 16 seconds
                {SNOPower.Wizard_Teleport, 16000},
                // Need to be able to check rune that let's us spam this 3 times in a row then cooldown
                {SNOPower.Wizard_WaveOfForce, 12000},
                // normally 15/16 seconds, but a certain rune can allow 12 seconds :(
                {SNOPower.Wizard_EnergyTwister, 5},
                {SNOPower.Wizard_Hydra, 12000},
                {SNOPower.Wizard_Meteor, 1000},
                {SNOPower.Wizard_Blizzard, 6000},
                // Effect lasts for 6 seconds, actual cooldown is 0...
                {SNOPower.Wizard_IceArmor, 60000},
                {SNOPower.Wizard_StormArmor, 60000},
                {SNOPower.Wizard_EnergyArmor, 60000},
                {SNOPower.Wizard_MagicWeapon, 60000},
                {SNOPower.Wizard_Familiar, 60000},
                {SNOPower.Wizard_ExplosiveBlast, 6000},
                {SNOPower.Wizard_MirrorImage, 5000},
                {SNOPower.Wizard_Archon, 100000},
                // Actually 120 seconds, but 100 seconds with a rune
                {SNOPower.Wizard_Archon_ArcaneBlast, 5000},
                {SNOPower.Wizard_Archon_ArcaneStrike, 200},
                {SNOPower.Wizard_Archon_DisintegrationWave, 5},
                {SNOPower.Wizard_Archon_SlowTime, 16000},
                {SNOPower.Wizard_Archon_Teleport, 10000},
                // Witch Doctor skills
                {SNOPower.Witchdoctor_PoisonDart, 5},
                {SNOPower.Witchdoctor_CorpseSpider, 5},
                {SNOPower.Witchdoctor_PlagueOfToads, 5},
                {SNOPower.Witchdoctor_Firebomb, 5},
                {SNOPower.Witchdoctor_GraspOfTheDead, 6000},
                {SNOPower.Witchdoctor_Firebats, 5},
                {SNOPower.Witchdoctor_Haunt, 12000},
                {SNOPower.Witchdoctor_Locust_Swarm, 8000},
                {SNOPower.Witchdoctor_SummonZombieDog, 25000},
                {SNOPower.Witchdoctor_Horrify, 16200},
                {SNOPower.Witchdoctor_SpiritWalk, 15200},
                {SNOPower.Witchdoctor_Hex, 15200},
                {SNOPower.Witchdoctor_SoulHarvest, 15000},
                {SNOPower.Witchdoctor_Sacrifice, 1000},
                {SNOPower.Witchdoctor_MassConfusion, 45200},
                {SNOPower.Witchdoctor_ZombieCharger, 5},
                {SNOPower.Witchdoctor_SpiritBarrage, 15000},
                {SNOPower.Witchdoctor_AcidCloud, 5},
                {SNOPower.Witchdoctor_WallOfZombies, 25200},
                {SNOPower.Witchdoctor_Gargantuan, 25000},
                {SNOPower.Witchdoctor_BigBadVoodoo, 120000},
                {SNOPower.Witchdoctor_FetishArmy, 90000},
                // Demon Hunter skills
                {SNOPower.DemonHunter_HungeringArrow, 5},
                {SNOPower.DemonHunter_EntanglingShot, 5},
                {SNOPower.DemonHunter_BolaShot, 5},
                {SNOPower.DemonHunter_Grenades, 5},
                {SNOPower.DemonHunter_Impale, 5},
                {SNOPower.DemonHunter_RapidFire, 5},
                {SNOPower.DemonHunter_Chakram, 5},
                {SNOPower.DemonHunter_ElementalArrow, 5},
                {SNOPower.DemonHunter_Caltrops, 6000},
                {SNOPower.DemonHunter_SmokeScreen, 3000},
                {SNOPower.DemonHunter_ShadowPower, 5000},
                {SNOPower.DemonHunter_Vault, 400},
                {SNOPower.DemonHunter_Preparation, 5000},
                {SNOPower.DemonHunter_Companion, 30000},
                {SNOPower.DemonHunter_MarkedForDeath, 3000},
                {SNOPower.DemonHunter_EvasiveFire, 300},
                {SNOPower.DemonHunter_FanOfKnives, 10000},
                {SNOPower.DemonHunter_SpikeTrap, 1000},
                {SNOPower.DemonHunter_Sentry, 8000},
                {SNOPower.DemonHunter_Strafe, 5},
                {SNOPower.DemonHunter_Multishot, 5},
                {SNOPower.DemonHunter_ClusterArrow, 150},
                {SNOPower.DemonHunter_RainOfVengeance, 10000},
            };
        // Actual delays copy the defaults
        public static Dictionary<SNOPower, int> dictAbilityRepeatDelay = new Dictionary<SNOPower, int>(dictAbilityRepeatDefaults);

        /// <summary>
        /// Last used-timers for all abilities to prevent spamming D3 memory for cancast checks too often
        /// These should NEVER need manually editing
        /// But you do need to make sure every skill used by Trinity is listed in here once!
        /// </summary>
        private static Dictionary<SNOPower, DateTime> dictAbilityLastUseDefaults = new Dictionary<SNOPower, DateTime>
            {
                {SNOPower.DrinkHealthPotion, DateTime.Today},{SNOPower.Weapon_Melee_Instant, DateTime.Today},{SNOPower.Weapon_Ranged_Instant, DateTime.Today},
                // Barbarian last-used timers
                {SNOPower.Barbarian_Bash, DateTime.Today},{SNOPower.Barbarian_Cleave, DateTime.Today},{SNOPower.Barbarian_Frenzy, DateTime.Today},
                {SNOPower.Barbarian_HammerOfTheAncients, DateTime.Today},{SNOPower.Barbarian_Rend, DateTime.Today},{SNOPower.Barbarian_SeismicSlam, DateTime.Today},
                {SNOPower.Barbarian_Whirlwind, DateTime.Today},{SNOPower.Barbarian_GroundStomp, DateTime.Today},{SNOPower.Barbarian_Leap, DateTime.Today},
                {SNOPower.Barbarian_Sprint, DateTime.Today},{SNOPower.Barbarian_IgnorePain, DateTime.Today},{SNOPower.Barbarian_AncientSpear, DateTime.Today},
                {SNOPower.Barbarian_Revenge, DateTime.Today},{SNOPower.Barbarian_FuriousCharge, DateTime.Today},{SNOPower.Barbarian_Overpower, DateTime.Today},
                {SNOPower.Barbarian_WeaponThrow, DateTime.Today},{SNOPower.Barbarian_ThreateningShout, DateTime.Today},{SNOPower.Barbarian_BattleRage, DateTime.Today},
                {SNOPower.Barbarian_WarCry, DateTime.Today},{SNOPower.Barbarian_Earthquake, DateTime.Today},{SNOPower.Barbarian_CallOfTheAncients, DateTime.Today},
                {SNOPower.Barbarian_WrathOfTheBerserker, DateTime.Today },
                // Monk last-used timers
                {SNOPower.Monk_FistsofThunder, DateTime.Today},{SNOPower.Monk_DeadlyReach, DateTime.Today},{SNOPower.Monk_CripplingWave, DateTime.Today},
                {SNOPower.Monk_WayOfTheHundredFists, DateTime.Today},{SNOPower.Monk_LashingTailKick, DateTime.Today},{SNOPower.Monk_TempestRush, DateTime.Today},
                {SNOPower.Monk_WaveOfLight, DateTime.Today},{SNOPower.Monk_BlindingFlash, DateTime.Today},{SNOPower.Monk_BreathOfHeaven, DateTime.Today},
                {SNOPower.Monk_Serenity, DateTime.Today}, {SNOPower.Monk_InnerSanctuary, DateTime.Today},{SNOPower.Monk_DashingStrike, DateTime.Today},
                {SNOPower.Monk_ExplodingPalm, DateTime.Today},{SNOPower.Monk_SweepingWind, DateTime.Today},{SNOPower.Monk_CycloneStrike, DateTime.Today},
                {SNOPower.Monk_SevenSidedStrike, DateTime.Today},{SNOPower.Monk_MysticAlly, DateTime.Today},{SNOPower.Monk_MantraOfEvasion, DateTime.Today},
                {SNOPower.Monk_MantraOfRetribution, DateTime.Today},{SNOPower.Monk_MantraOfHealing, DateTime.Today}, {SNOPower.Monk_MantraOfConviction, DateTime.Today},
                // Wizard last-used timers
                {SNOPower.Wizard_MagicMissile, DateTime.Today},{SNOPower.Wizard_ShockPulse, DateTime.Today},{SNOPower.Wizard_SpectralBlade, DateTime.Today},
                {SNOPower.Wizard_Electrocute, DateTime.Today},{SNOPower.Wizard_RayOfFrost, DateTime.Today},{SNOPower.Wizard_ArcaneOrb, DateTime.Today},
                {SNOPower.Wizard_ArcaneTorrent, DateTime.Today},{SNOPower.Wizard_Disintegrate, DateTime.Today},{SNOPower.Wizard_FrostNova, DateTime.Today},
                {SNOPower.Wizard_DiamondSkin, DateTime.Today},{SNOPower.Wizard_SlowTime, DateTime.Today},{SNOPower.Wizard_Teleport, DateTime.Today},
                {SNOPower.Wizard_WaveOfForce, DateTime.Today},{SNOPower.Wizard_EnergyTwister, DateTime.Today},{SNOPower.Wizard_Hydra, DateTime.Today},
                {SNOPower.Wizard_Meteor, DateTime.Today},{SNOPower.Wizard_Blizzard, DateTime.Today},{SNOPower.Wizard_IceArmor, DateTime.Today},
                {SNOPower.Wizard_StormArmor, DateTime.Today},{SNOPower.Wizard_MagicWeapon, DateTime.Today},{SNOPower.Wizard_Familiar, DateTime.Today},
                {SNOPower.Wizard_EnergyArmor, DateTime.Today},{SNOPower.Wizard_ExplosiveBlast, DateTime.Today},{SNOPower.Wizard_MirrorImage, DateTime.Today},
                {SNOPower.Wizard_Archon, DateTime.Today},{SNOPower.Wizard_Archon_ArcaneBlast, DateTime.Today},{SNOPower.Wizard_Archon_ArcaneStrike, DateTime.Today},
                {SNOPower.Wizard_Archon_DisintegrationWave, DateTime.Today},{SNOPower.Wizard_Archon_SlowTime, DateTime.Today},{SNOPower.Wizard_Archon_Teleport, DateTime.Today},
                // Witch Doctor last-used timers
                {SNOPower.Witchdoctor_PoisonDart, DateTime.Today},{SNOPower.Witchdoctor_CorpseSpider, DateTime.Today},{SNOPower.Witchdoctor_PlagueOfToads, DateTime.Today},
                {SNOPower.Witchdoctor_Firebomb, DateTime.Today},{SNOPower.Witchdoctor_GraspOfTheDead, DateTime.Today},{SNOPower.Witchdoctor_Firebats, DateTime.Today},
                {SNOPower.Witchdoctor_Haunt, DateTime.Today},{SNOPower.Witchdoctor_Locust_Swarm, DateTime.Today},{SNOPower.Witchdoctor_SummonZombieDog, DateTime.Today},
                {SNOPower.Witchdoctor_Horrify, DateTime.Today},{SNOPower.Witchdoctor_SpiritWalk, DateTime.Today},{SNOPower.Witchdoctor_Hex, DateTime.Today},
                {SNOPower.Witchdoctor_SoulHarvest, DateTime.Today},{SNOPower.Witchdoctor_Sacrifice, DateTime.Today},{SNOPower.Witchdoctor_MassConfusion, DateTime.Today},
                {SNOPower.Witchdoctor_ZombieCharger, DateTime.Today},{SNOPower.Witchdoctor_SpiritBarrage, DateTime.Today},{SNOPower.Witchdoctor_AcidCloud, DateTime.Today},
                {SNOPower.Witchdoctor_WallOfZombies, DateTime.Today},{SNOPower.Witchdoctor_Gargantuan, DateTime.Today},{SNOPower.Witchdoctor_BigBadVoodoo, DateTime.Today},
                {SNOPower.Witchdoctor_FetishArmy, DateTime.Today},
                // Demon Hunter last-used timers
                {SNOPower.DemonHunter_HungeringArrow, DateTime.Today},{SNOPower.DemonHunter_EntanglingShot, DateTime.Today},{SNOPower.DemonHunter_BolaShot, DateTime.Today},
                {SNOPower.DemonHunter_Grenades, DateTime.Today},{SNOPower.DemonHunter_Impale, DateTime.Today},{SNOPower.DemonHunter_RapidFire, DateTime.Today},
                {SNOPower.DemonHunter_Chakram, DateTime.Today},{SNOPower.DemonHunter_ElementalArrow, DateTime.Today},{SNOPower.DemonHunter_Caltrops, DateTime.Today},
                {SNOPower.DemonHunter_SmokeScreen, DateTime.Today},{SNOPower.DemonHunter_ShadowPower, DateTime.Today},{SNOPower.DemonHunter_Vault, DateTime.Today},
                {SNOPower.DemonHunter_Preparation, DateTime.Today},{SNOPower.DemonHunter_Companion, DateTime.Today},{SNOPower.DemonHunter_MarkedForDeath, DateTime.Today},
                {SNOPower.DemonHunter_EvasiveFire, DateTime.Today},{SNOPower.DemonHunter_FanOfKnives, DateTime.Today},{SNOPower.DemonHunter_SpikeTrap, DateTime.Today},
                {SNOPower.DemonHunter_Sentry, DateTime.Today},{SNOPower.DemonHunter_Strafe, DateTime.Today},{SNOPower.DemonHunter_Multishot, DateTime.Today},
                {SNOPower.DemonHunter_ClusterArrow, DateTime.Today},{SNOPower.DemonHunter_RainOfVengeance, DateTime.Today},
            };
        /// <summary>
        /// This is the ACTUAL dictionary used now (the above are used to quickly reset all timers back to defaults on death etc.)
        /// </summary>
        public static Dictionary<SNOPower, DateTime> dictAbilityLastUse = new Dictionary<SNOPower, DateTime>(dictAbilityLastUseDefaults);
        /// <summary>
        /// And this is to avoid using certain long-cooldown skills immediately after a fail
        /// </summary>
        public static Dictionary<SNOPower, DateTime> dictAbilityLastFailed = new Dictionary<SNOPower, DateTime>(dictAbilityLastUseDefaults);
        /// <summary>
        /// And a "global cooldown" to prevent non-signature-spells being used too fast
        /// </summary>
        public static DateTime lastGlobalCooldownUse = DateTime.Today;

        /* 
         * This set of dictionaries are used for performance increases throughout, and a minimization of DB mis-read/null exception errors
         * Uses a little more ram - but for a massive CPU gain. And ram is cheap, CPU is not!
         */


        /// <summary>
        /// Used only for certain skills that spam the powermanager regularly, to limit their CPU hits
        /// </summary>
        private static Dictionary<SNOPower, DateTime> dictAbilityLastPowerChecked = new Dictionary<SNOPower, DateTime>
            {
                {SNOPower.Barbarian_Revenge, DateTime.Today},
                {SNOPower.Barbarian_FuriousCharge, DateTime.Today},
                {SNOPower.Wizard_DiamondSkin, DateTime.Today},
                {SNOPower.Wizard_FrostNova, DateTime.Today},
                {SNOPower.Wizard_ExplosiveBlast, DateTime.Today},
                {SNOPower.Witchdoctor_Hex, DateTime.Today},
                {SNOPower.Witchdoctor_SoulHarvest, DateTime.Today},
            };
        /// <summary>
        /// Caches the GilesObjectType of each object as we find it (RactorGUID based)
        /// </summary>
        private static Dictionary<int, GObjectType> dictGilesObjectTypeCache = new Dictionary<int, GObjectType>();
        /// <summary>
        /// Caches monster affixes for the monster ID, as this value can be a pain to get and slow (RactorGUID based)
        /// </summary>
        private static Dictionary<int, MonsterAffixes> dictGilesMonsterAffixCache = new Dictionary<int, MonsterAffixes>();
        /// <summary>
        /// Caches each monster's max-health, since this never changes (RactorGUID based)
        /// </summary>
        private static Dictionary<int, double> dictGilesMaxHealthCache = new Dictionary<int, double>();
        /// <summary>
        /// Caches each monster's current health for brief periods  (RactorGUID based)
        /// </summary>
        private static Dictionary<int, double> dictGilesLastHealthCache = new Dictionary<int, double>();
        private static Dictionary<int, int> dictGilesLastHealthChecked = new Dictionary<int, int>();
        /// <summary>
        /// Store a "not-burrowed" value for monsters that we have already checked a burrowed-status of and found false (RactorGUID based)
        /// </summary>
        private static Dictionary<int, bool> dictGilesBurrowedCache = new Dictionary<int, bool>();
        /// <summary>
        /// Store Actor SNO for each object (RactorGUID based)
        /// </summary>
        private static Dictionary<int, int> dictGilesActorSNOCache = new Dictionary<int, int>();
        /// <summary>
        /// Store ACDGUID for each object (RactorGUID based)
        /// </summary>
        private static Dictionary<int, int> dictGilesACDGUIDCache = new Dictionary<int, int>();
        /// <summary>
        /// Store internal name for each object (RactorGUID based)
        /// </summary>
        private static Dictionary<int, string> dictGilesInternalNameCache = new Dictionary<int, string>();
        /// <summary>
        /// Store Collision-sphere radius for each object (SNO based)
        /// </summary>
        private static Dictionary<int, float> dictGilesCollisionSphereCache = new Dictionary<int, float>();
        /// <summary>
        /// Caches the game balance ID for each object, which can then be used to pull up the appropriate entry from within dictGilesGameBalanceDataCache (RactorGUID based)
        /// </summary>
        private static Dictionary<int, int> dictGilesGameBalanceIDCache = new Dictionary<int, int>();
        /// <summary>
        /// Caches the Dynamic ID for each object (only used for non-units) (RactorGUID based)
        /// </summary>
        private static Dictionary<int, int> dictGilesDynamicIDCache = new Dictionary<int, int>();
        /// <summary>
        /// Caches the position for each object (only used for non-units, as this data is static so can be cached) (RactorGUID based)
        /// </summary>
        private static Dictionary<int, Vector3> dictGilesVectorCache = new Dictionary<int, Vector3>();
        /// <summary>
        /// Same as above but for gold-amount of pile (RactorGUID based)
        /// </summary>
        private static Dictionary<int, int> dictGilesGoldAmountCache = new Dictionary<int, int>();
        /// <summary>
        /// Same as above but for quality of item, we check this twice to make bloody sure we don't miss a legendary from a mis-read though (RactorGUID based)
        /// </summary>
        private static Dictionary<int, ItemQuality> dictGilesQualityCache = new Dictionary<int, ItemQuality>();
        private static Dictionary<int, bool> dictGilesQualityRechecked = new Dictionary<int, bool>();
        /// <summary>
        /// Same as above but for whether we want to pick it up or not (RactorGUID based)
        /// </summary>
        private static Dictionary<int, bool> dictGilesPickupItem = new Dictionary<int, bool>();
        /// <summary>
        /// How many times the player tried to interact with this object in total
        /// </summary>
        private static Dictionary<int, int> dictTotalInteractionAttempts = new Dictionary<int, int>();
        /// <summary>
        /// Physics SNO for certain objects (SNO based)
        /// </summary>
        private static Dictionary<int, int> dictPhysicsSNO = new Dictionary<int, int>();
        /// <summary>
        /// Summoned-by ID for units (RactorGUID based)
        /// </summary>
        private static Dictionary<int, int> dictSummonedByID = new Dictionary<int, int>();

        /// <summary>
        /// Do we actually need this?
        /// </summary>
        private static Dictionary<int, GilesGameBalanceDataCache> dictGilesGameBalanceDataCache = new Dictionary<int, GilesGameBalanceDataCache>();
    }
}
