using HeyDoc.Web.Entity;
using HeyDoc.Web.Helpers;
using HeyDoc.Web.Models;
using HeyDoc.Web.WebApi;
using Microsoft.Azure;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace HeyDoc.Web.Services
{
    public class IcdService
    {

        private static readonly string WhoIcdApiEndpoint = "https://id.who.int";

        private static readonly string ReleaseId = "2022-02";
        private static readonly string LinearizationName = "mms";

        private static string _AccessToken = null;
        private static DateTime _TokenExpiry = DateTime.MinValue;

        private static async Task<string> GetAccessToken(HttpClient client, CancellationToken? cancelToken = null)
        {
            if (string.IsNullOrEmpty(_AccessToken) || DateTime.UtcNow > _TokenExpiry)
            {
                var response = await GetIcdApiToken(client, cancelToken);
                _AccessToken = response.access_token;
                _TokenExpiry = DateTime.UtcNow.AddSeconds(response.expires_in - 300); // re-obtain token 5 minutes earlier from expiry
            }
            return _AccessToken;
        }

        public static async Task<IcdSearchResponseModel> Search(string accessToken, string query)
        {
            using (var db = new db_HeyDocEntities())
            {
                var entityUser = AccountService.GetEntityUserByAccessToken(db, accessToken, false);
                if (!entityUser.Roles.Any(r => ConstantHelper.DoctorRoles.Contains(r)))
                {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.Unauthorized));
                }
            }

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("API-Version", "v2");
                client.DefaultRequestHeaders.Add("Accept-Language", "en");

                var cancelSource = new CancellationTokenSource();

                var taskList = new List<Task<HttpResponseMessage>>
                {

                    SearchWhoIcd(client, query, cancelSource.Token)
                };

                while (taskList.Count > 0)
                {
                    var completedTask = await Task.WhenAny(taskList);
                    try
                    {
                        var response = await completedTask;
                        if (taskList.Count == 1)
                        {
                            response.EnsureSuccessStatusCode();
                        }
                        else if (!response.IsSuccessStatusCode)
                        {
                            taskList.Remove(completedTask);
                            continue;
                        }

                        cancelSource.Cancel();
                        var model = await response.Content.ReadAsAsync<IcdSearchResponseModel>();
                        model.releaseId = ReleaseId;
                        model.linearizationName = LinearizationName;
                        return model;
                    }
                    catch (OperationCanceledException)
                    {
                        taskList.Remove(completedTask);
                        continue;
                    }
                }
                throw new WebApiException(new WebApiError(WebApiErrorCode.UnknownError));
            }
        }

        private static async Task<WhoIcdApiResponseModel> GetIcdApiToken(HttpClient client, CancellationToken? cancelToken = null)
        {
            HttpRequestMessage requestMsg = new HttpRequestMessage(HttpMethod.Post, ConstantHelper.IcdApiOAuthEndpoint)
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "client_id", CloudConfigurationManager.GetSetting("IcdApiClientId") },
                    { "client_secret", CloudConfigurationManager.GetSetting("IcdApiClientSecret") },
                    { "scope", "icdapi_access" },
                    { "grant_type", "client_credentials" }
                })
            };

            HttpResponseMessage response;
            if (cancelToken.HasValue)
            {
                response = await client.SendAsync(requestMsg, cancelToken.Value);
            }
            else
            {
                response = await client.SendAsync(requestMsg);
            }
            var jsonString = await response.Content.ReadAsStringAsync();
            var model = JsonConvert.DeserializeObject<WhoIcdApiResponseModel>(jsonString);
            return model;
        }

        private static async Task<HttpResponseMessage> SearchWhoIcd(HttpClient client, string query, CancellationToken cancelToken)
        {
           var token = await GetAccessToken(client, cancelToken);
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer { token }");

            cancelToken.ThrowIfCancellationRequested();
            return await client.GetAsync($"{ WhoIcdApiEndpoint }/icd/release/11/{ ReleaseId }/{ LinearizationName }/search?q={ query }", cancelToken);
        }
    }
}