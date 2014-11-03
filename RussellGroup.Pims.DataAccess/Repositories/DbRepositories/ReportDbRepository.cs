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

        // TODO: complete
        public IQueryable<InventoryHire> GetActiveInventoryHiresInJob(int jobId)
        {
            //return Db.InventoryHires.Where(f => f.JobId == jobId && !f.WhenEnded.HasValue);
            return null;
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

        // TODO: complete
        public InventoryLocationsReportModel GetInventoryLocationsByCategory(int? categoryId)
        {
            //var category = Db.Categories.Find(categoryId);

            //var hire = from j in Db.Jobs
            //           join h in Db.InventoryHires on j.Id equals h.JobId
            //           where h.Inventory.CategoryId == categoryId.Value && !h.WhenEnded.HasValue
            //           group h by j into g
            //           select new InventoryHireInJobReportModel
            //           {
            //               Job = g.Key,
            //               InventoryHires = g.OrderBy(f => f.WhenStarted).ToList()
            //           };

            //var model = new InventoryLocationsReportModel
            //{
            //    Category = category,
            //    InventoryHireInJobs = hire.OrderBy(f => f.Job.XJobId).ToList()
            //};

            //return model;
            return null;
        }

        public IQueryable<Plant> GetPlantCheckedIn()
        {
            return Db.Plants.Where(f => f.PlantHires.All(h => h.WhenEnded.HasValue));
        }

        // TODO: complete
        public IQueryable<Inventory> GetInventoryCheckedIn()
        {
            //return Db.Inventories.Where(f => f.InventoryHires.All(h => h.WhenEnded.HasValue));
            return null;
        }

        public byte[] SummaryOfHireChargesCsv(SummaryOfHireChargesReportViewModel model)
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
                        csv.NextRecord();   // not sure why another line is needed, but ASB does it! (sampled off their CSV export)

                        foreach (var job in model.ActiveJobs.ToList())
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

                return memory.ToArray();
            }
        }
    }
}
