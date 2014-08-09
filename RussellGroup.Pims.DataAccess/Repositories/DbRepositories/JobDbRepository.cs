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

        public override async Task<int> RemoveAsync(params object[] keyValues)
        {
            var job = await FindAsync(keyValues);

            foreach (var hire in job.PlantHires.ToArray())
            {
                Db.PlantHires.Remove(hire);
            }

            foreach (var hire in job.InventoryHires.ToArray())
            {
                Db.InventoryHires.Remove(hire);
            }

            return await base.RemoveAsync(keyValues);
        }
    }
}
