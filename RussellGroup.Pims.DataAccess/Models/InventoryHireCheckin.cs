using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RussellGroup.Pims.DataAccess.Models
{
    public class InventoryHireCheckin
    {
        [ScaffoldColumn(false)]
        public int Id { get; set; }

        [ForeignKey("InventoryHire")]
        public int InventoryHireId { get; set; }

        [Display(Name = "return docket")]
        public string Docket { get; set; }

        [Required]
        [Display(Name = "ended")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime WhenEnded { get; set; }

        [Display(Name = "quantity")]
        public int Quantity { get; set; }

        [Display(Name = "comments")]
        [DataType(DataType.MultilineText)]
        public string Comment { get; set; }

        [Display(Name = "inventory hire")]
        public virtual InventoryHire InventoryHire { get; set; }
    }
}
