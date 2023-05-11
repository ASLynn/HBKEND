using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Http;
using System.Configuration;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;

namespace HeyDoc.Web.Helpers
{
    public static class SMSHelper
    {
        public class SMSReqModel
        {
            public string from { get; set; }

            public string to { get; set; }

            public string text { get; set; }
        }
        public  class SMSResModel
        {
            public int status { get; set; }
            public long message_id { get; set; }

            public string to { get; set; }

            public int message_count { get; set; }

            public string client_ref { get; set; }
        }
        public static SMSResModel SMSSend(SMSReqModel sMSReqModel)
        {
            SMSResModel sMSResModel = new SMSResModel();
            HttpClient _client = new HttpClient();
            _client = new HttpClient
            {
                BaseAddress = new Uri(System.Configuration.ConfigurationManager.AppSettings["smsEndpoint"].ToString()),
                Timeout = new TimeSpan(0, 0, 0, 0, -1)
            };

            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(
                                 new MediaTypeWithQualityHeaderValue("application/json"));
            _client.DefaultRequestHeaders.Add("Authorization", System.Configuration.ConfigurationManager.AppSettings["smsKey"].ToString());
            var content = new StringContent(JsonConvert.SerializeObject(sMSReqModel), Encoding.UTF8, "application/json");

            var response = _client.PostAsync(System.Configuration.ConfigurationManager.AppSettings["smsEndpoint"].ToString(), content).Result;

            if (response.IsSuccessStatusCode)
            {
                //var res = response.Content.ReadAsAsync<IEnumerable<SMSResModel>>().Result;  //Make sure to add a reference to System.Net.Http.Formatting.dll
                //foreach(var a in res)
                //{
                //    sMSResModel.status = a.status;
                //    sMSResModel.message_id = a.message_id;
                //    sMSResModel.message_count = a.message_count;
                //    sMSResModel.to = a.to;
                //    sMSResModel.client_ref = a.client_ref;
                //}
                sMSResModel.status = 200;
            }
            else
            {
                sMSResModel.status = 0;
            }
            _client.Dispose();
            return sMSResModel;
        }
      
    }
}