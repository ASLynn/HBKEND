using CsvHelper.Configuration.Attributes;
using System;

namespace HeyDoc.Web.Models
{
    public class UserPrescriptionStatsModel
    {
        [Index(0)]
        [Name("Full Name")]
        public string FullName { get; set; }
        [Index(1)]
        public string Email { get; set; }
        [Index(2)]
        [Name("Is Dependant")]
        public string IsDependant { get; set; }
        [Index(3)]
        [Name("User Type")]
        public string UserType { get; set; }
        [Index(4)]
        [Name("Employee Name")]
        public string EmployeeName { get; set; }
        [Index(5)]
        [Name("Phone Number")]
        public string PhoneNumber { get; set; }
        [Index(6)]
        [Name("Prescriptions")]
        public int PrescriptionCount { get; set; }
        [Index(7)]
        [Name("Dispensed Prescriptions")]
        public int DispensedPrescriptionCount { get; set; }
        [Index(8)]
        [Name("User Join Date")]
        public DateTime JoinDate { get; set; }
        [Index(9)]
        [Name("User Last Active Date")]
        public DateTime? LastActivityDate { get; set; }
    }
}