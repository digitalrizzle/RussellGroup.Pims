﻿using RussellGroup.Pims.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RussellGroup.Pims.DataAccess.Repositories
{
    public interface IPlantRepository : IRepository<Plant>
    {
        IQueryable<Category> Categories { get; }
        IQueryable<Status> Statuses { get; }
        IQueryable<Condition> Conditions { get; }

        IQueryable<PlantHire> GetPlantHire(int plantId);
    }
}
