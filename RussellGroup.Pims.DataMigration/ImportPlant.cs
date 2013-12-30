using RussellGroup.Pims.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RussellGroup.Pims.DataMigration
{
    class ImportPlant : AbstractImport
    {
        protected override void Map(OleDbDataReader reader)
        {
            var ordinal = reader.GetOrdinal("Electrical Item?");
            var categoryName = reader.GetValue("Category");

            var plant = new Plant
            {
                XPlantId = reader.GetValue("Plant no"),
                XPlantNewId = reader.GetValue("PlantNew no"),
                CategoryId = TargetContext.Categories.Single(f => f.Name == categoryName).CategoryId,
                Description = reader.GetValue("Description"),
                Rate = reader.GetValueOrNull<decimal>("Rate").Value,
                Cost = reader.GetValueOrNull<decimal>("Cost") ?? 0,
                Serial = reader.GetValue("Serial number"),
                FixedAssetCode = reader.GetValue("Fixed Asset Code"),
                IsElectrical = reader.IsDBNull(ordinal) ? false : reader[ordinal].ToString().Trim().ToUpper().StartsWith("Y")
            };

            TargetContext.Plants.Add(plant);
        }
    }
}
