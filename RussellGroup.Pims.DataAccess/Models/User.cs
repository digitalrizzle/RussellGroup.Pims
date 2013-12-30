using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RussellGroup.Pims.DataAccess.Models
{
    public class User
    {
        public int UserId { get; set; }

        [Display(Name = "name")]
        public string Name { get; set; }

        [Display(Name = "group?")]
        public bool isGroup { get; set; }

        [Display(Name = "enabled?")]
        public bool isEnabled { get; set; }

        [Display(Name = "roles")]
        public virtual ICollection<Role> Roles { get; set; }
    }
}
