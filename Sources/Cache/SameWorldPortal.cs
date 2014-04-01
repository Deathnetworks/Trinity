using System;
using Zeta.Common;

namespace Trinity.Cache
{
    class SameWorldPortal
    {
        public int ActorSNO { get; set; }
        public int RActorGUID { get; set; }
        public Vector3 StartPosition { get; set; }
        public DateTime LastInteract { get; set; }
        public int WorldID { get; set; }

        public SameWorldPortal()
        {
            StartPosition = Trinity.Player.Position;
            WorldID = Trinity.Player.WorldID;
            LastInteract = DateTime.UtcNow;
        }
    }
}
