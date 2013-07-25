using System;
using System.Collections.Generic;
using Zeta.Common;
using Zeta.Common.Plugins;
using Zeta.Internals.Actors;

namespace Trinity
{
    /// <summary>
    /// Contains hard-coded meta-lists of ActorSNO's, Spells and other non-dynamic info
    /// </summary>
    internal class DataDictionary
    {
        /// <summary>
        /// Contains the list of Boss Level Area ID's
        /// </summary>
        public static HashSet<int> BossLevelAreaIDs { get { return bossLevelAreaIDs; } }
        private static readonly HashSet<int> bossLevelAreaIDs = new HashSet<int> { 
            109457, 185228, 60194, 130163, 60714, 19789, 62726, 90881, 195268, 58494, 81178, 60757, 111232, 112580, 
            119656, 111516, 143648, 215396, 119882, 109563, 153669, 215235, 55313, 60193, 
        };

        /// <summary>
        /// This list is used when an actor has an attribute BuffVisualEffect=1, e.g. fire floors in The Butcher arena
        /// </summary>
        public static HashSet<int> AvoidanceBuffs { get { return avoidanceBuffs; } }
        private static HashSet<int> avoidanceBuffs = new HashSet<int>
        {
            // Butcher Floor Panels
            201454, 201464, 201426, 201438, 200969, 201423, 201242,
        };

        /// <summary>
        /// A list of all the SNO's to avoid - you could add
        /// </summary>
        public static HashSet<int> Avoidances { get { return avoidances; } }
        private static HashSet<int> avoidances = new HashSet<int>
        {
            // Arcane        Arcane 2      Desecrator   Poison Tree    Molten Core   Molten Core 2   Molten Trail   Plague Cloud   Ice Balls
            219702,          221225,       84608,       5482,6578,     4803, 4804,   224225, 247987, 95868,         108869,        402, 223675,
            // Bees-Wasps    Plague-Hands  Azmo Pools   Azmo fireball  Azmo bodies   Belial 1       Belial 2
            5212,            3865,         123124,      123842,        123839,       161822,        161833,
            // Sha-Ball      Mol Ball      Mage Fire    Diablo Prison  Diablo Meteor Ice-trail
            4103,            160154,       432,         168031,        214845,       260377,
            // Zolt Bubble	 Zolt Twister  Ghom Gas 	Maghda Proj
            185924,			 139741,	   93837,		166686,
            // Diablo Ring of Fire
            226350, 226525,
        };

        /// <summary>
        /// A list of SNO's that are projectiles (so constantly look for new locations while avoiding)
        /// </summary>
        public static HashSet<int> AvoidanceProjectiles { get { return avoidanceProjectiles; } }
        private static HashSet<int> avoidanceProjectiles = new HashSet<int>
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
        public static HashSet<int> WhirlwindIgnoreSNOIds { get { return whirlwindIgnoreIds; } }
        internal static HashSet<int> whirlwindIgnoreIds = new HashSet<int> {
            4304, 5984, 5985, 5987, 5988,
         };

        /// <summary>
        /// ActorSNO's of Very fast moving mobs (eg wasps), for special skill-selection decisions
        /// </summary>
        public static HashSet<int> FastMovingMonsterIds { get { return fastMovementMonsterIds; } }
        private static HashSet<int> fastMovementMonsterIds = new HashSet<int> {
            5212
         };

        /// <summary>
        /// A list of crappy "summoned mobs" we should always ignore unless they are very close to us, eg "grunts", summoned skeletons etc.
        /// </summary>
        public static HashSet<int> ShortRangeAttackMonsterIds { get { return shortRangeAttackMonsterIds; } }
        private static HashSet<int> shortRangeAttackMonsterIds = new HashSet<int> {
            4084, 4085, 5395, 144315,
         };

