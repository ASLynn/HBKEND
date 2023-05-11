using HeyDoc.Web.Resources;
using HeyDoc.Web.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HeyDoc.Web.Models
{
    public class EmailLoginModel
    {
        public DeviceModel Device { get; set; }

        public string Email { get; set; }
        public string Password { get; set; }
        public string Phone { get; set; }
        public int CompanyId { get; set; } //Login from other application id, eg citizin pay
        public virtual void Validate()
        {
            Device.Validate();

            if (string.IsNullOrEmpty(Email))
            {
                throw new WebApiException(
                    new WebApiError(WebApiErrorCode.InvalidArguments, Account.ErrorEmailNull));
            }
            if (string.IsNullOrEmpty(Password))
            {
                throw new WebApiException(
                    new WebApiError(WebApiErrorCode.InvalidArguments, Account.ErrorPasswordNull));
            }
        }
    }

    public class TPALoginModel : EmailLoginModel
    {
        public string ICNumber { get; set; }
        public SourceType SourceType { get; set; }
    }

    public class ThirdPartyLoginModel
    {
        public string TPUserId { get; set; }
        public DeviceModel Device { get; set; }
    }
}