using RussellGroup.Pims.DataAccess.Models;
using RussellGroup.Pims.DataAccess.ReportModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RussellGroup.Pims.DataAccess.Repositories
{
    public interface IReportRepository : IDisposable
    {
        IQueryable<Job> Jobs { get; }
        IQueryable<Category> Categories { get; }

        PlantLocationsReportModel GetPlantLocationsByCategory(int? categoryId);
        InventoryLocationsReportModel GetInventoryLocationsByCategory(int? categoryId);
    }
}
