using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Trinity.Technicals;
using Zeta.Game.Internals.Actors;

namespace Trinity.Helpers
{
    class DebugUtil
    {

        private static DateTime _lastCacheClear = DateTime.MinValue;    

        private static Dictionary<string, DateTime> _seenAnimationCache = new Dictionary<string, DateTime>();
        private static Dictionary<string, DateTime> _seenUnknownCache = new Dictionary<string, DateTime>();
    

        public static void LogAnimation(TrinityCacheObject cacheObject)
        {
            if (!LogCategoryEnabled(LogCategory.Animation) || !cacheObject.CommonData.IsValid || !cacheObject.CommonData.AnimationInfo.IsValid)
                return;

            var state = cacheObject.CommonData.AnimationState.ToString();
            var name = cacheObject.CommonData.CurrentAnimation.ToString();

            // Log Animation
            if (!_seenAnimationCache.ContainsKey(name))
            {
                Logger.Log(LogCategory.Animation, "{0} State={1} By: {2} ({3})", name, state, cacheObject.InternalName, cacheObject.ActorSNO);
                _seenAnimationCache.Add(name,DateTime.UtcNow);
            }

            CacheMaintenance();          
        }

        internal static void LogUnknown(DiaObject diaObject)
        {
            if (!LogCategoryEnabled(LogCategory.UnknownObjects) || !diaObject.IsValid || !diaObject.CommonData.IsValid)
                return;

            // Log Object
            if (!_seenUnknownCache.ContainsKey(diaObject.Name))
            {
                Logger.Log(LogCategory.UnknownObjects, "{0} ({1})", diaObject.Name, diaObject.ActorSNO);
                _seenUnknownCache.Add(diaObject.Name, DateTime.UtcNow);
            }

            CacheMaintenance();
        }

        private static void CacheMaintenance()
        {
            var age = DateTime.UtcNow.Subtract(TimeSpan.FromSeconds(15));
            if (DateTime.UtcNow.Subtract(_lastCacheClear) > TimeSpan.FromSeconds(60))
            {
                if(_seenAnimationCache.Any())
                    _seenAnimationCache = _seenUnknownCache.Where(p => p.Value > age).ToDictionary(p => p.Key, p => p.Value);

                if(_seenUnknownCache.Any())
                    _seenUnknownCache = _seenUnknownCache.Where(p => p.Value > age).ToDictionary(p => p.Key, p => p.Value);
                
            }   
            _lastCacheClear = DateTime.UtcNow;
        }

        public static bool LogCategoryEnabled(LogCategory category)
        {
            return Trinity.Settings != null && Trinity.Settings.Advanced.LogCategories.HasFlag(category);
        }
    }
}
