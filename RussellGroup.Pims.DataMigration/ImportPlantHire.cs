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
            var docket = reader.GetValue("Doc no") ?? "Unknown";
            var returnDocket = reader.GetValue("Return doc no");
            var comments = reader.GetValue("Comments");

            DateTime? whenStarted = null;
            DateTime? whenEnded = null;
            try { whenStarted = reader.GetDateTime("Start date"); }
            catch { Trace.WriteLine(string.Format("Bad date: \"{0}\"", key)); }
            try { whenEnded = reader.GetDateTime("End date"); }
            catch { Trace.WriteLine(string.Format("Bad date: \"{0}\"", key)); }

            var plant = TargetContext.Plants.SingleOrDefault(f => f.XPlantId == sourcePlant);

            if (plant != null && sourceJob != null)
            {
                var job = TargetContext.Jobs.SingleOrDefault(f => f.XJobId == sourceJob);

                if (job == null) return;
                if (whenStarted == null) return;

                var hire = new PlantHire
                {
                    PlantId = plant.Id,
                    JobId = job.Id,
                    Docket = docket,
                    ReturnDocket = returnDocket,
                    WhenStarted = whenStarted.Value,
                    WhenEnded = whenEnded,
                    Rate = reader.GetValueOrNull<decimal>("Rate"),
                    Comment = reader.GetValue("Comments")
                };

                bool addHire = false;
                DateTime? whenDisused = hire.WhenEnded.HasValue ? hire.WhenEnded : hire.WhenStarted;

                // don't add plant for these jobs
                // these are duplicated in ImportInventoryHire
                switch (job.XJobId)
                {
                    case "940":  // available
                        plant.StatusId = Status.Available;
                        break;
                    case "941":  // in the yard but unavailable
                        plant.StatusId = Status.WrittenOff;
                        plant.WhenDisused = whenDisused;
                        break;
                    case "950":  // missing (which is now classified as stolen)
                    case "960":  // stolen
                    case "961":
                    case "962":
                    case "963":
                        plant.StatusId = Status.Stolen;
                        plant.WhenDisused = whenDisused;
                        break;
                    case "970":  // repairs
                        plant.StatusId = Status.UnderRepair;
                        plant.WhenDisused = whenDisused;
                        break;
                    case "980":  // written off
                    case "981":
                    case "982":
                    case "984":
                        plant.StatusId = Status.WrittenOff;
                        plant.WhenDisused = whenDisused;
                        break;
                    default:
                        addHire = true;
                        break;
                }

                if (addHire)
                {
                    TargetContext.PlantHires.Add(hire);
                }
                else
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
