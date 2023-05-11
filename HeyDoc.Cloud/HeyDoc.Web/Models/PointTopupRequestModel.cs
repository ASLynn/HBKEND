using Amazon.Runtime.Internal;
using DocumentFormat.OpenXml.Spreadsheet;
using HeyDoc.Web.Entity;
using System;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace HeyDoc.Web.Models
{
    public class PointTopupRequestModel
    {
        public int ID { get; set; }
        public string OrderId { get; set; }
        public int UserId { get; set; }
        public int? Amount { get; set; }    
        public string Payment_Method { get; set; }
        public string Status { get; set; }
        public DateTime RequestDate { get; set; }
        public DateTime? RequestSuccessDate { get; set; }
        public string Remark { get; set; }

        public PointTopupRequestModel() { }
        public PointTopupRequestModel(Entity.PointTopupRequest entity)
        {
            ID = entity.ID;
            OrderId = entity.RequestID;
            UserId = entity.UserID;
            Amount = entity.Amount;
            Payment_Method = entity.PaymentMethod;
            Status = entity.Status;
            RequestDate = entity.RequestDate;
            RequestSuccessDate = entity.RequestSuccessDate;
            Remark = entity.Remark;
        }
    }
}