using System;
using System.Collections.Generic;
using Zeta.Common;
using Zeta.Game;

namespace Trinity
{
    // Obstacles for quick mapping of paths etc.
    internal class CacheObstacleObject
    {
        public Vector3 Position { get; set; }
        public float Distance { get; set; }
        public float Radius { get; set; }
        public int ActorSNO { get; set; }
        public double Weight { get; set; }
        public string Name { get; set; }
        public int HitPointsCurPct { get; set; }
        public int HitPointsCur { get; set; }
        public DateTime Expires { get; set; }
        public int RActorGUID { get; set; }
        public float Rotation { get; set; }
        public float BeamLength { get; set; }

        public List<SNOAnim> AvoidanceAnimations { get; set; }
        public float DirectionalAvoidanceDegrees { get; set; }
        public bool AvoidAtPlayerPosition { get; set; }
        public GObjectType ObjectType { get; set; }
        public bool IsAvoidanceAnimations { get; set; }
        public SNOAnim Animation { get; set; }

        public AvoidanceType AvoidanceType
        {
            get
            {
                return AvoidanceManager.GetAvoidanceType(ActorSNO);
            }
        }

        public CacheObstacleObject() { }

        public CacheObstacleObject(Vector3 position, float radius, int actorSNO = 0, string name = "")
        {
            Position = position;
            Radius = radius;
            ActorSNO = actorSNO;
            Name = name;
            Expires = DateTime.MinValue;
        }

        public CacheObstacleObject(TrinityCacheObject tco)
        {
            ActorSNO = tco.ActorSNO;
            Radius = tco.Radius;
            Position = tco.Position;
            RActorGUID = tco.RActorGuid;
            ObjectType = tco.Type;
            Name = tco.InternalName;
        }
    }
}
