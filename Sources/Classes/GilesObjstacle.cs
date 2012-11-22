using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.Windows.Markup;
using Zeta;
using Zeta.Common;
using Zeta.Common.Plugins;
using Zeta.CommonBot;
using Zeta.CommonBot.Profile;
using Zeta.CommonBot.Profile.Composites;
using Zeta.Internals;
using Zeta.Internals.Actors;
using Zeta.Internals.Actors.Gizmos;
using Zeta.Internals.SNO;
using Zeta.Navigation;
using Zeta.TreeSharp;
using Zeta.XmlEngine;
namespace GilesTrinity
{
    public partial class GilesTrinity : IPlugin
    {
        // Obstacles for quick mapping of paths etc.
        public class GilesObstacle
        {
            public Vector3 vThisLocation { get; set; }
            public float fThisRadius { get; set; }
            public int iThisSNOID { get; set; }
            public double dThisWeight { get; set; }
            public GilesObstacle(Vector3 thislocation, float thisradius, int thissnoid, double thisweight = 0)
            {
                vThisLocation = thislocation;
                fThisRadius = thisradius;
                iThisSNOID = thissnoid;
                dThisWeight = thisweight;
            }
        }
    }
}
