using RussellGroup.Pims.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RussellGroup.Pims.DataAccess.Repositories
{
    public class TransactionDbRepository : ITransactionRepository
    {
        public const string DocketPrefix = "DCL";

        protected PimsDbContext Db { get; private set; }

        public TransactionDbRepository(PimsDbContext context)
        {
            Db = context;
        }

        public async Task<Job> GetJob(int? id)
        {
            return await Db.Jobs.SingleOrDefaultAsync(f => f.Id == id);
        }

        public IQueryable<Job> Jobs
        {
            get { return Db.Jobs; }
        }

        public IQueryable<Plant> Plants
        {
            get
            {
                return Db.Plants;
            }
        }

        public IQueryable<PlantHire> PlantHires
        {
            get
            {
                return Db.PlantHires.Include("Job");
            }
        }

        public IQueryable<Inventory> Inventories
        {
            get
            {
                return Db.Inventories;
            }
        }

        public IQueryable<InventoryHire> InventoryHires
        {
            get
            {
                return Db.InventoryHires.Include("Job");
            }
        }

        public IQueryable<Status> Statuses
        {
            get { return Db.Statuses; }
        }

        public IQueryable<Condition> Conditions
        {
            get { return Db.Conditions; }
        }

        public IQueryable<TransactionType> TransactionTypes
        {
            get { return Db.TransactionTypes; }
        }

        public IQueryable<Receipt> Receipts
        {
            get { return Db.Receipts; }
        }

        public async Task<long> GetLastIssuedDocketAsync()
        {
            var setting = await GetLastIssuedDocketSettingAsync();
            var value = StripPrefix(setting);
            var docket = long.Parse(value);

            return docket;
        }

        private string StripPrefix(Setting setting)
        {
            return setting.Value.Replace(DocketPrefix, null);
        }

        private async Task<Setting> GetLastIssuedDocketSettingAsync()
        {
            long docket;
            var setting = await Db.Settings.SingleOrDefaultAsync(f => f.Key.Equals("LastIssuedDocket"));

            if (setting != null)
            {
                var value = StripPrefix(setting);

                if (long.TryParse(value, out docket))
                {
                    return setting;
                }
            }

            throw new InvalidOperationException("The last issued docket could not be obtained.");
        }

        public static string FormatDocket(long docketNumber)
        {
            return $"{TransactionDbRepository.DocketPrefix}{docketNumber:00000}";
        }

        public async Task<Receipt> StoreAsync(Receipt receipt)
        {
            Db.Receipts.Add(receipt);

            await Db.SaveChangesAsync();

            return receipt;
        }

        public IEnumerable<InventoryHireCheckin> GetCheckinInventoryHires(Job job)
        {
            var groups = job.InventoryHires.GroupBy(f => f.Inventory);

            var hires = groups.Select(group => new InventoryHireCheckin
            {
                Job = job,
                JobId = job.Id,
                Inventory = group.Key,
                InventoryId = group.Key.Id,
                Quantity = GetQuantity(group),
                CheckedOutQuantity = GetQuantity(group).GetValueOrDefault()
            });

            return hires;
        }

        private int? GetQuantity(IGrouping<Inventory, InventoryHire> group)
        {
            return
                  group.Where(f => f is InventoryHireCheckout).Sum(f => f.Quantity)
                - group.Where(f => f is InventoryHireCheckin).Sum(f => f.Quantity);
        }

        // the docket number will be automatically assigned
        public async Task<long> Checkout(Job job, DateTime whenStarted, IEnumerable<int> plantIds, IEnumerable<KeyValuePair<int, int?>> inventoryIdsAndQuantities)
        {
            // get and update the docket number (SaveChanges() in Checkout ought to commit the change)
            var setting = await GetLastIssuedDocketSettingAsync();
            var value = StripPrefix(setting);
            var docket = long.Parse(value);

            docket++;
            setting.Value = docket.ToString();
            Db.Entry(setting).State = EntityState.Modified;

            // checkout as usual
            await Checkout(job, FormatDocket(docket), whenStarted, plantIds, inventoryIdsAndQuantities);

            return docket;
        }

        public async Task Checkout(Job job, string docket, DateTime whenStarted, IEnumerable<int> plantIds, IEnumerable<KeyValuePair<int, int?>> inventoryIdsAndQuantities)
        {
            // save plant
            foreach (var id in plantIds)
            {
                var plant = await Db.Plants.SingleOrDefaultAsync(f => f.Id == id);

                var hire = new PlantHire
                {
                    Plant = plant,
                    Job = job,
                    Docket = docket,
                    WhenStarted = whenStarted,
                    WhenEnded = null,
                    Rate = plant.Rate
                };

                Db.PlantHires.Add(hire);

                plant.StatusId = Status.CheckedOut;
                Db.Entry(plant).State = EntityState.Modified;
            }

            // save inventory
            foreach (var pair in inventoryIdsAndQuantities)
            {
                var quantity = pair.Value;

                var inventory = await Db.Inventories.SingleOrDefaultAsync(f => f.Id == pair.Key);

                var hire = new InventoryHireCheckout
                {
                    Inventory = inventory,
                    Job = job,
                    Docket = docket,
                    WhenStarted = whenStarted,
                    Quantity = quantity
                };

                Db.InventoryHires.Add(hire);

                // update the inventory quantity
                hire.Inventory.Quantity -= quantity.GetValueOrDefault();
            }

            await Db.SaveChangesAsync();
        }

        public async Task Checkin(Job job, string docket, DateTime whenEnded, IEnumerable<int> plantHireIds, IEnumerable<KeyValuePair<int, int?>> inventoryIdsAndQuantities)
        {
            await Checkin(job, docket, whenEnded, Status.Available, plantHireIds, inventoryIdsAndQuantities);
        }

        // the docket number will be automatically assigned
        public async Task<long> Checkin(Job job, DateTime whenEnded, int statusId, IEnumerable<int> plantIds, IEnumerable<KeyValuePair<int, int?>> inventoryIdsAndQuantities)
        {
            // get and update the docket number (SaveChanges() in Checkin ought to commit the change)
            var setting = await GetLastIssuedDocketSettingAsync();
            var docket = long.Parse(setting.Value);

            docket++;
            setting.Value = docket.ToString();
            Db.Entry(setting).State = EntityState.Modified;

            // checkout as usual
            await Checkin(job, FormatDocket(docket), whenEnded, statusId, plantIds, inventoryIdsAndQuantities);

            return docket;
        }

        public async Task Checkin(Job job, string docket, DateTime whenEnded, int statusId, IEnumerable<int> plantHireIds, IEnumerable<KeyValuePair<int, int?>> inventoryIdsAndQuantities)
        {
            // save plant
            foreach (var id in plantHireIds)
            {
                var hire = Db.PlantHires.SingleOrDefault(f => f.Id == id && !f.WhenEnded.HasValue);

                if (hire != null)
                {
                    hire.ReturnDocket = docket;
                    hire.WhenEnded = whenEnded;
                    Db.Entry(hire).State = EntityState.Modified;

                    // TODO: this could be the intermittent bug they're talking about
                    // only set the status if it is truly available
                    if (hire.Plant.IsCheckedIn)
                    {
                        hire.Plant.StatusId = statusId;
                        Db.Entry(hire.Plant).State = EntityState.Modified;
                    }
                }
            }

            // save inventory
            foreach (var pair in inventoryIdsAndQuantities)
            {
                var id = pair.Key;
                var quantity = pair.Value;

                var hire = new InventoryHireCheckin
                {
                    JobId = job.Id,
                    InventoryId = id,
                    Docket = docket,
                    WhenEnded = whenEnded,
                    Quantity = quantity
                };

                Db.Entry(hire).State = EntityState.Added;

                // update the inventory quantity
                var inventory = Db.Inventories.Single(f => f.Id == id);
                inventory.Quantity += quantity.GetValueOrDefault();
            }

            await Db.SaveChangesAsync();
        }

        public async Task<int> UpdateStatusAsync(int plantId, int statusId)
        {
            var plant = await Db.Plants.FindAsync(plantId);
            plant.StatusId = statusId;

            Db.Entry(plant).State = EntityState.Modified;

            return await Db.SaveChangesAsync();
        }
    }
}
