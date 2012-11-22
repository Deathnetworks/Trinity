using Zeta.Common.Plugins;
namespace GilesTrinity
{
    public partial class GilesTrinity : IPlugin
    {
        /// <summary>
        /// Primary "lowest level" item type (eg EXACTLY what kind of item it is)
        /// </summary>
        public enum GilesItemType
        {
            Unknown,
            Axe,
            CeremonialKnife,
            HandCrossbow,
            Dagger,
            FistWeapon,
            Mace,
            MightyWeapon,
            Spear,
            Sword,
            Wand,
            TwoHandAxe,
            TwoHandBow,
            TwoHandDaibo,
            TwoHandCrossbow,
            TwoHandMace,
            TwoHandMighty,
            TwoHandPolearm,
            TwoHandStaff,
            TwoHandSword,
            StaffOfHerding,
            Mojo,
            Source,
            Quiver,
            Shield,
            Amulet,
            Ring,
            Belt,
            Boots,
            Bracers,
            Chest,
            Cloak,
            Gloves,
            Helm,
            Pants,
            MightyBelt,
            Shoulders,
            SpiritStone,
            VoodooMask,
            WizardHat,
            FollowerEnchantress,
            FollowerScoundrel,
            FollowerTemplar,
            CraftingMaterial,
            CraftTome,
            Ruby,
            Emerald,
            Topaz,
            Amethyst,
            SpecialItem,
            CraftingPlan,
            HealthPotion,
            Dye,
            HealthGlobe,
            InfernalKey,
        }

        /// <summary>
        /// Base types, eg "one handed weapons" "armors" etc.
        /// </summary>
        public enum GilesBaseItemType
        {
            Unknown,
            WeaponOneHand,
            WeaponTwoHand,
            WeaponRange,
            Offhand,
            Armor,
            Jewelry,
            FollowerItem,
            Misc,
            Gem,
            HealthGlobe
        }

        /// <summary>
        /// Generic object types - eg a monster, an item to pickup, a shrine to click etc.
        /// </summary>
        public enum GilesObjectType
        {
            Unknown,
            Unit,
            Avoidance,
            Item,
            Gold,
            Globe,
            Shrine,
            HealthWell,
            Door,
            Container,
            Interactable,
            Destructible,
            Barricade,
            Backtrack
       }
    }
}
