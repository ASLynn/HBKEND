using HeyDoc.Web.Helpers;
using HeyDoc.Web.Lib;
using HeyDoc.Web.Models;
using HeyDoc.Web.Services;
using HeyDoc.Web.WebApi;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Net;
using HeyDoc.Web.Resources;
using System.Drawing;
using System.Data.Entity.Validation;

namespace HeyDoc.Web.Controllers
{
    [Authorize(Roles = "Admin,SuperAdmin,Doctor")]
    public class DoctorController : BaseController
    {
        private class validate
        {
            public Boolean valid;
        }
        [AllowAnonymous]
        public JsonResult ValidateEmail(string email)
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityUser = db.UserProfiles.FirstOrDefault(e => e.UserName == email && !e.IsDelete);
                var result = new validate();
                result.valid = entityUser != null ? false : true;
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
        [AllowAnonymous]
        public JsonResult ValidatePhone(string phone)
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityUser = db.UserProfiles.FirstOrDefault(e => e.PhoneNumber == phone && !e.IsDelete);
                var result = new validate();
                result.valid = entityUser != null ? false : true;
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
        [AllowAnonymous]
        public JsonResult ValidateEmailForEdit(string email, int userid)
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityUser = db.UserProfiles.FirstOrDefault(e => e.UserName == email && e.UserId != userid && !e.IsDelete);
                var result = new validate();
                result.valid = entityUser != null ? false : true;
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
        [AllowAnonymous]
        public JsonResult ValidatePhoneForEdit(string phone,int userid)
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityUser = db.UserProfiles.FirstOrDefault(e => e.PhoneNumber == phone && e.UserId != userid && !e.IsDelete);
                var result = new validate();
                result.valid = entityUser != null ? false : true;
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult Index()
        {
            var categories = new List<SelectListItem>();
            categories.Add(new SelectListItem() { Selected = true, Text = "All", Value = "0" });
            categories.AddRange(TeamService.GetCategoryList());
            var companies = new List<SelectListItem>();
            companies.AddRange(CompanyService.GetCompaniesSelectList());
            ViewBag.Categories = categories;
            ViewBag.Companies = companies;
            return View();
        }    

        [HttpPost]
        public JsonResult GetList(int draw, int length, int start, List<DataTableOrderOptions> order, List<DataTableColumnProps> columns, int groupId, string doctorType, string category, int companyId = 1, string searchKey = "")
        {
            int recordsTotal, recordsFiltered;
            List<DoctorModel> data = new List<DoctorModel>();
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                int categoryId = 0;
                if (!string.IsNullOrEmpty(category))
                {
                    categoryId = Convert.ToInt32(category);
                }

                var entityUserList = db.UserProfiles
                    .Where(
                    e => e.CompanyId == companyId &&
                    !e.IsDelete &&
                    e.webpages_Roles.Any(f => f.RoleName == RoleType.Doctor.ToString())
                    &&
                    (e.FullName.Contains(searchKey) || e.UserName.Contains(searchKey))
                    );
                recordsTotal = entityUserList.Select(e => e.UserId).Count();
                var filtersApplied = false;

                switch (doctorType)
                {
                    case "verified":
                        entityUserList = entityUserList.Where(e => e.Doctor.IsVerified);
                        filtersApplied = true;
                        break;
                    case "unverified":
                        entityUserList = entityUserList.Where(e => !e.Doctor.IsVerified);
                        filtersApplied = true;
                        break;
                }

                if (categoryId != 0 && categoryId > 0)
                {
                    entityUserList = entityUserList.Where(e => e.Doctor.CategoryId == categoryId);
                    filtersApplied = true;
                    if (groupId > 0)
                    {
                        entityUserList = entityUserList.Where(e => e.Doctor.GroupId == groupId);
                    }
                    else if (groupId == 0)
                    {
                        entityUserList = entityUserList.Where(e => e.Doctor.GroupId == null);
                    }
                }
                recordsFiltered = filtersApplied ? entityUserList.Select(e => e.UserId).Count() : recordsTotal;

                if (order.Count > 0)
                {
                    var firstOrdering = true;
                    foreach (var o in order)
                    {
                        var sortProperty = columns[o.column].name;
                        bool descendingOrder;
                        switch (o.dir)
                        {
                            case "asc":
                                descendingOrder = false;
                                break;
                            case "desc":
                                descendingOrder = true;
                                break;
                            default:
                                return Json(new
                                {
                                    error = $"Invalid ordering direction: '{o.dir}' for property: '{sortProperty}'"
                                });
                        }

                        switch (sortProperty)
                        {
                            case "FullName":
                                entityUserList = entityUserList.DynamicOrderBy(e => e.FullName, descendingOrder, firstOrdering);
                                firstOrdering = false;
                                break;
                            case "CreateDate":
                                entityUserList = entityUserList.DynamicOrderBy(e => e.CreateDate, descendingOrder, firstOrdering);
                                firstOrdering = false;
                                break;
                            default:
                                return Json(new
                                {
                                    error = $"Invalid sort property: '{sortProperty}'"
                                });
                        }
                    }
                }
                else
                {
                    // Default ordering if the request from DataTables didn't specify ordering
                    entityUserList = entityUserList.OrderByDescending(e => e.CreateDate);
                }

                entityUserList = entityUserList.Skip(start).Take(length);
                entityUserList = entityUserList
                    .Include("Doctor")
                    .Include("Doctor.Category")
                    .Include("Doctor.Group")
                    .Include("Photo")
                    .Include("Country")
                    .Include("webpages_Membership")
                    .Include("webpages_Roles");

                foreach (var entityUser in entityUserList)
                {
                    var docObj = new DoctorModel(entityUser, entityUser.Doctor);
                    docObj.NumberOfCancelRequest = ChatService.GetNumberOfRequestCancelled(db, docObj.UserId);
                    data.Add(docObj);
                }
            }

