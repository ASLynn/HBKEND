using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Shared.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeyDoc.Web.Services
{
    public class SASService
    {
        private static CloudStorageAccount storageAccount = CloudStorageAccount.Parse(System.Configuration.ConfigurationManager.ConnectionStrings["StorageConnection"].ConnectionString);

        public SASService()
        {
        }

        public static string GetAccountSASToken()
        {
            var policy = new SharedAccessAccountPolicy()
            {
                Permissions = SharedAccessAccountPermissions.Read | SharedAccessAccountPermissions.List,
                Services = SharedAccessAccountServices.File,
                ResourceTypes = SharedAccessAccountResourceTypes.Service,
                SharedAccessExpiryTime = DateTime.UtcNow.AddHours(1),
                Protocols = SharedAccessProtocol.HttpsOnly
            };

            return storageAccount.GetSharedAccessSignature(policy);
        }

        public void UseAccountSAS(string sasToken)
        {
            var accountSAS = new StorageCredentials(sasToken);
            var accountWithSAS = new CloudStorageAccount(accountSAS, storageAccount.Credentials.AccountName, endpointSuffix: null, useHttps: true);
            var blobClientWithSAS = accountWithSAS.CreateCloudBlobClient();

            blobClientWithSAS.SetServiceProperties(new ServiceProperties()
            {
                HourMetrics = new MetricsProperties()
                {
                    MetricsLevel = MetricsLevel.ServiceAndApi,
                    RetentionDays = 7,
                    Version = "1.0"
                },
                MinuteMetrics = new MetricsProperties()
                {
                    MetricsLevel = MetricsLevel.ServiceAndApi,
                    RetentionDays = 7,
                    Version = "1.0"
                },
                Logging = new LoggingProperties()
                {
                    LoggingOperations = LoggingOperations.All,
                    RetentionDays = 14,
                    Version = "1.0"
                }
            });

            ServiceProperties serviceProperties = blobClientWithSAS.GetServiceProperties();
            Console.WriteLine(serviceProperties.HourMetrics.MetricsLevel);
            Console.WriteLine(serviceProperties.HourMetrics.RetentionDays);
            Console.WriteLine(serviceProperties.HourMetrics.Version);
        }

        private static async Task CreateSharedAccessPolicyAsync(CloudBlobContainer container, string policyName)
        {
            var sharedPolicy = new SharedAccessBlobPolicy()
            {
                SharedAccessExpiryTime = DateTime.UtcNow.AddHours(1),
                Permissions = SharedAccessBlobPermissions.Read | SharedAccessBlobPermissions.List
            };

            var permissions = await container.GetPermissionsAsync();

            permissions.SharedAccessPolicies.Add(policyName, sharedPolicy);
            await container.SetPermissionsAsync(permissions);
        }

        private static string GetContainerSasUri(CloudBlobContainer container, string storedPolicyName = null)
        {
            string sasContainerToken;

            if (storedPolicyName == null)
            {
                var adHocPolicy = new SharedAccessBlobPolicy()
                {
                    SharedAccessExpiryTime = DateTime.UtcNow.AddHours(1),
                    Permissions = SharedAccessBlobPermissions.List
                };

                sasContainerToken = container.GetSharedAccessSignature(adHocPolicy, null);

                Console.WriteLine("SAS for blob container (ad hoc): {0}", sasContainerToken);
                Console.WriteLine();
            }
            else
            {
                sasContainerToken = container.GetSharedAccessSignature(null, storedPolicyName);

                Console.WriteLine("SAS for blob container (stored access policy): {0}", sasContainerToken);
                Console.WriteLine();
            }

            return container.Uri + sasContainerToken;
        }

        public static string GetBlobSASUri(string containerName, string blobName, string policyName = null, DateTime? expiryDateinUtc = null)
        {
            var _container = Lib.CloudBlob.Instance.GetContainer(containerName);
            return GetBlobSasUri(_container, blobName, policyName, expiryDateinUtc);
        }

        public static string GetBlobSignature(string containerName, string fileUrl, string policyName = null, DateTime? expiryDateinUtc = null)
        {
            var _container = Lib.CloudBlob.Instance.GetContainer(containerName);
            string sasBlobToken;
            fileUrl = fileUrl.Replace(_container.Uri.ToString()+"/", "");
            var blob = _container.GetBlockBlobReference(fileUrl);

            if (policyName == null)
            {
                sasBlobToken = _GenerateSASBlobToken(expiryDateinUtc, blob);
            }
            else
            {
                sasBlobToken = blob.GetSharedAccessSignature(null, policyName);
            }
            return sasBlobToken;

        }

        public static string GetBlobSasUri(CloudBlobContainer container, string blobName, string policyName = null, DateTime? expiryDateinUtc = null)
        {
            string sasBlobToken;

            var blob = container.GetBlockBlobReference(blobName);

            if (policyName == null)
            {
                sasBlobToken = _GenerateSASBlobToken(expiryDateinUtc, blob);
            }
            else
            {
                sasBlobToken = blob.GetSharedAccessSignature(null, policyName);
            }

            return blob.Uri + sasBlobToken;
        }

        private static string _GenerateSASBlobToken(DateTime? expiryDateinUtc, CloudBlockBlob blob)
        {
            string sasBlobToken;
            var adHocSAS = new SharedAccessBlobPolicy()
            {
                SharedAccessExpiryTime = expiryDateinUtc ?? DateTime.UtcNow.AddHours(1),
                Permissions = SharedAccessBlobPermissions.Read | SharedAccessBlobPermissions.List
            };

            sasBlobToken = blob.GetSharedAccessSignature(adHocSAS);
            return sasBlobToken;
        }
    }
}
