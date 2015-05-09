using System;
using System.Collections.Generic;
using Trinity.Objects;
using Trinity.Reference;
using Zeta.Common;
using Zeta.Game;
using Zeta.Game.Internals.Actors;
using Zeta.Game.Internals.SNO;

namespace Trinity
{
    /// <summary>
    /// Contains hard-coded meta-lists of ActorSNO's, Spells and other non-dynamic info
    /// </summary>
    internal class DataDictionary
    {
        public const int WALLER_SNO = 226808; //monsterAffix_waller_model (226808)

        public const int PLAYER_HEADSTONE_SNO = 4860; // PlayerHeadstone

        public static HashSet<int> PandemoniumFortressWorlds { get { return _pandemoniumFortressWorlds; } }
        private static readonly HashSet<int> _pandemoniumFortressWorlds = new HashSet<int>
        {
            271233, // Adventure Pand Fortress 1
            271235, // Adventure Pand Fortress 2
        };

        public static HashSet<int> PandemoniumFortressLevelAreaIds { get { return _pandemoniumFortressLevelAreaIds; } }
        private static readonly HashSet<int> _pandemoniumFortressLevelAreaIds = new HashSet<int>
        {
            333758, //LevelArea: X1_LR_Tileset_Fortress
        };

        public static HashSet<int> NoCheckKillRange { get { return _noCheckKillRange; } }
        private static readonly HashSet<int> _noCheckKillRange = new HashSet<int>
        {
            210120, // A4 Corrupt Growth
            210268, // A4 Corrupt Growth
        };

        public const int RiftTrialLevelAreaId = 405915;

        /// <summary>
        /// Contains a list of Rift WorldId's
        /// </summary>
        public static List<int> RiftWorldIds { get { return DataDictionary.riftWorldIds; } }
        private static readonly List<int> riftWorldIds = new List<int>()
        {
			288454,
			288685,
			288687,
			288798,
			288800,
			288802,
			288804,
			288806,
        };

        /// <summary>
        /// Contains all the Exit Name Hashes in Rifts
        /// </summary>
        public static List<int> RiftPortalHashes { get { return DataDictionary.riftPortalHashes; } }
        private static readonly List<int> riftPortalHashes = new List<int>()
		{
			1938876094,
			1938876095,
			1938876096,
			1938876097,
			1938876098,
			1938876099,
			1938876100,
			1938876101,
			1938876102,
		};

        public static HashSet<int> BountyTurnInQuests { get { return DataDictionary.bountyTurnInQuests; } }
        private static readonly HashSet<int> bountyTurnInQuests = new HashSet<int>()
        {
            356988, //x1_AdventureMode_BountyTurnin_A1 
            356994, //x1_AdventureMode_BountyTurnin_A2 
            356996, //x1_AdventureMode_BountyTurnin_A3 
            356999, //x1_AdventureMode_BountyTurnin_A4 
            357001, //x1_AdventureMode_BountyTurnin_A5 
        };

        public static HashSet<int> EventQuests { get { return DataDictionary.eventQuests; } }
        private static readonly HashSet<int> eventQuests = new HashSet<int>()
        {
            365821, // [D7499CC] Quest: x1_Catacombs_NS_06Mutant_Evant, QuestSNO: 365821, QuestMeter: -1, QuestState: InProgress, QuestStep: 10, KillCount: 0, BonusCount: 0 
            369381, // [2ECD96F4] Quest: x1_Event_Horde_HunterKillers, QuestSNO: 369381, QuestMeter: 0.004814815, QuestState: InProgress, QuestStep: 14, KillCount: 0, BonusCount: 0
            369431, // [2ECD9860] Quest: x1_Event_WaveFight_AncientEvils, QuestSNO: 369431, QuestMeter: -1, QuestState: InProgress, QuestStep: 13, KillCount: 0, BonusCount: 0
            336293, // [417DD860] Quest: X1_Graveyard_GraveRobber_Event, QuestSNO: 336293, QuestMeter: -1, QuestState: InProgress, QuestStep: 46, KillCount: 0, BonusCount: 0
            369414, // [33955B38] Quest: X1_Pand_Ext_ForgottenWar_Adventure, QuestSNO: 369414, QuestMeter: -1, QuestState: InProgress, QuestStep: 2, KillCount: 1, BonusCount: 0

            368306, // x1_Event_Horde_ArmyOfHell,
            369332, // x1_Event_Horde_Bonepit,
            365252, // x1_Event_Horde_DeathCellar,
            365150, // x1_Event_Horde_Deathfire,
            365695, // x1_Event_Horde_DesertFortress,
            367979, // x1_Event_Horde_Dustbowl,
            364880, // x1_Event_Horde_FleshpitGrove,
            369525, // x1_Event_Horde_FlyingAssasins,
            365796, // x1_Event_Horde_FoulHatchery,
            365305, // x1_Event_Horde_GhoulSwarm,
            369366, // x1_Event_Horde_GuardSlaughter,
            369381, // x1_Event_Horde_HunterKillers,
            368035, // x1_Event_Horde_InfernalSky,
            // 365269, // x1_Event_Horde_SpiderTrap,
            366331, // x1_Event_Horde_UdderChaos,
            239301, // x1_Event_Jar_Of_Souls_NecroVersion,
            370334, // x1_Event_SpeedKill_Angel_Corrupt_A,
            370316, // x1_Event_SpeedKill_BileCrawler_A,
            369841, // x1_Event_SpeedKill_Bloodhawk_A,
            370556, // x1_Event_SpeedKill_Boss_Adria,
            370373, // x1_Event_SpeedKill_Boss_Despair,
            370154, // x1_Event_SpeedKill_Boss_Ghom,
            369892, // x1_Event_SpeedKill_Boss_Maghda,
            365630, // x1_Event_SpeedKill_Boss_SkeletonKing,
            370349, // x1_Event_SpeedKill_Champion_BigRed_A,
            370082, // x1_Event_SpeedKill_Champion_FallenHound_D,
            369895, // x1_Event_SpeedKill_Champion_FleshPitFlyer_C,
            365586, // x1_Event_SpeedKill_Champion_GhostA,
            365593, // x1_Event_SpeedKill_Champion_GoatmanB,
            370364, // x1_Event_SpeedKill_Champion_MalletDemon_A,
            369906, // x1_Event_SpeedKill_Champion_SandShark_A,
            370135, // x1_Event_SpeedKill_Champion_SoulRipper_A,
            370837, // x1_Event_SpeedKill_Champion_SquiggletA,
            365617, // x1_Event_SpeedKill_Champion_SummonableA,
            370077, // x1_Event_SpeedKill_Champion_azmodanBodyguard_A,
            370066, // x1_Event_SpeedKill_Champion_creepMob_A,
            370341, // x1_Event_SpeedKill_Champion_morluSpellcaster_A,
            370516, // x1_Event_SpeedKill_Champion_x1_FloaterAngel_A,
            370544, // x1_Event_SpeedKill_Champon_x1_Rockworm_Pand_A,
            370320, // x1_Event_SpeedKill_CoreEliteDemon_A,
            370038, // x1_Event_SpeedKill_Fallen_C,
            365551, // x1_Event_SpeedKill_GhostHumansA,
            370053, // x1_Event_SpeedKill_Ghoul_E,
            364644, // x1_Event_SpeedKill_GoatmanA,
            365509, // x1_Event_SpeedKill_Goatman_Melee_A_Ghost,
            370044, // x1_Event_SpeedKill_Goatmutant_B,
            369873, // x1_Event_SpeedKill_Lacuni_B,
            370049, // x1_Event_SpeedKill_Monstrosity_Scorpion_A,
            369910, // x1_Event_SpeedKill_Rare_Ghoul_B,
            365622, // x1_Event_SpeedKill_Rare_Skeleton2HandA,
            370147, // x1_Event_SpeedKill_Rare_ThousandPounder,
            370359, // x1_Event_SpeedKill_Rare_demonTrooper_C,
            370499, // x1_Event_SpeedKill_Rare_x1_westmarchBrute_C,
            370060, // x1_Event_SpeedKill_Skeleton_E,
            364635, // x1_Event_SpeedKill_SkeletonsA,
            369856, // x1_Event_SpeedKill_Snakeman_A,
            369884, // x1_Event_SpeedKill_Spiderling_B,
            369863, // x1_Event_SpeedKill_Swarm_A,
            370666, // x1_Event_SpeedKill_TentacleBears,
            369832, // x1_Event_SpeedKill_TriuneCultist_C,
            365526, // x1_Event_SpeedKill_TriuneVesselA,
            365547, // x1_Event_SpeedKill_ZombieB,
            370033, // x1_Event_SpeedKill_demonFlyer_B,
            369817, // x1_Event_SpeedKill_electricEel_A,
            369837, // x1_Event_SpeedKill_fastMummy_A,
            370329, // x1_Event_SpeedKill_morluMelee_B,
            370482, // x1_Event_SpeedKill_x1_BileCrawler_Skeletal_A,
            370435, // x1_Event_SpeedKill_x1_BogFamily_A,
            370452, // x1_Event_SpeedKill_x1_Monstrosity_ScorpionBug_A,
            370427, // x1_Event_SpeedKill_x1_Skeleton_Ghost_A,
            370561, // x1_Event_SpeedKill_x1_Tentacle_A,
            370445, // x1_Event_SpeedKill_x1_bogBlight_Maggot_A,
            370466, // x1_Event_SpeedKill_x1_leaperAngel_A,
            370489, // x1_Event_SpeedKill_x1_portalGuardianMinion_A,
            370476, // x1_Event_SpeedKill_x1_westmarchHound_A,
            369431, // x1_Event_WaveFight_AncientEvils,
            365751, // x1_Event_WaveFight_ArmyOfTheDead,
            368092, // x1_Event_WaveFight_BloodClanAssault,
            365300, // x1_Event_WaveFight_ChamberOfBone,
            365033, // x1_Event_WaveFight_CultistLegion,
            368056, // x1_Event_WaveFight_DeathChill,
            365678, // x1_Event_WaveFight_FallenWarband,
            368124, // x1_Event_WaveFight_ForsakenSoldiers,
            369482, // x1_Event_WaveFight_HostileRealm,
            368334, // x1_Event_WaveFight_Juggernaut,
            365133, // x1_Event_WaveFight_KhazraWarband,
            365953, // x1_Event_WaveFight_SunkenGrave,
        };



