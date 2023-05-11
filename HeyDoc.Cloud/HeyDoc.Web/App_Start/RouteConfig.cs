using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace HeyDoc.Web
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
               name: "HCPSignUp",
               url: "SignUp",
               defaults: new { controller = "Doctor", action = "SignUp", id = UrlParameter.Optional }
           );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Account", action = "Login", id = UrlParameter.Optional }
            );

            routes.MapRoute(
               name: "OTP",
               url: "{controller}/{action}/{phonenumber}/{otpcode}",
               defaults: new { controller = "Account", action = "Otpverify", phonenumber = UrlParameter.Optional , otpcode = UrlParameter.Optional }
           );
        }
    }
}