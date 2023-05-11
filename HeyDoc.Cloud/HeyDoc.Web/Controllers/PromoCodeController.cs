using HeyDoc.Web.Lib;
using HeyDoc.Web.Models;
using HeyDoc.Web.Resources;
using HeyDoc.Web.Services;
using HeyDoc.Web.WebApi;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HeyDoc.Web.Controllers
{
    [Authorize(Roles="Admin,SuperAdmin")]
    public class PromoCodeController : BaseController
    {
        public ActionResult Index()
        {
            var categories = TeamService.GetCategoryList();
            List<SelectListItem> categoryList = new List<SelectListItem>() { new SelectListItem { Text = "All", Value = "0" } };
            categoryList.AddRange(categories);
            ViewBag.Categories = categoryList;
            return View();
        }

        [HttpPost]
        public JsonResult GetList(string partner, string code)
        {
            int take, skip, recordsTotal, recordsFiltered;
            List<PromoCodeModel> data = new List<PromoCodeModel>();
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                take = Convert.ToInt32(Request.Form["length"]);
                skip = Convert.ToInt32(Request.Form["start"]);

                var entityPromoCodeList = db.PromoCodes.Where(e => !e.IsDelete);

                recordsTotal = entityPromoCodeList.Count();
                if (!string.IsNullOrEmpty(partner))
                {
                    entityPromoCodeList = entityPromoCodeList.Where(e => e.PartnerName.Contains(partner));
                }
                if (!string.IsNullOrEmpty(code))
                {
                    entityPromoCodeList = entityPromoCodeList.Where(e => e.PromoCode1.Contains(code));
                }
                recordsFiltered = entityPromoCodeList.Count();

                entityPromoCodeList = entityPromoCodeList.OrderBy(e => e.PromoStatus).ThenByDescending(e=>e.CreateDate).ThenByDescending(e=>e.EndDate).Skip(skip).Take(take);

                foreach (var entityPromo in entityPromoCodeList)
                {
                    data.Add(new PromoCodeModel(entityPromo,isDetailed:true));
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
        public JsonResult Edit(PromoCodeModel model)
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityPromoCode = db.PromoCodes.FirstOrDefault(e => e.PromoCodeId == model.PromoCodeId);
                if (entityPromoCode == null)
                {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.NotFound, Promo.ErrorNotFound));
                }
                entityPromoCode.Description = model.Description;
                entityPromoCode.EndDate = model.EndDate;
                entityPromoCode.MaxUserLimit = model.MaxUserLimit;
                entityPromoCode.UserUsageLimit = model.UserUsageLimit;
                entityPromoCode.PartnerName = model.PartnerName;
                entityPromoCode.StartDate = model.StartDate;
                entityPromoCode.PromoStatus = PromoStatus.Active;
                if (model.DoctorId == 0)
                {
                    entityPromoCode.DoctorId = null;
                }
                else
                {
                    entityPromoCode.DoctorId = model.DoctorId;
                }

                if (model.CategoryId == 0)
                {
                    entityPromoCode.CategoryId = null;
                }
                else
                {
                    entityPromoCode.CategoryId = model.CategoryId;
                }
                db.SaveChanges();
                return Json(true, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost]
        public JsonResult Add(PromoCodeModel model, int codeCount, bool isBulkGenerate)
        {
            model.Validate(isBulkGenerate, codeCount);
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                if (!isBulkGenerate)
                {
                    var entityPromo = db.PromoCodes.FirstOrDefault(e => e.PromoCode1 == model.PromoCode && !e.IsDelete);
                    if (entityPromo != null)
                    {
                        throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidArguments, Promo.ErrorAddedBefore));
                    }
                    CreateEntityPromoCode(model, db);
                }
                else
                {
                    for (int i = 0; i < codeCount; i++)
                    {
                        bool isExistingCode = true;
                        while (isExistingCode)
                        {
                            model.PromoCode = RandomString(6);
                            isExistingCode = db.PromoCodes.FirstOrDefault(e => e.PromoCode1 == model.PromoCode && !e.IsDelete) != null;
                        }

                        CreateEntityPromoCode(model, db);
                    }
                }
            }
            return Json(true, JsonRequestBehavior.AllowGet);
        }

       
        public string RandomString(int length)
        {
             Random random = new Random();
             const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private static void CreateEntityPromoCode(PromoCodeModel model, Entity.db_HeyDocEntities db)
        {
            var entityPromoCode = db.PromoCodes.Create();
            entityPromoCode.CreateDate = DateTime.UtcNow;
            entityPromoCode.Description = model.Description;
            entityPromoCode.Discount = model.Discount;
            entityPromoCode.DiscountType = model.DiscountType;
            entityPromoCode.EndDate = model.EndDate;
            entityPromoCode.IsDelete = false;
            entityPromoCode.MaxUserLimit = model.MaxUserLimit;
            entityPromoCode.PartnerName = model.PartnerName;
            entityPromoCode.PromoCode1 = model.PromoCode;
            entityPromoCode.PromoStatus = PromoStatus.Active;
            entityPromoCode.StartDate = model.StartDate == DateTime.MinValue ? DateTime.UtcNow.Date : model.StartDate;
            entityPromoCode.UserUsageLimit = model.UserUsageLimit;

            if (model.DoctorId > 0)
            {
                entityPromoCode.DoctorId = model.DoctorId;
            }
            if (model.CategoryId > 0)
            {
                entityPromoCode.CategoryId = model.CategoryId;
            }

            db.PromoCodes.Add(entityPromoCode);
            db.SaveChanges();
        }
      
        [HttpPost]
        public JsonResult Delete(long promoCodeId)
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityPromoCode = db.PromoCodes.FirstOrDefault(e => e.PromoCodeId == promoCodeId);
                if (entityPromoCode == null)
                {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.NotFound, Promo.ErrorNotFound));
                }
                entityPromoCode.IsDelete = true;
              
                db.SaveChanges();
            }
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult History(long id)
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityPromoCode = db.PromoCodes.FirstOrDefault(e => e.PromoCodeId == id);
                if (entityPromoCode == null)
                {
                    return RedirectToAction("Index");
                }
                var PromoCode = new PromoCodeModel(entityPromoCode);
                return View(PromoCode);
            }
        }

        [HttpPost]
        public JsonResult GetRedemtionHistory(DateTime? startDate, DateTime? endDate, long promoId)
        {
            int take, skip, recordsTotal, recordsFiltered;
            List<PaymentRequestModel> data = new List<PaymentRequestModel>();
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                take = Convert.ToInt32(Request.Form["length"]);
                skip = Convert.ToInt32(Request.Form["start"]);
                int sortParam = Convert.ToInt32(Request.Form["order[0][column]"]);
                string sortOrder = Request.Form["order[0][dir]"];

                IQueryable<Entity.PaymentRequest> entityPayments = db.PaymentRequests.Where(e => e.PromoCodeId == promoId);
                recordsTotal = entityPayments.Count();

                if (startDate.HasValue)
                {
                    startDate = startDate.Value.Date;
                    entityPayments = entityPayments.Where(e => e.CreateDate > startDate.Value);
                }
                if (endDate.HasValue)
                {
                    endDate = endDate.Value.Date.AddDays(1);
                    entityPayments = entityPayments.Where(e => e.CreateDate < endDate.Value);
                }
                recordsFiltered = entityPayments.Count();
                entityPayments = entityPayments.OrderByDescending(e => e.CreateDate);
                

               
                entityPayments = entityPayments.Skip(skip).Take(take);

                foreach (var entityPayment in entityPayments)
                {
                    data.Add(new PaymentRequestModel(entityPayment, true));
                }

            }

            return Json(new
            {
                recordsTotal = recordsTotal,
                recordsFiltered = recordsFiltered,
                data = data,
            });
        }

        public void ExportToExcel(string partner, string code)
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityPromoCodeList = db.PromoCodes.Where(e => !e.IsDelete);

                if (!string.IsNullOrEmpty(partner))
                {
                    entityPromoCodeList = entityPromoCodeList.Where(e => e.PartnerName.Contains(partner));
                }
                if (!string.IsNullOrEmpty(code))
                {
                    entityPromoCodeList = entityPromoCodeList.Where(e => e.PromoCode1.Contains(code));
                }
                Byte[] bin;
                MemoryStream Result = new MemoryStream();
                using (ExcelPackage pack = new ExcelPackage())
                {
                    ExcelWorksheet ws = pack.Workbook.Worksheets.Add("PromoCodes");

                    ws.Cells.Style.Font.Size = 11; //Default font size for whole sheet
                    ws.Cells.Style.Font.Name = "Calibri"; //Default Font name for whole sheet

                    ws.Cells[1, 1, 1, 9].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center; // 

                    // column names
                    ws.Cells[1, 1].Value = "Code";
                    ws.Cells[1, 2].Value = "Discount";
                    ws.Cells[1, 3].Value = "Type";
                    ws.Cells[1, 4].Value = "Start Date";
                    ws.Cells[1, 5].Value = "End Date";
                    ws.Cells[1, 6].Value = "Partner";
                    ws.Cells[1, 7].Value = "User Usage Limit";
                    ws.Cells[1, 8].Value = "Max Redemptions";
                    ws.Cells[1, 9].Value = "Description";



                    int row = 2;
                    foreach (var model in entityPromoCodeList)
                    {
                        ws.Cells[row, 1].Value = model.PromoCode1;
                        ws.Cells[row, 2].Value = model.Discount;
                        ws.Cells[row, 3].Value = model.DiscountType.ToString();
                        ws.Cells[row, 4].Value = model.StartDate.ToString("dd-MM-yyyy");
                        ws.Cells[row, 5].Value = model.EndDate.HasValue?model.EndDate.Value.ToString("dd-MM-yyyy"):"";
                        ws.Cells[row, 6].Value = model.PartnerName;
                        ws.Cells[row, 7].Value = model.UserUsageLimit;
                        ws.Cells[row, 8].Value = model.MaxUserLimit;
                        ws.Cells[row, 9].Value = model.Description;

                        ws.Cells[row, 1, row, 9].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center; // 
                        row++;
                    }
                    bin = pack.GetAsByteArray();
                    //pack.SaveAs(Result);
                }

                string saveAsFileName = string.Format("promocodes.xlsx");
                Response.Clear();
                Response.ContentType = "application/excel";
                Response.AddHeader("Content-Disposition", string.Format("attachment;filename={0}", saveAsFileName));
                Response.BinaryWrite(bin);
                Response.End();
            }           
        }

        public void ExportHistory(DateTime? startDate, DateTime? endDate, long PromoCodeId)
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                IQueryable<Entity.PaymentRequest> entityPayments = db.PaymentRequests.Where(e => e.PromoCodeId == PromoCodeId);
                var entityPromoCode = db.PromoCodes.FirstOrDefault(e => e.PromoCodeId == PromoCodeId);

                if (startDate.HasValue)
                {
                    startDate = startDate.Value.Date;
                    entityPayments = entityPayments.Where(e => e.CreateDate > startDate.Value);
                }
                if (endDate.HasValue)
                {
                    endDate = endDate.Value.Date.AddDays(1);
                    entityPayments = entityPayments.Where(e => e.CreateDate < endDate.Value);
                }
                entityPayments = entityPayments.OrderByDescending(e => e.CreateDate);

                Byte[] bin;
                MemoryStream Result = new MemoryStream();
                using (ExcelPackage pack = new ExcelPackage())
                {
                    ExcelWorksheet ws = pack.Workbook.Worksheets.Add("PromoCodes");

                    ws.Cells.Style.Font.Size = 11; //Default font size for whole sheet
                    ws.Cells.Style.Font.Name = "Calibri"; //Default Font name for whole sheet

                    ws.Cells[1, 1, 1, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center; // 

                    // column names
                    ws.Cells[1, 1].Value = "User";
                    ws.Cells[1, 2].Value = "HCP";
                    ws.Cells[1, 3].Value = "Date";
                    ws.Cells[1, 4].Value = "Total";
                    ws.Cells[1, 5].Value = "Discount";
                    ws.Cells[1, 6].Value = "Paid";



                    int row = 2;
                    foreach (var model in entityPayments)
                    {
                        ws.Cells[row, 1].Value = string.Format("{0} ({1})", model.ChatRoom.Patient.UserProfile.FullName, model.ChatRoom.Patient.UserProfile.UserName);
                        ws.Cells[row, 2].Value = string.Format("{0} ({1})", model.ChatRoom.Doctor.UserProfile.FullName, model.ChatRoom.Doctor.UserProfile.UserName);
                        ws.Cells[row, 3].Value = model.CreateDate.ToString("dd-MM-yyyy");
                        ws.Cells[row, 4].Value = model.ActualAmount;
                        ws.Cells[row, 5].Value = model.ActualAmount - model.Amount;
                        ws.Cells[row, 6].Value = model.Amount;

                        ws.Cells[row, 1, row, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center; // 
                        row++;
                    }
                    bin = pack.GetAsByteArray();
                    //pack.SaveAs(Result);
                }

                string saveAsFileName = string.Format(entityPromoCode.PromoCode1+"_History.xlsx");
                Response.Clear();
                Response.ContentType = "application/excel";
                Response.AddHeader("Content-Disposition", string.Format("attachment;filename={0}", saveAsFileName));
                Response.BinaryWrite(bin);
                Response.End();
            }
        }
    }
}
