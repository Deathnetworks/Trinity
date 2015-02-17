using System;
using System.Collections.Generic;
using System.Linq;
using Trinity.Helpers;
using Trinity.Technicals;
using Zeta.Bot;
using Zeta.Game;
using Zeta.Game.Internals.Actors;

namespace Trinity
{
    public partial class CacheData
    {
        /// <summary>
        /// Fast Inventory Cache, Self-Updating, Use instead of ZetaDia.Inventory
        /// </summary>
        public class BuffsCache
        {
            static BuffsCache()
            {                
                Pulsator.OnPulse += (sender, args) => Instance.UpdateBuffsCache();
            }

            public BuffsCache()
            {
                // Make sure data is immediately available from
                // calls while bot is not running or before pulse starts
                UpdateBuffsCache();                
            }

            private static BuffsCache _instance = null;
            public static BuffsCache Instance
            {
                get { return _instance ?? (_instance = new BuffsCache()); }
                set { _instance = value; }
            }

            public class CachedBuff
            {
                public CachedBuff() { }

                public CachedBuff(Buff buff)
                {
                    _buff = buff;
                    InternalName = buff.InternalName;
                    IsCancellable = buff.IsCancelable;
                    StackCount = buff.StackCount;
                    Id = buff.SNOId;
                }

                private readonly Buff _buff;
                public string InternalName { get; set; }
                public bool IsCancellable { get; set; }
                public int StackCount { get; set; }
                public int Id { get; set; }

                public void Cancel()
                {
                    if (IsCancellable && _buff.IsValid)
                        _buff.Cancel();
                }

                public override string ToString()
                {
                    return ToStringReflector.GetObjectString(this);
                }
            }

            public List<CachedBuff> ActiveBuffs { get; private set; }
            public bool HasBlessedShrine { get; private set; }
            public bool HasFrenzyShrine { get; private set; }
            public bool HasArchon { get; private set; }
            public bool HasInvulnerableShrine { get; private set; }
            public bool HasCastingShrine { get; set; }
            public bool HasConduitPylon { get; set; }
            public DateTime LastUpdated = DateTime.MinValue;

            private Dictionary<int, CachedBuff> _buffsById = new Dictionary<int, CachedBuff>();

            public void UpdateBuffsCache()
            {
                if (ZetaDia.Me == null)
                    return;
                if (!ZetaDia.Me.IsFullyValid())
                    return;

                using (new PerformanceLogger("UpdateCachedBuffsData"))
                {
                    if (DateTime.UtcNow.Subtract(LastUpdated).TotalMilliseconds < 500)
                        return;

                    Clear();

                    foreach (var buff in ZetaDia.Me.GetAllBuffs())
                    {
                        if (!buff.IsValid)
                            return;

                        var cachedBuff = new CachedBuff(buff);

                        if (cachedBuff.Id == (int)SNOPower.Wizard_Archon)
                            HasArchon = true;
                        if (cachedBuff.Id == 30476) //Blessed (+25% defence)
                            HasBlessedShrine = true;
                        if (cachedBuff.Id == 30479) //Frenzy  (+25% atk speed)
                            HasFrenzyShrine = true;
                        if (cachedBuff.Id == (int)SNOPower.Pages_Buff_Invulnerable)
                            HasInvulnerableShrine = true;
                        if (cachedBuff.Id == (int)SNOPower.Pages_Buff_Infinite_Casting)
                            HasCastingShrine = true;
                        if (cachedBuff.Id == (int)SNOPower.Pages_Buff_Electrified)
                            HasCastingShrine = true;                        

                        if (!_buffsById.ContainsKey(buff.SNOId))
                            _buffsById.Add(buff.SNOId, cachedBuff);
                        else
                        {
                            Logger.LogDebug(LogCategory.CacheManagement, "Duplicate buff detected: {0}", cachedBuff);
                        }
                        ActiveBuffs.Add(cachedBuff);
                    }

                    LastUpdated = DateTime.UtcNow;

                    Logger.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement,
                        "Refreshed Inventory: ActiveBuffs={0}", ActiveBuffs.Count);
                }
            }

            public CachedBuff GetBuff(int id)
            {
                CachedBuff buff;
                return _buffsById.TryGetValue(id, out buff) ? new CachedBuff() : new CachedBuff();
            }

            public CachedBuff GetBuff(SNOPower id)
            {
                return GetBuff((int)id);
            }

            public bool HasBuff(int id)
            {
                return _buffsById.ContainsKey(id);
            }

            public bool HasBuff(SNOPower id)
            {
                return HasBuff((int)id);
            }

            public int GetBuffStacks(int id)
            {
                return GetBuff(id).StackCount;
            }

            public int GetBuffStacks(SNOPower id)
            {
                return GetBuffStacks((int)id);
            }

            public void Dump()
            {
                using (new MemoryHelper())
                {
                    foreach (var hotbarskill in ActiveBuffs.ToList())
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

            public void Clear()
            {
                ActiveBuffs = new List<CachedBuff>();
                _buffsById = new Dictionary<int, CachedBuff>();
            }

        }

    }
}