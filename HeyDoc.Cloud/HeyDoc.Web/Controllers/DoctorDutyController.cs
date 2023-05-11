using HeyDoc.Web.Models;
using HeyDoc.Web.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
namespace HeyDoc.Web.Controllers
{
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class DoctorDutyController : BaseController
    {
        public ActionResult Index()
        {
            var doctors = new List<SelectListItem>();
            doctors.AddRange(DoctorService.GetDoctorsWithRole());
            ViewBag.Doctors = doctors;
            DoctorDutyModel model = new DoctorDutyModel();            
            return View(model);
        }
        public ActionResult AddDutyTime()
        {
            int userid = int.Parse(Request.Form["UserId"].ToString());
            bool isMon = Request.Form["isMon"].Contains("true");
            bool isTue = Request.Form["isTue"].Contains("true");
            bool isWed = Request.Form["isWed"].Contains("true");
            bool isThu = Request.Form["isThu"].Contains("true");
            bool isFri = Request.Form["isFri"].Contains("true");
            bool isSat = Request.Form["isSat"].Contains("true");
            bool isSun = Request.Form["isSun"].Contains("true");
            if (isMon) { AddDay(userid, 1); }
            if (isTue) { AddDay(userid, 2); }
            if (isWed) { AddDay(userid, 3); }
            if (isThu) { AddDay(userid, 4); }
            if (isFri) { AddDay(userid, 5); }
            if (isSat) { AddDay(userid, 6); }
            if (isSun) { AddDay(userid, 7); }
            var doctors = new List<SelectListItem>();
            doctors.AddRange(DoctorService.GetDoctorsWithRole());
            ViewBag.Doctors = doctors;
            ViewBag.userid = userid;
            DoctorDutyModel model = new DoctorDutyModel();
            return View("index",model);
        }
        private void AddDay(int userid, int dayid)
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                Entity.DoctorDuty model = new Entity.DoctorDuty();
                model.UserId = userid;
                model.DayId = dayid;
                model.FromTime = TimeSpan.Parse(Request.Form["from"].ToString());
                model.ToTime = TimeSpan.Parse(Request.Form["to"].ToString());
                var existing = db.DoctorDuties.Where(e => e.UserId == userid && e.DayId == dayid).FirstOrDefault();
                if (existing == null)
                {
                    db.DoctorDuties.Add(model);
                }
                else
                {
                    existing.DayId = dayid;
                    existing.FromTime = TimeSpan.Parse(Request.Form["from"].ToString());
                    existing.ToTime = TimeSpan.Parse(Request.Form["to"].ToString());
                }
                db.SaveChanges();
            }
        }

        [HttpPost]
        public JsonResult GetDutyTime(int UserId)
        {            
            int recordsTotal, recordsFiltered;
            List<DoctorDutyModel> data = new List<DoctorDutyModel>();
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entitydd = db.DoctorDuties.Where(e => e.UserId == UserId).OrderBy(e => e.DayId);
                recordsTotal = entitydd.Count();
                recordsFiltered = entitydd.Count();
                foreach (var dd in entitydd)
                {
                    DoctorDutyModel Data = new DoctorDutyModel(dd);
                    data.Add(Data);
                }
                return Json(new
                {
                    recordsTotal,
                    recordsFiltered,
                    data,
                });
            }

        }
    }
}