using RussellGroup.Pims.DataAccess.Models;
using RussellGroup.Pims.DataAccess.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RussellGroup.Pims.DataAccess.Respositories
{
    public class TransactionDbRepository : ITransactionRepository
    {
        protected PimsContext db = new PimsContext();

        public async Task<Job> GetJob(int? id)
        {
            return await db.Jobs.SingleOrDefaultAsync(f => f.JobId == id);
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

        public IQueryable<PlantHire> GetActivePlantHiresInJob(int? jobId)
        {
            var job = db.Jobs.Find(jobId);
            var hire = job.PlantHires.Where(f => !f.WhenEnded.HasValue).AsQueryable();

            return hire;
        }

        public IQueryable<InventoryHire> GetActiveInventoryHiresInJob(int? jobId)
        {
            var job = db.Jobs.Find(jobId);
            var hire = job.InventoryHires.Where(f => !f.WhenEnded.HasValue).AsQueryable();

            return hire;
        }

        public async Task Checkout(Job job, string docket, IEnumerable<int> plantIds, IEnumerable<KeyValuePair<int, int?>> inventoryIdsAndQuantities)
        {
            // save plant
            foreach (var id in plantIds)
            {
                var plant = await db.Plants.SingleOrDefaultAsync(f => f.PlantId == id);

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

                plant.StatusId = 3; // unavailable
                db.Entry(plant).State = EntityState.Modified;
            }

            // save inventory
            foreach (var pair in inventoryIdsAndQuantities)
            {
                var inventory = await db.Inventories.SingleOrDefaultAsync(f => f.InventoryId == pair.Key);

                var hire = new InventoryHire
                {
                    Inventory = inventory,
                    Job = job,
                    Docket = docket,
                    WhenStarted = DateTime.Now,
                    WhenEnded = null,
                    Rate = inventory.Rate,
                    Quantity = pair.Value
                };

                db.InventoryHires.Add(hire);
            }

            await db.SaveChangesAsync();
        }

        public async Task Checkin(string returnDocket, IEnumerable<int> plantHireIds, IEnumerable<int> inventoryHireIds)
        {
            // save plant
            foreach (var id in plantHireIds)
            {
                var hire = db.PlantHires.SingleOrDefault(f => f.PlantHireId == id && !f.WhenEnded.HasValue);

                if (hire != null)
                {
                    hire.ReturnDocket = returnDocket;
                    hire.WhenEnded = DateTime.Now;
                    db.Entry(hire).State = EntityState.Modified;
                }

                hire.Plant.StatusId = 2; // available
                db.Entry(hire.Plant).State = EntityState.Modified;
            }

            // save inventory
            foreach (var id in inventoryHireIds)
            {
                var hire = db.InventoryHires.SingleOrDefault(f => f.InventoryHireId == id && !f.WhenEnded.HasValue);

                if (hire != null)
                {
                    hire.ReturnDocket = returnDocket;
                    hire.WhenEnded = DateTime.Now;
                    db.Entry(hire).State = EntityState.Modified;
                }
            }

            await db.SaveChangesAsync();
        }

        public void Dispose()
        {
            db.Dispose();
        }
    }
}
