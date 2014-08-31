using RussellGroup.Pims.DataAccess.Models;
using RussellGroup.Pims.DataAccess.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RussellGroup.Pims.DataAccess.Repositories
{
    public class TransactionDbRepository : ITransactionRepository
    {
        private bool _disposed;

        protected PimsDbContext db = new PimsDbContext();

        public async Task<Job> GetJob(int? id)
        {
            return await db.Jobs.SingleOrDefaultAsync(f => f.Id == id);
        }

        public IQueryable<Job> Jobs
        {
            get { return db.Jobs; }
        }

        public IQueryable<Plant> Plants
        {
            get { return db.Plants; }
        }

        public IQueryable<Inventory> Inventories
        {
            get { return db.Inventories; }
        }

        public IQueryable<PlantHire> GetCheckedOutPlantHiresInJob(int? jobId)
        {
            return db.PlantHires.Where(f => f.JobId == jobId && f.IsCheckedOut);
        }

        public IQueryable<InventoryHire> GetCheckedOutInventoryHiresInJob(int? jobId)
        {
            return db.InventoryHires.Where(f => f.JobId == jobId && f.IsCheckedOut);
        }

        public async Task Checkout(Job job, string docket, IEnumerable<int> plantIds, IEnumerable<KeyValuePair<int, int?>> inventoryIdsAndQuantities)
        {
            // save plant
            foreach (var id in plantIds)
            {
                var plant = await db.Plants.SingleOrDefaultAsync(f => f.Id == id);

                var hire = new PlantHire
                {
                    Plant = plant,
                    Job = job,
                    Docket = docket,
                    WhenStarted = DateTime.Now,
                    WhenEnded = null,
                    Rate = plant.Rate
                };

                db.PlantHires.Add(hire);

                plant.StatusId = Status.CheckedOut;
                db.Entry(plant).State = EntityState.Modified;
            }

            // save inventory
            foreach (var pair in inventoryIdsAndQuantities)
            {
                var quantity = pair.Value;

                var inventory = await db.Inventories.SingleOrDefaultAsync(f => f.Id == pair.Key);

                var hire = new InventoryHire
                {
                    Inventory = inventory,
                    Job = job,
                    Docket = docket,
                    WhenStarted = DateTime.Now,
                    WhenEnded = null,
                    Rate = inventory.Rate,
                    Quantity = quantity
                };

                db.InventoryHires.Add(hire);

                // update the inventory quantity
                hire.Inventory.Quantity -= quantity.GetValueOrDefault();
            }

            await db.SaveChangesAsync();
        }

        public async Task Checkin(string returnDocket, IEnumerable<int> plantHireIds, IEnumerable<KeyValuePair<int, int?>> inventoryHireIdsAndQuantities)
        {
            // save plant
            foreach (var id in plantHireIds)
            {
                var hire = db.PlantHires.SingleOrDefault(f => f.Id == id && f.IsCheckedOut);

                if (hire != null)
                {
                    hire.ReturnDocket = returnDocket;
                    hire.WhenEnded = DateTime.Now;
                    db.Entry(hire).State = EntityState.Modified;
                }

                // only set the status if it is truly available
                if (hire.Plant.IsCheckedIn)
                {
                    hire.Plant.StatusId = Status.Available;
                    db.Entry(hire.Plant).State = EntityState.Modified;
                }
            }

            // save inventory
            foreach (var pair in inventoryHireIdsAndQuantities)
            {
                var id = pair.Key;
                var returnQuantity = pair.Value;

                var hire = db.InventoryHires.SingleOrDefault(f => f.Id == id && f.IsCheckedOut);

                if (hire != null)
                {
                    hire.ReturnDocket = returnDocket;
                    hire.WhenEnded = DateTime.Now;
                    hire.ReturnQuantity = returnQuantity;
                    db.Entry(hire).State = EntityState.Modified;
                }

                // update the inventory quantity
                hire.Inventory.Quantity += returnQuantity.GetValueOrDefault();
            }

            await db.SaveChangesAsync();
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize((object)this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                db.Dispose();
            }

            _disposed = true;
        }
    }
}
