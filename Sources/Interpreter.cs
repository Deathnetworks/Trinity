using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Collections;
using Zeta.Internals.Actors;
using Zeta.Internals;
using Zeta.Common;
using System.Reflection;
using Zeta.CommonBot;

namespace GilesTrinity.ItemRules
{

    class Interpreter
    {
        string[] strArrayString = new string[] { "[BASETYPE]", "[TYPE]", "[QUALITY]", "[NAME]" };

        string[] strArrayBool = new string[] { "[ONEHAND]", "[TWOHAND]" };

        string[] strArrayFloat = new string[] { "[LEVEL]","[STR]","[DEX]","[INT]","[VIT]","[AS%]","[MS%]","[LIFE%]",
                                                "[LOH]","[REGEN]","[GLOBEBONUS]","[DPS]", "[WEAPAS]","[WEAPMAXDMG]",
                                                "[CRIT%]","[CRITDMG%]","[BLOCK%]","[MINDMG]","[MAXDMG]","[ALLRES]",
                                                "[RESPHYSICAL]","[RESFIRE]","[RESLIGHTNING]","[RESHOLY]","[RESARCAN]",
                                                "[RESCOLD]","[RESPOISON]","[FIREDMG%]","[LIGHTNINGDMG%]","[COLDDMG%]",
                                                "[POISONDMG%]","[ARCANEDMG%]","[HOLYDMG%]","[ARMOR]","[ARMORBONUS]",
                                                "[ARMORTOT]","[GF%]","[MF%]","[PICKUP]","[SOCKETS]","[THORNS]",
                                                "[MAXARCPOWER]","[HEALTHSPIRIT]","[MAXSPIRIT]","[SPIRITREG]","[ARCONCRIT]",
                                                "[MAXFURY]","[MAXDISCIP]","[LS%]","[WEAPMINDMG]","[DMGREDPHYSICAL]",
                                                "[HATREDREG]","[MAXMANA]","[MANAREG]","[MAXSTAT]","[MAXSTATVIT]",
                                                "[MAXONERES]","[TOTRES]", "[STRVIT]","[DEXVIT]","[INTVIT]",
                                                "[DMGFACTOR]","[AVGDMG]","[OFFSTATS]","[DEFSTATS]"};

        public enum LogType { LOG, TRASH, DEBUG, ERROR };

        public enum InterpreterAction { KEEP, TRASH, NULL };

        string[] comparators = new string[] { "==", "!=", "<=", ">=", "<", ">" };

        string[] operators = new string[] { "+" };

        ArrayList ruleSet;
        ACDItem item;
        TextWriter log;

        bool debugFlag = false, logFlag = false, trashLogFlag = false;

        string startTimestamp,customPath,logPath;

        static void Main()
        {
            //Interpreter interpreter = new Interpreter();
            //interpreter.init();
            //interpreter.checkItem(null);
        }

        /// <summary>
        /// 
        /// </summary>
        public Interpreter()
        {
            init();
        }

