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
    
    public partial class VaccineDetail
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public VaccineDetail()
        {
            this.VaccinatedUsers = new HashSet<VaccinatedUser>();
        }
    
        public int VaccineDetail_Id { get; set; }
        public int VaccineGeneral_Id { get; set; }
        public string VaccineDetailName { get; set; }
    
        public virtual VaccineGeneral VaccineGeneral { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<VaccinatedUser> VaccinatedUsers { get; set; }
    }
}
