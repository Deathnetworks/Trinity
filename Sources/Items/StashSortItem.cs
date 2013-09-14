
namespace Trinity
{
    /// <summary>
    /// Arrange your stash by highest to lowest scoring items
    /// </summary>
    internal class StashSortItem
    {
        public double Score { get; set; }
        public int StashOrPack { get; set; }
        public int InventoryColumn { get; set; }
        public int InventoryRow { get; set; }
        public int DynamicID { get; set; }
        public bool IsTwoSlot { get; set; }
        public StashSortItem(double stashScore, int stashOrPack, int column, int row, int dynamicId, bool isTwoSlot)
        {
            Score = stashScore;
            StashOrPack = stashOrPack;
            InventoryColumn = column;
            InventoryRow = row;
            DynamicID = dynamicId;
            IsTwoSlot = isTwoSlot;
        }
    }
}
