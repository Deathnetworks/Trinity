using GilesTrinity.DbProvider;
using GilesTrinity.Settings;
using GilesTrinity.Settings.Combat;
using GilesTrinity.Settings.Loot;
using GilesTrinity.UI;
using System.Diagnostics;
using System.Windows.Input;
using GilesTrinity.Technicals;
using System;
using System.Windows.Forms;
using Zeta.CommonBot;

namespace GilesTrinity.UIComponents
{
    /// <summary>
    /// ViewModel injected to Configuration Window 
    /// </summary>
    public class ConfigViewModel
    {
        private TrinitySetting _Model;
        private TrinitySetting _OriginalModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigViewModel" /> class.
        /// </summary>
        /// <param name="model">The model.</param>
        public ConfigViewModel(TrinitySetting model)
        {
            _OriginalModel = model;
            _Model = new TrinitySetting();
            _OriginalModel.CopyTo(_Model);
            InitializeResetCommand();
            SaveCommand = new RelayCommand(
                                    (parameter) =>
                                    {
                                        _Model.CopyTo(_OriginalModel);
                                        _OriginalModel.Save();
                                        GilesTrinity.SetBotTPS();
                                        GilesTrinity.SetUnstuckProvider();
                                        GilesTrinity.SetItemManagerProvider();
                                        UILoader.CloseWindow();
                                    });
            TestScoreCommand = new RelayCommand(
                                    (parameter) =>
                                    {
                                        GilesTrinity.TestScoring();
                                        UILoader.CloseWindow();
                                    });
            OrderStashCommand = new RelayCommand(
                                    (parameter) =>
                                    {
                                        global::GilesTrinity.TownRun.SortStash();
                                        UILoader.CloseWindow();
                                    });
            HelpLinkCommand = new RelayCommand(
                                    (parameter) =>
                                    {
                                        string link = parameter as string;
                                        if (!string.IsNullOrWhiteSpace(link))
                                        {
                                            Process.Start(link);
                                        }
                                    });
            ConfigureLootToHunting = new RelayCommand(
                                    (parameter) =>
                                    {
                                        ConfigHuntingLoot();
                                    });
            ConfigureLootToQuesting = new RelayCommand(
                                    (parameter) =>
                                    {
                                        ConfigQuestingLoot();
                                    });
            LoadItemRuleSetCommand = new RelayCommand(
                                    (parameter) =>
                                    {
                                        LoadItemRulesPath();
                                    });

        }

        /// <summary>
        /// Gets the HelpLink command.
        /// </summary>
        /// <value>The save command.</value>
        public ICommand LoadItemRuleSetCommand
        {
            get;
            private set;
        }
        /// <summary>
        /// Gets the HelpLink command.
        /// </summary>
        /// <value>The save command.</value>
        public ICommand HelpLinkCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the HelpLink command.
        /// </summary>
        /// <value>The save command.</value>
        public ICommand ConfigureLootToQuesting
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the HelpLink command.
        /// </summary>
        /// <value>The save command.</value>
        public ICommand ConfigureLootToHunting
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the test score command.
        /// </summary>
        /// <value>The save command.</value>
        public ICommand TestScoreCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the order stash command.
        /// </summary>
        /// <value>The save command.</value>
        public ICommand OrderStashCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the save command.
        /// </summary>
        /// <value>The save command.</value>
        public ICommand SaveCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the reset command for Misc Tab.
        /// </summary>
        /// <value>The reset command for Misc Tab.</value>
        public ICommand ResetMiscCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the reset command for Barbarian Tab.
        /// </summary>
        /// <value>The reset command for Barbarian Tab.</value>
        public ICommand ResetBarbCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the reset command for Monk Tab.
        /// </summary>
        /// <value>The reset command for Monk Tab.</value>
        public ICommand ResetMonkCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the reset command for Wizard Tab.
        /// </summary>
        /// <value>The reset command for Wizard Tab.</value>
        public ICommand ResetWizardCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the reset command for Witch Doctor Tab.
        /// </summary>
        /// <value>The reset command for Witch Doctor Tab.</value>
        public ICommand ResetWitchDoctorCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the reset command for Demon Hunter Tab.
        /// </summary>
        /// <value>The reset command for Demon Hunter Tab.</value>
        public ICommand ResetDemonHunterCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the reset command for World Object Tab.
        /// </summary>
        /// <value>The reset command for World Object Tab.</value>
        public ICommand ResetWorldObjetCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the reset command for Town Run Tab.
        /// </summary>
        /// <value>The reset command for Town Run Tab.</value>
        public ICommand ResetTownRunCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the reset command for Items Tab.
        /// </summary>
        /// <value>The reset command for Items Tab.</value>
        public ICommand ResetItemCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the reset command for Items Tab.
        /// </summary>
        /// <value>The reset command for Items Tab.</value>
        public ICommand ResetItemRulesCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Reloads Script Rules
        /// </summary>
        public ICommand ReloadScriptRulesCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the reset command for Advanced Tab.
        /// </summary>
        /// <value>The reset command for Advanced Tab.</value>
        public ICommand ResetAdvancedCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the reset command for Mobile Tab.
        /// </summary>
        /// <value>The reset command for Mobile Tab.</value>
        public ICommand ResetNotificationCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the reset command for Logs Tab.
        /// </summary>
        /// <value>The reset command for Logs Tab.</value>
        public ICommand ResetLogCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the reset command for all settings.
        /// </summary>
        /// <value>The reset command for all settings.</value>
        public ICommand ResetAllCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the Misc Configuration Model.
        /// </summary>
        /// <value>The Misc Configuration Model.</value>
        public MiscCombatSetting Misc
        {
            get
            {
                return _Model.Combat.Misc;
            }
        }

