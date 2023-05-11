using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HeyDoc.Web.Models
{
    public class OnSiteEventModel
    {
        public int OnSiteEventId { get; set; }
        public string EventCode { get; set; }
        public string Description { get; set; }
        public int CorporateId { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDate { get; set; }
        public int EventTypeId { get; set; }

        public OnSiteEventModel(Entity.OnSiteEvent entityEvent)
        {
            OnSiteEventId = entityEvent.Id;
            EventCode = entityEvent.Code;
            Description = entityEvent.Description;
            CorporateId = entityEvent.CorporateId;
            IsActive = entityEvent.IsActive;
            CreateDate = entityEvent.CreateDate;
            EventTypeId = entityEvent.EventTypeId;
        }
    }

    public class OnSiteEventUserAndCheckInsModel
    {
        public UserModel User { get; set; }
        public IEnumerable<DateTime> CheckInTimes { get; set; }
    }
}