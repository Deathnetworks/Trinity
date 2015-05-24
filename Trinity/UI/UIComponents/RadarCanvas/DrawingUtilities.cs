using System;
using System.Drawing;
using System.Windows;
using System.Windows.Media;
using Zeta.Common;
using Zeta.Game.Internals.SNO;
using Color = System.Drawing.Color;
using Point = System.Windows.Point;
using Size = System.Windows.Size;

namespace Trinity.UIComponents
{
    /// <summary>
    /// Useful tools for drawing stuff
    /// </summary>
    public static class DrawingUtilities
    {
        /// <summary>
        /// Convert to a Drawing System.Drawing.Color
        /// </summary>
        public static Color ToDrawingColor(this System.Windows.Media.Color color)
        {
            return Color.FromArgb(color.A, color.R, color.G, color.B);
        }

        /// <summary>
        /// Convert to a System.Windows.Media.Color
        /// </summary>
        public static System.Windows.Media.Color ToMediaColor(this Color color)
        {
            return System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B);
        }

        public static Point ToCanvasPoint(this Vector3 positionVector, CanvasData canvasData = null)
        {
            return new PointMorph(positionVector, canvasData ?? CanvasData.LastCanvas).Point;
        }

        public static void RelativeMove(DrawingGroup group, Vector3 origin, CanvasData canvasData = null)
        {
            var originPoint = new PointMorph(origin, CanvasData.LastCanvas);
            var transform = new TranslateTransform(originPoint.Point.X - CanvasData.LastCanvas.Center.X, originPoint.Point.Y - CanvasData.LastCanvas.Center.Y);
            group.Transform = transform;
        }

        public static System.Drawing.Point ToDrawingPoint(this Point point)
        {
            return new System.Drawing.Point((int)point.X, (int)point.Y);
        }

        public static Point ToPoint(this Vector3 worldVector)
        {
            return new Point(worldVector.X, worldVector.Y);
        }

        public static Rect ToCanvasRect(this AABB bounds)
        {
            var max = bounds.Max;
            var min = bounds.Min;
            var center = new Vector3((int)(max.X - (max.X - min.X) / 2), (int)(max.Y - (max.Y - min.Y) / 2), 0);
            var width = Math.Abs(max.X - min.X);
            var height = Math.Abs(max.Y - min.Y);
            return new Rect(center.ToCanvasPoint(), new Size((int)width, (int)height));
        }

        public static Point Center(this Rectangle rect)
        {
            return new Point(rect.Left + rect.Width / 2,
                             rect.Top + rect.Height / 2);
        }

    }
}
