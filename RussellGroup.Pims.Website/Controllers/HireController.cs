using RussellGroup.Pims.DataAccess.Models;
using RussellGroup.Pims.DataAccess.Respositories;
using RussellGroup.Pims.DataAccess.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace RussellGroup.Pims.Website.Controllers
{
    [PimsAuthorize(Roles = RoleType.All)]
    public class HireController : Controller
    {
        private readonly ITransactionRepository repository;

        public HireController(ITransactionRepository repository)
        {
            this.repository = repository;
        }

        #region Checkout

        public async Task<ActionResult> Checkout(int? id)
        {
            var transaction = new CheckoutTransaction()
            {
                Docket = string.Empty,
                Job = await repository.GetJob(id),
                Plants = new List<Plant>(),
                Inventories = new List<KeyValuePair<Inventory, int?>>()
            };

            return View(transaction);
        }

        public JsonResult GetPlantSuggestions()
        {
            string hint = Request["q"];

            var result = repository
                .Plants
                .Where(f => f.StatusId == 2 && (f.XPlantId.StartsWith(hint) || f.Description.Contains(hint)))
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

            var result = repository
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
            var inventoryIdsAndQuantities = GetIdsAndQuantities("inventory-id-field", "inventory-quantity-field", collection);
            var job = await repository.GetJob(jobId);
            var plants = new List<Plant>();
            var inventoriesAndQuantities = new List<KeyValuePair<Inventory, int?>>();

            if (string.IsNullOrWhiteSpace(docket)) ModelState.AddModelError("Docket", "A docket number is required.");
            if (plantIds.Count() == 0 && inventoryIdsAndQuantities.Count() == 0) ModelState.AddModelError(string.Empty, "There must be either one plant item or one inventory item to checkout.");

            foreach (var id in plantIds) plants.Add(repository.Plants.Single(f => f.PlantId == id));
            foreach (var pair in inventoryIdsAndQuantities) inventoriesAndQuantities.Add(new KeyValuePair<Inventory, int?>(repository.Inventories.Single(f => f.InventoryId == pair.Key), pair.Value));

            var transaction = new CheckoutTransaction()
            {
                Docket = docket,
                Job = job,
                Plants = plants,
                Inventories = inventoriesAndQuantities
            };

            if (ModelState.IsValid)
            {
                await repository.Checkout(job, docket, plantIds, inventoryIdsAndQuantities);
                return RedirectToAction("Details", "Job", new { id = jobId });
            }

            return View(transaction);
        }

        #endregion

        #region Checkin

        public async Task<ActionResult> Checkin(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Job job = await repository.GetJob(id);
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
            var inventoryHireIdsAndQuantities = GetQuantities("inventory-hire-quantity-field", collection, inventoryHireIds);

            if (string.IsNullOrWhiteSpace(returnDocket)) ModelState.AddModelError("Docket", "A docket number is required.");
            if (plantHireIds.Count() == 0 && inventoryHireIds.Count() == 0) ModelState.AddModelError(string.Empty, "There must be either one plant item or one inventory item to checkin.");

            if (ModelState.IsValid)
            {
                await repository.Checkin(returnDocket, plantHireIds, inventoryHireIdsAndQuantities);
                return RedirectToAction("Details", "Job", new { id = jobId });
            }

            // ModelState is invalid, so repopulate
            var job = await repository.GetJob(jobId);
            var plantHires = repository.GetActivePlantHiresInJob(jobId).ToList();
            var inventoryHires = repository.GetActiveInventoryHiresInJob(jobId).ToList();

            foreach (var hire in plantHires) if (plantHireIds.Any(f => f == hire.PlantHireId)) hire.IsChecked = true;

            foreach (var hire in inventoryHires) if (inventoryHireIds.Any(f => f == hire.InventoryHireId))
            {
                hire.IsChecked = true;
                hire.Quantity = inventoryHireIdsAndQuantities.Single(f => f.Key == hire.InventoryHireId).Value;
            }

            var transaction = new CheckinTransaction
            {
                Job = job,
                Docket = returnDocket,
                PlantHires = plantHires,
                InventoryHires = inventoryHires
            };

            return View(transaction);
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                repository.Dispose();
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

        private IDictionary<int, int> GetQuantities(string prefix, FormCollection collection, IEnumerable<int> ids)
        {
            var quantities = new Dictionary<int, int>();

            foreach (var key in collection.AllKeys)
            {
                foreach (var id in ids)
                {
                    if (key.Equals(prefix + id.ToString()) && !string.IsNullOrWhiteSpace(collection[key]))
                    {
                        var value = collection[key].Split(',')[0];
                        quantities.Add(id, Convert.ToInt32(value));
                    }
                }
            }

            return quantities;
        }

        private IEnumerable<KeyValuePair<int, int?>> GetIdsAndQuantities(string idPrefix, string quantityPrefix, FormCollection collection)
        {
            var pairs = new List<KeyValuePair<int, int?>>();

            foreach (var idKey in collection.AllKeys)
            {
                if (idKey.StartsWith(idPrefix) && !string.IsNullOrWhiteSpace(collection[idKey]))
                {
                    var idFieldValue = Regex.Replace(idKey, @"[^\d]", "");
                    var idValue = collection[idKey].Split(',')[0];
                    var id = Convert.ToInt32(idValue);

                    int? quantity = null;
                    var quantityKey = collection.AllKeys.SingleOrDefault(f => f == quantityPrefix + idFieldValue);

                    if (quantityKey != null)
                    {
                        int result;
                        var quantityValue = collection[quantityKey].Split(',')[0];

                        if (int.TryParse(quantityValue, out result))
                        {
                            quantity = result;
                        }
                    }

                    pairs.Add(new KeyValuePair<int, int?>(id, quantity));
                }
            }

            return pairs.Distinct();
        }

        private new ActionResult View()
        {
            return View((CheckoutTransaction)null);
        }

        private ActionResult View(CheckoutTransaction transaction)
        {
            var jobs = repository.Jobs.OrderByDescending(f => f.WhenStarted);

            ViewBag.Jobs = new SelectList(jobs, "JobId", "Description", transaction.JobId);

            return base.View(transaction);
        }

        private ActionResult View(CheckinTransaction transaction)
        {
            return base.View(transaction);
        }
    }
}