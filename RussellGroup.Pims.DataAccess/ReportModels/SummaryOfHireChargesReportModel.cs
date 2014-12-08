using RussellGroup.Pims.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace RussellGroup.Pims.DataAccess.ReportModels
{
    public class SummaryOfHireChargesReportModel
    {
        public List<Job> Jobs { get; set; }
        public DateTime WhenStarted { get; set; }
        public DateTime WhenEnded { get; set; }
        public Dictionary<Job, decimal> PlantHireCharges { get; set; }
        public List<InventoryHireChargesInJobReportModel> InventoryHireCharges { get; set; }

        public decimal GetPlantHireCharge(Job job)
        {
            return PlantHireCharges.SingleOrDefault(f => f.Key == job).Value;
        }

        public decimal GetInventoryHireCharge(Job job, string categoryType)
        {
            var charges = InventoryHireCharges
                .SingleOrDefault(f => f.Job == job)
                .Charges
                .Where(f => f.Inventory.Category.Type == categoryType);

            return charges.Sum(f => f.Cost);
        }
    }
}