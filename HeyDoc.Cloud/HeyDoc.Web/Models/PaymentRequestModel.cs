using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HeyDoc.Web.Models
{
    public class PaymentRequestModel
    {
        public long PaymentRequestId { get; set; }
        public int ChatRoomId { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public string BrainTreeTransactionId { get; set; }
        public string BrainTreeTransactionType { get; set; }
        public string BrainTreeTransactionStatus { get; set; }
        public decimal Amount { get; set; }
        public DateTime CreateDate { get; set; }
        public decimal PlatformAmount { get; set; }
        public decimal HcpAmount { get; set; }
        public decimal CutPercent { get; set; }
        public decimal ActualAmount { get; set; }
        public decimal ConsultationAmount { get; set; }
        public decimal MedicationAmount { get; set; }
        public decimal DeliveryAmount { get; set; }
        public bool IsPatientCorporate { get; set; }
        public CorporateUserType? PatientUserCorporateType { get; set; }

        public ChatRoomModel ChatRoom { get; set; }
        public PromoCodeModel PromoCode { get; set; }

        public PaymentRequestModel()
        {

        }        
        public PaymentRequestModel(Entity.PaymentRequest entityPayment,bool isDetailed)
        {
            PaymentRequestId = entityPayment.PaymentRequestId;
            ChatRoomId = entityPayment.ChatRoomId;
            PaymentStatus = entityPayment.PaymentStatus;
            BrainTreeTransactionId = entityPayment.BrainTreeTransactionId;
            BrainTreeTransactionStatus = entityPayment.BrainTreeTransactionStatus;
            BrainTreeTransactionType = entityPayment.BrainTreeTransactionType;
            var amount = entityPayment.Amount;
            if (entityPayment.MedicationFeesAmount.HasValue)
            {
                amount += entityPayment.MedicationFeesAmount.Value;
            }
            if (entityPayment.DeliveryFeesAmount.HasValue)
            {
                amount += entityPayment.DeliveryFeesAmount.Value;
            }
            Amount = amount;
            CreateDate = entityPayment.CreateDate;
            PlatformAmount = entityPayment.PlatformAmount;
            HcpAmount = entityPayment.HCPAmount;
            CutPercent = entityPayment.CutPercent;
            ActualAmount = entityPayment.ActualAmount;
            ConsultationAmount = entityPayment.Amount;
            MedicationAmount = entityPayment.MedicationFeesAmount ?? 0;
            DeliveryAmount = entityPayment.DeliveryFeesAmount ?? 0;
            IsPatientCorporate = entityPayment.UserCorporateId.HasValue;
            PatientUserCorporateType = entityPayment.CorporateUserType;
            if (isDetailed)
            {
                ChatRoom = new ChatRoomModel(entityPayment.ChatRoom);
                if (entityPayment.PromoCode != null)
                {
                    PromoCode = new PromoCodeModel(entityPayment.PromoCode);
                }
            }
        }
    }
}