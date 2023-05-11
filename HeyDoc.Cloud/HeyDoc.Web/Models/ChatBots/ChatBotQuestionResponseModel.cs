using HeyDoc.Web.Entity;
using HeyDoc.Web.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HeyDoc.Web.Models.ChatBots
{
    public class ChatBotQuestionResponseModel
    {
        public ChatBotQuestionResponseModel(ChatBotQuestion chatBotQuestion, DateTime timestamp)
        {
            Id = chatBotQuestion.Id;
            Type = chatBotQuestion.Type;
            QuestionText = "🤖Virtual Medical Assistant:\n" + chatBotQuestion.QuestionText;
            // Don't populate Answers if client is supposed to just fetch the next question
            // This is mainly important for questions where there is still a decision branch but is decided by the server instead of the client, e.g. checking if user is registered under a corporate
            if (chatBotQuestion.Type == ChatBotQuestionType.CALL_NEXT_QUESTION)
            {
                Answers = new List<ChatBotAnswerResponseModel>();
            }
            else
            {
                Answers = chatBotQuestion?.ChildAnswers?.Select(x => new ChatBotAnswerResponseModel(x))?.ToList();
            }
            Timestamp = timestamp;
            TimeDelay = chatBotQuestion.TimeDelay;
            ButtonName = chatBotQuestion.ButtonName;
            ButtonLink = chatBotQuestion.ButtonLink;
            ImageUrl = chatBotQuestion?.Photo?.ImageUrl;
        }

        public ChatBotQuestionResponseModel(ChatBotResponse chatBotResponse)
        {
            var chatBotQuestion = chatBotResponse.ChatBotQuestion;
            Id = chatBotQuestion.Id;
            Type = chatBotQuestion.Type;
            QuestionText = "🤖Virtual Medical Assistant:\n" + chatBotQuestion.QuestionText;
            // Don't populate Answers if client is supposed to just fetch the next question
            // This is mainly important for questions where there is still a decision branch but is decided by the server instead of the client, e.g. checking if user is registered under a corporate
            if (chatBotQuestion.Type == ChatBotQuestionType.CALL_NEXT_QUESTION)
            {
                Answers = new List<ChatBotAnswerResponseModel>();
            }
            else
            {
                Answers = chatBotQuestion?.ChildAnswers?.Select(x => new ChatBotAnswerResponseModel(x, chatBotResponse))?.ToList();
            }
            TimeDelay = 0;
            Timestamp = chatBotResponse.QuestionSentDate;
            ButtonName = chatBotQuestion.ButtonName;
            ButtonLink = chatBotQuestion.ButtonLink;
            ImageUrl = chatBotQuestion?.Photo?.ImageUrl;
        }

        public int Id { get; set; }
        public ChatBotQuestionType Type { get; set; }
        public string QuestionText { get; set; }
        // Time in seconds to delay displaying the question
        public int TimeDelay { get; set; }
        public string ButtonName { get; set; }
        public string ButtonLink { get; set; }
        public string ImageUrl { get; set; }
        public DateTime Timestamp { get; set; }

        public List<ChatBotAnswerResponseModel> Answers { get; set; }
    }
}