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
    
    public partial class PrescriptionLog
    {
        public long LogId { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public bool IsDelete { get; set; }
        public string LogText { get; set; }
        public int DispatchId { get; set; }
        public string LogUrl { get; set; }
        public HeyDoc.Web.PnActionType LogType { get; set; }
    
        public virtual PrescriptionDispatch PrescriptionDispatch { get; set; }
    }
}
