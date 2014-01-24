using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RussellGroup.Pims.DataAccess.Models
{
    public class Category
    {
        public int CategoryId { get; set; }

        [Display(Name = "name")]
        public string Name { get; set; }

        [Display(Name = "type")]
        public string Type { get; set; }

        [Display(Name = "plant items")]
        public virtual ICollection<Plant> Plants { get; set; }

        [Required]
        public bool IsImported { get; set; }
    }
}
