using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using RussellGroup.Pims.DataAccess.Models;

namespace RussellGroup.Pims.Website.Controllers
{
    public class PlantHireController : Controller
    {
        private PimsContext db = new PimsContext();

        // GET: /PlantHire/
        public async Task<ActionResult> Index(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var job = await db.Jobs.FindAsync(id);
            if (job == null)
            {
                return HttpNotFound();
            }

            return View(job);
        }

        // GET: /PlantHire/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PlantHire hire = await db.PlantHires.FindAsync(id);
            if (hire == null)
            {
                return HttpNotFound();
            }
            return View(hire);
        }

        // GET: /PlantHire/Create
        public async Task<ActionResult> Create(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var job = await db.Jobs.FindAsync(id);
            if (job == null)
            {
                return HttpNotFound();
            }

            var hire = new PlantHire() 
            {
                Job = job,
                JobId = job.JobId,
                WhenStarted = DateTime.Now
            };

            return View(hire);
        }

        // POST: /PlantHire/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include="PlantHireId,PlantId,JobId,Docket,WhenStarted,WhenEnded,Rate,Comment")] PlantHire hire)
        {
            if (ModelState.IsValid)
            {
                db.PlantHires.Add(hire);
                await db.SaveChangesAsync();
                return RedirectToAction("Index", new { id = hire.JobId });
            }

            return View(hire);
        }

        // GET: /PlantHire/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PlantHire hire = await db.PlantHires.FindAsync(id);
            if (hire == null)
            {
                return HttpNotFound();
            }
            return View(hire);
        }

        // POST: /PlantHire/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include="PlantHireId,PlantId,JobId,Docket,WhenStarted,WhenEnded,Rate,Comment")] PlantHire hire)
        {
            if (ModelState.IsValid)
            {
                db.Entry(hire).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index", new { id = hire.JobId });
            }
            return View(hire);
        }

        // GET: /PlantHire/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PlantHire hire = await db.PlantHires.FindAsync(id);
            if (hire == null)
            {
                return HttpNotFound();
            }
            return View(hire);
        }

        // POST: /PlantHire/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            PlantHire hire = await db.PlantHires.FindAsync(id);
            db.PlantHires.Remove(hire);
            await db.SaveChangesAsync();
            return RedirectToAction("Index", new { id = hire.JobId });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private new ActionResult View()
        {
            throw new NotSupportedException();
        }

        private ActionResult View(Job job)
        {
            return base.View(job);
        }

        private ActionResult View(PlantHire hire)
        {
            var plants = db.Plants.OrderBy(f => f.XPlantId);

            ViewBag.Plants = new SelectList(plants, "PlantId", "XPlantId", hire.PlantId);

            MvcSiteMapProvider.SiteMaps.Current.CurrentNode.ParentNode.RouteValues["id"] = hire.Job.JobId;
            MvcSiteMapProvider.SiteMaps.Current.CurrentNode.ParentNode.ParentNode.RouteValues["id"] = hire.Job.JobId;

            return base.View(hire);
        }
    }
}
