using HeyDoc.Web.Entity;
using PusherServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using HttpPostAttribute = System.Web.Mvc.HttpPostAttribute;

namespace HeyDoc.Web.Controllers.Api
{
    public class ChattingController : ApiController
    {
        [HttpPost]
        public async Task<ActionResult> Message(MessageDAO message)
        {
            var options = new PusherOptions
            {
                Cluster = "ap1",
                Encrypted = true
            };

            var pusher = new Pusher(
              "1499953",
              "30fac672a20ea2672a75",
              "c0f6146203860333b165",
              options);

            //string message = HttpContext.Current.Request.Form["Message"] ?? "";

            var result = await pusher.TriggerAsync(
              "Chat",
              "Message",
              data: new
              {
                  userId = message.userId,
                  message = message.message
              });

            return new HttpStatusCodeResult((int)HttpStatusCode.OK);
        }

        private ActionResult Ok(string[] strings)
        {
            throw new NotImplementedException();
        }
    }
}