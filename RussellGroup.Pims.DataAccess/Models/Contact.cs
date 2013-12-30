using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RussellGroup.Pims.DataAccess.Models
{
    public class Contact
    {
        public int ContactId { get; set; }

        [Display(Name = "name")]
        public string Name { get; set; }

        [Display(Name = "jobs")]
        public virtual ICollection<Job> Jobs { get; set; }
    }
}
