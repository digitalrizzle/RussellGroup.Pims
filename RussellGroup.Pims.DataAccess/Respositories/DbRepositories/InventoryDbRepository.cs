using RussellGroup.Pims.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RussellGroup.Pims.DataAccess.Respositories
{
    public class InventoryDbRepository : DbRepository<Inventory>, IInventoryRepository
    {
        public IQueryable<Category> Categories
        {
            get { return Db.Categories; }
        }
    }
}
