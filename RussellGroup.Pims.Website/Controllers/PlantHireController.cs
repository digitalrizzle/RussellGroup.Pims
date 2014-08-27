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
using LinqKit;
using DataTables.Mvc;
using System.Data.Entity.SqlServer;

namespace RussellGroup.Pims.Website.Controllers
{
    [PimsAuthorize(Role.CanView, Role.CanEdit)]
    public class PlantHireController : Controller
    {
        private readonly IHireRepository<PlantHire> _repository;

        public PlantHireController(IHireRepository<PlantHire> repository)
        {
            _repository = repository;
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

            return View("Index", job);
        }

        // https://github.com/ALMMa/datatables.mvc
        public JsonResult GetDataTableResult([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest model)
        {
            if (model == null)
            {
                throw new ArgumentNullException("model");
            }

            var id = Convert.ToInt32(Request["id"]);
            var hint = model.Search != null ? model.Search.Value : string.Empty;
            var sortColumn = model.Columns.GetSortedColumns().First();

            var all = _repository.GetAll().AsExpandable().Where(f => f.JobId == id);

            // filter
            var filtered = string.IsNullOrEmpty(hint)
                ? all
                : all.Where(f =>
                    f.Plant.XPlantId.Contains(hint) ||
                    f.Docket.Contains(hint) ||
                    Extensions.LittleEndianDateString.Invoke(f.WhenStarted).Contains(hint) ||
                    Extensions.LittleEndianDateString.Invoke(f.WhenEnded).Contains(hint) ||
                    SqlFunctions.StringConvert(f.Rate).Contains(hint) ||
                    f.Comment.Contains(hint));

            // ordering
            var sortColumnName = string.IsNullOrEmpty(sortColumn.Name) ? sortColumn.Data : sortColumn.Name;
            Func<PlantHire, IComparable> ordering = (c => c.GetValue(sortColumnName.Split('.')));

            // sorting
            var sorted = sortColumn.SortDirection == Column.OrderDirection.Ascendant
                ? filtered.OrderBy(ordering)
                : filtered.OrderByDescending(ordering);

            var paged = sorted
                .Skip(model.Start)
                .Take(model.Length)
                .ToList()
                .Select(c => new
                {
                    c.Id,
                    c.Plant.XPlantId,
                    c.Docket,
                    WhenStarted = c.WhenStarted.HasValue ? c.WhenStarted.Value.ToShortDateString() : string.Empty,
                    WhenEnded = c.WhenEnded.HasValue ? c.WhenEnded.Value.ToShortDateString() : string.Empty,
                    c.Rate,
                    c.Comment,
                    CrudLinks = this.CrudLinks(new { id = c.JobId, hireId = c.Id }, User.IsAuthorized(Role.CanEdit))
                });

            return Json(new DataTablesResponse(model.Draw, paged, filtered.Count(), all.Count()), JsonRequestBehavior.AllowGet);
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

            return View("Details", hire);
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

            return View("Create", hire);
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
            return View("Create", hire);
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
            return View("Edit", hire);
        }

        // POST: /PlantHire/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PimsAuthorize(Role.CanEdit)]
        public async Task<ActionResult> Edit(int? id, int? hireId, FormCollection collection)
        {
            if (hireId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var hire = await _repository.FindAsync(hireId);
            if (hire == null)
            {
                return HttpNotFound();
            }

            if (TryUpdateModel<PlantHire>(hire, "PlantId,JobId,Docket,ReturnDocket,WhenStarted,WhenEnded,Rate,Comment".Split(',')))
            {
                if (ModelState.IsValid)
                {
                    await _repository.UpdateAsync(hire);
                    return RedirectToAction("Index", new { id = hire.JobId });
                }
            }

            return View("Edit", hire);
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
            return View("Delete", hire);
        }

        // POST: /PlantHire/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [PimsAuthorize(Role.CanEdit)]
        public async Task<ActionResult> DeleteConfirmed(int? hireId)
        {
            if (hireId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var hire = await _repository.FindAsync(hireId);
            if (hire == null)
            {
                return HttpNotFound();
            }
            await _repository.RemoveAsync(hire);
            return RedirectToAction("Index", new { id = hire.JobId });
        }

        private ActionResult View(string viewName, PlantHire hire)
        {
            var plants = _repository.Plants.Where(f => !f.WhenDisused.HasValue).OrderBy(f => f.XPlantId);
            var plant = hire != null ? hire.PlantId : 0;
            ViewBag.Plants = new SelectList(plants, "Id", "XPlantId", hire.PlantId);

            return base.View(viewName, hire);
        }
    }
}
