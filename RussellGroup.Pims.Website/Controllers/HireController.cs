using RussellGroup.Pims.DataAccess.Models;
using RussellGroup.Pims.DataAccess.Repositories;
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
    [PimsAuthorize(Role.CanEdit)]
    public class HireController : Controller
    {
        private readonly ITransactionRepository _repository;

        public HireController(ITransactionRepository repository)
        {
            _repository = repository;
        }

        #region Checkout

        public async Task<ActionResult> Checkout(int? id)
        {
            var transaction = new CheckoutTransaction()
            {
                Docket = string.Empty,
                Job = await _repository.GetJob(id),
                Plants = new List<Plant>(),
                Inventories = new List<KeyValuePair<Inventory, int?>>()
            };

            return View(transaction);
        }

        public JsonResult GetPlantSuggestions()
        {
            string hint = Request["q"];

            var result = _repository
                .Plants
                // f.IsCheckedIn can't be used because it isn't queryable
                .Where(f =>
                    !f.WhenDisused.HasValue &&
                    (f.Description.Contains(hint) || f.XPlantId.StartsWith(hint)) &&
                    f.PlantHires.All(h => h.WhenEnded.HasValue) &&
                    (f.StatusId == Status.Unknown || f.StatusId == Status.Available))
                .Take(5)
                .OrderBy(f => f.XPlantId)
                .Select(f => new { id = f.Id, description = f.Description, xid = f.XPlantId })
                .ToArray();

            var json = Json(result, JsonRequestBehavior.AllowGet);

            return json;
        }

        public JsonResult GetInventorySuggestions()
        {
            string hint = Request["q"];

            var result = _repository
                .Inventories
                .Where(f =>
                    !f.WhenDisused.HasValue &&
                    (f.XInventoryId.StartsWith(hint) | f.Description.Contains(hint)))
                .Take(5)
                .OrderBy(f => f.XInventoryId)
                .Select(f => new { id = f.Id, description = f.Description, xid = f.XInventoryId })
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
            var plantIds = collection.GetIds("plant-id-field");
            var inventoryIdsAndQuantities = collection.GetIdsAndQuantities("inventory-id-field", "inventory-quantity-field");
            var job = await _repository.GetJob(jobId);
            var plants = new List<Plant>();
            var inventoriesAndQuantities = new List<KeyValuePair<Inventory, int?>>();

            if (string.IsNullOrWhiteSpace(docket)) ModelState.AddModelError("Docket", "A docket number is required.");
            if (plantIds.Count() == 0 && inventoryIdsAndQuantities.Count() == 0) ModelState.AddModelError(string.Empty, "There must be either one plant item or one inventory item to checkout.");

            foreach (var id in plantIds) plants.Add(_repository.Plants.Single(f => f.Id == id));
            foreach (var pair in inventoryIdsAndQuantities) inventoriesAndQuantities.Add(new KeyValuePair<Inventory, int?>(_repository.Inventories.Single(f => f.Id == pair.Key), pair.Value));

            var transaction = new CheckoutTransaction()
            {
                Docket = docket,
                Job = job,
                Plants = plants,
                Inventories = inventoriesAndQuantities
            };

            if (ModelState.IsValid)
            {
                await _repository.Checkout(job, docket, plantIds, inventoryIdsAndQuantities);
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
            Job job = await _repository.GetJob(id);
            if (job == null)
            {
                return HttpNotFound();
            }

            // set the return quantity to the original quantity
            foreach (var hire in job.InventoryHires)
            {
                hire.ReturnQuantity = hire.Quantity;
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
            var plantHireIds = collection.GetIds("plant-hire-id-field");
            var inventoryHireIdsAndQuantities = collection.GetIdsAndQuantities("inventory-hire-id-field", "inventory-hire-quantity-field");

            if (string.IsNullOrWhiteSpace(returnDocket)) ModelState.AddModelError("Docket", "A docket number is required.");
            if (plantHireIds.Count() == 0 && inventoryHireIdsAndQuantities.Count() == 0) ModelState.AddModelError(string.Empty, "There must be either one plant item or one inventory item to checkin.");

            if (ModelState.IsValid)
            {
                await _repository.Checkin(returnDocket, plantHireIds, inventoryHireIdsAndQuantities);
                return RedirectToAction("Details", "Job", new { id = jobId });
            }

            // ModelState is invalid, so repopulate
            var job = await _repository.GetJob(jobId);
            var plantHires = _repository.GetCheckedOutPlantHiresInJob(jobId).ToList();
            var inventoryHires = _repository.GetCheckedOutInventoryHiresInJob(jobId).ToList();

            foreach (var hire in plantHires) if (plantHireIds.Any(f => f == hire.Id)) hire.IsSelected = true;

            foreach (var hire in inventoryHires)
            {
                if (inventoryHireIdsAndQuantities.Any(f => f.Key == hire.Id))
                {
                    hire.IsSelected = true;
                    hire.ReturnQuantity = inventoryHireIdsAndQuantities.Single(f => f.Key == hire.Id).Value;
                }
                else
                {
                    hire.ReturnQuantity = hire.Quantity;
                }
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

        private new ActionResult View()
        {
            return View((CheckoutTransaction)null);
        }

        private ActionResult View(CheckoutTransaction transaction)
        {
            var jobs = _repository.Jobs.Where(f => !f.WhenEnded.HasValue).OrderBy(f => f.Description);

            ViewBag.Jobs = new SelectList(jobs, "Id", "Description", transaction.JobId);

            return base.View(transaction);
        }

        private ActionResult View(CheckinTransaction transaction)
        {
            return base.View(transaction);
        }
    }
}