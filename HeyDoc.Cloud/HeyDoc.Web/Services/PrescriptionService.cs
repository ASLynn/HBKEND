using HeyDoc.Web.Entity;
using HeyDoc.Web.Helpers;
using HeyDoc.Web.Lib;
using HeyDoc.Web.Models;
using HeyDoc.Web.WebApi;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using Fissoft.EntityFramework.Fts;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Data.Entity;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using HeyDoc.Web.Resources;
using System.Threading.Tasks;
using Microsoft.Azure;
using System.Text.RegularExpressions;
using WebMatrix.WebData;

namespace HeyDoc.Web.Services
{
    public class PrescriptionService
    {
        private const string prescriptionBlobContainer = "prescription";
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static async Task<(bool IsSuccess, string ErrorMessage)> CreatePrescriptionFromChatBot(db_HeyDocEntities db, Entity.UserProfile patientUser, ChatBotSession chatBotSession)
        {
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var chatBotResponsesWithMedicalCondition = ChatBotService.GetChatBotResponsesByTag(db, chatBotSession.Id, "MedicalConditionName");

                    var medicalConditions = new List<string>();
                    foreach (var chatBotResponse in chatBotResponsesWithMedicalCondition)
                    {
                        var jsonObject = JsonConvert.DeserializeObject<Dictionary<string, string>>(chatBotResponse.JsonValue);
                        medicalConditions.Add(jsonObject["MedicalConditionName"]);
                    }

                    medicalConditions = medicalConditions.Distinct().ToList();

                    var medicalSummary = string.Join(",", medicalConditions);
                    medicalSummary = string.IsNullOrEmpty(medicalSummary) ? null : medicalSummary;

                    var doctorId = chatBotSession.ChatRoom.DoctorId;
                    var entityPrescription = db.Prescriptions.Create();
                    entityPrescription.ChatRoomId = chatBotSession.ChatRoomId;
                    entityPrescription.CreateDate = DateTime.UtcNow;
                    entityPrescription.DoctorId = doctorId;
                    entityPrescription.IsDelete = false;
                    entityPrescription.PatientId = chatBotSession.ChatRoom.PatientId;
                    entityPrescription.MedicalSummary = medicalSummary;
                    entityPrescription.Allergy = chatBotSession.ChatRoom.Patient.Allergy;
                    var entityWeight = chatBotSession.ChatRoom.Patient.WeightHistories.OrderByDescending(e => e.CreateDate).FirstOrDefault();
                    var entityHeight = chatBotSession.ChatRoom.Patient.HeightHistories.OrderByDescending(e => e.CreateDate).FirstOrDefault();
                    var heightStr = entityHeight != null ? entityHeight.Height.ToString() + "cm" : "";
                    var weightStr = entityWeight != null ? entityWeight.Weight.ToString() + "kg" : "";
                    entityPrescription.Height = heightStr.Length > 9 ? "" : heightStr;
                    entityPrescription.Weight = weightStr.Length > 9 ? "" : weightStr;
                    entityPrescription.IsDispensed = false;
                    entityPrescription.Identifier1 = Guid.NewGuid();
                    entityPrescription.Identifier2 = Guid.NewGuid();
                    entityPrescription.FrontEndSource = PrescriptionFrontEndSource.CHATBOT;
                    entityPrescription.PrescriptionAvailabilityStatus = PrescriptionAvailabilityStatus.New;
                    entityPrescription.FileUrl = $"{ConstantHelper.ServerUrl}/Prescription/prescription?prescriptionId={entityPrescription.PrescriptionId}&p1={entityPrescription.Identifier1}&p2={entityPrescription.Identifier2}";
                    entityPrescription.ChatBotSessionId = chatBotSession.Id;
                    entityPrescription.MedicationType = PrescriptionMedicationType.LTM;
                    entityPrescription.ProcessingStatus = ProcessingStatus.Processing;
                    db.Prescriptions.Add(entityPrescription);
                    db.SaveChanges();

                    var chatBotResponsesWithMedicationId = ChatBotService.GetChatBotResponsesByTag(db, chatBotSession.Id, "MedicationId");
                    foreach (var chatBotResponse in chatBotResponsesWithMedicationId)
                    {
                        var jsonObject = JsonConvert.DeserializeObject<Dictionary<string, string>>(chatBotResponse.JsonValue);
                        if (!int.TryParse(jsonObject["MedicationId"], out var medicationId))
                        {
                            return (false, $"Invalid medication ID: {jsonObject["MedicationId"]}");
                        };
                        var drug = DrugService.GetLatestUserDrugByMedicationId(db, medicationId, patientUser.UserId);

                        var entityDrug = db.Drugs.Create();
                        entityDrug.MedicationId = medicationId;
                        entityDrug.DrugName = drug.DrugName;
                        entityDrug.Amount = drug.Amount;
                        entityDrug.Status = drug.Status;
                        entityDrug.Dosage = drug.Dosage;
                        entityDrug.Frequency = drug.Frequency;
                        entityDrug.Remark = drug.Remark;
                        entityDrug.Route = drug.Route;
                        entityDrug.IsDelete = false;
                        entityPrescription.Drugs.Add(entityDrug);
                    }
                    db.SaveChanges();

                    await NotificationService.NotifyUser(db, doctorId, PnActionType.PrescriptionStatus, entityPrescription.PrescriptionId, $"Chat assistant has prepared a prescription for {patientUser.FullName}. Kindly proceed to https://web.doc2us.com to review prescription.");

                    transaction.Commit();
                    return (true, null);
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    log.Error(ex);
                    return (false, ex.Message);
                }
            }
        }

        public static async Task<PrescriptionModel> CreatePrescription(string accessToken, int chatRoomId, PrescriptionModel model)
        {
            using (var db = new Entity.db_HeyDocEntities())
            {
                model.Validate();
                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        var entityUser = AccountService.GetEntityUserByAccessToken(db, accessToken, false);
                        var doctorRoles = ConstantHelper.DoctorRoles;
                        if (!entityUser.Roles.Any(r => doctorRoles.Contains(r)))
                        {
                            throw new WebApiException(new WebApiError(WebApiErrorCode.Unauthorized, string.Format(Errors.UnauthorizedRole, ConstantHelper.Doc2UsEmailContact)));
                        }

                        var entityChatRoom = db.ChatRooms.FirstOrDefault(e => e.ChatRoomId == chatRoomId && e.DoctorId == entityUser.UserId);

                        if (entityChatRoom == null)
                        {
                            throw new WebApiException(new WebApiError(WebApiErrorCode.NotFound, "Chat not found"));
                        }
                        if (entityUser.Roles.Contains(RoleType.User))
                        {
                            throw new WebApiException(new WebApiError(WebApiErrorCode.Unauthorized, string.Format(Errors.UnauthorizedRole, ConstantHelper.Doc2UsEmailContact)));
                        }
                        if (model.SignatureCredential == null || !WebSecurity.Login(entityUser.UserName, model.SignatureCredential.CertificatePassword))
                        {
                            throw new WebApiException(new WebApiError(WebApiErrorCode.Unauthorized, Errors.InvalidCredentials));
                        }

                        CheckPatientCanBePrescribed(entityChatRoom.Patient);

                        var entityPrescription = db.Prescriptions.Create();
                        entityPrescription.ChatRoomId = chatRoomId;
                        entityPrescription.CreateDate = DateTime.UtcNow;
                        entityPrescription.DoctorId = entityUser.UserId;
                        entityPrescription.IsDelete = false;
                        entityPrescription.PatientId = entityChatRoom.PatientId;
                        entityPrescription.MedicalSummary = model.MedicalSummary;
                        entityPrescription.Allergy = entityChatRoom.Patient.Allergy;
                        var entityWeight = entityChatRoom.Patient.WeightHistories.OrderByDescending(e => e.CreateDate).FirstOrDefault();
                        var entityHeight = entityChatRoom.Patient.HeightHistories.OrderByDescending(e => e.CreateDate).FirstOrDefault();
                        var heightStr = entityHeight != null ? entityHeight.Height.ToString() + "cm" : "";
                        var weightStr = entityWeight != null ? entityWeight.Weight.ToString() + "kg" : "";
                        entityPrescription.Height = heightStr.Length > 9 ? "" : heightStr;
                        entityPrescription.Weight = weightStr.Length > 9 ? "" : weightStr;
                        entityPrescription.IsDispensed = false;
                        entityPrescription.Identifier1 = Guid.NewGuid();
                        entityPrescription.Identifier2 = Guid.NewGuid();
                        entityPrescription.MedicationType = model.MedicationType ?? PrescriptionMedicationType.Unspecified;
                        entityPrescription.FrontEndSource = PrescriptionFrontEndSource.CHAT;
                        entityPrescription.PrescriptionAvailabilityStatus = PrescriptionAvailabilityStatus.Approved;
                        entityPrescription.ProcessingStatus = ProcessingStatus.Processing;
                        db.Prescriptions.Add(entityPrescription);
                        db.SaveChanges();

                        if (model.IcdEntries != null && model.IcdEntries.Count > 0)
                        {
                            foreach (var icdEntry in model.IcdEntries)
                            {
                                var entityIcd = db.Icds.FirstOrDefault(e => e.IcdEntityId == icdEntry.IcdEntityId && e.IcdLinearizationCode == icdEntry.LinearizationName && e.IcdReleaseVer == icdEntry.ReleaseId);
                                if (entityIcd != null)
                                {
                                    entityPrescription.Icds.Add(entityIcd);
                                }
                                else
                                {
                                    entityIcd = db.Icds.Create();
                                    entityIcd.IcdEntityId = icdEntry.IcdEntityId;
                                    entityIcd.IcdCode = icdEntry.IcdCode;
                                    entityIcd.IcdCodeDesc = icdEntry.IcdCodeDescription;
                                    entityIcd.IcdReleaseVer = icdEntry.ReleaseId;
                                    entityIcd.IcdLinearizationCode = icdEntry.LinearizationName;
                                    entityPrescription.Icds.Add(entityIcd);
                                }
                            }
                        }
                        db.SaveChanges();

                        foreach (var drug in model.Drugs)
                        {
                            var entityMedication = MedicationService.GetEntityMedicationById(db, drug.MedicationId);
                            var totalQuantity = 0;
                            var amount = drug.Amount;
                            if (entityMedication.IsLTM && drug.DurationInMonth <= 0)
                            {
                                throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidArguments, $"Please input the total duration in month for {drug.MedicationName} as it falls under the long term medication category."));
                            }
                            amount = new String(drug.Amount.TakeWhile(Char.IsDigit).ToArray());
                            if (!int.TryParse(amount, out totalQuantity))
                            {
                                throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidArguments, $"Please input number only as amount for {drug.MedicationName}."));
                            }
                            if (totalQuantity <= 0)
                            {
                                throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidArguments, $"Please input a valid amount for {drug.MedicationName} as it falls under the long term medication category."));
                            }
                            var entityDrug = db.Drugs.Create();
                            entityDrug.MedicationId = entityMedication.MedicationId;
                            entityDrug.DrugName = entityMedication.MedicationName ?? " ";
                            entityDrug.DurationInMonth = drug.DurationInMonth;
                            entityDrug.TotalQuantity = totalQuantity;
                            entityDrug.Amount = drug.Amount;
                            entityDrug.Status = drug.Status;
                            entityDrug.Dosage = drug.Dosage;
                            entityDrug.Frequency = drug.Frequency;
                            entityDrug.PrescriptionId = entityPrescription.PrescriptionId;
                            entityDrug.Remark = drug.Remark;
                            entityDrug.Route = drug.Route;
                            entityDrug.IsDelete = false;
                            db.Drugs.Add(entityDrug);
                        }
                        db.SaveChanges();

                        var notificationType = PnActionType.Prescription;
                        string relatedId = entityChatRoom.ChatRoomId.ToString();
                        Dictionary<string, string> extraParams = new Dictionary<string, string>();
                        if (entityChatRoom.Patient.UserProfile.CorporateId.HasValue)
                        {
                            notificationType = PnActionType.CorporateUserPrescription;
                        }
                        if (entityChatRoom.Patient.UserProfile.CorporateId.HasValue)
                        {
                            extraParams.Add("PrescriptionId", entityPrescription.PrescriptionId.ToString());
                        }
                        await NotificationService.NotifyUser(db, new List<int>() { entityChatRoom.PatientId }, notificationType, relatedId, string.Format("New prescription added by {0}", entityUser.FullName), extraParams);

                        var result = new PrescriptionModel(entityPrescription, true);
                        transaction.Commit();
                        return result;
                    }
                    catch (Exception ex)
                    {
                        if (ex is WebApiException)
                        {
                            throw;
                        }
                        log.Error(ex);
                        throw new WebApiException(new WebApiError(WebApiErrorCode.UnknownError, Errors.GenericError));
                    }
                }
            }
        }

        public static List<PrescriptionModel> GetPrescriptionList(string accessToken, int chatRoomId, int take = 15, int skip = 0)
        {
            ActivityAuditHelper.AddRequestParamsToLog();
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityUser = AccountService.GetEntityUserByAccessToken(db, accessToken, false);

                var entityChatroom = db.ChatRooms.FirstOrDefault(e => e.ChatRoomId == chatRoomId && !e.IsDelete);
                if (entityChatroom == null)
                {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.NotFound, Resources.Chat.ErrorRoomNotFound));
                }

                var entityPrescriptionList = db.Prescriptions.Where(e => e.PatientId == entityChatroom.PatientId && e.DoctorId == entityChatroom.DoctorId && !e.IsDelete);
                if (entityUser.Roles.Contains(RoleType.User))
                {
                    // Only list prescriptions created or approved by doctor for patients
                    entityPrescriptionList = entityPrescriptionList.Where(e => e.PatientId == entityUser.UserId && (e.PrescriptionAvailabilityStatus == PrescriptionAvailabilityStatus.NotApplicable || e.PrescriptionAvailabilityStatus == PrescriptionAvailabilityStatus.Approved));
                }

                entityPrescriptionList = entityPrescriptionList.OrderByDescending(e => e.CreateDate).Skip(skip).Take(take);

                List<PrescriptionModel> result = new List<PrescriptionModel>();

                foreach (var entityPrescription in entityPrescriptionList)
                {
                    result.Add(new PrescriptionModel(entityPrescription, false));
                }

                return result;
            }
        }


        public static List<Prescription> GetPrescriptionsByMedicationId(Entity.db_HeyDocEntities db, int userId, long medicationId)
        {
            //userid is patientid
            return db.Prescriptions
                     .Include("Drugs")
                     .Where(x => x.PatientId == userId &&
                                 !x.IsDelete &&
                                 x.Drugs.Any(y => y.MedicationId == medicationId))
                     .ToList();
        }

        public static List<PrescriptionModel> GetPrescriptionList(string accessToken, int take = 15, int skip = 0)
        {
            ActivityAuditHelper.AddRequestParamsToLog();
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var user = AccountService.GetEntityUserByAccessToken(db, accessToken, false);

                var prescriptions = db.Prescriptions
                        .Where(e => !e.IsDelete && (e.DoctorId == user.UserId || e.PatientId == user.UserId) && (e.ChatRoomId.HasValue || e.PrescriptionAvailabilityStatus == PrescriptionAvailabilityStatus.Approved));

                // Only list prescriptions created or approved by doctor for patients
                if (user.Roles.Contains(RoleType.User))
                {
                    prescriptions = prescriptions.Where(e => e.PrescriptionAvailabilityStatus == PrescriptionAvailabilityStatus.NotApplicable || e.PrescriptionAvailabilityStatus == PrescriptionAvailabilityStatus.Approved);
                }
                prescriptions = prescriptions
                    .OrderByDescending(e => e.CreateDate)
                    .Skip(skip)
                    .Take(take);

                var result = new List<PrescriptionModel>();

                foreach (var prescription in prescriptions)
                {
                    result.Add(new PrescriptionModel(prescription, false));
                }

                return result;
            }
        }

        public static List<PrescriptionModel> GetPrescriptionListByStatus(string accessToken, PrescriptionAvailabilityStatus? status, int take = 15, int skip = 0)
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var user = AccountService.GetEntityUserByAccessToken(db, accessToken, false);
                var doctorRoles = ConstantHelper.DoctorRoles;
                if (!user.Roles.Any(r => doctorRoles.Contains(r)))
                {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.Unauthorized, string.Format(Errors.UnauthorizedRole, ConstantHelper.Doc2UsEmailContact)));
                }

                var result = new List<PrescriptionModel>();

                var prescriptions = db.Prescriptions.Where(e => !e.IsDelete && e.DoctorId == user.UserId && e.PatientId != null && !e.Patient.UserProfile.IsDelete && !e.Patient.UserProfile.IsBan);

                switch (status)
                {
                    case PrescriptionAvailabilityStatus.New:
                        prescriptions = prescriptions.Where(e => e.PrescriptionAvailabilityStatus == PrescriptionAvailabilityStatus.New);
                        break;
                    case PrescriptionAvailabilityStatus.Approved:
                        prescriptions = prescriptions.Where(e => e.PrescriptionAvailabilityStatus == PrescriptionAvailabilityStatus.Approved);
                        break;
                    case PrescriptionAvailabilityStatus.Rejected:
                        prescriptions = prescriptions.Where(e => e.PrescriptionAvailabilityStatus == PrescriptionAvailabilityStatus.Rejected);
                        break;
                }

                prescriptions = prescriptions.OrderBy(e => e.CreateDate).Skip(skip).Take(take);

                foreach (var prescription in prescriptions)
                {
                    result.Add(new PrescriptionModel(prescription, false));
                }
                return result;
            }
        }

        public static PrescriptionModel GetPrescriptionById(string accessToken, long prescriptionId)
        {
            ActivityAuditHelper.AddRequestParamsToLog();
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityUser = AccountService.GetEntityUserByAccessToken(db, accessToken, false);
                Entity.Prescription entityPrescription = null;
                if (entityUser.Roles.Contains(RoleType.User))
                {
                    entityPrescription = db.Prescriptions.FirstOrDefault(e => e.PrescriptionId == prescriptionId && e.PatientId == entityUser.UserId);
                    if (entityPrescription?.PrescriptionAvailabilityStatus == PrescriptionAvailabilityStatus.Rejected)
                    {
                        throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidAction, "You are not allowed to view this prescription as it has been rejected by the doctor"));
                    }
                }
                else
                {
                    entityPrescription = db.Prescriptions.FirstOrDefault(e => e.PrescriptionId == prescriptionId);
                }

                if (entityPrescription == null)
                {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.NotFound, Forms.ErrorPrescriptionNotFound));
                }
                var result = new PrescriptionModel(entityPrescription, true)
                {
                    Identifier1 = entityPrescription.Identifier1,
                    Identifier2 = entityPrescription.Identifier2
                };
                if (!string.IsNullOrEmpty(entityPrescription.FileUrl))
                    result.PdfUrl = GetBlobSignedPdfUrl(entityPrescription.FileUrl);

                return result;
            }
        }

        public static BoolResult DeletePrescription(string accessToken, long prescriptionId)
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityUser = AccountService.GetEntityUserByAccessToken(db, accessToken, false);
                var doctorRoles = ConstantHelper.DoctorRoles;
                if (!entityUser.Roles.Any(r => doctorRoles.Contains(r)))
                {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.Unauthorized, string.Format(Errors.UnauthorizedRole, ConstantHelper.Doc2UsEmailContact)));
                }

                return DeletePrescription(db, prescriptionId, entityUser);
            }
        }

        public static BoolResult DeletePrescription(db_HeyDocEntities db, long prescriptionId, Entity.UserProfile entityDoctor = null)
        {
            Prescription entityPrescription;
            if (entityDoctor != null)
            {
                entityPrescription = db.Prescriptions.FirstOrDefault(e => e.PrescriptionId == prescriptionId && e.DoctorId == entityDoctor.UserId);
            }
            else
            {
                entityPrescription = db.Prescriptions.FirstOrDefault(e => e.PrescriptionId == prescriptionId);
            }

            if (entityPrescription == null)
            {
                throw new WebApiException(new WebApiError(WebApiErrorCode.NotFound, Forms.ErrorPrescriptionNotFound));
            }
            entityPrescription.IsDelete = true;
            db.SaveChanges();
            //CloudBlob.DeleteImage(entityPrescription.FileUrl);
            return new BoolResult(true);
        }

        public static BoolResult MarkDispensed(string accessToken, long prescriptionId, bool isDispensed)
        {
            //Api no longer needed
            throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidAction, "Please update your app and proceed."));
        }

        public static async Task<BoolResult> MarkPrescriptionDispensed(string accessToken, long prescriptionId, DispensePrescriptionModel model)
        {
            var signature = model as DigitalSignatureModel;
            using (var db = new Entity.db_HeyDocEntities())
            {
                using (var transaction = db.Database.BeginTransaction())
                {
                    var user = AccountService.GetEntityUserByAccessToken(db, accessToken, false);
                    var pharmacyRoles = new HashSet<RoleType> { RoleType.Pharmacy };
                    var doctorRoles = ConstantHelper.DoctorRoles;
                    var authorizedRoles = pharmacyRoles.Union(doctorRoles);
                    if (!user.Roles.Intersect(authorizedRoles).Any())
                    {
                        throw new WebApiException(new WebApiError(WebApiErrorCode.Unauthorized, Errors.UnauthorizedNotPartner));
                    }

                    var prescription = db.Prescriptions.Include(e => e.Drugs).FirstOrDefault(e => e.PrescriptionId == prescriptionId && !e.IsDelete);
                    if (prescription == null)
                    {
                        throw new WebApiException(new WebApiError(WebApiErrorCode.NotFound, Forms.ErrorPrescriptionNotFound));
                    }
                    if (prescription.IsDispensed)
                    {
                        throw new WebApiException(new WebApiError(WebApiErrorCode.NotFound, Medications.ErrorPrescriptionDispensed));
                    }
                    if (!(prescription.PrescriptionAvailabilityStatus == PrescriptionAvailabilityStatus.NotApplicable || prescription.PrescriptionAvailabilityStatus == PrescriptionAvailabilityStatus.Approved))
                    {
                        throw new WebApiException(new WebApiError(WebApiErrorCode.NotFound, Medications.ErrorPrescriptionNotApproved));
                    }
                    if (model.DrugDispensedAmounts != null)
                    {
                        foreach (var drugId in model.DrugDispensedAmounts.Keys)
                        {
                            var entityDrug = prescription.Drugs.FirstOrDefault(e => e.DrugId == drugId && !e.IsDelete);
                            if (entityDrug == null)
                            {
                                throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidArguments, $"Error recording supplied amounts (Could not find drug ID {drugId} for prescription {prescriptionId}). Please try again"));
                            }
                            var dispensedAmount = model.DrugDispensedAmounts[drugId];
                            var regexMatch = Regex.Match(dispensedAmount, @"^(?<amount>\d+) +(?<unit>\w.*)$");
                            if (!regexMatch.Success || !int.TryParse(regexMatch.Groups["amount"].Value, out var dispensedAmountNumber))
                            {
                                throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidArguments, "Please enter supplied amounts as \"{number} {unit}\", e.g. \"10 tablets\""));
                            }
                            var dispensedAmountUnit = regexMatch.Groups["unit"].Value;
                            if (!dispensedAmountUnit.IsWithinNvarcharLimit(30))
                            {
                                throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidArguments, "Supplied amount units must be 30 characters or less"));
                            }
                            entityDrug.DispensedAmount = dispensedAmountNumber;
                            entityDrug.DispensedAmountUnit = dispensedAmountUnit;
                        }
                    }

                    var dispensingUserId = user.UserId;
                    if (Path.GetExtension(prescription.FileUrl) == ".pdf")
                    {
                        if (signature == null || string.IsNullOrEmpty(signature.IC) || string.IsNullOrEmpty(signature.CertificatePassword))
                        {
                            throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidArguments, Forms.ErrorDigSignNull));
                        }
                        string signatureImage = "";
                        if (user.Roles.Any(r => pharmacyRoles.Contains(r)))
                        {
                            if (string.IsNullOrEmpty(signature.IC))
                                throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidArguments, Forms.ErrorICNull));
                            var pharmacist = db.UserProfiles.FirstOrDefault(e => e.IC == signature.IC && e.Doctor != null && !e.IsDelete);
                            if (pharmacist == null)
                            {
                                throw new WebApiException(new WebApiError(WebApiErrorCode.UserNotRegistered, "User not found, Please register with HOPE. Please contact your respective pharmacy manager for further action"));
                            }
                            if (pharmacist.IsBan)
                            {
                                throw new WebApiException(new WebApiError(WebApiErrorCode.UserIsBanned, Account.ErrorBanned));
                            }
                            if (!pharmacist.Doctor.IsVerified)
                            {
                                throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidAction, "Your account has not been verified, Please contact your respective pharmacy manager for further actions."));
                            }
                            dispensingUserId = pharmacist.UserId;
                            prescription.DispensedOutlet = user.UserId;
                            signatureImage = Convert.ToBase64String(CloudBlob.Instance.DownloadFile(pharmacist.Doctor.SignatureUrl));
                        }
                        else if (user.Roles.Any(r => doctorRoles.Contains(r)))
                        {
                            if (string.IsNullOrEmpty(user.IC))
                            {
                                throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidArguments, "Please update your IC with HOPE to digitally sign this prescription. Please contact Dr. Raymond Choy for assistance."));
                            }
                            signature.IC = user.IC;
                            signatureImage = Convert.ToBase64String(CloudBlob.Instance.DownloadFile(user.Doctor.SignatureUrl));
                        }
                    }

                    if (prescription.PrescriptionStatus == PrescriptionStatus.SelfCollection || prescription.PrescriptionStatus == PrescriptionStatus.Delivery)
                    {
                        await SendPrescriptionDispensedNotification(prescription.PrescriptionId);
                    }

                    prescription.DispensedBy = dispensingUserId;
                    prescription.IsDispensed = true;
                    prescription.DispensedDate = DateTime.UtcNow;
                    db.SaveChanges();
                    transaction.Commit();
                    return new BoolResult(true);
                }
            }
        }

        public static async Task SendMedicationRequestNotification(db_HeyDocEntities db, long prescriptionId)
        {
            var log = new PrescriptionLogModel()
            {
                LogText = "Dear valued customer, thank you for ordering medication(s) from us. We will process your prescription and get back to you as soon as possible.",
                LogType = PnActionType.Message
            };
            await AddLogPrescription(db, prescriptionId, log);
            var storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["StorageConnection"].ConnectionString);
            var queueClient = storageAccount.CreateCloudQueueClient();
            var queue = queueClient.GetQueueReference("prescription-processing-pns");
            queue.CreateIfNotExists();
            queue.AddMessage(new CloudQueueMessage(prescriptionId.ToString()), initialVisibilityDelay: new TimeSpan(1, 0, 0));
        }

        public static async Task SendPrescriptionDispensedNotification(long prescriptionId)
        {
            using (var db = new Entity.db_HeyDocEntities())
            {
                var patient = db.UserProfiles.FirstOrDefault(e => !e.IsDelete && !e.IsBan && e.Patient.Prescriptions.Any(p => p.PrescriptionId == prescriptionId && !p.IsDelete));
                if (patient == null)
                {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.NotFound));
                }
                var log = new PrescriptionLogModel
                {
                    LogText = $"Hi {patient.FullName}, your e-Prescription ({prescriptionId}) has been digitally supplied by the pharmacist.",
                    LogType = PnActionType.Message
                };
                await AddLogPrescription(db, prescriptionId, log);
            }
        }

        public static void SendPrescriptionCheckoutEmail(Prescription prescription)
        {
            if (prescription.MedicationType == PrescriptionMedicationType.Unspecified)
            {
                return;
            }
            string prescriptionType;
            switch (prescription.MedicationType)
            {
                case PrescriptionMedicationType.MinorAilment:
                    prescriptionType = "Minor Ailment";
                    break;
                case PrescriptionMedicationType.LTM:
                    prescriptionType = "LTM";
                    break;
                default:
                    prescriptionType = "";
                    break;
            }

            string requestType;
            switch (prescription.PrescriptionStatus)
            {
                case PrescriptionStatus.SelfCollection:
                    requestType = "Self Collection";
                    break;
                case PrescriptionStatus.Delivery:
                    requestType = "Delivery";
                    break;
                case PrescriptionStatus.OnSite:
                    requestType = "On Site Collection";
                    break;
                default:
                    requestType = "";
                    break;
            }
#if DEBUG
            var recepientList = CloudConfigurationManager.GetSetting("Doc2UsDevEmails").Split(',').ToList();
            var subject = $"[HOPE Staging] E-Prescription ID: {prescription.PrescriptionId} has been requested ({prescriptionType})";
#else
            // TODO M UNBLANK: Admin email as recepient
            var recepientList = new List<string>
                {
                    ""
                };
            var subject = $"[Doc2Us] E-Prescription ID: {prescription.PrescriptionId} has been requested ({prescriptionType})";
#endif
            var emailBody = string.Format(EmailHelper.ConvertEmailHtmlToString(@"Emails/PrescriptionRequested.html"), prescription.PrescriptionId, prescription.Patient.UserProfile.FullName, prescription.Patient.UserProfile.Corporate?.BranchName ?? "-", prescription.MedicalSummary, requestType, prescriptionType);
            EmailHelper.SendViaSendGrid(recepientList, subject, emailBody, string.Empty, null, null, true);
        }

        public static PrescriptionModel RequestMedication(string accessToken, long prescriptionId, PrescriptionStatus prescriptionStatus, int? Id = null, string deliveryAddress = "")
        {
            PrescriptionModel model;
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityUser = AccountService.GetEntityUserByAccessToken(db, accessToken, false, RoleType.User);
                var entityDevice = DeviceService.GetEntityDevice(db, accessToken, false);
                if (entityUser.CorporateId == null)
                {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.Unauthorized, string.Format(Errors.UnauthorizedRole, ConstantHelper.Doc2UsEmailContact)));
                }

                var entityPrescription = db.Prescriptions.FirstOrDefault(e => e.PrescriptionId == prescriptionId);
                _ValidatePrescriptionCheckout(entityPrescription);

                var dispatchModel = CreateDispatch(db, prescriptionId, prescriptionStatus, DispatchStatus.Requested, Id, deliveryAddress);
                entityPrescription.PrescriptionStatus = prescriptionStatus;
                db.SaveChanges();

                model = new PrescriptionModel(entityPrescription, false);
                model.Dispatch = new PrescriptionDispatchModel(dispatchModel);
            }
            return model;
        }

        public static async Task<PrescriptionModel> RequestMedicationOnsite(string accessToken, long prescriptionId, int Id, PackingType packingType = PackingType.NotSpecified)
        {
            PrescriptionModel model;
            var prescriptionStatus = PrescriptionStatus.OnSite;
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityOnsite = db.OnSiteDispenses.FirstOrDefault(e => !e.IsDelete && e.OnSiteId == Id);
                if (entityOnsite == null)
                {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.NotFound, Location.ErrorLocationNotFound));
                }
                OnSiteDispenseModel onSiteModel = new OnSiteDispenseModel(entityOnsite);

                var entityUser = AccountService.GetEntityUserByAccessToken(db, accessToken, false, RoleType.User);
                var entityDevice = DeviceService.GetEntityDevice(db, accessToken, false);
                if (entityUser.CorporateId == null)
                {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.Unauthorized, string.Format(Errors.UnauthorizedRole, ConstantHelper.Doc2UsEmailContact)));
                }

                var entityPrescription = db.Prescriptions.FirstOrDefault(e => e.PrescriptionId == prescriptionId);
                _ValidatePrescriptionCheckout(entityPrescription);

                string deliveryAddress = "";
                var dispatchModel = CreateDispatch(db, prescriptionId, prescriptionStatus, DispatchStatus.Requested, Id, deliveryAddress, packingType);
                entityPrescription.PrescriptionStatus = prescriptionStatus;
                if (onSiteModel.SelectionDateDispense != null)
                {
                    entityPrescription.NextDispenseDateTime = onSiteModel.SelectionDateDispense;
                }
                await SendMedicationRequestNotification(db, prescriptionId);

                db.SaveChanges();
                SendPrescriptionCheckoutEmail(entityPrescription);

                model = new PrescriptionModel(entityPrescription, false);
                model.Dispatch = new PrescriptionDispatchModel(dispatchModel);
            }
            return model;
        }

        public static async Task<PrescriptionModel> RequestMedicationSelfCollection(string accessToken, long prescriptionId, int Id, PackingType packingType = PackingType.NotSpecified)
        {
            ActivityAuditHelper.AddRequestParamsToLog();
            PrescriptionModel model;
            var prescriptionStatus = PrescriptionStatus.SelfCollection;
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityUser = AccountService.GetEntityUserByAccessToken(db, accessToken, false, RoleType.User);
                var entityDevice = DeviceService.GetEntityDevice(db, accessToken, false);
                var entityPrescription = db.Prescriptions.FirstOrDefault(e => e.PrescriptionId == prescriptionId);
                ValidateSelfPickupOrder(entityUser, entityDevice, entityPrescription);

                string deliveryAddress = "";
                var dispatchModel = CreateDispatch(db, prescriptionId, prescriptionStatus, DispatchStatus.Requested, Id, deliveryAddress, packingType);
                entityPrescription.PrescriptionStatus = prescriptionStatus;
                await SendMedicationRequestNotification(db, prescriptionId);
                db.SaveChanges();
                SendPrescriptionCheckoutEmail(entityPrescription);

                model = new PrescriptionModel(entityPrescription, false);
                model.Dispatch = new PrescriptionDispatchModel(dispatchModel);
            }
            return model;
        }

        public static void ValidateSelfPickupOrder(Entity.UserProfile entityUser, Entity.Device entityDevice, Prescription entityPrescription)
        {
            if (entityUser.CorporateId == null)
            {
                throw new WebApiException(new WebApiError(WebApiErrorCode.Unauthorized, string.Format(Errors.UnauthorizedRole, ConstantHelper.Doc2UsEmailContact)));
            }
            if (string.IsNullOrEmpty(entityUser.Address) || entityUser.Address == "INTERNATIONAL_PATIENT" || string.IsNullOrEmpty(entityUser.PhoneNumber) || entityUser.PhoneNumber == "00000000001")
            {
                throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidAction, "Please update your address and phone number. Go to Settings > Edit Profile"));
            }
            _ValidatePrescriptionCheckout(entityPrescription);
        }

        public static async Task<PrescriptionModel> RequestMedicationDelivery(string accessToken, long prescriptionId, string deliveryAddress, PackingType packingType = PackingType.NotSpecified)
        {
            ActivityAuditHelper.AddRequestParamsToLog();
            PrescriptionModel model;
            var prescriptionStatus = PrescriptionStatus.Delivery;
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityUser = AccountService.GetEntityUserByAccessToken(db, accessToken, false, RoleType.User);
                var entityDevice = DeviceService.GetEntityDevice(db, accessToken, false);
                if (entityUser.CorporateId == null)
                {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.Unauthorized, string.Format(Errors.UnauthorizedRole, ConstantHelper.Doc2UsEmailContact)));
                }
                if (string.IsNullOrEmpty(deliveryAddress) || deliveryAddress == "INTERNATIONAL_PATIENT" || string.IsNullOrEmpty(entityUser.PhoneNumber) || entityUser.PhoneNumber == "00000000001")
                {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidAction, "Please update your address and phone number. Go to Settings > Edit Profile"));
                }
                var entityPrescription = db.Prescriptions.FirstOrDefault(e => e.PrescriptionId == prescriptionId);
                _ValidatePrescriptionCheckout(entityPrescription);

                var dispatchModel = CreateDispatch(db, prescriptionId, prescriptionStatus, DispatchStatus.Requested, null, deliveryAddress, packingType);
                entityPrescription.PrescriptionStatus = prescriptionStatus;
                await SendMedicationRequestNotification(db, prescriptionId);
                db.SaveChanges();
                SendPrescriptionCheckoutEmail(entityPrescription);

                model = new PrescriptionModel(entityPrescription, false);
                model.Dispatch = new PrescriptionDispatchModel(dispatchModel);
            }
            return model;
        }

        public static PrescriptionDispatch CreateDispatch(Entity.db_HeyDocEntities db, long prescriptionId, PrescriptionStatus prescriptionStatus, DispatchStatus dispatchStatus, int? Id = null, string deliveryAddress = "", PackingType packingType = PackingType.NotSpecified)
        {
            var entityDispatch = db.PrescriptionDispatchs.Where(e => e.PrescriptionId == prescriptionId).OrderByDescending(o => o.CreatedDate).FirstOrDefault();
            if (entityDispatch == null)
            {
                entityDispatch = db.PrescriptionDispatchs.Create();
                entityDispatch.PrescriptionId = prescriptionId;

                entityDispatch.CreatedDate = DateTime.UtcNow;
                db.PrescriptionDispatchs.Add(entityDispatch);
            }
            else
            {
                entityDispatch.CreatedDate = DateTime.UtcNow;
            }

            entityDispatch.DispatchStatus = dispatchStatus;
            entityDispatch.PackingType = packingType;
            entityDispatch.PrescriptionStatus = prescriptionStatus;

            if (prescriptionStatus == PrescriptionStatus.Delivery && !string.IsNullOrEmpty(deliveryAddress))
            {
                entityDispatch.DeliveryAddress = deliveryAddress;
            }
            else
            {
                entityDispatch.DeliveryAddress = "";
            }

            if (prescriptionStatus == PrescriptionStatus.OnSite && Id.HasValue)
            {
                entityDispatch.OnSiteId = Id;
                var entityOnsite = db.OnSiteDispenses.FirstOrDefault(e => !e.IsDelete && e.OnSiteId == Id);
                OnSiteDispenseModel onSiteModel = new OnSiteDispenseModel(entityOnsite);
                if (onSiteModel.SelectionDateDispense.HasValue)
                {
                    entityDispatch.NextDispenseDate = onSiteModel.SelectionDateDispense;
                }
            }

            if (prescriptionStatus == PrescriptionStatus.SelfCollection && Id.HasValue)
            {
                entityDispatch.OutletId = Id;
            }

            db.SaveChanges();

            return entityDispatch;
        }

        public static async Task<BoolResult> SetApproveDispatch(Entity.db_HeyDocEntities db, long prescriptionDispatchId)
        {
            var entityPrescriptionDispacth = db.PrescriptionDispatchs.FirstOrDefault(e => e.DispatchId == prescriptionDispatchId);
            if (entityPrescriptionDispacth == null)
            {
                throw new WebApiException(new WebApiError(WebApiErrorCode.NotFound, Forms.ErrorPrescriptionNotFound));
            }
            entityPrescriptionDispacth.ApproveDate = DateTime.UtcNow;
            entityPrescriptionDispacth.DispatchStatus = DispatchStatus.Approved;
            db.SaveChanges();

            Dictionary<string, string> extraParams = new Dictionary<string, string>()
            {
                { "PrescriptionId", entityPrescriptionDispacth.Prescription.PrescriptionId.ToString() }
            };
            await NotificationService.NotifyUser(db, entityPrescriptionDispacth.Prescription.Patient.UserId, PnActionType.Prescription, entityPrescriptionDispacth.Prescription.ChatRoomId.Value,
                    string.Format("Your Prescription approved"), extraParams);

            return new BoolResult(true);
        }

        public static async Task<BoolResult> SetReadyDispatch(Entity.db_HeyDocEntities db, long prescriptionDispatchId)
        {
            var entityPrescriptionDispacth = db.PrescriptionDispatchs.FirstOrDefault(e => e.DispatchId == prescriptionDispatchId);
            if (entityPrescriptionDispacth == null)
            {
                throw new WebApiException(new WebApiError(WebApiErrorCode.NotFound, Forms.ErrorPrescriptionNotFound));
            }
            entityPrescriptionDispacth.ReadyOrDeliveredDate = DateTime.UtcNow;
            entityPrescriptionDispacth.DispatchStatus = DispatchStatus.Ready;
            db.SaveChanges();

            Dictionary<string, string> extraParams = new Dictionary<string, string>()
            {
                { "PrescriptionId", entityPrescriptionDispacth.Prescription.PrescriptionId.ToString() }
            };
            if (entityPrescriptionDispacth.PrescriptionStatus == PrescriptionStatus.Delivery)
            {
                await NotificationService.NotifyUser(db, entityPrescriptionDispacth.Prescription.Patient.UserId, PnActionType.Prescription, entityPrescriptionDispacth.Prescription.ChatRoomId.Value,
                    string.Format("Your Prescription is now Delivered"), extraParams);
            }

            if (entityPrescriptionDispacth.PrescriptionStatus == PrescriptionStatus.OnSite)
            {
                await NotificationService.NotifyUser(db, entityPrescriptionDispacth.Prescription.Patient.UserId, PnActionType.Prescription, entityPrescriptionDispacth.Prescription.ChatRoomId.Value,
                    string.Format("Your Prescription is Ready to pick On Site"), extraParams);
            }

            if (entityPrescriptionDispacth.PrescriptionStatus == PrescriptionStatus.SelfCollection)
            {
                await NotificationService.NotifyUser(db, entityPrescriptionDispacth.Prescription.Patient.UserId, PnActionType.Prescription, entityPrescriptionDispacth.Prescription.ChatRoomId.Value,
                    string.Format("Your Prescription is ready to self Collect"), extraParams);
            }

            return new BoolResult(true);
        }

        public static async Task<int> AddLogPrescription(Entity.db_HeyDocEntities db, long prescriptionId, PrescriptionLogModel model)
        {
            if (string.IsNullOrEmpty(model.LogText))
            {
                throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidAction, Forms.ErrorLogTextNull));
            }

            if (model.LogType == PnActionType.URL && (string.IsNullOrEmpty(model.LogUrl)))
            {
                throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidAction, Forms.ErrorURLNull));
            }

            var entityPrescriptionDispacth = db.PrescriptionDispatchs
                .Where(e => e.PrescriptionId == prescriptionId)
                .OrderByDescending(o => o.CreatedDate).FirstOrDefault();
            if (entityPrescriptionDispacth == null)
            {
                entityPrescriptionDispacth = CreateDispatch(db, prescriptionId, PrescriptionStatus.NoStatus, DispatchStatus.NoStatus);
            }
            entityPrescriptionDispacth.Prescription.LastUpdateDate = DateTime.UtcNow;

            var entityLog = db.PrescriptionLogs.Create();
            entityLog.LogText = model.LogText;
            entityLog.LogUrl = model.LogUrl;
            entityLog.LogType = model.LogType;
            entityLog.DispatchId = entityPrescriptionDispacth.DispatchId;
            entityLog.CreatedDate = DateTime.UtcNow;
            entityLog.IsDelete = false;
            db.PrescriptionLogs.Add(entityLog);

            if (model.LogType == PnActionType.URL)
            {
                await NotificationService.NotifyUser(db, entityPrescriptionDispacth.Prescription.Patient.UserId, model.LogType, model.LogUrl,
                   $"#{prescriptionId} : {model.LogText}");
            }
            else
            {
                Dictionary<string, string> extraParams = new Dictionary<string, string>()
                {
                    { "PrescriptionId", entityPrescriptionDispacth.Prescription.PrescriptionId.ToString() }
                };
                await NotificationService.NotifyUser(db, entityPrescriptionDispacth.Prescription.Patient.UserId, PnActionType.Prescription, entityPrescriptionDispacth.Prescription.ChatRoomId.Value,
                   $"#{prescriptionId} : {model.LogText}", extraParams);
            }

            db.SaveChanges();
            return entityPrescriptionDispacth.DispatchId;
        }

        public static async Task<BoolResult> MarkPrescriptionByStatus(string accessToken, long prescriptionId, DoctorRemarkModel model)
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                using (var transaction = db.Database.BeginTransaction())
                {
                    var now = DateTime.UtcNow;
                    var user = AccountService.GetEntityUserByAccessToken(db, accessToken, false);
                    var doctorRoles = ConstantHelper.DoctorRoles;
                    if (!user.Roles.Any(r => doctorRoles.Contains(r)))
                    {
                        throw new WebApiException(new WebApiError(WebApiErrorCode.Unauthorized, string.Format(Errors.UnauthorizedRole, ConstantHelper.Doc2UsEmailContact)));
                    }

                    var prescription = db.Prescriptions.FirstOrDefault(e => e.PrescriptionId == prescriptionId && e.DoctorId == user.UserId && e.PrescriptionAvailabilityStatus != PrescriptionAvailabilityStatus.Cancelled);

                    if (prescription == null)
                    {
                        throw new WebApiException(new WebApiError(WebApiErrorCode.NotFound, Forms.ErrorPrescriptionNotFound));
                    }

                    if (model.Status != PrescriptionAvailabilityStatus.Approved && model.Status != PrescriptionAvailabilityStatus.Rejected)
                    {
                        throw new WebApiException(new WebApiError(WebApiErrorCode.NotFound, "Please select whether Approved or Rejected only for status."));
                    }
                    if (model.Signature == null || model.Signature.IC != user.IC || !WebSecurity.Login(user.UserName, model.Signature.CertificatePassword))
                    {
                        throw new WebApiException(new WebApiError(WebApiErrorCode.Unauthorized, Errors.InvalidCredentials));
                    }
                    prescription.DoctorRemarks = model.DoctorRemarks;
                    prescription.PrescriptionAvailabilityStatus = model.Status;
                    prescription.ApprovedDate = now;

                    var drugs = db.Drugs.Where(e => e.PrescriptionId == prescriptionId && !e.IsDelete);
                    var drugStatus = (model.Status == PrescriptionAvailabilityStatus.Approved) ? "Prescribed" : "Rejected";

                    foreach (var drug in drugs)
                    {
                        drug.Status = drugStatus;
                    }

                    db.SaveChanges();
                    prescription.FileUrl = _GetPrescriptionWebUrl(prescription.PrescriptionId, db);

                    if (prescription.FrontEndSource == PrescriptionFrontEndSource.CHATBOT)
                    {
                        var chatBotSessionMessage = db.Chats.FirstOrDefault(e => e.ChatBotSessionId == prescription.ChatBotSessionId);
                        if (chatBotSessionMessage != null)
                        {
                            switch (model.Status)
                            {
                                case PrescriptionAvailabilityStatus.Approved:
                                    chatBotSessionMessage.Message = "Your conversation with the Chat Assistant was recorded here. (Prescription approved)";
                                    break;
                                case PrescriptionAvailabilityStatus.Rejected:
                                    chatBotSessionMessage.Message = "Your conversation with the Chat Assistant was recorded here. (Prescription rejected)";
                                    break;
                            }
                        }
                        Dictionary<string, string> extraParams = new Dictionary<string, string>()
                        {
                            { "PrescriptionId", prescription.PrescriptionId.ToString() }
                        };
                        var text = $"Your prescription (ID: {prescriptionId}) has been {model.Status} by {user.FullName}.";
                        if (model.Status == PrescriptionAvailabilityStatus.Rejected)
                        {
                            text = text + $" Reason: {model.DoctorRemarks}";
                        }
                        await NotificationService.NotifyUser(db, prescription.PatientId.Value, PnActionType.Prescription, prescription.ChatRoomId.Value, text, extraParams);
                    }
                    else
                    {
                        string text = "Medication record of " + prescriptionId.ToString() + " has been " + model.Status.ToString() + " by " + user.FullName;

                        if (model.Status == PrescriptionAvailabilityStatus.Approved)
                        {
                            await NotificationService.NotifyUser(db, prescription.PrescribedBy.Value, PnActionType.PrescriptionApproved, prescriptionId, text);
                        }
                        else
                        {
                            await NotificationService.NotifyUser(db, prescription.PrescribedBy.Value, PnActionType.PrescriptionRejected, prescriptionId, text);
                        }
                    }

                    // Send email if prescription was rejected
                    if (model.Status == PrescriptionAvailabilityStatus.Rejected && prescription.FrontEndSource != PrescriptionFrontEndSource.CHATBOT)
                    {
#if DEBUG
                        var recepientList = CloudConfigurationManager.GetSetting("Doc2UsDevEmails").Split(',').ToList();
                        var subject = $"[HOPE Staging] E-Prescription ID: {prescription.PrescriptionId} was rejected";
#else
                        // TODO M UNBLANK: Admin email as recepient
                        var recepientList = new List<string>
                        {
                            ""
                        };
                        var subject = $"[Doc2Us] E-Prescription ID: {prescription.PrescriptionId} was rejected";
#endif
                        var emailBody = string.Format(EmailHelper.ConvertEmailHtmlToString(@"Emails/PrescriptionRejected.html"), prescription.PrescriptionId, prescription.DoctorRemarks, prescription.Patient.UserProfile.FullName, prescription.PrescribedByUser.FullName);
                        EmailHelper.SendViaSendGrid(recepientList, subject, emailBody, string.Empty, null, null, true);
                    }

                    transaction.Commit();
                }
            }
            return new BoolResult(true);
        }

        public static PrescriptionModel PrescriptionRecord(string accessToken, long prescriptionId)
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var user = AccountService.GetEntityUserByAccessToken(db, accessToken, false);
                var doctorRoles = ConstantHelper.DoctorRoles;
                if (!user.Roles.Any(r => doctorRoles.Contains(r)))
                {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.Unauthorized, string.Format(Errors.UnauthorizedRole, ConstantHelper.Doc2UsEmailContact)));
                }

                var prescription = db.Prescriptions.FirstOrDefault(e => e.PrescriptionId == prescriptionId);
                if (prescription == null)
                {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.NotFound, Forms.ErrorPrescriptionNotFound));
                }

                var result = new PrescriptionModel(prescription, true);
                result.PdfUrl = GetBlobSignedPdfUrl(prescription.FileUrl);
                result.ChatRoomId = prescription.ChatRoomId;
                var userAnswers = db.MedicationUserAnswers.Where(e => e.PrescriptionId == prescriptionId).ToList();

                foreach (var drug in result.Drugs)
                {
                    foreach (var question in drug.MedicationQuestions)
                    {
                        var userAnswer = userAnswers.FirstOrDefault(e => e.QuestionID == question.QuestionId);
                        if (userAnswer != null)
                        {
                            question.UserAnswer = new UserAnswerModel(userAnswer);
                        }
                    }
                }
                return result;
            }
        }

        public static BoolResult CancelPrescription(string accessToken, long prescriptionId)
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var now = DateTime.UtcNow;

                var user = AccountService.GetEntityUserByAccessToken(db, accessToken, false);
                var doctorRoles = ConstantHelper.DoctorRoles;
                if (!user.Roles.Any(r => doctorRoles.Contains(r)))
                {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.Unauthorized, string.Format(Errors.UnauthorizedRole, ConstantHelper.Doc2UsEmailContact)));
                }


                var prescription = db.Prescriptions.FirstOrDefault(e => !e.IsDelete && e.PrescriptionId == prescriptionId && e.DoctorId == user.UserId);

                if (prescription == null)
                {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.NotFound, Forms.ErrorPrescriptionNotFound));
                }
                if (prescription.IsDispensed)
                {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.NotFound, "Dispensed prescription cannot be cancelled"));
                }

                prescription.PrescriptionAvailabilityStatus = PrescriptionAvailabilityStatus.Cancelled;
                prescription.ApprovedDate = now;
                db.SaveChanges();

                return new BoolResult(true);
            }
        }

        public static string GetPrescriptionUrlWithAccessSignature(Prescription prescription)
        {
            return $"{ConstantHelper.ServerUrl}/Prescription/prescription?prescriptionId={prescription.PrescriptionId}&p1={prescription.Identifier1}&p2={prescription.Identifier2}&accessSignature={GetTimedAccessToken(prescription.PrescriptionId)}";
        }

        // Returns the prescription URL without an access signature, which means the URL can't be used to view the prescription
        // Only used to fill in Prescription.FileURL when digital signing is disabled (in place of the pdf link), though this should probably be changed as there is not much point in storing these URLs
        private static string _GetPrescriptionWebUrl(long prescriptionId, Entity.db_HeyDocEntities db)
        {
            var prescription = GetPrescriptionById(db, prescriptionId);
            var prescriptionUrl = $"{ConstantHelper.ServerUrl}/Prescription/prescription?prescriptionId={prescription.PrescriptionId}&p1={prescription.Identifier1}&p2={prescription.Identifier2}";
            return prescriptionUrl;
        }

        public static string GetBlobSignedPdfUrl(string prescriptionUrl)
        {
            if (!prescriptionUrl.Contains(".pdf"))
            {
                return prescriptionUrl;
            }

            var signedUrl = prescriptionUrl + SASService.GetBlobSignature(prescriptionBlobContainer, prescriptionUrl, expiryDateinUtc: DateTime.UtcNow.AddMinutes(5));
            return signedUrl;

        }

        internal static Entity.Prescription GetPrescriptionById(Entity.db_HeyDocEntities db, long prescriptionId)
        {
            var prescription = db.Prescriptions.FirstOrDefault(e => !e.IsDelete && e.PrescriptionId == prescriptionId);
            if (prescription == null)
            {
                throw new WebApiException(new WebApiError(WebApiErrorCode.NotFound, Forms.ErrorPrescriptionNotFound));
            }
            return prescription;
        }

        internal static Prescription GetPrescriptionByFullId(db_HeyDocEntities db, long prescriptionId, Guid identifier1, Guid identifier2)
        {
            var prescription = db.Prescriptions.FirstOrDefault(e => !e.IsDelete && e.PrescriptionId == prescriptionId && e.Identifier1 == identifier1 && e.Identifier2 == identifier2);
            if (prescription == null)
            {
                throw new WebApiException(new WebApiError(WebApiErrorCode.NotFound, Forms.ErrorPrescriptionNotFound));
            }
            return prescription;
        }

        /// <summary>
        /// Get a list of prescriptions from a particular source (e.g. pharmacy outlets, doctors)
        /// </summary>
        /// <param name="source">Prescription source for which to get records for</param>
        /// <param name="skip">Number of records to skip</param>
        /// <param name="take">Max number of records to return</param>
        /// <param name="sortParams">List of fields to sort by and the direction in which to sort them, in order of priority (false for ascending, true for descending)</param>
        /// <param name="prescriptionId">Prescription ID to search for</param>
        /// <param name="searchName">Patient or doctor name to search for</param>
        /// <param name="recordsFiltered">Total number records after ID and name filtering</param>
        /// <param name="recordsTotal">Total number of records from the requested source before filtering</param>
        /// <returns>List of prescriptions from requested source</returns>
        public static List<PrescriptionModel> GetPrescriptionsBySource(int prescriptionSourceId, long prescriptionId, string searchName, int skip, int take, List<(PrescriptionSortField, bool)> sortParams, out int recordsFiltered, out int recordsTotal)
        {
            using (var db = new db_HeyDocEntities())
            {
                var prescriptions = db.Prescriptions.Where(p => !p.IsDelete);

                if (prescriptionSourceId != -1)
                {
                    if (ConstantHelper.DoctorPrescriptionSourceIds.Contains(prescriptionSourceId))
                    {
                        prescriptions = prescriptions.Where(p => p.Doctor.UserProfile.PrescriptionSourceId == prescriptionSourceId);
                    }
                    else
                    {
                        prescriptions = prescriptions.Where(p => p.PrescribedByUser.PrescriptionSourceId == prescriptionSourceId);
                    }
                }

                recordsTotal = prescriptions.Select(p => p.PrescriptionId).Count();

                var filteringApplied = false;
                if (prescriptionId > 0)
                {
                    prescriptions = prescriptions.Where(p => p.PrescriptionId == prescriptionId);
                    filteringApplied = true;
                }

                if (!string.IsNullOrEmpty(searchName))
                {
                    var searchText = FullTextSearchModelUtil.Contains($"\"{searchName}\"", true);
                    prescriptions = from p in prescriptions
                                    join u in db.UserProfiles on p.DoctorId equals u.UserId into doctorGroup
                                    from doctorUser in doctorGroup.DefaultIfEmpty()
                                    join u in db.UserProfiles on p.PatientId equals u.UserId into patientGroup
                                    from patientUser in patientGroup.DefaultIfEmpty()
                                    where doctorUser.FullName.Contains(searchText) || patientUser.FullName.Contains(searchText)
                                    select p;
                    filteringApplied = true;
                }

                recordsFiltered = filteringApplied ? prescriptions.Select(p => p.PrescriptionId).Count() : recordsTotal;

                if (sortParams.Count > 0)
                {
                    var firstOrdering = true;
                    foreach (var (sortField, sortOrder) in sortParams)
                    {
                        switch (sortField)
                        {
                            case PrescriptionSortField.CreateDate:
                                prescriptions = prescriptions.DynamicOrderBy(p => p.CreateDate, sortOrder, firstOrdering);
                                break;
                            case PrescriptionSortField.PatientName:
                                prescriptions = prescriptions.DynamicOrderBy(p => p.Patient.UserProfile.FullName, sortOrder, firstOrdering);
                                break;
                            case PrescriptionSortField.DoctorName:
                                prescriptions = prescriptions.DynamicOrderBy(p => p.Doctor.UserProfile.FullName, sortOrder, firstOrdering);
                                break;
                            case PrescriptionSortField.PrescriptionId:
                                prescriptions = prescriptions.DynamicOrderBy(p => p.PrescriptionId, sortOrder, firstOrdering);
                                break;
                            default:
                                throw new Exception("Invalid sort field in sortParams");
                        }
                        firstOrdering = false;
                    }
                }
                else
                {
                    prescriptions = prescriptions.OrderByDescending(p => p.CreateDate);
                }

                prescriptions = prescriptions.Skip(skip).Take(take);
                // Query has to be resolved to a list before constructing the models because Entity Framework doesn't support this kind of constructor
                var result = prescriptions.ToList().Select(p => new PrescriptionModel(p, false)).ToList();
                return result;
            }
        }

        public static string GetTimedAccessToken(long prescriptionId)
        {
            // Buffer for prescriptionId and DateTime
            var tokenData = new byte[sizeof(long) + sizeof(long)];
            Buffer.BlockCopy(BitConverter.GetBytes(prescriptionId), 0, tokenData, 0, sizeof(long));
            Buffer.BlockCopy(BitConverter.GetBytes(DateTime.UtcNow.ToBinary()), 0, tokenData, sizeof(long), sizeof(long));
            return HttpServerUtility.UrlTokenEncode(EncryptionService.EncryptAES(ConstantHelper.PrescriptionAccessEncryptionKey, tokenData));
        }

        public static bool VerifyTimedAccessToken(long prescriptionId, string accessToken)
        {
            var decryptedToken = EncryptionService.DecryptAES(ConstantHelper.PrescriptionAccessEncryptionKey, HttpServerUtility.UrlTokenDecode(accessToken));
            var tokenPrescriptionId = BitConverter.ToInt64(decryptedToken, 0);
            var tokenTime = DateTime.FromBinary(BitConverter.ToInt64(decryptedToken, sizeof(long)));
            return tokenPrescriptionId == prescriptionId && tokenTime.AddMinutes(15) >= DateTime.UtcNow;
        }

        public static bool AssignPrescriptionToSelf(string accessToken, long prescriptionId)
        {
            using (var db = new db_HeyDocEntities())
            {
                var entityDoctor = AccountService.GetEntityUserByAccessToken(db, accessToken, false);
                if (!entityDoctor.Roles.Any(r => ConstantHelper.DoctorRoles.Contains(r)))
                {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.Unauthorized));
                }
                if (entityDoctor.Doctor.GroupId != ConstantHelper.PrimaryDoctorGroupId || !(entityDoctor.Doctor.CanApproveEPS.HasValue && entityDoctor.Doctor.CanApproveEPS.Value))
                {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.Unauthorized));
                }

                var entityPrescription = db.Prescriptions.Find(prescriptionId);
                if (entityPrescription == null)
                {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.NotFound));
                }

                entityPrescription.DoctorId = entityDoctor.UserId;
                entityPrescription.AssignedDate = DateTime.Now;
                db.SaveChanges();

                return true;
            }
        }

        public static List<PrescriptionModel> GetUnassignedPrescriptionList(string accessToken, int skip, int take)
        {
            using (var db = new db_HeyDocEntities())
            {
                var entityDoctor = AccountService.GetEntityUserByAccessToken(db, accessToken, false);
                if (!entityDoctor.Roles.Any(r => ConstantHelper.DoctorRoles.Contains(r)))
                {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.Unauthorized));
                }

                if (entityDoctor.Doctor.GroupId != ConstantHelper.PrimaryDoctorGroupId || !(entityDoctor.Doctor.CanApproveEPS.HasValue && entityDoctor.Doctor.CanApproveEPS.Value))
                {
                    return new List<PrescriptionModel>();
                }

                return db.Prescriptions
                    .Where(p => !p.IsDelete && p.DoctorId == null && p.PatientId != null && !p.Patient.UserProfile.IsDelete && !p.Patient.UserProfile.IsBan)
                    .OrderBy(p => p.CreateDate)
                    .Skip(skip)
                    .Take(take)
                    .ToList()
                    .Select(p => new PrescriptionModel(p, false))
                    .ToList();
            }
        }

        private static void _ValidatePrescriptionCheckout(Prescription entityPrescription)
        {
            if (entityPrescription == null)
            {
                throw new WebApiException(new WebApiError(WebApiErrorCode.NotFound, Forms.ErrorPrescriptionNotFound));
            }
            if (entityPrescription.IsDispensed)
            {
                throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidAction, Medications.ErrorPrescriptionDispensed));
            }
            if (!(entityPrescription.PrescriptionAvailabilityStatus == PrescriptionAvailabilityStatus.NotApplicable || entityPrescription.PrescriptionAvailabilityStatus == PrescriptionAvailabilityStatus.Approved))
            {
                throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidAction, Medications.ErrorPrescriptionNotApproved));
            }
            if (entityPrescription.FrontEndSource == PrescriptionFrontEndSource.EPS)
            {
                throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidAction, Medications.ErrorEPSPrescriptionCheckout));
            }
        }

        public static IEnumerable<object> GeneratePrescriptionStats(db_HeyDocEntities db, int prescriptionSourceId, DateTimeOffset startDate, DateTimeOffset endDate)
        {
            if (startDate > endDate)
            {
                throw new Exception("Invalid date range. Start Date must be earlier or same day as End Date");
            }
            else if (startDate.AddDays(366) <= endDate)
            {
                throw new Exception("Invalid date range. Maximum allowed range is 366 days");
            }

            // Move endDate to next day so that all prescriptions created on the requested endDate are included
            var endDateNextDayUtc = endDate.AddDays(1).UtcDateTime;
            var startDateUtc = startDate.UtcDateTime;

            var prescriptions = from p in db.Prescriptions
                                where !p.IsDelete
                                where p.CreateDate >= startDateUtc && p.CreateDate < endDateNextDayUtc
                                select p;
            var users = db.UserProfiles.Where(u => !u.IsDelete);

            if (ConstantHelper.DoctorPrescriptionSourceIds.Contains(prescriptionSourceId))
            {
                var prescriptionStats = from u in users
                                        where u.PrescriptionSourceId == prescriptionSourceId
                                        let prescribedByUser = prescriptions.Where(p => p.DoctorId == u.UserId && !p.PrescribedBy.HasValue)
                                        let prescriptionCount = prescribedByUser.Select(p => p.PrescriptionId).Count()
                                        orderby prescriptionCount descending
                                        select new DoctorPrescriptionStatsModel
                                        {
                                            FullName = u.FullName,
                                            Email = u.UserName,
                                            PrescriptionCount = prescriptionCount,
                                            DispensedCount = prescribedByUser
                                                                .Where(p => p.IsDispensed)
                                                                .Select(p => p.PrescriptionId)
                                                                .Count()
                                        };
                return prescriptionStats;
            }
            else
            {
                var prescriptionStats = from u in users
                                        where u.PrescriptionSourceId == prescriptionSourceId
                                        let prescribedByUser = prescriptions.Where(p => p.PrescribedBy == u.UserId)
                                        let dispensedPharmacistFromUser = prescriptions.Where(p => p.DispensedOutlet == u.UserId
                                                && (p.FrontEndSource == PrescriptionFrontEndSource.EPS || (p.FrontEndSource == PrescriptionFrontEndSource.UNKNOWN && !p.ChatRoomId.HasValue)))
                                        let dispensedDoctorFromUser = prescriptions.Where(p => p.DispensedOutlet == u.UserId
                                                && (p.FrontEndSource == PrescriptionFrontEndSource.CHAT || p.FrontEndSource == PrescriptionFrontEndSource.CHATBOT || (p.FrontEndSource == PrescriptionFrontEndSource.UNKNOWN && p.ChatRoomId.HasValue)))
                                        let prescriptionCount = prescribedByUser.Select(p => p.PrescriptionId).Count()
                                        orderby prescriptionCount descending
                                        select new EpsPrescriptionStatsModel
                                        {
                                            FullName = u.FullName,
                                            Email = u.UserName,
                                            PrescriptionCount = prescriptionCount,
                                            ApprovedCount = prescribedByUser
                                                            .Where(p => p.PrescriptionAvailabilityStatus == PrescriptionAvailabilityStatus.Approved)
                                                            .Select(p => p.PrescriptionId)
                                                            .Count(),
                                            RejectedCount = prescribedByUser
                                                            .Where(p => p.PrescriptionAvailabilityStatus == PrescriptionAvailabilityStatus.Rejected)
                                                            .Select(p => p.PrescriptionId)
                                                            .Count(),
                                            DispensedPharmacistCount = dispensedPharmacistFromUser.Select(p => p.PrescriptionId).Count(),
                                            DispensedDoctorCount = dispensedDoctorFromUser.Select(p => p.PrescriptionId).Count()
                                        };
                return prescriptionStats;
            }
        }

        public static (bool canBePrescribed, List<string> missingFields) CheckPatientCanBePrescribed(Entity.Patient entityPatient)
        {
            var missingFieldsToDo = new List<string>();
            if (string.IsNullOrEmpty(entityPatient.Allergy))
            {
                missingFieldsToDo.Add("'Edit Biodata' of patient and add an 'Allergy' description.");
            }
            if (string.IsNullOrEmpty(entityPatient.UserProfile.IC))
            {
                missingFieldsToDo.Add("Inform patient to edit profile and fill in IC.");
            }
            if (string.IsNullOrEmpty(entityPatient.UserProfile.Address))
            {
                missingFieldsToDo.Add("Inform patient to edit profile and fill in address.");
            }
            if (!entityPatient.UserProfile.Gender.HasValue || !Enum.IsDefined(typeof(Gender), entityPatient.UserProfile.Gender.Value))
            {
                missingFieldsToDo.Add("Inform patient to edit profile and fill in gender.");
            }

            if (missingFieldsToDo.Count() > 0)
            {
                return (false, missingFieldsToDo);
            }
            return (true, null);
        }

        public static (bool isInfoComplete, List<string> missingFields) CheckChatroomMissingInfoForPrescription(string accessToken, int chatRoomId)
        {
            using (var db = new db_HeyDocEntities())
            {
                var entityUser = AccountService.GetEntityUserByAccessToken(db, accessToken, false);
                var doctorRoles = ConstantHelper.DoctorRoles;
                if (!entityUser.Roles.Any(r => doctorRoles.Contains(r)))
                {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.Unauthorized, string.Format(Errors.UnauthorizedRole, ConstantHelper.Doc2UsEmailContact)));
                }

                var entityChatRoom = db.ChatRooms.FirstOrDefault(e => e.ChatRoomId == chatRoomId && e.DoctorId == entityUser.UserId);

                if (entityChatRoom == null)
                {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.NotFound, "Chat not found"));
                }

                return CheckPatientCanBePrescribed(entityChatRoom.Patient);
            }
        }
    }
}
