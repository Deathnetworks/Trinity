using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Trinity.Technicals;
using Zeta.CommonBot;
using Zeta.CommonBot.Profile;
using Zeta.TreeSharp;
using Zeta.XmlEngine;
using Action = Zeta.TreeSharp.Action;

namespace Trinity.XmlTags
{
    // Thanks Nesox for the XML loading tricks!
    [XmlElement("TrinityLoadOnce")]
    public class TrinityLoadOnce : ProfileBehavior
    {
        internal static int lastUpdate = DateTime.Now.Ticks.GetHashCode();
        internal static List<string> UsedProfiles = new List<string>();
        string[] AvailableProfiles = { };
        string NextProfileName = String.Empty;
        string NextProfilePath = String.Empty;
        string CurrentProfilePath = String.Empty;
        string CurrentProfileName = String.Empty;
        Random rand = new Random();
        bool initialized = false;
        private bool isDone = false;

        public override bool IsDone
        {
            get { return !IsActiveQuestStep || isDone; }
        }

        [XmlElement("ProfileList")]
        public List<LoadProfileOnce> Profiles { get; set; }

        public TrinityLoadOnce()
        {
            if (Profiles == null)
                Profiles = new List<LoadProfileOnce>();
            else if (Profiles.Count() == 0)
                Profiles = new List<LoadProfileOnce>();

            GameEvents.OnGameJoined += TrinityLoadOnce_OnGameJoined;

        }

        ~TrinityLoadOnce()
        {
            GameEvents.OnGameJoined -= TrinityLoadOnce_OnGameJoined;
        }

        void TrinityLoadOnce_OnGameJoined(object sender, EventArgs e)
        {
            UsedProfiles = new List<string>();
        }


        private void Initialize()
        {
            if (initialized)
                return;

            if (Profiles == null)
                Profiles = new List<LoadProfileOnce>();

            CurrentProfilePath = Path.GetDirectoryName(ProfileManager.CurrentProfile.Path);
            CurrentProfileName = Path.GetFileName(ProfileManager.CurrentProfile.Path);

            initialized = true;


        }

        // Re-align filenames with A, B, n, etc
        public void RealignFileNames()
        {
            // Seed random generator with same INT until new game
            Random rand2 = new Random(lastUpdate);

            for (int i = 0; i < Profiles.Count; i++)
            {
                string[] profilesArray = Profiles[i].FileName.Split(';');
                Profiles[i].FileName = profilesArray[rand2.Next(0, profilesArray.Length)];
            }
        }

        protected override Composite CreateBehavior()
        {
            return new Sequence(
                new Action(ret => Initialize()),
                new Action(ret => RealignFileNames()),
                new Action(ret => Logger.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "TrinityLoadOnce: Found {0} Total Profiles, {1} Used Profiles, {2} Unused Profiles",
                    Profiles.Count(), UsedProfiles.Count(), Profiles.Where(p => !UsedProfiles.Contains(p.FileName)).Count())),
                new Action(ret => AvailableProfiles = (from p in Profiles where !UsedProfiles.Contains(p.FileName) && p.FileName != CurrentProfileName select p.FileName).ToArray()),
                new PrioritySelector(
                    new Decorator(ret => AvailableProfiles.Length == 0,
                        new Sequence(
                            new Action(ret => Logger.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "TrinityLoadOnce: All available profiles have been used!", true)),
                            new Action(ret => lastUpdate = DateTime.Now.Ticks.GetHashCode()),
                            new Action(ret => isDone = true)
                        )
                    ),
                    new Decorator(ret => AvailableProfiles.Length > 0,
                        new Sequence(
                            new Action(ret => NextProfileName = AvailableProfiles[rand.Next(0, AvailableProfiles.Length)]),
                            new Action(ret => NextProfilePath = Path.Combine(CurrentProfilePath, NextProfileName)),
                            new PrioritySelector(
                                new Decorator(ret => File.Exists(NextProfilePath),
                                    new Sequence(
                                        new Action(ret => Logger.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "TrinityLoadOnce: Loading next random profile: {0}", NextProfileName)),
                                        new Action(ret => UsedProfiles.Add(NextProfileName)),
                                        new Action(ret => ProfileManager.Load(NextProfilePath))
                                    )
                                ),
                                new Action(ret => Logger.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "TrinityLoadOnce: ERROR: Profile {0} does not exist!", NextProfilePath))
                            )
                        )
                    ),
                    new Action(ret => Logger.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "TrinityLoadOnce: Unkown error", true))
                )
           );
        }
        public override void ResetCachedDone()
        {
            initialized = false;
            isDone = false;
            base.ResetCachedDone();
        }
    }

    [XmlElement("LoadProfileOnce")]
    public class LoadProfileOnce
    {
        [XmlAttribute("filename")]
        [XmlAttribute("Filename")]
        [XmlAttribute("FileName")]
        [XmlAttribute("fileName")]
        [XmlAttribute("profile")]
        public string FileName { get; set; }

        public LoadProfileOnce(string filename)
        {
            this.FileName = filename;
        }

        public LoadProfileOnce()
        {

        }

        public override string ToString()
        {
            return FileName;
        }
    }

}
