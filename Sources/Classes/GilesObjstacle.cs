﻿using Zeta.Common;
using Zeta.Common.Plugins;
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
        public GilesObstacle(Vector3 location, float radius, int snoId, double weight = 0, string name = "")
        {
            Location = location;
            Radius = radius;
            ActorSNO = snoId;
            Weight = weight;
        }
    }
}
