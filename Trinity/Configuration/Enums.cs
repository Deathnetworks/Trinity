﻿using System;

namespace Trinity
{
    /// <summary>
    /// Primary "lowest level" item type (eg EXACTLY what kind of item it is)
    /// </summary>
    public enum TrinityItemType
    {
        Unknown,
        Amethyst,
        Amulet,
        Axe,
        Belt,
        Boots,
        Bracer,
        CeremonialKnife,
        Chest,
        Cloak,
        ConsumableAddSockets,
        CraftTome,
        CraftingMaterial,
        CraftingPlan,
        CrusaderShield,
        Dagger,
        Diamond,
        Dye,
        Emerald,
        FistWeapon,
        FollowerEnchantress,
        FollowerScoundrel,
        FollowerTemplar,
        Flail,
        Gloves,
        HandCrossbow,
        HealthGlobe,
        HealthPotion,
        Helm,
        HoradricRelic,
        HoradricCache,
        InfernalKey,
        Legs,
        LootRunKey,
        Mace,
        MightyBelt,
        MightyWeapon,
        Mojo,
        Orb,
        PowerGlobe,
        ProgressionGlobe,
        Quiver,
        Ring,
        Ruby,
        Shield,
        Shoulder,
        Spear,
        SpecialItem,
        SpiritStone,
        StaffOfHerding,
        Sword,
        TieredLootrunKey,
        Topaz,
        TwoHandAxe,
        TwoHandBow,
        TwoHandCrossbow,
        TwoHandDaibo,
        TwoHandFlail,
        TwoHandMace,
        TwoHandMighty,
        TwoHandPolearm,
        TwoHandStaff,
        TwoHandSword,
        VoodooMask,
        Wand,
        WizardHat,
    }

    /// <summary>
    /// Base types, eg "one handed weapons" "armors" etc.
    /// </summary>
    public enum TrinityItemBaseType
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
        HealthGlobe,
        PowerGlobe,
        ProgressionGlobe,
    }

    /// <summary>
    /// Generic object types - eg a monster, an item to pickup, a shrine to click etc.
    /// </summary>
    public enum TrinityObjectType
    {
        Unknown,
        Avoidance,
        Backtrack,
        Barricade,
        Checkpoint,
        Container,
        Destructible,
        Door,
        HealthGlobe,
        Gold,
        HealthWell,
        HotSpot,
        Interactable,
        Item,
        MarkerLocation,
        Player,
        PowerGlobe,
        ProgressionGlobe,
        Proxy,
        SavePoint,
        ServerProp,
        Shrine,
        StartLocation,
        Trigger,
        Unit,
        CursedChest,
        CursedShrine,
        Portal,
        Banner,
        Waypoint,
    }

    public enum Element
    {
        Unknown = 0,
        Arcane,
        Cold,
        Fire,
        Holy,
        Lightning,
        Physical,
        Poison
    }

    public enum SpellCategory
    {
        Unknown = 0,
        Primary,
        Secondary,
        Defensive,
        Might,
        Tactics,
        Rage,
        Techniques,
        Focus,
        Mantras,
        Utility,
        Laws,
        Conviction,
        Voodoo,
        Decay,
        Terror,
        Hunting,
        Archery,
        Devices,
        Conjuration,
        Mastery,
        Force,
    }

    public enum Resource
    {
        Unknown = 0,
        None,
        Fury,
        Arcane,
        Wrath,
        Mana,
        Discipline,
        Hatred,
        Spirit
    }

    public enum ItemQualityColor
    {
        Grey,
        White,
        Blue,
        Yellow,
        Orange,
        Green,
        Other
    }
    public enum ItemQualityName
    {
        Inferior,
        Normal,
        Magic,
        Rare,
        Legendary,
        Set,
        Special
    }

    public enum ResourceEffectType
    {
        None = 0,
        Generator,
        Spender
    }

    public enum AreaEffectShapeType
    {
        None = 0,
        Cone,                
        Circle,
        Beam        
    }

    [Flags]
    public enum EffectTypeFlags
    {        
        None            = 0,
        Stun            = 1 << 0,
        Knockback       = 1 << 1,
        Immobilize      = 1 << 2,
        Chill           = 1 << 3,
        Blind           = 1 << 4,
        Charm           = 1 << 5,
        Slow            = 1 << 6,
        Freeze          = 1 << 7, 

        Any = Freeze | Stun | Knockback | Immobilize | Chill | Blind | Charm | Slow
    }

    public enum IgnoreReason
    {        
        None = 0,
        Invalid,
        ObjectType,
        PlayerSummon,
        Environment,
        ClientEffect,
        Blacklist,
        ZDiff,
        IgnoreUnknown,
        IgnoreLoS,
        IgnoreName,
        Distance,
        Invulnerable,
        Invisible,
        PlayerHeadstone,
        NotInRange,
        DisabledByScript,
        NoDamage,
        HasBeenOperated,
        IsLocked,
        DoorAnimating,
        GizmoState1,
        Untargetable,
        EndAnimation,
        Settings,
        AlreadyOpen
    }
    

    /// <summary>
    /// Enum CraftingMaterialType - this is not finalized, i don't have all the items
    /// </summary>
    public enum CraftingMaterialType
    {
        Unknown = 0,
        CommonDebris = 1,
        ReusableParts = 361984,
        ArcaneDust = 361985,
        ExquisiteEssence = 3,
        ShimmeringEssence = 4,
        SubtleEssence = 5,
        WishfulEssence = 6,
        DeathsBreath = 361989,
        DemonicEssence = 8,
        EncrustedHoof = 9,
        FallenTooth = 10,
        IridescentTear = 11,
        LizardEye = 12,
        VeiledCrystal = 361986,
        FieryBrimstone = 189863,
        ForgottonSoul = 361988,
        KeyOfBones = 364694,
        KeyOfEvil = 364697,
        KeyOfGluttony = 364695,
        KeyOfWar = 364696,
        KeyOfDestruction = 255880,
        KeyOfHate = 255881,
        KeyOfTerror = 255882,
    }

    public enum MonsterQuality
    {
        Normal = 0,
        Champion = 1,
        Rare = 2,
        Minion = 3,
        Unique = 4,
        Hireling = 5,
        Boss = 7
    }

    public enum SummonType
    {
        Unknown = 0,
        Sentry,
        Hydra,
        ZombieDog,
        MysticAlly,
        Gargantuan,
        Companion,
        Hireling,
    }



}

