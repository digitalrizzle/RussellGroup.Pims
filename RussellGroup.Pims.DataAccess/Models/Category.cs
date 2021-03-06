﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RussellGroup.Pims.DataAccess.Models
{
    public class Category
    {
        [ScaffoldColumn(false)]
        public int Id { get; set; }

        [Required]
        [Display(Name = "name")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "type")]
        public string Type { get; set; }

        [Display(Name = "plant items")]
        public virtual ICollection<Plant> Plants { get; set; }

        [Display(Name = "inventory items")]
        public virtual ICollection<Inventory> Inventories { get; set; }

        public bool CanDelete
        {
            get
            {
                return (Plants != null && Plants.Count == 0) & (Inventories != null && Inventories.Count == 0);
            }
        }
    }
}
