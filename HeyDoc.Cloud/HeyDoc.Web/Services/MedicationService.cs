using HeyDoc.Web.Models;
using HeyDoc.Web.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Web;
using HeyDoc.Web.Resources;
using HeyDoc.Web.Entity;

namespace HeyDoc.Web.Services
{
    public class MedicationService
    {
        public static List<MedicationModel> GetMedicationList(string accessToken, int take, int skip, string searchString = null, MedicationSourceType? sourceType = MedicationSourceType.Doc2Us)
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityUser = AccountService.GetEntityUserByAccessToken(db, accessToken, false);
                if (entityUser.Doctor == null)
                {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidArguments, HCPPatient.ErrorDoctorNotFound));
                }

                var entityMedicationList = db.Medications.Where(e => !e.IsDelete).ConditionalWhere(sourceType.HasValue, e => e.MedicationSourceType == sourceType.Value);
                if (!string.IsNullOrEmpty(searchString))
                {
                    entityMedicationList = entityMedicationList.Where(e => e.MedicationName.Contains(searchString));
                }

                var entityOrdered = entityMedicationList.OrderBy(e => e.MedicationName);
                List<Medication> entityOrderedList;
                if (take == -1)
                {
                    entityOrderedList = entityOrdered.ToList();
                }
                else
                {
                    entityOrderedList = entityOrdered.Skip(skip).Take(take).ToList();
                }

                List<MedicationModel> medicationList = new List<MedicationModel>();
                foreach (var entityMedication in entityOrderedList)
                {
                    MedicationModel medication = new MedicationModel(entityMedication);
                    medicationList.Add(medication);
                }
                return medicationList;
            }
        }

        public static MedicationModel GetMedication(string accessToken, long medicationId)
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityUser = AccountService.GetEntityUserByAccessToken(db, accessToken, false);
                if (entityUser.Doctor == null)
                {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidArguments, HCPPatient.ErrorDoctorNotFound));
                }

                return new MedicationModel(GetEntityMedicationById(db, medicationId));
            }
        }

        public static IQueryable<Entity.Medication> GetMedicationsByMedicalConditionId(Entity.db_HeyDocEntities db, int medicalConditionId, int skip, int take)
        {
            var medicalConditionExists = db.MedicalConditions
                .Any(x => x.Id == medicalConditionId && !x.IsDeleted);
            if (!medicalConditionExists)
            {
                throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidArguments, HCPPatient.ErrorMedicalConditionNotFound));
            }

            var medicationEntities = db.MedicalConditionMedications
                .Where(e => e.MedicalConditionId == medicalConditionId)
                .Select(x => x.Medication)
                .Where(x => !x.IsDelete)
                .OrderBy(e => e.MedicationName)
                .Skip(skip);
            if (take >= 0)
            {
                medicationEntities = medicationEntities.Take(take);
            }

            return medicationEntities;
        }

        public static MedicationModel WebGetMedication(string userName, long medicationId)
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                //var entityUser = AccountService.GetEntityTargetUserByUsername(db, userName, false);
                return new MedicationModel(GetEntityMedicationById(db, medicationId));
            }
        }

        public static List<MedicationModel> GetMedicationsByConditionId(int conditionId)
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var medications = db.MedicalConditionMedications
                                    .Include("Medication")
                                    .Where(x => x.MedicalConditionId == conditionId)
                                    .Select(x => x.Medication)
                                    .Where(x => !x.IsDelete)
                                    .ToList();

                List<MedicationModel> medicationList = new List<MedicationModel>();
                foreach (var entityMedication in medications)
                {
                    MedicationModel medication = new MedicationModel(entityMedication);
                    medicationList.Add(medication);
                }
                return medicationList;
            }
        }

        internal static BoolResult AddMedication(MedicationModel model)
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                Entity.Medication entityMedication = new Entity.Medication
                {
                    MedicationName = model.MedicationName,
                    CreatedDate = DateTime.UtcNow,
                    IsDelete = false,
                    MedicationSourceType = MedicationSourceType.Doc2Us,
                    IsLTM = model.IsLTM
                };
                entityMedication.MedicalConditionMedications = new List<MedicalConditionMedication>();
                if (model.ForMedicalConditionsWithIds != null)
                {
                    foreach (var medicalConditionId in model.ForMedicalConditionsWithIds)
                    {
                        entityMedication.MedicalConditionMedications.Add(new MedicalConditionMedication
                        {
                            MedicationId = entityMedication.MedicationId,
                            MedicalConditionId = medicalConditionId,
                            CreatedDate = DateTime.UtcNow
                        });
                    }
                }
                db.Medications.Add(entityMedication);
                db.SaveChanges();

                return new BoolResult(true);
            }
        }

        internal static MedicationModel EditMedication(MedicationModel model)
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityMedication = GetEntityMedicationById(db, model.MedicationId);
                entityMedication.MedicationName = model.MedicationName;
                entityMedication.IsLTM = model.IsLTM;
                entityMedication.MedicalConditionMedications.Clear();
                if (model.ForMedicalConditionsWithIds != null)
                {
                    foreach (var medicalConditionId in model.ForMedicalConditionsWithIds)
                    {
                        entityMedication.MedicalConditionMedications.Add(new Entity.MedicalConditionMedication
                        {
                            MedicationId = model.MedicationId,
                            MedicalConditionId = medicalConditionId,
                            CreatedDate = DateTime.UtcNow
                        });
                    }
                }
                db.SaveChanges();

                return new MedicationModel(entityMedication);
            }
        }

        internal static Entity.Medication GetEntityMedicationById(Entity.db_HeyDocEntities db, long medicationId)
        {
            var entityMedication = db.Medications.FirstOrDefault(e => !e.IsDelete && e.MedicationId == medicationId);
            if (entityMedication == null)
            {
                throw new WebApiException(new WebApiError(WebApiErrorCode.NotFound, Medications.ErrorNotFound));
            }
            return entityMedication;
        }
    }
}