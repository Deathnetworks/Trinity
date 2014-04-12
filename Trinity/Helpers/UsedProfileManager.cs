using System;
using System.Collections.Generic;
using System.IO;
using Trinity.Combat.Abilities;
using Trinity.Technicals;
using Trinity.XmlTags;
using Zeta.Bot;
using Zeta.Bot.Profile;

namespace Trinity
{
    public class UsedProfileManager
    {
        private static List<string> UsedProfiles = new List<string>();

        internal static void RecordProfile()
        {
            using (new PerformanceLogger("RecordProfile"))
            {
                try
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
                        CombatBase.IsQuestingMode = false;

                        // See if we appear to have started a new game
                        if (Trinity.FirstProfile != "" && currentProfile == Trinity.FirstProfile)
                        {
                            Trinity.TotalProfileRecycles++;
                        }

                        Trinity.ProfileHistory.Add(currentProfile);
                        Trinity.CurrentProfile = currentProfile;
                        Trinity.CurrentProfileName = ProfileManager.CurrentProfile.Name;

                        SetProfileInWindowTitle();

                        if (Trinity.FirstProfile == "")
                            Trinity.FirstProfile = currentProfile;

                        // Clear Trinity Combat Ignore Tag
                        TrinityCombatIgnore.IgnoreList.Clear();
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError("Error recording new profile: " + ex.ToString());
                }
            }
        }

        internal static void SetProfileInWindowTitle()
        {

            string fileName = Path.GetFileName(ProfileManager.CurrentProfile.Path);

            if (ProfileManager.CurrentProfile != null && ProfileManager.CurrentProfile.Name != null)
            {
                Trinity.SetWindowTitle(Trinity.CurrentProfileName + " " + fileName);
            }
            else if (ProfileManager.CurrentProfile != null && string.IsNullOrWhiteSpace(ProfileManager.CurrentProfile.Name))
            {
                Trinity.SetWindowTitle(fileName);
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
            try
            {
                if (ProfileManager.CurrentProfile != null && ProfileManager.CurrentProfile.TargetBlacklists != null)
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
            catch (Exception ex)
            {
                Logger.LogError("Error in Refreshing Profile Blacklists: " + ex.ToString());
            }
        }
    }
}
