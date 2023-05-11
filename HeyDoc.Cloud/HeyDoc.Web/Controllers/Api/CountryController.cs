using HeyDoc.Web.Models;
using HeyDoc.Web.Services;
using HeyDoc.Web.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace HeyDoc.Web.Controllers.Api
{
    public class CountryController : ApiController
    {
        [HttpGet]
        public List<CountryModel> List()
        {
            return WebApiWrapper.Call<List<CountryModel>>(e => CountryService.CountryList());
        }
    }
    public class StateController : ApiController
    {
        [HttpGet]
        public List<StateModel> List()
        {
            return WebApiWrapper.Call<List<StateModel>>(e => StateService.GetStateList());
        }
    }
    public class TownshipController : ApiController
    {
        [HttpGet]
        public List<TownshipModel> List()
        {
            return WebApiWrapper.Call<List<TownshipModel>>(e => TownshipService.GetTownshipList());
        }
        [HttpPost]
        [HttpGet]
        public List<TownshipModel> ListByState(int StateId)
        {
            return WebApiWrapper.Call<List<TownshipModel>>(e => TownshipService.GetTownshipByState(StateId));
        }   
    }
    public class SpecialityController : ApiController
    {
        [HttpGet]
        public List<SpecialityModel> List()
        {
            return WebApiWrapper.Call<List<SpecialityModel>>(e => SpecialityService.GetSpecialityList());
        }
    }
    public class QualificationController : ApiController
    {
        [HttpGet]
        public List<QualificationModel> List()
        {
            return WebApiWrapper.Call<List<QualificationModel>>(e => QualificationService.GetQualificationList());
        }
    }
    public class RelationshipController : ApiController
    {
        [HttpGet]
        public List<RelationshipModel> List()
        {
            return WebApiWrapper.Call<List<RelationshipModel>>(e => RelationshipService.GetRelationshipList());
        }
    }
    public class FacilityController : ApiController
    {
        [HttpGet]
        public List<FacilityListModel> List()
        {
            return WebApiWrapper.Call<List<FacilityListModel>>(e => FacilityService.GetFacilityAll());
        }
    }
}
