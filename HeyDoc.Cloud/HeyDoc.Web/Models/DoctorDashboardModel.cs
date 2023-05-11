using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HeyDoc.Web.Models
{
    public class DoctorDashboardModel
    {
        public int PrescriptionCreatedCount { get; set; }
        public double EpsAverageResponseTimeInSeconds { get; set; }
        public double ChatAverageResponseTimeInSeconds { get; set; }
        public int TotalOnlineTimeInSeconds { get; set; }
        public double ChatSessionAverageDurationInSeconds { get; set; }
        public int RequestTotalCount { get; set; }
        public int RequestAcceptedCount { get; set; }
        public int PatientCount { get; set; }
        public double AverageRating { get; set; }
        public int TotalRatingCount { get; set; }
        public IList<ReviewModel> LatestRatingPreviews { get; set; }
        public decimal PointEarned { get; set; }
    }
}