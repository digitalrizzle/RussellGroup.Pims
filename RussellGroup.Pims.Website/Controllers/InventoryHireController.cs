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
using DataTables.Mvc;
using LinqKit;
using System.Data.Entity.SqlServer;

namespace RussellGroup.Pims.Website.Controllers
{
    [PimsAuthorize(Role.CanView, Role.CanEdit)]
    public class InventoryHireController : Controller
    {
        private readonly IHireRepository<InventoryHire> _repository;

        public InventoryHireController(IHireRepository<InventoryHire> repository)
        {
            _repository = repository;
        }

        // GET: /InventoryHire/5
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
                    f.Inventory.XInventoryId.Contains(hint) ||
                    f.Docket.Contains(hint) ||
                    Extensions.LittleEndianDateString.Invoke(f.WhenStarted).Contains(hint) ||
                    Extensions.LittleEndianDateString.Invoke(f.WhenEnded).Contains(hint) ||
                    SqlFunctions.StringConvert(f.Rate).Contains(hint) ||
                    SqlFunctions.StringConvert((double)f.Quantity).Contains(hint));

            // ordering
            var sortColumnName = string.IsNullOrEmpty(sortColumn.Name) ? sortColumn.Data : sortColumn.Name;
            Func<InventoryHire, IComparable> ordering = (c => c.GetValue(sortColumnName.Split('.')));

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
                    c.Inventory.XInventoryId,
                    c.Docket,
                    WhenStarted = c.WhenStarted.HasValue ? c.WhenStarted.Value.ToShortDateString() : string.Empty,
                    WhenEnded = c.WhenEnded.HasValue ? c.WhenEnded.Value.ToShortDateString() : string.Empty,
                    c.Rate,
                    c.Quantity,
                    CrudLinks = this.CrudLinks(new { id = c.JobId, hireId = c.Id }, User.IsAuthorized(Role.CanEdit))
                });

            return Json(new DataTablesResponse(model.Draw, paged, filtered.Count(), all.Count()), JsonRequestBehavior.AllowGet);
        }

        // GET: /InventoryHire/Details/5
        public async Task<ActionResult> Details(int? id, int? hireId)
        {
            if (hireId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            InventoryHire hire = await _repository.FindAsync(hireId);
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
            var job = await _repository.GetJob(id);
            if (job == null)
            {
                return HttpNotFound();
            }

            var hire = new InventoryHire()
            {
                Job = job,
                JobId = job.Id,
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
        public async Task<ActionResult> Create([Bind(Include = "InventoryId,JobId,Docket,ReturnDocket,WhenStarted,WhenEnded,Rate,Quantity,Comment")] InventoryHire hire)
        {
            if (ModelState.IsValid)
            {
                await _repository.AddAsync(hire);
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
            InventoryHire hire = await _repository.FindAsync(hireId);
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
        public async Task<ActionResult> Edit(int? id, FormCollection collection)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var hire = await _repository.FindAsync(id);
            if (hire == null)
            {
                return HttpNotFound();
            }

            if (TryUpdateModel<InventoryHire>(hire, "Id,InventoryId,JobId,Docket,ReturnDocket,WhenStarted,WhenEnded,Rate,Quantity,Comment".Split(',')))
            {
                if (ModelState.IsValid)
                {
                    await _repository.UpdateAsync(hire);
                    return RedirectToAction("Index", new { id = hire.JobId });
                }
            }

            return View("Edit", hire);
        }

        // GET: /InventoryHire/Delete/5
        [PimsAuthorize(Role.CanEdit)]
        public async Task<ActionResult> Delete(int? id, int? hireId)
        {
            if (hireId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            InventoryHire hire = await _repository.FindAsync(hireId);
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

        private new ActionResult View()
        {
            throw new NotSupportedException();
        }

        private ActionResult View(InventoryHire hire)
        {
            var inventories = _repository.Inventories.Where(f => !f.WhenDisused.HasValue).OrderBy(f => f.XInventoryId);

            ViewBag.Inventories = new SelectList(inventories, "Id", "XInventoryId", hire.InventoryId);

            return base.View(hire);
        }
    }
}
