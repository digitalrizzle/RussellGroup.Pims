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
                    repository.RemoveAsync(job).RunSynchronously();
                    Trace.WriteLine(string.Format("Deleted job: \"{0}\"", xJobId));
                }
            }

            TargetContext.SaveChanges();
            TargetContext.Dispose();

            return this;
        }
    }
}
