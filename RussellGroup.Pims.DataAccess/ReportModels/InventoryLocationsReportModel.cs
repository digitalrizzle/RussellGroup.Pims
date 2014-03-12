using RussellGroup.Pims.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RussellGroup.Pims.DataAccess.ReportModels
{
    public class InventoryLocationsReportModel
    {
        public Category Category { get; set; }
        public IEnumerable<InventoryHireInJobReportModel> InventoryHireInJobs { get; set; }
    }

    public class InventoryHireInJobReportModel
    {
        public Job Job { get; set; }
        public IEnumerable<InventoryHire> InventoryHires { get; set; }
    }
}
