using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HeyDoc.Web.Services
{
    public class RedeemService
    {
        public static List<SelectListItem> GetRedeemCategoryList()
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                return _GetRedeemCategoryList(db);
            }
        }

        public static List<SelectListItem> _GetRedeemCategoryList(Entity.db_HeyDocEntities db)
        {
            List<SelectListItem> categoryList = new List<SelectListItem>();
            var categories = db.Categories.Where(e => !e.IsDelete).OrderBy(e => e.Sequence).ThenBy(e => e.CategoryName).Select(e => new SelectListItem()
            {
                Text = e.CategoryName,
                Value = e.CategoryId.ToString(),
            });
            foreach (var category in categories)
            {
                categoryList.Add(category);
            }
            return categoryList;
        }
    }
}