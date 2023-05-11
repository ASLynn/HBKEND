using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HeyDoc.Web.Models
{
    public class AuditLogModel
    {
        public string Description { get; set; }
        public DateTime CreateDate { get; set; }
        public string AuditLogTypeStr { get; set; }
        public string UserFullName { get; set; }

        public AuditLogModel(Entity.AuditLog auditLog)
        {
            UserFullName = string.IsNullOrEmpty(auditLog.UserProfile.FullName) ? auditLog.UserProfile.Nickname : auditLog.UserProfile.FullName;
            AuditLogTypeStr = auditLog.LogType.GetDescription();
            Description = auditLog.Description;
            CreateDate = auditLog.CreateDate;
        }
    }
}