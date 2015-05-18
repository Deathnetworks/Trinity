using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trinity.Combat.Weighting
{
    /// <summary>
    /// A single weight calculation
    /// </summary>
    public struct Weight
    {

        public Weight(double amount, WeightMethod method, WeightReason reason)
        {
            Amount = amount;
            Reason = reason;
            Method = method;
            ExtraInfo = null;
        }

        public Weight(double amount, WeightMethod method, WeightReason reason, params object[] extraInfo)
        {
            Amount = amount;
            Reason = reason;
            Method = method;
            ExtraInfo = extraInfo;
        }

        public WeightReason Reason;
        public WeightMethod Method;
        public object[] ExtraInfo;
        public double Amount;

        public override string ToString()
        {
            var op = string.Empty;

            if (Method == WeightMethod.Add && Amount > 0)
                op = "+";
            else if (Method == WeightMethod.Multiply)
                op = "*";
            else if (Method == WeightMethod.Set)
                op = string.Empty;
            else if (Method == WeightMethod.Subtract && Amount > 0)
                op = "+";

            var extraInfo = ExtraInfo != null && ExtraInfo.Length > 0 ? "(" + string.Join(",", ExtraInfo) + ")" : string.Empty;

            return string.Format("{0}{1}: {2}{3:0.##}", Reason, extraInfo, op, Amount);
        }
    }

    /// <summary>
    /// Way of combining a weight with others
    /// </summary>
    public enum WeightMethod
    {
        None = 0,
        Multiply,
        Set,
        Add,
        Subtract
    }

    /// <summary>
    /// Why the weight is being adjusted 
    /// </summary>
    public enum WeightReason
    {
        None = 0,
        Start,
        InAOE,

        /// <summary>
        /// Set, 1, If there are monsters close to player
        /// </summary>
        DangerClose,
        IgnoreElites,
        HighPriorityContainer,
        HighPriorityShrine,
        HealthEmergency,

        /// <summary>
        /// Set, 0, If blocking movement
        /// </summary>
        NavBlocking,
        DisabledInSettings,
        GruesomeFeast,
        BloodVengeance,
        HighPrioritySetting,
        LowHealthPartyMember,

        /// <summary>
        /// Add, 1000, If very close to player (12f)
        /// </summary>
        CloseProximity,
        MediumProximity,
        ReapersWraps,

        /// <summary>
        /// Add, 400, If this was the target last tick
        /// </summary>
        PreviousTarget,
        InAvoidance,

        /// <summary>
        /// Set, 1, If monsters are between us and the object
        /// </summary>
        MonsterInLoS,
        CloseToMonster,
        CloseToAvoidance,
        MonstersNearPlayer,
        HasTarget,
        AlreadyCloseEnough,

        /// <summary>
        /// Set, 0, If we're supposed to be prioritizing quest things
        /// </summary>
        DisableForQuest,

        /// <summary>
        /// Set, 0, If boss or elite nearby
        /// </summary>
        BossOrEliteNearby,

        TypeMismatch,

        /// <summary>
        /// Set, 1, If combatlooting is disabled and we're in combat
        /// </summary>
        NoCombatLooting,

        /// <summary>
        /// Set, 1, If there is avoidance between us and the object
        /// </summary>
        AvoidanceInLoS,
        Event,

        /// <summary>
        /// Add, 250, If units are behind
        /// </summary>
        UnitsBehind,
        SettingForceDestructibles,

        /// <summary>
        /// Add, 5000, If very close to player (5f)
        /// </summary>
        TouchProximity,

        /// <summary>
        /// Set, 0, If supposed to be prioritizing close range monsters
        /// </summary>
        CloseRangePriority,
        PrioritizeForQuest,
        DestructableChest,
        HighPriorityInteractable,
        QuestObjective,
        HarringtonBuff,
        BossNearby,
        TownRun,
        IsLegendary,
        IgnoreNearElites,
        MonkTR,
        IgnoreOrphanTrash,
        IgnoreHealthDot,
        NotInKillRange,
        TooFarFromEvent,
        DeadUnit,
        GoblinKamikaze,
        NotAttackable,
        IsBossEliteRareUnique,
        BountyObjective,
        InActiveEvent,
        ArchonElite,
        ElitesNearPlayers,
        InHotSpot,
        RangedUnit,
        LowHPTrash,
        LowHPElite,
        XtraPriority,
        Summoner,
        CorruptGrowth,
        MinWeight,
        InSpecialAoE,
        InAoE,
        AoEPathLine,
        LowHPGoblin,
        GoblinNormal,
        GoblinPriority,
        AntiFlipFlop,
        IsNPC,
        IsBoss,
        Distance,
        IgnoreBosses,
        IgnoreTrash,
        Cluster,
        GoblinIgnore,
        IsElite,
        IsSummoned,
        IgnoreHealth,
        NotHostile,
        NullObject,
        OwnedByPlayer,
        MinGoldStack
    }
}
