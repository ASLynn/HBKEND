using HeyDoc.Web.Lib;
using HeyDoc.Web.Models;
using HeyDoc.Web.Services;
using HeyDoc.Web.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Mvc;
using System.Data.Entity;
using WebMatrix.WebData;
using System.Transactions;

namespace HeyDoc.Web.Controllers
{
    [Authorize(Roles = "Admin,SuperAdmin,PharmacyManagement")]
    public class PharmacyOutletsController : BaseController
    {
        public ActionResult Index()
        {
            using (var db = new Entity.db_HeyDocEntities())
            {
                var outletList = new List<SelectListItem>
                {
                    new SelectListItem { Text = "All", Value = "-1" }
                };
                outletList.AddRange(PrescriptionSourceService.GetPrescriptionSourceSelectListExcludingDoctors(db));
                ViewBag.Outlets = outletList;
                return View();
            }
        }

        [HttpPost]
        public JsonResult GetList(int length, int start, List<DataTableOrderOptions> order, List<DataTableColumnProps> columns, int? draw, string searchKey = "", int prescriptionSourceId = -1)
        {
            int recordsTotal, recordsFiltered;
            var data = new List<UserModel>();
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var doctorSourceIds = ConstantHelper.DoctorPrescriptionSourceIds;
                var entitySources = db.PrescriptionSources.Where(p => !doctorSourceIds.Contains(p.PrescriptionSourceId));
                IQueryable<Entity.UserProfile> entityUserList;
                if (prescriptionSourceId == -1)
                {
                    entityUserList = entitySources.SelectMany(p => p.UserProfiles);
                }
                else
                {
                    entityUserList = entitySources.Where(p => p.PrescriptionSourceId == prescriptionSourceId).SelectMany(p => p.UserProfiles);
                }
                entityUserList = entityUserList.Where(e => !e.IsDelete);
                recordsTotal = entityUserList.Select(e => e.UserId).Count();

                if (!string.IsNullOrEmpty(searchKey))
                {
                    entityUserList = entityUserList.Where(e => e.FullName.Contains(searchKey) || e.UserName.Contains(searchKey));
                }

                if (order.Count > 0)
                {
                    var firstOrdering = true;
                    foreach (var o in order)
                    {
                        var sortProperty = columns[o.column].name;
                        bool descendingOrder;
                        switch (o.dir)
                        {
                            case "asc":
                                descendingOrder = false;
                                break;
                            case "desc":
                                descendingOrder = true;
                                break;
                            default:
                                return Json(new
                                {
                                    error = $"Invalid ordering direction: '{o.dir}' for property: '{sortProperty}'"
                                });
                        }

                        switch (sortProperty)
                        {
                            case "FullName":
                                entityUserList = entityUserList.DynamicOrderBy(e => e.FullName, descendingOrder, firstOrdering);
                                firstOrdering = false;
                                break;
                            case "UserName":
                                entityUserList = entityUserList.DynamicOrderBy(e => e.UserName, descendingOrder, firstOrdering);
                                firstOrdering = false;
                                break;
                            case "CreateDate":
                                entityUserList = entityUserList.DynamicOrderBy(e => e.CreateDate, descendingOrder, firstOrdering);
                                firstOrdering = false;
                                break;
                            default:
                                return Json(new
                                {
                                    error = $"Invalid sort property: '{sortProperty}'"
                                });
                        }
                    }
                }
                else
                {
                    // Default ordering if the request from DataTables didn't specify ordering
                    entityUserList = entityUserList.OrderBy(e => e.FullName);
                }

                recordsFiltered = entityUserList.Select(e => e.UserId).Count();

                entityUserList = entityUserList
                    .Include(e => e.webpages_Membership)
                    .Include(e => e.webpages_Roles)
                    .Include(e => e.Photo)
                    .Include(e => e.Doctor)
                    .Include(e => e.PrescriptionSource)
                    .Include(e => e.PrescriptionSource.Photo)
                    .Skip(start).Take(length);

                foreach (var entityUser in entityUserList)
                {
                    data.Add(new UserModel(entityUser));
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

        // GET: /Doctor/Create
        public ActionResult Create()
        {
            using (var db = new Entity.db_HeyDocEntities())
            {
                RegisterModel model = new RegisterModel();
                ViewBag.OutletTypes = PrescriptionSourceService.GetPrescriptionSourceSelectListExcludingDoctors(db);

                return View(model);
            }
        }

        [HttpPost]
        public ActionResult Create(RegisterModel model, HttpPostedFileBase imageFile)
        {
            (var pass, var errorMessage) = PasswordModel.CheckPassword(model.Password);
            if (!pass)
            {
                ModelState.AddModelError("Password", errorMessage);
            }
            using (var db = new Entity.db_HeyDocEntities())
            {
                if (!ModelState.IsValid)
                {
                    ViewBag.OutletTypes = PrescriptionSourceService.GetPrescriptionSourceSelectListExcludingDoctors(db);
                    return View(model);
                }
                try
                {
                    model.RoleType = RoleType.Pharmacy;
                    UserModel result = AccountService.CreateUser(model, model.RoleType, false);
                    if (result == null)
                    {
                        ModelState.AddModelError("", "Email already exists!");
                        ViewBag.OutletTypes = PrescriptionSourceService.GetPrescriptionSourceSelectListExcludingDoctors(db);
                        return View(model);
                    }
                    if (imageFile != null)
                    {
                        var photo = CreateOrUpdateImage(result.UserId, imageFile.InputStream);
                    }
                    return RedirectToAction("Index");
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("", "Server Error!");
                }
                ViewBag.OutletTypes = PrescriptionSourceService.GetPrescriptionSourceSelectListExcludingDoctors(db);
                return View(model);
            }
        }

        public ActionResult Edit(string outletId)
        {
            UserModel model = new UserModel();
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityUser = AccountService.GetEntityUserByUserId(db, outletId, false, true);
                model = new UserModel(entityUser);
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(UserModel model, HttpPostedFileBase imageFile)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    Entity.UserProfile entityUser;
                    using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
                    {
                        DateTime now = DateTime.UtcNow;
                        entityUser = AccountService.GetEntityUserByUserId(db, model.UserId.ToString(), false, true);
                        if (entityUser.UserName != model.UserName)
                        {
                            var isUserExists = db.UserProfiles.Any(e => e.UserName == model.UserName);
                            if (isUserExists)
                            {
                                ModelState.AddModelError("", "Email exists already!.");
                                return View(model);
                            }
                            entityUser.UserName = model.UserName;
                        }

                        if (!string.IsNullOrEmpty(model.IC))
                        {
                            model.IC = model.IC.Replace("-", "");
                        }

                        entityUser.LastUpdateDate = now;
                        entityUser.FullName = model.FullName;
                        entityUser.Address = model.Address;
                        entityUser.IC = model.IC;
                        entityUser.PhoneNumber = model.PhoneNumber;
                        db.SaveChanges();
                    }

                    string password = Request.Params.Get("Password");
                    if (!string.IsNullOrEmpty(password))
                    {
                        (var pass, var errorMessage) = PasswordModel.CheckPassword(password);
                        if (!pass)
                        {
                            ModelState.AddModelError("", errorMessage);
                            return View(model);
                        }
                        string token = WebSecurity.GeneratePasswordResetToken(model.UserName);
                        if (!WebSecurity.ResetPassword(token, password))
                        {
                            ModelState.AddModelError("", "Error happened while resetting password!.");
                            return View(model);
                        }
                    }

                    if (imageFile != null)
                    {
                        var photo = CreateOrUpdateImage(entityUser.UserId, imageFile.InputStream);
                    }
                    return RedirectToAction("Index");
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("", "Server Error!");
                }
            }
            return View(model);
        }

