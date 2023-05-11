using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace HeyDoc.Web.Models
{
    public class CountryModel
    {
        public int CountryId { get; set; }

        [DisplayName("Country")]
        public string Name { get; set; }
        public string Code { get; set; }

        public CountryModel()
        {

        }

        public CountryModel(Entity.Country entityCountry)
        {
            CountryId = entityCountry.CountryId;
            Name = entityCountry.CountryName;
            Code = entityCountry.CountryCode;
        }
    }
}