using RussellGroup.Pims.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace RussellGroup.Pims.Website.Models
{
    public class BatchStatus
    {
        [Display(Name = "scans")]
        [DataType(DataType.MultilineText)]
        public string Scans { get; set; }

        [ForeignKey("Status")]
        public int StatusId { get; set; }

        [Display(Name = "status")]
        public virtual Status Status { get; set; }

        public IEnumerable<Plant> Plants { get; set; }
    }
}