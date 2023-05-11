using HeyDoc.Web.Helpers;
using HeyDoc.Web.Lib;
using HeyDoc.Web.Models;
using HeyDoc.Web.Resources;
using HeyDoc.Web.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HeyDoc.Web.Services
{
    public class PatientService
    {
        public static PatientModel ViewBioData(string accessToken, int? userId = null)
        {
            ActivityAuditHelper.AddRequestParamsToLog();
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityUser = AccountService.GetEntityUserByAccessToken(db, accessToken, false);
                Entity.Patient entityPatient = null;
                PatientModel result = null;
                if (userId.HasValue)
                {
                    if (userId.Value != entityUser.UserId && !entityUser.Roles.Any(r => ConstantHelper.DoctorRoles.Contains(r)))
                    {
                        throw new WebApiException(new WebApiError(WebApiErrorCode.Unauthorized));
                    }
                    var entityTargetUser = AccountService.GetEntityTargetUserByUserId(db, userId.Value, false);
                    entityPatient = entityTargetUser.Patient;
                    result = new PatientModel(entityTargetUser, entityPatient);
                }
                else
                {
                    entityPatient = entityUser.Patient;
                    result = new PatientModel(entityUser, entityPatient);
                }

                return _ViewBioDataPartial(entityPatient, result);
            }
        }

        // hw 20150714 : create
        public static PatientModel ViewBioData(Entity.UserProfile entityUser)
        {
            PatientModel result = null;
            var entityPatient = entityUser.Patient;
            result = new PatientModel(entityUser, entityPatient);
            if (entityPatient != null)
            {
                return _ViewBioDataPartial(entityPatient, result);
            }

            // If entityPatient is null then no bio data can be fetched
            // and a PatientModel with null bio data fields will be returned
            return result;
        }

        private static PatientModel _ViewBioDataPartial(Entity.Patient entityPatient, PatientModel result)
        {
            if (entityPatient == null)
            {
                throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidAction));
            }

            result.BloodGluccose = entityPatient.BloodGluccoseHistories.Count() > 0 ? entityPatient.BloodGluccoseHistories.OrderByDescending(e => e.CreateDate).FirstOrDefault().BloodGluccose : 0;
            result.BloodGluccoseFasting = entityPatient.BloodGluccoseFastingHistories.Count() > 0 ? entityPatient.BloodGluccoseFastingHistories.OrderByDescending(e => e.CreateDate).FirstOrDefault().BloodGluccose : 0;
            result.BloodPressure = entityPatient.BloodPressureHistories.Count() > 0 ? entityPatient.BloodPressureHistories.OrderByDescending(e => e.CreateDate).FirstOrDefault().BloodPressure : "";
            result.BodyTemperature = entityPatient.BodyTemperatureHistories.Count() > 0 ? entityPatient.BodyTemperatureHistories.OrderByDescending(e => e.CreateDate).FirstOrDefault().BodyTemperature : 0;
            result.HeartRate = entityPatient.HeartRateHistories.Count() > 0 ? entityPatient.HeartRateHistories.OrderByDescending(e => e.CreateDate).FirstOrDefault().HeartRate : 0;
            result.BMI = entityPatient.BMIHistories.Count() > 0 ? entityPatient.BMIHistories.OrderByDescending(e => e.CreateDate).FirstOrDefault().BMI : 0;
            result.Weight = entityPatient.WeightHistories.Count() > 0 ? entityPatient.WeightHistories.OrderByDescending(e => e.CreateDate).FirstOrDefault().Weight : 0;
            result.Height = entityPatient.HeightHistories.Count() > 0 ? entityPatient.HeightHistories.OrderByDescending(e => e.CreateDate).FirstOrDefault().Height : 0;
            if (entityPatient.UserProfile.Gender == Gender.Female)
            {
                result.MenstrualPeriod = entityPatient.MenstrualPeriodHistories.Count() > 0 ? entityPatient.MenstrualPeriodHistories.OrderByDescending(e => e.CreateDate).FirstOrDefault().MenstrualPeriod : 0;
                result.MenstrualDuration = entityPatient.MenstrualDurationHistories.Count() > 0 ? entityPatient.MenstrualDurationHistories.OrderByDescending(e => e.CreateDate).FirstOrDefault().MenstrualDuration : 0;
            }
            else
            {
                result.MenstrualPeriod = 0;
                result.MenstrualDuration = 0;
            }

            return result;
        }

        public static BoolResult UpdatePatientBioData(string accessToken, PatientModel model)
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityDoctor = AccountService.GetEntityUserByAccessToken(db, accessToken, false, RoleType.Doctor);
                var entityPatient = AccountService.GetEntityTargetUserByUserId(db, model.UserId, false);
                if (entityPatient.Role != RoleType.User)
                {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidAction, "You can update the profile of a patient only."));
                }
                var isHCPAttended = db.ChatRooms.Any(e => e.DoctorId == entityDoctor.UserId && e.PatientId == entityPatient.UserId);
                if (!isHCPAttended)
                {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.Unauthorized, HCPPatient.ErrorCannotChangeNonConsultedPatientBiodata));
                }
                _UpdateBioDataPartial(model, db, entityPatient.Patient);
                return new BoolResult(true);
            }
        }

        public static BoolResult UpdateBioData(string accessToken, PatientModel model)
        {
            ActivityAuditHelper.AddRequestParamsToLog();
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityUser = AccountService.GetEntityUserByAccessToken(db, accessToken, false, RoleType.User);

                _UpdateBioDataPartial(model, db, entityUser.Patient);

                return new BoolResult(true);
            }
        }

        internal static void _UpdateBioDataPartial(PatientModel model, Entity.db_HeyDocEntities db, Entity.Patient entityPatient)
        {
            model.Validate();
            if (entityPatient == null)
            {
                throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidAction));
            }
            var entityUser = entityPatient.UserProfile;
            DateTime now = DateTime.UtcNow;
            entityUser.LastUpdateDate = now;

            if (model.Weight.HasValue)
            {
                var entityWeight = db.WeightHistories.Create();
                entityWeight.Weight = model.Weight.Value;
                entityWeight.CreateDate = now;
                entityWeight.UserId = entityUser.UserId;

                db.WeightHistories.Add(entityWeight);
            }
            if (model.Height.HasValue)
            {
                var entityHeight = db.HeightHistories.Create();
                entityHeight.Height = model.Height.Value;
                entityHeight.CreateDate = now;
                entityHeight.UserId = entityUser.UserId;

                db.HeightHistories.Add(entityHeight);
            }
            if (model.BMI.HasValue)
            {
                var entityBMI = db.BMIHistories.Create();
                entityBMI.BMI = model.BMI.Value;
                entityBMI.CreateDate = now;
                entityBMI.UserId = entityUser.UserId;

                db.BMIHistories.Add(entityBMI);
            }
            if (model.BodyTemperature.HasValue)
            {
                var entityBodyTemperature = db.BodyTemperatureHistories.Create();
                entityBodyTemperature.BodyTemperature = model.BodyTemperature.Value;
                entityBodyTemperature.CreateDate = now;
                entityBodyTemperature.UserId = entityUser.UserId;

                db.BodyTemperatureHistories.Add(entityBodyTemperature);
            }
            if (model.HeartRate.HasValue)
            {
                var entityHeartRate = db.HeartRateHistories.Create();
                entityHeartRate.HeartRate = model.HeartRate.Value;
                entityHeartRate.CreateDate = now;
                entityHeartRate.UserId = entityUser.UserId;

                db.HeartRateHistories.Add(entityHeartRate);
            }
            if (!string.IsNullOrEmpty(model.BloodPressure))
            {
                var entityBloodPressure = db.BloodPressureHistories.Create();
                entityBloodPressure.BloodPressure = model.BloodPressure;
                entityBloodPressure.CreateDate = now;
                entityBloodPressure.UserId = entityUser.UserId;

                db.BloodPressureHistories.Add(entityBloodPressure);
            }
            if (model.BloodGluccose.HasValue)
            {
                var entityBloodGluccose = db.BloodGluccoseHistories.Create();
                entityBloodGluccose.BloodGluccose = model.BloodGluccose.Value;
                entityBloodGluccose.CreateDate = now;
                entityBloodGluccose.UserId = entityUser.UserId;

                db.BloodGluccoseHistories.Add(entityBloodGluccose);
            }
            if (model.BloodGluccoseFasting.HasValue)
            {
                var entityBloodGluccoseFasting = db.BloodGluccoseFastingHistories.Create();
                entityBloodGluccoseFasting.BloodGluccose = model.BloodGluccoseFasting.Value;
                entityBloodGluccoseFasting.CreateDate = now;
                entityBloodGluccoseFasting.UserId = entityUser.UserId;

                db.BloodGluccoseFastingHistories.Add(entityBloodGluccoseFasting);
            }

            if (entityUser.Gender == Gender.Female)
            {
                if (model.MenstrualPeriod.HasValue)
                {
                    var entityMenstrualPeriod = db.MenstrualPeriodHistories.Create();
                    entityMenstrualPeriod.MenstrualPeriod = model.MenstrualPeriod.Value;
                    entityMenstrualPeriod.CreateDate = now;
                    entityMenstrualPeriod.UserId = entityUser.UserId;

                    db.MenstrualPeriodHistories.Add(entityMenstrualPeriod);
                }

                if (model.MenstrualDuration.HasValue)
                {
                    var entityMenstrualDuration = db.MenstrualDurationHistories.Create();
                    entityMenstrualDuration.MenstrualDuration = model.MenstrualDuration.Value;
                    entityMenstrualDuration.CreateDate = now;
                    entityMenstrualDuration.UserId = entityUser.UserId;

                    db.MenstrualDurationHistories.Add(entityMenstrualDuration);
                }
            }

            if (!string.IsNullOrEmpty(model.Address))
            {
                entityPatient.UserProfile.Address = model.Address;
            }
            if (!string.IsNullOrEmpty(model.IC))
            {
                model.IC = model.IC.Replace("-", "");
                entityPatient.UserProfile.IC = model.IC;
            }
            if (!string.IsNullOrEmpty(model.PhoneNumber))
            {
                entityPatient.UserProfile.PhoneNumber = model.PhoneNumber;
            }
            if (!string.IsNullOrEmpty(model.Allergy))
            {
                entityPatient.Allergy = model.Allergy;
            }
            if (!string.IsNullOrEmpty(model.MedicationList))
            {
                entityPatient.MedicationList = model.MedicationList;
            }
            if (!string.IsNullOrEmpty(model.MedicationHistory))
            {
                entityPatient.MedicationHistory = model.MedicationHistory;
            }
            if (!string.IsNullOrEmpty(model.BloodType))
            {
                entityPatient.UserProfile.BloodType = model.BloodType;
            }
            db.SaveChanges();
        }

        public static BioDataModel BioDataList(string accessToken, BioDataType type, int? userId = null)
        {
            ActivityAuditHelper.AddRequestParamsToLog();
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityUser = AccountService.GetEntityUserByAccessToken(db, accessToken, false);

                Entity.Patient entityPatient = null;

                if (userId.HasValue)
                {
                    if (userId.Value != entityUser.UserId && !entityUser.Roles.Any(r => ConstantHelper.DoctorRoles.Contains(r)))
                    {
                        throw new WebApiException(new WebApiError(WebApiErrorCode.Unauthorized));
                    }
                    var entityTargetUser = AccountService.GetEntityTargetUserByUserId(db, userId.Value, false);
                    entityPatient = entityTargetUser.Patient;
                }
                else
                {
                    entityPatient = entityUser.Patient;
                }

                if (entityPatient == null)
                {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidAction));
                }

                var bioData = new BioDataModel();
                if (type == BioDataType.All || type == BioDataType.BodyTemperature)
                {
                    var entityTemperatureList = entityPatient.BodyTemperatureHistories.OrderByDescending(e => e.CreateDate).Take(10);
                    List<BodyTemperatureModel> result = new List<BodyTemperatureModel>();

                    foreach (var entityTemperature in entityTemperatureList)
                    {
                        result.Add(new BodyTemperatureModel(entityTemperature));
                    }

                    bioData.BodyTemperatureList = result;
                }

                if (type == BioDataType.All || type == BioDataType.Weight)
                {
                    var entityWeightList = entityPatient.WeightHistories.OrderByDescending(e => e.CreateDate).Take(10);
                    List<WeightModel> result = new List<WeightModel>();

                    foreach (var entityWeight in entityWeightList)
                    {
                        result.Add(new WeightModel(entityWeight));
                    }

                    bioData.WeightList = result;
                }

                if (type == BioDataType.All || type == BioDataType.Height)
                {
                    var entityHeightList = entityPatient.HeightHistories.OrderByDescending(e => e.CreateDate).Take(10);
                    List<HeightModel> result = new List<HeightModel>();

                    foreach (var entityHeight in entityHeightList)
                    {
                        result.Add(new HeightModel(entityHeight));
                    }

                    bioData.HeightList = result;
                }

                if (type == BioDataType.All || type == BioDataType.BMI)
                {
                    var entityBMIList = entityPatient.BMIHistories.OrderByDescending(e => e.CreateDate).Take(10);
                    List<BMIModel> result = new List<BMIModel>();

                    foreach (var entityBMI in entityBMIList)
                    {
                        result.Add(new BMIModel(entityBMI));
                    }

                    bioData.BMIList = result;
                }

                if (type == BioDataType.All || type == BioDataType.BloodPressure)
                {
                    var entitBloodPressureList = entityPatient.BloodPressureHistories.OrderByDescending(e => e.CreateDate).Take(10);
                    List<BloodPressureModel> result = new List<BloodPressureModel>();

                    foreach (var entityBloodPressure in entitBloodPressureList)
                    {
                        result.Add(new BloodPressureModel(entityBloodPressure));
                    }

                    bioData.BloodPressureList = result;
                }

                if (type == BioDataType.All || type == BioDataType.HeartRate)
                {
                    var entityHeartRateList = entityPatient.HeartRateHistories.OrderByDescending(e => e.CreateDate).Take(10);
                    List<HeartRateModel> result = new List<HeartRateModel>();

                    foreach (var entityHeartRate in entityHeartRateList)
                    {
                        result.Add(new HeartRateModel(entityHeartRate));
                    }

                    bioData.HeartRateList = result;
                }

                if (type == BioDataType.All || type == BioDataType.BloodGluccose)
                {
                    var entityBloodGluccoseList = entityPatient.BloodGluccoseHistories.OrderByDescending(e => e.CreateDate).Take(10);
                    List<BloodGluccoseModel> result = new List<BloodGluccoseModel>();

                    foreach (var entityBloodGluccose in entityBloodGluccoseList)
                    {
                        result.Add(new BloodGluccoseModel(entityBloodGluccose));
                    }

                    bioData.BloodGluccoseList = result;
                }

                if (type == BioDataType.All || type == BioDataType.BloodGluccoseFasting)
                {
                    var entityBloodGluccoseFastingList = entityPatient.BloodGluccoseFastingHistories.OrderByDescending(e => e.CreateDate).Take(10);
                    List<BloodGluccoseFastingModel> result = new List<BloodGluccoseFastingModel>();

                    foreach (var entityBloodGluccoseFasting in entityBloodGluccoseFastingList)
                    {
                        result.Add(new BloodGluccoseFastingModel(entityBloodGluccoseFasting));
                    }

                    bioData.BloodGluccoseFastingList = result;
                }

                if (type == BioDataType.All || type == BioDataType.MenstrualPeriod)
                {
                    var entityMenstrualPeriodList = entityPatient.MenstrualPeriodHistories.OrderByDescending(e => e.CreateDate).Take(10);
                    List<MenstrualPeriodModel> result = new List<MenstrualPeriodModel>();

                    foreach (var entityMenstrualPeriod in entityMenstrualPeriodList)
                    {
                        result.Add(new MenstrualPeriodModel(entityMenstrualPeriod));
                    }

                    bioData.MenstrualPeriodList = result;
                }

                if (type == BioDataType.All || type == BioDataType.MenstrualDuration)
                {
                    var entityMenstrualDurationList = entityPatient.MenstrualDurationHistories.OrderByDescending(e => e.CreateDate).Take(10);
                    List<MenstrualDurationModel> result = new List<MenstrualDurationModel>();

                    foreach (var entityMenstrualDuration in entityMenstrualDurationList)
                    {
                        result.Add(new MenstrualDurationModel(entityMenstrualDuration));
                    }

                    bioData.MenstrualDurationList = result;
                }
                bioData.BloodType = entityUser.BloodType;
                return bioData;
            }
        }
    }
}