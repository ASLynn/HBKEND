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
    public class DeviceController : ApiController
    {
        [HttpPost]
        public DeviceModel Register(DeviceModel model)
        {
            return WebApiWrapper.Call<DeviceModel>(e => DeviceService.RegisterDevice(model));
        }
    }
}
