using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RussellGroup.Pims.DataAccess.Models
{
    public class User
    {
        public int UserId { get; set; }

        [NotMapped]
        public int RoleId { get; set; }

        [Display(Name = "name")]
        public string Name { get; set; }

        [Display(Name = "group?")]
        public bool IsGroup { get; set; }

        [Display(Name = "enabled?")]
        public bool IsEnabled { get; set; }

        [Display(Name = "roles")]
        public virtual ICollection<Role> Roles { get; set; }

        [Display(Name="role")]
        public Role HighestRole
        {
            get
            {
                return Roles != null ? Roles.SingleOrDefault(r => r.RoleId == Roles.Max(f => f.RoleId)) : null;
            }
        }
    }
}
