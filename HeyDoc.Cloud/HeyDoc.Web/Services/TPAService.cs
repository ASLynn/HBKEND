using HeyDoc.Web.Entity;
using HeyDoc.Web.Models;
using HeyDoc.Web.WebApi;
using System.Collections.Generic;
using System.Linq;

namespace HeyDoc.Web.Services
{
    public class TPAService
    {
        public static List<TPAModel> GetTPAList(int skip, int take)
        {
            List<TPAModel> modelList = new List<TPAModel>();
            using (var db = new db_HeyDocEntities())
            {
                if (take > 2000)
                {
                    take = 2000;
                }
                var tpaList = db.ThirdPartyAdministrators.Where(e => !e.IsDelete).OrderBy(e => e.Name).Skip(skip);
                if (take != -1)
                {
                    tpaList.Take(take);
                }

                foreach (var tpa in tpaList)
                {
                    modelList.Add(new TPAModel(tpa));
                }
            }
            return modelList;
        }

        public static bool AddTPA(TPAModel tpaModel)
        {
            using (var db = new db_HeyDocEntities())
            {
                db.ThirdPartyAdministrators.Add(new ThirdPartyAdministrator
                {
                    Name = tpaModel.TPAName,
                    IsDelete = false,
                    SupplyingPharmacies = tpaModel.SupplyingPharmacyIds != null ? db.PrescriptionSources.Where(s => tpaModel.SupplyingPharmacyIds.Contains(s.PrescriptionSourceId)).ToList() : null
                });
                db.SaveChanges();

                return true;
            }
        }

        public static TPAModel UpdateTPA(TPAModel tpaModel)
        {
            using (var db = new db_HeyDocEntities())
            {
                var entityTPA = GetTPAById(db, tpaModel.TPAId);
                entityTPA.SupplyingPharmacies.Clear();
                if (tpaModel.SupplyingPharmacyIds != null)
                {
                    foreach (var prescriptionSource in db.PrescriptionSources.Where(s => tpaModel.SupplyingPharmacyIds.Contains(s.PrescriptionSourceId)))
                    {
                        entityTPA.SupplyingPharmacies.Add(prescriptionSource);
                    }
                }
                db.SaveChanges();

                return new TPAModel(entityTPA);
            }
        }

        public static ThirdPartyAdministrator GetTPAById(db_HeyDocEntities db, int tpaId)
        {
            var entityTPA = db.ThirdPartyAdministrators.FirstOrDefault(e => !e.IsDelete && e.TPAId == tpaId);
            if (entityTPA == null)
            {
                throw new WebApiException(new WebApiError(WebApiErrorCode.NotFound));
            }
            return entityTPA;
        }
    }
}