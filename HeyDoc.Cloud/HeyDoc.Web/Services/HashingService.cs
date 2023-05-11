using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Web;
using System.Security.Cryptography;
using System.Text;
using System.IO;

namespace HeyDoc.Web.Services
{
    public class HashingService
    {
        public class HMAC_SHA1
        {
            public static string ToHMACSHA1(string str, byte[] Secretkey)
            {
                HMACSHA1 hMACSHA1 = new HMACSHA1(Secretkey);
                byte[] byteStr = Encoding.ASCII.GetBytes(str);
                MemoryStream stream = new MemoryStream(byteStr);
                return hMACSHA1.ComputeHash(stream).Aggregate("", (s, e) => s + String.Format("{0:x2}", e), s => s);
            }
        }
    }
}