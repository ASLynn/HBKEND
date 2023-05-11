using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using HeyDoc.Web.Services;
namespace HeyDoc.Web.Models
{
    public class VaccinatedUserModel : VaccinatedUserInfoModel
    {
        public int Vaccination_Id { get; set; }
        public int VaccineDetail_Id { get; set; }
        public string Dose { get; set; }
        public DateTime? VaccinationDate { get; set; } 
        public string VaccinationSite { get; set; }
        public string VacCertificateUrl { get; set; }
        public string LotNoOfVaccine { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string Status { get; set; }
        public int? IsDelete { get; set; }

        [MaxLength] 
        public string RemarkReason { get; set; }

        public string gender { get; set; }

        public string VaccineDetailName { get; set; }

        public int VaccineGeneralId { get; set; }
        public VaccinatedUserModel()
        { }

        public VaccinatedUserModel(Entity.VaccinatedUser entityVaccinatedUser)
        {
            Vaccination_Id = entityVaccinatedUser.Vaccination_Id;
            VaccinatedUserId = entityVaccinatedUser.VaccinatedUserId;
            userProfile_UserId = entityVaccinatedUser.UserId;
            VaccineDetail_Id = entityVaccinatedUser.VaccineDetail_Id;
            if(entityVaccinatedUser.VaccineDetail == null)
            {
                using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
                {
                    entityVaccinatedUser.VaccineDetail = db.VaccineDetails.SingleOrDefault(e => e.VaccineDetail_Id == VaccineDetail_Id);
                }
            } 
            VaccineDetailName = entityVaccinatedUser.VaccineDetail.VaccineDetailName;
            Dose = entityVaccinatedUser.Dose;
            VaccinationDate = entityVaccinatedUser.VaccinationDate;
            VaccinationSite = entityVaccinatedUser.VaccinationSite;
            VacCertificateUrl = entityVaccinatedUser.VacCertificateUrl;
            LotNoOfVaccine = entityVaccinatedUser.LotNoOfVaccine;
            CreatedDate = entityVaccinatedUser.CreatedDate;
            Status = entityVaccinatedUser.Status;
            IsDelete = entityVaccinatedUser.IsDelete;
            RemarkReason = entityVaccinatedUser.RemarkReason;
            if (entityVaccinatedUser.VaccinatedUserInfo == null)
            {
                using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
                {
                    entityVaccinatedUser.VaccinatedUserInfo = db.VaccinatedUserInfoes.SingleOrDefault(e => e.VaccinatedUserId == VaccinatedUserId);
                }
            }

            VUserName = entityVaccinatedUser.VaccinatedUserInfo.VUserName;
            VNrc = entityVaccinatedUser.VaccinatedUserInfo.VNrc;
            VPhone = entityVaccinatedUser.VaccinatedUserInfo.VPhone;
            VSex = entityVaccinatedUser.VaccinatedUserInfo.VSex;
            gender = entityVaccinatedUser.VaccinatedUserInfo.VSex == 1 ? "Male" : "Female";
            VDob = entityVaccinatedUser.VaccinatedUserInfo.VDob;
            VAge = entityVaccinatedUser.VaccinatedUserInfo.VAge;
            VAddress = entityVaccinatedUser.VaccinatedUserInfo.VAddress;

        }
    }
}