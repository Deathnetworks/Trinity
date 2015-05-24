using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using Trinity.LazyCache;
using Zeta.Common;
using FlowDirection = System.Windows.FlowDirection;
using LineSegment = System.Windows.Media.LineSegment;
using Logger = Trinity.Technicals.Logger;

namespace Trinity.UIComponents
{

    public class RadarCanvas : Canvas
    {
        public RadarCanvas()
        {
            ClipToBounds = true;
            
            CanvasData.OnCanvasSizeChanged += (before, after) =>
            {
                // Scene drawings are specific to a canvas size.
                // Changing canvas size means we have to redraw them all.
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
        public RadarObject CenterActor { get; set; }

        /// <summary>
        /// Collection of game objects
        /// </summary>
        public List<RadarObject> Objects = new List<RadarObject>();

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

                CenterActor = new RadarObject(center, CanvasData);
                CanvasData.CenterVector = CenterActor.Actor.Position;

                // Calculate locations for all actors positions
                // on RadarObject ctor; or with .Update();

                foreach (var trinityObject in ItemsSource.OfType<TrinityObject>())
                {
                    var radarObject = new RadarObject(trinityObject, CanvasData);
                    Objects.Add(radarObject);
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

        /// <summary>
        /// Maintain our collection of walkable areas of scene drawings, removing old or invalid ones.
        /// Calculates new position for each scene drawing based on its origin point.
        /// </summary>
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

        /// <summary>
        /// OnRender is a core part of Canvas, replace it with our render code.
        /// Can be manually triggered by InvalidateVisual();
        /// </summary>
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
                    // And because translating geometry for every navcell on every frame is waaaaay too slow.
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
   
                        // Have to use SceneHash as key because scenes can appear multiple times with the same name
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
        /// Range guide draws a circle every 20yard from player
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

        private void DrawActor(DrawingContext dc, CanvasData canvas, RadarObject actor)
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
        private static Color GetActorColor(RadarObject actor)
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


    }







}
