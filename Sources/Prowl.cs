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
// Prowl Notification Support
        public static Queue<ProwlNotification> pushQueue = new Queue<ProwlNotification>();
        public struct ProwlNotification
        {
            public string Event { get; set; }
            public string Description { get; set; }
            public ProwlNotificationPriority Priority { get; set; }
        }
        public enum ProwlNotificationPriority : sbyte
        {
            VeryLow = -2,
            Moderate = -1,
            Normal = 0,
            High = 1,
            Emergency = 2
        }
        public static void AddNotificationToQueue(string description, string eventName, ProwlNotificationPriority priority)
        {
// Queue the notification message
            var newNotification =
                    new ProwlNotification
                    {
                        Description = description,
                        Event = eventName,
                        Priority = priority
                    };
            pushQueue.Enqueue(newNotification);
        }
        public static void SendNotification(ProwlNotification notification)
        {
            if (settings.bEnableProwl && sProwlAPIKey != "")
            {
                var newNotification =
                        new ProwlNotification
                        {
                            Description = notification.Description,
                            Event = notification.Event,
                            Priority = notification.Priority
                        };
                try
                {
                    PostNotification(newNotification);
                }
                catch
                {
                }
            }
            if (settings.bEnableAndroid && sAndroidAPIKey != "")
            {
                var newNotification =
                        new ProwlNotification
                        {
                            Description = notification.Description,
                            Event = notification.Event,
                            Priority = notification.Priority
                        };
                try
                {
                    PostNotification(newNotification, true);
                }
                catch
                {
                }
            }
        }
        public static void PostNotification(ProwlNotification notification_, bool bForAndroid = false)
        {
            string prowlUrlSb = !bForAndroid ? @"https:
//prowl.weks.net/publicapi/add" : @"https:
//www.notifymyandroid.com/publicapi/notify";
            string sThisAPIKey = !bForAndroid ? sProwlAPIKey : sAndroidAPIKey;
            prowlUrlSb += "?apikey=" + HttpUtility.UrlEncode(sThisAPIKey) +
                          "&application=" + HttpUtility.UrlEncode("GilesTrinity") +
                          "&description=" + HttpUtility.UrlEncode(notification_.Description) +
                          "&event=" + HttpUtility.UrlEncode(notification_.Event) +
                          "&priority=" + HttpUtility.UrlEncode(notification_.Priority.ToString());
            var updateRequest =
                (HttpWebRequest)WebRequest.Create(prowlUrlSb.ToString());
            updateRequest.ContentLength = 0;
            updateRequest.ContentType = "application/x-www-form-urlencoded";
            updateRequest.Method = "POST";
//updateRequest.Timeout = 5000;
            var postResponse = default(WebResponse);
            try
            {
                postResponse = updateRequest.GetResponse();
            }
            finally
            {
                if (postResponse != null)
                    postResponse.Close();
            }
        }
    }
}
