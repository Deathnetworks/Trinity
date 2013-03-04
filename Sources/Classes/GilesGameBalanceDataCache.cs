using System;
using Zeta.Internals.Actors;

namespace GilesTrinity
{
    [Serializable]
    internal class GameBalanceDataCache
    {
        public string ItemDisplayName { get; set; }
        public int ItemLevel { get; set; }
        public ItemBaseType ItemBaseType { get; set; }
        public ItemType ItemType { get; set; }
        public bool OneHand { get; set; }
        public bool TwoHand { get; set; }
        public FollowerType FollowerType { get; set; }

        public GameBalanceDataCache()
        {

        }
    }
}
