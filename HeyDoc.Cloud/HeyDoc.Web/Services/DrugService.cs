using HeyDoc.Web.Entity;
using System.Linq;

namespace HeyDoc.Web.Services
{
    public class DrugService
    {
        public static Drug GetLatestUserDrugByMedicationId(Entity.db_HeyDocEntities db, int medicationId, int userId)
        {
            var drug = db.Drugs
                         .Include("Prescription")
                         .Where(x => x.Prescription.PatientId == userId &&
                                     x.MedicationId == medicationId &&
                                     !x.IsDelete)
                         .OrderByDescending(x => x.DrugId)
                         .FirstOrDefault();

            return drug;
        }
    }
}