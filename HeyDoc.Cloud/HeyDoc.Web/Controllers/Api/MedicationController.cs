using HeyDoc.Web.Lib;
using HeyDoc.Web.Models;
using HeyDoc.Web.Services;
using HeyDoc.Web.WebApi;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.ValueProviders;
using System.Web.Http.ValueProviders.Providers;

namespace HeyDoc.Web.Controllers.Api
{
    public class MedicationController : ApiController
    {
        [HttpGet]
        public MedicationModel GetUserIdByMedicalId([ValueProvider(new[] { typeof(HeaderValueProviderFactory), typeof(QueryStringValueProviderFactory) })] string accessToken, long medicationId)
        {
            return WebApiWrapper.Call<MedicationModel>(e => MedicationService.GetMedication(accessToken, medicationId));

        }
        [HttpGet]
        public List<MedicationModel> GetList([ValueProvider(new[] { typeof(HeaderValueProviderFactory), typeof(QueryStringValueProviderFactory) })] string accessToken, int take = 15, int skip = 0, string searchString = null)
        {
            return WebApiWrapper.Call<List<MedicationModel>>(e => MedicationService.GetMedicationList(accessToken, -1, 0, searchString)); // removed skip and take for app compatibility ('take all' because of bug)
        }
        
        [HttpGet]
        public List<MedicationModel> GetListWithSkipTake([ValueProvider(new[] { typeof(HeaderValueProviderFactory), typeof(QueryStringValueProviderFactory) })] string accessToken, int take = 15, int skip = 0, string searchString = null)
        {
            return WebApiWrapper.Call(e => MedicationService.GetMedicationList(accessToken, take, skip, searchString, sourceType: null));
        }
    }
}