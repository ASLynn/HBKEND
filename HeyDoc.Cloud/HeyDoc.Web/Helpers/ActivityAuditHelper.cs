using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Web;

namespace HeyDoc.Web.Helpers
{
    public class ActivityAuditHelper
    {
        private static readonly log4net.ILog log =
           log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly log4net.ILog auditLogger = log4net.LogManager.GetLogger("ApiAuditLogger");

        public static bool AddRequestDataToLog()
        {
            try
            {
                log4net.LogicalThreadContext.Properties["Audit_RequestPath"] = HttpContext.Current.Request.Path;
                log4net.LogicalThreadContext.Properties["Audit_UserIP"] = HttpContext.Current.Request.UserHostAddress;
                var currentRouteData = HttpContext.Current.Request.RequestContext.RouteData.Values;
                log4net.LogicalThreadContext.Properties["Audit_RequestController"] = currentRouteData["Controller"];
                log4net.LogicalThreadContext.Properties["Audit_RequestAction"] = currentRouteData["Action"];

                return true;
            }
            catch (Exception ex)
            {
                var msg = $"Could not get request info for activity audit log (Stack trace: {Environment.StackTrace})";
                if (ex is NullReferenceException)
                {
                    if (HttpContext.Current == null)
                    {
                        msg += " (HttpContext.Current null)";
                    }
                    else if (HttpContext.Current.Request == null)
                    {
                        msg += " (HttpContext.Current.Request null)";
                    }
                    else if (HttpContext.Current.Request.RequestContext == null)
                    {
                        msg += " (HttpContext.Current.Request.RequestContext null)";
                    }
                    else if (HttpContext.Current.Request.RequestContext.RouteData == null)
                    {
                        msg += " (HttpContext.Current.Request.RequestContext.RouteData null)";
                    }
                    else if (HttpContext.Current.Request.RequestContext.RouteData.Values == null)
                    {
                        msg += " (HttpContext.Current.Request.RequestContext.RouteData.Values null)";
                    }
                }
                log.Error(msg, ex);
                
                return false;
            }
        }

        public static void AddDeviceDataToLog(Entity.Device entityDevice)
        {
            log4net.LogicalThreadContext.Properties["Audit_UserDeviceId"] = entityDevice.DeviceId;
            log4net.LogicalThreadContext.Properties["Audit_UserDeviceType"] = entityDevice.DeviceType.ToString();
            log4net.LogicalThreadContext.Properties["Audit_UserAppVersion"] = entityDevice.GuiVersion;
            log4net.LogicalThreadContext.Properties["Audit_UserTokenType"] = entityDevice.TokenType.ToString();
        }

        public static void AddUserDataToLog(Entity.UserProfile entityUser)
        {
            log4net.LogicalThreadContext.Properties["Audit_UserId"] = entityUser.UserId.ToString();
            log4net.LogicalThreadContext.Properties["Audit_UserName"] = entityUser.UserName;
            log4net.LogicalThreadContext.Properties["Audit_UserFullName"] = entityUser.FullName;
            log4net.LogicalThreadContext.Properties["Audit_UserCreateSource"] = entityUser.CreatedSource.ToString();
            log4net.LogicalThreadContext.Properties["Audit_UserRoles"] = string.Join(",", entityUser.Roles);
        }

        public static void AddRequestParamsToLog()
        {
            var queryParams = HttpContext.Current.Request.QueryString;
            if (queryParams.Count > 0)
            {
                log4net.LogicalThreadContext.Properties["Audit_QueryParams"] = JsonConvert.SerializeObject(queryParams.AllKeys.Where(k => k != null && k != "accessToken").ToDictionary(k => k, k => queryParams[k]));
            }
            if (HttpContext.Current.Request.ContentType == "application/json")
            {
                var contentEncoding = HttpContext.Current.Request.ContentEncoding;
                var contentStream = HttpContext.Current.Request.InputStream;
                contentStream.Seek(0, SeekOrigin.Begin);
                using (var reader = new StreamReader(contentStream, contentEncoding))
                {
                    log4net.LogicalThreadContext.Properties["Audit_Body"] = reader.ReadToEnd();
                }
            }
        }

        public static void LogEvent(string message, ActivityAuditEvent eventType)
        {
            log4net.LogicalThreadContext.Properties["Audit_EventType"] = ((int)eventType).ToString();
            auditLogger.Info(message);
        }
    }
}