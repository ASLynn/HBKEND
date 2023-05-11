using HeyDoc.Web.Resources;
using HeyDoc.Web.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HeyDoc.Web.Models
{
    public class DoctorBankModel
    {
        public int DoctorId { get; set; }
        public string AccountNumber { get; set; }
        public BankModel Bank { get; set; }
        public string AccountHolderName { get; set; }

        public DoctorBankModel()
        {

        }

        public DoctorBankModel(Entity.Doctor entityDoctor)
        {
            DoctorId = entityDoctor.UserId;
            AccountNumber = entityDoctor.AccountNumber;
            AccountHolderName = entityDoctor.AccountHolderName;
            if (entityDoctor.Bank != null)
            {
                Bank = new BankModel(entityDoctor.Bank);
            }
        }

        public void validate()
        {
            if (string.IsNullOrEmpty(AccountNumber))
            {
                throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidArguments, Forms.ErrorAccountNumberNull));
            }
            if (string.IsNullOrEmpty(AccountHolderName))
            {
                throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidArguments, Forms.ErrorAccountHolderNameNull));
            }
            if (Bank == null)
            {
                throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidArguments, Forms.ErrorBankDetailsNull));
            }
        }
    }
}