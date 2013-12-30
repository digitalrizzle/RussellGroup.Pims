using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RussellGroup.Pims.DataAccess.Models
{
    public class Job
    {
        public int JobId { get; set; }

        [Obsolete]
        public string XJobId { get; set; }

        [Display(Name = "description")]
        public string Description { get; set; }

        [Display(Name = "started")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? WhenStarted { get; set; }

        [Display(Name = "ended")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? WhenEnded { get; set; }

        [ForeignKey("ProjectManager")]
        public int? ProjectManagerContactId { get; set; }

        [ForeignKey("QuantitySurveyor")]
        public int? QuantitySurveyorContactId { get; set; }

        [Display(Name = "comments")]
        public string Comment { get; set; }

        [Display(Name = "project manager")]
        public virtual Contact ProjectManager { get; set; }

        [Display(Name = "quantity surveyor")]
        public virtual Contact QuantitySurveyor { get; set; }

        [Display(Name = "status")]
        public Status Status
        {
            get
            {
                return WhenEnded.HasValue ? Status.Complete : Models.Status.Incomplete;
            }
        }
    }
}
