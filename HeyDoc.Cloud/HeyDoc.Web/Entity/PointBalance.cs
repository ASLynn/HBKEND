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
    
    public partial class PointBalance
    {
        public int ID { get; set; }
        public int UserID { get; set; }
        public int Balance { get; set; }
        public Nullable<int> CreditBalance { get; set; }
        public int LastTopupRequestID { get; set; }
        public Nullable<System.DateTime> UpdateDate { get; set; }
    
        public virtual PointTopupRequest PointTopupRequest { get; set; }
        public virtual UserProfile UserProfile { get; set; }
    }
}