        public JsonResult Delete(string userId)
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityDoctor = AccountService.GetEntityUserByUserId(db, userId, false, true);
                entityDoctor.IsDelete = true;
                entityDoctor.UserName = "[deleted]" + entityDoctor.UserName + entityDoctor.UserId.ToString(); ;
                db.SaveChanges();
            }
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Ban(string userId)
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityUser = AccountService.GetEntityUserByUserId(db, userId, false, true);
                entityUser.IsBan = true;
                db.SaveChanges();
            }
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        public JsonResult UnBan(string userId)
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityUser = AccountService.GetEntityUserByUserId(db, userId, false, true);
                entityUser.IsBan = false;
                db.SaveChanges();
            }
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        private Entity.Photo CreateOrUpdateImage(int doctorId, Stream file)
        {
            Entity.Photo entityPhoto;
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                DateTime now = DateTime.UtcNow;
                Entity.UserProfile entityUser = AccountService.GetEntityUserByUserId(db, doctorId.ToString(), false, false);
                long oldPhotoId = 0;
                if (entityUser.PhotoId.HasValue)
                {
                    oldPhotoId = entityUser.PhotoId.Value;
                }

                string containerName = "u" + entityUser.UserId.ToString("D5");
                entityPhoto = PhotoHelper.UploadImageWithThumbnail(db, containerName, Guid.NewGuid().ToString(), file);
                entityUser.PhotoId = entityPhoto.PhotoId;

                entityUser.LastUpdateDate = DateTime.Now;

                db.SaveChanges();

                if (oldPhotoId > 0)
                {
                    PhotoHelper.DeletePhoto(db, oldPhotoId);
                }

            }
            return entityPhoto;
        }

        private Entity.Photo CreateOrUpdateImageForPharmacy(Entity.db_HeyDocEntities db, Entity.PrescriptionSource entitySource, Stream file)
        {
            long? oldPhotoId = entitySource.LogoPhotoId;

            string containerName = "pharmacy-logos";
            var entityPhoto = PhotoHelper.UploadImageWithThumbnail(db, containerName, $"ID{ entitySource.PrescriptionSourceId }-{ Guid.NewGuid() }", file);
            entitySource.LogoPhotoId = entityPhoto.PhotoId;

            db.SaveChanges();

            if (oldPhotoId.HasValue)
            {
                PhotoHelper.DeletePhoto(db, oldPhotoId.Value);
            }

            return entityPhoto;
        }

        [HttpGet]
        public JsonResult GetPharmacyList(int length, int start, int? draw, string searchKey = "")
        {
            int recordsTotal, recordsFiltered;
            var data = new List<PrescriptionSourceModel>();
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var doctorSourceIds = ConstantHelper.DoctorPrescriptionSourceIds;

                var entitySourceList = db.PrescriptionSources.Where(s => !s.IsDelete && !doctorSourceIds.Contains(s.PrescriptionSourceId));
                recordsTotal = entitySourceList.Count();
                if (!string.IsNullOrEmpty(searchKey))
                {
                    searchKey = searchKey.Trim().ToLower();
                    entitySourceList = entitySourceList.Where(s => s.PrescriptionSourceName.ToLower().Contains(searchKey));
                }

                recordsFiltered = entitySourceList.Count();
                entitySourceList = entitySourceList.OrderBy(s => s.PrescriptionSourceName).Skip(start).Take(length);

                foreach (var entitySource in entitySourceList)
                {
                    data.Add(new PrescriptionSourceModel(entitySource));
                }
            }

            return Json(new
            {
                draw, // Draw call number from DataTables, used to correctly order the AJAX responses on client-side
                recordsTotal,
                recordsFiltered,
                data,
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CreatePharmacy()
        {
            var model = new PharmacyRegisterModel();
            return View(model);
        }

        [HttpPost]
        public ActionResult CreatePharmacy(PharmacyRegisterModel model, HttpPostedFileBase imageFile)
        {
            if (imageFile == null)
            {
                ModelState.AddModelError("", "Logo image is required.");
            }
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            try
            {
                using (var db = new Entity.db_HeyDocEntities())
                {
                    using (var transaction = db.Database.BeginTransaction())
                    {
                        var entitySource = db.PrescriptionSources.Add(new Entity.PrescriptionSource
                        {
                            PrescriptionSourceName = model.Name
                        });
                        db.SaveChanges();

                        CreateOrUpdateImageForPharmacy(db, entitySource, imageFile.InputStream);
                        transaction.Commit();
                    }
                }
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Server Error!");
            }
            return View(model);
        }

        public ActionResult EditPharmacy(int sourceId)
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entitySource = db.PrescriptionSources.FirstOrDefault(s => s.PrescriptionSourceId == sourceId);
                if (entitySource == null)
                {
                    throw new Exception("Pharmacy not found.");
                }
                var model = new PrescriptionSourceModel(entitySource);
                return View(model);
            }
        }

        [HttpPost]
        public ActionResult EditPharmacy(PrescriptionSourceModel model, HttpPostedFileBase imageFile)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    Entity.PrescriptionSource entitySource;
                    using (var db = new Entity.db_HeyDocEntities())
                    {
                        using (var transaction = db.Database.BeginTransaction())
                        {
                            entitySource = db.PrescriptionSources.FirstOrDefault(s => s.PrescriptionSourceId == model.PrescriptionSourceId);
                            if (entitySource == null)
                            {
                                throw new Exception("Pharmacy not found.");
                            }

                            if (string.IsNullOrEmpty(model.PrescriptionSourceName))
                            {
                                throw new Exception("Pharmacy name cannot be empty.");
                            }

                            entitySource.PrescriptionSourceName = model.PrescriptionSourceName;
                            db.SaveChanges();

                            if (imageFile != null)
                            {
                                CreateOrUpdateImageForPharmacy(db, entitySource, imageFile.InputStream);
                            }
                            transaction.Commit();
                        }
                    }

                    return RedirectToAction("Index");
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("", "Server Error!");
                }
            }
            return View(model);
        }

        [HttpPost]
        public JsonResult DeletePharmacy(int sourceId)
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                using (var transaction = db.Database.BeginTransaction())
                {
                    var entitySource = db.PrescriptionSources.FirstOrDefault(s => s.PrescriptionSourceId == sourceId);
                    if (entitySource == null)
                    {
                        throw new Exception("Pharmacy not found.");
                    }

                    foreach (var entityOutlet in entitySource.UserProfiles)
                    {
                        entityOutlet.IsDelete = true;
                        entityOutlet.UserName = "[deleted]" + entityOutlet.UserName + entityOutlet.UserId.ToString();
                    }

                    entitySource.IsDelete = true;
                    db.SaveChanges();

                    transaction.Commit();
                }
            }
            return Json(true);
        }
    }
}
