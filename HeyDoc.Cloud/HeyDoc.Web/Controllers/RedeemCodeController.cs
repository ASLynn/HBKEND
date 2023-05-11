using HeyDoc.Web.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HeyDoc.Web.Controllers
{
    public class RedeemCodeController : BaseController
    {
        [Authorize(Roles = "Admin,SuperAdmin")]
        public ActionResult Index()
        {
            var rdmCat = TeamService.GetCategoryList();
            List<SelectListItem> categoryList = new List<SelectListItem>() { new SelectListItem { Text = "All", Value = "0" } };
            categoryList.AddRange(rdmCat);
            ViewBag.rdmCat = categoryList;
            return View();
        }
    }
}