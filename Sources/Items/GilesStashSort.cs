using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Trinity
{
    /// <summary>
    /// Arrange your stash by highest to lowest scoring items
    /// </summary>
    internal class GilesStashSort
    {
        public double dStashScore { get; set; }
        public int iStashOrPack { get; set; }
        public int iInventoryColumn { get; set; }
        public int InventoryRow { get; set; }
        public int iDynamicID { get; set; }
        public bool bIsTwoSlot { get; set; }
        public GilesStashSort(double stashscore, int stashorpack, int icolumn, int irow, int dynamicid, bool twoslot)
        {
            dStashScore = stashscore;
            iStashOrPack = stashorpack;
            iInventoryColumn = icolumn;
            InventoryRow = irow;
            iDynamicID = dynamicid;
            bIsTwoSlot = twoslot;
        }
    }
}
