using RussellGroup.Pims.DataAccess.Models;
using RussellGroup.Pims.DataAccess.ViewModels;
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

        public IQueryable<Inventory> Inventories
        {
            get
            {
                return Db.Inventories;
            }
        }

        public IQueryable<PlantHire> GetCheckedOutPlantHiresInJob(int? jobId)
        {
            return Db.PlantHires.Where(f => f.JobId == jobId && !f.WhenEnded.HasValue);
        }

        public IEnumerable<InventoryHire> GetCheckedOutInventoryHiresInJob(int? jobId)
        {
            var hires = Db.InventoryHires.Where(f => f.JobId == jobId).ToList();

            // set the return quantity however many are remaining
            for (var index = 0; index < hires.Count; index++)
            {
                var hire = hires[index];

                if (hire.Checkins != null)
                {
                    hire.Quantity -= hire.Checkins.Sum(f => f.Quantity);

                    if (hire.Quantity == 0)
                    {
                        hires.Remove(hire);
                    }
                }
            }

            return hires;
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

                var hire = new InventoryHire
                {
                    Inventory = inventory,
                    Job = job,
                    Docket = docket,
                    WhenStarted = whenStarted,
                    Rate = inventory.Rate,
                    Quantity = quantity
                };

                Db.InventoryHires.Add(hire);

                // update the inventory quantity
                hire.Inventory.Quantity -= quantity.GetValueOrDefault();
            }

            await Db.SaveChangesAsync();
        }

        public async Task Checkin(string returnDocket, DateTime whenEnded, IEnumerable<int> plantHireIds, IEnumerable<KeyValuePair<int, int?>> inventoryHireIdsAndQuantities)
        {
            // save plant
            foreach (var id in plantHireIds)
            {
                var hire = Db.PlantHires.SingleOrDefault(f => f.Id == id && !f.WhenEnded.HasValue);

                if (hire != null)
                {
                    hire.ReturnDocket = returnDocket;
                    hire.WhenEnded = whenEnded;
                    Db.Entry(hire).State = EntityState.Modified;
                }

                // only set the status if it is truly available
                if (hire.Plant.IsCheckedIn)
                {
                    hire.Plant.StatusId = Status.Available;
                    Db.Entry(hire.Plant).State = EntityState.Modified;
                }
            }

            // save inventory
            foreach (var pair in inventoryHireIdsAndQuantities)
            {
                var id = pair.Key;
                var returnQuantity = pair.Value;

                var hire = Db.InventoryHires.SingleOrDefault(f => f.Id == id);

                if (hire != null)
                {
                    var @return = new InventoryHireCheckin
                    {
                        InventoryHire = hire,
                        Docket = returnDocket,
                        WhenEnded = whenEnded,
                        Quantity = returnQuantity.GetValueOrDefault()
                    };

                    Db.Entry(@return).State = EntityState.Added;
                }

                // update the inventory quantity
                hire.Inventory.Quantity += returnQuantity.GetValueOrDefault();
            }

            await Db.SaveChangesAsync();
        }
    }
}
