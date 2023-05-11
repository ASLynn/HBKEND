using HeyDoc.Web.Services;

namespace HeyDoc.Web.Models
{
    public class DoctorFacilityModel : FacilityModel
    {

        public int DoctorId { get; set; }

        public int FacilityId { get; set; }

        public DoctorFacilityModel()
        {

        }

        public DoctorFacilityModel(Entity.DocFacilityAccessment entityFacility)
        {
            DoctorId = entityFacility.DoctorId;
            FacilityId = entityFacility.FacilityId;
            FacilityName = FacilityService.GetFacilityDescById(FacilityId);
        }
    }
}