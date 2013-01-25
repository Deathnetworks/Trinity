using GilesTrinity.DbProvider;
using Zeta.Common;
using Zeta.Navigation;

namespace GilesTrinity
{
    /// <summary>
    /// Blank Stuck Handler - to disable DB stuck handler
    /// </summary>
    public class StuckHandler : IStuckHandler
    {
        public bool IsStuck 
        { 
            get 
            { 
                return PlayerMover.UnstuckChecker(); 
                //return false;
            } 
        }

        public Vector3 GetUnstuckPos() 
        { 
            return PlayerMover.UnstuckHandler(); 
        }
    }
}
