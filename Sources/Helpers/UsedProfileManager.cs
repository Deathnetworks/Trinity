using System.Collections.Generic;
using System.IO;
using Trinity.Technicals;
using Trinity.XmlTags;
using Zeta.CommonBot;
using Zeta.CommonBot.Profile;

namespace Trinity
{
    public class UsedProfileManager
    {
        private static List<string> UsedProfiles = new List<string>();

        internal static void RecordProfile()
        {
            RecordTrinityLoadOnceProfile();

            string currentProfile = ProfileManager.CurrentProfile.Path;

            if (!UsedProfiles.Contains(currentProfile))
            {
                Logger.Log(TrinityLogLevel.Debug, LogCategory.UserInformation, "New profile found - updating TargetBlacklists");
                RefreshProfileBlacklists();
                UsedProfiles.Add(currentProfile);
            }

            if (currentProfile != Trinity.CurrentProfile)
            {
                // See if we appear to have started a new game
                if (Trinity.FirstProfile != "" && currentProfile == Trinity.FirstProfile)
                {
                    Trinity.TotalProfileRecycles++;
                }

                Trinity.ProfileHistory.Add(currentProfile);
                Trinity.CurrentProfile = currentProfile;

                if (ProfileManager.CurrentProfile != null && ProfileManager.CurrentProfile.Name != null)
                {
                    Trinity.SetWindowTitle(ProfileManager.CurrentProfile.Name);
                }

                if (Trinity.FirstProfile == "")
                    Trinity.FirstProfile = currentProfile;

                // Clear Trinity Combat Ignore Tag
                TrinityCombatIgnore.IgnoreList.Clear();
            }
        }

        private static void RecordTrinityLoadOnceProfile()
        {
            string currentProfileFileName = Path.GetFileName(ProfileManager.CurrentProfile.Path);
            if (!TrinityLoadOnce.UsedProfiles.Contains(currentProfileFileName))
            {
                TrinityLoadOnce.UsedProfiles.Add(currentProfileFileName);
            }
        }

        /// <summary>
        /// Adds profile blacklist entries to the Trinity Blacklist
        /// </summary>
        internal static void RefreshProfileBlacklists()
        {
            foreach (TargetBlacklist b in ProfileManager.CurrentProfile.TargetBlacklists)
            {
                if (!DataDictionary.BlackListIds.Contains(b.ActorId))
                {
                    Logger.Log(TrinityLogLevel.Debug, LogCategory.UserInformation, "Adding Profile TargetBlacklist {0} to Trinity Blacklist", b.ActorId);
                    DataDictionary.AddToBlacklist(b.ActorId);
                }
            }
        }
    }
}
