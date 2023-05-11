using HeyDoc.Web.Resources;
using HeyDoc.Web.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HeyDoc.Web.Models
{
    public class QuestionModel
    {
        public long MedicationId { get; set; }
        public int QuestionId { get; set; }
        public string Question { get; set; }
        public Answer PreferredAnswer { get; set; }
        public bool IsDelete { get; set; }

        public UserAnswerModel UserAnswer { get; set; }

        public QuestionModel()
        {

        }

        public QuestionModel(Entity.MedicationQuestion medicationQuestion)
        {
            MedicationId = medicationQuestion.MedicationId;
            QuestionId = medicationQuestion.QuestionID;
            Question = medicationQuestion.Question;
            PreferredAnswer = medicationQuestion.PreferredAnswer;
            IsDelete = medicationQuestion.IsDelete;
        }

        public void Validate()
        {
            if (string.IsNullOrEmpty(Question))
            {
                throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidArguments, Forms.ErrorQuestionTextNull));
            }
            if(Question.Length > 499)
            {
                throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidArguments, "Question text length should be less than 400 characters."));
            }
        }
    }

    public class UserAnswerModel
    {
        public int QuestionId { get; set; }
        public int AnswerId { get; set; }
        public Answer UserAnswer { get; set; }

        public UserAnswerModel() { }

        public UserAnswerModel(Entity.MedicationUserAnswer entityMedicationUserAnswer)
        {
            QuestionId = entityMedicationUserAnswer.QuestionID;
            AnswerId = entityMedicationUserAnswer.AnswerId;
            UserAnswer = entityMedicationUserAnswer.UserAnswer;
        }
    }
}