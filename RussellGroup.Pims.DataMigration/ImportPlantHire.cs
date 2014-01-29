using RussellGroup.Pims.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
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
            var comments = reader.GetValue("Comments");

            DateTime? whenStarted = null;
            DateTime? whenEnded = null;
            try { whenStarted = reader.GetDateTime("Start date"); }
            catch { Trace.WriteLine(string.Format("Bad date: \"{0}\"", key)); }
            try { whenEnded = reader.GetDateTime("End date"); }
            catch { Trace.WriteLine(string.Format("Bad date: \"{0}\"", key)); }

            var plant = TargetContext.Plants.SingleOrDefault(f => f.XPlantId == sourcePlant);

            if (plant != null)
            {
                var job = TargetContext.Jobs.Single(f => f.XJobId == sourceJob);

                var hire = new PlantHire
                    {
                        PlantId = plant.PlantId,
                        JobId = job.JobId,
                        Docket = docket,
                        ReturnDocket = returnDocket,
                        WhenStarted = whenStarted,
                        WhenEnded = whenEnded,
                        Rate = reader.GetValueOrNull<decimal>("Rate"),
                        Comment = reader.GetValue("Comments")
                    };

                TargetContext.PlantHires.Add(hire);

                bool updatePlant = true;
                DateTime? whenDisused = hire.WhenEnded.HasValue ? hire.WhenEnded : hire.WhenStarted;

                switch (job.XJobId)
                {
                    case "940":  // available
                        plant.StatusId = 2;
                        break;
                    case "941":  // unavailable
                        plant.StatusId = 3;
                        plant.WhenDisused = whenDisused;
                        break;
                    case "950":  // missing
                        plant.StatusId = 4;
                        plant.WhenDisused = whenDisused;
                        break;
                    case "960":  // stolen
                    case "961":
                    case "962":
                    case "963":
                        plant.StatusId = 5;
                        plant.WhenDisused = whenDisused;
                        break;
                    case "970":  // repairs
                        plant.StatusId = 6;
                        plant.WhenDisused = whenDisused;
                        break;
                    case "980":  // written off
                    case "981":
                    case "982":
                    case "984":
                        plant.StatusId = 7;
                        plant.WhenDisused = whenDisused;
                        break;
                    default:
                        updatePlant = false;
                        break;
                }

                if (updatePlant)
                {
                    TargetContext.Entry(plant).State = EntityState.Modified;
                }
            }
            else
            {
                Trace.WriteLine(string.Format("Orphaned: \"{0}\"", key));
            }
        }
    }
}
