using System;
using System.Collections.Generic;
using System.Linq;
using Zeta.Bot.Profile;
using Zeta.TreeSharp;
using Zeta.Game;
using Zeta.Game.Internals.Actors;
using Zeta.XmlEngine;
using Action = Zeta.TreeSharp.Action;

using Trinity.Technicals;

namespace Trinity.ProfileTags
{
    /// <summary>
    /// TrinityOOCToggle will turn OOC movement on or off. To set true: SetTrue = "true"
    /// </summary>
    [XmlElement("TrinityOOCToggle")]
    class TrinityOOCToggle : ProfileBehavior
    {
        private bool isDone;
        public override bool IsDone
        {
            get { return isDone; }
        }

        [XmlAttribute("SetTrue")]
        public bool onOff { get; set; }

        protected override Composite CreateBehavior()
        {
            return new Action(ret => ToggleOOC());
        }

        private void ToggleOOC()
        {
            if (onOff)
            {
                if (ZetaDia.Me.ActorClass == Zeta.Game.ActorClass.Witchdoctor && isAvailable(SNOPower.Witchdoctor_Hex))
                {
                    if (isAvailable(SNOPower.Witchdoctor_Horrify))
                    {
                        ZetaDia.Me.UsePower(SNOPower.Witchdoctor_Horrify, Trinity.Player.Position, Trinity.CurrentWorldDynamicId, -1);
                    }
                    if (isAvailable(SNOPower.Witchdoctor_SpiritWalk))
                    {
                        ZetaDia.Me.UsePower(SNOPower.Witchdoctor_SpiritWalk, Trinity.Player.Position, Trinity.CurrentWorldDynamicId, -1);
                    }
                    ZetaDia.Me.UsePower(SNOPower.Witchdoctor_Hex, Trinity.Player.Position, Trinity.CurrentWorldDynamicId, -1);
                }
                if (ZetaDia.Me.ActorClass == Zeta.Game.ActorClass.Wizard)
                {
                    if (isAvailable(SNOPower.Wizard_MirrorImage))
                    {
                        ZetaDia.Me.UsePower(SNOPower.Wizard_MirrorImage, Trinity.Player.Position, Trinity.CurrentWorldDynamicId, -1);
                    }
                    if (isAvailable(SNOPower.Wizard_SlowTime))
                    {
                        ZetaDia.Me.UsePower(SNOPower.Wizard_SlowTime, Trinity.Player.Position, Trinity.CurrentWorldDynamicId, -1);
                    }
                }
                if (ZetaDia.Me.ActorClass == Zeta.Game.ActorClass.Crusader)
                {
                    if (isAvailable(SNOPower.X1_Crusader_AkaratsChampion))
                    {
                        ZetaDia.Me.UsePower(SNOPower.X1_Crusader_AkaratsChampion, Trinity.Player.Position, Trinity.CurrentWorldDynamicId, -1);
                    }
                    if (isAvailable(SNOPower.X1_Crusader_LawsOfHope2))
                    {
                        ZetaDia.Me.UsePower(SNOPower.X1_Crusader_LawsOfHope2, Trinity.Player.Position, Trinity.CurrentWorldDynamicId, -1);
                    }
                    if (isAvailable(SNOPower.X1_Crusader_SteedCharge))
                    {
                        ZetaDia.Me.UsePower(SNOPower.X1_Crusader_SteedCharge, Trinity.Player.Position, Trinity.CurrentWorldDynamicId, -1);
                    }
                }
                if (ZetaDia.Me.ActorClass == Zeta.Game.ActorClass.DemonHunter)
                {
                    if (isAvailable(SNOPower.DemonHunter_Preparation) && Trinity.Player.SecondaryResource <= 15 && Trinity.TimeSinceUse(SNOPower.DemonHunter_Preparation) >= 1000)
                    {
                        ZetaDia.Me.UsePower(SNOPower.DemonHunter_Preparation, Trinity.Player.Position, Trinity.CurrentWorldDynamicId, -1);
                    }

                    if (isAvailable(SNOPower.DemonHunter_SmokeScreen))
                    {
                        ZetaDia.Me.UsePower(SNOPower.DemonHunter_SmokeScreen, Trinity.Player.Position, Trinity.CurrentWorldDynamicId, -1);
                    }

                    if (isAvailable(SNOPower.DemonHunter_Vault))
                    {
                        ZetaDia.Me.UsePower(SNOPower.DemonHunter_Vault, Trinity.Player.Position, Trinity.CurrentWorldDynamicId, -1);
                    }
                }
                Trinity.Settings.Combat.Misc.AllowOOCMovement = true;
                Logger.LogDebug("Turned OOC on.");
            }
            else if (!onOff)
            {
                if (ZetaDia.Me.ActorClass == Zeta.Game.ActorClass.Barbarian)
                {
                    Trinity.Settings.Combat.Misc.AllowOOCMovement = true;
                }
                else
                {
                    Trinity.Settings.Combat.Misc.AllowOOCMovement = false;
                    Logger.LogDebug("Turned OOC off.");
                }  
            }
            else
            {
                Logger.LogDebug("Unknown error trying to toggle OOC.");
            }

            isDone = true;
        }

        private bool isAvailable(SNOPower power)
        {
            if (!Trinity.GetHasBuff(power) && Trinity.Hotbar.Contains(power))
                return true;
            return false;
        }
    }
}