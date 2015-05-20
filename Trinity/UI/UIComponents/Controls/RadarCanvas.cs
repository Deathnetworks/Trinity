using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using System.Windows.Forms;
using Trinity.LazyCache;
using Zeta.Bot.Dungeons;
using Zeta.Common;
using Zeta.Game;
using Zeta.Game.Internals;
using Zeta.Game.Internals.Actors;
using Zeta.Game.Internals.SNO;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;
using FlowDirection = System.Windows.FlowDirection;
using LineSegment = System.Windows.Media.LineSegment;
using Pen = System.Windows.Media.Pen;
using Logger = Trinity.Technicals.Logger;
using Point = System.Windows.Point;
using Rectangle = System.Drawing.Rectangle;
using Size = System.Windows.Size;

namespace Trinity.UIComponents
{

    public class RadarCanvas : Canvas
    {
        public RadarCanvas()
        {
        }

        /// <summary>
        /// The canvas size of grid squares (in pixels) for 1yd of game distance
        /// </summary>
        public const int GridSize = 5;

        /// <summary>
        /// The number of grid squares/yards to draw horizontal/vertical lines on
        /// </summary>
        public const int GridLineFrequency = 10;

        /// <summary>
        /// The size (in pixels) to draw actor markers
        /// </summary>
        public double MarkerSize = 5;

        /// <summary>
        /// The actor who should be at the center of the radar
        /// </summary>
        public TrinityItemPoint CenterActor { get; set; }

        /// <summary>
        /// Collection of game objects
        /// </summary>
        public List<TrinityItemPoint> Objects = new List<TrinityItemPoint>();

        /// <summary>
        /// Information about the WPF canvas we'll be drawing on
        /// </summary>
        public CanvasData CanvasData = new CanvasData();

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

        #region ItemSource Changed Event Handling

        void OnItemsSourceChanged(DependencyPropertyChangedEventArgs args)
        {
            UpdateData();

            if (args.OldValue is INotifyCollectionChanged)
                (args.OldValue as INotifyCollectionChanged).CollectionChanged -= OnItemsSourceCollectionChanged;

            if (args.NewValue is INotifyCollectionChanged)
                (args.NewValue as INotifyCollectionChanged).CollectionChanged += OnItemsSourceCollectionChanged;
        }

        /// <summary>
        /// When objects are added/removed from ItemSource collection
        /// </summary>
        void OnItemsSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            UpdateData();

            //if (args.OldItems != null)
            //{
            //    foreach (object item in args.OldItems)
            //        if (item is INotifyPropertyChanged)
            //            (item as INotifyPropertyChanged).PropertyChanged -= OnItemPropertyChanged;

            //}
            //if (args.NewItems != null)
            //{
            //    foreach (object item in args.NewItems)
            //        if (item is INotifyPropertyChanged)
            //            (item as INotifyPropertyChanged).PropertyChanged += OnItemPropertyChanged;
            //}
        }

