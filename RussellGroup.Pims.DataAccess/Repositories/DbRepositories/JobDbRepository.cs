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

        public override Task<int> AddAsync(Job job)
        {
            Validate(job);

            return base.AddAsync(job);
        }

        public override Task<int> UpdateAsync(Job job)
        {
            Validate(job);

            return base.UpdateAsync(job);
        }

        public override async Task<int> RemoveAsync(Job job)
        {
            // remove hire is done using the repositories so that
            // they are correctly removed, i.e. plant items have
            // their status updated to unavailable

            var plantHires = job.PlantHires != null ? job.PlantHires.ToList() : new List<PlantHire>();
            var inventoryHires = job.InventoryHires != null ? job.InventoryHires.ToList() : new List<InventoryHire>();

            if (plantHires.Any())
            {
                var plantHireRepository = new HireDbRepository<PlantHire>(Db);

                foreach (var hire in plantHires)
                {
                    await plantHireRepository.RemoveAsync(hire);
                }
            }

            if (inventoryHires.Any())
            {
                var inventoryHireRepository = new HireDbRepository<InventoryHire>(Db);

                foreach (var hire in inventoryHires)
                {
                    Db.InventoryHires.Remove(hire);
                }
            }

            return await base.RemoveAsync(job);
        }

        private void Validate(Job job)
        {
            // does the xjobid value already exist?
            var isDuplicate = this.GetAll().Any(f => f.Id != job.Id && f.XJobId.Equals(job.XJobId, StringComparison.OrdinalIgnoreCase));

            if (isDuplicate)
            {
                throw new InvalidOperationException($"The XJobId value '{job.XJobId}' already exists.");
            }
        }
    }
}
