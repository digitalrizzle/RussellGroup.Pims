using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RussellGroup.Pims.DataAccess.Models
{
    public class Status
    {
        [ScaffoldColumn(false)]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.None)]
        public int StatusId { get; set; }

        [Display(Name = "name")]
        public string Name { get; set; }

        [Display(Name = "plant items")]
        public virtual ICollection<Plant> Plants { get; set; }
    }
}
