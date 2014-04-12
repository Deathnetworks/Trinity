using System.Xml.Serialization;

namespace Trinity
{
    /// <summary>
    /// Item Stats Class and Variables - for the detailed item drop/pickup etc. stats
    /// </summary>
    public class ItemDropStats
    {
        public double Total { get; set; }
        public double[] TotalPerQuality { get; set; }
        public double[] TotalPerLevel { get; set; }
        [XmlIgnore] public double[,] TotalPerQPerL { get; set; }
        public double TotalPotions { get; set; }
        public double[] PotionsPerLevel { get; set; }
        public double TotalGems { get; set; }
        public double[] GemsPerType { get; set; }
        public double[] GemsPerLevel { get; set; }
        [XmlIgnore] public double[,] GemsPerTPerL { get; set; }
        public int TotalInfernalKeys { get; set; }

        // For serialization
        public class Serializable2DimArray
        {
            public int rows;
            public int cols;
            public double[] data;

            public Serializable2DimArray() 
            { 
                rows = 0;
                cols = 0;
                data = null; 
            }

            public Serializable2DimArray(double[,] a)
            {
                rows = a.GetLength(0);
                cols = a.GetLength(1);
                data = new double[a.Length];

                int idx = 0;
                for (int r = 0; r < rows; r++)
                    for (int c = 0; c < cols; c++, idx++)
                        data[idx] = a[r, c];
            }

            public double[,] To2DArray()
            {
                double[,] res = new double[rows, cols];
                int idx = 0;
                for (int r = 0; r < rows; r++)
                    for (int c = 0; c < cols; c++, idx++)
                        res[r, c] = data[idx];
                return res;
            }
        }

        public Serializable2DimArray SerializableTotalPerQPerL
        {
            get { return new Serializable2DimArray(TotalPerQPerL); }
            set { TotalPerQPerL = value.To2DArray();  }
        }

        public Serializable2DimArray SerializableGemsPerTPerL
        {
            get { return new Serializable2DimArray(GemsPerTPerL); }
            set { GemsPerTPerL = value.To2DArray(); }
        }



        public ItemDropStats()
            : this(0, new double[4], new double[74], new double[4, 74], 0, new double[74], 0, new double[5], new double[74], new double[5, 74], 0)
        {
            // Creates the default values used by most
        }

        public ItemDropStats(
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
            int totalKeys)
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
