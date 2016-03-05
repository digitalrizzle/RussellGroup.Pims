using RussellGroup.Pims.DataAccess.Models;
using RussellGroup.Pims.DataAccess.Repositories;
using RussellGroup.Pims.Website.Models;
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
                WhenStarted = DateTime.Now.Date,
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
                .Include("PlantHires")
                // f.IsCheckedIn can't be used because it isn't queryable
                .Where(f =>
                    !f.WhenDisused.HasValue &&
                    (f.XPlantNewId.StartsWith(hint) | f.XPlantId.StartsWith(hint) | f.Description.Contains(hint))) //&&
                    //f.PlantHires.All(h => h.WhenEnded.HasValue) &&
                    //(f.StatusId == Status.Unknown || f.StatusId == Status.Available))
                .Take(5)
                .OrderBy(f => f.XPlantId)
                .ToArray()
                .Select(f => new { id = f.Id, description = f.Description, xid = f.XPlantIdAndXPlantNewId, rate = f.Rate.ToString("0.00"), status = f.Status.Name });

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
        public async Task<ActionResult> Checkout(CheckoutTransaction transaction)
        {
            var collection = Request.Form;
            var plantIds = collection.GetIds("plant-id-field");
            var inventoryIdsAndQuantities = collection.GetIdsAndQuantities("inventory-id-field", "inventory-quantity-field");

            int jobId;
            if (!int.TryParse(collection["JobId"], out jobId))
            {
                ModelState.AddModelError("JobId", "The job is invalid.");
                jobId = transaction.Job.Id;
            }

            var job = await _repository.GetJob(jobId);
            var plants = new List<Plant>();
            var inventoriesAndQuantities = new List<KeyValuePair<Inventory, int?>>();

            if (plantIds.Count() == 0 && inventoryIdsAndQuantities.Count() == 0) ModelState.AddModelError(string.Empty, "There must be, at least, either one plant item or one inventory item to checkout.");

            foreach (var id in plantIds) plants.Add(_repository.Plants.Single(f => f.Id == id));
            foreach (var pair in inventoryIdsAndQuantities) inventoriesAndQuantities.Add(new KeyValuePair<Inventory, int?>(_repository.Inventories.Single(f => f.Id == pair.Key), pair.Value));

            if (ModelState.IsValid)
            {
                await _repository.Checkout(job, transaction.Docket, transaction.WhenStarted, plantIds, inventoryIdsAndQuantities);
                return RedirectToAction("Details", "Job", new { id = job.Id });
            }

            // ModelState is invalid, so repopulate
            transaction.Job = job;
            transaction.Plants = plants;
            transaction.Inventories = inventoriesAndQuantities;

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

            var transaction = new CheckinTransaction
            {
                Job = job,
                ReturnDocket = string.Empty,
                WhenEnded = DateTime.Now.Date,
                PlantHires = job.PlantHires.Where(f => !f.WhenEnded.HasValue).ToArray(),
                InventoryHires = _repository.GetCheckinInventoryHires(job).ToArray()
            };

            return View(transaction);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Checkin(CheckinTransaction transaction)
        {
            Job job = await _repository.GetJob(transaction.JobId);
            if (job == null)
            {
                return HttpNotFound();
            }

            var plantHireIds = HttpUtility.ParseQueryString(Request.Form["selectedPlantHire"]).GetIds("plant-hire-id-field");
            var inventoryIdsAndQuantities = HttpUtility.ParseQueryString(Request.Form["selectedInventoryHire"]).GetIdsAndQuantities("inventory-id-field", "inventory-hire-quantity-field");

            if (string.IsNullOrWhiteSpace(transaction.ReturnDocket)) ModelState.AddModelError("ReturnDocket", "A docket number is required.");
            if (plantHireIds.Count() == 0 && inventoryIdsAndQuantities.Count() == 0) ModelState.AddModelError(string.Empty, "There must be, at least, either one plant item or one inventory item to checkin.");

            if (ModelState.IsValid)
            {
                await _repository.Checkin(job, transaction.ReturnDocket, transaction.WhenEnded, plantHireIds, inventoryIdsAndQuantities);
                return RedirectToAction("Details", "Job", new { id = transaction.JobId });
            }

            // ModelState is invalid, so repopulate
            var plantHires = job.PlantHires.Where(f => !f.WhenEnded.HasValue).ToArray();
            var inventoryHires = _repository.GetCheckinInventoryHires(job).ToArray();

            foreach (var hire in plantHires)
            {
                if (plantHireIds.Any(f => f == hire.Id))
                {
                    hire.IsSelected = true;
                }
            }

            foreach (var hire in inventoryHires)
            {
                if (inventoryIdsAndQuantities.Any(f => f.Key == hire.InventoryId))
                {
                    hire.IsSelected = true;
                    hire.Quantity = inventoryIdsAndQuantities.Single(f => f.Key == hire.InventoryId).Value;
                }
            }

            transaction.Job = job;
            transaction.PlantHires = plantHires;
            transaction.InventoryHires = inventoryHires;

            return View(transaction);
        }

        #endregion

        private new ActionResult View()
        {
            return View((CheckoutTransaction)null);
        }

        private ActionResult View(CheckoutTransaction transaction)
        {
            var jobs = _repository.Jobs.Where(f => !f.WhenEnded.HasValue).OrderBy(f => f.XJobId);

            ViewBag.Jobs = new SelectList(jobs, "Id", "JobAndDescription", transaction.JobId);

            return base.View(transaction);
        }

        private ActionResult View(CheckinTransaction transaction)
        {
            return base.View(transaction);
        }
    }
}