        /// <summary>
        /// Gets the Advanced Configuration Model.
        /// </summary>
        /// <value>The Advanced Configuration Model.</value>
        public AdvancedSetting Advanced
        {
            get
            {
                return _Model.Advanced;
            }
        }

        /// <summary>
        /// Gets the Avoidance Configuration Model.
        /// </summary>
        /// <value>The Avoidance Configuration Model.</value>
        public AvoidanceRadiusSetting Avoid
        {
            get
            {
                return _Model.Combat.AvoidanceRadius;
            }
        }

        /// <summary>
        /// Gets the Barbarian Configuration Model.
        /// </summary>
        /// <value>The Barbarian Configuration Model.</value>
        public BarbarianSetting Barb
        {
            get
            {
                return _Model.Combat.Barbarian;
            }
        }

        /// <summary>
        /// Gets the Demon Hunter Configuration Model.
        /// </summary>
        /// <value>The Demon Hunter Configuration Model.</value>
        public DemonHunterSetting DH
        {
            get
            {
                return _Model.Combat.DemonHunter;
            }
        }

        /// <summary>
        /// Gets the Monk Configuration Model.
        /// </summary>
        /// <value>The Monk Configuration Model.</value>
        public MonkSetting Monk
        {
            get
            {
                return _Model.Combat.Monk;
            }
        }

        /// <summary>
        /// Gets the Witch Doctor Configuration Model.
        /// </summary>
        /// <value>The Witch Doctor Configuration Model.</value>
        public WitchDoctorSetting WD
        {
            get
            {
                return _Model.Combat.WitchDoctor;
            }
        }

        /// <summary>
        /// Gets the Wizard Configuration Model.
        /// </summary>
        /// <value>The Wizard Configuration Model.</value>
        public WizardSetting Wiz
        {
            get
            {
                return _Model.Combat.Wizard;
            }
        }

        /// <summary>
        /// Gets the World Object Configuration Model.
        /// </summary>
        /// <value>The World Object Configuration Model.</value>
        public WorldObjectSetting WorldObject
        {
            get
            {
                return _Model.WorldObject;
            }
        }

        /// <summary>Gets the TownRun Configuration Model.</summary>
        /// <value>The TownRun Configuration Model.</value>
        public TownRunSetting TownRun
        {
            get
            {
                return _Model.Loot.TownRun;
            }
        }

        /// <summary>
        /// Gets the Pickup Configuration Model.
        /// </summary>
        /// <value>The Pickup Configuration Model.</value>
        public PickupSetting Pickup
        {
            get
            {
                return _Model.Loot.Pickup;
            }
        }

        /// <summary>
        /// Gets the Pickup Configuration Model.
        /// </summary>
        /// <value>The Pickup Configuration Model.</value>
        public ItemSetting Loot
        {
            get
            {
                return _Model.Loot;
            }
        }
        /// <summary>
        /// Gets the Pickup Configuration Model.
        /// </summary>
        /// <value>The Pickup Configuration Model.</value>
        public ItemRuleSetting ItemRules
        {
            get
            {
                return _Model.Loot.ItemRules;
            }
        }

        /// <summary>
        /// Gets the Pickup Configuration Model.
        /// </summary>
        /// <value>The Pickup Configuration Model.</value>
        public NotificationSetting Notification
        {
            get
            {
                return _Model.Notification;
            }
        }

