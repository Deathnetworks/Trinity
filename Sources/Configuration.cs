﻿using GilesTrinity.DbProvider;
using GilesTrinity.Settings.Combat;
using GilesTrinity.Settings.Loot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Zeta.Common.Plugins;
using Zeta.CommonBot;
using Zeta.Navigation;
namespace GilesTrinity
{
    public partial class GilesTrinity : IPlugin
    {
        // Save Configuration
        private void SaveConfiguration()
        {
            DbHelper.Log("Entry Assembly Location : {0}", Assembly.GetEntryAssembly().Location);
            DbHelper.Log("Entry Assembly Directory : {0}", Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));
            Settings.Save(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), @"Settings\GilesTrinity.xml"));
        }
        // Load Configuration
        private void LoadConfiguration()
        {
            DbHelper.Log("Entry Assembly Location : {0}", Assembly.GetEntryAssembly().Location);
            DbHelper.Log("Entry Assembly Directory : {0}", Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));
            Settings.Load(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), @"Settings\GilesTrinity.xml"));
        }
        // * CONFIG WINDOW REGION
        #region configWindow
        // First we create a variable that is of the "type" of the actual config window item - eg a "RadioButton" for each, well, radiobutton
        // Later on we will "Link" these variables to the ACTUAL items within the XAML file, so we can do things with the XAML stuff
        // I try to match the names of the variables here, with the "Name=" I give the item in the XAML - this isn't necessary, but makes things simpler
        private Button saveButton, defaultButton, testButton, sortButton, resetCombat, resetAOE0, resetAOE1, resetAOE2, resetAOE3, resetAOE4, resetWorld, resetItems, resetTown, resetAdvanced, resetMobile;
        private RadioButton checkTreasureIgnore, checkTreasureNormal, checkTreasurePrioritize, checkTreasureKamikaze, btnRulesGiles, btnRulesCustom, btnRulesTrinityWithScript, btnSalvage, btnSell, checkIgnoreAll, checkIgnoreNone;
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
            double dThisHealthLimit = (Math.Round(thisslider.Value / 100));
            textAOEHealth[iClass, iAvoid - 1].Text = (dThisHealthLimit * 100).ToString();
            switch (iClass)
            {
                case 0:
                    // Barbs
                    switch (iAvoid)
                    {
                        case 1:
                            Settings.Combat.Barbarian.AvoidArcaneHealth = (float)dThisHealthLimit;
                            break;
                        case 2:
                            Settings.Combat.Barbarian.AvoidDesecratorHealth = (float)dThisHealthLimit;
                            break;
                        case 3:
                            Settings.Combat.Barbarian.AvoidMoltenCoreHealth = (float)dThisHealthLimit;
                            break;
                        case 4:
                            Settings.Combat.Barbarian.AvoidMoltenTrailHealth = (float)dThisHealthLimit;
                            break;
                        case 5:
                            Settings.Combat.Barbarian.AvoidPoisonTreeHealth = (float)dThisHealthLimit;
                            break;
                        case 6:
                            Settings.Combat.Barbarian.AvoidPlagueCloudHealth = (float)dThisHealthLimit;
                            break;
                        case 7:
                            Settings.Combat.Barbarian.AvoidIceBallsHealth = (float)dThisHealthLimit;
                            break;
                        case 8:
                            Settings.Combat.Barbarian.AvoidPlagueHandsHealth = (float)dThisHealthLimit;
                            break;
                        case 9:
                            Settings.Combat.Barbarian.AvoidBeesWaspsHealth = (float)dThisHealthLimit;
                            break;
                        case 10:
                            Settings.Combat.Barbarian.AvoidAzmoPoolsHealth = (float)dThisHealthLimit;
                            break;
                        case 11:
                            Settings.Combat.Barbarian.AvoidAzmoBodiesHealth = (float)dThisHealthLimit;
                            break;
                        case 12:
                            Settings.Combat.Barbarian.AvoidShamanFireHealth = (float)dThisHealthLimit;
                            break;
                        case 13:
                            Settings.Combat.Barbarian.AvoidGhomGasHealth = (float)dThisHealthLimit;
                            break;
                    }
                    break;
                case 1:
                    // Monks
                    switch (iAvoid)
                    {
                        case 1:
                            Settings.Combat.Monk.AvoidArcaneHealth = (float)dThisHealthLimit;
                            break;
                        case 2:
                            Settings.Combat.Monk.AvoidDesecratorHealth = (float)dThisHealthLimit;
                            break;
                        case 3:
                            Settings.Combat.Monk.AvoidMoltenCoreHealth = (float)dThisHealthLimit;
                            break;
                        case 4:
                            Settings.Combat.Monk.AvoidMoltenTrailHealth = (float)dThisHealthLimit;
                            break;
                        case 5:
                            Settings.Combat.Monk.AvoidPoisonTreeHealth = (float)dThisHealthLimit;
                            break;
                        case 6:
                            Settings.Combat.Monk.AvoidPlagueCloudHealth = (float)dThisHealthLimit;
                            break;
                        case 7:
                            Settings.Combat.Monk.AvoidIceBallsHealth = (float)dThisHealthLimit;
                            break;
                        case 8:
                            Settings.Combat.Monk.AvoidPlagueHandsHealth = (float)dThisHealthLimit;
                            break;
                        case 9:
                            Settings.Combat.Monk.AvoidBeesWaspsHealth = (float)dThisHealthLimit;
                            break;
                        case 10:
                            Settings.Combat.Monk.AvoidAzmoPoolsHealth = (float)dThisHealthLimit;
                            break;
                        case 11:
                            Settings.Combat.Monk.AvoidAzmoBodiesHealth = (float)dThisHealthLimit;
                            break;
                        case 12:
                            Settings.Combat.Monk.AvoidShamanFireHealth = (float)dThisHealthLimit;
                            break;
                        case 13:
                            Settings.Combat.Monk.AvoidGhomGasHealth = (float)dThisHealthLimit;
                            break;
                    }
                    break;
                case 2:
                    // Wizards
                    switch (iAvoid)
                    {
                        case 1:
                            Settings.Combat.Wizard.AvoidArcaneHealth = (float)dThisHealthLimit;
                            break;
                        case 2:
                            Settings.Combat.Wizard.AvoidDesecratorHealth = (float)dThisHealthLimit;
                            break;
                        case 3:
                            Settings.Combat.Wizard.AvoidMoltenCoreHealth = (float)dThisHealthLimit;
                            break;
                        case 4:
                            Settings.Combat.Wizard.AvoidMoltenTrailHealth = (float)dThisHealthLimit;
                            break;
                        case 5:
                            Settings.Combat.Wizard.AvoidPoisonTreeHealth = (float)dThisHealthLimit;
                            break;
                        case 6:
                            Settings.Combat.Wizard.AvoidPlagueCloudHealth = (float)dThisHealthLimit;
                            break;
                        case 7:
                            Settings.Combat.Wizard.AvoidIceBallsHealth = (float)dThisHealthLimit;
                            break;
                        case 8:
                            Settings.Combat.Wizard.AvoidPlagueHandsHealth = (float)dThisHealthLimit;
                            break;
                        case 9:
                            Settings.Combat.Wizard.AvoidBeesWaspsHealth = (float)dThisHealthLimit;
                            break;
                        case 10:
                            Settings.Combat.Wizard.AvoidAzmoPoolsHealth = (float)dThisHealthLimit;
                            break;
                        case 11:
                            Settings.Combat.Wizard.AvoidAzmoBodiesHealth = (float)dThisHealthLimit;
                            break;
                        case 12:
                            Settings.Combat.Wizard.AvoidShamanFireHealth = (float)dThisHealthLimit;
                            break;
                        case 13:
                            Settings.Combat.Wizard.AvoidGhomGasHealth = (float)dThisHealthLimit;
                            break;
                    }
                    break;
                case 3:
                    // WD's
                    switch (iAvoid)
                    {
                        case 1:
                            Settings.Combat.WitchDoctor.AvoidArcaneHealth = (float)dThisHealthLimit;
                            break;
                        case 2:
                            Settings.Combat.WitchDoctor.AvoidDesecratorHealth = (float)dThisHealthLimit;
                            break;
                        case 3:
                            Settings.Combat.WitchDoctor.AvoidMoltenCoreHealth = (float)dThisHealthLimit;
                            break;
                        case 4:
                            Settings.Combat.WitchDoctor.AvoidMoltenTrailHealth = (float)dThisHealthLimit;
                            break;
                        case 5:
                            Settings.Combat.WitchDoctor.AvoidPoisonTreeHealth = (float)dThisHealthLimit;
                            break;
                        case 6:
                            Settings.Combat.WitchDoctor.AvoidPlagueCloudHealth = (float)dThisHealthLimit;
                            break;
                        case 7:
                            Settings.Combat.WitchDoctor.AvoidIceBallsHealth = (float)dThisHealthLimit;
                            break;
                        case 8:
                            Settings.Combat.WitchDoctor.AvoidPlagueHandsHealth = (float)dThisHealthLimit;
                            break;
                        case 9:
                            Settings.Combat.WitchDoctor.AvoidBeesWaspsHealth = (float)dThisHealthLimit;
                            break;
                        case 10:
                            Settings.Combat.WitchDoctor.AvoidAzmoPoolsHealth = (float)dThisHealthLimit;
                            break;
                        case 11:
                            Settings.Combat.WitchDoctor.AvoidAzmoBodiesHealth = (float)dThisHealthLimit;
                            break;
                        case 12:
                            Settings.Combat.WitchDoctor.AvoidShamanFireHealth = (float)dThisHealthLimit;
                            break;
                        case 13:
                            Settings.Combat.WitchDoctor.AvoidGhomGasHealth = (float)dThisHealthLimit;
                            break;
                    }
                    break;
                case 4:
                    // DH's
                    switch (iAvoid)
                    {
                        case 1:
                            Settings.Combat.DemonHunter.AvoidArcaneHealth = (float)dThisHealthLimit;
                            break;
                        case 2:
                            Settings.Combat.DemonHunter.AvoidDesecratorHealth = (float)dThisHealthLimit;
                            break;
                        case 3:
                            Settings.Combat.DemonHunter.AvoidMoltenCoreHealth = (float)dThisHealthLimit;
                            break;
                        case 4:
                            Settings.Combat.DemonHunter.AvoidMoltenTrailHealth = (float)dThisHealthLimit;
                            break;
                        case 5:
                            Settings.Combat.DemonHunter.AvoidPoisonTreeHealth = (float)dThisHealthLimit;
                            break;
                        case 6:
                            Settings.Combat.DemonHunter.AvoidPlagueCloudHealth = (float)dThisHealthLimit;
                            break;
                        case 7:
                            Settings.Combat.DemonHunter.AvoidIceBallsHealth = (float)dThisHealthLimit;
                            break;
                        case 8:
                            Settings.Combat.DemonHunter.AvoidPlagueHandsHealth = (float)dThisHealthLimit;
                            break;
                        case 9:
                            Settings.Combat.DemonHunter.AvoidBeesWaspsHealth = (float)dThisHealthLimit;
                            break;
                        case 10:
                            Settings.Combat.DemonHunter.AvoidAzmoPoolsHealth = (float)dThisHealthLimit;
                            break;
                        case 11:
                            Settings.Combat.DemonHunter.AvoidAzmoBodiesHealth = (float)dThisHealthLimit;
                            break;
                        case 12:
                            Settings.Combat.DemonHunter.AvoidShamanFireHealth = (float)dThisHealthLimit;
                            break;
                        case 13:
                            Settings.Combat.DemonHunter.AvoidGhomGasHealth = (float)dThisHealthLimit;
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
                    Settings.Combat.AvoidanceRadius.Arcane = iThisAvoidRadius;
                    break;
                case 2:
                    Settings.Combat.AvoidanceRadius.Desecrator = iThisAvoidRadius;
                    break;
                case 3:
                    Settings.Combat.AvoidanceRadius.MoltenCore = iThisAvoidRadius;
                    break;
                case 4:
                    Settings.Combat.AvoidanceRadius.MoltenTrail = iThisAvoidRadius;
                    break;
                case 5:
                    Settings.Combat.AvoidanceRadius.PoisonTree = iThisAvoidRadius;
                    break;
                case 6:
                    Settings.Combat.AvoidanceRadius.PlagueCloud = iThisAvoidRadius;
                    break;
                case 7:
                    Settings.Combat.AvoidanceRadius.IceBalls = iThisAvoidRadius;
                    break;
                case 8:
                    Settings.Combat.AvoidanceRadius.PlagueHands = iThisAvoidRadius;
                    break;
                case 9:
                    Settings.Combat.AvoidanceRadius.BeesWasps = iThisAvoidRadius;
                    break;
                case 10:
                    Settings.Combat.AvoidanceRadius.AzmoPools = iThisAvoidRadius;
                    break;
                case 11:
                    Settings.Combat.AvoidanceRadius.AzmoBodies = iThisAvoidRadius;
                    break;
                case 12:
                    Settings.Combat.AvoidanceRadius.ShamanFire = iThisAvoidRadius;
                    break;
                case 13:
                    Settings.Combat.AvoidanceRadius.GhomGas = iThisAvoidRadius;
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
            Settings.WorldObject.IgnoreNonBlocking = true;
        }
        private void checkIgnoreCorpses_uncheck(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            Settings.WorldObject.IgnoreNonBlocking = false;
        }
        private void slidePot0_Scroll(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (bSuppressEventChanges)
                return;
            slidePot0.Value = Math.Round(slidePot0.Value);
            textPot0.Text = slidePot0.Value.ToString();
            Settings.Combat.Barbarian.PotionLevel = (float)(Math.Round(slidePot0.Value) / 100);
        }
        private void slidePot1_Scroll(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (bSuppressEventChanges)
                return;
            slidePot1.Value = Math.Round(slidePot1.Value);
            textPot1.Text = slidePot1.Value.ToString();
            Settings.Combat.Monk.PotionLevel = (float)(Math.Round(slidePot1.Value) / 100);
        }
        private void slidePot2_Scroll(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (bSuppressEventChanges)
                return;
            slidePot2.Value = Math.Round(slidePot2.Value);
            textPot2.Text = slidePot2.Value.ToString();
            Settings.Combat.Wizard.PotionLevel = (float)(Math.Round(slidePot2.Value) / 100);
        }
        private void slidePot3_Scroll(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (bSuppressEventChanges)
                return;
            slidePot3.Value = Math.Round(slidePot3.Value);
            textPot3.Text = slidePot3.Value.ToString();
            Settings.Combat.WitchDoctor.PotionLevel = (float)(Math.Round(slidePot3.Value) / 100);
        }
        private void slidePot4_Scroll(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (bSuppressEventChanges)
                return;
            slidePot4.Value = Math.Round(slidePot4.Value);
            textPot4.Text = slidePot4.Value.ToString();
            Settings.Combat.DemonHunter.PotionLevel = (float)(Math.Round(slidePot4.Value) / 100);
        }
        private void slideGlobe0_Scroll(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (bSuppressEventChanges)
                return;
            slideGlobe0.Value = Math.Round(slideGlobe0.Value);
            textGlobe0.Text = slideGlobe0.Value.ToString();
            Settings.Combat.Barbarian.HealthGlobeLevel = (float)(Math.Round(slideGlobe0.Value) / 100);
        }
        private void slideGlobe1_Scroll(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (bSuppressEventChanges)
                return;
            slideGlobe1.Value = Math.Round(slideGlobe1.Value);
            textGlobe1.Text = slideGlobe1.Value.ToString();
            Settings.Combat.Monk.HealthGlobeLevel = (float)(Math.Round(slideGlobe1.Value) / 100);
        }
        private void slideGlobe2_Scroll(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (bSuppressEventChanges)
                return;
            slideGlobe2.Value = Math.Round(slideGlobe2.Value);
            textGlobe2.Text = slideGlobe2.Value.ToString();
            Settings.Combat.Wizard.HealthGlobeLevel = (float)(Math.Round(slideGlobe2.Value) / 100);
        }
        private void slideGlobe3_Scroll(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (bSuppressEventChanges)
                return;
            slideGlobe3.Value = Math.Round(slideGlobe3.Value);
            textGlobe3.Text = slideGlobe3.Value.ToString();
            Settings.Combat.WitchDoctor.HealthGlobeLevel = (float)(Math.Round(slideGlobe3.Value) / 100);
        }
        private void slideGlobe4_Scroll(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (bSuppressEventChanges)
                return;
            slideGlobe4.Value = Math.Round(slideGlobe4.Value);
            textGlobe4.Text = slideGlobe4.Value.ToString();
            Settings.Combat.DemonHunter.HealthGlobeLevel = (float)(Math.Round(slideGlobe4.Value) / 100);
        }
        private void trackContainerRange_Scroll(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (bSuppressEventChanges)
                return;
            slideContainerRange.Value = Math.Round(slideContainerRange.Value);
            textContainerRange.Text = slideContainerRange.Value.ToString();
            Settings.WorldObject.ContainerOpenRange = (int)Math.Round(slideContainerRange.Value);
        }
        private void trackDestructibleRange_Scroll(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (bSuppressEventChanges)
                return;
            slideDestructibleRange.Value = Math.Round(slideDestructibleRange.Value);
            textDestructibleRange.Text = slideDestructibleRange.Value.ToString();
            Settings.WorldObject.DestructibleRange = (int)Math.Round(slideDestructibleRange.Value);
        }
        private void checkIgnoreAll_check(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            Settings.WorldObject.UseShrine = false;
        }
        private void checkIgnoreNone_check(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            Settings.WorldObject.UseShrine = true;
        }
        private void trackGoldAmount_Scroll(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (bSuppressEventChanges)
                return;
            slideGoldAmount.Value = Math.Round(slideGoldAmount.Value);
            textGoldAmount.Text = slideGoldAmount.Value.ToString();
            Settings.Loot.Pickup.MinimumGoldStack = Convert.ToInt32(Math.Round(slideGoldAmount.Value));
        }
        private void comboWB_changed(object sender, SelectionChangedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            Settings.Loot.Pickup.WeaponBlueLevel = Convert.ToInt32(comboWB.SelectedValue);
        }
        private void comboLegendary_changed(object sender, SelectionChangedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            Settings.Loot.Pickup.LegendaryLevel = Convert.ToInt32(comboLegendary.SelectedValue);
        }
        private void comboWY_changed(object sender, SelectionChangedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            Settings.Loot.Pickup.WeaponYellowLevel = Convert.ToInt32(comboWY.SelectedValue);
        }
        private void comboAB_changed(object sender, SelectionChangedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            Settings.Loot.Pickup.ArmorBlueLevel = Convert.ToInt32(comboAB.SelectedValue);
        }
        private void comboAY_changed(object sender, SelectionChangedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            Settings.Loot.Pickup.ArmorYellowLevel = Convert.ToInt32(comboAY.SelectedValue);
        }
        private void comboJB_changed(object sender, SelectionChangedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            Settings.Loot.Pickup.JewelryBlueLevel = Convert.ToInt32(comboJB.SelectedValue);
        }
        private void comboJY_changed(object sender, SelectionChangedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            Settings.Loot.Pickup.JewelryYellowLevel = Convert.ToInt32(comboJY.SelectedValue);
        }
        private void comboGems_changed(object sender, SelectionChangedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            Settings.Loot.Pickup.GemLevel = Convert.ToInt32(comboGems.SelectedValue);
        }
        private void comboMisc_changed(object sender, SelectionChangedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            Settings.Loot.Pickup.MiscItemLevel = Convert.ToInt32(comboMisc.SelectedValue);
        }
        private void comboPotions_changed(object sender, SelectionChangedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            Settings.Loot.Pickup.PotionMode = (PotionMode)Convert.ToInt32(comboPotions.SelectedValue);
        }
        private void comboPotionLevel_changed(object sender, SelectionChangedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            Settings.Loot.Pickup.Potionlevel = Convert.ToInt32(comboPotionLevel.SelectedValue);
        }
        private void checkFollower_check(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            Settings.Loot.Pickup.FollowerItem = true;
        }
        private void checkFollower_uncheck(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            Settings.Loot.Pickup.FollowerItem = false;
        }
        private void checkDesigns_check(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            Settings.Loot.Pickup.DesignPlan = true;
        }
        private void checkDesigns_uncheck(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            Settings.Loot.Pickup.DesignPlan = false;
        }
        private void checkCraftTomes_check(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            Settings.Loot.Pickup.CraftTomes = true;
        }
        private void checkCraftTomes_uncheck(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            Settings.Loot.Pickup.CraftTomes = false;
        }
        private void btnRulesGiles_check(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            Settings.Loot.ItemFilterMode = ItemFilterMode.TrinityOnly;
        }
        private void btnRulesCustom_check(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            Settings.Loot.ItemFilterMode = ItemFilterMode.DemonBuddy;
        }
        private void btnRulesTrinityWithScript_check(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            Settings.Loot.ItemFilterMode = ItemFilterMode.TrinityWithItemRules;
        }
        private void btnSalvage_check(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            Settings.Loot.TownRun.TrashMode = TrashMode.Salvaging;
        }
        private void btnSell_check(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            Settings.Loot.TownRun.TrashMode = TrashMode.Salvaging;
        }
        private void checkEmerald_check(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            Settings.Loot.Pickup.GemType = Settings.Loot.Pickup.GemType | TrinityGemType.Emerald;
        }
        private void checkEmerald_uncheck(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            if ((Settings.Loot.Pickup.GemType & TrinityGemType.Emerald) == TrinityGemType.Emerald)
            {
                Settings.Loot.Pickup.GemType = (int)Settings.Loot.Pickup.GemType - TrinityGemType.Emerald;
            }
        }
        private void checkAmethyst_check(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            Settings.Loot.Pickup.GemType = Settings.Loot.Pickup.GemType | TrinityGemType.Amethys;
        }
        private void checkAmethyst_uncheck(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            if ((Settings.Loot.Pickup.GemType & TrinityGemType.Amethys) == TrinityGemType.Amethys)
            {
                Settings.Loot.Pickup.GemType = (int)Settings.Loot.Pickup.GemType - TrinityGemType.Amethys;
            }
        }
        private void checkTopaz_check(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            Settings.Loot.Pickup.GemType = Settings.Loot.Pickup.GemType | TrinityGemType.Topaz;
        }
        private void checkTopaz_uncheck(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            if ((Settings.Loot.Pickup.GemType & TrinityGemType.Topaz) == TrinityGemType.Topaz)
            {
                Settings.Loot.Pickup.GemType = (int)Settings.Loot.Pickup.GemType - TrinityGemType.Topaz;
            }
        }
        private void checkRuby_check(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            Settings.Loot.Pickup.GemType = Settings.Loot.Pickup.GemType | TrinityGemType.Ruby;
        }
        private void checkRuby_uncheck(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            if ((Settings.Loot.Pickup.GemType & TrinityGemType.Ruby) == TrinityGemType.Ruby)
            {
                Settings.Loot.Pickup.GemType = (int)Settings.Loot.Pickup.GemType - TrinityGemType.Ruby;
            }
        }
        private void checkAndroid_check(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            Settings.Notification.AndroidEnabled = true;
        }
        private void checkAndroid_uncheck(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            Settings.Notification.AndroidEnabled = false;
        }
        private void textAndroid_change(object sender, TextChangedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            Settings.Notification.AndroidKey = textAndroidKey.Text;
        }
        private void checkProwl_check(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            Settings.Notification.IPhoneEnabled = true;
        }
        private void checkProwl_uncheck(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            Settings.Notification.IPhoneEnabled = false;
        }
        private void textProwl_change(object sender, TextChangedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            Settings.Notification.IPhoneKey = textProwlKey.Text;
        }
        private void checkEmail_check(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            Settings.Notification.MailEnabled = true;
        }
        private void checkEmail_uncheck(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            Settings.Notification.MailEnabled = false;
        }
        private void checkLegendaryNotifyScore_check(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            Settings.Notification.LegendaryScoring = true;
        }
        private void checkLegendaryNotifyScore_uncheck(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            Settings.Notification.LegendaryScoring = false;
        }
        private void txtEmailAddress_change(object sender, TextChangedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            Settings.Notification.EmailAddress = txtEmailAddress.Text;
        }
        private void txtEmailPassword_change(object sender, TextChangedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            Settings.Notification.EmailPassword = txtEmailPassword.Text;
        }
        private void txtBotName_change(object sender, TextChangedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            Settings.Notification.BotName = txtBotName.Text;
        }
        private void trackScoreWeapons_Scroll(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (bSuppressEventChanges)
                return;
            slideWeapon.Value = Math.Round(slideWeapon.Value);
            Settings.Loot.TownRun.WeaponScore = (int)slideWeapon.Value;
            WeaponText.Text = slideWeapon.Value.ToString();
        }
        private void trackScoreArmor_Scroll(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (bSuppressEventChanges)
                return;
            slideArmor.Value = Math.Round(slideArmor.Value);
            Settings.Loot.TownRun.ArmorScore = (int)slideArmor.Value;
            ArmorText.Text = slideArmor.Value.ToString();
        }
        private void trackScoreJewelry_Scroll(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (bSuppressEventChanges)
                return;
            slideJewelry.Value = Math.Round(slideJewelry.Value);
            Settings.Loot.TownRun.JewelryScore = (int)slideJewelry.Value;
            JewelryText.Text = slideJewelry.Value.ToString();
        }
        private void trackNotifyWeapons_Scroll(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (bSuppressEventChanges)
                return;
            slideNotifyWeapon.Value = Math.Round(slideNotifyWeapon.Value);
            Settings.Notification.WeaponScore = (int)slideNotifyWeapon.Value;
            WeaponNotifyText.Text = slideNotifyWeapon.Value.ToString();
        }
        private void trackNotifyArmor_Scroll(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (bSuppressEventChanges)
                return;
            slideNotifyArmor.Value = Math.Round(slideNotifyArmor.Value);
            Settings.Notification.ArmorScore = (int)slideNotifyArmor.Value;
            ArmorNotifyText.Text = slideNotifyArmor.Value.ToString();
        }
        private void trackNotifyJewelry_Scroll(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (bSuppressEventChanges)
                return;
            slideNotifyJewelry.Value = Math.Round(slideNotifyJewelry.Value);
            Settings.Notification.JewelryScore = (int)slideNotifyJewelry.Value;
            JewelryNotifyText.Text = slideNotifyJewelry.Value.ToString();
        }
        private void checkBacktracking_check(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            Settings.Combat.Misc.AllowBacktracking = true;
        }
        private void checkBacktracking_uncheck(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            Settings.Combat.Misc.AllowBacktracking = false;
        }
        private void checkDebugInfo_check(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            Settings.Advanced.DebugInStatusBar = true;
        }
        private void checkDebugInfo_uncheck(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            Settings.Advanced.DebugInStatusBar = false;
        }
        private void checkTPS_check(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            Settings.Advanced.TPSEnabled = true;
            BotMain.TicksPerSecond = (int)Settings.Advanced.TPSLimit;
        }
        private void checkTPS_uncheck(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            Settings.Advanced.TPSEnabled = false;
            BotMain.TicksPerSecond = 10;
        }
        private void checkProfileReload_check(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            Settings.Advanced.AllowRestartGame = true;
        }
        private void checkProfileReload_uncheck(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            Settings.Advanced.AllowRestartGame = false;
        }
        private void checkUnstucker_check(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            Settings.Advanced.UnstuckerEnabled = true;
            Navigator.StuckHandler = new GilesStuckHandler();
        }
        private void checkUnstucker_uncheck(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            Settings.Advanced.UnstuckerEnabled = false;
            Navigator.StuckHandler = new DefaultStuckHandler();
        }
        private void checkLogStucks_check(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            Settings.Advanced.LogStuckLocation = true;
        }
        private void checkLogStucks_uncheck(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            Settings.Advanced.LogStuckLocation = false;
        }
        private void checkExtendedRange_check(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            Settings.Combat.Misc.ExtendedTrashKill = true;
        }
        private void checkExtendedRange_uncheck(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            Settings.Combat.Misc.ExtendedTrashKill = false;
        }
        private void checkWrath90_check(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            Settings.Combat.Barbarian.BoonBulKathosPassive = true;
        }
        private void checkWrath90_uncheck(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            Settings.Combat.Barbarian.BoonBulKathosPassive = false;
        }
        private void checkSelectiveWW_check(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            Settings.Combat.Barbarian.SelectiveWirlwind = true;
        }
        private void checkSelectiveWW_uncheck(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            Settings.Combat.Barbarian.SelectiveWirlwind = false;
        }
        private void checkKiteArchonOnly_check(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            Settings.Combat.Wizard.OnlyKiteInArchon = true;
        }
        private void checkKiteArchonOnly_uncheck(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            Settings.Combat.Wizard.OnlyKiteInArchon = false;
        }
        private void checkWaitArchonAzmo_check(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            Settings.Combat.Wizard.WaitArchon = true;
        }
        private void checkWaitArchonAzmo_uncheck(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            Settings.Combat.Wizard.WaitArchon = false;
        }
        private void checkWaitWrath_check(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            Settings.Combat.Barbarian.WaitWOTB = true;
        }
        private void checkWaitWrath_uncheck(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            Settings.Combat.Barbarian.WaitWOTB = false;
        }
        private void checkGoblinWrath_check(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            Settings.Combat.Barbarian.UseWOTBGoblin = true;
        }
        private void checkGoblinWrath_uncheck(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            Settings.Combat.Barbarian.UseWOTBGoblin = false;
        }
        private void checkMonkInna_check(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            Settings.Combat.Monk.HasInnaSet = true;
        }
        private void checkMonkInna_uncheck(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            Settings.Combat.Monk.HasInnaSet = false;
        }
        private void checkFuryDumpWrath_check(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            Settings.Combat.Barbarian.FuryDumpWOTB = true;
        }
        private void checkFuryDumpWrath_uncheck(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            Settings.Combat.Barbarian.FuryDumpWOTB = false;
        }
        private void checkFuryDumpAlways_check(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            Settings.Combat.Barbarian.FuryDumpAlways = true;
        }
        private void checkFuryDumpAlways_uncheck(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            Settings.Combat.Barbarian.FuryDumpAlways = false;
        }
        private void trackTPS_Scroll(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (bSuppressEventChanges)
                return;
            slideTPS.Value = Math.Round(slideTPS.Value);
            textTPS.Text = slideTPS.Value.ToString();
            Settings.Advanced.TPSLimit = (int)Math.Round(slideTPS.Value);
            if (Settings.Advanced.TPSEnabled)
            {
                BotMain.TicksPerSecond = Settings.Advanced.TPSLimit;
            }
        }
        private void checkAvoidance_check(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            Settings.Combat.Misc.AvoidAOE = true;
        }
        private void checkAvoidance_uncheck(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            Settings.Combat.Misc.AvoidAOE = false;
        }
        private void checkGlobes_check(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            Settings.Combat.Misc.CollectHealthGlobe = true;
        }
        private void checkGlobes_uncheck(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            Settings.Combat.Misc.CollectHealthGlobe = false;
        }
        private void checkCritical_check(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            Settings.Combat.Wizard.CriticalMass = true;
        }
        private void checkCritical_uncheck(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            Settings.Combat.Wizard.CriticalMass = false;
        }
        private void checkMovementAbilities_check(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            Settings.Combat.Misc.AllowOOCMovement = true;
        }
        private void checkMovementAbilities_uncheck(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            Settings.Combat.Misc.AllowOOCMovement = false;
        }
        private void slideKite0_Scroll(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (bSuppressEventChanges)
                return;
            slideKite0.Value = Math.Round(slideKite0.Value);
            textKite0.Text = slideKite0.Value.ToString();
            Settings.Combat.Barbarian.KiteLimit = (int)Math.Round(slideKite0.Value);
        }
        private void slideKite2_Scroll(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (bSuppressEventChanges)
                return;
            slideKite2.Value = Math.Round(slideKite2.Value);
            textKite2.Text = slideKite2.Value.ToString();
            Settings.Combat.Wizard.KiteLimit = (int)Math.Round(slideKite2.Value);
        }
        private void slideKite3_Scroll(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (bSuppressEventChanges)
                return;
            slideKite3.Value = Math.Round(slideKite3.Value);
            textKite3.Text = slideKite3.Value.ToString();
            Settings.Combat.WitchDoctor.KiteLimit = (int)Math.Round(slideKite3.Value);
        }
        private void slideKite4_Scroll(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (bSuppressEventChanges)
                return;
            slideKite4.Value = Math.Round(slideKite4.Value);
            textKite4.Text = slideKite4.Value.ToString();
            Settings.Combat.DemonHunter.KiteLimit = (int)Math.Round(slideKite4.Value);
        }
        private void slideVaultDelay_Scroll(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (bSuppressEventChanges)
                return;
            slideVaultDelay.Value = Math.Round(slideVaultDelay.Value);
            textVaultDelay.Text = slideVaultDelay.Value.ToString();
            Settings.Combat.DemonHunter.VaultMovementDelay = (int)Math.Round(slideVaultDelay.Value);
        }
        private void slideLootDelay_Scroll(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (bSuppressEventChanges)
                return;
            slideLootDelay.Value = Math.Round(slideLootDelay.Value);
            textLootDelay.Text = slideLootDelay.Value.ToString();
            Settings.Combat.Misc.DelayAfterKill = (int)Math.Round(slideLootDelay.Value);
        }
        private void trackTriggerRange_Scroll(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (bSuppressEventChanges)
                return;
            slideTriggerRange.Value = Math.Round(slideTriggerRange.Value);
            textTriggerRange.Text = slideTriggerRange.Value.ToString();
            Settings.Combat.Misc.NonEliteRange = (int)Math.Round(slideTriggerRange.Value);
        }
        // The three events for the treasure goblin priority choice
        private void checkTreasureIgnore_check(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            Settings.Combat.Misc.GoblinPriority = GoblinPriority.Ignore;
        }
        private void checkTreasureNormal_check(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            Settings.Combat.Misc.GoblinPriority = GoblinPriority.Normal;
        }
        private void checkTreasurePrioritize_check(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            Settings.Combat.Misc.GoblinPriority = GoblinPriority.Prioritize;
        }
        private void checkTreasureKamikaze_check(object sender, RoutedEventArgs e)
        {
            if (bSuppressEventChanges)
                return;
            Settings.Combat.Misc.GoblinPriority = GoblinPriority.Kamikaze;
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
            Settings.Reset();
            settingsWindowResetValues();
        }
        // Individual reset buttons
        private void resetCombat_Click(object sender, RoutedEventArgs e)
        {
            Settings.Combat.Misc.Reset();
            settingsWindowResetValues();
        }
        private void resetAOE0_Click(object sender, RoutedEventArgs e)
        {
            Settings.Combat.Barbarian.Reset();
            settingsWindowResetValues();
        }
        private void resetAOE1_Click(object sender, RoutedEventArgs e)
        {
            Settings.Combat.Monk.Reset();
            settingsWindowResetValues();
        }
        private void resetAOE2_Click(object sender, RoutedEventArgs e)
        {
            Settings.Combat.Wizard.Reset();
            settingsWindowResetValues();
        }
        private void resetAOE3_Click(object sender, RoutedEventArgs e)
        {
            Settings.Combat.WitchDoctor.Reset();
            settingsWindowResetValues();
        }
        private void resetAOE4_Click(object sender, RoutedEventArgs e)
        {
            Settings.Combat.DemonHunter.Reset();
            settingsWindowResetValues();
        }
        private void resetWorld_Click(object sender, RoutedEventArgs e)
        {
            Settings.WorldObject.Reset();
            settingsWindowResetValues();
        }
        private void resetItems_Click(object sender, RoutedEventArgs e)
        {
            Settings.Loot.Pickup.Reset();
            settingsWindowResetValues();
        }
        private void resetTown_Click(object sender, RoutedEventArgs e)
        {
            Settings.Loot.TownRun.Reset();
            settingsWindowResetValues();
        }
        private void resetAdvanced_Click(object sender, RoutedEventArgs e)
        {
            Settings.Advanced.Reset();
            settingsWindowResetValues();
        }
        private void resetMobile_Click(object sender, RoutedEventArgs e)
        {
            Settings.Notification.Reset();
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
                            DbHelper.Log("AvoidanceRadius.Arcane: {0}", Settings.Combat.AvoidanceRadius.Arcane);
                            slideAOERadius[i, n - 1].Value = Settings.Combat.AvoidanceRadius.Arcane;
                            break;
                        case 2:
                            slideAOERadius[i, n - 1].Value = Settings.Combat.AvoidanceRadius.Desecrator;
                            break;
                        case 3:
                            slideAOERadius[i, n - 1].Value = Settings.Combat.AvoidanceRadius.MoltenCore;
                            break;
                        case 4:
                            slideAOERadius[i, n - 1].Value = Settings.Combat.AvoidanceRadius.MoltenTrail;
                            break;
                        case 5:
                            slideAOERadius[i, n - 1].Value = Settings.Combat.AvoidanceRadius.PoisonTree;
                            break;
                        case 6:
                            slideAOERadius[i, n - 1].Value = Settings.Combat.AvoidanceRadius.PlagueCloud;
                            break;
                        case 7:
                            slideAOERadius[i, n - 1].Value = Settings.Combat.AvoidanceRadius.IceBalls;
                            break;
                        case 8:
                            slideAOERadius[i, n - 1].Value = Settings.Combat.AvoidanceRadius.PlagueHands;
                            break;
                        case 9:
                            slideAOERadius[i, n - 1].Value = Settings.Combat.AvoidanceRadius.BeesWasps;
                            break;
                        case 10:
                            slideAOERadius[i, n - 1].Value = Settings.Combat.AvoidanceRadius.AzmoPools;
                            break;
                        case 11:
                            slideAOERadius[i, n - 1].Value = Settings.Combat.AvoidanceRadius.AzmoBodies;
                            break;
                        case 12:
                            slideAOERadius[i, n - 1].Value = Settings.Combat.AvoidanceRadius.ShamanFire;
                            break;
                        case 13:
                            slideAOERadius[i, n - 1].Value = Settings.Combat.AvoidanceRadius.GhomGas;
                            break;
                    }
                    switch (i)
                    {
                        case 0:
                            // barbs
                            switch (n)
                            {
                                case 1:
                                    slideAOEHealth[i, n - 1].Value = Math.Round(Settings.Combat.Barbarian.AvoidArcaneHealth * 100);
                                    break;
                                case 2:
                                    slideAOEHealth[i, n - 1].Value = Math.Round(Settings.Combat.Barbarian.AvoidDesecratorHealth * 100);
                                    break;
                                case 3:
                                    slideAOEHealth[i, n - 1].Value = Math.Round(Settings.Combat.Barbarian.AvoidMoltenCoreHealth * 100);
                                    break;
                                case 4:
                                    slideAOEHealth[i, n - 1].Value = Math.Round(Settings.Combat.Barbarian.AvoidMoltenTrailHealth * 100);
                                    break;
                                case 5:
                                    slideAOEHealth[i, n - 1].Value = Math.Round(Settings.Combat.Barbarian.AvoidPoisonTreeHealth * 100);
                                    break;
                                case 6:
                                    slideAOEHealth[i, n - 1].Value = Math.Round(Settings.Combat.Barbarian.AvoidPlagueCloudHealth * 100);
                                    break;
                                case 7:
                                    slideAOEHealth[i, n - 1].Value = Math.Round(Settings.Combat.Barbarian.AvoidIceBallsHealth * 100);
                                    break;
                                case 8:
                                    slideAOEHealth[i, n - 1].Value = Math.Round(Settings.Combat.Barbarian.AvoidPlagueHandsHealth * 100);
                                    break;
                                case 9:
                                    slideAOEHealth[i, n - 1].Value = Math.Round(Settings.Combat.Barbarian.AvoidBeesWaspsHealth * 100);
                                    break;
                                case 10:
                                    slideAOEHealth[i, n - 1].Value = Math.Round(Settings.Combat.Barbarian.AvoidAzmoPoolsHealth * 100);
                                    break;
                                case 11:
                                    slideAOEHealth[i, n - 1].Value = Math.Round(Settings.Combat.Barbarian.AvoidAzmoBodiesHealth * 100);
                                    break;
                                case 12:
                                    slideAOEHealth[i, n - 1].Value = Math.Round(Settings.Combat.Barbarian.AvoidShamanFireHealth * 100);
                                    break;
                                case 13:
                                    slideAOEHealth[i, n - 1].Value = Math.Round(Settings.Combat.Barbarian.AvoidGhomGasHealth * 100);
                                    break;
                            }
                            break;
                        case 1:
                            // Monks
                            switch (n)
                            {
                                case 1:
                                    slideAOEHealth[i, n - 1].Value = Math.Round(Settings.Combat.Monk.AvoidArcaneHealth * 100);
                                    break;
                                case 2:
                                    slideAOEHealth[i, n - 1].Value = Math.Round(Settings.Combat.Monk.AvoidDesecratorHealth * 100);
                                    break;
                                case 3:
                                    slideAOEHealth[i, n - 1].Value = Math.Round(Settings.Combat.Monk.AvoidMoltenCoreHealth * 100);
                                    break;
                                case 4:
                                    slideAOEHealth[i, n - 1].Value = Math.Round(Settings.Combat.Monk.AvoidMoltenTrailHealth * 100);
                                    break;
                                case 5:
                                    slideAOEHealth[i, n - 1].Value = Math.Round(Settings.Combat.Monk.AvoidPoisonTreeHealth * 100);
                                    break;
                                case 6:
                                    slideAOEHealth[i, n - 1].Value = Math.Round(Settings.Combat.Monk.AvoidPlagueCloudHealth * 100);
                                    break;
                                case 7:
                                    slideAOEHealth[i, n - 1].Value = Math.Round(Settings.Combat.Monk.AvoidIceBallsHealth * 100);
                                    break;
                                case 8:
                                    slideAOEHealth[i, n - 1].Value = Math.Round(Settings.Combat.Monk.AvoidPlagueHandsHealth * 100);
                                    break;
                                case 9:
                                    slideAOEHealth[i, n - 1].Value = Math.Round(Settings.Combat.Monk.AvoidBeesWaspsHealth * 100);
                                    break;
                                case 10:
                                    slideAOEHealth[i, n - 1].Value = Math.Round(Settings.Combat.Monk.AvoidAzmoPoolsHealth * 100);
                                    break;
                                case 11:
                                    slideAOEHealth[i, n - 1].Value = Math.Round(Settings.Combat.Monk.AvoidAzmoBodiesHealth * 100);
                                    break;
                                case 12:
                                    slideAOEHealth[i, n - 1].Value = Math.Round(Settings.Combat.Monk.AvoidShamanFireHealth * 100);
                                    break;
                                case 13:
                                    slideAOEHealth[i, n - 1].Value = Math.Round(Settings.Combat.Monk.AvoidGhomGasHealth * 100);
                                    break;
                            }
                            break;
                        case 2:
                            // Wizards
                            switch (n)
                            {
                                case 1:
                                    slideAOEHealth[i, n - 1].Value = Math.Round(Settings.Combat.Wizard.AvoidArcaneHealth * 100);
                                    break;
                                case 2:
                                    slideAOEHealth[i, n - 1].Value = Math.Round(Settings.Combat.Wizard.AvoidDesecratorHealth * 100);
                                    break;
                                case 3:
                                    slideAOEHealth[i, n - 1].Value = Math.Round(Settings.Combat.Wizard.AvoidMoltenCoreHealth * 100);
                                    break;
                                case 4:
                                    slideAOEHealth[i, n - 1].Value = Math.Round(Settings.Combat.Wizard.AvoidMoltenTrailHealth * 100);
                                    break;
                                case 5:
                                    slideAOEHealth[i, n - 1].Value = Math.Round(Settings.Combat.Wizard.AvoidPoisonTreeHealth * 100);
                                    break;
                                case 6:
                                    slideAOEHealth[i, n - 1].Value = Math.Round(Settings.Combat.Wizard.AvoidPlagueCloudHealth * 100);
                                    break;
                                case 7:
                                    slideAOEHealth[i, n - 1].Value = Math.Round(Settings.Combat.Wizard.AvoidIceBallsHealth * 100);
                                    break;
                                case 8:
                                    slideAOEHealth[i, n - 1].Value = Math.Round(Settings.Combat.Wizard.AvoidPlagueHandsHealth * 100);
                                    break;
                                case 9:
                                    slideAOEHealth[i, n - 1].Value = Math.Round(Settings.Combat.Wizard.AvoidBeesWaspsHealth * 100);
                                    break;
                                case 10:
                                    slideAOEHealth[i, n - 1].Value = Math.Round(Settings.Combat.Wizard.AvoidAzmoPoolsHealth * 100);
                                    break;
                                case 11:
                                    slideAOEHealth[i, n - 1].Value = Math.Round(Settings.Combat.Wizard.AvoidAzmoBodiesHealth * 100);
                                    break;
                                case 12:
                                    slideAOEHealth[i, n - 1].Value = Math.Round(Settings.Combat.Wizard.AvoidShamanFireHealth * 100);
                                    break;
                                case 13:
                                    slideAOEHealth[i, n - 1].Value = Math.Round(Settings.Combat.Wizard.AvoidGhomGasHealth * 100);
                                    break;
                            }
                            break;
                        case 3:
                            // Witch Doctors
                            switch (n)
                            {
                                case 1:
                                    slideAOEHealth[i, n - 1].Value = Math.Round(Settings.Combat.WitchDoctor.AvoidArcaneHealth * 100);
                                    break;
                                case 2:
                                    slideAOEHealth[i, n - 1].Value = Math.Round(Settings.Combat.WitchDoctor.AvoidDesecratorHealth * 100);
                                    break;
                                case 3:
                                    slideAOEHealth[i, n - 1].Value = Math.Round(Settings.Combat.WitchDoctor.AvoidMoltenCoreHealth * 100);
                                    break;
                                case 4:
                                    slideAOEHealth[i, n - 1].Value = Math.Round(Settings.Combat.WitchDoctor.AvoidMoltenTrailHealth * 100);
                                    break;
                                case 5:
                                    slideAOEHealth[i, n - 1].Value = Math.Round(Settings.Combat.WitchDoctor.AvoidPoisonTreeHealth * 100);
                                    break;
                                case 6:
                                    slideAOEHealth[i, n - 1].Value = Math.Round(Settings.Combat.WitchDoctor.AvoidPlagueCloudHealth * 100);
                                    break;
                                case 7:
                                    slideAOEHealth[i, n - 1].Value = Math.Round(Settings.Combat.WitchDoctor.AvoidIceBallsHealth * 100);
                                    break;
                                case 8:
                                    slideAOEHealth[i, n - 1].Value = Math.Round(Settings.Combat.WitchDoctor.AvoidPlagueHandsHealth * 100);
                                    break;
                                case 9:
                                    slideAOEHealth[i, n - 1].Value = Math.Round(Settings.Combat.WitchDoctor.AvoidBeesWaspsHealth * 100);
                                    break;
                                case 10:
                                    slideAOEHealth[i, n - 1].Value = Math.Round(Settings.Combat.WitchDoctor.AvoidAzmoPoolsHealth * 100);
                                    break;
                                case 11:
                                    slideAOEHealth[i, n - 1].Value = Math.Round(Settings.Combat.WitchDoctor.AvoidAzmoBodiesHealth * 100);
                                    break;
                                case 12:
                                    slideAOEHealth[i, n - 1].Value = Math.Round(Settings.Combat.WitchDoctor.AvoidShamanFireHealth * 100);
                                    break;
                                case 13:
                                    slideAOEHealth[i, n - 1].Value = Math.Round(Settings.Combat.WitchDoctor.AvoidGhomGasHealth * 100);
                                    break;
                            }
                            break;
                        case 4:
                            // Demon Hunters
                            switch (n)
                            {
                                case 1:
                                    slideAOEHealth[i, n - 1].Value = Math.Round(Settings.Combat.DemonHunter.AvoidArcaneHealth * 100);
                                    break;
                                case 2:
                                    slideAOEHealth[i, n - 1].Value = Math.Round(Settings.Combat.DemonHunter.AvoidDesecratorHealth * 100);
                                    break;
                                case 3:
                                    slideAOEHealth[i, n - 1].Value = Math.Round(Settings.Combat.DemonHunter.AvoidMoltenCoreHealth * 100);
                                    break;
                                case 4:
                                    slideAOEHealth[i, n - 1].Value = Math.Round(Settings.Combat.DemonHunter.AvoidMoltenTrailHealth * 100);
                                    break;
                                case 5:
                                    slideAOEHealth[i, n - 1].Value = Math.Round(Settings.Combat.DemonHunter.AvoidPoisonTreeHealth * 100);
                                    break;
                                case 6:
                                    slideAOEHealth[i, n - 1].Value = Math.Round(Settings.Combat.DemonHunter.AvoidPlagueCloudHealth * 100);
                                    break;
                                case 7:
                                    slideAOEHealth[i, n - 1].Value = Math.Round(Settings.Combat.DemonHunter.AvoidIceBallsHealth * 100);
                                    break;
                                case 8:
                                    slideAOEHealth[i, n - 1].Value = Math.Round(Settings.Combat.DemonHunter.AvoidPlagueHandsHealth * 100);
                                    break;
                                case 9:
                                    slideAOEHealth[i, n - 1].Value = Math.Round(Settings.Combat.DemonHunter.AvoidBeesWaspsHealth * 100);
                                    break;
                                case 10:
                                    slideAOEHealth[i, n - 1].Value = Math.Round(Settings.Combat.DemonHunter.AvoidAzmoPoolsHealth * 100);
                                    break;
                                case 11:
                                    slideAOEHealth[i, n - 1].Value = Math.Round(Settings.Combat.DemonHunter.AvoidAzmoBodiesHealth * 100);
                                    break;
                                case 12:
                                    slideAOEHealth[i, n - 1].Value = Math.Round(Settings.Combat.DemonHunter.AvoidShamanFireHealth * 100);
                                    break;
                                case 13:
                                    slideAOEHealth[i, n - 1].Value = Math.Round(Settings.Combat.DemonHunter.AvoidGhomGasHealth * 100);
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
            comboLegendary.SelectedIndex = comboLegendary.Items.Cast<ComboBoxItem>().TakeWhile(cbi => !(cbi.Tag).Equals(Settings.Loot.Pickup.LegendaryLevel.ToString())).Count();
            comboWB.SelectedIndex = comboWB.Items.Cast<ComboBoxItem>().TakeWhile(cbi => !(cbi.Tag).Equals(Settings.Loot.Pickup.WeaponBlueLevel.ToString())).Count();
            comboWY.SelectedIndex = comboWY.Items.Cast<ComboBoxItem>().TakeWhile(cbi => !(cbi.Tag).Equals(Settings.Loot.Pickup.WeaponYellowLevel.ToString())).Count();
            comboAB.SelectedIndex = comboAB.Items.Cast<ComboBoxItem>().TakeWhile(cbi => !(cbi.Tag).Equals(Settings.Loot.Pickup.ArmorBlueLevel.ToString())).Count();
            comboAY.SelectedIndex = comboAY.Items.Cast<ComboBoxItem>().TakeWhile(cbi => !(cbi.Tag).Equals(Settings.Loot.Pickup.ArmorYellowLevel.ToString())).Count();
            comboJB.SelectedIndex = comboJB.Items.Cast<ComboBoxItem>().TakeWhile(cbi => !(cbi.Tag).Equals(Settings.Loot.Pickup.JewelryBlueLevel.ToString())).Count();
            comboJY.SelectedIndex = comboJY.Items.Cast<ComboBoxItem>().TakeWhile(cbi => !(cbi.Tag).Equals(Settings.Loot.Pickup.JewelryYellowLevel.ToString())).Count();
            comboGems.SelectedIndex = comboGems.Items.Cast<ComboBoxItem>().TakeWhile(cbi => !(cbi.Tag).Equals(Settings.Loot.Pickup.GemLevel.ToString())).Count();
            comboMisc.SelectedIndex = comboMisc.Items.Cast<ComboBoxItem>().TakeWhile(cbi => !(cbi.Tag).Equals(Settings.Loot.Pickup.MiscItemLevel.ToString())).Count();
            comboPotions.SelectedIndex = comboPotions.Items.Cast<ComboBoxItem>().TakeWhile(cbi => !(cbi.Tag).Equals(((int)Settings.Loot.Pickup.PotionMode).ToString())).Count();
            comboPotionLevel.SelectedIndex = comboPotionLevel.Items.Cast<ComboBoxItem>().TakeWhile(cbi => !(cbi.Tag).Equals(Settings.Loot.Pickup.Potionlevel.ToString())).Count();
            checkCraftTomes.IsChecked = Settings.Loot.Pickup.CraftTomes;
            checkDesigns.IsChecked = Settings.Loot.Pickup.DesignPlan;
            checkFollower.IsChecked = Settings.Loot.Pickup.FollowerItem;
            checkGemEmerald.IsChecked = (Settings.Loot.Pickup.GemType & TrinityGemType.Emerald) == TrinityGemType.Emerald;
            checkGemAmethyst.IsChecked = (Settings.Loot.Pickup.GemType & TrinityGemType.Amethys) == TrinityGemType.Amethys;
            checkGemTopaz.IsChecked = (Settings.Loot.Pickup.GemType & TrinityGemType.Topaz) == TrinityGemType.Topaz;
            checkGemRuby.IsChecked = (Settings.Loot.Pickup.GemType & TrinityGemType.Ruby) == TrinityGemType.Ruby;
            if (Settings.Loot.TownRun.TrashMode == TrashMode.Salvaging)
            {
                btnSalvage.IsChecked = true;
                btnSell.IsChecked = false;
            }
            else
            {
                btnSell.IsChecked = true;
                btnSalvage.IsChecked = false;
            }
            if (Settings.Loot.ItemFilterMode != ItemFilterMode.DemonBuddy)
            {
                btnRulesGiles.IsChecked = true;
                btnRulesCustom.IsChecked = false;
            }
            else
            {
                btnRulesCustom.IsChecked = true;
                btnRulesGiles.IsChecked = false;
            }
            slideWeapon.Value = Settings.Loot.TownRun.WeaponScore;
            WeaponText.Text = slideWeapon.Value.ToString();
            slideArmor.Value = Settings.Loot.TownRun.ArmorScore;
            ArmorText.Text = slideArmor.Value.ToString();
            slideJewelry.Value = Settings.Loot.TownRun.JewelryScore;
            JewelryText.Text = slideJewelry.Value.ToString();
            slideNotifyWeapon.Value = Settings.Notification.WeaponScore;
            WeaponNotifyText.Text = slideNotifyWeapon.Value.ToString();
            slideNotifyArmor.Value = Settings.Notification.ArmorScore;
            ArmorNotifyText.Text = slideNotifyArmor.Value.ToString();
            slideNotifyJewelry.Value = Settings.Notification.JewelryScore;
            JewelryNotifyText.Text = slideNotifyJewelry.Value.ToString();
            slideGoldAmount.Value = Settings.Loot.Pickup.MinimumGoldStack;
            textGoldAmount.Text = Settings.Loot.Pickup.MinimumGoldStack.ToString();
            slideTriggerRange.Value = Settings.Combat.Misc.NonEliteRange;
            textTriggerRange.Text = Settings.Combat.Misc.NonEliteRange.ToString();
            slideLootDelay.Value = Settings.Combat.Misc.DelayAfterKill;
            textLootDelay.Text = Settings.Combat.Misc.DelayAfterKill.ToString();
            slideVaultDelay.Value = Settings.Combat.DemonHunter.VaultMovementDelay;
            textVaultDelay.Text = Settings.Combat.DemonHunter.VaultMovementDelay.ToString();
            slideKite0.Value = Settings.Combat.Barbarian.KiteLimit;
            textKite0.Text = Settings.Combat.Barbarian.KiteLimit.ToString();
            slideKite2.Value = Settings.Combat.Wizard.KiteLimit;
            textKite2.Text = Settings.Combat.Wizard.KiteLimit.ToString();
            slideKite3.Value = Settings.Combat.WitchDoctor.KiteLimit;
            textKite3.Text = Settings.Combat.WitchDoctor.KiteLimit.ToString();
            slideKite4.Value = Settings.Combat.DemonHunter.KiteLimit;
            textKite4.Text = Settings.Combat.DemonHunter.KiteLimit.ToString();
            switch(Settings.Combat.Misc.GoblinPriority )
            {
                case GoblinPriority.Ignore:
                    checkTreasureNormal.IsChecked = false;
                    checkTreasurePrioritize.IsChecked = false;
                    checkTreasureKamikaze.IsChecked = false;
                    checkTreasureIgnore.IsChecked = true;
                    break;
                case GoblinPriority.Normal:
                    checkTreasureIgnore.IsChecked = false;
                    checkTreasurePrioritize.IsChecked = false;
                    checkTreasureKamikaze.IsChecked = false;
                    checkTreasureNormal.IsChecked = true;
                    break;
                case GoblinPriority.Prioritize:
                    checkTreasureIgnore.IsChecked = false;
                    checkTreasureNormal.IsChecked = false;
                    checkTreasureKamikaze.IsChecked = false;
                    checkTreasurePrioritize.IsChecked = true;
                    break;
                case GoblinPriority.Kamikaze:
                    checkTreasureIgnore.IsChecked = false;
                    checkTreasureNormal.IsChecked = false;
                    checkTreasurePrioritize.IsChecked = false;
                    checkTreasureKamikaze.IsChecked = true;
                    break;
            }
            if (!Settings.WorldObject.UseShrine)
            {
                checkIgnoreNone.IsChecked = false;
                checkIgnoreAll.IsChecked = true;
            }
            else
            {
                checkIgnoreAll.IsChecked = false;
                checkIgnoreNone.IsChecked = true;
            }
            checkBacktracking.IsChecked = Settings.Combat.Misc.AllowBacktracking;
            checkCritical.IsChecked = Settings.Combat.Wizard.CriticalMass;
            checkGrave.IsChecked = Settings.Combat.WitchDoctor.GraveInjustice;
            checkAvoidance.IsChecked = Settings.Combat.Misc.AvoidAOE;
            checkGlobes.IsChecked = Settings.Combat.Misc.CollectHealthGlobe;
            slideContainerRange.Value = Settings.WorldObject.ContainerOpenRange;
            textContainerRange.Text = Settings.WorldObject.ContainerOpenRange.ToString();
            slideDestructibleRange.Value = Settings.WorldObject.DestructibleRange;
            textDestructibleRange.Text = Settings.WorldObject.DestructibleRange.ToString();
            checkIgnoreCorpses.IsChecked = Settings.WorldObject.IgnoreNonBlocking;
            checkMovementAbilities.IsChecked = Settings.Combat.Misc.AllowOOCMovement;
            textTPS.Text = Settings.Advanced.TPSLimit.ToString();
            slideTPS.Value = Settings.Advanced.TPSLimit;
            checkTPS.IsChecked = Settings.Advanced.TPSEnabled;
            checkLogStucks.IsChecked = Settings.Advanced.LogStuckLocation;
            checkProwl.IsChecked = Settings.Notification.IPhoneEnabled;
            textProwlKey.Text = Settings.Notification.IPhoneKey;
            checkEmail.IsChecked = Settings.Notification.MailEnabled;
            checkLegendaryNotify.IsChecked = Settings.Notification.LegendaryScoring;
            txtEmailAddress.Text = Settings.Notification.EmailAddress;
            txtEmailPassword.Text = Settings.Notification.EmailPassword;
            txtBotName.Text = Settings.Notification.BotName; 
            checkAndroid.IsChecked = Settings.Notification.AndroidEnabled;
            textAndroidKey.Text = Settings.Notification.AndroidKey;
            checkUnstucker.IsChecked = Settings.Advanced.UnstuckerEnabled;
            checkProfileReload.IsChecked = Settings.Advanced.AllowRestartGame;
            checkExtendedRange.IsChecked = Settings.Combat.Misc.ExtendedTrashKill;
            checkSelectiveWW.IsChecked = Settings.Combat.Barbarian.SelectiveWirlwind;
            checkWrath90.IsChecked = Settings.Combat.Barbarian.BoonBulKathosPassive;
            checkWaitWrath.IsChecked = Settings.Combat.Barbarian.WaitWOTB;
            checkKiteArchonOnly.IsChecked = Settings.Combat.Wizard.OnlyKiteInArchon;
            checkWaitArchonAzmo.IsChecked = Settings.Combat.Wizard.WaitArchon;
            checkGoblinWrath.IsChecked = Settings.Combat.Barbarian.UseWOTBGoblin;
            checkFuryDumpWrath.IsChecked = Settings.Combat.Barbarian.FuryDumpWOTB;
            checkFuryDumpAlways.IsChecked = Settings.Combat.Barbarian.FuryDumpAlways;
            checkDebugInfo.IsChecked = Settings.Advanced.DebugInStatusBar;
            checkMonkInna.IsChecked = Settings.Combat.Monk.HasInnaSet;
            slidePot0.Value = Math.Floor(Settings.Combat.Barbarian.PotionLevel * 100);
            slidePot1.Value = Math.Floor(Settings.Combat.Monk.PotionLevel * 100);
            slidePot2.Value = Math.Floor(Settings.Combat.Wizard.PotionLevel * 100);
            slidePot3.Value = Math.Floor(Settings.Combat.WitchDoctor.PotionLevel * 100);
            slidePot4.Value = Math.Floor(Settings.Combat.DemonHunter.PotionLevel * 100);
            textPot0.Text = Math.Floor(Settings.Combat.Barbarian.PotionLevel * 100).ToString();
            textPot1.Text = Math.Floor(Settings.Combat.Monk.PotionLevel * 100).ToString();
            textPot2.Text = Math.Floor(Settings.Combat.Wizard.PotionLevel * 100).ToString();
            textPot3.Text = Math.Floor(Settings.Combat.WitchDoctor.PotionLevel * 100).ToString();
            textPot4.Text = Math.Floor(Settings.Combat.DemonHunter.PotionLevel * 100).ToString();
            slideGlobe0.Value = Math.Floor(Settings.Combat.Barbarian.HealthGlobeLevel * 100);
            slideGlobe1.Value = Math.Floor(Settings.Combat.Monk.HealthGlobeLevel * 100);
            slideGlobe2.Value = Math.Floor(Settings.Combat.Wizard.HealthGlobeLevel * 100);
            slideGlobe3.Value = Math.Floor(Settings.Combat.WitchDoctor.HealthGlobeLevel * 100);
            slideGlobe4.Value = Math.Floor(Settings.Combat.DemonHunter.HealthGlobeLevel * 100);
            textGlobe0.Text = Math.Floor(Settings.Combat.Barbarian.HealthGlobeLevel * 100).ToString();
            textGlobe1.Text = Math.Floor(Settings.Combat.Monk.HealthGlobeLevel * 100).ToString();
            textGlobe2.Text = Math.Floor(Settings.Combat.Wizard.HealthGlobeLevel * 100).ToString();
            textGlobe3.Text = Math.Floor(Settings.Combat.WitchDoctor.HealthGlobeLevel * 100).ToString();
            textGlobe4.Text = Math.Floor(Settings.Combat.DemonHunter.HealthGlobeLevel * 100).ToString();
            bSuppressEventChanges = false;
        }
        #endregion
        // * END OF CONFIG WINDOW REGION
        // Primary/"Default" functions, like logs, initialize, DB event handling etc.
    }
}