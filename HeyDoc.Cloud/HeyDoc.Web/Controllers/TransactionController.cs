using HeyDoc.Web.Lib;
using HeyDoc.Web.Models;
using HeyDoc.Web.Services;
using HeyDoc.Web.WebApi;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using CsvHelper;
using System.Dynamic;
using CsvHelper.TypeConversion;

namespace HeyDoc.Web.Controllers
{
    [Authorize(Roles = "Admin,SuperAdmin,Doctor")]
    public class TransactionController : Controller
    {
        public ActionResult Index()
        {
            var categories = new List<SelectListItem>();
            categories.Add(new SelectListItem() { Selected = true, Text = "All", Value = "0" });
            categories.AddRange(TeamService.GetCategoryList());
            ViewBag.Categories = categories;
            if (User.IsInRole("Doctor"))
            {
                using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
                {
                    var entityDoctors = db.UserProfiles.Where(x => x.UserName == User.Identity.Name).FirstOrDefault();
                    ViewBag.DoctorName = entityDoctors.FullName;
                }
            }

           
            return View();
        }

        [HttpPost]
        public JsonResult GetPlatformTotal(DateTime? startDate, DateTime? endDate, int categoryId, long groupId)
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                IQueryable<Entity.PaymentRequest> entityPayments = db.PaymentRequests;
                if (startDate.HasValue)
                {
                    startDate = startDate.Value.Date;
                    entityPayments = entityPayments.Where(e => e.CreateDate > startDate.Value);
                }
                if (endDate.HasValue)
                {
                    endDate = endDate.Value.Date.AddDays(1);
                    entityPayments = entityPayments.Where(e => e.CreateDate < endDate.Value);
                }
                if (categoryId > 0)
                {
                    entityPayments = entityPayments.Where(e => e.ChatRoom.Doctor.CategoryId == categoryId);
                    if (groupId == 0)
                    {
                        entityPayments = entityPayments.Where(e => e.ChatRoom.Doctor.GroupId == null);
                    }
                    else if (groupId > 0)
                    {
                        entityPayments = entityPayments.Where(e => e.ChatRoom.Doctor.GroupId == groupId);
                    }
                }

                entityPayments = entityPayments.Where(e => e.PaymentStatus == PaymentStatus.Paid);
                var total = entityPayments.Select(e => e.PlatformAmount).DefaultIfEmpty(0).Sum();
                return Json(total, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult GetTransactionList(DateTime? startDate, DateTime? endDate, PaymentStatus? status, int categoryId, long groupId, string pName, string dName)
        {
            int take, skip, recordsTotal, recordsFiltered;
            List<PaymentRequestModel> data = new List<PaymentRequestModel>();
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                take = Convert.ToInt32(Request.Form["length"]);
                skip = Convert.ToInt32(Request.Form["start"]);
                int sortParam = Convert.ToInt32(Request.Form["order[0][column]"]);
                string sortOrder = Request.Form["order[0][dir]"];

                //IQueryable<Entity.PaymentRequest> entityPayments = db.PaymentRequests.Where(e => e.PaymentStatus != PaymentStatus.Requested);
                IQueryable<Entity.PaymentRequest> entityPayments = from t1 in db.PaymentRequests
                                                                   join t2 in db.ChatRooms on t1.ChatRoomId equals t2.ChatRoomId
                                                                   join t3 in db.UserProfiles on t2.PatientId equals t3.UserId                                                                   
                                                                   where (t1.CreateDate > startDate.Value) || (startDate.Value == null)
                                                                   where (t1.CreateDate < endDate.Value) || (endDate.Value == null)
                                                                   where (t1.PaymentStatus == status.Value) || (status == null)
                                                                   where ((t3.FullName.Contains(pName) && t3.UserId == t2.PatientId) || (pName == ""))                                                                   
                                                                   select t1;

                if(dName != "")
                {
                    entityPayments = entityPayments.Where(e => e.ChatRoom.Doctor.UserProfile.FullName.Contains(dName));
                }
                recordsTotal = db.PaymentRequests.Count();
                if (categoryId > 0)
                {
                    entityPayments = entityPayments.Where(e => e.ChatRoom.Doctor.CategoryId == categoryId);
                    if (groupId == 0)
                    {
                        entityPayments = entityPayments.Where(e => e.ChatRoom.Doctor.GroupId == null);
                    }
                    else if (groupId > 0)
                    {
                        entityPayments = entityPayments.Where(e => e.ChatRoom.Doctor.GroupId == groupId);
                    }
                }
                recordsFiltered = entityPayments.Count();

                switch (sortOrder)
                {
                    case "asc":
                        switch (sortParam)
                        {
                            case 9:
                                entityPayments = entityPayments.OrderBy(e => e.CreateDate);
                                break;

                            default:
                                entityPayments = entityPayments.OrderBy(e => e.CreateDate);
                                break;
                        }
                        break;
                    case "desc":
                        switch (sortParam)
                        {
                            case 9:
                                entityPayments = entityPayments.OrderByDescending(e => e.CreateDate);
                                break;

                            default:
                                entityPayments = entityPayments.OrderByDescending(e => e.CreateDate);
                                break;
                        }
                        break;
                }
                entityPayments = entityPayments.Skip(skip).Take(take);
                foreach (var entityPayment in entityPayments)
                {
                    data.Add(new PaymentRequestModel(entityPayment, true));
                }
            }
            return Json(new
            {
                recordsTotal = recordsTotal,
                recordsFiltered = recordsFiltered,
                data = data,
            });
        }

