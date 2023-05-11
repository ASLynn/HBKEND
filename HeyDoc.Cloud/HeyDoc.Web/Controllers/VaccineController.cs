using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HeyDoc.Web.Services;
using HeyDoc.Web.Models;
using System.Data.Entity.Validation;
using System.Threading.Tasks;
namespace HeyDoc.Web.Controllers
{
    public class VaccineController : BaseController
    {
        public ActionResult VaccineApproval()
        {
            //string status = "all"; //4 for All
            //VaccineService.GetVaccineInfoListForApproval(status);
            return View();
        }

        [HttpPost]
        public JsonResult GetVaccineInfoListForApproval(int draw, int length, int start, List<DataTableColumnProps> columns, string Name = "", string Status = "all")
        {

            int recordsTotal, recordsFiltered;
            var data = new List<VaccinatedUserModel>();
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityUserList = db.VaccinatedUsers.Where(e => e.VaccinatedUserInfo.VUserName.Contains(Name) && e.Status.ToLower() == (Status.ToLower() == "all" ? e.Status.ToLower() : Status.ToLower()));
                recordsTotal = entityUserList.Count();
                recordsFiltered = entityUserList.Count();



                //entityUserList = entityUserList.Skip(start).Take(length);

                foreach (var entityUser in entityUserList)
                {
                    VaccinatedUserModel Data = new VaccinatedUserModel(entityUser);
                    data.Add(Data);
                }
            }

            return Json(new
            {
                draw, // Draw call number from DataTables, used to correctly order the AJAX responses on client-side
                recordsTotal,
                recordsFiltered,
                data,
            });
        }
        [HttpGet]
        public ActionResult VaccineApprovalDetail(int Vaccination_Id)
        {
           
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                Entity.VaccinatedUser entityUser = db.VaccinatedUsers.Where(e => e.Vaccination_Id == Vaccination_Id).First();
               
                VaccinatedUserModel model = new VaccinatedUserModel(entityUser);               
                return View(model);
            }


        }
       
        [HttpPost]
        public async Task<ActionResult> VaccineApprovalDetail(string submit, VaccinatedUserModel model)
        {
            model.Status = submit == "Approve" ? "Approved" : "Rejected";

            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                Entity.VaccinatedUser entityUser = db.VaccinatedUsers.Where(e => e.Vaccination_Id == model.Vaccination_Id).First();
                if (model.RemarkReason != null)
                {
                    entityUser.RemarkReason = model.RemarkReason.Trim();
                }else
                {
                    model.RemarkReason = "";
                }
            
                entityUser.Status = model.Status.Trim();
               
                try
                {
                    if (submit == "Approve")
                    {
                        await NotificationService.NotifyUserForVaccine(db, entityUser.UserId, PnActionType.Message, entityUser.UserId.ToString(), $"Congratulation!! E-Vaccination Certificate for { model.VaccineDetailName.Trim() } { entityUser.Dose.Trim() } has been Approved");
                    }
                    else
                    {
                        await NotificationService.NotifyUserForVaccine(db, entityUser.UserId, PnActionType.Message, entityUser.UserId.ToString(), $"E-Vaccination Certificate for { model.VaccineDetailName.Trim() } { entityUser.Dose.Trim() } has been Rejected for reason : {model.RemarkReason.Trim()}");
                    }

                    db.SaveChanges();
                }
                catch (DbEntityValidationException e)
                {
                    foreach (var eve in e.EntityValidationErrors)
                    {
                        Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                            eve.Entry.Entity.GetType().Name, eve.Entry.State);
                        foreach (var ve in eve.ValidationErrors)
                        {
                            Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                                ve.PropertyName, ve.ErrorMessage);
                        }
                    }
                    throw;
                }

            }
            return RedirectToAction("vaccineapproval");

        }
        [HttpGet]
        public ActionResult testvideochat()
        {
            return View();
        }

    }
}