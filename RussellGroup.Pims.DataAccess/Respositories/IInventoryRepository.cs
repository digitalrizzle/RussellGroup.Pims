using RussellGroup.Pims.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RussellGroup.Pims.DataAccess.Respositories
{
    public interface IInventoryRepository : IRepository<Inventory>
    {
        IQueryable<Category> Categories { get; }
    }
}
