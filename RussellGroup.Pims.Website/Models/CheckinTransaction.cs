using RussellGroup.Pims.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RussellGroup.Pims.Website.Models
{
    public class CheckinTransaction
    {
        [Display(Name = "job")]
        public Job Job { get; set; }

        [Required]
        [Display(Name = "is auto docket?")]
        public bool IsAutoDocket { get; set; }

        [Required]
        [Display(Name = "docket")]
        public string ReturnDocket { get; set; }

        [Display(Name = "when checked in")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime WhenEnded { get; set; }

        [Display(Name = "plant hire")]
        public virtual ICollection<PlantHire> PlantHires { get; set; }

        [Display(Name = "inventory hire")]
        public virtual ICollection<InventoryHire> InventoryHires { get; set; }

        public int? JobId
        {
            get
            {
                return Job != null ? Job.Id : (int?)null;
            }
        }
    }
}