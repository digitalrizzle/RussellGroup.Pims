using RussellGroup.Pims.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
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

            var plant = TargetContext.Plants.SingleOrDefault(f => f.XPlantId == sourcePlant);

            DateTime? whenStarted = null;
            DateTime? whenEnded = null;
            try { whenStarted = reader.GetDateTime("Start date"); } catch { Console.WriteLine("Bad date: \"{0}\"", key); }
            try { whenEnded = reader.GetDateTime("End date"); } catch { Console.WriteLine("Bad date: \"{0}\"", key); }                

            if (plant != null)
            {
                var hire = new PlantHire
                {
                    PlantId = plant.PlantId,
                    JobId = TargetContext.Jobs.Single(f => f.XJobId == sourceJob).JobId,
                    Docket = reader.GetValue("Doc no"),
                    ReturnDocket = reader.GetValue("Return doc no"),
                    WhenStarted = whenStarted,
                    WhenEnded = whenEnded,
                    Rate = reader.GetValueOrNull<decimal>("Rate"),
                    Comment = reader.GetValue("Comments"),
                };

                TargetContext.PlantHires.Add(hire);
            }
            else
            {
                Console.WriteLine("Orphaned: \"{0}\"", key);
            }
        }
    }
}
