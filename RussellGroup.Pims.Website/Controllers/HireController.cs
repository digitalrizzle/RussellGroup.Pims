using RussellGroup.Pims.DataAccess.Models;
using RussellGroup.Pims.DataAccess.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace RussellGroup.Pims.Website.Controllers
{
    [HandleError]
    [PimsAuthorize(Roles = RoleType.All)]
    public class HireController : Controller
    {
        private PimsContext db = new PimsContext();

        #region Checkout

        public async Task<ActionResult> Checkout(int? id)
        {
            var transaction = new CheckoutTransaction()
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
                .Where(f => !f.WhenDisused.HasValue && (f.XPlantId.StartsWith(hint) || f.Description.Contains(hint)))
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
                .Where(f => !f.WhenDisused.HasValue && (f.XInventoryId.StartsWith(hint) || f.Description.Contains(hint)))
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
            if (plantIds.Count() == 0 && inventoryIds.Count() == 0) ModelState.AddModelError(string.Empty, "There must be either one plant item or one inventory item to checkout.");

            foreach (var id in plantIds) plants.Add(db.Plants.Single(f => f.PlantId == id));
            foreach (var id in inventoryIds) inventories.Add(db.Inventories.Single(f => f.InventoryId == id));

            var transaction = new CheckoutTransaction()
            {
                Docket = docket,
                Job = await db.Jobs.FindAsync(jobId),
                Plants = plants,
                Inventories = inventories
            };

            if (ModelState.IsValid)
            {
                await Checkout(transaction);
                return RedirectToAction("Details", "Job", new { id = jobId });
            }

            return View(transaction);
        }

        private async Task Checkout(CheckoutTransaction transaction)
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

            // save inventory
            foreach (var inventory in transaction.Inventories)
            {
                var hire = new InventoryHire
                {
                    Inventory = inventory,
                    Job = transaction.Job,
                    Docket = transaction.Docket,
                    WhenStarted = DateTime.Now,
                    WhenEnded = null,
                    Rate = inventory.Rate,
                    Quantity = inventory.Quantity
                };

                db.InventoryHires.Add(hire);
            }

            await db.SaveChangesAsync();
        }

        #endregion

        #region Checkin

        public async Task<ActionResult> Checkin(int? id)
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

            var transaction = new CheckinTransaction
            {
                Job = job,
                Docket = string.Empty,
                PlantHires = job.PlantHires.Where(f => !f.WhenEnded.HasValue).ToArray(),
                InventoryHires = job.InventoryHires.Where(f => !f.WhenEnded.HasValue).ToArray()
            };

            return View(transaction);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Checkin(FormCollection collection)
        {
            var returnDocket = collection["Docket"];
            var jobId = Convert.ToInt32(collection["JobId"]);
            var plantHireIds = GetIds("plant-hire-id-field", collection);
            var inventoryHireIds = GetIds("inventory-hire-id-field", collection);

            if (string.IsNullOrWhiteSpace(returnDocket)) ModelState.AddModelError("Docket", "A docket number is required.");
            if (plantHireIds.Count() == 0 && inventoryHireIds.Count() == 0) ModelState.AddModelError(string.Empty, "There must be either one plant item or one inventory item to checkin.");

            if (ModelState.IsValid)
            {
                await Checkin(returnDocket, plantHireIds, inventoryHireIds);
                return RedirectToAction("Details", "Job", new { id = jobId });
            }

            var job = db.Jobs.Single(f => f.JobId == jobId);
            var plantHires = job.PlantHires.Where(f => !f.WhenEnded.HasValue).ToList();
            var inventoryHires = job.InventoryHires.Where(f => !f.WhenEnded.HasValue).ToList();

            foreach (var hire in plantHires) if (plantHireIds.Any(f => f == hire.PlantHireId)) hire.IsChecked = true;
            foreach (var hire in inventoryHires) if (inventoryHireIds.Any(f => f == hire.InventoryHireId)) hire.IsChecked = true;

            var transaction = new CheckinTransaction
            {
                Job = job,
                Docket = returnDocket,
                PlantHires = plantHires,
                InventoryHires = inventoryHires
            };

            return View(transaction);
        }

        private async Task Checkin(string returnDocket, IEnumerable<int> plantHireIds, IEnumerable<int> inventoryHireIds)
        {
            // save plant
            foreach (var id in plantHireIds)
            {
                var hire = db.PlantHires.SingleOrDefault(f => f.PlantHireId == id && !f.WhenEnded.HasValue);

                if (hire != null)
                {
                    hire.ReturnDocket = returnDocket;
                    hire.WhenEnded = DateTime.Now;
                    db.Entry(hire).State = EntityState.Modified;
                }
            }

            // save inventory
            foreach (var id in inventoryHireIds)
            {
                var hire = db.InventoryHires.SingleOrDefault(f => f.InventoryHireId == id && !f.WhenEnded.HasValue);

                if (hire != null)
                {
                    hire.ReturnDocket = returnDocket;
                    hire.WhenEnded = DateTime.Now;
                    db.Entry(hire).State = EntityState.Modified;
                }
            }

            await db.SaveChangesAsync();
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private IEnumerable<int> GetIds(string prefix, FormCollection collection)
        {
            var ids = new List<int>();

            foreach (var key in collection.AllKeys)
            {
                if (key.StartsWith(prefix) && !string.IsNullOrWhiteSpace(collection[key]))
                {
                    var value = collection[key].Split(',')[0];
                    ids.Add(Convert.ToInt32(value));
                }
            }

            return ids.Distinct();
        }

        private new ActionResult View()
        {
            return View((CheckoutTransaction)null);
        }

        private ActionResult View(CheckoutTransaction transaction)
        {
            var jobs = db.Jobs.OrderByDescending(f => f.WhenStarted);

            ViewBag.Jobs = new SelectList(jobs, "JobId", "Description", transaction.JobId);

            return base.View(transaction);
        }

        private ActionResult View(CheckinTransaction transaction)
        {
            return base.View(transaction);
        }
    }
}