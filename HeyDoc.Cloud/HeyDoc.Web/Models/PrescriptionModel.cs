using HeyDoc.Web.Helpers;
using HeyDoc.Web.Resources;
using HeyDoc.Web.Services;
using HeyDoc.Web.WebApi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Security;

namespace HeyDoc.Web.Models
{
    public class PrescriptionModel
    {
        public long PrescriptionId { get; set; }
        public DoctorModel Doctor { get; set; }
        public PatientModel Patient { get; set; }
        public string Allergy { get; set; }
        public string MedicalSummary { get; set; }
        public DateTime CreateDate { get; set; }
        public string FileUrl { get; set; }

        public bool IsDispensed { get; set; }
        public DateTime? DispensedDate { get; set; }

        public string Height { get; set; }
        public string Weight { get; set; }

        public List<DrugModel> Drugs { get; set; }

        public PrescriptionStatus? PrescriptionStatus { get; set; }
        public PrescriptionDispatchModel Dispatch { get; set; }

        public DateTime? NextDispenseDateTime { get; set; }
        public DateTime? LatestUpdate { get; set; }

        public bool IsPrescribedByPharmacist { get; set; }
        public PrescriptionAvailabilityStatus PrescriptionAvailabilityStatus { get; set; }
        public DateTime? ApprovedDate { get; set; }

        public string DoctorRemarks { get; set; }
        public string Remarks { get; set; }
        public string ReviewedBy { get; set; }
        public string RegisterNo { get; set; }
        public DigitalSignatureModel SignatureCredential { get; set; }

        public Guid? Identifier1 { get; set; }
        public Guid? Identifier2 { get; set; }

        public UserSimpleModel PrescribedBy { get; set; }
        public string PrescribedByOutlet { get; set; }

        public DoctorModel DispensedUser { get; set; }

        public int DispensedOutletId { get; set; }
        public PharmacyOutletModel DispensedOutlet { get; set; }

        public string PdfUrl { get; set; }
        public bool IsDigitallySigned { get; set; }

        public DateTime? ExpiryDate { get; set; }

        // For prescriptions of corporate users
        public bool CheckoutProcessed { get; set; }

        public int? ChatBotSessionId { get; set; }
        public int? ChatRoomId { get; set; }
        public PrescriptionMedicationType? MedicationType { get; set; }
        public decimal? ConsultationFees { get; set; }
        public decimal? MedicationFees { get; set; }
        public decimal? DeliveryFees { get; set; }
        public IList<IcdEntryModel> IcdEntries { get; set; }
        public string ConsignmentNumber { get; set; }
        public GdexShipmentStatus? LatestShipmentStatus { get; set; }
        public ProcessingStatus? ProcessingStatus { get; set; }

        public PrescriptionModel()
        {

        }

