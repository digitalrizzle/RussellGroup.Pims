using RussellGroup.Pims.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RussellGroup.Pims.DataMigration
{
    class Program
    {
        static void Main(string[] args)
        {
            var importCap = global::RussellGroup.Pims.DataMigration.Properties.Settings.Default.ImportCap;
            var traceLogPath = global::RussellGroup.Pims.DataMigration.Properties.Settings.Default.TraceLog;

            if (File.Exists(traceLogPath)) File.Delete(traceLogPath);

            using (var file = new TextWriterTraceListener(traceLogPath))
            {
                Trace.Listeners.Add(file);

                using (var connection = new OleDbConnection(global::RussellGroup.Pims.DataMigration.Properties.Settings.Default.OleDbConnectionString))
                {
                    connection.Open();

                    var categories = new ImportCategory() { SourceConnection = connection, SourceTableName = "Plant categories", TargetTableName = "Categories" };
                    var plant = new ImportPlant() { SourceConnection = connection, SourceTableName = "Plant", TargetTableName = "Plants" };
                    var inventory = new ImportInventory() { SourceConnection = connection, SourceTableName = "inventory", TargetTableName = "Inventories" };
                    var job = new ImportJob() { SourceConnection = connection, SourceTableName = "Jobs", TargetTableName = "Jobs" };

                    var plantHire = new ImportPlantHire() { SourceConnection = connection, SourceTableName = "Plant Hire", TargetTableName = "PlantHires", SourcePrimaryKeyColumnName = "ID" };
                    var inventoryHire = new ImportInventoryHire() { SourceConnection = connection, SourceTableName = "Inventory Hire", TargetTableName = "InventoryHires", SourcePrimaryKeyColumnName = "ID" };

                    job.SetAuditing(false);

                    //plantHire.Delete();
                    inventoryHire.Delete();
                    //job.Delete();
                    //plant.Delete();
                    //inventory.Delete();

                    //categories.Delete().Import();
                    //plant.Import();
                    //inventory.Import();

                    // importing jobs will add any missing ones (this won't work for any other import type)
                    job.Import();

                    //plantHire.Import(0, importCap);
                    inventoryHire.Import(0, importCap);

                    //job.Delete(new[] { "940", "941", "950", "960", "961", "962", "963", "970", "980", "981", "982", "984" });
                    
                    // DO NOT RUN THIS!
                    // It is supposed to remove any jobs that have no
                    // current plant or inventory hire, but it doesn't
                    // work properly
                    //job.Clean();

                    job.SetAuditing(true);
                }

                Trace.Flush();
                Trace.Listeners.Remove(file);
            }
        }
    }
}
