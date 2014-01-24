using RussellGroup.Pims.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RussellGroup.Pims.DataMigration
{
    class ImportCategory : AbstractImport
    {
        protected override void Map(OleDbDataReader reader)
        {
            var category = new Category
            {
                Name = reader.GetValue("Category"),
                Type = reader.GetValue("Plant Type"),
                IsImported = true
            };

            TargetContext.Categories.Add(category);
        }
    }
}
