using Zeta.Internals.Actors;

namespace GilesTrinity
{
    // Super Special Giles Sauce Data Caching
    internal class GilesGameBalanceDataCache
    {
        public int ItemLevel { get; set; }
        public ItemBaseType ItemBaseType { get; set; }
        public ItemType ItemType { get; set; }
        public bool OneHand { get; set; }
        public bool TwoHand { get; set; }
        public FollowerType FollowerType { get; set; }

        public GilesGameBalanceDataCache(int itemLevel, ItemBaseType itemBaseType, ItemType itemType, bool oneHand, bool twoHand, FollowerType followerType)
        {
            ItemLevel = itemLevel;
            ItemBaseType = itemBaseType;
            ItemType = itemType;
            OneHand = oneHand;
            TwoHand = twoHand;
            FollowerType = followerType;
        }
    }
}
