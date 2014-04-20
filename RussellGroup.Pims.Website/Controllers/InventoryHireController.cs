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
using RussellGroup.Pims.DataAccess.Respositories;

namespace RussellGroup.Pims.Website.Controllers
{
    [PimsAuthorize(Role.CanView, Role.CanEdit)]
    public class InventoryHireController : Controller
    {
        private readonly IHireRepository<InventoryHire> repository;

        public InventoryHireController(IHireRepository<InventoryHire> repository)
        {
            this.repository = repository;
        }

        // GET: /InventoryHire/5
        public async Task<ActionResult> Index(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var job = await repository.GetJob(id);
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
            InventoryHire hire = await repository.Find(hireId);
            if (hire == null)
            {
                return HttpNotFound();
            }
            return View(hire);
        }

        // GET: /InventoryHire/Create/5
        [PimsAuthorize(Role.CanEdit)]
        public async Task<ActionResult> Create(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var job = await repository.GetJob(id);
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
        [PimsAuthorize(Role.CanEdit)]
        public async Task<ActionResult> Create([Bind(Include = "InventoryHireId,InventoryId,JobId,Docket,WhenStarted,WhenEnded,Rate,Quantity,Comment")] InventoryHire hire)
        {
            if (ModelState.IsValid)
            {
                await repository.Add(hire);
                return RedirectToAction("Index");
            }
            return View(hire);
        }

        // GET: /InventoryHire/Edit/5
        [PimsAuthorize(Role.CanEdit)]
        public async Task<ActionResult> Edit(int? id, int? hireId)
        {
            if (hireId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            InventoryHire hire = await repository.Find(hireId);
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
        [PimsAuthorize(Role.CanEdit)]
        public async Task<ActionResult> Edit([Bind(Include = "InventoryHireId,InventoryId,JobId,Docket,WhenStarted,WhenEnded,Rate,Quantity,Comment")] InventoryHire hire)
        {
            if (ModelState.IsValid)
            {
                await repository.Update(hire);
                return RedirectToAction("Index", new { id = hire.JobId });
            }
            return View(hire);
        }

        // GET: /InventoryHire/Delete/5
        [PimsAuthorize(Role.CanEdit)]
        public async Task<ActionResult> Delete(int? id, int? hireId)
        {
            if (hireId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            InventoryHire hire = await repository.Find(hireId);
            if (hire == null)
            {
                return HttpNotFound();
            }
            return View(hire);
        }

        // POST: /InventoryHire/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [PimsAuthorize(Role.CanEdit)]
        public async Task<ActionResult> DeleteConfirmed(int hireId)
        {
            int id = (await repository.Find(hireId)).JobId;
            await repository.Remove(hireId);
            return RedirectToAction("Index", new { id = id });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                repository.Dispose();
            }
            base.Dispose(disposing);
        }

        private new ActionResult View()
        {
            throw new NotSupportedException();
        }

        private ActionResult View(InventoryHire hire)
        {
            var inventories = repository.Inventories.Where(f => !f.WhenDisused.HasValue).OrderBy(f => f.XInventoryId);

            ViewBag.Inventories = new SelectList(inventories, "InventoryId", "XInventoryId", hire.InventoryId);

            return base.View(hire);
        }
    }
}
