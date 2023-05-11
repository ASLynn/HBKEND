using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HeyDoc.Web.Models
{
    public class TransactionModel
    {
        public decimal TotalAmount { get; set; }
        public decimal? CashOutRequestAmount { get; set; }

        public List<PaymentRequestModel> Payments { get; set; }
    }
}