using HeyDoc.Web.Models;
using HeyDoc.Web.Services;
using HeyDoc.Web.WebApi;
using System.Threading.Tasks;
using System.Web.Http;

namespace HeyDoc.Web.Controllers.Api
{
    public class IcdController : ApiController
    {
        [HttpGet]
        public async Task<IcdSearchResponseModel> Search(string accessToken, string query)
        {
            return await WebApiWrapper.CallAsync(() => IcdService.Search(accessToken, query));
        }
    }
}