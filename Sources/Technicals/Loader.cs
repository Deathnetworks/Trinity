using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace GilesTrinity.Technicals
{
    internal static class Loader
    {
        /// <summary>Loads the specified filename to HashSet.</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filename">The filename.</param>
        /// <param name="name">The name.</param>
        /// <param name="valueName">Name of the value.</param>
        /// <returns></returns>
        public static HashSet<T> Load<T>(string filename, string name, string valueName)
        {
            HashSet<T> ret = new HashSet<T>();
            if (File.Exists(filename))
            {
                XElement xElem = XElement.Load(filename);
                xElem = xElem.Descendants("HashSet").FirstOrDefault(elem => elem.Attribute("Name").Value == name);
                if (xElem != null)
                {
                    List<T> lst = (from e in xElem.Descendants("Entry")
                                   where e.Attribute(valueName) != null && e.Attribute(valueName).Value != null
                                   select (T)Convert.ChangeType(e.Attribute(valueName).Value, typeof(T), CultureInfo.InvariantCulture)).ToList();
                    foreach (T item in lst)
                    {
                        ret.Add(item);
                    }
                }
            }
            return ret;
        }

        /// <summary>Loads the specified filename to IDictionary.</summary>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="filename">The filename.</param>
        /// <param name="name">The name.</param>
        /// <param name="keyName">Name of the key.</param>
        /// <param name="valueName">Name of the value.</param>
        /// <returns></returns>
        public static IDictionary<K, T> Load<K, T>(string filename, string name, string keyName, string valueName)
        {
            IDictionary<K, T> ret = new Dictionary<K, T>();
            if (File.Exists(filename))
            {
                XElement xElem = XElement.Load(filename);
                xElem = xElem.Descendants("Dictionary").FirstOrDefault(elem => elem.Attribute("Name").Value == name);
                if (xElem != null)
                {
                    List<KeyValuePair<K, T>> lst = (from e in xElem.Descendants("Entry")
                                                    where e.Attribute(keyName) != null && e.Attribute(keyName).Value != null
                                                    where e.Attribute(valueName) != null && e.Attribute(valueName).Value != null
                                                    select new KeyValuePair<K, T>(
                                                        (K)Convert.ChangeType(e.Attribute(keyName).Value, typeof(K), CultureInfo.InvariantCulture),
                                                        (T)Convert.ChangeType(e.Attribute(valueName).Value, typeof(T), CultureInfo.InvariantCulture))
                                   ).ToList();
                    foreach (KeyValuePair<K, T> item in lst)
                    {
                        ret.Add(item);
                    }
                }
            }
            return ret;
        }
    }
}
