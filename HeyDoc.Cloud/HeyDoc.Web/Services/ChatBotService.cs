using HeyDoc.Web.Entity;
using HeyDoc.Web.Models.ChatBots;
using HeyDoc.Web.WebApi;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HeyDoc.Web.Services
{
    // NOTE: There is no need to check IsDeleted on ChatBotAnswers as there should be no case where a question has both deleted and active answers as creating such a case would break session histories
    public static class ChatBotService
    {
        public static ChatBotSessionResponseModel CreateChatSession(string accessToken, int doctorId, bool forceNewSession)
        {
            using (var db = new Entity.db_HeyDocEntities())
            {
                var user = AccountService.GetEntityUserByAccessToken(db, accessToken);
                var doctor = db.Doctors.Include(e => e.UserProfile).FirstOrDefault(e => e.UserId == doctorId);
                if (!db.Prescriptions.Any(e => e.PatientId == user.UserId && !e.IsDelete)) {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidAction, "No prior prescription history found. Please consult directly with a doctor"));
                }
                if (!doctor.IsChatBotEnabled)
                {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidAction, "Chat Assistant is not currently available for this doctor"));
                }
                if (!doctor.UserProfile.IsOnline)
                {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidAction, Resources.Chat.ErrorDoctorNotOnline));
                }
                var chatRoom = ChatService.GetChatRoomWithDoctor(db, user, doctorId);
                if (chatRoom == null)
                {
                    var timeNow = DateTime.UtcNow;
                    chatRoom = db.ChatRooms.Create();
                    db.ChatRooms.Add(chatRoom);
                    chatRoom.DoctorId = doctorId;
                    chatRoom.PatientId = user.UserId;
                    chatRoom.CreateDate = timeNow;
                    chatRoom.IsNotified = false;
                    chatRoom.IsDelete = false;
                    chatRoom.RequestStatus = RequestStatus.Completed;
                    chatRoom.LastUpdatedDate = timeNow;
                }
                
                // Check for still active sessions
                var chatBotSessions = GetActiveChatBotSessionsByChatRoomId(db, chatRoom.ChatRoomId);
                if (chatBotSessions.Count > 0 && !forceNewSession)
                {
                    var currentSession = chatBotSessions.OrderByDescending(e => e.CreatedDate).First();
                    return new ChatBotSessionResponseModel(currentSession, false);
                }
                chatBotSessions.ForEach(x => x.Status = ChatBotSessionStatus.CANCELLED);

                var newChatBotSession = new ChatBotSession { ChatRoomId = chatRoom.ChatRoomId, CreatedDate = DateTime.UtcNow, Status = ChatBotSessionStatus.ACTIVE, IsDeleted = false };
                db.ChatBotSessions.Add(newChatBotSession);

                db.SaveChanges();

                return new ChatBotSessionResponseModel(newChatBotSession, true);
            }
        }

        public static ChatBotQuestionResponseModel InitiateChatBotQuestion(string accessToken, int chatBotSessionId)
        {
            using (var db = new Entity.db_HeyDocEntities())
            {
                var user = AccountService.GetEntityUserByAccessToken(db, accessToken);

                var chatBotSession = GetChatBotSessionById(db, chatBotSessionId);
                if (chatBotSession == null || chatBotSession.ChatRoom.PatientId != user.UserId || chatBotSession.Status != ChatBotSessionStatus.ACTIVE)
                    throw new WebApiException(new WebApiError(WebApiErrorCode.NotFound, "Chat assistant session not found"));

                // Get question with AsNoTracking so that tokens in text can be modified without being saved back to the database
                var question = GetFirstChatBotQuestion(db, true);
                if (question == null)
                    throw new WebApiException(new WebApiError(WebApiErrorCode.NotFound, "Could not find initial question"));

                var replaceDict = new Dictionary<string, string>
                {
                    { "Username", chatBotSession.ChatRoom.Patient.UserProfile.FullName},
                    { "Doctorname", chatBotSession.ChatRoom.Doctor.UserProfile.FullName}
                };

                // Replace placeholder tokens in question and answers
                question = ReplaceQuestionAndAnswerTokens(db, chatBotSessionId, question, replaceDict);

                // Create ChatBotResponse to record the values used for the placeholder tokens
                var chatBotResponseTags = new List<ChatBotResponseTag>
                {
                    new ChatBotResponseTag
                    {
                        Tag = "Username"
                    },
                    new ChatBotResponseTag
                    {
                        Tag = "Doctorname"
                    }
                };

                var chatBotResponse = new ChatBotResponse
                {
                    ChatBotSessionId = chatBotSessionId,
                    QuestionSentDate = DateTime.UtcNow,
                    QuestionId = question.Id,
                    AnswerId = null,
                    JsonValue = JsonConvert.SerializeObject(replaceDict),
                    ChatBotResponseTags = chatBotResponseTags
                };
                db.ChatBotResponses.Add(chatBotResponse);
                db.SaveChanges();

                return new ChatBotQuestionResponseModel(question, chatBotResponse.QuestionSentDate);
            }
        }

        public static async Task<ChatBotQuestionResponseModel> GetNextQuestion(string accessToken, int chatBotSessionId, int questionId)
        {
            using (var db = new Entity.db_HeyDocEntities())
            {
                var user = AccountService.GetEntityUserByAccessToken(db, accessToken);

                var chatBotSession = GetChatBotSessionById(db, chatBotSessionId);
                if (chatBotSession == null || chatBotSession.ChatRoom.PatientId != user.UserId || chatBotSession.Status != ChatBotSessionStatus.ACTIVE)
                    throw new WebApiException(new WebApiError(WebApiErrorCode.NotFound, "Chat assistant session not found"));

                // Get question with AsNoTracking so that tokens in text can be modified without being saved back to the database
                var question = db.ChatBotQuestions
                    .AsNoTracking()
                    .Include(e => e.NextQuestion)
                    .Include(e => e.NextQuestion.ChildAnswers)
                    .FirstOrDefault(e => e.Id == questionId && !e.IsDeleted);
                if (question == null)
                    throw new WebApiException(new WebApiError(WebApiErrorCode.NotFound, "Question not found"));

                var lastChatBotResponse = GetLatestChatBotResponseForSession(db, chatBotSessionId);

                ChatBotQuestion nextQuestion = null;
                if (question.CheckApi.HasValue)
                {
                    var checkSuccess = false;
                    var checkAnswers = db.ChatBotAnswers
                        .AsNoTracking()
                        .Where(e => e.ParentQuestionId == questionId)
                        .Include(e => e.NextQuestion)
                        .Include(e => e.NextQuestion.ChildAnswers);
                    switch (question.CheckApi.Value)
                    {
                        case ChatBotCheckApi.CHECK_USER_HAS_PRESCRIPTION:
                            checkSuccess = db.Prescriptions.Any(x => x.PatientId == user.UserId && !x.IsDelete);
                            break;
                        case ChatBotCheckApi.CHECK_USER_IS_CORPORATE_USER:
                            checkSuccess = user.CorporateId != null;
                            if (checkSuccess)
                            {
                                // Record user's corporate into chat bot response
                                var data = new Dictionary<string, string>
                                {
                                    { "CorporateId", user.CorporateId.ToString() },
                                    { "CorporateName", user.Corporate.BranchName }
                                };
                                lastChatBotResponse.JsonValue = JsonConvert.SerializeObject(data);
                                lastChatBotResponse.ChatBotResponseTags = new List<ChatBotResponseTag>
                                {
                                    new ChatBotResponseTag { Tag = "CorporateId" },
                                    new ChatBotResponseTag { Tag = "CorporateName" }
                                };
                            }
                            break;
                        case ChatBotCheckApi.CHECK_USER_PROFILE_HAS_INFO_FOR_PRESCRIPTION:
                            checkSuccess = CheckUserInfoForPrescription(db, user);
                            break;
                        case ChatBotCheckApi.CHECK_USER_PRESCRIBED_MEDICATION_BEFORE:
                            {
                                var data = JsonConvert.DeserializeObject<Dictionary<string, string>>(GetLatestChatBotResponseByTag(db, chatBotSessionId, "MedicationId").JsonValue);
                                checkSuccess = CheckUserPrescribedMedication(db, user.UserId, Convert.ToInt64(data["MedicationId"]));
                                break;
                            }
                        case ChatBotCheckApi.CREATE_PRESCRIPTION:
                            string errorMessage;
                            (checkSuccess, errorMessage) = await PrescriptionService.CreatePrescriptionFromChatBot(db, user, chatBotSession);
                            if (!checkSuccess)
                            {
                                var data = new Dictionary<string, string>
                                {
                                    { "CreatePrescriptionError", errorMessage }
                                };
                                lastChatBotResponse.JsonValue = JsonConvert.SerializeObject(data);
                                lastChatBotResponse.ChatBotResponseTags = new List<ChatBotResponseTag>
                                {
                                    new ChatBotResponseTag { Tag = "CreatePrescriptionError" }
                                };
                            }
                            break;
                        case ChatBotCheckApi.GET_MEDICATION_SPECIFIC_QUESTIONS:
                            {
                                var data = JsonConvert.DeserializeObject<Dictionary<string, string>>(GetLatestChatBotResponseByTag(db, chatBotSessionId, "MedicationId").JsonValue);
                                checkSuccess = GetMedicationSpecificQuestions(checkAnswers, Convert.ToInt32(data["MedicationId"]), out nextQuestion);
                                break;
                            }
                        case ChatBotCheckApi.CHECK_MEDICATIONS_AVAILABLE:
                            {
                                var data = JsonConvert.DeserializeObject<Dictionary<string, string>>(GetLatestChatBotResponseByTag(db, chatBotSessionId, "MedicalConditionId").JsonValue);
                                checkSuccess = MedicationService.GetMedicationsByMedicalConditionId(db, Convert.ToInt32(data["MedicalConditionId"]), 0, 1).Count() > 0;
                                break;
                            }
                    }
                    if (nextQuestion == null)
                    {
                        nextQuestion = checkSuccess ?
                            checkAnswers.FirstOrDefault(e => e.Type == ChatBotAnswerType.CHECK_SUCCESS).NextQuestion
                                : checkAnswers.FirstOrDefault(e => e.Type == ChatBotAnswerType.CHECK_FAILED).NextQuestion;
                    }
                }
                else
                {
                    nextQuestion = question.NextQuestion;
                }
                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        lastChatBotResponse.ResponseReceivedDate = DateTime.UtcNow;
                        db.SaveChanges();
                        // Replace placeholder tokens in next question and its answers
                        nextQuestion = ReplaceQuestionAndAnswerTokens(db, chatBotSessionId, nextQuestion);

                        var chatBotResponse = new ChatBotResponse
                        {
                            ChatBotSessionId = chatBotSessionId,
                            QuestionSentDate = DateTime.UtcNow,
                            QuestionId = nextQuestion.Id,
                            AnswerId = null,
                            JsonValue = null,
                            ChatBotResponseTags = null
                        };
                        db.ChatBotResponses.Add(chatBotResponse);

                        // end chatsession if it is the last question
                        if (nextQuestion.Type == ChatBotQuestionType.LAST_FAIL_DEFAULT || nextQuestion.Type == ChatBotQuestionType.LAST_SUCCESS_DEFAULT || nextQuestion.Type == ChatBotQuestionType.LAST)
                        {
                            chatBotSession.Status = ChatBotSessionStatus.ENDED;
                            ChatService.CreateChatBotSessionHistoryMessage(db, chatBotSession);
                        }
                        db.SaveChanges();

                        transaction.Commit();
                        return new ChatBotQuestionResponseModel(nextQuestion, chatBotResponse.QuestionSentDate);
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public static ChatBotQuestionResponseModel Interact(string accessToken, int chatBotSessionId, int questionId, int answerId, Dictionary<string, string> answerData)
        {
            using (var db = new Entity.db_HeyDocEntities())
            {
                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        var user = AccountService.GetEntityUserByAccessToken(db, accessToken);

                        var chatBotSession = GetChatBotSessionById(db, chatBotSessionId);
                        if (chatBotSession == null || chatBotSession.ChatRoom.PatientId != user.UserId || chatBotSession.Status != ChatBotSessionStatus.ACTIVE)
                            throw new WebApiException(new WebApiError(WebApiErrorCode.NotFound, "Chat assistant session not found"));

                        var answer = GetChatBotAnswerById(db, answerId, true);
                        if (answer == null)
                            throw new WebApiException(new WebApiError(WebApiErrorCode.NotFound, "Answer not found"));

                        if (answer.ParentQuestionId != questionId)
                            // TODO M UNBLANK: Support email
                            throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidAction, "Unexpected answer for question. Please try answering again or restarting the chat bot session. If the issue persists, contact support at {support email}"));

                        // rollback here
                        if (!string.IsNullOrEmpty(answer.RollbackTags))
                        {
                            foreach (var rollbackTag in answer.RollbackTags.Split(','))
                            {
                                var latestChatBotResponse = GetLatestChatBotResponseByTag(db, chatBotSessionId, rollbackTag);
                                latestChatBotResponse.IsRolledback = true;
                            }
                            db.SaveChanges();
                        }

                        // Extract response answers for tags, if question has any
                        string jsonString = null;
                        var chatBotResponseTags = new List<ChatBotResponseTag>();

                        if (!string.IsNullOrEmpty(answer.ParentQuestion.Tag))
                        {
                            if (answerData == null)
                            {
                                answerData = JsonConvert.DeserializeObject<Dictionary<string, string>>(answer.JsonValue);
                            }
                            var tags = answer.ParentQuestion.Tag.Split(',');
                            if (!tags.All(t => answerData.ContainsKey(t)))
                            {
                                throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidAction, "Something went wrong"));
                            }

                            // Filter out any extraneous data since this will be serialised and stored in database
                            answerData = answerData.Where(kv => tags.Contains(kv.Key)).ToDictionary(kv => kv.Key, kv => kv.Value);
                            foreach (var tag in tags)
                            {
                                chatBotResponseTags.Add(new ChatBotResponseTag
                                {
                                    Tag = tag
                                });
                            }

                            jsonString = JsonConvert.SerializeObject(answerData);
                        }

                        // Update response with answer data
                        var lastChatBotResponse = GetLatestChatBotResponseForSession(db, chatBotSessionId);
                        lastChatBotResponse.AnswerId = answerId;
                        lastChatBotResponse.ResponseReceivedDate = DateTime.UtcNow;
                        lastChatBotResponse.JsonValue = jsonString ?? lastChatBotResponse.JsonValue;
                        lastChatBotResponse.ChatBotResponseTags = chatBotResponseTags;
                        db.SaveChanges();

                        // get next question from answer
                        var nextQuestion = answer.NextQuestion;

                        // Replace placeholder tokens in next question and its answers
                        nextQuestion = ReplaceQuestionAndAnswerTokens(db, chatBotSessionId, nextQuestion);

                        var chatBotResponse = new ChatBotResponse
                        {
                            ChatBotSessionId = chatBotSessionId,
                            QuestionSentDate = DateTime.UtcNow,
                            QuestionId = nextQuestion.Id,
                            AnswerId = null,
                            JsonValue = null,
                            ChatBotResponseTags = null
                        };
                        db.ChatBotResponses.Add(chatBotResponse);

                        // end chatsession if it is the last question
                        if (nextQuestion.Type == ChatBotQuestionType.LAST_FAIL_DEFAULT || nextQuestion.Type == ChatBotQuestionType.LAST_SUCCESS_DEFAULT || nextQuestion.Type == ChatBotQuestionType.LAST)
                        {
                            chatBotSession.Status = ChatBotSessionStatus.ENDED;
                            ChatService.CreateChatBotSessionHistoryMessage(db, chatBotSession);
                        }
                        db.SaveChanges();

                        transaction.Commit();
                        return new ChatBotQuestionResponseModel(nextQuestion, chatBotResponse.QuestionSentDate);
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public static List<ChatBotQuestionResponseModel> GetChatBotSessionHistory(string accessToken, int chatBotSessionId)
        {
            using (var db = new db_HeyDocEntities())
            {
                var user = AccountService.GetEntityUserByAccessToken(db, accessToken);

                var chatBotSession = GetChatBotSessionById(db, chatBotSessionId);
                if (chatBotSession == null || (chatBotSession.ChatRoom.PatientId != user.UserId && chatBotSession.ChatRoom.DoctorId != user.UserId))
                    throw new WebApiException(new WebApiError(WebApiErrorCode.NotFound, "Chat assistant session not found"));

                // Use AsNoTracking so that tokens in text can be modified without being saved back to the database
                var chatBotResponses = db.ChatBotResponses
                    .AsNoTracking()
                    .Where(e => e.ChatBotSessionId == chatBotSessionId)
                    .Include(e => e.ChatBotQuestion)
                    .Include(e => e.ChatBotQuestion.ChildAnswers)
                    .Include(e => e.ChatBotResponseTags)
                    .ToList();
                foreach (var response in chatBotResponses)
                {
                    var question = response.ChatBotQuestion;
                    if (question.TextHasPlaceholderTokens)
                    {
                        question.QuestionText = ReplaceStringWithTagsForPastMessage(chatBotResponses, question.QuestionText, response.QuestionSentDate);
                    }
                    foreach (var answer in question.ChildAnswers)
                    {
                        if (response.AnswerId == answer.Id && answer.AnswerSelectedText != null)
                        {
                            answer.AnswerText = ReplaceStringWithTagsForPastMessage(chatBotResponses, answer.AnswerSelectedText, response.QuestionSentDate);
                        }
                        else if (answer.TextHasPlaceholderTokens)
                        {
                            answer.AnswerText = ReplaceStringWithTagsForPastMessage(chatBotResponses, answer.AnswerText, response.QuestionSentDate);
                        }
                    }
                }
                return chatBotResponses.Select(e => new ChatBotQuestionResponseModel(e)).ToList();
            }
        }

        private static ChatBotQuestion ReplaceQuestionAndAnswerTokens(db_HeyDocEntities db, int chatBotSessionId, ChatBotQuestion question, Dictionary<string, string> replacementDict = null)
        {
            if (question.TextHasPlaceholderTokens)
            {
                question.QuestionText = ReplaceStringWithTags(db, chatBotSessionId, question.QuestionText, replacementDict);
            }
            if (question.ChildAnswers != null)
            {
                foreach (var answer in question.ChildAnswers)
                {
                    if (answer.TextHasPlaceholderTokens)
                    {
                        answer.AnswerText = ReplaceStringWithTags(db, chatBotSessionId, answer.AnswerText, replacementDict);
                    }
                    if (answer.ApiEndpoint != null && answer.ApiEndpointHasPlaceholderTokens)
                    {
                        answer.ApiEndpoint = ReplaceStringWithTags(db, chatBotSessionId, answer.ApiEndpoint, replacementDict);
                    }
                }
            }
            return question;
        }

        private static string ReplaceStringWithTags(db_HeyDocEntities db, int chatBotSessionId, string targetString, Dictionary<string, string> replacementDict = null)
        {
            var resultString = Regex.Replace(targetString, @"{(.+?)}", (m) =>
                {
                    var tag = m.Groups[1].Value;
                    if (replacementDict != null && replacementDict.TryGetValue(tag, out var replacement))
                    {
                        return replacement;
                    }
                    else
                    {
                        var response = GetLatestChatBotResponseByTag(db, chatBotSessionId, tag);
                        var responseData = JsonConvert.DeserializeObject<Dictionary<string, string>>(response.JsonValue);
                        if (responseData.TryGetValue(tag, out replacement))
                        {
                            return replacement;
                        }
                        else
                        {
                            return m.Groups[0].Value;
                        }
                    }
                });

            return resultString;
        }

        private static string ReplaceStringWithTagsForPastMessage(ICollection<ChatBotResponse> chatBotResponses, string targetString, DateTime messageTime)
        {
            var resultString = Regex.Replace(targetString, @"{(.+?)}", (m) =>
                {
                    var tag = m.Groups[1].Value;
                    // Have to find latest response that is before the message we are running substitutions for because tags may appear multiple times in a chat bot session
                    var response = chatBotResponses.OrderByDescending(e => e.QuestionSentDate)
                        .FirstOrDefault(e => e.QuestionSentDate <= messageTime && e.ChatBotResponseTags.Any(x => x.Tag == tag));
                    var responseData = JsonConvert.DeserializeObject<Dictionary<string, string>>(response.JsonValue);
                    if (responseData.TryGetValue(tag, out var replacement))
                    {
                        return replacement;
                    }
                    else
                    {
                        return m.Groups[0].Value;
                    }
                });
            return resultString;
        }

        public static List<ChatBotSession> GetActiveChatBotSessionsByChatRoomId(Entity.db_HeyDocEntities db, int chatRoomId)
        {
            return db.ChatBotSessions
                      .Where(x => x.ChatRoomId == chatRoomId &&
                                  x.Status == ChatBotSessionStatus.ACTIVE &&
                                  !x.IsDeleted)
                      .ToList();
        }

        public static ChatBotSession GetChatBotSessionById(Entity.db_HeyDocEntities db, int id)
        {
            return db.ChatBotSessions
                     .Include(e => e.ChatRoom)
                     .FirstOrDefault(x => x.Id == id && !x.IsDeleted);
        }

        public static ChatBotQuestion GetFirstChatBotQuestion(Entity.db_HeyDocEntities db, bool noTracking)
        {
            var chatBotQuestions = noTracking ? db.ChatBotQuestions.AsNoTracking() : db.ChatBotQuestions;
            return chatBotQuestions
                     .Include(e => e.ChildAnswers)
                     .FirstOrDefault(x => x.Type == ChatBotQuestionType.FIRST && !x.IsDeleted);
        }

        public static ChatBotQuestion GetLastChatBotQuestionForFailedChat(Entity.db_HeyDocEntities db, bool noTracking)
        {
            var chatBotQuestions = noTracking ? db.ChatBotQuestions.AsNoTracking() : db.ChatBotQuestions;
            return chatBotQuestions
                     .Include(e => e.ChildAnswers)
                     .FirstOrDefault(x => x.Type == ChatBotQuestionType.LAST_FAIL_DEFAULT && !x.IsDeleted);
        }

        public static ChatBotAnswer GetChatBotAnswerById(Entity.db_HeyDocEntities db, int id, bool noTracking)
        {
            var chatBotAnswers = noTracking ? db.ChatBotAnswers.AsNoTracking() : db.ChatBotAnswers;
            return chatBotAnswers
                     .Include(e => e.ParentQuestion)
                     .Include(e => e.NextQuestion)
                     .Include(e => e.NextQuestion.ChildAnswers)
                     .FirstOrDefault(x => x.Id == id && !x.IsDeleted);
        }

        public static ChatBotResponse GetLatestChatBotResponseForSession(db_HeyDocEntities db, int chatBotSessionId)
        {
            return db.ChatBotResponses.OrderByDescending(e => e.QuestionSentDate).FirstOrDefault(e => e.ChatBotSessionId == chatBotSessionId);
        }

        public static ChatBotResponse GetLatestChatBotResponseByTag(Entity.db_HeyDocEntities db, int chatBotSessionId, string tag)
        {
            return GetChatBotResponsesByTag(db, chatBotSessionId, tag).OrderByDescending(x => x.QuestionSentDate).FirstOrDefault();
        }

        public static IQueryable<ChatBotResponse> GetChatBotResponsesByTag(Entity.db_HeyDocEntities db, int chatBotSessionId, string tag)
        {
            return db.ChatBotResponses
                     .Where(x => x.ChatBotSessionId == chatBotSessionId &&
                                 x.ChatBotResponseTags.Any(y => y.Tag == tag) &&
                                 !x.IsRolledback);
        }

        public static bool CheckUserInfoForPrescription(db_HeyDocEntities db, UserProfile user)
        {
            return !(string.IsNullOrEmpty(user.Patient.UserProfile.IC) || !user.Gender.HasValue || !Enum.IsDefined(typeof(Gender), user.Gender.Value) || string.IsNullOrEmpty(user.Patient.UserProfile.Address) || string.IsNullOrEmpty(user.Patient.Allergy));
        }

        // check if the user has been prescribed with a medication before
        private static bool CheckUserPrescribedMedication(Entity.db_HeyDocEntities db, int userId, long medicationId)
        {
            var prescriptions = PrescriptionService.GetPrescriptionsByMedicationId(db, userId, medicationId);
            return prescriptions.Count > 0;
        }

        private static bool GetMedicationSpecificQuestions(IEnumerable<ChatBotAnswer> chatBotAnswers, int medicationId, out ChatBotQuestion nextQuestion)
        {
            nextQuestion = chatBotAnswers.FirstOrDefault(e => e.ChatBotAnswerKeys.Select(x => x.RelatedId).Contains(medicationId))?.NextQuestion;
            return false;
        }
    }
}