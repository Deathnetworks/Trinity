using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Zeta.Game.Internals.Actors;
using Logger = Trinity.Technicals.Logger;

namespace Trinity.LazyCache
{
    public static class UtilityExtensions
    {
        public static bool IsProperValid(this ACD acd)
        {
            return acd != null && acd.IsValid && !acd.IsDisposed;
        }

        /// <summary>
        /// Fetches value from Dictionary or adds and returns a default value.
        /// </summary>
        internal static TV GetOrCreateValue<TK, TV>(this Dictionary<TK, TV> dictionary, TK key, TV newValue = default(TV)) where TV : new()
        {
            if (key == null)
                throw new ArgumentNullException("key");

            TV foundValue;
            if (dictionary.TryGetValue(key, out foundValue))
                return foundValue;

            if (newValue == null)
                newValue = (TV)Activator.CreateInstance(typeof(TV));

            dictionary.Add(key, newValue);
            return newValue;
        }

        /// <summary>
        /// Removed duplicates from a list based on specified property .DistinctBy(o => o.property)
        /// </summary>
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>
            (this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            var seenKeys = new HashSet<TKey>();
            return source.Where(element => seenKeys.Add(keySelector(element)));
        }

        /// <summary>
        /// Attempt to remove Key/Value object from ConcurrentDictionary
        /// </summary>
        public static bool TryRemove<TKey, TValue>(
            this ConcurrentDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if (dictionary == null) throw new ArgumentNullException("dictionary");
            return ((ICollection<KeyValuePair<TKey, TValue>>)dictionary).Remove(
                new KeyValuePair<TKey, TValue>(key, value));
        }

        /// <summary>
        /// Get an attribute, ReadProcessMemory exceptions get swallowed and default returned
        /// </summary>
        public static T GetAttributeOrDefault<T>(this ACD actor, ActorAttributeType type) where T : struct
        {
            try
            {
                actor.GetAttribute<T>(type);
            }
            catch (Exception ex)
            {
                if (!ex.Message.StartsWith("Only part of a ReadProcessMemory"))
                {
                    Logger.LogError("GetAttributeException Type={0} Exception={1} {2}", type, ex.Message, ex.InnerException);
                }
                else throw;
            }
            return default(T);
        }

    }
}
