using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Trinity.Technicals;

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
        public static List<TItem> ToList()
        {
            return _list ?? (_list = ToEnumerable().ToList());
        }

        private static IEnumerable<TItem> _enumerable;
        public static IEnumerable<TItem> ToEnumerable()
        {
            return _enumerable ?? (_enumerable = typeof(TBase).GetFields(BindingFlags.Public | BindingFlags.Static).Select(f => f.GetValue(null)).OfType<TItem>());
        }

        public static IEnumerable<TItem> Where(Func<TItem, bool> predicate)
        {
            return ToList().Where(predicate);
        }

        public static bool Any(Func<TItem, bool> predicate)
        {
            return ToList().Any(predicate);
        }
    }
    
}
