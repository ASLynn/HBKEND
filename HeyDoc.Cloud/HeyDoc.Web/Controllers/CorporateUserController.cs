using HeyDoc.Web.Models;
using HeyDoc.Web.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.IO;
using System.Text;
using CsvHelper;
using DocumentFormat.OpenXml.Bibliography;
using CsvHelper.TypeConversion;
using System.Web;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Data;
using System.Dynamic;
using System.Threading.Tasks;

namespace HeyDoc.Web.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public class CorporateUserController : BaseController
    {
        public ActionResult Index()
        {
            var corporateList = new List<SelectListItem>
            {
                new SelectListItem(){ Text = "All Corporate", Value = "" }
            };
            var tpaList = new List<SelectListItem>
            {
                new SelectListItem(){ Text = "All TPA", Value = "" }
            };
            var userTypeList = new List<SelectListItem>
            {
                new SelectListItem(){ Text = "All Registration Source", Value = "" }
            };
            using (Entity.db_HeyDocEntities dB = new Entity.db_HeyDocEntities())
            {
                var corporates = dB.Corporates
                                 .Where(y => !y.IsDelete)
                                 .OrderBy(c => c.BranchName)
                                 .Select(y => new SelectListItem() { Text = y.BranchName, Value = y.CorporateId.ToString() })
                                 .ToList();

                var entityTPAs = dB.ThirdPartyAdministrators.Where(e => !e.IsDelete).OrderBy(e => e.Name);
                tpaList.AddRange(entityTPAs.Select(e => new SelectListItem { Text = e.Name, Value = e.TPAId.ToString() }));
                corporateList.AddRange(corporates);
                ViewBag.CorporateWithoutAll = corporates; // Supply a selection list without the all option for statistics export UI
            }
            userTypeList.AddRange(Enum.GetValues(typeof(SourceType)).Cast<SourceType>().Select(e => new SelectListItem { Text = e.GetDescription(), Value = ((int)e).ToString() }));

            ViewBag.Corporate = corporateList;
            ViewBag.TPAName = tpaList;
            ViewBag.UserType = userTypeList;
            return View();
        }

        public ActionResult UserCorporate(string userId)
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityUser = AccountService.GetEntityUserByUserId(db, userId, false, true);
                var model = new UserModel(entityUser);

                return View(model);
            }
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpGet]
        public FileContentResult GetUserPrescriptionCSVStats(DateTimeOffset startDate, DateTimeOffset endDate, int corporateId)
        {
            if (startDate > endDate)
            {
                throw new Exception("Invalid date range. Start Date must be earlier or same day as End Date");
            }
            else if (startDate.AddDays(366) <= endDate)
            {
                throw new Exception("Invalid date range. Maximum allowed range is 366 days");
            }

            // Move endDate to next day so that all prescriptions created on the requested endDate are included
            var endDateNextDayUtc = endDate.AddDays(1).UtcDateTime;
            var startDateUtc = startDate.UtcDateTime;

            List<UserPrescriptionStatsModel> userStatsList;
            using (var db = new Entity.db_HeyDocEntities())
            {
                var userStats = from u in db.UserProfiles
                                where !u.IsDelete && (u.CorporateId == corporateId)
                                orderby u.FullName
                                let prescriptions = u.Patient.Prescriptions.Where(p => !p.IsDelete && p.CreateDate >= startDateUtc && p.CreateDate < endDateNextDayUtc)
                                let dispensedPrescriptions = prescriptions.Where(p => p.IsDispensed)
                                join c in db.UserCorperates on u.UserId equals c.UserId into userCorpGroup
                                let userCorp = userCorpGroup.FirstOrDefault()
                                select new UserPrescriptionStatsModel()
                                {
                                    FullName = u.FullName,
                                    Email = u.UserName,
                                    IsDependant = userCorp == null ? "" :
                                                  userCorp.CorporateUserType == CorporateUserType.EmployeeDependants ||
                                                      userCorp.CorporateUserType == CorporateUserType.EmployeeChild ? "Y" :
                                                  "",
                                    UserType = userCorp == null ? "Public" :
                                               userCorp.CorporateUserType == CorporateUserType.PublicUser ? "Public" :
                                               userCorp.CorporateUserType == CorporateUserType.EmployeeDependants ? "Employee's Dependant" :
                                               userCorp.CorporateUserType == CorporateUserType.EmployeeChild ? "Employee's Child" :
                                               "Employee",
                                    EmployeeName = userCorp != null ? userCorp.EmployeeDependant : "",
                                    PhoneNumber = u.PhoneNumber,
                                    PrescriptionCount = prescriptions.Select(p => p.PrescriptionId).Count(),
                                    DispensedPrescriptionCount = dispensedPrescriptions.Select(p => p.PrescriptionId).Count(),
                                    JoinDate = u.CreateDate,
                                    LastActivityDate = u.LastActivityDate
                                };
                userStatsList = userStats.ToList();
            }
            using (var csvStatsWriter = new StringWriter())
            {
                using (var csvStats = new CsvWriter(csvStatsWriter))
                {
                    var options = new TypeConverterOptions { Formats = new[] { "yyyy-MM-dd HH:mm:ss" } };
                    csvStats.Configuration.TypeConverterOptionsCache.AddOptions<DateTime>(options);
                    csvStats.Configuration.TypeConverterOptionsCache.AddOptions<DateTime?>(options);
                    csvStats.WriteRecords(userStatsList);
                }

                var filenameSb = $"User_Prescriptions_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.csv";

                return File(Encoding.UTF8.GetBytes(csvStatsWriter.ToString()), "text/csv", filenameSb.ToString());
            }
        }

        public JsonResult Delete(string userId)
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityDoctor = AccountService.GetEntityUserByUserId(db, userId, false, true);
                entityDoctor.IsDelete = true;
                entityDoctor.UserName = "[deleted]" + entityDoctor.UserName + entityDoctor.UserId.ToString(); ;
                db.SaveChanges();
            }
            return Json(true, JsonRequestBehavior.AllowGet);
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

        public ActionResult CorporateBulkInsert()
        {
            var corporateList = new List<SelectListItem>
            {
                new SelectListItem(){ Text = "All Corporate", Value = "" }
            };
            using (Entity.db_HeyDocEntities dB = new Entity.db_HeyDocEntities())
            {
                var corporates = dB.Corporates
                                 .Where(y => !y.IsDelete)
                                 .OrderBy(c => c.BranchName)
                                 .Select(y => new SelectListItem() { Text = y.BranchName, Value = y.CorporateId.ToString() })
                                 .ToList();

                corporateList.AddRange(corporates);
                ViewBag.CorporateWithoutAll = corporates; // Supply a selection list without the all option for statistics export UI

                int corpId = Convert.ToInt32(corporates[0].Value);
                var br = dB.Branchs
                                .Where(y => y.CorporateId == corpId)
                                .OrderBy(c => c.BranchName)
                                .Select(y => new SelectListItem() { Text = y.BranchName, Value = y.BranchId.ToString()})
                                .ToList();

                corporateList.AddRange(br);
                ViewBag.BranchList = br;
            }
            return View();
        }
        public ActionResult VerifyExcel(HttpPostedFileBase upexcel)
        {
            string companyid = Request.Form["CorporateId"].ToString();
            string branchid = Request.Form["BranchId"].ToString();
            //int pageNumber = 1;
            RegisterModel model = new RegisterModel();
            List<ImportedCorporateUser> ImportCorpUsers = new List<ImportedCorporateUser>();
            if (upexcel != null)
            {

                using (var excel = new ExcelPackage(upexcel.InputStream))
                {
                    var tbl = new DataTable();

                    var ws = excel.Workbook.Worksheets.First();
                    var hasHeader = true;  // adjust accordingly
                                           // add DataColumns to DataTable
                    foreach (var firstRowCell in ws.Cells[1, 1, 1, ws.Dimension.End.Column])
                        tbl.Columns.Add(hasHeader ? firstRowCell.Text
                            : String.Format("Column {0}", firstRowCell.Start.Column));

                    // add DataRows to DataTable
                    int startRow = hasHeader ? 2 : 1;
                    int succCount = 0;
                    for (int rowNum = startRow; rowNum <= ws.Dimension.End.Row; rowNum++)
                    {
                        var wsRow = ws.Cells[rowNum, 1, rowNum, ws.Dimension.End.Column];
                        DataRow row = tbl.NewRow();
                        foreach (var cell in wsRow)
                            row[cell.Start.Column - 1] = cell.Text;

                        model.CorporateId = Convert.ToInt32(companyid);
                        model.BranchId = Convert.ToInt32(branchid);
                        model.CorporatePositionId = row[0].ToString();
                        model.FullName = row[1].ToString();
                        model.PhoneNumber = row[2].ToString();
                        model.Email = row[3].ToString();
                        model.Password = row[4].ToString();
                        model.ConfirmPassword = row[4].ToString();
                        model.Birthday = Convert.ToDateTime(row[5]);
                        model.Gender = Convert.ToInt32(row[6]) == 1 ? Gender.Male : Gender.Female;
                        model.StateId = Convert.ToInt32(row[7]);
                        model.TownshipId = Convert.ToInt32(row[8]);
                        model.CorporateSecret = row[9].ToString();
                        model.createUserisAdmin = true;
                        try
                        {
                            var result = AccountService.RegisterEmailAPIV2(model);
                            succCount++;
                            //new ImportedCorporateUser { FullName = model.FullName, PhoneNumber = model.PhoneNumber, Email = model.Email };
                            // Add more items to the list  
                            ImportCorpUsers.Add(new ImportedCorporateUser { FullName = model.FullName, PhoneNumber = model.PhoneNumber, Email = model.Email });
                        }
                        catch
                        {
                            tbl.Rows.Add(row);
                        }
                    }
                    var msg = String.Format("Total failed:{0}", tbl.Rows.Count);
                    ViewBag.Message = msg;
                    if (tbl.Rows.Count > 0)
                    {
                        IEnumerable<DataRow> sequence = tbl.AsEnumerable();
                        ViewBag.mod = sequence;
                    }
                    ViewBag.successmsg = String.Format("Total of {0} Corporate User Register Successfully", succCount);

                }

            }
            else
            {
                ViewBag.Message = "Select a file to upload";
            }
            var corporateList = new List<SelectListItem>
            {
                new SelectListItem(){ Text = "All Corporate", Value = "" }
            };
            using (Entity.db_HeyDocEntities dB = new Entity.db_HeyDocEntities())
            {
                var corporates = dB.Corporates
                                 .Where(y => !y.IsDelete)
                                 .OrderBy(c => c.BranchName)
                                 .Select(y => new SelectListItem() { Text = y.BranchName, Value = y.CorporateId.ToString() })
                                 .ToList();



                corporateList.AddRange(corporates);
                ViewBag.CorporateWithoutAll = corporates; // Supply a selection list without the all option for statistics export UI
                int corpId = Convert.ToInt32(corporates[0].Value);
                var br = dB.Branchs
                                .Where(y => y.CorporateId == corpId)
                                .OrderBy(c => c.BranchName)
                                .Select(y => new SelectListItem() { Text = y.BranchName, Value = y.BranchId.ToString() })
                                .ToList();

                corporateList.AddRange(br);
                ViewBag.BranchList = br;
            }
            model.List = ImportCorpUsers;
            return View("CorporateBulkInsert", model);
        }

        public ActionResult GenerateExcel(HttpPostedFileBase generateExcel)
        {
            RegisterModel model = new RegisterModel();
            Byte[] bin;
            using (ExcelPackage p = new ExcelPackage())
            {
                ExcelWorksheet ws = p.Workbook.Worksheets.Add("Corporate User");
                ws.Cells.Style.Font.Size = 11; //Default font size for whole sheet
                ws.Cells.Style.Font.Name = "Calibri"; //Default Font name for whole sheet
                ws.Cells[1, 1, 1, 10].Style.Font.Bold = true;
                ws.Cells[1, 1, 1, 10].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center; // 

                // column names
                ws.Cells[1, 1].Value = "Title/Position";
                ws.Cells[1, 2].Value = "Full Name";
                ws.Cells[1, 3].Value = "Phone Number {09XXXXXXXXX}";
                ws.Cells[1, 4].Value = "E-Mail";
                ws.Cells[1, 5].Value = "Password";
                ws.Cells[1, 6].Value = "Birthday (MM/DD/YYYY)";
                ws.Cells[1, 7].Value = "Gender (Male/Female)";
                ws.Cells[1, 8].Value = "State";
                ws.Cells[1, 9].Value = "Township";
                ws.Cells[1, 10].Value = "SecretKey";
                ws.Cells.AutoFitColumns();
                bin = p.GetAsByteArray();
            }
            return File(bin, "Application/excel", "Corporate_User_" + DateTime.Now.ToString() + ".xlsx");
        }
        [HttpGet]
        public JsonResult GetBranches(int corporateId)
        {
            var branch = BranchService.GetBranchListMobile(corporateId, 0,100);
            return Json(branch, JsonRequestBehavior.AllowGet);
        }
    }
}