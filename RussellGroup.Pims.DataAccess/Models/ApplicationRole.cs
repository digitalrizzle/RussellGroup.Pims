using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RussellGroup.Pims.DataAccess.Models
{
    public class Role
    {
        public const string CanView = "CanView";
        public const string CanEdit = "CanEdit";
        public const string CanEditCategories = "CanEditCategories";
        public const string CanEditUsers = "CanEditUsers";
    }

    public class ApplicationRole : IdentityRole
    {
        [NotMapped]
        public bool IsChecked { get; set; }

        [Required]
        public int Precedence { get; set; }

        [DisplayName("description")]
        public string Description { get; set; }
    }
}
