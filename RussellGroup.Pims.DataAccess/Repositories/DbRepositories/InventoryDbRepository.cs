using RussellGroup.Pims.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RussellGroup.Pims.DataAccess.Repositories
{
    public class InventoryDbRepository : DbRepository<Inventory>, IInventoryRepository
    {
        public InventoryDbRepository(PimsDbContext context) : base(context) { }

        public IQueryable<Category> Categories
        {
            get { return Db.Categories; }
        }
    }
}
