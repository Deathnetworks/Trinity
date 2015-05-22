using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Forms;
using Trinity.Helpers;
using Trinity.LazyCache;
using Zeta.Bot.Dungeons;
using Zeta.Common;
using Zeta.Game;
using Zeta.Game.Internals;
using Zeta.Game.Internals.Actors;
using Zeta.Game.Internals.SNO;
using Brush = System.Windows.Media.Brush;
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
            ClipToBounds = true;
            
            CanvasData.OnCanvasSizeChanged += (before, after) =>
            {
                Drawings.Relative.Clear();
                Drawings.Static.Clear();
                Clip = CanvasData.ClipRegion;
            };

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
        /// How many compiled scene drawings to keep around
        /// </summary>
        public const int SceneGeomStorageLimit = 25;

        /// <summary>
        /// The size (in pixels) to draw actor markers
        /// </summary>
        public double MarkerSize = 5;

        /// <summary>
        /// The actor who should be at the center of the radar
        /// </summary>
        public TrinityNode CenterActor { get; set; }

        /// <summary>
        /// Collection of game objects
        /// </summary>
        public List<TrinityNode> Objects = new List<TrinityNode>();

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

        /// <summary>
        /// ItemSource binding on control is set
        /// </summary>
        void OnItemsSourceChanged(DependencyPropertyChangedEventArgs args)
        {
            UpdateData();

            if (args.OldValue is INotifyCollectionChanged)
                (args.OldValue as INotifyCollectionChanged).CollectionChanged -= OnItemsSourceCollectionChanged;

            if (args.NewValue is INotifyCollectionChanged)
                (args.NewValue as INotifyCollectionChanged).CollectionChanged += OnItemsSourceCollectionChanged;
        }

        /// <summary>
        /// When objects inside ItemSource collection change
        /// </summary>
        void OnItemsSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            UpdateData();
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
                if (DesiredSize.Height <= 0 || DesiredSize.Width <= 0)
                    return;

                Objects.Clear();

                CanvasData.Update(DesiredSize, GridSize);

                // Find the actor who should be in the center of the radar
                // and whos position all other points should be plotted against.

                var center = ItemsSource.OfType<TrinityUnit>().FirstOrDefault(u => u.IsMe);
                if (center == null)
                    return;

                CenterActor = new TrinityNode(center, CanvasData);
                CanvasData.CenterVector = CenterActor.Actor.Position;

                // Calculate locations for all actors positions
                // on TrinityItemPoint ctor; or with .Update();

                foreach (var trinityObject in ItemsSource.OfType<TrinityObject>())
                {
                    var itemPoint = new TrinityNode(trinityObject, CanvasData);
                    Objects.Add(itemPoint);
                }

                UpdateSceneData();

                // Trigger Canvas to Render
                InvalidateVisual();    
            }
            catch (Exception ex)
            {
                Logger.Log("Exception in RadarUI.UpdateData(). {0} {1}", ex.Message, ex.InnerException);
            }
        }

        internal static class Drawings
        {
            public static ConcurrentDictionary<string, RelativeDrawing> Relative = new ConcurrentDictionary<string, RelativeDrawing>();
            public static ConcurrentDictionary<string, StaticDrawing> Static = new ConcurrentDictionary<string, StaticDrawing>();
        }

        internal class StaticDrawing
        {
            public DrawingGroup Drawing { get; set; }
            public int WorldId { get; set; }
        }

        internal class RelativeDrawing : StaticDrawing
        {
            public PointMorph Origin { get; set; }
            public Vector3 Center { get; set; }
        }

        private void UpdateSceneData()
        {
            var sceneKeysToRemove = new List<string>();
            foreach (var scene in Drawings.Relative)
            {
                if (scene.Value.WorldId != CacheManager.WorldDynamicId)
                {
                    sceneKeysToRemove.Add(scene.Key);
                    continue;
                }
                
                DrawingUtilities.RelativeMove(scene.Value.Drawing, scene.Value.Origin.WorldVector);
            }
            
            RelativeDrawing removedItem;

            foreach (var key in sceneKeysToRemove)
            {
                Drawings.Relative.TryRemove(key, out removedItem);
            }

            if (Drawings.Relative.Count > SceneGeomStorageLimit)
            {
                var firstKey = Drawings.Relative.Keys.First();
                Drawings.Relative.TryRemove(firstKey, out removedItem);
            }
        }


        protected override void OnRender(DrawingContext dc)
        {

            if (CanvasData.CanvasSize.Width == 0 && CanvasData.CanvasSize.Height == 0 || CanvasData.CenterVector == Vector3.Zero)
                return;

            if (CenterActor.Point.X == 0 && CenterActor.Point.Y == 0)
                return;

            try
            {                
                DrawGrid(dc, CanvasData, GridLineFrequency);

                //DrawRangeGuide(dc, CanvasData);

                foreach (var actor in Objects)
                {
                    if (!actor.Morph.IsBeyondCanvas)
                        DrawActor(dc, CanvasData, actor);
                }

                dc.DrawRectangle(null, new Pen(Brushes.WhiteSmoke, 1), CanvasData.ClipRegion.Rect);

                DrawScenes(dc, CanvasData);                               
            }
            catch (Exception ex)
            {
                Logger.Log("Exception in RadarUI.OnRender(). {0} {1}", ex.Message, ex.InnerException);
            }

        }

        private static class RadarBrushes
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

        /// <summary>
        /// Mind = Blown
        /// </summary>
        private void DrawScenes(DrawingContext dc, CanvasData canvas)
        {
            try
            {
                if (CenterActor.Point.X == 0 && CenterActor.Point.Y == 0)
                    return;

                foreach (var pair in CacheManager.CachedScenes)
                {                    
                    var scene = pair.Value;

                    // Combine navcells into one drawing and store it; because they don't change relative to each other
                    if (!Drawings.Relative.ContainsKey(scene.SceneHash) && scene.WalkableNavCellBounds.Any() && scene.IsCurrentWorld)
                    {
                        var drawing = new DrawingGroup();

                        using (var groupdc = drawing.Open())
                        {
                            var figures = new List<PathFigure>();
                            
                            foreach (var navCellBounds in scene.WalkableNavCellBounds)
                            {
                                var cellSouthWest = new Vector3(navCellBounds.Max.X, navCellBounds.Min.Y, 0).ToCanvasPoint();
                                var cellSouthEast = new Vector3(navCellBounds.Max.X, navCellBounds.Max.Y, 0).ToCanvasPoint();
                                var cellNorthWest = new Vector3(navCellBounds.Min.X, navCellBounds.Min.Y, 0).ToCanvasPoint();
                                var cellNorthEast = new Vector3(navCellBounds.Min.X, navCellBounds.Max.Y, 0).ToCanvasPoint();

                                var segments = new[]
                                {
                                    new LineSegment(cellNorthWest, true),
                                    new LineSegment(cellNorthEast, true),
                                    new LineSegment(cellSouthEast, true)
                                };

                                figures.Add(new PathFigure(cellSouthWest, segments, true));
                            }            
                            
                            var geo = new PathGeometry(figures, FillRule.Nonzero, null);
                            geo.GetOutlinedPathGeometry();                            
                            groupdc.DrawGeometry(RadarBrushes.WalkableTerrain, null, geo);                            
                        }
   
                        // Have to use Guid as key because scenes can appear multiple times with the same name
                        Drawings.Relative.TryAdd(scene.SceneHash, new RelativeDrawing
                        {
                            Drawing = drawing,
                            Origin = CenterActor.Morph,
                            Center = scene.Center,
                            WorldId = scene.WorldDynamicId
                        });
                    }                  
                }

                foreach (var pair in Drawings.Relative)
                {
                    if (pair.Value.WorldId != CacheManager.WorldDynamicId)
                        continue;

                    if(!pair.Value.Drawing.Bounds.IntersectsWith(CanvasData.ClipRegion.Rect))
                        continue;

                    dc.DrawDrawing(pair.Value.Drawing);
                }

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
            for (var i = 20; i < 100; i+=20)
            {
                var radius = GridSize*i;
                dc.DrawEllipse(RadarBrushes.TransparentBrush, RadarBrushes.LightYellowPen, canvas.Center, radius, radius);

                dc.DrawText(new FormattedText(i.ToString(), CultureInfo.InvariantCulture, FlowDirection.LeftToRight, new Typeface("Arial"), 12, Brushes.LightYellow),
                    new Point(canvas.Center.X - (radius + 20), canvas.Center.Y));
            }            
        }

        private void DrawActor(DrawingContext dc, CanvasData canvas, TrinityNode actor)
        {
            try
            {

                var baseColor = GetActorColor(actor);            
                var actorName = actor.Actor.Name;
                //var trinityType = actor.Actor.TrinityType;
                var actorRadius = actor.Actor.Radius;

                var unit = actor.Actor as TrinityUnit;
                if (unit != null)
                {
                    if (unit.Source.IsDisposed || unit.IsDead)
                        return;

                    // Draw a line to indicate which way the unit is facing
                    var lighterBaseColor = ControlPaint.Light(baseColor.ToDrawingColor(), 50);
                    var drawingPen = new Pen(new SolidColorBrush(lighterBaseColor.ToMediaColor()), 1);
                    dc.DrawLine(drawingPen, actor.Point, actor.HeadingPointAtRadius);
                }

                if ((actor.Actor.IsGizmo || actor.Actor.IsUnit) && baseColor != Colors.Transparent )
                {
                    // Draw a circle representing the size of the actor
                    var outerFill = new SolidColorBrush(baseColor);
                    var gridRadius = actorRadius*GridSize;
                    outerFill.Opacity = 0.25;
                    dc.DrawEllipse(outerFill, RadarBrushes.BorderPen, actor.Point, gridRadius, gridRadius);

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
                    dc.DrawEllipse(innerFill, RadarBrushes.BorderPen, actor.Point, MarkerSize / 2, MarkerSize / 2);
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
        private static Color GetActorColor(TrinityNode actor)
        {
            Color baseColor = Colors.White;
            try
            {
                switch (actor.Actor.TrinityType)
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

                        var unit = actor.Actor as TrinityUnit;
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
            StaticDrawing sd;

            if (!Drawings.Static.TryGetValue("Grid", out sd))
            {
                var drawing = new DrawingGroup();

                using (var groupdc = drawing.Open())
                {
                    // vertical lines
                    int pos = 0;
                    do
                    {
                        groupdc.DrawLine(RadarBrushes.GridPen, new Point(pos, 0), new Point(pos, (int) canvas.CanvasSize.Height));
                        pos += (int) canvas.GridSquareSize.Height*gridLineFrequency;

                    } while (pos < canvas.CanvasSize.Width);

                    // horizontal lines
                    pos = 0;
                    do
                    {
                        groupdc.DrawLine(RadarBrushes.GridPen, new Point(0, pos), new Point((int) canvas.CanvasSize.Width, pos));
                        pos += (int) canvas.GridSquareSize.Width*gridLineFrequency;

                    } while (pos < canvas.CanvasSize.Height);
                }

                drawing.Freeze();

                Drawings.Static.TryAdd("Grid", new StaticDrawing
                {
                    Drawing = drawing,
                    WorldId = CacheManager.WorldDynamicId
                });
            }
            else
            {
                dc.DrawDrawing(sd.Drawing);
            }
        }

        /// <summary>
        /// TrinityNode wraps a TrinityObject to add a canvas plot location.
        /// </summary>
        public class TrinityNode : INotifyPropertyChanged
        {
            private TrinityObject _actor;

            /// <summary>
            /// Contains the actors position and other useful information.
            /// </summary>
            public PointMorph Morph = new PointMorph();

            /// <summary>
            /// Position in game world space for a point at radius distance 
            /// from actor's center and in the direction the actor is facing.
            /// </summary>
            public Vector3 HeadingVectorAtRadius { get; set; }

            /// <summary>
            /// Position on canvas (in pixels) for a point at radius distance 
            /// from actor's center and in the direction the actor is facing.
            /// </summary>
            public Point HeadingPointAtRadius { get; set; }

            /// <summary>
            /// Actors current position on canvas (in pixels).
            /// </summary>
            public Point Point
            {
                get { return Morph.Point; }
            }

            /// <summary>
            /// TrinityNode wraps a TrinityObject to add a canvas plot location.
            /// </summary>
            public TrinityNode(TrinityObject obj, CanvasData canvasData)
            {
                Actor = obj;
                obj.PropertyChanged += ItemOnPropertyChanged;
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
                    Morph.Update(Actor.Position);

                    if (Actor.IsUnit || Actor.IsProjectile || Actor.AvoidanceType == AvoidanceType.Arcane)
                    {
                        HeadingVectorAtRadius = MathEx.GetPointAt(new Vector3(Actor.Position.X, Actor.Position.Y, 0), Actor.Radius, Actor.Movement.GetHeadingRadians());
                        HeadingPointAtRadius = new PointMorph(HeadingVectorAtRadius, Morph.CanvasData).Point;                        
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log("Exception in RadarUI.TrinityItemPoint.Update(). {0} {1}", ex.Message, ex.InnerException);
                }                
            }

            /// <summary>
            /// The game object
            /// </summary>
            public TrinityObject Actor
            {
                set
                {
                    if (!Equals(value, _actor))
                    {
                        _actor = value;
                        OnPropertyChanged(new PropertyChangedEventArgs("Actor"));
                    }
                }
                get { return _actor; }
            }

            #region PropertyChanged Handling

            public event PropertyChangedEventHandler PropertyChanged;

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
                return Actor.GetHashCode();
            }

        }
    }

    /// <summary>
    /// Useful tools for drawing stuff
    /// </summary>
    public static class DrawingUtilities
    {
        /// <summary>
        /// Convert to a Drawing System.Drawing.Color
        /// </summary>
        public static System.Drawing.Color ToDrawingColor(this System.Windows.Media.Color color)
        {
            return System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
        }

        /// <summary>
        /// Convert to a System.Windows.Media.Color
        /// </summary>
        public static System.Windows.Media.Color ToMediaColor(this System.Drawing.Color color)
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

        public static System.Drawing.Point ToDrawingPoint(this System.Windows.Point point)
        {
            return new System.Drawing.Point((int)point.X, (int)point.Y);
        }

        public static Point ToPoint(this Vector3 worldVector)
        {
            return new Point(worldVector.X, worldVector.Y);
        }

        public static System.Windows.Rect ToCanvasRect(this AABB bounds)
        {
            var max = bounds.Max;
            var min = bounds.Min;
            var center = new Vector3((int)(max.X - (max.X - min.X)/2), (int)(max.Y - (max.Y - min.Y)/2),0);
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

            ClipRegion = new RectangleGeometry(new Rect(new Point(20,20), new Size(canvasSize.Width - 40, canvasSize.Height - 40)));

            if (OnCanvasSizeChanged != null && !previousSize.Equals(CanvasSize))
                OnCanvasSizeChanged(previousSize, CanvasSize);
        }

        public static CanvasData LastCanvas { get; set; }

        public delegate void CanvasSizeChanged(Size sizeBefore, Size sizeAfter);

        public event CanvasSizeChanged OnCanvasSizeChanged;

        public RectangleGeometry ClipRegion { get; set; }
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

        /// <summary>
        /// PointMorph handles the translation of a Vector3 world space position into Canvas space.
        /// </summary>
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
        /// If the point is Zero
        /// </summary>
        public bool IsZero
        {
            get { return Point.X == 0 || Point.Y == 0;  }
        }

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

        /// <summary>
        /// Absolute game world vector
        /// </summary>
        public Vector3 WorldVector { get; set; }

        /// <summary>
        /// Calculates Canvas position with a given game world position
        /// </summary>
        public void Update(Vector3 position)
        {
            try
            {
                WorldVector = position;

                var centerActorPosition = CanvasData.CenterVector;

                // Distance from Actor to Player
                RawWorldDistanceX = centerActorPosition.X - position.X;
                RawWorldDistanceY = centerActorPosition.Y - position.Y;

                //if (Math.Abs(RawWorldDistanceX) > 1000 || Math.Abs(RawWorldDistanceY) > 1000)
                //    return;

                // We want 1 yard of game distance to = Gridsize
                RawDrawDistanceX = RawWorldDistanceX * (float)CanvasData.GridSquareSize.Width;
                RawDrawDistanceY = RawWorldDistanceY * (float)CanvasData.GridSquareSize.Height;

                // Distance on canvas from center to actor
                RawDrawPositionX = (CanvasData.Center.X + RawDrawDistanceX);
                RawDrawPositionY = (CanvasData.Center.Y + RawDrawDistanceY);

                // Points in Canvas and Grid Scale
                RawPoint = new Point(RawDrawPositionX, RawDrawPositionY);
                RawGridPoint = new Point(RawDrawPositionX / CanvasData.GridSquareSize.Width, RawDrawPositionY / CanvasData.GridSquareSize.Height);

                Point point;
                if (CanvasData.Transforms.TryTransform(RawPoint, out point))
                {
                    Point = point;
                }
                else
                {
                    Logger.Log("Point Transform Failed on {0}x{1}", RawPoint.X, RawPoint.Y);
                }

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
