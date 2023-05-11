using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using HeyDoc.Web.Entity;
using HeyDoc.Web.Services;
using System.Text;
using System.Security.Cryptography;

namespace HeyDoc.Web.Controllers
{
    public class HelperController : Controller
    {
        private db_HeyDocEntities db = new db_HeyDocEntities();

        [HttpGet]
        public ActionResult GetTownshipByStateId(int StateId)
        {
           var res =  db.Townships.Where(x => x.StateId == StateId).OrderBy(x=>x.TownshipDesc).ToList();
            return Json(res,JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult CertiUpload(HttpPostedFileBase certiFile)
        {

            return Json("000",JsonRequestBehavior.AllowGet);
        }

        public static string GenerateOTPKey(int maxSize)
        {
            char[] chars = new char[62];
            chars =
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
            byte[] data = new byte[1];
            using (RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider())
            {
                crypto.GetNonZeroBytes(data);
                data = new byte[maxSize];
                crypto.GetNonZeroBytes(data);
            }
            StringBuilder result = new StringBuilder(maxSize);
            foreach (byte b in data)
            {
                result.Append(chars[b % (chars.Length)]);
            }
            return result.ToString();
        }
    }
}
