
using HeyDoc.Web.Services;
using System;
using System.Web.Mvc;

namespace HeyDoc.Web.Controllers
{
    public class AuditLogController : Controller
    {
        [Authorize(Roles = "SuperAdmin")]
        public ActionResult List()
        {
            return View();
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpPost]
        public JsonResult AuditLogList(string searchTerm = "")
        {
            int take, skip, recordsTotal, recordsFiltered;
            take = Convert.ToInt32(Request.Form["length"]);
            skip = Convert.ToInt32(Request.Form["start"]);
            int sortParam = Convert.ToInt32(Request.Form["order[0][column]"]);
            string sortOrder = Request.Form["order[0][dir]"];
            var data = AuditLogService.GetAuditLogList(skip, take, sortParam, sortOrder, searchTerm, out recordsFiltered, out recordsTotal);
            return Json(new
            {
                recordsTotal = recordsTotal,
                recordsFiltered = recordsFiltered,
                data = data
            });
        }
    }
}
