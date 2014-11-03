using RussellGroup.Pims.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RussellGroup.Pims.DataAccess.Repositories
{
    public class InventoryHireCheckinDbRepository : DbRepository<InventoryHireCheckin>, IInventoryHireCheckinRepository
    {
        public InventoryHireCheckinDbRepository(PimsDbContext context) : base(context) { }

        public Task<InventoryHire> GetInventoryHire(int? id)
        {
            return Db.InventoryHires.SingleOrDefaultAsync(f => f.Id == id);
        }
    }
}
