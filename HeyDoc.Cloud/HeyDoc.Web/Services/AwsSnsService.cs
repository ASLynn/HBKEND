using Amazon;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;


/* 
 *TODO : 1. Change the  constant variables
 *       2. Add columns to device table
 *       3.
 */


namespace HeyDoc.Web.Services
{
    public class AwsSnsService
    {
        static ILog _logger = LogManager.GetLogger("ServiceLog");

        private static Regex existingEndpointRegex = new Regex(".*Endpoint (arn:aws:sns.+) already exists with the same Token", RegexOptions.IgnoreCase);

        // TODO M UNBLANK
        private static string AWS_AccessKeyId = "";
        private static string AWS_SecretAccessKey = "";

#if RELEASE
		private static string  AWS_SNS_iOS_Dev_PlatformArn= "";
		private static string  AWS_SNS_iOS_Prod_PlatfromArn= "";
#else
        private static string AWS_SNS_iOS_Dev_PlatformArn = "";
        private static string AWS_SNS_iOS_Prod_PlatfromArn = "";
#endif


        public static void Register(Entity.Device entityDevice)
        {
            try
            {
                if (entityDevice.DeviceType == DeviceType.IOS && !string.IsNullOrEmpty(entityDevice.RegistrationId))
                {

                    try
                    {
                        _RegisterEndPoint(entityDevice, true);
                        _RegisterEndPoint(entityDevice, false);
                    }
                    catch
                    {
                        Thread.Sleep(500);

                        _RegisterEndPoint(entityDevice, true);
                        _RegisterEndPoint(entityDevice, false);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"{ex.Message}\nDevice ID: {entityDevice.DeviceId}\nRegistration token: {entityDevice.RegistrationId}");
            }
        }

        private static void _RegisterEndPoint(Entity.Device entityDevice, bool isProduction)
        {
            try
            {
                var client = new AmazonSimpleNotificationServiceClient(AWS_AccessKeyId, AWS_SecretAccessKey, RegionEndpoint.APSoutheast1);

                var createEndpointRequest = new CreatePlatformEndpointRequest()
                {
                    PlatformApplicationArn = isProduction ? AWS_SNS_iOS_Prod_PlatfromArn : AWS_SNS_iOS_Dev_PlatformArn,
                    CustomUserData = entityDevice.DeviceType.ToString() + "_" + entityDevice.DeviceId,
                    Token = entityDevice.RegistrationId
                };
                string endpointArn;
                try
                {
                    // TODO: Look into making this call async. Must make sure functions calling _RegisterEndPoint also handle async
                    endpointArn = client.CreatePlatformEndpoint(createEndpointRequest).EndpointArn;
                }
                catch (InvalidParameterException ex)
                {
                    MatchCollection m = existingEndpointRegex.Matches(ex.Message);
                    if (m.Count > 0 && m[0].Groups.Count > 1)
                    {
                        // The platform endpoint already exists for this token, but with
                        // additional custom data that createEndpoint doesn't want to overwrite.
                        // Just use the existing platform endpoint.
                        endpointArn = m[0].Groups[1].Value;
                        // The platform endpoint is out of sync with the current data;
                        // update the token and enable it.
                        // Reference : https://docs.amazonaws.cn/en_us/sns/latest/dg/sns-dg.pdf Page:130

                        var attribs = new Dictionary<string, string>()
                        {
                            { "Token", createEndpointRequest.Token },
                            { "CustomUserData", createEndpointRequest.CustomUserData },
                            { "Enabled", "true" }
                        };
                        var endpointAttributesRequest = new SetEndpointAttributesRequest()
                        {
                            EndpointArn = endpointArn,
                            Attributes = attribs
                        };
                        // TODO: Look into making this call async. Must make sure functions calling _RegisterEndPoint also handle async
                        client.SetEndpointAttributes(endpointAttributesRequest);
                    }
                    else
                    {
                        throw;
                    }
                }

                if (isProduction)
                {
                    entityDevice.AwsIosProdEndpointArn = endpointArn;
                }
                else
                {
                    entityDevice.AwsIosDevEndpointArn = endpointArn;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public static void Push(Entity.Device entityDevice, string jsonString, bool isProductionBuild)
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                try
                {
                    if (string.IsNullOrEmpty(entityDevice.AwsIosProdEndpointArn) || string.IsNullOrEmpty(entityDevice.AwsIosDevEndpointArn))
                    {
                        Register(entityDevice);
                        try
                        {
                            db.SaveChanges();
                        }
                        catch(Exception ex)
                        {
                            _logger.Error(ex);
                        }
                    }

                    var client = new AmazonSimpleNotificationServiceClient(AWS_AccessKeyId, AWS_SecretAccessKey, RegionEndpoint.APSoutheast1);
                    string msg = new AppleNotification(isProductionBuild, jsonString).ToJsonString();
                    var response= client.Publish(
                        new PublishRequest()
                        {
                            TargetArn = isProductionBuild ? entityDevice.AwsIosProdEndpointArn : entityDevice.AwsIosDevEndpointArn,
                            Message = msg,
                            MessageStructure = "json"
                        });
                }
                catch (EndpointDisabledException ede)
                {
                    _logger.Error(ede);

                    var client = new AmazonSimpleNotificationServiceClient(AWS_AccessKeyId, AWS_SecretAccessKey, RegionEndpoint.APSoutheast1);

                    if (!string.IsNullOrEmpty(entityDevice.AwsIosProdEndpointArn))
                    {
                        client.DeleteEndpoint(new DeleteEndpointRequest() { EndpointArn = entityDevice.AwsIosProdEndpointArn });
                        entityDevice.AwsIosProdEndpointArn = null;
                    }
                    if (!string.IsNullOrEmpty(entityDevice.AwsIosDevEndpointArn))
                    {
                        client.DeleteEndpoint(new DeleteEndpointRequest() { EndpointArn = entityDevice.AwsIosDevEndpointArn });
                        entityDevice.AwsIosDevEndpointArn = null;
                    }

                    entityDevice.RegistrationId = null;

                    db.SaveChanges();

                }
                catch (Exception ex)
                {
                    _logger.Info("iOS Push failed **************!");
                    _logger.Error($"{ex.Message}\nDevice ID: {entityDevice.DeviceId}\nARN: {(isProductionBuild ? entityDevice.AwsIosProdEndpointArn : entityDevice.AwsIosDevEndpointArn)}");
                }
            }

        }

        public class AppleNotification
        {
            public string APNS { get; set; }
            public string APNS_SANDBOX { get; set; }

            public AppleNotification(bool isProduction, string jsonString)
            {
                if (isProduction)
                {
                    APNS = jsonString;
                }
                else
                {
                    APNS_SANDBOX = jsonString;
                }
            }

            public string ToJsonString()
            {
                return JsonConvert.SerializeObject(this, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
            }
        }
    }
}