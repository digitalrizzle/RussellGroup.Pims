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
    [PimsAuthorize(Roles = new string[] { ApplicationRole.CanEdit })]
    public class PlantController : Controller
    {
        private readonly IPlantRepository repository;

        public PlantController(IPlantRepository repository)
        {
            this.repository = repository;
        }

        // GET: /Plant/
        public ActionResult Index()
        {
            return View();
        }

        // this method has been adapted from the code described here:
        // http://www.codeproject.com/KB/aspnet/JQuery-DataTables-MVC.aspx
        public JsonResult GetDataTableResult(JqueryDataTableParameterModel model)
        {
            IEnumerable<Plant> entries = repository.GetAll();
            var sortColumnIndex = model.iSortCol_0;

            // ordering
            Func<Plant, string> ordering = (c =>
                sortColumnIndex == 1 ? c.XPlantId :
                    sortColumnIndex == 2 ? c.XPlantNewId :
                        sortColumnIndex == 3 ? c.Description :
                            sortColumnIndex == 4 ? c.Category.Name : c.Status.Name);

            // sorting
            IEnumerable<Plant> ordered = model.sSortDir_0 == "asc" ?
                entries.OrderBy(ordering) :
                entries.OrderByDescending(ordering);

            // filter for sSearch
            string hint = (model.sSearch ?? string.Empty).ToUpperInvariant();
            IEnumerable<Plant> searched;

            if (string.IsNullOrEmpty(hint))
            {
                searched = ordered;
            }
            else
            {
                // don't include in the search the id as it is hidden from the display
                searched = ordered.Where(f =>
                    (f.XPlantId != null && f.XPlantId.ToUpperInvariant().Contains(hint)) ||
                    (f.XPlantNewId != null && f.XPlantNewId.ToUpperInvariant().Contains(hint)) ||
                    (f.Description != null && f.Description.ToUpperInvariant().Contains(hint)) ||
                    (f.Category != null && f.Category.Name.ToUpperInvariant().Contains(hint)) ||
                    (f.Status != null && f.Status.Name.ToUpperInvariant().Contains(hint))
                );
            }

            // filter for the display
            var filtered = searched
                .Skip(searched.Count() > model.iDisplayLength ? model.iDisplayStart : 0)
                .Take(searched.Count() > model.iDisplayLength ? model.iDisplayLength : searched.Count());

            // get the display values
            var displayData = filtered
                .Select(c => new string[]
                {
                    c.PlantId.ToString(),
                    c.XPlantId,
                    c.XPlantNewId,
                    c.Description,
                    c.Category != null ? c.Category.Name : string.Empty,
                    repository.GetJobs(c.PlantId).Count() == 0 ? string.Empty : this.ActionLink(repository.GetJobs(c.PlantId).Count().ToString(), "Jobs", new { id = c.PlantId }),
                    c.Status.Name,
                    this.CrudLinks(new { id = c.PlantId })
                });

            var result = new
            {
                sEcho = model.sEcho,
                iTotalRecords = repository.GetAll().Count(),
                iTotalDisplayRecords = searched.Count(),
                aaData = displayData
            };

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        // GET: /Plant/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Plant plant = await repository.Find(id);
            if (plant == null)
            {
                return HttpNotFound();
            }
            return View(plant);
        }

        // GET: /Plant/Jobs/5
        public async Task<ActionResult> Jobs(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Plant plant = await repository.Find(id);
            if (plant == null)
            {
                return HttpNotFound();
            }
            return View(plant);
        }

        // this method has been adapted from the code described here:
        // http://www.codeproject.com/KB/aspnet/JQuery-DataTables-MVC.aspx
        public JsonResult GetJobsDataTableResult(JqueryDataTableParameterModel model)
        {
            int id = Convert.ToInt32(Request["id"]);

            var entries = repository.GetJobs(id);
            var sortColumnIndex = model.iSortCol_0;

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
                    c.JobId.ToString(),
                    c.XJobId,
                    c.Description,
                    c.WhenStarted.HasValue ? c.WhenStarted.Value.ToShortDateString() : string.Empty,
                    c.WhenEnded.HasValue ? c.WhenEnded.Value.ToShortDateString() : string.Empty,
                    c.ProjectManager,
                    this.ActionLink("Details", "Details", "Job", new { id = c.JobId })
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
                iTotalRecords = entries.Count(),
                iTotalDisplayRecords = searched.Count(),
                aaData = filtered
            };

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        // GET: /Plant/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: /Plant/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "PlantId,CategoryId,StatusId,XPlantId,XPlantNewId,Description,WhenPurchased,WhenDisused,Rate,Cost,Serial,FixedAssetCode,IsElectrical,IsTool,Comment")] Plant plant)
        {
            if (ModelState.IsValid)
            {
                await repository.Add(plant);
                return RedirectToAction("Index");
            }

            return View(plant);
        }

        // GET: /Plant/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Plant plant = await repository.Find(id);
            if (plant == null)
            {
                return HttpNotFound();
            }
            return View(plant);
        }

        // POST: /Plant/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "PlantId,CategoryId,StatusId,XPlantId,XPlantNewId,Description,WhenPurchased,WhenDisused,Rate,Cost,Serial,FixedAssetCode,IsElectrical,IsTool,Comment")] Plant plant)
        {
            if (ModelState.IsValid)
            {
                await repository.Update(plant);
                return RedirectToAction("Index");
            }
            return View(plant);
        }

        // GET: /Plant/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Plant plant = await repository.Find(id);
            if (plant == null)
            {
                return HttpNotFound();
            }
            return View(plant);
        }

        // POST: /Plant/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            await repository.Remove(id);
            return RedirectToAction("Index");
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
            return View(null);
        }

        private ActionResult View(Plant plant)
        {
            var categories = repository.Categories.OrderBy(f => f.Name);
            var category = plant != null ? plant.CategoryId : 0;

            var statuses = repository.Statuses.OrderBy(f => f.StatusId);
            var status = plant != null ? plant.StatusId : 0;

            ViewBag.Categories = new SelectList(categories, "CategoryId", "Name", category);
            ViewBag.Statuses = new SelectList(statuses, "StatusId", "Name", status);

            return base.View(plant);
        }
    }
}