        public static HashSet<string> VanityItems { get { return DataDictionary.vanityItems; } }
        private static readonly HashSet<string> vanityItems = new HashSet<string>()
        {
            "x1_AngelWings_Imperius", // Wings of Valor
            "X1_SpectralHound_Skull_promo", // Liber Canis Mortui
            "WoDFlag", // Warsong Pennant
        };

        public static HashSet<int> NeverTownPortalLevelAreaIds { get { return neverTownPortalLevelAreaIds; } }
        private static readonly HashSet<int> neverTownPortalLevelAreaIds = new HashSet<int>()
        {
            202446, // A1 New Tristram "Attack Area"
            //19947, // A1 New Tristram "Attack Area"

            284069, // A5 Westmarch Overlook
            308323, // A5 Westmarch Wolf Gate
            315938, // A5 Westmarch Wolf Gate
            316374, // A5 Westmarch Storehouse
            311624, // A5 Westmarch Cathedral Courtyard
            311623, // A5 Streets of Westmarch
            309413, // A5 Westmarch Cathedral

        };


        public static HashSet<int> ForceTownPortalLevelAreaIds { get { return DataDictionary.forceTownPortalLevelAreaIds; } }
        private static readonly HashSet<int> forceTownPortalLevelAreaIds = new HashSet<int>
        {
            55313, // Act 2 Caldeum Bazaar
        };



        /// <summary>
        /// Contains the list of Boss Level Area ID's
        /// </summary>
        public static HashSet<int> BossLevelAreaIDs { get { return bossLevelAreaIDs; } }
        private static readonly HashSet<int> bossLevelAreaIDs = new HashSet<int>
        { 
            109457, 185228, 60194, 130163, 60714, 19789, 62726, 90881, 195268, 58494, 81178, 60757, 111232, 112580, 
            119656, 111516, 143648, 215396, 119882, 109563, 153669, 215235, 55313, 60193, 19789, 330576,
        };

        /// <summary>
        /// A list of LevelAreaId's that the bot should always use Straight line pathing (no navigator)
        /// </summary>
        public static HashSet<int> StraightLinePathingLevelAreaIds { get { return DataDictionary.straightLinePathingLevelAreaIds; } }
        private static readonly HashSet<int> straightLinePathingLevelAreaIds = new HashSet<int>
        {
            60757, // Belial's chambers
            405915, // p1_TieredRift_Challenge
        };

        public static HashSet<int> QuestLevelAreaIds { get { return DataDictionary.questLevelAreaIds; } }
        private static readonly HashSet<int> questLevelAreaIds = new HashSet<int>
        {
            202446, // A1 New Tristram "Attack Area"
            19947, // A1 New Tristram
            109457, // A1 New Tristram Inn
            109457, // A1 The Slaughtered Calf Inn
            62968, // A1 The Hidden Cellar
            60714, // A1 Leoric's Passage
            83110, // A1 Cellar of the Damned
            19935, // A1 Wortham
            100854, // A1 Khazra Den
            94672, // A1 Cursed Hold

            60757, // A2 Belial's chambers
            55313, // A2 Caldeum Bazaar
            102964, // A2 City of Caldeum

            309413, // A5 Westmarch Cathedral

            336846, // x1_westm_KingEvent01 - Westmarch Commons Contested Villa
            405915, // p1_TieredRift_Challenge
        };


        /// <summary>
        /// This list is used when an actor has an attribute BuffVisualEffect=1, e.g. fire floors in The Butcher arena
        /// </summary>
        public static HashSet<int> ButcherFloorPanels { get { return butcherFloorPanels; } }
        private static readonly HashSet<int> butcherFloorPanels = new HashSet<int>
        {
            // Butcher Floor Panels
            201454, 201464, 201426, 201438, 200969, 201423, 201242,
        };

        /// <summary>
        /// This list is used for Units with specific Animations we want to treat as avoidance
        /// </summary>
        public static HashSet<DoubleInt> AvoidanceAnimations { get { return DataDictionary.avoidanceAnimations; } }
        private static readonly HashSet<DoubleInt> avoidanceAnimations = new HashSet<DoubleInt>
        {
            // Fat guys that explode into worms
            // Stitch_Suicide_Bomb State=Transform By: Corpulent_C (3849)
            new DoubleInt((int)SNOActor.Corpulent_A, (int)SNOAnim.Stitch_Suicide_Bomb),
            new DoubleInt((int)SNOActor.Corpulent_A_Unique_01, (int)SNOAnim.Stitch_Suicide_Bomb),
            new DoubleInt((int)SNOActor.Corpulent_A_Unique_02, (int)SNOAnim.Stitch_Suicide_Bomb), 
            new DoubleInt((int)SNOActor.Corpulent_A_Unique_03, (int)SNOAnim.Stitch_Suicide_Bomb), 
            new DoubleInt((int)SNOActor.Corpulent_B, (int)SNOAnim.Stitch_Suicide_Bomb), 
            new DoubleInt((int)SNOActor.Corpulent_B_Unique_01, (int)SNOAnim.Stitch_Suicide_Bomb), 
            new DoubleInt((int)SNOActor.Corpulent_C, (int)SNOAnim.Stitch_Suicide_Bomb), 
            new DoubleInt((int)SNOActor.Corpulent_D_CultistSurvivor_Unique, (int)SNOAnim.Stitch_Suicide_Bomb),             
            new DoubleInt((int)SNOActor.Corpulent_C_OasisAmbush_Unique, (int)SNOAnim.Stitch_Suicide_Bomb),  
            new DoubleInt((int)SNOActor.Corpulent_D_Unique_Spec_01, (int)SNOAnim.Stitch_Suicide_Bomb), 

            new DoubleInt(330824, (int)SNOAnim.x1_Urzael_attack_06), // Urzael flame 
            new DoubleInt(330824, 348109), // Urzael Cannonball Aim
            new DoubleInt(330824, 344952), // Urzael Flying

            // Spinny AOE Attack
            new DoubleInt((int)SNOActor.x1_LR_DeathMaiden_A, (int)SNOAnim.x1_deathMaiden_attack_special_360_01),

            new DoubleInt((int)SNOActor.x1_portalGuardianMinion_Melee_A, (int)SNOAnim.x1_portalGuardianMinion_attack_charge_01), // x1_portalGuardianMinion_Melee_A (279052)
            new DoubleInt((int)SNOActor.X1_BigRed_Chronodemon_Burned_A, (int)SNOAnim.X1_BigRed_attack_02), // X1_BigRed_Chronodemon_Burned_A (326670)
            
            // Big guys with blades on their arms who jump accross the screen and stun you
            // x1_westmarchBrute_attack_02_out State=Attacking By: x1_westmarchBrute_A (258678)
            new DoubleInt((int)SNOActor.x1_westmarchBrute_A, (int)SNOAnim.x1_westmarchBrute_attack_02_in), 
            new DoubleInt((int)SNOActor.x1_westmarchBrute_A, (int)SNOAnim.x1_westmarchBrute_attack_02_mid), 
            new DoubleInt((int)SNOActor.x1_westmarchBrute_A, (int)SNOAnim.x1_westmarchBrute_attack_02_out),   
           
            // snakeMan_melee_generic_cast_01 State=Transform By: X1_LR_Boss_Snakeman_Melee_Belial (360281)
            new DoubleInt((int)SNOActor.X1_LR_Boss_Snakeman_Melee_Belial, (int)SNOAnim.snakeMan_melee_generic_cast_01),  
 
            //x1_Squigglet_Generic_Cast State=Transform By: X1_LR_Boss_Squigglet (353535)
            new DoubleInt((int)SNOActor.X1_LR_Boss_Squigglet, (int)SNOAnim.x1_Squigglet_Generic_Cast),
       };

