using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HeyDoc.Web.Models
{
    public class PatientExtraModel

    {
        public int PatientExtraId { get; set; }
        public int UserId { get; set; }
        public string EmergencyPerson { get; set; }
        public int? RelationshipId { get; set; }
        public string EmergencyContact { get; set; }
        public string EmergencyAddress { get; set; }        
       



        public PatientExtraModel()
        {

        }

        public PatientExtraModel(Entity.PatientExtra entityPatientExtra)
        {
            PatientExtraId = entityPatientExtra.PatientExtraId;
            UserId = entityPatientExtra.UserId;
            EmergencyPerson = entityPatientExtra.EmergencyPerson;
            RelationshipId = entityPatientExtra.RelationshipId;
            EmergencyContact = entityPatientExtra.EmergencyContact;
            EmergencyAddress = entityPatientExtra.EmergencyAddress;
           

        }




    }

   
}