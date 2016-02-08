using RussellGroup.Pims.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace RussellGroup.Pims.Website.Models
{
    public class BatchCheckin
    {
        [Display(Name = "scans")]
        [DataType(DataType.MultilineText)]
        public string Scans { get; set; }

        [Display(Name = "when checked in")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime WhenEnded { get; set; }

        [ForeignKey("Status")]
        public int StatusId { get; set; }

        [Display(Name = "status")]
        public virtual Status Status { get; set; }

        public IEnumerable<CheckinTransaction> CheckinTransactions { get; set; }
    }
}