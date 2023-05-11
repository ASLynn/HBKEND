using HeyDoc.Web.Models;
using HeyDoc.Web.Services;
using HeyDoc.Web.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace HeyDoc.Web.Controllers.Api
{
    public class BranchController : ApiController
    {
        [HttpGet]
        public List<BranchModel> GetBranchListMobile(int corperateId, int skip, int take)
        {
            return WebApiWrapper.Call<List<BranchModel>>(e => BranchService.GetBranchListMobile(corperateId, skip, take));
        }
    }
}