using HeyDoc.Web.Helpers;
using HeyDoc.Web.Models;
using HeyDoc.Web.Resources;
using HeyDoc.Web.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HeyDoc.Web.Services
{
    public class ReviewService
    {
        public static ReviewModel ReviewDoctor(string accessToken, int userId, ReviewModel model)
        {
            ActivityAuditHelper.AddRequestParamsToLog();
            model.Validate();

            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityUser = AccountService.GetEntityUserByAccessToken(db, accessToken, false);
                var entityDoctor = AccountService.GetEntityTargetUserByUserId(db, userId, false);

                var entityChat = db.ChatRooms.First(e => e.DoctorId == entityDoctor.UserId);

                if (entityChat == null)
                {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidAction, Forms.ErrorReviewNonConsultedDoctor));
                }

                var entityReview = entityUser.DoctorUserReviews1.FirstOrDefault(e => e.DoctorId == entityDoctor.UserId);
                if (entityReview == null)
                {
                    entityReview = db.DoctorUserReviews.Create();
                    entityReview.DoctorId = entityDoctor.UserId;
                    entityReview.UserId = entityUser.UserId;

                    db.DoctorUserReviews.Add(entityReview);
                }

                entityReview.Rating = (byte)model.Rating;
                entityReview.Comment = model.Comment;
                entityReview.CreateDate = DateTime.UtcNow;

                db.SaveChanges();

                return new ReviewModel(entityReview);
            }
        }

        public static List<ReviewModel> ReviewList(string accessToken, int? userId = null, int take = 15, int skip = 0)
        {
            ActivityAuditHelper.AddRequestParamsToLog();
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityUser = AccountService.GetEntityUserByAccessToken(db, accessToken, false);
                Entity.UserProfile entityDoctor = null;
                if (userId.HasValue)
                {
                    entityDoctor = AccountService.GetEntityTargetUserByUserId(db, userId.Value, false);
                }
                else
                {
                    entityDoctor = entityUser;
                }

                var entityReviewUserList = db.DoctorUserReviews.Where(e => e.DoctorId == entityDoctor.UserId).OrderByDescending(e => e.CreateDate).Skip(skip).Take(take);

                List<ReviewModel> result = new List<ReviewModel>();

                foreach (var entityReviewUser in entityReviewUserList)
                {
                    result.Add(new ReviewModel(entityReviewUser));
                }

                return result;
            }
        }
    }
}