using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Trinity
{
    /// <summary>
    /// Trinity Variables
    /// </summary>
    public static class V
    {
        private static object sync = new object();

        private static Dictionary<string, TVar> data = new Dictionary<string, TVar>();

        public static bool ContainsKey(string key)
        {
            lock (sync)
                return data.ContainsKey(key);
        }

        public static string GetString(string key)
        {
            lock (sync)
            {
                if (data.ContainsKey(key) && data[key].Value is string)
                    return (string)data[key].Value;
            }
            return string.Empty;
        }

        public static int GetInt(string key)
        {
            lock (sync)
            {
                if (data.ContainsKey(key) && data[key].Value is int)
                    return (int)data[key].Value;
            }
            return 0;
        }

        public static float GetFloat(string key)
        {
            lock (sync)
            {
                if (data.ContainsKey(key) && data[key].Value is float)
                    return (float)data[key].Value;
            }
            return 0f;
        }

        public static double GetDouble(string key)
        {
            lock (sync)
            {
                if (data.ContainsKey(key) && data[key].Value is double)
                    return (double)data[key].Value;
            }
            return 0d;
        }

        public static string S(string key)
        {
            return GetString(key);
        }

        public static int I(string key)
        {
            return GetInt(key);
        }
        public static float F(string key)
        {
            return GetFloat(key);
        }

        public static double D(string key)
        {
            return GetDouble(key);
        }

        public static Type GetType(string key)
        {
            lock (sync)
            {
                if (ContainsKey(key))
                    return data[key].Value.GetType();
            }
            return null;
        }

        public static void Set(TVar var)
        {
            lock (sync)
            {
                if (ContainsKey(var.Name))
                    data[var.Name] = var;
                else
                    data.Add(var.Name, var);
            }
        }

        public static object Get(string key)
        {
            lock (sync)
            {
                if (ContainsKey(key))
                    return data[key].Value;
            }
            return null;
        }

        public static void SetDefaultValue(string key)
        {
            lock (sync)
            {
                if (ContainsKey(key))
                {
                    TVar v = data[key];
                    v.Value = v.DefaultValue;
                    data[key] = v;
                }
            }
        }


        public static void SetDefaults()
        {
            Set(new TVar("Barbarian.Avoidance.WOTB.Other", 0.3f, "General WOTB Avoidance health multiplier"));
            Set(new TVar("Barbarian.Avoidance.WOTB.Desecrator", 0.2f, "WOTB Desecrator health multiplier"));
            Set(new TVar("Barbarian.Avoidance.WOTB.Arcane", 0f, "WOTB Arcane health multiplier"));
            Set(new TVar("Barbarian.Avoidance.WOTB.IceBall", 0f, "WOTB IceBall health multiplier"));
            Set(new TVar("Monk.Avoidance.Serenity", 0f, "Monk Serenity buff Avoidance health multiplier"));
            Set(new TVar("WitchDoctor.Avoidance.SpiritWalk", 0f, "WitchDoctor Spirit walk Avoidance health multiplier"));
        }
    }
}
