﻿#region Usings

using System;
using System.Collections.Generic;
using System.Linq;
using Trinity.Helpers;
using Trinity.Technicals;
using Zeta.Game;
using Zeta.Game.Internals;
using Zeta.Game.Internals.Actors;

#endregion

namespace Trinity.LazyCache
{
    public class CacheBuffs
    {
        private static readonly List<TrinityBuff> _buffs = new List<TrinityBuff>();
        private static readonly Dictionary<int, TrinityBuff> _buffsById = new Dictionary<int, TrinityBuff>();
        public static DateTime LastUpdated = DateTime.MinValue;

        public static List<TrinityBuff> ActiveBuffs
        {
            get { return _buffs; }
        }

        public static Element ConventionElement { get; set; }
        public bool HasArchon { get; private set; }
        public static bool HasBastiansWillGeneratorBuff { get; set; }
        public static bool HasBastiansWillSpenderBuff { get; set; }
        public static bool HasBlessedShrine { get; private set; }
        public static bool HasCastingShrine { get; set; }
        public static bool HasConduitPylon { get; set; }
        public static bool HasConduitShrine { get; set; }
        public static bool HasFrenzyShrine { get; private set; }
        public static bool HasInvulnerableShrine { get; private set; }

        public static void Clear()
        {
            _buffs.Clear();
            _buffsById.Clear();
        }

        public static void Dump()
        {
            using (new MemoryHelper())
            {
                foreach (var hotbarskill in ActiveBuffs)
                {
                    Logger.Log("Id={0} InternalName={1} Cancellable={2} StackCount={3}",
                        hotbarskill.Id,
                        hotbarskill.InternalName,
                        hotbarskill.IsCancellable,
                        hotbarskill.StackCount
                        );
                }
            }
        }

        public static TrinityBuff GetBuff(int id)
        {
            TrinityBuff buff;
            return _buffsById.TryGetValue(id, out buff) ? buff : new TrinityBuff();
        }

        public static TrinityBuff GetBuff(int id, int variantId)
        {
            return _buffs.FirstOrDefault(b => b.Id == id && b.VariantId == variantId);
        }

        public static TrinityBuff GetBuff(SNOPower id)
        {
            return GetBuff((int) id);
        }

        public static int GetBuffStacks(int id)
        {
            return GetBuff(id).StackCount;
        }

        public static int GetBuffStacks(SNOPower id)
        {
            return GetBuffStacks((int) id);
        }

        /// <summary>
        /// Finds the buff UIElement for a SnoPower
        /// </summary>
        public static List<UIElement> GetBuffUIElements(SNOPower snoPower)
        {
            var container = UIElement.FromName("Root.NormalLayer.buffs_backgroundScreen");
            var elements = UIElement.GetChildren(container);
            return elements.Where(element => element.Name.Contains(snoPower.ToString())).ToList();
        }

        /// <summary>
        /// Finds the code that differentiates the state of a buff icon or animation 
        /// Root.NormalLayer.buffs_backgroundScreen.buff P2_ItemPassive_Unique_Ring_038:1:2018967705:0 dlg.icon
        /// Root.NormalLayer.buffs_backgroundScreen.buff P2_ItemPassive_Unique_Ring_038:2:2018967705:0 dlg.icon
        /// </summary>
        public static int GetBuffVariantId(SNOPower snoPower)
        {
            if(!DataDictionary.CheckVariantBuffs.Contains(snoPower))
                return 0;

            var elements = GetBuffUIElements(snoPower);
            if (!elements.Any())
                return 0;

            foreach (var element in elements)
            {
                var buffCode = element.Name.Split(' ').ElementAtOrDefault(1);
                if (buffCode == null)
                    continue;

                var variantIdPart = buffCode.Split(':').ElementAtOrDefault(1);
                if (variantIdPart == null)
                    continue;

                int variantId;
                if (!int.TryParse(variantIdPart, out variantId))
                    continue;

                // Skip Duplicate Elements (Bastians)
                if (!HasBuff((int) snoPower, variantId))
                    return variantId;
            }

            return 0;
        }

        public static string GetBuffVariantName(TrinityBuff buff)
        {
            switch (buff.Id)
            {
                case (int) SNOPower.ItemPassive_Unique_Ring_735_x1: // Bastians of Will
                    return ((ResourceEffectType) buff.VariantId).ToString();

                case (int) SNOPower.P2_ItemPassive_Unique_Ring_038: // Convention of Elements
                    return ((Element) buff.VariantId).ToString();
            }
            return string.Empty;
        }

        private static bool HasBuff(SNOPower power, int variantId)
        {
            return HasBuff((int) power, variantId);
        }

        public static bool HasBuff(int id, int variantId)
        {
            return GetBuff(id, variantId) != null;
        }

        public static bool HasBuff(int id)
        {
            return _buffsById.ContainsKey(id);
        }

        public static bool HasBuff(SNOPower id)
        {
            return HasBuff((int) id);
        }

        public static void Update()
        {
            if (!CacheManager.Me.IsValid)
                return;

            using (new PerformanceLogger("UpdateCachedBuffsData"))
            {
                if (DateTime.UtcNow.Subtract(LastUpdated).TotalMilliseconds < 250)
                    return;

                Clear();

                foreach (var buff in ZetaDia.Me.GetAllBuffs())
                {
                    if (!buff.IsValid)
                        return;

                    var cachedBuff = new TrinityBuff(buff);

                    cachedBuff.VariantId = GetBuffVariantId((SNOPower) cachedBuff.Id);
                    cachedBuff.VariantName = GetBuffVariantName(cachedBuff);

                    // Convention of Elements
                    if (cachedBuff.Id == (int) SNOPower.P2_ItemPassive_Unique_Ring_038)
                    {
                        ConventionElement = (Element) cachedBuff.VariantId;
                    }

                    if (!_buffsById.ContainsKey(buff.SNOId))
                        _buffsById.Add(buff.SNOId, cachedBuff);

                    _buffs.Add(cachedBuff);

                    Logger.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement,
                        "ActiveBuffs: Id={0} Name={1} Stacks={2} VariantId={3} VariantName={4}", cachedBuff.Id, cachedBuff.InternalName, cachedBuff.StackCount, cachedBuff.VariantId, cachedBuff.VariantName);
                }

                LastUpdated = DateTime.UtcNow;

                Logger.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement,
                    "Refreshed Inventory: ActiveBuffs={0}", ActiveBuffs.Count);
            }

            // Bastians of Will
            HasBastiansWillSpenderBuff = HasBuff(SNOPower.ItemPassive_Unique_Ring_735_x1, 2);
            HasBastiansWillGeneratorBuff = HasBuff(SNOPower.ItemPassive_Unique_Ring_735_x1, 1);

            // Shrines
            HasBlessedShrine = HasBuff(30476); //Blessed (+25% defence)
            HasFrenzyShrine = HasBuff(30479); //Frenzy  (+25% atk speed)
            HasInvulnerableShrine = HasBuff(SNOPower.Pages_Buff_Invulnerable);
            HasCastingShrine = HasBuff(SNOPower.Pages_Buff_Infinite_Casting);
            HasConduitShrine = HasBuff(SNOPower.Pages_Buff_Electrified);
        }
    }
}