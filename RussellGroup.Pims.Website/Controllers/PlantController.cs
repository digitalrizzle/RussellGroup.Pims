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
    public class PlantController : Controller
    {
        private readonly IPlantRepository _repository;

        public PlantController(IPlantRepository repository)
        {
            _repository = repository;
        }

        // GET: /Plant/
        public ActionResult Index()
        {
            return View();
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
                    f.XPlantId.Contains(hint) ||
                    f.XPlantNewId.Contains(hint) ||
                    f.Category.Name.Contains(hint) ||
                    f.Status.Name.Contains(hint));

            // ordering
            var sortColumnName = string.IsNullOrEmpty(sortColumn.Name) ? sortColumn.Data : sortColumn.Name;
            Func<Plant, IComparable> ordering = (c => c.GetValue(sortColumnName.Split('.')));

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
                    c.XPlantId,
                    c.XPlantNewId,
                    c.Description,
                    Category = c.Category != null ? c.Category.Name : string.Empty,
                    Jobs = _repository.GetJobs(c.Id).Count() == 0 ? string.Empty : this.ActionLink(_repository.GetJobs(c.Id).Count().ToString(), "Jobs", new { id = c.Id }),
                    Status = c.Status.Name,
                    CrudLinks = this.CrudLinks(new { id = c.Id }, User.IsAuthorized(Role.CanEdit))
                });

            return Json(new DataTablesResponse(model.Draw, paged, filtered.Count(), all.Count()), JsonRequestBehavior.AllowGet);
        }

        // GET: /Plant/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Plant plant = await _repository.FindAsync(id);
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
            Plant plant = await _repository.FindAsync(id);
            if (plant == null)
            {
                return HttpNotFound();
            }
            return View(plant);
        }

        // https://github.com/ALMMa/datatables.mvc
        public JsonResult GetJobDataTableResult([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest model)
        {
            if (model == null)
            {
                throw new ArgumentNullException("model");
            }

            var id = Convert.ToInt32(Request["id"]);
            var hint = model.Search != null ? model.Search.Value : string.Empty;
            var sortColumn = model.Columns.GetSortedColumns().First();

            var all = _repository.GetJobs(id).AsExpandable();

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
                .Select(f => new
                {
                    f.Id,
                    f.XJobId,
                    f.Description,
                    WhenStarted = f.WhenStarted.HasValue ? f.WhenStarted.Value.ToShortDateString() : string.Empty,
                    WhenEnded = f.WhenEnded.HasValue ? f.WhenEnded.Value.ToShortDateString() : string.Empty,
                    f.ProjectManager,
                    CrudLinks = this.CrudLinks(new { id = f.Id }, User.IsAuthorized(Role.CanEdit))
                });

            return Json(new DataTablesResponse(model.Draw, paged, filtered.Count(), all.Count()), JsonRequestBehavior.AllowGet);
        }

        // GET: /Plant/Create
        [PimsAuthorize(Role.CanEdit)]
        public ActionResult Create()
        {
            var plant = new Plant { WhenPurchased = DateTime.Now };
            return View(plant);
        }

        // POST: /Plant/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PimsAuthorize(Role.CanEdit)]
        public async Task<ActionResult> Create([Bind(Include = "CategoryId,StatusId,ConditionId,XPlantId,XPlantNewId,Description,WhenPurchased,WhenDisused,Rate,Cost,Serial,FixedAssetCode,IsElectrical,IsTool,Comment")] Plant plant)
        {
            if (ModelState.IsValid)
            {
                await _repository.AddAsync(plant);
                return RedirectToAction("Index");
            }

            return View(plant);
        }

        // GET: /Plant/Edit/5
        [PimsAuthorize(Role.CanEdit)]
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Plant plant = await _repository.FindAsync(id);
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
        [PimsAuthorize(Role.CanEdit)]
        public async Task<ActionResult> Edit([Bind(Include = "Id,CategoryId,StatusId,ConditionId,XPlantId,XPlantNewId,Description,WhenPurchased,WhenDisused,Rate,Cost,Serial,FixedAssetCode,IsElectrical,IsTool,Comment")] Plant plant)
        {
            // TODO: prevent status change to unavailable
            if (ModelState.IsValid)
            {
                await _repository.UpdateAsync(plant);
                return RedirectToAction("Index");
            }
            return View(plant);
        }

        // GET: /Plant/Delete/5
        [PimsAuthorize(Role.CanEdit)]
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Plant plant = await _repository.FindAsync(id);
            if (plant == null)
            {
                return HttpNotFound();
            }
            return View(plant);
        }

        // POST: /Plant/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [PimsAuthorize(Role.CanEdit)]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            await _repository.RemoveAsync(id);
            return RedirectToAction("Index");
        }

        private new ActionResult View()
        {
            return View(null);
        }

        private ActionResult View(Plant plant)
        {
            ViewBag.IsUnavailable = plant != null ? plant.PlantHires.Any(f => !f.WhenEnded.HasValue) : false;

            var categories = _repository.Categories.OrderBy(f => f.Name);
            var category = plant != null ? plant.CategoryId : 0;
            ViewBag.Categories = new SelectList(categories, "Id", "Name", category);

            var statuses = _repository.Statuses.OrderBy(f => f.Id);
            var status = plant != null ? plant.StatusId : 0;
            ViewBag.Statuses = new SelectList(statuses, "Id", "Name", status);

            var conditions = _repository.Conditions.OrderBy(f => f.Id);
            var condition = plant != null ? plant.ConditionId : 0;
            ViewBag.Conditions = new SelectList(conditions, "Id", "Name", condition);

            return base.View(plant);
        }
    }
}
