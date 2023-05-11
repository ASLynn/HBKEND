using HeyDoc.Web.Helpers;
using HeyDoc.Web.Lib;
using HeyDoc.Web.Models;
using HeyDoc.Web.Resources;
using HeyDoc.Web.WebApi;
using Microsoft.Azure;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace HeyDoc.Web.Services
{
    public class AvixoService
    {
        private static readonly string SecretKey = CloudConfigurationManager.GetSetting("AvixoSecretKey", false);
        private static readonly string LoginUsername = CloudConfigurationManager.GetSetting("AvixoLoginUsername", false);
        private static readonly string LoginPassword = CloudConfigurationManager.GetSetting("AvixoLoginPassword", false);
        private static readonly string ApiUrl = CloudConfigurationManager.GetSetting("AvixoApiUrl", false);
        private static string BearerToken = null;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private const string McBlobContainer = "avixo-mc";

        public static void NotifyMCReady(AvixoMCNotifyModel model)
        {
            using (var db = new Entity.db_HeyDocEntities())
            {
                var entityUser = db.UserProfiles.FirstOrDefault(e => !e.IsDelete && !e.IsBan && e.IC == model.PatientIC && e.FullName == model.PatientName);
                if (entityUser == null)
                {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.BadRequest, HCPPatient.ErrorPatientNotFound));
                }
                var mcGuid = Guid.NewGuid();
                var mcFilename = new Uri(model.PDF).Segments.Last();
                var request = WebRequest.CreateHttp(model.PDF);
                var responseStream = request.GetResponse().GetResponseStream();
                var blobUrl = CloudBlob.UploadFile(McBlobContainer, $"patient-{entityUser.UserId}/{mcGuid}/{mcFilename}", responseStream, false);
                var entityMc = db.AvixoMedicalCertificates.Create();
                entityMc.Id = mcGuid;
                entityMc.UserId = entityUser.UserId;
                entityMc.AvixoUrl = model.PDF;
                entityMc.BlobUrl = blobUrl;
                entityMc.CreateDate = DateTime.UtcNow;
                db.AvixoMedicalCertificates.Add(entityMc);
                db.SaveChanges();
                var emailBody = string.Format(EmailHelper.ConvertEmailHtmlToString(@"Emails/AvixoMCReady.html"), entityUser.FullName, $"{ConstantHelper.WebUrl}/medical-certificate/{mcGuid}");
                var recepientEmail = entityUser.ContactEmail ?? entityUser.UserName;
#if DEBUG
                var emailSubject = "HOPE Staging: Your Electronic Medical Certificate is Ready";
                var bccList = CloudConfigurationManager.GetSetting("Doc2UsDevEmails").Split(',').ToList();
#else
                var emailSubject = "Doc2Us: Your Electronic Medical Certificate is Ready";
                // TODO M UNBLANK: Admin email as recepient
                var bccList = new List<string>
                {
                    ""
                };
#endif
                EmailHelper.SendViaSendGrid(new List<string> { recepientEmail }, emailSubject, emailBody, null, bccEmails: bccList);
            }
        }

        public static bool CheckKey(string key)
        {
            return key == SecretKey;
        }

        // Returns the URL to the page for the created patient on Avixo
        public static async Task<string> CreatePatientForChatroom(string accessToken, int chatRoomId)
        {
            using (var db = new Entity.db_HeyDocEntities())
            {
                var entityUser = AccountService.GetEntityUserByAccessToken(db, accessToken);
                if (!entityUser.Roles.Any(r => ConstantHelper.DoctorRoles.Contains(r)))
                {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.Unauthorized, string.Format(Errors.UnauthorizedRole, ConstantHelper.Doc2UsEmailContact)));
                }
                var entityChatroom = ChatService.GetChatRoom(accessToken, chatRoomId);
                var entityPatient = entityChatroom.Patient;
                if (!entityPatient.Birthday.HasValue)
                {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.BadRequest, HCPPatient.ErrorPatientDOBNull));
                }
                var request = new RestRequest("patient/create", Method.POST);
                var patientData = new AvixoPatientData()
                {
                    name = entityPatient.FullName,
                    dob = entityPatient.Birthday,
                    email = entityPatient.UserName,
                    gender = entityPatient.Gender == Gender.Male ? "Male" : "Female",
                    nric = entityPatient.IC,
                    address = entityPatient.Address,
                    tel = entityPatient.PhoneNumber
                };
                request.AddJsonBody(JsonConvert.SerializeObject(new {
                    patient = patientData
                }));
                var responseData = await ExecuteRequest<AvixoCreatePatientResponse>(request);

                return responseData.data.url;
            }
        }

        public static async Task<string> FetchToken()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11;
            var client = new RestClient(ApiUrl);
            var request = new RestRequest("auth/login", Method.POST);
            request.AddJsonBody(new
            {
                username = LoginUsername,
                password = LoginPassword
            });
            var response = await client.ExecuteAsync(request);
            if (response.IsSuccessful)
            {
                var responseJson = JObject.Parse(response.Content);
                if (responseJson.Value<string>("status") == "ok")
                {
                    return responseJson.ToObject<AvixoLoginResponse>().token;
                }
            }
            if (response.ResponseStatus == ResponseStatus.Completed)
            {
                log.Error($"Error obtaining Avixo token from {response.ResponseUri}. (Status code: {response.StatusCode}) (Response body: {response.Content})");
            }
            else
            {
                log.Error($"Error obtaining Avixo token. {response.ErrorMessage}");
            }
            throw new WebApiException(new WebApiError(WebApiErrorCode.ThirdPartyRequestError, Errors.ErrorAvixoAuthenticateFailed));
        }

        public static string GetMcPdf(string accessToken, Guid mcGuid)
        {
            using (var db = new Entity.db_HeyDocEntities())
            {
                Entity.AvixoMedicalCertificate entityMc = null;
                if (accessToken != null)
                {
                    var entityUser = AccountService.GetEntityUserByAccessToken(db, accessToken);
                    entityMc = db.AvixoMedicalCertificates.FirstOrDefault(e => e.Id == mcGuid && e.UserId == entityUser.UserId);
                }
                if (entityMc == null)
                {
                    var msg = accessToken != null ? HCPPatient.ErrorMCNotFound : HCPPatient.ErrorMCNotFoundNotLoggedIn;
                    throw new WebApiException(new WebApiError(WebApiErrorCode.NotFound, msg));
                }
                return entityMc.BlobUrl + SASService.GetBlobSignature(McBlobContainer, entityMc.BlobUrl, expiryDateinUtc: DateTime.UtcNow.AddMinutes(5));
            }
        }

        public static async Task<T> ExecuteRequest<T>(IRestRequest request)
        {
            if (BearerToken == null)
            {
                BearerToken = await FetchToken();
            }
            var client = new RestClient(ApiUrl)
            {
                Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(BearerToken, "Bearer")
            };

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11;
            var response = await client.ExecuteAsync(request);
            if (response.IsSuccessful)
            {
                var responseJson = JObject.Parse(response.Content);
                if (responseJson.Value<string>("status") != "error")
                {
                    return responseJson.ToObject<T>();
                }
            }
            // TODO: Add more specific check for whether the error is from token expiration before retrying
            else if (response.ResponseStatus == ResponseStatus.Completed)
            {
                BearerToken = await FetchToken();
                client.Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(BearerToken, "Bearer");
                response = client.Execute(request);
                if (response.IsSuccessful)
                {
                    var responseJson = JObject.Parse(response.Content);
                    if (responseJson.Value<string>("status") != "error")
                    {
                        return responseJson.ToObject<T>();
                    }
                }
            }

            if (response.ResponseStatus == ResponseStatus.Completed)
            {
                log.Error($"Error calling Avixo {response.ResponseUri}. (Status code: {response.StatusCode}) (Response body: {response.Content})");
            }
            else
            {
                log.Error($"Error calling Avixo {response.ResponseUri}. {response.ErrorMessage}");
            }
            throw new WebApiException(new WebApiError(WebApiErrorCode.ThirdPartyRequestError, Errors.ErrorAvixoResponseError));
        }
    }
}
