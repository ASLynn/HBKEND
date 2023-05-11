using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HeyDoc.Web.Models
{
    public class ReportDoctorReviewModel
    {
        public string DoctorName { get; set; }
        public string PatientName { get; set; }
        public byte Rating { get; set; }
        public string Comment { get; set; }
        public DateTime ReviewTime { get; set; }
    }
}