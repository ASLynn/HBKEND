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
    
    public partial class State
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public State()
        {
            this.UserProfiles = new HashSet<UserProfile>();
            this.Facilities = new HashSet<Facility>();
            this.VaccinatedUserInfoes = new HashSet<VaccinatedUserInfo>();
        }
    
        public int StateId { get; set; }
        public int StateCode { get; set; }
        public string StateDesc { get; set; }
        public string StateDescMM { get; set; }
        public int StateStatus { get; set; }
        public Nullable<int> StateNRCcode_EN { get; set; }
        public string StateNRCcode_MM { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<UserProfile> UserProfiles { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Facility> Facilities { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<VaccinatedUserInfo> VaccinatedUserInfoes { get; set; }
    }
}
