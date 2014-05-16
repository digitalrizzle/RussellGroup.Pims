using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RussellGroup.Pims.DataAccess.Models
{
    [MetadataType(typeof(ApplicationUserMetadata))]
    public class ApplicationUser : IdentityUser
    {
        internal sealed class ApplicationUserMetadata
        {
            // metadata classes are not meant to be instantiated
            private ApplicationUserMetadata() { }

            [DisplayName("user name")]
            [Required(AllowEmptyStrings = false, ErrorMessage = "A user name is required.")]
            public string UserName { get; set; }

            [DisplayName("email")]
            [EmailAddress(ErrorMessage = "The email address is not valid.")]
            public string Email { get; set; }

            [DisplayName("locked out?")]
            public string LockoutEnabled { get; set; }

            [DisplayName("when locked out")]
            public DateTime LockoutEndDateUtc { get; set; }
        }
    }
}
