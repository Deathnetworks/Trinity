using System;
using System.IO;
using System.Linq;
using Trinity.Cache;
using Trinity.DbProvider;
using Trinity.Helpers;
using Trinity.ItemRules;
using Trinity.Items;
using Trinity.Settings.Loot;
using Trinity.Technicals;
using Zeta.Bot;
using Zeta.Game;
using Zeta.Game.Internals.Actors;

namespace Trinity
{
    public partial class Trinity
    {
        /// <summary>
        ///     Output test scores for everything in the backpack
        /// </summary>
        internal static void TestScoring()
        {
            using (new PerformanceLogger("TestScoring"))
            {
                using (new ZetaCacheHelper())
                {
                    try
                    {
                        if (TownRun.TestingBackpack)
                            return;
                        TownRun.TestingBackpack = true;
                        //ZetaDia.Actors.Update();
                        if (ZetaDia.Actors.Me == null)
                        {
                            Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "Error testing scores - not in game world?");
                            return;
                        }
                        if (ZetaDia.IsInGame && !ZetaDia.IsLoadingWorld)
                        {
                            Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "===== Outputting Test Scores =====");
                            foreach (ACDItem item in ZetaDia.Me.Inventory.Backpack)
                            {
                                if (item.BaseAddress == IntPtr.Zero)
                                {
                                    Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "GSError: Diablo 3 memory read error, or item became invalid [TestScore-1]");
                                }
                                else
                                {
                                    bool shouldStash = ItemManager.Current.ShouldStashItem(item);
                                    Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, shouldStash ? "* KEEP *" : "-- TRASH --");
                                }
                            }
                            Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "===== Finished Test Score Outputs =====");
                        }
                        else
                        {
                            Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "Error testing scores - not in game world?");
                        }
                        TownRun.TestingBackpack = false;
                    }
                    catch (Exception ex)
                    {
                        Logger.LogNormal("Exception in TestScoring(): {0}", ex);
                        TownRun.TestingBackpack = false;
                    }
                }
            }
        }

        /// <summary>Return the score needed to keep something by the item type</summary>
        internal static double ScoreNeeded(ItemBaseType itemBaseType)
        {
            switch (itemBaseType)
            {
                case ItemBaseType.Weapon:
                    return Math.Round((double)Settings.Loot.TownRun.WeaponScore);
                case ItemBaseType.Armor:
                    return Math.Round((double)Settings.Loot.TownRun.ArmorScore);
                case ItemBaseType.Jewelry:
                    return Math.Round((double)Settings.Loot.TownRun.JewelryScore);
                default:
                    return 0;
            }
        }

        /// <summary>
        ///     Checks if score of item is suffisant for throw notification.
        /// </summary>
        public static bool CheckScoreForNotification(GItemBaseType itemBaseType, double itemValue)
        {
            switch (itemBaseType)
            {
                case GItemBaseType.WeaponOneHand:
                case GItemBaseType.WeaponRange:
                case GItemBaseType.WeaponTwoHand:
                    return (itemValue >= Settings.Notification.WeaponScore);
                case GItemBaseType.Armor:
                case GItemBaseType.Offhand:
                    return (itemValue >= Settings.Notification.ArmorScore);
                case GItemBaseType.Jewelry:
                    return (itemValue >= Settings.Notification.JewelryScore);
            }
            return false;
        }

    }
}