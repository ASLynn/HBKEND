using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HeyDoc.Web.Models
{
    public class VaccinatedUserInfoModel
    {
        public int VaccinatedUserId { get; set; }
        public int userProfile_UserId { get; set; }
        public string VUserName { get; set; }
        public string VPhone { get; set; }
        public int? VSex { get; set; }
        public int? VAge { get; set; }
        public DateTime? VDob { get; set; }
        public string VNrc { get; set; }
        public string VAddress { get; set; }
        public string Remark { get; set; }

        public int? StateId { get; set; }
        public int? TownshipId { get; set; }
        public string NorAe { get; set; }
        public string Passport { get; set; }

        public int? StateNRCcode_EN { get; set; }
        public string StateNRCcode_MM { get; set; }
        public string TownshipNRCabb_EN { get; set; }
        public string TownshipNRCabb_MM { get; set; }
        public string NorAe_MM { get; set; }
        public VaccinatedUserInfoModel()
        { }

        public VaccinatedUserInfoModel(Entity.VaccinatedUserInfo entityVaccinatedUserInfo)
        {
            VaccinatedUserId = entityVaccinatedUserInfo.VaccinatedUserId;
            userProfile_UserId = entityVaccinatedUserInfo.UserId;
            VUserName = entityVaccinatedUserInfo.VUserName;
            VPhone = entityVaccinatedUserInfo.VPhone;
            VSex = entityVaccinatedUserInfo.VSex;
            VAge = entityVaccinatedUserInfo.VAge;
            VDob = entityVaccinatedUserInfo.VDob;
            VNrc = entityVaccinatedUserInfo.VNrc.Trim();
            VAddress = entityVaccinatedUserInfo.VAddress;
            Remark = entityVaccinatedUserInfo.Remark;
            StateId = entityVaccinatedUserInfo.StateId;
            TownshipId = entityVaccinatedUserInfo.TownshipId;
            NorAe = entityVaccinatedUserInfo.NorAe;
            Passport = entityVaccinatedUserInfo.Passport;
            if (entityVaccinatedUserInfo.State == null)
            {
                using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
                {
                    entityVaccinatedUserInfo.State = db.States.SingleOrDefault(e => e.StateId == StateId);
                    entityVaccinatedUserInfo.Township = db.Townships.SingleOrDefault(e => e.TownshipId == TownshipId);
                }
            }
            StateNRCcode_EN = entityVaccinatedUserInfo.State.StateNRCcode_EN;
            StateNRCcode_MM = entityVaccinatedUserInfo.State.StateNRCcode_MM;
            TownshipNRCabb_EN = entityVaccinatedUserInfo.Township.TownshipNRCabb_EN;
            TownshipNRCabb_MM = entityVaccinatedUserInfo.Township.TownshipNRCabb_MM;
            NorAe_MM = entityVaccinatedUserInfo.NorAe == "A" ? "ဧည့်" : "နိုင်";

        }
    }
}