using HeyDoc.Web.Lib;
using HeyDoc.Web.Models;
using HeyDoc.Web.Services;
using HeyDoc.Web.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.ValueProviders;
using System.Web.Http.ValueProviders.Providers;

namespace HeyDoc.Web.Controllers.Api
{
    public class QueueMedController : ApiController
    {
        [HttpPost]
        public Task<BoolResult> SendNotificationAppointmentDoctor(QueuesMedNotificationModel model, string P1, string P2)
        {
            return WebApiWrapper.CallAsync(() => QueueMedService.SendNotificationAppointmentDoctor(model, P1, P2));
        }

        [HttpPost]
        public Task<BoolResult> SendNotificationUserAppointmentStatusUpdate(QueuesMedNotificationModel model , string P1, string P2)
        {
            return WebApiWrapper.CallAsync(() => QueueMedService.SendNotificationUserAppointmentStatusUpdate(model, P1, P2));
        }
        
        [HttpGet]
        public GenericDataModel<CategoryModel> QueueMedReadyCategories([ValueProvider(new[] { typeof(HeaderValueProviderFactory), typeof(QueryStringValueProviderFactory) })] string accessToken, int take, int skip)
        {
            return WebApiWrapper.Call<GenericDataModel<CategoryModel>>(e => QueueMedService.QueueMedReadyCategories(accessToken, take, skip));
        }

    }
}