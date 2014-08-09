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
        public async Task<ActionResult> Index()
        {
            return View(await _repository.GetAll().ToListAsync());
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
        public async Task<ActionResult> Edit([Bind(Include = "Id,Name,Type")] Category category)
        {
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
            await _repository.RemoveAsync(id);
            return RedirectToAction("Index");
        }
    }
}
