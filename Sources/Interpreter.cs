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
        readonly string version = "2.0.2.00";
        readonly string rulesPath = Path.Combine(FileManager.ItemRulePath, "Rules");
        readonly string logPath = Path.Combine(FileManager.ItemRulePath, "Log");
        readonly string configFile = "config.dis";
        readonly string pickupFile = "pickup.dis";
        readonly string logFile = "actualLog.txt";
        readonly string assign = "->", SEP = ";";
        readonly Regex filePattern = new Regex(@"\[FILE\][ ]*==[ ]*([A-Za-z]+.dis)", RegexOptions.Compiled);
        readonly Regex flagPattern = new Regex(@"\[([A-Z]+)\][ ]*==[ ]*([A-Za-z]+)", RegexOptions.Compiled);
        readonly Regex macroPattern = new Regex(@"(@[A-Z]+)[ ]*:=[ ]*(.+)", RegexOptions.Compiled);
       
        string ruleType = "soft";

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

        // dictonary for the use of macros
        private Dictionary<string, string> macroDic;
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
            DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, " ___________________Rel.-v {0}_______", version);
        }

        public void reset()
        {
            string actualLog = Path.Combine(logPath, logFile);
            string archivePath = Path.Combine(logPath, "Archive");
            string archiveLog = Path.Combine(archivePath, DateTime.Now.ToString("ddMMyyyyHHmmss") + "_log.txt");

            if (!File.Exists(actualLog))
                return;

            if (!Directory.Exists(archivePath))
                Directory.CreateDirectory(archivePath);

            File.Copy(actualLog, archiveLog, true);

            File.Delete(actualLog);
        }

        /// <summary>
        /// 
        /// </summary>
        public void readConfiguration()
        {
            reset();

            StreamReader streamReader = new StreamReader(Path.Combine(rulesPath, configFile));

            // initialize or reset ruleSet array
            ruleSet = new ArrayList();
            pickUpRuleSet = new ArrayList();
            
            // instantiating our macro dictonary
            macroDic = new Dictionary<string,string>();

            // instantiating our itemfilename array
            List<string> itemFileNames = new List<string>();

            string str;
            Match match1, match2;
            while ((str = streamReader.ReadLine()) != null)
            {
                str = str.Split(new string[] { "//" }, StringSplitOptions.None)[0].Replace(" ", "").Replace("\t", "");
                if (str.Length == 0) continue;

                match1 = filePattern.Match(str);
                match2 = flagPattern.Match(str);

                if (match1.Success && File.Exists(Path.Combine(rulesPath, ruleType, match1.Groups[1].Value)))
                {
                    logOut(Path.Combine(rulesPath, ruleType, match1.Groups[1].Value), LogType.DEBUG);
                    DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "path = {0}", Path.Combine(rulesPath, ruleType, match1.Groups[1].Value));
                    itemFileNames.Add(match1.Groups[1].Value);
                }
                else if (match2.Success)
                {
                    if (match2.Groups[1].Value.Contains("DEBUG"))
                        debugFlag = Boolean.Parse(match2.Groups[2].Value);

                    if (match2.Groups[1].Value.Contains("RULE"))
                        ruleType = match2.Groups[2].Value.ToLower();
                }

            }

            // parse pickup file
            DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "reading ... {0} ", pickupFile);
            pickUpRuleSet = readLinesToArray(new StreamReader(Path.Combine(rulesPath, pickupFile)), pickUpRuleSet);
            DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, " ... loaded: {0} pickup rules", pickUpRuleSet.Count);

            // parse all item files
            foreach (string itemFileName in itemFileNames)
            {
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "Reading ... {0}", itemFileName);
                logOut("Reading " + itemFileName + " " + ruleType, LogType.DEBUG);
                ruleSet = readLinesToArray(new StreamReader(Path.Combine(rulesPath, ruleType, itemFileName)), ruleSet);
            }
            DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, " ... loaded: {0} item " + ruleType + " rules", ruleSet.Count);
            DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, " ... loaded: {0} macros", macroDic.Count);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="streamReader"></param>
        /// <param name="array"></param>
        /// <returns></returns>
        private ArrayList readLinesToArray(StreamReader streamReader, ArrayList array)
        {
            string str = "";
            Match match;
            while ((str = streamReader.ReadLine()) != null)
            {
                str = str.Split(new string[] { "//" }, StringSplitOptions.None)[0]
                    .Replace(" ", "")
                    .Replace("\t", "");
                if (str.Length == 0)
                    continue;

                // - start macro transformation
                match = macroPattern.Match(str);

                if (match.Success)
                {
                    //DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, " macro added: {0} := {1}", match.Groups[1].Value, match.Groups[2].Value);
                    macroDic.Add(match.Groups[1].Value, match.Groups[2].Value);
                    continue;
                }
                // - stop macro transformation

                array.Add(str);
                logOut("Rule Nr." + array.Count + " - " + str, LogType.DEBUG);
            }
            return array;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="level"></param>
        /// <param name="quality"></param>
        /// <param name="itemBaseType"></param>
        /// <param name="itemType"></param>
        /// <param name="isOneHand"></param>
        /// <param name="isTwoHand"></param>
        /// <param name="gameBalanceID"></param>
        /// <returns></returns>
        public InterpreterAction checkPickUpItem(string name, int level, ItemQuality quality, ItemBaseType itemBaseType, ItemType itemType, bool isOneHand, bool isTwoHand, int gameBalanceId, int dynamicId)
        {
            fillPickupDic(name, level, quality, itemBaseType, itemType, isOneHand, isTwoHand, gameBalanceId, dynamicId);

            return checkItem(true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pickUp"></param>
        /// <returns></returns>
        public InterpreterAction checkItem(ACDItem item)
        {
            fillDic(item);

            return checkItem(false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public InterpreterAction checkItem(bool pickUp)
        {

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
                if (pickUp) tempAction = InterpreterAction.PICKUP;
                else tempAction = InterpreterAction.KEEP;

                string[] strings = str.Split(new string[] { assign }, StringSplitOptions.None);
                if (strings.Count() > 1)
                    tempAction = getInterpreterAction(strings[1]);

                try
                {
                    if (evaluate(strings[0], out parseErrors))
                    {
                        validRule = str;
                        action = tempAction;
                        if (parseErrors.Count > 0)
                            logOut("Have errors with out a catch!"
                                + SEP + "last use rule: " + str
                                + SEP + getParseErrors(parseErrors)
                                + SEP + getFullItem(), LogType.DEBUG);
                        break;
                    }
                }
                catch (Exception e)
                {
                    logOut(e.Message
                        + SEP + "last use rule: " + str
                        + SEP + getParseErrors(parseErrors)
                        + SEP + getFullItem(), LogType.ERROR);
                }
            }

            logOut(pickUp, validRule, action);

            return action;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pickUp"></param>
        /// <param name="validRule"></param>
        /// <param name="action"></param>
        private void logOut(bool pickUp, string validRule, InterpreterAction action)
        {
            string logString = getFullItem() + validRule;

            switch (action)
            {
                case InterpreterAction.PICKUP:
                    logOut(logString, action, LogType.DEBUG);
                    break;
                case InterpreterAction.IGNORE:
                    logOut(logString, action, LogType.DEBUG);
                    break;
                case InterpreterAction.KEEP:
                    logOut(logString, action, LogType.LOG);
                    break;
                case InterpreterAction.TRASH:
                    if (itemDic["[QUALITY]"].ToString() == "Legendary")
                        logOut(logString, action, LogType.LOG);
                    else
                        logOut(logString, action, LogType.DEBUG);
                    break;
                case InterpreterAction.SCORE:
                    logOut(logString, action, LogType.DEBUG);
                    break;
                case InterpreterAction.NULL:
                    if (pickUp)
                        logOut(logString, action, LogType.DEBUG);
                    else
                        logOut(logString, action, LogType.DEBUG);
                    break;
            }
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
        /// <param name="parseErrors"></param>
        /// <returns></returns>
        private object evaluateExpr(string str, out ParseErrors parseErrors)
        {
            ItemRules.Core.ParseTree tree = parser.Parse(str);
            parseErrors = tree.Errors;
            return tree.Eval(null);
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
        /// <param name="action"></param>
        /// <param name="logType"></param>
        public void logOut(string str, InterpreterAction action, LogType logType)
        {
            logOut(action + SEP + str, logType);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <param name="logType"></param>
        public void logOut(string str, LogType logType)
        {
            // no debugging when flag set false
            if (logType == LogType.DEBUG && !debugFlag)
                return;

            // create directory if it doesn't exists
            if (!Directory.Exists(logPath))
                Directory.CreateDirectory(logPath);

            log = new StreamWriter(Path.Combine(logPath, logFile), true);
            log.WriteLine(DateTime.Now.ToString("yyyyMMddHHmmssffff") + SEP + logType + SEP + str);
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
            string result = "tree.Errors = " + parseErrors.Count() + SEP;
            foreach (ParseError parseError in parseErrors)
                result += "ParseError( " + parseError.Code + "): " + parseError.Message + SEP;
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string getFullItem()
        {
            string result = "";

            // add stats            
            foreach (string key in itemDic.Keys)
            {
                object value;
                if (itemDic.TryGetValue(key, out value))
                {
                    if (value is float && (float)value > 0)
                        result += key.ToUpper() + ":" + ((float)value).ToString("0.00").Replace(".00","") + SEP;
                    else if (value is string && (string)value != "")
                        result += key.ToUpper() + ":" + value.ToString() + SEP;
                    else if (value is bool)
                        result += key.ToUpper() + ":" + value.ToString() + SEP;
                    else if (value is int)
                        result += key.ToUpper() + ":" + value.ToString() + SEP;
                }
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool getVariableValue(string key, out object obj) {

            string[] strArray = key.Split('.');

            if (Interpreter.itemDic.TryGetValue(strArray[0], out obj) && strArray.Count() > 1)
            {
                   switch (strArray[1])
                    {
                        case "dual":
                            if (obj is float && (float)obj > 0)
                                obj = (float)1;
                            break;
                        case "max":
                            if (obj is float)
                                obj = (float)1;
                            break;
                    }
            }

            return (obj != null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="level"></param>
        /// <param name="itemQuality"></param>
        /// <param name="itemBaseType"></param>
        /// <param name="itemType"></param>
        /// <param name="isOneHand"></param>
        /// <param name="isTwoHand"></param>
        /// <param name="gameBalanceId"></param>
        private void fillPickupDic(string name, int level, ItemQuality itemQuality, ItemBaseType itemBaseType, ItemType itemType, bool isOneHand, bool isTwoHand, int gameBalanceId, int dynamicId)
        {
            object result;
            itemDic = new Dictionary<string, object>();

            // add log unique key
            itemDic.Add("[KEY]", getHashKey(gameBalanceId, dynamicId));

            // - BASETYPE ---------------------------------------------------------//
            itemDic.Add("[BASETYPE]", itemBaseType.ToString());

            // - TYPE -------------------------------------------------------------//
            // TODO: this an ugly redundant piece of shit ... db returns unknow itemtype for legendary plans
            if (itemType == ItemType.Unknown && name.Contains("Plan"))
                result = ItemType.CraftingPlan.ToString();
            else result = itemType.ToString();
            itemDic.Add("[TYPE]", result);

            // - QUALITY -------------------------------------------------------//
            // TODO: this an ugly redundant piece of shit ... db returns unknow itemtype for legendary plans
            if ((itemType == ItemType.Unknown && name.Contains("Plan")) || itemType == ItemType.CraftingPlan)
            {
                if (name.Contains("ffbf642f"))
                    result = ItemQuality.Legendary.ToString();
                else if (name.Contains("Exalted Grand"))
                    result = ItemQuality.Rare6.ToString();
                else if (name.Contains("Exalted Fine"))
                    result = ItemQuality.Rare5.ToString();
                else if (name.Contains("Exalted"))
                    result = ItemQuality.Rare4.ToString();
                else
                    result = ItemQuality.Normal.ToString();
            }
            else
                result = Regex.Replace(itemQuality.ToString(), @"[\d-]", string.Empty);
            itemDic.Add("[QUALITY]", result);

            // - NAME -------------------------------------------------------------//
            if ((itemType == ItemType.Unknown && name.Contains("Plan")) || itemType == ItemType.CraftingPlan)
            {
                //{c:ffffff00}Plan: Exalted Fine Doomcaster{/c}
                itemDic.Add("[NAME]", Regex.Replace(name, @"{[/:a-zA-Z0-9 ]*}", string.Empty).Replace(" ", ""));
            }
            else
                itemDic.Add("[NAME]", name.ToString().Replace(" ", ""));

            // - LEVEL ------------------------------------------------------------//
            itemDic.Add("[LEVEL]", (float)level);
            itemDic.Add("[ONEHAND]", isOneHand);
            itemDic.Add("[TWOHAND]", isTwoHand);
            itemDic.Add("[UNIDENT]", (bool)true);
            //itemDic.Add("[GAMEBALANCEID]", (float)item.GameBalanceId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        private void fillDic(ACDItem item)
        {
            object result;
            itemDic = new Dictionary<string, object>();

            // return if no item available
            if (item == null)
            {
                logOut("received an item with a null reference!", LogType.ERROR);
                return;
            }

            // add log unique key
            itemDic.Add("[KEY]", getHashKey(item.GameBalanceId, item.DynamicId));

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
            itemDic.Add("[PICKRAD]", item.Stats.PickUpRadius);
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
            //itemDic.Add("[DYNAMICID]", item.DynamicId);

            // starting on macro implementation here
            foreach (string key in macroDic.Keys)
            {
                ParseErrors parseErrors = null;
                string expr = macroDic[key];
                try
                {
                    object exprValue = evaluateExpr(expr, out parseErrors);
                    itemDic.Add("[" + key + "]", exprValue);
                }
                catch (Exception e)
                {
                    logOut(e.Message
                        + SEP + "last use rule: " + expr
                        + SEP + getParseErrors(parseErrors)
                        + SEP + getFullItem(), LogType.ERROR);
                }
            }

            // end macro implementation
        }

        private object getHashKey(int gameBalanceId, int dynamicId)
        {
            return dynamicId;
        }

    }

    #endregion Interpreter
}
