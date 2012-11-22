using Zeta.Common.Plugins;
using Zeta.Internals;
using Zeta.Internals.Actors;
namespace GilesTrinity
{
    public partial class GilesTrinity : IPlugin
    {
// GilesCachedACDItem - Special caching class to help with backpack-item handling
// So we can make an object, read all item stats from a backpack item *ONCE*, then store it here while my behavior trees process everything
// Preventing any need for calling D3 memory again after the initial read (every D3 memory read is a chance for a DB crash/item mis-read/stuck!)
        public class GilesCachedACDItem
        {
            public string ThisInternalName { get; set; }
            public string ThisRealName { get; set; }
            public int ThisLevel { get; set; }
            public ItemQuality ThisQuality { get; set; }
            public int ThisGoldAmount { get; set; }
            public int ThisBalanceID { get; set; }
            public int ThisDynamicID { get; set; }
            public float ThisWeaponDPS { get; set; }
            public bool ThisOneHanded { get; set; }
            public DyeType ThisDyeType { get; set; }
            public ItemType ThisDBItemType { get; set; }
            public FollowerType ThisFollowerType { get; set; }
            public bool IsUnidentified { get; set; }
            public int ThisItemStackQuantity { get; set; }
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
            public GilesCachedACDItem(string internalname, string realname, int level, ItemQuality quality, int goldamount, int balanceid, int dynamicid, float dps,
                bool onehanded, DyeType dyetype, ItemType dbitemtype, FollowerType dbfollowertype, bool unidentified, int stackquantity, ItemStats thesestats)
            {
                ThisInternalName = internalname;
                ThisRealName = realname;
                ThisLevel = level;
                ThisQuality = quality;
                ThisGoldAmount = goldamount;
                ThisBalanceID = balanceid;
                ThisDynamicID = dynamicid;
                ThisWeaponDPS = dps;
                ThisOneHanded = onehanded;
                ThisDyeType = dyetype;
                ThisDBItemType = dbitemtype;
                ThisFollowerType = dbfollowertype;
                IsUnidentified = unidentified;
                ThisItemStackQuantity = stackquantity;
                Dexterity = thesestats.Dexterity;
                Intelligence = thesestats.Intelligence;
                Strength = thesestats.Strength;
                Vitality = thesestats.Vitality;
                LifePercent = thesestats.LifePercent;
                LifeOnHit = thesestats.LifeOnHit;
                LifeSteal = thesestats.LifeSteal;
                HealthPerSecond = thesestats.HealthPerSecond;
                MagicFind = thesestats.MagicFind;
                GoldFind = thesestats.GoldFind;
                MovementSpeed = thesestats.MovementSpeed;
                PickUpRadius = thesestats.PickUpRadius;
                Sockets = thesestats.Sockets;
                CritPercent = thesestats.CritPercent;
                CritDamagePercent = thesestats.CritDamagePercent;
                AttackSpeedPercent = thesestats.AttackSpeedPercent;
                MinDamage = thesestats.MinDamage;
                MaxDamage = thesestats.MaxDamage;
                BlockChance = thesestats.BlockChance;
                Thorns = thesestats.Thorns;
                ResistAll = thesestats.ResistAll;
                ResistArcane = thesestats.ResistArcane;
                ResistCold = thesestats.ResistCold;
                ResistFire = thesestats.ResistFire;
                ResistHoly = thesestats.ResistHoly;
                ResistLightning = thesestats.ResistLightning;
                ResistPhysical = thesestats.ResistPhysical;
                ResistPoison = thesestats.ResistPoison;
                WeaponDamagePerSecond = thesestats.WeaponDamagePerSecond;
                ArmorBonus = thesestats.ArmorBonus;
                MaxDiscipline = thesestats.MaxDiscipline;
                MaxMana = thesestats.MaxMana;
                ArcaneOnCrit = thesestats.ArcaneOnCrit;
                ManaRegen = thesestats.ManaRegen;
                GlobeBonus = thesestats.HealthGlobeBonus;
            }
        }
    }
}
