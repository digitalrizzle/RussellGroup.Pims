using Microsoft.AspNet.Identity.EntityFramework;
using RussellGroup.Pims.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RussellGroup.Pims.DataAccess.ViewModels
{
    public class UserRoles
    {
        [Display(Name = "user")]
        public ApplicationUser User { get; set; }

        [Display(Name = "roles")]
        public ICollection<ApplicationRole> Roles { get; set; }

        public string GetRoleNames()
        {
            return string.Join(", ", this.Roles.Select(f => f.Name));
        }
    }
}
