using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HeyDoc.Web.Models
{
    public class SpecialityModel
    {
        public int SpecialityId { get; set; }
        public string SpecialityDesc { get; set; }
        public int Status { get; set; }
      

        public SpecialityModel()
        {

        }

        public SpecialityModel(Entity.Speciality entitySpeciality)
        {
            SpecialityId = entitySpeciality.SpecialityId;
            SpecialityDesc = entitySpeciality.SpecialityDesc;
            Status = entitySpeciality.Status;

        }
    }

   
}