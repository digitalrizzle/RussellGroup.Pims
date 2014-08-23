using RussellGroup.Pims.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RussellGroup.Pims.DataMigration
{
    class ImportJob : AbstractImport
    {
        protected override void Map(OleDbDataReader reader)
        {
            var description = reader.GetValue("Description") ?? "Unnamed";
            var whenEnded = reader.GetDateTime("End date");

            // only include uncompleted jobs
            if (whenEnded.HasValue)
                return;

            var job = new Job
            {
                XJobId = reader.GetValue("Job ID"),
                Description = description,
                WhenStarted = reader.GetDateTime("Start date"),
                WhenEnded = whenEnded,
                ProjectManager = reader.GetValue("Project Manager"),
                QuantitySurveyor = reader.GetValue("QS"),
                Comment = reader.GetValue("Comments")
            };

            TargetContext.Jobs.Add(job);
        }
    }
}
