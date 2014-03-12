using RussellGroup.Pims.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RussellGroup.Pims.DataAccess.ReportModels
{
    public class PlantLocationsReportModel
    {
        public Category Category { get; set; }
        public IEnumerable<PlantHire> PlantHireInJobs { get; set; }
    }
}