        /// <summary>
        /// Initializes the Reset commands.
        /// </summary>
        private void InitializeResetCommand()
        {
            ResetMiscCommand = new RelayCommand(
                                    (parameter) =>
                                    {
                                        _Model.Combat.Misc.Reset();
                                    });
            ResetBarbCommand = new RelayCommand(
                                    (parameter) =>
                                    {
                                        _Model.Combat.Barbarian.Reset();
                                        _Model.Combat.AvoidanceRadius.Reset();
                                    });
            ResetMonkCommand = new RelayCommand(
                                    (parameter) =>
                                    {
                                        _Model.Combat.Monk.Reset();
                                        _Model.Combat.AvoidanceRadius.Reset();
                                    });
            ResetWizardCommand = new RelayCommand(
                                    (parameter) =>
                                    {
                                        _Model.Combat.Wizard.Reset();
                                        _Model.Combat.AvoidanceRadius.Reset();
                                    });
            ResetWitchDoctorCommand = new RelayCommand(
                                    (parameter) =>
                                    {
                                        _Model.Combat.WitchDoctor.Reset();
                                        _Model.Combat.AvoidanceRadius.Reset();
                                    });
            ResetDemonHunterCommand = new RelayCommand(
                                    (parameter) =>
                                    {
                                        _Model.Combat.DemonHunter.Reset();
                                        _Model.Combat.AvoidanceRadius.Reset();
                                    });
            ResetWorldObjetCommand = new RelayCommand(
                                    (parameter) =>
                                    {
                                        _Model.WorldObject.Reset();
                                    });
            ResetItemCommand = new RelayCommand(
                                    (parameter) =>
                                    {
                                        _Model.Loot.Pickup.Reset();
                                    });
            ResetItemRulesCommand = new RelayCommand(
                                    (parameter) =>
                                    {
                                        _Model.Loot.ItemRules.Reset();
                                    });
            ReloadScriptRulesCommand = new RelayCommand(
                                    (parameter) =>
                                    {
                                        BotMain.PauseWhile(GilesTrinity.StashRule.reloadFromUI);
                                    }
                                    );
            ResetTownRunCommand = new RelayCommand(
                                    (parameter) =>
                                    {
                                        _Model.Loot.TownRun.Reset();
                                    });
            ResetAdvancedCommand = new RelayCommand(
                                    (parameter) =>
                                    {
                                        _Model.Advanced.Reset();
                                    });
            ResetNotificationCommand = new RelayCommand(
                                    (parameter) =>
                                    {
                                        _Model.Notification.Reset();
                                    });

            ResetAllCommand = new RelayCommand(
                                    (parameter) =>
                                    {
                                        _Model.Reset();
                                    });
        }

        private void ConfigQuestingLoot()
        {
            _Model.Loot.Pickup.ArmorBlueLevel = 1;
            _Model.Loot.Pickup.ArmorYellowLevel = 1;
            _Model.Loot.Pickup.WeaponBlueLevel = 1;
            _Model.Loot.Pickup.WeaponYellowLevel = 1;
            _Model.Loot.Pickup.JewelryBlueLevel = 1;
            _Model.Loot.Pickup.JewelryYellowLevel = 1;
            _Model.Loot.Pickup.LegendaryLevel = 1;
            _Model.Loot.Pickup.GemLevel = 14;
            _Model.Loot.Pickup.GemType = TrinityGemType.All;
            _Model.Loot.Pickup.FollowerItem = true;
            _Model.Loot.Pickup.MinimumGoldStack = 0;
            _Model.Loot.Pickup.MiscItemLevel = 1;
            _Model.Loot.Pickup.PotionLevel = 1;
            _Model.Loot.Pickup.PotionMode = PotionMode.Cap;
            _Model.Loot.Pickup.Plans = true;
            _Model.Loot.Pickup.CraftTomes = true;
        }

        private void ConfigHuntingLoot()
        {
            _Model.Loot.Pickup.ArmorBlueLevel = 0;
            _Model.Loot.Pickup.ArmorYellowLevel = 63;
            _Model.Loot.Pickup.WeaponBlueLevel = 0;
            _Model.Loot.Pickup.WeaponYellowLevel = 63;
            _Model.Loot.Pickup.JewelryBlueLevel = 0;
            _Model.Loot.Pickup.JewelryYellowLevel = 58;
            _Model.Loot.Pickup.LegendaryLevel = 55;
            _Model.Loot.Pickup.GemLevel = 60;
            _Model.Loot.Pickup.GemType = TrinityGemType.All;
            _Model.Loot.Pickup.FollowerItem = true;
            _Model.Loot.Pickup.MinimumGoldStack = 300;
            _Model.Loot.Pickup.MiscItemLevel = 60;
            _Model.Loot.Pickup.PotionLevel = 60;
            _Model.Loot.Pickup.PotionMode = PotionMode.Cap;
            _Model.Loot.Pickup.Plans = true;
            _Model.Loot.Pickup.CraftTomes = true;
        }

        private void LoadItemRulesPath()
        {

            var folderDialog = new FolderBrowserDialog();

            folderDialog.ShowNewFolderButton = false;
            folderDialog.SelectedPath = FileManager.ItemRulePath;

            DialogResult result = folderDialog.ShowDialog();


            // Get the selected file name and display in a TextBox 
            if (result == DialogResult.OK)
            {
                // Open document 
                string directory = folderDialog.SelectedPath;

                if (directory != FileManager.ItemRulePath)
                {
                    ItemRules.ItemRuleSetPath = directory;

                    DbHelper.Log(TrinityLogLevel.Normal, LogCategory.Configuration, "Loaded ItemRule Set {0}", ItemRules.ItemRuleSetPath);
                }
            }
        }
    }
}
