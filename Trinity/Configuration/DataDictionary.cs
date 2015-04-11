using System;
using System.Collections.Generic;
using System.Linq;
using Trinity.Objects;
using Trinity.Reference;
using Zeta.Common;
using Zeta.Game;
using Zeta.Game.Internals.Actors;

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
        private static readonly HashSet<int> bossLevelAreaIDs = new HashSet<int> { 
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
            332335, // Navigator is very low here (25/30ms by move)
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

        public static Dictionary<SNOAnim, Anim> AvoidanceAnimations { get { return avoidAnimationsIds; } }
        private static readonly Dictionary<SNOAnim, Anim> avoidAnimationsIds = new Dictionary<SNOAnim, Anim>
        {
            { SNOAnim.a2dun_Cald_Belial_Acid_Attack_Action, new Anim(SNOAnim.a2dun_Cald_Belial_Acid_Attack_Action, AvoidType.Attack, Element.Poison) },
            { SNOAnim.a2dun_Cave_Larva_explode_01, new Anim(SNOAnim.a2dun_Cave_Larva_explode_01, AvoidType.Bomb, Element.Physical) },
            { SNOAnim.a2dun_Cave_SlimeGeyser_A_attack_0, new Anim(SNOAnim.a2dun_Cave_SlimeGeyser_A_attack_0, AvoidType.Attack, Element.Physical) },
            { SNOAnim.a2dun_Zolt_Tesla_Tower_Attack, new Anim(SNOAnim.a2dun_Zolt_Tesla_Tower_Attack, AvoidType.Attack, Element.Lightning) },
            { SNOAnim.a3_Battlefield_Demoic_Forge_Atacking, new Anim(SNOAnim.a3_Battlefield_Demoic_Forge_Atacking, AvoidType.Projectile, Element.Fire) },
            { SNOAnim.a3dun_crater_st_demonic_forge_Atacking_2, new Anim(SNOAnim.a3dun_crater_st_demonic_forge_Atacking_2, AvoidType.Projectile, Element.Fire) },
            { SNOAnim.a4dun_Garden_Corruption_Mine_attack, new Anim(SNOAnim.a4dun_Garden_Corruption_Mine_attack, AvoidType.GroundCircle, Element.Physical) },
            { SNOAnim.a4dun_Spire_Ground_Attack_A_Action, new Anim(SNOAnim.a4dun_Spire_Ground_Attack_A_Action, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Adria_fireball_cast, new Anim(SNOAnim.Adria_fireball_cast, AvoidType.Projectile, Element.Fire) },
            { SNOAnim.Angel_Corrupt_attack_01, new Anim(SNOAnim.Angel_Corrupt_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Angel_Corrupt_attack_dash_in, new Anim(SNOAnim.Angel_Corrupt_attack_dash_in, AvoidType.Dash, Element.Physical) },
            { SNOAnim.Angel_Corrupt_attack_dash_middle, new Anim(SNOAnim.Angel_Corrupt_attack_dash_middle, AvoidType.Dash, Element.Physical) },
            { SNOAnim.Angel_Corrupt_attack_dash_out, new Anim(SNOAnim.Angel_Corrupt_attack_dash_out, AvoidType.Dash, Element.Physical) },
            { SNOAnim.Angel_Corrupt_generic_cast_01, new Anim(SNOAnim.Angel_Corrupt_generic_cast_01, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.arcaneEnchantedAvengerDummy_turnLeft, new Anim(SNOAnim.arcaneEnchantedAvengerDummy_turnLeft, AvoidType.RotateLeft, Element.Physical, customRadius: 40f) },
            { SNOAnim.arcaneEnchantedDummy_60ft_base, new Anim(SNOAnim.arcaneEnchantedDummy_60ft_base, AvoidType.GroundCircle, Element.Physical, customRadius: 60f) },
            { SNOAnim.arcaneEnchantedDummy_60ft_turnLeft, new Anim(SNOAnim.arcaneEnchantedDummy_60ft_turnLeft, AvoidType.RotateLeft, Element.Physical, customRadius: 60f) },
            { SNOAnim.arcaneEnchantedDummy_60ft_turnRight, new Anim(SNOAnim.arcaneEnchantedDummy_60ft_turnRight, AvoidType.RotateRight, Element.Physical, customRadius: 60f) },
            { SNOAnim.arcaneEnchantedDummy_turnLeft, new Anim(SNOAnim.arcaneEnchantedDummy_turnLeft, AvoidType.RotateLeft, Element.Physical, customRadius: 40f) },
            { SNOAnim.arcaneEnchantedDummy_turnRight, new Anim(SNOAnim.arcaneEnchantedDummy_turnRight, AvoidType.RotateRight, Element.Physical, customRadius: 40f) },
            { SNOAnim.AssaultBeast_Land_attack_BoulderToss, new Anim(SNOAnim.AssaultBeast_Land_attack_BoulderToss, AvoidType.Attack, Element.Physical) },
            { SNOAnim.AssaultBeast_Land_attack_BullCharge_in, new Anim(SNOAnim.AssaultBeast_Land_attack_BullCharge_in, AvoidType.Charge, Element.Physical) },
            { SNOAnim.AssaultBeast_Land_attack_BullCharge_middle, new Anim(SNOAnim.AssaultBeast_Land_attack_BullCharge_middle, AvoidType.Charge, Element.Physical) },
            { SNOAnim.AssaultBeast_Land_attack_BullCharge_out, new Anim(SNOAnim.AssaultBeast_Land_attack_BullCharge_out, AvoidType.Charge, Element.Physical) },
            { SNOAnim.AssaultBeast_Land_Attack_Grab_Begin, new Anim(SNOAnim.AssaultBeast_Land_Attack_Grab_Begin, AvoidType.Grab, Element.Physical) },
            { SNOAnim.AssaultBeast_Land_Attack_Grab_Bite, new Anim(SNOAnim.AssaultBeast_Land_Attack_Grab_Bite, AvoidType.Grab, Element.Physical) },
            { SNOAnim.AssaultBeast_Land_Attack_Grab_Idle, new Anim(SNOAnim.AssaultBeast_Land_Attack_Grab_Idle, AvoidType.Grab, Element.Physical) },
            { SNOAnim.AssaultBeast_Land_Attack_Grab_Slam, new Anim(SNOAnim.AssaultBeast_Land_Attack_Grab_Slam, AvoidType.Grab, Element.Physical) },
            { SNOAnim.AssaultBeast_Land_attack_GravelBlasts, new Anim(SNOAnim.AssaultBeast_Land_attack_GravelBlasts, AvoidType.Bomb, Element.Physical) },
            { SNOAnim.AssaultBeast_Land_attack_Mini3Charges, new Anim(SNOAnim.AssaultBeast_Land_attack_Mini3Charges, AvoidType.Charge, Element.Physical) },
            { SNOAnim.AssaultBeast_Land_attack_PunchThruWallGrab_01, new Anim(SNOAnim.AssaultBeast_Land_attack_PunchThruWallGrab_01, AvoidType.Grab, Element.Physical) },
            { SNOAnim.AssaultBeast_Land_attack_sideswipe_left, new Anim(SNOAnim.AssaultBeast_Land_attack_sideswipe_left, AvoidType.AttackLeft, Element.Physical) },
            { SNOAnim.AssaultBeast_Land_attack_sideswipe_right, new Anim(SNOAnim.AssaultBeast_Land_attack_sideswipe_right, AvoidType.AttackRight, Element.Physical) },
            { SNOAnim.AssaultBeast_Land_attack_stomp, new Anim(SNOAnim.AssaultBeast_Land_attack_stomp, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Azmodan_Attack_01, new Anim(SNOAnim.Azmodan_Attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Azmodan_SpellCast, new Anim(SNOAnim.Azmodan_SpellCast, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.Azmodan_Spellcast_02, new Anim(SNOAnim.Azmodan_Spellcast_02, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.azmodanBodyguard_attack_01, new Anim(SNOAnim.azmodanBodyguard_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.azmodanBodyguard_generic_cast, new Anim(SNOAnim.azmodanBodyguard_generic_cast, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.azmodanBodyguard_strafe_l, new Anim(SNOAnim.azmodanBodyguard_strafe_l, AvoidType.Strafe, Element.Physical) },
            { SNOAnim.azmodanBodyguard_strafe_r, new Anim(SNOAnim.azmodanBodyguard_strafe_r, AvoidType.Strafe, Element.Physical) },
            { SNOAnim.Bat_attack_01, new Anim(SNOAnim.Bat_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Bat_generic_cast, new Anim(SNOAnim.Bat_generic_cast, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.Beast_attack_01, new Anim(SNOAnim.Beast_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Beast_cast_01, new Anim(SNOAnim.Beast_cast_01, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.Beast_charge_02, new Anim(SNOAnim.Beast_charge_02, AvoidType.Charge, Element.Physical) },
            { SNOAnim.Beast_charge_04, new Anim(SNOAnim.Beast_charge_04, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Beast_start_charge_02, new Anim(SNOAnim.Beast_start_charge_02, AvoidType.Charge, Element.Physical) },
            { SNOAnim.Belial_AOE_Intro_01, new Anim(SNOAnim.Belial_AOE_Intro_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Belial_AOE_Loop_01, new Anim(SNOAnim.Belial_AOE_Loop_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Belial_AOE_Outro_01, new Anim(SNOAnim.Belial_AOE_Outro_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Belial_attack_far_01, new Anim(SNOAnim.Belial_attack_far_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Belial_attack_near_01, new Anim(SNOAnim.Belial_attack_near_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Belial_combo_attack_01, new Anim(SNOAnim.Belial_combo_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Belial_spit_attack_01, new Anim(SNOAnim.Belial_spit_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Belial_spit_attack_reverse_01, new Anim(SNOAnim.Belial_spit_attack_reverse_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Belial_TrueForm_attack_01, new Anim(SNOAnim.Belial_TrueForm_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Belial_TrueForm_teleport_01, new Anim(SNOAnim.Belial_TrueForm_teleport_01, AvoidType.Teleport, Element.Physical) },
            { SNOAnim.BigRed_attack_02, new Anim(SNOAnim.BigRed_attack_02, AvoidType.Attack, Element.Physical) },
            { SNOAnim.BigRed_charge_01, new Anim(SNOAnim.BigRed_charge_01, AvoidType.Charge, Element.Physical) },
            { SNOAnim.BigRed_firebreath_combo_01, new Anim(SNOAnim.BigRed_firebreath_combo_01, AvoidType.Attack, Element.Fire) },
            { SNOAnim.BigRed_firebreath_intro_01, new Anim(SNOAnim.BigRed_firebreath_intro_01, AvoidType.Attack, Element.Fire) },
            { SNOAnim.BigRed_firebreath_loop_01, new Anim(SNOAnim.BigRed_firebreath_loop_01, AvoidType.Attack, Element.Fire) },
            { SNOAnim.BigRed_firebreath_outro_01, new Anim(SNOAnim.BigRed_firebreath_outro_01, AvoidType.Attack, Element.Fire) },
            { SNOAnim.BigRed_generic_cast_01, new Anim(SNOAnim.BigRed_generic_cast_01, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.Bloodhawk_Attack_01, new Anim(SNOAnim.Bloodhawk_Attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Bloodhawk_generic_cast_01, new Anim(SNOAnim.Bloodhawk_generic_cast_01, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.Boar_attack_01, new Anim(SNOAnim.Boar_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Boar_attack_02, new Anim(SNOAnim.Boar_attack_02, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Brickhouse_attack_01, new Anim(SNOAnim.Brickhouse_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Brickhouse_generic_cast, new Anim(SNOAnim.Brickhouse_generic_cast, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.Brickhouse_shuffle_01, new Anim(SNOAnim.Brickhouse_shuffle_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Brickhouse_shuffle_left, new Anim(SNOAnim.Brickhouse_shuffle_left, AvoidType.AttackLeft, Element.Physical) },
            { SNOAnim.Brickhouse_shuffle_right, new Anim(SNOAnim.Brickhouse_shuffle_right, AvoidType.AttackRight, Element.Physical) },
            { SNOAnim.Brickhouse_special_attack_01, new Anim(SNOAnim.Brickhouse_special_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Butcher_Attack_05_telegraph, new Anim(SNOAnim.Butcher_Attack_05_telegraph, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Butcher_Attack_Chain_01_in, new Anim(SNOAnim.Butcher_Attack_Chain_01_in, AvoidType.Projectile, Element.Physical) },
            { SNOAnim.Butcher_Attack_Chain_01_out, new Anim(SNOAnim.Butcher_Attack_Chain_01_out, AvoidType.Projectile, Element.Physical) },
            { SNOAnim.Butcher_Attack_Charge_01_in, new Anim(SNOAnim.Butcher_Attack_Charge_01_in, AvoidType.Charge, Element.Physical) },
            { SNOAnim.Butcher_Attack_FanOfChains, new Anim(SNOAnim.Butcher_Attack_FanOfChains, AvoidType.Projectile, Element.Physical) },
            { SNOAnim.Butcher_BreakFree_attack_01_begin, new Anim(SNOAnim.Butcher_BreakFree_attack_01_begin, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Butcher_BreakFree_attack_01_middle, new Anim(SNOAnim.Butcher_BreakFree_attack_01_middle, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Butcher_BreakFree_attack_01_out, new Anim(SNOAnim.Butcher_BreakFree_attack_01_out, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Butcher_BreakFree_attack_02_begin, new Anim(SNOAnim.Butcher_BreakFree_attack_02_begin, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Butcher_BreakFree_attack_02_middle, new Anim(SNOAnim.Butcher_BreakFree_attack_02_middle, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Butcher_BreakFree_attack_02_out, new Anim(SNOAnim.Butcher_BreakFree_attack_02_out, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Butcher_BreakFree_attack_03, new Anim(SNOAnim.Butcher_BreakFree_attack_03, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Butcher_BreakFree_attack_GrillLift, new Anim(SNOAnim.Butcher_BreakFree_attack_GrillLift, AvoidType.Attack, Element.Physical) },
            { SNOAnim.caldeumChild_Male_Kyla_attack_01, new Anim(SNOAnim.caldeumChild_Male_Kyla_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.caldeumGuard_Spear__Attack_01, new Anim(SNOAnim.caldeumGuard_Spear__Attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.caldeumGuard_Spear__Attack_02_doorBash, new Anim(SNOAnim.caldeumGuard_Spear__Attack_02_doorBash, AvoidType.Attack, Element.Physical) },
            { SNOAnim.caOut_Oasis_Attack_Plant_Attack, new Anim(SNOAnim.caOut_Oasis_Attack_Plant_Attack, AvoidType.Attack, Element.Physical) },
            { SNOAnim.coreEliteDemon_attack_01, new Anim(SNOAnim.coreEliteDemon_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.coreEliteDemon_generic_Cast_01, new Anim(SNOAnim.coreEliteDemon_generic_Cast_01, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.corpseSpider_momma_attack_01, new Anim(SNOAnim.corpseSpider_momma_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.corpseSpider_momma_generic_cast_01, new Anim(SNOAnim.corpseSpider_momma_generic_cast_01, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.creepMob_attack_01, new Anim(SNOAnim.creepMob_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.creepMob_attack_02, new Anim(SNOAnim.creepMob_attack_02, AvoidType.Attack, Element.Physical) },
            { SNOAnim.creepMob_attack_03, new Anim(SNOAnim.creepMob_attack_03, AvoidType.Attack, Element.Physical) },
            { SNOAnim.creepMob_attack_04_in, new Anim(SNOAnim.creepMob_attack_04_in, AvoidType.Attack, Element.Physical) },
            { SNOAnim.creepMob_attack_04_middle, new Anim(SNOAnim.creepMob_attack_04_middle, AvoidType.Attack, Element.Physical) },
            { SNOAnim.creepMob_attack_04_out, new Anim(SNOAnim.creepMob_attack_04_out, AvoidType.Attack, Element.Physical) },
            { SNOAnim.creepMob_generic_cast, new Anim(SNOAnim.creepMob_generic_cast, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.creepMobArm_attack_01_range_spit, new Anim(SNOAnim.creepMobArm_attack_01_range_spit, AvoidType.Attack, Element.Physical) },
            { SNOAnim.creepMobArm_attack_02, new Anim(SNOAnim.creepMobArm_attack_02, AvoidType.Attack, Element.Physical) },
            { SNOAnim.creepMobArm_Attack_AOE_01_Intro, new Anim(SNOAnim.creepMobArm_Attack_AOE_01_Intro, AvoidType.Attack, Element.Physical) },
            { SNOAnim.creepMobArm_Attack_AOE_01_Middle, new Anim(SNOAnim.creepMobArm_Attack_AOE_01_Middle, AvoidType.Attack, Element.Physical) },
            { SNOAnim.creepMobArm_Attack_AOE_01_Outtro, new Anim(SNOAnim.creepMobArm_Attack_AOE_01_Outtro, AvoidType.Attack, Element.Physical) },
            { SNOAnim.CritterRat_attack_01, new Anim(SNOAnim.CritterRat_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.CritterRat_jump_attack_intro, new Anim(SNOAnim.CritterRat_jump_attack_intro, AvoidType.Attack, Element.Physical) },
            { SNOAnim.CritterRat_jump_attack_middle, new Anim(SNOAnim.CritterRat_jump_attack_middle, AvoidType.Attack, Element.Physical) },
            { SNOAnim.CritterRat_jump_attack_outro, new Anim(SNOAnim.CritterRat_jump_attack_outro, AvoidType.Attack, Element.Physical) },
            { SNOAnim.CryptChild_Attack, new Anim(SNOAnim.CryptChild_Attack, AvoidType.Attack, Element.Physical) },
            { SNOAnim.CryptChild_generic_cast, new Anim(SNOAnim.CryptChild_generic_cast, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.CryptChild_unburrow, new Anim(SNOAnim.CryptChild_unburrow, AvoidType.Attack, Element.Physical) },
            { SNOAnim.DemonFetus_attack_01, new Anim(SNOAnim.DemonFetus_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.DemonFetus_attack_02, new Anim(SNOAnim.DemonFetus_attack_02, AvoidType.Attack, Element.Physical) },
            { SNOAnim.DemonFetus_attack_03, new Anim(SNOAnim.DemonFetus_attack_03, AvoidType.Attack, Element.Physical) },
            { SNOAnim.demonFlyer_attack_01, new Anim(SNOAnim.demonFlyer_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.demonFlyer_cast_01, new Anim(SNOAnim.demonFlyer_cast_01, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.demonFlyer_crash_in_place, new Anim(SNOAnim.demonFlyer_crash_in_place, AvoidType.Attack, Element.Physical) },
            { SNOAnim.demonFlyer_kamikaze, new Anim(SNOAnim.demonFlyer_kamikaze, AvoidType.Attack, Element.Physical) },
            { SNOAnim.demonFlyer_Mega_attack_01, new Anim(SNOAnim.demonFlyer_Mega_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.demonTrooper_attack_01, new Anim(SNOAnim.demonTrooper_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.demonTrooper_attack_01_loop, new Anim(SNOAnim.demonTrooper_attack_01_loop, AvoidType.Attack, Element.Physical) },
            { SNOAnim.demonTrooper_generic_cast, new Anim(SNOAnim.demonTrooper_generic_cast, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.DeSoto_Attack_Punch, new Anim(SNOAnim.DeSoto_Attack_Punch, AvoidType.Attack, Element.Physical) },
            { SNOAnim.DeSoto_Attack_Spellcast, new Anim(SNOAnim.DeSoto_Attack_Spellcast, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.DeSoto_Run_Charge, new Anim(SNOAnim.DeSoto_Run_Charge, AvoidType.Charge, Element.Physical) },
            { SNOAnim.DeSoto_Run_Charge_Attack, new Anim(SNOAnim.DeSoto_Run_Charge_Attack, AvoidType.Charge, Element.Physical) },
            { SNOAnim.Diablo_Attack_01, new Anim(SNOAnim.Diablo_Attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Diablo_beatdown_end, new Anim(SNOAnim.Diablo_beatdown_end, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Diablo_beatdown_ending, new Anim(SNOAnim.Diablo_beatdown_ending, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Diablo_beatdown_loop_01, new Anim(SNOAnim.Diablo_beatdown_loop_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Diablo_beatdown_loop_02, new Anim(SNOAnim.Diablo_beatdown_loop_02, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Diablo_beatdown_transition_01, new Anim(SNOAnim.Diablo_beatdown_transition_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Diablo_beatdown_transition_02, new Anim(SNOAnim.Diablo_beatdown_transition_02, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Diablo_beatdown_transition_ending, new Anim(SNOAnim.Diablo_beatdown_transition_ending, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Diablo_charge, new Anim(SNOAnim.Diablo_charge, AvoidType.Charge, Element.Physical) },
            { SNOAnim.Diablo_charge_attack, new Anim(SNOAnim.Diablo_charge_attack, AvoidType.Charge, Element.Physical) },
            { SNOAnim.Diablo_Corruption, new Anim(SNOAnim.Diablo_Corruption, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Diablo_Corruption_Float_loop, new Anim(SNOAnim.Diablo_Corruption_Float_loop, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Diablo_Corruption_Float_out, new Anim(SNOAnim.Diablo_Corruption_Float_out, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Diablo_FireBreath_End, new Anim(SNOAnim.Diablo_FireBreath_End, AvoidType.Attack, Element.Fire) },
            { SNOAnim.Diablo_FireBreath_Loop, new Anim(SNOAnim.Diablo_FireBreath_Loop, AvoidType.Attack, Element.Fire) },
            { SNOAnim.Diablo_FireBreath_Start, new Anim(SNOAnim.Diablo_FireBreath_Start, AvoidType.Attack, Element.Fire) },
            { SNOAnim.Diablo_FireColumn, new Anim(SNOAnim.Diablo_FireColumn, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Diablo_ground_stomp, new Anim(SNOAnim.Diablo_ground_stomp, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Diablo_LightningBreath_Cast, new Anim(SNOAnim.Diablo_LightningBreath_Cast, AvoidType.GenericCast, Element.Lightning) },
            { SNOAnim.Diablo_Ring_of_fire, new Anim(SNOAnim.Diablo_Ring_of_fire, AvoidType.Attack, Element.Fire) },
            { SNOAnim.Diablo_ringofFire_cast, new Anim(SNOAnim.Diablo_ringofFire_cast, AvoidType.Attack, Element.Fire) },
            { SNOAnim.Diablo_Roar_stage1_Cast, new Anim(SNOAnim.Diablo_Roar_stage1_Cast, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.DuneDervish_Ambush, new Anim(SNOAnim.DuneDervish_Ambush, AvoidType.Attack, Element.Physical) },
            { SNOAnim.DuneDervish_attack_intro, new Anim(SNOAnim.DuneDervish_attack_intro, AvoidType.Attack, Element.Physical) },
            { SNOAnim.DuneDervish_attack_loop, new Anim(SNOAnim.DuneDervish_attack_loop, AvoidType.Attack, Element.Physical) },
            { SNOAnim.DuneDervish_attack_outro, new Anim(SNOAnim.DuneDervish_attack_outro, AvoidType.Attack, Element.Physical) },
            { SNOAnim.DuneDervish_generic_cast, new Anim(SNOAnim.DuneDervish_generic_cast, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.DuneDervish_specialTeleport, new Anim(SNOAnim.DuneDervish_specialTeleport, AvoidType.Teleport, Element.Physical) },
            { SNOAnim.DuneDervish_strafe_l, new Anim(SNOAnim.DuneDervish_strafe_l, AvoidType.Strafe, Element.Physical) },
            { SNOAnim.DuneDervish_strafe_r, new Anim(SNOAnim.DuneDervish_strafe_r, AvoidType.Strafe, Element.Physical) },
            { SNOAnim.FallenChampion_attack_01, new Anim(SNOAnim.FallenChampion_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.FallenChampion_battle_cry_01, new Anim(SNOAnim.FallenChampion_battle_cry_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.FallenChampion_battle_cry_no_powers, new Anim(SNOAnim.FallenChampion_battle_cry_no_powers, AvoidType.Attack, Element.Physical) },
            { SNOAnim.FallenChampion_battle_cry_no_powers_loop, new Anim(SNOAnim.FallenChampion_battle_cry_no_powers_loop, AvoidType.Attack, Element.Physical) },
            { SNOAnim.FallenChampion_cast_01, new Anim(SNOAnim.FallenChampion_cast_01, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.FallenGrunt_attack_01, new Anim(SNOAnim.FallenGrunt_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.FallenGrunt_generic_cast, new Anim(SNOAnim.FallenGrunt_generic_cast, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.FallenGrunt_resurrect_01, new Anim(SNOAnim.FallenGrunt_resurrect_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.FallenGrunt_resurrect_02, new Anim(SNOAnim.FallenGrunt_resurrect_02, AvoidType.Attack, Element.Physical) },
            { SNOAnim.FallenHound_attack_01, new Anim(SNOAnim.FallenHound_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.FallenHound_attack_02, new Anim(SNOAnim.FallenHound_attack_02, AvoidType.Attack, Element.Physical) },
            { SNOAnim.FallenHound_generic_cast, new Anim(SNOAnim.FallenHound_generic_cast, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.FallenLunatic_attack_01, new Anim(SNOAnim.FallenLunatic_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.FallenLunatic_B_attack_01, new Anim(SNOAnim.FallenLunatic_B_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.FallenLunatic_C_attack_01, new Anim(SNOAnim.FallenLunatic_C_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.FallenLunatic_generic_cast, new Anim(SNOAnim.FallenLunatic_generic_cast, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.FallenShaman_cast_direct_01, new Anim(SNOAnim.FallenShaman_cast_direct_01, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.fastMummy_attack_01, new Anim(SNOAnim.fastMummy_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.fastMummy_generic_cast, new Anim(SNOAnim.fastMummy_generic_cast, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Ferret_attack_01, new Anim(SNOAnim.Ferret_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.FleshPitFlyer_Attack_Poison, new Anim(SNOAnim.FleshPitFlyer_Attack_Poison, AvoidType.Attack, Element.Poison) },
            { SNOAnim.FleshPitFlyer_Attack_Swoop_down_like_dat, new Anim(SNOAnim.FleshPitFlyer_Attack_Swoop_down_like_dat, AvoidType.Attack, Element.Physical) },
            { SNOAnim.FleshPitFlyer_generic_cast, new Anim(SNOAnim.FleshPitFlyer_generic_cast, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.FleshPitFlyer_Leoric_AttackOverride, new Anim(SNOAnim.FleshPitFlyer_Leoric_AttackOverride, AvoidType.Attack, Element.Physical) },
            { SNOAnim.FleshPitFlyer_Ranged_Attack, new Anim(SNOAnim.FleshPitFlyer_Ranged_Attack, AvoidType.Attack, Element.Physical) },
            { SNOAnim.FloaterDemon_attack_01, new Anim(SNOAnim.FloaterDemon_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.FloaterDemon_attack_02_middle, new Anim(SNOAnim.FloaterDemon_attack_02_middle, AvoidType.Attack, Element.Physical) },
            { SNOAnim.FloaterDemon_attack_03, new Anim(SNOAnim.FloaterDemon_attack_03, AvoidType.Attack, Element.Physical) },
            { SNOAnim.FloaterDemon_attack_charge_01, new Anim(SNOAnim.FloaterDemon_attack_charge_01, AvoidType.Charge, Element.Physical) },
            { SNOAnim.FloaterDemon_attack_charge_in_01, new Anim(SNOAnim.FloaterDemon_attack_charge_in_01, AvoidType.Charge, Element.Physical) },
            { SNOAnim.FloaterDemon_attack_charge_out_01, new Anim(SNOAnim.FloaterDemon_attack_charge_out_01, AvoidType.Charge, Element.Physical) },
            { SNOAnim.FloaterDemon_generic_cast_01, new Anim(SNOAnim.FloaterDemon_generic_cast_01, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.Ghost_attack_01, new Anim(SNOAnim.Ghost_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Ghost_attack_02_SoulLeech_begin, new Anim(SNOAnim.Ghost_attack_02_SoulLeech_begin, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Ghost_attack_02_SoulLeech_loop, new Anim(SNOAnim.Ghost_attack_02_SoulLeech_loop, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Ghost_attack_02_SoulLeech_out, new Anim(SNOAnim.Ghost_attack_02_SoulLeech_out, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Ghoul_attack_03_Ambush, new Anim(SNOAnim.Ghoul_attack_03_Ambush, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Ghoul_attack_04_Ambush, new Anim(SNOAnim.Ghoul_attack_04_Ambush, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Ghoul_attack_Slash_01, new Anim(SNOAnim.Ghoul_attack_Slash_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Ghoul_attack_Sword_Slap_01, new Anim(SNOAnim.Ghoul_attack_Sword_Slap_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Ghoul_attack_WhirlWind_01, new Anim(SNOAnim.Ghoul_attack_WhirlWind_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Ghoul_spellcast_01, new Anim(SNOAnim.Ghoul_spellcast_01, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.Gluttony_Attack_AreaEffect, new Anim(SNOAnim.Gluttony_Attack_AreaEffect, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Gluttony_Attack_Chomp, new Anim(SNOAnim.Gluttony_Attack_Chomp, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Gluttony_attack_ranged_01, new Anim(SNOAnim.Gluttony_attack_ranged_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Gluttony_Attack_Sneeze, new Anim(SNOAnim.Gluttony_Attack_Sneeze, AvoidType.Attack, Element.Physical) },
            { SNOAnim.GoatMutant_Melee_attack_01, new Anim(SNOAnim.GoatMutant_Melee_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.GoatMutant_Melee_attack_03, new Anim(SNOAnim.GoatMutant_Melee_attack_03, AvoidType.Attack, Element.Physical) },
            { SNOAnim.GoatMutant_Ranged_attack_01, new Anim(SNOAnim.GoatMutant_Ranged_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.GoatMutant_Ranged_attack_06, new Anim(SNOAnim.GoatMutant_Ranged_attack_06, AvoidType.Attack, Element.Physical) },
            { SNOAnim.GoatMutant_Ranged_Attack_Comp_Multi_01, new Anim(SNOAnim.GoatMutant_Ranged_Attack_Comp_Multi_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.GoatMutant_Ranged_cast_01, new Anim(SNOAnim.GoatMutant_Ranged_cast_01, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.GoatMutant_Shaman_Attack_Spellcast_01, new Anim(SNOAnim.GoatMutant_Shaman_Attack_Spellcast_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.GoatMutant_Shaman_Attack_Spellcast_02, new Anim(SNOAnim.GoatMutant_Shaman_Attack_Spellcast_02, AvoidType.Attack, Element.Physical) },
            { SNOAnim.GoatMutant_Shaman_Attack_Spellcast_03, new Anim(SNOAnim.GoatMutant_Shaman_Attack_Spellcast_03, AvoidType.Attack, Element.Physical) },
            { SNOAnim.GoatMutant_Shaman_melee_attack_01, new Anim(SNOAnim.GoatMutant_Shaman_melee_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.GoatWarrior_attack_01, new Anim(SNOAnim.GoatWarrior_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.GoatWarrior_attack_03_spear_throw, new Anim(SNOAnim.GoatWarrior_attack_03_spear_throw, AvoidType.Attack, Element.Physical) },
            { SNOAnim.GoatWarrior_attack_04, new Anim(SNOAnim.GoatWarrior_attack_04, AvoidType.Attack, Element.Physical) },
            { SNOAnim.GoatWarrior_generic_cast, new Anim(SNOAnim.GoatWarrior_generic_cast, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.GoatWarrior_Shaman_Cast_direct, new Anim(SNOAnim.GoatWarrior_Shaman_Cast_direct, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.GoatWarrior_Shaman_Cast_Special, new Anim(SNOAnim.GoatWarrior_Shaman_Cast_Special, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.GoatWarrior_Shaman_generic_cast, new Anim(SNOAnim.GoatWarrior_Shaman_generic_cast, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.GoatWarrior_Shaman_melee_attack_01, new Anim(SNOAnim.GoatWarrior_Shaman_melee_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Gorehound_attack_02, new Anim(SNOAnim.Gorehound_attack_02, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Gorehound_attack_warcry_01, new Anim(SNOAnim.Gorehound_attack_warcry_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.grateArmMonster_attack_01_loop, new Anim(SNOAnim.grateArmMonster_attack_01_loop, AvoidType.Attack, Element.Physical) },
            { SNOAnim.graveDigger_attack_01, new Anim(SNOAnim.graveDigger_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.graveRobber_attack_01, new Anim(SNOAnim.graveRobber_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.graveRobber_attack_throw_02, new Anim(SNOAnim.graveRobber_attack_throw_02, AvoidType.Attack, Element.Physical) },
            { SNOAnim.graveRobber_generic_cast_01, new Anim(SNOAnim.graveRobber_generic_cast_01, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.hoodedNightmare_SpellCast_01, new Anim(SNOAnim.hoodedNightmare_SpellCast_01, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.hoodedNightmare_SpellCast_BoneArmor, new Anim(SNOAnim.hoodedNightmare_SpellCast_BoneArmor, AvoidType.Attack, Element.Physical) },
            { SNOAnim.hoodedNightmare_SpellCast_Direct, new Anim(SNOAnim.hoodedNightmare_SpellCast_Direct, AvoidType.Attack, Element.Physical) },
            { SNOAnim.hoodedNightmare_SpellCast_Direct_Firewall, new Anim(SNOAnim.hoodedNightmare_SpellCast_Direct_Firewall, AvoidType.GenericCast, Element.Fire) },
            { SNOAnim.hoodedNightmare_SpellCast_Direct_Lightning, new Anim(SNOAnim.hoodedNightmare_SpellCast_Direct_Lightning, AvoidType.GenericCast, Element.Lightning) },
            { SNOAnim.Imperius_Imperius_1HS_attack_01, new Anim(SNOAnim.Imperius_Imperius_1HS_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Imperius_Imperius_1HS_Cleave, new Anim(SNOAnim.Imperius_Imperius_1HS_Cleave, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Imperius_Imperius_attack_cast, new Anim(SNOAnim.Imperius_Imperius_attack_cast, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Imperius_Imperius_attack_slam, new Anim(SNOAnim.Imperius_Imperius_attack_slam, AvoidType.Attack, Element.Physical) },
            { SNOAnim.lacuniFemale_attack_01, new Anim(SNOAnim.lacuniFemale_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.lacuniFemale_attack_02_throw, new Anim(SNOAnim.lacuniFemale_attack_02_throw, AvoidType.Attack, Element.Physical) },
            { SNOAnim.lacuniFemale_attack_03, new Anim(SNOAnim.lacuniFemale_attack_03, AvoidType.Attack, Element.Physical) },
            { SNOAnim.lacuniFemale_attack_05_Leap_intro, new Anim(SNOAnim.lacuniFemale_attack_05_Leap_intro, AvoidType.Leap, Element.Physical) },
            { SNOAnim.lacuniFemale_attack_05_Leap_intro_grass, new Anim(SNOAnim.lacuniFemale_attack_05_Leap_intro_grass, AvoidType.Leap, Element.Physical) },
            { SNOAnim.lacuniFemale_attack_05_Leap_intro_snow, new Anim(SNOAnim.lacuniFemale_attack_05_Leap_intro_snow, AvoidType.Leap, Element.Physical) },
            { SNOAnim.lacuniFemale_attack_05_Leap_middle, new Anim(SNOAnim.lacuniFemale_attack_05_Leap_middle, AvoidType.Leap, Element.Physical) },
            { SNOAnim.lacuniFemale_attack_05_Leap_out, new Anim(SNOAnim.lacuniFemale_attack_05_Leap_out, AvoidType.Leap, Element.Physical) },
            { SNOAnim.lacuniFemale_attack_05_Leap_out_grass, new Anim(SNOAnim.lacuniFemale_attack_05_Leap_out_grass, AvoidType.Leap, Element.Physical) },
            { SNOAnim.lacuniFemale_attack_05_Leap_out_snow, new Anim(SNOAnim.lacuniFemale_attack_05_Leap_out_snow, AvoidType.Leap, Element.Physical) },
            { SNOAnim.lacuniFemale_generic_cast, new Anim(SNOAnim.lacuniFemale_generic_cast, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.lacuniMale_attack_01, new Anim(SNOAnim.lacuniMale_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.lacuniMale_attack_combo, new Anim(SNOAnim.lacuniMale_attack_combo, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Lamprey_attack_01, new Anim(SNOAnim.Lamprey_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.lordOfDespair_Attack_EnergyBlast, new Anim(SNOAnim.lordOfDespair_Attack_EnergyBlast, AvoidType.Bomb, Element.Physical) },
            { SNOAnim.lordOfDespair_Attack_Ranged, new Anim(SNOAnim.lordOfDespair_Attack_Ranged, AvoidType.Attack, Element.Physical) },
            { SNOAnim.lordOfDespair_Attack_Stab, new Anim(SNOAnim.lordOfDespair_Attack_Stab, AvoidType.Attack, Element.Physical) },
            { SNOAnim.lordOfDespair_Attack_Teleport_End, new Anim(SNOAnim.lordOfDespair_Attack_Teleport_End, AvoidType.Teleport, Element.Physical) },
            { SNOAnim.lordOfDespair_Attack_Teleport_Full, new Anim(SNOAnim.lordOfDespair_Attack_Teleport_Full, AvoidType.Teleport, Element.Physical) },
            { SNOAnim.lordOfDespair_Attack_Teleport_Start, new Anim(SNOAnim.lordOfDespair_Attack_Teleport_Start, AvoidType.Teleport, Element.Physical) },
            { SNOAnim.lordOfDespair_Spellcast, new Anim(SNOAnim.lordOfDespair_Spellcast, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.lordOfDespair_Spellcast_360, new Anim(SNOAnim.lordOfDespair_Spellcast_360, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.lordOfDespair_teleport_outro, new Anim(SNOAnim.lordOfDespair_teleport_outro, AvoidType.Teleport, Element.Physical) },
            { SNOAnim.Maghda_cast_01, new Anim(SNOAnim.Maghda_cast_01, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.Maghda_cast_03, new Anim(SNOAnim.Maghda_cast_03, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.Maghda_cast_03_event19, new Anim(SNOAnim.Maghda_cast_03_event19, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.Maghda_cast_summon, new Anim(SNOAnim.Maghda_cast_summon, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Maghda_generic_cast_01, new Anim(SNOAnim.Maghda_generic_cast_01, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.Maghda_strafe_left_intro_01, new Anim(SNOAnim.Maghda_strafe_left_intro_01, AvoidType.StrafeLeft, Element.Physical) },
            { SNOAnim.Maghda_strafe_left_loop_01, new Anim(SNOAnim.Maghda_strafe_left_loop_01, AvoidType.StrafeLeft, Element.Physical) },
            { SNOAnim.Maghda_strafe_left_outro_01, new Anim(SNOAnim.Maghda_strafe_left_outro_01, AvoidType.StrafeLeft, Element.Physical) },
            { SNOAnim.Maghda_strafe_right_intro_01, new Anim(SNOAnim.Maghda_strafe_right_intro_01, AvoidType.StrafeRight, Element.Physical) },
            { SNOAnim.Maghda_strafe_right_loop_01, new Anim(SNOAnim.Maghda_strafe_right_loop_01, AvoidType.StrafeRight, Element.Physical) },
            { SNOAnim.Maghda_strafe_right_outro_01, new Anim(SNOAnim.Maghda_strafe_right_outro_01, AvoidType.StrafeRight, Element.Physical) },
            { SNOAnim.Maghda_teleport, new Anim(SNOAnim.Maghda_teleport, AvoidType.Teleport, Element.Physical) },
            { SNOAnim.malletDemon_attack_01, new Anim(SNOAnim.malletDemon_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.malletDemon_generic_cast, new Anim(SNOAnim.malletDemon_generic_cast, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.malletDemon_strafe_L_01, new Anim(SNOAnim.malletDemon_strafe_L_01, AvoidType.Strafe, Element.Physical) },
            { SNOAnim.malletDemon_strafe_R_01, new Anim(SNOAnim.malletDemon_strafe_R_01, AvoidType.Strafe, Element.Physical) },
            { SNOAnim.MastaBlasta_Combined_attack_ranged, new Anim(SNOAnim.MastaBlasta_Combined_attack_ranged, AvoidType.Attack, Element.Physical) },
            { SNOAnim.MastaBlasta_Combined_attack_stomp, new Anim(SNOAnim.MastaBlasta_Combined_attack_stomp, AvoidType.Attack, Element.Physical) },
            { SNOAnim.MastaBlasta_Combined_generic_cast, new Anim(SNOAnim.MastaBlasta_Combined_generic_cast, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.MastaBlasta_Rider_Attack_01, new Anim(SNOAnim.MastaBlasta_Rider_Attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.MastaBlasta_Rider_Attack_Exacute, new Anim(SNOAnim.MastaBlasta_Rider_Attack_Exacute, AvoidType.Attack, Element.Physical) },
            { SNOAnim.MastaBlasta_Rider_SpellCast_01, new Anim(SNOAnim.MastaBlasta_Rider_SpellCast_01, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.MastaBlasta_Steed_attack_01, new Anim(SNOAnim.MastaBlasta_Steed_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.MastaBlasta_Steed_attack_stomp, new Anim(SNOAnim.MastaBlasta_Steed_attack_stomp, AvoidType.Attack, Element.Physical) },
            { SNOAnim.MastaBlasta_Steed_generic_cast, new Anim(SNOAnim.MastaBlasta_Steed_generic_cast, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.MistressofPain_Attack_01, new Anim(SNOAnim.MistressofPain_Attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.MistressofPain_Attack_SpellCast_Poison, new Anim(SNOAnim.MistressofPain_Attack_SpellCast_Poison, AvoidType.GenericCast, Element.Poison) },
            { SNOAnim.monsterAffix_frozen_bomb_iceBall_explosion, new Anim(SNOAnim.monsterAffix_frozen_bomb_iceBall_explosion, AvoidType.Bomb, Element.Cold) },
            { SNOAnim.monsterAffix_frozen_bomb_iceBall_start, new Anim(SNOAnim.monsterAffix_frozen_bomb_iceBall_start, AvoidType.Bomb, Element.Cold) },
            { SNOAnim.Monstrosity_Scorpion_attack_02_attack01, new Anim(SNOAnim.Monstrosity_Scorpion_attack_02_attack01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Monstrosity_Scorpion_generic_cast, new Anim(SNOAnim.Monstrosity_Scorpion_generic_cast, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.morluMelee_attack_01, new Anim(SNOAnim.morluMelee_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.morluMelee_cast_01, new Anim(SNOAnim.morluMelee_cast_01, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.morluSpellcaster_attack_02, new Anim(SNOAnim.morluSpellcaster_attack_02, AvoidType.Attack, Element.Physical) },
            { SNOAnim.morluSpellcaster_attack_02_uber, new Anim(SNOAnim.morluSpellcaster_attack_02_uber, AvoidType.Attack, Element.Physical) },
            { SNOAnim.morluSpellcaster_attack_AOE_01, new Anim(SNOAnim.morluSpellcaster_attack_AOE_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.morluSpellcaster_attack_AOE_01_uber, new Anim(SNOAnim.morluSpellcaster_attack_AOE_01_uber, AvoidType.Attack, Element.Physical) },
            { SNOAnim.morluSpellcaster_attack_melee_01, new Anim(SNOAnim.morluSpellcaster_attack_melee_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.morluSpellcaster_generic_cast, new Anim(SNOAnim.morluSpellcaster_generic_cast, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.morluSpellcaster_idle_01, new Anim(SNOAnim.morluSpellcaster_idle_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.morluSpellcaster_teleport_full, new Anim(SNOAnim.morluSpellcaster_teleport_full, AvoidType.Teleport, Element.Physical) },
            { SNOAnim.morluSpellcaster_teleport_intro, new Anim(SNOAnim.morluSpellcaster_teleport_intro, AvoidType.Teleport, Element.Physical) },
            { SNOAnim.morluSpellcaster_teleport_outro, new Anim(SNOAnim.morluSpellcaster_teleport_outro, AvoidType.Teleport, Element.Physical) },
            { SNOAnim.orb_norm_unique_02_base, new Anim(SNOAnim.orb_norm_unique_02_base, AvoidType.Attack, Element.Physical) },
            { SNOAnim.p1_Greed_Attack_05_telegraph, new Anim(SNOAnim.p1_Greed_Attack_05_telegraph, AvoidType.Attack, Element.Physical) },
            { SNOAnim.p1_Greed_Attack_Chain_01_in, new Anim(SNOAnim.p1_Greed_Attack_Chain_01_in, AvoidType.Projectile, Element.Physical) },
            { SNOAnim.p1_Greed_Attack_Chain_01_out, new Anim(SNOAnim.p1_Greed_Attack_Chain_01_out, AvoidType.Projectile, Element.Physical) },
            { SNOAnim.p1_Greed_Attack_Charge_01_in, new Anim(SNOAnim.p1_Greed_Attack_Charge_01_in, AvoidType.Charge, Element.Physical) },
            { SNOAnim.p1_Greed_Attack_FanOfChains, new Anim(SNOAnim.p1_Greed_Attack_FanOfChains, AvoidType.Projectile, Element.Physical) },
            { SNOAnim.p1_Greed_BreakFree_attack_01_begin, new Anim(SNOAnim.p1_Greed_BreakFree_attack_01_begin, AvoidType.Attack, Element.Physical) },
            { SNOAnim.p1_Greed_BreakFree_attack_01_middle, new Anim(SNOAnim.p1_Greed_BreakFree_attack_01_middle, AvoidType.Attack, Element.Physical) },
            { SNOAnim.p1_Greed_BreakFree_attack_01_out, new Anim(SNOAnim.p1_Greed_BreakFree_attack_01_out, AvoidType.Attack, Element.Physical) },
            { SNOAnim.p1_Greed_BreakFree_attack_02_begin, new Anim(SNOAnim.p1_Greed_BreakFree_attack_02_begin, AvoidType.Attack, Element.Physical) },
            { SNOAnim.p1_Greed_BreakFree_attack_02_middle, new Anim(SNOAnim.p1_Greed_BreakFree_attack_02_middle, AvoidType.Attack, Element.Physical) },
            { SNOAnim.p1_Greed_BreakFree_attack_02_outt, new Anim(SNOAnim.p1_Greed_BreakFree_attack_02_outt, AvoidType.Attack, Element.Physical) },
            { SNOAnim.p1_Greed_BreakFree_attack_03, new Anim(SNOAnim.p1_Greed_BreakFree_attack_03, AvoidType.Attack, Element.Physical) },
            { SNOAnim.p1_Greed_BreakFree_attack_GrillLift, new Anim(SNOAnim.p1_Greed_BreakFree_attack_GrillLift, AvoidType.Attack, Element.Physical) },
            { SNOAnim.p1_Greed_BreakFree_charge_01, new Anim(SNOAnim.p1_Greed_BreakFree_charge_01, AvoidType.Charge, Element.Physical) },
            { SNOAnim.p1_TreasureTrooper_attack_01, new Anim(SNOAnim.p1_TreasureTrooper_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.p1_TreasureTrooper_attack_01_loop, new Anim(SNOAnim.p1_TreasureTrooper_attack_01_loop, AvoidType.Attack, Element.Physical) },
            { SNOAnim.p1_TreasureTrooper_generic_cast, new Anim(SNOAnim.p1_TreasureTrooper_generic_cast, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.Primordial_attack_01_FireBreath, new Anim(SNOAnim.Primordial_attack_01_FireBreath, AvoidType.Attack, Element.Fire) },
            { SNOAnim.QuillDemon_Attack_01, new Anim(SNOAnim.QuillDemon_Attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.QuillDemon_Ranged_Attack, new Anim(SNOAnim.QuillDemon_Ranged_Attack, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Raven_pet_attack_01, new Anim(SNOAnim.Raven_pet_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Rockworm_attack_01, new Anim(SNOAnim.Rockworm_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Rockworm_generic_cast_01, new Anim(SNOAnim.Rockworm_generic_cast_01, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.sandMonster_attack_01, new Anim(SNOAnim.sandMonster_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.sandMonster_attack_02, new Anim(SNOAnim.sandMonster_attack_02, AvoidType.Attack, Element.Physical) },
            { SNOAnim.sandMonster_attack_03_sandwall, new Anim(SNOAnim.sandMonster_attack_03_sandwall, AvoidType.Attack, Element.Physical) },
            { SNOAnim.sandMonster_generic_cast, new Anim(SNOAnim.sandMonster_generic_cast, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.sandMonsterBlack_attack_03_sandwall, new Anim(SNOAnim.sandMonsterBlack_attack_03_sandwall, AvoidType.Attack, Element.Physical) },
            { SNOAnim.SandShark_attack_submerged, new Anim(SNOAnim.SandShark_attack_submerged, AvoidType.Attack, Element.Physical) },
            { SNOAnim.SandShark_attack_submerged_end, new Anim(SNOAnim.SandShark_attack_submerged_end, AvoidType.Attack, Element.Physical) },
            { SNOAnim.SandShark_attack_submerged_start, new Anim(SNOAnim.SandShark_attack_submerged_start, AvoidType.Attack, Element.Physical) },
            { SNOAnim.SandShark_cast_01, new Anim(SNOAnim.SandShark_cast_01, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.SandShark_melee_attack_01, new Anim(SNOAnim.SandShark_melee_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.sandWasp_attack_01, new Anim(SNOAnim.sandWasp_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.sandWasp_attack_02, new Anim(SNOAnim.sandWasp_attack_02, AvoidType.Attack, Element.Physical) },
            { SNOAnim.sandWasp_attack_intro_01, new Anim(SNOAnim.sandWasp_attack_intro_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.sandWasp_attack_loop_01, new Anim(SNOAnim.sandWasp_attack_loop_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.sandWasp_attack_outro_01, new Anim(SNOAnim.sandWasp_attack_outro_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.sandWasp_attack_ranged_01, new Anim(SNOAnim.sandWasp_attack_ranged_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.sandWasp_attack_ranged_02, new Anim(SNOAnim.sandWasp_attack_ranged_02, AvoidType.Attack, Element.Physical) },
            { SNOAnim.sandWasp_cast_01, new Anim(SNOAnim.sandWasp_cast_01, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.Scavenger_attack_01, new Anim(SNOAnim.Scavenger_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Scavenger_generic_cast, new Anim(SNOAnim.Scavenger_generic_cast, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.Scoundrel_attack_01, new Anim(SNOAnim.Scoundrel_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Scoundrel_attack_02, new Anim(SNOAnim.Scoundrel_attack_02, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Scoundrel_BOW_attack_01, new Anim(SNOAnim.Scoundrel_BOW_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Scoundrel_BOW_attack_02, new Anim(SNOAnim.Scoundrel_BOW_attack_02, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Scoundrel_BOW_special_attack_01, new Anim(SNOAnim.Scoundrel_BOW_special_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Scoundrel_BOW_special_attack_02, new Anim(SNOAnim.Scoundrel_BOW_special_attack_02, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Scoundrel_HTH_attack_01, new Anim(SNOAnim.Scoundrel_HTH_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Scoundrel_HTH_special_attack_01, new Anim(SNOAnim.Scoundrel_HTH_special_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Scoundrel_special_attack_01, new Anim(SNOAnim.Scoundrel_special_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Scoundrel_special_attack_02, new Anim(SNOAnim.Scoundrel_special_attack_02, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Scoundrel_special_attack_03, new Anim(SNOAnim.Scoundrel_special_attack_03, AvoidType.Attack, Element.Physical) },
            { SNOAnim.shadowVermin_attack_01, new Anim(SNOAnim.shadowVermin_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.shadowVermin_attack_hoodedNightmare_death, new Anim(SNOAnim.shadowVermin_attack_hoodedNightmare_death, AvoidType.Attack, Element.Physical) },
            { SNOAnim.shieldSkeleton_arcaneSummoned, new Anim(SNOAnim.shieldSkeleton_arcaneSummoned, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Skeleton_AttackNew, new Anim(SNOAnim.Skeleton_AttackNew, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Skeleton_generic_cast, new Anim(SNOAnim.Skeleton_generic_cast, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.Skeleton_Shield_Sarcophagus_Attack_Intro_High, new Anim(SNOAnim.Skeleton_Shield_Sarcophagus_Attack_Intro_High, AvoidType.Attack, Element.Physical) },
            { SNOAnim.SkeletonArcher_Attack, new Anim(SNOAnim.SkeletonArcher_Attack, AvoidType.Attack, Element.Physical) },
            { SNOAnim.SkeletonArcher_Attack_Lightning, new Anim(SNOAnim.SkeletonArcher_Attack_Lightning, AvoidType.Attack, Element.Lightning) },
            { SNOAnim.skeletonArcher_generic_cast, new Anim(SNOAnim.skeletonArcher_generic_cast, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.SkeletonAxe_attack_BigSwing, new Anim(SNOAnim.SkeletonAxe_attack_BigSwing, AvoidType.Attack, Element.Physical) },
            { SNOAnim.SkeletonAxe_attack_new, new Anim(SNOAnim.SkeletonAxe_attack_new, AvoidType.Attack, Element.Physical) },
            { SNOAnim.SkeletonAxe_attack_OverHeadSwing, new Anim(SNOAnim.SkeletonAxe_attack_OverHeadSwing, AvoidType.Attack, Element.Physical) },
            { SNOAnim.SkeletonAxe_attack_whirlwind_begin, new Anim(SNOAnim.SkeletonAxe_attack_whirlwind_begin, AvoidType.Attack, Element.Physical) },
            { SNOAnim.SkeletonAxe_attack_whirlwind_middle, new Anim(SNOAnim.SkeletonAxe_attack_whirlwind_middle, AvoidType.Attack, Element.Physical) },
            { SNOAnim.SkeletonAxe_attack_whirlwind_middle_looping, new Anim(SNOAnim.SkeletonAxe_attack_whirlwind_middle_looping, AvoidType.Attack, Element.Physical) },
            { SNOAnim.SkeletonAxe_attack_whirlwind_out, new Anim(SNOAnim.SkeletonAxe_attack_whirlwind_out, AvoidType.Attack, Element.Physical) },
            { SNOAnim.skeletonAxe_generic_cast, new Anim(SNOAnim.skeletonAxe_generic_cast, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.SkeletonAxe_knockbak, new Anim(SNOAnim.SkeletonAxe_knockbak, AvoidType.Attack, Element.Physical) },
            { SNOAnim.SkeletonKing_Attack_01, new Anim(SNOAnim.SkeletonKing_Attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.SkeletonKing_Attack_01_attackImage, new Anim(SNOAnim.SkeletonKing_Attack_01_attackImage, AvoidType.Attack, Element.Physical) },
            { SNOAnim.SkeletonKing_Attack_02, new Anim(SNOAnim.SkeletonKing_Attack_02, AvoidType.Attack, Element.Physical) },
            { SNOAnim.SkeletonKing_Attack_02_attackImage, new Anim(SNOAnim.SkeletonKing_Attack_02_attackImage, AvoidType.Attack, Element.Physical) },
            { SNOAnim.SkeletonKing_Cast_Summon, new Anim(SNOAnim.SkeletonKing_Cast_Summon, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.SkeletonKing_ghost_spellcast, new Anim(SNOAnim.SkeletonKing_ghost_spellcast, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.SkeletonKing_Teleport, new Anim(SNOAnim.SkeletonKing_Teleport, AvoidType.Teleport, Element.Physical) },
            { SNOAnim.SkeletonKing_Teleport_Attack, new Anim(SNOAnim.SkeletonKing_Teleport_Attack, AvoidType.Teleport, Element.Physical) },
            { SNOAnim.SkeletonKing_teleport_back, new Anim(SNOAnim.SkeletonKing_teleport_back, AvoidType.Teleport, Element.Physical) },
            { SNOAnim.SkeletonKing_teleport_back_pose, new Anim(SNOAnim.SkeletonKing_teleport_back_pose, AvoidType.Teleport, Element.Physical) },
            { SNOAnim.SkeletonKing_teleport_pose, new Anim(SNOAnim.SkeletonKing_teleport_pose, AvoidType.Teleport, Element.Physical) },
            { SNOAnim.SkeletonKing_Whirlwind_end, new Anim(SNOAnim.SkeletonKing_Whirlwind_end, AvoidType.Attack, Element.Physical) },
            { SNOAnim.SkeletonKing_Whirlwind_loop, new Anim(SNOAnim.SkeletonKing_Whirlwind_loop, AvoidType.Attack, Element.Physical) },
            { SNOAnim.SkeletonKing_Whirlwind_loop_FX, new Anim(SNOAnim.SkeletonKing_Whirlwind_loop_FX, AvoidType.Attack, Element.Physical) },
            { SNOAnim.SkeletonKing_Whirlwind_start, new Anim(SNOAnim.SkeletonKing_Whirlwind_start, AvoidType.Attack, Element.Physical) },
            { SNOAnim.skeletonMage_Attack_01, new Anim(SNOAnim.skeletonMage_Attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.skeletonMage_Generic_Cast, new Anim(SNOAnim.skeletonMage_Generic_Cast, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.skeletonMage_SpellCast_Fire, new Anim(SNOAnim.skeletonMage_SpellCast_Fire, AvoidType.Projectile, Element.Fire) },
            { SNOAnim.skeletonMage_SpellCast_IceBolt, new Anim(SNOAnim.skeletonMage_SpellCast_IceBolt, AvoidType.Projectile, Element.Cold) },
            { SNOAnim.skeletonMage_SpellCast_LightningBolt, new Anim(SNOAnim.skeletonMage_SpellCast_LightningBolt, AvoidType.Projectile, Element.Lightning) },
            { SNOAnim.skeletonMage_SpellCast_Poison, new Anim(SNOAnim.skeletonMage_SpellCast_Poison, AvoidType.Projectile, Element.Poison) },
            { SNOAnim.SkeletonSummoner_attack_01, new Anim(SNOAnim.SkeletonSummoner_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.SkeletonSummoner_generic_cast, new Anim(SNOAnim.SkeletonSummoner_generic_cast, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.SkeletonSummoner_melee_attack_01, new Anim(SNOAnim.SkeletonSummoner_melee_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.SkeletonSummoner_show_n_tell_attack, new Anim(SNOAnim.SkeletonSummoner_show_n_tell_attack, AvoidType.Attack, Element.Physical) },
            { SNOAnim.SkeletonSummoner_specialAttack_01, new Anim(SNOAnim.SkeletonSummoner_specialAttack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.snakeMan_Caster_cast_AOE_01, new Anim(SNOAnim.snakeMan_Caster_cast_AOE_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.snakeMan_Caster_cast_electric_intro, new Anim(SNOAnim.snakeMan_Caster_cast_electric_intro, AvoidType.Attack, Element.Lightning) },
            { SNOAnim.snakeMan_Caster_cast_electric_mid, new Anim(SNOAnim.snakeMan_Caster_cast_electric_mid, AvoidType.Attack, Element.Lightning) },
            { SNOAnim.snakeMan_Caster_cast_electric_out, new Anim(SNOAnim.snakeMan_Caster_cast_electric_out, AvoidType.Attack, Element.Lightning) },
            { SNOAnim.snakeMan_Caster_generic_cast, new Anim(SNOAnim.snakeMan_Caster_generic_cast, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.snakeMan_Caster_melee_attack_01, new Anim(SNOAnim.snakeMan_Caster_melee_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.snakeMan_melee_attack_01, new Anim(SNOAnim.snakeMan_melee_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.SoulRipper_attack_01, new Anim(SNOAnim.SoulRipper_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.SoulRipper_attack_02_tongue, new Anim(SNOAnim.SoulRipper_attack_02_tongue, AvoidType.Attack, Element.Physical) },
            { SNOAnim.SoulRipper_attack_04, new Anim(SNOAnim.SoulRipper_attack_04, AvoidType.Attack, Element.Physical) },
            { SNOAnim.SoulRipper_attack_tongue_latch_01, new Anim(SNOAnim.SoulRipper_attack_tongue_latch_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.SoulRipper_attack_tongue_latch_01_end, new Anim(SNOAnim.SoulRipper_attack_tongue_latch_01_end, AvoidType.Attack, Element.Physical) },
            { SNOAnim.SoulRipper_attack_tongue_whip_01, new Anim(SNOAnim.SoulRipper_attack_tongue_whip_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.SoulRipper_generic_cast_01, new Anim(SNOAnim.SoulRipper_generic_cast_01, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.Spider_attack_02, new Anim(SNOAnim.Spider_attack_02, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Spider_attack_02_smallSpider, new Anim(SNOAnim.Spider_attack_02_smallSpider, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Spider_attack_bite_01, new Anim(SNOAnim.Spider_attack_bite_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Spider_attack_bite_01_smallSpider, new Anim(SNOAnim.Spider_attack_bite_01_smallSpider, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Spider_attack_egg_03, new Anim(SNOAnim.Spider_attack_egg_03, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Spider_attack_egg_03_smallSpider, new Anim(SNOAnim.Spider_attack_egg_03_smallSpider, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Spider_attack_lob, new Anim(SNOAnim.Spider_attack_lob, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Spider_attack_lob_smallSpider, new Anim(SNOAnim.Spider_attack_lob_smallSpider, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Spider_attack_vomit, new Anim(SNOAnim.Spider_attack_vomit, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Spider_attack_vomit_smallSpider, new Anim(SNOAnim.Spider_attack_vomit_smallSpider, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Spider_generic_cast, new Anim(SNOAnim.Spider_generic_cast, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.Spider_spiderbomb, new Anim(SNOAnim.Spider_spiderbomb, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Spiderling_attack_01, new Anim(SNOAnim.Spiderling_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Spiderling_attack_cleave, new Anim(SNOAnim.Spiderling_attack_cleave, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Spiderling_attack_leap_air, new Anim(SNOAnim.Spiderling_attack_leap_air, AvoidType.Leap, Element.Physical) },
            { SNOAnim.Spiderling_attack_leap_intro, new Anim(SNOAnim.Spiderling_attack_leap_intro, AvoidType.Leap, Element.Physical) },
            { SNOAnim.Spiderling_attack_leap_land, new Anim(SNOAnim.Spiderling_attack_leap_land, AvoidType.Leap, Element.Physical) },
            { SNOAnim.Spiderling_generic_cast_01, new Anim(SNOAnim.Spiderling_generic_cast_01, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.SpiderQueen_attack_02, new Anim(SNOAnim.SpiderQueen_attack_02, AvoidType.Attack, Element.Physical) },
            { SNOAnim.SpiderQueen_attack_bite_01, new Anim(SNOAnim.SpiderQueen_attack_bite_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.SpiderQueen_attack_egg_03, new Anim(SNOAnim.SpiderQueen_attack_egg_03, AvoidType.Attack, Element.Physical) },
            { SNOAnim.SpiderQueen_generic_cast, new Anim(SNOAnim.SpiderQueen_generic_cast, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.Stitch_Attack_Belly, new Anim(SNOAnim.Stitch_Attack_Belly, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Stitch_Attack_BunnySlap, new Anim(SNOAnim.Stitch_Attack_BunnySlap, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Stitch_Attack_Punch, new Anim(SNOAnim.Stitch_Attack_Punch, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Stitch_Attack_Push, new Anim(SNOAnim.Stitch_Attack_Push, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Stitch_generic_cast, new Anim(SNOAnim.Stitch_generic_cast, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.Stitch_Suicide_Bomb, new Anim(SNOAnim.Stitch_Suicide_Bomb, AvoidType.Bomb, Element.Physical, customRadius: 25f) },
            { SNOAnim.Stitch_Suicide_Bomb_Frost, new Anim(SNOAnim.Stitch_Suicide_Bomb_Frost, AvoidType.Bomb, Element.Physical, customRadius: 25f) },
            { SNOAnim.Stitch_Suicide_Bomb_Imps, new Anim(SNOAnim.Stitch_Suicide_Bomb_Imps, AvoidType.Bomb, Element.Physical, customRadius: 25f) },
            { SNOAnim.Stitch_Suicide_Bomb_spiders, new Anim(SNOAnim.Stitch_Suicide_Bomb_spiders, AvoidType.Bomb, Element.Physical, customRadius: 25f) },
            { SNOAnim.Succubus_attack_cast_01, new Anim(SNOAnim.Succubus_attack_cast_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Succubus_attack_melee_01, new Anim(SNOAnim.Succubus_attack_melee_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Succubus_generic_cast_01, new Anim(SNOAnim.Succubus_generic_cast_01, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.Swarm_attack_01, new Anim(SNOAnim.Swarm_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.TentacleBear_Attack, new Anim(SNOAnim.TentacleBear_Attack, AvoidType.Attack, Element.Physical) },
            { SNOAnim.tentacleFlower_attack_01, new Anim(SNOAnim.tentacleFlower_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.TentacleHorse_Attack_01, new Anim(SNOAnim.TentacleHorse_Attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.TentacleHorse_Attack_02, new Anim(SNOAnim.TentacleHorse_Attack_02, AvoidType.Attack, Element.Physical) },
            { SNOAnim.TentacleHorse_Charge_Intro_01, new Anim(SNOAnim.TentacleHorse_Charge_Intro_01, AvoidType.Charge, Element.Physical) },
            { SNOAnim.TentacleHorse_Charge_Loop_01, new Anim(SNOAnim.TentacleHorse_Charge_Loop_01, AvoidType.Charge, Element.Physical) },
            { SNOAnim.TentacleHorse_Charge_Loop_Outro_01, new Anim(SNOAnim.TentacleHorse_Charge_Loop_Outro_01, AvoidType.Charge, Element.Physical) },
            { SNOAnim.TerrorDemon_attack_01, new Anim(SNOAnim.TerrorDemon_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.TerrorDemon_attack_01_Uber, new Anim(SNOAnim.TerrorDemon_attack_01_Uber, AvoidType.Attack, Element.Physical) },
            { SNOAnim.TerrorDemon_attack_combo, new Anim(SNOAnim.TerrorDemon_attack_combo, AvoidType.Attack, Element.Physical) },
            { SNOAnim.TerrorDemon_attack_combo_Uber, new Anim(SNOAnim.TerrorDemon_attack_combo_Uber, AvoidType.Attack, Element.Physical) },
            { SNOAnim.TerrorDemon_earthquake_cast_uber, new Anim(SNOAnim.TerrorDemon_earthquake_cast_uber, AvoidType.Attack, Element.Physical) },
            { SNOAnim.TerrorDemon_generic_cast, new Anim(SNOAnim.TerrorDemon_generic_cast, AvoidType.Attack, Element.Physical) },
            { SNOAnim.TerrorDemon_teleport_intro, new Anim(SNOAnim.TerrorDemon_teleport_intro, AvoidType.Teleport, Element.Physical) },
            { SNOAnim.ThousandPounder_attack_01, new Anim(SNOAnim.ThousandPounder_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.ThousandPounder_attack_02, new Anim(SNOAnim.ThousandPounder_attack_02, AvoidType.Attack, Element.Physical) },
            { SNOAnim.ThousandPounder_attack_03, new Anim(SNOAnim.ThousandPounder_attack_03, AvoidType.Attack, Element.Physical) },
            { SNOAnim.ThousandPounder_attack_04, new Anim(SNOAnim.ThousandPounder_attack_04, AvoidType.Attack, Element.Physical) },
            { SNOAnim.ThousandPounder_generic_cast, new Anim(SNOAnim.ThousandPounder_generic_cast, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.Tornado_model_idle_01, new Anim(SNOAnim.Tornado_model_idle_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Triune_Berserker_attack_01, new Anim(SNOAnim.Triune_Berserker_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Triune_Berserker_attack_02, new Anim(SNOAnim.Triune_Berserker_attack_02, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Triune_Berserker_generic_cast, new Anim(SNOAnim.Triune_Berserker_generic_cast, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.Triune_Berserker_specialAttack_01, new Anim(SNOAnim.Triune_Berserker_specialAttack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Triune_Berserker_specialAttack_end_01, new Anim(SNOAnim.Triune_Berserker_specialAttack_end_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Triune_Berserker_specialAttack_loop_01, new Anim(SNOAnim.Triune_Berserker_specialAttack_loop_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Triune_Possessed_attack_01, new Anim(SNOAnim.Triune_Possessed_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Triune_Possessed_attack_02_end, new Anim(SNOAnim.Triune_Possessed_attack_02_end, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Triune_Possessed_attack_02_middle, new Anim(SNOAnim.Triune_Possessed_attack_02_middle, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Triune_Summonable_attack_01, new Anim(SNOAnim.Triune_Summonable_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Triune_Summonable_attack_02, new Anim(SNOAnim.Triune_Summonable_attack_02, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Triune_Summonable_generic_cast, new Anim(SNOAnim.Triune_Summonable_generic_cast, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.TriuneCultist_attack_01, new Anim(SNOAnim.TriuneCultist_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.TriuneCultist_generic_cast, new Anim(SNOAnim.TriuneCultist_generic_cast, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.TriuneSummoner_Attack_01, new Anim(SNOAnim.TriuneSummoner_Attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.TriuneSummoner_generic_cast, new Anim(SNOAnim.TriuneSummoner_generic_cast, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.TriuneSummoner_SpellCast_01, new Anim(SNOAnim.TriuneSummoner_SpellCast_01, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.TriuneVessel_generic_cast, new Anim(SNOAnim.TriuneVessel_generic_cast, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.Unburied_attack_01, new Anim(SNOAnim.Unburied_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Unburied_attack_02, new Anim(SNOAnim.Unburied_attack_02, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Unburied_attack_cleave, new Anim(SNOAnim.Unburied_attack_cleave, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Unburied_generic_cast, new Anim(SNOAnim.Unburied_generic_cast, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.WitherMoth_attack_01, new Anim(SNOAnim.WitherMoth_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.WitherMoth_ranged_attack_01, new Anim(SNOAnim.WitherMoth_ranged_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.WoodWraith_Attack_01, new Anim(SNOAnim.WoodWraith_Attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.WoodWraith_Attack_left, new Anim(SNOAnim.WoodWraith_Attack_left, AvoidType.AttackLeft, Element.Physical) },
            { SNOAnim.WoodWraith_Attack_right, new Anim(SNOAnim.WoodWraith_Attack_right, AvoidType.AttackRight, Element.Physical) },
            { SNOAnim.WoodWraith_Attack_Root, new Anim(SNOAnim.WoodWraith_Attack_Root, AvoidType.Attack, Element.Physical) },
            { SNOAnim.WoodWraith_attack_spore, new Anim(SNOAnim.WoodWraith_attack_spore, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_Adria_AOE_01, new Anim(SNOAnim.x1_Adria_AOE_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_Adria_Attack_Left_01, new Anim(SNOAnim.x1_Adria_Attack_Left_01, AvoidType.AttackLeft, Element.Physical) },
            { SNOAnim.x1_Adria_Attack_Right_01, new Anim(SNOAnim.x1_Adria_Attack_Right_01, AvoidType.AttackRight, Element.Physical) },
            { SNOAnim.x1_Adria_bloodProjectile_base, new Anim(SNOAnim.x1_Adria_bloodProjectile_base, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_Adria_bloodProjectile_turn_01, new Anim(SNOAnim.x1_Adria_bloodProjectile_turn_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_Adria_bloodProjectile_walk_01, new Anim(SNOAnim.x1_Adria_bloodProjectile_walk_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_Adria_cast_01, new Anim(SNOAnim.x1_Adria_cast_01, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.x1_Adria_melee_01, new Anim(SNOAnim.x1_Adria_melee_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_Adria_teleport_attack_01, new Anim(SNOAnim.x1_Adria_teleport_attack_01, AvoidType.Teleport, Element.Physical) },
            { SNOAnim.x1_Adria_teleport_cast_01, new Anim(SNOAnim.x1_Adria_teleport_cast_01, AvoidType.Teleport, Element.Physical) },
            { SNOAnim.x1_adriaMaggot_attack_01, new Anim(SNOAnim.x1_adriaMaggot_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_ArmorScavenger_BiPed_melee_01, new Anim(SNOAnim.x1_ArmorScavenger_BiPed_melee_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_ArmorScavenger_BiPed_temp_cast_01, new Anim(SNOAnim.x1_ArmorScavenger_BiPed_temp_cast_01, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.X1_BigRed_attack_02, new Anim(SNOAnim.X1_BigRed_attack_02, AvoidType.Attack, Element.Physical) },
            { SNOAnim.X1_BigRed_charge_01, new Anim(SNOAnim.X1_BigRed_charge_01, AvoidType.Charge, Element.Physical) },
            { SNOAnim.X1_BigRed_firebreath_combo_01, new Anim(SNOAnim.X1_BigRed_firebreath_combo_01, AvoidType.Attack, Element.Fire) },
            { SNOAnim.X1_BigRed_firebreath_intro_01, new Anim(SNOAnim.X1_BigRed_firebreath_intro_01, AvoidType.Attack, Element.Fire) },
            { SNOAnim.X1_BigRed_firebreath_loop_01, new Anim(SNOAnim.X1_BigRed_firebreath_loop_01, AvoidType.Attack, Element.Fire) },
            { SNOAnim.X1_BigRed_firebreath_outro_01, new Anim(SNOAnim.X1_BigRed_firebreath_outro_01, AvoidType.Attack, Element.Fire) },
            { SNOAnim.X1_BigRed_generic_cast_01, new Anim(SNOAnim.X1_BigRed_generic_cast_01, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.x1_BileCrawler_Skeletal_attack_01, new Anim(SNOAnim.x1_BileCrawler_Skeletal_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_BileCrawler_Skeletal_attack_02, new Anim(SNOAnim.x1_BileCrawler_Skeletal_attack_02, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_BileCrawler_Skeletal_attack_03, new Anim(SNOAnim.x1_BileCrawler_Skeletal_attack_03, AvoidType.Attack, Element.Physical) },
            { SNOAnim.X1_Bloodhawk_Attack_01, new Anim(SNOAnim.X1_Bloodhawk_Attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_Bloodhawk_divebomb_intro, new Anim(SNOAnim.x1_Bloodhawk_divebomb_intro, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_Bloodhawk_divebomb_mid, new Anim(SNOAnim.x1_Bloodhawk_divebomb_mid, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_Bloodhawk_divebomb_outro, new Anim(SNOAnim.x1_Bloodhawk_divebomb_outro, AvoidType.Attack, Element.Physical) },
            { SNOAnim.X1_Bloodhawk_generic_cast_01, new Anim(SNOAnim.X1_Bloodhawk_generic_cast_01, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.x1_Bog_Family_Guard_Tower_attack_0, new Anim(SNOAnim.x1_Bog_Family_Guard_Tower_attack_0, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_BogBlight_attack_melee_01, new Anim(SNOAnim.x1_BogBlight_attack_melee_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_BogBlight_generic_cast, new Anim(SNOAnim.x1_BogBlight_generic_cast, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.x1_BogBlight_shake_attack_long, new Anim(SNOAnim.x1_BogBlight_shake_attack_long, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_BogBlight_special_attack_01, new Anim(SNOAnim.x1_BogBlight_special_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_BogFamily_brute_attack_01, new Anim(SNOAnim.x1_BogFamily_brute_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_BogFamily_brute_charge_end_01, new Anim(SNOAnim.x1_BogFamily_brute_charge_end_01, AvoidType.Charge, Element.Physical) },
            { SNOAnim.x1_BogFamily_brute_charge_intro_01, new Anim(SNOAnim.x1_BogFamily_brute_charge_intro_01, AvoidType.Charge, Element.Physical) },
            { SNOAnim.x1_BogFamily_brute_charge_loop_01, new Anim(SNOAnim.x1_BogFamily_brute_charge_loop_01, AvoidType.Charge, Element.Physical) },
            { SNOAnim.x1_BogFamily_brute_generic_cast_01, new Anim(SNOAnim.x1_BogFamily_brute_generic_cast_01, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.x1_BogFamily_melee_generic_cast_01, new Anim(SNOAnim.x1_BogFamily_melee_generic_cast_01, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.x1_BogFamily_melee_melee__01, new Anim(SNOAnim.x1_BogFamily_melee_melee__01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_BogFamily_ranged_attack_dart_01, new Anim(SNOAnim.x1_BogFamily_ranged_attack_dart_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_BogFamily_ranged_attack_dart_tower_01, new Anim(SNOAnim.x1_BogFamily_ranged_attack_dart_tower_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_BogFamily_ranged_generic_cast_01, new Anim(SNOAnim.x1_BogFamily_ranged_generic_cast_01, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.x1_BogFamily_ranged_melee_attack_01, new Anim(SNOAnim.x1_BogFamily_ranged_melee_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_BogFamily_ranged_ranged_attack_01, new Anim(SNOAnim.x1_BogFamily_ranged_ranged_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_BogFamily_ranged_ranged_attack_02, new Anim(SNOAnim.x1_BogFamily_ranged_ranged_attack_02, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_BogFamily_ranged_ranged_attack_03, new Anim(SNOAnim.x1_BogFamily_ranged_ranged_attack_03, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_BogFamily_ranged_ranged_attack_tower_01, new Anim(SNOAnim.x1_BogFamily_ranged_ranged_attack_tower_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_coreEliteDemon_attack_01, new Anim(SNOAnim.x1_coreEliteDemon_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_coreEliteDemon_generic_Cast_01, new Anim(SNOAnim.x1_coreEliteDemon_generic_Cast_01, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.x1_dark_angel_attack_01, new Anim(SNOAnim.x1_dark_angel_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_dark_angel_attack_02, new Anim(SNOAnim.x1_dark_angel_attack_02, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_dark_Angel_attack_melee_01, new Anim(SNOAnim.x1_dark_Angel_attack_melee_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_dark_Angel_CAST, new Anim(SNOAnim.x1_dark_Angel_CAST, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_dark_Angel_generic_cast, new Anim(SNOAnim.x1_dark_Angel_generic_cast, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.x1_dark_Angel_teleport_intro, new Anim(SNOAnim.x1_dark_Angel_teleport_intro, AvoidType.Teleport, Element.Physical) },
            { SNOAnim.x1_dark_Angel_teleport_outro, new Anim(SNOAnim.x1_dark_Angel_teleport_outro, AvoidType.Teleport, Element.Physical) },
            { SNOAnim.x1_Dark_Ghost_attack_01, new Anim(SNOAnim.x1_Dark_Ghost_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_Dark_Ghost_attack_02_SoulLeech_begin, new Anim(SNOAnim.x1_Dark_Ghost_attack_02_SoulLeech_begin, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_Dark_Ghost_attack_02_SoulLeech_loop, new Anim(SNOAnim.x1_Dark_Ghost_attack_02_SoulLeech_loop, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_Dark_Ghost_attack_02_SoulLeech_out, new Anim(SNOAnim.x1_Dark_Ghost_attack_02_SoulLeech_out, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_Dark_Ghost_overlook_attack_01, new Anim(SNOAnim.x1_Dark_Ghost_overlook_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_deathMaiden_attack_01, new Anim(SNOAnim.x1_deathMaiden_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_deathMaiden_attack_04_AOE, new Anim(SNOAnim.x1_deathMaiden_attack_04_AOE, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_deathMaiden_attack_05_intro, new Anim(SNOAnim.x1_deathMaiden_attack_05_intro, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_deathMaiden_attack_05_middle, new Anim(SNOAnim.x1_deathMaiden_attack_05_middle, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_deathMaiden_attack_05_out, new Anim(SNOAnim.x1_deathMaiden_attack_05_out, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_deathMaiden_attack_castSpawingPool, new Anim(SNOAnim.x1_deathMaiden_attack_castSpawingPool, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_deathMaiden_attack_special_360_01, new Anim(SNOAnim.x1_deathMaiden_attack_special_360_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_deathMaiden_attack_special_flip_01, new Anim(SNOAnim.x1_deathMaiden_attack_special_flip_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_deathMaiden_fire_attack_01, new Anim(SNOAnim.x1_deathMaiden_fire_attack_01, AvoidType.Attack, Element.Fire) },
            { SNOAnim.x1_deathMaiden_fire_attack_03_sweep, new Anim(SNOAnim.x1_deathMaiden_fire_attack_03_sweep, AvoidType.Attack, Element.Fire) },
            { SNOAnim.x1_deathMaiden_fire_special_death_01, new Anim(SNOAnim.x1_deathMaiden_fire_special_death_01, AvoidType.Attack, Element.Fire) },
            { SNOAnim.x1_deathMaiden_fire_special_death_02, new Anim(SNOAnim.x1_deathMaiden_fire_special_death_02, AvoidType.Attack, Element.Fire) },
            { SNOAnim.x1_deathMaiden_temp_cast_01, new Anim(SNOAnim.x1_deathMaiden_temp_cast_01, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.x1_demonTrooper_attack_01, new Anim(SNOAnim.x1_demonTrooper_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_demonTrooper_attack_01_loop, new Anim(SNOAnim.x1_demonTrooper_attack_01_loop, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_Fortress_Ring_Hazard_attack, new Anim(SNOAnim.x1_Fortress_Ring_Hazard_attack, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_Fortress_Rotating_Door_attack, new Anim(SNOAnim.x1_Fortress_Rotating_Door_attack, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_Ghost_attack_01, new Anim(SNOAnim.x1_Ghost_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_Ghost_attack_02_SoulLeech_begin, new Anim(SNOAnim.x1_Ghost_attack_02_SoulLeech_begin, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_Ghost_attack_02_SoulLeech_loop, new Anim(SNOAnim.x1_Ghost_attack_02_SoulLeech_loop, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_Ghost_b_attack_01, new Anim(SNOAnim.x1_Ghost_b_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_Ghost_b_attack_02_SoulLeech_begin, new Anim(SNOAnim.x1_Ghost_b_attack_02_SoulLeech_begin, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_lacuniMale_plagued_attack_01, new Anim(SNOAnim.x1_lacuniMale_plagued_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_lacuniMale_plagued_attack_02, new Anim(SNOAnim.x1_lacuniMale_plagued_attack_02, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_lacuniMale_plagued_generic_cast, new Anim(SNOAnim.x1_lacuniMale_plagued_generic_cast, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.x1_LeaperAngel_GenericCast_01, new Anim(SNOAnim.x1_LeaperAngel_GenericCast_01, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.x1_LeaperAngel_LeapAttack_In, new Anim(SNOAnim.x1_LeaperAngel_LeapAttack_In, AvoidType.Leap, Element.Physical) },
            { SNOAnim.x1_LeaperAngel_LeapAttack_Mid, new Anim(SNOAnim.x1_LeaperAngel_LeapAttack_Mid, AvoidType.Leap, Element.Physical) },
            { SNOAnim.x1_LeaperAngel_LeapAttack_Out, new Anim(SNOAnim.x1_LeaperAngel_LeapAttack_Out, AvoidType.Leap, Element.Physical) },
            { SNOAnim.x1_LeaperAngel_LeapAttack_Out_rage, new Anim(SNOAnim.x1_LeaperAngel_LeapAttack_Out_rage, AvoidType.Leap, Element.Physical) },
            { SNOAnim.x1_LeaperAngel_Melee_01, new Anim(SNOAnim.x1_LeaperAngel_Melee_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_LeaperAngel_Melee_01_rage, new Anim(SNOAnim.x1_LeaperAngel_Melee_01_rage, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_LeaperAngel_Melee_AttackLong, new Anim(SNOAnim.x1_LeaperAngel_Melee_AttackLong, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_LeaperAngel_Melee_AttackLong_rage, new Anim(SNOAnim.x1_LeaperAngel_Melee_AttackLong_rage, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_Leoric_Deserters_attack_01, new Anim(SNOAnim.x1_Leoric_Deserters_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_LeoricDeserter_Archer_attack_01, new Anim(SNOAnim.x1_LeoricDeserter_Archer_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_LeoricDeserter_attack_stab, new Anim(SNOAnim.x1_LeoricDeserter_attack_stab, AvoidType.Attack, Element.Physical) },
            { SNOAnim.X1_LR_Boss_Angel_Corrupt_A_cast_01, new Anim(SNOAnim.X1_LR_Boss_Angel_Corrupt_A_cast_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.X1_LR_Boss_morluSpellcaster_generic_cast, new Anim(SNOAnim.X1_LR_Boss_morluSpellcaster_generic_cast, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.x1_Malthael_attack_slash_01, new Anim(SNOAnim.x1_Malthael_attack_slash_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_Malthael_gratesOfHell_cast_copies, new Anim(SNOAnim.x1_Malthael_gratesOfHell_cast_copies, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_Malthael_Mephis_FrozenOrb_Throw_01, new Anim(SNOAnim.x1_Malthael_Mephis_FrozenOrb_Throw_01, AvoidType.Attack, Element.Cold) },
            { SNOAnim.x1_Malthael_Mephis_FrozenOrbs_Vortex_01, new Anim(SNOAnim.x1_Malthael_Mephis_FrozenOrbs_Vortex_01, AvoidType.Attack, Element.Cold) },
            { SNOAnim.x1_Malthael_Spirit_AOE_01, new Anim(SNOAnim.x1_Malthael_Spirit_AOE_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_Malthael_summon_poison_pool_01, new Anim(SNOAnim.x1_Malthael_summon_poison_pool_01, AvoidType.GroundCircle, Element.Poison) },
            { SNOAnim.x1_Malthael_sword_shield_Shadow_attack_01_right, new Anim(SNOAnim.x1_Malthael_sword_shield_Shadow_attack_01_right, AvoidType.AttackRight, Element.Physical) },
            { SNOAnim.x1_Malthael_sword_shield_Shadow_attack_02_left, new Anim(SNOAnim.x1_Malthael_sword_shield_Shadow_attack_02_left, AvoidType.AttackLeft, Element.Physical) },
            { SNOAnim.x1_Malthael_sword_shield_Shadow_attack_03_front, new Anim(SNOAnim.x1_Malthael_sword_shield_Shadow_attack_03_front, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_Malthael_WMevent_attack, new Anim(SNOAnim.x1_Malthael_WMevent_attack, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_MoleMutant_Melee_attack_01, new Anim(SNOAnim.x1_MoleMutant_Melee_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_moleMutant_Melee_attack_combo, new Anim(SNOAnim.x1_moleMutant_Melee_attack_combo, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_MoleMutant_Ranged_attack_01, new Anim(SNOAnim.x1_MoleMutant_Ranged_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_MoleMutant_Ranged_attack_06, new Anim(SNOAnim.x1_MoleMutant_Ranged_attack_06, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_MoleMutant_Ranged_Attack_Comp_Multi_01, new Anim(SNOAnim.x1_MoleMutant_Ranged_Attack_Comp_Multi_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_MoleMutant_Ranged_cast_01, new Anim(SNOAnim.x1_MoleMutant_Ranged_cast_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_moleMutant_Ranged_MoleMutant_jumpback_attack, new Anim(SNOAnim.x1_moleMutant_Ranged_MoleMutant_jumpback_attack, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_moleMutant_Shaman_Attack_Spellcast_01, new Anim(SNOAnim.x1_moleMutant_Shaman_Attack_Spellcast_01, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.x1_moleMutant_Shaman_Attack_Spellcast_02, new Anim(SNOAnim.x1_moleMutant_Shaman_Attack_Spellcast_02, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.x1_moleMutant_Shaman_Attack_Spellcast_03, new Anim(SNOAnim.x1_moleMutant_Shaman_Attack_Spellcast_03, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.x1_moleMutant_Shaman_melee_attack_01, new Anim(SNOAnim.x1_moleMutant_Shaman_melee_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_Monstrosity_ScorpionBug_attack_02_attack01, new Anim(SNOAnim.x1_Monstrosity_ScorpionBug_attack_02_attack01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_Monstrosity_ScorpionBug_generic_cast, new Anim(SNOAnim.x1_Monstrosity_ScorpionBug_generic_cast, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.x1_nightScreamer_attack_01, new Anim(SNOAnim.x1_nightScreamer_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_nightScreamer_attack_scream_01, new Anim(SNOAnim.x1_nightScreamer_attack_scream_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_nightScreamer_DiveBomb_Intro, new Anim(SNOAnim.x1_nightScreamer_DiveBomb_Intro, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_nightScreamer_DiveBomb_Mid, new Anim(SNOAnim.x1_nightScreamer_DiveBomb_Mid, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_nightScreamer_DiveBomb_Outtro, new Anim(SNOAnim.x1_nightScreamer_DiveBomb_Outtro, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_nightScreamer_teleportAttack_01, new Anim(SNOAnim.x1_nightScreamer_teleportAttack_01, AvoidType.Teleport, Element.Physical) },
            { SNOAnim.x1_nightScreamer_tempcast, new Anim(SNOAnim.x1_nightScreamer_tempcast, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.x1_orb_norm_base_01_base, new Anim(SNOAnim.x1_orb_norm_base_01_base, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_PandExt_Ballista_Angelic_A_attack_02, new Anim(SNOAnim.x1_PandExt_Ballista_Angelic_A_attack_02, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_portalGuardianMinion_attack_01, new Anim(SNOAnim.x1_portalGuardianMinion_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_portalGuardianMinion_attack_charge_01, new Anim(SNOAnim.x1_portalGuardianMinion_attack_charge_01, AvoidType.Charge, Element.Physical) },
            { SNOAnim.x1_portalGuardianMinion_attack_charge_recover, new Anim(SNOAnim.x1_portalGuardianMinion_attack_charge_recover, AvoidType.Charge, Element.Physical) },
            { SNOAnim.x1_portalGuardianMinion_attack_charge_tumble, new Anim(SNOAnim.x1_portalGuardianMinion_attack_charge_tumble, AvoidType.Charge, Element.Physical) },
            { SNOAnim.x1_portalGuardianMinion_QUEEN_cast, new Anim(SNOAnim.x1_portalGuardianMinion_QUEEN_cast, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_portalGuardianMinion_ranged_attack_01, new Anim(SNOAnim.x1_portalGuardianMinion_ranged_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_portalGuardianMinion_tempcast, new Anim(SNOAnim.x1_portalGuardianMinion_tempcast, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.x1_Rockworm_attack_01, new Anim(SNOAnim.x1_Rockworm_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_Rockworm_Grabby_attack_cast_01, new Anim(SNOAnim.x1_Rockworm_Grabby_attack_cast_01, AvoidType.Grab, Element.Physical) },
            { SNOAnim.x1_scaryEyes_attack_01, new Anim(SNOAnim.x1_scaryEyes_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_scaryEyes_attack_charge_in, new Anim(SNOAnim.x1_scaryEyes_attack_charge_in, AvoidType.Charge, Element.Physical) },
            { SNOAnim.x1_scaryEyes_attack_charge_mid, new Anim(SNOAnim.x1_scaryEyes_attack_charge_mid, AvoidType.Charge, Element.Physical) },
            { SNOAnim.x1_scaryEyes_attack_charge_out, new Anim(SNOAnim.x1_scaryEyes_attack_charge_out, AvoidType.Charge, Element.Physical) },
            { SNOAnim.x1_Skeleton_Westmarch_attack_01, new Anim(SNOAnim.x1_Skeleton_Westmarch_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_Skeleton_Westmarch_attack_stab, new Anim(SNOAnim.x1_Skeleton_Westmarch_attack_stab, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_Skeleton_Westmarch_attack_stab_out, new Anim(SNOAnim.x1_Skeleton_Westmarch_attack_stab_out, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_Skeleton_Westmarch_generic_cast, new Anim(SNOAnim.x1_Skeleton_Westmarch_generic_cast, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.x1_Skeleton_Westmarch_ghost_attack_01, new Anim(SNOAnim.x1_Skeleton_Westmarch_ghost_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_Skeleton_Westmarch_Strafe_Left_01, new Anim(SNOAnim.x1_Skeleton_Westmarch_Strafe_Left_01, AvoidType.StrafeLeft, Element.Physical) },
            { SNOAnim.x1_Skeleton_Westmarch_Strafe_Right_01, new Anim(SNOAnim.x1_Skeleton_Westmarch_Strafe_Right_01, AvoidType.StrafeRight, Element.Physical) },
            { SNOAnim.x1_SkeletonArcher_Westmarch_attack_01, new Anim(SNOAnim.x1_SkeletonArcher_Westmarch_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_SkeletonArcher_Westmarch_generic_cast, new Anim(SNOAnim.x1_SkeletonArcher_Westmarch_generic_cast, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.x1_SkeletonArcher_Westmarch_ghost_attack_01, new Anim(SNOAnim.x1_SkeletonArcher_Westmarch_ghost_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_SkeletonArcher_Westmarch_ghost_generic_cast, new Anim(SNOAnim.x1_SkeletonArcher_Westmarch_ghost_generic_cast, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.x1_sniperAngel_attack_01, new Anim(SNOAnim.x1_sniperAngel_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_sniperAngel_cast_01, new Anim(SNOAnim.x1_sniperAngel_cast_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_sniperAngel_firebomb_01, new Anim(SNOAnim.x1_sniperAngel_firebomb_01, AvoidType.Projectile, Element.Fire) },
            { SNOAnim.x1_sniperAngel_temp_cast_01, new Anim(SNOAnim.x1_sniperAngel_temp_cast_01, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.x1_Squigglet_Generic_Cast, new Anim(SNOAnim.x1_Squigglet_Generic_Cast, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.x1_Squigglet_Melee_01, new Anim(SNOAnim.x1_Squigglet_Melee_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_Squigglet_RangedAttack_v2, new Anim(SNOAnim.x1_Squigglet_RangedAttack_v2, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_Squigglet_Strafe_Attack_01, new Anim(SNOAnim.x1_Squigglet_Strafe_Attack_01, AvoidType.Strafe, Element.Physical) },
            { SNOAnim.x1_Squigglet_Strafe_Attack_Left_01, new Anim(SNOAnim.x1_Squigglet_Strafe_Attack_Left_01, AvoidType.StrafeLeft, Element.Physical) },
            { SNOAnim.x1_Squigglet_Strafe_Attack_Right_01, new Anim(SNOAnim.x1_Squigglet_Strafe_Attack_Right_01, AvoidType.StrafeRight, Element.Physical) },
            { SNOAnim.x1_Squigglet_Strafe_Left_01, new Anim(SNOAnim.x1_Squigglet_Strafe_Left_01, AvoidType.StrafeLeft, Element.Physical) },
            { SNOAnim.x1_Squigglet_Strafe_Right_01, new Anim(SNOAnim.x1_Squigglet_Strafe_Right_01, AvoidType.StrafeRight, Element.Physical) },
            { SNOAnim.x1_Tentacle_Goatman_Melee_attack_01, new Anim(SNOAnim.x1_Tentacle_Goatman_Melee_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_Tentacle_Goatman_Melee_attack_04, new Anim(SNOAnim.x1_Tentacle_Goatman_Melee_attack_04, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_Tentacle_Goatman_Melee_generic_cast, new Anim(SNOAnim.x1_Tentacle_Goatman_Melee_generic_cast, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.x1_Tentacle_Goatman_Ranged_attack_03_spear_throw, new Anim(SNOAnim.x1_Tentacle_Goatman_Ranged_attack_03_spear_throw, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_Tentacle_Goatman_Shaman_generic_cast, new Anim(SNOAnim.x1_Tentacle_Goatman_Shaman_generic_cast, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.x1_Tentacle_Goatman_Shaman_melee_attack_01, new Anim(SNOAnim.x1_Tentacle_Goatman_Shaman_melee_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_Urzael_attack_06, new Anim(SNOAnim.x1_Urzael_attack_06, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_Urzael_attack_07, new Anim(SNOAnim.x1_Urzael_attack_07, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_Urzael_attack_melee_01, new Anim(SNOAnim.x1_Urzael_attack_melee_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_Urzael_attack_mortor_intro, new Anim(SNOAnim.x1_Urzael_attack_mortor_intro, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_Urzael_attack_mortor_loop, new Anim(SNOAnim.x1_Urzael_attack_mortor_loop, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_Urzael_attack_mortor_outro, new Anim(SNOAnim.x1_Urzael_attack_mortor_outro, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_Urzael_attack_shot_forward_intro, new Anim(SNOAnim.x1_Urzael_attack_shot_forward_intro, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_Urzael_attack_shot_forward_intro_phase2, new Anim(SNOAnim.x1_Urzael_attack_shot_forward_intro_phase2, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_Urzael_attack_shot_forward_middle, new Anim(SNOAnim.x1_Urzael_attack_shot_forward_middle, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_Urzael_attack_shot_forward_middle_phase2, new Anim(SNOAnim.x1_Urzael_attack_shot_forward_middle_phase2, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_Urzael_attack_shot_forward_outro, new Anim(SNOAnim.x1_Urzael_attack_shot_forward_outro, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_Urzael_attack_shot_forward_outro_phase2, new Anim(SNOAnim.x1_Urzael_attack_shot_forward_outro_phase2, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_Urzael_attack_shot_upward, new Anim(SNOAnim.x1_Urzael_attack_shot_upward, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_Urzael_attack_shot_upward_phase2, new Anim(SNOAnim.x1_Urzael_attack_shot_upward_phase2, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_Urzael_Wings_attack_flamethrower, new Anim(SNOAnim.x1_Urzael_Wings_attack_flamethrower, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_UrzaelGreybox_attack_03, new Anim(SNOAnim.x1_UrzaelGreybox_attack_03, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_westmarchBrute_attack_01, new Anim(SNOAnim.x1_westmarchBrute_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_westmarchBrute_attack_02_in, new Anim(SNOAnim.x1_westmarchBrute_attack_02_in, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_westmarchBrute_attack_02_mid, new Anim(SNOAnim.x1_westmarchBrute_attack_02_mid, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_westmarchBrute_attack_02_out, new Anim(SNOAnim.x1_westmarchBrute_attack_02_out, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_westmarchBrute_attack_03, new Anim(SNOAnim.x1_westmarchBrute_attack_03, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_westmarchBrute_attack_04, new Anim(SNOAnim.x1_westmarchBrute_attack_04, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_westmarchBrute_attack_05, new Anim(SNOAnim.x1_westmarchBrute_attack_05, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_westmarchBrute_attack_06_in, new Anim(SNOAnim.x1_westmarchBrute_attack_06_in, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_westmarchBrute_attack_06_mid, new Anim(SNOAnim.x1_westmarchBrute_attack_06_mid, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_westmarchBrute_attack_06_out, new Anim(SNOAnim.x1_westmarchBrute_attack_06_out, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_westmarchBrute_attack_06_slide, new Anim(SNOAnim.x1_westmarchBrute_attack_06_slide, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_westmarchBrute_B_attack_01, new Anim(SNOAnim.x1_westmarchBrute_B_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_westmarchBrute_B_attack_02_in, new Anim(SNOAnim.x1_westmarchBrute_B_attack_02_in, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_westmarchBrute_B_attack_05, new Anim(SNOAnim.x1_westmarchBrute_B_attack_05, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_westmarchBrute_B_attack_06_in, new Anim(SNOAnim.x1_westmarchBrute_B_attack_06_in, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_westmarchBrute_B_attack_06_mid, new Anim(SNOAnim.x1_westmarchBrute_B_attack_06_mid, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_westmarchBrute_B_attack_06_out, new Anim(SNOAnim.x1_westmarchBrute_B_attack_06_out, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_westmarchBrute_B_attack_06_slide, new Anim(SNOAnim.x1_westmarchBrute_B_attack_06_slide, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_westmarchBrute_generic_cast, new Anim(SNOAnim.x1_westmarchBrute_generic_cast, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.x1_westmarchHound_attack_01, new Anim(SNOAnim.x1_westmarchHound_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_westmarchHound_attack_bite_01, new Anim(SNOAnim.x1_westmarchHound_attack_bite_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_westmarchHound_attack_fire_01, new Anim(SNOAnim.x1_westmarchHound_attack_fire_01, AvoidType.Attack, Element.Fire) },
            { SNOAnim.x1_westmarchHound_generic_cast_01, new Anim(SNOAnim.x1_westmarchHound_generic_cast_01, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.x1_westmarchHound_Leader_attack_01, new Anim(SNOAnim.x1_westmarchHound_Leader_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_westmarchHound_Leader_attack_bite_01, new Anim(SNOAnim.x1_westmarchHound_Leader_attack_bite_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_westmarchHound_Leader_attack_fire_01, new Anim(SNOAnim.x1_westmarchHound_Leader_attack_fire_01, AvoidType.Attack, Element.Fire) },
            { SNOAnim.x1_westmarchHound_Leader_generic_cast_01, new Anim(SNOAnim.x1_westmarchHound_Leader_generic_cast_01, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.x1_westmarchHound_Leader_Skeleton_attack_01, new Anim(SNOAnim.x1_westmarchHound_Leader_Skeleton_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_westmarchHound_Leader_Skeleton_attack_bite_01, new Anim(SNOAnim.x1_westmarchHound_Leader_Skeleton_attack_bite_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_westmarchHound_Leader_Skeleton_attack_fire_01, new Anim(SNOAnim.x1_westmarchHound_Leader_Skeleton_attack_fire_01, AvoidType.Attack, Element.Fire) },
            { SNOAnim.x1_westmarchHound_Leader_Skeleton_generic_cast_01, new Anim(SNOAnim.x1_westmarchHound_Leader_Skeleton_generic_cast_01, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.x1_westmarchRanged_attack_01, new Anim(SNOAnim.x1_westmarchRanged_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_westmarchRanged_attack_firebomb_intro, new Anim(SNOAnim.x1_westmarchRanged_attack_firebomb_intro, AvoidType.Attack, Element.Fire) },
            { SNOAnim.x1_westmarchRanged_attack_firebomb_mid, new Anim(SNOAnim.x1_westmarchRanged_attack_firebomb_mid, AvoidType.Attack, Element.Fire) },
            { SNOAnim.x1_westmarchRanged_attack_firebomb_out, new Anim(SNOAnim.x1_westmarchRanged_attack_firebomb_out, AvoidType.Attack, Element.Fire) },
            { SNOAnim.x1_westmarchRanged_attack_slowPathway_01, new Anim(SNOAnim.x1_westmarchRanged_attack_slowPathway_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_westmarchRanged_generic_cast_01, new Anim(SNOAnim.x1_westmarchRanged_generic_cast_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_WickerMan_Attack_01, new Anim(SNOAnim.x1_WickerMan_Attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_WickerMan_cast_fireball, new Anim(SNOAnim.x1_WickerMan_cast_fireball, AvoidType.Projectile, Element.Fire) },
            { SNOAnim.x1_WickerMan_FireNova_cast_outro, new Anim(SNOAnim.x1_WickerMan_FireNova_cast_outro, AvoidType.Projectile, Element.Fire) },
            { SNOAnim.x1_WickerMan_generic_cast, new Anim(SNOAnim.x1_WickerMan_generic_cast, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.x1_WickerMan_teleport_intro, new Anim(SNOAnim.x1_WickerMan_teleport_intro, AvoidType.Teleport, Element.Physical) },
            { SNOAnim.x1_WickerMan_teleport_outro, new Anim(SNOAnim.x1_WickerMan_teleport_outro, AvoidType.Teleport, Element.Physical) },
            { SNOAnim.x1_Wraith_attack_02_BearHug_intro, new Anim(SNOAnim.x1_Wraith_attack_02_BearHug_intro, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_Wraith_attack_02_BearHug_Loop, new Anim(SNOAnim.x1_Wraith_attack_02_BearHug_Loop, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_Wraith_attack_03_spin, new Anim(SNOAnim.x1_Wraith_attack_03_spin, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_Wraith_attack_04_combo, new Anim(SNOAnim.x1_Wraith_attack_04_combo, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_Wraith_attack_05_charge_in, new Anim(SNOAnim.x1_Wraith_attack_05_charge_in, AvoidType.Charge, Element.Physical) },
            { SNOAnim.x1_Wraith_attack_05_charge_mid, new Anim(SNOAnim.x1_Wraith_attack_05_charge_mid, AvoidType.Charge, Element.Physical) },
            { SNOAnim.x1_Wraith_attack_05_charge_out, new Anim(SNOAnim.x1_Wraith_attack_05_charge_out, AvoidType.Charge, Element.Physical) },
            { SNOAnim.x1_Wraith_attack_cast_01, new Anim(SNOAnim.x1_Wraith_attack_cast_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_Wraith_attack_swerve_01, new Anim(SNOAnim.x1_Wraith_attack_swerve_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_Wraith_attack_swerve_02, new Anim(SNOAnim.x1_Wraith_attack_swerve_02, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_Wraith_attack_swerve_03, new Anim(SNOAnim.x1_Wraith_attack_swerve_03, AvoidType.Attack, Element.Physical) },
            { SNOAnim.x1_Wraith_generic_cast_01, new Anim(SNOAnim.x1_Wraith_generic_cast_01, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.x1_Wraith_walk_attack_combo_01, new Anim(SNOAnim.x1_Wraith_walk_attack_combo_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.ZoltunKulle_AOE_01, new Anim(SNOAnim.ZoltunKulle_AOE_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.ZoltunKulle_omni_cast_01, new Anim(SNOAnim.ZoltunKulle_omni_cast_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.ZoltunKulle_omni_cast_04, new Anim(SNOAnim.ZoltunKulle_omni_cast_04, AvoidType.Attack, Element.Physical) },
            { SNOAnim.ZoltunKulle_omni_cast_05, new Anim(SNOAnim.ZoltunKulle_omni_cast_05, AvoidType.Attack, Element.Physical) },
            { SNOAnim.ZoltunKulle_omni_cast_05_fadeOut, new Anim(SNOAnim.ZoltunKulle_omni_cast_05_fadeOut, AvoidType.Attack, Element.Physical) },
            { SNOAnim.ZoltunKulle_strafe_L, new Anim(SNOAnim.ZoltunKulle_strafe_L, AvoidType.Strafe, Element.Physical) },
            { SNOAnim.ZoltunKulle_strafe_R, new Anim(SNOAnim.ZoltunKulle_strafe_R, AvoidType.Strafe, Element.Physical) },
            { SNOAnim.Zombie_Female_attack_01, new Anim(SNOAnim.Zombie_Female_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Zombie_Female_attack_02, new Anim(SNOAnim.Zombie_Female_attack_02, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Zombie_Female_attack_03, new Anim(SNOAnim.Zombie_Female_attack_03, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Zombie_Female_attack_spit, new Anim(SNOAnim.Zombie_Female_attack_spit, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Zombie_Female_generic_cast, new Anim(SNOAnim.Zombie_Female_generic_cast, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.Zombie_Female_skeleton_attack_01, new Anim(SNOAnim.Zombie_Female_skeleton_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Zombie_Female_skeleton_attack_02, new Anim(SNOAnim.Zombie_Female_skeleton_attack_02, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Zombie_Female_skeleton_attack_03, new Anim(SNOAnim.Zombie_Female_skeleton_attack_03, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Zombie_Female_skeleton_attack_spit, new Anim(SNOAnim.Zombie_Female_skeleton_attack_spit, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Zombie_male_attack_01, new Anim(SNOAnim.Zombie_male_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.zombie_male_attack_spore_shake, new Anim(SNOAnim.zombie_male_attack_spore_shake, AvoidType.Attack, Element.Physical) },
            { SNOAnim.zombie_male_Charger_01, new Anim(SNOAnim.zombie_male_Charger_01, AvoidType.Charge, Element.Physical) },
            { SNOAnim.zombie_male_Charger_01_goldenRune, new Anim(SNOAnim.zombie_male_Charger_01_goldenRune, AvoidType.Charge, Element.Physical) },
            { SNOAnim.zombie_male_generic_cast_01, new Anim(SNOAnim.zombie_male_generic_cast_01, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.Zombie_male_skeleton_attack_01, new Anim(SNOAnim.Zombie_male_skeleton_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.zombie_male_skinny_attack_01, new Anim(SNOAnim.zombie_male_skinny_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.zombie_male_skinny_attack_02, new Anim(SNOAnim.zombie_male_skinny_attack_02, AvoidType.Attack, Element.Physical) },
            { SNOAnim.zombie_male_skinny_attack_03_lefthand, new Anim(SNOAnim.zombie_male_skinny_attack_03_lefthand, AvoidType.Attack, Element.Physical) },
            { SNOAnim.zombie_male_skinny_attack_04_bite, new Anim(SNOAnim.zombie_male_skinny_attack_04_bite, AvoidType.Attack, Element.Physical) },
            { SNOAnim.zombie_male_skinny_generic_cast, new Anim(SNOAnim.zombie_male_skinny_generic_cast, AvoidType.GenericCast, Element.Physical) },
            { SNOAnim.zombie_male_skinny_skeleton_attack_01, new Anim(SNOAnim.zombie_male_skinny_skeleton_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Zombiecrawler_male_attack_01, new Anim(SNOAnim.Zombiecrawler_male_attack_01, AvoidType.Attack, Element.Physical) },
            { SNOAnim.Zombiecrawler_male_attack_RootGrab_attempt, new Anim(SNOAnim.Zombiecrawler_male_attack_RootGrab_attempt, AvoidType.Grab, Element.Physical) },
            { SNOAnim.Zombiecrawler_male_attack_RootGrab_idle, new Anim(SNOAnim.Zombiecrawler_male_attack_RootGrab_idle, AvoidType.Grab, Element.Physical) },
        };

        public static HashSet<int> AvoidancesAtPlayer { get { return avoidancesAtPlayer; } }
        private static readonly HashSet<int> avoidancesAtPlayer = new HashSet<int>
        {
            337109, // Wormhole X1_MonsterAffix_TeleportMines
            175452, // morluSpellcaster_teleport_trailActor-23356 Type=ServerProp
            93629, // SkeletonKing_Teleport_Projectile-78908 Type=Projectile
            428810, // p2_morluSpellcaster_teleport_trailActor_cold-145111 Type=ServerProp
            177548, // angelCorrupt_dash_wave_model-164484
            //360598, // x1_Urzael_CeilingDebris_DamagingFire_wall
            //226808, // monsterAffix_waller_model-121230 Type=ServerProp
            3751, // Champion_Teleport_shell-111189
            93892, // SkeletonKing_Teleport_arrival_proxy-173260
            93909, // SkeletonKing_Teleport_Back_trailDudeModel-175506
            83860, // SkeletonKing_Teleport_trailDudeModel-184327
            190198, // Despair_Teleport_shell-21820
        };

        public static Dictionary<SNOActor, Avoidance> ActorAvoidances { get { return actorAvoidances; } }
        private static readonly Dictionary<SNOActor, Avoidance> actorAvoidances = new Dictionary<SNOActor, Avoidance>
        {
            /* Physical	*/							
            { SNOActor.monsterAffix_Avenger_glowSphere, new Avoidance(SNOActor.monsterAffix_Avenger_glowSphere, AvoidType.GroundCircle, Element.Physical) },
            { SNOActor.X1_MonsterAffix_TeleportMines, new Avoidance(SNOActor.X1_MonsterAffix_TeleportMines, AvoidType.Teleport, Element.Physical) },
            { SNOActor.monsterAffix_missileDampening_outsideGeo, new Avoidance(SNOActor.monsterAffix_missileDampening_outsideGeo, AvoidType.Projectile, Element.Physical) },
            { SNOActor.monsterAffix_missileDampening_shield_add, new Avoidance(SNOActor.monsterAffix_missileDampening_shield_add, AvoidType.Projectile, Element.Physical) },
            { SNOActor.monsterAffix_Vortex_model, new Avoidance(SNOActor.monsterAffix_Vortex_model, AvoidType.GroundCircle, Element.Physical) },
            { SNOActor.monsterAffix_Vortex_proxy, new Avoidance(SNOActor.monsterAffix_Vortex_proxy, AvoidType.GroundCircle, Element.Physical) },
            { SNOActor.monsterAffix_vortex_target_trailActor, new Avoidance(SNOActor.monsterAffix_vortex_target_trailActor, AvoidType.GroundCircle, Element.Physical) },
            { SNOActor.monsterAffix_waller_model, new Avoidance(SNOActor.monsterAffix_waller_model, AvoidType.GroundCircle, Element.Physical) },
            { SNOActor.monsterAffix_waller_wall, new Avoidance(SNOActor.monsterAffix_waller_wall, AvoidType.GroundCircle, Element.Physical) },
            { SNOActor.X1_MonsterAffix_corpseBomber_bomb, new Avoidance(SNOActor.X1_MonsterAffix_corpseBomber_bomb, AvoidType.Bomb, Element.Physical) },
            { SNOActor.x1_MonsterAffix_CorpseBomber_bomb_start, new Avoidance(SNOActor.x1_MonsterAffix_CorpseBomber_bomb_start, AvoidType.Bomb, Element.Physical) },
            { SNOActor.x1_MonsterAffix_CorpseBomber_projectile, new Avoidance(SNOActor.x1_MonsterAffix_CorpseBomber_projectile, AvoidType.Bomb, Element.Physical) },
            { SNOActor.monsterAffix_entangler_ringGlow_geo, new Avoidance(SNOActor.monsterAffix_entangler_ringGlow_geo, AvoidType.GroundCircle, Element.Physical) },
            { SNOActor.monsterAffix_frenzySwipe, new Avoidance(SNOActor.monsterAffix_frenzySwipe, AvoidType.GroundCircle, Element.Physical) },
            { SNOActor.monsterAffix_ghostly_distGeo, new Avoidance(SNOActor.monsterAffix_ghostly_distGeo, AvoidType.GroundCircle, Element.Physical) },
            { SNOActor.monsterAffix_healthLink_jumpActor, new Avoidance(SNOActor.monsterAffix_healthLink_jumpActor, AvoidType.GroundCircle, Element.Physical) },
            { SNOActor.monsterAffix_linked_chainHit, new Avoidance(SNOActor.monsterAffix_linked_chainHit, AvoidType.GroundCircle, Element.Physical) },
            { SNOActor.Corpulent_suicide_blood, new Avoidance(SNOActor.Corpulent_suicide_blood, AvoidType.Bomb, Element.Physical, customRadius: 25f) },
            { SNOActor.Corpulent_suicide_frost, new Avoidance(SNOActor.Corpulent_suicide_frost, AvoidType.Bomb, Element.Physical, customRadius: 25f) },
            { SNOActor.Corpulent_suicide_imps, new Avoidance(SNOActor.Corpulent_suicide_imps, AvoidType.Bomb, Element.Physical, customRadius: 25f) },
            { SNOActor.Corpulent_suicide_spiders, new Avoidance(SNOActor.Corpulent_suicide_spiders, AvoidType.Bomb, Element.Physical, customRadius: 25f) },
            { SNOActor.BoneCage_Proxy, new Avoidance(SNOActor.BoneCage_Proxy, AvoidType.Grab, Element.Physical) },
            { SNOActor.ThousandPounder_ribCage, new Avoidance(SNOActor.ThousandPounder_ribCage, AvoidType.Grab, Element.Physical) },
            { SNOActor.ZK_tornado_antimatter, new Avoidance(SNOActor.ZK_tornado_antimatter, AvoidType.Projectile, Element.Physical) },
            { SNOActor.ZK_tornado_model, new Avoidance(SNOActor.ZK_tornado_model, AvoidType.Projectile, Element.Physical) },

            /* Arcane */							
            { SNOActor.MonsterAffix_ArcaneEnchanted_PetSweep, new Avoidance(SNOActor.MonsterAffix_ArcaneEnchanted_PetSweep, AvoidType.RotateRight, Element.Arcane) },
            { SNOActor.MonsterAffix_ArcaneEnchanted_PetSweep_reverse, new Avoidance(SNOActor.MonsterAffix_ArcaneEnchanted_PetSweep_reverse, AvoidType.RotateLeft, Element.Arcane) },
            { SNOActor.MonsterAffix_ArcaneEnchanted_Proxy, new Avoidance(SNOActor.MonsterAffix_ArcaneEnchanted_Proxy, AvoidType.GroundCircle, Element.Arcane) },
            { SNOActor.MonsterAffix_ArcaneEnchanted_trailActor, new Avoidance(SNOActor.MonsterAffix_ArcaneEnchanted_trailActor, AvoidType.GroundCircle, Element.Arcane) },
            { SNOActor.Azmodan_AOD_Demon, new Avoidance(SNOActor.Azmodan_AOD_Demon, AvoidType.GroundCircle, Element.Arcane) },
            { SNOActor.arcaneEnchantedDummy_spawn, new Avoidance(SNOActor.arcaneEnchantedDummy_spawn, AvoidType.GroundCircle, Element.Arcane) },
            { SNOActor.ArcaneSummon_skeleton, new Avoidance(SNOActor.ArcaneSummon_skeleton, AvoidType.GroundCircle, Element.Arcane) },
            { SNOActor.ArcaneSummon_trailActor, new Avoidance(SNOActor.ArcaneSummon_trailActor, AvoidType.GroundCircle, Element.Arcane) },
            { SNOActor.ArcaneTorrent_Target, new Avoidance(SNOActor.ArcaneTorrent_Target, AvoidType.GroundCircle, Element.Arcane) },
            { SNOActor.ArcaneTorrent_Target_crimson, new Avoidance(SNOActor.ArcaneTorrent_Target_crimson, AvoidType.GroundCircle, Element.Arcane) },
            { SNOActor.ArcaneTorrent_Target_golden, new Avoidance(SNOActor.ArcaneTorrent_Target_golden, AvoidType.GroundCircle, Element.Arcane) },
            { SNOActor.ArcaneTorrent_Target_indigo, new Avoidance(SNOActor.ArcaneTorrent_Target_indigo, AvoidType.GroundCircle, Element.Arcane) },
            { SNOActor.TEMP_proxy_AOE_constant_arcane, new Avoidance(SNOActor.TEMP_proxy_AOE_constant_arcane, AvoidType.GroundCircle, Element.Arcane) },
            { SNOActor.UnholyShield_model_arcane, new Avoidance(SNOActor.UnholyShield_model_arcane, AvoidType.GroundCircle, Element.Arcane) },
            { SNOActor.x1_adria_arcane_pool_rageSphere, new Avoidance(SNOActor.x1_adria_arcane_pool_rageSphere, AvoidType.GroundCircle, Element.Arcane) },
            { SNOActor.X1_Adria_arcanePool, new Avoidance(SNOActor.X1_Adria_arcanePool, AvoidType.GroundCircle, Element.Arcane) },

            /* Poison */							
            { SNOActor.monsterAffix_Plagued_endCloud, new Avoidance(SNOActor.monsterAffix_Plagued_endCloud, AvoidType.GroundCircle, Element.Poison, true) },
            { SNOActor.monsterAffix_plagued_groundGeo, new Avoidance(SNOActor.monsterAffix_plagued_groundGeo, AvoidType.GroundCircle, Element.Poison, true) },
            { SNOActor.Evacuation_PoisonLaser, new Avoidance(SNOActor.Evacuation_PoisonLaser, AvoidType.GroundCircle, Element.Poison) },
            { SNOActor.Poison_Glob, new Avoidance(SNOActor.Poison_Glob, AvoidType.GroundCircle, Element.Poison) },
            { SNOActor.UnholyShield_model_poison, new Avoidance(SNOActor.UnholyShield_model_poison, AvoidType.GroundCircle, Element.Poison) },
            { SNOActor.Gluttony_gasCloud_proxy, new Avoidance(SNOActor.Gluttony_gasCloud_proxy, AvoidType.GroundCircle, Element.Poison) },

            /* Cold */							
            { SNOActor.monsterAffix_Frozen_deathExplosion_Proxy, new Avoidance(SNOActor.monsterAffix_Frozen_deathExplosion_Proxy, AvoidType.Bomb, Element.Cold) },
            { SNOActor.monsterAffix_frozen_iceClusters, new Avoidance(SNOActor.monsterAffix_frozen_iceClusters, AvoidType.GroundCircle, Element.Cold) },
            { SNOActor.x1_MonsterAffix_frozenPulse_monster, new Avoidance(SNOActor.x1_MonsterAffix_frozenPulse_monster, AvoidType.GroundCircle, Element.Cold) },
            { SNOActor.x1_MonsterAffix_frozenPulse_shard, new Avoidance(SNOActor.x1_MonsterAffix_frozenPulse_shard, AvoidType.GroundCircle, Element.Cold) },
            { SNOActor.x1_MonsterAffix_frozenPulse_shard_search, new Avoidance(SNOActor.x1_MonsterAffix_frozenPulse_shard_search, AvoidType.GroundCircle, Element.Cold) },
            { SNOActor.x1_monsterAffix_generic_coldDOT_rune_emitter, new Avoidance(SNOActor.x1_monsterAffix_generic_coldDOT_rune_emitter, AvoidType.GroundCircle, Element.Cold) },
            { SNOActor.x1_monsterAffix_generic_coldDOT_runeGeo, new Avoidance(SNOActor.x1_monsterAffix_generic_coldDOT_runeGeo, AvoidType.GroundCircle, Element.Cold) },
            { SNOActor.UnholyShield_model_cold, new Avoidance(SNOActor.UnholyShield_model_cold, AvoidType.GroundCircle, Element.Cold) },
            { SNOActor.x1_skeletonArcher_arrow_cold, new Avoidance(SNOActor.x1_skeletonArcher_arrow_cold, AvoidType.Projectile, Element.Cold) },
            { SNOActor.x1_skeletonArcher_arrow_cold_impact, new Avoidance(SNOActor.x1_skeletonArcher_arrow_cold_impact, AvoidType.Projectile, Element.Cold) },

            /* Fire */							
            { SNOActor.monsterAffix_Desecrator_damage_AOE, new Avoidance(SNOActor.monsterAffix_Desecrator_damage_AOE, AvoidType.GroundCircle, Element.Fire, true) },
            { SNOActor.monsterAffix_Desecrator_telegraph, new Avoidance(SNOActor.monsterAffix_Desecrator_telegraph, AvoidType.GroundCircle, Element.Fire, true) },
            { SNOActor.monsterAffix_molten_bomb_buildUp_geo, new Avoidance(SNOActor.monsterAffix_molten_bomb_buildUp_geo, AvoidType.Bomb, Element.Fire, true) },
            { SNOActor.monsterAffix_Molten_deathExplosion_Proxy, new Avoidance(SNOActor.monsterAffix_Molten_deathExplosion_Proxy, AvoidType.Bomb, Element.Fire, true) },
            { SNOActor.monsterAffix_Molten_deathStart_Proxy, new Avoidance(SNOActor.monsterAffix_Molten_deathStart_Proxy, AvoidType.GroundCircle, Element.Fire, true) },
            { SNOActor.monsterAffix_molten_fireRing, new Avoidance(SNOActor.monsterAffix_molten_fireRing, AvoidType.GroundCircle, Element.Fire, true) },
            { SNOActor.monsterAffix_Molten_trail, new Avoidance(SNOActor.monsterAffix_Molten_trail, AvoidType.GroundCircle, Element.Fire, true) },
            { SNOActor.MonsterAffix_Mortar_Pending, new Avoidance(SNOActor.MonsterAffix_Mortar_Pending, AvoidType.GroundCircle, Element.Fire) },
            { SNOActor.x1_monsteraffix_mortar_blastwave, new Avoidance(SNOActor.x1_monsteraffix_mortar_blastwave, AvoidType.GroundCircle, Element.Fire) },
            { SNOActor.diablo_fireRing, new Avoidance(SNOActor.diablo_fireRing, AvoidType.GroundCircle, Element.Fire) },
            { SNOActor.diablo_melee_fireSwipe, new Avoidance(SNOActor.diablo_melee_fireSwipe, AvoidType.GroundCircle, Element.Fire) },
            { SNOActor.diablo_melee_fireSwipe_left, new Avoidance(SNOActor.diablo_melee_fireSwipe_left, AvoidType.GroundCircle, Element.Fire) },
            { SNOActor.Diablo_ringofFire_damageArea, new Avoidance(SNOActor.Diablo_ringofFire_damageArea, AvoidType.GroundCircle, Element.Fire) },
            { SNOActor.Diablo_ringofFire_damageArea_stage3, new Avoidance(SNOActor.Diablo_ringofFire_damageArea_stage3, AvoidType.GroundCircle, Element.Fire) },
            { SNOActor.DiabloArena_Firewall_HeatDist, new Avoidance(SNOActor.DiabloArena_Firewall_HeatDist, AvoidType.GroundCircle, Element.Fire) },
            { SNOActor.Fire_AlphaTorch_Pole, new Avoidance(SNOActor.Fire_AlphaTorch_Pole, AvoidType.GroundCircle, Element.Fire) },
            { SNOActor.Uber_BossWorld3_st_Demon_ChainPylon_Fire_Azmodan, new Avoidance(SNOActor.Uber_BossWorld3_st_Demon_ChainPylon_Fire_Azmodan, AvoidType.GroundCircle, Element.Fire) },
            { SNOActor.UnholyShield_model_fire, new Avoidance(SNOActor.UnholyShield_model_fire, AvoidType.GroundCircle, Element.Fire) },
            { SNOActor.x1_Abattoir_furnaceSpinner_fireBeam_clockwise, new Avoidance(SNOActor.x1_Abattoir_furnaceSpinner_fireBeam_clockwise, AvoidType.GroundCircle, Element.Fire) },
            { SNOActor.x1_Abattoir_furnaceSpinner_fireBeam_counterClockwise, new Avoidance(SNOActor.x1_Abattoir_furnaceSpinner_fireBeam_counterClockwise, AvoidType.GroundCircle, Element.Fire) },
            { SNOActor.x1_Urzael_teleport_fireTrails, new Avoidance(SNOActor.x1_Urzael_teleport_fireTrails, AvoidType.GroundCircle, Element.Fire) },
            { SNOActor.x1_Urzael_teleport_fireTrails_blue, new Avoidance(SNOActor.x1_Urzael_teleport_fireTrails_blue, AvoidType.GroundCircle, Element.Fire) },
            { SNOActor.x1_Urzael_teleport_fireTrails2, new Avoidance(SNOActor.x1_Urzael_teleport_fireTrails2, AvoidType.GroundCircle, Element.Fire) },
            { SNOActor.x1_Urzael_teleport_fireTrails2_blue, new Avoidance(SNOActor.x1_Urzael_teleport_fireTrails2_blue, AvoidType.GroundCircle, Element.Fire) },

            /* Lightning */							
            { SNOActor.monsterAffix_Electrified_deathExplosion_proxy, new Avoidance(SNOActor.monsterAffix_Electrified_deathExplosion_proxy, AvoidType.Bomb, Element.Lightning) },
            { SNOActor.X1_MonsterAffix_LightningStorm_Wanderer, new Avoidance(SNOActor.X1_MonsterAffix_LightningStorm_Wanderer, AvoidType.GroundCircle, Element.Lightning) },
            { SNOActor.x1_MonsterAffix_Thunderstorm_Impact, new Avoidance(SNOActor.x1_MonsterAffix_Thunderstorm_Impact, AvoidType.GroundCircle, Element.Lightning) },
            { SNOActor.X1_MonsterAffix_Orbiter_FocalPoint, new Avoidance(SNOActor.X1_MonsterAffix_Orbiter_FocalPoint, AvoidType.GroundCircle, Element.Lightning) },
            { SNOActor.x1_MonsterAffix_orbiter_glowSphere, new Avoidance(SNOActor.x1_MonsterAffix_orbiter_glowSphere, AvoidType.GroundCircle, Element.Lightning) },
            { SNOActor.X1_MonsterAffix_Orbiter_Projectile, new Avoidance(SNOActor.X1_MonsterAffix_Orbiter_Projectile, AvoidType.GroundCircle, Element.Lightning) },
            { SNOActor.x1_MonsterAffix_orbiter_projectile_focus, new Avoidance(SNOActor.x1_MonsterAffix_orbiter_projectile_focus, AvoidType.GroundCircle, Element.Lightning) },
            { SNOActor.x1_MonsterAffix_orbiter_projectile_orb, new Avoidance(SNOActor.x1_MonsterAffix_orbiter_projectile_orb, AvoidType.GroundCircle, Element.Lightning) },
            { SNOActor.hoodedNightmare_lightningAtk_cast_sphere, new Avoidance(SNOActor.hoodedNightmare_lightningAtk_cast_sphere, AvoidType.GroundCircle, Element.Lightning) },
            { SNOActor.diablo_lightningBreath_buff_glowBurst, new Avoidance(SNOActor.diablo_lightningBreath_buff_glowBurst, AvoidType.GroundCircle, Element.Lightning)		 },
	
            /* Leap */	
            { SNOActor.MastaBlasta_Rider_leap_trailActor, new Avoidance(SNOActor.MastaBlasta_Rider_leap_trailActor, AvoidType.Leap, Element.Physical) },
            { SNOActor.LacuniFemale_C_CliffLeap, new Avoidance(SNOActor.LacuniFemale_C_CliffLeap, AvoidType.Leap, Element.Physical) },
            { SNOActor.LacuniFemale_A_CliffLeap, new Avoidance(SNOActor.LacuniFemale_A_CliffLeap, AvoidType.Leap, Element.Physical) },
            { SNOActor.x1_westmarchBrute_leap_telegraph, new Avoidance(SNOActor.x1_westmarchBrute_leap_telegraph, AvoidType.Leap, Element.Physical) },
            { SNOActor.x1_westmarchBrute_B_leap_trailActor, new Avoidance(SNOActor.x1_westmarchBrute_B_leap_trailActor, AvoidType.Leap, Element.Physical) },
            { SNOActor.x1_leaperAngel_leap_trailActor, new Avoidance(SNOActor.x1_leaperAngel_leap_trailActor, AvoidType.Leap, Element.Physical) },
            { SNOActor.x1_westmarchBrute_leap_trailActor, new Avoidance(SNOActor.x1_westmarchBrute_leap_trailActor, AvoidType.Leap, Element.Physical) },
            { SNOActor.x1_Spawner_leaperAngel_A_FortressUnique, new Avoidance(SNOActor.x1_Spawner_leaperAngel_A_FortressUnique, AvoidType.Leap, Element.Physical) },
            { SNOActor.x1_Urzael_leap_trailActor, new Avoidance(SNOActor.x1_Urzael_leap_trailActor, AvoidType.Leap, Element.Physical) },
            { SNOActor.x1_westmarch_rat_leap_trailActor, new Avoidance(SNOActor.x1_westmarch_rat_leap_trailActor, AvoidType.Leap, Element.Physical) },

            /* AOE */							
            { SNOActor.X1_Unique_Monster_Generic_AOE_DOT_Cold_10foot, new Avoidance(SNOActor.X1_Unique_Monster_Generic_AOE_DOT_Cold_10foot, AvoidType.GroundCircle, Element.Cold, customRadius: 12f) },
            { SNOActor.X1_Unique_Monster_Generic_AOE_DOT_Cold_20foot, new Avoidance(SNOActor.X1_Unique_Monster_Generic_AOE_DOT_Cold_20foot, AvoidType.GroundCircle, Element.Cold, customRadius: 22f) },
            { SNOActor.X1_Unique_Monster_Generic_AOE_DOT_Fire_10foot, new Avoidance(SNOActor.X1_Unique_Monster_Generic_AOE_DOT_Fire_10foot, AvoidType.GroundCircle, Element.Fire, customRadius: 12f) },
            { SNOActor.X1_Unique_Monster_Generic_AOE_DOT_Fire_20foot, new Avoidance(SNOActor.X1_Unique_Monster_Generic_AOE_DOT_Fire_20foot, AvoidType.GroundCircle, Element.Fire, customRadius: 22f) },
            { SNOActor.X1_Unique_Monster_Generic_AOE_DOT_Lightning_10foot, new Avoidance(SNOActor.X1_Unique_Monster_Generic_AOE_DOT_Lightning_10foot, AvoidType.GroundCircle, Element.Lightning, customRadius: 12f) },
            { SNOActor.X1_Unique_Monster_Generic_AOE_DOT_Lightning_20foot, new Avoidance(SNOActor.X1_Unique_Monster_Generic_AOE_DOT_Lightning_20foot, AvoidType.GroundCircle, Element.Lightning, customRadius: 22f) },
            { SNOActor.X1_Unique_Monster_Generic_AOE_DOT_Lightning_hardpoints, new Avoidance(SNOActor.X1_Unique_Monster_Generic_AOE_DOT_Lightning_hardpoints, AvoidType.GroundCircle, Element.Lightning, customRadius: 12f) },
            { SNOActor.X1_Unique_Monster_Generic_AOE_DOT_Poison_10foot, new Avoidance(SNOActor.X1_Unique_Monster_Generic_AOE_DOT_Poison_10foot, AvoidType.GroundCircle, Element.Poison, customRadius: 12f) },
            { SNOActor.X1_Unique_Monster_Generic_AOE_DOT_Poison_20foot, new Avoidance(SNOActor.X1_Unique_Monster_Generic_AOE_DOT_Poison_20foot, AvoidType.GroundCircle, Element.Poison, customRadius: 22f) },
            { SNOActor.X1_Unique_Monster_Generic_AOE_Lightning_Ring, new Avoidance(SNOActor.X1_Unique_Monster_Generic_AOE_Lightning_Ring, AvoidType.GroundCircle, Element.Lightning) },
            { SNOActor.X1_Unique_Monster_Generic_AOE_Sphere_Distortion, new Avoidance(SNOActor.X1_Unique_Monster_Generic_AOE_Sphere_Distortion, AvoidType.GroundCircle, Element.Physical) },

            /* Orb */							
            { SNOActor.x1_unique_orb_myken_sphere, new Avoidance(SNOActor.x1_unique_orb_myken_sphere, AvoidType.Projectile, Element.Physical) },
            { SNOActor.Azmodan_OA_fire_orb, new Avoidance(SNOActor.Azmodan_OA_fire_orb, AvoidType.Projectile, Element.Fire) },
            { SNOActor.arcanumOrb_model, new Avoidance(SNOActor.arcanumOrb_model, AvoidType.Projectile, Element.Arcane) },
            { SNOActor.Azmodan_orbofAnnihilation_groundExplode, new Avoidance(SNOActor.Azmodan_orbofAnnihilation_groundExplode, AvoidType.GroundCircle, Element.Physical) },
            { SNOActor.Azmodan_orbOfAnnihilation_projectile, new Avoidance(SNOActor.Azmodan_orbOfAnnihilation_projectile, AvoidType.Projectile, Element.Physical) },
            { SNOActor.cupcakeOrb_model, new Avoidance(SNOActor.cupcakeOrb_model, AvoidType.Projectile, Element.Physical) },
            { SNOActor.goatWarrior_shaman_orb, new Avoidance(SNOActor.goatWarrior_shaman_orb, AvoidType.Projectile, Element.Physical) },
            { SNOActor.goatWarrior_shaman_orb_cheap, new Avoidance(SNOActor.goatWarrior_shaman_orb_cheap, AvoidType.Projectile, Element.Physical) },
            { SNOActor.hoodedNightmare_curse_attractorBolt, new Avoidance(SNOActor.hoodedNightmare_curse_attractorBolt, AvoidType.Projectile, Element.Physical) },
            { SNOActor.mistressOfPain_cast_attractorBolt, new Avoidance(SNOActor.mistressOfPain_cast_attractorBolt, AvoidType.Projectile, Element.Physical) },
            { SNOActor.mistressOfPain_death_leg_dust_attractorBolt, new Avoidance(SNOActor.mistressOfPain_death_leg_dust_attractorBolt, AvoidType.Projectile, Element.Physical) },
            { SNOActor.Spawner_X1_ZombieCrawler_Orb, new Avoidance(SNOActor.Spawner_X1_ZombieCrawler_Orb, AvoidType.Projectile, Element.Physical) },
            { SNOActor.x1_deathMaiden_orb_summon_rope_head_glow, new Avoidance(SNOActor.x1_deathMaiden_orb_summon_rope_head_glow, AvoidType.Projectile, Element.Physical) },
            { SNOActor.x1_sniperAngel_shardBolt_orb, new Avoidance(SNOActor.x1_sniperAngel_shardBolt_orb, AvoidType.Projectile, Element.Physical) },
            { SNOActor.X1_ZombieCrawler_Orb, new Avoidance(SNOActor.X1_ZombieCrawler_Orb, AvoidType.Projectile, Element.Physical) },

            /* Arrow */							
            { SNOActor.BoneArcher_arrow, new Avoidance(SNOActor.BoneArcher_arrow, AvoidType.Projectile, Element.Physical) },
            { SNOActor.Ranger_BombArrow, new Avoidance(SNOActor.Ranger_BombArrow, AvoidType.Projectile, Element.Physical) },
            { SNOActor.Scoundrel_arrow_cripplingShot, new Avoidance(SNOActor.Scoundrel_arrow_cripplingShot, AvoidType.Projectile, Element.Physical) },
            { SNOActor.Scoundrel_arrow_poisoned, new Avoidance(SNOActor.Scoundrel_arrow_poisoned, AvoidType.GroundCircle, Element.Poison) },
            { SNOActor.Scoundrel_arrow_powerShot, new Avoidance(SNOActor.Scoundrel_arrow_powerShot, AvoidType.Projectile, Element.Physical) },
            { SNOActor.x1_Bog_Knockback_Plant_Arrow, new Avoidance(SNOActor.x1_Bog_Knockback_Plant_Arrow, AvoidType.Projectile, Element.Physical) },
            { SNOActor.x1_PandExt_Ballista_Angelic_B_arrowCast, new Avoidance(SNOActor.x1_PandExt_Ballista_Angelic_B_arrowCast, AvoidType.Projectile, Element.Physical) },
            { SNOActor.x1_PandExt_Ballista_Angelic_B_arrowForming, new Avoidance(SNOActor.x1_PandExt_Ballista_Angelic_B_arrowForming, AvoidType.Projectile, Element.Physical) },
            { SNOActor.x1_PandExt_Ballista_Angelic_B_formingEnergyArrow, new Avoidance(SNOActor.x1_PandExt_Ballista_Angelic_B_formingEnergyArrow, AvoidType.Projectile, Element.Physical) },
            { SNOActor.BoneArcher_arrow_lightning, new Avoidance(SNOActor.BoneArcher_arrow_lightning, AvoidType.Projectile, Element.Lightning) },

            /* Ball */
            { SNOActor.TriuneSummoner_fireBall_obj, new Avoidance(SNOActor.TriuneSummoner_fireBall_obj, AvoidType.Projectile, Element.Fire) },
            { SNOActor.fallenShaman_fireBall_obj, new Avoidance(SNOActor.fallenShaman_fireBall_obj, AvoidType.Projectile, Element.Fire) },
            { SNOActor.fireBall_meteor_shard, new Avoidance(SNOActor.fireBall_meteor_shard, AvoidType.Projectile, Element.Fire) },
            { SNOActor.Zombie_female_barfBall, new Avoidance(SNOActor.Zombie_female_barfBall, AvoidType.Projectile, Element.Physical) },
            { SNOActor.Goatman_Shaman_Iceball_Explosion, new Avoidance(SNOActor.Goatman_Shaman_Iceball_Explosion, AvoidType.Projectile, Element.Cold) },
            { SNOActor.x1_Urzael_Cannonball, new Avoidance(SNOActor.x1_Urzael_Cannonball, AvoidType.Projectile, Element.Physical) },
            { SNOActor.x1_Urzael_Cannonball_burning, new Avoidance(SNOActor.x1_Urzael_Cannonball_burning, AvoidType.Projectile, Element.Fire) },
            { SNOActor.x1_urzael_cannonball_burning_model, new Avoidance(SNOActor.x1_urzael_cannonball_burning_model, AvoidType.Projectile, Element.Fire) },
            { SNOActor.x1_Wickerman_turnIntoFireBall_swirl, new Avoidance(SNOActor.x1_Wickerman_turnIntoFireBall_swirl, AvoidType.Projectile, Element.Fire) },
            { SNOActor.x1_Wickerman_turnIntoFireBall_corona, new Avoidance(SNOActor.x1_Wickerman_turnIntoFireBall_corona, AvoidType.Projectile, Element.Fire) },
            { SNOActor.x1_bloodGolem_shaman_bloodBall, new Avoidance(SNOActor.x1_bloodGolem_shaman_bloodBall, AvoidType.Projectile, Element.Physical) },
            { SNOActor.x1_urzael_cannonball_dummy, new Avoidance(SNOActor.x1_urzael_cannonball_dummy, AvoidType.Projectile, Element.Physical) },
            { SNOActor.x1_Urzael_Cannonball_burning_invisible, new Avoidance(SNOActor.x1_Urzael_Cannonball_burning_invisible, AvoidType.Projectile, Element.Fire) },
            { SNOActor.x1_malthael_deathBall_head, new Avoidance(SNOActor.x1_malthael_deathBall_head, AvoidType.Projectile, Element.Physical) },
            { SNOActor.x1_malthael_deathBall_spawn, new Avoidance(SNOActor.x1_malthael_deathBall_spawn, AvoidType.Projectile, Element.Physical) },
            { SNOActor.x1_malthael_deathBall_spawn_ghost, new Avoidance(SNOActor.x1_malthael_deathBall_spawn_ghost, AvoidType.Projectile, Element.Physical) },
            { SNOActor.x1_malthael_deathBall_gatherStorm, new Avoidance(SNOActor.x1_malthael_deathBall_gatherStorm, AvoidType.Projectile, Element.Physical) },
            { SNOActor.x1_malthael_gratesOfHell_darkBall, new Avoidance(SNOActor.x1_malthael_gratesOfHell_darkBall, AvoidType.Projectile, Element.Physical) },
            { SNOActor.x1_malthael_gratesOfHell_darkBall_glowOuter, new Avoidance(SNOActor.x1_malthael_gratesOfHell_darkBall_glowOuter, AvoidType.Projectile, Element.Physical) },
            { SNOActor.x1_malthael_gratesOfHell_darkBall_glow, new Avoidance(SNOActor.x1_malthael_gratesOfHell_darkBall_glow, AvoidType.Projectile, Element.Physical) },
            { SNOActor.x1_malthael_deathBall_explosion, new Avoidance(SNOActor.x1_malthael_deathBall_explosion, AvoidType.Projectile, Element.Physical) },
            { SNOActor.x1_malthael_deathBall_explosion_blastWave, new Avoidance(SNOActor.x1_malthael_deathBall_explosion_blastWave, AvoidType.Projectile, Element.Physical) },

            /* Projectile */							
            { SNOActor.Grenadier_Proj, new Avoidance(SNOActor.Grenadier_Proj, AvoidType.Projectile, Element.Physical) },
            { SNOActor.grenadier_proj_trail, new Avoidance(SNOActor.grenadier_proj_trail, AvoidType.Projectile, Element.Physical) },
            { SNOActor.BelialFireBomb, new Avoidance(SNOActor.BelialFireBomb, AvoidType.Projectile, Element.Fire) },
            { SNOActor.a2dun_Cave_SlimeGeyser_A_projectile, new Avoidance(SNOActor.a2dun_Cave_SlimeGeyser_A_projectile, AvoidType.Projectile, Element.Physical) },
            { SNOActor.a2dun_Zolt_Tesla_Tower_projectile, new Avoidance(SNOActor.a2dun_Zolt_Tesla_Tower_projectile, AvoidType.Projectile, Element.Physical) },
            { SNOActor.a3dun_crater_Demon_GroundTrap_GasChamber_Projectile, new Avoidance(SNOActor.a3dun_crater_Demon_GroundTrap_GasChamber_Projectile, AvoidType.Projectile, Element.Physical) },
            { SNOActor.a4dun_Garden_Corruption_Mine_projectile, new Avoidance(SNOActor.a4dun_Garden_Corruption_Mine_projectile, AvoidType.Projectile, Element.Physical) },
            { SNOActor.a4dun_Spire_CorruptionGeyser_projectile2, new Avoidance(SNOActor.a4dun_Spire_CorruptionGeyser_projectile2, AvoidType.Projectile, Element.Physical) },
            { SNOActor.a4dun_spire_firewall_projectile, new Avoidance(SNOActor.a4dun_spire_firewall_projectile, AvoidType.Projectile, Element.Fire) },
            { SNOActor.Belial_armSlam_projectile, new Avoidance(SNOActor.Belial_armSlam_projectile, AvoidType.Projectile, Element.Physical) },
            { SNOActor.Belial_groundProjectile, new Avoidance(SNOActor.Belial_groundProjectile, AvoidType.Projectile, Element.Physical) },
            { SNOActor.creepMob_burrowArm_projectile, new Avoidance(SNOActor.creepMob_burrowArm_projectile, AvoidType.Projectile, Element.Physical) },
            { SNOActor.demonFlyer_bomb_projectile, new Avoidance(SNOActor.demonFlyer_bomb_projectile, AvoidType.Projectile, Element.Physical) },
            { SNOActor.demonFlyer_fireball_projectile, new Avoidance(SNOActor.demonFlyer_fireball_projectile, AvoidType.Projectile, Element.Fire) },
            { SNOActor.Diablo_expandingFire_projectile, new Avoidance(SNOActor.Diablo_expandingFire_projectile, AvoidType.Projectile, Element.Fire) },
            { SNOActor.Diablo_expandingFire_projectile_new, new Avoidance(SNOActor.Diablo_expandingFire_projectile_new, AvoidType.Projectile, Element.Fire) },
            { SNOActor.Diablo_lightningBreath_projectile, new Avoidance(SNOActor.Diablo_lightningBreath_projectile, AvoidType.Projectile, Element.Lightning) },
            { SNOActor.FallenShaman_fireball_projectile, new Avoidance(SNOActor.FallenShaman_fireball_projectile, AvoidType.Projectile, Element.Fire) },
            { SNOActor.Gluttony_loogiespawn_projectile, new Avoidance(SNOActor.Gluttony_loogiespawn_projectile, AvoidType.Projectile, Element.Physical) },
            { SNOActor.Goatman_shaman_snowShield_retribution_projectile, new Avoidance(SNOActor.Goatman_shaman_snowShield_retribution_projectile, AvoidType.Projectile, Element.Physical) },
            { SNOActor.GoatMutant_Shaman_blast_projectile, new Avoidance(SNOActor.GoatMutant_Shaman_blast_projectile, AvoidType.Projectile, Element.Physical) },
            { SNOActor.goatWarrior_shaman_projectile, new Avoidance(SNOActor.goatWarrior_shaman_projectile, AvoidType.Projectile, Element.Physical) },
            { SNOActor.hoodedNightmare_Lighting_projectile, new Avoidance(SNOActor.hoodedNightmare_Lighting_projectile, AvoidType.Projectile, Element.Lightning) },
            { SNOActor.lacuniFemale_bomb_projectile, new Avoidance(SNOActor.lacuniFemale_bomb_projectile, AvoidType.Projectile, Element.Physical) },
            { SNOActor.lacuniFemale_bomb_projectile_actor, new Avoidance(SNOActor.lacuniFemale_bomb_projectile_actor, AvoidType.Projectile, Element.Physical) },
            { SNOActor.lordOfDespair_summoning_projectile, new Avoidance(SNOActor.lordOfDespair_summoning_projectile, AvoidType.Projectile, Element.Physical) },
            { SNOActor.lordOfDespair_volley_projectile, new Avoidance(SNOActor.lordOfDespair_volley_projectile, AvoidType.Projectile, Element.Physical) },
            { SNOActor.Maghda_event19_portal_projectile, new Avoidance(SNOActor.Maghda_event19_portal_projectile, AvoidType.Projectile, Element.Physical) },
            { SNOActor.Maghda_event19_projectile, new Avoidance(SNOActor.Maghda_event19_projectile, AvoidType.Projectile, Element.Physical) },
            { SNOActor.Maghda_Punish_projectile, new Avoidance(SNOActor.Maghda_Punish_projectile, AvoidType.Projectile, Element.Physical) },
            { SNOActor.MastaBlasta_Rider_projectile, new Avoidance(SNOActor.MastaBlasta_Rider_projectile, AvoidType.Projectile, Element.Physical) },
            { SNOActor.MistressOfPain_painBolt_projectile, new Avoidance(SNOActor.MistressOfPain_painBolt_projectile, AvoidType.Projectile, Element.Physical) },
            { SNOActor.QuillDemonHorn_Projectile, new Avoidance(SNOActor.QuillDemonHorn_Projectile, AvoidType.Projectile, Element.Physical) },
            { SNOActor.SandWasp_Projectile, new Avoidance(SNOActor.SandWasp_Projectile, AvoidType.Projectile, Element.Physical) },
            { SNOActor.sandWasp_Projectile_actor, new Avoidance(SNOActor.sandWasp_Projectile_actor, AvoidType.Projectile, Element.Physical) },
            { SNOActor.SkeletonKing_Teleport_Projectile, new Avoidance(SNOActor.SkeletonKing_Teleport_Projectile, AvoidType.Projectile, Element.Physical) },
            { SNOActor.skeletonMage_Cold_projectile, new Avoidance(SNOActor.skeletonMage_Cold_projectile, AvoidType.Projectile, Element.Cold) },
            { SNOActor.skeletonMage_Fire_projectile, new Avoidance(SNOActor.skeletonMage_Fire_projectile, AvoidType.Projectile, Element.Fire) },
            { SNOActor.skeletonMage_Lightning_projectile, new Avoidance(SNOActor.skeletonMage_Lightning_projectile, AvoidType.Projectile, Element.Lightning) },
            { SNOActor.skeletonMage_Poison_projectile, new Avoidance(SNOActor.skeletonMage_Poison_projectile, AvoidType.Projectile, Element.Poison) },
            { SNOActor.SkeletonSummoner_projectile, new Avoidance(SNOActor.SkeletonSummoner_projectile, AvoidType.Projectile, Element.Physical) },
            { SNOActor.soulRipper_tongue_proxy_projectile, new Avoidance(SNOActor.soulRipper_tongue_proxy_projectile, AvoidType.Projectile, Element.Physical) },
            { SNOActor.succubus_bloodStar_projectile, new Avoidance(SNOActor.succubus_bloodStar_projectile, AvoidType.Projectile, Element.Physical) },
            { SNOActor.TEMP_projectile_lobbed_pink, new Avoidance(SNOActor.TEMP_projectile_lobbed_pink, AvoidType.Projectile, Element.Physical) },
            { SNOActor.TEMP_projectile_pink, new Avoidance(SNOActor.TEMP_projectile_pink, AvoidType.Projectile, Element.Physical) },
            { SNOActor.tempDesign_projectile, new Avoidance(SNOActor.tempDesign_projectile, AvoidType.Projectile, Element.Physical) },
            { SNOActor.TriuneSummoner_fireball_projectile, new Avoidance(SNOActor.TriuneSummoner_fireball_projectile, AvoidType.Projectile, Element.Fire) },
            { SNOActor.UberMaghda_Punish_projectile, new Avoidance(SNOActor.UberMaghda_Punish_projectile, AvoidType.Projectile, Element.Physical) },
            { SNOActor.unique_shenlong_projectile_model, new Avoidance(SNOActor.unique_shenlong_projectile_model, AvoidType.Projectile, Element.Physical) },
            { SNOActor.Wand_physical_projectile, new Avoidance(SNOActor.Wand_physical_projectile, AvoidType.Projectile, Element.Physical) },
            { SNOActor.wardenMissile_projectile, new Avoidance(SNOActor.wardenMissile_projectile, AvoidType.Projectile, Element.Physical) },
            { SNOActor.x1_Adria_bouncingProjectile, new Avoidance(SNOActor.x1_Adria_bouncingProjectile, AvoidType.Projectile, Element.Physical) },
            { SNOActor.x1_Adria_cauldron_spawn_Projectile, new Avoidance(SNOActor.x1_Adria_cauldron_spawn_Projectile, AvoidType.Projectile, Element.Physical) },
            { SNOActor.x1_Adria_spitAtPlayer_projectile, new Avoidance(SNOActor.x1_Adria_spitAtPlayer_projectile, AvoidType.Projectile, Element.Physical) },
            { SNOActor.x1_Bog_Bear_Trap_projectile_lobbed, new Avoidance(SNOActor.x1_Bog_Bear_Trap_projectile_lobbed, AvoidType.Projectile, Element.Physical) },
            { SNOActor.x1_Bog_Bear_Trap_projectile_Tossed, new Avoidance(SNOActor.x1_Bog_Bear_Trap_projectile_Tossed, AvoidType.Projectile, Element.Physical) },
            { SNOActor.x1_bogBlight_pustule_projectile, new Avoidance(SNOActor.x1_bogBlight_pustule_projectile, AvoidType.Projectile, Element.Physical) },
            { SNOActor.x1_Cesspool_Slime_Posion_Attack_Projectile, new Avoidance(SNOActor.x1_Cesspool_Slime_Posion_Attack_Projectile, AvoidType.Projectile, Element.Physical) },
            { SNOActor.x1_DarkAngel_Summon_groundFog_projectile, new Avoidance(SNOActor.x1_DarkAngel_Summon_groundFog_projectile, AvoidType.Projectile, Element.Physical) },
            { SNOActor.x1_Lieutenant_Mortar_projectile, new Avoidance(SNOActor.x1_Lieutenant_Mortar_projectile, AvoidType.Projectile, Element.Physical) },
            { SNOActor.X1_LR_Boss_FireNova_projectile, new Avoidance(SNOActor.X1_LR_Boss_FireNova_projectile, AvoidType.Projectile, Element.Fire) },
            { SNOActor.x1_Malthael_Diablo_Fire_Projectile, new Avoidance(SNOActor.x1_Malthael_Diablo_Fire_Projectile, AvoidType.Projectile, Element.Fire) },
            { SNOActor.x1_Malthael_Diablo_lightningBreath_projectile, new Avoidance(SNOActor.x1_Malthael_Diablo_lightningBreath_projectile, AvoidType.Projectile, Element.Lightning) },
            { SNOActor.x1_MoleMutant_ranged_projectile, new Avoidance(SNOActor.x1_MoleMutant_ranged_projectile, AvoidType.Projectile, Element.Physical) },
            { SNOActor.x1_nightScreamer_scream_pseudoProjectileActor, new Avoidance(SNOActor.x1_nightScreamer_scream_pseudoProjectileActor, AvoidType.Projectile, Element.Physical) },
            { SNOActor.x1_Pand_Ext_Event_greatWeapon_bossSpawn_projectile, new Avoidance(SNOActor.x1_Pand_Ext_Event_greatWeapon_bossSpawn_projectile, AvoidType.Projectile, Element.Physical) },
            { SNOActor.x1_Pand_test_rlarsen_projectileTrail, new Avoidance(SNOActor.x1_Pand_test_rlarsen_projectileTrail, AvoidType.Projectile, Element.Physical) },
            { SNOActor.x1_PandExt_Ballista_Angelic_B_arrowProjectile, new Avoidance(SNOActor.x1_PandExt_Ballista_Angelic_B_arrowProjectile, AvoidType.Projectile, Element.Physical) },
            { SNOActor.x1_PortalGuardianMinion_projectile_geo, new Avoidance(SNOActor.x1_PortalGuardianMinion_projectile_geo, AvoidType.Projectile, Element.Physical) },
            { SNOActor.X1_projectile_mystically_runec_boulder, new Avoidance(SNOActor.X1_projectile_mystically_runec_boulder, AvoidType.Projectile, Element.Physical) },
            { SNOActor.x1_Rockworm_Pand_projectile, new Avoidance(SNOActor.x1_Rockworm_Pand_projectile, AvoidType.Projectile, Element.Physical) },
            { SNOActor.x1_squigglet_projectile, new Avoidance(SNOActor.x1_squigglet_projectile, AvoidType.Projectile, Element.Physical) },
            { SNOActor.X1_Unique_Monster_Generic_Projectile_Arcane, new Avoidance(SNOActor.X1_Unique_Monster_Generic_Projectile_Arcane, AvoidType.Projectile, Element.Arcane) },
            { SNOActor.X1_Unique_Monster_Generic_Projectile_Cold, new Avoidance(SNOActor.X1_Unique_Monster_Generic_Projectile_Cold, AvoidType.Projectile, Element.Cold) },
            { SNOActor.X1_Unique_Monster_Generic_Projectile_Fire, new Avoidance(SNOActor.X1_Unique_Monster_Generic_Projectile_Fire, AvoidType.Projectile, Element.Fire) },
            { SNOActor.X1_Unique_Monster_Generic_Projectile_Holy, new Avoidance(SNOActor.X1_Unique_Monster_Generic_Projectile_Holy, AvoidType.Projectile, Element.Holy) },
            { SNOActor.X1_Unique_Monster_Generic_Projectile_Lightning, new Avoidance(SNOActor.X1_Unique_Monster_Generic_Projectile_Lightning, AvoidType.Projectile, Element.Lightning) },
            { SNOActor.X1_Unique_Monster_Generic_Projectile_Physical, new Avoidance(SNOActor.X1_Unique_Monster_Generic_Projectile_Physical, AvoidType.Projectile, Element.Physical) },
            { SNOActor.X1_Unique_Monster_Generic_Projectile_Poison, new Avoidance(SNOActor.X1_Unique_Monster_Generic_Projectile_Poison, AvoidType.Projectile, Element.Poison) },
            { SNOActor.x1_Urzael_death_miniProjectile, new Avoidance(SNOActor.x1_Urzael_death_miniProjectile, AvoidType.Projectile, Element.Physical) },
            { SNOActor.x1_westmarchRanged_projectile, new Avoidance(SNOActor.x1_westmarchRanged_projectile, AvoidType.Projectile, Element.Physical) },
            { SNOActor.x1_westmarchRanged_projectile_invisible, new Avoidance(SNOActor.x1_westmarchRanged_projectile_invisible, AvoidType.Projectile, Element.Physical) },
            { SNOActor.x1_westmarchRanged_projectile_miss, new Avoidance(SNOActor.x1_westmarchRanged_projectile_miss, AvoidType.Projectile, Element.Physical) },
            { SNOActor.x1_Wickerman_fireball_projectile, new Avoidance(SNOActor.x1_Wickerman_fireball_projectile, AvoidType.Projectile, Element.Fire) },
            { SNOActor.x1_Wickerman_turnIntoFireBall_coronaProjectile, new Avoidance(SNOActor.x1_Wickerman_turnIntoFireBall_coronaProjectile, AvoidType.Projectile, Element.Fire) },
            { SNOActor.zoltunKulle_fieryBoulder_projectile, new Avoidance(SNOActor.zoltunKulle_fieryBoulder_projectile, AvoidType.Projectile, Element.Physical) },
            { SNOActor.zombie_female_barfBall_projectile, new Avoidance(SNOActor.zombie_female_barfBall_projectile, AvoidType.Projectile, Element.Physical) },

            /* Impact */							
            { SNOActor.Grenadier_Proj_mortar_inpact, new Avoidance(SNOActor.Grenadier_Proj_mortar_inpact, AvoidType.Projectile, Element.Physical) },
            { SNOActor.Beast_impactWave, new Avoidance(SNOActor.Beast_impactWave, AvoidType.Projectile, Element.Physical) },
            { SNOActor.Belial_GroundBomb_Impact, new Avoidance(SNOActor.Belial_GroundBomb_Impact, AvoidType.Projectile, Element.Physical) },
            { SNOActor.demonFlyer_fireball_impact, new Avoidance(SNOActor.demonFlyer_fireball_impact, AvoidType.Projectile, Element.Physical) },
            { SNOActor.Demonic_Meteor_Impact, new Avoidance(SNOActor.Demonic_Meteor_Impact, AvoidType.Projectile, Element.Physical) },
            { SNOActor.Diablo_Meteor_Impact, new Avoidance(SNOActor.Diablo_Meteor_Impact, AvoidType.Projectile, Element.Physical) },
            { SNOActor.fallenChamp_attack2_impactRays, new Avoidance(SNOActor.fallenChamp_attack2_impactRays, AvoidType.Projectile, Element.Physical) },
            { SNOActor.fallenChamp_attack2_impactSphere, new Avoidance(SNOActor.fallenChamp_attack2_impactSphere, AvoidType.Projectile, Element.Physical) },
            { SNOActor.fallenShaman_fireBall_impact, new Avoidance(SNOActor.fallenShaman_fireBall_impact, AvoidType.Projectile, Element.Physical) },
            { SNOActor.g_chargedBolt_groundImpact, new Avoidance(SNOActor.g_chargedBolt_groundImpact, AvoidType.Projectile, Element.Physical) },
            { SNOActor.g_chargedBolt_impact, new Avoidance(SNOActor.g_chargedBolt_impact, AvoidType.Projectile, Element.Physical) },
            { SNOActor.g_MagicImpact_Purple_Emitter, new Avoidance(SNOActor.g_MagicImpact_Purple_Emitter, AvoidType.Projectile, Element.Physical) },
            { SNOActor.g_monster_projectile_arcane_impact, new Avoidance(SNOActor.g_monster_projectile_arcane_impact, AvoidType.Projectile, Element.Arcane) },
            { SNOActor.g_monster_projectile_cold_impact, new Avoidance(SNOActor.g_monster_projectile_cold_impact, AvoidType.Projectile, Element.Cold) },
            { SNOActor.g_monster_projectile_fire_impact, new Avoidance(SNOActor.g_monster_projectile_fire_impact, AvoidType.Projectile, Element.Fire) },
            { SNOActor.g_monster_projectile_holy_impact, new Avoidance(SNOActor.g_monster_projectile_holy_impact, AvoidType.Projectile, Element.Holy) },
            { SNOActor.g_monster_projectile_lightning_impact, new Avoidance(SNOActor.g_monster_projectile_lightning_impact, AvoidType.Projectile, Element.Lightning) },
            { SNOActor.g_monster_projectile_phys_impact, new Avoidance(SNOActor.g_monster_projectile_phys_impact, AvoidType.Projectile, Element.Physical) },
            { SNOActor.g_monster_projectile_poison_impact, new Avoidance(SNOActor.g_monster_projectile_poison_impact, AvoidType.Projectile, Element.Poison) },
            { SNOActor.Goatmutant_Shaman_projectile_impact, new Avoidance(SNOActor.Goatmutant_Shaman_projectile_impact, AvoidType.Projectile, Element.Physical) },
            { SNOActor.lacuniFemale_bomb_impact_flameFingers, new Avoidance(SNOActor.lacuniFemale_bomb_impact_flameFingers, AvoidType.Projectile, Element.Physical) },
            { SNOActor.lordOfDespair_volley_projectile_groundImpact, new Avoidance(SNOActor.lordOfDespair_volley_projectile_groundImpact, AvoidType.Projectile, Element.Physical) },
            { SNOActor.MastaBlasta_Rider_projectile_impact, new Avoidance(SNOActor.MastaBlasta_Rider_projectile_impact, AvoidType.Projectile, Element.Physical) },
            { SNOActor.mistressOfPain_painBolt_impact, new Avoidance(SNOActor.mistressOfPain_painBolt_impact, AvoidType.Projectile, Element.Physical) },
            { SNOActor.MorluSpellcaster_Meteor_Impact, new Avoidance(SNOActor.MorluSpellcaster_Meteor_Impact, AvoidType.Projectile, Element.Physical) },
            { SNOActor.RattlerSkull_Impact, new Avoidance(SNOActor.RattlerSkull_Impact, AvoidType.Projectile, Element.Physical) },
            { SNOActor.SandWasp_Projectile_impact, new Avoidance(SNOActor.SandWasp_Projectile_impact, AvoidType.Projectile, Element.Physical) },
            { SNOActor.SandWasp_Projectile_targetImpact, new Avoidance(SNOActor.SandWasp_Projectile_targetImpact, AvoidType.Projectile, Element.Physical) },
            { SNOActor.skeletonMage_Cold_groundImpact, new Avoidance(SNOActor.skeletonMage_Cold_groundImpact, AvoidType.Projectile, Element.Cold) },
            { SNOActor.skeletonMage_fire_groundImpact, new Avoidance(SNOActor.skeletonMage_fire_groundImpact, AvoidType.Projectile, Element.Physical) },
            { SNOActor.skeletonMage_Lightning_impact, new Avoidance(SNOActor.skeletonMage_Lightning_impact, AvoidType.Projectile, Element.Lightning) },
            { SNOActor.skeletonMage_poison_groundImpact, new Avoidance(SNOActor.skeletonMage_poison_groundImpact, AvoidType.Projectile, Element.Poison) },
            { SNOActor.SkeletonSummoner_impact, new Avoidance(SNOActor.SkeletonSummoner_impact, AvoidType.Projectile, Element.Physical) },
            { SNOActor.TEMP_projectile_groundImpact_pool_pink, new Avoidance(SNOActor.TEMP_projectile_groundImpact_pool_pink, AvoidType.Projectile, Element.Physical) },
            { SNOActor.TEMP_projectile_impact_holy, new Avoidance(SNOActor.TEMP_projectile_impact_holy, AvoidType.Projectile, Element.Physical) },
            { SNOActor.TEMP_projectile_impact_pink, new Avoidance(SNOActor.TEMP_projectile_impact_pink, AvoidType.Projectile, Element.Physical) },
            { SNOActor.Wand_physical_impact, new Avoidance(SNOActor.Wand_physical_impact, AvoidType.Projectile, Element.Physical) },
            { SNOActor.wardenMissile_Impact, new Avoidance(SNOActor.wardenMissile_Impact, AvoidType.Projectile, Element.Physical) },
            { SNOActor.x1_Adria_homingProjectile_impact, new Avoidance(SNOActor.x1_Adria_homingProjectile_impact, AvoidType.Projectile, Element.Physical) },
            { SNOActor.x1_adria_projectile_impact, new Avoidance(SNOActor.x1_adria_projectile_impact, AvoidType.Projectile, Element.Physical) },
            { SNOActor.x1_bloodGolem_shaman_impact, new Avoidance(SNOActor.x1_bloodGolem_shaman_impact, AvoidType.Projectile, Element.Physical) },
            { SNOActor.x1_Cesspool_Slime_Posion_Attack_impact, new Avoidance(SNOActor.x1_Cesspool_Slime_Posion_Attack_impact, AvoidType.Projectile, Element.Physical) },
            { SNOActor.x1_Lieutenant_Mortar_Impact, new Avoidance(SNOActor.x1_Lieutenant_Mortar_Impact, AvoidType.Projectile, Element.Physical) },
            { SNOActor.x1_portalGuardianMinion_projectile_impact, new Avoidance(SNOActor.x1_portalGuardianMinion_projectile_impact, AvoidType.Projectile, Element.Physical) },
            { SNOActor.x1_Rockworm_Pand_impact, new Avoidance(SNOActor.x1_Rockworm_Pand_impact, AvoidType.Projectile, Element.Physical) },
            { SNOActor.x1_Urzael_Cannonball_burning_impact, new Avoidance(SNOActor.x1_Urzael_Cannonball_burning_impact, AvoidType.Projectile, Element.Physical) },
            { SNOActor.x1_Urzael_Cannonball_impact, new Avoidance(SNOActor.x1_Urzael_Cannonball_impact, AvoidType.Projectile, Element.Physical) },
            { SNOActor.x1_Urzael_Cannonball_impactSphere, new Avoidance(SNOActor.x1_Urzael_Cannonball_impactSphere, AvoidType.Projectile, Element.Physical) },
            { SNOActor.x1_Urzael_ceilingDebris_beam_impact_emitter, new Avoidance(SNOActor.x1_Urzael_ceilingDebris_beam_impact_emitter, AvoidType.Projectile, Element.Physical) },
            { SNOActor.x1_Urzael_ceilingDebris_beam_impact_shadow, new Avoidance(SNOActor.x1_Urzael_ceilingDebris_beam_impact_shadow, AvoidType.Projectile, Element.Physical) },
            { SNOActor.x1_Urzael_ceilingDebris_Impact_Beam, new Avoidance(SNOActor.x1_Urzael_ceilingDebris_Impact_Beam, AvoidType.Projectile, Element.Physical) },
            { SNOActor.x1_Urzael_ceilingDebris_Impact_Circle, new Avoidance(SNOActor.x1_Urzael_ceilingDebris_Impact_Circle, AvoidType.Projectile, Element.Physical) },
            { SNOActor.zoltunKulle_fieryBoulder_groundImpact, new Avoidance(SNOActor.zoltunKulle_fieryBoulder_groundImpact, AvoidType.Projectile, Element.Physical) },
            { SNOActor.zombie_female_barfBall_projectile_impact, new Avoidance(SNOActor.zombie_female_barfBall_projectile_impact, AvoidType.Projectile, Element.Physical) },
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
            340319, // PoisonEnchanted x1_MonsterAffix_CorpseBomber_bomb_start (340319)
            341512, // Thunderstorm x1_MonsterAffix_Thunderstorm_Impact
            337109, // Wormhole X1_MonsterAffix_TeleportMines

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

            //buddyMe add
            250031, // MonsterAffix_Mortar_Pending
            365810, // Grenadier_Proj_mortar_inpact
            300476, // x1_wizard_staticField_electricField
            //167628, // Wizard_FamiliarRune_Pierce_Glow
            299099, // x1_wizard_staticField_blastwaveGeo
            3851, // Corpulent_suicide_blood
            //99566, // Wizard_magicMissile_wobble-40033 Type=ServerProp
            159166, // g_monster_projectile_poison_impact-40858 Type=ServerProp
            217470, // grenadier_proj_trail
            142797, // zombie_female_barfBall_projectile_impact-38722 Type=ServerProp
            4219, // gibClusters_fire_humanoid-47261 Type=ServerProp
            5391, // SkeletonSummoner_impact-90121 Type=Projectile
            340343, // x1_skeletonArcher_arrow_cold_impact-103170 Type=ServerProp
            //4393, // g_chargedBolt_impact-113582 Type=ServerProp
            //226808, // monsterAffix_waller_model-121230 Type=ServerProp
            //85809, // monsterAffix_Vortex_proxy-130046 Type=ServerProp
            52689, // gibClusters_fire_humanoid_large-135376 Type=ServerPro
            //89862, // monsterAffix_Vortex_model-18897 Type=Gizmo
            334552, // x1_bloodScratch_leaperAngel_attack2-29155 Type=ServerProp
            98220, // monsterAffix_healthLink_jumpActor-29351 Type=ServerProp
            260812, // Unique_Monster_IceTrail-29820 Type=ServerProp
            159369, // MorluSpellcaster_Meteor_Pending-1262 Type=ServerProp
            160401, // demonFlyer_fireball_impact-1272 Type=ServerProp
            159368, // MorluSpellcaster_Meteor_Impact-1312 Type=ServerProp
            159367, // MorluSpellcaster_Meteor_afterBurn-1314 Type=ServerProp
            5214, // SandWasp_Projectile_impact-2157 Type=Projectile
            5373, // skeletonMage_fire_groundImpact-5478 Type=Projectile
            161448, // bloodScratch_azmodanBodyguard_attack02-46172 Type=ServerProp
            334547, // x1_bloodScratch_leaperAngel_attack1-37848 Type=ServerProp
            178102, // bloodScratch_angelCorrupt_attack01-51055 Type=ServerProp
            175452, // morluSpellcaster_teleport_trailActor-23356 Type=ServerProp
            72100, // brickHouse_swipe_attack01-38768 Type=ServerProp
            185843, // zoltunKulle_fieryBoulder_groundImpact-169060 Type=ServerProp
            99355, // Goatman_Shaman_Iceball_Explosion-106065 Type=ServerProp
            428810, // p2_morluSpellcaster_teleport_trailActor_cold-145111 Type=ServerProp
            349779, // x1_MonsterAffix_frozenPulse_shard-143059 Type=ServerProp
            149482, // bloodScratch_demonTrooper_attack01_model-10282 Type=ServerProp
            186055, // ZK_tornado_model-177099 Type=Projectile
            346976, // x1_Urzael_Cannonball_burning_impact
            159166, // g_monster_projectile_poison_impact-40858 Type=ServerProp
            142797, // zombie_female_barfBall_projectile_impact-38722 Type=ServerProp
            179234, // MastaBlasta_Rider_projectile_impact-116890 Type=Projectile
            4101, // fallenShaman_fireBall_impact-143262 Type=Projectile
            159164, // g_monster_projectile_cold_impact-29999 Type=ServerProp
            322355, // x1_portalGuardianMinion_projectile_impact-1758 Type=ServerProp
            182428, // mistressOfPain_painBolt_impact-24368 Type=ServerProp
            290108, // x1_westmarchRanged_projectile_impact-42165 Type=ServerProp
            290108, // x1_westmarchRanged_projectile_impact-48018 Type=ServerProp
            5369, // skeletonMage_Cold_groundImpact-57622 Type=ServerProp
            176534, // Goatmutant_Shaman_projectile_impact-126332 Type=ServerProp
            189480, // lordOfDespair_volley_projectile_groundImpact-48074 Type=Projectile
            5215, // SandWasp_Projectile_targetImpact-3639 Type=ServerProp
            266673, // x1_bloodScratch_bogFamily_grunt_attack02-4194 Type=ServerProp
            334773, // x1_bloodScratch_leaperAngel_attack1_permR-8905 Type=ServerProp
            334758, // x1_bloodScratch_leaperAngel_attack1_permL-8904 Type=ServerProp
            3346, // Beast_impactWave-59332 Type=ServerProp
            282455, // x1_Lieutenant_Mortar_Impact-180450 Type=Projectile
            4224, // gibClusters_fire_skeleton-32236 Type=ServerProp
            289811, // x1_bloodScratch_westmarchBrute_attackDecap-42762 Type=ServerProp
            159163, // g_monster_projectile_fire_impact-20140 Type=ServerProp
            5384, // skeletonMage_poison_groundImpact-72748 Type=Projectile
            //170199, // Wizard_teleport_castGlow-14804 Type=ServerProp
            266303, // x1_bloodScratch_bogFamily_brute_attack08_B-207570 Type=ServerProp
            5378, // skeletonMage_Lightning_impact-56968 Type=Projectile
            328379, // x1_squigglet_cast_emitter-13534 Type=ServerProp
            367950, // x1_Cesspool_Slime_Posion_Attack_impact-43600 Type=ServerProp
            155356, // emitter_spiral
            343582, // X1_MonsterAffix_Orbiter_FocalPoint-38229 Type=ServerProp
            343539, // Orbiter X1_MonsterAffix_Orbiter_Projectile
            346805, // x1_MonsterAffix_orbiter_projectile_orb-40179 Type=Projectile
            346837, // x1_MonsterAffix_orbiter_projectile_focus-40176 Type=Projectile
            346839, // x1_MonsterAffix_orbiter_glowSphere-40177 Type=Projectile

            //test
            154028, // GrenadeProxy_Indigo-83075
            337172, // x1_Bog_Bear_Trap_inHand-83343
            237062, // x1_Bog_Bear_Trap-88163
            290259, // x1_Bog_Bear_Trap_Fizzle_Client-89719
            337080, // x1_BogFamily_ranged_blowGun_model-92093
            339394, // x1_bloodScratch_bogFamily_grunt_attack05-95381
            266736, // x1_bloodScratch_bogFamily_grunt_attack03-96705
            272583, // x1_bogBlight_summon_cast_glowSphere-103855
            219808, // bloodScratch_morluMelee_attack02-159571
            177548, // angelCorrupt_dash_wave_model-164484
            120652, // a3dun_crater_Demon_GroundTrap_GasChamber-166670
            201912, // trOut_sign_arrow_south_Dock-606
            201911, // trOut_sign_arrow_north_Wilderness-617
            185366, // Demonic_Meteor_Impact-36403
            //6524, // Wizard_disintegrate_sourceGlow-57492
            290140, // x1_bloodScratch_deathMaiden_attack01-58676
            4102, // fallenShaman_fireBall_obj-3073
            230834, // SkeletonArcher_Jail-39897
            59401, // caOut_Oasis_Attack_Plant-67356
            3424, // BoneArcher_arrow-71648
            219793, // bloodScratch_morluMelee_attack01-99247
            148077, // wizard_rayOfFrost_dome_swirls3-88160
            3751, // Champion_Teleport_shell-111189
            5383, // skeletonMage_Poison_death-342304
            178418, // bloodScratch_soulRipper_attack02-362478
            137122, // Corpulent_suicide_spiders-376550
            147977, // wizard_rayOfFrost_dome_swirls1-138997
            3199, // ArcaneSummon_trailActor-4801
            3198, // ArcaneSummon_skeleton-4906
            4243, // gibClusters_poison_humanoid-11502
            4245, // gibClusters_poison_humanoid_small-34035
            52692, // gibClusters_poison_humanoid_large-3884
            337387, // x1_bloodScratch_leaperAngel_attack1_permL_rage-12533
            93892, // SkeletonKing_Teleport_arrival_proxy-173260
            93909, // SkeletonKing_Teleport_Back_trailDudeModel-175506
            83860, // SkeletonKing_Teleport_trailDudeModel-184327
            190198, // Despair_Teleport_shell-21820
            4606, // lightningRadialDisc-27787
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
            
            316389, // PoisonEnchanted x1_MonsterAffix_CorpseBomber_projectile (316389)
            340319, // PoisonEnchanted x1_MonsterAffix_CorpseBomber_bomb_start (340319)
            
            // A5
            338889, // x1_Adria_bouncingProjectile
            362850, // x1_Urzael_Cannonball_burning_invisible
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

            // buddyMe add
            //4394, //g_ChargedBolt_Projectile-200915 (4394) Type=Projectile
            368392, // x1_Cesspool_Slime_Posion_Attack_Projectile-222254 (368392) Type=Projectile
            //99566, // Wizard_magicMissile_wobble-40033 Type=ServerProp
            120957, // zombie_female_barfBall_projectile-38684 Type=Projectile
            312942, // x1_skeletonArcher_arrow_cold-49212 Type=Projectile
            228885, // monsterAffix_entangler_ringGlow_geo-57709 Type=Projectile
            4221, // gibClusters_fire_humanoid_small-81993 Type=Projectile
            340319, // x1_MonsterAffix_CorpseBomber_bomb_start-85444 Type=ServerProp
            316389, // x1_MonsterAffix_CorpseBomber_projectile-85594 Type=Projectile
            5392, // SkeletonSummoner_projectile-90107 Type=Projectile
            164829, // succubus_bloodStar_projectile-116681 Type=Projectile
            179226, // MastaBlasta_Rider_projectile-116739 Type=Projectile
            4220, // gibClusters_fire_humanoid_fat-121048 Type=Projectile
            366924, // x1_MonsterAffix_frozenPulse_shard_search-126831 Type=Projectile
            165123, // bloodScratch_Succubus_attack02_swipe1-129793 Type=Projectile
            52689, // gibClusters_fire_humanoid_large-135376 Type=ServerProp
            143266, // monsterAffix_frenzySwipe-139031 Type=Projectile
            179880, // hoodedNightmare_Lighting_projectile-18273 Type=Projectile
            4546, // lacuniFemale_bomb_projectile-18299 Type=Projectile
            4543, // lacuniFemale_bomb_groundMiss-18411 Type=Projectile
            323212, // x1_squigglet_projectile-18625 Type=Projectile
            4543, // lacuniFemale_bomb_groundMiss-19234 Type=ServerProp
            4546, // lacuniFemale_bomb_projectile-19277 Type=Projectile
            257306, // arcaneEnchantedDummy_spawn-19349 Type=ServerProp
            208962, // Spider_Elemental_Fire_tesla_A-21395 Type=Projectile
            337386, // x1_bloodScratch_leaperAngel_attack2_rage-22885 Type=Projectile
            137992, // Spider_Elemental_Fire_A-24160 Type=Projectile
            4981, // QuillDemonHorn_Projectile-39553 Type=Projectile
            192591, // battle_arrowLayerFire_far-7376 Type=Projectile
            180248, // battleFieldsBridge_fireBall-7929 Type=ServerProp
            322319, // x1_PortalGuardianMinion_projectile_geo-1678 Type=Projectile
            169669, // bloodScratch_GoatMutant_Melee_attack02-51079 Type=Projectile
            169665, // bloodScratch_GoatMutant_Melee_attack01-69048 Type=Projectile
            161448, // bloodScratch_azmodanBodyguard_attack02-9527 Type=Projectile
            339972, // Poison_Glob-13268 Type=Projectile
            //71129, // Wizard_meteor_distortExplosion-23927 Type=Projectile
            226722, // monsterAffix_Avenger_glowSphere-55342 Type=Projectile
            208963, // Spider_Elemental_Poison_tesla_A-22687 Type=ServerProp
            180206, // MistressOfPain_painBolt_projectile-24296 Type=Projectile
            221658, // MonsterAffix_ArcaneEnchanted_trailActor-29232 Type=ServerProp
            173299, // MorluSpellcaster_meteor_model-18782 Type=Projectile
            4764, // morluSpellcast_meteor_castSphere-23153 Type=ServerProp
            360430, // x1_westmarchRanged_projectile_invisible-41996 Type=Projectile
            290043, // x1_westmarchRanged_projectile-42109 Type=Projectile
            290043, // x1_westmarchRanged_projectile-48008 Type=Projectile
            360430, // x1_westmarchRanged_projectile_invisible-48137 Type=Projectile
            290109, // x1_westmarchRanged_projectile_miss-51450 Type=ServerProp
            93629, // SkeletonKing_Teleport_Projectile-78908 Type=Projectile
            5361, // SkeletonKing_Ghost_attackModel-80987 Type=Projectile
            80143, // goatWarrior_shaman_projectile-106030 Type=Projectile
            185679, // zoltunKulle_fieryBoulder_projectile-169021 Type=Projectile
            4218, // gibClusters_fire_Beast-14608 Type=Projectile
            161310, // bloodScratch_azmodanBodyguard_attack01-30679 Type=Projectile
            337388, // x1_bloodScratch_leaperAngel_attack1_permR_rage-33197 Type=Projectile
            5370, // skeletonMage_Cold_projectile-57612 Type=Projectile
            223933, // monsterAffix_plagued_groundGeo-46188 Type=ServerProp
            71686, // creepMob_burrowArm_projectile-83476 Type=Projectile
            176406, // GoatMutant_Shaman_blast_projectile-126320 Type=ServerProp
            189476, // lordOfDespair_volley_projectile-47945 Type=Projectile
            89588, // soulRipper_tongue_proxy_projectile-14605 Type=Projectile
            5374, // skeletonMage_Fire_projectile-5282 Type=Projectile
            4547, // lacuniFemale_bomb_projectile_actor-16918 Type=Projectile
            332231, // x1_bloodScratch_westmarchBrute_B_attack01A-45540 Type=Projectile
            341902, // x1_bloodScratch_deathMaiden_fire_attack01-207658 Type=Projectile
            202859, // g_monster_projectile_poison_globModel-38106 Type=ServerProp
            //6513, // Wizard_arcaneOrb_aoe_blastWave-108621 Type=Projectile
            4074, // fallenChamp_attack1Swipe-150326 Type=Projectile
            282452, // x1_Lieutenant_Mortar_projectile-180295 Type=Projectile
            141867, // bloodScratch_terrorDemon_attack01_swipe2-21043 Type=Projectile
            5385, // skeletonMage_Poison_projectile-71726 Type=Projectile
            5379, // skeletonMage_Lightning_projectile-56957 Type=Projectile
            337385, // x1_bloodScratch_leaperAngel_attack1_rage-68359 Type=Projectile
            184029, // bloodScratch_Despair_attack270-133528 Type=Projectile
            316389, // PoisonEnchanted x1_MonsterAffix_CorpseBomber_projectile (316389)
            338889, // x1_Adria_bouncingProjectile
            377086, // X1_Unique_Monster_Generic_Projectile_Arcane
            377087, // X1_Unique_Monster_Generic_Projectile_Cold
            377088, // X1_Unique_Monster_Generic_Projectile_Fire
            377089, // X1_Unique_Monster_Generic_Projectile_Holy
            377090, // X1_Unique_Monster_Generic_Projectile_Lightning
            377091, // X1_Unique_Monster_Generic_Projectile_Physical
            377092, // X1_Unique_Monster_Generic_Projectile_Poison
            120957, // zombie_female_barfBall_projectile-38684 Type=Projectile
            74501, // a1dun_leor_firewall1_dist-30329 Type=Projectile
            284752, // x1_Bog_Bear_Trap_projectile_lobbed-36341 Type=Projectile
            6040, // TriuneSummoner_fireball_projectile-36483 Type=Projectile
            347298, // x1_DarkAngel_Summon_groundFog_projectile-2225 Type=Projectile
            377326, // x1_monsterAffix_generic_coldDOT_runeGeo-15147 Type=Projectile
            226799, // monsterAffix_ghostly_distGeo-12269 Type=Projectile
            5213, // sandWasp_Projectile_actor-4209 Type=ServerProp
            284766, // _Bog_Bear_Trap_projectile_Tossed-4833 Type=ServerProp
            273844, // x1_bogBlight_pustule_projectile-6413 Type=Projectile
            189247, // a4dun_Garden_Corruption_Mine_projectile-37646
            467, // TriuneSummoner_fireBall_obj-106116
            159165, // g_monster_projectile_lightning_impact-284336
            373937, // X1_LR_Boss_FireNova_projectile-172413
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
            159369, // MorluSpellcaster_Meteor_Pending-178011 (159369)
        };

        /// <summary>
        /// The duration the AoE from AvoidanceSpawners should be avoided for
        /// </summary>
        public static Dictionary<int, TimeSpan> AvoidanceSpawnerDuration { get { return avoidanceSpawnerDuration; } }
        private static readonly Dictionary<int, TimeSpan> avoidanceSpawnerDuration = new Dictionary<int, TimeSpan>
        {
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

            {118595, 12f}, // ANIM * A3_Battlefield_DemonMine_C_death 
        };

        /*
         * Combat-related dictionaries/defaults
         */

        /// <summary>
        /// ActorSNO's of Very fast moving mobs (eg wasps }, for special skill-selection decisions
        /// </summary>
        public static HashSet<int> FastMovingMonsterIds { get { return fastMovementMonsterIds; } }
        private static readonly HashSet<int> fastMovementMonsterIds = new HashSet<int> {
            5212
         };

        /// <summary>
        /// A list of crappy "summoned mobs" we should always ignore unless they are very close to us, eg "grunts", summoned skeletons etc.
        /// </summary>
        public static HashSet<int> ShortRangeAttackMonsterIds { get { return shortRangeAttackMonsterIds; } }
        private static readonly HashSet<int> shortRangeAttackMonsterIds = new HashSet<int> {
            4084, 4085, 5395, 144315,
         };

        /// <summary>
        /// Dictionary of Actor SNO's and cooresponding weighting/Priority 
        /// </summary>
        public static Dictionary<int, int> MonsterCustomWeights { get { return monsterCustomWeights; } }
        private static readonly Dictionary<int, int> monsterCustomWeights = new Dictionary<int, int> {
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
        private static readonly HashSet<int> goblinIds = new HashSet<int> {
            (int)SNOActor.Lore_Bestiary_TreasureGoblin,
            (int)SNOActor.treasureGoblin_A,
            (int)SNOActor.treasureGoblin_A_Slave,
            (int)SNOActor.treasureGoblin_B,
            (int)SNOActor.treasureGoblin_C,
            (int)SNOActor.treasureGoblin_C_Unique_DevilsHand,
            (int)SNOActor.treasureGoblin_D,
         };

        /// <summary>
        /// Contains ActorSNO of ranged units that should be attacked even if outside of kill radius
        /// </summary>
        public static HashSet<int> RangedMonsterIds { get { return rangedMonsterIds; } }
        private static readonly HashSet<int> rangedMonsterIds = new HashSet<int> {
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
            // Siegebreaker (96192 }, Azmodan (89690 }, Cydea (95250 }, Heart-thing (193077 }, 
            96192,                   89690,           95250,         193077, 
            //Kulle (80509 }, Small Belial (220160 }, Big Belial (3349 }, Diablo 1 (114917 }, terror Diablo (133562)
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
            // Jondar, Chancellor, Queen Araneae (act 1 dungeons }, Skeleton King, Butcher
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

            255704, // GoatMutant_Ranged_A_Unique_Uber - Odeg the Keywarden
            256022, // DuneDervish_B_Unique_Uber - Sokahr the Keywarden
            256040, // morluSpellcaster_A_Unique_Uber - Xah'Rith Keywarden
            256054, // TerrorDemon_A_Unique_Uber - Nekarat the Keywarden
        };

        // A list of Unique mini-bosses in the game, just to make CERTAIN they are treated as elites
        /// <summary>
        /// Contains ActorSNO of known mini-bosses, work in progress by Kevin S.
        /// Will be useful for any achievements profile, and more
        /// </summary>
        public static HashSet<int> EliteRareUniqueIds { get { return eliteRareUniqueIds; } }
        private static readonly HashSet<int> eliteRareUniqueIds = new HashSet<int>
        {
		//A1
		361347, // Adventurer_D_TemplarIntroUnique_AdventureMode - Rad'Noj
		129439, // Spider_A_Unique - Arsect The Venomous
		218456, // Spiderling_A_Unique_01 - Venimite
		108444, // ZombieFemale_A_TristramQuest (Wretched Mothers)
        176889, // ZombieFemale_Unique_WretchedQueen
        76676, // Goatman_Shaman_B_Unique
        76953, // Unburied_A_Unique
        81342, // GoatMutant_Melee_A_Unique_Gharbad
        81533, // Goatman_Shaman_A_Event_Graveyard_Unique
        82563, // Unburied_A_Unique_LeoricBoss
        85971, // Ghost_A_Unique_House1000Undead
        86624, // Adventurer_D_TemplarIntroUnique
        104247, // Scavenger_B_Armorer_Unique
        105620, // Zombie_Inferno_C_Unique
        111321, // Zombie_Plagued_C_Unique
		496, // WoodWraith_Unique_A
		115403, // Skeleton_A_Cain_Unique
        129997, // ThousandPounder_Unique
        131131, // TriuneSummoner_A_Unique_SwordOfJustice
        156353, // Ghost_A_Unique_Chancellor
        156511, // Triune_Berserker_C_Unique_AlcarnusBridge
        156801, // Unique_CaptainDaltyn
        165602, // Ghost_D_Unique01
        167205, // Scavenger_B_Unique_ScavengerFarm
		//A2
		164502, // sandMonster_A_Head_Guardian
		217744, // Nine Toads
		222011, // Scar Talon
		222180, // Torsar
		168951, // snakeMan_Caster_A_Unique_WaterfallAmbush - Ssthrass
		115132, // LacuniMale_B_Unique_OasisLacuniAmbush - Leodesh the Stalker
		140424, // Mundunogo
		113994, // Corpulent_C_OasisAmbush_Unique - Fezuul
		258955, // Bramok the Overlord
		208543, // Ashangu
		259187, // Rakanishu
		222236, // Grool
		222413, // sandMonster_B_Unique_01
        222523, // sandMonster_C_Unique_01
		5203, // SandShark_Unique_Mother
        59593, // LacuniMale_B_UniqueTower
        59970, // LacuniFemale_A_Unique
        60583, // Khamsin_Mine_Unique
		111868, // DuneDervish_A_DervishTwister_Unique
		166133, // FallenShaman_A_Unique01
		168240, // Zombie_Unique_JewelerQuest
		323525, // LacuniFemale_C_OpenWorld_Unique
		//A3
		//A4
		196102, // TerrorDemon_A_Unique_1000Monster
		//A5
		361973, // x1_BogFamily_ranged_Unique_A
        361974, // x1_BogFamily_ranged_Unique_B
        361991, // x1_NightScreamer_Unique_B
        362299, // x1_MoleMutant_Melee_Unique_A
        362303, // x1_MoleMutant_Melee_Unique_B
        362305, // x1_moleMutant_Ranged_Unique_A
        362307, // x1_MoleMutant_Shaman_Unique_A
        362309, // x1_moleMutant_Ranged_Unique_B
        362310, // x1_MoleMutant_Shaman_Unique_B
        362891, // X1_armorScavenger_Unique_A
        362895, // X1_armorScavenger_Unique_B
        363051, // x1_Rockworm_Pand_Unique_A
        363060, // x1_Rockworm_Pand_Unique_B
        363073, // x1_Squigglet_Unique_A
        363108, // x1_Squigglet_Unique_B
        363228, // x1_leaperAngel_Unique_A
        363230, // x1_leaperAngel_Unique_B
        363232, // x1_Wraith_Unique_A
        363361, // x1_Wraith_Unique_B
        363367, // x1_sniperAngel_Unique_A
        363374, // x1_sniperAngel_Unique_B
        363378, // x1_FortressBrute_Unique_A
        363421, // x1_FortressBrute_Unique_B
        363910, // x1_westmarchHound_Leader_Unique_A
        363986, // x1_westmarchHound_Leader_Unique_B
        363988, // x1_westmarchHound_Unique_A
        363990, // x1_westmarchHound_Unique_B
        365050, // X1_demonTrooper_Demon_Event_Unique
        365101, // X1_demonTrooper_MouseTrap_Event_Unique
		273418, // x1_DeathMaiden_Unique_A
        273419, // x1_DeathMaiden_Unique_B
        274324, // x1_DeathMaiden_Unique_C
        284676, // x1_westmarchRanged_A_Unique_01
        284677, // x1_westmarchRanged_A_Unique_02
        288471, // x1_WickerMan_Unique_A
        294987, // x1_Tentacle_Goatman_Melee_A_Unique
        307099, // x1_FloaterAngel_Unique_03
        307329, // X1_Angel_Trooper_Unique_HexMaze
        307331, // X1_MastaBlasta_Rider_A_Unique_HexMaze
        307333, // X1_armorScavenger_Unique_HexMaze
        307335, // X1_BigRed_Unique_HexMaze
        307339, // X1_Rockworm_Pand_Unique_HexMaze
        309462, // x1_FloaterAngel_Unique_04
        309508, // x1_westmarchBrute_Unique_Event_Pontificus
        311343, // x1_westmarchBrute_Unique_B
        321953, // x1_westmarchBrute_A_Unique_captainStokely
        323524, // x1_westmarchBrute_Unique_C
        328026, // x1_Succubus_Doomed_Unique_A
        329999, // x1_westmarchBrute_B_Unique_Event_BrutelyUnfortunate
        330456, // x1_westmarchBrute_A_Unique_FireAmbush
        332432, // x1_devilshand_unique_SkeletonSummoner_B
        332433, // x1_devilshand_unique_TriuneSummoner_C
        332861, // x1_westmarchFemale_A_Graveyard_Unique_1
        334282, // X1_armorScavenger_Unique_Event_Worldstone
		335078, // x1_westmarchRanged_Graveyard_Unique_1
        336400, // x1_Skeleton_Westmarch_A_UniqueEvent_MassacredGuards
        336418, // x1_BogFamily_Brute_Unique_A
        336800, // X1_Fast_ZombieSkinny_Unique_A
        339754, // x1_Monstrosity_ScorpionBug_A_event_unique
        340326, // x1_BogFamily_brute_unique_familyEvent_A
        340452, // x1_devilshand_unique_Rockworm_A3
        341104, // x1_BogFamily_ranged_A_unique_hunter
        341240, // x1_TriuneSummoner_C_Unique_01
        341273, // x1_bogBlight_Maggot_A_unique_deathGrub
        341598, // x1_BogBlight_A_Unique_MaggotCrew
		354549, // x1_MoleMutant_Garden_Unique_A
        354550, // x1_MoleMutant_Garden_Unique_B
        354551, // x1_MoleMutant_Garden_Unique_C
        354552, // x1_MoleMutant_Garden_Unique_D
        354582, // x1_MoleMutant_Garden_Unique_E
        355667, // x1_DeathMaiden_Unique_Fire_A
        355672, // x1_DeathMaiden_Unique_Fire_B
        355680, // x1_DeathMaiden_Unique_Fire_C
        356380, // x1_BogFamily_brute_BogMonsterEvent_Unique
        356781, // x1_bogBlight_Maggot_A_unique_MaggotLoad
        356808, // x1_leaperAngel_A_Unique_LeaperOfSouls
        356912, // X1_Shield_Skeleton_Westmarch_Unique_YardRush
        357048, // x1_FloaterAngel_A_ZombieSorcerer_Unique
        357197, // x1_Westm_Graveyard_Ghost_Female_01_UniqueEvent
        359982, // X1_Shield_Skeleton_Westmarch_Unique_SkeletonRush
        360206, // x1_WestM_Intro_BadGuy_Unique
        360241, // x1_DeathMaiden_Pand_A_FortressUnique
        360242, // x1_leaperAngel_A_FortressUnique
        360243, // x1_sniperAngel_A_FortressUnique
        360244, // x1_Wraith_A_FortressUnique
        360245, // x1_westmarchBrute_C_FortressUnique
		348771, // x1_DeathMaiden_Unique_Heaven
        349156, // x1_BogFamily_melee_A_unique_key
        350754, // x1_BogBlight_MME_Unique_A
        351179, // x1_BogFamily_melee_A_Unique_DH
        351183, // x1_DeathMaiden_Unique_A_DH
        351252, // x1_BogBlight_MaggotDinnerParty_Unique
        353240, // X1_Fast_Zombie_A_GraveRobertUnique
        354378, // x1_Squigglet_A_unique_cellarEventB
		360826, // x1_Ghost_Dark_Unique_A
        360842, // x1_Ghost_Dark_Unique_B
        360849, // x1_FloaterAngel_Unique_05
        360853, // x1_Shield_Skeleton_Westmarch_Unique_A
        360858, // x1_Skeleton_Westmarch_Unique_A
        360861, // x1_SkeletonArcher_Westmarch_Unique_A
        360864, // x1_westmarchBrute_Unique_D
        360869, // x1_Ghost_Dark_Unique_C
        360881, // x1_westmarchRanged_Unique_A
        361088, // X1_Plagued_LacuniMale_Unique_A
        361099, // X1_Plagued_LacuniMale_Unique_B
        361129, // x1_DeathMaiden_Unique_Heaven_VO
        361291, // x1_Dark_Angel_Unique_A
        361313, // x1_Dark_Angel_Unique_B
		361417, // x1_westmarchRanged_Unique_B
        361419, // X1_Fast_ZombieSkinny_Unique_B
        361755, // x1_NightScreamer_Unique_A
        361771, // x1_BogFamily_Brute_Unique_B
        361952, // x1_BogFamily_ranged_A_unique_hunter_B
		368175, // x1_Beast_Skeleton_Unique_A
        369424, // x1_DeathMaiden_Unique_D
        369430, // x1_Ghost_Dark_Unique_D
        369435, // x1_FloaterAngel_Unique_06
        369465, // x1_westmarchRanged_Unique_C
        369466, // x1_westmarchRanged_Unique_D
        369467, // x1_westmarchRanged_Unique_E
        369505, // x1_westmarchRanged_Unique_abattoir_DeadEndDoorAmbush
		370283, // x1_Squigglet_Unique_C
        370768, // x1_BogBlight_Unique_A
        370800, // x1_WestmarchBat_Unique_B
		365759, // X1_BigRed_Chronodemon_Burned_A_unique
        365850, // x1_demonMelee_Catacombs_Mutant_Event_Unique
		373871, // x1_westmarchBrute_C_Unique_01
        373873, // x1_sniperAngel_Unique_C
        373879, // x1_moleMutant_Ranged_Unique_C
        373881, // x1_MoleMutant_Shaman_Unique_C
        373883, // x1_Squigglet_Unique_D
        373892, // X1_armorScavenger_Unique_C
        374739, // x1_Monstrosity_ScorpionBug_Unique_B
        374987, // X1_Spider_Poison_A_Unique_01
        375398, // x1_DeathMaiden_Unique_Fire_AbattoirFurnace
        375402, // x1_WitherMoth_A_Unique_01
		373819, // X1_Fast_Zombie_Unique_A
        373821, // x1_westmarchRanged_Unique_F
        373830, // x1_Shield_Skeleton_Westmarch_Unique_B
        373833, // TentacleHorse_Fat_Unique_B
        373842, // x1_MoleMutant_Melee_Unique_C
        373848, // x1_Monstrosity_ScorpionBug_Unique_A

        169533, // Goatman_Shaman_B_Unique_MysticWagon
        176889, // ZombieFemale_Unique_WretchedQueen
        178619, // TownAttack_Summoner_Unique
        189906, // TriuneVesselActivated_A_Unique_Tower_Of_Power
        195639, // Corpulent_D_CultistSurvivor_Unique
        201679, // TentacleHorse_B_Unique_01
        201878, // QuillDemon_A_Unique_LootHoarderLeader
        203795, // fastMummy_B_FacePuzzleUnique
        207605, // Ghost_D_FacePuzzleUnique
        207838, // CoreEliteDemon_A_NoPod_Unique
        208543, // FallenShaman_A_Unique_MiniPools
        209506, // TentacleHorse_A_Unique_01
        209553, // Ghost_A_Unique_01
        209596, // Succubus_A_Unique_01
        209608, // ZombieSkinny_A_Unique_01
        212664, // TentacleBear_A_Unique_01
        212667, // tentacleFlower_A_Unique_01
        212731, // unique_talRashasLidlesseye_model
        212750, // LacuniFemale_C_Unique
        212942, // ThousandPounder_B_Unique
        214948, // TentacleHorse_C_Unique_01
        215445, // FallenShaman_A_Unique01Whipple
        217479, // WoodWraith_Unique_A_Static
        217744, // fastMummy_C_Unique
        218206, // graveDigger_B_Ghost_Unique
        218270, // ZombieSkinny_A_Unique_02
        218307, // Corpulent_A_Unique_01
        218308, // Corpulent_A_Unique_02
        218314, // FleshPitFlyer_A_Unique_01
        218321, // Skeleton_A_Unique_02
        218332, // Scavenger_A_Unique_01
        218345, // ZombieSkinny_A_Unique_03
        218348, // graveRobber_A_Ghost_Unique_01
        218351, // graveRobber_A_Ghost_Unique_02
        218356, // Unburied_A_Unique_01
        218362, // FleshPitFlyer_A_Unique_02
        218364, // Skeleton_A_Unique_03
        218396, // Shield_Skeleton_A_Unique_01
        218400, // SkeletonArcher_A_Unique_01
        218405, // Corpulent_B_Unique_01
        218422, // Beast_A_Unique_01
        218424, // Scavenger_B_Unique_01
        218428, // Goatman_Melee_A_Unique_01
        218431, // ZombieSkinny_A_Unique_04
        218441, // Ghost_A_Unique_02
        218444, // Ghoul_A_Unique_01
        218448, // Spider_A_Unique_01
        218456, // Spiderling_A_Unique_01
        218458, // Spider_Poison_A_Unique_01
        218462, // Spider_Poison_A_Unique_02
        218469, // Goatman_Melee_B_Unique_01
        218473, // Goatman_Ranged_A_Unique_01
        218508, // Goatman_Shaman_A_Unique_01
        218536, // Beast_A_Unique_02
        218566, // FleshPitFlyer_C_Unique_01
        218656, // TriuneCultist_A_Unique_01
        218662, // TriuneSummoner_A_Unique_01
        218664, // TriuneSummoner_A_Unique_02
        218666, // ZombieSkinny_A_Unique_05
        218672, // Triune_Berserker_A_Unique_01
        218674, // Triune_Berserker_A_Unique_02
        218676, // TriuneCultist_A_Unique_02
        218678, // Triune_Berserker_A_Unique_03
        218802, // TentacleHorse_A_Unique_02
        218804, // TentacleHorse_Fat_A_Unique_01
        218806, // TentacleHorse_A_Unique_03
        218807, // TentacleHorse_A_Unique_04
        218808, // TentacleHorse_A_Unique_05
        218873, // MorluSpellcaster_A_Sao_Unique
        219651, // CoreEliteDemon_A_Unique_01
        219668, // MastaBlasta_Rider_A_Unique_01
        219725, // ZombieFemale_A_TristramQuest_Unique
        219727, // BigRed_A_Unique_01
        219736, // MalletDemon_A_Unique_01
        219751, // MalletDemon_A_Unique_02
        219847, // Succubus_C_Unique_01
        219893, // Angel_Corrupt_A_Unique_03
        219916, // MastaBlasta_Steed_A_Unique_01
        219925, // morluMelee_A_Unique_01
        219936, // morluMelee_A_Unique_02
        219949, // HoodedNightmare_A_Unique_01
        219960, // HoodedNightmare_A_Unique_02
        219985, // morluSpellcaster_A_Unique_01
        219995, // ZombieSkinny_A_Unique_06
        220034, // Triune_Berserker_A_Unique_04
        220232, // demonFlyer_A_Unique_01
        220377, // FallenChampion_D_Unique_01
        220381, // FallenHound_D_Unique_01
        220395, // demonTrooper_A_Unique_02
        220397, // demonTrooper_A_Unique_03
        220435, // FallenHound_D_Unique_02
        220444, // SoulRipper_A_Unique_01
        220455, // QuillDemon_C_Unique_01
        220468, // Shield_Skeleton_E_Unique_01
        220476, // demonTrooper_B_Unique_01
        220479, // skeleton_twoHander_Keep_Swift_E_Unique_01
        220481, // SoulRipper_A_Unique_02
        220485, // Brickhouse_A_Unique_01
        220491, // Brickhouse_A_Unique_02
        220499, // SkeletonArcher_E_Unique_01
        220509, // creepMob_A_Unique_01
        220683, // graveRobber_A_Ghost_Unique_03
        220688, // GoatMutant_Melee_A_Unique_01
        220691, // fastMummy_C_Unique_01
        220699, // GoatMutant_Melee_A_Unique_02
        220701, // demonFlyer_B_Unique_01
        220705, // GoatMutant_Ranged_A_Unique_01
        220708, // GoatMutant_Melee_A_Unique_03
        220710, // Shield_Skeleton_E_Unique_02
        220727, // GoatMutant_Shaman_A_Unique_01
        220773, // ThousandPounder_Unique_01
        220775, // demonFlyer_B_Unique_02
        220777, // Rockworm_A3_Crater_Unique_01
        220783, // Succubus_A_Unique_02
        220789, // creepMob_A_Unique_02
        220795, // Monstrosity_Scorpion_A_Unique_01
        220806, // azmodanBodyguard_A_Unique_01
        220810, // Ghoul_E_Unique_01
        220812, // azmodanBodyguard_A_Unique_02
        220814, // SoulRipper_A_Unique_03
        220817, // Monstrosity_Scorpion_A_Unique_02
        220850, // GoatMutant_Shaman_B_Unique_01
        220851, // Rockworm_A3_Crater_Unique_02
        220853, // GoatMutant_Ranged_B_Unique_01
        220857, // GoatMutant_Melee_B_Unique_01
        220862, // GoatMutant_Melee_B_Unique_02
        220868, // GoatMutant_Shaman_B_Unique_02
        220881, // ThousandPounder_C_Unique_01
        220884, // azmodanBodyguard_A_Unique_03
        220889, // Monstrosity_Scorpion_B_Unique_01
        220982, // Triune_Berserker_E_Unique
        221367, // LacuniFemale_A_Unique_01
        221372, // LacuniFemale_A_Unique_02
        221377, // FallenChampion_A_Unique_01
        221379, // FallenChampion_A_Unique_02
        221402, // SandShark_A_Unique_01
        221406, // FallenGrunt_A_Unique_01
        221442, // Triune_Berserker_B_Unique_01
        221656, // a1dun_Leoric_Unburied_A_Unique
        221810, // Triune_Summonable_D_Unique_01
        221981, // TriuneCultist_C_Unique_01
        221999, // TriuneCultist_C_Unique_02
        222001, // TriuneSummoner_C_Unique_01
        222003, // Triune_Berserker_C_Unique_01
        222005, // snakeMan_Melee_A_Unique_01
        222008, // snakeMan_Caster_A_Unique_01
        222011, // Bloodhawk_A_Unique_01
        222180, // DuneDervish_B_Unique_01
        222186, // fastMummy_B_Unique_01
        222189, // Swarm_B_Unique_01
        222236, // Ghoul_B_Unique_01
        222238, // snakeMan_Melee_A_Unique_02
        222335, // FallenChampion_B_Unique_01
        222339, // LacuniFemale_B_Unique_01
        222352, // Sandling_B_Unique_01
        222385, // Bloodhawk_A_Unique_02
        222400, // fastMummy_B_Unique_02
        222413, // sandMonster_B_Unique_01
        222427, // FleshPitFlyer_C_Unique_02
        222502, // skeletonMage_Cold_B_Unique_01
        222510, // skeletonMage_Fire_B_Unique_01
        222511, // skeletonMage_Lightning_B_Unique_01
        222512, // skeletonMage_Poison_B_Unique_01
        222523, // sandMonster_C_Unique_01
        222526, // Ghost_D_Unique_01
        223691, // morluSpellcaster_A_Unique_Sigil
        225502, // graveDigger_B_Ghost_Unique_01
        226509, // ZombieSkinny_A_Unique_Marko
        229946, // Triune_Berserker_B_G_Unique_01
        229948, // Triune_Berserker_B_G_Unique_02
        229950, // Triune_Berserker_B_G_Unique_03
        230757, // skeletonMage_Fire_B_Unique_BloodGuardian
        257972, // Unique_Monster_Earthquake_Prototype
        260226, // creepMob_A_MedicalCamp_Unique
        260227, // demonTrooper_A_Reinforcements_Unique
        260228, // FallenChampion_B_PrisonersEvent_Unique
        260229, // FallenGrunt_A_Rakanishu_Unique
        260230, // FallenGrunt_C_RescueEscort_Unique
        260231, // FleshPitFlyer_B_FarmhouseAmbush_Unique
        260232, // Ghoul_A_NephMonument_Unique
        260233, // Ghoul_E_BlazeOfGlory_Unique
        260234, // Scavenger_B_MinerEvent_Unique
        260235, // Skeleton_D_Fire_BlacksmithEvent_Unique
        260236, // Triune_Berserker_B_RestlessSands_Unique
        260812, // Unique_Monster_IceTrail
        334402, // BigRed_A_Unique_03
        334765, // TerrorDemon_A_Unique_01
        343033, // ThousandPounder_C_Unique_DevilsHand
        343046, // treasureGoblin_C_Unique_DevilsHand
        360614, // Bloodhawk_A_Unique_HexMaze
        361349, // Unique_CaptainDaltyn_AdventureMode
        361972, // QuillDemon_Mother_Unique
        365330, // Goatman_Melee_A_Unique_03
        365335, // Beast_A_Unique_03
        365425, // Skeleton_B_Unique_01
        365429, // Skeleton_B_Unique_02
        365438, // Goatman_Shaman_B_Unique_01
        365450, // Corpulent_A_Unique_03
        365465, // Corpulent_D_Unique_Spec_01
        365906, // Unburied_C_Unique_01
        366975, // Goatman_Shaman_C_Unique_01
        366981, // Goatman_Shaman_C_Unique_02
        366990, // TriuneCultist_B_Unique_01
        366998, // TriuneCultist_A_Unique_03
        367006, // FallenChampion_B_Unique_02
        367011, // DuneDervish_A_Unique_01
        367018, // FallenGrunt_B_Unique_01
        367073, // snakeMan_Caster_B_Unique_01
        367095, // snakeMan_Caster_B_Unique_02
        367096, // snakeMan_Melee_B_Unique_01
        367333, // demonTrooper_B_Unique_02
        367335, // demonTrooper_B_Unique_03
        367341, // FallenShaman_C_Unique_01
        367360, // ThousandPounder_B_Unique_02
        367366, // demonFlyer_C_unique_01
        367371, // FallenHound_E_Unique_01
        370238, // TentacleHorse_B_Unique_02
        373017, // ZombieFemale_Spitter_Unique_A
        373869, // TentacleBear_C_Unique_01
		};

        // Three special lists used purely for checking for the existance of a player's summoned mystic ally, gargantuan, or zombie dog

        public static HashSet<int> MysticAllyIds { get { return mysticAllyIds; } }
        private static readonly HashSet<int> mysticAllyIds = new HashSet<int> { 
            169123, 123885, 169890, 168878, 169891, 169077, 169904, 169907, 169906, 169908, 169905, 169909 
        };

        public static HashSet<int> GargantuanIds { get { return gargantuanIds; } }
        private static readonly HashSet<int> gargantuanIds = new HashSet<int> { 
            179780, 179778, 179772, 179779, 179776, 122305 };

        public static HashSet<int> ZombieDogIds { get { return zombieDogIds; } }
        private static readonly HashSet<int> zombieDogIds = new HashSet<int> { 
            110959, 103235, 103215, 105763, 103217, 51353, 
        };

        public static HashSet<int> FetishArmyIds { get { return fetishArmyIds; } }
        private static readonly HashSet<int> fetishArmyIds = new HashSet<int> { 
            87189, 89934, 90072, 409656, 410238, 89933 }; // 409590 Name: Fetish_Melee_Sycophants-2004 //182271

        public static HashSet<int> BigBadVoodooIds { get { return bigBadVoodooIds; } }
        private static readonly HashSet<int> bigBadVoodooIds = new HashSet<int> { 
            (int)SNOActor.Witchdoctor_BigBadVoodoo_fetish,
            (int)SNOActor.Witchdoctor_BigBadVoodoo_fetish_blue,
            (int)SNOActor.Witchdoctor_BigBadVoodoo_fetish_purple,
            (int)SNOActor.Witchdoctor_BigBadVoodoo_fetish_red,
            (int)SNOActor.Witchdoctor_BigBadVoodoo_fetish_yellow,
        };

        public static HashSet<int> DemonHunterPetIds { get { return demonHunterPetIds; } }
        private static readonly HashSet<int> demonHunterPetIds = new HashSet<int> { 
            178664, 
            173827, 
            133741, 
            159144, 
            181748, 
            159098,
            159102,
            159144,
            334861,
            150025,
            150037,
            155149,
            154593,
            154199,
            147960,
            129934,
            130366,
            367223,
            367258,
            160612,
        };

        public static HashSet<int> DemonHunterSentryIds { get { return demonHunterSentryIds; } }
        private static readonly HashSet<int> demonHunterSentryIds = new HashSet<int> { 
           141402, 150025, 150024, 168815, 150026, 150027,
        };

        public static HashSet<int> WizardHydraIds { get { return wizardHydraIds; } }
        private static readonly HashSet<int> wizardHydraIds = new HashSet<int>
        { 
            80745, // Wizard_HydraHead_Default_1
            80757, // Wizard_HydraHead_Default_2
            80758, // Wizard_HydraHead_Default_3
        };

        public static HashSet<int> FollowerIds { get { return followerIds; } }
        private static readonly HashSet<int> followerIds = new HashSet<int>
        { 
            4482, // Enchantress
            4538, // Templar
            4644, // Scoundrel
        };


        /// <summary>
        /// World-object dictionaries eg large object lists, ignore lists etc.
        /// A list of SNO's to *FORCE* to type: Item. (BE CAREFUL WITH THIS!).
        /// </summary>
        public static HashSet<int> ForceToItemOverrideIds { get { return forceToItemOverrideIds; } }
        private static readonly HashSet<int> forceToItemOverrideIds = new HashSet<int> {
            166943, // DemonTrebuchetKey, infernal key
            255880, // DemonKey_Destruction
            255881, // DemonKey_Hate
            255882, // DemonKey_Terror
        };

        /// <summary>
        /// Interactable whitelist - things that need interacting with like special wheels, levers - they will be blacklisted for 30 seconds after one-use
        /// </summary>
        public static HashSet<int> InteractWhiteListIds { get { return interactWhiteListIds; } }
        private static readonly HashSet<int> interactWhiteListIds = new HashSet<int> {
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
        private static readonly Dictionary<int, float> customObjectRadius = new Dictionary<int, float> {
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
            {340480, 30f}, // x1_Catacombs_Door_A_FX_Mouse
            {374731, 30f}, // x1_Catacombs_Door_Server_Prop
            {262286, 30f}, // x1_Catacombs_Door_A_FX_Rays_Slowing
            {345761, 30f}, // x1_Catacombs_Door_A_Ground _Center}
        };

        /// <summary>
        /// Navigation obstacles for standard navigation down dungeons etc. to help DB movement
        /// MAKE SURE you add the *SAME* SNO to the "size" dictionary below, and include a reasonable size (keep it smaller rather than larger) for the SNO.
        /// </summary>
        public static HashSet<int> NavigationObstacleIds { get { return navigationObstacleIds; } }
        private static readonly HashSet<int> navigationObstacleIds = new HashSet<int> {
            194682, 81699, 3340, 123325, 
            (int)SNOActor.a3_Battlefield_demonic_forge,
            (int)SNOActor.a3_crater_st_demonic_forge,
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
            /*226808, // monsterAffix_waller_model (226808)
            340480, // x1_Catacombs_Door_A_FX_Mouse
            374731, // x1_Catacombs_Door_Server_Prop
            262286, // x1_Catacombs_Door_A_FX_Rays_Slowing
            345761, // x1_Catacombs_Door_A_Ground _Center*/
        };

        /// <summary>
        /// Size of the navigation obstacles above (actual SNO list must be matching the above list!)
        /// </summary>
        public static Dictionary<int, float> ObstacleCustomRadius { get { return obstacleCustomRadius; } }
        private static readonly Dictionary<int, float> obstacleCustomRadius = new Dictionary<int, float> {
            {174900, 25}, {194682, 20}, {81699, 20}, {3340, 12}, {123325, 25}, {185391, 25},
            {104596, 15}, // trOut_FesteringWoods_Neph_Column_B
            {104632, 15}, // trOut_FesteringWoods_Neph_Column_B_Broken_Base
            {105303, 15}, // trOut_FesteringWoods_Neph_Column_C_Broken_Base_Bottom
            {104827, 15}, // trOut_FesteringWoods_Neph_Column_C_Broken_Base 
            {355898, 12}, // x1_Bog_Family_Guard_Tower_Stump
            {376917, 10}, 
         };

        public static HashSet<int> ForceAtDoorType { get { return forceAtDoorType; } }
        private static HashSet<int> forceAtDoorType = new HashSet<int>()
        {
            (int)SNOActor.x1_Catacombs_Door_A,
            (int)SNOActor.x1_Catacombs_Door_A_FX,
            (int)SNOActor.x1_Catacombs_Door_A_FX_Mouse,
            (int)SNOActor.x1_Catacombs_Door_A_FX_Rays,
            (int)SNOActor.x1_Catacombs_Door_A_FX_Rays_Ground,
            (int)SNOActor.x1_Catacombs_Door_A_FX_Rays_Slowing_Client,
            (int)SNOActor.x1_Catacombs_Door_A_Ground_Center_FX,
            (int)SNOActor.x1_Catacombs_Door_Adria_Locked_FX,
            (int)SNOActor.x1_Catacombs_Door_B,
            (int)SNOActor.x1_Catacombs_Door_B_Locked_FX,
            (int)SNOActor.x1_Catacombs_Door_Server_Prop,
            (int)SNOActor.x1_Catacombs_Door_Server_Prop_B,
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
        private static readonly Dictionary<int, float> destructableObjectRadius = new Dictionary<int, float> {
            {2972, 10}, {80357, 16}, {116508, 10}, {113932, 8}, {197514, 18}, {108587, 8}, {108618, 8}, {108612, 8}, {116409, 18}, {121586, 22},
            {195101, 10}, {195108, 25}, {170657, 5}, {181228, 10}, {211959, 25}, {210418, 25}, {174496, 4}, {193963, 5}, {159066, 12}, {160570, 12},
            {55325, 5}, {5718, 14}, {5909, 10}, {5792, 8}, {108194, 8}, {129031, 30}, {192867, 3.5f}, {155255, 8}, {54530, 6}, {157541, 6},
            {93306, 10},
         };

        /// <summary>
        /// Destructible things that need targeting by a location instead of an ACDGUID (stuff you can't "click on" to destroy in-game)
        /// </summary>
        public static HashSet<int> DestroyAtLocationIds { get { return destroyAtLocationIds; } }
        private static readonly HashSet<int> destroyAtLocationIds = new HashSet<int> {
            170657, 116409, 121586, 155255, 104596, 93306,
         };

        /// <summary>
        /// Resplendent chest SNO list
        /// </summary>
        public static HashSet<int> ResplendentChestIds { get { return resplendentChestIds; } }
        private static readonly HashSet<int> resplendentChestIds = new HashSet<int> {
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
            220160, // Small Belial (220160 }, 
            3349,   // Big Belial (3349 },    
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
        private static readonly HashSet<int> containerWhiteListIds = new HashSet<int> {
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
        private static HashSet<int> blacklistIds = new HashSet<int> {
            
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

        /// <summary>
        /// A list of LevelAreaId's that the bot should always ignore Line of Sight
        /// </summary>
        public static HashSet<int> NeverRaycastWorlds { get { return neverRaycastWorlds; } }
        private static readonly HashSet<int> neverRaycastWorlds = new HashSet<int>()
        {
            288685,
            288454,
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
            //5432, // A2 Snakem
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

        public static readonly Dictionary<int, string> LegendaryGems = new Dictionary<int, string>
        {
            {405775,"Bane of the Powerful"},
            {405781,"Bane of the Trapped"},
            {405792,"Wreath of Lightning"},
            {405793,"Gem of Efficacious Toxin"},
            {405794,"Pain Enhancer"},
            {405795,"Mirinae, Teardrop of the Starweaver"},
            {405796,"Gogok of Swiftness"},
            {405797,"Invigorating Gemstone"},
            {405798,"Enforcer"},
            {405800,"Moratorium"},
            {405801,"Zei's Stone of Vengeance"},
            {405802,"Simplicity's Strength"},
            {405803,"Boon of the Hoarder"},
            {405804,"Taeguk"},
            {428033,"Esoteric Alteration"},
            {405783,"Gem of Ease"},
            {428034,"Molten Wildebeest’s Gizzard"},
        };

        #region Methods

        public static bool ContainsKeyValue(Dictionary<int, HashSet<int>> dict, int expectedKey, int expectedValue)
        {
            HashSet<int> actualValue;
            return dict.TryGetValue(expectedKey, out actualValue) &&
                   actualValue.Any(value => value == expectedValue);
        }

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
