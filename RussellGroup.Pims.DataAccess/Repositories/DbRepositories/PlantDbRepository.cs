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

        public override async Task<Plant> FindAsync(params object[] keyValues)
        {
            var id = (int)keyValues[0];
            var plants = Db.Plants.Include("Photograph.Content");
            var plant = plants.SingleOrDefault(f => f.Id == id);

            return await Task.FromResult(plant);
        }

        public override Task<int> UpdateAsync(Plant plant)
        {
            File photo = null;

            // get the photo if it already exists
            if (plant.PhotographId.HasValue)
            {
                photo = Db.Files.Find(plant.PhotographId.Value);
            }

            // is there a photo?
            if (plant.Photograph != null)
            {
                // has the photo changed?
                if (photo != null && plant.Photograph.Content.Hash == null)
                {
                    // then remove the existing photo
                    Remove(photo);
                }
            }
            // otherwise, remove the existing photo
            else
            {
                if (photo != null)
                {
                    Remove(photo);
                }
            }

            return base.UpdateAsync(plant);
        }

        public override async Task<int> RemoveAsync(Plant plant)
        {
            Remove(plant.Photograph);

            return await base.RemoveAsync(plant);
        }

        public IQueryable<PlantHire> GetPlantHire(int plantId)
        {
            return Db.PlantHires.Where(f => f.PlantId == plantId);
        }

        private void Remove(File file)
        {
            if (file != null)
            {
                Db.Contents.Remove(file.Content);
                Db.Files.Remove(file);
            }
        }
    }
}
