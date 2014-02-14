using RussellGroup.Pims.DataAccess.Models;
using RussellGroup.Pims.DataAccess.ViewModels;
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
        protected PimsContext db = new PimsContext();

        public async Task<Job> GetJob(int? id)
        {
            return await db.Jobs.SingleOrDefaultAsync(f => f.JobId == id);
        }

        public IQueryable<Job> Jobs
        {
            get { return db.Jobs; }
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

        public void Dispose()
        {
            db.Dispose();
        }
    }
}
