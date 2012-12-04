﻿using GilesTrinity.Cache;
using GilesTrinity.Technicals;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Zeta;

namespace GilesTrinity.ScriptedRules
{
    internal static class RulesManager
    {
        private static readonly IDictionary<int, ScriptedRule> _RuleCache = new Dictionary<int, ScriptedRule>();
        private static readonly IDictionary<string, string> _StandardKeyword;

        static RulesManager()
        {
            _StandardKeyword = FileManager.Load<string, string>("ItemRuleKeywords", "Keyword", "Code");
        }

        private static Delegate ConstructOperation(string lambdaExpression, Type inputParameterType, Type targetType)
        {
            int opi = lambdaExpression.IndexOf("=>");
            if (opi < 0)
                throw new Exception("No lambda operator =>");

            string param = lambdaExpression.Substring(0, opi);
            string body = lambdaExpression.Substring(opi + 2);
            ParameterExpression p = Expression.Parameter(inputParameterType, param);
            LambdaExpression lambda = System.Linq.Dynamic.DynamicExpression.ParseLambda(
                                        new ParameterExpression[] { p },
                                        targetType,
                                        body);

            return lambda.Compile();
        }

        /// <summary>
        /// Cleans All Rules.
        /// </summary>
        public static void Clean()
        {
            _RuleCache.Clear();
        }

        /// <summary>
        /// Loads the Scripted Rules.
        /// </summary>
        public static void LoadLootRules()
        {
            using (new PerformanceLogger("RulesManager.LoadLootRules"))
            {
            
                Clean();
                string filename = Path.Combine(FileManager.SpecificItemRulePath, "Loot.utr");
                if (!File.Exists(filename))
                {
                    DbHelper.Log(TrinityLogLevel.Normal, LogCategory.ScriptRule, "No Loot Rules file for BattleTag found in '{0}', General file is considered.", filename);
                    filename = Path.Combine(FileManager.ItemRulePath, "Loot.utr");
                }
                if (!File.Exists(filename))
                {
                    DbHelper.Log(TrinityLogLevel.Normal, LogCategory.ScriptRule, "No General Loot Rules file found in '{0}' by default all Item are routed to Trinity Scoring.", filename);
                    ScriptedRule rule = new ScriptedRule()
                                        {
                                            Name = "Default Error rule",
                                            Expression = "true",
                                            LambdaExpression = ConstructOperation("item => true", typeof(Object), typeof(bool)),
                                            Action = ScriptedRuleAction.Route
                                        };
                    _RuleCache.Add(1, rule);
                }
                ParseFile(filename, null);
            }
        }

