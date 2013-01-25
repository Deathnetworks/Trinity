using System;
using Zeta.Internals;
using Zeta.Internals.Actors;
namespace GilesTrinity
{
    // GilesCachedACDItem - Special caching class to help with backpack-item handling
    // So we can make an object, read all item stats from a backpack item *ONCE*, then store it here while my behavior trees process everything
    // Preventing any need for calling D3 memory again after the initial read (every D3 memory read is a chance for a DB crash/item mis-read/stuck!)
    public class GilesCachedACDItem : IComparable
    {
        public string InternalName { get; set; }
        public string RealName { get; set; }
        public int Level { get; set; }
        public ItemQuality Quality { get; set; }
        public int GoldAmount { get; set; }
        public int BalanceID { get; set; }
        public int DynamicID { get; set; }
        public float WeaponDPS { get; set; }
        public bool OneHanded { get; set; }
        public bool TwoHanded { get; set; }
        public DyeType DyeType { get; set; }
        public ItemType DBItemType { get; set; }
        public Zeta.Internals.Actors.ItemBaseType DBBaseType { get; set; }
        public FollowerType FollowerType { get; set; }
        public bool IsUnidentified { get; set; }
        public int ItemStackQuantity { get; set; }
        public float Dexterity { get; set; }
        public float Intelligence { get; set; }
        public float Strength { get; set; }
        public float Vitality { get; set; }
        public float LifePercent { get; set; }
        public float LifeOnHit { get; set; }
        public float LifeSteal { get; set; }
        public float HealthPerSecond { get; set; }
        public float MagicFind { get; set; }
        public float GoldFind { get; set; }
        public float MovementSpeed { get; set; }
        public float PickUpRadius { get; set; }
        public float Sockets { get; set; }
        public float CritPercent { get; set; }
        public float CritDamagePercent { get; set; }
        public float AttackSpeedPercent { get; set; }
        public float MinDamage { get; set; }
        public float MaxDamage { get; set; }
        public float BlockChance { get; set; }
        public float Thorns { get; set; }
        public float ResistAll { get; set; }
        public float ResistArcane { get; set; }
        public float ResistCold { get; set; }
        public float ResistFire { get; set; }
        public float ResistHoly { get; set; }
        public float ResistLightning { get; set; }
        public float ResistPhysical { get; set; }
        public float ResistPoison { get; set; }
        public float WeaponDamagePerSecond { get; set; }
        public float ArmorBonus { get; set; }
        public float MaxDiscipline { get; set; }
        public float MaxMana { get; set; }
        public float ArcaneOnCrit { get; set; }
        public float ManaRegen { get; set; }
        public float GlobeBonus { get; set; }
        public float HatredRegen { get; set; }
        public float MaxFury { get; set; }
        public float SpiritRegen { get; set; }
        public float MaxSpirit { get; set; }
        public float HealthPerSpiritSpent { get; set; }
        public float MaxArcanePower { get; set; }
        public float DamageReductionPhysicalPercent { get; set; }
        public float ArmorTotal { get; set; }
        public float Armor { get; set; }
        public float FireDamagePercent { get; set; }
        public float LightningDamagePercent { get; set; }
        public float ColdDamagePercent { get; set; }
        public float PoisonDamagePercent { get; set; }
        public float ArcaneDamagePercent { get; set; }
        public float HolyDamagePercent { get; set; }
        public float HealthGlobeBonus { get; set; }
        public float WeaponAttacksPerSecond { get; set; }
        public float WeaponMaxDamage { get; set; }
        public float WeaponMinDamage { get; set; }
        public ACDItem AcdItem { get; set; }
        public int Row { get; set; }
        public int Column { get; set; }
        public string ItemLink { get; set; }

        public GilesCachedACDItem(ItemStats stats)
        {
            WeaponDamagePerSecond = stats.WeaponDamagePerSecond;
            Dexterity = stats.Dexterity;
            Intelligence = stats.Intelligence;
            Strength = stats.Strength;
            Vitality = stats.Vitality;
            LifePercent = stats.LifePercent;
            LifeOnHit = stats.LifeOnHit;
            LifeSteal = stats.LifeSteal;
            HealthPerSecond = stats.HealthPerSecond;
            MagicFind = stats.MagicFind;
            GoldFind = stats.GoldFind;
            MovementSpeed = stats.MovementSpeed;
            PickUpRadius = stats.PickUpRadius;
            Sockets = stats.Sockets;
            CritPercent = stats.CritPercent;
            CritDamagePercent = stats.CritDamagePercent;
            AttackSpeedPercent = stats.AttackSpeedPercent;
            MinDamage = stats.MinDamage;
            MaxDamage = stats.MaxDamage;
            BlockChance = stats.BlockChance;
            Thorns = stats.Thorns;
            ResistAll = stats.ResistAll;
            ResistArcane = stats.ResistArcane;
            ResistCold = stats.ResistCold;
            ResistFire = stats.ResistFire;
            ResistHoly = stats.ResistHoly;
            ResistLightning = stats.ResistLightning;
            ResistPhysical = stats.ResistPhysical;
            ResistPoison = stats.ResistPoison;
            WeaponDamagePerSecond = stats.WeaponDamagePerSecond;
            ArmorBonus = stats.ArmorBonus;
            MaxDiscipline = stats.MaxDiscipline;
            MaxMana = stats.MaxMana;
            ArcaneOnCrit = stats.ArcaneOnCrit;
            ManaRegen = stats.ManaRegen;
            GlobeBonus = stats.HealthGlobeBonus;
            HatredRegen = stats.HatredRegen;
            MaxFury = stats.MaxFury;
            SpiritRegen = stats.SpiritRegen;
            MaxSpirit = stats.MaxSpirit;
            HealthPerSpiritSpent = stats.HealthPerSpiritSpent;
            MaxArcanePower = stats.MaxArcanePower;
            DamageReductionPhysicalPercent = stats.DamageReductionPhysicalPercent;
            ArmorTotal = stats.ArmorTotal;
            Armor = stats.Armor;
            FireDamagePercent = stats.FireDamagePercent;
            LightningDamagePercent = stats.LightningDamagePercent;
            ColdDamagePercent = stats.ColdDamagePercent;
            PoisonDamagePercent = stats.PoisonDamagePercent;
            ArcaneDamagePercent = stats.ArcaneDamagePercent;
            HolyDamagePercent = stats.HolyDamagePercent;
            HealthGlobeBonus = stats.HealthGlobeBonus;
            WeaponAttacksPerSecond = stats.WeaponAttacksPerSecond;
            WeaponMaxDamage = stats.WeaponMaxDamage;
            WeaponMinDamage = stats.WeaponMinDamage;
        }

