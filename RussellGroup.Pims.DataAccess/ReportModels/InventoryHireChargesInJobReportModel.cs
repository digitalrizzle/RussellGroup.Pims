using RussellGroup.Pims.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RussellGroup.Pims.DataAccess.ReportModels
{
    public class InventoryHireChargesInJobReportModel
    {
        public Job Job { get; set; }
        public DateTime WhenStarted { get; set; }
        public DateTime WhenEnded { get; set; }
        public List<InventoryHireChargeReportModel> Charges { get; set; }
    }

    public class InventoryHireChargeReportModel
    {
        public Inventory Inventory { get; set; }
        public int Days { get; set; }
        public int OpeningBalance { get; set; }
        public List<InventoryHireItemChargeReportModel> ItemCharges { get; set; }

        public int ClosingBalance
        {
            get
            {
                return OpeningBalance + ItemCharges.Sum(f => f.Quantity);
            }
        }

        public decimal Rate
        {
            get
            {
                return Inventory.Rate.GetValueOrDefault();
            }
        }

        public decimal OpeningBalanceCost
        {
            get
            {
                return Days * OpeningBalance * Rate;
            }
        }

        public decimal Cost
        {
            get
            {
                return 
                      OpeningBalanceCost
                    + ItemCharges.Sum(f => f.Cost);
            }
        }
    }

    public class InventoryHireItemChargeReportModel
    {
        public Inventory Inventory { get; set; }
        public string Docket { get; set; }
        public DateTime? WhenStarted { get; set; }
        public DateTime? WhenEnded { get; set; }
        public int Days { get; set; }
        public int Quantity { get; set; }

        public decimal Rate
        {
            get
            {
                return Inventory.Rate.GetValueOrDefault();
            }
        }

        public decimal Cost
        {
            get
            {
                return Days * Quantity * Rate;
            }
        }
    }
}
