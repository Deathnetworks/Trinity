using System;
using System.Linq;
using System.Runtime.Serialization;
using Trinity.Cache;
using Trinity.Combat.Abilities;
using Trinity.Items;
using Trinity.Reference;
using Trinity.UIComponents;
using Zeta.Game;
using Zeta.Game.Internals.Actors;

namespace Trinity.Objects
{
    [DataContract(Namespace = "")]
    public class Item : IUnique, IEquatable<Item>
    {
        [DataMember]
        public int Id { get; set; }
        public string Name { get; set; }

        public ItemType ItemType { get; set; }

        public ItemQuality Quality { get; set; }
        public ItemBaseType BaseType { get; set; }

        public string Slug { get; set; }
        public string InternalName { get; set; }
        public string RelativeUrl { get; set; }
        public string DataUrl { get; set; }
        public string Url { get; set; }
        public string LegendaryAffix { get; set; }
        public string SetName { get; set; }
        public bool IsCrafted { get; set; }

        public Item()
        {
            
        }

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
            get { return CacheData.Inventory.EquippedIds.Contains(Id); }
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

        public int BuffStacks
        {
            get
            {
                if (!IsEquipped) 
                    return 0;

                SNOPower power;
                return DataDictionary.PowerByItem.TryGetValue(this, out power) ? CacheData.Buffs.GetBuffStacks(power) : 0;
            }
        }

        public ActorClass ClassRestriction
        {
            get { return GetClassRestriction(GItemType); }
        }

        public static ActorClass GetClassRestriction(GItemType type)
        {
            switch (type)
            {
                case GItemType.Flail:
                case GItemType.CrusaderShield:
                case GItemType.TwoHandFlail:
                    return ActorClass.Crusader;

                case GItemType.FistWeapon:
                case GItemType.SpiritStone:
                case GItemType.TwoHandDaibo:
                    return ActorClass.Monk;

                case GItemType.VoodooMask:
                case GItemType.Mojo:
                case GItemType.CeremonialKnife:
                    return ActorClass.Witchdoctor;

                case GItemType.MightyBelt:
                case GItemType.MightyWeapon:
                    return ActorClass.Barbarian;

                case GItemType.WizardHat:
                case GItemType.Orb:
                    return ActorClass.Wizard;

                case GItemType.HandCrossbow:
                case GItemType.Cloak:
                case GItemType.Quiver:
                case GItemType.TwoHandBow:
                case GItemType.TwoHandCrossbow:
                    return ActorClass.DemonHunter;   
            }
            
            return ActorClass.Invalid;            
        }

        public bool Equals(Item other)
        {
            return GetHashCode().Equals(other.GetHashCode());
        }

        /// <summary>
        /// Unique Identifier so that dictionarys can compare this object properly.
        /// </summary>   
        public override int GetHashCode()
        {
            return Id.GetHashCode() ^ Name.GetHashCode();
        }

        public bool IsTwoHanded { get; set; }

        public GItemType GItemType { get; set; }

        public string IconUrl { get; set; }
    }
}
