using HeyDoc.Web.Lib;
using HeyDoc.Web.Models;
using HeyDoc.Web.Services;
using HeyDoc.Web.WebApi;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.ValueProviders;
using System.Web.Http.ValueProviders.Providers;

namespace HeyDoc.Web.Controllers.Api
{
    public class AvixoController : ApiController
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        [HttpPost]
        public async Task NotifyMCReady()
        {
            var content = await Request.Content.ReadAsStringAsync();
            log.Debug($"Avixo MC ready called with request authorization header: {Request.Headers.Authorization}\ncontent: {content}");
            if (Request.Headers.Authorization == null || Request.Headers.Authorization.Scheme != "Basic")
            {
                var response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
                response.Headers.Add("WWW-Authenticate", "Basic");
                throw new HttpResponseException(response);
            }
            var credentials = Encoding.UTF8.GetString(Convert.FromBase64String(Request.Headers.Authorization.Parameter));
            var separatorIndex = credentials.IndexOf(':');
            var username = credentials.Substring(0, separatorIndex);
            var password = credentials.Substring(separatorIndex + 1);
            if (username != "avixo" || !AvixoService.CheckKey(password))
            {
                var response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
                response.Headers.Add("WWW-Authenticate", "Basic");
                throw new HttpResponseException(response);
            }
            var model = JsonConvert.DeserializeObject<AvixoMCNotifyModel>(content);
            WebApiWrapper.Call(() => AvixoService.NotifyMCReady(model));
        }

        [HttpPost]
        public async Task<StringResult> CreatePatientForChatroom([ValueProvider(new[] { typeof(HeaderValueProviderFactory), typeof(QueryStringValueProviderFactory) })] string accessToken, int chatRoomId)
        {
            return new StringResult()
            {
                Result = await WebApiWrapper.CallAsync(() => AvixoService.CreatePatientForChatroom(accessToken, chatRoomId))
            };
        }

        [HttpGet]
        public StringResult GetMcPdf(Guid mcGuid, [ValueProvider(new[] { typeof(HeaderValueProviderFactory), typeof(QueryStringValueProviderFactory) })] string accessToken = null)
        {
            return new StringResult()
            {
                Result = WebApiWrapper.Call(e => AvixoService.GetMcPdf(accessToken, mcGuid))
            };
        }
    }
}