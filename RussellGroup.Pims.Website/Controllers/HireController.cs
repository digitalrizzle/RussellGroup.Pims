using RussellGroup.Pims.DataAccess.Models;
using RussellGroup.Pims.DataAccess.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace RussellGroup.Pims.Website.Controllers
{
    public class HireController : Controller
    {
        private PimsContext db = new PimsContext();

        public async Task<ActionResult> Checkout(int? id)
        {
            var transaction = new HireTransaction()
            {
                Docket = string.Empty,
                Job = await db.Jobs.FindAsync(id),
                Plants = new List<Plant>(),
                Inventories = new List<Inventory>()
            };

            return View(transaction);
        }

        public JsonResult GetPlantSuggestions()
        {
            string hint = Request["q"];

            var result = db
                .Plants
                .Where(f => !f.WhenDisused.HasValue && (f.XPlantId.Contains(hint) || f.Description.Contains(hint)))
                .Take(5)
                .OrderBy(f => f.Description)
                .Select(f => new { id = f.PlantId, description = f.Description, xid = f.XPlantId })
                .ToArray();

            var json = Json(result, JsonRequestBehavior.AllowGet);

            return json;
        }

        public JsonResult GetInventorySuggestions()
        {
            string hint = Request["q"];

            var result = db
                .Inventories
                .Where(f => !f.WhenDisused.HasValue && (f.XInventoryId.Contains(hint) || f.Description.Contains(hint)))
                .Take(5)
                .OrderBy(f => f.Description)
                .Select(f => new { id = f.InventoryId, description = f.Description, xid = f.XInventoryId })
                .ToArray();

            var json = Json(result, JsonRequestBehavior.AllowGet);

            return json;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Checkout(FormCollection collection)
        {
            var docket = collection["Docket"];
            var jobId = Convert.ToInt32(collection["JobId"]);
            var plantIds = GetIds("plant-id-field", collection);
            var inventoryIds = GetIds("inventory-id-field", collection);
            var plants = new List<Plant>();
            var inventories = new List<Inventory>();

            if (string.IsNullOrWhiteSpace(docket)) ModelState.AddModelError("Docket", "A docket number is required.");
            if (plantIds.Count == 0 && inventoryIds.Count == 0) ModelState.AddModelError(string.Empty, "There must be either one plant item or one inventory item to checkout.");

            foreach (var id in plantIds) plants.Add(db.Plants.Single(f => f.PlantId == id));
            foreach (var id in inventoryIds) inventories.Add(db.Inventories.Single(f => f.InventoryId == id));

            var transaction = new HireTransaction()
            {
                Docket = docket,
                Job = await db.Jobs.FindAsync(jobId),
                Plants = plants,
                Inventories = inventories
            };

            if (ModelState.IsValid)
            {
                await Save(transaction);
                return RedirectToAction("Index", "PlantHire", new { id = jobId });
            }

            return View(transaction);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private List<int> GetIds(string prefix, FormCollection collection)
        {
            var ids = new List<int>();

            foreach (var key in collection.AllKeys)
            {
                if (key.StartsWith(prefix) && !string.IsNullOrWhiteSpace(collection[key]))
                {
                    ids.Add(Convert.ToInt32(collection[key]));
                }
            }

            return ids;
        }

        private async Task Save(HireTransaction transaction)
        {
            // save plant
            foreach (var plant in transaction.Plants)
            {
                var hire = new PlantHire
                {
                    Plant = plant,
                    Job = transaction.Job,
                    Docket = transaction.Docket,
                    WhenStarted = DateTime.Now,
                    WhenEnded = null,
                    Rate = plant.Rate
                };

                db.PlantHires.Add(hire);
            }

            await db.SaveChangesAsync();
        }

        private new ActionResult View()
        {
            return View(null);
        }

        private ActionResult View(HireTransaction transaction)
        {
            var jobs = db.Jobs.OrderByDescending(f => f.WhenStarted);

            ViewBag.Jobs = new SelectList(jobs, "JobId", "Description", transaction.JobId);

            return base.View(transaction);
        }
    }
}