using GilesTrinity.Technicals;
using System;
using System.Linq;
using Zeta.Common.Plugins;
using Zeta.Internals.Actors;
namespace GilesTrinity
{
    public class ItemValuation
    {
        private static double[] ItemMaxStats = new double[Constants.TOTALSTATS];
        private static double[] ItemMaxPoints = new double[Constants.TOTALSTATS];
        private static double[] HadStat = new double[Constants.TOTALSTATS] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        private static double[] HadPoints = new double[Constants.TOTALSTATS] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        private static GItemBaseType baseItemType = GItemBaseType.Unknown;

        /// <summary>
        /// The bizarre mystery function to score your lovely items!
        /// </summary>
        /// <param name="item"></param>
        /// <param name="itemType"></param>
        /// <returns></returns>
        internal static double ValueThisItem(GilesCachedACDItem item, GItemType itemType)
        {
            // Reset static variables
            TownRun.ValueItemStatString = "";
            TownRun.junkItemStatString = "";

            ItemMaxStats = new double[Constants.TOTALSTATS];
            ItemMaxPoints = new double[Constants.TOTALSTATS];
            HadStat = new double[Constants.TOTALSTATS] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            HadPoints = new double[Constants.TOTALSTATS] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            baseItemType = GilesTrinity.DetermineBaseType(itemType);

            // Make sure we got a valid item here, otherwise score it a big fat 0
            if (item.IsUnidentified || CheckForInvalidItemType(itemType))
            {
                DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation, "-- Invalid Item Type or Unidentified?");
                return 0;
            }

            DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation, "NEXT ITEM= " + item.RealName + " - " + item.InternalName + " [" + baseItemType.ToString() + " - " + itemType.ToString() + "]");

