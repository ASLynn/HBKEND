using HeyDoc.Web.Helpers;
using HeyDoc.Web.Models;
using HeyDoc.Web.Services;
using Microsoft.Azure;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Data.Entity;
using Z.EntityFramework.Plus;

namespace HeyDoc.Web.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public class PrescriptionRequestController : BaseController
    { 
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public JsonResult GetList(List<DataTableColumnProps> columns, string searchKey = "")
        {
            int take, skip, recordsTotal, recordsFiltered;
            List<UserModel> data = new List<UserModel>();
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                take = Convert.ToInt32(Request.Form["length"]);
                skip = Convert.ToInt32(Request.Form["start"]);

                int sortParamIdx = Convert.ToInt32(Request.Form["order[0][column]"]);
                string sortProperty = columns[sortParamIdx].name;
                string sortOrder = Request.Form["order[0][dir]"];

                var entityUserList = db.UserProfiles
                    .Where(e => e.webpages_Roles.FirstOrDefault(f => f.RoleName == "User") != null && e.CorporateId != null && !e.IsDelete);
                recordsTotal = entityUserList.Count();
                if (!string.IsNullOrEmpty(searchKey))
                {
                    entityUserList = entityUserList.Where(e => e.FullName.Contains(searchKey) || e.IC.Contains(searchKey));
                }
                if (!string.IsNullOrEmpty(Request.Form["corporateId"]))
                {
                    var corporateId = Int32.Parse(Request.Form["corporateId"]);
                    entityUserList = entityUserList.Where(y => y.CorporateId == corporateId);
                }
                if (!string.IsNullOrEmpty(Request.Form["tpaId"]))
                {
                    var tpaId = Int32.Parse(Request.Form["tpaId"]);
                    entityUserList = entityUserList.Where(y => y.Corporate.TPAId == tpaId);
                }
                if (!string.IsNullOrEmpty(Request.Form["createdSource"]) && Enum.TryParse<SourceType>(Request.Form["createdSource"], out var createdSource))
                {                   
                    entityUserList = entityUserList.Where(y => y.CreatedSource == createdSource);

                }
                switch (sortProperty)
                {
                    case "CorporateName":
                        switch (sortOrder)
                        {
                            case "asc":
                                entityUserList = entityUserList.OrderBy(e => e.Corporate.BranchName);
                                break;
                            case "desc":
                                entityUserList = entityUserList.OrderByDescending(e => e.Corporate.BranchName);
                                break;
                        }
                        break;
                    case "CreateDate":               
                        switch (sortOrder)
                        {
                            case "asc":
                                entityUserList = entityUserList.OrderBy(e => e.CreateDate);
                                break;
                            case "desc":
                                entityUserList = entityUserList.OrderByDescending(e => e.CreateDate);
                                break;
                        }
                        break;
                    case "TPAName":       
                        switch (sortOrder)
                        {
                            case "asc":
                                entityUserList = entityUserList.OrderBy(e => e.Corporate.ThirdPartyAdministrator.Name);
                                break;
                            case "desc":
                                entityUserList = entityUserList.OrderByDescending(e => e.Corporate.ThirdPartyAdministrator.Name);
                                break;
                        }
                        break;
                    case "CreatedSource":
                    default:
                        switch (sortOrder)
                        {
                            case "asc":
                                entityUserList = entityUserList.OrderBy(e => e.CreatedSource);
                                break;
                            case "desc":
                                entityUserList = entityUserList.OrderByDescending(e => e.CreatedSource);
                                break;
                        }
                        break;
                }

                recordsFiltered = entityUserList.Count();

                entityUserList = entityUserList.Skip(skip).Take(take);

                foreach (var entityUser in entityUserList)
                {
                    var userModel = new UserModel(entityUser)
                    {
                        CreatedSourceName = entityUser.CreatedSource.GetDescription()
                    };
                    data.Add(userModel);
                }
            }

            return Json(new
            {
                recordsTotal = recordsTotal,
                recordsFiltered = recordsFiltered,
                data = data,
            });
        }

        public ActionResult PrescriptionLog(int PrescriptionId, int Id = 0)
        {
            ViewBag.Id = Id;
            ViewBag.PrescriptionId = PrescriptionId;
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                if (db.PrescriptionDispatchs.Any(e => e.PrescriptionId == PrescriptionId)  && Id < 1)
                {
                    ViewBag.Id = db.PrescriptionDispatchs.Where(e => e.PrescriptionId == PrescriptionId).OrderByDescending(o => o.CreatedDate).FirstOrDefault().DispatchId;
                }
            }
            return View();
        }

        public ActionResult RequestList()
        {
            ViewBag.Pharmacies = new List<SelectListItem>
            {
                new SelectListItem { Text = "All", Value = "All" },
            };

            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                ViewBag.Corporates = new List<SelectListItem>
                {
                    new SelectListItem { Text = "All", Value = "0" },
                    new SelectListItem { Text = "None", Value = "-1" }
                };
                ViewBag.Corporates.AddRange(db.Corporates.Select(e => new SelectListItem { Text = e.BranchName, Value = e.CorporateId.ToString() })
                                                    .Distinct()
                                                    .ToList());

                ViewBag.TPAs = new List<SelectListItem>
                {
                    new SelectListItem { Text = "All", Value = "0" },
                    new SelectListItem { Text = "None", Value = "-1" }
                };
                ViewBag.TPAs.AddRange(db.ThirdPartyAdministrators.Select(e => new SelectListItem { Text = e.Name, Value = e.TPAId.ToString() })
                                                .Distinct()
                                                .ToList());

                var statusSelectList = new List<SelectListItem>
                {
                    new SelectListItem { Text = "All", Value = "0" }
                };

                var entityPrescriptions = _GetPrescriptionsByUserRole(db, "All");

                var statusCounts = entityPrescriptions
                    .GroupBy(p => p.ProcessingStatus)
                    .ToDictionary(g => g.Key.HasValue ? (int)g.Key.Value : -1, g => g.Count());

                var unknownExists = statusCounts.TryGetValue(-1, out int unknownCount);
                statusSelectList.Add(new SelectListItem
                {
                    Text = $"Unknown ({( unknownExists ? unknownCount : 0 )})",
                    Value = "-1"
                });

                foreach (var status in Enum.GetValues(typeof(ProcessingStatus)).Cast<ProcessingStatus>())
                {
                    var statusInt = (int)status;
                    var statusExists = statusCounts.TryGetValue(statusInt, out int statusCount);
                    statusSelectList.Add(new SelectListItem
                    {
                        Text = $"{ status } ({( statusExists ? statusCount : 0 )})",
                        Value = statusInt.ToString()
                    });
                }

                ViewBag.Statuses = statusSelectList;

                var processingStatusValues = Enum.GetValues(typeof(ProcessingStatus)).Cast<ProcessingStatus>();
                var processingStatusDict = processingStatusValues.ToDictionary(e => e.ToString(), e => Convert.ToInt32(e));
                ViewBag.ProcessingStatusJson = JsonConvert.SerializeObject(processingStatusDict);

                var shipmentStatusValues = Enum.GetValues(typeof(GdexShipmentStatus)).Cast<GdexShipmentStatus>();
                var shipmentStatusDict = shipmentStatusValues.ToDictionary(e => e.ToString(), e => Convert.ToInt32(e));
                ViewBag.ShipmentStatusJson = JsonConvert.SerializeObject(shipmentStatusDict);
            }

            return View();
        }

        public ActionResult Detail(int Id)
        {
            PrescriptionModel model;
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityPrescription = db.Prescriptions.FirstOrDefault(e => e.PrescriptionId == Id);
                model = new PrescriptionModel(entityPrescription, true)
                {
                    Identifier1 = entityPrescription.Identifier1
                };
            }
            return View(model);
        }

        [HttpPost]
        public JsonResult GePrescriptiontList(string searchKey = "", int userId = 0, PrescriptionStatus? prescriptionStatus = null)
         {
            int take, skip, recordsTotal, recordsFiltered;
            List<PrescriptionModel> data = new List<PrescriptionModel>();
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                take = Convert.ToInt32(Request.Form["length"]);
                skip = Convert.ToInt32(Request.Form["start"]);

                int sortParam = Convert.ToInt32(Request.Form["order[0][column]"]);
                string sortOrder = Request.Form["order[0][dir]"];

                var entityPrescriptionList = db.Prescriptions
                                             .Include("Patient")
                                             .Include("Patient.UserProfile")
                                             .Include("PrescriptionDispatchs")
                                             .Where(e => !e.IsDelete && e.PatientId == userId);
                recordsTotal = entityPrescriptionList.Count();
                if (!string.IsNullOrEmpty(searchKey))
                {
                    int Id = 0;
                    Int32.TryParse(searchKey, out Id);
                    entityPrescriptionList = entityPrescriptionList.Where(e => e.PrescriptionId == Id);
                }
            
                var dateConstrain = new DateTime(2018, 4, 15, 11, 59, 00);
                entityPrescriptionList = entityPrescriptionList.Where(e => e.PrescriptionDispatchs.Any(p => p.CreatedDate > dateConstrain) || e.PrescriptionStatus == null);

                //Krish : 11 Feb 2019 --> Support #23667 --> Only prescription from corporate users and created through Doc2Us app.
                entityPrescriptionList = entityPrescriptionList.Where(e => e.Patient.UserProfile.CorporateId > 0 && !e.PrescribedBy.HasValue);

                if (prescriptionStatus != null)
                {
                    if (prescriptionStatus == PrescriptionStatus.NoStatus)
                    {
                        entityPrescriptionList = entityPrescriptionList.Where(e => e.PrescriptionStatus == null);
                    }
                    else
                    {
                        entityPrescriptionList = entityPrescriptionList.Where(e => e.PrescriptionDispatchs.Any(p => p.PrescriptionStatus == prescriptionStatus));
                    }
                }

                switch (sortOrder)
                {
                    case "asc":
                        switch (sortParam)
                        {
                            case 4:
                                entityPrescriptionList = entityPrescriptionList.OrderBy(e => e.CreateDate);
                                break;
                            default:
                                entityPrescriptionList = entityPrescriptionList.OrderBy(e => e.CreateDate);
                                break;
                        }
                        break;
                    case "desc":
                        switch (sortParam)
                        {
                            case 4:
                                entityPrescriptionList = entityPrescriptionList.OrderByDescending(e => e.CreateDate);
                                break;

                            default:
                                entityPrescriptionList = entityPrescriptionList.OrderByDescending(e => e.CreateDate);
                                break;
                        }
                        break;
                }

                recordsFiltered = entityPrescriptionList.Count();

                entityPrescriptionList = entityPrescriptionList.OrderByDescending(e => e.CreateDate).Skip(skip).Take(take);

                foreach (var entityPrescription in entityPrescriptionList)
                {
                    data.Add(new PrescriptionModel(entityPrescription, true));
                }
            }

            return Json(new
            {
                recordsTotal = recordsTotal,
                recordsFiltered = recordsFiltered,
                data = data,
            });
        }

        [HttpPost]
        public JsonResult GetRequestedList(int start, int length, List<DataTableOrderOptions> order, List<DataTableColumnProps> columns, string responsiblePharmacy, int corporateId, int tpaId, int? draw, int status = 0, string searchKey = "", string searchName = "", PrescriptionStatus? prescriptionStatus = null, DateTimeOffset? startDate = null, DateTimeOffset? endDate = null)
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                int recordsTotal, recordsFiltered;
                List<PrescriptionModel> data = new List<PrescriptionModel>();

                var entityPrescriptionList = _GetPrescriptionsByUserRole(db, responsiblePharmacy);

                recordsTotal = entityPrescriptionList.Select(e => e.PrescriptionId).Count();

                var statusCounts = entityPrescriptionList
                    .GroupBy(p => p.ProcessingStatus)
                    .Select(g => new
                    {
                        Status = g.Key.HasValue ? (int)g.Key.Value : -1,
                        Count = g.Count()
                    })
                    .ToList();

                foreach (var s in Enum.GetValues(typeof(ProcessingStatus)).Cast<ProcessingStatus>())
                {
                    var sInt = (int)s;
                    if (!statusCounts.Any(c => c.Status == sInt))
                    {
                        statusCounts.Add(new
                        {
                            Status = sInt,
                            Count = 0
                        });
                    }
                }

                var filtersApplied = false;
                if (!string.IsNullOrEmpty(searchKey))
                {
                    int Id = 0;
                    Int32.TryParse(searchKey, out Id);
                    entityPrescriptionList = entityPrescriptionList.Where(e => e.PrescriptionId == Id);
                    filtersApplied = true;
                }
                if (!string.IsNullOrEmpty(searchName))
                {
                    entityPrescriptionList =  entityPrescriptionList.Where(e => e.PatientProfile.FullName.Contains(searchName));
                    filtersApplied = true;
                }
                if (corporateId > 0)
                {
                    filtersApplied = true;
                    entityPrescriptionList = entityPrescriptionList.Where(e => e.PatientProfile.CorporateId == corporateId);
                }
                else if (corporateId == -1)
                {
                    filtersApplied = true;
                    entityPrescriptionList = entityPrescriptionList.Where(e => !e.PatientProfile.CorporateId.HasValue);
                }
                switch (tpaId)
                {
                    case 0: // All TPAs
                        break;
                    case -1: // No TPA
                        filtersApplied = true;
                        entityPrescriptionList = entityPrescriptionList.Where(e => !e.PatientProfile.CorporateId.HasValue || !e.PatientProfile.Corporate.TPAId.HasValue);
                        break;
                    default:
                        filtersApplied = true;
                        entityPrescriptionList = entityPrescriptionList.Where(e => e.PatientProfile.CorporateId.HasValue && e.PatientProfile.Corporate.TPAId.Value == tpaId);
                        break;
                }
                if (prescriptionStatus != null)
                {
                    filtersApplied = true;
                    if (prescriptionStatus == PrescriptionStatus.NoStatus)
                    {
                        entityPrescriptionList = entityPrescriptionList.Where(e => e.PrescriptionStatus == null);
                    }
                    else
                    {
                        entityPrescriptionList = entityPrescriptionList.Where(e => e.PrescriptionDispatchs.Any(p => p.PrescriptionStatus == prescriptionStatus));
                    }
                }
                if (startDate.HasValue) { 
                    filtersApplied = true;
                    var startDateUtc = startDate.Value.UtcDateTime;
                    entityPrescriptionList = entityPrescriptionList.Where(e => e.CreateDate >= startDateUtc);
                }
                if (endDate.HasValue)
                {
                    filtersApplied = true;
                    var endDateUtc = endDate.Value.UtcDateTime.AddDays(1); // Add 1 day to include results for endDate itself
                    entityPrescriptionList = entityPrescriptionList.Where(e => e.CreateDate < endDateUtc);
                }
                switch (status)
                {
                    case 0: // all statuses
                        break;
                    case -1: // unknown status
                        filtersApplied = true;
                        entityPrescriptionList = entityPrescriptionList.Where(e => !e.ProcessingStatus.HasValue);
                        break;
                    default:
                        filtersApplied = true;
                        entityPrescriptionList = entityPrescriptionList.Where(e => e.ProcessingStatus == (ProcessingStatus)status);
                        break;
                }
                recordsFiltered = filtersApplied ? entityPrescriptionList.Select(e => e.PrescriptionId).Count() : recordsTotal;

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
                            case "CreateDate":
                                entityPrescriptionList = entityPrescriptionList.DynamicOrderBy(e => e.CreateDate, descendingOrder, firstOrdering);
                                firstOrdering = false;
                                break;
                            case "RequestDate":
                                entityPrescriptionList = entityPrescriptionList.DynamicOrderBy(e => e.PrescriptionDispatchs.FirstOrDefault().CreatedDate, descendingOrder, firstOrdering);
                                firstOrdering = false;
                                break;
                            case "LastUpdateDate":
                                entityPrescriptionList = entityPrescriptionList.DynamicOrderBy(e => e.LastUpdateDate, descendingOrder, firstOrdering);
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
                    entityPrescriptionList.OrderByDescending(e => e.PrescriptionStatus != null)
                                            .ThenBy(e => e.NextDispenseDateTime == null)
                                            .ThenBy(e => e.PrescriptionDispatchs.FirstOrDefault().CreatedDate);
                }

                entityPrescriptionList = entityPrescriptionList.Skip(start).Take(length);

                // Load IDs first so that queries for IncludeOptimized can run with a simple filter condition
                var prescriptionIds = entityPrescriptionList.Select(e => e.PrescriptionId).ToList();
                // Use Join to create the prescription list with the same ordering as the IDs list
                var presList = prescriptionIds.Join(
                    db.Prescriptions
                        .Where(e => prescriptionIds.Contains(e.PrescriptionId))
                        .Include(e => e.PrescriptionDispatchs)
                        .Include(e => e.PrescribedByUser)
                        .Include(e => e.PrescribedByUser.Photo)
                        .IncludeOptimized(e => e.Drugs)
                        .IncludeOptimized(e => e.Drugs.Select(d => d.Medication))
                        .IncludeOptimized(e => e.Drugs.Select(d => d.Medication.MedicationQuestions))
                        .AsEnumerable(),
                    id => id,
                    p => p.PrescriptionId,
                    (id, p) => p
                ).ToList();

                // Load in UserProfile and related data separately as directly using Include on the main query increases the complexity and causes redundant joins
                // particularly for webpages_Membership
                var patientIds = presList.Select(e => e.PatientId).Distinct();
                db.UserProfiles.Where(u => patientIds.Contains(u.UserId))
                    .Include(e => e.Patient)
                    .Include(e => e.UserCorperates)
                    .Include(e => e.UserCorperates.Select(c => c.Corporate))
                    .Include(e => e.UserCorperates.Select(c => c.Branch))
                    .Include(e => e.webpages_Membership)
                    .Include(e => e.webpages_Roles)
                    .Include(e => e.Photo)
                    .Include(e => e.Country)
                    .Load();
                var doctorIds = presList.Select(e => e.DoctorId).Union(presList.Select(e => e.DispensedBy)).Distinct();
                db.UserProfiles.Where(u => doctorIds.Contains(u.UserId))
                    .Include(e => e.Doctor)
                    .Include(e => e.Doctor.Category)
                    .Include(e => e.Doctor.Group)
                    .Include(e => e.webpages_Membership)
                    .Include(e => e.webpages_Roles)
                    .Include(e => e.Photo)
                    .Include(e => e.Country)
                    .Load();

                foreach (var entityPrescription in presList)
                {
                    var model = new PrescriptionModel(entityPrescription, true)
                    {
                        Identifier1 = entityPrescription.Identifier1
                    };
                    data.Add(model);
                }

                return Json(new
                {
                    draw,
                    recordsTotal,
                    recordsFiltered,
                    data,
                    statusCounts
                });
            }
        }

        [HttpPost]
        public JsonResult ChangeCheckoutProcessed(int prescriptionId, bool processed)
        {
            using (var db = new Entity.db_HeyDocEntities())
            {
                var prescription = db.Prescriptions.FirstOrDefault(e => e.PrescriptionId == prescriptionId);
                if (prescription == null)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return Json(new { error = $"Could not find prescription {prescriptionId} to update processed status" });
                }
                prescription.CheckoutProcessed = processed;
                db.SaveChanges();
                return Json(new EmptyResult());
            }
        }
            
        [HttpPost]
        public JsonResult GetPrescriptionLogList(int start, int length, int? draw, int dispatchId = 0)
        {
            int recordsTotal = 0, recordsFiltered = 0;
            List<PrescriptionLogModel> data = new List<PrescriptionLogModel>();

            if (dispatchId != 0)
            {
                using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
                {
                    var entityPrescriptionLogList = db.PrescriptionLogs.Where(e => !e.IsDelete && e.DispatchId == dispatchId);
                    recordsTotal = entityPrescriptionLogList.Select(e => e.LogId).Count();
                    recordsFiltered = recordsTotal;

                    entityPrescriptionLogList = entityPrescriptionLogList.OrderBy(e => e.CreatedDate).Skip(start).Take(length);

                    foreach (var entityPrescriptionLog in entityPrescriptionLogList)
                    {
                        data.Add(new PrescriptionLogModel(entityPrescriptionLog));
                    }
                }
            }

            return Json(new
            {
                recordsTotal,
                recordsFiltered,
                data,
                draw
            });
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

        [HttpPost]
        public async Task<JsonResult> AddLogPrescription(long prescriptionId, PrescriptionLogModel model)
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var result = await PrescriptionService.AddLogPrescription(db, prescriptionId, model);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public FileResult GetDownloadRequest(DateTimeOffset startDate, DateTimeOffset endDate, string responsiblePharmacy)
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

            var user = User.Identity.Name;
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var dateConstrain = new DateTime(2018, 4, 15, 11, 59, 00);
                var entityPrescriptionList = db.Prescriptions
                        .Where(e => (e.Patient.UserProfile.CorporateId.HasValue)
                               && e.CreateDate >= startDateUtc
                               && e.CreateDate < endDateNextDayUtc
                               && (e.PrescriptionDispatchs.Any(p => p.CreatedDate > dateConstrain) || e.PrescriptionStatus == null) && e.DoctorId.HasValue);

                string roleFilter = "";
                if (User.IsInRole("SuperAdmin"))
                {
                    roleFilter = responsiblePharmacy;
                }
                switch (roleFilter)
                {
                    case "All":
                    // All means no filtering needs to be done
                    default:
                        // Default to all
                        break;
                }
                var recordsTotal = entityPrescriptionList.Count();

                entityPrescriptionList = entityPrescriptionList
                    .OrderByDescending(e => e.LastUpdateDate)
                    .ThenByDescending(e => e.PrescriptionDispatchs.FirstOrDefault().CreatedDate);
                if (!string.IsNullOrEmpty(user) && recordsTotal > 0)
                {
                    byte[] bin = DeliveryPortalService.GetExcelReport(db, entityPrescriptionList);
                    string saveAsFileName = $"{roleFilter}_RequestList_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.xlsx";

                    Response.Clear();
                    Response.ContentType = "application/excel";
                    Response.AddHeader("Content-Disposition", string.Format("attachment;filename={0}", saveAsFileName));
                    Response.BinaryWrite(bin);
                    Response.End();
                }
            }
            return null;
        }

        [HttpPost]
        public BoolResult RecordOrEditPrescriptionPaymentAmount(long prescriptionId, decimal? consultationFees, decimal? medicationFees, decimal? deliveryFees)
        {
            if (!consultationFees.HasValue || !medicationFees.HasValue || !deliveryFees.HasValue)
            {
                throw new Exception("Missing amount. All fees and amounts must be given.");
            }
            using (var db = new Entity.db_HeyDocEntities())
            {
                PaymentService.GenerateOrEditCorporatePrescriptionPaymentRequest(db, prescriptionId, consultationFees, medicationFees, deliveryFees);
            }
            return new BoolResult(true);
        }

        public decimal GetDoctorConsultationRate(int doctorId, DateTime consultationDateTimeUtc)
        {
            using (var db = new Entity.db_HeyDocEntities())
            {
                var doctor = db.Doctors.FirstOrDefault(d => d.UserId == doctorId);
                if (doctor == null)
                {
                    throw new Exception("Doctor not found.");
                }

                decimal amount = doctor.Category.CategoryPrice;
                var malaysianTime = consultationDateTimeUtc.AddHours(8);
                if (malaysianTime.Hour >= 0 && malaysianTime.Hour < 8)
                {
                    amount = doctor.Category.MidNightPrice;
                }

                return amount;
            }
            
        }

        [HttpGet]
        public ActionResult ViewPatientMedia(int patientId, int page = 1)
        {
            var take = 30;
            using (var db = new Entity.db_HeyDocEntities())
            {
                var chatData = ChatService.GetPatientAllMedia(db, patientId, (page - 1) * take, take);
                ViewBag.PatientId = patientId;
                ViewBag.Page = page;
                ViewBag.Take = take;
                ViewBag.TotalRecords = chatData.totalRecords;
                return View(chatData.chatList);
            }
        }

        [HttpPost]
        public async Task<string> CreateGdexCn(long prescriptionId, string receiverName, string phoneNumber, string address, string postcode, string shipmentWeight, string remarks)
        {
            if (!double.TryParse(shipmentWeight, out double weightDouble))
            {
                throw new Exception("Please enter a valid shipment weight.");
            }
            if (string.IsNullOrEmpty(address))
            {
                throw new Exception("Please enter the address");
            }
            if (string.IsNullOrEmpty(postcode))
            {
                throw new Exception("Please enter the postcode");
            }

            using (var db = new Entity.db_HeyDocEntities())
            {
                var entityPrescription = db.Prescriptions.FirstOrDefault(p => !p.IsDelete && p.PrescriptionId == prescriptionId);
                if (entityPrescription == null)
                {
                    throw new Exception("Prescription not found. Check that it has not been deleted.");
                }

                var client = new HttpClient();
                var reqUrl = $"{ ConstantHelper.MyGdexPrimeApiBaseUrl }/CreateConsignment";
                HttpResponseMessage response;

                string json = JsonConvert.SerializeObject(new object[] { new
                {
                    shipmentType = "Parcel",
                    totalPiece = 1,
                    shipmentWeight = weightDouble,
                    receiverName = receiverName,
                    receiverMobile = phoneNumber ?? "",
                    receiverAddress1 = address,
                    receiverPostcode = postcode,
                    receiverCountry = "Malaysia",
                    isDangerousGoods = false,
                    remarks = remarks
                } });

                using (var content = new StringContent(json))
                {
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    content.Headers.Add("ApiToken", CloudConfigurationManager.GetSetting("GdexPrimeApiToken"));
                    content.Headers.Add("Subscription-Key", CloudConfigurationManager.GetSetting("GdexPrimeApiSubscriptionKey"));
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11;

                    response = await client.PostAsync(reqUrl, content);
                }

                var responseData = await response.Content.ReadAsAsync<GdexApiModel<string[]>>();
                if (response.IsSuccessStatusCode)
                {
                    var consignmentNumber = responseData.r[0];
                    entityPrescription.ConsignmentNumber = consignmentNumber;
                    entityPrescription.LatestShipmentStatus = GdexShipmentStatus.Pending;
                    await EditPrescriptionProcessingStatus(new long[] { entityPrescription.PrescriptionId }, ProcessingStatus.ToShip);
                    db.SaveChanges();
                    return consignmentNumber;
                }
                else
                {
                    throw new Exception($"Error received from GDex servers: { responseData.e }");
                }
            }
        }

        [HttpPost]
        public async Task<bool> EditPrescriptionProcessingStatus([System.Web.Http.FromUri]long[] prescriptionIds, ProcessingStatus status)
        {
            using (var db = new Entity.db_HeyDocEntities())
            {
                var allPrescriptions = db.Prescriptions.Select(p => new
                {
                    p.PrescriptionId,
                    p.ProcessingStatus,
                    p.PatientId
                });

                var prescriptionsFiltered = allPrescriptions.Where(p => prescriptionIds.Contains(p.PrescriptionId)).ToList();
                if (prescriptionsFiltered.Count() != prescriptionIds.Count())
                {
                    var missingId = prescriptionIds.FirstOrDefault(id => !prescriptionsFiltered.Any(p => p.PrescriptionId == id));
                    throw new Exception($"Prescription with ID { missingId } not found");
                }
                else if (prescriptionsFiltered.Any(p => p.ProcessingStatus == ProcessingStatus.Shipped || p.ProcessingStatus == ProcessingStatus.Rejected))
                {
                    throw new Exception($"One of selected prescriptions is in Shipped or Rejected status. Unable to perform status change");
                }

                db.Prescriptions.Where(p => prescriptionIds.Contains(p.PrescriptionId)).Update(p => new Entity.Prescription() { ProcessingStatus = status });
                db.SaveChanges();

                foreach (var entry in prescriptionsFiltered)
                {
                    if (entry.PatientId.HasValue)
                    {
                        await NotificationService.NotifyUser(
                            db,
                            entry.PatientId.Value,
                            PnActionType.PrescriptionStatus,
                            entry.PrescriptionId,
                            $"Prescription #{ entry.PrescriptionId } status updated: { status.GetDescription() }"
                        );
                    }
                }
            }
            return true;
        }

        private IQueryable<Entity.Prescription> _GetPrescriptionsByUserRole(Entity.db_HeyDocEntities db, string pharmacyRoleName)
        {
            var entityPrescriptionList = from p in db.Prescriptions
                                         where !p.IsDelete
                                             && !p.PatientProfile.IsDelete
                                             && !p.PatientProfile.IsBan
                                             && p.ChatRoomId.HasValue
                                             && (p.PatientProfile.CorporateId.HasValue)
                                         select p;
            //block request medication 15/04/2018
            var dateConstrain = new DateTime(2018, 4, 15, 11, 59, 00);
            entityPrescriptionList = entityPrescriptionList.Where(e => e.PrescriptionDispatchs.Any(p => p.CreatedDate > dateConstrain) || e.PrescriptionStatus == null && e.DoctorId.HasValue);

            string roleFilter = "";
            if (User.IsInRole("SuperAdmin"))
            {
                roleFilter = pharmacyRoleName;
            }
            switch (roleFilter)
            {
                case "All":
                    // All means no filtering needs to be done
                default:
                    // Default to all
                    break;
            }

            return entityPrescriptionList;
        }

        public async Task<ActionResult> BulkPrintConsignmentNotes([System.Web.Http.FromUri] long[] prescriptionIds)
        {
            var cnList = new List<string>();
            using (var db = new Entity.db_HeyDocEntities())
            {
                cnList = db.Prescriptions.Where(p => prescriptionIds.Contains(p.PrescriptionId) && !string.IsNullOrEmpty(p.ConsignmentNumber)).Select(p => p.ConsignmentNumber).ToList();
            }

            var client = new HttpClient();
            var reqUrl = $"{ ConstantHelper.MyGdexPrimeApiBaseUrl }/GetConsignmentDocument";
            HttpResponseMessage response;

            string json = JsonConvert.SerializeObject(cnList);

            using (var content = new StringContent(json))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                content.Headers.Add("ApiToken", CloudConfigurationManager.GetSetting("GdexPrimeApiToken"));
                content.Headers.Add("Subscription-Key", CloudConfigurationManager.GetSetting("GdexPrimeApiSubscriptionKey"));
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11;

                response = await client.PostAsync(reqUrl, content);
            }

            if (response.IsSuccessStatusCode)
            {
                Response.Clear();
                Response.ContentType = "application/zip";
                Response.AddHeader("Content-Disposition", $"attachment;filename=BulkCN_{ DateTime.Now.ToShortDateString() }.zip");
                Response.BinaryWrite(await response.Content.ReadAsByteArrayAsync());
                Response.End();
            }
            else
            {
                try
                {
                    var responseData = await response.Content.ReadAsAsync<GdexApiModel<GdexGetConsignmentImageErrorModel[]>>();
                    if (responseData != null && responseData.r.Length > 0)
                    {
                        var contentStr = "Error(s) encountered:";
                        foreach (var r in responseData.r)
                        {
                            contentStr += $"<br/>Consignment note { r.consignmentNo }: { r.errorMessage }";
                        }
                        return Content(contentStr);
                    }
                }
                catch
                {
                    return Content("Unknown error occurred");
                }
            }

            return null;
        }

        //[HttpPost]
        //public JsonResult Approve(long prescriptionDispatchId)
        //{
        //    using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
        //    {
        //        var result = PrescriptionService.SetApproveDispatch(db, prescriptionDispatchId);
        //        return Json(result, JsonRequestBehavior.AllowGet);
        //    }
        //}

        //[HttpPost]
        //public JsonResult Ready(long prescriptionDispatchId)
        //{
        //    using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
        //    {
        //        var result = PrescriptionService.SetReadyDispatch(db, prescriptionDispatchId);
        //        return Json(result, JsonRequestBehavior.AllowGet);
        //    }
        //}
    }
}
