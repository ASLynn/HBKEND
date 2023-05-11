using HeyDoc.Web.Services;

namespace HeyDoc.Web.Models
{
    public class DoctorQualificationModel : QualificationModel
    {

        public int DoctorId { get; set; }

        public int QualificationId { get; set; }

        public DoctorQualificationModel()
        {

        }

        public DoctorQualificationModel(Entity.DoctorQualification entityQualification)
        {
            DoctorId = entityQualification.DoctorId;
            QualificationId = entityQualification.QualificationId;
            QualificationDesc = QualificationService.GetQualificationDescById(QualificationId);
        }

    }
}