        /// <summary>
        /// This list is used for Units with specific Animations we want to treat as avoidance
        /// </summary>
        public static readonly HashSet<DoubleInt> DirectionalAvoidanceAnimations = new HashSet<DoubleInt>
        {
            // Beast Charge
            new DoubleInt((int)SNOActor.Beast_A, (int)SNOAnim.Beast_start_charge_02),
            new DoubleInt((int)SNOActor.Beast_A, (int)SNOAnim.Beast_charge_02),
            new DoubleInt((int)SNOActor.Beast_A, (int)SNOAnim.Beast_charge_04),
            new DoubleInt((int)SNOActor.Beast_B, (int)SNOAnim.Beast_start_charge_02),
            new DoubleInt((int)SNOActor.Beast_B, (int)SNOAnim.Beast_charge_02),
            new DoubleInt((int)SNOActor.Beast_B, (int)SNOAnim.Beast_charge_04),
            new DoubleInt((int)SNOActor.Beast_C, (int)SNOAnim.Beast_start_charge_02),
            new DoubleInt((int)SNOActor.Beast_C, (int)SNOAnim.Beast_charge_02),
            new DoubleInt((int)SNOActor.Beast_C, (int)SNOAnim.Beast_charge_04),
            new DoubleInt((int)SNOActor.Beast_D, (int)SNOAnim.Beast_start_charge_02),
            new DoubleInt((int)SNOActor.Beast_D, (int)SNOAnim.Beast_charge_02),
            new DoubleInt((int)SNOActor.Beast_D, (int)SNOAnim.Beast_charge_04),

            // Nobody wants to get hit by a mallet demon
            new DoubleInt(343767, (int)SNOAnim.malletDemon_attack_01), // X1_LR_Boss_MalletDemon
            new DoubleInt(106709, (int)SNOAnim.malletDemon_attack_01), // MalletDemon_A
            new DoubleInt(219736, (int)SNOAnim.malletDemon_attack_01), // MalletDemon_A_Unique_01  
            new DoubleInt(219751, (int)SNOAnim.malletDemon_attack_01), // MalletDemon_A_Unique_02 

           
            // Angels with those big clubs with a dashing attack
            // Angel_Corrupt_attack_dash_in State=Transform By: Angel_Corrupt_A (106711)
            new DoubleInt((int)SNOActor.Angel_Corrupt_A, (int)SNOAnim.Angel_Corrupt_attack_dash_in), 
            new DoubleInt((int)SNOActor.Angel_Corrupt_A, (int)SNOAnim.Angel_Corrupt_attack_dash_middle), 
            new DoubleInt((int)SNOActor.Angel_Corrupt_A, (int)SNOAnim.Angel_Corrupt_attack_dash_out), 
           

            //] Triune_Berserker_specialAttack_loop_01 State=TakingDamage By: Triune_Berserker_A (6052)
            new DoubleInt((int)SNOActor.Triune_Berserker_A, (int)SNOAnim.Triune_Berserker_specialAttack_01),
            new DoubleInt((int)SNOActor.Triune_Berserker_A, (int)SNOAnim.Triune_Berserker_specialAttack_loop_01),
            new DoubleInt((int)SNOActor.Triune_Berserker_B, (int)SNOAnim.Triune_Berserker_specialAttack_01),
            new DoubleInt((int)SNOActor.Triune_Berserker_B, (int)SNOAnim.Triune_Berserker_specialAttack_loop_01),
            new DoubleInt((int)SNOActor.Triune_Berserker_C, (int)SNOAnim.Triune_Berserker_specialAttack_01),
            new DoubleInt((int)SNOActor.Triune_Berserker_C, (int)SNOAnim.Triune_Berserker_specialAttack_loop_01),
            new DoubleInt((int)SNOActor.Triune_Berserker_D, (int)SNOAnim.Triune_Berserker_specialAttack_01),
            new DoubleInt((int)SNOActor.Triune_Berserker_D, (int)SNOAnim.Triune_Berserker_specialAttack_loop_01),
       };

        /// <summary>
        /// This list is used for animations where the avoidance point should be the player's current location
        /// </summary>
        public static HashSet<int> AvoidAnimationAtPlayer { get { return avoidAnimationAtPlayer; } }
        private static readonly HashSet<int> avoidAnimationAtPlayer = new HashSet<int>
        {
            (int)SNOAnim.Beast_start_charge_02, // A1 Savage Beast Charge - needs special handling!
            (int)SNOAnim.Beast_charge_02, // A1 Savage Beast Charge - needs special handling!
            (int)SNOAnim.Beast_charge_04, // A1 Savage Beast Charge - needs special handling!
            (int)SNOAnim.morluSpellcaster_attack_AOE_01, //morluSpellcaster_D
            (int)SNOAnim.X1_LR_Boss_morluSpellcaster_generic_cast, //morluSpellcaster_D
            (int)SNOAnim.snakeMan_melee_generic_cast_01, //X1_LR_Boss_Snakeman_Melee_Belial (360281)
       };

        public static Dictionary<int, float> DefaultAvoidanceAnimationCustomRadius { get { return defaultAvoidanceAnimationCustomRadius; } }
        private static readonly Dictionary<int, float> defaultAvoidanceAnimationCustomRadius = new Dictionary<int, float>()
        {
            {(int)SNOAnim.morluSpellcaster_attack_AOE_01, 20f },
            {(int)SNOAnim.x1_deathMaiden_attack_special_360_01, 15f},
            {(int)SNOAnim.x1_Squigglet_Generic_Cast, 40f}, // Rift Boss Slime AOE
        };

        /// <summary>
        /// A list of all the SNO's to avoid - you could add
        /// </summary>
        public static HashSet<int> Avoidances { get { return avoidances; } }
        private static readonly HashSet<int> avoidances = new HashSet<int>
        {
            219702, // Arcane
            221225, // Arcane 2
            5482,   // Poison Tree
            6578,   // Poison Tree
            4803,   // monsterAffix_Molten_deathStart_Proxy
            4804,   // monsterAffix_Molten_deathExplosion_Proxy 
            4806,   // monsterAffix_Electrified_deathExplosion_proxy
            224225, // Molten Core 2
            247987, // Molten Core 2
            95868,  // Molten Trail
            108869, // Plague Cloud
            402,    // Ice Balls
            223675, // Ice Balls
            5212,   // Bees-Wasps
            3865,   // Plague-Hands
            123124, // Azmo Pools
            123842, // Azmo fireball
            123839, // Azmo bodies
            161822, // Belial 1
            161833, // Belial 2
            4103,   // Sha-Ball
            160154, // Mol Ball
            432,    // Mage Fire
            168031, // Diablo Prison
            214845, // Diablo Meteor
            260377, // Ice-trail
            185924, // Zolt Bubble
            139741, // Zolt Twister
            93837,  // Ghom Gas
            166686, // Maghda Proj
            226350, // Diablo Ring of Fire
            226525, // Diablo Ring of Fire
            250031, // Mortar MonsterAffix_Mortar_Pending

            84608,  // Desecrator monsterAffix_Desecrator_damage_AOE
            84606,  // Desecrator monsterAffix_Desecrator_telegraph

            /* 2.0 */
            349774, // FrozenPulse x1_MonsterAffix_frozenPulse_monster
            343539, // Orbiter X1_MonsterAffix_Orbiter_Projectile
            316389, // PoisonEnchanted x1_MonsterAffix_CorpseBomber_projectile (316389)
            340319, // PoisonEnchanted x1_MonsterAffix_CorpseBomber_bomb_start (340319)
            341512, // Thunderstorm x1_MonsterAffix_Thunderstorm_Impact
            337109, // Wormhole X1_MonsterAffix_TeleportMines

            338889, // x1_Adria_bouncingProjectile
            360738, // X1_Adria_arcanePool
            358404, // X1_Adria_blood_large

            360598, // x1_Urzael_CeilingDebris_DamagingFire_wall
            359205, // x1_Urzael_ceilingDebris_Impact_Beam
            360883, // x1_Urzael_ceilingDebris_Impact_Circle

            362850, // x1_Urzael_Cannonball_burning_invisible
            346976, // x1_Urzael_Cannonball_burning_impact
            346975, // x1_Urzael_Cannonball_burning

            335505, // x1_malthael_drainSoul_ghost
            325136, // x1_Malthael_DeathFogMonster
            340512, // x1_Malthael_Mephisto_LightningObject

            359703, // X1_Unique_Monster_Generic_AOE_DOT_Cold_10foot
            363356, // X1_Unique_Monster_Generic_AOE_DOT_Cold_20foot
            359693, // X1_Unique_Monster_Generic_AOE_DOT_Fire_10foot
            363357, // X1_Unique_Monster_Generic_AOE_DOT_Fire_20foot
            364542, // X1_Unique_Monster_Generic_AOE_DOT_Lightning_10foot
            364543, // X1_Unique_Monster_Generic_AOE_DOT_Lightning_20foot
            377537, // X1_Unique_Monster_Generic_AOE_DOT_Lightning_hardpoints
            360046, // X1_Unique_Monster_Generic_AOE_DOT_Poison_10foot
            363358, // X1_Unique_Monster_Generic_AOE_DOT_Poison_20foot
            368156, // X1_Unique_Monster_Generic_AOE_Lightning_Ring
            358917, // X1_Unique_Monster_Generic_AOE_Sphere_Distortion
            377086, // X1_Unique_Monster_Generic_Projectile_Arcane
            377087, // X1_Unique_Monster_Generic_Projectile_Cold
            377088, // X1_Unique_Monster_Generic_Projectile_Fire
            377089, // X1_Unique_Monster_Generic_Projectile_Holy
            377090, // X1_Unique_Monster_Generic_Projectile_Lightning
            377091, // X1_Unique_Monster_Generic_Projectile_Physical
            377092, // X1_Unique_Monster_Generic_Projectile_Poison
            (int)SNOActor.x1_LR_boss_terrorDemon_A_projectile, 

        };

