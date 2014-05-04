using RussellGroup.Pims.DataAccess.Models;
using RussellGroup.Pims.DataAccess.Respositories;
using RussellGroup.Pims.DataAccess.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace RussellGroup.Pims.Website.Controllers
{
    [PimsAuthorize(Role.CanView, Role.CanEdit)]
    public class ReportController : Controller
    {
        private static readonly CultureInfo culture = CultureInfo.CreateSpecificCulture("en-NZ");
        private static readonly DateTimeStyles styles = DateTimeStyles.None;

        private readonly IReportRepository _repository;

        public ReportController(IReportRepository _repository)
        {
            this._repository = _repository;
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
            ViewBag.WhenStarted = now.AddMonths(-1);
            ViewBag.WhenEnded = now;

            return View("JobIndexWithDateFilter");
        }

        // GET: /Report/Categories
        public async Task<ActionResult> Categories()
        {
            return View("CategoryIndex", await _repository.Categories.ToListAsync());
        }

        // this method has been adapted from the code described here:
        // http://www.codeproject.com/KB/aspnet/JQuery-DataTables-MVC.aspx
        public JsonResult GetDataTableResult(JqueryDataTableParameterModel model)
        {
            IEnumerable<Job> entries = _repository.Jobs;
            bool isDetailed = bool.Parse(Request["bDetailed"] ?? false.ToString());
            var sortColumnIndex = int.Parse(Request["iSortCol_0"]);

            // ordering
            Func<Job, string> ordering = (c =>
                sortColumnIndex == 1 ? c.XJobId :
                    sortColumnIndex == 2 ? c.Description :
                        sortColumnIndex == 3 ? (c.WhenStarted.HasValue ? c.WhenStarted.Value.ToString(MvcApplication.DATE_TIME_FORMAT) : string.Empty) :
                            sortColumnIndex == 4 ? (c.WhenEnded.HasValue ? c.WhenEnded.Value.ToString(MvcApplication.DATE_TIME_FORMAT) : string.Empty) : c.ProjectManager);

            // sorting
            IEnumerable<Job> ordered = Request["sSortDir_0"] == "asc" ?
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
                    null,
                    null,
                    isDetailed ?
                        string.Format("{0}",
                            string.Format("<a href=\"{0}\" target=\"_blank\">{1}</a>", Url.Action("PlantHireChargesInJob", new { id = c.JobId }), "Plant Charges #50")
                        ) :
                        string.Format("{0} | {1} | {2}",
                            string.Format("<a href=\"{0}\" target=\"_blank\">{1}</a>", Url.Action("PlantInJob", new { id = c.JobId }), "Plant #51"),
                            string.Format("<a href=\"{0}\" target=\"_blank\">{1}</a>", Url.Action("InventoryInJob", new { id = c.JobId }), "Inventory #56"),
                            string.Format("<a href=\"{0}\" target=\"_blank\">{1}</a>", Url.Action("InventoryStocktakeInJob", new { id = c.JobId }), "Stocktake #70")
                        )
                });

            // filter for sSearch
            string hint = Request["sSearch"];
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

            // add the counts of plant and inventory items
            foreach (string[] row in filtered)
            {
                int id = Convert.ToInt32(row[0]);
                var job = _repository.Jobs.Single(f => f.JobId == id);

                row[6] = job.PlantHires.Count(f => !f.WhenEnded.HasValue).ToString();
                row[7] = job.InventoryHires.Count(f => !f.WhenEnded.HasValue).ToString();
            }

            var result = new
            {
                sEcho = model.sEcho,
                iTotalRecords = _repository.Jobs.Count(),
                iTotalDisplayRecords = searched.Count(),
                aaData = filtered
            };

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> PlantLocations(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category category = await _repository.Categories.SingleOrDefaultAsync(f => f.CategoryId == id);
            if (category == null)
            {
                return HttpNotFound();
            }
            return View(_repository.GetPlantLocationsByCategory(category.CategoryId));
        }

        public async Task<ActionResult> InventoryLocations(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category category = await _repository.Categories.SingleOrDefaultAsync(f => f.CategoryId == id);
            if (category == null)
            {
                return HttpNotFound();
            }
            return View(_repository.GetInventoryLocationsByCategory(category.CategoryId));
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
            var whenStarted = ParseDate(Request["WhenStarted"]);
            var whenEnded = ParseDate(Request["WhenEnded"]);

            return await JobView("PlantHireChargesInJob", id, whenStarted, whenEnded);
        }

        private async Task<ActionResult> JobView(string viewName, int? id, DateTime whenStarted, DateTime whenEnded)
        {
            ViewBag.WhenStarted = whenStarted;
            ViewBag.WhenEnded = whenEnded;

            return await JobView(viewName, id);
        }

        private async Task<ActionResult> JobView(string viewName, int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var job = await _repository.Jobs.SingleOrDefaultAsync(f => f.JobId == id);
            if (job == null)
            {
                return HttpNotFound();
            }
            return View(viewName, job);
        }

        private DateTime ParseDate(string value)
        {
            DateTime date;

            if (!DateTime.TryParse(value, culture, styles, out date))
            {
                date = DateTime.MinValue;
            }

            return date;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _repository.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}