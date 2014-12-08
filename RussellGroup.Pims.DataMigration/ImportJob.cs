using RussellGroup.Pims.DataAccess.Models;
using RussellGroup.Pims.DataAccess.Repositories;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RussellGroup.Pims.DataMigration
{
    class ImportJob : AbstractImport
    {
        protected override void Map(OleDbDataReader reader)
        {
            var xJobId = reader.GetValue("Job ID");
            var description = reader.GetValue("Description") ?? "Unnamed";
            var whenEnded = reader.GetDateTime("End date");

            // only include incomplete jobs
            if (whenEnded.HasValue) return;

            // only add jobs that don't already exist
            if (TargetContext.Jobs.Any(f => f.XJobId == xJobId)) return;

            var job = new Job
            {
                XJobId = xJobId,
                Description = description,
                WhenStarted = reader.GetDateTime("Start date"),
                WhenEnded = whenEnded,
                ProjectManager = reader.GetValue("Project Manager"),
                QuantitySurveyor = reader.GetValue("QS"),
                Comment = reader.GetValue("Comments")
            };

            TargetContext.Jobs.Add(job);
        }

        public ImportJob Delete(IEnumerable<string> xJobIds)
        {
            if (TargetContext != null) TargetContext.Dispose();

            TargetContext = new PimsDbContext();
            TargetContext.SetContextUserName(PimsDbContext.DefaultContextUserName);

            var repository = new JobDbRepository(TargetContext);

            foreach (var xJobId in xJobIds)
            {
                var job = repository.GetAll().SingleOrDefault(f => f.XJobId == xJobId);

                if (job != null)
                {
                    Task.Run(async () => { return await repository.RemoveAsync(job); }).Wait();
                    Trace.WriteLine(string.Format("Deleted job: \"{0}\"", xJobId));
                }
            }

            TargetContext.SaveChanges();
            TargetContext.Dispose();

            return this;
        }

        // removes any jobs that do not have current hire
        public ImportJob Clean()
        {
            if (TargetContext != null) TargetContext.Dispose();

            TargetContext = new PimsDbContext();
            TargetContext.SetContextUserName(PimsDbContext.DefaultContextUserName);

            var jobs = TargetContext.Jobs.Where(f =>
                (!f.PlantHires.Any() || !f.InventoryHires.Any()) ||
                (f.PlantHires.All(h => h.WhenEnded.HasValue) /* && f.InventoryHires.All(h => h.WhenEnded.HasValue) */ ));

            foreach (var job in jobs.ToList())
            {
                if (job != null)
                {
                    Trace.WriteLine(string.Format("Deleting job: \"{0}\"", job.XJobId));

                    if (job.PlantHires.Any())
                    {
                        foreach (var hire in job.PlantHires.ToList()) TargetContext.PlantHires.Remove(hire);
                    }
                    if (job.InventoryHires.Any())
                    {
                        foreach (var hire in job.InventoryHires.ToList()) TargetContext.InventoryHires.Remove(hire);
                    }

                    TargetContext.Jobs.Remove(job);
                    TargetContext.SaveChanges();
                }
            }

            TargetContext.Dispose();

            return this;
        }
    }
}
