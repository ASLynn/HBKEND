using HeyDoc.Web.Helpers;
using HeyDoc.Web.Models;
using HeyDoc.Web.Resources;
using HeyDoc.Web.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System.Collections.Specialized;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Net.Http;
using System.Net.Http.Headers;
using RestSharp;
using Newtonsoft.Json.Linq;

namespace HeyDoc.Web.Services
{
    public class PaymentService
    {        
        public static List<PaymentRequestModel> GetAllTransactionList(string accessToken, int skip, int take)
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityUser = AccountService.GetEntityUserByAccessToken(db, accessToken, false);
                if (entityUser.Role != RoleType.Doctor && entityUser.Role != RoleType.User)
                {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.Unauthorized, string.Format(Errors.UnauthorizedRole, ConstantHelper.Doc2UsEmailContact)));
                }
                var result = _GetTransactionsPartial(skip, take, db, entityUser);

                return result;
            }
        }

        private static List<PaymentRequestModel> _GetTransactionsPartial(int skip, int take, Entity.db_HeyDocEntities db, Entity.UserProfile entityUser)
        {
            var entityPaymentList = db.PaymentRequests.Where(e => (e.ChatRoom.PatientId == entityUser.UserId || e.ChatRoom.DoctorId == entityUser.UserId) && e.PaymentStatus==PaymentStatus.Paid);
            entityPaymentList = entityPaymentList.OrderByDescending(e => e.CreateDate).Skip(skip).Take(take);
            var result = new List<PaymentRequestModel>();
            foreach (var entityPayment in entityPaymentList)
            {
                result.Add(new PaymentRequestModel(entityPayment, true));
            }
            return result;
        }

        public static PaymentRequestModel GetTransaction(string accessToken, long paymentRequestId)
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityUser = AccountService.GetEntityUserByAccessToken(db, accessToken, false);
                var entityPayment = db.PaymentRequests.FirstOrDefault(e => e.PaymentRequestId == paymentRequestId && (e.ChatRoom.PatientId == entityUser.UserId || e.ChatRoom.DoctorId == entityUser.UserId));
                if (entityPayment == null)
                {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.NotFound, Payments.ErrorTransactionNotFound));
                }
                return new PaymentRequestModel(entityPayment, true);
            }
        }

        public static TransactionModel GetTranscationHistory(string accessToken)
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityUser = AccountService.GetEntityUserByAccessToken(db, accessToken, false, RoleType.Doctor);
                var transactionList = _GetTransactionsPartial(0, 15, db, entityUser);
                TransactionModel result = new TransactionModel();
                result.Payments = transactionList;
                result.TotalAmount = db.PaymentRequests.Where(e => e.PaymentStatus == PaymentStatus.Paid && e.ChatRoom.DoctorId == entityUser.UserId && e.CashOutRequestId == null).Select(e=>e.HCPAmount).DefaultIfEmpty(0).Sum();
                result.CashOutRequestAmount = db.CashOutRequests.Where(e => e.CashOutRequestStatus == CashOutRequestStatus.Requested).Select(e => e.Amount).DefaultIfEmpty(0).Sum();
                return result;
            }
        }

        internal static StringResult GetPointBalanceForUser(string accessToken)
            {
            using (var db = new Entity.db_HeyDocEntities())
            {
                var entityUser = AccountService.GetEntityUserByAccessToken(db, accessToken, false);
                Entity.PointBalance pBalance = db.PointBalances.FirstOrDefault(e => e.UserID == entityUser.UserId);
                if(pBalance == null)
                {
                    return new StringResult("0");                    
                }
                else return new StringResult(pBalance.Balance.ToString());
            }
        }

        internal static PaymentMethodModel GetPaymentMethod(string accessToken)
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityUser = AccountService.GetEntityUserByAccessToken(db, accessToken, false);               
                var eCurrentLogin = db.CompanyWhiteLabelCurrentLogins.FirstOrDefault(e => e.UserId == entityUser.UserId);
                var eWhiteLabelCompany = db.CompanyWhiteLabels.FirstOrDefault(e => e.CompanyId == eCurrentLogin.CurrentLoginCompanyId);
                var ePaymentMethod = db.PaymentMethods.FirstOrDefault(e => (e.CompanyId == 1 && eWhiteLabelCompany.CompanyType == "c") || (e.CompanyId == eCurrentLogin.CurrentLoginCompanyId && eWhiteLabelCompany.CompanyType == "w"));
                return new PaymentMethodModel(ePaymentMethod);
            }
        }

        internal static StringResult GetPaymentMethodss(string accessToken)
        {
            //string[] paymentMethodstring = new string[] { "1", "2", "6", "8", "9", "10", "11", "12", "13", "15", "16" };
            int[] pmArr = new int[] { 1, 2, 6, 8, 9, 10, 11, 12, 13, 15, 16 };
            var jsonSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            string jsonPaymentMethods = jsonSerializer.Serialize(pmArr);
            return new StringResult(jsonPaymentMethods);
        }
        internal static StringResult SaveTopupAmt(string orderId, string amount)
        {
            using (var db = new Entity.db_HeyDocEntities())
            {                
                Entity.PointTopupRequest preq = db.PointTopupRequests.FirstOrDefault(e => e.RequestID == orderId);
                Entity.PaymentMethod paymentMethod = db.PaymentMethods.FirstOrDefault(e => e.merchant_id == "5d9f4570-0d3c-11ec-be75-bd09a09905e5");
                if (preq == null)
                {
                    return new StringResult("fail");
                }
                else
                {
                    preq.Amount = Convert.ToInt32(amount);
                    preq.Status = "ReadyAmount";
                    db.SaveChanges();

                    //Get PaymentMethods (Wave 1, KBZ 2, etc..)
                    int[] pmArr = new int[] { 1, 2, 6, 8, 9, 10, 11, 12, 13, 15, 16 };
                    var jsonSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                    string jsonPaymentMethods = jsonSerializer.Serialize(pmArr);

                    //GetStringArraySort
                    string[] StringArr = new string[] { paymentMethod.merchant_id, paymentMethod.service_name, paymentMethod.password, paymentMethod.email, amount, orderId, jsonPaymentMethods };
                    Array.Sort(StringArr, StringComparer.Ordinal);

                    //GetSignatureStringValue
                    string input = "";
                    foreach (string s in StringArr)
                    {
                        input += s;
                    }

                    //GetSecretKeyByte
                    string Secrettoken = "AliwP35Ho)ekq321nFR";
                    var secret = Encoding.ASCII.GetBytes(Secrettoken);

                    //HMAC_SHA1 HASH
                    var hashVal = HashingService.HMAC_SHA1.ToHMACSHA1(input, secret);

                    //URL ENCODE
                    hashVal = WebUtility.UrlEncode(hashVal.ToString().ToUpper());

                    return new StringResult(hashVal);
                }
            }
        }

        internal static StringResult DeductPointBalanceForUser(Entity.db_HeyDocEntities db, string accessToken, int consultPrice)
        {
            var entityUser = AccountService.GetEntityUserByAccessToken(db, accessToken, false);
            Entity.PointBalance pBalance = db.PointBalances.FirstOrDefault(e => e.UserID == entityUser.UserId);
            if (pBalance == null)
            {
                return new StringResult("0");
            }
            else
            {
                pBalance.Balance = pBalance.Balance - consultPrice;             
                return new StringResult(pBalance.Balance.ToString());
            }

        }
        internal static StringResult GetPaymentOrderID(string accessToken)
        {
            string preText = "";          
            if (System.Web.HttpContext.Current.Request.IsLocal || System.Web.HttpContext.Current.Request.Url.ToString().Contains("hopestaging")
                || System.Web.HttpContext.Current.Request.Url.ToString().Contains("hopewebstaging"))
            {
                preText = "pdHOPE-";
            }
            else
            {
                preText = "pdHOPE-";
            }
            using (var db = new Entity.db_HeyDocEntities())
            {
                var entityUser = AccountService.GetEntityUserByAccessToken(db, accessToken, false);
                Entity.PointTopupRequest topupOrder = new Entity.PointTopupRequest();
                topupOrder.RequestDate = DateTime.UtcNow;
                topupOrder.Status = "ReadyTopup";
                topupOrder.UserID = entityUser.UserId;
                topupOrder.RequestID = "";
                db.PointTopupRequests.Add(topupOrder);
                db.SaveChanges();
                string orderidModify = preText + topupOrder.ID + "_" + DateTime.Now.ToString("yyyy-MM-dd_T_HH-mm-ss");
                topupOrder.RequestID = orderidModify;
                db.SaveChanges();
                return new StringResult(orderidModify); //"testHOPE-1201_2022-08-05_T_16-24-42"
            }
        }

        internal static bool WalletPaymentCallback(string InvoiceNo, string amount, string status, string walletName)
        {
            if (InvoiceNo == null)
            {
                return false;
            }
            else
            {
                if(amount == null)
                {
                    return false;
                }
                else
                {
                    if(status == null)
                    {
                        return false;
                    }
                    else
                    {
                        if(walletName == null)
                        {
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }
                }
            }
        }
       

        internal static bool TopupPaymentCallback(string OrderId, string amount, string status, string payment_method)
        {
            using (var db = new Entity.db_HeyDocEntities())
            {
                using (var transaction = db.Database.BeginTransaction())
                {
                    Entity.PointTopupRequest topupOrder = db.PointTopupRequests.FirstOrDefault(e => e.RequestID.Trim() == OrderId.Trim());
                    Entity.PaymentMethod paymentMethod = db.PaymentMethods.FirstOrDefault(e => e.merchant_id == "5d9f4570-0d3c-11ec-be75-bd09a09905e5");
                    //SortArrayList(paymentMethod.merchant_id, paymentMethod.service_name, paymentMethod.password, paymentMethod.email, amount, OrderId);
                    //var hashvalue = HashingService.ToHMACSHA1("test");

                    //Check Hacker 
                    //Call API to check there is success transaction at BP Pay Start

                    //string token = "Bearer eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsImp0aSI6ImNlY2IwODA4ZGFiNWM1MGFkMDhiYmQ0MTM1YjFhMmM2ZWNlZjU0NTQ4NWRkYjZmMjE4YmUzZjkzOTQxYjc0NmM0OTlhZmI3NDI0MWYxYjFlIn0.eyJhdWQiOiIzIiwianRpIjoiY2VjYjA4MDhkYWI1YzUwYWQwOGJiZDQxMzViMWEyYzZlY2VmNTQ1NDg1ZGRiNmYyMThiZTNmOTM5NDFiNzQ2YzQ5OWFmYjc0MjQxZjFiMWUiLCJpYXQiOjE2NTcwMTI0MTksIm5iZiI6MTY1NzAxMjQxOSwiZXhwIjoxNjg4NTQ4NDE5LCJzdWIiOiIxMDQ3Iiwic2NvcGVzIjpbXX0.EY_Z1a_C3dwUZ - jTSAk3T8 - C5B8yP5NeqBoPYNRv6p0fYlcubuMW6 - SUUfSl8qVly9O - KWsRogVPWUuVAsOeHzYNeAAAkpjqtTE8rfQWi_JTGfRNS7DqgsamMFZcPaAbtzobGIstWW6BLLD7pg4359uzb_XbTMX_ved44u0ojs53zux5xzJhBvd6IcfY2643QqjlYS - Tr9jSyHC3SAbfWSv6j0Sn4SJv_ZPcdXX_PPyqnBIP9x0 - _cSaYwi_el6YZ55oo0eDXSW_awgI - m2nVNy5qVeQ5EBd5HSWwJu25NxyL2B7q1I053njRaOy095W92w4X3kKkNv1ogE9F5iVX516DphairpeVit9__F2VpDeYe9znJ - erjr8u_8UZCmB3JYpvIlr6h585IUKmrDYj6EoC7gutDwFMTuPR9B2Ujwfjb9kXjuwDJVF8q_o3o - 9qGliWcAvUfYNCF8_DDOnTm4PFUuZcq58TeaVNY6R2n3q5ot4UgYv7f0tBvRED5RypD3NrFB0wnJtci6ewpseLbpRENrEKd6UuvPmm6H0_RQeXsKFyIZCwtzfLKt7ttY17 - w9fHMXbgCS3CHS3YQLL70_4xcGv3L8VrJlaZwvFfzx9ouZmZCaOqXfVCGchasUHTBcmtKQeSgi3jG - TlaaX4AxBklvsUxFFkuXg_--3w8";
                    string token = "Bearer eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsImp0aSI6IjYwYjNjYWRiN2UyOTI2Zjk5NTQ3OGI1MzczM2VlMTEyMGI0ZGQ2MTlhNTg0YTBiMmFlOTVhZDY0NWM4MmZmOTkwMTM2ZmYxNzUzMjBkNzk4In0.eyJhdWQiOiIzMiIsImp0aSI6IjYwYjNjYWRiN2UyOTI2Zjk5NTQ3OGI1MzczM2VlMTEyMGI0ZGQ2MTlhNTg0YTBiMmFlOTVhZDY0NWM4MmZmOTkwMTM2ZmYxNzUzMjBkNzk4IiwiaWF0IjoxNjQ0Mzc1NDQzLCJuYmYiOjE2NDQzNzU0NDMsImV4cCI6MTY3NTkxMTQ0Mywic3ViIjoiNzQiLCJzY29wZXMiOltdfQ.qirRFpq6WgiaC7tU - R49mvNflKMKnBnwe8g5m3BhLH8E47dn - RqO6qQYytBQ6Ik23FN_6x7SJaYcEegEtItSaeeqARU0opzk3gBf3zegLUKqe8gRqdH3ieREaB60Vdawb87OjNfHVDfvwOTuBedxrw1owfnlY - Wypc5q1DLhzPfQ9sWpNvXDi__x2VI_gPIH6f6rygm8ctLhz8IfGUZRRn8Il0E98kD9l4xwWxNyOpcSSqOUGE6USz1KlS - 7x7R - E1OJgODw9D8xKwqThYfiMFBVY1VHp6lnFkTHyj - iPT8n5MEkoohG4l - R5OYStx3MMVCArS24jwcN2eNwLZrFwU - glxtVndieHP - e1f_LEe6CzfxaG2bW - r47vM14tKxLbessRIfQbqLXgvf - PRTzpNg5DnKyFGMe183utgervU0OgUtZhOwaIv9j9SpNbYr5Le7EiQ - 1j_04EZ2wCMP6Hu885iBR87eM8TXx7wUiCREHBG1vDavUmnWvhdanfR5_ZA4fy7PszQPUdOo340lRlf1XCSGX1vC_gl5RQDcaXgudIzjHH7tcozyRVzNqg6pDOBRU7noX3QpTPQbLNN512aDCjf42ZwGC3_zTExt1HV1gN1Jis6jY_70cO0AwUIDkV0LsV0QE4etXtAHQM8FVTXv15C2C9PBfRRx3IBVXyVU";
                    string userDefined = "";
                    var client = new RestClient("https://bppays.com/api/order/confirm");
                    var request = new RestRequest(Method.POST);
                    request.AlwaysMultipartFormData = true;
                    request.AddHeader("Authorization", token);
                    request.AddParameter("merchant_id", paymentMethod.merchant_id, "form-data", ParameterType.GetOrPost);
                    request.AddParameter("order_id", OrderId, "form-data", ParameterType.GetOrPost);
                    request.AddParameter("userDefined", userDefined, "form-data", ParameterType.GetOrPost);
                    try
                    {
                        IRestResponse response = client.Execute(request);
                        dynamic obj = JObject.Parse(response.Content);
                        if(status.ToLower() != "success")
                        {
                            topupOrder.RequestSuccessDate = DateTime.UtcNow;
                            topupOrder.Status = status;
                            topupOrder.PaymentMethod = payment_method;
                            topupOrder.Amount = Convert.ToInt32(amount);
                            topupOrder.Remark = "finished";
                            db.SaveChanges();
                            transaction.Commit();
                            return false;
                        }                       
                    }
                    catch (Exception ex) { return false; }

                    //Call API to check there is success transaction at BP Pay END
                    if ((topupOrder.Amount != Convert.ToInt32(amount)) || (topupOrder.Amount > 50000 || Convert.ToInt32(amount) > 50000))
                    {
                        var entityUser = AccountService.GetEntityUserByUserId(db, topupOrder.UserID.ToString(), false, true);
                        entityUser.IsBan = true;
                        entityUser.ContactEmail = "topupOrder.Amount:" + topupOrder.Amount + ",amount:" + amount + ",TimeBan:" + DateTime.Now + ",OrderId:" + OrderId + "";
                        db.SaveChanges();
                        transaction.Commit();
                        return false;
                    }
                    //Check Hacker Finished
                    else
                    {
                        if (topupOrder.Remark != "finished")
                        {
                            topupOrder.RequestSuccessDate = DateTime.UtcNow;
                            topupOrder.Status = status;
                            topupOrder.PaymentMethod = payment_method;
                            topupOrder.Amount = Convert.ToInt32(amount);
                            topupOrder.Remark = "finished";
                            int userid = topupOrder.UserID;
                            int lastTopupRequestId = topupOrder.ID;
                            Entity.PointBalance pBalance = db.PointBalances.FirstOrDefault(e => e.UserID == userid);
                            try
                            {
                                if (status.ToLower() == "success")
                                {
                                    //Everything is ok add purchased points for user

                                    if (pBalance == null)
                                    {
                                        Entity.PointBalance newPbal = new Entity.PointBalance();
                                        //Create new record
                                        newPbal.UserID = userid;
                                        newPbal.Balance = Convert.ToInt32(amount);
                                        newPbal.CreditBalance = 0;
                                        newPbal.LastTopupRequestID = lastTopupRequestId;
                                        newPbal.UpdateDate = DateTime.UtcNow;
                                        db.PointBalances.Add(newPbal);
                                    }
                                    else
                                    {
                                        //Update Existing record
                                        pBalance.LastTopupRequestID = lastTopupRequestId;
                                        pBalance.UpdateDate = DateTime.UtcNow;
                                        pBalance.Balance = Convert.ToInt32(pBalance.Balance) + (Convert.ToInt32(amount));
                                        pBalance.CreditBalance = 0;
                                    }
                                    db.SaveChanges();
                                    transaction.Commit();
                                    return true;
                                }
                                else
                                {
                                    if (pBalance == null)
                                    {
                                        Entity.PointBalance newPbal = new Entity.PointBalance();
                                        //Create new record
                                        newPbal.UserID = userid;
                                        newPbal.Balance = 0;
                                        newPbal.CreditBalance = 0;
                                        newPbal.LastTopupRequestID = lastTopupRequestId;
                                        newPbal.UpdateDate = DateTime.UtcNow;
                                        db.PointBalances.Add(newPbal);
                                    }
                                    else
                                    {
                                        //Update Existing record
                                        pBalance.LastTopupRequestID = lastTopupRequestId;
                                        pBalance.UpdateDate = DateTime.UtcNow;
                                    }
                                    db.SaveChanges();
                                    transaction.Commit();
                                    return false;
                                }
                            }
                            catch (Exception ex)
                            {
                                throw ex;
                                return false;
                            }
                        }
                        else
                        {
                            return true;
                        }
                    }


                }
            }
        }

        static void SortArrayList ( params string[] strings)
        {
            if(strings == null || strings.Length == 0)
            {

            }
            else
            {
                Array.Sort(strings);
            }
        }
        public static List<CashOutRequestModel> GetCashOutRequests(string accessToken, int skip, int take)
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityUser = AccountService.GetEntityUserByAccessToken(db, accessToken, false, RoleType.Doctor);
                var result = new List<CashOutRequestModel>();
                var entityCashOutRequests = db.CashOutRequests.Where(e => e.UserId == entityUser.UserId).OrderByDescending(e=>e.CreateDate).Skip(skip).Take(take);
                foreach (var entityRequest in entityCashOutRequests)
                {
                    result.Add(new CashOutRequestModel(entityRequest));
                }
                return result;
            }
        }

        public static CashOutRequestModel RequestForCashOut(string accessToken, DoctorBankModel model)
        {
            model.validate();
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                CashOutRequestModel result;
                var entityUser = AccountService.GetEntityUserByAccessToken(db, accessToken, false, RoleType.Doctor);
                entityUser.Doctor.BankId = model.Bank.BankId;
                entityUser.Doctor.AccountHolderName = model.AccountHolderName;
                entityUser.Doctor.AccountNumber = model.AccountNumber;
                db.SaveChanges();
                var entityPayments = db.PaymentRequests.Where(e => e.PaymentStatus == PaymentStatus.Paid && e.ChatRoom.DoctorId == entityUser.UserId && e.CashOutRequestId == null);                
                var totalAmount=entityPayments.Select(e=>e.HCPAmount).DefaultIfEmpty(0).Sum();
                if (totalAmount < 50)
                {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidAction, Payments.ErrorCashOutLimitNotReached));
                }
                using (var transaction = db.Database.BeginTransaction())
                {
                    var entityCashOutRequest = db.CashOutRequests.Create();
                    entityCashOutRequest.CashOutRequestStatus = CashOutRequestStatus.Requested;
                    entityCashOutRequest.CreateDate = DateTime.UtcNow;
                    entityCashOutRequest.UserId = entityUser.UserId;
                    entityCashOutRequest.Amount = totalAmount;
                    db.CashOutRequests.Add(entityCashOutRequest);
                    foreach (var entitypayment in entityPayments)
                    {
                        entitypayment.CashOutRequestId = entityCashOutRequest.CashOutRequestId;
                    }
                    db.SaveChanges();
                    result = new CashOutRequestModel(entityCashOutRequest);
                    transaction.Commit();
                }
                return result;
            }
        }

        public static List<BankModel> GetBankList(string accessToken)
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                List<BankModel> result = new List<BankModel>();
                var entityUser = AccountService.GetEntityUserByAccessToken(db, accessToken, false, RoleType.Doctor);
                var entityBanks = db.Banks.OrderBy(e => e.BankName);
                foreach (var entityBank in entityBanks)
                {
                    result.Add(new BankModel(entityBank));
                }
                return result;
            }
        }

        public static List<DoctorBankModel> GetRegisteredBanks(string accessToken)
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                List<DoctorBankModel> result = new List<DoctorBankModel>();
                var entityUser = AccountService.GetEntityUserByAccessToken(db, accessToken, false, RoleType.Doctor);
                if (entityUser.Doctor.Bank != null)
                {
                    result.Add(new DoctorBankModel(entityUser.Doctor));
                }
                return result;
            }
        }

        public static PromoCodeModel ValidatePromoCode(string accessToken, string promoCode, int doctorId)
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityUser = AccountService.GetEntityUserByAccessToken(db, accessToken, false);
                var entityTargetUser = AccountService.GetEntityTargetUserByUserId(db, doctorId, false);
                if (entityTargetUser.Doctor == null)
                {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidArguments, HCPPatient.ErrorDoctorNotFound));
                }
                var entityPromocode = _VerifyPromoCodePartial(promoCode, db, entityUser,entityTargetUser);

                decimal amount = entityTargetUser.Doctor.Category.CategoryPrice;
                decimal discount = 0;
                var malaysianTime = DateTime.UtcNow.AddHours(8);
                if (malaysianTime.Hour >= 0 && malaysianTime.Hour < 8)
                {
                    amount = entityTargetUser.Doctor.Category.MidNightPrice;
                }

                var actualAmount = amount;
                if (entityPromocode != null)
                {
                    if (entityPromocode.DiscountType == PromoDiscountType.Amount)
                    {
                        amount = amount - entityPromocode.Discount;
                        discount = entityPromocode.Discount;
                    }
                    else
                    {
                        discount = Math.Round(((amount * entityPromocode.Discount) / 100), 2);
                        amount = amount - discount;
                    }
                }
                amount = amount < 0 ? 0 : amount;
                return new PromoCodeModel(entityPromocode,actualAmount,discount,amount);

            }
        }

        internal static Entity.PromoCode _VerifyPromoCodePartial(string promoCode, Entity.db_HeyDocEntities db, Entity.UserProfile entityUser, Entity.UserProfile entityDoctor)
        {
            DateTime malaysianDate = DateTime.UtcNow.AddHours(8);
            var entityPromocode = db.PromoCodes.FirstOrDefault(e => e.PromoCode1 == promoCode && !e.IsDelete && e.PromoStatus == PromoStatus.Active);
            if (entityPromocode == null)
            {
                throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidPromoCode, Promo.ErrorInvalidOrExpired));
            }
            if (entityPromocode.CategoryId.HasValue && entityPromocode.CategoryId.Value != entityDoctor.Doctor.CategoryId)
            {
                throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidPromoCode, Promo.ErrorMismatched));
            }
            if (entityPromocode.DoctorId.HasValue && entityPromocode.DoctorId.Value != entityDoctor.UserId)
            {
                throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidPromoCode, Promo.ErrorMismatched));
            }

            if (entityPromocode.EndDate.HasValue && entityPromocode.EndDate.Value < malaysianDate)
            {
                entityPromocode.PromoStatus = PromoStatus.Expired;
                db.SaveChanges();
                throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidPromoCode, Promo.ErrorExpired));
            }
            if (entityPromocode.StartDate > malaysianDate)
            {
                throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidPromoCode, Promo.ErrorInvalid));
            }
            if (entityPromocode.MaxUserLimit.HasValue)
            {
                var totalUsage = db.PaymentRequests.Count(e => e.PromoCodeId == entityPromocode.PromoCodeId && e.PaymentStatus != PaymentStatus.Canceled);
                if (totalUsage >= entityPromocode.MaxUserLimit.Value)
                {
                    entityPromocode.PromoStatus = PromoStatus.MaxUserLimitReached;
                    db.SaveChanges();
                    throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidPromoCode, Promo.ErrorLimitReached));
                }
            }
            if (entityPromocode.UserUsageLimit.HasValue)
            {
                var totalUsage = db.PaymentRequests.Count(e => e.PromoCodeId == entityPromocode.PromoCodeId && e.ChatRoom.PatientId == entityUser.UserId && e.PaymentStatus != PaymentStatus.Canceled);
                if (totalUsage >= entityPromocode.UserUsageLimit.Value)
                {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidPromoCode, Promo.ErrorLimitReachedForUser));
                }
            }
            return entityPromocode;
        }

        internal static int getDoctorConsultPrice(Entity.db_HeyDocEntities db, int doctorID)
        {
            var query = from category in db.Categories
                        join doctor in db.Doctors on category.CategoryId equals doctor.CategoryId
                        where doctor.UserId == doctorID
                        select category.CategoryPrice;
            int consultPrice = Convert.ToInt32(query.FirstOrDefault());
            return consultPrice;
        }
       
        public static void GenerateOrEditCorporatePrescriptionPaymentRequest(Entity.db_HeyDocEntities db, long prescriptionId, decimal? consultationFees, decimal? medicationFees, decimal? deliveryFees)
        {
            var entityPrescription = db.Prescriptions.FirstOrDefault(p => p.PrescriptionId == prescriptionId);
            if (entityPrescription == null)
            {
                throw new WebApiException(new WebApiError(WebApiErrorCode.NotFound));
            }
            if (entityPrescription.ChatRoom == null)
            {
                throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidAction, "Only prescriptions created by doctors can include payment amount."));
            }
            if (entityPrescription.PaymentRequest == null && (!consultationFees.HasValue || !medicationFees.HasValue || !deliveryFees.HasValue))
            {
                throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidAction, "All amounts are required to generate a payment record for prescriptions."));
            }

            decimal cutPercent = 0;
            decimal platformAmount = 0;
            decimal hcpAmount = consultationFees.Value;
            Entity.ChatRoom entityChatroom = entityPrescription.ChatRoom;
            if (consultationFees.HasValue)
            {
                cutPercent = 0;
                if (entityChatroom.Doctor.Group != null)
                {
                    cutPercent = entityChatroom.Doctor.Group.PlatformCut;
                }
                else
                {
                    var entityCutPercent = db.PlatformPercents.OrderByDescending(e => e.CreateDate).FirstOrDefault();
                    cutPercent = entityCutPercent != null ? entityCutPercent.CutPercent : 0;
                }

                platformAmount = Math.Round(((consultationFees.Value * cutPercent) / 100), 2);
                hcpAmount = consultationFees.Value - platformAmount;
            }

            if (entityPrescription.PaymentRequest == null)
            {
                var entityPaymentRequest = db.PaymentRequests.OrderByDescending(r => r.CreateDate).FirstOrDefault(r => r.CreateDate <= entityPrescription.CreateDate && r.ChatRoomId == entityPrescription.ChatRoomId && (r.PaymentStatus == PaymentStatus.Authorised || r.PaymentStatus == PaymentStatus.Paid));

                if (entityPaymentRequest == null || entityPaymentRequest.Amount > 0)
                {
                    entityPaymentRequest = db.PaymentRequests.Add(new Entity.PaymentRequest
                    {
                        Amount = consultationFees.Value,
                        MedicationFeesAmount = medicationFees.Value,
                        DeliveryFeesAmount = deliveryFees.Value,
                        ChatRoomId = entityPrescription.ChatRoomId.Value,
                        CreateDate = DateTime.UtcNow,
                        PaymentStatus = PaymentStatus.Paid,
                        CutPercent = cutPercent,
                        PlatformAmount = platformAmount,
                        HCPAmount = hcpAmount,
                        ActualAmount = consultationFees.Value,
                        BrainTreeTransactionId = "",
                        BrainTreeTransactionStatus = "",
                        BrainTreeTransactionType = ""
                    });

                    var patientProfile = entityChatroom.Patient.UserProfile;
                    if (patientProfile.CorporateId != null)
                    {
                        entityPaymentRequest.UserCorporateId = patientProfile.CorporateId;
                        entityPaymentRequest.CorporateUserType = patientProfile.UserCorperates.FirstOrDefault().CorporateUserType;
                    }
                }
                else
                {
                    entityPaymentRequest.Amount = consultationFees.Value;
                    entityPaymentRequest.MedicationFeesAmount = medicationFees.Value;
                    entityPaymentRequest.DeliveryFeesAmount = deliveryFees.Value;
                    entityPaymentRequest.PlatformAmount = platformAmount;
                    entityPaymentRequest.HCPAmount = hcpAmount;
                    entityPaymentRequest.ActualAmount = consultationFees.Value;
                    entityPaymentRequest.PaymentStatus = PaymentStatus.Paid;
                }

                entityPrescription.PaymentRequestId = entityPaymentRequest.PaymentRequestId;
            }
            else
            {
                var entityPaymentRequest = entityPrescription.PaymentRequest;
                if (consultationFees.HasValue)
                {
                    entityPaymentRequest.Amount = consultationFees.Value;
                    entityPaymentRequest.PlatformAmount = platformAmount;
                    entityPaymentRequest.HCPAmount = hcpAmount;
                    entityPaymentRequest.ActualAmount = consultationFees.Value;
                }
                if (medicationFees.HasValue)
                {
                    entityPaymentRequest.MedicationFeesAmount = medicationFees.Value;
                }
                if (deliveryFees.HasValue)
                {
                    entityPaymentRequest.DeliveryFeesAmount = deliveryFees.Value;
                }
            }

            db.SaveChanges();
        }
    }
}