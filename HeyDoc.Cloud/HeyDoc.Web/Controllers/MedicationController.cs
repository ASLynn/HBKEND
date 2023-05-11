using HeyDoc.Web.Lib;
using HeyDoc.Web.Models;
using HeyDoc.Web.Services;
using HeyDoc.Web.WebApi;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace HeyDoc.Web.Controllers
{
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class MedicationController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public JsonResult GetList(string searchKey = "")
        {
            int take, skip, recordsTotal, recordsFiltered;
            List<MedicationModel> data = new List<MedicationModel>();
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                take = Convert.ToInt32(Request.Form["length"]);
                skip = Convert.ToInt32(Request.Form["start"]);

                var entityMedicationList = db.Medications.Where(e => !e.IsDelete && ((e.MedicationName.Contains(searchKey)) || (e.MedicationName.Contains(searchKey))));
                recordsTotal = entityMedicationList.Count();
                recordsFiltered = recordsTotal;

                entityMedicationList = entityMedicationList.OrderBy(e => e.MedicationName).Skip(skip).Take(take);
                foreach (var entityMedication in entityMedicationList)
                {
                    var Data = new MedicationModel(entityMedication);
                    data.Add(Data);
                }
            }
            return Json(new
            {
                recordsTotal = recordsTotal,
                recordsFiltered = recordsFiltered,
                data = data,
            });
        }

        public ActionResult Add()
        {
            ViewBag.MedicalConditions = MedicalConditionService.GetList();
            return View(new MedicationModel());
        }

        [HttpPost]
        public ActionResult Add(MedicationModel model)
        {
            try
            {
                MedicationService.AddMedication(model);
            }
            catch (Exception e)
            {
                ModelState.AddModelError("", "Server Error!");
            }
            return RedirectToAction("Index");
        }

        public ActionResult Edit(long medicationId)
        {
            var result = MedicationService.WebGetMedication(User.Identity.Name, medicationId);

            ViewBag.MedicalConditions = MedicalConditionService.GetList();

            return View(result);
        }

        [HttpPost]
        public ActionResult Edit(MedicationModel model)
        {
            try
            {
                var result =  MedicationService.EditMedication(model);
            }
            catch (Exception e)
            {
                ModelState.AddModelError("", "Server Error!");
            }
            return RedirectToAction("Index");
        }

        public JsonResult Delete(long medicationId)
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityMedication = MedicationService.GetEntityMedicationById(db, medicationId);
                entityMedication.IsDelete = true;
                db.SaveChanges();
            }
            return Json(true, JsonRequestBehavior.AllowGet);
        }
    }
}