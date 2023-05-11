using HeyDoc.Web.Models;
using HeyDoc.Web.Resources;
using HeyDoc.Web.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HeyDoc.Web.Services
{
    public class DeviceService
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static DeviceModel RegisterDevice(DeviceModel model)
        {
            model.Validate();
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                return new DeviceModel(CreateOrUpdateEntityDevice(db, model, AccessTokenType.Doc2Us));
            }
        }

        #region Internal Entity Methods

        internal static Entity.Device CreateOrUpdateEntityDevice(Entity.db_HeyDocEntities db, DeviceModel model, AccessTokenType tokenType)
        {
            DateTime now = DateTime.UtcNow;

            var entityDevice = db.Devices.FirstOrDefault(e =>
                e.DeviceType == model.DeviceType &&
                e.DeviceId == model.DeviceId &&
                e.TokenType == tokenType);

            if (entityDevice == null)
            {
                entityDevice = new Entity.Device();
                entityDevice.DeviceType = model.DeviceType;
                entityDevice.DeviceId = model.DeviceId;
                entityDevice.TokenType = tokenType;
                entityDevice.CreateDate = now;

                db.Devices.Add(entityDevice);
            }
            if (!string.IsNullOrEmpty(model.RegistrationId) && (model.RegistrationId.Count() > 50))
            {
                if (model.RegistrationId != entityDevice.RegistrationId && entityDevice.DeviceType == DeviceType.IOS)
                {
                    entityDevice.AwsIosDevEndpointArn = null;
                    entityDevice.AwsIosProdEndpointArn = null;
                }
                entityDevice.RegistrationId = model.RegistrationId;
            }
            if (!string.IsNullOrEmpty(model.GuiVersion))
            {
                entityDevice.GuiVersion = model.GuiVersion;
            }
            entityDevice.LastActivityDate = now;
            if (model.DeviceType == DeviceType.IOS && !string.IsNullOrEmpty(model.RegistrationId))
            {
                // Make sure RegistrationId isn't duplicated in other devices
                var duplicatedDevices = db.Devices.Where(e => e.DeviceType == DeviceType.IOS && e.RegistrationId == entityDevice.RegistrationId && !(e.DeviceId == entityDevice.DeviceId && e.TokenType == entityDevice.TokenType));
                foreach (var duplicatedDevice in duplicatedDevices)
                {
                    duplicatedDevice.RegistrationId = null;
                }
            }
            db.SaveChanges();

            return entityDevice;
        }

        internal static Entity.Device GetEntityDevice(Entity.db_HeyDocEntities db, DeviceModel model, AccessTokenType tokenType)
        {
            var entityDevice = CreateOrUpdateEntityDevice(db, model, tokenType);
            return entityDevice;
        }

        internal static Entity.Device GetEntityDevice(Entity.db_HeyDocEntities db, string accessToken, bool nullable)
        {
            var entityDevice = db.Devices.FirstOrDefault(e => e.AccessToken == accessToken);
            if (entityDevice == null && !nullable)
            {
                throw new WebApiException(new WebApiError(WebApiErrorCode.Unauthorized, Account.ErrorSessionExpired));
            }
            entityDevice.LastActivityDate = DateTime.UtcNow;
            return entityDevice;
        }

        #endregion
    }
}