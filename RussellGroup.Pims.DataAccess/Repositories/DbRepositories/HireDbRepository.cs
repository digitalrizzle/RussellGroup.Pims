using RussellGroup.Pims.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RussellGroup.Pims.DataAccess.Repositories
{
    public class HireDbRepository<T> : DbRepository<T>, IHireRepository<T> where T : class
    {
        public HireDbRepository(PimsDbContext context)
            : base(context)
        {
            // ensure that only PlantHire or InventoryHire is used as the generic type argument
            var arg = this.GetType().GetGenericArguments()[0];

            if (arg == typeof(PlantHire) || arg == typeof(InventoryHire))
            {
                return;
            }

            throw new NotSupportedException();
        }

        public override async Task<int> AddAsync(T item)
        {
            // adjust the inventory quantity
            if (item is InventoryHire)
            {
                var hire = item as InventoryHire;

                if (hire.ReturnQuantity.HasValue)
                {
                    hire.Inventory.Quantity -= hire.ReturnQuantity.Value - hire.Quantity.Value;
                }
                else
                {
                    hire.Inventory.Quantity -= hire.Quantity.Value;
                }
            }

            var result = await base.AddAsync(item);

            // update the plant status to checked out
            if (item is PlantHire)
            {
                var hire = item as PlantHire;

                if (hire.Plant == null)
                {
                    await Db.Entry(hire).Reference(f => f.Plant).LoadAsync();
                }

                hire.Plant.StatusId = Status.CheckedOut;

                await Db.SaveChangesAsync();
            }

            return result;
        }

        public override async Task<int> UpdateAsync(T item)
        {
            // update the plant status to checked out
            if (item is PlantHire)
            {
                var hire = item as PlantHire;

                if (hire.Plant == null)
                {
                    await Db.Entry(hire).Reference(f => f.Plant).LoadAsync();
                }

                if (hire.IsCheckedOut)
                {
                    hire.Plant.StatusId = Status.CheckedOut;
                }
                else if (hire.Plant.IsCheckedIn && (hire.Plant.StatusId == Status.Unknown || hire.Plant.StatusId == Status.CheckedOut))
                {
                    hire.Plant.StatusId = Status.Available;
                }
            }

            // adjust the inventory quantity
            if (item is InventoryHire)
            {
                var hire = item as InventoryHire;
                var originalHire = await Db.InventoryHires.AsNoTracking().SingleOrDefaultAsync(f => f.Id == hire.Id);

                hire.Inventory.Quantity += originalHire.Quantity.GetValueOrDefault() - hire.Quantity.GetValueOrDefault();
                hire.Inventory.Quantity += hire.ReturnQuantity.GetValueOrDefault() - originalHire.ReturnQuantity.GetValueOrDefault();
            }

            return await base.UpdateAsync(item);
        }

        public override async Task<int> RemoveAsync(T item)
        {
            // set plant back to available whenever possible
            if (item is PlantHire)
            {
                var hire = item as PlantHire;

                // only set the status if it is truly available
                if (hire.Plant.IsCheckedOut && (hire.Plant.StatusId == Status.Unknown || hire.Plant.StatusId == Status.CheckedOut))
                {
                    hire.Plant.StatusId = Status.Available;
                }
            }

            // otherwise adjust the inventory quantity
            if (item is InventoryHire)
            {
                var hire = item as InventoryHire;

                if (hire.ReturnQuantity.HasValue)
                {
                    hire.Inventory.Quantity += hire.ReturnQuantity.Value - hire.Quantity.Value;
                }
                else
                {
                    hire.Inventory.Quantity += hire.Quantity.Value;
                }
            }

            return await base.RemoveAsync(item);
        }

        public Task<Job> GetJob(int? id)
        {
            return Db.Jobs.SingleOrDefaultAsync(f => f.Id == id);
        }

        public IQueryable<Job> Jobs
        {
            get { return Db.Jobs; }
        }

        public IQueryable<Plant> Plants
        {
            get { return Db.Plants; }
        }

        public IQueryable<Inventory> Inventories
        {
            get { return Db.Inventories; }
        }
    }
}
