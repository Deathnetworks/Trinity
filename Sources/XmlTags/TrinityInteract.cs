using Trinity.Technicals;
using System.Linq;
using Zeta;
using Zeta.Common;
using Zeta.CommonBot.Profile;
using Zeta.Internals.Actors;
using Zeta.TreeSharp;
using Zeta.XmlEngine;
using System;

namespace Trinity.XmlTags
{
    // TrinityInteract attempts a blind object-use of an SNO without movement
    [XmlElement("TrinityInteract")]
    public class TrinityInteract : ProfileBehavior
    {
        private bool isDone = false;

        public override bool IsDone
        {
            get { return isDone; }
        }

        public override void OnStart()
        {
            Logger.Log(LogCategory.UserInformation, "[TrinityInteract] Started Tag; snoid=\"{0}\" name=\"{1}\" questId=\"{2}\" stepId=\"{3}\" worldId=\"{4}\" levelAreaId=\"{5}\"",
                this.ActorId, this.Name, this.QuestId, this.StepId, ZetaDia.CurrentWorldId, ZetaDia.CurrentLevelAreaId);
        }

        protected override Composite CreateBehavior()
        {
            return
            new Zeta.TreeSharp.Action(ret => DoInteract());
        }

        private RunStatus DoInteract()
        {
            if (!ZetaDia.IsInGame || ZetaDia.IsLoadingWorld || !ZetaDia.Me.IsValid)
            {
                Logger.Log(TrinityLogLevel.Verbose, LogCategory.UserInformation, "TrinityInteract called during invalid bot state");
                return RunStatus.Success;
            }

            Vector3 myPos = Trinity.Player.CurrentPosition;

            var interactTarget = ZetaDia.Actors.GetActorsOfType<DiaObject>(true, false)
                .Where(a => a.ActorSNO == ActorId)
                .OrderBy(a => a.Position.Distance2D(myPos)).FirstOrDefault();

            if (interactTarget != null && interactTarget.IsValid)
            {
                Zeta.CommonBot.GameEvents.FireWorldTransferStart();
                interactTarget.Interact();
            }

            isDone = true;

            return RunStatus.Success;
        }

        [XmlAttribute("snoid")]
        [XmlAttribute("actorId")]
        public int ActorId { get; set; }

        [XmlAttribute("name")]
        public string Name { get; set; }


        public override void ResetCachedDone()
        {
            isDone = false;
            base.ResetCachedDone();
        }
    }
}
