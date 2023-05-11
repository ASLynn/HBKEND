using HeyDoc.Web.Resources;
using HeyDoc.Web.WebApi;
using System;
using System.Collections.Generic;

namespace HeyDoc.Web.Models
{
    public class MedicationModel
    {
        public long MedicationId { get; set; }
        public string MedicationName { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsDelete { get; set; }
        public string ImageUrl { get; set; }
        public string Precaution { get; set; }
        public decimal Price { get; set; }
        public string XilnexId { get; set; }
        public string MALNo { get; set; }
        public string ItemCode { get; set; }
        public bool IsLTM { get; set; }
        public MedicationGroupModel Group { get; set; }
        public int? QuestionCount { get; set; }
        public int QuantityPerUnit { get; set; }

        public List<QuestionModel> MedicationQuestions { get; set; }
        public List<int> ForMedicalConditionsWithIds { get; set; }

        public MedicationModel() { }

        public MedicationModel(Entity.Medication entityMedication, int? questionCount = null)
        {
            MedicationId = entityMedication.MedicationId;
            MedicationName = entityMedication.MedicationName;
            CreatedDate = entityMedication.CreatedDate;
            IsDelete = entityMedication.IsDelete;
            ImageUrl = entityMedication.ImageUrl;
            Precaution = entityMedication.Precaution;
            Price = entityMedication.Price;
            XilnexId = entityMedication.XilnexId;
            QuestionCount = questionCount;
            MALNo = entityMedication.MALNo;
            IsLTM = entityMedication.IsLTM;
            ItemCode = entityMedication.ItemCode;
            QuantityPerUnit = entityMedication.QuantityPerUnit;
            if (entityMedication.MedicationGroup != null)
            {
                Group = new MedicationGroupModel(entityMedication.MedicationGroup);
            }
            ForMedicalConditionsWithIds = new List<int>();
            foreach (var entityConditionMedication in entityMedication.MedicalConditionMedications)
            {
                ForMedicalConditionsWithIds.Add(entityConditionMedication.MedicalConditionId);
            }
        }

        public void Validate()
        {
            if (string.IsNullOrEmpty(MedicationName))
            {
                throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidArguments, Medications.ErrorMedNameNull));
            }
            if (MedicationName.Length > 399)
            {
                throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidArguments, "Medication name length should be less than 400 characters."));
            }
            if (!string.IsNullOrEmpty(Precaution) && Precaution.Length > 999)
            {
                throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidArguments, "Precaution length should be less than 1000 characters."));
            }
            if (!string.IsNullOrEmpty(XilnexId) && XilnexId.Length > 999)
            {
                throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidArguments, Medications.ErrorXilnexIDTooLong));
            }

        }
    }

    public class MedicationGroupModel
    {
        public long GroupId { get; set; }
        public string GroupName { get; set; }
        public DateTime CreateDate { get; set; }
        public int? MedicationCount { get; set; }

        public MedicationGroupModel()
        {

        }
        public MedicationGroupModel(Entity.MedicationGroup group, int? medicationCount = null)
        {
            GroupId = group.GroupId;
            GroupName = group.GroupName;
            CreateDate = group.CreateDate;
            MedicationCount = medicationCount;
        }
    }
}