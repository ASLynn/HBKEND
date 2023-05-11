using HeyDoc.Web.Entity;
using HeyDoc.Web.Models;
using HeyDoc.Web.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HeyDoc.Web.Services
{
    public class SpecialityService
    {
        public static bool DocsSpecialitySave(int specialidyId, int DoctorsId)
        {
            Entity.DoctorSpeciality ds = new Entity.DoctorSpeciality();
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {

                ds.DoctorId = DoctorsId;
                ds.SpecialityId = specialidyId;
                db.DoctorSpecialities.Add(ds);
                db.SaveChanges();
                return true;

            }

        }
        public static List<SpecialityModel> GetSpecialityList()
        {
            List<SpecialityModel> specialityList = new List<SpecialityModel>();
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var tmpRes = from e in db.Specialities
                             orderby e.SpecialityDesc
                             select e;

                foreach (var a in tmpRes)
                {

                    specialityList.Add(new SpecialityModel(a));
                }
            }
            return specialityList;
        }
        public static List<SelectListItem> GetSpecialityAll()
        {
            List<SelectListItem> specialityList = new List<SelectListItem>();
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                specialityList = db.Specialities.Where (a=>a.Status == 1).OrderBy(e => e.SpecialityDesc).Select(e => new SelectListItem()
                              {
                                  Text = e.SpecialityDesc,
                                  Value = e.SpecialityId.ToString()
                              }).ToList();

            }
            return specialityList;

        }
        public static List<SelectListItem> GetSpecialityById(int SpecialtyId)
        {
            List<SelectListItem> specialityList = new List<SelectListItem>();
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                specialityList = db.Specialities.Where(a => (a.Status == 1)&&(a.SpecialityId == SpecialtyId)).OrderBy(e => e.SpecialityDesc).Select(e => new SelectListItem()
                {
                    Text = e.SpecialityDesc,
                    Value = e.SpecialityId.ToString()
                }).ToList();

            }
            return specialityList;

        }

        public static string GetSpecialityDescById(int SpecialtyId)
        {
            string str = "";
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {

                var result = from e in db.Specialities
                                 where e.SpecialityId == SpecialtyId                                 
                                 select e;

                str = result.FirstOrDefault().SpecialityDesc;
            }
            return str;

        }

    }
    public class DoctorSpecialityService
    {
        internal static List<int> GetDoctorSpecialityIDsByDoctorId(string doctorId)
        {

            int docid = Convert.ToInt32(doctorId);
            List<int> flist = new List<int>();
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var tmpRes = from e in db.DoctorSpecialities
                             where e.DoctorId == docid
                             select e;

                foreach (var a in tmpRes)
                {
                    flist.Add(a.SpecialityId);

                }
            }
            return flist;
        }
      
    }


}