using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace HeyDoc.Web.Models
{
    public class ChatModel
    {
        public long ChatId { get; set; }
        public int FromUserId { get; set; }
        public string FromUserFullName { get; set; }
        public int ToUserId { get; set; }
        public string ToUserFullName { get; set; }
        public int ChatRoomId { get; set; }
        public MessageType MessageType { get; set; }
        public string Message { get; set; }
        public long? PhotoId { get; set; }
        public int? ChatBotSessionId { get; set; }
        public DateTime CreateDate { get; set; }
        public bool IsRead { get; set; }
        public long? VoiceId { get; set; }
        public PhotoModel Photo { get; set; }
        public VoiceModel Voice { get; set; }
        public TimeSpan? VoiceDuration { get; set; }
        public HttpPostedFileBase File { get; set; }
        public bool IsDeleted { get; set; }

        public bool IsReplied { get; set; }

        public ChatModel()
        {

        }

        public ChatModel(Entity.Chat entityChat)
        {
            ChatId = entityChat.ChatId;
            FromUserId = entityChat.FromUserId;
            var fromUser = entityChat.UserProfile;
            FromUserFullName = fromUser.FullName;
            if (!string.IsNullOrEmpty(fromUser.Title))
            {
                FromUserFullName = string.Format("{0}. {1}", fromUser.Title, fromUser.FullName);
            }
            ToUserId = entityChat.ToUserId;
            var toUser = entityChat.UserProfile1;
            ToUserFullName = toUser.FullName;
            if (!string.IsNullOrEmpty(toUser.Title))
            {
                ToUserFullName = string.Format("{0}. {1}", toUser.Title, toUser.FullName);
            }
            MessageType = entityChat.MessageType;
            Message = entityChat.Message;
            if (entityChat.PhotoId.HasValue)
            {
                Photo = new PhotoModel(entityChat.Photo);
            }
            if (entityChat.VoiceId.HasValue)
            {
                Voice = new VoiceModel(entityChat.Voice);
                if (entityChat.VoiceDuration.HasValue)
                {
                    VoiceDuration = entityChat.VoiceDuration.Value;
                }
            }
            ChatBotSessionId = entityChat.ChatBotSessionId;
            IsRead = entityChat.IsRead;
            CreateDate = entityChat.CreateDate;
            IsDeleted = entityChat.IsDelete;
            IsReplied = entityChat.IsReplied;
        }
    }
}
