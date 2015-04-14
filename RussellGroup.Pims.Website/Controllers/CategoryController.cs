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
    [PimsAuthorize(Role.CanView, Role.CanEdit, Role.CanEditCategories)]
    public class CategoryController : Controller
    {
        private readonly IRepository<Category> _repository;

        public CategoryController(IRepository<Category> repository)
        {
            _repository = repository;
        }

        // GET: /Category/
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
                    f.Name.Contains(hint) ||
                    f.Type.Contains(hint) ||
                    SqlFunctions.StringConvert((double)f.Plants.Count).Contains(hint) ||
                    SqlFunctions.StringConvert((double)f.Inventories.Count).Contains(hint));

            // ordering
            var sortColumnName = string.IsNullOrEmpty(sortColumn.Name) ? sortColumn.Data : sortColumn.Name;
            Func<Category, IComparable> ordering = (c => c.GetValue(sortColumnName.Split('.')));

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
                    c.Name,
                    c.Type,
                    PlantQuantity = c.Plants.Count,
                    InventoryQuantity = c.Inventories.Count,
                    CrudLinks = this.CrudLinks(new { id = c.Id }, User.IsAuthorized(Role.CanEditCategories))
                });

            return Json(new DataTablesResponse(model.Draw, paged, filtered.Count(), all.Count()), JsonRequestBehavior.AllowGet);
        }

        // GET: /Category/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category category = await _repository.FindAsync(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            return View(category);
        }

        // GET: /Category/Create
        [PimsAuthorize(Role.CanEditCategories)]
        public ActionResult Create()
        {
            return View();
        }

        // POST: /Category/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PimsAuthorize(Role.CanEditCategories)]
        public async Task<ActionResult> Create([Bind(Include = "Name,Type")] Category category)
        {
            if (ModelState.IsValid)
            {
                await _repository.AddAsync(category);
                return RedirectToAction("Index");
            }

            return View(category);
        }

        // GET: /Category/Edit/5
        [PimsAuthorize(Role.CanEditCategories)]
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category category = await _repository.FindAsync(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            return View(category);
        }

        // POST: /Category/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PimsAuthorize(Role.CanEditCategories)]
        public async Task<ActionResult> Edit(int? id, FormCollection collection)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category category = await _repository.FindAsync(id);
            if (category == null)
            {
                return HttpNotFound();
            }

            if (!TryUpdateModel<Category>(category, "Name,Type".Split(',')))
            {
                ModelState.AddModelError(string.Empty, "The item could not be updated.");
            }

            if (ModelState.IsValid)
            {
                await _repository.UpdateAsync(category);
                return RedirectToAction("Index");
            }
            return View(category);
        }

        public JsonResult GetTypeSuggestions()
        {
            string hint = Request["q"];

            var result = _repository
                .GetAll()
                .Where(f => f.Type.Contains(hint))
                .Select(f => new { value = f.Type })
                .Distinct()
                .OrderBy(f => f)
                .Take(5)
                .ToArray();

            var json = Json(result, JsonRequestBehavior.AllowGet);

            return json;
        }

        // GET: /Category/Delete/5
        [PimsAuthorize(Role.CanEditCategories)]
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category category = await _repository.FindAsync(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            return View(category);
        }

        // POST: /Category/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [PimsAuthorize(Role.CanEditCategories)]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var category = await _repository.FindAsync(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            await _repository.RemoveAsync(category);
            return RedirectToAction("Index");
        }
    }
}
