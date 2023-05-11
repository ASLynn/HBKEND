using CsvHelper.Configuration.Attributes;

namespace HeyDoc.Web.Models
{
    public class DoctorPrescriptionStatsModel
    {
        [Index(0)]
        [Name("Full Name")]
        public string FullName { get; set; }
        [Index(1)]
        public string Email { get; set; }
        [Index(2)]
        [Name("Prescriptions")]
        public int PrescriptionCount { get; set; }
        [Index(3)]
        [Name("Dispensed")]
        public int DispensedCount { get; set; }
    }
}