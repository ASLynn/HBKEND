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
    
    public partial class PatientExtra
    {
        public int PatientExtraId { get; set; }
        public int UserId { get; set; }
        public string EmergencyPerson { get; set; }
        public Nullable<int> RelationshipId { get; set; }
        public string EmergencyContact { get; set; }
        public string EmergencyAddress { get; set; }
    
        public virtual Patient Patient { get; set; }
    }
}