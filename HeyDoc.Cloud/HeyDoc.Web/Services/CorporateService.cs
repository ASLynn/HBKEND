using HeyDoc.Web.Models;
using HeyDoc.Web.Helpers;
using HeyDoc.Web.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using HeyDoc.Web.Entity;
using HeyDoc.Web.WebApi;
using ClosedXML.Excel;
using HeyDoc.Web.Resources;
using System.Web.Mvc;

namespace HeyDoc.Web.Services
{
    public class CorporateService
    {
        public static Corporate CreateCorporate(db_HeyDocEntities db, CorporateModel model)
        {
            var entityCorporate = db.Corporates.Create();
            entityCorporate.BranchName = model.BranchName;
            entityCorporate.BranchAddress = model.BranchAddress;
            entityCorporate.CreatedDate = DateTime.UtcNow;
            entityCorporate.TPAId = model.TPAId.HasValue && model.TPAId.Value != 0 ? model.TPAId : null;
            entityCorporate.IsHiddenPublicSelection = model.IsHiddenPublicSelection;
            entityCorporate.IsDelete = false;
            entityCorporate.ThirdPartyCorporateId = model.ThirdPartyCorporateId;
            if (model.SupplyingPharmacyIds != null)
            {
                entityCorporate.SupplyingPharmacies = db.PrescriptionSources.Where(s => model.SupplyingPharmacyIds.Contains(s.PrescriptionSourceId)).ToList();
            }
            entityCorporate.PolicyHasSameDayDelivery = model.PolicyHasSameDayDelivery;
            entityCorporate.PolicySupplyDurationInMonths = model.PolicySupplyDurationInMonths;
            entityCorporate.PolicyRemarks = model.PolicyRemarks;
            entityCorporate.PolicyEMC = model.PolicyEMC;
            entityCorporate.SecretKey = model.SecretKey;
            entityCorporate.MaxSecretKey = model.MaxSecretKey;
            db.Corporates.Add(entityCorporate);

            db.SaveChanges();
            return entityCorporate;
        }

        public static Corporate UpdateCorporate(db_HeyDocEntities db, CorporateModel model)
        {
            var entityCorporate = db.Corporates.FirstOrDefault(e => !e.IsDelete && e.CorporateId == model.CorporateId);
            if (entityCorporate == null)
            {
                throw new WebApiException(new WebApiError(WebApiErrorCode.NotFound, Forms.ErrorCorporateNotFound));
            }
            entityCorporate.BranchName = model.BranchName;
            entityCorporate.BranchAddress = model.BranchAddress;
            entityCorporate.TPAId = model.TPAId.HasValue && model.TPAId.Value != 0 ? model.TPAId : null;
            entityCorporate.IsHiddenPublicSelection = model.IsHiddenPublicSelection;
            entityCorporate.IsDelete = false;
            entityCorporate.SupplyingPharmacies.Clear();
            if (model.SupplyingPharmacyIds != null)
            {
                foreach (var prescriptionSource in db.PrescriptionSources.Where(s => model.SupplyingPharmacyIds.Contains(s.PrescriptionSourceId)))
                {
                    entityCorporate.SupplyingPharmacies.Add(prescriptionSource);
                }
            }
            entityCorporate.PolicyHasSameDayDelivery = model.PolicyHasSameDayDelivery;
            entityCorporate.PolicySupplyDurationInMonths = model.PolicySupplyDurationInMonths;
            entityCorporate.PolicyRemarks = model.PolicyRemarks;
            entityCorporate.PolicyEMC = model.PolicyEMC;
            db.SaveChanges();
            return entityCorporate;
        }

