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
        public const int Unknown = 1;
        public const int Available = 2;
        public const int CheckedOut = 3;
        public const int Stolen = 4;
        public const int UnderRepair = 5;
        public const int WrittenOff = 6;

        [ScaffoldColumn(false)]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        [Display(Name = "name")]
        public string Name { get; set; }

        [Display(Name = "plant items")]
        public virtual ICollection<Plant> Plants { get; set; }
    }
}
