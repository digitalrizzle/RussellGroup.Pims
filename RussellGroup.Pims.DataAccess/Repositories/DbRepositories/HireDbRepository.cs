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
        public HireDbRepository()
        {
            // ensure that only PlantHire or InventoryHire is used as the generic type argument
            var arg = this.GetType().GetGenericArguments()[0];

            if (arg == typeof(PlantHire) || arg == typeof(InventoryHire))
            {
                return;
            }

            throw new NotSupportedException();
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
