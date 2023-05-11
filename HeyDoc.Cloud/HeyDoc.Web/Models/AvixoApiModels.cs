using HeyDoc.Web.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HeyDoc.Web.Models
{
    public class AvixoLoginResponse
    {
        public string status { get; set; }
        public string token { get; set; }
    }

    public class AvixoPatientData
    {
        public string name { get; set; }
        [JsonConverter(typeof(CustomDateTimeConverter), "yyyy-MM-dd")]
        public DateTime? dob { get; set; }
        public string email { get; set; }
        public string gender { get; set; }
        public string nric { get; set; }
        public string address { get; set; }
        public string postal_code { get; set; }
        public string tel { get; set; }
        public string home_phone { get; set; }
        public string url { get; set; }
    }

    public class AvixoCreatePatientResponse
    {
        public string status { get; set; }
        public string msg { get; set; }
        public AvixoPatientData data { get; set; }
    }
}