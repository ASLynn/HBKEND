using HeyDoc.Web.Models;
using HeyDoc.Web.Services;
using HeyDoc.Web.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace HeyDoc.Web.Controllers.Api
{
    public class SettingController : ApiController
    {
        [HttpGet]
        public List<SettingModel> List()
        {
            return WebApiWrapper.Call<List<SettingModel>>(e => SettingService.GetList());
        }

        [HttpGet]
        public SettingModel IsApnsDev(bool value)
        {
            return WebApiWrapper.Call<SettingModel>(e => SettingService.UpdateSetting(SettingService.IS_APNS_DEV, value));
        }
    }
}
