//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace HeyDoc.Web.Entity
{
    using System;
    using System.Collections.Generic;
    
    public partial class AuditLog
    {
        public long LogId { get; set; }
        public int UserId { get; set; }
        public HeyDoc.Web.AuditLogType LogType { get; set; }
        public string Description { get; set; }
        public string RelatedId { get; set; }
        public System.DateTime CreateDate { get; set; }
    
        public virtual UserProfile UserProfile { get; set; }
    }
}
