using System.Windows.Media;

namespace Trinity.UIComponents
{
    public static class RadarBrushes
    {
        static RadarBrushes()
        {
            WalkableTerrain = new SolidColorBrush(Colors.LightSlateGray)
            {
                Opacity = 0.2
            };

            YellowPen = new Pen(Brushes.Yellow, 0.1);
            BluePen = new Pen(Brushes.CornflowerBlue, 0.2);
            LightYellowPen = new Pen(Brushes.LightYellow, 0.1);
            TransparentBrush = new SolidColorBrush(Colors.Transparent);
            BorderPen = new Pen(Brushes.Black, 0.1);
            GridPen = new Pen(Brushes.Black, 0.1);

            WhiteDashPen = new Pen(new SolidColorBrush(Colors.WhiteSmoke), 1)
            {
                DashStyle = DashStyles.DashDotDot,
                DashCap = PenLineCap.Flat
            };
        }

        public static Brush WalkableTerrain { get; set; }

        public static Pen YellowPen { get; set; }

        public static Pen BluePen { get; set; }

        public static SolidColorBrush TransparentBrush { get; set; }

        public static Pen LightYellowPen { get; set; }

        public static Pen BorderPen { get; set; }

        public static Pen GridPen { get; set; }

        public static Pen WhiteDashPen { get; set; }
    }
}
