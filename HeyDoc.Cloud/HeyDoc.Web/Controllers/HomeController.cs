using HeyDoc.Web.Helpers;
using HeyDoc.Web.Lib;
using HeyDoc.Web.Models;
using HeyDoc.Web.Services;
using HeyDoc.Web.WebApi;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
namespace HeyDoc.Web.Controllers
{
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class HomeController :  BaseController
    {
        public ActionResult Index()
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                ViewBag.TotalUsers = db.Patients.Where(e=>!e.UserProfile.IsDelete).Count();
                ViewBag.TotalDoctors = db.Doctors.Where(e => e.IsVerified && !e.UserProfile.IsDelete).Count();
                ViewBag.TotalPartnes = db.Doctors.Where(e => e.IsVerified && !e.UserProfile.IsDelete && e.IsPartner).Count();
            }
            return View();
        }

        [HttpPost]
        public JsonResult GetGraphData(DateTime startDate, DateTime endDate)
        {
            var initialStartDate = startDate = startDate.Date;
            endDate = endDate.Date.AddHours(23).AddMinutes(59);
            int totalDays = (endDate - startDate).Days;
            if (totalDays > 31)
            {
                throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidArguments, "Date range must be less than 30 days"));
            }
            List<GraphModel> result = new List<GraphModel>();
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                do
                {
                    var entityPaymentAmounts = db.PaymentRequests.Where(e => e.PaymentStatus == PaymentStatus.Paid && e.CreateDate < startDate && e.CreateDate > initialStartDate).Select(e=>e.Amount).DefaultIfEmpty();
                    decimal amount = 0;
                    if (entityPaymentAmounts != null)
                    {
                        amount = entityPaymentAmounts.Sum();
                    }
                    result.Add(new GraphModel(startDate.ToString("dd-MMM"), amount.ToString()));
                    startDate = startDate.AddDays(7);
                    if (startDate > endDate && (startDate - endDate).Days < 7)
                    {
                        startDate = endDate;
                    }
                }
                while (startDate <= endDate);
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}
