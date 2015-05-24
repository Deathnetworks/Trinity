using System.Windows.Media;

namespace Trinity.UIComponents
{
    /// <summary>
    /// Object for saving and reusing static drawings.
    /// Static drawings like you might assume, do not move.
    /// Example: Grid Lines.
    /// </summary>
    internal class StaticDrawing
    {
        public DrawingGroup Drawing { get; set; }
        public int WorldId { get; set; }
    }
}
