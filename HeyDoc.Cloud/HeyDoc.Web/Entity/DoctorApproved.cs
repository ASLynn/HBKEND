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
    
    public partial class DoctorApproved
    {
        public int DoctorApprovedId { get; set; }
        public int DoctorId { get; set; }
        public int ApproverId { get; set; }
        public System.DateTime ApprovedDT { get; set; }
        public int DoctorApprovedStatus { get; set; }
    }
}