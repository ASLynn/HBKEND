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
    
    public partial class Drug
    {
        public long DrugId { get; set; }
        public long PrescriptionId { get; set; }
        public string DrugName { get; set; }
        public string Dosage { get; set; }
        public string Route { get; set; }
        public string Frequency { get; set; }
        public string Amount { get; set; }
        public string Status { get; set; }
        public string Remark { get; set; }
        public string RemarkByUser { get; set; }
        public Nullable<long> MedicationId { get; set; }
        public bool IsDelete { get; set; }
        public int TotalQuantity { get; set; }
        public int DurationInMonth { get; set; }
        public Nullable<int> DispensedAmount { get; set; }
        public string DispensedAmountUnit { get; set; }
    
        public virtual Medication Medication { get; set; }
        public virtual Prescription Prescription { get; set; }
    }
}
