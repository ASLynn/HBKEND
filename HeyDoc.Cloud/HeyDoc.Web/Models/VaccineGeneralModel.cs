using System;

namespace HeyDoc.Web.Models
{
    public class VaccineGeneralModel
    {
        public int VaccineGeneral_Id { get; set; }
        public string VaccineGeneralName { get; set; }
        public int? AdultChild { get; set; }
        public DateTime? AddedDate { get; set; }
      

        public VaccineGeneralModel()
        { }

        public VaccineGeneralModel(Entity.VaccineGeneral entityVaccineGeneral)
        {
            VaccineGeneral_Id = entityVaccineGeneral.VaccineGeneral_Id;
            VaccineGeneralName = entityVaccineGeneral.VaccineGeneralName;
            AdultChild = entityVaccineGeneral.AdultChild;
            AddedDate = entityVaccineGeneral.AddedDate;
        }
    }
}