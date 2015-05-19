using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using System.Windows.Forms;
using Trinity.LazyCache;
using Zeta.Common;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;
using FlowDirection = System.Windows.FlowDirection;
using Pen = System.Windows.Media.Pen;
using Logger = Trinity.Technicals.Logger;
using Point = System.Windows.Point;
using Rectangle = System.Drawing.Rectangle;
using Size = System.Windows.Size;

namespace Trinity.UIComponents
{
    /// <summary>
    /// Canvas information to be passable by reference
    /// </summary>
    public class CanvasData
    {
        public Point Center;
        public Size Size;
        public RotateTransform RotationTransform;
        public ScaleTransform FlipTransform;
        public TransformGroup Transforms;
        public Size Grid = Size.Empty;

        public void Update(Size desiredSize)
        {
            Center = new Point(desiredSize.Width / 2, desiredSize.Height / 2);

            // Raw Vectors need to translated 45 degrees clockwise
            RotationTransform = new RotateTransform(45, Center.X, Center.Y);

            // Raw Vectors need to be flipped horizontally
            FlipTransform = new ScaleTransform(-1, 1, Center.X, Center.Y);
            
            Transforms = new TransformGroup();            
            Transforms.Children.Add(RotationTransform);
            Transforms.Children.Add(FlipTransform);

            Size = desiredSize;
        }
    }

    public class RadarCanvas : Canvas
    {
        /// <summary>
        /// The drawing space size for 1yd
        /// </summary>
        public const int GridSize = 10;

        public TrinityItemPoint CenterActor { get; set; }

        public double MarkerSize = 5;

        public List<TrinityItemPoint> Objects = new List<TrinityItemPoint>();

        public CanvasData CanvasData = new CanvasData();

        public RadarCanvas()
        {
        }

        #region ItemSource Property

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource",
                typeof(IList),
                typeof(RadarCanvas),
                new PropertyMetadata(null, OnItemsSourceChanged));
        
        public IList ItemsSource
        {
            set { SetValue(ItemsSourceProperty, value); }
            get { return (IList)GetValue(ItemsSourceProperty); }
        }

