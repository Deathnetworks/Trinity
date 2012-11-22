using Zeta.Common;
using Zeta.Navigation;
namespace GilesTrinity
{

    // Blank Stuck Handler - to disable DB stuck handler
    public class GilesStuckHandler : IStuckHandler
    {
        public bool IsStuck { get { return GilesPlayerMover.UnstuckChecker(); } }
        public Vector3 GetUnstuckPos() { return GilesPlayerMover.UnstuckHandler(); }
    }
}
