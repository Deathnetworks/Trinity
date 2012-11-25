using System;
using Zeta.Common.Plugins;
using Zeta.Internals.Actors;
namespace GilesTrinity
{
    public partial class GilesTrinity : IPlugin
    {
        private static double[] ItemMaxStats = new double[TOTALSTATS];
        private static double[] ItemMaxPoints = new double[TOTALSTATS];
        private static bool IsInvalidItem = true;
        private static double TotalItemPoints = 0;
        private static GBaseItemType baseItemType = GBaseItemType.Unknown;
        private static double BestFinalBonus = 1d;

        // Constants for convenient stat names
        private static double[] HadStat = new double[TOTALSTATS] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        private static double[] HadPoints = new double[TOTALSTATS] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        private static double SafeLifePercentage = 0;
        private static bool SocketsCanReplacePrimaries = false;
        private static double HighestScoringPrimary = 0;
        private static int WhichPrimaryIsHighest = 0;
        private static double AmountHighestScoringPrimary = 0;
        // End of main 0-TOTALSTATS stat loop
        private static int TotalRequirements;
        private static double GlobalMultiplier = 1;

        /// <summary>
        /// The bizarre mystery function to score your lovely items!
        /// </summary>
        /// <param name="item"></param>
        /// <param name="itemType"></param>
        /// <returns></returns>
        private static double ValueThisItem(GilesCachedACDItem item, GItemType itemType)
        {
            TotalItemPoints = 0;
            IsInvalidItem = true;
            ItemMaxStats = new double[TOTALSTATS];
            ItemMaxPoints = new double[TOTALSTATS];
            baseItemType = DetermineBaseType(itemType);

            CheckForInvalidItemType(itemType);

            // Double safety check for unidentified items
            if (item.IsUnidentified)
                IsInvalidItem = true;

            // Make sure we got a valid item here, otherwise score it a big fat 0
            if (IsInvalidItem)
            {
                if (fullItemAnalysis) 
                    Log("-- Invalid Item Type or Unidentified?");
                return 0;
            }

            if (fullItemAnalysis)
                Log("NEXT ITEM= " + item.RealName + " - " + item.InternalName + " [" + baseItemType.ToString() + " - " + itemType.ToString() + "]");

            ValueItemStatString = "";
            junkItemStatString = "";

            // We loop through all of the stats, in a particular order. The order *IS* important, because it pulls up primary stats first, BEFORE other stats
            for (int i = 0; i <= (TOTALSTATS - 1); i++)
            {
                double TempStatistic = 0;

                // Now we lookup each stat on this item we are scoring, and store it in the variable "iTempStatistic" - which is used for calculations further down
                switch (i)
                {
                    case DEXTERITY: TempStatistic = item.Dexterity; break;
                    case INTELLIGENCE: TempStatistic = item.Intelligence; break;
                    case STRENGTH: TempStatistic = item.Strength; break;
                    case VITALITY: TempStatistic = item.Vitality; break;
                    case LIFEPERCENT: TempStatistic = item.LifePercent; break;
                    case LIFEONHIT: TempStatistic = item.LifeOnHit; break;
                    case LIFESTEAL: TempStatistic = item.LifeSteal; break;
                    case LIFEREGEN: TempStatistic = item.HealthPerSecond; break;
                    case MAGICFIND: TempStatistic = item.MagicFind; break;
                    case GOLDFIND: TempStatistic = item.GoldFind; break;
                    case MOVEMENTSPEED: TempStatistic = item.MovementSpeed; break;
                    case PICKUPRADIUS: TempStatistic = item.PickUpRadius; break;
                    case SOCKETS: TempStatistic = item.Sockets; break;
                    case CRITCHANCE: TempStatistic = item.CritPercent; break;
                    case CRITDAMAGE: TempStatistic = item.CritDamagePercent; break;
                    case ATTACKSPEED: TempStatistic = item.AttackSpeedPercent; break;
                    case MINDAMAGE: TempStatistic = item.MinDamage; break;
                    case MAXDAMAGE: TempStatistic = item.MaxDamage; break;
                    case BLOCKCHANCE: TempStatistic = item.BlockChance; break;
                    case THORNS: TempStatistic = item.Thorns; break;
                    case ALLRESIST: TempStatistic = item.ResistAll; break;
                    case RANDOMRESIST:
                        //intell -- sugerir
                        if (item.ResistArcane > TempStatistic) TempStatistic = item.ResistArcane;
                        if (item.ResistCold > TempStatistic) TempStatistic = 0;
                        //thisitem.ResistCold;
                        if (item.ResistFire > TempStatistic) TempStatistic = item.ResistFire;
                        if (item.ResistHoly > TempStatistic) TempStatistic = item.ResistHoly;
                        if (item.ResistLightning > TempStatistic) TempStatistic = 0;
                        //thisitem.ResistLightning;
                        if (item.ResistPhysical > TempStatistic) TempStatistic = item.ResistPhysical;
                        if (item.ResistPoison > TempStatistic) TempStatistic = 0;
                        //thisitem.ResistPoison;
                        break;
                    case TOTALDPS: TempStatistic = item.WeaponDamagePerSecond; break;
                    case ARMOR: TempStatistic = item.ArmorBonus; break;
                    case MAXDISCIPLINE: TempStatistic = item.MaxDiscipline; break;
                    case MAXMANA: TempStatistic = item.MaxMana; break;
                    case ARCANECRIT: TempStatistic = item.ArcaneOnCrit; break;
                    case MANAREGEN: TempStatistic = item.ManaRegen; break;
                    case GLOBEBONUS: TempStatistic = item.GlobeBonus; break;
                }
                HadStat[i] = TempStatistic;
                HadPoints[i] = 0;

                // Now we check that the current statistic in the "for" loop, actually exists on this item, and is a stat we are measuring (has >0 in the "max stats" array)
                if (ItemMaxStats[i] > 0 && TempStatistic > 0)
                {

                    // Final bonus granted is an end-of-score multiplier. 1 = 100%, so all items start off with 100%, of course!
                    double FinalBonusGranted = 1;

                    // Temp percent is what PERCENTAGE of the *MAXIMUM POSSIBLE STAT*, this stat is at.

                    // Note that stats OVER the max will get a natural score boost, since this value will be over 1!
                    double itemStatRatio = TempStatistic / ItemMaxStats[i];

                    // Now multiply the "max points" value, by that percentage, as the start/basis of the scoring for this statistic
                    double iTempPoints = ItemMaxPoints[i] * itemStatRatio;

                    if (fullItemAnalysis) 
                        Log("--- " + StatNames[i] + ": " + TempStatistic.ToString() + " out of " + ItemMaxStats[i].ToString() + " (" + ItemMaxPoints[i].ToString() + " * " + itemStatRatio.ToString() + " = " + iTempPoints.ToString() + ")");

                    // Check if this statistic is over the "bonus threshold" array value for this stat - if it is, then it gets a score bonus when over a certain % of max-stat
                    if (itemStatRatio > BonusThreshold[i] && BonusThreshold[i] > 0f)
                    {
                        FinalBonusGranted += ((itemStatRatio - BonusThreshold[i]) * 0.9);
                    }

                    // We're going to store the life % stat here for quick-calculations against other stats. Don't edit this bit!
                    if (i == LIFEPERCENT)
                    {
                        if (ItemMaxStats[LIFEPERCENT] > 0)
                        {
                            SafeLifePercentage = (TempStatistic / ItemMaxStats[LIFEPERCENT]);
                        }
                        else
                        {
                            SafeLifePercentage = 0;
                        }
                    }

                    // This *REMOVES* score from follower items for stats that followers don't care about
                    if (baseItemType == GBaseItemType.FollowerItem && (i == CRITDAMAGE || i == LIFEONHIT || i == ALLRESIST))
                        FinalBonusGranted -= 0.9;

                    // Bonus 15% for being *at* the stat cap (ie - completely maxed out, or very very close to), but not for the socket stat (since sockets are usually 0 or 1!)
                    if (i != SOCKETS)
                    {
                        if ((TempStatistic / ItemMaxStats[i]) >= 0.99)
                            FinalBonusGranted += 0.15;

                        // Else bonus 10% for being in final 95%
                        else if ((TempStatistic / ItemMaxStats[i]) >= 0.95)
                            FinalBonusGranted += 0.10;
                    }

                    // Socket handling

                    // Sockets give special bonuses for certain items, depending how close to the max-socket-count it is for that item

                    // It also enables bonus scoring for stats which usually rely on a high primary stat - since a socket can make up for a lack of a high primary (you can socket a +primary stat!)
                    if (i == SOCKETS)
                    {

                        // Off-handers get less value from sockets
                        if (baseItemType == GBaseItemType.Offhand)
                        {
                            FinalBonusGranted -= 0.35;
                        }

                        // Chest
                        if (itemType == GItemType.Chest || itemType == GItemType.Cloak)
                        {
                            if (TempStatistic >= 2)
                            {
                                SocketsCanReplacePrimaries = true;
                                if (TempStatistic >= 3)
                                    FinalBonusGranted += 0.25;
                            }
                        }

                        // Pants
                        if (itemType == GItemType.Pants)
                        {
                            if (TempStatistic >= 2)
                            {
                                SocketsCanReplacePrimaries = true;
                                FinalBonusGranted += 0.25;
                            }
                        }

                        // Helmets can have a bonus for a socket since it gives amazing MF/GF
                        if (TempStatistic >= 1 && (itemType == GItemType.Helm || itemType == GItemType.WizardHat || itemType == GItemType.VoodooMask ||
                            itemType == GItemType.SpiritStone))
                        {
                            SocketsCanReplacePrimaries = true;
                        }

                        // And rings and amulets too
                        if (TempStatistic >= 1 && (itemType == GItemType.Ring || itemType == GItemType.Amulet))
                        {
                            SocketsCanReplacePrimaries = true;
                        }
                    }

                    // Right, here's quite a long bit of code, but this is basically all about granting all sorts of bonuses based on primary stat values of all different ranges

                    // For all item types *EXCEPT* weapons
                    if (baseItemType != GBaseItemType.WeaponRange && baseItemType != GBaseItemType.WeaponOneHand && baseItemType != GBaseItemType.WeaponTwoHand)
                    {
                        double SpecialBonus = 0;
                        if (i > LIFEPERCENT)
                        {

                            // Knock off points for being particularly low
                            if ((TempStatistic / ItemMaxStats[i]) < 0.2 && (BonusThreshold[i] <= 0f || BonusThreshold[i] >= 0.2))
                                FinalBonusGranted -= 0.35;
                            else if ((TempStatistic / ItemMaxStats[i]) < 0.4 && (BonusThreshold[i] <= 0f || BonusThreshold[i] >= 0.4))
                                FinalBonusGranted -= 0.15;

                            // Remove 80% if below minimum threshold
                            if ((TempStatistic / ItemMaxStats[i]) < MinimumThreshold[i] && MinimumThreshold[i] > 0f)
                                FinalBonusGranted -= 0.8;

                            // Primary stat/vitality minimums or zero-check reductions on other stats
                            if (StatMinimumPrimary[i] > 0)
                            {

                                // Remove 40% from all stats if there is no prime stat present or vitality/life present and this is below 90% of max
                                if (((TempStatistic / ItemMaxStats[i]) < .90) && ((HadStat[DEXTERITY] / ItemMaxStats[DEXTERITY]) < StatMinimumPrimary[i]) &&
                                    ((HadStat[STRENGTH] / ItemMaxStats[STRENGTH]) < (StatMinimumPrimary[i] + 0.1)) && ((HadStat[INTELLIGENCE] / ItemMaxStats[INTELLIGENCE]) < StatMinimumPrimary[i]) &&
                                    ((HadStat[VITALITY] / ItemMaxStats[VITALITY]) < StatMinimumPrimary[i]) && (SafeLifePercentage < (StatMinimumPrimary[i] * 2.5)) && !SocketsCanReplacePrimaries)
                                {
                                    if (itemType != GItemType.Ring && itemType != GItemType.Amulet)
                                        FinalBonusGranted -= 0.4;
                                    else
                                        FinalBonusGranted -= 0.3;

                                    // And another 25% off for armor and all resist which are more useful with primaries, as long as not jewelry
                                    if ((i == ARMOR || i == ALLRESIST || i == RANDOMRESIST) && itemType != GItemType.Ring && itemType != GItemType.Amulet && !SocketsCanReplacePrimaries)
                                        FinalBonusGranted -= 0.15;
                                }
                            }
                            else
                            {

                                // Almost no primary stats or health at all
                                if (HadStat[DEXTERITY] <= 60 && HadStat[STRENGTH] <= 60 && HadStat[INTELLIGENCE] <= 60 && HadStat[VITALITY] <= 60 && SafeLifePercentage < 0.9 && !SocketsCanReplacePrimaries)
                                {

                                    // So 35% off for all items except jewelry which is 20% off
                                    if (itemType != GItemType.Ring && itemType != GItemType.Amulet)
                                    {
                                        FinalBonusGranted -= 0.35;

                                        // And another 25% off for armor and all resist which are more useful with primaries
                                        if (i == ARMOR || i == ALLRESIST)
                                            FinalBonusGranted -= 0.15;
                                    }
                                    else
                                    {
                                        FinalBonusGranted -= 0.20;
                                    }
                                }
                            }
                            if (baseItemType == GBaseItemType.Armor || baseItemType == GBaseItemType.Jewelry)
                            {

                                // Grant a 50% bonus to stats if a primary is above 200 AND (vitality above 200 or life% within 90% max)
                                if ((HadStat[DEXTERITY] > 200 || HadStat[STRENGTH] > 200 || HadStat[INTELLIGENCE] > 200) && (HadStat[VITALITY] > 200 || SafeLifePercentage > .97))
                                {
                                    if (0.5 > SpecialBonus) SpecialBonus = 0.5;
                                }

                                // Else grant a 40% bonus to stats if a primary is above 200
                                if (HadStat[DEXTERITY] > 200 || HadStat[STRENGTH] > 200 || HadStat[INTELLIGENCE] > 200)
                                {
                                    if (0.4 > SpecialBonus) SpecialBonus = 0.4;
                                }

                                // Grant a 30% bonus if vitality > 200 or life percent within 90% of max
                                if (HadStat[VITALITY] > 200 || SafeLifePercentage > .97)
                                {
                                    if (0.3 > SpecialBonus) SpecialBonus = 0.3;
                                }
                            }

                            // Checks for various primary & health levels
                            if ((HadStat[DEXTERITY] / ItemMaxStats[DEXTERITY]) > .85 || (HadStat[STRENGTH] / ItemMaxStats[STRENGTH]) > .85 || (HadStat[INTELLIGENCE] / ItemMaxStats[INTELLIGENCE]) > .85)
                            {
                                if ((HadStat[VITALITY] / ItemMaxStats[VITALITY]) > .6 || SafeLifePercentage > .90)
                                {
                                    if (0.5 > SpecialBonus) SpecialBonus = 0.5;
                                }
                                else if ((HadStat[VITALITY] / ItemMaxStats[VITALITY]) > .35 || SafeLifePercentage > .85)
                                {
                                    if (0.4 > SpecialBonus) SpecialBonus = 0.4;
                                }
                                else
                                {
                                    if (0.2 > SpecialBonus) SpecialBonus = 0.2;
                                }
                            }
                            if ((HadStat[DEXTERITY] / ItemMaxStats[DEXTERITY]) > .75 || (HadStat[STRENGTH] / ItemMaxStats[STRENGTH]) > .75 || (HadStat[INTELLIGENCE] / ItemMaxStats[INTELLIGENCE]) > .75)
                            {
                                if ((HadStat[VITALITY] / ItemMaxStats[VITALITY]) > .6 || SafeLifePercentage > .90)
                                {
                                    if (0.35 > SpecialBonus) SpecialBonus = 0.35;
                                }
                                else if ((HadStat[VITALITY] / ItemMaxStats[VITALITY]) > .35 || SafeLifePercentage > .85)
                                {
                                    if (0.30 > SpecialBonus) SpecialBonus = 0.30;
                                }
                                else
                                {
                                    if (0.15 > SpecialBonus) SpecialBonus = 0.15;
                                }
                            }
                            if ((HadStat[DEXTERITY] / ItemMaxStats[DEXTERITY]) > .65 || (HadStat[STRENGTH] / ItemMaxStats[STRENGTH]) > .65 || (HadStat[INTELLIGENCE] / ItemMaxStats[INTELLIGENCE]) > .65)
                            {
                                if ((HadStat[VITALITY] / ItemMaxStats[VITALITY]) > .6 || SafeLifePercentage > .90)
                                {
                                    if (0.26 > SpecialBonus) SpecialBonus = 0.26;
                                }
                                else if ((HadStat[VITALITY] / ItemMaxStats[VITALITY]) > .35 || SafeLifePercentage > .85)
                                {
                                    if (0.22 > SpecialBonus) SpecialBonus = 0.22;
                                }
                                else
                                {
                                    if (0.11 > SpecialBonus) SpecialBonus = 0.11;
                                }
                            }
                            if ((HadStat[DEXTERITY] / ItemMaxStats[DEXTERITY]) > .55 || (HadStat[STRENGTH] / ItemMaxStats[STRENGTH]) > .55 || (HadStat[INTELLIGENCE] / ItemMaxStats[INTELLIGENCE]) > .55)
                            {
                                if ((HadStat[VITALITY] / ItemMaxStats[VITALITY]) > .6 || SafeLifePercentage > .90)
                                {
                                    if (0.18 > SpecialBonus) SpecialBonus = 0.18;
                                }
                                else if ((HadStat[VITALITY] / ItemMaxStats[VITALITY]) > .35 || SafeLifePercentage > .85)
                                {
                                    if (0.14 > SpecialBonus) SpecialBonus = 0.14;
                                }
                                else
                                {
                                    if (0.08 > SpecialBonus) SpecialBonus = 0.08;
                                }
                            }
                            if ((HadStat[DEXTERITY] / ItemMaxStats[DEXTERITY]) > .5 || (HadStat[STRENGTH] / ItemMaxStats[STRENGTH]) > .5 || (HadStat[INTELLIGENCE] / ItemMaxStats[INTELLIGENCE]) > .5)
                            {
                                if ((HadStat[VITALITY] / ItemMaxStats[VITALITY]) > .6 || SafeLifePercentage > .90)
                                {
                                    if (0.12 > SpecialBonus) SpecialBonus = 0.12;
                                }
                                else if ((HadStat[VITALITY] / ItemMaxStats[VITALITY]) > .35 || SafeLifePercentage > .85)
                                {
                                    if (0.05 > SpecialBonus) SpecialBonus = 0.05;
                                }
                                else
                                {
                                    if (0.03 > SpecialBonus) SpecialBonus = 0.03;
                                }
                            }
                            if (itemType == GItemType.Ring || itemType == GItemType.Amulet)
                            {
                                if ((HadStat[DEXTERITY] / ItemMaxStats[DEXTERITY]) > .4 || (HadStat[STRENGTH] / ItemMaxStats[STRENGTH]) > .4 || (HadStat[INTELLIGENCE] / ItemMaxStats[INTELLIGENCE]) > .4)
                                {
                                    if ((HadStat[VITALITY] / ItemMaxStats[VITALITY]) > .6 || SafeLifePercentage > .90)
                                    {
                                        if (0.10 > SpecialBonus) SpecialBonus = 0.10;
                                    }
                                    else if ((HadStat[VITALITY] / ItemMaxStats[VITALITY]) > .35 || SafeLifePercentage > .85)
                                    {
                                        if (0.08 > SpecialBonus) SpecialBonus = 0.08;
                                    }
                                    else
                                    {
                                        if (0.05 > SpecialBonus) SpecialBonus = 0.05;
                                    }
                                }
                            }
                            if ((HadStat[VITALITY] / ItemMaxStats[VITALITY]) > .8 || SafeLifePercentage > .98)
                            {
                                if (0.20 > SpecialBonus) SpecialBonus = 0.20;
                            }
                            if ((HadStat[VITALITY] / ItemMaxStats[VITALITY]) > .7 || SafeLifePercentage > .95)
                            {
                                if (0.16 > SpecialBonus) SpecialBonus = 0.16;
                            }
                            if ((HadStat[VITALITY] / ItemMaxStats[VITALITY]) > .6 || SafeLifePercentage > .92)
                            {
                                if (0.12 > SpecialBonus) SpecialBonus = 0.12;
                            }
                            if ((HadStat[VITALITY] / ItemMaxStats[VITALITY]) > .55 || SafeLifePercentage > .89)
                            {
                                if (0.07 > SpecialBonus) SpecialBonus = 0.07;
                            }
                            else if ((HadStat[VITALITY] / ItemMaxStats[VITALITY]) > .5 || SafeLifePercentage > .87)
                            {
                                if (0.05 > SpecialBonus) SpecialBonus = 0.05;
                            }
                            else if ((HadStat[VITALITY] / ItemMaxStats[VITALITY]) > .45 || SafeLifePercentage > .86)
                            {
                                if (0.02 > SpecialBonus) SpecialBonus = 0.02;
                            }
                        }

                        // This stat is one after life percent stat

                        // Shields get less of a special bonus from high prime stats
                        if (itemType == GItemType.Shield)
                            SpecialBonus *= 0.7;
                        if (fullItemAnalysis) Log("------- special bonus =" + SpecialBonus.ToString(), true);
                        FinalBonusGranted += SpecialBonus;
                    }

                    // NOT A WEAPON!?
                    //intell -- sugerir
                    if (i == LIFESTEAL && itemType == GItemType.MightyBelt)
                        FinalBonusGranted += 0.3;

                    // Knock off points for being particularly low
                    if ((TempStatistic / ItemMaxStats[i]) < MinimumThreshold[i] && MinimumThreshold[i] > 0f)
                        FinalBonusGranted -= 0.35;

                    // Grant a 20% bonus to vitality or Life%, for being paired with any prime stat above minimum threshold +.1
                    if (((i == VITALITY && (TempStatistic / ItemMaxStats[VITALITY]) > MinimumThreshold[VITALITY]) ||
                          i == LIFEPERCENT && (TempStatistic / ItemMaxStats[LIFEPERCENT]) > MinimumThreshold[LIFEPERCENT]) &&
                        ((HadStat[DEXTERITY] / ItemMaxStats[DEXTERITY]) > (MinimumThreshold[DEXTERITY] + 0.1) || (HadStat[STRENGTH] / ItemMaxStats[STRENGTH]) > (MinimumThreshold[STRENGTH] + 0.1) ||
                         (HadStat[INTELLIGENCE] / ItemMaxStats[INTELLIGENCE]) > (MinimumThreshold[INTELLIGENCE] + 0.1)))
                        FinalBonusGranted += 0.2;

                    // Blue item point reduction for non-weapons
                    if (item.Quality < ItemQuality.Rare4 && (baseItemType == GBaseItemType.Armor || baseItemType == GBaseItemType.Offhand ||
                        baseItemType == GBaseItemType.Jewelry || baseItemType == GBaseItemType.FollowerItem) && ((TempStatistic / ItemMaxStats[i]) < 0.88))
                        FinalBonusGranted -= 0.9;

                    // Special all-resist bonuses
                    if (i == ALLRESIST)
                    {

                        // Shields with < 60% max all resist, lost some all resist score
                        if (itemType == GItemType.Shield && (TempStatistic / ItemMaxStats[i]) <= 0.6)
                            FinalBonusGranted -= 0.30;
                        double iSpecialBonus = 0;

                        // All resist gets a special bonus if paired with good strength and some vitality
                        if ((HadStat[STRENGTH] / ItemMaxStats[STRENGTH]) > 0.7 && (HadStat[VITALITY] / ItemMaxStats[VITALITY]) > 0.3)
                            if (0.45 > iSpecialBonus) iSpecialBonus = 0.45;

                        // All resist gets a smaller special bonus if paired with good dexterity and some vitality
                        if ((HadStat[DEXTERITY] / ItemMaxStats[DEXTERITY]) > 0.7 && (HadStat[VITALITY] / ItemMaxStats[VITALITY]) > 0.3)
                            if (0.35 > iSpecialBonus) iSpecialBonus = 0.35;

                        // All resist gets a slight special bonus if paired with good intelligence and some vitality
                        if ((HadStat[INTELLIGENCE] / ItemMaxStats[INTELLIGENCE]) > 0.7 && (HadStat[INTELLIGENCE] / ItemMaxStats[INTELLIGENCE]) > 0.3)
                            if (0.25 > iSpecialBonus) iSpecialBonus = 0.25;

                        // Smaller bonuses for smaller stats

                        // All resist gets a special bonus if paired with good strength and some vitality
                        if ((HadStat[STRENGTH] / ItemMaxStats[STRENGTH]) > 0.55 && (HadStat[VITALITY] / ItemMaxStats[VITALITY]) > 0.3)
                            if (0.45 > iSpecialBonus) iSpecialBonus = 0.20;

                        // All resist gets a smaller special bonus if paired with good dexterity and some vitality
                        if ((HadStat[DEXTERITY] / ItemMaxStats[DEXTERITY]) > 0.55 && (HadStat[VITALITY] / ItemMaxStats[VITALITY]) > 0.3)
                            if (0.35 > iSpecialBonus) iSpecialBonus = 0.15;

                        // All resist gets a slight special bonus if paired with good intelligence and some vitality
                        if ((HadStat[INTELLIGENCE] / ItemMaxStats[INTELLIGENCE]) > 0.55 && (HadStat[INTELLIGENCE] / ItemMaxStats[INTELLIGENCE]) > 0.3)
                            if (0.25 > iSpecialBonus) iSpecialBonus = 0.10;

                        // This stat is one after life percent stat
                        FinalBonusGranted += iSpecialBonus;

                        // Global bonus to everything
                        if ((ItemMaxStats[i] - TempStatistic) < 10.2f)
                            GlobalMultiplier += 0.05;
                    }

                    // All resist special bonuses
                    if (itemType != GItemType.Ring && itemType != GItemType.Amulet)
                    {

                        // Shields get 10% less on everything
                        if (itemType == GItemType.Shield)
                            FinalBonusGranted -= 0.10;

                        // Prime stat gets a 20% bonus if 50 from max possible
                        if ((i == DEXTERITY || i == STRENGTH || i == INTELLIGENCE || i == VITALITY) && (ItemMaxStats[i] - TempStatistic) < 50.5f)
                            FinalBonusGranted += 0.25;

                        // Reduce a prime stat by 75% if less than 100 *OR* less than 50% max
                        if ((i == DEXTERITY || i == STRENGTH || i == INTELLIGENCE) && (TempStatistic < 100 || ((TempStatistic / ItemMaxStats[i]) < 0.5)))
                            FinalBonusGranted -= 0.75;

                        // Reduce a vitality/life% stat by 60% if less than 80 vitality/less than 60% max possible life%
                        if ((i == VITALITY && TempStatistic < 80) || (i == LIFEPERCENT && ((TempStatistic / ItemMaxStats[LIFEPERCENT]) < 0.6)))
                            FinalBonusGranted -= 0.6;

                        // Grant 10% to any 4 main stat above 200
                        if ((i == DEXTERITY || i == STRENGTH || i == INTELLIGENCE || i == VITALITY) && TempStatistic > 200)
                            FinalBonusGranted += 0.1;

                        // Special stat handling stuff for non-jewelry types

                        // Within 2 block chance
                        if (i == BLOCKCHANCE && (ItemMaxStats[i] - TempStatistic) < 2.3f)
                            FinalBonusGranted += 1;

                        // Within final 5 gold find
                        if (i == GOLDFIND && (ItemMaxStats[i] - TempStatistic) < 5.3f)
                        {
                            FinalBonusGranted += 0.04;

                            // Even bigger bonus if got prime stat & vit
                            if (((HadStat[DEXTERITY] / ItemMaxStats[DEXTERITY]) > MinimumThreshold[DEXTERITY] || (HadStat[STRENGTH] / ItemMaxStats[STRENGTH]) > MinimumThreshold[STRENGTH] ||
                                (HadStat[INTELLIGENCE] / ItemMaxStats[INTELLIGENCE]) > MinimumThreshold[INTELLIGENCE]) && (HadStat[VITALITY] / ItemMaxStats[VITALITY]) > MinimumThreshold[VITALITY])
                                FinalBonusGranted += 0.02;
                        }

                        // Within final 3 gold find
                        if (i == GOLDFIND && (ItemMaxStats[i] - TempStatistic) < 3.3f)
                        {
                            FinalBonusGranted += 0.04;
                        }

                        // Within final 2 gold find
                        if (i == GOLDFIND && (ItemMaxStats[i] - TempStatistic) < 2.3f)
                        {
                            FinalBonusGranted += 0.05;
                        }

                        // Within final 3 magic find
                        if (i == MAGICFIND && (ItemMaxStats[i] - TempStatistic) < 3.3f)
                            FinalBonusGranted += 0.08;

                        // Within final 2 magic find
                        if (i == MAGICFIND && (ItemMaxStats[i] - TempStatistic) < 2.3f)
                        {
                            FinalBonusGranted += 0.04;

                            // Even bigger bonus if got prime stat & vit
                            if (((HadStat[DEXTERITY] / ItemMaxStats[DEXTERITY]) > MinimumThreshold[DEXTERITY] || (HadStat[STRENGTH] / ItemMaxStats[STRENGTH]) > MinimumThreshold[STRENGTH] ||
                                (HadStat[INTELLIGENCE] / ItemMaxStats[INTELLIGENCE]) > MinimumThreshold[INTELLIGENCE]) && (HadStat[VITALITY] / ItemMaxStats[VITALITY]) > MinimumThreshold[VITALITY])
                                FinalBonusGranted += 0.03;
                        }

                        // Within final magic find
                        if (i == MAGICFIND && (ItemMaxStats[i] - TempStatistic) < 1.3f)
                        {
                            FinalBonusGranted += 0.05;
                        }

                        // Within final 10 all resist
                        if (i == ALLRESIST && (ItemMaxStats[i] - TempStatistic) < 10.2f)
                        {
                            FinalBonusGranted += 0.05;

                            // Even bigger bonus if got prime stat & vit
                            if (((HadStat[DEXTERITY] / ItemMaxStats[DEXTERITY]) > MinimumThreshold[DEXTERITY] || (HadStat[STRENGTH] / ItemMaxStats[STRENGTH]) > MinimumThreshold[STRENGTH] ||
                                (HadStat[INTELLIGENCE] / ItemMaxStats[INTELLIGENCE]) > MinimumThreshold[INTELLIGENCE]) && (HadStat[VITALITY] / ItemMaxStats[VITALITY]) > MinimumThreshold[VITALITY])
                                FinalBonusGranted += 0.20;
                        }

                        // Within final 50 armor
                        if (i == ARMOR && (ItemMaxStats[i] - TempStatistic) < 50.2f)
                        {
                            FinalBonusGranted += 0.10;
                            if ((HadStat[STRENGTH] / ItemMaxStats[STRENGTH]) > MinimumThreshold[STRENGTH])
                                FinalBonusGranted += 0.10;
                        }

                        // Within final 15 armor
                        if (i == ARMOR && (ItemMaxStats[i] - TempStatistic) < 15.2f)
                            FinalBonusGranted += 0.15;

                        // Within final 5 critical hit damage
                        if (i == CRITDAMAGE && (ItemMaxStats[i] - TempStatistic) < 5.2f)
                            FinalBonusGranted += 0.25;

                        // More than 2.5 crit chance out
                        if (i == CRITCHANCE && (ItemMaxStats[i] - TempStatistic) > 2.45f)
                            FinalBonusGranted -= 0.35;

                        // More than 20 crit damage out
                        if (i == CRITDAMAGE && (ItemMaxStats[i] - TempStatistic) > 19.95f)
                            FinalBonusGranted -= 0.35;

                        // More than 2 attack speed out
                        if (i == ATTACKSPEED && (ItemMaxStats[i] - TempStatistic) > 1.95f)
                            FinalBonusGranted -= 0.35;

                        // More than 2 move speed
                        if (i == MOVEMENTSPEED && (ItemMaxStats[i] - TempStatistic) > 1.95f)
                            FinalBonusGranted -= 0.35;

                        // More than 5 gold find out
                        if (i == GOLDFIND && (ItemMaxStats[i] - TempStatistic) > 5.2f)
                            FinalBonusGranted -= 0.40;

                        // More than 8 gold find out
                        if (i == GOLDFIND && (ItemMaxStats[i] - TempStatistic) > 8.2f)
                            FinalBonusGranted -= 0.1;

                        // More than 5 magic find out
                        if (i == MAGICFIND && (ItemMaxStats[i] - TempStatistic) > 5.2f)
                            FinalBonusGranted -= 0.40;

                        // More than 7 magic find out
                        if (i == MAGICFIND && (ItemMaxStats[i] - TempStatistic) > 7.2f)
                            FinalBonusGranted -= 0.1;

                        // More than 20 all resist out
                        if (i == ALLRESIST && (ItemMaxStats[i] - TempStatistic) > 20.2f)
                            FinalBonusGranted -= 0.50;

                        // More than 30 all resist out
                        if (i == ALLRESIST && (ItemMaxStats[i] - TempStatistic) > 30.2f)
                            FinalBonusGranted -= 0.20;
                    }

                    // And now for jewelry checks...
                    else
                    {

                        // Global bonus to everything if jewelry has an all resist above 50%
                        if (i == ALLRESIST && (TempStatistic / ItemMaxStats[i]) > 0.5)
                            GlobalMultiplier += 0.08;

                        // Within final 10 all resist
                        if (i == ALLRESIST && (ItemMaxStats[i] - TempStatistic) < 10.2f)
                            FinalBonusGranted += 0.10;

                        // Within final 5 critical hit damage
                        if (i == CRITDAMAGE && (ItemMaxStats[i] - TempStatistic) < 5.2f)
                            FinalBonusGranted += 0.25;

                        // Within 3 block chance
                        if (i == BLOCKCHANCE && (ItemMaxStats[i] - TempStatistic) < 3.3f)
                            FinalBonusGranted += 0.15;

                        // Reduce a prime stat by 60% if less than 60
                        if ((i == DEXTERITY || i == STRENGTH || i == INTELLIGENCE) && (TempStatistic < 60 || ((TempStatistic / ItemMaxStats[i]) < 0.3)))
                            FinalBonusGranted -= 0.6;

                        // Reduce a vitality/life% stat by 50% if less than 50 vitality/less than 40% max possible life%
                        if ((i == VITALITY && TempStatistic < 50) || (i == LIFEPERCENT && ((TempStatistic / ItemMaxStats[LIFEPERCENT]) < 0.4)))
                            FinalBonusGranted -= 0.5;

                        // Grant 20% to any 4 main stat above 150
                        if ((i == DEXTERITY || i == STRENGTH || i == INTELLIGENCE || i == VITALITY) && TempStatistic > 150)
                            FinalBonusGranted += 0.2;

                        // Special stat handling stuff for jewelry
                        if (itemType == GItemType.Ring)
                        {

                            // Prime stat gets a 25% bonus if 30 from max possible
                            if ((i == DEXTERITY || i == STRENGTH || i == INTELLIGENCE || i == VITALITY) && (ItemMaxStats[i] - TempStatistic) < 30.5f)
                                FinalBonusGranted += 0.25;

                            // Within final 5 magic find
                            if (i == MAGICFIND && (ItemMaxStats[i] - TempStatistic) < 5.2f)
                                FinalBonusGranted += 0.4;

                            // Within final 5 gold find
                            if (i == GOLDFIND && (ItemMaxStats[i] - TempStatistic) < 5.2f)
                                FinalBonusGranted += 0.35;

                            // Within final 45 life on hit
                            if (i == LIFEONHIT && (ItemMaxStats[i] - TempStatistic) < 45.2f)
                                FinalBonusGranted += 1.2;

                        }
                        else
                        {

                            // Prime stat gets a 25% bonus if 60 from max possible
                            if ((i == DEXTERITY || i == STRENGTH || i == INTELLIGENCE || i == VITALITY) && (ItemMaxStats[i] - TempStatistic) < 60.5f)
                                FinalBonusGranted += 0.25;

                            // Within final 10 magic find
                            if (i == MAGICFIND && (ItemMaxStats[i] - TempStatistic) < 10.2f)
                                FinalBonusGranted += 0.4;

                            // Within final 10 gold find
                            if (i == GOLDFIND && (ItemMaxStats[i] - TempStatistic) < 10.2f)
                                FinalBonusGranted += 0.35;

                            // Within final 40 life on hit
                            if (i == LIFEONHIT && (ItemMaxStats[i] - TempStatistic) < 40.2f)
                                FinalBonusGranted += 1.2;

                        }

                        // Within final 50 armor
                        if (i == ARMOR && (ItemMaxStats[i] - TempStatistic) < 50.2f)
                        {
                            FinalBonusGranted += 0.30;
                            if ((HadStat[STRENGTH] / ItemMaxStats[STRENGTH]) > MinimumThreshold[STRENGTH])
                                FinalBonusGranted += 0.30;
                        }

                        // Within final 15 armor
                        if (i == ARMOR && (ItemMaxStats[i] - TempStatistic) < 15.2f)
                            FinalBonusGranted += 0.20;

                        // More than 2.5 crit chance out
                        if (i == CRITCHANCE && (ItemMaxStats[i] - TempStatistic) > 5.55f)
                            FinalBonusGranted -= 0.20;

                        // More than 20 crit damage out
                        if (i == CRITDAMAGE && (ItemMaxStats[i] - TempStatistic) > 19.95f)
                            FinalBonusGranted -= 0.20;

                        // More than 2 attack speed out
                        if (i == ATTACKSPEED && (ItemMaxStats[i] - TempStatistic) > 1.95f)
                            FinalBonusGranted -= 0.20;

                        // More than 15 gold find out
                        if (i == GOLDFIND && (ItemMaxStats[i] - TempStatistic) > 15.2f)
                            FinalBonusGranted -= 0.1;

                        // More than 15 magic find out
                        if (i == MAGICFIND && (ItemMaxStats[i] - TempStatistic) > 15.2f)
                            FinalBonusGranted -= 0.1;

                        // More than 30 all resist out
                        if (i == ALLRESIST && (ItemMaxStats[i] - TempStatistic) > 20.2f)
                            FinalBonusGranted -= 0.1;

                        // More than 40 all resist out
                        if (i == ALLRESIST && (ItemMaxStats[i] - TempStatistic) > 30.2f)
                            FinalBonusGranted -= 0.1;
                    }

                    // All the "set to 0" checks now

                    // Disable specific primary stat scoring for certain class-specific item types
                    if ((itemType == GItemType.VoodooMask || itemType == GItemType.WizardHat || itemType == GItemType.Wand ||
                        itemType == GItemType.CeremonialKnife || itemType == GItemType.Mojo || itemType == GItemType.Source)
                        && (i == STRENGTH || i == DEXTERITY))
                        FinalBonusGranted = 0;
                    if ((itemType == GItemType.Quiver || itemType == GItemType.HandCrossbow || itemType == GItemType.Cloak ||
                        itemType == GItemType.SpiritStone || itemType == GItemType.TwoHandDaibo || itemType == GItemType.FistWeapon)
                        && (i == STRENGTH || i == INTELLIGENCE))
                        FinalBonusGranted = 0;
                    if ((itemType == GItemType.MightyBelt || itemType == GItemType.MightyWeapon || itemType == GItemType.TwoHandMighty)
                        && (i == DEXTERITY || i == INTELLIGENCE))
                        FinalBonusGranted = 0;

                    // Remove unwanted follower stats for specific follower types
                    if (itemType == GItemType.FollowerEnchantress && (i == STRENGTH || i == DEXTERITY))
                        FinalBonusGranted = 0;
                    if (itemType == GItemType.FollowerEnchantress && (i == INTELLIGENCE || i == VITALITY))
                        FinalBonusGranted -= 0.4;
                    if (itemType == GItemType.FollowerScoundrel && (i == STRENGTH || i == INTELLIGENCE))
                        FinalBonusGranted = 0;
                    if (itemType == GItemType.FollowerScoundrel && (i == DEXTERITY || i == VITALITY))
                        FinalBonusGranted -= 0.4;
                    if (itemType == GItemType.FollowerTemplar && (i == DEXTERITY || i == INTELLIGENCE))
                        FinalBonusGranted = 0;
                    if (itemType == GItemType.FollowerTemplar && (i == STRENGTH || i == VITALITY))
                        FinalBonusGranted -= 0.4;

                    // Attack speed is always on a quiver so forget it
                    if ((itemType == GItemType.Quiver) && (i == ATTACKSPEED))
                        FinalBonusGranted = 0;

                    // Single resists worth nothing without all-resist
                    if (i == RANDOMRESIST && (HadStat[ALLRESIST] / ItemMaxStats[ALLRESIST]) < MinimumThreshold[ALLRESIST])
                        FinalBonusGranted = 0;
                    if (FinalBonusGranted < 0)
                        FinalBonusGranted = 0;

                    // Grant the final bonus total
                    iTempPoints *= FinalBonusGranted;

                    // If it's a primary stat, log the highest scoring primary... else add these points to the running total
                    if (i == DEXTERITY || i == STRENGTH || i == INTELLIGENCE)
                    {
                        if (fullItemAnalysis)
                            Log("---- +" + iTempPoints.ToString() + " (*" + FinalBonusGranted.ToString() + " multiplier) [MUST BE MAX STAT SCORE TO COUNT]", true);
                        if (iTempPoints > HighestScoringPrimary)
                        {
                            HighestScoringPrimary = iTempPoints;
                            WhichPrimaryIsHighest = i;
                            AmountHighestScoringPrimary = TempStatistic;
                        }
                    }
                    else
                    {
                        if (fullItemAnalysis)
                            Log("---- +" + iTempPoints.ToString() + " score (*" + FinalBonusGranted.ToString() + " multiplier)", true);
                        TotalItemPoints += iTempPoints;
                    }
                    HadPoints[i] = iTempPoints;

                    // For item logs
                    if (i != DEXTERITY && i != STRENGTH && i != INTELLIGENCE)
                    {
                        if (ValueItemStatString != "")
                            ValueItemStatString += ". ";
                        ValueItemStatString += StatNames[i] + "=" + Math.Round(TempStatistic).ToString();
                        if (junkItemStatString != "")
                            junkItemStatString += ". ";
                        junkItemStatString += StatNames[i] + "=" + Math.Round(TempStatistic).ToString();
                    }
                }
            }

            // Now add on one of the three primary stat scores, whichever was higher
            if (HighestScoringPrimary > 0)
            {

                // Give a 30% of primary-stat-score-possible bonus to the primary scoring if paired with a good amount of life % or vitality
                if ((HadStat[VITALITY] / ItemMaxStats[VITALITY] > (MinimumThreshold[VITALITY] + 0.1)) || SafeLifePercentage > 0.85)
                    HighestScoringPrimary += ItemMaxPoints[WhichPrimaryIsHighest] * 0.3;

                // Reduce a primary a little if there is no vitality or life
                if ((HadStat[VITALITY] < 40) || SafeLifePercentage < 0.7)
                    HighestScoringPrimary *= 0.8;
                TotalItemPoints += HighestScoringPrimary;
                ValueItemStatString = StatNames[WhichPrimaryIsHighest] + "=" + Math.Round(AmountHighestScoringPrimary).ToString() + ". " + ValueItemStatString;
                junkItemStatString = StatNames[WhichPrimaryIsHighest] + "=" + Math.Round(AmountHighestScoringPrimary).ToString() + ". " + junkItemStatString;
            }
            if (fullItemAnalysis)
                Log("--- +" + TotalItemPoints.ToString() + " total score pre-special reductions. (GM=" + GlobalMultiplier.ToString() + ")", true);

            // Global multiplier
            TotalItemPoints *= GlobalMultiplier;

            // 2 handed weapons and ranged weapons lose a large score for low DPS
            if (baseItemType == GBaseItemType.WeaponRange || baseItemType == GBaseItemType.WeaponTwoHand)
            {
                if ((HadStat[TOTALDPS] / ItemMaxStats[TOTALDPS]) <= 0.7)
                    TotalItemPoints *= 0.75;
            }

            // Weapons should get a nice 15% bonus score for having very high primaries
            if (baseItemType == GBaseItemType.WeaponRange || baseItemType == GBaseItemType.WeaponOneHand || baseItemType == GBaseItemType.WeaponTwoHand)
            {
                if (HighestScoringPrimary > 0 && (HighestScoringPrimary >= ItemMaxPoints[WhichPrimaryIsHighest] * 0.9))
                {
                    TotalItemPoints *= 1.15;
                }

                // And an extra 15% for a very high vitality
                if (HadStat[VITALITY] > 0 && (HadStat[VITALITY] >= ItemMaxPoints[VITALITY] * 0.9))
                {
                    TotalItemPoints *= 1.15;
                }

                // And an extra 15% for a very high life-on-hit
                if (HadStat[LIFEONHIT] > 0 && (HadStat[LIFEONHIT] >= ItemMaxPoints[LIFEONHIT] * 0.9))
                {
                    TotalItemPoints *= 1.15;
                }
            }

            // Shields 
            if (itemType == GItemType.Shield)
            {

                // Strength/Dex based shield calculations
                if (WhichPrimaryIsHighest == STRENGTH || WhichPrimaryIsHighest == DEXTERITY)
                {
                    if (HadStat[BLOCKCHANCE] < 20)
                    {
                        TotalItemPoints *= 0.7;
                    }
                    else if (HadStat[BLOCKCHANCE] < 25)
                    {
                        TotalItemPoints *= 0.9;
                    }
                }

                // Intelligence/no primary based shields
                else
                {
                    if (HadStat[BLOCKCHANCE] < 28)
                        TotalItemPoints -= HadPoints[BLOCKCHANCE];
                }
            }

            // Quivers
            if (itemType == GItemType.Quiver)
            {
                TotalRequirements = 0;
                if (HadStat[DEXTERITY] >= 100)
                    TotalRequirements++;
                else
                    TotalRequirements -= 3;
                if (HadStat[DEXTERITY] >= 160)
                    TotalRequirements++;
                if (HadStat[DEXTERITY] >= 250)
                    TotalRequirements++;
                if (HadStat[ATTACKSPEED] < 14)
                    TotalRequirements -= 2;
                if (HadStat[VITALITY] >= 70 || SafeLifePercentage >= 0.85)
                    TotalRequirements++;
                else
                    TotalRequirements--;
                if (HadStat[VITALITY] >= 260)
                    TotalRequirements++;
                if (HadStat[MAXDISCIPLINE] >= 8)
                    TotalRequirements++;
                if (HadStat[MAXDISCIPLINE] >= 10)
                    TotalRequirements++;
                if (HadStat[SOCKETS] >= 1)
                    TotalRequirements++;
                if (HadStat[CRITCHANCE] >= 6)
                    TotalRequirements++;
                if (HadStat[CRITCHANCE] >= 8)
                    TotalRequirements++;
                if (HadStat[LIFEPERCENT] >= 8)
                    TotalRequirements++;
                if (HadStat[MAGICFIND] >= 18)
                    TotalRequirements++;
                if (TotalRequirements < 4)
                    TotalItemPoints *= 0.4;
                else if (TotalRequirements < 5)
                    TotalItemPoints *= 0.5;
                if (TotalRequirements >= 7)
                    TotalItemPoints *= 1.2;
            }

            // Mojos and Sources
            if (itemType == GItemType.Source || itemType == GItemType.Mojo)
            {
                TotalRequirements = 0;
                if (HadStat[INTELLIGENCE] >= 100)
                    TotalRequirements++;
                else if (HadStat[INTELLIGENCE] < 80)
                    TotalRequirements -= 3;
                else if (HadStat[INTELLIGENCE] < 100)
                    TotalRequirements -= 1;
                if (HadStat[INTELLIGENCE] >= 160)
                    TotalRequirements++;
                if (HadStat[MAXDAMAGE] >= 250)
                    TotalRequirements++;
                else
                    TotalRequirements -= 2;
                if (HadStat[MAXDAMAGE] >= 340)
                    TotalRequirements++;
                if (HadStat[MINDAMAGE] >= 50)
                    TotalRequirements++;
                else
                    TotalRequirements--;
                if (HadStat[MINDAMAGE] >= 85)
                    TotalRequirements++;
                if (HadStat[VITALITY] >= 70)
                    TotalRequirements++;
                if (HadStat[SOCKETS] >= 1)
                    TotalRequirements++;
                if (HadStat[CRITCHANCE] >= 6)
                    TotalRequirements++;
                if (HadStat[CRITCHANCE] >= 8)
                    TotalRequirements++;
                if (HadStat[LIFEPERCENT] >= 8)
                    TotalRequirements++;
                if (HadStat[MAGICFIND] >= 15)
                    TotalRequirements++;
                if (HadStat[MAXMANA] >= 60)
                    TotalRequirements++;
                if (HadStat[ARCANECRIT] >= 8)
                    TotalRequirements++;
                if (HadStat[ARCANECRIT] >= 10)
                    TotalRequirements++;
                if (TotalRequirements < 4)
                    TotalItemPoints *= 0.4;
                else if (TotalRequirements < 5)
                    TotalItemPoints *= 0.5;
                if (TotalRequirements >= 8)
                    TotalItemPoints *= 1.2;
            }

            // Chests/cloaks/pants without a socket lose 17% of total score
            if ((itemType == GItemType.Chest || itemType == GItemType.Cloak || itemType == GItemType.Pants) && HadStat[SOCKETS] == 0)
                TotalItemPoints *= 0.83;

            // Boots with no movement speed get reduced score
            if ((itemType == GItemType.Boots) && HadStat[MOVEMENTSPEED] <= 6)
                TotalItemPoints *= 0.75;

            // Helmets
            if (itemType == GItemType.Helm || itemType == GItemType.WizardHat || itemType == GItemType.VoodooMask || itemType == GItemType.SpiritStone)
            {
                // Helmets without a socket lose 20% of total score, and most of any MF/GF bonus
                if (HadStat[SOCKETS] == 0)
                {
                    TotalItemPoints *= 0.8;
                    if (HadStat[MAGICFIND] > 0 || HadStat[GOLDFIND] > 0)
                    {
                        if (HadStat[MAGICFIND] > 0 && HadStat[GOLDFIND] > 0)
                            TotalItemPoints -= ((HadPoints[MAGICFIND] * 0.25) + (HadPoints[GOLDFIND] * 0.25));
                        else
                            TotalItemPoints -= ((HadPoints[MAGICFIND] * 0.65) + (HadPoints[GOLDFIND] * 0.65));
                    }
                }
            }

            GetBestFinalPoints(itemType);

            TotalItemPoints *= BestFinalBonus;

            if (fullItemAnalysis)
                Log("TOTAL: " + TotalItemPoints.ToString());

            if (fullItemAnalysis)
                Log("");

            return Math.Round(TotalItemPoints);
        }

