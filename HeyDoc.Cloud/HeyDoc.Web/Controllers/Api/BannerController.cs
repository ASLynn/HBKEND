using HeyDoc.Web.Models;
using HeyDoc.Web.Services;
using HeyDoc.Web.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebApi.OutputCache.V2;

namespace HeyDoc.Web.Controllers.Api
{
    public class BannerController : ApiController
    {
        [CacheOutput(ClientTimeSpan = 300, ServerTimeSpan = 300)]
        [HttpGet]
        public List<BannerModel> List(int take = 10, int skip = 0)
        {
            return WebApiWrapper.Call<List<BannerModel>>(e => BannerService.GetList());
        }
    }
}
