using HeyDoc.Web.Entity;
using HeyDoc.Web.Helpers;
using HeyDoc.Web.Models;
using HeyDoc.Web.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using HeyDoc.Web.Resources;

namespace HeyDoc.Web.Services
{
    public class TeamService
    {
        public static List<DoctorModel> GetDoctorList(string accessToken, string searchString, int? categoryId = null, long groupId = 0, int take = 15, int skip = 0)
        {
            ActivityAuditHelper.AddRequestParamsToLog();
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityUser = AccountService.GetEntityUserByAccessToken(db, accessToken, true);
                var catId = GetCategoryForPatient(entityUser, accessToken, categoryId);

                return GetDoctorList(db, entityUser, searchString, catId, groupId, take, skip);
            }
        }

        internal static List<DoctorModel> GetDoctorListNoLogin(int companyId, string accessToken, string searchString, int? categoryId, long groupId, int take, int skip)
        {
            ActivityAuditHelper.AddRequestParamsToLog();
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityUser = AccountService.GetEntityUserByAccessToken(db, accessToken, true);
                var catId = GetCategoryForPatient(entityUser, accessToken, categoryId);
                var entityDoctorList = db.Doctors.Where(e => !e.UserProfile.IsDelete && e.IsVerified && !e.UserProfile.IsBan && e.ShowInApp && e.UserProfile.CompanyId == companyId);
                if (categoryId.HasValue)
                {
                    entityDoctorList = entityDoctorList.Where(e => e.CategoryId == categoryId.Value);
                }

                if (groupId == 0)
                {
                    if (!categoryId.HasValue)
                    {
                        // Don't allow this as it would return doctors from multiple categories
                        throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidArguments, "Cannot get independent group without category"));
                    }
                    entityDoctorList = entityDoctorList.Where(e => e.GroupId == null);
                }
                else
                {
                    entityDoctorList = entityDoctorList.Where(e => e.GroupId == groupId);
                }

                if (!string.IsNullOrEmpty(searchString))
                {
                    entityDoctorList = entityDoctorList.Where(e => e.Specialty.Contains(searchString) || e.UserProfile.FullName.Contains(searchString));
                    entityDoctorList = entityDoctorList.OrderByDescending(e => e.UserProfile.IsOnline).ThenByDescending(e => e.UserProfile.LastActivityDate).ThenBy(e => e.UserProfile.FullName).Skip(skip).Take(take);
                }
                else
                {
                    entityDoctorList = entityDoctorList.OrderByDescending(e => e.UserProfile.IsOnline).ThenByDescending(e => e.UserProfile.LastActivityDate).Skip(skip).Take(take);
                }

                List<DoctorModel> result = new List<DoctorModel>();
                foreach (var entityDoctor in entityDoctorList)
                {
                    result.Add(new DoctorModel(entityDoctor.UserProfile, entityDoctor));
                }
                return result;
            }
        }

        private static List<DoctorModel> GetDoctorList(Entity.db_HeyDocEntities db, Entity.UserProfile entityUser, string searchString, int? categoryId, long groupId, int take, int skip)
        {
            var entityDoctorList = db.Doctors.Where(e => !e.UserProfile.IsDelete && e.IsVerified && !e.UserProfile.IsBan && e.ShowInApp && ((entityUser.CompanyWhiteLabel.CompanyType == "w") || (e.UserProfile.CompanyId == entityUser.CompanyId && entityUser.CompanyWhiteLabel.CompanyType == "c")));
            if (categoryId.HasValue)
            {
                entityDoctorList = entityDoctorList.Where(e => e.CategoryId == categoryId.Value);
            }

            if (groupId == 0)
            {
                if (!categoryId.HasValue)
                {
                    // Don't allow this as it would return doctors from multiple categories
                    throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidArguments, "Cannot get independent group without category"));
                }
                entityDoctorList = entityDoctorList.Where(e => e.GroupId == null);
            }
            else
            {
                entityDoctorList = entityDoctorList.Where(e => e.GroupId == groupId);
            }

            if (!string.IsNullOrEmpty(searchString))
            {
                entityDoctorList = entityDoctorList.Where(e => e.Specialty.Contains(searchString) || e.UserProfile.FullName.Contains(searchString));
                entityDoctorList = entityDoctorList.OrderByDescending(e => e.UserProfile.IsOnline).ThenByDescending(e => e.UserProfile.LastActivityDate).ThenBy(e => e.UserProfile.FullName).Skip(skip).Take(take);
            }
            else
            {
                entityDoctorList = entityDoctorList.OrderByDescending(e => e.UserProfile.IsOnline).ThenByDescending(e => e.UserProfile.LastActivityDate).Skip(skip).Take(take);
            }

            List<DoctorModel> result = new List<DoctorModel>();
            foreach (var entityDoctor in entityDoctorList)
            {
                result.Add(new DoctorModel(entityDoctor.UserProfile, entityDoctor));
            }
            return result;
        }

        public static DoctorModel GetDoctor(string accessToken, int doctorId)
        {
            ActivityAuditHelper.AddRequestParamsToLog();
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                //var entityUser = AccountService.GetEntityUserByAccessToken(db, accessToken,true);

                var entityDoctor = AccountService.GetEntityTargetUserByUserId(db, doctorId, false);
                if (entityDoctor.Doctor == null)
                {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.NotFound));
                }

                int attendedUser = (int)(entityDoctor.Doctor.DoctorStat?.AttendedUsers ?? 0);

                int avgRating = (int)(entityDoctor.Doctor.DoctorStat?.AverageRating ?? 0);

                return new DoctorModel(entityDoctor.Doctor, attendedUser, entityDoctor.DoctorUserReviews.Count(), avgRating);
            }
        }

        public static DoctorModel GetRandomDoctor(string accessToken)
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityUser = AccountService.GetEntityUserByAccessToken(db, accessToken, false);

                // Random doctor only user is first time using the free package, to prevent user exit and re-enter to choose doctor
                var entityLastChat = db.ChatRooms.Where(e => e.PatientId == entityUser.UserId && e.Doctor.UserProfile.IsOnline).OrderByDescending(e => e.LastChatId).FirstOrDefault();

                Entity.Doctor entityDoctor = null;
                if (entityLastChat == null || entityLastChat.Doctor.UserProfile.IsDelete)
                {
                    entityDoctor = db.Doctors.Where(e => !e.UserProfile.IsDelete && e.UserProfile.IsOnline ).OrderBy(r => Guid.NewGuid()).FirstOrDefault();
                    if (entityDoctor == null)
                    {
                        entityDoctor = db.Doctors.Where(e => !e.UserProfile.IsDelete && e.UserProfile.IsOnline).OrderBy(r => Guid.NewGuid()).FirstOrDefault();
                        if (entityDoctor == null)
                        {
                            //Send Mail
                            entityDoctor = new Entity.Doctor();
                            entityDoctor.UserProfile = new Entity.UserProfile();
                            entityDoctor.UserProfile.Photo = new Entity.Photo();
                            entityDoctor.UserProfile.Country = new Entity.Country();
                            entityDoctor.Specialty = string.Empty;
                            entityDoctor.MedicalSch = string.Empty;
                            entityDoctor.AboutMe = string.Empty;
                            entityDoctor.UserProfile.Nickname = string.Empty;
                            entityDoctor.UserProfile.UserName = string.Empty;
                            entityDoctor.UserProfile.Birthday = new DateTime();
                            entityDoctor.UserProfile.Language = string.Empty;
                            entityDoctor.UserProfile.Country.CountryName = string.Empty;
                            entityDoctor.UserProfile.Country.CountryCode = string.Empty;
                            var lastChat = db.ChatRooms.Where(e => e.PatientId == entityUser.UserId).OrderByDescending(e => e.LastChatId).FirstOrDefault();
                            var entityPatient = lastChat.Patient.UserProfile;
                            var entityLastDoc = lastChat.Doctor.UserProfile;
                            var lastChatList = db.Chats.Where(x => x.ChatRoomId == lastChat.ChatRoomId && !x.IsDelete).OrderByDescending(x => x.CreateDate).Skip(0).Take(10);
                            StringBuilder chat = new StringBuilder();
                            chat.Append("<table style=\"margin-left: 0%; border-spacing: 0; border-collapse: collapse; vertical-align: top; height: 50px; width: 77%; table-layout: fixed;\"><tbody>");
                            foreach (var entityChat in lastChatList)
                            {
                                chat.Append("<tr><td><b>");
                                //chat.Append("Doctor :");
                                chat.Append(entityChat.UserProfile.FullName);
                                chat.Append(" (");
                                chat.Append(entityChat.CreateDate.AddHours(8));
                                chat.Append(") :");
                                chat.Append("</b></td>");
                                if (entityChat.MessageType == MessageType.Photo)
                                {
                                    if (entityChat.Photo != null)
                                    {
                                        chat.Append("<td><a  href=\"" + entityChat.Photo.ImageUrl + "\" >Image</a>");
                                    }
                                    chat.Append("</td>");

                                }
                                if (entityChat.MessageType == MessageType.Message)
                                {
                                    chat.Append("<td><b>");
                                    chat.Append(entityChat.Message);
                                    chat.Append("</b></td>");
                                }
                                if (entityChat.MessageType == MessageType.Voice)
                                {
                                    if (entityChat.Voice != null)
                                    {

                                        chat.Append("<td><a  href=\"" + entityChat.Voice.VoiceUrl + "\" >Audio</a>");
                                    }
                                    chat.Append("</td>");

                                }
                                chat.Append("</tr>");

                            }
                            chat.Append("</tbody></table>");
                            string chatList = chat.ToString();
                            AccountService.SendPatientSupportFreeChatEmail(entityPatient, entityLastDoc, chatList);
                            return new DoctorModel(entityDoctor, 0, 0, 0);

                        }
                    }
                }
                else
                {
                    entityDoctor = entityLastChat.Doctor;
                }

                int attendedUser = (int)(entityDoctor.DoctorStat.AttendedUsers ?? 0);

                int avgRating = (int)(entityDoctor.DoctorStat.AverageRating ?? 0);

                return new DoctorModel(entityDoctor, attendedUser, entityDoctor.UserProfile.DoctorUserReviews1.Count(), avgRating);
            }
        }

        public static List<CategoryModel> GetAllCategories(string accessToken, int skip, int take)
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityUser = AccountService.GetEntityUserByAccessToken(db, accessToken,true);
                var result = _GetAllCategoriesPartial(db, skip, take);
                return result;
            }
        }

        public static List<GroupModel> GetGroupList(string accessToken, int skip, int take, int? categoryId = null)
        {
            ActivityAuditHelper.AddRequestParamsToLog();
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityUser = AccountService.GetEntityUserByAccessToken(db, accessToken, true);
                int? catId = GetCategoryForPatient(entityUser, accessToken, categoryId);
                
                return GetGroupList(db, entityUser, skip, take, catId);
            }
        }

        private static List<GroupModel> GetGroupList(Entity.db_HeyDocEntities db, Entity.UserProfile entityUser, int skip, int take, int? categoryId)
        {
            var entityGroups = db.Groups.Include(e => e.Photo).Where(e => !e.IsDeleted);
            var doctorGroupUserTypeCategory = DoctorGroupUserTypeCategories.Doc2Us;
            if (categoryId.HasValue)
            {
                entityGroups = entityGroups.Where(e => e.CategoryId == categoryId.Value).OrderBy(e => e.GroupName);
            }
            else
            {
                throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidArguments));
            }

            var totalCount = entityGroups.Count();
            entityGroups = entityGroups.Skip(skip).Take(take);
            var result = new List<GroupModel>();

            
            foreach (var entityGroup in entityGroups)
            {
                result.Add(new GroupModel(entityGroup, userTypeCategory: doctorGroupUserTypeCategory));
            }

            if (totalCount <= (skip + take) && categoryId.HasValue)
            {
                var group = new GroupModel
                {
                    CategoryId = categoryId.Value,
                    GroupId = 0,
                    GroupName = "Independent"
                };
                result.Add(group);
            }
            return result;
        }

        private static int? GetCategoryForPatient(Entity.UserProfile entityUser, string accessToken, int? categoryId)
        {
            int? catId;
            if (categoryId.HasValue)
            {
                catId = categoryId.Value;
            }
            else if (entityUser == null)
            {
                if (string.IsNullOrEmpty(accessToken))
                {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidArguments));
                }
                else
                {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.AccessTokenNotFound, Account.ErrorSessionExpired));
                }
            }
            else
            {
                throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidArguments));
            }
            return catId;
        }

        internal static List<CategoryModel> _GetAllCategoriesPartial(Entity.db_HeyDocEntities db, int skip, int take)
        {
            var entityCategories = db.Categories.Where(e => !e.IsDelete && !e.IsHiddenFromPublic);
            entityCategories = entityCategories.Where(e=>!e.IsDelete).OrderBy(e => e.Sequence).ThenBy(e=>e.CategoryName).Skip(skip).Take(take);
            var result = new List<CategoryModel>();
            foreach (var entityCategory in entityCategories)
            {
                result.Add(new CategoryModel(entityCategory));
            }
            return result;
        }

        public static List<SelectListItem> GetCategoryList()
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                return _GetCategoryList(db);
            }
        }

        public static List<SelectListItem> _GetCategoryList(Entity.db_HeyDocEntities db)
        {
            List<SelectListItem> categoryList = new List<SelectListItem>();
            var categories = db.Categories.Where(e=>!e.IsDelete).OrderBy(e => e.Sequence).ThenBy(e => e.CategoryName).Select(e => new SelectListItem()
            {
                Text = e.CategoryName,
                Value = e.CategoryId.ToString(),
            });
            foreach (var category in categories)
            {
                categoryList.Add(category);
            }
            return categoryList;
        }

    }
}
