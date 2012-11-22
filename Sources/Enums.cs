using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.Windows.Markup;
using Zeta;
using Zeta.Common;
using Zeta.Common.Plugins;
using Zeta.CommonBot;
using Zeta.CommonBot.Profile;
using Zeta.CommonBot.Profile.Composites;
using Zeta.Internals;
using Zeta.Internals.Actors;
using Zeta.Internals.Actors.Gizmos;
using Zeta.Internals.SNO;
using Zeta.Navigation;
using Zeta.TreeSharp;
using Zeta.XmlEngine;
namespace GilesTrinity
{
    public partial class GilesTrinity : IPlugin
    {
// Primary "lowest level" item type (eg EXACTLY what kind of item it is)
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
// Base types, eg "one handed weapons" "armors" etc.
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
// Generic object types - eg a monster, an item to pickup, a shrine to click etc.
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
