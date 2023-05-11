using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HeyDoc.Web.Models
{
    public class BrainTreePaymentModel
    {
            public string PaymentMethodNonce { get; set; }
            public string PaymentMethodToken { get; set; }

            public BrainTreePaymentModel() { }
    }
}