using HeyDoc.Web.Entity;
using HeyDoc.Web.Models;
using HeyDoc.Web.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HeyDoc.Web.Services
{
    public class DoctorService
    {
        internal static Entity.DoctorUserReview GetEntityUserReview(Entity.db_HeyDocEntities db, int doctorId)
        {
            Entity.DoctorUserReview entityDoctorReview = db.DoctorUserReviews.FirstOrDefault(e => e.DoctorId == doctorId && !e.UserProfile.IsDelete);

            if (entityDoctorReview == null)
            {
                throw new WebApiException(new WebApiError(WebApiErrorCode.NotFound));
            }

            return entityDoctorReview;
        }

        internal static IEnumerable<SelectListItem> GetDoctorsWithRole()
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                List<SelectListItem> doctorList = new List<SelectListItem>();
                var doctors = from u in db.UserProfiles
                              join d in db.Doctors on u.UserId equals d.UserId
                              where u.IsDelete == false && d.ShowInApp == true && d.IsVerified == true
                              orderby u.FullName
                              select u;

                var doc = doctors.Select(e => new SelectListItem()
                {
                    Text = e.FullName,
                    Value = e.UserId.ToString(),
                });
                foreach (var doctor in doc)
                {
                    doctorList.Add(doctor);
                }
                return doctorList;
            }
        }

        public static IQueryable<Doctor> GetDoctorsWithRole(db_HeyDocEntities db, RoleType role)
        {
            return db.Doctors.Where(e => !e.UserProfile.IsDelete && !e.UserProfile.IsBan && e.UserProfile.webpages_Roles.Any(r => r.RoleName == role.ToString()));
        }
    }
}
