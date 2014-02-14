using RussellGroup.Pims.DataAccess.Models;
using RussellGroup.Pims.DataAccess.Respositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace RussellGroup.Pims.Website.Controllers
{
    [PimsAuthorize(Roles = RoleType.All)]
    public class ReportController : Controller
    {
        private readonly IReportRepository repository;

        public ReportController(IReportRepository repository)
        {
            this.repository = repository;
        }

        // GET: /Report/
        public ActionResult Index()
        {
            return View();
        }

        // this method has been adapted from the code described here:
        // http://www.codeproject.com/KB/aspnet/JQuery-DataTables-MVC.aspx
        public JsonResult GetDataTableResult(JqueryDataTableParameterModel model)
        {
            IEnumerable<Job> entries = repository.Jobs;
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
                    string.Format("<a href=\"{0}\" target=\"_blank\">{1}</a>", Url.Action("Job", new { id = c.JobId }), "Hire")
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

                row[6] = repository.GetActivePlantHiresInJob(id).Count().ToString();
                row[7] = repository.GetActiveInventoryHiresInJob(id).Count().ToString();
            }

            var result = new
            {
                sEcho = model.sEcho,
                iTotalRecords = repository.Jobs.Count(),
                iTotalDisplayRecords = searched.Count(),
                aaData = filtered
            };

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> Job(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Job job = await repository.GetJob(id);
            if (job == null)
            {
                return HttpNotFound();
            }
            return View(job);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                repository.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}