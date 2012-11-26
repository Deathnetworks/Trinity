using Zeta.Common.Plugins;
namespace GilesTrinity
{
    public partial class GilesTrinity : IPlugin
    {
        // These constants are used for item scoring and stashing
        private const int DEXTERITY = 0;
        private const int INTELLIGENCE = 1;
        private const int STRENGTH = 2;
        private const int VITALITY = 3;
        private const int LIFEPERCENT = 4;
        private const int LIFEONHIT = 5;
        private const int LIFESTEAL = 6;
        private const int LIFEREGEN = 7;
        private const int MAGICFIND = 8;
        private const int GOLDFIND = 9;
        private const int MOVEMENTSPEED = 10;
        private const int PICKUPRADIUS = 11;
        private const int SOCKETS = 12;
        private const int CRITCHANCE = 13;
        private const int CRITDAMAGE = 14;
        private const int ATTACKSPEED = 15;
        private const int MINDAMAGE = 16;
        private const int MAXDAMAGE = 17;
        private const int BLOCKCHANCE = 18;
        private const int THORNS = 19;
        private const int ALLRESIST = 20;
        private const int RANDOMRESIST = 21;
        private const int TOTALDPS = 22;
        private const int ARMOR = 23;
        private const int MAXDISCIPLINE = 24;
        private const int MAXMANA = 25;
        private const int ARCANECRIT = 26;
        private const int MANAREGEN = 27;
        private const int GLOBEBONUS = 28;
        /// <summary>
        /// The number of stats we have to work with
        /// </summary>
        private const int TOTALSTATS = 29;
        // starts at 0, remember... 0-26 = 1-27!
        // Readable names of the above stats that get output into the trash/stash log files
        private static readonly string[] StatNames = new string[29] { 
            "Dexterity",
            "Intelligence",
            "Strength",
            "Vitality",
            "Life %",
            "Life On Hit",
            "Life Steal %",
            "Life Regen",
            "Magic Find %",
            "Gold Find   %",
            "Movement Speed %",
            "Pickup Radius",
            "Sockets",
            "Crit Chance %",
            "Crit Damage %",
            "Attack Speed %",
            "+Min Damage",
            "+Max Damage",
            "Total Block %",
            "Thorns",
            "+All Resist",
            "+Highest Single Resist",
            "DPS",
            "Armor",
            "Max Disc.",
            "Max Mana",
            "Arcane-On-Crit",
            "Mana Regen",
            "Globe Bonus"
        };
        // Stores the apparent maximums of each stat for each item slot
        // Note that while these SHOULD be *actual* maximums for most stats - for things like DPS, these can just be more sort of "what a best-in-slot DPS would be"

