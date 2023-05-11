using Braintree;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace HeyDoc.Web.Lib
{
    public class BrainTreeConfiguration
    {
        public string EnvironmentStr { get; set; }
        public Braintree.Environment Environment { get; set; }
        public string MerchantId { get; set; }
        public string PublicKey { get; set; }
        public string PrivateKey { get; set; }
        private IBraintreeGateway BraintreeGateway { get; set; }

        public IBraintreeGateway CreateGateway()
        {
            MerchantId = GetConfigurationSetting("BraintreeMerchantId");
            PublicKey = GetConfigurationSetting("BraintreePublicKey");
            PrivateKey = GetConfigurationSetting("BraintreePrivateKey");
            EnvironmentStr = GetConfigurationSetting("BraintreeEnvironment");

            switch (EnvironmentStr)
            {
                case "PRODUCTION": Environment = Braintree.Environment.PRODUCTION; break;
                case "DEVELOPMENT": Environment = Braintree.Environment.DEVELOPMENT; break;
                case "QA": Environment = Braintree.Environment.QA; break;
                case "SANDBOX": Environment = Braintree.Environment.SANDBOX; break;
                default: Environment = Braintree.Environment.SANDBOX; break;
            }

            return new BraintreeGateway(Environment, MerchantId, PublicKey, PrivateKey);
        }

        public string GetConfigurationSetting(string setting)
        {
            return ConfigurationManager.AppSettings[setting];
        }

        public IBraintreeGateway GetGateway()
        {
            if (BraintreeGateway == null)
            {
                BraintreeGateway = CreateGateway();
            }

            return BraintreeGateway;
        }
    }
}