            #region CustomScoring
            if ((itemType == GItemType.Quiver ||
                    itemType == GItemType.SpiritStone ||
                    itemType == GItemType.Orb ||
                    itemType == GItemType.Shield ||
                    itemType == GItemType.Mojo)
                && item.CritPercent <= 0)
            {
                DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation, string.Format("ItemType: {0}, has no crit so returning a score of 1000", itemType));
                DisplayItemStats(item);
                return 1000;
            }

            if ((itemType == GItemType.Quiver || itemType == GItemType.SpiritStone) && item.Dexterity < 200)
            {
                DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation, string.Format("ItemType: {0}, doesn't have min 200 Dex", itemType));
                DisplayItemStats(item);
                return 1000;
            }

            if ((itemType == GItemType.Orb || itemType == GItemType.Mojo) && item.Intelligence < 200)
            {
                DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation, string.Format("ItemType: {0}, doesn't have min 200 Int", itemType));
                DisplayItemStats(item);
                return 1000;
            }
            #endregion

            int[] PossiblePrimarys = new int[] { System.Convert.ToInt32(item.Dexterity), System.Convert.ToInt32(item.Strength), System.Convert.ToInt32(item.Intelligence) };
            int PrimaryStat = PossiblePrimarys.Max();
            int itemScore = 1000;

            DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation, "===== Type: =====: " + itemType);
            DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation, "===== baseItemType: =====: " + baseItemType);

            //Calc Primary Stat
            #region CalcPrimaryStat
            if (PrimaryStat > 0)
            {
                float maxStat = 0;
                if (System.Convert.ToInt32(item.Dexterity) > System.Convert.ToInt32(item.Strength) &&
                    System.Convert.ToInt32(item.Dexterity) > System.Convert.ToInt32(item.Intelligence))
                {
                    maxStat = item.Dexterity;
                    TownRun.ValueItemStatString = "Dexterity:" + item.Dexterity.ToString();
                    //DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation,"===== TownRun.ValueItemStatString: =====: " + TownRun.ValueItemStatString);
                }

                if (System.Convert.ToInt32(item.Intelligence) > System.Convert.ToInt32(item.Strength) &&
                    System.Convert.ToInt32(item.Intelligence) > System.Convert.ToInt32(item.Dexterity))
                {
                    maxStat = item.Intelligence;
                    TownRun.ValueItemStatString = "Intelligence:" + item.Intelligence.ToString();
                    //DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation,"===== TownRun.ValueItemStatString: =====: " + TownRun.ValueItemStatString);
                }

                if (System.Convert.ToInt32(item.Strength) > System.Convert.ToInt32(item.Dexterity) &&
                    System.Convert.ToInt32(item.Strength) > System.Convert.ToInt32(item.Intelligence))
                {
                    maxStat = item.Strength;
                    TownRun.ValueItemStatString = "Strength:" + item.Strength.ToString();
                    //DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation,"===== TownRun.ValueItemStatString: =====: " + TownRun.ValueItemStatString);
                }

                //Override primary stat
                if (itemType == GItemType.Quiver ||
                    itemType == GItemType.SpiritStone)
                {
                    PrimaryStat = System.Convert.ToInt32(item.Dexterity);
                    maxStat = System.Convert.ToInt32(item.Dexterity);
                    TownRun.ValueItemStatString = "Dexterity: " + item.Dexterity.ToString();
                    //DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation,"===== TownRun.ValueItemStatString: =====: " + TownRun.ValueItemStatString);
                }

                if (itemType == GItemType.Orb ||
                   itemType == GItemType.CeremonialKnife ||
                   itemType == GItemType.WizardHat ||
                   itemType == GItemType.VoodooMask)
                {
                    PrimaryStat = System.Convert.ToInt32(item.Intelligence);
                    maxStat = item.Intelligence;
                    TownRun.ValueItemStatString = "Intelligence: " + item.Intelligence.ToString();
                    //DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation,"===== TownRun.ValueItemStatString: =====: " + TownRun.ValueItemStatString);
                }

                if (itemType == GItemType.MightyBelt ||
                    itemType == GItemType.MightyWeapon)
                {
                    PrimaryStat = System.Convert.ToInt32(item.Strength);
                    maxStat = item.Strength;
                    TownRun.ValueItemStatString = "Strength: " + item.Strength.ToString();
                    //DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation,"===== TownRun.ValueItemStatString: =====: " + TownRun.ValueItemStatString);
                }

                int imaxStat = int.Parse(maxStat.ToString());

                DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation, "===== Item's Primary Stat =====: " + PrimaryStat);
                DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation, "===== Item's Max Primary Stat =====: " + ItemMaxStats[imaxStat]);
                DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation, "===== Item's Primary Stat Points =====: " + System.Convert.ToInt32(System.Convert.ToDouble(PrimaryStat / ItemMaxStats[imaxStat]) * 10000));
                itemScore += System.Convert.ToInt32(System.Convert.ToDouble(PrimaryStat / ItemMaxStats[imaxStat]) * 10000);
            }
            #endregion


            #region FollowerItems
            if (itemType == GItemType.FollowerEnchantress && item.Intelligence > 310)
            {
                itemScore = 30000;
                TownRun.ValueItemStatString += "Enchantress Follower Item has over 310 Intelligence, autoscore 30,000";
                //DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation,"===== TownRun.ValueItemStatString: =====: " + TownRun.ValueItemStatString);
            }

            if (itemType == GItemType.FollowerScoundrel && item.Dexterity > 310)
            {
                itemScore = 30000;
                TownRun.ValueItemStatString += "Scoundrel Follower Item has over 310 Dexterity, autoscore 30,000";
                //DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation,"===== TownRun.ValueItemStatString: =====: " + TownRun.ValueItemStatString);
            }

            if (itemType == GItemType.FollowerTemplar && item.Strength > 310)
            {
                itemScore = 30000;
                TownRun.ValueItemStatString += "Templar Follower Item has over 310 Strength, autoscore 30,000";
                //DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation,"===== TownRun.ValueItemStatString: =====: " + TownRun.ValueItemStatString);
            }
            #endregion

            #region WeaponScoring
            if ((baseItemType == GItemBaseType.WeaponOneHand ||
               baseItemType == GItemBaseType.WeaponRange) &&
               item.WeaponDamagePerSecond > 950)
            {

                if (item.CritDamagePercent > 0)
                    itemScore += 20000;

                if (item.LifeOnHit > 0)
                    itemScore += 20000;

                if (item.LifeSteal > 0)
                    itemScore += 20000;

                if (item.Sockets > 0)
                    itemScore += 20000;

                TownRun.ValueItemStatString += "Bonus 40000 with 2 good stats for 1h and ranged weapons with over 950dps";
            }

            if (baseItemType == GItemBaseType.WeaponTwoHand && item.WeaponDamagePerSecond > 1350)
            {

                if (item.CritDamagePercent > 0)
                    itemScore += 20000;

                if (item.LifeOnHit > 0)
                    itemScore += 20000;

                if (item.LifeSteal > 0)
                    itemScore += 20000;

                if (item.Sockets > 0)
                    itemScore += 20000;

                TownRun.ValueItemStatString += "Bonus 40000 with 2 good stats for 2h weapons with over 1350dps";
            }

            if ((baseItemType == GItemBaseType.WeaponTwoHand ||
               baseItemType == GItemBaseType.WeaponOneHand ||
               baseItemType == GItemBaseType.WeaponRange) &&
               item.WeaponDamagePerSecond > 0)
            {
                TownRun.ValueItemStatString += " -DPS:" + item.WeaponDamagePerSecond.ToString();
                //DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation,"===== TownRun.ValueItemStatString: =====: " + TownRun.ValueItemStatString);
            }
            //Scoring Weapon
            if ((baseItemType == GItemBaseType.WeaponTwoHand ||
                 baseItemType == GItemBaseType.WeaponOneHand ||
                 baseItemType == GItemBaseType.WeaponRange) &&
                (item.WeaponDamagePerSecond > 0 &&
                ((item.WeaponDamagePerSecond / ItemMaxStats[Constants.TOTALDPS]) * 100) > 75))
            {
                if (item.Sockets >= 1)
                {
                    DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation, "===== Item's Number of SOCKETS =====: " + item.Sockets);
                    DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation, "===== Item's SOCKETS points =====: " + item.Sockets * 10000);
                    itemScore += System.Convert.ToInt32(item.Sockets * 10000);
                    TownRun.ValueItemStatString += " -Sockets:" + item.Sockets.ToString();
                    //DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation,"===== TownRun.ValueItemStatString: =====: " + TownRun.ValueItemStatString);
                }


                if (item.CritDamagePercent > 0 && ItemMaxStats[Constants.CRITDAMAGE] > 0)
                {
                    DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation, "===== Item's CRITDAMAGE =====: " + item.CritDamagePercent);
                    DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation, "===== Item's Max CRITDAMAGE =====: " + ItemMaxStats[Constants.CRITDAMAGE]);
                    DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation, "===== Item's CRITDAMAGE Points =====: " + System.Convert.ToInt32(System.Convert.ToDouble(item.CritDamagePercent / ItemMaxStats[Constants.CRITDAMAGE]) * 10000));
                    itemScore += System.Convert.ToInt32(System.Convert.ToDouble(item.CritDamagePercent / ItemMaxStats[Constants.CRITDAMAGE]) * 10000);
                    TownRun.ValueItemStatString += " -CritDamagePercent:" + item.CritDamagePercent.ToString();
                    //DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation,"===== TownRun.ValueItemStatString: =====: " + TownRun.ValueItemStatString);
                }

                if (item.LifeOnHit > 0 && ItemMaxStats[Constants.LIFEONHIT] > 0)
                {
                    DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation, "===== Item's LIFEONHIT =====: " + item.LifeOnHit);
                    DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation, "===== Item's Max LIFEONHIT =====: " + ItemMaxStats[Constants.LIFEONHIT]);
                    DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation, "===== Item's LIFEONHIT Points =====: " + System.Convert.ToInt32(System.Convert.ToDouble(item.LifeOnHit / ItemMaxStats[Constants.LIFEONHIT]) * 5000));
                    itemScore += System.Convert.ToInt32(System.Convert.ToDouble(item.LifeOnHit / ItemMaxStats[Constants.LIFEONHIT]) * 5000);
                    TownRun.ValueItemStatString += " -LifeOnHit:" + item.LifeOnHit.ToString();
                    //DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation,"===== TownRun.ValueItemStatString: =====: " + TownRun.ValueItemStatString);
                }

                if (item.LifeSteal > 0 && ItemMaxStats[Constants.LIFESTEAL] > 0)
                {
                    DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation, "===== Item's LIFESTEAL =====: " + item.LifeSteal);
                    DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation, "===== Item's Max LIFESTEAL =====: " + ItemMaxStats[Constants.LIFESTEAL]);
                    DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation, "===== Item's LIFESTEAL Points =====: " + System.Convert.ToInt32(System.Convert.ToDouble(item.LifeSteal / ItemMaxStats[Constants.LIFESTEAL]) * 5000));
                    itemScore += System.Convert.ToInt32(System.Convert.ToDouble(item.LifeSteal / ItemMaxStats[Constants.LIFESTEAL]) * 5000);
                    TownRun.ValueItemStatString += " -LifeSteal:" + item.LifeSteal.ToString();
                    //DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation,"===== TownRun.ValueItemStatString: =====: " + TownRun.ValueItemStatString);
                }

                DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation, "===== Item's DPS =====: " + item.WeaponDamagePerSecond);
                DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation, "===== Item's Max DPS =====: " + ItemMaxStats[Constants.TOTALDPS]);
                DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation, "===== Item's DPS Points =====: " + System.Convert.ToInt32(System.Convert.ToDouble(item.WeaponDamagePerSecond / ItemMaxStats[Constants.TOTALDPS]) * 25000));
                itemScore += System.Convert.ToInt32(System.Convert.ToDouble(item.WeaponDamagePerSecond / ItemMaxStats[Constants.TOTALDPS]) * 25000);
            }
            #endregion

            #region ArmorJewelry
            //Scoring Armor + Jewelry
            if (baseItemType == GItemBaseType.Jewelry ||
                baseItemType == GItemBaseType.Armor ||
                baseItemType == GItemBaseType.Misc ||
                baseItemType == GItemBaseType.Offhand ||
                baseItemType == GItemBaseType.Unknown)
            {
                if (item.MinDamage > 0 && ItemMaxStats[Constants.MINDAMAGE] > 0)
                {
                    DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation, "===== Item's MINDAMAGE =====: " + item.MinDamage);
                    DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation, "===== Item's Max MINDAMAGE =====: " + ItemMaxStats[Constants.MINDAMAGE]);
                    DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation, "===== Item's MINDAMAGE Points =====: " + System.Convert.ToInt32(System.Convert.ToDouble(item.MinDamage / ItemMaxStats[Constants.MINDAMAGE]) * 2500));
                    itemScore += System.Convert.ToInt32(System.Convert.ToDouble(item.MinDamage / ItemMaxStats[Constants.MINDAMAGE]) * 2500);
                    TownRun.ValueItemStatString += " -MinDamage:" + item.MinDamage.ToString();
                    //DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation,"===== TownRun.ValueItemStatString: =====: " + TownRun.ValueItemStatString);
                }

                if (item.MaxDamage > 0 && ItemMaxStats[Constants.MAXDAMAGE] > 0)
                {
                    DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation, "===== Item's MAXDAMAGE =====: " + item.MaxDamage);
                    DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation, "===== Item's Max MAXDAMAGE =====: " + ItemMaxStats[Constants.MAXDAMAGE]);
                    DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation, "===== Item's MAXDAMAGE Points =====: " + System.Convert.ToInt32(System.Convert.ToDouble(item.MaxDamage / ItemMaxStats[Constants.MAXDAMAGE]) * 2500));
                    itemScore += System.Convert.ToInt32(System.Convert.ToDouble(item.MaxDamage / ItemMaxStats[Constants.MAXDAMAGE]) * 2500);
                    TownRun.ValueItemStatString += " -MaxDamage:" + item.MaxDamage.ToString();
                    //DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation,"===== TownRun.ValueItemStatString: =====: " + TownRun.ValueItemStatString);
                }

                if (item.CritPercent > 0 && ItemMaxStats[Constants.CRITCHANCE] > 0)
                {
                    //itemScore += 7500;
                    DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation, "===== Item's CRITCHANCE =====: " + item.CritPercent);
                    DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation, "===== Item's Max CRITCHANCE =====: " + ItemMaxStats[Constants.CRITCHANCE]);
                    DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation, "===== Item's CRITCHANCE Points =====: " + System.Convert.ToInt32(System.Convert.ToDouble(item.CritPercent / ItemMaxStats[Constants.CRITCHANCE]) * 10000));
                    itemScore += System.Convert.ToInt32(System.Convert.ToDouble(item.CritPercent / ItemMaxStats[Constants.CRITCHANCE]) * 10000);
                    TownRun.ValueItemStatString += " -CritPercent:" + item.CritPercent.ToString();
                    //DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation,"===== TownRun.ValueItemStatString: =====: " + TownRun.ValueItemStatString);
                }

                if (item.CritDamagePercent > 0 && ItemMaxStats[Constants.CRITDAMAGE] > 0)
                {
                    DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation, "===== Item's CRITDAMAGE =====: " + item.CritDamagePercent);
                    DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation, "===== Item's Max CRITDAMAGE =====: " + ItemMaxStats[Constants.CRITDAMAGE]);
                    DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation, "===== Item's CRITDAMAGE Points =====: " + System.Convert.ToInt32(System.Convert.ToDouble(item.CritDamagePercent / ItemMaxStats[Constants.CRITDAMAGE]) * 10000));
                    itemScore += System.Convert.ToInt32(System.Convert.ToDouble(item.CritDamagePercent / ItemMaxStats[Constants.CRITDAMAGE]) * 10000);
                    TownRun.ValueItemStatString += " -CritDamagePercent:" + item.CritDamagePercent.ToString();
                    //DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation,"===== TownRun.ValueItemStatString: =====: " + TownRun.ValueItemStatString);
                }

                if (item.AttackSpeedPercent > 0 && ItemMaxStats[Constants.ATTACKSPEED] > 0)
                {
                    //itemScore += 7500;
                    DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation, "===== Item's ATTACKSPEED =====: " + item.AttackSpeedPercent);
                    DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation, "===== Item's Max ATTACKSPEED =====: " + ItemMaxStats[Constants.ATTACKSPEED]);
                    DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation, "===== Item's ATTACKSPEED Points =====: " + System.Convert.ToInt32(System.Convert.ToDouble(item.AttackSpeedPercent / ItemMaxStats[Constants.ATTACKSPEED]) * 10000));
                    itemScore += System.Convert.ToInt32(System.Convert.ToDouble(item.AttackSpeedPercent / ItemMaxStats[Constants.ATTACKSPEED]) * 10000);
                    TownRun.ValueItemStatString += " -AttackSpeedPercent:" + item.AttackSpeedPercent.ToString();
                    //DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation,"===== TownRun.ValueItemStatString: =====: " + TownRun.ValueItemStatString);
                }

                //DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation,"At VITALITY");
                if (item.Vitality > 0 && ItemMaxStats[Constants.VITALITY] > 0)
                {
                    DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation, "===== Item's VITALITY =====: " + item.Vitality);
                    DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation, "===== Item's Max VITALITY =====: " + ItemMaxStats[Constants.VITALITY]);
                    DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation, "===== Item's VITALITY Points =====: " + System.Convert.ToInt32(System.Convert.ToDouble(item.Vitality / ItemMaxStats[Constants.VITALITY]) * 10000));
                    itemScore += System.Convert.ToInt32(System.Convert.ToDouble(item.Vitality / ItemMaxStats[Constants.VITALITY]) * 10000);
                    TownRun.ValueItemStatString += " -Vit:" + item.Vitality.ToString();
                    //DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation,"===== TownRun.ValueItemStatString: =====: " + TownRun.ValueItemStatString);
                }

                //DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation,"At ALLRESIST");
                if (item.ResistAll > 0 && ItemMaxStats[Constants.ALLRESIST] > 0)
                {
                    DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation, "===== Item's ALLRESIST =====: " + item.ResistAll);
                    DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation, "===== Item's Max ALLRESIST =====: " + ItemMaxStats[Constants.ALLRESIST]);
                    DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation, "===== Item's ALLRESIST Points =====: " + System.Convert.ToInt32(System.Convert.ToDouble(item.ResistAll / ItemMaxStats[Constants.ALLRESIST]) * 10000));
                    itemScore += System.Convert.ToInt32(System.Convert.ToDouble(item.ResistAll / ItemMaxStats[Constants.ALLRESIST]) * 10000);
                    TownRun.ValueItemStatString += " -RA:" + item.ResistAll.ToString();
                    //DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation,"===== TownRun.ValueItemStatString: =====: " + TownRun.ValueItemStatString);
                }

                //DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation,"At MOVEMENTSPEED");
                if (item.MovementSpeed > 0 && ItemMaxStats[Constants.MOVEMENTSPEED] > 0)
                {
                    DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation, "===== Item's MOVEMENTSPEED =====: " + item.MovementSpeed);
                    DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation, "===== Item's Max MOVEMENTSPEED =====: " + ItemMaxStats[Constants.MOVEMENTSPEED]);
                    DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation, "===== Item's MOVEMENTSPEED Points =====: " + System.Convert.ToInt32(System.Convert.ToDouble(item.MovementSpeed / ItemMaxStats[Constants.MOVEMENTSPEED]) * 5000));
                    itemScore += System.Convert.ToInt32(System.Convert.ToDouble(item.MovementSpeed / ItemMaxStats[Constants.MOVEMENTSPEED]) * 5000);
                    TownRun.ValueItemStatString += " -MS:" + item.MovementSpeed.ToString();
                    //DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation,"===== TownRun.ValueItemStatString: =====: " + TownRun.ValueItemStatString);
                }

                //DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation,"At PICKUPRADIUS");
                if (item.PickUpRadius > 0 && ItemMaxStats[Constants.PICKUPRADIUS] > 0)
                {
                    DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation, "===== Item's PICKUPRADIUS =====: " + item.PickUpRadius);
                    DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation, "===== Item's Max PICKUPRADIUS =====: " + ItemMaxStats[Constants.PICKUPRADIUS]);
                    DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation, "===== Item's PICKUPRADIUS Points =====: " + System.Convert.ToInt32(System.Convert.ToDouble(item.PickUpRadius / ItemMaxStats[Constants.PICKUPRADIUS]) * 5000));
                    itemScore += System.Convert.ToInt32(System.Convert.ToDouble(item.PickUpRadius / ItemMaxStats[Constants.PICKUPRADIUS]) * 5000);
                    TownRun.ValueItemStatString += " -PR:" + item.PickUpRadius.ToString();
                    //DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation,"===== TownRun.ValueItemStatString: =====: " + TownRun.ValueItemStatString);
                }

                if (item.LifePercent > 0 && ItemMaxStats[Constants.LIFEPERCENT] > 0)
                {
                    DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation, "===== Item's LIFEPERCENT =====: " + item.LifePercent);
                    DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation, "===== Item's Max LIFEPERCENT =====: " + ItemMaxStats[Constants.LIFEPERCENT]);
                    DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation, "===== Item's LIFEPERCENT Points =====: " + System.Convert.ToInt32(System.Convert.ToDouble(item.LifePercent / ItemMaxStats[Constants.LIFEPERCENT]) * 5000));
                    itemScore += System.Convert.ToInt32(System.Convert.ToDouble(item.LifePercent / ItemMaxStats[Constants.LIFEPERCENT]) * 5000);
                    TownRun.ValueItemStatString += " -LP:" + item.LifePercent.ToString();
                    //DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation,"===== TownRun.ValueItemStatString: =====: " + TownRun.ValueItemStatString);
                }

                if (item.ResistArcane > 0 && ItemMaxStats[Constants.RANDOMRESIST] > 0)
                {
                    DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation, "===== Item's ResistArcane =====: " + item.ResistArcane);
                    DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation, "===== Item's Max RANDOMRESIST =====: " + ItemMaxStats[Constants.RANDOMRESIST]);
                    DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation, "===== Item's ResistArcane Points =====: " + System.Convert.ToInt32(System.Convert.ToDouble(item.ResistArcane / ItemMaxStats[Constants.RANDOMRESIST]) * 4000));
                    itemScore += System.Convert.ToInt32(System.Convert.ToDouble(item.ResistArcane / ItemMaxStats[Constants.RANDOMRESIST]) * 4000);
                    TownRun.ValueItemStatString += " -AR:" + item.ResistArcane.ToString();
                    //DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation,"===== TownRun.ValueItemStatString: =====: " + TownRun.ValueItemStatString);
                }

                if (item.ResistCold > 0 && ItemMaxStats[Constants.RANDOMRESIST] > 0)
                {
                    DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation, "===== Item's ResistCold =====: " + item.ResistCold);
                    DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation, "===== Item's Max RANDOMRESIST =====: " + ItemMaxStats[Constants.RANDOMRESIST]);
                    DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation, "===== Item's ResistCold Points =====: " + System.Convert.ToInt32(System.Convert.ToDouble(item.ResistCold / ItemMaxStats[Constants.RANDOMRESIST]) * 2000));
                    itemScore += System.Convert.ToInt32(System.Convert.ToDouble(item.ResistCold / ItemMaxStats[Constants.RANDOMRESIST]) * 2000);
                    TownRun.ValueItemStatString += " -CR:" + item.ResistCold.ToString();
                    //DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation,"===== TownRun.ValueItemStatString: =====: " + TownRun.ValueItemStatString);
                }

                if (item.ResistFire > 0 && ItemMaxStats[Constants.RANDOMRESIST] > 0)
                {
                    DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation, "===== Item's ResistFire =====: " + item.ResistFire);
                    DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation, "===== Item's Max RANDOMRESIST =====: " + ItemMaxStats[Constants.RANDOMRESIST]);
                    DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation, "===== Item's ResistFire Points =====: " + System.Convert.ToInt32(System.Convert.ToDouble(item.ResistFire / ItemMaxStats[Constants.RANDOMRESIST]) * 2500));
                    itemScore += System.Convert.ToInt32(System.Convert.ToDouble(item.ResistFire / ItemMaxStats[Constants.RANDOMRESIST]) * 2500);
                    TownRun.ValueItemStatString += " -FR:" + item.ResistFire.ToString();
                    //DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation,"===== TownRun.ValueItemStatString: =====: " + TownRun.ValueItemStatString);
                }

                if (item.ResistHoly > 0 && ItemMaxStats[Constants.RANDOMRESIST] > 0)
                {
                    DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation, "===== Item's ResistHoly =====: " + item.ResistHoly);
                    DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation, "===== Item's Max RANDOMRESIST =====: " + ItemMaxStats[Constants.RANDOMRESIST]);
                    DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation, "===== Item's ResistHoly Points =====: " + System.Convert.ToInt32(System.Convert.ToDouble(item.ResistHoly / ItemMaxStats[Constants.RANDOMRESIST]) * 1500));
                    itemScore += System.Convert.ToInt32(System.Convert.ToDouble(item.ResistHoly / ItemMaxStats[Constants.RANDOMRESIST]) * 1500);
                    TownRun.ValueItemStatString += " -HR:" + item.ResistHoly.ToString();
                    //DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation,"===== TownRun.ValueItemStatString: =====: " + TownRun.ValueItemStatString);
                }

                if (item.ResistLightning > 0 && ItemMaxStats[Constants.RANDOMRESIST] > 0)
                {
                    DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation, "===== Item's ResistLightning =====: " + item.ResistLightning);
                    DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation, "===== Item's Max RANDOMRESIST =====: " + ItemMaxStats[Constants.RANDOMRESIST]);
                    DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation, "===== Item's ResistLightning Points =====: " + System.Convert.ToInt32(System.Convert.ToDouble(item.ResistLightning / ItemMaxStats[Constants.RANDOMRESIST]) * 1500));
                    itemScore += System.Convert.ToInt32(System.Convert.ToDouble(item.ResistLightning / ItemMaxStats[Constants.RANDOMRESIST]) * 1500);
                    TownRun.ValueItemStatString += " -LR:" + item.ResistLightning.ToString();
                    //DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation,"===== TownRun.ValueItemStatString: =====: " + TownRun.ValueItemStatString);
                }

                if (item.ResistPhysical > 0 && ItemMaxStats[Constants.RANDOMRESIST] > 0)
                {
                    DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation, "===== Item's ResistPhysical =====: " + item.ResistPhysical);
                    DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation, "===== Item's Max RANDOMRESIST =====: " + ItemMaxStats[Constants.RANDOMRESIST]);
                    DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation, "===== Item's ResistPhysical Points =====: " + System.Convert.ToInt32(System.Convert.ToDouble(item.ResistPhysical / ItemMaxStats[Constants.RANDOMRESIST]) * 3500));
                    itemScore += System.Convert.ToInt32(System.Convert.ToDouble(item.ResistPhysical / ItemMaxStats[Constants.RANDOMRESIST]) * 3500);
                    TownRun.ValueItemStatString += " -PhR:" + item.ResistPhysical.ToString();
                    //DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation,"===== TownRun.ValueItemStatString: =====: " + TownRun.ValueItemStatString);
                }

                if (item.ResistPoison > 0 && ItemMaxStats[Constants.RANDOMRESIST] > 0)
                {
                    DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation, "===== Item's ResistPoison =====: " + item.ResistPoison);
                    DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation, "===== Item's Max RANDOMRESIST =====: " + ItemMaxStats[Constants.RANDOMRESIST]);
                    DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation, "===== Item's ResistPoison Points =====: " + System.Convert.ToInt32(System.Convert.ToDouble(item.ResistPoison / ItemMaxStats[Constants.RANDOMRESIST]) * 3500));
                    itemScore += System.Convert.ToInt32(System.Convert.ToDouble(item.ResistPoison / ItemMaxStats[Constants.RANDOMRESIST]) * 3500);
                    TownRun.ValueItemStatString += " -PoR:" + item.ResistPoison.ToString();
                    //DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation,"===== TownRun.ValueItemStatString: =====: " + TownRun.ValueItemStatString);
                }

                if (item.Sockets >= 1)
                {
                    DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation, "===== Item's Number of SOCKETS =====: " + item.Sockets);
                    DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation, "===== Item's SOCKETS points =====: " + item.Sockets * 2500);
                    itemScore += System.Convert.ToInt32(item.Sockets * 2500);
                    TownRun.ValueItemStatString += " -Sockets:" + item.Sockets.ToString();
                    //DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation,"===== TownRun.ValueItemStatString: =====: " + TownRun.ValueItemStatString);
                }
            }
            #endregion

            #region FillMaxStats
            //Give Amulets and Rings Bonus 2000 points
            if (itemType == GItemType.Amulet ||
                    itemType == GItemType.Ring)
            {
                itemScore += 2000;
                DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation, string.Format("Adding Bonus 2000 for Amulets and Rings, New item score", itemScore));
            }
            #endregion

            DisplayItemStats(item);

            TownRun.junkItemStatString = TownRun.ValueItemStatString;

            DbHelper.Log(TrinityLogLevel.Normal, LogCategory.ItemValuation, "===== Final Score =====: " + itemScore.ToString());
            return itemScore;
        }

        internal static void ResetValuationStatStrings()
        {
            TownRun.ValueItemStatString = "";
            TownRun.junkItemStatString = "";
        }

        private static bool CheckForInvalidItemType(GItemType itemType)
        {
            // One Handed Weapons 
            if (itemType == GItemType.Axe || itemType == GItemType.CeremonialKnife || itemType == GItemType.Dagger ||
                 itemType == GItemType.FistWeapon || itemType == GItemType.Mace || itemType == GItemType.MightyWeapon ||
                 itemType == GItemType.Spear || itemType == GItemType.Sword || itemType == GItemType.Wand ||
                 itemType == GItemType.HandCrossbow)
            {
                Array.Copy(Constants.MaxPointsWeaponOneHand, ItemMaxStats, Constants.TOTALSTATS);
                Array.Copy(Constants.WeaponPointsAtMax, ItemMaxPoints, Constants.TOTALSTATS);
                return false;
            }

            // Two Handed Weapons
            if (itemType == GItemType.TwoHandAxe || itemType == GItemType.TwoHandDaibo || itemType == GItemType.TwoHandMace ||
                itemType == GItemType.TwoHandMighty || itemType == GItemType.TwoHandPolearm || itemType == GItemType.TwoHandStaff ||
                itemType == GItemType.TwoHandSword ||
                itemType == GItemType.TwoHandCrossbow || itemType == GItemType.TwoHandBow)
            {
                Array.Copy(Constants.MaxPointsWeaponTwoHand, ItemMaxStats, Constants.TOTALSTATS);
                Array.Copy(Constants.WeaponPointsAtMax, ItemMaxPoints, Constants.TOTALSTATS);
                return false;
            }

            // Off-handed stuff

            // Mojo, Source, Quiver
            if (itemType == GItemType.Mojo || itemType == GItemType.Orb || itemType == GItemType.Quiver)
            {
                Array.Copy(Constants.MaxPointsOffHand, ItemMaxStats, Constants.TOTALSTATS);
                Array.Copy(Constants.ArmorPointsAtMax, ItemMaxPoints, Constants.TOTALSTATS);
                return false;
            }

            // Shields
            if (itemType == GItemType.Shield)
            {
                Array.Copy(Constants.MaxPointsShield, ItemMaxStats, Constants.TOTALSTATS);
                Array.Copy(Constants.ArmorPointsAtMax, ItemMaxPoints, Constants.TOTALSTATS);
                return false;
            }

            // Jewelry

            // Ring
            if (itemType == GItemType.Amulet)
            {
                Array.Copy(Constants.MaxPointsAmulet, ItemMaxStats, Constants.TOTALSTATS);
                Array.Copy(Constants.JewelryPointsAtMax, ItemMaxPoints, Constants.TOTALSTATS);
                return false;
            }

            // Ring
            if (itemType == GItemType.Ring)
            {
                Array.Copy(Constants.MaxPointsRing, ItemMaxStats, Constants.TOTALSTATS);
                Array.Copy(Constants.JewelryPointsAtMax, ItemMaxPoints, Constants.TOTALSTATS);
                return false;
            }

            // Armor

            // Belt
            if (itemType == GItemType.Belt)
            {
                Array.Copy(Constants.MaxPointsBelt, ItemMaxStats, Constants.TOTALSTATS);
                Array.Copy(Constants.ArmorPointsAtMax, ItemMaxPoints, Constants.TOTALSTATS);
                return false;
            }

            // Boots
            if (itemType == GItemType.Boots)
            {
                Array.Copy(Constants.MaxPointsBoots, ItemMaxStats, Constants.TOTALSTATS);
                Array.Copy(Constants.ArmorPointsAtMax, ItemMaxPoints, Constants.TOTALSTATS);
                return false;
            }

            // Bracers
            if (itemType == GItemType.Bracer)
            {
                Array.Copy(Constants.MaxPointsBracer, ItemMaxStats, Constants.TOTALSTATS);
                Array.Copy(Constants.ArmorPointsAtMax, ItemMaxPoints, Constants.TOTALSTATS);
                return false;
            }

            // Chest
            if (itemType == GItemType.Chest)
            {
                Array.Copy(Constants.MaxPointsChest, ItemMaxStats, Constants.TOTALSTATS);
                Array.Copy(Constants.ArmorPointsAtMax, ItemMaxPoints, Constants.TOTALSTATS);
                return false;
            }
            if (itemType == GItemType.Cloak)
            {
                Array.Copy(Constants.MaxPointsCloak, ItemMaxStats, Constants.TOTALSTATS);
                Array.Copy(Constants.ArmorPointsAtMax, ItemMaxPoints, Constants.TOTALSTATS);
                return false;
            }

            // Gloves
            if (itemType == GItemType.Gloves)
            {
                Array.Copy(Constants.MaxPointsGloves, ItemMaxStats, Constants.TOTALSTATS);
                Array.Copy(Constants.ArmorPointsAtMax, ItemMaxPoints, Constants.TOTALSTATS);
                return false;
            }

            // Helm
            if (itemType == GItemType.Helm)
            {
                Array.Copy(Constants.MaxPointsHelm, ItemMaxStats, Constants.TOTALSTATS);
                Array.Copy(Constants.ArmorPointsAtMax, ItemMaxPoints, Constants.TOTALSTATS);
                return false;
            }

            // Pants
            if (itemType == GItemType.Legs)
            {
                Array.Copy(Constants.MaxPointsPants, ItemMaxStats, Constants.TOTALSTATS);
                Array.Copy(Constants.ArmorPointsAtMax, ItemMaxPoints, Constants.TOTALSTATS);
                return false;
            }
            if (itemType == GItemType.MightyBelt)
            {
                Array.Copy(Constants.MaxPointsMightyBelt, ItemMaxStats, Constants.TOTALSTATS);
                Array.Copy(Constants.ArmorPointsAtMax, ItemMaxPoints, Constants.TOTALSTATS);
                return false;
            }

            // Shoulders
            if (itemType == GItemType.Shoulder)
            {
                Array.Copy(Constants.MaxPointsShoulders, ItemMaxStats, Constants.TOTALSTATS);
                Array.Copy(Constants.ArmorPointsAtMax, ItemMaxPoints, Constants.TOTALSTATS);
                return false;
            }
            if (itemType == GItemType.SpiritStone)
            {
                Array.Copy(Constants.MaxPointsSpiritStone, ItemMaxStats, Constants.TOTALSTATS);
                Array.Copy(Constants.ArmorPointsAtMax, ItemMaxPoints, Constants.TOTALSTATS);
                return false;
            }
            if (itemType == GItemType.VoodooMask)
            {
                Array.Copy(Constants.MaxPointsVoodooMask, ItemMaxStats, Constants.TOTALSTATS);
                Array.Copy(Constants.ArmorPointsAtMax, ItemMaxPoints, Constants.TOTALSTATS);
                return false;
            }

            // Wizard Hat
            if (itemType == GItemType.WizardHat)
            {
                Array.Copy(Constants.MaxPointsWizardHat, ItemMaxStats, Constants.TOTALSTATS);
                Array.Copy(Constants.ArmorPointsAtMax, ItemMaxPoints, Constants.TOTALSTATS);
                return false;
            }

            // Follower Items
            if (itemType == GItemType.FollowerEnchantress || itemType == GItemType.FollowerScoundrel || itemType == GItemType.FollowerTemplar)
            {
                Array.Copy(Constants.MaxPointsFollower, ItemMaxStats, Constants.TOTALSTATS);
                Array.Copy(Constants.JewelryPointsAtMax, ItemMaxPoints, Constants.TOTALSTATS);
                return false;
            }

            return true;
        }

        private static void DisplayItemStats(GilesCachedACDItem item)
        {
            TownRun.ValueItemStatString += Environment.NewLine + "Complete List Of Item Stats:" + Environment.NewLine;

            if (item.RealName != null)
                TownRun.ValueItemStatString += "Real Name: " + item.RealName + Environment.NewLine;

            if (item.InternalName != null)
                TownRun.ValueItemStatString += "Internal Name: " + item.InternalName + Environment.NewLine;

            if (item.Level > 0)
                TownRun.ValueItemStatString += "Level: " + item.Level + Environment.NewLine;

            if (item.ArcaneDamagePercent > 0)
                TownRun.ValueItemStatString += "ArcaneDamagePercent: " + item.ArcaneDamagePercent + Environment.NewLine;

            if (item.ArcaneOnCrit > 0)
                TownRun.ValueItemStatString += "ArcaneOnCrit: " + item.ArcaneOnCrit + Environment.NewLine;

            if (item.Armor > 0)
                TownRun.ValueItemStatString += "Armor: " + item.Armor + Environment.NewLine;

            if (item.ArmorBonus > 0)
                TownRun.ValueItemStatString += "ArmorBonus: " + item.ArmorBonus + Environment.NewLine;

            if (item.ArmorTotal > 0)
                TownRun.ValueItemStatString += "ArmorTotal: " + item.ArmorTotal + Environment.NewLine;

            if (item.AttackSpeedPercent > 0)
                TownRun.ValueItemStatString += "AttackSpeedPercent: " + item.AttackSpeedPercent + Environment.NewLine;

            if (item.BlockChance > 0)
                TownRun.ValueItemStatString += "BlockChance: " + item.BlockChance + Environment.NewLine;

            if (item.ColdDamagePercent > 0)
                TownRun.ValueItemStatString += "ColdDamagePercent: " + item.ColdDamagePercent + Environment.NewLine;

            if (item.CritDamagePercent > 0)
                TownRun.ValueItemStatString += "CritDamagePercent: " + item.CritDamagePercent + Environment.NewLine;

            if (item.CritPercent > 0)
                TownRun.ValueItemStatString += "CritPercent: " + item.CritPercent + Environment.NewLine;

            if (item.DamageReductionPhysicalPercent > 0)
                TownRun.ValueItemStatString += "DamageReductionPhysicalPercent: " + item.DamageReductionPhysicalPercent + Environment.NewLine;

            if (item.Dexterity > 0)
                TownRun.ValueItemStatString += "Dexterity: " + item.Dexterity + Environment.NewLine;

            if (item.FireDamagePercent > 0)
                TownRun.ValueItemStatString += "FireDamagePercent: " + item.FireDamagePercent + Environment.NewLine;

            if (item.GlobeBonus > 0)
                TownRun.ValueItemStatString += "GlobeBonus: " + item.GlobeBonus + Environment.NewLine;

            if (item.GoldAmount > 0)
                TownRun.ValueItemStatString += "GoldAmount: " + item.GoldAmount + Environment.NewLine;

            if (item.GoldFind > 0)
                TownRun.ValueItemStatString += "GoldFind: " + item.GoldFind + Environment.NewLine;

            if (item.HatredRegen > 0)
                TownRun.ValueItemStatString += "HatredRegen: " + item.HatredRegen + Environment.NewLine;

            if (item.HealthGlobeBonus > 0)
                TownRun.ValueItemStatString += "HealthGlobeBonus: " + item.HealthGlobeBonus + Environment.NewLine;

            if (item.HealthPerSecond > 0)
                TownRun.ValueItemStatString += "HealthPerSecond: " + item.HealthPerSecond + Environment.NewLine;

            if (item.HealthPerSpiritSpent > 0)
                TownRun.ValueItemStatString += "HealthPerSpiritSpent: " + item.HealthPerSpiritSpent + Environment.NewLine;

            if (item.HolyDamagePercent > 0)
                TownRun.ValueItemStatString += "HolyDamagePercent: " + item.HolyDamagePercent + Environment.NewLine;

            if (item.Intelligence > 0)
                TownRun.ValueItemStatString += "Intelligence: " + item.Intelligence + Environment.NewLine;

            if (item.LifeOnHit > 0)
                TownRun.ValueItemStatString += "LifeOnHit: " + item.LifeOnHit + Environment.NewLine;

            if (item.LifePercent > 0)
                TownRun.ValueItemStatString += "LifePercent: " + item.LifePercent + Environment.NewLine;

            if (item.LifeSteal > 0)
                TownRun.ValueItemStatString += "LifeSteal: " + item.LifeSteal + Environment.NewLine;

            if (item.LightningDamagePercent > 0)
                TownRun.ValueItemStatString += "LightningDamagePercent: " + item.LightningDamagePercent + Environment.NewLine;

            if (item.MagicFind > 0)
                TownRun.ValueItemStatString += "MagicFind: " + item.MagicFind + Environment.NewLine;

            if (item.ManaRegen > 0)
                TownRun.ValueItemStatString += "ManaRegen: " + item.ManaRegen + Environment.NewLine;

            if (item.MaxArcanePower > 0)
                TownRun.ValueItemStatString += "MaxArcanePower: " + item.MaxArcanePower + Environment.NewLine;

            if (item.MaxDamage > 0)
                TownRun.ValueItemStatString += "MaxDamage: " + item.MaxDamage + Environment.NewLine;

            if (item.MaxDiscipline > 0)
                TownRun.ValueItemStatString += "MaxDiscipline: " + item.MaxDiscipline + Environment.NewLine;

            if (item.MaxFury > 0)
                TownRun.ValueItemStatString += "MaxFury: " + item.MaxFury + Environment.NewLine;

            if (item.MaxMana > 0)
                TownRun.ValueItemStatString += "MaxMana: " + item.MaxMana + Environment.NewLine;

            if (item.MaxSpirit > 0)
                TownRun.ValueItemStatString += "MaxSpirit: " + item.MaxSpirit + Environment.NewLine;

            if (item.MinDamage > 0)
                TownRun.ValueItemStatString += "MinDamage: " + item.MinDamage + Environment.NewLine;

            if (item.MovementSpeed > 0)
                TownRun.ValueItemStatString += "MovementSpeed: " + item.MovementSpeed + Environment.NewLine;

            if (item.PickUpRadius > 0)
                TownRun.ValueItemStatString += "PickUpRadius: " + item.PickUpRadius + Environment.NewLine;

            if (item.PoisonDamagePercent > 0)
                TownRun.ValueItemStatString += "PoisonDamagePercent: " + item.PoisonDamagePercent + Environment.NewLine;

            if (item.ResistAll > 0)
                TownRun.ValueItemStatString += "ResistAll: " + item.ResistAll + Environment.NewLine;

            if (item.ResistArcane > 0)
                TownRun.ValueItemStatString += "ResistArcane: " + item.ResistArcane + Environment.NewLine;

            if (item.ResistCold > 0)
                TownRun.ValueItemStatString += "ResistCold: " + item.ResistCold + Environment.NewLine;

            if (item.ResistFire > 0)
                TownRun.ValueItemStatString += "ResistFire: " + item.ResistFire + Environment.NewLine;

            if (item.ResistHoly > 0)
                TownRun.ValueItemStatString += "ResistHoly: " + item.ResistHoly + Environment.NewLine;

            if (item.ResistLightning > 0)
                TownRun.ValueItemStatString += "ResistLightning: " + item.ResistLightning + Environment.NewLine;

            if (item.ResistPhysical > 0)
                TownRun.ValueItemStatString += "ResistPhysical: " + item.ResistPhysical + Environment.NewLine;

            if (item.ResistPoison > 0)
                TownRun.ValueItemStatString += "ResistPoison: " + item.ResistPoison + Environment.NewLine;

            if (item.Sockets > 0)
                TownRun.ValueItemStatString += "Sockets: " + item.Sockets + Environment.NewLine;

            if (item.SpiritRegen > 0)
                TownRun.ValueItemStatString += "SpiritRegen: " + item.SpiritRegen + Environment.NewLine;

            if (item.Strength > 0)
                TownRun.ValueItemStatString += "Strength: " + item.Strength + Environment.NewLine;

            if (item.Thorns > 0)
                TownRun.ValueItemStatString += "Thorns: " + item.Thorns + Environment.NewLine;

            if (item.Vitality > 0)
                TownRun.ValueItemStatString += "Vitality: " + item.Vitality + Environment.NewLine;

            if (item.WeaponDPS > 0)
                TownRun.ValueItemStatString += "WeaponDPS: " + item.WeaponDPS + Environment.NewLine;
        }
    }
}
