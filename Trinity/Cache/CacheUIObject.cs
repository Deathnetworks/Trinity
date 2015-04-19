using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Trinity.UI.UIComponents;
using Zeta.Game.Internals.Actors;

namespace Trinity.Cache
{
    public class CacheUIObject : IEquatable<CacheUIObject>
    {
        public int Distance { get; set; }
        public int Radius { get; set; }
        public int RadiusDistance { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string InCache { get; set; }
        public string IgnoreReason { get; set; }
        public int Weight { get; set; }
        public string IsBoss { get; set; }
        public string IsElite { get; set; }
        public string IsQuestMonster { get; set; }
        public string IsMinimapActive { get; set; }
        public string WeightInfo { get; set; }
        public int RActorGUID { get; set; }
        public int ActorSNO { get; set; }

        public CacheUIObject(DiaObject obj)
        {
            try
            {
                RActorGUID = obj.RActorGuid;
                ActorSNO = obj.ActorSNO;
                Distance = (int)obj.Distance;
                Radius = (int)obj.CollisionSphere.Radius;
                RadiusDistance = Distance - Radius;
                Name = CleanName(obj.Name);
                var cacheObject = Trinity.ObjectCache.FirstOrDefault(o => o.RActorGuid == RActorGUID);
                if (cacheObject == null)
                {
                    Type = obj.ActorType.ToString();
                    InCache = "";
                    return;
                }
                InCache = "True";
                Type = cacheObject.Type.ToString();
                Weight = (int)cacheObject.Weight;
                IsBoss = BoolToString(cacheObject.IsBoss);
                IsElite = BoolToString(cacheObject.IsEliteRareUnique);
                WeightInfo = cacheObject.WeightInfo;

                string ignoreReason = "";
                CacheData.IgnoreReasons.TryGetValue(RActorGUID, out ignoreReason);
                IgnoreReason = ignoreReason;
                IsQuestMonster = BoolToString(cacheObject.IsQuestMonster);
                IsMinimapActive = BoolToString(cacheObject.IsMinimapActive);
            }
            catch (Exception ex)
            {
                WeightInfo = ex.Message;
            }
        }

        private string BoolToString(bool val)
        {
            if (val)
                return "True";
            return "";
        }

        private string CleanName(string name)
        {
            return name.Split('-')[0];
        }

        public bool Equals(CacheUIObject other)
        {
            if (other == null)
                return false;
            return GetHashCode() != other.GetHashCode();
        }

        public override int GetHashCode()
        {
            return RActorGUID.GetHashCode() ^ Name.GetHashCode() ^ Distance.GetHashCode() ^ InCache.GetHashCode();
        }
        public static bool operator ==(CacheUIObject a, CacheUIObject b)
        {
            // If both are null, or both are same instance, return true.
            if (ReferenceEquals(a, b))
            {
                return true;
            }
            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }
            return a.Equals(b);
        }
        public static bool operator !=(CacheUIObject a, CacheUIObject b)
        {
            return !(a == b);
        }
    }
}
