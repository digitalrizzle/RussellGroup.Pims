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
        IQueryable<PlantHire> PlantHires { get; }
        IQueryable<Inventory> Inventories { get; }
        IQueryable<InventoryHire> InventoryHires { get; }
        IQueryable<Receipt> Receipts { get; }

        IQueryable<Status> Statuses { get; }
        IQueryable<Condition> Conditions { get; }
        IQueryable<TransactionType> TransactionTypes { get; }

        Task<long> GetLastIssuedDocketAsync();
        Task StoreAsync(Receipt receipt);

        IEnumerable<InventoryHireCheckin> GetCheckinInventoryHires(Job job);

        Task<long> Checkout(Job job, DateTime whenStarted, IEnumerable<int> plantIds, IEnumerable<KeyValuePair<int, int?>> inventoryIdsAndQuantities);
        Task Checkout(Job job, string docket, DateTime whenStarted, IEnumerable<int> plantIds, IEnumerable<KeyValuePair<int, int?>> inventoryIdsAndQuantities);

        Task Checkin(Job job, string docket, DateTime whenEnded, IEnumerable<int> plantHireIds, IEnumerable<KeyValuePair<int, int?>> inventoryIdsAndQuantities);
        Task<long> Checkin(Job job, DateTime whenEnded, int statusId, IEnumerable<int> plantHireIds, IEnumerable<KeyValuePair<int, int?>> inventoryIdsAndQuantities);
        Task Checkin(Job job, string docket, DateTime whenEnded, int statusId, IEnumerable<int> plantHireIds, IEnumerable<KeyValuePair<int, int?>> inventoryIdsAndQuantities);

        Task<int> UpdateStatusAsync(int plantId, int statusId);
    }
}
