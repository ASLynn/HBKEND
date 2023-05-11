using ClosedXML.Excel;
using HeyDoc.Web.Entity;
using HeyDoc.Web.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;

namespace HeyDoc.Web.Services
{
    public class DeliveryPortalService
    {
        public static byte[] GetExcelReport(Entity.db_HeyDocEntities db, IQueryable<Entity.Prescription> entityPrescriptionList)
        {
            using (XLWorkbook wb = new XLWorkbook())
            {
                var ws = wb.Worksheets.Add("Request Report");

                var columnHeaders = new List<string> {
                    "No",
                    "Prescription Number",
                    "Patient Name",
                    "Doctor Name",
                    "Corporate Name",
                    "Branch",
                    "TPA Name",
                    "IC/Passport",
                    "Prescription Create Date",
                    "Request Date",
                    "Requset Mode",
                    "Outlet Pickup",
                    "Delivery Address",
                    "On-site Dispensary Address",
                    "On-site Dispensary Date",
                    "Checkout Processed",
                    "Last Status Date",
                    "Last Status Log",
                    "Medication Type",
                    "Medication Name",
                    "Medication Dosage",
                    "Medication Frequency",
                    "Medication Amount"
                };

                ws.Cell(1, 1).InsertData(columnHeaders, true);

                entityPrescriptionList = entityPrescriptionList
                    .Include(e => e.Patient.UserProfile)
                    .Include(e => e.Patient.UserProfile.UserCorperates)
                    .Include(e => e.Doctor.UserProfile);
                var dataQuery = from p in entityPrescriptionList
                                join drug in db.Drugs on p.PrescriptionId equals drug.PrescriptionId
                                let dispatch = db.PrescriptionDispatchs.Where(e => e.PrescriptionId == p.PrescriptionId)
                                    .OrderBy(e => e.CreatedDate)
                                    .FirstOrDefault()
                                let userCorporate = p.Patient.UserProfile.UserCorperates.FirstOrDefault()
                                let tpa = userCorporate != null ? userCorporate.Corporate.ThirdPartyAdministrator : null
                                let lastStatusUpdate = dispatch != null ? dispatch.PrescriptionLogs.Where(e => !e.IsDelete).OrderByDescending(e => e.CreatedDate).FirstOrDefault() : null
                                select new
                                {
                                    PrescriptionId = "#" + p.PrescriptionId,
                                    PatientName = p.Patient.UserProfile.FullName,
                                    DoctorName = p.Doctor.UserProfile.FullName,
                                    CorporateName = userCorporate != null ? userCorporate.Corporate.BranchName : null,
                                    BranchName = userCorporate != null ? userCorporate.Branch.BranchName : null,
                                    TPA = tpa != null ? tpa.Name : null,
                                    p.Patient.UserProfile.IC,
                                    p.CreateDate,
                                    RequestDate = dispatch != null ? (DateTime?)dispatch.CreatedDate : null,
                                    RequestMode = dispatch == null ? "No Status" :
                                                  dispatch.PrescriptionStatus == PrescriptionStatus.Delivery ? "Delivery" :
                                                  dispatch.PrescriptionStatus == PrescriptionStatus.OnSite ? "On-Site" :
                                                  dispatch.PrescriptionStatus == PrescriptionStatus.SelfCollection ? "Self Collection" :
                                                  "No Status",
                                    PickupAddress = dispatch != null && dispatch.PrescriptionStatus == PrescriptionStatus.SelfCollection ? dispatch.OutletUser.FullName + ", " + dispatch.OutletUser.Address : null,
                                    dispatch.DeliveryAddress,
                                    OnSiteAddress = dispatch != null && dispatch.PrescriptionStatus == PrescriptionStatus.OnSite ? dispatch.OnSiteDispens.OnSiteName + ", " + dispatch.OnSiteDispens.OnSiteAddress : null,
                                    NextDispenseDate = p.NextDispenseDateTime,
                                    CheckoutProcessed = p.CheckoutProcessed ? "Y" : "N",
                                    LastStatusUpdateDate = p.LastUpdateDate,
                                    LastStatusUpdateText = lastStatusUpdate != null ? lastStatusUpdate.LogText : null,
                                    MedicationType = p.MedicationType,
                                    MedicationName = drug.DrugName,
                                    MedicationDosage = drug.Dosage,
                                    MedicationFrequency = drug.Frequency,
                                    MedicationAmount = drug.Amount
                                };
                ws.Cell(2, 2).InsertData(dataQuery.AsEnumerable());
                ws.Cell(2, 1).InsertData(Enumerable.Range(1, dataQuery.Count()));

                using (var ms = new MemoryStream())
                {
                    wb.SaveAs(ms);
                    byte[] bin = ms.ToArray();
                    return bin;
                }
            }
        }
    }
}