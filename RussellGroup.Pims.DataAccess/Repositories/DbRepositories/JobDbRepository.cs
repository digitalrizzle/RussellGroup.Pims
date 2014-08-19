using RussellGroup.Pims.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RussellGroup.Pims.DataAccess.Repositories
{
    public class JobDbRepository : DbRepository<Job>, IJobRepository
    {
        public JobDbRepository(PimsDbContext context) : base(context) { }

        public override async Task<int> RemoveAsync(Job job)
        {
            // remove hire is done using the repositories so that
            // they are correctly removed, i.e. plant items have
            // their status updated to unavailable

            var plantHires = job.PlantHires.ToList();
            var inventoryHires = job.InventoryHires.ToList();

            if (plantHires.Any())
            {
                var plantHireRepository = new HireDbRepository<PlantHire>(Db);

                foreach (var hire in job.PlantHires.ToList())
                {
                    await plantHireRepository.RemoveAsync(hire);
                }
            }

            if (inventoryHires.Any())
            {
                var inventoryHireRepository = new HireDbRepository<InventoryHire>(Db);

                foreach (var hire in job.InventoryHires.ToList())
                {
                    Db.InventoryHires.Remove(hire);
                }
            }

            return await base.RemoveAsync(job);
        }
    }
}
