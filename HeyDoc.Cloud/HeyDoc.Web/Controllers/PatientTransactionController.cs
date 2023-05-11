using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HeyDoc.Web.Entity;
using HeyDoc.Web.Models;
namespace HeyDoc.Web.Controllers
{
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class PatientTransactionController : Controller
    {
        public ActionResult Index(string patientID)
        {
            //var categories = new List<SelectListItem>();
            //categories.Add(new SelectListItem() { Selected = true, Text = "All", Value = "0" });
            //categories.AddRange(TeamService.GetCategoryList());
            //ViewBag.Categories = categories;
            UserModel model = new UserModel();
            model.UserId = 0;
            if (patientID != null)
            {
                int pid = Convert.ToInt32(patientID);
                using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
                {
                    var item = db.UserProfiles.Where(e => e.UserId == pid).FirstOrDefault();
                    model.UserId = item.UserId;
                    model.FullName = item.FullName;
                    //model.StateId = item.PointBalances.FirstOrDefault().Balance;
                    model.StateId = item.StateId;

                }
            }
            return View(model);
        }


        [HttpPost]
        public JsonResult GetPatientTransaction(DateTime? startDate, DateTime? endDate, int userId, string pName)
        {
            int take, skip, recordsFiltered;
            // begin::original code
            // List<PointTopupRequestModel> data = new List<PointTopupRequestModel>();
            // end::original code

            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var pointTopUpUserList = from p in db.PointTopupRequests
                                         join u in db.UserProfiles on p.UserID equals u.UserId into c
                                         from u in c.DefaultIfEmpty()
                                         where p.Status == "success"
                                         select new { p.RequestID, p.RequestSuccessDate, p.PaymentMethod, p.Amount, p.ID, p.UserID, u.FullName, u.PhoneNumber };

                take = Convert.ToInt32(Request.Form["length"]);
                skip = Convert.ToInt32(Request.Form["start"]);

                if (pName.Trim() != "")
                {
                    pointTopUpUserList = pointTopUpUserList.Where(e => e.FullName.Contains(pName));
                }
                if (userId != 0)
                {
                    pointTopUpUserList = pointTopUpUserList.Where(e => e.UserID == userId);
                }
                if (startDate.HasValue)
                {
                    startDate = startDate.Value.Date;
                    pointTopUpUserList = pointTopUpUserList.Where(e => e.RequestSuccessDate >= startDate.Value);
                }
                if (endDate.HasValue)
                {
                    endDate = endDate.Value.Date.AddDays(1);
                    pointTopUpUserList = pointTopUpUserList.Where(e => e.RequestSuccessDate <= endDate.Value);
                }
                recordsFiltered = pointTopUpUserList.Count();
                pointTopUpUserList = pointTopUpUserList.OrderByDescending(e => e.RequestSuccessDate).Skip(skip).Take(take);

                // begin::Original code 
                //IQueryable<Entity.PointTopupRequest> entity = db.PointTopupRequests.Where(e => e.Status == "success");                
                //if(pName.Trim() != "")
                //{
                //    entity = entity.Where(e => e.UserProfile.FullName.Contains(pName));
                //}
                //if(userId != 0)
                //{
                //    entity = entity.Where(e => e.UserID == userId);
                //}
                //if (startDate.HasValue)
                //{
                //    startDate = startDate.Value.Date;
                //    entity = entity.Where(e => e.RequestSuccessDate >= startDate.Value);
                //}
                //if (endDate.HasValue)
                //{
                //    endDate = endDate.Value.Date.AddDays(1);
                //    entity = entity.Where(e => e.RequestSuccessDate <= endDate.Value);
                //}
                //recordsFiltered = entity.Count();

                //entity = entity.OrderByDescending(e => e.RequestSuccessDate).Skip(skip).Take(take);
                //foreach (var ee in entity)
                //{
                //    data.Add(new PointTopupRequestModel(ee));
                //}
                //end::original code

                return Json(new
                {
                    recordsFiltered = recordsFiltered,
                    data = pointTopUpUserList.ToList()
                });
            }

            // begin::original
            //return Json(new
            //{
            //    recordsFiltered = recordsFiltered,
            //    data = data
            //}) ;
            // begin::end
        }

        [HttpPost]
        public JsonResult GetPatientTransactionSpend(DateTime? startDate, DateTime? endDate, int userId, string pName)
        {
            int take, skip, recordsFiltered;
            List<PaymentRequestModel> data = new List<PaymentRequestModel>();
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                take = Convert.ToInt32(Request.Form["length"]);
                skip = Convert.ToInt32(Request.Form["start"]);
                IQueryable<Entity.PaymentRequest> entityPayments = from t1 in db.PaymentRequests
                                                                   join t2 in db.ChatRooms on t1.ChatRoomId equals t2.ChatRoomId
                                                                   join t3 in db.UserProfiles on t2.PatientId equals t3.UserId
                                                                   where t1.PaymentStatus == PaymentStatus.Authorised || t1.PaymentStatus == PaymentStatus.Paid
                                                                   where (t1.CreateDate >= startDate.Value) || (startDate.Value == null)
                                                                   where (t1.CreateDate <= endDate.Value) || (endDate.Value == null)

                                                                   where (((t3.FullName.Contains(pName) && t3.UserId == t2.PatientId) || (pName == ""))) && (t3.UserId == userId || userId == 0)
                                                                   select t1;

                recordsFiltered = entityPayments.Count();
                entityPayments = entityPayments.OrderByDescending(e => e.CreateDate).Skip(skip).Take(take);
                foreach (var entityPayment in entityPayments)
                {
                    data.Add(new PaymentRequestModel(entityPayment, true));
                }
            }
            return Json(new
            {

                recordsFiltered = recordsFiltered,
                data = data,
            });

        }
    }
}