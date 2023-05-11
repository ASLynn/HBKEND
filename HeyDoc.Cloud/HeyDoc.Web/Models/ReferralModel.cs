using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HeyDoc.Web.Models
{
    public class ReferralModel
    {
       public long InvitationId { get; set; }
        public int UserId { get; set; }
        public int InvitedUserId { get; set; }
        public DateTime CreateDate { get; set; }
        public bool IsRegistered { get; set; }

        public ReferralModel()
        {

        }

        public ReferralModel(Entity.Invitation entityReferral)
        {
            InvitationId = entityReferral.InvitationId;
            UserId = entityReferral.UserId;
            InvitedUserId = entityReferral.InvitedUserId;
            CreateDate = entityReferral.CreateDate;
            IsRegistered = entityReferral.IsRegistered;
        }
    }
}