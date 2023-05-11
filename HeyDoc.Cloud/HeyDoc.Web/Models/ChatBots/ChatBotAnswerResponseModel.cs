using HeyDoc.Web.Entity;

namespace HeyDoc.Web.Models.ChatBots
{
    public class ChatBotAnswerResponseModel
    {
        public ChatBotAnswerResponseModel(ChatBotAnswer chatBotAnswer)
        {
            Id = chatBotAnswer.Id;
            AnswerText = chatBotAnswer.AnswerText;
            Type = chatBotAnswer.Type;
            ApiEndpoint = chatBotAnswer.ApiEndpoint;
            SecondaryScreenTitle = chatBotAnswer.SecondaryScreenTitle;
        }

        public ChatBotAnswerResponseModel(ChatBotAnswer chatBotAnswer, ChatBotResponse chatBotResponse)
        {
            Id = chatBotAnswer.Id;
            AnswerText = chatBotAnswer.AnswerText;
            Type = chatBotAnswer.Type;
            Selected = chatBotAnswer.Id == chatBotResponse.AnswerId;
        }

        public int Id { get; set; }
        public string AnswerText { get; set; }
        public string ApiEndpoint { get; set; }
        // Title to display on secondary screen, e.g. the list selection for LIST_SELECT type answers
        public string SecondaryScreenTitle { get; set; }
        public ChatBotAnswerType Type { get; set; }
        public bool? Selected { get; set; }
    }
}