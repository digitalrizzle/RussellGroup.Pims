using RussellGroup.Pims.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RussellGroup.Pims.DataMigration
{
    class ImportContact : AbstractImport
    {
        protected override void Map(OleDbDataReader reader)
        {
            var projectManager = reader.GetValue("Project manager");
            var quantitySurveyor = reader.GetValue("QS");

            if (!string.IsNullOrWhiteSpace(projectManager) && !TargetContext.Contacts.Any(f => f.Name.Equals(projectManager, StringComparison.OrdinalIgnoreCase)))
            {
                TargetContext.Contacts.Add(new Contact { Name = projectManager });
            }

            TargetContext.SaveChanges();

            if (!string.IsNullOrWhiteSpace(quantitySurveyor) && !TargetContext.Contacts.Any(f => f.Name.Equals(quantitySurveyor, StringComparison.OrdinalIgnoreCase)))
            {
                TargetContext.Contacts.Add(new Contact { Name = quantitySurveyor });
            }

            TargetContext.SaveChanges();
        }
    }
}
