using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.IO;
using System.Collections;
using Zeta.Internals.Actors;
using Zeta.Common;
using GilesTrinity.ItemRules.Core;
using GilesTrinity.Technicals;

namespace GilesTrinity.ItemRules
{
    #region Interpreter

    /// <summary>
    /// +---------------------------------------------------------------------------+
    /// | _______ __                     ______         __                   ______ 
    /// ||_     _|  |_.-----.--------.  |   __ \.--.--.|  |.-----.-----.    |__    |
    /// | _|   |_|   _|  -__|        |  |      <|  |  ||  ||  -__|__ --|    |    __|
    /// ||_______|____|_____|__|__|__|  |___|__||_____||__||_____|_____|    |______|
    /// |+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++ 
    /// +---------------------------------------------------------------------------+
    /// | - Created by darkfriend77
    /// +---------------------------------------------------------------------------+
    /// </summary>
    public class Interpreter
    {
        // enumerations
        public enum LogType
        {
            LOG,
            TRASH,
            DEBUG,
            ERROR
        };
        public enum InterpreterAction
        {
            PICKUP,
            IGNORE,
            KEEP,
            TRASH,
            SCORE,
            NULL
        };

        // final variables
        readonly string version = "2.0.0.5";
        readonly string customPath = @"Plugins\GilesTrinity\ItemRules\Rules\";
        readonly string logPath = @"Plugins\GilesTrinity\ItemRules\Log\";
        readonly string configFile = "config.dis";
        readonly string pickupFile = "pickup.dis";
        readonly string assign = "->", lineBreak = "\r\n";
        readonly string filePattern = @"\[FILE\][ ]*==[ ]*([A-Za-z]+.dis)";
        readonly string flagPattern = @"\[([A-Z]+)\]==([Tt][Rr][Uu][Ee]|[Ff][Aa][Ll][Ss][Ee])";

        // variables
        string startTimestamp;

        // objects
        ArrayList ruleSet, pickUpRuleSet;
        TextWriter log;
        Scanner scanner;
        Parser parser;
        //TextHighlighter highlighter;

        // flags
        bool debugFlag = false;

        // dictonary for the item
        public static Dictionary<string, object> itemDic;

        //static void Main()
        //{
        //    Interpreter interpreter = new Interpreter();
        //    interpreter.checkItem((ACDItem)null, false);
        //}

