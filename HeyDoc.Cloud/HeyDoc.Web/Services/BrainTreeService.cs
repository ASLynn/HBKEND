using Braintree;
using HeyDoc.Web.Helpers;
using HeyDoc.Web.Lib;
using HeyDoc.Web.Models;
using HeyDoc.Web.Resources;
using HeyDoc.Web.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace HeyDoc.Web.Services
{
    public class BraintreeService
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public static BrainTreeConfiguration config = new BrainTreeConfiguration();
        private static readonly IBraintreeGateway gateway = config.CreateGateway();

        public static StringResult GetClientToken(string accessToken)
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityUser = AccountService.GetEntityUserByAccessToken(db, accessToken, false);
                if (entityUser.Role != RoleType.User)
                {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidAction, HCPPatient.ErrorOnlyPatientCanAddPayment));
                }

                var clientToken = _GetClientToken(db, entityUser);

                return new StringResult(clientToken);
            }
        }

        private static string _GetClientToken(Entity.db_HeyDocEntities db, Entity.UserProfile entityUser)
        {
            if (string.IsNullOrEmpty(entityUser.Patient.BrainTreeUserId))
            {
                var request = new CustomerRequest()
                {
                    FirstName = entityUser.FullName,
                    Email = entityUser.UserName,
#if DEBUG
                    Id = "Staging" + entityUser.UserId.ToString("D8"),
#else
                    Id = "U" + entityUser.UserId.ToString("D8"),
#endif
                };

                Result<Customer> result = gateway.Customer.Create(request);

                bool success = result.IsSuccess();
                // true
                string customerId = result.Target.Id;

                entityUser.Patient.BrainTreeUserId = customerId;
                db.SaveChanges();
            }
            var clientToken = gateway.ClientToken.generate(new ClientTokenRequest { CustomerId = entityUser.Patient.BrainTreeUserId });
            return clientToken;
        }

        public static BoolResult AddPaymentMethod(string accessToken, BrainTreePaymentModel model)
        {
            if (model == null && string.IsNullOrEmpty(model.PaymentMethodNonce))
            {
                throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidArguments, Payments.ErrorPaymentModelInvalid));
            }
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityUser = AccountService.GetEntityUserByAccessToken(db, accessToken, false);
                if (entityUser.Role != RoleType.User)
                {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidAction, HCPPatient.ErrorOnlyPatientCanAddPayment));
                }
                var clientToken = _GetClientToken(db, entityUser);
                var result = _AddPaymentMethodPartial(entityUser, model.PaymentMethodNonce);

                return new BoolResult(true);
            }
        }

        internal static string _AddPaymentMethodPartial(Entity.UserProfile entityUser, string paymentMethodNonce)
        {
            if (string.IsNullOrEmpty(entityUser.Patient.BrainTreeUserId))
            {
                throw new WebApiException(
                    new WebApiError(WebApiErrorCode.InvalidArguments, Payments.ErrorTokenMissing));
            }

            var paymentMethodOptions = new CreditCardOptionsRequest()
            {
                MakeDefault = true,
                VerifyCard = true,
            };

            var request = new CreditCardRequest
            {
                CustomerId = entityUser.Patient.BrainTreeUserId,
                PaymentMethodNonce = paymentMethodNonce,
                Options = paymentMethodOptions
            };

            Result<Braintree.CreditCard> result = gateway.CreditCard.Create(request);
            if (!result.IsSuccess())
            {
                string errorMessage = string.Format("[{0}] {1}", "Vault_Error", result.Message);

                throw new WebApiException(new WebApiError(WebApiErrorCode.BrainTreeError, errorMessage));
            }
            else
            {
                Braintree.CreditCard paymentMethod = result.Target;
                if (paymentMethod.Debit == CreditCardDebit.YES)
                {
                    var clientToken = GetDefaultPaymentToken(entityUser.Patient.BrainTreeUserId);
                    var deletedResult = gateway.PaymentMethod.Delete(clientToken);
                    throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidArguments, Payments.ErrorDebitCardUnsupported));
                }
                var token = paymentMethod.Token;

                return token;
            }
        }

        public static bool trans(string accessToken)
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityUser = AccountService.GetEntityUserByAccessToken(db, accessToken, false);
                var paymentMethodToken = GetDefaultPaymentToken(entityUser.Patient.BrainTreeUserId);
                return _CreateTransaction(15, paymentMethodToken, entityUser.Patient.BrainTreeUserId);
            }
        }

        public static bool MakeTransaction(Entity.db_HeyDocEntities db, Entity.ChatRoom entityChatRoom)
        {
            bool returnValue = true;
            var entityRequest = entityChatRoom.PaymentRequests.Where(e => e.PaymentStatus == PaymentStatus.Authorised).OrderByDescending(e => e.CreateDate).FirstOrDefault();
            if (entityRequest == null)
            {
                return returnValue;
            }
            string msg = "";
            if (!string.IsNullOrEmpty(entityRequest.BrainTreeTransactionId))
            {
                Result<Transaction> result = gateway.Transaction.SubmitForSettlement(entityRequest.BrainTreeTransactionId);

                if (result.IsSuccess())
                {
                    Transaction settledTransaction = result.Target;
                    entityRequest.BrainTreeTransactionStatus = settledTransaction.Status.ToString();
                    entityRequest.BrainTreeTransactionId = settledTransaction.Id;
                    entityRequest.PaymentStatus = PaymentStatus.Paid;
                }
                else
                {
                    entityRequest.BrainTreeTransactionStatus = result.Message.Truncate(49);
                    entityRequest.PaymentStatus = PaymentStatus.Failed;
                    returnValue = false;
                }
                msg = result.Message;
            }
            else
            {
                entityRequest.PaymentStatus = PaymentStatus.Paid;
                entityRequest.PlatformAmount = 0;
            }

            try
            {
                string subject = "HOPE: Transaction Summary";
                string content = EmailHelper.ConvertEmailHtmlToString(@"Emails\TransactionEmail.html");
                var discount = entityRequest.ActualAmount - entityRequest.Amount;
                content = string.Format(content,
                                        entityChatRoom.PatientId, //{0}
                                        entityChatRoom.Patient.UserProfile.FullName,//{1} 
                                        entityChatRoom.Patient.UserProfile.UserName, //{2}
                                        entityRequest.Amount, //{3}
                                        DateTime.UtcNow.AddHours(8).ToString("dd-MMM-yyy hh:mm tt"),//{4}
                                        entityChatRoom.DoctorId, //{5}
                                        entityChatRoom.Doctor.UserProfile.FullName, //{6}
                                        entityRequest.BrainTreeTransactionId,//{7}
                                        entityRequest.PaymentStatus.ToString(), //{8}
                                        msg, //{9}
                                        entityRequest.ActualAmount, //{10}
                                        (discount < 0) ? 0 : discount, //{11}
                                        (entityRequest.PromoCode != null ? entityRequest.PromoCode.PromoCode1 : "")); //{12}

                // TODO M UNBLANK: Support email as recepient
                EmailHelper.SendViaSendGrid(new List<string>() { "" }, subject, content, string.Empty);
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
            //db.SaveChanges();

            //if (entityChatRoom.RequestStatus != RequestStatus.Canceled && entityChatRoom.RequestStatus != RequestStatus.Rejected)
            //{
            string paymentNum = "xxxxxx";
            if (!string.IsNullOrEmpty(entityChatRoom.Patient.BrainTreeUserId))
            {
                Customer customer = gateway.Customer.Find(entityChatRoom.Patient.BrainTreeUserId);
                if (customer != null)
                {
                    var chosenPayment = _GetDefaultPaymentMethod(customer);
                    if (chosenPayment != null)
                    {
                        paymentNum = chosenPayment.MaskedNumber.ToString().Substring(Math.Max(0, chosenPayment.MaskedNumber.Length - 4));
                    }
                }

            }

            if (entityRequest.Amount > 0)
            {
                string startDate = "Data not available";
                string endDate = "Data not available";
                var startChat = ChatService.GetChatResponse(db, entityChatRoom.ChatRoomId, RequestStatus.Accepted);
                if (startChat != null)
                {
                    startDate = startChat.CreatedDate.AddHours(8).ToString("dd-MMM-yyy hh:mm tt");
                }
                var endChat = ChatService.GetChatResponse(db, entityChatRoom.ChatRoomId, RequestStatus.Completed);
                if (endChat != null)
                {
                    endDate = endChat.CreatedDate.AddHours(8).ToString("dd-MMM-yyy hh:mm tt");
                }
                var receiptSubject = "HOPE: Chat Receipt";
                var receiptContent = EmailHelper.ConvertEmailHtmlToString(@"Emails\ChatReceipt.html");
                var total = entityRequest.ActualAmount.ToString();
                var promoDiscount = entityRequest.ActualAmount - entityRequest.Amount;
                promoDiscount = promoDiscount < 0 ? 0 : promoDiscount;
                receiptContent = string.Format(receiptContent, entityChatRoom.Patient.UserProfile.FullName,
                                                                entityRequest.Amount.ToString(),
                                                                DateTime.UtcNow.AddHours(8).ToString("dd-MMM-yyyy"),
                                                                entityChatRoom.Doctor.UserProfile.FullName,
                                                                entityChatRoom.Patient.UserProfile.FullName,
                                                                entityRequest.BrainTreeTransactionId,
                                                                total, entityRequest.Amount.ToString(),
                                                                paymentNum, startDate, endDate, promoDiscount.ToString());
                try
                {
                    EmailHelper.SendViaSendGrid(new List<string>() { entityChatRoom.Patient.UserProfile.ContactEmail ?? entityChatRoom.Patient.UserProfile.UserName }, receiptSubject, receiptContent, string.Empty);
                }
                catch (Exception ex)
                {
                    log.Error(ex);
                }
            }
            //}
            return returnValue;
        }

        internal static bool _CreateTransaction(decimal amount, string paymentMethodToken, string customerId)
        {
            if (amount <= 0)
            {
                return true;
            }
            var request = new TransactionRequest
            {
                Amount = amount,
                PaymentMethodToken = paymentMethodToken,
                CustomerId = customerId,
                Options = new TransactionOptionsRequest
                {
                    SubmitForSettlement = true
                }
            };

            Result<Transaction> result = gateway.Transaction.Sale(request);

            if (result.IsSuccess())
            {
                Transaction transaction = result.Target;

                return true;
            }
            else
            {
                string errorMessage = string.Format("[{0}] {1}", "Vault_Error", result.Message);

                throw new WebApiException(new WebApiError(WebApiErrorCode.BrainTreeError, errorMessage));
            }
        }

        internal static string GetDefaultPaymentToken(string brainTreeUserId)
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                Customer customer = gateway.Customer.Find(brainTreeUserId);

                string token = "";
                foreach (var paymentMethod in customer.PaymentMethods)
                {
                    if (paymentMethod.IsDefault.HasValue && paymentMethod.IsDefault.Value && !string.IsNullOrEmpty(paymentMethod.Token))
                    {
                        token = paymentMethod.Token;
                    }
                }
                return token;
            }
        }

        internal static bool GeneratePaymentRequest(Entity.db_HeyDocEntities db, Entity.Device entityDevice, Entity.ChatRoom entityChatRoom, Entity.PromoCode entityPromoCode)
        {
            decimal amount = entityChatRoom.Doctor.Category.CategoryPrice;
            // TODO M: Set timing
            var malaysianTime = DateTime.UtcNow.AddHours(8);
            if (malaysianTime.Hour >= 0 && malaysianTime.Hour < 8)
            {
                amount = entityChatRoom.Doctor.Category.MidNightPrice;
            }

            decimal cutPercent = 0;
            if (entityChatRoom.Doctor.Group != null)
            {
                cutPercent = entityChatRoom.Doctor.Group.PlatformCut;
            }
            else
            {
                var entityCutPercent = db.PlatformPercents.OrderByDescending(e => e.CreateDate).FirstOrDefault();
                cutPercent = entityCutPercent != null ? entityCutPercent.CutPercent : 0;
            }

            var platformAmount = Math.Round(((amount * cutPercent) / 100), 2);
            var hcpAmount = amount - platformAmount;
            var actualAmount = amount;
            if (entityPromoCode != null)
            {
                if (entityPromoCode.DiscountType == PromoDiscountType.Amount)
                {
                    amount = amount - entityPromoCode.Discount;
                }
                else
                {
                    decimal discount = Math.Round(((amount * entityPromoCode.Discount) / 100), 2);
                    amount = amount - discount;
                }
                amount = amount < 0 ? 0 : amount;
                platformAmount = (amount > hcpAmount) ? (amount - hcpAmount) : 0;

            }

            var isCorporateUser = db.UserCorperates.Any(e => e.UserId == entityChatRoom.PatientId);
            if (isCorporateUser)
            {
                amount = 0;
            }

            if (amount > 0)
            {
                if (string.IsNullOrEmpty(entityChatRoom.Patient.BrainTreeUserId))
                {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidArguments, Payments.ErrorTokenMissing));
                }

                Customer customer = gateway.Customer.Find(entityChatRoom.Patient.BrainTreeUserId);

                if (customer.PaymentMethods.Count() <= 0)
                {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidArguments, Payments.ErrorNoPaymentMethod));
                }
                var chosenPayment = _GetDefaultPaymentMethod(customer);

                if (chosenPayment.UserCardType == Web.UserCardType.Debit)
                {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidArguments, Payments.ErrorDebitCardUnsupported));
                }
            }

            var entityPaymentRequest = db.PaymentRequests.Create();
            entityPaymentRequest.Amount = amount;
            entityPaymentRequest.BrainTreeTransactionId = "";
            entityPaymentRequest.BrainTreeTransactionStatus = "";
            entityPaymentRequest.BrainTreeTransactionType = "";
            entityPaymentRequest.ChatRoomId = entityChatRoom.ChatRoomId;
            entityPaymentRequest.CreateDate = DateTime.UtcNow;
            entityPaymentRequest.PaymentStatus = PaymentStatus.Requested;
            entityPaymentRequest.CutPercent = cutPercent;
            entityPaymentRequest.PlatformAmount = platformAmount;
            entityPaymentRequest.HCPAmount = hcpAmount;
            entityPaymentRequest.ActualAmount = actualAmount;
            if (entityPromoCode != null)
            {
                entityPaymentRequest.PromoCodeId = entityPromoCode.PromoCodeId;
            }
            var patientProfile = entityChatRoom.Patient.UserProfile;
            if (patientProfile.CorporateId != null)
            {
                entityPaymentRequest.UserCorporateId = patientProfile.CorporateId;
                entityPaymentRequest.CorporateUserType = patientProfile.UserCorperates.FirstOrDefault().CorporateUserType;
            }
            db.PaymentRequests.Add(entityPaymentRequest);
            db.SaveChanges();
            return true;
        }

        internal static async Task<bool> AuthorizePayment(Entity.db_HeyDocEntities db, Entity.ChatRoom entityChatRoom)
        {

            var entityPaymentRequest = db.PaymentRequests.Where(e => e.ChatRoomId == entityChatRoom.ChatRoomId && e.PaymentStatus == PaymentStatus.Requested).OrderByDescending(e => e.CreateDate).FirstOrDefault();
            if (entityPaymentRequest == null)
            {
                entityChatRoom.RequestStatus = RequestStatus.Canceled;
                ChatService.CreateChatResponse(db, entityChatRoom.ChatRoomId, RequestStatus.Canceled);
                throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidArguments, Payments.ErrorPaymentRequestNotFound));
            }

            TransactionRequest request = null;


            if (entityPaymentRequest.Amount <= 0)
            {
                entityPaymentRequest.PaymentStatus = PaymentStatus.Authorised;
                return true;
            }

            if (string.IsNullOrEmpty(entityChatRoom.Patient.BrainTreeUserId))
            {
                throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidArguments, Payments.ErrorTokenMissing));
            }

            Customer customer = gateway.Customer.Find(entityChatRoom.Patient.BrainTreeUserId);

            if (customer == null || customer.PaymentMethods == null || customer.PaymentMethods.Count() <= 0)
            {
                throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidArguments, Payments.ErrorNoPaymentMethod));
            }

            var chosenPayment = _GetDefaultPaymentMethod(customer);

            if (chosenPayment.UserCardType == Web.UserCardType.Debit)
            {
                entityChatRoom.RequestStatus = RequestStatus.Canceled;
                entityPaymentRequest.PaymentStatus = PaymentStatus.Failed;
                ChatService.CreateChatResponse(db, entityChatRoom.ChatRoomId, RequestStatus.Canceled);
                await NotificationService.NotifyUser(db, entityChatRoom.PatientId, PnActionType.ChatRejected, entityChatRoom.ChatRoomId, Payments.ErrorDebitCardUnsupported);
                throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidArguments, Payments.ErrorPaymentFailed));
            }
            else
            {
                request = new TransactionRequest
                {
                    Amount = entityPaymentRequest.Amount,
                    CustomerId = entityChatRoom.Patient.BrainTreeUserId,
                    //PaymentMethodToken = chosenPayment.PaymentMethodNonce,
                    Options = new TransactionOptionsRequest()
                    {
                        SubmitForSettlement = false
                    }
                };
            }

            Result<Transaction> result = gateway.Transaction.Sale(request);


            if (!result.IsSuccess())
            {
                entityChatRoom.RequestStatus = RequestStatus.Canceled;
                entityPaymentRequest.PaymentStatus = PaymentStatus.Failed;
                ChatService.CreateChatResponse(db, entityChatRoom.ChatRoomId, RequestStatus.Canceled);
                var msg = ProcessUnsuccessfulTransaction(result);
                await NotificationService.NotifyUser(db, entityChatRoom.PatientId, PnActionType.ChatRejected, entityChatRoom.ChatRoomId, "Your chat request cannot be accepted, Error with payment! " + msg);
                throw new WebApiException(new WebApiError(WebApiErrorCode.BrainTreeError, Payments.ErrorPaymentFailed));
            }
            else
            {
                Transaction transaction = result.Target;
                entityPaymentRequest.BrainTreeTransactionId = transaction.Id;
                entityPaymentRequest.BrainTreeTransactionStatus = transaction.Status.ToString();
                entityPaymentRequest.BrainTreeTransactionType = transaction.Type.ToString();
                entityPaymentRequest.ChatRoomId = entityChatRoom.ChatRoomId;
                entityPaymentRequest.PaymentStatus = PaymentStatus.Authorised;
                db.SaveChanges();
            }

            return true;
        }

        private static string ProcessUnsuccessfulTransaction(Result<Transaction> result)
        {
            string errorMessage = null;
            if (result.Transaction != null)
            {
                Transaction transaction = result.Transaction;

                if (transaction.Status == TransactionStatus.PROCESSOR_DECLINED)
                {
                    errorMessage = string.Format("[{0}] {1}", transaction.ProcessorResponseCode, transaction.ProcessorResponseText);
                }
                if (transaction.Status == TransactionStatus.SETTLEMENT_DECLINED)
                {
                    errorMessage = string.Format("[{0}] {1}", transaction.ProcessorSettlementResponseCode, transaction.ProcessorSettlementResponseText);
                }
                if (transaction.Status == TransactionStatus.GATEWAY_REJECTED)
                {
                    errorMessage = string.Format("[{0}] {1}", "Gateway_Rejected", transaction.GatewayRejectionReason);
                }
            }
            else
            {
                errorMessage = string.Format("[{0}] {1}", "Validation_Error", result.Message);
            }
            errorMessage = "Customer credit card error (" + errorMessage + ")";

            return errorMessage;
        }

        public static BoolResult DeleteCard(string accessToken)
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityUser = AccountService.GetEntityUserByAccessToken(db, accessToken, false, RoleType.User);
                Result<PaymentMethod> result;
                if (string.IsNullOrEmpty(entityUser.Patient.BrainTreeUserId))
                {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.BrainTreeError, Payments.ErrorNoPaymentMethod));
                }
                try
                {
                    var clientToken = GetDefaultPaymentToken(entityUser.Patient.BrainTreeUserId);
                    result = gateway.PaymentMethod.Delete(clientToken);
                }
                catch
                {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.BrainTreeError, Payments.ErrorGateway));
                }
                if (result.IsSuccess())
                {
                    return new BoolResult(true);
                }
                else
                {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.BrainTreeError, result.Message));
                }
            }
        }

        public static List<CreditCardModel> GetDefaultPaymentMethodDetails(string accessToken)
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityUser = AccountService.GetEntityUserByAccessToken(db, accessToken, false, RoleType.User);
                var result = new List<CreditCardModel>();
                if (string.IsNullOrEmpty(entityUser.Patient.BrainTreeUserId))
                {
                    return result;
                }
                try
                {
                    //var clientToken = GetDefaultPaymentToken(entityUser.Patient.BrainTreeUserId);
                    //var creditCard = gateway.CreditCard.Find(clientToken);
                    Customer customer = gateway.Customer.Find(entityUser.Patient.BrainTreeUserId);
                    foreach (var paymentMethod in customer.PaymentMethods)
                    {
                        string nonce = null;
                        if (paymentMethod.IsDefault.HasValue && paymentMethod.IsDefault.Value)
                        {
                            Result<PaymentMethodNonce> resultNonce = gateway.PaymentMethodNonce.Create(paymentMethod.Token);
                            nonce = resultNonce.Target.Nonce;
                        }
                        var obj = new CreditCardModel((Braintree.CreditCard)paymentMethod, nonce);
                        result.Add(obj);
                    }
                }
                catch
                {
                    //throw new WebApiException(new WebApiError(WebApiErrorCode.BrainTreeError, "Error with payment gateway. Try again later"));
                }
                return result;
            }
        }

        private static CreditCardModel _GetDefaultPaymentMethod(Customer customer)
        {
            var paymentMethod = customer.PaymentMethods.FirstOrDefault(e => e.IsDefault.Value);

            if (paymentMethod is Braintree.CreditCard)
            {
                Result<PaymentMethodNonce> result = gateway.PaymentMethodNonce.Create(paymentMethod.Token);
                string nonce = result.Target.Nonce;

                return new CreditCardModel((Braintree.CreditCard)paymentMethod, nonce);
            }
            return null;
        }
    }
}