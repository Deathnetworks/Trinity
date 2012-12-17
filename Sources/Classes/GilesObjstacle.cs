using Zeta.Common;
namespace GilesTrinity
{
    // Obstacles for quick mapping of paths etc.
    internal class GilesObstacle
    {
        public Vector3 Location { get; set; }
        public float Radius { get; set; }
        public int ActorSNO { get; set; }
        public double Weight { get; set; }
        public string Name { get; set; }
        public int HitPointsCurPct { get; set; }
        public int HitPointsCur { get; set; }

        public GilesObstacle()
        {

        }

        public GilesObstacle(Vector3 location, float radius, int snoId, double weight = 0, string name = "")
        {
            Location = location;
            Radius = radius;
            ActorSNO = snoId;
            Weight = weight;
        }
    }
}
