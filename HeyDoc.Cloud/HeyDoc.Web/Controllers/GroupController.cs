using HeyDoc.Web.Lib;
using HeyDoc.Web.Models;
using HeyDoc.Web.Resources;
using HeyDoc.Web.Services;
using HeyDoc.Web.WebApi;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;

namespace HeyDoc.Web.Controllers
{
    [Authorize(Roles="Admin,SuperAdmin")]
    public class GroupController : BaseController
    {
        public ActionResult Index()
        {
            ViewBag.Categories = TeamService.GetCategoryList();
            return View();
        }

        [HttpPost]
        public JsonResult GetList(int category)
        {
            int take, skip, recordsTotal, recordsFiltered;
            List<GroupModel> data = new List<GroupModel>();
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                take = Convert.ToInt32(Request.Form["length"]);
                skip = Convert.ToInt32(Request.Form["start"]);

                var entityGroupList = db.Groups.Where(e => !e.IsDeleted && e.CategoryId==category);

                recordsTotal = entityGroupList.Count();
                recordsFiltered = recordsTotal;

                entityGroupList = entityGroupList.OrderBy(e => e.GroupName).Skip(skip).Take(take);

                foreach (var entityGroup in entityGroupList)
                {
                    data.Add(new GroupModel(entityGroup, true));
                }
            }

            return Json(new
            {
                recordsTotal = recordsTotal,
                recordsFiltered = recordsFiltered,
                data = data,
            });
        }

        public ActionResult Create()
        {
            var model = new GroupEditModel();
            ViewBag.Categories = TeamService.GetCategoryList();
            return View(model);
        }

        [HttpPost]
        public ActionResult Create(GroupEditModel model, HttpPostedFileBase imageFile, HttpPostedFileBase tpImageFile)
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                try
                {
                    var entityGroup = db.Groups.Create();
                    entityGroup.GroupName = model.GroupName;
                    entityGroup.GenericGroupName = model.GenericGroupName;
                    entityGroup.PlatformCut = model.PlatformCut;
                    entityGroup.CategoryId = model.CategoryId;
                    entityGroup.CreateDate = DateTime.UtcNow;
                    entityGroup.IsDeleted = false;
                    db.Groups.Add(entityGroup);

                    if (imageFile != null)
                    {
                        var oldPhoto = entityGroup.Photo;
                        var entityPhoto = PhotoHelper.UploadImageWithThumbnail(db, "group", entityGroup.GroupId.ToString() + "/" + Guid.NewGuid().ToString(), imageFile.InputStream);
                        entityGroup.PhotoId = entityPhoto.PhotoId;
                        if (oldPhoto != null)
                        {
                            PhotoHelper.DeletePhoto(db, oldPhoto);
                        }
                    }
                    if (tpImageFile != null)
                    {
                        var oldPhoto = entityGroup.TpPhoto;
                        var entityPhoto = PhotoHelper.UploadImageWithThumbnail(db, "group", entityGroup.GroupId.ToString() + "/" + Guid.NewGuid().ToString(), tpImageFile.InputStream);
                        entityGroup.TpPhotoId = entityPhoto.PhotoId;
                        if (oldPhoto != null)
                        {
                            PhotoHelper.DeletePhoto(db, oldPhoto);
                        }
                    }
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch
                {
                    ModelState.AddModelError("", "Server error. Please try again");
                }
            }
            ViewBag.Categories = TeamService.GetCategoryList();
            return View(model);
        }

        public ActionResult Edit(long groupId)
        {
            using (var db = new Entity.db_HeyDocEntities())
            {
                ViewBag.Categories = TeamService.GetCategoryList();
                var entityGroup = db.Groups
                    .Include(e => e.Photo)
                    .Include(e => e.TpPhoto)
                    .Include(e => e.Category)
                    .FirstOrDefault(e => e.GroupId == groupId);
                return View(new GroupEditModel(entityGroup));
            }
        }

        [HttpPost]
        public ActionResult Edit(GroupEditModel model, HttpPostedFileBase imageFile, HttpPostedFileBase tpImageFile)
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                try
                {
                    var entityGroup = db.Groups.FirstOrDefault(e => e.GroupId == model.GroupId);
                    if (entityGroup != null)
                    {
                        entityGroup.GroupName = model.GroupName;
                        entityGroup.GenericGroupName = model.GenericGroupName;
                        entityGroup.PlatformCut = model.PlatformCut;
                        entityGroup.CategoryId = model.CategoryId;
                        if (imageFile != null)
                        {
                            var oldPhoto = entityGroup.Photo;
                            var entityPhoto = PhotoHelper.UploadImageWithThumbnail(db, "group", entityGroup.GroupId.ToString() + "/" + Guid.NewGuid().ToString(), imageFile.InputStream);
                            entityGroup.PhotoId = entityPhoto.PhotoId;
                            if (oldPhoto != null)
                            {
                                PhotoHelper.DeletePhoto(db, oldPhoto);
                            }
                        }
                        if (tpImageFile != null)
                        {
                            var oldPhoto = entityGroup.TpPhoto;
                            var entityPhoto = PhotoHelper.UploadImageWithThumbnail(db, "group", entityGroup.GroupId.ToString() + "/" + Guid.NewGuid().ToString(), tpImageFile.InputStream);
                            entityGroup.TpPhotoId = entityPhoto.PhotoId;
                            if (oldPhoto != null)
                            {
                                PhotoHelper.DeletePhoto(db, oldPhoto);
                            }
                        }
                        db.SaveChanges();
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Could not find group to edit");
                    }
                }
                catch
                {
                    ModelState.AddModelError("", "Server error. Please try again");
                }
            }
            ViewBag.Categories = TeamService.GetCategoryList();
            return View(model);
        }
      
        [HttpPost]
        public JsonResult Delete(long groupId)
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityGroup = db.Groups.FirstOrDefault(e => e.GroupId == groupId);
                if (entityGroup == null)
                {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.NotFound, Medications.ErrorGroupNotFound));
                }
                entityGroup.IsDeleted = true;
                var entityDoctors = db.Doctors.Where(e => e.GroupId == entityGroup.GroupId);
                foreach (var doctor in entityDoctors)
                {
                    doctor.GroupId = null;
                }
                db.SaveChanges();
            }
            return Json(true, JsonRequestBehavior.AllowGet);
        }
        [AllowAnonymous]
        public JsonResult GetGroupDDL(int categoryId)
        {
            List<GroupModel> result = new List<GroupModel>();
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityGroups = db.Groups.Where(e => e.CategoryId == categoryId && !e.IsDeleted);
               
                foreach (var entityGroup in entityGroups)
                {
                    result.Add(new GroupModel(entityGroup));
                }
                if (categoryId != 0)
                {
                    result.Add(new GroupModel() { GroupId = 0, GroupName = "Independent" });
                }
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

    }
}
