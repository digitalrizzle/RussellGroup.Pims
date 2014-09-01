﻿using RussellGroup.Pims.DataAccess.Models;
using RussellGroup.Pims.DataAccess.ReportModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RussellGroup.Pims.DataAccess.Repositories
{
    public interface IReportRepository
    {
        IQueryable<Job> Jobs { get; }
        IQueryable<Category> Categories { get; }

        PlantLocationsReportModel GetPlantLocationsByCategory(int? categoryId);
        InventoryLocationsReportModel GetInventoryLocationsByCategory(int? categoryId);

        IEnumerable<Plant> GetPlantCheckedIn();
        IEnumerable<Inventory> GetInventoryCheckedIn();

        [Obsolete]
        byte[] GetInventoryChargesCsv(int? jobId, DateTime whenStarted, DateTime whenEnded);
    }
}
