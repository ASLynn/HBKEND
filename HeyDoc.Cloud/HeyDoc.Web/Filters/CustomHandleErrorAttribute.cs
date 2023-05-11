using HeyDoc.Web.WebApi;
using log4net;
using System.Web;
using System.Web.Mvc;

namespace HeyDoc.Web.Filters
{
    public class CustomHandleErrorAttribute : HandleErrorAttribute
    {
        private readonly ILog logger;

        public CustomHandleErrorAttribute()
        {
            logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        }

        public override void OnException(ExceptionContext filterContext)
        {
            if (filterContext.ExceptionHandled)
            {
                return;
            }

            if (new HttpException(null, filterContext.Exception).GetHttpCode() != 500)
            {
                return;
            }

            if (!ExceptionType.IsInstanceOfType(filterContext.Exception))
            {
                return;
            }

            WebApiException wae = filterContext.Exception as WebApiException;
            bool a = filterContext.HttpContext.Request.IsAjaxRequest();
            // if the request is AJAX return JSON else view.
            if (filterContext.HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                string errorMessage;
                if (wae != null)
                {
                    errorMessage = wae.ErrorDetails.ErrorMessage;
                }
                else
                {
                    errorMessage = filterContext.Exception.Message;
                }
                filterContext.Result = new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { error = errorMessage }
                };
            }
            else
            {
                var controllerName = (string)filterContext.RouteData.Values["controller"];
                var actionName = (string)filterContext.RouteData.Values["action"];
                var model = new HandleErrorInfo(filterContext.Exception, controllerName, actionName);

                filterContext.Result = new ViewResult
                {
                    ViewName = View,
                    MasterName = Master,
                    ViewData = new ViewDataDictionary<HandleErrorInfo>(model),
                    TempData = filterContext.Controller.TempData
                };
            }

            // log the error using log4net.
            logger.Error(filterContext.Exception.Message, filterContext.Exception);

            filterContext.ExceptionHandled = true;
            filterContext.HttpContext.Response.Clear();
            filterContext.HttpContext.Response.StatusCode = 500;


            filterContext.HttpContext.Response.TrySkipIisCustomErrors = true;
        }
    }
}