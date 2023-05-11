using System.Collections.Generic;

namespace HeyDoc.Web.Models.ChatBots
{
    public class ChatBotInteractRequestModel
    {
        public int ChatSessionId { get; set; }
        public int QuestionId { get; set; }
        public int? AnswerId { get; set; }
        public Dictionary<string, string> AnswerData { get; set; }
    }
}