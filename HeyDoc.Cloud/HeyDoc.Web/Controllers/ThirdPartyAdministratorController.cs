using HeyDoc.Web.Entity;
using HeyDoc.Web.Helpers;
using HeyDoc.Web.Models;
using HeyDoc.Web.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HeyDoc.Web.Controllers
{
    public class ThirdPartyAdministratorController : Controller
    {
        public ActionResult Index()
        {
            using (var db = new db_HeyDocEntities())
            {
                ViewBag.Pharmacies = _GetPharmacySelectList(db, true);
            }

            return View();
        }

        [HttpPost]
        public JsonResult GetTPAList(int start, int length, List<DataTableOrderOptions> order, int pharmacyRoleId)
        {
            int recordsTotal, recordsFiltered;
            List<TPAModel> data = TPAService.GetTPAList(0, -1);

            recordsTotal = data.Count();

            if (pharmacyRoleId > 0)
            {
                data = data.Where(e => e.SupplyingPharmacyIds.Any(id => id == pharmacyRoleId)).ToList();
            }

            recordsFiltered = data.Count();

            if (order[0].dir == "desc")
            {
                data = data.OrderByDescending(e => e.TPAName).ToList();
            }
            else
            {
                data = data.OrderBy(e => e.TPAName).ToList();
            }

            data = data.Skip(start).Take(length).ToList();

            return Json(new
            {
                recordsTotal,
                recordsFiltered,
                data
            });
        }

        public ActionResult Add()
        {
            return _GetAddView();
        }

        [HttpPost]
        public ActionResult Add(TPAModel model)
        {
            try
            {
                TPAService.AddTPA(model);
                return RedirectToAction("Index");
            }
            catch
            {
                ModelState.AddModelError("", "Server Error!");
                return _GetAddView();
            }
        }

        public ActionResult Edit(int tpaId)
        {
            return _GetEditView(tpaId);
        }

        [HttpPost]
        public ActionResult Edit(TPAModel model)
        {
            try
            {
                TPAService.UpdateTPA(model);
                return RedirectToAction("Index");
            }
            catch
            {
                ModelState.AddModelError("", "Server Error!");
                return _GetEditView(model.TPAId);
            }
        }

        public JsonResult Delete(int tpaId)
        {
            using (var db = new db_HeyDocEntities())
            {
                var entityTPA = TPAService.GetTPAById(db, tpaId);
                entityTPA.IsDelete = true;
                db.SaveChanges();
            }
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        private ActionResult _GetAddView()
        {
            using (var db = new db_HeyDocEntities())
            {
                ViewBag.Pharmacies = _GetPharmacySelectList(db);
                return View();
            }
        }

        private ActionResult _GetEditView(int tpaId)
        {
            using (var db = new db_HeyDocEntities())
            {
                ViewBag.Pharmacies = _GetPharmacySelectList(db);

                var entityTPA = TPAService.GetTPAById(db, tpaId);
                return View(new TPAModel(entityTPA));
            }
        }

        private List<SelectListItem> _GetPharmacySelectList(db_HeyDocEntities db, bool includeAllOption = false)
        {
            var res = new List<SelectListItem>();

            if (includeAllOption)
            {
                res.Add(new SelectListItem { Text = "All", Value = "0" });
            }

            res.AddRange(PrescriptionSourceService.GetPrescriptionSourceSelectListExcludingDoctors(db));

            return res;
        }
    }
}