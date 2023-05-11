using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HeyDoc.Web.Models
{
    public class ChatRoomModel
    {
        public int ChatRoomId { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime LastUpdateDate { get; set; }
        public bool IsDelete { get; set; }

        public RequestStatus RequestStatus { get; set; }

        public DoctorModel Doctor { get; set; }
        public UserModel Patient { get; set; }

        public ChatModel LastChat { get; set; }
        public int UnreadMsgCount { get; set; }
        public string ThirdPartyTransactionId { get; set; }

        public ChatRoomModel()
        {

        }

        public ChatRoomModel(Entity.ChatRoom entityChatRoom, int? unreadCount = null)
        {
            ChatRoomId = entityChatRoom.ChatRoomId;
            Doctor = new DoctorModel(entityChatRoom.Doctor.UserProfile, entityChatRoom.Doctor);
            Patient = new UserModel(entityChatRoom.Patient.UserProfile);
            if (entityChatRoom.LastChatId != null)
            {
                LastChat = new ChatModel(entityChatRoom.Chat);
            }
            if (unreadCount.HasValue)
            {
                UnreadMsgCount = unreadCount.Value;
            }
            if (entityChatRoom.ThirdPartyTransactionId != null)
            {
                ThirdPartyTransactionId = entityChatRoom.ThirdPartyTransactionId;
            }
            CreateDate = entityChatRoom.CreateDate;
            RequestStatus = entityChatRoom.RequestStatus;
            LastUpdateDate = entityChatRoom.LastUpdatedDate;
        }
    }
}