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
using RussellGroup.Pims.Website.Models;
using RussellGroup.Pims.DataAccess;

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
                    f.XPlantId.Contains(hint) ||
                    f.XPlantNewId.Contains(hint) ||
                    f.Description.Contains(hint) ||
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
                    c.XPlantId,
                    c.XPlantNewId,
                    c.Description,
                    Category = c.Category != null ? c.Category.Name : string.Empty,
                    Hire = _repository.GetPlantHire(c.Id).Count() == 0 ? string.Empty : this.ActionLink(_repository.GetPlantHire(c.Id).Count().ToString(), "PlantHire", new { id = c.Id }),
                    IsDisused = c.WhenDisused.HasValue ? "Yes" : "No",
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
            var plant = await _repository.FindAsync(id);
            if (plant == null)
            {
                return HttpNotFound();
            }
            return View("Details", plant);
        }

        // GET: /Plant/Create
        [PimsAuthorize(Role.CanEdit)]
        public ActionResult Create()
        {
            var plant = new Plant { WhenPurchased = DateTime.Now };

            return View("Create", plant);
        }

        // POST: /Plant/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PimsAuthorize(Role.CanEdit)]
        public async Task<ActionResult> Create(FormCollection collection)
        {
            var plant = new Plant();

            if (TryUpdateModel(plant, "Plant", (Plant.IncludeProperties + ",StatusId").Split(',')))
            {
                var model = GetPlantViewModel(plant);
                TryUpdateModel(model.FileViewModel, "FileViewModel", collection);
                model.UpdatePlantPhotograph();
                Validate(model.Plant.Photograph.Content.Data);

                if (ModelState.IsValid)
                {
                    await _repository.AddAsync(plant);
                    return RedirectToAction("Index");
                }
            }

            return View("Create", plant);
        }

        // GET: /Plant/Edit/5
        [PimsAuthorize(Role.CanEdit)]
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var plant = await _repository.FindAsync(id);
            if (plant == null)
            {
                return HttpNotFound();
            }
            return View("Edit", plant);
        }

        // POST: /Plant/Edit/5
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
            var plant = await _repository.FindAsync(id);
            if (plant == null)
            {
                return HttpNotFound();
            }

            // ensure the status isn't updated if it is checked out
            var includeProperties = Plant.IncludeProperties;
            if (!plant.IsCheckedOut)
            {
                includeProperties += ",StatusId";
            }

            if (TryUpdateModel(plant, "Plant", includeProperties.Split(',')))
            {
                var model = GetPlantViewModel(plant);
                TryUpdateModel(model.FileViewModel, "FileViewModel", collection);
                model.UpdatePlantPhotograph();
                Validate(model.Plant.Photograph.Content.Data);

                if (ModelState.IsValid)
                {
                    await _repository.UpdateAsync(model.Plant);
                    return RedirectToAction("Index");
                }
            }

            return View("Edit", plant);
        }

        // GET: /Plant/Delete/5
        [PimsAuthorize(Role.CanEdit)]
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var plant = await _repository.FindAsync(id);
            if (plant == null)
            {
                return HttpNotFound();
            }
            return View("Delete", plant);
        }

        // POST: /Plant/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [PimsAuthorize(Role.CanEdit)]
        public async Task<ActionResult> DeleteConfirmed(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var plant = await _repository.FindAsync(id);
            if (plant == null)
            {
                return HttpNotFound();
            }
            await _repository.RemoveAsync(plant);
            return RedirectToAction("Index");
        }

        private void Validate(byte[] buffer)
        {
            if (buffer == null) return;

            var mimeType = MimeHelper.GetMimeType(buffer);

            switch (mimeType)
            {
                case "image/gif":
                case "image/jpeg":
                case "image/png":
                case "image/svg+xml":
                case "image/tiff":
                    return;
                default:
                    ModelState.AddModelError("", "The photograph is not valid.");
                    break;
            }
        }

        private PlantViewModel GetPlantViewModel(Plant plant)
        {
            var model = new PlantViewModel
            {
                Plant = plant,
                FileViewModel = FileViewModel.Create(plant.Photograph)
            };

            return model;
        }

        private ActionResult View(string viewName, Plant plant)
        {
            var categories = _repository.Categories.OrderBy(f => f.Name);
            var category = plant != null ? plant.CategoryId : 0;
            ViewBag.Categories = new SelectList(categories, "Id", "Name", category);

            var statuses = _repository.Statuses.OrderBy(f => f.Id);
            var status = plant != null ? plant.StatusId : 0;
            ViewBag.Statuses = new SelectList(statuses, "Id", "Name", plant.IsCheckedOut ? Status.CheckedOut : status);

            var conditions = _repository.Conditions.OrderBy(f => f.Id);
            var condition = plant != null ? plant.ConditionId : 0;
            ViewBag.Conditions = new SelectList(conditions, "Id", "Name", condition);

            var model = GetPlantViewModel(plant);

            return base.View(viewName, model);
        }
    }
}
