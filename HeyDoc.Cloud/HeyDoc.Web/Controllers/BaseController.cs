using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HeyDoc.Web.Controllers
{
    public class BaseController : Controller
    {
        public DateTime? FormatDateToLocal(DateTime? utc)
        {
            if (utc != null)
            {
                var gmtHours = Int32.Parse(Request.Cookies["offset"].Value);
                return utc.Value.AddHours(-gmtHours);
            }
            else
            {
                return null;
            }
        }

        public DateTime? FormatDateToUtc(DateTime? local)
        {
            if (local != null)
            {
                var gmtHours = Int32.Parse(Request.Cookies["offset"].Value);
                return local.Value.AddHours(gmtHours);
            }
            else
            {
                return null;
            }
        }

        public DateTime ConvertDateToLocal(DateTime utc)
        {
            var gmtHours = Int32.Parse(Request.Cookies["offset"].Value);
            return utc.AddHours(-gmtHours);
        }

        public DateTime ConvertDateToUtc(DateTime local)
        {
            var gmtHours = Int32.Parse(Request.Cookies["offset"].Value);
            return local.AddHours(gmtHours);
        }
    }
}
