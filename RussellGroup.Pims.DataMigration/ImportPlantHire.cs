using RussellGroup.Pims.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RussellGroup.Pims.DataMigration
{
    class ImportPlantHire : AbstractImport
    {
        protected override void Map(OleDbDataReader reader)
        {
            var key = reader.GetValue(SourcePrimaryKeyColumnName);
            var sourcePlant = reader.GetValue("Plant no");
            var sourceJob = reader.GetValue("Job ID");

            var docket = reader.GetValue("Doc no");
            var returnDocket = reader.GetValue("Return doc no");
            if (!string.IsNullOrEmpty(returnDocket) && returnDocket.Length >= 5)
            {
                docket = returnDocket;
                Trace.Write(string.Format("Docket overwritten: \"{0}\"\r", key));
            }

            var plant = TargetContext.Plants.SingleOrDefault(f => f.XPlantId == sourcePlant);

            DateTime? whenStarted = null;
            DateTime? whenEnded = null;
            try { whenStarted = reader.GetDateTime("Start date"); } catch { Trace.WriteLine(string.Format("Bad date: \"{0}\"", key)); }
            try { whenEnded = reader.GetDateTime("End date"); } catch { Trace.WriteLine(string.Format("Bad date: \"{0}\"", key)); }                

            if (plant != null)
            {
                var hire = new PlantHire
                {
                    PlantId = plant.PlantId,
                    JobId = TargetContext.Jobs.Single(f => f.XJobId == sourceJob).JobId,
                    Docket = docket,
                    WhenStarted = whenStarted,
                    WhenEnded = whenEnded,
                    Rate = reader.GetValueOrNull<decimal>("Rate"),
                    Comment = reader.GetValue("Comments"),
                };

                TargetContext.PlantHires.Add(hire);
            }
            else
            {
                Trace.WriteLine(string.Format("Orphaned: \"{0}\"", key));
            }
        }
    }
}
