using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HeyDoc.Web.Models
{
    public class FacilityTypeModel
    {
        public int FacilityTypeId { get; set; }
        public string FacilityTypeDesc { get; set; }
        public int FacilityTypeStatus { get; set; }

        public FacilityTypeModel()
        {

        }

        public FacilityTypeModel (Entity.FacilityType entityFacilityType)
        {
            FacilityTypeId = entityFacilityType.FacilityTypeId;
            FacilityTypeDesc = entityFacilityType.FacilityTypeDesc;
            FacilityTypeStatus = entityFacilityType.FacilityTypeStatus;
        }
    }
}