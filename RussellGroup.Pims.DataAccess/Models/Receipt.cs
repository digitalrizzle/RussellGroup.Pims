using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RussellGroup.Pims.DataAccess.Models
{
    public class Receipt
    {
        [ScaffoldColumn(false)]
        public int Id { get; set; }

        [Display(Name = "transaction type")]
        [ForeignKey("TransactionType")]
        public int TransactionTypeId { get; set; }

        [ForeignKey("Content")]
        public int ContentId { get; set; }

        [Display(Name = "user name")]
        public string UserName { get; set; }

        [Display(Name = "when created")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime WhenCreated { get; set; }

        [Display(Name = "scans")]
        [DataType(DataType.MultilineText)]
        public string Scans { get; set; }

        [Display(Name = "dockets")]
        [DataType(DataType.Text)]
        public string Dockets { get; set; }

        [Display(Name = "content")]
        public Content Content { get; set; }

        public string ContentType { get; set; }

        [Display(Name = "transaction type")]
        public virtual TransactionType TransactionType { get; set; }
    }
}
