using HeyDoc.Web.Lib;
using HeyDoc.Web.Models;
using HeyDoc.Web.Models.ChatBots;
using HeyDoc.Web.Services;
using HeyDoc.Web.WebApi;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.ValueProviders;
using System.Web.Http.ValueProviders.Providers;

namespace HeyDoc.Web.Controllers.Api
{
    public class ChatBotController : ApiController
    {
        [HttpGet]
        public ChatBotSessionResponseModel CreateChatSession([ValueProvider(new[] { typeof(HeaderValueProviderFactory), typeof(QueryStringValueProviderFactory) })] string accessToken, int doctorId, bool forceNewSession)
        {
            return WebApiWrapper.Call(e => ChatBotService.CreateChatSession(accessToken, doctorId, forceNewSession));
        }

        [HttpPost]
        public Task<ChatBotQuestionResponseModel> Interact([ValueProvider(new[] { typeof(HeaderValueProviderFactory), typeof(QueryStringValueProviderFactory) })] string accessToken, [FromBody]ChatBotInteractRequestModel model)
        {
            if (model.QuestionId == 0) //first question
            {
                return WebApiWrapper.CallIntoTask(e => ChatBotService.InitiateChatBotQuestion(accessToken, model.ChatSessionId));
            }
            else if (model.AnswerId == null || model.AnswerId == 0) //consecutive question
            {
                return WebApiWrapper.CallAsync(() => ChatBotService.GetNextQuestion(accessToken, model.ChatSessionId, model.QuestionId));
            }
            else
            {
                return WebApiWrapper.CallIntoTask(e => ChatBotService.Interact(accessToken, model.ChatSessionId, model.QuestionId, model.AnswerId.Value, model.AnswerData));
            }
        }

        [HttpGet]
        public List<ChatBotSelectListResponseModel> GetMedicinesByMedicalConditionId([ValueProvider(new[] { typeof(HeaderValueProviderFactory), typeof(QueryStringValueProviderFactory) })] string accessToken, int id)
        {
            using (var db = new Entity.db_HeyDocEntities())
            {
                // Just attempt to get user from access token to check token validity
                AccountService.GetEntityUserByAccessToken(db, accessToken);
                var medicines = MedicationService.GetMedicationsByMedicalConditionId(db, id, 0, -1);
                var medicinesSelection = medicines
                    .AsEnumerable()
                    .Select(x => new ChatBotSelectListResponseModel { Value = new Dictionary<string, string> { { "MedicationId", x.MedicationId.ToString() }, { "MedicationName", x.MedicationName } }, Name = x.MedicationName });
                return WebApiWrapper.Call(e => medicinesSelection.ToList());
            }
        }
        
        [HttpGet]
        public List<ChatBotSelectListResponseModel> GetMedicalConditions([ValueProvider(new[] { typeof(HeaderValueProviderFactory), typeof(QueryStringValueProviderFactory) })] string accessToken)
        {
            using (var db = new Entity.db_HeyDocEntities())
            {
                // Just attempt to get user from access token to check token validity
                AccountService.GetEntityUserByAccessToken(db, accessToken);
                var medicalConditionsSelection = db.MedicalConditions
                    .Where(e => !e.IsDeleted)
                    .OrderBy(e => e.Name)
                    .AsEnumerable()
                    .Select(e => new ChatBotSelectListResponseModel { Value = new Dictionary<string, string> { { "MedicalConditionId", e.Id.ToString() }, { "MedicalConditionName", e.Name } }, Name = e.Name });
                return WebApiWrapper.Call(e => medicalConditionsSelection.ToList());
            }
        }

        [HttpGet]
        public List<ChatBotQuestionResponseModel> GetChatBotSessionHistory([ValueProvider(new[] { typeof(HeaderValueProviderFactory), typeof(QueryStringValueProviderFactory) })] string accessToken, int chatBotSessionId)
        {
            return ChatBotService.GetChatBotSessionHistory(accessToken, chatBotSessionId);
        }
    }
}