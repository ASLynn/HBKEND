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
    
    public partial class DocFacilityAccessment
    {
        public int DocFacilityAccessmentId { get; set; }
        public int DoctorId { get; set; }
        public int FacilityId { get; set; }
        public int DocFacilityAccessmentStatus { get; set; }
    
        public virtual Doctor Doctor { get; set; }
        public virtual Facility Facility { get; set; }
    }
}