            return Json(new
            {
                recordsTotal = recordsTotal,
                recordsFiltered = recordsFiltered,
                data = data,
            });
        }

       

        // GET: /Doctor/Create
        public ActionResult Create()
        {
            RegisterModel model = new RegisterModel();
            var countries = CountryService.GetCountryList();
            var selectedCountry = countries.FirstOrDefault(e => e.Selected);
            model.CountryId = selectedCountry != null ? int.Parse(selectedCountry.Value) : 0;
            ViewBag.Countries = countries;
            ViewBag.Categories = TeamService.GetCategoryList();
            ViewBag.FacilityList = FacilityService.GetFacility();
          

            var state = StateService.GetState();
            var selectedState = state.FirstOrDefault(e => e.Selected);
            model.StateId = selectedState != null ? int.Parse(selectedState.Value) : 0;
            ViewBag.State = state;

            var township = TownshipService.GetTownshipByStateId(model.StateId);
            var selectedTownship = state.FirstOrDefault(e => e.Selected);
            model.TownshipId = selectedTownship != null ? int.Parse(selectedTownship.Value) : 0;
            ViewBag.Township = township;

            ViewBag.SpecilityList = SpecialityService.GetSpecialityAll();
            ViewBag.QualificationList = QualificationService.GetqualificationAll();

            return View(model);
        }
        [HttpGet]
        [AllowAnonymous]
        public JsonResult GetTownship(int stateId)
        {            
            var township = TownshipService.GetTownshipByStateId(stateId);
            return Json(township, JsonRequestBehavior.AllowGet);
        }
        //[HttpPost]
        //[AllowAnonymous]
        //public ActionResult SignUp(RegisterModel model, HttpPostedFileBase imageFile, HttpPostedFileBase signatureFile)
        //{
        //    bool isSuccess = false;
        //    (var pass, var errorMessage) = PasswordModel.CheckPassword(model.Password);
        //    if (!pass)
        //    {
        //        ModelState.AddModelError("Password", errorMessage);
        //    }
        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            if (signatureFile == null)
        //            {
        //                ModelState.AddModelError("signatureFile", "Signature is missing");
        //                ViewBag.Countries = CountryService.GetCountryList();
        //                ViewBag.Categories = TeamService.GetCategoryList();
        //                ViewBag.FacilityList = FacilityService.GetFacility();
        //                return View(model);
        //            }
        //            UserModel result = AccountService.CreateUser(model, RoleType.Doctor, false, signatureFile);
        //            if (result == null)
        //            {
        //                ModelState.AddModelError("", "Duplicate Email!");
        //                return View(model);
        //            }
        //            if (imageFile != null)
        //            {
        //                var photo = CreateOrUpdateImage(result.UserId, imageFile.InputStream);
        //            }

        //            if (User.Identity.IsAuthenticated)
        //            {
        //                return RedirectToAction("Index");
        //            }
        //            else
        //            {
        //                isSuccess = true;
        //            }

        //        }
        //        catch(WebApiException ex)
        //        {
        //            ModelState.AddModelError("", ex.ErrorDetails.ErrorMessage);
        //        }
        //        catch (Exception e)
        //        {
        //            ModelState.AddModelError("", "Server Error!");
        //        }
        //    }
        //    ViewBag.Countries = CountryService.GetCountryList();
        //    ViewBag.Categories = TeamService.GetCategoryList();
        //    ViewBag.FacilityList = FacilityService.GetFacility();
        //    if (isSuccess)
        //    {
        //        ViewBag.IsSuccess = true;
        //        return View("SignUp", model);
        //    }

        //    return View(model);
        //}

