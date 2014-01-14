using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RussellGroup.Pims.DataAccess.Models
{
    public class Setting
    {
        [Key]
        [Required(AllowEmptyStrings=false)]
        public string Key { get; set; }

        public string Value { get; set; }
    }
}
