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
        public static void SetDefaults()
        {
            // Barbarian 
            Set(new TVar("Barbarian.MinEnergyReserve", 56, "Ignore Pain Emergency Use Minimum Health Percent"));
            Set(new TVar("Barbarian.IgnorePain.MinHealth", 0.45f, "Ignore Pain Emergency Use Minimum Health Percent"));
            Set(new TVar("Barbarian.Avoidance.WOTB.Other", 0.3f, "General WOTB Avoidance health multiplier"));
            Set(new TVar("Barbarian.Avoidance.WOTB.Desecrator", 0.2f, "WOTB Desecrator health multiplier"));
            Set(new TVar("Barbarian.Avoidance.WOTB.Arcane", 0f, "WOTB Arcane health multiplier"));
            Set(new TVar("Barbarian.Avoidance.WOTB.IceBall", 0f, "WOTB IceBall health multiplier"));
            Set(new TVar("Barbarian.ThreatShout.Range", 25f, "Threating Shout Mob Range distance"));
            Set(new TVar("Barbarian.ThreatShout.OOCMaxFury", 25, "Threating Shout Out of Combat Max Fury"));
            Set(new TVar("Barbarian.WarCry.MaxFury", 60, "Maximum Fury to cast WarCry (with buff)"));
            Set(new TVar("Barbarian.Sprint.MinFury", 20, "Minimum Fury to try cast Sprint"));
            Set(new TVar("Barbarian.BattleRage.MinFury", 20, "Minimum Fury to try cast Battle Rage"));
            Set(new TVar("Barbarian.WOTB.MinFury", 50, "Minimum Fury to try cast WOTB"));
            Set(new TVar("Barbarian.WOTB.MinRange", 20f, "Elites in Range to try cast WOTB (with WOTB.MinCount) non-hard elites, non ignore elites"));
            Set(new TVar("Barbarian.WOTB.MinCount", 1, "Elite count to try cast WOTB (with WOTB.MinRange) non-hard elites, non ignore elites"));
            Set(new TVar("Barbarian.WOTB.RangeNear", 25f, "Nearby range check to use WOTB with Ignore Elites"));
            Set(new TVar("Barbarian.WOTB.RangeFar", 50f, "Nearby mob count check to use WOTB with Ignore Elites"));
            Set(new TVar("Barbarian.WOTB.CountNear", 3, "Extended range check to use WOTB with Ignore Elites"));
            Set(new TVar("Barbarian.WOTB.CountFar", 10, "Extended mob count check to use WOTB with Ignore Elites"));
            Set(new TVar("Barbarian.WOTB.HardEliteCountOverride", 4, "Will over-ride WOTB hard elite check when this many elites are present"));
            Set(new TVar("Barbarian.WOTB.HardEliteRangeOverride", 50f, "Range check distance for WOTB Hard elite override"));
            Set(new TVar("Barbarian.CallOfTheAncients.MinFury", 50, "Minimum Fury to try cast Call of the Ancients"));
            Set(new TVar("Barbarian.CallOfTheAncients.MinEliteRange", 25f, "Minimum range elites must be in to use COTA"));
            Set(new TVar("Barbarian.CallOfTheAncients.TickDelay", 4, "Pre and Post use Tick Delay"));
            Set(new TVar("Barbarian.Leap.UseRange", 35f, "Power Use Range"));
            Set(new TVar("Barbarian.SeismicSlam.UseRange", 40f, "Power Use Range"));
            Set(new TVar("Barbarian.AncientSpear.UseRange", 55f, "Power Use Range"));
            Set(new TVar("Barbarian.Whirlwind.UseRange", 10f, "Power Use Range"));
            Set(new TVar("Barbarian.Whirlwind.MinFury", 10, "Minimum Fury"));
            Set(new TVar("Barbarian.Whirlwind.ZigZagDistance", 15f, "Whirlwind ZigZag Range"));
            Set(new TVar("Barbarian.Whirlwind.ZigZagMaxTime", 1200, "Maximum time to keep a zig zag point before forcing a new point (millseconds)"));
            Set(new TVar("Barbarian.Bash.UseRange", 6f, "Power Use Range"));
            Set(new TVar("Barbarian.Frenzy.UseRange", 10f, "Power Use Range"));
            Set(new TVar("Barbarian.Cleave.UseRange", 6f, "Power Use Range"));
            Set(new TVar("Barbarian.Rend.UseRange", 10f, "Power Use Range"));
            Set(new TVar("Barbarian.WeaponThrow.UseRange", 25f, "Power Use Range"));
            Set(new TVar("Barbarian.GroundStomp.UseBelowHealthPct", 0.70f, "Use Ground Stomp below this health % (regardless of unit count)"));
            Set(new TVar("Barbarian.GroundStomp.EliteRange", 15f, "Use Ground Stomp Elites check Range"));
            Set(new TVar("Barbarian.GroundStomp.EliteCount", 1, "Use Ground Stomp when this many Elites in Range"));
            Set(new TVar("Barbarian.GroundStomp.TrashRange", 15f, "Use Ground Stomp Trash Mobs check Range"));
            Set(new TVar("Barbarian.GroundStomp.TrashCount", 4, "Use Ground Stomp when this many Trash Mobs in Range"));
            Set(new TVar("Barbarian.Revenge.TrashRange", 9f, "Use Revenge Trash Range"));
            Set(new TVar("Barbarian.Revenge.TrashCount", 1, "Use Revenge Trash Count"));
            Set(new TVar("Barbarian.FuriousCharge.EliteRange", 15f, "Furious Charge Elite Check Range"));
            Set(new TVar("Barbarian.FuriousCharge.EliteCount", 1, "Minimum Furious Charge Elite Count"));
            Set(new TVar("Barbarian.FuriousCharge.TrashRange", 15f, "Furious Charge Trash Check Range"));
            Set(new TVar("Barbarian.FuriousCharge.TrashCount", 3, "Minimum Furious Charge Trash Count"));
            Set(new TVar("Barbarian.FuriousCharge.UseRange", 32f, "Use Range"));
            Set(new TVar("Barbarian.FuriousCharge.MinExtraTargetDistance", 5f, "Extra distance added to target for Furious Charge direction"));
            Set(new TVar("Barbarian.Leap.EliteRange", 20f, "Leap Elite Check Range"));
            Set(new TVar("Barbarian.Leap.EliteCount", 1, "Minimum Leap Elite Count"));
            Set(new TVar("Barbarian.Leap.TrashRange", 20f, "Leap Trash Check Range"));
            Set(new TVar("Barbarian.Leap.TrashCount", 1, "Minimum Leap Trash Count"));
            Set(new TVar("Barbarian.Leap.MinExtraDistance", 4f, "Extra distance added to target for Leap direction"));
            Set(new TVar("Barbarian.Rend.MinNonBleedMobCount", 3, "Cast rend when this many mobs surrounding are not bleeding"));
            Set(new TVar("Barbarian.Rend.MinUseIntervalMillseconds", 1500, "Cast rend when this many mobs surrounding are not bleeding"));
            Set(new TVar("Barbarian.Rend.MaxRange", 10f, "Maximum Range for targets to be Rended"));
            Set(new TVar("Barbarian.Rend.MinFury", 20, "Minimum Fury"));
            Set(new TVar("Barbarian.Rend.TickDelay", 4, "Rend Pre and Post Tick Delay"));
            Set(new TVar("Barbarian.OverPower.MaxRange", 9f, "Maximum Range Overpower is triggered"));
            Set(new TVar("Barbarian.SeismicSlam.CurrentTargetRange", 40f, "Maximum Current Target range"));
            Set(new TVar("Barbarian.SeismicSlam.MinFury", 15, "Minimum Fury for Seismic Slam"));
            Set(new TVar("Barbarian.HammerOfTheAncients.UseRange", 10f, "Use Range"));              

            // Demon Hunter
            Set(new TVar("DemonHunter.MinEnergyReserve", 0, "Ignore Pain Emergency Use Minimum Health Percent"));

            // Monk
            Set(new TVar("Monk.Avoidance.Serenity", 0f, "Monk Serenity buff Avoidance health multiplier"));
            Set(new TVar("Monk.MinEnergyReserve", 0, "Ignore Pain Emergency Use Minimum Health Percent"));

            // Witch Doctor
            Set(new TVar("WitchDoctor.Avoidance.SpiritWalk", 0f, "WitchDoctor Spirit walk Avoidance health multiplier"));
            Set(new TVar("WitchDoctor.MinEnergyReserve", 0, "Ignore Pain Emergency Use Minimum Health Percent"));

            // Wizard
            Set(new TVar("Wizard.MinEnergyReserve", 0, "Ignore Pain Emergency Use Minimum Health Percent"));

        }

        /// <summary>
        /// Making this entire thing thread safe
        /// </summary>
        private static object sync = new object();

        /// <summary>
        /// Contains all of our configuration data
        /// </summary>
        private static Dictionary<string, TVar> data = new Dictionary<string, TVar>();

        /// <summary>
        /// Check if we have the given key in our dictionary
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool ContainsKey(string key)
        {
            lock (sync)
                return data.ContainsKey(key);
        }

        /// <summary>
        /// Returns a variable value as a string
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetString(string key)
        {
            lock (sync)
            {
                if (!ContainsKey(key))
                    return string.Empty;

                try
                {
                    if (data[key].Value is string)
                        return (string)data[key].Value;

                    string cast = (string)data[key].Value;
                    return cast;
                }
                catch (InvalidCastException)
                {
                    return string.Empty;
                }
            }
        }

        /// <summary>
        /// Returns a variable value as an Integer
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static int GetInt(string key)
        {
            lock (sync)
            {
                if (!ContainsKey(key))
                    return 0;

                try
                {
                    if (data[key].Value is int)
                        return (int)data[key].Value;

                    int cast = (int)data[key].Value;
                    return cast;
                }
                catch (InvalidCastException)
                {
                    return 0;
                }
            }
        }

        /// <summary>
        /// Returns a variable value as a float
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static float GetFloat(string key)
        {
            lock (sync)
            {
                if (!ContainsKey(key))
                    return 0f;

                try
                {
                    if (data[key].Value is float)
                        return (float)data[key].Value;

                    float cast = (float)data[key].Value;
                    return cast;
                }
                catch (InvalidCastException)
                {
                    return 0f;
                }

            }
        }

        /// <summary>
        /// Returns a variable value as a double
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static double GetDouble(string key)
        {
            lock (sync)
            {
                if (!ContainsKey(key))
                    return 0d;

                try
                {
                    if (data[key].Value is double)
                        return (double)data[key].Value;

                    double cast = (double)data[key].Value;
                    return cast;
                }
                catch (InvalidCastException)
                {
                    return 0d;
                }
            }
        }

        /// <summary>
        /// Returns a variable value as a Boolean
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool GetBool(string key)
        {
            if (!ContainsKey(key))
                return false;

            lock (sync)
            {
                try
                {
                    if (data[key].Value is bool)
                        return (bool)data[key].Value;

                    bool cast = (bool)data[key].Value;
                    return cast;
                }
                catch (InvalidCastException)
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Returns a variable value as a string
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string S(string key)
        {
            return GetString(key);
        }

        /// <summary>
        /// Returns a variable value as an Integer
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static int I(string key)
        {
            return GetInt(key);
        }

        /// <summary>
        /// Returns a variable value as a float
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static float F(string key)
        {
            return GetFloat(key);
        }

        /// <summary>
        /// Returns a variable value as a double
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static double D(string key)
        {
            return GetDouble(key);
        }

        /// <summary>
        /// Returns a variable value as a Boolean
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool B(string key)
        {
            return GetBool(key);
        }

        /// <summary>
        /// Gets the type of a given variable
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static Type GetType(string key)
        {
            lock (sync)
            {
                if (ContainsKey(key))
                    return data[key].Value.GetType();
            }
            return null;
        }

        /// <summary>
        /// Sets a Variable (stores it in the dictionary)
        /// </summary>
        /// <param name="var"></param>
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

        /// <summary>
        /// Gets a variable value
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static object GetValue(string key)
        {
            lock (sync)
            {
                if (ContainsKey(key))
                    return data[key].Value;
            }
            return null;
        }

        /// <summary>
        /// Restores the default value of a variable
        /// </summary>
        /// <param name="key"></param>
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
    }
}
