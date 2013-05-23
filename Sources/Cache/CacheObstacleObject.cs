using Zeta.Common;
namespace Trinity
{
    // Obstacles for quick mapping of paths etc.
    internal class CacheObstacleObject
    {
        public Vector3 Location { get; set; }
        public float Radius { get; set; }
        public int ActorSNO { get; set; }
        public double Weight { get; set; }
        public string Name { get; set; }
        public int HitPointsCurPct { get; set; }
        public int HitPointsCur { get; set; }
        public AvoidanceType AvoidanceType
        {
            get
            {
                return AvoidanceManager.GetAvoidanceType(this.ActorSNO);
            }
        }

        public CacheObstacleObject()
        {

        }

        public CacheObstacleObject(Vector3 location, float radius, int snoId, double weight = 0, string name = "")
        {
            Location = location;
            Radius = radius;
            ActorSNO = snoId;
            Weight = weight;
        }
    }
}
