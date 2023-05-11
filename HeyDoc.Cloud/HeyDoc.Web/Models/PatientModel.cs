using HeyDoc.Web.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HeyDoc.Web.Models
{
    public class PatientModel : UserModel
    {
        public float? Weight { get; set; }
        public float? Height { get; set; }
        public float? BMI { get; set; }
        public float? BodyTemperature { get; set; }
        public float? HeartRate { get; set; }

        public string BloodPressure { get; set; }
        public float? BloodGluccoseFasting { get; set; }
        public float? BloodGluccose { get; set; }
        public int? MenstrualPeriod { get; set; }
        public int? MenstrualDuration { get; set; }

        public string Allergy { get; set; }
        public string MedicationHistory { get; set; }
        public string MedicationList { get; set; }
        
        public PatientModel()
        {

        }

        public PatientModel(Entity.UserProfile entityUser, Entity.Patient entityPatient)
            : base(entityUser, isNotDoctor: true)
        {
            Allergy = entityPatient.Allergy != null ? entityPatient.Allergy : "";
            MedicationHistory = entityPatient.MedicationHistory != null ? entityPatient.MedicationHistory : "";
            MedicationList = entityPatient.MedicationList != null ? entityPatient.MedicationList : "";
            IC = entityUser.IC;
            PhoneNumber = entityUser.PhoneNumber;
            Address = entityUser.Address;
            BloodType = entityUser.BloodType;
        }

        public PatientModel(Entity.Patient entityPatient) : base(entityPatient.UserProfile, isNotDoctor: true)
        {
            Allergy = entityPatient != null ? entityPatient.Allergy : null;
            MedicationHistory = entityPatient != null ? entityPatient.MedicationHistory : null;
            MedicationList = entityPatient != null ? entityPatient.MedicationList : null;
            BloodType = entityPatient.UserProfile.BloodType;
        }

        public void Validate()
        {
            if (!string.IsNullOrEmpty(MedicationHistory) && MedicationHistory.Length > 5000)
            {
                throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidArguments, "Medication History should be less than 5000 characters"));
            }

            if (!string.IsNullOrEmpty(MedicationList) && MedicationList.Length > 5000)
            {
                throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidArguments, "Medication list should be less than 5000 characters"));
            }

            if (!string.IsNullOrEmpty(Allergy) && Allergy.Length > 5000)
            {
                throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidArguments, "Allergy should be less than 5000 characters"));
            }
            if (Weight.HasValue && Weight.Value.ToString().Length > 10)
            {
                throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidArguments, "Invalid value for weight."));
            }
            if (Height.HasValue && Height.Value.ToString().Length > 9)
            {
                throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidArguments, "Invalid value for Height."));
            }
        }
    }

    public class WeightModel
    {
        public long WeightId { get; set; }
        public int UserId { get; set; }
        public float Weight { get; set; }
        public System.DateTime CreateDate { get; set; }

        public WeightModel()
        {

        }

        public WeightModel(Entity.WeightHistory entityWeight)
        {
            Weight = entityWeight.Weight;
            CreateDate = entityWeight.CreateDate;
        }
    }

    public class HeightModel
    {
        public long HeightId { get; set; }
        public int UserId { get; set; }
        public float Height { get; set; }
        public System.DateTime CreateDate { get; set; }

        public HeightModel()
        {

        }

        public HeightModel(Entity.HeightHistory entityHeight)
        {
            Height = entityHeight.Height;
            CreateDate = entityHeight.CreateDate;
        }
    }

    public class BMIModel
    {
        public long BMIId { get; set; }
        public int UserId { get; set; }
        public float BMI { get; set; }
        public System.DateTime CreateDate { get; set; }

        public BMIModel()
        {

        }

        public BMIModel(Entity.BMIHistory entityBMI)
        {
            BMI = entityBMI.BMI;
            CreateDate = entityBMI.CreateDate;
        }
    }

    public class BodyTemperatureModel
    {
        public long TempreratureId { get; set; }
        public int UserId { get; set; }
        public float BodyTemperature { get; set; }
        public System.DateTime CreateDate { get; set; }

        public BodyTemperatureModel()
        {

        }

        public BodyTemperatureModel(Entity.BodyTemperatureHistory entityBodyTemperature)
        {
            BodyTemperature = entityBodyTemperature.BodyTemperature;
            CreateDate = entityBodyTemperature.CreateDate;
        }

    }

    public class HeartRateModel
    {
        public long HeartRateId { get; set; }
        public int UserId { get; set; }
        public float HeartRate { get; set; }
        public System.DateTime CreateDate { get; set; }

        public HeartRateModel()
        {

        }

        public HeartRateModel(Entity.HeartRateHistory entityHeartRate)
        {
            HeartRate = entityHeartRate.HeartRate;
            CreateDate = entityHeartRate.CreateDate;
        }
    }
    public class BloodPressureModel
    {
        public long PressureId { get; set; }
        public int UserId { get; set; }
        public System.DateTime CreateDate { get; set; }
        public string BloodPressure { get; set; }

        public BloodPressureModel()
        {

        }

        public BloodPressureModel(Entity.BloodPressureHistory entityBloodPressure)
        {
            BloodPressure = entityBloodPressure.BloodPressure;
            CreateDate = entityBloodPressure.CreateDate;
        }
    }
    public class BloodGluccoseFastingModel
    {
        public long GluccoseId { get; set; }
        public int UserId { get; set; }
        public float BloodGluccose { get; set; }
        public System.DateTime CreateDate { get; set; }

        public BloodGluccoseFastingModel()
        {

        }

        public BloodGluccoseFastingModel(Entity.BloodGluccoseFastingHistory entityBloodGluccoseFasting)
        {
            BloodGluccose = entityBloodGluccoseFasting.BloodGluccose;
            CreateDate = entityBloodGluccoseFasting.CreateDate;
        }
    }
    public class BloodGluccoseModel
    {
        public long GluccoseId { get; set; }
        public int UserId { get; set; }
        public float BloodGluccose { get; set; }
        public System.DateTime CreateDate { get; set; }

        public BloodGluccoseModel()
        {

        }

        public BloodGluccoseModel(Entity.BloodGluccoseHistory entityBloodGluccose)
        {
            BloodGluccose = entityBloodGluccose.BloodGluccose;
            CreateDate = entityBloodGluccose.CreateDate;
        }
    }

    public class MenstrualPeriodModel
    {
        public long MenstrualPeriodId { get; set; }
        public int UserId { get; set; }
        public float MenstrualPeriod { get; set; }
        public System.DateTime CreateDate { get; set; }

        public MenstrualPeriodModel()
        {

        }

        public MenstrualPeriodModel(Entity.MenstrualPeriodHistory entityMenstrualPeriod)
        {
            MenstrualPeriod = entityMenstrualPeriod.MenstrualPeriod;
            CreateDate = entityMenstrualPeriod.CreateDate;
        }
    }

    public class MenstrualDurationModel
    {
        public long MenstrualDurationId { get; set; }
        public int UserId { get; set; }
        public float MenstrualDuration { get; set; }
        public System.DateTime CreateDate { get; set; }

        public MenstrualDurationModel()
        {

        }

        public MenstrualDurationModel(Entity.MenstrualDurationHistory entityMenstrualDuration)
        {
            MenstrualDuration = entityMenstrualDuration.MenstrualDuration;
            CreateDate = entityMenstrualDuration.CreateDate;
        }
    }

    public class BioDataModel
    {
        public List<WeightModel> WeightList { get; set; }
        public List<HeightModel> HeightList { get; set; }
        public List<BMIModel> BMIList { get; set; }
        public List<BodyTemperatureModel> BodyTemperatureList { get; set; }
        public List<HeartRateModel> HeartRateList { get; set; }
        public List<BloodPressureModel> BloodPressureList { get; set; }
        public List<BloodGluccoseFastingModel> BloodGluccoseFastingList { get; set; }
        public List<BloodGluccoseModel> BloodGluccoseList { get; set; }
        public List<MenstrualPeriodModel> MenstrualPeriodList { get; set; }
        public List<MenstrualDurationModel> MenstrualDurationList { get; set; }

        public string BloodType { get; set; }
        public BioDataModel()
        {
            WeightList = new List<WeightModel>();
            HeightList = new List<HeightModel>();
            BMIList = new List<BMIModel>();
            BodyTemperatureList = new List<BodyTemperatureModel>();
            HeartRateList = new List<HeartRateModel>();
            BloodPressureList = new List<BloodPressureModel>();
            BloodGluccoseFastingList = new List<BloodGluccoseFastingModel>();
            BloodGluccoseList = new List<BloodGluccoseModel>();
            MenstrualPeriodList = new List<MenstrualPeriodModel>();
            MenstrualDurationList = new List<MenstrualDurationModel>();
        }
    }
}