using System;
using System.Collections.Generic;
using System.Linq;
using Trinity.Technicals;
using Zeta.Common;

namespace Trinity
{
    internal class GridResults
    {
        public static void ResetTickValues()
        {
            if (RecordedValues_GetNodeAt.Count() > 1000)
                RecordedValues_GetNodeAt.Clear();

            if (RecordedValues_GetBestSafeNode.Count() > 1000)
                RecordedValues_GetBestSafeNode.Clear();

            if (RecordedValues_GetBestClusterNode.Count() > 1000)
                RecordedValues_GetBestClusterNode.Clear();
        }

        public static Dictionary<Vector3, GridNode> RecordedValues_GetNodeAt = new Dictionary<Vector3, GridNode>();
        public static Dictionary<Tuple<float, float, Vector3>, GridNode> RecordedValues_GetBestSafeNode = new Dictionary<Tuple<float, float, Vector3>, GridNode>();
        public static Dictionary<Tuple<float, float, Vector3>, GridNode> RecordedValues_GetBestClusterNode = new Dictionary<Tuple<float, float, Vector3>, GridNode>();
    }
}