        /// <summary>
        /// Determine if item should be pickup.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>
        /// <c>true</c> if item should be pickup, otherwise <c>false</c>
        /// </returns>
        public static bool ShouldPickup(CacheItem item)
        {
            if (_RuleCache.Count < 1)
            {
                LoadLootRules();
            }
            using (new PerformanceLogger("RulesManager.ShoulPickup"))
            {
                foreach (ScriptedRule rule in _RuleCache.Where(r => r.Value.Action != ScriptedRuleAction.Trash).Select(r => r.Value))
                {
                    if ((bool)rule.UnidentifiedLambdaExpression.DynamicInvoke(item))
                    {
                        DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ScriptRule, "{0} Pickup Item : '{1}'", rule.Name, rule.UnidentifiedExpression);
                        return true;
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// Determine if item should be stash.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>
        /// <c>true</c> if item should be stash, otherwise <c>false</c>
        /// </returns>
        public static bool? ShouldStash(CacheItem item)
        {
            if (_RuleCache.Count < 1)
            {
                LoadLootRules();
            }
            using (new PerformanceLogger("RulesManager.ShoulStash"))
            {
                foreach (ScriptedRule rule in _RuleCache.Where(r => r.Value.Action != ScriptedRuleAction.Trash).Select(r => r.Value))
                {
                    if ((bool)rule.UnidentifiedLambdaExpression.DynamicInvoke(item) && (bool)rule.LambdaExpression.DynamicInvoke(item))
                    {
                        DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ScriptRule, "{0} {2} Item : '{1}'", rule.Name, rule.UnidentifiedExpression, rule.Action);
                        switch (rule.Action)
                        {
                            case ScriptedRuleAction.Route:
                                return null;
                            case ScriptedRuleAction.Stash:
                                return true;
                            case ScriptedRuleAction.Trash:
                                return false;
                        }

                    }
                }
                return null;
            }
        }

        /// <summary>
        /// Parses Sripted Rule file.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <param name="existingMacros">The existing macros.</param>
        private static void ParseFile(string filename, IDictionary<string, string> existingMacros)
        {
            using (new PerformanceLogger("RulesManager.ParseFile"))
            {
                using (TextReader reader = File.OpenText(filename))
                {
                    IDictionary<string, string> listMacros = null;
                    if (existingMacros != null)
                    {
                        listMacros = new Dictionary<string, string>(existingMacros);
                    }
                    else
                    {
                        listMacros = new Dictionary<string, string>();
                    }

                    string line = reader.ReadLine();
                    while (line != null)
                    {
                        line = CleanLine(line);

                        if (line.Length > 1) // Tag + 1 char min
                        {
                            switch (line.Substring(0, 1))
                            {
                                case "@":
                                    // include file
                                    string fileToInclude = Path.Combine(Path.GetDirectoryName(filename), line.Substring(1));
                                    if (File.Exists(fileToInclude))
                                    {
                                        ParseFile(fileToInclude, listMacros);
                                    }
                                    else
                                    {
                                        DbHelper.Log(TrinityLogLevel.Normal, LogCategory.ScriptRule, "Include tag with file not found in '{0}'.", fileToInclude);
                                    }
                                    break;
                                case "&":
                                    // Macro
                                    KeyValuePair<string, string> macro = ParseMacro(line.Substring(1));
                                    if (!string.IsNullOrWhiteSpace(macro.Key))
                                    {
                                        if (listMacros.ContainsKey(macro.Key))
                                        {
                                            listMacros[macro.Key] = macro.Value;
                                        }
                                        else
                                        {
                                            listMacros.Add(macro);
                                        }
                                    }
                                    else
                                    {
                                        DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ScriptRule, "This macro line have incorrect format '%{0}'.", line);
                                    }
                                    break;
                                case "#":
                                    ScriptedRule rule = ParseRule(line.Substring(1));
                                    if (rule != null)
                                    {
                                        if (string.IsNullOrWhiteSpace(rule.Name))
                                        {
                                            rule.Name = string.Format("Rule {0:000}", _RuleCache.Count + 1);
                                        }
                                        _RuleCache.Add(_RuleCache.Count + 1, rule);
                                    }
                                    else
                                    {
                                        DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ScriptRule, "This rule have incorrect format '#{0}'.", line);
                                    }
                                    break;
                            }
                        }
                        // Next line
                        line = reader.ReadLine();
                    }
                    using (new PerformanceLogger("RulesManager.ParseFile - Replace Macro and Keyword"))
                    {
                        foreach (ScriptedRule rule in _RuleCache.Values.Where(r => r.LambdaExpression == null))
                        {
                            ReplaceMacro(rule, listMacros);
                            if (rule.LambdaExpression == null)
                            {
                                ReplaceMacro(rule, _StandardKeyword);
                                if (rule.LambdaExpression == null)
                                {
                                    System.Diagnostics.Debug.WriteLine("{0} Unidentified >> {1}", rule.Name, rule.UnidentifiedExpression);
                                    System.Diagnostics.Debug.WriteLine("{0} Identified   >> {1}", rule.Name, rule.Expression);
                                    rule.LambdaExpression = ConstructOperation(string.Format("item=>{0}", rule.Expression), typeof(CacheItem), typeof(bool));
                                    rule.UnidentifiedLambdaExpression = ConstructOperation(string.Format("item=>{0}", rule.UnidentifiedExpression), typeof(CacheItem), typeof(bool));
                                }

                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Formats the expression before parsing.
        /// </summary>
        /// <param name="rule">The rule.</param>
        private static void FormatExpression(ScriptedRule rule)
        {
            rule.Expression = (" " + rule.Expression.Replace("(", " ( ")
                                             .Replace(")", " ) ")
                                             .Replace("&", " & ")
                                             .Replace("|", " | ")
                                             .Replace("=", " = ")
                                             .Replace(":", " : ")
                                             .Replace(",", " , ")
                                             .Replace(";", " ; ")
                                             .Replace("<", " < ")
                                             .Replace(">", " > ")
                                             .Replace("^", " ^ ")
                                             .Replace("+", " + ")
                                             .Replace("-", " - ")
                                             .Replace("*", " * ")
                                             .Replace("/", " / ")
                                             .Replace("%", " % ")
                                             .Replace("!", " ! ")
                                             .Replace("\\", " \\ ")
                                             .Replace("=  =", "==")
                                             .Replace("!  =", " != ")
                                             .Replace("&  &", "&&")
                                             .Replace("|  |", "||")
                                             .Replace("<  =", "<=")
                                             .Replace(">  =", ">=")
                                             .Replace("^  =", "^=")
                                             .Replace("|  =", "|=")
                                             .Replace("&  =", "&=") + " ")
                                             .Replace("  ", " ");
            rule.UnidentifiedExpression = (" " + rule.UnidentifiedExpression.Replace("(", " ( ")
                                             .Replace(")", " ) ")
                                             .Replace("&", " & ")
                                             .Replace("|", " | ")
                                             .Replace("=", " = ")
                                             .Replace(":", " : ")
                                             .Replace(",", " , ")
                                             .Replace(";", " ; ")
                                             .Replace("<", " < ")
                                             .Replace(">", " > ")
                                             .Replace("^", " ^ ")
                                             .Replace("+", " + ")
                                             .Replace("-", " - ")
                                             .Replace("*", " * ")
                                             .Replace("/", " / ")
                                             .Replace("%", " % ")
                                             .Replace("!", " ! ")
                                             .Replace("\\", " \\ ")
                                             .Replace("=  =", "==")
                                             .Replace("!  =", " != ")
                                             .Replace("&  &", "&&")
                                             .Replace("|  |", "||")
                                             .Replace("<  =", "<=")
                                             .Replace(">  =", ">=")
                                             .Replace("^  =", "^=")
                                             .Replace("|  =", "|=")
                                             .Replace("&  =", "&=") + " ")
                                             .Replace("  ", " ");
        }

        /// <summary>
        /// Replaces the macro in expression by macro expression.
        /// </summary>
        /// <param name="rule">The rule.</param>
        /// <param name="listMacros">The list macros.</param>
        private static void ReplaceMacro(ScriptedRule rule, IDictionary<string, string> listMacros)
        {
            foreach (KeyValuePair<string, string> macro in listMacros.OrderByDescending(kp => kp.Key.Length)) 
            {
                FormatExpression(rule);
                if (rule.Expression.IndexOf(string.Format(" {0} ", macro.Key)) > -1 && !string.IsNullOrWhiteSpace(macro.Key))
                {
                    rule.Expression = rule.Expression.Replace(string.Format(" {0} ", macro.Key), string.Format(" {0} ", macro.Value));
                }

                if (rule.UnidentifiedExpression.IndexOf(string.Format(" {0} ", macro.Key)) > -1 && !string.IsNullOrWhiteSpace(macro.Key))
                {
                    rule.UnidentifiedExpression = rule.UnidentifiedExpression.Replace(string.Format(" {0} ", macro.Key), string.Format(" {0} ", macro.Value));
                }
            }
        }

        /// <summary>
        /// Cleans the line.
        /// </summary>
        /// <remarks>Remove comments and useless space</remarks>
        /// <param name="line">The line.</param>
        /// <returns>Cleared line</returns>
        private static string CleanLine(string line)
        {
            int commentPosition = line.IndexOf("//");
            if (commentPosition >= 0)
            {
                return line.Substring(0, commentPosition).Trim();
            }
            return line.Trim();
        }

        /// <summary>
        /// Parses the line with rule.
        /// </summary>
        /// <param name="line">The line.</param>
        /// <returns>The rule</returns>
        private static ScriptedRule ParseRule(string line)
        {
            string[] parts = line.Split(new char[] { '#' }, 4);
            if (parts != null && parts.Length >= 2)
            {
                ScriptedRule rule = new ScriptedRule();
                ScriptedRuleAction parsedAction = ScriptedRuleAction.Stash;
                if (parts.Length == 2)
                {
                    rule.UnidentifiedExpression = parts[0].Trim();
                    rule.Expression = parts[1].Trim();
                }
                else if (parts.Length == 3)
                {
                    if (Enum.TryParse(parts[2].Trim(), true, out parsedAction))
                    {
                        rule.UnidentifiedExpression = parts[0].Trim();
                        rule.Expression = parts[1].Trim();
                    }
                    else
                    {
                        rule.Name = parts[0].Trim();
                        rule.UnidentifiedExpression = parts[1].Trim();
                        rule.Expression = parts[2].Trim();
                    }
                }
                else
                {
                    rule.Name = parts[0].Trim();
                    rule.UnidentifiedExpression = parts[1].Trim();
                    rule.Expression = parts[2].Trim();
                    Enum.TryParse(parts[3].Trim(), true, out parsedAction);
                }
                rule.Action = parsedAction;
                return rule;
            }
            return null;
        }

        /// <summary>
        /// Parses the line with macro.
        /// </summary>
        /// <param name="line">The line.</param>
        /// <returns>The macro</returns>
        private static KeyValuePair<string, string> ParseMacro(string line)
        {
            string[] parts = line.Split(new char[] {'='}, 2);
            if (parts != null && parts.Length == 2)
            {
                return new KeyValuePair<string, string>(parts[0].Trim(), parts[1].Trim());
            }
            return new KeyValuePair<string,string>(null, null);
        }
    }
}
