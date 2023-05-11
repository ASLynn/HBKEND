using HeyDoc.Web;
using HeyDoc.Web.Helpers;
//using Microsoft.WindowsAzure.ServiceRuntime;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using WebMatrix.WebData;
[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace HeyDoc.Web
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        private static readonly log4net.ILog log =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        protected void Application_Start()
        {
            if (System.Web.HttpContext.Current.IsDebuggingEnabled)
            {
            }
            else
            {
                log4net.Config.XmlConfigurator.Configure();
            }
               

            // Convert all dates to UTC
            var json = GlobalConfiguration.Configuration.Formatters.JsonFormatter;
            json.SerializerSettings.DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Utc;
            json.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;

            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            AuthConfig.RegisterAuth();
            Fissoft.EntityFramework.Fts.DbInterceptors.Init();
            Z.EntityFramework.Extensions.EntityFrameworkManager.IsCommunity = true;

            //GlobalConfiguration.Configuration.Formatters.Remove(GlobalConfiguration.Configuration.Formatters.XmlFormatter); 

            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

            DbDataInitialization();
            //#if !DEBUG
            //            if (RoleEnvironment.IsAvailable && RoleEnvironment.CurrentRoleInstance != null)
            //            {
            //                var splitData = RoleEnvironment.CurrentRoleInstance.Id.Split('.');
            //                if (splitData[splitData.Length - 1].Contains("Web_IN_0"))
            //                {
            //#endif
            //#if !DEBUG
            //                }
            //            }
            //#endif

            Z.EntityFramework.Extensions.EntityFrameworkManager.IsCommunity = true;
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            string[] xsfAllowedRoutes = {
                "/prescription/prescription",
            };

            if (!xsfAllowedRoutes.Contains(HttpContext.Current.Request.Url.AbsolutePath.ToLowerInvariant()))
            {
                HttpContext.Current.Response.AddHeader("X-Frame-Options", "SAMEORIGIN");
                HttpContext.Current.Response.AddHeader("Content-Security-Policy", $"frame-ancestors 'self' {ConstantHelper.WebUrl}");
            }
        }

        protected void Application_AcquireRequestState(object sender, EventArgs e)
        {
            var supportedLangs = new List<string>
            {
                "en",
                "zh-CN"
            };

            var preferredLangString = Request.QueryString.Get("preferredLang");
            var preferredLangs = preferredLangString?.Split(',') ?? Request.UserLanguages;

            var culture = "en";
            if (preferredLangs != null)
            {
                var matchingLangFound = false;
                // Search for exact matches first, in priority order given in header
                foreach (var lang in preferredLangs)
                {
                    var langIndex = supportedLangs.FindIndex(l => l == lang);
                    if (langIndex == -1) continue;

                    culture = supportedLangs[langIndex];
                    matchingLangFound = true;
                    break;
                }
                if (!matchingLangFound)
                {
                    // Search based on first segment if exact match wasn't found
                    foreach (var lang in preferredLangs.Select(l => l.Split('-')[0]))
                    {
                        var langIndex = supportedLangs.FindIndex(l => l.Split('-')[0] == lang);
                        if (langIndex == -1) continue;

                        culture = supportedLangs[langIndex];
                        break;
                    }
                }
            }

            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(culture);
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(culture);
        }

        private void DbDataInitialization()
        {
            LazyInitializer.EnsureInitialized(ref _initializer, ref _isInitialized, ref _initializerLock);

            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                if (db.webpages_Roles.Count() == 0)
                {
                    foreach (RoleType roleType in (RoleType[])Enum.GetValues(typeof(RoleType)))
                    {
                        var entityRole = new Entity.webpages_Roles();
                        entityRole.RoleName = roleType.ToString();

                        db.webpages_Roles.Add(entityRole);
                        db.SaveChanges();
                    }
                }
            }
        }

        private static SimpleMembershipInitializer _initializer;
        private static object _initializerLock = new object();
        private static bool _isInitialized;

        public class SimpleMembershipInitializer
        {
            public SimpleMembershipInitializer()
            {
                using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
                {
                    db.UserProfiles.Find(1);
                }
                
                if (!WebSecurity.Initialized)
                    WebSecurity.InitializeDatabaseConnection("DefaultConnection", "UserProfile", "UserId", "UserName", autoCreateTables: false, casingBehavior: SimpleMembershipProviderCasingBehavior.RelyOnDatabaseCollation);
            }
        }
    }
}