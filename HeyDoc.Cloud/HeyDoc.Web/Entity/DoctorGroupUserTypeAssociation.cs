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
    
    public partial class DoctorGroupUserTypeAssociation
    {
        public long GroupId { get; set; }
        public HeyDoc.Web.DoctorGroupUserTypeCategories UserType { get; set; }
        public Nullable<byte> OrderRank { get; set; }
    
        public virtual Group Group { get; set; }
    }
}
