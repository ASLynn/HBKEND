using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HeyDoc.Web.Models
{
    public class PartnerRegisterModel
    {
        [Required]
        [Display(Name = "User name")]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 4)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        public string Nickname { get; set; }
        public string Outlet { get; set; }
        public string Language { get; set; }

        public Gender Gender { get; set; }

        public Nullable<DateTime> Birthday { get; set; }

        public int? CountryId { get; set; }
         [Required(ErrorMessage="Company Required")]
        public int CompanyId { get; set; }
        [Required(ErrorMessage = "Position Required")]
        public int PositionId { get; set; }
        public string MobileNumber { get; set; }

    }
}