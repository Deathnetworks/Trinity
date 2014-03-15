using System;
using System.Collections.Generic;
using System.Linq;
using Trinity.DbProvider;
using Trinity.Technicals;
using Zeta.Bot.Navigation;
using Zeta.Bot.Profile;
using Zeta.Common;
using Zeta.Game;
using Zeta.Game.Internals;
using Zeta.Game.Internals.SNO;
using Zeta.TreeSharp;
using Zeta.XmlEngine;
using Action = Zeta.TreeSharp.Action;
using Logger = Trinity.Technicals.Logger;

namespace Trinity.XmlTags
{
    [XmlElement("TrinityMoveToScene")]
    public class TrinityMoveToScene : ProfileBehavior
    {
        /// <summary>
        /// The Scene SNOId
        /// </summary>
        [XmlAttribute("sceneId")]
        public int SceneId { get; set; }

        /// <summary>
        /// The Scene Name, will match a sub-string
        /// </summary>
        [XmlAttribute("sceneName")]
        public string SceneName { get; set; }

        /// <summary>
        /// The distance the bot will mark the position as visited
        /// </summary>
        [XmlAttribute("pathPrecision")]
        public float PathPrecision { get; set; }

        /// <summary>
        /// The current player position
        /// </summary>
        private Vector3 myPos { get { return Trinity.Player.Position; } }

        /// <summary>
        /// The last scene SNOId we entered
        /// </summary>
        private int mySceneId = -1;
        /// <summary>
        /// The last position we updated the ISearchGridProvider at
        /// </summary>
        private Vector3 GPUpdatePosition = Vector3.Zero;

        /// <summary>
        /// Called when the profile behavior starts
        /// </summary>
        public override void OnStart()
        {
            Logger.Log(TrinityLogLevel.Info, LogCategory.ProfileTag, "TrinityMoveToScene OnStart()");

            if (PathPrecision == 0)
                PathPrecision = 15f;

            if (SceneId == 0 && SceneName.Trim() == String.Empty)
            {
                Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "TrinityMoveToScene: No sceneId or sceneName specified!");
                isDone = true;
            }

        }

        protected override Composite CreateBehavior()
        {
            return
            new Sequence(
                UpdateSearchGridProvider(),
                PrioritySceneCheck()
            );
        }

        /// <summary>
        /// Will find and move to Prioritized Scene's based on Scene SNOId or Name
        /// </summary>
        /// <returns></returns>
        private Composite PrioritySceneCheck()
        {
            return
            new Decorator(ret => !(SceneId == 0 && string.IsNullOrWhiteSpace(SceneName)),
                new Sequence(
                    new DecoratorContinue(ret => DateTime.UtcNow.Subtract(lastCheckedScenes).TotalMilliseconds > 1000,
                        new Sequence(
                            new Action(ret => lastCheckedScenes = DateTime.UtcNow),
                            new Action(ret => FindPrioritySceneTarget())
                        )
                    ),
                    new PrioritySelector(
                        new Decorator(ret => PrioritySceneTarget != Vector3.Zero,
                            new PrioritySelector(
                                new Decorator(ret => PrioritySceneTarget.Distance2D(myPos) <= PathPrecision,
                                    new Action(ret =>
                                    {
                                        Logger.Log(TrinityLogLevel.Info, LogCategory.ProfileTag, "Successfully navigated to scene {0} {1} center {2} Distance {3:0}",
                                            CurrentPriorityScene.Name, CurrentPriorityScene.SceneInfo.SNOId, PrioritySceneTarget, PrioritySceneTarget.Distance2D(myPos));
                                        isDone = true;
                                    })
                                ),
                                new Action(ret => MoveToPriorityScene())
                            )
                        ),
                        new Action(ret => {
                            Logger.Log(TrinityLogLevel.Info, LogCategory.ProfileTag, "Unable to navigate to Scene (not found) cancelling!");
                            isDone = true;
                        })
                    )
                )
            );
        }
        /// <summary>
        /// Handles actual movement to the Priority Scene
        /// </summary>
        private void MoveToPriorityScene()
        {
            MoveResult moveResult = PlayerMover.NavigateTo(PrioritySceneTarget);

            Logger.Log(TrinityLogLevel.Info, LogCategory.ProfileTag, "Moved to Scene ({0}) {1} - {2} Center {3} Distance {4:0}",
                moveResult, CurrentPriorityScene.Name, CurrentPriorityScene.SceneInfo.SNOId, PrioritySceneTarget, PrioritySceneTarget.Distance2D(myPos));


            if (moveResult == MoveResult.Failed || moveResult == MoveResult.PathGenerationFailed)
            {
                Logger.Log(TrinityLogLevel.Info, LogCategory.ProfileTag, "Unable to navigate to Scene ({0}) {1} - {2} Center {3} Distance {4:0}, cancelling!",
                    moveResult, CurrentPriorityScene.Name, CurrentPriorityScene.SceneInfo.SNOId, PrioritySceneTarget, PrioritySceneTarget.Distance2D(myPos));
                PrioritySceneMoveToFinished();
            }
        }

