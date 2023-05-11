using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using CsvHelper;
using CsvHelper.TypeConversion;
using HeyDoc.Web.Entity;
using HeyDoc.Web.Helpers;
using HeyDoc.Web.Models;
using HeyDoc.Web.Services;

namespace HeyDoc.Web.Controllers
{
    [Authorize(Roles ="Admin,SuperAdmin")]
    public class OnSiteEventController : Controller
    {
        private db_HeyDocEntities db = new db_HeyDocEntities();

        // GET: OnSiteEvent
        public ActionResult Index()
        {
            var onSiteEvents = db.OnSiteEvents.Include(o => o.Corporate);
            return View(onSiteEvents.ToList());
        }

        [HttpPost]
        public JsonResult GetList(int draw, int length, int start, List<DataTableOrderOptions> order, List<DataTableColumnProps> columns)
        {
            var entityEventList = db.OnSiteEvents.AsQueryable();
            var recordsTotal = entityEventList.Select(e => e.Id).Count();
            var recordsFiltered = recordsTotal;

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
                        case "Code":
                            entityEventList = entityEventList.DynamicOrderBy(e => e.Code, descendingOrder, firstOrdering);
                            firstOrdering = false;
                            break;
                        case "Description":
                            entityEventList = entityEventList.DynamicOrderBy(e => e.Description, descendingOrder, firstOrdering);
                            firstOrdering = false;
                            break;
                        case "CorporateName":
                            entityEventList = entityEventList.DynamicOrderBy(e => e.Corporate.BranchName, descendingOrder, firstOrdering);
                            firstOrdering = false;
                            break;
                        case "EventType":
                            entityEventList = entityEventList.DynamicOrderBy(e => e.EventType.Name, descendingOrder, firstOrdering);
                            firstOrdering = false;
                            break;
                        case "CreateDate":
                            entityEventList = entityEventList.DynamicOrderBy(e => e.CreateDate, descendingOrder, firstOrdering);
                            firstOrdering = false;
                            break;
                        case "UsersCheckedInCount":
                            entityEventList = entityEventList.DynamicOrderBy(e => e.OnSiteEventCheckIns.Count(), descendingOrder, firstOrdering);
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
                entityEventList = entityEventList.OrderByDescending(e => e.CreateDate);
            }

            entityEventList = entityEventList.Skip(start).Take(length);
            var data = entityEventList.Select(e => new
            {
                e.Id,
                e.Code,
                e.Description,
                e.CreateDate,
                e.IsActive,
                UsersCheckedInCount = e.OnSiteEventCheckIns.Count(),
                CorporateName = e.Corporate.BranchName,
                EventType = e.EventType.Name
            });

            return Json(new
            {
                draw,
                recordsTotal,
                recordsFiltered,
                data
            });
        }

        // GET: OnSiteEvent/Create
        public ActionResult Create()
        {
            ViewBag.CorporateId = new SelectList(db.Corporates.Where(e => !e.IsDelete), "CorporateId", "BranchName");
            ViewBag.EventTypeId = new SelectList(db.OnSiteEventTypes, "Id", "Name");
            return View();
        }

        // POST: OnSiteEvent/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Code,Description,CorporateId,IsActive,EventTypeId")] OnSiteEvent onSiteEvent)
        {
            if (ModelState.IsValid)
            {
                onSiteEvent.CreateDate = DateTime.UtcNow;
                db.OnSiteEvents.Add(onSiteEvent);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CorporateId = new SelectList(db.Corporates.Where(e => !e.IsDelete), "CorporateId", "BranchName", onSiteEvent.CorporateId);
            ViewBag.EventTypeId = new SelectList(db.OnSiteEventTypes, "Id", "Name", onSiteEvent.EventTypeId);
            return View(onSiteEvent);
        }

        // GET: OnSiteEvent/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            OnSiteEvent onSiteEvent = db.OnSiteEvents.Find(id);
            if (onSiteEvent == null)
            {
                return HttpNotFound();
            }
            ViewBag.CorporateId = new SelectList(db.Corporates.Where(e => !e.IsDelete), "CorporateId", "BranchName", onSiteEvent.CorporateId);
            ViewBag.EventTypeId = new SelectList(db.OnSiteEventTypes, "Id", "Name", onSiteEvent.EventTypeId);
            return View(onSiteEvent);
        }