        /// <summary>
        /// A list of SNO's that are projectiles (so constantly look for new locations while avoiding)
        /// </summary>
        public static HashSet<int> AvoidanceProjectiles { get { return avoidanceProjectiles; } }
        private static readonly HashSet<int> avoidanceProjectiles = new HashSet<int>
        {
            5212,   // Bees-Wasps
            4103,   // Sha-Ball
            160154, // Molten Ball
            123842, // Azmo fireball
            139741, // Zolt Twister
            166686, // Maghda Projectile
            185999, // Diablo Expanding Fire
            196526, // Diablo Expanding Fire
            136533, // Diablo Lightning Breath
            343539, // Orbiter X1_MonsterAffix_Orbiter_Projectile
            341512, // Thunderstorm 
            316389, // PoisonEnchanted x1_MonsterAffix_CorpseBomber_projectile (316389)
            340319, // PoisonEnchanted x1_MonsterAffix_CorpseBomber_bomb_start (340319)
            
            // A5

            338889, // x1_Adria_bouncingProjectile

            362850, // x1_Urzael_Cannonball_burning_invisible
            346976, // x1_Urzael_Cannonball_burning_impact
            346975, // x1_Urzael_Cannonball_burning

            335505, // x1_malthael_drainSoul_ghost
            325136, // x1_Malthael_DeathFogMonster
            340512, // x1_Malthael_Mephisto_LightningObject

            377086, // X1_Unique_Monster_Generic_Projectile_Arcane
            377087, // X1_Unique_Monster_Generic_Projectile_Cold
            377088, // X1_Unique_Monster_Generic_Projectile_Fire
            377089, // X1_Unique_Monster_Generic_Projectile_Holy
            377090, // X1_Unique_Monster_Generic_Projectile_Lightning
            377091, // X1_Unique_Monster_Generic_Projectile_Physical
            377092, // X1_Unique_Monster_Generic_Projectile_Poison

            3528,   // Butcher_hook

            // 4394, //g_ChargedBolt_Projectile-200915 (4394) Type=Projectile
            // 368392, // x1_Cesspool_Slime_Posion_Attack_Projectile-222254 (368392) Type=Projectile
            (int)SNOActor.x1_LR_boss_terrorDemon_A_projectile, 
        };

        /// <summary>
        /// A list of SNO's that spawn AoE then disappear from the object manager
        /// </summary>
        public static HashSet<int> AvoidanceSpawners { get { return avoidanceSpawners; } }
        private static readonly HashSet<int> avoidanceSpawners = new HashSet<int>
        {
            5482,   // Poison Tree
            6578,   // Poison Tree
            316389, // PoisonEnchanted x1_MonsterAffix_CorpseBomber_projectile (316389)
            340319, // PoisonEnchanted x1_MonsterAffix_CorpseBomber_bomb_start (340319)
            159369, //MorluSpellcaster_Meteor_Pending-178011 (159369)
        };

        /// <summary>
        /// The duration the AoE from AvoidanceSpawners should be avoided for
        /// </summary>
        public static Dictionary<int, TimeSpan> AvoidanceSpawnerDuration { get { return avoidanceSpawnerDuration; } }
        private static readonly Dictionary<int, TimeSpan> avoidanceSpawnerDuration = new Dictionary<int, TimeSpan>
        {
            {5482, TimeSpan.FromSeconds(15)},   // Poison Tree
            {6578, TimeSpan.FromSeconds(15)},   // Poison Tree
            {316389, TimeSpan.FromSeconds(6)}, // PoisonEnchanted 
            {340319, TimeSpan.FromSeconds(6)}, // PoisonEnchanted 
            {4803, TimeSpan.FromSeconds(10)}, // Molten Core
            {4804, TimeSpan.FromSeconds(10)}, // Molten Core
            {224225, TimeSpan.FromSeconds(10)}, // Molten Core
            {247987, TimeSpan.FromSeconds(10)}, // Molten Core
            {159369, TimeSpan.FromSeconds(3)}, // Morlu Meteor
        };

        public static Dictionary<int, float> DefaultAvoidanceCustomRadius { get { return defaultAvoidanceCustomRadius; } }
        private static readonly Dictionary<int, float> defaultAvoidanceCustomRadius = new Dictionary<int, float>()
        {
            {330824, 35f }, // A5 Urzael animations
            {360598, 10f }, // x1_Urzael_CeilingDebris_DamagingFire_wall
            {359205, 20f }, // x1_Urzael_ceilingDebris_Impact_Beam
            {360883, 20f }, // x1_Urzael_ceilingDebris_Impact_Circle
            {362850, 12f }, // x1_Urzael_Cannonball_burning_invisible
            {346976, 12f }, // x1_Urzael_Cannonball_burning_impact
            {346975, 12f }, // x1_Urzael_Cannonball_burning

            {360738, 12f}, // X1_Adria_arcanePool
            {338889, 5f}, // x1_Adria_bouncingProjectile
            {358404, 15f}, // X1_Adria_blood_large

            {335505, 5f}, // x1_malthael_drainSoul_ghost
            {325136, 20f}, // x1_Malthael_DeathFogMonster
            {340512, 8f}, // x1_Malthael_Mephisto_LightningObject
            {343767, 35f }, // Mallet Demons
            {106709, 35f }, // Mallet Demons
            {219736, 35f }, // Mallet Demons
            {219751, 35f }, // Mallet Demons

            {159369, 20f }, //MorluMeteor
            {4103, 25f}, // Meteor
            {3528, 15f}, // Butcher_hook

            {359703, 10f}, // X1_Unique_Monster_Generic_AOE_DOT_Cold_10foot
            {363356, 20f}, // X1_Unique_Monster_Generic_AOE_DOT_Cold_20foot
            {359693, 10f}, // X1_Unique_Monster_Generic_AOE_DOT_Fire_10foot
            {363357, 20f}, // X1_Unique_Monster_Generic_AOE_DOT_Fire_20foot
            {364542, 10f}, // X1_Unique_Monster_Generic_AOE_DOT_Lightning_10foot
            {364543, 20f}, // X1_Unique_Monster_Generic_AOE_DOT_Lightning_20foot
            {377537, 10f}, // X1_Unique_Monster_Generic_AOE_DOT_Lightning_hardpoints
            {360046, 10f}, // X1_Unique_Monster_Generic_AOE_DOT_Poison_10foot
            {363358, 20f}, // X1_Unique_Monster_Generic_AOE_DOT_Poison_20foot
            {368156, 10f}, // X1_Unique_Monster_Generic_AOE_Lightning_Ring
            {358917, 10f}, // X1_Unique_Monster_Generic_AOE_Sphere_Distortion
            {377086, 10f}, // X1_Unique_Monster_Generic_Projectile_Arcane
            {377087, 10f}, // X1_Unique_Monster_Generic_Projectile_Cold
            {377088, 10f}, // X1_Unique_Monster_Generic_Projectile_Fire
            {377089, 10f}, // X1_Unique_Monster_Generic_Projectile_Holy
            {377090, 10f}, // X1_Unique_Monster_Generic_Projectile_Lightning
            {377091, 10f}, // X1_Unique_Monster_Generic_Projectile_Physical
            {377092, 10f}, // X1_Unique_Monster_Generic_Projectile_Poison
            {(int)SNOActor.x1_LR_boss_terrorDemon_A_projectile, 10f}, 

        };

