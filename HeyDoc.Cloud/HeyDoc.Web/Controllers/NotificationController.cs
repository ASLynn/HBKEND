using HeyDoc.Web.Helpers;
using HeyDoc.Web.Lib;
using HeyDoc.Web.Models;
using HeyDoc.Web.Services;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;

namespace HeyDoc.Web.Controllers
{
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class NotificationController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Create()
        {
            CreateNotificationModel model = new CreateNotificationModel();
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                ViewBag.CorporateList = _GetCorporateSelectList(db);

                model.PnTagSelectList = NotificationService.GetAllPnTags(db).Select(t => new SelectListItem
                {
                    Text = t.TagName,
                    Value = t.TagId.ToString(),
                    Selected = false
                }).ToList();
            }

            return View(model);
        }

        public ActionResult CreateTag()
        {
            CreateNotificationTagModel model = new CreateNotificationTagModel();
            return View(model);
        }

        [HttpPost]
        public ActionResult CreateTag(CreateNotificationTagModel model)
        {
            if (ModelState.IsValid)
            {
                using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
                {
                    db.NotificationTags.Add(new Entity.NotificationTag
                    {
                        TagName = model.TagName
                    });
                    db.SaveChanges();
                }
                return RedirectToAction("Index", "Notification");
            }
            return View(model);
        }

        public ActionResult Stats()
        {
            var pnTypeSelectList = new List<SelectListItem>
            {
                new SelectListItem
                {
                    Text = "All",
                    Value = ""
                }
            };

            foreach (PnActionType pnActionType in (PnActionType[])Enum.GetValues(typeof(PnActionType)))
            {
                pnTypeSelectList.Add(new SelectListItem
                {
                    Text = pnActionType.ToString(),
                    Value = ((int)pnActionType).ToString()
                });
            }

            var tagSelectList = new List<SelectListItem>
            {
                new SelectListItem
                {
                    Text = "All",
                    Value = ""
                }
            };

            using (var db = new Entity.db_HeyDocEntities())
            {
                var tags = db.NotificationTags;
                foreach (var tag in tags)
                {
                    tagSelectList.Add(new SelectListItem
                    {
                        Text = tag.TagName,
                        Value = tag.TagId.ToString()
                    });
                }
            }

            ViewBag.PnTypes = pnTypeSelectList;
            ViewBag.Tags = tagSelectList;
            return View();
        }

        private static List<SelectListItem> _GetCorporateSelectList(Entity.db_HeyDocEntities db)
        {
            var corporateList = CorporateService.GetCorporateListIncludingHidden(db);

            var selectList = new List<SelectListItem>()
            {
                new SelectListItem()
                { 
                    Text = "--All--",
                    Value = "0"
                }
            };

            if (corporateList != null)
            {
                foreach (var entityCorporate in corporateList)
                {
                    selectList.Add(new SelectListItem()
                    {
                        Text = string.Format("{0} ({1})", entityCorporate.BranchName, entityCorporate.BranchAddress),
                        Value = entityCorporate.CorporateId.ToString()
                    });
                }
            }

            return selectList;
        }

        [HttpPost]
        public async Task<ActionResult> Create(CreateNotificationModel model)
        {
            if (model.ScheduledDateTime.HasValue && model.ScheduledDateTime.Value.UtcDateTime < DateTime.UtcNow)
            {
                ModelState.AddModelError("", "Cannot schedule into the past. Please select a future date or leave it blank to push the notification now.");
                model.ScheduledDateTime = null;
            }

            if (ModelState.IsValid)
            {
                await NotifyUser(model);
                return RedirectToAction("Index", "Notification");
            }
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                List<SelectListItem> selectList = _GetCorporateSelectList(db);
                ViewBag.CorporateList = selectList;
            }
            return View(model);
        }

        [HttpPost]
        public async Task<JsonResult> Delete(long sequenceNumber)
        {
            await ServiceBusService.DeleteFromQueue(ConstantHelper.PushNotificationServiceBusQueueName, sequenceNumber);
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetScheduledList()
        {
            using (var db = new Entity.db_HeyDocEntities())
            {
                var entityCorporates = CorporateService.GetCorporateListIncludingHidden(db);
                Dictionary<long, string> corporateDict = new Dictionary<long, string>();
                foreach (var entityCorporate in entityCorporates)
                {
                    corporateDict.Add(entityCorporate.CorporateId, $"{entityCorporate.BranchName} ({entityCorporate.BranchAddress})");
                }

                var messages = await ServiceBusService.PeekSerializedQueueMessages<CreateNotificationModel>(ConstantHelper.PushNotificationServiceBusQueueName, 100);
                messages = messages.OrderBy(m => m.Message.ScheduledDateTime).ToList();

                var results = messages.Select(m => new
                {
                    SequenceNumber = m.SequenceNumber,
                    NotificationType = m.Message.NotificationType.ToString(),
                    Text = m.Message.Text,
                    URL = m.Message.NotificationType == PnActionType.URL ? m.Message.URL : "-",
                    DeviceType = m.Message.DeviceType == DeviceType.Invalid ? "All" : m.Message.DeviceType.ToString(),
                    ScheduledDateTime = m.Message.ScheduledDateTime.Value.UtcDateTime.ToString("O"),
                    Corporate = m.Message.CorporateId == 0 ? "-" : corporateDict[m.Message.CorporateId]
                });
                return Json(new
                {
                    data = results
                }, JsonRequestBehavior.AllowGet);
            }
        }

        public async Task<ActionResult> Edit(long sequenceNumber)
        {
            var model = await ServiceBusService.PeekSerializedQueueMessage<CreateNotificationModel>(ConstantHelper.PushNotificationServiceBusQueueName, sequenceNumber);
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                ViewBag.CorporateList = _GetCorporateSelectList(db);

                model.Message.PnTagSelectList = NotificationService.GetAllPnTags(db).Select(t => new SelectListItem
                {
                    Text = t.TagName,
                    Value = t.TagId.ToString(),
                    Selected = model.Message.PnTagSelectList.Exists(selection => int.Parse(selection.Value) == t.TagId)
                }).ToList();
            }
            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> Edit(ServiceBusQueueMessage<CreateNotificationModel> model)
        {
            if (ModelState.IsValid)
            {
                await ServiceBusService.DeleteFromQueue(ConstantHelper.PushNotificationServiceBusQueueName, model.SequenceNumber);
                await NotifyUser(model.Message);
                return RedirectToAction("Index", "Notification");
            }

            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                List<SelectListItem> selectList = _GetCorporateSelectList(db);
                ViewBag.CorporateList = selectList;
            }
            return View(model);
        }

        public JsonResult GetNotificationTagStats()
        {
            using (var db = new Entity.db_HeyDocEntities())
            {
                var counts = db.NotificationTags
                    .Select(t => new
                    {
                        TagName = t.TagName,
                        ClickedCount = t.Notifications.Count > 0 ? t.Notifications.Sum(n => n.NotificationReadStatus.Count(r => r.IsRead.HasValue && r.IsRead.Value == true)) : 0,
                        TotalCount = t.Notifications.Count > 0 ? t.Notifications.Sum(n => n.NotificationReadStatus.Count()) : 0
                    });

                var results = counts.Select(c => new NotificationTagStatsModel
                {
                    TagName = c.TagName,
                    Ctr = c.TotalCount > 0 ? (double) c.ClickedCount / c.TotalCount : 0
                }).ToList();

                return Json(new
                {
                    data = results
                }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult GetNotificationStats(int length, int start, List<DataTableOrderOptions> order, List<DataTableColumnProps> columns, string pnType = "-1", string tag = "-1")
        {
            int recordsTotal, recordsFiltered;
            using (var db = new Entity.db_HeyDocEntities())
            {
                recordsTotal = db.Notifications.Count();

                var entityStats = db.Notifications.Select(e => e);
                var o = order[0];
                var sortProperty = columns[o.column].name;
                switch (sortProperty)
                {
                    case "SentDateTime":
                        entityStats = entityStats.DynamicOrderBy(e => e.CreateDate, o.dir == "desc");
                        break;
                    default:
                        entityStats = entityStats.OrderByDescending(e => e.CreateDate);
                        break;
                }

                if (!string.IsNullOrEmpty(pnType))
                {
                    if (pnType != "-1")
                    {
                        if (Enum.TryParse(pnType, out PnActionType actionType))
                        {
                            entityStats = entityStats.Where(e => e.NotificationType == actionType);
                        }
                    }
                }

                if (!string.IsNullOrEmpty(tag))
                {
                    if (tag != "-1")
                    {
                        if (int.TryParse(tag, out int tagId))
                        {
                            entityStats = entityStats.Where(e => e.NotificationTags.Any(t => t.TagId == tagId));
                        }
                    }
                }

                recordsFiltered = entityStats.Count();

                var stats = entityStats.Skip(start).Take(length).Select(n => new
                {
                    n.NotificationId,
                    n.NotificationType,
                    n.Text,
                    n.RelatedId,
                    n.DeviceType,
                    n.CreateDate,
                    n.NotificationReadStatus,
                    TagNames = n.NotificationTags.Select(t => t.TagName)
                }).ToList().Select(n => new NotificationStatsModel
                {
                    NotificationId = n.NotificationId,
                    NotificationType = n.NotificationType.ToString(),
                    Text = n.Text,
                    URL = n.NotificationType == PnActionType.URL ? n.RelatedId : "-",
                    DeviceType = n.DeviceType.HasValue && n.DeviceType.Value == (byte)DeviceType.Invalid ? "All" : n.DeviceType.ToString(),
                    SentDateTime = n.CreateDate.ToString("O"),
                    TagNames = string.Join(", ", n.TagNames),
                    Reach = n.NotificationReadStatus.Count,
                    Ctr = n.NotificationReadStatus.Count > 0 ? (n.NotificationReadStatus.Count(r => r.IsRead == true) / n.NotificationReadStatus.Count) : 0
                });

                return Json(new
                {
                    recordsTotal = recordsTotal,
                    recordsFiltered = recordsFiltered,
                    data = stats
                });
            }
        }

        #region Private
        private async Task NotifyUser(CreateNotificationModel model)
        {
            if (model.ScheduledDateTime.HasValue)
            {
                var isMoreThan13DaysAway = model.ScheduledDateTime.Value.Subtract(DateTimeOffset.UtcNow).TotalDays > 13;
                await ServiceBusService.ScheduleQueueMessage(ConstantHelper.PushNotificationServiceBusQueueName, JsonConvert.SerializeObject(model), isMoreThan13DaysAway ? DateTime.UtcNow.AddDays(13) : model.ScheduledDateTime.Value.UtcDateTime);
            }
            else
            {
                using (HeyDoc.Web.Entity.db_HeyDocEntities db = new HeyDoc.Web.Entity.db_HeyDocEntities())
                {
                    await NotificationService.NotifyUser(db, model);
                }
            }
        }
        #endregion Private
    }
}
