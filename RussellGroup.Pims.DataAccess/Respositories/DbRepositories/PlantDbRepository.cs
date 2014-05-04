using RussellGroup.Pims.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RussellGroup.Pims.DataAccess.Respositories
{
    public class PlantDbRepository : DbRepository<Plant>, IPlantRepository
    {
        public IQueryable<Category> Categories
        {
            get { return Db.Categories; }
        }

        public IQueryable<Status> Statuses
        {
            get { return Db.Statuses; }
        }

        public IQueryable<Job> GetJobs(int plantId)
        {
            var jobs = (from j in Db.Jobs
                        join p in Db.PlantHires on j.JobId equals p.JobId
                        where !j.WhenEnded.HasValue && p.PlantId == plantId
                        select j).Distinct();

            return jobs;
        }
    }
}
