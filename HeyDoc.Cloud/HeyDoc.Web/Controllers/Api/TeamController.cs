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
    public class TeamController : ApiController
    {
        [HttpGet]
        public List<DoctorModel> DoctorListPWA(int companyId, int categoryId, string searchString, long groupId = 0, [ValueProvider(new[] { typeof(HeaderValueProviderFactory), typeof(QueryStringValueProviderFactory) })] string accessToken = "", int take = 15, int skip = 0)
        {
            return WebApiWrapper.Call<List<DoctorModel>>(e => TeamService.GetDoctorListNoLogin(companyId, accessToken, searchString, categoryId, groupId, take, skip));
        }

        [HttpGet]
        public List<DoctorModel> List(int categoryId, string searchString, long groupId = 0, [ValueProvider(new[] { typeof(HeaderValueProviderFactory), typeof(QueryStringValueProviderFactory) })] string accessToken = "", int take = 15, int skip = 0)
        {
            return WebApiWrapper.Call<List<DoctorModel>>(e => TeamService.GetDoctorList(accessToken, searchString, categoryId, groupId, take, skip));

        }
        [HttpGet]
        public List<DoctorModel> List(int companyId, int categoryId, string searchString, long groupId = 0, [ValueProvider(new[] { typeof(HeaderValueProviderFactory), typeof(QueryStringValueProviderFactory) })] string accessToken = "", int take = 15, int skip = 0)
        {
            return WebApiWrapper.Call<List<DoctorModel>>(e => TeamService.GetDoctorListNoLogin(companyId, accessToken, searchString, categoryId, groupId, take, skip));

        }
        // Doctor list API that will return a set category based on the user details such as corporate or user create source
        [HttpGet]
        public List<DoctorModel> CorporateDoctorList(string searchString, [ValueProvider(new[] { typeof(HeaderValueProviderFactory), typeof(QueryStringValueProviderFactory) })] string accessToken, long groupId, int take = 15, int skip = 0)
        {
            return WebApiWrapper.Call(e => TeamService.GetDoctorList(accessToken, searchString, null, groupId, take, skip));
        }

        [HttpGet]
        public DoctorModel Single(int doctorId, [ValueProvider(new[] { typeof(HeaderValueProviderFactory), typeof(QueryStringValueProviderFactory) })] string accessToken = "")
        {
            return WebApiWrapper.Call<DoctorModel>(e => TeamService.GetDoctor(accessToken, doctorId));
        }

        //[HttpGet]
        //public DoctorModel Random(string accessToken)
        //{
        //    return WebApiWrapper.Call<DoctorModel>(e => TeamService.GetRandomDoctor(accessToken));
        //}

        [HttpGet]
        public List<CategoryModel> Categories([ValueProvider(new[] { typeof(HeaderValueProviderFactory), typeof(QueryStringValueProviderFactory) })] string accessToken = "",  int take = 15, int skip = 0)
        {
            return WebApiWrapper.Call<List<CategoryModel>>(e => TeamService.GetAllCategories(accessToken,skip,take));
        }

        [HttpGet]
        public List<GroupModel> Groups(int categoryId, [ValueProvider(new[] { typeof(HeaderValueProviderFactory), typeof(QueryStringValueProviderFactory) })] string accessToken = "", int take = 15, int skip = 0)
        {
            return WebApiWrapper.Call<List<GroupModel>>(e => TeamService.GetGroupList(accessToken, skip, take, categoryId));
        }

        // Group list API that will return a set category based on the user details such as corporate or user create source
        [HttpGet]
        public List<GroupModel> CorporateGroups([ValueProvider(new[] { typeof(HeaderValueProviderFactory), typeof(QueryStringValueProviderFactory) })] string accessToken, int take = 15, int skip = 0)
        {
            return WebApiWrapper.Call(e => TeamService.GetGroupList(accessToken, skip, take));
        }
    }
}
