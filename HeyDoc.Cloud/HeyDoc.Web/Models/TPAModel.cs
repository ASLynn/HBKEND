using HeyDoc.Web.Entity;
using System.Collections.Generic;
using System.Linq;

namespace HeyDoc.Web.Models
{
    public class TPAModel
    {
        public int TPAId { get; set; }
        public string TPAName { get; set; }
        public IEnumerable<int> SupplyingPharmacyIds { get; set; }
        public IEnumerable<string> SupplyingPharmacyNames { get; set; }

        public TPAModel() { }

        public TPAModel(ThirdPartyAdministrator entityTpa)
        {
            TPAId = entityTpa.TPAId;
            TPAName = entityTpa.Name;
            SupplyingPharmacyIds = entityTpa.SupplyingPharmacies.Select(e => e.PrescriptionSourceId);
            SupplyingPharmacyNames = entityTpa.SupplyingPharmacies.Select(e => e.PrescriptionSourceName);
        }
    }
}