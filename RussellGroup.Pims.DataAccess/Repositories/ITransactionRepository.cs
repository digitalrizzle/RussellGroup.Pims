using RussellGroup.Pims.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RussellGroup.Pims.DataAccess.Repositories
{
    public interface ITransactionRepository
    {
        Task<Job> GetJob(int? id);

        IQueryable<Job> Jobs { get; }
        IQueryable<Plant> Plants { get; }
        IQueryable<Inventory> Inventories { get; }

        IEnumerable<InventoryHireCheckin> GetCheckinInventoryHires(Job job);

        Task Checkout(Job job, string docket, DateTime whenStarted, IEnumerable<int> plantIds, IEnumerable<KeyValuePair<int, int?>> inventoryIdsAndQuantities);
        Task Checkin(Job job, string docket, DateTime whenEnded, IEnumerable<int> plantHireIds, IEnumerable<KeyValuePair<int, int?>> inventoryIdsAndQuantities);
    }
}
