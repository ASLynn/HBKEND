using CsvHelper.Configuration.Attributes;

namespace HeyDoc.Web.Models
{
    public class EpsPrescriptionStatsModel
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
        [Name("Approved")]
        public int ApprovedCount { get; set; }
        [Index(4)]
        [Name("Rejected")]
        public int RejectedCount { get; set; }
        [Index(5)]
        [Name("Dispensed (Pharmacist created)")]
        public int DispensedPharmacistCount { get; set; }
        [Index(6)]
        [Name("Dispensed (Doctor created)")]
        public int DispensedDoctorCount { get; set; }
    }
}