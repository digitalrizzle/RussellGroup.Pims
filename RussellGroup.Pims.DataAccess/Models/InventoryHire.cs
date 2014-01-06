﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RussellGroup.Pims.DataAccess.Models
{
    public class InventoryHire
    {
        public int InventoryHireId { get; set; }

        [ForeignKey("Inventory")]
        public int InventoryId { get; set; }

        [ForeignKey("Job")]
        public int JobId { get; set; }

        [Display(Name = "docket")]
        public string Docket { get; set; }

        [Display(Name = "started")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? WhenStarted { get; set; }

        [Display(Name = "ended")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? WhenEnded { get; set; }

        [Display(Name = "rate")]
        public decimal? Rate { get; set; }

        [Display(Name = "quantity")]
        public int? Quantity { get; set; }

        [Display(Name = "comments")]
        [DataType(DataType.MultilineText)]
        public string Comment { get; set; }

        [Display(Name = "inventory")]
        public virtual Inventory Inventory { get; set; }

        [Display(Name = "job")]
        public virtual Job Job { get; set; }
    }
}