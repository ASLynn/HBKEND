using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HeyDoc.Web.Models
{
    public class PharmacyRegisterModel
    {
        [Required]
        public string Name { get; set; }
    }
}