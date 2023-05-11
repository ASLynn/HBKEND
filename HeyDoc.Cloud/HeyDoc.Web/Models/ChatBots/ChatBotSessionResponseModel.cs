using HeyDoc.Web.Entity;

namespace HeyDoc.Web.Models.ChatBots
{
    public class ChatBotSessionResponseModel
    {
        public ChatBotSessionResponseModel(ChatBotSession chatBotSession, bool isNewSession)
        {
            ChatBotSessionId = chatBotSession.Id;
            IsNewSession = isNewSession;
        }

        public int ChatBotSessionId { get; set; }
        public bool IsNewSession { get; set; }
    }
}