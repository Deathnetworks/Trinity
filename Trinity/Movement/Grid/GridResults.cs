using System;
using System.Collections.Generic;
using System.Linq;
using Zeta.Common;

namespace Trinity
{
    class GridResults
    {
        static GridResults()
        {
        }

        public static void ResetTickValues()
        {
            TickValues_GetBestClusterNode = new HashSet<GetBestClusterNodeResult>();
            TickValues_GetBestNode = new HashSet<GetBestNodeResult>();
            TickValues_GetWeightAtPoint = new HashSet<GetWeightResult>();
            TickValue_GetWeightAtPlayer = 0f;
            TickValue_ToDictionary = new Dictionary<Vector3, float>();
            IsTickRecorded_ClusterExist = false;
            TickValue_ClusterExist = false;
        }

        public static HashSet<GetBestClusterNodeResult> TickValues_GetBestClusterNode = new HashSet<GetBestClusterNodeResult>();
        public static HashSet<GetBestNodeResult> TickValues_GetBestNode = new HashSet<GetBestNodeResult>();
        public static HashSet<GetWeightResult> TickValues_GetWeightAtPoint = new HashSet<GetWeightResult>();
        public static float TickValue_GetWeightAtPlayer = 0f;
        public static Dictionary<Vector3, float> TickValue_ToDictionary = new Dictionary<Vector3, float>();
        public static bool IsTickRecorded_ClusterExist = false;
        public static bool TickValue_ClusterExist = false;

        public static bool HasTickValue_GetBestClusterNode(out GridNode gridPoint, float maxRange = 100f, Vector3 loc = new Vector3())
        {
            gridPoint = new GridNode(new Vector3());

            if (!TickValues_GetBestClusterNode.Any())
                return false;

            var p = gridPoint;
            try { p = TickValues_GetBestClusterNode.FirstOrDefault(r => r != null && r.Equals(maxRange, loc)).GridLocation; }
            catch { return false; }

            if (p != null)
            {
                gridPoint = p;
                return true;
            }

            return false;
        }

        public static bool HasTickValue_GetBestNode(out GridNode gridPoint, float miRange = 5f, float maRange = 100f, Vector3 loc = new Vector3())
        {
            gridPoint = new GridNode(new Vector3());

            if (!TickValues_GetBestNode.Any())
                return false;

            var p = gridPoint;
            try { p = TickValues_GetBestNode.FirstOrDefault(r => r != null && r.Equals(miRange, maRange, loc)).GridLocation; }
            catch { return false; }

            if (p != null)
            {
                gridPoint = p;
                return true;
            }

            return false;
        }

        public static bool HasTickValue_GetWeightAtPoint(out float weight, Vector3 loc = new Vector3())
        {
            weight = 0f;

            if (!TickValues_GetWeightAtPoint.Any())
                return false;

            var w = weight;
            try { w = TickValues_GetWeightAtPoint.FirstOrDefault(r => r != null && r.Equals(loc)).Weight; }
            catch { return false; }
             
            if (w != 0f)
            {
                weight = w;
                return true;
            }

            return false;
        }

        public static bool HasTickValue_GetWeightAtPlayer(out float weight)
        {
            weight = TickValue_GetWeightAtPlayer;
            if (weight != 0f)
                return true;

            return false;
        }

        public static bool HasTickValue_ToDictionary(out Dictionary<Vector3, float> dict)
        {
            dict = TickValue_ToDictionary;
            if (dict != new Dictionary<Vector3, float>())
                return true;

            return false;
        }

        public static bool HasTickValue_ClusterExist(out bool exist)
        {
            exist = TickValue_ClusterExist;
            if (IsTickRecorded_ClusterExist)
                return true;

            return false;
        }
    }

    class GetBestClusterNodeResult : IEquatable<GetBestClusterNodeResult>
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

    class GetBestNodeResult : IEquatable<GetBestNodeResult>
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

    class GetWeightResult : IEquatable<GetWeightResult>
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
