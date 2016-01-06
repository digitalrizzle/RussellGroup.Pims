using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RussellGroup.Pims.DataAccess.Models
{
    public class Job
    {
        [ScaffoldColumn(false)]
        public int Id { get; set; }

        [Obsolete]
        [Display(Name = "id")]
        public string XJobId { get; set; }

        [Required]
        [Display(Name = "description")]
        public string Description { get; set; }

        [Display(Name = "when started")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? WhenStarted { get; set; }

        [Display(Name = "when ended")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? WhenEnded { get; set; }

        [Display(Name = "project manager")]
        public string ProjectManager { get; set; }

        [Display(Name = "quantity surveyor")]
        public string QuantitySurveyor { get; set; }

        [Display(Name = "comments")]
        [DataType(DataType.MultilineText)]
        public string Comment { get; set; }

        [NotMapped]
        public string BarcodeText
        {
            get
            {
                return (string.IsNullOrWhiteSpace(XJobId) ? "UNKNOWN" : XJobId).ToUpper();
            }
        }

        [NotMapped]
        public string Code39
        {
            get
            {
                return $"*{BarcodeText}*";
            }
        }

        // for the batch confirmation
        [NotMapped]
        [Display(Name = "error")]
        public bool IsError { get; set; }

        [Display(Name = "plant hire")]
        public virtual ICollection<PlantHire> PlantHires { get; set; }

        [Display(Name = "inventory hire")]
        public virtual ICollection<InventoryHire> InventoryHires { get; set; }

        public string JobAndDescription
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(XJobId))
                {
                    return string.Format("[{0}] {1}", XJobId, Description);
                }

                return Description;
            }
        }

        public IEnumerable<InventoryHire> CollatedInventoryHires
        {
            get
            {
                var collated =
                    InventoryHires
                    .GroupBy(f => f.Inventory)
                    .Select(f => new InventoryHire
                    {
                        Job = this,
                        Inventory = f.Key,
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
