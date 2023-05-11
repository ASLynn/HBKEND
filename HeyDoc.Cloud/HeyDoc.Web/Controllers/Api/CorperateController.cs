using HeyDoc.Web.Helpers;
using HeyDoc.Web.Lib;
using HeyDoc.Web.Models;
using HeyDoc.Web.Services;
using HeyDoc.Web.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.ValueProviders;
using System.Web.Http.ValueProviders.Providers;

namespace HeyDoc.Web.Controllers.Api
{
    public class CorperateController : ApiController
    {
        [HttpGet]
        public List<CorporateModel> GetCorporateListMobile(int skip, int take)
        {
            return WebApiWrapper.Call<List<CorporateModel>>(e => CorporateService.GetCorporateListMobile(skip, take));
        }

        [HttpGet]
        // TODO M: Possibly rename
        public List<PharmacyOutletModel> GetPharmacy1OutletListMobile([ValueProvider(new[] { typeof(HeaderValueProviderFactory), typeof(QueryStringValueProviderFactory) })] string accessToken, int skip, int take)
        {
            return WebApiWrapper.Call<List<PharmacyOutletModel>>(e => CorporateService.GetPharmacy1OutletListMobile(accessToken, skip, take));
        }

        [HttpGet]
        public List<OnSiteDispenseModel> GetOnSiteListMobile([ValueProvider(new[] { typeof(HeaderValueProviderFactory), typeof(QueryStringValueProviderFactory) })] string accessToken, int skip, int take)
        {
            return WebApiWrapper.Call<List<OnSiteDispenseModel>>(e => OnSiteDispensesService.GetOnSiteListMobile(accessToken, skip, take));
        }

        [HttpGet]
        public List<PrescriptionLogModel> GetPrescriptionLogStatus([ValueProvider(new[] { typeof(HeaderValueProviderFactory), typeof(QueryStringValueProviderFactory) })] string accessToken, int dispatchId, int skip, int take)
        {
            return WebApiWrapper.Call<List<PrescriptionLogModel>>(e => CorporateService.GetPrescriptionLogStatus(accessToken, dispatchId, skip, take));
        }
        [HttpGet]
        public List<CorporatePositionModel> GetCorporatePosition(int corporateId)
        {           
            return WebApiWrapper.Call<List<CorporatePositionModel>>(e => CorporateService.GetCorporatePositions(corporateId));             
        }
        [HttpGet]
        public CorporateModel GetCorporateDetailsWithPolicy([ValueProvider(new[] { typeof(HeaderValueProviderFactory), typeof(QueryStringValueProviderFactory) })] string accessToken, int corporateId)
        {
            using (var db = new Entity.db_HeyDocEntities())
            {
                var entityUser = AccountService.GetEntityUserByAccessToken(db, accessToken, false);
                if (!entityUser.Roles.Any(r => ConstantHelper.DoctorRoles.Contains(r)))
                {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.Unauthorized));
                }

            }
            return WebApiWrapper.Call(e => CorporateService.GetCorporateById(corporateId, true));
        }
        private void isAccessTokenValid(string accessToken)
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityUser = AccountService.GetEntityUserByAccessToken(db, accessToken, false);
            }
        }
    }
}