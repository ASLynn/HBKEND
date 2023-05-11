using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HeyDoc.Web.Models
{
    public class VaccineDetailModel
    {
        public int VaccineDetail_Id { get; set; }
        public int VaccineGeneral_Id { get; set; }
        public string VaccineDetailName { get; set; }

        public VaccineDetailModel()
        { }

        public VaccineDetailModel(Entity.VaccineDetail entityVaccineDetail)
        {
            VaccineDetail_Id = entityVaccineDetail.VaccineDetail_Id;
            VaccineGeneral_Id = entityVaccineDetail.VaccineGeneral_Id;
            VaccineDetailName = entityVaccineDetail.VaccineDetailName;           
        }
    }
}