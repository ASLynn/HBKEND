using HeyDoc.Web.Models;
using HeyDoc.Web.Services;
using CsvHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Text;
using HeyDoc.Web.Helpers;
using CsvHelper.TypeConversion;

namespace HeyDoc.Web.Controllers
{
    public class PrescriptionController : Controller
    { 
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public ActionResult Prescription(long prescriptionId, Guid p1, Guid p2, string accessSignature)
        {
            using (var db = new Entity.db_HeyDocEntities())
            {
                PrescriptionModel model;
                try
                {
                    model = GetPrescriptionDetails(prescriptionId, p1, p2);
                }
                catch (Exception ex)
                {
                    log.Error(ex);
                    return View("Error");
                }
                if (!PrescriptionService.VerifyTimedAccessToken(prescriptionId, accessSignature))
                {
                    ViewBag.ErrorMessage = "Prescription link has expired";
                    return View("Error");
                }
                ViewBag.QRIdentifier = $"{prescriptionId}_{p1}";

                if (model.PrescribedBy != null)
                {
                    ViewBag.PharmacyLogo = model.PrescribedBy.PrescriptionSource.Logo.ImageUrl;
                }

                return View(model);
            }
        }

        private PrescriptionModel GetPrescriptionDetails(long prescriptionId, Guid p1, Guid p2)
        {
            PrescriptionModel model;
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityPrescription = db.Prescriptions.FirstOrDefault(e => e.PrescriptionId == prescriptionId && e.Identifier1 == p1 && e.Identifier2 == p2);
                if (entityPrescription == null)
                {
                    throw new Exception();
                }
                model = new PrescriptionModel(entityPrescription, true);
            }
            return model;
        }

        [Authorize(Roles = "SuperAdmin")]
        public ViewResult AllPrescriptions()
        {
            using (var db = new Entity.db_HeyDocEntities())
            {
                var prescriptionSourceList = new List<SelectListItem>
                {
                    new SelectListItem { Text = "All", Value = "-1" }
                };
                prescriptionSourceList.AddRange(PrescriptionSourceService.GetPrescriptionSourceSelectList(db));
                ViewBag.PrescriptionSources = prescriptionSourceList;
                ViewBag.PrescriptionSourcesWithoutAll = prescriptionSourceList.Skip(1);
                return View();
            }
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpPost]
        public JsonResult PrescriptionListForSource(int length, int start, List<DataTableOrderOptions> order, List<DataTableColumnProps> columns, int? draw, int prescriptionSourceId = -1, long prescriptionNo = 0, string searchName = "")
        {
            var sortParams = new List<(PrescriptionSortField, bool)>();
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

                PrescriptionSortField sortField;
                switch (sortProperty)
                {
                    case "CreateDate":
                        sortField = PrescriptionSortField.CreateDate;
                        break;
                    case "PatientName":
                        sortField = PrescriptionSortField.PatientName;
                        break;
                    case "DoctorName":
                        sortField = PrescriptionSortField.DoctorName;
                        break;
                    case "PrescriptionId":
                        sortField = PrescriptionSortField.PrescriptionId;
                        break;
                    default:
                        return Json(new
                        {
                            error = $"Invalid sort property: '{sortProperty}'"
                        });
                }
                sortParams.Add((sortField, descendingOrder));
            }
            var data = PrescriptionService.GetPrescriptionsBySource(prescriptionSourceId, prescriptionNo, searchName, start, length, sortParams, out var recordsFiltered, out var recordsTotal);
            return Json(new
            {
                draw,
                recordsTotal,
                recordsFiltered,
                data
            });
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpGet]
        public FileResult PrescriptionCSVStats(int prescriptionSourceId, DateTimeOffset startDate, DateTimeOffset endDate)
        {
            using (var db = new Entity.db_HeyDocEntities())
            {
                var prescriptionSource = db.PrescriptionSources.FirstOrDefault(s => s.PrescriptionSourceId == prescriptionSourceId);
                if (prescriptionSource == null)
                {
                    throw new Exception("Prescription source not found.");
                }

                if (ConstantHelper.DoctorPrescriptionSourceIds.Contains(prescriptionSourceId))
                {
                    return CreateCsv(PrescriptionService.GeneratePrescriptionStats(db, prescriptionSourceId, startDate, endDate), $"{prescriptionSource.PrescriptionSourceName}_");
                }
                return CreateCsv(PrescriptionService.GeneratePrescriptionStats(db, prescriptionSourceId, startDate, endDate), $"{prescriptionSource.PrescriptionSourceName}_Outlet_");
            }

            FileContentResult CreateCsv<T>(IEnumerable<T> data, string filenamePrefix)
            {
                using (var csvStatsWriter = new StringWriter())
                {
                    using (var csvStats = new CsvWriter(csvStatsWriter))
                    {
                        var options = new TypeConverterOptions { Formats = new[] { "yyyy-MM-dd HH:mm:ss" } };
                        csvStats.Configuration.TypeConverterOptionsCache.AddOptions<DateTime>(options);
                        csvStats.Configuration.TypeConverterOptionsCache.AddOptions<DateTime?>(options);
                        csvStats.WriteRecords(data);
                    }

                    var filenameSb = $"{filenamePrefix}Prescriptions_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.csv";

                    return File(Encoding.UTF8.GetBytes(csvStatsWriter.ToString()), "text/csv", filenameSb.ToString());
                }
            }
        }

        [Authorize]
        public ActionResult DigitalPrescription(long id)
        {
            var prescriptionUrl = "";
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                prescriptionUrl = db.Prescriptions.FirstOrDefault(e => e.PrescriptionId == id).FileUrl;
            }
            return Redirect(PrescriptionService.GetBlobSignedPdfUrl(prescriptionUrl));
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpPost]
        public JsonResult Delete(long prescriptionId)
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                PrescriptionService.DeletePrescription(db, prescriptionId);
            }
            return Json(true);
        }

        //For testing the PDF later 
        //[AllowAnonymous]
        //[HttpGet]
        //public ActionResult Pdf()
        //{
        //    using (var db = new Entity.db_HeyDocEntities())
        //    {
        //        var document = PrescriptionService._CreatePdfPrescriptionPartial(db, 1481);
        //        return File(document, "application/pdf", $"prescription_{DateTime.UtcNow.ToString("ddMMyyyyhhmmss")}.pdf");
        //    }
        //}
    }
}
