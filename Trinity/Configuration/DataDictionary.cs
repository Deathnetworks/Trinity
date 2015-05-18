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
    internal partial class DataDictionary
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
            //ActorType.ServerProp,
            //ActorType.Player,
            //ActorType.Projectile, //lots of avoidance are classified as projectile
            ActorType.Critter,
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
            3461, // OneShot Box Trigger
            //6442, // Waypoint
            3795, // Checkpoint
            5992, // OneShot Trigger Sphere
            180941 // SavePoint
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

        public static HashSet<int> AvoidanceBuffSNO = new HashSet<int>
        {
            201454,
            201464,
            201426,
            201438,
            200969,
            201423,
            201242,
        };

        public static HashSet<int> WhirlWindIgnoreSNO = new HashSet<int>
        {
            4304,
            5984,
            5985,
            5987,
            5988,
        };

        public static HashSet<int> FastMonsterSNO = new HashSet<int>
        {
            5212
        };

        public static Dictionary<int, AvoidanceType> AvoidanceTypeSNO = new Dictionary<int, AvoidanceType>
        {
            {349774, AvoidanceType.FrozenPulse},
            {343539, AvoidanceType.Orbiter},
            {316389, AvoidanceType.PoisonEnchanted},
            {340319, AvoidanceType.PoisonEnchanted},
            {341512, AvoidanceType.Thunderstorm},
            {337109, AvoidanceType.Wormhole},
            {123839, AvoidanceType.AzmodanBody},
            {123124, AvoidanceType.AzmodanPool},
            {123842, AvoidanceType.AzmoFireball},
            {219702, AvoidanceType.Arcane},
            {221225, AvoidanceType.Arcane},
            {3337, AvoidanceType.BeastCharge},
            {5212, AvoidanceType.BeeWasp},
            {161822, AvoidanceType.Belial},
            {161833, AvoidanceType.Belial},
            {201454, AvoidanceType.ButcherFloorPanel},
            {201464, AvoidanceType.ButcherFloorPanel},
            {201426, AvoidanceType.ButcherFloorPanel},
            {201438, AvoidanceType.ButcherFloorPanel},
            {200969, AvoidanceType.ButcherFloorPanel},
            {201423, AvoidanceType.ButcherFloorPanel},
            {201242, AvoidanceType.ButcherFloorPanel},
            {226350, AvoidanceType.DiabloRingOfFire},
            {226525, AvoidanceType.DiabloRingOfFire},
            {84608, AvoidanceType.Desecrator},
            {93837, AvoidanceType.GhomGas},
            {3847, AvoidanceType.Grotesque},
            {168031, AvoidanceType.DiabloPrison},
            {214845, AvoidanceType.DiabloMeteor},
            {402, AvoidanceType.IceBall},
            {223675, AvoidanceType.IceBall},
            {260377, AvoidanceType.IceTrail},
            {432, AvoidanceType.MageFire},
            {166686, AvoidanceType.MaghdaProjectille},
            {160154, AvoidanceType.MoltenBall},
            {4803, AvoidanceType.MoltenCore},
            {4804, AvoidanceType.MoltenCore},
            {224225, AvoidanceType.MoltenCore},
            {247987, AvoidanceType.MoltenCore},
            {95868, AvoidanceType.MoltenTrail},
            {250031, AvoidanceType.Mortar},
            {108869, AvoidanceType.PlagueCloud},
            {3865, AvoidanceType.PlagueHand},
            {5482, AvoidanceType.PoisonTree},
            {6578, AvoidanceType.PoisonTree},
            {4103, AvoidanceType.ShamanFire},
            {185924, AvoidanceType.ZoltBubble},
            {139741, AvoidanceType.ZoltTwister}
        };

        public static Dictionary<int, AvoidanceType> AvoidanceProjectileSNO = new Dictionary<int, AvoidanceType>
        {
            { 343539, AvoidanceType.Orbiter },
            { 316389, AvoidanceType.PoisonEnchanted },
            { 340319, AvoidanceType.PoisonEnchanted },
            { 5212, AvoidanceType.BeeWasp },
            { 4103, AvoidanceType.ShamanFire },
            { 160154, AvoidanceType.MoltenBall },
            { 123842, AvoidanceType.AzmoFireball },
            { 139741, AvoidanceType.ZoltTwister },
            { 166686, AvoidanceType.MaghdaProjectille },
            { 185999, AvoidanceType.DiabloRingOfFire },
            { 196526, AvoidanceType.DiabloRingOfFire },
            { 136533, AvoidanceType.DiabloLightning },
        };

        public static HashSet<int> GoldSNO = new HashSet<int>
        {
            //GoldCoin
            376,
            //GoldLarge
            4311,
            //GoldMedium
            4312,
            //GoldSmall
            4313,
            //PlacedGold
            166389,
            //GoldCoins
            209200,
        };

        public static HashSet<int> HealthGlobeSNO = new HashSet<int>
        {
            //HealthGlobe
            4267,
            //HealthGlobe_02
            85798,
            //healthGlobe_swipe
            85816,
            //HealthGlobe_03
            209093,
            //HealthGlobe_04
            209120,
            //X1_NegativeHealthGlobe
            333196,
            //x1_healthGlobe
            366139,
            //x1_healthGlobe_playerIsHealed_attract
            367978,
            //HealthGlobe_steak
            375124,
            //HealthGlobe_steak_02
            375125,
            //x1_healthGlobe_steak_model
            375132,	
        };

        public static HashSet<int> HealthWellSNO = new HashSet<int>
        {
            //caOut_Healthwell
            3648,
            //HealthWell_Global
            138989,
            //HealthWell_Water_Plane
            139129,
            //a4_Heaven_HealthWell_Global
            218885,
            //a4dun_DIablo_Arena_Health_Well
            180575,
        };

        public static HashSet<int> ShrineSNO = new HashSet<int>
        {
            //shrine_fxSphere_corrupt
            5333,
            //Shrine_Global
            135384,
            //shrine_fxGeo_model_Global
            139931,
            //Shrine_Global_Glow
            156680,
            //Shrine_Global_Blessed
            176074,
            //Shrine_Global_Enlightened
            176075,
            //Shrine_Global_Fortune
            176076,
            //Shrine_Global_Frenzied
            176077,
            //a4_Heaven_Shrine_Global_Blessed
            225025,
            //a4_Heaven_Shrine_Global_Fortune
            225027,
            //a4_Heaven_Shrine_Global_Frenzied
            225028,
            //a4_Heaven_Shrine_Global_Enlightened
            225030,
            //a4_Heaven_Shrine_Global_DemonCorrupted_Blessed
            225261,
            //a4_Heaven_Shrine_Global_DemonCorrupted_Enlightened
            225262,
            //a4_Heaven_Shrine_Global_DemonCorrupted_Fortune
            225263,
            //a4_Heaven_Shrine_Global_DemonCorrupted_Frenzied
            225266,
            //a4_Heaven_Shrine_Global_DemonCorrupted_Hoarder
            260342,
            //a4_Heaven_Shrine_Global_DemonCorrupted_Reloaded
            260343,
            //a4_Heaven_Shrine_Global_Hoarder
            260344,
            //a4_Heaven_Shrine_Global_Reloaded
            260345,
            //Shrine_Global_Hoarder
            260346,
            //Shrine_Global_Reloaded
            260347,
            //PVP_Shrine_Murderball
            275729,
            //x1_LR_Shrine_Damage
            330695,
            //x1_LR_Shrine_Electrified
            330696,
            //x1_LR_Shrine_Infinite_Casting
            330697,
            //x1_LR_Shrine_Invulnerable
            330698,
            //x1_LR_Shrine_Run_Speed
            330699,
            //x1_Event_CursedShrine
            364601,
            //x1_Event_CursedShrine_Heaven
            368169,
            //x1_player_isShielded_riftShrine_model
            369696,
            //x1_LR_Shrine_Electrified_TieredRift
            398654,
            //shrine_Shadow
            434722,
        };

        public static HashSet<int> CursedShrineSNO = new HashSet<int>
        {
            //x1_Event_CursedShrine
            364601,
            //x1_Event_CursedShrine_Heaven
            368169,
        };

        public static HashSet<int> CursedChestSNO = new HashSet<int>
        {
            //x1_Global_Chest_CursedChest
            364559,
            //x1_Global_Chest_CursedChest_B
            365097,
            //x1_Global_Chest_CursedChest_B_MutantEvent
            374391,
        };

        public static HashSet<int> AvoidanceSNO = new HashSet<int>
        {
            //monsterAffix_Frozen_deathExplosion_Proxy
            402,
            //monsterAffix_Molten_deathStart_Proxy
            4803,
            //monsterAffix_Molten_deathExplosion_Proxy
            4804,
            //monsterAffix_Electrified_deathExplosion_proxy
            4806,
            //monsterAffix_Desecrator_telegraph
            84606,
            //monsterAffix_Desecrator_damage_AOE
            84608,
            //monsterAffix_Vortex_proxy
            85809,
            //monsterAffix_Vortex_model
            89862,
            //monsterAffix_Molten_trail
            95868,
            //monsterAffix_healthLink_jumpActor
            98220,
            //monsterAffix_Plagued_endCloud
            108869,
            //monsterAffix_frenzySwipe
            143266,
            //monsterAffix_vortex_target_trailActor
            210407,
            //monsterAffix_missileDampening_shield_add
            219458,
            //MonsterAffix_ArcaneEnchanted_PetSweep
            219702,
            //monsterAffix_missileDampening_outsideGeo
            220191,
            //MonsterAffix_ArcaneEnchanted_PetSweep_reverse
            221225,
            //MonsterAffix_ArcaneEnchanted_Proxy
            221560,
            //MonsterAffix_ArcaneEnchanted_trailActor
            221658,
            //monsterAffix_frozen_iceClusters
            223675,
            //monsterAffix_plagued_groundGeo
            223933,
            //monsterAffix_molten_fireRing
            224225,
            //monsterAffix_waller_wall
            226296,
            //monsterAffix_Avenger_glowSphere
            226722,
            //monsterAffix_ghostly_distGeo
            226799,
            //monsterAffix_waller_model
            226808,
            //monsterAffix_invulnerableMinion_distGeo
            227697,
            //monsterAffix_linked_chainHit
            228275,
            //monsterAffix_entangler_ringGlow_geo
            228885,
            //monsterAffix_molten_bomb_buildUp_geo
            247980,
            //monsterAffix_invulnerableMinion_colorGeo
            248043,
            //MonsterAffix_Mortar_Pending
            250031,
            //x1_MonsterAffix_CorpseBomber_projectile
            316389,
            //X1_MonsterAffix_corpseBomber_bomb
            325761,
            //X1_MonsterAffix_LightningStorm_Wanderer
            328307,
            //X1_MonsterAffix_TeleportMines
            337109,
            //x1_MonsterAffix_CorpseBomber_bomb_start
            340319,
            //x1_MonsterAffix_Thunderstorm_Impact
            341512,
            //X1_MonsterAffix_Orbiter_Projectile
            343539,
            //X1_MonsterAffix_Orbiter_FocalPoint
            343582,
            //x1_Spawner_Skeleton_MonsterAffix_World_1
            345764,
            //x1_Spawner_Skeleton_MonsterAffix_World_2
            345765,
            //x1_Spawner_Skeleton_MonsterAffix_World_3
            345766,
            //x1_Spawner_Skeleton_MonsterAffix_World_4
            345767,
            //x1_Spawner_Skeleton_MonsterAffix_World_5
            345768,
            //x1_MonsterAffix_orbiter_projectile_orb
            346805,
            //x1_MonsterAffix_orbiter_projectile_focus
            346837,
            //x1_MonsterAffix_orbiter_glowSphere
            346839,
            //x1_MonsterAffix_frozenPulse_monster
            349774,
            //x1_MonsterAffix_frozenPulse_shard
            349779,
            //x1_monsteraffix_mortar_blastwave
            365830,
            //x1_MonsterAffix_frozenPulse_shard_search
            366924,
            //x1_monsterAffix_generic_coldDOT_runeGeo
            377326,
            //x1_monsterAffix_generic_coldDOT_rune_emitter
            377374,
            //MonsterAffix_Avenger_ArcaneEnchanted_PetSweep
            384431,
            //MonsterAffix_Avenger_ArcaneEnchanted_PetSweep_reverse
            384433,
            //X1_MonsterAffix_Avenger_Orbiter_Projectile
            384575,
            //X1_MonsterAffix_Avenger_Orbiter_FocalPoint
            384576,
            //x1_MonsterAffix_Avenger_CorpseBomber_bomb_start
            384614,
            //x1_MonsterAffix_Avenger_CorpseBomber_projectile
            384617,
            //x1_MonsterAffix_Avenger_frozenPulse_monster
            384631,
            //x1_MonsterAffix_Avenger_arcaneEnchanted_dummySpawn
            386997,
            //x1_MonsterAffix_Avenger_ArcaneEnchanted_trailActor
            387010,
            //x1_MonsterAffix_Avenger_orbiter_projectile_orb
            387679,
            //x1_MonsterAffix_Avenger_orbiter_projectile_focus
            388435,
            //x1_MonsterAffix_Avenger_corpseBomber_slime
            389483,    

	        //x1_Bog_bloodSpring_medium
            332922,
	        //x1_Bog_bloodSpring_large
            332923,
	        //x1_Bog_bloodSpring_small
            332924,

            //p4_RatKing_RatBallMonster
            427170
        };

        public static HashSet<int> PlayerBannerSNO = new HashSet<int>
        {
            //Banner_Player_1
            123714,
            //Banner_Player_2
            123715,
            //Banner_Player_3
            123716,
            //Banner_Player_4
            123717,
            //Banner_Player_1_Act2
            212879,
            //Banner_Player_2_Act2
            212880,
            //Banner_Player_3_Act2
            212881,
            //Banner_Player_4_Act2
            212882,
            //Banner_Player_1_Act5
            367451,
            //Banner_Player_2_Act5
            367452,
            //Banner_Player_3_Act5
            367453,
            //Banner_Player_4_Act5
            367454,
        };

        public static HashSet<int> PlayerSNO = new HashSet<int>
        {
            //Wizard_Female
            6526,
            //Wizard_Female_characterSelect
            6527,
            //Wizard_Male
            6544,
            //Wizard_Male_characterSelect
            6545,
            //Wizard_Male_FrontEnd
            218883,
            //Wizard_Female_FrontEnd
            218917,
            //Barbarian_Female
            3285,
            //Barbarian_Female_characterSelect
            3287,
            //Barbarian_Male
            3301,
            //Barbarian_Male_characterSelect
            3302,
            //Barbarian_Male_FrontEnd
            218882,
            //Barbarian_Female_FrontEnd
            218909,
            //Demonhunter_Female_FrontEnd
            218911,
            //Demonhunter_Male_FrontEnd
            218912,
            //Demonhunter_Female
            74706,
            //Demonhunter_Male
            75207,
            //X1_Crusader_Male
            238284,
            //X1_Crusader_Female
            238286,
            //X1_Crusader_Male_FrontEnd
            238287,
            //X1_Crusader_Female_FrontEnd
            238288,
            //Crusader_Female_characterSelect
            279361,
            //Crusader_Male_characterSelect
            279362,
            //WitchDoctor_Male
            6485,
            //WitchDoctor_Male_characterSelect
            6486,
            //WitchDoctor_Female
            6481,
            //WitchDoctor_Female_characterSelect
            6482,
            //Monk_Male
            4721,
            //Monk_Male_characterSelect
            4722,
            //Monk_Female
            4717,
            //Monk_Female_characterSelect
            4718,
        };

        public static HashSet<int> SummonerSNO = new HashSet<int>
        {
            //TriuneSummoner_fireBall_obj
            467,
            //SkeletonSummoner_A
            5387,
            //SkeletonSummoner_B
            5388,
            //SkeletonSummoner_C
            5389,
            //SkeletonSummoner_D
            5390,
            //TriuneSummoner_A
            6035,
            //TriuneSummoner_B
            6036,
            //TriuneSummoner_C
            6038,
            //TriuneSummoner_D
            6039,
            //SkeletonSummoner_A_TemplarIntro
            104728,
            //TownAttack_TriuneSummonerBoss_C
            105539,
            //triuneSummoner_summonRope_glow
            111554,
            //TriuneSummoner_B_RabbitHoleEvent
            111580,
            //Spawner_SkeletonSummoner_A_Immediate_Chand
            117947,
            //TriuneSummoner_A_Unique_SwordOfJustice
            131131,
            //a4_heaven_hellportal_summoner_loc
            143502,
            //TownAttack_SummonerSpawner
            173527,
            //TownAttack_Summoner
            178297,
            //TownAttack_Summoner_Unique
            178619,
            //SkeletonSummoner_E
            182279,
            //TriuneSummoner_A_CainEvent
            186039,
            //TriuneSummoner_A_Unique_01
            218662,
            //TriuneSummoner_A_Unique_02
            218664,
            //TriuneSummoner_C_Unique_01
            222001,
            //x1_TriuneSummoner_WestMCultist
            288215,
            //x1_Spawner_TriuneSummoner_A
            290730,
            //x1_Spawner_SkeletonSummoner_A
            292297,
            //x1_westm_Soul_Summoner
            298827,
            //x1_westm_Soul_Summoner_Hands
            301425,
            //x1_Spawner_SkeletonSummoner_D
            303859,
            //x1_TEST_FallenChampion_LR_Summoner
            308285,
            //x1_westm_Soul_Summoner_Spawner
            308823,
            //x1_soul_summoner_hands_trail
            316395,
            //x1_westm_Soul_Summoner_twoHands
            316560,
            //x1_Soul_Summoner_glowSphere
            316716,
            //x1_westm_Soul_Summoner_GhostChase
            330609,
            //x1_devilshand_unique_SkeletonSummoner_B
            332432,
            //x1_devilshand_unique_TriuneSummoner_C
            332433,
            //x1_TriuneSummoner_C_Unique_01
            341240,
            //X1_LR_Boss_SkeletonSummoner_C
            359094,
            //x1_Heaven_Soul_Summoner
            361480,
            //TriuneSummoner_Unique_Cultist_Leader_Hershberg
            396812,
            //TriuneSummoner_Unique_Cultist_Leader_Son_of_Jacob
            396836,
            //TriuneSummoner_Unique_Cultist_Leader_Poirier
            396849,
            //TriuneSummoner_Unique_Cultist_Leader_Buckley
            396863,
        };


        public static HashSet<int> SpawnerSNO = new HashSet<int>
        {
            //BlizzCon_KingGhost_Spawner
            327,
            //Spawner_Triune_Cultist_D
            436,
            //A1C4CultistSpawner
            2876,
            //A1C5RFarmerScavengerSpawner
            2902,
            //A2C2GreedyMinerFallenSpawner
            2926,
            //A2C2RAdventurerFallenBossSpawner
            2936,
            //A2C2RAdventurerFallenSpawner
            2937,
            //a2dun_GhoulSpawner01
            2955,
            //a2dun_Swr_Arch_Spawner
            2960,
            //a2dun_Zolt_Round_Spawner
            3037,
            //a2dun_Zolt_Round_Spawner_Portal
            3038,
            //a2dun_Zolt_Round_Spawner_Portal_Black
            3039,
            //a2dun_Zolt_Round_Spawner_SandSwirl
            3040,
            //a2dun_Zolt_Round_Spawner_SandSwirl_Reverse
            3041,
            //Encounter_Spawner_Major
            4040,
            //fastMummy_Spawner_A
            4109,
            //fastMummy_Spawner_B
            4110,
            //FleshPitFlyerSpawner_A
            4152,
            //FleshPitFlyerSpawner_B
            4153,
            //FleshPitFlyerSpawner_C
            4154,
            //FleshPitFlyerSpawner_D
            4155,
            //pvpHealthSpawner
            4921,
            //Siegebreaker_Skeleton_Spawner
            5339,
            //Skeleton_Spawner_Burrow
            5409,
            //Spawner_DuneDervish_A_Immediately
            5442,
            //Spawner_FastMummy_Climb_A
            5444,
            //Spawner_FastMummy_Climb_B
            5445,
            //Spawner_Lacuni_Female_A
            5447,
            //Spawner_sandWasp_A
            5449,
            //Spawner_Triune_Summonable_D
            5450,
            //trDun_Cath_FloorSpawner_01
            5758,
            //trDun_Cath_FloorSpawner_02
            5759,
            //trDun_Crypt_Pillar_Spawner
            5840,
            //trDun_Crypt_Pillar_Spawner_Crack_Debris
            5841,
            //trDun_Crypt_Pillar_Spawner_Energy_Planes
            5843,
            //trDun_Crypt_Pillar_Spawner_E_Planes_End
            5844,
            //trDun_Crypt_Pillar_Spawner_E_Planes_Start
            5845,
            //trDun_Crypt_Pillar_Spawner_Final_Debris
            5846,
            //trDun_Crypt_Pillar_Spawner_Panel_Cracks
            5847,
            //trDun_Crypt_Pillar_Spawner_Panel_Cracks2
            5848,
            //trDun_GhoulSpawner01
            5875,
            //trDun_GhoulSpawner03
            5877,
            //trDun_RescueCainSkelSpawner
            5913,
            //trDun_SkeletonSpawner_WallJump_01
            5951,
            //SandMonster_spawner
            52799,
            //Spawner_Skeleton_A
            54551,
            //Spawner_Skeleton_B
            54552,
            //Spawner_Skeleton_C
            54553,
            //Spawner_Skeleton_D
            54554,
            //Spawner_Shield_Skeleton_A
            54555,
            //Spawner_Shield_Skeleton_C
            54557,
            //Spawner_Skeleton_TwoHander_B
            54560,
            //Spawner_SkeletonArcher_B
            54564,
            //Spawner_SkeletonArcher_D
            54566,
            //Spawner_SkeletonMage_Cold_A
            54571,
            //Spawner_SkeletonMage_Cold_B
            54572,
            //Spawner_SkeletonMage_Fire_B
            54574,
            //Spawner_SkeletonMage_Lightning_B
            54576,
            //a2dun_Swr_Arch_Spawner_CryptChild
            54739,
            //a2dun_Swr_Arch_Spawner_CryptChild_01
            55005,
            //WaterloggedCorpse_SwarmSpawner_A_01
            55258,
            //WaterloggedCorpse_TreasureSpawner_A_01
            55259,
            //a3dun_Keep_Skeleton_Spawner
            55659,
            //Crypt_Endless_Spawner_A_Door_01
            56988,
            //Crypt_Endless_Spawner_A_Base_01
            57157,
            //Spawner_Swarm_A
            57356,
            //Crypt_Endless_Spawner
            57736,
            //WaterloggedCorpse_EelSpawner_A_01
            57930,
            //Ghost_C_GhostTower_Spawner
            58899,
            //Spawner_Zombie_A
            60033,
            //Spawner_Zombie_B
            60034,
            //Spawner_ZombieCrawler_B
            60061,
            //Spawner_ZombieFemale_A
            60064,
            //Spawner_ZombieSkinny_A
            60068,
            //Spawner_ZombieSkinny_B
            60069,
            //Spawner_FleshPitFlyer_A
            60158,
            //Spawner_FleshPitFlyer_B_Immediate
            60159,
            //Spawner_Goatman_Melee_A
            63127,
            //Spawner_SandMonster_D
            64053,
            //Spawner_Goatman_Melee_A1
            66963,
            //Spawner_Goatman_Melee_A3
            66964,
            //Spawner_Goatman_Range_A1
            66966,
            //Spawner_Goatman_Range_A3
            66967,
            //Spawner_Goatman_Range_A2
            66968,
            //Spawner_ZombieSkinny_A_ShortClimb
            69730,
            //a2dun_zolt_smallFloorSpawner_emitter
            71324,
            //Spawner_SkeletonMage_Cold_B_Hologram
            73041,
            //Spawner_SkeletonMage_Fire_B_Hologram
            73043,
            //Spawner_SkeletonMage_Lightning_B_Hologram
            73098,
            //Spawner_SkeletonMage_Poison_B_Hologram
            73123,
            //spawner_zolt_centerpiece
            74187,
            //Spawner_ZombieCrawler_Custom_A3
            76857,
            //Spawner_Skeleton_Climb_From_Under
            77382,
            //Spawner_Goatman_Melee_B5
            77702,
            //Spawner_Goatman_Range_B3
            77704,
            //a2dun_Spider_Egg_Spawner
            79158,
            //Spawner_Triune_Summonable_B
            79414,
            //Event_Tower_Of_Power_Spawner1
            80206,
            //Spawner_ScavengerA_Burrow
            81162,
            //trDun_RescueCainSkelSpawner_01
            81418,
            //trDun_RescueCainSkelSpawner_02
            81419,
            //trDun_RescueCainSkelSpawner_03
            81443,
            //Event_Gharbad_The_Weak_Spawner
            81551,
            //FleshPitFlyerSpawner_B_Event_FarmAmbush
            81982,
            //trDun_Skeleton_B_Spawner_WallJump_01
            85830,
            //Spawner_Ghost_A_Immediate
            85973,
            //trOut_Wilderness_Coffin_Spawner
            87430,
            //Spawner_ZombieSkinny_A_Immediate
            89957,
            //TownAttack_CultistSpawnerMelee
            90007,
            //CrownAttack_CultistSpawner
            92530,
            //Spawner_Shape_Skeleton_A_WallJump
            93410,
            //ZombieTorso_Spawner_1
            93424,
            //a1Dun_InfernoZombie_WallSpawner
            93486,
            //Spawner_Siege_wallMonster_A
            93499,
            //ShadowVermin_A_Spawner
            94954,
            //Belial_ProxyHealthSpawner
            95821,
            //FastMummySpawner_Gibs
            96347,
            //a3_demon_trooper_climb_spawner
            96764,
            //Spawner_Leor_Iron_Maiden
            100956,
            //Spawner_Spider_A
            102135,
            //trDun__JailGhoulSpawner01
            104751,
            //Spawner_Skeleton_A_TemplarIntro
            104764,
            //a1dun_Leor_Spike_Spawner_Chain
            104894,
            //a1dun_Leor_Spike_Spawner_Switch
            105478,
            //Spawner_Leor_Iron_Maiden_Event
            105619,
            //a3_ramparts_ambush_demon_climb_spawner
            106125,
            //a3_ramparts_ambush_demon_flyer_spawner
            106383,
            //Wilderness_Coffin_Spawner
            106649,
            //Spawner_Skeleton_A_HangingTree2
            106732,
            //WaterWheel_FallenSpawner
            106833,
            //Spawner_Zombie_B_ShortClimb
            107169,
            //a1dun_Leor_Door_FireZombie_Spawner_A
            108237,
            //Spawner_Immediately_WitherMoth_A
            108627,
            //Spawner_Skeleton_A_HangingTree5
            109093,
            //Spawner_Zombie_A_Immediate
            109607,
            //RakEvent_FallenSpawner
            109716,
            //Spawner_Skeleton_C_PortalRouletteRare
            110397,
            //fastMummy_Spawner_B_AqdFastMummyAmbush
            110572,
            //Spawner_Ghost_A_Shape
            110617,
            //Spawner_FastMummy_Climb_B_Aqd_FastMummyAmbush_Rare
            110620,
            //Spawner_FleshPitFlyer_B_Shape
            110628,
            //Spawner_Ghoul_A_Immediate
            110802,
            //Spawner_SkeletonArcher_B_Immediate
            111732,
            //Spawner_Ghost_A_Immediate_FWAmbush
            111764,
            //Spawner_Shield_Skeleton_ScoundrelEvent
            112201,
            //Spawner_Skeleton_C_EOasisAmbush
            113530,
            //Spawner_Swarm_A_OasisSwarmWave
            114466,
            //a2dun_Swr_Grate_Spawner_a
            114642,
            //a2dun_Aqd_Grate_Spawner_a
            114858,
            //Spawner_Lacuni_Female_A_OasisLacuniAmbush
            115148,
            //a2dun_Swr_Grate_Spawner_a_Lightbeam
            115375,
            //Spawner_Crypt_Alcove_Of_Rot
            115382,
            //trDun_RescueCainSkelSpawner_Unique
            115419,
            //Snakeman_Caster_A_Spawner_KamyrAttack
            115546,
            //Snakeman_Melee_A_Spawner_KamyrAttack
            115547,
            //a1dun_Leor_Door_FireZombie_Spawner_A_Dead
            115550,
            //Spawner_Ghost_C
            116025,
            //a2dun_Spider_Ground_Spawner
            116063,
            //a2dun_Aqd_Grate_Spawner
            116144,
            //a2dun_SWR_Grate_Spawner
            116161,
            //Spawner_ZombieSkinny_A_Immediate_Chand
            117944,
            //Spawner_Zombie_A_Immediate_Chand
            117945,
            //Spawner_Skeleton_A_Immediate_Chand
            117946,
            //Spawner_SkeletonSummoner_A_Immediate_Chand
            117947,
            //Spawner_Skeleton_B_Immediate_Chand
            117948,
            //Spawner_SkeletonArcher_A_Immediate_Chand
            117949,
            //Spawner_SandMonster_A_SandMonsterPit
            120534,
            //Spawner_Sandling_A_SandMonsterPit
            120538,
            //Spawner_FleshPitFlyer_A_Rare
            121203,
            //a2dun_Aqd_Mummy_Spawner_Muck
            121821,
            //a3_azmodan_fight_TEMP_spawner
            122924,
            //Spawner_ZombieCrawler_Custom_B3
            123159,
            //Spawner_Spider_A_Fast
            123321,
            //caOut_Boneyard_SkullSpawner
            123325,
            //Spawner_Spiderling_A
            123572,
            //Spawner_Goatman_SpiritJourney
            128823,
            //Spawner_DemonFlyer_A
            129227,
            //trOut_OldTristram_Cellar_ZombieAmbush_Spawner
            129685,
            //Spawner_MorluSpellcaster_A
            129936,
            //Spawner_ThousandPounder_KeepEvent
            129994,
            //Spawner_Cultist_A_SwordOfJustice
            131150,
            //Spawner_Triune_Berserker_A_Immediately
            133550,
            //Spawner_Triune_Cultist_C_Immediately
            134797,
            //Spawner_Zombie_B_MedClimb
            135025,
            //Spawner_ZombieSkinny_B_MedClimb
            135033,
            //hoodedNightmare_GatewayToHell_Spawner
            136165,
            //MistressOfPain_SpiderSpawner
            137126,
            //Spawner_Spider_A_Instant
            140006,
            //a3_demon_trooper_climb_spawner_short
            140429,
            //DrownedTempleGhostSpawner
            140599,
            //Spawner_Ghost_B_Immediate
            140671,
            //Spawner_Sandling_A
            140681,
            //Spawner_electricEel_A
            140945,
            //Spawner_ThousandPounder_A
            148818,
            //TerrorDemon_A_Spawner
            149740,
            //Spawner_Izual_BigRed
            150222,
            //Spawner_BloodHawk_A_nofly
            151863,
            //Spawner_demonTrooper_A_Immediate
            152154,
            //Spawner_demonTrooper_A
            152680,
            //Spawner_demonTrooper_A_summoned
            152741,
            //Snakeman_Melee_Spawner_Siege
            153950,
            //Spawner_Snakeman_Melee_A_Immediate
            154508,
            //Spawner_FleshPitFlyer_D_WormCave
            154629,
            //Ghost_A_Unique_Chancellor_Spawner
            156381,
            //Spawner_SandShark_B_SewerSharkEvent
            156740,
            //trDun_Skeleton_A_Spawner_WallJump_01
            156766,
            //Spawner_Ghost_D_GhostHunters
            156768,
            //CaldeumEscape_GuardSpawner
            157508,
            //Snakeman_Melee_A_Spawner_Evacuation
            157519,
            //Spawner_ZombieSkinny_A_Crawl
            158089,
            //Spawner_TristramGuard_A_Ghost
            158115,
            //Spawner_Unburied_A_DarkRitual
            158124,
            //Spawner_SkeletonArcher_A_Resurrect
            158501,
            //Snakeman_Caster_A_Spawner_EscapeFromCaldeum
            160443,
            //Snakeman_Melee_A_Spawner_EscapeFromCaldeum
            160444,
            //FastMummySpawner_B_Gibs
            160581,
            //a2dun_Aqd_Grate_Spawner_AlphaCat
            162073,
            //a2dun_Aqd_Grate_Spawner_AlphaCat_LacuniFemale
            162074,
            //Spawner_Lacuni_Female_Immediately
            165549,
            //Spawner_DemonFlyer_A_Bomber
            166385,
            //Spawner_Triune_Berserker_C
            167178,
            //Spawner_Triune_Summonable_C
            167202,
            //Spawner_Spider_A_Rappel
            167273,
            //A2C2GreedyMinerFallenShamanSpawner
            167507,
            //Spawner_CoreEliteDemon_A
            167526,
            //Spawner_ShadowVermin_A
            167542,
            //Spawner_BigRed_A
            167633,
            //Spawner_electricEel_A_Aquaducts
            168060,
            //Spawner_Leor_Iron_Maiden_JewelerQuest
            168235,
            //Snakeman_Melee_A_Spawner_WaterfallAmbush
            168666,
            //Spawner_FallenLunatic_A
            168843,
            //Wizard_arcaneTorrent_projectile_indigo_spawner
            170268,
            //Event_Spawner_FastMummy_Jump_A_Small
            171503,
            //Event_Spawner_FastMummy_Jump_A_Big
            171504,
            //Spawner_Leor_Iron_Maiden_JewelerQuest_JewelThief
            171885,
            //Spawner_Graverobber
            172968,
            //TownAttack_SummonerSpawner
            173527,
            //Spawner_Graverobber_Nigel
            174012,
            //Spawner_Inferno_Zombie_ShortClimb
            174379,
            //Spawner_Ghoul_E_Climb
            174388,
            //ThousandMonster_Spawner_ShadowVermin_B
            174429,
            //Spawner_Skeleton_D_Fire
            175359,
            //zombieCrawler_Spawner_B
            176054,
            //trDun_Skeleton_A_Spawner_WallJump_01_Champion
            176221,
            //Wilderness_Coffin_Spawner_PushingDaisies
            176551,
            //Spawner_Zombie_C_ShortClimb
            177358,
            //Spawner_Cath_SkeletonTotem
            178281,
            //Spawner_A3_UniqueVendor_Alchemist
            178521,
            //Wilderness_Coffin_Spawner_FamilyTiesA
            178553,
            //Spawner_FallenGrunt_A
            178886,
            //Spawner_FallenChampion_A
            178887,
            //Spawner_Triune_Cultist_C_Maghda
            179121,
            //Spawner_ZombieSkinny_A_Unique
            180566,
            //Spawner_kidVendor_larra
            180947,
            //Spawner_Monstrosity_Scorpion_A
            181151,
            //fastMummy_Spawner_A_PortalRoulette
            181235,
            //SandShark_Mother_Spawner
            183730,
            //Spawner_Triune_Summonable_C_Prison
            184867,
            //Spawner_ThousandPounder_A_FastSpawn
            185593,
            //Spawner_CaldeumTortured_DogBiteCellar
            186293,
            //a4dun_Garden_Corruption_Angel_Spawner
            187244,
            //SandMonster_spawner_black
            187583,
            //a4dun_Garden_Corruption_Angel_Spawner_Twist
            188328,
            //Spawner_ZombieCrawler_Custom_B2
            188590,
            //Spawner_ZombieCrawler_Custom_B4
            188591,
            //Spawner_Swarm_B
            190519,
            //Spawner_Triune_Cultist_D_Immediately
            193346,
            //Spawner_Skeleton_C_Summoned_Immediately
            194607,
            //Spawner_Itherael
            195687,
            //shadowVermin_spawner_geyser
            196403,
            //Spawner_Ghoul_E_DropDown
            196708,
            //a2dunCaves_Interactives_dead_worm_spawner
            196896,
            //Spawner_ShadowVermin_Gardens
            197873,
            //Spawner_creepMob_A
            197950,
            //Spawner_Goatman_Melee_A_TinkerEvent
            199384,
            //Spawner_ShadowVermin_Spire
            199465,
            //a3_demon_trooper_climb_spawner_hub
            201639,
            //Spawner_Shield_Skeleton_A_MassGraveEvent
            201983,
            //Spawner_Ghoul_E_FromGround
            202006,
            //caOut_Boneyard_SkullSpawner_B
            204168,
            //a2dun_Zolt_Tesla_Tower_Spawner
            204509,
            //Spawner_DemonFlyer_B
            204856,
            //Spawner_DemonFlyer_B_Distribution
            205453,
            //Spawner_DemonFlyer_A_KeepAmbush
            205477,
            //a1dun_Crypts_AlcoveOfRot_MobSpawner
            205775,
            //Spawner_FallenShaman_D
            206011,
            //Spawner_FallenGrunt_D_PlayingDeadEvent
            206030,
            //Spawner_Scavenger_B_MinerEvent
            206318,
            //Spawner_DemonFlyerMega_A
            207286,
            //FleshPitFlyerSpawner_E_Gardens
            207433,
            //Spawner_FastMummy_FASTClimb_C
            208584,
            //a2dun_Zolt_Tesla_Tower_Spawner_Cold
            208824,
            //a2dun_Zolt_Tesla_Tower_Spawner_Fire
            208825,
            //a2dun_Zolt_Tesla_Tower_Spawner_Poison
            208826,
            //a2dun_Swr_Arch_Spawner_Server
            209018,
            //Spawner_FallenLunatic_A_Pools
            209496,
            //Spawner_FallenShaman_A
            212683,
            //Spawner_sandMonster_A_Head_Guardian
            212729,
            //Snakeman_Melee_A_Spawner_NotDisabled
            212932,
            //Spawner_Goatman_Shaman_goatmanPyreEvent
            213416,
            //trout_fields_goatman_trap_door_Spawner
            213518,
            //trout_highlands_goatman_trap_door_Spawner
            213527,
            //trout_highlands_UniqueWagon_Spawner
            213559,
            //trOut_OldTristram_AdriasHut_ZombieAmbush_Spawner
            213955,
            //Spawner_DemonFlyer_B_KeepAmbush
            214037,
            //Spawner_Skeleton_A_Coffin
            214623,
            //a3_azmodan_spawner_fireRing
            214636,
            //FallenGrunt_B_Spawner
            215267,
            //FallenGrunt_B_HealthDropper_Spawner
            215268,
            //Spawner_ZombieSkinny_B_CloseClimb
            217011,
            //Spawner_FleshPitFlyer_F_WormCave
            217316,
            //Spawner_skeletonMage_ZK_BodyGuardian_Fire_Hologram
            217385,
            //Spawner_skeletonMage_ZK_BodyGuardian_Cold_Hologram
            217386,
            //Spawner_skeletonMage_ZK_BodyGuardian_Poison_Hologram
            217387,
            //Spawner_skeletonMage_ZK_BodyGuardian_Lightning_Hologram
            217388,
            //Spawner_ZombieSkinny_A_Unique_02
            218301,
            //Spawner_Skeleton_A_Unique_01
            218320,
            //Spawner_ZombieSkinny_A_Unique_03
            218669,
            //a2dunCaves_Interactives_dead_worm_spawner_02
            218846,
            //Spawner_ShadowVermin_Geyser
            219035,
            //Spawner_BigRed_A_Geyser
            219175,
            //Spawner_Angel_Corrupt_A_Geyser
            219180,
            //Spawner_Swarm_B_FastMummyA
            219208,
            //Spawner_FastMummy_B_CorpseHive
            219213,
            //a2dun_Swr_Arch_Spawner_ZombieCrawler_01
            219223,
            //BileCrawler_A_Spawner
            219249,
            //fastMummy_Spawner_A_ShadeOfRadament
            219579,
            //Spawner_Triune_Berserker_C_NecromancerChampions
            219621,
            //Spawner_sandMonster_A_PortalRoulette
            219836,
            //Spawner_LacuniMale_B_Unique_TowerRuins
            219841,
            //Spawner_fastMummy_B_SmallFacePuzzleUnique
            219885,
            //Spawner_FastMummy_Climb_A_SmallFacePuzzle
            219901,
            //Spawner_Ghost_D_FacePuzzleUnique
            219918,
            //Spawner_Ghost_D_FacePuzzle
            219919,
            //Spawner_LacuniMale_A
            220159,
            //Spawner_GhostKnight_DoKEvent
            220218,
            //Spawner_Leoric_DoKEvent
            220219,
            //Spawner_Siege_wallMonster_catapult
            220470,
            //Spawner_Triune_Berserker_E_Unique
            220984,
            //Spawner_DemonFlyer_B_TideOfBattle
            221731,
            //BileCrawler_B_Spawner
            223720,
            //Spawner_CoreEliteDemon_A_Max1
            224676,
            //Spawner_CoreEliteDemon_A_Max2
            224677,
            //Spawner_CoreEliteDemon_A_Max3
            224678,
            //Spawner_CoreEliteDemon_A_Max4
            224679,
            //HoodedNightmare_ShadowRealm_A_Spawner
            225586,
            //Spawner_Triune_Berserker_A_Immediately_Champion
            225990,
            //Spawner_ZombieSkinny_A_Unique_Marko
            226501,
            //Spawner_Triune_Berserker_C_Alcarnus
            229342,
            //a3_azmodan_fight_spawner
            230097,
            //Spawner_ZombieSkinny_A_Rare
            230332,
            //Spawner_Zombie_A_rare
            230333,
            //Spawner_Shield_Skeleton_E_Dropdown
            230728,
            //trDun__JailGhoulSpawner02_instant
            231311,
            //Event_Spawner_bogFamily
            237183,
            //Spawner_Skeleton_NecroJar
            239338,
            //x1_Bog_props_bogpeople_spawner_A
            247999,
            //x1_Bog_Props_Bogpeople_spawner_Branches
            248012,
            //x1_Bog_Props_Bogpeople_spawner_A_stump
            248018,
            //x1_Bog_props_bogpeople_spawner_spawnerA
            248174,
            //x1_Bog_props_bogpeople_spawner_spawnerA
            248174,
            //x1_Bog_Spawner_BogBrute_A
            252663,
            //Spawner_Ghoul_E_ClimbShort
            260239,
            //X1_Spawner_Skeleton_POF_ThousandPounderOfSouls
            262207,
            //PVP_spawner_destructible_temp
            265704,
            //PVP_spawner_invulnerable
            265953,
            //Spawner_MastaBlastaRider_A
            266003,
            //X1_Westm_Spawner_Window_Protoype_Double_A_Server
            269877,
            //X1_Westm_Spawner_Window_Protoype_Large_Server
            269899,
            //X1_Westm_Spawner_Short_Wall
            270031,
            //Spawner_x1_WestM_RoofJumper_01
            282364,
            //x1_Spawner_Shield_Skeleton_Westm_CircleOfDeath
            285839,
            //x1_Spawner_Rat_A
            286159,
            //x1_Spawner_Ghoul_A_Challenge_GhoulSwarm
            286954,
            //x1_Spawner_FleshPitFlyer_B
            290535,
            //x1_Spawner_Triune_Berserker_A
            290728,
            //x1_Spawner_Triune_Cultist_A
            290729,
            //x1_Spawner_TriuneSummoner_A
            290730,
            //Spawner_x1_FloaterDemon_A
            290979,
            //x1_Spawner_Skeleton_A
            292294,
            //x1_Spawner_SkeletonArcher_A
            292296,
            //x1_Spawner_SkeletonSummoner_A
            292297,
            //x1_Spawner_Unburied_A_CursedTomb_Unique
            292331,
            //x1_Spawner_Zombie_Inferno_C
            292760,
            //x1_Spawner_FleshPitFlyer_Inferno
            292762,
            //x1_Spawner_Spiderling_A_Unburrow
            293025,
            //x1_Spawner_Goatman_Melee_B_Challenge
            293362,
            //x1_Spawner_Goatman_Ranged_B_Challenge
            293363,
            //x1_Spawner_Goatman_Shaman_B_Challenge
            293364,
            //x1westmInt_Boat_Spawner_A
            294765,
            //x1_WestM_Grate_Spawner_01
            294834,
            //x1_Tentacle_Melee_A_Spawner
            295032,
            //x1_Spawner_Sandling_B_Challenge
            296513,
            //x1_Spawner_SandMonster_A_Challenge
            296514,
            //x1_Challenge_Spawner_Lacuni_Female_A
            297799,
            //x1_Challenge_Spawner_LacuniMale_A
            297800,
            //x1_Challenege_Spawner_FallenChampion_A
            297877,
            //x1_Challenege_Spawner_FallenGrunt_A
            297878,
            //x1_Challenge_Spawner_FallenShaman_A
            297881,
            //x1_Spawner_Ghoul_A_Challenge_GhoulSwarm_02
            300146,
            //x1_Challenge_Corupulent_A_Spawner
            300174,
            //Spawner_x1_Pand_MosquitoBat_Ideation
            300763,
            //x1_Spawner_Pand_Ext_Ideation_SandMonster_Spawner
            301226,
            //x1_Spawner_Pand_Ext_Ideation_SandMonster_Spawner
            301226,
            //Spawner_Angel_Corrupt_A_Voltron
            301332,
            //Spawner_Dark_Angel_Voltron
            301333,
            //x1_Pand_BatteringRam_Spawner
            301453,
            //x_Challenge_Spawner_ZombieSkinny_SunkenGrave
            302114,
            //x1_Challenge_Spawner_ZombieFemale_SunkenGrave
            302115,
            //x1_Challenge_Spawner_Zombie_SunkenGrave
            302116,
            //x1_Spawner_BloodHawk_A_nofly_Challenge
            302476,
            //x1_Spawner_DuneDervish_A_Challenge
            302512,
            //x1_Spawner_Skeleton_TwoHander_E_Keep_Swift
            303857,
            //x1_Spawner_Shield_Skeleton_E
            303858,
            //x1_Spawner_SkeletonSummoner_D
            303859,
            //x1_westm_Spawner_WestmarchBrute_A
            303980,
            //x1_Spawner_GoatMutant_Melee_B
            304282,
            //x1_Spawner_GoatMutant_Ranged_B
            304283,
            //x1_Spawner_GoatMutant_Shaman_B
            304284,
            //x1_Spawner_DemonFlyer_A_Challenge
            304393,
            //x1_Spawner_ThousandPounder_A_Challenge
            304395,
            //x1_Spawner_creepMob_B_Challenge
            304509,
            //x1_Spawner_Lacuni_Female_Snow_Challenge
            304623,
            //x1_Spawner_LacuniMale_Snow_Challenge
            304624,
            //x1_Spawner_DemonFlyer_C_Challenge
            306221,
            //x1_Spawner_BigRed_A_Challenge
            306222,
            //x1_Spawner_Succubus_B_Challenge
            306223,
            //x1_BileCrawler_A_Spawner_Challenge
            306462,
            //x1_Spawner_MorluMelee_A_Challenge
            307092,
            //Spawner_x1_WestM_RoofJumper_Unique
            307115,
            //x1_Spawner_FleshPitFlyerSpawner_B
            308159,
            //x1_Spawner_FleshPitFlyerSpawner_B
            308159,
            //x1_MalletDemon_A_Spawner_Challenge
            308192,
            //x1_westm_Soul_Summoner_Spawner
            308823,
            //X1_Pand_HexMaze_EN_SpawnerCoreElite
            309822,
            //X1_Pand_HexMaze_EN_SpawnerSuccubus
            309827,
            //X1_Spawner_WestM_HauntedManor_Ghosts
            312634,
            //Spawner_Siege_wallMonster_A_SiegeBreaker
            316255,
            //x1_NagleEventSpawner
            316385,
            //x1_Spawner_Skeleton_Westmarch_Ghost_A
            316927,
            //x1_Spawner_ZombieSkinny_A_Challenge
            317001,
            //x1_Death_Orb_Spawner_Root
            325546,
            //X1_demonTrooper_Event_Prison_Spawner
            325954,
            //x1_Spawner_portalGuardianMinion_Melee_A_ClimbFromGround
            326288,
            //x1_PandExt_RocklingCharger_Spawner
            326505,
            //x1_PandExt_RocklingRanged_Spawner
            326517,
            //x1_PandExt_ArmorScavenger_Spawner
            326519,
            //x1_PandExt_Squigglet_Spawner
            326520,
            //x1_Spawner_portalGuardianMinion_Melee_A
            326720,
            //x1_Spawner_portalGuardianMinion_Ranged_A
            326721,
            //x1_Spawner_portalGuardianMinion_Ranged_A_ClimbFromGround
            326813,
            //Spawner_x1_FloaterAngel_A
            328214,
            //Spawner_x1_FloaterAngel_A_DropDown
            328215,
            //Spawner_x1_WestmarchBrute_A
            328216,
            //x1_Spawner_Zombie_Inferno_C_Skeleton_Rush
            328266,
            //x1_westm_Spawner_Skeleton_enc
            328419,
            //x1_westm_Spawner_DeathMaiden_A
            330044,
            //x1_westm_Spawner_Skeleton_GhostChase
            330497,
            //x1_Pand_Ext_FallingRocks_Spawner
            330592,
            //x1_westm_Spawner_SkeletonArcher_GhostChase
            330705,
            //x1_westm_Spawner_Hound_GhostChase
            330748,
            //x1_Adria_CauldronSpawner_Temp
            330772,
            //x1_SkeletonArcher_Westmarch_Ghost_Spawner
            331417,
            //x1_Challenge_Spawner_Skeleton_Westmarch_Ghost_A
            331774,
            //x1_Challenge_Spawner_westm_WestmarchBrute_A
            331782,
            //x1_Spawner_Dark_Angel_Challenge
            331957,
            //x1_Spawner_LeaperAngel_A_Climb
            332667,
            //x1_Spawner_sniperAngel_A_Summoned
            332668,
            //x1_Spawner_westmarchBrute_B_pande
            332724,
            //x1_Spawner_PandeWraith_A
            332874,
            //x1_Spawner_Fortress_JudgeEvent_leaperAngel
            334283,
            //x1_Spawner_Wraith_A_Dark_Event_Worldstone
            334291,
            //x1_Spawner_LeaperAngel_A_Event_Worldstone
            334294,
            //x1_Spawner_Fortress_JudgeEvent_shadowVermin
            334295,
            //x1_BileCrawler_Skeletal_A_Spawner
            334842,
            //x1_Skeleton_Fire_A_spawner
            334845,
            //x1_SkeletonTwoHander_Fire_A_spawner
            334854,
            //x1_Spawner_Graveyard_Unique_1_shadowVermin
            335076,
            //x1_Graveyard_Alter_Event_Coffin_Spawner
            335570,
            //Spawner_x1_Monstrosity_ScorpionBug_A
            335579,
            //x1_Spawner_WestmarchBrute_C_Dropdown
            335654,
            //Spawner_x1_Ghost_A
            335750,
            //x1_Spawner_Fast_Zombie_A
            337200,
            //X1_Spawner_Fast_ZombieFemale_A
            337208,
            //X1_Spawner_Fast_ZombieSkinny_A
            337209,
            //x1_Spawner_BogBlight_A
            337425,
            //x1_Spawner_Bogblight_Maggot_A
            337426,
            //x1_Spawner_CaveRipper_A
            337427,
            //x1_Spawner_DemonMelee_A
            337688,
            //x1_Spawner_demonRanged_A
            337690,
            //x1_Spawner_demonMage_A
            337691,
            //x1_Bog_Spawner_BogMelee_A_Challenge
            337700,
            //x1_Bog_Spawner_BogBrute_A_Challenge
            337701,
            //Spawner_x1_demonMelee_A
            337706,
            //Spawner_x1_demonRanged_A
            337707,
            //x1_Spawner_Bogblight_Maggot_A_Offset
            338744,
            //x1_Graveyard_Alter_Event_Coffin_Spawner_Unique
            340153,
            //x1_Spawner_RocklingCharger_Challenge
            340488,
            //x1_Spawner_RocklingRanged_Challenge
            340489,
            //x1_Spawner_ArmorScavenger_Challenge
            340490,
            //Spawner_X1_ZombieCrawler_Orb
            340558,
            //x1_Bog_Spawner_BogRanged_A
            340730,
            //x1_Bog_Spawner_BogMelee_A_3bears
            340731,
            //x1_Bog_Spawner_BogBrute_A_3bears
            340732,
            //Spawner_x1_WestmarchBrute_A_Immediate
            340829,
            //x1_Ghost_Dark_A_Spawner_Immediate
            340847,
            //x1_Spawner_LeaperAngel_A_Climb_Challenge
            340942,
            //x1_Spawner_WestmarchBrute_C_Challenge
            340948,
            //x1_Spawner_Bogblight_Maggot_A_Event
            341145,
            //x1_Spawner_BogMelee_AdriaRitual_FromAbove
            341331,
            //x1_Spawner_BogMelee_AdriaRitual_Jump
            341336,
            //x1_Spawner_BogMelee_AdriaRitual_Burrow
            341337,
            //x1_Spawner_BogBrute_AdriaRitual_Burrow
            341338,
            //Spawner_x1_Monstrosity_ScorpionBug_GardenEvent1
            342357,
            //x1_Spawner_BogFamily_melee_A_UnderGround
            342749,
            //x1_Heaven_AngelTrooper_Spawner
            342851,
            //x1_Spawner_BogFamily_melee_A_RunOut
            343026,
            //x1_Spawner_BogFamily_melee_A_Jump10Unit
            343027,
            //x1_Spawner_BogFamily_melee_A_Jump20Unit
            343028,
            //x1_Graveyard_Coffin_Spawner
            343035,
            //x1_Tentacle_Ranged_A_Spawner
            343774,
            //x1_Tentacle_Shaman_A_Spawner
            343775,
            //x1_Spawner_BogFamily_melee_A_RunOut_event
            343804,
            //x1_Spawner_demonTrooper_Burned_A_FromGround
            344033,
            //x1_Spawner_MorluMelee_C_Ressurect
            344038,
            //X1_Bog_Props_Bogpeople_Spawner_Door
            344384,
            //x1_Spawner_ZombieSkinny_Skeleton_A
            345320,
            //x1_Spawner_LeaperAngel_A_ClimbDownChain
            345614,
            //x1_Spawner_LeaperAngel_A_ClimbUpChain
            345615,
            //x1_Spawner_BogFamily_melee_A_DropsDown
            345631,
            //x1_Spawner_Skeleton_MonsterAffix_World_1
            345764,
            //x1_Spawner_Skeleton_MonsterAffix_World_2
            345765,
            //x1_Spawner_Skeleton_MonsterAffix_World_3
            345766,
            //x1_Spawner_Skeleton_MonsterAffix_World_4
            345767,
            //x1_Spawner_Skeleton_MonsterAffix_World_5
            345768,
            //x1_HexMaze_PortalMinion_Melee_Spawner_A_DropDown_5
            348039,
            //Spawner_x1_Monstrosity_ScorpionBug_A_WallBonePile_3
            348869,
            //Spawner_x1_Monstrosity_ScorpionBug_A_SinkHole_3
            349382,
            //Spawner_x1_Monstrosity_ScorpionBug_A_Wall_25Foot_3
            349592,
            //x1_Spawner_WickerMan_Unique_A
            349984,
            //x1_Spawner_Bogblight_Maggot_A_FromGround
            350415,
            //x1_Spawner_Bogblight_Maggot_A_FromAbove
            350416,
            //x1_PandExt_Bloodhawk_spawner
            350647,
            //x1_Spawner_BogBlight_MME_Unique_A
            350752,
            //x1_Spawner_BogBlight_MaggotDinnerParty_Unique_A
            351274,
            //x1_Spawner_portalGuardianMinion_Ranged_A_ClimbFromGround_Immediate
            351572,
            //x1_Spawner_LeaperAngel_A_Dropdown
            352428,
            //x1_Spawner_Zombie_GraveRobertUnique
            353234,
            //x1_Crusader_Fallingsword_SwordnadoRig_Spawner
            353516,
            //x1_PandExt_RocklingCharger_Spawner_event
            354509,
            //X1_Spawner_RebelGuard_KingEvent3
            356343,
            //x1_Spawner_Bogblight_Maggot_A_wall_arc
            356404,
            //x1_Spawner_Bogblight_Maggot_A_wall_crawl_out
            356405,
            //x1_Spawner_ZombiePile_A
            356490,
            //x1_Spawner_Bogblight_Maggot_A_JumpOut
            356746,
            //X1_Spawner_Skeleton_Westmarch_A
            356952,
            //X1_Spawner_LordofFools
            359076,
            //x1_Spawner_LeaperAngel_A_DropdownDistribution
            360067,
            //x1_Spawner_BogBlight_LedgeSpawn
            360079,
            //x1_Spawner_leaperAngel_A_FortressUnique
            360255,
            //x1_Spawner_sniperAngel_A_FortressUnique
            360256,
            //x1_Spawner_westmarchBrute_C_FortressUnique
            360257,
            //x1_Spawner_Wraith_A_FortressUnique
            360258,
            //x1_Spawner_westmarchBrute_C_DropdownDistribution
            360339,
            //x1_Spawner_BogBlight_A_Unburrow
            361031,
            //Spawner_x1_Beast_Skeleton_A
            361505,
            //Spawner_x1_Monstrosity_ScorpionBug_A_SinkHole_Trigger
            361970,
            //Spawner_x1_Monstrosity_ScorpionBug_A_SinkHole_BigHeadEvent
            362003,
            //x1_PandExt_Bloodhawk_spawner_Clinger
            362821,
            //x1_Spawner_BogFamily_melee_A_Jump20Unit_immediate
            363582,
            //x1_Event_SpeedKill_SkeletonsA_Spawner
            364638,
            //x1_Event_SpeedKill_GoatmanA_Spawner
            364640,
            //x1_Event_SpeedKill_Goatman_melee_Ghost_A_Spawner
            365505,
            //x1_Event_SpeedKill_TriuneVesselA_Spawner
            365523,
            //x1_Event_SpeedKill_ZombieB_Spawner
            365541,
            //x1_Event_SpeedKill_GhostHumansA_Spawner
            365556,
            //x1_Spawner_Skeleton_C_Cursed
            365718,
            //x1_Spawner_Shield_Skeleton_C_Cursed
            365746,
            //Spawner_SkeletonMage_Lightning_B_Cursed
            365754,
            //X1_Spawner_RatAlleyEvent
            367487,
            //x1_Urzael_SoundSpawner
            368268,
            //x1_Westm_Rat_Spawner_Well
            368432,
            //x1_Urzael_SoundSpawner_02
            368599,
            //x1_Urzael_SoundSpawner_03
            368621,
            //x1_Urzael_SoundSpawner_04
            368626,
            //x1_PandExt_ArmorScavenger_Spawner_NoArmorStart
            369053,
            //x1_LeoricDeserters_Spawner
            369120,
            //x1_Spawner_BileCrawler_Skeletal_A_Challenge
            369331,
            //Spawner_x1_Ghost_A_Challenge
            369337,
            //X1_Spawner_demonTrooper_Chronodemon_Burned_A
            369517,
            //Spawner_x1_westmarch_rat_DeadEndDoorAmbush
            369536,
            //x1_Event_SpeedKill_electricEel_A_Spawner
            369821,
            //x1_Event_SpeedKill_TriuneCultist_C_Spawner
            369830,
            //x1_Event_SpeedKill_fastMummy_A_Spawner
            369840,
            //x1_Event_SpeedKill_Bloodhawk_A_Spawner
            369845,
            //x1_Event_SpeedKill_Snakeman_A_Spawner
            369848,
            //x1_Event_SpeedKill_Swarm_A_Spawner
            369860,
            //x1_Event_SpeedKill_Lacuni_B_Spawner
            369876,
            //x1_Event_SpeedKill_Spiderling_B_Spawner
            369883,
            //x1_Event_SpeedKill_demonFlyer_B_Spawner
            370030,
            //x1_Event_SpeedKill_Fallen_C_Spawner
            370036,
            //x1_Event_SpeedKill_Goatmutant_B_Spawner
            370043,
            //x1_Event_SpeedKill_Monstrosity_Scorpion_A_Spawner
            370048,
            //x1_Event_SpeedKill_Ghoul_E_Spawner
            370055,
            //x1_Event_SpeedKill_Skeleton_E_Spawner
            370059,
            //x1_Event_SpeedKill_Champion_creepMob_A_Spawner
            370065,
            //x1_Event_SpeedKill_Champion_azmodanBodyguard_A_Spawner
            370073,
            //x1_Event_SpeedKill_Champion_FallenHound_D_Spawner
            370086,
            //x1_Event_SpeedKill_Champion_SoulRipper_A_Spawner
            370138,
            //x1_Event_SpeedKill_BileCrawler_A_Spawner
            370314,
            //x1_Event_SpeedKill_CoreEliteDemon_A_Spawner
            370319,
            //x1_Event_SpeedKill_morluMelee_B_Spawner
            370327,
            //x1_Event_SpeedKill_Angel_Corrupt_A_Spawner
            370332,
            //x1_Event_SpeedKill_Champion_morluSpellcaster_A_Spawner
            370345,
            //x1_Event_SpeedKill_Champion_BigRed_A_Spawner
            370351,
            //x1_Event_SpeedKill_Champion_MalletDemon_A_Spawner
            370367,
            //x1_Event_SpeedKill_x1_Skeleton_Ghost_A_Spawner
            370430,
            //x1_Event_SpeedKill_x1_BogFamily_A_Spawner
            370437,
            //x1_Event_SpeedKill_x1_bogBlight_Maggot_A_Spawner
            370448,
            //x1_Event_SpeedKill_x1_Monstrosity_ScorpionBug_A_Spawner
            370455,
            //x1_Event_SpeedKill_x1_leaperAngel_A_Spawner
            370463,
            //x1_Event_SpeedKill_x1_westmarchHound_A_Spawner
            370478,
            //x1_Event_SpeedKill_x1_BileCrawler_Skeletal_A_Spawner
            370485,
            //x1_Event_SpeedKill_x1_portalGuardianMinion_A_Spawner
            370492,
            //x1_Event_SpeedKill_Champion_x1_FloaterAngel_A_Spawner
            370515,
            //x1_Event_SpeedKill_Champion_x1_Rockworm_Pand_A_Spawner
            370543,
            //x1_Event_SpeedKill_TentacleBears_Spawner
            370668,
            //X1_Bog_Props_Bogpeople_Spawner_Door_Noframe
            370682,
            //X1_Bog_Props_Bogpeople_Spawner_Door_Noframe_Short
            370757,
            //x1_Event_SpeedKill_Champion_SquiggletA_Spawner
            370836,
            //X1_Spawner_Fast_Zombie_Random
            373264,
            //x1_Pand_Cellar_FallingRock_Spawner
            374882,
            //x1_Spawner_x1_Skeleton_B_Fire
            375395,
            //x1_Spawner_DeathMaiden_Fire_AbattoirFurnaceEvent
            375413,
            //x1_Spawner_Zombie_Inferno_C_CursedChest
            376511,
            //X2_ZPVP_Spawner_GoblinHunter
            378932,
            //X2_ZPVP_Spawner_Powerups
            378933,
            //X1_Spawner_graveRobber_ScoundrelEvents
            388989,
            //a3_azmodan_fight_spawner_old_gen
            401265,
            //px_Spawner_Siege_wallMonster_C
            410365,
            //Spawner_BigRed_A_Unique
            410383,
            //x1_Spawner_FleshPitFlyerSpawner_Gardens
            410428,
            //x1_Spawner_FleshPitFlyerSpawner_Gardens
            410428,
            //Spawner_Ghost_JarOfSouls_Unique
            415758,
            //Spawner_x1_FloaterAngel_A_Instant
            415812,
            //P2_Goblin_Spawner_LR
            425478,
            //Spawner_MarkerLocation_SpecialGoblinRiftSpawn
            429676,
            //px_Wilderness_Camp_TemplarSpawner
            430767,
            //px_FesteringWoods_Camp_ThievesGuildSpawner
            432668,
            //px_SpiderCaves_Camp_Cocoon_MysterySpawner
            432776,
            //px_SpiderCaves_Camp_Cocoon_RareSpawner
            432779,
            //px_Spawner_Spider_A_Fast
            432783,
            //px_StingingWinds_Camp_CultistSpawner
            433058,
            //px_Bounty_Westmarch_Spawner_Reapers
            433253,
            //px_Bounty_Camp_azmodan_fight_spawner
            433295,
            //px_Spawner_Skeleton_A_Bounty_Camp_Graveyard
            433327,
            //px_Crater_Camp_AzmodanMinions_PortalSpawner
            434333,
            //px_Spire_Camp_HellPortals_PortalSpawner
            434340,
            //px_Highlands_Camp_ResurgentCult_PortalSpawner
            434361,            
        };


        /// <summary>
        /// SNOs that belong to a player, spells, summons, effects, projectiles etc.
        /// </summary>
        public static HashSet<int> PlayerOwnedSNO = new HashSet<int>
        {
            493, 3368, 6509, 6511, 6513, 6514, 6515, 6516, 6519, 6522, 6523, 6524, 6528, 6535, 6542, 6543, 6550, 6551, 6553, 6554,
            6558, 6560, 6561, 6562, 6563, 58362, 61419, 61445, 62054, 71129, 75631, 75642, 75726, 75731, 75732, 76019, 77097, 77098,
            77116, 77333, 77805, 80600, 80745, 80757, 80758, 81103, 81229, 81230, 81231, 81232, 81238, 81239, 81515, 82109, 82111,
            82660, 82972, 83024, 83025, 83028, 83043, 83959, 83964, 84504, 86082, 86769, 86790, 87621, 87913, 88032, 90364, 91424,
            91440, 91441, 91702, 92030, 92031, 92032, 93067, 93076, 93560, 93582, 93592, 93718, 97691, 97821, 98010, 99565, 99566,
            99567, 99572, 99574, 99629, 107916, 112560, 112572, 112585, 112588, 112594, 112675, 112697, 112806, 112808, 112811,
            117557, 130029, 130030, 130035, 130073, 130074, 130668, 141936, 144003, 147977, 148060, 148070, 148077, 148220, 148634,
            148700, 149837, 154769, 161695, 161772, 162301, 164699, 166051, 166130, 166172, 167260, 167261, 167262, 167263, 167382,
            167397, 167419, 167463, 167564, 167628, 167724, 167807, 167814, 167817, 167978, 170169, 170199, 170268, 170287, 170385,
            170405, 170445, 170496, 170574, 170592, 170935, 171179, 171180, 171184, 171185, 171225, 171226, 176247, 176248, 176262,
            176265, 176287, 176288, 176356, 176390, 176407, 176440, 176600, 176653, 185106, 185125, 185226, 185233, 185263, 185273,
            185283, 185301, 185309, 185316, 185459, 185513, 185660, 185661, 185662, 185663, 189372, 189373, 189375, 189458, 189460,
            191967, 192126, 192210, 192211, 192271, 199154, 201526, 210804, 210896, 215324, 215420, 215488, 215511, 215516, 215700,
            215711, 215809, 215853, 216040, 216069, 216462, 216529, 216817, 216818, 216851, 216874, 216890, 216897, 216905, 216941,
            216956, 216975, 216988, 217121, 217130, 217139, 217142, 217172, 217180, 217287, 217307, 217311, 217457, 217458, 217459,
            219070, 219196, 219200, 219254, 219292, 219295, 219300, 219314, 219315, 219316, 219391, 219392, 219393, 226648, 249225,
            249226, 249227, 249228, 249975, 249976, 251688, 251689, 251690, 261341, 261342, 261343, 261344, 261616, 261617, 299099,
            300476, 302468, 314792, 315588, 316207, 316239, 316270, 316271, 317398, 317409, 317501, 317507, 317652, 317809, 319692,
            319698, 319732, 319771, 322022, 322236, 322350, 322406, 322488, 323029, 323092, 323149, 323897, 324143, 324451, 324459,
            324466, 325154, 325552, 325804, 325807, 325813, 325815, 326285, 326305, 326308, 326313, 326755, 328146, 328161, 328171,
            328199, 336410, 337757, 339443, 339473, 341373, 341381, 341396, 341410, 341411, 341412, 341426, 341427, 341441, 341442,
            342082, 343197, 343293, 343300, 347101, 362960, 362961, 366983, 381908, 381915, 381917, 381919, 385215, 385216, 394102,
            396290, 396291, 396292, 396293, 409287, 409352, 409430, 409523, 322, 3276, 3277, 3280, 3281, 3282, 3283, 3289, 3290,
            3291, 3297, 3298, 3303, 3305, 3306, 3308, 3309, 3310, 3314, 3315, 3316, 3317, 3319, 51303, 74636, 79400, 90443, 90535,
            90536, 92895, 93481, 93903, 99541, 100800, 100832, 100839, 100934, 101068, 108742, 108746, 108767, 108772, 108784,
            108789, 108808, 108819, 108868, 108907, 108920, 109151, 136261, 143994, 158990, 159030, 159614, 159626, 159631, 159940,
            160587, 160685, 160818, 160893, 161452, 161457, 161599, 161607, 161654, 161657, 161890, 161892, 161893, 161894, 161960,
            162005, 162087, 162114, 162386, 162387, 162548, 162577, 162590, 162593, 162621, 162622, 162623, 162766, 162839, 162920,
            162929, 163353, 163462, 163494, 163501, 163541, 163552, 163783, 163792, 163861, 163925, 163949, 163968, 164066, 164112,
            164708, 164709, 164710, 164712, 164713, 164714, 164747, 164770, 164788, 164804, 165040, 165043, 165069, 165381, 165382,
            165514, 165515, 165560, 165561, 165988, 166214, 166222, 166223, 166438, 168307, 168440, 168460, 173342, 174723, 189078,
            189094, 198346, 209487, 220559, 220562, 220565, 220569, 220632, 252478, 252479, 317733, 356731, 358574, 360571, 362283,
            362949, 362951, 363301, 363760, 363765, 364460, 364953, 365194, 365291, 365338, 365340, 365342, 365534, 365789, 373063,
            373074, 373211, 374667, 374683, 375757, 396470, 408515, 408532, 410567, 75678, 75887, 77569, 77604, 77813, 87564, 88244,
            88251, 111307, 111330, 111503, 129228, 129603, 129621, 129785, 129787, 129788, 129934, 131701, 141937, 143995, 147809,
            148845, 148846, 148847, 149944, 149946, 149947, 149948, 151832, 151842, 153029, 153864, 153865, 153866, 153867, 153868,
            154674, 155092, 155147, 155749, 155938, 158843, 158844, 158916, 158917, 158940, 158941, 160932, 165558, 166549, 166550,
            166556, 166557, 166582, 166583, 166584, 166585, 166618, 166619, 166620, 166621, 166636, 166637, 167169, 167171, 167172,
            167218, 194565, 194566, 200561, 200672, 200808, 212547, 314804, 347447, 349626, 360546, 360547, 360548, 360549, 360550,
            360561, 360563, 360564, 360697, 360773, 361118, 361213, 361214, 362954, 362955, 428572, 428574, 129784, 129932, 130366,
            130572, 130661, 131664, 131672, 132068, 132615, 132732, 133714, 133741, 134841, 134917, 135207, 136149, 141402, 141681,
            141734, 143853, 143854, 147960, 148788, 148900, 149338, 149770, 149790, 149935, 149949, 149975, 150024, 150025, 150026,
            150027, 150036, 150037, 150038, 150039, 150061, 150062, 150063, 150064, 150065, 150449, 151591, 151805, 151929, 151998,
            152116, 152269, 152589, 152736, 152857, 152863, 153075, 153352, 154093, 154194, 154198, 154199, 154200, 154201, 154227,
            154292, 154590, 154591, 154592, 154593, 154595, 154657, 154736, 154750, 154811, 155096, 155149, 155159, 155276, 155280,
            155353, 155374, 155376, 155734, 155848, 156100, 157728, 159098, 159102, 159144, 160612, 162563, 165340, 165467, 165767,
            166462, 166613, 166732, 167223, 167235, 168815, 173827, 178664, 180622, 180640, 181748, 182234, 182263, 186050, 193463,
            193493, 193496, 193497, 193499, 193500, 196030, 196615, 200810, 215242, 215727, 218467, 218504, 219494, 219509, 219534,
            219577, 219580, 219609, 219610, 220527, 220800, 221261, 221440, 222109, 222115, 222117, 222122, 222128, 222130, 222135,
            222141, 222143, 222151, 230674, 251704, 251710, 261665, 366893, 366897, 366921, 366933, 366935, 367223, 367232, 367251,
            367258, 370495, 370496, 375869, 375871, 376739, 376743, 396318, 405388, 406116, 408096, 408100, 408103, 408300, 408327,
            408333, 408335, 408379, 408422, 408568, 410274, 410299, 428075, 253211, 253416, 253717, 255056, 256083, 256180, 257376,
            257777, 257782, 258133, 263866, 265474, 266503, 277722, 277805, 277808, 280196, 280462, 280702, 284686, 284872, 284920,
            285209, 285380, 285476, 286805, 286925, 289010, 289991, 290291, 290315, 290325, 290330, 290340, 290460, 290494, 290508,
            290521, 290532, 291672, 292175, 292608, 292837, 292849, 293223, 293293, 293307, 293312, 293318, 293332, 293342, 293644,
            293682, 293701, 293725, 293762, 293769, 293773, 293797, 293843, 293850, 293866, 293872, 293895, 293912, 293915, 293919,
            293927, 293930, 293947, 293998, 294005, 294009, 294012, 294023, 294047, 294052, 294057, 294169, 294182, 294330, 294333,
            294336, 294340, 294348, 294351, 294355, 294358, 294361, 294365, 294368, 294371, 294375, 294378, 294381, 294436, 294439,
            294442, 294446, 294449, 294452, 294456, 294459, 294462, 294466, 294469, 294472, 294476, 294479, 294482, 294517, 294520,
            294523, 294527, 294530, 294533, 294537, 294540, 294543, 294547, 294550, 294553, 294557, 294560, 294563, 294732, 295009,
            295012, 295015, 300142, 304115, 306040, 306210, 306225, 308143, 312522, 312552, 312658, 312661, 314802, 316079, 316835,
            319402, 323582, 324046, 324050, 324059, 324081, 324126, 324856, 324878, 325092, 325528, 326101, 327375, 327987, 329016,
            330019, 330042, 330082, 330728, 332236, 332450, 332465, 332631, 332644, 332702, 332705, 332759, 332760, 335993, 335995,
            336138, 336201, 336202, 336203, 336279, 336285, 336286, 336339, 336356, 336360, 336361, 336468, 336710, 336968, 337088,
            337184, 338224, 338225, 338226, 338227, 338246, 338252, 338598, 338678, 338807, 339343, 340460, 342209, 342213, 342257,
            342332, 342336, 342562, 342581, 342587, 342919, 342938, 342940, 342945, 343022, 343099, 343180, 343250, 343262, 343614,
            343792, 343879, 343881, 343954, 344224, 344305, 344330, 344543, 344546, 344571, 344573, 344577, 344588, 345114, 345224,
            345228, 345232, 345249, 345295, 345296, 345305, 345682, 345787, 345800, 345892, 346151, 346291, 346293, 346915, 347135,
            347248, 347249, 347360, 347421, 347466, 347643, 347647, 347677, 347792, 347798, 348203, 348262, 348731, 348735, 348766,
            348991, 348993, 349465, 349483, 349485, 349498, 349509, 349510, 349534, 349540, 349705, 349895, 349932, 349942, 349943,
            349959, 349977, 349989, 349991, 350052, 350072, 350083, 350135, 350158, 350204, 350219, 350461, 350468, 350631, 350634,
            350636, 350685, 350686, 350706, 350808, 351031, 351051, 351093, 351139, 352414, 352433, 352455, 352677, 352680, 352683,
            352687, 352723, 352737, 352757, 352871, 352933, 352942, 352969, 352985, 353516, 353843, 354249, 354412, 354579, 354606,
            354969, 355126, 355171, 356719, 356879, 356904, 357248, 357258, 357261, 357317, 357350, 357351, 357355, 357356, 357358,
            357369, 357381, 357382, 357387, 357388, 357426, 357432, 357436, 357439, 357472, 357475, 357574, 357584, 357587, 357645,
            357802, 357803, 357903, 357914, 357916, 357946, 357949, 358001, 358010, 358106, 358235, 358236, 358243, 358244, 358245,
            358246, 359040, 359041, 359042, 359043, 359044, 359045, 359709, 362665, 362952, 362953, 363775, 363890, 369795, 369975,
            369976, 369977, 369978, 369979, 369980, 428280, 6443, 6450, 6451, 6452, 6453, 6457, 6459, 6463, 6465, 6489, 51353,
            59155, 61398, 71336, 102723, 103215, 103217, 103235, 104079, 105606, 105763, 105772, 105792, 105795, 105816, 105828,
            105829, 106426, 106561, 106569, 106593, 106841, 106862, 107507, 107662, 107881, 107889, 107899, 108238, 108389, 108520,
            108536, 108543, 108550, 108556, 108560, 109122, 109123, 110959, 111243, 111338, 111345, 111372, 111530, 111535, 111566,
            113765, 120950, 121595, 121904, 121908, 121919, 121920, 121960, 122281, 122305, 123587, 123910, 123911, 123912, 123913,
            131202, 131504, 131640, 135016, 146534, 171491, 171501, 171502, 175354, 179772, 179776, 179778, 179779, 179780, 181767,
            181773, 181818, 181842, 181867, 181871, 181880, 182042, 182050, 182056, 182095, 182102, 182119, 182136, 182153, 182574,
            182576, 182603, 182608, 182610, 182612, 183977, 184445, 184585, 184968, 184999, 185025, 186469, 188447, 188484, 191204,
            193964, 193965, 193966, 193967, 193968, 193969, 193970, 193971, 193972, 193973, 194308, 194359, 216050, 251637, 348308,
            354714, 356987, 356991, 357125, 357569, 357846, 358018, 358120, 358358, 358653, 359900, 361799, 366173, 376693, 395431,
            404291, 404352, 404785, 404802, 405092, 405095, 405096, 405097, 405098, 405104, 426125, 426135, 432690, 432691, 432692,
            432693, 432694, 432695, 6483, 6487, 6494, 69308, 71643, 74042, 74056, 101441, 105501, 105502, 105812, 105953, 105955,
            105956, 105957, 105958, 105969, 105977, 106385, 106502, 106504, 106584, 106731, 106749, 107011, 107030, 107031, 107035,
            107067, 107107, 107112, 107114, 107162, 107223, 107265, 107705, 110714, 112311, 112327, 112338, 112345, 112347, 117574,
            134115, 134121, 134123, 134125, 134127, 134128, 140874, 144001, 182271, 182276, 182278, 182283, 186485, 193295, 206229,
            206230, 215811, 215813, 215814, 215815, 215816, 215817, 215818, 215819, 215820, 215822, 215835, 215841, 215844, 215847,
            215852, 218915, 218916, 251609, 251610, 314817, 362958, 362959, 3919, 4699, 4700, 4701, 4716, 4719, 4724, 4725, 4730,
            4731, 51327, 59822, 71909, 97399, 97458, 97558, 98829, 98835, 98836, 98871, 98883, 98940, 99063, 99096, 99694, 101550,
            110525, 110526, 110546, 110549, 117329, 120216, 120239, 120365, 121145, 122566, 123865, 123885, 128993, 129183, 129197,
            129329, 129333, 136022, 136893, 136925, 137408, 137413, 137491, 137525, 137527, 137528, 137567, 137572, 137656, 137675,
            137752, 137761, 137767, 137779, 137780, 137781, 137848, 137859, 137928, 137943, 137964, 137968, 137976, 138261, 139403,
            139419, 139431, 139455, 139780, 139869, 139889, 140271, 140312, 140779, 140878, 141074, 141081, 141143, 141175, 141186,
            141192, 141198, 141341, 141354, 141376, 141577, 141581, 141622, 141630, 141700, 141736, 141773, 141938, 142048, 142433,
            142478, 142503, 142514, 142719, 142737, 142788, 142826, 142845, 142851, 143216, 143225, 143273, 143445, 143504, 143509,
            143513, 143598, 143601, 143759, 143770, 143773, 143776, 143797, 143799, 143800, 143806, 143814, 143818, 143996, 144045,
            144046, 144100, 144199, 144209, 144214, 144218, 144234, 144461, 144765, 144770, 144773, 144774, 144775, 144776, 144785,
            144786, 144788, 144789, 144790, 144791, 144832, 144837, 144838, 144840, 144841, 144842, 144843, 144891, 144949, 144950,
            144952, 144953, 144954, 144955, 144974, 145010, 145025, 145028, 145089, 145195, 145196, 145295, 145310, 145461, 145485,
            145503, 145541, 145659, 145685, 145709, 145715, 146041, 146048, 146502, 146593, 146596, 147043, 147136, 147227, 147239,
            147257, 147934, 147935, 147936, 147937, 147938, 149848, 149849, 149851, 150410, 150411, 150412, 150413, 150414, 150415,
            150416, 150417, 150418, 150419, 168878, 169077, 169123, 169890, 169891, 169904, 169905, 169906, 169907, 169908, 169909,
            170972, 171008, 172170, 172187, 172191, 172193, 172489, 176921, 176924, 176955, 181336, 181431, 181568, 182353, 182360,
            182365, 182370, 182384, 191350, 192095, 192103, 197887, 202172, 213766, 215087, 215205, 215635, 218913, 218914, 223650,
            224033, 224150, 224172, 247403, 247405, 247407, 249990, 249994, 305732, 305733, 305734, 305830, 305831, 305832, 317351,
            317352, 317353, 317354, 317355, 317356, 319337, 319583, 319776, 320135, 320136, 323124, 357875, 357880, 357881, 358124,
            358125, 358126, 358128, 358129, 362956, 362957, 363236, 363237, 366341, 367774, 374071, 374080, 374084, 376290, 391711,
            391762, 392434, 392477, 392611, 392620, 395888, 395892, 396441, 396442, 396443, 396444, 396445, 396650, 409250, 409362,
            409385, 409486, 409487, 409488, 409528, 409533, 409641, 409688, 409705, 409708, 409710, 409745, 409748, 409852, 409853,
            409858, 409861, 409930, 410107, 410215, 426074, 426080, 426081, 426083, 426091, 426092, 426095, 426103, 426106, 426107,
            426110, 426121, 426123,
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
