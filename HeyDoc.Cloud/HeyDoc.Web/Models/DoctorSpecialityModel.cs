using HeyDoc.Web.Entity;
using HeyDoc.Web.Services;
using System;
using System.Collections.Generic;

namespace HeyDoc.Web.Models
{
    public class DoctorSpecialityModel : SpecialityModel
    {
        public int DoctorId { get; set; }

        public int SpecialityId { get; set; }

        public DoctorSpecialityModel()
        {

        }       
     
        public DoctorSpecialityModel(Entity.DoctorSpeciality entitySpeciality)
        {
            DoctorId = entitySpeciality.DoctorId;
            SpecialityId = entitySpeciality.SpecialityId;
            SpecialityDesc = SpecialityService.GetSpecialityDescById(SpecialityId);
        }

        
    }
}