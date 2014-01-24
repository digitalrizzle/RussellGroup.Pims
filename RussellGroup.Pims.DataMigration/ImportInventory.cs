using RussellGroup.Pims.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RussellGroup.Pims.DataMigration
{
    class ImportInventory : AbstractImport
    {
        protected override void Map(OleDbDataReader reader)
        {
            var categoryName = reader.GetValue("Category");

            var inventory = new Inventory
            {
                XInventoryId = reader.GetValue("Inv no"),
                CategoryId = TargetContext.Categories.Single(f => f.Name == categoryName).CategoryId,
                Description = reader.GetValue("Description"),
                Rate = reader.GetValueOrNull<decimal>("Rate"),
                Cost = reader.GetValueOrNull<decimal>("Cost"),
                Quantity = Convert.ToInt32(reader.GetValue("Qty")),
                IsImported = true
            };

            TargetContext.Inventories.Add(inventory);
        }
    }
}
