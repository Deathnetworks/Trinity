using Zeta.Common.Plugins;
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
