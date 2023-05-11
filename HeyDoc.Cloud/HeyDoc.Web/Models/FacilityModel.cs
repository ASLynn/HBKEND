using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HeyDoc.Web.Models
{
    public class FacilityModel
    {
        public int FacilityId { get; set; }
        public string FacilityName { get; set; }
        public string FacilityAddress { get; set; }
        public string FacilityPh { get; set; }
        public int FacilityTypeId { get; set; }
        public int FacilityStatus { get; set; }
        public int StateId { get; set; }
        public int TownshipId { get; set; }

        public FacilityModel ()
        {

        }

        public FacilityModel (Entity.Facility entityFacility)
        {
            FacilityId = entityFacility.FacilityId;
            FacilityName = entityFacility.FacilityName;
            FacilityAddress = entityFacility.FacilityAddress;
            FacilityPh = entityFacility.FacilityPh;
            FacilityStatus = entityFacility.FacilityStatus;
            StateId = entityFacility.StateId;
            TownshipId = entityFacility.TownshipId;

        }




    }

    public class FacilityListModel
    {
        public int FacilityId { get;set; }
        public string FacilityName { get; set; }
        public string FacilityAddress { get; set; }
        public string FacilityPh { get; set; }
        public int FacilityTypeId { get; set; }
        public string FacilityType { get; set; }
        public int StateId { get; set; }
        public string State { get; set; }
        public int TownshipId { get; set; }
        public string Township { get; set; }
        public int FacilityStatus { get; set; }

    }
}