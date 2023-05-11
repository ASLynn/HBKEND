using HeyDoc.Web.Lib;
using HeyDoc.Web.Models;
using HeyDoc.Web.Services;
using HeyDoc.Web.WebApi;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.ValueProviders;
using System.Web.Http.ValueProviders.Providers;

namespace HeyDoc.Web.Controllers.Api
{
    /// <summary>
    /// AI API
    /// </summary>
    public class AIController : ApiController
    {
        /// <summary>
        /// Upload picture to be processed by AI
        /// </summary>
        /// <param name="accessToken">Access token</param>
        /// <param name="classifier">Classifier</param>
        /// <returns>AI result</returns>
        [HttpPost]
        public async Task<AIModel> Medical([ValueProvider(new[] { typeof(HeaderValueProviderFactory), typeof(QueryStringValueProviderFactory) })] string accessToken, string classifier)
        {
            //TODO: Create a BaseController or Filter to do authentication
            using (Entity.db_HeyDocEntities Db = new Entity.db_HeyDocEntities())
            {
                Entity.UserProfile entityUser = AccountService.GetEntityUserByAccessToken(Db, accessToken, false);
            }

            //TODO: Return value instead throw exception. 
            if (!Request.Content.IsMimeMultipartContent())
                throw new WebApiException(new WebApiError(WebApiErrorCode.UnsupportedMediaType));

            var multipartResult = await Request.Content.ReadAsMultipartAsync();
            if (!multipartResult.Contents.Any())
               return new AIModel();

            foreach (var data in multipartResult.Contents)
            {
                var fileName = data.Headers.ContentDisposition.FileName;
                if (!string.IsNullOrEmpty(fileName))
                {
                    fileName = fileName.Substring(1, fileName.Length - 2);
                    var fileStream = await data.ReadAsStreamAsync();
                    var imageResult = await AIService.Medical(fileStream, classifier, fileName);
                    AIModel res = new AIModel();
                    res.Image = imageResult.Image;
                    res.Score = imageResult.Score;
                    res.RiskLevel = AIService.GetRiskLevel(imageResult.Score);
                    return res;
                }
            }
            //at this point, we dont have any data
            return new AIModel();
        }
    }
}