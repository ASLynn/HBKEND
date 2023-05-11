using HeyDoc.Web.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PushSharp.Apple;
using PushSharp.Core;
using PushSharp.Google;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace HeyDoc.Web.Lib
{
    public class PushManager
    {
        private static readonly log4net.ILog log =
           log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region Singleton
        private static volatile GcmServiceBroker gcmInstance;
        //private static volatile ApnsServiceBroker apnsInstance;
        //private static volatile ApnsServiceBroker devApnsInstance;
        //private static volatile FeedbackService apnsInstanceFbs;
        private static object apnsSyncRoot = new Object();
        private static object devApnsSyncRoot = new Object();
        private static object gcmSyncRoot = new Object();

        private static bool HandleFCMException(Exception ex)
        {
            if (ex is GcmNotificationException notificationException)
            {
                var gcmNotification = notificationException.Notification;
                var description = notificationException.Description;

                log.Error($"FCM Failure: [{gcmNotification.MessageId}] {description} -> {gcmNotification}", ex);

            }
            else if (ex is GcmMulticastResultException multicastException)
            {
#if DEBUG
                foreach (var succeededNotification in multicastException.Succeeded)
                {
                    log.Info("FCM Sent: " + succeededNotification);
                }
#endif

                foreach (var failedKvp in multicastException.Failed)
                {
                    HandleFCMException(failedKvp.Value);
                }
            }
            else if (ex is DeviceSubscriptionExpiredException expiredException)
            {
                var oldId = expiredException.OldSubscriptionId;
                var newId = expiredException.NewSubscriptionId;

                log.Info($"FCM Device Registration Changed:  Old-> {oldId}  New-> {newId} -> {expiredException.Notification}");

                try
                {
                    using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
                    {
                        var entityDeviceList = db.Devices.Where(e => e.RegistrationId == oldId);
                        foreach (var entityDevice in entityDeviceList)
                        {
                            entityDevice.RegistrationId = string.IsNullOrEmpty(newId) ? null : newId;
                        }
                        db.SaveChanges();
                    }
                }
                catch (Exception exception)
                {
                    log.Error(exception);
                }
            }
            else if (ex is RetryAfterException retryException)
            {
                // If you get rate limited, you should stop sending messages until after the RetryAfterUtc date
                //Console.WriteLine ($"GCM Rate Limited, don't send more until after {retryException.RetryAfterUtc}");
            }
            else
            {
                //Console.WriteLine("GCM Notification Failed for some unknown reason");
            }
            // Mark it as handled
            return true;
        }

        public static GcmServiceBroker GcmInstance
        {
            get
            {
                if (gcmInstance == null)
                {
                    lock (gcmSyncRoot)
                    {
                        if (gcmInstance == null)
                        {
                            // TODO M UNBLANK
                            string key = Microsoft.Azure.CloudConfigurationManager.GetSetting("FirebaseCloudMessagingServerKey");
                            var config = new GcmConfiguration(key);
                            config.GcmUrl = "https://fcm.googleapis.com/fcm/send";
                            gcmInstance = new GcmServiceBroker(config);

                            gcmInstance.OnNotificationFailed += (notification, aggregateEx) =>
                            {
                                aggregateEx.Handle(HandleFCMException);
                            };

#if DEBUG
                            gcmInstance.OnNotificationSucceeded += (notification) =>
                            {
                                log.Info("FCM Sent: " + notification);
                            };
#endif

                            gcmInstance.Start();
                        }
                    }
                }
                return gcmInstance;
            }
        }
        #endregion Singleton

        public static async Task Push(List<Entity.Device> entityDeviceList, Notification notification, int? unreadCount = null)
        {
            try
            {
                bool isApnsDevs = false;
                using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
                {
                    isApnsDevs = SettingService.GetSettingValue(db, SettingService.IS_APNS_DEV);
                }

                //var currentInstance = (isApnsDevs) ? DevApnsInstance : ApnsInstance;

                var notificationJson = JObject.FromObject(notification.Items);
                List<string> AndroidRegIdList = new List<string>();
                var payload = notification.ToApnsPayload();
                // Track third party user IDs that notification was sent to as we don't want to call third party PN API for every device the user has
                var sentThirdPartyIds = new HashSet<string>();
                var thirdPartyPNTasks = new List<Task>();

                foreach (var entityDevice in entityDeviceList)
                {
                    var deviceType = entityDevice.DeviceType;
                    switch (entityDevice.TokenType)
                    {
                        case AccessTokenType.Doc2Us:
                            switch (deviceType)
                            {
                                case DeviceType.Android:
                                case DeviceType.Web:
                                    if (AndroidRegIdList.Count < 999)
                                    {
                                        AndroidRegIdList.Add(entityDevice.RegistrationId);
                                    }
                                    else
                                    {
                                        AndroidRegIdList.Add(entityDevice.RegistrationId);// device 1000
                                        var androidNotification = new GcmNotification() { RegistrationIds = AndroidRegIdList.ToList(), Data = notificationJson, Priority = GcmNotificationPriority.High };
                                        GcmInstance.QueueNotification(androidNotification);
                                        AndroidRegIdList.Clear();
                                    }
                                    break;
                                case DeviceType.IOS:
                                    //var appleNotification = new ApnsNotification() { DeviceToken = entityDevice.RegistrationId, Payload = payload, LowPriority = false };
                                    //ApnsInstance.QueueNotification(appleNotification);
                                    //currentInstance.QueueNotification(appleNotification);

                                    AwsSnsService.Push(entityDevice, payload.ToString(), !isApnsDevs);

                                    // apnsInstanceFbs.Check();
                                    break;
                                default:
                                    // do nothing
                                    break;
                            }
                            break;
                    }
                }
                if (AndroidRegIdList.Count > 0)
                {
                    var androidNotification = new GcmNotification() { RegistrationIds = AndroidRegIdList.ToList(), Data = notificationJson }; // AndroidRegIdList.ToList() if you did not convert it to ToList(), then androidNotification.RegistrationIds gets cleared
                    GcmInstance.QueueNotification(androidNotification);
                    AndroidRegIdList.Clear();
                }
                await Task.WhenAll(thirdPartyPNTasks);
            }
            catch (Exception ex)
            {
                log.Error("PN Unknown Error", ex);
            }
        }

        // mn
        public static async Task NotifyUser(List<int> userIdList, string title, PnActionType type, PackageType? packagetype = null, string fromUser = null, string relatedId = null, Dictionary<string, string> extraParams = null)
        {
            try
            {
                Notification notification = new Notification();
                notification.Title = title.Truncate(100);
                notification.ActionType = type;
                notification.Items.Add("sound", "default");
                if (!string.IsNullOrEmpty(fromUser))
                {
                    notification.Items.Add("FromUser", fromUser);
                }
                if (!string.IsNullOrEmpty(relatedId))
                {
                    notification.Items.Add("RelatedId", relatedId);
                }
                if (extraParams != null && extraParams.Count > 0)
                {
                    foreach (var notificationParam in extraParams)
                    {
                        notification.Items.Add(notificationParam.Key, notificationParam.Value);
                    }
                }
                using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
                {
                    var entityDevices = db.Devices.Where(e => userIdList.Contains(e.UserId.Value) && (e.TokenType != AccessTokenType.Doc2Us || (e.RegistrationId != null && e.RegistrationId.Length > 50)) && e.AccessToken != null && e.UserId != null).ToList();
                    await Push(entityDevices, notification);
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        // mn
        public static async Task NotifySupportDoctor(List<int> userIdList, string title, int patientid, PnActionType type, PackageType? packagetype = null, long? packageid = null)
        {
            try
            {
                Notification notification = new Notification();
                notification.Title = title.Truncate(100);
                notification.ActionType = type;

                using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
                {
                    // Check chatroom already exists, if not create chat room
                    var entityChatRoom = db.ChatRooms.FirstOrDefault(e => !e.IsDelete && e.PatientId == patientid && e.DoctorId == userIdList.FirstOrDefault());
                    if (entityChatRoom == null)
                    {
                        entityChatRoom = db.ChatRooms.Create();
                        db.ChatRooms.Add(entityChatRoom);
                        entityChatRoom.DoctorId = userIdList.FirstOrDefault();
                        entityChatRoom.PatientId = patientid;
                        entityChatRoom.CreateDate = DateTime.UtcNow;

                        db.SaveChanges();
                    }
                    notification.ActionContent = entityChatRoom.ChatRoomId.ToString();
                    notification.Items.Add("sound", "default");
                    var entityDevices = db.Devices.Where(e => userIdList.Contains(e.UserId.Value) && (e.TokenType != AccessTokenType.Doc2Us || e.RegistrationId != null) && e.AccessToken != null && e.UserId != null).ToList();
                    await Push(entityDevices, notification);

                    //update last job date of doctor
                    var entitySupport = db.Doctors.Where(e => userIdList.Contains(e.UserId));
                    entitySupport.FirstOrDefault().LastJobDate = DateTime.UtcNow;
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }
    }

    public class Notification
    {
        public const string TITLE = "Title";

        private long id;
        public long Id
        {
            get
            {
                return id;
            }
            set
            {
                id = value;
                Items.Add("NotificationId", id.ToString());
            }
        }

        private string title;
        public string Title
        {
            get
            {
                if (Items.ContainsKey(TITLE))
                {
                    return Items[TITLE];
                }
                else
                {
                    return title;
                }
            }
            set
            {
                title = value;
                if (Items.ContainsKey(TITLE))
                {
                    Items[TITLE] = title;
                }
                else
                {
                    Items.Add(TITLE, title);
                }
            }
        }

        private string actionContent;
        public string ActionContent
        {
            get
            {
                return actionContent;
            }
            set
            {
                actionContent = value;
                Items.Add("RelatedId", actionContent);
            }
        }

        private PnActionType actionType;
        public PnActionType ActionType
        {
            get
            {
                return actionType;
            }
            set
            {
                actionType = value;
                Items.Add("NotificationType", ((int)actionType).ToString());
            }
        }


        public Dictionary<string, string> Items { get; private set; }

        public Notification()
        {
            Items = new Dictionary<string, string>();
        }

        public void AddItem(string key, string value)
        {
            Items.Add(key, value);
        }

        public JObject ToApnsPayload()
        {
            Title = Title.Replace("\"", "\\\"");
            string jsonString = "{\"aps\":{\"alert\":\"" + Title + "\"";

            foreach (var item in Items)
            {
                if (item.Key == "badge")
                {
                    jsonString = jsonString + ",\"" + item.Key + "\":" + item.Value;
                    continue;

                }
                if (item.Key != Notification.TITLE)
                {
                    var itemValue = item.Value != null ? item.Value.Replace("\"", "\\\"") : "";
                    jsonString = jsonString + ",\"" + item.Key + "\":\"" + itemValue + "\"";
                }

            }


            jsonString = jsonString + "}}";
             return JObject.Parse(jsonString);
        }
    }
}