        /*
         * Combat-related dictionaries/defaults
         */

        /// <summary>
        /// ActorSNO's of Very fast moving mobs (eg wasps), for special skill-selection decisions
        /// </summary>
        public static HashSet<int> FastMovingMonsterIds { get { return fastMovementMonsterIds; } }
        private static readonly HashSet<int> fastMovementMonsterIds = new HashSet<int>
        {
            5212
         };

        /// <summary>
        /// A list of crappy "summoned mobs" we should always ignore unless they are very close to us, eg "grunts", summoned skeletons etc.
        /// </summary>
        public static HashSet<int> ShortRangeAttackMonsterIds { get { return shortRangeAttackMonsterIds; } }
        private static readonly HashSet<int> shortRangeAttackMonsterIds = new HashSet<int>
        {
            4084, 4085, 5395, 144315,
         };

        /// <summary>
        /// Dictionary of Actor SNO's and cooresponding weighting/Priority 
        /// </summary>
        public static Dictionary<int, int> MonsterCustomWeights { get { return monsterCustomWeights; } }
        private static readonly Dictionary<int, int> monsterCustomWeights = new Dictionary<int, int>
        {
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

            // A5 Forgotton War trash
            { 300864, -300 },
         };


        /// <summary>
        /// A list of all known SNO's of treasure goblins/bandits etc.
        /// </summary>
        public static HashSet<int> GoblinIds { get { return goblinIds; } }
        private static readonly HashSet<int> goblinIds = new HashSet<int>
        {
            5984, 5985, 5987, 5988, 405186, 380657
         };

        /// <summary>
        /// Contains ActorSNO of ranged units that should be attacked even if outside of kill radius
        /// </summary>
        public static HashSet<int> RangedMonsterIds { get { return rangedMonsterIds; } }
        private static readonly HashSet<int> rangedMonsterIds = new HashSet<int>
        {
            365, 4100, // fallen
            4304, 4300, // goat shaman 
            4738, // pestilence 
            4299, // goat ranged
            62736, 130794, // demon flyer
            5508, // succubus 
            5388, 4286, 256015, 256000, 255996,
            5984, 5985, 5987, 5988, 405186, //goblins
       };
        // A list of bosses in the game, just to make CERTAIN they are treated as elites
        /// <summary>
        /// Contains ActorSNO of known Bosses
        /// </summary>
        public static HashSet<int> BossIds { get { return bossIds; } }
        private static readonly HashSet<int> bossIds = new HashSet<int>
        {
            // Siegebreaker (96192), Azmodan (89690), Cydea (95250), Heart-thing (193077), 
            96192,                   89690,           95250,         193077, 
            //Kulle (80509), Small Belial (220160), Big Belial (3349), Diablo 1 (114917), terror Diablo (133562)
            80509,           220160,                3349,              114917,            133562, 
            62975, // Belial TrueForm
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
            361347, //Jondar from the Adventure mode
            215103, // Istaku            
            4630, // Rakanoth
            256015, // Xah'Rith Keywarden
            115403, // A1 Cain Skeleton boss
            4373, 4376, 177539, // A1 Robbers
            168240, // A2 Jewler quest
            84919, // Skeleton King
            108444, // ZombieFemale_A_TristramQuest (Wretched Mothers)
            176889, // ZombieFemale_Unique_WretchedQueen
            129439, //Arsect The Venomous
            164502, // sandMonster_A_Head_Guardian
            378665, // Greed

            // A5
            316839, // x1_deathOrb_bodyPile
            375106, // A5 x1_Death_Orb_Monster
            375111, // A5 x1_Death_Orb_Master_Monster
            279394, // A5 Adria 

            300862, // X1_BigRed_Chronodemon_Event_ForgottenWar
            318425, // X1_CoreEliteDemon_Chronodemon_Event_ForgottenWar

            300866, // X1_Angel_TrooperBoss_Event_ForgottenWar

            346482, // X1_PandExt_TimeTrap 
            367456, // x1_Pand_Ext_Event_Hive_Blocker

            347276, // x1_Fortress_Soul_Grinder_A

            374751, // x1_PortalGuardian_A
            307339, // X1_Rockworm_Pand_Unique_HexMaze
            297730, // x1_Malthael_Boss
        };

        // Three special lists used purely for checking for the existance of a player's summoned mystic ally, gargantuan, or zombie dog

        public static HashSet<int> MysticAllyIds { get { return mysticAllyIds; } }
        private static readonly HashSet<int> mysticAllyIds = new HashSet<int>
        { 
            169123, 123885, 169890, 168878, 169891, 169077, 169904, 169907, 169906, 169908, 169905, 169909 
        };

        public static HashSet<int> GargantuanIds { get { return gargantuanIds; } }
        private static readonly HashSet<int> gargantuanIds = new HashSet<int>
        { 
            179780, 179778, 179772, 179779, 179776, 122305 };

        public static HashSet<int> ZombieDogIds { get { return zombieDogIds; } }
        private static readonly HashSet<int> zombieDogIds = new HashSet<int>
        { 
            110959, 103235, 103215, 105763, 103217, 51353, 
        };

        public static HashSet<int> DemonHunterPetIds { get { return demonHunterPetIds; } }
        private static readonly HashSet<int> demonHunterPetIds = new HashSet<int>
        { 
            178664, 
            173827, 
            133741, 
            159144, 
            181748, 
            159098,
            159102,
            159144,
            334861,

        };

        public static HashSet<int> DemonHunterSentryIds { get { return demonHunterSentryIds; } }
        private static readonly HashSet<int> demonHunterSentryIds = new HashSet<int>
        { 
           141402, 150025, 150024, 168815, 150026, 150027,
        };

        public static HashSet<int> WizardHydraIds { get { return wizardHydraIds; } }
        private static readonly HashSet<int> wizardHydraIds = new HashSet<int>
        { 
            // Some hydras are 3 monsters, only use one of their heads.
            82972, //Type: Monster Name: Wizard_HydraHead_Frost_1-1037 ActorSNO: 82972
            83959, // Type: Monster Name: Wizard_HydraHead_Big-1168 ActorSNO: 83959
            325807, // Type: Monster Name: Wizard_HydraHead_fire2_1-1250 ActorSNO: 325807
            82109, // Type: Monster Name: Wizard_HydraHead_Lightning_1-1288 ActorSNO: 82109, 
            81515, // Type: Monster Name: Wizard_HydraHead_Arcane_1-1338 ActorSNO: 81515, 
            80745, // Type: Monster Name: Wizard_HydraHead_Default_1-1364 ActorSNO: 80745, 
        };

        /// <summary>
        /// World-object dictionaries eg large object lists, ignore lists etc.
        /// A list of SNO's to *FORCE* to type: Item. (BE CAREFUL WITH THIS!).
        /// </summary>
        public static HashSet<int> ForceToItemOverrideIds { get { return forceToItemOverrideIds; } }
        private static readonly HashSet<int> forceToItemOverrideIds = new HashSet<int>
        {
            166943, // DemonTrebuchetKey, infernal key
            255880, // DemonKey_Destruction
            255881, // DemonKey_Hate
            255882, // DemonKey_Terror
        };

        /// <summary>
        /// Interactable whitelist - things that need interacting with like special wheels, levers - they will be blacklisted for 30 seconds after one-use
        /// </summary>
        public static HashSet<int> InteractWhiteListIds { get { return interactWhiteListIds; } }
        private static readonly HashSet<int> interactWhiteListIds = new HashSet<int>
        {
            56686, // a3dun_Keep_Bridge_Switch 
            211999, // a3dun_Keep_Bridge_Switch_B 
            52685, // a3dun_Keep_Bridge
            54882, // a3dun_Keep_Door_Wooden_A
            105478, // a1dun_Leor_Spike_Spawner_Switch
            102927, // A1 Cursed Hold Prisoners
            5747, // A1 Cathedral Switch
            365097, // Cursed Chest - Damp Cellar

            // A5
            348096, // Paths of the Drowned - portal switches - x1_Bog_Beacon_B
            361364, // A5 Siege Rune Path of War

            274457, // A5 Spirit of Malthael - Tower of Korelan
            368515, // A5 Nephalem Switch - Passage to Corvus 

            354407, // X1_Angel_Common_Event_GreatWeapon

        };

