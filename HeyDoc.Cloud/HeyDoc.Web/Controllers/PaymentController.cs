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
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace HeyDoc.Web.Controllers
{
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class PaymentController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }


        [HttpPost]
        public JsonResult GetCashOutList()
        {
            int take, skip, recordsTotal, recordsFiltered;
            List<CashOutRequestModel> data = new List<CashOutRequestModel>();
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                take = Convert.ToInt32(Request.Form["length"]);
                skip = Convert.ToInt32(Request.Form["start"]);
                int sortParam = Convert.ToInt32(Request.Form["order[0][column]"]);
                string sortOrder = Request.Form["order[0][dir]"];

                var entityCashOutRequests = db.CashOutRequests.OrderBy(e => e.CashOutRequestStatus).ThenByDescending(e => e.CreateDate).Skip(skip).Take(take);
                recordsFiltered = recordsTotal = db.CashOutRequests.Count();

                switch (sortOrder)
                {
                    case "asc":
                        switch (sortParam)
                        {
                            case 2:
                                entityCashOutRequests = entityCashOutRequests.OrderBy(e => e.UserProfile.FullName);
                                break;
                            case 4:
                                entityCashOutRequests = entityCashOutRequests.OrderBy(e => e.CashOutRequestStatus);
                                break;
                            case 5:
                                entityCashOutRequests = entityCashOutRequests.OrderBy(e => e.CreateDate);
                                break;

                            case 6:
                                entityCashOutRequests = entityCashOutRequests.OrderBy(e => e.CashOutDate);
                                break;

                            default:
                                entityCashOutRequests = entityCashOutRequests.OrderBy(e => e.CreateDate);
                                break;
                        }
                        break;
                    case "desc":
                        switch (sortParam)
                        {
                            case 2:
                                entityCashOutRequests = entityCashOutRequests.OrderByDescending(e => e.UserProfile.FullName);
                                break;
                            case 4:
                                entityCashOutRequests = entityCashOutRequests.OrderByDescending(e => e.CashOutRequestStatus);
                                break;
                            case 5:
                                entityCashOutRequests = entityCashOutRequests.OrderByDescending(e => e.CreateDate);
                                break;

                            case 6:
                                entityCashOutRequests = entityCashOutRequests.OrderByDescending(e => e.CashOutDate);
                                break;

                            default:
                                entityCashOutRequests = entityCashOutRequests.OrderByDescending(e => e.CreateDate);
                                break;
                        }
                        break;
                }

                foreach (var entityCashOutReq in entityCashOutRequests)
                {
                    data.Add(new CashOutRequestModel(entityCashOutReq));
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
        public async Task<JsonResult> ChangeStatus(CashOutRequestStatus status, long requestId)
        {
            string remark = Request.Form["remark"];
            HttpPostedFileBase receipt = null;
            if (Request.Files != null && Request.Files.Count > 0)
            {
                receipt = Request.Files[0];
            }
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityRequest = db.CashOutRequests.FirstOrDefault(e => e.CashOutRequestId == requestId);
                if (entityRequest == null)
                {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.NotFound, "No record found"));
                }
                if (receipt != null)
                {
                    string containerName = "receipts";
                    var extension = Path.GetExtension(receipt.FileName);
                    var originalBlobUrl = CloudBlob.UploadFile(containerName, entityRequest.CashOutRequestId.ToString() + "/" + Guid.NewGuid().ToString() + extension, receipt.InputStream);
                    entityRequest.ReceiptUrl = originalBlobUrl;
                }
                entityRequest.CashOutRequestStatus = status;
                entityRequest.AdminRemark = remark;
                entityRequest.CashOutDate = DateTime.UtcNow;
                db.SaveChanges();
                await ChatService.PushToUsers(db, "HOPETeam", entityRequest.UserProfile.UserId, PnActionType.Message, entityRequest.CashOutRequestId, "You have received a payment from HOPE");
                SendMail(entityRequest.UserProfile.ContactEmail ?? entityRequest.UserProfile.UserName, entityRequest.ReceiptUrl);
            }
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        private void SendMail(string email, string receipt)
        {
            string subject = "HOPE: Check your recent payment";
            string content = EmailHelper.ConvertEmailHtmlToString(@"Emails\CashOutReceipt.html");
            content = string.Format(content, DateTime.UtcNow.ToString("dd-MMM-yyyy"), receipt);

            EmailHelper.SendViaSendGrid(new List<string>() { email }, subject, content, string.Empty);
        }

        [HttpPost]
        [AllowAnonymous]
        public void TopupPaymentCallbackRedirectFrontEnd(PointTopupRequestModel model)
        {
            //Response.Redirect("http://localhost:4200/topupstatus?orderid=" + model.OrderId + "&amount=" + model.Amount + "&status=" + model.Status + "&payment_method=" + model.Payment_Method + "");
            Response.Redirect("https://web.hope.com.mm/topupstatus?orderid=" + model.OrderId + "&amount=" + model.Amount + "&status=" + model.Status + "&payment_method=" + model.Payment_Method + "");
        }
    }
}
