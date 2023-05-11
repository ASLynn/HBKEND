using HeyDoc.Web.Helpers;
using HeyDoc.Web.Lib;
using HeyDoc.Web.Models;
using HeyDoc.Web.Services;
using HeyDoc.Web.WebApi;
using Newtonsoft.Json;
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
    public class SettingsController : Controller
    {
        public ActionResult Index()
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityCutPercent = db.PlatformPercents.OrderByDescending(e => e.CreateDate).FirstOrDefault();
                var entityUrl = db.PromotionUrls.OrderByDescending(e => e.CreateDate).FirstOrDefault();

                ViewBag.CutPercent = entityCutPercent != null ? entityCutPercent.CutPercent : 0;
                ViewBag.PromotionUrl = entityUrl != null ? entityUrl.Url : "";
            }
            return View();
        }

        public JsonResult SavePercent(decimal percent)
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityPercent = db.PlatformPercents.Create();
                entityPercent.CreateDate = DateTime.UtcNow;
                entityPercent.CutPercent = percent;
                db.PlatformPercents.Add(entityPercent);
                db.SaveChanges();
            }
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SaveUrl(string url)
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityUrl = db.PromotionUrls.Create();
                entityUrl.CreateDate = DateTime.UtcNow;
                entityUrl.Url = url;
                db.PromotionUrls.Add(entityUrl);
                db.SaveChanges();
            }
            return Json(true, JsonRequestBehavior.AllowGet);
        }
    }
}
