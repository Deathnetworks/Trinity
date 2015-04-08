namespace Trinity
{
    /// <summary>
    /// Primary "lowest level" item type (eg EXACTLY what kind of item it is)
    /// </summary>
    public enum GItemType
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
    public enum GItemBaseType
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
    public enum GObjectType
    {
        Unknown,
        Avoidance,
        OocAvoidance,
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
    }

    public enum Element
    {
        Unknown = 0,
        Physical,
        Cold,
        Fire,
        Lightning,
        Poison,
        Arcane,
        Holy,
        GroundEffect,
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

    public enum MoveType
    {
        Avoidance = 5,
        Kite = 4,
        SpecialCombat = 3,
        BasicCombat = 2,
        KeepMoving = 1,
        TargetAttempt = 0
    }

    public enum AvoidType
    {
        Unknown = 0,
        GenericCast,
        MeleeAttack,
        RangedAttack,
        Attack,
        AttackRight,
        AttackLeft,
        Teleport,
        Leap,
        Charge,
        Strafe,
        StrafeRight,
        StrafeLeft,
        Dash,
        Grab,
        GroundCircle,
        KnockBack,
        RotateLeft,
        RotateRight,
        Bomb,
        Projectile,
    }

    public enum WeightType
    {
        Unknown = 0,
        Unchangeable,
        Dynamic,
        Target,
        Cluster,
        Monster
    }
}
