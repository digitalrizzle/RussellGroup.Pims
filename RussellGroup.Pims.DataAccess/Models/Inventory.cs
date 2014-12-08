using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RussellGroup.Pims.DataAccess.Models
{
    public class Inventory
    {
        [ScaffoldColumn(false)]
        public int Id { get; set; }

        [ForeignKey("Category")]
        public int CategoryId { get; set; }

        [Obsolete]
        [Display(Name = "id")]
        public string XInventoryId { get; set; }

        [Display(Name = "description")]
        public string Description { get; set; }

        [Display(Name = "when purchased")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? WhenPurchased { get; set; }

        [Display(Name = "when disused")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? WhenDisused { get; set; }

        [Display(Name = "rate")]
        public decimal? Rate { get; set; }

        [Display(Name = "cost")]
        public decimal? Cost { get; set; }

        [Display(Name = "yard quantity")]
        public int Quantity { get; set; }

        [Display(Name = "category")]
        public virtual Category Category { get; set; }

        [Display(Name = "hire")]
        public virtual ICollection<InventoryHire> InventoryHires { get; set; }

        [Display(Name = "disused?")]
        public bool IsDisused
        {
            get
            {
                return WhenDisused.HasValue;
            }
        }

        public bool CanDelete
        {
            get
            {
                return InventoryHires != null && !InventoryHires.Any();
            }
        }

        public IEnumerable<InventoryHire> CollatedInventoryHires
        {
            get
            {
                var collated =
                    InventoryHires
                    .GroupBy(f => f.Job)
                    .Select(f => new InventoryHire
                    {
                        Job = f.Key,
                        Inventory = this,
                        Quantity = f.Sum(hire => hire is InventoryHireCheckout
                            ? hire.Quantity
                            : -hire.Quantity
                        )
                    });

                return collated;
            }
        }
    }
}