        public PrescriptionModel(Entity.Prescription entityPrescription, bool isDetailed)
        {
            PrescriptionId = entityPrescription.PrescriptionId;
            //Allergy = entityPrescription.Patient.Allergy;
            MedicalSummary = entityPrescription.MedicalSummary;
            CreateDate = entityPrescription.CreateDate;
            if (entityPrescription.DoctorId.HasValue)
            {
                Doctor = new DoctorModel(entityPrescription.Doctor);
            }
            if (entityPrescription.PatientId.HasValue)
            {
                Patient = new PatientModel(entityPrescription.Patient);
            }
            ExpiryDate = entityPrescription.ExpiryDate;
            IsDispensed = entityPrescription.IsDispensed;
            DispensedDate = entityPrescription.DispensedDate;
            if (isDetailed)
            {
                Drugs = new List<DrugModel>();
                foreach (var entityDrug in entityPrescription.Drugs.Where(e => !e.IsDelete))
                {
                    Drugs.Add(new DrugModel(entityDrug));
                }
                if (entityPrescription.DispensedByUser != null)
                {
                    if (entityPrescription.DispensedByUser.Doctor != null)
                    {
                        DispensedUser = new DoctorModel(entityPrescription.DispensedByUser.Doctor);
                    }
                    else
                    {
                        DispensedUser = new DoctorModel(entityPrescription.DispensedByUser);
                    }
                }
            }
            DoctorRemarks = entityPrescription.DoctorRemarks;
            Remarks = entityPrescription.Remarks;
            ReviewedBy = entityPrescription.ReviewedBy;
            RegisterNo = entityPrescription.RegisterNumber;

            IsPrescribedByPharmacist = entityPrescription.DoctorId.HasValue && entityPrescription.Doctor.UserProfile.Title == "Ph";

            if (entityPrescription.PrescribedByUser != null)
            {
                PrescribedBy = new UserSimpleModel(entityPrescription.PrescribedByUser);
                PrescribedByOutlet = PrescribedBy.PrescriptionSource.PrescriptionSourceName;
            }

            //var entityWeight=entityPrescription.Patient.WeightHistories.OrderByDescending(e=>e.CreateDate).FirstOrDefault();
            //var entityHeight = entityPrescription.Patient.HeightHistories.OrderByDescending(e => e.CreateDate).FirstOrDefault();
            //Height = entityHeight != null ? entityHeight.Height.ToString()+"kg" : "";
            //Weight = entityWeight != null ? entityWeight.Weight.ToString()+"cm" : "";
            //FileUrl = entityPrescription.FileUrl;
            Allergy = entityPrescription.Allergy;
            Weight = entityPrescription.Weight;
            Height = entityPrescription.Height;
            IsDigitallySigned = false;
            if (Path.GetExtension(entityPrescription.FileUrl) == ".pdf")
            {
                IsDigitallySigned = true;
                PdfUrl = entityPrescription.FileUrl;
            }
            FileUrl = PrescriptionService.GetPrescriptionUrlWithAccessSignature(entityPrescription);

            PrescriptionStatus = Web.PrescriptionStatus.NoStatus;
            if (entityPrescription.PrescriptionDispatchs.Any())
            {
                Dispatch = new PrescriptionDispatchModel(entityPrescription.PrescriptionDispatchs.OrderByDescending(e => e.CreatedDate).FirstOrDefault());
                PrescriptionStatus = Dispatch.PrescriptionStatus;
                LatestUpdate = entityPrescription.LastUpdateDate;
            }
            NextDispenseDateTime = entityPrescription.NextDispenseDateTime;
            PrescriptionAvailabilityStatus = entityPrescription.PrescriptionAvailabilityStatus;
            ApprovedDate = entityPrescription.ApprovedDate;

            DispensedOutlet = new PharmacyOutletModel();
            if (entityPrescription.DispensedOutlet.HasValue)
            {
                DispensedOutletId = entityPrescription.DispensedFromOutlet.UserId;
                DispensedOutlet = new PharmacyOutletModel()
                {
                    UserId = entityPrescription.DispensedFromOutlet.UserId,
                    OutletName = entityPrescription.DispensedFromOutlet.FullName,
                    OutletAddress = entityPrescription.DispensedFromOutlet.Address,
                    PhoneNumber = entityPrescription.DispensedFromOutlet.PhoneNumber
                };
            }
            CheckoutProcessed = entityPrescription.CheckoutProcessed;
            ChatBotSessionId = entityPrescription.ChatBotSessionId;
            MedicationType = entityPrescription.MedicationType;

            var payment = entityPrescription.PaymentRequest;
            if (payment != null)
            {
                ConsultationFees = payment.Amount;
                MedicationFees = payment.MedicationFeesAmount ?? 0;
                DeliveryFees = payment.DeliveryFeesAmount ?? 0;
            }

            if (entityPrescription.Icds.Count > 0)
            {
                var icds = new List<IcdEntryModel>();
                foreach (var icd in entityPrescription.Icds)
                {
                    icds.Add(new IcdEntryModel
                    {
                        IcdEntityId = icd.IcdEntityId,
                        IcdCode = icd.IcdCode,
                        IcdCodeDescription = icd.IcdCodeDesc,
                        ReleaseId = icd.IcdReleaseVer,
                        LinearizationName = icd.IcdLinearizationCode
                    });
                }
                IcdEntries = icds;
            }

            ConsignmentNumber = entityPrescription.ConsignmentNumber;
            LatestShipmentStatus = entityPrescription.LatestShipmentStatus;
            ProcessingStatus = entityPrescription.ProcessingStatus;
        }

        public void Validate()
        {
            if (Drugs == null || Drugs.Count == 0)
            {
                throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidArguments, Medications.ErrorDrugsEmpty));
            }
            foreach (var drug in Drugs)
            {
                drug.Validate();
            }
            if (SignatureCredential == null)
            {
                throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidArguments, "Please fill in the certificate password and OTP for digital signature"));
            }
            if (MedicalSummary != null && !MedicalSummary.IsWithinNvarcharLimit(4000))
            {
                throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidArguments, "Medical summary should not more than 4000 characters"));
            }
            if (DoctorRemarks != null && !DoctorRemarks.IsWithinNvarcharLimit(500))
            {
                throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidArguments, "Doctor Remark should not be more than 500 characters"));
            }
            if (Remarks != null && !Remarks.IsWithinNvarcharLimit(500))
            {
                throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidArguments, "Remark should not more than 500 characters"));
            }
        }
    }

    public class DigitalSignatureModel
    {
        public string IC { get; set; }
        public string CertificatePassword { get; set; }
        public string OTP { get; set; }
    }

    public class DispensePrescriptionModel : DigitalSignatureModel
    {
        public IDictionary<int, string> DrugDispensedAmounts;
    }

    public class PrescriptionFullIdModel
    {
        public long PrescriptionId { get; set; }
        public Guid Identifier1 { get; set; }
        public Guid Identifier2 { get; set; }
    }
}
