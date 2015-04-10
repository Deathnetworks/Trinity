using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Trinity.Objects;
using Trinity.Reference;
using Trinity.Technicals;
using Zeta.Game;
using Zeta.Game.Internals.Actors;
using Zeta.TreeSharp;
using Logger = Trinity.Technicals.Logger;

namespace Trinity.Helpers
{
    class DebugUtil
    {
        private static DateTime _lastCacheClear = DateTime.MinValue;

        private static Dictionary<string, DateTime> _seenAnimationCache = new Dictionary<string, DateTime>();
        private static Dictionary<int, DateTime> _seenUnknownCache = new Dictionary<int, DateTime>();


        public static void LogAnimation(TrinityCacheObject cacheObject)
        {
            if (!LogCategoryEnabled(LogCategory.Animation) || cacheObject.CommonData == null || !cacheObject.CommonData.IsValid || !cacheObject.CommonData.AnimationInfo.IsValid)
                return;

            var state = cacheObject.CommonData.AnimationState.ToString();
            var name = cacheObject.CommonData.CurrentAnimation.ToString();

            // Log Animation
            if (!_seenAnimationCache.ContainsKey(name))
            {
                Logger.Log(LogCategory.Animation, "{0} State={1} By: {2} ({3})", name, state, cacheObject.InternalName, cacheObject.ActorSNO);
                _seenAnimationCache.Add(name, DateTime.UtcNow);
            }

            CacheMaintenance();
        }

        internal static void LogUnknown(DiaObject diaObject)
        {
            if (!LogCategoryEnabled(LogCategory.UnknownObjects) || !diaObject.IsValid || !diaObject.CommonData.IsValid)
                return;

            // Log Object
            if (!_seenUnknownCache.ContainsKey(diaObject.ActorSNO))
            {
                Logger.Log(LogCategory.UnknownObjects, "{0} ({1}) Type={2}", diaObject.Name, diaObject.ActorSNO, diaObject.ActorType);
                _seenUnknownCache.Add(diaObject.ActorSNO, DateTime.UtcNow);
            }

            CacheMaintenance();
        }

        internal static void LogInFile(string file, string msg)
        {
            FileStream logStream = null;

            string filePath = Path.Combine(FileManager.LoggingPath, file + ".log");
            logStream = File.Open(filePath, FileMode.Append, FileAccess.Write, FileShare.Read);

            //TODO : Change File Log writing
            using (var logWriter = new StreamWriter(logStream))
            {
                logWriter.WriteLine(msg);
            }
        }

        private static void CacheMaintenance()
        {
            var age = DateTime.UtcNow.Subtract(TimeSpan.FromSeconds(15));
            if (DateTime.UtcNow.Subtract(_lastCacheClear) > TimeSpan.FromSeconds(15))
            {
                if (_seenAnimationCache.Any())
                    _seenAnimationCache = _seenAnimationCache.Where(p => p.Value < age).ToDictionary(p => p.Key, p => p.Value);

                if (_seenUnknownCache.Any())
                    _seenUnknownCache = _seenUnknownCache.Where(p => p.Value < age).ToDictionary(p => p.Key, p => p.Value);

            }
            _lastCacheClear = DateTime.UtcNow;
        }

        public static bool LogCategoryEnabled(LogCategory category)
        {
            return Trinity.Settings != null && Trinity.Settings.Advanced.LogCategories.HasFlag(category);
        }

        internal static void LogOnPulse()
        {
            if (CacheData.Buffs != null && CacheData.Buffs.ActiveBuffs != null)
                CacheData.Buffs.ActiveBuffs.ForEach(b => Logger.Log(LogCategory.ActiveBuffs, "Buff '{0}' is Active", b.InternalName));
        }

        public static void LogBuildAndItems(TrinityLogLevel level = TrinityLogLevel.Debug)
        {
            using (new MemoryHelper())
            {
                Action<Item, TrinityLogLevel> logItem = (i, l) =>
                {
                    Logger.Log(l, LogCategory.UserInformation, string.Format("Item: {0}: {1} ({2}) is Equipped", i.ItemType, i.Name, i.Id));
                };

                var actualEquipped = CacheData.Inventory.Equipped.Where(i => i.ItemQualityLevel == ItemQuality.Legendary).ToList();
                var referenceEquipped = Legendary.Equipped.Where(i => i.IsEquipped).ToList();
                if (actualEquipped.Count != referenceEquipped.Count)
                {
                    var missingItems = actualEquipped.Where(i => referenceEquipped.All(item => item.Id != i.ActorSNO));
                    Logger.Log(">> Warning - One or more of your equipped items is recorded incorrectly in Trinity; please report:");
                    Zeta.Common.Extensions.ForEach(missingItems, i => Logger.Log(">> {0} {1} ActorSNO={2} BaseType={3} ItemType={4}", i.InternalName, i.Name, i.ActorSNO, i.ItemBaseType, i.ItemType));
                }

                Logger.Log(level, LogCategory.UserInformation, "------ Equipped Legendaries: Items={0}, Sets={1} ------", Legendary.Equipped.Count, Sets.Equipped.Count);

                Zeta.Common.Extensions.ForEach(Legendary.Equipped.Where(c => !c.IsSetItem || !c.Set.IsEquipped), i => logItem(i, level));

                Sets.Equipped.ForEach(s =>
                {
                    Logger.Log(level, LogCategory.UserInformation, "------ Set: {0} {1}: {2}/{3} Equipped. ActiveBonuses={4}/{5} ------",
                        s.Name,
                        s.IsClassRestricted ? "(" + s.ClassRestriction + ")" : string.Empty,
                        s.EquippedItems.Count,
                        s.Items.Count,
                        s.CurrentBonuses,
                        s.MaxBonuses);

                    Zeta.Common.Extensions.ForEach(s.Items.Where(i => i.IsEquipped), i => logItem(i, level));
                });

                Logger.Log(level, LogCategory.UserInformation, "------ Active Skills / Runes ------", SkillUtils.Active.Count, SkillUtils.Active.Count);

                Action<Skill> logSkill = s =>
                {
                    Logger.Log(level, LogCategory.UserInformation, "Skill: {0} Rune={1} Type={2}",
                        s.Name,
                        s.CurrentRune.Name,
                        (s.Category == SpellCategory.Primary) ? "Primary" : "Spender"
                        );
                };

                SkillUtils.Active.ForEach(logSkill);

                Logger.Log(level, LogCategory.UserInformation, "------ Passives ------", SkillUtils.Active.Count, SkillUtils.Active.Count);

                Action<Passive> logPassive = p => Logger.Log(level, LogCategory.UserInformation, "Passive: {0}", p.Name);

                PassiveUtils.Active.ForEach(logPassive);

            }
        }


