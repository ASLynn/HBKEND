using HeyDoc.Web.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HeyDoc.Web.Models
{
    public class ReviewModel
    {
        public int DoctorId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime CreateDate { get; set; }

        public UserModel Patient { get; set; }

        public ReviewModel()
        {

        }
        public ReviewModel(Entity.DoctorUserReview entityUserReview)
        {
            DoctorId = entityUserReview.DoctorId;
            Patient = new UserModel(entityUserReview.UserProfile1);
            Rating = entityUserReview.Rating;
            Comment = entityUserReview.Comment;
            CreateDate = entityUserReview.CreateDate;
        }

        public void Validate()
        {
            if (Rating < 1 || Rating > 5)
            {
                throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidArguments, "Rating must be between 1 and 5"));
            }
        }


    }

}