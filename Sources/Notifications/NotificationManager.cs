using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web;
using Trinity.Technicals;

namespace Trinity.Notifications
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
            //New string to pass in notification type
            string notificationType = "";

            if (Trinity.Settings.Notification.IPhoneEnabled && !string.IsNullOrWhiteSpace(Trinity.Settings.Notification.IPhoneKey))
            {
                var newNotification =
                        new ProwlNotification
                        {
                            Description = notification.Description,
                            Event = notification.Event,
                            Priority = notification.Priority
                        };
                notificationType = "iphone";
                try
                {
                    PostNotification(newNotification, notificationType);
                }
                catch
                {
                }
            }
            if (Trinity.Settings.Notification.AndroidEnabled && !string.IsNullOrWhiteSpace(Trinity.Settings.Notification.AndroidKey))
            {
                var newNotification =
                        new ProwlNotification
                        {
                            Description = notification.Description,
                            Event = notification.Event,
                            Priority = notification.Priority
                        };
                notificationType = "android";
                try
                {
                    PostNotification(newNotification, notificationType);
                }
                catch
                {
                }
            }

            //Adding in Pushover Stuffs. 
            if (Trinity.Settings.Notification.PushoverEnabled && !string.IsNullOrWhiteSpace(Trinity.Settings.Notification.PushoverKey))
            {
                var newNotification =
                        new ProwlNotification
                        {
                            Description = notification.Description,
                            Event = notification.Event,
                            Priority = notification.Priority
                        };
                notificationType = "pushover";
                try
                {
                    PostNotification(newNotification, notificationType);
                }
                catch
                {
                }
            }



        }
        //No longer takes in a bool, rather takes in actual type since there are three options
        public static void PostNotification(ProwlNotification notification_, string notType)
        {
            //
            if (notType == "pushover")
            {
                string prowlUrlSb = "https://api.pushover.net/1/messages.json";
                string sThisAPIKey = Trinity.Settings.Notification.PushoverKey;
                //The registered token for Trinity I set up on pushover.net
                prowlUrlSb += "?token=aBf5s4BGSCkRHkneWq3VcGQMX2GjgP" + // Created "Trinity" application in Pushover.net
                              "&user=" + HttpUtility.UrlEncode(sThisAPIKey.Trim()) +
                              "&message=" + HttpUtility.UrlEncode(notification_.Description) +
                              "&title=" + HttpUtility.UrlEncode(notification_.Event) +
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

            if (notType == "iphone")
            {
                string prowlUrlSb = "https://prowl.weks.net/publicapi/add";
                string sThisAPIKey = Trinity.Settings.Notification.IPhoneKey;
                prowlUrlSb += "?apikey=" + HttpUtility.UrlEncode(sThisAPIKey.Trim()) +
                             "&application=" + HttpUtility.UrlEncode("Trinity") +
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

            if (notType == "android")
            {
                string prowlUrlSb = "https://www.notifymyandroid.com/publicapi/notify";
                string sThisAPIKey = Trinity.Settings.Notification.AndroidKey;
                prowlUrlSb += "?apikey=" + HttpUtility.UrlEncode(sThisAPIKey.Trim()) +
                              "&application=" + HttpUtility.UrlEncode("Trinity") +
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
                Logger.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "Error sending email.{0}{1}", Environment.NewLine, e.ToString());
            }
        }
    }
}
