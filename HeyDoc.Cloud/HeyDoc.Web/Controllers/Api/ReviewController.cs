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
    public class ReviewController : ApiController
    {
        [HttpPost]
        public ReviewModel Review([ValueProvider(new[] { typeof(HeaderValueProviderFactory), typeof(QueryStringValueProviderFactory) })] string accessToken, int userId, ReviewModel model)
        {
            return WebApiWrapper.Call<ReviewModel>(e => ReviewService.ReviewDoctor(accessToken, userId, model));
        }

        [HttpGet]
        public List<ReviewModel> List([ValueProvider(new[] { typeof(HeaderValueProviderFactory), typeof(QueryStringValueProviderFactory) })] string accessToken, int? userId = null, int take = 15, int skip = 0)
        {
            return WebApiWrapper.Call<List<ReviewModel>>(e => ReviewService.ReviewList(accessToken, userId, take, skip));
        }
    }
}
