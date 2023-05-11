using HeyDoc.Web.Entity;
using HeyDoc.Web.Helpers;
using HeyDoc.Web.Lib;
using HeyDoc.Web.Models;
using HeyDoc.Web.Services;
using HeyDoc.Web.WebApi;
using Newtonsoft.Json;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using WebMatrix.WebData;

namespace HeyDoc.Web.Controllers
{
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class CorporateController : BaseController
    {
        public ActionResult Index()
        {
            var tpaSelectList = new List<SelectListItem>
            {
                new SelectListItem
                {
                    Text = "All",
                    Value = ""
                }
            };

            using (var db = new db_HeyDocEntities())
            {
                var entityTPAs = db.ThirdPartyAdministrators.Where(e => !e.IsDelete);
                tpaSelectList.AddRange(entityTPAs.Select(e => new SelectListItem { Text = e.Name, Value = e.TPAId.ToString() }));
            }

            tpaSelectList.Add(new SelectListItem
            {
                Text = "-",
                Value = "-1"
            });

            ViewBag.TPAs = tpaSelectList;
            return View();
        }

        public ActionResult Create()
        {
            CorporateModel model = new CorporateModel()
            {
                PolicySupplyDurationInMonths = 1
            };

            return GetCreateView(model);
        }

        [HttpPost]
        public ActionResult Create(CorporateModel model)
        {
            if (!ModelState.IsValid)
            {
                return GetCreateView(model);
            }

            try
            {
                using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
                {
                    var entityCorporate = CorporateService.CreateCorporate(db, model);
                }

                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Server error. Please try again");
            }
            return GetCreateView(model);
        }

        private ActionResult GetCreateView(CorporateModel model)
        {
            var TPAList = new List<SelectListItem>() { new SelectListItem() { Text = "Independent", Value = "0" } };
            ViewBag.TPAList = TPAList;

            var pharmacyList = new List<SelectListItem>();
            ViewBag.PharmacyList = pharmacyList;

            using (var db = new Entity.db_HeyDocEntities())
            {
                TPAList.AddRange(db.ThirdPartyAdministrators.Where(e => !e.IsDelete).OrderBy(e => e.Name).Select(e => new SelectListItem { Text = e.Name, Value = e.TPAId.ToString() }));                

                pharmacyList.AddRange(PrescriptionSourceService.GetPrescriptionSourceSelectListExcludingDoctors(db));
            }
            return View(model);
        }

        public ActionResult Edit(int corporateId)
        {
            CorporateModel model = CorporateService.GetCorporateById(corporateId, true);
            return GetEditView(model);
        }

        [HttpPost]
        public ActionResult Edit(CorporateModel model)
        {
            if (!ModelState.IsValid)
            {
                return GetEditView(model);
            }

            try
            {
                using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
                {
                    var result = CorporateService.UpdateCorporate(db, model);
                }
                return RedirectToAction("Index");
            }
            catch
            {
                ModelState.AddModelError("", "Server error. Please try again");
                return GetEditView(model);
            }
        }

        private ActionResult GetEditView(CorporateModel model)
        {
            var TPAList = new List<SelectListItem>() { new SelectListItem() { Text = "Independent", Value = "0" } };
            ViewBag.TPAList = TPAList;

            var pharmacyList = new List<SelectListItem>();
            ViewBag.PharmacyList = pharmacyList;

            using (var db = new Entity.db_HeyDocEntities())
            {
                TPAList.AddRange(db.ThirdPartyAdministrators.Where(e => !e.IsDelete).OrderBy(e => e.Name).Select(e => new SelectListItem { Text = e.Name, Value = e.TPAId.ToString() }));

                pharmacyList.AddRange(PrescriptionSourceService.GetPrescriptionSourceSelectListExcludingDoctors(db));
            }
            
            return View(model);
        }

        [HttpPost, ValidateInput(false)]
        public JsonResult GetCorporateList(List<DataTableOrderOptions> order, List<DataTableColumnProps> columns, string searchKey = "", string tpaId = "-1")
        {
            int take, skip, recordsTotal, recordsFiltered;
            List<CorporateModel> data = new List<CorporateModel>();
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                take = Convert.ToInt32(Request.Form["length"]);
                skip = Convert.ToInt32(Request.Form["start"]);

                var entityCorporateList = db.Corporates.Where(e => !e.IsDelete);
                recordsTotal = entityCorporateList.Count();
                if (!string.IsNullOrEmpty(searchKey))
                {
                    entityCorporateList = entityCorporateList.Where(e => e.BranchName.Contains(searchKey));
                }
                if (!string.IsNullOrEmpty(tpaId))
                {
                    if (tpaId == "-1")
                    {
                        entityCorporateList = entityCorporateList.Where(e => !e.TPAId.HasValue);
                    }
                    else
                    {
                        entityCorporateList = entityCorporateList.Where(e => e.TPAId.ToString() == tpaId);
                    }
                }

                recordsFiltered = entityCorporateList.Count();

                var o = order[0];
                var sortProperty = columns[o.column].name;
                switch (sortProperty)
                {
                    case "CreatedDate":
                        entityCorporateList = entityCorporateList.DynamicOrderBy(e => e.CreatedDate, o.dir == "desc");
                        break;
                    case "TPAName":
                        entityCorporateList = entityCorporateList.DynamicOrderBy(e => e.ThirdPartyAdministrator.Name, o.dir == "desc");
                        break;
                    case "BranchName":
                    default:
                        entityCorporateList = entityCorporateList.DynamicOrderBy(e => e.BranchName, o.dir == "desc");
                        break;
                }

                //entityPrescriptionList = entityPrescriptionList.OrderBy(e => e.PrescriptionDispatchs.FirstOrDefault().CreatedDate).Skip(skip).Take(take);
                entityCorporateList = entityCorporateList.Skip(skip).Take(take).Include(e => e.ThirdPartyAdministrator);

                foreach (var entityCorporate in entityCorporateList)
                {
                    data.Add(new CorporateModel(entityCorporate, true));
                }
            }

            return Json(new
            {
                recordsTotal = recordsTotal,
                recordsFiltered = recordsFiltered,
                data = data,
            });
        }


