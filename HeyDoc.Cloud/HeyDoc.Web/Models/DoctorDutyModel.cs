using HeyDoc.Web.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HeyDoc.Web.Models
{
    public class DoctorDutyModel
    {
        public DoctorDutyModel()
        {
        }
        

        public int DoctorDutyId { get; set; }
        public int UserId { get; set; }
       
        public int DayId { get; set; }

        public string DayName { get; set; }
        public TimeSpan FromTime { get; set; }
        public TimeSpan ToTime { get; set; }

        public DoctorDutyModel(Entity.DoctorDuty edd)
        {
            DoctorDutyId = edd.DoctorDutyId;
            UserId = edd.UserId;
            DayId = edd.DayId;
            FromTime = edd.FromTime;
            ToTime = edd.ToTime;
            switch (edd.DayId)
            {
                case 1: DayName = "Monday"; break;
                case 2: DayName = "Tuesday"; break;
                case 3: DayName = "Wednesday"; break;
                case 4: DayName = "Thursday"; break;
                case 5: DayName = "Friday"; break;
                case 6: DayName = "Saturday"; break;
                case 7: DayName = "Sunday"; break;
            }

        }

    }
}