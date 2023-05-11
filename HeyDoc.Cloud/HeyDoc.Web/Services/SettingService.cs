using HeyDoc.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HeyDoc.Web.Services
{
    public class SettingService
    {
        internal const string IS_APNS_DEV = "IsAspnDev";
        internal const string IS_DIGITALSIGN_ENABLE = "IsDigitalSignEnable";

        public static List<SettingModel> GetList()
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entitySettingList = db.Settings.ToList();

                List<SettingModel> result = new List<SettingModel>();
                foreach (var entitySetting in entitySettingList)
                {
                    result.Add(new SettingModel(entitySetting));
                }
                return result;
            }
        }

        public static SettingModel UpdateSetting(string key, bool value)
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entitySetting = db.Settings.FirstOrDefault(e => e.SettingKey == key);
                if (entitySetting == null)
                {
                    entitySetting = _CreateEntitySetting(db, key, value);
                }
                entitySetting.SettingValue = value.ToString().ToLower();
                db.SaveChanges();

                return new SettingModel(entitySetting);
            }
        }

        private static Entity.Setting _CreateEntitySetting(Entity.db_HeyDocEntities db, string key, bool value)
        {
            var entitySetting = new Entity.Setting() { SettingKey = key, SettingValue = value.ToString().ToLower() };
            db.Settings.Add(entitySetting);
            db.SaveChanges();
            return entitySetting;
        }

        internal static Entity.Setting GetEntitySetting(Entity.db_HeyDocEntities db, string key)
        {
            var entitySetting = db.Settings.FirstOrDefault(e => e.SettingKey == key);
            if (entitySetting == null)
            {
                entitySetting = _CreateEntitySetting(db, key, false);
            }

            return entitySetting;
        }

        internal static bool GetSettingValue(Entity.db_HeyDocEntities db, string key)
        {
            var entitySetting = db.Settings.FirstOrDefault(e => e.SettingKey == key);
            if (entitySetting == null)
            {
                entitySetting = _CreateEntitySetting(db, key, false);
            }

            return entitySetting.SettingValue == "true";
        }
    }
}