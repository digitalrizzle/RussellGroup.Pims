using RussellGroup.Pims.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RussellGroup.Pims.Website.Models
{
    public class UserRolesViewModel
    {
        [Display(Name = "user")]
        public ApplicationUser User { get; set; }

        [Display(Name = "roles")]
        public List<ApplicationRole> Roles { get; set; }

        public string RoleNames
        {
            get { return string.Join(", ", this.Roles.Where(f => f.IsChecked).OrderBy(f => f.Name).Select(f => f.Name)); }
        }
    }
}