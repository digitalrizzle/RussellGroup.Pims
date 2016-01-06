using RussellGroup.Pims.DataAccess.Models;
using RussellGroup.Pims.DataAccess.Repositories;
using RussellGroup.Pims.Website.Models;
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
    [PimsAuthorize(Role.CanEdit)]
    public class BatchController : Controller
    {
        private readonly ITransactionRepository _repository;
        private static readonly Regex _jobRegex = new Regex(@"[a-z]{2}\d{4}", RegexOptions.IgnoreCase);
        private static readonly Regex _statusRegex = new Regex(@"WRITTEN OFF|UNDER REPAIR|STOLEN|AVAILABLE|UNKNOWN", RegexOptions.IgnoreCase);

        public BatchController(ITransactionRepository repository)
        {
            _repository = repository;
        }

        public async Task<ActionResult> JobBarcodes()
        {
            var jobs = await _repository.Jobs
                .Where(f => !f.WhenEnded.HasValue)
                .OrderByDescending(f => f.WhenStarted)
                .ToListAsync();

            return View(jobs);
        }

        public async Task<ActionResult> PlantBarcodes()
        {
            var plants = await _repository.Plants
                .Where(f => f.StatusId.Equals(Status.Available))
                .OrderByDescending(f => f.XPlantNewId)
                .ToListAsync();

            return View(plants);
        }

        public async Task<ActionResult> StatusBarcodes()
        {
            var statuses = await _repository.Statuses
                .Where(f => !f.Id.Equals(Status.CheckedOut))
                .OrderByDescending(f => f.Id)
                .ToListAsync();

            return View(statuses);
        }

        #region Checkout

        public ActionResult Checkout(string whenStarted, string scans)
        {
            DateTime parsedDate;

            if (!DateTime.TryParse(whenStarted, out parsedDate))
            {
                parsedDate = DateTime.Now;
            }

            var model = new BatchCheckout
            {
                WhenStarted = parsedDate.Date,
                Scans = scans
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Checkout(BatchCheckout model)
        {
            var transactions = await GetCheckoutTransactionsAsync(model);

            model.CheckoutTransactions = transactions;

            return View("ConfirmCheckout", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ConfirmCheckout(BatchCheckout model, string command)
        {
            //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            switch (command)
            {
                case "Checkout":
                    var transactions = await GetCheckoutTransactionsAsync(model);
                    model.CheckoutTransactions = transactions;

                    // one more check before the actual commit
                    if (!ModelState.IsValid)
                    {
                        return View("ConfirmCheckout", model);
                    }

                    // commit each transaction; the docket number will be automatically incremented
                    foreach (var transaction in transactions)
                    {
                        var plantIds = transaction.Plants.Select(f => f.Id);
                        var inventoriesAndQuantities = new List<KeyValuePair<int, int?>>();

                        var docket = await _repository.Checkout(transaction.Job, transaction.WhenStarted, plantIds, inventoriesAndQuantities);

                        transaction.Docket = docket.ToString();
                    }

                    return View("CheckoutReceipt", model);

                default:
                    return RedirectToAction("Checkout", new { whenStarted = model.WhenStarted.ToShortDateString(), scans = model.Scans });
            }
        }

        private async Task<IEnumerable<CheckoutTransaction>> GetCheckoutTransactionsAsync(BatchCheckout model)
        {
            var transactions = new List<CheckoutTransaction>();

            if (!string.IsNullOrWhiteSpace(model.Scans))
            {
                var scans = model.Scans.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

                CheckoutTransaction transaction = null;
                var docket = await _repository.GetLastIssuedDocketAsync();

                foreach (var scan in scans)
                {
                    // jobs
                    if (_jobRegex.IsMatch(scan))
                    {
                        // check that the previous job has plant items
                        if (transaction != null && transaction.Plants.Count == 0)
                        {
                            transaction.Job.IsError = true;
                            ModelState.AddModelError(string.Empty, $"The job {transaction.Job.XJobId} has no plant to checkout.");
                        }

                        docket++;
                        transaction = new CheckoutTransaction { Plants = new List<Plant>(), WhenStarted = model.WhenStarted, Docket = docket.ToString() };
                        transactions.Add(transaction);

                        var job = await _repository.Jobs.SingleOrDefaultAsync(
                            f => f.XJobId.Equals(scan, StringComparison.OrdinalIgnoreCase));

                        if (job == null)
                        {
                            job = new Job { XJobId = scan, IsError = true };
                            ModelState.AddModelError(string.Empty, $"The job {scan} could not be found.");
                        }

                        // check a job hasn't already been added
                        if (transaction.Job != null && transactions.Select(f => f.Job).Any(f => f.Id.Equals(job.Id)))
                        {
                            job.IsError = true;
                            ModelState.AddModelError(string.Empty, $"The job {job.XJobId} has been added more than once.");
                        }

                        transaction.Job = job;
                    }
                    // plant
                    else
                    {
                        // ensure that a job is scanned first
                        if (transaction == null)
                        {
                            transaction = new CheckoutTransaction
                            {
                                Job = new Job { XJobId = "?", IsError = true },
                                Docket = "?",
                                Plants = new List<Plant>(),
                                WhenStarted = DateTime.Now.Date
                            };

                            transactions.Add(transaction);

                            ModelState.AddModelError(string.Empty, $"A job must be scanned first.");
                        }

                        var plant = await _repository.Plants.SingleOrDefaultAsync(f =>
                            f.XPlantNewId.Equals(scan, StringComparison.OrdinalIgnoreCase) ||
                            f.XPlantId.Equals(scan, StringComparison.OrdinalIgnoreCase));

                        if (plant == null)
                        {
                            plant = new Plant { XPlantId = scan, XPlantNewId = scan, IsError = true };
                            ModelState.AddModelError(string.Empty, $"The plant {scan} could not be found.");
                        }
                        else if (plant.Status.Id != Status.Available)
                        {
                            ModelState.AddModelError(string.Empty, $"The plant {scan} is not available.");
                        }

                        // check a plant item hasn't already been added
                        if (transactions.SelectMany(f => f.Plants).Any(f =>
                            f.XPlantNewId.Equals(scan, StringComparison.OrdinalIgnoreCase) ||
                            f.XPlantId.Equals(scan, StringComparison.OrdinalIgnoreCase)))
                        {
                            plant.IsError = true;
                            ModelState.AddModelError(string.Empty, $"The plant {scan} has been added more than once.");
                        }

                        transaction.Plants.Add(plant);
                    }
                }

                // check that the last job has plant items
                if (transaction != null && transaction.Plants.Count == 0)
                {
                    transaction.Job.IsError = true;
                    ModelState.AddModelError(string.Empty, $"The job {transaction.Job.XJobId} has no plant to checkout.");
                }
            }

            if (!transactions.Any())
            {
                ModelState.AddModelError(string.Empty, "Nothing was scanned to checkout.");
            }

            return transactions;
        }

        #endregion

        #region Checkin

        public ActionResult Checkin(string whenEnded, int? statusId, string scans)
        {
            DateTime parsedDate;

            if (!DateTime.TryParse(whenEnded, out parsedDate))
            {
                parsedDate = DateTime.Now;
            }

            if (statusId == null)
            {
                statusId = Status.UnderRepair;
            }

            var model = new BatchCheckin
            {
                WhenEnded = parsedDate.Date,
                StatusId = statusId.Value,
                Scans = scans
            };

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Checkin(BatchCheckin model)
        {
            var transactions = await GetCheckinTransactionsAsync(model);

            model.CheckinTransactions = transactions;

            return View("ConfirmCheckin", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ConfirmCheckin(BatchCheckin model, string command)
        {
            switch (command)
            {
                case "Checkin":
                    var transactions = await GetCheckinTransactionsAsync(model);
                    model.CheckinTransactions = transactions;

                    // one more check before the actual commit
                    if (!ModelState.IsValid)
                    {
                        return View("ConfirmCheckin", model);
                    }

                    // commit each transaction
                    foreach (var transaction in transactions)
                    {
                        var plantIds = transaction.PlantHires.Select(f => f.Id);
                        var inventoriesAndQuantities = new List<KeyValuePair<int, int?>>();

                        await _repository.Checkin(transaction.Job, transaction.ReturnDocket, transaction.WhenEnded, model.StatusId, plantIds, inventoriesAndQuantities);
                    }

                    return View("CheckinReceipt", model);

                default:
                    return RedirectToAction("Checkin", new { whenStarted = model.WhenEnded.ToShortDateString(), statusId = model.StatusId, scans = model.Scans });
            }
        }

        private async Task<IEnumerable<CheckinTransaction>> GetCheckinTransactionsAsync(BatchCheckin model)
        {
            bool nextScanIsDocket = false;
            string docket = null;
            var transactions = new List<CheckinTransaction>();

            if (!string.IsNullOrWhiteSpace(model.Scans))
            {
                var scans = model.Scans.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                var errorJob = new Job { Id = -1, Description = "?", IsError = true };

                foreach (var scan in scans)
                {
                    // return docket
                    if (scan.StartsWith("DOCKET"))
                    {
                        nextScanIsDocket = true;
                        continue;
                    }
                    else if (nextScanIsDocket)
                    {
                        docket = scan;
                        nextScanIsDocket = false;
                        continue;
                    }
                    // statuses
                    else if (_statusRegex.IsMatch(scan))
                    {
                        var status = await _repository.Statuses.SingleOrDefaultAsync(f => f.Name.Equals(scan, StringComparison.OrdinalIgnoreCase));

                        if (status != null)
                        {
                            model.Status = status;
                            model.StatusId = status.Id;
                        }
                    }
                    // plant
                    else
                    {
                        var hires = await _repository.PlantHires.Where(f =>
                        f.Plant.StatusId.Equals(Status.CheckedOut) &&
                        (f.Plant.XPlantNewId.Equals(scan, StringComparison.OrdinalIgnoreCase) ||
                         f.Plant.XPlantId.Equals(scan, StringComparison.OrdinalIgnoreCase)))
                         .ToListAsync();

                        var hire = hires.SingleOrDefault(f => f.IsCheckedOut);

                        if (hire == null)
                        {
                            hire = new PlantHire { JobId = errorJob.Id, Job = errorJob, Plant = new Plant { XPlantId = scan, XPlantNewId = scan, IsError = true } };
                            ModelState.AddModelError(string.Empty, $"The plant {scan} could not be found.");
                        }
                        else if (hire.Plant.Status.Id != Status.CheckedOut)
                        {
                            ModelState.AddModelError(string.Empty, $"The plant {scan} is not checked out.");
                        }

                        // check a plant item hasn't already been added
                        if (transactions.SelectMany(f => f.PlantHires).Any(f =>
                            f.Plant.XPlantNewId.Equals(scan, StringComparison.OrdinalIgnoreCase) ||
                            f.Plant.XPlantId.Equals(scan, StringComparison.OrdinalIgnoreCase)))
                        {
                            hire.Plant.IsError = true;
                            ModelState.AddModelError(string.Empty, $"The plant {scan} has been added more than once.");
                        }

                        // check the docket exists
                        if (string.IsNullOrWhiteSpace(docket))
                        {
                            hire.Plant.IsError = true;
                            ModelState.AddModelError(string.Empty, $"There is no docket for the plant {scan}.");
                        }

                        // find the job for the transaction
                        var transaction = transactions.SingleOrDefault(f => f.JobId.Equals(hire.JobId));

                        if (transaction == null)
                        {
                            transaction = new CheckinTransaction { Job = hire.Job, WhenEnded = model.WhenEnded, PlantHires = new List<PlantHire>() };
                            transactions.Add(transaction);
                        }

                        transaction.ReturnDocket = docket;
                        transaction.PlantHires.Add(hire);
                    }
                }
            }

            if (!transactions.Any())
            {
                ModelState.AddModelError(string.Empty, "Nothing was scanned to checkin.");
            }

            return transactions;
        }

        #endregion

        private new ActionResult View(object model)
        {
            return View(null, model);
        }

        private new ActionResult View(string viewName, object model)
        {
            var statuses = _repository.Statuses.Where(f => !f.Id.Equals(Status.CheckedOut)).OrderBy(f => f.Id);

            if (model is BatchCheckin)
            {
                var checkin = model as BatchCheckin;
                checkin.Status = statuses.SingleOrDefault(f => f.Id.Equals(checkin.StatusId));
                ViewBag.Statuses = new SelectList(statuses, "Id", "Name", checkin.StatusId);
            }
            else
            {
                ViewBag.Statuses = new SelectList(statuses, "Id", "Name");
            }

            if (string.IsNullOrEmpty(viewName))
            {
                return base.View(model);
            }

            return base.View(viewName, model);
        }
    }
}