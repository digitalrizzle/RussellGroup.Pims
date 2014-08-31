using RussellGroup.Pims.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RussellGroup.Pims.DataAccess.Repositories
{
    public class PlantDbRepository : DbRepository<Plant>, IPlantRepository
    {
        public PlantDbRepository(PimsDbContext context) : base(context) { }

        public IQueryable<Category> Categories
        {
            get { return Db.Categories; }
        }

        public IQueryable<Status> Statuses
        {
            get { return Db.Statuses; }
        }

        public IQueryable<Condition> Conditions
        {
            get { return Db.Conditions; }
        }

        public IQueryable<PlantHire> GetPlantHire(int plantId)
        {
            return Db.PlantHires.Where(f => f.PlantId == plantId);
        }
    }
}
