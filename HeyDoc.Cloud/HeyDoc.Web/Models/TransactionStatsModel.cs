using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HeyDoc.Web.Models
{
    public class TransactionStatsModel
    {
        [Index(0)]
        [Name("Payment Request ID")]
        public long PaymentRequestId { get; set; }
        [Index(1)]
        [Name("Patient Name")]
        public string PatientName { get; set; }
        [Index(2)]
        [Name("Patient Email")]
        public string PatientEmail { get; set; }
        [Index(3)]
        [Name("Patient Birthday")]
        [Format("yyyy-MM-dd")]
        public DateTime? PatientBirthday { get; set; }
        [Index(4)]
        [Name("HCP Name")]
        public string HcpName { get; set; }
        [Index(5)]
        [Name("HCP Email")]
        public string HcpEmail { get; set; }
        [Index(6)]
        [Name("HCP Birthday")]
        [Format("yyyy-MM-dd")]
        public DateTime? HcpBirthday { get; set; }
        [Index(7)]
        [Name("HCP Is Partner")]
        public bool HcpIsPartner { get; set; }
        [Index(8)]
        [Name("Amount Paid")]
        public decimal AmountPaid { get; set; }
        [Index(9)]
        [Name("Original Amount")]
        public decimal OriginalAmount { get; set; }
        [Index(10)]
        [Name("Platform Earning")]
        public decimal PlatformEarning { get; set; }
        [Index(11)]
        [Name("HCP Amount")]
        public decimal HcpAmount { get; set; }
        [Index(12)]
        [Name("Status")]
        public string PaymentStatus { get; set; }
        [Index(13)]
        [Name("Date")]
        [Format("yyyy-MM-dd HH:mm")]
        public DateTime PaymentRequestTime { get; set; }
        [Index(14)]
        [Name("Braintree Transaction ID")]
        public string BraintreeTransactionId { get; set; }
        [Index(15)]
        [Name("Braintree Transaction Status")]
        public string BraintreeTransactionStatus { get; set; }
    }
}