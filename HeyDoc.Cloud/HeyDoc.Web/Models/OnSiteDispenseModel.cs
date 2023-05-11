using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HeyDoc.Web.Models
{
    public class OnSiteDispenseModel
    {
        public int OnSiteId { get; set; }
        public string OnSiteName { get; set; }
        public string OnSiteAddress { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime CreatedDate { get; set; }

        public DateTime? SelectionDateDispense { get; set; }

        public OnSiteDispenseModel()
        {

        }

        public OnSiteDispenseModel(Entity.OnSiteDispens entityOnsite)
        {
            OnSiteId = entityOnsite.OnSiteId;
            OnSiteName = entityOnsite.OnSiteName;
            OnSiteAddress = entityOnsite.OnSiteAddress;
            PhoneNumber = entityOnsite.OnSitePhoneNumber;
            CreatedDate = entityOnsite.CreatedDate;

            var selectionDate = entityOnsite.OnSiteDateSelections.OrderByDescending(e => e.CreatedDate).FirstOrDefault(e => !e.IsDelete);
            if (selectionDate != null)
            {
                if (selectionDate.SelectionDate > DateTime.UtcNow)
                {
                    SelectionDateDispense = selectionDate.SelectionDate;
                }                
            }
        }
    }
}