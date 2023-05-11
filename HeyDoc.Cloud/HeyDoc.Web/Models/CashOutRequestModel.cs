using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HeyDoc.Web.Models
{
    public class CashOutRequestModel
    {
        public long ChasOutRequestId { get; set; }
        public int UserId { get; set; }
        public CashOutRequestStatus CashOutRequestStatus { get; set; }
        public DateTime RequestDate { get; set; }
        public DateTime? CashOutDate { get; set; }
        public Decimal? Amount { get; set; }
        public DoctorModel Doctor { get; set; }
        public DoctorBankModel Account { get; set; }
        public string Remark { get; set; }
        public string ReceiptUrl { get; set; }

        public CashOutRequestModel()
        {

        }

        public CashOutRequestModel(Entity.CashOutRequest entityCashOutRequest)
        {
            ChasOutRequestId = entityCashOutRequest.CashOutRequestId;
            UserId = entityCashOutRequest.UserId;
            CashOutRequestStatus = entityCashOutRequest.CashOutRequestStatus;
            RequestDate = entityCashOutRequest.CreateDate;
            CashOutDate = entityCashOutRequest.CashOutDate;
            Amount = entityCashOutRequest.Amount;
            Doctor = new DoctorModel(entityCashOutRequest.UserProfile, entityCashOutRequest.UserProfile.Doctor);
            Account = new DoctorBankModel(entityCashOutRequest.UserProfile.Doctor);
            Remark = entityCashOutRequest.AdminRemark;
            ReceiptUrl = entityCashOutRequest.ReceiptUrl;
        }
    }
}