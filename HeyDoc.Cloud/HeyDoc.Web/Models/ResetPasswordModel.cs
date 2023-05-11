using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HeyDoc.Web.Models
{
    public class ResetPasswordModel
    {
        public string ResetToken { get; set; }
        [Display(Name = "New Password")]
        public string NewPassword { get; set; }
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; }
    }
}