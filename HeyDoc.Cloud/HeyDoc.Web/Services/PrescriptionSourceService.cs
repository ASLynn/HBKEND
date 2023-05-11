using HeyDoc.Web.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HeyDoc.Web.Services
{
    public class PrescriptionSourceService
    {
        public static IEnumerable<SelectListItem> GetPrescriptionSourceSelectList(Entity.db_HeyDocEntities db)
        {
            return db.PrescriptionSources
                .OrderBy(p => p.PrescriptionSourceName)
                .Select(p => new SelectListItem { Text = p.PrescriptionSourceName, Value = p.PrescriptionSourceId.ToString() })
                .ToList();
        }

        public static IEnumerable<SelectListItem> GetPrescriptionSourceSelectListExcludingDoctors(Entity.db_HeyDocEntities db)
        {
            var doctorSourceIds = ConstantHelper.DoctorPrescriptionSourceIds;
            return db.PrescriptionSources
                .Where(p => !doctorSourceIds.Contains(p.PrescriptionSourceId))
                .OrderBy(p => p.PrescriptionSourceName)
                .Select(p => new SelectListItem { Text = p.PrescriptionSourceName, Value = p.PrescriptionSourceId.ToString() })
                .ToList();
        }
    }
}