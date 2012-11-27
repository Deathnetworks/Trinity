using System;
using System.Collections.Generic;
using Zeta.Common;
using Zeta.Common.Plugins;
using Zeta.Internals.Actors;
namespace GilesTrinity
{
    public partial class GilesTrinity : IPlugin
    {
        /// <summary>
        /// This list is used when an actor has an attribute BuffVisualEffect=1, e.g. fire floors in The Butcher arena
        /// </summary>
        private static HashSet<int> hashAvoidanceBuffSNOList = new HashSet<int>
        {
            // Butcher Floor Panels
            201454, 201464, 201426, 201438, 200969, 201423, 201242,
            // A1 Halls of Agony Fire walls
            89578, 89579,

        };

        /// <summary>
        /// A list of all the SNO's to avoid - you could add
        /// </summary>
        private static HashSet<int> hashAvoidanceSNOList = new HashSet<int>
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
        private static HashSet<int> hashAvoidanceSNOProjectiles = new HashSet<int>
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
        private static HashSet<int> hashActorSNOWhirlwindIgnore = new HashSet<int> {
            4304, 5984, 5985, 5987, 5988,
         };

        /// <summary>
        /// Very fast moving mobs (eg wasps), for special skill-selection decisions
        /// </summary>
        private static HashSet<int> hashActorSNOFastMobs = new HashSet<int> {
            5212
         };
        /// <summary>
        /// A list of crappy "summoned mobs" we should always ignore unless they are very close to us, eg "grunts", summoned skeletons etc.
        /// </summary>
        private static HashSet<int> hashActorSNOShortRangeOnly = new HashSet<int> {
            4084, 4085, 5395, 144315,
         };
        /// <summary>
        /// Dictionary for priorities, like the skeleton summoner cos it keeps bringing new stuff
        /// </summary>
        private static Dictionary<int, int> dictActorSNOPriority = new Dictionary<int, int> {
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
        private static HashSet<int> hashActorSNOGoblins = new HashSet<int> {
            5984, 5985, 5987, 5988
         };
        // A list of ranged mobs that should be attacked even if they are outside of the routines current kill radius
        //365, 4100 = fallen; 4300, 4304 = goat shaman; 4738 = pestilence; 4299 = goat ranged; 62736, 130794 = demon flyer; 5508 = succubus
        // 210120 = corrupt growths, act 4
        /// <summary>
        /// Contains ActorSNO of ranged units that should be attacked even if outside of kill radius
        /// </summary>
        private static HashSet<int> hashActorSNORanged = new HashSet<int> {
            365, 4100, 4304, 4300, 4738, 4299, 62736, 130794, 5508, 210120, 5388, 4286
         };
        // A list of bosses in the game, just to make CERTAIN they are treated as elites
        /// <summary>
        /// Contains ActorSNO of known Bosses
        /// </summary>
        private static HashSet<int> hashBossSNO = new HashSet<int>
        {
            // Siegebreaker (96192), Azmodan (89690), Cydea (95250), Heart-thing (193077), Kulle (80509), Small Belial (220160), Big Belial (3349), Diablo 1 (114917), terror Diablo (133562)
            96192, 89690, 95250, 193077, 80509, 220160, 3349, 114917, 133562, 255929, 256711, 256508, 256187, 256189, 256709,
            // Another Cydaea
            137139,
            // Diablo shadow clones (needs all of them, there is a male & female version of each class!)
            144001, 144003, 143996, 143994, 
            // Jondar, Chancellor, Queen Araneae (act 1 dungeons), Skeleton King, Butcher
            86624, 156353, 51341, 5350, 3526,
            // Corrupt Growths, Act 4, Istaku
            210120, 215103,
         };
        // IGNORE LIST / BLACKLIST - for units / monsters / npcs
        // Special blacklist for things like ravens, templar/scoundrel/enchantress in town, witch-doctor summons, tornado-animations etc. etc. that should never be attacked
        // Note: This is a MONSTER blacklist - so only stuff that needs to be ignored by the combat-engine. An "object" blacklist is further down!
        /// <summary>
        /// Contains ActorSNO of Units that should be blacklisted
        /// </summary>
        private static HashSet<int> hashActorSNOIgnoreBlacklist = new HashSet<int>
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
            108882,
            // rrrix act 2
            213907, 92519, 61544, 105681, 113983, 114527, 114642, 139933, 144405, 156890, 164057, 164195, 180254, 180802, 180809, 181173, 181174, 181177, 181181,
            181182, 181185, 181290, 181292, 181306, 181309, 181313, 181326, 181563, 181857, 181858, 187265, 191433, 191462, 191641, 192880, 192881, 196413, 196435,
            197280, 199191, 199264, 199274, 199597, 199664, 200979, 200982, 201236, 201580, 201581, 201583, 204183, 205746, 205756, 210087, 213907, 218228, 219223,
            220114, 3011, 3205, 3539, 3582, 3584, 3595, 3600, 4580, 52693, 5466, 55005, 5509, 62522, 5188,
            205756, 5509, 200371,
            // rrrix act 3
            60108,
         };
        // Three special lists used purely for checking for the existance of a player's summoned mystic ally, gargantuan, or zombie dog
        private static HashSet<int> hashMysticAlly = new HashSet<int> { 169123, 123885, 169890, 168878, 169891, 169077, 169904, 169907, 169906, 169908, 169905, 169909 };
        private static HashSet<int> hashGargantuan = new HashSet<int> { 179780, 179778, 179772, 179779, 179776, 122305 };
        private static HashSet<int> hashZombie = new HashSet<int> { 110959, 103235, 103215, 105763, 103217, 51353 };
        private static HashSet<int> hashDHPets = new HashSet<int> { 178664, 173827, 133741, 159144, 181748, 159098 };

