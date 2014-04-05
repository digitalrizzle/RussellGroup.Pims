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
        public const int Unavailable = 3;
        public const int Missing = 4;
        public const int Stolen = 5;
        public const int UnderRepair = 6;
        public const int WrittenOff = 7;

        [ScaffoldColumn(false)]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.None)]
        public int StatusId { get; set; }

        [Display(Name = "name")]
        public string Name { get; set; }

        [Display(Name = "plant items")]
        public virtual ICollection<Plant> Plants { get; set; }
    }
}
