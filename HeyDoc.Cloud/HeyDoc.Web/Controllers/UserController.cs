using HeyDoc.Web.Models;
using HeyDoc.Web.Services;
using HeyDoc.Web.Helpers;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Dynamic;

namespace HeyDoc.Web.Controllers
{
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class UserController : BaseController
    {
        public ActionResult Index()
        {
            var companies = new List<SelectListItem>();
            companies.AddRange(CompanyService.GetCompaniesSelectList());            
            ViewBag.Companies = companies;
            return View();
        }

        [HttpPost]
        public JsonResult GetList(int draw, int length, int start, List<DataTableOrderOptions> order, List<DataTableColumnProps> columns, int CompanyId = 1, string Email = "", string Name = "", ListUserType UserType = ListUserType.All)
        {
            int recordsTotal, recordsFiltered;
            var data = new List<PatientModel>();
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityUserList = db.UserProfiles.Where(e => e.CompanyId == CompanyId && e.webpages_Roles.Any(f => f.RoleName == "User") && !e.IsDelete);
                recordsTotal = entityUserList.Count();
                if (UserType == ListUserType.NonCorporateUser)
                {
                    entityUserList = entityUserList.Where(e => e.CorporateId == null);
                }
                if (UserType == ListUserType.CorporateUser)
                {
                    entityUserList = entityUserList.Where(e => e.CorporateId != null);
                }

                if (!string.IsNullOrEmpty(Email))
                {
                    entityUserList = entityUserList.Where(x => x.UserName.Contains(Email));
                }
                if (!string.IsNullOrEmpty(Name))
                {
                    entityUserList = entityUserList.Where(x => x.FullName.Contains(Name));
                }
                recordsFiltered = entityUserList.Count();

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
                            case "FullNameAndUserName":
                                entityUserList = entityUserList.DynamicOrderBy(e => e.FullName, descendingOrder, firstOrdering);
                                firstOrdering = false;
                                entityUserList = entityUserList.DynamicOrderBy(e => e.UserName, descendingOrder, firstOrdering);
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
                    entityUserList = entityUserList.OrderBy(e => e.FullName);
                }

                entityUserList = entityUserList.Skip(start).Take(length);

                foreach (var entityUser in entityUserList)
                {
                    var Data = PatientService.ViewBioData(entityUser);
                    data.Add(Data);
                }
            }

