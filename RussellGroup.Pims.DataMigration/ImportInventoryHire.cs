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
    class ImportInventoryHire : AbstractImport
    {
        protected override void Map(OleDbDataReader reader)
        {
            var key = reader.GetValue(SourcePrimaryKeyColumnName);
            var sourceInventory = reader.GetValue("Inv no");
            var sourceJob = reader.GetValue("Job ID");
            var docket = reader.GetValue("Doc no") ?? "Unknown";
            var returnDocket = reader.GetValue("Return doc no");
            decimal? rate = reader.GetValueOrNull<decimal>("Rate");
            var quantity = reader.GetValueOrNull<int>("Qty");
            var comment = reader.GetValue("Comments");

            var inventory = TargetContext.Inventories.SingleOrDefault(f => f.XInventoryId == sourceInventory);

            DateTime? whenStarted = null;
            DateTime? whenEnded = null;
            try { whenStarted = reader.GetDateTime("Start date"); } catch { Trace.WriteLine(string.Format("Bad date: \"{0}\", skipping import", key)); }
            try { whenEnded = reader.GetDateTime("End date"); } catch { Trace.WriteLine(string.Format("Bad date: \"{0}\"", key)); }                

            if (inventory != null && sourceJob != null)
            {
                var job = TargetContext.Jobs.SingleOrDefault(f => f.XJobId == sourceJob);

                if (job == null) return;
                if (whenStarted == null) return;

                // don't add inventory for these jobs
                // these are duplicated in ImportPlantHire
                switch (job.XJobId)
                {
                    case "940":
                    case "941":
                    case "950":
                    case "960":
                    case "961":
                    case "962":
                    case "963":
                    case "970":
                    case "980":
                    case "981":
                    case "982":
                    case "984":
                        var hired = TargetContext.InventoryHires.Where(f => f.Inventory.Id == inventory.Id).OrderByDescending(f => f.Id).Take(1).SingleOrDefault();

                        if (hired != null)
                        {
                            hired.WhenEnded = whenEnded.HasValue ? whenEnded : whenStarted;
                            hired.ReturnDocket = docket;
                            hired.ReturnQuantity = quantity.HasValue ? Math.Abs(quantity.Value) : 0;
                            hired.Comment += "/" + comment;

                            TargetContext.Entry(hired).State = EntityState.Modified;
                        }
                        else
                        {
                            Trace.WriteLine(string.Format("Orphaned: \"{0}\"", key));
                        }

                        return;
                }

                if (rate.HasValue && rate == 0) rate = inventory.Rate;

                var hire = new InventoryHire
                {
                    InventoryId = inventory.Id,
                    JobId = job.Id,
                    Docket = docket,
                    ReturnDocket = returnDocket,
                    WhenStarted = whenStarted.Value,
                    WhenEnded = whenEnded,
                    Rate = rate,
                    Quantity = quantity,
                    Comment = comment
                };

                TargetContext.InventoryHires.Add(hire);
            }
            else
            {
                Trace.WriteLine(string.Format("Orphaned: \"{0}\"", key));
            }
        }
    }
}