        public GilesCachedACDItem(
            ACDItem acdItem,
            string internalName, 
            string realName, 
            int level, 
            ItemQuality quality, 
            int goldAmount, 
            int balanceId, 
            int dynamicId, 
            float dps,
            bool oneHanded, 
            bool twoHanded,
            DyeType dyeType, 
            ItemType itemType,
            Zeta.Internals.Actors.ItemBaseType itembasetype, 
            FollowerType followerType, 
            bool unidentified, 
            int stackQuantity, 
            ItemStats itemStats)
        {
            AcdItem = acdItem;
            InternalName = internalName;
            RealName = realName;
            Level = level;
            Quality = quality;
            GoldAmount = goldAmount;
            BalanceID = balanceId;
            DynamicID = dynamicId;
            WeaponDPS = dps;
            OneHanded = oneHanded;
            TwoHanded = twoHanded;
            DyeType = dyeType;
            DBItemType = itemType;
            DBBaseType = itembasetype;
            FollowerType = followerType;
            IsUnidentified = unidentified;
            ItemStackQuantity = stackQuantity;



            Dexterity = itemStats.Dexterity;
            Intelligence = itemStats.Intelligence;
            Strength = itemStats.Strength;
            Vitality = itemStats.Vitality;
            LifePercent = itemStats.LifePercent;
            LifeOnHit = itemStats.LifeOnHit;
            LifeSteal = itemStats.LifeSteal;
            HealthPerSecond = itemStats.HealthPerSecond;
            MagicFind = itemStats.MagicFind;
            GoldFind = itemStats.GoldFind;
            MovementSpeed = itemStats.MovementSpeed;
            PickUpRadius = itemStats.PickUpRadius;
            Sockets = itemStats.Sockets;
            CritPercent = itemStats.CritPercent;
            CritDamagePercent = itemStats.CritDamagePercent;
            AttackSpeedPercent = itemStats.AttackSpeedPercent;
            MinDamage = itemStats.MinDamage;
            MaxDamage = itemStats.MaxDamage;
            BlockChance = itemStats.BlockChance;
            Thorns = itemStats.Thorns;
            ResistAll = itemStats.ResistAll;
            ResistArcane = itemStats.ResistArcane;
            ResistCold = itemStats.ResistCold;
            ResistFire = itemStats.ResistFire;
            ResistHoly = itemStats.ResistHoly;
            ResistLightning = itemStats.ResistLightning;
            ResistPhysical = itemStats.ResistPhysical;
            ResistPoison = itemStats.ResistPoison;
            WeaponDamagePerSecond = itemStats.WeaponDamagePerSecond;
            ArmorBonus = itemStats.ArmorBonus;
            MaxDiscipline = itemStats.MaxDiscipline;
            MaxMana = itemStats.MaxMana;
            ArcaneOnCrit = itemStats.ArcaneOnCrit;
            ManaRegen = itemStats.ManaRegen;
            GlobeBonus = itemStats.HealthGlobeBonus;
            HatredRegen = itemStats.HatredRegen;
            MaxFury = itemStats.MaxFury;
            SpiritRegen = itemStats.SpiritRegen;
            MaxSpirit = itemStats.MaxSpirit;
            HealthPerSpiritSpent = itemStats.HealthPerSpiritSpent;
            MaxArcanePower = itemStats.MaxArcanePower;
            DamageReductionPhysicalPercent = itemStats.DamageReductionPhysicalPercent;
            ArmorTotal = itemStats.ArmorTotal;
            Armor = itemStats.Armor;
            FireDamagePercent = itemStats.FireDamagePercent;
            LightningDamagePercent = itemStats.LightningDamagePercent;
            ColdDamagePercent = itemStats.ColdDamagePercent;
            PoisonDamagePercent = itemStats.PoisonDamagePercent;
            ArcaneDamagePercent = itemStats.ArcaneDamagePercent;
            HolyDamagePercent = itemStats.HolyDamagePercent;
            HealthGlobeBonus = itemStats.HealthGlobeBonus;
            WeaponAttacksPerSecond = itemStats.WeaponAttacksPerSecond;
            WeaponMaxDamage = itemStats.WeaponMaxDamage;
            WeaponMinDamage = itemStats.WeaponMinDamage;

        }


        public int CompareTo(object obj)
        {
            GilesCachedACDItem item = (GilesCachedACDItem)obj;

            if (this.Row < item.Row)
                return -1;
            else if (this.Column < item.Column)
                return -1;
            else if (this.Column == item.Column && this.Row == item.Row)
                return 0;
            else
                return 1;
        }
    }
}
