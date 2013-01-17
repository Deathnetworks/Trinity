using System.Collections.Generic;
using System.Linq;
using GilesTrinity.DbProvider;
using GilesTrinity.Technicals;
using Zeta;
using Zeta.Common;
using Zeta.Navigation;
using Zeta.TreeSharp;
using Action = Zeta.TreeSharp.Action;

namespace GilesTrinity
{
    /// <summary>
    /// Class to help track MiniMapMarkers during Dungeon Exploration
    /// </summary>
    public class MiniMapMarker
    {
        public int MarkerNameHash { get; set; }
        public Vector3 Position { get; set; }
        public bool Visited { get; set; }
        public MiniMapMarker() { }
        internal static List<MiniMapMarker> KnownMarkers = new List<MiniMapMarker>();

        internal static bool AnyUnvisitedMarkers()
        {
            return MiniMapMarker.KnownMarkers.Any(m => !m.Visited);
        }

        internal static void SetNearbyMarkersVisited(Vector3 near, float pathPrecision)
        {
            foreach (MiniMapMarker marker in KnownMarkers.Where(m => Vector3.Distance(near, m.Position) <= pathPrecision))
            {
                DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.XmlTag, "Setting MiniMapMarker {0} as Visited", marker.MarkerNameHash);
                marker.Visited = true;
            }
        }

        internal static MiniMapMarker GetNearestUnvisitedMarker(Vector3 near)
        {
            return KnownMarkers.OrderBy(m => m.MarkerNameHash != 0).ThenBy(m => Vector3.Distance(near, m.Position)).FirstOrDefault(m => !m.Visited);
        }

        internal static void AddMarkersToList(int includeMarker = 0)
        {
            foreach (Zeta.Internals.MinimapMarker marker in ZetaDia.Minimap.Markers.CurrentWorldMarkers.Where(m => (m.NameHash == 0 || m.NameHash == includeMarker) && !KnownMarkers.Any(ml => ml.Position == m.Position)))
            {
                KnownMarkers.Add(new MiniMapMarker()
                {
                    MarkerNameHash = marker.NameHash,
                    Position = marker.Position,
                    Visited = false
                });
            }
        }

        internal static DecoratorContinue DetectMiniMapMarkers(int includeMarker = 0)
        {
            return
            new DecoratorContinue(ret => ZetaDia.Minimap.Markers.CurrentWorldMarkers.Any(m => (m.NameHash == 0 || m.NameHash == includeMarker) && !MiniMapMarker.KnownMarkers.Any(m2 => m2.Position != m.Position)),
                new Sequence(
                    new Action(ret => MiniMapMarker.AddMarkersToList(includeMarker))
                )
            );
        }

        internal static Decorator VisitMiniMapMarkers(Vector3 near, float markerDistance)
        {
            return
            new Decorator(ret => MiniMapMarker.AnyUnvisitedMarkers(),
                new Sequence(
                    new Action(ret => MiniMapMarker.SetNearbyMarkersVisited(ZetaDia.Me.Position, markerDistance)),
                    new Decorator(ret => MiniMapMarker.GetNearestUnvisitedMarker(ZetaDia.Me.Position) != null,
                        new Sequence(
                            new Action(ret => DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.XmlTag, "Moving to inspect nameHash 0 at {0} distance {1:0}",
                                MiniMapMarker.GetNearestUnvisitedMarker(ZetaDia.Me.Position).Position,
                                Vector3.Distance(ZetaDia.Me.Position, MiniMapMarker.GetNearestUnvisitedMarker(ZetaDia.Me.Position).Position))),
                            new Action(ret => PlayerMover.NavigateTo(MiniMapMarker.GetNearestUnvisitedMarker(near).Position))
                        )
                    )
                )
            );
        }

    }
}
