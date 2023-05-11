using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HeyDoc.Web.Models
{
    public class NotificationStatsModel
    {
        public long NotificationId { get; set; }
        public string NotificationType { get; set; }
        public string Text { get; set; }
        public string URL { get; set; }
        public string DeviceType { get; set; }
        public string SentDateTime { get; set; }
        public string TagNames { get; set; }
        public int Reach { get; set; }
        public double Ctr { get; set; }
    }
}