        public static HashSet<int> HighPriorityInteractables { get { return highPriorityInteractables; } }
        private static readonly HashSet<int> highPriorityInteractables = new HashSet<int>
        {
            56686, // a3dun_Keep_Bridge_Switch 
            211999, // a3dun_Keep_Bridge_Switch_B 
        };

        public static Dictionary<int, int> InteractEndAnimations { get { return interactEndAnimations; } }
        private static readonly Dictionary<int, int> interactEndAnimations = new Dictionary<int, int>()
        {
            {348096, 348093}, // x1_Bog_Beacon_B
        };

        /// <summary>
        /// NOTE: you don't NEED interactable SNO's listed here. But if they are listed here, *THIS* is the range at which your character will try to walk to within the object
        /// BEFORE trying to actually "click it". Certain objects need you to get very close, so it's worth having them listed with low interact ranges
        /// </summary>
        public static Dictionary<int, float> CustomObjectRadius { get { return customObjectRadius; } }
        private static readonly Dictionary<int, float> customObjectRadius = new Dictionary<int, float>
        {
            {56686, 4}, 
            {52685, 4}, 
            {54882, 40}, 
            {3349, 25}, // Belial
            {225270, 35}, 
            {180575, 10},  // Diablo Arena Health Well
            {375111, 45f}, // A5 x1_Death_Orb_Master_Monster
            {301177, 15f}, // x1_PandExt_Time_Activator
            {368515, 5f}, // A5 Nephalem Switch -  Passage to Corvus
            {309432, 37f}, // x1_westm_Bridge
            {54850, 14f}, // a3dun_Keep_SiegeTowerDoor
            {325136, 15f},
        };

        /// <summary>
        /// Navigation obstacles for standard navigation down dungeons etc. to help DB movement
        /// MAKE SURE you add the *SAME* SNO to the "size" dictionary below, and include a reasonable size (keep it smaller rather than larger) for the SNO.
        /// </summary>
        public static HashSet<int> NavigationObstacleIds { get { return navigationObstacleIds; } }
        private static readonly HashSet<int> navigationObstacleIds = new HashSet<int>
        {
            174900, 185391, // demonic forge
            194682, 81699, 3340, 123325, 

            158681, // A1 Blacksmith_Lore
            104596, // A1 trOut_FesteringWoods_Neph_Column_B
            104632, // A1 trOut_FesteringWoods_Neph_Column_B_Broken_Base
            105303, // A1 trOut_FesteringWoods_Neph_Column_C_Broken_Base_Bottom
            104827, // A1 trOut_FesteringWoods_Neph_Column_C_Broken_Base 

            332924, // x1_Bog_bloodSpring_small, Blood Spring - Overgrown Ruins
            332922, // x1_Bog_bloodSpring_medium
            332923, // x1_Bog_bloodSpring_large     
            321855, // x1_Pand_Ext_Ordnance_Mine
            355898, // x1_Bog_Family_Guard_Tower_Stump
            376917, // [1FA3B814] Type: ServerProp Name: x1_Westm_Hub_Stool_A-381422 ActorSNO: 376917, Distance: 2.337004
            (int)SNOActor.px_Bounty_Camp_Hellportals_Frame, // A4 bounties
            (int)SNOActor.px_Bounty_Camp_Pinger, // A4 bounties

            // DyingHymn A4 Bounties
            433402, 
            434971, 
        };

        /// <summary>
        /// Size of the navigation obstacles above (actual SNO list must be matching the above list!)
        /// </summary>
        public static Dictionary<int, float> ObstacleCustomRadius { get { return obstacleCustomRadius; } }
        private static readonly Dictionary<int, float> obstacleCustomRadius = new Dictionary<int, float>
        {
            {174900, 25}, {194682, 20}, {81699, 20}, {3340, 12}, {123325, 25}, {185391, 25},
            {104596, 15}, // trOut_FesteringWoods_Neph_Column_B
            {104632, 15}, // trOut_FesteringWoods_Neph_Column_B_Broken_Base
            {105303, 15}, // trOut_FesteringWoods_Neph_Column_C_Broken_Base_Bottom
            {104827, 15}, // trOut_FesteringWoods_Neph_Column_C_Broken_Base 
            {355898, 12}, // x1_Bog_Family_Guard_Tower_Stump
            {376917, 10}, 

            // DyingHymn A4 Bounties
            {433402, 8},
            {434971, 10},
            
         };

        public static HashSet<int> ForceDestructibles { get { return forceDestructibles; } }
        private static HashSet<int> forceDestructibles = new HashSet<int>()
        {
            273323, // x1_westm_Door_Wide_Clicky
            55325, // a3dun_Keep_Door_Destructable

            225252, // Shamanic Ward - Revenge of Gharbad bounty

            331397, // x1_westm_Graveyard_Floor_Sarcophagus_Undead_Husband_Event

            386274, // Tgoblin_Gold_Pile_C

            211861, //Pinata
        };

        /// <summary>
        /// This is the RadiusDistance at which destructibles and barricades (logstacks, large crates, carts, etc.) are added to the cache
        /// </summary>
        public static Dictionary<int, float> DestructableObjectRadius { get { return destructableObjectRadius; } }
        private static readonly Dictionary<int, float> destructableObjectRadius = new Dictionary<int, float>
        {
            {2972, 10}, {80357, 16}, {116508, 10}, {113932, 8}, {197514, 18}, {108587, 8}, {108618, 8}, {108612, 8}, {116409, 18}, {121586, 22},
            {195101, 10}, {195108, 25}, {170657, 5}, {181228, 10}, {211959, 25}, {210418, 25}, {174496, 4}, {193963, 5}, {159066, 12}, {160570, 12},
            {55325, 5}, {5718, 14}, {5909, 10}, {5792, 8}, {108194, 8}, {129031, 30}, {192867, 3.5f}, {155255, 8}, {54530, 6}, {157541, 6},
            {93306, 10},
         };

        /// <summary>
        /// Destructible things that need targeting by a location instead of an ACDGUID (stuff you can't "click on" to destroy in-game)
        /// </summary>
        public static HashSet<int> DestroyAtLocationIds { get { return destroyAtLocationIds; } }
        private static readonly HashSet<int> destroyAtLocationIds = new HashSet<int>
        {
            170657, 116409, 121586, 155255, 104596, 93306,
         };

        /// <summary>
        /// Resplendent chest SNO list
        /// </summary>
        public static HashSet<int> ResplendentChestIds { get { return resplendentChestIds; } }
        private static readonly HashSet<int> resplendentChestIds = new HashSet<int>
        {
            62873, 95011, 81424, 108230, 111808, 111809, 211861, 62866, 109264, 62866, 62860, 96993,
            // Magi
			112182,
			363725, 357331, // chests after Cursed Chest

             301177, // A5 Timeless Prison Switch
        };
        /// <summary>
        /// Objects that should never be ignored due to no Line of Sight (LoS) or ZDiff
        /// </summary>
        public static HashSet<int> LineOfSightWhitelist { get { return lineOfSightWhitelist; } }
        private static readonly HashSet<int> lineOfSightWhitelist = new HashSet<int>
        {
            116807, // Butcher Health Well
            180575, // Diablo arena Health Well
            129031, // A3 Skycrown Catapults
            220160, // Small Belial (220160), 
            3349,   // Big Belial (3349),    
            210268, // Corrupt Growths 2nd Tier
            193077, // a3dun_Crater_ST_GiantDemonHeart_Mob

            375106, // A5 x1_Death_Orb_Monster
            375111, // A5 x1_Death_Orb_Master_Monster

            329390, // x1_Pand_BatteringRam_Hook_B_low
            368515, // A5 Nephalem Switch -  Passage to Corvus 
            347276, // x1_Fortress_Soul_Grinder_A
            326937, // x1_Pand_BatteringRam_Hook_B
            291368, // x1_Urzael_Boss
        };

