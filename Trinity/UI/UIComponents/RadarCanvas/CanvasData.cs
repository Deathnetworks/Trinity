using System.Windows;
using System.Windows.Media;
using Zeta.Common;

namespace Trinity.UIComponents
{
    /// <summary>
    /// Houses canvas information, so a bunch of structs can be accessed by reference.
    /// </summary>
    public class CanvasData
    {
        /// <summary>
        /// Center of the canvas (in pixels)
        /// </summary>
        public Point Center;

        /// <summary>
        /// Size of the canvas (in pixels)
        /// </summary>
        public Size CanvasSize;

        /// <summary>
        /// Size of the canvas (in grid squares)
        /// </summary>
        public Size Grid;

        /// <summary>
        /// A transform for 45 degrees clockwise around canvas center
        /// </summary>
        public RotateTransform RotationTransform;

        /// <summary>
        /// A transform for flipping point vertically on canvas center
        /// </summary>
        public ScaleTransform FlipTransform;

        /// <summary>
        /// A transform for both FlipTransform and RotationTransform
        /// </summary>
        public TransformGroup Transforms;

        /// <summary>
        /// A transform for -45 degrees clockwise around canvas center
        /// </summary>
        public RotateTransform RotationTransformReverse;

        /// <summary>
        /// A reverse transform for both FlipTransform and RotationTransform
        /// </summary>
        public TransformGroup TransformsReverse;

        /// <summary>
        /// Size of a single grid square
        /// </summary>
        public Size GridSquareSize;

        /// <summary>
        /// The world space vector3 for the center for the canvas
        /// </summary>
        public Vector3 CenterVector { get; set; }

        /// <summary>
        /// Updates the canvas information (for if canvas size changes)
        /// </summary>
        public void Update(Size canvasSize, int gridSize)
        {
            var previousSize = CanvasSize;

            Center = new Point(canvasSize.Width / 2, canvasSize.Height / 2);

            RotationTransform = new RotateTransform(45, Center.X, Center.Y);
            FlipTransform = new ScaleTransform(1, -1, Center.X, Center.Y);
            Transforms = new TransformGroup();
            Transforms.Children.Add(RotationTransform);
            Transforms.Children.Add(FlipTransform);

            RotationTransformReverse = new RotateTransform(-45, Center.X, Center.Y);
            TransformsReverse = new TransformGroup();
            TransformsReverse.Children.Add(FlipTransform);
            TransformsReverse.Children.Add(RotationTransform);

            CanvasSize = canvasSize;
            Grid = new Size((int)(canvasSize.Width / gridSize), (int)(canvasSize.Height / gridSize));
            GridSquareSize = new Size(gridSize, gridSize);
            LastCanvas = this;

            ClipRegion = new RectangleGeometry(new Rect(new Point(20, 20), new Size(canvasSize.Width - 40, canvasSize.Height - 40)));

            if (OnCanvasSizeChanged != null && !previousSize.Equals(CanvasSize))
                OnCanvasSizeChanged(previousSize, CanvasSize);
        }

        public static CanvasData LastCanvas { get; set; }

        public delegate void CanvasSizeChanged(Size sizeBefore, Size sizeAfter);

        public event CanvasSizeChanged OnCanvasSizeChanged;

        public RectangleGeometry ClipRegion { get; set; }
    }
}
