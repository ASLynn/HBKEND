using RestSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Web;

namespace HeyDoc.Web.WebApi
{
    public enum WebApiErrorCode
    {
        [Description("Unexpected error happened in server. Please try again later.")]
        [HttpStatusCode(HttpStatusCode.InternalServerError)]
        UnknownError = 1,

        [Description("Server Busy. Please try again later.")]
        [HttpStatusCode(HttpStatusCode.InternalServerError)]
        ServerBusy = 2,

        [Description("Missing required data")]
        [HttpStatusCode(HttpStatusCode.BadRequest)]
        BadRequest = 3,

        [Description("Invalid arguments")]
        [HttpStatusCode(HttpStatusCode.BadRequest)]
        InvalidArguments = 4,

        [Description("Requested item not found")]
        [HttpStatusCode(HttpStatusCode.NotFound)]
        NotFound = 5,

        [Description("Unauthorized action")]
        [HttpStatusCode(HttpStatusCode.Unauthorized)]
        Unauthorized = 6,

        [Description("Unsupported Media Type")]
        [HttpStatusCode(HttpStatusCode.UnsupportedMediaType)]
        UnsupportedMediaType = 7,

        [Description("Invalid action")]
        [HttpStatusCode(HttpStatusCode.Forbidden)]
        InvalidAction = 8,

        [Description("Duplicated item")]
        [HttpStatusCode(HttpStatusCode.Forbidden)]
        Duplicated = 9,

        [Description("Payment Error")]
        [HttpStatusCode(HttpStatusCode.Forbidden)]
        BrainTreeError = 10,

        [Description("Request to third party server returned an error")]
        [HttpStatusCode(HttpStatusCode.InternalServerError)]
        ThirdPartyRequestError = 11,

        // User management
        [Description("Access token not found")]
        [HttpStatusCode(HttpStatusCode.NotFound)]
        AccessTokenNotFound = 1000,

        [Description("Device is not registered")]
        [HttpStatusCode(HttpStatusCode.NotFound)]
        DeviceNotRegistered = 1001,

        [Description("User is not registered")]
        [HttpStatusCode(HttpStatusCode.NotFound)]
        UserNotRegistered = 1002,

        [Description("The user is banned")]
        [HttpStatusCode(HttpStatusCode.Unauthorized)]
        UserIsBanned = 1003,

        [Description("Duplicated email")]
        [HttpStatusCode(HttpStatusCode.Forbidden)]
        DuplicatedEmail = 1004,

        [Description("Unverified email")]
        [HttpStatusCode(HttpStatusCode.Forbidden)]
        UnverifiedEmail = 1005,
        [Description("Free Usage Limit")]
        [HttpStatusCode(HttpStatusCode.Forbidden)]
        FreeUsageLimit = 1006,

        [HttpStatusCode(HttpStatusCode.Forbidden)]
        InvalidPromoCode = 1007,

        [HttpStatusCode(HttpStatusCode.InternalServerError)]
        DigitalSiginingFailed = 1008,

        [Description("TPA user not exist")]
        [HttpStatusCode(HttpStatusCode.NotFound)]
        TPAUserNotExist = 2001,

        [Description("TPA user updated password")]
        [HttpStatusCode(HttpStatusCode.Unauthorized)]
        TPAUserUpdatedPassword = 2002,
    }

    [System.AttributeUsage(System.AttributeTargets.Field)]
    public class HttpStatusCodeAttribute : System.Attribute
    {
        public HttpStatusCode StatusCode { get; set; }

        public HttpStatusCodeAttribute(HttpStatusCode statusCode)
        {
            StatusCode = statusCode;
        }
    }
}