        public ActionResult BranchList(int corporateId)
        {
            CorporateModel model = CorporateService.GetCorporateById(corporateId); 
            
            return View(model);
        }

        [HttpPost]
        public JsonResult GetBranchList(string searchKey = "", int corporateId = 0)
        {
            int take, skip, recordsTotal, recordsFiltered;
            List<BranchModel> data = new List<BranchModel>();
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                take = Convert.ToInt32(Request.Form["length"]);
                skip = Convert.ToInt32(Request.Form["start"]);

                int sortParam = Convert.ToInt32(Request.Form["order[0][column]"]);
                string sortOrder = Request.Form["order[0][dir]"];

                var entityBranchList = db.Branchs.Where(e => !e.IsDelete && e.CorporateId == corporateId);
                recordsTotal = entityBranchList.Count();
                if (!string.IsNullOrEmpty(searchKey))
                {
                    entityBranchList = entityBranchList.Where(e => e.BranchName.Contains(searchKey));
                }

                entityBranchList = entityBranchList.OrderByDescending(e => e.CreatedDate);
                recordsFiltered = entityBranchList.Count();

                //entityPrescriptionList = entityPrescriptionList.OrderBy(e => e.PrescriptionDispatchs.FirstOrDefault().CreatedDate).Skip(skip).Take(take);
                entityBranchList = entityBranchList.Skip(skip).Take(take);

                foreach (var entityBranch in entityBranchList)
                {
                    data.Add(new BranchModel(entityBranch));
                }
            }

