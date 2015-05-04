using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using Zeta.Game;
using Zeta.Game.Internals.Actors;
using Logger = Trinity.Technicals.Logger;

namespace Trinity.LazyCache
{
    public static class CacheUtilities
    {
        public static class FastConstructor
        {
            //http://rogeralsing.com/2008/02/28/linq-expressions-creating-objects/

            public delegate T ObjectActivator<T>(params object[] args);

            public static ObjectActivator<T> GetActivator<T> (ConstructorInfo ctor)
            {
                Type type = ctor.DeclaringType;
                ParameterInfo[] paramsInfo = ctor.GetParameters();

                //create a single param of type object[]
                ParameterExpression param = Expression.Parameter(typeof(object[]), "args");

                Expression[] argsExp = new Expression[paramsInfo.Length];

                //pick each arg from the params array 
                //and create a typed expression of them
                for (int i = 0; i < paramsInfo.Length; i++)
                {
                    Expression index = Expression.Constant(i);
                    Type paramType = paramsInfo[i].ParameterType;

                    Expression paramAccessorExp =
                        Expression.ArrayIndex(param, index);

                    Expression paramCastExp =
                        Expression.Convert(paramAccessorExp, paramType);

                    argsExp[i] = paramCastExp;
                }

                //make a NewExpression that calls the
                //ctor with the args we just created
                NewExpression newExp = Expression.New(ctor, argsExp);

                //create a lambda with the New
                //Expression as body and our param object[] as arg
                LambdaExpression lambda =
                    Expression.Lambda(typeof(ObjectActivator<T>), newExp, param);

                //compile it
                ObjectActivator<T> compiled = (ObjectActivator<T>)lambda.Compile();
                return compiled;
            }

        }

        /// <summary>
        /// This is supposed to be considerably faster than Activator.CreateInstance
        /// </summary>
        public static T New<T>(params object[] args)
        {
            var ctor = typeof(T).GetConstructors().First();
            var activator = FastConstructor.GetActivator<T>(ctor);
            return activator(args);                
        }

        public static bool IsProperValid(this ACD acd)
        {
            return acd != null && acd.IsValid && !acd.IsDisposed && acd.ACDGuid != -1;
        }

        /// <summary>
        /// Fetches value from Dictionary or adds and returns a default value.
        /// </summary>
        internal static TV GetOrCreateValue<TK, TV>(this Dictionary<TK, TV> dictionary, TK key, TV newValue = default(TV)) 
        {
            if (key == null)
                throw new ArgumentNullException("key");

            TV foundValue;
            if (dictionary.TryGetValue(key, out foundValue))
                return foundValue;

            if (newValue == null)
                newValue = New<TV>(); //(TV)Activator.CreateInstance(typeof(TV));

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
        /// IDisposable for ForceRefresh
        /// </summary>
        public class ForceRefreshHelper : IDisposable
        {
            public ForceRefreshHelper()
            {
                ++CacheManager.ForceRefreshLevel;
            }

            public void Dispose()
            {
                --CacheManager.ForceRefreshLevel;
                GC.SuppressFinalize(this);
            }
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
                if (ex.Message.StartsWith("Only part of a ReadProcessMemory"))
                {
                    Logger.LogError("DB Memory Exception in GetAttributeOrDefault Caller={0} ACDGuid={1} InternalName={2} ActorType={3} SNO={4} Exception={5} {6}",
                        "", actor.ACDGuid, actor.Name, actor.ActorType, actor.ActorSNO, ex.Message, ex.InnerException);
                }
                else throw;  
            }
            return Default<T>();
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
            return Default<T>();
        }


        internal static Regex NameNumberTrimRegex = new Regex(@"-\d+$", RegexOptions.Compiled);


        /// <summary>
        /// Customized default value (modifies string and D3 generated enums without a proper 0 index default)
        /// </summary>
        public static T Default<T>()
        {
            var value = default(T);
            return typeof(T) == typeof(string) ? (T)(object)String.Empty : value;
        }
    }

}
