using HeyDoc.Web.Entity;
using HeyDoc.Web.Lib;
using HeyDoc.Web.Models;
using HeyDoc.Web.WebApi;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HeyDoc.Web.Services
{
    public class NotificationService
    {
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
       

        public static async Task NotifyUser(Entity.db_HeyDocEntities db, int userId, PnActionType type, long relatedId, string text, Dictionary<string, string> extraParams = null)
        {
            await NotifyUser(db, new List<int>() { userId }, type, relatedId.ToString(), text, extraParams);
        }

        public static async Task NotifyUser(Entity.db_HeyDocEntities db, int userId, PnActionType type, string relatedId, string text, Dictionary<string, string> extraParams = null)
        {
            await NotifyUser(db, new List<int>() { userId }, type, relatedId, text, extraParams);
        }

        public static async Task NotifyUser(Entity.db_HeyDocEntities db, List<int> userIds, PnActionType type, string relatedId, string text, Dictionary<string, string> extraParams = null)
        {
            try
            {
                Entity.PromotionUrl entityPromoUrl = null;
                if (type == PnActionType.ChatEnded)
                {
                    entityPromoUrl = db.PromotionUrls.OrderByDescending(e => e.CreateDate).FirstOrDefault();
                }
                foreach (var id in userIds)
                {
                    CreateNotification(db, type, relatedId, text, entityPromoUrl, id);
                }
                List<Entity.Device> entityDeviceList = new List<Entity.Device>();

                var entityUserDeviceList = db.Devices.Where(e => (e.TokenType != AccessTokenType.Doc2Us || e.RegistrationId != null) && e.AccessToken != null && e.UserId != null && userIds.Contains(e.UserId.Value));
                if (entityUserDeviceList.Count() > 0)
                {
                    foreach (var entityUserDevice in entityUserDeviceList)
                    {
                        entityDeviceList.Add(entityUserDevice);
                    }
                }

                Lib.Notification notification = new Lib.Notification
                {
                    Title = text,
                    ActionType = type,
                    ActionContent = relatedId.ToString()
                };
                if (entityPromoUrl != null)
                {
                    notification.Items.Add("URL", entityPromoUrl.Url);
                }
                notification.Items.Add("sound", "default");
                if (extraParams != null && extraParams.Count > 0)
                {
                    foreach (var notificationParam in extraParams)
                    {
                        notification.Items.Add(notificationParam.Key, notificationParam.Value);
                    }
                }

                await PushManager.Push(entityDeviceList, notification);
            }
            catch (Exception ex)
            {
                logger.Error("PushToUsers Exception: " + ex.ToString());
            }
        }

        public static async Task NotifyUserForVaccine(Entity.db_HeyDocEntities db, int userIds, PnActionType type, string relatedId, string text, Dictionary<string, string> extraParams = null)
        {
            try
            {
                Entity.PromotionUrl entityPromoUrl = null;
                var entityNotification = db.Notifications.Create();
                entityNotification.CreateDate = DateTime.UtcNow;
                entityNotification.DeviceType = null;
                entityNotification.IsRead = false;
                entityNotification.NotificationType = type;
                entityNotification.RelatedId = relatedId;
                entityNotification.Text = text;
                entityNotification.UserId = userIds;
                db.Notifications.Add(entityNotification);
                db.SaveChanges();

                entityNotification.NotificationReadStatus.Add(new NotificationReadStatu
                {
                    NotificationId = entityNotification.NotificationId,
                    ToUserId = userIds,
                    IsRead = false
                });
                db.SaveChanges();

                List<Entity.Device> entityDeviceList = new List<Entity.Device>();

                var entityUserDeviceList = db.Devices.Where(e => (e.TokenType != AccessTokenType.Doc2Us || e.RegistrationId != null) && e.AccessToken != null && e.UserId != null && e.UserId.Value == userIds);
                if (entityUserDeviceList.Count() > 0)
                {
                    foreach (var entityUserDevice in entityUserDeviceList)
                    {
                        entityDeviceList.Add(entityUserDevice);
                    }
                }

                Lib.Notification notification = new Lib.Notification
                {
                    Title = text,
                    ActionType = type,
                    ActionContent = relatedId.ToString()
                };
                if (entityPromoUrl != null)
                {
                    notification.Items.Add("URL", entityPromoUrl.Url);
                }
                notification.Items.Add("sound", "default");
                notification.Items.Add("NotificationId", entityNotification.NotificationId.ToString());

                if (extraParams != null && extraParams.Count > 0)
                {
                    foreach (var notificationParam in extraParams)
                    {
                        notification.Items.Add(notificationParam.Key, notificationParam.Value);
                    }
                }
                await PushManager.Push(entityDeviceList, notification);
            }
            catch (Exception ex)
            {
                logger.Error("PushToUsers Exception: " + ex.ToString());
            }
        }
        public static void CreateNotification(Entity.db_HeyDocEntities db, PnActionType type, string relatedId, string text, Entity.PromotionUrl entityPromoUrl, int id)
        {
            var entityNotification = db.Notifications.Create();
            entityNotification.CreateDate = DateTime.UtcNow;
            entityNotification.DeviceType = null;
            entityNotification.IsRead = false;
            entityNotification.NotificationType = type;
            entityNotification.RelatedId = relatedId;
            entityNotification.Text = text;
            entityNotification.UserId = id;
            db.Notifications.Add(entityNotification);
            db.SaveChanges();
            if (entityPromoUrl != null)
            {
                entityNotification.PromotionUrl = entityPromoUrl;
            }
            entityNotification.NotificationReadStatus.Add(new NotificationReadStatu
            {
                NotificationId = entityNotification.NotificationId,
                ToUserId = id,
                IsRead = false
            });
            db.SaveChanges();           
        }

        public static List<NotificationModel> GetAllNotification(string accessToken, int skip, int take)
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                take = take > 50 ? 50 : take;
                var entityUser = AccountService.GetEntityUserByAccessToken(db, accessToken, false);
                var entityNotifications = db.Notifications.Where(e => (e.UserId == entityUser.UserId) && e.CreateDate > entityUser.CreateDate);
                entityNotifications = entityNotifications.OrderByDescending(e => e.CreateDate).Skip(skip).Take(take);
                List<NotificationModel> result = new List<NotificationModel>();
                foreach (var notification in entityNotifications)
                {
                    result.Add(new NotificationModel(notification));
                }
                return result;
            }
        }

        public static BoolResult ReadNotification(string accessToken, long notificationId)
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityUser = AccountService.GetEntityUserByAccessToken(db, accessToken, false);
                var entityNotification = db.Notifications.FirstOrDefault(e => e.NotificationId == notificationId);

                if (entityNotification.UserId == entityUser.UserId)
                {
                    entityNotification.IsRead = true;
                }
                else
                {
                    var entityReadStatus = entityNotification.NotificationReadStatus.FirstOrDefault(s => s.ToUserId == entityUser.UserId);
                    if (entityReadStatus == null)
                    {
                        throw new WebApiException(new WebApiError(WebApiErrorCode.NotFound, "Notification not found"));
                    }

                    entityReadStatus.IsRead = true;
                }
                db.SaveChanges();
                return new BoolResult(true);
            }
        }

        public static BoolResult ReadAll(string accessToken)
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityUser = AccountService.GetEntityUserByAccessToken(db, accessToken, false);
                var entityNotiications = db.Notifications.Where(e => e.UserId == entityUser.UserId);
                foreach (var entityNotification in entityNotiications)
                {
                    entityNotification.IsRead = true;
                }
                var entityReadStatuses = db.NotificationReadStatus.Where(e => e.ToUserId == entityUser.UserId);
                foreach (var entityReadStatus in entityReadStatuses)
                {
                    entityReadStatus.IsRead = true;
                }
                db.SaveChanges();
                return new BoolResult(true);
            }
        }

        public static async Task NotifyUser(db_HeyDocEntities db, CreateNotificationModel model)
        {
            IQueryable<Device> entityDeviceList = db.Devices.Where(e => (e.TokenType != AccessTokenType.Doc2Us || e.RegistrationId != null) && e.AccessToken != null && e.UserId != null);

            switch (model.DeviceType)
            {
                case DeviceType.IOS:
                    entityDeviceList = entityDeviceList.Where(e => e.DeviceType == DeviceType.IOS);
                    break;
                case DeviceType.Android:
                    entityDeviceList = entityDeviceList.Where(e => e.DeviceType == DeviceType.Android);
                    break;
                case DeviceType.Invalid:
                default:
                    break;
            }

            if (model.CorporateId > 0 || !string.IsNullOrEmpty(model.Email))
            {
                if (!string.IsNullOrEmpty(model.Email))
                {
                    var emailArray = model.Email.Split(';');
                    entityDeviceList = entityDeviceList.Where(e => emailArray.Contains(e.UserProfile.UserName));
                }
                if (model.CorporateId > 0)
                {
                    entityDeviceList = entityDeviceList.Where(e => e.UserProfile.CorporateId == model.CorporateId);
                }
                var uniqueUserDeviceList = entityDeviceList.Select(e => new { e.UserId }).Distinct();
                if (uniqueUserDeviceList.Any())
                {
                    var entityNotification = db.Notifications.Create();
                    entityNotification.CreateDate = DateTime.UtcNow;
                    entityNotification.DeviceType = null;
                    entityNotification.IsRead = false;
                    entityNotification.NotificationType = model.NotificationType;
                    entityNotification.RelatedId = model.URL;
                    entityNotification.Text = model.Text;
                    entityNotification.UserId = null;
                    try
                    {
                        entityNotification.NotificationTags = db.NotificationTags.ToList().Where(t => model.PnTagSelectList.Exists(m => int.Parse(m.Value) == t.TagId && m.Selected)).ToList();
                    } catch (Exception) { }
                    db.Notifications.Add(entityNotification);
                    db.SaveChanges();

                    uniqueUserDeviceList.InsertFromQuery("NotificationReadStatus", x => new
                    {
                        entityNotification.NotificationId,
                        ToUserId = x.UserId.Value,
                        IsRead = false
                    });
                    db.SaveChanges();
                }
            }
            else
            {
                var entityNotification = db.Notifications.Create();
                entityNotification.CreateDate = DateTime.UtcNow;
                entityNotification.DeviceType = null;
                entityNotification.IsRead = false;
                entityNotification.NotificationType = model.NotificationType;
                entityNotification.RelatedId = model.URL;
                entityNotification.Text = model.Text;
                entityNotification.UserId = null;
                try
                {
                    entityNotification.NotificationTags = db.NotificationTags.ToList().Where(t => model.PnTagSelectList.Exists(m => int.Parse(m.Value) == t.TagId && m.Selected)).ToList();
                } catch (Exception) { }
                db.Notifications.Add(entityNotification);
                db.SaveChanges();

                entityDeviceList.Select(e => new { e.UserId }).Distinct().InsertFromQuery("NotificationReadStatus", x => new
                {
                    entityNotification.NotificationId,
                    ToUserId = x.UserId.Value,
                    IsRead = false
                });
                db.SaveChanges();
            }

            var entityDevices = entityDeviceList.ToList();

            Lib.Notification notification = new Lib.Notification
            {
                Id = model.NotificationId,
                Title = model.Text.Truncate(499),
                ActionType = model.NotificationType,
                ActionContent = model.URL
            };
            if (model.NotificationType == PnActionType.URL)
            {
                notification.AddItem("URL", model.URL);
            }

            await PushManager.Push(entityDevices, notification);
        }

        public static List<NotificationTagModel> GetAllPnTags(db_HeyDocEntities db)
        {
            return db.NotificationTags.Select(e => new NotificationTagModel
            {
                TagId = e.TagId,
                TagName = e.TagName
            }).ToList();
        }

    }
}
