using RussellGroup.Pims.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RussellGroup.Pims.DataAccess.Respositories
{
    public interface IReportRepository : IDisposable
    {
        Task<Job> GetJob(int? id);

        IQueryable<Job> Jobs { get; }

        IQueryable<PlantHire> GetActivePlantHiresInJob(int? jobId);
        IQueryable<InventoryHire> GetActiveInventoryHiresInJob(int? jobId);
    }
}
