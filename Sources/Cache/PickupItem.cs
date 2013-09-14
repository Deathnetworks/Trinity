using Zeta.Common;
using Zeta.Internals.Actors;

namespace Trinity.Cache
{
    internal class PickupItem
    {
        public string Name { get; set; }
        public string InternalName { get; set; }
        public int Level { get; set; }
        public ItemQuality Quality { get; set; }
        public int BalanceID { get; set; }
        public ItemBaseType DBBaseType { get; set; }
        public ItemType DBItemType { get; set; }
        public bool IsOneHand { get; set; }
        public bool IsTwoHand { get; set; }
        public FollowerType ItemFollowerType { get; set; }
        public int DynamicID { get; set; }
        public Vector3 Position { get; set; }
        public int ActorSNO { get; set; }

        public PickupItem() { }

        public PickupItem(string Name, string internalName, int level, ItemQuality quality, int balanceId, ItemBaseType dbItemBaseType, ItemType dbItemType, bool isOneHand, bool isTwoHand, FollowerType followerType, int dynamicID = 0)
        {
            this.Name = Name;
            this.InternalName = internalName;
            this.Level = level;
            this.Quality = quality;
            this.BalanceID = balanceId;
            this.DBBaseType = dbItemBaseType;
            this.DBItemType = dbItemType;
            this.IsOneHand = IsOneHand;
            this.IsTwoHand = IsTwoHand;
            this.ItemFollowerType = followerType;
            this.DynamicID = dynamicID;
        }

    }
}
