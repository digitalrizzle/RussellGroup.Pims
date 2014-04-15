using RussellGroup.Pims.DataAccess.Models;
using RussellGroup.Pims.DataAccess.ReportModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RussellGroup.Pims.DataAccess.Respositories
{
    public class ReportDbRepository : IReportRepository
    {
        private bool _disposed;

        protected PimsDbContext db = new PimsDbContext();

        public IQueryable<Job> Jobs
        {
            get { return db.Jobs; }
        }

        public IQueryable<Category> Categories
        {
            get { return db.Categories; }
        }

        public IQueryable<PlantHire> GetActivePlantHiresInJob(int? jobId)
        {
            var job = db.Jobs.Find(jobId);
            var hire = job.PlantHires.Where(f => !f.WhenEnded.HasValue).AsQueryable();

            return hire;
        }

        public IQueryable<InventoryHire> GetActiveInventoryHiresInJob(int? jobId)
        {
            var job = db.Jobs.Find(jobId);
            var hire = job.InventoryHires.Where(f => !f.WhenEnded.HasValue).AsQueryable();

            return hire;
        }

        public PlantLocationsReportModel GetPlantLocationsByCategory(int? categoryId)
        {
            var category = db.Categories.Find(categoryId);

            var hire = from h in db.PlantHires
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
            var category = db.Categories.Find(categoryId);

            var hire = from j in db.Jobs
                       join h in db.InventoryHires on j.JobId equals h.JobId
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

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize((object)this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                db.Dispose();
            }

            _disposed = true;
        }
    }
}
