using System.Linq;
using Trinity.Cache;
using Trinity.Combat.Abilities;
using Trinity.Reference;
using Zeta.Game;
using Zeta.Game.Internals.Actors;

namespace Trinity.Objects
{
    public class Item
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ItemType ItemType { get; set; }

        public Item(int actorId, string name = "", ItemType itemType = ItemType.Unknown)
        {
            Id = actorId;
            Name = name;
            ItemType = itemType;
        }

        public Item(ACDItem acdItem)
        {
            Id = acdItem.ActorSNO;
            Name = acdItem.Name;
            ItemType = acdItem.ItemType;
        }

        /// <summary>
        /// If this item is currently equipped
        /// </summary>
        public bool IsEquipped
        {
            get { return EquippedItemCache.Instance.ItemIds.Contains(Id); }
        }

        /// <summary>
        /// If this item belongs to a set
        /// </summary>
        public bool IsSetItem
        {
            get { return Sets.SetItemIds.Contains(Id); }
        }

        /// <summary>
        /// The set this item belongs to, if applicable.
        /// </summary>
        public Set Set
        {
            get
            {
                var set = Sets.ToList().FirstOrDefault(s => s.ItemIds.Contains(Id));
                return set ?? new Set();
            }
        }

        /// <summary>
        /// If the associated buff or minion is currently active
        /// </summary>
        public bool IsBuffActive
        {
            get
            {
                if (!IsEquipped) return false;

                // Item Creates Buff
                SNOPower power;
                if (DataDictionary.PowerByItem.TryGetValue(this, out power))
                    return CombatBase.GetHasBuff(power);

                // Item Spawns Minions
                string internalNameToken;
                if (DataDictionary.MinionInternalNameTokenByItem.TryGetValue(this, out internalNameToken))
                    return ZetaDia.Actors.GetActorsOfType<DiaUnit>().Any(u => u.PetType > 0 && u.Name.Contains(internalNameToken));

                return false;
            }
        }

        /// <summary>
        /// Unique Identifier so that dictionarys can compare this object properly.
        /// </summary>   
        public override int GetHashCode()
        {
            return Id.GetHashCode() ^ Name.GetHashCode();
        }

    }
}
