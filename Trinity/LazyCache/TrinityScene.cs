using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media;
using System.Xml;
using Zeta.Common;
using Zeta.Game;
using Zeta.Game.Internals;
using Zeta.Game.Internals.SNO;
using Logger = Trinity.Technicals.Logger;

namespace Trinity.LazyCache
{
    /// <summary>
    ///  RadarUI shits itself when i try to request scene data, so caching it with CacheManager instead so RadarUI can just read the data.
    /// </summary>
    public class TrinityScene
    {
        private NavZoneDef _navZoneDef;
        private NavZone _zone;
        private Scene _scene;
        private List<AABB> _walkableNavCellBounds = new List<AABB>();

        public int Guid { get; set; }
        public string Name { get; set; }
        public AABB Bounds { get; set; }
        public Vector3 Min { get; set; }
        public Vector3 Max { get; set; }
        public NavZone Zone { get; set; }
        public NavZoneDef NavZoneDef { get; set; }
        public bool IsFiller { get; set; }
        public string SceneHash { get; set; }
        public int WorldDynamicId { get; set; }
        public float CenterX { get; set; }
        public float CenterY { get; set; }
        public Vector3 Center { get; set; }
        public Vector3 NorthEast { get; set; }
        public Vector3 NorthWest { get; set; }
        public Vector3 SouthEast { get; set; }
        public Vector3 SouthWest { get; set; }
        public float HalfEdgeLength { get; set; }
        public float EdgeLength { get; set; }
        public bool HasLoadedNavCells { get; set; }

        public TrinityScene(Scene scene, int sceneGuid = -1)
        {
            _scene = scene;
            Guid = sceneGuid != -1 ? sceneGuid : scene.SceneGuid;
            Name = scene.Name;
            IsFiller = scene.Name.ToLowerInvariant().Contains("fill");            
            WorldDynamicId = CacheManager.WorldDynamicId;

            Update(scene);
        }

        public TrinityScene Update(Scene scene)
        {
            if (Zone == null)
            {
                Zone = scene.Mesh.Zone;

                if (Zone != null)
                {
                    NavZoneDef = Zone.NavZoneDef;
                    CalculateBounds();
                }
            }
     
            if (!HasLoadedNavCells && Zone != null)
                _walkableNavCellBounds = FindWalkableNavCellBounds();   
        
            return this;
        }

        public void CalculateBounds()
        {
            Bounds = new AABB
            {
                Max = Zone.ZoneMax.ToVector3(),
                Min = Zone.ZoneMin.ToVector3()
            };

            Max = Bounds.Max;
            Min = Bounds.Min;

            // Create a persistable way to uniquely identify a scene at a location
            SceneHash = string.Format("{0}_[{1},{2}][{3},{4}]", _scene.Name, Min.X, Min.Y, Max.X, Max.Y);

            EdgeLength = Bounds.Max.X - Bounds.Min.X / 2;
            HalfEdgeLength = EdgeLength / 2;
            
            SouthWest = new Vector3(Bounds.Max.X, Bounds.Min.Y, 0);
            SouthEast = new Vector3(Bounds.Max.X, Bounds.Max.Y, 0);
            NorthWest = new Vector3(Bounds.Min.X, Bounds.Min.Y, 0);
            NorthEast = new Vector3(Bounds.Min.X, Bounds.Max.Y, 0);
            CenterX = Bounds.Min.X + (Bounds.Max.X - Bounds.Min.X) / 2;
            CenterY = Bounds.Min.Y + (Bounds.Max.Y - Bounds.Min.Y) / 2;
            Center = new Vector3(CenterX, CenterY, 0);            
        }
        
        public List<AABB> WalkableNavCellBounds
        {
            get { return _walkableNavCellBounds; }
        }

        public List<TrinityNavCell> NavCells { get; private set; }

        public bool IsCurrentScene
        {
            get { return Guid == CacheManager.Me.CurrentSceneGuid;  }            
        }

        public bool IsCurrentWorld
        {
            get { return WorldDynamicId == CacheManager.WorldDynamicId; }
        }

        public class TrinityNavCell
        {
            public TrinityNavCell() {}

            public TrinityNavCell(NavCell navCell, AABB zoneBounds)
            {
                RelativeBounds = navCell.Bounds;
                Flags = navCell.Flags;
                Min = navCell.Min;
                Max = navCell.Max;                
                ZoneBounds = zoneBounds;
                AbsBounds = new AABB
                {
                    Max = new Vector3(ZoneBounds.Min.X + navCell.Max.X, ZoneBounds.Min.Y + navCell.Max.Y, 0),
                    Min = new Vector3(ZoneBounds.Min.X + navCell.Min.X, ZoneBounds.Min.Y + navCell.Min.Y, 0)
                };
            }

            public NavCellFlags Flags { get; set; }
            public AABB RelativeBounds { get; set; }
            public Vector3 Min { get; set; }
            public Vector3 Max { get; set; }
            public AABB AbsBounds { get; set; }
            public AABB ZoneBounds { get; set; }
        }

        private List<AABB> FindWalkableNavCellBounds()
        {                
            var walkableCells = new List<AABB>();

            using(new PerformanceTimer("FindWalkableNavCellBounds"))
            {
                if (NavZoneDef != null && NavZoneDef.IsValid)
                {
                    NavCells = NavZoneDef.NavCells.Select(nc => new TrinityNavCell(nc,Bounds)).ToList();

                    foreach (var navCell in NavCells)
                    {
                        if (!navCell.Flags.HasFlag(NavCellFlags.AllowWalk))
                            continue;

                        walkableCells.Add(navCell.AbsBounds);
                    }

                    HasLoadedNavCells = true;
                }
            }

            return walkableCells;
        }

    }
}