        [HttpPost]
        [AllowAnonymous]
        public ActionResult SignUp(RegisterModel model, HttpPostedFileBase imageFile, HttpPostedFileBase certificateFile1, HttpPostedFileBase certificateFile2, HttpPostedFileBase certificateFile3, HttpPostedFileBase certificateFile4, HttpPostedFileBase certificateFile5, HttpPostedFileBase certificateFile6, HttpPostedFileBase certificateFile7, HttpPostedFileBase signatureFile)
        {
            var countries = CountryService.GetCountryList();
            //var selectedCountry = countries.FirstOrDefault(e => e.Selected);          
            ViewBag.Countries = countries;
            ViewBag.SpecilityList = SpecialityService.GetSpecialityAll();
            ViewBag.QualificationList = QualificationService.GetqualificationAll();
            ViewBag.FacilityList = FacilityService.GetFacility();
            ViewBag.Categories = TeamService.GetCategoryList();
            ViewBag.State = StateService.GetState();
            ViewBag.Township = TownshipService.GetTownship();

            model.createUserisAdmin = false;

            if (model.GroupId.HasValue)
            {
                if (!model.CategoryId.HasValue)
                {
                    ModelState.AddModelError("GroupId", "Cannot have group with no category");
                }
                if (model.GroupId.Value == 0)
                {
                    model.GroupId = null;
                }
                else
                {
                    using (var db = new Entity.db_HeyDocEntities())
                    {
                        if (!db.Groups.Where(e => e.CategoryId == model.CategoryId.Value).Select(e => e.GroupId).Contains(model.GroupId.Value))
                        {
                            ModelState.AddModelError("GroupId", "Selected group is not in selected category");
                        }
                    }
                }
            }
            (var pass, var errorMessage) = PasswordModel.CheckPassword(model.Password);
            if (!pass)
            {
                ModelState.AddModelError("Password", errorMessage);
            }
            if (ModelState.IsValid)
            {
                try
                {

                    if (signatureFile == null)
                    {
                        ModelState.AddModelError("signatureFile", "Signature is missing");

                        return View(model);
                    }
                    //if (certiList == null)
                    //{
                    //    ModelState.AddModelError("", "At least One Certificate Need!");
                    //    return View(model);
                    //}
                    if (model.FacilitiesId == null)
                    {
                        ModelState.AddModelError("", "At least One Hospital or Clinic");
                        return View(model);
                    }
                    if (model.SpecialityId == null)
                    {
                        ModelState.AddModelError("", "At least One Speciality");
                        return View(model);
                    }
                    if (model.QualificationId == null)
                    {
                        ModelState.AddModelError("", "At least One Qulifaction");
                        return View(model);
                    }


                    if (string.IsNullOrEmpty(model.Email))
                    {
                        model.Email = model.PhoneNumber + "." + DateTime.UtcNow.Ticks + "@hope.com";                        
                    }
                    // Everything is ok Saving to DB
                    UserModel result = AccountService.CreateUser(model, RoleType.Doctor, false, signatureFile);
                    if (result == null)
                    {
                        ModelState.AddModelError("", "Duplicate Email!");
                        return View(model);
                    }

                    if (imageFile != null)
                    {
                        var photo = CreateOrUpdateImage(result.UserId, imageFile.InputStream);
                    }

                 

                    if (certificateFile1 != null)
                    {
                        
                        CreateOrUpdateCertificate(result.UserId, certificateFile1.InputStream, certificateFile1.FileName,1);
                    }
                    if (certificateFile2 != null)
                    {
                        CreateOrUpdateCertificate(result.UserId, certificateFile2.InputStream, certificateFile2.FileName, 2);
                    }
                    if (certificateFile3 != null)
                    {
                        CreateOrUpdateCertificate(result.UserId, certificateFile3.InputStream, certificateFile3.FileName, 3);
                    }
                    if (certificateFile4 != null)
                    {
                        CreateOrUpdateCertificate(result.UserId, certificateFile4.InputStream, certificateFile4.FileName, 4);
                    }
                    if (certificateFile5 != null)
                    {
                        CreateOrUpdateCertificate(result.UserId, certificateFile5.InputStream, certificateFile5.FileName, 5);
                    }
                    if (certificateFile6 != null)
                    {
                        CreateOrUpdateCertificate(result.UserId, certificateFile6.InputStream, certificateFile6.FileName, 6);
                    }
                    if (certificateFile7 != null)
                    {
                        CreateOrUpdateCertificate(result.UserId, certificateFile7.InputStream, certificateFile7.FileName, 7);
                    }

               
                    var resTemp1 = false;
                    foreach (var a in model.FacilitiesId)
                    {
                        resTemp1 = FacilityService.DocsFacilitiesSave(Convert.ToInt32(a.ToString()), result.UserId);
                    }
                    var resTemp2 = false;
                    foreach (var a in model.SpecialityId)
                    {
                        resTemp2 = SpecialityService.DocsSpecialitySave(Convert.ToInt32(a.ToString()), result.UserId);
                    }
                    var resTemp3 = false;
                    foreach (var a in model.QualificationId)
                    {
                        resTemp3 = QualificationService.DocsQulificationSave(Convert.ToInt32(a.ToString()), result.UserId);
                    }

                    if (User.Identity.IsAuthenticated)
                    {
                        return RedirectToAction("Index");
                    }
                    ViewBag.UserId = result.UserId;
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("", "Server Error!");
                }
            }



            ViewBag.IsSuccess = true;
           
            return View("SignUp", model);



        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult Create(RegisterModel model, HttpPostedFileBase imageFile, HttpPostedFileBase certificateFile1, HttpPostedFileBase certificateFile2, HttpPostedFileBase certificateFile3, HttpPostedFileBase certificateFile4, HttpPostedFileBase certificateFile5, HttpPostedFileBase certificateFile6, HttpPostedFileBase certificateFile7, HttpPostedFileBase signatureFile)
        {
            var countries = CountryService.GetCountryList();
            //var selectedCountry = countries.FirstOrDefault(e => e.Selected);          
            ViewBag.Countries = countries;
            ViewBag.SpecilityList = SpecialityService.GetSpecialityAll();
            ViewBag.QualificationList = QualificationService.GetqualificationAll();
            ViewBag.FacilityList = FacilityService.GetFacility();
            ViewBag.Categories = TeamService.GetCategoryList();
            ViewBag.State = StateService.GetState();
            ViewBag.Township = TownshipService.GetTownship();

            model.createUserisAdmin = true;
            if (string.IsNullOrEmpty(model.Email))
            {
                model.Email = model.PhoneNumber + "." + DateTime.UtcNow.Ticks + "@hope.com";
            }

            if (model.GroupId.HasValue)
            {
                if (!model.CategoryId.HasValue)
                {
                    ModelState.AddModelError("GroupId", "Cannot have group with no category");
                }
                if (model.GroupId.Value == 0)
                {
                    model.GroupId = null;
                }
                else
                {
                    using (var db = new Entity.db_HeyDocEntities())
                    {
                        if (!db.Groups.Where(e => e.CategoryId == model.CategoryId.Value).Select(e => e.GroupId).Contains(model.GroupId.Value))
                        {
                            ModelState.AddModelError("GroupId", "Selected group is not in selected category");
                        }
                    }
                }
            }
            (var pass, var errorMessage) = PasswordModel.CheckPassword(model.Password);
            if (!pass)
            {
                ModelState.AddModelError("Password", errorMessage);
            }
            if (ModelState.IsValid)
            {
                try
                {

                    //if (signatureFile == null)
                    //{
                    //    ModelState.AddModelError("signatureFile", "Signature is missing");

                    //    return View(model);
                    //}
                    //if (certiList == null)
                    //{
                    //    ModelState.AddModelError("", "At least One Certificate Need!");
                    //    return View(model);
                    //}
                    if (model.FacilitiesId == null)
                    {
                        ModelState.AddModelError("", "At least One Hospital or Clinic");
                        return View(model);
                    }
                    if (model.SpecialityId == null)
                    {
                        ModelState.AddModelError("", "At least One Speciality");
                        return View(model);
                    }
                    if (model.QualificationId == null)
                    {
                        ModelState.AddModelError("", "At least One Qulifaction");
                        return View(model);
                    }



                    // Everything is ok Saving to DB
                    UserModel result = AccountService.CreateUser(model, RoleType.Doctor,false,signatureFile);
                    if (result == null)
                    {
                        ModelState.AddModelError("", "Duplicate Email!");
                        return View(model);
                    }
                    if (imageFile != null)
                    {
                        var photo = CreateOrUpdateImage(result.UserId, imageFile.InputStream);
                    }
                    if (certificateFile1 != null)
                    {
                        CreateOrUpdateCertificate(result.UserId, certificateFile1.InputStream, Path.GetExtension(certificateFile1.FileName), 1);
                    }
                    if (certificateFile2 != null)
                    {
                        CreateOrUpdateCertificate(result.UserId, certificateFile2.InputStream, Path.GetExtension(certificateFile2.FileName), 2);
                    }
                    if (certificateFile3 != null)
                    {
                        CreateOrUpdateCertificate(result.UserId, certificateFile3.InputStream, Path.GetExtension(certificateFile3.FileName), 3);
                    }
                    if (certificateFile4 != null)
                    {
                        CreateOrUpdateCertificate(result.UserId, certificateFile4.InputStream, Path.GetExtension(certificateFile4.FileName), 4);
                    }
                    if (certificateFile5 != null)
                    {
                        CreateOrUpdateCertificate(result.UserId, certificateFile5.InputStream, Path.GetExtension(certificateFile5.FileName), 5);
                    }
                    if (certificateFile6 != null)
                    {
                        CreateOrUpdateCertificate(result.UserId, certificateFile6.InputStream, Path.GetExtension(certificateFile6.FileName), 6);
                    }
                    if (certificateFile7 != null)
                    {
                        CreateOrUpdateCertificate(result.UserId, certificateFile7.InputStream, Path.GetExtension(certificateFile7.FileName), 7);
                    }
                   

                    var resTemp1 = false;
                    foreach (var a in model.FacilitiesId)
                    {
                        resTemp1 = FacilityService.DocsFacilitiesSave(Convert.ToInt32(a.ToString()), result.UserId);
                    }
                    var resTemp2 = false;
                    foreach (var a in model.SpecialityId)
                    {
                        resTemp2 = SpecialityService.DocsSpecialitySave(Convert.ToInt32(a.ToString()), result.UserId);
                    }
                    var resTemp3 = false;
                    foreach (var a in model.QualificationId)
                    {
                        resTemp3 = QualificationService.DocsQulificationSave(Convert.ToInt32(a.ToString()), result.UserId);
                    }

                    if (User.Identity.IsAuthenticated)
                    {
                        return RedirectToAction("Index");
                    }
                   
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("", "Server Error!");
                }
            }


           
            ViewBag.IsSuccess = true;
            return View("SignUp", model);



        }

        [HttpPost]
        [AllowAnonymous]
        public UserModel CreateDoctorAPI(RegisterModel model, HttpRequest request)
        {
            
            UserModel responseErrorModel = new UserModel();
            responseErrorModel.ErrorMessage = "";
            model.createUserisAdmin = false;
            //Making email optional field here
            string username;
            string phone;
            username = model.Email.Trim();
            phone = model.PhoneNumber.Trim();
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {

                var entityUser = AccountService.GetEntityTargetUserByPhoneNumber(db, phone, true, model);
                var entityUserEmail = AccountService.GetEntityTargetUserByUsername(db, username, true, model);
                if (entityUser != null || entityUserEmail != null)
                {

                    responseErrorModel.ErrorMessage += "Error, Email or Phone Number is already registered";
                    return responseErrorModel;
                }
            }
          
            try
            {
               

                HttpPostedFileBase signatureFile = null;
                HttpPostedFileBase profileImageFile = null;

                if (request.Files["signatureFile"] != null)
                {
                    signatureFile = new HttpPostedFileWrapper(request.Files["signatureFile"]);                   
                }

                if (request.Files["profileImage"] != null)
                {
                    profileImageFile = new HttpPostedFileWrapper(request.Files["profileImage"]);                
                }
                if (request.Form["FacilitiesId"] != null)
                {
                    model.FacilitiesId = request.Form["FacilitiesId"].Split(',').Select(item => int.Parse(item));
                }
                if (request.Form["SpecialityId"] != null)
                {
                    model.SpecialityId = request.Form["SpecialityId"].Split(',').Select(item => int.Parse(item));
                }
                if (request.Form["QualificationId"] != null)
                {
                    model.QualificationId = request.Form["QualificationId"].Split(',').Select(item => int.Parse(item));
                }

                if (request.Form["Gender"] != null)
                {
                    if(request.Form["Gender"].ToString() == "1")
                    {
                        model.Gender = Gender.Male;
                    }
                    else
                    {
                        model.Gender = Gender.Female;
                    }
                }


                (var pass, var errorMessage) = PasswordModel.CheckPassword(model.Password);
                if (!pass)
                {
                    responseErrorModel.ErrorMessage += "Error, Password is too weak, password lenght must be atleast 6";                    
                }
                try
                {   
                    if (model.SpecialityId == null)
                    {
                        responseErrorModel.ErrorMessage += "| Error, at least one Speciality ID must include";
                    }
                    if (model.QualificationId == null)
                    {
                        responseErrorModel.ErrorMessage += "| Error, at least one Qualification ID must include";
                    }
                    if(responseErrorModel.ErrorMessage.Length > 0)
                    {
                        return responseErrorModel;
                    }

                  
                    
                    if (username.Length == 0)
                    {
                        username = phone + "." + DateTime.UtcNow.Ticks + "@hope.com";
                        model.Email = username;
                    }
                    // Everything is ok Saving to DB
                    UserModel result = AccountService.CreateUser(model, RoleType.Doctor, false, signatureFile);
                    if (result == null)
                    {
                        responseErrorModel.ErrorMessage += "| Error, Email or Phone is already registered.";
                        return responseErrorModel;
                    }

                    if (profileImageFile != null)
                    {
                        var photo = CreateOrUpdateImage(result.UserId, profileImageFile.InputStream);
                    }

                   
                    if (request.Files["cert1_mbbs"] != null)
                    {
                        HttpPostedFileBase cert1 = new HttpPostedFileWrapper(request.Files["cert1_mbbs"]);
                        CreateOrUpdateCertificate(result.UserId, cert1.InputStream, Path.GetExtension(cert1.FileName), 1);
                    }
                    if (request.Files["cert2_mmedsc"] != null)
                    {
                        HttpPostedFileBase cert2 = new HttpPostedFileWrapper(request.Files["cert2_mmedsc"]);
                        CreateOrUpdateCertificate(result.UserId, cert2.InputStream, Path.GetExtension(cert2.FileName), 2);
                    }
                    if (request.Files["cert3_drmedsc"] != null)
                    {
                        HttpPostedFileBase cert3 = new HttpPostedFileWrapper(request.Files["cert3_drmedsc"]);
                        CreateOrUpdateCertificate(result.UserId, cert3.InputStream, Path.GetExtension(cert3.FileName), 3);
                    }
                    if (request.Files["cert4_sama"] != null)
                    {
                        HttpPostedFileBase cert4 = new HttpPostedFileWrapper(request.Files["cert4_sama"]);
                        CreateOrUpdateCertificate(result.UserId, cert4.InputStream, Path.GetExtension(cert4.FileName), 4);
                    }
                    if (request.Files["cert5_phd"] != null)
                    {
                        HttpPostedFileBase cert5 = new HttpPostedFileWrapper(request.Files["cert5_phd"]);
                        CreateOrUpdateCertificate(result.UserId, cert5.InputStream, Path.GetExtension(cert5.FileName), 5);
                    }
                    if (request.Files["cert6_dfamilymed"] != null)
                    {
                        HttpPostedFileBase cert6 = new HttpPostedFileWrapper(request.Files["cert6_dfamilymed"]);
                        CreateOrUpdateCertificate(result.UserId, cert6.InputStream, Path.GetExtension(cert6.FileName), 6);
                    }
                    if (request.Files["cert7_other"] != null)
                    {
                        HttpPostedFileBase cert7 = new HttpPostedFileWrapper(request.Files["cert7_other"]);
                        CreateOrUpdateCertificate(result.UserId, cert7.InputStream, Path.GetExtension(cert7.FileName), 7);
                    }
                   

                    var resTemp1 = false;
                    if (model.FacilitiesId != null)
                    {
                        foreach (var a in model.FacilitiesId)
                        {
                            resTemp1 = FacilityService.DocsFacilitiesSave(Convert.ToInt32(a.ToString()), result.UserId);
                        }
                    }
                    var resTemp2 = false;
                    foreach (var a in model.SpecialityId)
                    {
                        resTemp2 = SpecialityService.DocsSpecialitySave(Convert.ToInt32(a.ToString()), result.UserId);
                    }
                    var resTemp3 = false;
                    foreach (var a in model.QualificationId)
                    {
                        resTemp3 = QualificationService.DocsQulificationSave(Convert.ToInt32(a.ToString()), result.UserId);
                    }
                    return result;
                }
                catch (Exception e)
                {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidArguments,e.Message));
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
       

        public ActionResult Edit(string doctorId)
        {
            if(User.IsInRole("Doctor"))
            {
                string ownDocId = WebMatrix.WebData.WebSecurity.GetUserId(User.Identity.Name).ToString();
                if (doctorId != ownDocId)
                {
                    doctorId = ownDocId;
                }
            }
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
              
                Entity.UserProfile entityUser = AccountService.GetEntityUserByUserId(db, doctorId, false, true);
                DoctorUpdateModel model = new DoctorUpdateModel(entityUser);
                model.FacilitiesId = DocFacilityAccessmentService.GetDoctorFacilityIDsByDoctorId(doctorId);
                model.SpecialityId = DoctorSpecialityService.GetDoctorSpecialityIDsByDoctorId(doctorId);
                model.QualificationId = QualificationService.GetDoctorQualificationIDsByDoctorId(doctorId);
                model.StateId = entityUser.StateId;
                model.TownshipId = entityUser.TownshipId;
                model.certiModels = CertificateService.GetDoctorCertificateByDoctorId(doctorId);
                ViewBag.Categories = TeamService.GetCategoryList();
                ViewBag.Countries = CountryService.GetCountryList();
                ViewBag.SpecilityList = SpecialityService.GetSpecialityAll();
                ViewBag.QualificationList = QualificationService.GetqualificationAll();
                ViewBag.FacilityList = FacilityService.GetFacility();
              


               
                ViewBag.State = StateService.GetState();
                ViewBag.Township = TownshipService.GetTownshipByStateId(model.StateId.GetValueOrDefault());

                return View(model);
            }
        }

        [AllowAnonymous]
        public ActionResult SignUp()
        {
            if (User.IsInRole("Admin") || User.IsInRole("SuperAdmin"))
            {
                return RedirectToAction("Create","Doctor");
            }
            RegisterModel model = new RegisterModel();
            var countries = CountryService.GetCountryList();
            var selectedCountry = countries.FirstOrDefault(e => e.Selected);
            model.CountryId = selectedCountry != null ? int.Parse(selectedCountry.Value) : 0;
            ViewBag.Countries = countries;
            ViewBag.Categories = TeamService.GetCategoryList();
            ViewBag.FacilityList = FacilityService.GetFacility();


            var state = StateService.GetState();
            var selectedState = state.FirstOrDefault(e => e.Selected);
            model.StateId = selectedState != null ? int.Parse(selectedState.Value) : 0;
            ViewBag.State = state;

            var township = TownshipService.GetTownshipByStateId(model.StateId);
            var selectedTownship = state.FirstOrDefault(e => e.Selected);
            model.TownshipId = selectedTownship != null ? int.Parse(selectedTownship.Value) : 0;
            ViewBag.Township = township;

            ViewBag.SpecilityList = SpecialityService.GetSpecialityAll();
            ViewBag.QualificationList = QualificationService.GetqualificationAll();

            return View(model);

           
        }


        [HttpPost]
        public ActionResult Edit(DoctorUpdateModel model, HttpPostedFileBase imageFile, HttpPostedFileBase certificateFile, HttpPostedFileBase signatureFile)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    Entity.UserProfile entityUser;
                    using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
                    {
                        if (!string.IsNullOrEmpty(model.IC))
                        {
                            model.IC = model.IC.Replace("-", "");
                        }
                        DateTime now = DateTime.UtcNow;
                        entityUser = AccountService.GetEntityUserByUserId(db, model.DoctorId.ToString(), false, true);
                        entityUser.Birthday = model.Birthday;
                        entityUser.Gender = model.Gender;
                        entityUser.CountryId = model.CountryId;
                        entityUser.Nickname = model.Nickname;
                        entityUser.LastUpdateDate = now;
                        //entityUser.LastActivityDate = now;
                        entityUser.Language = model.Language;
                        entityUser.FullName = model.FullName;
                        entityUser.Title = model.Title;
                        entityUser.IC = model.IC;
                        entityUser.BloodType = model.BloodType;
                        entityUser.StateId = model.StateId;
                        entityUser.TownshipId = model.TownshipId;
                        entityUser.Doctor.Specialty = "";
                        entityUser.Doctor.Practicing = model.Practicing;
                        entityUser.Doctor.MedicalSch = model.MedicalSch;
                        entityUser.Doctor.AboutMe = model.AboutMe;
                        entityUser.Doctor.IsPartner = model.IsPartner;
                        entityUser.Doctor.CategoryId = model.CategoryId;
                        entityUser.Doctor.Qualifications = model.Qualification;
                        entityUser.Doctor.RegisterNumber = model.RegisterNumber;
                        entityUser.Doctor.HospitalName = model.HospitalName;
                        entityUser.Doctor.ShowInApp = model.ShowInApp;
                        entityUser.Doctor.IsDigitalSignatureEnabled = model.IsDigitalSignatureEnabled;
                        entityUser.Doctor.IsChatBotEnabled = model.IsChatBotEnabled;
                        entityUser.Doctor.VideoChatUrl = string.IsNullOrWhiteSpace(model.VideoChatURL) ? null : model.VideoChatURL;
                        entityUser.Address = model.Address;
                        entityUser.PhoneNumber = model.PhoneNumber;
                        entityUser.Doctor.CanApproveEPS = model.CanApproveEPS;
                        if (model.GroupId == 0)
                        {
                            entityUser.Doctor.GroupId = null;
                        }
                        else
                        {
                            entityUser.Doctor.GroupId = model.GroupId;
                        }

                        var ToRemoveDSP = db.DoctorSpecialities.Where(rec => rec.DoctorId == model.DoctorId);
                        db.DoctorSpecialities.RemoveRange(ToRemoveDSP);
                        if (model.SpecialityId != null)
                        {
                            foreach (var item in model.SpecialityId)
                            {
                                Entity.DoctorSpeciality eds = new Entity.DoctorSpeciality();
                                eds.DoctorId = model.DoctorId;
                                eds.SpecialityId = item;
                                db.DoctorSpecialities.Add(eds);
                            }
                        }
                        var ToRemoveDQ = db.DoctorQualifications.Where(rec => rec.DoctorId == model.DoctorId);
                        db.DoctorQualifications.RemoveRange(ToRemoveDQ);
                        if (model.QualificationId != null)
                        {
                            foreach (var item in model.QualificationId)
                            {
                                Entity.DoctorQualification eds = new Entity.DoctorQualification();
                                eds.DoctorId = model.DoctorId;
                                eds.QualificationId = item;
                                db.DoctorQualifications.Add(eds);
                            }
                        }
                        var ToRemoveDF = db.DocFacilityAccessments.Where(rec => rec.DoctorId == model.DoctorId);
                        db.DocFacilityAccessments.RemoveRange(ToRemoveDF);
                        if (model.FacilitiesId != null)
                        {
                            foreach (var item in model.FacilitiesId)
                            {
                                Entity.DocFacilityAccessment eds = new Entity.DocFacilityAccessment();
                                eds.DoctorId = model.DoctorId;
                                eds.FacilityId = item;
                                db.DocFacilityAccessments.Add(eds);
                            }
                        }
                        try
                        {
                            db.SaveChanges();
                        }
                        catch (DbEntityValidationException e)
                        {
                            throw;
                        }


                        if (signatureFile != null)
                        {
                            AccountService.CreateOrUpdateSignature(db, entityUser.Doctor, signatureFile.InputStream,signatureFile.FileName);
                        }
                    }

                    if (imageFile != null)
                    {
                        var photo = CreateOrUpdateImage(entityUser.UserId, imageFile.InputStream);
                    }
                    if (certificateFile != null)
                    {
                        var extension = Path.GetExtension(certificateFile.FileName);
                        CreateOrUpdateCertificate(entityUser.UserId, certificateFile.InputStream, extension,1);
                    }
                    
                    return RedirectToAction("Index");
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("", "Server Error!");
                }
            }
            ViewBag.Countries = CountryService.GetCountryList();
            ViewBag.Categories = TeamService.GetCategoryList();
            return View(model);
        }

        public JsonResult Delete(string doctorId)
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityDoctor = AccountService.GetEntityUserByUserId(db, doctorId, false, true);
                entityDoctor.IsDelete = true;
                entityDoctor.UserName = "[deleted]" + entityDoctor.UserName + entityDoctor.UserId.ToString(); ;
                db.SaveChanges();
            }
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Verify(string doctorId)
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityDoctor = AccountService.GetEntityUserByUserId(db, doctorId, false, true);
                entityDoctor.Doctor.IsVerified = true;
                entityDoctor.Doctor.VerifiedDate = DateTime.UtcNow;
                db.SaveChanges();
            }
            return Json(true, JsonRequestBehavior.AllowGet);
        }
               
        //public JsonResult CheckRepeat(string email)
        //{
        //    bool result;
        //    using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
        //    {
        //        var entityUser = AccountService.GetEntityTargetUserByUsername(db, email, true);
        //        result = (entityUser == null);
        //    }
        //    return Json(new { valid = result }, JsonRequestBehavior.AllowGet);
        //}

        public JsonResult Ban(string doctorId)
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityUser = AccountService.GetEntityUserByUserId(db, doctorId, false, true);
                entityUser.IsBan = true;
                db.SaveChanges();
            }
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        public JsonResult UnBan(string doctorId)
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityUser = AccountService.GetEntityUserByUserId(db, doctorId, false, true);
                entityUser.IsBan = false;
                db.SaveChanges();
            }
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        public static bool CreateOrUpdateCertificate(int doctorId, Stream file, string extension, int certiType)
        {
            
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                DateTime now = DateTime.UtcNow;
                
                Entity.Certi certi = new Entity.Certi();
                
                //if(db.Certis.Where(x => x.DoctorId == doctorId).Count() > 0)
                //{
                //    var certiExist = db.Certis.Where(x => x.DoctorId == doctorId).ToList();
                //    certiExist.ForEach(x => x.CertiStatus = 4);
                //    db.SaveChanges();

                //}

                //Entity.UserProfile entityUser = AccountService.GetEntityUserByUserId(db, doctorId.ToString(), false, false);
                string containerName = "d" + doctorId.ToString("D5");
                string oldPhotoUrl = certi.CertiUrl;
                var originalBlobUrl = CloudBlob.UploadFile(containerName, Guid.NewGuid().ToString() + extension, file);
                certi.CertiUrl = originalBlobUrl;
                certi.DoctorId = doctorId;
                certi.CertiStatus = certiType;
                
                db.Certis.Add(certi);
                db.SaveChanges();
                if (!string.IsNullOrEmpty(oldPhotoUrl))
                {
                    CloudBlob.DeleteImage(oldPhotoUrl);
                }
                return true;
            }
        }

        private Entity.Photo CreateOrUpdateImage(int doctorId, Stream file)
        {
            Entity.Photo entityPhoto;
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                DateTime now = DateTime.UtcNow;
                Entity.UserProfile entityUser = AccountService.GetEntityUserByUserId(db, doctorId.ToString(), false, false);
                long oldPhotoId = 0;
                if (entityUser.PhotoId.HasValue)
                {
                    oldPhotoId = entityUser.PhotoId.Value;
                }

                string containerName = "u" + entityUser.UserId.ToString("D5");
                entityPhoto = PhotoHelper.UploadImageWithThumbnail(db, containerName, Guid.NewGuid().ToString(), file);
                entityUser.PhotoId = entityPhoto.PhotoId;

                entityUser.LastUpdateDate = DateTime.Now;

                db.SaveChanges();

                if (oldPhotoId > 0)
                {
                    PhotoHelper.DeletePhoto(db, oldPhotoId);
                }

            }
            return entityPhoto;
        }        


        public JsonResult GetStatistics(int doctorId)
        {
            StatisticsModel result;
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                result = new StatisticsModel(db, doctorId);
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        #region Reviews

        public ActionResult Reviews(string doctorId)
        {
            UserModel model;
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityUser = AccountService.GetEntityUserByUserId(db, doctorId, false, true);
                model = new UserModel(entityUser);
            }
            return View(model);
        }

        public JsonResult GetReviews(int doctorId)
        {
            int take, skip, recordsTotal, recordsFiltered;
            List<ReviewModel> data = new List<ReviewModel>();
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                take = Convert.ToInt32(Request.Form["length"]);
                skip = Convert.ToInt32(Request.Form["start"]);
                var entityReviewList = db.DoctorUserReviews.Where(e => e.DoctorId == doctorId).OrderBy(e => e.UserProfile.FullName).Skip(skip).Take(take);
                recordsTotal = entityReviewList.Count();
                recordsFiltered = recordsTotal;
                foreach (var enittyReview in entityReviewList)
                {
                    data.Add(new ReviewModel(enittyReview));
                }
            }

            return Json(new
            {
                recordsTotal = recordsTotal,
                recordsFiltered = recordsFiltered,
                data = data,
            });
        }

        public JsonResult DeleteReview(int doctorId, int patientId)
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityReview = db.DoctorUserReviews.Where(e => e.DoctorId == doctorId && e.UserId == patientId).FirstOrDefault();
                if (entityReview != null)
                {
                    db.DoctorUserReviews.Remove(entityReview);
                    db.SaveChanges();
                }
                else
                {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.NotFound, Account.ErrorUserNotFound));
                }
            }
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        #endregion Reviews
    }
}