        // Variable name                                                  Dex, Int, Str, Vit, Life%,  LOH,Steal%, LPS, MF, GF, MS, Rad, Sox,  Crit%, CDam%, ASPD,  Min+, Max+, Blk, Thorn, AR, SR,  DPS, ARMOR, Disc, Mana, Arc.,  Regen, Globes
        private static double[] MaxPointsWeaponOneHand = new double[29] { 400, 400, 400, 400, 00000, 1000, 00003, 000, 00, 00, 00, 000, 0001, 00000, 00100, 00021, 0000, 0000, 000, 00000, 00, 00, 1500, 00000, 0010, 0000, 00010, 00014, 00000 };
        private static double[] MaxPointsWeaponTwoHand = new double[29] { 700, 700, 700, 700, 00000, 1800, 00008, 000, 00, 00, 00, 000, 0001, 00000, 00200, 00020, 0000, 0000, 000, 00000, 00, 00, 1700, 00000, 0010, 0000, 00010, 00014, 00000 };
        private static double[] MaxPointsWeaponRanged = new double[29]  { 320, 000, 000, 000, 00000, 0000, 00000, 000, 00, 00, 00, 000, 0000, 00000, 00000, 00000, 0000, 0000, 000, 00000, 00, 00, 0000, 00000, 0000, 0000, 00000, 00000, 00000 };
        private static double[] MaxPointsOffHand = new double[29]       { 350, 350, 350, 350, 00012, 0000, 00000, 350, 20, 25, 00, 000, 0001, 00010, 00000, 00020, 0000, 0000, 000, 01450, 00, 00, 0000, 00000, 0010, 0119, 00010, 00011, 05977 };
        private static double[] MaxPointsShield = new double[29]        { 350, 350, 350, 350, 00016, 0000, 00000, 350, 20, 25, 00, 000, 0001, 00010, 00000, 00000, 0000, 0000, 030, 02544, 80, 60, 0000, 00397, 0000, 0000, 00000, 00000, 12794 };
        private static double[] MaxPointsRing = new double[29]          { 262, 262, 262, 262, 00012, 0479, 00000, 342, 20, 25, 00, 000, 0001, 00006, 00050, 00009, 0036, 0100, 000, 00979, 80, 60, 0000, 00265, 0000, 0000, 00000, 00000, 05977 };
        private static double[] MaxPointsAmulet = new double[29]        { 350, 350, 350, 350, 00016, 0959, 00000, 600, 45, 50, 00, 000, 0001, 00010, 00100, 00009, 0036, 0100, 000, 02544, 80, 60, 0000, 00397, 0000, 0000, 00000, 00000, 05977 };
        private static double[] MaxPointsShoulders = new double[29]     { 300, 300, 300, 200, 00012, 0000, 00000, 342, 20, 25, 00, 007, 0000, 00000, 00000, 00000, 0000, 0000, 000, 02544, 80, 60, 0000, 00265, 0000, 0000, 00000, 00000, 12794 };
        private static double[] MaxPointsHelm = new double[29]          { 300, 300, 300, 200, 00012, 0000, 00000, 342, 20, 25, 00, 007, 0001, 00006, 00000, 00000, 0000, 0000, 000, 01454, 80, 60, 0000, 00397, 0000, 0000, 00000, 00000, 12794 };
        private static double[] MaxPointsPants = new double[29]         { 296, 296, 296, 375, 00000, 0000, 00000, 342, 20, 25, 00, 007, 0002, 00000, 00000, 00000, 0000, 0000, 000, 01454, 80, 60, 0000, 00397, 0000, 0000, 00000, 00000, 12794 };
        private static double[] MaxPointsGloves = new double[29]        { 300, 300, 300, 235, 00000, 0000, 00000, 342, 20, 25, 00, 007, 0000, 00010, 00050, 00009, 0000, 0000, 000, 01454, 80, 60, 0000, 00265, 0000, 0000, 00000, 00000, 12794 };
        private static double[] MaxPointsChest = new double[29]         { 300, 300, 300, 300, 00012, 0000, 00000, 599, 20, 25, 00, 007, 0003, 00000, 00000, 00000, 0000, 0000, 000, 02544, 80, 60, 0000, 00397, 0000, 0000, 00000, 00000, 12794 };
        private static double[] MaxPointsBracer = new double[29]        { 267, 267, 267, 200, 00000, 0000, 00000, 342, 20, 25, 00, 007, 0000, 00006, 00000, 00000, 0000, 0000, 000, 01454, 80, 60, 0000, 00265, 0000, 0000, 00000, 00000, 12794 };
        private static double[] MaxPointsBoots = new double[29]         { 300, 300, 300, 200, 00000, 0000, 00000, 342, 20, 25, 12, 007, 0000, 00000, 00000, 00000, 0000, 0000, 000, 01454, 80, 60, 0000, 00265, 0000, 0000, 00000, 00000, 12794 };
        private static double[] MaxPointsBelt = new double[29]          { 300, 300, 300, 200, 00012, 0000, 00000, 342, 20, 25, 00, 007, 0000, 00000, 00000, 00000, 0000, 0000, 000, 02544, 80, 60, 0000, 00265, 0000, 0000, 00000, 00000, 12794 };
        private static double[] MaxPointsCloak = new double[29]         { 200, 200, 200, 300, 00012, 0000, 00000, 410, 20, 25, 00, 007, 0003, 00000, 00000, 00000, 0000, 0000, 000, 02544, 70, 50, 0000, 00397, 0010, 0000, 00000, 00000, 12794 };
        private static double[] MaxPointsMightyBelt = new double[29]    { 200, 200, 300, 200, 00012, 0000, 00003, 342, 20, 25, 00, 007, 0000, 00000, 00000, 00000, 0000, 0000, 000, 02544, 70, 50, 0000, 00265, 0000, 0000, 00000, 00000, 12794 };
        private static double[] MaxPointsSpiritStone = new double[29]   { 200, 300, 200, 200, 00012, 0000, 00000, 342, 20, 25, 00, 007, 0001, 00006, 00000, 00000, 0000, 0000, 000, 01454, 70, 50, 0000, 00397, 0000, 0000, 00000, 00000, 12794 };
        private static double[] MaxPointsVoodooMask = new double[29]    { 200, 300, 200, 200, 00012, 0000, 00000, 342, 20, 25, 00, 007, 0001, 00006, 00000, 00000, 0000, 0000, 000, 01454, 70, 50, 0000, 00397, 0000, 0119, 00000, 00011, 12794 };
        private static double[] MaxPointsWizardHat = new double[29]     { 200, 300, 200, 200, 00012, 0000, 00000, 342, 20, 25, 00, 007, 0001, 00006, 00000, 00000, 0000, 0000, 000, 01454, 70, 50, 0000, 00397, 0000, 0000, 00010, 00000, 12794 };
        private static double[] MaxPointsFollower = new double[29]      { 350, 350, 350, 350, 00000, 1000, 00000, 600, 00, 00, 00, 000, 0000, 00000, 00055, 00000, 0000, 0000, 000, 00000, 80, 60, 0000, 00000, 0000, 0000, 00000, 00000, 00000 };
        // Stores the total points this stat is worth at the above % point of maximum
        // Note that these values get all sorts of bonuses+ multipliers+ and extra things applied in the actual scoring routine. These values are more of a base value.
        // Variable name                                                Dex,   Int,   Str,   Vit,    L%,   LOH, Stl%, LPS,    MF,   GF,   MS,  Rad,   Sox, Crit%, CDam%, ASPD, Min+, Max+,  Blk,  Thn,    AR,   SR,   DPS, ARMR,  Disc, Mana, Arc.,  Regn, Globe
        private static double[] WeaponPointsAtMax = new double[29]  { 14000, 14000, 14000, 14000, 13000, 20000, 7000, 1000, 6000, 6000, 6000, 0500, 16000, 15000, 15000, 0000, 0000, 0000, 0000, 1000, 11000, 0000, 90000, 0000, 10000, 8500, 8500, 10000, 8000 };
        private static double[] ArmorPointsAtMax = new double[29]   { 11000, 11000, 11000, 09500, 09000, 10000, 4000, 1200, 3000, 3000, 3500, 1000, 04300, 09000, 06100, 7000, 3000, 3000, 5000, 1200, 07500, 1500, 00000, 5000, 04000, 3000, 3000, 06000, 4000 };
        private static double[] JewelryPointsAtMax = new double[29] { 11500, 11000, 11000, 10000, 08000, 11000, 4000, 1200, 4500, 4500, 3500, 1000, 03500, 07500, 06300, 6800, 0800, 0800, 5000, 1200, 07500, 1500, 00000, 4500, 04000, 3000, 3000, 06000, 4000 };
        // Some special values for score calculations 
        // BonusThreshold is a percentage of the max-stat possible that the stat starts to get a multiplier on it's score. 1 means it has to be above 100% of the max-stat to get a multiplier (so only possible if the max-stat isn't ACTUALLY the max possible)
        // MinimumThreshold is a percentage of the max-stat possible that the stat will simply be ignored for being too low. eg if set to .5 - then anything less than 50% of the max-stat will be ignored.
        // MinimumPrimary is used for some stats only - and means that at least ONE primary stat has to be above that level  to get score. Eg magic-find has .5 - meaning any item without at least 50% of a max-stat primary  will ignore magic-find scoring.
        // Variable name                                               Dex,  Int,  Str,  Vit,   L%,  LOH, Stl%,  LPS,   MF,   GF,   MS,  Rad,  Sox, Crit, CDam, ASPD, Min+, Max+,  Blk, Thn,    AR,   SR,  DPS, ARMR, Disc, Mana, Arc., Regn, Globe
        private static double[] BonusThreshold = new double[29]     { 0.75, 0.75, 0.75, 0.75, 0.80, 0.70, 0.80, 1.00, 1.00, 1.00, 0.95, 1.00, 1.00, 0.70, 0.90, 0.90, 0.90, 0.90, 0.83, 1.00, 0.85, 0.95, 0.70, 0.90, 1.00, 1.00, 1.00, 0.90, 1.00 };
        private static double[] MinimumThreshold = new double[29]   { 0.40, 0.40, 0.40, 0.30, 0.60, 0.35, 0.60, 0.70, 0.40, 0.40, 0.75, 0.80, 0.40, 0.40, 0.60, 0.40, 0.20, 0.20, 0.65, 0.60, 0.40, 0.75, 0.70, 0.80, 0.70, 0.70, 0.70, 0.70, 0.40 };
        private static double[] StatMinimumPrimary = new double[29] { 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.20, 0.40, 0.40, 0.30, 0.00, 0.00, 0.00, 0.00, 0.00, 0.40, 0.40, 0.40, 0.40, 0.40, 0.40, 0.00, 0.40, 0.40, 0.40, 0.40, 0.40, 0.40 };

