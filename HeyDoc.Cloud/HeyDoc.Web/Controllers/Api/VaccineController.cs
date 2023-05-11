using System.Collections.Generic;
using System.Web.Http;
using HeyDoc.Web.WebApi;
using HeyDoc.Web.Services;
using HeyDoc.Web.Models;



using QRCoder;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using static QRCoder.PayloadGenerator;

namespace HeyDoc.Web.Controllers.Api
{
    public class VaccineController : ApiController
    {
        [HttpPost]
        public VaccinatedUserInfoModel SaveVaccinatedUserInfo(VaccinatedUserInfoModel model, string accessToken)
        {
            isAccessTokenValid(accessToken);
            return WebApiWrapper.Call<VaccinatedUserInfoModel>(e => VaccineService.SaveVaccinatedUserInfo(model));
        }
        [HttpPost]
        public VaccinatedUserInfoModel GetVaccinatedUserInfo(VaccinatedUserInfoModel model, string accessToken)
        {
            isAccessTokenValid(accessToken);
            return WebApiWrapper.Call<VaccinatedUserInfoModel>(e => VaccineService.GetVaccinatedUserInfo(model));
        }
        [HttpPost]
        public List<VaccineGeneralModel> GetVaccineGeneralList(string accessToken)
        {
            isAccessTokenValid(accessToken);
            return WebApiWrapper.Call<List<VaccineGeneralModel>>(e => VaccineService.GetVaccineGeneralList());
        }      

        [HttpPost]
        public List<VaccineDetailModel> GetVaccineDetailList(VaccineDetailModel model, string accessToken)
        {
            isAccessTokenValid(accessToken);
            return WebApiWrapper.Call<List<VaccineDetailModel>>(e => VaccineService.GetVaccineDetailList(model.VaccineGeneral_Id));
        }
        [HttpPost]
        public VaccinatedUserModel SaveVaccineInfoForUser(VaccinatedUserModel model, string accessToken)
        {
            isAccessTokenValid(accessToken);
            var request = System.Web.HttpContext.Current.Request;
            return WebApiWrapper.Call<VaccinatedUserModel>(e => VaccineService.SaveVaccineInfoForUser(model, request));
        }
        [HttpPost]
        public List<VaccinatedUserModel> GetVaccineInfoListForUser(VaccinatedUserModel model, string accessToken)
        {
            isAccessTokenValid(accessToken);
            return WebApiWrapper.Call<List<VaccinatedUserModel>>(e => VaccineService.GetVaccineInfoListForUser(model));
        }
        [HttpPost]
        public List<VaccineDetailModel> GetVaccineByGeneralVaccineId(VaccineGeneralModel model, string accessToken)
        {
            isAccessTokenValid(accessToken);
            return WebApiWrapper.Call<List<VaccineDetailModel>>(e => VaccineService.GetVaccineByGeneralVaccineId(model));
        }
        [HttpGet]
        [HttpPost]
        public IHttpActionResult GetDoseInfo(VaccinatedUserModel model, string accessToken)
        {
            isAccessTokenValid(accessToken);
            var item = VaccineService.GetVaccineInfoListForUser(model);
            string strLastDose = "No Dose";
            string strLastStatus = "No Status";
            if (item.Count > 0)
            {
                strLastDose = item[item.Count - 1].Dose;
                strLastStatus = item[item.Count - 1].Status;
                if (strLastDose.Trim().ToLower() == "1st dose" && strLastStatus.Trim().ToLower() == "rejected")
                {
                    strLastDose = "No Dose";
                }
            }

            return Json(new
            {
                LastDose = strLastDose.Trim(),
                LastStatus = strLastStatus.Trim()
            });

        }
        [HttpPost]
        public IHttpActionResult GetDoseInfoWeb(VaccinatedUserModel model, string accessToken)
        {
            isAccessTokenValid(accessToken);
            string strLastDose = "No Dose";
            string strLastStatus = "No Status";          
            var item = VaccineService.GetDoseInfo(model);         
            if (item.Count > 0)
            {
                strLastDose = item[0].Dose;
                strLastStatus = item[0].Status;
            }       
            return Json(new
            {
                LastDose = strLastDose.Trim(),
                LastStatus = strLastStatus.Trim()              
            });

        }
        [HttpPost]
        public IHttpActionResult GetShouldEditUserprofile(VaccinatedUserModel model, string accessToken)
        {
            isAccessTokenValid(accessToken);
            string tempStatus = "No Status";
            bool shouldEdit = true;
            var allvaccine = VaccineService.GetVaccineInfoListForUser(model);

            foreach (var a in allvaccine)
            {
                if (a.Status.ToLower().Trim() == "approved") { shouldEdit = false; }
            }
            var item = VaccineService.GetLastDoseInfo(model);
            if (item.Count > 0)
            {
                tempStatus = item[item.Count - 1].Status;
            }
            if (shouldEdit)
            {
                if (tempStatus.ToLower().Trim() == "pending") { shouldEdit = false; }
            }
            return Json(new
            {
                ShouldEdit = shouldEdit
            });
        }
        private void isAccessTokenValid(string accessToken)
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityUser = AccountService.GetEntityUserByAccessToken(db, accessToken, false);
            }
        }

    }
}
