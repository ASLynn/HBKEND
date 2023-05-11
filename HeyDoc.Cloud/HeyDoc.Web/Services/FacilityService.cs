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
    public class FacilityService
    {
        public static bool  DocsFacilitiesSave(int facilityId, int DoctorsId)
        {
            Entity.DocFacilityAccessment dfa = new Entity.DocFacilityAccessment();
           

            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {


                dfa.FacilityId = facilityId;
                dfa.DoctorId = DoctorsId;
                dfa.DocFacilityAccessmentStatus = 1;
                
                db.DocFacilityAccessments.Add(dfa);
                db.SaveChanges();
                return true;

            }

        }

        internal static string GetFacilityDescById(int facilityId)
        {
            string str = "";
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {

                var result = from e in db.Facilities
                             where e.FacilityId == facilityId
                             select e;

                str = result.FirstOrDefault().FacilityName;
            }
            return str;
        }

        public static List<SelectListItem> GetFacility()
        {
            List<SelectListItem> facilityList = new List<SelectListItem>();
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var tempLst = from a in db.Facilities 
                                join b in db.FacilityTypes on a.FacilityTypeId equals b.FacilityTypeId   
                                join c in db.States on a.StateId equals c.StateId
                                join d in db.Townships on a.TownshipId equals d.TownshipId
                              where a.FacilityStatus == 1
                              select new
                              {
                                  a.FacilityId,a.FacilityName,a.FacilityStatus,b.FacilityTypeDesc,c.StateDesc,d.TownshipDesc
                              };

          

                facilityList = tempLst.OrderBy(e => e.FacilityName).Select(e => new SelectListItem()
                {
                    Text = e.FacilityName + " | " + e.TownshipDesc + " | " + e.StateDesc,
                    Value = e.FacilityId.ToString()
                }).ToList();




            }
            return facilityList;

        }
       
        public static List<FacilityListModel> GetFacilityAll()
        {
            List<FacilityListModel> facilityListModels = new List<FacilityListModel>();
            FacilityListModel facilityListModel = new FacilityListModel();
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var tempLst = from a in db.Facilities
                              join b in db.FacilityTypes on a.FacilityTypeId equals b.FacilityTypeId
                              join c in db.States on a.StateId equals c.StateId
                              join d in db.Townships on a.TownshipId equals d.TownshipId
                              where a.FacilityStatus == 1
                              select new
                              {
                                  a.FacilityId,
                                  a.FacilityName,
                                  a.FacilityAddress,
                                  a.FacilityPh,
                                  a.FacilityTypeId,
                                  b.FacilityTypeDesc,
                                  a.StateId,
                                  c.StateDesc,
                                  a.TownshipId,
                                  d.TownshipDesc,
                                  a.FacilityStatus
                              };

                foreach(var a in tempLst)
                {
                    facilityListModel = new FacilityListModel();
                    facilityListModel.FacilityId = a.FacilityId;
                    facilityListModel.FacilityName = a.FacilityName;
                    facilityListModel.FacilityAddress = a.FacilityAddress;
                    facilityListModel.FacilityPh = a.FacilityPh;
                    facilityListModel.FacilityTypeId = a.FacilityTypeId;
                    facilityListModel.FacilityType = a.FacilityTypeDesc;
                    facilityListModel.StateId = a.StateId;
                    facilityListModel.State = a.StateDesc;
                    facilityListModel.TownshipId = a.TownshipId;
                    facilityListModel.Township = a.TownshipDesc;
                    facilityListModel.FacilityStatus = a.FacilityStatus;
                    facilityListModels.Add(facilityListModel);
                }




            }
            return facilityListModels;

        }

       

    }

    public class FacilityTypeService
    {
        public static List<SelectListItem> GetFacilityType()
        {
            List<SelectListItem> facilityTypeList = new List<SelectListItem>();
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                facilityTypeList = db.FacilityTypes.Where(c => c.FacilityTypeStatus == 1).OrderBy(e => e.FacilityTypeDesc).Select(e => new SelectListItem()
                {
                    Text = e.FacilityTypeDesc,
                    Value = e.FacilityTypeId.ToString()
                }).ToList();

            }
            return facilityTypeList;
        }
        public static List<SelectListItem> GetFacilityTypeById(int FacilityTypeId)
        {
            List<SelectListItem> facilityTypeList = new List<SelectListItem>();
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                facilityTypeList = db.FacilityTypes.Where(c => c.FacilityTypeStatus == 1 && c.FacilityTypeId == FacilityTypeId).OrderBy(e => e.FacilityTypeDesc).Select(e => new SelectListItem()
                {
                    Text = e.FacilityTypeDesc,
                    Value = e.FacilityTypeId.ToString()
                }).ToList();

            }
            return facilityTypeList;
        }
    }

    public class DocFacilityAccessmentService
    {
        internal static List<int> GetDoctorFacilityIDsByDoctorId(string doctorId)
        {
            int docid = Convert.ToInt32(doctorId);
            List<int> flist = new List<int>();
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var tmpRes = from e in db.DocFacilityAccessments
                             where e.DoctorId == docid
                             select e;

                foreach (var a in tmpRes)
                {
                    flist.Add(a.FacilityId);

                }
            }
            return flist;
        }
    }
    public class StateService
    {
        public static List<StateModel> GetStateList()
        {
            List<StateModel> stateList = new List<StateModel>();
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var tmpRes = from e in db.States
                             where e.StateStatus == 1 // Filter only active countries, checking HasValue since IsActive is a nullable
                             orderby e.StateDescMM
                             select e;

                foreach (var a in tmpRes)
                {
                    stateList.Add(new StateModel (a));

                }
            }
            return stateList;
        }
        public static List<SelectListItem> GetState()
        {
            List<SelectListItem> stateList = new List<SelectListItem>();
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                stateList = db.States.Where(c => c.StateStatus == 1).OrderBy(e => e.StateDesc).Select(e => new SelectListItem()
                {
                    Text = e.StateDesc,
                    Value = e.StateId.ToString(),
                    Selected = e.StateDesc == "Yangon"
                }).ToList();

            }
            return stateList;
        }
        public static List<SelectListItem> GetStateById(int StateId)
        {
            List<SelectListItem> stateList = new List<SelectListItem>();
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                stateList = db.States.Where(c => c.StateStatus == 1 && c.StateId == StateId).OrderBy(e => e.StateDesc).Select(e => new SelectListItem()
                {
                    Text = e.StateDesc,
                    Value = e.StateId.ToString()
                }).ToList();

            }
            return stateList;
        }      
    }

    public class TownshipService
    {
        public static List<TownshipModel> GetTownshipList()
        {
            List<TownshipModel> townshipList = new List<TownshipModel>();
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var tmpRes = from e in db.Townships
                             where e.TownshipStatus == 1 // Filter only active countries, checking HasValue since IsActive is a nullable
                             orderby e.TownshipDescMM
                             select e;

                foreach (var a in tmpRes)
                {
                    townshipList.Add(new TownshipModel(a));

                }
            }
            return townshipList;
        }
        public static List<TownshipModel> GetTownshipByState(int StateId)
        {
            List<TownshipModel> townshipList = new List<TownshipModel>();
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var tmpRes = from e in db.Townships
                             where e.TownshipStatus == 1 && e.StateId == StateId // Filter only active countries, checking HasValue since IsActive is a nullable
                             orderby e.TownshipDescMM
                             select e;

                foreach (var a in tmpRes)
                {
                    townshipList.Add(new TownshipModel(a));

                }
            }
            return townshipList;
        }
        public static List<SelectListItem> GetTownship()
        {
            List<SelectListItem> townshipList = new List<SelectListItem>();
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                 townshipList = db.Townships.Where(c => c.TownshipStatus == 1).OrderBy(e => e.TownshipDesc).Select(e => new SelectListItem()
                {
                    Text = e.TownshipDesc ,
                    Value = e.TownshipId.ToString(),
                   
                }).ToList();
              
            }
            return townshipList;
        }
        public static List<SelectListItem> GetTownshipById(int TownshipId)
        {
            List<SelectListItem> townshipList = new List<SelectListItem>();
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                townshipList = db.Townships.Where(c => c.TownshipStatus == 1 && c.TownshipId == TownshipId).OrderBy(e => e.TownshipDesc).Select(e => new SelectListItem()
                {
                    Text = e.TownshipDesc,
                    Value = e.TownshipId.ToString()
                }).ToList();

            }
            return townshipList;
        }
        public static List<SelectListItem> GetTownshipByStateId(int StateId)
        {
            List<SelectListItem> townshipList = new List<SelectListItem>();
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                townshipList = db.Townships.Where(c => c.TownshipStatus == 1 && c.StateId == StateId).OrderBy(e => e.TownshipDesc).Select(e => new SelectListItem()
                {
                    Text = e.TownshipDesc,
                    Value = e.TownshipId.ToString()

                }).ToList();

            }
            return townshipList;
        }
    }

    public class CertificateService
    {
        internal static List<CertiModel> GetDoctorCertificateByDoctorId(string doctorId)
        {
            int docid = Convert.ToInt32(doctorId);
            List<CertiModel> flist = new List<CertiModel>();
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var tmpRes = from e in db.Certis
                             where e.DoctorId == docid
                             select e;

                foreach (var a in tmpRes)
                {
                    CertiModel cm = new CertiModel();
                    cm.DoctorId = a.DoctorId;
                    cm.CertiUrl = a.CertiUrl;
                    cm.CertiStatus = a.CertiStatus;
                    flist.Add(cm);
                }
            }
            return flist;
        }
    }

    //public class TitleService
    //{
    //    public static List<TitleModel> GetTitleList()
    //    {
    //        List<TitleModel> specialityList = new List<TitleModel>();
    //        using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
    //        {
    //            var tmpRes = from e in db.Title
    //                         orderby e.TitleId
    //                         select e;

    //            foreach (var a in tmpRes)
    //            {

    //                specialityList.Add(new TitleModel(a));
    //            }
    //        }
    //        return specialityList;
    //    }
    //}
}