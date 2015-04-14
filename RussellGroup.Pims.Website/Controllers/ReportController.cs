using DataTables.Mvc;
using RussellGroup.Pims.DataAccess.Models;
using RussellGroup.Pims.DataAccess.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using LinqKit;
using RussellGroup.Pims.Website.Models;
using RussellGroup.Pims.DataAccess.ReportModels;

namespace RussellGroup.Pims.Website.Controllers
{
    [PimsAuthorize(Role.CanView, Role.CanEdit, Role.CanEditCategories, Role.CanEditUsers)]
    public class ReportController : Controller
    {
        private readonly IReportRepository _repository;

        public ReportController(IReportRepository repository)
        {
            _repository = repository;
        }

        // filtered plant status chart data
        public JsonResult GetFilteredPlantData()
        {
            var plants = _repository.Plants;

            var unknown = plants.Count(f => f.StatusId == Status.Unknown);
            var available = plants.Count(f => f.StatusId == Status.Available);
            var checkedout = plants.Count(f => f.StatusId == Status.CheckedOut);
            var underRepair = plants.Count(f => f.StatusId == Status.UnderRepair);

            var data = new[]
            {
                new { key = available + " available", value = available },
                new { key = checkedout + " checked out", value = checkedout },
                new { key = unknown + " unknown", value = unknown },
                new { key = underRepair + " under repair", value = underRepair }
            };

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        // plant status chart data
        public JsonResult GetAllPlantData()
        {
            var plants = _repository.Plants;

            var unknown = plants.Count(f => f.StatusId == Status.Unknown);
            var available = plants.Count(f => f.StatusId == Status.Available);
            var checkedout = plants.Count(f => f.StatusId == Status.CheckedOut);
            var underRepair = plants.Count(f => f.StatusId == Status.UnderRepair);
            var stolen = plants.Count(f => f.StatusId == Status.Stolen);
            var writtenOff = plants.Count(f => f.StatusId == Status.WrittenOff);

            var data = new[]
            {
                new { key = available + " available", value = available },
                new { key = checkedout + " checked out", value = checkedout },
                new { key = unknown + " unknown", value = unknown },
                new { key = underRepair + " under repair", value = underRepair },
                new { key = stolen + " stolen", value = stolen },
                new { key = writtenOff + " written off", value = writtenOff }
            };

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        // plant condition chart data
        public JsonResult GetConditionData()
        {
            var plants = _repository.Plants.Where(f =>
                f.StatusId == Status.Available ||
                f.StatusId == Status.CheckedOut);

            var unknown = plants.Count(f => f.ConditionId == Condition.Unknown);
            var poor = plants.Count(f => f.ConditionId == Condition.Poor);
            var fair = plants.Count(f => f.ConditionId == Condition.Fair);
            var good = plants.Count(f => f.ConditionId == Condition.Good);
            var excellent = plants.Count(f => f.ConditionId == Condition.Excellent);

            var data = new[]
            {
                new { key = excellent + " excellent", value = excellent },
                new { key = good + " good", value = good },
                new { key = fair + " fair", value = fair },
                new { key = poor + " poor", value = poor },
                new { key = unknown + " unknown", value = unknown }
            };

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        // plant category chart data
        public JsonResult GetPlantCategoryData()
        {
            var data = _repository
                .Categories
                .Where(f => f.Type.Equals("plant", StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(f => f.Plants.Count)
                .ToList()
                .Select(f => new
                {
                    key = string.Format("{0} {1}", f.Plants.Count, f.Name),
                    value = f.Plants.Count
                });

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        // GET: /Report/Jobs
        public ActionResult Jobs()
        {
            return View("JobIndex");
        }

        // GET: /Report/Jobs
        public ActionResult JobsWithDateFilter()
        {
            var now = DateTime.Now.Date;
            ViewBag.WhenStarted = new DateTime(now.Year, now.Month, 1);
            ViewBag.WhenEnded = now;

            return View("JobIndexWithDateFilter");
        }

        // GET: /Report/Categories
        public async Task<ActionResult> Locations()
        {
            return View("LocationIndex", await _repository.Categories.ToListAsync());
        }

        // https://github.com/ALMMa/datatables.mvc
        public JsonResult GetDataTableResult([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest model)
        {
            if (model == null)
            {
                throw new ArgumentNullException("model");
            }

            var hint = model.Search != null ? model.Search.Value : string.Empty;
            bool isFiltered = bool.Parse(Request["filtered"] ?? false.ToString());
            var sortColumn = model.Columns.GetSortedColumns().First();

            var all = _repository.Jobs.AsExpandable();

            // filter
            var filtered = string.IsNullOrEmpty(hint)
                ? all
                : all.Where(f =>
                    f.XJobId.Contains(hint) ||
                    f.Description.Contains(hint) ||
                    Extensions.LittleEndianDateString.Invoke(f.WhenStarted).Contains(hint) ||
                    Extensions.LittleEndianDateString.Invoke(f.WhenEnded).Contains(hint));

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
                    PlantHires = c.PlantHires.Count(),
                    InventoryHires = c.CollatedInventoryHires.Sum(f => f.Quantity),
                    CrudLinks = isFiltered ?
                        string.Format("{0}&nbsp;| {1}",
                            string.Format("<a href=\"{0}\" target=\"_blank\">{1}</a>", Url.Action("PlantHireChargesInJob", new { id = c.Id }), "Plant&nbsp;Charges&nbsp;#50"),
                            string.Format("<a href=\"{0}\" target=\"_blank\">{1}</a>", Url.Action("InventoryHireChargesInJob", new { id = c.Id }), "Inventory&nbsp;Charges")
                        ) :
                        string.Format("{0}&nbsp;| {1}",
                            string.Format("<a href=\"{0}\" target=\"_blank\">{1}</a>", Url.Action("PlantInJob", new { id = c.Id }), "Plant&nbsp;#51"),
                            string.Format("<a href=\"{0}\" target=\"_blank\">{1}</a>", Url.Action("InventoryInJob", new { id = c.Id }), "Inventory&nbsp;#56")

                            // this report seems redundant
                            //string.Format("<a href=\"{0}\" target=\"_blank\">{1}</a>", Url.Action("InventoryStocktakeInJob", new { id = c.Id }), "Stocktake&nbsp;#70")
                        )
                });

            return Json(new DataTablesResponse(model.Draw, paged, filtered.Count(), all.Count()), JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> PlantLocations(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category category = await _repository.Categories.SingleOrDefaultAsync(f => f.Id == id);
            if (category == null)
            {
                return HttpNotFound();
            }
            return View(_repository.GetPlantLocationsByCategory(category.Id));
        }

        public async Task<ActionResult> InventoryLocations(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category category = await _repository.Categories.SingleOrDefaultAsync(f => f.Id == id);
            if (category == null)
            {
                return HttpNotFound();
            }
            return View(_repository.GetInventoryLocationsByCategory(category.Id));
        }

        public async Task<ActionResult> PlantInJob(int? id)
        {
            return await JobView("PlantInJob", id);
        }

        public async Task<ActionResult> PlantInJobChecklist(int? id)
        {
            return await JobView("PlantInJobChecklist", id);
        }

        public async Task<ActionResult> InventoryInJob(int? id)
        {
            return await JobView("InventoryInJob", id);
        }

        public async Task<ActionResult> InventoryStocktakeInJob(int? id)
        {
            return await JobView("InventoryStocktakeInJob", id);
        }

        public async Task<ActionResult> PlantHireChargesInJob(int? id)
        {
            return await JobView("PlantHireChargesInJob", id);
        }

        public async Task<ActionResult> InventoryHireChargesInJob(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var job = await _repository.Jobs.SingleOrDefaultAsync(f => f.Id == id);
            if (job == null)
            {
                return HttpNotFound();
            }

            var whenStarted = ParseDate(Request["WhenStarted"]);
            var whenEnded = ParseDate(Request["WhenEnded"]);

            var model = _repository.GetInventoryHireCharges(job, whenStarted, whenEnded);

            return base.View("InventoryHireChargesInJob", model);
        }

        public async Task<ActionResult> SummaryOfHireCharges()
        {
            var whenStarted = ParseDate(Request["WhenStarted"]);
            var whenEnded = ParseDate(Request["WhenEnded"]);

            var model = await _repository.GetSummaryOfHireChargesAsync(whenStarted, whenEnded);

            return base.View("SummaryOfHireCharges", model);
        }

        public async Task<FileContentResult> DownloadSummaryOfHireChargesCsv()
        {
            var fileName = string.Format("SummaryOfHireCharges-{0}.csv", DateTime.Now.ToString("yyyyMMddHHmmss"));

            var whenStarted = ParseDate(Request["WhenStarted"]);
            var whenEnded = ParseDate(Request["WhenEnded"]);

            var model = await _repository.GetSummaryOfHireChargesAsync(whenStarted, whenEnded);

            return File(_repository.SummaryOfHireChargesCsv(model), "text/csv", fileName);
        }

        public ActionResult YardStocktake()
        {
            return View("YardStocktake");
        }

        public async Task<ActionResult> PlantStatus()
        {
            return View("PlantStatus", await _repository.Plants.ToListAsync());
        }

        public async Task<ActionResult> YardPlantStocktake()
        {
            return View("YardPlantStocktake", await _repository.GetAvailablePlant().ToListAsync());
        }

        public async Task<ActionResult> YardInventoryStocktake()
        {
            return View("YardInventoryStocktake", await _repository.Inventories.ToListAsync());
        }

        private async Task<ActionResult> JobView(string viewName, int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var job = await _repository.Jobs.SingleOrDefaultAsync(f => f.Id == id);
            if (job == null)
            {
                return HttpNotFound();
            }

            ViewBag.WhenStarted = ParseDate(Request["WhenStarted"]);
            ViewBag.WhenEnded = ParseDate(Request["WhenEnded"]);

            return View(viewName, job);
        }

        private DateTime ParseDate(string value)
        {
            DateTime date;

            if (!DateTime.TryParse(value, MvcApplication.CULTURE_EN_NZ, MvcApplication.DATE_TIME_STYLES_NONE, out date))
            {
                date = DateTime.MinValue;
            }

            return date;
        }
    }
}