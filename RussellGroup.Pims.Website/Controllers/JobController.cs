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

namespace RussellGroup.Pims.Website.Controllers
{
    [PimsAuthorize(Role.CanView, Role.CanEdit)]
    public class JobController : Controller
    {
        private readonly IJobRepository _repository;

        public JobController(IJobRepository repository)
        {
            _repository = repository;
        }

        // GET: /Job/
        public ActionResult Index()
        {
            return View("Index");
        }

        // https://github.com/ALMMa/datatables.mvc
        public JsonResult GetDataTableResult([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest model)
        {
            if (model == null)
            {
                throw new ArgumentNullException("model");
            }

            var hint = model.Search != null ? model.Search.Value : string.Empty;
            var sortColumn = model.Columns.GetSortedColumns().First();

            var all = _repository.GetAll().AsExpandable();

            // filter
            var filtered = string.IsNullOrEmpty(hint)
                ? all
                : all.Where(f =>
                    f.XJobId.Contains(hint) ||
                    f.Description.Contains(hint) ||
                    Extensions.LittleEndianDateString.Invoke(f.WhenStarted).Contains(hint) ||
                    Extensions.LittleEndianDateString.Invoke(f.WhenEnded).Contains(hint) ||
                    f.ProjectManager.Contains(hint));

            // ordering
            var sortColumnName = string.IsNullOrEmpty(sortColumn.Name) ? sortColumn.Data : sortColumn.Name;
            Func<Job, IComparable> ordering = (c => c.GetValue(sortColumnName.Split('.')));

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
                    c.XJobId,
                    c.Description,
                    WhenStarted = c.WhenStarted.HasValue ? c.WhenStarted.Value.ToShortDateString() : string.Empty,
                    WhenEnded = c.WhenEnded.HasValue ? c.WhenEnded.Value.ToShortDateString() : string.Empty,
                    c.ProjectManager,
                    CrudLinks = CrudAndHireLinks(c.WhenEnded.HasValue, new { id = c.Id }, User.IsAuthorized(Role.CanEdit))
                });

            return Json(new DataTablesResponse(model.Draw, paged, filtered.Count(), all.Count()), JsonRequestBehavior.AllowGet);
        }

        // GET: /Job/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var job = await _repository.FindAsync(id);
            if (job == null)
            {
                return HttpNotFound();
            }
            return View("Details", job);
        }

        // GET: /Job/Create
        [PimsAuthorize(Role.CanEdit)]
        public ActionResult Create()
        {
            var job = new Job { WhenStarted = DateTime.Now };
            return View("Create", job);
        }

        // POST: /Job/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PimsAuthorize(Role.CanEdit)]
        public async Task<ActionResult> Create([Bind(Include = "XJobId,Description,WhenStarted,WhenEnded,ProjectManager,QuantitySurveyor,Comment")] Job job)
        {
            if (ModelState.IsValid)
            {
                await _repository.AddAsync(job);
                return RedirectToAction("Index");
            }

            return View("Create", job);
        }

        // GET: /Job/Edit/5
        [PimsAuthorize(Role.CanEdit)]
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var job = await _repository.FindAsync(id);
            if (job == null)
            {
                return HttpNotFound();
            }
            return View("Edit", job);
        }

        // POST: /Job/Edit/5
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
            var job = await _repository.FindAsync(id);
            if (job == null)
            {
                return HttpNotFound();
            }

            if (TryUpdateModel<Job>(job, "XJobId,Description,WhenStarted,WhenEnded,ProjectManager,QuantitySurveyor,Comment".Split(',')))
            {
                if (ModelState.IsValid)
                {
                    await _repository.UpdateAsync(job);
                    return RedirectToAction("Index");
                }
            }

            return View("Edit", job);
        }

        public JsonResult GetProjectManagerSuggestions()
        {
            string hint = Request["q"];

            var result = _repository
                .GetAll()
                .Where(f => f.ProjectManager.Contains(hint))
                .OrderBy(f => f.ProjectManager)
                .Select(f => new { value = f.ProjectManager })
                .Distinct()
                .Take(5)
                .ToArray();

            var json = Json(result, JsonRequestBehavior.AllowGet);

            return json;
        }

        public JsonResult GetQuantitySurveyorSuggestions()
        {
            string hint = Request["q"];

            var result = _repository
                .GetAll()
                .Where(f => f.QuantitySurveyor.Contains(hint))
                .OrderBy(f => f.QuantitySurveyor)
                .Select(f => new { value = f.QuantitySurveyor })
                .Distinct()
                .Take(5)
                .ToArray();

            var json = Json(result, JsonRequestBehavior.AllowGet);

            return json;
        }

        // GET: /Job/Delete/5
        [PimsAuthorize(Role.CanEdit)]
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var job = await _repository.FindAsync(id);
            if (job == null)
            {
                return HttpNotFound();
            }
            return View("Delete", job);
        }

        // POST: /Job/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [PimsAuthorize(Role.CanEdit)]
        public async Task<ActionResult> DeleteConfirmed(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var job = await _repository.FindAsync(id);
            if (job == null)
            {
                return HttpNotFound();
            }
            await _repository.RemoveAsync(job);
            return RedirectToAction("Index");
        }

        private string CrudAndHireLinks(bool isComplete, object routeValues, bool canEdit)
        {
            var links = this.CrudLinks(routeValues, canEdit);

            if (!isComplete && canEdit)
            {
                var checkin = string.Format("<a href=\"{0}\">{1}</a>", Url.Action("Checkin", "Hire", routeValues), "Checkin");
                var checkout = string.Format("<a href=\"{0}\">{1}</a>", Url.Action("Checkout", "Hire", routeValues), "Checkout");

                links = string.Format("{0} | {1} | {2}", checkout, checkin, links);
            }

            return links;
        }
    }
}
