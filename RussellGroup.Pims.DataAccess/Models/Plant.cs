using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RussellGroup.Pims.DataAccess.Models
{
    public class Plant
    {
        public int PlantId { get; set; }

        [ForeignKey("Category")]
        public int CategoryId { get; set; }

        [Obsolete]
        public string XPlantId { get; set; }

        [Obsolete]
        public string XPlantNewId { get; set; }

        [Display(Name = "description")]
        public string Description { get; set; }

        [Display(Name = "rate")]
        public decimal Rate { get; set; }

        [Display(Name = "cost")]
        public decimal Cost { get; set; }

        [Display(Name = "serial")]
        public string Serial { get; set; }

        [Display(Name = "fixed asset code")]
        public string FixedAssetCode { get; set; }

        [Display(Name = "electrical?")]
        public bool IsElectrical { get; set; }

        [Display(Name = "tool?")]
        public bool IsTool { get; set; }

        [Display(Name = "category")]
        public virtual Category Category { get; set; }

        [Display(Name = "hirage")]
        public virtual ICollection<PlantHire> PlantHires { get; set; }
    }
}
