using Zeta.Common;
using Zeta.Internals.Actors;
using Zeta.Internals.SNO;

namespace GilesTrinity
{
    // GilesObject type used to cache all data
    // Let's me create an object list with ONLY the data I need read from D3 memory, and then read from this while
    // Handling movement and interaction with the target - whether the target is a shrine, an item or a monster
    // Completely minimizing the D3 memory reads to the bare minimum
    internal class GilesObject
    {
        // Generic stuff applicable to all objects
        public GObjectType Type { get; set; }
        public double Weight { get; set; }
        public Vector3 Position { get; set; }
        public float CentreDistance { get; set; }
        public float RadiusDistance { get; set; }
        public string InternalName { get; set; }
        public int ACDGuid { get; set; }
        public int RActorGuid { get; set; }
        public int DynamicID { get; set; }
        public int BalanceID { get; set; }
        public int ActorSNO { get; set; }
        // Item/gold/other stuff
        public int Level { get; set; }
        public int GoldAmount { get; set; }
        public bool OneHanded { get; set; }
        public bool TwoHanded { get; set; }
        public ItemQuality ItemQuality { get; set; }
        public ItemBaseType DBItemBaseType { get; set; }
        public ItemType DBItemType { get; set; }
        public FollowerType FollowerType { get; set; }
        public GItemType GilesItemType { get; set; }
        // Monster/unit stuff
        public bool IsElite { get; set; }
        public bool IsRare { get; set; }
        public bool IsUnique { get; set; }
        public bool IsMinion { get; set; }
        public bool IsTreasureGoblin { get; set; }
        public bool IsEliteRareUnique { get; set; }
        public bool IsBoss { get; set; }
        public bool IsBossOrEliteRareUnique { get { return (IsEliteRareUnique || IsBoss); } }
        public bool IsAttackable { get; set; }
        /// <summary>
        /// Percentage hit points
        /// </summary>
        public double HitPoints { get; set; }
        public float Radius { get; set; }
        public bool ForceLeapAgainst { get; set; }
        public MonsterSize MonsterStyle { get; set; }
        // A reference to the original object for fast updates
        public DiaUnit Unit
        {
            get
            {
                if (DiaObject is DiaUnit)
                    return DiaObject as DiaUnit;
                else
                    return null;
            }
        }
        public DiaObject DiaObject { get; set; }

        public GilesObject(DiaObject _DiaObject = null)
        {
            DiaObject = _DiaObject;
        }

        // For cloning the object (and not simply referencing it)
        public GilesObject Clone()
        {
            GilesObject newGilesObject = new GilesObject(Unit)
            {
                Position = Position,
                Type = Type,
                Weight = Weight,
                CentreDistance = CentreDistance,
                RadiusDistance = RadiusDistance,
                InternalName = InternalName,
                ACDGuid = ACDGuid,
                RActorGuid = RActorGuid,
                DynamicID = DynamicID,
                BalanceID = BalanceID,
                ActorSNO = ActorSNO,
                Level = Level,
                GoldAmount = GoldAmount,
                OneHanded = OneHanded,
                TwoHanded = TwoHanded,
                ItemQuality = ItemQuality,
                DBItemBaseType = DBItemBaseType,
                DBItemType = DBItemType,
                FollowerType = FollowerType,
                GilesItemType = GilesItemType,
                IsElite = IsElite,
                IsRare = IsRare,
                IsUnique = IsUnique,
                IsMinion = IsMinion,
                IsTreasureGoblin = IsTreasureGoblin,
                IsEliteRareUnique = IsEliteRareUnique,
                IsBoss = IsBoss,
                IsAttackable = IsAttackable,
                HitPoints = HitPoints,
                Radius = Radius,
                MonsterStyle = MonsterStyle,
                ForceLeapAgainst = ForceLeapAgainst
            };
            return newGilesObject;
        }
    }
}
