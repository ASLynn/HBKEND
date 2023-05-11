using HeyDoc.Web.Lib;
using HeyDoc.Web.Models;
using HeyDoc.Web.Services;
using HeyDoc.Web.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.ValueProviders;
using System.Web.Http.ValueProviders.Providers;

namespace HeyDoc.Web.Controllers.Api
{
    public class NotificationController : ApiController
    {

        [HttpGet]
        public List<NotificationModel> List([ValueProvider(new[] { typeof(HeaderValueProviderFactory), typeof(QueryStringValueProviderFactory) })] string accessToken, int skip, int take)
        {
            return WebApiWrapper.Call<List<NotificationModel>>(e => NotificationService.GetAllNotification(accessToken, skip, take));
        }

        [HttpGet]
        public BoolResult ReadAll([ValueProvider(new[] { typeof(HeaderValueProviderFactory), typeof(QueryStringValueProviderFactory) })] string accessToken)
        {
            return WebApiWrapper.Call<BoolResult>(e => NotificationService.ReadAll(accessToken));
        }

        [HttpGet]
        public BoolResult Read([ValueProvider(new[] { typeof(HeaderValueProviderFactory), typeof(QueryStringValueProviderFactory) })] string accessToken, long notificationId)
        {
            return WebApiWrapper.Call<BoolResult>(e => NotificationService.ReadNotification(accessToken, notificationId));
        }
    }
}
