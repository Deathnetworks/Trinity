using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GilesTrinity.Technicals;
using GilesTrinity.XmlTags;
using Zeta.CommonBot;
using Zeta.CommonBot.Profile;

namespace GilesTrinity
{
    public class UsedProfileManager
    {
        private static List<string> UsedProfiles = new List<string>();

        internal static void RecordProfile()
        {
            string currentProfileFileName = Path.GetFileName(ProfileManager.CurrentProfile.Path);
            if (!TrinityLoadOnce.UsedProfiles.Contains(currentProfileFileName))
            {
                TrinityLoadOnce.UsedProfiles.Add(currentProfileFileName);
            }

            string currentProfile = ProfileManager.CurrentProfile.Path;

            if (!UsedProfiles.Contains(currentProfile))
            {
                DbHelper.Log(TrinityLogLevel.Debug, LogCategory.UserInformation, "New profile found - updating TargetBlacklists");
                RefreshProfileBlacklists();
                UsedProfiles.Add(currentProfile);
            }

            if (currentProfile != GilesTrinity.CurrentProfile)
            {
                // See if we appear to have started a new game
                if (GilesTrinity.FirstProfile != "" && currentProfile == GilesTrinity.FirstProfile)
                {
                    GilesTrinity.TotalProfileRecycles++;
                }

                GilesTrinity.listProfilesLoaded.Add(currentProfile);
                GilesTrinity.CurrentProfile = currentProfile;

                if (ProfileManager.CurrentProfile != null && ProfileManager.CurrentProfile.Name != null)
                {
                    GilesTrinity.SetWindowTitle(ProfileManager.CurrentProfile.Name);
                }

                if (GilesTrinity.FirstProfile == "")
                    GilesTrinity.FirstProfile = currentProfile;
            }
        }

        /// <summary>
        /// Adds profile blacklist entries to the Giles Blacklist
        /// </summary>
        internal static void RefreshProfileBlacklists()
        {
            foreach (TargetBlacklist b in ProfileManager.CurrentProfile.TargetBlacklists)
            {
                if (!GilesTrinity.hashSNOIgnoreBlacklist.Contains(b.ActorId))
                {
                    DbHelper.Log(TrinityLogLevel.Debug, LogCategory.UserInformation, "Adding Profile TargetBlacklist {0} to Trinity Blacklist", b.ActorId);
                    GilesTrinity.hashSNOIgnoreBlacklist.Add(b.ActorId);
                }
                if (!GilesTrinity.hashActorSNOIgnoreBlacklist.Contains(b.ActorId))
                {
                    GilesTrinity.hashActorSNOIgnoreBlacklist.Add(b.ActorId);
                }
            }
        }
    }
}
