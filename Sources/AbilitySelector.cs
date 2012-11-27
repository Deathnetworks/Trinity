using Zeta.Common.Plugins;
using Zeta.Internals.Actors;
namespace GilesTrinity
{
    public partial class GilesTrinity : IPlugin
    {
        /// <summary>
        /// A default power in case we can't use anything else
        /// </summary>
        private static GilesPower defaultPower = new GilesPower(SNOPower.None, 0, vNullLocation, -1, -1, 0, 0, false);

        /// <summary>
        /// Returns an appropriately selected GilesPower and related information
        /// </summary>
        /// <param name="bCurrentlyAvoiding">Are we currently avoiding?</param>
        /// <param name="bOOCBuff">Buff Out Of Combat</param>
        /// <param name="bDestructiblePower">Is this for breaking destructables?</param>
        /// <returns></returns>
        internal static GilesPower GilesAbilitySelector(bool bCurrentlyAvoiding = false, bool bOOCBuff = false, bool bDestructiblePower = false)
        {
            // Refresh buffs once to save buff-check-spam
            GilesRefreshBuffs();
            // See if archon just appeared/disappeared, so update the hotbar
            if (bRefreshHotbarAbilities)
                GilesRefreshHotbar(GilesHasBuff(SNOPower.Wizard_Archon));
            // Extra height thingy, not REALLY used as it was originally going to be, will probably get phased out...
            float iThisHeight = iExtraHeight;
            // Switch based on the cached character class
            switch (iMyCachedActorClass)
            {
                // Barbs
                case ActorClass.Barbarian: 
                    return GetBarbarianPower(bCurrentlyAvoiding, bOOCBuff, bDestructiblePower);

                // Monks
                case ActorClass.Monk: 
                    return GetMonkPower(bCurrentlyAvoiding, bOOCBuff, bDestructiblePower);

                // Wizards
                case ActorClass.Wizard: 
                    return GetWizardPower(bCurrentlyAvoiding, bOOCBuff, bDestructiblePower);

                // Witch Doctors
                case ActorClass.WitchDoctor:
                    return GetWitchDoctorPower(bCurrentlyAvoiding, bOOCBuff, bDestructiblePower, iThisHeight);

                // Demon Hunters
                case ActorClass.DemonHunter:
                    return GetDemonHunterPower(bCurrentlyAvoiding, bOOCBuff, bDestructiblePower);
            }
            return defaultPower;
        }

        /// <summary>
        /// Returns true if we have the ability and the buff is up, or if we don't have the ability in our hotbar
        /// </summary>
        /// <param name="snoPower"></param>
        /// <returns></returns>
        internal static bool CheckAbilityAndBuff(SNOPower snoPower)
        {
            return
                (!hashPowerHotbarAbilities.Contains(snoPower) || (hashPowerHotbarAbilities.Contains(snoPower) && GilesHasBuff(snoPower)));

        }

    }
}
