using HeyDoc.Web.Entity;
using HeyDoc.Web.WebApi;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace HeyDoc.Web.Models
{
    public class CorporateModel: IValidatableObject
    {
        public int CorporateId { get; set; }
        public string BranchName { get; set; }
        public string BranchAddress { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? TPAId { get; set; }
        public string TPAName { get; set; }
        public IEnumerable<int> TPASupplyingPharmacyIds { get; set; }
        public IEnumerable<string> TPASupplyingPharmacyNames { get; set; }
        public bool IsHiddenPublicSelection { get; set; }
        public string ThirdPartyCorporateId { get; set; }
        public bool PolicyHasSameDayDelivery { get; set; }
        public byte PolicySupplyDurationInMonths { get; set; }
        public string PolicyRemarks { get; set; }
        public string PolicyEMC { get; set; }
        public bool IsBan { get; set; }
        public IEnumerable<int> SupplyingPharmacyIds { get; set; }
        public IEnumerable<string> SupplyingPharmacyNames { get; set; }
        public string SecretKey { get; set; }
        public int MaxSecretKey { get; set; }
        public CorporateModel()
        {

        }

        public CorporateModel(Entity.Corporate entityCorporate, bool shouldIncludePolicy = false)
        {
            CorporateId = entityCorporate.CorporateId;
            BranchName = entityCorporate.BranchName;
            BranchAddress = entityCorporate.BranchAddress;
            CreatedDate = entityCorporate.CreatedDate;
            TPAId = entityCorporate.TPAId;
            if (entityCorporate.ThirdPartyAdministrator != null)
            {
                TPAName = entityCorporate.ThirdPartyAdministrator.Name;
                TPASupplyingPharmacyIds = entityCorporate.ThirdPartyAdministrator.SupplyingPharmacies.Select(p => p.PrescriptionSourceId);
                TPASupplyingPharmacyNames = entityCorporate.ThirdPartyAdministrator.SupplyingPharmacies.Select(p => p.PrescriptionSourceName);
            }
            IsHiddenPublicSelection = entityCorporate.IsHiddenPublicSelection;
            IsBan = entityCorporate.IsBan;
            SupplyingPharmacyIds = entityCorporate.SupplyingPharmacies.Select(e => e.PrescriptionSourceId);
            SupplyingPharmacyNames = entityCorporate.SupplyingPharmacies.Select(e => e.PrescriptionSourceName);
            if (shouldIncludePolicy)
            {
                PolicyHasSameDayDelivery = entityCorporate.PolicyHasSameDayDelivery;
                PolicySupplyDurationInMonths = entityCorporate.PolicySupplyDurationInMonths ?? 1;
                PolicyRemarks = entityCorporate.PolicyRemarks;
                PolicyEMC = entityCorporate.PolicyEMC;
            }
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var model = (CorporateModel)validationContext.ObjectInstance;
            if (string.IsNullOrEmpty(model.BranchName))
            {
                yield return new ValidationResult("Name cannot be empty", new[] { nameof(BranchName) });
            }

            if (string.IsNullOrEmpty(model.BranchAddress))
            {
                yield return new ValidationResult("Address cannot be empty", new[] { nameof(BranchAddress) });
            }

            using (var db = new Entity.db_HeyDocEntities())
            {
                if (model.TPAId.HasValue && model.TPAId.Value != 0 && !db.ThirdPartyAdministrators.Any(e => e.TPAId == model.TPAId && !e.IsDelete))
                {
                    yield return new ValidationResult("Selected TPA does not exist", new[] { nameof(TPAId) });
                }
            }
        }
    }

    public class TPARegisterCorporateModel
    {
        public string BranchName { get; set; }
        public string BranchAddress { get; set; }
        public string TPACorporateId { get; set; }
    }
}
