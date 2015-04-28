using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Zeta.Game;
using Zeta.Game.Internals.Actors;
using Zeta.Game.Internals.SNO;
using Logger = Trinity.Technicals.Logger;

namespace Trinity.LazyCache
{
    public static class CacheUtilities
    {
        public static bool IsProperValid(this ACD acd)
        {
            return acd != null && acd.IsValid && !acd.IsDisposed && acd.ACDGuid != -1;
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

        /// <summary>
        /// Get an attribute, ReadProcessMemory exceptions get swallowed and default returned
        /// </summary>
        public static T ReadMemoryOrDefault<T>(this ACD actor, int offset) where T : struct
        {
            try
            {
                return ZetaDia.Memory.Read<T>(actor.BaseAddress + 0xB8);
            }
            catch (Exception ex)
            {
                if (!ex.Message.StartsWith("Only part of a ReadProcessMemory"))
                {
                    Logger.LogError("ReadMemoryValueOrDefault Offset={0} Exception={1} {2}", offset, ex.Message, ex.InnerException);
                }
                else throw;
            }
            return default(T);
        }        

        /// <summary>
        /// Used for trimming off numbers from object names in RefreshDiaObject
        /// </summary>
        internal static Regex NameNumberTrimRegex = new Regex(@"-\d+$", RegexOptions.Compiled);

        /// <summary>
        /// Generates an SHA1 hash of a particular CacheObject
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string GenerateObjectHash(TrinityObject obj)
        {
            using (MD5 md5 = MD5.Create())
            {
                string objHashBase;
                if (obj.Type == TrinityObjectType.Unit)
                    objHashBase = obj.ActorSNO + obj.InternalName + obj.Position + obj.Type + Trinity.CurrentWorldDynamicId;
                else if (obj.Type == TrinityObjectType.Item && obj is TrinityItem)
                {
                    var objItem = (TrinityItem) obj;
                    return HashGenerator.GenerateItemHash(obj.Position, obj.ActorSNO, obj.InternalName, Trinity.CurrentWorldId, objItem.ItemQuality, objItem.ItemLevel);
                }
                else
                    objHashBase = String.Format("{0}{1}{2}{3}", obj.ActorSNO, obj.Position, obj.Type, Trinity.CurrentWorldDynamicId);

                string objHash = HashGenerator.GetMd5Hash(md5, objHashBase);
                return objHash;
            }
        }

        internal static TrinityObjectType GetTrinityObjectType(this TrinityObject trinityObject)
        {
            if (trinityObject.AvoidanceType != AvoidanceType.None)
                return TrinityObjectType.Avoidance;

            if (trinityObject.GizmoType != GizmoType.None)
                return TrinityObjectType.Interactable;            

            if (trinityObject.IsUnit)
                return TrinityObjectType.Unit;

            return TrinityObjectType.Unknown;
        }
    }
}
