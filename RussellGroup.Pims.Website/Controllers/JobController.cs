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
    public class JobController : Controller
    {
        private readonly IJobRepository _repository;

        public JobController(IJobRepository _repository)
        {
            this._repository = _repository;
        }

        // GET: /Job/
        public ActionResult Index()
        {
            return View("Index");
        }

        // this method has been adapted from the code described here:
        // http://www.codeproject.com/KB/aspnet/JQuery-DataTables-MVC.aspx
        public JsonResult GetDataTableResult(JqueryDataTableParameterModel model)
        {
            IQueryable<Job> entries = _repository.GetAll();
            var sortColumnIndex = model.iSortCol_0;
            var canEdit = User.IsAuthorized(Role.CanEdit);

            // ordering
            Func<Job, string> ordering = (c =>
                sortColumnIndex == 1 ? c.XJobId :
                    sortColumnIndex == 2 ? c.Description :
                        sortColumnIndex == 3 ? (c.WhenStarted.HasValue ? c.WhenStarted.Value.ToString(MvcApplication.DATE_TIME_FORMAT) : string.Empty) :
                            sortColumnIndex == 4 ? (c.WhenEnded.HasValue ? c.WhenEnded.Value.ToString(MvcApplication.DATE_TIME_FORMAT) : string.Empty) : c.ProjectManager);

            // sorting
            IEnumerable<Job> ordered = model.sSortDir_0 == "asc" ?
                entries.OrderBy(ordering) :
                entries.OrderByDescending(ordering);

            // get the display values
            var displayData = ordered
                .Select(c => new string[]
                {
                    c.Id.ToString(),
                    c.XJobId,
                    c.Description,
                    c.WhenStarted.HasValue ? c.WhenStarted.Value.ToShortDateString() : string.Empty,
                    c.WhenEnded.HasValue ? c.WhenEnded.Value.ToShortDateString() : string.Empty,
                    c.ProjectManager,
                    this.CrudAndHireLinks(c.WhenEnded.HasValue, new { id = c.Id }, canEdit)
                });

            // filter for sSearch
            string hint = model.sSearch;
            List<string[]> searched = new List<string[]>();

            if (string.IsNullOrEmpty(hint))
            {
                searched.AddRange(displayData);
            }
            else
            {
                foreach (string[] row in displayData)
                {
                    // don't include in the search the id as it is hidden from the display
                    // don't include in the search the CRUD links either
                    for (int index = 0; index < row.Length - 1; index++)
                    {
                        if (!string.IsNullOrEmpty(row[index]) && row[index].IndexOf(hint, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            searched.Add(row);
                            break;
                        }
                    }
                }
            }

            // filter for the display
            var filtered = searched
                .Skip(searched.Count > model.iDisplayLength ? model.iDisplayStart : 0)
                .Take(searched.Count > model.iDisplayLength ? model.iDisplayLength : searched.Count);

            var result = new
            {
                sEcho = model.sEcho,
                iTotalRecords = _repository.GetAll().Count(),
                iTotalDisplayRecords = searched.Count(),
                aaData = filtered
            };

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        // GET: /Job/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Job job = await _repository.FindAsync(id);
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
            return View();
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
            Job job = await _repository.FindAsync(id);
            if (job == null)
            {
                return HttpNotFound();
            }
            return View(job);
        }

        // POST: /Job/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PimsAuthorize(Role.CanEdit)]
        public async Task<ActionResult> Edit([Bind(Include = "Id,XJobId,Description,WhenStarted,WhenEnded,ProjectManager,QuantitySurveyor,Comment")] Job job)
        {
            if (ModelState.IsValid)
            {
                await _repository.UpdateAsync(job);
                return RedirectToAction("Index");
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
            Job job = await _repository.FindAsync(id);
            if (job == null)
            {
                return HttpNotFound();
            }
            return View(job);
        }

        // POST: /Job/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [PimsAuthorize(Role.CanEdit)]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            await _repository.RemoveAsync(id);
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _repository.Dispose();
            }
            base.Dispose(disposing);
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