        /// <summary>
        /// Sets a priority scene as finished
        /// </summary>
        private void PrioritySceneMoveToFinished()
        {
            PriorityScenesInvestigated.Add(PrioritySceneSNOId);
            PrioritySceneSNOId = -1;
            PrioritySceneTarget = Vector3.Zero;
        }


        /// <summary>
        /// Updates the search grid provider as needed
        /// </summary>
        /// <returns></returns>
        private Composite UpdateSearchGridProvider()
        {
            return
            new DecoratorContinue(ret => mySceneId != Trinity.Player.SceneId || Vector3.Distance(myPos, GPUpdatePosition) > 150,
                new Sequence(
                    new Action(ret => mySceneId = Trinity.Player.SceneId),
                    new Action(ret => GPUpdatePosition = myPos),
                    new Action(ret => MiniMapMarker.UpdateFailedMarkers())
                )
            );
        }
        private Vector3 PrioritySceneTarget = Vector3.Zero;
        private int PrioritySceneSNOId = -1;
        private Scene CurrentPriorityScene = null;
        /// <summary>
        /// A list of Scene SNOId's that have already been investigated
        /// </summary>
        private List<int> PriorityScenesInvestigated = new List<int>();

        private DateTime lastCheckedScenes = DateTime.MinValue;
        /// <summary>
        /// Finds a navigable point in a priority scene
        /// </summary>
        private void FindPrioritySceneTarget()
        {
            if (SceneId == 0 && string.IsNullOrWhiteSpace(SceneName))
                return;

            if (PrioritySceneTarget != Vector3.Zero)
                return;

            bool foundPriorityScene = false;

            // find any matching priority scenes in scene manager - match by name or SNOId

            List<Scene> PScenes = new List<Scene>();
            var allScenes = ZetaDia.Scenes.GetScenes();

            if (allScenes.Any())
            {
                PScenes = GetPScenesBySceneId(allScenes);

                PScenes.AddRange(GetPScenesByName(allScenes));
            }
            else
            {
                return;
            }

            List<Scene> foundPriorityScenes = new List<Scene>();
            Dictionary<int, Vector3> foundPrioritySceneIndex = new Dictionary<int, Vector3>();

            foreach (Scene scene in PScenes)
            {
                try
                {
                    if (PriorityScenesInvestigated.Contains(scene.SceneInfo.SNOId))
                        continue;

                    foundPriorityScene = true;

                    NavZone navZone = scene.Mesh.Zone;
                    NavZoneDef zoneDef = navZone.NavZoneDef;

                    Vector2 zoneMin = navZone.ZoneMin;
                    Vector2 zoneMax = navZone.ZoneMax;

                    Vector3 zoneCenter = GetNavZoneCenter(navZone);

                    List<NavCell> NavCells = zoneDef.NavCells.Where(c => c.Flags.HasFlag(NavCellFlags.AllowWalk)).ToList();

                    if (!NavCells.Any())
                        continue;

                    NavCell bestCell = NavCells.OrderBy(c => GetNavCellCenter(c.Min, c.Max, navZone).Distance2D(zoneCenter)).FirstOrDefault();

                    if (bestCell != null)
                    {
                        foundPrioritySceneIndex.Add(scene.SceneInfo.SNOId, GetNavCellCenter(bestCell, navZone));
                        foundPriorityScenes.Add(scene);
                    }
                    else
                    {
                        Logger.Log(TrinityLogLevel.Info, LogCategory.ProfileTag, "Found Scene but could not find a navigable point!", true);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(ex.ToString());
                }
            }

            if (foundPrioritySceneIndex.Any())
            {
                KeyValuePair<int, Vector3> nearestPriorityScene = foundPrioritySceneIndex.OrderBy(s => s.Value.Distance2D(myPos)).FirstOrDefault();

                PrioritySceneSNOId = nearestPriorityScene.Key;
                PrioritySceneTarget = nearestPriorityScene.Value;
                CurrentPriorityScene = foundPriorityScenes.FirstOrDefault(s => s.SceneInfo.SNOId == PrioritySceneSNOId);

                Logger.Log(TrinityLogLevel.Info, LogCategory.ProfileTag, "Found Scene {0} - {1} Center {2} Distance {3:0}",
                    CurrentPriorityScene.Name, CurrentPriorityScene.SceneInfo.SNOId, PrioritySceneTarget, PrioritySceneTarget.Distance2D(myPos));
            }

            if (!foundPriorityScene)
            {
                PrioritySceneTarget = Vector3.Zero;
            }
        }

        private List<Scene> GetPScenesByName(IEnumerable<Scene> allScenes)
        {
            if (string.IsNullOrWhiteSpace(SceneName))
                return new List<Scene>();

            try
            {
                var matchingScenes = allScenes.Where(s => s.Name.ToLower().Contains(SceneName.ToLower()));

                return matchingScenes.ToList();
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString());
                return new List<Scene>();
            }
        }

