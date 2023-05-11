using HeyDoc.Web.Resources;
using HeyDoc.Web.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HeyDoc.Web.Models
{
    public class OAuthLoginModel
    {
        public DeviceModel Device { get; set; }

        public OAuthType OAuthType { get; set; }
        public string OAuthId { get; set; }
        public string OAuthToken { get; set; }
        public void Validate()
        {
            Device.Validate();

            if (!Enum.IsDefined(typeof(OAuthType), OAuthType))
            {
                throw new WebApiException(
                    new WebApiError(WebApiErrorCode.InvalidArguments, "Invalid OAuth Type: " + OAuthType));
            }
            if (string.IsNullOrEmpty(OAuthId))
            {
                throw new WebApiException(
                    new WebApiError(WebApiErrorCode.InvalidArguments, Errors.ErrorOAuthIDNull));
            }
            if (string.IsNullOrEmpty(OAuthToken))
            {
                throw new WebApiException(
                    new WebApiError(WebApiErrorCode.InvalidArguments, Errors.ErrorOAuthTokenNull));
            }
        }
    }

    public class OAuthData
    {
        private string email;
        public string Email
        {
            get { return email; }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.BadRequest, "Please grant the permission to get Email while login with Facebook"));
                }
                else
                {
                    email = value;
                }
            }
        }

        private string name;
        public string Name
        {
            get { return name; }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    name = value.Truncate(95);
                }
            }
        }
        private DateTime dateofbirth;
        public DateTime Dateofbirth
        {
            get { return dateofbirth; }
            set
            {


                dateofbirth = value;

            }
        }

        public string ImageUrl { get; set; }


    }
}