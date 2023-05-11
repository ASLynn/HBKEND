using HeyDoc.Web.Resources;
using HeyDoc.Web.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HeyDoc.Web.Models
{
    public class DeviceModel
    {
        public DeviceType DeviceType { get; set; }
        public string DeviceId { get; set; }
        public string RegistrationId { get; set; }
        public string GuiVersion { get; set; }
        public DateTime CreateDate { get; set; }

        public DeviceModel()
        { 
        
        }

        public DeviceModel(Entity.Device entityDevice)
        {
            DeviceType = entityDevice.DeviceType;
            DeviceId = entityDevice.DeviceId;
            RegistrationId = entityDevice.RegistrationId;
            GuiVersion = entityDevice.GuiVersion;
            CreateDate = entityDevice.CreateDate;
        }

        public void Validate()
        {
            if (DeviceType == DeviceType.Invalid || !Enum.IsDefined(typeof(DeviceType), DeviceType))
            {
                throw new WebApiException(
                    new WebApiError(WebApiErrorCode.InvalidArguments, "Invalid Device Type: " + DeviceType));
            }
            if (string.IsNullOrEmpty(DeviceId))
            {
                throw new WebApiException(
                    new WebApiError(WebApiErrorCode.InvalidArguments, Forms.ErrorDeviceIDNull));
            }
        }
    }
}