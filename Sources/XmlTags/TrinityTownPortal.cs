using System.Diagnostics;
using Trinity.Technicals;
using Zeta;
using Zeta.CommonBot;
using Zeta.CommonBot.Profile;
using Zeta.Navigation;
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
        public static int WaitTime { get; set; }

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
            if (ZetaDia.Me.IsInTown)
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
            else
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
                new Decorator(ret => ZetaDia.Me.IsInTown && !DataDictionary.ForceTownPortalLevelAreaIds.Contains(Trinity.Player.LevelAreaId),
                    new Action(ret =>
                    {
                        ForceClearArea = false;
                        AreaClearTimer.Reset();
                        _IsDone = true;
                    })
                ),
                new Decorator(ret => !ZetaDia.Me.IsInTown && !ZetaDia.Me.CanUseTownPortal(),
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
                                Zeta.CommonBot.CommonBehaviors.MoveStop(),
                                new Sleep(1000)
                            )
                        ),
                        new Decorator(ret => PortalCastTimer.IsRunning && PortalCastTimer.ElapsedMilliseconds >= 7000,
                            new Sequence(
                                new Action(ret => {
                                    Technicals.Logger.LogNormal("Stuck casting town portal, moving a little");
                                    Navigator.MoveTo(NavHelper.FindSafeZone(Trinity.Player.Position, false, true));
                                    PortalCastTimer.Reset();
                                })
                            )
                        ),
                        new Sequence(
                            new DecoratorContinue(ret => PortalCastTimer.IsRunning && ZetaDia.Me.LoopingAnimationEndTime > 0, // Already casting, just wait
                                new Action()
                            ),
                            new Action(ret =>
                            {
                                PortalCastTimer.Restart();
                                GameEvents.FireWorldTransferStart();
                                ZetaDia.Me.UseTownPortal();
                            })
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
