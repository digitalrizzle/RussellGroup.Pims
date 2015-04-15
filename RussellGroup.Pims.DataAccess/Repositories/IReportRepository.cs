using RussellGroup.Pims.DataAccess.Models;
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
        IQueryable<Plant> Plants { get; }
        IQueryable<Inventory> Inventories { get; }
        IQueryable<Category> Categories { get; }

        PlantLocationsReportModel GetPlantLocationsByCategory(int? categoryId);
        InventoryLocationsReportModel GetInventoryLocationsByCategory(int? categoryId);

        IQueryable<Plant> GetAvailablePlant();
        InventoryHireChargesInJobReportModel GetInventoryHireCharges(Job job, DateTime whenStarted, DateTime whenEnded);

        Task<SummaryOfHireChargesReportModel> GetSummaryOfHireChargesAsync(DateTime whenStarted, DateTime whenEnded);

        byte[] GetSummaryOfHireChargesCsv(SummaryOfHireChargesReportModel model);
    }
}
