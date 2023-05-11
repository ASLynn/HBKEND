using HeyDoc.Web.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HeyDoc.Web.Models;
using System.IO;
using HeyDoc.Web.Lib;
using System.Data.Entity.Validation;
using System.Drawing;
using System.Drawing.Imaging;

namespace HeyDoc.Web.Services
{
    public static class VaccineService
    {
        private static readonly log4net.ILog log =
          log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        internal static List<VaccinatedUserModel> GetVaccineInfoListForApproval(string status)
        {
            List<VaccinatedUserModel> VDList = new List<VaccinatedUserModel>();
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var tmpRes = from e in db.VaccinatedUsers
                             where e.Status.ToLower() == (status.ToLower() == "all" ? e.Status.ToLower() : status.ToLower())
                             orderby e.CreatedDate
                             select e;

                foreach (var a in tmpRes)
                {
                    VDList.Add(new VaccinatedUserModel(a));
                }
            }
            return VDList;
        }

        internal static VaccinatedUserInfoModel GetVaccinatedUserInfo(VaccinatedUserInfoModel model)
        {            
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityVU = db.VaccinatedUserInfoes.SingleOrDefault(e => e.UserId == model.userProfile_UserId);
                if (entityVU == null)
                {
                    VaccinatedUserInfoModel a = new VaccinatedUserInfoModel();
                    return a;
                }
                else
                {
                    VaccinatedUserInfoModel result = new VaccinatedUserInfoModel(entityVU);
                    return result;                    
                }
            }
        }

        internal static List<VaccineGeneralModel> GetVaccineGeneralList()
        {
            List<VaccineGeneralModel> VGMList = new List<VaccineGeneralModel>();
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var tmpRes = from e in db.VaccineGenerals
                             where e.AdultChild == 1 // Filter only adult
                             orderby e.VaccineGeneralName
                             select e;

                foreach (var a in tmpRes)
                {
                    VGMList.Add(new VaccineGeneralModel(a));
                }
            }
            return VGMList;
        }

        internal static List<VaccineDetailModel> GetVaccineByGeneralVaccineId(VaccineGeneralModel model)
        {
            List<VaccineDetailModel> VDList = new List<VaccineDetailModel>();
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var tmpRes = from e in db.VaccineDetails
                             where e.VaccineGeneral_Id == model.VaccineGeneral_Id                          
                             orderby e.VaccineDetailName
                             select e;

                foreach (var a in tmpRes)
                {
                    VDList.Add(new VaccineDetailModel(a));
                }
            }
            return VDList;
        }

        internal static List<VaccineDetailModel> GetVaccineDetailList(int generalVaccineId)
        {
            List<VaccineDetailModel> VDList = new List<VaccineDetailModel>();
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var tmpRes = from e in db.VaccineDetails
                             where e.VaccineGeneral_Id == generalVaccineId // Filter only active countries, checking HasValue since IsActive is a nullable
                             orderby e.VaccineDetailName
                             select e;

                foreach (var a in tmpRes)
                {
                    VDList.Add(new VaccineDetailModel(a));
                }
            }
            return VDList;
        }
        internal static List<VaccinatedUserModel> GetDoseInfo(VaccinatedUserModel model)
        {
            List<VaccinatedUserModel> VDList = new List<VaccinatedUserModel>();
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var tmpRes = from t1 in db.VaccinatedUsers
                             where (t1.VaccinatedUserId == model.VaccinatedUserId) && (t1.VaccineDetail_Id == model.VaccineDetail_Id)
                             orderby t1.Dose descending, t1.Status
                             select t1;

                foreach (var a in tmpRes)
                {
                    VDList.Add(new VaccinatedUserModel(a));
                }
            }
            return VDList;
        }
        internal static List<VaccinatedUserModel> GetLastDoseInfo(VaccinatedUserModel model)
        {
            List<VaccinatedUserModel> VDList = new List<VaccinatedUserModel>();
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var tmpRes = from t1 in db.VaccinatedUsers
                             where (t1.VaccinatedUserId == model.VaccinatedUserId)
                             orderby t1.CreatedDate
                             select t1;

                foreach (var a in tmpRes)
                {
                    VDList.Add(new VaccinatedUserModel(a));
                }
            }
            return VDList;
        }
        internal static List<VaccinatedUserModel> GetVaccineInfoListForUser(VaccinatedUserModel model)
        {
            int vaccineGeneralId = 1;
            if(model.VaccineGeneralId > 0)
            {
                vaccineGeneralId = model.VaccineGeneralId;
            }
           

            List<VaccinatedUserModel> VDList = new List<VaccinatedUserModel>();
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var tmpRes = from t1 in db.VaccinatedUsers
                             join t2 in db.VaccineDetails on t1.VaccineDetail_Id equals t2.VaccineDetail_Id
                             where t1.VaccinatedUserId == model.VaccinatedUserId
                             where t2.VaccineGeneral_Id == vaccineGeneralId
                             orderby t1.VaccinationDate
                             orderby t1.Vaccination_Id
                             select t1;

                foreach (var a in tmpRes)
                {
                    VDList.Add(new VaccinatedUserModel(a));
                }
            }
            return VDList;
        }
     
        internal static VaccinatedUserInfoModel SaveVaccinatedUserInfo(VaccinatedUserInfoModel model)
        {
            ActivityAuditHelper.AddRequestDataToLog();
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityVU = db.VaccinatedUserInfoes.SingleOrDefault(e => e.UserId == model.userProfile_UserId);
                if (entityVU == null)
                {
                    return CreateVaccinatedUser(model);
                }
                else
                {
                    //VaccinatedUserInfoModel result = new VaccinatedUserInfoModel(entityVU);
                    //return result; 
                    return EditVaccinatedUser(db,model, entityVU);
                }              
            }
        }

        private static VaccinatedUserInfoModel EditVaccinatedUser(Entity.db_HeyDocEntities db, VaccinatedUserInfoModel model, Entity.VaccinatedUserInfo entityVU)
        {
            try
            {
                entityVU.VUserName = model.VUserName;
                entityVU.VPhone = model.VPhone;
                entityVU.VSex = model.VSex;
                entityVU.VAge = model.VAge;
                entityVU.VDob = model.VDob;
                entityVU.VNrc = model.VNrc;
                entityVU.VAddress = model.VAddress;
                entityVU.Remark = model.Remark;
                entityVU.StateId = model.StateId;
                entityVU.TownshipId = model.TownshipId;
                entityVU.NorAe = model.NorAe;
                entityVU.Passport = model.Passport;
                db.SaveChanges();
                VaccinatedUserInfoModel result = new VaccinatedUserInfoModel(entityVU);
                return result;

            }
            catch (Exception ex)
            {
                log.Error(ex);
                throw ex;
            }
        }

        private static VaccinatedUserInfoModel CreateVaccinatedUser(VaccinatedUserInfoModel model)
        {

            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                DateTime now = DateTime.UtcNow;
                Entity.VaccinatedUserInfo vuser = new Entity.VaccinatedUserInfo();

                vuser.UserId = model.userProfile_UserId;
                vuser.VUserName = model.VUserName;
                vuser.VPhone = model.VPhone;
                vuser.VSex = model.VSex;
                vuser.VAge = model.VAge;
                vuser.VDob = model.VDob;
                vuser.VNrc = model.VNrc;
                vuser.VAddress = model.VAddress;
                vuser.Remark = model.Remark;
                vuser.StateId = model.StateId;
                vuser.TownshipId = model.TownshipId;
                vuser.NorAe = model.NorAe;
                vuser.Passport = model.Passport;
                db.VaccinatedUserInfoes.Add(vuser);

                db.SaveChanges();                
                VaccinatedUserInfoModel result = new VaccinatedUserInfoModel(vuser);
                return result;
            }
        }
        internal static VaccinatedUserModel SaveVaccineInfoForUser(VaccinatedUserModel model, HttpRequest request)
        {
            try
            {
                using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
                {
                    Entity.VaccinatedUser vuser = new Entity.VaccinatedUser();

                    vuser.VaccinatedUserId = model.VaccinatedUserId;
                    vuser.UserId = model.userProfile_UserId;
                    vuser.VaccineDetail_Id = model.VaccineDetail_Id;
                    vuser.Dose = model.Dose;
                    vuser.VaccinationDate = model.VaccinationDate;
                    vuser.VaccinationSite = model.VaccinationSite;
                    vuser.LotNoOfVaccine = model.LotNoOfVaccine;
                    vuser.CreatedDate = DateTime.UtcNow;
                    vuser.Status = "Pending";
                    vuser.IsDelete = 0;
                    vuser.RemarkReason = "";
                    db.VaccinatedUsers.Add(vuser);

                    HttpPostedFileBase vaccineCertificacte = new HttpPostedFileWrapper(request.Files["vaccineCertificate"]);
                    SaveVaccineCertificacte(vuser, vaccineCertificacte.InputStream, vaccineCertificacte.FileName);

                    db.SaveChanges();

                    var entityUserList = db.VaccinatedUsers.First(e => e.Vaccination_Id == vuser.Vaccination_Id);

                    VaccinatedUserModel Data = new VaccinatedUserModel(entityUserList);
                    return Data;



                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
                throw ex;
            }
        }

        private static void SaveVaccineCertificacte(Entity.VaccinatedUser vuser, Stream inputStream, string fileName)
        {
            Image originalImage = null;
            Image thumbnailImage = null;
            Stream thumbnailStream = null;
            if (inputStream.Length > 1000000)
            {
               
                originalImage = Image.FromStream(inputStream);
                thumbnailImage = PhotoHelper.GetThumbnail(originalImage, 900); //900kb
                thumbnailStream = ToStream(thumbnailImage, ImageFormat.Png);
            }
            else
            {
                thumbnailStream = inputStream;
            }
            

            var ext = Path.GetExtension(fileName);
            if (string.IsNullOrEmpty(ext))
            {
                ext = ".png";
            }
            string containerName = "u" + vuser.Vaccination_Id.ToString("D5");
            string path = "vaccinecertificate/" + Guid.NewGuid().ToString() + ext;
            var originalBlobUrl = CloudBlob.UploadFile(containerName, path, thumbnailStream);
            vuser.VacCertificateUrl = originalBlobUrl;
        }
        private static Stream ToStream(this Image image, ImageFormat format)
        {
            var stream = new System.IO.MemoryStream();
            image.Save(stream, format);
            stream.Position = 0;
            return stream;
        }
    }
}