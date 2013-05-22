using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Zeta;

namespace Trinity.Technicals
{
    /// <summary>
    /// Manage File Access and Path.
    /// </summary>
    internal static class FileManager
    {
        /// <summary>
        /// Loads the specified filename to HashSet.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">The name.</param>
        /// <param name="valueName">Name of the value.</param>
        /// <returns></returns>
        public static HashSet<T> Load<T>(string name, string valueName)
        {
            return Load<T>(Path.Combine(FileManager.PluginPath, "Dictionaries.xml"), name, valueName);
        }

        /// <summary>
        /// Loads the specified filename to HashSet.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filename">The filename.</param>
        /// <param name="name">The name.</param>
        /// <param name="valueName">Name of the value.</param>
        /// <returns></returns>
        private static HashSet<T> Load<T>(string filename, string name, string valueName)
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
                                   select typeof(T).IsEnum ? (T)Enum.Parse(typeof(T), e.Attribute(valueName).Value, true) : (T)Convert.ChangeType(e.Attribute(valueName).Value, typeof(T), CultureInfo.InvariantCulture)
                                   ).ToList();
                    foreach (T item in lst)
                    {
                        ret.Add(item);
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// Loads the specified filename to IDictionary.
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">The name.</param>
        /// <param name="keyName">Name of the key.</param>
        /// <param name="valueName">Name of the value.</param>
        /// <returns></returns>
        public static IDictionary<K, T> Load<K, T>(string name, string keyName, string valueName)
        {
            return Load<K, T>(Path.Combine(FileManager.PluginPath, "Dictionaries.xml"), name, keyName, valueName);
        }

        /// <summary>
        /// Loads the specified filename to IDictionary.
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="filename">The filename.</param>
        /// <param name="name">The name.</param>
        /// <param name="keyName">Name of the key.</param>
        /// <param name="valueName">Name of the value.</param>
        /// <returns></returns>
        private static IDictionary<K, T> Load<K, T>(string filename, string name, string keyName, string valueName)
        {
            DbHelper.Log(TrinityLogLevel.Normal, LogCategory.Configuration, "Loading Dictionary file={0} name={1} keys={2} values={3}",filename, name, keyName, valueName);
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
                                                        typeof(K).IsEnum ? (K)Enum.Parse(typeof(K), e.Attribute(keyName).Value, true) : (K)Convert.ChangeType(e.Attribute(keyName).Value, typeof(K), CultureInfo.InvariantCulture),
                                                        typeof(T).IsEnum ? (T)Enum.Parse(typeof(T), e.Attribute(valueName).Value, true) : (T)Convert.ChangeType(e.Attribute(valueName).Value, typeof(T), CultureInfo.InvariantCulture))
                                                    ).ToList();
                    
                    foreach (KeyValuePair<K, T> item in lst)
                    {
                        DbHelper.Log(TrinityLogLevel.Debug, LogCategory.Configuration, "Found dictionary item {0} = {1}", item.Key, item.Value);
                        ret.Add(item);
                    }
                }
            }
            if (ret.Count > 0)
            {
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.Configuration, "Loaded Dictionary name={0} key={1} value={2} with {3} values", name, keyName, valueName, ret.Count);
            }
            else
            {
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.Configuration, "Attempted to load Dictionary name={0} key={1} value={2} but 0 values found!", name, keyName, valueName, ret.Count);
            }
            return ret;
        }

        /// <summary>
        /// Gets the DemonBuddy path.
        /// </summary>
        /// <value>The demon buddy path.</value>
        public static string DemonBuddyPath
        {
            get
            {
                return Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            }
        }

        /// <summary>
        /// Gets the plugin path.
        /// </summary>
        /// <value>The plugin path.</value>
        public static string PluginPath
        {
            get
            {
                return Path.Combine(DemonBuddyPath, "Plugins", "Trinity");
            }
        }

        /// <summary>
        /// Gets the settings path.
        /// </summary>
        /// <value>The settings path.</value>
        public static string SettingsPath
        {
            get
            {
                return Path.Combine(DemonBuddyPath, "Settings");
            }
        }

        /// <summary>
        /// Gets the settings path specific to current hero.
        /// </summary>
        /// <value>The specific settings path.</value>
        public static string SpecificSettingsPath
        {
            get
            {
                string path = Path.Combine(DemonBuddyPath, "Settings", ZetaDia.Service.CurrentHero.BattleTagName);
                CreateDirectory(path);
                return path;
            }
        }

        /// <summary>
        /// Gets the logging path for battletag specific logging
        /// </summary>
        /// <value>The logging path.</value>
        public static string LoggingPath
        {
            get
            {
                string path = Path.Combine(DemonBuddyPath, "TrinityLogs", ZetaDia.Service.CurrentHero.BattleTagName);
                CreateDirectory(path);
                return path;
            }
        }

        /// <summary>
        /// Gets the TrinityLogs path - for NON-battletag specific logging
        /// </summary>
        public static string TrinityLogsPath
        {
            get
            {
                string path = Path.Combine(DemonBuddyPath, "TrinityLogs");
                CreateDirectory(path);
                return path;
            }
        }

        /// <summary>
        /// Gets the scripted item rules path.
        /// </summary>
        /// <value>The item rule path.</value>
        public static string ItemRulePath
        {
            get
            {
                return Path.Combine(PluginPath, "ItemRules");
            }
        }

        /// <summary>
        /// Gets the scripted item rules path specific to current hero.
        /// </summary>
        /// <value>The item rule path.</value>
        public static string SpecificItemRulePath
        {
            get
            {
                return Path.Combine(DemonBuddyPath, "ItemRules", ZetaDia.Service.CurrentHero.BattleTagName);
            }
        }

        /// <summary>
        /// Creates the directory structure.
        /// </summary>
        /// <param name="path">The path.</param>
        private static void CreateDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                if (!Directory.Exists(Path.GetDirectoryName(path)))
                {
                    CreateDirectory(Path.GetDirectoryName(path));
                }
                Directory.CreateDirectory(path);
            }
        }
    }
}
