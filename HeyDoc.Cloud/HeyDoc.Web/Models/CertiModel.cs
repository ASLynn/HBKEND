using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HeyDoc.Web.Models
{
    public class CertiModel
    {
        public int CertiId { get; set; }
        public int DoctorId { get; set; }
        public string CertiUrl { get; set; }
        public byte[] CertiUrlByte { get; set; }
        public int CertiStatus { get; set; }

        public CertiModel()
        {

        }

        public CertiModel(Entity.Certi entityCerti)
        {
            CertiId = entityCerti.CertiId;
            DoctorId = entityCerti.DoctorId;
            CertiUrl = entityCerti.CertiUrl;
            CertiStatus = entityCerti.CertiStatus;
        }




    }
}