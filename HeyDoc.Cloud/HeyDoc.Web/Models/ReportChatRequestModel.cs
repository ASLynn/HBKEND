using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HeyDoc.Web.Models
{
    public class ReportChatRequestModel
    {
        public string PatientName { get; set; }
        public string DoctorName { get; set; }
        public DateTime ChatRequestTime { get; set; }
        public DateTime? RequestRespondedTime { get; set; }
        public DateTime? SessionEndTime { get; set; }
        public string ResponseTime { get; set; }
        public string SessionDuration { get; set; }
        public string RequestResponse { get; set; }
    }
}