        public static CorporateModel GetCorporateById(int corporateId, bool shouldIncludePolicy = false)
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityCorporate = db.Corporates.Include(e => e.ThirdPartyAdministrator).FirstOrDefault(e => !e.IsDelete && e.CorporateId == corporateId);
                if (entityCorporate == null)
                {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.NotFound, Forms.ErrorCorporateNotFound));
                }
                return new CorporateModel(entityCorporate, shouldIncludePolicy);
            }
        }

        public static BoolResult DeleteCorporate(int corperateId)
        {
            using (db_HeyDocEntities db = new db_HeyDocEntities())
            {
                var entityCorporate = db.Corporates.FirstOrDefault(e => e.CorporateId == corperateId);
                if (entityCorporate == null)
                {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.NotFound, Forms.ErrorCorporateNotFound));
                }
                entityCorporate.IsDelete = true;
                db.SaveChanges();

                return new BoolResult(true);
            }
        }

        private static List<CorporateModel> GetCorporateList(db_HeyDocEntities db, int skip, int take)
        {
            if (take > 2000)
            {
                take = 2000;
            }
            var corporateList = db.Corporates.Where(e => !e.IsDelete && !e.IsHiddenPublicSelection && !e.IsBan).OrderBy(e => e.BranchName).Skip(skip);
            if (take != -1)
            {
                corporateList.Take(take);
            }
            corporateList = corporateList.Include(e => e.ThirdPartyAdministrator);
            List<CorporateModel> modelList = new List<CorporateModel>();
            foreach (var corporate in corporateList)
            {
                modelList.Add(new CorporateModel(corporate));
            }
            return modelList;
        }

        public static IEnumerable<Corporate> GetCorporateListIncludingHidden(db_HeyDocEntities db, int skip = 0, int take = 0)
        {
            var entityCorporates = db.Corporates.Where(e => !e.IsDelete && !e.IsBan).OrderBy(e => e.BranchName).Skip(skip);

            if (take != 0)
            {
                entityCorporates = entityCorporates.Take(take);
            }

            return entityCorporates;
        }

        public static List<CorporateModel> GetCorporateListMobile(int skip, int take)
        {
            List<CorporateModel> modelList;
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                modelList = GetCorporateList(db, skip, take);
            }
            return modelList;
        }

        // TODO M: Possibly rename
        private static List<PharmacyOutletModel> GetPharmacy1OutletList(Entity.db_HeyDocEntities db, Entity.UserProfile entityUser, int skip, int take)
        {
            if (take > 2000)
            {
                take = 2000;
            }

            var entityUserList = db.UserProfiles.Where(e => e.PrescriptionSourceId == ConstantHelper.Pharmacy1PrescriptionSourceId && !e.IsDelete && !e.IsBan).OrderBy(e => e.Nickname).Skip(skip).Take(take).ToList();
            List<PharmacyOutletModel> outletList = new List<PharmacyOutletModel>();

            foreach (var outlet in entityUserList)
            {
                outletList.Add(new PharmacyOutletModel(outlet));
            }
            return outletList;
        }

        // TODO M: Possibly rename
        public static List<PharmacyOutletModel> GetPharmacy1OutletListMobile(string accessToken, int skip, int take)
        {
            ActivityAuditHelper.AddRequestParamsToLog();
            List<PharmacyOutletModel> modelList;
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityUser = AccountService.GetEntityUserByAccessToken(db, accessToken, false);
                modelList = GetPharmacy1OutletList(db, entityUser, skip, take);
            }
            return modelList;
        }

        public static List<PrescriptionLogModel> GetPrescriptionLogStatus(string accessToken, int dispatchId, int skip, int take)
        {
            ActivityAuditHelper.AddRequestParamsToLog();
            List<PrescriptionLogModel> modelList = new List<PrescriptionLogModel>();
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityUser = AccountService.GetEntityUserByAccessToken(db, accessToken, false);
                var entityPrescriptionLogList = db.PrescriptionLogs.Where(e => !e.IsDelete && e.DispatchId == dispatchId && e.PrescriptionDispatch.Prescription.PatientId == entityUser.UserId);
                entityPrescriptionLogList = entityPrescriptionLogList.OrderBy(e => e.CreatedDate).Skip(skip).Take(take);

                foreach (var entityPrescriptionLog in entityPrescriptionLogList)
                {
                    modelList.Add(new PrescriptionLogModel(entityPrescriptionLog));
                }
            }
            return modelList;
        }

        public static BoolResult LoadBranchList(int corporateId, HttpPostedFileBase file)
        {
            using (Entity.db_HeyDocEntities db = new db_HeyDocEntities())
            {
                try
                {
                    using (XLWorkbook wb = new XLWorkbook(file.InputStream))
                    {
                        var ws = wb.Worksheets.FirstOrDefault();

                        string branchName = "STARTSTART";
                        string addressPartial = "";
                        string contact = "";
                        string addressComplete = "";
                        int blankRow = 0;
                        for (int rowNumber = 2; rowNumber <= ws.LastRowUsed().RowNumber(); rowNumber++)
                        {                            
                            IXLRow row = ws.Row(rowNumber);
                            string branch= ws.Cell(rowNumber, 1).GetValue<string>();
                            blankRow++;
                            if (branchName == "ENDEND" || blankRow == 20)
                            {
                                break;
                            }
                            if (!string.IsNullOrEmpty(branch))
                            {
                                if (branch != branchName && branchName != "STARTSTART")
                                {
                                    blankRow = 0;

                                    var entityBranch = db.Branchs.Create();
                                    entityBranch.BranchName = branchName;
                                    entityBranch.BranchAddress = addressComplete;
                                    entityBranch.PhoneNumber = contact;
                                    entityBranch.CorporateId = corporateId;
                                    entityBranch.IsDelete = false;
                                    entityBranch.CreatedDate = DateTime.UtcNow;

                                    db.Branchs.Add(entityBranch);
                                    db.SaveChanges();
                                }
                                addressComplete = "";
                                branchName = branch;
                                contact = ws.Cell(rowNumber, 5).GetValue<string>();
                            }

                            addressPartial = ws.Cell(rowNumber, 4).GetValue<string>();
                            if (!string.IsNullOrEmpty(addressPartial))
                            {
                                addressComplete += " " + addressPartial;
                            }
                        }
                    }                   
                }
                catch (Exception e)
                {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.BadRequest, Errors.ErrorFileInvalid));
                }

            }
            return new BoolResult(true);
        }

        public static BoolResult DeleteBranch(long branchId)
        {
            using (Entity.db_HeyDocEntities db = new db_HeyDocEntities())
            {
                var branch = db.Branchs.FirstOrDefault(e => e.BranchId == branchId);
                if (branch == null)
                {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.NotFound, Forms.ErrorCorporateBranchNotFound));
                }
                branch.IsDelete = true;
                db.SaveChanges();
                return new BoolResult(true);
            }
        }

        public static BoolResult BanCorporate(int corperateId)
        {
            using (db_HeyDocEntities db = new db_HeyDocEntities())
            {
                var entityCorporate = db.Corporates.FirstOrDefault(e => e.CorporateId == corperateId);
                if (entityCorporate == null)
                {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.NotFound, Forms.ErrorCorporateNotFound));
                }
                entityCorporate.IsBan = true;
                db.SaveChanges();

                return new BoolResult(true);
            }
        }
     
        public static List<SelectListItem> GetCorporatePositions(int CorporateId,int PositionId)
        {
            List<SelectListItem> positionList = new List<SelectListItem>();
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                positionList = db.CorporatePositions.Where(c => c.CorporateId == CorporateId).OrderBy(e => e.Position).Select(e => new SelectListItem()
                {
                    Text = e.Position,
                    Value = e.PositionId.ToString(),
                    Selected = PositionId == e.PositionId

                }).ToList();

            }
            return positionList;
        }
        public static List<CorporatePositionModel>GetCorporatePositions(int CorporateId)
        {
            List<CorporatePositionModel> VDList = new List<CorporatePositionModel>();
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var tmpRes = from e in db.CorporatePositions
                             where e.CorporateId == CorporateId
                             orderby e.Position
                             select e;

                foreach (var a in tmpRes)
                {
                    VDList.Add(new CorporatePositionModel(a));
                }
            }
            return VDList;

          
        }
        public static BoolResult UnbanCorporate(int corperateId)
        {
            using (db_HeyDocEntities db = new db_HeyDocEntities())
            {
                var entityCorporate = db.Corporates.FirstOrDefault(e => e.CorporateId == corperateId);
                if (entityCorporate == null)
                {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.NotFound, Forms.ErrorCorporateNotFound));
                }
                entityCorporate.IsBan = false;
                db.SaveChanges();

                return new BoolResult(true);
            }
        }

    }
}