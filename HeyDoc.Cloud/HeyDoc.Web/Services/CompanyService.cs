using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HeyDoc.Web.Services
{
    public class CompanyService
    {
        internal static IEnumerable<SelectListItem> GetCompaniesSelectList()
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                List<SelectListItem> clist = new List<SelectListItem>();
                var companies = db.CompanyWhiteLabels.Where(e => e.CompanyType == "c").Select(e => new SelectListItem()
                {
                    Text = e.CompanyName,
                    Value = e.CompanyId.ToString(),
                });
                foreach (var com in companies)
                {
                    clist.Add(com);
                }
                return clist;
            }
        }
    }
}