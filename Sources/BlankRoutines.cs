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
// Give DB a Blank Combat Target Provider
    public class GilesCombatTargetingReplacer : ITargetingProvider
    {
        private static readonly List<DiaObject> listEmptyList = new List<DiaObject>();
        public List<DiaObject> GetObjectsByWeight()
        {
            if (!GilesTrinity.bDontMoveMeIAmDoingShit || GilesTrinity.thisFakeObject == null)
                return listEmptyList;
            List<DiaObject> listFakeList = new List<DiaObject>();
            listFakeList.Add(GilesTrinity.thisFakeObject);
            return listFakeList;
        }
    }
    public class GilesLootTargetingProvider : ITargetingProvider
    {
        private static readonly List<DiaObject> listEmptyList = new List<DiaObject>();
        public List<DiaObject> GetObjectsByWeight()
        {
            return listEmptyList;
        }
    }
    public class GilesObstacleTargetingProvider : ITargetingProvider
    {
        private static readonly List<DiaObject> listEmptyList = new List<DiaObject>();
        public List<DiaObject> GetObjectsByWeight()
        {
            return listEmptyList;
        }
    }
}