        /// <summary>
        /// Dictionary of Actor SNO's and cooresponding weighting/Priority 
        /// </summary>
        public static Dictionary<int, int> MonsterCustomWeights { get { return monsterCustomWeights; } }
        private static Dictionary<int, int> monsterCustomWeights = new Dictionary<int, int> {
            // Wood wraiths all this line (495 & 496 & 6572 & 139454 & 139456 & 170324 & 170325)
            {495, 901}, {496, 901}, {6572, 901}, {139454, 901}, {139456, 901}, {170324, 901}, {170325, 901},
            // -- added 4099 (act 2 fallen shaman)
            // Fallen Shaman prophets goblin Summoners (365 & 4100)
            {365, 1901}, {4099, 1901}, {4100, 1901},
            // The annoying grunts summoned by the above
            {4084, -250},
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
        public static HashSet<int> GoblinIds { get { return goblinIds; } }
        private static HashSet<int> goblinIds = new HashSet<int> {
            5984, 5985, 5987, 5988
         };

        /// <summary>
        /// Contains ActorSNO of ranged units that should be attacked even if outside of kill radius
        /// </summary>
        public static HashSet<int> RangedMonsterIds { get { return rangedMonsterIds; } }
        private static HashSet<int> rangedMonsterIds = new HashSet<int> {
            365, 4100, // fallen
            4304, 4300, // goat shaman 
            4738, // pestilence 
            4299, // goat ranged
            62736, 130794, // demon flyer
            5508, // succubus 
            210120, // corrupt growths, act 4
            5388, 4286, 256015, 256000, 255996,
       };
        // A list of bosses in the game, just to make CERTAIN they are treated as elites
        /// <summary>
        /// Contains ActorSNO of known Bosses
        /// </summary>
        public static HashSet<int> BossIds { get { return bossIds; } }
        private static HashSet<int> bossIds = new HashSet<int>
        {
            // Siegebreaker (96192), Azmodan (89690), Cydea (95250), Heart-thing (193077), 
            96192,                   89690,           95250,         193077, 
            //Kulle (80509), Small Belial (220160), Big Belial (3349), Diablo 1 (114917), terror Diablo (133562)
            80509,           220160,                3349,              114917,            133562, 
            //Maghda, Kamyr (MiniBoss before Belial)
            6031, 51298,
            // Ghom
            87642,
            // I dunno?
            255929, 256711, 256508, 256187, 256189, 256709,
            // Another Cydaea
            137139,
            // Diablo shadow clones (needs all of them, there is a male & female version of each class!)
            144001, 144003, 143996, 143994, 
            // Jondar, Chancellor, Queen Araneae (act 1 dungeons), Skeleton King, Butcher
            86624, 156353, 51341, 5350, 3526,
            215103, // Istaku            
            210120, // A4 Corrupt Growths                 
            4630, // Rakanoth
            256015, //Xah'Rith Keywarden
            115403, // A1 Cain Skeleton boss
         };


        // Three special lists used purely for checking for the existance of a player's summoned mystic ally, gargantuan, or zombie dog

        public static HashSet<int> MysticAllyIds { get { return mysticAllyIds; } }
        private static HashSet<int> mysticAllyIds = new HashSet<int> { 
            169123, 123885, 169890, 168878, 169891, 169077, 169904, 169907, 169906, 169908, 169905, 169909 
        };

        public static HashSet<int> GargantuanIds { get { return gargantuanIds; } }
        private static HashSet<int> gargantuanIds = new HashSet<int> { 
            179780, 179778, 179772, 179779, 179776, 122305 };

        public static HashSet<int> ZombieDogIds { get { return zombieDogIds; } }
        private static HashSet<int> zombieDogIds = new HashSet<int> { 
            110959, 103235, 103215, 105763, 103217, 51353 
        };

        public static HashSet<int> DemonHunterPetIds { get { return demonHunterPetIds; } }
        private static HashSet<int> demonHunterPetIds = new HashSet<int> { 
            178664, 173827, 133741, 159144, 181748, 159098 
        };

        /// <summary>
        /// World-object dictionaries eg large object lists, ignore lists etc.
        /// A list of SNO's to *FORCE* to type: Item. (BE CAREFUL WITH THIS!).
        /// </summary>
        public static HashSet<int> ForceToItemOverrideIds { get { return forceToItemOverrideIds; } }
        private static HashSet<int> forceToItemOverrideIds = new HashSet<int> {
            166943, // infernal key
        };

        /// <summary>
        /// Interactable whitelist - things that need interacting with like special wheels, levers - they will be blacklisted for 30 seconds after one-use
        /// </summary>
        public static HashSet<int> InteractWhiteListIds { get { return interactWhiteListIds; } }
        private static HashSet<int> interactWhiteListIds = new HashSet<int> {
            56686, 211999, 52685, 54882, 180575, 105478, 
        };

        /// <summary>
        /// NOTE: you don't NEED interactable SNO's listed here. But if they are listed here, *THIS* is the range at which your character will try to walk to within the object
        /// BEFORE trying to actually "click it". Certain objects need you to get very close, so it's worth having them listed with low interact ranges
        /// </summary>
        public static Dictionary<int, int> InteractAtCustomRange { get { return interactAtCustomRange; } }
        private static Dictionary<int, int> interactAtCustomRange = new Dictionary<int, int> {
            {56686, 4}, {52685, 4}, {54882, 40}, {3349, 25}, {225270, 35}, {180575, 10}
        };

        /// <summary>
        /// Navigation obstacles for standard navigation down dungeons etc. to help DB movement
        /// MAKE SURE you add the *SAME* SNO to the "size" dictionary below, and include a reasonable size (keep it smaller rather than larger) for the SNO.
        /// </summary>
        public static HashSet<int> NavigationObstacleIds { get { return navigationObstacleIds; } }
        private static HashSet<int> navigationObstacleIds = new HashSet<int> {
            174900, 185391, // demonic forge
            104632, 194682, 81699, 3340, 123325, 
        };

        /// <summary>
        /// Size of the navigation obstacles above (actual SNO list must be matching the above list!)
        /// </summary>
        public static Dictionary<int, int> ObstacleCustomRadius { get { return obstacleCustomRadius; } }
        private static Dictionary<int, int> obstacleCustomRadius = new Dictionary<int, int> {
            {174900, 25}, {104632, 20}, {194682, 20}, {81699, 20}, {3340, 12}, {123325, 25}, {185391, 25},
         };

        /// <summary>
        /// This is the RadiusDistance at which destructibles and barricades (logstacks, large crates, carts, etc.) are added to the cache
        /// </summary>
        public static Dictionary<int, float> DestructableObjectRadius { get { return destructableObjectRadius; } }
        private static Dictionary<int, float> destructableObjectRadius = new Dictionary<int, float> {
            {2972, 10}, {80357, 16}, {116508, 10}, {113932, 8}, {197514, 18}, {108587, 8}, {108618, 8}, {108612, 8}, {116409, 18}, {121586, 22},
            {195101, 10}, {195108, 25}, {170657, 5}, {181228, 10}, {211959, 25}, {210418, 25}, {174496, 4}, {193963, 5}, {159066, 12}, {160570, 12},
            {55325, 5}, {5718, 14}, {5909, 10}, {5792, 8}, {108194, 8}, {129031, 30}, {192867, 3.5f}, {155255, 8}, {54530, 6}, {157541, 6},
         };

        /// <summary>
        /// Destructible things that need targeting by a location instead of an ACDGUID (stuff you can't "click on" to destroy in-game)
        /// </summary>
        public static HashSet<int> DestroyAtLocationIds { get { return destroyAtLocationIds; } }
        private static HashSet<int> destroyAtLocationIds = new HashSet<int> {
            170657, 116409, 121586, 155255, 104596,
         };

        /// <summary>
        /// Resplendent chest SNO list
        /// </summary>
        public static HashSet<int> ResplendentChestIds { get { return resplendentChestIds; } }
        private static HashSet<int> resplendentChestIds = new HashSet<int> {
            62873, 95011, 81424, 108230, 111808, 111809, 211861, 62866, 109264, 62866, 62860, 96993,
            // Magi
			112182,
         };
        /// <summary>
        /// Objects that should never be ignored due to no Line of Sight (LoS) or ZDiff
        /// </summary>
        public static HashSet<int> LineOfSightWhitelist { get { return lineOfSightWhitelist; } }
        private static HashSet<int> lineOfSightWhitelist = new HashSet<int>
        {
            116807, // Butcher Health Well
            180575, // Diablo arena Health Well
            129031, // A3 Skycrown Catapults
            220160, // Small Belial (220160), 
            3349,   // Big Belial (3349),    
            210268, // Corrupt Growths 2nd Tier
            193077, // a3dun_Crater_ST_GiantDemonHeart_Mob
        };

        /// <summary>
        /// Chests/average-level containers that deserve a bit of extra radius (ie - they are more worthwhile to loot than "random" misc/junk containers)
        /// </summary>
        public static HashSet<int> ContainerWhiteListIds { get { return containerWhiteListIds; } }
        private static HashSet<int> containerWhiteListIds = new HashSet<int> {
            62859, 62865, 62872, 78790, 79016, 94708, 96522, 130170, 108122, 111870, 111947, 213447, 213446, 51300, 179865, 109264, 212491, 210422, 211861,
            // Magi
			196945, 70534,
         };

        /// <summary>
        /// Contains ActorSNO's of world objects that should be blacklisted
        /// </summary>
        public static HashSet<int> BlackListIds { get { return blacklistIds; } }
        private static HashSet<int> blacklistIds = new HashSet<int> {
            
            // World Objects
            163449, 78030, 2909, 58283, 58321, 87809, 90150, 91600, 97023, 97350, 97381, 72689, 121327, 54515, 3340, 122076, 123640,
            60665, 60844, 78554, 86400, 86428, 81699, 86266, 86400, 192466, 6190, 80002, 104596, 58836, 104827, 74909, 6155, 6156, 6158, 6159, 75132,
            181504, 91688, 3007, 3011, 3014, 130858, 131573, 214396, 182730, 226087, 141639, 206569, 15119, 54413, 54926, 2979, 5776, 3949,
            108490, 52833, 200371, 153752, 2972, 206527, 3628,
            //a3dun_crater_st_Demo_ChainPylon_Fire_Azmodan, a3dun_crater_st_Demon_ChainPylon_Fire_MistressOfPain
            198977, 201680,
            //trOut_Leor_painting
            217285,
            // uber fire chains in Realm of Turmoil  
            263014,

            // Units below here
            111456, 5013, 5014, 205756, 205746, 4182, 4183, 4644, 4062, 4538, 52693, 162575, 2928, 51291, 51292,
            96132, 90958, 90959, 80980, 51292, 51291, 2928, 3546, 129345, 81857, 138428, 81857, 60583, 170038, 174854, 190390,
            194263, 87189, 90072, 107031, 106584, 186130, 187265,
            108012, 103279, 74004, 84531, 84538,  190492, 209133, 6318, 107705, 105681, 89934,
            89933, 182276, 117574, 182271, 182283, 182278, 128895, 81980, 82111, 81226, 81227, 107067, 106749,
            107107, 107112, 106731, 107752, 107829, 90321, 107828, 121327, 249320, 81232, 81231, 81239, 81515, 210433, 195414,
            80758, 80757, 80745, 81229, 81230, 82109, 83024, 83025, 82972, 83959, 249190, 251396, 138472, 118260, 200226, 192654, 245828,
            215103, 132951, 217508, 199998, 199997, 114527, 245910, 169123, 123885, 169890, 168878, 169891, 169077, 169904, 169907,
            169906, 169908, 169905, 169909, 179780, 179778, 179772, 179779, 179776, 122305, 80140, 110959, 103235, 103215, 105763, 103217, 51353,
            4176, 178664, 173827, 133741, 159144, 181748, 159098, 206569, 200706, 5895, 5896, 5897, 5899, 4686, 87037, 85843, 103919, 249338,
            251416, 249192, 80812, 4798, 183892,196899, 196900, 196903, 223333, 220636, 218951, 245838,
            //bone pile
            218951,245838,
            // rrrix act 1
            108882, 245919, 5944, 165475, 199998, 168875, 105323, 85690, 105321, 108266,
            // rrrix act 2
            213907, 92519, 61544, 105681, 113983, 114527, 114642, 139933, 144405, 156890, 164057, 164195, 180254, 180802, 180809, 181173, 181174, 181177, 181181,
            181182, 181185, 181290, 181292, 181306, 181309, 181313, 181326, 181563, 181857, 181858, 187265, 191433, 191462, 191641, 192880, 192881, 196413, 196435,
            197280, 199191, 199264, 199274, 199597, 199664, 200979, 200982, 201236, 201580, 201581, 201583, 204183, 205746, 205756, 210087, 213907, 218228, 219223,
            220114, 3011, 3205, 3539, 3582, 3584, 3595, 3600, 4580, 52693, 5466, 55005, 5509, 62522, 
            205756, 5509, 200371, 167185, 181195, 217346, 178161,
            // rrrix act 3
            60108, 182443,
            // uber fire chains in Realm of Turmoil and Iron Gate in Realm of Chaos
            263014, 
        };

        /// <summary>
        /// Add an ActorSNO to the blacklist. Returns false if the blacklist already contains the ActorSNO
        /// </summary>
        /// <param name="actorId"></param>
        /// <returns></returns>
        public static bool AddToBlacklist(int actorId)
        {
            if (!blacklistIds.Contains(actorId))
            {
                blacklistIds.Add(actorId);
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Timers for abilities and selection of best ability etc.
        /// </summary>
        //public static Dictionary<SNOPower, int> AbilityRepeatDelays
        //{
        //    get { return abilityRepeatDelays; }
        //    internal set { abilityRepeatDelays = value; }
        //}
        //private static Dictionary<SNOPower, int> abilityRepeatDelays = new Dictionary<SNOPower, int>();

        /// <summary>
        /// Default Values for AbilityRepeatDelays
        /// </summary>
        //public static Dictionary<SNOPower, int> AbilityRepeatDelayDefaults { get { return abilityRepeatDelayDefaults; } }
        //private static Dictionary<SNOPower, int> abilityRepeatDelayDefaults = new Dictionary<SNOPower, int>
        //    {
        //        {SNOPower.DrinkHealthPotion, 30000},
        //        {SNOPower.Weapon_Melee_Instant, 5},
        //        {SNOPower.Weapon_Ranged_Instant, 5},
        //        {SNOPower.Barbarian_Bash, 5},
        //        {SNOPower.Barbarian_Cleave, 5},
        //        {SNOPower.Barbarian_Frenzy, 5},
        //        {SNOPower.Barbarian_HammerOfTheAncients, 150},
        //        {SNOPower.Barbarian_Rend, 2650},
        //        {SNOPower.Barbarian_SeismicSlam, 200},
        //        {SNOPower.Barbarian_Whirlwind, 5},
        //        {SNOPower.Barbarian_GroundStomp, 12200},
        //        {SNOPower.Barbarian_Leap, 10200},
        //        {SNOPower.Barbarian_Sprint, 2900},
        //        {SNOPower.Barbarian_IgnorePain, 30200},
        //        {SNOPower.Barbarian_AncientSpear, 300},
        //        // Has a rune that resets cooldown from 10 seconds to 0 on crit
        //        {SNOPower.Barbarian_Revenge, 600},
        //        {SNOPower.Barbarian_FuriousCharge, 500},
        //        // Need to be able to check skill-rune for the dynamic cooldown - set to 10 always except for the skill rune :(
        //        {SNOPower.Barbarian_Overpower, 200},
        //        {SNOPower.Barbarian_WeaponThrow, 5},
        //        {SNOPower.Barbarian_ThreateningShout, 10200},
        //        {SNOPower.Barbarian_BattleRage, 118000},
        //        {SNOPower.Barbarian_WarCry, 20500},
        //        {SNOPower.Barbarian_Earthquake, 120500},
        //        // Need to be able to check skill-run for dynamic cooldown, and passive for extra cooldown
        //        {SNOPower.Barbarian_CallOfTheAncients, 120500},
        //        // Need to be able to check passive for cooldown
        //        {SNOPower.Barbarian_WrathOfTheBerserker, 120500},
        //        // Need to be able to check passive for cooldown
        //        // Monk skills
        //        {SNOPower.Monk_FistsofThunder, 5},
        //        {SNOPower.Monk_DeadlyReach, 5},
        //        {SNOPower.Monk_CripplingWave, 5},
        //        {SNOPower.Monk_WayOfTheHundredFists, 5},
        //        {SNOPower.Monk_LashingTailKick, 250},
        //        {SNOPower.Monk_TempestRush, 15},
        //        {SNOPower.Monk_WaveOfLight, 750},
        //        {SNOPower.Monk_BlindingFlash, 15200},
        //        {SNOPower.Monk_BreathOfHeaven, 15200},
        //        {SNOPower.Monk_Serenity, 20200},
        //        {SNOPower.Monk_InnerSanctuary, 20200},
        //        {SNOPower.Monk_DashingStrike, 1000},
        //        {SNOPower.Monk_ExplodingPalm, 5000},
        //        {SNOPower.Monk_SweepingWind, 1500},
        //        {SNOPower.Monk_CycloneStrike, 900},
        //        {SNOPower.Monk_SevenSidedStrike, 30200},
        //        {SNOPower.Monk_MysticAlly, 30000},
        //        {SNOPower.Monk_MantraOfEvasion, 3300},
        //        {SNOPower.Monk_MantraOfRetribution, 3300},
        //        {SNOPower.Monk_MantraOfHealing, 3300},
        //        {SNOPower.Monk_MantraOfConviction, 3300},
        //        // Wizard skills
        //        {SNOPower.Wizard_MagicMissile, 5},
        //        {SNOPower.Wizard_ShockPulse, 5},
        //        {SNOPower.Wizard_SpectralBlade, 5},
        //        {SNOPower.Wizard_Electrocute, 5},
        //        {SNOPower.Wizard_RayOfFrost, 5},
        //        {SNOPower.Wizard_ArcaneOrb, 500},
        //        {SNOPower.Wizard_ArcaneTorrent, 5},
        //        {SNOPower.Wizard_Disintegrate, 5},
        //        {SNOPower.Wizard_FrostNova, 9000},
        //        {SNOPower.Wizard_DiamondSkin, 15000},
        //        {SNOPower.Wizard_SlowTime, 16000},
        //        {SNOPower.Wizard_Teleport, 16000},
        //        // Need to be able to check rune that let's us spam this 3 times in a row then cooldown
        //        {SNOPower.Wizard_WaveOfForce, 12000},
        //        // normally 15/16 seconds, but a certain rune can allow 12 seconds :(
        //        {SNOPower.Wizard_EnergyTwister, 5},
        //        {SNOPower.Wizard_Hydra, 12000},
        //        {SNOPower.Wizard_Meteor, 1000},
        //        {SNOPower.Wizard_Blizzard, 4000},
        //        // Effect lasts for 6 seconds, actual cooldown is 0...
        //        {SNOPower.Wizard_IceArmor, 60000},
        //        {SNOPower.Wizard_StormArmor, 60000},
        //        {SNOPower.Wizard_EnergyArmor, 60000},
        //        {SNOPower.Wizard_MagicWeapon, 60000},
        //        {SNOPower.Wizard_Familiar, 60000},
        //        {SNOPower.Wizard_ExplosiveBlast, 6000},
        //        {SNOPower.Wizard_MirrorImage, 5000},
        //        {SNOPower.Wizard_Archon, 100000},
        //        // Actually 120 seconds, but 100 seconds with a rune
        //        {SNOPower.Wizard_Archon_ArcaneBlast, 5000},
        //        {SNOPower.Wizard_Archon_ArcaneStrike, 200},
        //        {SNOPower.Wizard_Archon_DisintegrationWave, 5},
        //        {SNOPower.Wizard_Archon_SlowTime, 16000},
        //        {SNOPower.Wizard_Archon_Teleport, 10000},
        //        {SNOPower.Wizard_Archon_Cancel, 1500},
        //        // Witch Doctor skills
        //        {SNOPower.Witchdoctor_PoisonDart, 5},
        //        {SNOPower.Witchdoctor_CorpseSpider, 5},
        //        {SNOPower.Witchdoctor_PlagueOfToads, 5},
        //        {SNOPower.Witchdoctor_Firebomb, 5},
        //        {SNOPower.Witchdoctor_GraspOfTheDead, 6000},
        //        {SNOPower.Witchdoctor_Firebats, 5},
        //        {SNOPower.Witchdoctor_Haunt, 12000},
        //        {SNOPower.Witchdoctor_Locust_Swarm, 8000},
        //        {SNOPower.Witchdoctor_SummonZombieDog, 25000},
        //        {SNOPower.Witchdoctor_Horrify, 16200},
        //        {SNOPower.Witchdoctor_SpiritWalk, 15200},
        //        {SNOPower.Witchdoctor_Hex, 15200},
        //        {SNOPower.Witchdoctor_SoulHarvest, 15000},
        //        {SNOPower.Witchdoctor_Sacrifice, 1000},
        //        {SNOPower.Witchdoctor_MassConfusion, 45200},
        //        {SNOPower.Witchdoctor_ZombieCharger, 5},
        //        {SNOPower.Witchdoctor_SpiritBarrage, 15000},
        //        {SNOPower.Witchdoctor_AcidCloud, 5},
        //        {SNOPower.Witchdoctor_WallOfZombies, 25200},
        //        {SNOPower.Witchdoctor_Gargantuan, 25000},
        //        {SNOPower.Witchdoctor_BigBadVoodoo, 120000},
        //        {SNOPower.Witchdoctor_FetishArmy, 90000},
        //        // Demon Hunter skills
        //        {SNOPower.DemonHunter_HungeringArrow, 5},
        //        {SNOPower.DemonHunter_EntanglingShot, 5},
        //        {SNOPower.DemonHunter_BolaShot, 5},
        //        {SNOPower.DemonHunter_Grenades, 5},
        //        {SNOPower.DemonHunter_Impale, 5},
        //        {SNOPower.DemonHunter_RapidFire, 5},
        //        {SNOPower.DemonHunter_Chakram, 5},
        //        {SNOPower.DemonHunter_ElementalArrow, 5},
        //        {SNOPower.DemonHunter_Caltrops, 3000},
        //        {SNOPower.DemonHunter_SmokeScreen, 3000},
        //        {SNOPower.DemonHunter_ShadowPower, 5000},
        //        {SNOPower.DemonHunter_Vault, 400},
        //        {SNOPower.DemonHunter_Preparation, 5000},
        //        {SNOPower.DemonHunter_Companion, 30000},
        //        {SNOPower.DemonHunter_MarkedForDeath, 3000},
        //        {SNOPower.DemonHunter_EvasiveFire, 300},
        //        {SNOPower.DemonHunter_FanOfKnives, 10000},
        //        {SNOPower.DemonHunter_SpikeTrap, 1000},
        //        {SNOPower.DemonHunter_Sentry, 8000},
        //        {SNOPower.DemonHunter_Strafe, 5},
        //        {SNOPower.DemonHunter_Multishot, 5},
        //        {SNOPower.DemonHunter_ClusterArrow, 150},
        //        {SNOPower.DemonHunter_RainOfVengeance, 10000},
        //    };

        /// <summary>
        /// Last used-timers for all abilities to prevent spamming D3 memory for cancast checks too often
        /// These should NEVER need manually editing
        /// But you do need to make sure every skill used by Trinity is listed in here once!
        /// </summary>
        public static Dictionary<SNOPower, DateTime> LastUseAbilityTimeDefaults
        {
            get { return lastUseAbilityTimeDefaults; }
            internal set { lastUseAbilityTimeDefaults = value; }
        }
        private static Dictionary<SNOPower, DateTime> lastUseAbilityTimeDefaults = new Dictionary<SNOPower, DateTime>
            {
                {SNOPower.DrinkHealthPotion, DateTime.MinValue},
                {SNOPower.Weapon_Melee_Instant, DateTime.MinValue},
                {SNOPower.Weapon_Ranged_Instant, DateTime.MinValue},

            //    // Barbarian last-used timers
            //    {SNOPower.Barbarian_Bash, DateTime.Today},{SNOPower.Barbarian_Cleave, DateTime.Today},{SNOPower.Barbarian_Frenzy, DateTime.Today},
            //    {SNOPower.Barbarian_HammerOfTheAncients, DateTime.Today},{SNOPower.Barbarian_Rend, DateTime.Today},{SNOPower.Barbarian_SeismicSlam, DateTime.Today},
            //    {SNOPower.Barbarian_Whirlwind, DateTime.Today},{SNOPower.Barbarian_GroundStomp, DateTime.Today},{SNOPower.Barbarian_Leap, DateTime.Today},
            //    {SNOPower.Barbarian_Sprint, DateTime.Today},{SNOPower.Barbarian_IgnorePain, DateTime.Today},{SNOPower.Barbarian_AncientSpear, DateTime.Today},
            //    {SNOPower.Barbarian_Revenge, DateTime.Today},{SNOPower.Barbarian_FuriousCharge, DateTime.Today},{SNOPower.Barbarian_Overpower, DateTime.Today},
            //    {SNOPower.Barbarian_WeaponThrow, DateTime.Today},{SNOPower.Barbarian_ThreateningShout, DateTime.Today},{SNOPower.Barbarian_BattleRage, DateTime.Today},
            //    {SNOPower.Barbarian_WarCry, DateTime.Today},{SNOPower.Barbarian_Earthquake, DateTime.Today},{SNOPower.Barbarian_CallOfTheAncients, DateTime.Today},
            //    {SNOPower.Barbarian_WrathOfTheBerserker, DateTime.Today },
            //    // Monk last-used timers
            //    {SNOPower.Monk_FistsofThunder, DateTime.Today},{SNOPower.Monk_DeadlyReach, DateTime.Today},{SNOPower.Monk_CripplingWave, DateTime.Today},
            //    {SNOPower.Monk_WayOfTheHundredFists, DateTime.Today},{SNOPower.Monk_LashingTailKick, DateTime.Today},{SNOPower.Monk_TempestRush, DateTime.Today},
            //    {SNOPower.Monk_WaveOfLight, DateTime.Today},{SNOPower.Monk_BlindingFlash, DateTime.Today},{SNOPower.Monk_BreathOfHeaven, DateTime.Today},
            //    {SNOPower.Monk_Serenity, DateTime.Today}, {SNOPower.Monk_InnerSanctuary, DateTime.Today},{SNOPower.Monk_DashingStrike, DateTime.Today},
            //    {SNOPower.Monk_ExplodingPalm, DateTime.Today},{SNOPower.Monk_SweepingWind, DateTime.Today},{SNOPower.Monk_CycloneStrike, DateTime.Today},
            //    {SNOPower.Monk_SevenSidedStrike, DateTime.Today},{SNOPower.Monk_MysticAlly, DateTime.Today},{SNOPower.Monk_MantraOfEvasion, DateTime.Today},
            //    {SNOPower.Monk_MantraOfRetribution, DateTime.Today},{SNOPower.Monk_MantraOfHealing, DateTime.Today}, {SNOPower.Monk_MantraOfConviction, DateTime.Today},
            //    // Wizard last-used timers
            //    {SNOPower.Wizard_MagicMissile, DateTime.Today},{SNOPower.Wizard_ShockPulse, DateTime.Today},{SNOPower.Wizard_SpectralBlade, DateTime.Today},
            //    {SNOPower.Wizard_Electrocute, DateTime.Today},{SNOPower.Wizard_RayOfFrost, DateTime.Today},{SNOPower.Wizard_ArcaneOrb, DateTime.Today},
            //    {SNOPower.Wizard_ArcaneTorrent, DateTime.Today},{SNOPower.Wizard_Disintegrate, DateTime.Today},{SNOPower.Wizard_FrostNova, DateTime.Today},
            //    {SNOPower.Wizard_DiamondSkin, DateTime.Today},{SNOPower.Wizard_SlowTime, DateTime.Today},{SNOPower.Wizard_Teleport, DateTime.Today},
            //    {SNOPower.Wizard_WaveOfForce, DateTime.Today},{SNOPower.Wizard_EnergyTwister, DateTime.Today},{SNOPower.Wizard_Hydra, DateTime.Today},
            //    {SNOPower.Wizard_Meteor, DateTime.Today},{SNOPower.Wizard_Blizzard, DateTime.Today},{SNOPower.Wizard_IceArmor, DateTime.Today},
            //    {SNOPower.Wizard_StormArmor, DateTime.Today},{SNOPower.Wizard_MagicWeapon, DateTime.Today},{SNOPower.Wizard_Familiar, DateTime.Today},
            //    {SNOPower.Wizard_EnergyArmor, DateTime.Today},{SNOPower.Wizard_ExplosiveBlast, DateTime.Today},{SNOPower.Wizard_MirrorImage, DateTime.Today},
            //    {SNOPower.Wizard_Archon, DateTime.Today},{SNOPower.Wizard_Archon_ArcaneBlast, DateTime.Today},{SNOPower.Wizard_Archon_ArcaneStrike, DateTime.Today},
            //    {SNOPower.Wizard_Archon_DisintegrationWave, DateTime.Today},{SNOPower.Wizard_Archon_SlowTime, DateTime.Today},{SNOPower.Wizard_Archon_Teleport, DateTime.Today},
            //    // Witch Doctor last-used timers
            //    {SNOPower.Witchdoctor_PoisonDart, DateTime.Today},{SNOPower.Witchdoctor_CorpseSpider, DateTime.Today},{SNOPower.Witchdoctor_PlagueOfToads, DateTime.Today},
            //    {SNOPower.Witchdoctor_Firebomb, DateTime.Today},{SNOPower.Witchdoctor_GraspOfTheDead, DateTime.Today},{SNOPower.Witchdoctor_Firebats, DateTime.Today},
            //    {SNOPower.Witchdoctor_Haunt, DateTime.Today},{SNOPower.Witchdoctor_Locust_Swarm, DateTime.Today},{SNOPower.Witchdoctor_SummonZombieDog, DateTime.Today},
            //    {SNOPower.Witchdoctor_Horrify, DateTime.Today},{SNOPower.Witchdoctor_SpiritWalk, DateTime.Today},{SNOPower.Witchdoctor_Hex, DateTime.Today},
            //    {SNOPower.Witchdoctor_SoulHarvest, DateTime.Today},{SNOPower.Witchdoctor_Sacrifice, DateTime.Today},{SNOPower.Witchdoctor_MassConfusion, DateTime.Today},
            //    {SNOPower.Witchdoctor_ZombieCharger, DateTime.Today},{SNOPower.Witchdoctor_SpiritBarrage, DateTime.Today},{SNOPower.Witchdoctor_AcidCloud, DateTime.Today},
            //    {SNOPower.Witchdoctor_WallOfZombies, DateTime.Today},{SNOPower.Witchdoctor_Gargantuan, DateTime.Today},{SNOPower.Witchdoctor_BigBadVoodoo, DateTime.Today},
            //    {SNOPower.Witchdoctor_FetishArmy, DateTime.Today},
            //    // Demon Hunter last-used timers
            //    {SNOPower.DemonHunter_HungeringArrow, DateTime.Today},{SNOPower.DemonHunter_EntanglingShot, DateTime.Today},{SNOPower.DemonHunter_BolaShot, DateTime.Today},
            //    {SNOPower.DemonHunter_Grenades, DateTime.Today},{SNOPower.DemonHunter_Impale, DateTime.Today},{SNOPower.DemonHunter_RapidFire, DateTime.Today},
            //    {SNOPower.DemonHunter_Chakram, DateTime.Today},{SNOPower.DemonHunter_ElementalArrow, DateTime.Today},{SNOPower.DemonHunter_Caltrops, DateTime.Today},
            //    {SNOPower.DemonHunter_SmokeScreen, DateTime.Today},{SNOPower.DemonHunter_ShadowPower, DateTime.Today},{SNOPower.DemonHunter_Vault, DateTime.Today},
            //    {SNOPower.DemonHunter_Preparation, DateTime.Today},{SNOPower.DemonHunter_Companion, DateTime.Today},{SNOPower.DemonHunter_MarkedForDeath, DateTime.Today},
            //    {SNOPower.DemonHunter_EvasiveFire, DateTime.Today},{SNOPower.DemonHunter_FanOfKnives, DateTime.Today},{SNOPower.DemonHunter_SpikeTrap, DateTime.Today},
            //    {SNOPower.DemonHunter_Sentry, DateTime.Today},{SNOPower.DemonHunter_Strafe, DateTime.Today},{SNOPower.DemonHunter_Multishot, DateTime.Today},
            //    {SNOPower.DemonHunter_ClusterArrow, DateTime.Today},{SNOPower.DemonHunter_RainOfVengeance, DateTime.Today},
            };

        public static HashSet<int> ForceUseWOTBIds
        {
            get { return DataDictionary.forceUseWOTBIds; }
        }
        private static HashSet<int> forceUseWOTBIds = new HashSet<int>()
        {
            256015, 256000, 255996
        };
    }
}