        static void OnItemsSourceChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            (obj as RadarCanvas).OnItemsSourceChanged(args);
        }

        #endregion

        void OnItemsSourceChanged(DependencyPropertyChangedEventArgs args)
        {
            UpdateInternalCollections();

            if (args.OldValue is INotifyCollectionChanged)
            {
                (args.OldValue as INotifyCollectionChanged).CollectionChanged -= OnItemsSourceCollectionChanged;
            }

            if (args.NewValue is INotifyCollectionChanged)
            {
                (args.NewValue as INotifyCollectionChanged).CollectionChanged += OnItemsSourceCollectionChanged;
            }
        }

        void OnItemsSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            UpdateInternalCollections();

            if (args.OldItems != null)
            {
                foreach (object item in args.OldItems)
                    if (item is INotifyPropertyChanged)
                        (item as INotifyPropertyChanged).PropertyChanged -= OnItemPropertyChanged;

            }
            if (args.NewItems != null)
            {
                foreach (object item in args.NewItems)
                    if (item is INotifyPropertyChanged)
                        (item as INotifyPropertyChanged).PropertyChanged += OnItemPropertyChanged;
            }
        }

        void UpdateInternalCollections()
        {
            Objects.Clear();

            CanvasData.Update(DesiredSize);

            foreach (var trinityObject in ItemsSource.OfType<TrinityObject>())
            {
                var itemPoint = new TrinityItemPoint(trinityObject, CanvasData);

                Objects.Add(itemPoint);

                if (trinityObject.IsMe)
                {
                    CenterActor = itemPoint;
                }                    
            }

            foreach (var actor in Objects)
            {
                actor.UpdateRawPointFromWorldSpace(CenterActor.Item.Position);
            }

            // Triggers Rendering
            InvalidateVisual();
        }

        void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {

        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            CanvasData.Update(DesiredSize);            
        }
        
        protected override Size MeasureOverride(Size availableSize)
        {
            return availableSize;
        }

        protected override void OnRender(DrawingContext dc)
        {
            DrawGrid(dc, out CanvasData.Grid);

            // display the grid size
            var title = string.Format("{0}x{1}", CanvasData.Grid.Height, CanvasData.Grid.Width);
            dc.DrawText(new FormattedText(title, CultureInfo.InvariantCulture, FlowDirection.LeftToRight, new Typeface("Arial"), 20, Brushes.White), new Point(0, 0));

            DrawRangeGuide(dc);

            foreach (var actor in Objects)
            {
                if(!actor.IsBeyondCanvas)
                    DrawActor(dc, actor);
            }
        }

        private void DrawRangeGuide(DrawingContext dc)
        {
            var pen = new Pen(Brushes.LightYellow, 0.1);
            var brush = new SolidColorBrush(Colors.Transparent);

            for (var i = 20; i < 100; i+=20)
            {
                var radius = GridSize*i;
                dc.DrawEllipse(brush, pen, CanvasData.Center, radius, radius);

                dc.DrawText(new FormattedText(i.ToString(), CultureInfo.InvariantCulture, FlowDirection.LeftToRight, new Typeface("Arial"), 12, Brushes.LightYellow),
                    new Point(CanvasData.Center.X - (radius + 20), CanvasData.Center.Y));

            }            
        }

        private void DrawActor(DrawingContext dc, TrinityItemPoint actor)
        {
            var border = new Pen(Brushes.Black, 0.1);

            Color baseColor;
            switch (actor.Item.TrinityType)
            {
                case TrinityObjectType.Avoidance:
                    baseColor = Colors.OrangeRed;
                    break;

                case TrinityObjectType.Container:
                case TrinityObjectType.CursedChest:
                case TrinityObjectType.CursedShrine:
                case TrinityObjectType.Shrine:
                case TrinityObjectType.HealthWell:
                case TrinityObjectType.Interactable:
                    baseColor = Colors.Yellow;
                    break;

                case TrinityObjectType.Gold:
                case TrinityObjectType.Item:
                    baseColor = Colors.DarkSeaGreen;
                    break;

                case TrinityObjectType.Player:
                    baseColor = Colors.White;
                    break;

                case TrinityObjectType.Unit:

                    var unit = actor.Item as TrinityUnit;
                    if (unit != null)
                    {
                        if (unit.IsBossOrEliteRareUnique)
                            baseColor = Colors.Blue;
                        else if(unit.IsHostile)
                            baseColor = Colors.DodgerBlue; 
                        else
                            baseColor = Colors.LightSkyBlue;

                        
                    }
                    else
                    {
                        baseColor = Colors.SlateGray;
                    }
                    break;

                default:
                    baseColor = Colors.Transparent;
                    break;
            }

            var text = String.Format("{0}", actor.Item.Name);

            var formattedText = new FormattedText(text, CultureInfo.InvariantCulture, FlowDirection.LeftToRight, new Typeface("Arial"), 11, new SolidColorBrush(baseColor))
            {
                MaxTextWidth = 80,
                Trimming = TextTrimming.WordEllipsis,
                TextAlignment = TextAlignment.Center
            };

            var textOffsetPosition = new Point(actor.Point.X - (formattedText.WidthIncludingTrailingWhitespace / 2), actor.Point.Y - 10 - formattedText.Height);
            dc.DrawText(formattedText, textOffsetPosition);

            // Draw a dot in the center of the actor;
            var innerFill = new SolidColorBrush(baseColor);
            dc.DrawEllipse(innerFill, border, actor.Point, MarkerSize / 2, MarkerSize / 2);

            if (baseColor == Colors.Transparent)
            {
                border = new Pen(new SolidColorBrush(baseColor), 0.1);

                // Create a custom dash pattern.
                border.DashStyle = DashStyles.Dash;
            }
                

            // Draw a circle representing the size of the actor
            var outerFill = new SolidColorBrush(baseColor);
            outerFill.Opacity = 0.25;
            dc.DrawEllipse(outerFill, border, actor.Point, actor.Item.Radius * GridSize, actor.Item.Radius * GridSize);
        }

        private void DrawGrid(DrawingContext dc, out Size outGridSize)
        {                
            Pen pen = new Pen(Brushes.Black, 0.1);

            // vertical lines
            int pos = 0;
            var height = 0;
            do
            {
                dc.DrawLine(pen, new Point(pos, 0), new Point(pos, (int) DesiredSize.Height));
                pos += GridSize;
                height++;
            } while (pos < DesiredSize.Width);

            // horizontal lines
            pos = 0;
            var width = 0;
            do
            {
                dc.DrawLine(pen, new Point(0, pos), new Point((int) DesiredSize.Width, pos));
                pos += GridSize;
                width++;
            } while (pos < DesiredSize.Height);

            outGridSize = new Size(width, height);
        }

        /// <summary>
        /// TrinityItemPoint wraps a TrinityObject and provides some drawing related extensions.
        /// </summary>
        public class TrinityItemPoint : INotifyPropertyChanged
        {
            private TrinityObject _item;
            private Point _point;

            public TrinityItemPoint(TrinityObject item, CanvasData canvasData)
            {
                Item = item;
                item.PropertyChanged += ItemOnPropertyChanged;
                CanvasData = canvasData;
            }

            /// <summary>
            /// Information about the canvas
            /// </summary>
            public CanvasData CanvasData { get; private set; }

            /// <summary>
            /// Point in GridSquare (Yards) Space before translations
            /// </summary>
            public Point RawGridPoint { get; private set; }

            /// <summary>
            /// Point before any translations
            /// </summary>
            public Point RawPoint { get; private set; }

            /// <summary>
            /// Point rotated around canvas center by global rotation amount.
            /// </summary>
            public Point RotatedPoint  { get; private set; }

            /// <summary>
            /// Point flipped on Y-Axis
            /// </summary>
            public Point FlippedPoint { get; private set; }

            /// <summary>
            /// Flipped and Rotated point
            /// </summary>
            public Point Point { get; private set; }

            /// <summary>
            /// Point coods based on Grid Scale
            /// </summary>
            public Point GridPoint { get; private set; }

            /// <summary>
            /// Point restricted to the boundary of the canvas
            /// </summary>
            public Point BoundPoint { get; private set; }

            /// <summary>
            /// If the point is located outside of the canvas bounds
            /// </summary>
            public bool IsBeyondCanvas { get; private set; }


            public float RawWorldDistanceX { get; private set; }
            public float RawWorldDistanceY { get; private set; }
            public float RawDrawDistanceX { get; private set; }
            public float RawDrawDistanceY { get; private set; }
            public double RawDrawPositionX { get; private set; }
            public double RawDrawPositionY { get; private set; }

            public TrinityObject Item
            {
                set
                {
                    if (!Equals(value, _item))
                    {
                        _item = value;
                        OnPropertyChanged(new PropertyChangedEventArgs("Item"));
                    }
                }
                get { return _item; }
            }

            public void UpdateRawPointFromWorldSpace(Vector3 position)
            {
                var pos = _item.Position;

                // Distance from Actor to Player
                RawWorldDistanceX = pos.X - position.X;
                RawWorldDistanceY = pos.Y - position.Y;
                
                // We want 1 yard of game distance to = Gridsize
                RawDrawDistanceX = RawWorldDistanceX * GridSize;
                RawDrawDistanceY = RawWorldDistanceY * GridSize;

                // Distance on canvas from center to actor
                RawDrawPositionX = (CanvasData.Center.X + RawDrawDistanceX);
                RawDrawPositionY = (CanvasData.Center.Y + RawDrawDistanceY);

                // Points in Canvas and Grid Scale
                RawPoint = new Point(RawDrawPositionX, RawDrawPositionY);                
                RawGridPoint = new Point(RawDrawPositionX / GridSize, RawDrawPositionX / GridSize);

                TranslatePoints();
            }

            /// <summary>
            /// Moves and Rotates raw point to final position.
            /// </summary>
            public void TranslatePoints()
            {
                Point = CanvasData.Transforms.Transform(RawPoint);                
                RotatedPoint = CanvasData.RotationTransform.Transform(RawPoint);
                FlippedPoint =  CanvasData.FlipTransform.Transform(RawPoint);
                BoundPoint = new Point(Clamp(Point.X, 0, CanvasData.Size.Width), Clamp(Point.Y, 0, CanvasData.Size.Height));
                GridPoint = new Point((int)(Point.X / GridSize), (int)(Point.Y / GridSize));
                IsBeyondCanvas = Point.X < 0 || Point.X > CanvasData.Size.Width || Point.Y < 0 || Point.Y > CanvasData.Size.Height;
            }

            #region PropertyChanged Handling

            public event PropertyChangedEventHandler PropertyChanged;

            /// <summary>
            /// When the internal Item has changed, bubble it.
            /// </summary>
            private void ItemOnPropertyChanged(object sender, PropertyChangedEventArgs args)
            {
                OnPropertyChanged(args);
            }

            protected virtual void OnPropertyChanged(PropertyChangedEventArgs args)
            {                
                if (PropertyChanged != null)
                    PropertyChanged(this, args);
            }

            #endregion

            public override int GetHashCode()
            {
                return Item.GetHashCode();
            }

        }

        public static T Clamp<T>(T val, T min, T max) where T : IComparable<T>
        {
            if (val.CompareTo(min) < 0) return min;
            if (val.CompareTo(max) > 0) return max;
            return val;
        }
    }

}