        /// <summary>
        /// 
        /// </summary>
        public Interpreter()
        {
            // initialize parser objects
            scanner = new Scanner();
            parser = new Parser(scanner);
            //highlighter = new TextHighlighter(richTextBox, scanner, parser);

            // read configuration file and item files now
            readConfiguration();
            DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, " _______________________________________");
            DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, " ___-|: Darkfriend's Item Rules 2 :|-___");
            DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, " ___________________Rel.-v" + version + "_______");
        }

        /// <summary>
        /// 
        /// </summary>
        public void readConfiguration()
        {
            startTimestamp = DateTime.Now.ToString("ddMMyyyyHHmmss");
            StreamReader streamReader = new StreamReader(customPath + configFile);

            // initialize or reset ruleSet array
            ruleSet = new ArrayList();
            pickUpRuleSet = new ArrayList();
            List<string> itemFileNames = new List<string>();

            string str;
            Match match1, match2;
            while ((str = streamReader.ReadLine()) != null)
            {
                str = str.Split(new string[] { "//" }, StringSplitOptions.None)[0].Replace(" ", "").Replace("\t", "");
                if (str.Length == 0) continue;

                // match files

                match1 = Regex.Match(str, filePattern);

                // match flags
                match2 = Regex.Match(str, flagPattern);

                if (match1.Success && File.Exists(customPath + match1.Groups[1].Value))

                    itemFileNames.Add(match1.Groups[1].Value);

                else if (match2.Success)
                {
                    if (match2.Groups[1].Value.Contains("DEBUG"))
                        debugFlag = Boolean.Parse(match2.Groups[2].Value);
                    if (debugFlag) logOut("debug flag ... " + debugFlag, LogType.DEBUG);
                }

            }

            // parse pickup file
            DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, " ... reading: " + pickupFile);
            if (debugFlag) logOut("... reading file: " + pickupFile, LogType.DEBUG);
            streamReader = new StreamReader(customPath + pickupFile);
            while ((str = streamReader.ReadLine()) != null)
            {
                str = str.Split(new string[] { "//" }, StringSplitOptions.None)[0].Replace(" ", "").Replace("\t", "");

                if (str.Length == 0) continue;
                pickUpRuleSet.Add(str);
                if (debugFlag) logOut(pickUpRuleSet.Count + ":" + str, LogType.DEBUG);
            }
            DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, " ... loaded: " + pickUpRuleSet.Count + " item pick up rules");

            // parse all item files
            foreach (string itemFileName in itemFileNames)
            {
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, " ... reading: " + itemFileName);
                if (debugFlag) logOut("... reading file: " + itemFileName, LogType.DEBUG);
                streamReader = new StreamReader(customPath + itemFileName);
                while ((str = streamReader.ReadLine()) != null)
                {
                    str = str.Split(new string[] { "//" }, StringSplitOptions.None)[0].Replace(" ", "").Replace("\t", "");

                    if (str.Length == 0) continue;
                    ruleSet.Add(str);
                    if (debugFlag) logOut(ruleSet.Count + ":" + str, LogType.DEBUG);
                }
            }
            DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, " ... loaded: " + ruleSet.Count + " item rules");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public InterpreterAction checkItem(ACDItem item, bool pickUp)
        {
            fillDic(item);

            if (debugFlag) logOut(getFullItem(item), LogType.DEBUG);

            InterpreterAction action = InterpreterAction.NULL;
            string validRule = "";

            ArrayList rules;
            if (pickUp) rules = pickUpRuleSet;
            else rules = ruleSet;

            foreach (string str in rules)
            {
                ParseErrors parseErrors = null;

                // default configuration for positive rules is pickup and keep
                InterpreterAction tempAction;
                if (pickUp) tempAction = InterpreterAction.KEEP;
                else tempAction = InterpreterAction.KEEP;

                string[] strings = str.Split(new string[] { assign }, StringSplitOptions.None);
                if (strings.Count() > 1)
                {
                    tempAction = getInterpreterAction(strings[1]);
                }
                try
                {
                    if (evaluate(strings[0], out parseErrors))
                    {
                        validRule = str;
                        action = tempAction;
                        if (debugFlag && parseErrors.Count > 0)
                            logOut("DEBUG: Have errors with out a catch!"
                                + lineBreak + "last use rule: " + str
                                + lineBreak + getParseErrors(parseErrors)
                                + lineBreak + getFullItem(item)
                                + lineBreak, LogType.ERROR);
                        break;
                    }
                }
                catch (Exception e)
                {
                    logOut("ERROR: " + e.Message
                        + lineBreak + "last use rule: " + str
                        + lineBreak + getParseErrors(parseErrors)
                        + lineBreak + getFullItem(item)
                        + lineBreak, LogType.ERROR);
                }
            }

            if (action == InterpreterAction.TRASH && (debugFlag || item.ItemQualityLevel == ItemQuality.Legendary))
                logOut(getFullItem(item) + lineBreak, LogType.TRASH);
            else if (action == InterpreterAction.KEEP || (!pickUp || debugFlag))
                if (pickUp)
                    logOut("PICKUP: " + getItemTag(item)
                        + lineBreak + validRule + " [ACTION = " + action + "]"
                        + lineBreak, LogType.DEBUG);
                else
                    logOut(getFullItem(item)
                        + lineBreak + validRule + " [ACTION = " + action + "]"
                        + lineBreak, LogType.LOG);

            return action;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <param name="item"></param>
        /// <param name="parseErrors"></param>
        /// <returns></returns>
        private bool evaluate(string str, out ParseErrors parseErrors)
        {
            bool result = false;
            ItemRules.Core.ParseTree tree = parser.Parse(str);
            parseErrors = tree.Errors;
            object obj = tree.Eval(null);

            if (!Boolean.TryParse(obj.ToString(), out result))
                tree.Errors.Add(new ParseError("TryParse Boolean failed!", 101, 0, 0, 0, 0));

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private InterpreterAction getInterpreterAction(string str)
        {
            foreach (InterpreterAction action in Enum.GetValues(typeof(InterpreterAction)))
                if (str.IndexOf(action.ToString()) != -1)
                    return action;
            return InterpreterAction.NULL;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <param name="logType"></param>
        public void logOut(string str, LogType logType)
        {
            // create directory if it doesn't exists
            if (!Directory.Exists(logPath))
                Directory.CreateDirectory(logPath);

            log = new StreamWriter(logPath + (logType.ToString().ToLower() + "_" + startTimestamp) + ".txt", true);
            log.WriteLine(DateTime.Now.ToString("G") + ": " + str);
            log.Close();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parseErrors"></param>
        /// <returns></returns>
        private string getParseErrors(ParseErrors parseErrors)
        {
            if (parseErrors == null) return null;
            string result = "tree.Errors = " + parseErrors.Count() + lineBreak;
            foreach (ParseError parseError in parseErrors)
                result += "ParseError( " + parseError.Code + "): " + parseError.Message + lineBreak;
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public string getItemTag(ACDItem item)
        {
            if (item == null)
                return "nullItem";

            return item.Name
                + "(" + item.Level + ")"
                + " " + item.ItemQualityLevel
                + " " + item.ItemBaseType
                + " " + item.ItemType
                + " [" + item.GameBalanceId + "]";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public string getFullItem(ACDItem item)
        {
            // add item info
            string result = getItemTag(item);

            if (item == null) return result;
            if (item.IsUnidentified) return result + " (unidentified)";


            if (item.ItemType == ItemType.Unknown
                || item.ItemBaseType == ItemBaseType.Gem
                || item.ItemBaseType == ItemBaseType.Misc)
                return result;

            // add stats            
            result += lineBreak + "+--------------------------------------------------------------";
            foreach (string key in itemDic.Keys)
            {
                object value;
                if (itemDic.TryGetValue(key, out value))
                {
                    if (value is float && (float)value > 0)
                        result += lineBreak + "| - " + key.ToUpper() + ": " + ((float)value).ToString("0.00");
                    else if (value is string && (string)value != "")
                        result += lineBreak + "| - " + key.ToUpper() + ": " + value.ToString();
                    else if (value is bool)
                        result += lineBreak + "| - " + key.ToUpper() + ": " + value.ToString();
                }
            }
            result += lineBreak + "+--------------------------------------------------------------";
            return result;
        }


        private void fillDic(ACDItem item)
        {
            object result;
            itemDic = new Dictionary<string, object>();

            // test values
            //itemDic.Add("[O]", (float)0);
            //itemDic.Add("[I]", (float)1);
            //itemDic.Add("[IO]", (float)10);
            //itemDic.Add("[TEST]", "TEST");
            //itemDic.Add("[TRUE]", true);

            // return if no item available
            if (item == null)
            {
                logOut("We received an item with a null reference!", LogType.ERROR);
                return;
            }

            // - BASETYPE ---------------------------------------------------------//
            itemDic.Add("[BASETYPE]", item.ItemBaseType.ToString());

            // - TYPE -------------------------------------------------------------//
            // TODO: this an ugly redundant piece of shit ... db returns unknow itemtype for legendary plans
            if (item.ItemType == ItemType.Unknown && item.Name.Contains("Plan"))
                result = ItemType.CraftingPlan.ToString();
            else result = item.ItemType.ToString();
            itemDic.Add("[TYPE]", result);

            // - QUALITY -------------------------------------------------------//
            // TODO: this an ugly redundant piece of shit ... db returns unknow itemtype for legendary plans
            if ((item.ItemType == ItemType.Unknown && item.Name.Contains("Plan")) || item.ItemType == ItemType.CraftingPlan)
            {
                if (item.Name.Contains("ffbf642f"))
                    result = ItemQuality.Legendary.ToString();
                else if (item.Name.Contains("Exalted Grand"))
                    result = ItemQuality.Rare6.ToString();
                else if (item.Name.Contains("Exalted Fine"))
                    result = ItemQuality.Rare5.ToString();
                else if (item.Name.Contains("Exalted"))
                    result = ItemQuality.Rare4.ToString();
                else
                    result = ItemQuality.Normal.ToString();
            }
            else
                result = Regex.Replace(item.ItemQualityLevel.ToString(), @"[\d-]", string.Empty);
            itemDic.Add("[QUALITY]", result);

            // - NAME -------------------------------------------------------------//
            if ((item.ItemType == ItemType.Unknown && item.Name.Contains("Plan")) || item.ItemType == ItemType.CraftingPlan)
            {
                //{c:ffffff00}Plan: Exalted Fine Doomcaster{/c}
                itemDic.Add("[NAME]", Regex.Replace(item.Name, @"{[/:a-zA-Z0-9 ]*}", string.Empty).Replace(" ", ""));
            }
            else
                itemDic.Add("[NAME]", item.Name.ToString().Replace(" ", ""));

            // - LEVEL ------------------------------------------------------------//
            itemDic.Add("[LEVEL]", (float)item.Level);
            itemDic.Add("[ONEHAND]", item.IsOneHand);
            itemDic.Add("[TWOHAND]", item.IsTwoHand);
            itemDic.Add("[UNIDENT]", item.IsUnidentified);

            // if there are no stats return
            //if (item.Stats == null) return;

            itemDic.Add("[STR]", item.Stats.Strength);
            itemDic.Add("[DEX]", item.Stats.Dexterity);
            itemDic.Add("[INT]", item.Stats.Intelligence);
            itemDic.Add("[VIT]", item.Stats.Vitality);
            itemDic.Add("[AS%]", item.Stats.AttackSpeedPercent);
            itemDic.Add("[MS%]", item.Stats.MovementSpeed);
            itemDic.Add("[LIFE%]", item.Stats.LifePercent);
            itemDic.Add("[LS%]", item.Stats.LifeSteal);
            itemDic.Add("[LOH]", item.Stats.LifeOnHit);
            itemDic.Add("[REGEN]", item.Stats.HealthPerSecond);
            itemDic.Add("[GLOBEBONUS]", item.Stats.HealthGlobeBonus);
            itemDic.Add("[DPS]", item.Stats.WeaponDamagePerSecond);
            itemDic.Add("[WEAPAS]", item.Stats.WeaponAttacksPerSecond);
            itemDic.Add("[WEAPMAXDMG]", item.Stats.WeaponMaxDamage);
            itemDic.Add("[WEAPMINDMG]", item.Stats.WeaponMinDamage);
            itemDic.Add("[CRIT%]", item.Stats.CritPercent);
            itemDic.Add("[CRITDMG%]", item.Stats.CritDamagePercent);
            itemDic.Add("[BLOCK%]", item.Stats.BlockChance);
            itemDic.Add("[MINDMG]", item.Stats.MinDamage);
            itemDic.Add("[MAXDMG]", item.Stats.MaxDamage);
            itemDic.Add("[ALLRES]", item.Stats.ResistAll);
            itemDic.Add("[RESPHYSICAL]", item.Stats.ResistPhysical);
            itemDic.Add("[RESFIRE]", item.Stats.ResistFire);
            itemDic.Add("[RESLIGHTNING]", item.Stats.ResistLightning);
            itemDic.Add("[RESHOLY]", item.Stats.ResistHoly);
            itemDic.Add("[RESARCAN]", item.Stats.ResistArcane);
            itemDic.Add("[RESCOLD]", item.Stats.ResistCold);
            itemDic.Add("[RESPOISON]", item.Stats.ResistPoison);
            itemDic.Add("[FIREDMG%]", item.Stats.FireDamagePercent);
            itemDic.Add("[LIGHTNINGDMG%]", item.Stats.LightningDamagePercent);
            itemDic.Add("[COLDDMG%]", item.Stats.ColdDamagePercent);
            itemDic.Add("[POISONDMG%]", item.Stats.PoisonDamagePercent);
            itemDic.Add("[ARCANEDMG%]", item.Stats.ArcaneDamagePercent);
            itemDic.Add("[HOLYDMG%]", item.Stats.HolyDamagePercent);
            itemDic.Add("[ARMOR]", item.Stats.Armor);
            itemDic.Add("[ARMORBONUS]", item.Stats.ArmorBonus);
            itemDic.Add("[ARMORTOT]", item.Stats.ArmorTotal);
            itemDic.Add("[GF%]", item.Stats.GoldFind);
            itemDic.Add("[MF%]", item.Stats.MagicFind);
            itemDic.Add("[PICKUP]", item.Stats.PickUpRadius);
            itemDic.Add("[SOCKETS]", (float)item.Stats.Sockets);
            itemDic.Add("[THORNS]", item.Stats.Thorns);
            itemDic.Add("[DMGREDPHYSICAL]", item.Stats.DamageReductionPhysicalPercent);
            itemDic.Add("[MAXARCPOWER]", item.Stats.MaxArcanePower);
            itemDic.Add("[HEALTHSPIRIT]", item.Stats.HealthPerSpiritSpent);
            itemDic.Add("[MAXSPIRIT]", item.Stats.MaxSpirit);
            itemDic.Add("[SPIRITREG]", item.Stats.SpiritRegen);
            itemDic.Add("[ARCONCRIT]", item.Stats.ArcaneOnCrit);
            itemDic.Add("[MAXFURY]", item.Stats.MaxFury);
            itemDic.Add("[MAXDISCIP]", item.Stats.MaxDiscipline);
            itemDic.Add("[HATREDREG]", item.Stats.HatredRegen);
            itemDic.Add("[MAXMANA]", item.Stats.MaxMana);
            itemDic.Add("[MANAREG]", item.Stats.ManaRegen);
            itemDic.Add("[MAXSTAT]", new float[] { item.Stats.Strength, item.Stats.Intelligence, item.Stats.Dexterity }.Max());
            itemDic.Add("[MAXSTATVIT]", new float[] { item.Stats.Strength, item.Stats.Intelligence, item.Stats.Dexterity }.Max() + item.Stats.Vitality);
            itemDic.Add("[STRVIT]", item.Stats.Strength + item.Stats.Vitality);
            itemDic.Add("[DEXVIT]", item.Stats.Dexterity + item.Stats.Vitality);
            itemDic.Add("[INTVIT]", item.Stats.Intelligence + item.Stats.Vitality);
            itemDic.Add("[MAXONERES]", new float[] { item.Stats.ResistArcane, item.Stats.ResistCold, item.Stats.ResistFire, item.Stats.ResistHoly, item.Stats.ResistLightning, item.Stats.ResistPhysical, item.Stats.ResistPoison }.Max());
            itemDic.Add("[TOTRES]", item.Stats.ResistArcane + item.Stats.ResistCold + item.Stats.ResistFire + item.Stats.ResistHoly + item.Stats.ResistLightning + item.Stats.ResistPhysical + item.Stats.ResistPoison + item.Stats.ResistAll);
            itemDic.Add("[DMGFACTOR]", item.Stats.AttackSpeedPercent + item.Stats.CritPercent * 2 + item.Stats.CritDamagePercent / 5 + (item.Stats.MinDamage + item.Stats.MaxDamage) / 20);
            itemDic.Add("[AVGDMG]", (item.Stats.MinDamage + item.Stats.MaxDamage) / 2);

            float offstats = 0;
            //if (new float[] { item.Stats.Strength, item.Stats.Intelligence, item.Stats.Dexterity }.Max() > 0)
            //    offstats += 1;
            if (item.Stats.CritPercent > 0)
                offstats += 1;
            if (item.Stats.CritDamagePercent > 0)
                offstats += 1;
            if (item.Stats.AttackSpeedPercent > 0)
                offstats += 1;
            if (item.Stats.MinDamage + item.Stats.MaxDamage > 0)
                offstats += 1;
            itemDic.Add("[OFFSTATS]", offstats);

            float defstats = 0;
            //if (item.Stats.Vitality > 0)
            defstats += 1;
            if (item.Stats.ResistAll > 0)
                defstats += 1;
            if (item.Stats.ArmorBonus > 0)
                defstats += 1;
            if (item.Stats.BlockChance > 0)
                defstats += 1;
            if (item.Stats.LifePercent > 0)
                defstats += 1;
            //if (item.Stats.HealthPerSecond > 0)
            //    defstats += 1;
            itemDic.Add("[DEFSTATS]", defstats);
            //itemDic.Add("[GAMEBALANCEID]", (float)item.GameBalanceId);
        }

    }

    #endregion Interpreter
}
