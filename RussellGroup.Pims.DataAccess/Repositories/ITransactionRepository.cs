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

        IQueryable<PlantHire> GetCheckedOutPlantHiresInJob(int? jobId);
        IEnumerable<InventoryHire> GetCheckedOutInventoryHiresInJob(int? jobId);

        Task Checkout(Job job, string docket, DateTime whenStarted, IEnumerable<int> plantIds, IEnumerable<KeyValuePair<int, int?>> inventoryIdsAndQuantities);
        Task Checkin(string returnDocket, DateTime whenEnded, IEnumerable<int> plantHireIds, IEnumerable<KeyValuePair<int, int?>> inventoryHireIdsAndQuantities);
    }
}
