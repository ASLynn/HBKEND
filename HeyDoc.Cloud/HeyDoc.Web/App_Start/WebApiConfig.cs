using Microsoft.Owin.Security.OAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Cors;

namespace HeyDoc.Web
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            //if (System.Web.HttpContext.Current.IsDebuggingEnabled)
            //{
            //    var cors = new EnableCorsAttribute("http://localhost:4200", "*", "*");
            //    config.EnableCors(cors);
            //} //ok this is correct

            // Web API Configuration and Services
            config.SuppressDefaultHostAuthentication();
            config.Filters.Add(new HostAuthenticationFilter(OAuthDefaults.AuthenticationType));

            // Web API Routes
            //config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{action}/{id}",
                defaults: new { action = RouteParameter.Optional, id = RouteParameter.Optional }
            );

            GlobalConfiguration.Configuration.Formatters.Add(new MultipartMediaTypeFormatter());
            // Uncomment the following line of code to enable query support for actions with an IQueryable or IQueryable<T> return type.
            // To avoid processing unexpected or malicious queries, use the validation settings on QueryableAttribute to validate incoming queries.
            // For more information, visit http://go.microsoft.com/fwlink/?LinkId=279712.
            //config.EnableQuerySupport();
        }
    }
}