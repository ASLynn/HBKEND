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
    
    public partial class PromoCode
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public PromoCode()
        {
            this.PaymentRequests = new HashSet<PaymentRequest>();
        }
    
        public long PromoCodeId { get; set; }
        public string PromoCode1 { get; set; }
        public string Description { get; set; }
        public decimal Discount { get; set; }
        public HeyDoc.Web.PromoDiscountType DiscountType { get; set; }
        public string PartnerName { get; set; }
        public System.DateTime StartDate { get; set; }
        public Nullable<System.DateTime> EndDate { get; set; }
        public Nullable<int> UserUsageLimit { get; set; }
        public Nullable<int> MaxUserLimit { get; set; }
        public HeyDoc.Web.PromoStatus PromoStatus { get; set; }
        public bool IsDelete { get; set; }
        public System.DateTime CreateDate { get; set; }
        public Nullable<int> CategoryId { get; set; }
        public Nullable<int> DoctorId { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PaymentRequest> PaymentRequests { get; set; }
        public virtual Doctor Doctor { get; set; }
        public virtual Category Category { get; set; }
    }
}