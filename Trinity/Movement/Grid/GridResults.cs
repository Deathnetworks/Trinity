using System;
using System.Collections.Generic;
using System.Linq;
using Trinity.Technicals;
using Zeta.Common;

namespace Trinity
{
    internal class GridResults
    {
        static GridResults()
        {
        }

        public static void ResetTickValues()
        {
            if (RecordedValues_GetBestClusterNode != null)
                RecordedValues_GetBestClusterNode.RemoveWhere(n => n != null && n.GridLocation != null && n.GridLocation.Distance > 150);
            if (RecordedValues_GetBestNode != null)
                RecordedValues_GetBestNode.RemoveWhere(n => n != null && n.GridLocation != null && n.GridLocation.Distance > 150);

            TickValues_GetWeightAtPoint = new HashSet<GetWeightResult>();
            TickValue_GetWeightAtPlayer = 0f;
            TickValue_ToDictionary = new Dictionary<Vector3, float>();
            IsTickRecorded_ClusterExist = false;
            TickValue_ClusterExist = false;
        }

        public static HashSet<GetBestClusterNodeResult> RecordedValues_GetBestClusterNode = new HashSet<GetBestClusterNodeResult>();
        public static HashSet<GetBestNodeResult> RecordedValues_GetBestNode = new HashSet<GetBestNodeResult>();

        public static HashSet<GetWeightResult> TickValues_GetWeightAtPoint = new HashSet<GetWeightResult>();
        public static float TickValue_GetWeightAtPlayer = 0f;
        public static Dictionary<Vector3, float> TickValue_ToDictionary = new Dictionary<Vector3, float>();
        public static bool IsTickRecorded_ClusterExist = false;
        public static bool TickValue_ClusterExist = false;

        public static bool HasRecordedValue_GetBestClusterNode(out GridNode gridPoint, float maxRange = 100f, Vector3 loc = new Vector3())
        {
            using (new MemorySpy("HasRecordedValue_GetBestClusterNode"))
            {
                gridPoint = new GridNode(new Vector3());

                if (!RecordedValues_GetBestClusterNode.Any())
                    return false;

                var p = gridPoint;
                try
                {
                    p = RecordedValues_GetBestClusterNode.FirstOrDefault(r => r != null && r.Equals(maxRange, loc)).GridLocation;
                }
                catch
                {
                    return false;
                }

                if (p != null)
                {
                    gridPoint = p;
                    return true;
                }

                return false;
            }
        }

        public static bool HasRecordedValue_GetBestNode(out GridNode gridPoint, float miRange = 5f, float maRange = 100f, Vector3 loc = new Vector3())
        {
            using (new MemorySpy("HasRecordedValue_GetBestNode"))
            {
                gridPoint = new GridNode(new Vector3());

                if (!RecordedValues_GetBestNode.Any())
                    return false;

                var p = gridPoint;
                try
                {
                    p = RecordedValues_GetBestNode.FirstOrDefault(r => r != null && r.Equals(miRange, maRange, loc)).GridLocation;
                }
                catch
                {
                    return false;
                }

                if (p != null)
                {
                    gridPoint = p;
                    return true;
                }

                return false;
            }
        }

        public static bool HasTickValue_GetWeightAtPoint(out float weight, Vector3 loc = new Vector3())
        {
            using (new MemorySpy("HasTickValue_GetWeightAtPoint"))
            {
                weight = 0f;

                if (!TickValues_GetWeightAtPoint.Any())
                    return false;

                var w = weight;
                try
                {
                    w = TickValues_GetWeightAtPoint.FirstOrDefault(r => r != null && r.Equals(loc)).Weight;
                }
                catch
                {
                    return false;
                }

                if (w != 0f)
                {
                    weight = w;
                    return true;
                }

                return false;
            }
        }

        public static bool HasTickValue_GetWeightAtPlayer(out float weight)
        {
            using (new MemorySpy("HasTickValue_GetWeightAtPlayer"))
            {
                weight = TickValue_GetWeightAtPlayer;
                if (weight != 0f)
                    return true;

                return false;
            }
        }

        public static bool HasTickValue_ToDictionary(out Dictionary<Vector3, float> dict)
        {
            using (new MemorySpy("HasTickValue_ToDictionary"))
            {
                dict = TickValue_ToDictionary;
                if (dict != new Dictionary<Vector3, float>())
                    return true;

                return false;
            }
        }

        public static bool HasTickValue_ClusterExist(out bool exist)
        {
            using (new MemorySpy("HasTickValue_ClusterExist"))
            {
                exist = TickValue_ClusterExist;
                if (IsTickRecorded_ClusterExist)
                    return true;

                return false;
            }
        }
    }

    internal class GetBestClusterNodeResult : IEquatable<GetBestClusterNodeResult>
    {
        public GridNode GridLocation { get; set; }
        public float MaxRange { get; set; }
        public Vector3 Location { get; set; }

        public GetBestClusterNodeResult(GridNode gridLocation, float maxRange = 100f, Vector3 loc = new Vector3())
        {
            GridLocation = gridLocation;
            MaxRange = maxRange;
            Location = loc;
        }

        public bool Equals(GetBestClusterNodeResult other)
        {
            return GridLocation == other.GridLocation;
        }

        public bool Equals(float maxRange = 100f, Vector3 loc = new Vector3())
        {
            return MaxRange == maxRange && Location.Distance2D(loc) <= 2f;
        }
    }

    internal class GetBestNodeResult : IEquatable<GetBestNodeResult>
    {
        public GridNode GridLocation { get; set; }
        public float MiRange { get; set; }
        public float MaRange { get; set; }
        public Vector3 Location { get; set; }
        public bool PrioritizeDist { get; set; }

        public GetBestNodeResult(GridNode gridLocation, float miRange = 5f, float maRange = 100f, Vector3 loc = new Vector3(), bool prioritizeDist = false)
        {
            GridLocation = gridLocation;
            MiRange = miRange;
            MaRange = maRange;
            Location = loc;
            PrioritizeDist = prioritizeDist;
        }

        public bool Equals(GetBestNodeResult other)
        {
            return GridLocation == other.GridLocation;
        }

        public bool Equals(float miRange = 5f, float maRange = 100f, Vector3 loc = new Vector3(), bool prioritizeDist = false)
        {
            return MiRange == miRange && MaRange == maRange && Location.Distance2D(loc) <= 2f && PrioritizeDist == prioritizeDist;
        }
    }

    internal class GetWeightResult : IEquatable<GetWeightResult>
    {
        public float Weight { get; set; }
        public Vector3 Location { get; set; }

        public GetWeightResult(float weight = 5f, Vector3 loc = new Vector3())
        {
            Weight = weight;
            Location = loc;
        }

        public bool Equals(GetWeightResult other)
        {
            return Equals(Location, other.Location);
        }

        public bool Equals(Vector3 loc)
        {
            return Location.Distance2D(loc) <= 2f;
        }
    }
}