        /// <summary>
        /// When objects within the ItemSource collection change
        /// </summary>
        void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            
        }

        #endregion

        #region Canvas Changed Event Handling

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            CanvasData.Update(DesiredSize, GridSize);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            // Not entirely sure what this does
            return availableSize;
        }

        #endregion

        /// <summary>
        /// Go through the ItemSource collection and calculate their canvas positions
        /// </summary>
        void UpdateData()
        {
            try
            {
                Objects.Clear();

                CanvasData.Update(DesiredSize, GridSize);

                // Find the actor who should be in the center of the radar
                // and whos position all other points should be plotted against.

                var center = ItemsSource.OfType<TrinityUnit>().FirstOrDefault(u => u.IsMe);
                if (center == null)
                    return;

                CenterActor = new TrinityItemPoint(center, CanvasData);
                CanvasData.CenterVector = CenterActor.Item.Position;

                // Calculate locations for all actors positions
                // on TrinityItemPoint ctor; or with .Update();

                foreach (var trinityObject in ItemsSource.OfType<TrinityObject>())
                {
                    var itemPoint = new TrinityItemPoint(trinityObject, CanvasData);
                    Objects.Add(itemPoint);
                }

                // Trigger Canvas to Render
                InvalidateVisual();            
            }
            catch (Exception ex)
            {
                Logger.Log("Exception in RadarUI.UpdateData(). {0} {1}", ex.Message, ex.InnerException);
            }
        }

        protected override void OnRender(DrawingContext dc)
        {
            if (!dc.CheckAccess())
            {
                Logger.Log("CurrentThread '{0} ({1})' does not have access to DrawingContext", Thread.CurrentThread.Name, Thread.CurrentThread.ManagedThreadId);
                return;
            }

            if (CanvasData.CanvasSize.Width == 0 && CanvasData.CanvasSize.Height == 0 || CanvasData.CenterVector == Vector3.Zero)
                return;

            try
            {                
                DrawGrid(dc, CanvasData, GridLineFrequency);

                // display the grid size text at the top
                var title = string.Format("{0}x{1}", CanvasData.Grid.Height, CanvasData.Grid.Width);
                dc.DrawText(new FormattedText(title, CultureInfo.InvariantCulture, FlowDirection.LeftToRight, new Typeface("Arial"), 20, Brushes.White), new Point(0, 0));

                DrawRangeGuide(dc, CanvasData);

                foreach (var actor in Objects)
                {
                    if (!actor.Morph.IsBeyondCanvas)
                        DrawActor(dc, CanvasData, actor);
                }

                DrawScenes(dc, CanvasData);
            }
            catch (Exception ex)
            {
                Logger.Log("Exception in RadarUI.OnRender(). {0} {1}", ex.Message, ex.InnerException);
            }
        }

        private void DrawScenes(DrawingContext dc, CanvasData canvas)
        {
            var pen = new Pen(Brushes.Yellow, 0.1);
            var brush = new SolidColorBrush(Colors.Transparent);

            try
            {
                //var currentScene = ZetaDia.Me.CurrentScene.Mesh.Zone.NavZoneDef.
                //var scenes = ZetaDia.Scenes.Where(s => s.Mesh.Zone != null).ToList();
                ////scenes.ForEach(s =>
                ////{
                //    //GridSegmentation.Update();

                //    //var nodes = GridSegmentation.Nodes.Where(n => n.Flags.HasFlag(NavCellFlags.AllowWalk));
              
                //    //var squares = s.Mesh.Zone.GridSquares.Where(sq => sq.Flags.HasFlag(NavCellFlags.AllowWalk))
                //    scenes.ForEach(n =>
                //    {
                //        //var point = new PointMorph(n.Center.ToVector3(), canvas).GridPoint;
                //        dc.DrawEllipse(brush, pen, new PointMorph(n.Mesh.Zone.ZoneMin.ToVector3(), canvas).Point, 5, 5);
                //        //Logger.Log("Node GridPoint={0}x{1}", point.X, point.Y);
                //    });
                    
                    //var min = new PointMorph(s.Mesh.Zone.ZoneMin.ToVector3(), canvas);
                    //var max = new PointMorph(, canvas);
                    //Logger.Log("Rect GridPoints Min={0}x{1} Max={2}{3}", min.GridPoint.X, min.GridPoint.Y, max.GridPoint, max.GridPoint.Y);
                    //dc.DrawRectangle(brush, pen, new Rect(min.Point,max.Point));                    
                //});

            }
            catch (Exception ex)
            {
                Logger.Log("Exception in RadarUI.DrawScenes(). {0} {1}", ex.Message, ex.InnerException);
            }
            
        }

        /// <summary>
        /// Range guide drawsd a circle every 20yard from player
        /// </summary>
        private void DrawRangeGuide(DrawingContext dc, CanvasData canvas)
        {
            var pen = new Pen(Brushes.LightYellow, 0.1);
            var brush = new SolidColorBrush(Colors.Transparent);

            for (var i = 20; i < 100; i+=20)
            {
                var radius = GridSize*i;
                dc.DrawEllipse(brush, pen, canvas.Center, radius, radius);

                dc.DrawText(new FormattedText(i.ToString(), CultureInfo.InvariantCulture, FlowDirection.LeftToRight, new Typeface("Arial"), 12, Brushes.LightYellow),
                    new Point(canvas.Center.X - (radius + 20), canvas.Center.Y));
            }            
        }

        private void DrawActor(DrawingContext dc, CanvasData canvas, TrinityItemPoint actor)
        {
            try
            {
                var border = new Pen(Brushes.Black, 0.1);
                var baseColor = GetActorColor(actor);            
                var actorName = actor.Item.Name;
                var trinityType = actor.Item.TrinityType;
                var actorRadius = actor.Item.Radius;

                if (baseColor == Colors.Transparent)
                {                
                    border = new Pen(new SolidColorBrush(Colors.LightGray), 1);
                    border.DashStyle = DashStyles.DashDotDot;
                    border.DashCap = PenLineCap.Flat;
                }
                else
                {
                    var formattedText = new FormattedText(string.Format("{0}", actorName), CultureInfo.InvariantCulture, FlowDirection.LeftToRight, new Typeface("Arial"), 11, new SolidColorBrush(baseColor))
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
                }
                                
                if (actor.Item.ActorType == ActorType.Projectile)
                {
                    // Draw a line representing the projectile
                    dc.DrawLine(new Pen(new SolidColorBrush(Colors.Red), 3), actor.Point, actor.HeadingPointAtRadius); 
                }
                else
                {
                    // Draw a circle representing the size of the actor
                    var outerFill = new SolidColorBrush(baseColor);
                    var gridRadius = actorRadius * GridSize;
                    outerFill.Opacity = 0.25;
                    dc.DrawEllipse(outerFill, border, actor.Point, gridRadius, gridRadius);
                }

                var unit = actor.Item as TrinityUnit;
                if (unit != null)
                {      
                    // Draw a line to indicate which way the unit is facing
                    var lighterBaseColor = ControlPaint.Light(baseColor.ToDrawingColor(), 50);
                    var drawingPen = new Pen(new SolidColorBrush(lighterBaseColor.ToMediaColor()), 1);
                    dc.DrawLine(drawingPen, actor.Point, actor.HeadingPointAtRadius);                          
                }

            }
            catch (Exception ex)
            {
                Logger.Log("Exception in RadarUI.DrawActor(). {0} {1}", ex.Message, ex.InnerException);
            }           
        }

        /// <summary>
        /// Returns a base color for actor based on type and stuff
        /// </summary>
        private static Color GetActorColor(TrinityItemPoint actor)
        {
            Color baseColor = Colors.White;
            try
            {
                switch (actor.Item.TrinityType)
                {
                    case TrinityObjectType.Avoidance:
                        baseColor = Colors.OrangeRed;
                        break;

                    case TrinityObjectType.Portal:
                    case TrinityObjectType.Container:
                    case TrinityObjectType.CursedChest:
                    case TrinityObjectType.CursedShrine:
                    case TrinityObjectType.Shrine:
                    case TrinityObjectType.HealthWell:
                    case TrinityObjectType.Interactable:
                        baseColor = Colors.Yellow;
                        break;

                    case TrinityObjectType.ProgressionGlobe:
                    case TrinityObjectType.PowerGlobe:
                    case TrinityObjectType.HealthGlobe:
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
                            else if (unit.IsHostile)
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
                    

            }
            catch (Exception ex)
            {
                Logger.Log("Exception in RadarUI.GetActorColor(). {0} {1}", ex.Message, ex.InnerException);
            }   
            return baseColor;
        }

        private void DrawGrid(DrawingContext dc, CanvasData canvas, int gridLineFrequency)
        {                
            Pen pen = new Pen(Brushes.Black, 0.1);

            // vertical lines
            int pos = 0;
            var height = 0;
            do
            {
                if (gridLineFrequency != 0 && (pos / gridLineFrequency) != 1)
                {
                    dc.DrawLine(pen, new Point(pos, 0), new Point(pos, (int) DesiredSize.Height));
                }

                pos += (int)canvas.GridSquareSize.Height;
                height++;

            } while (pos < DesiredSize.Width);

            // horizontal lines
            pos = 0;
            var width = 0;
            do
            {
                if (gridLineFrequency != 0 && (pos/gridLineFrequency) != 1)
                {
                    dc.DrawLine(pen, new Point(0, pos), new Point((int) DesiredSize.Width, pos));
                }
                pos += (int)canvas.GridSquareSize.Width;
                width++;
            } while (pos < DesiredSize.Height);

        }

        /// <summary>
        /// TrinityItemPoint wraps a TrinityObject to add a canvas plot location.
        /// </summary>
        public class TrinityItemPoint : INotifyPropertyChanged
        {
            private TrinityObject _item;

            public PointMorph Morph = new PointMorph();

            public Vector3 HeadingVectorAtRadius { get; set; }

            public Point HeadingPointAtRadius { get; set; }

            public Point Point
            {
                get { return Morph.Point; }
            }

            public TrinityItemPoint(TrinityObject item, CanvasData canvasData)
            {
                Item = item;
                item.PropertyChanged += ItemOnPropertyChanged;
                Morph.CanvasData = canvasData;
                Update();
            }

            /// <summary>
            /// Updates the plot location on canvas based on Item's current position.
            /// </summary>
            public void Update()
            {
                try
                {
                    Morph.Update(Item.Position);

                    HeadingVectorAtRadius = MathEx.GetPointAt(new Vector3(Item.Position.X, Item.Position.Y, 0), Item.Radius, Item.Movement.GetHeadingRadians());
                    HeadingPointAtRadius = new PointMorph(HeadingVectorAtRadius, Morph.CanvasData).Point;
                }
                catch (Exception ex)
                {
                    Logger.Log("Exception in RadarUI.TrinityItemPoint.Update(). {0} {1}", ex.Message, ex.InnerException);
                }                
            }

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
    }

    /// <summary>
    /// Houses canvas information, so a bunch of structs can be accessed by reference.
    /// </summary>
    public static class DrawingUtilities
    {
        public static System.Drawing.Color ToDrawingColor(this System.Windows.Media.Color color)
        {
            return System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
        }

        public static System.Windows.Media.Color ToMediaColor(this System.Drawing.Color color)
        {
            return System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B);
        }

        
    }

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
        /// Size of a single grid square
        /// </summary>
        public Size GridSquareSize;

        /// <summary>
        /// The world space vector3 for the center for the canvas
        /// </summary>
        public Vector3 CenterVector { get; set; }

        public bool Initialized { get; set; }

        /// <summary>
        /// Updates the canvas information (for if canvas size changes)
        /// </summary>
        public void Update(Size canvasSize, int gridSize)
        {            
            Center = new Point(canvasSize.Width / 2, canvasSize.Height / 2);
            RotationTransform = new RotateTransform(45, Center.X, Center.Y);
            FlipTransform = new ScaleTransform(1, -1, Center.X, Center.Y);
            Transforms = new TransformGroup();
            Transforms.Children.Add(RotationTransform);
            Transforms.Children.Add(FlipTransform);
            CanvasSize = canvasSize;
            Grid = new Size((int)(canvasSize.Width / gridSize), (int)(canvasSize.Height / gridSize));
            GridSquareSize = new Size(gridSize, gridSize);
            Initialized = true;
        }
    }

    /// <summary>
    /// PointMorph handles the translation of a Vector3 world space position into Canvas space.
    /// </summary>
    public class PointMorph
    {
        public PointMorph() { }

        public PointMorph(CanvasData canvasData)
        {
            CanvasData = canvasData;
        }

        public PointMorph(Vector3 vectorPosition, CanvasData canvasData)
        {
            CanvasData = canvasData;
            Update(vectorPosition);
        }

        /// <summary>
        /// Information about the canvas
        /// </summary>
        public CanvasData CanvasData { get; set; }

        /// <summary>
        /// Point in GridSquare (Yards) Space before translations
        /// </summary>
        public Point RawGridPoint { get; set; }

        /// <summary>
        /// Point before any translations
        /// </summary>
        public Point RawPoint { get; set; }

        /// <summary>
        /// Flipped and Rotated point
        /// </summary>
        public Point Point { get; set; }

        /// <summary>
        /// Point coods based on Grid Scale
        /// </summary>
        public Point GridPoint { get; set; }

        /// <summary>
        /// If the point is located outside of the canvas bounds
        /// </summary>
        public bool IsBeyondCanvas { get; set; }

        /// <summary>
        /// Game world distance from this point to the center actor on X-Axis
        /// </summary>
        public float RawWorldDistanceX { get; set; }

        /// <summary>
        /// Game world distance from this point to the center actor on Y-Axis
        /// </summary>
        public float RawWorldDistanceY { get; set; }

        /// <summary>
        /// Canvas (pixels) distance from this point to the center actor on Y-Axis
        /// </summary>
        public float RawDrawDistanceX { get; set; }

        /// <summary>
        /// Canvas (pixels) distance from this point to the center actor on Y-Axis
        /// </summary>
        public float RawDrawDistanceY { get; set; }

        /// <summary>
        /// Absolute canvas X-Axis coodinate for this actor (in pixels)
        /// </summary>
        public double RawDrawPositionX { get; set; }

        /// <summary>
        /// Absolute canvas Y-Axis coodinate for this actor (in pixels)
        /// </summary>
        public double RawDrawPositionY { get; set; }

        public void Update(Vector3 position)
        {
            try
            {
                var centerActorPosition = CanvasData.CenterVector;

                // Distance from Actor to Player
                RawWorldDistanceX = centerActorPosition.X - position.X;
                RawWorldDistanceY = centerActorPosition.Y - position.Y;

                if (Math.Abs(RawWorldDistanceX) > 200 || Math.Abs(RawWorldDistanceY) > 200)
                    return;

                // We want 1 yard of game distance to = Gridsize
                RawDrawDistanceX = RawWorldDistanceX * (float)CanvasData.GridSquareSize.Width;
                RawDrawDistanceY = RawWorldDistanceY * (float)CanvasData.GridSquareSize.Height;

                // Distance on canvas from center to actor
                RawDrawPositionX = (CanvasData.Center.X + RawDrawDistanceX);
                RawDrawPositionY = (CanvasData.Center.Y + RawDrawDistanceY);

                // Points in Canvas and Grid Scale
                RawPoint = new Point(RawDrawPositionX, RawDrawPositionY);
                RawGridPoint = new Point(RawDrawPositionX / CanvasData.GridSquareSize.Width, RawDrawPositionY / CanvasData.GridSquareSize.Height);

                Point = CanvasData.Transforms.Transform(RawPoint);
                GridPoint = new Point((int)(Point.X / CanvasData.GridSquareSize.Width), (int)(Point.Y / CanvasData.GridSquareSize.Height));
                IsBeyondCanvas = Point.X < 0 || Point.X > CanvasData.CanvasSize.Width || Point.Y < 0 || Point.Y > CanvasData.CanvasSize.Height;

            }
            catch (Exception ex)
            {
                Logger.Log("Exception in RadarUI.PointMorph.Update(). {0} {1}", ex.Message, ex.InnerException);
            }
        }

    }

}
