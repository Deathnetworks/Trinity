using System;
using Zeta;
using Zeta.Common;
using Zeta.Internals.Actors;
using Zeta.Internals.SNO;
using System.Text.RegularExpressions;

namespace GilesTrinity.Cache
{
    /// <summary>
    /// Cached Gizmos
    /// </summary>
    internal class CacheGizmo : CacheObject
    {
        public CacheGizmo(DiaObject dia)
            : base(dia)
        {
            CacheType = CacheType.Gizmo;
        }
        /// <summary>
        /// Gets the DiaGizmo
        /// </summary>
        public DiaGizmo InternalGizmo { get; set; }
        public bool IsObstacle { get; set; }
        public GizmoType Gizmotype { get; set; }
        public bool IsBarricade { get; set; }
        public bool Isdestructible { get; set; }
        public bool IsGizmoDisabledByScript { get; set; }
        public bool IsDoorOpen { get; set; }
        public bool IsHealthWellReady { get; set; }
        public bool IsLootContainerOpen { get; set; }

        public override CacheObject Clone()
        {
            return (CacheObject)this.MemberwiseClone();
        }


        public override float RadiusDistance
        {
            get;
            set;
        }

        public override float Radius
        {
            get;
            set;

        }

        public override string IgnoreReason
        {
            get;
            set;

        }

        public override string IgnoreSubStep
        {
            get;
            set;

        }
    }

}
