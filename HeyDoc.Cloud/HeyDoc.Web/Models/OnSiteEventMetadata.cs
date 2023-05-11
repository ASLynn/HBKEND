using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HeyDoc.Web.Entity
{
    public interface IOnSiteEventMetadata
    {
        [Required]
        string Code { get; set; }
        [Required]
        string Description { get; set; }
        [Required]
        int CorporateId { get; set; }
        [Required]
        bool IsActive { get; set; }
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}Z")]
        DateTime CreateDate { get; set; }
    }

    // IOnSiteEventMetadata is simply for applying data annotations to the OnSiteEvent entity
    // If OnSiteEvent is changed in the database, IOnSiteEventMetadata should be updated to reflect those changes
    [MetadataType(typeof(IOnSiteEventMetadata))]
    public partial class OnSiteEvent : IOnSiteEventMetadata, IValidatableObject
    {
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            using (var db = new db_HeyDocEntities())
            {
                if (db.OnSiteEvents.Any(e => e.Code == Code && e.Id != Id))
                {
                    yield return new ValidationResult("An event with this code already exists", new[] { nameof(Code) });
                }
                if (!db.Corporates.Any(e => e.CorporateId == CorporateId && !e.IsDelete))
                {
                    yield return new ValidationResult("Chosen corporate no longer exists in database", new[] { nameof(CorporateId) });
                }
            }
        }
    }
}