        /// <summary>
        /// Chests/average-level containers that deserve a bit of extra radius (ie - they are more worthwhile to loot than "random" misc/junk containers)
        /// </summary>
        public static HashSet<int> ContainerWhiteListIds { get { return containerWhiteListIds; } }
        private static readonly HashSet<int> containerWhiteListIds = new HashSet<int>
        {
            62859,  // TrOut_Fields_Chest
            62865,  // TrOut_Highlands_Chest
            62872,  // CaOut_Oasis_Chest
            78790,  // trOut_wilderness_chest
            79016,  // trOut_Tristram_chest
            94708,  // a1dun_Leor_Chest
            96522,  // a1dun_Cath_chest
            130170, // a3dun_Crater_Chest
            108122, // caOut_StingingWinds_Chest
            111870, // A3_Battlefield_Chest_Snowy
            111947, // A3_Battlefield_Chest_Frosty
            213447, // Lore_AzmodanChest3
            213446, // Lore_AzmodanChest2
            51300,  // a3dun_Keep_Chest_A
            179865, // a3dun_Crater_ST_Chest
            109264, // a3dun_Keep_Chest_Rare
            212491, // a1dun_Random_Cloud
            210422, // a1dun_random_pot_of_gold_A
            211861, // Pinata
			196945, // a2dun_Spider_EggSack__Chest
            70534,  // a2dun_Spider_Chest
            289794, // Weaponracks on battlefields of eternity --> best place to farm white crafting materials
            103919, // Demonic Vessels         
            78030,  // GizmoType: Chest Name: trOut_Wilderness_Scarecrow_A-3924 ActorSNO: 78030 
            173325, // Anvil of Fury

            301177, // A5 Timeless Prison Switch

            // Kevin Spacey was here
            193023, //LootType3_GraveGuard_C_Corpse_03
			156682, //Adventurer_A_Corpse_01_WarrivEvent
			5758, //trDun_Cath_FloorSpawner_01
			5724, //trDun_Cath_BookcaseShelves_A
			85790, //Cath_Lecturn_ LachdanansScroll
			227305, //Lore_Inarius_Corrupt
			137125, //FesteringWoods_WarriorsRest_Lore
        };

        /// <summary>
        /// Contains ActorSNO's of world objects that should be blacklisted
        /// </summary>
        public static HashSet<int> BlackListIds { get { return blacklistIds; } }
        private static HashSet<int> blacklistIds = new HashSet<int>
        {
            
            362323, // x1_WestmHub_GuardNoHelmUnarmed
            // World Objects
            163449, 2909, 58283, 58321, 87809, 90150, 91600, 97023, 97350, 97381, 72689, 121327, 54515, 3340, 122076, 123640,
            60665, 60844, 78554, 86400, 86428, 81699, 86266, 86400, 6190, 80002, 104596, 58836, 104827, 74909, 6155, 6156, 6158, 6159, 75132,
            181504, 91688, 3007, 3011, 3014, 130858, 131573, 214396, 182730, 226087, 141639, 206569, 15119, 54413, 54926, 2979, 5776, 3949,
            108490, 52833, 200371, 153752, 2972, 206527, 3628,
            //a3dun_crater_st_Demo_ChainPylon_Fire_Azmodan, a3dun_crater_st_Demon_ChainPylon_Fire_MistressOfPain
            198977, 201680,
            217285,  //trOut_Leor_painting
            5902, // trDun_Magic_Painting_H_NoSpawn
            // uber fire chains in Realm of Turmoil  
            263014,
            249192, 251416, 249191, 251730, // summoned skeleton from ring   
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
            4176, 178664, 173827, 133741, 159144, 181748, 159098, 206569, 200706, 5895, 5896, 5897, 5899, 4686, 85843, 249338,
            251416, 249192, 4798, 183892,196899, 196900, 196903, 223333, 220636, 218951, 245838,
            //bone pile
            218951,245838,
            // rrrix act 1
            108882, 245919, 5944, 165475, 199998, 168875, 105323, 85690, 105321, 108266, 89578,
            175181, // trDun_Crypt_Skeleton_King_Throne_Parts 
            // rrrix act 2
            213907, 92519, 61544, 105681, 113983, 114527, 114642, 139933, 144405, 156890, 164057, 164195, 180254, 180802, 180809, 181173, 181174, 181177, 181181,
            181182, 181185, 181290, 181292, 181306, 181309, 181313, 181326, 181563, 181857, 181858, 187265, 191433, 191462, 191641, 192880, 192881, 196413, 196435,
            197280, 199191, 199264, 199274, 199597, 199664, 200979, 200982, 201236, 201580, 201581, 201583, 204183, 205746, 205756, 210087, 213907, 218228, 219223,
            220114, 3011, 3205, 3539, 3582, 3584, 3595, 3600, 4580, 52693, 5466, 55005, 5509, 62522, 
            205756, 5509, 200371, 167185, 181195, 217346, 178161, 60108, 
            // rrrix act 3
            182443, 211456,
            // uber fire chains in Realm of Turmoil and Iron Gate in Realm of Chaos
            263014, 
            5724, 5727, 
            5869, // trDun_Gargoyle_01
            5738, // trDun_Cath_Breakable_pillar

            105478, // a1dun_Leor_Spike_Spawner_Switch
            /*
             * A5
             */

            // Pandemonium Fortress
            357297, // X1_Fortress_Rack_C
            //374196, // X1_Fortress_Rack_C_Stump
            357295, // X1_Fortress_Rack_B
            //374195, // X1_Fortress_Rack_B_Stump
            357299, // X1_Fortress_Rack_D
            //374197, // X1_Fortress_Rack_D_Stump
            357301, // X1_Fortress_Rack_E
            //374198, // X1_Fortress_Rack_E_Stump
            357306, // X1_Fortress_Rack_F
            //374199, // X1_Fortress_Rack_F_Stump
            365503, // X1_Fortress_FloatRubble_A
            365562, // X1_Fortress_FloatRubble_B
            365580, // X1_Fortress_FloatRubble_C
            365602, // X1_Fortress_FloatRubble_D
            365611, // X1_Fortress_FloatRubble_E
            365739, // X1_Fortress_FloatRubble_F

            355365, // x1_Abattoir_furnaceWall

            304313, // x1_abattoir_furnace_01 
            375383, // x1_Abattoir_furnaceSpinner_Event_Phase2 -- this is a rotating avoidance, with a fire "beam" about 45f in length

            265637, // x1_Catacombs_Weapon_Rack_Raise

            321479, // x1_Westm_HeroWorship03_VO

            328008, // X1_Westm_Door_Giant_Closed
            312441, // X1_Westm_Door_Giant_Opening_Event

            328942, // x1_Pand_Ext_ImperiusCharge_Barricade 
            324867, // x1_Westm_DeathOrb_Caster_TEST
            313302, // X1_Westm_Breakable_Wolf_Head_A

            368268, // x1_Urzael_SoundSpawner
            368626, // x1_Urzael_SoundSpawner
            368599, // x1_Urzael_SoundSpawner
            368621, // x1_Urzael_SoundSpawner

            377253, // x1_Fortress_Crystal_Prison_Shield
            316221, // X1_WarpToPortal 
            370187, // x1_Malthael_Boss_Orb_Collapse
            328830, // x1_Fortress_Portal_Switch
            374174, // X1_WarpToPortal

            356639, // x1_Catacombs_Breakable_Window_Relief

            //x1_Westm_HeroWorship01_VO Dist=10 IsElite=False LoS=True HP=1,00 Dir=SW - Name=x1
            //It's trying to attack that in town
            //x1_Westm_HeroWorship01_VO = 321451,
            //x1_Westm_HeroWorship02_VO = 321454,
            //x1_Westm_HeroWorship03_VO = 321479,
            321451, 321454, 321479

        };

        /// <summary>
        /// A list of LevelAreaId's that the bot should always ignore Line of Sight
        /// </summary>
        public static HashSet<int> NeverRaycastLevelAreaIds { get { return neverRaycastLevelAreaIds; } }
        private static readonly HashSet<int> neverRaycastLevelAreaIds = new HashSet<int>()
        {
            405915, // p1_TieredRift_Challenge 
        };


        public static HashSet<int> AlwaysRaycastWorlds { get { return DataDictionary.alwaysRaycastWorlds; } }
        private static readonly HashSet<int> alwaysRaycastWorlds = new HashSet<int>()
        {
            271233, // Pandemonium Fortress 1
            271235, // Pandemonium Fortress 2
        };



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
            };

        public static HashSet<int> ForceUseWOTBIds { get { return DataDictionary.forceUseWOTBIds; } }
        private static readonly HashSet<int> forceUseWOTBIds = new HashSet<int>()
        {
            256015, 256000, 255996
        };

        public static HashSet<int> IgnoreUntargettableAttribute { get { return DataDictionary.ignoreUntargettableAttribute; } }
        private static readonly HashSet<int> ignoreUntargettableAttribute = new HashSet<int>()
        {
            5432, // A2 Snakem
        };

