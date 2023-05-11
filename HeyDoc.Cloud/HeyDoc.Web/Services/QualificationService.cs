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
    public class QualificationService
    {
      

        public static List<QualificationModel> GetQualificationList()
        {
            List<QualificationModel> qualificationModels = new List<QualificationModel>();
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var tmpRes = from e in db.Qualifications
                             orderby e.QualificationDesc
                             select e;

                foreach (var a in tmpRes)
                {

                    qualificationModels.Add(new QualificationModel(a));
                }
            }
            return qualificationModels;
        }
        public static List<SelectListItem> GetqualificationAll()
        {
            List<SelectListItem> listItems = new List<SelectListItem>();
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                listItems = db.Qualifications.Where(a => (a.Status == 1) ).OrderBy(e => e.QualificationDesc).Select(e => new SelectListItem()
                {
                    Text = e.QualificationDesc,
                    Value = e.QualificationId.ToString()
                }).ToList();

            }
            return listItems;

        }
        public static List<SelectListItem> GetqualificationById(int QualificationId)
        {
            List<SelectListItem> listItems = new List<SelectListItem>();
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                listItems = db.Qualifications.Where(a => (a.Status == 1)&&(a.QualificationId == QualificationId)).OrderBy(e => e.QualificationDesc).Select(e => new SelectListItem()
                {
                    Text = e.QualificationDesc,
                    Value = e.QualificationId.ToString()
                }).ToList();

            }
            return listItems;

        }

        internal static List<int> GetDoctorQualificationIDsByDoctorId(string doctorId)
        {
            int docid = Convert.ToInt32(doctorId);
            List<int> flist = new List<int>();
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var tmpRes = from e in db.DoctorQualifications
                             where e.DoctorId == docid
                             select e;

                foreach (var a in tmpRes)
                {
                    flist.Add(a.QualificationId);

                }
            }
            return flist;
        }
        public static bool DocsQulificationSave(int qualificationId, int DoctorsId)
        {
            Entity.DoctorQualification dq = new Entity.DoctorQualification();
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {

                dq.DoctorId = DoctorsId;
                dq.QualificationId = qualificationId;
                db.DoctorQualifications.Add(dq);
                db.SaveChanges();
                return true;

            }

        }

        internal static string GetQualificationDescById(int qualificationId)
        {
            string str = "";
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {

                var result = from e in db.Qualifications
                             where e.QualificationId == qualificationId
                             select e;

                str = result.FirstOrDefault().QualificationDesc;
            }
            return str;

        }
    }


}