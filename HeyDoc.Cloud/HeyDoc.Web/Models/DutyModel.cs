using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HeyDoc.Web.Models
{
    public class DutyModel
    {
        public int DutyId { get; set; }
        public string DutyFileUrl { get; set; }
        public DateTime CreateDate { get; set; }

        public DutyModel()
        {

        }

         public DutyModel(string fileUrl)
        {
            DutyFileUrl = fileUrl;
        }

         public DutyModel(Entity.DutyRoster entityDuty)
        {
            DutyId = entityDuty.DutyId;
            DutyFileUrl = entityDuty.FileUrl;
        }
    }
}