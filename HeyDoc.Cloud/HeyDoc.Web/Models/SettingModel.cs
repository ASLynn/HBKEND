using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HeyDoc.Web.Models
{
    public class SettingModel
    {
        public string Key { get; set; }
        public string Value { get; set; }

        public SettingModel()
        {

        }

        public SettingModel(Entity.Setting entitySetting)
        {
            Key = entitySetting.SettingKey;
            Value = entitySetting.SettingValue;
        }
    }
}