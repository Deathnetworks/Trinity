using System;
using Zeta;
using Zeta.Common;
using Zeta.Internals.Actors;
using Zeta.Internals.SNO;
using System.Text.RegularExpressions;


namespace GilesTrinity.Cache
{
    internal class CacheAvoidance : CacheObject
    {
        public CacheAvoidance(ACD acd)
            : base(acd)
        {

        }

        public bool IsProjectile { get; set; }
        public bool IsBuffVisualEffect { get; set; }

        public override CacheObject Clone()
        {
            return new CacheAvoidance(this.CommonData);
        }

    }
}
