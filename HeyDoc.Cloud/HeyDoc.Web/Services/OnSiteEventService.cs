using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using HeyDoc.Web.Services;
using HeyDoc.Web.WebApi;
using HeyDoc.Web.Models;
using System.Threading.Tasks;

namespace HeyDoc.Web.Services
{
    public class OnSiteEventService
    {
        public static async Task CheckInUser(string userMedicalId, string eventCode)
        {
            using (var db = new Entity.db_HeyDocEntities())
            {
                await _CheckInUserAux(db, userMedicalId, eventCode, false);
            }
        }

        public static async Task CheckInUserNoRestrictions(string userMedicalId, string eventCode)
        {
            using (var db = new Entity.db_HeyDocEntities())
            {
                await _CheckInUserAux(db, userMedicalId, eventCode, true);
            }
        }

        private static async Task _CheckInUserAux(Entity.db_HeyDocEntities db, string userMedicalId, string eventCode, bool noRestrictions)
        {
            var entityEvent = db.OnSiteEvents.FirstOrDefault(e => e.Code == eventCode && (noRestrictions || e.IsActive));
            var entityUser = db.UserProfiles.Include(e => e.webpages_Membership).FirstOrDefault(e => e.MedicalId == userMedicalId && !e.IsDelete);
            var validationError = ValidateEventAndUser(entityEvent, entityUser, !noRestrictions);
            if (validationError != null)
            {
                throw new WebApiException(validationError);
            }

            var entityCheckIn = new Entity.OnSiteEventCheckIn()
            {
                OnSiteEvent = entityEvent,
                UserProfile = entityUser,
                CheckInDateTime = DateTime.UtcNow
            };

            var entityUserCorporate = entityUser.UserCorperates.FirstOrDefault();
            if (entityUserCorporate != null)
            {
                entityCheckIn.UserBranchId = entityUserCorporate.BranchId;
            }

            entityEvent.OnSiteEventCheckIns.Add(entityCheckIn);
            await NotificationService.NotifyUser(db, entityUser.UserId, PnActionType.Message, null, $"Hi {entityUser.FullName}, you have been checked into event: {entityEvent.Description}");
            db.SaveChanges();
        }

        public static UserModel GetUserDetails(string userMedicalId, string eventCode)
        {
            using (var db = new Entity.db_HeyDocEntities())
            {
                var entityEvent = db.OnSiteEvents.FirstOrDefault(e => e.Code == eventCode && e.IsActive);
                var entityUser = db.UserProfiles.Include(e => e.webpages_Membership).FirstOrDefault(e => e.MedicalId == userMedicalId && !e.IsDelete);
                var validationError = ValidateEventAndUser(entityEvent, entityUser);
                if (validationError != null)
                {
                    throw new WebApiException(validationError);
                }

                return new UserModel(entityUser);
            }
        }

        private static WebApiError ValidateEventAndUser(Entity.OnSiteEvent entityEvent, Entity.UserProfile entityUser, bool restrictCorporate = true)
        {
            if (entityEvent == null)
            {
                return new WebApiError(WebApiErrorCode.InvalidArguments, "Could not find event with entered code");
            }
            if (entityUser == null)
            {
                return new WebApiError(WebApiErrorCode.InvalidArguments, "Could not find user with entered ID");
            }
            if (entityUser.IsBan)
            {
                return new WebApiError(WebApiErrorCode.UserIsBanned);
            }
            if (restrictCorporate)
            {
                if (!entityUser.CorporateId.HasValue)
                {
                    return new WebApiError(WebApiErrorCode.InvalidArguments, "Patient is not registered under a corporate");
                }
                if (entityUser.CorporateId.Value != entityEvent.CorporateId)
                {
                    return new WebApiError(WebApiErrorCode.InvalidArguments, "Patient is not registered under the corporate this event is for");
                }
            }
            var isUserMissingInfo = false;
            var missingUserInfo = new List<string>();
            if (string.IsNullOrWhiteSpace(entityUser.UserName))
            {
                isUserMissingInfo = true;
                missingUserInfo.Add("Fill in email");
            }
            if (!(entityUser.webpages_Membership.IsConfirmed.HasValue && entityUser.webpages_Membership.IsConfirmed.Value))
            {
                isUserMissingInfo = true;
                missingUserInfo.Add("Verify email");
            }
            if (string.IsNullOrWhiteSpace(entityUser.IC))
            {
                isUserMissingInfo = true;
                missingUserInfo.Add("Fill in IC");
            }
            if (string.IsNullOrWhiteSpace(entityUser.FullName))
            {
                isUserMissingInfo = true;
                missingUserInfo.Add("Fill in name");
            }
            if (isUserMissingInfo)
            {
                return new WebApiError(WebApiErrorCode.InvalidArguments, $"Please ask the patient to check the following: {string.Join(", ", missingUserInfo)}");
            }

            return null;
        }

        public static OnSiteEventModel GetEventDetails(string eventCode)
        {
            using (var db = new Entity.db_HeyDocEntities())
            {
                var entityEvent = db.OnSiteEvents.FirstOrDefault(e => e.Code == eventCode);
                if (entityEvent == null)
                {
                    throw new Exception("Event not found.");
                }
                return new OnSiteEventModel(entityEvent);
            }
        }

        public static List<DateTime> GetCheckInsForUser(int userId, string eventCode)
        {
            using (var db = new Entity.db_HeyDocEntities())
            {
                var eventId = db.OnSiteEvents.Where(e => e.Code == eventCode).Select(e => e.Id).FirstOrDefault();
                if (eventId == 0)
                {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.NotFound, "Event not found"));
                }
                if (!db.UserProfiles.Any(e => e.UserId == userId))
                {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.NotFound, "User not found"));
                }

                return GetCheckInsForUserUnchecked(db, userId, eventId);
            }
        }

        public static OnSiteEventUserAndCheckInsModel GetUserDetailsAndCheckIns(string userMedicalId, string eventCode)
        {
            using (var db = new Entity.db_HeyDocEntities())
            {
                var entityEvent = db.OnSiteEvents.FirstOrDefault(e => e.Code == eventCode && e.IsActive);
                var entityUser = db.UserProfiles.Include(e => e.webpages_Membership).FirstOrDefault(e => e.MedicalId == userMedicalId && !e.IsDelete);
                var validationError = ValidateEventAndUser(entityEvent, entityUser);
                if (validationError != null)
                {
                    throw new WebApiException(validationError);
                }

                return new OnSiteEventUserAndCheckInsModel
                {
                    User = new UserModel(entityUser),
                    CheckInTimes = GetCheckInsForUserUnchecked(db, entityUser.UserId, entityEvent.Id)
                };
            }
        }

        private static List<DateTime> GetCheckInsForUserUnchecked(Entity.db_HeyDocEntities db, int userId, int eventId)
        {
            var entityCheckInList = db.OnSiteEventCheckIns.Where(e => e.EventId == eventId && e.UserId == userId);
            return entityCheckInList.Select(e => e.CheckInDateTime).ToList();
        }
    }
}