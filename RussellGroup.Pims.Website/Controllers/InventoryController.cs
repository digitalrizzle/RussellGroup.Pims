﻿using System;
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
    public class InventoryController : Controller
    {
        private readonly IInventoryRepository _repository;

        public InventoryController(IInventoryRepository repository)
        {
            _repository = repository;
        }

        // GET: /Inventory/
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
                    f.XInventoryId.Contains(hint) ||
                    f.Description.Contains(hint) ||
                    SqlFunctions.StringConvert((double)f.Quantity).Contains(hint) ||
                    f.Category.Name.Contains(hint));

            // ordering
            var sortColumnName = string.IsNullOrEmpty(sortColumn.Name) ? sortColumn.Data : sortColumn.Name;
            Func<Inventory, IComparable> ordering = (c => c.GetValue(sortColumnName.Split('.')));

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
                    c.XInventoryId,
                    c.Description,
                    Category = c.Category.Name,
                    c.Quantity,
                    CollatedQuantity = c.CollatedInventoryHires.Sum(f => f.Quantity),
                    IsDisused = c.WhenDisused.HasValue ? "Yes" : "No",
                    CrudLinks = this.CrudLinks(new { id = c.Id }, User.IsAuthorized(Role.CanEdit))
                });

            return Json(new DataTablesResponse(model.Draw, paged, filtered.Count(), all.Count()), JsonRequestBehavior.AllowGet);
        }

        // GET: /Inventory/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var inventory = await _repository.FindAsync(id);
            if (inventory == null)
            {
                return HttpNotFound();
            }
            return View("Details", inventory);
        }

        // GET: /Inventory/Create
        [PimsAuthorize(Role.CanEdit)]
        public ActionResult Create()
        {
            var inventory = new Inventory { WhenPurchased = DateTime.Now };
            return View("Create", inventory);
        }

        // POST: /Inventory/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PimsAuthorize(Role.CanEdit)]
        public async Task<ActionResult> Create([Bind(Include = "CategoryId,XInventoryId,Description,WhenPurchased,WhenDisused,Rate,Cost,Quantity")] Inventory inventory)
        {
            if (ModelState.IsValid)
            {
                await _repository.AddAsync(inventory);
                return RedirectToAction("Index");
            }

            return View("Create", inventory);
        }

        // GET: /Inventory/Edit/5
        [PimsAuthorize(Role.CanEdit)]
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var inventory = await _repository.FindAsync(id);
            if (inventory == null)
            {
                return HttpNotFound();
            }
            return View("Edit", inventory);
        }

        // POST: /Inventory/Edit/5
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

            if (TryUpdateModel<Inventory>(plant, "CategoryId,XInventoryId,Description,WhenPurchased,WhenDisused,Rate,Cost,Quantity".Split(',')))
            {
                if (ModelState.IsValid)
                {
                    await _repository.UpdateAsync(plant);
                    return RedirectToAction("Index");
                }
            }

            return View("Edit", plant);
        }

        // GET: /Inventory/Delete/5
        [PimsAuthorize(Role.CanEdit)]
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var inventory = await _repository.FindAsync(id);
            if (inventory == null)
            {
                return HttpNotFound();
            }
            return View("Delete", inventory);
        }

        // POST: /Inventory/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [PimsAuthorize(Role.CanEdit)]
        public async Task<ActionResult> DeleteConfirmed(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var inventory = await _repository.FindAsync(id);
            if (inventory == null)
            {
                return HttpNotFound();
            }
            await _repository.RemoveAsync(inventory);
            return RedirectToAction("Index");
        }

        private ActionResult View(string viewName, Inventory inventory)
        {
            var categories = _repository.Categories.OrderBy(f => f.Name);
            var category = inventory != null ? inventory.CategoryId : 0;
            ViewBag.Categories = new SelectList(categories, "Id", "Name", category);

            return base.View(inventory);
        }
    }
}
