using RussellGroup.Pims.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RussellGroup.Pims.DataAccess.ReportModels
{
    public class SummaryOfHireChargesReportViewModel
    {
        public SummaryOfHireChargesReportViewModel(IEnumerable<Job> jobs, DateTime whenStarted, DateTime whenEnded)
        {
            Jobs = jobs;
            WhenStarted = whenStarted;
            WhenEnded = whenEnded;
        }

        public IEnumerable<Job> Jobs { get; private set; }
        public DateTime WhenStarted { get; private set; }
        public DateTime WhenEnded { get; private set; }

        public IEnumerable<Job> ActiveJobs
        {
            get
            {
                return Jobs
                    .Where(f => HasActiveHire(f))
                    .OrderBy(f => f.XJobId)
                    .ToList();
            }
        }

        public bool HasActiveHire(Job job)
        {
            //var hasActivePlantHire = job
            //    .PlantHires
            //    .Where(f => (f.WhenStarted < WhenEnded.AddDays(1) && f.WhenEnded >= WhenStarted) ||
            //                (f.WhenStarted < WhenEnded.AddDays(1) && !f.WhenEnded.HasValue))
            //    .Any();

            //var hasActiveInventoryHire = job
            //    .InventoryHires
            //    .Where(f => (f.WhenStarted < WhenEnded.AddDays(1) && f.WhenEnded >= WhenStarted) ||
            //                (f.WhenStarted < WhenEnded.AddDays(1) && !f.WhenEnded.HasValue))
            //    .Any();

            //return hasActivePlantHire || hasActiveInventoryHire;
            return false;
        }

        public decimal GetPlantHireCharge(Job job)
        {
            decimal cost = 0;

            var hires = job
                .PlantHires
                .Where(f =>
                    (f.WhenStarted < WhenEnded.AddDays(1) && f.WhenEnded >= WhenStarted) ||
                    (f.WhenStarted < WhenEnded.AddDays(1) && !f.WhenEnded.HasValue))
                .ToList();

            foreach (var hire in hires)
            {
                var whenStarted = WhenStarted > hire.WhenStarted
                    ? WhenStarted
                    : hire.WhenStarted;

                var days = hire.WhenEnded.HasValue
                    ? hire.WhenEnded.Value.AddDays(1).Subtract(whenStarted).Days
                    : WhenEnded.AddDays(1).Subtract(whenStarted).Days;

                cost += days * hire.Rate.Value;
            }

            return cost;
        }

        public decimal GetInventoryHireCharge(Job job, string categoryType)
        {
            decimal cost = 0;

            //var hires = job
            //    .InventoryHires
            //    .Where(f =>
            //        (f.Inventory.Category.Type.Equals(categoryType, StringComparison.OrdinalIgnoreCase)) &&
            //        ((f.WhenStarted < WhenEnded.AddDays(1) && f.WhenEnded >= WhenStarted) ||
            //        (f.WhenStarted < WhenEnded.AddDays(1) && !f.WhenEnded.HasValue)))
            //    .ToList();

            //foreach (var hire in hires)
            //{
            //    var whenStarted = WhenStarted > hire.WhenStarted
            //        ? WhenStarted
            //        : hire.WhenStarted;

            //    var days = hire.WhenEnded.HasValue
            //        ? hire.WhenEnded.Value.AddDays(1).Subtract(whenStarted).Days
            //        : WhenEnded.AddDays(1).Subtract(whenStarted).Days;

            //    cost += days * hire.Rate.Value * hire.Quantity.Value;
            //}

            return cost;
        }
    }
}