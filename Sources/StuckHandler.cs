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
using Zeta.CommonBot.Profile.Common;
namespace GilesTrinity
{

    // Blank Stuck Handler - to disable DB stuck handler
    public class GilesStuckHandler : IStuckHandler
    {
        public bool IsStuck { get { return GilesPlayerMover.UnstuckChecker(); } }
        public Vector3 GetUnstuckPos() { return GilesPlayerMover.UnstuckHandler(); }
    }
}
