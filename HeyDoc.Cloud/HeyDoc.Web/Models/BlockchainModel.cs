using System;
using System.Collections.Generic;

namespace HeyDoc.Web.Models
{
    public class BlockchainUser
    {
        public string Email { get; set; }
        public string Name { get; set; }
        public string Gender { get; set; }
        public string Address { get; set; }
        public string IdentityCard { get; set; }
        public string PhoneNumber { get; set; }
        public string Birthday { get; set; }
    }

    public class BlockchainBioData
    {
        public double Weight { get; set; }
        public double Height { get; set; }
        public double Bmi { get; set; }
        public double BodyTemperature { get; set; }
        public string BloodPressure { get; set; }
        public double BloodGlucoseFasting { get; set; }
        public double BloodGlucose { get; set; }
        public double MenstrualPeriod { get; set; }
        public double MenstrualDuration { get; set; }
        public double HeartRate { get; set; }
        public string Allergy { get; set; }
        public DateTime LastEditedDate { get; set; }
    }

    public class BlockchainHealth
    {
        public BlockchainBioData Biodata { get; set; }
        public string OwnerId { get; set; }
        public string Owner { get; set; }
        public List<string> AuthorizedRead { get; set; }
        public List<string> AuthorizedWrite { get; set; }
    }

    public class BlockchainRegisterModel
    {
        public BlockchainUser User { get; set; }
        public BlockchainHealth Health { get; set; }
    }

}