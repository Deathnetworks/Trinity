using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.Windows.Markup;
using Zeta;
using Zeta.Common;
using Zeta.Common.Plugins;
using Zeta.CommonBot;
using Zeta.CommonBot.Profile;
using Zeta.CommonBot.Profile.Composites;
using Zeta.Internals;
using Zeta.Internals.Actors;
using Zeta.Internals.Actors.Gizmos;
using Zeta.Internals.SNO;
using Zeta.Navigation;
using Zeta.TreeSharp;
using Zeta.XmlEngine;
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
