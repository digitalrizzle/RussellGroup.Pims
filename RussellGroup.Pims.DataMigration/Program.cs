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
        const int IMPORT_CAP = 00; // set to 0 to import all rows
        const string OLEDB_CONNECTION_STRING = @"Provider=Microsoft.Jet.OLEDB.4.0;Mode=Read;Data Source=C:\Users\Brett\Documents\Visual Studio 2013\Projects\RussellGroup.Pims\DCL PLANT (2002).mdb";
        const string TRACE_LOG_PATH = @"C:\Users\Brett\Documents\Visual Studio 2013\Projects\RussellGroup.Pims\PIMS data migration.log";

        static void Main(string[] args)
        {
            if (File.Exists(TRACE_LOG_PATH)) File.Delete(TRACE_LOG_PATH);

            using (var file = new TextWriterTraceListener(TRACE_LOG_PATH))
            {
                Trace.Listeners.Add(file);

                using (var connection = new OleDbConnection(OLEDB_CONNECTION_STRING))
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
                    //inventoryHire.Delete();
                    //job.Delete();

                    //categories.Delete().Import();
                    //plant.Delete().Import();
                    //inventory.Delete().Import();
                    //job.Import();

                    plantHire.Delete().Import(0, IMPORT_CAP);
                    //inventoryHire.Import(0, IMPORT_CAP);

                    job.SetAuditing(true);
                }

                Trace.Flush();
                Trace.Listeners.Remove(file);
            }

            //Console.WriteLine("Ready.");
            //Console.ReadKey();
        }
    }
}
