using HeyDoc.Web;
using HeyDoc.Web.Resources;
using HeyDoc.Web.WebApi;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HeyDoc.Web.Models
{

    public class NotificationModell
    {
        public int UserId { get; set; }
        public int FromUserId { get; set; }

        [DisplayName("Type")]
        public PnActionType NotificationType { get; set; }
        public string RelatedId { get; set; }
        [Required]
        public string Text { get; set; }
        public string URL { get; set; }
        public string Email { get; set; }
        public DateTime CreateDate { get; set; }

        public bool IsRead { get; set; }

        public NotificationModell()
        {
        }

        public NotificationModell(Entity.Notification entityNotification)
        {
            NotificationType = entityNotification.NotificationType;
            RelatedId = entityNotification.RelatedId;
            Text = entityNotification.Text;
            if (entityNotification.PromotionUrl != null)
            {
                URL = entityNotification.PromotionUrl.Url;
            }
            else if (entityNotification.NotificationType == PnActionType.URL)
            {
                URL = entityNotification.RelatedId;
            }
            CreateDate = entityNotification.CreateDate;
            IsRead = entityNotification.IsRead;
        }
    }

    public class CreateNotificationModell : NotificationModell
    {
        public DeviceType DeviceType { get; set; }
        [Required]
        [DisplayName("Corporate")]
        public long CorporateId { get; set; }
        public DateTimeOffset? ScheduledDateTime { get; set; }
        public List<SelectListItem> PnTagSelectList { get; set; }
        public CreateNotificationModell() { }
    }


    public class QueuesMedNotificationModell
    {
        public int UserId { get; set; }
        public string Message { get; set; }

        public void Validate()
        {
            if (UserId <= 0)
            {
                throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidArguments, Account.ErrorUserNotFound));
            }

            if (string.IsNullOrEmpty(Message))
            {
                throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidArguments, Forms.ErrorMessageNull));
            }

            if (Message.Length > 100)
            {
                throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidArguments, Forms.ErrorMessageTooLong));
            }
        }

    }

    public class EnqueuedNotificationModell
    {
        public long ServiceBusQueueSequenceNumber { get; set; }
        public CreateNotificationModel CreateNotificationModel { get; set; }
    }
}