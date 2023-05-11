using HeyDoc.Web.Lib;
using HeyDoc.Web.Models;
using HeyDoc.Web.Services;
using HeyDoc.Web.WebApi;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using PusherServer;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Filters;
using System.Web.Http.ValueProviders;
using System.Web.Http.ValueProviders.Providers;
using JwtRegisteredClaimNames = System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames;

namespace HeyDoc.Web.Controllers.Api
{
    public class ChatController : ApiController
    {
        [Authorize]
        [HttpPost]
        public Task<ChatModel> ChatPWA([ValueProvider(new[] { typeof(HeaderValueProviderFactory), typeof(QueryStringValueProviderFactory) })] string accessToken, int userId, int chatRoomId, MessageType msgType, TimeSpan? voiceDuration = null)
        {
            HttpPostedFile uploadedFile = HttpContext.Current.Request.Files["file"];

            string message = HttpContext.Current.Request.Form["Message"] ?? "";

            return WebApiWrapper.CallAsync(() => ChatService.ChattingPWA(accessToken, userId, chatRoomId, msgType, message, uploadedFile, voiceDuration));
        }

        [HttpPost]
        public Task<ChatModel> Chat([ValueProvider(new[] { typeof(HeaderValueProviderFactory), typeof(QueryStringValueProviderFactory) })] string accessToken, int userId, int chatRoomId, MessageType msgType, TimeSpan? voiceDuration = null)
        {
            HttpPostedFile uploadedFile = HttpContext.Current.Request.Files["file"];

            string message = HttpContext.Current.Request.Form["Message"] ?? "";

            return WebApiWrapper.CallAsync(() => ChatService.Chatting(accessToken, userId, chatRoomId, msgType, message, uploadedFile, voiceDuration));
        }

        [HttpGet]
        public List<ChatModel> List([ValueProvider(new[] { typeof(HeaderValueProviderFactory), typeof(QueryStringValueProviderFactory) })] string accessToken, int chatRoomId, DateTime? lastMessageDateTime = null, int take = 15, int skip = 0)
        {
            return WebApiWrapper.Call<List<ChatModel>>(e => ChatService.ChatList(accessToken, chatRoomId, lastMessageDateTime, take, skip));
        }

        [HttpGet]
        public List<ChatModel> List([ValueProvider(new[] { typeof(HeaderValueProviderFactory), typeof(QueryStringValueProviderFactory) })] string accessToken, int chatRoomId, DateTime oldestMessageDateTime, int take = 15)
        {
            return WebApiWrapper.Call<List<ChatModel>>(e => ChatService.ChatListBeforeTimestamp(accessToken, chatRoomId, oldestMessageDateTime, take));
        }

        [HttpGet]
        public List<ChatRoomModel> ChatList([ValueProvider(new[] { typeof(HeaderValueProviderFactory), typeof(QueryStringValueProviderFactory) })] string accessToken, int take, int skip, string searchText = null)
        {
            return WebApiWrapper.Call<List<ChatRoomModel>>(e => ChatService.ChatRoomList(accessToken, take, skip, searchText, null));
        }

        [HttpGet]
        public List<ChatRoomModel> ChatRequests([ValueProvider(new[] { typeof(HeaderValueProviderFactory), typeof(QueryStringValueProviderFactory) })] string accessToken, int take, int skip)
        {
            return WebApiWrapper.Call<List<ChatRoomModel>>(e => ChatService.ChatRoomList(accessToken, take, skip, "", Web.RequestStatus.Requested));
        }

        [HttpGet]
        public List<ChatRoomModel> ChatList([ValueProvider(new[] { typeof(HeaderValueProviderFactory), typeof(QueryStringValueProviderFactory) })] string accessToken,int userId, int take, int skip)
        {
            return WebApiWrapper.Call<List<ChatRoomModel>>(e => ChatService.ChatRoomList(accessToken,userId, take, skip));
        }

        [HttpGet]
        public ChatRoomModel ChatRoom([ValueProvider(new[] { typeof(HeaderValueProviderFactory), typeof(QueryStringValueProviderFactory) })] string accessToken, int chatRoomId)
        {
            return WebApiWrapper.Call<ChatRoomModel>(e => ChatService.GetChatRoom(accessToken, chatRoomId));
        }

        [HttpDelete]
        public BoolResult Delete([ValueProvider(new[] { typeof(HeaderValueProviderFactory), typeof(QueryStringValueProviderFactory) })] string accessToken, long chatId)
        {
            return WebApiWrapper.Call<BoolResult>(e => ChatService.DeleteChat(accessToken, chatId));
        }

        [HttpPut]
        public Task<BoolResult> Exit([ValueProvider(new[] { typeof(HeaderValueProviderFactory), typeof(QueryStringValueProviderFactory) })] string accessToken, int chatRoomId,ChatFeeType? feeType = null)
        {
            return WebApiWrapper.CallAsync(() => ChatService.ExitChatRoom(accessToken, chatRoomId, feeType));
        }

        [HttpPost]
        public Task<ChatRoomModel> Start([ValueProvider(new[] { typeof(HeaderValueProviderFactory), typeof(QueryStringValueProviderFactory) })] string accessToken, int userId, string promoCode = "", string thirdPartyTransactionId = null)
        {
            return WebApiWrapper.CallAsync(() => ChatService.StartChat(accessToken, userId, promoCode, thirdPartyTransactionId));
        }

        [HttpGet]
        public Task<BoolResult> RequestStatus([ValueProvider(new[] { typeof(HeaderValueProviderFactory), typeof(QueryStringValueProviderFactory) })] string accessToken, int chatRoomId, RequestStatus status)
        {
            return WebApiWrapper.CallAsync(() => ChatService.ChangeChatRequestStatus(accessToken, chatRoomId,status));
        }

        [HttpGet]
        public IntResult UnreadCount([ValueProvider(new[] { typeof(HeaderValueProviderFactory), typeof(QueryStringValueProviderFactory) })] string accessToken, DateTime lastGetDate)
        {
            return WebApiWrapper.Call<IntResult>(e => ChatService.GetUnreadChatCount(accessToken, lastGetDate));
        }

        [HttpGet]
        public StringResult GetExternalVideoChatURL([ValueProvider(new[] { typeof(HeaderValueProviderFactory), typeof(QueryStringValueProviderFactory) })] string accessToken, int chatRoomId)
        {
            return WebApiWrapper.Call(e => ChatService.GetExternalVideoChatURL(accessToken, chatRoomId));
        }

        [HttpGet]
        public List<ChatModel> ListAllMediaByChatroomPatient([ValueProvider(new[] { typeof(HeaderValueProviderFactory), typeof(QueryStringValueProviderFactory) })] string accessToken, int chatroomId, DateTimeOffset? skipTimestamp = null, int take = 15)
        {
            using (var db = new Entity.db_HeyDocEntities())
            {
                return WebApiWrapper.Call(e => ChatService.GetChatroomPatientAllMedia(db, accessToken, chatroomId, skipTimestamp, take));
            }
        }
    }
}
