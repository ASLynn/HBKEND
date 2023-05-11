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
    
    public partial class VaccinatedUserInfo
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public VaccinatedUserInfo()
        {
            this.VaccinatedUsers = new HashSet<VaccinatedUser>();
        }
    
        public int VaccinatedUserId { get; set; }
        public int UserId { get; set; }
        public string VUserName { get; set; }
        public string VPhone { get; set; }
        public Nullable<int> VSex { get; set; }
        public Nullable<int> VAge { get; set; }
        public Nullable<System.DateTime> VDob { get; set; }
        public string VNrc { get; set; }
        public string VAddress { get; set; }
        public string Remark { get; set; }
        public string Passport { get; set; }
        public Nullable<int> StateId { get; set; }
        public Nullable<int> TownshipId { get; set; }
        public string NorAe { get; set; }
    
        public virtual UserProfile UserProfile { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<VaccinatedUser> VaccinatedUsers { get; set; }
        public virtual State State { get; set; }
        public virtual Township Township { get; set; }
    }
}