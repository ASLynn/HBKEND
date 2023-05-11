using HeyDoc.Web.Entity;
using Microsoft.WindowsAzure.Storage.Blob.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HeyDoc.Web.Models
{
    public class PublicReferralSignUpResources
    {
        public string ReferralName { get; set; }
        public string LogoUrl { get; set; }
        public string LogoDisplayName { get; set; }
        public string ReferralDisclaimer { get; set; }

        public PublicReferralSignUpResources(UserReferralCode entityReferral)
        {
            ReferralName = entityReferral.ReferrerName;
            LogoUrl = entityReferral.LogoUrl;
            LogoDisplayName = entityReferral.LogoDisplayName;
            ReferralDisclaimer = entityReferral.ReferralDisclaimerText;
        }
    }
}