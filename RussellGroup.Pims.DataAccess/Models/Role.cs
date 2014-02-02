using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RussellGroup.Pims.DataAccess.Models
{
    public class Role
    {
        [ScaffoldColumn(false)]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.None)]
        public int RoleId { get; set; }

        [Display(Name = "name")]
        public string Name { get; set; }

        [Display(Name = "users")]
        public virtual ICollection<User> Users { get; set; }
    }
}
