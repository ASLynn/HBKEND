using HeyDoc.Web.Models;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace HeyDoc.Web.Services
{
    public class BlockchainService
    {
        public static void RegisterParticipant(RegisterModel model)
        {
            var userAsset = new BlockchainUser()
            {
                Email = model.Email,
                Address = model.Address,
                Birthday = model.Birthday != null ? model.Birthday.Value.ToShortDateString() : null,
                Gender = model.Gender.ToString(),
                IdentityCard = model.IC,
                Name = model.FullName,
                PhoneNumber = model.PhoneNumber
            };

            var health = new BlockchainHealth()
            {
                Biodata = new BlockchainBioData(),
                AuthorizedRead = null,
                AuthorizedWrite = new List<string>() { "doc2us" },
                Owner = model.Email,
                OwnerId = model.Email
            };

            var newUser = new BlockchainRegisterModel()
            {
                Health = health,
                User = userAsset
            };

            QueueService.PublishMessage(ConfigurationManager.AppSettings["BlockchainQueueName"], new QueueMessage("Register", newUser));
        }

        public static void UpdateBioData(string email, PatientModel model)
        {
            var health = new BlockchainBioData()
            {
                Weight = model.Weight ?? 0,
                Height = model.Height ?? 0,
                Bmi = model.BMI ?? 0,
                BodyTemperature = model.BodyTemperature ?? 0,
                BloodPressure = model.BloodPressure,
                BloodGlucoseFasting = model.BloodGluccoseFasting ?? 0,
                MenstrualPeriod = model.MenstrualPeriod ?? 0,
                MenstrualDuration = model.MenstrualDuration ?? 0,
                HeartRate = model.HeartRate ?? 0,
                Allergy = model.Allergy,
                BloodGlucose = model.BloodGluccose ?? 0,
                LastEditedDate = DateTime.UtcNow
            };

            QueueService.PublishMessage(ConfigurationManager.AppSettings["BlockchainQueueName"], new QueueUpdateMessage(email, "UpdateBioData", health));
        }

        public static void UpdateUser(string email, UserModel model)
        {
            var updatedUser = new BlockchainUser()
            {
                Email = email,
                Name = model.FullName ?? "",
                Gender = model.Gender.ToString(),
                Birthday = model.Birthday != null ? model.Birthday.Value.ToShortDateString() : "",
                Address = model.Address,
                IdentityCard = model.IC,
                PhoneNumber = model.PhoneNumber
            };

            QueueService.PublishMessage(ConfigurationManager.AppSettings["BlockchainQueueName"], new QueueUpdateMessage(email, "UpdateUser", updatedUser));
        }
    }


}