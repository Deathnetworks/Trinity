using Zeta.Internals.Actors;

namespace GilesTrinity
{
    // Super Special Giles Sauce Data Caching
    internal class GilesGameBalanceDataCache
    {
        public int ItemLevel { get; set; }
        public Zeta.Internals.Actors.ItemType ItemType { get; set; }
        public bool OneHand { get; set; }
        public FollowerType FollowerType { get; set; }

        public GilesGameBalanceDataCache(int itemLevel, Zeta.Internals.Actors.ItemType itemType, bool oneHand, FollowerType followerType)
        {
            ItemLevel = itemLevel;
            ItemType = itemType;
            OneHand = oneHand;
            FollowerType = followerType;
        }
    }
}
