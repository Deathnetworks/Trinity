using System.Collections.Concurrent;

namespace Trinity.UIComponents
{
    internal static class Drawings
    {
        public static ConcurrentDictionary<string, RelativeDrawing> Relative = new ConcurrentDictionary<string, RelativeDrawing>();
        public static ConcurrentDictionary<string, StaticDrawing> Static = new ConcurrentDictionary<string, StaticDrawing>();
    }
}
