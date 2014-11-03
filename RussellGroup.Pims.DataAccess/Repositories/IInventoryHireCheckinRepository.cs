using RussellGroup.Pims.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RussellGroup.Pims.DataAccess.Repositories
{
    public interface IInventoryHireCheckinRepository : IRepository<InventoryHireCheckin>
    {
        Task<InventoryHire> GetInventoryHire(int? id);
    }
}
