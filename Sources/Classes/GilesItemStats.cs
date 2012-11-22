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
// Item Stats Class and Variables - for the detailed item drop/pickup etc. stats
        private class GilesItemStats
        {
            public double iTotal { get; set; }
            public double[] iTotalPerQuality { get; set; }
            public double[] iTotalPerLevel { get; set; }
            public double[,] iTotalPerQPerL { get; set; }
            public double iTotalPotions { get; set; }
            public double[] iPotionsPerLevel { get; set; }
            public double iTotalGems { get; set; }
            public double[] iGemsPerType { get; set; }
            public double[] iGemsPerLevel { get; set; }
            public double[,] iGemsPerTPerL { get; set; }
            public double iTotalInfernalKeys { get; set; }
            public GilesItemStats(double total, double[] totalperq, double[] totalperl, double[,] totalperqperl, double totalpotions, double[] potionsperlevel, double totalgems,
                double[] gemspertype, double[] gemsperlevel, double[,] gemspertperl, double totalkeys)
            {
                iTotal = total;
                iTotalPerQuality = totalperq;
                iTotalPerLevel = totalperl;
                iTotalPerQPerL = totalperqperl;
                iTotalPotions = totalpotions;
                iPotionsPerLevel = potionsperlevel;
                iTotalGems = totalgems;
                iGemsPerType = gemspertype;
                iGemsPerLevel = gemsperlevel;
                iGemsPerTPerL = gemspertperl;
                iTotalInfernalKeys = totalkeys;
            }
        }
    }
}
