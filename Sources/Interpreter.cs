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

namespace GilesTrinity
{

    public class Interpreter
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
                                                "[DMGFACTOR]","[AVGDMG]"};

        public enum LogType { LOG, TRASH, ERROR };

        public enum InterpreterAction { KEEP, TRASH, NULL };

        string[] comparators = new string[] { "==", "!=", "<=", ">=", "<", ">" };

        ArrayList ruleSet;

        GilesCachedACDItem item;

        GilesItemType truetype;
        GilesBaseItemType basetype;

        TextWriter log;

        bool debugFlag = false, logFlag = false, trashLogFlag = false;

        string startTimestamp = DateTime.Now.ToString("ddMMyyyyHHmm");

        string customPath;

        string logPath;


        public Interpreter()
        {
            //init();
        }

        public void init()
        {
            customPath = GilesTrinity.sTrinityPluginPath + @"Specification\";
            logPath = GilesTrinity.sTrinityPluginPath + @"Log\";
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
                logOut("... reading file: " + itemFileName, LogType.LOG);
                readItemFile(new StreamReader(customPath + itemFileName));
            }
            logOut("initialized " + ruleSet.Count + " itemrulesets!", LogType.LOG);
            Logging.Write("initialized " + ruleSet.Count + " itemrulesets!");
            Logging.Write("finished initializing Item Rule Set!");
        }

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
                    logOut(ruleSet.Count + ":" + rule[0], LogType.LOG);
                }
                else if (rule[0].Length > 0)
                {
                    logOut("#WARNING(BAD LINE): " + rule[0], LogType.LOG);
                }
            }
        }

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
                logOut(nullTest, LogType.LOG);
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

        private string interpretConfig(string str)
        {
            string fileName = "";
            if (hasComparator(str) != null)
            {
                string comparator = hasComparator(str);
                string[] strings = str.Split(new string[] { comparator }, StringSplitOptions.None);
                fileName = interpret(strings[0], comparator, strings[1]);
            }
            return fileName;
        }

        private string interpret(string str1, string comparator, string str2)
        {
            string fileName = "";
            switch (str1)
            {
                case "[DEBUG]":
                    debugFlag = Boolean.Parse(str2);
                    logOut("Debugging set to " + logFlag, LogType.LOG);
                    break;
                case "[LOG]":
                    logFlag = Boolean.Parse(str2);
                    logOut("Logging set to " + logFlag, LogType.LOG);
                    break;
                case "[TRASHLOG]":
                    trashLogFlag = Boolean.Parse(str2);
                    logOut("Logging trashed legendarys set to " + logFlag, LogType.LOG);
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

        public InterpreterAction checkItem(GilesCachedACDItem item, GilesItemType TrueItemType, GilesBaseItemType thisGilesBaseType)
        {
            this.item = item;
            truetype = TrueItemType;
            basetype = thisGilesBaseType;

            bool checkFlag = true;

            InterpreterAction action = InterpreterAction.NULL;

            foreach (string str in ruleSet)
            {
                action = interpret(str, ref checkFlag);

                if (!checkFlag)
                {
                    logOut("#WARNING(RULE): " + str, LogType.LOG);
                    logItemFullTag(item, LogType.LOG);
                    return InterpreterAction.NULL;
                }

                if (action != InterpreterAction.NULL)
                {
                    loggingAction(item, action, str);
                    break;
                }
            }
            return action;
        }

        private void loggingAction(GilesCachedACDItem item, InterpreterAction action, string rule)
        {
            if (trashLogFlag && action == InterpreterAction.TRASH && (item == null || item.Quality == ItemQuality.Legendary))
            {
                logOut("-----------------------------------------------------------------", LogType.TRASH);
                if (item != null) logOut(action.ToString() + ": " + getItemTag(item), LogType.TRASH);
                logOut("Rule:" + rule, LogType.TRASH);
                if (item != null) logItemFullTag(item, LogType.TRASH);
            }
            else if (logFlag && action == InterpreterAction.KEEP || debugFlag)
            {
                logOut("-----------------------------------------------------------------", LogType.LOG);
                if (item != null) logOut(action.ToString() + ": " + getItemTag(item), LogType.LOG);
                logOut("Rule:" + rule, LogType.LOG);
                if (debugFlag)
                    if (item != null) logItemFullTag(item, LogType.LOG);
            }
        }

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
            else if (hasComparator(str) != null)
            {
                string comparator = hasComparator(str);
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

        private InterpreterAction getInterpreterAction(string str)
        {
            foreach (InterpreterAction action in Enum.GetValues(typeof(InterpreterAction)))
                if (str.IndexOf(action.ToString()) != -1)
                    return action;
            return InterpreterAction.NULL;
        }

        private string hasComparator(string str)
        {
            foreach (string comparator in comparators)
                if (str.IndexOf(comparator) != -1)
                    return comparator;
            return null;
        }

        private Object getValueFromString(string str, ref bool checkFlag)
        {
            switch (str)
            {
                case "[BASETYPE]":
                    return basetype.ToString();
                case "[TYPE]":
                    return truetype.ToString();
                case "[QUALITY]":
                   return Regex.Replace(item.Quality.ToString(), @"[\d-]", string.Empty);
                case "[NAME]":
                    return item.RealName.ToString().Replace(" ","");
                case "[LEVEL]":
                    return item.Level;
                case "[ONEHAND]":
                    return item.OneHanded;
                case "[TWOHAND]":
                    return item.TwoHanded;
                case "[STR]":
                    return item.Strength;
                case "[DEX]":
                    return item.Dexterity;
                case "[INT]":
                    return item.Intelligence;
                case "[VIT]":
                    return item.Vitality;
                case "[AS%]":
                    return item.AttackSpeedPercent;
                case "[MS%]":
                    return item.MovementSpeed;

                case "[LIFE%]":
                    return item.LifePercent;
                case "[LS%]":
                    return item.LifeSteal;
                case "[LOH]":
                    return item.LifeOnHit;
                case "[REGEN]":
                    return item.HealthPerSecond;
                case "[GLOBEBONUS]":
                    return item.HealthGlobeBonus;

                case "[DPS]":
                    return item.WeaponDamagePerSecond;
                case "[WEAPAS]":
                    return item.WeaponAttacksPerSecond;
                case "[WEAPMAXDMG]":
                    return item.WeaponMaxDamage;
                case "[WEAPMINDMG]":
                    return item.WeaponMinDamage;
                case "[CRIT%]":
                    return item.CritPercent;
                case "[CRITDMG%]":
                    return item.CritDamagePercent;
                case "[BLOCK%]":
                    return item.BlockChance;
                case "[MINDMG]":
                    return item.MinDamage;
                case "[MAXDMG]":
                    return item.MaxDamage;

                case "[ALLRES]":
                    return item.ResistAll;
                case "[RESPHYSICAL]":
                    return item.ResistPhysical;
                case "[RESFIRE]":
                    return item.ResistFire;
                case "[RESLIGHTNING]":
                    return item.ResistLightning;
                case "[RESHOLY]":
                    return item.ResistHoly;
                case "[RESARCAN]":
                    return item.ResistArcane;
                case "[RESCOLD]":
                    return item.ResistCold;
                case "[RESPOISON]":
                    return item.ResistPoison;

                case "[FIREDMG%]":
                    return item.FireDamagePercent;
                case "[LIGHTNINGDMG%]":
                    return item.LightningDamagePercent;
                case "[COLDDMG%]":
                    return item.ColdDamagePercent;
                case "[POISONDMG%]":
                    return item.PoisonDamagePercent;
                case "[ARCANEDMG%]":
                    return item.ArcaneDamagePercent;
                case "[HOLYDMG%]":
                    return item.HolyDamagePercent;
                case "[ARMOR]":
                    return item.Armor;
                case "[ARMORBONUS]":
                    return item.ArmorBonus;
                case "[ARMORTOT]":
                    return item.ArmorTotal;
                case "[GF%]":
                    return item.GoldFind;
                case "[MF%]":
                    return item.MagicFind;
                case "[PICKUP]":
                    return item.PickUpRadius;

                case "[SOCKETS]":
                    return item.Sockets;
                case "[THORNS]":
                    return item.Thorns;
                case "[DMGREDPHYSICAL]":
                    return item.DamageReductionPhysicalPercent;

                case "[MAXARCPOWER]":
                    return item.MaxArcanePower;
                case "[HEALTHSPIRIT]":
                    return item.HealthPerSpiritSpent;
                case "[MAXSPIRIT]":
                    return item.MaxSpirit;
                case "[SPIRITREG]":
                    return item.SpiritRegen;
                case "[ARCONCRIT]":
                    return item.ArcaneOnCrit;
                case "[MAXFURY]":
                    return item.MaxFury;
                case "[MAXDISCIP]":
                    return item.MaxDiscipline;
                case "[HATREDREG]":
                    return item.HatredRegen;
                case "[MAXMANA]":
                    return item.MaxMana;
                case "[MANAREG]":
                    return item.ManaRegen;

                /* ---------------------- *
                 * SPECIAL FUNCTIONS      *
                 * ---------------------- */
                case "[MAXSTAT]":
                    return new float[] { item.Strength, item.Intelligence, item.Dexterity }.Max();
                case "[MAXSTATVIT]":
                    return new float[] { item.Strength, item.Intelligence, item.Dexterity }.Max() + item.Vitality;
                case "[MAXONERES]":
                    return new float[] { item.ResistArcane, item.ResistCold, item.ResistFire, item.ResistHoly, item.ResistLightning, item.ResistPhysical, item.ResistPoison }.Max();
                
                case "[TOTRES]":
                    return item.ResistArcane + item.ResistCold + item.ResistFire + item.ResistHoly + item.ResistLightning + item.ResistPhysical + item.ResistPoison + item.ResistAll;
                case "[DMGFACTOR]":
                    return item.AttackSpeedPercent / 2 + item.CritPercent * 2 + item.CritDamagePercent / 5 + (item.MinDamage + item.MaxDamage) / 20;
                
                case "[STRVIT]":
                    return item.Strength + item.Vitality;
                case "[DEXVIT]":
                    return item.Dexterity + item.Vitality;
                case "[INTVIT]":
                    return item.Intelligence + item.Vitality;

                case "[AVGDMG]":
                    return (item.MinDamage + item.MaxDamage)/2;

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

        public void logOut(string str, LogType logType)
        {
            log = new StreamWriter(logPath + (logType.ToString().ToLower() + "_" + startTimestamp) + ".txt", true);
            log.WriteLine(DateTime.Now.ToString("ddMMyyyyHHmmss") + ": " + str);
            log.Close();
        }

        public string getItemTag(GilesCachedACDItem item)
        {
            if (item == null)
                return "nullItem";
            else
                return item.RealName + "," + item.Quality + "," + item.DBItemType + " (" + item.Level + ") " + item.BalanceID;
        }

        public void logItemFullTag(GilesCachedACDItem item, LogType logType)
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
                    float hold = (float) obj;
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
