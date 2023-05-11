using HeyDoc.Web.Resources;
using HeyDoc.Web.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HeyDoc.Web.Models
{
    public class DrugModel
    {
        public long DrugId { get; set; }
        public long MedicationId { get; set; }
        public string MedicationName { get; set; }
        public string Dosage { get; set; }
        public string Route { get; set; }
        public string Frequency { get; set; }
        public string Amount { get; set; }
        public string Status { get; set; }
        public string Remark { get; set; }

        public int DurationInMonth { get; set; }
        public int TotalQuantity { get; set; }
        public string DispensedAmount { get; set; }

        public List<QuestionModel> MedicationQuestions { get; set; }
        public MedicationModel Medication { get; set; }

        public DrugModel()
        {

        }

        public DrugModel(Entity.Drug entityDrug)
        {
            DrugId = entityDrug.DrugId;
            if (entityDrug.MedicationId.HasValue)
            {
                MedicationId = entityDrug.MedicationId.Value;
                MedicationName = entityDrug.Medication.MedicationName;
            }
            Dosage = entityDrug.Dosage;
            Route = entityDrug.Route;
            Frequency = entityDrug.Frequency;
            Amount = entityDrug.Amount;
            Status = entityDrug.Status;
            Remark = entityDrug.Remark;
            DurationInMonth = entityDrug.DurationInMonth;
            TotalQuantity = entityDrug.TotalQuantity;
            if (entityDrug.DispensedAmount.HasValue)
            {
                DispensedAmount = $"{entityDrug.DispensedAmount} {entityDrug.DispensedAmountUnit}";
            }
            if (entityDrug.MedicationId.HasValue)
            {
                MedicationQuestions = new List<QuestionModel>();
                foreach (var question in entityDrug.Medication.MedicationQuestions.Where(e => !e.IsDelete))
                {
                    MedicationQuestions.Add(new QuestionModel(question));
                }
            }
        }

        public void Validate()
        {
            if (string.IsNullOrEmpty(Dosage))
            {
                throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidArguments, Forms.ErrorDrugDosageNull));
            }
            if (string.IsNullOrEmpty(Route))
            {
                throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidArguments, Forms.ErrorDrugRouteNull));
            }
            if (string.IsNullOrEmpty(Frequency))
            {
                throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidArguments, Forms.ErrorDrugFreqNull));
            }
            if (string.IsNullOrEmpty(Amount))
            {
                throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidArguments, Forms.ErrorDrugAmountNull));
            }
            if (string.IsNullOrEmpty(Status))
            {
                throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidArguments, Forms.ErrorDrugStatusNull));
            }
        }
    }
}