        public static HashSet<string> WhiteItemCraftingWhiteList { get { return whiteItemCraftingWhiteList; } }

        private static HashSet<string> whiteItemCraftingWhiteList = new HashSet<string>()
        {
            "Ascended Pauldrons",
            "Ascended Armor",
            "Ascended Bracers",
            "Ascended Crown",
            "Ascended Pauldrons",
            "Archon Sash",
            "Ascended Faulds",
            "Ascended Greaves",
            "Ascended Gauntlets",

            "Limb Cleaver",
            "Doubleshot",
            "Whirlwind Staff",
            "Flesh Render",
            "Penetrator",
            "Ascended Shield",
            "Punyal",
            "Dire Axe",
            "Tsunami Blade",
            "Kerykeion",
            "Steppes Smasher",
            "Grandfather Flail",
            "Oxybeles",
            "Persuader",
            "Skullsplitter",
            "Suwaiya",
            "Tecpatl",
            "Diabolic Wand"
        };
        // Chamber of Suffering (Butcher's Lair)
        public static HashSet<Vector3> ChamberOfSufferingSafePoints = new HashSet<Vector3>
        {
            new Vector3(122.3376f, 120.1721f, 0), // Center
            new Vector3(138.504f,  88.64854f, 0), // Top Left
            new Vector3(98.61596f, 95.93278f, 0), // Top
            new Vector3(93.04589f, 134.9459f, 0), // Top Right
            new Vector3(107.9791f, 150.6952f, 0), // Bottom Right
            new Vector3(146.8563f, 144.0836f, 0), // Bottom
            new Vector3(151.9562f, 104.8417f, 0), // Bottom Left
        };

        public static HashSet<int> TownLevelAreaIds = new HashSet<int>()
        {
            332339,168314,92945,270011        
        };

        public static Dictionary<int, Vector3> ButcherPanelPositions = new Dictionary<int, Vector3>
        {
            { 201426, new Vector3(121, 121, 0)}, // ButcherLair_FloorPanel_MidMiddle_Base
            { 201242, new Vector3(158, 111, 0)}, // ButcherLair_FloorPanel_LowerLeft_Base
            { 200969, new Vector3(152, 151, 0)}, // ButcherLair_FloorPanel_LowerMid_Base
            { 201438, new Vector3(91, 91, 0)}, // ButcherLair_FloorPanel_UpperMid_Base
            { 201423, new Vector3(133, 78, 0)}, // ButcherLair_FloorPanel_UpperLeft_Base
            { 201464, new Vector3(107, 160, 0)}, // ButcherLair_FloorPanel_LowerRight_Base
            { 201454, new Vector3(80, 134, 0)}, // ButcherLair_FloorPanel_UpperRight_Base
        };

        public readonly static Dictionary<Item, SNOPower> PowerByItem = new Dictionary<Item, SNOPower>
        {
            { Legendary.HarringtonWaistguard, SNOPower.ItemPassive_Unique_Ring_685_x1 },
            { Legendary.PoxFaulds, SNOPower.itemPassive_Unique_Pants_007_x1 },
            { Legendary.RechelsRingOfLarceny, SNOPower.ItemPassive_Unique_Ring_508_x1 },
            //{ Legendary.BottomlessPotionofKulleAid, SNOPower.X1_Legendary_Potion_06 },
            //{ Legendary.PridesFall, SNOPower.ItemPassive_Unique_Helm_017_x1 },
            { Legendary.KekegisUnbreakableSpirit, SNOPower.ItemPassive_Unique_Ring_569_x1 },
        };

        public readonly static Dictionary<Item, string> MinionInternalNameTokenByItem = new Dictionary<Item, string>
        {
            { Legendary.Maximus, "DemonChains_ItemPassive" },
            { Legendary.HauntOfVaxo, "_shadowClone_" }
        };

        public readonly static Dictionary<Skill, Set> AllRuneSetsBySkill = new Dictionary<Skill, Set>
        {
            { Skills.Barbarian.FuriousCharge, Sets.TheLegacyOfRaekor },
            { Skills.Wizard.Archon, Sets.VyrsAmazingArcana },
        };

        public readonly static HashSet<TrinityObjectType> InteractableTypes = new HashSet<TrinityObjectType>
        {
            TrinityObjectType.Item,
            TrinityObjectType.Door,
            TrinityObjectType.Container,
            TrinityObjectType.HealthWell,  
            TrinityObjectType.CursedChest,
            TrinityObjectType.Interactable,
            TrinityObjectType.Shrine,
            TrinityObjectType.CursedShrine               
        };

        public readonly static HashSet<TrinityObjectType> DestroyableTypes = new HashSet<TrinityObjectType>
        {
            TrinityObjectType.Barricade,
            TrinityObjectType.Destructible,
            TrinityObjectType.HealthGlobe,
            TrinityObjectType.ProgressionGlobe                        
        };

        public readonly static HashSet<TrinityObjectType> NoPickupClickTypes = new HashSet<TrinityObjectType>
        {
            TrinityObjectType.Gold,
            TrinityObjectType.PowerGlobe,
            TrinityObjectType.HealthGlobe,
            TrinityObjectType.ProgressionGlobe                     
        };

        public readonly static HashSet<SNOActor> Shrines = new HashSet<SNOActor>
        {
            SNOActor.Shrine_Global_Fortune,
            SNOActor.Shrine_Global_Blessed,
            SNOActor.Shrine_Global_Frenzied,
            SNOActor.Shrine_Global_Reloaded,                     
            SNOActor.Shrine_Global_Enlightened,    
            SNOActor.Shrine_Global_Hoarder,    
            SNOActor.x1_LR_Shrine_Infinite_Casting,    
            SNOActor.x1_LR_Shrine_Electrified,    
            SNOActor.x1_LR_Shrine_Invulnerable,    
            SNOActor.x1_LR_Shrine_Run_Speed,    
            SNOActor.x1_LR_Shrine_Damage 
        };

        internal static HashSet<string> ActorIgnoreNames = new HashSet<string>
        {
            "MarkerLocation", 
            "Generic_Proxy", 
            "Hireling", 
            "Start_Location", 
            "SphereTrigger", 
            "Checkpoint", 
            "ConductorProxyMaster", 
            "BoxTrigger", 
            "SavePoint", 
            "TriggerSphere", 
            "minimapicon", 
        };

        internal static HashSet<string> ActorIgnoreNameParts = new HashSet<string>
        {
            "markerlocation", 
            "start_location", 
            "checkpoint", 
            "savepoint", 
            "triggersphere", 
            "minimapicon", 
            "proxy",
            "invisbox",
            "trigger",
            "invisible"
        };

        /// <summary>
        /// ActorIds that should be ignored
        /// </summary>
        internal static HashSet<int> ActorIgnoreIds = new HashSet<int>
        {
            0000, 
        };

        internal static HashSet<SNOAnim> ActorChargeAnimations = new HashSet<SNOAnim>
        {
            SNOAnim.Beast_start_charge_02, 
            SNOAnim.Beast_charge_02, 
            SNOAnim.Beast_charge_04, 
            SNOAnim.Butcher_Attack_Charge_01_in, 
            SNOAnim.Butcher_Attack_Chain_01_out,
            SNOAnim.Butcher_Attack_05_telegraph, 
            SNOAnim.Butcher_Attack_Chain_01_in, 
            SNOAnim.Butcher_BreakFree_run_01,  
        };

        /// <summary>
        /// Actor types that we dont wan't to even look at from DB's ACD List.
        /// </summary>
        public static HashSet<ActorType> ExcludedActorTypes = new HashSet<ActorType>
        {
            ActorType.Environment,
            ActorType.ClientEffect,
            ActorType.AxeSymbol,
            ActorType.CustomBrain,
            ActorType.Invalid,
            ActorType.ServerProp,
            ActorType.Critter
        };

        /// <summary>
        /// ActorSNO that we want to completely ignore
        /// </summary>
        public static HashSet<int> ExcludedActorIds = new HashSet<int>
        {
            -1, 
            4176, // Generic Proxy
            5502, // Start Location
            375658, // Banter Trigger
            3462, // Box Trigger
            5466, // Sphere Trigger
            3461 // OneShot Box Trigger
        };

        public static HashSet<MonsterType> NonHostileMonsterTypes = new HashSet<MonsterType>
        {
            MonsterType.Ally,
            MonsterType.Scenery,
            MonsterType.Team,
            MonsterType.None,
            MonsterType.Helper,
        };

        public static Dictionary<int, MonsterType> MonsterTypeOverrides = new Dictionary<int, MonsterType>
        {
            { 86624, MonsterType.Undead }, // Jondar, DB thinks he's permanent ally
        };

        #region Methods

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
        #endregion
    }
}
