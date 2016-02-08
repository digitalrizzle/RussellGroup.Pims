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

        [ForeignKey("Job")]
        public int JobId { get; set; }

        [ForeignKey("Content")]
        public int ContentId { get; set; }

        [Display(Name = "user name")]
        public string UserName { get; set; }

        [Display(Name = "when created")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime WhenCreated { get; set; }

        [Display(Name = "docket")]
        [DataType(DataType.Text)]
        public string Docket { get; set; }

        [Display(Name = "project manager")]
        public string ProjectManager { get; set; }

        [Display(Name = "email recipients")]
        [RegularExpression(@"^[\W]*([\w+\-.%]+@[\w\-.]+\.[A-Za-z]{2,4}[\W]*,{1}[\W]*)*([\w+\-.%]+@[\w\-.]+\.[A-Za-z]{2,4})[\W]*$", ErrorMessage = "The email address(es) are not valid.")]
        public string Recipients { get; set; }

        [Display(Name = "job")]
        public virtual Job Job { get; set; }

        [Display(Name = "content")]
        public Content Content { get; set; }

        [Display(Name = "transaction type")]
        public virtual TransactionType TransactionType { get; set; }
    }
}
