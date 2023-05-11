using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using HeyDoc.Web.Entity;

namespace HeyDoc.Web
{
    public class QualificationController : Controller
    {
        private db_HeyDocEntities db = new db_HeyDocEntities();

        // GET: Qualification
        public ActionResult Index()
        {
            return View(db.Qualifications.ToList());
        }

        // GET: Qualification/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Qualification qualification = db.Qualifications.Find(id);
            if (qualification == null)
            {
                return HttpNotFound();
            }
            return View(qualification);
        }

        // GET: Qualification/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Qualification/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "QualificationId,QualificationDesc,Status")] Qualification qualification)
        {
            if (ModelState.IsValid)
            {
                db.Qualifications.Add(qualification);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(qualification);
        }

        // GET: Qualification/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Qualification qualification = db.Qualifications.Find(id);
            if (qualification == null)
            {
                return HttpNotFound();
            }
            return View(qualification);
        }

        // POST: Qualification/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "QualificationId,QualificationDesc,Status")] Qualification qualification)
        {
            if (ModelState.IsValid)
            {
                db.Entry(qualification).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(qualification);
        }

        // GET: Qualification/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Qualification qualification = db.Qualifications.Find(id);
            if (qualification == null)
            {
                return HttpNotFound();
            }
            return View(qualification);
        }

        // POST: Qualification/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Qualification qualification = db.Qualifications.Find(id);
            db.Qualifications.Remove(qualification);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
