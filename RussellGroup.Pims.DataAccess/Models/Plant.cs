﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RussellGroup.Pims.DataAccess.Models
{
    public class Plant
    {
        [ScaffoldColumn(false)]
        public int Id { get; set; }

        [ForeignKey("Category")]
        public int CategoryId { get; set; }

        [ForeignKey("Status")]
        public int StatusId { get; set; }

        [ForeignKey("Condition")]
        public int ConditionId { get; set; }

        [Required]
        [Display(Name = "id")]
        public string XPlantId { get; set; }

        [Display(Name = "new id")]
        // StringLength isn't used here because there is existing data that doesn't meet this criteria
        [RegularExpression(@"(?i)\w{5}", ErrorMessage = "The new id field must be five characters.")]
        public string XPlantNewId { get; set; }

        [Required]
        [Display(Name = "description")]
        public string Description { get; set; }

        [Display(Name = "when purchased")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? WhenPurchased { get; set; }

        [Display(Name = "when disused")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? WhenDisused { get; set; }

        [Display(Name = "default rate")]
        public decimal Rate { get; set; }

        [Display(Name = "cost")]
        public decimal Cost { get; set; }

        [Display(Name = "serial")]
        public string Serial { get; set; }

        [Display(Name = "fixed asset code")]
        public string FixedAssetCode { get; set; }

        [Display(Name = "electrical?")]
        public bool IsElectrical { get; set; }

        [Display(Name = "tool?")]
        public bool IsTool { get; set; }

        [Display(Name = "category")]
        public virtual Category Category { get; set; }

        [Display(Name = "status")]
        public virtual Status Status { get; set; }

        [Display(Name = "condition")]
        public virtual Condition Condition { get; set; }

        [Display(Name = "comments")]
        [DataType(DataType.MultilineText)]
        public string Comment { get; set; }

        [NotMapped]
        public string BarcodeText
        {
            get
            {
                return (string.IsNullOrWhiteSpace(XPlantNewId) ? string.IsNullOrWhiteSpace(XPlantId) ? "UNKNOWN" : XPlantId : XPlantNewId).ToUpper();
            }
        }

        [NotMapped]
        public string Code39
        {
            get
            {
                return $"*{BarcodeText}*";
            }
        }

        // for the batch confirmation
        [NotMapped]
        [Display(Name = "error")]
        public bool IsError { get; set; }

        [Display(Name = "hire")]
        public virtual ICollection<PlantHire> PlantHires { get; set; }

        public bool CanDelete
        {
            get
            {
                return PlantHires != null && !PlantHires.Any();
            }
        }

        public bool IsCheckedIn
        {
            get
            {
                return PlantHires != null ? PlantHires.All(f => f.WhenEnded.HasValue) : true;
            }
        }

        public bool IsCheckedOut
        {
            get
            {
                return PlantHires != null ? PlantHires.Any(f => !f.WhenEnded.HasValue) : false;
            }
        }

        public string XPlantIdAndXPlantNewId
        {
            get
            {
                var ids = new[] { XPlantId, XPlantNewId };

                return string.Join("/", ids.Where(f => !string.IsNullOrWhiteSpace(f)).Distinct());
            }
        }
    }
}
