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
    public class PaymentController : ApiController
    {
        [HttpGet]
        public StringResult ClientToken([ValueProvider(new[] { typeof(HeaderValueProviderFactory), typeof(QueryStringValueProviderFactory) })] string accessToken)
        {
            return WebApiWrapper.Call<StringResult>(e => BraintreeService.GetClientToken(accessToken));
        }

        [HttpPost]
        public BoolResult PaymentMethod([ValueProvider(new[] { typeof(HeaderValueProviderFactory), typeof(QueryStringValueProviderFactory) })] string accessToken, BrainTreePaymentModel model)
        {
            return WebApiWrapper.Call<BoolResult>(e => BraintreeService.AddPaymentMethod(accessToken,model));
        }

        //[HttpGet]
        //public bool trans(string accessToken)
        //{
        //    return WebApiWrapper.Call<bool>(e => BraintreeService.trans(accessToken));
        //}

        [HttpGet]
        public List<PaymentRequestModel> TransactionList([ValueProvider(new[] { typeof(HeaderValueProviderFactory), typeof(QueryStringValueProviderFactory) })] string accessToken, int skip, int take)
        {
            return WebApiWrapper.Call<List<PaymentRequestModel>>(e => PaymentService.GetAllTransactionList(accessToken, skip, take));
        }

        [HttpGet]
        public PaymentRequestModel Transaction([ValueProvider(new[] { typeof(HeaderValueProviderFactory), typeof(QueryStringValueProviderFactory) })] string accessToken, long paymentRequestId)
        {
            return WebApiWrapper.Call<PaymentRequestModel>(e => PaymentService.GetTransaction(accessToken, paymentRequestId));
        }

        [HttpGet]
        public List<CreditCardModel> CardDetails([ValueProvider(new[] { typeof(HeaderValueProviderFactory), typeof(QueryStringValueProviderFactory) })] string accessToken)
        {
            return WebApiWrapper.Call<List<CreditCardModel>>(e => BraintreeService.GetDefaultPaymentMethodDetails(accessToken));
        }

        [HttpGet]
        public BoolResult DeleteCard([ValueProvider(new[] { typeof(HeaderValueProviderFactory), typeof(QueryStringValueProviderFactory) })] string accessToken)
        {
            return WebApiWrapper.Call<BoolResult>(e => BraintreeService.DeleteCard(accessToken));
        }

        [HttpGet]
        public PromoCodeModel ValidatePromo([ValueProvider(new[] { typeof(HeaderValueProviderFactory), typeof(QueryStringValueProviderFactory) })] string accessToken, string promoCode, int doctorId)
        {
            return WebApiWrapper.Call<PromoCodeModel>(e => PaymentService.ValidatePromoCode(accessToken, promoCode,doctorId));
        }
        [HttpPost]
        public StringResult GetPointBalanceForUser([ValueProvider(new[] { typeof(HeaderValueProviderFactory), typeof(QueryStringValueProviderFactory) })] string accessToken)
        {
           return WebApiWrapper.Call(e => PaymentService.GetPointBalanceForUser(accessToken));
        }
        [HttpPost]
        public StringResult GetPaymentOrderID([ValueProvider(new[] { typeof(HeaderValueProviderFactory), typeof(QueryStringValueProviderFactory) })] string accessToken)
        {
            return WebApiWrapper.Call(e => PaymentService.GetPaymentOrderID(accessToken));
        }
        [HttpPost]
        public PaymentMethodModel GetPaymentMethod([ValueProvider(new[] { typeof(HeaderValueProviderFactory), typeof(QueryStringValueProviderFactory) })] string accessToken)
        {
            isAccessTokenValid(accessToken);
            return WebApiWrapper.Call(e => PaymentService.GetPaymentMethod(accessToken));
        }
        [HttpPost]
        public StringResult GetPaymentMethodss([ValueProvider(new[] { typeof(HeaderValueProviderFactory), typeof(QueryStringValueProviderFactory) })] string accessToken)
        {
            return WebApiWrapper.Call(e => PaymentService.GetPaymentMethodss(accessToken));
        }
        [HttpPost]
        public StringResult SaveTopupAmt([ValueProvider(new[] { typeof(HeaderValueProviderFactory), typeof(QueryStringValueProviderFactory) })] string accessToken,string orderId,string amount)
        {
            isAccessTokenValid(accessToken);
            return WebApiWrapper.Call(e => PaymentService.SaveTopupAmt(orderId,amount));
        }
        [HttpPost]
        public IHttpActionResult TopupPaymentCallback(string OrderId,string amount,string status,string payment_method)
        {
            if(PaymentService.TopupPaymentCallback(OrderId,amount,status,payment_method))
            {
                return Json(new
                {
                    status = "success"
                });
            }
            else
            {
                return Json(new
                {
                    status = "fail"
                });
            }
        }
        [HttpPost]
        public IHttpActionResult WalletPaymentCallback(string InvoiceNo, string amount, string status, string walletName)
        {
            if (PaymentService.WalletPaymentCallback(InvoiceNo, amount, status, walletName))
            {
                return Json(new
                {
                    status = "success"
                });
            }
            else
            {
                return Json(new
                {
                    status = "fail"
                });
            }
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
