﻿using RussellGroup.Pims.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RussellGroup.Pims.Website.Models
{
    public class CheckoutTransaction
    {
        [Display(Name = "job")]
        public Job Job { get; set; }

        [Required]
        [Display(Name = "docket")]
        public string Docket { get; set; }

        [Display(Name = "when checked out")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime WhenStarted { get; set; }

        [Display(Name = "plant")]
        public virtual ICollection<Plant> Plants { get; set; }

        [Display(Name = "inventory")]
        public virtual List<KeyValuePair<Inventory, int?>> Inventories { get; set; }

        public int? JobId
        {
            get
            {
                return Job != null ? Job.Id : (int?)null;
            }
        }
    }
}