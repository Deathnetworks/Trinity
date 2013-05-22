using System;
using Zeta;
using Zeta.Common;
using Zeta.Internals.Actors;
using Zeta.Internals.SNO;
using System.Text.RegularExpressions;


namespace Trinity.Cache
{
    internal class CacheAvoidance : CacheObject
    {
        public CacheAvoidance(DiaObject dia)
            : base(dia)
        {
            CacheType = CacheType.Avoidance;
        }

        public bool IsProjectile { get; set; }
        public bool IsBuffVisualEffect { get; set; }

        public override CacheObject Clone()
        {
            return (CacheAvoidance)this.MemberwiseClone();
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
