using HeyDoc.Web.Models;
using HeyDoc.Web.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace HeyDoc.Web.Services
{
    public class PatientGoalService
    {
        public static async Task<PatientGoalModel> SetGoal(string accessToken, int chatRoomId, PatientGoalModel model)
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityUser = AccountService.GetEntityUserByAccessToken(db, accessToken, false, RoleType.Doctor);

                var entityChatRoom = db.ChatRooms.FirstOrDefault(e => e.ChatRoomId == chatRoomId && e.DoctorId == entityUser.UserId);

                if (entityChatRoom == null)
                {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.NotFound));
                }

                var entityGoal = db.PatientGoals.Create();
                entityGoal.ChatRoomId = chatRoomId;
                entityGoal.Description = model.Description;
                entityGoal.Duration = model.Duration;
                entityGoal.StartTime = model.StartTime;
                entityGoal.EndTime = model.EndTime;
                entityGoal.IsLifeTime = model.IsLifeTime;
                entityGoal.CreateDate = model.CreateDate;

                db.PatientGoals.Add(entityGoal);
                db.SaveChanges();

                ChatService.PushToUsers(db, entityUser.Nickname, entityChatRoom.PatientId, PnActionType.Goal, entityChatRoom.ChatRoomId, string.Format("New Goal set by {0}", entityUser.Nickname));
                await NotificationService.NotifyUser(db, entityChatRoom.PatientId, PnActionType.Goal, entityChatRoom.ChatRoomId, string.Format("New Goal set by {0}", entityUser.FullName));

                return new PatientGoalModel(entityGoal);
            }
        }

        public static BoolResult SetCompleteGoal(string accessToken, long goalId, bool isComplete)
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityUser = AccountService.GetEntityUserByAccessToken(db, accessToken, false);

                var entityGoal = db.PatientGoals.FirstOrDefault(e => e.GoalId == goalId);

                if (entityGoal == null)
                {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.NotFound));
                }

                entityGoal.IsComplete = isComplete;
                db.SaveChanges();

                return new BoolResult(true);

            }
        }

        public static List<PatientGoalModel> GetGoalList(string accessToken, int chatRoomId, int take = 15, int skip = 0)
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityUser = AccountService.GetEntityUserByAccessToken(db, accessToken, false);

                var entityGoalList = db.PatientGoals.Where(e => e.ChatRoomId == chatRoomId );
                if (entityUser.Role == RoleType.User)
                {
                    entityGoalList = entityGoalList.Where(e => e.ChatRoom.PatientId == entityUser.UserId);
                }

                entityGoalList = entityGoalList.OrderBy(e => e.IsComplete).ThenBy(e => e.IsLifeTime).ThenBy(e => e.Duration).Skip(skip).Take(take);

                List<PatientGoalModel> result = new List<PatientGoalModel>();

                foreach (var entityGoal in entityGoalList)
                {
                    var model = new PatientGoalModel(entityGoal);
                    model.RemarkCount = db.Remarks.Count(x => x.GoalId == entityGoal.GoalId && !x.IsDeleted);
                    model.ChatRoomId = chatRoomId;
                    result.Add(model);
                }

                return result;
            }
        }

        public static BoolResult DeleteGoal(string accessToken, long goalId)
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityUser = AccountService.GetEntityUserByAccessToken(db, accessToken, false);

                var entityGoal = db.PatientGoals.FirstOrDefault(e => e.GoalId == goalId);

                if (entityGoal == null)
                {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.NotFound));
                }

                var entityChatRoom = entityGoal.ChatRoom;

                if (entityChatRoom == null && entityChatRoom.PatientId != entityUser.UserId || entityChatRoom.DoctorId != entityUser.UserId)
                {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.NotFound));
                }

                db.PatientGoals.Remove(entityGoal);
                db.SaveChanges();

                return new BoolResult(true);

            }
        }
        public static async Task<RemarkModel> CreateRemark(string accessToken, RemarkModel model)
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                model.Validate();
                var entityUser = AccountService.GetEntityUserByAccessToken(db, accessToken, false);                

                var entityGoal = db.PatientGoals.FirstOrDefault(e => e.GoalId == model.GoalId);
                if (entityGoal == null)
                {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.NotFound));
                }

                var entityRemark = db.Remarks.Create();
                entityRemark.GoalId = model.GoalId;
                entityRemark.UserId = entityUser.UserId;
                entityRemark.Remarks = model.Remarks;
                entityRemark.CreatedDate = DateTime.UtcNow;
                entityRemark.IsDeleted = false;
                db.Remarks.Add(entityRemark);
                db.SaveChanges();
                var entityRemarkUserList = db.Remarks.Where(x => x.GoalId == model.GoalId && x.UserId != entityUser.UserId).Select(x => x.UserId).Distinct().ToList();
                if (entityGoal.ChatRoom.DoctorId != entityUser.UserId)
                {
                    entityRemarkUserList.Add(entityGoal.ChatRoom.DoctorId);
                }
                if (entityGoal.ChatRoom.PatientId != entityUser.UserId)
                {
                    entityRemarkUserList.Add(entityGoal.ChatRoom.PatientId);
                }
                await NotificationService.NotifyUser(db, entityRemarkUserList, PnActionType.Remark, entityGoal.ChatRoom.ChatRoomId.ToString(), 
                    string.Format("A new remark is added under Goal - {0}", entityGoal.Description).Truncate(50));

                //foreach (var userId in entityRemarkUserList)
                //{
                //    ChatService.PushToUsers(db, entityUser.Nickname, userId, PnActionType.Remark, entityGoal.ChatRoomId, string.Format("New Remark set by {0}", entityUser.FullName));
                //}
                //ChatService.PushToUsers(db, entityUser.Nickname, entityGoal.ChatRoom.PatientId, PnActionType.Remark, entityGoal.ChatRoomId, string.Format("New Remark set by {0}", entityUser.FullName));
               
                return new RemarkModel(entityRemark);
            }
        }
        public static List<RemarkModel> GetRemarks(string accessToken, long goalId, int take , int skip)
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityUser = AccountService.GetEntityUserByAccessToken(db, accessToken, false);
                var entityGoal = db.PatientGoals.FirstOrDefault(e => e.GoalId == goalId);
                if (entityGoal == null)
                {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.NotFound));
                }
                var modelList = new List<RemarkModel>();
                var entityRemarkList = db.Remarks.Where(x => x.GoalId == goalId && !x.IsDeleted);
                entityRemarkList = entityRemarkList.OrderByDescending(x => x.CreatedDate).Skip(skip).Take(take);
                foreach (var entityRemark in entityRemarkList)
                {
                    modelList.Add(new RemarkModel(entityRemark));
                }
                return modelList;

            }
        }
        public static RemarkModel GetRemark(string accessToken, long goalId, long remarkId)
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityUser = AccountService.GetEntityUserByAccessToken(db, accessToken, false);
                var entityGoal = db.PatientGoals.FirstOrDefault(e => e.GoalId == goalId);
                if (entityGoal == null)
                {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.NotFound));
                }
                var modelList = new List<RemarkModel>();
                var entityRemark = db.Remarks.FirstOrDefault(x => x.GoalId == goalId && !x.IsDeleted && x.RemarkId == remarkId);
                if (entityRemark == null)
                {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.NotFound));

                }
                return new RemarkModel(entityRemark);

            }
        }
        public static BoolResult DeleteRemark(string accessToken, long goalId, long remarkId)
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityUser = AccountService.GetEntityUserByAccessToken(db, accessToken, false);

                var entityGoal = db.PatientGoals.FirstOrDefault(e => e.GoalId == goalId);

                if (entityGoal == null)
                {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.NotFound));
                }

                var entityRemark = db.Remarks.FirstOrDefault(x => x.GoalId == goalId && !x.IsDeleted && x.RemarkId == remarkId&&x.UserId==entityUser.UserId);
                if (entityRemark == null)
                {
                    return new BoolResult(false);
                }
                entityRemark.IsDeleted = true;
                db.SaveChanges();
                return new BoolResult(true);

            }
        }
    }
}