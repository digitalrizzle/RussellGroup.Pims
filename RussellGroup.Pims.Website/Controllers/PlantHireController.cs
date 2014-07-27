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
using RussellGroup.Pims.DataAccess.Repositories;

namespace RussellGroup.Pims.Website.Controllers
{
    [PimsAuthorize(Role.CanView, Role.CanEdit)]
    public class PlantHireController : Controller
    {
        private readonly IHireRepository<PlantHire> _repository;

        public PlantHireController(IHireRepository<PlantHire> _repository)
        {
            this._repository = _repository;
        }

        // GET: /PlantHire/5
        public async Task<ActionResult> Index(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var job = await _repository.GetJob(id);
            if (job == null)
            {
                return HttpNotFound();
            }

            return View(job);
        }

        // GET: /PlantHire/Details/5
        public async Task<ActionResult> Details(int? id, int? hireId)
        {
            if (hireId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PlantHire hire = await _repository.FindAsync(hireId);
            if (hire == null)
            {
                return HttpNotFound();
            }

            return View(hire);
        }

        // GET: /PlantHire/Create/5
        [PimsAuthorize(Role.CanEdit)]
        public async Task<ActionResult> Create(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var job = await _repository.GetJob(id);
            if (job == null)
            {
                return HttpNotFound();
            }

            var hire = new PlantHire() 
            {
                Job = job,
                JobId = job.Id,
                WhenStarted = DateTime.Now
            };

            return View(hire);
        }

        // POST: /PlantHire/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PimsAuthorize(Role.CanEdit)]
        public async Task<ActionResult> Create([Bind(Include = "PlantId,JobId,Docket,ReturnDocket,WhenStarted,WhenEnded,Rate,Comment")] PlantHire hire)
        {
            if (ModelState.IsValid)
            {
                await _repository.AddAsync(hire);
                return RedirectToAction("Index", new { id = hire.JobId });
            }
            return View(hire);
        }

        // GET: /PlantHire/Edit/5
        [PimsAuthorize(Role.CanEdit)]
        public async Task<ActionResult> Edit(int? id, int? hireId)
        {
            if (hireId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PlantHire hire = await _repository.FindAsync(hireId);
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
        [PimsAuthorize(Role.CanEdit)]
        public async Task<ActionResult> Edit([Bind(Include = "Id,PlantId,JobId,Docket,ReturnDocket,WhenStarted,WhenEnded,Rate,Comment")] PlantHire hire)
        {
            if (ModelState.IsValid)
            {
                await _repository.UpdateAsync(hire);
                return RedirectToAction("Index", new { id = hire.JobId });
            }
            return View(hire);
        }

        // GET: /PlantHire/Delete/5
        [PimsAuthorize(Role.CanEdit)]
        public async Task<ActionResult> Delete(int? id, int? hireId)
        {
            if (hireId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PlantHire hire = await _repository.FindAsync(hireId);
            if (hire == null)
            {
                return HttpNotFound();
            }
            return View(hire);
        }

        // POST: /PlantHire/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [PimsAuthorize(Role.CanEdit)]
        public async Task<ActionResult> DeleteConfirmed(int hireId)
        {
            int id = (await _repository.FindAsync(hireId)).JobId;
            await _repository.RemoveAsync(hireId);
            return RedirectToAction("Index", new { id = id });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _repository.Dispose();
            }
            base.Dispose(disposing);
        }

        private new ActionResult View()
        {
            throw new NotSupportedException();
        }

        private ActionResult View(PlantHire hire)
        {
            var plants = _repository.Plants.Where(f => !f.WhenDisused.HasValue).OrderBy(f => f.XPlantId);

            ViewBag.Plants = new SelectList(plants, "Id", "XPlantId", hire.PlantId);

            return base.View(hire);
        }
    }
}
