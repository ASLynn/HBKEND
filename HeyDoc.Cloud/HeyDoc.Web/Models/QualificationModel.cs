using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HeyDoc.Web.Models
{
    public class QualificationModel

    {
        public int QualificationId { get; set; }
        public string QualificationDesc { get; set; }
        public int Status { get; set; }
      

        public QualificationModel()
        {

        }

        public QualificationModel(Entity.Qualification entityQualification)
        {
            QualificationId = entityQualification.QualificationId;
            QualificationDesc = entityQualification.QualificationDesc;
            Status = entityQualification.Status;

        }




    }

   
}