        private static void CheckForInvalidItemType(GItemType itemType)
        {
            // One Handed Weapons 
            if (itemType == GItemType.Axe || itemType == GItemType.CeremonialKnife || itemType == GItemType.Dagger ||
                 itemType == GItemType.FistWeapon || itemType == GItemType.Mace || itemType == GItemType.MightyWeapon ||
                 itemType == GItemType.Spear || itemType == GItemType.Sword || itemType == GItemType.Wand)
            {
                Array.Copy(MaxPointsWeaponOneHand, ItemMaxStats, TOTALSTATS);
                Array.Copy(WeaponPointsAtMax, ItemMaxPoints, TOTALSTATS);
                IsInvalidItem = false;
            }

            // Two Handed Weapons
            if (itemType == GItemType.TwoHandAxe || itemType == GItemType.TwoHandDaibo || itemType == GItemType.TwoHandMace ||
                itemType == GItemType.TwoHandMighty || itemType == GItemType.TwoHandPolearm || itemType == GItemType.TwoHandStaff ||
                itemType == GItemType.TwoHandSword)
            {
                Array.Copy(MaxPointsWeaponTwoHand, ItemMaxStats, TOTALSTATS);
                Array.Copy(WeaponPointsAtMax, ItemMaxPoints, TOTALSTATS);
                IsInvalidItem = false;
            }

            // Ranged Weapons
            if (itemType == GItemType.TwoHandCrossbow || itemType == GItemType.TwoHandBow || itemType == GItemType.HandCrossbow)
            {
                Array.Copy(MaxPointsWeaponRanged, ItemMaxStats, TOTALSTATS);
                Array.Copy(WeaponPointsAtMax, ItemMaxPoints, TOTALSTATS);
                if (itemType == GItemType.HandCrossbow)
                {
                    ItemMaxStats[TOTALDPS] -= 150;
                }
                IsInvalidItem = false;
            }

            // Off-handed stuff

            // Mojo, Source, Quiver
            if (itemType == GItemType.Mojo || itemType == GItemType.Source || itemType == GItemType.Quiver)
            {
                Array.Copy(MaxPointsOffHand, ItemMaxStats, TOTALSTATS);
                Array.Copy(ArmorPointsAtMax, ItemMaxPoints, TOTALSTATS);
                IsInvalidItem = false;
            }

            // Shields
            if (itemType == GItemType.Shield)
            {
                Array.Copy(MaxPointsShield, ItemMaxStats, TOTALSTATS);
                Array.Copy(ArmorPointsAtMax, ItemMaxPoints, TOTALSTATS);
                IsInvalidItem = false;
            }

            // Jewelry

            // Ring
            if (itemType == GItemType.Amulet)
            {
                Array.Copy(MaxPointsAmulet, ItemMaxStats, TOTALSTATS);
                Array.Copy(JewelryPointsAtMax, ItemMaxPoints, TOTALSTATS);
                IsInvalidItem = false;
            }

            // Ring
            if (itemType == GItemType.Ring)
            {
                Array.Copy(MaxPointsRing, ItemMaxStats, TOTALSTATS);
                Array.Copy(JewelryPointsAtMax, ItemMaxPoints, TOTALSTATS);
                IsInvalidItem = false;
            }

            // Armor

            // Belt
            if (itemType == GItemType.Belt)
            {
                Array.Copy(MaxPointsBelt, ItemMaxStats, TOTALSTATS);
                Array.Copy(ArmorPointsAtMax, ItemMaxPoints, TOTALSTATS);
                IsInvalidItem = false;
            }

            // Boots
            if (itemType == GItemType.Boots)
            {
                Array.Copy(MaxPointsBoots, ItemMaxStats, TOTALSTATS);
                Array.Copy(ArmorPointsAtMax, ItemMaxPoints, TOTALSTATS);
                IsInvalidItem = false;
            }

            // Bracers
            if (itemType == GItemType.Bracers)
            {
                Array.Copy(MaxPointsBracer, ItemMaxStats, TOTALSTATS);
                Array.Copy(ArmorPointsAtMax, ItemMaxPoints, TOTALSTATS);
                IsInvalidItem = false;
            }

            // Chest
            if (itemType == GItemType.Chest)
            {
                Array.Copy(MaxPointsChest, ItemMaxStats, TOTALSTATS);
                Array.Copy(ArmorPointsAtMax, ItemMaxPoints, TOTALSTATS);
                IsInvalidItem = false;
            }
            if (itemType == GItemType.Cloak)
            {
                Array.Copy(MaxPointsCloak, ItemMaxStats, TOTALSTATS);
                Array.Copy(ArmorPointsAtMax, ItemMaxPoints, TOTALSTATS);
                IsInvalidItem = false;
            }

            // Gloves
            if (itemType == GItemType.Gloves)
            {
                Array.Copy(MaxPointsGloves, ItemMaxStats, TOTALSTATS);
                Array.Copy(ArmorPointsAtMax, ItemMaxPoints, TOTALSTATS);
                IsInvalidItem = false;
            }

            // Helm
            if (itemType == GItemType.Helm)
            {
                Array.Copy(MaxPointsHelm, ItemMaxStats, TOTALSTATS);
                Array.Copy(ArmorPointsAtMax, ItemMaxPoints, TOTALSTATS);
                IsInvalidItem = false;
            }

            // Pants
            if (itemType == GItemType.Pants)
            {
                Array.Copy(MaxPointsPants, ItemMaxStats, TOTALSTATS);
                Array.Copy(ArmorPointsAtMax, ItemMaxPoints, TOTALSTATS);
                IsInvalidItem = false;
            }
            if (itemType == GItemType.MightyBelt)
            {
                Array.Copy(MaxPointsMightyBelt, ItemMaxStats, TOTALSTATS);
                Array.Copy(ArmorPointsAtMax, ItemMaxPoints, TOTALSTATS);
                IsInvalidItem = false;
            }

            // Shoulders
            if (itemType == GItemType.Shoulders)
            {
                Array.Copy(MaxPointsShoulders, ItemMaxStats, TOTALSTATS);
                Array.Copy(ArmorPointsAtMax, ItemMaxPoints, TOTALSTATS);
                IsInvalidItem = false;
            }
            if (itemType == GItemType.SpiritStone)
            {
                Array.Copy(MaxPointsSpiritStone, ItemMaxStats, TOTALSTATS);
                Array.Copy(ArmorPointsAtMax, ItemMaxPoints, TOTALSTATS);
                IsInvalidItem = false;
            }
            if (itemType == GItemType.VoodooMask)
            {
                Array.Copy(MaxPointsVoodooMask, ItemMaxStats, TOTALSTATS);
                Array.Copy(ArmorPointsAtMax, ItemMaxPoints, TOTALSTATS);
                IsInvalidItem = false;
            }

            // Wizard Hat
            if (itemType == GItemType.WizardHat)
            {
                Array.Copy(MaxPointsWizardHat, ItemMaxStats, TOTALSTATS);
                Array.Copy(ArmorPointsAtMax, ItemMaxPoints, TOTALSTATS);
                IsInvalidItem = false;
            }

            // Follower Items
            if (itemType == GItemType.FollowerEnchantress || itemType == GItemType.FollowerScoundrel || itemType == GItemType.FollowerTemplar)
            {
                Array.Copy(MaxPointsFollower, ItemMaxStats, TOTALSTATS);
                Array.Copy(JewelryPointsAtMax, ItemMaxPoints, TOTALSTATS);
                IsInvalidItem = false;
            }
        }

