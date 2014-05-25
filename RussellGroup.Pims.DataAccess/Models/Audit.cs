using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RussellGroup.Pims.DataAccess.Models
{
    public class Audit
    {
        [ScaffoldColumn(false)]
        public int Id { get; set; }

        [Display(Name = "when changed")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime WhenChanged { get; set; }

        [Display(Name = "table")]
        public string Table { get; set; }

        [StringLength(128)]
        [Display(Name = "first primary key")]
        public string PrimaryKeyId1 { get; set; }

        [Display(Name = "second primary key")]
        [StringLength(128)]
        public string PrimaryKeyId2 { get; set; }

        [StringLength(1)]
        public string Action { get; set; }

        [Display(Name = "old data")]
        [Column(TypeName = "xml")]
        public string OldData { get; set; }

        [Display(Name = "new data")]
        [Column(TypeName = "xml")]
        public string NewData { get; set; }

        [Display(Name = "user name")]
        public string UserName { get; set; }

        [Display(Name = "action")]
        public string ActionName
        {
            get
            {
                switch (this.Action)
                {
                    case "I": return "insert";
                    case "U": return "update";
                    case "D": return "delete";
                    default: return "unknown";
                }
            }
        }
    }
}
