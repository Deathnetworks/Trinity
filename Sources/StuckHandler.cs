using GilesTrinity.DbProvider;
using Zeta.Common;
using Zeta.Navigation;

namespace GilesTrinity
{
    /// <summary>
    /// Blank Stuck Handler - to disable DB stuck handler
    /// </summary>
    public class GilesStuckHandler : IStuckHandler
    {
        public bool IsStuck 
        { 
            get 
            { 
                return GilesPlayerMover.UnstuckChecker(); 
            } 
        }

        public Vector3 GetUnstuckPos() 
        { 
            return GilesPlayerMover.UnstuckHandler(); 
        }
    }
}
