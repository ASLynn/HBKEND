using HeyDoc.Web.Lib;
using HeyDoc.Web.Models;
using HeyDoc.Web.Services;
using HeyDoc.Web.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.ValueProviders;
using System.Web.Http.ValueProviders.Providers;

namespace HeyDoc.Web.Controllers.Api
{
    public class DutyController : ApiController
    {
        //Temporary for admin 
        [HttpPost]
        public DutyModel Upload()
        {
            return WebApiWrapper.Call<DutyModel>(e => DutyService.UploadFile(Request.Content));
        }

        [HttpGet]
        public DutyModel Download([ValueProvider(new[] { typeof(HeaderValueProviderFactory), typeof(QueryStringValueProviderFactory) })] string accessToken)
        {
            return WebApiWrapper.Call<DutyModel>(e => DutyService.DownloadFile(accessToken));
        }
    }
}
