﻿using DataTables.Mvc;
using LinqKit;
using RussellGroup.Pims.DataAccess.Models;
using RussellGroup.Pims.DataAccess.Repositories;
using RussellGroup.Pims.Website.Generators;
using RussellGroup.Pims.Website.Helpers;
using RussellGroup.Pims.Website.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace RussellGroup.Pims.Website.Controllers
{
    [PimsAuthorize(Role.CanEdit)]
    public class BatchController : Controller
    {
        private readonly ITransactionRepository _repository;
        private static readonly Regex _jobRegex = new Regex(@"[a-z]{2}\d{4}", RegexOptions.IgnoreCase);
        private static readonly Regex _statusRegex = new Regex(@"UNKNOWN|AVAILABLE|CHECKED OUT|STOLEN|UNDER REPAIR|WRITTEN OFF", RegexOptions.IgnoreCase);

        public BatchController(ITransactionRepository repository)
        {
            _repository = repository;
        }

        public ActionResult Receipts(bool? resendSuccess)
        {
            if (resendSuccess.HasValue)
            {
                ViewBag.Success = resendSuccess.Value;
                ViewBag.Message = resendSuccess.Value ? "The receipts have been sent." : "There was a problem resending the receipts.";
            }

            return View("Receipts");
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

            var all = _repository.Receipts.AsExpandable();

            // filter
            var filtered = string.IsNullOrEmpty(hint)
                ? all
                : all.Where(f =>
                    f.TransactionType.Name.Contains(hint) ||
                    Extensions.LittleEndianDateString.Invoke(f.WhenCreated).Contains(hint) ||
                    f.Docket.Contains(hint));

            // ordering
            var sortColumnName = string.IsNullOrEmpty(sortColumn.Name) ? sortColumn.Data : sortColumn.Name;
            Func<Receipt, IComparable> ordering = (c => c.GetValue(sortColumnName.Split('.')));

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
                    TransactionType = c.TransactionType.Name,
                    WhenCreated = c.WhenCreated.ToString("dd/MM/yyyy h:mm:ss tt"),
                    XJobId = c.Job.XJobId,
                    c.Docket,
                    CrudLinks =
                        $"<a href=\"{Url.Action("Resend", new { id = c.Id })}\">Resend</a> | " +
                        $"<a href=\"{Url.Action("Receipt", new { id = c.Id })}\" class=\"download\" target=\"_blank\">View</a>"
                });

            return Json(new DataTablesResponse(model.Draw, paged, filtered.Count(), all.Count()), JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> Receipt(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var receipt = await _repository.Receipts.Include("Content").SingleOrDefaultAsync(f => f.Id.Equals(id.Value));
            if (receipt == null)
            {
                return HttpNotFound();
            }

            return File(receipt.Content.Data, receipt.Content.ContentType);
        }

        public async Task<ActionResult> Resend(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var receipt = await _repository.Receipts.Include("Content").SingleOrDefaultAsync(f => f.Id.Equals(id.Value));
            if (receipt == null)
            {
                return HttpNotFound();
            }

            return View(receipt);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Resend(Receipt receipt)
        {
            if (receipt == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            receipt = await _repository.Receipts.Include("Content").SingleOrDefaultAsync(f => f.Id.Equals(receipt.Id));
            if (receipt == null)
            {
                return HttpNotFound();
            }

            bool resendSuccess = false;

            if (!string.IsNullOrEmpty(receipt.Job.NotificationEmail))
            {
                try
                {
                    receipt.Recipients = receipt.Job.NotificationEmail;

                    MailHelper.Send(receipt);
                    resendSuccess = true;
                }
                catch (Exception ex)
                {
                    // do nothing
                }
            }

            return RedirectToAction("Receipts", new { resendSuccess });
        }

        public ActionResult Barcodes()
        {
            return View();
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

            statuses.Add(new Status { Name = "Docket" });
            statuses.Add(new Status { Name = "Auto Docket" });

            return View(statuses);
        }

        #region Checkout

        [HttpGet]
        public ActionResult Checkout(string whenStarted, string scans, bool? sendSuccess)
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

            if (sendSuccess.HasValue)
            {
                ViewBag.Success = sendSuccess.Value;
                ViewBag.Message = sendSuccess.Value ? "The receipts have been created and sent." : "There was a problem sending the receipts.";
            }

            return View("Checkout", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ConfirmCheckout(BatchCheckout model)
        {
            var transactions = await GetCheckoutTransactionsAsync(model);

            model.CheckoutTransactions = transactions;

            return View("ConfirmCheckout", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CommitCheckout(BatchCheckout model, string command)
        {
            switch (command)
            {
                case "Checkout":
                    var sendSuccess = false;
                    var transactions = await GetCheckoutTransactionsAsync(model);
                    model.CheckoutTransactions = transactions;

                    // one more check before the actual commit
                    if (!ModelState.IsValid)
                    {
                        return View("ConfirmCheckout", model);
                    }

                    // TODO: move the auto docket number into one transaction
                    // commit each transaction; the docket number will be automatically incremented
                    foreach (var transaction in transactions)
                    {
                        var plantIds = transaction.Plants.Select(f => f.Id);
                        var inventoriesAndQuantities = new List<KeyValuePair<int, int?>>();

                        var docket = await _repository.CheckoutAsync(transaction.Job, transaction.WhenStarted, plantIds, inventoriesAndQuantities);

                        transaction.Docket = TransactionDbRepository.FormatDocket(docket);
                    }

                    // generate and store a receipt per job
                    foreach (var job in model.CheckoutTransactions.Select(f => f.Job).Distinct())
                    {
                        byte[] render = new ReceiptPdfGenerator().Create(model, job);

                        var receipt = new Receipt
                        {
                            Job = job,
                            JobId = job.Id,
                            TransactionType = _repository.TransactionTypes.Single(f => f.Id == TransactionType.Checkout),
                            TransactionTypeId = TransactionType.Checkout,
                            UserName = HttpContext.User.Identity.Name,
                            WhenCreated = DateTime.Now,
                            Docket = string.Join(", ", model.CheckoutTransactions.Where(f => f.JobId == job.Id).Select(f => f.Docket).Distinct()),
                            ProjectManager = job.ProjectManager,
                            Recipients = job.NotificationEmail,
                            Content = new Content(render)
                        };

                        await _repository.StoreAsync(receipt);

                        try
                        {
                            if (!string.IsNullOrEmpty(job.NotificationEmail))
                            {
                                MailHelper.Send(receipt);
                                sendSuccess = true;
                            }
                        }
                        catch (Exception ex)
                        {
                            // do nothing
                        }
                    }

                    return RedirectToAction("Checkout", new { whenStarted = model.WhenStarted.ToShortDateString(), sendSuccess });

                case "Retry":
                    return RedirectToAction("Checkout", new { whenStarted = model.WhenStarted.ToShortDateString(), scans = model.Scans });

                default:
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
        }

        private async Task<IEnumerable<CheckoutTransaction>> GetCheckoutTransactionsAsync(BatchCheckout model)
        {
            var transactions = new List<CheckoutTransaction>();

            if (!string.IsNullOrWhiteSpace(model.Scans))
            {
                // split into lines and trim the whitespace
                var scans = model.Scans.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).Select(f => f.Trim()).Where(f => !string.IsNullOrWhiteSpace(f));

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
                            ModelState.AddModelError(string.Empty, $"The job {transaction.Job.XJobId} has no plant to check out.");
                        }

                        // this is only a temporary docket number, it is assigned during commit
                        docket++;
                        transaction = new CheckoutTransaction { Plants = new List<Plant>(), WhenStarted = model.WhenStarted, Docket = TransactionDbRepository.FormatDocket(docket) };
                        transactions.Add(transaction);

                        var job = await _repository.Jobs.SingleOrDefaultAsync(
                            f => f.XJobId.Equals(scan, StringComparison.OrdinalIgnoreCase));

                        if (job == null)
                        {
                            job = new Job { XJobId = scan, IsError = true };
                            ModelState.AddModelError(string.Empty, $"The job {scan} could not be found.");
                        }

                        // check the job hasn't already been added
                        if (transactions.Select(f => f.Job).Any(f => f != null && f.Id.Equals(job.Id)))
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

                        // only the last five digits are used for plant scans
                        var plantScan = scan.Length >= 5 ? scan.Substring(scan.Length - 5, 5) : scan;

                        var plant = await _repository.Plants.SingleOrDefaultAsync(f =>
                            f.XPlantNewId.Equals(plantScan, StringComparison.OrdinalIgnoreCase) ||
                            f.XPlantId.Equals(plantScan, StringComparison.OrdinalIgnoreCase));

                        if (plant == null)
                        {
                            plant = new Plant { XPlantId = plantScan, XPlantNewId = plantScan, IsError = true };
                            ModelState.AddModelError(string.Empty, $"The plant {plantScan} could not be found.");
                        }
                        else if (plant.Status.Id != Status.Available)
                        {
                            plant.IsError = true;
                            ModelState.AddModelError(string.Empty, $"The plant {plantScan} is not available.");
                        }

                        // check a plant item hasn't already been added
                        if (transactions.SelectMany(f => f.Plants).Any(f =>
                            f.XPlantNewId.Equals(plantScan, StringComparison.OrdinalIgnoreCase) ||
                            f.XPlantId.Equals(plantScan, StringComparison.OrdinalIgnoreCase)))
                        {
                            plant.IsError = true;
                            ModelState.AddModelError(string.Empty, $"The plant {plantScan} has been added more than once.");
                        }

                        transaction.Plants.Add(plant);
                    }
                }

                // check that the last job has plant items
                if (transaction != null && transaction.Plants.Count == 0)
                {
                    transaction.Job.IsError = true;
                    ModelState.AddModelError(string.Empty, $"The job {transaction.Job.XJobId} has no plant to check out.");
                }
            }

            if (!transactions.Any())
            {
                ModelState.AddModelError(string.Empty, "Nothing was scanned to check out.");
            }

            return transactions;
        }

        #endregion

        #region Checkin

        [HttpGet]
        public ActionResult Checkin(string whenEnded, string scans, bool? sendSuccess)
        {
            DateTime parsedDate;

            if (!DateTime.TryParse(whenEnded, out parsedDate))
            {
                parsedDate = DateTime.Now;
            }

            var model = new BatchCheckin
            {
                WhenEnded = parsedDate.Date,
                Scans = scans
            };

            if (sendSuccess.HasValue)
            {
                ViewBag.Success = sendSuccess.Value;
                ViewBag.Message = sendSuccess.Value ? "The receipts have been created and sent." : "There was a problem sending the receipts.";
            }

            return View("Checkin", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ConfirmCheckin(BatchCheckin model)
        {
            var transactions = await GetCheckinTransactionsAsync(model);

            model.CheckinTransactions = transactions;

            return View("ConfirmCheckin", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CommitCheckin(BatchCheckin model, string command)
        {
            switch (command)
            {
                case "Checkin":
                    var sendSuccess = false;
                    var transactions = await GetCheckinTransactionsAsync(model);
                    model.CheckinTransactions = transactions;

                    // one more check before the actual commit
                    if (!ModelState.IsValid)
                    {
                        return View("ConfirmCheckin", model);
                    }

                    // TODO: move the auto docket number into one transaction
                    // commit each transaction; auto generate the return docket if required
                    foreach (var transaction in transactions)
                    {
                        var plantIds = transaction.PlantHires.Select(f => f.Id);
                        var inventoriesAndQuantities = new List<KeyValuePair<int, int?>>();

                        if (transaction.IsAutoDocket)
                        {
                            var docket = await _repository.CheckinAsync(transaction.Job, transaction.WhenEnded, model.StatusId, plantIds, inventoriesAndQuantities);
                            transaction.ReturnDocket = TransactionDbRepository.FormatDocket(docket);
                        }
                        else
                        {
                            await _repository.CheckinAsync(transaction.Job, transaction.ReturnDocket, transaction.WhenEnded, model.StatusId, plantIds, inventoriesAndQuantities);
                        }
                    }

                    // generate and store a receipt per job
                    foreach (var job in model.CheckinTransactions.Select(f => f.Job).Distinct())
                    {
                        byte[] render = new ReceiptPdfGenerator().Create(model, job);

                        // store the receipt data
                        var receipt = new Receipt
                        {
                            Job = job,
                            JobId = job.Id,
                            TransactionType = _repository.TransactionTypes.Single(f => f.Id == TransactionType.Checkin),
                            TransactionTypeId = TransactionType.Checkin,
                            UserName = HttpContext.User.Identity.Name,
                            WhenCreated = DateTime.Now,
                            Docket = string.Join(", ", model.CheckinTransactions.Where(f => f.JobId == job.Id).Select(f => f.ReturnDocket).Distinct()),
                            ProjectManager = job.ProjectManager,
                            Recipients = job.NotificationEmail,
                            Content = new Content(render)
                        };

                        await _repository.StoreAsync(receipt);

                        try
                        {
                            if (!string.IsNullOrEmpty(job.NotificationEmail))
                            {
                                MailHelper.Send(receipt);
                                sendSuccess = true;
                            }
                        }
                        catch (Exception ex)
                        {
                            // do nothing
                        }
                    }

                    return RedirectToAction("Checkin", new { whenEnded = model.WhenEnded.ToShortDateString(), sendSuccess });

                case "Retry":
                    return RedirectToAction("Checkin", new { whenEnded = model.WhenEnded.ToShortDateString(), scans = model.Scans });

                default:
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
        }

        private async Task<IEnumerable<CheckinTransaction>> GetCheckinTransactionsAsync(BatchCheckin model)
        {
            string docket = null;
            var nextScanIsDocket = false;
            var isAutoDocket = false;
            var autoDocket = await _repository.GetLastIssuedDocketAsync();

            string statusScan = null;
            var transactions = new List<CheckinTransaction>();

            if (!string.IsNullOrWhiteSpace(model.Scans))
            {
                // split into lines and trim the whitespace
                var scans = model.Scans.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).Select(f => f.Trim()).Where(f => !string.IsNullOrWhiteSpace(f));
                var errorJob = new Job { Id = -1, Description = "?", IsError = true };

                foreach (var scan in scans)
                {
                    // system assigned return docket
                    if (scan.StartsWith("AUTO DOCKET", StringComparison.OrdinalIgnoreCase))
                    {
                        isAutoDocket = true;
                        continue;
                    }
                    // return docket
                    if (scan.StartsWith("DOCKET", StringComparison.OrdinalIgnoreCase))
                    {
                        nextScanIsDocket = true;
                        continue;
                    }
                    else if (nextScanIsDocket)
                    {
                        docket = scan;
                        isAutoDocket = false;
                        nextScanIsDocket = false;
                        continue;
                    }
                    // statuses
                    else if (_statusRegex.IsMatch(scan))
                    {
                        if (string.IsNullOrEmpty(statusScan))
                        {
                            statusScan = scan;
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, "The status can only be set once.");
                        }
                    }
                    // plant
                    else
                    {
                        // only the last five digits are used for plant scans
                        var plantScan = scan.Length >= 5 ? scan.Substring(scan.Length - 5, 5) : scan;

                        var hires = await _repository
                            .PlantHires
                            .Where(f =>
                                (f.Plant.XPlantNewId.Equals(plantScan, StringComparison.OrdinalIgnoreCase) ||
                                f.Plant.XPlantId.Equals(plantScan, StringComparison.OrdinalIgnoreCase)))
                            .ToListAsync();

                        var hire = hires.SingleOrDefault(f => f.IsCheckedOut);

                        if (!hires.Any())
                        {
                            hire = new PlantHire { JobId = errorJob.Id, Job = errorJob, Plant = new Plant { XPlantId = scan, XPlantNewId = scan, IsError = true } };
                            ModelState.AddModelError(string.Empty, $"The plant {plantScan} could not be found.");
                        }
                        else if (hire == null)
                        {
                            hire = new PlantHire { JobId = errorJob.Id, Job = errorJob, Plant = new Plant { XPlantId = scan, XPlantNewId = scan, IsError = true } };
                            ModelState.AddModelError(string.Empty, $"The plant {plantScan} is not checked out.");
                        }

                        // check a plant item hasn't already been added
                        if (transactions.SelectMany(f => f.PlantHires).Any(f =>
                            f.Plant.XPlantNewId.Equals(scan, StringComparison.OrdinalIgnoreCase) ||
                            f.Plant.XPlantId.Equals(scan, StringComparison.OrdinalIgnoreCase)))
                        {
                            hire.Plant.IsError = true;
                            ModelState.AddModelError(string.Empty, $"The plant {plantScan} has been added more than once.");
                        }

                        // check the docket exists
                        if (!isAutoDocket && string.IsNullOrWhiteSpace(docket))
                        {
                            hire.Plant.IsError = true;
                            ModelState.AddModelError(string.Empty, $"There is no docket for the plant {plantScan}.");
                        }

                        // find the job for the transaction
                        var transaction = transactions.SingleOrDefault(f => f.JobId.Equals(hire.JobId));

                        if (transaction == null)
                        {
                            transaction = new CheckinTransaction { Job = hire.Job, WhenEnded = model.WhenEnded, ReturnDocket = docket, PlantHires = new List<PlantHire>() };
                            transactions.Add(transaction);
                        }

                        transaction.IsAutoDocket = isAutoDocket;
                        transaction.PlantHires.Add(hire);
                    }
                }

                // default the status scan to under repair
                statusScan = statusScan ?? "UNDER REPAIR";
                var status = await _repository.Statuses.SingleOrDefaultAsync(f => f.Name.Equals(statusScan, StringComparison.OrdinalIgnoreCase));

                if (status != null)
                {
                    model.Status = status;
                    model.StatusId = status.Id;
                }

                if (model.StatusId == Status.CheckedOut)
                {
                    ModelState.AddModelError(string.Empty, $"The status cannot be changed to checked out.");
                }

                // check that plant items were scanned
                if (!transactions.Any())
                {
                    var transaction = new CheckinTransaction { Job = errorJob, WhenEnded = model.WhenEnded, ReturnDocket = docket, PlantHires = new List<PlantHire>() };
                    transactions.Add(transaction);

                    ModelState.AddModelError(string.Empty, $"The was no plant scanned to check in.");
                }

                // issue auto dockets, if any
                foreach (var transaction in transactions.Where(f => f.IsAutoDocket))
                {
                    autoDocket++;
                    transaction.ReturnDocket = TransactionDbRepository.FormatDocket(autoDocket);
                }

                // check that DOCKET and AUTO DOCKET cannot be batched together
                if (transactions.Count >= 2 &&
                    (transactions.Count(f => f.IsAutoDocket) != transactions.Count &&
                     transactions.Count(f => !f.IsAutoDocket) != transactions.Count))
                {
                    var firstTransaction = transactions.First();
                    firstTransaction.Job.IsError = true;

                    var nextDifferentTransaction = transactions.First(f => f.IsAutoDocket != firstTransaction.IsAutoDocket);
                    nextDifferentTransaction.Job.IsError = true;

                    ModelState.AddModelError(string.Empty, $"DOCKET and AUTO DOCKET cannot be batched together.");
                }

                // check that dockets numbers are different between transactions
                var duplicates = transactions
                    .Where(f => !string.IsNullOrEmpty(f.ReturnDocket))
                    .GroupBy(f => f.ReturnDocket)
                    .Where(f => f.Count() > 1)
                    .SelectMany(f => transactions.Where(job => job.ReturnDocket == f.Key))
                    .ToList();

                if (duplicates.Any())
                {
                    ModelState.AddModelError(string.Empty, $"The docket numbers cannot be the same between different jobs.");

                    duplicates.ForEach((transaction) =>
                    {
                        transaction.Job.IsError = true;
                    });
                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Nothing was scanned to check in.");
            }

            return transactions;
        }

        #endregion

        #region Status

        [HttpGet]
        public ActionResult StatusUpdate(string scans)
        {
            var model = new BatchStatus
            {
                Scans = scans
            };

            return View("Status", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ConfirmStatusUpdate(BatchStatus model)
        {
            var plants = await GetStatusUpdatesAsync(model);

            model.Plants = plants;

            return View("ConfirmStatus", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CommitStatusUpdate(BatchStatus model, string command)
        {
            switch (command)
            {
                case "Update Status":
                    var plants = await GetStatusUpdatesAsync(model);
                    model.Plants = plants;

                    // one more check before the actual commit
                    if (!ModelState.IsValid)
                    {
                        return View("ConfirmStatus", model);
                    }

                    // commit each plant
                    foreach (var plant in plants)
                    {
                        await _repository.UpdateStatusAsync(plant.Id, model.StatusId);
                    }

                    return RedirectToAction("StatusUpdate");

                case "Retry":
                    return RedirectToAction("StatusUpdate", new { scans = model.Scans });

                default:
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
        }

        private async Task<IEnumerable<Plant>> GetStatusUpdatesAsync(BatchStatus model)
        {
            string statusScan = null;
            List<Plant> plants = new List<Plant>();

            if (!string.IsNullOrWhiteSpace(model.Scans))
            {
                // split into lines and trim the whitespace
                var scans = model.Scans.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).Select(f => f.Trim()).Where(f => !string.IsNullOrWhiteSpace(f));

                foreach (var scan in scans)
                {
                    // statuses
                    if (_statusRegex.IsMatch(scan))
                    {
                        if (string.IsNullOrEmpty(statusScan))
                        {
                            statusScan = scan;
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, "The status can only be set once.");
                        }
                    }
                    // plant
                    else
                    {
                        // only the last five digits are used for plant scans
                        var plantScan = scan.Length >= 5 ? scan.Substring(scan.Length - 5, 5) : scan;

                        var plant = await _repository.Plants.SingleOrDefaultAsync(f =>
                            f.XPlantNewId.Equals(plantScan, StringComparison.OrdinalIgnoreCase) ||
                            f.XPlantId.Equals(plantScan, StringComparison.OrdinalIgnoreCase));

                        if (plant == null)
                        {
                            plant = new Plant { XPlantId = plantScan, XPlantNewId = plantScan, IsError = true };
                            ModelState.AddModelError(string.Empty, $"The plant {plantScan} could not be found.");
                        }
                        else if (plant.StatusId == Status.CheckedOut)
                        {
                            plant.IsError = true;
                            ModelState.AddModelError(string.Empty, $"The plant {plantScan} is checked out.");
                        }

                        // check a plant item hasn't already been added
                        if (plants.Any(f =>
                            f.XPlantNewId.Equals(plantScan, StringComparison.OrdinalIgnoreCase) ||
                            f.XPlantId.Equals(plantScan, StringComparison.OrdinalIgnoreCase)))
                        {
                            plant.IsError = true;
                            ModelState.AddModelError(string.Empty, $"The plant {plantScan} has been added more than once.");
                        }

                        plants.Add(plant);
                    }
                }

                // default the status scan to under repair
                var status = await _repository.Statuses.SingleOrDefaultAsync(f => f.Name.Equals(statusScan, StringComparison.OrdinalIgnoreCase));

                if (status != null)
                {
                    model.Status = status;
                    model.StatusId = status.Id;

                    // ensure the status is not checked out
                    if (model.StatusId == Status.CheckedOut)
                    {
                        ModelState.AddModelError(string.Empty, $"The status cannot be changed to checked out.");
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, $"There was no status scanned.");
                }

                // check that plant items were scanned
                if (!plants.Any())
                {
                    ModelState.AddModelError(string.Empty, $"There was no plant scanned to update the status.");
                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Nothing was scanned to update the status.");
            }

            return plants;
        }

        #endregion
    }
}