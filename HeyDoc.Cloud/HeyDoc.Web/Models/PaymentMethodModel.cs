using HeyDoc.Web.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HeyDoc.Web.Models
{
    public class PaymentMethodModel
    {
        public string merchant_id;
        public string service_name;
        public string email;
        public string password;

        public PaymentMethodModel(Entity.PaymentMethod model)
        {
            merchant_id = model.merchant_id;
            service_name = model.service_name;
            email = model.email;
            password = model.password;
        }
    }
}