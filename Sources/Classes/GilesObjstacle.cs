using Zeta.Common;
using Zeta.Common.Plugins;
namespace GilesTrinity
{
    public partial class GilesTrinity : IPlugin
    {
        // Obstacles for quick mapping of paths etc.
        public class GilesObstacle
        {
            public Vector3 vThisLocation { get; set; }
            public float fThisRadius { get; set; }
            public int iThisSNOID { get; set; }
            public double dThisWeight { get; set; }
            public GilesObstacle(Vector3 thislocation, float thisradius, int thissnoid, double thisweight = 0)
            {
                vThisLocation = thislocation;
                fThisRadius = thisradius;
                iThisSNOID = thissnoid;
                dThisWeight = thisweight;
            }
        }
    }
}
