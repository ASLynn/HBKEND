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
    public class TransactionController : ApiController
    {
        [HttpGet]
        public TransactionModel Transaction([ValueProvider(new[] { typeof(HeaderValueProviderFactory), typeof(QueryStringValueProviderFactory) })] string accessToken)
        {
            return WebApiWrapper.Call<TransactionModel>(e => PaymentService.GetTranscationHistory(accessToken));
        }

        [HttpGet]
        public List<CashOutRequestModel> CashOutRequestList([ValueProvider(new[] { typeof(HeaderValueProviderFactory), typeof(QueryStringValueProviderFactory) })] string accessToken, int skip, int take)
        {
            return WebApiWrapper.Call<List<CashOutRequestModel>>(e => PaymentService.GetCashOutRequests(accessToken, skip, take));
        }

        [HttpPost]
        public CashOutRequestModel RequestCashOut([ValueProvider(new[] { typeof(HeaderValueProviderFactory), typeof(QueryStringValueProviderFactory) })] string accessToken, DoctorBankModel model)
        {
            return WebApiWrapper.Call<CashOutRequestModel>(e => PaymentService.RequestForCashOut(accessToken,model));
        }

        [HttpGet]
        public List<BankModel> Banks([ValueProvider(new[] { typeof(HeaderValueProviderFactory), typeof(QueryStringValueProviderFactory) })] string accessToken)
        {
            return WebApiWrapper.Call<List<BankModel>>(e => PaymentService.GetBankList(accessToken));
        }

        [HttpGet]
        public List<DoctorBankModel> RegisteredAccount([ValueProvider(new[] { typeof(HeaderValueProviderFactory), typeof(QueryStringValueProviderFactory) })] string accessToken)
        {
            return WebApiWrapper.Call<List<DoctorBankModel>>(e => PaymentService.GetRegisteredBanks(accessToken));
        }
    }
}
