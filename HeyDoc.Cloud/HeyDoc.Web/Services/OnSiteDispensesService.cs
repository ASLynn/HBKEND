using HeyDoc.Web.Models;
using HeyDoc.Web.Resources;
using HeyDoc.Web.WebApi;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace HeyDoc.Web.Services
{
    public class OnSiteDispensesService
    {
        private static List<OnSiteDispenseModel> GetOnSiteList(Entity.db_HeyDocEntities db, int skip, int take)
        {
            if (take > 50)
            {
                take = 50;
            }
            List<OnSiteDispenseModel> modelList = new List<OnSiteDispenseModel>();
            var entityOnsiteDispens = db.OnSiteDispenses.Where(e => !e.IsDelete).OrderBy(e => e.OnSiteName).Skip(skip).Take(take).ToList();
            foreach (var onsite in entityOnsiteDispens)
            {
                modelList.Add(new OnSiteDispenseModel(onsite));
            }

            return modelList;
        }

        public static List<OnSiteDispenseModel> GetOnSiteListMobile(string accessToken, int skip, int take)
        {
            List<OnSiteDispenseModel> modelList;
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityUser = AccountService.GetEntityUserByAccessToken(db, accessToken, false);
                modelList = GetOnSiteList(db, skip, take);
            }
            return modelList;
        }

        public static BoolResult SetNextDispenseDate(Entity.db_HeyDocEntities db, long onSiteId, string nextDispenseDate, string nextDispenseTime)
        {
            var entityOnsite = db.OnSiteDispenses.FirstOrDefault(e => !e.IsDelete && e.OnSiteId == onSiteId);
            if (entityOnsite == null)
            {
                throw new WebApiException(new WebApiError(WebApiErrorCode.NotFound, Location.ErrorLocationNotFound));
            }
            OnSiteDispenseModel onSite = new OnSiteDispenseModel(entityOnsite);

            string datetime = nextDispenseDate + ' ' + nextDispenseTime;
            DateTime DateDispense = DateTime.ParseExact(datetime, "yyyy/MM/dd HH:mm", CultureInfo.InvariantCulture);

            if (DateDispense <= DateTime.UtcNow.Date)
            {
                throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidAction, Forms.ErrorDateNotInFuture));
            }

            var entityNewSelectionDateDispense = db.OnSiteDateSelections.Create();
            entityNewSelectionDateDispense.SelectionDate = DateDispense;
            entityNewSelectionDateDispense.OnSiteId = onSite.OnSiteId;
            entityNewSelectionDateDispense.CreatedDate = DateTime.UtcNow;
            entityNewSelectionDateDispense.IsDelete = false;

            db.OnSiteDateSelections.Add(entityNewSelectionDateDispense);
            db.SaveChanges();

            return new BoolResult(true);
        }

        public static BoolResult UpdateOnSite(Entity.db_HeyDocEntities db, OnSiteDispenseModel model)
        {
            var entityOnsite = db.OnSiteDispenses.FirstOrDefault(e => !e.IsDelete && e.OnSiteId == model.OnSiteId);
            if (entityOnsite == null)
            {
                throw new WebApiException(new WebApiError(WebApiErrorCode.NotFound, Location.ErrorLocationNotFound));
            }
            entityOnsite.OnSiteName = model.OnSiteName;
            entityOnsite.OnSiteAddress = model.OnSiteAddress;
            entityOnsite.OnSitePhoneNumber = model.PhoneNumber;
            db.SaveChanges();
            return new BoolResult(true);
        }

        public static OnSiteDispenseModel GetOnsiteById(int onSiteId)
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityOnsite = db.OnSiteDispenses.FirstOrDefault(e => !e.IsDelete && e.OnSiteId == onSiteId);
                if (entityOnsite == null)
                {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.NotFound, Location.ErrorLocationNotFound));
                }

                return new OnSiteDispenseModel(entityOnsite);
            }
            
        }
    }
}