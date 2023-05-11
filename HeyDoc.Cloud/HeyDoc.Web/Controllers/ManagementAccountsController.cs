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
using WebMatrix.WebData;

namespace HeyDoc.Web.Controllers
{
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class ManagementAccountsController : BaseController
    {
        // TODO M: Add roles as appropriate
        // Order of this list affects the selection dropdown on the page, so preferable to keep it alphabetical
        private readonly List<RoleType> ManagementRoles = new List<RoleType>
        {
            RoleType.Admin,
            RoleType.Doctor,
            RoleType.User,
            RoleType.SuperAdmin,
            RoleType.Pharmacy,
            RoleType.PharmacyManagement
        };

        private readonly Dictionary<RoleType, string> RoleToName = new Dictionary<RoleType, string>
        {
        };

        private IEnumerable<SelectListItem> _managementRoleSelectList;
        private IEnumerable<SelectListItem> ManagementRoleSelectList
        {
            get
            {
                if (_managementRoleSelectList == null)
                {
                }
                _managementRoleSelectList = ManagementRoles.Select(r => new SelectListItem { Text = r.GetDescription(), Value = ((int)r).ToString() });
                return _managementRoleSelectList;
            }
        }

        public ActionResult Index()
        {
            var roleList = new List<SelectListItem>
            {
                new SelectListItem { Text = "All", Value = "-1" }
            };
            roleList.AddRange(ManagementRoles.Select(r => new SelectListItem { Text = r.GetDescription(), Value = ((int)r).ToString() }));
            //RoleToName[r]
            //((int)r).ToString()
            ViewBag.ManagementRoles = roleList;
            var roleToNameMap = RoleToName.ToDictionary(kv => ((int)kv.Key).ToString(), kv => kv.Value);
            ViewBag.RoleToNameJson = new JavaScriptSerializer().Serialize(roleToNameMap);
            return View();
        }

        [HttpPost]
        public JsonResult GetList(int length, int start, List<DataTableOrderOptions> order, List<DataTableColumnProps> columns, int? draw, string searchKey = "", int roleFilter = -1)
        {
            int recordsTotal, recordsFiltered;
            var data = new List<UserModel>();
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityUserList = db.UserProfiles.Where(e => !e.IsDelete);
                if (roleFilter == -1)
                {
                    var allManagementRoles = new HashSet<string>(ManagementRoles.Select(r => r.ToString()));
                    entityUserList = entityUserList.Where(e => e.webpages_Roles.Any(r => allManagementRoles.Contains(r.RoleName)));
                }
                else
                {
                    var success = Enum.TryParse<RoleType>(roleFilter.ToString(), out var role);
                    if (success && ManagementRoles.Contains(role))
                    {
                        entityUserList = entityUserList.Where(e => e.webpages_Roles.Any(r => r.RoleName == role.ToString()));
                    }
                    else
                    {
                        return Json(new
                        {
                            error = $"Invalid role '{roleFilter}', expected -1 or one of {string.Join(", ", ManagementRoles.Select(r => (int)r))}"
                        });
                    }
                }
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

                entityUserList = entityUserList.Skip(start).Take(length);

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
            var model = new ManagementRegisterModel();
            ViewBag.ManagementRoles = ManagementRoleSelectList;

            return View(model);
        }

        [HttpPost]
        public ActionResult Create(ManagementRegisterModel model, HttpPostedFileBase imageFile)
        {
            (var pass, var errorMessage) = PasswordModel.CheckPassword(model.Password);
            if (!pass)
            {
                ModelState.AddModelError("Password", errorMessage);
            }
            if (!ModelState.IsValid)
            {
                ViewBag.ManagementRoles = ManagementRoleSelectList;
                return View(model);
            }
            try
            {
                var registerModel = new RegisterModel
                {
                    Email = model.Email,
                    Password = model.Password,
                    ConfirmPassword = model.ConfirmPassword,
                    FullName = model.FullName,
                    RoleType = model.RoleType,
                    Address = model.Address,
                    PhoneNumber = model.PhoneNumber,
                    CountryId = 74,
                    StateId = 1,
                    TownshipId = 1,
                    BloodType = "A",
                    createUserisAdmin = true
                };
                UserModel result = AccountService.CreateUser(registerModel, model.RoleType, false);
                if (result == null)
                {
                    ModelState.AddModelError("", "Email already exists!");
                    ViewBag.ManagementRoles = ManagementRoleSelectList;
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
            ViewBag.ManagementRoles = ManagementRoleSelectList;
            return View(model);
        }

        public ActionResult Edit(string userId)
        {
            UserModel model = new UserModel();
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityUser = AccountService.GetEntityUserByUserId(db, userId, false, true);
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

                        entityUser.LastUpdateDate = now;
                        entityUser.FullName = model.FullName;
                        entityUser.Address = model.Address;
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
    }
}
