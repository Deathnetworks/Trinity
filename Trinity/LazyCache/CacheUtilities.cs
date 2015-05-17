using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.SessionState;
using System.Windows.Navigation;
using Org.BouncyCastle.Asn1.X509.Qualified;
using Zeta.Common;
using Zeta.Game;
using Zeta.Game.Internals;
using Zeta.Game.Internals.Actors;
using Zeta.Game.Internals.SNO;
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
            var type = typeof (T);
            var argTypes = args.Select(arg => arg.GetType()).ToArray();
            var ctor = type.GetConstructors().FirstOrDefault();
            
            if(ctor==null)
                ctor = type.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, argTypes, null);

            var activator = FastConstructor.GetActivator<T>(ctor);
            return activator(args);                
        }

        #region Zeta API

        private static readonly Type ZetaDiaType = typeof (ZetaDia);

        private static Func<ACDManager> _getActorCommonData;

        public static ACDManager GetActorCommonData()
        {
            if (_getActorCommonData == null)
            {
                _getActorCommonData = GetStaticAccessor<ACDManager>(ZetaDiaType, "ActorCommonData");
            }
            return _getActorCommonData();
        }

        private static Func<RActorManager> _getRActors;

        public static RActorManager GetRActors()
        {
            if (_getRActors == null)
            {
                _getRActors = GetStaticAccessor<RActorManager>(ZetaDiaType, "RActors");
            }
            return _getRActors();
        }

        static readonly Dictionary<Type, Func<int, IntPtr>> GetRecordPtrMethods = new Dictionary<Type, Func<int, IntPtr>>();

        /// <summary>
        /// Call private method GetRecordPtr on SNOTable instance
        /// GetRecordPtr() finds a record pointer in a table for given value
        /// e.g. var testPtr = ZetaDia.SNO[ClientSNOTable.ActorInfo].GetRecordPtr(ZetaDia.Me.ActorSNO);
        /// </summary>
        public static IntPtr GetRecordPtr(this SNOTable table, int id)
        {
            var type = typeof(SNOTable);
            Func<int, IntPtr> expr;

            if (!GetRecordPtrMethods.TryGetValue(type, out expr))
            {
                // Get all delcared private methods
                var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);

                // GetRecordPtr is obfusticated with no name so find a method with the right pattern of args
                var method = methods.FirstOrDefault(m => m.ReturnType == typeof(IntPtr));

                if (method == null)
                    throw new NullReferenceException("GetRecordPtr MethodInfo cannot be null");

                // Define that expression will take an Int argument
                var parameterExpr = Expression.Parameter(typeof(int), "input");

                // Define instance that MethodInfo will be executed against.
                var instanceExpr = Expression.Constant(table);

                // Formalize instance, method and arguments.
                var methodCallExpr = Expression.Call(instanceExpr, method, parameterExpr);

                expr = Expression.Lambda<Func<int, IntPtr>>(methodCallExpr, parameterExpr).Compile();

                GetRecordPtrMethods.Add(type, expr);
            }

            return expr != null ? expr(id) : new IntPtr(-1);
        }

        static readonly Dictionary<Type, Action<IntPtr>> PurgeSNORecordPtrMethods = new Dictionary<Type, Action<IntPtr>>();

        /// <summary>
        /// Call private method GetRecordPtr on SNOTable instance.
        /// Apparently bad things ensue if you don't purge the record after using it
        /// </summary>
        public static void PurgeRecordPtr(this SNOTable table, IntPtr ptr)
        {
            var type = typeof(SNOTable);
            Action<IntPtr> expr;

            if (!PurgeSNORecordPtrMethods.TryGetValue(type, out expr))
            {
                // Get all delcared private methods
                var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);

                // PurgeSNORecord is obfusticated with no name so find a method with the right pattern of args
                var method = methods.FirstOrDefault(m => m.ReturnType == typeof(void));

                if (method == null)
                    throw new NullReferenceException("GetRecordPtr MethodInfo cannot be null");

                // Define that expression will take an Int argument
                var parameterExpr = Expression.Parameter(typeof(IntPtr), "input");

                // Define instance that MethodInfo will be executed against.
                var instanceExpr = Expression.Constant(table);

                // Formalize instance, method and arguments.
                var methodCallExpr = Expression.Call(instanceExpr, method, parameterExpr);

                expr = Expression.Lambda<Action<IntPtr>>(methodCallExpr, parameterExpr).Compile();

                PurgeSNORecordPtrMethods.Add(type, expr);
            }

            if (expr != null)
                expr(ptr);
        }

        public static IntPtr GetMonsterInfoPointer(int monsterSNO)
        {
            return GetRecordPtr((SNOTable) MonsterSNOTable, monsterSNO);
        }

        public static IntPtr GetActorInfoPointer(int actorSNO)
        {
            return GetRecordPtr((SNOTable) ActorSNOTable, actorSNO);
        }

        internal static SNOTable ActorSNOTable
        {
            get { return ZetaDia.SNO[ClientSNOTable.Actor]; }
        }

        internal static SNOTable MonsterSNOTable
        {
            get { return ZetaDia.SNO[ClientSNOTable.Monster]; }
        }

        #endregion        

        #region Reflection Utiltiies

        public static Type GetEnumerableType(Type type)
        {
            foreach (Type intType in type.GetInterfaces())
            {
                if (intType.IsGenericType && intType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                {
                    return intType.GetGenericArguments()[0];
                }
            }
            return null;
        }

        /// <summary>
        /// Gets a static field or property
        /// </summary>
        /// <typeparam name="T">the return type of the member</typeparam>
        /// <param name="containingClassType">type of the containing class</param>
        /// <param name="memberName">the name of the member</param>
        /// <returns>function to access the member</returns>
        public static Func<T> GetStaticAccessor<T>(Type containingClassType, string memberName)
        {
            var param = Expression.Parameter(containingClassType, "arg");
            var member = StaticPropertyOrField(containingClassType, memberName);
            var lambda = Expression.Lambda(member);
            return (Func<T>)lambda.Compile();
        }

        /// <summary>
        /// Gets an instanced field or property
        /// </summary>
        /// <typeparam name="T">class containing the member</typeparam>
        /// <typeparam name="TR">the return type of the member</typeparam>
        /// <param name="memberName">the name of the member</param>
        /// <returns>function to access the member</returns>
        public static Func<T, TR> GetInstanceAccessor<T, TR>(string memberName)
        {
            var type = typeof(T);
            var param = Expression.Parameter(type, "arg");
            var member = Expression.PropertyOrField(param, memberName);
            var lambda = Expression.Lambda(typeof(Func<T, TR>), member, param);
            return (Func<T, TR>)lambda.Compile();
        }

        /// <summary>
        /// Create a MemberExpression for a static Property or Field
        /// </summary>
        public static MemberExpression StaticPropertyOrField(Type type, string propertyOrFieldName)
        {
            if(type == null)
                throw new ArgumentNullException("type");

            PropertyInfo property = type.GetProperty(propertyOrFieldName, BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.Static);
            if (property != null)
            {
                return Expression.Property(null, property);
            }
            FieldInfo field = type.GetField(propertyOrFieldName, BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.Static);
            if (field == null)
            {
                property = type.GetProperty(propertyOrFieldName, BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.IgnoreCase | BindingFlags.Static);
                if (property != null)
                {
                    return Expression.Property(null, property);
                }
                field = type.GetField(propertyOrFieldName, BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.IgnoreCase | BindingFlags.Static);
                if (field == null)
                {
                    throw new ArgumentException(String.Format("{0} NotAMemberOfType {1}", propertyOrFieldName, type));
                }
            }
            return Expression.Field(null, field);
        }

        #endregion

        #region Collection Utilities

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

        #endregion

        public static bool IsProbablyValid(this ACD acd)
        {
            return acd != null && acd.IsValid;
        }

        public static bool IsProbablyValid(this SNORecord acd)
        {
            return acd != null && acd.IsValid && !acd.IsDisposed;
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
                    //Logger.LogError("DB Memory Exception in GetAttributeOrDefault Caller={0} ACDGuid={1} InternalName={2} ActorType={3} SNO={4} Exception={5} {6}",
                    //    "", actor.ACDGuid, actor.Name, actor.ActorType, actor.ActorSNO, ex.Message, ex.InnerException);
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
                    //Logger.LogError("ReadMemoryValueOrDefault Offset={0} Exception={1} {2}", offset, ex.Message, ex.InnerException);
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
            var type = typeof(T).Name;

            switch (type)
            {
                case "string":
                    return (T)(object)String.Empty;

                case "MonsterType":
                    return (T)(object)MonsterType.None;

                case "InventorySlot":
                    return (T)(object)InventorySlot.None;
            }

            return default(T);
        }

        internal static void LogTime(Stopwatch sw, [CallerMemberName] string member = "")
        {
            Logger.Log("{0} took {1:00.00000}ms.", member, sw.Elapsed.TotalMilliseconds);
        }

        static readonly Dictionary<Type, List<FieldInfo>> ReflectedFields = new Dictionary<Type, List<FieldInfo>>();

        internal static List<FieldInfo> GetFields<T>()
        {
            var type = typeof (T);
            List<FieldInfo> fields;
            if (!ReflectedFields.TryGetValue(type, out fields))
            {
                fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).ToList();
                ReflectedFields.Add(type,fields);
            }
            return fields;
        }

        /// <summary>
        /// Set all CacheField in object IsFrozen to True
        /// </summary>
        internal static void Freeze<T>(T cacheBase)
        {
            foreach (var field in GetFields<T>())
            {
                if (field.FieldType.IsConstructedGenericType && field.FieldType.GetGenericTypeDefinition() == typeof(CacheField<>))
                {
                    var val = field.GetValue(cacheBase);

                    if (val is IFreezable)
                        (val as IFreezable).IsFrozen = true;                    
                }
            }
        }

        /// <summary>
        /// Set all CacheField in object IsFrozen to False
        /// </summary>
        internal static void UnFreeze<T>(T cacheBase)
        {
            foreach (var field in GetFields<T>())
            {
                if (field.FieldType.IsConstructedGenericType && field.FieldType.GetGenericTypeDefinition() == typeof(CacheField<>))
                {
                    var val = field.GetValue(cacheBase);

                    if (val is IFreezable)
                        (val as IFreezable).IsFrozen = false;
                }
            }
        }

        static LambdaExpression CreateLambda(Type type, string methodName)
        {
            var source = Expression.Parameter(
                typeof(IEnumerable<>).MakeGenericType(type), "source");

            var call = Expression.Call(
                typeof(Enumerable), methodName, new Type[] { type }, source);

            return Expression.Lambda(call, source);
        }

        static Expression<Func<IEnumerable<T>, T>> CreateLambda<T>(string methodName)
        {
            var source = Expression.Parameter(
                typeof(IEnumerable<T>), "source");

            var call = Expression.Call(
                typeof(Enumerable), methodName, new Type[] { typeof(T) }, source);

            return Expression.Lambda<Func<IEnumerable<T>, T>>(call, source);
        }

        static bool IsSubclassOfRawGeneric(Type generic, Type toCheck)
        {
            while (toCheck != null && toCheck != typeof(object))
            {
                var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (generic == cur)
                {
                    return true;
                }
                toCheck = toCheck.BaseType;
            }
            return false;
        }

        public static string CSharpName(this Type type)
        {
            var sb = new StringBuilder();
            var name = type.Name;
            if (!type.IsGenericType) return name;
            sb.Append(name.Substring(0, name.IndexOf('`')));
            sb.Append("<");
            sb.Append(string.Join(", ", type.GetGenericArguments()
                                            .Select(t => t.CSharpName())));
            sb.Append(">");
            return sb.ToString();
        }

    }

 
}
