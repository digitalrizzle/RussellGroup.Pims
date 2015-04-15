using CsvHelper;
using RussellGroup.Pims.DataAccess.Models;
using RussellGroup.Pims.DataAccess.ReportModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RussellGroup.Pims.DataAccess.Repositories
{
    public class ReportDbRepository : IReportRepository
    {
        protected PimsDbContext Db { get; private set; }

        public ReportDbRepository(PimsDbContext context)
        {
            Db = context;
        }

        public IQueryable<Job> Jobs
        {
            get { return Db.Jobs; }
        }

        public IQueryable<Plant> Plants
        {
            get { return Db.Plants; }
        }

        public IQueryable<Inventory> Inventories
        {
            get { return Db.Inventories; }
        }

        public IQueryable<Category> Categories
        {
            get { return Db.Categories; }
        }

        public PlantLocationsReportModel GetPlantLocationsByCategory(int? categoryId)
        {
            var category = Db.Categories.Find(categoryId);

            var hire = from h in Db.PlantHires
                       where h.Plant.CategoryId == categoryId.Value && !h.WhenEnded.HasValue
                       orderby h.Job.XJobId, h.WhenStarted
                       select h;

            var model = new PlantLocationsReportModel
            {
                Category = category,
                PlantHireInJobs = hire.ToList()
            };

            return model;
        }

        public InventoryLocationsReportModel GetInventoryLocationsByCategory(int? categoryId)
        {
            var category = Db.Categories.Find(categoryId);

            var hire = from j in Db.Jobs
                       join h in Db.InventoryHires on j.Id equals h.JobId
                       where h.Inventory.CategoryId == categoryId.Value
                       group h by j into g
                       select new InventoryHireInJobReportModel
                       {
                           Job = g.Key,
                           InventoryHires = g.OrderBy(f => f.InventoryId).ToList()
                       };

            var model = new InventoryLocationsReportModel
            {
                Category = category,
                InventoryHireInJobs = hire.OrderBy(f => f.Job.XJobId).ToList()
            };

            return model;
        }

        public InventoryHireChargesInJobReportModel GetInventoryHireCharges(Job job, DateTime whenStarted, DateTime whenEnded)
        {
            var charges = new List<InventoryHireChargeReportModel>();

            foreach (var inventory in job
                .InventoryHires
                .Where(f =>
                    ((f is InventoryHireCheckout) && (f as InventoryHireCheckout).WhenStarted < whenEnded.AddDays(1) ||
                    ((f is InventoryHireCheckin) && (f as InventoryHireCheckin).WhenEnded >= whenStarted)))
                .Select(f => f.Inventory)
                .Distinct()
                .OrderBy(f => f.XInventoryId)
                .ToList())
            {
                var quantityTotal = 0;

                var charge = new InventoryHireChargeReportModel
                {
                    Inventory = inventory,
                    ItemCharges = new List<InventoryHireItemChargeReportModel>()
                };

                var hires = inventory
                    .InventoryHires
                    .Where(f => f.Job == job)
                    .ToList();

                foreach (var hire in hires)
                {
                    int days = 0;
                    int quantity = 0;
                    DateTime openingDate;

                    // determine the number of days
                    if (hire is InventoryHireCheckout)
                    {
                        var checkout = hire as InventoryHireCheckout;

                        var hireWhenStarted = whenStarted > checkout.WhenStarted
                            ? whenStarted
                            : checkout.WhenStarted;

                        openingDate = checkout.WhenStarted;
                        days = whenEnded.AddDays(1).Subtract(hireWhenStarted).Days;
                    }
                    else
                    {
                        var checkin = hire as InventoryHireCheckin;

                        openingDate = checkin.WhenEnded;
                        days = whenEnded.AddDays(1).Subtract(checkin.WhenEnded).Days;
                    }

                    quantity = hire.Quantity.GetValueOrDefault() * (hire is InventoryHireCheckout ? 1 : -1);
                    quantityTotal += quantity;

                    // have we calculated the opening balance to the report date yet?
                    if (openingDate < whenStarted)
                    {
                        charge.OpeningBalance = quantityTotal;
                        continue;
                    }

                    // add the charges
                    charge.ItemCharges.Add(new InventoryHireItemChargeReportModel
                    {
                        Inventory = inventory,
                        Docket = hire.Docket,
                        WhenStarted = hire is InventoryHireCheckout ? (hire as InventoryHireCheckout).WhenStarted : (DateTime?)null,
                        WhenEnded = hire is InventoryHireCheckin ? (hire as InventoryHireCheckin).WhenEnded : (DateTime?)null,
                        Days = days,
                        Quantity = quantity
                    });
                }

                charge.Days = whenEnded.AddDays(1).Subtract(whenStarted).Days;

                if (charge.OpeningBalance != 0 || charge.ItemCharges.Any())
                {
                    charges.Add(charge);
                }
            }

            var model = new InventoryHireChargesInJobReportModel
            {
                Job = job,
                WhenStarted = whenStarted,
                WhenEnded = whenEnded,
                Charges = charges
            };

            return model;
        }

        #region Summary

        public IQueryable<Plant> GetAvailablePlant()
        {
            return Db.Plants.Where(f => f.StatusId == Status.Available && f.PlantHires.All(h => h.WhenEnded.HasValue));
        }

        public decimal GetPlantHireCharge(Job job, DateTime whenStarted, DateTime whenEnded)
        {
            decimal cost = 0;

            var hires = job
                .PlantHires
                .Where(f =>
                    (f.WhenStarted < whenEnded.AddDays(1) && f.WhenEnded >= whenStarted) ||
                    (f.WhenStarted < whenEnded.AddDays(1) && !f.WhenEnded.HasValue))
                .ToList();

            foreach (var hire in hires)
            {
                var hireWhenStarted = whenStarted > hire.WhenStarted
                    ? whenStarted
                    : hire.WhenStarted;

                var days = hire.WhenEnded.HasValue
                    ? hire.WhenEnded.Value.AddDays(1).Subtract(hireWhenStarted).Days
                    : whenEnded.AddDays(1).Subtract(hireWhenStarted).Days;

                cost += days * hire.Rate.GetValueOrDefault();
            }

            return cost;
        }

        public async Task<SummaryOfHireChargesReportModel> GetSummaryOfHireChargesAsync(DateTime whenStarted, DateTime whenEnded)
        {
            var model = new SummaryOfHireChargesReportModel
            {
                Jobs = await Db.Jobs.OrderBy(f => f.XJobId).ToListAsync(),
                WhenStarted = whenStarted,
                WhenEnded = whenEnded,
                PlantHireCharges = new Dictionary<Job, decimal>(),
                InventoryHireCharges = new List<InventoryHireChargesInJobReportModel>()
            };

            // populate the plant hire and inventory hire charges
            foreach (var job in model.Jobs)
            {
                model.PlantHireCharges.Add(job, GetPlantHireCharge(job, whenStarted, whenEnded));
                model.InventoryHireCharges.Add(GetInventoryHireCharges(job, whenStarted, whenEnded));
            }

            return model;
        }

        public byte[] GetSummaryOfHireChargesCsv(SummaryOfHireChargesReportModel model)
        {
            using (var memory = new MemoryStream())
            {
                using (var writer = new StreamWriter(memory))
                {
                    using (var csv = new CsvWriter(writer))
                    {
                        csv.WriteField(string.Format("Created: {0}", DateTime.Now.ToString("dd/MM/yyyy h:mm:ss tt"))); csv.NextRecord();
                        csv.WriteField(string.Format("From date: {0}", model.WhenStarted.ToShortDateString())); csv.NextRecord();
                        csv.WriteField(string.Format("To date: {0}", model.WhenEnded.ToShortDateString())); csv.NextRecord();
                        csv.NextRecord();

                        // header
                        csv.WriteField("Job ID");
                        csv.WriteField("Description");
                        csv.WriteField("Plant");
                        csv.WriteField("Alum Scaffolding");
                        csv.WriteField("Other");
                        csv.WriteField("Peri");
                        csv.WriteField("Scaffolding");
                        csv.WriteField("Shoreloading");
                        csv.WriteField("Total");
                        csv.NextRecord();

                        foreach (var job in model.Jobs)
                        {
                            var plantHireCharge = model.GetPlantHireCharge(job);
                            var alumScaffoldingHireCharge = model.GetInventoryHireCharge(job, "Alum Scaffolding");
                            var otherHireCharge = model.GetInventoryHireCharge(job, "Other");
                            var periHireCharge = model.GetInventoryHireCharge(job, "Peri");
                            var scaffoldingHireCharge = model.GetInventoryHireCharge(job, "Scaffolding");
                            var shoreloadingHireCharge = model.GetInventoryHireCharge(job, "Shoreloading");

                            var total =
                                plantHireCharge
                                + alumScaffoldingHireCharge
                                + otherHireCharge
                                + periHireCharge
                                + scaffoldingHireCharge
                                + shoreloadingHireCharge;

                            if (total != 0)
                            {
                                csv.WriteField(job.XJobId);
                                csv.WriteField(job.Description);
                                csv.WriteField(plantHireCharge);
                                csv.WriteField(alumScaffoldingHireCharge);
                                csv.WriteField(otherHireCharge);
                                csv.WriteField(periHireCharge);
                                csv.WriteField(scaffoldingHireCharge);
                                csv.WriteField(shoreloadingHireCharge);
                                csv.WriteField(total);
                                csv.NextRecord();
                            }
                        }
                    }
                }

                return memory.ToArray();
            }
        }

        #endregion
    }
}
