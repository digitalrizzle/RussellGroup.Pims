using CsvHelper;
using RussellGroup.Pims.DataAccess.Models;
using RussellGroup.Pims.DataAccess.ReportModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
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

        public IQueryable<Category> Categories
        {
            get { return Db.Categories; }
        }

        public IQueryable<PlantHire> GetActivePlantHiresInJob(int jobId)
        {
            return Db.PlantHires.Where(f => f.JobId == jobId && !f.WhenEnded.HasValue);
        }

        public IQueryable<InventoryHire> GetActiveInventoryHiresInJob(int jobId)
        {
            return Db.InventoryHires.Where(f => f.JobId == jobId && !f.WhenEnded.HasValue);
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
                       where h.Inventory.CategoryId == categoryId.Value && !h.WhenEnded.HasValue
                       group h by j into g
                       select new InventoryHireInJobReportModel
                       {
                           Job = g.Key,
                           InventoryHires = g.OrderBy(f => f.WhenStarted).ToList()
                       };

            var model = new InventoryLocationsReportModel
            {
                Category = category,
                InventoryHireInJobs = hire.OrderBy(f => f.Job.XJobId).ToList()
            };

            return model;
        }

        public IQueryable<Plant> GetPlantCheckedIn()
        {
            return Db.Plants.Where(f => f.PlantHires.All(h => h.WhenEnded.HasValue));
        }

        public IQueryable<Inventory> GetInventoryCheckedIn()
        {
            return Db.Inventories.Where(f => f.InventoryHires.All(h => h.WhenEnded.HasValue));
        }

        [Obsolete]
        public byte[] GetInventoryChargesCsv(int? jobId, DateTime whenStarted, DateTime whenEnded)
        {
            using (var memory = new MemoryStream())
            {
                using (var writer = new StreamWriter(memory))
                {
                    using (var csv = new CsvWriter(writer))
                    {
                        var job = Db.Jobs.Find(jobId);

                        csv.WriteField(string.Format("Created: {0}", DateTime.Now.ToString("dd/MM/yyyy h:mm:ss tt"))); csv.NextRecord();
                        csv.WriteField(string.Format("Job: {0}", job.Description)); csv.NextRecord();
                        csv.WriteField(string.Format("Project Manager: {0}", job.ProjectManager)); csv.NextRecord();
                        csv.WriteField(string.Format("From date: {0}", whenStarted.ToShortDateString())); csv.NextRecord();
                        csv.WriteField(string.Format("To date: {0}", whenEnded.ToShortDateString())); csv.NextRecord();
                        csv.NextRecord();

                        // header
                        csv.WriteField("Category");
                        csv.WriteField("Quantity");
                        csv.WriteField("Cost");
                        csv.NextRecord();
                        csv.NextRecord();   // not sure why another line is needed, but ASB does it! (sampled off their CSV export)

                        foreach (var category in job.InventoryHires
                            .Where(f => (f.WhenStarted < whenEnded.AddDays(1) && f.WhenEnded >= whenStarted) || (f.WhenStarted < whenEnded.AddDays(1) && !f.WhenEnded.HasValue))
                            .Select(f => f.Inventory.Category)
                            .Distinct()
                            .OrderBy(f => f.Name)
                            .ToList())
                        {
                            int totalQuantity = 0, quantity = 0;
                            decimal totalCost = 0, cost = 0;

                            foreach (var item in job.InventoryHires
                                .Where(f => f.Inventory.Category == category)
                                .Where(f => (f.WhenStarted < whenEnded.AddDays(1) && f.WhenEnded >= whenStarted) || (f.WhenStarted < whenEnded.AddDays(1) && !f.WhenEnded.HasValue))
                                .OrderBy(f => f.Inventory.XInventoryId))
                            {
                                quantity += 1;
                                cost = DateTime.Now.Subtract(whenStarted).Days * item.Rate.Value;

                                totalQuantity += 1;
                                totalCost += cost;
                            }

                            csv.WriteField(category.Name);
                            csv.WriteField(totalQuantity);
                            csv.WriteField(totalCost);
                            csv.NextRecord();
                        }
                    }
                }

                return memory.ToArray();
            }
        }
    }
}
