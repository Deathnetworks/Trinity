using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Zeta.Game.Internals.Actors;

namespace Trinity.Combat.Abilities
{
    class WizardCombat : CombatBase
    {
        public static int SerpentSparkerId = 272084;

        public static Config.Combat.WizardSetting WizardSettings
        {
            get { return Trinity.Settings.Combat.Wizard; }
        }

        public static TrinityPower GetPower()
        {
            TrinityPower power = null;

            // In Combat, Avoiding
            if (!UseOOCBuff && IsCurrentlyAvoiding)
            {


            }

            // In combat, Not Avoiding
            if (!UseOOCBuff && !IsCurrentlyAvoiding && CurrentTarget != null)
            {

                // Default Attacks
                if (IsNull(null))
                    power = DefaultPower;
            }

            // Buffs
            if (UseOOCBuff)
            {

            }

            return power;
        }


        private static TrinityPower DestroyObjectPower
        {
            get
            {
                if (CanCast(SNOPower.Wizard_WaveOfForce) && Player.PrimaryResource >= 25)
                    return new TrinityPower(SNOPower.Wizard_WaveOfForce, 9f);

                if (CanCast(SNOPower.Wizard_EnergyTwister) && Player.PrimaryResource >= 35)
                    return new TrinityPower(SNOPower.Wizard_EnergyTwister, 9f);

                if (CanCast(SNOPower.Wizard_ArcaneOrb))
                    return new TrinityPower(SNOPower.Wizard_ArcaneOrb, 35f);

                if (CanCast(SNOPower.Wizard_MagicMissile))
                    return new TrinityPower(SNOPower.Wizard_MagicMissile, 15f);

                if (CanCast(SNOPower.Wizard_ShockPulse))
                    return new TrinityPower(SNOPower.Wizard_ShockPulse, 10f);

                if (CanCast(SNOPower.Wizard_SpectralBlade))
                    return new TrinityPower(SNOPower.Wizard_SpectralBlade, 5f);

                if (CanCast(SNOPower.Wizard_Electrocute))
                    return new TrinityPower(SNOPower.Wizard_Electrocute, 9f);

                if (CanCast(SNOPower.Wizard_ArcaneTorrent))
                    return new TrinityPower(SNOPower.Wizard_ArcaneTorrent, 9f);

                if (CanCast(SNOPower.Wizard_Blizzard))
                    return new TrinityPower(SNOPower.Wizard_Blizzard, 9f);
                return DefaultPower;
            }
        }

        
    }
}
