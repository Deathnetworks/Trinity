using System.ComponentModel;
using Zeta.Common;
using Zeta.Game.Internals.Actors;

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
        public GItemBaseType TBaseType { get; set; }
        public GItemType TType { get; set; }
        public bool IsOneHand { get; set; }
        public bool IsTwoHand { get; set; }
        public FollowerType ItemFollowerType { get; set; }
        public int DynamicID { get; set; }
        public Vector3 Position { get; set; }
        public int ActorSNO { get; set; }
        public int ACDGuid { get; set; }
        public int RActorGUID { get; set; }
        public ACDItem ACDItem { get { return Zeta.Game.ZetaDia.Actors.GetACDItemByGuid(ACDGuid); } }
        public bool IsUpgrade { get; set; }
        public float UpgradeDamage { get; set; }
        public float UpgradeToughness { get; set; }
        public float UpgradeHealing { get; set; }

        public PickupItem() { }

        public PickupItem(ACDItem item, GItemBaseType gItemBaseType, GItemType gItemType)
        {
            Name = item.Name;
            InternalName = item.InternalName;
            Level = item.Level;
            Quality = item.ItemQualityLevel;
            BalanceID = item.GameBalanceId;
            DBBaseType = item.ItemBaseType;
            DBItemType = item.ItemType;
            TBaseType = gItemBaseType;
            TType = gItemType;
            IsOneHand = item.IsOneHand;
            IsTwoHand = item.IsTwoHand;
            ItemFollowerType = item.FollowerSpecialType;
            DynamicID = item.DynamicId;
            ActorSNO = item.ActorSNO;
            ACDGuid = item.ACDGuid;
        }

        public PickupItem(string name, string internalName, int level, ItemQuality quality, int balanceId, ItemBaseType dbItemBaseType, 
            ItemType dbItemType, bool isOneHand, bool isTwoHand, FollowerType followerType, int acdGuid, int dynamicID = 0)
        {
            Name = name;
            InternalName = internalName;
            Level = level;
            Quality = quality;
            BalanceID = balanceId;
            DBBaseType = dbItemBaseType;
            DBItemType = dbItemType;
            IsOneHand = isOneHand;
            IsTwoHand = isTwoHand;
            ItemFollowerType = followerType;
            ACDGuid = acdGuid;
            DynamicID = dynamicID;
        }



    }
}