        public static void LogSystemInformation(TrinityLogLevel level = TrinityLogLevel.Debug)
        {
            Logger.Log(level, LogCategory.UserInformation, "------ System Information ------");
            Logger.Log(level, LogCategory.UserInformation, "Processor: " + SystemInformation.Processor);
            Logger.Log(level, LogCategory.UserInformation, "Current Speed: " + SystemInformation.ActualProcessorSpeed);
            Logger.Log(level, LogCategory.UserInformation, "Operating System: " + SystemInformation.OperatingSystem);
            Logger.Log(level, LogCategory.UserInformation, "Motherboard: " + SystemInformation.MotherBoard);
            Logger.Log(level, LogCategory.UserInformation, "System Type: " + SystemInformation.SystemType);
            Logger.Log(level, LogCategory.UserInformation, "Free Physical Memory: " + SystemInformation.FreeMemory);
            Logger.Log(level, LogCategory.UserInformation, "Hard Drive: " + SystemInformation.HardDisk);
            Logger.Log(level, LogCategory.UserInformation, "Video Card: " + SystemInformation.VideoCard);
            Logger.Log(level, LogCategory.UserInformation, "Resolution: " + SystemInformation.Resolution);
        }


        internal static void DumpReferenceItems(TrinityLogLevel level = TrinityLogLevel.Debug)
        {

            var path = Path.Combine(FileManager.DemonBuddyPath, "Resources\\JS Class Generator\\ItemReference.js");

            if(File.Exists(path))
                File.Delete(path);

            using (StreamWriter w = File.AppendText(path))
            {
                w.WriteLine("var itemLookup = {");

                foreach (var item in Legendary.ToList())
                {
                    if(item.Id!=0)
                        w.WriteLine(string.Format("     \"{0}\": [\"{1}\", {2}, \"{3}\"],",item.Slug, item.Name, item.Id, item.InternalName));
                }

                w.WriteLine("}");                
            }

            Logger.Log("Dumped Reference Items to: {0}", path);
        }

        internal static void LogInvalidItems(TrinityLogLevel level = TrinityLogLevel.Debug)
        {
            var p = Logger.Prefix;
            Logger.Prefix = "";

            var dropItems = Legendary.ToList().Where(i => !i.IsCrafted && i.Id==0).OrderBy(i => i.GItemType).ToList();
            var craftedItems = Legendary.ToList().Where(i => i.IsCrafted && i.Id==0).OrderBy(i => i.GItemType).ToList();

            Logger.Log("Dropped Items: {0}", dropItems.Count);
            foreach (var item in dropItems)
            {
                    Logger.Log("{0} - {1} = 0", item.GItemType, item.Name);                    
            }

            Logger.Log(" ");
            Logger.Log("Crafted Items: {0}", craftedItems.Count);
            foreach (var item in craftedItems)
            {
                    Logger.Log("{0} - {1} = 0", item.GItemType, item.Name);
            }

            Logger.Prefix = p;
        }

        internal static void LogNewItems()
        {
            var knownIds = Legendary.ItemIds;

            using (new MemoryHelper())
            {
                if (ZetaDia.Me == null || !ZetaDia.Me.IsValid)
                {
                    Logger.Log("Not in game, can't check right now.");
                    return;
                }

                Logger.Log("Checking items Stashed/Backpacked/Equipped:");

                var allItems = new List<ACDItem>();
                allItems.AddRange(ZetaDia.Me.Inventory.StashItems);
                allItems.AddRange(ZetaDia.Me.Inventory.Equipped);
                allItems.AddRange(ZetaDia.Me.Inventory.Backpack);

                if (!allItems.Any())
                {
                    Logger.Log("No new items found.");
                    return;
                }
                    
                var newItems = allItems.Where(i => i != null && i.IsValid && i.ItemQualityLevel == ItemQuality.Legendary && (i.ItemBaseType == ItemBaseType.Jewelry || i.ItemBaseType == ItemBaseType.Armor || i.ItemBaseType == ItemBaseType.Weapon) && !knownIds.Contains(i.ActorSNO)).DistinctBy(p => p.ActorSNO).OrderBy(i => i.ItemType).ToList();

                newItems.ForEach(i =>
                {
                    Logger.Log(string.Format("Item: {0}: {1} ({2})", i.ItemType, i.Name, i.ActorSNO));
                });                
            }        
        }


    }
}
