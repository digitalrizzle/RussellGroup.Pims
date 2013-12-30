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
            var projectManager = reader.GetValue("Project Manager");
            var quantitySurveyor = reader.GetValue("QS");

            var job = new Job
            {
                XJobId = reader.GetValue("Job ID"),
                Description = reader.GetValue("Description"),
                WhenStarted = reader.GetDateTime("Start date"),
                WhenEnded = reader.GetDateTime("End date"),
                ProjectManager = TargetContext.Contacts.SingleOrDefault(f => f.Name.Equals(projectManager, StringComparison.OrdinalIgnoreCase)),
                QuantitySurveyor = TargetContext.Contacts.SingleOrDefault(f => f.Name.Equals(quantitySurveyor, StringComparison.OrdinalIgnoreCase)),
                Comment = reader.GetValue("Comments")
            };

            TargetContext.Jobs.Add(job);
        }
    }
}
