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

namespace RussellGroup.Pims.Website.Controllers
{
    [HandleError]
    [PimsAuthorize(Roles = RoleType.All)]
    public class JobController : Controller
    {
        private PimsContext db = new PimsContext();

        // GET: /Job/
        public ActionResult Index()
        {
            return View();
        }

        // this method has been adapted from the code described here:
        // http://www.codeproject.com/KB/aspnet/JQuery-DataTables-MVC.aspx
        public JsonResult GetDataTableResult(JqueryDataTableParameterModel model)
        {
            IEnumerable<Job> entries = db.Jobs;
            var sortColumnIndex = int.Parse(Request["iSortCol_0"]);

            // ordering
            Func<Job, string> ordering = (c =>
                sortColumnIndex == 1 ? c.Description :
                    sortColumnIndex == 2 ? (c.WhenStarted.HasValue ? c.WhenStarted.Value.ToString("yyyyMMddhhmmss") : string.Empty) :
                        sortColumnIndex == 3 ? (c.WhenEnded.HasValue ? c.WhenEnded.Value.ToString("yyyyMMddhhmmss") : string.Empty) :
                            sortColumnIndex == 4 ? (c.ProjectManager != null ? c.ProjectManager.Name : string.Empty) : c.Status.ToString());

            // sorting
            IEnumerable<Job> ordered = Request["sSortDir_0"] == "asc" ?
                entries.OrderBy(ordering) :
                entries.OrderByDescending(ordering);

            // get the display values
            var displayData = ordered
                .Select(c => new string[]
                {
                    c.JobId.ToString(),
                    c.Description,
                    c.WhenStarted.HasValue ? c.WhenStarted.Value.ToShortDateString() : string.Empty,
                    c.WhenEnded.HasValue ? c.WhenEnded.Value.ToShortDateString() : string.Empty,
                    c.ProjectManager != null ? c.ProjectManager.Name : string.Empty,
                    c.Status.ToString(),
                    this.CrudAndCheckLinks(c.Status, new { id = c.JobId })
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

            var result = new
            {
                sEcho = model.sEcho,
                iTotalRecords = db.Jobs.Count(),
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
            Job job = await db.Jobs.FindAsync(id);
            if (job == null)
            {
                return HttpNotFound();
            }
            return View(job);
        }

        // GET: /Job/Create
        [PimsAuthorize(Roles = RoleType.Administrator)]
        public ActionResult Create()
        {
            return View();
        }

        // POST: /Job/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PimsAuthorize(Roles = RoleType.Administrator)]
        public async Task<ActionResult> Create([Bind(Include = "JobId,XJobId,Description,WhenStarted,WhenEnded,ProjectManagerContactId,QuantitySurveyorContactId,Comment")] Job job)
        {
            if (ModelState.IsValid)
            {
                db.Jobs.Add(job);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(job);
        }

        // GET: /Job/Edit/5
        [PimsAuthorize(Roles = RoleType.Administrator)]
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Job job = await db.Jobs.FindAsync(id);
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
        [PimsAuthorize(Roles = RoleType.Administrator)]
        public async Task<ActionResult> Edit([Bind(Include = "JobId,XJobId,Description,WhenStarted,WhenEnded,ProjectManagerContactId,QuantitySurveyorContactId,Comment")] Job job)
        {
            if (ModelState.IsValid)
            {
                db.Entry(job).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(job);
        }

        // GET: /Job/Delete/5
        [PimsAuthorize(Roles = RoleType.Administrator)]
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Job job = await db.Jobs.FindAsync(id);
            if (job == null)
            {
                return HttpNotFound();
            }
            return View(job);
        }

        // POST: /Job/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [PimsAuthorize(Roles = RoleType.Administrator)]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Job job = await db.Jobs.FindAsync(id);
            db.Jobs.Remove(job);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private string CrudAndCheckLinks(Status status, object routeValues)
        {
            var links = this.CrudLinks(routeValues);

            if (status == Status.Incomplete)
            {
                var checkin = string.Format("<a href=\"{0}\">{1}</a>", Url.Action("Checkin", "Hire", routeValues), "Checkin");
                var checkout = string.Format("<a href=\"{0}\">{1}</a>", Url.Action("Checkout", "Hire", routeValues), "Checkout");

                links = string.Format("{0} | {1} | {2}", checkout, checkin, links);
            }

            return links;
        }

        private new ActionResult View()
        {
            return View(null);
        }

        private ActionResult View(Job job)
        {
            var contacts = db.Contacts.OrderBy(f => f.Name);
            var projectManager = job != null ? job.ProjectManagerContactId : null;
            var quantitySurveyor = job != null ? job.QuantitySurveyorContactId : null;

            ViewBag.ProjectManagers = new SelectList(contacts, "ContactId", "Name", projectManager);
            ViewBag.QuantitySurveyors = new SelectList(contacts, "ContactId", "Name", quantitySurveyor);

            return base.View(job);
        }
    }
}
