﻿using RussellGroup.Pims.DataAccess.Models;
using System;
using System.Collections.Generic;
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

            var inventory = TargetContext.Inventories.SingleOrDefault(f => f.XInventoryId == sourceInventory);

            DateTime? whenStarted = null;
            DateTime? whenEnded = null;
            try { whenStarted = reader.GetDateTime("Start date"); } catch { Trace.WriteLine(string.Format("Bad date: \"{0}\"", key)); }
            try { whenEnded = reader.GetDateTime("End date"); } catch { Trace.WriteLine(string.Format("Bad date: \"{0}\"", key)); }                

            decimal? rate = reader.GetValueOrNull<decimal>("Rate");
            if (rate.HasValue && rate == 0) rate = null;

            if (inventory != null)
            {
                var hire = new InventoryHire
                {
                    InventoryId = inventory.InventoryId,
                    JobId = TargetContext.Jobs.Single(f => f.XJobId == sourceJob).JobId,
                    Docket = reader.GetValue("Doc no"),
                    ReturnDocket = reader.GetValue("Return doc no"),
                    WhenStarted = whenStarted,
                    WhenEnded = whenEnded,
                    Rate = rate,
                    Quantity = reader.GetValueOrNull<int>("Qty"),
                    Comment = reader.GetValue("Comments"),
                    IsImported = true
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
