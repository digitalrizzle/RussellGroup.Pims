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
using RussellGroup.Pims.DataAccess.Respositories;

namespace RussellGroup.Pims.Website.Controllers
{
    [PimsAuthorize(Roles = new string[] { ApplicationRole.CanView })]
    public class CategoryController : Controller
    {
        private readonly IRepository<Category> repository;

        public CategoryController(IRepository<Category> repository)
        {
            this.repository = repository;
        }

        // GET: /Category/
        public async Task<ActionResult> Index()
        {
            return View(await repository.GetAll().ToListAsync());
        }

        // GET: /Category/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category category = await repository.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            return View(category);
        }

        // GET: /Category/Create
        [PimsAuthorize(Roles = new string[] { ApplicationRole.CanEditCategories })]
        public ActionResult Create()
        {
            return View();
        }

        // POST: /Category/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PimsAuthorize(Roles = new string[] { ApplicationRole.CanEditCategories })]
        public async Task<ActionResult> Create([Bind(Include = "CategoryId,Name,Type")] Category category)
        {
            if (ModelState.IsValid)
            {
                await repository.Add(category);
                return RedirectToAction("Index");
            }

            return View(category);
        }

        // GET: /Category/Edit/5
        [PimsAuthorize(Roles = new string[] { ApplicationRole.CanEditCategories })]
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category category = await repository.Find(id);
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
        [PimsAuthorize(Roles = new string[] { ApplicationRole.CanEditCategories })]
        public async Task<ActionResult> Edit([Bind(Include = "CategoryId,Name,Type")] Category category)
        {
            if (ModelState.IsValid)
            {
                await repository.Update(category);
                return RedirectToAction("Index");
            }
            return View(category);
        }

        public JsonResult GetTypeSuggestions()
        {
            string hint = Request["q"];

            var result = repository
                .GetAll()
                .Where(f => f.Type.Contains(hint))
                .OrderBy(f => f.Type)
                .Select(f => new { value = f.Type })
                .Distinct()
                .Take(5)
                .ToArray();

            var json = Json(result, JsonRequestBehavior.AllowGet);

            return json;
        }

        // GET: /Category/Delete/5
        [PimsAuthorize(Roles = new string[] { ApplicationRole.CanEditCategories })]
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category category = await repository.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            return View(category);
        }

        // POST: /Category/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [PimsAuthorize(Roles = new string[] { ApplicationRole.CanEditCategories })]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            await repository.Remove(id);
            return RedirectToAction("Index");
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
