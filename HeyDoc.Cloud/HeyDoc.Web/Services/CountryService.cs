using HeyDoc.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HeyDoc.Web.Services
{
    public class CountryService
    {
        public static List<CountryModel> CountryList()
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityCountryList = from e in db.Countries
                                        where e.IsActive.HasValue && e.IsActive.Value // Filter only active countries, checking HasValue since IsActive is a nullable
                                        orderby e.OrderRank, e.CountryName
                                        select e;

                var result = new List<CountryModel>();

                foreach (var entityCountry in entityCountryList)
                {
                    result.Add(new CountryModel(entityCountry));
                }

                return result;
            }
        }

        public static List<SelectListItem> GetCountryList()
        {
            List<SelectListItem> countryList = new List<SelectListItem>();
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var countries = db.Countries.Where(c => c.IsActive.HasValue && c.IsActive.Value).OrderBy(e => e.CountryName).Select(e => new SelectListItem()
                  {
                      Text = e.CountryName + " ( " + e.CountryCode + " )",
                      Value = e.CountryId.ToString(),
                      Selected = e.CountryCode == "MY"
                  });
                foreach (var country in countries)
                {
                    countryList.Add(country);
                }
            }
            return countryList;
        }
    }
}