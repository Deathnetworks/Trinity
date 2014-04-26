

using System.Diagnostics;
using Trinity.Technicals;
using Zeta.Bot;
using Zeta.Bot.Navigation;
using Zeta.Bot.Profile;
using Zeta.Game;
using Zeta.TreeSharp;
using Zeta.XmlEngine;
using Action = Zeta.TreeSharp.Action;

namespace Trinity.XmlTags
{
    // TrinityTownRun forces a town-run request
    [XmlElement("TrinityTownPortal")]
    public class TrinityTownPortal : ProfileBehavior
    {
        public static int DefaultWaitTime = -1;

        [XmlAttribute("waitTime")]
        [XmlAttribute("wait")]
        public int WaitTime { get; set; }

        public static Stopwatch AreaClearTimer = null;
        public static Stopwatch PortalCastTimer = null;
        public static bool ForceClearArea = false;

        private double _StartHealth = -1;

        private bool _IsDone = false;

        public override bool IsDone
        {
            get { return _IsDone || !IsActiveQuestStep; }
        }

        public TrinityTownPortal()
        {
            AreaClearTimer = new Stopwatch();
            PortalCastTimer = new Stopwatch();
        }

        public override void OnStart()
        {
            if (ZetaDia.IsInTown)
            {
                _IsDone = true;
                return;
            }

            ForceClearArea = true;
            AreaClearTimer.Reset();
            AreaClearTimer.Start();
            DefaultWaitTime = V.I("XmlTag.TrinityTownPortal.DefaultWaitTime");
            int forceWaitTime = V.I("XmlTag.TrinityTownPortal.ForceWaitTime");
            if (WaitTime <= 0 && forceWaitTime == -1)
            {
                WaitTime = DefaultWaitTime;
            }
            else if (WaitTime <= 0 && forceWaitTime > 0)
            {
                WaitTime = forceWaitTime;
            }
            _StartHealth = ZetaDia.Me.HitpointsCurrent;
            Logger.Log(LogCategory.UserInformation, "TrinityTownPortal started - clearing area, waitTime={0}, startHealth={1:0}", WaitTime, _StartHealth);
        }

        protected override Composite CreateBehavior()
        {
            return new
            PrioritySelector(
                new Decorator(ret => ZetaDia.IsLoadingWorld,
                    new Action()
                ),
                new Decorator(ret => ZetaDia.IsInTown && !DataDictionary.ForceTownPortalLevelAreaIds.Contains(Trinity.Player.LevelAreaId),
                    new Action(ret =>
                    {
                        ForceClearArea = false;
                        AreaClearTimer.Reset();
                        _IsDone = true;
                    })
                ),
                new Decorator(ret => !ZetaDia.IsInTown && !ZetaDia.Me.CanUseTownPortal(),
                    new Action(ret =>
                    {
                        ForceClearArea = false;
                        AreaClearTimer.Reset();
                        _IsDone = true;
                    })
                ),
                new Decorator(ret => ZetaDia.Me.HitpointsCurrent < _StartHealth,
                    new Action(ret =>
                    {
                        _StartHealth = ZetaDia.Me.HitpointsCurrent;
                        AreaClearTimer.Restart();
                        ForceClearArea = true;
                    })
                ),
                new Decorator(ret => AreaClearTimer.IsRunning,
                    new PrioritySelector(
                        new Decorator(ret => AreaClearTimer.ElapsedMilliseconds <= WaitTime,
                            new Action(ret => ForceClearArea = true) // returns RunStatus.Success
                        ),
                        new Decorator(ret => AreaClearTimer.ElapsedMilliseconds > WaitTime,
                            new Action(ret =>
                            {
                                Logger.Log(LogCategory.UserInformation, "Town Portal timer finished");
                                ForceClearArea = false;
                                AreaClearTimer.Reset();
                            })
                        )
                    )
                ),

                new Decorator(ret => !ForceClearArea,
                    new PrioritySelector(

                        new Decorator(ret => ZetaDia.Me.Movement.IsMoving,
                            new Sequence(
                                Zeta.Bot.CommonBehaviors.MoveStop(),
                                new Sleep(1000)
                            )
                        ),

                        new Decorator(ret => PortalCastTimer.IsRunning && PortalCastTimer.ElapsedMilliseconds >= 7000,
                            new Sequence(
                                new Action(ret =>
                                {
                                    Technicals.Logger.LogNormal("Stuck casting town portal, moving a little");
                                    Navigator.MoveTo(NavHelper.SimpleUnstucker(),"Unstuck Position");
                                    PortalCastTimer.Reset();
                                })
                            )
                        ),


                        new Decorator(ret => PortalCastTimer.IsRunning && ZetaDia.Me.LoopingAnimationEndTime > 0, // Already casting, just wait
                            new Action(ret => RunStatus.Success)
                        ),

                        new Sequence(
                            new Action(ret =>
                            {
                                PortalCastTimer.Restart();
                                GameEvents.FireWorldTransferStart();
                                ZetaDia.Me.UseTownPortal();
                            }),

                            new WaitContinue(3, ret => ZetaDia.Me.LoopingAnimationEndTime > 0,
                                new Sleep(100)
                            )
                        )
                    )
                )
            );
        }

        public override void ResetCachedDone()
        {
            _IsDone = false;
            base.ResetCachedDone();
        }
    }
}

