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
    public class InventoryHireController : Controller
    {
        private PimsContext db = new PimsContext();

        // GET: /InventoryHire/5
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

        // GET: /InventoryHire/Details/5
        public async Task<ActionResult> Details(int? id, int? hireId)
        {
            if (hireId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            InventoryHire hire = await db.InventoryHires.FindAsync(hireId);
            if (hire == null)
            {
                return HttpNotFound();
            }
            return View(hire);
        }

        // GET: /InventoryHire/Create/5
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

            var hire = new InventoryHire()
            {
                Job = job,
                JobId = job.JobId,
                WhenStarted = DateTime.Now
            };

            return View(hire);
        }

        // POST: /InventoryHire/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include="InventoryHireId,InventoryId,JobId,Docket,WhenStarted,WhenEnded,Rate,Quantity,Comment")] InventoryHire hire)
        {
            if (ModelState.IsValid)
            {
                db.InventoryHires.Add(hire);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(hire);
        }

        // GET: /InventoryHire/Edit/5
        public async Task<ActionResult> Edit(int? id, int? hireId)
        {
            if (hireId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            InventoryHire hire = await db.InventoryHires.FindAsync(hireId);
            if (hire == null)
            {
                return HttpNotFound();
            }
            return View(hire);
        }

        // POST: /InventoryHire/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include="InventoryHireId,InventoryId,JobId,Docket,WhenStarted,WhenEnded,Rate,Quantity,Comment")] InventoryHire hire)
        {
            if (ModelState.IsValid)
            {
                db.Entry(hire).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index", new { id = hire.JobId });
            }
            return View(hire);
        }

        // GET: /InventoryHire/Delete/5
        public async Task<ActionResult> Delete(int? id, int? hireId)
        {
            if (hireId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            InventoryHire hire = await db.InventoryHires.FindAsync(hireId);
            if (hire == null)
            {
                return HttpNotFound();
            }
            return View(hire);
        }

        // POST: /InventoryHire/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int hireId)
        {
            InventoryHire hire = await db.InventoryHires.FindAsync(hireId);
            db.InventoryHires.Remove(hire);
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

        private ActionResult View(InventoryHire hire)
        {
            var inventories = db.Inventories.Where(f => !f.WhenDisused.HasValue).OrderBy(f => f.XInventoryId);

            ViewBag.Inventories = new SelectList(inventories, "InventoryId", "XInventoryId", hire.InventoryId);

            return base.View(hire);
        }
    }
}
