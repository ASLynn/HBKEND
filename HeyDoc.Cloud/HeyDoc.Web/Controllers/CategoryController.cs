using HeyDoc.Web.Helpers;
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
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace HeyDoc.Web.Controllers
{
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class CategoryController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }


        [HttpPost]
        public JsonResult GetCategoryList()
        {
            int take, skip, recordsTotal, recordsFiltered;
            List<CategoryModel> data = new List<CategoryModel>();
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                take = Convert.ToInt32(Request.Form["length"]);
                skip = Convert.ToInt32(Request.Form["start"]);
                int sortParam = Convert.ToInt32(Request.Form["order[0][column]"]);
                string sortOrder = Request.Form["order[0][dir]"];

                var entityCategories = db.Categories.Where(e => !e.IsDelete).OrderBy(e => e.Sequence).Skip(skip).Take(take);
                recordsFiltered = recordsTotal = db.Categories.Where(e => !e.IsDelete).Count();

                //switch (sortOrder)
                //{
                //    case "asc":
                //        switch (sortParam)
                //        {
                //            case 2:
                //                entityCategories = entityCategories.OrderBy(e => e.UserProfile.FullName);
                //                break;
                //            case 4:
                //                entityCategories = entityCategories.OrderBy(e => e.CashOutRequestStatus);
                //                break;
                //            case 5:
                //                entityCategories = entityCategories.OrderBy(e => e.CreateDate);
                //                break;

                //            case 6:
                //                entityCategories = entityCategories.OrderBy(e => e.CashOutDate);
                //                break;

                //            default:
                //                entityCategories = entityCategories.OrderBy(e => e.CreateDate);
                //                break;
                //        }
                //        break;
                //    case "desc":
                //        switch (sortParam)
                //        {
                //            case 2:
                //                entityCategories = entityCategories.OrderByDescending(e => e.UserProfile.FullName);
                //                break;
                //            case 4:
                //                entityCategories = entityCategories.OrderByDescending(e => e.CashOutRequestStatus);
                //                break;
                //            case 5:
                //                entityCategories = entityCategories.OrderByDescending(e => e.CreateDate);
                //                break;

                //            case 6:
                //                entityCategories = entityCategories.OrderByDescending(e => e.CashOutDate);
                //                break;

                //            default:
                //                entityCategories = entityCategories.OrderByDescending(e => e.CreateDate);
                //                break;
                //        }
                //        break;
                //}

                foreach (var entityCategory in entityCategories)
                {
                    data.Add(new CategoryModel(entityCategory));
                }
               
            }

            return Json(new
            {
                recordsTotal = recordsTotal,
                recordsFiltered = recordsFiltered,
                data = data,
            });
        }

        public JsonResult Delete(int categoryId)
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityCategory = db.Categories.FirstOrDefault(e => e.CategoryId == categoryId);
                if (entityCategory == null)
                {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.NotFound, "Category not found"));
                }
                entityCategory.IsDelete = true;
                db.SaveChanges();
            }
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Edit(int id)
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityCategory = db.Categories.FirstOrDefault(e => e.CategoryId == id);
                CategoryModel result = null;
                if (entityCategory != null)
                {
                    result = new CategoryModel(entityCategory);
                }
                return View("Add", result);
            }
        }

        public ActionResult Create()
        {
            return View("Add",  new CategoryModel());
        }

        public ActionResult AddOrEditCategory(CategoryModel model, HttpPostedFileBase categoryImage)
        {
            if (model.IsFreeChat && (model.CategoryPrice != 0 || model.MidnightPrice != 0))
            {
                ModelState.AddModelError("", "Unable to set a price for free chat. To proceed, unselect 'Free Chat' or set all prices to 0.");
            }
            if (!ModelState.IsValid)
            {
                return View("Add", model);
            }
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                Entity.Category entityCategory = null;
                if (model.CategoryId > 0)
                {
                    entityCategory = db.Categories.FirstOrDefault(e => e.CategoryId == model.CategoryId);
                    if (entityCategory == null)
                    {
                        ModelState.AddModelError("", "Category not found");
                        return View("Add", model);
                    }
                }
                else
                {
                    entityCategory = db.Categories.Create();
                    entityCategory.CreateDate = DateTime.UtcNow;
                    db.Categories.Add(entityCategory);
                }
                entityCategory.CategoryName = model.CategoryName;
                entityCategory.CategoryPrice = model.IsFreeChat ? 0 : model.CategoryPrice;
                entityCategory.IsDelete = false;
                entityCategory.MidNightPrice = model.IsFreeChat ? 0 : model.MidnightPrice;
                entityCategory.Sequence = model.Sequence;
                entityCategory.IsFreeChat = model.IsFreeChat;
                entityCategory.IsQueueMedPublicUser = model.IsQueueMedPublicUser;
                entityCategory.IsQueueMedCorporateUser = model.IsQueueMedCorporateUser;
                entityCategory.IsHiddenFromPublic = model.IsHiddenFromPublic;
                db.SaveChanges();
                if (categoryImage != null)
                {
                    CreateOrUpdateImage(db, entityCategory, categoryImage.InputStream);
                }

            }
            return RedirectToAction("Index");
        }

        private bool CreateOrUpdateImage(Entity.db_HeyDocEntities db, Entity.Category entityCategory, Stream fileStream)
        {
            string oldUrl = "";
            if (!string.IsNullOrEmpty(entityCategory.ImageUrl))
            {
                oldUrl = entityCategory.ImageUrl;
            }

            string containerName = "c" + entityCategory.CategoryId.ToString("D5");
            System.Drawing.Image originalImage = System.Drawing.Image.FromStream(fileStream);
            var originalBlobUrl = CloudBlob.UploadImage(containerName, Guid.NewGuid().ToString(), System.Drawing.Imaging.ImageFormat.Jpeg, originalImage);
            entityCategory.ImageUrl = originalBlobUrl;

            db.SaveChanges();

            if (!string.IsNullOrEmpty(oldUrl))
            {
                CloudBlob.DeleteImage(oldUrl);
            }
            return true;
        }
    }
}
