using HeyDoc.Web.Models;
using HeyDoc.Web.Resources;
using HeyDoc.Web.WebApi;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace HeyDoc.Web.Services
{
    public class QueueMedService
    { 
        private static string authKey1 = "";
        private static string authKey2 = "";

        public static async Task<BoolResult> SendNotificationAppointmentDoctor(QueuesMedNotificationModel model, string P1, string P2)
        {
            if (!P1.Equals(authKey1) || !P2.Equals(authKey2))
                throw new WebApiException(new WebApiError(WebApiErrorCode.BadRequest, Errors.InvalidAction));

            model.Validate();

            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var doctor = db.Doctors.OrderByDescending(o => o.UserId).FirstOrDefault(e => e.UserId == model.UserId);//this API is check by registration number
                if (doctor == null)
                {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.NotFound, Account.ErrorUserNotFound));
                }

                await NotificationService.NotifyUser(db, doctor.UserId, PnActionType.AppointmentMade, "", model.Message);
            }

            return new BoolResult(true);
        }

        public static async Task<BoolResult> SendNotificationUserAppointmentStatusUpdate(QueuesMedNotificationModel model, string P1, string P2)
        {
            if (!P1.Equals(authKey1) || !P2.Equals(authKey2))
                throw new WebApiException(new WebApiError(WebApiErrorCode.BadRequest, Errors.InvalidAction));

            model.Validate();
               
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var user = db.Patients.FirstOrDefault(e => e.UserId == model.UserId);
                if (user == null)
                {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.NotFound, Account.ErrorUserNotFound));
                }

                await NotificationService.NotifyUser(db, user.UserId, PnActionType.AppointmentStatusUpdate, "", model.Message);
            }

            return new BoolResult(true);
        }

        public static GenericDataModel<CategoryModel> QueueMedReadyCategories(string accessToken, int take = 10, int skip = 0)
        {
            if (take > 100)
            {
                take = 100;
            }

            var result = new GenericDataModel<CategoryModel>(); 
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityUser = AccountService.GetEntityUserByAccessToken(db, accessToken, false);
                var isCorporateUser = entityUser.CorporateId.HasValue;

                var entityCategories = db.Categories.Where(e => !e.IsDelete);
                if (isCorporateUser)
                {
                    entityCategories = entityCategories.Where(e => e.IsQueueMedCorporateUser);
                }
                else
                {
                    entityCategories = entityCategories.Where(e => e.IsQueueMedPublicUser);
                }
                result.Total = entityCategories.Select(s => s.CategoryId).Count();
                var data = entityCategories.OrderBy(o => o.CategoryId).Skip(skip).Take(take);

                var categoryList = new List<CategoryModel>();
                foreach (var entityCategory in data)
                {
                    var category = new CategoryModel(entityCategory);
                    categoryList.Add(category);
                }
                result.Data = categoryList;
            }
            return result;
        }
    }
}