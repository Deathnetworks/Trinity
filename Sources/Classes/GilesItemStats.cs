using Zeta.Common.Plugins;

namespace GilesTrinity
{
    // Item Stats Class and Variables - for the detailed item drop/pickup etc. stats
    internal class GilesItemStats
    {
        public double Total { get; set; }
        public double[] TotalPerQuality { get; set; }
        public double[] TotalPerLevel { get; set; }
        public double[,] TotalPerQPerL { get; set; }
        public double TotalPotions { get; set; }
        public double[] PotionsPerLevel { get; set; }
        public double TotalGems { get; set; }
        public double[] GemsPerType { get; set; }
        public double[] GemsPerLevel { get; set; }
        public double[,] GemsPerTPerL { get; set; }
        public double TotalInfernalKeys { get; set; }
        public GilesItemStats(
            double total, 
            double[] totalPerQuality,
            double[] totalPerLevel,
            double[,] totalPerQPerL,
            double totalPotions,
            double[] potionsPerLevel, 
            double totalGems,
            double[] gemsPerType, 
            double[] gemsPerLevel, 
            double[,] gemsPerTPerL, 
            double totalKeys)
        {
            Total = total;
            TotalPerQuality = totalPerQuality;
            TotalPerLevel = totalPerLevel;
            TotalPerQPerL = totalPerQPerL;
            TotalPotions = totalPotions;
            PotionsPerLevel = potionsPerLevel;
            TotalGems = totalGems;
            GemsPerType = gemsPerType;
            GemsPerLevel = gemsPerLevel;
            GemsPerTPerL = gemsPerTPerL;
            TotalInfernalKeys = totalKeys;
        }
    }
}
