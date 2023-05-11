using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace HeyDoc.Web.WebApi
{
    public class WebApiException : HttpResponseException
    {
        public WebApiError ErrorDetails { get; set; }

        public WebApiException(WebApiError webApiError)
            : base(webApiError.httpResponseMessage)
        {
            ErrorDetails = webApiError;
        }
    }

    public class WebApiError
    {
        private static readonly log4net.ILog log =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [JsonIgnore]
        public Enum ErrorCodeEnum { get; set; }

        public int ErrorCode { get; set; }

        public string ErrorCodeDescription { get; set; }

        public string ErrorMessage { get; set; }

        [JsonIgnore]
        public string Key { get; set; }
        [JsonIgnore]
        public HttpResponseMessage httpResponseMessage { get; set; }
        [JsonIgnore]
        public string JsonString { get; set; }

        public WebApiError(Enum errorCode)
            : this(errorCode, errorCode.GetDescription(), errorCode.GetDescription(), errorCode.GetHttpStatusCode())
        { }

        public WebApiError(Enum errorCode, string errorMessage)
            : this(errorCode, errorCode.GetDescription(), errorMessage, errorCode.GetHttpStatusCode())
        { }

        public WebApiError(Enum errorCode, string errorMessage, string key)
            : this(errorCode, errorCode.GetDescription(), errorMessage, errorCode.GetHttpStatusCode())
        {
            Key = key;
        }

        public WebApiError(Enum errorCode, string errorCodeDescription, string errorMessage, HttpStatusCode httpStatusCode)
        {
            ErrorCode = Convert.ToInt32(errorCode);
            ErrorCodeEnum = errorCode;
            ErrorCodeDescription = errorCode.GetDescription();
            ErrorMessage = errorMessage;
            JsonString = this.ToJsonString();

            httpResponseMessage = new HttpResponseMessage(errorCode.GetHttpStatusCode())
            {
                Content = new StringContent(JsonString),
                ReasonPhrase = ErrorCodeDescription
            };
            httpResponseMessage.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
        }

        public WebApiError(Exception exception)
        {
            // All the callers of this constructor already log the exception and the InnerException is logged automatically along with the containing exception, so this is redundant
            //log.Error(exception);
            //if (exception.InnerException != null)
            //{
            //    log.Error(exception.InnerException);
            //}

            if (exception is System.Data.Entity.Core.EntityCommandExecutionException || exception is System.Data.Entity.Core.EntityException || exception is System.Data.SqlClient.SqlException)
            {
                ErrorCodeEnum = WebApiErrorCode.ServerBusy;
            }
            else
            {
                ErrorCodeEnum = WebApiErrorCode.UnknownError;
            }
            ErrorCode = Convert.ToInt32(ErrorCodeEnum);
            ErrorCodeDescription = ErrorCodeEnum.GetDescription();
            ErrorMessage = ErrorCodeEnum.GetDescription();
            //if (exception.InnerException != null)
            //{
            //    ErrorMessage += " *Inner Exception* : " + exception.InnerException.Message;
            //}

            JsonString = this.ToJsonString();

            httpResponseMessage = new HttpResponseMessage(ErrorCodeEnum.GetHttpStatusCode())
            {
                Content = new StringContent(JsonString),
                ReasonPhrase = ErrorCodeDescription
            };
            httpResponseMessage.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
        }

        public string ToJsonString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}