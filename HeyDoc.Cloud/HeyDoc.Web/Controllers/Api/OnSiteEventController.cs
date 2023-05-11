using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using HeyDoc.Web.Models;
using HeyDoc.Web.Services;
using HeyDoc.Web.WebApi;

namespace HeyDoc.Web.Controllers.Api
{
    public class OnSiteEventController : ApiController
    {
        [HttpPost]
        public Task CheckInUser(string userMedicalId, string eventCode)
        {
            return WebApiWrapper.CallAsync(() => OnSiteEventService.CheckInUser(userMedicalId, eventCode));
        }

        [HttpGet]
        public UserModel GetUserDetails(string userMedicalId, string eventCode)
        {
            return WebApiWrapper.Call(_ => OnSiteEventService.GetUserDetails(userMedicalId, eventCode));
        }

        [HttpGet]
        public OnSiteEventUserAndCheckInsModel GetUserDetailsAndCheckIns(string userMedicalId, string eventCode)
        {
            return WebApiWrapper.Call(_ => OnSiteEventService.GetUserDetailsAndCheckIns(userMedicalId, eventCode));
        }
    }
}