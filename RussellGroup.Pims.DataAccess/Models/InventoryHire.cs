﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RussellGroup.Pims.DataAccess.Models
{
    public class InventoryHire
    {
        [ScaffoldColumn(false)]
        public int Id { get; set; }

        [ForeignKey("Inventory")]
        [DisplayName("inventory id")]
        public int InventoryId { get; set; }

        [ForeignKey("Job")]
        public int JobId { get; set; }

        [Required]
        [Display(Name = "docket")]
        public string Docket { get; set; }

        [Display(Name = "quantity")]
        public int? Quantity { get; set; }

        [Display(Name = "comments")]
        [DataType(DataType.MultilineText)]
        public string Comment { get; set; }

        [Display(Name = "inventory")]
        public virtual Inventory Inventory { get; set; }

        [Display(Name = "job")]
        public virtual Job Job { get; set; }

        [NotMapped]
        public bool IsSelected { get; set; }
    }

    public class InventoryHireCheckout : InventoryHire
    {
        [Required]
        [Display(Name = "when started")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime WhenStarted { get; set; }
    }

    public class InventoryHireCheckin : InventoryHire
    {
        [Display(Name = "when ended")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime WhenEnded { get; set; }

        [NotMapped]
        [Display(Name = "checked out")]
        public int CheckedOutQuantity { get; set; }
    }
}
