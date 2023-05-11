
using System.ComponentModel.DataAnnotations;

namespace HeyDoc.Web.Models
{
    public class OTPverifyModel
    {
      
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }
        [Display(Name = "OTP Code")]
        public string OTPCode { get; set; }
        public int CompanyId { get; set; }
    }
}