            return Json(new
            {
                draw, // Draw call number from DataTables, used to correctly order the AJAX responses on client-side
                recordsTotal,
                recordsFiltered,
                data,
            });
        }

        public JsonResult Ban(string userId)
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityUser = AccountService.GetEntityUserByUserId(db, userId, false, true);
                entityUser.IsBan = true;
                db.SaveChanges();
            }
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        public JsonResult UnBan(string userId)
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityUser = AccountService.GetEntityUserByUserId(db, userId, false, true);
                entityUser.IsBan = false;
                db.SaveChanges();
            }
            return Json(true, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult Delete(string userId)
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityUser = AccountService.GetEntityUserByUserId(db, userId, false, true);
                entityUser.IsDelete = true;
                entityUser.UserName = "[deleted]" + entityUser.UserName + entityUser.UserId.ToString();
                db.SaveChanges();
            }
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Verify(string userId)
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityUser = AccountService.GetEntityUserByUserId(db, userId, false, true);
                var entityMembership = db.webpages_Membership.FirstOrDefault(e => e.UserId == entityUser.UserId);
                if (entityMembership.IsConfirmed.HasValue && entityMembership.IsConfirmed.Value == false)
                {
                    entityMembership.IsConfirmed = true;
                    db.SaveChanges();
                }
            }
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetAllHCP(int categoryId)
        {
            List<UserSimpleModel> result = new List<UserSimpleModel>();
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityHCPList = db.Doctors.Where(e => e.CategoryId == categoryId && !e.UserProfile.IsDelete && !e.UserProfile.IsBan).OrderByDescending(e => e.UserProfile.FullName);
                
                foreach (var entityHCP in entityHCPList)
                {
                    result.Add(new UserSimpleModel(entityHCP));
                }
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

       
        [Authorize(Roles = "Admin,SuperAdmin")]
        public FileContentResult Export()
        {
            Byte[] bin;
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                List<Entity.UserProfile> entityUserList = new List<Entity.UserProfile>();

                entityUserList = db.UserProfiles
                   .Where(e => e.webpages_Roles.FirstOrDefault(f => f.RoleName == "User") != null)
                   .OrderBy(e => e.UserName)
                   .ToList();

                using (ExcelPackage p = new ExcelPackage())
                {
                    ExcelWorksheet ws = p.Workbook.Worksheets.Add("User List");
                    ws.Cells.Style.Font.Size = 11; //Default font size for whole sheet
                    ws.Cells.Style.Font.Name = "Calibri"; //Default Font name for whole sheet

                    ws.Cells[1, 1, 1, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center; // 

                    // column names
                    ws.Cells[1, 1].Value = "Index";
                    ws.Cells[1, 2].Value = "User name";
                    ws.Cells[1, 3].Value = "Email";
                    ws.Cells[1, 4].Value = "Birthday";
                    ws.Cells[1, 5].Value = "Is ban";
                    ws.Cells[1, 6].Value = "Language";
                    ws.Cells[1, 7].Value = "Country";

                    int row = 2, index = 1;
                    foreach (var model in entityUserList)
                    {
                        ws.Cells[row, 1].Value = index++;
                        ws.Cells[row, 2].Value = model.FullName;
                        ws.Cells[row, 3].Value = model.UserName;
                        ws.Cells[row, 4].Value = model.Birthday != null ? ConvertDateToLocal(model.Birthday.Value).ToString("yyyy-MM-dd") : "";
                        ws.Cells[row, 5].Value = model.IsBan;
                        ws.Cells[row, 6].Value = model.Language;
                        ws.Cells[row, 7].Value = model.Country != null ? model.Country.CountryName : "";

                        ws.Cells[row, 1, row, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center; // 
                        row++;
                    }

                    bin = p.GetAsByteArray();
                }
                return File(bin, "Application/excel", "HeyDoc_User_" + ConvertDateToLocal(DateTime.UtcNow).ToString("yyyy-MM-dd") + ".xlsx");
            }
        }

        [HttpGet]
        public ActionResult Edit(string userId)
        {
            return _GetEditView(userId);
        }

        [HttpPost]
        public ActionResult Edit(UserModel model)
        {
            if (!ModelState.IsValid)
            {
                return _GetEditView(model.UserId.ToString());
            }

            try
            {
                using (var db = new Entity.db_HeyDocEntities())
                {
                    AccountService.UpdateProfileWithCorporate(db, model);
                }
                return RedirectToAction("Index");
            }
            catch
            {
                ModelState.AddModelError("", "Server error. Please try again");
                return _GetEditView(model.UserId.ToString());
            }
        }

        private ActionResult _GetEditView(string userId)
        {
            using (var db = new Entity.db_HeyDocEntities())
            {
                var model = AccountService.GetEntityUserByUserId(db, userId, false);

                ViewBag.CorporateUserTypeSelectList = Enum.GetValues(typeof(CorporateUserType)).Cast<CorporateUserType>().Where(e => e != CorporateUserType.Unknown).Select(e => new SelectListItem
                {
                    Text = e.ToString(),
                    Value = ((int)e).ToString()
                });

                ViewBag.CorporateSelectList = CorporateService.GetCorporateListIncludingHidden(db).Select(e => new SelectListItem
                {
                    Text = e.BranchName,
                    Value = e.CorporateId.ToString(),
                    Selected = model.CorporateId == e.CorporateId
                }).ToList();

                ViewBag.BranchList = BranchService.GetAllBranches(db).Select<BranchModel, ExpandoObject>(e =>
                {
                    dynamic res = new ExpandoObject();
                    res.Text = e.BranchName;
                    res.Value = e.BranchId.ToString();
                    res.Selected = model.UserCorperates.Any(c => c.BranchId == e.BranchId);
                    res.CorporateId = e.CorporateId;
                    return res;
                }).ToList();

                ViewBag.fuckyou = CorporateService.GetCorporatePositions(model.CorporateId.GetValueOrDefault(),model.PositionId.GetValueOrDefault());               
                ViewBag.State = StateService.GetState();
                ViewBag.Township = TownshipService.GetTownshipByStateId(model.StateId.GetValueOrDefault());
                ViewBag.Relationship = RelationshipService.GetRelationshipAll();
                return View("Edit", new UserModel(model));
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public JsonResult GetTownship(int stateId)
        {
            var township = TownshipService.GetTownshipByStateId(stateId);
            return Json(township, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]        
        public JsonResult GetCorporatePosition(int corporateId)
        {   var position = CorporateService.GetCorporatePositions(corporateId,0); 
            return Json(position, JsonRequestBehavior.AllowGet);
        }
    }
}
