using System.Web.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HeyDoc.Web.Models;
using System.Collections.Immutable;

namespace HeyDoc.Web.Helpers
{
    public class ConstantHelper
    {
        private static string _serverUrl;
        public static string ServerUrl
        {
            get
            {
                if (string.IsNullOrEmpty(_serverUrl))
                {
                    _serverUrl = WebConfigurationManager.AppSettings["ServerUrl"];
                    if (System.Web.HttpContext.Current.Request.Url.ToString().Contains("hopestaging"))
                    {
                        _serverUrl = "https://hopestaging.azurewebsites.net";
                    }

                    if (System.Web.HttpContext.Current.Request.Url.ToString().Contains("localhost"))
                    {
                        _serverUrl = "http://localhost:46484/";
                    }

                    if (System.Web.HttpContext.Current.Request.Url.ToString().Contains("web.hope.com.mm"))
                    {
                        _serverUrl = "https://web.hope.com.mm/";
                    }

                }
                return _serverUrl;
            }
        }

        public const int PrimaryDoctorGroupId =
#if DEBUG
            0;
#else
            0;
#endif

        // List of languages to use when sending messages in multiple languages
        public static readonly List<string> MultilingualLangs = new List<string>
        {
            "en",
            "zh-CN"
        };

        private static HashSet<RoleType> _doctorRoles;
        public static HashSet<RoleType> DoctorRoles
        {
            get
            {
                if (_doctorRoles == null)
                {
                    _doctorRoles = new HashSet<RoleType>
                    {
                        RoleType.Doctor
                    };
                }
                return _doctorRoles;
            }
        }

        public static readonly byte[] PrescriptionAccessEncryptionKey = { 118, 227, 165, 245, 115, 82, 180, 69, 155, 208, 56, 136, 170, 143, 41, 49 };

        // TODO M UNBLANK: URL of Web App
        public static readonly string WebUrl =
#if DEBUG
            "https://telemed-web-staging.azurewebsites.net";
#else
            "";
#endif

        // TODO M UNBLANK
        public static string GoogleAnalyticsId
        {
            get
            {
#if DEBUG
                return "";
#else
                return "";
#endif
            }
        }

        // TODO M UNBLANK: Support contact email for users
        public const string Doc2UsEmailContact = "";

        public const string IcdApiOAuthEndpoint = "https://icdaccessmanagement.who.int/connect/token";

        public const int VeterinaryCategoryId =
#if DEBUG
            0;
#else
            0;
#endif

        public const string Doc2UsGoogleApiCredsFilename = "google-credential.json";

        public const string CorporatePrescriptionCountMonthlyReportSheetId =
#if DEBUG
            "";
#else
            "";
#endif

        public const string CorporateSignUpCountMonthlyReportSheetId =
#if DEBUG
            "";
#else
            "";
#endif

        public const string DoctorChatAndEpsStatsReportSheetId =
#if DEBUG
            "";
#else
            "";
#endif

        public const string PushNotificationServiceBusQueueName = "push-notification";

        public const int DoctorPrescriptionSourceId = 1;
        // TODO M: Possibly rename
        public const int Pharmacy1PrescriptionSourceId = 2;

        public static readonly ImmutableHashSet<int> DoctorPrescriptionSourceIds = ImmutableHashSet.Create(
            DoctorPrescriptionSourceId
        );


        public const string MyGdexPrimeApiBaseUrl =
#if DEBUG
            "https://myopenapi.gdexpress.com/api/demo/prime";
#else
            "https://myopenapi.gdexpress.com/api/prime";
#endif
    }
}
