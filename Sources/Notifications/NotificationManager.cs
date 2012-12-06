using GilesTrinity.Technicals;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Text;

namespace GilesTrinity.Notifications
{
    //TODO: Add mail management here 

    internal static class NotificationManager
    {
        /// <summary>
        /// Email Notification SMTP Server
        /// </summary>
        public static string SmtpServer = "smtp.gmail.com";
        /// <summary>
        /// Email Notification Message
        /// </summary>
        public static StringBuilder EmailMessage = new StringBuilder();
        
        public static Queue<ProwlNotification> pushQueue = new Queue<ProwlNotification>();

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
            if (GilesTrinity.Settings.Notification.IPhoneEnabled && !string.IsNullOrWhiteSpace(GilesTrinity.Settings.Notification.IPhoneKey))
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
            if (GilesTrinity.Settings.Notification.AndroidEnabled && !string.IsNullOrWhiteSpace(GilesTrinity.Settings.Notification.AndroidKey))
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
        public static void PostNotification(ProwlNotification notification_, bool android = false)
        {
            string prowlUrlSb = !android ?
                                    @"https://prowl.weks.net/publicapi/add" :
                                    @"https://www.notifymyandroid.com/publicapi/notify";
            string sThisAPIKey = !android ? GilesTrinity.Settings.Notification.IPhoneKey : GilesTrinity.Settings.Notification.AndroidKey;
            prowlUrlSb += "?apikey=" + HttpUtility.UrlEncode(sThisAPIKey.Trim()) +
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

        public static void SendEmail(string to, string from, string subject, string body, string server, string password)
        {
            try
            {
                MailAddress fromAddress = new MailAddress(from);
                MailAddress toAddress = new MailAddress(to);
                SmtpClient smtpClient = new SmtpClient
                                    {
                                        Host = server,
                                        Port = 587,
                                        EnableSsl = true,
                                        DeliveryMethod = SmtpDeliveryMethod.Network,
                                        UseDefaultCredentials = false,
                                        Credentials = new NetworkCredential(fromAddress.Address, password)
                                    };
                using (MailMessage message = new MailMessage(fromAddress, toAddress)
                                                {
                                                    Subject = subject,
                                                    Body = body
                                                })
                {
                    smtpClient.Send(message);
                }
            }
            catch (Exception e)
            {
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "Error sending email.{0}{1}", Environment.NewLine, e.ToString());
            }
        }
    }
}
