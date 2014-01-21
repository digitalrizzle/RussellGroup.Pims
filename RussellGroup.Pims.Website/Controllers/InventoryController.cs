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
    public class InventoryController : Controller
    {
        private PimsContext db = new PimsContext();

        // GET: /Inventory/
        public ActionResult Index()
        {
            return View();
        }

        // this method has been adapted from the code described here:
        // http://www.codeproject.com/KB/aspnet/JQuery-DataTables-MVC.aspx
        public JsonResult GetDataTableResult(JqueryDataTableParameterModel model)
        {
            IEnumerable<Inventory> entries = db.Inventories;
            var sortColumnIndex = int.Parse(Request["iSortCol_0"]);

            // ordering
            Func<Inventory, string> ordering = (c =>
                sortColumnIndex == 1 ? c.XInventoryId :
                    sortColumnIndex == 2 ? c.Description :
                        sortColumnIndex == 3 ? (c.WhenPurchased.HasValue ? c.WhenPurchased.Value.ToString(MvcApplication.DATE_TIME_FORMAT) : string.Empty) :
                            sortColumnIndex == 4 ? (c.WhenDisused.HasValue ? c.WhenDisused.Value.ToString(MvcApplication.DATE_TIME_FORMAT) : string.Empty) :
                                sortColumnIndex == 5 ? c.Quantity.ToString() : c.Category.Name);

            // sorting
            IEnumerable<Inventory> ordered = Request["sSortDir_0"] == "asc" ?
                entries.OrderBy(ordering) :
                entries.OrderByDescending(ordering);

            // get the display values
            var displayData = ordered
                .Select(c => new string[]
                {
                    c.InventoryId.ToString(),
                    c.XInventoryId,
                    c.Description,
                    c.WhenPurchased.HasValue ? c.WhenPurchased.Value.ToShortDateString() : string.Empty,
                    c.WhenDisused.HasValue ? c.WhenDisused.Value.ToShortDateString() : string.Empty,
                    c.Quantity.ToString(),
                    c.Category != null ? c.Category.Name : string.Empty,
                    this.CrudLinks(new { id = c.InventoryId })
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
                iTotalRecords = db.Inventories.Count(),
                iTotalDisplayRecords = searched.Count(),
                aaData = filtered
            };

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        // GET: /Inventory/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Inventory inventory = await db.Inventories.FindAsync(id);
            if (inventory == null)
            {
                return HttpNotFound();
            }
            return View(inventory);
        }

        // GET: /Inventory/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: /Inventory/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include="InventoryId,CategoryId,XInventoryId,Description,WhenPurchased,WhenDisused,Rate,Cost,Quantity")] Inventory inventory)
        {
            if (ModelState.IsValid)
            {
                db.Inventories.Add(inventory);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(inventory);
        }

        // GET: /Inventory/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Inventory inventory = await db.Inventories.FindAsync(id);
            if (inventory == null)
            {
                return HttpNotFound();
            }
            return View(inventory);
        }

        // POST: /Inventory/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include="InventoryId,CategoryId,XInventoryId,Description,WhenPurchased,WhenDisused,Rate,Cost,Quantity")] Inventory inventory)
        {
            if (ModelState.IsValid)
            {
                db.Entry(inventory).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(inventory);
        }

        // GET: /Inventory/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Inventory inventory = await db.Inventories.FindAsync(id);
            if (inventory == null)
            {
                return HttpNotFound();
            }
            return View(inventory);
        }

        // POST: /Inventory/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Inventory inventory = await db.Inventories.FindAsync(id);
            db.Inventories.Remove(inventory);
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

        private new ActionResult View()
        {
            return View(null);
        }

        private ActionResult View(Inventory inventory)
        {
            var categories = db.Categories.OrderBy(f => f.Name);
            var category = inventory != null ? inventory.CategoryId : 0;

            ViewBag.Categories = new SelectList(categories, "CategoryId", "Name", category);

            return base.View(inventory);
        }
    }
}