        [HttpGet]
        public FileResult ExportTransactionList(DateTime? startDate, DateTime? endDate, PaymentStatus? status, int? categoryId, long? groupId)
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                IQueryable<Entity.PaymentRequest> entityPayments = db.PaymentRequests.Where(e => e.PaymentStatus != PaymentStatus.Requested);

                if (startDate.HasValue)
                {
                    startDate = startDate.Value.Date;
                    entityPayments = entityPayments.Where(e => e.CreateDate > startDate.Value);
                }
                if (endDate.HasValue)
                {
                    endDate = endDate.Value.Date.AddDays(1);
                    entityPayments = entityPayments.Where(e => e.CreateDate < endDate.Value);
                }

                if (status.HasValue)
                {
                    entityPayments = entityPayments.Where(e => e.PaymentStatus == status.Value);
                }

                if (categoryId > 0)
                {
                    entityPayments = entityPayments.Where(e => e.ChatRoom.Doctor.CategoryId == categoryId);
                    if (groupId == 0)
                    {
                        entityPayments = entityPayments.Where(e => e.ChatRoom.Doctor.GroupId == null);
                    }
                    else if (groupId > 0)
                    {
                        entityPayments = entityPayments.Where(e => e.ChatRoom.Doctor.GroupId == groupId);
                    }
                }

                entityPayments = entityPayments.OrderByDescending(e => e.CreateDate);

                var exportList = from e in entityPayments
                                 let patient = e.ChatRoom.Patient.UserProfile
                                 let hcp = e.ChatRoom.Doctor.UserProfile
                                 let s = e.PaymentStatus
                                 let paymentStatus = s == PaymentStatus.Authorised ? "Authorised" :
                                                     s == PaymentStatus.Canceled ? "Cancelled" :
                                                     s == PaymentStatus.Paid ? "Paid" :
                                                     s == PaymentStatus.Failed ? "Failed" :
                                                     s == PaymentStatus.Requested ? "Pre-Authorisation" :
                                                     ""
                                 select new TransactionStatsModel
                                 {
                                     PaymentRequestId = e.PaymentRequestId,
                                     PatientName = patient.FullName,
                                     PatientEmail = patient.UserName,
                                     PatientBirthday = patient.Birthday,
                                     HcpName = hcp.FullName,
                                     HcpEmail = hcp.UserName,
                                     HcpBirthday = hcp.Birthday,
                                     HcpIsPartner = e.ChatRoom.Doctor.IsPartner,
                                     AmountPaid = e.Amount,
                                     OriginalAmount = e.ActualAmount,
                                     PlatformEarning = e.PlatformAmount,
                                     HcpAmount = e.HCPAmount,
                                     PaymentStatus = paymentStatus,
                                     PaymentRequestTime = e.CreateDate,
                                     BraintreeTransactionId = e.BrainTreeTransactionId,
                                     BraintreeTransactionStatus = e.BrainTreeTransactionStatus
                                 };

                using (var csvStatsWriter = new StringWriter())
                {
                    using (var csvStats = new CsvWriter(csvStatsWriter))
                    {
                        var options = new TypeConverterOptions { Formats = new[] { "yyyy-MM-dd HH:mm:ss" } };
                        csvStats.Configuration.TypeConverterOptionsCache.AddOptions<DateTime>(options);
                        csvStats.Configuration.TypeConverterOptionsCache.AddOptions<DateTime?>(options);
                        csvStats.WriteRecords(exportList);
                    }

                    if (!endDate.HasValue)
                    {
                        endDate = DateTime.UtcNow;
                    }

                    string filename;
                    if (startDate.HasValue)
                    {
                        filename = $"Transactions_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.csv";
                    }
                    else
                    {
                        filename = $"Transactions_to_{endDate:yyyyMMdd}.csv";
                    }
                    return File(Encoding.UTF8.GetBytes(csvStatsWriter.ToString()), "text/csv", filename);
                }
            }
        }

        
    }
}
