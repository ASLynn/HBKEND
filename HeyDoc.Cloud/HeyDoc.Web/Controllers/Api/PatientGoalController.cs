using HeyDoc.Web.Lib;
using HeyDoc.Web.Models;
using HeyDoc.Web.Services;
using HeyDoc.Web.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.ValueProviders;
using System.Web.Http.ValueProviders.Providers;

namespace HeyDoc.Web.Controllers.Api
{
    public class PatientGoalController : ApiController
    {
        [HttpPost]
        public Task<PatientGoalModel> Add([ValueProvider(new[] { typeof(HeaderValueProviderFactory), typeof(QueryStringValueProviderFactory) })] string accessToken, int chatRoomId, PatientGoalModel model)
        {
            return WebApiWrapper.CallAsync(() => PatientGoalService.SetGoal(accessToken, chatRoomId, model));
        }

        [HttpGet]
        public List<PatientGoalModel> List([ValueProvider(new[] { typeof(HeaderValueProviderFactory), typeof(QueryStringValueProviderFactory) })] string accessToken, int chatRoomId, int take = 15, int skip = 0)
        {
            return WebApiWrapper.Call<List<PatientGoalModel>>(e => PatientGoalService.GetGoalList(accessToken, chatRoomId, take, skip));
        }

        [HttpPut]
        public BoolResult Set([ValueProvider(new[] { typeof(HeaderValueProviderFactory), typeof(QueryStringValueProviderFactory) })] string accessToken, int goalId, bool isComplete)
        {
            return WebApiWrapper.Call<BoolResult>(e => PatientGoalService.SetCompleteGoal(accessToken, goalId, isComplete));
        }

        [HttpDelete]
        public BoolResult Delete([ValueProvider(new[] { typeof(HeaderValueProviderFactory), typeof(QueryStringValueProviderFactory) })] string accessToken, int goalId)
        {
            return WebApiWrapper.Call<BoolResult>(e => PatientGoalService.DeleteGoal(accessToken, goalId));
        }
        [HttpPost]
        public Task<RemarkModel> CreateRemark([ValueProvider(new[] { typeof(HeaderValueProviderFactory), typeof(QueryStringValueProviderFactory) })] string accessToken, RemarkModel model)
        {
            return WebApiWrapper.CallAsync(() => PatientGoalService.CreateRemark(accessToken, model));
        }
        [HttpGet]
        public List<RemarkModel> GetRemarks([ValueProvider(new[] { typeof(HeaderValueProviderFactory), typeof(QueryStringValueProviderFactory) })] string accessToken, long goalId, int take = 15, int skip = 0)
        {
            return WebApiWrapper.Call<List<RemarkModel>>(e => PatientGoalService.GetRemarks(accessToken, goalId,take,skip));
        }
        [HttpGet]
        public RemarkModel GetRemark([ValueProvider(new[] { typeof(HeaderValueProviderFactory), typeof(QueryStringValueProviderFactory) })] string accessToken, long goalId,long remarkId)
        {
            return WebApiWrapper.Call<RemarkModel>(e => PatientGoalService.GetRemark(accessToken, goalId,remarkId));
        }
        [HttpDelete]
        public BoolResult DeleteRemark([ValueProvider(new[] { typeof(HeaderValueProviderFactory), typeof(QueryStringValueProviderFactory) })] string accessToken, long goalId, long remarkId)
        {
            return WebApiWrapper.Call<BoolResult>(e => PatientGoalService.DeleteRemark(accessToken, goalId, remarkId));
        }
    }
}