            return Json(new
            {
                recordsTotal = recordsTotal,
                recordsFiltered = recordsFiltered,
                data = data,
            });
        }
        [HttpPost]
        public JsonResult GetPositionList(int corporateId = 0)
        {
            int recordsTotal;
            List<CorporatePositionModel> data = new List<CorporatePositionModel>();
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityPositionList = db.CorporatePositions.Where(e => e.CorporateId == corporateId).OrderBy(e => e.Position);
                recordsTotal = entityPositionList.Count();
                foreach (var entityPosition in entityPositionList)
                {
                    data.Add(new CorporatePositionModel(entityPosition));
                }
            }

            return Json(new
            {
                recordsTotal = recordsTotal,
                recordsFiltered = 0,
                data = data,
            });
        }
        public ActionResult CreateBranch(int corporateId)
        {
            BranchModel model = new BranchModel();
            ViewBag.CorpId = corporateId;
            model.CorporateId = corporateId;
            return View(model);
        }

        [HttpPost]
        public ActionResult CreateBranch(BranchModel model)
        {
            ViewBag.CorpId = model.CorporateId;
            bool errorPass = false;
            if (string.IsNullOrEmpty(model.BranchName))
            {
                ModelState.AddModelError("Name is empty", "Name is empty");
                errorPass = true;
            }

            if (string.IsNullOrEmpty(model.BranchAdress))
            {
                ModelState.AddModelError("Address is empty", "Address is empty");
                errorPass = true;
            }

            if (errorPass)
            {
                return View(model);
            }

            try
            {
                using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
                {
                    var entityBranch = BranchService.CreateBranch(db, model);
                }

                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                ModelState.AddModelError("", "Server Error!");
            }
            return View(model);
        }

        public ActionResult EditBranch(int branchId)
        {
            BranchModel model = BranchService.GetBranchById(branchId);
            return View(model);
        }
        public ActionResult EditPosition(int positionId)
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var result = db.CorporatePositions.Where(e => e.PositionId == positionId).FirstOrDefault();
                CorporatePositionModel cp = new CorporatePositionModel(result);
                return View(cp);
            }           
        }
        [HttpPost]
        public ActionResult EditBranch(BranchModel model)
        {
            bool errorPass = false;
            if (string.IsNullOrEmpty(model.BranchName))
            {
                ModelState.AddModelError("Name is empty", "Name is empty");
                errorPass = true;
            }

            if (string.IsNullOrEmpty(model.BranchAdress))
            {
                ModelState.AddModelError("Address is empty", "Address is empty");
                errorPass = true;
            }

            if (errorPass)
            {
                return View(model);
            }
            BranchModel result;
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                result = BranchService.UpdateBranch(db, model);
            }
            return RedirectToAction("Index");
        }
        public ActionResult CreatePosition(int corporateId)
        {
            CorporatePositionModel model = new CorporatePositionModel();
            ViewBag.CorpId = corporateId;
            model.CorporateId = corporateId;
            return View(model);
        }

        [HttpPost]
        public ActionResult CreatePosition(CorporatePositionModel model)
        {
            ViewBag.CorpId = model.CorporateId;
            bool errorPass = false;
            if (string.IsNullOrEmpty(model.Position))
            {
                ModelState.AddModelError("Name is empty", "Name is empty");
                errorPass = true;
            }
            if (errorPass)
            {
                return View(model);
            }

            try
            {
                using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
                {
                    Entity.CorporatePosition corporatePosition = new Entity.CorporatePosition();
                    corporatePosition.CorporateId=model.CorporateId;
                    corporatePosition.Position = model.Position;
                    corporatePosition.Active = 1;

                    db.CorporatePositions.Add(corporatePosition);
                    db.SaveChanges();                   
                }
                return View(model);
            }
            catch (Exception e)
            {
                ModelState.AddModelError("", "Server Error!");
            }
            return View(model);
        }
        [HttpPost]
        public ActionResult EditPosition(CorporatePositionModel model)
        {
            bool errorPass = false;
            if (string.IsNullOrEmpty(model.Position))
            {
                ModelState.AddModelError("Name is empty", "Name is empty");
                errorPass = true;
            }
            if (errorPass)
            {
                return View(model);
            }
            
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var result = db.CorporatePositions.Where(e => e.PositionId == model.PositionId).FirstOrDefault();
                result.Position = model.Position;
                db.SaveChanges();
                CorporatePositionModel cp = new CorporatePositionModel(result);
            }
            return RedirectToAction("Index");
        }
        [HttpPost]
        public JsonResult DeleteBranch(long branchId)
        {
            var result = CorporateService.DeleteBranch(branchId);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public ActionResult UploadBranch(int corporateId)
        {
            ViewBag.corporateId = corporateId;
            return View();
        }

        [HttpPost]
        public ActionResult UploadNewBranch(int corporateId, HttpPostedFileBase file)
        {
            var result = CorporateService.LoadBranchList(corporateId, file);
            return View("Index");
        }

        [HttpPost]
        public JsonResult DeletePosition(int positionId)
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var result = db.CorporatePositions.Where(e => e.PositionId == positionId).FirstOrDefault();
                if(result.Active == 0)
                {
                    result.Active = 1;
                }
                else
                {
                    result.Active = 0;
                }
                
                db.SaveChanges();             
                return Json(true, JsonRequestBehavior.AllowGet);
            }
           
        }

        [HttpPost]
        public JsonResult DeleteCorporate(int corporateId)
        {
            var result = CorporateService.DeleteCorporate(corporateId);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult BanCorporate(int corporateId)
        {
            var result = CorporateService.BanCorporate(corporateId);
            return Json(result);
        }

        [HttpPost]
        public JsonResult UnbanCorporate(int corporateId)
        {
            var result = CorporateService.UnbanCorporate(corporateId);
            return Json(result);
        }
    }
}