        private static void GetBestFinalPoints(GItemType itemType)
        {
            // Gold-find and pickup radius combined
            if ((HadStat[GOLDFIND] / ItemMaxStats[GOLDFIND] > 0.55) && (HadStat[PICKUPRADIUS] / ItemMaxStats[PICKUPRADIUS] > 0.5))
                TotalItemPoints += (((ItemMaxPoints[PICKUPRADIUS] + ItemMaxPoints[GOLDFIND]) / 2) * 0.25);

            // All-resist and pickup radius combined
            if ((HadStat[ALLRESIST] / ItemMaxStats[ALLRESIST] > 0.55) && (HadStat[PICKUPRADIUS] > 0))
                TotalItemPoints += (((ItemMaxPoints[PICKUPRADIUS] + ItemMaxPoints[ALLRESIST]) / 2) * 0.65);

            // Special crit hit/crit chance/attack speed combos
            if ((HadStat[CRITCHANCE] > (ItemMaxStats[CRITCHANCE] * 0.8)) && (HadStat[CRITDAMAGE] > (ItemMaxStats[CRITDAMAGE] * 0.8)) && (HadStat[ATTACKSPEED] > (ItemMaxStats[ATTACKSPEED] * 0.8)))
            {
                if (BestFinalBonus < 3.2 && itemType != GItemType.Quiver)
                    BestFinalBonus = 3.2;
            }
            if ((HadStat[CRITCHANCE] > (ItemMaxStats[CRITCHANCE] * 0.8)) && (HadStat[CRITDAMAGE] > (ItemMaxStats[CRITDAMAGE] * 0.8)))
            {
                if (BestFinalBonus < 2.3)
                    BestFinalBonus = 2.3;
            }
            if ((HadStat[CRITCHANCE] > (ItemMaxStats[CRITCHANCE] * 0.8)) && (HadStat[ATTACKSPEED] > (ItemMaxStats[ATTACKSPEED] * 0.8)))
            {
                if (BestFinalBonus < 2.1 && itemType != GItemType.Quiver)
                    BestFinalBonus = 2.1;
            }
            if ((HadStat[CRITDAMAGE] > (ItemMaxStats[CRITDAMAGE] * 0.8)) && (HadStat[ATTACKSPEED] > (ItemMaxStats[ATTACKSPEED] * 0.8)))
            {
                if (BestFinalBonus < 1.8 && itemType != GItemType.Quiver)
                    BestFinalBonus = 1.8;
            }
            if ((HadStat[CRITCHANCE] > (ItemMaxStats[CRITCHANCE] * 0.65)) && (HadStat[CRITDAMAGE] > (ItemMaxStats[CRITDAMAGE] * 0.65)) && (HadStat[ATTACKSPEED] > (ItemMaxStats[ATTACKSPEED] * 0.65)))
            {
                if (BestFinalBonus < 2.1 && itemType != GItemType.Quiver)
                    BestFinalBonus = 2.1;
            }
            if ((HadStat[CRITCHANCE] > (ItemMaxStats[CRITCHANCE] * 0.65)) && (HadStat[CRITDAMAGE] > (ItemMaxStats[CRITDAMAGE] * 0.65)))
            {
                if (BestFinalBonus < 1.9) BestFinalBonus = 1.9;
            }
            if ((HadStat[CRITCHANCE] > (ItemMaxStats[CRITCHANCE] * 0.65)) && (HadStat[ATTACKSPEED] > (ItemMaxStats[ATTACKSPEED] * 0.65)))
            {
                if (BestFinalBonus < 1.7 && itemType != GItemType.Quiver)
                    BestFinalBonus = 1.7;
            }
            if ((HadStat[CRITDAMAGE] > (ItemMaxStats[CRITDAMAGE] * 0.65)) && (HadStat[ATTACKSPEED] > (ItemMaxStats[ATTACKSPEED] * 0.65)))
            {
                if (BestFinalBonus < 1.5 && itemType != GItemType.Quiver)
                    BestFinalBonus = 1.5;
            }
            if ((HadStat[CRITCHANCE] > (ItemMaxStats[CRITCHANCE] * 0.45)) && (HadStat[CRITDAMAGE] > (ItemMaxStats[CRITDAMAGE] * 0.45)) && (HadStat[ATTACKSPEED] > (ItemMaxStats[ATTACKSPEED] * 0.45)))
            {
                if (BestFinalBonus < 1.7 && itemType != GItemType.Quiver)
                    BestFinalBonus = 1.7;
            }
            if ((HadStat[CRITCHANCE] > (ItemMaxStats[CRITCHANCE] * 0.45)) && (HadStat[CRITDAMAGE] > (ItemMaxStats[CRITDAMAGE] * 0.45)))
            {
                if (BestFinalBonus < 1.4) BestFinalBonus = 1.4;
            }
            if ((HadStat[CRITCHANCE] > (ItemMaxStats[CRITCHANCE] * 0.45)) && (HadStat[ATTACKSPEED] > (ItemMaxStats[ATTACKSPEED] * 0.45)))
            {
                if (BestFinalBonus < 1.3 && itemType != GItemType.Quiver)
                    BestFinalBonus = 1.3;
            }
            if ((HadStat[CRITDAMAGE] > (ItemMaxStats[CRITDAMAGE] * 0.45)) && (HadStat[ATTACKSPEED] > (ItemMaxStats[ATTACKSPEED] * 0.45)))
            {
                if (BestFinalBonus < 1.1 && itemType != GItemType.Quiver)
                    BestFinalBonus = 1.1;
            }
        }

    }
}
