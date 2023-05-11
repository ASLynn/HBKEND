using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Results;
using System.Web.Http.ValueProviders;
using System.Web.Http.ValueProviders.Providers;
using HeyDoc.Web.Lib;
using HeyDoc.Web.Services;
namespace HeyDoc.Web.Controllers.Api
{
    public class MobileController : ApiController
    {
        [HttpPost]
        [Route("api/Mobile/SignUpInfo")]
        public IHttpActionResult SignUpInfo([ValueProvider(new[] { typeof(HeaderValueProviderFactory), typeof(QueryStringValueProviderFactory) })] string AccessKey)
        {
            if(SettingService.GetList().Where(x=>x.Value == AccessKey).Count() > 0 )
            {
                return Json(new
                {
                    ResCode = "000",
                    ResDesc = "SignUpInfo Requested Successful",
                    CountryList = CountryService.GetCountryList(),
                    CategoryList = TeamService.GetCategoryList(),
                    StateList = StateService.GetState(),
                    TownshipList = TownshipService.GetTownship(),
                    FacilityList = FacilityService.GetFacility(),
                    SpecialityList = SpecialityService.GetSpecialityList(),
                    QualificationList = QualificationService.GetQualificationList()
                    //TitleList = TitleService.GetTitleList()

                });
            }
            else
            {
                return Json(new
                {
                    ResCode = "010",
                    ResDesc = "Authorization Failed!"
                   
                });
            }
        }

       
    }
}