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
        // Save Configuration
        private void SaveConfiguration()
        {
            if (bSavingConfig) return;
            bSavingConfig = true;
            FileStream configStream = File.Open(sTrinityConfigFile, FileMode.Create, FileAccess.Write, FileShare.Read);
            using (StreamWriter configWriter = new StreamWriter(configStream))
            {
                configWriter.WriteLine("JewelryPoints=" + settings.iNeedPointsToKeepJewelry.ToString());
                configWriter.WriteLine("ArmorPoints=" + settings.iNeedPointsToKeepArmor.ToString());
                configWriter.WriteLine("WeaponPoints=" + settings.iNeedPointsToKeepWeapon.ToString());
                configWriter.WriteLine(settings.bSalvageJunk ? "Salvage=true" : "Salvage=false");
                configWriter.WriteLine(settings.bUseGilesFilters ? "Filters=true" : "Filters=false");
                configWriter.WriteLine(settings.bGemsEmerald ? "Emerald=true" : "Emerald=false");
                configWriter.WriteLine(settings.bGemsAmethyst ? "Amethyst=true" : "Amethyst=false");
                configWriter.WriteLine(settings.bGemsTopaz ? "Topaz=true" : "Topaz=false");
                configWriter.WriteLine(settings.bGemsRuby ? "Ruby=true" : "Ruby=false");
                configWriter.WriteLine(settings.bPickupCraftTomes ? "Tomes=true" : "Tomes=false");
                configWriter.WriteLine(settings.bPickupPlans ? "Plans=true" : "Plans=false");
                configWriter.WriteLine(settings.bPickupFollower ? "Followers=true" : "Followers=false");
                configWriter.WriteLine("Potions=" + settings.iFilterPotions.ToString());
                configWriter.WriteLine("ilvlPots=" + settings.iFilterPotionLevel.ToString());
                configWriter.WriteLine("ilvlLegendary=" + settings.iFilterLegendary.ToString());
                configWriter.WriteLine("ilvlWB=" + settings.iFilterBlueWeapons.ToString());
                configWriter.WriteLine("ilvlWY=" + settings.iFilterYellowWeapons.ToString());
                configWriter.WriteLine("ilvlAB=" + settings.iFilterBlueArmor.ToString());
                configWriter.WriteLine("ilvlAY=" + settings.iFilterYellowArmor.ToString());
                configWriter.WriteLine("ilvlJB=" + settings.iFilterBlueJewelry.ToString());
                configWriter.WriteLine("ilvlJY=" + settings.iFilterYellowJewelry.ToString());
                configWriter.WriteLine("ilvlGems=" + settings.iFilterGems.ToString());
                configWriter.WriteLine("ilvlMisc=" + settings.iFilterMisc.ToString());
                configWriter.WriteLine("GoldPickup=" + settings.iMinimumGoldStack.ToString());
                configWriter.WriteLine("GoblinPriority=" + settings.iTreasureGoblinPriority.ToString());
                configWriter.WriteLine("TriggerRange=" + settings.iMonsterKillRange.ToString());
                configWriter.WriteLine("LootDelay=" + settings.iKillLootDelay.ToString());
                configWriter.WriteLine("VaultDelay=" + settings.iDHVaultMovementDelay.ToString());
                configWriter.WriteLine("MonkInna=" + settings.bMonkInnaSet.ToString());
                configWriter.WriteLine("Avoidance=" + settings.bEnableAvoidance.ToString());
                configWriter.WriteLine("Globes=" + settings.bEnableGlobes.ToString());
                configWriter.WriteLine("CriticalMass=" + settings.bEnableCriticalMass.ToString());
                configWriter.WriteLine("OOCMovementPower=" + settings.bOutOfCombatMovementPowers.ToString());
                configWriter.WriteLine("ExtendedKills=" + settings.bExtendedKillRange.ToString());
                configWriter.WriteLine("SelectiveWW=" + settings.bSelectiveWhirlwind.ToString());
                configWriter.WriteLine("Wrath90=" + settings.bWrath90Seconds.ToString());
                configWriter.WriteLine("WizKiteArchonOnly=" + settings.bKiteOnlyArchon.ToString());
                configWriter.WriteLine("WizWaitForArchon=" + settings.bWaitForArchon.ToString());
                configWriter.WriteLine("BarbWaitForWrath=" + settings.bWaitForWrath.ToString());
                configWriter.WriteLine("BarbGoblinWrath=" + settings.bGoblinWrath.ToString());
                configWriter.WriteLine("BarbFuryDumpWrath=" + settings.bFuryDumpWrath.ToString());
                configWriter.WriteLine("BarbFuryDumpAlways=" + settings.bFuryDumpAlways.ToString());
                configWriter.WriteLine("LogStucks=" + settings.bLogStucks.ToString());
                configWriter.WriteLine("Unstucker=" + settings.bEnableUnstucker.ToString());
                configWriter.WriteLine("ProfileReloading=" + settings.bEnableProfileReloading.ToString());
                configWriter.WriteLine("Backtracking=" + settings.bEnableBacktracking.ToString());
                configWriter.WriteLine(settings.bIgnoreAllShrines ? "ShrineIgnore=all" : "ShrineIgnore=none");
                configWriter.WriteLine("ContainerRange=" + settings.iContainerOpenRange.ToString());
                configWriter.WriteLine("DestructibleRange=" + settings.iDestructibleAttackRange.ToString());
                configWriter.WriteLine("IgnoreCorpses=" + settings.bIgnoreCorpses.ToString());
                configWriter.WriteLine(settings.bEnableTPS ? "TPSEnabled=true" : "TPSEnabled=false");
                configWriter.WriteLine("TPSAmount=" + settings.iTPSAmount.ToString());
                configWriter.WriteLine(settings.bDebugInfo ? "DebugInfo=true" : "DebugInfo=false");
                configWriter.WriteLine(settings.bEnableProwl ? "EnableProwl=true" : "EnableProwl=false");
                configWriter.WriteLine(settings.bEnableAndroid ? "EnableAndroid=true" : "EnableAndroid=false");
                configWriter.WriteLine(settings.bEnableEmail ? "EnableEmail=true" : "EnableEmail=false");
                configWriter.WriteLine("EmailAddress=" + sEmailAddress);
                configWriter.WriteLine("EmailPassword=" + sEmailPassword);
                configWriter.WriteLine("ProwlKey=" + sProwlAPIKey);
                configWriter.WriteLine("AndroidKey=" + sAndroidAPIKey);
                configWriter.WriteLine("EnableLegendaryNotifyScore=" + (settings.bEnableLegendaryNotifyScore ? "true" : "false"));
                configWriter.WriteLine("JewelryNotify=" + settings.iNeedPointsToNotifyJewelry.ToString());
                configWriter.WriteLine("ArmorNotify=" + settings.iNeedPointsToNotifyArmor.ToString());
                configWriter.WriteLine("WeaponNotify=" + settings.iNeedPointsToNotifyWeapon.ToString());
                configWriter.WriteLine("KiteBarb=" + settings.iKiteDistanceBarb.ToString());
                configWriter.WriteLine("KiteWiz=" + settings.iKiteDistanceWiz.ToString());
                configWriter.WriteLine("KiteWitch=" + settings.iKiteDistanceWitch.ToString());
                configWriter.WriteLine("KiteDemon=" + settings.iKiteDistanceDemon.ToString());
                configWriter.WriteLine("PotBarb=" + settings.dEmergencyHealthPotionBarb.ToString());
                configWriter.WriteLine("PotMonk=" + settings.dEmergencyHealthPotionMonk.ToString());
                configWriter.WriteLine("PotWiz=" + settings.dEmergencyHealthPotionWiz.ToString());
                configWriter.WriteLine("PotWitch=" + settings.dEmergencyHealthPotionWitch.ToString());
                configWriter.WriteLine("PotDemon=" + settings.dEmergencyHealthPotionDemon.ToString());
                configWriter.WriteLine("GlobeBarb=" + settings.dEmergencyHealthGlobeBarb.ToString());
                configWriter.WriteLine("GlobeMonk=" + settings.dEmergencyHealthGlobeMonk.ToString());
                configWriter.WriteLine("GlobeWiz=" + settings.dEmergencyHealthGlobeWiz.ToString());
                configWriter.WriteLine("GlobeWitch=" + settings.dEmergencyHealthGlobeWitch.ToString());
                configWriter.WriteLine("GlobeDemon=" + settings.dEmergencyHealthGlobeDemon.ToString());
                string sHealthLine = "";
                for (int i = 1; i <= 13; i++)
                {
                    switch (i)
                    {
                        case 1:
                            sHealthLine += dictAvoidanceHealthBarb[219702].ToString();
                            break;
                        case 2:
                            sHealthLine += dictAvoidanceHealthBarb[84608].ToString();
                            break;
                        case 3:
                            sHealthLine += dictAvoidanceHealthBarb[4804].ToString();
                            break;
                        case 4:
                            sHealthLine += dictAvoidanceHealthBarb[95868].ToString();
                            break;
                        case 5:
                            sHealthLine += dictAvoidanceHealthBarb[5482].ToString();
                            break;
                        case 6:
                            sHealthLine += dictAvoidanceHealthBarb[108869].ToString();
                            break;
                        case 7:
                            sHealthLine += dictAvoidanceHealthBarb[223675].ToString();
                            break;
                        case 8:
                            sHealthLine += dictAvoidanceHealthBarb[3865].ToString();
                            break;
                        case 9:
                            sHealthLine += dictAvoidanceHealthBarb[5212].ToString();
                            break;
                        case 10:
                            sHealthLine += dictAvoidanceHealthBarb[123124].ToString();
                            break;
                        case 11:
                            sHealthLine += dictAvoidanceHealthBarb[123839].ToString();
                            break;
                        case 12:
                            sHealthLine += dictAvoidanceHealthBarb[4103].ToString();
                            break;
                        case 13:
                            sHealthLine += dictAvoidanceHealthBarb[93837].ToString();
                            break;
                    }
                    if (i < 13)
                        sHealthLine += " ";
                }
                configWriter.WriteLine("AOEBarbHealth=" + sHealthLine);
                sHealthLine = "";
                for (int i = 1; i <= 13; i++)
                {
                    switch (i)
                    {
                        case 1:
                            sHealthLine += dictAvoidanceHealthMonk[219702].ToString();
                            break;
                        case 2:
                            sHealthLine += dictAvoidanceHealthMonk[84608].ToString();
                            break;
                        case 3:
                            sHealthLine += dictAvoidanceHealthMonk[4804].ToString();
                            break;
                        case 4:
                            sHealthLine += dictAvoidanceHealthMonk[95868].ToString();
                            break;
                        case 5:
                            sHealthLine += dictAvoidanceHealthMonk[5482].ToString();
                            break;
                        case 6:
                            sHealthLine += dictAvoidanceHealthMonk[108869].ToString();
                            break;
                        case 7:
                            sHealthLine += dictAvoidanceHealthMonk[223675].ToString();
                            break;
                        case 8:
                            sHealthLine += dictAvoidanceHealthMonk[3865].ToString();
                            break;
                        case 9:
                            sHealthLine += dictAvoidanceHealthMonk[5212].ToString();
                            break;
                        case 10:
                            sHealthLine += dictAvoidanceHealthMonk[123124].ToString();
                            break;
                        case 11:
                            sHealthLine += dictAvoidanceHealthMonk[123839].ToString();
                            break;
                        case 12:
                            sHealthLine += dictAvoidanceHealthMonk[4103].ToString();
                            break;
                        case 13:
                            sHealthLine += dictAvoidanceHealthMonk[93837].ToString();
                            break;
                    }
                    if (i < 13)
                        sHealthLine += " ";
                }
                configWriter.WriteLine("AOEMonkHealth=" + sHealthLine);
                sHealthLine = "";
                for (int i = 1; i <= 13; i++)
                {
                    switch (i)
                    {
                        case 1:
                            sHealthLine += dictAvoidanceHealthWizard[219702].ToString();
                            break;
                        case 2:
                            sHealthLine += dictAvoidanceHealthWizard[84608].ToString();
                            break;
                        case 3:
                            sHealthLine += dictAvoidanceHealthWizard[4804].ToString();
                            break;
                        case 4:
                            sHealthLine += dictAvoidanceHealthWizard[95868].ToString();
                            break;
                        case 5:
                            sHealthLine += dictAvoidanceHealthWizard[5482].ToString();
                            break;
                        case 6:
                            sHealthLine += dictAvoidanceHealthWizard[108869].ToString();
                            break;
                        case 7:
                            sHealthLine += dictAvoidanceHealthWizard[223675].ToString();
                            break;
                        case 8:
                            sHealthLine += dictAvoidanceHealthWizard[3865].ToString();
                            break;
                        case 9:
                            sHealthLine += dictAvoidanceHealthWizard[5212].ToString();
                            break;
                        case 10:
                            sHealthLine += dictAvoidanceHealthWizard[123124].ToString();
                            break;
                        case 11:
                            sHealthLine += dictAvoidanceHealthWizard[123839].ToString();
                            break;
                        case 12:
                            sHealthLine += dictAvoidanceHealthWizard[4103].ToString();
                            break;
                        case 13:
                            sHealthLine += dictAvoidanceHealthWizard[93837].ToString();
                            break;
                    }
                    if (i < 13)
                        sHealthLine += " ";
                }
                configWriter.WriteLine("AOEWizardHealth=" + sHealthLine);
                sHealthLine = "";
                for (int i = 1; i <= 13; i++)
                {
                    switch (i)
                    {
                        case 1:
                            sHealthLine += dictAvoidanceHealthWitch[219702].ToString();
                            break;
                        case 2:
                            sHealthLine += dictAvoidanceHealthWitch[84608].ToString();
                            break;
                        case 3:
                            sHealthLine += dictAvoidanceHealthWitch[4804].ToString();
                            break;
                        case 4:
                            sHealthLine += dictAvoidanceHealthWitch[95868].ToString();
                            break;
                        case 5:
                            sHealthLine += dictAvoidanceHealthWitch[5482].ToString();
                            break;
                        case 6:
                            sHealthLine += dictAvoidanceHealthWitch[108869].ToString();
                            break;
                        case 7:
                            sHealthLine += dictAvoidanceHealthWitch[223675].ToString();
                            break;
                        case 8:
                            sHealthLine += dictAvoidanceHealthWitch[3865].ToString();
                            break;
                        case 9:
                            sHealthLine += dictAvoidanceHealthWitch[5212].ToString();
                            break;
                        case 10:
                            sHealthLine += dictAvoidanceHealthWitch[123124].ToString();
                            break;
                        case 11:
                            sHealthLine += dictAvoidanceHealthWitch[123839].ToString();
                            break;
                        case 12:
                            sHealthLine += dictAvoidanceHealthWitch[4103].ToString();
                            break;
                        case 13:
                            sHealthLine += dictAvoidanceHealthWitch[93837].ToString();
                            break;
                    }
                    if (i < 13)
                        sHealthLine += " ";
                }
                configWriter.WriteLine("AOEWitchHealth=" + sHealthLine);
                sHealthLine = "";
                for (int i = 1; i <= 13; i++)
                {
                    switch (i)
                    {
                        case 1:
                            sHealthLine += dictAvoidanceHealthDemon[219702].ToString();
                            break;
                        case 2:
                            sHealthLine += dictAvoidanceHealthDemon[84608].ToString();
                            break;
                        case 3:
                            sHealthLine += dictAvoidanceHealthDemon[4804].ToString();
                            break;
                        case 4:
                            sHealthLine += dictAvoidanceHealthDemon[95868].ToString();
                            break;
                        case 5:
                            sHealthLine += dictAvoidanceHealthDemon[5482].ToString();
                            break;
                        case 6:
                            sHealthLine += dictAvoidanceHealthDemon[108869].ToString();
                            break;
                        case 7:
                            sHealthLine += dictAvoidanceHealthDemon[223675].ToString();
                            break;
                        case 8:
                            sHealthLine += dictAvoidanceHealthDemon[3865].ToString();
                            break;
                        case 9:
                            sHealthLine += dictAvoidanceHealthDemon[5212].ToString();
                            break;
                        case 10:
                            sHealthLine += dictAvoidanceHealthDemon[123124].ToString();
                            break;
                        case 11:
                            sHealthLine += dictAvoidanceHealthDemon[123839].ToString();
                            break;
                        case 12:
                            sHealthLine += dictAvoidanceHealthDemon[4103].ToString();
                            break;
                        case 13:
                            sHealthLine += dictAvoidanceHealthDemon[93837].ToString();
                            break;
                    }
                    if (i < 13)
                        sHealthLine += " ";
                }
                configWriter.WriteLine("AOEDemonHealth=" + sHealthLine);
                sHealthLine = "";
                for (int i = 1; i <= 13; i++)
                {
                    switch (i)
                    {
                        case 1:
                            sHealthLine += dictAvoidanceRadius[219702].ToString();
                            break;
                        case 2:
                            sHealthLine += dictAvoidanceRadius[84608].ToString();
                            break;
                        case 3:
                            sHealthLine += dictAvoidanceRadius[4804].ToString();
                            break;
                        case 4:
                            sHealthLine += dictAvoidanceRadius[95868].ToString();
                            break;
                        case 5:
                            sHealthLine += dictAvoidanceRadius[5482].ToString();
                            break;
                        case 6:
                            sHealthLine += dictAvoidanceRadius[108869].ToString();
                            break;
                        case 7:
                            sHealthLine += dictAvoidanceRadius[223675].ToString();
                            break;
                        case 8:
                            sHealthLine += dictAvoidanceRadius[3865].ToString();
                            break;
                        case 9:
                            sHealthLine += dictAvoidanceRadius[5212].ToString();
                            break;
                        case 10:
                            sHealthLine += dictAvoidanceRadius[123124].ToString();
                            break;
                        case 11:
                            sHealthLine += dictAvoidanceRadius[123839].ToString();
                            break;
                        case 12:
                            sHealthLine += dictAvoidanceRadius[4103].ToString();
                            break;
                        case 13:
                            sHealthLine += dictAvoidanceRadius[93837].ToString();
                            break;
                    }
                    if (i < 13)
                        sHealthLine += " ";
                }
                configWriter.WriteLine("AOERadius=" + sHealthLine);
            }
            configStream.Close();
            saveEmailConfiguration();
            bSavingConfig = false;
            bMappedPlayerAbilities = false;
        }
        private void saveEmailConfiguration()
        {
            FileStream emailConfigStream = File.Open(sTrinityEmailConfigFile, FileMode.Create, FileAccess.Write, FileShare.Read);
            using (StreamWriter configWriter = new StreamWriter(emailConfigStream))
            {
                configWriter.WriteLine(settings.bEnableEmail ? "EnableEmail=true" : "EnableEmail=false");
                configWriter.WriteLine("EmailAddress=" + sEmailAddress);
                configWriter.WriteLine("EmailPassword=" + sEmailPassword);
                configWriter.WriteLine("BotName=" + sBotName);
            }
            emailConfigStream.Close();
        }
        // Load Configuration
        private void LoadConfiguration()
        {
            try
            {
                //Check for Config file
                if (!File.Exists(sTrinityConfigFile))
                {
                    Log("No config file found, now creating a new config from defaults at: " + sTrinityConfigFile);
                    SaveConfiguration();
                    return;
                }
                //Load File
                string[] healthlevels;
                using (StreamReader configReader = new StreamReader(sTrinityConfigFile))
                {
                    while (!configReader.EndOfStream)
                    {
                        string[] config = configReader.ReadLine().Split('=');
                        if (config != null)
                        {
                            switch (config[0])
                            {
                                case "GoblinPriority":
                                    settings.iTreasureGoblinPriority = Convert.ToInt32(config[1]);
                                    break;
                                case "TriggerRange":
                                    settings.iMonsterKillRange = Convert.ToDouble(config[1]);
                                    break;
                                case "LootDelay":
                                    settings.iKillLootDelay = Convert.ToInt32(config[1]);
                                    break;
                                case "VaultDelay":
                                    settings.iDHVaultMovementDelay = Convert.ToInt32(config[1]);
                                    break;
                                case "MonkInna":
                                    settings.bMonkInnaSet = Convert.ToBoolean(config[1]);
                                    break;
                                case "Avoidance":
                                    settings.bEnableAvoidance = Convert.ToBoolean(config[1]);
                                    break;
                                case "Globes":
                                    settings.bEnableGlobes = Convert.ToBoolean(config[1]);
                                    break;
                                case "CriticalMass":
                                    settings.bEnableCriticalMass = Convert.ToBoolean(config[1]);
                                    break;
                                case "OOCMovementPower":
                                    settings.bOutOfCombatMovementPowers = Convert.ToBoolean(config[1]);
                                    break;
                                case "Backtracking":
                                    settings.bEnableBacktracking = Convert.ToBoolean(config[1]);
                                    break;
                                case "JewelryPoints":
                                    settings.iNeedPointsToKeepJewelry = Convert.ToDouble(config[1]);
                                    break;
                                case "ArmorPoints":
                                    settings.iNeedPointsToKeepArmor = Convert.ToDouble(config[1]);
                                    break;
                                case "WeaponPoints":
                                    settings.iNeedPointsToKeepWeapon = Convert.ToDouble(config[1]);
                                    break;
                                case "JewelryNotify":
                                    settings.iNeedPointsToNotifyJewelry = Convert.ToDouble(config[1]);
                                    break;
                                case "ArmorNotify":
                                    settings.iNeedPointsToNotifyArmor = Convert.ToDouble(config[1]);
                                    break;
                                case "WeaponNotify":
                                    settings.iNeedPointsToNotifyWeapon = Convert.ToDouble(config[1]);
                                    break;
                                case "Salvage":
                                    settings.bSalvageJunk = Convert.ToBoolean(config[1]);
                                    break;
                                case "Filters":
                                    settings.bUseGilesFilters = Convert.ToBoolean(config[1]);
                                    break;
                                case "Emerald":
                                    settings.bGemsEmerald = Convert.ToBoolean(config[1]);
                                    break;
                                case "Amethyst":
                                    settings.bGemsAmethyst = Convert.ToBoolean(config[1]);
                                    break;
                                case "Topaz":
                                    settings.bGemsTopaz = Convert.ToBoolean(config[1]);
                                    break;
                                case "Ruby":
                                    settings.bGemsRuby = Convert.ToBoolean(config[1]);
                                    break;
                                case "Tomes":
                                    settings.bPickupCraftTomes = Convert.ToBoolean(config[1]);
                                    break;
                                case "Plans":
                                    settings.bPickupPlans = Convert.ToBoolean(config[1]);
                                    break;
                                case "Followers":
                                    settings.bPickupFollower = Convert.ToBoolean(config[1]);
                                    break;
                                case "Potions":
                                    settings.iFilterPotions = Convert.ToInt16(config[1]);
                                    break;
                                case "ilvlPots":
                                    settings.iFilterPotionLevel = Convert.ToInt16(config[1]);
                                    break;
                                case "ilvlLegendary":
                                    settings.iFilterLegendary = Convert.ToInt16(config[1]);
                                    break;
                                case "ilvlWB":
                                    settings.iFilterBlueWeapons = Convert.ToInt16(config[1]);
                                    break;
                                case "ilvlWY":
                                    settings.iFilterYellowWeapons = Convert.ToInt16(config[1]);
                                    break;
                                case "ilvlAB":
                                    settings.iFilterBlueArmor = Convert.ToInt16(config[1]);
                                    break;
                                case "ilvlAY":
                                    settings.iFilterYellowArmor = Convert.ToInt16(config[1]);
                                    break;
                                case "ilvlJB":
                                    settings.iFilterBlueJewelry = Convert.ToInt16(config[1]);
                                    break;
                                case "ilvlJY":
                                    settings.iFilterYellowJewelry = Convert.ToInt16(config[1]);
                                    break;
                                case "ilvlGems":
                                    settings.iFilterGems = Convert.ToInt16(config[1]);
                                    break;
                                case "ilvlMisc":
                                    settings.iFilterMisc = Convert.ToInt16(config[1]);
                                    break;
                                case "GoldPickup":
                                    settings.iMinimumGoldStack = Convert.ToInt16(config[1]);
                                    break;
                                case "ShrineIgnore":
                                    settings.bIgnoreAllShrines = (config[1] == "all");
                                    break;
                                case "ContainerRange":
                                    settings.iContainerOpenRange = Convert.ToDouble(config[1]);
                                    break;
                                case "DestructibleRange":
                                    settings.iDestructibleAttackRange = Convert.ToDouble(config[1]);
                                    break;
                                case "IgnoreCorpses":
                                    settings.bIgnoreCorpses = Convert.ToBoolean(config[1]);
                                    break;
                                case "TPSEnabled":
                                    settings.bEnableTPS = Convert.ToBoolean(config[1]);
                                    break;
                                case "TPSAmount":
                                    settings.iTPSAmount = Convert.ToDouble(config[1]);
                                    break;
                                case "DebugInfo":
                                    settings.bDebugInfo = Convert.ToBoolean(config[1]);
                                    break;
                                case "LogStucks":
                                    settings.bLogStucks = Convert.ToBoolean(config[1]);
                                    break;
                                case "ProfileReloading":
                                    settings.bEnableProfileReloading = Convert.ToBoolean(config[1]);
                                    break;
                                case "Unstucker":
                                    settings.bEnableUnstucker = Convert.ToBoolean(config[1]);
                                    if (settings.bEnableUnstucker)
                                        Navigator.StuckHandler = new GilesStuckHandler();
                                    else
                                        Navigator.StuckHandler = new DefaultStuckHandler();
                                    break;
                                case "ExtendedKills":
                                    settings.bExtendedKillRange = Convert.ToBoolean(config[1]);
                                    break;
                                case "SelectiveWW":
                                    settings.bSelectiveWhirlwind = Convert.ToBoolean(config[1]);
                                    break;
                                case "Wrath90":
                                    settings.bWrath90Seconds = Convert.ToBoolean(config[1]);
                                    break;
                                case "WizKiteArchonOnly":
                                    settings.bKiteOnlyArchon = Convert.ToBoolean(config[1]);
                                    break;
                                case "WizWaitForArchon":
                                    settings.bWaitForArchon = Convert.ToBoolean(config[1]);
                                    break;
                                case "BarbWaitForWrath":
                                    settings.bWaitForWrath = Convert.ToBoolean(config[1]);
                                    break;
                                case "BarbGoblinWrath":
                                    settings.bGoblinWrath = Convert.ToBoolean(config[1]);
                                    break;
                                case "BarbFuryDumpWrath":
                                    settings.bFuryDumpWrath = Convert.ToBoolean(config[1]);
                                    break;
                                case "BarbFuryDumpAlways":
                                    settings.bFuryDumpAlways = Convert.ToBoolean(config[1]);
                                    break;
                                case "EnableProwl":
                                    settings.bEnableProwl = Convert.ToBoolean(config[1]);
                                    break;
                                case "EnableLegendaryNotifyScore":
                                    settings.bEnableLegendaryNotifyScore = Convert.ToBoolean(config[1]);
                                    break;
                                case "ProwlKey":
                                    sProwlAPIKey = config[1];
                                    break;
                                case "EnableEmail":
                                    settings.bEnableEmail = Convert.ToBoolean(config[1]);
                                    break;
                                case "EmailAddress":
                                    sEmailAddress = config[1];
                                    break;
                                case "EmailPassword":
                                    sEmailPassword = config[1];
                                    break;
                                case "EnableAndroid":
                                    settings.bEnableAndroid = Convert.ToBoolean(config[1]);
                                    break;
                                case "AndroidKey":
                                    sAndroidAPIKey = config[1];
                                    break;
                                case "KiteBarb":
                                    settings.iKiteDistanceBarb = Convert.ToInt32(config[1]);
                                    break;
                                case "KiteWiz":
                                    settings.iKiteDistanceWiz = Convert.ToInt32(config[1]);
                                    break;
                                case "KiteWitch":
                                    settings.iKiteDistanceWitch = Convert.ToInt32(config[1]);
                                    break;
                                case "KiteDemon":
                                    settings.iKiteDistanceDemon = Convert.ToInt32(config[1]);
                                    break;
                                case "PotBarb":
                                    settings.dEmergencyHealthPotionBarb = Convert.ToDouble(config[1]);
                                    break;
                                case "PotMonk":
                                    settings.dEmergencyHealthPotionMonk = Convert.ToDouble(config[1]);
                                    break;
                                case "PotWiz":
                                    settings.dEmergencyHealthPotionWiz = Convert.ToDouble(config[1]);
                                    break;
                                case "PotWitch":
                                    settings.dEmergencyHealthPotionWitch = Convert.ToDouble(config[1]);
                                    break;
                                case "PotDemon":
                                    settings.dEmergencyHealthPotionDemon = Convert.ToDouble(config[1]);
                                    break;
                                case "GlobeBarb":
                                    settings.dEmergencyHealthGlobeBarb = Convert.ToDouble(config[1]);
                                    break;
                                case "GlobeMonk":
                                    settings.dEmergencyHealthGlobeMonk = Convert.ToDouble(config[1]);
                                    break;
                                case "GlobeWiz":
                                    settings.dEmergencyHealthGlobeWiz = Convert.ToDouble(config[1]);
                                    break;
                                case "GlobeWitch":
                                    settings.dEmergencyHealthGlobeWitch = Convert.ToDouble(config[1]);
                                    break;
                                case "GlobeDemon":
                                    settings.dEmergencyHealthGlobeDemon = Convert.ToDouble(config[1]);
                                    break;
                                case "AOEBarbHealth":
                                    healthlevels = config[1].Split(new string[] { " " }, StringSplitOptions.None);
                                    for (int i = 1; i <= healthlevels.Length; i++)
                                    {
                                        switch (i)
                                        {
                                            case 1:
                                                dictAvoidanceHealthBarb[219702] = Convert.ToDouble(healthlevels[i - 1]);
                                                dictAvoidanceHealthBarb[221225] = Convert.ToDouble(healthlevels[i - 1]);
                                                break;
                                            case 2:
                                                dictAvoidanceHealthBarb[84608] = Convert.ToDouble(healthlevels[i - 1]);
                                                break;
                                            case 3:
                                                dictAvoidanceHealthBarb[4803] = Convert.ToDouble(healthlevels[i - 1]);
                                                dictAvoidanceHealthBarb[4804] = Convert.ToDouble(healthlevels[i - 1]);
                                                dictAvoidanceHealthBarb[224225] = Convert.ToDouble(healthlevels[i - 1]);
                                                dictAvoidanceHealthBarb[247987] = Convert.ToDouble(healthlevels[i - 1]);
                                                break;
                                            case 4:
                                                dictAvoidanceHealthBarb[95868] = Convert.ToDouble(healthlevels[i - 1]);
                                                break;
                                            case 5:
                                                dictAvoidanceHealthBarb[5482] = Convert.ToDouble(healthlevels[i - 1]);
                                                dictAvoidanceHealthBarb[6578] = Convert.ToDouble(healthlevels[i - 1]);
                                                break;
                                            case 6:
                                                dictAvoidanceHealthBarb[108869] = Convert.ToDouble(healthlevels[i - 1]);
                                                break;
                                            case 7:
                                                dictAvoidanceHealthBarb[402] = Convert.ToDouble(healthlevels[i - 1]);
                                                dictAvoidanceHealthBarb[223675] = Convert.ToDouble(healthlevels[i - 1]);
                                                break;
                                            case 8:
                                                dictAvoidanceHealthBarb[3865] = Convert.ToDouble(healthlevels[i - 1]);
                                                break;
                                            case 9:
                                                dictAvoidanceHealthBarb[5212] = Convert.ToDouble(healthlevels[i - 1]);
                                                break;
                                            case 10:
                                                dictAvoidanceHealthBarb[123124] = Convert.ToDouble(healthlevels[i - 1]);
                                                break;
                                            case 11:
                                                dictAvoidanceHealthBarb[123839] = Convert.ToDouble(healthlevels[i - 1]);
                                                break;
                                            case 12:
                                                dictAvoidanceHealthBarb[4103] = Convert.ToDouble(healthlevels[i - 1]);
                                                break;
                                            case 13:
                                                dictAvoidanceHealthBarb[93837] = Convert.ToDouble(healthlevels[i - 1]);
                                                break;
                                        }
                                    }
                                    break;
                                case "AOEMonkHealth":
                                    healthlevels = config[1].Split(new string[] { " " }, StringSplitOptions.None);
                                    for (int i = 1; i <= healthlevels.Length; i++)
                                    {
                                        switch (i)
                                        {
                                            case 1:
                                                dictAvoidanceHealthMonk[219702] = Convert.ToDouble(healthlevels[i - 1]);
                                                dictAvoidanceHealthMonk[221225] = Convert.ToDouble(healthlevels[i - 1]);
                                                break;
                                            case 2:
                                                dictAvoidanceHealthMonk[84608] = Convert.ToDouble(healthlevels[i - 1]);
                                                break;
                                            case 3:
                                                dictAvoidanceHealthMonk[4803] = Convert.ToDouble(healthlevels[i - 1]);
                                                dictAvoidanceHealthMonk[4804] = Convert.ToDouble(healthlevels[i - 1]);
                                                dictAvoidanceHealthMonk[224225] = Convert.ToDouble(healthlevels[i - 1]);
                                                dictAvoidanceHealthMonk[247987] = Convert.ToDouble(healthlevels[i - 1]);
                                                break;
                                            case 4:
                                                dictAvoidanceHealthMonk[95868] = Convert.ToDouble(healthlevels[i - 1]);
                                                break;
                                            case 5:
                                                dictAvoidanceHealthMonk[5482] = Convert.ToDouble(healthlevels[i - 1]);
                                                dictAvoidanceHealthMonk[6578] = Convert.ToDouble(healthlevels[i - 1]);
                                                break;
                                            case 6:
                                                dictAvoidanceHealthMonk[108869] = Convert.ToDouble(healthlevels[i - 1]);
                                                break;
                                            case 7:
                                                dictAvoidanceHealthMonk[402] = Convert.ToDouble(healthlevels[i - 1]);
                                                dictAvoidanceHealthMonk[223675] = Convert.ToDouble(healthlevels[i - 1]);
                                                break;
                                            case 8:
                                                dictAvoidanceHealthMonk[3865] = Convert.ToDouble(healthlevels[i - 1]);
                                                break;
                                            case 9:
                                                dictAvoidanceHealthMonk[5212] = Convert.ToDouble(healthlevels[i - 1]);
                                                break;
                                            case 10:
                                                dictAvoidanceHealthMonk[123124] = Convert.ToDouble(healthlevels[i - 1]);
                                                break;
                                            case 11:
                                                dictAvoidanceHealthMonk[123839] = Convert.ToDouble(healthlevels[i - 1]);
                                                break;
                                            case 12:
                                                dictAvoidanceHealthMonk[4103] = Convert.ToDouble(healthlevels[i - 1]);
                                                break;
                                            case 13:
                                                dictAvoidanceHealthMonk[93837] = Convert.ToDouble(healthlevels[i - 1]);
                                                break;
                                        }
                                    }
                                    break;
                                case "AOEWizardHealth":
                                    healthlevels = config[1].Split(new string[] { " " }, StringSplitOptions.None);
                                    for (int i = 1; i <= healthlevels.Length; i++)
                                    {
                                        switch (i)
                                        {
                                            case 1:
                                                dictAvoidanceHealthWizard[219702] = Convert.ToDouble(healthlevels[i - 1]);
                                                dictAvoidanceHealthWizard[221225] = Convert.ToDouble(healthlevels[i - 1]);
                                                break;
                                            case 2:
                                                dictAvoidanceHealthWizard[84608] = Convert.ToDouble(healthlevels[i - 1]);
                                                break;
                                            case 3:
                                                dictAvoidanceHealthWizard[4803] = Convert.ToDouble(healthlevels[i - 1]);
                                                dictAvoidanceHealthWizard[4804] = Convert.ToDouble(healthlevels[i - 1]);
                                                dictAvoidanceHealthWizard[224225] = Convert.ToDouble(healthlevels[i - 1]);
                                                dictAvoidanceHealthWizard[247987] = Convert.ToDouble(healthlevels[i - 1]);
                                                break;
                                            case 4:
                                                dictAvoidanceHealthWizard[95868] = Convert.ToDouble(healthlevels[i - 1]);
                                                break;
                                            case 5:
                                                dictAvoidanceHealthWizard[5482] = Convert.ToDouble(healthlevels[i - 1]);
                                                dictAvoidanceHealthWizard[6578] = Convert.ToDouble(healthlevels[i - 1]);
                                                break;
                                            case 6:
                                                dictAvoidanceHealthWizard[108869] = Convert.ToDouble(healthlevels[i - 1]);
                                                break;
                                            case 7:
                                                dictAvoidanceHealthWizard[402] = Convert.ToDouble(healthlevels[i - 1]);
                                                dictAvoidanceHealthWizard[223675] = Convert.ToDouble(healthlevels[i - 1]);
                                                break;
                                            case 8:
                                                dictAvoidanceHealthWizard[3865] = Convert.ToDouble(healthlevels[i - 1]);
                                                break;
                                            case 9:
                                                dictAvoidanceHealthWizard[5212] = Convert.ToDouble(healthlevels[i - 1]);
                                                break;
                                            case 10:
                                                dictAvoidanceHealthWizard[123124] = Convert.ToDouble(healthlevels[i - 1]);
                                                break;
                                            case 11:
                                                dictAvoidanceHealthWizard[123839] = Convert.ToDouble(healthlevels[i - 1]);
                                                break;
                                            case 12:
                                                dictAvoidanceHealthWizard[4103] = Convert.ToDouble(healthlevels[i - 1]);
                                                break;
                                            case 13:
                                                dictAvoidanceHealthWizard[93837] = Convert.ToDouble(healthlevels[i - 1]);
                                                break;
                                        }
                                    }
                                    break;
                                case "AOEWitchHealth":
                                    healthlevels = config[1].Split(new string[] { " " }, StringSplitOptions.None);
                                    for (int i = 1; i <= healthlevels.Length; i++)
                                    {
                                        switch (i)
                                        {
                                            case 1:
                                                dictAvoidanceHealthWitch[219702] = Convert.ToDouble(healthlevels[i - 1]);
                                                dictAvoidanceHealthWitch[221225] = Convert.ToDouble(healthlevels[i - 1]);
                                                break;
                                            case 2:
                                                dictAvoidanceHealthWitch[84608] = Convert.ToDouble(healthlevels[i - 1]);
                                                break;
                                            case 3:
                                                dictAvoidanceHealthWitch[4803] = Convert.ToDouble(healthlevels[i - 1]);
                                                dictAvoidanceHealthWitch[4804] = Convert.ToDouble(healthlevels[i - 1]);
                                                dictAvoidanceHealthWitch[224225] = Convert.ToDouble(healthlevels[i - 1]);
                                                dictAvoidanceHealthWitch[247987] = Convert.ToDouble(healthlevels[i - 1]);
                                                break;
                                            case 4:
                                                dictAvoidanceHealthWitch[95868] = Convert.ToDouble(healthlevels[i - 1]);
                                                break;
                                            case 5:
                                                dictAvoidanceHealthWitch[5482] = Convert.ToDouble(healthlevels[i - 1]);
                                                dictAvoidanceHealthWitch[6578] = Convert.ToDouble(healthlevels[i - 1]);
                                                break;
                                            case 6:
                                                dictAvoidanceHealthWitch[108869] = Convert.ToDouble(healthlevels[i - 1]);
                                                break;
                                            case 7:
                                                dictAvoidanceHealthWitch[402] = Convert.ToDouble(healthlevels[i - 1]);
                                                dictAvoidanceHealthWitch[223675] = Convert.ToDouble(healthlevels[i - 1]);
                                                break;
                                            case 8:
                                                dictAvoidanceHealthWitch[3865] = Convert.ToDouble(healthlevels[i - 1]);
                                                break;
                                            case 9:
                                                dictAvoidanceHealthWitch[5212] = Convert.ToDouble(healthlevels[i - 1]);
                                                break;
                                            case 10:
                                                dictAvoidanceHealthWitch[123124] = Convert.ToDouble(healthlevels[i - 1]);
                                                break;
                                            case 11:
                                                dictAvoidanceHealthWitch[123839] = Convert.ToDouble(healthlevels[i - 1]);
                                                break;
                                            case 12:
                                                dictAvoidanceHealthWitch[4103] = Convert.ToDouble(healthlevels[i - 1]);
                                                break;
                                            case 13:
                                                dictAvoidanceHealthWitch[93837] = Convert.ToDouble(healthlevels[i - 1]);
                                                break;
                                        }
                                    }
                                    break;
                                case "AOEDemonHealth":
                                    healthlevels = config[1].Split(new string[] { " " }, StringSplitOptions.None);
                                    for (int i = 1; i <= healthlevels.Length; i++)
                                    {
                                        switch (i)
                                        {
                                            case 1:
                                                dictAvoidanceHealthDemon[219702] = Convert.ToDouble(healthlevels[i - 1]);
                                                dictAvoidanceHealthDemon[221225] = Convert.ToDouble(healthlevels[i - 1]);
                                                break;
                                            case 2:
                                                dictAvoidanceHealthDemon[84608] = Convert.ToDouble(healthlevels[i - 1]);
                                                break;
                                            case 3:
                                                dictAvoidanceHealthDemon[4803] = Convert.ToDouble(healthlevels[i - 1]);
                                                dictAvoidanceHealthDemon[4804] = Convert.ToDouble(healthlevels[i - 1]);
                                                dictAvoidanceHealthDemon[224225] = Convert.ToDouble(healthlevels[i - 1]);
                                                dictAvoidanceHealthDemon[247987] = Convert.ToDouble(healthlevels[i - 1]);
                                                break;
                                            case 4:
                                                dictAvoidanceHealthDemon[95868] = Convert.ToDouble(healthlevels[i - 1]);
                                                break;
                                            case 5:
                                                dictAvoidanceHealthDemon[5482] = Convert.ToDouble(healthlevels[i - 1]);
                                                dictAvoidanceHealthDemon[6578] = Convert.ToDouble(healthlevels[i - 1]);
                                                break;
                                            case 6:
                                                dictAvoidanceHealthDemon[108869] = Convert.ToDouble(healthlevels[i - 1]);
                                                break;
                                            case 7:
                                                dictAvoidanceHealthDemon[402] = Convert.ToDouble(healthlevels[i - 1]);
                                                dictAvoidanceHealthDemon[223675] = Convert.ToDouble(healthlevels[i - 1]);
                                                break;
                                            case 8:
                                                dictAvoidanceHealthDemon[3865] = Convert.ToDouble(healthlevels[i - 1]);
                                                break;
                                            case 9:
                                                dictAvoidanceHealthDemon[5212] = Convert.ToDouble(healthlevels[i - 1]);
                                                break;
                                            case 10:
                                                dictAvoidanceHealthDemon[123124] = Convert.ToDouble(healthlevels[i - 1]);
                                                break;
                                            case 11:
                                                dictAvoidanceHealthDemon[123839] = Convert.ToDouble(healthlevels[i - 1]);
                                                break;
                                            case 12:
                                                dictAvoidanceHealthDemon[4103] = Convert.ToDouble(healthlevels[i - 1]);
                                                break;
                                            case 13:
                                                dictAvoidanceHealthDemon[93837] = Convert.ToDouble(healthlevels[i - 1]);
                                                break;
                                        }
                                    }
                                    break;
                                case "AOERadius":
                                    healthlevels = config[1].Split(new string[] { " " }, StringSplitOptions.None);
                                    for (int i = 1; i <= healthlevels.Length; i++)
                                    {
                                        switch (i)
                                        {
                                            case 1:
                                                dictAvoidanceRadius[219702] = Convert.ToInt32(healthlevels[i - 1]);
                                                dictAvoidanceRadius[221225] = Convert.ToInt32(healthlevels[i - 1]);
                                                break;
                                            case 2:
                                                dictAvoidanceRadius[84608] = Convert.ToInt32(healthlevels[i - 1]);
                                                break;
                                            case 3:
                                                dictAvoidanceRadius[4803] = Convert.ToInt32(healthlevels[i - 1]);
                                                dictAvoidanceRadius[4804] = Convert.ToInt32(healthlevels[i - 1]);
                                                dictAvoidanceRadius[224225] = Convert.ToInt32(healthlevels[i - 1]);
                                                dictAvoidanceRadius[247987] = Convert.ToInt32(healthlevels[i - 1]);
                                                break;
                                            case 4:
                                                dictAvoidanceRadius[95868] = Convert.ToInt32(healthlevels[i - 1]);
                                                break;
                                            case 5:
                                                dictAvoidanceRadius[5482] = Convert.ToInt32(healthlevels[i - 1]);
                                                dictAvoidanceRadius[6578] = Convert.ToInt32(healthlevels[i - 1]);
                                                break;
                                            case 6:
                                                dictAvoidanceRadius[108869] = Convert.ToInt32(healthlevels[i - 1]);
                                                break;
                                            case 7:
                                                dictAvoidanceRadius[402] = Convert.ToInt32(healthlevels[i - 1]);
                                                dictAvoidanceRadius[223675] = Convert.ToInt32(healthlevels[i - 1]);
                                                break;
                                            case 8:
                                                dictAvoidanceRadius[3865] = Convert.ToInt32(healthlevels[i - 1]);
                                                break;
                                            case 9:
                                                dictAvoidanceRadius[5212] = Convert.ToInt32(healthlevels[i - 1]);
                                                break;
                                            case 10:
                                                dictAvoidanceRadius[123124] = Convert.ToInt32(healthlevels[i - 1]);
                                                break;
                                            case 11:
                                                dictAvoidanceRadius[123839] = Convert.ToInt32(healthlevels[i - 1]);
                                                break;
                                            case 12:
                                                dictAvoidanceRadius[4103] = Convert.ToInt32(healthlevels[i - 1]);
                                                break;
                                            case 13:
                                                dictAvoidanceRadius[93837] = Convert.ToInt32(healthlevels[i - 1]);
                                                break;
                                        }
                                    }
                                    break;
                            }
                        }
                    }
                    configReader.Close();
                }
            }
            catch
            {
                SaveConfiguration();
                return;
            }
            if (!File.Exists(sTrinityEmailConfigFile))
            {
                saveEmailConfiguration();
            }
            using (StreamReader configReader = new StreamReader(sTrinityEmailConfigFile))
            {
                while (!configReader.EndOfStream)
                {
                    string[] config = configReader.ReadLine().Split('=');
                    if (config != null)
                    {
                        switch (config[0])
                        {
                            case "EnableEmail":
                                settings.bEnableEmail = Convert.ToBoolean(config[1]);
                                break;
                            case "EmailAddress":
                                sEmailAddress = config[1];
                                break;
                            case "EmailPassword":
                                sEmailPassword = config[1];
                                break;
                            case "BotName":
                                sBotName = config[1];
                                break;
                        }
                    }
                }
                configReader.Close();
            }
            bMappedPlayerAbilities = false;
        }
        // * CONFIG WINDOW REGION
        #region configWindow
        // First we create a variable that is of the "type" of the actual config window item - eg a "RadioButton" for each, well, radiobutton
        // Later on we will "Link" these variables to the ACTUAL items within the XAML file, so we can do things with the XAML stuff
        // I try to match the names of the variables here, with the "Name=" I give the item in the XAML - this isn't necessary, but makes things simpler
        private Button saveButton, defaultButton, testButton, sortButton, resetCombat, resetAOE0, resetAOE1, resetAOE2, resetAOE3, resetAOE4, resetWorld, resetItems, resetTown, resetAdvanced, resetMobile;
        private RadioButton checkTreasureIgnore, checkTreasureNormal, checkTreasurePrioritize, checkTreasureKamikaze, btnRulesGiles, btnRulesCustom, btnSalvage, btnSell, checkIgnoreAll, checkIgnoreNone;
        private CheckBox checkAvoidance, checkGlobes, checkCritical, checkGrave, checkBacktracking, checkCraftTomes, checkDesigns, checkFollower, checkGemEmerald, checkGemAmethyst, checkGemTopaz, checkGemRuby,
            checkIgnoreCorpses, checkMovementAbilities, checkTPS, checkLogStucks, checkUnstucker, checkExtendedRange, checkDebugInfo, checkProwl, checkAndroid, checkSelectiveWW,
            checkWaitWrath, checkGoblinWrath, checkFuryDumpWrath, checkFuryDumpAlways, checkProfileReload, checkMonkInna, checkKiteArchonOnly, checkWaitArchonAzmo, checkWrath90, checkEmail, checkLegendaryNotify;
        private Slider slideTriggerRange, slideWeapon, slideJewelry, slideArmor, slideGoldAmount, slideContainerRange, slideDestructibleRange, slideTPS,
            slideNotifyWeapon, slideNotifyJewelry, slideNotifyArmor, slideLootDelay, slideVaultDelay, slideKite0, slideKite2, slideKite3, slideKite4;
        private TextBox textTriggerRange, JewelryText, ArmorText, WeaponText, JewelryNotifyText, ArmorNotifyText, WeaponNotifyText, textGoldAmount, textContainerRange, textDestructibleRange,
            textTPS, textProwlKey, textAndroidKey, textLootDelay, textVaultDelay, textKite0, textKite2, textKite3, textKite4, txtEmailAddress, txtEmailPassword, txtBotName;
        private ComboBox comboWB, comboWY, comboAB, comboAY, comboJB, comboJY, comboGems, comboMisc, comboPotions, comboPotionLevel, comboLegendary;
        // I used an array of sliders for all the AOE stuff because there were just too many to handle separately, and they all affect the same sort of values
        // So looping through arrays and doing things to them this way meant less code and easier to add more AOE stuff in the future
        private Slider[,] slideAOERadius = new Slider[5, 13];
        private Slider[,] slideAOEHealth = new Slider[5, 13];
        private TextBox[,] textAOERadius = new TextBox[5, 13];
        private TextBox[,] textAOEHealth = new TextBox[5, 13];
        // Sliders for health potions, and health globes, for each class
        private Slider slidePot0, slidePot1, slidePot2, slidePot3, slidePot4, slideGlobe0, slideGlobe1, slideGlobe2, slideGlobe3, slideGlobe4;
        private TextBox textPot0, textPot1, textPot2, textPot3, textPot4, textGlobe0, textGlobe1, textGlobe2, textGlobe3, textGlobe4;
        // This is needed by DB, is essentially the ACTUAL window object itself
        private Window configWindow;
        // This is what "creates" the window
        public Window DisplayWindow
        {
            get
            {
                // Check we can actually find the .xaml file first - if not, report an error
                if (!File.Exists(sTrinityPluginPath + "GilesTrinity.xaml"))
                    Log("ERROR: Can't find \"" + sTrinityPluginPath + "GilesTrinity.xaml\"");
                try
                {
                    if (configWindow == null)
                    {
                        configWindow = new Window();
                    }
                    StreamReader xamlStream = new StreamReader(sTrinityPluginPath + "GilesTrinity.xaml");
                    DependencyObject xamlContent = XamlReader.Load(xamlStream.BaseStream) as DependencyObject;
                    configWindow.Content = xamlContent;
                    // I'm not going to comment everything below - it's all pretty similar
                    // Basically the concept is this:
                    // You take the variable you created above (30 lines up or so), and you use "FindLogicalNode" to sort of "link" the variable, to that object within the XAML file
                    // By using the "Name" tag as the way of finding it
                    // After assigning the variable to the actual node in the XAML, you then need to add event handlers - so we can do things when the user makes changes to those elements
                    // You can also alter settings and values of the nodes - eg the min-max values, the current value etc. - by using the variable we link
                    // Now - the huge list below is because I have so many damned config options of different types!
                    // Note that I do *NOT* have any events on text boxes - because I set all textboxes to uneditable/unchangeable - they are "read only"
                    // I simply use them to show the user what the slider-value is currently set to (so when the slider changes, my code updates the text box)
                    slideWeapon = LogicalTreeHelper.FindLogicalNode(xamlContent, "slideWeaponScore") as Slider;
                    slideWeapon.ValueChanged += trackScoreWeapons_Scroll;
                    slideWeapon.SmallChange = 200;
                    slideWeapon.LargeChange = 1000;
                    slideWeapon.TickFrequency = 2000;
                    slideWeapon.IsSnapToTickEnabled = true;
                    slideJewelry = LogicalTreeHelper.FindLogicalNode(xamlContent, "sliderJewelryScore") as Slider;
                    slideJewelry.ValueChanged += trackScoreJewelry_Scroll;
                    slideJewelry.SmallChange = 100;
                    slideJewelry.LargeChange = 500;
                    slideJewelry.TickFrequency = 1000;
                    slideJewelry.IsSnapToTickEnabled = true;
                    slideArmor = LogicalTreeHelper.FindLogicalNode(xamlContent, "sliderArmorScore") as Slider;
                    slideArmor.ValueChanged += trackScoreArmor_Scroll;
                    slideArmor.SmallChange = 100;
                    slideArmor.LargeChange = 500;
                    slideArmor.TickFrequency = 1000;
                    slideArmor.IsSnapToTickEnabled = true;
                    slideGoldAmount = LogicalTreeHelper.FindLogicalNode(xamlContent, "slideGoldAmount") as Slider;
                    slideGoldAmount.ValueChanged += trackGoldAmount_Scroll;
                    slideGoldAmount.SmallChange = 5;
                    slideGoldAmount.LargeChange = 20;
                    slideGoldAmount.TickFrequency = 50;
                    slideGoldAmount.IsSnapToTickEnabled = true;
                    slideNotifyWeapon = LogicalTreeHelper.FindLogicalNode(xamlContent, "slideWeaponNotifyScore") as Slider;
                    slideNotifyWeapon.ValueChanged += trackNotifyWeapons_Scroll;
                    slideNotifyWeapon.SmallChange = 200;
                    slideNotifyWeapon.LargeChange = 1000;
                    slideNotifyWeapon.TickFrequency = 2000;
                    slideNotifyWeapon.IsSnapToTickEnabled = true;
                    slideNotifyJewelry = LogicalTreeHelper.FindLogicalNode(xamlContent, "slideJewelryNotifyScore") as Slider;
                    slideNotifyJewelry.ValueChanged += trackNotifyJewelry_Scroll;
                    slideNotifyJewelry.SmallChange = 100;
                    slideNotifyJewelry.LargeChange = 500;
                    slideNotifyJewelry.TickFrequency = 1000;
                    slideNotifyJewelry.IsSnapToTickEnabled = true;
                    slideNotifyArmor = LogicalTreeHelper.FindLogicalNode(xamlContent, "slideArmorNotifyScore") as Slider;
                    slideNotifyArmor.ValueChanged += trackNotifyArmor_Scroll;
                    slideNotifyArmor.SmallChange = 100;
                    slideNotifyArmor.LargeChange = 500;
                    slideNotifyArmor.TickFrequency = 1000;
                    slideNotifyArmor.IsSnapToTickEnabled = true;
                    textGoldAmount = LogicalTreeHelper.FindLogicalNode(xamlContent, "textGoldAmount") as TextBox;
                    JewelryText = LogicalTreeHelper.FindLogicalNode(xamlContent, "JewelryScore") as TextBox;
                    ArmorText = LogicalTreeHelper.FindLogicalNode(xamlContent, "ArmorScore") as TextBox;
                    WeaponText = LogicalTreeHelper.FindLogicalNode(xamlContent, "WeaponScore") as TextBox;
                    JewelryNotifyText = LogicalTreeHelper.FindLogicalNode(xamlContent, "JewelryNotifyScore") as TextBox;
                    ArmorNotifyText = LogicalTreeHelper.FindLogicalNode(xamlContent, "ArmorNotifyScore") as TextBox;
                    WeaponNotifyText = LogicalTreeHelper.FindLogicalNode(xamlContent, "WeaponNotifyScore") as TextBox;
                    comboWB = LogicalTreeHelper.FindLogicalNode(xamlContent, "comboWB") as ComboBox;
                    comboWB.SelectionChanged += comboWB_changed;
                    comboWY = LogicalTreeHelper.FindLogicalNode(xamlContent, "comboWY") as ComboBox;
                    comboWY.SelectionChanged += comboWY_changed;
                    comboAB = LogicalTreeHelper.FindLogicalNode(xamlContent, "comboAB") as ComboBox;
                    comboAB.SelectionChanged += comboAB_changed;
                    comboAY = LogicalTreeHelper.FindLogicalNode(xamlContent, "comboAY") as ComboBox;
                    comboAY.SelectionChanged += comboAY_changed;
                    comboJB = LogicalTreeHelper.FindLogicalNode(xamlContent, "comboJB") as ComboBox;
                    comboJB.SelectionChanged += comboJB_changed;
                    comboJY = LogicalTreeHelper.FindLogicalNode(xamlContent, "comboJY") as ComboBox;
                    comboJY.SelectionChanged += comboJY_changed;
                    comboGems = LogicalTreeHelper.FindLogicalNode(xamlContent, "comboGems") as ComboBox;
                    comboGems.SelectionChanged += comboGems_changed;
                    comboMisc = LogicalTreeHelper.FindLogicalNode(xamlContent, "comboMisc") as ComboBox;
                    comboMisc.SelectionChanged += comboMisc_changed;
                    comboPotions = LogicalTreeHelper.FindLogicalNode(xamlContent, "comboPotions") as ComboBox;
                    comboPotions.SelectionChanged += comboPotions_changed;
                    comboPotionLevel = LogicalTreeHelper.FindLogicalNode(xamlContent, "comboPotionLevel") as ComboBox;
                    comboPotionLevel.SelectionChanged += comboPotionLevel_changed;
                    comboLegendary = LogicalTreeHelper.FindLogicalNode(xamlContent, "comboLegendary") as ComboBox;
                    comboLegendary.SelectionChanged += comboLegendary_changed;
                    checkCraftTomes = LogicalTreeHelper.FindLogicalNode(xamlContent, "checkCraftTomes") as CheckBox;
                    checkCraftTomes.Checked += checkCraftTomes_check;
                    checkCraftTomes.Unchecked += checkCraftTomes_uncheck;
                    checkDesigns = LogicalTreeHelper.FindLogicalNode(xamlContent, "checkDesigns") as CheckBox;
                    checkDesigns.Checked += checkDesigns_check;
                    checkDesigns.Unchecked += checkDesigns_uncheck;
                    checkFollower = LogicalTreeHelper.FindLogicalNode(xamlContent, "checkFollower") as CheckBox;
                    checkFollower.Checked += checkFollower_check;
                    checkFollower.Unchecked += checkFollower_uncheck;
                    checkGemEmerald = LogicalTreeHelper.FindLogicalNode(xamlContent, "checkGemEmerald") as CheckBox;
                    checkGemEmerald.Checked += checkEmerald_check;
                    checkGemEmerald.Unchecked += checkEmerald_uncheck;
                    checkGemTopaz = LogicalTreeHelper.FindLogicalNode(xamlContent, "checkGemTopaz") as CheckBox;
                    checkGemTopaz.Checked += checkTopaz_check;
                    checkGemTopaz.Unchecked += checkTopaz_uncheck;
                    checkGemAmethyst = LogicalTreeHelper.FindLogicalNode(xamlContent, "checkGemAmethyst") as CheckBox;
                    checkGemAmethyst.Checked += checkAmethyst_check;
                    checkGemAmethyst.Unchecked += checkAmethyst_uncheck;
                    checkGemRuby = LogicalTreeHelper.FindLogicalNode(xamlContent, "checkGemRuby") as CheckBox;
                    checkGemRuby.Checked += checkRuby_check;
                    checkGemRuby.Unchecked += checkRuby_uncheck;
                    checkMovementAbilities = LogicalTreeHelper.FindLogicalNode(xamlContent, "checkMovementAbilities") as CheckBox;
                    checkMovementAbilities.Checked += checkMovementAbilities_check;
                    checkMovementAbilities.Unchecked += checkMovementAbilities_uncheck;
                    btnRulesGiles = LogicalTreeHelper.FindLogicalNode(xamlContent, "btnRulesGiles") as RadioButton;
                    btnRulesGiles.Checked += btnRulesGiles_check;
                    btnRulesCustom = LogicalTreeHelper.FindLogicalNode(xamlContent, "btnRulesCustom") as RadioButton;
                    btnRulesCustom.Checked += btnRulesCustom_check;
                    btnSalvage = LogicalTreeHelper.FindLogicalNode(xamlContent, "btnSalvage") as RadioButton;
                    btnSalvage.Checked += btnSalvage_check;
                    btnSell = LogicalTreeHelper.FindLogicalNode(xamlContent, "btnSell") as RadioButton;
                    btnSell.Checked += btnSell_check;
                    testButton = LogicalTreeHelper.FindLogicalNode(xamlContent, "buttonTest") as Button;
                    testButton.Click += buttonTest_Click;
                    sortButton = LogicalTreeHelper.FindLogicalNode(xamlContent, "buttonSort") as Button;
                    sortButton.Click += buttonSort_Click;
                    checkTreasureIgnore = LogicalTreeHelper.FindLogicalNode(xamlContent, "btnTreasureIgnore") as RadioButton;
                    checkTreasureIgnore.Checked += checkTreasureIgnore_check;
                    checkTreasureNormal = LogicalTreeHelper.FindLogicalNode(xamlContent, "btnTreasureNormal") as RadioButton;
                    checkTreasureNormal.Checked += checkTreasureNormal_check;
                    checkTreasurePrioritize = LogicalTreeHelper.FindLogicalNode(xamlContent, "btnTreasurePrioritize") as RadioButton;
                    checkTreasurePrioritize.Checked += checkTreasurePrioritize_check;
                    checkTreasureKamikaze = LogicalTreeHelper.FindLogicalNode(xamlContent, "btnTreasureKamikaze") as RadioButton;
                    checkTreasureKamikaze.Checked += checkTreasureKamikaze_check;
                    checkDebugInfo = LogicalTreeHelper.FindLogicalNode(xamlContent, "checkDebugInfo") as CheckBox;
                    checkDebugInfo.Checked += checkDebugInfo_check;
                    checkDebugInfo.Unchecked += checkDebugInfo_uncheck;
                    // prowl stuff
                    checkProwl = LogicalTreeHelper.FindLogicalNode(xamlContent, "checkProwl") as CheckBox;
                    checkProwl.Checked += checkProwl_check;
                    checkProwl.Unchecked += checkProwl_uncheck;
                    textProwlKey = LogicalTreeHelper.FindLogicalNode(xamlContent, "txtProwlAPI") as TextBox;
                    textProwlKey.TextChanged += textProwl_change;
                    checkLegendaryNotify = LogicalTreeHelper.FindLogicalNode(xamlContent, "checkLegendaryNotify") as CheckBox;
                    checkLegendaryNotify.Checked += checkLegendaryNotifyScore_check;
                    checkLegendaryNotify.Unchecked += checkLegendaryNotifyScore_uncheck;
                    //Email stuff
                    checkEmail = LogicalTreeHelper.FindLogicalNode(xamlContent, "checkEmail") as CheckBox;
                    checkEmail.Checked += checkEmail_check;
                    checkEmail.Unchecked += checkEmail_uncheck;
                    txtEmailAddress = LogicalTreeHelper.FindLogicalNode(xamlContent, "txtEmailAddress") as TextBox;
                    txtEmailAddress.TextChanged += txtEmailAddress_change;
                    txtEmailPassword = LogicalTreeHelper.FindLogicalNode(xamlContent, "txtEmailPassword") as TextBox;
                    txtEmailPassword.TextChanged += txtEmailPassword_change;
                    txtBotName = LogicalTreeHelper.FindLogicalNode(xamlContent, "txtBotName") as TextBox;
                    txtBotName.TextChanged += txtBotName_change;
                    // android stuff
                    checkAndroid = LogicalTreeHelper.FindLogicalNode(xamlContent, "checkAndroid") as CheckBox;
                    checkAndroid.Checked += checkAndroid_check;
                    checkAndroid.Unchecked += checkAndroid_uncheck;
                    textAndroidKey = LogicalTreeHelper.FindLogicalNode(xamlContent, "txtAndroidAPI") as TextBox;
                    textAndroidKey.TextChanged += textAndroid_change;
                    checkTPS = LogicalTreeHelper.FindLogicalNode(xamlContent, "checkTPS") as CheckBox;
                    checkTPS.Checked += checkTPS_check;
                    checkTPS.Unchecked += checkTPS_uncheck;
                    textTPS = LogicalTreeHelper.FindLogicalNode(xamlContent, "textTPS") as TextBox;
                    slideTPS = LogicalTreeHelper.FindLogicalNode(xamlContent, "slideTPS") as Slider;
                    slideTPS.ValueChanged += trackTPS_Scroll;
                    slideTPS.SmallChange = 1;
                    slideTPS.LargeChange = 1;
                    slideTPS.TickFrequency = 5;
                    slideTPS.IsSnapToTickEnabled = false;
                    checkLogStucks = LogicalTreeHelper.FindLogicalNode(xamlContent, "checkLogStucks") as CheckBox;
                    checkLogStucks.Checked += checkLogStucks_check;
                    checkLogStucks.Unchecked += checkLogStucks_uncheck;
                    checkUnstucker = LogicalTreeHelper.FindLogicalNode(xamlContent, "checkUnstucker") as CheckBox;
                    checkUnstucker.Checked += checkUnstucker_check;
                    checkUnstucker.Unchecked += checkUnstucker_uncheck;
                    checkProfileReload = LogicalTreeHelper.FindLogicalNode(xamlContent, "checkProfileReload") as CheckBox;
                    checkProfileReload.Checked += checkProfileReload_check;
                    checkProfileReload.Unchecked += checkProfileReload_uncheck;
                    checkExtendedRange = LogicalTreeHelper.FindLogicalNode(xamlContent, "checkExtendedRange") as CheckBox;
                    checkExtendedRange.Checked += checkExtendedRange_check;
                    checkExtendedRange.Unchecked += checkExtendedRange_uncheck;
                    checkSelectiveWW = LogicalTreeHelper.FindLogicalNode(xamlContent, "checkSelectiveWW") as CheckBox;
                    checkSelectiveWW.Checked += checkSelectiveWW_check;
                    checkSelectiveWW.Unchecked += checkSelectiveWW_uncheck;
                    checkWrath90 = LogicalTreeHelper.FindLogicalNode(xamlContent, "checkWrath90") as CheckBox;
                    checkWrath90.Checked += checkWrath90_check;
                    checkWrath90.Unchecked += checkWrath90_uncheck;
                    checkKiteArchonOnly = LogicalTreeHelper.FindLogicalNode(xamlContent, "checkKiteArchonOnly") as CheckBox;
                    checkKiteArchonOnly.Checked += checkKiteArchonOnly_check;
                    checkKiteArchonOnly.Unchecked += checkKiteArchonOnly_uncheck;
                    checkWaitArchonAzmo = LogicalTreeHelper.FindLogicalNode(xamlContent, "checkWaitArchonAzmo") as CheckBox;
                    checkWaitArchonAzmo.Checked += checkWaitArchonAzmo_check;
                    checkWaitArchonAzmo.Unchecked += checkWaitArchonAzmo_uncheck;
                    checkWaitWrath = LogicalTreeHelper.FindLogicalNode(xamlContent, "checkWaitWrath") as CheckBox;
                    checkWaitWrath.Checked += checkWaitWrath_check;
                    checkWaitWrath.Unchecked += checkWaitWrath_uncheck;
                    checkGoblinWrath = LogicalTreeHelper.FindLogicalNode(xamlContent, "checkGoblinWrath") as CheckBox;
                    checkGoblinWrath.Checked += checkGoblinWrath_check;
                    checkGoblinWrath.Unchecked += checkGoblinWrath_uncheck;
                    checkFuryDumpWrath = LogicalTreeHelper.FindLogicalNode(xamlContent, "checkFuryDumpWrath") as CheckBox;
                    checkFuryDumpWrath.Checked += checkFuryDumpWrath_check;
                    checkFuryDumpWrath.Unchecked += checkFuryDumpWrath_uncheck;
                    checkFuryDumpAlways = LogicalTreeHelper.FindLogicalNode(xamlContent, "checkFuryDumpAlways") as CheckBox;
                    checkFuryDumpAlways.Checked += checkFuryDumpAlways_check;
                    checkFuryDumpAlways.Unchecked += checkFuryDumpAlways_uncheck;
                    checkMonkInna = LogicalTreeHelper.FindLogicalNode(xamlContent, "checkMonkInna") as CheckBox;
                    checkMonkInna.Checked += checkMonkInna_check;
                    checkMonkInna.Unchecked += checkMonkInna_uncheck;
                    checkBacktracking = LogicalTreeHelper.FindLogicalNode(xamlContent, "checkBacktracking") as CheckBox;
                    checkBacktracking.Checked += checkBacktracking_check;
                    checkBacktracking.Unchecked += checkBacktracking_uncheck;
                    checkAvoidance = LogicalTreeHelper.FindLogicalNode(xamlContent, "checkAvoidance") as CheckBox;
                    checkAvoidance.Checked += checkAvoidance_check;
                    checkAvoidance.Unchecked += checkAvoidance_uncheck;
                    checkGlobes = LogicalTreeHelper.FindLogicalNode(xamlContent, "checkGlobes") as CheckBox;
                    checkGlobes.Checked += checkGlobes_check;
                    checkGlobes.Unchecked += checkGlobes_uncheck;
                    checkCritical = LogicalTreeHelper.FindLogicalNode(xamlContent, "checkCritical") as CheckBox;
                    checkCritical.Checked += checkCritical_check;
                    checkCritical.Unchecked += checkCritical_uncheck;
                    checkGrave = LogicalTreeHelper.FindLogicalNode(xamlContent, "checkGrave") as CheckBox;
                    checkGrave.Checked += checkCritical_check;
                    checkGrave.Unchecked += checkCritical_uncheck;
                    textTriggerRange = LogicalTreeHelper.FindLogicalNode(xamlContent, "textTriggerRange") as TextBox;
                    slideTriggerRange = LogicalTreeHelper.FindLogicalNode(xamlContent, "slideTriggerRange") as Slider;
                    slideTriggerRange.ValueChanged += trackTriggerRange_Scroll;
                    slideTriggerRange.SmallChange = 1;
                    slideTriggerRange.LargeChange = 1;
                    slideTriggerRange.TickFrequency = 5;
                    slideTriggerRange.IsSnapToTickEnabled = false;
                    slideLootDelay = LogicalTreeHelper.FindLogicalNode(xamlContent, "slideLootDelay") as Slider;
                    slideLootDelay.ValueChanged += slideLootDelay_Scroll;
                    slideLootDelay.SmallChange = 100;
                    slideLootDelay.LargeChange = 100;
                    slideLootDelay.TickFrequency = 100;
                    slideLootDelay.IsSnapToTickEnabled = true;
                    textLootDelay = LogicalTreeHelper.FindLogicalNode(xamlContent, "textLootDelay") as TextBox;
                    slideVaultDelay = LogicalTreeHelper.FindLogicalNode(xamlContent, "slideVaultDelay") as Slider;
                    slideVaultDelay.ValueChanged += slideVaultDelay_Scroll;
                    slideVaultDelay.SmallChange = 100;
                    slideVaultDelay.LargeChange = 100;
                    slideVaultDelay.TickFrequency = 100;
                    slideVaultDelay.IsSnapToTickEnabled = true;
                    textVaultDelay = LogicalTreeHelper.FindLogicalNode(xamlContent, "textVaultDelay") as TextBox;
                    slideKite0 = LogicalTreeHelper.FindLogicalNode(xamlContent, "slideKite0") as Slider;
                    slideKite0.ValueChanged += slideKite0_Scroll;
                    slideKite0.IsSnapToTickEnabled = true;
                    textKite0 = LogicalTreeHelper.FindLogicalNode(xamlContent, "textKite0") as TextBox;
                    slideKite2 = LogicalTreeHelper.FindLogicalNode(xamlContent, "slideKite2") as Slider;
                    slideKite2.ValueChanged += slideKite2_Scroll;
                    slideKite2.IsSnapToTickEnabled = true;
                    textKite2 = LogicalTreeHelper.FindLogicalNode(xamlContent, "textKite2") as TextBox;
                    slideKite3 = LogicalTreeHelper.FindLogicalNode(xamlContent, "slideKite3") as Slider;
                    slideKite3.ValueChanged += slideKite3_Scroll;
                    slideKite3.IsSnapToTickEnabled = true;
                    textKite3 = LogicalTreeHelper.FindLogicalNode(xamlContent, "textKite3") as TextBox;
                    slideKite4 = LogicalTreeHelper.FindLogicalNode(xamlContent, "slideKite4") as Slider;
                    slideKite4.ValueChanged += slideKite4_Scroll;
                    slideKite4.IsSnapToTickEnabled = true;
                    textKite4 = LogicalTreeHelper.FindLogicalNode(xamlContent, "textKite4") as TextBox;
                    checkIgnoreAll = LogicalTreeHelper.FindLogicalNode(xamlContent, "btnIgnoreAll") as RadioButton;
                    checkIgnoreAll.Checked += checkIgnoreAll_check;
                    checkIgnoreNone = LogicalTreeHelper.FindLogicalNode(xamlContent, "btnIgnoreNone") as RadioButton;
                    checkIgnoreNone.Checked += checkIgnoreNone_check;
                    checkIgnoreCorpses = LogicalTreeHelper.FindLogicalNode(xamlContent, "checkIgnoreCorpses") as CheckBox;
                    checkIgnoreCorpses.Checked += checkIgnoreCorpses_check;
                    checkIgnoreCorpses.Unchecked += checkIgnoreCorpses_uncheck;
                    textContainerRange = LogicalTreeHelper.FindLogicalNode(xamlContent, "textContainerRange") as TextBox;
                    textDestructibleRange = LogicalTreeHelper.FindLogicalNode(xamlContent, "textDestructibleRange") as TextBox;
                    slideContainerRange = LogicalTreeHelper.FindLogicalNode(xamlContent, "slideContainerRange") as Slider;
                    slideContainerRange.ValueChanged += trackContainerRange_Scroll;
                    slideContainerRange.SmallChange = 1;
                    slideContainerRange.LargeChange = 1;
                    slideContainerRange.TickFrequency = 5;
                    slideContainerRange.IsSnapToTickEnabled = false;
                    slideDestructibleRange = LogicalTreeHelper.FindLogicalNode(xamlContent, "slideDestructibleRange") as Slider;
                    slideDestructibleRange.ValueChanged += trackDestructibleRange_Scroll;
                    slideDestructibleRange.SmallChange = 1;
                    slideDestructibleRange.LargeChange = 1;
                    slideDestructibleRange.TickFrequency = 5;
                    slideDestructibleRange.IsSnapToTickEnabled = false;
                    // Globe & pot sliders
                    slidePot0 = LogicalTreeHelper.FindLogicalNode(xamlContent, "slidePot0") as Slider;
                    slidePot0.ValueChanged += slidePot0_Scroll;
                    slidePot1 = LogicalTreeHelper.FindLogicalNode(xamlContent, "slidePot1") as Slider;
                    slidePot1.ValueChanged += slidePot1_Scroll;
                    slidePot2 = LogicalTreeHelper.FindLogicalNode(xamlContent, "slidePot2") as Slider;
                    slidePot2.ValueChanged += slidePot2_Scroll;
                    slidePot3 = LogicalTreeHelper.FindLogicalNode(xamlContent, "slidePot3") as Slider;
                    slidePot3.ValueChanged += slidePot3_Scroll;
                    slidePot4 = LogicalTreeHelper.FindLogicalNode(xamlContent, "slidePot4") as Slider;
                    slidePot4.ValueChanged += slidePot4_Scroll;
                    slideGlobe0 = LogicalTreeHelper.FindLogicalNode(xamlContent, "slideGlobe0") as Slider;
                    slideGlobe0.ValueChanged += slideGlobe0_Scroll;
                    slideGlobe1 = LogicalTreeHelper.FindLogicalNode(xamlContent, "slideGlobe1") as Slider;
                    slideGlobe1.ValueChanged += slideGlobe1_Scroll;
                    slideGlobe2 = LogicalTreeHelper.FindLogicalNode(xamlContent, "slideGlobe2") as Slider;
                    slideGlobe2.ValueChanged += slideGlobe2_Scroll;
                    slideGlobe3 = LogicalTreeHelper.FindLogicalNode(xamlContent, "slideGlobe3") as Slider;
                    slideGlobe3.ValueChanged += slideGlobe3_Scroll;
                    slideGlobe4 = LogicalTreeHelper.FindLogicalNode(xamlContent, "slideGlobe4") as Slider;
                    slideGlobe4.ValueChanged += slideGlobe4_Scroll;
                    textPot0 = LogicalTreeHelper.FindLogicalNode(xamlContent, "textPot0") as TextBox;
                    textPot1 = LogicalTreeHelper.FindLogicalNode(xamlContent, "textPot1") as TextBox;
                    textPot2 = LogicalTreeHelper.FindLogicalNode(xamlContent, "textPot2") as TextBox;
                    textPot3 = LogicalTreeHelper.FindLogicalNode(xamlContent, "textPot3") as TextBox;
                    textPot4 = LogicalTreeHelper.FindLogicalNode(xamlContent, "textPot4") as TextBox;
                    textGlobe0 = LogicalTreeHelper.FindLogicalNode(xamlContent, "textGlobe0") as TextBox;
                    textGlobe1 = LogicalTreeHelper.FindLogicalNode(xamlContent, "textGlobe1") as TextBox;
                    textGlobe2 = LogicalTreeHelper.FindLogicalNode(xamlContent, "textGlobe2") as TextBox;
                    textGlobe3 = LogicalTreeHelper.FindLogicalNode(xamlContent, "textGlobe3") as TextBox;
                    textGlobe4 = LogicalTreeHelper.FindLogicalNode(xamlContent, "textGlobe4") as TextBox;
                    // See how much less code for loops have!? The following links up to, and assigns events to, the 100 or so slider bars and textboxes for AOE config stuff!
                    for (int i = 0; i <= 4; i++)
                    {
                        for (int n = 0; n <= 12; n++)
                        {
                            string sThisNode;
                            sThisNode = "slideRadius" + i.ToString() + "_" + (n + 1).ToString();
                            slideAOERadius[i, n] = LogicalTreeHelper.FindLogicalNode(xamlContent, sThisNode) as Slider;
                            sThisNode = "slideHealth" + i.ToString() + "_" + (n + 1).ToString();
                            slideAOEHealth[i, n] = LogicalTreeHelper.FindLogicalNode(xamlContent, sThisNode) as Slider;
                            sThisNode = "textRadius" + i.ToString() + "_" + (n + 1).ToString();
                            textAOERadius[i, n] = LogicalTreeHelper.FindLogicalNode(xamlContent, sThisNode) as TextBox;
                            sThisNode = "textHealth" + i.ToString() + "_" + (n + 1).ToString();
                            textAOEHealth[i, n] = LogicalTreeHelper.FindLogicalNode(xamlContent, sThisNode) as TextBox;
                            slideAOERadius[i, n].ValueChanged += trackAOERadius_Scroll;
                            slideAOEHealth[i, n].ValueChanged += trackAOEHealth_Scroll;
                            slideAOERadius[i, n].SmallChange = 1;
                            slideAOERadius[i, n].LargeChange = 1;
                            slideAOERadius[i, n].TickFrequency = 1;
                            slideAOERadius[i, n].IsSnapToTickEnabled = true;
                            slideAOEHealth[i, n].SmallChange = 1;
                            slideAOEHealth[i, n].LargeChange = 1;
                            slideAOEHealth[i, n].TickFrequency = 1;
                            slideAOEHealth[i, n].IsSnapToTickEnabled = true;
                        }
                    }
                    // Finally the "defaults" button, and the save config button
                    defaultButton = LogicalTreeHelper.FindLogicalNode(xamlContent, "buttonDefaults") as Button;
                    defaultButton.Click += buttonDefaults_Click;
                    resetCombat = LogicalTreeHelper.FindLogicalNode(xamlContent, "ResetCombat") as Button;
                    resetCombat.Click += resetCombat_Click;
                    resetAOE0 = LogicalTreeHelper.FindLogicalNode(xamlContent, "ResetAOE0") as Button;
                    resetAOE0.Click += resetAOE0_Click;
                    resetAOE1 = LogicalTreeHelper.FindLogicalNode(xamlContent, "ResetAOE1") as Button;
                    resetAOE1.Click += resetAOE1_Click;
                    resetAOE2 = LogicalTreeHelper.FindLogicalNode(xamlContent, "ResetAOE2") as Button;
                    resetAOE2.Click += resetAOE2_Click;
                    resetAOE3 = LogicalTreeHelper.FindLogicalNode(xamlContent, "ResetAOE3") as Button;
                    resetAOE3.Click += resetAOE3_Click;
                    resetAOE4 = LogicalTreeHelper.FindLogicalNode(xamlContent, "ResetAOE4") as Button;
                    resetAOE4.Click += resetAOE4_Click;
                    resetWorld = LogicalTreeHelper.FindLogicalNode(xamlContent, "ResetWorld") as Button;
                    resetWorld.Click += resetWorld_Click;
                    resetItems = LogicalTreeHelper.FindLogicalNode(xamlContent, "ResetItems") as Button;
                    resetItems.Click += resetItems_Click;
                    resetTown = LogicalTreeHelper.FindLogicalNode(xamlContent, "ResetTown") as Button;
                    resetTown.Click += resetTown_Click;
                    resetAdvanced = LogicalTreeHelper.FindLogicalNode(xamlContent, "ResetAdvanced") as Button;
                    resetAdvanced.Click += resetAdvanced_Click;
                    resetMobile = LogicalTreeHelper.FindLogicalNode(xamlContent, "ResetMobile") as Button;
                    resetMobile.Click += resetMobile_Click;
                    saveButton = LogicalTreeHelper.FindLogicalNode(xamlContent, "buttonSave") as Button;
                    saveButton.Click += buttonSave_Click;
                    UserControl mainControl = LogicalTreeHelper.FindLogicalNode(xamlContent, "mainControl") as UserControl;
                    // Set height and width and window title of main window
                    configWindow.Height = mainControl.Height + 30;
                    configWindow.Width = mainControl.Width;
                    configWindow.Title = "Giles Trinity";
                    // Event handling for the config window loading up/closing
                    configWindow.Loaded += configWindow_Loaded;
                    configWindow.Closed += configWindow_Closed;
                    // And finally put all of this content in effect
                    configWindow.Content = xamlContent;
                }
                catch (XamlParseException ex)
                {
                    // Log specific XAML exceptions that might have happened above
                    Log(ex.ToString());
                }
                catch (Exception ex)
                {
                    // Log any other issues
                    Log(ex.ToString());
                }
                return configWindow;
            }
        }
        // The below are all event handlers for all the window-elements within the config window
        // WARNING: If you use code to alter the value of something that has an event attached...
        // For example a slider - then your code automatically also fires the event for that slider
        // I use "suppresseventchanges" to make sure that event code ONLY gets called from the USER changing values
        // And NOT from my own code trying to change values
        private static bool bSuppressEventChanges = false;
        // And now the start of all the events, starting with the largest ones - the AOE stuff - which has to figure out WHICH slider you changed
        // And then change the appropriate variable(s)
        private void trackAOEHealth_Scroll(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (bSuppressEventChanges)
                return;
            Slider thisslider = sender as Slider;
            string sSliderName = thisslider.Name.Substring(11);
            int iClass = Convert.ToInt32(sSliderName.Substring(0, 1));
            int iAvoid = Convert.ToInt32(sSliderName.Substring(2));
            double dThisHealthLimit = (Math.Round(thisslider.Value) / 100);
            textAOEHealth[iClass, iAvoid - 1].Text = (dThisHealthLimit * 100).ToString();
            switch (iClass)
            {
                case 0:
                    // Barbs
                    switch (iAvoid)
                    {
                        case 1:
                            dictAvoidanceHealthBarb[219702] = dThisHealthLimit;
                            dictAvoidanceHealthBarb[221225] = dThisHealthLimit;
                            break;
                        case 2:
                            dictAvoidanceHealthBarb[84608] = dThisHealthLimit;
                            break;
                        case 3:
                            dictAvoidanceHealthBarb[4803] = dThisHealthLimit;
                            dictAvoidanceHealthBarb[4804] = dThisHealthLimit;
                            dictAvoidanceHealthBarb[224225] = dThisHealthLimit;
                            dictAvoidanceHealthBarb[247987] = dThisHealthLimit;
                            break;
                        case 4:
                            dictAvoidanceHealthBarb[95868] = dThisHealthLimit;
                            break;
                        case 5:
                            dictAvoidanceHealthBarb[5482] = dThisHealthLimit;
                            dictAvoidanceHealthBarb[6578] = dThisHealthLimit;
                            break;
                        case 6:
                            dictAvoidanceHealthBarb[108869] = dThisHealthLimit;
                            break;
                        case 7:
                            dictAvoidanceHealthBarb[402] = dThisHealthLimit;
                            dictAvoidanceHealthBarb[223675] = dThisHealthLimit;
                            break;
                        case 8:
                            dictAvoidanceHealthBarb[3865] = dThisHealthLimit;
                            break;
                        case 9:
                            dictAvoidanceHealthBarb[5212] = dThisHealthLimit;
                            break;
                        case 10:
                            dictAvoidanceHealthBarb[123124] = dThisHealthLimit;
                            break;
                        case 11:
                            dictAvoidanceHealthBarb[123839] = dThisHealthLimit;
                            break;
                        case 12:
                            dictAvoidanceHealthBarb[4103] = dThisHealthLimit;
                            break;
                        case 13:
                            dictAvoidanceHealthBarb[93837] = dThisHealthLimit;
                            break;
                    }
                    break;
                case 1:
                    // Monks
                    switch (iAvoid)
                    {
                        case 1:
                            dictAvoidanceHealthMonk[219702] = dThisHealthLimit;
                            dictAvoidanceHealthMonk[221225] = dThisHealthLimit;
                            break;
                        case 2:
                            dictAvoidanceHealthMonk[84608] = dThisHealthLimit;
                            break;
                        case 3:
                            dictAvoidanceHealthMonk[4803] = dThisHealthLimit;
                            dictAvoidanceHealthMonk[4804] = dThisHealthLimit;
                            dictAvoidanceHealthMonk[224225] = dThisHealthLimit;
                            dictAvoidanceHealthMonk[247987] = dThisHealthLimit;
                            break;
                        case 4:
                            dictAvoidanceHealthMonk[95868] = dThisHealthLimit;
                            break;
                        case 5:
                            dictAvoidanceHealthMonk[5482] = dThisHealthLimit;
                            dictAvoidanceHealthMonk[6578] = dThisHealthLimit;
                            break;
                        case 6:
                            dictAvoidanceHealthMonk[108869] = dThisHealthLimit;
                            break;
                        case 7:
                            dictAvoidanceHealthMonk[402] = dThisHealthLimit;
                            dictAvoidanceHealthMonk[223675] = dThisHealthLimit;
                            break;
                        case 8:
                            dictAvoidanceHealthMonk[3865] = dThisHealthLimit;
                            break;
                        case 9:
                            dictAvoidanceHealthMonk[5212] = dThisHealthLimit;
                            break;
                        case 10:
                            dictAvoidanceHealthMonk[123124] = dThisHealthLimit;
                            break;
                        case 11:
                            dictAvoidanceHealthMonk[123839] = dThisHealthLimit;
                            break;
                        case 12:
                            dictAvoidanceHealthMonk[4103] = dThisHealthLimit;
                            break;
                        case 13:
                            dictAvoidanceHealthMonk[93837] = dThisHealthLimit;
                            break;
                    }
                    break;
                case 2:
                    // Wizards
                    switch (iAvoid)
                    {
                        case 1:
                            dictAvoidanceHealthWizard[219702] = dThisHealthLimit;
                            dictAvoidanceHealthWizard[221225] = dThisHealthLimit;
                            break;
                        case 2:
                            dictAvoidanceHealthWizard[84608] = dThisHealthLimit;
                            break;
                        case 3:
                            dictAvoidanceHealthWizard[4803] = dThisHealthLimit;
                            dictAvoidanceHealthWizard[4804] = dThisHealthLimit;
                            dictAvoidanceHealthWizard[224225] = dThisHealthLimit;
                            dictAvoidanceHealthWizard[247987] = dThisHealthLimit;
                            break;
                        case 4:
                            dictAvoidanceHealthWizard[95868] = dThisHealthLimit;
                            break;
                        case 5:
                            dictAvoidanceHealthWizard[5482] = dThisHealthLimit;
                            dictAvoidanceHealthWizard[6578] = dThisHealthLimit;
                            break;
                        case 6:
                            dictAvoidanceHealthWizard[108869] = dThisHealthLimit;
                            break;
                        case 7:
                            dictAvoidanceHealthWizard[402] = dThisHealthLimit;
                            dictAvoidanceHealthWizard[223675] = dThisHealthLimit;
                            break;
                        case 8:
                            dictAvoidanceHealthWizard[3865] = dThisHealthLimit;
                            break;
                        case 9:
                            dictAvoidanceHealthWizard[5212] = dThisHealthLimit;
                            break;
                        case 10:
                            dictAvoidanceHealthWizard[123124] = dThisHealthLimit;
                            break;
                        case 11:
                            dictAvoidanceHealthWizard[123839] = dThisHealthLimit;
                            break;
                        case 12:
                            dictAvoidanceHealthWizard[4103] = dThisHealthLimit;
                            break;
                        case 13:
                            dictAvoidanceHealthWizard[93837] = dThisHealthLimit;
                            break;
                    }
                    break;
                case 3:
                    // WD's
                    switch (iAvoid)
                    {
                        case 1:
                            dictAvoidanceHealthWitch[219702] = dThisHealthLimit;
                            dictAvoidanceHealthWitch[221225] = dThisHealthLimit;
                            break;
                        case 2:
                            dictAvoidanceHealthWitch[84608] = dThisHealthLimit;
                            break;
                        case 3:
                            dictAvoidanceHealthWitch[4803] = dThisHealthLimit;
                            dictAvoidanceHealthWitch[4804] = dThisHealthLimit;
                            dictAvoidanceHealthWitch[224225] = dThisHealthLimit;
                            dictAvoidanceHealthWitch[247987] = dThisHealthLimit;
                            break;
                        case 4:
                            dictAvoidanceHealthWitch[95868] = dThisHealthLimit;
                            break;
                        case 5:
                            dictAvoidanceHealthWitch[5482] = dThisHealthLimit;
                            dictAvoidanceHealthWitch[6578] = dThisHealthLimit;
                            break;
                        case 6:
                            dictAvoidanceHealthWitch[108869] = dThisHealthLimit;
                            break;
                        case 7:
                            dictAvoidanceHealthWitch[402] = dThisHealthLimit;
                            dictAvoidanceHealthWitch[223675] = dThisHealthLimit;
                            break;
                        case 8:
                            dictAvoidanceHealthWitch[3865] = dThisHealthLimit;
                            break;
                        case 9:
                            dictAvoidanceHealthWitch[5212] = dThisHealthLimit;
                            break;
                        case 10:
                            dictAvoidanceHealthWitch[123124] = dThisHealthLimit;
                            break;
                        case 11:
                            dictAvoidanceHealthWitch[123839] = dThisHealthLimit;
                            break;
                        case 12:
                            dictAvoidanceHealthWitch[4103] = dThisHealthLimit;
                            break;
                        case 13:
                            dictAvoidanceHealthWitch[93837] = dThisHealthLimit;
                            break;
                    }
                    break;
                case 4:
                    // DH's
                    switch (iAvoid)
                    {
                        case 1:
                            dictAvoidanceHealthDemon[219702] = dThisHealthLimit;
                            dictAvoidanceHealthDemon[221225] = dThisHealthLimit;
                            break;
                        case 2:
                            dictAvoidanceHealthDemon[84608] = dThisHealthLimit;
                            break;
                        case 3:
                            dictAvoidanceHealthDemon[4803] = dThisHealthLimit;
                            dictAvoidanceHealthDemon[4804] = dThisHealthLimit;
                            dictAvoidanceHealthDemon[224225] = dThisHealthLimit;
                            dictAvoidanceHealthDemon[247987] = dThisHealthLimit;
                            break;
                        case 4:
                            dictAvoidanceHealthDemon[95868] = dThisHealthLimit;
                            break;
                        case 5:
                            dictAvoidanceHealthDemon[5482] = dThisHealthLimit;
                            dictAvoidanceHealthDemon[6578] = dThisHealthLimit;
                            break;
                        case 6:
                            dictAvoidanceHealthDemon[108869] = dThisHealthLimit;
                            break;
                        case 7:
                            dictAvoidanceHealthDemon[402] = dThisHealthLimit;
                            dictAvoidanceHealthDemon[223675] = dThisHealthLimit;
                            break;
                        case 8:
                            dictAvoidanceHealthDemon[3865] = dThisHealthLimit;
                            break;
                        case 9:
                            dictAvoidanceHealthDemon[5212] = dThisHealthLimit;
                            break;
                        case 10:
                            dictAvoidanceHealthDemon[123124] = dThisHealthLimit;
                            break;
                        case 11:
                            dictAvoidanceHealthDemon[123839] = dThisHealthLimit;
                            break;
                        case 12:
                            dictAvoidanceHealthDemon[4103] = dThisHealthLimit;
                            break;
                        case 13:
                            dictAvoidanceHealthDemon[93837] = dThisHealthLimit;
                            break;
                    }
                    break;
            }
            bMappedPlayerAbilities = false;
        }
        private void trackAOERadius_Scroll(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (bSuppressEventChanges)
                return;
            Slider thisslider = sender as Slider;
            string sSliderName = thisslider.Name.Substring(11);
            int iClass = Convert.ToInt32(sSliderName.Substring(0, 1));
            int iAvoid = Convert.ToInt32(sSliderName.Substring(2));
            int iThisAvoidRadius = (int)Math.Round(thisslider.Value);
            textAOERadius[iClass, iAvoid - 1].Text = iThisAvoidRadius.ToString();
            switch (iAvoid)
            {
                case 1:
                    dictAvoidanceRadius[219702] = iThisAvoidRadius;
                    dictAvoidanceRadius[221225] = iThisAvoidRadius;
                    break;
                case 2:
                    dictAvoidanceRadius[84608] = iThisAvoidRadius;
                    break;
                case 3:
                    dictAvoidanceRadius[4803] = iThisAvoidRadius;
                    dictAvoidanceRadius[4804] = iThisAvoidRadius;
                    dictAvoidanceRadius[224225] = iThisAvoidRadius;
                    dictAvoidanceRadius[247987] = iThisAvoidRadius;
                    break;
                case 4:
                    dictAvoidanceRadius[95868] = iThisAvoidRadius;
                    break;
                case 5:
                    dictAvoidanceRadius[5482] = iThisAvoidRadius;
                    dictAvoidanceRadius[6578] = iThisAvoidRadius;
                    break;
                case 6:
                    dictAvoidanceRadius[108869] = iThisAvoidRadius;
                    break;
                case 7:
                    dictAvoidanceRadius[402] = iThisAvoidRadius;
                    dictAvoidanceRadius[223675] = iThisAvoidRadius;
                    break;
                case 8:
                    dictAvoidanceRadius[3865] = iThisAvoidRadius;
                    break;
                case 9:
                    dictAvoidanceRadius[5212] = iThisAvoidRadius;
                    break;
                case 10:
                    dictAvoidanceRadius[123124] = iThisAvoidRadius;
                    break;
                case 11:
                    dictAvoidanceRadius[123839] = iThisAvoidRadius;
                    break;
                case 12:
                    dictAvoidanceRadius[4103] = iThisAvoidRadius;
                    break;
                case 13:
                    dictAvoidanceRadius[93837] = iThisAvoidRadius;
                    break;
            }
            bool bOldSuppress = bSuppressEventChanges;
            bSuppressEventChanges = true;
            for (int i = 0; i <= 4; i++)
            {
                if (i != iClass)
                {
                    slideAOERadius[i, iAvoid - 1].Value = iThisAvoidRadius;
                    textAOERadius[i, iAvoid - 1].Text = iThisAvoidRadius.ToString();
                }
            }
            bSuppressEventChanges = bOldSuppress;
            bMappedPlayerAbilities = false;
        }
        private void checkIgnoreCorpses_check(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            settings.bIgnoreCorpses = true;
        }
        private void checkIgnoreCorpses_uncheck(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            settings.bIgnoreCorpses = false;
        }
        private void slidePot0_Scroll(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (bSuppressEventChanges)
                return;
            slidePot0.Value = Math.Round(slidePot0.Value);
            textPot0.Text = slidePot0.Value.ToString();
            settings.dEmergencyHealthPotionBarb = (Math.Round(slidePot0.Value) / 100);
        }
        private void slidePot1_Scroll(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (bSuppressEventChanges)
                return;
            slidePot1.Value = Math.Round(slidePot1.Value);
            textPot1.Text = slidePot1.Value.ToString();
            settings.dEmergencyHealthPotionMonk = (Math.Round(slidePot1.Value) / 100);
        }
        private void slidePot2_Scroll(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (bSuppressEventChanges)
                return;
            slidePot2.Value = Math.Round(slidePot2.Value);
            textPot2.Text = slidePot2.Value.ToString();
            settings.dEmergencyHealthPotionWiz = (Math.Round(slidePot2.Value) / 100);
        }
        private void slidePot3_Scroll(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (bSuppressEventChanges)
                return;
            slidePot3.Value = Math.Round(slidePot3.Value);
            textPot3.Text = slidePot3.Value.ToString();
            settings.dEmergencyHealthPotionWitch = (Math.Round(slidePot3.Value) / 100);
        }
        private void slidePot4_Scroll(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (bSuppressEventChanges)
                return;
            slidePot4.Value = Math.Round(slidePot4.Value);
            textPot4.Text = slidePot4.Value.ToString();
            settings.dEmergencyHealthPotionDemon = (Math.Round(slidePot4.Value) / 100);
        }
        private void slideGlobe0_Scroll(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (bSuppressEventChanges)
                return;
            slideGlobe0.Value = Math.Round(slideGlobe0.Value);
            textGlobe0.Text = slideGlobe0.Value.ToString();
            settings.dEmergencyHealthGlobeBarb = (Math.Round(slideGlobe0.Value) / 100);
        }
        private void slideGlobe1_Scroll(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (bSuppressEventChanges)
                return;
            slideGlobe1.Value = Math.Round(slideGlobe1.Value);
            textGlobe1.Text = slideGlobe1.Value.ToString();
            settings.dEmergencyHealthGlobeMonk = (Math.Round(slideGlobe1.Value) / 100);
        }
        private void slideGlobe2_Scroll(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (bSuppressEventChanges)
                return;
            slideGlobe2.Value = Math.Round(slideGlobe2.Value);
            textGlobe2.Text = slideGlobe2.Value.ToString();
            settings.dEmergencyHealthGlobeWiz = (Math.Round(slideGlobe2.Value) / 100);
        }
        private void slideGlobe3_Scroll(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (bSuppressEventChanges)
                return;
            slideGlobe3.Value = Math.Round(slideGlobe3.Value);
            textGlobe3.Text = slideGlobe3.Value.ToString();
            settings.dEmergencyHealthGlobeWitch = (Math.Round(slideGlobe3.Value) / 100);
        }
        private void slideGlobe4_Scroll(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (bSuppressEventChanges)
                return;
            slideGlobe4.Value = Math.Round(slideGlobe4.Value);
            textGlobe4.Text = slideGlobe4.Value.ToString();
            settings.dEmergencyHealthGlobeDemon = (Math.Round(slideGlobe4.Value) / 100);
        }
        private void trackContainerRange_Scroll(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (bSuppressEventChanges)
                return;
            slideContainerRange.Value = Math.Round(slideContainerRange.Value);
            textContainerRange.Text = slideContainerRange.Value.ToString();
            settings.iContainerOpenRange = Math.Round(slideContainerRange.Value);
        }
        private void trackDestructibleRange_Scroll(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (bSuppressEventChanges)
                return;
            slideDestructibleRange.Value = Math.Round(slideDestructibleRange.Value);
            textDestructibleRange.Text = slideDestructibleRange.Value.ToString();
            settings.iDestructibleAttackRange = Math.Round(slideDestructibleRange.Value);
        }
        private void checkIgnoreAll_check(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            settings.bIgnoreAllShrines = true;
        }
        private void checkIgnoreNone_check(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            settings.bIgnoreAllShrines = false;
        }
        private void trackGoldAmount_Scroll(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (bSuppressEventChanges)
                return;
            slideGoldAmount.Value = Math.Round(slideGoldAmount.Value);
            textGoldAmount.Text = slideGoldAmount.Value.ToString();
            settings.iMinimumGoldStack = Convert.ToInt32(Math.Round(slideGoldAmount.Value));
        }
        private void comboWB_changed(object sender, SelectionChangedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            settings.iFilterBlueWeapons = Convert.ToInt32(comboWB.SelectedValue);
        }
        private void comboLegendary_changed(object sender, SelectionChangedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            settings.iFilterLegendary = Convert.ToInt32(comboLegendary.SelectedValue);
        }
        private void comboWY_changed(object sender, SelectionChangedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            settings.iFilterYellowWeapons = Convert.ToInt32(comboWY.SelectedValue);
        }
        private void comboAB_changed(object sender, SelectionChangedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            settings.iFilterBlueArmor = Convert.ToInt32(comboAB.SelectedValue);
        }
        private void comboAY_changed(object sender, SelectionChangedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            settings.iFilterYellowArmor = Convert.ToInt32(comboAY.SelectedValue);
        }
        private void comboJB_changed(object sender, SelectionChangedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            settings.iFilterBlueJewelry = Convert.ToInt32(comboJB.SelectedValue);
        }
        private void comboJY_changed(object sender, SelectionChangedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            settings.iFilterYellowJewelry = Convert.ToInt32(comboJY.SelectedValue);
        }
        private void comboGems_changed(object sender, SelectionChangedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            settings.iFilterGems = Convert.ToInt32(comboGems.SelectedValue);
        }
        private void comboMisc_changed(object sender, SelectionChangedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            settings.iFilterMisc = Convert.ToInt32(comboMisc.SelectedValue);
        }
        private void comboPotions_changed(object sender, SelectionChangedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            settings.iFilterPotions = Convert.ToInt32(comboPotions.SelectedValue);
        }
        private void comboPotionLevel_changed(object sender, SelectionChangedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            settings.iFilterPotionLevel = Convert.ToInt32(comboPotionLevel.SelectedValue);
        }
        private void checkFollower_check(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            settings.bPickupFollower = true;
        }
        private void checkFollower_uncheck(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            settings.bPickupFollower = false;
        }
        private void checkDesigns_check(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            settings.bPickupPlans = true;
        }
        private void checkDesigns_uncheck(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            settings.bPickupPlans = false;
        }
        private void checkCraftTomes_check(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            settings.bPickupCraftTomes = true;
        }
        private void checkCraftTomes_uncheck(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            settings.bPickupCraftTomes = false;
        }
        private void btnRulesGiles_check(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            settings.bUseGilesFilters = true;
        }
        private void btnRulesCustom_check(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            settings.bUseGilesFilters = false;
        }
        private void btnSalvage_check(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            settings.bSalvageJunk = true;
        }
        private void btnSell_check(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            settings.bSalvageJunk = false;
        }
        private void checkEmerald_check(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            settings.bGemsEmerald = true;
        }
        private void checkEmerald_uncheck(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            settings.bGemsEmerald = false;
        }
        private void checkAmethyst_check(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            settings.bGemsAmethyst = true;
        }
        private void checkAmethyst_uncheck(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            settings.bGemsAmethyst = false;
        }
        private void checkTopaz_check(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            settings.bGemsTopaz = true;
        }
        private void checkTopaz_uncheck(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            settings.bGemsTopaz = false;
        }
        private void checkRuby_check(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            settings.bGemsRuby = true;
        }
        private void checkRuby_uncheck(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            settings.bGemsRuby = false;
        }
        private void checkAndroid_check(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            settings.bEnableAndroid = true;
        }
        private void checkAndroid_uncheck(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            settings.bEnableAndroid = false;
        }
        private void textAndroid_change(object sender, TextChangedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            sAndroidAPIKey = textAndroidKey.Text;
        }
        private void checkProwl_check(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            settings.bEnableProwl = true;
        }
        private void checkProwl_uncheck(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            settings.bEnableProwl = false;
        }
        private void textProwl_change(object sender, TextChangedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            sProwlAPIKey = textProwlKey.Text;
        }
        private void checkEmail_check(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            settings.bEnableEmail = true;
        }
        private void checkEmail_uncheck(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            settings.bEnableEmail = false;
        }
        private void checkLegendaryNotifyScore_check(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            settings.bEnableLegendaryNotifyScore = true;
        }
        private void checkLegendaryNotifyScore_uncheck(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            settings.bEnableLegendaryNotifyScore = false;
        }
        private void txtEmailAddress_change(object sender, TextChangedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            sEmailAddress = txtEmailAddress.Text;
        }
        private void txtEmailPassword_change(object sender, TextChangedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            sEmailPassword = txtEmailPassword.Text;
        }
        private void txtBotName_change(object sender, TextChangedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            sBotName = txtBotName.Text;
        }
        private void trackScoreWeapons_Scroll(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (bSuppressEventChanges)
                return;
            slideWeapon.Value = Math.Round(slideWeapon.Value);
            settings.iNeedPointsToKeepWeapon = slideWeapon.Value;
            WeaponText.Text = slideWeapon.Value.ToString();
        }
        private void trackScoreArmor_Scroll(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (bSuppressEventChanges)
                return;
            slideArmor.Value = Math.Round(slideArmor.Value);
            settings.iNeedPointsToKeepArmor = slideArmor.Value;
            ArmorText.Text = slideArmor.Value.ToString();
        }
        private void trackScoreJewelry_Scroll(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (bSuppressEventChanges)
                return;
            slideJewelry.Value = Math.Round(slideJewelry.Value);
            settings.iNeedPointsToKeepJewelry = slideJewelry.Value;
            JewelryText.Text = slideJewelry.Value.ToString();
        }
        private void trackNotifyWeapons_Scroll(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (bSuppressEventChanges)
                return;
            slideNotifyWeapon.Value = Math.Round(slideNotifyWeapon.Value);
            settings.iNeedPointsToNotifyWeapon = slideNotifyWeapon.Value;
            WeaponNotifyText.Text = slideNotifyWeapon.Value.ToString();
        }
        private void trackNotifyArmor_Scroll(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (bSuppressEventChanges)
                return;
            slideNotifyArmor.Value = Math.Round(slideNotifyArmor.Value);
            settings.iNeedPointsToNotifyArmor = slideNotifyArmor.Value;
            ArmorNotifyText.Text = slideNotifyArmor.Value.ToString();
        }
        private void trackNotifyJewelry_Scroll(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (bSuppressEventChanges)
                return;
            slideNotifyJewelry.Value = Math.Round(slideNotifyJewelry.Value);
            settings.iNeedPointsToNotifyJewelry = slideNotifyJewelry.Value;
            JewelryNotifyText.Text = slideNotifyJewelry.Value.ToString();
        }
        private void checkBacktracking_check(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            settings.bEnableBacktracking = true;
        }
        private void checkBacktracking_uncheck(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            settings.bEnableBacktracking = false;
        }
        private void checkDebugInfo_check(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            settings.bDebugInfo = true;
        }
        private void checkDebugInfo_uncheck(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            settings.bDebugInfo = false;
        }
        private void checkTPS_check(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            settings.bEnableTPS = true;
            BotMain.TicksPerSecond = (int)settings.iTPSAmount;
        }
        private void checkTPS_uncheck(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            settings.bEnableTPS = false;
            BotMain.TicksPerSecond = 10;
        }
        private void checkProfileReload_check(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            settings.bEnableProfileReloading = true;
        }
        private void checkProfileReload_uncheck(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            settings.bEnableProfileReloading = false;
        }
        private void checkUnstucker_check(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            settings.bEnableUnstucker = true;
            Navigator.StuckHandler = new GilesStuckHandler();
        }
        private void checkUnstucker_uncheck(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            settings.bEnableUnstucker = false;
            Navigator.StuckHandler = new DefaultStuckHandler();
        }
        private void checkLogStucks_check(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            settings.bLogStucks = true;
        }
        private void checkLogStucks_uncheck(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            settings.bLogStucks = false;
        }
        private void checkExtendedRange_check(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            settings.bExtendedKillRange = true;
        }
        private void checkExtendedRange_uncheck(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            settings.bExtendedKillRange = false;
        }
        private void checkWrath90_check(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            settings.bWrath90Seconds = true;
        }
        private void checkWrath90_uncheck(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            settings.bWrath90Seconds = false;
        }
        private void checkSelectiveWW_check(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            settings.bSelectiveWhirlwind = true;
        }
        private void checkSelectiveWW_uncheck(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            settings.bSelectiveWhirlwind = false;
        }
        private void checkKiteArchonOnly_check(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            settings.bKiteOnlyArchon = true;
        }
        private void checkKiteArchonOnly_uncheck(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            settings.bKiteOnlyArchon = false;
        }
        private void checkWaitArchonAzmo_check(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            settings.bWaitForArchon = true;
        }
        private void checkWaitArchonAzmo_uncheck(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            settings.bWaitForArchon = false;
        }
        private void checkWaitWrath_check(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            settings.bWaitForWrath = true;
        }
        private void checkWaitWrath_uncheck(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            settings.bWaitForWrath = false;
        }
        private void checkGoblinWrath_check(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            settings.bGoblinWrath = true;
        }
        private void checkGoblinWrath_uncheck(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            settings.bGoblinWrath = false;
        }
        private void checkMonkInna_check(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            settings.bMonkInnaSet = true;
        }
        private void checkMonkInna_uncheck(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            settings.bMonkInnaSet = false;
        }
        private void checkFuryDumpWrath_check(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            settings.bFuryDumpWrath = true;
        }
        private void checkFuryDumpWrath_uncheck(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            settings.bFuryDumpWrath = false;
        }
        private void checkFuryDumpAlways_check(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            settings.bFuryDumpAlways = true;
        }
        private void checkFuryDumpAlways_uncheck(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            settings.bFuryDumpAlways = false;
        }
        private void trackTPS_Scroll(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (bSuppressEventChanges)
                return;
            slideTPS.Value = Math.Round(slideTPS.Value);
            textTPS.Text = slideTPS.Value.ToString();
            settings.iTPSAmount = Math.Round(slideTPS.Value);
            if (settings.bEnableTPS)
            {
                BotMain.TicksPerSecond = (int)settings.iTPSAmount;
            }
        }
        private void checkAvoidance_check(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            settings.bEnableAvoidance = true;
        }
        private void checkAvoidance_uncheck(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            settings.bEnableAvoidance = false;
        }
        private void checkGlobes_check(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            settings.bEnableGlobes = true;
        }
        private void checkGlobes_uncheck(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            settings.bEnableGlobes = false;
        }
        private void checkCritical_check(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            settings.bEnableCriticalMass = true;
        }
        private void checkCritical_uncheck(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            settings.bEnableCriticalMass = false;
        }
        private void checkMovementAbilities_check(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            settings.bOutOfCombatMovementPowers = true;
        }
        private void checkMovementAbilities_uncheck(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            settings.bOutOfCombatMovementPowers = false;
        }
        private void slideKite0_Scroll(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (bSuppressEventChanges)
                return;
            slideKite0.Value = Math.Round(slideKite0.Value);
            textKite0.Text = slideKite0.Value.ToString();
            settings.iKiteDistanceBarb = (int)Math.Round(slideKite0.Value);
        }
        private void slideKite2_Scroll(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (bSuppressEventChanges)
                return;
            slideKite2.Value = Math.Round(slideKite2.Value);
            textKite2.Text = slideKite2.Value.ToString();
            settings.iKiteDistanceWiz = (int)Math.Round(slideKite2.Value);
        }
        private void slideKite3_Scroll(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (bSuppressEventChanges)
                return;
            slideKite3.Value = Math.Round(slideKite3.Value);
            textKite3.Text = slideKite3.Value.ToString();
            settings.iKiteDistanceWitch = (int)Math.Round(slideKite3.Value);
        }
        private void slideKite4_Scroll(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (bSuppressEventChanges)
                return;
            slideKite4.Value = Math.Round(slideKite4.Value);
            textKite4.Text = slideKite4.Value.ToString();
            settings.iKiteDistanceDemon = (int)Math.Round(slideKite4.Value);
        }
        private void slideVaultDelay_Scroll(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (bSuppressEventChanges)
                return;
            slideVaultDelay.Value = Math.Round(slideVaultDelay.Value);
            textVaultDelay.Text = slideVaultDelay.Value.ToString();
            settings.iDHVaultMovementDelay = (int)Math.Round(slideVaultDelay.Value);
        }
        private void slideLootDelay_Scroll(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (bSuppressEventChanges)
                return;
            slideLootDelay.Value = Math.Round(slideLootDelay.Value);
            textLootDelay.Text = slideLootDelay.Value.ToString();
            settings.iKillLootDelay = (int)Math.Round(slideLootDelay.Value);
        }
        private void trackTriggerRange_Scroll(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (bSuppressEventChanges)
                return;
            slideTriggerRange.Value = Math.Round(slideTriggerRange.Value);
            textTriggerRange.Text = slideTriggerRange.Value.ToString();
            settings.iMonsterKillRange = Math.Round(slideTriggerRange.Value);
        }
        // The three events for the treasure goblin priority choice
        private void checkTreasureIgnore_check(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            settings.iTreasureGoblinPriority = 0;
        }
        private void checkTreasureNormal_check(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            settings.iTreasureGoblinPriority = 1;
        }
        private void checkTreasurePrioritize_check(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            settings.iTreasureGoblinPriority = 2;
        }
        private void checkTreasureKamikaze_check(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            settings.iTreasureGoblinPriority = 3;
        }
        // Event handler for the config window being closed
        private void configWindow_Closed(object sender, EventArgs e)
        {
            configWindow = null;
        }
        // Event handler for the config window loading, update all the window elements!
        private void configWindow_Loaded(object sender, RoutedEventArgs e)
        {
            settingsWindowResetValues();
        }
        // Button-clicked for testing backpack stash-replacer scores
        private void buttonTest_Click(object sender, RoutedEventArgs e)
        {
            TestScoring();
            configWindow.Close();
        }
        // Button-clicked for testing backpack stash-replacer scores
        private void buttonSort_Click(object sender, RoutedEventArgs e)
        {
            configWindow.Close();
            SortStash();
        }
        // Button-clicked for saving the config
        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            SaveConfiguration();
            configWindow.Close();
        }
        // Default button clicked
        private void buttonDefaults_Click(object sender, RoutedEventArgs e)
        {
            settings = new GilesSettings();
            dictAvoidanceHealthBarb = new Dictionary<int, double>(dictAvoidanceHealthBarbDefaults);
            dictAvoidanceHealthMonk = new Dictionary<int, double>(dictAvoidanceHealthMonkDefaults);
            dictAvoidanceHealthWizard = new Dictionary<int, double>(dictAvoidanceHealthWizardDefaults);
            dictAvoidanceHealthWitch = new Dictionary<int, double>(dictAvoidanceHealthWitchDefaults);
            dictAvoidanceHealthDemon = new Dictionary<int, double>(dictAvoidanceHealthDemonDefaults);
            dictAvoidanceRadius = new Dictionary<int, double>(dictAvoidanceRadiusDefaults);
            settingsWindowResetValues();
        }
        // Individual reset buttons
        private void resetCombat_Click(object sender, RoutedEventArgs e)
        {
            GilesSettings tempsettings = new GilesSettings();
            settings.bEnableBacktracking = tempsettings.bEnableBacktracking;
            settings.bEnableAvoidance = tempsettings.bEnableAvoidance;
            settings.bEnableGlobes = tempsettings.bEnableGlobes;
            settings.bEnableCriticalMass = tempsettings.bEnableCriticalMass;
            settings.iTreasureGoblinPriority = tempsettings.iTreasureGoblinPriority;
            settings.iMonsterKillRange = tempsettings.iMonsterKillRange;
            settings.iKillLootDelay = tempsettings.iKillLootDelay;
            settings.iDHVaultMovementDelay = tempsettings.iDHVaultMovementDelay;
            settings.bMonkInnaSet = tempsettings.bMonkInnaSet;
            settings.bOutOfCombatMovementPowers = tempsettings.bOutOfCombatMovementPowers;
            settings.bExtendedKillRange = tempsettings.bExtendedKillRange;
            settings.bSelectiveWhirlwind = tempsettings.bSelectiveWhirlwind;
            settings.bWrath90Seconds = tempsettings.bWrath90Seconds;
            settings.bWaitForWrath = tempsettings.bWaitForWrath;
            settings.bGoblinWrath = tempsettings.bGoblinWrath;
            settings.bFuryDumpWrath = tempsettings.bFuryDumpWrath;
            settings.bFuryDumpAlways = tempsettings.bFuryDumpAlways;
            settings.bKiteOnlyArchon = tempsettings.bKiteOnlyArchon;
            settings.bWaitForArchon = tempsettings.bWaitForArchon;
            settingsWindowResetValues();
        }
        private void resetAOE0_Click(object sender, RoutedEventArgs e)
        {
            dictAvoidanceHealthBarb = new Dictionary<int, double>(dictAvoidanceHealthBarbDefaults);
            dictAvoidanceRadius = new Dictionary<int, double>(dictAvoidanceRadiusDefaults);
            GilesSettings tempsettings = new GilesSettings();
            settings.dEmergencyHealthPotionBarb = tempsettings.dEmergencyHealthPotionBarb;
            settings.dEmergencyHealthGlobeBarb = tempsettings.dEmergencyHealthGlobeBarb;
            settings.iKiteDistanceBarb = tempsettings.iKiteDistanceBarb;
            settingsWindowResetValues();
        }
        private void resetAOE1_Click(object sender, RoutedEventArgs e)
        {
            dictAvoidanceHealthMonk = new Dictionary<int, double>(dictAvoidanceHealthMonkDefaults);
            dictAvoidanceRadius = new Dictionary<int, double>(dictAvoidanceRadiusDefaults);
            GilesSettings tempsettings = new GilesSettings();
            settings.dEmergencyHealthPotionMonk = tempsettings.dEmergencyHealthPotionMonk;
            settings.dEmergencyHealthGlobeMonk = tempsettings.dEmergencyHealthGlobeMonk;
            settings.bMonkInnaSet = tempsettings.bMonkInnaSet;
            settingsWindowResetValues();
        }
        private void resetAOE2_Click(object sender, RoutedEventArgs e)
        {
            dictAvoidanceHealthWizard = new Dictionary<int, double>(dictAvoidanceHealthWizardDefaults);
            dictAvoidanceRadius = new Dictionary<int, double>(dictAvoidanceRadiusDefaults);
            GilesSettings tempsettings = new GilesSettings();
            settings.dEmergencyHealthPotionWiz = tempsettings.dEmergencyHealthPotionWiz;
            settings.dEmergencyHealthGlobeWiz = tempsettings.dEmergencyHealthGlobeWiz;
            settings.iKiteDistanceWiz = tempsettings.iKiteDistanceWiz;
            settings.bKiteOnlyArchon = tempsettings.bKiteOnlyArchon;
            settings.bWaitForArchon = tempsettings.bWaitForArchon;
            settingsWindowResetValues();
        }
        private void resetAOE3_Click(object sender, RoutedEventArgs e)
        {
            dictAvoidanceHealthWitch = new Dictionary<int, double>(dictAvoidanceHealthWitchDefaults);
            dictAvoidanceRadius = new Dictionary<int, double>(dictAvoidanceRadiusDefaults);
            GilesSettings tempsettings = new GilesSettings();
            settings.dEmergencyHealthPotionWitch = tempsettings.dEmergencyHealthPotionWitch;
            settings.dEmergencyHealthGlobeWitch = tempsettings.dEmergencyHealthGlobeWitch;
            settings.iKiteDistanceWitch = tempsettings.iKiteDistanceWitch;
            settingsWindowResetValues();
        }
        private void resetAOE4_Click(object sender, RoutedEventArgs e)
        {
            dictAvoidanceHealthDemon = new Dictionary<int, double>(dictAvoidanceHealthDemonDefaults);
            dictAvoidanceRadius = new Dictionary<int, double>(dictAvoidanceRadiusDefaults);
            GilesSettings tempsettings = new GilesSettings();
            settings.dEmergencyHealthPotionDemon = tempsettings.dEmergencyHealthPotionDemon;
            settings.dEmergencyHealthGlobeDemon = tempsettings.dEmergencyHealthGlobeDemon;
            settings.iKiteDistanceDemon = tempsettings.iKiteDistanceDemon;
            settingsWindowResetValues();
        }
        private void resetWorld_Click(object sender, RoutedEventArgs e)
        {
            GilesSettings tempsettings = new GilesSettings();
            settings.bIgnoreAllShrines = tempsettings.bIgnoreAllShrines;
            settings.bIgnoreCorpses = tempsettings.bIgnoreCorpses;
            settings.iContainerOpenRange = tempsettings.iContainerOpenRange;
            settings.iDestructibleAttackRange = tempsettings.iDestructibleAttackRange;
            settingsWindowResetValues();
        }
        private void resetItems_Click(object sender, RoutedEventArgs e)
        {
            GilesSettings tempsettings = new GilesSettings();
            settings.bUseGilesFilters = tempsettings.bUseGilesFilters;
            settings.iMinimumGoldStack = tempsettings.iMinimumGoldStack;
            settings.iFilterPotions = tempsettings.iFilterPotions;
            settings.iFilterLegendary = tempsettings.iFilterLegendary;
            settings.iFilterBlueWeapons = tempsettings.iFilterBlueWeapons;
            settings.iFilterYellowWeapons = tempsettings.iFilterYellowWeapons;
            settings.iFilterBlueArmor = tempsettings.iFilterBlueArmor;
            settings.iFilterYellowArmor = tempsettings.iFilterYellowArmor;
            settings.iFilterBlueJewelry = tempsettings.iFilterBlueJewelry;
            settings.iFilterYellowJewelry = tempsettings.iFilterYellowJewelry;
            settings.iFilterGems = tempsettings.iFilterGems;
            settings.iFilterMisc = tempsettings.iFilterMisc;
            settings.iFilterPotionLevel = tempsettings.iFilterPotionLevel;
            settings.bGemsEmerald = tempsettings.bGemsEmerald;
            settings.bGemsAmethyst = tempsettings.bGemsAmethyst;
            settings.bGemsTopaz = tempsettings.bGemsTopaz;
            settings.bGemsRuby = tempsettings.bGemsRuby;
            settings.bPickupCraftTomes = tempsettings.bPickupCraftTomes;
            settings.bPickupPlans = tempsettings.bPickupPlans;
            settings.bPickupFollower = tempsettings.bPickupFollower;
            settingsWindowResetValues();
        }
        private void resetTown_Click(object sender, RoutedEventArgs e)
        {
            GilesSettings tempsettings = new GilesSettings();
            settings.bSalvageJunk = tempsettings.bSalvageJunk;
            settings.iNeedPointsToKeepJewelry = tempsettings.iNeedPointsToKeepJewelry;
            settings.iNeedPointsToKeepArmor = tempsettings.iNeedPointsToKeepArmor;
            settings.iNeedPointsToKeepWeapon = tempsettings.iNeedPointsToKeepWeapon;
            settingsWindowResetValues();
        }
        private void resetAdvanced_Click(object sender, RoutedEventArgs e)
        {
            GilesSettings tempsettings = new GilesSettings();
            settings.bEnableTPS = tempsettings.bEnableTPS;
            settings.iTPSAmount = tempsettings.iTPSAmount;
            settings.bLogStucks = tempsettings.bLogStucks;
            settings.bEnableUnstucker = tempsettings.bEnableUnstucker;
            settings.bEnableProfileReloading = tempsettings.bEnableProfileReloading;
            settings.bDebugInfo = tempsettings.bDebugInfo;
            settings.bEnableLegendaryNotifyScore = tempsettings.bEnableLegendaryNotifyScore;
            settingsWindowResetValues();
        }
        private void resetMobile_Click(object sender, RoutedEventArgs e)
        {
            GilesSettings tempsettings = new GilesSettings();
            settings.bEnableProwl = tempsettings.bEnableProwl;
            settings.bEnableAndroid = tempsettings.bEnableAndroid;
            settings.iNeedPointsToNotifyJewelry = tempsettings.iNeedPointsToNotifyJewelry;
            settings.iNeedPointsToNotifyArmor = tempsettings.iNeedPointsToNotifyArmor;
            settings.iNeedPointsToNotifyWeapon = tempsettings.iNeedPointsToNotifyWeapon;
            settings.bEnableEmail = tempsettings.bEnableEmail;
            settingsWindowResetValues();
        }
        // This function sets all of the window elements of the config window, to the current actual values held in the variables
        private void settingsWindowResetValues()
        {
            bSuppressEventChanges = true;
            for (int i = 0; i <= 4; i++)
            {
                for (int n = 1; n <= 13; n++)
                {
                    switch (n)
                    {
                        case 1:
                            slideAOERadius[i, n - 1].Value = dictAvoidanceRadius[219702];
                            break;
                        case 2:
                            slideAOERadius[i, n - 1].Value = dictAvoidanceRadius[84608];
                            break;
                        case 3:
                            slideAOERadius[i, n - 1].Value = dictAvoidanceRadius[4804];
                            break;
                        case 4:
                            slideAOERadius[i, n - 1].Value = dictAvoidanceRadius[95868];
                            break;
                        case 5:
                            slideAOERadius[i, n - 1].Value = dictAvoidanceRadius[5482];
                            break;
                        case 6:
                            slideAOERadius[i, n - 1].Value = dictAvoidanceRadius[108869];
                            break;
                        case 7:
                            slideAOERadius[i, n - 1].Value = dictAvoidanceRadius[223675];
                            break;
                        case 8:
                            slideAOERadius[i, n - 1].Value = dictAvoidanceRadius[3865];
                            break;
                        case 9:
                            slideAOERadius[i, n - 1].Value = dictAvoidanceRadius[5212];
                            break;
                        case 10:
                            slideAOERadius[i, n - 1].Value = dictAvoidanceRadius[123124];
                            break;
                        case 11:
                            slideAOERadius[i, n - 1].Value = dictAvoidanceRadius[123839];
                            break;
                        case 12:
                            slideAOERadius[i, n - 1].Value = dictAvoidanceRadius[4103];
                            break;
                        case 13:
                            slideAOERadius[i, n - 1].Value = dictAvoidanceRadius[93837];
                            break;
                    }
                    switch (i)
                    {
                        case 0:
                            // barbs
                            switch (n)
                            {
                                case 1:
                                    slideAOEHealth[i, n - 1].Value = dictAvoidanceHealthBarb[219702] * 100;
                                    break;
                                case 2:
                                    slideAOEHealth[i, n - 1].Value = dictAvoidanceHealthBarb[84608] * 100;
                                    break;
                                case 3:
                                    slideAOEHealth[i, n - 1].Value = dictAvoidanceHealthBarb[4804] * 100;
                                    break;
                                case 4:
                                    slideAOEHealth[i, n - 1].Value = dictAvoidanceHealthBarb[95868] * 100;
                                    break;
                                case 5:
                                    slideAOEHealth[i, n - 1].Value = dictAvoidanceHealthBarb[5482] * 100;
                                    break;
                                case 6:
                                    slideAOEHealth[i, n - 1].Value = dictAvoidanceHealthBarb[108869] * 100;
                                    break;
                                case 7:
                                    slideAOEHealth[i, n - 1].Value = dictAvoidanceHealthBarb[223675] * 100;
                                    break;
                                case 8:
                                    slideAOEHealth[i, n - 1].Value = dictAvoidanceHealthBarb[3865] * 100;
                                    break;
                                case 9:
                                    slideAOEHealth[i, n - 1].Value = dictAvoidanceHealthBarb[5212] * 100;
                                    break;
                                case 10:
                                    slideAOEHealth[i, n - 1].Value = dictAvoidanceHealthBarb[123124] * 100;
                                    break;
                                case 11:
                                    slideAOEHealth[i, n - 1].Value = dictAvoidanceHealthBarb[123839] * 100;
                                    break;
                                case 12:
                                    slideAOEHealth[i, n - 1].Value = dictAvoidanceHealthBarb[4103] * 100;
                                    break;
                                case 13:
                                    slideAOEHealth[i, n - 1].Value = dictAvoidanceHealthBarb[93837] * 100;
                                    break;
                            }
                            break;
                        case 1:
                            // Monks
                            switch (n)
                            {
                                case 1:
                                    slideAOEHealth[i, n - 1].Value = dictAvoidanceHealthMonk[219702] * 100;
                                    break;
                                case 2:
                                    slideAOEHealth[i, n - 1].Value = dictAvoidanceHealthMonk[84608] * 100;
                                    break;
                                case 3:
                                    slideAOEHealth[i, n - 1].Value = dictAvoidanceHealthMonk[4804] * 100;
                                    break;
                                case 4:
                                    slideAOEHealth[i, n - 1].Value = dictAvoidanceHealthMonk[95868] * 100;
                                    break;
                                case 5:
                                    slideAOEHealth[i, n - 1].Value = dictAvoidanceHealthMonk[5482] * 100;
                                    break;
                                case 6:
                                    slideAOEHealth[i, n - 1].Value = dictAvoidanceHealthMonk[108869] * 100;
                                    break;
                                case 7:
                                    slideAOEHealth[i, n - 1].Value = dictAvoidanceHealthMonk[223675] * 100;
                                    break;
                                case 8:
                                    slideAOEHealth[i, n - 1].Value = dictAvoidanceHealthMonk[3865] * 100;
                                    break;
                                case 9:
                                    slideAOEHealth[i, n - 1].Value = dictAvoidanceHealthMonk[5212] * 100;
                                    break;
                                case 10:
                                    slideAOEHealth[i, n - 1].Value = dictAvoidanceHealthMonk[123124] * 100;
                                    break;
                                case 11:
                                    slideAOEHealth[i, n - 1].Value = dictAvoidanceHealthMonk[123839] * 100;
                                    break;
                                case 12:
                                    slideAOEHealth[i, n - 1].Value = dictAvoidanceHealthMonk[4103] * 100;
                                    break;
                                case 13:
                                    slideAOEHealth[i, n - 1].Value = dictAvoidanceHealthMonk[93837] * 100;
                                    break;
                            }
                            break;
                        case 2:
                            // Wizards
                            switch (n)
                            {
                                case 1:
                                    slideAOEHealth[i, n - 1].Value = dictAvoidanceHealthWizard[219702] * 100;
                                    break;
                                case 2:
                                    slideAOEHealth[i, n - 1].Value = dictAvoidanceHealthWizard[84608] * 100;
                                    break;
                                case 3:
                                    slideAOEHealth[i, n - 1].Value = dictAvoidanceHealthWizard[4804] * 100;
                                    break;
                                case 4:
                                    slideAOEHealth[i, n - 1].Value = dictAvoidanceHealthWizard[95868] * 100;
                                    break;
                                case 5:
                                    slideAOEHealth[i, n - 1].Value = dictAvoidanceHealthWizard[5482] * 100;
                                    break;
                                case 6:
                                    slideAOEHealth[i, n - 1].Value = dictAvoidanceHealthWizard[108869] * 100;
                                    break;
                                case 7:
                                    slideAOEHealth[i, n - 1].Value = dictAvoidanceHealthWizard[223675] * 100;
                                    break;
                                case 8:
                                    slideAOEHealth[i, n - 1].Value = dictAvoidanceHealthWizard[3865] * 100;
                                    break;
                                case 9:
                                    slideAOEHealth[i, n - 1].Value = dictAvoidanceHealthWizard[5212] * 100;
                                    break;
                                case 10:
                                    slideAOEHealth[i, n - 1].Value = dictAvoidanceHealthWizard[123124] * 100;
                                    break;
                                case 11:
                                    slideAOEHealth[i, n - 1].Value = dictAvoidanceHealthWizard[123839] * 100;
                                    break;
                                case 12:
                                    slideAOEHealth[i, n - 1].Value = dictAvoidanceHealthWizard[4103] * 100;
                                    break;
                                case 13:
                                    slideAOEHealth[i, n - 1].Value = dictAvoidanceHealthWizard[93837] * 100;
                                    break;
                            }
                            break;
                        case 3:
                            // Witch Doctors
                            switch (n)
                            {
                                case 1:
                                    slideAOEHealth[i, n - 1].Value = dictAvoidanceHealthWitch[219702] * 100;
                                    break;
                                case 2:
                                    slideAOEHealth[i, n - 1].Value = dictAvoidanceHealthWitch[84608] * 100;
                                    break;
                                case 3:
                                    slideAOEHealth[i, n - 1].Value = dictAvoidanceHealthWitch[4804] * 100;
                                    break;
                                case 4:
                                    slideAOEHealth[i, n - 1].Value = dictAvoidanceHealthWitch[95868] * 100;
                                    break;
                                case 5:
                                    slideAOEHealth[i, n - 1].Value = dictAvoidanceHealthWitch[5482] * 100;
                                    break;
                                case 6:
                                    slideAOEHealth[i, n - 1].Value = dictAvoidanceHealthWitch[108869] * 100;
                                    break;
                                case 7:
                                    slideAOEHealth[i, n - 1].Value = dictAvoidanceHealthWitch[223675] * 100;
                                    break;
                                case 8:
                                    slideAOEHealth[i, n - 1].Value = dictAvoidanceHealthWitch[3865] * 100;
                                    break;
                                case 9:
                                    slideAOEHealth[i, n - 1].Value = dictAvoidanceHealthWitch[5212] * 100;
                                    break;
                                case 10:
                                    slideAOEHealth[i, n - 1].Value = dictAvoidanceHealthWitch[123124] * 100;
                                    break;
                                case 11:
                                    slideAOEHealth[i, n - 1].Value = dictAvoidanceHealthWitch[123839] * 100;
                                    break;
                                case 12:
                                    slideAOEHealth[i, n - 1].Value = dictAvoidanceHealthWitch[4103] * 100;
                                    break;
                                case 13:
                                    slideAOEHealth[i, n - 1].Value = dictAvoidanceHealthWitch[93837] * 100;
                                    break;
                            }
                            break;
                        case 4:
                            // Demon Hunters
                            switch (n)
                            {
                                case 1:
                                    slideAOEHealth[i, n - 1].Value = dictAvoidanceHealthDemon[219702] * 100;
                                    break;
                                case 2:
                                    slideAOEHealth[i, n - 1].Value = dictAvoidanceHealthDemon[84608] * 100;
                                    break;
                                case 3:
                                    slideAOEHealth[i, n - 1].Value = dictAvoidanceHealthDemon[4804] * 100;
                                    break;
                                case 4:
                                    slideAOEHealth[i, n - 1].Value = dictAvoidanceHealthDemon[95868] * 100;
                                    break;
                                case 5:
                                    slideAOEHealth[i, n - 1].Value = dictAvoidanceHealthDemon[5482] * 100;
                                    break;
                                case 6:
                                    slideAOEHealth[i, n - 1].Value = dictAvoidanceHealthDemon[108869] * 100;
                                    break;
                                case 7:
                                    slideAOEHealth[i, n - 1].Value = dictAvoidanceHealthDemon[223675] * 100;
                                    break;
                                case 8:
                                    slideAOEHealth[i, n - 1].Value = dictAvoidanceHealthDemon[3865] * 100;
                                    break;
                                case 9:
                                    slideAOEHealth[i, n - 1].Value = dictAvoidanceHealthDemon[5212] * 100;
                                    break;
                                case 10:
                                    slideAOEHealth[i, n - 1].Value = dictAvoidanceHealthDemon[123124] * 100;
                                    break;
                                case 11:
                                    slideAOEHealth[i, n - 1].Value = dictAvoidanceHealthDemon[123839] * 100;
                                    break;
                                case 12:
                                    slideAOEHealth[i, n - 1].Value = dictAvoidanceHealthDemon[4103] * 100;
                                    break;
                                case 13:
                                    slideAOEHealth[i, n - 1].Value = dictAvoidanceHealthDemon[93837] * 100;
                                    break;
                            }
                            break;
                    }
                    // Switch on i
                    textAOERadius[i, n - 1].Text = slideAOERadius[i, n - 1].Value.ToString();
                    textAOEHealth[i, n - 1].Text = slideAOEHealth[i, n - 1].Value.ToString();
                }
                // Loop through the avoidances
            }
            // Loop through the classes
            comboLegendary.SelectedIndex = comboLegendary.Items.Cast<ComboBoxItem>().TakeWhile(cbi => !(cbi.Tag).Equals(settings.iFilterLegendary.ToString())).Count();
            comboWB.SelectedIndex = comboWB.Items.Cast<ComboBoxItem>().TakeWhile(cbi => !(cbi.Tag).Equals(settings.iFilterBlueWeapons.ToString())).Count();
            comboWY.SelectedIndex = comboWY.Items.Cast<ComboBoxItem>().TakeWhile(cbi => !(cbi.Tag).Equals(settings.iFilterYellowWeapons.ToString())).Count();
            comboAB.SelectedIndex = comboAB.Items.Cast<ComboBoxItem>().TakeWhile(cbi => !(cbi.Tag).Equals(settings.iFilterBlueArmor.ToString())).Count();
            comboAY.SelectedIndex = comboAY.Items.Cast<ComboBoxItem>().TakeWhile(cbi => !(cbi.Tag).Equals(settings.iFilterYellowArmor.ToString())).Count();
            comboJB.SelectedIndex = comboJB.Items.Cast<ComboBoxItem>().TakeWhile(cbi => !(cbi.Tag).Equals(settings.iFilterBlueJewelry.ToString())).Count();
            comboJY.SelectedIndex = comboJY.Items.Cast<ComboBoxItem>().TakeWhile(cbi => !(cbi.Tag).Equals(settings.iFilterYellowJewelry.ToString())).Count();
            comboGems.SelectedIndex = comboGems.Items.Cast<ComboBoxItem>().TakeWhile(cbi => !(cbi.Tag).Equals(settings.iFilterGems.ToString())).Count();
            comboMisc.SelectedIndex = comboMisc.Items.Cast<ComboBoxItem>().TakeWhile(cbi => !(cbi.Tag).Equals(settings.iFilterMisc.ToString())).Count();
            comboPotions.SelectedIndex = comboPotions.Items.Cast<ComboBoxItem>().TakeWhile(cbi => !(cbi.Tag).Equals(settings.iFilterPotions.ToString())).Count();
            comboPotionLevel.SelectedIndex = comboPotionLevel.Items.Cast<ComboBoxItem>().TakeWhile(cbi => !(cbi.Tag).Equals(settings.iFilterPotionLevel.ToString())).Count();
            checkCraftTomes.IsChecked = settings.bPickupCraftTomes;
            checkDesigns.IsChecked = settings.bPickupPlans;
            checkFollower.IsChecked = settings.bPickupFollower;
            checkGemEmerald.IsChecked = settings.bGemsEmerald;
            checkGemAmethyst.IsChecked = settings.bGemsAmethyst;
            checkGemTopaz.IsChecked = settings.bGemsTopaz;
            checkGemRuby.IsChecked = settings.bGemsRuby;
            if (settings.bSalvageJunk)
            {
                btnSalvage.IsChecked = true;
                btnSell.IsChecked = false;
            }
            else
            {
                btnSell.IsChecked = true;
                btnSalvage.IsChecked = false;
            }
            if (settings.bUseGilesFilters)
            {
                btnRulesGiles.IsChecked = true;
                btnRulesCustom.IsChecked = false;
            }
            else
            {
                btnRulesCustom.IsChecked = true;
                btnRulesGiles.IsChecked = false;
            }
            slideWeapon.Value = Math.Round(settings.iNeedPointsToKeepWeapon);
            WeaponText.Text = slideWeapon.Value.ToString();
            slideArmor.Value = Math.Round(settings.iNeedPointsToKeepArmor);
            ArmorText.Text = slideArmor.Value.ToString();
            slideJewelry.Value = Math.Round(settings.iNeedPointsToKeepJewelry);
            JewelryText.Text = slideJewelry.Value.ToString();
            slideNotifyWeapon.Value = Math.Round(settings.iNeedPointsToNotifyWeapon);
            WeaponNotifyText.Text = slideNotifyWeapon.Value.ToString();
            slideNotifyArmor.Value = Math.Round(settings.iNeedPointsToNotifyArmor);
            ArmorNotifyText.Text = slideNotifyArmor.Value.ToString();
            slideNotifyJewelry.Value = Math.Round(settings.iNeedPointsToNotifyJewelry);
            JewelryNotifyText.Text = slideNotifyJewelry.Value.ToString();
            slideGoldAmount.Value = settings.iMinimumGoldStack;
            textGoldAmount.Text = settings.iMinimumGoldStack.ToString();
            slideTriggerRange.Value = settings.iMonsterKillRange;
            textTriggerRange.Text = settings.iMonsterKillRange.ToString();
            slideLootDelay.Value = settings.iKillLootDelay;
            textLootDelay.Text = settings.iKillLootDelay.ToString();
            slideVaultDelay.Value = settings.iDHVaultMovementDelay;
            textVaultDelay.Text = settings.iDHVaultMovementDelay.ToString();
            slideKite0.Value = settings.iKiteDistanceBarb;
            textKite0.Text = settings.iKiteDistanceBarb.ToString();
            slideKite2.Value = settings.iKiteDistanceWiz;
            textKite2.Text = settings.iKiteDistanceWiz.ToString();
            slideKite3.Value = settings.iKiteDistanceWitch;
            textKite3.Text = settings.iKiteDistanceWitch.ToString();
            slideKite4.Value = settings.iKiteDistanceDemon;
            textKite4.Text = settings.iKiteDistanceDemon.ToString();
            if (settings.iTreasureGoblinPriority == 0)
            {
                checkTreasureNormal.IsChecked = false;
                checkTreasurePrioritize.IsChecked = false;
                checkTreasureKamikaze.IsChecked = false;
                checkTreasureIgnore.IsChecked = true;
            }
            else if (settings.iTreasureGoblinPriority == 1)
            {
                checkTreasureIgnore.IsChecked = false;
                checkTreasurePrioritize.IsChecked = false;
                checkTreasureKamikaze.IsChecked = false;
                checkTreasureNormal.IsChecked = true;
            }
            else if (settings.iTreasureGoblinPriority == 2)
            {
                checkTreasureIgnore.IsChecked = false;
                checkTreasureNormal.IsChecked = false;
                checkTreasureKamikaze.IsChecked = false;
                checkTreasurePrioritize.IsChecked = true;
            }
            else
            {
                checkTreasureIgnore.IsChecked = false;
                checkTreasureNormal.IsChecked = false;
                checkTreasurePrioritize.IsChecked = false;
                checkTreasureKamikaze.IsChecked = true;
            }
            if (settings.bIgnoreAllShrines)
            {
                checkIgnoreNone.IsChecked = false;
                checkIgnoreAll.IsChecked = true;
            }
            else
            {
                checkIgnoreAll.IsChecked = false;
                checkIgnoreNone.IsChecked = true;
            }
            checkBacktracking.IsChecked = settings.bEnableBacktracking;
            checkCritical.IsChecked = settings.bEnableCriticalMass;
            checkGrave.IsChecked = settings.bEnableCriticalMass;
            checkAvoidance.IsChecked = settings.bEnableAvoidance;
            checkGlobes.IsChecked = settings.bEnableGlobes;
            slideContainerRange.Value = settings.iContainerOpenRange;
            textContainerRange.Text = settings.iContainerOpenRange.ToString();
            slideDestructibleRange.Value = settings.iDestructibleAttackRange;
            textDestructibleRange.Text = settings.iDestructibleAttackRange.ToString();
            checkIgnoreCorpses.IsChecked = settings.bIgnoreCorpses;
            checkMovementAbilities.IsChecked = settings.bOutOfCombatMovementPowers;
            textTPS.Text = settings.iTPSAmount.ToString();
            slideTPS.Value = settings.iTPSAmount;
            checkTPS.IsChecked = settings.bEnableTPS;
            checkLogStucks.IsChecked = settings.bLogStucks;
            checkProwl.IsChecked = settings.bEnableProwl;
            textProwlKey.Text = sProwlAPIKey;
            checkEmail.IsChecked = settings.bEnableEmail;
            checkLegendaryNotify.IsChecked = settings.bEnableLegendaryNotifyScore;
            txtEmailAddress.Text = sEmailAddress;
            txtEmailPassword.Text = sEmailPassword;
            txtBotName.Text = sBotName; checkAndroid.IsChecked = settings.bEnableAndroid;
            textAndroidKey.Text = sAndroidAPIKey;
            checkUnstucker.IsChecked = settings.bEnableUnstucker;
            checkProfileReload.IsChecked = settings.bEnableProfileReloading;
            checkExtendedRange.IsChecked = settings.bExtendedKillRange;
            checkSelectiveWW.IsChecked = settings.bSelectiveWhirlwind;
            checkWrath90.IsChecked = settings.bWrath90Seconds;
            checkWaitWrath.IsChecked = settings.bWaitForWrath;
            checkKiteArchonOnly.IsChecked = settings.bKiteOnlyArchon;
            checkWaitArchonAzmo.IsChecked = settings.bWaitForArchon;
            checkGoblinWrath.IsChecked = settings.bGoblinWrath;
            checkFuryDumpWrath.IsChecked = settings.bFuryDumpWrath;
            checkFuryDumpAlways.IsChecked = settings.bFuryDumpAlways;
            checkDebugInfo.IsChecked = settings.bDebugInfo;
            checkMonkInna.IsChecked = settings.bMonkInnaSet;
            slidePot0.Value = Math.Floor(settings.dEmergencyHealthPotionBarb * 100);
            slidePot1.Value = Math.Floor(settings.dEmergencyHealthPotionMonk * 100);
            slidePot2.Value = Math.Floor(settings.dEmergencyHealthPotionWiz * 100);
            slidePot3.Value = Math.Floor(settings.dEmergencyHealthPotionWitch * 100);
            slidePot4.Value = Math.Floor(settings.dEmergencyHealthPotionDemon * 100);
            textPot0.Text = Math.Floor(settings.dEmergencyHealthPotionBarb * 100).ToString();
            textPot1.Text = Math.Floor(settings.dEmergencyHealthPotionMonk * 100).ToString();
            textPot2.Text = Math.Floor(settings.dEmergencyHealthPotionWiz * 100).ToString();
            textPot3.Text = Math.Floor(settings.dEmergencyHealthPotionWitch * 100).ToString();
            textPot4.Text = Math.Floor(settings.dEmergencyHealthPotionDemon * 100).ToString();
            slideGlobe0.Value = Math.Floor(settings.dEmergencyHealthGlobeBarb * 100);
            slideGlobe1.Value = Math.Floor(settings.dEmergencyHealthGlobeMonk * 100);
            slideGlobe2.Value = Math.Floor(settings.dEmergencyHealthGlobeWiz * 100);
            slideGlobe3.Value = Math.Floor(settings.dEmergencyHealthGlobeWitch * 100);
            slideGlobe4.Value = Math.Floor(settings.dEmergencyHealthGlobeDemon * 100);
            textGlobe0.Text = Math.Floor(settings.dEmergencyHealthGlobeBarb * 100).ToString();
            textGlobe1.Text = Math.Floor(settings.dEmergencyHealthGlobeMonk * 100).ToString();
            textGlobe2.Text = Math.Floor(settings.dEmergencyHealthGlobeWiz * 100).ToString();
            textGlobe3.Text = Math.Floor(settings.dEmergencyHealthGlobeWitch * 100).ToString();
            textGlobe4.Text = Math.Floor(settings.dEmergencyHealthGlobeDemon * 100).ToString();
            bSuppressEventChanges = false;
        }
        #endregion
        // * END OF CONFIG WINDOW REGION
        // Primary/"Default" functions, like logs, initialize, DB event handling etc.
    }
}