        private List<Scene> GetPScenesBySceneId(IEnumerable<Scene> allScenes)
        {
            try
            {
                return allScenes.Where(s => s.SceneInfo.SNOId == SceneId).ToList();
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString());
                return new List<Scene>();
            }
        }

        /// <summary>
        /// Gets the center of a given Navigation Zone
        /// </summary>
        /// <param name="zone"></param>
        /// <returns></returns>
        private Vector3 GetNavZoneCenter(NavZone zone)
        {
            float X = zone.ZoneMin.X + ((zone.ZoneMax.X - zone.ZoneMin.X) / 2);
            float Y = zone.ZoneMin.Y + ((zone.ZoneMax.Y - zone.ZoneMin.Y) / 2);

            return new Vector3(X, Y, 0);
        }

        /// <summary>
        /// Gets the center of a given Navigation Cell
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="zone"></param>
        /// <returns></returns>
        private Vector3 GetNavCellCenter(NavCell cell, NavZone zone)
        {
            return GetNavCellCenter(cell.Min, cell.Max, zone);
        }

        /// <summary>
        /// Gets the center of a given box with min/max, adjusted for the Navigation Zone
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="zone"></param>
        /// <returns></returns>
        private Vector3 GetNavCellCenter(Vector3 min, Vector3 max, NavZone zone)
        {
            float X = zone.ZoneMin.X + min.X + ((max.X - min.X) / 2);
            float Y = zone.ZoneMin.Y + min.Y + ((max.Y - min.Y) / 2);
            float Z = min.Z + ((max.Z - min.Z) / 2);

            return new Vector3(X, Y, Z);
        }

        private bool isDone = false;
        /// <summary>
        /// When true, the next profile tag is used
        /// </summary>
        public override bool IsDone
        {
            get { return !IsActiveQuestStep || isDone; }
        }

        public override void ResetCachedDone()
        {
            isDone = false;
            base.ResetCachedDone();
        }

    }
}
