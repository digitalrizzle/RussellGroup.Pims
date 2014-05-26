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

                    plantHire.Delete();
                    inventoryHire.Delete();
                    job.Delete();
                    plant.Delete();
                    inventory.Delete();

                    categories.Delete().Import();
                    plant.Import();
                    inventory.Import();
                    job.Import();

                    plantHire.Import(0, importCap);
                    inventoryHire.Import(0, importCap);

                    job.SetAuditing(true);
                }

                Trace.Flush();
                Trace.Listeners.Remove(file);
            }
        }
    }
}
