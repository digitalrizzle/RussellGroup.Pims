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
    public class InventoryHireCheckinController : Controller
    {
        private readonly IInventoryHireCheckinRepository _repository;

        public InventoryHireCheckinController(IInventoryHireCheckinRepository repository)
        {
            _repository = repository;
        }

        // GET: /InventoryHireReturn/5
        public async Task<ActionResult> Index(int? id, int? hireId)
        {
            if (hireId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            InventoryHire hire = await _repository.GetInventoryHire(hireId);
            if (hire == null)
            {
                return HttpNotFound();
            }

            return View(hire);
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

            var all = _repository.GetAll().AsExpandable().Where(f => f.InventoryHireId == id);

            // filter
            var filtered = string.IsNullOrEmpty(hint)
                ? all
                : all.Where(f =>
                    f.Docket.Contains(hint) ||
                    Extensions.LittleEndianDateString.Invoke(f.WhenEnded).Contains(hint) ||
                    SqlFunctions.StringConvert((double)f.Quantity).Contains(hint));

            // ordering
            var sortColumnName = string.IsNullOrEmpty(sortColumn.Name) ? sortColumn.Data : sortColumn.Name;
            Func<InventoryHireCheckin, IComparable> ordering = (c => c.GetValue(sortColumnName.Split('.')));

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
                    c.Docket,
                    WhenEnded = c.WhenEnded.ToShortDateString(),
                    c.Quantity,
                    CrudLinks = this.CrudLinks(new { id = c.InventoryHire.JobId, hireId = c.InventoryHireId, checkinId = c.Id }, User.IsAuthorized(Role.CanEdit))
                });

            return Json(new DataTablesResponse(model.Draw, paged, filtered.Count(), all.Count()), JsonRequestBehavior.AllowGet);
        }

        // GET: /InventoryHireReturn/Details/5
        public async Task<ActionResult> Details(int? id, int? hireId, int? checkinId)
        {
            if (checkinId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            InventoryHireCheckin checkin = await _repository.FindAsync(checkinId);
            if (checkin == null)
            {
                return HttpNotFound();
            }
            return View(checkin);
        }

        // GET: /InventoryHireReturn/Create/5
        [PimsAuthorize(Role.CanEdit)]
        public async Task<ActionResult> Create(int? id, int? hireId)
        {
            if (hireId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var hire = await _repository.GetInventoryHire(hireId);
            if (hire == null)
            {
                return HttpNotFound();
            }

            var checkin = new InventoryHireCheckin()
            {
                InventoryHire = hire,
                InventoryHireId = hire.Id,
                WhenEnded = DateTime.Now
            };

            return View(checkin);
        }

        // POST: /InventoryHireReturn/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PimsAuthorize(Role.CanEdit)]
        public async Task<ActionResult> Create([Bind(Include = "InventoryHireId,Docket,WhenEnded,Quantity,Comment")] InventoryHireCheckin checkin)
        {
            if (ModelState.IsValid)
            {
                await _repository.AddAsync(checkin);

                var hire = await _repository.GetInventoryHire(checkin.InventoryHireId);

                if (hire == null)
                {
                    return HttpNotFound();
                }

                return RedirectToAction("Index", new { id = hire.JobId, hireId = hire.Id });
            }

            return View(checkin);
        }

        // GET: /InventoryHireReturn/Edit/5
        [PimsAuthorize(Role.CanEdit)]
        public async Task<ActionResult> Edit(int? id, int? hireId, int? checkinId)
        {
            if (checkinId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            InventoryHireCheckin checkin = await _repository.FindAsync(checkinId);
            if (checkin == null)
            {
                return HttpNotFound();
            }
            return View(checkin);
        }

        // POST: /InventoryHireReturn/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PimsAuthorize(Role.CanEdit)]
        public async Task<ActionResult> Edit(int? id, int? hireId, int? checkinId, FormCollection collection)
        {
            if (checkinId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            InventoryHireCheckin checkin = await _repository.FindAsync(checkinId);
            if (checkin == null)
            {
                return HttpNotFound();
            }

            // InventoryId isn't included as we do not want to update this
            if (TryUpdateModel<InventoryHireCheckin>(checkin, "Docket,WhenEnded,Quantity,Comment".Split(',')))
            {
                if (ModelState.IsValid)
                {
                    await _repository.UpdateAsync(checkin);

                    var hire = await _repository.GetInventoryHire(checkin.InventoryHireId);

                    if (hire == null)
                    {
                        return HttpNotFound();
                    }

                    return RedirectToAction("Index", new { id = hire.JobId, hireId = hire.Id });
                }
            }

            return View("Edit", checkin);
        }

        // GET: /InventoryHireReturn/Delete/5
        [PimsAuthorize(Role.CanEdit)]
        public async Task<ActionResult> Delete(int? id, int? hireId, int? checkinId)
        {
            if (checkinId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            InventoryHireCheckin checkin = await _repository.FindAsync(checkinId);
            if (checkin == null)
            {
                return HttpNotFound();
            }
            return View(checkin);
        }

        // POST: /InventoryHireReturn/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [PimsAuthorize(Role.CanEdit)]
        public async Task<ActionResult> DeleteConfirmed(int? checkinId)
        {
            if (checkinId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            InventoryHireCheckin checkin = await _repository.FindAsync(checkinId);
            if (checkin == null)
            {
                return HttpNotFound();
            }
            await _repository.RemoveAsync(checkin);

            var hire = await _repository.GetInventoryHire(checkin.InventoryHireId);

            if (hire == null)
            {
                return HttpNotFound();
            }

            return RedirectToAction("Index", new { id = hire.JobId, hireId = hire.Id });
        }

        private new ActionResult View()
        {
            throw new NotSupportedException();
        }
    }
}