        /// <summary>
        /// World-object dictionaries eg large object lists, ignore lists etc.
        /// A list of SNO's to *FORCE* to type: Item. (BE CAREFUL WITH THIS!).
        /// 166943 = infernal key
        /// </summary>
        private static HashSet<int> hashForceSNOToItemList = new HashSet<int> {
            166943,
         };
        // Interactable whitelist - things that need interacting with like special wheels, levers - they will be blacklisted for 30 seconds after one-use
        private static HashSet<int> hashSNOInteractWhitelist = new HashSet<int> {
            54908, 56686, 54850,
            211999, 52685, 54882,  180575,
         };
        /// <summary>
        /// NOTE: you don't NEED interactable SNO's listed here. But if they are listed here, *THIS* is the range at which your character will try to walk to within the object
        /// BEFORE trying to actually "click it". Certain objects need you to get very close, so it's worth having them listed with low interact ranges
        /// </summary>
        private static Dictionary<int, int> dictInteractableRange = new Dictionary<int, int> {
            {56686, 4}, {52685, 4}, {54850, 14},  {54882, 40}, {54908, 4}, {3349, 25}, {225270, 35}, {180575, 10}
         };
        /// <summary>
        /// Navigation obstacles for standard navigation down dungeons etc. to help DB movement
        /// MAKE SURE you add the *SAME* SNO to the "size" dictionary below, and include a reasonable size (keep it smaller rather than larger) for the SNO.
        /// </summary>
        private static HashSet<int> hashSNONavigationObstacles = new HashSet<int> {
            174900, 191459, 104632
        };
        /// <summary>
        /// Size of the navigation obstacles above (actual SNO list must be matching the above list!)
        /// </summary>
        public static Dictionary<int, int> dictSNONavigationSize = new Dictionary<int, int> {
            {174900, 10}, {191459, 13}, {54908, 10}, {104632, 20},
         };
        /// <summary>
        /// Destructible things that are very large and need breaking at a bigger distance - eg logstacks, large crates, carts, etc.
        /// </summary>
        private static Dictionary<int, int> dictSNOExtendedDestructRange = new Dictionary<int, int> {
            {2972, 10}, {80357, 16}, {116508, 10}, {113932, 8}, {197514, 18}, {108587, 8}, {108618, 8}, {108612, 8}, {116409, 18}, {121586, 18},
            {195101, 10}, {195108, 25}, {170657, 8}, {181228, 10}, {211959, 25}, {210418, 25}, {174496, 8}, {193963, 10}, {159066, 12}, {160570, 12},
            {55325, 14}, {5718, 14}, {5909, 10}, {5792, 8}, {108194, 8}, {129031, 20}, {192867, 8}, {155255, 8}
         };
        /// <summary>
        /// Destructible things that need targeting by a location instead of an ACDGUID (stuff you can't "click on" to destroy in-game)
        /// </summary>
        private static HashSet<int> hashDestructableLocationTarget = new HashSet<int> {
            170657, 116409, 121586,
         };
        /// <summary>
        /// Resplendent chest SNO list
        /// </summary>
        private static HashSet<int> hashSNOContainerResplendant = new HashSet<int> {
            62873, 95011, 81424, 108230, 111808, 111809, 211861,
         };
        /// <summary>
        /// Objects that should never be ignored due to no Line of Sight (LoS)
        /// </summary>
        private static HashSet<int> LineOfSightWhitelist = new HashSet<int>
        {
            116807, // Butcher Health Well
            180575, // Diablo arena Health Well
        };
        /// <summary>
        /// Chests/average-level containers that deserve a bit of extra radius (ie - they are more worthwhile to loot than "random" misc/junk containers)
        /// </summary>
        private static HashSet<int> hashSNOContainerWhitelist = new HashSet<int> {
            62859, 62865, 62872, 78790, 79016, 94708, 96522, 130170, 108122, 111870, 111947, 213447, 213446, 51300, 179865, 109264, 212491, 210422, 211861,
         };
        // IGNORE LIST / BLACKLIST - for world objects
        // World objects that should always be ignored - eg certain destructables, certain containers, etc. - anything handled as a "world object" rather than a monster
        /// <summary>
        /// Contains ActorSNO's of world objects that should be blacklisted
        /// </summary>
        private static HashSet<int> hashSNOIgnoreBlacklist = new HashSet<int> {
            163449, 78030, 2909, 58283, 58299, 58309, 58321, 87809, 88005, 90150, 91600, 97023, 97350, 97381, 72689, 121327, 54952, 54515, 3340, 122076, 123640,
            60665, 60844, 78554, 86400, 86428, 81699, 86266, 86400, 110769, 192466, 211456, 6190, 80002, 104596, 58836, 104827, 74909, 6155, 6156, 6158, 6159, 75132,
            181504, 91688, 3016, 3007, 3011, 3014, 130858, 131573, 214396, 182730, 226087, 141639, 206569, 15119, 54413, 54926, 2979, 56416, 53802, 5776, 3949,
            108490, 52833, 200371,
            //a3dun_crater_st_Demo_ChainPylon_Fire_Azmodan
            198977,
            //a3dun_crater_st_Demon_ChainPylon_Fire_MistressOfPain
            201680,
            //trOut_Leor_painting
            217285,
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
                {SNOPower.Monk_TempestRush, 250},
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
                {SNOPower.Wizard_IceArmor, 115000},
                {SNOPower.Wizard_StormArmor, 115000},
                {SNOPower.Wizard_MagicWeapon, 60000},
                {SNOPower.Wizard_Familiar, 60000},
                {SNOPower.Wizard_EnergyArmor, 115000},
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

        // Giant Super Special Sauce Dictionary
        // Here's the a huuuuuge dictionary I have been building up containing cached data on items to minimize D3 memory reads and help prevent some DB mis-handling of items
        // Note that even if an item is not on the list - it will add it to the cache "temporarily" (lasting for the duration of the bot run) - so if your bot encounters the
        // same kind of item twice in a row - the second time, it will use the cached data - so it will still minimize issues even if I haven't added all items to this list :D
        /// <summary>
        /// Do we actually need this?
        /// </summary>
        private static Dictionary<int, GilesGameBalanceDataCache> dictGilesGameBalanceDataCache = new Dictionary<int, GilesGameBalanceDataCache>();
        //#region GameBalanceIDCache
        //{
        //    {-1106917318, new GilesGameBalanceDataCache(1, ItemType.CraftingPage, false, FollowerType.None)},
        //    //Lore_Book_Flippy-10246    - Could be an error item!?!?
        //    {181033993, new GilesGameBalanceDataCache(62, ItemType.Crossbow, false, FollowerType.None)},
        //    //XBow_norm_base_flippy_07-946
        //    {-970366835, new GilesGameBalanceDataCache(61, ItemType.CraftingPage, false, FollowerType.None)},
        //    //Lore_Book_Flippy-898
        //    {126259833, new GilesGameBalanceDataCache(1, ItemType.Unknown, false, FollowerType.None)},
        //    //GoldSmall-914
        //    {126259831, new GilesGameBalanceDataCache(1, ItemType.Unknown, false, FollowerType.None)},
        //    //GoldCoin-1881
        //    {126259832, new GilesGameBalanceDataCache(1, ItemType.Unknown, false, FollowerType.None)},
        //    //GoldCoins-1007
        //    {126259834, new GilesGameBalanceDataCache(1, ItemType.Unknown, false, FollowerType.None)},
        //    //GoldMedium-657
        //    {126259835, new GilesGameBalanceDataCache(1, ItemType.Unknown, false, FollowerType.None)},
        //    //GoldLarge-7981
        //    {-1483610851, new GilesGameBalanceDataCache(60, ItemType.Potion, false, FollowerType.None)},
        //    //healthPotion_Mythic-776
        //    {-1962741247, new GilesGameBalanceDataCache(1, ItemType.Unknown, false, FollowerType.None)},
        //    //HealthGlobe-580
        //    {1661414572, new GilesGameBalanceDataCache(61, ItemType.Axe, true, FollowerType.None)},
        //    //Axe_norm_base_flippy_05-495
        //    {-1533912123, new GilesGameBalanceDataCache(61, ItemType.Gloves, false, FollowerType.None)},
        //    //Gloves_norm_base_flippy-1995
        //    {-330720411, new GilesGameBalanceDataCache(63, ItemType.Shield, false, FollowerType.None)},
        //    //Shield_norm_base_flippy_07-1580
        //    {-733829188, new GilesGameBalanceDataCache(61, ItemType.Belt, false, FollowerType.None)},
        //    //Belt_norm_base_flippy-1696
        //    {2140882331, new GilesGameBalanceDataCache(60, ItemType.Boots, false, FollowerType.None)},
        //    //Boots_norm_base_flippy-1701
        //    {-1616888606, new GilesGameBalanceDataCache(55, ItemType.Mace, false, FollowerType.None)},
        //    //twoHandedMace_norm_base_flippy_02-1697
        //    {1565456762, new GilesGameBalanceDataCache(60, ItemType.Helm, false, FollowerType.None)},
        //    //Helm_norm_base_flippy-3182
        //    {2140882332, new GilesGameBalanceDataCache(61, ItemType.Boots, false, FollowerType.None)},
        //    //Boots_norm_base_flippy-3189
        //    {-2115689173, new GilesGameBalanceDataCache(62, ItemType.Staff, false, FollowerType.None)},
        //    //Staff_norm_base_flippy_06-3325
        //    {-1962741209, new GilesGameBalanceDataCache(1, ItemType.Unknown, false, FollowerType.None)},
        //    //HealthGlobe_02-3320
        //    {2140882334, new GilesGameBalanceDataCache(63, ItemType.Boots, false, FollowerType.None)},
        //    //Boots_norm_base_flippy-3495
        //    {40857596, new GilesGameBalanceDataCache(56, ItemType.Cloak, false, FollowerType.None)},
        //    //chestArmor_norm_base_flippy-3571
        //    {2058771892, new GilesGameBalanceDataCache(54, ItemType.Gem, false, FollowerType.None)},
        //    //Topaz_07-3299
        //    {-1303413119, new GilesGameBalanceDataCache(62, ItemType.Dagger, true, FollowerType.None)},
        //    //Dagger_norm_base_flippy_06-3766
        //    {-2115689174, new GilesGameBalanceDataCache(61, ItemType.Staff, false, FollowerType.None)},
        //    //Staff_norm_base_flippy_05-3744
        //    {1565456763, new GilesGameBalanceDataCache(61, ItemType.Helm, false, FollowerType.None)},
        //    //Helm_norm_base_flippy-3738
        //    {290068679, new GilesGameBalanceDataCache(57, ItemType.MightyWeapon, true, FollowerType.None)},
        //    //mightyWeapon_1H_norm_base_flippy_01-3850
        //    {-1533912124, new GilesGameBalanceDataCache(60, ItemType.Gloves, false, FollowerType.None)},
        //    //Gloves_norm_base_flippy-3917
        //    {88667232, new GilesGameBalanceDataCache(61, ItemType.Wand, true, FollowerType.None)},
        //    //Wand_norm_base_flippy_05-4365
        //    {1146967348, new GilesGameBalanceDataCache(62, ItemType.Ring, false, FollowerType.None)},
        //    //Ring_flippy-4767
        //    {-1512729955, new GilesGameBalanceDataCache(63, ItemType.Legs, false, FollowerType.None)},
        //    //pants_norm_base_flippy-4731
        //    {-1303413123, new GilesGameBalanceDataCache(55, ItemType.Dagger, true, FollowerType.None)},
        //    //Dagger_norm_base_flippy_02-4517
        //    {1700549963, new GilesGameBalanceDataCache(61, ItemType.Axe, false, FollowerType.None)},
        //    //twoHandedAxe_norm_base_flippy_03-4584
        //    {-2115689176, new GilesGameBalanceDataCache(57, ItemType.Staff, false, FollowerType.None)},
        //    //Staff_norm_base_flippy_03-4601
        //    {-335464095, new GilesGameBalanceDataCache(59, ItemType.Shield, false, FollowerType.None)},
        //    //Shield_norm_base_flippy_03-4602
        //    {-733830276, new GilesGameBalanceDataCache(52, ItemType.Belt, false, FollowerType.None)},
        //    //Belt_norm_base_flippy-4600
        //    {620036246, new GilesGameBalanceDataCache(61, ItemType.VoodooMask, false, FollowerType.None)},
        //    //Helm_norm_base_flippy-4950
        //    {-1616888603, new GilesGameBalanceDataCache(62, ItemType.Mace, false, FollowerType.None)},
        //    //twoHandedMace_norm_base_flippy_05-4944
        //    {1682228653, new GilesGameBalanceDataCache(62, ItemType.Amulet, false, FollowerType.None)},
        //    //Amulet_norm_base_flippy-5681
        //    {-733829189, new GilesGameBalanceDataCache(60, ItemType.Belt, false, FollowerType.None)},
        //    //Belt_norm_base_flippy-6048
        //    {-1303413121, new GilesGameBalanceDataCache(59, ItemType.Dagger, true, FollowerType.None)},
        //    //Dagger_norm_base_flippy_04-1383
        //    {1815809035, new GilesGameBalanceDataCache(55, ItemType.Shield, false, FollowerType.None)},
        //    //Shield_norm_base_flippy_01-2564
        //    {1565456765, new GilesGameBalanceDataCache(63, ItemType.Helm, false, FollowerType.None)},
        //    //Helm_norm_base_flippy-3402
        //    {1603007817, new GilesGameBalanceDataCache(60, ItemType.Gem, false, FollowerType.None)},
        //    //Ruby_08-3158
        //    {1236607149, new GilesGameBalanceDataCache(61, ItemType.FistWeapon, true, FollowerType.None)},
        //    //fistWeapon_norm_base_flippy_02-3659
        //    {1146967346, new GilesGameBalanceDataCache(60, ItemType.Ring, false, FollowerType.None)},
        //    //Ring_flippy-3918
        //    {-1337761336, new GilesGameBalanceDataCache(62, ItemType.Polearm, false, FollowerType.None)},
        //    //Polearm_norm_base_flippy_07-4052
        //    {181033988, new GilesGameBalanceDataCache(53, ItemType.Crossbow, false, FollowerType.None)},
        //    //XBow_norm_base_flippy_02-4078
        //    {-1337761340, new GilesGameBalanceDataCache(56, ItemType.Polearm, false, FollowerType.None)},
        //    //Polearm_norm_base_flippy_03-5687
        //    {2140882330, new GilesGameBalanceDataCache(58, ItemType.Boots, false, FollowerType.None)},
        //    //Boots_norm_base_flippy-5685
        //    {1565456761, new GilesGameBalanceDataCache(58, ItemType.Helm, false, FollowerType.None)},
        //    //Helm_norm_base_flippy-5684
        //    {-1533912125, new GilesGameBalanceDataCache(58, ItemType.Gloves, false, FollowerType.None)},
        //    //Gloves_norm_base_flippy-6368
        //    {-270936739, new GilesGameBalanceDataCache(59, ItemType.Sword, true, FollowerType.None)},
        //    //Sword_norm_base_flippy_03-6486
        //    {-1337761339, new GilesGameBalanceDataCache(58, ItemType.Polearm, false, FollowerType.None)},
        //    //Polearm_norm_base_flippy_04-7960
        //    {-2091501889, new GilesGameBalanceDataCache(63, ItemType.Bow, false, FollowerType.None)},
        //    //Bow_norm_base_flippy_06-7966
        //    {-101310578, new GilesGameBalanceDataCache(58, ItemType.Spear, true, FollowerType.None)},
        //    //Spear_norm_base_flippy_02-7962
        //    {1755623811, new GilesGameBalanceDataCache(62, ItemType.WizardHat, false, FollowerType.None)},
        //    //HelmCloth_norm_base_flippy-8781
        //    {1236607148, new GilesGameBalanceDataCache(60, ItemType.FistWeapon, true, FollowerType.None)},
        //    //fistWeapon_norm_base_flippy_03-8789
        //    {-1733388799, new GilesGameBalanceDataCache(60, ItemType.Gem, false, FollowerType.None)},
        //    //Emerald_08-10718
        //    {1682228651, new GilesGameBalanceDataCache(60, ItemType.Amulet, false, FollowerType.None)},
        //    //Amulet_norm_base_flippy-10727
        //    {-136815383, new GilesGameBalanceDataCache(53, ItemType.Mojo, false, FollowerType.None)},
        //    //Mojo_norm_base_flippy_04-10810
        //    {1700549962, new GilesGameBalanceDataCache(59, ItemType.Axe, false, FollowerType.None)},
        //    //twoHandedAxe_norm_base_flippy_02-10812
        //    {2112157586, new GilesGameBalanceDataCache(61, ItemType.MightyBelt, false, FollowerType.None)},
        //    //Belt_norm_base_flippy-11311
        //    {-1533912121, new GilesGameBalanceDataCache(63, ItemType.Gloves, false, FollowerType.None)},
        //    //Gloves_norm_base_flippy-11314
        //    {181033989, new GilesGameBalanceDataCache(54, ItemType.Crossbow, false, FollowerType.None)},
        //    //XBow_norm_base_flippy_03-12195
        //    {2140882329, new GilesGameBalanceDataCache(55, ItemType.Boots, false, FollowerType.None)},
        //    //Boots_norm_base_flippy-12217
        //    {-1303413122, new GilesGameBalanceDataCache(57, ItemType.Dagger, true, FollowerType.None)},
        //    //Dagger_norm_base_flippy_03-13817
        //    {-1303413120, new GilesGameBalanceDataCache(61, ItemType.Dagger, true, FollowerType.None)},
        //    //Dagger_norm_base_flippy_05-14074
        //    {-270936738, new GilesGameBalanceDataCache(61, ItemType.Sword, true, FollowerType.None)},
        //    //Sword_norm_base_flippy_04-14072
        //    {-1337761337, new GilesGameBalanceDataCache(61, ItemType.Polearm, false, FollowerType.None)},
        //    //Polearm_norm_base_flippy_06-14964
        //    {365492431, new GilesGameBalanceDataCache(62, ItemType.Shoulder, false, FollowerType.None)},
        //    //shoulderPads_norm_base_flippy-1437
        //    {1565456764, new GilesGameBalanceDataCache(62, ItemType.Helm, false, FollowerType.None)},
        //    //Helm_norm_base_flippy-1315
        //    {-101310577, new GilesGameBalanceDataCache(61, ItemType.Spear, true, FollowerType.None)},
        //    //Spear_norm_base_flippy_03-3318
        //    {-1512729959, new GilesGameBalanceDataCache(58, ItemType.Legs, false, FollowerType.None)},
        //    //pants_norm_base_flippy-7736
        //    {-875942695, new GilesGameBalanceDataCache(63, ItemType.Bracer, false, FollowerType.None)},
        //    //Bracers_norm_base_06-8312
        //    {-2015049108, new GilesGameBalanceDataCache(46, ItemType.CraftingPlan, false, FollowerType.None)},
        //    //CraftingPlan_Smith_Drop-11075
        //    {-733829186, new GilesGameBalanceDataCache(63, ItemType.Belt, false, FollowerType.None)},
        //    //Belt_norm_base_flippy-11066
        //    {2140882333, new GilesGameBalanceDataCache(62, ItemType.Boots, false, FollowerType.None)},
        //    //Boots_norm_base_flippy-11993
        //    {1539238478, new GilesGameBalanceDataCache(56, ItemType.Quiver, false, FollowerType.None)},
        //    //Quiver_norm_base_flippy_01-12027
        //    {-733829187, new GilesGameBalanceDataCache(62, ItemType.Belt, false, FollowerType.None)},
        //    //Belt_norm_base_flippy-13327
        //    {-231801347, new GilesGameBalanceDataCache(61, ItemType.Sword, false, FollowerType.None)},
        //    //twoHandedSword_norm_base_flippy_05-14009
        //    {-331906332, new GilesGameBalanceDataCache(62, ItemType.Shield, false, FollowerType.None)},
        //    //Shield_norm_base_flippy_06-15167
        //    {-1616888604, new GilesGameBalanceDataCache(61, ItemType.Mace, false, FollowerType.None)},
        //    //twoHandedMace_norm_base_flippy_04-16886
        //    {-875942700, new GilesGameBalanceDataCache(55, ItemType.Bracer, false, FollowerType.None)},
        //    //Bracers_norm_base_01-668
        //    {-1337761335, new GilesGameBalanceDataCache(63, ItemType.Polearm, false, FollowerType.None)},
        //    //Polearm_norm_base_flippy_08-1354
        //    {1700549964, new GilesGameBalanceDataCache(62, ItemType.Axe, false, FollowerType.None)},
        //    //twoHandedAxe_norm_base_flippy_04-1346
        //    {-363389486, new GilesGameBalanceDataCache(61, ItemType.HandCrossbow, true, FollowerType.None)},
        //    //handXBow_norm_base_flippy_06-1802
        //    {-1512729958, new GilesGameBalanceDataCache(60, ItemType.Legs, false, FollowerType.None)},
        //    //pants_norm_base_flippy-3342
        //    {-2115689175, new GilesGameBalanceDataCache(59, ItemType.Staff, false, FollowerType.None)},
        //    //Staff_norm_base_flippy_04-3662
        //    {-1411866890, new GilesGameBalanceDataCache(60, ItemType.Gem, false, FollowerType.None)},
        //    //Amethyst_08-3681
        //    {2058771893, new GilesGameBalanceDataCache(60, ItemType.Gem, false, FollowerType.None)},
        //    //Topaz_08-3683
        //    {-875942698, new GilesGameBalanceDataCache(60, ItemType.Bracer, false, FollowerType.None)},
        //    //Bracers_norm_base_03-5860
        //    {1565456760, new GilesGameBalanceDataCache(55, ItemType.Helm, false, FollowerType.None)},
        //    //Helm_norm_base_flippy-7297
        //    {-1512729957, new GilesGameBalanceDataCache(61, ItemType.Legs, false, FollowerType.None)},
        //    //pants_norm_base_flippy-8321
        //    {-875942699, new GilesGameBalanceDataCache(58, ItemType.Bracer, false, FollowerType.None)},
        //    //Bracers_norm_base_02-9464
        //    {-1337761341, new GilesGameBalanceDataCache(54, ItemType.Polearm, false, FollowerType.None)},
        //    //Polearm_norm_base_flippy_02-9531
        //    {-363389487, new GilesGameBalanceDataCache(60, ItemType.HandCrossbow, true, FollowerType.None)},
        //    //handXBow_norm_base_flippy_05-9530
        //    {-229899869, new GilesGameBalanceDataCache(57, ItemType.FollowerSpecial, false, FollowerType.Scoundrel)},
        //    //JewelBox_Flippy-10528
        //    {-733829191, new GilesGameBalanceDataCache(55, ItemType.Belt, false, FollowerType.None)},
        //    //Belt_norm_base_flippy-10549
        //    {1146967347, new GilesGameBalanceDataCache(61, ItemType.Ring, false, FollowerType.None)},
        //    //Ring_flippy-10553
        //    {1612259883, new GilesGameBalanceDataCache(58, ItemType.Chest, false, FollowerType.None)},
        //    //chestArmor_norm_base_flippy-13213
        //    {1682228652, new GilesGameBalanceDataCache(61, ItemType.Amulet, false, FollowerType.None)},
        //    //Amulet_norm_base_flippy-13904
        //    {-101310576, new GilesGameBalanceDataCache(62, ItemType.Spear, true, FollowerType.None)},
        //    //Spear_norm_base_flippy_04-14801
        //    {-333092253, new GilesGameBalanceDataCache(61, ItemType.Shield, false, FollowerType.None)},
        //    //Shield_norm_base_flippy_05-15478
        //    {-875942696, new GilesGameBalanceDataCache(62, ItemType.Bracer, false, FollowerType.None)},
        //    //Bracers_norm_base_05-2766
        //    {1661414569, new GilesGameBalanceDataCache(56, ItemType.Axe, true, FollowerType.None)},
        //    //Axe_norm_base_flippy_02-4779
        //    {-363389485, new GilesGameBalanceDataCache(62, ItemType.HandCrossbow, true, FollowerType.None)},
        //    //handXBow_norm_base_flippy_07-6008
        //    {2140881244, new GilesGameBalanceDataCache(52, ItemType.Boots, false, FollowerType.None)},
        //    //Boots_norm_base_flippy-6020
        //    {290068680, new GilesGameBalanceDataCache(61, ItemType.MightyWeapon, true, FollowerType.None)},
        //    //mightyWeapon_1H_norm_base_flippy_02-6907
        //    {365492432, new GilesGameBalanceDataCache(63, ItemType.Shoulder, false, FollowerType.None)},
        //    //shoulderPads_norm_base_flippy-7287
        //    {-1656025083, new GilesGameBalanceDataCache(51, ItemType.Mace, true, FollowerType.None)},
        //    //Mace_norm_base_flippy_07-14357
        //    {-635267403, new GilesGameBalanceDataCache(60, ItemType.CeremonialDagger, true, FollowerType.None)},
        //    //ceremonialDagger_norm_base_flippy_03-14369
        //    {-242893289, new GilesGameBalanceDataCache(60, ItemType.SpiritStone, false, FollowerType.None)},
        //    //Helm_norm_base_flippy-17237
        //    {1539238479, new GilesGameBalanceDataCache(59, ItemType.Quiver, false, FollowerType.None)},
        //    //Quiver_norm_base_flippy_01-17846
        //    {1612259885, new GilesGameBalanceDataCache(61, ItemType.Chest, false, FollowerType.None)},
        //    //chestArmor_norm_base_flippy-4464
        //    {1612259886, new GilesGameBalanceDataCache(62, ItemType.Chest, false, FollowerType.None)},
        //    //chestArmor_norm_base_flippy-7114
        //    {290068681, new GilesGameBalanceDataCache(62, ItemType.MightyWeapon, true, FollowerType.None)},
        //    //mightyWeapon_1H_norm_base_flippy_03-7147
        //    {-1616888605, new GilesGameBalanceDataCache(58, ItemType.Mace, false, FollowerType.None)},
        //    //twoHandedMace_norm_base_flippy_03-7560
        //    {1146967345, new GilesGameBalanceDataCache(57, ItemType.Ring, false, FollowerType.None)},
        //    //Ring_flippy-7571
        //    {-2091501894, new GilesGameBalanceDataCache(52, ItemType.Bow, false, FollowerType.None)},
        //    //Bow_norm_base_flippy_01-7570
        //    {1612259882, new GilesGameBalanceDataCache(55, ItemType.Chest, false, FollowerType.None)},
        //    //chestArmor_norm_base_flippy-7569
        //    {1661414570, new GilesGameBalanceDataCache(58, ItemType.Axe, true, FollowerType.None)},
        //    //Axe_norm_base_flippy_03-7578
        //    {-363389488, new GilesGameBalanceDataCache(58, ItemType.HandCrossbow, true, FollowerType.None)},
        //    //handXBow_norm_base_flippy_04-9381
        //    {1147341802, new GilesGameBalanceDataCache(60, ItemType.FollowerSpecial, false, FollowerType.Templar)},
        //    //JewelBox_Flippy-10391
        //    {-1512729960, new GilesGameBalanceDataCache(55, ItemType.Legs, false, FollowerType.None)},
        //    //pants_norm_base_flippy-13909
        //    {365492429, new GilesGameBalanceDataCache(60, ItemType.Shoulder, false, FollowerType.None)},
        //    //shoulderPads_norm_base_flippy-13913
        //    {-101310575, new GilesGameBalanceDataCache(63, ItemType.Spear, true, FollowerType.None)},
        //    //Spear_norm_base_flippy_05-15926
        //    {88667230, new GilesGameBalanceDataCache(58, ItemType.Wand, true, FollowerType.None)},
        //    //Wand_norm_base_flippy_03-17386
        //    {-1656023995, new GilesGameBalanceDataCache(62, ItemType.Mace, true, FollowerType.None)},
        //    //Mace_norm_base_flippy_06-17434
        //    {-363389490, new GilesGameBalanceDataCache(54, ItemType.HandCrossbow, true, FollowerType.None)},
        //    //handXbow_norm_base_flippy_02-19180
        //    {365491342, new GilesGameBalanceDataCache(52, ItemType.Shoulder, false, FollowerType.None)},
        //    //shoulderPads_norm_base_flippy-19272
        //    {365492430, new GilesGameBalanceDataCache(61, ItemType.Shoulder, false, FollowerType.None)},
        //    //shoulderPads_norm_base_flippy-20609
        //    {-136814295, new GilesGameBalanceDataCache(61, ItemType.Mojo, false, FollowerType.None)},
        //    //Mojo_norm_base_flippy_03-24992
        //    {-1512729956, new GilesGameBalanceDataCache(62, ItemType.Legs, false, FollowerType.None)},
        //    //pants_norm_base_flippy-3581
        //    {181033991, new GilesGameBalanceDataCache(59, ItemType.Crossbow, false, FollowerType.None)},
        //    //XBow_norm_base_flippy_05-5325
        //    {-2091501892, new GilesGameBalanceDataCache(58, ItemType.Bow, false, FollowerType.None)},
        //    //Bow_norm_base_flippy_03-6306
        //    {1565455675, new GilesGameBalanceDataCache(52, ItemType.Helm, false, FollowerType.None)},
        //    //Helm_norm_base_flippy-2176
        //    {181033992, new GilesGameBalanceDataCache(61, ItemType.Crossbow, false, FollowerType.None)},
        //    //XBow_norm_base_flippy_06-3659
        //    {1661413485, new GilesGameBalanceDataCache(52, ItemType.Axe, true, FollowerType.None)},
        //    //Axe_norm_base_flippy_07-3991
        //    {-1533912122, new GilesGameBalanceDataCache(62, ItemType.Gloves, false, FollowerType.None)},
        //    //Gloves_norm_base_flippy-3996
        //    {2112157587, new GilesGameBalanceDataCache(62, ItemType.MightyBelt, false, FollowerType.None)},
        //    //Belt_norm_base_flippy-6378
        //    {-1303413118, new GilesGameBalanceDataCache(63, ItemType.Dagger, true, FollowerType.None)},
        //    //Dagger_norm_base_flippy_07-8259
        //    {-1733388800, new GilesGameBalanceDataCache(54, ItemType.Gem, false, FollowerType.None)},
        //    //Emerald_07-8261
        //    {1612258797, new GilesGameBalanceDataCache(52, ItemType.Chest, false, FollowerType.None)},
        //    //chestArmor_norm_base_flippy-8260
        //    {1682228650, new GilesGameBalanceDataCache(55, ItemType.Amulet, false, FollowerType.None)},
        //    //Amulet_norm_base_flippy-9071
        //    {-1411866891, new GilesGameBalanceDataCache(54, ItemType.Gem, false, FollowerType.None)},
        //    //Amethyst_07-2037
        //    {-1512731045, new GilesGameBalanceDataCache(52, ItemType.Legs, false, FollowerType.None)},
        //    //pants_norm_base_flippy-2899
        //    {88667231, new GilesGameBalanceDataCache(60, ItemType.Wand, true, FollowerType.None)},
        //    //Wand_norm_base_flippy_04-3797
        //    {-2115689177, new GilesGameBalanceDataCache(55, ItemType.Staff, false, FollowerType.None)},
        //    //Staff_norm_base_flippy_02-5676
        //    {-1656023997, new GilesGameBalanceDataCache(59, ItemType.Mace, true, FollowerType.None)},
        //    //Mace_norm_base_flippy_04-11365
        //    {-136814294, new GilesGameBalanceDataCache(62, ItemType.Mojo, false, FollowerType.None)},
        //    //Mojo_norm_base_flippy_04-11892
        //    {329204073, new GilesGameBalanceDataCache(61, ItemType.MightyWeapon, false, FollowerType.None)},
        //    //mightyWeapon_2H_norm_base_flippy_02-1331
        //    {-270936736, new GilesGameBalanceDataCache(63, ItemType.Sword, true, FollowerType.None)},
        //    //Sword_norm_base_flippy_07-2509
        //    {-1616888602, new GilesGameBalanceDataCache(63, ItemType.Mace, false, FollowerType.None)},
        //    //twoHandedMace_norm_base_flippy_06-3566
        //    {-1656023999, new GilesGameBalanceDataCache(55, ItemType.Mace, true, FollowerType.None)},
        //    //Mace_norm_base_flippy_02-6299
        //    {329204072, new GilesGameBalanceDataCache(56, ItemType.MightyWeapon, false, FollowerType.None)},
        //    //mightyWeapon_2H_norm_base_flippy_01-6293
        //    {-733829190, new GilesGameBalanceDataCache(58, ItemType.Belt, false, FollowerType.None)},
        //    //Belt_norm_base_flippy-7011
        //    {1661414573, new GilesGameBalanceDataCache(62, ItemType.Axe, true, FollowerType.None)},
        //    //Axe_norm_base_flippy_06-8628
        //    {181033994, new GilesGameBalanceDataCache(63, ItemType.Crossbow, false, FollowerType.None)},
        //    //XBow_norm_base_flippy_08-9102
        //    {1603007816, new GilesGameBalanceDataCache(54, ItemType.Gem, false, FollowerType.None)},
        //    //Ruby_07-11231
        //    {-1962741148, new GilesGameBalanceDataCache(1, ItemType.Unknown, false, FollowerType.None)},
        //    //HealthGlobe_02-15599
        //    {-334278174, new GilesGameBalanceDataCache(60, ItemType.Shield, false, FollowerType.None)},
        //    //Shield_norm_base_flippy_04-11811
        //    {1539238480, new GilesGameBalanceDataCache(60, ItemType.Quiver, false, FollowerType.None)},
        //    //Quiver_norm_base_flippy_01-13479
        //    {-1656023996, new GilesGameBalanceDataCache(61, ItemType.Mace, true, FollowerType.None)},
        //    //Mace_norm_base_flippy_05-16080
        //    {1815809036, new GilesGameBalanceDataCache(57, ItemType.Shield, false, FollowerType.None)},
        //    //Shield_norm_base_flippy_02-16556
        //    {40857598, new GilesGameBalanceDataCache(61, ItemType.Cloak, false, FollowerType.None)},
        //    //chestArmor_norm_base_flippy-18151
        //    {329204074, new GilesGameBalanceDataCache(62, ItemType.MightyWeapon, false, FollowerType.None)},
        //    //mightyWeapon_2H_norm_base_flippy_03-18904
        //    {-1656023998, new GilesGameBalanceDataCache(57, ItemType.Mace, true, FollowerType.None)},
        //    //Mace_norm_base_flippy_03-19117
        //    {329202986, new GilesGameBalanceDataCache(51, ItemType.MightyWeapon, false, FollowerType.None)},
        //    //mightyWeapon_2H_norm_base_flippy_04-21596
        //    {1755622722, new GilesGameBalanceDataCache(53, ItemType.WizardHat, false, FollowerType.None)},
        //    //HelmCloth_norm_base_flippy-1940
        //    {1236607150, new GilesGameBalanceDataCache(62, ItemType.FistWeapon, true, FollowerType.None)},
        //    //fistWeapon_norm_base_flippy_03-3679
        //    {-231801345, new GilesGameBalanceDataCache(63, ItemType.Sword, false, FollowerType.None)},
        //    //twoHandedSword_norm_base_flippy_06-7909
        //    {365492428, new GilesGameBalanceDataCache(58, ItemType.Shoulder, false, FollowerType.None)},
        //    //shoulderPads_norm_base_flippy-8662
        //    {620036244, new GilesGameBalanceDataCache(56, ItemType.VoodooMask, false, FollowerType.None)},
        //    //Helm_norm_base_flippy-11733
        //    {88667228, new GilesGameBalanceDataCache(54, ItemType.Wand, true, FollowerType.None)},
        //    //Wand_norm_base_flippy_01-19036
        //    {-875942697, new GilesGameBalanceDataCache(61, ItemType.Bracer, false, FollowerType.None)},
        //    //Bracers_norm_base_04-21441
        //    {-363389484, new GilesGameBalanceDataCache(63, ItemType.HandCrossbow, true, FollowerType.None)},
        //    //handXBow_norm_base_flippy_08-21449
        //    {-270936737, new GilesGameBalanceDataCache(62, ItemType.Sword, true, FollowerType.None)},
        //    //Sword_norm_base_flippy_08-3422
        //    {1661414568, new GilesGameBalanceDataCache(54, ItemType.Axe, true, FollowerType.None)},
        //    //Axe_norm_base_flippy_01-4034
        //    {2112156498, new GilesGameBalanceDataCache(53, ItemType.MightyBelt, false, FollowerType.None)},
        //    //Belt_norm_base_flippy-4089
        //    {-2091501890, new GilesGameBalanceDataCache(62, ItemType.Bow, false, FollowerType.None)},
        //    //Bow_norm_base_flippy_05-5750
        //    {1539238481, new GilesGameBalanceDataCache(61, ItemType.Quiver, false, FollowerType.None)},
        //    //Quiver_norm_base_flippy_01-7305
        //    {-363389489, new GilesGameBalanceDataCache(56, ItemType.HandCrossbow, true, FollowerType.None)},
        //    //handXBow_norm_base_flippy_03-7443
        //    {-1533912126, new GilesGameBalanceDataCache(55, ItemType.Gloves, false, FollowerType.None)},
        //    //Gloves_norm_base_flippy-7516
        //    {-2091501893, new GilesGameBalanceDataCache(55, ItemType.Bow, false, FollowerType.None)},
        //    //Bow_norm_base_flippy_02-10478
        //    {-231801349, new GilesGameBalanceDataCache(55, ItemType.Sword, false, FollowerType.None)},
        //    //twoHandedSword_norm_base_flippy_03-10866
        //    {-231801348, new GilesGameBalanceDataCache(58, ItemType.Sword, false, FollowerType.None)},
        //    //twoHandedSword_norm_base_flippy_02-15614
        //    {-1337761342, new GilesGameBalanceDataCache(52, ItemType.Polearm, false, FollowerType.None)},
        //    //Polearm_norm_base_flippy_01-15747
        //    {620036245, new GilesGameBalanceDataCache(60, ItemType.VoodooMask, false, FollowerType.None)},
        //    //Helm_norm_base_flippy-16062
        //    {1755623809, new GilesGameBalanceDataCache(60, ItemType.WizardHat, false, FollowerType.None)},
        //    //HelmCloth_norm_base_flippy-18920
        //    {-242893288, new GilesGameBalanceDataCache(61, ItemType.SpiritStone, false, FollowerType.None)},
        //    //Helm_norm_base_flippy-18926
        //    {1612259884, new GilesGameBalanceDataCache(60, ItemType.Chest, false, FollowerType.None)},
        //    //chestArmor_norm_base_flippy-22557
        //    {181033990, new GilesGameBalanceDataCache(56, ItemType.Crossbow, false, FollowerType.None)},
        //    //XBow_norm_base_flippy_04-22861
        //    {1661414571, new GilesGameBalanceDataCache(60, ItemType.Axe, true, FollowerType.None)},
        //    //Axe_norm_base_flippy_04-2058
        //    {1146967320, new GilesGameBalanceDataCache(51, ItemType.Ring, false, FollowerType.None)},
        //    //Ring_flippy-5049
        //    {-229899868, new GilesGameBalanceDataCache(60, ItemType.FollowerSpecial, false, FollowerType.Scoundrel)},
        //    //JewelBox_Flippy-7661
        //    {1146967321, new GilesGameBalanceDataCache(54, ItemType.Ring, false, FollowerType.None)},
        //    //Ring_flippy-8882
        //    {-242893290, new GilesGameBalanceDataCache(56, ItemType.SpiritStone, false, FollowerType.None)},
        //    //Helm_norm_base_flippy-8875
        //    {761439027, new GilesGameBalanceDataCache(60, ItemType.FollowerSpecial, false, FollowerType.Enchantress)},
        //    //JewelBox_Flippy-12503
        //    {-231801346, new GilesGameBalanceDataCache(62, ItemType.Sword, false, FollowerType.None)},
        //    //twoHandedSword_norm_base_flippy_04-18285
        //    {-1656024000, new GilesGameBalanceDataCache(53, ItemType.Mace, true, FollowerType.None)},
        //    //Mace_norm_base_flippy_01-18617
        //    {1755623808, new GilesGameBalanceDataCache(56, ItemType.WizardHat, false, FollowerType.None)},
        //    //HelmCloth_norm_base_flippy-1483
        //    {40857597, new GilesGameBalanceDataCache(60, ItemType.Cloak, false, FollowerType.None)},
        //    //chestArmor_norm_base_flippy-8723
        //    {-875943785, new GilesGameBalanceDataCache(52, ItemType.Bracer, false, FollowerType.None)},
        //    //Bracers_norm_base_05-11841
        //    {1700549961, new GilesGameBalanceDataCache(55, ItemType.Axe, false, FollowerType.None)},
        //    //twoHandedAxe_norm_base_flippy_01-14697
        //    {-1656023994, new GilesGameBalanceDataCache(63, ItemType.Mace, true, FollowerType.None)},
        //    //Mace_norm_base_flippy_07-23400
        //    {-2115690261, new GilesGameBalanceDataCache(51, ItemType.Staff, false, FollowerType.None)},
        //    //Staff_norm_base_flippy_07-440
        //    {1771751032, new GilesGameBalanceDataCache(61, ItemType.Daibo, false, FollowerType.None)},
        //    //combatStaff_norm_base_flippy_02-9926
        //    {-270936742, new GilesGameBalanceDataCache(53, ItemType.Sword, true, FollowerType.None)},
        //    //Sword_norm_base_flippy_02-11499
        //    {-2091501891, new GilesGameBalanceDataCache(61, ItemType.Bow, false, FollowerType.None)},
        //    //Bow_norm_base_flippy_04-15879
        //    {1612259887, new GilesGameBalanceDataCache(63, ItemType.Chest, false, FollowerType.None)},
        //    //chestArmor_norm_base_flippy-16971
        //    {88667229, new GilesGameBalanceDataCache(56, ItemType.Wand, true, FollowerType.None)},
        //    //Wand_norm_base_flippy_02-17893
        //    {-242893287, new GilesGameBalanceDataCache(62, ItemType.SpiritStone, false, FollowerType.None)},
        //    //Helm_norm_base_flippy-21766
        //    {-635267401, new GilesGameBalanceDataCache(62, ItemType.CeremonialDagger, true, FollowerType.None)},
        //    //ceremonialDagger_norm_base_flippy_03-5833
        //    {1661414574, new GilesGameBalanceDataCache(63, ItemType.Axe, true, FollowerType.None)},
        //    //Axe_norm_base_flippy_07-7199
        //    {1771751034, new GilesGameBalanceDataCache(63, ItemType.Daibo, false, FollowerType.None)},
        //    //combatStaff_norm_base_flippy_04-11249
        //    {2112157584, new GilesGameBalanceDataCache(56, ItemType.MightyBelt, false, FollowerType.None)},
        //    //Belt_norm_base_flippy-11552
        //    {40857599, new GilesGameBalanceDataCache(62, ItemType.Cloak, false, FollowerType.None)},
        //    //chestArmor_norm_base_flippy-14809
        //    {-363389491, new GilesGameBalanceDataCache(52, ItemType.HandCrossbow, true, FollowerType.None)},
        //    //handXbow_norm_base_flippy_01-15178
        //    {-2115689172, new GilesGameBalanceDataCache(63, ItemType.Staff, false, FollowerType.None)},
        //    //Staff_norm_base_flippy_07-17487
        //    {1755623810, new GilesGameBalanceDataCache(61, ItemType.WizardHat, false, FollowerType.None)},
        //    //HelmCloth_norm_base_flippy-21671
        //    {1539237393, new GilesGameBalanceDataCache(53, ItemType.Quiver, false, FollowerType.None)},
        //    //Quiver_norm_base_flippy_01-23717
        //    {1236607147, new GilesGameBalanceDataCache(56, ItemType.FistWeapon, true, FollowerType.None)},
        //    //fistWeapon_norm_base_flippy_02-8621
        //    {1905181656, new GilesGameBalanceDataCache(62, ItemType.Orb, false, FollowerType.None)},
        //    //orb_norm_base_flippy_04-5332
        //    {620035158, new GilesGameBalanceDataCache(53, ItemType.VoodooMask, false, FollowerType.None)},
        //    //Helm_norm_base_flippy-490
        //    {1288600121, new GilesGameBalanceDataCache(46, ItemType.CraftingPlan, false, FollowerType.None)},
        //    //CraftingPlan_Smith_Drop-8240
        //    {1147341801, new GilesGameBalanceDataCache(57, ItemType.FollowerSpecial, false, FollowerType.Templar)},
        //    //JewelBox_Flippy-6201
        //    {-635267400, new GilesGameBalanceDataCache(63, ItemType.CeremonialDagger, true, FollowerType.None)},
        //    //ceremonialDagger_norm_base_flippy_04-24457
        //    {1700549965, new GilesGameBalanceDataCache(63, ItemType.Axe, false, FollowerType.None)},
        //    //twoHandedAxe_norm_base_flippy_05-7304
        //    {-2115689178, new GilesGameBalanceDataCache(53, ItemType.Staff, false, FollowerType.None)},
        //    //Staff_norm_base_flippy_01-20183
        //    {365492427, new GilesGameBalanceDataCache(55, ItemType.Shoulder, false, FollowerType.None)},
        //    //shoulderPads_norm_base_flippy-1374
        //    {290067593, new GilesGameBalanceDataCache(52, ItemType.MightyWeapon, true, FollowerType.None)},
        //    //mightyWeapon_1H_norm_base_flippy_04-6474
        //    {-136814296, new GilesGameBalanceDataCache(60, ItemType.Mojo, false, FollowerType.None)},
        //    //Mojo_norm_base_flippy_02-9438
        //    {-1533913211, new GilesGameBalanceDataCache(52, ItemType.Gloves, false, FollowerType.None)},
        //    //Gloves_norm_base_flippy-15515
        //    {-270936741, new GilesGameBalanceDataCache(55, ItemType.Sword, true, FollowerType.None)},
        //    //Sword_norm_base_flippy_05-23963
        //    {-101310579, new GilesGameBalanceDataCache(55, ItemType.Spear, true, FollowerType.None)},
        //    //Spear_norm_base_flippy_01-3836
        //    {88667233, new GilesGameBalanceDataCache(62, ItemType.Wand, true, FollowerType.None)},
        //    //Wand_norm_base_flippy_06-5434
        //    {1905181655, new GilesGameBalanceDataCache(61, ItemType.Orb, false, FollowerType.None)},
        //    //orb_norm_base_flippy_03-18857
        //    {1236607151, new GilesGameBalanceDataCache(63, ItemType.FistWeapon, true, FollowerType.None)},
        //    //fistWeapon_norm_base_flippy_04-4638
        //    {2112157585, new GilesGameBalanceDataCache(60, ItemType.MightyBelt, false, FollowerType.None)},
        //    //Belt_norm_base_flippy-688
        //    {88667234, new GilesGameBalanceDataCache(63, ItemType.Wand, true, FollowerType.None)},
        //    //Wand_norm_base_flippy_07-2315
        //    {761439026, new GilesGameBalanceDataCache(57, ItemType.FollowerSpecial, false, FollowerType.Enchantress)},
        //    //JewelBox_Flippy-5680
        //    {-1337761338, new GilesGameBalanceDataCache(60, ItemType.Polearm, false, FollowerType.None)},
        //    //Polearm_norm_base_flippy_05-1977
        //    {1236607146, new GilesGameBalanceDataCache(52, ItemType.FistWeapon, true, FollowerType.None)},
        //    //fistWeapon_norm_base_flippy_01-6741
        //    {-635267402, new GilesGameBalanceDataCache(61, ItemType.CeremonialDagger, true, FollowerType.None)},
        //    //ceremonialDagger_norm_base_flippy_02-682
        //    {1771751033, new GilesGameBalanceDataCache(62, ItemType.Daibo, false, FollowerType.None)},
        //    //combatStaff_norm_base_flippy_03-1186
        //    {290068682, new GilesGameBalanceDataCache(63, ItemType.MightyWeapon, true, FollowerType.None)},
        //    //mightyWeapon_1H_norm_base_flippy_04-1455
        //    {1815807951, new GilesGameBalanceDataCache(51, ItemType.Shield, false, FollowerType.None)},
        //    //Shield_norm_base_flippy_06-7216
        //    {-242894376, new GilesGameBalanceDataCache(53, ItemType.SpiritStone, false, FollowerType.None)},
        //    //Helm_norm_base_flippy-7932
        //    {-1303413124, new GilesGameBalanceDataCache(53, ItemType.Dagger, true, FollowerType.None)},
        //    //Dagger_norm_base_flippy_01-7929
        //    {-136814297, new GilesGameBalanceDataCache(56, ItemType.Mojo, false, FollowerType.None)},
        //    //Mojo_norm_base_flippy_01-10670
        //    {-1303414207, new GilesGameBalanceDataCache(51, ItemType.Dagger, true, FollowerType.None)},
        //    //Dagger_norm_base_flippy_07-15282
        //    {-270936740, new GilesGameBalanceDataCache(57, ItemType.Sword, true, FollowerType.None)},
        //    //Sword_norm_base_flippy_06-22060
        //    {88666145, new GilesGameBalanceDataCache(52, ItemType.Wand, true, FollowerType.None)},
        //    //Wand_norm_base_flippy_07-25299
        //    {-275669100, new GilesGameBalanceDataCache(46, ItemType.CraftingPlan, false, FollowerType.None)},
        //    //CraftingPlan_Smith_Drop-6906
        //    {1815807952, new GilesGameBalanceDataCache(53, ItemType.Shield, false, FollowerType.None)},
        //    //Shield_norm_base_flippy_07-45607
        //    {1905181654, new GilesGameBalanceDataCache(60, ItemType.Orb, false, FollowerType.None)},
        //    //orb_norm_base_flippy_02-3074
        //    {-231801350, new GilesGameBalanceDataCache(52, ItemType.Sword, false, FollowerType.None)},
        //    //twoHandedSword_norm_base_flippy_01-10063
        //    {620036247, new GilesGameBalanceDataCache(62, ItemType.VoodooMask, false, FollowerType.None)},
        //    //Helm_norm_base_flippy-17813
        //    {-229899870, new GilesGameBalanceDataCache(54, ItemType.FollowerSpecial, false, FollowerType.Scoundrel)},
        //    //JewelBox_Flippy-66005
        //    {-270936743, new GilesGameBalanceDataCache(51, ItemType.Sword, true, FollowerType.None)},
        //    //Sword_norm_base_flippy_01-9813
        //    {1771751031, new GilesGameBalanceDataCache(58, ItemType.Daibo, false, FollowerType.None)},
        //    //combatStaff_norm_base_flippy_03-996
        //    {-635267404, new GilesGameBalanceDataCache(56, ItemType.CeremonialDagger, true, FollowerType.None)},
        //    //ceremonialDagger_norm_base_flippy_02-20442
        //    {-1616888607, new GilesGameBalanceDataCache(52, ItemType.Mace, false, FollowerType.None)},
        //    //twoHandedMace_norm_base_flippy_01-12628
        //    {1147341800, new GilesGameBalanceDataCache(54, ItemType.FollowerSpecial, false, FollowerType.Templar)},
        //    //JewelBox_Flippy-4753
        //    {1539238482, new GilesGameBalanceDataCache(62, ItemType.Quiver, false, FollowerType.None)},
        //    //Quiver_norm_base_flippy_01-16011
        //    {761439025, new GilesGameBalanceDataCache(54, ItemType.FollowerSpecial, false, FollowerType.Enchantress)},
        //    //JewelBox_Flippy-17285
        //    {1771751030, new GilesGameBalanceDataCache(55, ItemType.Daibo, false, FollowerType.None)},
        //    //combatStaff_norm_base_flippy_02-19316
        //    {1771751029, new GilesGameBalanceDataCache(52, ItemType.Daibo, false, FollowerType.None)},
        //    //combatStaff_norm_base_flippy_01-10268
        //    {1905180567, new GilesGameBalanceDataCache(53, ItemType.Orb, false, FollowerType.None)},
        //    //orb_norm_base_flippy_04-10666
        //    {181032905, new GilesGameBalanceDataCache(52, ItemType.Crossbow, false, FollowerType.None)},
        //    //XBow_norm_base_flippy_08-15728
        //    {-246124382, new GilesGameBalanceDataCache(63, ItemType.Shoulder, false, FollowerType.None)},
        //    //shoulderPads_norm_base_flippy-9313
        //    {1809242064, new GilesGameBalanceDataCache(56, ItemType.Gloves, false, FollowerType.None)},
        //    //Gloves_norm_base_flippy-33597
        //    {-1137443897, new GilesGameBalanceDataCache(61, ItemType.Amulet, false, FollowerType.None)},
        //    //Amulet_norm_base_flippy-10421
        //    {1905181653, new GilesGameBalanceDataCache(56, ItemType.Orb, false, FollowerType.None)},
        //    //orb_norm_base_flippy_01-18233
        //    {329204075, new GilesGameBalanceDataCache(63, ItemType.MightyWeapon, false, FollowerType.None)},
        //    //mightyWeapon_2H_norm_base_flippy_04-14928
        //    {386201988, new GilesGameBalanceDataCache(62, ItemType.Sword, false, FollowerType.None)},
        //    //twoHandedSword_norm_base_flippy_07-59260
        //    {-1849339001, new GilesGameBalanceDataCache(63, ItemType.Unknown, false, FollowerType.None)},
        //    //Helm_norm_base_flippy-14437
        //    {1700548876, new GilesGameBalanceDataCache(51, ItemType.Axe, false, FollowerType.None)},
        //    //twoHandedAxe_norm_base_flippy_05-10421
        //    {40856510, new GilesGameBalanceDataCache(53, ItemType.Cloak, false, FollowerType.None)},
        //    //chestArmor_norm_base_flippy-9233
        //    {-635267405, new GilesGameBalanceDataCache(52, ItemType.CeremonialDagger, true, FollowerType.None)},
        //    //ceremonialDagger_norm_base_flippy_01-10948
        //    {-101311664, new GilesGameBalanceDataCache(52, ItemType.Spear, true, FollowerType.None)},
        //    //Spear_norm_base_flippy_05-2331
        //    {255305004, new GilesGameBalanceDataCache(46, ItemType.CraftingPlan, false, FollowerType.None)},
        //    //CraftingPlan_Smith_Drop-12352
        //    {402571149, new GilesGameBalanceDataCache(63, ItemType.Bracer, false, FollowerType.None)},
        //    //Bracers_norm_base_01-7789
        //    {-247310303, new GilesGameBalanceDataCache(58, ItemType.Shoulder, false, FollowerType.None)},
        //    //shoulderPads_norm_base_flippy-21253
        //    {-327168932, new GilesGameBalanceDataCache(63, ItemType.Quiver, false, FollowerType.None)},
        //    //Quiver_norm_base_flippy_01-49247
        //    {1810427985, new GilesGameBalanceDataCache(63, ItemType.Gloves, false, FollowerType.None)},
        //    //Gloves_norm_base_flippy-57737
        //    {399013386, new GilesGameBalanceDataCache(60, ItemType.Bracer, false, FollowerType.None)},
        //    //Bracers_norm_base_01-16344
        //    {-494657717, new GilesGameBalanceDataCache(63, ItemType.WizardHat, false, FollowerType.None)},
        //    //HelmCloth_norm_base_flippy-19691
        //    {-578170868, new GilesGameBalanceDataCache(61, ItemType.Ring, false, FollowerType.None)},
        //    //Ring_flippy-17344
        //    {-1953228509, new GilesGameBalanceDataCache(63, ItemType.VoodooMask, false, FollowerType.None)},
        //    //Helm_norm_base_flippy-17168
        //    {-1960344035, new GilesGameBalanceDataCache(58, ItemType.VoodooMask, false, FollowerType.None)},
        //    //Helm_norm_base_flippy-18639
        //    {253088131, new GilesGameBalanceDataCache(63, ItemType.Legs, false, FollowerType.None)},
        //    //pants_norm_base_flippy-67142
        //    {-1961529956, new GilesGameBalanceDataCache(62, ItemType.VoodooMask, false, FollowerType.None)},
        //    //Helm_norm_base_flippy-26460
        //    {1841261931, new GilesGameBalanceDataCache(61, ItemType.Gloves, false, FollowerType.None)},
        //    //Gloves_norm_base_flippy-24850
        //    {-1855268606, new GilesGameBalanceDataCache(62, ItemType.Unknown, false, FollowerType.None)},
        //    //Helm_norm_base_flippy-69134
        //    {469402902, new GilesGameBalanceDataCache(60, ItemType.Chest, false, FollowerType.None)},
        //    //chestArmor_norm_base_flippy-54263
        //    {-1271477944, new GilesGameBalanceDataCache(54, ItemType.Cloak, false, FollowerType.None)},
        //    //chestArmor_norm_base_flippy-72607
        //    {761573024, new GilesGameBalanceDataCache(1, ItemType.Unknown, false, FollowerType.None)},
        //    //BlackRockLedger01-7009
        //    {761573025, new GilesGameBalanceDataCache(1, ItemType.Unknown, false, FollowerType.None)},
        //    //BlackRockLedger02-7561
        //    {761573026, new GilesGameBalanceDataCache(1, ItemType.Unknown, false, FollowerType.None)},
        //    //BlackRockLedger03-8774
        //    {761573027, new GilesGameBalanceDataCache(1, ItemType.Unknown, false, FollowerType.None)},
        //    //BlackRockLedger04-9277
        //    {761573028, new GilesGameBalanceDataCache(1, ItemType.Unknown, false, FollowerType.None)},
        //    //BlackRockLedger05-7156
        //    {761573029, new GilesGameBalanceDataCache(1, ItemType.Unknown, false, FollowerType.None)},
        //    //BlackRockLedger06-7390
        //    {193631, new GilesGameBalanceDataCache(0, ItemType.Unknown, false, FollowerType.None)},
        //    //A2C2AlcarnusPrisoner2-5586
        //    {-1385743629, new GilesGameBalanceDataCache(1, ItemType.Unknown, false, FollowerType.None)},
        //    //Lore_AzmodansOrders6-41895
        //    {543691114, new GilesGameBalanceDataCache(46, ItemType.CraftingPlan, false, FollowerType.None)},
        //    //CraftingPlan_Smith_Drop-13741
        //    {368302887, new GilesGameBalanceDataCache(46, ItemType.CraftingPlan, false, FollowerType.None)},
        //    //CraftingPlan_Smith_Drop-11107
        //    {-636820188, new GilesGameBalanceDataCache(46, ItemType.CraftingPlan, false, FollowerType.None)},
        //    //CraftingPlan_Smith_Drop-18831
        //    {-576445432, new GilesGameBalanceDataCache(46, ItemType.CraftingPlan, false, FollowerType.None)},
        //    //CraftingPlan_Smith_Drop-20698
        //    {-275669102, new GilesGameBalanceDataCache(46, ItemType.CraftingPlan, false, FollowerType.None)},
        //    //CraftingPlan_Smith_Drop-1847
        //    {435695962, new GilesGameBalanceDataCache(46, ItemType.CraftingPlan, false, FollowerType.None)},
        //    //CraftingPlan_Smith_Drop-10675
        //    {-636820187, new GilesGameBalanceDataCache(46, ItemType.CraftingPlan, false, FollowerType.None)},
        //    //CraftingPlan_Smith_Drop-14144
        //    {-115137849, new GilesGameBalanceDataCache(46, ItemType.CraftingPlan, false, FollowerType.None)},
        //    //CraftingPlan_Smith_Drop-31858
        //    {1108898771, new GilesGameBalanceDataCache(46, ItemType.CraftingPlan, false, FollowerType.None)},
        //    //CraftingPlan_Smith_Drop-15883
        //    {1134806015, new GilesGameBalanceDataCache(46, ItemType.CraftingPlan, false, FollowerType.None)},
        //    //CraftingPlan_Smith_Drop-17495
        //    {-1689047028, new GilesGameBalanceDataCache(46, ItemType.CraftingPlan, false, FollowerType.None)},
        //    //CraftingPlan_Smith_Drop-12708
        //    {-1051150314, new GilesGameBalanceDataCache(46, ItemType.CraftingPlan, false, FollowerType.None)},
        //    //CraftingPlan_Smith_Drop-18380
        //    {623242820, new GilesGameBalanceDataCache(46, ItemType.CraftingPlan, false, FollowerType.None)},
        //    //CraftingPlan_Smith_Drop-5655
        //    {-1660666895, new GilesGameBalanceDataCache(46, ItemType.CraftingPlan, false, FollowerType.None)},
        //    //CraftingPlan_Smith_Drop-18640
        //    {-1661852814, new GilesGameBalanceDataCache(46, ItemType.CraftingPlan, false, FollowerType.None)},
        //    //CraftingPlan_Smith_Drop-36893
        //    {368302886, new GilesGameBalanceDataCache(46, ItemType.CraftingPlan, false, FollowerType.None)},
        //    //CraftingPlan_Smith_Drop-52051
        //    {-1205502140, new GilesGameBalanceDataCache(46, ItemType.CraftingPlan, false, FollowerType.None)},
        //    //CraftingPlan_Smith_Drop-52452
        //    {-1162323497, new GilesGameBalanceDataCache(46, ItemType.CraftingPlan, false, FollowerType.None)},
        //    //CraftingPlan_Smith_Drop-43350
        //    {-576445431, new GilesGameBalanceDataCache(46, ItemType.CraftingPlan, false, FollowerType.None)},
        //    //CraftingPlan_Smith_Drop-7553
        //    {-807237754, new GilesGameBalanceDataCache(46, ItemType.CraftingPlan, false, FollowerType.None)},
        //    //CraftingPlan_Smith_Drop-22338
        //    {-807237753, new GilesGameBalanceDataCache(46, ItemType.CraftingPlan, false, FollowerType.None)},
        //    //CraftingPlan_Smith_Drop-8329
        //    {82340209, new GilesGameBalanceDataCache(46, ItemType.CraftingPlan, false, FollowerType.None)},
        //    //CraftingPlan_Smith_Drop-28639
        //    {398631475, new GilesGameBalanceDataCache(46, ItemType.CraftingPlan, false, FollowerType.None)},
        //    //CraftingPlan_Smith_Drop-65157
        //    {543691115, new GilesGameBalanceDataCache(46, ItemType.CraftingPlan, false, FollowerType.None)},
        //    //CraftingPlan_Smith_Drop-19549
        //    {-1690232950, new GilesGameBalanceDataCache(46, ItemType.CraftingPlan, false, FollowerType.None)},
        //    //CraftingPlan_Smith_Drop-50444
        //    {1717766203, new GilesGameBalanceDataCache(46, ItemType.CraftingPlan, false, FollowerType.None)},
        //    //CraftingPlan_Smith_Drop-12732
        //    {844895417, new GilesGameBalanceDataCache(46, ItemType.CraftingPlan, false, FollowerType.None)},
        //    //CraftingPlan_Smith_Drop-1926
        //    {1288600122, new GilesGameBalanceDataCache(46, ItemType.CraftingPlan, false, FollowerType.None)},
        //    //CraftingPlan_Smith_Drop-4913
        //    {-1661852816, new GilesGameBalanceDataCache(46, ItemType.CraftingPlan, false, FollowerType.None)},
        //    //CraftingPlan_Smith_Drop-9603
        //    {972140825, new GilesGameBalanceDataCache(46, ItemType.CraftingPlan, false, FollowerType.None)},
        //    //CraftingPlan_Smith_Drop-21742
        //    {2129978459, new GilesGameBalanceDataCache(46, ItemType.CraftingPlan, false, FollowerType.None)},
        //    //CraftingPlan_Smith_Drop-30273
        //    {844895416, new GilesGameBalanceDataCache(46, ItemType.CraftingPlan, false, FollowerType.None)},
        //    //CraftingPlan_Smith_Drop-68468
        //    {-1690232948, new GilesGameBalanceDataCache(46, ItemType.CraftingPlan, false, FollowerType.None)},
        //    //CraftingPlan_Smith_Drop-31138
        //    {255305005, new GilesGameBalanceDataCache(46, ItemType.CraftingPlan, false, FollowerType.None)},
        //    //CraftingPlan_Smith_Drop-6242
        //    {364927530, new GilesGameBalanceDataCache(46, ItemType.CraftingPlan, false, FollowerType.None)},
        //    //CraftingPlan_Smith_Drop-7631
        //    {623242821, new GilesGameBalanceDataCache(46, ItemType.CraftingPlan, false, FollowerType.None)},
        //    //CraftingPlan_Smith_Drop-11172
        //    {-1690232949, new GilesGameBalanceDataCache(46, ItemType.CraftingPlan, false, FollowerType.None)},
        //    //CraftingPlan_Smith_Drop-64675
        //    {1110084692, new GilesGameBalanceDataCache(46, ItemType.CraftingPlan, false, FollowerType.None)},
        //    //CraftingPlan_Smith_Drop-9210
        //    {-638006108, new GilesGameBalanceDataCache(46, ItemType.CraftingPlan, false, FollowerType.None)},
        //    //CraftingPlan_Smith_Drop-59824
        //    {368302888, new GilesGameBalanceDataCache(46, ItemType.CraftingPlan, false, FollowerType.None)},
        //    //CraftingPlan_Smith_Drop-11092
        //    {82340210, new GilesGameBalanceDataCache(46, ItemType.CraftingPlan, false, FollowerType.None)},
        //    //CraftingPlan_Smith_Drop-29649
        //    {521743063, new GilesGameBalanceDataCache(61, ItemType.CraftingPlan, false, FollowerType.None)},
        //    //CraftingPlan_Jeweler_Drop-7137
        //    {-1171649812, new GilesGameBalanceDataCache(61, ItemType.CraftingPlan, false, FollowerType.None)},
        //    //CraftingPlan_Jeweler_Drop-60398
        //    {872611723, new GilesGameBalanceDataCache(61, ItemType.CraftingPlan, false, FollowerType.None)},
        //    //CraftingPlan_Jeweler_Drop-3119
        //    {2147165121, new GilesGameBalanceDataCache(62, ItemType.CraftingPlan, false, FollowerType.None)},
        //    //CraftingPlan_Jeweler_Drop-51842
        //    {626121463, new GilesGameBalanceDataCache(63, ItemType.HandCrossbow, true, FollowerType.None)},
        //    //handXbow_norm_unique_flippy_08-28023
        //    {1738057815, new GilesGameBalanceDataCache(63, ItemType.Spear, true, FollowerType.None)},
        //    //Spear_norm_unique_flippy_02-46594
        //    {-1864479819, new GilesGameBalanceDataCache(62, ItemType.FistWeapon, true, FollowerType.None)},
        //    //fistWeapon_norm_unique_flippy_04-52011
        //    {1880318728, new GilesGameBalanceDataCache(62, ItemType.Mace, true, FollowerType.None)},
        //    //Mace_norm_unique_flippy_07-42689
        //    {1845044989, new GilesGameBalanceDataCache(63, ItemType.Daibo, false, FollowerType.None)},
        //    //combatStaff_norm_unique_flippy_08-45075
        //    {1025903124, new GilesGameBalanceDataCache(61, ItemType.Shield, false, FollowerType.None)},
        //    //Shield_norm_unique_flippy_08-10113
        //    {140743477, new GilesGameBalanceDataCache(60, ItemType.Axe, true, FollowerType.None)},
        //    //Axe_norm_unique_flippy_06-8301
        //    {589357912, new GilesGameBalanceDataCache(60, ItemType.HandCrossbow, true, FollowerType.None)},
        //    //handXbow_norm_unique_flippy_02-9863
        //    {-1665099598, new GilesGameBalanceDataCache(57, ItemType.Mojo, false, FollowerType.None)},
        //    //Mojo_norm_unique_flippy_04-9022
        //    {-1269640592, new GilesGameBalanceDataCache(60, ItemType.Staff, false, FollowerType.None)},
        //    //Staff_norm_unique_flippy_07-4037
        //    {-1178676596, new GilesGameBalanceDataCache(60, ItemType.Polearm, false, FollowerType.None)},
        //    //Polearm_norm_unique_flippy_02-24734
        //    {421779618, new GilesGameBalanceDataCache(63, ItemType.Sword, false, FollowerType.None)},
        //    //twoHandedSword_norm_unique_flippy_04-45679
        //    {-1451977669, new GilesGameBalanceDataCache(61, ItemType.Crossbow, false, FollowerType.None)},
        //    //XBow_norm_unique_flippy_02-36957
        //    {1034204571, new GilesGameBalanceDataCache(63, ItemType.Shield, false, FollowerType.None)},
        //    //Shield_norm_unique_flippy_06-26363
        //    {1033018650, new GilesGameBalanceDataCache(56, ItemType.Shield, false, FollowerType.None)},
        //    //Shield_norm_unique_flippy_07-69093
        //    {1949306026, new GilesGameBalanceDataCache(62, ItemType.Mace, false, FollowerType.None)},
        //    //twoHandedMace_norm_unique_flippy_04-35520
        //    {-1241813500, new GilesGameBalanceDataCache(61, ItemType.CeremonialDagger, true, FollowerType.None)},
        //    //ceremonialDagger_norm_unique_flippy_09-12122
        //    {-1264896908, new GilesGameBalanceDataCache(61, ItemType.Staff, false, FollowerType.None)},
        //    //Staff_norm_unique_flippy_05-15262
        //    {-1450791748, new GilesGameBalanceDataCache(63, ItemType.Crossbow, false, FollowerType.None)},
        //    //XBow_norm_unique_flippy_06-41422
        //    {-2078257915, new GilesGameBalanceDataCache(56, ItemType.Dagger, true, FollowerType.None)},
        //    //Dagger_norm_unique_flippy_02-1460
        //    {-2041494364, new GilesGameBalanceDataCache(63, ItemType.Dagger, true, FollowerType.None)},
        //    //Dagger_norm_unique_flippy_05-46093
        //    {1888620175, new GilesGameBalanceDataCache(60, ItemType.Mace, true, FollowerType.None)},
        //    //Mace_norm_unique_flippy_02-15158
        //    {-1262525066, new GilesGameBalanceDataCache(63, ItemType.Staff, false, FollowerType.None)},
        //    //Staff_norm_unique_flippy_04-17468
        //    {1664512741, new GilesGameBalanceDataCache(63, ItemType.MightyWeapon, true, FollowerType.None)},
        //    //mightyWeapon_1H_norm_unique_flippy_06-26377
        //    {1279577735, new GilesGameBalanceDataCache(63, ItemType.Wand, true, FollowerType.None)},
        //    //Wand_norm_unique_flippy_01-21404
        //    {139557556, new GilesGameBalanceDataCache(63, ItemType.Axe, true, FollowerType.None)},
        //    //Axe_norm_unique_flippy_04-39065
        //};
        //#endregion

    }
}
