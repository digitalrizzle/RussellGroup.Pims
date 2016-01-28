using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RussellGroup.Pims.DataAccess.Models
{
    public class TransactionType
    {
        public const int Checkout = 1;
        public const int Checkin = 2;

        [ScaffoldColumn(false)]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        [Display(Name = "name")]
        public string Name { get; set; }
    }
}
