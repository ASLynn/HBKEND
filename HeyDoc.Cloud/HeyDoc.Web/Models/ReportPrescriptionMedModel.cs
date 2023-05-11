using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HeyDoc.Web.Models
{
    public class ReportPrescriptionMedModel
    {
        public string PatientName { get; set; }
        public long PrescriptionId { get; set; }
        public string MedicationName { get; set; }
        public DateTime PrescriptionCreateTime { get; set; }
        public string MedicalSummary { get; set; }
    }
}