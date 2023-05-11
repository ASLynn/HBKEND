using HeyDoc.Web.Lib;
using HeyDoc.Web.Models;
using HeyDoc.Web.Services;
using HeyDoc.Web.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.ValueProviders;
using System.Web.Http.ValueProviders.Providers;

namespace HeyDoc.Web.Controllers.Api
{
    public class PatientController : ApiController
    {
        [HttpGet]
        public PatientModel View([ValueProvider(new[] { typeof(HeaderValueProviderFactory), typeof(QueryStringValueProviderFactory) })] string accessToken, int? userId = null)
        {
            return WebApiWrapper.Call<PatientModel>(e => PatientService.ViewBioData(accessToken, userId));
        }
        [HttpPost]
        [HttpPut]
        public BoolResult Update([ValueProvider(new[] { typeof(HeaderValueProviderFactory), typeof(QueryStringValueProviderFactory) })] string accessToken, PatientModel model)
        {
            return WebApiWrapper.Call<BoolResult>(e => PatientService.UpdateBioData(accessToken, model));
        }

        [HttpPost]
        public BoolResult UpdatePatient([ValueProvider(new[] { typeof(HeaderValueProviderFactory), typeof(QueryStringValueProviderFactory) })] string accessToken, PatientModel model)
        {
            return WebApiWrapper.Call<BoolResult>(e => PatientService.UpdatePatientBioData(accessToken, model));
        }

        [HttpGet]
        public BioDataModel ViewBioData([ValueProvider(new[] { typeof(HeaderValueProviderFactory), typeof(QueryStringValueProviderFactory) })] string accessToken, BioDataType type, int? userId = null)
        {
            return WebApiWrapper.Call<BioDataModel>(e => PatientService.BioDataList(accessToken, type, userId));
        }

    }
}
