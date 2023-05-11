using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HeyDoc.Web.Models
{
    public class DoctorReviewModel
    {
        public int DoctorId { get; set; }
        public int UserId { get; set; }
        public byte Rating { get; set; }
        public string Comment { get; set; }
        public DateTime CreateDate { get; set; }

        public DoctorReviewModel()
        {

        }

        public DoctorReviewModel(Entity.DoctorUserReview entityReview)
        {
            Rating = entityReview.Rating;
            Comment = entityReview.Comment;
            CreateDate = entityReview.CreateDate;
        }
    }
}