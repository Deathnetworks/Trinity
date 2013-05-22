using System;
using System.Diagnostics;
using Trinity.Technicals;
using Zeta.CommonBot;
using Zeta.CommonBot.Profile;
using Zeta.TreeSharp;
using Zeta.XmlEngine;
using Action = Zeta.TreeSharp.Action;

namespace Trinity.XmlTags
{
    // * TrinityUseReset - Resets a UseOnce tag as if it has never been used
    [XmlElement("TrinityRandomWait")]
    public class TrinityRandomWait : ProfileBehavior
    {
        private bool isDone = false;
        private int minDelay;
        private int maxDelay;
        private int delay;
        private string statusText;
        private Stopwatch timer = new Stopwatch();

        public override bool IsDone
        {
            get { return isDone; }
        }

        protected override Composite CreateBehavior()
        {
            Sequence RandomWaitSequence = new Sequence(
                new Action(ret => delay = new Random().Next(minDelay, maxDelay)),
                new Action(ret => statusText = String.Format("[XML Tag] Trinity Random Wait - Taking a break for {0:3} seconds.", delay)),
                new Action(ret => DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ProfileTag, statusText)),
                new Action(ctx => DoRandomWait(ctx)),
                new Action(ret => isDone = true)
            );

            return RandomWaitSequence;
        }

        private RunStatus DoRandomWait(object ctx)
        {
            if (!timer.IsRunning)
            {
                timer.Start();
                return RunStatus.Running;
            }
            else if (timer.IsRunning && timer.ElapsedMilliseconds < delay)
            {
                return RunStatus.Running;
            }
            else
            {
                timer.Reset();
                return RunStatus.Success;
            }
        }


        [XmlAttribute("min")]
        public int min
        {
            get
            {
                return minDelay;
            }
            set
            {
                minDelay = value;
            }
        }
        [XmlAttribute("max")]
        public int max
        {
            get
            {
                return maxDelay;
            }
            set
            {
                maxDelay = value;
            }
        }

        public override void ResetCachedDone()
        {
            isDone = false;
            base.ResetCachedDone();
        }
    }
}
