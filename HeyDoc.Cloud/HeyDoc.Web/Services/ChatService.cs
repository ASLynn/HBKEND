using HeyDoc.Web.Helpers;
using HeyDoc.Web.Lib;
using HeyDoc.Web.Models;
using HeyDoc.Web.WebApi;
using log4net;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Web;
using System.Data.Entity;
using HeyDoc.Web.Resources;
using DocumentFormat.OpenXml.Spreadsheet;
using PusherServer;

namespace HeyDoc.Web.Services
{
    public class ChatService
    {
        private static readonly ILog logger =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static async Task<ChatModel> ChattingPWA(string accessToken, int userId, int chatRoomId, MessageType msgType, string message, HttpPostedFile file, TimeSpan? voiceDuration = null)
        {
            ActivityAuditHelper.AddRequestParamsToLog();
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                DateTime now = DateTime.UtcNow;
                var entityUser = AccountService.GetEntityUserByAccessToken(db, accessToken, false);
                var entityTargetUser = AccountService.GetEntityTargetUserByUserId(db, userId, false);
                var entityChatRoom = db.ChatRooms.FirstOrDefault(e => ((e.PatientId == entityUser.UserId && e.DoctorId == entityTargetUser.UserId)
                                                                   || (e.PatientId == entityTargetUser.UserId && e.DoctorId == entityUser.UserId)) && e.ChatRoomId == chatRoomId && !e.IsDelete);

                if (entityChatRoom == null)
                {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.NotFound, Chat.ErrorRoomNotFound));
                }
                if (entityChatRoom.RequestStatus == RequestStatus.Completed)
                {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.Unauthorized, Chat.ErrorSessionAlreadyEnded));
                }
                if (entityChatRoom.RequestStatus != RequestStatus.Accepted)
                {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.Unauthorized, Chat.ErrorSessionNotActive));
                }

                var entityChat = db.Chats.Create();
                entityChat.ChatRoomId = entityChatRoom.ChatRoomId;
                entityChat.MessageType = msgType;
                entityChat.CreateDate = now;
                entityChat.FromUserId = entityUser.UserId;
                entityChat.ToUserId = entityTargetUser.UserId;

                if (msgType == MessageType.Message)
                {
                    entityChat.Message = message.Trim();
                }
                else
                {
                    // Check if the request contains multipart/form-data.
                    if (file == null)
                    {
                        throw new WebApiException(
                               new WebApiError(WebApiErrorCode.InvalidAction, Chat.ErrorNoFileReceived));
                    }

                    string containerName = "s" + entityUser.UserId.ToString("D5");

                    if (msgType == MessageType.Photo)
                    {
                        var entityPhoto = PhotoHelper.UploadImageWithThumbnail(db, containerName, Guid.NewGuid().ToString(), file.InputStream);

                        entityChat.PhotoId = entityPhoto.PhotoId;
                    }
                    else
                    {
                        StringBuilder filenameWithExtension = new StringBuilder();
                        filenameWithExtension.Append(Guid.NewGuid().ToString());
                        var extensionArray = file.FileName.Split('.');
                        if (extensionArray != null && extensionArray.Length == 2)
                        {
                            filenameWithExtension.Append(".");
                            filenameWithExtension.Append(extensionArray[1]);
                        }

                        var entityVoice = VoiceHelper.UploadFile(db, containerName, filenameWithExtension.ToString(), file.InputStream);

                        if (voiceDuration.HasValue)
                        {
                            entityChat.VoiceDuration = voiceDuration;
                        }

                        entityChat.VoiceId = entityVoice.VoiceId;
                    }
                }
                db.Chats.Add(entityChat);

                //update last job date of doctor
                var entityUpdateDoc = db.Doctors.FirstOrDefault(e => e.UserId == entityUser.UserId || e.UserId == entityTargetUser.UserId);
                if (entityUpdateDoc != null)
                {
                    entityUpdateDoc.LastJobDate = DateTime.UtcNow;
                }
                entityChatRoom.LastUpdatedDate = now;
                db.SaveChanges();

                //Replied all message
                db.SP_RepliedAllMessage(entityUser.UserId, chatRoomId);


                // if (db.PatientPackages.Count(e=>!e.IsExpired &&(e.PackageId==2||e.PackageId==4)&&e.UserId==entityUser.UserId)==0)
                // if (isPremium)
                //  {
                if (entityTargetUser.Role == RoleType.Doctor)
                {
                    await PushToUsers(db, entityUser.FullName, entityTargetUser.UserId, PnActionType.Chat, entityChatRoom.ChatRoomId, entityChat.Message, entityChat.MessageType, entityUser.UserId);
                }
                else if (entityTargetUser.Role == RoleType.User)
                {
                    await PushToUsers(db, entityUser.FullName, entityTargetUser.UserId, PnActionType.Chat, entityChatRoom.ChatRoomId, entityChat.Message, entityChat.MessageType, entityUser.UserId);
                }
                //}
                //else
                //{


                //    if (entityTargetUser.Role == RoleType.Doctor && entityTargetUser.IsOnline)
                //    {
                //        PushToUsers(db, entityUser.Nickname, entityTargetUser.UserId, PnActionType.Chat, entityChatRoom.ChatRoomId, entityChat.Message, entityChat.MessageType, PackageType.Free, entityUser.UserId);


                //    }
                //    else if (entityTargetUser.Role == RoleType.User && entityTargetUser.IsOnline)
                //    {
                //        PushToUsers(db, entityUser.Nickname, entityTargetUser.UserId, PnActionType.Chat, entityChatRoom.ChatRoomId, entityChat.Message, entityChat.MessageType, PackageType.Free, entityUser.UserId);


                //    }

                //}
                return new ChatModel(entityChat);
            }
        }

        public static async Task<ChatModel> Chatting(string accessToken, int userId, int chatRoomId, MessageType msgType, string message, HttpPostedFile file, TimeSpan? voiceDuration = null)
        {
            ActivityAuditHelper.AddRequestParamsToLog();
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                DateTime now = DateTime.UtcNow;
                var entityUser = AccountService.GetEntityUserByAccessToken(db, accessToken, false);
                var entityTargetUser = AccountService.GetEntityTargetUserByUserId(db, userId, false);
                var entityChatRoom = db.ChatRooms.FirstOrDefault(e => ((e.PatientId == entityUser.UserId && e.DoctorId == entityTargetUser.UserId)
                                                                   || (e.PatientId == entityTargetUser.UserId && e.DoctorId == entityUser.UserId)) && e.ChatRoomId == chatRoomId && !e.IsDelete);

                if (entityChatRoom == null)
                {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.NotFound, Chat.ErrorRoomNotFound));
                }
                if (entityChatRoom.RequestStatus == RequestStatus.Completed)
                {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.Unauthorized, Chat.ErrorSessionAlreadyEnded));
                }
                if (entityChatRoom.RequestStatus != RequestStatus.Accepted)
                {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.Unauthorized, Chat.ErrorSessionNotActive));
                }

                var entityChat = db.Chats.Create();
                entityChat.ChatRoomId = entityChatRoom.ChatRoomId;
                entityChat.MessageType = msgType;
                entityChat.CreateDate = now;
                entityChat.FromUserId = entityUser.UserId;
                entityChat.ToUserId = entityTargetUser.UserId;

                if (msgType == MessageType.Message)
                {
                    entityChat.Message = message.Trim();
                }
                else
                {
                    // Check if the request contains multipart/form-data.
                    if (file == null)
                    {
                        throw new WebApiException(
                               new WebApiError(WebApiErrorCode.InvalidAction, Chat.ErrorNoFileReceived));
                    }

                    string containerName = "s" + entityUser.UserId.ToString("D5");

                    if (msgType == MessageType.Photo)
                    {
                        var entityPhoto = PhotoHelper.UploadImageWithThumbnail(db, containerName, Guid.NewGuid().ToString(), file.InputStream);

                        entityChat.PhotoId = entityPhoto.PhotoId;
                    }
                    else
                    {
                        StringBuilder filenameWithExtension = new StringBuilder();
                        filenameWithExtension.Append(Guid.NewGuid().ToString());
                        var extensionArray = file.FileName.Split('.');
                        if (extensionArray != null && extensionArray.Length == 2)
                        {
                            filenameWithExtension.Append(".");
                            filenameWithExtension.Append(extensionArray[1]);
                        }

                        var entityVoice = VoiceHelper.UploadFile(db, containerName, filenameWithExtension.ToString(), file.InputStream);

                        if (voiceDuration.HasValue)
                        {
                            entityChat.VoiceDuration = voiceDuration;
                        }

                        entityChat.VoiceId = entityVoice.VoiceId;
                    }
                }
                db.Chats.Add(entityChat);

                //update last job date of doctor
                var entityUpdateDoc = db.Doctors.FirstOrDefault(e => e.UserId == entityUser.UserId || e.UserId == entityTargetUser.UserId);
                if (entityUpdateDoc != null)
                {
                    entityUpdateDoc.LastJobDate = DateTime.UtcNow;
                }
                entityChatRoom.LastUpdatedDate = now;
                db.SaveChanges();

                //Replied all message
                db.SP_RepliedAllMessage(entityUser.UserId, chatRoomId);


                // if (db.PatientPackages.Count(e=>!e.IsExpired &&(e.PackageId==2||e.PackageId==4)&&e.UserId==entityUser.UserId)==0)
                // if (isPremium)
                //  {
                if (entityTargetUser.Role == RoleType.Doctor)
                {
                    await PushToUsers(db, entityUser.FullName, entityTargetUser.UserId, PnActionType.Chat, entityChatRoom.ChatRoomId, entityChat.Message, entityChat.MessageType, entityUser.UserId);
                }
                else if (entityTargetUser.Role == RoleType.User)
                {
                    await PushToUsers(db, entityUser.FullName, entityTargetUser.UserId, PnActionType.Chat, entityChatRoom.ChatRoomId, entityChat.Message, entityChat.MessageType, entityUser.UserId);
                }
                //}
                //else
                //{


                //    if (entityTargetUser.Role == RoleType.Doctor && entityTargetUser.IsOnline)
                //    {
                //        PushToUsers(db, entityUser.Nickname, entityTargetUser.UserId, PnActionType.Chat, entityChatRoom.ChatRoomId, entityChat.Message, entityChat.MessageType, PackageType.Free, entityUser.UserId);


                //    }
                //    else if (entityTargetUser.Role == RoleType.User && entityTargetUser.IsOnline)
                //    {
                //        PushToUsers(db, entityUser.Nickname, entityTargetUser.UserId, PnActionType.Chat, entityChatRoom.ChatRoomId, entityChat.Message, entityChat.MessageType, PackageType.Free, entityUser.UserId);


                //    }

                //}
                return new ChatModel(entityChat);
            }
        }

        public static async Task<BoolResult> ChangeChatRequestStatus(string accessToken, int chatRoomId, RequestStatus status)
        {
            ActivityAuditHelper.AddRequestParamsToLog();
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                using (var transaction = db.Database.BeginTransaction())
                {
                    var entityUser = AccountService.GetEntityUserByAccessToken(db, accessToken, false, null);
                    await _ChangeChatRequestStatusPartial(db, entityUser, chatRoomId, status, true);

                    transaction.Commit();
                }
                return new BoolResult(true);
            }
        }

        private static async Task _ChangeChatRequestStatusPartial(Entity.db_HeyDocEntities db, Entity.UserProfile entityUser, int chatRoomId, RequestStatus status, bool sendNotifications = true)
        {
            var entityChatRoom = db.ChatRooms.FirstOrDefault(e => e.ChatRoomId == chatRoomId && (e.PatientId == entityUser.UserId || e.DoctorId == entityUser.UserId));

            var now = DateTime.UtcNow;
            var totalMinutes = (now - entityChatRoom.LastUpdatedDate).TotalMinutes;

            if (entityChatRoom == null)
            {
                throw new WebApiException(new WebApiError(WebApiErrorCode.NotFound, Chat.ErrorRoomNotFound));
            }
            if (entityChatRoom.RequestStatus != RequestStatus.Requested)
            {
                throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidAction, Chat.ErrorCannotChangeStatusRoomNotInRequestedStatus));
            }
            if (entityUser.Role == RoleType.Doctor && status != RequestStatus.Accepted && status != RequestStatus.Rejected)
            {
                throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidArguments, Chat.ErrorCanOnlyAcceptOrReject));
            }
            if (entityUser.Role == RoleType.User)
            {
                if (status != RequestStatus.Canceled)
                {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidArguments, Chat.ErrorCanOnlyCancel));
                }
                const int requestMinWaitTime = 5;
                if (entityChatRoom.RequestStatus == RequestStatus.Requested && totalMinutes < requestMinWaitTime)
                {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidArguments, string.Format(Chat.ErrorMustWaitToCancelRequest, Math.Ceiling(requestMinWaitTime - totalMinutes))));
                }
            }
            if (entityUser.Role == RoleType.Doctor && status == RequestStatus.Accepted)
            {
                // TODO M: Implement payment
                //await BraintreeService.AuthorizePayment(db, entityChatRoom);
                // Payment mock
                var entityPaymentRequest = db.PaymentRequests.Where(e => e.ChatRoomId == entityChatRoom.ChatRoomId && e.PaymentStatus == PaymentStatus.Requested).OrderByDescending(e => e.CreateDate).FirstOrDefault();
                if (entityPaymentRequest == null)
                {
                    entityChatRoom.RequestStatus = RequestStatus.Canceled;
                    ChatService.CreateChatResponse(db, entityChatRoom.ChatRoomId, RequestStatus.Canceled);
                    throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidArguments, Payments.ErrorPaymentRequestNotFound));
                }
                entityPaymentRequest.PaymentStatus = PaymentStatus.Authorised;
                
               
                // END Payment mock
            }
            else
            {
                var entityPaymentRequest = db.PaymentRequests.Where(e => e.ChatRoomId == entityChatRoom.ChatRoomId && e.PaymentStatus == PaymentStatus.Requested).OrderByDescending(e => e.CreateDate).FirstOrDefault();
                if (entityPaymentRequest != null)
                {
                    entityPaymentRequest.PaymentStatus = PaymentStatus.Canceled;
                    db.SaveChanges();
                }
            }
            entityChatRoom.RequestStatus = status;
            entityChatRoom.LastUpdatedDate = DateTime.UtcNow;
            db.SaveChanges();
            //if (status == RequestStatus.Canceled || status == RequestStatus.Rejected)
            //{
            //    var result = BraintreeService.MakeTransaction(db, entityChatRoom, ChatFeeType.Free);  //note: implementation changed, no need to void the trans, since no authorisation made
            //    db.SaveChanges();
            //}

            CreateChatResponse(db, chatRoomId, status);

            if (sendNotifications)
            {
                if (entityUser.Role == RoleType.User)
                {
                    await PushToUsers(db, entityUser.FullName, entityChatRoom.DoctorId, PnActionType.ChatCanceled, entityChatRoom.ChatRoomId, "Chat request canceled", null, entityUser.UserId);
                    var receiptSubject = "[HOPE] Chat request canceled";
                    //var receiptContent = EmailHelper.ConvertEmailHtmlToString(@"Emails\ChatCancel.html");
                    //var patientName = string.IsNullOrEmpty(entityChatRoom.Patient.UserProfile.FullName) ? entityChatRoom.Patient.UserProfile.UserName : entityChatRoom.Patient.UserProfile.FullName;
                    //receiptContent = string.Format(receiptContent, entityChatRoom.Doctor.UserProfile.FullName, patientName);
                    //try
                    //{
                    //    EmailHelper.SendViaSendGrid(new List<string>() { entityChatRoom.Doctor.UserProfile.ContactEmail ?? entityChatRoom.Doctor.UserProfile.UserName }, receiptSubject, receiptContent, string.Empty);
                    //}
                    //catch (Exception ex)
                    //{
                    //    logger.Error(ex);
                    //}

                    //Email to User
                    //receiptSubject = "[HOPE] Chat request canceled";
                    //receiptContent = EmailHelper.ConvertEmailHtmlToString(@"Emails\ChatCancelUser.html");
                    //patientName = string.IsNullOrEmpty(entityChatRoom.Patient.UserProfile.FullName) ? entityChatRoom.Patient.UserProfile.UserName : entityChatRoom.Patient.UserProfile.FullName;
                    //receiptContent = string.Format(receiptContent, patientName);
                    //try
                    //{
                    //    EmailHelper.SendViaSendGrid(new List<string>() { entityChatRoom.Patient.UserProfile.ContactEmail ?? entityChatRoom.Patient.UserProfile.UserName }, receiptSubject, receiptContent, string.Empty);
                    //}
                    //catch (Exception ex)
                    //{
                    //    logger.Error(ex);
                    //}
                }
                else if (entityUser.Role == RoleType.Doctor)
                {
                    if (status == RequestStatus.Accepted)
                    {
                        await PushToUsers(db, entityUser.FullName, entityChatRoom.PatientId, PnActionType.ChatAccepted, entityChatRoom.ChatRoomId, "Chat request accepted", null, entityUser.UserId);
                    }
                    else
                    {
                        await PushToUsers(db, entityUser.FullName, entityChatRoom.PatientId, PnActionType.ChatRejected, entityChatRoom.ChatRoomId, "Chat request rejected", null, entityUser.UserId);
                    }
                }
            }
        }

        public static async Task<ChatRoomModel> StartChat(string accessToken, int doctorId, string promoCode = "", string tpTransactionId = null)
        {
            ActivityAuditHelper.AddRequestParamsToLog();
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                using (var transaction = db.Database.BeginTransaction())
                {
                    var entityUser = AccountService.GetEntityUserByAccessToken(db, accessToken, false, RoleType.User);
                    var entityTargetUser = AccountService.GetEntityTargetUserByUserId(db, doctorId, false);
                    int patientId = entityUser.UserId;

                    int? doctorCategoryId = entityTargetUser.Doctor.CategoryId;
                    if (entityTargetUser.Doctor == null)
                    {
                        throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidArguments, Chat.ErrorDoctorNotFound));
                    }
                    if (!entityTargetUser.IsOnline)
                    {
                        throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidAction, Chat.ErrorDoctorNotOnline));
                    }
                    if (entityTargetUser.Doctor != null && !entityTargetUser.Doctor.IsVerified)
                    {
                        throw new WebApiException(new WebApiError(WebApiErrorCode.Unauthorized, Chat.ErrorDoctorNotVerified));
                    }
                    //if not corporate user deduct point OR If corporate user , free for Inhouse Doctor
                    if ((entityUser.CorporateId == null) || (entityUser.CorporateId != null && doctorCategoryId != 4)) 
                    {
                        //get Balance 
                        Entity.PointBalance pBalance = db.PointBalances.FirstOrDefault(e => e.UserID == patientId);
                        int pointBal = 0;
                        if (pBalance != null) { pointBal = pBalance.Balance; }
                        int price = PaymentService.getDoctorConsultPrice(db, doctorId);
                        if (price > pointBal)
                        {
                            throw new WebApiException(new WebApiError(WebApiErrorCode.Unauthorized, Chat.ErrorPointNotEnought));
                        }
                        else
                        {
                            if(price > 0)
                            {
                                //Point Balance Deduction
                                pBalance.Balance = pBalance.Balance - price;
                            }
                        }
                      
                    }
                    /*Krish 22-Nov-2017 :Already checking inside   BraintreeService.GeneratePaymentRequest(db, entityChatRoom, model, entityPromoCode);*/
                    //if (string.IsNullOrEmpty(entityUser.Patient.BrainTreeUserId) && entityTargetUser.Doctor.Category.CategoryPrice > 0)
                    //{
                    //    throw new WebApiException(new WebApiError(WebApiErrorCode.Unauthorized, "Register your card first."));
                    //}

                    var entityChatRoom = db.ChatRooms.FirstOrDefault(e => e.PatientId == entityUser.UserId && e.DoctorId == entityTargetUser.UserId && !e.IsDelete);
                    var entityDevice = DeviceService.GetEntityDevice(db, accessToken, false);

                    entityChatRoom = _StartChatPartial(db, entityDevice, entityUser, entityTargetUser, entityChatRoom, promoCode, tpTransactionId);

                    string msg = "New chat request";
                    await PushToUsers(db, entityUser.FullName, entityTargetUser.UserId, PnActionType.ChatRequest, entityChatRoom.ChatRoomId, msg, null, entityUser.UserId);

                    //var receiptSubject = "[HOPE] You have a new chat request";
                    //var receiptContent = EmailHelper.ConvertEmailHtmlToString(@"Emails\ChatRequest.html");
                    //var patientName = string.IsNullOrEmpty(entityChatRoom.Patient.UserProfile.FullName) ? entityChatRoom.Patient.UserProfile.UserName : entityChatRoom.Patient.UserProfile.FullName;
                    //receiptContent = string.Format(receiptContent, entityChatRoom.Doctor.UserProfile.FullName, patientName);
                    //try
                    //{
                    //    EmailHelper.SendViaSendGrid(new List<string>() { entityChatRoom.Doctor.UserProfile.ContactEmail ?? entityChatRoom.Doctor.UserProfile.UserName }, receiptSubject, receiptContent, string.Empty);
                    //}
                    //catch (Exception ex)
                    //{
                    //    logger.Error(ex);
                    //}
                    db.SaveChanges();
                    transaction.Commit();

                    return new ChatRoomModel(entityChatRoom);
                }
            }
        }

        private static Entity.ChatRoom _StartChatPartial(Entity.db_HeyDocEntities db, Entity.Device entityDevice, Entity.UserProfile entityUser, Entity.UserProfile entityTargetUser, Entity.ChatRoom entityChatRoom, string promoCode, string tpTransactionId)
        {
            if (entityChatRoom != null && entityChatRoom.RequestStatus == RequestStatus.Requested)
            {
                throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidAction, Chat.ErrorRequestAlreadyPending));
            }
            else if (entityChatRoom != null && entityChatRoom.RequestStatus == RequestStatus.Accepted)
            {
                throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidAction, Chat.ErrorAlreadyActiveSession));
            }

            Entity.PromoCode entityPromoCode = null;
            if (!string.IsNullOrEmpty(promoCode))
            {
                entityPromoCode = PaymentService._VerifyPromoCodePartial(promoCode, db, entityUser, entityTargetUser);
            }

            DateTime now = DateTime.UtcNow;
            if (entityChatRoom == null)
            {
                entityChatRoom = db.ChatRooms.Create();
                entityChatRoom.DoctorId = entityTargetUser.UserId;
                entityChatRoom.PatientId = entityUser.UserId;
                entityChatRoom.CreateDate = now;
                db.ChatRooms.Add(entityChatRoom);
            }
            if (!string.IsNullOrEmpty(tpTransactionId))
            {
                entityChatRoom.ThirdPartyTransactionId = tpTransactionId;
            }
            entityChatRoom.IsNotified = false;
            entityChatRoom.IsDelete = false;
            entityChatRoom.RequestStatus = RequestStatus.Requested;
            entityChatRoom.LastUpdatedDate = now;

            if (entityUser.CorporateId.HasValue)
            {
                var refillReminderMessage = new Entity.Chat
                {
                    ChatRoomId = entityChatRoom.ChatRoomId,
                    FromUserId = entityChatRoom.Doctor.UserId,
                    ToUserId = entityChatRoom.Patient.UserId,
                    CreateDate = DateTime.UtcNow,
                    MessageType = MessageType.Message,
                    Message = "[This is a system-generated message] 🩺💊💊Refilling monthly medications? Have your doctor's prescription ready (within 6 months validity)👩🏼‍⚕️📝🔍",
                    IsRead = false,
                    IsReplied = false,
                    IsDelete = false
                };
                db.Chats.Add(refillReminderMessage);
            }
            

            db.SaveChanges();

            CreateChatResponse(db, entityChatRoom.ChatRoomId, RequestStatus.Requested);

            // TODO M: Implement payment
            //BraintreeService.GeneratePaymentRequest(db, entityDevice, entityChatRoom, entityPromoCode);
            // Payment mock
            decimal amount = entityChatRoom.Doctor.Category.CategoryPrice;
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
            try
            {

                db.SaveChanges();
            }
            catch (Exception ex)
            {               
                throw ex;
            }
   

            // END Payment mock

            return entityChatRoom;
        }

        public static List<ChatModel> ChatList(string accessToken, int chatRoomId, DateTime? lastMessageDateTime, int take = 15, int skip = 0)
        {
            return _ChatListAux(accessToken, chatRoomId, lastMessageDateTime, shouldTakeBeforeTimestamp: false, skip, take);
        }

        public static List<ChatModel> ChatListBeforeTimestamp(string accessToken, int chatRoomId, DateTime oldestMessageDateTime, int take = 15)
        {
            return _ChatListAux(accessToken, chatRoomId, oldestMessageDateTime, shouldTakeBeforeTimestamp: true, skip: 0, take: take);
        }

        private static List<ChatModel> _ChatListAux(string accessToken, int chatRoomId, DateTime? timestamp, bool shouldTakeBeforeTimestamp = true, int skip = 0, int take = 15)
        {
            ActivityAuditHelper.AddRequestParamsToLog();
            if (timestamp != null) { timestamp = DateTimeOffset.Parse(timestamp.ToString()).UtcDateTime; }
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityUser = AccountService.GetEntityUserByAccessToken(db, accessToken, false);

                var entityChatList = db.Chats.Where(e => e.ChatRoomId == chatRoomId && (e.ChatRoom.PatientId == entityUser.UserId || e.ChatRoom.DoctorId == entityUser.UserId));

                if (timestamp.HasValue && timestamp.Value != DateTime.MinValue)
                {
                    if (shouldTakeBeforeTimestamp)
                    {
                        entityChatList = entityChatList.Where(e => e.CreateDate < timestamp.Value);
                    }
                    else
                    {
                        entityChatList = entityChatList.Where(e => e.CreateDate > timestamp.Value);
                    }
                }

                //Read all message
                db.SP_ReadAllMessage(entityUser.UserId, chatRoomId);

                entityChatList = entityChatList.OrderByDescending(e => e.ChatId).Skip(skip).Take(take);

                entityChatList = entityChatList.OrderBy(e => e.ChatId);

                List<ChatModel> result = new List<ChatModel>();

                foreach (var entityChat in entityChatList)
                {
                    result.Add(new ChatModel(entityChat));
                }
                db.SaveChanges();
                return result;
            }
        }

        public static ChatRoomModel GetChatRoom(string accessToken, int chatRoomId)
        {
            ActivityAuditHelper.AddRequestParamsToLog();
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityUser = AccountService.GetEntityUserByAccessToken(db, accessToken, false);

                var entityChatRoom = db.ChatRooms.FirstOrDefault(e => (e.DoctorId == entityUser.UserId || e.PatientId == entityUser.UserId) && !e.IsDelete && e.ChatRoomId == chatRoomId);

                if (entityChatRoom == null)
                {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.NotFound, Chat.ErrorRoomNotFound));
                }
                return new ChatRoomModel(entityChatRoom, null);
            }
        }

        public static List<ChatRoomModel> ChatRoomList(string accessToken, int take = 15, int skip = 0, string searchText = null, RequestStatus? status = null)
        {
            ActivityAuditHelper.AddRequestParamsToLog();
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityUser = AccountService.GetEntityUserByAccessToken(db, accessToken, false);

                var entityChatRoomList = db.ChatRooms.Where(e =>
                    (e.DoctorId == entityUser.UserId || e.PatientId == entityUser.UserId)
                    && !e.IsDelete
                    && !e.Doctor.UserProfile.IsDelete
                    && !e.Patient.UserProfile.IsDelete
                    && (e.Chat != null || e.RequestStatus == RequestStatus.Requested || e.RequestStatus == RequestStatus.Accepted || e.RequestStatus == RequestStatus.Completed));

                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    // search only the other party's name
                    entityChatRoomList = entityChatRoomList.Where(e => e.DoctorId == entityUser.UserId ? e.Patient.UserProfile.FullName.Contains(searchText) : e.Doctor.UserProfile.FullName.Contains(searchText));
                }

                if (status.HasValue)
                {
                    entityChatRoomList = entityChatRoomList.Where(e => e.RequestStatus == status.Value);
                }

                entityChatRoomList = entityChatRoomList.
                                    OrderByDescending(e => e.RequestStatus == RequestStatus.Requested).
                                    ThenByDescending(e => e.RequestStatus == RequestStatus.Accepted).
                                    ThenByDescending(e => e.LastUpdatedDate).
                                    ThenByDescending(e => e.LastChatId).
                                    ThenByDescending(e => e.Chat != null && !e.Chat.IsRead).
                                    ThenByDescending(e => e.Chat.CreateDate).
                                    ThenByDescending(e => e.CreateDate).
                                    Skip(skip).Take(take);

                List<ChatRoomModel> result = new List<ChatRoomModel>();

                foreach (var entityChatRoom in entityChatRoomList)
                {
                    int UnreadCount = db.Chats.Where(e => e.ChatRoomId == entityChatRoom.ChatRoomId && e.ToUserId == entityUser.UserId && !e.IsRead && !e.IsDelete).Count();

                    result.Add(new ChatRoomModel(entityChatRoom, UnreadCount));
                }

                return result;
            }
        }

        public static List<ChatRoomModel> ChatRoomList(string accessToken, int userId, int take = 15, int skip = 0)
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityDoctorOrPartner = AccountService.GetEntityUserByAccessToken(db, accessToken);
                if (entityDoctorOrPartner.Role == RoleType.User || entityDoctorOrPartner.Role == RoleType.Admin || entityDoctorOrPartner.Role == RoleType.SuperAdmin)
                {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.Unauthorized, string.Format(Errors.UnauthorizedRole, ConstantHelper.Doc2UsEmailContact)));
                }
                var entityUser = AccountService.GetEntityUserByUserId(db, userId.ToString());

                var entityChatRoomList = db.ChatRooms.Where(e => (e.DoctorId == entityUser.UserId || e.PatientId == entityUser.UserId) && !e.IsDelete && !e.Doctor.UserProfile.IsDelete && !e.Patient.UserProfile.IsDelete)
                                    .OrderBy(e => e.RequestStatus == RequestStatus.Requested).OrderByDescending(e => e.LastUpdatedDate).OrderByDescending(e => e.LastChatId)
                                    .ThenByDescending(e => e.Chat != null && !e.Chat.IsRead).ThenByDescending(e => e.Chat.CreateDate).Skip(skip).Take(take);

                var result = new List<ChatRoomModel>();

                foreach (var entityChatRoom in entityChatRoomList)
                {
                    int UnreadCount = db.Chats.Where(e => e.ChatRoomId == entityChatRoom.ChatRoomId && e.ToUserId == entityUser.UserId && !e.IsRead).Count();

                    result.Add(new ChatRoomModel(entityChatRoom, UnreadCount));
                }

                return result;
            }
        }

        public static BoolResult DeleteChat(string accessToken, long chatId)
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityUser = AccountService.GetEntityUserByAccessToken(db, accessToken, false);

                var entityChat = db.Chats.FirstOrDefault(e => e.ChatId == chatId && e.FromUserId == entityUser.UserId && !e.IsDelete);

                if (entityChat == null)
                {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.NotFound, Chat.ErrorMessageNotFound));
                }

                if (entityChat.ChatId == entityChat.ChatRoom.LastChatId)
                {
                    var entityLastChat = db.Chats.OrderByDescending(e => e.ChatId).FirstOrDefault(e => e.ChatRoomId == entityChat.ChatRoomId && e.ChatId != entityChat.ChatId && !e.IsDelete);
                    entityChat.ChatRoom.Chat = entityLastChat;
                }
                entityChat.IsDelete = true;
                db.SaveChanges();

                return new BoolResult(true);
            }
        }

        public static async Task<BoolResult> ExitChatRoom(string accessToken, int chatRoomId, ChatFeeType? feeType = null)
        {
            ActivityAuditHelper.AddRequestParamsToLog();
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityUser = AccountService.GetEntityUserByAccessToken(db, accessToken, false);

                var entityChat = db.ChatRooms.FirstOrDefault(e => e.ChatRoomId == chatRoomId && (e.PatientId == entityUser.UserId || e.DoctorId == entityUser.UserId));

                if (entityChat == null)
                {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.NotFound, Chat.ErrorRoomNotFound));
                }
                await _ExitChatroomPartial(db, entityUser, entityChat);

                return new BoolResult(true);
            }
        }

        public static void MockMakeTransaction(Entity.db_HeyDocEntities db, Entity.ChatRoom entityChat)
        {
            var entityRequest = entityChat.PaymentRequests.Where(e => e.PaymentStatus == PaymentStatus.Authorised).OrderByDescending(e => e.CreateDate).FirstOrDefault();
            if (entityRequest != null)
            {
                entityRequest.PaymentStatus = PaymentStatus.Paid;
                entityRequest.PlatformAmount = 0;

                try
                {
                    string subject = "HOPE: Transaction Summary";
                    string content = EmailHelper.ConvertEmailHtmlToString(@"Emails\TransactionEmail.html");
                    var discount = entityRequest.ActualAmount - entityRequest.Amount;
                    content = string.Format(content,
                                            entityChat.PatientId, //{0}
                                            entityChat.Patient.UserProfile.FullName,//{1} 
                                            entityChat.Patient.UserProfile.UserName, //{2}
                                            entityRequest.Amount, //{3}
                                            DateTime.UtcNow.AddHours(8).ToString("dd-MMM-yyy hh:mm tt"),//{4}
                                            entityChat.DoctorId, //{5}
                                            entityChat.Doctor.UserProfile.FullName, //{6}
                                            entityRequest.BrainTreeTransactionId,//{7}
                                            entityRequest.PaymentStatus.ToString(), //{8}
                                            "", //{9}
                                            entityRequest.ActualAmount, //{10}
                                            (discount < 0) ? 0 : discount, //{11}
                                            (entityRequest.PromoCode != null ? entityRequest.PromoCode.PromoCode1 : "")); //{12}

                    EmailHelper.SendViaSendGrid(new List<string>() { "" }, subject, content, string.Empty);
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                }
                string paymentNum = "xxxxxx";

                if (entityRequest.Amount > 0)
                {
                    string startDate = "Data not available";
                    string endDate = "Data not available";
                    var startChat = ChatService.GetChatResponse(db, entityChat.ChatRoomId, RequestStatus.Accepted);
                    if (startChat != null)
                    {
                        startDate = startChat.CreatedDate.AddHours(8).ToString("dd-MMM-yyy hh:mm tt");
                    }
                    var endChat = ChatService.GetChatResponse(db, entityChat.ChatRoomId, RequestStatus.Completed);
                    if (endChat != null)
                    {
                        endDate = endChat.CreatedDate.AddHours(8).ToString("dd-MMM-yyy hh:mm tt");
                    }
                    var receiptSubject = "HOPE: Chat Receipt";
                    var receiptContent = EmailHelper.ConvertEmailHtmlToString(@"Emails\ChatReceipt.html");
                    var total = entityRequest.ActualAmount.ToString();
                    var promoDiscount = entityRequest.ActualAmount - entityRequest.Amount;
                    promoDiscount = promoDiscount < 0 ? 0 : promoDiscount;
                    receiptContent = string.Format(receiptContent, entityChat.Patient.UserProfile.FullName,
                                                                    entityRequest.Amount.ToString(),
                                                                    DateTime.UtcNow.AddHours(8).ToString("dd-MMM-yyyy"),
                                                                    entityChat.Doctor.UserProfile.FullName,
                                                                    entityChat.Patient.UserProfile.FullName,
                                                                    entityRequest.BrainTreeTransactionId,
                                                                    total, entityRequest.Amount.ToString(),
                                                                    paymentNum, startDate, endDate, promoDiscount.ToString());
                    try
                    {
                        EmailHelper.SendViaSendGrid(new List<string>() { entityChat.Patient.UserProfile.ContactEmail ?? entityChat.Patient.UserProfile.UserName }, receiptSubject, receiptContent, string.Empty);
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex);
                    }
                }
            }
        }

        internal static async Task _ExitChatroomPartial(Entity.db_HeyDocEntities db, Entity.UserProfile entityUser, Entity.ChatRoom entityChat)
        {
            entityChat.IsMailSent = false;
            CreateChatResponse(db, entityChat.ChatRoomId, RequestStatus.Completed);
            // TODO M: Implement payment
            //var result = BraintreeService.MakeTransaction(db, entityChat);
            // Payment mock
            MockMakeTransaction(db, entityChat);
            // END Payment mock

            //entityChat.IsDelete = true;
            entityChat.RequestStatus = RequestStatus.Completed;
            entityChat.LastUpdatedDate = DateTime.UtcNow;

            db.SaveChanges();
            string msg;
            // Build a list with the message in every desired language
            var multiLangMsg =
                ConstantHelper.MultilingualLangs
                    .Select(lang =>
                        Chat.ResourceManager.GetString("SessionEndedNotification", CultureInfo.GetCultureInfo(lang)))
                    .Distinct(); // Use Distinct so we don't show duplicates of the English message for unavailable languages
            msg = string.Join("\n", multiLangMsg);

            if (entityUser.Role == RoleType.Doctor)
            {
                msg = string.Format(msg, entityUser.FullName);
                var extraParams = new Dictionary<string, string>
                {
                    { "DoctorId", entityChat.DoctorId.ToString() }
                };
                await NotificationService.NotifyUser(db, entityChat.PatientId, PnActionType.ChatEnded, entityChat.ChatRoomId, msg, extraParams);
            }
            else
            {
                msg = string.Format(msg, entityChat.Doctor.UserProfile.FullName);
                var entityPromoUrl = db.PromotionUrls.OrderByDescending(e => e.CreateDate).FirstOrDefault();
                NotificationService.CreateNotification(db, PnActionType.ChatEnded, entityChat.ChatRoomId.ToString(), msg, entityPromoUrl, entityUser.UserId);
            }
            //PushToUsers(db, entityUser.FullName, toId, PnActionType.ChatEnded, entityChat.ChatRoomId, msg, null, entityUser.UserId);
        }

        internal static async Task PushToUsers(Entity.db_HeyDocEntities db, string fromUserName, int toUserId, PnActionType type, long relatedId, string text, MessageType? msgType = null, int? fromUserId = null)
        {
            if (fromUserName.Length > 50)
            {
                fromUserName.Truncate(50);
            }

            if (!string.IsNullOrEmpty(text) && text.Length > 100)
            {
                text.Trim().Truncate(100);
            }
            try
            {
                List<Entity.Device> enittyDeviceList = new List<Entity.Device>();

                var entityUserDeviceList = AccountService.GetEntityTargetUserByUserId(db, toUserId, false).Devices.Where(e => (e.TokenType != AccessTokenType.Doc2Us || e.RegistrationId != null) && e.AccessToken != null && e.UserId != null);
                if (entityUserDeviceList.Count() > 0)
                {
                    foreach (var entityUserDevice in entityUserDeviceList)
                    {
                        enittyDeviceList.Add(entityUserDevice);
                    }
                }

                var unreadCount = db.Chats.Count(e => e.ToUserId == toUserId && !e.IsRead && !e.IsDelete);

                Notification notification = new Notification();
                if (MessageType.Photo == msgType)
                {
                    notification.Title = fromUserName + ':' + ' ' + "Image" + ' ' + text;

                }
                else if (MessageType.Voice == msgType)
                {
                    notification.Title = fromUserName + ':' + ' ' + "Voice" + ' ' + text;
                }
                else
                {
                    notification.Title = fromUserName + ':' + ' ' + text;
                }
                notification.ActionType = type;
                notification.ActionContent = relatedId.ToString();
                notification.Items.Add("FromUser", fromUserName);


                if (msgType.HasValue)
                {
                    notification.Items.Add("MsgType", ((int)msgType).ToString());
                }
                notification.Items.Add("FromUserId", fromUserId.ToString());
                notification.Items.Add("ToUserId", toUserId.ToString());

                if (type == PnActionType.ChatRequest)
                {
                    notification.Items.Add("sound", "elegant_ringtone.caf");
                }
                else
                {
                    notification.Items.Add("sound", "default");
                }
                notification.Items.Add("badge", unreadCount.ToString());

                /* KHN Added this to fix notification id null problem and to save chat request to doctor */
                var entityNotification = db.Notifications.Create();
                entityNotification.CreateDate = DateTime.UtcNow;
                entityNotification.DeviceType = null;
                entityNotification.IsRead = false;
                entityNotification.NotificationType = type;
                entityNotification.RelatedId = relatedId.ToString();
                entityNotification.Text = text;
                entityNotification.UserId = null;
               
                db.Notifications.Add(entityNotification);
                db.SaveChanges();
                notification.Items.Add("NotificationId", entityNotification.NotificationId.ToString());
                /* KHN End */

                await PushManager.Push(enittyDeviceList, notification, unreadCount: unreadCount);
                //await PusherServer.Message
            }
            catch (Exception ex)
            {
                logger.Error("PushToUsers Exception: " + ex.ToString());
            }
        }

        public static IntResult GetUnreadChatCount(string accessToken, DateTime lastGetDate)
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityUser = AccountService.GetEntityUserByAccessToken(db, accessToken, false);
                var count = db.ChatRooms.Count(e => (e.DoctorId == entityUser.UserId || e.PatientId == entityUser.UserId) && e.LastUpdatedDate > lastGetDate);
                return new IntResult(count);
            }
        }

        public static StringResult GetExternalVideoChatURL(string accessToken, int chatRoomId)
        {
            ActivityAuditHelper.AddRequestParamsToLog();
            using (var db = new Entity.db_HeyDocEntities())
            {
                var entityUser = AccountService.GetEntityUserByAccessToken(db, accessToken, false);
                var chatroom = db.ChatRooms
                        .Include(e => e.Doctor)
                        .Include(e => e.Doctor.UserProfile)
                        .FirstOrDefault(e => e.ChatRoomId == chatRoomId);

                if (chatroom == null)
                {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.NotFound));
                }
                if (chatroom.PatientId != entityUser.UserId)
                {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.Unauthorized));
                }
                if (chatroom.Doctor.VideoChatUrl == null)
                {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidAction, Resources.Chat.ErrorDoctorVideoNotEnabled));
                }
                if (!chatroom.Doctor.UserProfile.IsOnline)
                {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidAction, Resources.Chat.ErrorDoctorNotOnline));
                }

                var chatLog = db.ExternalVideoChatLogs.Create();
                chatLog.CallDate = DateTime.UtcNow;
                chatLog.PatientId = chatroom.PatientId;
                chatLog.DoctorId = chatroom.DoctorId;
                db.ExternalVideoChatLogs.Add(chatLog);
                db.SaveChanges();

                return new StringResult(chatroom.Doctor.VideoChatUrl);
            }
        }

        public static Entity.ChatRoom GetChatRoomWithDoctor(Entity.db_HeyDocEntities db, Entity.UserProfile patient, int doctorId)
        {
            var entityChatRoom = db.ChatRooms.FirstOrDefault(e => e.PatientId == patient.UserId && e.DoctorId == doctorId && !e.IsDelete);

            return entityChatRoom;
        }

        public static Entity.ChatRoom GetChatRoomByDoctor(Entity.db_HeyDocEntities db, int chatRoomId, Entity.UserProfile entityUser)
        {
            var entityChatRoom = db.ChatRooms.FirstOrDefault(e => e.ChatRoomId == chatRoomId && e.DoctorId == entityUser.UserId);

            if (entityChatRoom == null)
            {
                throw new WebApiException(new WebApiError(WebApiErrorCode.NotFound));
            }

            return entityChatRoom;
        }

        public static Entity.ChatRoom GetChatRoomByUser(Entity.db_HeyDocEntities db, int chatRoomId, Entity.UserProfile entityUser)
        {
            var entityChatRoom = db.ChatRooms.FirstOrDefault(e => e.ChatRoomId == chatRoomId && (e.DoctorId == entityUser.UserId || e.PatientId == entityUser.UserId));

            if (entityChatRoom == null)
            {
                throw new WebApiException(new WebApiError(WebApiErrorCode.NotFound));
            }

            return entityChatRoom;
        }

        internal static int GetNumberOfRequestCancelled(Entity.db_HeyDocEntities db, int userId)
        {
            int count = db.ChatResponses.Where(e => e.ChatRoom.DoctorId == userId && !e.ChatRoom.IsDelete && e.RequestStatus == RequestStatus.Canceled).Count();

            return count;
        }

        internal static BoolResult CreateChatResponse(Entity.db_HeyDocEntities db, int chatRoomId, RequestStatus status)
        {
            _CreateChatResponsePartial(db, chatRoomId, status);
            db.SaveChanges();

            return new BoolResult(true);
        }

        internal static void _CreateChatResponsePartial(Entity.db_HeyDocEntities db, int chatRoomId, RequestStatus status)
        {
            Entity.ChatResponse entityChatResponse = new Entity.ChatResponse();
            entityChatResponse.IsDelete = false;
            entityChatResponse.ChatRoomId = chatRoomId;
            entityChatResponse.RequestStatus = status;
            entityChatResponse.CreatedDate = DateTime.UtcNow;
            db.ChatResponses.Add(entityChatResponse);
        }

        // For use from WebJobs
        public static void CreateChatResponsePartial(Entity.db_HeyDocEntities db, int chatRoomId, RequestStatus status)
        {
            _CreateChatResponsePartial(db, chatRoomId, status);
        }

        internal static void CreateChatBotSessionHistoryMessage(Entity.db_HeyDocEntities db, Entity.ChatBotSession chatBotSession)
        {
            var prescriptionCreated = db.Prescriptions.Any(e => e.ChatBotSessionId == chatBotSession.Id);
            var message = "Your conversation with the Chat Assistant was recorded here. " + (prescriptionCreated ? "(Prescription pending approval)" : "(No prescription created)");
            var chatRoom = chatBotSession.ChatRoom;
            var sessionLinkMessage = new Entity.Chat
            {
                ChatRoomId = chatRoom.ChatRoomId,
                FromUserId = chatRoom.DoctorId,
                ToUserId = chatRoom.PatientId,
                CreateDate = DateTime.UtcNow,
                MessageType = MessageType.ChatBotSession,
                ChatBotSessionId = chatBotSession.Id,
                Message = message,
                IsRead = false,
                IsReplied = false,
                IsDelete = false
            };
            db.Chats.Add(sessionLinkMessage);
        }

        internal static Entity.ChatResponse GetChatResponse(Entity.db_HeyDocEntities db, int chatRoomId, RequestStatus status)
        {
            var entityChatResponse = db.ChatResponses.Where(e => e.ChatRoomId == chatRoomId && e.RequestStatus == status).OrderByDescending(e => e.CreatedDate).FirstOrDefault();
            if (entityChatResponse == null)
            {
                return null;
            }
            return entityChatResponse;
        }

        public static List<ChatModel> GetChatroomPatientAllMedia(Entity.db_HeyDocEntities db, string accessToken, int chatroomId, DateTimeOffset? skipTimestamp = null, int take = 15)
        {
            var entityUser = AccountService.GetEntityUserByAccessToken(db, accessToken, false);
            var doctorRoles = ConstantHelper.DoctorRoles;
            if (!doctorRoles.Any(r => entityUser.Roles.Contains(r)))
            {
                throw new WebApiException(new WebApiError(WebApiErrorCode.Unauthorized));
            }

            var entityChatroom = db.ChatRooms.FirstOrDefault(e => e.ChatRoomId == chatroomId && e.DoctorId == entityUser.UserId);
            if (entityChatroom == null)
            {
                throw new WebApiException(new WebApiError(WebApiErrorCode.NotFound));
            }

            return GetPatientAllMedia(db, entityChatroom.PatientId, skipTimestamp, take);
        }

        internal static List<ChatModel> GetPatientAllMedia(Entity.db_HeyDocEntities db, int patientId, DateTimeOffset? skipTimestamp = null, int take = 15)
        {
            var entityChatList = db.Chats.Where(e => e.ChatRoom.PatientId == patientId && e.FromUserId == patientId && e.MessageType == MessageType.Photo);
            if (skipTimestamp.HasValue && skipTimestamp.Value.UtcDateTime != DateTimeOffset.MinValue.UtcDateTime)
            {
                var utcDateTime = skipTimestamp.Value.UtcDateTime;
                entityChatList = entityChatList.Where(e => e.CreateDate < utcDateTime);
            }
            entityChatList = entityChatList.OrderByDescending(e => e.ChatId).Take(take);

            List<ChatModel> result = new List<ChatModel>();
            foreach (var entityChat in entityChatList)
            {
                result.Add(new ChatModel(entityChat));
            }

            return result;
        }

        internal static (List<ChatModel> chatList, int totalRecords) GetPatientAllMedia(Entity.db_HeyDocEntities db, int patientId, int skip = 0, int take = 15)
        {
            var entityChats = db.Chats.Where(e => e.ChatRoom.PatientId == patientId && e.FromUserId == patientId && e.MessageType == MessageType.Photo);
            var entityChatList = entityChats.OrderByDescending(e => e.ChatId).Skip(skip).Take(take);

            List<ChatModel> result = new List<ChatModel>();
            foreach (var entityChat in entityChatList)
            {
                result.Add(new ChatModel(entityChat));
            }

            return (result, entityChats.Count());
        }
    }
}
