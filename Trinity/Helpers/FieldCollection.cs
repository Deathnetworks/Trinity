using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Trinity.Helpers
{
    /// <summary>
    /// Exposes compile-time value of fields as collection.    
    /// </summary>
    /// <typeparam name="TBase">Class to Build enumeration of TValue properties</typeparam>
    /// <typeparam name="TItem">Type of Fields to include</typeparam>
    public class FieldCollection<TBase, TItem>
    {
        private static List<TItem> _list;
        public static List<TItem> ToList(bool clone = false)
        {
            return _list ?? (_list = ToEnumerable(clone).ToList());
        }

        private static IEnumerable<TItem> _enumerable;
        public static IEnumerable<TItem> ToEnumerable(bool clone = false)
        {
            var source = _enumerable ?? (_enumerable = typeof(TBase).GetFields(BindingFlags.Public | BindingFlags.Static).Select(f => f.GetValue(null)).OfType<TItem>());
            
            if (clone && IsCloneableType(typeof (TItem)))
            {
                return source.Select(o => (TItem)((ICloneable)o).Clone());
            }

            return source;
        }

        public static IEnumerable<TItem> Where(Func<TItem, bool> predicate)
        {
            return ToList().Where(predicate);
        }

        public static bool Any(Func<TItem, bool> predicate)
        {
            return ToList().Any(predicate);
        }

        public static bool IsCloneableType(Type type)
        {
            if (typeof(ICloneable).IsAssignableFrom(type))
                return true;

            return type.IsValueType;
        }
    }
    
}
