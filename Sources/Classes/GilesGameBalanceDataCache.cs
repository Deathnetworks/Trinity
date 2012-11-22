using Zeta.Common.Plugins;
using Zeta.Internals.Actors;
namespace GilesTrinity
{
    public partial class GilesTrinity : IPlugin
    {
// Super Special Giles Sauce Data Caching
        public class GilesGameBalanceDataCache
        {
            public int iThisItemLevel { get; set; }
            public ItemType thisItemType { get; set; }
            public bool bThisOneHand { get; set; }
            public FollowerType thisFollowerType { get; set; }
            public GilesGameBalanceDataCache(int itemlevel, ItemType itemtype, bool onehand, FollowerType followertype)
            {
                iThisItemLevel = itemlevel;
                thisItemType = itemtype;
                bThisOneHand = onehand;
                thisFollowerType = followertype;
            }
        }
    }
}
