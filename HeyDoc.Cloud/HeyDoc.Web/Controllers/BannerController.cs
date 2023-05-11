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
    public class BannerController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public JsonResult GetBannerList()
        {
            int take, skip, recordsTotal, recordsFiltered;
            List<BannerModel> data = new List<BannerModel>();
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                take = Convert.ToInt32(Request.Form["length"]);
                skip = Convert.ToInt32(Request.Form["start"]);
                int sortParam = Convert.ToInt32(Request.Form["order[0][column]"]);
                string sortOrder = Request.Form["order[0][dir]"];

                var entityBanners = db.Banners.OrderBy(e => e.Sequence).Skip(skip).Take(take);
                recordsFiltered = recordsTotal = db.Categories.Where(e => !e.IsDelete).Count();

               
                foreach (var entityBanner in entityBanners)
                {
                    data.Add(new BannerModel(entityBanner));
                }

            }

            return Json(new
            {
                recordsTotal = recordsTotal,
                recordsFiltered = recordsFiltered,
                data = data,
            });
        }

        public JsonResult Delete(int bannerId)
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityBanner = db.Banners.FirstOrDefault(e => e.BannerId == bannerId);
                if (entityBanner == null)
                {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.NotFound, "Banner not found"));
                }
                db.Banners.Remove(entityBanner);
                db.SaveChanges();
            }
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Edit(int id)
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityBanner = db.Banners.FirstOrDefault(e => e.BannerId == id);
                BannerModel result = null;
                if (entityBanner != null)
                {
                    result = new BannerModel(entityBanner);
                }
                return View("Add", result);
            }
        }

        public ActionResult Create()
        {
            return View("Add", new BannerModel());
        }

        public ActionResult AddOrEditBanner(BannerModel model, HttpPostedFileBase bannerImage)
        {
            if (!ModelState.IsValid)
            {
                return View("Add", model);
            }
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                Entity.Banner entityBanner = null;
                if (model.BannerId > 0)
                {
                    entityBanner = db.Banners.FirstOrDefault(e => e.BannerId == model.BannerId);
                    if (entityBanner == null)
                    {
                        ModelState.AddModelError("", "Banner not found");
                        return View("Add", model);
                    }
                }
                else
                {
                    entityBanner = db.Banners.Create();
                    db.Banners.Add(entityBanner);
                }
                entityBanner.Sequence = model.Sequence;
                entityBanner.LinkUrl = model.LinkUrl;
  
                if (bannerImage != null)
                {
                    CreateOrUpdateImage(db, entityBanner, bannerImage.InputStream);
                }
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        private bool CreateOrUpdateImage(Entity.db_HeyDocEntities db, Entity.Banner entityBanner, Stream fileStream)
        {
            string oldUrl = "";
            if (!string.IsNullOrEmpty(entityBanner.ImageUrl))
            {
                oldUrl = entityBanner.ImageUrl;
            }

            string containerName = "banner" + entityBanner.BannerId.ToString("D5");
            System.Drawing.Image originalImage = System.Drawing.Image.FromStream(fileStream);
            entityBanner.Height = originalImage.Height;// get the image height and width first before being disposed after upload
            entityBanner.Width = originalImage.Width;
            var originalBlobUrl = CloudBlob.UploadImage(containerName, Guid.NewGuid().ToString(), System.Drawing.Imaging.ImageFormat.Jpeg, originalImage);
            entityBanner.ImageUrl = originalBlobUrl;

            db.SaveChanges();

            if (!string.IsNullOrEmpty(oldUrl))
            {
                CloudBlob.DeleteImage(oldUrl);
            }
            return true;
        }
    }
}