        ////												                  Dex  Int  Str  Vit  Life% LOH Steal%  LPS Magic% Gold% MSPD Rad. Sox Crit% CDam% ASPD Min+ Max+ Block% Thorn Allres Res   DPS  ARMOR Disc.Mana Arc. Regen  Globes
        //private static double[] MaxPointsWeaponOneHand = new double[29] { 400, 400, 400, 400, 0,   1000, 3,      0,    0,    0,   0,   0,   1,   0,   85,   0,   0,   0,    0,     0,    0,    0,  1500, 0,     10, 150, 10,   14, 0 };
        //private static double[] MaxPointsWeaponTwoHand = new double[29] { 530, 530, 530, 530, 0,   1800, 5.8,    0,    0,    0,   0,   0,   1,   0,  170,  0,   0,   0,    0,     0,    0,    0,  1700, 0,     10, 119, 10,   14, 0 };
        //private static double[] MaxPointsWeaponRanged = new double[29]  { 320, 320, 320, 320, 0,    850, 2.8,    0,    0,    0,   0,   0,   1,   0,   85, 0, 0, 0, 0, 0, 0, 0, 1410, 0, 0, 0, 0, 14, 0 };
        //private static double[] MaxPointsOffHand = new double[29]       { 300, 300, 300, 300, 9,      0, 0,    234,   18,   20,   0,   0,   1, 8.5,    0, 15, 110, 402, 0, 979, 0, 0, 0, 0, 10, 119, 10, 11, 5977 };
        //private static double[] MaxPointsShield = new double[29]        { 330, 330, 330, 330, 16,     0, 0,    342,   20,   25,   0,   0,   1,  10,    0, 0, 0, 0, 30, 2544, 80, 60, 0, 397, 0, 0, 0, 0, 12794 };
        //private static double[] MaxPointsRing = new double[29]          { 200, 200, 200, 200, 12,   479, 0,    234,   20,   25,   0,   0,   1,   6,   50, 9, 36, 100, 0, 979, 80, 60, 0, 240, 0, 0, 0, 0, 5977 };
        //private static double[] MaxPointsAmulet = new double[29]        { 350, 350, 350, 350, 16,   959, 0,    410,   45,   50,   0,   0,   1,  10,  100, 9, 36, 100, 0, 1712, 80, 60, 0, 360, 0, 0, 0, 0, 5977 };
        //private static double[] MaxPointsShoulders = new double[29]     { 200, 200, 300, 200, 12,      0, 0,   342,   20,   25,   0,   7,   0,   0,    0, 0, 0, 0, 0, 2544, 80, 60, 0, 265, 0, 0, 0, 0, 12794 };
        //private static double[] MaxPointsHelm = new double[29]          { 200, 300, 200, 200, 12,      0, 0,   342,   20,   25,   0,   7,   1,   6,    0, 0, 0, 0, 0, 1454, 80, 60, 0, 397, 0, 0, 0, 0, 12794 };
        //private static double[] MaxPointsPants = new double[29]         { 200, 200, 200, 300, 0,       0, 0,   342,   20,   25,   0,   7,   2,   0,    0, 0, 0, 0, 0, 1454, 80, 60, 0, 397, 0, 0, 0, 0, 12794 };
        //private static double[] MaxPointsGloves = new double[29]        { 300, 300, 200, 200, 0,       0, 0,   342,   20,   25,   0,   7,   0,  10,   50, 9, 0, 0, 0, 1454, 80, 60, 0, 265, 0, 0, 0, 0, 12794 };
        //private static double[] MaxPointsChest = new double[29]         { 200, 200, 200, 300, 12,      0, 0,   599,   20,   25,   0,   7,   3,   0,    0, 0, 0, 0, 0, 2544, 80, 60, 0, 397, 0, 0, 0, 0, 12794 };
        //private static double[] MaxPointsBracer = new double[29]        { 200, 200, 200, 200, 0,       0, 0,   342,   20,   25,   0,   7,   0,   6,    0, 0, 0, 0, 0, 1454, 80, 60, 0, 265, 0, 0, 0, 0, 12794 };
        //private static double[] MaxPointsBoots = new double[29]         { 300, 200, 200, 200, 0,       0, 0,   342,   20,   25,   12,  7,   0,   0,    0, 0, 0, 0, 0, 1454, 80, 60, 0, 265, 0, 0, 0, 0, 12794 };
        //private static double[] MaxPointsBelt = new double[29]          { 200, 200, 300, 200, 12,      0, 0,   342,   20,   25,   0,   7,   0,   0,    0, 0, 0, 0, 0, 2544, 80, 60, 0, 265, 0, 0, 0, 0, 12794 };
        //private static double[] MaxPointsCloak = new double[29]         { 200, 200, 200, 300, 12,      0, 0,   410,   20,   25,   0,   7,   3,   0,    0, 0, 0, 0, 0, 2544, 70, 50, 0, 397, 10, 0, 0, 0, 12794 };
        //private static double[] MaxPointsMightyBelt = new double[29]    { 200, 200, 300, 200, 12,      0, 3,   342,   20,   25,   0,   7,   0,   0,    0, 0, 0, 0, 0, 2544, 70, 50, 0, 265, 0, 0, 0, 0, 12794 };
        //private static double[] MaxPointsSpiritStone = new double[29]   { 200, 300, 200, 200, 12,      0, 0,   342,   20,   25,   0,   7,   1,   6,    0, 0, 0, 0, 0, 1454, 70, 50, 0, 397, 0, 0, 0, 0, 12794 };
        //private static double[] MaxPointsVoodooMask = new double[29]    { 200, 300, 200, 200, 12,      0, 0,   342,   20,   25,   0,   7,   1,   6,    0, 0, 0, 0, 0, 1454, 70, 50, 0, 397, 0, 119, 0, 11, 12794 };
        //private static double[] MaxPointsWizardHat = new double[29]     { 200, 300, 200, 200, 12,      0, 0,   342,   20,   25,   0,   7,   1,   6,    0, 0, 0, 0, 0, 1454, 70, 50, 0, 397, 0, 0, 10, 0, 12794 };
        //private static double[] MaxPointsFollower = new double[29]      { 300, 300, 300, 200, 0,     300, 0,   234,   0,     0,   0,   0,   0,   0, 55, 0, 0, 0, 0, 0, 50, 40, 0, 0, 0, 0, 0, 0, 0 };
        //// Stores the total points this stat is worth at the above % point of maximum
        //// Note that these values get all sorts of bonuses, multipliers, and extra things applied in the actual scoring routine. These values are more of a "base" value.
        ////                                                              Dex    Int    Str    Vit    Life%  LOH    Steal% LPS   Magic%  Gold%  MSPD   Rad  Sox    Crit%  CDam%  ASPD   Min+  Max+ Block% Thorn Allres Res   DPS    ARMOR  Disc.  Mana  Arc.  Regen  Globes
        //private static double[] WeaponPointsAtMax = new double[29] { 14000, 14000, 14000, 14000, 13000, 20000, 7000, 1000, 6000, 6000, 6000, 500, 16000, 15000, 15000, 0, 0, 0, 0, 1000, 11000, 0, 64000, 0, 10000, 8500, 8500, 10000, 8000 };
        ////                                                              Dex    Int    Str    Vit    Life%  LOH    Steal% LPS   Magic%  Gold%  MSPD   Rad. Sox    Crit%  CDam%  ASPD   Min+  Max+ Block% Thorn Allres Res   DPS    ARMOR  Disc.  Mana  Arc.  Regen  Globes
        //private static double[] ArmorPointsAtMax = new double[29] { 11000, 11000, 11000, 9500, 9000, 10000, 4000, 1200, 3000, 3000, 3500, 1000, 4300, 9000, 6100, 7000, 3000, 3000, 5000, 1200, 7500, 1500, 0, 5000, 4000, 3000, 3000, 6000, 5000 };
        //private static double[] JewelryPointsAtMax = new double[29] { 11500, 11000, 11000, 10000, 8000, 11000, 4000, 1200, 4500, 4500, 3500, 1000, 3500, 7500, 6300, 6800, 800, 800, 5000, 1200, 7500, 1500, 0, 4500, 4000, 3000, 3000, 6000, 5000 };
        //// Some special values for score calculations
        //// BonusThreshold is a percentage of the "max-stat possible", that the stat starts to get a multiplier on it's score. 1 means it has to be above 100% of the "max-stat" to get a multiplier (so only possible if the max-stat isn't ACTUALLY the max possible)
        //// MinimumThreshold is a percentage of the "max-stat possible", that the stat will simply be ignored for being too low. eg if set to .5 - then anything less than 50% of the max-stat will be ignored.
        //// MinimumPrimary is used for some stats only - and means that at least ONE primary stat has to be above that level, to get score. Eg magic-find has .5 - meaning any item without at least 50% of a max-stat primary, will ignore magic-find scoring.
        ////                                                             Dex  Int  Str  Vit  Life%  LOH  Steal%   LPS Magic% Gold% MSPD Radi  Sox  Crit% CDam% ASPD  Min+  Max+  Block%  Thorn  Allres  Res   DPS  ARMOR   Disc. Mana  Arc. Regen  Globes
        //private static double[] BonusThreshold = new double[29] { .75, .75, .75, .75, .80, .70, .8, 1, 1, 1, .95, 1, 1, .70, .90, 1, .9, .9, .83, 1, .85, .95, .80, .90, 1, 1, 1, .9, 1.0 };
        //private static double[] MinimumThreshold = new double[29] { .40, .40, .40, .30, .60, .35, .6, .7, .40, .40, .75, .8, .4, .40, .60, .40, .2, .2, .65, .6, .40, .55, .80, .80, .7, .7, .7, .7, .40 };
        //private static double[] StatMinimumPrimary = new double[29] { 0, 0, 0, 0, 0, 0, 0, .2, .40, .40, .30, 0, 0, 0, 0, 0, .40, .40, .40, .40, .40, .40, 0, .40, .40, .40, .40, .4, .40 };
    }
}
