using HeyDoc.Web.Resources;
using HeyDoc.Web.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HeyDoc.Web.Models
{
    public class DoctorRemarkModel
    {
        public string DoctorRemarks { get; set; }
        public PrescriptionAvailabilityStatus Status { get; set; }
        public DigitalSignatureModel Signature { get; set; }

        public void Validate()
        {
            if (Signature == null)
            {
                throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidArguments, Forms.ErrorDigSignInvalid));
            }
        }

        public DoctorRemarkModel()
        {

        }
    }
}