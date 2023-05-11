using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace HeyDoc.Web.Models
{
    public class CorporateSignUpOptions
    {
        public List<SignUpOption> SignUpPurpose { get; set; }
        public int? DefaultCorporateId { get; set; }
        public List<SignUpOption> Corporates { get; set; }
        public string ReferralName { get; set; }
        public string LogoUrl { get; set; }
        public string LogoDisplayName { get; set; }
        public string ReferralDisclaimer { get; set; }
    }

    public class SignUpOption
    {
        public string Value { get; set; }
        public string DisplayName { get; set; }
    }
}