        /// <summary>
        /// 
        /// </summary>
        public void init()
        {
            startTimestamp = DateTime.Now.ToString("ddMMyyyyHHmmss");
            customPath = @"Plugins\GilesTrinity\ItemRules\";
            logPath = @"Plugins\GilesTrinity\Log\";

            ruleSet = new ArrayList();

            string disFileName = "config.dis";
            StreamReader stream = new StreamReader(customPath + disFileName);

            string str;
            string[] rule;

            Logging.Write("starting initializing Item Rule Set!");

            logOut("--- STARTING A NEW SESSION WITH ITEMRULESET ---", LogType.LOG);
            List<string> itemFileNames = new List<string>();
            while ((str = stream.ReadLine()) != null)
            {
                string fileName = "";
                rule = str.Replace(" ", "").Replace("\t", "").Split(new string[] { "//" }, StringSplitOptions.None);
                if (rule[0].IndexOf("$$") != -1)
                {
                    fileName = interpretConfig(rule[0].Replace("$$", ""));
                    if (fileName.Length > 0 && fileName.Contains(".dis"))
                        itemFileNames.Add(fileName);
                }
            }


            foreach (string itemFileName in itemFileNames)
            {
                logOut("... reading file: " + itemFileName, LogType.DEBUG);
                readItemFile(new StreamReader(customPath + itemFileName));
            }
            logOut("initialized " + ruleSet.Count + " itemrulesets!", LogType.DEBUG);
            Logging.Write("initialized " + ruleSet.Count + " itemrulesets!");
            Logging.Write("finished initializing Item Rule Set!");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        private void readItemFile(StreamReader stream)
        {
            string str;
            string[] rule;

            while ((str = stream.ReadLine()) != null)
            {
                rule = str.Replace(" ", "").Replace("\t", "").Split(new string[] { "//" }, StringSplitOptions.None);

                if (rule[0].IndexOf('#') != -1 && testRule(rule[0]))
                {
                    ruleSet.Add(rule[0]);
                    logOut(ruleSet.Count + ":" + rule[0], LogType.DEBUG);
                }
                else if (rule[0].Length > 0)
                {
                    logOut("#WARNING(BAD LINE): " + rule[0], LogType.ERROR);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rule"></param>
        /// <returns></returns>
        private bool testRule(string rule)
        {

            // count test
            if (rule.Split('(').Length - 1 != rule.Split(')').Length - 1)
                return false;

            if (rule.Split('[').Length - 1 != rule.Split(']').Length - 1)
                return false;

            // syntax null rest test
            string nullTest = rule;
            foreach (string str in strArrayString)
                nullTest = nullTest.Replace(str, "");
            foreach (string str in strArrayBool)
                nullTest = nullTest.Replace(str, "");
            foreach (string str in strArrayFloat)
                nullTest = nullTest.Replace(str, "");
            foreach (string str in comparators)
                nullTest = nullTest.Replace(str, "");
            nullTest = nullTest.Replace("[1]", "");
            nullTest = nullTest.Replace("[TEST]", "");
            nullTest = nullTest.Replace("[TRUE]", "");
            nullTest = nullTest.Replace("[KEEP]", "");
            nullTest = nullTest.Replace("[TRASH]", "");

            if (nullTest.Split('[').Length > 1)
            {
                logOut(nullTest, LogType.ERROR);
                return false;
            }

            bool checkFlag = true;

            // logical testing
            foreach (string str in strArrayString)
                rule = rule.Replace(str, "[TEST]");
            foreach (string str in strArrayBool)
                rule = rule.Replace(str, "[TRUE]");
            foreach (string str in strArrayFloat)
                rule = rule.Replace(str, "[0]");

            interpret(rule, ref checkFlag);
            return checkFlag;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private string interpretConfig(string str)
        {
            string fileName = "";
            if (hasStrFromArrayIn(comparators, str) != null)
            {
                string comparator = hasStrFromArrayIn(comparators, str);
                string[] strings = str.Split(new string[] { comparator }, StringSplitOptions.None);
                fileName = interpret(strings[0], comparator, strings[1]);
            }
            return fileName;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="str1"></param>
        /// <param name="comparator"></param>
        /// <param name="str2"></param>
        /// <returns></returns>
        private string interpret(string str1, string comparator, string str2)
        {
            string fileName = "";
            switch (str1)
            {
                case "[DEBUG]":
                    debugFlag = Boolean.Parse(str2);
                    logOut("Debugging set to " + logFlag, LogType.DEBUG);
                    break;
                case "[LOG]":
                    logFlag = Boolean.Parse(str2);
                    logOut("Logging set to " + logFlag, LogType.DEBUG);
                    break;
                case "[TRASHLOG]":
                    trashLogFlag = Boolean.Parse(str2);
                    logOut("Logging trashed legendarys set to " + logFlag, LogType.DEBUG);
                    break;
                case "[FILE]":
                    if (File.Exists(customPath + str2))
                        fileName = str2;
                    break;
                default:
                    break;
            }
            return fileName;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public InterpreterAction checkItem(ACDItem item)
        {
            this.item = item;

            if (debugFlag)
            {
                logOut("- CHECK ITEM START ----------------------------------------------", LogType.DEBUG);
                if (item != null) logOut("checkItem: " + getItemTag(item), LogType.DEBUG);
            }

            bool checkFlag = true;

            InterpreterAction action = InterpreterAction.NULL;

            string validRule = "";

            foreach (string str in ruleSet)
            {
                action = interpret(str, ref checkFlag);

                if (!checkFlag)
                {
                    logOut("#WARNING(RULE): " + str, LogType.ERROR);
                    logItemFullTag(item, LogType.ERROR);
                    return InterpreterAction.NULL;
                }

                if (action != InterpreterAction.NULL)
                {
                    validRule = str;
                    loggingAction(item, action, str);
                    break;
                }

            }

            if (item != null && debugFlag)
            {
                if (item != null) logItemFullTag(item, LogType.DEBUG);
                logOut("Matching rule: " + validRule, LogType.DEBUG);
                logOut("Performing action: " + action.ToString(), LogType.DEBUG);
                logOut("- CHECK ITEM END ------------------------------------------------", LogType.DEBUG);
            }

            return action;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <param name="action"></param>
        /// <param name="rule"></param>
        private void loggingAction(ACDItem item, InterpreterAction action, string rule)
        {
            if (trashLogFlag && action == InterpreterAction.TRASH && (item == null || item.ItemQualityLevel == ItemQuality.Legendary))
            {
                logOut("-----------------------------------------------------------------", LogType.TRASH);
                if (item != null) logOut(action.ToString() + ": " + getItemTag(item), LogType.TRASH);
                logOut("Rule:" + rule, LogType.TRASH);
                if (item != null) logItemFullTag(item, LogType.TRASH);
            }
            else if (logFlag && action == InterpreterAction.KEEP)
            {
                logOut("-----------------------------------------------------------------", LogType.LOG);
                if (item != null) logOut(action.ToString() + ": " + getItemTag(item), LogType.LOG);
                logOut("Rule:" + rule, LogType.LOG);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        /// <param name="checkFlag"></param>
        /// <returns></returns>
        private InterpreterAction interpret(string line, ref bool checkFlag)
        {

            InterpreterAction action = InterpreterAction.KEEP;

            string[] strings = line.Split('#');

            if (strings.Length == 3)
                action = getInterpreterAction(strings[2]);

            if (checkTruth(strings[0], ref checkFlag) && checkTruth(strings[1], ref checkFlag))
                return action;

            return InterpreterAction.NULL;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <param name="checkFlag"></param>
        /// <returns></returns>
        private bool checkTruth(string str, ref bool checkFlag)
        {
            if (!checkFlag)
                return false;

            int firstBracket = str.IndexOf(')');

            if (firstBracket != -1)
            {
                int closetBracket = str.Substring(0, firstBracket).LastIndexOf('(');
                string bracketContainer = str.Substring(closetBracket + 1, firstBracket - closetBracket - 1);
                return checkTruth(str.Replace("(" + bracketContainer + ")", checkTruth(bracketContainer, ref checkFlag).ToString()), ref checkFlag);
            }
            else if (str.IndexOf("&&") != -1)
            {
                bool result = true;
                string[] strings = str.Split(new string[] { "&&" }, StringSplitOptions.None);
                foreach (string hold in strings)
                    result &= checkTruth(hold, ref checkFlag);
                return result;
            }
            else if (str.IndexOf("||") != -1)
            {
                bool result = false;
                string[] strings = str.Split(new string[] { "||" }, StringSplitOptions.None);
                foreach (string hold in strings)
                    result |= checkTruth(hold, ref checkFlag);
                return result;
            }
            //else if (hasStrFromArrayIn(operators, str) != null)
            //{
            //    string[] strNoComparators = new string[] {str};
            //    string comparator = hasStrFromArrayIn(comparators, str);
            //    if (comparator != null)
            //        strNoComparators = str.Split(new string[] { comparator }, StringSplitOptions.None);
            //    for (int i = 0; i < strNoComparators.Length; i++)
            //        strNoComparators[i] = doMath(strNoComparators[i]);
            //    str = "";
            //    for (int i = 0; i < strNoComparators.Length; i++)
            //    {
            //        str += strNoComparators[i];
            //        if (comparator != null && i < strNoComparators.Length - 1)
            //            str += comparator;
            //    }
            //}
            else if (hasStrFromArrayIn(comparators, str) != null)
            {
                string comparator = hasStrFromArrayIn(comparators, str);
                string[] strings = str.Split(new string[] { comparator }, StringSplitOptions.None);
                return checkExpression(strings[0], comparator, strings[1], ref checkFlag);
            }
            else if (str == "True")
                return true;
            else if (str == "False")
                return false;

            checkFlag = false;
            return false;
        }

        //private string doMath(string str)
        //{
        //    string signOperator = hasStrFromArrayIn(operators, str);
        //    string[] strNoOperators = new string[] { str };
        //    if (signOperator != null)
        //        strNoOperators = str.Split(new string[] { signOperator }, StringSplitOptions.None);

        //    bool checkFlag = true;
        //    float result;
        //    for (int i = 0; i < strNoOperators.Length; i++)
        //        if (!Single.TryParse(strNoOperators[i], out result))
        //            strNoOperators[i] = ((float) getValueFromString(strNoOperators[0], ref checkFlag)).ToString();

        //    result = 0;
        //    foreach (string partStr in strNoOperators)
        //        result += Single.Parse(partStr);

        //    return result.ToString();
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="str1"></param>
        /// <param name="comparator"></param>
        /// <param name="str2"></param>
        /// <param name="checkFlag"></param>
        /// <returns></returns>
        private bool compare(string str1, string comparator, string str2, ref bool checkFlag)
        {
            switch (comparator)
            {
                case "==":
                    return str1 == str2;
                case "!=":
                    return str1 != str2;
                default:
                    checkFlag = false;
                    return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bool1"></param>
        /// <param name="comparator"></param>
        /// <param name="bool2"></param>
        /// <param name="checkFlag"></param>
        /// <returns></returns>
        private bool compare(bool bool1, string comparator, bool bool2, ref bool checkFlag)
        {
            switch (comparator)
            {
                case "==":
                    return bool1 == bool2;
                case "!=":
                    return bool1 != bool2;
                default:
                    checkFlag = false;
                    return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="float1"></param>
        /// <param name="comparator"></param>
        /// <param name="float2"></param>
        /// <param name="checkFlag"></param>
        /// <returns></returns>
        private bool compare(float float1, string comparator, float float2, ref bool checkFlag)
        {
            switch (comparator)
            {
                case "<":
                    return float1 < float2;
                case "<=":
                    return float1 <= float2;
                case ">":
                    return float1 > float2;
                case ">=":
                    return float1 >= float2;
                case "==":
                    return float1 == float2;
                case "!=":
                    return float1 != float2;
                default:
                    checkFlag = false;
                    return false;
            }
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
        /// <param name="array"></param>
        /// <param name="str"></param>
        /// <returns></returns>
        private string hasStrFromArrayIn(string[] array, string str)
        {
            foreach (string hold in array)
                if (str.IndexOf(hold) != -1)
                    return hold;
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <param name="checkFlag"></param>
        /// <returns></returns>
        private Object getValueFromString(string str, ref bool checkFlag)
        {
            string result = "";
            switch (str)
            {
                case "[BASETYPE]":
                    return item.ItemBaseType.ToString();
                case "[TYPE]":
                    // TODO: this an ugly redundant piece of shit ... db returns unknow itemtype for legendary plans
                    if (item.ItemType == ItemType.Unknown && item.Name.Contains("Plan"))
                        result = ItemType.CraftingPlan.ToString();
                    else
                        result = item.ItemType.ToString();
                    return result;
                case "[QUALITY]":
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
                    return result;
                case "[NAME]":
                    return item.Name.ToString().Replace(" ", "");
                case "[LEVEL]":
                    return item.Level;
                case "[ONEHAND]":
                    return item.IsOneHand;
                case "[TWOHAND]":
                    return item.IsTwoHand;
                case "[STR]":
                    return item.Stats.Strength;
                case "[DEX]":
                    return item.Stats.Dexterity;
                case "[INT]":
                    return item.Stats.Intelligence;
                case "[VIT]":
                    return item.Stats.Vitality;
                case "[AS%]":
                    return item.Stats.AttackSpeedPercent;
                case "[MS%]":
                    return item.Stats.MovementSpeed;

                case "[LIFE%]":
                    return item.Stats.LifePercent;
                case "[LS%]":
                    return item.Stats.LifeSteal;
                case "[LOH]":
                    return item.Stats.LifeOnHit;
                case "[REGEN]":
                    return item.Stats.HealthPerSecond;
                case "[GLOBEBONUS]":
                    return item.Stats.HealthGlobeBonus;

                case "[DPS]":
                    return item.Stats.WeaponDamagePerSecond;
                case "[WEAPAS]":
                    return item.Stats.WeaponAttacksPerSecond;
                case "[WEAPMAXDMG]":
                    return item.Stats.WeaponMaxDamage;
                case "[WEAPMINDMG]":
                    return item.Stats.WeaponMinDamage;
                case "[CRIT%]":
                    return item.Stats.CritPercent;
                case "[CRITDMG%]":
                    return item.Stats.CritDamagePercent;
                case "[BLOCK%]":
                    return item.Stats.BlockChance;
                case "[MINDMG]":
                    return item.Stats.MinDamage;
                case "[MAXDMG]":
                    return item.Stats.MaxDamage;

                case "[ALLRES]":
                    return item.Stats.ResistAll;
                case "[RESPHYSICAL]":
                    return item.Stats.ResistPhysical;
                case "[RESFIRE]":
                    return item.Stats.ResistFire;
                case "[RESLIGHTNING]":
                    return item.Stats.ResistLightning;
                case "[RESHOLY]":
                    return item.Stats.ResistHoly;
                case "[RESARCAN]":
                    return item.Stats.ResistArcane;
                case "[RESCOLD]":
                    return item.Stats.ResistCold;
                case "[RESPOISON]":
                    return item.Stats.ResistPoison;

                case "[FIREDMG%]":
                    return item.Stats.FireDamagePercent;
                case "[LIGHTNINGDMG%]":
                    return item.Stats.LightningDamagePercent;
                case "[COLDDMG%]":
                    return item.Stats.ColdDamagePercent;
                case "[POISONDMG%]":
                    return item.Stats.PoisonDamagePercent;
                case "[ARCANEDMG%]":
                    return item.Stats.ArcaneDamagePercent;
                case "[HOLYDMG%]":
                    return item.Stats.HolyDamagePercent;

                case "[ARMOR]":
                    return item.Stats.Armor;
                case "[ARMORBONUS]":
                    return item.Stats.ArmorBonus;
                case "[ARMORTOT]":
                    return item.Stats.ArmorTotal;
                case "[GF%]":
                    return item.Stats.GoldFind;
                case "[MF%]":
                    return item.Stats.MagicFind;
                case "[PICKUP]":
                    return item.Stats.PickUpRadius;

                case "[SOCKETS]":
                    return item.Stats.Sockets;
                case "[THORNS]":
                    return item.Stats.Thorns;
                case "[DMGREDPHYSICAL]":
                    return item.Stats.DamageReductionPhysicalPercent;

                case "[MAXARCPOWER]":
                    return item.Stats.MaxArcanePower;
                case "[HEALTHSPIRIT]":
                    return item.Stats.HealthPerSpiritSpent;
                case "[MAXSPIRIT]":
                    return item.Stats.MaxSpirit;
                case "[SPIRITREG]":
                    return item.Stats.SpiritRegen;
                case "[ARCONCRIT]":
                    return item.Stats.ArcaneOnCrit;
                case "[MAXFURY]":
                    return item.Stats.MaxFury;
                case "[MAXDISCIP]":
                    return item.Stats.MaxDiscipline;
                case "[HATREDREG]":
                    return item.Stats.HatredRegen;
                case "[MAXMANA]":
                    return item.Stats.MaxMana;
                case "[MANAREG]":
                    return item.Stats.ManaRegen;

                // +- Special functions -------------------------------------------------------+ 
                case "[MAXSTAT]":
                    return new float[] { item.Stats.Strength, item.Stats.Intelligence, item.Stats.Dexterity }.Max();
                case "[MAXSTATVIT]":
                    return new float[] { item.Stats.Strength, item.Stats.Intelligence, item.Stats.Dexterity }.Max() + item.Stats.Vitality;
                case "[STRVIT]":
                    return item.Stats.Strength + item.Stats.Vitality;
                case "[DEXVIT]":
                    return item.Stats.Dexterity + item.Stats.Vitality;
                case "[INTVIT]":
                    return item.Stats.Intelligence + item.Stats.Vitality;
                case "[MAXONERES]":
                    return new float[] { item.Stats.ResistArcane, item.Stats.ResistCold, item.Stats.ResistFire, item.Stats.ResistHoly, item.Stats.ResistLightning, item.Stats.ResistPhysical, item.Stats.ResistPoison }.Max();
                case "[TOTRES]":
                    return item.Stats.ResistArcane + item.Stats.ResistCold + item.Stats.ResistFire + item.Stats.ResistHoly + item.Stats.ResistLightning + item.Stats.ResistPhysical + item.Stats.ResistPoison + item.Stats.ResistAll;
                case "[DMGFACTOR]":
                    return item.Stats.AttackSpeedPercent + item.Stats.CritPercent * 2 + item.Stats.CritDamagePercent / 5 + (item.Stats.MinDamage + item.Stats.MaxDamage) / 20;
                case "[AVGDMG]":
                    return (item.Stats.MinDamage + item.Stats.MaxDamage) / 2;
                case "[OFFSTATS]":
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
                    return offstats;
                case "[DEFSTATS]":
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
                    return defstats;

                // +- Test functions ----------------------------------------------------------+ 
                case "[0]":
                    return 0;
                case "[1]":
                    return 1;
                case "[TEST]":
                    return "Test";
                case "[TRUE]":
                    return true;

                default:
                    checkFlag = false;
                    return null;
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private bool isAttribute(string str)
        {
            return str.Contains("[") && str.Contains("]");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="str1"></param>
        /// <param name="comparator"></param>
        /// <param name="str2"></param>
        /// <param name="checkFlag"></param>
        /// <returns></returns>
        private bool checkExpression(string str1, string comparator, string str2, ref bool checkFlag)
        {

            Object value = getValueFromString(str1, ref checkFlag);

            if (!checkFlag)
                return false;

            switch (value.GetType().Name)
            {
                case "String":
                    return compare((string)value, comparator, str2, ref checkFlag);
                case "Boolean":
                    return compare((bool)value, comparator, Boolean.Parse(str2), ref checkFlag);
                case "Int32":
                    return compare((int)value, comparator, Int32.Parse(str2), ref checkFlag);
                case "Single":
                    return compare((float)value, comparator, Single.Parse(str2), ref checkFlag);
                default:
                    checkFlag = false;
                    return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <param name="logType"></param>
        public void logOut(string str, LogType logType)
        {
            log = new StreamWriter(logPath + (logType.ToString().ToLower() + "_" + startTimestamp) + ".txt", true);
            log.WriteLine(DateTime.Now.ToString("G") + ": " + str);
            log.Close();
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
            else
                return item.Name + "," + item.ItemQualityLevel + "," + item.ItemType + "(" + item.Level + ")"+ "["+ item.InternalName + "]" + "[" + item.GameBalanceId + "]";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <param name="logType"></param>
        public void logItemFullTag(ACDItem item, LogType logType)
        {
            bool checkFlag = true;
            logOut("---" + getItemTag(item), logType);
            foreach (string str in strArrayString)
            {
                Object obj = getValueFromString(str, ref checkFlag);
                if (obj.GetType().Name == "String")
                    logOut("\t" + str + ": " + (string)obj, logType);
                else
                    logOut("INFO " + obj.GetType().Name + " strArrayString", logType);
            }
            foreach (string str in strArrayBool)
            {
                Object obj = getValueFromString(str, ref checkFlag);
                if (obj.GetType().Name == "Boolean")
                    logOut("\t" + str + ": " + (bool)obj, logType);
                else
                    logOut("INFO: " + obj.GetType().Name + " strArrayBool", logType);
            }
            foreach (string str in strArrayFloat)
            {
                Object obj = getValueFromString(str, ref checkFlag);
                if (obj.GetType().Name == "Single")
                {
                    float hold = (float)obj;
                    if (hold > 0)
                        logOut("\t" + str + ": " + hold, logType);
                }
                else if (obj.GetType().Name == "Int32")
                {
                    int hold = (int)obj;
                    if (hold > 0)
                        logOut("\t" + str + ": " + hold, logType);
                }
                else
                    logOut("INFO " + obj.GetType().Name + " strArrayFloat", logType);
            }
        }
    }
}