        // POST: OnSiteEvent/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Code,Description,CorporateId,IsActive,EventTypeId")] OnSiteEvent onSiteEvent)
        {
            if (ModelState.IsValid)
            {
                db.Entry(onSiteEvent).State = EntityState.Modified;
                // Exclude CreateDate from being updated as it will become null otherwise (technically the default DateTime value, not null)
                db.Entry(onSiteEvent).Property(e => e.CreateDate).IsModified = false;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CorporateId = new SelectList(db.Corporates.Where(e => !e.IsDelete), "CorporateId", "BranchName", onSiteEvent.CorporateId);
            ViewBag.EventTypeId = new SelectList(db.OnSiteEventTypes.Where(e => !e.IsDeleted), "Id", "Name", onSiteEvent.EventTypeId);
            return View(onSiteEvent);
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var onSiteEvent = db.OnSiteEvents.Find(id);
            if (onSiteEvent == null)
            {
                return HttpNotFound();
            }
            return View(onSiteEvent);
        }

        public JsonResult GetUserList(int draw, int length, int start, List<DataTableOrderOptions> order, List<DataTableColumnProps> columns, int eventId)
        {
            var entityCheckInList = db.OnSiteEventCheckIns.Where(e => e.EventId == eventId);
            var recordsTotal = entityCheckInList.Count();
            var recordsFiltered = recordsTotal;

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
                            entityCheckInList = entityCheckInList.DynamicOrderBy(e => e.UserProfile.FullName, descendingOrder, firstOrdering);
                            firstOrdering = false;
                            entityCheckInList = entityCheckInList.DynamicOrderBy(e => e.UserProfile.UserName, descendingOrder, firstOrdering);
                            break;
                        case "CheckInDateTime":
                            entityCheckInList = entityCheckInList.DynamicOrderBy(e => e.CheckInDateTime, descendingOrder, firstOrdering);
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
                entityCheckInList = entityCheckInList.OrderByDescending(e => e.CheckInDateTime);
            }

            entityCheckInList = entityCheckInList.Skip(start).Take(length);
            var data = entityCheckInList.Select(e => new
            {
                e.UserId,
                e.UserProfile.FullName,
                e.UserProfile.UserName,
                e.UserProfile.Gender,
                e.CheckInDateTime,
                e.UserBranch.BranchName
            });

            return Json(new
            {
                draw,
                recordsTotal,
                recordsFiltered,
                data
            });
        }

        // POST: OnSiteEvent/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            OnSiteEvent onSiteEvent = db.OnSiteEvents.Find(id);
            db.OnSiteEvents.Remove(onSiteEvent);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult CheckedInUsersCSV(int id)
        {
            var onSiteEvent = db.OnSiteEvents.Find(id);
            if (onSiteEvent == null)
            {
                return HttpNotFound();
            }
            var entityCheckInList = db.OnSiteEventCheckIns.Where(e => e.EventId == id).OrderByDescending(e => e.CheckInDateTime);
            var data = entityCheckInList.Select(e => new
            {
                e.UserId,
                e.UserProfile.FullName,
                e.UserProfile.UserName,
                Gender = e.UserProfile.Gender.HasValue && e.UserProfile.Gender != 0 ? e.UserProfile.Gender.ToString() : "-",
                e.CheckInDateTime,
                e.UserBranch.BranchName
            });

            using (var csvStatsWriter = new StringWriter())
            {
                using (var csvStats = new CsvWriter(csvStatsWriter))
                {
                    var options = new TypeConverterOptions { Formats = new[] { "yyyy-MM-dd HH:mm:ss" } };
                    csvStats.Configuration.TypeConverterOptionsCache.AddOptions<DateTime>(options);
                    csvStats.Configuration.TypeConverterOptionsCache.AddOptions<DateTime?>(options);
                    csvStats.WriteRecords(data);
                }

                var filename = $"EventCheckedInUsers_{onSiteEvent.Code}.csv";

                return File(Encoding.UTF8.GetBytes(csvStatsWriter.ToString()), "text/csv", filename);
            }
        }

        public ActionResult CheckIn(int id)
        {
            string eventCode;
            using (var db = new db_HeyDocEntities())
            {
                eventCode = db.OnSiteEvents.FirstOrDefault(e => e.Id == id).Code;
            }
            ViewBag.EventCode = eventCode;
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        [HttpPost]
        public async Task<BoolResult> CheckInUser(string userMedicalId, string eventCode)
        {
            await OnSiteEventService.CheckInUserNoRestrictions(userMedicalId, eventCode);
            return new BoolResult(true);
        }

        [HttpGet]
        public JsonResult GetEventDetails(string eventCode)
        {
            return Json(OnSiteEventService.GetEventDetails(eventCode), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetCheckInsForUser(int userId, string eventCode)
        {
            return Json(OnSiteEventService.GetCheckInsForUser(userId, eventCode), JsonRequestBehavior.AllowGet);
        }
    }
}
