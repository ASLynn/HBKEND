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
    
    public partial class Notification
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Notification()
        {
            this.NotificationReadStatus = new HashSet<NotificationReadStatu>();
            this.NotificationTags = new HashSet<NotificationTag>();
        }
    
        public long NotificationId { get; set; }
        public Nullable<int> UserId { get; set; }
        public string Text { get; set; }
        public string RelatedId { get; set; }
        public System.DateTime CreateDate { get; set; }
        public HeyDoc.Web. PnActionType  NotificationType { get; set; }
        public bool IsRead { get; set; }
        public Nullable<byte> DeviceType { get; set; }
    
        public virtual UserProfile UserProfile { get; set; }
        public virtual PromotionUrl PromotionUrl { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<NotificationReadStatu> NotificationReadStatus { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<NotificationTag> NotificationTags { get; set; }
    }
}