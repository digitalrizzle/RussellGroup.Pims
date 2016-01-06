using RussellGroup.Pims.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace RussellGroup.Pims.Website.Models
{
    public class BatchCheckout
    {
        [Display(Name = "scans")]
        [DataType(DataType.MultilineText)]
        public string Scans { get; set; }

        [Display(Name = "when checked out")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime WhenStarted { get; set; }

        public IEnumerable<CheckoutTransaction> CheckoutTransactions { get; set; }
    }
}